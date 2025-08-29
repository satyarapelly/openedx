// <copyright file="InitializationController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService;
    using Microsoft.Commerce.Payments.PXService.V7.Contexts;
    using Newtonsoft.Json;
    using Tracing;
    using UUIDNext;

    public class InitializationController : ProxyController
    {
        /// <summary>
        /// Payment client initialize - PIDLDocIn/PIDL for each payment client components
        /// </summary>
        /// <group>PaymentClient</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/PaymentClient/Initialize</url>
        /// <param name="initializeData">Initialize data</param>
        /// <response code="200">An Initialize object</response>
        /// <returns>A Initialize result include all payment client component props</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> Initialize(
            [FromBody] PIDLData initializeData)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(Constants.Operations.Initialize);

            // Extract request context - payment/checkout/wallet
            RequestContext requestContext = this.GetRequestContext(traceActivityId);

            if (requestContext == null)
            {
                throw new System.Exception("Failed to get request context from the request headers");
            }

            string partner = null;
            if (!PXService.PartnerSettingsHelper.TenantIdPartnerNameMapper.TryGetValue(requestContext.TenantId, out partner))
            {
                partner = requestContext.TenantId;
            }

            CheckoutRequestClientActions checkoutRequestClientActions = null;
            PaymentRequestClientActions paymentRequestClientActions = null;
            List<PIDLResource> retVal = null;
            string requestType = V7.Contexts.RequestContext.GetRequestType(requestContext);

            string paymentRequestClientActionsString = initializeData?.TryGetPropertyValueFromPIDLData(V7.Constants.PIDLDataPropertyNames.PaymentRequestClientActions);

            if (this.UsePaymentRequestApiEnabled() && PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.PaymentClientHandlePaymentCollection, null, setting))
            {
                if (this.ExposedFlightFeatures?.Contains(PXCommon.Flighting.Features.IncludePIDLDescriptionsV2, StringComparer.OrdinalIgnoreCase) ?? false)
                {
                    paymentRequestClientActions = await this.GetPaymentRequestClientActions(initializeData, requestContext, traceActivityId);
                }
                else if (string.IsNullOrEmpty(paymentRequestClientActionsString))
                {
                    paymentRequestClientActions = await this.Settings.PaymentOrchestratorServiceAccessor.GetClientActionForPaymentRequest(traceActivityId, requestContext?.RequestId);
                }
                else
                {
                    try
                    {
                        paymentRequestClientActions = JsonConvert.DeserializeObject<PaymentRequestClientActions>(paymentRequestClientActionsString);
                    }
                    catch (JsonException ex)
                    {
                        throw new InvalidOperationException($"Failed to deserialize PaymentRequestClientActions from PIDLData: {paymentRequestClientActionsString}", ex);
                    }
                }

                // Component description Generation
                retVal = InitializationHandler.GetDescription(requestContext?.RequestId, paymentRequestClientActions, partner, setting, this.ExposedFlightFeatures, initializeData: initializeData);
            }
            else if (requestType == V7.Constants.RequestContextType.Checkout)
            {
                checkoutRequestClientActions = await this.Settings.PaymentOrchestratorServiceAccessor.GetClientAction(traceActivityId, requestContext?.RequestId);

                // Component description Generation
                retVal = InitializationHandler.GetDescription(requestContext?.RequestId, checkoutRequestClientActions, partner, setting, this.ExposedFlightFeatures, initializeData: initializeData);
            }
            else
            {
                retVal = InitializationHandler.GetDescription(requestContext?.RequestId, checkoutRequestClientActions, partner, setting, this.ExposedFlightFeatures, initializeData: initializeData);
            }

            if (this.ExposedFlightFeatures?.Contains(PXCommon.Flighting.Features.PXEnableAllComponentDescriptions, StringComparer.OrdinalIgnoreCase) ?? false)
            {
                string challengeWindowSize = initializeData?.TryGetPropertyValue(V7.Constants.PIDLDataPropertyNames.ComponentsDataConfirmWindowSize);

                if (this.UsePaymentRequestApiEnabled() && PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.PaymentClientHandlePaymentCollection, null, setting))
                {
                    // If payment request API is enabled, we need to get the payment request client actions
                    await this.GetEligibleComponentDescriptions(retVal, partner, requestContext, traceActivityId, setting, checkoutRequestClientActions, paymentRequestClientActions, challengeWindowSize: challengeWindowSize);
                }
                else if ((this.UsePaymentRequestApiEnabled() && !PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.PaymentClientHandlePaymentCollection, null, setting))
                    || requestType == V7.Constants.RequestContextType.Payment)
                {
                    // For current battle next flow, we need to do confirm instead of get client actions
                    // If payment request API is enabled but the PaymentClientHandlePaymentCollection feature is not enabled, we need to do follow the same logic of the current battle net flow
                    // We will remove "requestType == V7.Constants.RequestContextType.Payment" logic once cr and pr is merged. Then whether to do confirm or not will be controlled by the feature flag.
                    paymentRequestClientActions = await this.Settings.PaymentOrchestratorServiceAccessor.PaymentRequestConfirm(requestContext?.RequestId, traceActivityId);

                    await this.GetEligibleComponentDescriptions(retVal, partner, requestContext, traceActivityId, setting, checkoutRequestClientActions, paymentRequestClientActions, challengeWindowSize: challengeWindowSize);
                }
                else
                {
                    // For the currrent candy crush flow, we don't need to make the confirm call
                    await this.GetEligibleComponentDescriptions(retVal, partner, requestContext, traceActivityId, setting, checkoutRequestClientActions, paymentRequestClientActions, challengeWindowSize: challengeWindowSize);
                }
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, retVal);
        }

        private static string GetOperationByResourceType(string type)
        {
            switch (type)
            {
                case "addResource":
                    return V7.Constants.Operations.Add;
                case "selectResource":
                    return V7.Constants.Operations.SelectInstance;
                default:
                    throw new ArgumentException($"Unsupported action Type: {type}");
            }
        }

        private static PaymentRequestContext GetPaymentRequestContext(PIDLData initializeData)
        {
            if (initializeData != null)
            {
                var paymentRequestContextString = JsonConvert.SerializeObject(initializeData);
                if (!string.IsNullOrEmpty(paymentRequestContextString))
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<PaymentRequestContext>(paymentRequestContextString);
                    }
                    catch (JsonException ex)
                    {
                        throw new InvalidOperationException($"Failed to deserialize PaymentRequestContext from PIDLData: {paymentRequestContextString}", ex);
                    }
                }
            }

            return new PaymentRequestContext();
        }

        private static string GetPaymentAccountId(RequestContext requestContext)
        {
            if (requestContext != null && requestContext.TenantCustomerId != null)
            {
                var prefix = "ac_";
                var name = requestContext.TenantCustomerId;
                var namespaceId = Guid.Parse(requestContext.TenantId.Substring("tn_".Length));

                return prefix + Uuid.NewNameBased(namespaceId, name).ToString("N");
            }

            return null;
        }

        /// <summary>
        /// Gets component descriptions for all available components
        /// </summary>
        /// <param name="retVal">Current PIDL description</param>
        /// <param name="partner">partner name</param>
        /// <param name="requestContext">Request context</param>
        /// <param name="traceActivityId">TraceActivity object</param>
        /// <param name="setting">Payment Experience Setting</param>
        /// <param name="checkoutRequestClientActions">Checkout request client action object</param>
        /// <param name="paymentRequestClientActions">Payment request client action object</param>
        /// <param name="challengeWindowSize">Window size value</param>
        /// <returns>Task no actual values</returns>
        private async Task GetEligibleComponentDescriptions(
        List<PIDLResource> retVal,
        string partner,
        RequestContext requestContext,
        EventTraceActivity traceActivityId,
        PaymentExperienceSetting setting,
        CheckoutRequestClientActions checkoutRequestClientActions = null,
        PaymentRequestClientActions paymentRequestClientActions = null,
        string challengeWindowSize = null)
        {
            if (checkoutRequestClientActions == null && paymentRequestClientActions == null)
            {
                throw new System.Exception("Initialize:GetAllDescription - Both checkoutRequestClientActions and paymentRequestClientActions cannot be null.");
            }

            string country = null;
            string language = null;
            string scenario = null;

            var components = new Dictionary<string, string>();

            if (checkoutRequestClientActions != null
                || (this.UsePaymentRequestApiEnabled()
                && PidlFactory.V7.PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PidlFactory.V7.PartnerSettingsHelper.Features.PaymentClientHandlePaymentCollection, country, setting)))
            {
                components = new Dictionary<string, string>()
                {
                    { V7.Constants.Component.OrderSummary, V7.Constants.Operations.Show }
                };

                var paymentMethods = QuickPaymentDescription.GetQuickPaymentMethods(this.UsePaymentRequestApiEnabled() ? paymentRequestClientActions?.PaymentMethodResults?.PaymentMethods : checkoutRequestClientActions?.PaymentMethodResults?.PaymentMethods);
                if (paymentMethods?.Count > 0)
                {
                    components.Add(V7.Constants.Component.QuickPayment, V7.Constants.Operations.ExpressCheckout);
                }

                var filteredPMs = PaymentDescription.GetFilteredPaymentMethods(this.UsePaymentRequestApiEnabled() ? paymentRequestClientActions?.PaymentMethodResults?.PaymentMethods : checkoutRequestClientActions?.PaymentMethodResults?.PaymentMethods);
                if (filteredPMs?.Count > 0)
                {
                    components.Add(V7.Constants.Component.Payment, V7.Constants.Operations.Select);

                    if (PaymentDescription.GetFilteredPaymentInstruments(this.UsePaymentRequestApiEnabled() ? paymentRequestClientActions?.PaymentMethodResults?.PaymentInstruments : checkoutRequestClientActions?.PaymentMethodResults?.PaymentInstruments)?.Count > 0)
                    {
                        scenario = AddressDescription.AddressScenario;
                    }
                }

                components.Add(V7.Constants.Component.Address, V7.Constants.Operations.Add);
                components.Add(V7.Constants.Component.Profile, V7.Constants.Operations.Add);
                components.Add(V7.Constants.Component.Confirm, V7.Constants.Operations.Add);
            }
            else if (paymentRequestClientActions != null)
            {
                country = paymentRequestClientActions?.Country;
                language = paymentRequestClientActions?.Language;

                components = new Dictionary<string, string>()
                {
                    { V7.Constants.Component.Confirm, V7.Constants.Operations.Add }
                };
            }

            // Process each component to get its descriptions
            foreach (var componentType in components)
            {
                var descriptions = await this.GetDescriptionsForEeligibleComponents(
                    partner,
                    componentType.Value,
                    componentType.Key,
                    scenario,
                    null,
                    null,
                    requestContext,
                    traceActivityId,
                    checkoutRequestClientActions,
                    paymentRequestClientActions,
                    challengeWindowSize);

                if (descriptions != null)
                {
                    retVal?.ForEach(val =>
                    {
                        if (val?.InitializeContext.Components == null)
                        {
                            val.InitializeContext.Components = new Dictionary<string, object>();
                        }

                        val.InitializeContext.Components.Add(componentType.Key, descriptions);
                    });
                }

                if (this.ExposedFlightFeatures?.Contains(PXCommon.Flighting.Features.PXEnableCachedPrefetcherData, StringComparer.OrdinalIgnoreCase) ?? false
                    && componentType.Key.Equals(V7.Constants.Component.Payment))
                {
                    foreach (var pidlDesc in descriptions)
                    {
                        if (pidlDesc?.PIDLInstanceContexts != null)
                        {
                            foreach (var pidlInstanceContext in pidlDesc.PIDLInstanceContexts)
                            {
                                PidlCacheKey pidlCacheKey = new PidlCacheKey()
                                {
                                    OperationType = GetOperationByResourceType(pidlInstanceContext.Value.Action),
                                    ResourceType = pidlInstanceContext.Value.PidlDocInfo.ResourceType,
                                    PidlDocInfo = pidlInstanceContext.Value.PidlDocInfo,
                                };

                                string partnerName = null;
                                pidlInstanceContext.Value.PidlDocInfo.Parameters?.TryGetValue(V7.Constants.QueryParameterName.Partner, out partnerName);
                                string componentName = null;
                                pidlInstanceContext.Value.PidlDocInfo.Parameters?.TryGetValue(V7.Constants.QueryParameterName.Component, out componentName);
                                string scenarioName = null;
                                pidlInstanceContext.Value.PidlDocInfo.Parameters?.TryGetValue(V7.Constants.QueryParameterName.Scenario, out scenarioName);
                                string family = null;
                                pidlInstanceContext.Value.PidlDocInfo.Parameters?.TryGetValue(V7.Constants.QueryParameterName.Family, out family);
                                string type = null;
                                pidlInstanceContext.Value.PidlDocInfo.Parameters?.TryGetValue(V7.Constants.QueryParameterName.Type, out type);

                                var pidldesc = await this.GetDescriptionsForEeligibleComponents(
                                    partnerName,
                                    GetOperationByResourceType(pidlInstanceContext.Value.Action),
                                    componentName,
                                    scenarioName,
                                    family,
                                    type,
                                    requestContext,
                                    traceActivityId,
                                    checkoutRequestClientActions,
                                    paymentRequestClientActions,
                                    challengeWindowSize);

                                if (pidldesc != null)
                                {
                                    retVal?.ForEach(val =>
                                    {
                                        if (val.InitializeContext.CachedPrefetcherData == null)
                                        {
                                            val.InitializeContext.CachedPrefetcherData = new Dictionary<string, object>();
                                        }

                                        val.InitializeContext.CachedPrefetcherData.Add(JsonConvert.SerializeObject(pidlCacheKey), pidldesc);
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }

        private async Task<List<PIDLResource>> GetDescriptionsForEeligibleComponents(string partner, string operation, string component, string scenario, string family, string type, RequestContext requestContext, EventTraceActivity traceActivityId, CheckoutRequestClientActions checkoutRequestClientActions, PaymentRequestClientActions paymentRequestClientActions, string challengeWindowSize = null)
        {
            ComponentDescription componentInstance = null;
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(operation);

            // Create component description instance for each type
            if (this.UsePaymentRequestApiEnabled())
            {
                componentInstance = ComponentDescriptionFactory.CreateInstance(component, requestContext, setting, this.Generate3DS2ChallengePIDLResource);
            }
            else
            {
                componentInstance = ComponentDescriptionFactory.CreateInstance(component, requestContext, this.Generate3DS2ChallengePIDLResource);
            }

            if (componentInstance != null)
            {
                // Load component description properties
                componentInstance.LoadComponentsData(requestContext?.RequestId, this.Settings, traceActivityId, setting, this.ExposedFlightFeatures, operation: operation, partner: partner, family: family, type: type, scenario: scenario, request: this.Request, checkoutRequestClientActions: checkoutRequestClientActions, paymentRequestClientActions: paymentRequestClientActions, challengeWindowSize: challengeWindowSize);
                this.EnableFlightingsInPartnerSetting(componentInstance.PSSSetting, componentInstance.Country);

                // Component description Generation
                List<PIDLResource> descriptions = await componentInstance.GetDescription();

                // Post Porcess
                FeatureContext featureContext = new FeatureContext(
                        componentInstance.Country,
                        GetSettingTemplate(partner, componentInstance.PSSSetting, componentInstance.DescriptionType),
                        componentInstance.DescriptionType,
                        operation,
                        componentInstance.Scenario,
                        componentInstance.Language,
                        null,
                        this.ExposedFlightFeatures,
                        componentInstance.PSSSetting?.Features,
                        componentInstance.Family,
                        componentInstance.PaymentMethodType,
                        originalPartner: partner,
                        isGuestAccount: GuestAccountHelper.IsGuestAccount(this.Request),
                        defaultPaymentMethod: null,
                        xmsFlightHeader: this.GetPartnerXMSFlightExposed(),
                        tokenizationPublicKey: await this.GetTokenizationPublicKey(traceActivityId),
                        tokenizationServiceUrls: this.Settings.TokenizationServiceAccessor.GetTokenizationServiceUrls());

                PostProcessor.Process(descriptions, PIDLResourceFactory.FeatureFactory, featureContext);

                return descriptions;
            }

            return null;
        }

        private async Task<PaymentRequestClientActions> GetPaymentRequestClientActions(PIDLData initializeData, RequestContext requestContext, EventTraceActivity eventTraceActivity)
        {
            var paymentRequestClientActions = new PaymentRequestClientActions();
            if (initializeData != null)
            {
                var paymentRequestContext = GetPaymentRequestContext(initializeData);
                paymentRequestClientActions.PaymentRequestId = requestContext.RequestId;
                paymentRequestClientActions.Country = paymentRequestContext?.Country;
                paymentRequestClientActions.Language = paymentRequestContext?.Language;
                paymentRequestClientActions.Currency = paymentRequestContext?.Currency;
                paymentRequestClientActions.Amount = paymentRequestContext?.Amount ?? 0;

                var paymentAccountId = GetPaymentAccountId(requestContext);
                paymentRequestClientActions.Capabilities = paymentRequestContext?.Capabilities;
                var allowedPaymentMethods = paymentRequestContext?.MerchantAccountProfile?.AllowedPaymentMethods;

                try
                {
                    var paymentMethodsResult = await this.Settings.PIMSAccessor.GetEligiblePaymentMethods(paymentRequestClientActions.Country, paymentRequestClientActions.Amount, paymentAccountId, allowedPaymentMethods?.ToList(), eventTraceActivity);
                    paymentRequestClientActions.PaymentMethodResults = paymentMethodsResult;
                }
                catch (Exception ex)
                {
                    ////TODO : clean this up once PIMS resolve the issue to make PX call GetEligiblePaymentMethods
                    SllWebLogger.TracePXServiceException("Exception caught in InitializationController.GetPaymentRequest : " + ex.ToString(), eventTraceActivity);
                    var defaultResponseRaw = "{\"paymentMethods\":[{\"paymentMethodType\":\"visa\",\"paymentMethodFamily\":\"credit_card\",\"display\":{\"name\":\"Visa\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_visa_rect.png\",\"logos\":[{\"mimeType\":\"image/png\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_visa_rect.png\"},{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_visa.svg\"}]},\"properties\":{\"offlineRecurring\":true,\"userManaged\":true,\"soldToAddressRequired\":true,\"chargeThresholds\":[],\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"moBillingIdentityUrl\":\"\",\"riskyPaymentMethod\":false,\"authWindow\":10080,\"fundsAvailabilityWindow\":0,\"multipleLineItemsSupported\":true,\"splitPaymentSupported\":true,\"purchaseWaitTime\":0,\"redirectRequired\":[],\"reAuthWindows\":[129600]},\"exclusionTags\":[]},{\"paymentMethodType\":\"amex\",\"paymentMethodFamily\":\"credit_card\",\"display\":{\"name\":\"American Express\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_amex_rect.png\",\"logos\":[{\"mimeType\":\"image/png\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_amex_rect.png\"},{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_amex.svg\"}]},\"properties\":{\"offlineRecurring\":true,\"userManaged\":true,\"soldToAddressRequired\":true,\"chargeThresholds\":[],\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"moBillingIdentityUrl\":\"\",\"riskyPaymentMethod\":false,\"authWindow\":10080,\"fundsAvailabilityWindow\":0,\"multipleLineItemsSupported\":true,\"splitPaymentSupported\":true,\"purchaseWaitTime\":0,\"redirectRequired\":[],\"reAuthWindows\":[10080,10080,10080,10080,10080,10080,10080,10080,10080,10080,10080,10080]},\"exclusionTags\":[]},{\"paymentMethodType\":\"mc\",\"paymentMethodFamily\":\"credit_card\",\"display\":{\"name\":\"MasterCard\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_mc_rect.png\",\"logos\":[{\"mimeType\":\"image/png\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_mc_rect.png\"},{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_mc.svg\"}]},\"properties\":{\"offlineRecurring\":true,\"userManaged\":true,\"soldToAddressRequired\":true,\"chargeThresholds\":[],\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"moBillingIdentityUrl\":\"\",\"riskyPaymentMethod\":false,\"authWindow\":10080,\"fundsAvailabilityWindow\":0,\"multipleLineItemsSupported\":true,\"splitPaymentSupported\":true,\"purchaseWaitTime\":0,\"redirectRequired\":[],\"reAuthWindows\":[43200,43200,43200,43200,43200]},\"exclusionTags\":[]},{\"paymentMethodType\":\"discover\",\"paymentMethodFamily\":\"credit_card\",\"display\":{\"name\":\"Discover Network\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_discover_rect.png\",\"logos\":[{\"mimeType\":\"image/png\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_discover_rect.png\"},{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_discover.svg\"}]},\"properties\":{\"offlineRecurring\":true,\"userManaged\":true,\"soldToAddressRequired\":true,\"chargeThresholds\":[],\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"moBillingIdentityUrl\":\"\",\"riskyPaymentMethod\":false,\"authWindow\":10080,\"fundsAvailabilityWindow\":0,\"multipleLineItemsSupported\":true,\"splitPaymentSupported\":true,\"purchaseWaitTime\":0,\"redirectRequired\":[],\"reAuthWindows\":[]},\"exclusionTags\":[]},{\"paymentMethodType\":\"applepay\",\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"ApplePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_applepay.svg\"}]},\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":[],\"redirectRequired\":[],\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"nonStoredPaymentMethodId\":null,\"isNonStoredPaymentMethod\":false},\"exclusionTags\":[]},{\"paymentMethodType\":\"googlepay\",\"paymentMethodFamily\":\"ewallet\",\"display\":{\"name\":\"GooglePay\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\",\"logos\":[{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_googlepay.svg\"}]},\"properties\":{\"offlineRecurring\":true,\"userManaged\":false,\"chargeThresholds\":[],\"redirectRequired\":[],\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false,\"nonStoredPaymentMethodId\":null,\"isNonStoredPaymentMethod\":false},\"exclusionTags\":[]}],\"paymentInstruments\":[],\"defaultPaymentInstrument\":null}";
                    paymentRequestClientActions.PaymentMethodResults = JsonConvert.DeserializeObject<PaymentMethodResult>(defaultResponseRaw);
                }
            }

            return paymentRequestClientActions;
        }
    }
}