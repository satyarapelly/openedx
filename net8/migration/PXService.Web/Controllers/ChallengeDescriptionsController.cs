// <copyright file="ChallengeDescriptionsController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.Pidl.Localization;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLGeneration;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Newtonsoft.Json;
    using PaymentChallenge.Model;
    using PayerAuth = PXService.Model.PayerAuthService;

    public class ChallengeDescriptionsController : ProxyController
    {
        private readonly HashSet<string> upiPaymentMethodSet = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            Constants.PaymentMethodType.UPI,
            Constants.PaymentMethodType.UPIQr,
            Constants.PaymentMethodType.UPICommercial,
            Constants.PaymentMethodType.UPIQrCommercial,
        };

        /// <summary>
        /// Returns a challenge PIDL for the given challenge type.
        /// ("/GetById" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>ChallengeDescriptions</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/ChallengeDescriptions/GetById</url>
        /// <param name="accountId">User's account id</param>
        /// <param name="type">The challenge type, e.g. cvv</param>
        /// <param name="language">Code specifying the language for PIDL localization</param>
        /// <param name="partner">The name of the partner</param>
        /// <response code="200">List&lt;PIDLResource&gt;</response>
        /// <returns>Returns challenge PIDL for the given type</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Needs to be an instance method for Route action selection")]
        [HttpGet]
        public async Task<List<PIDLResource>> GetById(
            string accountId,
            string type,
            string language = null,
            string partner = Constants.ServiceDefaults.DefaultPartnerName)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddTracingProperties(accountId, null, null, null, null);
            this.Request.AddPartnerProperty(partner?.ToLower());

            // Use Partner Settings if enabled for the partner
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(Constants.Component.HandlePurchaseRiskChallenge);
            this.EnableFlightingsInPartnerSetting(setting, string.Empty);

            accountId = accountId + string.Empty;
            List<PIDLResource> retVal = PIDLResourceFactory.Instance.GetChallengeDescriptions(type, language, partner, setting, this.ExposedFlightFeatures);

            FeatureContext featureContext = new FeatureContext(
                country: null,
                partner,
                Constants.DescriptionTypes.ChallengeDescription,
                null,
                scenario: null,
                language,
                null,
                this.ExposedFlightFeatures,
                null,
                typeName: type,
                tokenizationPublicKey: await this.GetTokenizationPublicKey(traceActivityId),
                tokenizationServiceUrls: this.Settings.TokenizationServiceAccessor.GetTokenizationServiceUrls());

            PostProcessor.Process(retVal, PIDLResourceFactory.FeatureFactory, featureContext);
            return retVal;
        }

        /// <summary>
        /// Returns a PIDL for completing the given challenge type on the given piid.
        /// These PIDLs will be used by Webblends and Cart for completing challenges during purchase.
        /// ("/GetByTypePiidAndSessionId" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>ChallengeDescriptions</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/ChallengeDescriptions/GetByTypePiidAndSessionId</url>
        /// <param name="accountId">User's account id</param>
        /// <param name="piid">Id of payment instrument that requires challenge</param>
        /// <param name="type">The challenge type, e.g. cvv</param>
        /// <param name="sessionId">Identity the user's purchase session</param>
        /// <param name="language">Code specifying the language for PIDL localization</param>
        /// <param name="partner">The name of the partner</param>
        /// <param name="scenario">The name of the pidl scenario</param>
        /// <throws>HttpResponseException if the payment instrument cannot be found</throws>
        /// <response code="200">List&lt;PIDLResource&gt;</response>
        /// <returns>Returns challenge PIDL for the given type and specfic to the given piid</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Needs to be an instance method for Route action selection")]
        [HttpGet]
        public async Task<ActionResult<List<PIDLResource>>> GetByTypePiidAndSessionId(
            string accountId,
            string piid,
            string type,
            string sessionId,
            string language = null,
            string partner = Constants.ServiceDefaults.DefaultPartnerName,
            string scenario = null)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddPartnerProperty(partner?.ToLower());

            // Use Partner Settings if enabled for the partner
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(Constants.Component.HandlePurchaseRiskChallenge);
            this.EnableFlightingsInPartnerSetting(setting, string.Empty);

            ActionResult<List<PIDLResource>> retVal = await this.GetChallengeDescriptionsByTypePiidAndSessionId(
                accountId: accountId,
                piid: piid,
                type: type,
                sessionId: sessionId,
                language: language,
                partner: partner,
                scenario: scenario,
                setting);
            if (retVal.Result != null)
            {
                return retVal.Result;
            }

            List<PIDLResource> resources = retVal.Value;

            FeatureContext featureContext = new FeatureContext(
                country: null,
                partner,
                Constants.DescriptionTypes.ChallengeDescription,
                null,
                scenario: null,
                language,
                null,
                this.ExposedFlightFeatures,
                featureConfigs: setting?.Features,
                null,
                typeName: type,
                isGuestAccount: GuestAccountHelper.IsGuestAccount(this.Request),
                tokenizationPublicKey: await this.GetTokenizationPublicKey(traceActivityId),
                tokenizationServiceUrls: this.Settings.TokenizationServiceAccessor.GetTokenizationServiceUrls());

            PostProcessor.Process(resources, PIDLResourceFactory.FeatureFactory, featureContext);
            return resources;
        }

        /// <summary>
        /// Returns a PIDL for completing the given challenge type on the given piid.
        /// These PIDLs will be used by BingAds for India 3DS PI Reauth flow for CVV challenge before purchase, where sessionId does not exist.
        /// ("/GetByTypeAndPiid" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>ChallengeDescriptions</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/ChallengeDescriptions/GetByTypeAndPiid</url>
        /// <param name="accountId">User's account id</param>
        /// <param name="piid">Id of payment instrument that requires challenge</param>
        /// <param name="type">The challenge type, e.g. cvv</param>
        /// <param name="language">Code specifying the language for PIDL localization</param>
        /// <param name="partner">The name of the partner</param>
        /// <throws>HttpResponseException if the payment instrument cannot be found</throws>
        /// <response code="200">List&lt;PIDLResource&gt;</response>
        /// <returns>Returns challenge PIDL for the given type and specfic to the given piid</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Needs to be an instance method for Route action selection")]
        [HttpGet]
        public async Task<ActionResult<List<PIDLResource>>> GetByTypeAndPiid(
            string accountId,
            string piid,
            string type,
            string language = null,
            string partner = Constants.ServiceDefaults.DefaultPartnerName)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            this.Request.AddPartnerProperty(partner?.ToLower());

            // Use Partner Settings if enabled for the partner
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(Constants.Component.HandlePurchaseRiskChallenge);
            this.EnableFlightingsInPartnerSetting(setting, string.Empty);

            ActionResult<List<PIDLResource>> retVal = await this.GetChallengeDescriptionsByTypePiidAndSessionId(
                accountId: accountId,
                piid: piid,
                type: type,
                sessionId: null,
                language: language,
                partner: partner,
                scenario: null,
                setting);

            if (retVal.Result != null)
            {
                return retVal.Result;
            }

            List<PIDLResource> resources = retVal.Value;

            FeatureContext featureContext = new FeatureContext(
                country: null,
                partner,
                Constants.DescriptionTypes.ChallengeDescription,
                null,
                scenario: null,
                language,
                null,
                this.ExposedFlightFeatures,
                setting?.Features,
                typeName: type,
                tokenizationPublicKey: await this.GetTokenizationPublicKey(traceActivityId),
                tokenizationServiceUrls: this.Settings.TokenizationServiceAccessor.GetTokenizationServiceUrls());

            PostProcessor.Process(resources, PIDLResourceFactory.FeatureFactory, featureContext);
            return resources;
        }

        /// <summary>
        /// Returns the PIDL those PIDLs will be used to challenge the user for payment (requiring fingreprinting for a transaction),
        /// ("/GetPaymentChallenge" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>ChallengeDescriptions</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/ChallengeDescriptions/GetPaymentChallenge</url>
        /// <param name="accountId">User's account id</param>
        /// <param name="paymentSessionOrData">paymentSession | paymentSessionData, the user is allow to send payment session or the data to create payment session</param>
        /// <param name="timezoneOffset">time zone offset</param>
        /// <throws>HttpResponseException if the payment instrument cannot be found</throws>
        /// <response code="200">List&lt;PIDLResource&gt;</response>
        /// <returns>Returns challenge PIDL for the given type and specfic to the given piid</returns>
        [HttpGet]
        public async Task<HttpResponseMessage> GetPaymentChallenge(
            string accountId,
            string paymentSessionOrData,
            string timezoneOffset = null)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            List<PIDLResource> resources = new List<PIDLResource>();
            if (string.IsNullOrEmpty(timezoneOffset))
            {
                timezoneOffset = "0";
            }

            PaymentSession paymentSession = null;
            PaymentSessionData paymentSessionData = null;
            try
            {
                paymentSession = JsonConvert.DeserializeObject<PaymentSession>(paymentSessionOrData);
            }
            catch
            {
                try
                {
                    paymentSessionData = JsonConvert.DeserializeObject<PaymentSessionData>(paymentSessionOrData);
                }
                catch (Exception ex)
                {
                    throw new ValidationException(ErrorCode.InvalidRequestData, "paymentSessionOrData is invalid." + ex.Message);
                }
            }

            if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableGettingStoredSessionForChallengeDescriptionsController) && paymentSession != null)
            {
                paymentSession = await this.UpdatePaymentSessionWithStoredSession(paymentSession, traceActivityId);
            }

            // Use Partner Settings if enabled for the partner
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(Constants.Component.HandlePaymentChallenge);
            this.EnableFlightingsInPartnerSetting(setting, string.Empty);

            bool isGuestUser = GuestAccountHelper.IsGuestAccount(this.Request);

            var paymentSessionsHandler = await this.GetVersionBasedPaymentSessionsHandler(traceActivityId);

            // Create PaymentSession If needed.
            if (paymentSessionData != null)
            {
                string isMotoAuthorized = this.Request.GetRequestHeader(GlobalConstants.HeaderValues.IsMotoHeader);
                string tid = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.AadInfo.Tid);
                string userId = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.Puid);

                paymentSession = await paymentSessionsHandler.CreatePaymentSession(
                    accountId: accountId,
                    paymentSessionData: paymentSessionData,
                    deviceChannel: PXService.Model.ThreeDSExternalService.DeviceChannel.Browser,
                    exposedFlightFeatures: this.ExposedFlightFeatures,
                    emailAddress: await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.EmailAddress),
                    traceActivityId: traceActivityId,
                    isMotoAuthorized: isMotoAuthorized,
                    tid: tid,
                    testContext: HttpRequestHelper.GetTestHeader(this.Request.ToHttpRequestMessage()),
                    setting: setting,
                    userId: userId,
                    isGuestUser: isGuestUser);
            }

            this.Request.AddTracingProperties(accountId, paymentSession.PaymentInstrumentId, null, null);

            if (!paymentSession.HasValidSignature())
            {
                throw new ValidationException(ErrorCode.InvalidRequestData, "PaymentSession is altered");
            }

            // Taking Care of 3ds Challenge
            PayerAuth.BrowserInfo browserInfo = await this.CollectBrowserInfo();
            browserInfo.BrowserTZ = timezoneOffset;
            browserInfo.BrowserLanguage = paymentSession.Language ?? GlobalConstants.Defaults.Locale;
            browserInfo.ChallengeWindowSize = paymentSession.ChallengeWindowSize;

            if (paymentSession.IsChallengeRequired && string.Equals(paymentSession.ChallengeType, Constants.ChallengeTypes.India3DSChallenge, StringComparison.OrdinalIgnoreCase))
            {
                PaymentInstrument piDetails = await this.Settings.PIMSAccessor.GetExtendedPaymentInstrument(
                    piid: paymentSession.PaymentInstrumentId,
                    traceActivityId: traceActivityId,
                    paymentSession.Partner,
                    exposedFlightFeatures: this.ExposedFlightFeatures);

                // Add browserInfo stored session
                await paymentSessionsHandler.UpdateSessionResourceData(
                    browserInfo,
                    paymentSession,
                    traceActivityId);

                if (piDetails.IsCreditCard())
                {
                    // Azure and commercialstores do not have a india3ds scenario pidl so only send indiathreeds scenario for those partners that have one
                    string scenario = (PartnerHelper.IsIndiaThreeDSCommercialPartner(paymentSession.Partner)
                                        || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.ThreeDSOne, paymentSession.Country, setting, PXCommon.Constants.DisplayCustomizationDetail.EnableIndia3dsForNonZeroPaymentTransaction)) ? null : Constants.ScenarioNames.IndiaThreeDS;

                    var cvvPidl = PIDLResourceFactory.Instance.GetChallengeDescriptionsForPi(
                        pi: piDetails,
                        type: Constants.ChallengeDescriptionTypes.Cvv,
                        language: paymentSession.Language,
                        partnerName: paymentSession.Partner,
                        sessionId: paymentSession.Id,
                        scenario: scenario,
                        classicProduct: paymentSession.ClassicProduct,
                        session: paymentSession,
                        exposedFlightFeatures: this.ExposedFlightFeatures,
                        setting: setting);

                    // Condition has been added to check the pss is using the template or not. If it is using the template then we need to process the feature context.
                    if (this.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.PXEnableSecureFieldIndia3DSChallenge, StringComparer.OrdinalIgnoreCase)
                        || this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableTokenizationEncryptionOtherOperation, StringComparer.OrdinalIgnoreCase)
                        || this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableTokenizationEncryptionFetchConfigOtherOperation, StringComparer.OrdinalIgnoreCase)
                        || string.Equals(setting?.Template, Constants.TemplateName.DefaultTemplate, StringComparison.OrdinalIgnoreCase))
                    {
                        FeatureContext featureContext = new FeatureContext(
                        country: paymentSession.Country,
                        partner: paymentSession.Partner,
                        resourceType: Constants.ChallengeDescriptionTypes.Cvv,
                        operationType: null,
                        scenario: null,
                        language: paymentSession.Language,
                        paymentMethods: null,
                        this.ExposedFlightFeatures,
                        featureConfigs: setting?.Features,
                        typeName: null,
                        tokenizationPublicKey: await this.GetTokenizationPublicKey(traceActivityId),
                        tokenizationServiceUrls: this.Settings.TokenizationServiceAccessor.GetTokenizationServiceUrls());

                        PostProcessor.Process(cvvPidl, PIDLResourceFactory.FeatureFactory, featureContext);
                    }

                    return this.Request.CreateResponse(HttpStatusCode.OK, cvvPidl);
                }
            }

            // Return PaymentSession PIDL, if the challenge isn't required.
            if (!paymentSession.IsChallengeRequired)
            {
                return this.Request.CreateResponse(PIDLResourceFactory.GetPaymentSessionPidl(paymentSession));
            }

            // call HandlePaymentChallenge which will either get BrowserFlowContext based on ValidatePiOnAttach or GetThreeDSMethodUrl as per original code
            BrowserFlowContext result;

            if (string.Equals(paymentSession.ChallengeType, Constants.ChallengeTypes.UPIChallenge, StringComparison.OrdinalIgnoreCase))
            {
                result = await paymentSessionsHandler.AuthenticateUpiPaymentTxn(
                    accountId: accountId,
                    browserInfo: browserInfo,
                    paymentSession: paymentSession,
                    traceActivityId: traceActivityId,
                    exposedFlightFeatures: this.ExposedFlightFeatures);
            }
            else if (PartnerHelper.IsValidatePIOnAttachEnabled(paymentSession.Partner, this.ExposedFlightFeatures)
                || (paymentSession.PaymentInstrumentId?.StartsWith(V7.Constants.WalletServiceConstants.ApplePayPiidPrefix) ?? false))
            {
                result = await paymentSessionsHandler.HandlePaymentChallenge(
                    accountId,
                    browserInfo,
                    paymentSession,
                    traceActivityId,
                    this.ExposedFlightFeatures,
                    await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.EmailAddress),
                    await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.Puid));
            }
            else
            {
                result = await paymentSessionsHandler.GetThreeDSMethodURL(
                    accountId: accountId,
                    browserInfo: browserInfo,
                    paymentSession: paymentSession,
                    traceActivityId: traceActivityId,
                    exposedFlightFeatures: this.ExposedFlightFeatures);
            }

            List<PIDLResource> pidls = null;
            var testHeader = HttpRequestHelper.GetRequestHeader(PaymentConstants.PaymentExtendedHttpHeaders.TestHeader);

            // Legacy BillDesk need to go through payment challenge, otherwise treat it as failed
            if (!result.IsAcsChallengeRequired && string.Equals(paymentSession.ChallengeType, Constants.ChallengeTypes.LegacyBillDeskPaymentChallenge, StringComparison.OrdinalIgnoreCase))
            {
                result.PaymentSession.ChallengeStatus = PaymentChallengeStatus.Failed;
            }

            string pxAuthUrl = string.Format("{0}/paymentSessions/{1}/authenticate", this.Settings.PifdBaseUrl, paymentSession.Id);
            bool cspFrameEnabled = this.ExposedFlightFeatures.Contains(Flighting.Features.PXPSD2EnableCSPProxyFrame, StringComparer.OrdinalIgnoreCase);
            bool cspSourceUrlEnabled = this.ExposedFlightFeatures.Contains(Flighting.Features.PXPSD2EnableCSPUrlProxyFrame, StringComparer.OrdinalIgnoreCase) || this.ExposedFlightFeatures.Contains(Flighting.Features.PXPSD2EnableCSPUrlProxyFrameWithSanitizedInput, StringComparer.OrdinalIgnoreCase);
            if (result.IsFingerPrintRequired)
            {
                string cspStep = Constants.CSPStepNames.None;
                string formActionUrl = result.FormActionURL;

                // handle iframe auth timeout as failure client action
                var errorDetails = new ServiceErrorResponse { ErrorCode = PaymentChallengeStatus.Failed.ToString(), InnerError = new ServiceErrorResponse { ErrorCode = "authTimeout", Message = PaymentChallengeStatus.Failed.ToString() } };
                if (cspFrameEnabled || cspSourceUrlEnabled)
                {
                    cspStep = Constants.CSPStepNames.Fingerprint;
                    formActionUrl = pxAuthUrl;
                }

                if (cspSourceUrlEnabled)
                {
                    pidls = PIDLResourceFactory.GetThreeDSFingerprintUrlIFrameDescription(formActionUrl, result.FormInputThreeDSMethodData, paymentSession.Id, pxAuthUrl, cspStep, testHeader, errorDetails, this.ExposedFlightFeatures);
                }
                else
                {
                    pidls = PIDLResourceFactory.GetThreeDSFingerprintIFrameDescription(formActionUrl, result.FormInputThreeDSMethodData, paymentSession.Id, pxAuthUrl, cspStep, testHeader, errorDetails, exposedFlightFeatures: this.ExposedFlightFeatures);
                }
            }
            else if (result.IsAcsChallengeRequired)
            {
                if (string.Equals(paymentSession.ChallengeType, Constants.ChallengeTypes.LegacyBillDeskPaymentChallenge, StringComparison.OrdinalIgnoreCase))
                {
                    PIDLResource threeDSRedirectPidl = new PIDLResource()
                    {
                        ClientAction = await this.GetLegacyBillDeskRedirectAndStatusCheckClientAction(result.FormActionURL, paymentSession, traceActivityId, result, setting)
                    };

                    pidls = new List<PIDLResource>() { threeDSRedirectPidl };
                }
                else
                {
                    string cspStep = Constants.CSPStepNames.None;
                    string formActionUrl = result.FormActionURL;

                    if (cspFrameEnabled || cspSourceUrlEnabled)
                    {
                        cspStep = Constants.CSPStepNames.Challenge;
                        formActionUrl = pxAuthUrl;
                    }

                    if (cspSourceUrlEnabled)
                    {
                        pidls = PIDLResourceFactory.GetThreeDSChallengeUrlIFrameDescription(
                            formActionUrl,
                            result.FormInputCReq,
                            result.FormInputThreeDSSessionData,
                            paymentSession.Id,
                            cspStep,
                            result.ChallengeWindowSize.Width,
                            result.ChallengeWindowSize.Height,
                            testHeader,
                            this.ExposedFlightFeatures);
                    }
                    else
                    {
                        pidls = PIDLResourceFactory.GetThreeDSChallengeIFrameDescription(
                            formActionUrl,
                            result.FormInputCReq,
                            result.FormInputThreeDSSessionData,
                            paymentSession.Id,
                            cspStep,
                            result.ChallengeWindowSize.Width,
                            result.ChallengeWindowSize.Height,
                            testHeader);
                    }
                }
            }
            else if (result.PaymentSession.ChallengeStatus == PaymentChallengeStatus.Succeeded
                || result.PaymentSession.ChallengeStatus == PaymentChallengeStatus.ByPassed
                || result.PaymentSession.ChallengeStatus == PaymentChallengeStatus.NotApplicable)
            {
                pidls = new List<PIDLResource>()
                {
                    new PIDLResource()
                    {
                        ClientAction = new ClientAction(ClientActionType.ReturnContext, result.PaymentSession)
                    }
                };
            }
            else if ((PartnerHelper.IsValidatePIOnAttachEnabled(paymentSession.Partner, this.ExposedFlightFeatures)
                || (paymentSession.PaymentInstrumentId?.StartsWith(V7.Constants.WalletServiceConstants.ApplePayPiidPrefix) ?? false))
                && string.Equals(paymentSession.ChallengeType, V7.Constants.ChallengeTypes.ValidatePIOnAttachChallenge, StringComparison.OrdinalIgnoreCase)
                && result.PaymentSession.ChallengeStatus == PaymentChallengeStatus.Failed)
            {
                var validatePIOnAttachFailedResponse = new ServiceErrorResponse
                {
                    ErrorCode = PaymentChallengeStatus.Failed.ToString(),
                    InnerError = new ServiceErrorResponse
                    {
                        ErrorCode = V7.Constants.PSD2ErrorCodes.ValidatePIOnAttachFailed,
                        Message = PaymentChallengeStatus.Failed.ToString()
                    }
                };

                validatePIOnAttachFailedResponse.InnerError.UserDisplayMessage = PidlModelHelper.GetLocalizedString(
                    configText: V7.Constants.PSD2UserDisplayMessages.ValidatePIOnAttachFailed,
                    language: paymentSession.Language ?? GlobalConstants.Defaults.Locale);

                pidls = new List<PIDLResource>()
                {
                    new PIDLResource()
                    {
                        ClientAction = new ClientAction(ClientActionType.Failure, validatePIOnAttachFailedResponse)
                    }
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, pidls);
            }
            else
            {
                return this.Request.CreateResponse(
                HttpStatusCode.BadRequest,
                new ErrorMessage()
                {
                    ErrorCode = V7.Constants.PSD2ErrorCodes.RejectedByProvider,
                    Message = result.PaymentSession.ChallengeStatus.ToString()
                });
            }

            return this.Request.CreateResponse(pidls);
        }

        /// <summary>
        /// Returns a PIDL representing the next action that needs to be taken for the given piid and sessionId.
        /// This will be used by Cart for completing the disclaimer redirect for DPA.
        /// ("/GetByPiidAndSessionId" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>ChallengeDescriptions</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/ChallengeDescriptions/GetByPiidAndSessionId</url>
        /// <param name="accountId">User's account id</param>
        /// <param name="piid">Id of payment instrument that requires action</param>
        /// <param name="sessionId">Identity the user's purchase session</param>
        /// <param name="partner">The name of the partner</param>
        /// <param name="language">Code specifying the language for PIDL localization</param>
        /// <param name="orderId">Identity of user's order</param>
        /// <throws>HttpResponseException if the payment instrument cannot be found</throws>
        /// <returns>Returns a PIDL representing the next action that needs to be taken</returns>
        [HttpGet]
        public async Task<ActionResult<List<PIDLResource>>> GetByPiidAndSessionId(
                string accountId, string piid, string sessionId,
                string partner = null, string language = null, string orderId = null)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddTracingProperties(accountId, piid, null, null, null);

            accountId = accountId + string.Empty;
            PaymentInstrument pi = await this.Settings.PIMSAccessor.GetPaymentInstrument(accountId, piid, traceActivityId);
            if (pi == null)
            {
                return NotFound();
            }

            this.Request.AddTracingProperties(null, null, pi.PaymentMethod.PaymentMethodFamily, pi.PaymentMethod.PaymentMethodType);

            if (string.Equals(pi.PaymentMethod.PaymentMethodFamily, Constants.PaymentMethodFamilyName.OnlineBankTransfer, StringComparison.OrdinalIgnoreCase) && string.Equals(pi.PaymentMethod.PaymentMethodType, Constants.PaymentMethodType.Paysafecard, StringComparison.OrdinalIgnoreCase))
            {
                // Use Partner Settings if enabled for the partner
                PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(Constants.Component.HandlePurchaseRiskChallenge);
                var retVal = PIDLResourceFactory.GetPurchaseConfirmationPidl(pi, sessionId, this.ExposedFlightFeatures, partner, language, orderId, setting: setting);

                FeatureContext featureContext = new FeatureContext(null, partner, "challenge", "renderPidlPage", null, language, null, this.ExposedFlightFeatures, setting?.Features, Constants.PaymentMethodFamilyName.OnlineBankTransfer, Constants.PaymentMethodType.Paysafecard);

                PostProcessor.Process(retVal, PIDLResourceFactory.FeatureFactory, featureContext);
                return retVal;
            }
            else if (string.Equals(pi.PaymentMethod.PaymentMethodFamily, Constants.PaymentMethodFamilyName.RealTimePayments, StringComparison.OrdinalIgnoreCase) &&
                      this.upiPaymentMethodSet.Contains(pi.PaymentMethod.PaymentMethodType))
            {
                List<PIDLResource> retList = new List<PIDLResource>();
                PIDLResource retVal = new PIDLResource();

                // Use Partner Settings if enabled for the partner
                PaymentExperienceSetting settingsForConfirmPayment = this.GetPaymentExperienceSetting(Constants.Component.ConfirmPayment);

                if (settingsForConfirmPayment != null)
                {
                    string challengeType = $"{pi.PaymentMethod.PaymentMethodFamily}.{pi.PaymentMethod.PaymentMethodType}";
                    string redirectionPattern = TemplateHelper.GetRedirectionPatternFromPartnerSetting(settingsForConfirmPayment, Constants.DescriptionTypes.ChallengeDescription, challengeType);

                    if (string.Equals(redirectionPattern, Constants.RedirectionPatterns.FullPage, StringComparison.OrdinalIgnoreCase))
                    {
                        // TODO [PrachiBansal]: Remove the flight for polling and use PSS template for purchase polling
                        // For that changes need to made in PSS code
                        List<string> flights = this.ExposedFlightFeatures;

                        if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.PXEnablePurchasePollingForUPIConfirmPayment))
                        {
                            flights.Add(Constants.PartnerFlightValues.PXEnablePurchasePollingForUPIConfirmPayment);
                        }

                        // Open new tab
                        var clientAction = new ClientAction(ClientActionType.Pidl)
                        {
                            Context = PIDLResourceFactory.Instance.GetUPIRedirectAndStatusCheckDescriptionForPI(pi, language, partner, sessionId, challengeType, settingsForConfirmPayment, orderId, flights)
                        };

                        retVal.ClientAction = clientAction;
                    }
                    else if (string.Equals(redirectionPattern, Constants.RedirectionPatterns.Inline, StringComparison.OrdinalIgnoreCase))
                    {
                        // Full page redirection
                        RedirectionServiceLink redirectLink = new RedirectionServiceLink()
                        {
                            BaseUrl = string.Format(Constants.UriTemplate.ConfirmPaymentRedirectUrlTemplate, sessionId)
                        };

                        ClientAction clientAction = new ClientAction(ClientActionType.Redirect)
                        {
                            Context = redirectLink
                        };
                        retVal.ClientAction = clientAction;
                    }
                }

                retList.Add(retVal);
                return retList;
            }
            else
            {
                return PIDLResourceFactory.GetChallengePidlsForSession(pi, sessionId, this.ExposedFlightFeatures);
            }
        }

        /// <summary>
        /// Returns challenge PIDLs needed by Wallet for credit card digitization.
        /// ("/GetByAccountIdAndPiid" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>ChallengeDescriptions</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/ChallengeDescriptions/GetByAccountIdAndPiid</url>
        /// <param name="accountId">User's account id</param>
        /// <param name="piid">Id of payment instrument that requires action</param>
        /// <param name="language">Code specifying the language for PIDL localization</param>
        /// <param name="revertChallengeOption">Bool value for revertChallengeOption</param>
        /// <param name="partner">The name of the partner</param>
        /// <throws>HttpResponseException if the payment instrument cannot be found</throws>
        /// <response code="200">List&lt;PIDLResource&gt;</response>
        /// <returns>Returns a challenge PIDL</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Needs to be an instance method for Route action selection")]
        [HttpGet]
        public async Task<ActionResult<List<PIDLResource>>> GetByAccountIdAndPiid(
            string accountId,
            string piid,
            string language = null,
            bool revertChallengeOption = false,
            string partner = Constants.ServiceDefaults.DefaultPartnerName)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddTracingProperties(accountId, piid, null, null, null);
            this.Request.AddPartnerProperty(partner?.ToLower());

            accountId = accountId + string.Empty;
            PaymentInstrument digitizedCard = await this.Settings.PIMSAccessor.GetPaymentInstrument(accountId, piid, traceActivityId);
            this.Request.AddTracingProperties(null, null, digitizedCard.PaymentMethod.PaymentMethodFamily, digitizedCard.PaymentMethod.PaymentMethodType);

            if (digitizedCard == null)
            {
                return Unauthorized(); // in methods returning IActionResult or ActionResult<T>
            }

            ChallengePidlArgs challengeArgs = new ChallengePidlArgs()
            {
                AccountId = accountId,
                PaymentInstrument = digitizedCard,
                RevertChallengeOption = revertChallengeOption,
                Language = language,
                EventTraceActivity = traceActivityId,
                PartnerName = partner,
                PifdBaseUrl = this.PidlBaseUrl
            };

            return PidlDigitizationResourceFactory.GetChallengePidl(challengeArgs);
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

        private async Task<ClientAction> GetLegacyBillDeskRedirectAndStatusCheckClientAction(string rdsUrl, PaymentSession paymentSession, EventTraceActivity traceActivityId, BrowserFlowContext result, PaymentExperienceSetting setting = null)
        {
            ClientAction clientAction = new ClientAction(ClientActionType.Redirect);
            bool isTemplatePartner = PIDLResourceFactory.IsTemplateInList(paymentSession.Partner, setting, Constants.DescriptionTypes.StaticDescription, Constants.StaticDescriptionTypes.LegacyBillDesk3DSRedirectAndStatusCheckPidl);

            if (!string.IsNullOrEmpty(rdsUrl))
            {
                string rdsSessionId = rdsUrl.TrimEnd('/', ' ').Split('/').LastOrDefault();
                var paymentSessionsHandler = await this.GetVersionBasedPaymentSessionsHandler(traceActivityId, paymentSession.Id);
                await paymentSessionsHandler.LinkSession(paymentSessionId: paymentSession.Id, linkSessionId: result.TransactionSessionId ?? rdsSessionId ?? paymentSession.Id, traceActivityId: traceActivityId);
                if (isTemplatePartner && setting != null && setting.RedirectionPattern != null)
                {
                    PIDLGeneratorContext context = new PIDLGeneratorContext(
                        paymentSession?.Country,
                        paymentSession.Partner,
                        TemplateHelper.GetSettingTemplate(paymentSession.Partner, setting, V7.Constants.DescriptionTypes.StaticDescription, Constants.StaticDescriptionTypes.LegacyBillDesk3DSRedirectAndStatusCheckPidl),
                        paymentSession?.Language,
                        Constants.Component.HandlePaymentChallenge,
                        Constants.DescriptionTypes.StaticDescription,
                        PidlFactory.GlobalConstants.PaymentMethodFamilyTypeIds.EwalletLegacyBilldeskPayment,
                        sessionId: paymentSession.Id,
                        rdsSessionId: rdsSessionId,
                        redirectUrl: rdsUrl,
                        Constants.StaticDescriptionTypes.LegacyBillDesk3DSRedirectAndStatusCheckPidl,
                        true,
                        setting);

                    clientAction = PIDLGenerator.Generate<ClientAction>(PIDLResourceFactory.ClientActionGenerationFactory, context);
                }
                else if (PartnerHelper.IsAzurePartner(paymentSession.Partner))
                {
                    clientAction = new ClientAction(ClientActionType.Pidl);

                    List<PIDLResource> redirectPidls = PIDLResourceFactory.Instance.Get3DSRedirectAndStatusCheckDescriptionForPaymentAuth(
                        rdsUrl,
                        rdsSessionId,
                        paymentSession.Id,
                        paymentSession.Partner,
                        paymentSession.Language,
                        paymentSession.Country,
                        Constants.StaticDescriptionTypes.LegacyBillDesk3DSRedirectAndStatusCheckPidl,
                        null,
                        PidlFactory.GlobalConstants.PaymentMethodFamilyTypeIds.EwalletLegacyBilldeskPayment);
                    clientAction.Context = redirectPidls;
                }
                else
                {
                    RedirectionServiceLink redirectLink = new RedirectionServiceLink { BaseUrl = rdsUrl };
                    redirectLink.RuParameters.Add("sessionId", paymentSession.Id);
                    redirectLink.RxParameters.Add("sessionId", paymentSession.Id);

                    clientAction = new ClientAction(ClientActionType.Redirect);
                    clientAction.Context = redirectLink;
                }
            }
            else
            {
                clientAction = CreateFailureClientAction(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError.ToString(), "Invalid IndiaThreeDS URL");
            }

            return clientAction;
        }

        private async Task<ActionResult<List<PIDLResource>>> GetChallengeDescriptionsByTypePiidAndSessionId(
             string accountId, string piid, string type, string sessionId,
             string language, string partner, string scenario, PaymentExperienceSetting setting = null)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddTracingProperties(accountId, piid, null, null);

            accountId = accountId + string.Empty;

            PaymentInstrument pi = await this.Settings.PIMSAccessor.GetPaymentInstrument(accountId, piid, traceActivityId);

            if (pi == null)
            {
                return NotFound();
            }

            string emailAddress = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.EmailAddress);
            if (string.IsNullOrEmpty(emailAddress))
            {
                emailAddress = LocalizationRepository.Instance.GetLocalizedString("Email address", language);
            }

            this.Request.AddTracingProperties(null, null, pi.PaymentMethod.PaymentMethodFamily, pi.PaymentMethod.PaymentMethodType);

            // When the inline feature `showCardInformationInChallenge` is enabled, it will set the scenario to `india3ds` for the template partner, which will retrieve the PIDL for the `authorizeCvvPage`.
            scenario = PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.ShowCardInformationInChallenge, null, setting) ? Constants.ScenarioNames.IndiaThreeDS : scenario;

            return PIDLResourceFactory.Instance.GetChallengeDescriptionsForPi(
                pi,
                type,
                language,
                partner,
                sessionId,
                scenario,
                emailAddress: emailAddress,
                exposedFlightFeatures: this.ExposedFlightFeatures,
                setting: setting);
        }

        private async Task<PaymentSession> UpdatePaymentSessionWithStoredSession(PaymentSession paymentSession, EventTraceActivity traceActivityId)
        {
            try
            {
                string sessionId = paymentSession.Id;
                var paymentSessionsHandler = await this.GetVersionBasedPaymentSessionsHandler(traceActivityId, sessionId);
                Model.PXInternal.PaymentSession storedSession = await paymentSessionsHandler.GetStoredSession(sessionId, traceActivityId);
                if (!string.IsNullOrEmpty(storedSession.ChallengeType))
                {
                    paymentSession.IsChallengeRequired = storedSession.PiRequiresAuthentication;
                    paymentSession.ChallengeType = storedSession.ChallengeType;
                }
            }
            catch (Exception e)
            {
                SllWebLogger.TracePXServiceException("Error retrieving payment session. " + e.Message, traceActivityId);
            }

            return paymentSession;
        }
    }
}
