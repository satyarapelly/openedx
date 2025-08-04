// <copyright file="PaymentInstrumentsExController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Tracing;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Transaction;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.Pidl.Localization;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Model.D365Service;
    using Microsoft.Commerce.Payments.PXService.Model.FraudDetectionService;
    using Microsoft.Commerce.Payments.PXService.Model.HIPService;
    using Microsoft.Commerce.Payments.PXService.Model.IssuerService;
    using Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService;
    using Microsoft.Commerce.Payments.PXService.Model.PXInternal;
    using Microsoft.Commerce.Payments.PXService.Model.ShortURLService;
    using Microsoft.Commerce.Payments.PXService.V7.Contexts;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentClient;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using PXService.Model.AccountService.AddressValidation;
    using AddressEnrichmentService = Microsoft.Commerce.Payments.PXService.Accessors.AddressEnrichmentService.DataModel;
    using AddressInfo = PimsModel.V4.AddressInfo;
    using ClientActionType = PXCommon.ClientActionType;
    using NetworkToken = Model.NetworkTokenizationService.NetworkToken;
    using PaymentCommonInstruments = Microsoft.Commerce.Payments.Common.Instruments;
    using PaymentInstrument = PimsModel.V4.PaymentInstrument;
    using PIMSModel = Microsoft.Commerce.Payments.PimsModel.V4;
    using Purchase = PXService.Model.PurchaseService;

    public class PaymentInstrumentsExController : ProxyController
    {
        private const string PostMessageHtmlTemplate = "<html><script>window.parent.postMessage(\"{0}\", \"*\");</script><body/></html>";
        private static string[] defaultStatusToQuery = new string[] { "active" };

        /// <summary>
        /// Get Html post message for anonymous resume pending operation
        /// </summary>
        /// <group>PaymentInstrumentsEx</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/paymentInstrumentsEx?piid={piid}&amp;isSuccessful={isSuccessful}&amp;language={language}&amp;partner={partner}&amp;sessionQueryUrl={sessionQueryUrl}&amp;country={country}</url>
        /// <param name="piid" required="false" cref="string" in="query">Payment instrument id</param>
        /// <param name="isSuccessful" required="true" cref="bool" in="query">A bool value incidicate whether previous operation is successful or not</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <param name="sessionQueryUrl" required="false" cref="string" in="query">A url used for querying session status</param>
        /// <param name="country" required="false" cref="string" in="query">Two letter country code</param>
        /// <response code="200">Html post message for anonymous resume pending operation</response>
        /// <returns>Html post message</returns>
        [HttpGet]
        public HttpResponseMessage AnonymousResumePendingOperation(
            string piid,
            bool isSuccessful = false,
            string language = "en",
            string partner = Constants.ServiceDefaults.DefaultPartnerName,
            string sessionQueryUrl = null,
            string country = null)
        {
            this.Request.AddPartnerProperty(partner?.ToLower());
            ClientAction nextAction = null;
            if (isSuccessful)
            {
                List<PIDLResource> retVal = PIDLResourceFactory.GetCc3DSIframeStatusCheckDescriptionForPI(piid, language, partner, null, null, false, country, sessionQueryUrl);
                nextAction = new ClientAction(ClientActionType.Pidl, retVal);
            }
            else
            {
                nextAction = CreateFailureClientAction(HttpStatusCode.BadRequest, V7.Constants.ThreeDSErrorCodes.ThreeDSOneResumeAddPiFailed, "Resume add PI failed");
            }

            string rdsSessionId;
            try
            {
                rdsSessionId = sessionQueryUrl.Split('/')[1];
            }
            catch
            {
                rdsSessionId = string.Empty;
            }

            if (nextAction != null)
            {
                nextAction.ActionId = rdsSessionId;
            }

            return ComposeHtmlPostMessageResponse(nextAction);
        }

        /// <summary>
        /// Anonymous Post Modern PI ("/PostModernPI" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>PaymentInstrumentsEx</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/paymentInstrumentsEx/create?country={country}&amp;language={language}&amp;partner={partner}&amp;sessionId={sessionId}</url>
        /// <param name="pi" required="true" cref="object" in="body">Payment instrument</param>
        /// <param name="sessionId" required="true" cref="string" in="query">user session id</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <param name="classicProduct" required="false" cref="string" in="query">Classic product name</param>
        /// <param name="billableAccountId" required="false" cref="string" in="query">Billable account id</param>
        /// <param name="completePrerequisites" required="false" cref="bool" in="query">Bool value to indicate whether to complete prerequisites</param>
        /// <param name="country" required="false" cref="string" in="query">Two letter country code</param>
        /// <param name="scenario" required="false" cref="string" in="query">Scenario name</param>
        /// <param name="orderId" required="false" cref="string" in="query">Order id</param>
        /// <response code="200">A payment instrument object</response>
        /// <returns>A payment instrument object</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> CreateModernPI(
            [FromBody] PIDLData pi,
            string sessionId = null,
            string language = "en-us",
            string partner = Constants.ServiceDefaults.DefaultPartnerName,
            string classicProduct = null,
            string billableAccountId = null,
            bool completePrerequisites = false,
            string country = null,
            string scenario = null,
            string orderId = null)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            var requestContext = this.GetRequestContext(traceActivityId);
            if (requestContext != null && !string.IsNullOrWhiteSpace(requestContext.RequestId))
            {
                return await this.AddPaymentInstrumentToPaymentAccount(
                                    traceActivityId: traceActivityId,
                                    requestContext: requestContext,
                                    pi: pi,
                                    language: language,
                                    partner: partner,
                                    country: country);
            }

            this.Request.AddPartnerProperty(partner?.ToLower());

            // Anonymous Add CC QR Code Flow
            if (IsAnonymousSecondScreen(scenario))
            {
                return await this.HandleAnonymousAdd(pi, sessionId, language, partner, traceActivityId, country, scenario, orderId, completePrerequisites, billableAccountId);
            }

            ErrorResponse content = new ErrorResponse(
                Constants.CreditCardErrorCodes.InvalidRequestData,
                string.Format("Invalid anonymous post call"));

            return Request.CreateResponse(HttpStatusCode.BadRequest, content);
        }

        /// <summary>
        /// List Modern PIs ("/ListModernPIs" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>PaymentInstrumentsEx</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/paymentInstrumentsEx/ListModernPIs?status={status}&amp;deviceId={deviceId}&amp;language={language}&amp;partner={partner}&amp;country={country}&amp;includeDuplicates={includeDuplicates}</url>
        /// <param name="accountId" required="true" cref="string" in="path">account id</param>
        /// <param name="status" required="false" cref="Array" in="query">pi status</param>
        /// <param name="deviceId" required="false" cref="string" in="query">device id</param>
        /// <param name="includePidl" required="false" cref="bool" in="query">Bool value to indicate whether to return new combined pidlPayload object, false by default.</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner coe</param>
        /// <param name="country" required="false" cref="string" in="query">Two letter country code</param>
        /// <param name="scenario" required="false" cref="string" in="query">Scenario name</param>
        /// <param name="includeDuplicates" required="false" cref="bool" in="query">a bool value to indicate whther to include duplicates</param>
        /// <param name="operation" required="false" cref="string" in="query">Operation name</param>
        /// <response code="200">A list of payment instrument</response>
        /// <returns>A list of payment instrument or IPidlPayload object</returns>
        [HttpGet]
        public async Task<HttpResponseMessage> ListModernPIs(
            string accountId,
            string[] status = null,
            ulong deviceId = 0,
            bool includePidl = false,
            string language = "en",
            string partner = Constants.ServiceDefaults.DefaultPartnerName,
            string country = null,
            string scenario = null,
            bool includeDuplicates = false,
            string operation = null)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddTracingProperties(accountId, null, null, null);
            this.Request.AddPartnerProperty(partner?.ToLower());

            // Enable flighting based on the setting partner
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(operation);
            this.EnableFlightingsInPartnerSetting(setting, country);

            // Remove references to Venmo flights once it's been bumped to 100%
            if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.PxEnableVenmo))
            {
                this.ExposedFlightFeatures.Add(Constants.PartnerFlightValues.PxEnableVenmo);
            }

            if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.PxEnableSelectPMAddPIVenmo))
            {
                this.ExposedFlightFeatures.Add(Constants.PartnerFlightValues.PxEnableSelectPMAddPIVenmo);
            }

            if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.ListModernPIsWithCardArt))
            {
                this.ExposedFlightFeatures.Add(Constants.PartnerFlightValues.ListModernPIsWithCardArt);
            }

            status = (status == null || status.Length == 0) ? defaultStatusToQuery : status;
            string listUrl = string.Format(Constants.UriTemplate.ListPI, accountId, deviceId);
            List<KeyValuePair<string, string>> queryParams = new List<KeyValuePair<string, string>>();
            if (!string.IsNullOrWhiteSpace(country))
            {
                queryParams.Add(new KeyValuePair<string, string>(Constants.QueryParameterName.Country, country));
            }

            if (includeDuplicates)
            {
                queryParams.Add(new KeyValuePair<string, string>(Constants.QueryParameterName.IncludeDuplicates, includeDuplicates.ToString()));
            }

            PaymentInstrument[] pimsPaymentInstruments = await this.Settings.PIMSAccessor.ListPaymentInstrument(accountId, deviceId, status, traceActivityId, queryParams, partner, country, language, this.ExposedFlightFeatures, setting: setting);

            var tokenIndex = new Dictionary<string, NetworkToken>();
            if (ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.ListModernPIsWithCardArt))
            {
                try
                {
                    string puid = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.Puid);

                    NetworkTokenizationServiceResponse networkTokens = await this.Settings.NetworkTokenizationServiceAccessor.GetNetworkTokens(puid, deviceId.ToString(), traceActivityId, ExposedFlightFeatures);

                    if (networkTokens != null && networkTokens.Tokens != null)
                    {
                        foreach (var token in networkTokens.Tokens)
                        {
                            tokenIndex.Add(token.NetworkTokenId, token);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Getting the network tokens for card art is a best effort and the ListModernPIs API should not fail because of it, but do log the failure as a warning.
                    SllWebLogger.TraceServerMessage($"NetworkTokenizationServiceAccessor.GetNetworkTokens:" + ex.ToString(), traceActivityId.ToString(), null, "Failed to get the network tokens.", EventLevel.Warning);
                }
            }

            foreach (var paymentInstrument in pimsPaymentInstruments)
            {
                ClientActionFactory.AddClientActionToPaymentInstrument(paymentInstrument, accountId, language, partner, null, null, traceActivityId, this.PidlBaseUrl, Constants.RequestType.GetPI, setting: setting);
                if (ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.ListModernPIsWithCardArt))
                {
                    AddCardArt(paymentInstrument, tokenIndex);
                }
            }

            this.OverrideExpiration(pimsPaymentInstruments);

            if (includePidl && this.ExposedFlightFeatures != null && this.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.IncludePIDLWithPaymentInstrumentList, StringComparer.OrdinalIgnoreCase))
            {
                List<PaymentInstrument> paymentInstrumentList = new List<PaymentInstrument>(pimsPaymentInstruments);

                // Get the selectInstance pidl
                List<PIDLResource> selectInstancePidl = this.GetSelectPaymentResourcePidl(country, language, partner, operation, scenario, null, null, paymentInstrumentList);
                Dictionary<string, List<PIDLResource>> pidls = new Dictionary<string, List<PIDLResource>>()
                {
                    { Constants.Operations.SelectInstance, selectInstancePidl }
                };

                // Make the identity
                Dictionary<string, string> identity = new Dictionary<string, string>()
                {
                    { Constants.QueryParameterName.Partner, partner },
                    { Constants.QueryParameterName.Language, language },
                    { Constants.QueryParameterName.Country, country }
                };

                // Make the new response object with PI list and PIDL info
                PidlInfo pidlInfo = new PidlInfo(pidls, identity);
                PidlPayload payload = new PidlPayload(paymentInstrumentList, pidlInfo);
                return this.Request.CreateResponse(payload);
            }

            // And then in the original method:
            if (PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.AddNewPaymentMethodOption, country, setting))
            {
                var addNewPM = CreateNewPaymentMethodOption(accountId);
                pimsPaymentInstruments = pimsPaymentInstruments.Concat(new PaymentInstrument[] { addNewPM }).ToArray();
            }

            return this.Request.CreateResponse(pimsPaymentInstruments);
        }

        /// <summary>
        /// Get Payment Instrument ("/GetModernPI" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>PaymentInstrumentsEx</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/paymentInstrumentsEx/GetModernPI?completePrerequisites={completePrerequisites}&amp;ignoreMissingTaxId={ignoreMissingTaxId}&amp;family={family}&amp;type={type}&amp;country={country}&amp;language={language}&amp;partner={partner}&amp;operation={operation}&amp;scenario={scenario}&amp;classicProduct={classicProduct}&amp;billableAccountId={billableAccountId}</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="piid" required="false" cref="string" in="query">Payment instrument id</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <param name="completePrerequisites" required="true" cref="bool" in="query">Bool value to indicate whether to complete prerequisites</param>
        /// <param name="country" required="false" cref="string" in="query">Two letter country code</param>
        /// <param name="scenario" required="false" cref="string" in="query">Scenario name</param>
        /// <param name="sessionQueryUrl" required="false" cref="string" in="query">A url used for querying session status</param>
        /// <param name="classicProduct" required="false" cref="string" in="query">Classic product name</param>
        /// <param name="sessionId" required="false" cref="string" in="query">Used to get PI from corresponding RDS session</param>
        /// <response code="200">A payment instrument object</response>
        /// <returns>A payment instrument object</returns>
        [HttpGet]
        public async Task<PaymentInstrument> GetModernPI(
            string accountId,
            string piid,
            string language = "en",
            string partner = Constants.ServiceDefaults.DefaultPartnerName,
            bool completePrerequisites = false,
            string country = null,
            string scenario = null,
            string sessionQueryUrl = null,
            string classicProduct = null,
            string sessionId = null)
        {
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(Constants.Operations.Add);
            if (PXCommon.Constants.PartnerGroups.IsVenmoEnabledPartner(partner) || TemplateHelper.IsTemplateBasedPIDLIncludingDefaultTemplate(TemplateHelper.GetSettingTemplate(partner, setting, Constants.DescriptionTypes.PaymentMethodDescription, $"{Constants.PaymentMethodFamily.ewallet}.{Constants.PaymentMethodType.Venmo}")))
            {
                if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.PxEnableVenmo))
                {
                    this.ExposedFlightFeatures.Add(Constants.PartnerFlightValues.PxEnableVenmo);
                }

                if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.PxEnableSelectPMAddPIVenmo))
                {
                    this.ExposedFlightFeatures.Add(Constants.PartnerFlightValues.PxEnableSelectPMAddPIVenmo);
                }
            }

            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddTracingProperties(accountId, piid, null, null);
            this.Request.AddPartnerProperty(partner?.ToLower());

            // Enable flighting based on setting partner
            this.EnableFlightingsInPartnerSetting(setting, country);

            // Bug 1686756:Consider remove ValidateQueryParametersForCompletePrerequisites from GET PI POST PI and Resume PI once pidl sdk complete prerequisite integration is completed
            this.ValidateQueryParametersForCompletePrerequisites(completePrerequisites, traceActivityId);

            if (sessionId != null && PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner) && string.Equals(scenario, Constants.ScenarioNames.XboxCoBrandedCard, StringComparison.OrdinalIgnoreCase))
            {
                string puid = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.Puid);
                if (string.IsNullOrWhiteSpace(puid))
                {
                    throw new ValidationException(ErrorCode.PuidNotFound, "PUID required for ApplicationDetails");
                }

                List<Model.IssuerService.Application> response = await this.Settings.IssuerServiceAccessor.ApplicationDetails(puid, Constants.PaymentMethodCardProductTypes.XboxCreditCard, sessionId);

                return await this.GetXboxCoBrandedCardPidlResponse(accountId, language, traceActivityId, response, partner, country, setting);
            }

            string getUrl = string.Format(Constants.UriTemplate.GetPI, accountId, piid);

            // Alipay requires user emailAddress on alipayQrCodeChallengePage page, pass it to AddClientActionToPaymentInstrument as a nullable parameter.
            string emailAddress = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.EmailAddress);
            if (string.IsNullOrEmpty(emailAddress))
            {
                emailAddress = LocalizationRepository.Instance.GetLocalizedString("Email address", language);
            }

            PimsSessionDetailsResource pimsSessionDetails = null;
            if (!string.IsNullOrEmpty(sessionQueryUrl))
            {
                if ((PartnerHelper.IsPaypayQrCodeBasedAddPIPartner(partner) && string.Equals(scenario, Constants.ScenarioNames.PaypalQrCode, StringComparison.InvariantCultureIgnoreCase))
                    || string.Equals(scenario, Constants.ScenarioNames.ThreeDSOnePolling, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(scenario, Constants.ScenarioNames.VenmoQRCode, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(scenario, Constants.ScenarioNames.VenmoWebPolling, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(setting?.RedirectionPattern, Constants.RedirectionPatterns.QRCode, StringComparison.OrdinalIgnoreCase))
                {
                    pimsSessionDetails = await this.Settings.PIMSAccessor.GetSessionDetails(accountId, sessionQueryUrl, traceActivityId);
                    PaymentInstrument paymentInstrumentPayPalQrCodePolling = new PaymentInstrument();

                    if (pimsSessionDetails.Status == PimsSessionStatus.InProgress || pimsSessionDetails.Status == PimsSessionStatus.Created)
                    {
                        paymentInstrumentPayPalQrCodePolling.Status = PaymentInstrumentStatus.Pending;
                        return paymentInstrumentPayPalQrCodePolling;
                    }
                    else if (pimsSessionDetails.Status == PimsSessionStatus.Failed || pimsSessionDetails.Status == PimsSessionStatus.Expired)
                    {
                        paymentInstrumentPayPalQrCodePolling.Status = PaymentInstrumentStatus.Declined;
                        return paymentInstrumentPayPalQrCodePolling;
                    }
                    else if (pimsSessionDetails.Status == PimsSessionStatus.Success && string.Equals(scenario, Constants.ScenarioNames.ThreeDSOnePolling, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (pimsSessionDetails?.Details?.PaymentInstrumentId == null)
                        {
                            throw TraceCore.TraceException(
                                traceActivityId,
                                new IntegrationException(
                                    PXCommon.Constants.ServiceNames.InstrumentManagementService,
                                    string.Format("PaymentInstrumentId should be present when session state is Success"),
                                    Constants.PXServiceIntegrationErrorCodes.InvalidSessionInfo));
                        }

                        piid = pimsSessionDetails.Details.PaymentInstrumentId;
                    }
                }
                else
                {
                    pimsSessionDetails = await this.Settings.PIMSAccessor.GetSessionDetails(accountId, sessionQueryUrl, traceActivityId);

                    if (pimsSessionDetails.Status == PimsSessionStatus.Success)
                    {
                        if (pimsSessionDetails?.Details?.PaymentInstrumentId == null)
                        {
                            throw TraceCore.TraceException(
                                traceActivityId,
                                new IntegrationException(
                                    PXCommon.Constants.ServiceNames.InstrumentManagementService,
                                    string.Format("PaymentInstrumentId should be present when session state is Success"),
                                    Constants.PXServiceIntegrationErrorCodes.InvalidSessionInfo));
                        }

                        piid = pimsSessionDetails.Details.PaymentInstrumentId;
                    }
                    else if (pimsSessionDetails.Status == PimsSessionStatus.Failed)
                    {
                        var instrumentManagementError = new ServiceErrorResponse(pimsSessionDetails.Details?.SessionError?.ErrorCode, pimsSessionDetails.Details?.SessionError?.Message);
                        instrumentManagementError.Source = PXCommon.Constants.ServiceNames.InstrumentManagementService;
                        var pxServiceErrorResponse = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, instrumentManagementError)
                        {
                            ErrorCode = instrumentManagementError?.ErrorCode ?? Constants.PXServiceIntegrationErrorCodes.PimsSessionFailed,
                            Message = string.Format("PimsSession is in a Failed status."),
                            Source = PXCommon.Constants.ServiceNames.InstrumentManagementService
                        };

                        var pxServiceResponseException = new PimsSessionException(pxServiceErrorResponse);
                        throw TraceCore.TraceException(traceActivityId, pxServiceResponseException);
                    }
                    else if (pimsSessionDetails.Status == PimsSessionStatus.Expired)
                    {
                        var pxServiceErrorResponse = new ServiceErrorResponse()
                        {
                            CorrelationId = traceActivityId.ActivityId.ToString(),
                            ErrorCode = Constants.PXServiceIntegrationErrorCodes.PimsSessionExpired,
                            Message = string.Format("PimsSession is in an Expired status."),
                            Source = PXCommon.Constants.ServiceNames.InstrumentManagementService,
                        };

                        var pxServiceResponseException = new PimsSessionException(pxServiceErrorResponse);
                        throw TraceCore.TraceException(traceActivityId, pxServiceResponseException);
                    }
                }
            }

            PaymentInstrument paymentInstrument = null;

            try
            {
                paymentInstrument = await this.Settings.PIMSAccessor.GetPaymentInstrument(accountId, piid, traceActivityId, partner, country, language, this.ExposedFlightFeatures, setting: setting);
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

            if (paymentInstrument != null)
            {
                this.Request.AddTracingProperties(null, null, paymentInstrument.PaymentMethod.PaymentMethodFamily, paymentInstrument.PaymentMethod.PaymentMethodType);

                // Since IdealBA Get PI call won't return redirect url from PIMS. To avoid PIMS later fix break PX, for idealBA, we won't add client action.
                if (!IsIdealBillingAgreement(paymentInstrument))
                {
                    string requestType = Constants.RequestType.GetPI;

                    ClientActionFactory.AddClientActionToPaymentInstrument(paymentInstrument, accountId, language, partner, classicProduct, null, traceActivityId, this.PidlBaseUrl, requestType, completePrerequisites, country, emailAddress, scenario, sessionQueryUrl, setting: setting, exposedFlightFeatures: this.ExposedFlightFeatures);
                }

                await this.RaisePIAttachOnOfferEvent(
                    traceActivityId: traceActivityId,
                    newPI: paymentInstrument,
                    partner: partner,
                    country: country,
                    offerId: this.OfferId,
                    exposedFlightFeatures: this.ExposedFlightFeatures);

                await this.HandleProfileAddress(accountId, paymentInstrument, completePrerequisites, language, partner, country, HttpMethod.Get, traceActivityId, this.ExposedFlightFeatures, setting: setting);
            }

            this.OverrideExpiration(paymentInstrument);

            return paymentInstrument;
        }

        /// <summary>
        /// Post Modern PI ("/PostModernPI" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>PaymentInstrumentsEx</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/paymentInstrumentsEx/PostModernPI</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="pi" required="true" cref="object" in="body">Payment instrument</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <param name="classicProduct" required="false" cref="string" in="query">Classic product name</param>
        /// <param name="billableAccountId" required="false" cref="string" in="query">Billable account id</param>
        /// <param name="completePrerequisites" required="false" cref="bool" in="query">Bool value to indicate whether to complete prerequisites</param>
        /// <param name="country" required="false" cref="string" in="query">Two letter country code</param>
        /// <param name="scenario" required="false" cref="string" in="query">Scenario name</param>
        /// <param name="orderId" required="false" cref="string" in="query">Order id</param>
        /// <response code="200">A payment instrument object</response>
        /// <returns>A payment instrument object</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> PostModernPI(
            string accountId,
            [FromBody] PIDLData pi,
            string language = "en",
            string partner = Constants.ServiceDefaults.DefaultPartnerName,
            string classicProduct = null,
            string billableAccountId = null,
            bool completePrerequisites = false,
            string country = null,
            string scenario = null,
            string orderId = null)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddPartnerProperty(partner?.ToLower());

            return await this.AddNewPI(
            traceActivityId: traceActivityId,
            accountId: accountId,
            pi: pi,
            language: language,
            partner: partner,
            classicProduct: classicProduct,
            billableAccountId: billableAccountId,
            completePrerequisites: completePrerequisites,
            country: country,
            scenario: scenario,
            orderId: orderId);
        }

        /// <summary>
        /// Update Modern PI ("/UpdateModernPI" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>PaymentInstrumentsEx</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/paymentInstrumentsEx/UpdateModernPI</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="piid" required="true" cref="string" in="query">Payment instrument id</param>
        /// <param name="pi" required="true" cref="object" in="body">Payment instrument</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <param name="billableAccountId" required="false" cref="string" in="query">Billable account id</param>
        /// <response code="200">A payment instrument object</response>
        /// <returns>A payment instrument object</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> UpdateModernPI(
            string accountId,
            string piid,
            [FromBody] PIDLData pi,
            string language = "en",
            string partner = Constants.ServiceDefaults.DefaultPartnerName,
            string billableAccountId = null)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddPartnerProperty(partner?.ToLower());

            string paymentMethodFamily = pi["paymentMethodFamily"].ToString();
            string paymentMethodType = pi["paymentMethodType"].ToString();
            this.Request.AddTracingProperties(accountId, piid, paymentMethodFamily, paymentMethodType);

            // Use Partner Settings if enabled for the partner
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(Constants.Operations.Update);

            // Enable flighting based on setting partner
            this.EnableFlightingsInPartnerSetting(setting, string.Empty);

            // Add ip address to pi payload and pass it to PIMS
            if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXPassIpAddressToPIMSForAddUpdatePI, StringComparer.OrdinalIgnoreCase))
            {
                await this.AddParameterToPIRiskData(pi, Constants.DeviceInfoProperty.IPAddress, GlobalConstants.ClientContextKeys.DeviceInfo.IPAddress);
            }

            // Add user agent to pi payload and pass it to PIMS
            if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXPassUserAgentToPIMSForAddUpdatePI, StringComparer.OrdinalIgnoreCase))
            {
                await this.AddParameterToPIRiskData(pi, Constants.DeviceInfoProperty.UserAgent, GlobalConstants.ClientContextKeys.DeviceInfo.UserAgent);
            }

            // Compliance - CELA - Use AVS Suggested address with 9-digit
            if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXAddressZipCodeUpdateTo9Digit, StringComparer.OrdinalIgnoreCase))
            {
                await this.TryUpdateAddressWith9digitZipCode(pi);
            }

            // For Azure Update PI scenario, if legacy billable account does not have an address within account, use PI's address to update account address
            if ((PartnerHelper.IsAzurePartner(partner) || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UpdatePIaddressToAccount, string.Empty, setting))
                && (string.Equals(paymentMethodFamily, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase)
                    || PIHelper.IsVirtualLegacyInvoice(paymentMethodFamily, paymentMethodType)))
            {
                try
                {
                    billableAccountId = billableAccountId ?? LegacyAccountHelper.GetBillableAccountId(piid, traceActivityId);

                    if (!string.IsNullOrWhiteSpace(billableAccountId))
                    {
                        string altSecId = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.AadInfo.AltSecId);
                        string orgPuid = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.AadInfo.OrgPuid);
                        string ipAddress = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.DeviceInfo.IPAddress);
                        LegacyAccountHelper.UpdateLegacyBillableAccountAddress(this.Settings, billableAccountId, pi, traceActivityId, altSecId, orgPuid, ipAddress, language);
                    }
                }
                catch (ServiceErrorResponseException legacyAccountException)
                {
                    return this.Request.CreateResponse(legacyAccountException.Error.HttpStatusCode, legacyAccountException.Error, "application/json");
                }
            }

            PaymentInstrument updatedPI = null;
            ServiceErrorResponseException ex = null;
            try
            {
                updatedPI = await this.Settings.PIMSAccessor.UpdatePaymentInstrument(accountId, piid, pi, traceActivityId, partner, this.ExposedFlightFeatures);
            }
            catch (ServiceErrorResponseException exception)
            {
                ex = exception;
            }

            if (ex != null)
            {
                // TODO Task 1614907:[PX CAD] Refactor ClientActionFactory into its own assembly
                if (IsChinaUnionPay(paymentMethodFamily, paymentMethodType))
                {
                    // TODO Once deployment of the new error mapping (where ValidationFailed means terminating) is complete in legacy, ValidationFailed
                    // clause should be removed from the condition below.
                    if (string.Equals(ex.Error.ErrorCode, Constants.CupErrorCodes.ValidationFailed, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(ex.Error.ErrorCode, Constants.CupErrorCodes.InvalidPhoneValue, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(ex.Error.ErrorCode, Constants.CupErrorCodes.InvalidCvv, StringComparison.OrdinalIgnoreCase))
                    {
                        ex.Error.Message = Constants.ClientActionContract.NoMessage;

                        // Error in phone or cvv returns the same ValidationFailed error code
                        // For CUP CC, mark both phone and cvv fields
                        // For CUP DC, mark phone field
                        if (string.Equals(paymentMethodType, Constants.PaymentMethodType.UnionPayCreditCard.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            ex.Error.AddDetail(new ServiceErrorDetail()
                            {
                                ErrorCode = ex.Error.ErrorCode,
                                Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CupErrorMessages.InvalidPhoneOrCvv, language),
                                Target = string.Format("{0},{1}", Constants.CupErrorTargets.Cvv, Constants.CupErrorTargets.PhoneNumber)
                            });
                        }
                        else if (string.Equals(paymentMethodType, Constants.PaymentMethodType.UnionPayDebitCard.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            ex.Error.AddDetail(new ServiceErrorDetail()
                            {
                                ErrorCode = ex.Error.ErrorCode,
                                Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CupErrorMessages.InvalidPhoneNumber, language),
                                Target = Constants.CupErrorTargets.PhoneNumber
                            });
                        }
                        else
                        {
                            // For same purpose, if CUP has third payment type
                            ex.Error.Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CupErrorMessages.Generic, language);
                        }
                    }
                    else if (string.Equals(ex.Error.ErrorCode, Constants.CupErrorCodes.InvalidExpiryDate, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(ex.Error.ErrorCode, Constants.CupErrorCodes.ExpiredCard, StringComparison.OrdinalIgnoreCase))
                    {
                        ex.Error.Message = Constants.ClientActionContract.NoMessage;
                        ex.Error.AddDetail(new ServiceErrorDetail()
                        {
                            ErrorCode = ex.Error.ErrorCode,
                            Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CupErrorMessages.InvalidExpiryDate, language),
                            Target = string.Format("{0},{1}", Constants.CupErrorTargets.ExpiryMonth, Constants.CupErrorTargets.ExpiryYear)
                        });
                    }
                    else if (string.Equals(ex.Error.ErrorCode, Constants.CupErrorCodes.TooManyOperations, StringComparison.OrdinalIgnoreCase))
                    {
                        ex.Error.Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CupErrorMessages.TooManySmsRequests, language);
                    }
                    else
                    {
                        // Catch all for any other error scenario
                        ex.Error.Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CupErrorMessages.Generic, language);
                    }
                }
                else if (IsCreditCard(paymentMethodFamily, paymentMethodType))
                {
                    MapCreditCardCommonError(ref ex, language);
                }
                else if (IsAch(paymentMethodFamily, paymentMethodType) || IsSepa(paymentMethodFamily, paymentMethodType))
                {
                    MapCreditCardCommonError(ref ex, language);
                }

                return this.Request.CreateResponse(ex.Response.StatusCode, ex.Error, "application/json");
            }

            return this.Request.CreateResponse(updatedPI);
        }

        /// <summary>
        /// Replace Modern PI ("/ReplaceModernPI" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>PaymentInstrumentsEx</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/paymentInstrumentsEx/ReplaceModernPI</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="piid" required="true" cref="string" in="query">Payment instrument id</param>
        /// <param name="pi" required="true" cref="object" in="body">Payment instrument</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <param name="classicProduct" required="false" cref="string" in="query">Classic product name</param>
        /// <param name="completePrerequisites" required="false" cref="bool" in="query">Bool value to indicate whether to complete prerequisites</param>
        /// <param name="country" required="false" cref="string" in="query">Two letter country code</param>
        /// <param name="scenario" required="false" cref="string" in="query">Scenario name</param>
        /// <response code="200">A payment instrument object</response>
        /// <returns>A payment instrument object</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> ReplaceModernPI(
            string accountId,
            string piid,
            [FromBody] PIDLData pi,
            string language = "en",
            string partner = Constants.ServiceDefaults.DefaultPartnerName,
            string classicProduct = null,
            bool completePrerequisites = false,
            string country = null,
            string scenario = null)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddPartnerProperty(partner?.ToLower());

            string targetPaymentInstrumentId = null;
            string paymentSessionId = null;

            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            PaymentInstrument newPI = null;

            // Get paymentSessionId if exists
            if (pi.ContainsKey("paymentSessionId"))
            {
                paymentSessionId = pi["paymentSessionId"]?.ToString();
            }

            // Check whether existing PI is provided in the payload
            if (pi.ContainsKey("targetPaymentInstrumentId"))
            {
                targetPaymentInstrumentId = pi["targetPaymentInstrumentId"]?.ToString();
            }
            else
            {
                // Currently we (PIMS) only supports replacePI for credit cards
                if (!pi.ContainsKey("paymentMethodFamily"))
                {
                    throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, string.Format("response status code: {0}, error: {1}", "InvalidPIData", "paymentMethodFamily is missing")));
                }

                string paymentMethodFamily = pi["paymentMethodFamily"].ToString();
                if (!PIHelper.IsCreditCard(paymentMethodFamily))
                {
                    throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, string.Format("response status code: {0}, error: {1}", "InvalidPIData", "Replace operation not supported for the given PI type")));
                }

                // Add new PI
                responseMessage = await this.AddNewPI(
                    traceActivityId: traceActivityId,
                    accountId: accountId,
                    pi: pi,
                    language: language,
                    partner: partner,
                    classicProduct: classicProduct,
                    completePrerequisites: completePrerequisites,
                    country: country,
                    scenario: scenario,
                    orderId: null,
                    isReplacePI: true);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var newPI = await responseMessage.Content.ReadFromJsonAsync<PaymentInstrument>();
                    if (newPI != null && newPI.ClientAction == null)
                    {
                        targetPaymentInstrumentId = newPI.PaymentInstrumentId;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(targetPaymentInstrumentId)
                && !string.Equals(piid, targetPaymentInstrumentId))
            {
                // If PI has subscriptions or preorders, and paymentSessionId doesn't exist, then check whether it needs PSD2 challenge
                if (string.Equals(scenario, "hasSubsOrPreOrders", StringComparison.OrdinalIgnoreCase)
                    && string.IsNullOrWhiteSpace(paymentSessionId))
                {
                    var extendedPI = await this.Settings.PIMSAccessor.GetExtendedPaymentInstrument(
                            piid: targetPaymentInstrumentId,
                            traceActivityId: traceActivityId,
                            partner,
                            exposedFlightFeatures: this.ExposedFlightFeatures);

                    List<string> challengeList = extendedPI?.PaymentInstrumentDetails?.RequiredChallenge;
                    bool challengeRequired = challengeList != null && challengeList.Contains("3ds2");

                    // check the test header
                    TestContext testContext = null;
                    if (!challengeRequired && this.Request != null && this.Request.TryGetTestContext(out testContext))
                    {
                        challengeRequired = HttpRequestHelper.HasAnyPSD2TestScenarios(testContext);
                    }

                    // If challenge is required, return to the client to let client do the challenge flow
                    if (challengeRequired)
                    {
                        return this.Request.CreateResponse(HttpStatusCode.OK, new { piid = targetPaymentInstrumentId, challengeRequired = true, pi = newPI });
                    }
                }

                // Replace old PI with the new one
                try
                {
                    await this.Settings.OrchestrationServiceAccessor.ReplacePaymentInstrument(piid, targetPaymentInstrumentId, paymentSessionId, traceActivityId);
                }
                catch (ServiceErrorResponseException ex)
                {
                    MapCreditCardReplacePIError(ref ex, language);
                    return this.Request.CreateResponse(ex.Response.StatusCode, ex.Error, "application/json");
                }
            }

            return responseMessage;
        }

        /// <summary>
        /// Redeem Modern PI ("/RedeemModernPI" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>PaymentInstrumentsEx</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/paymentInstrumentsEx/RedeemModernPI</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="piid" required="true" cref="string" in="query">Payment instrument id</param>
        /// <param name="pi" required="true" cref="object" in="body">Payment instrument</param>
        /// <param name="country" required="false" cref="string" in="query">Two letter country code</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <response code="200">A payment instrument object</response>
        /// <returns>A PIDLResource object</returns>
        [HttpPost]
        public async Task<PIDLResource> RedeemModernPI(
            string accountId,
            string piid,
            [FromBody] PIDLData pi,
            string country = "us",
            string language = "en",
            string partner = Constants.ServiceDefaults.DefaultPartnerName)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddTracingProperties(accountId, piid, null, null);
            this.Request.AddPartnerProperty(partner?.ToLower());

            // Use Partner Settings if enabled for the partner
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(Constants.Operations.FundStoredValue);

            // Enable flighting based on setting partner
            this.EnableFlightingsInPartnerSetting(setting, country);

            if (string.Equals(PaymentCommonInstruments.GlobalPaymentInstrumentId.BitPay, piid, StringComparison.OrdinalIgnoreCase))
            {
                string amount = pi.TryGetPropertyValue("amount");
                string puid = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.Puid);
                var legacyAccount = await LegacyAccountHelper.GetPersonalLegacyBillableAccountFromMarket(this.Settings, traceActivityId, puid, GlobalConstants.Defaults.Language, country, accountId);
                string greenId = pi.TryGetPropertyValue("riskData.greenId");
                string ipAddress = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.DeviceInfo.IPAddress);
                string userAgent = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.DeviceInfo.UserAgent);
                var fundStoreProcessing = await this.Settings.StoredValueServiceAccessor.FundStoredValue(amount, country, "USD", PaymentCommonInstruments.GlobalPaymentInstrumentId.BitPay, puid, legacyAccount.AccountID, greenId, ipAddress, userAgent, traceActivityId, "PX fund store value");
                var redeemPidl = PIDLResourceFactory.GetPaymentMethodFundStoredValueRedeemDescriptions(amount, "ewallet", "bitcoin", country, "USD", language, partner, PaymentCommonInstruments.GlobalPaymentInstrumentId.BitPay, fundStoreProcessing.RedirectionUrl, fundStoreProcessing.Id, greenId, setting);
                return new PIDLResource()
                {
                    ClientAction = new ClientAction(ClientActionType.Pidl, redeemPidl)
                };
            }

            return null;
        }

        /// <summary>
        /// Redeem Modern PI ("/RedeemModernPI" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>PaymentInstrumentsEx</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/paymentInstrumentsEx/RedeemModernPI</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="piid" required="true" cref="string" in="query">Payment instrument id</param>
        /// <param name="pi" required="true" cref="object" in="body">Payment instrument</param>
        /// <param name="amount" required="true" cref="string" in="query">ammount for redeem</param>
        /// <param name="referenceId" required="true" cref="string" in="query">reference id</param>
        /// <param name="country" required="false" cref="string" in="query">Two letter country code</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="currency" required="false" cref="string" in="query">Currency for redeem</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <param name="greenId" required="false" cref="string" in="query">Green id</param>
        /// <response code="200">A payment instrument object</response>
        /// <returns>A payment instrument object</returns>
        [HttpGet]
        public async Task<object> RedeemModernPI(
            string accountId,
            string piid,
            [FromBody] PIDLData pi,
            string amount,
            string referenceId,
            string country = "us",
            string language = "en",
            string currency = "USD",
            string partner = Constants.ServiceDefaults.DefaultPartnerName,
            string greenId = "")
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddTracingProperties(accountId, piid, null, null);
            this.Request.AddPartnerProperty(partner?.ToLower());

            if (string.Equals(PaymentCommonInstruments.GlobalPaymentInstrumentId.BitPay, piid, StringComparison.OrdinalIgnoreCase))
            {
                string puid = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.Puid);
                var legacyAccount = await LegacyAccountHelper.GetPersonalLegacyBillableAccountFromMarket(this.Settings, traceActivityId, puid, GlobalConstants.Defaults.Language, country, accountId);
                var checkFundStore = await this.Settings.StoredValueServiceAccessor.CheckFundStoredValue(legacyAccount.AccountID, referenceId, traceActivityId);

                if (string.Equals(checkFundStore.Status, "completed", StringComparison.OrdinalIgnoreCase))
                {
                    // PidlFactory.V7.Constants.PollingResponseActionKey.BitcoinFundStoredValueSuccess
                    return new Dictionary<string, string>() { { "status", "completed" } };
                }
                else
                {
                    return new Dictionary<string, string>() { { "status", "poll" } };
                }
            }

            return null;
        }

        /// <summary>
        /// Remove Modern PI ("/RemoveModernPI" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>PaymentInstrumentsEx</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/paymentInstrumentsEx/RemoveModernPI</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="piid" required="true" cref="string" in="query">Payment instrument id</param>
        /// <param name="removeReason" required="true" cref="object" in="body">Remove reason</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <response code="200">A payment instrument object</response>
        /// <returns>A payment instrument object</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> RemoveModernPI(
            string accountId,
            string piid,
            [FromBody] object removeReason,
            string language = "en",
            string partner = Constants.ServiceDefaults.DefaultPartnerName)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddTracingProperties(accountId, piid, null, null);

            try
            {
                await this.Settings.OrchestrationServiceAccessor.RemovePaymentInstrument(piid, traceActivityId);
            }
            catch (ServiceErrorResponseException exception)
            {
                ServiceErrorResponse error = exception.Error;

                if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner))
                {
                    PaymentExperienceSetting setting = null;
                    FeatureContext featureContext = null;
                    bool usePostProcessingFeature = this.ExposedFlightFeatures != null && this.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.PXUsePostProcessingFeatureForRemovePI, StringComparer.OrdinalIgnoreCase);

                    if (usePostProcessingFeature)
                    {
                        setting = this.GetPaymentExperienceSetting(Constants.Operations.Delete);
                        featureContext = new FeatureContext(
                            country: string.Empty,
                            partner,
                            Constants.DescriptionTypes.PaymentInstrumentDescription,
                            Constants.Operations.Delete,
                            null,
                            language,
                            null,
                            exposedFlightFeatures: this.ExposedFlightFeatures,
                            setting?.Features,
                            smdMarkets: null,
                            originalPartner: partner,
                            isGuestAccount: GuestAccountHelper.IsGuestAccount(this.Request));
                    }

                    if (!this.ExposedFlightFeatures.Contains(Flighting.Features.XboxNativeBaseErrorPage, StringComparer.OrdinalIgnoreCase) &&
                    string.Equals(error.ErrorCode, Constants.DeletionErrorCodes.SubscriptionNotCanceled, StringComparison.OrdinalIgnoreCase))
                    {
                        List<PIDLResource> pidl = PIDLResourceFactory.Instance.GetStaticPidlDescriptions(Constants.StaticDescriptionTypes.DeletionSubscriptionErrorPidl, language, PXCommon.Constants.PartnerNames.XboxNative);
                        if (pidl != null && usePostProcessingFeature)
                        {
                            PostProcessor.Process(pidl, PIDLResourceFactory.FeatureFactory, featureContext);
                        }

                        ClientAction clientAction = new ClientAction(ClientActionType.Pidl, pidl);
                        return this.Request.CreateResponse(HttpStatusCode.OK, new PIDLResource { ClientAction = clientAction });
                    }
                    else if (this.ExposedFlightFeatures.Contains(Flighting.Features.XboxNativeBaseErrorPage, StringComparer.OrdinalIgnoreCase))
                    {
                        List<PIDLResource> pidl = PIDLResourceFactory.Instance.GetStaticPidlDescriptions(Constants.StaticDescriptionTypes.XboxNativeBaseErrorPidl, language, PXCommon.Constants.PartnerNames.XboxNative);
                        ContainerDisplayHint xboxNativeBaseErrorTopGroup = pidl[0].GetDisplayHintById(Constants.DisplayHintIds.XboxNativeBaseErrorTopGroup) as ContainerDisplayHint;

                        if (string.Equals(error.ErrorCode, Constants.DeletionErrorCodes.SubscriptionNotCanceled, StringComparison.OrdinalIgnoreCase))
                        {
                            xboxNativeBaseErrorTopGroup.AddDisplayHint(new TextDisplayHint { DisplayContent = PidlModelHelper.GetLocalizedString(Constants.DeletionErrorMessages.SubscriptionNotCanceledMessage, language), StyleHints = new List<string> { "margin-bottom-x-small" } });
                        }
                        else if (string.Equals(error.ErrorCode, Constants.DeletionErrorCodes.OutstandingBalance, StringComparison.OrdinalIgnoreCase))
                        {
                            xboxNativeBaseErrorTopGroup.AddDisplayHint(new TextDisplayHint { DisplayContent = PidlModelHelper.GetLocalizedString(Constants.DeletionErrorMessages.OutstandingBalanceMessage, language), StyleHints = new List<string> { "margin-bottom-x-small" } });
                        }
                        else if (string.Equals(error.ErrorCode, Constants.DeletionErrorCodes.RemovePIAccessDeniedForTheCaller, StringComparison.OrdinalIgnoreCase))
                        {
                            xboxNativeBaseErrorTopGroup.AddDisplayHint(new TextDisplayHint { DisplayContent = PidlModelHelper.GetLocalizedString(Constants.DeletionErrorMessages.RemoveBusinessInstrumentNotSupportedMessage, language), StyleHints = new List<string> { "margin-bottom-x-small" } });
                        }
                        else
                        {
                            // Catching all other errors not included in the if blocks above.
                            MapRemovePIError(ref exception, language);
                            return this.Request.CreateResponse(exception.Response.StatusCode, exception.Error, "application/json");
                        }

                        xboxNativeBaseErrorTopGroup.AddDisplayHint(new TextDisplayHint { DisplayContent = PidlModelHelper.GetLocalizedString(Constants.DeletionErrorMessages.ManageMessage, language) });
                        if (pidl != null && usePostProcessingFeature)
                        {
                            PostProcessor.Process(pidl, PIDLResourceFactory.FeatureFactory, featureContext);
                        }

                        ClientAction clientAction = new ClientAction(ClientActionType.Pidl, pidl);
                        return this.Request.CreateResponse(HttpStatusCode.OK, new PIDLResource { ClientAction = clientAction });
                    }
                }

                MapRemovePIError(ref exception, language);
                return this.Request.CreateResponse(exception.Response.StatusCode, exception.Error, "application/json");
            }

            return this.Request.CreateResponse(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Resume Pending Operation ("/ReplaceModernPI" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>PaymentInstrumentsEx</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/paymentInstrumentsEx/ResumePendingOperation</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="piid" required="true" cref="string" in="query">Payment instrument id</param>
        /// <param name="pendingOpRequestData" required="true" cref="object" in="body">Pending operation request data</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <param name="classicProduct" required="false" cref="string" in="query">Classic product name</param>
        /// <param name="billableAccountId" required="false" cref="string" in="query">Billable Account Id</param>
        /// <param name="completePrerequisites" required="false" cref="bool" in="query">Bool value to indicate whether to complete prerequisites</param>
        /// <param name="country" required="false" cref="string" in="query">Two letter country code</param>
        /// <response code="200">A payment instrument object</response>
        /// <returns>A payment instrument object</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> ResumePendingOperation(
            string accountId,
            string piid,
            [FromBody] PIDLData pendingOpRequestData,
            string language = "en",
            string partner = Constants.ServiceDefaults.DefaultPartnerName,
            string classicProduct = null,
            string billableAccountId = null,
            bool completePrerequisites = false,
            string country = null)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddTracingProperties(accountId, piid, null, null);
            this.Request.AddPartnerProperty(partner?.ToLower());

            // Use Partner Settings if enabled for the partner
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(Constants.Operations.Add);

            // Bug 1686756:Consider remove ValidateQueryParametersForCompletePrerequisites from GET PI POST PI and Resume PI once pidl sdk complete prerequisite integration is completed
            this.ValidateQueryParametersForCompletePrerequisites(completePrerequisites, traceActivityId);

            PaymentInstrument pi = await this.Settings.PIMSAccessor.GetPaymentInstrument(accountId, piid, traceActivityId);
            string family = pi.PaymentMethod.PaymentMethodFamily;
            string type = pi.PaymentMethod.PaymentMethodType;
            this.Request.AddTracingProperties(null, null, family, type);

            // Add ip address to pi payload and pass it to PIMS
            if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXPassIpAddressToPIMSForAddUpdatePI, StringComparer.OrdinalIgnoreCase))
            {
                await this.AddParameterToPIRiskData(pendingOpRequestData, Constants.DeviceInfoProperty.IPAddress, GlobalConstants.ClientContextKeys.DeviceInfo.IPAddress);
            }

            // Add user agent to pi payload and pass it to PIMS
            if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXPassUserAgentToPIMSForAddUpdatePI, StringComparer.OrdinalIgnoreCase))
            {
                await this.AddParameterToPIRiskData(pendingOpRequestData, Constants.DeviceInfoProperty.UserAgent, GlobalConstants.ClientContextKeys.DeviceInfo.UserAgent);
            }

            var queryparams = this.Request.Query.AsEnumerable().Select(q => new KeyValuePair<string, string>(q.Key, q.Value));
            PaymentInstrument updatedPi = null;
            ServiceErrorResponseException ex = null;

            // Pims initialize a new endpoint for resume Sepa/picv challenge
            if (IsSepa(family, type))
            {
                try
                {
                    updatedPi = await this.Settings.PIMSAccessor.ValidatePicv(accountId, piid, pendingOpRequestData, traceActivityId, queryparams);
                    await this.RaisePIAttachOnOfferEvent(
                        traceActivityId: traceActivityId,
                        newPI: updatedPi,
                        partner: partner,
                        country: country,
                        offerId: this.OfferId,
                        exposedFlightFeatures: this.ExposedFlightFeatures);
                }
                catch (ServiceErrorResponseException exception)
                {
                    ex = exception;
                }
            }
            else
            {
                try
                {
                    updatedPi = await this.Settings.PIMSAccessor.ResumePendingOperation(accountId, piid, pendingOpRequestData, traceActivityId, queryparams);
                    await this.RaisePIAttachOnOfferEvent(
                        traceActivityId: traceActivityId,
                        newPI: updatedPi,
                        partner: partner,
                        country: country,
                        offerId: this.OfferId,
                        exposedFlightFeatures: this.ExposedFlightFeatures);
                }
                catch (ServiceErrorResponseException exception)
                {
                    ex = exception;
                }
            }

            if (ex != null)
            {
                // TODO Task 1614907:[PX CAD] Refactor ClientActionFactory into its own assembly
                if (IsChinaUnionPay(family, type))
                {
                    if ((string.Equals(ex.Error.ErrorCode, Constants.CupErrorCodes.ValidationFailed, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(ex.Error.ErrorCode, Constants.CupErrorCodes.InvalidCvv, StringComparison.OrdinalIgnoreCase))
                            && string.Equals(type, Constants.PaymentMethodType.UnionPayCreditCard.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        ex.Error.Message = Constants.ClientActionContract.NoMessage;
                        ex.Error.AddDetail(new ServiceErrorDetail()
                        {
                            ErrorCode = ex.Error.ErrorCode,
                            Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CupErrorMessages.InvalidPhoneOrCvv, language),
                            Target = string.Format("{0},{1}", Constants.CupErrorTargets.Cvv, Constants.CupErrorTargets.PhoneNumber),
                        });

                        // TODO: How do we get the country in which the PI was originally added during the resume PI call?
                        // How do we know if its add or update?
                        AddJumpbackClientActionToError(ex.Error, pi, "cn", family, type, "add", language, partner, classicProduct, billableAccountId, completePrerequisites);
                    }
                    else if ((string.Equals(ex.Error.ErrorCode, Constants.CupErrorCodes.ValidationFailed, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(ex.Error.ErrorCode, Constants.CupErrorCodes.InvalidCvv, StringComparison.OrdinalIgnoreCase))
                            && string.Equals(type, Constants.PaymentMethodType.UnionPayDebitCard.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        // Debit card
                        ex.Error.Message = Constants.ClientActionContract.NoMessage;
                        ex.Error.AddDetail(new ServiceErrorDetail()
                        {
                            ErrorCode = ex.Error.ErrorCode,
                            Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CupErrorMessages.InvalidPhoneNumber, language),
                            Target = Constants.CupErrorTargets.PhoneNumber
                        });

                        AddJumpbackClientActionToError(ex.Error, pi, "cn", family, type, "add", language, partner, classicProduct, billableAccountId, completePrerequisites);
                    }
                    else if (string.Equals(ex.Error.ErrorCode, Constants.CupErrorCodes.InvalidExpiryDate, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(ex.Error.ErrorCode, Constants.CupErrorCodes.ExpiredCard, StringComparison.OrdinalIgnoreCase))
                    {
                        ex.Error.Message = Constants.ClientActionContract.NoMessage;
                        ex.Error.AddDetail(new ServiceErrorDetail()
                        {
                            ErrorCode = ex.Error.ErrorCode,
                            Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CupErrorMessages.InvalidExpiryDate, language),
                            Target = string.Format("{0},{1}", Constants.CupErrorTargets.ExpiryMonth, Constants.CupErrorTargets.ExpiryYear)
                        });

                        AddJumpbackClientActionToError(ex.Error, pi, "cn", family, type, "add", language, partner, classicProduct, billableAccountId, completePrerequisites);
                    }
                    else if (string.Equals(ex.Error.ErrorCode, Constants.CupErrorCodes.InvalidChallengeCode, StringComparison.OrdinalIgnoreCase))
                    {
                        ex.Error.Message = Constants.ClientActionContract.NoMessage;
                        ex.Error.AddDetail(new ServiceErrorDetail()
                        {
                            ErrorCode = Constants.CupErrorCodes.InvalidChallengeCode,
                            Target = Constants.CupErrorTargets.Sms,
                            Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CupErrorMessages.InvalidSmsCode, language)
                        });
                    }
                    else if (string.Equals(ex.Error.ErrorCode, Constants.CupErrorCodes.ChallengeCodeExpired, StringComparison.OrdinalIgnoreCase))
                    {
                        ex.Error.Message = Constants.ClientActionContract.NoMessage;
                        ex.Error.AddDetail(new ServiceErrorDetail()
                        {
                            ErrorCode = Constants.CupErrorCodes.ChallengeCodeExpired,
                            Target = Constants.CupErrorTargets.Sms,
                            Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CupErrorMessages.SmsCodeExpired, language)
                        });
                    }
                    else if (string.Equals(ex.Error.ErrorCode, Constants.CupErrorCodes.TooManyOperations, StringComparison.OrdinalIgnoreCase))
                    {
                        ex.Error.Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CupErrorMessages.TooManySmsRequests, language);
                    }
                    else
                    {
                        // Catch all for any other error scenario
                        ex.Error.Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CupErrorMessages.Generic, language);
                    }
                }
                else if (IsAch(family, type) || IsSepa(family, type))
                {
                    if (string.Equals(ex.Error.ErrorCode, Constants.DirectDebitErrorCodes.InvalidAmount, StringComparison.OrdinalIgnoreCase))
                    {
                        ex.Error.Message = Constants.ClientActionContract.NoMessage;
                        ex.Error.AddDetail(new ServiceErrorDetail()
                        {
                            ErrorCode = ex.Error.ErrorCode,
                            Message = LocalizationRepository.Instance.GetLocalizedString(Constants.DirectDebitErrorMessages.InvalidAmount, language),
                            Target = Constants.DirectDebitErrorTargets.Amount
                        });

                        pi = await this.Settings.PIMSAccessor.GetPaymentInstrument(accountId, piid, traceActivityId);

                        if (IsAch(family, type))
                        {
                            AddDirectDebitClientActionToError(ex.Error, pi, Constants.PidlResourceDescriptionType.AchPicVChallenge, language, partner, classicProduct, billableAccountId, setting: setting);
                        }
                        else if (IsSepa(family, type))
                        {
                            AddDirectDebitClientActionToError(ex.Error, pi, Constants.PidlResourceDescriptionType.SepaPicVChallenge, language, partner, classicProduct, billableAccountId, setting: setting);
                        }
                    }
                    else
                    {
                        // Catch all for any other error scenario
                        ex.Error.Message = LocalizationRepository.Instance.GetLocalizedString(Constants.DirectDebitErrorMessages.Generic, language);
                    }
                }
                else if (IsAliPay(family, type))
                {
                    MapAlipayResumeError(ref ex, language);
                }
                else if (IsNonSim(family))
                {
                    MapNonSimResumeError(ref ex, language);
                }

                return this.Request.CreateResponse(ex.Response.StatusCode, ex.Error);
            }

            ClientActionFactory.AddClientActionToPaymentInstrument(updatedPi, accountId, language, partner, classicProduct, billableAccountId, traceActivityId, this.PidlBaseUrl, IsSepa(family, type) ? Constants.RequestType.AddPI : null, completePrerequisites, country, setting: setting);

            await this.HandleProfileAddress(accountId, updatedPi, completePrerequisites, language, partner, country, HttpMethod.Post, traceActivityId, this.ExposedFlightFeatures, setting: setting);

            if (updatedPi != null && updatedPi.ClientAction != null && updatedPi.ClientAction.Context != null)
            {
                var pidls = updatedPi.ClientAction.Context as List<PIDLResource>;
                if (pidls != null)
                {
                    FeatureContext featureContext = new FeatureContext(
                        country,
                        partner,
                        Constants.DescriptionTypes.PaymentInstrumentDescription,
                        Constants.Operations.Add,
                        null,
                        language,
                        null,
                        exposedFlightFeatures: this.ExposedFlightFeatures,
                        setting?.Features,
                        updatedPi?.PaymentMethod?.PaymentMethodFamily,
                        updatedPi?.PaymentMethod?.PaymentMethodType,
                        smdMarkets: null,
                        originalPartner: partner,
                        isGuestAccount: GuestAccountHelper.IsGuestAccount(this.Request));

                    PostProcessor.Process(pidls, PIDLResourceFactory.FeatureFactory, featureContext);
                }
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, updatedPi);
        }

        /// <summary>
        /// Get Card Profile ("/GetCardProfile" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>PaymentInstrumentsEx</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/paymentInstrumentsEx/GetCardProfile</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="piid" required="true" cref="string" in="query">Payment instrument id</param>
        /// <param name="deviceId" required="true" cref="string" in="query">Device Id</param>
        /// <response code="200">A card profeil object</response>
        /// <returns>A payment instrument object</returns>
        [HttpGet]
        public async Task<object> GetCardProfile(string accountId, string piid, ulong deviceId)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddTracingProperties(accountId, piid, null, null);

            return await this.Settings.PIMSAccessor.GetCardProfile(accountId, piid, deviceId, traceActivityId);
        }

        /// <summary>
        /// Get Se Card Persos ("/GetSeCardPersos" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>PaymentInstrumentsEx</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/paymentInstrumentsEx/GetSeCardPersos</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="piid" required="true" cref="string" in="query">Payment instrument id</param>
        /// <param name="deviceId" required="true" cref="string" in="query">Device Id</param>
        /// <response code="200">A card profeil object</response>
        /// <returns>A payment instrument object</returns>
        [HttpGet]
        public async Task<object> GetSeCardPersos(string accountId, string piid, ulong deviceId)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddTracingProperties(accountId, piid, null, null);

            return await this.Settings.PIMSAccessor.GetSeCardPersos(accountId, piid, deviceId, traceActivityId);
        }

        /// <summary>
        /// Post Replenish Transaction Credentials ("/PostReplenishTransactionCredentials" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>PaymentInstrumentsEx</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/paymentInstrumentsEx/PostReplenishTransactionCredentials</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="piid" required="true" cref="string" in="query">Payment instrument id</param>
        /// <param name="deviceId" required="true" cref="string" in="query">Device Id</param>
        /// <param name="requestData" required="true" cref="object" in="body">Request data</param>
        /// <response code="200">A payment instrument object</response>
        /// <returns>A object</returns>
        [HttpPost]
        public async Task<object> PostReplenishTransactionCredentials(string accountId, string piid, ulong deviceId, [FromBody] object requestData)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddTracingProperties(accountId, piid, null, null);

            return await this.Settings.PIMSAccessor.ReplenishTransactionCredentials(accountId, piid, deviceId, requestData, traceActivityId);
        }

        /// <summary>
        /// Acquire LUKs ("/AcquireLUKs" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>PaymentInstrumentsEx</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/paymentInstrumentsEx/AcquireLUKs</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="piid" required="true" cref="string" in="query">Payment instrument id</param>
        /// <param name="deviceId" required="true" cref="string" in="query">Device Id</param>
        /// <param name="requestData" required="true" cref="object" in="body">Request data</param>
        /// <response code="200">A payment instrument object</response>
        /// <returns>A object</returns>
        [HttpPost]
        public async Task<object> AcquireLUKs(string accountId, string piid, ulong deviceId, [FromBody] object requestData)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddTracingProperties(accountId, piid, null, null);

            return await this.Settings.PIMSAccessor.AcquireLUKs(accountId, piid, deviceId, requestData, traceActivityId);
        }

        /// <summary>
        /// ConfirmL UKs ("/ConfirmLUKs" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>PaymentInstrumentsEx</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/paymentInstrumentsEx/ConfirmLUKs</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="deviceId" required="true" cref="string" in="query">Device Id</param>
        /// <param name="piid" required="true" cref="string" in="query">Payment instrument id</param>
        /// <response code="200">A payment instrument object</response>
        /// <returns>A object</returns>
        [HttpPost]
        public async Task<object> ConfirmLUKs(string accountId, ulong deviceId, string piid)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddTracingProperties(accountId, piid, null, null);

            return await this.Settings.PIMSAccessor.ConfirmLUKs(accountId, piid, deviceId, traceActivityId);
        }

        /// <summary>
        /// Validate Cvv ("/ValidateCvv" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>PaymentInstrumentsEx</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/paymentInstrumentsEx/ValidateCvv</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="piid" required="true" cref="string" in="query">Payment instrument id</param>
        /// <param name="language" required="true" cref="string" in="query">Two digit language Id</param>
        /// <param name="requestData" required="true" cref="object" in="body">Request data</param>
        /// <response code="200">A payment instrument object</response>
        /// <returns>A object</returns>
        [HttpPost]
        public async Task<object> ValidateCvv(string accountId, string piid, string language, [FromBody] object requestData)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddTracingProperties(accountId, piid, null, null);

            object response = null;
            ServiceErrorResponseException ex = null;
            try
            {
                response = await this.Settings.PIMSAccessor.ValidateCvv(accountId, piid, requestData, traceActivityId);
            }
            catch (ServiceErrorResponseException exception)
            {
                ex = exception;
            }

            if (ex != null)
            {
                if (string.Equals(ex.Error.ErrorCode, Constants.ValidateCvvErrorCodes.InvalidCvv, StringComparison.OrdinalIgnoreCase))
                {
                    ex.Error.Message = Constants.ClientActionContract.NoMessage;
                    ex.Error.AddDetail(new ServiceErrorDetail()
                    {
                        ErrorCode = ex.Error.ErrorCode,
                        Message = LocalizationRepository.Instance.GetLocalizedString(Constants.ValidateCvvErrorMessages.InvalidCvv, language),
                        Target = Constants.ValidateCvvErrorTargets.Cvv,
                    });
                }

                return this.Request.CreateResponse(ex.Response.StatusCode, ex.Error);
            }

            if (response == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.NoContent);
            }

            // validateCvv returns 204 on success, if content was received, there was some error
            return this.Request.CreateResponse(
                HttpStatusCode.BadRequest,
                new ErrorMessage()
                {
                    ErrorCode = "ValidateCVVReturnedContent",
                    Message = "Validate CVV returned 200 instead of 204",
                    Retryable = false,
                });
        }

        /// <summary>
        /// Posts data to Issuer Service to get redirect url to Barclays for Xbox creditcard application
        /// </summary>
        /// <param name="partner">partner name</param>
        /// <param name="operation">operation Id</param>
        /// <param name="country">country name</param>
        /// <param name="language">language code</param>
        /// <param name="initializeData">Data needed to create a sessionId if one has not been provided for the apply flow. If sessionId provided, this is ignored</param>
        /// <param name="sessionId">Optional sessionId to be used for the apply flow. If this is not provided, initializeData is used to create a sessionId</param>
        /// <returns>Message from IssuerService API</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> Apply(
            string partner,
            string operation,
            string country,
            string language,
            [FromBody] InitializeRequest initializeData,
            string sessionId = null)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddPartnerProperty(partner?.ToLower());

            if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.PXXboxCardApplicationEnableWebview))
            {
                this.ExposedFlightFeatures.Add(Constants.PartnerFlightValues.PXXboxCardApplicationEnableWebview);
            }

            var applySession = sessionId;

            // Use Partner Settings if enabled for the partner
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(Constants.Operations.Apply);

            // If an applySession is not provided, use the provided initializeData to generate one for the Apply call
            if (string.IsNullOrEmpty(applySession))
            {
                applySession = await this.GetValidatedSessionId(traceActivityId, initializeData);
            }

            // For xbox partners, don't call Apply directly and instead use the created sessionId to return a static PIDL resource
            if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner))
            {
                string ocid = Request?.Query.FirstOrDefault(i => i.Key == Constants.QueryParameterName.OCID).Value.ToString() ?? string.Empty;
                return await this.GetXboxCoBrandedCardNativePidl(language, partner, country, applySession, initializeData.Channel, initializeData.ReferrerId, ocid, setting);
            }
            else
            {
                string customerPuid = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.Puid);
                if (string.IsNullOrEmpty(customerPuid))
                {
                    throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.PuidNotFound, "Customer puid invalid and is required for Apply flow"));
                }

                ApplyResponse response = null;
                ApplyRequest applyData = new ApplyRequest()
                {
                    SessionId = applySession
                };

                try
                {
                    response = await this.Settings.IssuerServiceAccessor.Apply(customerPuid, applyData);
                }
                catch (ServiceErrorResponseException e)
                {
                    // If the session was provided as a query parameter and we received a BadSessionState error code, reattempt with a newly created session
                    // as a fallback before failing
                    if (!string.IsNullOrEmpty(sessionId) &&
                        string.Equals(e?.Error?.ErrorCode, Constants.ApplyErrrorCodes.BadSessionState, StringComparison.OrdinalIgnoreCase))
                    {
                        applyData.SessionId = await this.GetValidatedSessionId(traceActivityId, initializeData);
                        response = await this.Settings.IssuerServiceAccessor.Apply(customerPuid, applyData);
                    }
                    else
                    {
                        throw e;
                    }
                }

                if (string.IsNullOrEmpty(response?.RedirectUrl))
                {
                    throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.ServiceError, "RedirectUrl from IssuerServiceAccessor.Apply was not valid"));
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, PIDLResourceFactory.GetRedirectPidl(response.RedirectUrl));
            }
        }

        /// <summary>
        /// Add various expected challenge contexts for the PSD2 3DS challenge flow
        /// </summary>
        /// <group>PaymentInstrumentsEx</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/paymentInstrumentsEx/{piid}/GetChallengeContext</url>
        /// <param name="accountId" required="true" cref="string" in="path">account id</param>
        /// <param name="piid" required="true" cref="string" in="query">payment instrument id</param>
        /// <response code="200">A list of payment instrument</response>
        /// <returns>Response message with populated ChallengeContext object</returns>
        [HttpGet]
        [ActionName("GetChallengeContext")]
        public async Task<HttpResponseMessage> GetChallengeContext(
            string accountId,
            string piid)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddTracingProperties(accountId, piid);

            Model.ThreeDSExternalService.ChallengeContext challengeContext = new Model.ThreeDSExternalService.ChallengeContext();
            PaymentInstrument paymentInstrument = null;

            try
            {
                paymentInstrument = await this.Settings.PIMSAccessor.GetPaymentInstrument(accountId, piid, traceActivityId, exposedFlightFeatures: this.ExposedFlightFeatures);
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

            challengeContext.PaymentMethodType = paymentInstrument?.PaymentMethod?.PaymentMethodType;
            challengeContext.IpAddress = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.DeviceInfo.IPAddress);

            if (challengeContext.IpAddress == null)
            {
                SllWebLogger.TraceServerMessage(
                    GlobalConstants.APINames.GetChallengeContextEx,
                    traceActivityId.ToString(),
                    null,
                    "Public IP address is null",
                        EventLevel.Warning);
            }

            return this.Request.CreateResponse(challengeContext);
        }

        internal static ServiceErrorResponse GenerateValidationFailedServiceErrorResponse(string language)
        {
            var serviceErrorResponse = new ServiceErrorResponse(
                            errorCode: Constants.CreditCardErrorCodes.ValidationFailed,
                            message: LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.Generic, language),
                            source: GlobalConstants.ServiceName);

            serviceErrorResponse.CorrelationId = Guid.NewGuid().ToString();

            serviceErrorResponse.InnerError = new ServiceErrorResponse(
                errorCode: Constants.CreditCardErrorCodes.ValidationFailed,
                message: Constants.CreditCardErrorMessages.PIMSValidationFailed,
                source: PIMSAccessor.PimsServiceName);

            serviceErrorResponse.Details = new List<ServiceErrorDetail>();

            return serviceErrorResponse;
        }

        internal static bool IsCreditCard(string family, string type, string country = null)
        {
            bool isCardFamily = string.Equals(family, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase);
            bool isCreditAmericanExpressType = string.Equals(type, Constants.PaymentMethodType.CreditCardAmericanExpress.ToString(), StringComparison.OrdinalIgnoreCase);
            bool isCreditDiscoverType = string.Equals(type, Constants.PaymentMethodType.CreditCardDiscover.ToString(), StringComparison.OrdinalIgnoreCase);
            bool isCreditVisaType = string.Equals(type, Constants.PaymentMethodType.CreditCardVisa.ToString(), StringComparison.OrdinalIgnoreCase);
            bool isCreditMasterCardType = string.Equals(type, Constants.PaymentMethodType.CreditCardMasterCard.ToString(), StringComparison.OrdinalIgnoreCase);
            bool isCreditRupayType = string.Equals(type, Constants.PaymentMethodType.CreditCardRupay.ToString(), StringComparison.OrdinalIgnoreCase);
            return isCardFamily && (isCreditAmericanExpressType || isCreditDiscoverType || isCreditVisaType || isCreditMasterCardType || isCreditRupayType || IsAllowedEmptyPaymentTypeDuringPost(family, type, country));
        }

        internal static bool IsStoredValue(string family, string type)
        {
            bool isStoredValueFamily = string.Equals(family, Constants.PaymentMethodFamily.ewallet.ToString(), StringComparison.OrdinalIgnoreCase);
            bool isStoredValueType = string.Equals(type, Constants.PaymentMethodType.StoredValue.ToString(), StringComparison.OrdinalIgnoreCase);
            return isStoredValueFamily && isStoredValueType;
        }

        internal static bool IsAch(string family, string type)
        {
            bool isAchFamily = string.Equals(family, Constants.PaymentMethodFamily.direct_debit.ToString(), StringComparison.OrdinalIgnoreCase);
            bool isAchType = string.Equals(type, Constants.PaymentMethodType.Ach.ToString(), StringComparison.OrdinalIgnoreCase);
            return isAchFamily && isAchType;
        }

        internal static bool IsSepa(string family, string type)
        {
            bool isSepaFamily = string.Equals(family, Constants.PaymentMethodFamily.direct_debit.ToString(), StringComparison.OrdinalIgnoreCase);
            bool isSepaType = string.Equals(type, Constants.PaymentMethodType.Sepa.ToString(), StringComparison.OrdinalIgnoreCase);
            return isSepaFamily && isSepaType;
        }

        private static HttpResponseMessage ComposeHtmlPostMessageResponse(ClientAction clientAction)
        {
            string jsEncodedClientAction = HttpUtility.JavaScriptStringEncode(JsonConvert.SerializeObject(clientAction));
            string responseContent = string.Format(PostMessageHtmlTemplate, jsEncodedClientAction);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(responseContent);
            response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/html; charset=utf-8");
            return response;
        }

        private static void TranslateUnicodes(List<PIDLResource> pidl)
        {
            TextDisplayHint xboxCardApplySuccessText = pidl.First<PIDLResource>()?.GetDisplayHintById("xboxCardApplySuccessText") as TextDisplayHint;
            if (xboxCardApplySuccessText != null)
            {
                xboxCardApplySuccessText.DisplayContent = PXCommon.StringHelper.MapUnicodeChars(xboxCardApplySuccessText.DisplayContent);
            }

            TextDisplayHint xboxCardApplySuccessSubtext = pidl.First<PIDLResource>()?.GetDisplayHintById("xboxCardApplySuccessSubtext") as TextDisplayHint;
            if (xboxCardApplySuccessSubtext != null)
            {
                xboxCardApplySuccessSubtext.DisplayContent = PXCommon.StringHelper.MapUnicodeChars(xboxCardApplySuccessSubtext.DisplayContent);
            }
        }

        private static ClientAction CreateFailureClientAction(HttpStatusCode statusCode, string errorCode, string errorMessage, string userMessage = null)
        {
            // special handling for iframe post message,
            // if there is any failure when iframe talk to pifd/px,
            // iframe should post error message back to pidlsdk and
            // let pidlsdk propagate to partner in the standard way
            var errorResponse = new ServiceErrorResponse()
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

            return new ClientAction(ClientActionType.Failure, errorResponse);
        }

        private static void AddJumpbackClientActionToError(
            ServiceErrorResponse error,
            PaymentInstrument pi,
            string country,
            string family,
            string type,
            string operation,
            string language,
            string partner,
            string classicProduct,
            string billableAccountId,
            bool completePrerequisites)
        {
            HashSet<PIMSModel.PaymentMethod> paymentMethodHashSet = new HashSet<PIMSModel.PaymentMethod>();
            paymentMethodHashSet.Add(pi.PaymentMethod);
            ClientAction clientAction = new ClientAction(ClientActionType.Pidl);
            clientAction.Context = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(paymentMethodHashSet, country, family, type, operation, language, partner, null, classicProduct, billableAccountId, null, completePrerequisites);
            error.ClientAction = clientAction;
        }

        private static void AddDirectDebitClientActionToError(
            ServiceErrorResponse error,
            PaymentInstrument pi,
            string descriptionType,
            string language,
            string partner,
            string classicProduct,
            string billableAccountId,
            PaymentExperienceSetting setting = null)
        {
            ClientAction clientAction = new ClientAction(ClientActionType.Pidl);
            clientAction.Context = PIDLResourceFactory.Instance.GetPicvChallengeDescriptionForPI(pi, descriptionType, language, partner, classicProduct, billableAccountId, setting: setting);
            error.ClientAction = clientAction;
        }

        private static void MapCreditCardReplacePIError(ref ServiceErrorResponseException ex, string language)
        {
            // Now for replace PI, it always returns the same generic error message as required.
            // TODO: add more cases once getting the list of error code from PIMS for replacing PI
            ex.Error.Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.Generic, language);
        }

        private static void MapRemovePIError(ref ServiceErrorResponseException ex, string language)
        {
            ex.Error.Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.Generic, language);
        }

        private static void MapCreditCardCommonError(ref ServiceErrorResponseException ex, string language)
        {
            if (string.Equals(ex.Error.ErrorCode, Constants.CreditCardErrorCodes.InvalidCvv, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.InvalidCvv, language),
                    Target = Constants.CreditCardErrorTargets.Cvv,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.CreditCardErrorCodes.InvalidAccountHolder, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.InvalidAccountHolder, language),
                    Target = Constants.CreditCardErrorTargets.AccountHolderName,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.CreditCardErrorCodes.ExpiredCard, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.ExpiredCard, language),
                    Target = string.Format("{0},{1}", Constants.CreditCardErrorTargets.ExpiryMonth, Constants.CreditCardErrorTargets.ExpiryYear),
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.CreditCardErrorCodes.InvalidExpiryDate, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.InvalidExpiryDate, language),
                    Target = string.Format("{0},{1}", Constants.CreditCardErrorTargets.ExpiryMonth, Constants.CreditCardErrorTargets.ExpiryYear),
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.CreditCardErrorCodes.InvalidCity, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.InvalidCity, language),
                    Target = Constants.CreditCardErrorTargets.City,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.CreditCardErrorCodes.InvalidState, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.InvalidState, language),
                    Target = Constants.CreditCardErrorTargets.State,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.CreditCardErrorCodes.InvalidZipCode, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.InvalidZipCode, language),
                    Target = Constants.CreditCardErrorTargets.PostalCode,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.CreditCardErrorCodes.InvalidCountryCode, StringComparison.OrdinalIgnoreCase)
                || string.Equals(ex.Error.ErrorCode, Constants.CreditCardErrorCodes.InvalidCountry, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.InvalidCountry, language),
                    Target = Constants.CreditCardErrorTargets.Country,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.CreditCardErrorCodes.InvalidAddress, StringComparison.OrdinalIgnoreCase))
            {
                // For InvalidAddress, PxService does not know the exact payload of address group.
                // Set full-set of address info as target, PIDL SDK will highlight only existed fields.
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.InvalidAddress, language),
                    Target = string.Format(
                    "{0},{1},{2},{3},{4},{5},{6}",
                    Constants.CreditCardErrorTargets.AddressLine1,
                    Constants.CreditCardErrorTargets.AddressLine2,
                    Constants.CreditCardErrorTargets.AddressLine3,
                    Constants.CreditCardErrorTargets.City,
                    Constants.CreditCardErrorTargets.State,
                    Constants.CreditCardErrorTargets.Country,
                    Constants.CreditCardErrorTargets.PostalCode),
                });
            }
            else
            {
                // Catch all for any other error scenario
                ex.Error.Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.Generic, language);
            }
        }

        private static void MapKlarnaAddError(ref ServiceErrorResponseException ex, string language)
        {
            if (string.Equals(ex.Error.ErrorCode, Constants.KlarnaErrorCodes.PersonalNumberBadFormat, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.KlarnaErrorMessages.PersonalNumberBadFormat, language),
                    Target = Constants.KlarnaErrorTargets.PersonalNumber,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.KlarnaErrorCodes.InvalidPersonalNumber, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.KlarnaErrorMessages.InvalidPersonalNumber, language),
                    Target = Constants.KlarnaErrorTargets.PersonalNumber,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.KlarnaErrorCodes.InvalidGender, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.KlarnaErrorMessages.InvalidGender, language),
                    Target = Constants.KlarnaErrorTargets.Gender,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.KlarnaErrorCodes.InvalidFirstName, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.KlarnaErrorMessages.InvalidFirstName, language),
                    Target = Constants.KlarnaErrorTargets.FirstName,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.KlarnaErrorCodes.InvalidLastName, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.KlarnaErrorMessages.InvalidLastName, language),
                    Target = Constants.KlarnaErrorTargets.LastName,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.KlarnaErrorCodes.InvalidFirstAndLastNames, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.KlarnaErrorMessages.InvalidFirstAndLastNames, language),
                    Target = string.Format(
                    "{0},{1}",
                    Constants.KlarnaErrorTargets.FirstName,
                    Constants.KlarnaErrorTargets.LastName),
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.KlarnaErrorCodes.InvalidNameAndAddress, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.KlarnaErrorMessages.InvalidNameAndAddress, language),
                    Target = string.Format(
                    "{0},{1},{2},{3},{4},{5},{6}",
                    Constants.KlarnaErrorTargets.AddressLine1,
                    Constants.KlarnaErrorTargets.AddressLine2,
                    Constants.KlarnaErrorTargets.City,
                    Constants.KlarnaErrorTargets.Country,
                    Constants.KlarnaErrorTargets.PostalCode,
                    Constants.KlarnaErrorTargets.FirstName,
                    Constants.KlarnaErrorTargets.LastName),
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.KlarnaErrorCodes.InvalidPhoneValue, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.KlarnaErrorMessages.InvalidPhoneValue, language),
                    Target = Constants.KlarnaErrorTargets.Phone,
                });
            }
            else
            {
                // Catch all for any other error scenario
                ex.Error.Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.Generic, language);
            }
        }

        private static void MapDirectDebitCommonError(ref ServiceErrorResponseException ex, string paymentMethodFamily, string paymentMethodType, string language)
        {
            if (string.Equals(ex.Error.ErrorCode, Constants.DirectDebitErrorCodes.InvalidPaymentInstrumentInfo, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                if (IsAch(paymentMethodFamily, paymentMethodType))
                {
                    ex.Error.AddDetail(new ServiceErrorDetail()
                    {
                        ErrorCode = ex.Error.ErrorCode,
                        Message = LocalizationRepository.Instance.GetLocalizedString(Constants.DirectDebitErrorMessages.InvalidPaymentInstrumentInfoAch, language),
                        Target = Constants.DirectDebitErrorTargets.AccountNumber,
                    });
                }
                else if (IsSepa(paymentMethodFamily, paymentMethodType))
                {
                    ex.Error.AddDetail(new ServiceErrorDetail()
                    {
                        ErrorCode = ex.Error.ErrorCode,
                        Message = LocalizationRepository.Instance.GetLocalizedString(Constants.DirectDebitErrorMessages.InvalidPaymentInstrumentInfoSepa, language),
                        Target = Constants.DirectDebitErrorTargets.AccountNumber,
                    });
                }
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.DirectDebitErrorCodes.InvalidAccountHolder, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.DirectDebitErrorMessages.InvalidAccountHolder, language),
                    Target = Constants.DirectDebitErrorTargets.AccountHolderName,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.DirectDebitErrorCodes.InvalidCity, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.DirectDebitErrorMessages.InvalidCity, language),
                    Target = Constants.DirectDebitErrorTargets.City,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.DirectDebitErrorCodes.InvalidState, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.DirectDebitErrorMessages.InvalidState, language),
                    Target = Constants.DirectDebitErrorTargets.State,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.DirectDebitErrorCodes.InvalidZipCode, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.DirectDebitErrorMessages.InvalidZipCode, language),
                    Target = Constants.DirectDebitErrorTargets.PostalCode,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.DirectDebitErrorCodes.InvalidAddress, StringComparison.OrdinalIgnoreCase))
            {
                // For InvalidAddress, PxService does not know the exact payload of address group.
                // Set full-set of address info as target, PIDL SDK will highlight only existed fields.
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.DirectDebitErrorMessages.InvalidAddress, language),
                    Target = string.Format(
                    "{0},{1},{2},{3},{4}",
                    Constants.DirectDebitErrorTargets.AddressLine1,
                    Constants.DirectDebitErrorTargets.AddressLine2,
                    Constants.DirectDebitErrorTargets.City,
                    Constants.DirectDebitErrorTargets.State,
                    Constants.DirectDebitErrorTargets.PostalCode),
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.DirectDebitErrorCodes.ValidationFailed, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                if (IsAch(paymentMethodFamily, paymentMethodType))
                {
                    ex.Error.AddDetail(new ServiceErrorDetail()
                    {
                        ErrorCode = ex.Error.ErrorCode,
                        Message = LocalizationRepository.Instance.GetLocalizedString(Constants.DirectDebitErrorMessages.ValidationFailedAch, language),
                        Target = string.Format(
                        "{0},{1}",
                        Constants.DirectDebitErrorTargets.BankCode,
                        Constants.DirectDebitErrorTargets.AccountNumber),
                    });
                }
                else if (IsSepa(paymentMethodFamily, paymentMethodType))
                {
                    ex.Error.AddDetail(new ServiceErrorDetail()
                    {
                        ErrorCode = ex.Error.ErrorCode,
                        Message = LocalizationRepository.Instance.GetLocalizedString(Constants.DirectDebitErrorMessages.ValidationFailedSepa, language),
                        Target = string.Format(
                        "{0},{1}",
                        Constants.DirectDebitErrorTargets.BankCode,
                        Constants.DirectDebitErrorTargets.AccountNumber),
                    });
                }
            }
            else
            {
                // Catch all for any other error scenario
                ex.Error.Message = LocalizationRepository.Instance.GetLocalizedString(Constants.DirectDebitErrorMessages.Generic, language);
            }
        }

        private static void MapAlipayAddError(ref ServiceErrorResponseException ex, string language)
        {
            if (string.Equals(ex.Error.ErrorCode, Constants.AlipayErrorCodes.InvalidAlipayAccount, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.AlipayErrorMessages.InvalidAlipayAccount, language),
                    Target = Constants.AlipayErrorTargets.Account,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.AlipayErrorCodes.UserMobileNotMatch, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.AlipayErrorMessages.UserMobileNotMatch, language),
                    Target = Constants.AlipayErrorTargets.PhoneNumber,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.AlipayErrorCodes.UserCertNoMatch, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.AlipayErrorMessages.UserCertNoMatch, language),
                    Target = Constants.AlipayErrorTargets.LastFiveCertNo,
                });
            }
            else
            {
                // Catch all for any other error scenario
                ex.Error.Message = LocalizationRepository.Instance.GetLocalizedString(Constants.AlipayErrorMessages.Generic, language);
            }
        }

        private static void MapUpiAddError(ref ServiceErrorResponseException ex, string language)
        {
            if (string.Equals(ex.Error.ErrorCode, Constants.UpiErrorCodes.InvalidUpiAccount, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.UpiErrorMessages.InvalidUpiAccount, language),
                    Target = Constants.UpiErrorTargets.Account,
                });
            }
            else
            {
                // Catch all for any other error scenario
                ex.Error.Message = LocalizationRepository.Instance.GetLocalizedString(Constants.UpiErrorMessages.Generic, language);
            }
        }

        private static void MapNonSimAddError(ref ServiceErrorResponseException ex, string language, string partnerName)
        {
            if (string.Equals(ex.Error.ErrorCode, Constants.NonSimErrorCodes.MOAccountNotFound, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(ex.Error.ErrorCode, Constants.NonSimErrorCodes.RiskRejected, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.NonSimErrorMessages.MOAccountNotFound, language),
                    Target = Constants.NonSimErrorTargets.PhoneNumber,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.NonSimErrorCodes.PaymentInstrumentAddAlready, StringComparison.OrdinalIgnoreCase) && string.Equals(partnerName, PXCommon.Constants.PartnerNames.XboxSettings, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.NonSimErrorMessages.PaymentInstrumentAddAlready, language),
                    Target = Constants.NonSimErrorTargets.PhoneNumber,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.NonSimErrorCodes.RejectedByProvider, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.NonSimErrorMessages.RejectedByProvider, language),
                    Target = Constants.NonSimErrorTargets.PhoneNumber,
                });
            }
            else
            {
                // Catch all for any other error scenario
                ex.Error.Message = LocalizationRepository.Instance.GetLocalizedString(Constants.NonSimErrorMessages.Generic, language);
            }
        }

        private static void MapAlipayResumeError(ref ServiceErrorResponseException ex, string language)
        {
            if (string.Equals(ex.Error.ErrorCode, Constants.AlipayErrorCodes.InvalidChallengeCode, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.AlipayErrorMessages.InvalidChallengeCode, language),
                    Target = Constants.AlipayErrorTargets.Sms,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.AlipayErrorCodes.ChallengeCodeExpired, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.AlipayErrorMessages.ChallengeCodeExpired, language),
                    Target = Constants.AlipayErrorTargets.Sms,
                });
            }
            else
            {
                // Catch all for any other error scenario
                ex.Error.Message = LocalizationRepository.Instance.GetLocalizedString(Constants.AlipayErrorMessages.Generic, language);
            }
        }

        private static void MapNonSimResumeError(ref ServiceErrorResponseException ex, string language)
        {
            if (string.Equals(ex.Error.ErrorCode, Constants.NonSimErrorCodes.InvalidChallengeCode, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.NonSimErrorMessages.InvalidChallengeCode, language),
                    Target = Constants.NonSimErrorTargets.Sms,
                });
            }
            else
            {
                // Catch all for any other error scenario
                ex.Error.Message = LocalizationRepository.Instance.GetLocalizedString(Constants.NonSimErrorMessages.Generic, language);
            }
        }

        private static void MapLegacyInvoiceAddError(ref ServiceErrorResponseException ex, string language)
        {
            if (string.Equals(ex.Error.ErrorCode, Constants.LegacyInvoiceErrorCodes.InvalidCity, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.LegacyInvoiceErrorMessages.InvalidCity, language),
                    Target = Constants.LegacyInvoiceErrorTargets.City,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.LegacyInvoiceErrorCodes.InvalidState, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.LegacyInvoiceErrorMessages.InvalidState, language),
                    Target = Constants.LegacyInvoiceErrorTargets.State,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.LegacyInvoiceErrorCodes.InvalidZipCode, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.LegacyInvoiceErrorMessages.InvalidZipCode, language),
                    Target = Constants.LegacyInvoiceErrorTargets.PostalCode,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.LegacyInvoiceErrorCodes.InvalidCountryCode, StringComparison.OrdinalIgnoreCase)
                || string.Equals(ex.Error.ErrorCode, Constants.LegacyInvoiceErrorCodes.InvalidCountry, StringComparison.OrdinalIgnoreCase))
            {
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.LegacyInvoiceErrorMessages.InvalidCountry, language),
                    Target = Constants.LegacyInvoiceErrorTargets.Country,
                });
            }
            else if (string.Equals(ex.Error.ErrorCode, Constants.LegacyInvoiceErrorCodes.InvalidAddress, StringComparison.OrdinalIgnoreCase))
            {
                // For InvalidAddress, PxService does not know the exact payload of address group.
                // Set full-set of address info as target, PIDL SDK will highlight only existed fields.
                ex.Error.Message = Constants.ClientActionContract.NoMessage;
                ex.Error.AddDetail(new ServiceErrorDetail()
                {
                    ErrorCode = ex.Error.ErrorCode,
                    Message = LocalizationRepository.Instance.GetLocalizedString(Constants.LegacyInvoiceErrorMessages.InvalidAddress, language),
                    Target = string.Format(
                    "{0},{1},{2},{3},{4},{5},{6}",
                    Constants.LegacyInvoiceErrorTargets.AddressLine1,
                    Constants.LegacyInvoiceErrorTargets.AddressLine2,
                    Constants.LegacyInvoiceErrorTargets.AddressLine3,
                    Constants.LegacyInvoiceErrorTargets.City,
                    Constants.LegacyInvoiceErrorTargets.State,
                    Constants.LegacyInvoiceErrorTargets.Country,
                    Constants.LegacyInvoiceErrorTargets.PostalCode),
                });
            }
            else
            {
                // Catch all for any other error scenario
                ex.Error.Message = LocalizationRepository.Instance.GetLocalizedString(Constants.LegacyInvoiceErrorMessages.Generic, language);
            }
        }

        private static bool IsChinaUnionPay(string family, string type)
        {
            bool isCardFamily = string.Equals(family, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase);
            bool isCUPCreditType = string.Equals(type, Constants.PaymentMethodType.UnionPayCreditCard.ToString(), StringComparison.OrdinalIgnoreCase);
            bool isCUPDebitType = string.Equals(type, Constants.PaymentMethodType.UnionPayDebitCard.ToString(), StringComparison.OrdinalIgnoreCase);
            return isCardFamily && (isCUPCreditType || isCUPDebitType);
        }

        private static bool IsAliPay(string family, string type)
        {
            bool isAlipayFamily = string.Equals(family, Constants.PaymentMethodFamily.ewallet.ToString(), StringComparison.OrdinalIgnoreCase);
            bool isAlipayType = string.Equals(type, Constants.PaymentMethodType.AlipayBillingAgreement.ToString(), StringComparison.OrdinalIgnoreCase);
            return isAlipayFamily && isAlipayType;
        }

        private static bool IsKlarna(string family, string type)
        {
            bool isKlarnaFamily = string.Equals(family, Constants.PaymentMethodFamily.invoice_credit.ToString(), StringComparison.OrdinalIgnoreCase);
            bool isKlarnaType = string.Equals(type, Constants.PaymentMethodType.Klarna.ToString(), StringComparison.OrdinalIgnoreCase);
            return isKlarnaFamily && isKlarnaType;
        }

        private static bool IsNonSim(string family)
        {
            return string.Equals(family, Constants.PaymentMethodFamily.mobile_billing_non_sim.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsNonSimMobi(PaymentInstrument paymentInstrument)
        {
            return paymentInstrument != null && paymentInstrument.PaymentMethod != null && string.Equals(paymentInstrument.PaymentMethod.PaymentMethodFamily, Constants.PaymentMethodFamily.mobile_billing_non_sim.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsPaypal(PaymentInstrument paymentInstrument)
        {
            return paymentInstrument != null && paymentInstrument.PaymentMethod != null && string.Equals(paymentInstrument.PaymentMethod.PaymentMethodType, Constants.PaymentMethodType.PayPal, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsIdealBillingAgreement(PaymentInstrument paymentInstrument)
        {
            return string.Equals(paymentInstrument.PaymentMethod.PaymentMethodType, Constants.PaymentMethodType.IdealBillingAgreement, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsAllowedEmptyPaymentTypeDuringPost(string family, string type, string country)
        {
            return string.IsNullOrEmpty(type) && string.Equals(family, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase) && string.Equals(country, "kr", StringComparison.OrdinalIgnoreCase);
        }

        // TODO: As part of T-54279751 investigation, the AddressNoCityState flow found to be not used by any partner
        // And below code & csv config related to it can be removed
        private static bool IsAddressNoCityStatePI(PIDLData pi)
        {
            return !pi.ContainsProperty("details.address.city") && !pi.ContainsProperty("details.address.region");
        }

        private static string RetrieveQueryParameterValue(IEnumerable<KeyValuePair<string, string>> queryParameters, string parameterName)
        {
            return queryParameters.FirstOrDefault(pair => string.Equals(pair.Key, parameterName, StringComparison.InvariantCultureIgnoreCase)).Value;
        }

        private static PaymentInstrument CreateAddtionalPaymentInstrument(string displayName, string family, string type)
        {
            return new PaymentInstrument()
            {
                PaymentMethod = new PIMSModel.PaymentMethod() { Display = new PaymentInstrumentDisplayDetails() { Name = displayName }, PaymentMethodFamily = family, PaymentMethodType = type },
                PaymentInstrumentId = Guid.NewGuid().ToString(),
                Status = PaymentInstrumentStatus.Active,
            };
        }

        private static bool TryUpdateAddress(PIDLData pi, string city, string region)
        {
            // if pi properties were set sucessfully return true - else false
            return pi.TrySetProperty("details.address.city", city) && pi.TrySetProperty("details.address.region", region);
        }

        private static PIDLResource ConstructAddressEnrichmentFailurePidl(
            List<string> displayedPaymentMethodTypes,
            EventTraceActivity traceActivityId,
            string paymentMethodFamily,
            string paymentMethodType,
            string language,
            string country,
            string partner,
            List<string> exposedFlightFeatures,
            bool includeZipErrorCode = false,
            bool completePrerequisites = false)
        {
            paymentMethodType = string.Join(",", displayedPaymentMethodTypes);
            ActionContext optionContext = new ActionContext()
            {
                Action = PaymentInstrumentActions.ToString(PIActionType.AddResource),
                Id = $"{paymentMethodFamily}.{paymentMethodType}",
                PaymentMethodFamily = paymentMethodFamily,
                PaymentMethodType = paymentMethodType
            };

            PidlDocInfo docInfo = null;

            // TODO: T-54217106: RS5 scenario is deprecating and not required to be added in templates
            if (PartnerHelper.IsOfficeOobePartner(partner)
                || PartnerHelper.IsOXOOobePartner(partner)
                || PartnerHelper.IsSmbOobePartner(partner))
            {
                docInfo = new PidlDocInfo(Constants.DescriptionTypes.PaymentMethodDescription, language, country, partner, paymentMethodType, paymentMethodFamily, Constants.ScenarioNames.RS5);
            }
            else
            {
                docInfo = new PidlDocInfo(Constants.DescriptionTypes.PaymentMethodDescription, language, country, partner, paymentMethodType, paymentMethodFamily);
            }

            optionContext.ResourceActionContext = new ResourceActionContext()
            {
                Action = PaymentInstrumentActions.ToString(PIActionType.AddResource),
                PidlDocInfo = docInfo
            };

            var fallbackPidlResource = new PIDLResource()
            {
                ClientAction = new ClientAction(ClientActionType.Pidl, optionContext)
            };

            fallbackPidlResource.ClientAction.PidlRetainUserInput = true;
            var errorData = new PIDLError()
            {
                Code = Constants.CreditCardErrorCodes.InvalidZipCode
            };

            var stateErrorDataDetail = new PIDLErrorDetail()
            {
                Code = Constants.CreditCardErrorCodes.InvalidState,
                Message = Constants.CreditCardErrorMessages.InvalidState,
                Target = Constants.CreditCardErrorTargets.State
            };

            var cityErrorDataDetail = new PIDLErrorDetail()
            {
                Code = Constants.CreditCardErrorCodes.InvalidCity,
                Message = Constants.CreditCardErrorMessages.InvalidCity,
                Target = Constants.CreditCardErrorTargets.City
            };

            errorData.AddDetail(stateErrorDataDetail);
            errorData.AddDetail(cityErrorDataDetail);

            if (includeZipErrorCode)
            {
                var zipCodeErrorDataDetail = new PIDLErrorDetail()
                {
                    Code = Constants.CreditCardErrorCodes.InvalidZipCode,
                    Message = Constants.CreditCardErrorMessages.InvalidZipCode,
                    Target = Constants.CreditCardErrorTargets.PostalCode
                };

                errorData.AddDetail(zipCodeErrorDataDetail);
            }

            fallbackPidlResource.ClientAction.PidlError = errorData;

            if (completePrerequisites)
            {
                docInfo.Parameters.Add("completePrerequisites", "true");
            }

            return fallbackPidlResource;
        }

        private static bool GetShowChallengeParameterFromPI(PIDLData pi)
        {
            bool result = false;
            if (pi != null && pi.ContainsKey(Constants.PaymentInstrument.Details) && pi[Constants.PaymentInstrument.Details] != null)
            {
                dynamic pidetails = pi[Constants.PaymentInstrument.Details];
                if (pidetails[Constants.DataDescriptionPropertyNames.ResourceActionContext] == null)
                {
                    return result;
                }

                dynamic getResourceActionContext = Newtonsoft.Json.JsonConvert.DeserializeObject(pidetails[Constants.DataDescriptionPropertyNames.ResourceActionContext].ToString());
                dynamic getParameter = getResourceActionContext?.resourceActionContext?.pidlDocInfo?.parameters;
                Dictionary<string, string> parameters = getParameter?.ToObject<Dictionary<string, string>>();
                if (parameters != null && parameters.Keys != null && parameters.Keys.Count > 0 && parameters.ContainsKey(Constants.QueryParameterName.ShowChallenge))
                {
                    bool temp;
                    if (bool.TryParse(parameters[Constants.QueryParameterName.ShowChallenge], out temp))
                    {
                        result = temp;
                    }
                }
            }

            return result;
        }

        private static PIDLData AddChallengeEvidenceToPidlData(PIDLData pi, string pxChallengeSessionId)
        {
            dynamic piDetails;
            if (pi.TryGetValue(Constants.PaymentInstrument.Details, out piDetails))
            {
                piDetails[Constants.DataDescriptionPropertyNames.ChallengeEvidence] = JToken.FromObject(new ChallengeEvidenceData
                {
                    ChallengeType = Constants.ChallengeEvidenceTypes.Challenge,
                    ChallengeId = pxChallengeSessionId,
                    ChallengeResult = Constants.ChallengeEvidenceResults.Success,
                    ChallengeResultReason = Constants.ChallengeEvidenceResults.Success
                });
            }

            return pi;
        }

        private static List<PIDLResource> ProcessRedeemCSVPIDL(List<PIDLResource> pidlResources, string country, string language, string partner, string tokenIdentifierValue, string formattedCurrency)
        {
            foreach (var pidlResource in pidlResources)
            {
                // Add default values to the data description
                var tokenIddataDescription = pidlResource.DataDescription[Constants.PropertyDescriptionIds.RedeemTokenIdentifierValue] as PropertyDescription;
                if (tokenIddataDescription != null)
                {
                    tokenIddataDescription.DefaultValue = tokenIdentifierValue;
                }

                var tokenActionDescription = pidlResource.DataDescription[Constants.PropertyDescriptionIds.RedeemTokenActionType] as PropertyDescription;
                if (tokenActionDescription != null)
                {
                    tokenActionDescription.DefaultValue = "redeem";
                }

                var csvAmountText = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.CsvAmountText) as TextDisplayHint;
                if (csvAmountText != null)
                {
                    csvAmountText.DisplayContent = string.Format(csvAmountText.DisplayContent, formattedCurrency);
                }

                var giftCardTokenText = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.GiftCardToken) as TextDisplayHint;
                if (giftCardTokenText != null)
                {
                    giftCardTokenText.DisplayContent = string.Format(giftCardTokenText.DisplayContent, tokenIdentifierValue).ToUpper();
                }

                // Add a PidlAction to the submit button
                var saveConfirmButton = pidlResource.GetDisplayHintById(Constants.DisplayHintIds.SaveConfirmButton) as ButtonDisplayHint;
                if (saveConfirmButton != null)
                {
                    var redeemCSVActionContext = new PXCommon.RestLink();
                    redeemCSVActionContext.Href = $"{Constants.SubmitUrls.RedeemCSVToken}?partner={partner}&language={language}&country={country}";
                    redeemCSVActionContext.Method = "POST";
                    saveConfirmButton.Action = new DisplayHintAction(DisplayHintActionType.submit.ToString(), true, redeemCSVActionContext, null);
                }
            }

            return pidlResources;
        }

        private static void AddCardArt(PaymentInstrument paymentInstrument, Dictionary<string, NetworkToken> tokenIndex)
        {
            var networkTokens = paymentInstrument?.PaymentInstrumentDetails?.NetworkTokens;
            if (networkTokens == null)
            {
                return;
            }

            NetworkToken tokenFromIndex = null;

            foreach (var networkToken in networkTokens)
            {
                if (tokenIndex.TryGetValue(networkToken.Id, out tokenFromIndex))
                {
                    break;
                }
            }

            paymentInstrument.PaymentMethod.Display.CardArt = tokenFromIndex?.CardMetadata;
        }

        private static bool IsAnonymousSecondScreen(string scenario)
        {
            return scenario != null && scenario.Equals(Constants.ScenarioNames.SecondScreenAddPi) ? true : false;
        }

        private static void SetPiData(PIDLData pi, Dictionary<string, object> data)
        {
            foreach (var item in data)
            {
                if (pi.ContainsKey(item.Key))
                {
                    pi[item.Key] = item.Value;
                }
                else
                {
                    pi.Add(item.Key, item.Value);
                }
            }
        }

        private async Task<HttpResponseMessage> HandleAnonymousAdd(PIDLData pi, string sessionId, string language, string partner, EventTraceActivity traceActivityId, string country, string scenario, string orderId, bool completePrerequisites, string billableAccountId)
        {
            string accountId = await this.AddCcQrCodeAnonymousCallCheck(pi, partner, sessionId, traceActivityId);
            if (accountId == null)
            {
                ErrorResponse invalidAccount = new ErrorResponse(
                    Constants.CreditCardErrorCodes.InvalidRequestData,
                    "Invalid account ID returned from AddCcQrCodeAnonymousCallCheck");
                return Request.CreateResponse(HttpStatusCode.BadRequest, invalidAccount);
            }

            HttpResponseMessage response = await this.AddNewPI(
                traceActivityId: traceActivityId,
                accountId: accountId,
                pi: pi,
                language: language,
                partner: partner,
                billableAccountId: billableAccountId,
                completePrerequisites: completePrerequisites,
                country: country,
                scenario: scenario,
                orderId: orderId);

            PaymentInstrument newPi = await response.Content.ReadFromJsonAsync<PaymentInstrument>();

            if (newPi != null)
            {
                await this.UpdateAnonymousAddCCCall(newPi, sessionId, newPi.Status, newPi.PaymentInstrumentId, traceActivityId);
            }
            else
            {
                await this.UpdateAnonymousAddCCCall(null, sessionId, PaymentInstrumentStatus.Cancelled, null, traceActivityId);
            }

            return response;
        }

        private void ValidateQueryParametersForCompletePrerequisites(bool completePrerequisites, EventTraceActivity traceActivityId)
        {
            if (completePrerequisites)
            {
                string country = RetrieveQueryParameterValue(this.Request.Query.Select(q => new KeyValuePair<string, string>(q.Key, q.Value)), Constants.QueryParameterName.Country);
                if (string.IsNullOrWhiteSpace(country))
                {
                    throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, string.Format("response status code: {0}, error: {1}", "MissingQueryParameter", "Country is required.")));
                }

                string partner = RetrieveQueryParameterValue(this.Request.Query.Select(q => new KeyValuePair<string, string>(q.Key, q.Value)), Constants.QueryParameterName.Partner);
                if (string.IsNullOrWhiteSpace(partner))
                {
                    throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, string.Format("response status code: {0}, error: {1}", "MissingQueryParameter", "Partner is required.")));
                }
            }
        }

        private async Task<string> CreateShortUrl(string longUrl, int? ttlMinutes, EventTraceActivity traceActivityId)
        {
            try
            {
                CreateShortURLResponse shortUrlCreateResponse = await this.Settings.ShortURLServiceAccessor.CreateShortURL(longUrl, ttlMinutes, traceActivityId);
                return shortUrlCreateResponse.Uri.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<string> CreateShortUrlForPaymentInstrumentAndFlight(PaymentInstrument newPI, string language, EventTraceActivity traceActivityId, string sessionId = null, string country = null, PaymentExperienceSetting setting = null)
        {
            string redirectUrl = null;

            if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableShortUrlPayPal) &&
                string.Equals(newPI.PaymentMethod.PaymentMethodType, Constants.PaymentMethodType.PayPal, StringComparison.OrdinalIgnoreCase))
            {
                redirectUrl = PIDLResourceFactory.GetRedirectURL(newPI, language, Constants.ChallengeDescriptionTypes.PaypalQrCodeXboxNative, "qrCode");
            }
            else if ((PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.ShortURLPaypal, country, setting) ||
                PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.EnableShortURL, country, setting)) &&
                string.Equals(newPI.PaymentMethod.PaymentMethodType, Constants.PaymentMethodType.PayPal, StringComparison.OrdinalIgnoreCase))
            {
                redirectUrl = PIDLResourceFactory.GetRedirectURL(newPI, language, Constants.ChallengeDescriptionTypes.PaypalQrCode, "qrCode");
            }
            else if ((this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableShortUrlVenmo) || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.ShortURLVenmo, country, setting) || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.EnableShortURL, country, setting)) &&
                string.Equals(newPI.PaymentMethod.PaymentMethodType, Constants.PaymentMethodType.Venmo, StringComparison.OrdinalIgnoreCase))
            {
                redirectUrl = PIDLResourceFactory.GetRedirectURL(newPI, language, Constants.ChallengeDescriptionTypes.VenmoQrCode, "qrCode", sessionId);
            }

            return redirectUrl != null
                ? await this.CreateShortUrl(redirectUrl, Constants.ShortURLServiceTimeToLive.ShortURLActiveTTL, traceActivityId)
                : null;
        }

        private async Task<PaymentInstrument> HandleProfileAddress(string accountId, PaymentInstrument paymentInstrument, bool completePrerequisites, string language, string partner, string country, HttpMethod httpMethod, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures = null, PaymentExperienceSetting setting = null)
        {
            if (completePrerequisites && paymentInstrument != null)
            {
                if ((httpMethod == HttpMethod.Post && paymentInstrument.Status == PaymentInstrumentStatus.Active && !IsNonSimMobi(paymentInstrument)) ||
                    (httpMethod == HttpMethod.Get && (IsPaypal(paymentInstrument) || IsAliPay(paymentInstrument.PaymentMethod.PaymentMethodFamily, paymentInstrument.PaymentMethod.PaymentMethodType)) && paymentInstrument.Status == PaymentInstrumentStatus.Active) ||
                    (httpMethod == HttpMethod.Get && IsIdealBillingAgreement(paymentInstrument) && (paymentInstrument.Status == PaymentInstrumentStatus.Active || paymentInstrument.Status == PaymentInstrumentStatus.Pending)))
                {
                    string profileType = this.GetProfileType();

                    // Only handle profile address (step 3 for prerequisites) for consumer profile
                    // Commercial profile does not support PI without address (ex, paypal) now, PxService can't verify the following scenario
                    if (string.Equals(profileType, GlobalConstants.ProfileTypes.Consumer, StringComparison.OrdinalIgnoreCase))
                    {
                        bool overrideJarvisVersionToV3 = false;
                        if (GuestAccountHelper.IsGuestAccount(this.Request) ||
                            (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXUseJarvisV3ForCompletePrerequisites, StringComparer.OrdinalIgnoreCase)))
                        {
                            overrideJarvisVersionToV3 = true;
                        }

                        if (overrideJarvisVersionToV3)
                        {
                            AccountProfileV3 profileV3 = await this.Settings.AccountServiceAccessor.GetProfileV3(accountId, profileType, traceActivityId);
                            AddressInfoV3 address = await this.ReturnDefaultAddressV3ByCountry(accountId, profileV3, profileType, country, traceActivityId);
                            if (address == null)
                            {
                                ClientActionFactory.AddProfileV3AddressClientActionToPaymentInstrument(paymentInstrument, country, Constants.AddressTypes.Billing, language, partner, false, profileV3, traceActivityId, exposedFlightFeatures, setting);
                            }
                        }
                        else
                        {
                            AccountProfile profile = await this.Settings.AccountServiceAccessor.GetProfile(accountId, profileType, traceActivityId);
                            AddressInfo address = await this.ReturnDefaultAddressByCountry(accountId, profile, country, traceActivityId);
                            if (address == null)
                            {
                                ClientActionFactory.AddProfileAddressClientActionToPaymentInstrument(paymentInstrument, country, Constants.AddressTypes.Billing, language, partner, false, profile, overrideJarvisVersionToV3, traceActivityId, exposedFlightFeatures, setting);
                            }
                        }
                    }
                }
            }

            return paymentInstrument;
        }

        private async Task AddRiskDataPayloadToPI(PIDLData pi)
        {
            string riskDataPropertyName = "riskData";

            // Add the userInfo field to the request payload
            JsonSerializerSettings serializationSettings = new JsonSerializerSettings();
            serializationSettings.NullValueHandling = NullValueHandling.Ignore;
            serializationSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            RiskData riskDataObject = null;
            string riskDataPayload;

            string paymentMethodFamily = pi["paymentMethodFamily"].ToString();
            string paymentMethodType = pi["paymentMethodType"].ToString();

            riskDataObject = new RiskData();
            riskDataObject.DeviceId = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.DeviceInfo.XboxLiveDeviceId);

            // x-ms-deviceinfo from PIFD can contain multiple IP addresses, we are using the first IP address (user's IP address), as the subsequent IP addresses could be from the other proxy layers
            string ipAddress = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.DeviceInfo.IPAddress);
            if (!string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = ipAddress.Split(',')[0];
            }

            riskDataObject.IPAddress = ipAddress;
            riskDataPayload = JsonConvert.SerializeObject(riskDataObject, serializationSettings);
            pi[riskDataPropertyName] = JsonConvert.DeserializeObject<PIDLData>(riskDataPayload, serializationSettings);
        }

        private async Task AddParameterToPIRiskData(PIDLData pi, string propertyName, string propertyPath, string partner = null, string paymentMethodType = null, string operation = null, string country = null)
        {
            string riskDataPropertyName = "riskData";
            string propertyValue = await this.TryGetClientContext(propertyPath);

            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(operation);

            // Xbox does not send user agent for venmo, but required for venmo flow.
            if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner) && string.IsNullOrEmpty(propertyValue) && string.Equals(paymentMethodType, Constants.PaymentMethodType.Venmo, StringComparison.OrdinalIgnoreCase))
            {
                propertyValue = Constants.XboxConsoleBrowserAgent.Xbox;
            }
            else if (PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.AddVenmoForWindows, country, setting) && string.IsNullOrEmpty(propertyValue) && string.Equals(paymentMethodType, Constants.PaymentMethodType.Venmo, StringComparison.OrdinalIgnoreCase))
            {
                propertyValue = Constants.WindowsBrowserAgent.Windows;
            }

            if (!string.IsNullOrEmpty(propertyValue))
            {
                // x-ms-deviceinfo from PIFD can contain multiple IP addresses, we are using the first IP address (user's IP address), as the subsequent IP addresses could be from the other proxy layers
                if (string.Equals(propertyName, Constants.DeviceInfoProperty.IPAddress))
                {
                    propertyValue = propertyValue.Split(',')[0];
                }

                JsonSerializerSettings serializationSettings = new JsonSerializerSettings();
                serializationSettings.NullValueHandling = NullValueHandling.Ignore;
                serializationSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                RiskData riskDataObject = null;
                string riskDataPayload;

                if (pi.ContainsKey(riskDataPropertyName) && pi[riskDataPropertyName] != null)
                {
                    riskDataPayload = JsonConvert.SerializeObject(pi[riskDataPropertyName], serializationSettings);
                    riskDataObject = JsonConvert.DeserializeObject<RiskData>(riskDataPayload, serializationSettings);
                }
                else
                {
                    riskDataObject = new RiskData();
                }

                riskDataObject.GetType().GetProperty(propertyName).SetValue(riskDataObject, propertyValue);

                riskDataPayload = JsonConvert.SerializeObject(riskDataObject, serializationSettings);
                pi[riskDataPropertyName] = JsonConvert.DeserializeObject<PIDLData>(riskDataPayload, serializationSettings);
            }
        }

        private async Task RaisePIAttachOnOfferEvent(EventTraceActivity traceActivityId, PaymentInstrument newPI, string partner, string country, string offerId, List<string> exposedFlightFeatures = null)
        {
            try
            {
                // The feature flag PXDisableRaisePIAddedOnOfferEvent is used to disable the telemetry for PI added on offer event.
                if (exposedFlightFeatures?.Contains(Flighting.Features.PXDisableRaisePIAddedOnOfferEvent, StringComparer.OrdinalIgnoreCase) ?? false)
                {
                    return;
                }

                if (newPI.Status == PaymentInstrumentStatus.Active
                    && Constants.PIAttachIncentivePartners.Contains(partner)
                    && offerId != null)
                {
                    string puid = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.Puid);
                    SllWebLogger.TracePXServicePIAddedOnOffer(
                        serviceName: PXCommon.Constants.ServiceNames.PXService,
                        request: this.Request,
                        requestTraceId: traceActivityId.ToString(),
                        paymentInstrumentId: newPI.PaymentInstrumentId,
                        paymentMethodFamily: newPI.PaymentMethod?.PaymentMethodFamily,
                        paymentMethodType: newPI.PaymentMethod?.PaymentMethodType,
                        partner: partner,
                        country: country,
                        offerId: offerId,
                        puid: puid ?? "unknownPuid"); // lgtm[cs/secret-data-in-code] -Suppressing because of a false positive from Semmle
                }
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException("PaymentInstrumentsExController.RaisePIAttachOnOffer: " + ex.ToString(), traceActivityId);
            }
        }

        private async void TryAddressNoCityStateCompletePrerequisites(
            PIDLData pi,
            string accountId,
            string country,
            string addressLineOne,
            string zipcode,
            string city,
            string state,
            EventTraceActivity traceActivityId)
        {
            // TODO: As part of T-54279751 investigation, the AddressNoCityState flow found to be not used by any partner
            // And below code & csv config related to it can be removed
            try
            {
                var profileType = this.GetProfileType();
                AccountProfile profile = null;
                AccountProfileV3 profileV3 = null;
                AddressInfo completePrerequisitesAddressData = null;
                AddressInfoV3 completePrerequisitesAddressDataV3 = null;
                bool overrideJarvisVersionToV3 = GuestAccountHelper.IsGuestAccount(this.Request) || this.ExposedFlightFeatures.Contains(Flighting.Features.PXUseJarvisV3ForCompletePrerequisites);

                if (overrideJarvisVersionToV3)
                {
                    profileV3 = await this.Settings.AccountServiceAccessor.GetProfileV3(accountId, profileType, traceActivityId);
                    completePrerequisitesAddressDataV3 = await this.ReturnDefaultAddressV3ByCountry(accountId, profileV3, profileType, country, traceActivityId);
                    if (completePrerequisitesAddressDataV3 == null)
                    {
                        var prerequisitesAddressV3 = new AddressInfoV3()
                        {
                            AddressLine1 = addressLineOne,
                            AddressLine2 = pi.TryGetPropertyValue("details.address.address_line2"),
                            City = city,
                            Country = country,
                            PostalCode = zipcode,
                            Region = state,
                        };

                        var responseV3 = await this.Settings.AccountServiceAccessor.PostAddress(accountId, prerequisitesAddressV3, GlobalConstants.AccountServiceApiVersion.V3, traceActivityId);

                        if (responseV3 != null && responseV3.Id != null)
                        {
                            profileV3.DefaultAddressId = responseV3.Id;
                            await this.Settings.AccountServiceAccessor.UpdateProfileV3(accountId, profileV3, profileType, traceActivityId, this.ExposedFlightFeatures);
                        }
                    }
                }
                else
                {
                    profile = await this.Settings.AccountServiceAccessor.GetProfile(accountId, profileType, traceActivityId);
                    completePrerequisitesAddressData = await this.ReturnDefaultAddressByCountry(accountId, profile, country, traceActivityId);
                    if (completePrerequisitesAddressData == null)
                    {
                        var prerequisitesAddress = new AddressInfo()
                        {
                            AddressLine1 = addressLineOne,
                            AddressLine2 = pi.TryGetPropertyValue("details.address.address_line2"),
                            City = city,
                            Country = country,
                            Zip = zipcode,
                            State = state,
                        };

                        var response = await this.Settings.AccountServiceAccessor.PostAddress(accountId, prerequisitesAddress, GlobalConstants.AccountServiceApiVersion.V2, traceActivityId);

                        if (response != null && response.Id != null)
                        {
                            profile.DefaultAddressId = response.Id;
                            await this.Settings.AccountServiceAccessor.UpdateProfile(accountId, profile, traceActivityId);
                        }
                    }
                }
            }
            catch
            {
                SllWebLogger.TracePXServiceIntegrationError("AddressNoCityState", IntegrationErrorCode.InvalidRequestParameterFormat, "Complete prerequisites was not successful for AddressNoCityState.", traceActivityId.ToString());
            }
        }

        private async Task<HttpResponseMessage> AddNewPI(
            EventTraceActivity traceActivityId,
            string accountId,
            PIDLData pi,
            string language = "en",
            string partner = Constants.ServiceDefaults.DefaultPartnerName,
            string classicProduct = null,
            string billableAccountId = null,
            bool completePrerequisites = false,
            string country = null,
            string scenario = null,
            string orderId = null,
            bool isReplacePI = false,
            string requestId = null)
        {
            // Bug 1686756:Consider remove ValidateQueryParametersForCompletePrerequisites from GET PI POST PI and Resume PI once pidl sdk complete prerequisite integration is completed
            this.ValidateQueryParametersForCompletePrerequisites(completePrerequisites, traceActivityId);

            // Validate incoming PI payload
            if (pi == null)
            {
                throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, string.Format("response status code: {0}, error: {1}", "InvalidPIData", "The input PI data is invalid.")));
            }

            if (!pi.ContainsKey("paymentMethodFamily"))
            {
                throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, string.Format("response status code: {0}, error: {1}", "InvalidPIData", "paymentMethodFamily is missing")));
            }

            const string CvvTokenPath = "details.cvvToken";
            string paymentMethodFamily = pi["paymentMethodFamily"].ToString();

            string paymentMethodType = null;
            if (!pi.ContainsKey("paymentMethodType"))
            {
                if (!IsAllowedEmptyPaymentTypeDuringPost(paymentMethodFamily, paymentMethodType, country))
                {
                    throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, string.Format("response status code: {0}, error: {1}", "InvalidPIData", "paymentMethodType is missing")));
                }
            }
            else
            {
                paymentMethodType = pi["paymentMethodType"].ToString();
            }

            this.Request.AddTracingProperties(accountId, null, paymentMethodFamily, paymentMethodType, country);

            string pxChallengeSessionId;
            bool isPXChallengeRequired = false;
            this.Request.Query.TryGetValue(Constants.QueryParameterName.PXChallengeSessionId, out var pxChallengeSessionIdValues);
            pxChallengeSessionId = pxChallengeSessionIdValues.ToString();

            //// If switch and enable challenge by default flight is On, show the challenge
            if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXChallengeSwitch, StringComparer.OrdinalIgnoreCase)
                    && this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableChallenge, StringComparer.OrdinalIgnoreCase)
                    && IsCreditCard(paymentMethodFamily, paymentMethodType)
                    && string.IsNullOrEmpty(pxChallengeSessionId))
            {
                SllWebLogger.TraceServerMessage("AddNewPI", traceActivityId.CorrelationVectorV4.Value, traceActivityId.ActivityId.ToString(), $"Challenge triggered by PXChallengeSwitch and PXEnableChallenge for PXChallengeSessionId:{pxChallengeSessionId}", EventLevel.Informational);
                return await this.CreatePXChallengeResponse(pi, language, partner, pxChallengeSessionId, traceActivityId, null, accountId);
            }

            //// PX Challenge Session state validation
            if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXChallengeSwitch, StringComparer.OrdinalIgnoreCase)
                && IsCreditCard(paymentMethodFamily, paymentMethodType)
                && !string.IsNullOrEmpty(pxChallengeSessionId))
            {
                var pXSessionValidationResponse = await PXChallengeManagementHandler.GetPXChallengeSession(pxChallengeSessionId, accountId, traceActivityId);
                if (!pXSessionValidationResponse["isPXChallengeSessionActive"] || !pXSessionValidationResponse["isPXChallengeSessionAccountValid"])
                {
                    // If Session is not a valid state, clear PXChallengeSessionId and reshow the challenge
                    pxChallengeSessionId = null;
                    return await this.CreatePXChallengeResponse(pi, language, partner, pxChallengeSessionId, traceActivityId, null, accountId);
                }
            }

            List<KeyValuePair<string, string>> headers = null;

            // Remove challenge evidence property from payload
            pi.TryRemoveProperty($"{Constants.PaymentInstrument.Details}.{Constants.DataDescriptionPropertyNames.ChallengeEvidence}");

            ////Validate Challenge Status
            // captchaSolution is null or empty indicates it is not legacy HIP captcha flow
            if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXChallengeSwitch, StringComparer.OrdinalIgnoreCase)
                && IsCreditCard(paymentMethodFamily, paymentMethodType))
            {
                isPXChallengeRequired = GetShowChallengeParameterFromPI(pi);
                if (!string.IsNullOrEmpty(pxChallengeSessionId) && isPXChallengeRequired)
                {
                    try
                    {
                        var challengeCompleted = await this.PXChallengeManagementHandler.GetPXChallengeStatus(pxChallengeSessionId, traceActivityId);
                        if (challengeCompleted)
                        {
                            // If Challenge is completed, add evidence to the payload and continue with the request
                            pi = AddChallengeEvidenceToPidlData(pi, pxChallengeSessionId);
                        }
                        else
                        {
                            // If Challenge is not completed reshow the challenge
                            return await this.CreatePXChallengeResponse(pi, language, partner, pxChallengeSessionId, traceActivityId);
                        }
                    }
                    catch (Exception pXChallengeStatusException)
                    {
                        if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXChallengeValidationFailureHandling, StringComparer.OrdinalIgnoreCase))
                        {
                            SllWebLogger.TracePXServiceException("PaymentInstrumentsExController.AddNewPI: " + pXChallengeStatusException.ToString(), traceActivityId);
                            pi = AddChallengeEvidenceToPidlData(pi, pxChallengeSessionId);
                        }
                        else
                        {
                            // reshow the challenge
                            return await this.CreatePXChallengeResponse(pi, language, partner, pxChallengeSessionId, traceActivityId);
                        }
                    }
                }
            }

            // Use Partner Settings if enabled for the partner
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(Constants.Operations.Add);

            // Enable flighting based on setting partner
            this.EnableFlightingsInPartnerSetting(setting, country);

            if (IsCreditCard(paymentMethodFamily, paymentMethodType) && this.ExposedFlightFeatures.Contains(Flighting.Features.PXRateLimitPerAccount))
            {
                var serviceErrorResp = GenerateValidationFailedServiceErrorResponse(language);
                this.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.Message] = $"{GlobalConstants.AbnormalDetection.LogMsgWhenCaughtByPX} " +
                    $"by flight {Flighting.Features.PXRateLimitPerAccount} limited by accountId {accountId}, family {paymentMethodFamily}, type {paymentMethodType}.";
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, serviceErrorResp, GlobalConstants.HeaderValues.JsonContent);
            }

            // Handle Validate and Redeem csv token
            if (IsStoredValue(paymentMethodFamily, paymentMethodType)
                && (PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.RedeemGiftCard, country, setting)
                || this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableRedeemCSVFlow)))
            {
                FeatureContext featureContext = new FeatureContext(
                            country,
                            GetSettingTemplate(partner, setting, Constants.DescriptionTypes.PaymentMethodDescription),
                            Constants.DescriptionTypes.PaymentMethodDescription,
                            Constants.Operations.Add,
                            null,
                            language,
                            null,
                            this.ExposedFlightFeatures,
                            setting?.Features,
                            paymentMethodFamily,
                            paymentMethodType,
                            originalPartner: partner,
                            isGuestAccount: GuestAccountHelper.IsGuestAccount(this.Request));

                return await this.HandleStoredValuePI(pi, accountId, partner, country, language, featureContext, traceActivityId, this.ExposedFlightFeatures, setting);
            }

            if (string.Equals(partner, Constants.PartnerName.Xbox, StringComparison.InvariantCultureIgnoreCase)
                       || string.Equals(partner, Constants.PartnerName.AmcXbox, StringComparison.InvariantCultureIgnoreCase)
                       || PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner))
            {
                await this.AddRiskDataPayloadToPI(pi);
            }

            // Include x-ms-flight header with value PXAlipayQRCode for PIMS to trigger Alipay QR code flow
            if (headers == null)
            {
                headers = new List<KeyValuePair<string, string>>();
            }

            headers.Add(new KeyValuePair<string, string>(GlobalConstants.HeaderValues.ExtendedFlightName, Constants.FlightValues.PXAlipayQRCode));

            // Add ip address to pi payload and pass it to PIMS
            if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXPassIpAddressToPIMSForAddUpdatePI, StringComparer.OrdinalIgnoreCase))
            {
                await this.AddParameterToPIRiskData(pi, Constants.DeviceInfoProperty.IPAddress, GlobalConstants.ClientContextKeys.DeviceInfo.IPAddress, partner, paymentMethodType, operation: Constants.Operations.Add, country);
            }

            if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXPassUserAgentToPIMSForAddUpdatePI, StringComparer.OrdinalIgnoreCase))
            {
                await this.AddParameterToPIRiskData(pi, Constants.DeviceInfoProperty.UserAgent, GlobalConstants.ClientContextKeys.DeviceInfo.UserAgent, partner, paymentMethodType, operation: Constants.Operations.Add, country);
            }

            // flight cleanup task - 56373987
            if (IsSepa(paymentMethodFamily, paymentMethodType) && this.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.EnableSepaJpmc, StringComparer.OrdinalIgnoreCase))
            {
                headers.Add(new KeyValuePair<string, string>(GlobalConstants.HeaderValues.ExtendedFlightName, Constants.PartnerFlightValues.EnableSepaJpmc));
                await this.AddParameterToPIRiskData(pi, Constants.DeviceInfoProperty.UserAgent, GlobalConstants.ClientContextKeys.DeviceInfo.UserAgent, partner, paymentMethodType, operation: Constants.Operations.Add, country);
            }

            // Add flight header for Updating Challenge Error Code and pass it to PIMS
            if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXChallengeSwitch, StringComparer.OrdinalIgnoreCase))
            {
                headers.Add(new KeyValuePair<string, string>(GlobalConstants.HeaderValues.ExtendedFlightName, Constants.FlightValues.UpdateCaptchaErrorMessage + "," + Constants.FlightValues.HonorNewRiskCode));
            }

            // For Azure add credit card or virtual legacy invoice,
            // if legacy billable account does not have an address within account,
            // use PI's address to update account address
            if ((PartnerHelper.IsAzurePartner(partner) || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UpdatePIaddressToAccount, country, setting))
                && (string.Equals(paymentMethodFamily, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase)
                    || PIHelper.IsVirtualLegacyInvoice(paymentMethodFamily, paymentMethodType)))
            {
                if (!string.IsNullOrWhiteSpace(billableAccountId))
                {
                    try
                    {
                        string altSecId = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.AadInfo.AltSecId);
                        string orgPuid = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.AadInfo.OrgPuid);
                        string ipAddress = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.DeviceInfo.IPAddress);
                        LegacyAccountHelper.UpdateLegacyBillableAccountAddress(this.Settings, billableAccountId, pi, traceActivityId, altSecId, orgPuid, ipAddress, language);
                    }
                    catch (ServiceErrorResponseException e)
                    {
                        return this.Request.CreateResponse(e.Error.HttpStatusCode, e.Error, "application/json");
                    }
                }
            }

            // resolve to original partner flow with retain of user input if zipcode resolution fails, otherwise update PI with address and then let existing logic process it
            // TODO: As part of T-54279751 investigation, the AddressNoCityState flow found to be not used by any partner
            // And below code & csv config related to it can be removed
            if (IsAddressNoCityStatePI(pi) && IsCreditCard(paymentMethodFamily, paymentMethodType, country)
                && string.Equals(country, Constants.CountryCodes.UnitedStates, StringComparison.OrdinalIgnoreCase)
                && string.Equals(scenario, Constants.ScenarioNames.AddressNoCityState, StringComparison.OrdinalIgnoreCase))
            {
                string zipcode = pi.TryGetPropertyValue("details.address.postal_code");
                List<string> displayedPaymentMethodTypes = pi.ContainsKey("displayedPaymentMethodTypes") ? JsonConvert.DeserializeObject<List<string>>((string)pi["displayedPaymentMethodTypes"]) : new List<string>();

                PIDLResource fallbackPidlResource = null;
                bool includeZipErrorCode = false;
                List<Tuple<string, string>> addressCityState = null;
                var addressLineOne = pi.TryGetPropertyValue("details.address.address_line1");

                if (country != null && zipcode != null && addressLineOne != null)
                {
                    var avsAccessor = this.Settings.AddressEnrichmentServiceAccessor;
                    addressCityState = await avsAccessor.GetCityStateMapping(country, zipcode, traceActivityId);
                }

                if (addressCityState == null || addressCityState.Count != 1 || !TryUpdateAddress(pi, addressCityState[0].Item1.ToLower(), addressCityState[0].Item2.ToLower()))
                {
                    if (addressCityState == null || addressCityState.Count == 0)
                    {
                        includeZipErrorCode = true;
                    }

                    fallbackPidlResource = ConstructAddressEnrichmentFailurePidl(displayedPaymentMethodTypes, traceActivityId, paymentMethodFamily, paymentMethodType, language, country, partner, this.ExposedFlightFeatures, includeZipErrorCode: includeZipErrorCode, completePrerequisites: completePrerequisites);
                    return this.Request.CreateResponse(fallbackPidlResource);
                }

                // reaching this point means AddressEnrichmentService has successfully returned a single city/state combination
                if (completePrerequisites)
                {
                    // TODO: As part of T-54279751 investigation, the AddressNoCityState flow found to be not used by any partner
                    // And below code & csv config related to it can be removed
                    this.TryAddressNoCityStateCompletePrerequisites(pi, accountId, country, addressLineOne, zipcode, addressCityState[0].Item1, addressCityState[0].Item2, traceActivityId);
                }
            }

            // Compliance - CELA - Use AVS Suggested address with 9-digit
            if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXAddressZipCodeUpdateTo9Digit, StringComparer.OrdinalIgnoreCase))
            {
                await this.TryUpdateAddressWith9digitZipCode(pi);
            }

            var queryparams = this.Request.Query.AsEnumerable().Select(q => new KeyValuePair<string, string>(q.Key, q.Value));
            if (!string.IsNullOrEmpty(billableAccountId))
            {
                if (!this.Request.Query.TryGetValue(GlobalConstants.QueryParamNames.BillableAccountId, out _))
                {
                    queryparams = queryparams.Concat(new[] { new KeyValuePair<string, string>(GlobalConstants.QueryParamNames.BillableAccountId, billableAccountId) });
                }
            }

            StringBuilder adMessage = new StringBuilder();
            Dictionary<string, string> adData = new Dictionary<string, string>();
            if (IsCreditCard(paymentMethodFamily, paymentMethodType)
            && !Constants.PXRateLimitAddCCSkipAccounts.Contains(accountId, StringComparer.OrdinalIgnoreCase))
            {
                bool isAnomaly = false;
                string ipAddress = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.DeviceInfo.IPAddress);
                if (!string.IsNullOrWhiteSpace(ipAddress))
                {
                    adData[PXCommon.Constants.Dimensions.IPAddress] = ipAddress;
                }

                adData[PXCommon.Constants.Dimensions.AccountId] = accountId;

                adMessage.Append("Calling IsCardTesting. ");
                adMessage.Append(string.Join(", ", adData.ToList())).Append(". ");
                this.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.Message] = adMessage.ToString();

                isAnomaly = this.IsMaliciousAccountId(accountId, traceActivityId, adMessage) || this.IsMaliciousClientIP(ipAddress, traceActivityId, adMessage);

                var exceedingDimensions = AnomalyDetection.IsCardTesting(adData, this.ExposedFlightFeatures);

                if (exceedingDimensions == null)
                {
                    adMessage.Append("Returned null. ");
                    this.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.Message] = adMessage.ToString();
                }
                else
                {
                    adMessage.Append("Returned dimensions: ");
                    adMessage.Append(string.Join(", ", exceedingDimensions)).Append(". ");
                    adMessage.Append($"{GlobalConstants.AbnormalDetection.LogMsgWhenCaughtByPX}");
                    this.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.Message] = adMessage.ToString();

                    AnomalyDetection.AddData(adData, true);
                    isAnomaly = true;
                }

                if (isAnomaly)
                {
                    var ser = GenerateValidationFailedServiceErrorResponse(language);
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, ser, "application/json");
                }
            }

            if (IsCreditCard(paymentMethodFamily, paymentMethodType)
                && !Constants.PXRateLimitAddCCSkipAccounts.Contains(accountId, StringComparer.OrdinalIgnoreCase)
                && this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableJarvisHMACValidation))
            {
                string jarvisAccountIdHmac = pi.TryGetPropertyValue(Constants.JarvisAccountIdHmacProperty);

                if (string.Equals(ObfuscationHelper.GetHashValue(accountId, ObfuscationHelper.JarvisAccountIdHashSalt), jarvisAccountIdHmac, StringComparison.OrdinalIgnoreCase))
                {
                    adMessage.Append("JarvisAccountIdHmac matched. ");
                    this.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.Message] = adMessage.ToString();
                }
                else
                {
                    adMessage.Append("JarvisAccountIdHmac did not match. ");
                    this.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.Message] = adMessage.ToString();

                    // Short circuit the call if JarvisAccountIdHmac is not as expected
                    var ser = GenerateValidationFailedServiceErrorResponse(language);
                    adMessage.Append($"{GlobalConstants.AbnormalDetection.LogMsgWhenCaughtByPX}");
                    adMessage.Append("Calling AddData for bad request.");
                    this.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.Message] = adMessage.ToString();
                    AnomalyDetection.AddData(adData, true);
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, ser, "application/json");
                }

                pi.TryRemoveProperty(Constants.JarvisAccountIdHmacProperty);
            }

            PaymentInstrument newPI = null;
            ServiceErrorResponseException ex = null;
            Model.PaymentOrchestratorService.AttachPaymentInstrumentResponse attachPIResponse = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(requestId))
                {
                    if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXIntegrateFraudDetectionService, StringComparer.OrdinalIgnoreCase))
                    {
                        EvaluationResult evaluationResult;
                        try
                        {
                            evaluationResult = await this.Settings.FraudDetectionServiceAccessor.BotDetection(requestId, traceActivityId);
                        }
                        catch (Exception)
                        {
                            // In case of bot check failure, we will continue with the request and return Approved recommendation
                            evaluationResult = new EvaluationResult { Recommendation = Constants.FraudDetectionServiceConstants.ApprovedRecommendation };
                        }
                    }

                    newPI = await this.Settings.PIMSAccessor.PostPaymentInstrument(pi, traceActivityId, queryparams, headers, partner, this.ExposedFlightFeatures);
                    var savePaymentDetails = pi.TryGetPropertyValue($"{Constants.DataDescriptionPropertyNames.SavePaymentDetails}");

                    if (this.UsePaymentRequestApiEnabled())
                    {
                        attachPIResponse = await this.Settings.PaymentOrchestratorServiceAccessor.AttachPaymentInstrumentToPaymentRequest(requestId, newPI.PaymentInstrumentId, pi.TryGetPropertyValue(CvvTokenPath), traceActivityId, savePaymentDetails);
                    }
                    else
                    {
                        attachPIResponse = await this.Settings.PaymentOrchestratorServiceAccessor.AttachPaymentInstrument(requestId, newPI.PaymentInstrumentId, pi.TryGetPropertyValue(CvvTokenPath), traceActivityId, savePaymentDetails);
                    }

                    if (requestId.ToLower().StartsWith("cr_")
                        || (this.UsePaymentRequestApiEnabled()
                            && PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.PaymentClientHandlePaymentCollection, country, setting)))
                    {
                        newPI.ClientAction = CheckoutRequestsExHandler.CreateClientActionForPostPI(newPI.PaymentInstrumentId);
                        if (this.UsePaymentRequestApiEnabled())
                        {
                            await this.Settings.PaymentOrchestratorServiceAccessor.AttachAddressToPaymentRequest(CheckoutRequestsExHandler.ConvertPIAddressToPOAddress(pi), Constants.AddressTypes.Billing, traceActivityId, requestId);
                        }
                        else
                        {
                            await this.Settings.PaymentOrchestratorServiceAccessor.AttachAddress(CheckoutRequestsExHandler.ConvertPIAddressToPOAddress(pi), Constants.AddressTypes.Billing, traceActivityId, requestId);
                        }
                    }
                }
                else
                {
                    newPI = await this.Settings.PIMSAccessor.PostPaymentInstrument(accountId, pi, traceActivityId, queryparams, headers, partner, this.ExposedFlightFeatures);
                }

                await this.RaisePIAttachOnOfferEvent(
                    traceActivityId: traceActivityId,
                    newPI: newPI,
                    partner: partner,
                    country: country,
                    offerId: this.OfferId,
                    exposedFlightFeatures: this.ExposedFlightFeatures);
            }
            catch (ServiceErrorResponseException exception)
            {
                ex = exception;
            }

            if (ex != null)
            {
                if (IsCreditCard(paymentMethodFamily, paymentMethodType)
                    && !Constants.PXRateLimitAddCCSkipAccounts.Contains(accountId, StringComparer.OrdinalIgnoreCase))
                {
                    if ((int)ex.Response.StatusCode >= 400 && (int)ex.Response.StatusCode <= 499)
                    {
                        adMessage.Append("Calling AddData for bad request.");
                        this.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.Message] = adMessage.ToString();

                        AnomalyDetection.AddData(adData, true);
                    }
                }

                if (GuestAccountHelper.IsGuestAccount(this.Request)
                    && string.Equals(ex.Error.ErrorCode, Constants.CreditCardErrorCodes.ChallengeRequired, StringComparison.OrdinalIgnoreCase))
                {
                    // For guest user, pims returns ChallengeRequired error incase of bot attack/GC flight is turned off.
                    // We do not want to expose this information to the UX, so we are returning a generic error message.
                    var serviceErrorResponse = new ServiceErrorResponse(
                        errorCode: ex.Error.ErrorCode,
                        message: LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.Generic, language),
                        source: GlobalConstants.ServiceName);
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, serviceErrorResponse, GlobalConstants.HeaderValues.JsonContent);
                }

                // TODO Task 1614907:[PX CAD] Refactor ClientActionFactory into its own assembly
                if (IsChinaUnionPay(paymentMethodFamily, paymentMethodType))
                {
                    // TODO Once deployment of the new error mapping (where ValidationFailed means terminating) is complete in legacy, ValidationFailed
                    // clause should be removed from the condition below.
                    if (string.Equals(ex.Error.ErrorCode, Constants.CupErrorCodes.ValidationFailed, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(ex.Error.ErrorCode, Constants.CupErrorCodes.InvalidPhoneValue, StringComparison.OrdinalIgnoreCase))
                    {
                        ex.Error.Message = Constants.ClientActionContract.NoMessage;
                        ex.Error.AddDetail(new ServiceErrorDetail()
                        {
                            ErrorCode = ex.Error.ErrorCode,
                            Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CupErrorMessages.InvalidPhoneOrCard, language),
                            Target = string.Format("{0},{1}", Constants.CupErrorTargets.CardNumber, Constants.CupErrorTargets.PhoneNumber),
                        });
                    }
                    else if (string.Equals(ex.Error.ErrorCode, Constants.CupErrorCodes.InvalidPaymentInstrumentInfo, StringComparison.OrdinalIgnoreCase))
                    {
                        ex.Error.Message = Constants.ClientActionContract.NoMessage;
                        ex.Error.AddDetail(new ServiceErrorDetail()
                        {
                            ErrorCode = ex.Error.ErrorCode,
                            Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CupErrorMessages.InvalidCardNumber, language),
                            Target = Constants.CupErrorTargets.CardNumber
                        });
                    }
                    else if (string.Equals(ex.Error.ErrorCode, Constants.CupErrorCodes.TooManyOperations, StringComparison.OrdinalIgnoreCase))
                    {
                        ex.Error.Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CupErrorMessages.TooManySmsRequests, language);
                    }
                    else
                    {
                        // Catch all for any other error scenario
                        ex.Error.Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CupErrorMessages.Generic, language);
                    }
                }
                else if (PIHelper.IsPayPal(paymentMethodFamily, paymentMethodType))
                {
                    if (string.Equals(ex.Error.ErrorCode, Constants.PayPalErrorCodes.IncorrectCredential, StringComparison.OrdinalIgnoreCase))
                    {
                        ex.Error.Message = Constants.ClientActionContract.NoMessage;
                        ex.Error.AddDetail(new ServiceErrorDetail()
                        {
                            ErrorCode = ex.Error.ErrorCode,
                            Message = LocalizationRepository.Instance.GetLocalizedString(Constants.PayPalErrorMessages.IncorrectCredential, language),
                            Target = string.Format("{0},{1}", Constants.PayPalErrorTargets.Email, Constants.PayPalErrorTargets.Password),
                        });
                    }
                    else
                    {
                        // Catch all for any other error scenario
                        ex.Error.Message = LocalizationRepository.Instance.GetLocalizedString(Constants.PayPalErrorMessages.Generic, language);
                    }
                }
                else if ((this.ExposedFlightFeatures.Contains(Flighting.Features.PXCheckCreditCardTypes) ? IsCreditCard(paymentMethodFamily, paymentMethodType, country) : string.Equals(paymentMethodFamily, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase))
                        && !string.Equals(partner, Constants.PartnerName.Wallet, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Checking if PX Challenge switch is on and error Code ChallegeRequired received from PIMS
                    if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXChallengeSwitch) && string.Equals(ex.Error.ErrorCode, Constants.CreditCardErrorCodes.ChallengeRequired, StringComparison.OrdinalIgnoreCase))
                    {
                        SllWebLogger.TraceServerMessage("AddNewPI", traceActivityId.CorrelationVectorV4.Value, traceActivityId.ActivityId.ToString(), $"Challenge triggered by PIMS Signal for PXChallengeSessionId:{pxChallengeSessionId}", EventLevel.Informational);
                        return await this.CreatePXChallengeResponse(pi, language, partner, pxChallengeSessionId, traceActivityId, null, accountId);
                    }

                    // For add credit card, it has two extra errors - InvalidPaymentInstrumentInfo(credit card number error) and PrepaidCardNotSupported
                    if (string.Equals(ex.Error.ErrorCode, Constants.CreditCardErrorCodes.InvalidPaymentInstrumentInfo, StringComparison.OrdinalIgnoreCase))
                    {
                        ex.Error.Message = Constants.ClientActionContract.NoMessage;
                        ex.Error.AddDetail(new ServiceErrorDetail()
                        {
                            ErrorCode = ex.Error.ErrorCode,
                            Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.InvalidCardNumber, language),
                            Target = Constants.CreditCardErrorTargets.CardNumber,
                        });
                    }
                    else if (string.Equals(ex.Error.ErrorCode, Constants.CreditCardErrorCodes.PrepaidCardNotSupported, StringComparison.OrdinalIgnoreCase))
                    {
                        ex.Error.Message = Constants.ClientActionContract.NoMessage;
                        ex.Error.AddDetail(new ServiceErrorDetail()
                        {
                            ErrorCode = ex.Error.ErrorCode,
                            Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.PrepaidCardNotSupported, language),
                            Target = Constants.CreditCardErrorTargets.CardNumber,
                        });
                    }
                    else if (string.Equals(ex.Error.ErrorCode, Constants.CreditCardErrorCodes.InvalidIssuerResponseWithTRPAU0009, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(ex.Error.ErrorCode, Constants.CreditCardErrorCodes.InvalidIssuerResponseWithTRPAU0008, StringComparison.OrdinalIgnoreCase))
                    {
                        ex.Error.Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.InvalidIssuerResponse, language);
                        if (this.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.PXDisplay3dsNotEnabledErrorInline))
                        {
                            ex.Error.AddDetail(new ServiceErrorDetail()
                            {
                                ErrorCode = ex.Error.ErrorCode,
                                Message = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.InvalidIssuerResponse, language),
                                Target = this.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.PXDisplay3dsNotEnabledErrorInline) ? Constants.CreditCardErrorTargets.CardNumber : null,
                            });
                        }
                    }
                    else if (string.Equals(ex.Error.ErrorCode, Constants.CreditCardErrorCodes.InvalidAddress, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(country, Constants.CountryCodes.UnitedStates, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(scenario, Constants.ScenarioNames.AddressNoCityState, StringComparison.OrdinalIgnoreCase)
                        && (string.Equals(partner, Constants.PartnerName.OXOWebDirect, StringComparison.OrdinalIgnoreCase)
                            || string.Equals(partner, Constants.PartnerName.Webblends, StringComparison.OrdinalIgnoreCase)
                            || string.Equals(partner, Constants.PartnerName.OfficeOobe, StringComparison.OrdinalIgnoreCase)
                            || string.Equals(partner, Constants.PartnerName.SmbOobe, StringComparison.OrdinalIgnoreCase)))
                    {
                        // TODO: As part of T-54279751 investigation, the AddressNoCityState flow found to be not used by any partner
                        // And code & csv config related to it can be removed
                        // Not checking for IsAddressNoCityStatePI(pi) above since PI was already altered based on the response from AVS address enrichment service
                        List<string> displayedPaymentMethodTypes = pi.ContainsKey("displayedPaymentMethodTypes") ? JsonConvert.DeserializeObject<List<string>>((string)pi["displayedPaymentMethodTypes"]) : new List<string>();
                        var fallbackPidlResource = ConstructAddressEnrichmentFailurePidl(displayedPaymentMethodTypes, traceActivityId, paymentMethodFamily, paymentMethodType, language, country, partner, this.ExposedFlightFeatures, includeZipErrorCode: true, completePrerequisites: completePrerequisites);
                        return this.Request.CreateResponse(fallbackPidlResource);
                    }
                    else
                    {
                        MapCreditCardCommonError(ref ex, language);
                    }

                    //// If EnableConditionalFieldsForBillingAddress feature is enabled, an UpdatePropertyValue clientAction is attached to the error to show address group if there is an non terminal error returned from service.
                    //// For terminal errors, the error will still be returned to client in a failure event even with UpdatePropertyValue client action 
                    if (PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.EnableConditionalFieldsForBillingAddress, country, setting))
                    {
                        ex.Error.ClientAction = new ClientAction(
                                    ClientActionType.UpdatePropertyValue,
                                    new ActionContext(Constants.ConditionalFieldsDescriptionName.HideAddressGroup, false));
                    }
                }
                else if (IsAch(paymentMethodFamily, paymentMethodType) || IsSepa(paymentMethodFamily, paymentMethodType))
                {
                    if (IsSepa(paymentMethodFamily, paymentMethodType) && string.Equals(ex.Error.ErrorCode, Constants.DirectDebitErrorCodes.OperationNotSupported, StringComparison.OrdinalIgnoreCase))
                    {
                        ex.Response.StatusCode = HttpStatusCode.InternalServerError;
                    }
                    else if (string.Equals(ex.Error.ErrorCode, Constants.DirectDebitErrorCodes.InvalidBankCode, StringComparison.OrdinalIgnoreCase))
                    {
                        ex.Error.Message = Constants.ClientActionContract.NoMessage;
                        if (IsAch(paymentMethodFamily, paymentMethodType))
                        {
                            ex.Error.AddDetail(new ServiceErrorDetail()
                            {
                                ErrorCode = ex.Error.ErrorCode,
                                Message = LocalizationRepository.Instance.GetLocalizedString(Constants.DirectDebitErrorMessages.InvalidBankCodeAch, language),
                                Target = Constants.DirectDebitErrorTargets.BankCode,
                            });
                        }
                        else if (IsSepa(paymentMethodFamily, paymentMethodType))
                        {
                            ex.Error.AddDetail(new ServiceErrorDetail()
                            {
                                ErrorCode = ex.Error.ErrorCode,
                                Message = LocalizationRepository.Instance.GetLocalizedString(Constants.DirectDebitErrorMessages.InvalidBankCodeSepa, language),
                                Target = Constants.DirectDebitErrorTargets.BankCode,
                            });
                        }
                    }
                    else
                    {
                        MapDirectDebitCommonError(ref ex, paymentMethodFamily, paymentMethodType, language);
                    }
                }
                else if (IsAliPay(paymentMethodFamily, paymentMethodType))
                {
                    MapAlipayAddError(ref ex, language);
                }
                else if (IsNonSim(paymentMethodFamily))
                {
                    MapNonSimAddError(ref ex, language, partner);
                }
                else if (IsKlarna(paymentMethodFamily, paymentMethodType))
                {
                    MapKlarnaAddError(ref ex, language);
                }
                else if (PIHelper.IsVirtualLegacyInvoice(paymentMethodFamily, paymentMethodType))
                {
                    MapLegacyInvoiceAddError(ref ex, language);
                }
                else if (PIHelper.IsUpi(paymentMethodFamily, paymentMethodType))
                {
                    MapUpiAddError(ref ex, language);
                }
                else if (PIHelper.IsUpiCommercial(paymentMethodFamily, paymentMethodType))
                {
                    MapUpiAddError(ref ex, language);
                }

                return this.Request.CreateResponse(ex.Response.StatusCode, ex.Error, "application/json");
            }

            // Adding anomaly detection message on the request
            this.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.Message] = adMessage.ToString();

            if (IsCreditCard(paymentMethodFamily, paymentMethodType)
                && !Constants.PXRateLimitAddCCSkipAccounts.Contains(accountId, StringComparer.OrdinalIgnoreCase))
            {
                adMessage.Append("Calling AddData for good request.");
                this.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.Message] = adMessage.ToString();

                AnomalyDetection.AddData(adData, false);
            }

            if (string.IsNullOrEmpty(paymentMethodType))
            {
                this.Request.AddTracingProperties(accountId, null, paymentMethodFamily, newPI.PaymentMethod.PaymentMethodType, country);
            }

            // User emailAddress on resumePI page, pass it to AddClientActionToPaymentInstrument as a nullable parameter.
            string emailAddress = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.EmailAddress);
            if (string.IsNullOrEmpty(emailAddress))
            {
                emailAddress = LocalizationRepository.Instance.GetLocalizedString("Email address", language);
            }

            string sessionId = null;
            if (pi.ContainsKey("sessionId"))
            {
                sessionId = pi["sessionId"].ToString();
            }

            // 3DS1 needs to get whether or not to show redirect url in iframe or new page from session
            if (string.Equals(country, Constants.CountryCodes.India, StringComparison.InvariantCultureIgnoreCase) && newPI.IsCreditCard() && !string.IsNullOrEmpty(newPI.PaymentInstrumentDetails.SessionQueryUrl))
            {
                newPI.PaymentInstrumentDetails.IsFullPageRedirect = await this.IsFullPageRedirect(newPI, traceActivityId, partner);
            }

            string shortUrl = null;
            string contextDescriptionType = null;
            if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnablePaypalRedirectUrlText, StringComparer.OrdinalIgnoreCase)
                || Constants.PartnersToEnablePaypalRedirectOnTryAgain.Contains(partner, StringComparer.OrdinalIgnoreCase)
                || ((Constants.CountriesToEnablePaypalSecondScreenForXbox.Contains(country, StringComparer.OrdinalIgnoreCase)
                || this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnablePaypalSecondScreenForXbox, StringComparer.OrdinalIgnoreCase))
                && Constants.PartnersToEnablePaypalSecondScreenForXbox.Contains(partner, StringComparer.OrdinalIgnoreCase))
                || PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner)
                || string.Equals(partner, Constants.PartnerName.OXOWebDirect, StringComparison.OrdinalIgnoreCase)
                || string.Equals(partner, Constants.PartnerName.OXODIME, StringComparison.OrdinalIgnoreCase)
                || string.Equals(partner, Constants.PartnerName.OXOOobe, StringComparison.OrdinalIgnoreCase)
                || TemplateHelper.IsTemplateBasedPIDLIncludingDefaultTemplate(TemplateHelper.GetSettingTemplate(partner, setting, Constants.DescriptionTypes.PaymentMethodDescription, $"{paymentMethodFamily}.{paymentMethodType}")))
            {
                shortUrl = await this.CreateShortUrlForPaymentInstrumentAndFlight(newPI, language, traceActivityId, sessionId, country, setting);
                ClientActionFactory.AddClientActionToPaymentInstrument(newPI, accountId, language, partner, classicProduct, billableAccountId, traceActivityId, this.PidlBaseUrl, Constants.RequestType.AddPI, completePrerequisites, country, emailAddress, scenario, exposedFlightFeatures: this.ExposedFlightFeatures, sessionId: sessionId, shortUrl: shortUrl?.ToString(), setting: setting);
                contextDescriptionType = ClientActionFactory.GetPIDLGenerationContextDescriptionType(newPI, Constants.RequestType.AddPI, country, setting);
            }
            else
            {
                ClientActionFactory.AddClientActionToPaymentInstrument(newPI, accountId, language, partner, classicProduct, billableAccountId, traceActivityId, this.PidlBaseUrl, Constants.RequestType.AddPI, completePrerequisites, country, emailAddress, scenario, exposedFlightFeatures: this.ExposedFlightFeatures, setting: setting);
            }

            await this.HandleProfileAddress(accountId, newPI, completePrerequisites, language, partner, country, HttpMethod.Post, traceActivityId, this.ExposedFlightFeatures, setting);

            if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXChallengeSwitch, StringComparer.OrdinalIgnoreCase) && !string.IsNullOrEmpty(pxChallengeSessionId))
            {
                await this.PXChallengeManagementHandler.UpdatePXSessionCompletedStatus(pxChallengeSessionId, traceActivityId);
            }

            if (newPI != null && newPI.ClientAction != null)
            {
                var pidls = newPI.ClientAction.Context as List<PIDLResource>;
                if (pidls != null)
                {
                    FeatureContext featureContext = new FeatureContext(
                        country,
                        partner,
                        Constants.DescriptionTypes.PaymentInstrumentDescription,
                        Constants.Operations.Add,
                        scenario,
                        language,
                        null,
                        exposedFlightFeatures: this.ExposedFlightFeatures,
                        setting?.Features,
                        paymentMethodFamily,
                        paymentMethodType,
                        smdMarkets: null,
                        originalPartner: partner,
                        isGuestAccount: GuestAccountHelper.IsGuestAccount(this.Request),
                        shortUrl: shortUrl,
                        contextDescriptionType: contextDescriptionType);

                    PostProcessor.Process(pidls, PIDLResourceFactory.FeatureFactory, featureContext);
                }
            }

            if (string.IsNullOrWhiteSpace(requestId)
                || (!string.IsNullOrEmpty(requestId) && requestId.ToLower().StartsWith("cr_"))
                || (this.UsePaymentRequestApiEnabled()
                    && PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.PaymentClientHandlePaymentCollection, country, setting)))
            {
                return this.Request.CreateResponse(newPI);
            }
            else
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, attachPIResponse);
            }
        }

        private async Task<PaymentInstrument> GetXboxCoBrandedCardPidlResponse(string accountId, string language, EventTraceActivity traceActivityId, List<Model.IssuerService.Application> applicationDetailsList, string partner, string country, PaymentExperienceSetting setting = null)
        {
            PaymentInstrument ret = new PaymentInstrument();
            string staticPidlId;
            Model.IssuerService.Application applicationDetails = applicationDetailsList.First();

            if (applicationDetails == null)
            {
                staticPidlId = Constants.StaticDescriptionTypes.XboxCardApplicationErrorPidl;

                ret.Status = PaymentInstrumentStatus.Unknown;
            }
            else if (applicationDetails.Status == Constants.XboxCardEligibilityStatus.Approved ||
                applicationDetails.Status == Constants.XboxCardEligibilityStatus.CardAlreadyIssued)
            {
                string paymentInstrumentId = applicationDetails.PaymentInstrumentId;

                try
                {
                    ret = await this.Settings.PIMSAccessor.GetPaymentInstrument(accountId, paymentInstrumentId, traceActivityId);
                    staticPidlId = Constants.StaticDescriptionTypes.XboxCardSuccessPidl;
                }
                catch
                {
                    staticPidlId = Constants.StaticDescriptionTypes.XboxCardApplicationErrorPidl;
                }
            }
            else if (
                applicationDetails.Status == Constants.XboxCardEligibilityStatus.PendingOnIssuer ||
                applicationDetails.Status == Constants.XboxCardEligibilityStatus.PendingOnApplication ||
                applicationDetails.Status == Constants.XboxCardEligibilityStatus.Duplicate)
            {
                // Pending screen
                staticPidlId = Constants.StaticDescriptionTypes.XboxCardApplicationPendingPidl;
                ret.Status = PaymentInstrumentStatus.Pending;
            }
            else if (applicationDetails.Status == Constants.XboxCardEligibilityStatus.Cancelled)
            {
                // Should not have pidl, should trigger gohome instead
                ret.Status = PaymentInstrumentStatus.Cancelled;
                return ret;
            }
            else
            {
                staticPidlId = Constants.StaticDescriptionTypes.XboxCardApplicationErrorPidl;
                ret.Status = PaymentInstrumentStatus.Unknown;
            }

            List<PIDLResource> pidl = PIDLResourceFactory.Instance.GetStaticPidlDescriptions(staticPidlId, language, PXCommon.Constants.PartnerNames.XboxNative, flightNames: this.ExposedFlightFeatures);

            FeatureContext featureContext = new FeatureContext(
                country,
                GetSettingTemplate(partner, setting, Constants.DescriptionTypes.PaymentInstrumentDescription),
                Constants.DescriptionTypes.PaymentInstrumentDescription,
                Constants.Operations.Apply,
                null,
                language,
                null,
                this.ExposedFlightFeatures,
                setting?.Features,
                null,
                null,
                originalPartner: partner,
                isGuestAccount: GuestAccountHelper.IsGuestAccount(this.Request));

            PostProcessor.Process(pidl, PIDLResourceFactory.FeatureFactory, featureContext);

            ClientAction clientAction = new ClientAction(ClientActionType.Pidl, pidl);
            ret.ClientAction = clientAction;

            TranslateUnicodes(pidl);

            return ret;
        }

        private bool IsMaliciousAccountId(string accountId, EventTraceActivity traceActivityId, StringBuilder adMessage)
        {
            if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableMaliciousAccountIdRejection)
                && this.Settings.AnomalyDetectionAccessor.IsMaliciousAccountId(accountId, traceActivityId))
            {
                adMessage.Append("Blocked based on anomaly detection result on accountId.");
                adMessage.Append($"{GlobalConstants.AbnormalDetection.LogMsgWhenCaughtByPX}");
                this.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.Message] = adMessage.ToString();
                //// Task 41895852: Blocking is put behind another flight to remove the need for another deployment to turn on blocking based on malicious account ID
                return this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableMaliciousAccountIdRejectionEffect);
            }

            return false;
        }

        private bool IsMaliciousClientIP(string ipAddress, EventTraceActivity traceActivityId, StringBuilder adMessage)
        {
            if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableMaliciousClientIPRejection)
                && this.Settings.AnomalyDetectionAccessor.IsMaliciousClientIP(ipAddress, traceActivityId))
            {
                adMessage.Append("Blocked based on anomaly detection result on clientIP.");
                adMessage.Append($"{GlobalConstants.AbnormalDetection.LogMsgWhenCaughtByPX}");
                this.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.Message] = adMessage.ToString();
                //// Task 41895852: Blocking is put behind another flight to remove the need for another deployment to turn on blocking based on malicious client IP
                return this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableMaliciousClientIPRejectionEffect);
            }

            return false;
        }

        private async Task<bool> IsFullPageRedirect(PaymentInstrument newPI, EventTraceActivity traceActivityId, string partner)
        {
            TestContext testContext = HttpRequestHelper.GetTestHeader(this.Request);

            //// whether to use "isFullPageRedirect" in PI details to show iframe experience is under discussion. For now, iframe is the experience only for the partners like payin i.e. partners who have not done work to open in new tab or full page redir
            //// for partners who have adopted either new tab or full page redir, do not show iframe experience for now, unless iframe test header is sent
            if (HttpRequestHelper.HasThreeDSOneTestScenarioIframeOverriding(testContext) || PartnerHelper.IsThreeDSOneIframeBasedPartner(partner))
            {
                return false;
            }

            bool isFullPageRedirect = true;
            try
            {
                var rdsSessionURL = newPI.PaymentInstrumentDetails.SessionQueryUrl;

                if (!string.IsNullOrEmpty(rdsSessionURL))
                {
                    var rdsSessionId = rdsSessionURL.Split('/');
                    var session = await this.Settings.SessionServiceAccessor.GetSessionResourceData<BillDeskRedirectSession>(rdsSessionId[1], traceActivityId);

                    if (session != null && session.Data != null)
                    {
                        var data = JsonConvert.DeserializeObject<BillDeskRedirectSessionData>(session.Data);
                        isFullPageRedirect = data.IsFullPageRedirect;
                    }
                }
            }
            catch
            {
                // Full page redirect is our default scenario
                isFullPageRedirect = true;
            }

            return isFullPageRedirect;
        }

        private async Task TryUpdateAddressWith9digitZipCode(PIDLData pi)
        {
            string currentAddress = pi.TryGetPropertyValue("details.address");
            if (!string.IsNullOrWhiteSpace(currentAddress))
            {
                EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

                try
                {
                    AddressValidationAVSResponse validationResult;
                    validationResult = await this.Settings.AccountServiceAccessor.ModernValidateAddress<AddressValidationAVSResponse>(JsonConvert.DeserializeObject<AddressInfoV3>(currentAddress), traceActivityId);

                    if (validationResult.Status == AddressAVSValidationStatus.Verified
                        || validationResult.Status == AddressAVSValidationStatus.VerifiedShippable)
                    {
                        AddressInfoV3 suggestedAddress = JsonConvert.DeserializeObject<AddressInfoV3>(validationResult.SuggestedAddress.ToString());
                        pi.TrySetProperty("details.address.postal_code", suggestedAddress.PostalCode);
                        if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXAddressZipCodeUpdateTo9DigitPassVerfiedToPIMS, StringComparer.OrdinalIgnoreCase))
                        {
                            pi.TrySetProperty("details.address.verified", "true");
                        }
                    }
                    else
                    {
                        SllWebLogger.TracePXServiceException("PaymentInstrumentsExController.TryUpdateAddressWith9digitZipCode: " + validationResult.ValidationMessage, traceActivityId);
                    }
                }
                catch (Exception ex)
                {
                    SllWebLogger.TracePXServiceException("PaymentInstrumentsExController.TryUpdateAddressWith9digitZipCode: " + ex.ToString(), traceActivityId);
                }
            }
        }

        private void OverrideExpiration(PaymentInstrument[] paymentInstruments)
        {
            if (paymentInstruments != null && this.ExposedFlightFeatures.Any(o => o.StartsWith(Flighting.Features.PXNSSetExpiry)))
            {
                foreach (var pi in paymentInstruments)
                {
                    this.OverrideExpiration(pi);
                }
            }
        }

        private void OverrideExpiration(PaymentInstrument pi)
        {
            if (pi != null && this.ExposedFlightFeatures.Any(o => o.StartsWith(Flighting.Features.PXNSSetExpiry)))
            {
                List<string> fullPXNSExpiryData = this.ExposedFlightFeatures.Where(o => o.StartsWith(Flighting.Features.PXNSSetExpiry)).ToList();
                foreach (var expiryString in fullPXNSExpiryData)
                {
                    string[] expiryData = expiryString.Split('_');

                    if (expiryData.Length == 4)
                    {
                        string cardType = expiryData[1];
                        string lastFour = expiryData[2];
                        string expiryOverride = expiryData[3];

                        if (pi.PaymentMethod != null && string.Equals(pi.PaymentMethod.PaymentMethodType, cardType, StringComparison.OrdinalIgnoreCase) &&
                            pi.PaymentInstrumentDetails != null && string.Equals(pi.PaymentInstrumentDetails.LastFourDigits, lastFour, StringComparison.OrdinalIgnoreCase) &&
                            Regex.IsMatch(expiryOverride, @"^\d{6,6}$") &&
                            pi.LastUpdatedTime != null && pi.LastUpdatedTime.Value <= DateTime.UtcNow.AddHours(-1))
                        {
                            if (expiryOverride.StartsWith("0"))
                            {
                                pi.PaymentInstrumentDetails.ExpiryMonth = $"{expiryOverride[1]}";
                            }
                            else
                            {
                                pi.PaymentInstrumentDetails.ExpiryMonth = $"{expiryOverride[0]}{expiryOverride[1]}";
                            }

                            pi.PaymentInstrumentDetails.ExpiryYear = $"{expiryOverride[2]}{expiryOverride[3]}{expiryOverride[4]}{expiryOverride[5]}";
                        }
                    }
                }
            }
        }

        private async Task<HttpResponseMessage> CreatePXChallengeResponse(PIDLData pi, string language, string partner, string pxChallengeSessionId, EventTraceActivity traceActivity, ServiceErrorResponse pimsBadRequestError = null, string accountId = null)
        {
            dynamic pidetails = pi[Constants.PaymentInstrument.Details];
            if (pidetails != null && pidetails.ContainsKey(Constants.DataDescriptionPropertyNames.ResourceActionContext))
            {
                if (string.IsNullOrEmpty(pxChallengeSessionId))
                {
                    pxChallengeSessionId = await PXChallengeManagementHandler.CreatePXChallengeSessionId(Request.Query.ToDictionary(list => list.Key, list => list.Value.ToString()), accountId, traceActivity, this.PidlSdkVersion);
                }

                dynamic getResourceActionContext = Newtonsoft.Json.JsonConvert.DeserializeObject(pidetails[Constants.DataDescriptionPropertyNames.ResourceActionContext].ToString());
                ActionContext resourceActionContext = Newtonsoft.Json.JsonConvert.DeserializeObject<ActionContext>(pidetails[Constants.DataDescriptionPropertyNames.ResourceActionContext].ToString());

                // ActionContext is not able to extract parameters, so deserializing resourceActionContext again and using SetParameters to set parameters for ActionContext was done as a work around
                dynamic getParameter = getResourceActionContext.resourceActionContext.pidlDocInfo.parameters;
                Dictionary<string, string> parameter = getParameter.ToObject<Dictionary<string, string>>();
                parameter[Constants.QueryParameterName.ShowChallenge] = bool.TrueString;
                parameter[Constants.QueryParameterName.PXChallengeSessionId] = pxChallengeSessionId;

                resourceActionContext.ResourceActionContext.PidlDocInfo.SetParameters(parameter);

                var fallbackPidlResource = new PIDLResource()
                {
                    ClientAction = new ClientAction(ClientActionType.Pidl, resourceActionContext)
                };

                fallbackPidlResource.ClientAction.PidlRetainUserInput = true;
                fallbackPidlResource.ClientAction.PidlUserInputToClear = Constants.DataDescriptionPropertyNames.CaptchaSolution;

                PIDLError errorData = new PIDLError();

                if (pimsBadRequestError != null)
                {
                    errorData = new PIDLError() { Code = pimsBadRequestError.ErrorCode };

                    if (pimsBadRequestError.Details != null)
                    {
                        foreach (var errorDetail in pimsBadRequestError.Details)
                        {
                            errorData.AddDetail(new PIDLErrorDetail() { Code = errorDetail.ErrorCode, Message = errorDetail.Message, Target = errorDetail.Target });
                        }
                    }
                }

                fallbackPidlResource.ClientAction.PidlError = errorData;
                return this.Request.CreateResponse(fallbackPidlResource);
            }
            else
            {
                var serviceErrorResp = GenerateValidationFailedServiceErrorResponse(language);
                this.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.Message] = $"{GlobalConstants.AbnormalDetection.LogMsgWhenCaughtByPX} " +
                    $"by flight {Flighting.Features.PXChallengeSwitch}. Missing currentContext for Add CreditCard request with AccountId: {accountId}";
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, serviceErrorResp, GlobalConstants.HeaderValues.JsonContent);
            }
        }

        private async Task<HttpResponseMessage> GetXboxCoBrandedCardNativePidl(string language, string partner, string country, string sessionId, string channel, string referrerId, string ocid, PaymentExperienceSetting setting = null)
        {
            List<PIDLResource> retVal = PIDLResourceFactory.Instance.GetStaticPidlDescriptions(Constants.StaticDescriptionTypes.XboxCoBrandedCardQRCodePidl, language, PXCommon.Constants.PartnerNames.XboxNative);

            string emailAddress = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.EmailAddress);
            if (string.IsNullOrEmpty(emailAddress))
            {
                emailAddress = LocalizationRepository.Instance.GetLocalizedString("Email address", language);
            }

            string xboxUrlLanguageMarket;
            if (!Constants.XboxCardApplyCountryToLanguage.TryGetValue(country.ToLower(), out xboxUrlLanguageMarket))
            {
                xboxUrlLanguageMarket = Constants.XboxCardApplyCountryToLanguage["us"];
            }

            var qrCodeURL = string.Format(Constants.PidlUrlConstants.XboxCoBrandedCardQRCodeURL, xboxUrlLanguageMarket, channel, referrerId, sessionId);
            var webviewURL = string.Format(Constants.PidlUrlConstants.XboxCoBrandedCardWebviewURL, xboxUrlLanguageMarket, channel, referrerId, sessionId);

            if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXXboxCardApplicationOriginPPEUrl) ||
                this.IsPartnerFlightExposed(Flighting.Features.PXXboxCardApplicationOriginPPEUrl))
            {
                qrCodeURL = qrCodeURL.Replace(Constants.RequestDomains.XboxCom, Constants.RequestDomains.OriginPPEXboxCom);
                webviewURL = webviewURL.Replace(Constants.RequestDomains.XboxCom, Constants.RequestDomains.OriginPPEXboxCom);
            }

            if (!string.IsNullOrEmpty(ocid))
            {
                qrCodeURL += "&ocid=" + ocid;
                webviewURL += "&ocid=" + ocid;
            }

            string gamertag = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.XboxProfile.Gamertag);
            if (!string.IsNullOrEmpty(gamertag))
            {
                webviewURL += "&gt=" + gamertag;
            }

            string shortUrl = null;
            if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXXboxCardApplicationEnableShortUrl))
            {
                EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
                shortUrl = await this.CreateShortUrl(qrCodeURL, Constants.ShortURLServiceTimeToLive.ShortURLActiveTTL, traceActivityId);
                if (shortUrl != null)
                {
                    qrCodeURL = shortUrl;
                }
            }

            bool useIntPolling = this.Request.RequestUri.ToString().Contains(Constants.RequestDomains.Localhost);

            PIDLResourceFactory.UpdateXboxCoBrandedCardQrCodeDescription(
                retVal,
                qrCodeURL,
                webviewURL,
                language,
                Constants.ChallengeDescriptionTypes.XboxCoBrandedCard,
                useIntPolling,
                partner,
                emailAddress,
                country,
                this.ExposedFlightFeatures,
                sessionId,
                null,
                channel,
                referrerId);

            // Should set button as navigation action unless PXXboxCardApplicationEnableWebview flight is passed in
            if (!this.ExposedFlightFeatures.Contains(Flighting.Features.PXXboxCardApplicationEnableWebview) &&
                    !this.IsPartnerFlightExposed(Flighting.Features.PXXboxCardApplicationEnableWebview))
            {
                ButtonDisplayHint applyOnConsoleButton = retVal.First<PIDLResource>()?.GetDisplayHintById(Constants.DisplayHintIds.XboxCoBrandedCardQrCodeRedirectButton) as ButtonDisplayHint;
                applyOnConsoleButton.Action.Context = webviewURL;
                applyOnConsoleButton.Action.ActionType = DisplayHintActionType.navigate.ToString();
                applyOnConsoleButton.Action.DestinationId = Constants.DestinationId.ApplyOnConsole;
            }

            if (shortUrl != null && this.ExposedFlightFeatures.Contains(Flighting.Features.PXXboxCardApplicationEnableShortUrlText))
            {
                List<DisplayHint> shortUrlTextHints = retVal.First<PIDLResource>()?.GetAllDisplayHintsOfId(Constants.DisplayHintIds.XboxCoBrandedCardQrCodeShortUrlText);

                if (shortUrlTextHints != null)
                {
                    foreach (TextDisplayHint shortUrlText in shortUrlTextHints)
                    {
                        shortUrlText.DisplayContent = shortUrlText.DisplayContent.Replace(Constants.StringPlaceholders.ShortUrlPlaceholder, shortUrl);
                    }
                }
            }
            else
            {
                retVal.First<PIDLResource>()?.RemoveDisplayHintById("xboxCoBrandedCardQrCodeShortUrlText");
            }

            FeatureContext featureContext = new FeatureContext(
                country,
                GetSettingTemplate(partner, setting, Constants.DescriptionTypes.PaymentInstrumentDescription),
                Constants.DescriptionTypes.PaymentInstrumentDescription,
                Constants.Operations.Apply,
                null,
                language,
                null,
                this.ExposedFlightFeatures,
                setting?.Features,
                null,
                null,
                originalPartner: partner,
                isGuestAccount: GuestAccountHelper.IsGuestAccount(this.Request));

            PostProcessor.Process(retVal, PIDLResourceFactory.FeatureFactory, featureContext);

            ClientAction action = new ClientAction(ClientActionType.Pidl) { Context = retVal };
            return this.Request.CreateResponse(HttpStatusCode.OK, new PIDLResource { ClientAction = action });
        }

        private async Task<string> GetValidatedSessionId(EventTraceActivity traceActivityId, InitializeRequest initializeData)
        {
            if (string.IsNullOrEmpty(initializeData.Card))
            {
                throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, string.Format("response status code: {0}, error: {1}", "RequiredParametersMissing", "Card is required if sessionId is not provided")));
            }

            if (string.IsNullOrEmpty(initializeData.Channel))
            {
                throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, string.Format("response status code: {0}, error: {1}", "RequiredParametersMissing", "Channel is required if sessionId is not provided")));
            }

            if (string.IsNullOrEmpty(initializeData.Market))
            {
                throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, string.Format("response status code: {0}, error: {1}", "RequiredParametersMissing", "Market is required if sessionId is not provided")));
            }

            var initializeResponse = await this.Settings.IssuerServiceAccessor.Initialize(initializeData);
            if (string.IsNullOrEmpty(initializeResponse.SessionId))
            {
                throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.ServiceError, "SessionId from IssuerServiceAccessor.Initialize was not valid"));
            }

            return initializeResponse.SessionId;
        }

        private List<PIDLResource> GetSelectPaymentResourcePidl(
            string country = null,
            string language = null,
            string partner = Constants.ServiceDefaults.DefaultPartnerName,
            string operation = null,
            string scenario = null,
            string classicProduct = null,
            string billableAccountId = null,
            List<PaymentInstrument> paymentInstrumentList = null)
        {
            // Use Partner Settings if enabled for the partner
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(operation);
            this.EnableFlightingsInPartnerSetting(setting, country);

            List<PIDLResource> retVal = null;

            if ((PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseClientSidePrefill, country, setting) || PartnerHelper.IsClientSideListPIPrefillRequired(partner) || PartnerHelper.IsCartPartner(partner)) && operation.Equals(Constants.Operations.SelectInstance, StringComparison.OrdinalIgnoreCase))
            {
                retVal = PIDLResourceFactory.GetPaymentInsturmentSelectDescriptions(country, language, partner, scenario, classicProduct, billableAccountId, this.ExposedFlightFeatures, setting: setting);

                PIDLResourceFactory.ProcessPossibleOptions(retVal, Constants.DisplayHintIds.PaymentInstrumentListPi, country, language, paymentInstrumentList);
                PIDLResourceFactory.AddPidlActionToDisplayHint<ButtonDisplayHint>(retVal, Constants.DisplayHintIds.ChooseNewPaymentMethodLink, PIActionType.SelectResourceType);

                FeatureContext featureContext = new FeatureContext(
                    country,
                    GetSettingTemplate(partner, setting, Constants.DescriptionTypes.PaymentMethodDescription),
                    Constants.DescriptionTypes.PaymentMethodDescription,
                    operation,
                    scenario,
                    language,
                    null,
                    this.ExposedFlightFeatures,
                    setting?.Features,
                    originalPartner: partner,
                    isGuestAccount: GuestAccountHelper.IsGuestAccount(this.Request));

                PostProcessor.Process(retVal, PIDLResourceFactory.FeatureFactory, featureContext);
            }

            return retVal;
        }

        private async Task<HttpResponseMessage> HandleStoredValuePI(PIDLData pi, string accountId, string partner, string country, string language, FeatureContext featureContext, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures = null, PaymentExperienceSetting setting = null)
        {
            // Extract user context from the request
            string userId = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.Puid);
            string ipAddress = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.DeviceInfo.IPAddress);
            if (!pi.ContainsKey("tokenIdentifierValue") || !pi.ContainsKey("actionType"))
            {
                throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, string.Format("response status code: {0}, error: {1}", "InvalidPIData", "tokenIdentifierValue or action is missing")));
            }

            if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXDisableRedeemCSVFlow, StringComparer.OrdinalIgnoreCase))
            {
                // If the PXDisableRedeemCSVFlow flight is enabled, return a failure response to disable the redeem CSV flow
                var serviceErrorResponse = new ServiceErrorResponse(CSVTokenStatus.CouldNotValidate.ToString(), "Couldn't validate CSV token. Please try again later.", GlobalConstants.ServiceName)
                {
                    InnerError = new ServiceErrorResponse(CSVTokenStatus.CouldNotValidate.ToString(), "Couldn't validate CSV token. Please try again later.", PXCommon.Constants.ServiceNames.TokenPolicyService),
                    CorrelationId = Guid.NewGuid().ToString(),
                    Details = new List<ServiceErrorDetail>()
                };

                return this.Request.CreateResponse(HttpStatusCode.BadRequest, serviceErrorResponse);
            }

            string tokenIdentifierValue = pi["tokenIdentifierValue"].ToString();
            string actionType = pi["actionType"].ToString();

            ClientAction clientAction;
            switch (actionType)
            {
                case "validate":
                    CSVTokenValidationResult validationResult = await CSVTokenHelper.ValidateCSVToken(this.Settings, userId, tokenIdentifierValue, country, language, ipAddress, traceActivityId);
                    if (validationResult?.TokenStatus == CSVTokenStatus.ValidCSVToken && validationResult.TokenValue != null)
                    {
                        string formattedCurrency = CurrencyHelper.FormatCurrency(country, language, validationResult.TokenValue ?? 0, validationResult.TokenCurrency);
                        List<PIDLResource> confirmRedeemPIDL = PIDLResourceFactory.Instance.GetConfirmCSVRedeemDescriptions(country, language, partner, setting: setting);

                        // postProcess the PIDL using featureContext
                        PostProcessor.Process(confirmRedeemPIDL, PIDLResourceFactory.FeatureFactory, featureContext);
                        clientAction = new ClientAction(ClientActionType.Pidl)
                        {
                            Context = ProcessRedeemCSVPIDL(confirmRedeemPIDL, country, language, partner, tokenIdentifierValue, formattedCurrency)
                        };
                    }
                    else
                    {
                        var serviceErrorResponse = new ServiceErrorResponse(
                             errorCode: (validationResult?.TokenStatus ?? CSVTokenStatus.Unknown).ToString(),
                             message: "Invalid CSV token. Please try again.",
                             source: GlobalConstants.ServiceName);

                        serviceErrorResponse.CorrelationId = Guid.NewGuid().ToString();

                        serviceErrorResponse.InnerError = new ServiceErrorResponse(
                            errorCode: (validationResult?.TokenStatus ?? CSVTokenStatus.Unknown).ToString(),
                            message: "Invalid CSV token. Please try again.",
                            source: PXCommon.Constants.ServiceNames.TokenPolicyService);

                        serviceErrorResponse.Details = new List<ServiceErrorDetail>();

                        return this.Request.CreateResponse(HttpStatusCode.BadRequest, serviceErrorResponse);
                    }

                    break;
                case "redeem":
                    CSVTokenRedemptionResult redeemResult = await CSVTokenHelper.RedeemCSVToken(this.Settings, userId, tokenIdentifierValue, country, language, ipAddress, traceActivityId);
                    if (redeemResult?.IsSuccess ?? false)
                    {
                        PaymentInstrument csvPI = null;

                        // Get CSV PI details
                        csvPI = await PIHelper.GetCSVPI(this.Settings, accountId, partner, country, language, traceActivityId, exposedFlightFeatures: this.ExposedFlightFeatures);
                        
                        clientAction = new ClientAction(ClientActionType.ReturnContext)
                        {
                            Context = new { redeemResult, csvPI }
                        };
                    }
                    else
                    {
                        clientAction = new ClientAction(ClientActionType.Failure)
                        {
                            Context = new { redeemResult }
                        };
                    }

                    break;
                default:
                    throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, string.Format("response status code: {0}, error: {1}", "InvalidPIData", "action is invalid")));
            }

            PIDLResource retVal = new PIDLResource { ClientAction = clientAction };
            return this.Request.CreateResponse(retVal);
        }

        private async Task<string> AddCcQrCodeAnonymousCallCheck(PIDLData pi, string partner, string userSessionId, EventTraceActivity traceActivityId)
        {
            string accountId = string.Empty;

            if (string.Equals(pi[Constants.PaymentInstrument.PaymentMethodFamily].ToString(), Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(pi[Constants.PaymentInstrument.PaymentMethodOperation].ToString(), Constants.Operations.Add, StringComparison.OrdinalIgnoreCase)
                && userSessionId != null)
            {
                QRCodeSecondScreenSession qrCodePaymentSessionData = await this.SecondScreenSessionHandler.GetQrCodeSessionData(userSessionId, traceActivityId);

                if (!PIHelper.IsQrCodeValidSession(qrCodePaymentSessionData))
                {
                    throw new ValidationException(ErrorCode.RequestFailed, "AddCcQrCodeAnonymousCallCheck failed");
                }

                // PIFD currently does not pass if it is a test account because this is an anonymous call. Get the test acc information from the session that was created during the authenticate call
                if (qrCodePaymentSessionData.AllowTestHeader)
                {
                    this.Request.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.TestHeader, Constants.TestAccountHeaders.MDollarPurchase);
                }
                else
                {
                    // remove x-ms-test header for any non test accounts. Kept to prevent malicious users. 
                    this.Request.Headers.Remove(PaymentConstants.PaymentExtendedHttpHeaders.TestHeader);
                }

                SllWebLogger.TraceServerMessage("AddNewPI", traceActivityId.CorrelationVectorV4.Value, traceActivityId.ActivityId.ToString(), $"userAccountId:{qrCodePaymentSessionData.AccountId}", EventLevel.Informational);
            }

            return accountId;
        }

        private async Task<PaymentInstrument> UpdateAnonymousAddCCCall(PaymentInstrument newPI, string sessionId, PaymentInstrumentStatus piStatus, string piid, EventTraceActivity traceActivityId)
        {
            QRCodeSecondScreenSession qrCodePaymentSessionData = await this.SecondScreenSessionHandler.GetQrCodeSessionData(sessionId, traceActivityId);

            if (!PIHelper.IsQrCodeValidSession(qrCodePaymentSessionData))
            {
                throw new ValidationException(ErrorCode.RequestFailed, "UpdateAnonymousAddCCCall failed");
            }

            //// Disable sessionId, so that it can't be used again, update the Pi status
            await SecondScreenSessionHandler.UpdateQrCodeSessionResourceData(qrCodePaymentSessionData.UseCount, qrCodePaymentSessionData, traceActivityId, status: piStatus, piid: piid);

            return newPI;
        }

        private async Task<HttpResponseMessage> AddPaymentInstrumentToPaymentAccount(
            EventTraceActivity traceActivityId,
            RequestContext requestContext,
            PIDLData pi,
            string language = "en-us",
            string partner = Constants.ServiceDefaults.DefaultPartnerName,
            string country = null)
        {
            if (requestContext == null || string.IsNullOrWhiteSpace(requestContext.PaymentAccountId))
            {
                throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, string.Format("response status code: {0}, error: {1}", "InvalidPIData", "paymentAccountId is missing")));
            }

            if (string.IsNullOrWhiteSpace(requestContext.RequestId))
            {
                throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, string.Format("response status code: {0}, error: {1}", "InvalidPIData", "RequestId is missing")));
            }

            var additionalPIProps = new Dictionary<string, object>() { { "ValidationType", "none" }, { "AttachmentType", AttachmentType.Standalone } };
            SetPiData(pi, additionalPIProps);

            var metaData = ProxyController.GetMetaData(requestContext);
            var additionalProps = new Dictionary<string, object>() { { "UsageType", UsageType.Inline }, { "MetaData", metaData } };
            ProxyController.SetDetailsData(pi, additionalProps, traceActivityId);

            return await this.AddNewPI(
                                traceActivityId: traceActivityId,
                                accountId: requestContext.PaymentAccountId,
                                pi: pi,
                                language: language,
                                partner: partner,
                                billableAccountId: null,
                                completePrerequisites: false,
                                country: country,
                                scenario: null,
                                orderId: null,
                                requestId: requestContext.RequestId);
        }
    }
}