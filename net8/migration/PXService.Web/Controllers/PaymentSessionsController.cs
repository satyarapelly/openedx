// <copyright file="PaymentSessionsController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge
{
    using Common;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Transaction;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLGeneration;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Model.SessionService;
    using Microsoft.Commerce.Payments.PXService.Model.TransactionService;
    using Microsoft.CTP.CommerceAPI.Proxy.v201112;
    using Model;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Http;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text.Encodings.Web;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using PaymentContext = PXService.Model.SessionService.PaymentContext;
    using PaymentInstrumentPollingStatus = Microsoft.Commerce.Payments.PimsModel.V4.PaymentInstrumentStatus;
    using PimsSessionDetailsResource = Microsoft.Commerce.Payments.PimsModel.V4.PimsSessionDetailsResource;
    using PollingPaymentInstrument = Microsoft.Commerce.Payments.PimsModel.V4.PaymentInstrument;
    using PSD2ChallengeIndicator = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.ChallengeCancelIndicator;
    using PSD2TransStatus = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.TransactionStatus;
    using PSD2TransStatusReason = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.TransactionStatusReason;
    using PXConstants = Microsoft.Commerce.Payments.PXService.V7.Constants;
    using System.Diagnostics.Tracing;

    public class PaymentSessionsController : ProxyController
    {
        private const string PostMessageHtmlTemplate = "<html><script>window.parent.postMessage(\"{0}\", \"*\");</script><body/></html>";

        /// <summary>
        /// Used by App flow
        /// Create Payment Session 
        /// </summary>
        /// <param name="accountId">Pi Account Id</param>
        /// <returns>Returns the created PaymentSession</returns>
        [HttpPost]
        [ActionName("PostPaymentSession")]
        public async Task<PaymentSession> Post(string accountId)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            string paymentSessionData = await this.Request.GetRequestPayload();
            PaymentSessionData data = null;
            try
            {
                data = JsonConvert.DeserializeObject<PaymentSessionData>(paymentSessionData);
                this.Request.AddPartnerProperty(data?.Partner?.ToLower());
            }
            catch (Exception e)
            {
                throw new ValidationException(ErrorCode.InvalidRequestData, "missing required field " + e.Message);
            }

            string isMotoAuthorized = this.Request.GetRequestHeader(PXService.GlobalConstants.HeaderValues.IsMotoHeader);
            string tid = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.AadInfo.Tid);
            string userId = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.Puid);

            var deviceChannel = PXService.Model.ThreeDSExternalService.DeviceChannel.AppBased;

            // Previously this endpoint was only used by xbet so we defaulted to AppBased, now let's start with a partner based flight to enable browser based device channel
            if (this.ExposedFlightFeatures != null && this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableBrowserBasedDeviceChannel, StringComparer.OrdinalIgnoreCase))
            {
                deviceChannel = PXService.Model.ThreeDSExternalService.DeviceChannel.Browser;
            }

            PaymentSessionsHandler paymentSessionsHandler = await this.GetVersionBasedPaymentSessionsHandler(traceActivityId);
            return await paymentSessionsHandler.CreatePaymentSession(
                accountId: accountId,
                paymentSessionData: data,
                deviceChannel: deviceChannel,
                exposedFlightFeatures: this.ExposedFlightFeatures,
                emailAddress: await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.EmailAddress),
                traceActivityId: traceActivityId,
                testContext: HttpRequestHelper.GetTestHeader(),
                isMotoAuthorized: isMotoAuthorized,
                tid: tid,
                userId: userId);
        }

        /// <summary>
        /// Get Payment Session
        /// </summary>
        /// <param name="accountId">Pi Account Id</param>
        /// <param name="sessionId">Session Id</param>
        /// <returns>Returns queried PaymentSession that matches the session and account id</returns>
        [HttpGet]
        public async Task<HttpResponseMessage> GetPaymentSession(string accountId, string sessionId)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            try
            {
                PaymentSessionsHandler paymentSessionsHandler = await this.GetVersionBasedPaymentSessionsHandler(traceActivityId, sessionId);
                PaymentSession threeDsSession = await paymentSessionsHandler.TryGetPaymentSession(sessionId, traceActivityId);

                if (threeDsSession == null)
                {
                    throw new ValidationException(ErrorCode.InvalidRequestData, "PaymentSession with the given sessionId for the accountId is missing.");
                }

                if (!string.Equals(accountId, threeDsSession.PaymentInstrumentAccountId, StringComparison.OrdinalIgnoreCase))
                {
                    bool result = false;
                    if (!string.IsNullOrWhiteSpace(threeDsSession.PaymentInstrumentId))
                    {
                        // PIFD is rollout a change to replace paymentInstrumentAccountId from PUID caid and AAD caid
                        // To avoid the mismatch during the rollout (part of regions use AAD caid and the others use PUID caid), add this logic
                        // to do ownership check by using account Id passed by PIFD and piid. PIMS can return true for both AAD caid and PUID caid.
                        // The code can be kept after rollout to deal with multiple owners scenario in future
                        result = await PaymentSessionsHandler.CheckOwnership(accountId, threeDsSession.PaymentInstrumentId, traceActivityId, exposedFlightFeatures: this.ExposedFlightFeatures);
                    }

                    if (!result)
                    {
                        throw new ValidationException(ErrorCode.InvalidRequestData, "PaymentSession with the given sessionId for the accountId is missing.");
                    }
                }

                string paymentSessionId = threeDsSession?.Id;

                if (!string.IsNullOrEmpty(paymentSessionId))
                {
                    TestContext testContext = HttpRequestHelper.GetTestHeader(this.Request.ToHttpRequestMessage());

                    if (HttpRequestHelper.HasThreeDSOneTestScenarioWithFailure(testContext))
                    {
                        threeDsSession.ChallengeStatus = PaymentChallengeStatus.Failed;
                    }

                    if (HttpRequestHelper.HasThreeDSOneTestScenarioWithSuccess(testContext))
                    {
                        threeDsSession.ChallengeStatus = PaymentChallengeStatus.Succeeded;
                    }

                    if (HttpRequestHelper.HasThreeDSOneTestScenarioWithInternalServerError(testContext))
                    {
                        threeDsSession.ChallengeStatus = PaymentChallengeStatus.InternalServerError;
                    }

                    if (HttpRequestHelper.HasThreeDSOneTestScenarioWithTimeOut(testContext))
                    {
                        threeDsSession.ChallengeStatus = PaymentChallengeStatus.TimedOut;
                    }

                    // Call to Partner Settings to get the setting for the 3dsSession partner
                    this.PartnerSettings = await PXService.PartnerSettingsHelper.GetPaymentExperienceSetting(this.Settings, threeDsSession.Partner, null, traceActivityId, this.ExposedFlightFeatures);
                    PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(V7.Constants.Component.HandlePaymentChallenge);

                    // 3DS1 QR code partners which do not have one of these statuses should return session for polling
                    if (threeDsSession.ChallengeStatus == PaymentChallengeStatus.Failed
                        || threeDsSession.ChallengeStatus == PaymentChallengeStatus.Succeeded
                        || threeDsSession.ChallengeStatus == PaymentChallengeStatus.ByPassed
                        || threeDsSession.ChallengeStatus == PaymentChallengeStatus.NotApplicable
                        || threeDsSession.ChallengeStatus == PaymentChallengeStatus.InternalServerError
                        || threeDsSession.ChallengeStatus == PaymentChallengeStatus.TimedOut
                        || PartnerHelper.IsThreeDSOneQrCodeBasedPurchasePartner(threeDsSession.Partner, setting)
                        || PartnerHelper.IsThreeDSOneIframeBasedPartner(threeDsSession.Partner, setting))
                    {
                        PIDLResource threeDsStatusPidl = new PIDLResource();

                        ClientAction clientAction = new ClientAction(IsBadPaymentChallengeStatus(threeDsSession.ChallengeStatus) ? ClientActionType.Failure : ClientActionType.ReturnContext)
                        {
                            Context = threeDsSession,
                        };

                        if (clientAction.ActionType == ClientActionType.Failure && !PartnerHelper.IsThreeDSOneQrCodeBasedPurchasePartner(threeDsSession.Partner, setting))
                        {
                            clientAction.Context = CreateFailureClientActionContext(threeDsSession.ChallengeStatus == PaymentChallengeStatus.InternalServerError ? HttpStatusCode.InternalServerError : HttpStatusCode.BadRequest, threeDsSession.ChallengeStatus.ToString(), "Invalid ThreeDS session status", threeDsSession.UserDisplayMessage);
                        }

                        threeDsStatusPidl.ClientAction = clientAction;

                        return this.Request.CreateResponse(HttpStatusCode.OK, threeDsStatusPidl);
                    }
                    else
                    {
                        string pxBrowserAuthenticateRedirectionUrl = string.Format(V7.Constants.UriTemplate.PxBrowserAuthenticateRedirectionUrlTemplate, this.PidlBaseUrl, threeDsSession.Id);
                        PIDLResource tryAgainPidl;

                        if (setting != null && setting.RedirectionPattern != null)
                        {
                            PIDLGeneratorContext context = new PIDLGeneratorContext(
                                threeDsSession.Country,
                                threeDsSession.Partner,
                                TemplateHelper.GetSettingTemplate(threeDsSession.Partner, setting, V7.Constants.DescriptionTypes.ChallengeDescription, threeDsSession.PaymentMethodType),
                                threeDsSession.Language,
                                V7.Constants.Component.HandlePaymentChallenge,
                                V7.Constants.DescriptionTypes.ChallengeDescription,
                                threeDsSession.PaymentMethodType,
                                threeDsSession.Id,
                                null,
                                pxBrowserAuthenticateRedirectionUrl,
                                PXCommon.Constants.StaticDescriptionTypes.Cc3DSStatusCheckPidl,
                                false,
                                setting);
                            tryAgainPidl = PIDLGenerator.Generate(PIDLResourceFactory.ClientActionGenerationFactory, context).Context as PIDLResource;
                        }
                        else
                        {
                            tryAgainPidl = PIDLResourceFactory.Instance.GetCc3DSStatusCheckDescriptionForPaymentSession(threeDsSession.Id, threeDsSession.Language, threeDsSession.Partner, pxBrowserAuthenticateRedirectionUrl);
                        }

                        return this.Request.CreateResponse(HttpStatusCode.OK, tryAgainPidl);
                    }
                }

                return this.Request.CreateResponse(HttpStatusCode.BadRequest, new ErrorMessage() { ErrorCode = V7.Constants.PSD2ErrorCodes.InvalidPaymentSession, Message = "paymentSessionId is missing" });
            }
            catch (ServiceErrorResponseException ex)
            {
                return this.Request.CreateResponse(ex.Response.StatusCode, ex.Error);
            }
        }

        /// <summary>
        /// Get Payment Session for Add PI QR Code Flow
        /// </summary>
        /// <group>PaymentSessions</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/secondScreenSessions/{sessionId}/qrCodeStatus</url>
        /// <param name="accountId">Pi Account Id</param>
        /// <param name="sessionId">Session Id</param>
        /// <returns>Returns payment instrument or payment instrument status used for polling</returns>
        [HttpGet]
        [ActionName("qrCodeStatus")]
        public async Task<Microsoft.Commerce.Payments.PimsModel.V4.PaymentInstrument> GetQRCodePaymentSession(string accountId, string sessionId)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            try
            {
                PXService.Model.PXInternal.QRCodeSecondScreenSession qrCodeSecondScreenSession = new PXService.Model.PXInternal.QRCodeSecondScreenSession();

                // Get the session data 
                try
                {
                    qrCodeSecondScreenSession = await SecondScreenSessionHandler.GetQrCodeSessionData(sessionId, traceActivityId);
                }
                catch (Exception e)
                {
                    throw new ValidationException(ErrorCode.InvalidRequestData, e.Message);
                }

                // Null session data = invalid, throw error immediately
                if (qrCodeSecondScreenSession == null)
                {
                    throw new ValidationException(ErrorCode.InvalidRequestData, "qrCodeSecondScreenSession with the given sessionId for the accountId is missing.");
                }

                // validate that the two account ids are the same, if not throw error immediately
                if (!string.Equals(accountId, qrCodeSecondScreenSession.AccountId, StringComparison.OrdinalIgnoreCase))
                {
                    bool result = false;
                    if (!string.IsNullOrWhiteSpace(qrCodeSecondScreenSession.Id))
                    {
                        // The code can be kept after rollout to deal with multiple owners scenario in future
                        result = await PaymentSessionsHandler.CheckOwnership(accountId, qrCodeSecondScreenSession.PaymentInstrumentId, traceActivityId, exposedFlightFeatures: this.ExposedFlightFeatures);
                    }

                    if (!result)
                    {
                        throw new ValidationException(ErrorCode.InvalidRequestData, "PaymentSession with the given sessionId for the accountId is missing.");
                    }
                }

                string sessionQueryUrl = WebUtility.UrlEncode(string.Format("sessions/{0}", sessionId));

                PollingPaymentInstrument paymentInstrumentQrCodePolling = new PollingPaymentInstrument();

                if (qrCodeSecondScreenSession.Status == PaymentInstrumentPollingStatus.Pending)
                {
                    paymentInstrumentQrCodePolling.Status = qrCodeSecondScreenSession.Status;
                    return paymentInstrumentQrCodePolling;
                }
                else if (qrCodeSecondScreenSession.Status == PaymentInstrumentPollingStatus.Active)
                {
                    if (qrCodeSecondScreenSession?.PaymentInstrumentId == null)
                    {
                        throw TraceCore.TraceException(
                            traceActivityId,
                            new IntegrationException(
                                PXCommon.Constants.ServiceNames.InstrumentManagementService,
                                string.Format("PaymentInstrumentId should be present when session state is Success"),
                                "Unknown PIID"));
                    }

                    paymentInstrumentQrCodePolling = await this.Settings.PIMSAccessor.GetPaymentInstrument(accountId, qrCodeSecondScreenSession?.PaymentInstrumentId, traceActivityId, qrCodeSecondScreenSession.Partner, qrCodeSecondScreenSession.Country, qrCodeSecondScreenSession.Language, this.ExposedFlightFeatures);
                    return paymentInstrumentQrCodePolling;
                }
                else
                {
                    throw TraceCore.TraceException(
                        traceActivityId,
                        new IntegrationException(
                            PXCommon.Constants.ServiceNames.InstrumentManagementService,
                            string.Format("Unable to add payment instrument through second screen"),
                            "Unknown payment status"));
                }
            }
            catch (ServiceErrorResponseException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    var innerError = new ServiceErrorResponse(ex.Error.InnerError.ErrorCode, ex.Error.InnerError.Message);
                    var error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError);
                    var exception = new ServiceErrorResponseException() { Error = error, Response = this.Request.CreateResponse(HttpStatusCode.NotFound), HandlingType = ExceptionHandlingPolicy.ByPass };

                    throw TraceCore.TraceException(traceActivityId, exception);
                }

                throw;
            }
        }

        /// <summary>
        /// Used by App flow
        /// Authenticate
        /// </summary>
        /// <param name="accountId">Pi Account Id</param>
        /// <param name="sessionId">Session Id</param>
        /// <returns>Returns AuthenticationResponse</returns>
        [HttpPost]
        [ActionName("Authenticate")]
        public async Task<AuthenticationResponse> Authenticate(string accountId, string sessionId)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            string payload = await this.Request.GetRequestPayload();
            AuthenticationRequest authenticateRequest = null;
            try
            {
                authenticateRequest = JsonConvert.DeserializeObject<AuthenticationRequest>(payload);
            }
            catch (Exception e)
            {
                throw new ValidationException(ErrorCode.InvalidRequestData, e.Message);
            }

            PaymentSessionsHandler.ValidateSettingsVersion(
                authRequest: authenticateRequest,
                exposedFlightFeatures: this.ExposedFlightFeatures);

            PaymentSessionsHandler paymentSessionsHandler = await this.GetVersionBasedPaymentSessionsHandler(traceActivityId, sessionId);
            return await paymentSessionsHandler.Authenticate(
                accountId: accountId,
                sessionId: sessionId,
                authRequest: authenticateRequest,
                exposedFlightFeatures: this.ExposedFlightFeatures,
                traceActivityId: traceActivityId,
                testContext: HttpRequestHelper.GetTestHeader());
        }

        /// <summary>
        /// Used by App flow
        /// Create Payment Session and authenticate the payment session 
        /// It is merged call of create payment session and authenticate to reduce the network latency
        /// </summary>
        /// <param name="accountId">Pi Account Id</param>
        /// <returns>Returns the created PaymentSession</returns>
        [HttpPost]
        [ActionName("CreateAndAuthenticate")]
        public async Task<CreateAndAuthenticateResponse> CreateAndAuthenticate(string accountId)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            string payload = await this.Request.GetRequestPayload();
            CreateAndAuthenticateRequest request = null;
            try
            {
                request = JsonConvert.DeserializeObject<CreateAndAuthenticateRequest>(payload);
            }
            catch (Exception e)
            {
                throw new ValidationException(ErrorCode.InvalidRequestData, "missing required field " + e.Message);
            }

            PaymentSessionsHandler.ValidateSettingsVersion(
                authRequest: request.AuthenticateRequest,
                exposedFlightFeatures: this.ExposedFlightFeatures);

            string isMotoAuthorized = this.Request.GetRequestHeader(GlobalConstants.HeaderValues.IsMotoHeader);
            string tid = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.AadInfo.Tid);

            PaymentSessionsHandler paymentSessionsHandler = await this.GetVersionBasedPaymentSessionsHandler(traceActivityId);
            PaymentSession session = await paymentSessionsHandler.CreatePaymentSession(
                accountId: accountId,
                paymentSessionData: request.PaymentSessionData,
                deviceChannel: PXService.Model.ThreeDSExternalService.DeviceChannel.AppBased,
                exposedFlightFeatures: this.ExposedFlightFeatures,
                emailAddress: await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.EmailAddress),
                traceActivityId: traceActivityId,
                testContext: HttpRequestHelper.GetTestHeader(),
                isMotoAuthorized: isMotoAuthorized,
                tid: tid);

            if (session.IsChallengeRequired)
            {
                // TODO: Move the isChallengeRequired logic check to PaymentSessionHandler.Authenticate after flighting 
                // 100 % to V3 at which point, we can store isChallengeRequired field along with all the other field. 
                paymentSessionsHandler = await this.GetVersionBasedPaymentSessionsHandler(traceActivityId, session.Id);
                AuthenticationResponse response = await paymentSessionsHandler.Authenticate(
                    accountId: accountId,
                    sessionId: session.Id,
                    authRequest: request.AuthenticateRequest,
                    exposedFlightFeatures: this.ExposedFlightFeatures,
                    traceActivityId: traceActivityId,
                    testContext: HttpRequestHelper.GetTestHeader());

                session.ChallengeStatus = response.ChallengeStatus;
                session.UserDisplayMessage = response.CardHolderInfo;

                return new CreateAndAuthenticateResponse
                {
                    PaymentSession = session,
                    AuthenticateResponse = response
                };
            }
            else
            {
                return new CreateAndAuthenticateResponse
                {
                    PaymentSession = session,
                };
            }
        }

        /// <summary>
        /// Used by App flow
        /// Notify ThreeDS Challenge Completed
        /// </summary>
        /// <param name="accountId">Pi Account Id</param>
        /// <param name="sessionId">Session Id</param>
        /// <returns>Returns the created PaymentSession</returns>
        [HttpPost]
        [ActionName("NotifyThreeDSChallengeCompleted")]
        public async Task<HttpResponseMessage> NotifyThreeDSChallengeCompleted(string accountId, string sessionId)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            PaymentSessionsHandler paymentSessionsHandler = await this.GetVersionBasedPaymentSessionsHandler(traceActivityId, sessionId);
            PaymentSession paymentSession = await paymentSessionsHandler.CompleteThreeDSChallenge(
                accountId: accountId,
                sessionId: sessionId,
                exposedFlightFeatures: this.ExposedFlightFeatures,
                traceActivityId: traceActivityId);

            return this.Request.CreateResponse(paymentSession);
        }

        /// <summary>
        /// Used by browser flow
        /// Authenticate the payment session
        /// </summary>
        /// <param name="sessionId">Session Id</param>
        /// <returns>Returns the created PaymentSession</returns>
        [HttpPost]
        [ActionName("BrowserAuthenticate")]
        public async Task<HttpResponseMessage> Authenticate(string sessionId)
        {
            ClientAction nextAction = null;
            try
            {
                EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
                List<string> exposedFlightFeatures = await this.PaymentSessionsHandler.GetExposedFlightFeatures(sessionId, traceActivityId);
                string browser = HttpRequestHelper.GetBrowser(this.Request.ToHttpRequestMessage());
                int browserMajorVersion = HttpRequestHelper.GetBrowserMajorVer(this.Request.ToHttpRequestMessage());

                if (!this.Request.HasFormContentType)
                {
                    throw new ValidationException(ErrorCode.InvalidRequestData, "HTML form URL-encoded data is expected");
                }

                var formData = await Request.ReadFormAsync();
                string methodDataFieldValue = formData[V7.Constants.SessionFieldNames.ThreeDSMethodData];

                if (string.IsNullOrEmpty(methodDataFieldValue))
                {
                    SllWebLogger.TracePXServiceException(
                        $"ThreeDSMethodData is missing in Authenticate call for the sessionId - {sessionId}",
                        traceActivityId
                    );

                    // TODO: Uncomment the following code when banks are sending this data correctly.
                    // throw new ValidationException(ErrorCode.InvalidRequestData, string.Format(V7.Constants.MissingErrorMessage.MissingValue, V7.Constants.SessionFieldNames.ThreeDSMethodData));
                }

                PaymentSessionsHandler paymentSessionsHandler = await this.GetVersionBasedPaymentSessionsHandler(traceActivityId, sessionId);

                //// inner iframe is requested for fingerprint step
                string cspStepValue = formData.TryGetValue(V7.Constants.SessionFieldNames.CSPStep, out var cspStep)
                    ? cspStep.ToString()
                    : null;
                if (PXConstants.CSPStepNames.Fingerprint.Equals(cspStepValue))
                {
                    BrowserFlowContext browserFlowContext = await paymentSessionsHandler.GetThreeDSMethodData(sessionId, traceActivityId);

                    string responseContent = PIDLResourceFactory.ComposeHtmlCSPThreeDSFingerprintIFrameContent(browserFlowContext.FormActionURL, browserFlowContext.FormInputThreeDSMethodData);

                    // Needed to limit the fix to use src instead of srcdoc to Edge 78 and below as it breaks Webview 2
                    if ((browser.Equals("edge", StringComparison.OrdinalIgnoreCase) && browserMajorVersion < 79) ||
                        (browser.Equals("edge mobile", StringComparison.OrdinalIgnoreCase) && browserMajorVersion < 79) ||
                        (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXPSD2EnableCSPPostThreeDSMethodDataSrc, StringComparer.OrdinalIgnoreCase)))
                    {
                        responseContent = PIDLResourceFactory.ComposeHtmlCSPThreeDSFingerprintSrcIFrameContent(browserFlowContext.FormActionURL, browserFlowContext.FormInputThreeDSMethodData);
                    }

                    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StringContent(responseContent);
                    response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/html; charset=utf-8");
                    return response;
                }
                else if (PXConstants.CSPStepNames.Challenge.Equals(cspStepValue))
                {
                    //// inner iframe is requested for challenge step 
                    BrowserFlowContext browserFlowContext = await paymentSessionsHandler.GetThreeDSAuthenticationData(sessionId, traceActivityId);

                    string responseContent = PIDLResourceFactory.ComposeHtmlCSPThreeDSChallengeIFrameDescription(browserFlowContext.FormActionURL, browserFlowContext.FormInputCReq, browserFlowContext.FormInputThreeDSSessionData);

                    // Needed to limit the fix to use src instead of srcdoc to Edge 78 and below as it breaks Webview 2
                    if ((browser.Equals("edge", StringComparison.OrdinalIgnoreCase) && browserMajorVersion < 79) ||
                        (browser.Equals("edge mobile", StringComparison.OrdinalIgnoreCase) && browserMajorVersion < 79) ||
                        (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXPSD2EnableCSPPostThreeDSSessionDataSrc, StringComparer.OrdinalIgnoreCase)))
                    {
                        responseContent = PIDLResourceFactory.ComposeHtmlCSPThreeDSChallengeSrcIFrameDescription(browserFlowContext.FormActionURL, browserFlowContext.FormInputCReq, browserFlowContext.FormInputThreeDSSessionData);
                    }

                    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StringContent(responseContent);
                    response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/html; charset=utf-8");
                    return response;
                }

                // Inorder to pass the test header through the iframe. 
                // The method below set header as the input field in the form 
                SetupTestHeader(formData);

                string testHeader = HttpRequestHelper.GetRequestHeader(PaymentConstants.PaymentExtendedHttpHeaders.TestHeader);

                // check whether fingerprint timedout
                bool isThreeDSMethodCompleted = string.IsNullOrWhiteSpace(
                    formData.TryGetValue(PaymentConstants.NamedPorperties.FingerPrintTimedout, out var fingerPrintTimedout)
                        ? fingerPrintTimedout.ToString()
                        : null);

                BrowserFlowContext result = await paymentSessionsHandler.Authenticate(
                    sessionId: sessionId,
                    isThreeDSMethodCompleted: isThreeDSMethodCompleted,
                    traceActivityId: traceActivityId);

                if (result.IsAcsChallengeRequired)
                {
                    string pxAuthUrl = string.Format("{0}/paymentSessions/{1}/authenticate", this.Settings.PifdBaseUrl, sessionId);
                    bool cspFrameEnabled = exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXPSD2EnableCSPProxyFrame, StringComparer.OrdinalIgnoreCase);
                    bool cspFrameUrlEnabled = exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXPSD2EnableCSPUrlProxyFrame, StringComparer.OrdinalIgnoreCase);
                    cspStep = PXConstants.CSPStepNames.None;
                    string formActionUrl = result.FormActionURL;

                    if (cspFrameEnabled || cspFrameUrlEnabled)
                    {
                        cspStep = PXConstants.CSPStepNames.Challenge;
                        formActionUrl = pxAuthUrl;
                    }

                    if (cspFrameUrlEnabled)
                    {
                        nextAction = PIDLResourceFactory.GetThreeDSChallengeUrlIFrameClientAction(
                            formActionUrl,
                            result.FormInputCReq,
                            result.FormInputThreeDSSessionData,
                            sessionId,
                            cspStep,
                            result.ChallengeWindowSize?.Width,
                            result.ChallengeWindowSize?.Height,
                            testHeader,
                            exposedFlightFeatures);
                    }
                    else
                    {
                        nextAction = PIDLResourceFactory.GetThreeDSChallengeIFrameClientAction(
                            formActionUrl,
                            result.FormInputCReq,
                            result.FormInputThreeDSSessionData,
                            sessionId,
                            cspStep,
                            result.ChallengeWindowSize?.Width,
                            result.ChallengeWindowSize?.Height,
                            testHeader);
                    }
                }
                else if (result.PaymentSession.ChallengeStatus == PaymentChallengeStatus.Succeeded
                    || result.PaymentSession.ChallengeStatus == PaymentChallengeStatus.ByPassed
                    || result.PaymentSession.ChallengeStatus == PaymentChallengeStatus.NotApplicable)
                {
                    nextAction = new ClientAction(ClientActionType.ReturnContext, result.PaymentSession);

                    // for caas/paas flows where request Id is not null, we need to attach the challenge data to PO if the challenge was successful/bypassed/not applicable.
                    if (!string.IsNullOrEmpty(result.PaymentSession?.RequestId))
                    {
                        if (this.UsePaymentRequestApiEnabled())
                        {
                            var clientActions = await this.Settings.PaymentOrchestratorServiceAccessor.PSD2AttachChallengeDataToPaymentRequest(
                                result.PaymentSession.RequestId,
                                result.PaymentSession.PaymentInstrumentId,
                                PXService.Model.PaymentOrchestratorService.PaymentInstrumentChallengeType.ThreeDs2,
                                result.PaymentSession.ChallengeStatus,
                                result.PaymentSession.Id,
                                traceActivityId,
                                result.PaymentSession?.TenantId);

                            nextAction = new ClientAction(ClientActionType.ReturnContext, clientActions);
                        }
                        else
                        {
                            var clientActions = await this.Settings.PaymentOrchestratorServiceAccessor.PSD2AttachChallengeData(
                            result.PaymentSession.RequestId,
                            result.PaymentSession.PaymentInstrumentId,
                            PXService.Model.PaymentOrchestratorService.PaymentInstrumentChallengeType.ThreeDs2,
                            result.PaymentSession.ChallengeStatus,
                            result.PaymentSession.Id,
                            traceActivityId,
                            result.PaymentSession?.TenantId);

                            nextAction = new ClientAction(ClientActionType.ReturnContext, clientActions);
                        }
                    }
                }
                else
                {
                    nextAction = CreateFailureClientAction(HttpStatusCode.BadRequest, V7.Constants.PSD2ErrorCodes.RejectedByProvider, result.PaymentSession.ChallengeStatus.ToString(), result.CardHolderInfo);
                }
            }
            catch (Exception ex)
            {
                nextAction = CreateFailureClientAction(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError.ToString(), ex.Message);
            }

            if (nextAction != null)
            {
                nextAction.ActionId = sessionId;
            }

            return ComposeHtmlPostMessageResponse(nextAction);
        }

        [HttpPost]
        [ActionName("BrowserNotifyThreeDSChallengeCompleted")]
        public async Task<HttpResponseMessage> NotifyThreeDSChallengeCompleted(string sessionId)
        {
            ClientAction clientAction = null;
            try
            {
                EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

                if (!this.Request.HasFormContentType)
                {
                    throw new ValidationException(ErrorCode.InvalidRequestData, "HTML form URL-encoded data is expected");
                }

                var formData = await Request.ReadFormAsync();

                // Inorder to pass the test header through the iframe. 
                // The method below set header as the input field in the form 
                SetupTestHeader(formData);

                List<string> exposedFlightFeatures = this.ExposedFlightFeatures;

                PaymentSessionsHandler paymentSessionsHandler = await this.GetVersionBasedPaymentSessionsHandler(traceActivityId, sessionId);
                PaymentSession paymentSession = await paymentSessionsHandler.CompleteThreeDSChallenge(
                    accountId: null,
                    sessionId: sessionId,
                    exposedFlightFeatures: exposedFlightFeatures,
                    traceActivityId: traceActivityId);

                if (paymentSession.ChallengeStatus == PaymentChallengeStatus.Succeeded)
                {
                    clientAction = new ClientAction(ClientActionType.ReturnContext, paymentSession);

                    // for caas/paas flows where request Id is not null, we need to attach the challenge data to PO if the challenge was successful/bypassed/not applicable.
                    if (!string.IsNullOrEmpty(paymentSession?.RequestId))
                    {
                        if (this.UsePaymentRequestApiEnabled())
                        {
                            var clientActions = await this.Settings.PaymentOrchestratorServiceAccessor.PSD2AttachChallengeDataToPaymentRequest(
                                paymentSession.RequestId,
                                paymentSession.PaymentInstrumentId,
                                PXService.Model.PaymentOrchestratorService.PaymentInstrumentChallengeType.ThreeDs2,
                                paymentSession.ChallengeStatus,
                                paymentSession.Id,
                                traceActivityId,
                                paymentSession?.TenantId);

                            clientAction = new ClientAction(ClientActionType.ReturnContext, clientActions);
                        }
                        else
                        {
                            var clientActions = await this.Settings.PaymentOrchestratorServiceAccessor.PSD2AttachChallengeData(
                                paymentSession.RequestId,
                                paymentSession.PaymentInstrumentId,
                                PXService.Model.PaymentOrchestratorService.PaymentInstrumentChallengeType.ThreeDs2,
                                paymentSession.ChallengeStatus,
                                paymentSession.Id,
                                traceActivityId,
                                paymentSession?.TenantId);

                            clientAction = new ClientAction(ClientActionType.ReturnContext, clientActions);
                        }
                    }
                }
                else if (paymentSession.ChallengeStatus == PaymentChallengeStatus.Cancelled)
                {
                    clientAction = new ClientAction(ClientActionType.GoHome, paymentSession);
                }
                else
                {
                    clientAction = CreateFailureClientAction(HttpStatusCode.BadRequest, V7.Constants.PSD2ErrorCodes.RejectedByProvider, paymentSession.ChallengeStatus.ToString());
                }
            }
            catch (Exception ex)
            {
                clientAction = CreateFailureClientAction(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError.ToString(), ex.Message);
            }

            if (clientAction != null)
            {
                clientAction.ActionId = sessionId;
            }

            return ComposeHtmlPostMessageResponse(clientAction);
        }

        /// <summary>
        /// Used by Consumer and Commercial StoreFronts for 3DS (1.0) authentication in India market
        /// POST /authenticateIndiaThreeDS
        /// </summary>
        /// <param name="accountId">Pi Account Id</param>
        /// <param name="sessionId">Session Id</param>
        /// <param name="cvvChallengePayload">Payload containing tokenized CVV</param>
        /// <returns>Returns RDS URL</returns>
        [HttpPost]
        [ActionName("BrowserAuthenticateThreeDSOne")]
        public async Task<HttpResponseMessage> BrowserAuthenticateThreeDSOne(
            string accountId,
            string sessionId,
            [FromBody] PIDLData cvvChallengePayload)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            TestContext testContext = HttpRequestHelper.GetTestHeader(this.Request);
            PaymentSession session = await PaymentSessionsHandler.TryGetPaymentSession(sessionId, traceActivityId);

            // Use Partner Settings if enabled for the partner
            this.PartnerSettings = await PXService.PartnerSettingsHelper.GetPaymentExperienceSetting(this.Settings, session.Partner, null, traceActivityId, this.ExposedFlightFeatures);
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(V7.Constants.Component.HandlePaymentChallenge);

            // Return static pidl back, remove the following block once india3ds work end to end
            if (HttpRequestHelper.HasThreeDSOneTestScenario(testContext))
            {
                PIDLResource resource = new PIDLResource();
                string rdsUrl = "https://india3dssimpleredirectgroup.azurewebsites.net";

                if (PartnerHelper.IsThreeDSOneQrCodeBasedPurchasePartner(session.Partner, setting))
                {
                    var clientAction = new ClientAction(ClientActionType.Pidl);
                    clientAction.Context = PIDLResourceFactory.Instance.GetQrCodeChallengeDescriptionForThreeDSOnePurchase(rdsUrl, PXCommon.Constants.ChallengeTypes.ThreeDSOneQrCode, session.Language, session.Country, session.Partner, sessionId, session);
                    resource.ClientAction = clientAction;
                }
                else
                {
                    if (setting != null && setting.RedirectionPattern != null)
                    {
                        PIDLGeneratorContext context = new PIDLGeneratorContext(
                                    session.Country,
                                    session.Partner,
                                    TemplateHelper.GetSettingTemplate(session.Partner, setting, V7.Constants.DescriptionTypes.ChallengeDescription, session.PaymentMethodType),
                                    session.Language,
                                    V7.Constants.Component.HandlePaymentChallenge,
                                    V7.Constants.DescriptionTypes.ChallengeDescription,
                                    session.PaymentMethodType,
                                    session.Id,
                                    null,
                                    rdsUrl,
                                    PXCommon.Constants.StaticDescriptionTypes.Cc3DSRedirectAndStatusCheckPidl,
                                    false,
                                    setting);
                        resource = PIDLGenerator.Generate(PIDLResourceFactory.ClientActionGenerationFactory, context).Context as PIDLResource;
                        return this.Request.CreateResponse(HttpStatusCode.OK, resource);
                    }

                    resource = PartnerHelper.IsIndiaThreeDSFlightedInlinePartner(session.Partner, setting) // Read From PSS Settings
                        ? PIDLResourceFactory.GetRedirectPidl(rdsUrl)
                        : PIDLResourceFactory.Instance.GetCc3DSRedirectAndStatusCheckDescriptionForPaymentSession(session.Id, session.Language, session.Partner, rdsUrl, setting: setting);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, resource);
            }

            object cvvToken;
            if (!cvvChallengePayload.TryGetValue("cvvToken", out cvvToken))
            {
                throw new ValidationException(ErrorCode.InvalidRequestData, "missing required field ");
            }

            ClientAction nextAction;
            try
            {
                PaymentSessionsHandler paymentSessionsHandler = await this.GetVersionBasedPaymentSessionsHandler(traceActivityId, sessionId);
                BrowserFlowContext result = await paymentSessionsHandler.AuthenticateThreeDSOne(
                    sessionId: sessionId,
                    cvvToken: (string)cvvToken,
                    traceActivityId: traceActivityId,
                    userId: await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.Puid));

                //// whether to use "isFullPageRedirect" is under discussion. For now, iframe is the experience only for the partners like payin i.e. partners who have not done work to open in new tab or full page redir
                //// for partners who have adopted either new tab or full page redir, do not show iframe experience for now, unless iframe test header is sent
                //// Todo: Remove the HasThreeDSOneTestScenarioIframeOverriding once payerAuth support correct isFullPageRedirect value. Temp work around
                if (HttpRequestHelper.HasThreeDSOneTestScenarioIframeOverriding(testContext) || PartnerHelper.IsThreeDSOneIframeBasedPartner(session.Partner, setting))
                {
                    result.FormFullPageRedirectAcsURL = false;
                }

                string testHeader = HttpRequestHelper.GetRequestHeader(PaymentConstants.PaymentExtendedHttpHeaders.TestHeader);

                if (result.IsAcsChallengeRequired
                    && (result.FormFullPageRedirectAcsURL != null && !(bool)result.FormFullPageRedirectAcsURL))
                {
                    if (PartnerHelper.IsThreeDSOneQrCodeBasedPurchasePartner(session.Partner))
                    {
                        string rdsUrl = $"{this.PidlBaseUrl}/paymentSessions/{session.Id}/browserAuthenticateRedirectionThreeDSOne";
                        var clientAction = new ClientAction(ClientActionType.Pidl);
                        clientAction.Context = PIDLResourceFactory.Instance.GetQrCodeChallengeDescriptionForThreeDSOnePurchase(rdsUrl, PXCommon.Constants.ChallengeTypes.ThreeDSOneQrCode, session.Language, session.Country, session.Partner, sessionId, session);
                        var resource = new PIDLResource
                        {
                            ClientAction = clientAction
                        };

                        return this.Request.CreateResponse(HttpStatusCode.OK, resource);
                    }
                    else
                    {
                        nextAction = PIDLResourceFactory.GetThreeDSOneChallengeIFrameClientAction(
                            result.FormActionURL,
                            result.FormInputCReq,
                            sessionId,
                            session.Language,
                            result.ChallengeWindowSize?.Width,
                            result.ChallengeWindowSize?.Height,
                            session.Partner,
                            testHeader);
                    }
                }
                else if (result.IsAcsChallengeRequired && (result.FormFullPageRedirectAcsURL != null && (bool)result.FormFullPageRedirectAcsURL))
                {
                    PIDLResource resource = null;
                    string redirectUrl = null;

                    if (HttpRequestHelper.HasAnyThreeDSOneTestScenarios(testContext))
                    {
                        redirectUrl = "https://india3dssimpleredirectgroup.azurewebsites.net";
                    }
                    else
                    {
                        redirectUrl = $"{this.PidlBaseUrl}/paymentSessions/{session.Id}/browserAuthenticateRedirectionThreeDSOne";
                    }

                    if (PartnerHelper.IsThreeDSOneQrCodeBasedPurchasePartner(session.Partner, setting))
                    {
                        var clientAction = new ClientAction(ClientActionType.Pidl);
                        clientAction.Context = PIDLResourceFactory.Instance.GetQrCodeChallengeDescriptionForThreeDSOnePurchase(redirectUrl, PXCommon.Constants.ChallengeTypes.ThreeDSOneQrCode, session.Language, session.Country, session.Partner, sessionId, session);
                        resource = new PIDLResource
                        {
                            ClientAction = clientAction
                        };
                    }
                    else
                    {
                        if (setting != null && setting.RedirectionPattern != null)
                        {
                            PIDLGeneratorContext context = new PIDLGeneratorContext(
                                        session.Country,
                                        session.Partner,
                                        TemplateHelper.GetSettingTemplate(session.Partner, setting, V7.Constants.DescriptionTypes.ChallengeDescription, session.PaymentMethodType),
                                        session.Language,
                                        V7.Constants.Component.HandlePaymentChallenge,
                                        V7.Constants.DescriptionTypes.ChallengeDescription,
                                        session.PaymentMethodType,
                                        session.Id,
                                        null,
                                        redirectUrl,
                                        PXCommon.Constants.StaticDescriptionTypes.Cc3DSRedirectAndStatusCheckPidl,
                                        false,
                                        setting);
                            resource = PIDLGenerator.Generate(PIDLResourceFactory.ClientActionGenerationFactory, context).Context as PIDLResource;
                            return this.Request.CreateResponse(HttpStatusCode.OK, resource);
                        }

                        resource = PartnerHelper.IsIndiaThreeDSFlightedInlinePartner(session.Partner, setting)
                            ? PIDLResourceFactory.GetRedirectPidl(redirectUrl)
                            : PIDLResourceFactory.Instance.GetCc3DSRedirectAndStatusCheckDescriptionForPaymentSession(session.Id, session.Language, session.Partner, redirectUrl);
                    }

                    return this.Request.CreateResponse(HttpStatusCode.OK, resource);
                }
                else if (result.PaymentSession.ChallengeStatus == PaymentChallengeStatus.Succeeded
                    || result.PaymentSession.ChallengeStatus == PaymentChallengeStatus.ByPassed
                    || result.PaymentSession.ChallengeStatus == PaymentChallengeStatus.NotApplicable)
                {
                    nextAction = new ClientAction(ClientActionType.ReturnContext, result.PaymentSession);
                }
                else
                {
                    nextAction = CreateFailureClientAction(HttpStatusCode.BadRequest, V7.Constants.PSD2ErrorCodes.RejectedByProvider, result.PaymentSession.ChallengeStatus.ToString(), result.CardHolderInfo);
                }
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException("BrowserAuthenticateThreeDSOne exception: " + ex.ToString(), traceActivityId);
                nextAction = CreateFailureClientAction(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError.ToString(), ex.Message);
            }

            if (nextAction != null)
            {
                nextAction.ActionId = sessionId;
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, new PIDLResource { ClientAction = nextAction });
        }

        /// <summary>
        /// Used by Browser based flows for 3DS (1.0) authentication redirection in India market
        /// GET /BrowserAuthenticateRedirectionThreeDSOne
        /// </summary>
        /// <param name="sessionId">Session Id</param>
        /// <param name="ru">Pi Account Id</param>
        /// <param name="rx">Payload containing tokenized CVV</param>
        /// <returns>Returns the HTML that does a POST to ACS Url</returns>
        [HttpGet]
        [ActionName("BrowserAuthenticateRedirectionThreeDSOne")]
        public async Task<HttpResponseMessage> BrowserAuthenticateRedirectionThreeDSOne(string sessionId, string ru, string rx)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            if (!IsBrowserAuthenticateThreeDSOneRedirectionUrlValid(ru))
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, new ErrorMessage() { ErrorCode = V7.Constants.PSD2ErrorCodes.InvalidSuccessRedirectionUrl, Message = "Invalid success redirection url" });
            }

            if (!IsBrowserAuthenticateThreeDSOneRedirectionUrlValid(rx))
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, new ErrorMessage() { ErrorCode = V7.Constants.PSD2ErrorCodes.InvalidFailureRedirectionUrl, Message = "Invalid failure redirection url" });
            }

            PaymentSessionsHandler paymentSessionsHandler = await this.GetVersionBasedPaymentSessionsHandler(traceActivityId, sessionId);
            var browserFlowContext = await paymentSessionsHandler.AuthenticateRedirectionThreeDSOne(
                sessionId: sessionId,
                successUrl: ru,
                failureUrl: rx,
                traceActivityId: traceActivityId);

            string html = PIDLResourceFactory.ComposeHtmlAuthenticateRedirectionThreeDSOne(browserFlowContext.FormActionURL, browserFlowContext.FormInputCReq);

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(html)
            };

            response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/html; charset=utf-8");

            return response;
        }

        [HttpPost]
        [ActionName("BrowserNotifyThreeDSOneChallengeCompleted")]
        public async Task<HttpResponseMessage> BrowserNotifyThreeDSOneChallengeCompleted(string sessionId)
        {
            Uri redirectUrl;

            try
            {
                EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

                if (!this.Request.HasFormContentType)
                {
                    throw new ValidationException(ErrorCode.InvalidRequestData, "HTML form URL-encoded data is expected");
                }

                var formData = await Request.ReadFormAsync();
                SetupTestHeader(formData);

                Dictionary<string, string> authParams = new Dictionary<string, string>();
                foreach (var key in formData.Keys)
                {
                    authParams.Add(key, formData[key]);
                }

                PaymentSessionsHandler paymentSessionsHandler = await this.GetVersionBasedPaymentSessionsHandler(traceActivityId, sessionId);
                PaymentSession paymentSession = await paymentSessionsHandler.CompleteThreeDSOneChallenge(
                    accountId: null,
                    sessionId: sessionId,
                    authParameters: authParams,
                    exposedFlightFeatures: this.ExposedFlightFeatures,
                    traceActivityId: traceActivityId);

                // for the iframe flow, we will return paymentsession back.
                if (string.IsNullOrEmpty(paymentSession.FailureUrl) && string.IsNullOrEmpty(paymentSession.SuccessUrl))
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK);
                }

                redirectUrl = PaymentSessionsHandler.GetChallengeRedirectUriFromPaymentSession(paymentSession);
            }
            catch (Exception ex)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }

            string html = PIDLResourceFactory.ComposeHtmlNotifyThreeDSOneChallengeCompleted(redirectUrl.ToString());
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(html)
            };

            response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/html; charset=utf-8");
            return response;
        }

        /// <summary>
        /// Used by Commercial StoreFronts for 3DS (1.0) authentication in India market
        /// POST /authenticateIndiaThreeDS
        /// </summary>
        /// <param name="accountId">Pi Account Id</param>
        /// <param name="sessionId">Session Id</param>
        /// <param name="cvvChallengePayload">Payload containing tokenized CVV</param>
        /// <returns>Returns RDS URL</returns>
        [HttpPost]
        [ActionName("AuthenticateIndiaThreeDS")]
        public async Task<HttpResponseMessage> AuthenticateIndiaThreeDS(string accountId, string sessionId, [FromBody] PIDLData cvvChallengePayload)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            object cvvToken;
            if (!cvvChallengePayload.TryGetValue("cvvToken", out cvvToken))
            {
                throw new ValidationException(ErrorCode.InvalidRequestData, "missing required field ");
            }

            PaymentSession paymentSession = await PaymentSessionsHandler.TryGetPaymentSession(sessionId, traceActivityId);
            string partnerName = paymentSession?.Partner;

            // Use Partner Settings if enabled for the partner
            this.PartnerSettings = await PXService.PartnerSettingsHelper.GetPaymentExperienceSetting(this.Settings, partnerName, null, traceActivityId, this.ExposedFlightFeatures);
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(V7.Constants.Component.HandlePaymentChallenge);

            PaymentSessionsHandler paymentSessionsHandler = await this.GetVersionBasedPaymentSessionsHandler(traceActivityId, sessionId);
            TransactionResource transactionResource = await paymentSessionsHandler.AuthenticateIndiaThreeDS(
                accountId: accountId,
                sessionId: sessionId,
                cvvToken: cvvToken.ToString(),
                traceActivityId: traceActivityId,
                setting);

            string rdsUrl = transactionResource != null ? transactionResource.RedirectUrl : string.Empty;

            ClientAction clientAction = new ClientAction(ClientActionType.Redirect);
            PIDLResource threeDSRedirectPidl = new PIDLResource();
            HttpStatusCode httpStatusCode = HttpStatusCode.OK;

            if (!string.IsNullOrEmpty(rdsUrl))
            {
                string rdsSessionId = rdsUrl.TrimEnd('/', ' ').Split('/').LastOrDefault();

                if (setting != null && setting.RedirectionPattern != null)
                {
                    PIDLGeneratorContext context = new PIDLGeneratorContext(
                        paymentSession?.Country,
                        partnerName,
                        TemplateHelper.GetSettingTemplate(partnerName, setting, V7.Constants.DescriptionTypes.ChallengeDescription, paymentSession.PaymentMethodType),
                        paymentSession?.Language,
                        V7.Constants.Component.HandlePaymentChallenge,
                        V7.Constants.DescriptionTypes.ChallengeDescription,
                        paymentSession.PaymentMethodType,
                        sessionId: sessionId,
                        rdsSessionId: rdsSessionId,
                        redirectUrl: rdsUrl,
                        PXCommon.Constants.StaticDescriptionTypes.Cc3DSRedirectAndStatusCheckPidl,
                        true,
                        setting);

                    clientAction = PIDLGenerator.Generate<ClientAction>(PIDLResourceFactory.ClientActionGenerationFactory, context);
                    if (clientAction != null)
                    {
                        threeDSRedirectPidl.ClientAction = clientAction;
                        return this.Request.CreateResponse(httpStatusCode, threeDSRedirectPidl);
                    }
                }

                if (PartnerHelper.IsAzurePartner(partnerName))
                {
                    clientAction = new ClientAction(ClientActionType.Pidl);
                    clientAction.Context = PIDLResourceFactory.Instance.Get3DSRedirectAndStatusCheckDescriptionForPaymentAuth(rdsUrl, rdsSessionId, sessionId, partnerName, paymentSession?.Language, paymentSession?.Country, PXCommon.Constants.StaticDescriptionTypes.Cc3DSRedirectAndStatusCheckPidl);
                }
                else
                {
                    RedirectionServiceLink redirectLink = new RedirectionServiceLink { BaseUrl = rdsUrl };
                    redirectLink.RuParameters.Add("sessionId", sessionId);
                    redirectLink.RxParameters.Add("sessionId", sessionId);

                    clientAction = new ClientAction(ClientActionType.Redirect);
                    clientAction.Context = redirectLink;
                }

                threeDSRedirectPidl.ClientAction = clientAction;
            }
            else
            {
                // converting system error to user error
                threeDSRedirectPidl = IndiaThreeDSErrorHandler(threeDSRedirectPidl, transactionResource, out httpStatusCode, this.ExposedFlightFeatures);
            }

            return this.Request.CreateResponse(httpStatusCode, threeDSRedirectPidl);
        }

        /// <summary>
        /// Verifies that the user's PI has been authenticated via a 3DS2 challenge by the financial provider when applicable.
        /// </summary>
        /// <param name="accountId">ID of the user account which PI was challenged</param>
        /// <param name="sessionId">ID of the 3ds challenge session being verified</param>
        /// <param name="piId">ID of the payment instrument that was challenged</param>
        /// <param name="paymentContext">Json object of all the fields that needs to be verified</param>
        /// <returns>Returns an object with the property Verified=true when the challenge has been verified or is not applicable.
        /// False if the challenge was failed or its status is unknown.</returns>
        [HttpGet]
        [ActionName("AuthenticationStatus")]
        public async Task<HttpResponseMessage> AuthenticationStatus(string accountId, string sessionId, string piId, string paymentContext = null)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            string correlationContext = this.Request.GetRequestHeader(GlobalConstants.HeaderValues.CorrelationContext);
            string partnerValue = correlationContext?.Split(',')
                .Select(part => part.Split('='))
                .FirstOrDefault(parts => parts.Length == 2 && parts[0] == "ms.b.tel.partner")?[1];
            try
            {
                if (this.ExposedFlightFeatures != null && this.ExposedFlightFeatures.Contains(Flighting.Features.PXAuthenticateStatusForceVerifiedTrue))
                {
                    return this.BuildSuccessResponse(true, sessionId, piId, verified: true, PaymentChallengeStatus.NotApplicable);
                }

                var overrideVerification = this.ExposedFlightFeatures != null && this.ExposedFlightFeatures.Contains(Flighting.Features.PXAuthenticateStatusOverrideVerification);

                PaymentInstrumentSession piSession = null;
                if (this.ExposedFlightFeatures != null && this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnablePSD2PaymentInstrumentSession, StringComparer.OrdinalIgnoreCase))
                {
                    try
                    {
                        piSession = await this.Settings.SessionServiceAccessor.GetSessionResourceData<PaymentInstrumentSession>(V7.Constants.Prefixes.PaymentInstrumentSessionPrefix + piId, traceActivityId);
                    }
                    catch (Exception e)
                    {
                        SllWebLogger.TracePXServiceException("Error retrieving payment instrument session. " + e.Message, traceActivityId);
                    }
                }

                List<string> challenges;
                bool isChallengeRequired;
                if (piSession != null)
                {
                    challenges = piSession.RequiredChallenge ?? new List<string>();
                    isChallengeRequired = challenges.Contains(V7.Constants.ScenarioNames.ThreeDSTwo);
                }
                else
                {
                    // Look PI and validate the ownership
                    PimsModel.V4.PaymentInstrument paymentInstrument = await this.LookupPaymentInstrument(accountId, piId, extendedView: true, traceActivityId);
                    if (paymentInstrument == null)
                    {
                        return this.BuildFailureResponse(overrideVerification, piId, sessionId);
                    }
                    else if ((paymentInstrument.IsCreditCard()
                        || paymentInstrument.IsGooglePay()) &&
                        !string.Equals(accountId, paymentInstrument.PaymentInstrumentAccountId, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // since the PI accountId doesn't match with the incoming account, let's try another way to verify the ownership
                        // when billableAccountId is used for the Azure flows
                        var pi = await this.LookupPaymentInstrument(accountId, piId, extendedView: false, traceActivityId);
                        if (pi == null)
                        {
                            return this.BuildFailureResponse(overrideVerification, piId, sessionId);
                        }
                    }

                    // check if the PI requires 3DS2 challenge
                    challenges = paymentInstrument.PaymentInstrumentDetails?.RequiredChallenge ?? new List<string>();
                    isChallengeRequired = challenges.Contains(V7.Constants.ScenarioNames.ThreeDSTwo);
                }

                if (!isChallengeRequired)
                {
                    return this.BuildSuccessResponse(overrideVerification, sessionId, piId, verified: true, PaymentChallengeStatus.NotApplicable);
                }

                // evaluate for the challenge completion
                var paymentSession = await PaymentSessionsHandler.TryGetStoredSession(piSession?.SessionId, traceActivityId);
                if (paymentSession == null)
                {
                    paymentSession = await PaymentSessionsHandler.TryGetStoredSession(sessionId, traceActivityId);
                }

                if (paymentSession != null)
                {
                    // this is additional check to deal with the BIN based PSD2 flighting that is controlled by the DCS configuration
                    // this situation happens when the BIN based flighting is NOT turned at 100%
                    if (!paymentSession.PiRequiresAuthentication)
                    {
                        return this.BuildSuccessResponse(overrideVerification, sessionId, piId, verified: true, PaymentChallengeStatus.NotApplicable);
                    }

                    bool ownerVerified = string.Equals(piId, paymentSession.PaymentInstrumentId, StringComparison.InvariantCultureIgnoreCase);

                    bool challengeVerified = paymentSession.ChallengeStatus == PaymentChallengeStatus.ByPassed ||
                        paymentSession.ChallengeStatus == PaymentChallengeStatus.NotApplicable ||
                        paymentSession.ChallengeStatus == PaymentChallengeStatus.Succeeded;

                    if (paymentSession.ChallengeStatus == PaymentChallengeStatus.Unknown)
                    {
                        challengeVerified = challengeVerified || paymentSession.IsSystemError || IsValidTransactionStatus(paymentSession);
                    }

                    VerificationResult verficationResult = VerificationResult.Success;
                    bool additionalParamsVerified = true;
                    if (this.ExposedFlightFeatures != null &&
                            (this.ExposedFlightFeatures.Contains(Flighting.Features.ValidatePaymentSessionProperties, StringComparer.OrdinalIgnoreCase)
                            || this.ExposedFlightFeatures.Contains(Flighting.Features.ValidatePaymentSessionPropertiesLogging, StringComparer.OrdinalIgnoreCase)))
                    {
                        if (paymentContext != null)
                        {
                            verficationResult = VerifyAdditionalSessionDataParams(paymentSession, paymentContext, piId, this.ExposedFlightFeatures);

                            if (verficationResult != VerificationResult.Success)
                            {
                                additionalParamsVerified = false;
                            }
                        }
                    }

                    // ToDo: clean this up after SDK is fixed
                    if (this.ExposedFlightFeatures != null
                        && this.ExposedFlightFeatures.Contains(Flighting.Features.PXAuthenticateStatusOverrideVerificationForXbox, StringComparer.OrdinalIgnoreCase)
                        && paymentSession.ChallengeStatus == PaymentChallengeStatus.Failed
                        && string.Equals(partnerValue, "xbox", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Xbox partner has a special case where the challenge status is not reliable
                        // logging an exception to track if the issue is fixed after an SDK update
                        SllWebLogger.TracePXServiceException("Overriding verified flag for xbox partner", traceActivityId);
                        return this.BuildSuccessResponse(overrideVerification, sessionId, piId, verified: true, PaymentChallengeStatus.NotApplicable);
                    }

                    if (this.ExposedFlightFeatures != null && this.ExposedFlightFeatures.Contains(Flighting.Features.ValidatePaymentSessionProperties, StringComparer.OrdinalIgnoreCase))
                    {
                        return this.BuildSuccessResponse(overrideVerification, sessionId, piId, verified: ownerVerified && challengeVerified && additionalParamsVerified, paymentSession.ChallengeStatus, verficationResult);
                    }

                    return this.BuildSuccessResponse(overrideVerification, sessionId, piId, verified: ownerVerified && challengeVerified, paymentSession.ChallengeStatus);
                }
                else
                {
                    return this.BuildFailureResponse(overrideVerification, piId, sessionId);
                }
            }
            catch (ServiceErrorResponseException ex)
            {
                return this.Request.CreateResponse(ex.Response.StatusCode, ex.Error);
            }
        }

        private static VerificationResult VerifyAdditionalSessionDataParams(PXService.Model.PXInternal.PaymentSession paymentSession, string paymentContext, string piId, List<string> exposedFlightFeatures)
        {
            try
            {
                PaymentContext paymentContextObj = JsonConvert.DeserializeObject<PaymentContext>(paymentContext);

                // Capture currency values at the exact point of comparison to avoid race conditions
                string paymentContextCurrency = paymentContextObj?.Currency;
                string paymentSessionCurrency = paymentSession?.Currency;
                decimal paymentSessionAmount = (decimal)paymentSession?.Amount;
                decimal preTaxAmount = (decimal)paymentContextObj?.Pretax;
                decimal postTaxAmount = (decimal)paymentContextObj?.PostTax;

                // For 0 amount transactions, we skip currency verification since currency mismatch is acceptable in those cases
                bool currencyVerified = paymentSession?.Amount == 0 || string.Equals(paymentContextCurrency, paymentSessionCurrency, StringComparison.OrdinalIgnoreCase);
                bool partnerVerified = string.Equals(paymentContextObj?.Partner, paymentSession.Partner, StringComparison.OrdinalIgnoreCase);
                bool countryVerified = string.Equals(paymentContextObj?.Country, paymentSession.Country, StringComparison.OrdinalIgnoreCase);
                bool hasPreorderVerified = bool.Equals(paymentContextObj?.HasPreOrder, paymentSession.HasPreOrder);
                bool isMotoVerified = bool.Equals(paymentContextObj?.IsMOTO, paymentSession.IsMOTO);
                bool challengeScenarioVerified = string.Equals(paymentContextObj?.ChallengeScenario?.ToString(), paymentSession?.ChallengeScenario.ToString(), StringComparison.OrdinalIgnoreCase);
                bool purchaseOrderIdVerified = string.Equals(paymentContextObj?.PurchaseOrderId, paymentSession.PurchaseOrderId, StringComparison.OrdinalIgnoreCase);

                // Session amount should be within +/-30% of pretax and posttax amounts 
                decimal preTax_lowerLimit = (decimal)preTaxAmount * (decimal)0.7;
                decimal preTax_upperLimit = (decimal)preTaxAmount * (decimal)1.3;

                decimal postTax_lowerLimit = (decimal)postTaxAmount * (decimal)0.7;
                decimal postTax_upperLimit = (decimal)postTaxAmount * (decimal)1.3;

                // Only needs to fit one of the following
                bool preTaxVarienceVerified = paymentSessionAmount >= preTax_lowerLimit && paymentSessionAmount <= preTax_upperLimit;
                bool postTaxVarienceVerified = paymentSessionAmount >= postTax_lowerLimit && paymentSessionAmount <= postTax_upperLimit;

                if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.ValidatePaymentSessionPropertiesLogging, StringComparer.OrdinalIgnoreCase))
                {
                    var totalValidationResults = new
                    {
                        currencyVerified,
                        partnerVerified,
                        countryVerified,
                        hasPreorderVerified,
                        isMotoVerified,
                        challengeScenarioVerified,
                        preTaxVarienceVerified,
                        postTaxVarienceVerified,
                        paymentSessionAmount,
                        preTaxAmount,
                        postTaxAmount,
                        preTax_lowerLimit,
                        preTax_upperLimit,
                        postTax_lowerLimit,
                        postTax_upperLimit,
                        paymentContextCurrency,
                        paymentSessionCurrency,
                        paymentSession,
                        paymentContext,
                        paymentContextObj
                    };

                    SllWebLogger.TraceServerMessage("VerifyAdditionalSessionDataParams", piId, paymentSession.Id, JsonConvert.SerializeObject(totalValidationResults), EventLevel.Informational);
                }

                if ((decimal)paymentContextObj?.Pretax == 0 && (decimal)paymentContextObj?.PostTax == 0)
                {
                    // If both pre-tax and post-tax amounts are zero (ie. token redemption), we skip the amount verification
                    return VerificationResult.Success;
                }

                // Currently only requiring validation of the following - can update check with the above verifications when needed
                if (!currencyVerified)
                {
                    return VerificationResult.CurrencyMismatch;
                }
                else if (!(preTaxVarienceVerified || postTaxVarienceVerified))
                {
                    return VerificationResult.AmountMismatch;
                }

                return VerificationResult.Success;
            }
            catch (Exception ex)
            {
                throw new ValidationException(ErrorCode.InvalidRequestData, "incorrectly formatted paymentContext " + ex.Message);
            }
        }

        private static void SetupTestHeader(IFormCollection formData)
        {
            // check if the form data contains x-ms-test variable
            // Set the test header value to the current context through the HttpRequestHelper
            string testHeader = formData.TryGetValue(PaymentConstants.PaymentExtendedHttpHeaders.TestHeader, out var headerValue)
                ? headerValue.ToString()
                : null;
            if (!string.IsNullOrEmpty(testHeader))
            {
                var testHeaderData = ThreeDSUtils.DecodeBase64(ThreeDSUtils.DecodeUrl(testHeader));
                HttpRequestHelper.SetTestHeader(testHeaderData);
            }
        }

        private static bool IsBadPaymentChallengeStatus(PaymentChallengeStatus challengeStatus)
        {
            return challengeStatus == PaymentChallengeStatus.Failed || challengeStatus == PaymentChallengeStatus.InternalServerError || challengeStatus == PaymentChallengeStatus.TimedOut;
        }

        private static HttpResponseMessage ComposeHtmlPostMessageResponse(ClientAction clientAction)
        {
            string jsEncodedClientAction = JavaScriptEncoder.Default.Encode(JsonConvert.SerializeObject(clientAction)); string responseContent = string.Format(PostMessageHtmlTemplate, jsEncodedClientAction);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(responseContent);
            response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/html; charset=utf-8");
            return response;
        }

        private static ClientAction CreateFailureClientAction(HttpStatusCode statusCode, string errorCode, string errorMessage, string userMessage = null)
        {
            // special handling for iframe post message, 
            // if there is any failure when iframe talk to pifd/px, 
            // iframe should post error message back to pidlsdk and 
            // let pidlsdk propagate to partner in the standard way
            var errorResponse = new ServiceErrorResponse();
            errorResponse.ErrorCode = statusCode.ToString();
            errorResponse.ErrorCode = errorMessage;
            errorResponse.InnerError = new ServiceErrorResponse();
            errorResponse.InnerError.ErrorCode = errorCode;
            errorResponse.InnerError.Message = errorMessage;
            errorResponse.InnerError.UserDisplayMessage = userMessage;
            return new ClientAction(ClientActionType.Failure, errorResponse);
        }

        private static ServiceErrorResponse CreateFailureClientActionContext(HttpStatusCode statusCode, string errorCode, string errorMessage, string userMessage = null)
        {
            return new ServiceErrorResponse()
            {
                ErrorCode = statusCode.ToString(),
                Message = errorMessage,
                InnerError = new ServiceErrorResponse()
                {
                    ErrorCode = errorCode,
                    Message = errorMessage,
                    UserDisplayMessage = userMessage
                }
            };
        }

        private static bool IsBrowserAuthenticateThreeDSOneRedirectionUrlValid(string redirectionUrl)
        {
            try
            {
                Uri redirectionUri = new Uri(redirectionUrl);
                List<string> allowedHostnames = V7.Constants.AllowedOneBoxINTBrowserAuthenticateThreeDSOneRedirectionUrlHostname;
                if (Common.Environments.Environment.IsProdOrPPEEnvironment)
                {
                    allowedHostnames = V7.Constants.AllowedPPEPRODBrowserAuthenticateThreeDSOneRedirectionUrlHostname;
                }

                if (redirectionUri.Scheme != Uri.UriSchemeHttps)
                {
                    return false;
                }

                foreach (string allowedHostname in allowedHostnames)
                {
                    if (allowedHostname.Equals(redirectionUri.Host, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static PIDLResource IndiaThreeDSErrorHandler(PIDLResource threeDSRedirectPidl, TransactionResource transactionResource, out HttpStatusCode httpStatusCode, List<string> flights)
        {
            httpStatusCode = HttpStatusCode.OK;
            try
            {
                if (transactionResource != null && transactionResource.StatusDetails != null && transactionResource.StatusDetails.ProcessorResponse != null)
                {
                    // we can add more user errors which are returning as system error
                    string[] userErrors = { "Please enter a valid DEBIT card number.", "Invalid Expiry Date" };
                    string errorMsg = Convert.ToString(JObject.Parse(JsonConvert.SerializeObject(transactionResource.StatusDetails.ProcessorResponse))["error_message"]);
                    TransactionDeclineCode code = transactionResource.StatusDetails.Code;

                    if (userErrors.Contains(errorMsg, StringComparer.OrdinalIgnoreCase)
                        || ((flights?.Contains(PXCommon.Constants.FlightValues.PXEnableHandleTransactionNotAllowed) ?? false) && string.Equals(code, TransactionDeclineCode.TransactionNotAllowed)))
                    {
                        threeDSRedirectPidl.ClientAction = CreateFailureClientAction(HttpStatusCode.BadRequest, HttpStatusCode.BadRequest.ToString(), errorMsg);
                    }
                    else
                    {
                        threeDSRedirectPidl.ClientAction = CreateFailureClientAction(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError.ToString(), errorMsg);
                        httpStatusCode = HttpStatusCode.InternalServerError;
                    }
                }
                else
                {
                    threeDSRedirectPidl.ClientAction = CreateFailureClientAction(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError.ToString(), "Authentication failed");
                }
            }
            catch (Exception ex)
            {
                threeDSRedirectPidl.ClientAction = CreateFailureClientAction(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError.ToString(), "Authentication failed. " + ex.Message);
            }

            return threeDSRedirectPidl;
        }

        // this code needs to be refactored into the PaymentSessionHandler
        private static bool IsValidTransactionStatus(PXService.Model.PXInternal.PaymentSession paymentSession)
        {
            bool result = paymentSession.TransactionStatus != null;
            switch (paymentSession.TransactionStatus)
            {
                case PSD2TransStatus.C:
                case PSD2TransStatus.FR:
                case PSD2TransStatus.R:
                    result = false;

                    break;

                case PSD2TransStatus.N:
                    // TSR 10 is failure
                    result = !(PSD2TransStatusReason.TSR10 == paymentSession.TransactionStatusReason);

                    PSD2ChallengeIndicator cancelIndicator;
                    if (result && Enum.TryParse(paymentSession.ChallengeCancelIndicator, out cancelIndicator))
                    {
                        result = cancelIndicator == PSD2ChallengeIndicator.TransactionError || cancelIndicator == PSD2ChallengeIndicator.Unknown;
                    }

                    break;
            }

            return result;
        }

        private async Task<PimsModel.V4.PaymentInstrument> LookupPaymentInstrument(string accountId, string piId, bool extendedView, EventTraceActivity traceActivityId)
        {
            PimsModel.V4.PaymentInstrument paymentInstrument = null;
            try
            {
                if (extendedView)
                {
                    paymentInstrument = await this.Settings.PIMSAccessor.GetExtendedPaymentInstrument(
                        piid: piId,
                        traceActivityId: traceActivityId,
                        exposedFlightFeatures: this.ExposedFlightFeatures);
                }
                else
                {
                    paymentInstrument = await this.Settings.PIMSAccessor.GetPaymentInstrument(
                        accountId: accountId,
                        piid: piId,
                        traceActivityId: traceActivityId);
                }
            }
            catch (ServiceErrorResponseException ex)
            {
                if (ex.Response.StatusCode != HttpStatusCode.NotFound &&
                    ex.Error?.ErrorCode != "AccountPINotFound")
                {
                    throw;
                }
            }

            return paymentInstrument;
        }

        private HttpResponseMessage BuildSuccessResponse(bool overrideVerification, string sessionId, string piId, bool verified, PaymentChallengeStatus challengeStatus, VerificationResult verificationResult = VerificationResult.Success)
        {
            var authStatus = new AuthenticationStatus()
            {
                Verified = verified | overrideVerification,
                PiId = piId,
                SessionId = sessionId,
                ChallengeStatus = overrideVerification && !verified ? PaymentChallengeStatus.NotApplicable : challengeStatus,
            };

            if (verificationResult != VerificationResult.Success)
            {
                authStatus.FailureReason = verificationResult;
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, authStatus);
        }

        private HttpResponseMessage BuildFailureResponse(bool overrideResponse, string piid, string sessionId)
        {
            if (overrideResponse)
            {
                var authStatus = new AuthenticationStatus()
                {
                    Verified = true,
                    PiId = piid,
                    SessionId = sessionId,
                    ChallengeStatus = PaymentChallengeStatus.NotApplicable
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, authStatus);
            }

            return this.Request.CreateResponse(HttpStatusCode.NotFound);
        }
    }
}