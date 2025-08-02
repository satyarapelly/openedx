// <copyright file="AddressesExController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Metrics;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Accessors.AddressEnrichmentService.DataModel;
    using Common.Tracing;
    using Common.Web;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.Pidl.Localization;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess;
    using PidlFactory.V7;
    using PidlModel.V7;
    using PimsModel.V4;
    using PXCommon;

    public class AddressesExController : ProxyController
    {
        /// <summary>
        /// Get Address by Id
        /// </summary>
        /// <group>AddressesEx</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/AddressesEx</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="addressId" required="true" cref="string" in="query">address id</param>
        /// <response code="200">An address object</response>
        /// <returns>An address object</returns>
        [HttpGet]
        public async Task<HttpResponseMessage> GetAddressById(string accountId, string addressId)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            AddressInfoV3 address = await this.Settings.AccountServiceAccessor.GetAddress<AddressInfoV3>(accountId, addressId, GlobalConstants.AccountServiceApiVersion.V3, traceActivityId);

            return this.Request.CreateResponse(address);
        }

        /// <summary>
        /// Post an address
        /// </summary>
        /// <group>AddressesEx</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/AddressesEx</url>
        /// <param name="address" required="true" cref="object" in="body">address object</param>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="partner" required="true" cref="string" in="query">partner name</param>
        /// <param name="language" required="true" cref="string" in="query">language code</param>
        /// <param name="avsSuggest" required="true" cref="bool" in="query">a boolean value to indicate whehter avs suggestion should be shown or not</param>
        /// <param name="scenario" required="false" cref="string" in="query">scenario name</param>
        /// <response code="200">An address object</response>
        /// <returns>An address object</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> Post(
            [FromBody]PIDLData address, 
            string accountId, 
            string partner, 
            string language, 
            bool avsSuggest,
            string scenario = null)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddPartnerProperty(partner?.ToLower());
            this.Request.AddAvsSuggest(avsSuggest);
            HttpResponseMessage response;

            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(Constants.Operations.Add) ?? this.GetPaymentExperienceSetting(Constants.Operations.Update);

            PXAddressV3Info userEnteredAddress = new PXAddressV3Info(address);

            if (ShouldValidateCountryAddress(userEnteredAddress.Country) && avsSuggest)
            {
                var type = address.ContainsKey("addressType") ? address["addressType"].ToString() : Constants.AddressTypes.PXV3;
                try
                {
                    response = await this.Suggest(accountId, userEnteredAddress, partner, language, traceActivityId, type, scenario, setting);
                }
                catch (ServiceErrorResponseException ex)
                {
                    return this.Request.CreateResponse(ex.Response.StatusCode, ex.Error, "application/json");
                }
            }
            else
            {
                if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXRateLimitPerAccount))
                {
                    var serviceErrorResp = GenerateBadRequestServiceErrorResponse();
                    this.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.Message] = $"{GlobalConstants.AbnormalDetection.LogMsgWhenCaughtByPX} " +
                    $"flight {Flighting.Features.PXRateLimitPerAccount} limited by accountId {accountId}";
                    return this.Request.CreateResponse(System.Net.HttpStatusCode.BadRequest, serviceErrorResp, GlobalConstants.HeaderValues.JsonContent);
                }

                // Added the Inline PSS feature UseLegacyAccountAndSync to the condition to check if the legacy account should be created and synced
                int syncToLegacyCode = DetermineSyncToLegacyCode(partner, scenario, userEnteredAddress, setting);
                try
                {
                    response = await this.PostAddress(userEnteredAddress, accountId, traceActivityId, syncToLegacyCode);
                }
                catch (ServiceErrorResponseException ex)
                {
                    return this.Request.CreateResponse(ex.Response.StatusCode, ex.Error, "application/json");
                }
            }

            return response;
        }

        /// <summary>
        /// Edit an address
        /// </summary>
        /// <group>AddressesEx</group>
        /// <verb>PATCH</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/AddressesEx</url>
        /// <param name="address" required="true" cref="object" in="body">address object</param>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="addressId" required="true" cref="string" in="query">address id</param>
        /// <param name="partner" required="true" cref="string" in="query">partner name</param>
        /// <param name="scenario" required="false" cref="string" in="query">scenario name</param>
        /// <response code="200">An address object</response>
        /// <returns>An address object</returns>
        [HttpPatch]
        public async Task<HttpResponseMessage> Patch(
            [FromBody] PIDLData address, 
            string accountId, 
            string addressId, 
            string partner,
            string scenario = null)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddPartnerProperty(partner?.ToLower());
            
            // Todo: Add Scenario to Log and all the controllers
            this.Request.AddScenarioProperty(scenario?.ToLower());

            Microsoft.Extensions.Primitives.StringValues etags;

            string etag = null;
            if (this.Request.Headers.TryGetValue(GlobalConstants.HeaderValues.IfMatch, out etags))
            {
                etag = etags.FirstOrDefault();
            }

            if (address.Count != 1 || !address.ContainsKey(GlobalConstants.CMAddressV3Fields.IsUserConsented))
            {
                throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, $"only {GlobalConstants.CMAddressV3Fields.IsUserConsented} is allowed"));
            }

            AddressInfoV3 savedAddress = await this.Settings.AccountServiceAccessor.PatchAddress(
                accountId,
                addressId,
                new AddressInfoV3()
                {
                    IsCustomerConsented = (bool)address[GlobalConstants.CMAddressV3Fields.IsUserConsented]
                },
                etag,
                traceActivityId);

            ClientAction clientAction = new ClientAction(ClientActionType.ReturnContext, savedAddress);
            PIDLResource clientActionPidl = new PIDLResource();
            clientActionPidl.ClientAction = clientAction;
            return this.Request.CreateResponse(clientActionPidl);
        }

        private static bool ShouldValidateCountryAddress(string country)
        {
            return string.Equals(country, Constants.CountryCodes.Australia, StringComparison.OrdinalIgnoreCase)
                || string.Equals(country, Constants.CountryCodes.Canada, StringComparison.OrdinalIgnoreCase)
                || string.Equals(country, Constants.CountryCodes.UnitedStates, StringComparison.OrdinalIgnoreCase);
        }

        private static bool ShouldCreateLegacyAccountAndSync(string partner, string scenario)
        {         
            return string.Equals(partner, Constants.PartnerName.AmcWeb, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(scenario, Constants.ScenarioNames.ProfileAddress, StringComparison.OrdinalIgnoreCase);
        }

        private static ServiceErrorResponse GenerateBadRequestServiceErrorResponse()
        {
            var serviceErrorResponse = new ServiceErrorResponse(
                            errorCode: ErrorConstants.ErrorCodes.InvalidOperationException,
                            message: "Receive a bad request response from Account service: {\"error_code\":\"InvalidProperty\",\"message\":\"A property has invalid data.\",\"parameters\":{\"property_name\":\"invalid_address_fields_combination\",\"details\":\"VERAZIP: Invalid state code/ZIP code/city name combinations. Both state code/ZIP code and state code/city name were incorrect.\"},\"object_type\":\"Error\"}.");

            serviceErrorResponse.CorrelationId = Guid.NewGuid().ToString();

            return serviceErrorResponse;
        }

        private static int DetermineSyncToLegacyCode(string partner, string scenario, PXAddressV3Info userEnteredAddress, PaymentExperienceSetting setting)
        {
            if (ShouldCreateLegacyAccountAndSync(partner, scenario)
               || (PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseLegacyAccountAndSync, userEnteredAddress.Country, setting)
               && string.Equals(scenario, Constants.ScenarioNames.ProfileAddress, StringComparison.OrdinalIgnoreCase)))
            {
                return V7.Constants.SyncToLegacyCodes.CreateLegacyAccountAndSync;
            }

            return V7.Constants.SyncToLegacyCodes.NoSyncAndValidation;
        }

        private async Task<HttpResponseMessage> Suggest(
            string accountId, 
            PXAddressV3Info userEnteredAddress, 
            string partner, 
            string language, 
            EventTraceActivity traceActivityId, 
            string addressType = Constants.AddressTypes.PXV3,
            string scenario = null,
            PaymentExperienceSetting setting = null)
        {
            string incomingAddressId = userEnteredAddress.Id;
            userEnteredAddress.Id = PidlFactory.GlobalConstants.SuggestedAddressesIds.UserEntered;
            var addressSuggestionFeatureValue = new Dictionary<string, object>
            {
                { Constants.DisplayCustomizationDetail.AddressSuggestion, true },
                { Constants.DisplayCustomizationDetail.SubmitActionType, Constants.DisplayCustomizationDetail.AddressEx },
            };

            var operation = (Constants.AvsSuggestEnabledPartners.Contains(partner)
                            || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(
                                PartnerSettingsHelper.Features.CustomizeAddressForm,
                                userEnteredAddress.Country,
                                setting,
                                addressSuggestionFeatureValue))
                            ? Constants.Operations.ValidateInstance : Constants.Operations.Add;

            userEnteredAddress.Id = PidlFactory.GlobalConstants.SuggestedAddressesIds.UserEntered;

           setting = this.GetPaymentExperienceSetting(operation);

            List<PIDLResource> pidls = new List<PIDLResource>();

            if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner))
            {
                var setAsDefaultBilling = false;

                Address enrichmentReqAddress = new Address(userEnteredAddress);

                if (string.Equals(scenario, Constants.ScenarioNames.Profile, StringComparison.OrdinalIgnoreCase))
                {
                    setAsDefaultBilling = true;
                }

                // run legacy AVS check here, if passed, continue as normal, else need to return error message to user
                if (userEnteredAddress.DefaultBillingAddress)
                {
                    var result = await this.Settings.AccountServiceAccessor.LegacyValidateAddress(enrichmentReqAddress, traceActivityId);
                }

                var avsAccessor = this.Settings.AddressEnrichmentServiceAccessor;
                AddressValidateResponse validateResponse = await AddressesExHelper.AddressEnrichmentHelper(this.Settings.AccountServiceAccessor, avsAccessor, enrichmentReqAddress, traceActivityId, this.ExposedFlightFeatures);

                // Skip address suggestion page if address is verified or verified and shippable
                if ((validateResponse.Status == EnrichmentValidationStatus.Verified ||
                        validateResponse.Status == EnrichmentValidationStatus.VerifiedShippable) && this.ExposedFlightFeatures.Contains(Flighting.Features.PXSkipSuggestedAddressPageIfAVSVerified, StringComparer.OrdinalIgnoreCase))
                {
                    PXAddressV3Info pxSuggestedAddress = validateResponse?.SuggestedAddress?.ToPXAddressV3Info(userEnteredAddress.Id, userEnteredAddress);

                    var createdAddress = await this.Settings.AccountServiceAccessor.PostAddress(accountId, pxSuggestedAddress, GlobalConstants.AccountServiceApiVersion.V3, V7.Constants.SyncToLegacyCodes.NoSyncAndValidation, traceActivityId);
                    return this.Request.CreateResponse(createdAddress);
                }
                else
                {
                    List<PXAddressV3Info> pxSuggestedAddresses =
                        validateResponse.SuggestedAddresses?.Select((address, index) => address.ToPXAddressV3Info(index.ToString(), userEnteredAddress)).ToList();

                    // Make sure we have a least an empty list so we don't have to check for null
                    pxSuggestedAddresses = pxSuggestedAddresses ?? new List<PXAddressV3Info>();

                    pidls = AddressSelectionHelper.GetSuggestedAddressesPidls(
                        pxSuggestedAddresses,
                        userEnteredAddress,
                        partner,
                        language,
                        operation,
                        validateResponse.Status.ToString(),
                        this.ExposedFlightFeatures,
                        addressType,
                        setAsDefaultBilling,
                        incomingAddressId,
                        scenario);
                }
            }
            else
            {
                // Use Validate Instance V2 UX for PIDL page if the partner is template based
                bool useValidateInstanceV2UXForPidlPage = TemplateHelper.IsTemplateBasedPIDLIncludingDefaultTemplate(ProxyController.GetSettingTemplate(partner, setting, Constants.DescriptionTypes.Addresses, addressType));
                var avsAccessor = this.Settings.AddressEnrichmentServiceAccessor;

                pidls = await AddressesExHelper.SuggestAddressPidl(
                    accountId,
                    userEnteredAddress,
                    language,
                    partner,
                    operation,
                    traceActivityId,
                    userEnteredAddress.DefaultBillingAddress,
                    this.ExposedFlightFeatures,
                    this.Settings.AccountServiceAccessor,
                    avsAccessor,
                    scenario: scenario,
                    setting: setting,
                    useValidateInstanceV2UXForPidlPage: useValidateInstanceV2UXForPidlPage);

                if (pidls.Count > 0 && pidls[0]?.ClientAction != null && pidls[0].ClientAction.ActionType == ClientActionType.ReturnContext)
                {
                    return this.Request.CreateResponse(pidls[0].ClientAction.Context);
                }
            }

            ClientAction clientAction = new ClientAction(ClientActionType.Pidl, pidls);
            clientAction.PidlRetainUserInput = false;
            PIDLResource suggestedAddressPidl = new PIDLResource();
            suggestedAddressPidl.ClientAction = clientAction;

            FeatureContext featureContext = new FeatureContext(
                country: userEnteredAddress?.Country,
                GetSettingTemplate(partner, setting, Constants.DescriptionTypes.AddressDescription),
                Constants.DescriptionTypes.AddressDescription,
                operation,
                scenario,
                language,
                null,
                this.ExposedFlightFeatures,
                setting?.Features,
                originalPartner: partner,
                isGuestAccount: GuestAccountHelper.IsGuestAccount(this.Request));

            PostProcessor.Process(pidls, PIDLResourceFactory.FeatureFactory, featureContext);

            return this.Request.CreateResponse(suggestedAddressPidl);
        }

        private async Task<HttpResponseMessage> PostAddress(PXAddressV3Info editedAddress, string accountId, EventTraceActivity traceActivityId, int syncToLegacyCode = V7.Constants.SyncToLegacyCodes.NoSyncAndValidation)
        {
            // 1. save the address to Jarvis
            editedAddress.CustomerId = accountId;

            PXAddressV3Info savedAddress = await this.Settings.AccountServiceAccessor.PostAddress(accountId, editedAddress, GlobalConstants.AccountServiceApiVersion.V3, syncToLegacyCode, traceActivityId);

            // Jarvis doesn't return the set_as_default_shipping_address and set_as_default_billing_address properties so we make sure they are added back in here
            savedAddress.DefaultBillingAddress = editedAddress.DefaultBillingAddress;
            savedAddress.DefaultShippingAddress = editedAddress.DefaultShippingAddress;

            // update the profile if the editedAddress is marked as the default shipping address
            if (editedAddress.DefaultShippingAddress)
            {
                // 2. Retrive the customer profile (consumer)
                AccountProfileV3 profile = await this.Settings.AccountServiceAccessor.GetProfileV3(accountId, GlobalConstants.ProfileTypes.Consumer, traceActivityId);

                // 3. Update the default shipping address on the profile
                profile.DefaultShippingAddressId = savedAddress.Id;
                await this.Settings.AccountServiceAccessor.UpdateProfileV3(accountId, profile, GlobalConstants.ProfileTypes.Consumer, traceActivityId, this.ExposedFlightFeatures);
            }

            // update the profile if the editedAddress is marked as the default billing address
            if (editedAddress.DefaultBillingAddress)
            {
                // 2. Retrive the customer profile (consumer)
                AccountProfileV3 profile = await this.Settings.AccountServiceAccessor.GetProfileV3(accountId, GlobalConstants.ProfileTypes.Consumer, traceActivityId);

                // 3. Update the default billing address on the profile
                profile.DefaultAddressId = savedAddress.Id;

                bool syncLegacyAddress = syncToLegacyCode != V7.Constants.SyncToLegacyCodes.NoSyncAndValidation;
                await this.Settings.AccountServiceAccessor.UpdateProfileV3(accountId, profile, GlobalConstants.ProfileTypes.Consumer, traceActivityId, this.ExposedFlightFeatures, syncLegacyAddress);
            }

            // 4. Return the client action to the client with the address and the id
            ClientAction clientAction = new ClientAction(ClientActionType.ReturnContext, savedAddress);

            PidlModel.V7.PIDLResource clientActionPidl = new PidlModel.V7.PIDLResource();
            clientActionPidl.ClientAction = clientAction;
            return this.Request.CreateResponse(clientActionPidl);
        }
    }
}
