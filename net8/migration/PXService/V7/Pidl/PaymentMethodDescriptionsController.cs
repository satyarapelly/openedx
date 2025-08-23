// <copyright file="PaymentMethodDescriptionsController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.WebUtilities;
    using Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.Pidl.Localization;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Model.PXInternal;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using PaymentCommonInstruments = Microsoft.Commerce.Payments.Common.Instruments;
    using PXCommonConstants = Microsoft.Commerce.Payments.PXCommon.Constants;
    using ActionContext = Microsoft.Commerce.Payments.PidlModel.V7.ActionContext;

    [ApiController]
    [Route("{version}")]

    public class PaymentMethodDescriptionsController : ProxyController
    {
        private static class AccountV3Headers
        {
            public const string Etag = "ETag";
            public const string IfMatch = "If-Match";
        }

        private Dictionary<string, List<string>> northStarExperiencePartnersScenarios = new Dictionary<string, List<string>>
        {
            {
                Constants.PartnerName.AmcWeb,
                new List<string>()
                {
                    Constants.ScenarioNames.PayNow,
                    Constants.ScenarioNames.ChangePI
                }
            }
        };

        /// <summary>
        /// Get Anonymous Pidl
        /// </summary>
        /// <group>PaymentMethodDescriptions</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/paymentMethodDescriptions?country={country}&amp;language={language}&amp;partner={partner}&amp;operation={operation}&amp;scenario={scenario}</url>
        /// <param name="family" required="false" cref="string" in="query">Payment method family name</param>
        /// <param name="type" required="false" cref="string" in="query">Payment method type name</param>
        /// <param name="country" required="false" cref="string" in="query">Two letter country code</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <param name="operation" required="false" cref="string" in="query">Operation name</param>
        /// <param name="scenario" required="false" cref="string" in="query">Scenario name</param>
        /// <param name="sessionId" required="false" cref="string" in="query">SessionId used for apply</param>
        /// <response code="200">List&lt;PIDLResource&gt; for GetAnonymousPidl</response>
        /// <returns>A list of PIDLResource</returns>
        //// Anonymous Add or Update
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Needs to be an instance method for Route action selection")]
        [HttpGet()]
        public async Task<List<PIDLResource>> GetAnonymousPidl(
            string family,
            string type = null,
            string country = null,
            string language = null,
            string partner = Constants.ServiceDefaults.DefaultPartnerName,
            string operation = Constants.ServiceDefaults.DefaultOperationType,
            string scenario = null,
            string sessionId = null)
        {
            string apiName = string.Concat(GlobalConstants.APINames.GetPaymentMethodDescriptions, operation);
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddTracingProperties(null, null, family, type, country);
            this.Request.AddPartnerProperty(partner?.ToLower());
            this.Request.AddPidlOperation(operation?.ToLower());

            var requestContext = this.GetRequestContext(traceActivityId);

            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(operation);
            string updatedPartner = string.Empty;

            if (IsSecondScreenAddPiFlow(scenario, family, operation))
            {
                updatedPartner = Constants.TemplateName.SecondScreenTemplate;

                GetAllowedPaymentMethodTypes(ref type);
            }

            if (string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase))
            {
                ValidateAndInferCountry(ref country, family, type, operation);
            }

            if (string.Equals(operation, Constants.Operations.Search, StringComparison.OrdinalIgnoreCase))
            {
                return PIDLResourceFactory.GetPaymentMethodSearchDescriptions(family, type, country, language, partner, setting: setting);
            }

            var paymentMethodList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PaymentMethod>>("[{\"paymentMethodFamily\":\"credit_card_marketplace_card\",\"paymentMethodType\":\"amex\",\"display\":{\"name\":\"American Express\",\"logo\":\"https://pmservices.cp.microsoft.com/staticresourceservice/images/v3/logo_amex.png\",\"logos\":[{\"mimeType\":\"image/png\",\"url\":\"https://pmservices.cp.microsoft.com/staticresourceservice/images/v3/logo_amex.png\"},{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft.com/staticresourceservice/images/v4/logo_amex.svg\"}]},\"properties\":{\"offlineRecurring\":true,\"userManaged\":true,\"soldToAddressRequired\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"moBillingIdentityUrl\":\"\",\"riskyPaymentMethod\":false,\"authWindow\":10080,\"fundsAvailabilityWindow\":0,\"multipleLineItemsSupported\":true,\"splitPaymentSupported\":true,\"purchaseWaitTime\":0}},{\"paymentMethodFamily\":\"credit_card_marketplace_card\",\"paymentMethodType\":\"discover\",\"display\":{\"name\":\"Discover\",\"logo\":\"https://pmservices.cp.microsoft.com/staticresourceservice/images/v3/logo_discover.png\",\"logos\":[{\"mimeType\":\"image/png\",\"url\":\"https://pmservices.cp.microsoft.com/staticresourceservice/images/v3/logo_discover.png\"},{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft.com/staticresourceservice/images/v4/logo_discover.svg\"}]},\"properties\":{\"offlineRecurring\":true,\"userManaged\":true,\"soldToAddressRequired\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"moBillingIdentityUrl\":\"\",\"riskyPaymentMethod\":false,\"authWindow\":10080,\"fundsAvailabilityWindow\":0,\"multipleLineItemsSupported\":true,\"splitPaymentSupported\":true,\"purchaseWaitTime\":0}},{\"paymentMethodFamily\":\"credit_card_marketplace_card\",\"paymentMethodType\":\"mc\",\"display\":{\"name\":\"Mastercard\",\"logo\":\"https://pmservices.cp.microsoft.com/staticresourceservice/images/v3/logo_mc.png\",\"logos\":[{\"mimeType\":\"image/png\",\"url\":\"https://pmservices.cp.microsoft.com/staticresourceservice/images/v3/logo_mc.png\"},{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft.com/staticresourceservice/images/v4/logo_mc.svg\"}]},\"properties\":{\"offlineRecurring\":true,\"userManaged\":true,\"soldToAddressRequired\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"moBillingIdentityUrl\":\"\",\"riskyPaymentMethod\":false,\"authWindow\":10080,\"fundsAvailabilityWindow\":0,\"multipleLineItemsSupported\":true,\"splitPaymentSupported\":true,\"purchaseWaitTime\":0}},{\"paymentMethodFamily\":\"credit_card_marketplace_card\",\"paymentMethodType\":\"visa\",\"display\":{\"name\":\"Visa\",\"logo\":\"https://pmservices.cp.microsoft.com/staticresourceservice/images/v3/logo_visa.png\",\"logos\":[{\"mimeType\":\"image/png\",\"url\":\"https://pmservices.cp.microsoft.com/staticresourceservice/images/v3/logo_visa.png\"},{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft.com/staticresourceservice/images/v4/logo_visa.svg\"}]},\"properties\":{\"offlineRecurring\":true,\"userManaged\":true,\"soldToAddressRequired\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"moBillingIdentityUrl\":\"\",\"riskyPaymentMethod\":false,\"authWindow\":10080,\"fundsAvailabilityWindow\":0,\"multipleLineItemsSupported\":true,\"splitPaymentSupported\":true,\"purchaseWaitTime\":0}}]");
            string templateOrPartnerName = partner;
            if (string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase))
            {
                IList<KeyValuePair<string, string>> additionalHeaders = this.GetAdditionalHeadersFromRequest();
                templateOrPartnerName = ProxyController.GetSettingTemplate(partner, setting, Constants.DescriptionTypes.PaymentMethodDescription, type, family);

                // For PaaS partners, we need to get the eligible payment methods from PO service
                if (requestContext != null && !string.IsNullOrWhiteSpace(requestContext.RequestId))
                {
                    var eligiblePaymentMethods = await this.Settings.PaymentOrchestratorServiceAccessor.GetEligiblePaymentMethods(traceActivityId, requestContext.RequestId);
                    paymentMethodList = MapToPaymentMethods(eligiblePaymentMethods);
                }
                else
                {
                    paymentMethodList = await this.Settings.PIMSAccessor.GetPaymentMethods(country, family, type, language, traceActivityId, additionalHeaders, templateOrPartnerName, this.ExposedFlightFeatures, operation, setting: setting);
                }
            }

            var paymentMethodHash = new HashSet<PaymentMethod>(paymentMethodList);

            List<PIDLResource> retVal = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(paymentMethodHash, country, family, type, operation, language, templateOrPartnerName, null, null, null, null, false, exposedFlightFeatures: this.ExposedFlightFeatures, scenario, null, null, null, null, null, null, null, partner);
            if (string.Equals(templateOrPartnerName, Constants.PartnerName.MarketPlace, StringComparison.InvariantCultureIgnoreCase) && string.Equals(family, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                foreach (PIDLResource resource in retVal)
                {
                    resource.SetPropertyState("country", true, new List<string>() { "cn" });
                }
            }

            if (IsSecondScreenAddPiFlow(scenario, family, operation))
            {
                QRCodeSecondScreenSession qrCodePaymentSessionData = await this.GetAndValidateQrCodeSession(sessionId, traceActivityId, paymentMethodList);
                retVal = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(paymentMethodHash, country, family, type, operation, language, updatedPartner, qrCodePaymentSessionData.Email, null, null, null, false, exposedFlightFeatures: this.ExposedFlightFeatures, scenario, firstName: qrCodePaymentSessionData.FirstName, lastName: qrCodePaymentSessionData.LastName);
                UpdateAnonymousAddScreenWithPersonalData(qrCodePaymentSessionData, ref retVal);
                this.UpdateSessionDataForSecondScreen(qrCodePaymentSessionData, traceActivityId);
            }

            if (string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase))
            {
                PIDLResourceFactory.RemoveEmptyPidlContainerHints(retVal);
                this.EnableFlightingsInPartnerSetting(setting, country);
                var smdMarkets = await this.GetSingleMarkets(country, traceActivityId);
                FeatureContext featureContext = new FeatureContext(
                    country,
                    templateOrPartnerName,
                    Constants.DescriptionTypes.PaymentMethodDescription,
                    operation,
                    scenario,
                    language,
                    null,
                    this.ExposedFlightFeatures,
                    setting?.Features,
                    family,
                    type,
                    smdMarkets: smdMarkets,
                    originalPartner: partner,
                    isGuestAccount: GuestAccountHelper.IsGuestAccount(this.Request),
                    tokenizationPublicKey: await this.GetTokenizationPublicKey(traceActivityId),
                    tokenizationServiceUrls: this.Settings.TokenizationServiceAccessor.GetTokenizationServiceUrls(),
                    sessionId: sessionId,
                    xmsFlightHeader: this.GetPartnerXMSFlightExposed());

                PostProcessor.Process(retVal, PIDLResourceFactory.FeatureFactory, featureContext);

                DescriptionHelper.RemoveUpdateAddresEnabled(family, scenario, retVal, this.ExposedFlightFeatures);

                if (!string.Equals(family, "ewallet", StringComparison.OrdinalIgnoreCase))
                {
                    RemoveCardDisplayTransformation(retVal);
                }

                RemoveResolutionRegex(partner, retVal);

                RemoveLuhnValidation(family, partner, retVal, this.ExposedFlightFeatures);

                DescriptionHelper.AddOrUpdateServerErrorCode_CreditCardFamily(null, family, language, partner, operation, retVal, this.ExposedFlightFeatures);

                if ((this.ExposedFlightFeatures?.Contains(Flighting.Features.PXPaasAddCCDfpIframe, StringComparer.OrdinalIgnoreCase) ?? false)
                    && (requestContext != null && !string.IsNullOrWhiteSpace(requestContext.RequestId)))
                {
                    DescriptionHelper.AddDFPIframe(retVal, requestContext.RequestId, this.ExposedFlightFeatures);
                }

                foreach (var pidl in retVal)
                {
                    pidl.RemoveEmptyPidlContainerHints();

                    if (requestContext == null || string.IsNullOrWhiteSpace(requestContext.RequestId))
                    {
                        // TODO: the following submit url should be constructed through PIDL factory, mark it for next CR
                        var saveButtonLink = pidl.GetDisplayHintById("saveButton")?.Action?.Context as Microsoft.Commerce.Payments.PXCommon.RestLink;
                        if (saveButtonLink != null)
                        {
                            saveButtonLink.Href = "https://{pifd-endpoint}/viewModel/cards";
                        }

                        var useCardButtonLink = pidl.GetDisplayHintById("useCardButton")?.Action?.Context as Microsoft.Commerce.Payments.PXCommon.RestLink;
                        if (useCardButtonLink != null)
                        {
                            useCardButtonLink.Href = "https://{pifd-endpoint}/viewModel/cards";
                        }
                    }
                }
            }

            return retVal;
        }

        /// <summary>
        /// Get Payment Method Descriptions by Family and Type ("/GetByFamilyAndType" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>PaymentMethodDescriptions</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/paymentMethodDescriptions/GetByFamilyAndType?family={family}&amp;type={type}&amp;piid={piid}&amp;country={country}&amp;language={language}&amp;partner={partner}&amp;operation={operation}&amp;scenario={scenario}&amp;classicProduct={classicProduct}&amp;billableAccountId={billableAccountId}</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="family" required="true" cref="string" in="query">Payment method family name</param>
        /// <param name="type" required="false" cref="string" in="query">Payment method type name</param>
        /// <param name="country" required="false" cref="string" in="query">Two letter country code</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <param name="operation" required="false" cref="string" in="query">Operation name</param>
        /// <param name="classicProduct" required="false" cref="string" in="query">Classic product name</param>
        /// <param name="billableAccountId" required="false" cref="string" in="query">Billable account Id</param>
        /// <param name="piid" required="false" cref="string" in="query">Payment instrument id</param>
        /// <param name="scenario" required="false" cref="string" in="query">Scenario name</param>
        /// <param name="orderId" required="false" cref="string" in="query">Order Id</param>
        /// <param name="channel" required="false" cref="string" in="query">Channel name used for apply</param>
        /// <param name="referrerId" required="false" cref="string" in="query">ReferrerId used for apply</param>
        /// <param name="sessionId" required="false" cref="string" in="query">SessionId used for apply</param>
        /// <response code="200">List&lt;PIDLResource&gt; for get payment method descriptions by family and type</response>
        /// <returns>A list of PIDLResource</returns>
        //// If the family is specified, operation must be add or update
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Needs to be an instance method for Route action selection")]
        [HttpGet()]
        public async Task<List<PIDLResource>> GetByFamilyAndType(
            string accountId,
            string family,
            string type = null,
            string country = null,
            string language = null,
            string partner = Constants.ServiceDefaults.DefaultPartnerName,
            string operation = Constants.ServiceDefaults.DefaultOperationType,
            string classicProduct = null,
            string billableAccountId = null,
            string piid = null,
            string scenario = null,
            string orderId = null,
            string channel = null,
            string referrerId = null,
            string sessionId = null)
        {
            if ((PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner)
                || (Constants.PartnersToEnablePaypalSecondScreenForXbox.Contains(partner, StringComparer.OrdinalIgnoreCase)
                    && (this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnablePaypalSecondScreenForXbox, StringComparer.OrdinalIgnoreCase)
                        || Constants.CountriesToEnablePaypalSecondScreenForXbox.Contains(country, StringComparer.OrdinalIgnoreCase))))
                && string.Equals(family, Constants.PaymentMethodFamily.ewallet.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(type, Constants.PaymentMethodType.PayPal, StringComparison.OrdinalIgnoreCase)
                && string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase))
            {
                scenario = Constants.ScenarioNames.PaypalQrCode;
            }

            // Use Partner Settings if enabled for the partner
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(operation);

            if ((PXCommon.Constants.PartnerGroups.IsVenmoEnabledPartner(partner) || TemplateHelper.IsTemplateBasedPIDLIncludingDefaultTemplate(TemplateHelper.GetSettingTemplate(partner, setting, Constants.DescriptionTypes.PaymentMethodDescription, $"{Constants.PaymentMethodFamily.ewallet}.{Constants.PaymentMethodType.Venmo}")))
                && string.Equals(family, Constants.PaymentMethodFamily.ewallet.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(type, Constants.PaymentMethodType.Venmo, StringComparison.OrdinalIgnoreCase)
                && string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase)
                && string.Equals(country, Constants.CountryCodes.UnitedStates, StringComparison.OrdinalIgnoreCase))
            {
                if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner))
                {
                    scenario = Constants.ScenarioNames.VenmoQRCode;
                }

                if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.PxEnableVenmo))
                {
                    this.ExposedFlightFeatures.Add(Constants.PartnerFlightValues.PxEnableVenmo);
                }

                if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.PxEnableSelectPMAddPIVenmo))
                {
                    this.ExposedFlightFeatures.Add(Constants.PartnerFlightValues.PxEnableSelectPMAddPIVenmo);
                }
            }

            // To show AVS suggestions, PX has to enable in the flight and partners has to send the flight as well
            if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableAVSSuggestions, StringComparer.OrdinalIgnoreCase))
            {
                // Pass ShowAVSSuggestions
                // Remove it from x-ms-flight to prevent passing to PIMS
                if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.ShowAVSSuggestions))
                {
                    this.ExposedFlightFeatures.Add(Constants.PartnerFlightValues.ShowAVSSuggestions);
                    this.RemovePartnerFlight(Constants.PartnerFlightValues.ShowAVSSuggestions);

                    if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.EnableAVSAddtionalFlags))
                    {
                        this.ExposedFlightFeatures.Add(Constants.PartnerFlightValues.EnableAVSAddtionalFlags);
                        this.RemovePartnerFlight(Constants.PartnerFlightValues.EnableAVSAddtionalFlags);
                    }
                }

                // Pass ShowAVSSuggestionsModal
                // Remove it from x-ms-flight to prevent passing to PIMS
                if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.ShowAVSSuggestionsModal))
                {
                    this.ExposedFlightFeatures.Add(Constants.PartnerFlightValues.ShowAVSSuggestions);
                    this.ExposedFlightFeatures.Add(Flighting.Features.TradeAVSUsePidlModalInsteadofPidlPage);
                    this.RemovePartnerFlight(Constants.PartnerFlightValues.ShowAVSSuggestionsModal);
                }
            }

            this.AddOrRemovePartnerFlight(country, Constants.PartnerFlightValues.IndiaTokenizationConsentCapture, GlobalConstants.CountryCodes.IN);

            this.AddOrRemovePartnerFlight(country, Constants.PartnerFlightValues.IndiaExpiryGroupDelete, GlobalConstants.CountryCodes.IN);

            // Remove the exposed flights if the pidlsdkversion is less than a specified version in order for styling changes to line up.
            Version fullPidlSdkVersion = HttpRequestHelper.GetFullPidlSdkVersion(this.Request.ToHttpRequestMessage());

            if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner) && fullPidlSdkVersion != null)
            {
                Version lowestCompatiblePidlVersion = new Version(Constants.PidlSdkVersionNumber.PidlSdkMajor1, Constants.PidlSdkVersionNumber.PidlSdkMinor22, Constants.PidlSdkVersionNumber.PidlSdkBuild0, Constants.PidlSdkVersionNumber.PidlSdkAlpha144);

                this.FlightByPidlVersion(fullPidlSdkVersion, lowestCompatiblePidlVersion, Constants.PartnerFlightValues.ShowSummaryPage);

                this.FlightByPidlVersion(fullPidlSdkVersion, lowestCompatiblePidlVersion, Constants.PartnerFlightValues.ShowAVSSuggestions);
            }

            // Remove any unsupported flight features based on client version.
            RemoveUnsupportedFlightFeatures(this.ExposedFlightFeatures, this.PidlSdkVersion);

            string apiName = string.Concat(GlobalConstants.APINames.GetPaymentMethodDescriptions, operation);
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            this.Request.AddTracingProperties(accountId, null, family, type, country);
            this.Request.AddPartnerProperty(partner?.ToLower());
            this.Request.AddPidlOperation(operation?.ToLower());

            ValidateAndInferCountry(ref country, family, type, operation);

            // Pass ShowMiddleName to PidlFactory
            // Remove it from x-ms-flight to prevent passing to PIMS
            if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.ShowMiddleName))
            {
                this.ExposedFlightFeatures.Add(Constants.PartnerFlightValues.ShowMiddleName);
                this.RemovePartnerFlight(Constants.PartnerFlightValues.ShowMiddleName);
            }

            if (SkipAddPIForPaymentMethod(family, type, operation))
            {
                return await this.ReturnPIForPaymentMethodCanSkipAddPI(family, type, operation, accountId, traceActivityId, classicProduct, billableAccountId, partner, country);
            }

            // Enable flighting based on setting partner
            this.EnableFlightingsInPartnerSetting(setting, country);

            List<PIDLResource> retVal = null;

            if (this.IsInstancePIEnabled(type, family))
            {
                return await this.CreateInstancePI(accountId, country, type, classicProduct, billableAccountId, operation, traceActivityId, partner);
            }

            // For global PIs/PMs (e.g. paysafecard, sofort), as add PI is not needed, PX will return a restAction clientAction to PIDLSDK to get the PI details through paymentInstrumentsEx endpoint, and PIDLSDK will return PI details in the success event.
            if (this.IsGlobalPIInAddResourceEnabled(partner, operation, country, setting))
            {
                retVal = await this.ReturnRestActionToGetPIForGlobalPI(accountId, partner, country, language, family, type, traceActivityId, operation, setting);
                if (retVal != null)
                {
                    return retVal;
                }
            }

            if (string.Equals(operation, Constants.Operations.Show, StringComparison.OrdinalIgnoreCase))
            {
                retVal = PIDLResourceFactory.GetPaymentMethodShowDescriptions(family, type, country, language, partner, setting: setting);
            }
            else if (string.Equals(operation, Constants.Operations.SearchTransactions, StringComparison.OrdinalIgnoreCase))
            {
                retVal = null;

                string[] statusList = new string[] { Constants.PaymentInstrumentStatus.Active };
                List<KeyValuePair<string, string>> queryParams = new List<KeyValuePair<string, string>>()
                {
                  new KeyValuePair<string, string>(Constants.QueryParameterName.Language, language)
                };
                IList<PaymentInstrument> paymentInstruments = await this.Settings.PIMSAccessor.ListPaymentInstrument(accountId, 0, statusList, traceActivityId, queryParams, partner, language: language, country: country, exposedFlightFeatures: this.ExposedFlightFeatures, operation: operation, setting: setting);
                List<PaymentInstrument> paymentInstrumentsList = new List<PaymentInstrument>(paymentInstruments.Where(pi => pi.IsCreditCard()));
                paymentInstrumentsList?.ForEach(x =>
                x.PaymentInstrumentDetails.CardHolderName = JsonDataMasker.TruncateAndMaskName(JToken.Parse(JsonConvert.SerializeObject(x.PaymentInstrumentDetails?.CardHolderName)), 20).ToString());

                retVal = PIDLResourceFactory.GetPaymentInsturmentSearchTransactionsDescriptions(country, language, partner, paymentInstrumentsList, setting: setting);
            }
            else if (string.Equals(operation, Constants.Operations.FundStoredValue, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(family, Constants.PaymentMethodFamily.ewallet.ToString(), StringComparison.OrdinalIgnoreCase) &&
                (string.Equals(type, Constants.EwalletType.bitcoin.ToString(), StringComparison.OrdinalIgnoreCase) || type.IndexOf(Constants.EwalletType.stored_value.ToString(), StringComparison.OrdinalIgnoreCase) >= 0))
            {
                string ewalletPiId = null;
                var fundingOptions = new Dictionary<string, string>();

                if (string.Equals(type, Constants.EwalletType.bitcoin.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    ewalletPiId = PaymentCommonInstruments.GlobalPaymentInstrumentId.BitPay;
                    var catalog = await this.Settings.StoredValueServiceAccessor.GetStoredValueFundingCatalog("USD", traceActivityId);

                    foreach (var option in catalog)
                    {
                        string amount = option.Amount.ToString("F0");
                        fundingOptions.Add(amount, "$" + amount);
                    }
                }

                retVal = PIDLResourceFactory.GetPaymentMethodFundStoredValueSelectDescriptions(family, type, country, language, partner, ewalletPiId, fundingOptions, setting);
            }
            else
            {
                // Replace PIDL is same as Update PIDL
                bool isReplaceOperation = string.Equals(operation, Constants.Operations.Replace, StringComparison.OrdinalIgnoreCase);
                string operationName = isReplaceOperation ? Constants.Operations.Add : operation;

                // Prevent address reduction on non-cc PI
                // TODO: As part of T-54279751 investigation, the AddressNoCityState flow found to be not used by any partner
                // And below code & csv config related to it can be removed
                if (string.Equals(scenario, Constants.ScenarioNames.AddressNoCityState, StringComparison.OrdinalIgnoreCase)
                    && (!string.Equals(family, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase) || !string.Equals(country, Constants.CountryCodes.UnitedStates, StringComparison.OrdinalIgnoreCase) || !string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase)))
                {
                    // TODO: T-54217106: RS5 scenario is deprecating and not required to be added in templates
                    if (string.Equals(partner, Constants.PartnerName.OfficeOobe, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(partner, Constants.PartnerName.OXOOobe, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(partner, Constants.PartnerName.SmbOobe, StringComparison.OrdinalIgnoreCase))
                    {
                        scenario = Constants.ScenarioNames.RS5;
                    }
                    else
                    {
                        scenario = null;
                    }
                }

                string updatedPartner = string.Empty;

                // Risk Eligibility Check
                if (await this.IsRiskEligible(family, type, language, partner, operation, this.GetBillingAccountContext()) == false)
                {
                    return GetRiskEligibilityRejectedClientAction(language);
                }

                if (this.IsNorthStarExperiencePartnerAndScenario(partner, scenario))
                {
                    retVal = await this.GetNorthstarPaymentMethodDescriptions(traceActivityId, accountId, country, family, type, language, partner, operationName, classicProduct, billableAccountId, false, piid, scenario, setting: setting);
                }
                else if (this.IsConsoleAddCCQrCodeFlow(this.ExposedFlightFeatures, scenario, fullPidlSdkVersion, retVal))
                {
                    sessionId = await this.GenerateSessionIdAsync(accountId, country, partner, operation, language, traceActivityId);
                    updatedPartner = Constants.TemplateName.ConsoleTemplate; // TODO: Remove once PSS integrates xbox partners https://microsoft.visualstudio.com/OSGS/_workitems/edit/54303371
                    retVal = await this.GetPaymentMethodDescriptionsFromFactory(traceActivityId, accountId, country, family, type, language, updatedPartner, operation, classicProduct, billableAccountId, false, null, scenario, orderId, channel, referrerId, sessionId, this.ExposedFlightFeatures, setting: setting);

                    if (bool.Equals(retVal?.First().DisplayPages.First().HintId.Equals(Constants.DisplayHintIds.ProfilePrerequisitesPage), false))
                    {
                        PIDLResourceFactory.AddCCQrCodeInAddConsole(ref retVal, scenario, Constants.ChallengeTypes.CreditCardQrCode, Constants.TemplateName.ConsoleTemplate, type, family, language, country, this.ExposedFlightFeatures, sessionId, accountId, partner);
                    }
                }
                else
                {
                    retVal = await this.GetPaymentMethodDescriptionsFromFactory(
                    traceActivityId,
                    accountId,
                    country,
                    family,
                    type,
                    language,
                    partner,
                    operationName,
                    classicProduct,
                    billableAccountId,
                    false,
                    piid,
                    scenario,
                    orderId,
                    channel,
                    referrerId,
                    sessionId,
                    exposedFlightFeatures: this.ExposedFlightFeatures,
                    setting: setting);

                    if (ShouldUpdateTokenizationConsentPropertyDetails(family, country, operation, partner, this.ExposedFlightFeatures, setting, type))
                    {
                        UpdateTokenizationConsentPropertyDetails(retVal);
                    }
                }

                if (isReplaceOperation)
                {
                    ConvertUpdatePidlToReplacePidl(retVal, language, scenario);
                }

                bool isUpdateOperation = string.Equals(operation, Constants.Operations.Update, StringComparison.OrdinalIgnoreCase);
                if ((isReplaceOperation || isUpdateOperation) && string.Equals(partner, Constants.PartnerName.NorthStarWeb, StringComparison.OrdinalIgnoreCase))
                {
                    RemoveDefaultValuesInAddress(retVal);
                }
            }

            // Remove mandatory fields message when the scenario is DisplayOptionalFields or when the flight is ON.
            // TODO: This flight acts as safety net. We can remove this flight condition once webblends confirms no issues with this change
            if (string.Equals(scenario, Constants.ScenarioNames.DisplayOptionalFields, StringComparison.OrdinalIgnoreCase)
                || this.ExposedFlightFeatures.Contains(Flighting.Features.PXRemoveMandatoryFieldsMessage, StringComparer.OrdinalIgnoreCase))
            {
                ProxyController.RemoveDisplayDescription(retVal, new[] { "mandatory_fields_message" });
            }

            // Add modern Validate for azure, commercialstores, and xboxnative partners
            this.AddModernAVSValidationAction(retVal, partner, family, type, operation, language, country);

            // Added AVSSuggestion flags : family virtual and type invoice_basic, invoice_check for add operation
            if (string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase)
                && this.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.EnableAVSAddtionalFlags, StringComparer.OrdinalIgnoreCase)
                && Constants.PIFamilyTypeEnableAVSAdditionalFlags.ContainsKey(family)
                && Constants.PIFamilyTypeEnableAVSAdditionalFlags[family].Contains(type, StringComparer.OrdinalIgnoreCase))
            {
                foreach (PIDLResource pidl in retVal)
                {
                    // For include "is_customer_consented" and "is_avs_full_validation_succeeded" in payload when posting to Jarvis
                    ProxyController.AddHiddenCheckBoxElement(pidl, GlobalConstants.CommercialZipPlusFourPropertyNames.IsUserConsented, null);
                    ProxyController.AddHiddenCheckBoxElement(pidl, GlobalConstants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded, null);
                }
            }

            if (this.AllowViewTermsTrigger(partner))
            {
                AddViewTermsTriggerEventContext(retVal);
            }

            PIDLResourceFactory.RemoveEmptyPidlContainerHints(retVal);

            if (ShouldRemoveExpiryDateForTemplates(setting, family, type, partner, country, operation))
            {
                TryRemoveExpiryMonthAndYearFromPIDL(retVal);
            }

            // TODO: Remove the if block once all the partners in ShouldRemoveExpiryDateForIndiaCommercial are moved to use the template
            // This block is enabled by default for templates in above ShouldRemoveExpiryDateForTemplates.
            if (ShouldRemoveExpiryDateForIndiaCommercial(operation, country, partner, this.ExposedFlightFeatures))
            {
                TryRemoveExpiryMonthAndYearFromPIDL(retVal);
            }

            if (ShouldRemoveExpiryDateForIndiaConsumer(operation, country, partner, this.ExposedFlightFeatures))
            {
                if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner) && string.Equals(family, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    PIDLResourceFactory.RemoveIndiaEditExpiry(retVal, this.ExposedFlightFeatures);
                    PIDLResourceFactory.IndiaEditSummaryFooter(retVal);
                }
                else
                {
                    TryRemoveExpiryMonthAndYearFromPIDL(retVal, partner, scenario);
                }
            }
            else if (ShouldUpdateSummaryFooterForIndiaConsumer(operation, country, partner, family, this.ExposedFlightFeatures))
            {
                PIDLResourceFactory.IndiaEditSummaryFooter(retVal);
            }

            if (string.Equals(operation, Constants.Operations.Delete, StringComparison.OrdinalIgnoreCase)
                && PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner)
                && string.Equals(country, Constants.CountryCodes.India, StringComparison.OrdinalIgnoreCase)
                && this.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.IndiaExpiryGroupDelete, StringComparer.OrdinalIgnoreCase))
            {
                PIDLResourceFactory.RemoveIndiaDeleteExpiry(retVal);
            }

            var smdMarkets = await this.GetSingleMarkets(country, traceActivityId);

            FeatureContext featureContext = new FeatureContext(
                    country,
                    GetSettingTemplate(partner, setting, Constants.DescriptionTypes.PaymentMethodDescription, type, family),
                    Constants.DescriptionTypes.PaymentMethodDescription,
                    operation,
                    scenario,
                    language,
                    null,
                    this.ExposedFlightFeatures,
                    setting?.Features,
                    family,
                    type,
                    smdMarkets: smdMarkets,
                    originalPartner: partner,
                    isGuestAccount: GuestAccountHelper.IsGuestAccount(this.Request),
                    tokenizationPublicKey: await this.GetTokenizationPublicKey(traceActivityId),
                    tokenizationServiceUrls: this.Settings.TokenizationServiceAccessor.GetTokenizationServiceUrls(),
                    traceActivityId: traceActivityId,
                    sessionId: sessionId,
                    accountId: accountId);

            PostProcessor.Process(retVal, PIDLResourceFactory.FeatureFactory, featureContext);

            // Check for scenario here
            if (string.Equals(scenario, Constants.ScenarioNames.XboxApplyFullPageRender, StringComparison.OrdinalIgnoreCase))
            {
                retVal.First<PIDLResource>()?.RemoveDisplayHintById(Constants.ButtonDisplayHintIds.CancelBackButton);
                retVal.First<PIDLResource>()?.RemoveDisplayHintById(Constants.ButtonDisplayHintIds.CancelButton);
            }

            return retVal;
        }

        /// <summary>
        /// Select Payment Resource (/SelectPaymentResource is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>PaymentMethodDescriptions</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/paymentMethodDescriptions/SelectPaymentResource?country={country}&amp;language={language}&amp;partner={partner}&amp;allowedPaymentMethods={allowedPaymentMethods}&amp;defaultPaymentMethod={defaultPaymentMethod}&amp;operation={operation}&amp;filters={filters}&amp;orderId={orderId}&amp;sessionId={sessionId}&amp;merchantId={merchantId}&amp;currency={currency}&amp;scenario={scenario}&amp;classicProduct={classicProduct}&amp;billableAccountId={billableAccountId};pmGroupPageId={pmGroupPageId}</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="country" required="false" cref="string" in="query">Two letter country code</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <param name="allowedPaymentMethods" required="false" cref="string" in="query">Allowed payment methods</param>
        /// <param name="defaultPaymentMethod" required="false" cref="string" in="query">Default payment methods</param>
        /// <param name="operation" required="false" cref="string" in="query">Operation name</param>
        /// <param name="filters" required="false" cref="string" in="query">Filters for payment methods</param>
        /// <param name="orderId" required="false" cref="string" in="query">Order Id</param>
        /// <param name="sessionId" required="false" cref="string" in="query">Session Id</param>
        /// <param name="merchantId" required="false" cref="string" in="query">Merchant Id</param>
        /// <param name="currency" required="false" cref="string" in="query">Currency value</param>
        /// <param name="scenario" required="false" cref="string" in="query">Scenario name</param>
        /// <param name="classicProduct" required="false" cref="string" in="query">Classic product name</param>
        /// <param name="billableAccountId" required="false" cref="string" in="query">Billable account Id</param>
        /// <param name="pmGroupPageId" required="false" cref="string" in="query">Page Id, controls whether the back button in Add PI page redirects to Select PM page or PM family subpage</param>
        /// <param name="expressCheckoutData" required="false" cref="string" in="query">Stringifyed express checkout data will be used to pre fill the PIDL</param>
        /// <response code="200">List&lt;PIDLResource&gt; for SelectPaymentResource</response>
        /// <returns>A list of PIDLResource</returns>
        //// If the family is not specified, operation must be select, selectInstance, selectSingleInstance or validateInstance
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Needs to be an instance method for Route action selection")]
        [HttpGet]
        public async Task<List<PIDLResource>> SelectPaymentResource(
            string accountId,
            string country = null,
            string language = null,
            string partner = Constants.ServiceDefaults.DefaultPartnerName,
            string allowedPaymentMethods = null,
            string defaultPaymentMethod = null,
            string operation = null,
            string filters = null,
            string orderId = null,
            string sessionId = null,
            string merchantId = null,
            string currency = null,
            string scenario = null,
            string classicProduct = null,
            string billableAccountId = null,
            string pmGroupPageId = null,
            string expressCheckoutData = null)
        {
            string apiName = string.Concat(GlobalConstants.APINames.GetPaymentMethodDescriptions, operation);
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddTracingProperties(accountId, null, null, null, country);
            this.Request.AddPartnerProperty(partner?.ToLower());
            this.Request.AddPidlOperation(operation?.ToLower());

            // Remove it from x-ms-flight to prevent passing to PIMS
            if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.EnablePaymentMethodGrouping))
            {
                this.ExposedFlightFeatures.Add(Constants.PartnerFlightValues.EnablePaymentMethodGrouping);
                this.RemovePartnerFlight(Constants.PartnerFlightValues.EnablePaymentMethodGrouping);

                if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.PXSwapSelectPMPages))
                {
                    this.ExposedFlightFeatures.Add(Constants.PartnerFlightValues.PXSwapSelectPMPages);
                    this.RemovePartnerFlight(Constants.PartnerFlightValues.PXSwapSelectPMPages);
                }
            }

            // Remove EnablePMGroupingSubpageSubmitBlock flight from x-ms-flight to prevent passing to PIMS
            if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.EnablePMGroupingSubpageSubmitBlock))
            {
                this.ExposedFlightFeatures.Add(Constants.PartnerFlightValues.EnablePMGroupingSubpageSubmitBlock);
                this.RemovePartnerFlight(Constants.PartnerFlightValues.EnablePMGroupingSubpageSubmitBlock);
            }

            if (this.IsPartnerFlightExposed(Flighting.Features.PXEnableXboxNativeListPIRewardsPointsDisplay))
            {
                this.ExposedFlightFeatures.Add(Flighting.Features.PXEnableXboxNativeListPIRewardsPointsDisplay);
                this.RemovePartnerFlight(Flighting.Features.PXEnableXboxNativeListPIRewardsPointsDisplay);
            }

            if (this.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.PXEnableXboxCardUpsell, StringComparer.OrdinalIgnoreCase) &&
                operation.Equals(Constants.Operations.Offer, StringComparison.OrdinalIgnoreCase) &&
                PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner))
            {
                // TODO:  Possibly refactor this out of SelectPaymentResource()
                return PIDLResourceFactory.Instance.XboxCardUpsell(language);
            }

            // Use Partner Settings if enabled for the partner
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(operation);
            this.EnableFlightingsInPartnerSetting(setting, country);

            var smdMarkets = await this.GetSingleMarkets(country, traceActivityId);

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
                isGuestAccount: GuestAccountHelper.IsGuestAccount(this.Request),
                smdMarkets: smdMarkets,
                xmsFlightHeader: this.GetPartnerXMSFlightExposed(),
                filters: filters);

            if (PXCommon.Constants.PartnerGroups.IsGroupedSelectPMPartner(partner)
                && this.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.EnablePaymentMethodGrouping, StringComparer.OrdinalIgnoreCase))
            {
                Version fullPidlSdkVersion = HttpRequestHelper.GetFullPidlSdkVersion(this.Request.ToHttpRequestMessage());

                if (fullPidlSdkVersion != null)
                {
                    Version lowestCompatiblePidlVersion = new Version(Constants.PidlSdkVersionNumber.PidlSdkMajor1, Constants.PidlSdkVersionNumber.PidlSdkMinor22, Constants.PidlSdkVersionNumber.PidlSdkBuild3);

                    // Remove the flight if pidl sdk version is less than 1.22.3
                    this.FlightByPidlVersion(fullPidlSdkVersion, lowestCompatiblePidlVersion, Constants.PartnerFlightValues.EnablePaymentMethodGrouping);
                }
                else
                {
                    // Remove the flight to take user through the non-grouped experience
                    this.ExposedFlightFeatures.Remove(Constants.PartnerFlightValues.EnablePaymentMethodGrouping);
                }
            }

            if ((PXCommon.Constants.PartnerGroups.IsVenmoEnabledPartner(partner) || TemplateHelper.IsTemplateBasedPIDLIncludingDefaultTemplate(TemplateHelper.GetSettingTemplate(partner, setting, Constants.DescriptionTypes.PaymentMethodDescription, $"{Constants.PaymentMethodFamily.ewallet}.{Constants.PaymentMethodType.Venmo}")))
                && string.Equals(country, Constants.CountryCodes.UnitedStates, StringComparison.OrdinalIgnoreCase))
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

            if (country == null)
            {
                // We cannot make the country parameter mandatory in the parameters list. If we do that, for incoming requests that have the accountId,
                // family as well as the country, GetByFamilyAndType will match 2 mandatory parameters (accountId and family) and SelectPaymentResource
                // will also match 2 mandatory parameters (accountId and country). The WebAPI routing will error out since it sees multiple actions matching
                // the incoming request.
                throw new Common.ValidationException(ErrorCode.InvalidRequestData, "country is a required parameter when the operation is select");
            }

            if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.IndiaTokenizationMessage) && string.Equals(country, GlobalConstants.CountryCodes.IN, StringComparison.OrdinalIgnoreCase))
            {
                this.ExposedFlightFeatures.Add(Constants.PartnerFlightValues.IndiaTokenizationMessage);
                this.RemovePartnerFlight(Constants.PartnerFlightValues.IndiaTokenizationMessage);
            }

            string actualPuid = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.Puid);

            if (!UseServerSidePrefillPidl(partner, country, setting)
                && UseClientSidePrefillPidl(partner, operation, country, setting)
                && !ExposedFlightFeatures.Contains(Flighting.Features.PXEnableEmpOrgListPI, StringComparer.OrdinalIgnoreCase))
            {
                List<PIDLResource> retVal = PIDLResourceFactory.GetPaymentInsturmentSelectDescriptions(country, language, partner, scenario, classicProduct, billableAccountId, this.ExposedFlightFeatures, setting: setting);
                if (this.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.PXPreventAddNewPaymentMethodDefaultSelection, StringComparer.OrdinalIgnoreCase)
                    || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.PreventAddNewPaymentMethodDefaultSelection, country, setting))
                {
                    await this.PopulatePaymentInstrumentsIntoDataSources(retVal, accountId, traceActivityId, partner, country, language, operation, billableAccountId, allowedPaymentMethods, filters, merchantId, currency, orderId, sessionId, actualPuid, setting);
                }

                foreach (PIDLResource resource in retVal)
                {
                    HashSet<string> includeSet = GetAllowedPaymentMethods(allowedPaymentMethods, traceActivityId);
                    if (includeSet.Contains(PidlFactory.GlobalConstants.PaymentMethodFamilyTypeIds.EwalletLegacyBilldeskPayment, StringComparer.OrdinalIgnoreCase))
                    {
                        resource.AppendDataSourceQueryParam(PidlFactory.GlobalConstants.DataSourceNames.PaymentInstruments, Constants.QueryParameterName.IncludeOneTimeChargeInstrument, bool.TrueString);
                    }

                    if (PXCommonConstants.PartnerGroups.CommercialSMDEnabledPartners.Contains(partner, StringComparer.OrdinalIgnoreCase))
                    {
                        if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.AADSupportSMD))
                        {
                            resource.UpdateDataSourceHeaders(PidlFactory.GlobalConstants.DataSourceNames.PaymentInstruments, GlobalConstants.HeaderValues.ExtendedFlightName, Constants.PartnerFlightValues.AADSupportSMD);
                        }

                        if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.SMDDisabled))
                        {
                            resource.UpdateDataSourceHeaders(PidlFactory.GlobalConstants.DataSourceNames.PaymentInstruments, GlobalConstants.HeaderValues.ExtendedFlightName, Constants.PartnerFlightValues.SMDDisabled);
                        }
                    }
                }

                PostProcessor.Process(retVal, PIDLResourceFactory.FeatureFactory, featureContext);

                return retVal;
            }
            else
            {
                var paymentMethods = await this.GetPaymentMethods(country, language, traceActivityId, partner, operation, merchantId, currency, setting);

                if (!string.IsNullOrEmpty(operation))
                {
                    if (operation.Equals(Constants.Operations.Select, StringComparison.OrdinalIgnoreCase) ||
                        operation.Equals(Constants.Operations.ExpressCheckout, StringComparison.OrdinalIgnoreCase))
                    {
                        if (orderId != null && sessionId != null && actualPuid != null)
                        {
                            paymentMethods = await this.Settings.RiskServiceAccessor.FilterPaymentMethods(actualPuid, partner, orderId, sessionId, paymentMethods, traceActivityId);
                        }

                        HashSet<PaymentMethod> paymentMethodHashSet = new HashSet<PaymentMethod>(paymentMethods);

                        // For commercial stores, PxService requires partner to tell whether certain payment method is allowed.
                        // Add invoice_basic, invoice_check, alipay and unionpay by default and use allowedPaymentMethods to filter the result.
                        if ((string.Equals(partner, Constants.PartnerName.CommercialStores, StringComparison.OrdinalIgnoreCase)
                                && string.Equals(scenario, Constants.ScenarioNames.EligiblePI))
                            || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.EnableVirtualFamilyPM, country, setting))
                        {
                            string invoicePIDisplayName = Constants.VirtualPIDisplayName.Invoice;

                            // Need special treatment for Brazil based on task 27254232
                            if (string.Equals(country, "br", StringComparison.OrdinalIgnoreCase))
                            {
                                invoicePIDisplayName = Constants.VirtualPIDisplayName.InvoiceBR;
                            }

                            paymentMethodHashSet.Add(new PaymentMethod() { PaymentMethodFamily = Constants.PaymentMethodFamilyName.Virtual, PaymentMethodType = Constants.PaymentMethodType.InvoiceBasicVirtual, Properties = new PaymentMethodCapabilities(), Display = new PaymentInstrumentDisplayDetails() { Name = invoicePIDisplayName } });
                            paymentMethodHashSet.Add(new PaymentMethod() { PaymentMethodFamily = Constants.PaymentMethodFamilyName.Virtual, PaymentMethodType = Constants.PaymentMethodType.InvoiceCheckVirtual, Properties = new PaymentMethodCapabilities(), Display = new PaymentInstrumentDisplayDetails() { Name = invoicePIDisplayName } });
                            paymentMethodHashSet.Add(new PaymentMethod() { PaymentMethodFamily = Constants.PaymentMethodFamilyName.Virtual, PaymentMethodType = Constants.PaymentMethodType.AlipayVirtual, Properties = new PaymentMethodCapabilities(), Display = new PaymentInstrumentDisplayDetails() { Name = Constants.VirtualPIDisplayName.Alipay } });
                            paymentMethodHashSet.Add(new PaymentMethod() { PaymentMethodFamily = Constants.PaymentMethodFamilyName.Virtual, PaymentMethodType = Constants.PaymentMethodType.UnionpayVirtual, Properties = new PaymentMethodCapabilities(), Display = new PaymentInstrumentDisplayDetails() { Name = Constants.VirtualPIDisplayName.Unionpay } });
                        }

                        var filteredPMs = PaymentSelectionHelper.GetFilteredPaymentMethods(
                            paymentMethodHashSet,
                            allowedPaymentMethods,
                            filters,
                            operation == null ? null : operation.ToLower(),
                            partner == null ? null : partner.ToLower(),
                            country,
                            setting);

                        List<PIDLResource> retVal = null;
                        if (operation.Equals(Constants.Operations.ExpressCheckout, StringComparison.OrdinalIgnoreCase))
                        {
                            var expressCheckout = new ExpressCheckoutDescription(expressCheckoutData, filteredPMs?.ToList());
                            await expressCheckout.LoadComponentDescription(null, this.Settings, traceActivityId, setting, this.ExposedFlightFeatures, operation: operation, partner: partner, country: country, language: expressCheckout.ExpressCheckoutRequest?.Language, currency: currency, scenario: scenario, request: this.Request.ToHttpRequestMessage());
                            retVal = await expressCheckout.GetDescription();
                        }
                        else
                        {
                            retVal = PIDLResourceFactory.GetPaymentSelectDescriptions(paymentMethodHashSet, country, operation, language, partner, allowedPaymentMethods, defaultPaymentMethod, filters, null, null, this.ExposedFlightFeatures, scenario, setting);
                        }

                        featureContext = new FeatureContext(
                            country,
                            GetSettingTemplate(partner, setting, Constants.DescriptionTypes.PaymentMethodDescription),
                            Constants.DescriptionTypes.PaymentMethodDescription,
                            operation,
                            scenario,
                            language,
                            filteredPMs,
                            this.ExposedFlightFeatures,
                            setting?.Features,
                            originalPartner: partner,
                            pmGroupPageId: pmGroupPageId,
                            isGuestAccount: GuestAccountHelper.IsGuestAccount(this.Request),
                            defaultPaymentMethod: defaultPaymentMethod,
                            xmsFlightHeader: this.GetPartnerXMSFlightExposed());

                        PostProcessor.Process(retVal, PIDLResourceFactory.FeatureFactory, featureContext);

                        return retVal;
                    }
                    else if (operation.Equals(Constants.Operations.SelectInstance, StringComparison.OrdinalIgnoreCase) ||
                        operation.Equals(Constants.Operations.SelectSingleInstance, StringComparison.OrdinalIgnoreCase) ||
                        operation.Equals(Constants.Operations.ValidateInstance, StringComparison.OrdinalIgnoreCase))
                    {
                        HashSet<PaymentMethod> paymentMethodHashSet = new HashSet<PaymentMethod>(paymentMethods);
                        List<PaymentInstrument> disabledPaymentInstruments = new List<PaymentInstrument>();
                        List<PaymentInstrument> paymentInstrumentList = await this.GetPaymentInstruments(accountId, traceActivityId, partner, country, language, operation, billableAccountId, orderId, sessionId, actualPuid, setting: setting);

                        if (string.Equals(partner, Constants.PartnerName.XboxSettings, StringComparison.OrdinalIgnoreCase) &&
                            ExposedFlightFeatures.Contains(Flighting.Features.PXEnableXboxNativeListPIRewardsPointsDisplay))
                        {
                            UpdateCurrencyBalanceText(paymentInstrumentList, country, language);
                        }

                        List<PIDLResource> retVal = PIDLResourceFactory.GetPaymentSelectDescriptions(paymentMethodHashSet, country, operation, language, partner, allowedPaymentMethods, null, filters, paymentInstrumentList, disabledPaymentInstruments, exposedFlightFeatures: this.ExposedFlightFeatures, scenario, setting: setting);

                        featureContext.PaymentInstruments = paymentInstrumentList;
                        featureContext.DisabledPaymentInstruments = disabledPaymentInstruments;
                        PostProcessor.Process(retVal, PIDLResourceFactory.FeatureFactory, featureContext);

                        return retVal;
                    }
                }
            }

            throw new InvalidOperationException("Parameter operation is not in the list of supported operation types");
        }

        /// <summary>
        /// Get Payment Method Descriptions by Family and Type with complete prerequisites ("/GetByFamilyAndTypeWithCompletePrerequisitesOption" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>PaymentMethodDescriptions</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/paymentMethodDescriptions/GetByFamilyAndTypeWithCompletePrerequisitesOption?completePrerequisites={completePrerequisites}&amp;ignoreMissingTaxId={ignoreMissingTaxId}&amp;family={family}&amp;type={type}&amp;country={country}&amp;language={language}&amp;partner={partner}&amp;operation={operation}&amp;scenario={scenario}&amp;classicProduct={classicProduct}&amp;billableAccountId={billableAccountId}</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="country" required="false" cref="string" in="query">Two letter country code</param>
        /// <param name="family" required="true" cref="string" in="query">Payment method family name</param>
        /// <param name="completePrerequisites" required="true" cref="bool" in="query">Bool value to indicate whether to complete prerequisites</param>
        /// <param name="ignoreMissingTaxId" required="false" cref="bool" in="query">Bool value to indiciate whether ignore missing tax id</param>
        /// <param name="type" required="false" cref="string" in="query">Payment method type name</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <param name="operation" required="false" cref="string" in="query">Operation name</param>
        /// <param name="classicProduct" required="false" cref="string" in="query">Classic product name</param>
        /// <param name="billableAccountId" required="false" cref="string" in="query">Billable account Id</param>
        /// <param name="scenario" required="false" cref="string" in="query">Scenario name</param>
        /// <param name="orderId" required="false" cref="string" in="query">Order Id</param>
        /// <param name="channel" required="false" cref="string" in="query">Channel name used for apply</param>
        /// <param name="referrerId" required="false" cref="string" in="query">ReferrerId name used for apply</param>
        /// <param name="sessionId" required="false" cref="string" in="query">SessionId used for apply</param>
        /// <response code="200">List&lt;PIDLResource&gt; for get payment method descriptions by family and type with complete prerequisites</response>
        /// <returns>A list of PIDLResource</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Needs to be an instance method for Route action selection")]
        [HttpGet]
        public async Task<List<PIDLResource>> GetByFamilyAndTypeWithCompletePrerequisitesOption(
            string accountId,
            string country,
            string family,
            bool completePrerequisites,
            bool ignoreMissingTaxId = false,
            string type = null,
            string language = null,
            string partner = Constants.ServiceDefaults.DefaultPartnerName,
            string operation = Constants.ServiceDefaults.DefaultOperationType,
            string classicProduct = null,
            string billableAccountId = null,
            string scenario = null,
            string orderId = null,
            string channel = null,
            string referrerId = null,
            string sessionId = null)
        {
            if ((PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner)
                || (Constants.PartnersToEnablePaypalSecondScreenForXbox.Contains(partner, StringComparer.OrdinalIgnoreCase)
                    && (this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnablePaypalSecondScreenForXbox, StringComparer.OrdinalIgnoreCase)
                        || Constants.CountriesToEnablePaypalSecondScreenForXbox.Contains(country, StringComparer.OrdinalIgnoreCase))))
                && string.Equals(family, Constants.PaymentMethodFamily.ewallet.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(type, Constants.PaymentMethodType.PayPal, StringComparison.OrdinalIgnoreCase)
                && string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase))
            {
                scenario = Constants.ScenarioNames.PaypalQrCode;
            }

            // Use Partner Settings if enabled for the partner
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(operation);

            if ((PXCommon.Constants.PartnerGroups.IsVenmoEnabledPartner(partner) || TemplateHelper.IsTemplateBasedPIDLIncludingDefaultTemplate(TemplateHelper.GetSettingTemplate(partner, setting, Constants.DescriptionTypes.PaymentMethodDescription, $"{Constants.PaymentMethodFamily.ewallet}.{Constants.PaymentMethodType.Venmo}")))
                && string.Equals(family, Constants.PaymentMethodFamily.ewallet.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(type, Constants.PaymentMethodType.Venmo, StringComparison.OrdinalIgnoreCase)
                && string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase)
                && string.Equals(country, Constants.CountryCodes.UnitedStates, StringComparison.OrdinalIgnoreCase))
            {
                if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner))
                {
                    scenario = Constants.ScenarioNames.VenmoQRCode;
                }

                if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.PxEnableVenmo))
                {
                    this.ExposedFlightFeatures.Add(Constants.PartnerFlightValues.PxEnableVenmo);
                }

                if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.PxEnableSelectPMAddPIVenmo))
                {
                    this.ExposedFlightFeatures.Add(Constants.PartnerFlightValues.PxEnableSelectPMAddPIVenmo);
                }
            }

            // To show AVS suggestions, PX has to enable in the flight and partners has to send the flight as well
            if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableAVSSuggestions, StringComparer.OrdinalIgnoreCase))
            {
                // Pass ShowAVSSuggestions
                // Remove it from x-ms-flight to prevent passing to PIMS
                if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.ShowAVSSuggestions))
                {
                    this.ExposedFlightFeatures.Add(Constants.PartnerFlightValues.ShowAVSSuggestions);
                    this.RemovePartnerFlight(Constants.PartnerFlightValues.ShowAVSSuggestions);

                    if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.EnableAVSAddtionalFlags))
                    {
                        this.ExposedFlightFeatures.Add(Constants.PartnerFlightValues.EnableAVSAddtionalFlags);
                        this.RemovePartnerFlight(Constants.PartnerFlightValues.EnableAVSAddtionalFlags);
                    }
                }

                // Pass ShowAVSSuggestionsModal
                // Remove it from x-ms-flight to prevent passing to PIMS
                if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.ShowAVSSuggestionsModal))
                {
                    this.ExposedFlightFeatures.Add(Constants.PartnerFlightValues.ShowAVSSuggestions);
                    this.ExposedFlightFeatures.Add(Flighting.Features.TradeAVSUsePidlModalInsteadofPidlPage);
                    this.RemovePartnerFlight(Constants.PartnerFlightValues.ShowAVSSuggestionsModal);
                }
            }

            this.AddOrRemovePartnerFlight(country, Constants.PartnerFlightValues.IndiaTokenizationConsentCapture, GlobalConstants.CountryCodes.IN);

            if (string.Equals(country, GlobalConstants.CountryCodes.IN, StringComparison.OrdinalIgnoreCase))
            {
                if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.IndiaExpiryGroupDelete))
                {
                    this.ExposedFlightFeatures.Add(Constants.PartnerFlightValues.IndiaExpiryGroupDelete);
                    this.RemovePartnerFlight(Constants.PartnerFlightValues.IndiaExpiryGroupDelete);
                }
            }
            else
            {
                this.ExposedFlightFeatures.Remove(Constants.PartnerFlightValues.IndiaExpiryGroupDelete);
            }

            // Only keep the exposed flights if the pidlsdkversion is greater than a specified version in order for styling changes to line up.
            Version fullPidlSdkVersion = HttpRequestHelper.GetFullPidlSdkVersion(this.Request.ToHttpRequestMessage());

            if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner) && fullPidlSdkVersion != null)
            {
                Version lowestCompatiblePidlVersion = new Version(Constants.PidlSdkVersionNumber.PidlSdkMajor1, Constants.PidlSdkVersionNumber.PidlSdkMinor22, Constants.PidlSdkVersionNumber.PidlSdkBuild0, Constants.PidlSdkVersionNumber.PidlSdkAlpha144);

                this.FlightByPidlVersion(fullPidlSdkVersion, lowestCompatiblePidlVersion, Constants.PartnerFlightValues.ShowSummaryPage);

                this.FlightByPidlVersion(fullPidlSdkVersion, lowestCompatiblePidlVersion, Constants.PartnerFlightValues.ShowAVSSuggestions);
            }

            // Remove any unsuppoted flight features based on client version.
            RemoveUnsupportedFlightFeatures(this.ExposedFlightFeatures, this.PidlSdkVersion);

            // Enable flighting based on the setting patner
            this.EnableFlightingsInPartnerSetting(setting, country);

            List<PIDLResource> retVal = null;

            string apiName = string.Concat(GlobalConstants.APINames.GetPaymentMethodDescriptions, operation);
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddTracingProperties(accountId, null, family, type, country);
            this.Request.AddPartnerProperty(partner?.ToLower());
            this.Request.AddPidlOperation(operation?.ToLower());

            if (SkipAddPIForPaymentMethod(family, type, operation))
            {
                return await this.ReturnPIForPaymentMethodCanSkipAddPI(family, type, operation, accountId, traceActivityId, classicProduct, billableAccountId, partner, country);
            }

            if (this.IsInstancePIEnabled(type, family))
            {
                return await this.CreateInstancePI(accountId, country, type, classicProduct, billableAccountId, operation, traceActivityId, partner);
            }

            // For global PIs/PMs (e.g. paysafecard, sofort), as add PI is not needed, PX will return a restAction clientAction to PIDLSDK to get the PI details through paymentInstrumentsEx endpoint, and PIDLSDK will return PI details in the success event.
            if (this.IsGlobalPIInAddResourceEnabled(partner, operation, country, setting))
            {
                retVal = await this.ReturnRestActionToGetPIForGlobalPI(accountId, partner, country, language, family, type, traceActivityId, operation, setting);
                if (retVal != null)
                {
                    return retVal;
                }
            }

            // Prevent address reduction on non-cc PI
            // TODO: As part of T-54279751 investigation, the AddressNoCityState flow found to be not used by any partner
            // And below code & csv config related to it can be removed
            if (string.Equals(scenario, Constants.ScenarioNames.AddressNoCityState, StringComparison.OrdinalIgnoreCase)
                && (!string.Equals(family, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase) || !string.Equals(country, Constants.CountryCodes.UnitedStates, StringComparison.OrdinalIgnoreCase) || !string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase)))
            {
                // TODO: T-54217106: RS5 scenario is deprecating and not required to be added in templates
                if (string.Equals(partner, Constants.PartnerName.OfficeOobe, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(partner, Constants.PartnerName.OXOOobe, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(partner, Constants.PartnerName.SmbOobe, StringComparison.OrdinalIgnoreCase))
                {
                    scenario = Constants.ScenarioNames.RS5;
                }
                else
                {
                    scenario = null;
                }
            }

            var smdMarkets = await this.GetSingleMarkets(country, traceActivityId);
            FeatureContext featureContext = new FeatureContext(
                country,
                partner,
                Constants.DescriptionTypes.PaymentMethodDescription,
                operation,
                scenario,
                language,
                null,
                this.ExposedFlightFeatures,
                setting?.Features,
                family,
                type,
                smdMarkets: smdMarkets,
                originalPartner: partner,
                isGuestAccount: GuestAccountHelper.IsGuestAccount(this.Request),
                tokenizationPublicKey: await this.GetTokenizationPublicKey(traceActivityId),
                tokenizationServiceUrls: this.Settings.TokenizationServiceAccessor.GetTokenizationServiceUrls(),
                traceActivityId: traceActivityId,
                accountId: accountId,
                xmsFlightHeader: this.GetPartnerXMSFlightExposed());

            // If the partner send x-ms-billing-account-id header, then we should skip complete prerequisites            
            bool isMyBaUser = false;
            List<string> billingAccountContext = GetBillingAccountContext();
            if (billingAccountContext != null)
            {
                // This flag is used down the flow to get payment methods from PIMS instead static values.
                isMyBaUser = true;

                // If the partner sends x-ms-billing-account-id header for SEPA, then we should skip complete prerequisites
                // This is temporary fix to avoid the issue from profile creation flow.
                completePrerequisites = false;
            }

            if (completePrerequisites)
            {
                // Pass ShowMiddleName to PidlFactory
                // Remove it from x-ms-flight to prevent passing to PIMS
                if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.ShowMiddleName))
                {
                    this.ExposedFlightFeatures.Add(Constants.PartnerFlightValues.ShowMiddleName);
                    this.RemovePartnerFlight(Constants.PartnerFlightValues.ShowMiddleName);
                }

                string profileType = this.GetProfileType();
                if (string.Equals(profileType, GlobalConstants.ProfileTypes.Consumer, StringComparison.InvariantCultureIgnoreCase))
                {
                    AccountProfile profile = null;
                    AccountProfileV3 profileV3 = null;
                    bool useJarvisV3 = GuestAccountHelper.IsGuestAccount(this.Request) || this.ExposedFlightFeatures.Contains(Flighting.Features.PXUseJarvisV3ForCompletePrerequisites, StringComparer.OrdinalIgnoreCase);

                    if (useJarvisV3)
                    {
                        profileV3 = await this.Settings.AccountServiceAccessor.GetProfileV3(accountId, profileType, traceActivityId);
                    }
                    else
                    {
                        profile = await this.Settings.AccountServiceAccessor.GetProfile(accountId, profileType, traceActivityId);
                    }

                    object[] taxIds = null;
                    if (!ignoreMissingTaxId && this.IsCountryEnabledTaxIdInConsumerFlow(country))
                    {
                        taxIds = await this.Settings.TaxIdServiceAccessor.GetTaxIds(accountId, traceActivityId);
                    }

                    // Check whether profile page is needed for the user
                    if ((useJarvisV3 && this.ShouldReturnProfileDescription(profileV3, taxIds, country, ignoreMissingTaxId)) || (!useJarvisV3 && this.ShouldReturnProfileDescription(profile, taxIds, country, ignoreMissingTaxId)))
                    {
                        // Setup nextPidlUrl
                        string baseUrl = @"https://{pifd-endpoint}/users/{userId}/paymentMethodDescriptions";
                        bool requiresTaxIds = this.RequiresTaxIds(taxIds, country, ignoreMissingTaxId);
                        var queryParams = this.Request.Query.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());

                        if (requiresTaxIds)
                        {
                            // Mark ignoreMissingTaxId to be true in the nextPidlUrl to ensure the CPF(TaxId) field only shown once to the user,
                            // no matter user enters the CPF or not
                            queryParams["ignoreMissingTaxId"] = "true";
                        }

                        string nextPidlUrl = QueryHelpers.AddQueryString(baseUrl, queryParams);
                        PXCommon.RestLink nextPidlLink = new PXCommon.RestLink() { Href = nextPidlUrl, Method = HttpMethod.Get.ToString() };

                        // Get Profile PIDL
                        if (useJarvisV3)
                        {
                            // V3 configs for PIDL/Submit links for Webblends and Cart Partner
                            // TODO: 48926388 - Remove partner check once all partners are migrated to V3
                            if (string.Equals(partner, Constants.PartnerName.Webblends, StringComparison.OrdinalIgnoreCase)
                                || string.Equals(partner, Constants.PartnerName.Cart, StringComparison.OrdinalIgnoreCase)
                                || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseProfilePrerequisitesV3, country, setting))
                            {
                                string profileOperation = Constants.Operations.Add;
                                Dictionary<string, string> profileV3Headers = null;

                                if (profileV3 != null)
                                {
                                    profileOperation = Constants.Operations.Update;
                                    profileV3Headers = new Dictionary<string, string>
                                    {
                                        { AccountV3Headers.Etag, profileV3.Etag },
                                        { AccountV3Headers.IfMatch, profileV3.Etag }
                                    };
                                }

                                retVal = PIDLResourceFactory.Instance.GetProfileDescriptions(country, profileType + "PrerequisitesV3", profileOperation, language, partner, nextPidlLink, profileV3 != null ? profileV3.Id : null, profileV3Headers, true, this.ExposedFlightFeatures, scenario, setting: setting);
                            }
                            else
                            {
                                retVal = PIDLResourceFactory.Instance.GetProfileDescriptions(country, profileType + "Prerequisites", profileV3 != null ? Constants.Operations.Update : Constants.Operations.Add, language, partner, nextPidlLink, profileV3 != null ? profileV3.Id : null, null, false, this.ExposedFlightFeatures, scenario, setting: setting);
                            }
                        }
                        else
                        {
                            retVal = PIDLResourceFactory.Instance.GetProfileDescriptions(country, profileType + "Prerequisites", profile != null ? Constants.Operations.Update : Constants.Operations.Add, language, partner, nextPidlLink, profile != null ? profile.Id : null, null, false, this.ExposedFlightFeatures, scenario, setting: setting);
                        }

                        retVal.ForEach(r => r.MakeSecondaryResource());

                        // Prefill User Data
                        await this.PrefillUserData(retVal, accountId, country, partner, traceActivityId, completePrerequisites: completePrerequisites, billableAccountId: billableAccountId);

                        // TODO: Add this logic to CSV which includes country, pi, operation with scenario and tax pidl id
                        // Add TaxId PIDL if necessary
                        if (requiresTaxIds)
                        {
                            List<PIDLResource> taxIdPidl = PIDLResourceFactory.Instance.GetTaxIdDescriptions(country, Constants.TaxIdTypes.Consumer, language, partner, profileType, setting: setting);
                            taxIdPidl.ForEach(r => r.MakeSecondaryResource());
                            if (retVal != null && taxIdPidl.Count > 0)
                            {
                                PIDLResourceFactory.AddLinkedPidlToResourceList(retVal, taxIdPidl[0], partner);
                            }
                            else
                            {
                                retVal = taxIdPidl;
                            }
                        }
                    }

                    string updatedPartner = string.Empty;

                    // Show PI PIDL if prerequisites are all completed
                    if (retVal == null)
                    {
                        if (this.IsNorthStarExperiencePartnerAndScenario(partner, scenario))
                        {
                            retVal = await this.GetNorthstarPaymentMethodDescriptions(traceActivityId, accountId, country, family, type, language, partner, operation, classicProduct, billableAccountId, completePrerequisites, null, scenario, setting: setting);
                        }
                        else if (this.IsConsoleAddCCQrCodeFlow(this.ExposedFlightFeatures, scenario, fullPidlSdkVersion, retVal))
                        {
                            sessionId = await this.GenerateSessionIdAsync(accountId, country, partner, operation, language, traceActivityId);
                            updatedPartner = Constants.TemplateName.ConsoleTemplate; // TODO: Remove once PSS integrates xbox partners https://microsoft.visualstudio.com/OSGS/_workitems/edit/54303371
                            retVal = await this.GetPaymentMethodDescriptionsFromFactory(traceActivityId, accountId, country, family, type, language, updatedPartner, operation, classicProduct, billableAccountId, completePrerequisites, null, scenario, orderId, channel, referrerId, sessionId, this.ExposedFlightFeatures, setting: setting);

                            if (bool.Equals(retVal?.First().DisplayPages.First().HintId.Equals(Constants.DisplayHintIds.ProfilePrerequisitesPage), false))
                            {
                                PIDLResourceFactory.AddCCQrCodeInAddConsole(ref retVal, scenario, Constants.ChallengeTypes.CreditCardQrCode, Constants.TemplateName.ConsoleTemplate, type, family, language, country, this.ExposedFlightFeatures, sessionId, accountId, partner);
                            }
                        }
                        else
                        {
                            retVal = await this.GetPaymentMethodDescriptionsFromFactory(
                                traceActivityId,
                                accountId,
                                country,
                                family,
                                type,
                                language,
                                partner,
                                operation,
                                classicProduct,
                                billableAccountId,
                                completePrerequisites,
                                null,
                                scenario,
                                orderId,
                                channel,
                                referrerId,
                                sessionId,
                                this.ExposedFlightFeatures,
                                setting: setting);
                            if (ShouldUpdateTokenizationConsentPropertyDetails(family, country, operation, partner, this.ExposedFlightFeatures, setting, type))
                            {
                                UpdateTokenizationConsentPropertyDetails(retVal);
                            }

                            if (ShouldRemoveExpiryDateForTemplates(setting, family, type, partner, country, operation))
                            {
                                TryRemoveExpiryMonthAndYearFromPIDL(retVal);
                            }

                            // TODO: Remove the if block once all the partners in ShouldRemoveExpiryDateForIndiaCommercial are moved to use the template
                            // This block is enabled by default for templates in above ShouldRemoveExpiryDateForTemplates.
                            if (ShouldRemoveExpiryDateForIndiaCommercial(operation, country, partner, this.ExposedFlightFeatures))
                            {
                                TryRemoveExpiryMonthAndYearFromPIDL(retVal);
                            }

                            if (ShouldRemoveExpiryDateForIndiaConsumer(operation, country, partner, this.ExposedFlightFeatures))
                            {
                                if (!(PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner)
                                    && string.Equals(operation, Constants.Operations.Update, StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    TryRemoveExpiryMonthAndYearFromPIDL(retVal, partner, scenario);
                                }
                            }
                        }

                        AddressInfo address = null;
                        AddressInfoV3 addressV3 = null;
                        bool overrideJarvisVersionToV3 = false;
                        string addressType = string.Empty;

                        if (useJarvisV3)
                        {
                            addressV3 = await this.ReturnDefaultAddressV3ByCountry(accountId, profileV3, profileType, country, traceActivityId);
                            overrideJarvisVersionToV3 = true;
                        }
                        else
                        {
                            address = await this.ReturnDefaultAddressByCountry(accountId, profile, country, traceActivityId);
                        }

                        if ((useJarvisV3 && addressV3 == null) || (!useJarvisV3 && address == null))
                        {
                            foreach (PIDLResource pidl in retVal)
                            {
                                // TODO: As part of T-54279751 investigation, the AddressNoCityState flow found to be not used by any partner
                                // And below code & csv config related to it can be removed
                                if (ContainsAddressInPIPidl(pidl) && !IsAddressNoCityStateConfiguration(pidl, scenario, country, family))
                                {
                                    if ((string.Equals(partner, Constants.PartnerName.OXOWebDirect, StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(partner, Constants.PartnerName.Webblends, StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(partner, Constants.PartnerName.Cart, StringComparison.OrdinalIgnoreCase))
                                        && (this.ExposedFlightFeatures != null && this.ExposedFlightFeatures.Contains(Flighting.Features.PXUseShippingV3ForCompletePrerequisites, StringComparer.OrdinalIgnoreCase)))
                                    {
                                        addressType = Constants.AddressTypes.ShippingV3;
                                    }
                                    else
                                    {
                                        addressType = Constants.AddressTypes.Billing;
                                    }

                                    List<PIDLResource> profileAddressPidls = PIDLResourceFactory.Instance.GetAddressDescriptions(country, addressType, language, partner, "profileAddressAutoSubmit", overrideJarvisVersionToV3, scenario, this.ExposedFlightFeatures, setting: setting);

                                    profileAddressPidls.ForEach(r => r.MakeSecondaryResource());

                                    if (profileAddressPidls.Count > 0)
                                    {
                                        profileAddressPidls[0].SetErrorHandlingToIgnore();

                                        if (useJarvisV3)
                                        {
                                            Dictionary<string, string> profileV3Headers = new Dictionary<string, string>();
                                            profileV3Headers.Add(AccountV3Headers.Etag, profileV3.Etag);
                                            profileV3Headers.Add(AccountV3Headers.IfMatch, profileV3.Etag);

                                            PIDLResourceFactory.AddSecondarySubmitAddressV3Context(profileAddressPidls, profileV3, partner, profileV3Headers, country: country, setting: setting);
                                        }
                                        else
                                        {
                                            PIDLResourceFactory.AddSecondarySubmitAddressContext(profileAddressPidls, profile, partner, country: country, setting: setting);
                                        }

                                        PIDLResourceFactory.AddLinkedPidlToResource(pidl, profileAddressPidls[0], partner, PidlContainerDisplayHint.SubmissionOrder.BeforeBase);
                                        PIDLResourceFactory.ShowNoProfileAddressToResource(retVal);
                                    }
                                }
                                else if (IsAddressNoCityStateConfiguration(pidl, scenario, country, family))
                                {
                                    // TODO: As part of T-54279751 investigation, the AddressNoCityState flow found to be not used by any partner
                                    // And code & csv config related to it can be removed
                                    PIDLResourceFactory.ShowNoProfileAddressToResource(retVal);
                                }
                            }
                        }
                    }
                }
                else if (string.Equals(profileType, GlobalConstants.ProfileTypes.Employee, StringComparison.InvariantCultureIgnoreCase))
                {
                    AccountEmployeeProfileV3 profile = await this.Settings.AccountServiceAccessor.GetProfileV3(accountId, profileType, traceActivityId) as AccountEmployeeProfileV3;
                    AddressInfoV3 address = null;

                    if (profile != null && !string.IsNullOrEmpty(profile.DefaultAddressId))
                    {
                        address = await this.ReturnDefaultAddressV3ByCountry(accountId, profile, profileType, country, traceActivityId);
                    }

                    string profileCountry = country;
                    bool smdCommercialEnabled = this.IsSmdCommercialEnabled(partner, family, operation) || this.IsSingleMarketDirectiveEnabled(operation, family, country, setting);
                    if (smdCommercialEnabled)
                    {
                        // Reducing the scope of this for only when Commercial SMD is enabled
                        address = await this.TryGetDefaultAddressV3(accountId, profile, traceActivityId);
                        profileCountry = address?.Country ?? this.GetOriginCountry() ?? country;
                    }
                    else if (this.ExposedFlightFeatures != null && this.ExposedFlightFeatures.Contains(Flighting.Features.PXSetDefaultAddressCountryForProfileUpdatePartial, StringComparer.OrdinalIgnoreCase))
                    {
                        address = await this.TryGetDefaultAddressV3(accountId, profile, traceActivityId);
                        profileCountry = address?.Country ?? country;
                    }

                    if (string.Equals(scenario, Constants.ScenarioNames.DepartmentalPurchase, StringComparison.OrdinalIgnoreCase))
                    {
                        bool isAddressValid = true;

                        // Legacy AVS address validation for profile.default_address
                        if (address != null)
                        {
                            try
                            {
                                object result = await this.Settings.AccountServiceAccessor.LegacyValidateAddress(address, traceActivityId);
                                if (!string.Equals((string)result, "Valid", System.StringComparison.OrdinalIgnoreCase))
                                {
                                    isAddressValid = false;
                                }
                            }
                            catch
                            {
                                isAddressValid = false;
                            }
                        }

                        List<PIDLResource> paymentMethodPidls = await this.GetPaymentMethodDescriptionsFromFactory(
                                    traceActivityId: traceActivityId,
                                    accountId: accountId,
                                    country: country,
                                    family: family,
                                    type: type,
                                    language: language,
                                    partner: partner,
                                    operation: operation,
                                    classicProduct: classicProduct,
                                    billableAccountId: billableAccountId,
                                    completePrerequisites: completePrerequisites,
                                    piid: null,
                                    scenario: scenario,
                                    setting: setting);

                        if (profile == null || address == null || InvalidEmployeeName(address.FirstName) || InvalidEmployeeName(address.LastName) || !isAddressValid)
                        {
                            string profileOperation = profile == null ? Constants.Operations.AddPartial : Constants.Operations.UpdatePartial;
                            if (profile != null && address != null && !InvalidEmployeeName(address.FirstName) && !InvalidEmployeeName(address.LastName))
                            {
                                scenario += "AddressOnly";
                            }

                            var profileV3Headers = new Dictionary<string, string>();
                            string profileId = null;
                            if (string.Equals(profileOperation, Constants.Operations.UpdatePartial, StringComparison.OrdinalIgnoreCase))
                            {
                                profileId = profile.Id;
                                profileV3Headers.Add(AccountV3Headers.Etag, profile.Etag);
                                profileV3Headers.Add(AccountV3Headers.IfMatch, profile.Etag);
                            }

                            PIDLResource profilePidl = PIDLResourceFactory.Instance.GetProfileDescriptions(
                                    country: country,
                                    type: profileType,
                                    operation: profileOperation,
                                    language: language,
                                    partnerName: partner,
                                    nextPidlLink: null,
                                    profileId: profileId,
                                    profileV3Headers: profileV3Headers,
                                    overrideJarvisVersionToV3: setting?.Template != null ? true : false,
                                    exposedFlightFeatures: this.ExposedFlightFeatures,
                                    scenario: scenario,
                                    setting: setting).First();

                            string addressPath = "default_address";
                            string[] addressProperties = { "address_line1", "address_line2", "address_line3", "city", "region", "postal_code", "country" };
                            string[] nameProperties = { "first_name", "last_name", "middle_name" };

                            // If address passes legacy address validation, remove address part from payload to prevent overwriting profile's default address
                            // Else if address does not pass validation and first name/last name is valid, remove first name/last name from the payload
                            // Else keep both address and first name/last name in payload
                            if (address != null && isAddressValid)
                            {
                                PIDLResourceFactory.RemoveDataDescriptionWithFullPath(profilePidl, addressPath, addressProperties);
                                ProxyController.UpdateIsOptionalPropertyWithFullPath(profilePidl, Constants.PropertiesToMakeMandatory.ToArray(), false);
                            }
                            else if (address != null && !InvalidEmployeeName(address.FirstName) && !InvalidEmployeeName(address.LastName))
                            {
                                PIDLResourceFactory.RemoveDataDescriptionWithFullPath(profilePidl, addressPath, nameProperties);
                            }
                            else
                            {
                                ProxyController.UpdateIsOptionalPropertyWithFullPath(profilePidl, Constants.PropertiesToMakeMandatory.ToArray(), false);
                            }

                            profilePidl.RemoveDataSource();
                            profilePidl.MakeSecondaryResource();
                            if (!this.ExposedFlightFeatures.Contains(Flighting.Features.PXThrowOnLinkedProfilePidlErrors, StringComparer.OrdinalIgnoreCase)
                                || profile != null)
                            {
                                profilePidl.SetErrorHandlingToIgnore();
                            }

                            if (smdCommercialEnabled && !string.Equals(profileCountry, country, StringComparison.OrdinalIgnoreCase))
                            {
                                UpdateProfileAddressDataDescription(profilePidl, profileCountry, country, this.ExposedFlightFeatures, setting);
                            }

                            foreach (PIDLResource paymentMethodPidl in paymentMethodPidls)
                            {
                                PIDLResourceFactory.AddLinkedPidlToResource(paymentMethodPidl, profilePidl, partner, PidlContainerDisplayHint.SubmissionOrder.BeforeBase);

                                // Add modern Validate for commercial partners when linkedPidl is present in PIDL
                                if (this.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.ShowAVSSuggestions, StringComparer.OrdinalIgnoreCase))
                                {
                                    this.AddModernValidationActionForPaymentMethodDescription(paymentMethodPidl, "address", Constants.AddressTypes.Internal, partner, language, country);

                                    // Add two additional flags to linked profile pidl in dp scenario
                                    if (this.ExposedFlightFeatures.Contains(
                                            Constants.PartnerFlightValues.EnableAVSAddtionalFlags,
                                            StringComparer.OrdinalIgnoreCase))
                                    {
                                        EnableAVSAdditionalFlagsToLinkedJarvisProfile(paymentMethodPidl);
                                    }
                                }
                            }

                            retVal = paymentMethodPidls;
                            PIDLResourceFactory.RemoveEmptyPidlContainerHints(retVal);

                            if (ShouldUpdateTokenizationConsentPropertyDetails(family, country, operation, partner, this.ExposedFlightFeatures, setting, type))
                            {
                                UpdateTokenizationConsentPropertyDetails(retVal);
                            }

                            PostProcessor.Process(retVal, PIDLResourceFactory.FeatureFactory, featureContext);

                            return retVal;
                        }

                        retVal = paymentMethodPidls;

                        // Add modern Validate for azure and commercialstores partners
                        this.AddModernAVSValidationAction(retVal, partner, family, type, operation, language, country);

                        PIDLResourceFactory.RemoveEmptyPidlContainerHints(retVal);

                        if (ShouldUpdateTokenizationConsentPropertyDetails(family, country, operation, partner, this.ExposedFlightFeatures, setting, type))
                        {
                            UpdateTokenizationConsentPropertyDetails(retVal);
                        }

                        PostProcessor.Process(retVal, PIDLResourceFactory.FeatureFactory, featureContext);

                        return retVal;
                    }

                    if (profile == null || string.IsNullOrEmpty(profile.Email) || address == null || string.IsNullOrEmpty(address.FirstName) || string.IsNullOrEmpty(address.LastName))
                    {
                        // MAC Prereq
                        // Show Add PI page with linked profile pidl instead of prereq page
                        // No profile fields shown on UI, pidlsdk is responsible for prefilling the data
                        if (Constants.HiddenLinkedProfilePIDLInAddCCPartners.Contains(partner, StringComparer.OrdinalIgnoreCase) || !string.IsNullOrEmpty(setting?.Template))
                        {
                            if (profile == null
                                && string.Equals(operation, Constants.Operations.Add, StringComparison.InvariantCultureIgnoreCase)
                                && (string.Equals(family, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.InvariantCultureIgnoreCase)
                                    || string.Equals(type, Constants.PaymentMethodType.Ach, StringComparison.InvariantCultureIgnoreCase)
                                    || string.Equals(type, Constants.PaymentMethodType.Sepa, StringComparison.InvariantCultureIgnoreCase)))
                            {
                                List<PIDLResource> paymentMethodPidls = await this.GetPaymentMethodDescriptionsFromFactory(
                                    traceActivityId: traceActivityId,
                                    accountId: accountId,
                                    country: country,
                                    family: family,
                                    type: type,
                                    language: language,
                                    partner: partner,
                                    operation: operation,
                                    classicProduct: classicProduct,
                                    billableAccountId: billableAccountId,
                                    completePrerequisites: completePrerequisites,
                                    piid: null,
                                    scenario: scenario,
                                    setting: setting,
                                    exposedFlightFeatures: this.ExposedFlightFeatures);

                                PIDLResource profilePidl = null;

                                // Move pidl container to above cardholderName (index 5 by default, index 1 for smboobe, index 2 for smboobe with the roobe scenario)
                                foreach (PIDLResource pidl in paymentMethodPidls)
                                {
                                    foreach (PageDisplayHint displayPage in pidl.DisplayPages)
                                    {
                                        int index = displayPage.Members.FindIndex(hint => hint.HintId == "pidlContainer");
                                        if (index != -1)
                                        {
                                            var pidlContainer = displayPage.Members[index];
                                            displayPage.Members.RemoveAt(index);

                                            if (string.Equals(partner, Constants.PartnerName.SmbOobe, StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                displayPage.Members.Insert(string.Equals(scenario, Constants.ScenarioNames.Roobe, StringComparison.InvariantCultureIgnoreCase) ? 2 : 1, pidlContainer);
                                            }
                                            else
                                            {
                                                displayPage.Members.Insert(5, pidlContainer);
                                            }
                                        }
                                    }
                                }

                                profilePidl = PIDLResourceFactory.Instance.GetProfileDescriptions(
                                    country: country,
                                    type: profileType,
                                    operation: Constants.Operations.Add,
                                    language: language,
                                    partnerName: partner,
                                    nextPidlLink: null,
                                    profileId: null,
                                    profileV3Headers: null,
                                    overrideJarvisVersionToV3: setting?.Template != null ? true : false,
                                    exposedFlightFeatures: this.ExposedFlightFeatures,
                                    scenario: string.Equals(
                                        scenario, Constants.ScenarioNames.Roobe, StringComparison.InvariantCultureIgnoreCase) ?
                                        Constants.ScenarioNames.HiddenProfileWithNameRoobe :
                                        Constants.ScenarioNames.HiddenProfileWithName,
                                    setting: setting).First();

                                profilePidl.MakeSecondaryResource();

                                if (!this.ExposedFlightFeatures.Contains(Flighting.Features.PXThrowOnLinkedProfilePidlErrors, StringComparer.OrdinalIgnoreCase))
                                {
                                    profilePidl.SetErrorHandlingToIgnore();
                                }

                                ProxyController.UpdateIsOptionalPropertyWithFullPath(profilePidl, Constants.PropertiesToMakeMandatory.ToArray(), false);

                                if (smdCommercialEnabled && !string.Equals(profileCountry, country, StringComparison.OrdinalIgnoreCase))
                                {
                                    UpdateProfileAddressDataDescription(profilePidl, profileCountry, country, this.ExposedFlightFeatures, setting);
                                }

                                if (string.Equals(scenario, Constants.ScenarioNames.CommercialSignUp, StringComparison.OrdinalIgnoreCase))
                                {
                                    HideAddressFirstNameLastName(profilePidl);
                                }

                                foreach (PIDLResource paymentMethodPidl in paymentMethodPidls)
                                {
                                    PIDLResourceFactory.AddLinkedPidlToResource(paymentMethodPidl, profilePidl, partner, PidlContainerDisplayHint.SubmissionOrder.BeforeBase);

                                    // Add modern Validate for commercial partners when linkedPidl is present in PIDL
                                    if (this.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.ShowAVSSuggestions, StringComparer.OrdinalIgnoreCase))
                                    {
                                        this.AddModernValidationActionForPaymentMethodDescription(paymentMethodPidl, "address", Constants.AddressTypes.Internal, partner, language, country);

                                        // Add two additional flags to linked profile pidl
                                        if (this.ExposedFlightFeatures.Contains(
                                                Constants.PartnerFlightValues.EnableAVSAddtionalFlags,
                                                StringComparer.OrdinalIgnoreCase))
                                        {
                                            EnableAVSAdditionalFlagsToLinkedJarvisProfile(paymentMethodPidl);
                                        }
                                    }
                                }

                                retVal = paymentMethodPidls;
                                PIDLResourceFactory.RemoveEmptyPidlContainerHints(retVal);

                                if (ShouldUpdateTokenizationConsentPropertyDetails(family, country, operation, partner, this.ExposedFlightFeatures, setting, type))
                                {
                                    UpdateTokenizationConsentPropertyDetails(retVal);
                                }

                                PostProcessor.Process(retVal, PIDLResourceFactory.FeatureFactory, featureContext);

                                return retVal;
                            }

                            // Setup nextPidlUrl
                            string baseUrl = @"https://{pifd-endpoint}/users/{userId}/paymentMethodDescriptions";
                            var queryParams = this.Request.Query.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());

                            string nextPidlUrl = QueryHelpers.AddQueryString(baseUrl, queryParams);
                            PXCommon.RestLink nextPidlLink = new PXCommon.RestLink() { Href = nextPidlUrl, Method = HttpMethod.Get.ToString() };

                            // Get Profile PIDL
                            // Task 20658227: [PxService] Remove old "shipping_v3", "emp profile", "org profile" PIDL
                            // Rmove all flight related code for profile/address migration
                            Dictionary<string, string> profileV3Headers = null;
                            if (profile != null)
                            {
                                profileV3Headers = new Dictionary<string, string>();
                                profileV3Headers.Add(AccountV3Headers.Etag, profile.Etag);
                                profileV3Headers.Add(AccountV3Headers.IfMatch, profile.Etag);
                            }

                            string profileOperation = (profile == null ? Constants.Operations.Add : Constants.Operations.Update) + "_partial";
                            retVal = PIDLResourceFactory.Instance.GetProfileDescriptions(profileCountry, profileType, profileOperation, language, partner, nextPidlLink, profile != null ? profile.Id : null, profileV3Headers, false, this.ExposedFlightFeatures, scenario, setting: setting);
                            retVal[0].Identity["operation"] = profile == null ? Constants.Operations.Add : Constants.Operations.Update;
                            ProxyController.UpdateIsOptionalPropertyWithFullPath(retVal[0], Constants.PropertiesToMakeMandatory.ToArray(), false);
                            await this.PrefillUserData(retVal, accountId, profileCountry, partner, traceActivityId);
                            retVal.ForEach(r => r.MakeSecondaryResource());
                        }
                    }

                    // Show PI PIDL if prerequisites are all completed
                    if (retVal == null)
                    {
                        retVal = await this.GetPaymentMethodDescriptionsFromFactory(traceActivityId, accountId, country, family, type, language, partner, operation, classicProduct, billableAccountId, completePrerequisites, null, scenario, setting: setting);
                    }
                }
                else if (string.Equals(profileType, GlobalConstants.ProfileTypes.Organization, StringComparison.InvariantCultureIgnoreCase))
                {
                    retVal = await this.GetPaymentMethodDescriptionsFromFactory(traceActivityId, accountId, country, family, type, language, partner, operation, classicProduct, billableAccountId, completePrerequisites, null, scenario, setting: setting);
                }
            }
            else
            {
                if (PartnerHelper.IsAzurePartner(partner))
                {
                    retVal = await this.GetPaymentMethodDescriptionsFromFactory(traceActivityId, accountId, country, family, type, language, partner, operation, classicProduct, billableAccountId, false, null, scenario, setting: setting, isMyBaUser: isMyBaUser);
                }
                else
                {
                    retVal = await this.GetPaymentMethodDescriptionsFromFactory(traceActivityId, accountId, country, family, type, language, partner, operation, null, null, false, null, scenario, orderId, setting: setting, isMyBaUser: isMyBaUser);
                }
            }

            // Remove mandatory fields message when the scenario is DisplayOptionalFields or when the flight is ON.
            // TODO: This flight acts as safety net. We can remove this flight condition once webblends confirms no issues with this change
            if (string.Equals(scenario, Constants.ScenarioNames.DisplayOptionalFields, StringComparison.OrdinalIgnoreCase)
                || this.ExposedFlightFeatures.Contains(Flighting.Features.PXRemoveMandatoryFieldsMessage, StringComparer.OrdinalIgnoreCase))
            {
                ProxyController.RemoveDisplayDescription(retVal, new[] { "mandatory_fields_message" });
            }

            // Add modern Validate for azure and commercialstores partners
            this.AddModernAVSValidationAction(retVal, partner, family, type, operation, language, country);

            if (this.AllowViewTermsTrigger(partner))
            {
                AddViewTermsTriggerEventContext(retVal);
            }

            PIDLResourceFactory.RemoveEmptyPidlContainerHints(retVal);

            if (ShouldRemoveExpiryDateForTemplates(setting, family, type, partner, country, operation))
            {
                TryRemoveExpiryMonthAndYearFromPIDL(retVal);
            }

            if (ShouldUpdateTokenizationConsentPropertyDetails(family, country, operation, partner, this.ExposedFlightFeatures, setting, type))
            {
                UpdateTokenizationConsentPropertyDetails(retVal);
            }

            // TODO: Remove the if block once all the partners in ShouldRemoveExpiryDateForIndiaCommercial are moved to use the template
            // This block is enabled by default for templates in above ShouldRemoveExpiryDateForTemplates.
            if (ShouldRemoveExpiryDateForIndiaCommercial(operation, country, partner, this.ExposedFlightFeatures))
            {
                TryRemoveExpiryMonthAndYearFromPIDL(retVal);
            }

            if (ShouldRemoveExpiryDateForIndiaConsumer(operation, country, partner, this.ExposedFlightFeatures))
            {
                if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner)
                    && retVal?.First()?.Identity[Constants.PidlIdentityFields.Type] != Constants.PidlIdentityValues.ConsumerPrerequisites)
                {
                    PIDLResourceFactory.RemoveIndiaEditExpiry(retVal, this.ExposedFlightFeatures);
                }
                else
                {
                    TryRemoveExpiryMonthAndYearFromPIDL(retVal, partner, scenario);
                }
            }

            // Risk Eligibility Check
            if (await this.IsRiskEligible(family, type, language, partner, operation, billingAccountContext) == false)
            {
                return GetRiskEligibilityRejectedClientAction(language);
            }

            PostProcessor.Process(retVal, PIDLResourceFactory.FeatureFactory, featureContext);

            return retVal;
        }

        private static void UpdateAnonymousAddScreenWithPersonalData(QRCodeSecondScreenSession qrCodePaymentSessionData, ref List<PIDLResource> retVal)
        {
            foreach (PIDLResource pidl in retVal)
            {
                ButtonDisplayHint anonymousSaveButton = pidl.GetDisplayHintById(Constants.DisplayHintIds.AnonymousSaveButton) as ButtonDisplayHint;
                if (anonymousSaveButton != null)
                {
                    PXCommon.RestLink context = new PXCommon.RestLink()
                    {
                        Method = GlobalConstants.HTTPVerbs.POST,
                        Href = string.Format(Constants.SubmitUrls.PayMicrosoftMobilePageSubmit, qrCodePaymentSessionData.Country, qrCodePaymentSessionData.Language, qrCodePaymentSessionData.Partner, qrCodePaymentSessionData.Id, Constants.ScenarioNames.SecondScreenAddPi)
                    };

                    anonymousSaveButton.Action.Context = context;
                }
            }
        }

        private static void UpdateCurrencyBalanceText(List<PaymentInstrument> paymentInstrumentList, string country, string language)
        {
            foreach (PaymentInstrument pi in paymentInstrumentList)
            {
                if (string.Equals(country, Constants.CountryCodes.UnitedStates, StringComparison.OrdinalIgnoreCase) &&
                    pi.PaymentInstrumentDetails.IsXboxCoBrandedCard == true &&
                    pi.PaymentInstrumentDetails.PointsBalanceDetails?.RewardsEnabled == true)
                {
                    RewardsSummary rewardsSummary = pi.PaymentInstrumentDetails.PointsBalanceDetails.RewardsSummary;
                    double currencyBalance = rewardsSummary.CurrencyBalance;
                    string currencyCode = rewardsSummary.CurrencyCode;
                    rewardsSummary.CurrencyBalanceText = CurrencyHelper.FormatCurrency(country, language, Convert.ToDecimal(currencyBalance), currencyCode);
                }
            }
        }

        private static void TranslateUnicodes(List<PIDLResource> pidl)
        {
            var displayHints = pidl.First<PIDLResource>()?.GetAllDisplayHints();

            foreach (var displayHint in displayHints)
            {
                var textDisplayHint = displayHint as TextDisplayHint;
                if (textDisplayHint != null)
                {
                    textDisplayHint.DisplayContent = PXCommon.StringHelper.MapUnicodeChars(textDisplayHint.DisplayContent);
                }
            }
        }

        private static void AddViewTermsTriggerEventContext(List<PIDLResource> ret)
        {
            foreach (PIDLResource pidl in ret)
            {
                List<DisplayHint> viewTermsButtonList = pidl.GetAllDisplayHintsOfId(Constants.DisplayHintIds.ViewTermsButton);

                foreach (DisplayHint button in viewTermsButtonList)
                {
                    TriggerEventContext eventContext = new TriggerEventContext(CustomEventNameType.viewTermsTriggered.ToString());
                    button.Action.NextAction = new DisplayHintAction(DisplayHintActionType.triggerEvent.ToString());
                    button.Action.NextAction.Context = eventContext;
                }
            }
        }

        private static bool ShouldRemoveExpiryDateForTemplates(PaymentExperienceSetting setting, string family, string type, string partner, string country, string operation)
        {
            return string.Equals(operation, Constants.Operations.Update, StringComparison.OrdinalIgnoreCase)
                && string.Equals(country, Constants.CountryCodes.India, StringComparison.OrdinalIgnoreCase)
                && TemplateHelper.IsTemplateBasedPIDLIncludingDefaultTemplate(ProxyController.GetSettingTemplate(partner, setting, Constants.DescriptionTypes.PaymentMethodDescription, type, family));
        }

        private static bool ShouldUpdateTokenizationConsentPropertyDetails(string family, string country, string operation, string partner, List<string> exposedFlightFeatures, PaymentExperienceSetting setting, string type)
        {
            if (!exposedFlightFeatures.Contains(Constants.PartnerFlightValues.IndiaTokenizationConsentCapture, StringComparer.OrdinalIgnoreCase)
                && !TemplateHelper.IsTemplateBasedPIDLIncludingDefaultTemplate(ProxyController.GetSettingTemplate(partner, setting, Constants.DescriptionTypes.PaymentMethodDescription, type, family)))
            {
                return false;
            }

            return string.Equals(family, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(country, Constants.CountryCodes.India, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsGlobalPI(List<PaymentMethod> paymentMethods)
        {
            return paymentMethods != null &&
                paymentMethods.Count == 1 &&
                paymentMethods[0].Properties.IsNonStoredPaymentMethod;
        }

        private static HashSet<string> GetAllowedPaymentMethods(string allowedPaymentMethods, EventTraceActivity traceActivityId)
        {
            HashSet<string> includeSet = new HashSet<string>();
            if (!string.IsNullOrEmpty(allowedPaymentMethods))
            {
                // Allowed payment methods can either be a dictionary or a string array. Try deserializing to dictionary first.
                try
                {
                    includeSet = new HashSet<string>(JObject.Parse(allowedPaymentMethods).ToObject<Dictionary<string, int>>().Keys);
                }
                catch
                {
                    try
                    {
                        includeSet = new HashSet<string>(JsonConvert.DeserializeObject<string[]>(allowedPaymentMethods.ToLower()));
                    }
                    catch
                    {
                        throw TraceCore.TraceException<IntegrationException>(
                            traceActivityId,
                            new IntegrationException(
                                PXCommonConstants.ServiceNames.PXService,
                                "Error deserializing allowedPaymentMethods query param",
                                Constants.PXServiceIntegrationErrorCodes.PIDLInvalidAllowedPaymentMethods));
                    }
                }
            }

            return includeSet;
        }

        private static IEnumerable<string> GetAllowedPaymentMethodTypes(ref string type)
        {
            IEnumerable<string> allowedPaymentMethodTypes = null;
            if (!string.IsNullOrWhiteSpace(type))
            {
                allowedPaymentMethodTypes = type.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(i => i.Trim());

                // if there are more than 1 allowed type, set type to null so that we can get all paymentmethod details in a single call
                if (allowedPaymentMethodTypes.Count() > 1)
                {
                    type = null;
                }
            }

            return allowedPaymentMethodTypes;
        }

        private static void ValidateAndInferCountry(ref string country, string family, string type, string operation)
        {
            if (country == null)
            {
                // Currently, CUP PI instances from PIMS do not indicate that the country is CN. However, during update, GET /Pidl requires
                // the country parameter. In the AMC scenario, the country is unknown (the user is not browsing the store of any particular
                // country). So, if the user chooses to update a CUP PI, the client cannot figure out what the country parameter should be.
                // Hence the below hack until we have a more generalized solution (e.g Pims to add this to the PI payload) and below task is to track this.
                // Task 17620427: GET / paymentMethodDescriptions should not try to infer country from payment method type
                if (PIHelper.IsChinaUnionPay(family, type) && string.Equals(operation, "update", StringComparison.OrdinalIgnoreCase))
                {
                    country = "cn";
                }
                else
                {
                    throw new Common.ValidationException(ErrorCode.InvalidRequestData, "Country is a mandatory argument");
                }
            }
        }

        private static void RemoveCardDisplayTransformation(List<PIDLResource> retVal)
        {
            int minLength = int.MaxValue;
            int maxLength = int.MinValue;

            foreach (PIDLResource resource in retVal)
            {
                var lengths = GetAccountTokenLengths(resource);
                minLength = Math.Min(minLength, lengths.Item1);
                maxLength = Math.Max(maxLength, lengths.Item2);
            }

            foreach (PIDLResource resource in retVal)
            {
                bool removed = resource.RemoveDisplayTransformations();
                if (removed)
                {
                    string cardType;
                    if (resource.Identity.TryGetValue("type", out cardType))
                    {
                        resource.SetMinLength("accountToken", minLength);
                        resource.SetMaxLength("accountToken", maxLength);
                    }
                }
            }
        }

        private static void RemoveResolutionRegex(string partner, List<PIDLResource> retVal)
        {
            if (!PXCommonConstants.PartnerGroups.CCQuickResolutionEnabledPartners.Contains(partner, StringComparer.OrdinalIgnoreCase))
            {
                foreach (PIDLResource resource in retVal)
                {
                    resource.RemoveResolutionRegex();
                }
            }
        }

        private static void RemoveLuhnValidation(string family, string partner, List<PIDLResource> retVal, List<string> exposedFlights)
        {
            // This below block works independent between the LuhnValidationEnabledPartners LIST and the FLIGHT of PXLuhnValidationEnabledPartners.
            if (family == Constants.PaymentMethodFamily.credit_card.ToString()
                && !(Constants.LuhnValidationEnabledPartners.Contains(partner, StringComparer.OrdinalIgnoreCase)
                    || exposedFlights.Contains(PXCommon.Flighting.Features.PXLuhnValidationEnabledPartners, StringComparer.OrdinalIgnoreCase)))
            {
                foreach (PIDLResource resource in retVal)
                {
                    PropertyDescription accountToken = resource.GetPropertyDescriptionByPropertyName(Constants.CreditCardPropertyDescriptionName.AccountToken);
                    if (accountToken != null && accountToken.Validations != null && accountToken.Validations.Count > 1)
                    {
                        accountToken.Validations.RemoveAt(1);
                    }
                }
            }
        }

        private static void RemoveUnsupportedFlightFeatures(List<string> exposedFlightFeatures, Version pidlSdkVersion)
        {
            if (exposedFlightFeatures?.Contains(Flighting.Features.PXUseEdgePIFD, StringComparer.OrdinalIgnoreCase) ?? false)
            {
                // Use this flight only for the pidlsdk client version 1.17.0 and above
                if (pidlSdkVersion == null || pidlSdkVersion < new Version(1, 17))
                {
                    exposedFlightFeatures.RemoveAll(s => string.Equals(s, Flighting.Features.PXUseEdgePIFD, StringComparison.OrdinalIgnoreCase));
                }
            }
        }

        private static bool SkipAddPIForPaymentMethod(string family, string type, string operation)
        {
            return string.Equals(family, Constants.PaymentMethodFamily.ewallet.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(type, Constants.PaymentMethodType.LegacyBilldeskPayment, StringComparison.OrdinalIgnoreCase)
                && string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase);
        }

        private static IEnumerable<KeyValuePair<string, string>> GetPiQueryParams(
            string country,
            string classicProduct,
            string billableAccountId)
        {
            var retVal = new System.Collections.Generic.List<KeyValuePair<string, string>>();

            if (!string.IsNullOrEmpty(country))
            {
                retVal.Add(new KeyValuePair<string, string>(GlobalConstants.QueryParamNames.Country, country));
            }

            if (!string.IsNullOrEmpty(classicProduct))
            {
                retVal.Add(new KeyValuePair<string, string>(GlobalConstants.QueryParamNames.ClassicProduct, classicProduct));
            }

            if (!string.IsNullOrEmpty(billableAccountId))
            {
                retVal.Add(new KeyValuePair<string, string>(GlobalConstants.QueryParamNames.BillableAccountId, billableAccountId));
            }

            return retVal;
        }

        private static bool InvalidEmployeeName(string name)
        {
            Guid value;
            return string.IsNullOrEmpty(name) || Guid.TryParse(name, out value) || name.Contains('@');
        }

        private static void EnableAVSAdditionalFlagsToLinkedJarvisProfile(PIDLResource pidlResources)
        {
            ProxyController.AddHiddenCheckBoxElement(
                pidlResources,
                GlobalConstants.CommercialZipPlusFourPropertyNames.IsUserConsented,
                Constants.Profile.DefaultAddress,
                inlinkedPidl: true);
            ProxyController.AddHiddenCheckBoxElement(
                pidlResources,
                GlobalConstants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded,
                Constants.Profile.DefaultAddress,
                inlinkedPidl: true);
        }

        private static bool ContainsAddressInPIPidl(PIDLResource retVal)
        {
            object result = null;
            object customer = null;

            if (retVal.DataDescription.TryGetValue(Constants.PaymentInstrument.Details, out result))
            {
                List<PIDLResource> details = result as List<PIDLResource>;
                return details != null && details.Count > 0 && details[0].DataDescription != null && details[0].DataDescription.ContainsKey(Constants.DescriptionTypes.AddressDescription);
            }
            else if (retVal.DataDescription.TryGetValue(Constants.DescriptionTypes.Customer, out customer))
            {
                List<PIDLResource> customerPidlResource = customer as List<PIDLResource>;
                return customerPidlResource != null && customerPidlResource.Count > 0 && customerPidlResource[0].DataDescription != null && customerPidlResource[0].DataDescription.ContainsKey(Constants.DescriptionTypes.BillingAddressDescription);
            }

            return false;
        }

        private static bool IsAddressNoCityStateConfiguration(PIDLResource retVal, string scenario, string country, string family)
        {
            object result = null;

            // TODO: As part of T-54279751 investigation, the AddressNoCityState flow found to be not used by any partner
            // And below code & csv config related to it can be removed
            if (retVal.DataDescription.TryGetValue(Constants.PaymentInstrument.Details, out result)
                && string.Equals(scenario, Constants.ScenarioNames.AddressNoCityState, StringComparison.InvariantCultureIgnoreCase)
                && string.Equals(country, Constants.CountryCodes.UnitedStates, StringComparison.OrdinalIgnoreCase)
                && string.Equals(family, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                List<PIDLResource> details = result as List<PIDLResource>;
                return details != null && details.Count > 0 && details[0].DataDescription != null && details[0].DataDescription.ContainsKey(Constants.DescriptionTypes.AddressDescription) && !details[0].DataDescription.ContainsKey(Constants.AddressNoCityStatePropertyNames.City) && !details[0].DataDescription.ContainsKey(Constants.AddressNoCityStatePropertyNames.Region);
            }

            return false;
        }

        private static void ConvertUpdatePidlToReplacePidl(List<PIDLResource> pidlResources, string language, string scenario)
        {
            foreach (var pidl in pidlResources)
            {
                pidl.Identity["operation"] = Constants.Operations.Replace;

                // Modify the display elements
                HeadingDisplayHint headingEl = pidl.GetDisplayHintById("add_credit_debit_heading") as HeadingDisplayHint;
                if (headingEl != null)
                {
                    headingEl.DisplayContent = LocalizationRepository.Instance.GetLocalizedString("(Replace payment method **{partnerData.prefillData.lastFourDigits})", language);
                }

                // Add replace operation to submit URL
                if (string.Equals(scenario, "withNewAddress", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(scenario, "hasSubsOrPreOrders", StringComparison.OrdinalIgnoreCase))
                {
                    var saveButtonLink = pidl.GetDisplayHintById("saveButton")?.Action?.Context as Microsoft.Commerce.Payments.PXCommon.RestLink;
                    if (!string.IsNullOrWhiteSpace(saveButtonLink?.Href))
                    {
                        saveButtonLink.Href = saveButtonLink.Href.Replace("/paymentInstrumentsEx?", "/paymentInstrumentsEx/{partnerData.prefillData.piid}/replace?");
                    }
                }
            }
        }

        private static void RemoveDefaultValuesInAddress(List<PIDLResource> pidlResources)
        {
            foreach (var pidl in pidlResources)
            {
                foreach (string addressFieldPropertyName in Constants.AddressFieldsWithDefaultValueNotNeededForUpdateAndReplace)
                {
                    PropertyDescription propertyDescription = pidl.GetPropertyDescriptionByPropertyName(addressFieldPropertyName);
                    if (propertyDescription != null)
                    {
                        propertyDescription.DefaultValue = null;
                    }
                }
            }
        }

        private static Tuple<int, int> GetAccountTokenLengths(PIDLResource resource)
        {
            int minLength = int.MaxValue;
            int maxLength = int.MinValue;
            string cardType;
            if (resource.Identity.TryGetValue("type", out cardType))
            {
                if (Constants.KoreaLocalCardTypes.Contains(cardType))
                {
                    minLength = 13;
                    maxLength = 20;
                }
                else if (Constants.BrazilLocalCardTypes.Contains(cardType) || Constants.NigeriaLocalCardTypes.Contains(cardType))
                {
                    minLength = 13;
                    maxLength = 19;
                }
                else
                {
                    // default to international card lengths
                    minLength = 15;
                    maxLength = 16;
                }
            }

            return new Tuple<int, int>(minLength, maxLength);
        }

        private static void UpdateProfileAddressDataDescription(PIDLResource profilePidl, string originCountry, string requestCountry, List<string> exposedFlightFeatures, PaymentExperienceSetting setting)
        {
            string addressPath = "default_address";
            string[] addressPropertiesExceptCountry = { "address_line1", "address_line2", "address_line3", "city", "region", "postal_code" };

            PIDLResourceFactory.RemoveDataDescriptionWithFullPath(profilePidl, addressPath, addressPropertiesExceptCountry);

            var addressCountryPropertyDescription = new PropertyDescription()
            {
                PropertyType = "clientData",
                DataType = "hidden",
                PropertyDescriptionType = "hidden",
                IsUpdatable = false,
                DefaultValue = originCountry
            };
            profilePidl.UpdateDataDescription(addressPath, "country", addressCountryPropertyDescription);

            if ((exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXOverrideCultureAndLanguageTransformation, StringComparer.OrdinalIgnoreCase))
                || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.OverrideLinkedProfileCultureAndLanguageTransformation, requestCountry, setting))
            {
                AddOrUpdatePidlPropertyTransformationForSubmit(profilePidl, Constants.Profile.Culture, Constants.Profile.ProfileEmployeeCulture, originCountry);
                AddOrUpdatePidlPropertyTransformationForSubmit(profilePidl, Constants.Profile.Language, Constants.Profile.ProfileEmployeeLanguage, originCountry);
            }
        }

        private static void HideAddressFirstNameLastName(PIDLResource profilePidl)
        {
            string[] addressProperties = { Constants.DisplayHintIds.AddressFirstName, Constants.DisplayHintIds.AddressLastName };
            string[] addressDataDescriptionProperties = { Constants.DisplayHintIds.FirstName, Constants.DisplayHintIds.LastName };

            foreach (string targetDataDescriptionId in addressProperties)
            {
                profilePidl.SetVisibilityOfDisplayHint(targetDataDescriptionId, true);
            }

            foreach (string addressDataDescriptionProperty in addressDataDescriptionProperties)
            {
                profilePidl.UpdatePropertyType(addressDataDescriptionProperty, "clientData");
            }
        }

        private static void AddOrUpdatePidlPropertyTransformationForSubmit(PIDLResource pidl, string pidlPropertyToUpdate, string newTransformationPropertyName, string newTransformationCountry)
        {
            string transformationTarget = "forSubmit";
            PropertyTransformationInfo newTransformationInfo = null;

            PropertyDescription propertyDescription = pidl.GetPropertyDescriptionByPropertyName(pidlPropertyToUpdate);
            PIDLResourceFactory.Instance.GetPropertyTransformation(newTransformationPropertyName, newTransformationCountry)?.TryGetValue(transformationTarget, out newTransformationInfo);

            if (propertyDescription != null && newTransformationInfo != null)
            {
                if (propertyDescription.Transformation != null)
                {
                    if (propertyDescription.Transformation.ContainsKey(transformationTarget))
                    {
                        propertyDescription.Transformation[transformationTarget] = newTransformationInfo;
                    }
                    else
                    {
                        propertyDescription.Transformation.Add(transformationTarget, newTransformationInfo);
                    }
                }
                else
                {
                    propertyDescription.AddTransformation(new Dictionary<string, PropertyTransformationInfo>()
                    {
                        { transformationTarget, newTransformationInfo }
                    });
                }
            }
        }

        private static void UpdateTokenizationConsentPropertyDetails(List<PIDLResource> pidlResources)
        {
            foreach (var pidl in pidlResources)
            {
                object interObj;

                pidl.DataDescription.TryGetValue("details", out interObj);

                if (interObj == null)
                {
                    continue;
                }

                var tokenizationConsentObj = new PropertyDescription
                {
                    PropertyType = "clientData",
                    PropertyDescriptionType = "hidden",
                    DataType = "bool",
                    IsKey = false,
                    IsOptional = false,
                    IsUpdatable = false,
                    DefaultValue = true,
                };

                var dataDescriptionDetails = (List<PIDLResource>)interObj;

                if (dataDescriptionDetails == null || dataDescriptionDetails.Count == 0 || dataDescriptionDetails[0].DataDescription == null)
                {
                    continue;
                }

                dataDescriptionDetails[0].DataDescription["tokenizationConsent"] = tokenizationConsentObj;
            }
        }

        private static bool ShouldRemoveExpiryDateForIndiaCommercial(string operation, string country, string partner, List<string> exposedFlightFeatures)
        {
            if (string.Equals(operation, Constants.Operations.Update, StringComparison.OrdinalIgnoreCase) && string.Equals(country, Constants.CountryCodes.India, StringComparison.OrdinalIgnoreCase) && exposedFlightFeatures.Contains(Constants.PartnerFlightValues.IndiaExpiryGroupDelete, StringComparer.OrdinalIgnoreCase))
            {
                if (string.Equals(partner, Constants.PartnerName.Azure, StringComparison.OrdinalIgnoreCase) || string.Equals(partner, Constants.PartnerName.CommercialStores, StringComparison.OrdinalIgnoreCase) || string.Equals(partner, Constants.PartnerName.Bing, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ShouldRemoveExpiryDateForIndiaConsumer(string operation, string country, string partner, List<string> exposedFlightFeatures)
        {
            if (string.Equals(operation, Constants.Operations.Update, StringComparison.OrdinalIgnoreCase) && string.Equals(country, Constants.CountryCodes.India, StringComparison.OrdinalIgnoreCase) && exposedFlightFeatures.Contains(Constants.PartnerFlightValues.IndiaExpiryGroupDelete, StringComparer.OrdinalIgnoreCase))
            {
                if (string.Equals(partner, Constants.PartnerName.AmcWeb, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(partner, Constants.PartnerName.Webblends, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(partner, Constants.PartnerName.Payin, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(partner, Constants.PartnerName.NorthStarWeb, StringComparison.OrdinalIgnoreCase)
                    || PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ShouldUpdateSummaryFooterForIndiaConsumer(string operation, string country, string partner, string family, List<string> exposedFlightFeatures)
        {
            if (string.Equals(operation, Constants.Operations.Update, StringComparison.OrdinalIgnoreCase)
                && string.Equals(country, Constants.CountryCodes.India, StringComparison.OrdinalIgnoreCase)
                && !exposedFlightFeatures.Contains(Constants.PartnerFlightValues.IndiaExpiryGroupDelete, StringComparer.OrdinalIgnoreCase)
                && string.Equals(family, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase)
                && PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner))
            {
                return true;
            }

            return false;
        }

        private static void TryRemoveExpiryMonthAndYearFromPIDL(List<PIDLResource> pidlResources, string partner = null, string scenario = null)
        {
            foreach (var pidl in pidlResources)
            {
                // If PIDL is not complete, we do not need to remove the fields as they would not exist in the first place.
                if (!pidl.DataDescription.ContainsKey("details"))
                {
                    continue;
                }

                var dataDescriptionDetails = (List<PIDLResource>)pidl.DataDescription["details"];
                object expiryMonth, expiryYear;
                if (dataDescriptionDetails[0].DataDescription.TryGetValue("expiryMonth", out expiryMonth) && expiryMonth != null)
                {
                    dataDescriptionDetails[0].DataDescription.Remove("expiryMonth");
                }

                if (dataDescriptionDetails[0].DataDescription.TryGetValue("expiryYear", out expiryYear) && expiryYear != null)
                {
                    dataDescriptionDetails[0].DataDescription.Remove("expiryYear");
                }

                if (string.Equals(partner, Constants.PartnerName.NorthStarWeb, StringComparison.OrdinalIgnoreCase) || (string.Equals(partner, Constants.PartnerName.AmcWeb, StringComparison.OrdinalIgnoreCase) && string.Equals(scenario, Constants.ScenarioNames.PayNow, StringComparison.OrdinalIgnoreCase)) || (string.Equals(partner, Constants.PartnerName.AmcWeb, StringComparison.OrdinalIgnoreCase) && string.Equals(scenario, Constants.ScenarioNames.ChangePI, StringComparison.OrdinalIgnoreCase)))
                {
                    foreach (var page in pidl.DisplayPages)
                    {
                        var expiryAndCvvGroupProperty = page.Members.FirstOrDefault(propertyHint => propertyHint.HintId == "expiryAndCvvUpdateGroup") as GroupDisplayHint;
                        if (expiryAndCvvGroupProperty != null)
                        {
                            var expiryGroupProperty = expiryAndCvvGroupProperty.Members.FirstOrDefault(propertyHint => propertyHint.HintId == "expiryGroup") as GroupDisplayHint;
                            expiryAndCvvGroupProperty.Members.Remove(expiryGroupProperty);
                        }
                    }
                }
                else
                {
                    foreach (var page in pidl.DisplayPages)
                    {
                        var expiryGroupProperty = page.Members.FirstOrDefault(propertyHint => propertyHint.HintId == "expiryGroup");
                        page.Members.Remove(expiryGroupProperty);
                    }
                }
            }
        }

        private static void UpdateCCNameRegex(List<PIDLResource> pidlResources)
        {
            string updatedRegex = "^(?![\\.\\-\\& '0-9]+$)[\\.\\-\\& '0-9a-zA-Z\\xC0-\\uFFFF]{1,64}$";
            foreach (PIDLResource resource in pidlResources)
            {
                PropertyDescription accountHolderName = resource.GetPropertyDescriptionByPropertyName(Constants.CreditCardPropertyDescriptionName.AccountHolderName);
                if (accountHolderName != null)
                {
                    if (accountHolderName.Validation != null)
                    {
                        accountHolderName.Validation.Regex = updatedRegex;
                    }

                    if (accountHolderName.Validations != null)
                    {
                        foreach (PropertyValidation validation in accountHolderName.Validations)
                        {
                            if (string.Equals(validation.ValidationType, "regex", StringComparison.OrdinalIgnoreCase))
                            {
                                validation.Regex = updatedRegex;
                            }
                        }
                    }
                }
            }
        }

        private static bool IsSecondScreenAddPiFlow(string scenario, string family, string operation)
        {
            // TODO: Check to see if secondScreenTemplate is being passed by PSS https://microsoft.visualstudio.com/OSGS/_workitems/edit/54365478
            if (scenario != null && scenario.Equals(Constants.ScenarioNames.SecondScreenAddPi, StringComparison.OrdinalIgnoreCase)
               && family != null && family.Equals(Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase)
               && operation != null && operation.Equals(Constants.Operations.Add, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static bool UseClientSidePrefillPidl(string partner, string operation, string country, PaymentExperienceSetting setting)
        {
            return PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseClientSidePrefill, country, setting) || (PartnerHelper.IsClientSideListPIPrefillRequired(partner) && operation.Equals(Constants.Operations.SelectInstance, StringComparison.OrdinalIgnoreCase));
        }

        private static bool UseServerSidePrefillPidl(string partner, string country, PaymentExperienceSetting setting)
        {
            return PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseServerSidePrefill, country, setting);
        }

        private static void SetAlwaysUpdateModelValue(List<PIDLResource> pidlResourceList, string displayHintId)
        {
            foreach (PIDLResource pidlResource in pidlResourceList)
            {
                var propertyDisplayHint = pidlResource.GetDisplayHintById(displayHintId) as PropertyDisplayHint;
                if (propertyDisplayHint != null)
                {
                    propertyDisplayHint.AlwaysUpdateModelValue = true;
                }
            }
        }

        private static List<PIDLResource> GetRiskEligibilityRejectedClientAction(string language)
        {
            // risk eligibility check failed, retrun a failure.
            var piEligibilityFailedResponse = new ServiceErrorResponse
            {
                ErrorCode = V7.Constants.RiskErrorCode.PIEligibilityCheckRejectedbyRisk,
                InnerError = new ServiceErrorResponse
                {
                    ErrorCode = V7.Constants.RiskErrorCode.PIEligibilityCheckRejectedbyRisk,
                    Message = V7.Constants.RiskErrorMessages.PIEligibilityCheckRejectedbyRisk,
                }
            };

            piEligibilityFailedResponse.InnerError.UserDisplayMessage = PidlModelHelper.GetLocalizedString(
                configText: V7.Constants.RiskErrorMessages.PIEligibilityCheckRejectedbyRisk,
                language: language ?? GlobalConstants.Defaults.Locale);

            var pidls = new List<PIDLResource>()
                        {
                            new PIDLResource()
                            {
                               ClientAction = new ClientAction(ClientActionType.Failure, piEligibilityFailedResponse)
                            }
                        };

            return pidls;
        }

        private static List<PaymentMethod> MapToPaymentMethods(Model.PaymentOrchestratorService.WalletEligiblePaymentMethods eligiblePaymentMethods)
        {
            List<PaymentMethod> paymentMethodList = new List<PaymentMethod>();

            if (eligiblePaymentMethods == null || eligiblePaymentMethods.PaymentMethods == null)
            {
                return paymentMethodList;
            }

            foreach (var paymentMethodResource in eligiblePaymentMethods.PaymentMethods)
            {
                if (paymentMethodResource == null)
                {
                    continue;
                }

                PaymentMethod paymentMethod = new PaymentMethod
                {
                    PaymentMethodFamily = paymentMethodResource.PaymentMethodFamily,
                    PaymentMethodType = paymentMethodResource.PaymentMethodType,
                    PaymentMethodGroup = paymentMethodResource.PaymentMethodGroup,
                    GroupDisplayName = paymentMethodResource.GroupDisplayName,
                    Properties = new PaymentMethodCapabilities()
                };

                // Copy display details if available
                if (paymentMethodResource.Display != null)
                {
                    paymentMethod.Display = new PaymentInstrumentDisplayDetails
                    {
                        Name = paymentMethodResource.Display.Name ?? string.Empty,
                        Logo = paymentMethodResource.Display.Logo ?? string.Empty
                    };

                    // Convert Logo[] to List<Logo>
                    if (paymentMethodResource.Display.Logos != null && paymentMethodResource.Display.Logos.Length > 0)
                    {
                        paymentMethod.Display.Logos = paymentMethodResource.Display.Logos.Select(logo => new Logo
                        {
                            MimeType = logo.MimeType,
                            Url = logo.Url
                        }).ToList();
                    }
                }

                // Copy payment method properties if available
                if (paymentMethodResource.Properties != null)
                {
                    paymentMethod.Properties.OfflineRecurring = paymentMethodResource.Properties.OfflineRecurring;
                    paymentMethod.Properties.UserManaged = paymentMethodResource.Properties.UserManaged;
                    paymentMethod.Properties.SoldToAddressRequired = paymentMethodResource.Properties.SoldToAddressRequired;
                    paymentMethod.Properties.Taxable = paymentMethodResource.Properties.Taxable;
                    paymentMethod.Properties.ProviderRemittable = paymentMethodResource.Properties.ProviderRemittable;
                    paymentMethod.Properties.SplitPaymentSupported = paymentMethodResource.Properties.SplitPaymentSupported;

                    if (paymentMethodResource.Properties.SupportedOperations != null && paymentMethodResource.Properties.SupportedOperations.Length > 0)
                    {
                        foreach (var operation in paymentMethodResource.Properties.SupportedOperations)
                        {
                            paymentMethod.Properties.AddOperation(operation);
                        }
                    }
                }

                // Convert exclusion tags if they exist
                if (paymentMethodResource.ExclusionTags != null && paymentMethodResource.ExclusionTags.Length > 0)
                {
                    paymentMethod.ExclusionTags = new List<string>(paymentMethodResource.ExclusionTags);
                }

                paymentMethodList.Add(paymentMethod);
            }

            return paymentMethodList;
        }

        private bool ShouldReturnProfileDescription(AccountProfile profile, object[] taxIds, string country, bool ignoreMissingTaxId)
        {
            return profile == null
                || string.IsNullOrEmpty(profile.FirstName)
                || string.IsNullOrEmpty(profile.LastName)
                || string.IsNullOrEmpty(profile.EmailAddress)
                || this.RequiresTaxIds(taxIds, country, ignoreMissingTaxId);
        }

        private bool ShouldReturnProfileDescription(AccountProfileV3 profile, object[] taxIds, string country, bool ignoreMissingTaxId)
        {
            return profile == null
                || string.IsNullOrEmpty(profile.FirstName)
                || string.IsNullOrEmpty(profile.LastName)
                || string.IsNullOrEmpty(profile.GetEmailAddressPropertyValue())
                || this.RequiresTaxIds(taxIds, country, ignoreMissingTaxId);
        }

        private bool IsCountryEnabledTaxIdInConsumerFlow(string country)
        {
            return string.Equals(country, Constants.CountryCodes.Brazil, StringComparison.OrdinalIgnoreCase)
                || (string.Equals(country, Constants.CountryCodes.Portugal, StringComparison.OrdinalIgnoreCase) && this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableTaxIdInPT));
        }

        private bool RequiresTaxIds(object[] taxIds, string country, bool ignoreMissingTaxId)
        {
            return !ignoreMissingTaxId
                && country != null
                && this.IsCountryEnabledTaxIdInConsumerFlow(country)
                && (taxIds == null || taxIds.Length == 0);
        }

        private bool AllowViewTermsTrigger(string partner)
        {
            return PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner) &&
                this.ExposedFlightFeatures.Contains(Flighting.Features.PXViewTermsTriggerCustomEvent, StringComparer.OrdinalIgnoreCase);
        }

        private bool IsNorthStarExperiencePartnerAndScenario(string partner, string scenario)
        {
            return this.northStarExperiencePartnersScenarios.ContainsKey(partner.ToLower())
                && this.northStarExperiencePartnersScenarios[partner].Contains(scenario, StringComparer.OrdinalIgnoreCase);
        }

        private bool IsConsoleAddCCQrCodeFlow(List<string> exposedFlightFeatures, string scenario, Version fullPidlSdkVersion, List<PIDLResource> retVal)
        {
            Version lowestCompatiblePidlVersionForQrCodeFlow = new Version(Constants.PidlSdkVersionNumber.PidlSdkMajor2, Constants.PidlSdkVersionNumber.PidlSdkMinor7, Constants.PidlSdkVersionNumber.PidlSdkBuild0);
            Version pidlVersionForQrCodeScxenarioStyles = new Version(Constants.PidlSdkVersionNumber.PidlSdkMajor2, Constants.PidlSdkVersionNumber.PidlSdkMinor7, Constants.PidlSdkVersionNumber.PidlSdkBuild1);

            //// TODO: Uncomment once PSS integrates xbox partners https://microsoft.visualstudio.com/OSGS/_workitems/edit/54303371
            //// && ProxyController.GetSettingTemplate(partner, setting, Constants.DescriptionTypes.PaymentMethodDescription, type, family) == Constants.TemplateName.ConsoleTemplate)
            if (this.ExposedFlightFeatures.Contains(Flighting.Features.PxEnableAddCcQrCode, StringComparer.OrdinalIgnoreCase)
                && string.Equals(scenario, Constants.ScenarioNames.AddCCQrCode)
                && fullPidlSdkVersion >= lowestCompatiblePidlVersionForQrCodeFlow
                && (this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableXboxNativeStyleHints, StringComparer.OrdinalIgnoreCase) || fullPidlSdkVersion >= pidlVersionForQrCodeScxenarioStyles))
            {
                return true;
            }

            return false;
        }

        private async Task<IList<PaymentMethod>> GetPaymentMethods(
            string country,
            string language,
            EventTraceActivity traceActivityId,
            string partner,
            string operation,
            string merchantId,
            string currency,
            PaymentExperienceSetting setting)
        {
            IList<PaymentMethod> paymentMethods = new List<PaymentMethod>();
            if (!(string.Equals(partner, PXCommon.Constants.PartnerNames.XboxSettings, StringComparison.OrdinalIgnoreCase) && operation.Equals(Constants.Operations.SelectInstance, StringComparison.OrdinalIgnoreCase)))
            {
                paymentMethods = await this.Settings.PIMSAccessor.GetPaymentMethods(country, null, null, language, traceActivityId, this.GetAdditionalHeadersFromRequest(), partner, this.ExposedFlightFeatures, operation, setting: setting);
            }

            if (merchantId != null && currency != null)
            {
                paymentMethods = await this.Settings.MerchantCapabilitiesAccessor.FilterPaymentMethods(merchantId, currency, paymentMethods, traceActivityId);
            }

            return paymentMethods;
        }

        private async Task<List<PaymentInstrument>> GetPaymentInstruments(
            string accountId,
            EventTraceActivity traceActivityId,
            string partner,
            string country,
            string language,
            string operation,
            string billableAccountId,
            string orderId,
            string sessionId,
            string actualPuid,
            PaymentExperienceSetting setting = null)
        {
            string[] statusList = new string[] { "active", "pending" };
            List<KeyValuePair<string, string>> queryParams = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(Constants.QueryParameterName.Language, language)
            };

            if (!string.Equals(partner, PXCommon.Constants.PartnerNames.XboxSettings, StringComparison.OrdinalIgnoreCase))
            {
                queryParams.Add(new KeyValuePair<string, string>(Constants.QueryParameterName.Country, country));
            }

            if (!string.IsNullOrEmpty(billableAccountId))
            {
                queryParams.Add(new KeyValuePair<string, string>(Constants.QueryParameterName.BillableAccountId, billableAccountId));
            }

            List<PaymentInstrument> disabledPaymentInstruments = new List<PaymentInstrument>();
            IList<PaymentInstrument> paymentInstruments = new List<PaymentInstrument>();

            if (ExposedFlightFeatures.Contains(Flighting.Features.PXEnableEmpOrgListPI, StringComparer.OrdinalIgnoreCase))
            {
                paymentInstruments = await this.Settings.PIMSAccessor.ListUserAndTenantPaymentInstrument(0, statusList, traceActivityId, queryParams, partner, language: language, country: country, exposedFlightFeatures: this.ExposedFlightFeatures, operation: operation, setting: setting);
            }
            else
            {
                paymentInstruments = await this.Settings.PIMSAccessor.ListPaymentInstrument(accountId, 0, statusList, traceActivityId, queryParams, partner, language: language, country: country, exposedFlightFeatures: this.ExposedFlightFeatures, operation: operation, setting: setting);
            }

            if (orderId != null && sessionId != null && actualPuid != null)
            {
                paymentInstruments = await this.Settings.RiskServiceAccessor.FilterPaymentInstruments(actualPuid, partner, orderId, sessionId, paymentInstruments, disabledPaymentInstruments, traceActivityId);
            }

            return new List<PaymentInstrument>(paymentInstruments);
        }

        private async Task PopulatePaymentInstrumentsIntoDataSources(
            List<PIDLResource> resourceList,
            string accountId,
            EventTraceActivity traceActivityId,
            string partner,
            string country,
            string language,
            string operation,
            string billableAccountId,
            string allowedPaymentMethods,
            string filters,
            string merchantId,
            string currency,
            string orderId,
            string sessionId,
            string actualPuid,
            PaymentExperienceSetting setting = null)
        {
            // get Payment Methods and filter them
            var paymentMethods = await this.GetPaymentMethods(country, language, traceActivityId, partner, operation, merchantId, currency, setting);
            HashSet<PaymentMethod> paymentMethodHashSet = new HashSet<PaymentMethod>(paymentMethods);
            HashSet<PaymentMethod> filteredPaymentMethods = PaymentSelectionHelper.GetFilteredPaymentMethods(paymentMethodHashSet, allowedPaymentMethods, filters, operation, partner, country, setting);

            // get Payment Instruments and filter them
            List<PaymentInstrument> disabledPaymentInstruments = new List<PaymentInstrument>();
            List<PaymentInstrument> paymentInstrumentList = await this.GetPaymentInstruments(accountId, traceActivityId, partner, country, language, operation, billableAccountId, orderId, sessionId, actualPuid, setting: setting);
            if (string.Equals(partner, Constants.PartnerName.XboxSettings, StringComparison.OrdinalIgnoreCase)
                && ExposedFlightFeatures.Contains(Flighting.Features.PXEnableXboxNativeListPIRewardsPointsDisplay))
            {
                UpdateCurrencyBalanceText(paymentInstrumentList, country, language);
            }

            List<PaymentInstrument> filteredPaymentInstruments = PaymentSelectionHelper.GetFilteredPaymentInstruments(paymentInstrumentList, disabledPaymentInstruments, filteredPaymentMethods, allowedPaymentMethods, filters, partner, country);

            // Add new payment method option if feature is enabled
            if (PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.AddNewPaymentMethodOption, country, setting))
            {
                var addNewPM = CreateNewPaymentMethodOption(accountId);
                if (filteredPaymentInstruments.Count == 0)
                {
                    // if PI list is empty then add a suffix to addNewPM PM type to prevent default selection
                    addNewPM.PaymentMethod.PaymentMethodType = Constants.PaymentMethodType.AddNewPMNoDefaultSelection;

                    // set AlwaysUpdateModelValue to true on the dropdown element
                    // this will force update model value, in pidl sdk, even when the selected value is not changed
                    SetAlwaysUpdateModelValue(resourceList, Constants.DisplayHintIds.PaymentInstrumentListPi);
                }

                filteredPaymentInstruments.Add(addNewPM);
            }

            foreach (PIDLResource resource in resourceList)
            {
                resource.DataSources["paymentInstruments"] = new DataSource(filteredPaymentInstruments?.Cast<object>()?.ToList());
            }
        }

        private async Task<List<PIDLResource>> ReturnPIForPaymentMethodCanSkipAddPI(
            string family,
            string type,
            string operation,
            string accountId,
            EventTraceActivity traceActivityId,
            string classicProduct,
            string billableAccountId,
            string partner,
            string country)
        {
            PIDLData pi = new PIDLData();
            pi["paymentMethodFamily"] = family;
            pi["paymentMethodType"] = type;
            PaymentInstrument newPI = await this.Settings.PIMSAccessor.PostPaymentInstrument(accountId, pi, traceActivityId, GetPiQueryParams(country, classicProduct, billableAccountId), null, partner);

            SuccessResponsePayload pidlSDKSuccessResponsePayload = new SuccessResponsePayload()
            {
                Response = newPI,
                Id = newPI?.PaymentInstrumentId,
                OperationType = operation
            };

            PIDLResource pidlResource = new PIDLResource
            {
                ClientAction = new ClientAction(ClientActionType.ReturnContext, pidlSDKSuccessResponsePayload)
            };

            return new List<PIDLResource> { pidlResource };
        }

        private async Task<List<PIDLResource>> ReturnRestActionToGetPIForGlobalPI(
            string accountId,
            string partner,
            string country,
            string language,
            string family,
            string type,
            EventTraceActivity traceActivityId,
            string operation,
            PaymentExperienceSetting setting)
        {
            List<PaymentMethod> paymentMethods = await this.Settings.PIMSAccessor.GetPaymentMethods(country, family, type, language, traceActivityId, this.GetAdditionalHeadersFromRequest(), partner, this.ExposedFlightFeatures, operation, setting: setting);
            if (IsGlobalPI(paymentMethods))
            {
                RestActionContext actionContext = new RestActionContext();
                actionContext.Href = @"https://{pifd-endpoint}/" + "users/{userId}" + "/" + "paymentInstrumentsEx/" + paymentMethods[0].Properties.NonStoredPaymentMethodId + "?country=" + country + "&language=" + language + "&partner=" + partner;
                actionContext.Method = "GET";

                // Set ShouldHandleSuccess to be true to let PIDLSDK invoke success event for the response from restAction.
                actionContext.ShouldHandleSuccess = true;

                PIDLResource pidlResource = new PIDLResource
                {
                    ClientAction = new ClientAction(ClientActionType.RestAction, actionContext)
                };

                return new List<PIDLResource> { pidlResource };
            }
            else
            {
                return null;
            }
        }

        private async Task<List<string>> GetSingleMarkets(string country, EventTraceActivity traceActivityId)
        {
            var singleMarkets = await this.Settings.CatalogServiceAccessor.GetSingleMarkets(traceActivityId);

            if (singleMarkets == null)
            {
                singleMarkets = new List<string>(PIDLResourceFactory.GetCopiedDictionaryFromDomainDictionaries("MarketsEUSMD").Keys);
            }

            return singleMarkets.ConvertAll(market => market.ToLower());
        }

        private async Task<List<PIDLResource>> GetNorthstarPaymentMethodDescriptions(
                    EventTraceActivity traceActivityId,
                    string accountId,
                    string country,
                    string family,
                    string type = null,
                    string language = null,
                    string partner = Constants.ServiceDefaults.DefaultPartnerName,
                    string operation = Constants.ServiceDefaults.DefaultOperationType,
                    string classicProduct = null,
                    string billableAccountId = null,
                    bool completePrerequisites = false,
                    string piid = null,
                    string scenario = null,
                    PaymentExperienceSetting setting = null)
        {
            List<PIDLResource> pidl = await this.GetPaymentMethodDescriptionsFromFactory(
                traceActivityId,
                accountId,
                country,
                family,
                type,
                language,
                Constants.PartnerName.NorthStarWeb,
                operation,
                classicProduct,
                billableAccountId,
                completePrerequisites,
                null,
                scenario,
                setting: setting);

            foreach (PIDLResource resource in pidl)
            {
                DisplayHint saveButton = resource.GetDisplayHintById(Constants.ButtonDisplayHintIds.SaveButton) as ButtonDisplayHint;

                if (saveButton != null)
                {
                    PXCommon.RestLink restLink = saveButton.Action.Context as PXCommon.RestLink;
                    restLink.Href = restLink.Href.Replace(Constants.PartnerName.NorthStarWeb, partner);
                }
            }

            return pidl;
        }

        private async Task<List<PIDLResource>> GetPaymentMethodDescriptionsFromFactory(
            EventTraceActivity traceActivityId,
            string accountId,
            string country,
            string family,
            string type = null,
            string language = null,
            string partner = Constants.ServiceDefaults.DefaultPartnerName,
            string operation = Constants.ServiceDefaults.DefaultOperationType,
            string classicProduct = null,
            string billableAccountId = null,
            bool completePrerequisites = false,
            string piid = null,
            string scenario = null,
            string orderId = null,
            string channel = null,
            string referrerId = null,
            string sessionId = null,
            List<string> exposedFlightFeatures = null,
            PaymentExperienceSetting setting = null,
            bool isMyBaUser = false)
        {
            IEnumerable<string> allowedPaymentMethodTypes = GetAllowedPaymentMethodTypes(ref type);
            string deviceClass = await this.GetDeviceClass();

            IList<KeyValuePair<string, string>> additionalHeaders = this.GetAdditionalHeadersFromRequest();
            IEnumerable<PaymentMethod> paymentMethods = null;
            string pxChallengeSessionId = string.Empty;
            if (this.Request.Query.TryGetValue(Constants.QueryParameterName.PXChallengeSessionId, out var pxChallengeValue))
            {
                pxChallengeSessionId = pxChallengeValue.ToString();
            }

            // For Apply operations, check if the user is eligible to apply or if an error pidl should be returned
            if (string.Equals(operation, Constants.Operations.Apply, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var eligibityErrorPidl = await this.CheckApplyEligibilityAndGetErrorPidl(accountId, language, partner, setting);
                    if (eligibityErrorPidl != null)
                    {
                        // Check for scenario here
                        if (string.Equals(scenario, Constants.ScenarioNames.XboxApplyFullPageRender, StringComparison.OrdinalIgnoreCase))
                        {
                            eligibityErrorPidl.First<PIDLResource>()?.RemoveDisplayHintById(Constants.ButtonDisplayHintIds.CancelBackButton);
                            eligibityErrorPidl.First<PIDLResource>()?.RemoveDisplayHintById(Constants.ButtonDisplayHintIds.CancelButton);
                        }

                        TranslateUnicodes(eligibityErrorPidl);

                        return eligibityErrorPidl;
                    }
                    else if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner) && string.Equals(scenario, Constants.ScenarioNames.XboxCoBrandedCard, StringComparison.OrdinalIgnoreCase))
                    {
                        string ocid = this.Request.Query.TryGetValue(Constants.QueryParameterName.OCID, out var ocidVal) ? ocidVal.ToString() : string.Empty;
                        return PIDLResourceFactory.XboxCoBrandedCardQRCodeRestAction(partner, country, language, this.ExposedFlightFeatures, channel, referrerId, ocid);
                    }
                }
                catch (Exception ex)
                {
                    string exceptionMessage = $"{ex.Message}; {ex.StackTrace}";
                    SllWebLogger.TracePXServiceException($"Error occured while starting Apply flow: {exceptionMessage}.", traceActivityId);
                    throw ex;
                }
            }

            // Since PIMS currently does not support SEPA as a PI in Ireland, we need to
            // hard-code it to return a static PaymentsMethods collection (Bug 29583658)
            if (string.Equals(operation, Constants.Operations.Update, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(country, Constants.CountryCodes.Ireland, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(family, Constants.PaymentMethodFamily.direct_debit.ToString(), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(type, Constants.PaymentMethodType.Sepa, StringComparison.OrdinalIgnoreCase))
            {
                if (isMyBaUser)
                {
                    paymentMethods = await this.Settings.PIMSAccessor.GetPaymentMethods(country, family, type, language, traceActivityId, additionalHeaders, partner, this.ExposedFlightFeatures, operation, setting: setting);
                }
                else
                {
                    PaymentMethod pm = new PaymentMethod()
                    {
                        PaymentMethodId = "StaticIdPlaceholder",
                        PaymentMethodFamily = V7.Constants.PaymentMethodFamily.direct_debit.ToString(),
                        PaymentMethodType = V7.Constants.PaymentMethodType.Sepa
                    };
                    paymentMethods = new List<PaymentMethod>() { pm };
                }
            }
            else
            {
                paymentMethods = await this.Settings.PIMSAccessor.GetPaymentMethods(country, family, type, language, traceActivityId, additionalHeaders, partner, this.ExposedFlightFeatures, operation, setting: setting);
            }

            // filter payment methods when required
            if (type == null && allowedPaymentMethodTypes != null)
            {
                paymentMethods = paymentMethods.Where(i => allowedPaymentMethodTypes.Contains(i.PaymentMethodType, StringComparer.OrdinalIgnoreCase));
            }

            HashSet<PaymentMethod> paymentMethodHashSet = new HashSet<PaymentMethod>(paymentMethods);

            // For commercial stores, PxService requires partner to tell whether certain payment method is allowed.
            // Add family virtual with type invoice_basic, invoice_check, alipay and unionpay if partner requests that.
            if (string.Equals(partner, Constants.PartnerName.CommercialStores, StringComparison.OrdinalIgnoreCase)
                || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.EnableVirtualFamilyPM, country, setting))
            {
                if (string.Equals(family, Constants.PaymentMethodFamilyName.Virtual, StringComparison.OrdinalIgnoreCase) &&
                    (allowedPaymentMethodTypes.Contains(Constants.PaymentMethodType.InvoiceBasicVirtual, StringComparer.OrdinalIgnoreCase) || allowedPaymentMethodTypes.Contains(Constants.PaymentMethodType.InvoiceCheckVirtual, StringComparer.OrdinalIgnoreCase)
                    || allowedPaymentMethodTypes.Contains(Constants.PaymentMethodType.AlipayVirtual, StringComparer.OrdinalIgnoreCase) || allowedPaymentMethodTypes.Contains(Constants.PaymentMethodType.UnionpayVirtual, StringComparer.OrdinalIgnoreCase)))
                {
                    paymentMethodHashSet.Add(new PaymentMethod() { PaymentMethodFamily = family, PaymentMethodType = type });
                }
            }

            string emailAddress = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.EmailAddress);
            if (string.IsNullOrEmpty(emailAddress))
            {
                emailAddress = LocalizationRepository.Instance.GetLocalizedString("Email address", language);
            }

            List<PIDLResource> retVal = null;

            if ((string.Equals(partner, Constants.PartnerName.Xbox, StringComparison.OrdinalIgnoreCase)
                || string.Equals(partner, Constants.PartnerName.AmcXbox, StringComparison.OrdinalIgnoreCase))
                && string.Equals(family, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase)
                && PXCommonConstants.PartnerGroups.SMDEnabledPartners.Contains(partner, StringComparer.OrdinalIgnoreCase))
            {
                var singleMarkets = await this.GetSingleMarkets(country, traceActivityId);
                if (!singleMarkets.Contains(country.ToLower()))
                {
                    scenario = Constants.ScenarioNames.FixedCountrySelection;
                }
            }

            if (string.Equals(partner, Constants.PartnerName.Xbox, StringComparison.OrdinalIgnoreCase) && this.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.XboxOOBE) && string.Equals(country, Constants.CountryCodes.India, StringComparison.OrdinalIgnoreCase))
            {
                scenario = Constants.ScenarioNames.PhoneConfirm;
            }

            retVal = PIDLResourceFactory.Instance.GetPaymentMethodDescriptions(
                paymentMethodHashSet,
                country,
                family,
                type,
                operation,
                language,
                partner,
                emailAddress,
                classicProduct,
                billableAccountId,
                this.ClientContext,
                completePrerequisites,
                this.ExposedFlightFeatures,
                scenario,
                orderId,
                channel,
                referrerId,
                sessionId,
                setting: setting,
                pxChallengeSessionId: pxChallengeSessionId,
                deviceClass);

            if (string.Equals(partner, Constants.PartnerName.XboxSettings, StringComparison.OrdinalIgnoreCase))
            {
                string[] bottomControlButtonOptions =
                {
                        Constants.ButtonDisplayHintIds.NextButton,
                        Constants.ButtonDisplayHintIds.ConfirmationButton,
                        Constants.ButtonDisplayHintIds.BackButton,
                        Constants.ButtonDisplayHintIds.CancelButton,
                        Constants.ButtonDisplayHintIds.CancelBackButton,
                        Constants.ButtonDisplayHintIds.AddressPreviousButton,
                        Constants.ButtonDisplayHintIds.ViewTermsButton,
                    };

                foreach (PIDLResource resource in retVal)
                {
                    var displayPages = resource.DisplayPages;
                    foreach (var page in displayPages)
                    {
                        foreach (var buttonName in bottomControlButtonOptions)
                        {
                            var bottomControlButton = resource.GetDisplayHintFromContainer(page, buttonName) as ButtonDisplayHint;
                            if (bottomControlButton != null)
                            {
                                bottomControlButton.AddOrUpdateDisplayTag("accessibilityName", bottomControlButton.DisplayContent);
                            }
                        }
                    }
                }
            }

            if (string.Equals(partner, Constants.PartnerName.WebPay, StringComparison.InvariantCultureIgnoreCase) && string.Equals(family, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                foreach (PIDLResource resource in retVal)
                {
                    resource.SetPropertyState("country", true, new List<string>() { "cn" });
                }
            }

            if (string.Equals(family, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase)
                && ((PXCommonConstants.PartnerGroups.SMDEnabledPartners.Contains(partner, StringComparer.OrdinalIgnoreCase)
                        && !string.Equals(scenario, Constants.ScenarioNames.FixedCountrySelection, StringComparison.OrdinalIgnoreCase))
                    || this.IsSmdCommercialEnabled(partner, family, operation)))
            {
                var singleMarkets = await this.GetSingleMarkets(country, traceActivityId);
                if (singleMarkets.Contains(country.ToLower()))
                {
                    foreach (PIDLResource resource in retVal)
                    {
                        resource.SetPropertyState(Constants.DataDescriptionPropertyNames.Country, true);
                        resource.UpdateDisplayHintPossibleOptions(Constants.DataDescriptionPropertyNames.Country, singleMarkets);
                    }
                }
            }

            if (string.Equals(family, Constants.PaymentMethodFamily.invoice_credit.ToString(), StringComparison.OrdinalIgnoreCase) && string.Equals(type, Constants.PaymentMethodType.Klarna.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                foreach (PIDLResource resource in retVal)
                {
                    PropertyDescription propertyDescription = resource.GetPropertyDescriptionByPropertyName("ipAddress");
                    if (propertyDescription != null)
                    {
                        propertyDescription.DefaultValue = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.DeviceInfo.IPAddress);
                    }
                }
            }

            if (string.Equals(family, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase) && string.Equals(scenario, Constants.ScenarioNames.WithProfileAddress, StringComparison.OrdinalIgnoreCase))
            {
                // To update organization profile's address, get tenant id (org profile id)
                string tid = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.AadInfo.Tid);
                if (tid != null)
                {
                    await this.LinkProfilePIDL(retVal, tid, Constants.ProfileType.OrganizationProfile, country, partner, language, scenario, traceActivityId);
                }
            }

            if (Constants.PartnersToEnableFocusoutResolutionPolicy.Contains(partner, StringComparer.OrdinalIgnoreCase))
            {
                foreach (PIDLResource resource in retVal)
                {
                    resource.SetResolutionPolicy("accountToken", "focusout");
                }
            }

            if (string.Equals(family, Constants.PaymentMethodFamily.credit_card.ToString())
                && !this.ExposedFlightFeatures.Contains(Flighting.Features.PXIncludeUpdateAddressCheckboxInAddCC, StringComparer.OrdinalIgnoreCase)
                && !string.Equals(scenario, Constants.ScenarioNames.WithProfileAddress, StringComparison.OrdinalIgnoreCase))
            {
                foreach (PIDLResource resource in retVal)
                {
                    resource.DataDescription.Remove("update_address_enabled");
                }
            }

            if (!string.Equals(family, "ewallet", StringComparison.OrdinalIgnoreCase))
            {
                RemoveCardDisplayTransformation(retVal);
            }

            if (!PXCommonConstants.PartnerGroups.CCQuickResolutionEnabledPartners.Contains(partner, StringComparer.OrdinalIgnoreCase))
            {
                foreach (PIDLResource resource in retVal)
                {
                    resource.RemoveResolutionRegex();
                }
            }

            if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXRtlLanguages, StringComparer.OrdinalIgnoreCase)
                && this.ExposedFlightFeatures.Contains(Flighting.Features.PXAddltrinrtlTag, StringComparer.OrdinalIgnoreCase))
            {
                foreach (PIDLResource resource in retVal)
                {
                    resource.AddDisplayTag("accountToken", "ltrinrtl", "ltrinrtl");
                }
            }

            if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXUseAlignedLogos, StringComparer.OrdinalIgnoreCase))
            {
                foreach (PIDLResource resource in retVal)
                {
                    IEnumerable<LogoDisplayHint> logoHints = resource.GetDisplayHints().Where(displayHint => displayHint.DisplayHintType.Equals(HintType.Logo.ToString().ToLower())).Select(displayHint => displayHint as LogoDisplayHint);
                    foreach (LogoDisplayHint logoHint in logoHints ?? Enumerable.Empty<LogoDisplayHint>())
                    {
                        string imageName = logoHint.SourceUrl.Substring(logoHint.SourceUrl.LastIndexOf('/') + 1);
                        Tuple<string, string> alignedLogoUrl = null;
                        if (Constants.LogosWithAlignedAlternative.TryGetValue(imageName, out alignedLogoUrl))
                        {
                            logoHint.SourceUrl = logoHint.SourceUrl.Replace(imageName, this.ExposedFlightFeatures.Contains(Flighting.Features.PXRtlLanguages, StringComparer.OrdinalIgnoreCase) ? alignedLogoUrl.Item1 : alignedLogoUrl.Item2);
                        }
                    }
                }
            }

            RemoveLuhnValidation(family, partner, retVal, this.ExposedFlightFeatures);

            if (family == Constants.PaymentMethodFamily.credit_card.ToString() && this.ExposedFlightFeatures.Contains(Flighting.Features.PXUseEdgeTokenization, StringComparer.OrdinalIgnoreCase))
            {
                string[] tokenPropertyNames = { Constants.CreditCardPropertyDescriptionName.AccountToken, Constants.CreditCardPropertyDescriptionName.CvvToken, Constants.CreditCardPropertyDescriptionName.HMac };
                foreach (PIDLResource resource in retVal)
                {
                    foreach (string propName in tokenPropertyNames)
                    {
                        PropertyDescription propDescription = resource.GetPropertyDescriptionByPropertyName(propName);
                        if (propDescription != null)
                        {
                            propDescription.UseEdgeTokenization = true;
                        }
                    }
                }
            }

            if (family == Constants.PaymentMethodFamily.credit_card.ToString())
            {
                foreach (PIDLResource resource in retVal)
                {
                    if (resource.PidlResourceStrings == null)
                    {
                        resource.PidlResourceStrings = new PidlResourceStrings();
                    }

                    // Add InvalidIssuerResponse error codes to display inline message with card number field.
                    if (this.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.PXDisplay3dsNotEnabledErrorInline))
                    {
                        resource.PidlResourceStrings.AddOrUpdateServerErrorCode(
                        Constants.CreditCardErrorCodes.InvalidIssuerResponseWithTRPAU0009,
                        new ServerErrorCode()
                        {
                            Target = string.Format(
                                "{0}",
                                Constants.CreditCardErrorTargets.CardNumber),
                            ErrorMessage = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.InvalidIssuerResponse, language),
                        });

                        resource.PidlResourceStrings.AddOrUpdateServerErrorCode(
                        Constants.CreditCardErrorCodes.InvalidIssuerResponseWithTRPAU0008,
                        new ServerErrorCode()
                        {
                            Target = string.Format(
                                "{0}",
                                Constants.CreditCardErrorTargets.CardNumber),
                            ErrorMessage = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.InvalidIssuerResponse, language),
                        });
                    }

                    DescriptionHelper.AddServerErrorCodeForAddPI(resource, language, exposedFlightFeatures);

                    // TODO: Before we enable the retry on InvalidRequestData error to all partners, we need to ensure that this specific error code is not returned for anything other than token expiry.
                    // Ideally, it would be better to get specific error code for expired tokens than the generalized InvaliRequestData
                    if (Constants.PartnersToEnableRetryOnInvalidRequestData.Contains(partner, StringComparer.OrdinalIgnoreCase))
                    {
                        resource.PidlResourceStrings.AddOrUpdateServerErrorCode(
                        Constants.CreditCardErrorCodes.InvalidRequestData,
                        new ServerErrorCode()
                        {
                            Target = string.Format(
                                "{0},{1}",
                                Constants.CreditCardErrorTargets.CardNumber,
                                Constants.CreditCardErrorTargets.Cvv),
                            ErrorMessage = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.Generic, language),
                            RetryPolicy = new RetryPolicy()
                            {
                                RetryPolicyType = RetryPolicyType.limitedRetry,
                                Context = new RetryPolicyContext()
                                {
                                    MaxRetryCount = 3
                                }
                            }
                        });
                    }

                    if (string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase)
                        && (Constants.JarvisAccountIdHmacPartners.Contains(partner, StringComparer.OrdinalIgnoreCase)
                        || this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableJarvisHMAC)))
                    {
                        resource.DataDescription[Constants.JarvisAccountIdHmacProperty] = new PropertyDescription()
                        {
                            PropertyType = "clientData",
                            DataType = "hidden",
                            PropertyDescriptionType = "hidden",
                            IsUpdatable = false,
                            DefaultValue = ObfuscationHelper.GetHashValue(accountId, ObfuscationHelper.JarvisAccountIdHashSalt),
                        };
                    }
                }
            }

            if (family == Constants.PaymentMethodFamily.credit_card.ToString() && PXCommonConstants.PartnerGroups.CCTokenizationRetryEnabledPartners.Contains(partner, StringComparer.OrdinalIgnoreCase))
            {
                foreach (PIDLResource resource in retVal)
                {
                    if (resource.PidlResourceStrings == null)
                    {
                        resource.PidlResourceStrings = new PidlResourceStrings();
                    }

                    resource.PidlResourceStrings.AddOrUpdateServerErrorCode(
                        Constants.CreditCardErrorCodes.TokenizationFailed,
                        new ServerErrorCode()
                        {
                            Target = string.Format(
                                "{0}",
                                Constants.CreditCardErrorTargets.CardNumber),
                            ErrorMessage = LocalizationRepository.Instance.GetLocalizedString(Constants.CreditCardErrorMessages.ValidationFailed, language),
                            RetryPolicy = new RetryPolicy()
                            {
                                RetryPolicyType = RetryPolicyType.limitedRetry,
                                Context = new RetryPolicyContext()
                                {
                                    MaxRetryCount = 3
                                }
                            }
                        });
                }
            }

            if (Constants.PartnersToEnableReorderCCAndCardholder.Contains(partner, StringComparer.OrdinalIgnoreCase))
            {
                string accountDetailsPageDisplayName = "AccountDetailsPage";
                string numberDisplayHintId = "cardNumber";
                string amexNumberDisplayHintId = "cardNumberAmex";
                string nameDisplayHintId = "cardholderName";
                string accountSummaryPageDisplayName = "AccountSummaryPage";
                string creditCardMembersHintId = "creditCardSummaryGroup";
                string creditCardGroupLine1HintId = "creditCardSummaryLine1";
                string creditCardGroupLine2HintId = "creditCardSummaryLine2";

                foreach (PIDLResource resource in retVal)
                {
                    if (resource?.DisplayPages != null)
                    {
                        foreach (PageDisplayHint displayPage in resource.DisplayPages)
                        {
                            if (displayPage.DisplayName == accountDetailsPageDisplayName)
                            {
                                int nameMemberSequencePosition = displayPage.Members.FindIndex(displayHint => displayHint.HintId == nameDisplayHintId);
                                int numberMemberSequencePosition = (resource.Identity["type"] == "amex")
                                    ? displayPage.Members.FindIndex(displayHint => displayHint.HintId == amexNumberDisplayHintId)
                                    : displayPage.Members.FindIndex(displayHint => displayHint.HintId == numberDisplayHintId);

                                // only swap if name is currently before number
                                if (numberMemberSequencePosition != -1 && nameMemberSequencePosition != -1 && nameMemberSequencePosition < numberMemberSequencePosition)
                                {
                                    DisplayHint temp = displayPage.Members[numberMemberSequencePosition];

                                    displayPage.Members[numberMemberSequencePosition] = displayPage.Members[nameMemberSequencePosition];
                                    displayPage.Members[nameMemberSequencePosition] = temp;
                                }
                            }
                            else if (displayPage.DisplayName == accountSummaryPageDisplayName)
                            {
                                GroupDisplayHint creditCardGroupMembers = displayPage.Members.Find(displayHint => displayHint.HintId == creditCardMembersHintId) as GroupDisplayHint;

                                if (creditCardGroupMembers != null)
                                {
                                    int nameMemberSequencePosition = creditCardGroupMembers.Members.FindIndex(displayHint => displayHint.HintId == creditCardGroupLine1HintId);
                                    int numberMemberSequencePosition = creditCardGroupMembers.Members.FindIndex(displayHint => displayHint.HintId == creditCardGroupLine2HintId);

                                    // line number doesn't explicitly correspond with name and number
                                    if (numberMemberSequencePosition != -1 && nameMemberSequencePosition != -1 && nameMemberSequencePosition < numberMemberSequencePosition)
                                    {
                                        DisplayHint temp = creditCardGroupMembers.Members[numberMemberSequencePosition];

                                        creditCardGroupMembers.Members[numberMemberSequencePosition] = creditCardGroupMembers.Members[nameMemberSequencePosition];
                                        creditCardGroupMembers.Members[nameMemberSequencePosition] = temp;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Xboxnative delete flow requires dataSources to populate pidl variables. PrefillUserData removed that from the payload, therefore should be skipped.
            bool skipPrefill = string.Equals(partner, Constants.PartnerName.Wallet, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(partner, Constants.PartnerName.CommercialStores, StringComparison.InvariantCultureIgnoreCase)
                || (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner) && string.Equals(operation, Constants.Operations.Delete, StringComparison.OrdinalIgnoreCase))
                || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.DisablePrefillUserData, country, setting);
            if (!skipPrefill)
            {
                await this.PrefillUserData(retVal, accountId, country, partner, traceActivityId, completePrerequisites: completePrerequisites, billableAccountId: billableAccountId);
            }

            // TODO: As part of T-54279751 investigation, the AddressNoCityState flow found to be not used by any partner
            // And below code & csv config related to it can be removed
            if (string.Equals(scenario, Constants.ScenarioNames.AddressNoCityState, StringComparison.OrdinalIgnoreCase)
                && string.Equals(family, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase))
            {
                List<string> displayedPaymentMethodTypes = new List<string>();

                foreach (PIDLResource pidl in retVal)
                {
                    string cardType;
                    if (pidl.Identity.TryGetValue(Constants.PidlIdentityFields.Type, out cardType))
                    {
                        displayedPaymentMethodTypes.Add(cardType);
                    }

                    pidl.RemoveFirstDataDescriptionByPropertyName("address_line2");
                    pidl.RemoveFirstDataDescriptionByPropertyName("city");
                    pidl.RemoveFirstDataDescriptionByPropertyName("region");
                }

                foreach (PIDLResource pidl in retVal)
                {
                    pidl.DataDescription["displayedPaymentMethodTypes"] = new PropertyDescription()
                    {
                        PropertyType = "clientData",
                        DataType = "hidden",
                        PropertyDescriptionType = "hidden",
                        IsUpdatable = false,
                        DefaultValue = Newtonsoft.Json.JsonConvert.SerializeObject(displayedPaymentMethodTypes),
                    };
                }
            }

            if (string.Equals(scenario, Constants.ScenarioNames.DepartmentalPurchase, StringComparison.OrdinalIgnoreCase))
            {
                foreach (PIDLResource pidl in retVal)
                {
                    foreach (PageDisplayHint displayPage in pidl.DisplayPages)
                    {
                        int index = displayPage.Members.FindIndex(hint => hint.HintId == "addressCountry");
                        if (index != -1)
                        {
                            var countryHint = displayPage.Members[index];
                            displayPage.Members.RemoveAt(index);
                            displayPage.Members.Insert(0, countryHint);
                        }

                        index = displayPage.Members.FindIndex(hint => hint.HintId == "pidlContainer");
                        if (index != -1)
                        {
                            var pidlContainer = displayPage.Members[index];
                            displayPage.Members.RemoveAt(index);
                            displayPage.Members.Insert(1, pidlContainer);
                        }
                    }
                }
            }

            // Temp solution to convert year YY to YYYY
            // There are two ways to fix it in csv: 1, separate northstar cc and use expiry2, 2, move PropertyTransformation.csv to displayDescription
            // Both of them will change the PIDL structure. For now, add this transformation info in c# code.
            if (string.Equals(partner, Constants.PartnerName.NorthStarWeb, StringComparison.OrdinalIgnoreCase)
                && string.Equals(family, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase)
                && !TemplateHelper.IsTemplateBasedPIDLIncludingDefaultTemplate(ProxyController.GetSettingTemplate(partner, setting, Constants.DescriptionTypes.PaymentMethodDescription, type, family)))
            {
                foreach (PIDLResource pidl in retVal)
                {
                    PropertyDescription expiryYear = pidl.GetPropertyDescriptionByPropertyName(Constants.CreditCardPropertyDescriptionName.ExpiryYear);
                    if (expiryYear != null)
                    {
                        PropertyTransformationInfo submitTransformation = new PropertyTransformationInfo()
                        {
                            TransformCategory = "regex",
                            InputRegex = "^\\d{0,2}(\\d{2})",
                            TransformRegex = "20$1"
                        };

                        PropertyTransformationInfo displayTransformation = new PropertyTransformationInfo()
                        {
                            TransformCategory = "regex",
                            InputRegex = "^\\d{0,2}(\\d{2})",
                            TransformRegex = "$1"
                        };

                        expiryYear.AddTransformation(new Dictionary<string, PropertyTransformationInfo>()
                        {
                            { "forSubmit", submitTransformation },
                            { "forDisplay", displayTransformation }
                        });
                    }
                }
            }

            // Temp solution to change address text group id to "billingAddressShowGroup"
            // Frontend code should use accessiblity name to control the style, not display hint id
            // This logic will be removed once frontend code is completed
            if (string.Equals(partner, Constants.PartnerName.NorthStarWeb, StringComparison.OrdinalIgnoreCase)
                && (string.Equals(family, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase) || string.Equals(family, Constants.PaymentMethodFamily.direct_debit.ToString(), StringComparison.OrdinalIgnoreCase)))
            {
                foreach (PIDLResource pidl in retVal)
                {
                    string addressGroupId = string.Equals(operation, Constants.Operations.Update, StringComparison.OrdinalIgnoreCase) ? "billingAddressShowUpdateGroup" : "billingAddressShowAddGroup";
                    GroupDisplayHint showAddressGroup = pidl.GetDisplayHintById(addressGroupId) as GroupDisplayHint;

                    if (showAddressGroup != null)
                    {
                        showAddressGroup.HintId = "billingAddressShowGroup";
                    }
                }
            }

            if (PIHelper.IsPayPal(family, type) && PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner))
            {
                // Public key in CSVs is for xbox, but for xbox native partner it must be overwritten
                foreach (PIDLResource resource in retVal)
                {
                    PropertyDescription encryptedPasswordDescription = resource.GetPropertyDescriptionByPropertyName("encryptedPassword");

                    if (encryptedPasswordDescription != null)
                    {
                        encryptedPasswordDescription.DataProtection.Parameters["serialNumber"] = "00C0C88DD51847D869";

                        encryptedPasswordDescription.DataProtection.Parameters["publicKey"] = @"MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA5JKsI5gExaiOxUuRwT0mhdK46dyiYQOvLGFrRUmspxoRg86s3DuGlT9bIVl4UoGwANVY7xQvjlVXb7jy4sUXLeLxflHv2eBTfKrJHGXb2iTGlCsnoVx25CFUjXF68iDJWFjmItKySPS3GjYKxHYb0sAC+ymSvxDVd8cAquM3IHvF9mqJoisrIVc0H3ctWOPXfYl9RTNtdeKoPhHHLpqMthWJ2Hmth9LQvLj+9pAnz9aca8z5LmLQYPSaCHOoQikVSR4hJZZ0cyYw+stKLvWXnIovxP2bkIak99jnKzD0L6wWnkrpC2zDR5btqNdk38dJgKQ3VkUYqKy6KSqoVrdaRQIDAQAB";
                    }
                }
            }

            //// Add captcha challenge context in Add CC flow
            if (string.Equals(family, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase)
                && (this.ExposedFlightFeatures.Contains(Flighting.Features.PXSupportCaptchaSignalFromPIMS, StringComparer.OrdinalIgnoreCase)
                || this.ExposedFlightFeatures.Contains(Flighting.Features.PXChallengeSwitch)))
            {
                this.AddActionContext(retVal, type, partner, country, language, family, accountId);

                if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXChallengeSwitch)
                    && (this.Request.Query.TryGetValue(Constants.QueryParameterName.ShowChallenge, out var showChallengeValue)
                        && string.Equals(showChallengeValue.ToString(), bool.TrueString, StringComparison.OrdinalIgnoreCase)))
                {
                    if (!string.IsNullOrEmpty(pxChallengeSessionId))
                    {
                        try
                        {
                            await PXChallengeManagementHandler.AddChallenge(retVal, traceActivityId, partner, pxChallengeSessionId, language, this.ExposedFlightFeatures);
                        }
                        catch (ServiceErrorResponseException serviceError)
                        {
                            if (!string.IsNullOrEmpty(serviceError.Error?.ErrorCode) && serviceError.Error.ErrorCode.Equals(Constants.ChallengeManagementServiceErrorCodes.Conflict))
                            {
                                await PXChallengeManagementHandler.UpdatePXSessionAbandonedStatus(pxChallengeSessionId, traceActivityId);
                            }
                        }
                    }
                }
            }

            //// Update Cardholder name regex under the flight
            if (string.Equals(family, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase)
                && this.ExposedFlightFeatures.Contains(Flighting.Features.PXCCNameRegexUpdate, StringComparer.OrdinalIgnoreCase))
            {
                UpdateCCNameRegex(retVal);
            }

            // This code block will be deleted along with PxEnableVenmo and PxEnableSelectPMAddPIVenmo flights
            if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner)
                && string.Equals(type, Constants.PaymentMethodType.Venmo, StringComparison.OrdinalIgnoreCase)
                && string.Equals(operation, Constants.Operations.Delete, StringComparison.OrdinalIgnoreCase)
                && this.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.PxEnableVenmo, StringComparer.OrdinalIgnoreCase))
            {
                retVal[0].DataSources["paymentInstrument"].Href = retVal[0].DataSources["paymentInstrument"].Href + "?partner=" + partner;
            }

            return retVal;
        }

        /// <summary>
        /// GetDeviceClass - Get the device class from the request header.userAgent. If the device is an Xbox, we need to set the device class to Console.
        /// </summary>
        /// <returns>Possible values Web/MobileApp/GameConsole</returns>
        private async Task<string> GetDeviceClass()
        {
            // Extract the client info from the request header.userAgent. If the device is an Xbox, we need to set the device class to Console.
            string xboxDeviceId = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.DeviceInfo.XboxLiveDeviceId);
            return !string.IsNullOrWhiteSpace(xboxDeviceId) ? GlobalConstants.DeviceClass.Console : HttpRequestHelper.GetDeviceClass(this.Request.ToHttpRequestMessage());
        }

        private void AddActionContext(List<PIDLResource> pidlResources, string type, string partner, string country, string language, string family, string accountId)
        {
            IEnumerable<KeyValuePair<string, string>> queryparams = this.Request.Query.Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value.ToString()));

            Dictionary<string, string> parameters = queryparams.ToDictionary(list => list.Key, list => list.Value);

            bool anonymousPidl = string.IsNullOrEmpty(accountId);

            ActionContext optionContext = new ActionContext()
            {
                Action = PaymentInstrumentActions.ToString(PIActionType.AddResource),
                Id = $"{family}.{type}",
                PaymentMethodFamily = family,
                PaymentMethodType = type
            };

            PidlDocInfo docInfo = new PidlDocInfo(Constants.DescriptionTypes.PaymentMethodDescription, language, country, partner, type, family)
            {
                AnonymousPidl = anonymousPidl
            };

            docInfo.SetParameters(parameters);

            optionContext.ResourceActionContext = new ResourceActionContext()
            {
                Action = PaymentInstrumentActions.ToString(PIActionType.AddResource),
                PidlDocInfo = docInfo
            };

            string currentContext = JsonConvert.SerializeObject(optionContext);

            foreach (PIDLResource resource in pidlResources)
            {
                Dictionary<string, object> detailsDataDescription = resource.GetTargetDataDescription("details");

                detailsDataDescription[Constants.DataDescriptionPropertyNames.ResourceActionContext] = new PropertyDescription()
                {
                    PropertyType = "clientData",
                    DataType = "string",
                    PropertyDescriptionType = "hidden",
                    IsUpdatable = false,
                    DefaultValue = currentContext
                };
            }
        }

        private async Task LinkProfilePIDL(List<PIDLResource> pidlResource, string accountId, string profileType, string country, string partner, string language, string scenario, EventTraceActivity traceActivityId)
        {
            // Get current profile, update the shipping address with PI's billing address
            AccountOrganizationProfileV3 profile = await this.Settings.AccountServiceAccessor.GetProfileV3(accountId, profileType, traceActivityId) as AccountOrganizationProfileV3;

            if (profile != null)
            {
                // For now, this feature is only required by commercial partners
                // Will extend it to support consumer partner if required
                Dictionary<string, string> profileV3Headers = new Dictionary<string, string>();
                profileV3Headers.Add(AccountV3Headers.Etag, profile.Etag);
                profileV3Headers.Add(AccountV3Headers.IfMatch, profile.Etag);

                // Task 20658227: [PxService] Remove old "shipping_v3", "emp profile", "org profile" PIDL
                // MSFB team is in the progress of migration from profile put to profile patch, hardcode operation type to "update_partial" here to use the patch route
                List<PIDLResource> profilePidls = PIDLResourceFactory.Instance.GetProfileDescriptions(country, profileType, "update_partial", language, partner, null, profile.Id, profileV3Headers, true, this.ExposedFlightFeatures, scenario);

                // Use the first (and only) PidlResource from GetProfileDescriptions
                profilePidls[0].Identity["operation"] = "update";
                profilePidls[0].MakeSecondaryResource();
                profilePidls[0].SetErrorHandlingToIgnore();
                PIDLResourceFactory.UpdateProfilePidlSubmitUrl(profilePidls[0], profile.CustomerId);
                PIDLResourceFactory.RemoveEmptyPidlContainerHints(profilePidls);

                // For update shipping address, ignore the following two fields in profile payload
                string[] propertyNames = { "company_name", "email" };
                ProxyController.UpdateIsOptionalProperty(profilePidls, propertyNames, true);

                foreach (PIDLResource resource in pidlResource)
                {
                    // For PIDL with resource_id "credit_card.visa_with_profile_address", add org profile pidl as linked pidl.
                    string pidlResourceId = null;
                    resource.Identity.TryGetValue(Constants.PidlIdentityFields.ResourceId, out pidlResourceId);
                    if (!string.IsNullOrEmpty(pidlResourceId) && pidlResourceId.Contains("with_profile_address"))
                    {
                        PIDLResourceFactory.AddLinkedPidlToResource(resource, profilePidls[0], partner, PidlContainerDisplayHint.SubmissionOrder.AfterBase);
                    }
                }
            }
        }

        private void AddModernAVSValidationAction(List<PIDLResource> pidlResources, string partner, string family, string type, string operation, string language, string country)
        {
            const string AddressPropertyName = "address";
            const string DefaultAddressPropertyName = "default_address";

            // Add modern Validate for azure, commercialstores, and xboxnative partners
            if ((PartnerHelper.IsAzurePartner(partner) || PartnerHelper.IsCommercialStoresPartner(partner) || PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner))
                && (string.Equals(family, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase)
                    || string.Equals(family, Constants.PaymentMethodFamily.direct_debit.ToString(), StringComparison.OrdinalIgnoreCase)
                    || string.Equals(type, Constants.PaymentMethodType.LegacyInvoice, StringComparison.OrdinalIgnoreCase))
                && (string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase) || string.Equals(operation, Constants.Operations.Update, StringComparison.OrdinalIgnoreCase))
                && this.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.ShowAVSSuggestions, StringComparer.OrdinalIgnoreCase))
            {
                foreach (PIDLResource paymentMethodPidl in pidlResources)
                {
                    // Add AVS validation endpoint only if it has address property name
                    if (paymentMethodPidl != null)
                    {
                        var propertyName = paymentMethodPidl.HasDataDescriptionWithKey(AddressPropertyName) ? AddressPropertyName : (paymentMethodPidl.HasDataDescriptionWithKey(DefaultAddressPropertyName) ? DefaultAddressPropertyName : string.Empty);
                        if (!string.IsNullOrEmpty(propertyName))
                        {
                            this.AddModernValidationActionForPaymentMethodDescription(paymentMethodPidl, propertyName, Constants.AddressTypes.Internal, partner, language, country);
                        }
                    }
                }
            }
        }

        private bool IsSmdCommercialEnabled(string partner, string family, string operation)
        {
            return PXCommonConstants.PartnerGroups.CommercialSMDEnabledPartners.Contains(partner, StringComparer.OrdinalIgnoreCase)
                && string.Equals(family, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase)
                && (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.AADSupportSMD)
                    || this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableSmdCommercial, StringComparer.OrdinalIgnoreCase))
                && !this.IsPartnerFlightExposed(Constants.PartnerFlightValues.SMDDisabled);
        }

        private bool IsSingleMarketDirectiveEnabled(string operation, string family, string country, PaymentExperienceSetting setting)
        {
            return string.Equals(family, Constants.PaymentMethodFamily.credit_card.ToString(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase)
                && PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.SingleMarketDirective, country, setting)
                && !this.IsPartnerFlightExposed(Constants.PartnerFlightValues.SMDDisabled);
        }

        private bool IsInstancePIEnabled(string type, string family)
        {
            return this.ExposedFlightFeatures.Contains(V7.Constants.PartnerFlightValues.GPayApayInstancePI, StringComparer.OrdinalIgnoreCase)
                && string.Equals(family, Constants.PaymentMethodFamily.ewallet.ToString(), StringComparison.OrdinalIgnoreCase)
                && (string.Equals(type, Constants.PaymentMethodType.GooglePay.ToString(), StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.PaymentMethodType.ApplePay.ToString(), StringComparison.OrdinalIgnoreCase));
        }

        private async Task<List<PIDLResource>> CreateInstancePI(
            string accountId,
            string country,
            string type,
            string classicProduct,
            string billableAccountId,
            string operation,
            EventTraceActivity traceActivityId,
            string partner)
        {
            PIDLData pi = CreatePiPayload(country, type);

            PaymentInstrument newPI = await this.Settings.PIMSAccessor.PostPaymentInstrument(accountId, pi, traceActivityId, GetPiQueryParams(country, classicProduct, billableAccountId), null, partner, this.ExposedFlightFeatures);

            SuccessResponsePayload pidlSDKSuccessResponsePayload = new SuccessResponsePayload()
            {
                Response = newPI,
                Id = newPI?.PaymentInstrumentId,
                OperationType = operation
            };

            PIDLResource pidlResource = new PIDLResource
            {
                ClientAction = new ClientAction(ClientActionType.ReturnContext, pidlSDKSuccessResponsePayload)
            };

            return new List<PIDLResource> { pidlResource };
        }

        private bool IsGlobalPIInAddResourceEnabled(string partner, string operation, string country, PaymentExperienceSetting setting)
        {
            if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner))
            {
                return this.ExposedFlightFeatures.Contains(V7.Constants.PartnerFlightValues.EnableGlobalPiInAddResource, StringComparer.OrdinalIgnoreCase)
                    && string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase);
            }

            return ((this.IsPartnerFlightExposed(Constants.PartnerFlightValues.EnableGlobalPiInAddResource) || this.ExposedFlightFeatures.Contains(V7.Constants.PartnerFlightValues.EnableGlobalPiInAddResource, StringComparer.OrdinalIgnoreCase))
                && Constants.PartnersEnabledWithGlobalPIInAddResource.Contains(partner, StringComparer.OrdinalIgnoreCase)
                && string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase))
                || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.EnableGlobalPiInAddResource, country, setting);
        }

        private string GetOriginCountry()
        {
            string originCountryFlight = this.ExposedFlightFeatures.FirstOrDefault((f) => f.StartsWith(Constants.PartnerFlightValues.OriginCountryPrefix, StringComparison.OrdinalIgnoreCase));
            string originCountry = null;
            if (!string.IsNullOrEmpty(originCountryFlight) && originCountryFlight.Length > Constants.PartnerFlightValues.OriginCountryPrefix.Length)
            {
                originCountry = originCountryFlight.Substring(Constants.PartnerFlightValues.OriginCountryPrefix.Length).ToLowerInvariant();
            }

            return originCountry;
        }

        private void AddOrRemovePartnerFlight(string country, string flight, string countryToValidate)
        {
            if (string.Equals(country, countryToValidate, StringComparison.OrdinalIgnoreCase))
            {
                if (this.IsPartnerFlightExposed(flight))
                {
                    this.ExposedFlightFeatures.Add(flight);
                    this.RemovePartnerFlight(flight);
                }
            }
            else
            {
                this.ExposedFlightFeatures.Remove(flight);
            }
        }

        // Remove flighted features to account for styling based on pidlsdk version
        private void FlightByPidlVersion(Version fullPidlSdkVersion, Version lowestCompatiblePidlVersion, string flightName)
        {
            if (fullPidlSdkVersion < lowestCompatiblePidlVersion && this.ExposedFlightFeatures.Contains(flightName))
            {
                this.ExposedFlightFeatures.Remove(flightName);
            }
        }

        private async Task<string> GenerateSessionIdAsync(string accountId, string country, string partner, string operation, string language, EventTraceActivity traceActivityId)
        {
            QRCodeSecondScreenSession qrCodeContext = new QRCodeSecondScreenSession();
            qrCodeContext.Partner = partner;
            qrCodeContext.Country = country;
            qrCodeContext.UseCount = 0;
            qrCodeContext.Operation = operation;
            qrCodeContext.Language = language;
            qrCodeContext.AccountId = accountId;
            qrCodeContext.Email = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.EmailAddress);
            qrCodeContext.FirstName = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.FirstName);
            qrCodeContext.LastName = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.LastName);
            qrCodeContext.Status = PaymentInstrumentStatus.Pending;
            qrCodeContext.QrCodeCreatedTime = DateTime.UtcNow;
            qrCodeContext.AllowTestHeader = false;

            Microsoft.Extensions.Primitives.StringValues testHeader;
            this.Request.Headers.TryGetValue(PaymentConstants.PaymentExtendedHttpHeaders.TestHeader, out testHeader);
            if (this.ExposedFlightFeatures.Contains(V7.Constants.PartnerFlightValues.PXCOTTestAccounts, StringComparer.OrdinalIgnoreCase) || testHeader.Contains(Constants.TestAccountHeaders.MDollarPurchase))
            {
                qrCodeContext.AllowTestHeader = true;
            }

            await this.AddRiskDataToContext(qrCodeContext);

            var paymentSession = await SecondScreenSessionHandler.CreateAddCCQRCodePaymentSession(qrCodeContext, traceActivityId);

            return paymentSession.Id;
        }

        private async Task AddRiskDataToContext(QRCodeSecondScreenSession qrCodeContext)
        {
            // Add the userInfo field to the request payload
            JsonSerializerSettings serializationSettings = new JsonSerializerSettings();
            serializationSettings.NullValueHandling = NullValueHandling.Ignore;
            serializationSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            RiskData riskDataObject = new RiskData();
            riskDataObject.DeviceId = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.DeviceInfo.XboxLiveDeviceId);

            // x-ms-deviceinfo from PIFD can contain multiple IP addresses, we are using the first IP address (user's IP address), as the subsequent IP addresses could be from the other proxy layers
            string ipAddress = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.DeviceInfo.IPAddress);
            if (!string.IsNullOrEmpty(ipAddress) && ipAddress.Contains(','))
            {
                ipAddress = ipAddress.Split(',')[0];
            }

            riskDataObject.IPAddress = ipAddress;
            riskDataObject.UserAgent = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.DeviceInfo.UserAgent);

            if (string.IsNullOrWhiteSpace(riskDataObject.IPAddress) || string.IsNullOrWhiteSpace(riskDataObject.DeviceId))
            {
                // TODO: Delete once resolved, uncomment throw
                ////throw new ValidationException(ErrorCode.CannotBeNull, "Necessary risk data missing");
                SllWebLogger.TraceServerMessage(
                    "RiskDataError",
                    string.Empty,
                    null,
                    "IPAddress: " + riskDataObject.IPAddress + ", DeviceId: " + riskDataObject.DeviceId,
                    System.Diagnostics.Tracing.EventLevel.Informational);
            }

            qrCodeContext.RiskData = riskDataObject;
        }

        private async Task<List<PIDLResource>> CheckApplyEligibilityAndGetErrorPidl(string accountId, string language, string partner, PaymentExperienceSetting setting)
        {
            string errorPidlKey = string.Empty;
            string puid = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.Puid);
            if (string.IsNullOrWhiteSpace(puid))
            {
                throw new ValidationException(ErrorCode.PuidNotFound, "PUID not found");
            }

            var eligibilityResponse = await this.Settings.IssuerServiceAccessor.Eligibility(puid, Constants.PaymentMethodCardProductTypes.XboxCreditCard);
            if (eligibilityResponse == null)
            {
                // If we don't receive back a valid response, set to the internal error page
                errorPidlKey = Constants.StaticDescriptionTypes.XboxCardInternalErrorStaticPidl;
            }

            // If the response states that the user is not eligible to apply, check the ApplicationStatus enum to know which error screen to display
            if (!eligibilityResponse.EligibleToApply)
            {
                switch (eligibilityResponse.ApplicationStatus)
                {
                    case Constants.XboxCardEligibilityStatus.PendingOnApplication:
                    case Constants.XboxCardEligibilityStatus.PendingOnIssuer:
                        errorPidlKey = Constants.StaticDescriptionTypes.XboxCardPendingErrorStaticPidl;
                        break;

                    case Constants.XboxCardEligibilityStatus.Approved:
                        errorPidlKey = Constants.StaticDescriptionTypes.XboxCardApprovedErrorStaticPidl;
                        break;

                    case Constants.XboxCardEligibilityStatus.None:
                        errorPidlKey = Constants.StaticDescriptionTypes.XboxCardNotEligibleErrorStaticPidl;
                        break;

                    case Constants.XboxCardEligibilityStatus.Error:
                    default:
                        errorPidlKey = Constants.StaticDescriptionTypes.XboxCardInternalErrorStaticPidl;
                        break;
                }
            }

            if (!string.IsNullOrEmpty(errorPidlKey))
            {
                return PIDLResourceFactory.Instance.GetStaticPidlDescriptions(errorPidlKey, language, partner, flightNames: this.ExposedFlightFeatures, setting: setting);
            }

            return null;
        }

        private async Task<bool> IsRiskEligible(string family, string type, string language, string partner, string operation, List<string> billingAccountContext = null)
        {
            if (this.ExposedFlightFeatures?.Contains(V7.Constants.PartnerFlightValues.PxEnableRiskEligibilityCheck, StringComparer.OrdinalIgnoreCase) ?? false)
            {
                // For now, we will keep it to only for sepa and will expand it for other PI types if required in future.
                if (string.Equals(type, Constants.PaymentMethodType.Sepa, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase))
                {
                    string puid = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.MsaProfile.Puid);
                    string tid = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.AadInfo.Tid);
                    string oid = await this.TryGetClientContext(GlobalConstants.ClientContextKeys.AadInfo.Oid);                   

                    var browserInfo = await this.CollectBrowserInfo();
                    string deviceType = browserInfo.BrowserUserAgent;
                    string ipAddress = browserInfo.BrowserIP;
                    string locale = PidlFactory.Helper.GetCultureInfo(language)?.Name?.ToLower();

                    EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

                    IList<PaymentMethod> paymentMethods = new List<PaymentMethod>
                    {
                        new PaymentMethod { PaymentMethodType = type, PaymentMethodFamily = family }
                    };

                    string commerceRootId = null;
                    string organizationId = null;
                    string idNameSpace = string.Empty;
                    IList<PaymentMethod> paymentMethodsfromRisk = new List<PaymentMethod>();
                    if (billingAccountContext != null)
                    {
                        idNameSpace = "MCAPI";
                        commerceRootId = billingAccountContext.FirstOrDefault();
                        organizationId = billingAccountContext.LastOrDefault();

                        paymentMethodsfromRisk = await this.Settings.RiskServiceAccessor.FilterBasedOnRiskEvaluation(partner, puid, tid, oid, idNameSpace, commerceRootId, organizationId, paymentMethods, ipAddress, locale, deviceType, traceActivityId);
                    }
                    else
                    {
                        paymentMethodsfromRisk = await this.Settings.RiskServiceAccessor.FilterBasedOnRiskEvaluation(partner, puid, tid, oid, paymentMethods, ipAddress, locale, deviceType, traceActivityId);
                    }

                    if (paymentMethodsfromRisk.Count == 0)
                    {
                        // risk eligibility check failed, retrun a failure.
                        return false;
                    }
                }
            }

            return true;
        }

        private async Task<QRCodeSecondScreenSession> GetAndValidateQrCodeSession(string sessionId, EventTraceActivity traceActivityId, List<PaymentMethod> paymentMethodList)
        {
            // Figure out if the Session is still valid
            QRCodeSecondScreenSession qrCodePaymentSessionData = new QRCodeSecondScreenSession();

            qrCodePaymentSessionData = await this.SecondScreenSessionHandler.GetQrCodeSessionData(sessionId, traceActivityId);

            if (!PIHelper.IsQrCodeValidSession(qrCodePaymentSessionData) || paymentMethodList == null || paymentMethodList.Count == 0)
            {
                await this.SecondScreenSessionHandler.UpdateQrCodeSessionResourceData(qrCodePaymentSessionData.UseCount, qrCodePaymentSessionData, traceActivityId, status: PaymentInstrumentStatus.Cancelled, null);
                throw new ValidationException(ErrorCode.RequestFailed, "sessionIdData is incorrect");
            }

            return qrCodePaymentSessionData;
        }

        private async void UpdateSessionDataForSecondScreen(QRCodeSecondScreenSession qrCodePaymentSessionData, EventTraceActivity traceActivityId)
        {
            qrCodePaymentSessionData.FormRenderedTime = DateTime.UtcNow;

            await SecondScreenSessionHandler.UpdateQrCodeSessionResourceData(qrCodePaymentSessionData.UseCount + 1, qrCodePaymentSessionData, traceActivityId);
        }
    }
}