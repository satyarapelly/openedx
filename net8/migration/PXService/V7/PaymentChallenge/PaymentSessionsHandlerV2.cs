// <copyright file="PaymentSessionsHandlerV2.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics.Tracing;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using Common.Environments;
    using Common.Transaction;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.Pidl.Localization;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Accessors.TransactionDataService;
    using Microsoft.Commerce.Payments.PXService.Model.ThreeDSExternalService;
    using Microsoft.Commerce.Payments.PXService.Model.TransactionService;
    using Model;
    using Newtonsoft.Json;
    using PayerAuth = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService;
    using PXInternal = Microsoft.Commerce.Payments.PXService.Model.PXInternal;

    // Refactored from PaymentSessionsHandler
    public class PaymentSessionsHandlerV2 : PaymentSessionsHandler
    {
        public static readonly new string HandlerVersion = "V2";

        private const string ExceptionMessage = "Exception in PaymentSessionsHandlerV2: {0}";

        private static readonly Dictionary<ChallengeWindowSize, WindowSize> ChallengeWindowSizes = new Dictionary<ChallengeWindowSize, WindowSize>()
        {
            { ChallengeWindowSize.One, new WindowSize() { Width = "250px", Height = "400px" } },
            { ChallengeWindowSize.Two, new WindowSize() { Width = "390px", Height = "400px" } },
            { ChallengeWindowSize.Three, new WindowSize() { Width = "500px", Height = "600px" } },
            { ChallengeWindowSize.Four, new WindowSize() { Width = "600px", Height = "400px" } },
            { ChallengeWindowSize.Five, new WindowSize() { Width = "100%", Height = "100%" } },
        };

        private PXInternal.PaymentSession storedSession = null;

        public PaymentSessionsHandlerV2(
            IPayerAuthServiceAccessor payerAuthServiceAccessor,
            IPIMSAccessor pimsAccessor,
            ISessionServiceAccessor sessionServiceAccessor,
            IAccountServiceAccessor accountServiceAccessor,
            IPurchaseServiceAccessor purchaseServiceAccessor,
            ITransactionServiceAccessor transactionServiceAccessor,
            ITransactionDataServiceAccessor transactionDataServiceAccessor,
            string pifdBaseUrl) :
            base(
                payerAuthServiceAccessor,
                pimsAccessor,
                sessionServiceAccessor,
                accountServiceAccessor,
                purchaseServiceAccessor,
                transactionServiceAccessor,
                transactionDataServiceAccessor,
                pifdBaseUrl)
        {
        }

        public static new void ValidateSettingsVersion(AuthenticationRequest authRequest, List<string> exposedFlightFeatures)
        {
            string targetSettingsVersion = GetLatestAvailablePSD2SettingVersion(exposedFlightFeatures);

            // On 2nd or subsequent TryCount, dont throw this exception even if there is a mismatch. This is to allow
            // the client to attempt Authentication if it fails to download the target settings version for some reason.
            if (string.Equals(targetSettingsVersion, authRequest.SettingsVersion, StringComparison.OrdinalIgnoreCase) == false
                && authRequest.SettingsVersionTryCount == 1)
            {
                throw new ValidationException(ErrorCode.SettingsVersionMismatch, targetSettingsVersion, string.Empty);
            }
        }

        public static new Uri GetChallengeRedirectUriFromPaymentSession(PaymentSession paymentSession)
        {
            string url = (paymentSession.ChallengeStatus == PaymentChallengeStatus.Succeeded) ? paymentSession.SuccessUrl : paymentSession.FailureUrl;

            UriBuilder builder = new UriBuilder(new Uri(url));
            NameValueCollection query = HttpUtility.ParseQueryString(builder.Query);

            query["challengeStatus"] = paymentSession.ChallengeStatus.ToString();

            if (paymentSession.ChallengeStatus == PaymentChallengeStatus.Succeeded)
            {
                query["sessionId"] = paymentSession.Id;
                query["piid"] = paymentSession.PaymentInstrumentId;
            }
            else
            {
                query["errorCode"] = V7.Constants.PSD2ErrorCodes.RejectedByProvider;
                query["errorMessage"] = paymentSession.ChallengeStatus.ToString();

                if (paymentSession.ChallengeStatus == PaymentChallengeStatus.InternalServerError)
                {
                    query["errorCode"] = V7.Constants.ThreeDSErrorCodes.InternalServerError;
                }

                if (!string.IsNullOrEmpty(paymentSession.UserDisplayMessage))
                {
                    query["userDisplayMessage"] = paymentSession.UserDisplayMessage;
                }
            }

            builder.Query = query.ToString();
            return builder.Uri;
        }

        public static new string GetTransactionServiceStore(string partner, PaymentExperienceSetting setting)
        {
            string store = null;
            if (setting != null)
            {
                if (PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseOmsTransactionServiceStore, null, setting))
                {
                    store = V7.Constants.TransationServiceStore.OMS;
                }
                else if (PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseAzureTransactionServiceStore, null, setting))
                {
                    store = V7.Constants.TransationServiceStore.Azure;
                }
            }

            if (string.IsNullOrEmpty(store))
            {
                if (PartnerHelper.IsAzurePartner(partner))
                {
                    store = V7.Constants.TransationServiceStore.Azure;
                }
                else if (string.Equals(V7.Constants.PartnerName.CommercialStores, partner, StringComparison.OrdinalIgnoreCase))
                {
                    store = V7.Constants.TransationServiceStore.OMS;
                }
            }

            return store ?? V7.Constants.TransationServiceStore.Azure;
        }

        /// <summary>
        /// Used by both Browser flow and App flow
        /// Create Payment Session Object
        /// </summary>
        /// <param name="accountId">Pi Account Id</param>
        /// <param name="paymentSessionData">The context to create PaymentSession object</param>
        /// <param name="deviceChannel">Sepcifies whether this call is from the App flow or the browser flow</param>
        /// <param name="emailAddress">Used for risk assessment during ValidatePI call</param>
        /// <param name="exposedFlightFeatures">Exposed flight features</param>'
        /// <param name="traceActivityId">Event trace active id for logging</param>
        /// <param name="testContext">Test Context</param>
        /// <param name="isMotoAuthorized">Whether the request is authorized for moto scenario</param>
        /// <param name="tid">Org tid</param>
        /// <param name="setting">payment experience partner setting</param>
        /// <param name="userId">Incoming user PUID</param>
        /// <param name="isGuestUser">Guest user</param>
        /// <param name="requestContext"> for payment or checkout request context</param>
        /// <throws>HttpResponseException if the payment instrument cannot be found</throws>
        /// <returns>Returns the data ThreeDSMethodPreparationResult for PIDL to consume </returns>
        public override async Task<PaymentSession> CreatePaymentSession(
            string accountId,
            PaymentSessionData paymentSessionData,
            DeviceChannel deviceChannel,
            string emailAddress,
            List<string> exposedFlightFeatures,
            EventTraceActivity traceActivityId,
            TestContext testContext,
            string isMotoAuthorized = null,
            string tid = null,
            PaymentExperienceSetting setting = null,
            string userId = null,
            bool isGuestUser = false,
            V7.Contexts.RequestContext requestContext = null)
        {
            try
            {
                if (requestContext != null)
                {
                    return await this.InternalCreatePaymentSessionWithRequestId(
                                    accountId: accountId,
                                    paymentSessionData: paymentSessionData,
                                    deviceChannel: deviceChannel,
                                    emailAddress: emailAddress,
                                    exposedFlightFeatures: exposedFlightFeatures,
                                    traceActivityId: traceActivityId,
                                    testContext: testContext,
                                    isMotoAuthorized: isMotoAuthorized,
                                    tid: tid,
                                    setting: setting,
                                    userId: userId,
                                    isGuestUser: isGuestUser,
                                    requestContext: requestContext);
                }
                else
                {
                    return await this.InternalCreatePaymentSession(
                                    accountId: accountId,
                                    paymentSessionData: paymentSessionData,
                                    deviceChannel: deviceChannel,
                                    emailAddress: emailAddress,
                                    exposedFlightFeatures: exposedFlightFeatures,
                                    traceActivityId: traceActivityId,
                                    testContext: testContext,
                                    isMotoAuthorized: isMotoAuthorized,
                                    tid: tid,
                                    setting: setting,
                                    userId: userId,
                                    isGuestUser: isGuestUser);
                }
            }
            catch (ValidationException ex)
            {
                SllWebLogger.TracePXServiceException(string.Format(ExceptionMessage, ex.ToString()), traceActivityId);

                throw ex;
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException(string.Format(ExceptionMessage, ex.ToString()), traceActivityId);

                return GetSafetyNetPaymentSession(paymentSessionData);
            }
        }

        /// <summary>
        /// Used by browser flow
        /// 1. Gather Information to generate ThreeDSMethod form (fingerprinting)
        /// 2. Store Information to the Session which required in the later ThreeDS call
        /// </summary>
        /// <param name="accountId">The account Id of PI</param>
        /// <param name="browserInfo">The browser info, collected from user request, need store in the session and used in the later call</param>
        /// <param name="paymentSession">The serialized purchase context, need store in the session and used in the later call</param>
        /// <param name="traceActivityId">Event trace active id for logging</param>
        /// <param name="exposedFlightFeatures">Exposed flights</param>
        /// <throws>HttpResponseException if the payment instrument cannot be found</throws>
        /// <returns>Returns the data BrowserFlowContext for PIDL to consume </returns>
        public override async Task<BrowserFlowContext> GetThreeDSMethodURL(
            string accountId,
            PayerAuth.BrowserInfo browserInfo,
            PaymentSession paymentSession,
            EventTraceActivity traceActivityId,
            List<string> exposedFlightFeatures = null)
        {
            try
            {
                PayerAuth.ThreeDSMethodData methodData = null;

                // 1. Get the stored session
                await this.CachedGetStoredSession(
                    sessionId: paymentSession.Id,
                    traceActivityId: traceActivityId);

                // 2. Call PayerAuth.Get3DSMethodUrl
                methodData = await this.PayerAuthServiceAccessor.Get3DSMethodURL(
                    paymentSession: new PayerAuth.PaymentSession(this.storedSession),
                    traceActivityId: traceActivityId);

                // 3. Add browserInfo and methodData to the stored session
                this.storedSession.BrowserInfo = browserInfo;
                this.storedSession.MethodData = methodData;

                // 4. If fingerprinting was skipped by ACS, call PayerAuth.Authenticate
                bool skipFingerprint = exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXPSD2SkipFingerprint, StringComparer.OrdinalIgnoreCase);
                if (string.IsNullOrEmpty(methodData.ThreeDSMethodURL) || skipFingerprint)
                {
                    return await this.Authenticate(
                        threeDSMethodCompletionIndicator: (skipFingerprint && !string.IsNullOrEmpty(methodData.ThreeDSMethodURL)) ? PayerAuth.ThreeDSMethodCompletionIndicator.N : PayerAuth.ThreeDSMethodCompletionIndicator.U,
                        exposedFlightFeatures: this.storedSession.ExposedFlightFeatures,
                        traceActivityId: traceActivityId,
                        piQueryParams: GetPiQueryParams(
                            billableAccountId: this.storedSession.BillableAccountId,
                            classicProduct: this.storedSession.ClassicProduct,
                            partner: this.storedSession.Partner));
                }

                PXService.Model.ThreeDSExternalService.ThreeDSMethodData formInputThreeDSMethod = new PXService.Model.ThreeDSExternalService.ThreeDSMethodData()
                {
                    ThreeDSServerTransID = methodData.ThreeDSServerTransID,
                    ThreeDSMethodNotificationURL = string.Format("{0}/paymentSessions/{1}/authenticate", this.PifdBaseUrl, paymentSession.Id),
                };

                return new BrowserFlowContext
                {
                    IsFingerPrintRequired = true,
                    FormActionURL = methodData.ThreeDSMethodURL,
                    FormInputThreeDSMethodData = ThreeDSUtils.EncodeUrl(ThreeDSUtils.EncodeObjectToBase64(formInputThreeDSMethod))
                };
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException(string.Format(ExceptionMessage, ex.ToString()), traceActivityId);

                var context = GetSafetyNetBrowserFlowContext(paymentSession);

                if (this.storedSession != null)
                {
                    this.storedSession.IsSystemError = true;
                    this.storedSession.ChallengeStatus = context.PaymentSession.ChallengeStatus;

                    if (this.storedSession.ChallengeStatus == PaymentChallengeStatus.Succeeded)
                    {
                        await this.PostProcessOnSuccess(
                        traceActivityId: traceActivityId,
                        piQueryParams: GetPiQueryParams(
                            billableAccountId: this.storedSession.BillableAccountId,
                            classicProduct: this.storedSession.ClassicProduct,
                            partner: this.storedSession.Partner));
                    }
                }

                return context;
            }
            finally
            {
                if (this.storedSession != null)
                {
                    bool authenticationVerified = this.storedSession.ChallengeStatus == PaymentChallengeStatus.Succeeded;
                    await this.UpdateSessionOnFinally(paymentSession.Id, accountId, authenticationVerified, traceActivityId);
                }
            }
        }

        public override async Task<BrowserFlowContext> AuthenticateUpiPaymentTxn(
            string accountId,
            PayerAuth.BrowserInfo browserInfo,
            PaymentSession paymentSession,
            EventTraceActivity traceActivityId,
            List<string> exposedFlightFeatures = null)
        {
            if (await CallSafetyNetOperation(
                operation: async () =>
                {
                    await this.CachedGetStoredSession(
                        sessionId: paymentSession.Id,
                        traceActivityId: traceActivityId);
                },
                traceActivityId))
            {
                return GetSafetyNetBrowserFlowContext(paymentSession);
            }

            PaymentInstrument paymentInstrument = null;

            try
            {
                paymentInstrument = await this.PimsAccessor.GetPaymentInstrument(
                       accountId: accountId,
                       piid: paymentSession.PaymentInstrumentId,
                       traceActivityId: traceActivityId);
            }
            catch (ServiceErrorResponseException ex)
            {
                if (ex.Error?.ErrorCode == "AccountPINotFound")
                {
                    throw new ValidationException(ErrorCode.PaymentInstrumentNotFound, "Caller is not authorized to access specified PaymentInstrumentId.");
                }

                throw;
            }

            if (paymentInstrument == null || (!paymentInstrument.PaymentMethod.IsUpi() && !paymentInstrument.PaymentMethod.IsUpiQr()))
            {
                throw new ValidationException(ErrorCode.InvalidPaymentInstrumentDetails, "Provided Payment Instrument is not a valid UPI PI.");
            }

            PayerAuth.AuthenticationRequest authRequest = new PayerAuth.AuthenticationRequest
            {
                PaymentSession = this.storedSession,
                BrowserInfo = browserInfo
            };

            PayerAuth.AuthenticationResponse resp = null;
            await CallSafetyNetOperation(
                async () =>
                {
                    resp = await this.PayerAuthServiceAccessor.Authenticate(authRequest, traceActivityId);
                },
                traceActivityId,
                exposedFeatures: null,
                excludeErrorFeatureFormat: "PSD2SafetyNet-MotoAuthN-{0}-{1}");

            paymentSession.ChallengeStatus = resp.EnrollmentStatus == PayerAuth.PaymentInstrumentEnrollmentStatus.Bypassed ? PaymentChallengeStatus.ByPassed : PaymentChallengeStatus.Failed;

            return new BrowserFlowContext()
            {
                PaymentSession = paymentSession,
            };
        }

        /// <summary>
        /// Used by Browser Flow, after users submit ThreeDSMethod (Fingerprinting) form to 3DS Server,
        /// the users will call the API to notify us the ThreeDSMethod is completed
        /// We will contact the underline service to check either user need to be challenged or not
        /// and return the next step to the user.
        /// </summary>
        /// <param name="sessionId">Identity the user's purchase session</param>
        /// <param name="isThreeDSMethodCompleted">boolean value indicating whether 3ds method completed successfully or not</param>
        /// <param name="traceActivityId">Event trace active id for logging</param>
        /// <throws>HttpResponseException if the payment instrument cannot be found</throws>
        /// <returns>Returns the data BrowserFlowContext for PIDL to consume </returns>
        public override async Task<BrowserFlowContext> Authenticate(
            string sessionId,
            bool isThreeDSMethodCompleted,
            EventTraceActivity traceActivityId)
        {
            try
            {
                // 1. Get stored session
                await this.CachedGetStoredSession(
                    sessionId: sessionId,
                    traceActivityId: traceActivityId);

                // 2. Call Authenticate
                return await this.Authenticate(
                    threeDSMethodCompletionIndicator: isThreeDSMethodCompleted ? PayerAuth.ThreeDSMethodCompletionIndicator.Y : PayerAuth.ThreeDSMethodCompletionIndicator.N,
                    traceActivityId: traceActivityId,
                    exposedFlightFeatures: this.storedSession.ExposedFlightFeatures,
                    piQueryParams: GetPiQueryParams(
                        billableAccountId: this.storedSession.BillableAccountId,
                        classicProduct: this.storedSession.ClassicProduct,
                        partner: this.storedSession.Partner));
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException(string.Format(ExceptionMessage, ex.ToString()), traceActivityId);

                if (this.storedSession != null)
                {
                    this.storedSession.IsSystemError = true;
                    var ps = new PaymentSession(this.storedSession);
                    ps.Signature = ps.GenerateSignature();
                    var context = GetSafetyNetBrowserFlowContext(ps);
                    this.storedSession.ChallengeStatus = context.PaymentSession.ChallengeStatus;

                    if (this.storedSession.ChallengeStatus == PaymentChallengeStatus.Succeeded)
                    {
                        await this.PostProcessOnSuccess(
                        traceActivityId: traceActivityId,
                        piQueryParams: GetPiQueryParams(
                            billableAccountId: this.storedSession.BillableAccountId,
                            classicProduct: this.storedSession.ClassicProduct,
                            partner: this.storedSession.Partner));
                    }

                    return context;
                }
                else
                {
                    return GetSafetyNetBrowserFlowContext(GetSafetyNetPaymentSession(sessionId));
                }
            }
            finally
            {
                if (this.storedSession != null)
                {
                    await this.SafetyNetUpdateSession(traceActivityId);
                }
            }
        }

        /// <summary>
        /// Used by Browser Flow, after users provide CVV in 3DS 1.0 authentication flow
        /// </summary>
        /// <param name="sessionId">Identity the user's purchase session</param>
        /// <param name="cvvToken">CVV token</param>
        /// <param name="traceActivityId">Event trace active id for logging</param>
        /// <param name="userId">puid from x-ms-msaprofile header</param>
        /// <throws>HttpResponseException if the payment instrument cannot be found</throws>
        /// <returns>Returns the data BrowserFlowContext for PIDL to consume </returns>
        public override async Task<BrowserFlowContext> AuthenticateThreeDSOne(
            string sessionId,
            string cvvToken,
            EventTraceActivity traceActivityId,
            string userId)
        {
            string paymentSessionId = string.Empty;
            try
            {
                // 1. Get stored session
                await this.CachedGetStoredSession(
                        sessionId: sessionId,
                        traceActivityId: traceActivityId);

                // 2. Call AuthenticateThreeDSOne
                return await this.AuthenticateThreeDSOne(
                                cvvToken: cvvToken,
                                piQueryParams: GetPiQueryParams(
                                    billableAccountId: this.storedSession.BillableAccountId,
                                    classicProduct: this.storedSession.ClassicProduct,
                                    partner: this.storedSession.Partner),
                                traceActivityId: traceActivityId,
                                userId: userId);
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException(string.Format(ExceptionMessage, ex.ToString()), traceActivityId);

                if (this.storedSession != null)
                {
                    var ps = new PaymentSession(this.storedSession);
                    paymentSessionId = ps.Id;
                    this.storedSession.IsSystemError = true;
                    await this.SafetyNetUpdateSession(traceActivityId);
                    ps.Signature = ps.GenerateSignature();
                    List<string> exposedFlightFeatures = await this.GetExposedFlightFeatures(ps.Id, traceActivityId);
                    return GetSafetyNetBrowserFlowContext(ps, exposedFlightFeatures);
                }
                else
                {
                    return GetSafetyNetBrowserFlowContext(GetSafetyNetPaymentSession(sessionId));
                }
            }
            finally
            {
                if (this.storedSession != null)
                {
                    bool authenticationVerified = PaymentSessionsHandler.IsAuthenticationVerified(this.storedSession.ChallengeStatus) || this.storedSession.IsSystemError;
                    await this.UpdateSessionOnFinally(paymentSessionId, null, authenticationVerified, traceActivityId);
                }
            }
        }

        public override async Task<BrowserFlowContext> AuthenticateRedirectionThreeDSOne(
            string sessionId,
            string successUrl,
            string failureUrl,
            EventTraceActivity traceActivityId)
        {
            try
            {
                // 1. Get stored session
                await this.CachedGetStoredSession(
                        sessionId: sessionId,
                        traceActivityId: traceActivityId);

                PaymentSession paymentSession = new PaymentSession(this.storedSession);
                paymentSession.Signature = paymentSession.GenerateSignature();

                // 2. Update stored session with ru and rx
                this.storedSession.SuccessUrl = successUrl;
                this.storedSession.FailureUrl = failureUrl;
                await this.SessionServiceAccessor.UpdateSessionResourceData<PXInternal.PaymentSession>(
                    sessionId: this.storedSession.Id,
                    newSessionData: this.storedSession,
                    traceActivityId: traceActivityId);

                return new BrowserFlowContext()
                {
                    IsAcsChallengeRequired = true,
                    FormInputCReq = this.storedSession.AuthenticationResponse.AcsSignedContent,
                    FormActionURL = this.storedSession.AuthenticationResponse.AcsUrl,
                    FormPostAcsURL = this.storedSession.AuthenticationResponse.IsFormPostAcsUrl,
                    FormFullPageRedirectAcsURL = this.storedSession.AuthenticationResponse.IsFullPageRedirect,
                    CardHolderInfo = this.storedSession.AuthenticationResponse.CardHolderInfo,
                    PaymentSession = paymentSession
                };
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException(string.Format(ExceptionMessage, ex.ToString()), traceActivityId);

                return GetSafetyNetBrowserFlowContext(GetSafetyNetPaymentSession(sessionId));
            }
        }

        /// <summary>
        /// Used by App Flow,
        /// the users will call the API to authenticate
        /// and return the next step to the user.
        /// </summary>
        /// <param name="accountId">Identify the user by account Id</param>
        /// <param name="sessionId">Identify the sessionId</param>
        /// <param name="authRequest">Authenticate request payload sent by user</param>
        /// <param name="exposedFlightFeatures">Exposed flight features</param>
        /// <param name="traceActivityId">Event trace active id for logging</param>
        /// <param name="testContext">Test Context</param>
        /// <throws>HttpResponseException if the payment instrument cannot be found</throws>
        /// <returns>Returns the data AuthenticationResponse for PIDL to consume </returns>
        public override async Task<AuthenticationResponse> Authenticate(
            string accountId,
            string sessionId,
            AuthenticationRequest authRequest,
            List<string> exposedFlightFeatures,
            EventTraceActivity traceActivityId,
            TestContext testContext)
        {
            bool authenticationVerified = true;

            try
            {
                await this.CachedGetStoredSession(
                    sessionId: sessionId,
                    traceActivityId: traceActivityId);

                if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXAuthenticateChallengeTypeOnStoredSession, StringComparer.OrdinalIgnoreCase)
                    && this.storedSession.ChallengeType == V7.Constants.ChallengeTypes.ValidatePIOnAttachChallenge)
                {
                    return GetSafetyNetAuthenticationResponse();
                }

                // Set the device channel to App. Webblends calls createPaymentChallenge from the browser
                // and if challenge is needed, store calls handlePaymentChallenge. So, here set the device channel
                // to App.
                this.storedSession.DeviceChannel = DeviceChannel.AppBased;

                // 2. Convert PX's AuthRequest object to PayerAuth's AuthRequest object
                var payerAuthRequest = new PayerAuth.AuthenticationRequest(
                    paymentSession: new PayerAuth.PaymentSession(this.storedSession),
                    authRequest: authRequest);

                if (string.IsNullOrEmpty(payerAuthRequest.MessageVersion))
                {
                    payerAuthRequest.MessageVersion = GlobalConstants.PSD2Constants.FallbackMessageVersion;
                }

                PayerAuth.AuthenticationResponse payerAuthResponse = null;

                if (exposedFlightFeatures.Contains(Flighting.Features.PXPSD2EnforcePreferredChallengeIndicator))
                {
                    payerAuthRequest.RiskChallengIndicator = PayerAuth.RiskChallengeIndicator.ChallengeRequestedPreference;
                }

                payerAuthResponse = await this.PayerAuthServiceAccessor.Authenticate(
                                authRequest: payerAuthRequest,
                                traceActivityId: traceActivityId);
                this.storedSession.TransactionStatus = payerAuthResponse.TransactionStatus;
                this.storedSession.TransactionStatusReason = payerAuthResponse.TransactionStatusReason;

                // TODO : Remove after Fraud Investigation
                SllWebLogger.TraceServerMessage("Authenticate App", null, sessionId, payerAuthResponse?.TransactionStatus != null ? "TransStatus: " + payerAuthResponse.TransactionStatus.ToString() : "No transStatus available", EventLevel.Informational);

                var mappedStatus = GetMappedStatusForAuthentication(
                    transStatus: payerAuthResponse.TransactionStatus,
                    isMoto: this.storedSession.IsMOTO,
                    additionalInputs: new List<string>() { payerAuthResponse.TransactionStatusReason.ToString() },
                    exposedFlights: this.storedSession.ExposedFlightFeatures);

                // TODO : Remove after Fraud Investigation
                SllWebLogger.TraceServerMessage("Authenticate App", null, sessionId, "Mapped Status1: " + mappedStatus.ToString(), EventLevel.Informational);

                // Localized text to display in PSD2 app native challenges
                var localizations = GetPSD2NativeChallengeLocalizations(authRequest.Language);

                if (mappedStatus == PaymentChallengeStatus.Unknown && exposedFlightFeatures.Contains(Flighting.Features.PXEnablePSD2ServiceSideCertificateValidation))
                {
                    // ACSSignedContent will be assigned to the AuthenticationResponse
                    // only if the PaymentChallengeStatus is Unknown,
                    // therefore further checks for validity are required.
                    // TODO (54267708):  Ask Ravi about traffic going here.  If it's healthy, we need to refactor this and remove the flight requirement.
                    mappedStatus = this.ValidateACSSignedContent(authRequest, mappedStatus, exposedFlightFeatures, payerAuthResponse, testContext, traceActivityId, sessionId);
                }

                // TODO : Remove after Fraud Investigation
                SllWebLogger.TraceServerMessage("Authenticate App", null, sessionId, "Mapped Status2: " + mappedStatus.ToString(), EventLevel.Informational);

                authenticationVerified = PaymentSessionsHandler.IsAuthenticationVerified(mappedStatus);

                // TODO : Remove after Fraud Investigation
                SllWebLogger.TraceServerMessage("Authenticate App", null, sessionId, "AuthenticationVerified: " + authenticationVerified.ToString(), EventLevel.Informational);

                // 5. Update stored session with AuthResponse. This will be used when calling CompleteChallenge
                this.storedSession.AuthenticationResponse = payerAuthResponse;
                this.storedSession.ChallengeStatus = mappedStatus;

                // 6. Covert PayerAuth's AuthResponse to PX's AuthResponse and return
                return new AuthenticationResponse()
                {
                    AcsTransactionID = payerAuthResponse.AcsTransactionId,
                    AcsSignedContent = payerAuthResponse.AcsSignedContent,
                    ThreeDSServerTransactionID = payerAuthResponse.ThreeDSServerTransactionId,
                    EnrollmentStatus = payerAuthResponse.EnrollmentStatus,
                    ChallengeStatus = mappedStatus,
                    AcsRenderingType = new AcsRenderingType(payerAuthResponse.AcsRenderingType),
                    CardHolderInfo = payerAuthResponse.CardHolderInfo,
                    AuthenticationType = payerAuthResponse.AuthenticationType,
                    AcsChallengeMandated = payerAuthResponse.AcsChallengeMandated,
                    AcsReferenceNumber = payerAuthResponse.AcsReferenceNumber,
                    DsReferenceNumber = payerAuthResponse.DsReferenceNumber,
                    AcsOperatorID = payerAuthResponse.AcsOperatorID,
                    MessageVersion = payerAuthResponse.MessageVersion ?? GlobalConstants.PSD2Constants.FallbackMessageVersion,
                    DisplayStrings = localizations
                };
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException(string.Format(ExceptionMessage, ex.ToString()), traceActivityId);

                if (this.storedSession != null)
                {
                    this.storedSession.IsSystemError = true;
                    this.storedSession.ChallengeStatus = PaymentChallengeStatus.Succeeded;
                }

                // TODO : Remove after Fraud Investigation
                SllWebLogger.TraceServerMessage("Authenticate App", null, sessionId, this.storedSession != null ? "Failure, storedSession: " + JsonConvert.SerializeObject(this.storedSession) : "No stored session", EventLevel.Informational);

                // add trace logging
                return GetSafetyNetAuthenticationResponse();
            }
            finally
            {
                // TODO : Remove after Fraud Investigation
                SllWebLogger.TraceServerMessage("Authenticate App", null, sessionId, this.storedSession != null ? "storedSession: " + JsonConvert.SerializeObject(this.storedSession) : "No stored session", EventLevel.Informational);

                await this.UpdateSessionOnFinally(sessionId, accountId, authenticationVerified, traceActivityId);
            }
        }

        /// <summary>
        /// Used by Browser and App Flow, after users submit ThreeDSChallenge to ACS,
        /// the users will call the API to notify us the ThreeDSChallenge is completed
        /// We will contact the underline service to fetch result
        /// </summary>
        /// <param name="accountId">Identity the user by accountId</param>
        /// <param name="sessionId">Identity the user's purchase session</param>
        /// <param name="exposedFlightFeatures">Exposed flight features</param>
        /// <param name="traceActivityId">Event trace active id for logging</param>
        /// <returns>Returns the data PaymentSession for PIDL to consume </returns>
        public override async Task<PaymentSession> CompleteThreeDSChallenge(
            string accountId,
            string sessionId,
            List<string> exposedFlightFeatures,
            EventTraceActivity traceActivityId)
        {
            bool authenticationVerified = false;

            try
            {
                // 1. Get stored session
                await this.CachedGetStoredSession(
                    sessionId: sessionId,
                    traceActivityId: traceActivityId);

                // 2. Create a CompletionRequest object
                var completionRequest = new PayerAuth.CompletionRequest()
                {
                    PaymentSession = new PayerAuth.PaymentSession(this.storedSession),
                    AuthenticationContext = this.storedSession.AuthenticationResponse
                };

                // 3. Call PayerAuth.CompleteChallenge
                PayerAuth.CompletionResponse completionResponse = null;
                if (await CallSafetyNetOperation(
                        async () =>
                        {
                            completionResponse = await this.PayerAuthServiceAccessor.CompleteChallenge(
                               completionRequest: completionRequest,
                               traceActivityId: traceActivityId);

                            this.storedSession.TransactionStatus = completionResponse.TransactionStatus;
                            this.storedSession.TransactionStatusReason = completionResponse.TransactionStatusReason;
                            this.storedSession.ChallengeCancelIndicator = completionResponse.ChallengeCancelIndicator;
                            await this.SafetyNetUpdateSession(traceActivityId);
                        },
                        traceActivityId,
                        exposedFeatures: this.storedSession.ExposedFlightFeatures,
                        excludeErrorFeatureFormat: "PSD2SafetyNet-Completion-{0}-{1}"))
                {
                    this.storedSession.IsSystemError = true;
                    completionResponse = GetSafetyNetCompletionResponse();
                }

                // 4. Convert PayerAuth's PaymentSession object to a PX PaymentSession
                var paymentSession = new PXService.V7.PaymentChallenge.Model.PaymentSession(this.storedSession);
                paymentSession.Signature = paymentSession.GenerateSignature();

                // 5. Update PaymentSession object and return
                var mappedStatusString = GetMappedStatusString(
                    mapPrefix: "PXPSD2Comp",
                    transStatus: completionResponse.TransactionStatus,
                    isMoto: this.storedSession.IsMOTO,
                    additionalInputs: new List<string>() { completionResponse.TransactionStatusReason.ToString(), completionResponse.ChallengeCancelIndicator },
                    exposedFlights: this.storedSession.ExposedFlightFeatures);

                var mappedStatus = PaymentChallengeStatus.Succeeded;
                if (mappedStatusString != null && Enum.TryParse<PaymentChallengeStatus>(mappedStatusString, true, out mappedStatus))
                {
                    paymentSession.ChallengeStatus = mappedStatus;
                    this.storedSession.ChallengeStatus = mappedStatus;
                }
                else
                {
                    switch (completionResponse.TransactionStatus)
                    {
                        case PayerAuth.TransactionStatus.FR:
                        case PayerAuth.TransactionStatus.R:
                            paymentSession.ChallengeStatus = PaymentChallengeStatus.Failed;
                            break;

                        case PayerAuth.TransactionStatus.N:
                            paymentSession.ChallengeStatus = PaymentChallengeStatus.Succeeded;
                            if (!string.IsNullOrWhiteSpace(completionResponse.ChallengeCancelIndicator))
                            {
                                PayerAuth.ChallengeCancelIndicator cancelIndicator;
                                if (Enum.TryParse(completionResponse.ChallengeCancelIndicator, out cancelIndicator))
                                {
                                    switch (cancelIndicator)
                                    {
                                        case PayerAuth.ChallengeCancelIndicator.TransactionCReqTimedOut:
                                        case PayerAuth.ChallengeCancelIndicator.TransactionTimedOut:
                                            paymentSession.ChallengeStatus = PaymentChallengeStatus.TimedOut;
                                            break;

                                        case PayerAuth.ChallengeCancelIndicator.CancelledByCardHolder:
                                        case PayerAuth.ChallengeCancelIndicator.CancelledByRequestor:
                                        case PayerAuth.ChallengeCancelIndicator.TransactionAbandoned:
                                            paymentSession.ChallengeStatus = PaymentChallengeStatus.Cancelled;
                                            break;
                                    }
                                }
                            }
                            else if (completionResponse.TransactionStatusReason == PayerAuth.TransactionStatusReason.TSR14)
                            {
                                paymentSession.ChallengeStatus = PaymentChallengeStatus.TimedOut;
                            }

                            break;
                        default:
                            paymentSession.ChallengeStatus = PaymentChallengeStatus.Succeeded;
                            break;
                    }

                    this.storedSession.ChallengeStatus = paymentSession.ChallengeStatus;
                }

                authenticationVerified = PaymentSessionsHandler.IsAuthenticationVerified(paymentSession.ChallengeStatus);

                // BrowserNotifyThreeDSChallengeCompleted passes a null accountId.  Retrieve value from storedSession otherwise attestation will not execute.
                if (string.IsNullOrEmpty(accountId) && this.storedSession != null)
                {
                    if (!string.IsNullOrWhiteSpace(this.storedSession.AccountId))
                    {
                        accountId = this.storedSession.AccountId;
                    }
                    else if (!string.IsNullOrWhiteSpace(this.storedSession.PaymentInstrumentAccountId))
                    {
                        accountId = this.storedSession.PaymentInstrumentAccountId;
                    }
                    else if (!string.IsNullOrWhiteSpace(this.storedSession.BillableAccountId))
                    {
                        accountId = this.storedSession.BillableAccountId;
                    }
                    else if (!string.IsNullOrWhiteSpace(this.storedSession.CommercialAccountId))
                    {
                        accountId = this.storedSession.CommercialAccountId;
                    }
                }

                return paymentSession;
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException(string.Format(ExceptionMessage, ex.ToString()), traceActivityId);

                if (this.storedSession != null)
                {
                    this.storedSession.IsSystemError = true;
                }

                return GetSafetyNetPaymentSession(sessionId);
            }
            finally
            {
                await this.UpdateSessionOnFinally(sessionId, accountId, authenticationVerified, traceActivityId);
            }
        }

        /// <summary>
        /// Used by Browser and App Flow, after users submit ThreeDSChallenge to ACS,
        /// the users will call the API to notify us the ThreeDSChallenge is completed
        /// We will contact the underline service to fetch result
        /// </summary>
        /// <param name="accountId">Identity the user by accountId</param>
        /// <param name="sessionId">Identity the user's purchase session</param>
        /// <param name="exposedFlightFeatures">Exposed flight features</param>
        /// <param name="traceActivityId">Event trace active id for logging</param>
        /// <param name="authParameters">ACS parameters</param>
        /// <returns>Returns the data PaymentSession for PIDL to consume </returns>
        public override async Task<PaymentSession> CompleteThreeDSOneChallenge(
            string accountId,
            string sessionId,
            List<string> exposedFlightFeatures,
            EventTraceActivity traceActivityId,
            Dictionary<string, string> authParameters = null)
        {
            bool authenticationVerified = false;

            try
            {
                // 1. Get stored session
                await this.CachedGetStoredSession(
                    sessionId: sessionId,
                    traceActivityId: traceActivityId);

                // 2. Create a CompletionRequest object
                var completionRequest = new PayerAuth.CompletionRequest()
                {
                    PaymentSession = new PayerAuth.PaymentSession(this.storedSession),
                    AuthenticationContext = this.storedSession.AuthenticationResponse,
                };

                // We need to send PaRes as part of step 2
                if (authParameters != null)
                {
                    foreach (var key in authParameters.Keys)
                    {
                        completionRequest.AuthorizationParameters.Add(key, authParameters[key]);
                    }
                }

                // 3. Call PayerAuth.CompleteChallenge
                bool isSafetyNetCalled = false;
                PayerAuth.CompletionResponse completionResponse = null;
                if (await CallSafetyNetOperation(
                        async () =>
                        {
                            completionResponse = await this.PayerAuthServiceAccessor.CompleteChallenge(
                               completionRequest: completionRequest,
                               traceActivityId: traceActivityId);
                        },
                        traceActivityId,
                        exposedFeatures: this.storedSession.ExposedFlightFeatures))
                {
                    completionResponse = GetSafetyNetThreeDSOneCompletionResponse();
                    isSafetyNetCalled = true;
                }

                // 4. Convert PayerAuth's PaymentSession object to a PX PaymentSession
                var paymentSession = new PXService.V7.PaymentChallenge.Model.PaymentSession(this.storedSession);
                paymentSession.Signature = paymentSession.GenerateSignature();

                // 5. Update PaymentSession object and return
                switch (completionResponse.TransactionStatus)
                {
                    case PayerAuth.TransactionStatus.U:
                        paymentSession.ChallengeStatus = PaymentChallengeStatus.Failed;
                        break;

                    case PayerAuth.TransactionStatus.R:
                        paymentSession.ChallengeStatus = isSafetyNetCalled ? PaymentChallengeStatus.InternalServerError : PaymentChallengeStatus.Failed;
                        break;

                    case PayerAuth.TransactionStatus.N:
                        paymentSession.ChallengeStatus = PaymentChallengeStatus.Succeeded;
                        if (!string.IsNullOrWhiteSpace(completionResponse.ChallengeCancelIndicator))
                        {
                            PayerAuth.ChallengeCancelIndicator cancelIndicator;
                            if (Enum.TryParse(completionResponse.ChallengeCancelIndicator, out cancelIndicator))
                            {
                                switch (cancelIndicator)
                                {
                                    case PayerAuth.ChallengeCancelIndicator.TransactionCReqTimedOut:
                                    case PayerAuth.ChallengeCancelIndicator.TransactionTimedOut:
                                        paymentSession.ChallengeStatus = PaymentChallengeStatus.TimedOut;
                                        break;

                                    case PayerAuth.ChallengeCancelIndicator.CancelledByCardHolder:
                                    case PayerAuth.ChallengeCancelIndicator.CancelledByRequestor:
                                    case PayerAuth.ChallengeCancelIndicator.TransactionAbandoned:
                                        paymentSession.ChallengeStatus = PaymentChallengeStatus.Cancelled;
                                        break;
                                }
                            }
                        }
                        else if (completionResponse.TransactionStatusReason == PayerAuth.TransactionStatusReason.TSR14)
                        {
                            paymentSession.ChallengeStatus = PaymentChallengeStatus.TimedOut;
                        }

                        break;
                    default:
                        // Authentication was Successful
                        paymentSession.ChallengeStatus = PaymentChallengeStatus.Succeeded;
                        break;
                }

                this.storedSession.ChallengeStatus = paymentSession.ChallengeStatus;

                await this.SessionServiceAccessor.UpdateSessionResourceData<PXInternal.PaymentSession>(sessionId, this.storedSession, traceActivityId);

                authenticationVerified = PaymentSessionsHandler.IsAuthenticationVerified(paymentSession.ChallengeStatus);

                return paymentSession;
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException(string.Format(ExceptionMessage, ex.ToString()), traceActivityId);

                if (this.storedSession != null)
                {
                    this.storedSession.IsSystemError = true;
                }

                return GetSafetyNetPaymentSession(sessionId);
            }
            finally
            {
                await this.UpdateSessionOnFinally(sessionId, accountId, authenticationVerified, traceActivityId);
            }
        }

        /// <summary>
        /// Used by Commercial Portals for 3DS (1.0) in India market
        /// </summary>
        /// <param name="accountId">Identify the user by account Id</param>
        /// <param name="sessionId">Identify the sessionId</param>
        /// <param name="cvvToken">Tokenized CVV</param>
        /// <param name="traceActivityId">Event trace active id for logging</param>
        /// <param name="setting">Setting data from Partner Settings Service</param>
        /// <throws>HttpResponseException if the payment instrument cannot be found</throws>
        /// <returns>Returns the RDS URL </returns>
        public override async Task<TransactionResource> AuthenticateIndiaThreeDS(
            string accountId,
            string sessionId,
            string cvvToken,
            EventTraceActivity traceActivityId,
            PaymentExperienceSetting setting = null)
        {
            await CallSafetyNetOperation(
                   operation: async () =>
                   {
                       await this.CachedGetStoredSession(
                           sessionId: sessionId,
                           traceActivityId: traceActivityId);
                   },
                   traceActivityId);

            PaymentResource paymentResource = await this.TransactionServiceAccessor.CreatePaymentObject(accountId, traceActivityId);
            string paymentId = paymentResource.Id;

            string threeDSSessionId = Guid.NewGuid().ToString();

            ValidationParameters validationParameters = new ValidationParameters
            {
                Amount = this.storedSession.Amount,
                Country = this.storedSession.Country.ToUpperInvariant(),
                Currency = this.storedSession.Currency.ToUpperInvariant(),
                PaymentInstrument = this.storedSession.PaymentInstrumentId,
                Store = GetTransactionServiceStore(this.storedSession.Partner, setting),
                CommercialTransaction = true,
                SessionId = threeDSSessionId,
                AuthenticationData = new AuthenticationData
                {
                    ChallengeType = ChallengeType.Cvv,
                    Data = cvvToken
                },
                UpdateValidation = false
            };

            TransactionResource transactionResource = await this.TransactionServiceAccessor.ValidateCvv(accountId, paymentId, validationParameters, traceActivityId);

            if (!string.IsNullOrEmpty(transactionResource.RedirectUrl))
            {
                await this.PostProcessOnSuccessIndiaThreeDS(
                    traceActivityId: traceActivityId,
                    sessionId: threeDSSessionId,
                    piQueryParams: GetPiQueryParams(
                        billableAccountId: this.storedSession.BillableAccountId,
                        classicProduct: this.storedSession.ClassicProduct,
                        partner: this.storedSession.Partner));
            }

            return transactionResource;
        }

        public override async Task<PaymentSession> TryGetPaymentSession(string sessionId, EventTraceActivity traceActivityId)
        {
            await CallSafetyNetOperation(
                   operation: async () =>
                   {
                       await this.CachedGetStoredSession(
                           sessionId: sessionId,
                           traceActivityId: traceActivityId);
                   },
                   traceActivityId);

            PaymentSession paymentSession = null;
            if (this.storedSession != null)
            {
                paymentSession = new PaymentSession(this.storedSession);
                paymentSession.Language = this.storedSession.Language;
                paymentSession.BillableAccountId = this.storedSession.BillableAccountId;
                paymentSession.ClassicProduct = this.storedSession.ClassicProduct;
                paymentSession.Signature = paymentSession.GenerateSignature();

                paymentSession.ChallengeStatus = this.storedSession.ChallengeStatus;
                paymentSession.PaymentInstrumentId = this.storedSession.PaymentInstrumentId;
                paymentSession.UserDisplayMessage = this.storedSession.AuthenticationResponse?.CardHolderInfo;
            }

            return paymentSession;
        }

        public override async Task<List<string>> GetExposedFlightFeatures(string sessionId, EventTraceActivity traceActivityId)
        {
            PXInternal.PaymentSession paymentSession = await this.SessionServiceAccessor.GetSessionResourceData<PXInternal.PaymentSession>(
                    sessionId: sessionId,
                    traceActivityId: traceActivityId);
            return paymentSession.ExposedFlightFeatures;
        }

        public override async Task<BrowserFlowContext> GetThreeDSMethodData(string sessionId, EventTraceActivity traceActivityId)
        {
            PXInternal.PaymentSession paymentSession = await this.SessionServiceAccessor.GetSessionResourceData<PXInternal.PaymentSession>(
                    sessionId: sessionId,
                    traceActivityId: traceActivityId);

            PXService.Model.ThreeDSExternalService.ThreeDSMethodData formInputThreeDSMethod = new PXService.Model.ThreeDSExternalService.ThreeDSMethodData()
            {
                ThreeDSServerTransID = paymentSession.MethodData.ThreeDSServerTransID,
                ThreeDSMethodNotificationURL = string.Format("{0}/paymentSessions/{1}/authenticate", this.PifdBaseUrl, paymentSession.Id),
            };

            return new BrowserFlowContext
            {
                IsFingerPrintRequired = true,
                FormActionURL = paymentSession.MethodData.ThreeDSMethodURL,
                FormInputThreeDSMethodData = ThreeDSUtils.EncodeUrl(ThreeDSUtils.EncodeObjectToBase64(formInputThreeDSMethod))
            };
        }

        public override async Task<BrowserFlowContext> GetThreeDSAuthenticationData(string sessionId, EventTraceActivity traceActivityId)
        {
            PXInternal.PaymentSession paymentSession = await this.SessionServiceAccessor.GetSessionResourceData<PXInternal.PaymentSession>(
                    sessionId: sessionId,
                    traceActivityId: traceActivityId);

            PayerAuth.AuthenticationResponse authResponse = paymentSession.AuthenticationResponse;
            ThreeDSSessionData threeDSSessionData = new ThreeDSSessionData
            {
                ThreeDSServerTransID = authResponse.ThreeDSServerTransactionId,
                AcsTransID = paymentSession.AuthenticationResponse.AcsTransactionId,
            };

            // 5. Calculate the Form Data to Return for Challenge Scenario.
            ChallengeRequest creq = new ChallengeRequest
            {
                ThreeDSServerTransID = authResponse.ThreeDSServerTransactionId,
                AcsTransID = authResponse.AcsTransactionId,
                MessageType = "CReq",
                MessageVersion = authResponse.MessageVersion ?? GlobalConstants.PSD2Constants.FallbackMessageVersion,
                ChallengeWindowSize = paymentSession.BrowserInfo.ChallengeWindowSize
            };

            return new BrowserFlowContext()
            {
                IsFingerPrintRequired = false,
                IsAcsChallengeRequired = true,
                FormInputThreeDSSessionData = ThreeDSUtils.EncodeUrl(ThreeDSUtils.EncodeObjectToBase64(threeDSSessionData)),
                FormInputCReq = ThreeDSUtils.EncodeUrl(ThreeDSUtils.EncodeObjectToBase64(creq)),
                FormActionURL = authResponse.AcsUrl,
                CardHolderInfo = authResponse.CardHolderInfo,
                TransactionSessionId = authResponse.TransactionSessionId
            };
        }

        public override async Task<bool> CheckOwnership(string accountId, string paymentInstrumentId, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures)
        {
            // Call GetPI to verify the account owns paymentInstrumentId
            try
            {
                PaymentInstrument piDetails = await this.PimsAccessor.GetPaymentInstrument(
                    accountId: accountId,
                    piid: paymentInstrumentId,
                    traceActivityId: traceActivityId);
                return true;
            }
            catch (ServiceErrorResponseException ex)
            {
                if (ex.Error?.ErrorCode == "AccountPINotFound")
                {
                    return false;
                }

                throw;
            }
        }

        public override async Task<BrowserFlowContext> HandlePaymentChallenge(
            string accountId,
            PayerAuth.BrowserInfo browserInfo,
            PaymentSession paymentSession,
            EventTraceActivity traceActivityId,
            List<string> exposedFlightFeatures,
            string email = null,
            string puid = null)
        {
            if (string.Equals(paymentSession.ChallengeType, V7.Constants.ChallengeTypes.ValidatePIOnAttachChallenge, StringComparison.OrdinalIgnoreCase))
            {
                var piQueryParams = GetPiQueryParams(
                        billableAccountId: paymentSession.BillableAccountId,
                        classicProduct: paymentSession.ClassicProduct,
                        partner: paymentSession.Partner);

                await CallSafetyNetOperation(
                   operation: async () =>
                   {
                       await this.CachedGetStoredSession(
                           sessionId: paymentSession.Id,
                           traceActivityId: traceActivityId);
                   },
                   traceActivityId);

                bool successValidatePI = await this.ValidatePI(traceActivityId, piQueryParams);
                if (successValidatePI)
                {
                    paymentSession.ChallengeStatus = PaymentChallengeStatus.Succeeded;
                }
                else
                {
                    paymentSession.ChallengeStatus = PaymentChallengeStatus.Failed;
                }

                return new BrowserFlowContext()
                {
                    IsFingerPrintRequired = false,
                    IsAcsChallengeRequired = false,
                    PaymentSession = paymentSession
                };
            }
            else
            {
                // else return what current 3ds challenge does. Yet under flighting this code will never get called.
                return await this.GetThreeDSMethodURL(
                accountId: accountId,
                browserInfo: browserInfo,
                paymentSession: paymentSession,
                traceActivityId: traceActivityId,
                exposedFlightFeatures: exposedFlightFeatures);
            }
        }

        /// <summary>
        /// Add browserInfo to stored session
        /// </summary>
        /// <param name="browserInfo">The browser info, collected from user request, need store in the session and used in the later call</param>
        /// <param name="paymentSession">The serialized purchase context, need store in the session and used in the later call</param>
        /// <param name="traceActivityId">Event trace active id for logging</param>
        /// <throws>HttpResponseException if the payment instrument cannot be found</throws>
        /// <returns>Returns the data BrowserFlowContext for PIDL to consume </returns>
        public override async Task UpdateSessionResourceData(
            PayerAuth.BrowserInfo browserInfo,
            PaymentSession paymentSession,
            EventTraceActivity traceActivityId)
        {
            // 1. Get the stored session
            if (await CallSafetyNetOperation(
                operation: async () =>
                {
                    await this.CachedGetStoredSession(
                        sessionId: paymentSession.Id,
                        traceActivityId: traceActivityId);
                },
                traceActivityId))
            {
                return;
            }

            if (await CallSafetyNetOperation(
                    operation: async () =>
                    {
                        // 2. Add browserInfo to the stored session
                        this.storedSession.BrowserInfo = browserInfo;
                        await this.SessionServiceAccessor.UpdateSessionResourceData<PXInternal.PaymentSession>(
                            sessionId: paymentSession.Id,
                            newSessionData: this.storedSession,
                            traceActivityId: traceActivityId);
                    },
                    traceActivityId,
                    exposedFeatures: this.storedSession.ExposedFlightFeatures))
            {
                return;
            }
        }

        public override async Task LinkSession(string paymentSessionId, string linkSessionId, EventTraceActivity traceActivityId)
        {
            // 1. Get the stored session
            if (await CallSafetyNetOperation(
                operation: async () =>
                {
                    await this.CachedGetStoredSession(
                        sessionId: paymentSessionId,
                        traceActivityId: traceActivityId);
                },
                traceActivityId))
            {
                return;
            }

            // 2. Save session against PI
            await this.PimsAccessor.LinkSession(
                accountId: this.storedSession.PaymentInstrumentAccountId,
                piid: this.storedSession.PaymentInstrumentId,
                payload: new LinkSession(linkSessionId),
                traceActivityId: traceActivityId,
                queryParams: GetPiQueryParams(
                    billableAccountId: this.storedSession.BillableAccountId,
                    classicProduct: this.storedSession.ClassicProduct,
                    partner: this.storedSession.Partner));
        }

        public override async Task<PXInternal.PaymentSession> GetStoredSession(string sessionId, EventTraceActivity traceActivityId)
        {
            PXInternal.PaymentSession storedSession = await this.SessionServiceAccessor.GetSessionResourceData<PXInternal.PaymentSession>(
                sessionId: sessionId,
                traceActivityId: traceActivityId);
            return storedSession;
        }

        public override async Task<PXInternal.PaymentSession> TryGetStoredSession(string sessionId, EventTraceActivity traceActivityId)
        {
            await CallSafetyNetOperation(
                operation: async () =>
                {
                    await this.CachedGetStoredSession(sessionId: sessionId, traceActivityId: traceActivityId);
                },
                traceActivityId);

            return this.storedSession;
        }

        private static bool IsTokenCollected(PaymentInstrument paymentInstrument)
        {
            string walletType = paymentInstrument?.PaymentInstrumentDetails?.WalletType;
            return string.Equals(walletType, PXCommon.Constants.WalletTypeValues.GooglePay, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(walletType, PXCommon.Constants.WalletTypeValues.ApplePay, StringComparison.OrdinalIgnoreCase);
        }

        private static bool PaymentMethodNotRequiresPaymentChallenge(PaymentInstrument piDetails, List<string> exposedFlightFeatures)
        {
            if (piDetails.IsUpiQr() && exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.EnableLtsUpiQRConsumer, StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }

            if (piDetails.IsGooglePay() && exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXEnablePSD2ForGooglePay, StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }

            return !piDetails.IsCreditCard() && !piDetails.IsLegacyBilldeskPayment() && !PIHelper.IsUpi(piDetails.PaymentMethod.PaymentMethodFamily, piDetails.PaymentMethod.PaymentMethodType);
        }

        private static bool IsPSD2EnabledInPartnerSetting(PaymentExperienceSetting setting)
        {
            if (setting?.Features == null)
            {
                return false;
            }

            PartnerSettingsModel.FeatureConfig featureConfig;
            return setting.Features.TryGetValue(PXCommon.Constants.FeatureNames.PSD2, out featureConfig);
        }

        // VerifyAuthorization functions is called at the time of session creation. For all subsequent calls,
        // the session ids act as secret/credential and there is no authorization needed. If the session id
        // is invalid, the retrieval from session service will fail.
        private static void VerifyMOTOAuthorization(
            PaymentSessionData sessionData,
            string isMotoAuthorized)
        {
            if (sessionData.IsMOTO && !string.Equals("true", isMotoAuthorized, StringComparison.OrdinalIgnoreCase))
            {
                throw new ValidationException(ErrorCode.UnauthorizedMotoPaymentSession, "Unauthorized Moto payment session");
            }
        }

        private static IEnumerable<KeyValuePair<string, string>> GetPiQueryParams(
            string billableAccountId,
            string classicProduct,
            string partner)
        {
            var retVal = new System.Collections.Generic.List<KeyValuePair<string, string>>();

            if (!string.IsNullOrEmpty(billableAccountId))
            {
                retVal.Add(new KeyValuePair<string, string>(GlobalConstants.QueryParamNames.BillableAccountId, billableAccountId));
            }

            if (!string.IsNullOrEmpty(classicProduct))
            {
                retVal.Add(new KeyValuePair<string, string>(GlobalConstants.QueryParamNames.ClassicProduct, classicProduct));
            }

            if (!string.IsNullOrEmpty(partner))
            {
                retVal.Add(new KeyValuePair<string, string>(GlobalConstants.QueryParamNames.Partner, partner));
            }

            return retVal;
        }

        private static string GetLatestAvailablePSD2SettingVersion(List<string> exposedFlightFeatures)
        {
            var regex = new Regex(PSD2SettingVersionFlightRegex, RegexOptions.IgnoreCase);
            IEnumerable<int> availableVersions = exposedFlightFeatures.Where(x => regex.IsMatch(x))
                .Select(x => int.Parse(regex.Match(x).Groups["versionNumber"].Value));
            return availableVersions.Any() ? $"V{availableVersions.Max()}" : PaymentSessionsHandler.DefaultPSD2SettingsVersion;
        }

        /// <summary>
        /// Calls a specified operation within a "safety net". That is certain types of exceptions are caught
        /// and logged.
        /// </summary>
        /// <param name="operation">The operation that could throw an exception</param>
        /// <param name="traceActivity">The event trace activity</param>
        /// <param name="exposedFeatures">Features exposes for the current request (per flight configuration on carbon)</param>
        /// <param name="excludeErrorFeatureFormat">Format of a carbon feature name that that has two place holders. 1. Http Status code 2. ErrorCode.</param>
        /// <returns>
        /// True if an exception was caught. This can be used by the caller to take remedial actions.
        /// </returns>
        private static async Task<bool> CallSafetyNetOperation(
            Func<Task> operation,
            EventTraceActivity traceActivity,
            List<string> exposedFeatures = null,
            string excludeErrorFeatureFormat = null)
        {
            try
            {
                await operation();
            }
            catch (ServiceErrorResponseException ex)
            {
                if (exposedFeatures == null || excludeErrorFeatureFormat == null || !exposedFeatures.Contains(
                        string.Format(excludeErrorFeatureFormat, ex.Response?.StatusCode.ToString(), ex.Error?.ErrorCode)))
                {
                    SllWebLogger.TracePXServiceException($"SafetyNet caught ServiceErrorResponseException: {ex}", traceActivity);
                    return true;
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException($"SafetyNet caught nonServiceErrorResponseException : {ex}", traceActivity);
                return true;
            }

            return false;
        }

        private static async Task TryWithRetry(Func<Task> operation)
        {
            var tries = 0;
            while (tries < 2)
            {
                try
                {
                    await operation();
                    break;
                }
                catch (Exception ex)
                {
                    if (tries < 1)
                    {
                        Thread.Sleep(50);
                    }
                    else
                    {
                        throw ex;
                    }
                }

                tries++;
            }
        }

        private static BrowserFlowContext GetSafetyNetBrowserFlowContext(PaymentSession ps, List<string> exposedFlightFeatures = null)
        {
            ps.ChallengeStatus = PaymentChallengeStatus.Succeeded;

            if ((!string.IsNullOrEmpty(ps.Country) && string.Equals(ps.Country, "IN", StringComparison.OrdinalIgnoreCase))
                && (!string.IsNullOrEmpty(ps.Currency) && string.Equals(ps.Currency, "INR", StringComparison.OrdinalIgnoreCase))
                && (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXReturnFailedSessionState)))
            {
                ps.ChallengeStatus = PaymentChallengeStatus.Failed;
            }

            return new BrowserFlowContext
            {
                IsFingerPrintRequired = false,
                IsAcsChallengeRequired = false,
                PaymentSession = ps
            };
        }

        private static PaymentSession GetSafetyNetPaymentSession(
            PaymentSessionData paymentSessionData,
            PaymentChallengeStatus challengeStatus = PaymentChallengeStatus.NotApplicable)
        {
            var ps = new PaymentSession(paymentSessionData);
            ps.Id = Guid.NewGuid().ToString();
            ps.IsChallengeRequired = false;
            ps.ChallengeStatus = challengeStatus;
            ps.Signature = ps.GenerateSignature();
            return ps;
        }

        private static PaymentSession GetSafetyNetPaymentSession(
            string id,
            bool isChallengeRequired = true,
            PaymentChallengeStatus challengeStatus = PaymentChallengeStatus.Succeeded)
        {
            var ps = new PaymentSession()
            {
                Id = id,
                IsChallengeRequired = isChallengeRequired,
                ChallengeStatus = challengeStatus
            };

            ps.Signature = ps.GenerateSignature();
            return ps;
        }

        private static AuthenticationResponse GetSafetyNetAuthenticationResponse()
        {
            return new AuthenticationResponse()
            {
                EnrollmentStatus = PXService.Model.PayerAuthService.PaymentInstrumentEnrollmentStatus.Bypassed,
                ChallengeStatus = PaymentChallengeStatus.Succeeded
            };
        }

        private static PayerAuth.CompletionResponse GetSafetyNetCompletionResponse()
        {
            return new PayerAuth.CompletionResponse()
            {
                ChallengeCompletionIndicator = PXService.Model.PayerAuthService.ChallengeCompletionIndicator.Y,
            };
        }

        private static PayerAuth.CompletionResponse GetSafetyNetThreeDSOneCompletionResponse()
        {
            return new PayerAuth.CompletionResponse()
            {
                TransactionStatus = PayerAuth.TransactionStatus.R,
                ChallengeCompletionIndicator = PXService.Model.PayerAuthService.ChallengeCompletionIndicator.Y,
            };
        }

        private static PaymentChallengeStatus GetMappedStatusForAuthentication(
            PayerAuth.TransactionStatus transStatus,
            bool isMoto,
            List<string> additionalInputs,
            List<string> exposedFlights)
        {
            bool bypassMOTOChallenges = ShouldBypassMOTOPSD2Challenge(isMoto, exposedFlights);

            var mappedStatusString = GetMappedStatusString(
                 mapPrefix: "PXPSD2Auth",
                 transStatus: transStatus,
                 isMoto: bypassMOTOChallenges,
                 additionalInputs: additionalInputs,
                 exposedFlights: exposedFlights);

            var mappedStatus = PaymentChallengeStatus.Succeeded;
            if (mappedStatusString == null || !Enum.TryParse<PaymentChallengeStatus>(mappedStatusString, true, out mappedStatus))
            {
                // Fallback to hardcoded status mapping if parsing from flights did not work for some reasons
                if (transStatus == PayerAuth.TransactionStatus.C)
                {
                    mappedStatus = PaymentChallengeStatus.Unknown;
                }
                else if (transStatus == PayerAuth.TransactionStatus.R)
                {
                    mappedStatus = PaymentChallengeStatus.Failed;
                }
                else
                {
                    mappedStatus = bypassMOTOChallenges ? PaymentChallengeStatus.ByPassed : PaymentChallengeStatus.Succeeded;
                }
            }

            if (transStatus == PayerAuth.TransactionStatus.FR)
            {
                mappedStatus = PaymentChallengeStatus.Failed;
            }

            return mappedStatus;
        }

        private static PaymentChallengeStatus GetMappedStatusForThreeDSOneAuthentication(PayerAuth.TransactionStatus transStatus)
        {
            var mappedStatus = PaymentChallengeStatus.Succeeded;
            if (transStatus == PayerAuth.TransactionStatus.C)
            {
                mappedStatus = PaymentChallengeStatus.Unknown;
            }
            else if (transStatus == PayerAuth.TransactionStatus.U || transStatus == PayerAuth.TransactionStatus.N)
            {
                mappedStatus = PaymentChallengeStatus.Failed;
            }

            return mappedStatus;
        }

        /// <summary>
        /// At the time of PSD2 launch, there was no clarity on how external actors (banks, processors) would interpret
        /// PSD2 specifications.  For example, during Completion API (mapPrefix "PXPSD2Comp"), if transaction status is "N"
        /// and transaction status reason is TSR10 and status indicator is 01, does it mean that the flow is successful,
        /// cancelled, failed, or timedout - it was unclear.  So, we had to have this mapping be configurable and use flights
        /// as configuration that we could tweak without requiring a re-deployment.  Similar ambiguity existed about how to
        /// interpret returned values during the Auth call (mapPrefix "PXPSD2Auth").  Hence the below function to find mapping
        /// if it exists as a flight name for the current set of returned values.
        /// Details:
        /// It creates every combination of strings in the additionalInputs list and appends it to "{mapPrefix}-{transStatus}-"
        /// and verifies if a flight begins with that string.  On first match, returns the flight suffix.  For example, let's
        /// say inputs are as below:
        ///  - mapPrefix = "PXPSD2Comp"
        ///  - transStatus = "N"
        ///  - additionalInputs = { "TSR10", "01" }
        /// For these inputs, the function generates the following strings (order is important) and for each, it checks if
        /// exposedFlights has any string that begins with it.
        ///  1. PXPSD2Comp-N-TSR10-01-
        ///  2. PXPSD2Comp-N-_-01-
        ///  3. PXPSD2Comp-N-TSR10-_-
        ///  4. PXPSD2Comp-N-_-_-
        /// On first match, it returns the suffix.  So, let's say exposedFlights contains the following (order is NOT important):
        ///  - PXPSD2Comp-R-_-_-Failed
        ///  - PXPSD2Comp-N-_-04-TimedOut
        ///  - PXPSD2Comp-N-TSR10-_-Failed
        ///  - PXPSD2Comp-_-_-01-Cancelled
        ///  - PXPSD2Comp-_-_-_-Succeeded
        /// The function returns "Failed" because in this example, "PXPSD2Comp-N-TSR10-_-Failed" is the first (and only) match
        /// </summary>
        /// <param name="mapPrefix">e.g. PXPSD2Comp</param>
        /// <param name="transStatus">e.g. N</param>
        /// <param name="isMoto">On behalf of scenario</param>
        /// <param name="additionalInputs">Additional inputs that can affect what the outcome is.  All combination of these will be added to the return string</param>
        /// <param name="exposedFlights">Flights that are ON currently</param>
        /// <returns>Given the above inputs, returns the output indicating if we should consider this transaction Cancelled, Failed, Timedout etc.</returns>
        private static string GetMappedStatusString(
            string mapPrefix,
            PayerAuth.TransactionStatus transStatus,
            bool isMoto,
            List<string> additionalInputs,
            List<string> exposedFlights)
        {
            // Get mapped status from flights
            try
            {
                for (int i = 0; i < Math.Pow(2, additionalInputs.Count); i++)
                {
                    string input = string.Format("{0}-{1}-", mapPrefix, transStatus.ToString());

                    // Append every possible combination of strings in additionalInputs.  "_" is a wildcard (we couldn't use
                    // "*" because its not allowed to be part of a flight name on carbon.  Hence using "_" instead).
                    for (int j = 0; j < additionalInputs.Count; j++)
                    {
                        // Bitwise shift 1 (0000 0000 0000 0001) left by j positions and logical-and with i
                        input += ((i & (1 << j)) == 0) ? additionalInputs[j] ?? string.Empty : "_";
                        input += "-";
                    }

                    var inputOutputMap = exposedFlights.FirstOrDefault((f) => f.StartsWith(input, StringComparison.OrdinalIgnoreCase));
                    if (inputOutputMap != null)
                    {
                        return inputOutputMap.Substring(input.Length);
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private static bool PiRequiresAuthentication(bool piRequires3ds2Authentication, bool piRequires3ds1Authentication, PimsModel.V4.PaymentMethod pm, List<string> exposedFlightFeatures)
        {
            if (pm.IsUpiQr() && exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.EnableLtsUpiQRConsumer, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }

            return piRequires3ds2Authentication || piRequires3ds1Authentication || pm.IsUpi();
        }

        private static PSD2NativeChallengeLocalizations GetPSD2NativeChallengeLocalizations(string language = "en-GB")
        {
            string header = LocalizationRepository.Instance.GetLocalizedString(V7.Constants.PSD2NativeChallengeStrings.Header, language);
            string cancel = LocalizationRepository.Instance.GetLocalizedString(V7.Constants.PSD2NativeChallengeStrings.CancelButtonLabel, language);
            string back = LocalizationRepository.Instance.GetLocalizedString(V7.Constants.PSD2NativeChallengeStrings.BackButtonLabel, language);
            string pressBack = LocalizationRepository.Instance.GetLocalizedString(V7.Constants.PSD2NativeChallengeStrings.BackButtonAccessibilityLabel, language);
            string pressCancel = LocalizationRepository.Instance.GetLocalizedString(V7.Constants.PSD2NativeChallengeStrings.CancelButtonAccessibilityLabel, language);
            string ordering = LocalizationRepository.Instance.GetLocalizedString(V7.Constants.PSD2NativeChallengeStrings.OrderingAccessibilityLabel, language);
            string bankLogo = LocalizationRepository.Instance.GetLocalizedString(V7.Constants.PSD2NativeChallengeStrings.BankLogoAccessibilityLabel, language);
            string cardLogo = LocalizationRepository.Instance.GetLocalizedString(V7.Constants.PSD2NativeChallengeStrings.CardLogoAccessibilityLabel, language);

            return new PSD2NativeChallengeLocalizations
            {
                ChallengePageHeader = header,
                BackButtonLabel = back,
                BackButtonAccessibilityLabel = pressBack,
                CancelButtonLabel = cancel,
                CancelButtonAccessibilityLabel = pressCancel,
                OrderingAccessibilityLabel = ordering,
                BankLogoAccessibilityLabel = bankLogo,
                CardLogoAccessibilityLabel = cardLogo
            };
        }

        private static bool ExcludeJCBChallenge(PaymentInstrument paymentInstrument, List<string> exposedFlightFeatures)
        {
            // TODO: Remove unused logic after flighting/testing  https://microsoft.visualstudio.com/OSGS/_boards/board/t/DPX/Deliverables%20and%20Task%20Groups/?workitem=55766231
            if (paymentInstrument.IsCreditCard()
                && string.Equals(paymentInstrument.PaymentMethod.PaymentMethodType, "JCB", StringComparison.InvariantCultureIgnoreCase)
                && (!exposedFlightFeatures.Contains("PXPSD2SettingVersionV25") || !exposedFlightFeatures.Contains("PXDisplayJCBChallenge")))
            {
                return true;
            }

            return false;
        }

        private static bool ShouldBypassMOTOPSD2Challenge(bool isMOTO, List<string> exposedFlightFeatures)
        {
            return isMOTO && (exposedFlightFeatures == null || !exposedFlightFeatures.Contains(Flighting.Features.PXEnableChallengesForMOTO));
        }

        private async Task UpdateSessionOnFinally(string sessionId, string accountId, bool authenticationVerified, EventTraceActivity traceActivityId)
        {
            if (this.storedSession != null)
            {
                authenticationVerified = authenticationVerified || this.storedSession.IsSystemError;
                if (authenticationVerified)
                {
                    await this.PostProcessOnSuccess(
                        traceActivityId: traceActivityId,
                        piQueryParams: GetPiQueryParams(
                            billableAccountId: this.storedSession.BillableAccountId,
                            classicProduct: this.storedSession.ClassicProduct,
                            partner: this.storedSession.Partner));
                }

                await this.SafetyNetUpdateSession(traceActivityId);
            }

            authenticationVerified = authenticationVerified || this.storedSession == null;

            // TODO : Remove after Fraud Investigation
            SllWebLogger.TraceServerMessage("UpdateSessionOnFinally", null, sessionId, this.storedSession != null ? "storedSession: " + JsonConvert.SerializeObject(this.storedSession) : "No stored session", EventLevel.Informational);

            if (authenticationVerified)
            {
                await CallSafetyNetOperation(
                    async () =>
                    {
                        await this.TransactionDataServiceAccessor.UpdateCustomerChallengeAttestation(accountId, sessionId, authenticationVerified, traceActivityId);
                    },
                    traceActivityId);
            }
        }

        private async Task<PaymentSession> InternalCreatePaymentSession(
            string accountId,
            PaymentSessionData paymentSessionData,
            DeviceChannel deviceChannel,
            string emailAddress,
            List<string> exposedFlightFeatures,
            EventTraceActivity traceActivityId,
            TestContext testContext,
            string isMotoAuthorized = null,
            string tid = null,
            PaymentExperienceSetting setting = null,
            string userId = null,
            bool isGuestUser = false)
        {
            // Migrate code over to CreatePaymentSessionInternal
            // If exception is thrown, then return safety net session
            PaymentSession session = new PaymentSession(paymentSessionData);
            PaymentInstrument paymentInstrument = null;
            bool piRequires3ds1Authentication = false;
            bool piRequires3ds2Authentication = false;
            bool piIsIssuedIn3ds1RequiredCountry = false;

            try
            {
                // Remove the following check once ThreeDS Provider MC Prod is ready.
                if (!IsPSD2EnabledInPartnerSetting(setting) &&
                    (exposedFlightFeatures == null || !exposedFlightFeatures.Contains(Flighting.Features.PXPSD2ProdIntegration, StringComparer.OrdinalIgnoreCase)) &&
                    Common.Environments.Environment.Current.EnvironmentType != EnvironmentType.Integration &&
                    Common.Environments.Environment.Current.EnvironmentType != EnvironmentType.OneBox &&
                    !HttpRequestHelper.HasAnyPSD2TestScenarios(testContext))
                {
                    session.IsChallengeRequired = false;
                    session.Id = Guid.NewGuid().ToString();
                    session.ChallengeStatus = PaymentChallengeStatus.NotApplicable;
                    session.Signature = session.GenerateSignature();
                    return session;
                }

                // Verify Moto and Pi ownership before creating a session
                VerifyMOTOAuthorization(
                    sessionData: paymentSessionData,
                    isMotoAuthorized: isMotoAuthorized);

                // 0. GetExtendedPI from PIMS to see if PI is creditcard or not. We are using this API instead of GetPI to bypass Authorization checks on the PI
                if ((exposedFlightFeatures != null && V7.Constants.PSD2IgnorePIAuthorizationPartners.Contains(paymentSessionData.Partner, StringComparer.OrdinalIgnoreCase))
                    || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.PSD2, session.Country, setting, PXCommon.Constants.DisplayCustomizationDetail.Psd2IgnorePIAuthorization))
                {
                    // The check for commercialstores should be done before accountId, piid ownership check. It is because commercial stores has my-org scenario
                    // to do purchase against SEPA or ACH. To avoid failure for SEPA and ACH, return "NotApplicable" before we do ownership check.
                    if (await CallSafetyNetOperation(
                    async () =>
                    {
                        paymentInstrument = await this.PimsAccessor.GetExtendedPaymentInstrument(
                            piid: session.PaymentInstrumentId,
                            traceActivityId: traceActivityId,
                            paymentSessionData.Partner,
                            exposedFlightFeatures: exposedFlightFeatures);
                    },
                    traceActivityId,
                    exposedFeatures: exposedFlightFeatures,
                    excludeErrorFeatureFormat: "PSD2SafetyNet-GetPIExt-{0}-{1}"))
                    {
                        return GetSafetyNetPaymentSession(paymentSessionData);
                    }

                    if (PaymentMethodNotRequiresPaymentChallenge(paymentInstrument, exposedFlightFeatures))
                    {
                        session.IsChallengeRequired = false;
                        session.Id = Guid.NewGuid().ToString();
                        session.ChallengeStatus = PaymentChallengeStatus.NotApplicable;
                        session.Signature = session.GenerateSignature();
                        return session;
                    }
                }

                // 1. GetPI to verify paymentInstrumentAccountId owns paymentInstrumentId (GetExtendedPI does not verify ownership)
                try
                {
                    PaymentInstrument piDetails = await this.PimsAccessor.GetPaymentInstrument(
                        accountId: accountId,
                        piid: session.PaymentInstrumentId,
                        traceActivityId: traceActivityId,
                        setting: setting);

                    // For Gpay and Apay, we need to check if the token is collected. If collected, we need to set isTokenCollected as true so PIDLSDK can use it to distinguish between long form flow and express checkout flow.
                    if (piDetails.IsGooglePay() || piDetails.IsApplePay())
                    {
                        session.IsTokenCollected = IsTokenCollected(piDetails);
                    }

                    if (piDetails.IsApplePay())
                    {
                        return this.AddValidatePIOnAttachChallengeToSession(session, accountId, paymentSessionData, piDetails, deviceChannel, exposedFlightFeatures, emailAddress);
                    }

                    if (PaymentMethodNotRequiresPaymentChallenge(piDetails, exposedFlightFeatures))
                    {
                        session.IsChallengeRequired = false;
                        session.Id = Guid.NewGuid().ToString();
                        session.ChallengeStatus = PaymentChallengeStatus.NotApplicable;
                        session.Signature = session.GenerateSignature();
                        return session;
                    }
                }
                catch (ServiceErrorResponseException ex)
                {
                    if (ex.Error?.ErrorCode == "AccountPINotFound")
                    {
                        throw new ValidationException(
                            ErrorCode.PaymentInstrumentNotFound,
                            "Caller is not authorized to access specified PaymentInstrumentId");
                    }
                    else if (ex.Error?.InnerError?.ErrorCode == "InvalidQueryStringParameter"
                        && ex.Error?.InnerError?.Target == "AccountService")
                    {
                        throw new ValidationException(ErrorCode.InvalidAccountId, "Specified PaymentInstrumentAccountId was not found");
                    }

                    throw;
                }

                // 2. GetExtendedPI from PIMS to see if challenge may be required
                if (await CallSafetyNetOperation(
                    async () =>
                    {
                        paymentInstrument = await this.PimsAccessor.GetExtendedPaymentInstrument(
                            piid: session.PaymentInstrumentId,
                            traceActivityId: traceActivityId,
                            paymentSessionData.Partner,
                            exposedFlightFeatures: exposedFlightFeatures);

                        List<string> challengeList = paymentInstrument?.PaymentInstrumentDetails?.RequiredChallenge;

                        // For India flows, check another flight. This will be removed later.
                        piIsIssuedIn3ds1RequiredCountry = challengeList != null && challengeList.Contains("3ds")
                            && ((exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.India3dsEnableForBilldesk, StringComparer.OrdinalIgnoreCase)) || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.ThreeDSOne, session.Country, setting, PXCommon.Constants.DisplayCustomizationDetail.India3dsEnableForBilldesk));

                        piRequires3ds1Authentication = piIsIssuedIn3ds1RequiredCountry
                                                        && string.Equals(session.Country, "IN", StringComparison.OrdinalIgnoreCase)
                                                        && string.Equals(session.Currency, "INR", StringComparison.OrdinalIgnoreCase);
                        piRequires3ds2Authentication = challengeList != null && challengeList.Contains("3ds2");
                    },
                    traceActivityId,
                    exposedFeatures: exposedFlightFeatures,
                    excludeErrorFeatureFormat: "PSD2SafetyNet-GetPIExt-{0}-{1}"))
                {
                    return GetSafetyNetPaymentSession(paymentSessionData);
                }

                // Guest checkout user
                bool isGuestCheckoutWithValidSessionId = false;

                if (isGuestUser
                    && paymentInstrument?.PaymentInstrumentDetails?.UsageType == UsageType.Inline
                    && !string.IsNullOrWhiteSpace(paymentInstrument?.PaymentInstrumentDetails?.TransactionLink?.LinkedPaymentSessionId))
                {
                    // For 1PP Guest checkout
                    session.IsChallengeRequired = false;
                    session.ChallengeStatus = PaymentChallengeStatus.NotApplicable;
                    session.Id = paymentInstrument.PaymentInstrumentDetails.TransactionLink.LinkedPaymentSessionId;
                    session.Signature = session.GenerateSignature();

                    // Let the guest checkout complete rest of the flow (psd2) instead of returning session from here
                    if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXEnablePSD2ForGuestCheckoutFlow, StringComparer.OrdinalIgnoreCase))
                    {
                        isGuestCheckoutWithValidSessionId = true;
                    }
                    else
                    {
                        return session;
                    }
                }

                // Since Amex is not enabled through billdesk in India market yet, we need to continue processing amex with adyen, so we need to disable 2FA for Amex until Amex is enabled through billdesk
                // Once Amex is enabled through billdesk, we need to enable 2FA for Amex. Created task 38121719 to track this.
                if (piRequires3ds1Authentication
                    && paymentInstrument.IsCreditCard()
                    && ((!((exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXEnableIndia3DS1Challenge, StringComparer.OrdinalIgnoreCase)) || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.ThreeDSOne, session.Country, setting, PXCommon.Constants.DisplayCustomizationDetail.PXEnableIndia3DS1Challenge)))
                        || (paymentInstrument.PaymentMethod.IsCreditCardAmex()
                        && string.Equals(session.Country, "IN", StringComparison.OrdinalIgnoreCase))))
                {
                    piRequires3ds1Authentication = false;
                }

                if (piRequires3ds1Authentication == false
                    && paymentInstrument.IsCreditCard()
                    && HttpRequestHelper.HasAnyThreeDSOneTestScenarios(testContext))
                {
                    piRequires3ds1Authentication = true;
                    piRequires3ds2Authentication = false;
                }

                if (piRequires3ds2Authentication == false
                    && (paymentInstrument.IsCreditCard() ||
                        paymentInstrument.IsGooglePay())
                    && ((exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXPSD2PretendPIMSReturned3DS2, StringComparer.OrdinalIgnoreCase))
                        || HttpRequestHelper.HasE2EPSD2TestScenarios(testContext)))
                {
                    piRequires3ds2Authentication = true;

                    // Because 3ds1 and PSD2 share the same PSD2 test header, when the PSD2 test header passed together with "EnableThreeDSOne" partner flight, both 3ds1 and PSD2 are enabled, which is not expected
                    // Therefore, set piRequires3ds2Authentication to be false when both PSD2 test header and "EnableThreeDSOne" partner flight are sent from partner
                    if ((exposedFlightFeatures.Contains(Flighting.Features.PXEnableIndia3DS1Challenge, StringComparer.OrdinalIgnoreCase) || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.ThreeDSOne, session.Country, setting, PXCommon.Constants.DisplayCustomizationDetail.PXEnableIndia3DS1Challenge)) && string.Equals(session.Country, "IN", StringComparison.OrdinalIgnoreCase))
                    {
                        piRequires3ds2Authentication = false;
                    }
                }

                if (piRequires3ds1Authentication == false
                    && paymentInstrument.IsCreditCard()
                    && HttpRequestHelper.HasE2EPSD2TestScenarios(testContext)
                    && string.Equals(session.Country, "IN", StringComparison.OrdinalIgnoreCase))
                {
                    piRequires3ds1Authentication = true;
                    piRequires3ds2Authentication = false;

                    if ((PartnerHelper.IsIndiaThreeDSCommercialPartner(paymentSessionData.Partner)
                        || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.ThreeDSOne, session.Country, setting, PXCommon.Constants.DisplayCustomizationDetail.EnableIndia3dsForNonZeroPaymentTransaction))
                        && ((session.ChallengeScenario == ChallengeScenario.PaymentTransaction
                        && session.Amount == 0)
                        || session.ChallengeScenario == ChallengeScenario.RecurringTransaction)
                        && string.Equals(session.Country, "IN", StringComparison.OrdinalIgnoreCase))
                    {
                        piRequires3ds1Authentication = false;
                    }
                }

                // Used to enable 0INR no challenge flow for "digital attach" scenario for webblends in India market
                // set piRequires3ds1Authentication = false to ensure payerauth CreatePaymentSessionId creates a short term session and PX returns isChallengeRequired = false, challengeStatus = NotApplicable
                // TODO: Remove this short term approach once Free Trials/Reduced price purchases are enabled for India market
                if (exposedFlightFeatures != null
                    && exposedFlightFeatures.Contains(Flighting.Features.PXSkipChallengeForZeroAmountIndiaAuth, StringComparer.OrdinalIgnoreCase)
                    && string.Equals(paymentSessionData.Country, "IN", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(paymentSessionData.Partner, PXCommon.Constants.PartnerNames.Webblends, StringComparison.OrdinalIgnoreCase)
                    && paymentSessionData.Amount == 0)
                {
                    piRequires3ds1Authentication = false;
                }

                // 3. Create a PayerAuth's PaymentSession object
                var payerAuthPaymentSession = new PayerAuth.PaymentSession(
                    accountId: accountId,
                    id: null,
                    data: paymentSessionData,
                    paymentInstrument: paymentInstrument,
                    deviceChannel: deviceChannel,
                    piRequiresAuthentication: PiRequiresAuthentication(piRequires3ds2Authentication, piRequires3ds1Authentication, paymentInstrument.PaymentMethod, exposedFlightFeatures));

                if (paymentSessionData.RedeemRewards && !string.IsNullOrEmpty(userId))
                {
                    payerAuthPaymentSession.UserId = userId;
                }

                // 4. Use the existing sessionId created by pims for guest checkout or else Call CreatePaymentSessionId
                if (isGuestCheckoutWithValidSessionId)
                {
                    payerAuthPaymentSession.Id = paymentInstrument.PaymentInstrumentDetails.TransactionLink.LinkedPaymentSessionId;
                    session.Id = paymentInstrument.PaymentInstrumentDetails.TransactionLink.LinkedPaymentSessionId;
                }
                else
                {
                    PayerAuth.PaymentSessionResponse resp = null;

                    if (await CallSafetyNetOperation(
                        async () =>
                        {
                            resp = await this.PayerAuthServiceAccessor.CreatePaymentSessionId(
                                paymentSessionData: payerAuthPaymentSession,
                                traceActivityId: traceActivityId);
                        },
                        traceActivityId,
                        exposedFeatures: exposedFlightFeatures,
                        excludeErrorFeatureFormat: "PSD2SafetyNet-CreatePS-{0}-{1}"))
                    {
                        return GetSafetyNetPaymentSession(paymentSessionData);
                    }

                    payerAuthPaymentSession.Id = resp.PaymentSessionId;
                    session.Id = resp.PaymentSessionId;
                }

                this.storedSession = new PXInternal.PaymentSession(
                    exposedFlightFeatures: exposedFlightFeatures,
                    payerAuthApiVersion: GlobalConstants.PayerAuthApiVersions.V3,
                    accountId: accountId,
                    payerAuthPaymentSession: payerAuthPaymentSession,
                    billableAccountId: paymentSessionData.BillableAccountId,
                    classicProduct: paymentSessionData.ClassicProduct,
                    emailAddress: emailAddress,
                    language: paymentSessionData.Language,
                    isGuestCheckout: isGuestCheckoutWithValidSessionId);

                bool skipMOTOChallenges = ShouldBypassMOTOPSD2Challenge(session.IsMOTO, exposedFlightFeatures);

                // 5. If MOTO, call PayerAuth.Authenticate to notify that this is MOTO and return with no challenge
                if (skipMOTOChallenges || session.RedeemRewards)
                {
                    session.IsChallengeRequired = false;
                    session.ChallengeStatus = session.RedeemRewards ? PaymentChallengeStatus.NotApplicable : PaymentChallengeStatus.ByPassed;
                    session.Signature = session.GenerateSignature();

                    PayerAuth.AuthenticationRequest authRequest = new PayerAuth.AuthenticationRequest()
                    {
                        PaymentSession = payerAuthPaymentSession,
                    };

                    await CallSafetyNetOperation(
                        async () =>
                        {
                            await this.PayerAuthServiceAccessor.Authenticate(authRequest, traceActivityId);
                        },
                        traceActivityId,
                        exposedFeatures: exposedFlightFeatures,
                        excludeErrorFeatureFormat: "PSD2SafetyNet-MotoAuthN-{0}-{1}");

                    this.storedSession.ChallengeStatus = session.ChallengeStatus;

                    if (exposedFlightFeatures == null || !exposedFlightFeatures.Contains(Flighting.Features.PXSkipDuplicatePostProcessForMotoAndRewards))
                    {
                        // TODO: Inspect authResponse and raise Integration exception if EnrollmentStatus is not ByPassed
                        await this.PostProcessOnSuccess(
                            traceActivityId: traceActivityId,
                            piQueryParams: GetPiQueryParams(
                                billableAccountId: paymentSessionData.BillableAccountId,
                                classicProduct: paymentSessionData.ClassicProduct,
                                partner: paymentSessionData.Partner));
                    }

                    return session;
                }

                // 7. Update PX's PaymentSession object, PostProcessOnSuccess and return
                if (piRequires3ds2Authentication)
                {
                    session.IsChallengeRequired = true;
                    session.ChallengeStatus = PaymentChallengeStatus.Unknown;
                    if (PartnerHelper.IsValidatePIOnAttachEnabled(paymentSessionData.Partner, exposedFlightFeatures))
                    {
                        session.ChallengeType = V7.Constants.ChallengeTypes.PSD2Challenge;
                    }
                    else if (paymentInstrument.IsLegacyBilldeskPayment())
                    {
                        session.ChallengeType = V7.Constants.ChallengeTypes.LegacyBillDeskPaymentChallenge;
                    }

                    if (ExcludeJCBChallenge(paymentInstrument, exposedFlightFeatures))
                    {
                        session.IsChallengeRequired = false;
                        session.ChallengeStatus = PaymentChallengeStatus.NotApplicable;
                    }
                }
                else if (string.Equals(payerAuthPaymentSession.PaymentMethodType, V7.Constants.PaymentMethodType.UPI, StringComparison.OrdinalIgnoreCase)
                    && (session.ChallengeScenario == ChallengeScenario.RecurringTransaction || session.ChallengeScenario == ChallengeScenario.PaymentTransaction))
                {
                    session.IsChallengeRequired = true;
                    session.ChallengeStatus = PaymentChallengeStatus.Unknown;
                    session.ChallengeType = V7.Constants.ChallengeTypes.UPIChallenge;
                }
                else if (string.Equals(payerAuthPaymentSession.PaymentMethodType, V7.Constants.PaymentMethodType.UPIQr, StringComparison.OrdinalIgnoreCase)
                    && (session.ChallengeScenario == ChallengeScenario.RecurringTransaction || session.ChallengeScenario == ChallengeScenario.PaymentTransaction)
                    && exposedFlightFeatures.Contains(Flighting.Features.EnableLtsUpiQRConsumer, StringComparer.OrdinalIgnoreCase))
                {
                    session.IsChallengeRequired = true;
                    session.ChallengeStatus = PaymentChallengeStatus.Unknown;
                    session.ChallengeType = V7.Constants.ChallengeTypes.UPIChallenge;
                }
                else if (((PartnerHelper.IsIndiaThreeDSCommercialPartner(paymentSessionData.Partner)
                    || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.ThreeDSOne, session.Country, setting, PXCommon.Constants.DisplayCustomizationDetail.EnableIndia3dsForNonZeroPaymentTransaction))
                    && session.ChallengeScenario == ChallengeScenario.PaymentTransaction
                    && session.Amount != 0
                    && string.Equals(session.Country, "IN", StringComparison.OrdinalIgnoreCase))
                    || piRequires3ds1Authentication)
                {
                    session.IsChallengeRequired = true;
                    session.ChallengeStatus = PaymentChallengeStatus.Unknown;
                    session.ChallengeType = V7.Constants.ChallengeTypes.India3DSChallenge;
                }
                else if (PartnerHelper.IsValidatePIOnAttachEnabled(paymentSessionData.Partner, exposedFlightFeatures)
                         && (paymentInstrument.IsCreditCard() || paymentInstrument.IsGooglePayInstancePI()) && !string.Equals(session.Country, "IN", StringComparison.OrdinalIgnoreCase)
                         && !piIsIssuedIn3ds1RequiredCountry)
                {
                    session.IsChallengeRequired = true;
                    session.ChallengeStatus = PaymentChallengeStatus.Unknown;
                    session.ChallengeType = V7.Constants.ChallengeTypes.ValidatePIOnAttachChallenge;
                }
                else
                {
                    session.IsChallengeRequired = false;
                    session.ChallengeStatus = PaymentChallengeStatus.NotApplicable;
                }

                if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXEnableGettingStoredSessionForChallengeDescriptionsController, StringComparer.OrdinalIgnoreCase))
                {
                    this.storedSession.ChallengeType = session.ChallengeType;
                    this.storedSession.PiRequiresAuthentication = session.IsChallengeRequired;
                }

                // Place holder for generate signature
                session.Signature = session.GenerateSignature();
                return session;
            }
            finally
            {
                if (this.storedSession != null)
                {
                    // For guestCheckout we reuse the sessionId from PIMS, so if the HandlePaymentChallenge is called again
                    // then the session already created in the session service first time and skip the CreateSessionFromData call
                    if (this.storedSession.IsGuestCheckout)
                    {
                        try
                        {
                            await this.SessionServiceAccessor.GetSessionResourceData<PXInternal.PaymentSession>(
                                sessionId: session.Id,
                                traceActivityId: traceActivityId);
                        }
                        catch (Exception ex)
                        {
                            if (ex is KeyNotFoundException && ex.Message.ToLower().Contains(V7.Constants.PXServiceIntegrationErrorCodes.InvalidOrExpiredSessionId.ToLower()))
                            {
                                await TryWithRetry(async () =>
                                {
                                    await this.SessionServiceAccessor.CreateSessionFromData<PXInternal.PaymentSession>(
                                    sessionId: session.Id,
                                    sessionData: this.storedSession,
                                    traceActivityId: traceActivityId);
                                });
                            }
                        }
                    }
                    else
                    {
                        this.storedSession.HandlerVersion = HandlerVersion;
                        await TryWithRetry(async () =>
                        {
                            await this.SessionServiceAccessor.CreateSessionFromData<PXInternal.PaymentSession>(
                                    sessionId: session.Id,
                                    sessionData: this.storedSession,
                                    traceActivityId: traceActivityId);
                        });
                    }

                    if (paymentInstrument != null && exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXEnablePSD2PaymentInstrumentSession, StringComparer.OrdinalIgnoreCase))
                    {
                        PaymentInstrumentSession piSession = new PaymentInstrumentSession(this.storedSession.Id, accountId, paymentInstrument.PaymentInstrumentDetails?.RequiredChallenge);
                        await CallSafetyNetOperation(
                            async () =>
                            {
                                // performing an upsert operation to update the PaymentInstrumentSession object if it already exists
                                await this.SessionServiceAccessor.UpdateSessionResourceData<PaymentInstrumentSession>(
                                    sessionId: V7.Constants.Prefixes.PaymentInstrumentSessionPrefix + paymentInstrument.PaymentInstrumentId,
                                    newSessionData: piSession,
                                    traceActivityId: traceActivityId);
                            },
                            traceActivityId);
                    }

                    // Should attest for Paymod when Moto = true, or if PIMS send a challenge but PX is unable to fulfill it.
                    bool updateSessionForRedeem = session.RedeemRewards && exposedFlightFeatures?.Contains(Flighting.Features.PXSkipDuplicatePostProcessForMotoAndRewards) == true;

                    // If we are enabling challenges for MOTO, then skip last session update
                    bool updateMOTOSession = session.IsMOTO;
                    if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXEnableChallengesForMOTO))
                    {
                        updateMOTOSession = false;
                    }

                    if (updateMOTOSession || updateSessionForRedeem || (piRequires3ds2Authentication && session.IsChallengeRequired == false))
                    {
                        await this.UpdateSessionOnFinally(this.storedSession.Id, accountId, true, traceActivityId);
                    }
                }
            }
        }

        private async Task<PaymentSession> InternalCreatePaymentSessionWithRequestId(
            string accountId,
            PaymentSessionData paymentSessionData,
            DeviceChannel deviceChannel,
            string emailAddress,
            List<string> exposedFlightFeatures,
            EventTraceActivity traceActivityId,
            TestContext testContext,
            string isMotoAuthorized = null,
            string tid = null,
            PaymentExperienceSetting setting = null,
            string userId = null,
            bool isGuestUser = false,
            V7.Contexts.RequestContext requestContext = null)
        {
            PaymentSession session = new PaymentSession(paymentSessionData);
            PaymentInstrument paymentInstrument = null;

            try
            {
                // Step 1 GetExtendedPI from PIMS to see if PI is creditcard/googlePay or not. We are using this API instead of GetPI to bypass Authorization checks on the PI
                if (await CallSafetyNetOperation(
                async () =>
                {
                    paymentInstrument = await this.PimsAccessor.GetExtendedPaymentInstrument(
                        piid: session.PaymentInstrumentId,
                        traceActivityId: traceActivityId,
                        paymentSessionData.Partner,
                        exposedFlightFeatures: exposedFlightFeatures);
                },
                traceActivityId,
                exposedFeatures: exposedFlightFeatures,
                excludeErrorFeatureFormat: "PSD2SafetyNet-GetPIExt-{0}-{1}"))
                {
                    return GetSafetyNetPaymentSession(paymentSessionData);
                }

                if (PaymentMethodNotRequiresPaymentChallenge(paymentInstrument, exposedFlightFeatures))
                {
                    session.IsChallengeRequired = false;
                    session.Id = Guid.NewGuid().ToString();
                    session.ChallengeStatus = PaymentChallengeStatus.NotApplicable;
                    session.Signature = session.GenerateSignature();
                    return session;
                }

                if (ExcludeJCBChallenge(paymentInstrument, exposedFlightFeatures))
                {
                    session.IsChallengeRequired = false;
                    session.ChallengeStatus = PaymentChallengeStatus.NotApplicable;
                    return session;
                }

                // Step 2 Create a PayerAuth's PaymentSession object
                var payerAuthPaymentSession = new PayerAuth.PaymentSession(
                    accountId: accountId,
                    id: null,
                    data: paymentSessionData,
                    paymentInstrument: paymentInstrument,
                    deviceChannel: deviceChannel,
                    piRequiresAuthentication: true);

                PayerAuth.PaymentSessionResponse resp = null;

                if (await CallSafetyNetOperation(
                    async () =>
                    {
                        resp = await this.PayerAuthServiceAccessor.CreatePaymentSessionId(
                            paymentSessionData: payerAuthPaymentSession,
                            traceActivityId: traceActivityId);
                    },
                    traceActivityId,
                    exposedFeatures: exposedFlightFeatures,
                    excludeErrorFeatureFormat: "PSD2SafetyNet-CreatePS-{0}-{1}"))
                {
                    return GetSafetyNetPaymentSession(paymentSessionData);
                }

                payerAuthPaymentSession.Id = resp.PaymentSessionId;
                session.Id = resp.PaymentSessionId;

                // Step 3 create a short term session for the challenge
                this.storedSession = new PXInternal.PaymentSession(
                    exposedFlightFeatures: exposedFlightFeatures,
                    payerAuthApiVersion: GlobalConstants.PayerAuthApiVersions.V3,
                    accountId: accountId,
                    payerAuthPaymentSession: payerAuthPaymentSession,
                    billableAccountId: paymentSessionData.BillableAccountId,
                    classicProduct: paymentSessionData.ClassicProduct,
                    emailAddress: emailAddress,
                    language: paymentSessionData.Language,
                    requestId: requestContext.RequestId,
                    tenantId: requestContext.TenantId);

                session.IsChallengeRequired = true;
                session.ChallengeStatus = PaymentChallengeStatus.Unknown;

                if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXEnableGettingStoredSessionForChallengeDescriptionsController, StringComparer.OrdinalIgnoreCase))
                {
                    this.storedSession.ChallengeType = session.ChallengeType;
                    this.storedSession.PiRequiresAuthentication = session.IsChallengeRequired;
                }

                // Place holder for generate signature
                session.Signature = session.GenerateSignature();
                return session;
            }
            finally
            {
                if (this.storedSession != null)
                {
                    this.storedSession.HandlerVersion = HandlerVersion;
                    await TryWithRetry(async () =>
                    {
                        await this.SessionServiceAccessor.CreateSessionFromData<PXInternal.PaymentSession>(
                                sessionId: session.Id,
                                sessionData: this.storedSession,
                                traceActivityId: traceActivityId);
                    });

                    if (paymentInstrument != null && exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXEnablePSD2PaymentInstrumentSession, StringComparer.OrdinalIgnoreCase))
                    {
                        PaymentInstrumentSession piSession = new PaymentInstrumentSession(this.storedSession.Id, accountId, paymentInstrument.PaymentInstrumentDetails?.RequiredChallenge);
                        await CallSafetyNetOperation(
                            async () =>
                            {
                                // performing an upsert operation to update the PaymentInstrumentSession object if it already exists
                                await this.SessionServiceAccessor.UpdateSessionResourceData<PaymentInstrumentSession>(
                                    sessionId: V7.Constants.Prefixes.PaymentInstrumentSessionPrefix + paymentInstrument.PaymentInstrumentId,
                                    newSessionData: piSession,
                                    traceActivityId: traceActivityId);
                            },
                            traceActivityId);
                    }

                    // Should attest if PIMS send a challenge but PX is unable to fulfill it.
                    if (session.IsChallengeRequired == false)
                    {
                        await this.UpdateSessionOnFinally(this.storedSession.Id, accountId, true, traceActivityId);
                    }
                }
            }
        }

        private async Task<BrowserFlowContext> Authenticate(
            PayerAuth.ThreeDSMethodCompletionIndicator threeDSMethodCompletionIndicator,
            List<string> exposedFlightFeatures,
            EventTraceActivity traceActivityId,
            IEnumerable<KeyValuePair<string, string>> piQueryParams = null)
        {
            var paymentSessionId = string.Empty;
            var authenticationVerified = false;
            var accountId = string.Empty;

            if (!string.IsNullOrWhiteSpace(this.storedSession.AccountId))
            {
                accountId = this.storedSession.AccountId;
            }
            else if (!string.IsNullOrWhiteSpace(this.storedSession.PaymentInstrumentAccountId))
            {
                accountId = this.storedSession.PaymentInstrumentAccountId;
            }
            else if (!string.IsNullOrWhiteSpace(this.storedSession.BillableAccountId))
            {
                accountId = this.storedSession.BillableAccountId;
            }
            else if (!string.IsNullOrWhiteSpace(this.storedSession.CommercialAccountId))
            {
                accountId = this.storedSession.CommercialAccountId;
            }

            try
            {
                // 1. Create an AuthRequest object from stored session
                PayerAuth.AuthenticationRequest authRequest = new PayerAuth.AuthenticationRequest()
                {
                    PaymentSession = new PayerAuth.PaymentSession(this.storedSession),
                    BrowserInfo = this.storedSession.BrowserInfo,
                    ThreeDSServerTransId = this.storedSession.MethodData.ThreeDSServerTransID,
                    ThreeDSMethodCompletionIndicator = threeDSMethodCompletionIndicator,
                    AcsChallengeNotificationUrl = string.Format(
                        "{0}/paymentSessions/{1}/NotifyThreeDSChallengeCompleted",
                        this.PifdBaseUrl,
                        this.storedSession.Id),
                    RiskChallengIndicator = this.storedSession.IsGuestCheckout ? PayerAuth.RiskChallengeIndicator.ChallengeRequestedPreference : PayerAuth.RiskChallengeIndicator.NoPreference,
                    MessageVersion = GlobalConstants.PSD2Constants.DefaultBrowserRequestMessageVersion
                };

                // 2. Call PayerAuth.Authenticate
                var authResponse = await this.PayerAuthServiceAccessor.Authenticate(authRequest, traceActivityId);
                this.storedSession.TransactionStatus = authResponse.TransactionStatus;
                this.storedSession.TransactionStatusReason = authResponse.TransactionStatusReason;

                // 3. Return if challenge is not required.
                PaymentSession paymentSession = new PaymentSession(this.storedSession);
                paymentSessionId = paymentSession.Id;
                paymentSession.Signature = paymentSession.GenerateSignature();

                // TODO : Remove after Fraud Investigation
                SllWebLogger.TraceServerMessage("Authenticate Browser", null, paymentSessionId, authResponse?.TransactionStatus != null ? "TransStatus: " + authResponse.TransactionStatus.ToString() : "No transStatus available", EventLevel.Informational);

                var mappedStatus = GetMappedStatusForAuthentication(
                    transStatus: authResponse.TransactionStatus,
                    isMoto: this.storedSession.IsMOTO,
                    additionalInputs: new List<string>() { authResponse.TransactionStatusReason.ToString() },
                    exposedFlights: this.storedSession.ExposedFlightFeatures);

                paymentSession.ChallengeStatus = mappedStatus;

                // TODO : Remove after Fraud Investigation
                SllWebLogger.TraceServerMessage("Authenticate Browser", null, paymentSessionId, "MappedStatus: " + mappedStatus.ToString(), EventLevel.Informational);

                if (mappedStatus != PaymentChallengeStatus.Unknown)
                {
                    this.storedSession.ChallengeStatus = mappedStatus;

                    if (PaymentSessionsHandler.IsAuthenticationVerified(mappedStatus))
                    {
                        await this.PostProcessOnSuccess(
                            traceActivityId: traceActivityId,
                            piQueryParams: piQueryParams);
                    }

                    authenticationVerified = PaymentSessionsHandler.IsAuthenticationVerified(mappedStatus);

                    // TODO : Remove after Fraud Investigation
                    SllWebLogger.TraceServerMessage("Authenticate Browser", null, paymentSessionId, "AuthenticationVerified: " + authenticationVerified.ToString(), EventLevel.Informational);

                    return new BrowserFlowContext()
                    {
                        IsFingerPrintRequired = false,
                        IsAcsChallengeRequired = false,
                        PaymentSession = paymentSession,
                        CardHolderInfo = authResponse.CardHolderInfo,
                    };
                }

                // 4. Update stored session with AuthResponse
                this.storedSession.AuthenticationResponse = authResponse;

                // 5. Calculate the Form Data to Return for Challenge Scenario.
                ChallengeRequest creq = new ChallengeRequest
                {
                    ThreeDSServerTransID = authResponse.ThreeDSServerTransactionId,
                    AcsTransID = authResponse.AcsTransactionId,
                    MessageType = "CReq",
                    MessageVersion = authResponse.MessageVersion ?? GlobalConstants.PSD2Constants.FallbackMessageVersion,
                    ChallengeWindowSize = this.storedSession.BrowserInfo.ChallengeWindowSize
                };

                ThreeDSSessionData threeDSSessionData = new ThreeDSSessionData
                {
                    ThreeDSServerTransID = authResponse.ThreeDSServerTransactionId,
                    AcsTransID = authResponse.AcsTransactionId,
                };

                paymentSession.ChallengeStatus = PaymentChallengeStatus.Unknown;

                WindowSize wz = null;
                ChallengeWindowSizes.TryGetValue(this.storedSession.BrowserInfo.ChallengeWindowSize, out wz);

                if (wz == null)
                {
                    throw new KeyNotFoundException(
                        string.Format(
                            "Not matching window size found for {0}",
                            this.storedSession.BrowserInfo.ChallengeWindowSize));
                }

                return new BrowserFlowContext()
                {
                    IsFingerPrintRequired = false,
                    IsAcsChallengeRequired = true,
                    FormInputThreeDSSessionData = ThreeDSUtils.EncodeUrl(ThreeDSUtils.EncodeObjectToBase64(threeDSSessionData)),
                    FormInputCReq = ThreeDSUtils.EncodeUrl(ThreeDSUtils.EncodeObjectToBase64(creq)),
                    FormActionURL = authResponse.AcsUrl,
                    CardHolderInfo = authResponse.CardHolderInfo,
                    PaymentSession = paymentSession,
                    ChallengeWindowSize = wz,
                    TransactionSessionId = authResponse.TransactionSessionId
                };
            }
            catch (Exception ex)
            {
                authenticationVerified = true;
                this.storedSession.IsSystemError = true;
                throw ex;
            }
            finally
            {
                // TODO : Remove after Fraud Investigation
                SllWebLogger.TraceServerMessage("Authenticate Browser", null, paymentSessionId, "AuthenticationVerified In Finally: " + authenticationVerified, EventLevel.Informational);

                await this.UpdateSessionOnFinally(paymentSessionId, accountId, authenticationVerified, traceActivityId);
            }
        }

        private async Task<BrowserFlowContext> AuthenticateThreeDSOne(
           string cvvToken,
           IEnumerable<KeyValuePair<string, string>> piQueryParams,
           EventTraceActivity traceActivityId,
           string userId)
        {
            // 1. Create an AuthRequest object from stored session
            PayerAuth.AuthenticationRequest authRequest = new PayerAuth.AuthenticationRequest()
            {
                PaymentSession = new PayerAuth.PaymentSession(this.storedSession)
                {
                    CvvToken = cvvToken,
                    UserId = userId
                },
                BrowserInfo = this.storedSession.BrowserInfo,
                AcsChallengeNotificationUrl = string.Format(
                    "{0}/paymentSessions/{1}/BrowserNotifyThreeDSOneChallengeCompleted",
                    this.PifdBaseUrl,
                    this.storedSession.Id)
            };

            // 2. Call PayerAuth.Authenticate
            var authResponse = await this.PayerAuthServiceAccessor.AuthenticateThreeDSOne(authRequest, traceActivityId);

            // 3. Return if challenge is not required.
            PaymentSession paymentSession = new PaymentSession(this.storedSession);
            paymentSession.Signature = paymentSession.GenerateSignature();

            var mappedStatus = GetMappedStatusForThreeDSOneAuthentication(authResponse.TransactionStatus);

            paymentSession.ChallengeStatus = mappedStatus;
            this.storedSession.ChallengeStatus = mappedStatus;
            if (mappedStatus != PaymentChallengeStatus.Unknown)
            {
                // In the case of 3DS 1.0, we should never get a status other than Unknown
                if (PaymentSessionsHandler.IsAuthenticationVerified(mappedStatus))
                {
                    await this.PostProcessOnSuccess(
                        traceActivityId: traceActivityId,
                        piQueryParams: piQueryParams);
                }

                return new BrowserFlowContext()
                {
                    IsAcsChallengeRequired = false,
                    PaymentSession = paymentSession,
                    CardHolderInfo = authResponse.CardHolderInfo,
                };
            }

            // 4. Update stored session with AuthResponse
            this.storedSession.AuthenticationResponse = authResponse;
            await this.SessionServiceAccessor.UpdateSessionResourceData<PXInternal.PaymentSession>(
                sessionId: this.storedSession.Id,
                newSessionData: this.storedSession,
                traceActivityId: traceActivityId);

            // 5. Calculate the Form Data to Return for Challenge Scenario.
            WindowSize wz = null;
            ChallengeWindowSizes.TryGetValue(this.storedSession.BrowserInfo.ChallengeWindowSize, out wz);

            return new BrowserFlowContext()
            {
                IsAcsChallengeRequired = true,
                FormInputCReq = authResponse.AcsSignedContent,
                FormActionURL = authResponse.AcsUrl,
                FormPostAcsURL = authResponse.IsFormPostAcsUrl,
                FormFullPageRedirectAcsURL = authResponse.IsFullPageRedirect,
                CardHolderInfo = authResponse.CardHolderInfo,
                PaymentSession = paymentSession,
                ChallengeWindowSize = wz
            };
        }

        private async Task<bool> ValidatePI(
            EventTraceActivity traceActivityId,
            IEnumerable<KeyValuePair<string, string>> piQueryParams = null)
        {
            bool isValidatePiSuccessful = true;
            await CallSafetyNetOperation(
                operation: async () =>
                {
                    // For ValidatePIOnAttachChallenge, call ValidatePaymentInstrument on PIMS only when the payment (session) is for a pre-order or for Zero amount
                    if (this.storedSession.HasPreOrder
                        || this.storedSession.Amount == 0)
                    {
                        var validatePiResponse = await this.PimsAccessor.ValidatePaymentInstrument(
                            accountId: this.storedSession.PaymentInstrumentAccountId,
                            piid: this.storedSession.PaymentInstrumentId,
                            payload: new ValidatePaymentInstrument(
                                this.storedSession.Id,
                                new RiskData()
                                {
                                    UserInfo = new UserInfo()
                                    {
                                        Email = this.storedSession.EmailAddress
                                    }
                                }),
                            traceActivityId: traceActivityId,
                            queryParams: piQueryParams);

                        isValidatePiSuccessful = !string.Equals(validatePiResponse.Result, V7.Constants.ValidateResultMessages.Failed, StringComparison.OrdinalIgnoreCase);
                    }
                },
                traceActivityId,
                exposedFeatures: this.storedSession?.ExposedFlightFeatures,
                excludeErrorFeatureFormat: "PSD2SafetyNet-ValidatePI-{0}-{1}");

            return isValidatePiSuccessful;
        }

        private async Task PostProcessOnSuccess(
            EventTraceActivity traceActivityId,
            IEnumerable<KeyValuePair<string, string>> piQueryParams = null)
        {
            // For caas/paas flow, there is no need to link session to PI. LinkSession is now only used for first party legacy flow
            if (!string.IsNullOrEmpty(this.storedSession.RequestId))
            {
                return;
            }

            // 1. Call ValidatePI if PreOrder or MIT (subscription setup) to notify PIMS->PayMod
            await this.ValidatePI(traceActivityId, piQueryParams);

            // 2. Save session against PI
            await CallSafetyNetOperation(
                operation: async () =>
                {
                    await this.PimsAccessor.LinkSession(
                        accountId: this.storedSession.PaymentInstrumentAccountId,
                        piid: this.storedSession.PaymentInstrumentId,
                        payload: new LinkSession(this.storedSession.Id),
                        traceActivityId: traceActivityId,
                        queryParams: piQueryParams);
                },
                traceActivityId,
                exposedFeatures: this.storedSession.ExposedFlightFeatures,
                excludeErrorFeatureFormat: "PSD2SafetyNet-LinkSessionToPI-{0}-{1}");
        }

        private async Task PostProcessOnSuccessIndiaThreeDS(
            EventTraceActivity traceActivityId,
            string sessionId,
            IEnumerable<KeyValuePair<string, string>> piQueryParams = null)
        {
            await CallSafetyNetOperation(
                operation: async () =>
                {
                    await this.PimsAccessor.LinkSession(
                        accountId: this.storedSession.PaymentInstrumentAccountId,
                        piid: this.storedSession.PaymentInstrumentId,
                        payload: new LinkSession(sessionId),
                        traceActivityId: traceActivityId,
                        queryParams: piQueryParams);
                },
                traceActivityId,
                exposedFeatures: this.storedSession.ExposedFlightFeatures,
                excludeErrorFeatureFormat: "PSD2SafetyNet-LinkSessionToPI-{0}-{1}");
        }

        private async Task CachedGetStoredSession(
            string sessionId,
            EventTraceActivity traceActivityId)
        {
            // 1. If local copy of the stored session exists but is not the corect one (should never happen),
            // clear it and log telemetry
            if (this.storedSession != null && this.storedSession.Id != sessionId)
            {
                // TODO: Log telemetry but continue to service the request
                this.storedSession = null;
            }

            // 2. Get stored session if local copy is not available
            if (this.storedSession == null)
            {
                this.storedSession = await this.SessionServiceAccessor.GetSessionResourceData<PXInternal.PaymentSession>(
                    sessionId: sessionId,
                    traceActivityId: traceActivityId);
            }
        }

        // The following method will only be called if the payment method type is a credit card
        private PaymentChallengeStatus ValidateACSSignedContent(
            AuthenticationRequest authRequest,
            PaymentChallengeStatus mappedStatus,
            List<string> exposedFlightFeatures,
            PayerAuth.AuthenticationResponse payerAuthResponse,
            TestContext testContext,
            EventTraceActivity traceActivityId,
            string sessionId)
        {
            string version = GetLatestAvailablePSD2SettingVersion(exposedFlightFeatures);

            // Only versions above V17 will have the attribute caRootCertificates, which is prefered to parse through.
            if (int.Parse(version.Substring(1, 2)) <= V7.Constants.RootCertVersion.UpdatedVersionInt17)
            {
                version = V7.Constants.RootCertVersion.UpdatedVersion17;
            }

            string paymentMethodType = this.storedSession.PaymentMethodType;
            try
            {
                string paymentClientConfig = File.ReadAllText(
                                            Path.Combine(
                                                AppDomain.CurrentDomain.BaseDirectory,
                                                string.Format(@"App_Data\PSD2Config\certs-{0}\DsCertificates.json", version)));

                PaymentClientSettingsData settingsConfig = JsonConvert.DeserializeObject<PaymentClientSettingsData>(paymentClientConfig);

                if (!string.IsNullOrEmpty(paymentMethodType))
                {
                    DirectoryServerData dsInfo = null;

                    if (!settingsConfig.DirectoryServerInfo.TryGetValue(paymentMethodType, out dsInfo))
                    {
                        mappedStatus = PaymentChallengeStatus.Failed;

                        SllWebLogger.TracePXServiceIntegrationError(
                            V7.Constants.ServiceNames.DsCertificateValidation,
                            IntegrationErrorCode.DSInfoNotFound,
                            $"{V7.Constants.PSD2ValidationErrorMessage.PaymentMethodTypeIssue} Payment Method Type: {paymentMethodType}, SessionId: {sessionId} ",
                            traceActivityId.ToString());
                        return mappedStatus;
                    }

                    List<string> certs = dsInfo.CaRootCertificates ?? new List<string> { dsInfo.CaRootCertificate };

                    PaymentPSD2CertificatesValidator certVal = new PaymentPSD2CertificatesValidator(payerAuthResponse.AcsSignedContent, certs, testContext, sessionId);

                    if (!certVal.VerifySignature(traceActivityId))
                    {
                        // Intentially kept status as Unknown in order to mitigate blockage.
                        // Errors will be logged and data regarding this change will be monitored.
                        // If there are not many unexpected errors, the status will be switched back to Failed
                        // Update PaymentSessionsHandlerTests (ValidatePaymentChallenge_CheckCertificateValidation_UnknownOrFailedAsync) accordingly.
                        mappedStatus = PaymentChallengeStatus.Unknown;
                        SllWebLogger.TracePXServiceIntegrationError(
                            V7.Constants.ServiceNames.DsCertificateValidation,
                            IntegrationErrorCode.InvalidBuild,
                            $"Mapped Status intentially passed as Unknown, but there was an issue in PaymentPSD2CertificatesValidator.cs. Check the logs to determine the error. " +
                            $"{V7.Constants.PSD2ValidationErrorMessage.PaymentMethodTypeIssue} Payment Method Type: {paymentMethodType}, SessionId: {sessionId}",
                            traceActivityId.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                mappedStatus = PaymentChallengeStatus.Failed;

                SllWebLogger.TracePXServiceException(
                    ex.ToString(),
                    traceActivityId);
            }

            return mappedStatus;
        }

        private async Task<bool> SafetyNetUpdateSession(EventTraceActivity traceActivityId)
        {
            return await CallSafetyNetOperation(
                async () =>
                {
                    await this.SessionServiceAccessor.UpdateSessionResourceData<PXInternal.PaymentSession>(
                    sessionId: this.storedSession.Id,
                    newSessionData: this.storedSession,
                    traceActivityId: traceActivityId);
                },
                traceActivityId,
                exposedFeatures: this.storedSession.ExposedFlightFeatures,
                excludeErrorFeatureFormat: "PSD2SafetyNet-UpdateSessionResourceData-{0}-{1}");
        }

        private PaymentSession AddValidatePIOnAttachChallengeToSession(
           PaymentSession session,
           string accountId,
           PaymentSessionData paymentSessionData,
           PimsModel.V4.PaymentInstrument piDetails,
           DeviceChannel deviceChannel,
           List<string> exposedFlightFeatures,
           string emailAddress)
        {
            session.IsChallengeRequired = true;
            session.Id = Guid.NewGuid().ToString();
            session.ChallengeStatus = PaymentChallengeStatus.Unknown;
            session.ChallengeType = V7.Constants.ChallengeTypes.ValidatePIOnAttachChallenge;
            session.Signature = session.GenerateSignature();
            var payerAuthPaymentSession = new PayerAuth.PaymentSession(
                accountId: accountId,
                id: null,
                data: paymentSessionData,
                paymentInstrument: piDetails,
                deviceChannel: deviceChannel,
                piRequiresAuthentication: false);

            payerAuthPaymentSession.Id = Guid.NewGuid().ToString() + "_init";
            this.storedSession = new PXInternal.PaymentSession(
                exposedFlightFeatures: exposedFlightFeatures,
                payerAuthApiVersion: GlobalConstants.PayerAuthApiVersions.V3,
                accountId: accountId,
                payerAuthPaymentSession: payerAuthPaymentSession,
                billableAccountId: paymentSessionData.BillableAccountId,
                classicProduct: paymentSessionData.ClassicProduct,
                emailAddress: emailAddress,
                language: paymentSessionData.Language);

            return session;
        }
    }
}