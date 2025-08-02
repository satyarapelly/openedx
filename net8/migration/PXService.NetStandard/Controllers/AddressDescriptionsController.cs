// <copyright file="AddressDescriptionsController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlFactory.V7.FeatureContextProcess;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;

    public class AddressDescriptionsController : ProxyController
    {
        /// <summary>
        /// Get address by id ("/GetById" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>AddressDescriptions</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/AddressDescriptions/GetById</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="country" required="true" cref="string" in="query">Two letter country code</param>
        /// <param name="type" required="true" cref="string" in="query">address type</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <param name="scenario" required="false" cref="object" in="body">scenario name</param>
        /// <param name="operation" required="false" cref="string" in="query">operation name</param>
        /// <param name="addressId" required="false" cref="string" in="query">address id</param>
        /// <param name="avsSuggest" required="false" cref="bool" in="query">A bool to indicate whether avs suggestion should be shown or not</param>
        /// <param name="setAsDefaultBilling" required="false" cref="string" in="query">A bool to indicate whether address should be set as default builling address</param>
        /// <response code="200">List&lt;PIDLResource&gt; for AddressDescriptions</response>
        /// <returns>A list of PIDLResource object</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Needs to be an instance method for Route action selection")]
        [HttpGet]
        public async Task<List<PIDLResource>> GetById(
            string accountId,
            string country,
            string type,
            string language = null,
            string partner = Constants.ServiceDefaults.DefaultPartnerName,
            string scenario = null,
            string operation = null,
            string addressId = null,
            bool avsSuggest = false,
            bool setAsDefaultBilling = false)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            string incomingOperation = operation;
            accountId = accountId + string.Empty;
            this.Request.AddTracingProperties(accountId, null, null, type, country);
            this.Request.AddPartnerProperty(partner?.ToLower());
            this.Request.AddPidlOperation(operation?.ToLower());
            this.Request.AddAvsSuggest(avsSuggest);

            // Use Partner Settings if enabled for the partner
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(operation);

            // Enable flighting based on the setting patner
            this.EnableFlightingsInPartnerSetting(setting, country);

            FeatureContext featureContext = new FeatureContext(
                country,
                partner,
                Constants.DescriptionTypes.AddressDescription,
                operation,
                scenario,
                language,
                null,
                this.ExposedFlightFeatures,
                featureConfigs: setting?.Features,
                typeName: type,
                originalPartner: partner,
                originalTypeName: type,
                avsSuggest: avsSuggest,
                isGuestAccount: GuestAccountHelper.IsGuestAccount(this.Request));
            FeatureContextProcessor.Process(featureContext, PIDLResourceFactory.FeatureContextFactory);
            type = featureContext.TypeName;

            List<PIDLResource> retVal = new List<PIDLResource>();

            // To show AVS suggestions, PX has to enable in the flight and partners has to send the flight as well
            if (this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableAVSSuggestions, StringComparer.OrdinalIgnoreCase))
            {
                // Pass ShowAVSSuggestions
                // Remove it from x-ms-flight to prevent passing to PIMS
                if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.ShowAVSSuggestions))
                {
                    this.ExposedFlightFeatures.Add(Constants.PartnerFlightValues.ShowAVSSuggestions);
                    this.RemovePartnerFlight(Constants.PartnerFlightValues.ShowAVSSuggestions);
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

            // when the partner is xbox native if type is billing set to v3 billing address and if not then we can assume its shipping and set to v3 shipping address
            if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner) || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseV3AddressPIDL, country, setting))
            {
                type = string.Equals(scenario, Constants.AddressTypes.Shipping, StringComparison.OrdinalIgnoreCase) ? Constants.AddressTypes.PXV3Shipping : Constants.AddressTypes.PXV3Billing;
                featureContext.TypeName = type;

                // Xbox Native partner Update is the same exact flow as the add so we use add to not have to duplicate a lot of csv entries and logic.
                if (string.Equals(operation, Constants.Operations.Update, StringComparison.OrdinalIgnoreCase))
                {
                    operation = Constants.Operations.Add;
                }
            }

            if (string.Equals(operation, Constants.Operations.ValidateInstance, StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(addressId) || string.IsNullOrWhiteSpace(language) || string.IsNullOrWhiteSpace(country))
                {
                    throw TraceCore.TraceException(traceActivityId, new ValidationException(ErrorCode.InvalidRequestData, "Missing query parameters addressId, language or/and country"));
                }

                var pidlDoc = new List<PIDLResource>()
                {
                    new PIDLResource()
                    {
                        ClientAction = new ClientAction(
                            ClientActionType.ReturnContext,
                            new AddressInfoV3()
                            {
                                Id = addressId
                            })
                    }
                };

                try
                {
                    AddressInfoV3 address = await this.Settings.AccountServiceAccessor.GetAddress<AddressInfoV3>(accountId, addressId, GlobalConstants.AccountServiceApiVersion.V3, traceActivityId);
                    PXAddressV3Info userPickedAddress = new PXAddressV3Info(address);

                    // Use Validate Instance V2 UX for PIDL page if the partner is template based
                    bool useValidateInstanceV2UXForPidlPage = TemplateHelper.IsTemplateBasedPIDLIncludingDefaultTemplate(ProxyController.GetSettingTemplate(partner, setting, Constants.DescriptionTypes.AddressDescription, type));
                    var avsAccessor = this.Settings.AddressEnrichmentServiceAccessor;

                    retVal = await AddressesExHelper.SuggestAddressPidl(
                        accountId,
                        userPickedAddress,
                        language,
                        partner,
                        operation,
                        traceActivityId,
                        setAsDefaultBilling,
                        this.ExposedFlightFeatures,
                        this.Settings.AccountServiceAccessor,
                        avsAccessor,
                        existingAddress: true,
                        scenario: scenario,
                        useValidateInstanceV2UXForPidlPage: useValidateInstanceV2UXForPidlPage,
                        setting: setting);
                    PostProcessor.Process(retVal, PIDLResourceFactory.FeatureFactory, featureContext);
                    return retVal;
                }
                catch (Exception ex)
                {
                    SllWebLogger.TracePXServiceException($"Valiate Address API {ex}", traceActivityId);
                    return pidlDoc;
                }
            }
            else if (string.Equals(operation, Constants.Operations.SelectInstance, StringComparison.OrdinalIgnoreCase))
            {
                if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner) || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.SelectInstanceForAddress, country, setting))
                {
                    CMResources<PXAddressV3Info> addressGroups = await this.Settings.AccountServiceAccessor.GetAddressesByCountry<CMResources<PXAddressV3Info>>(accountId, country, GlobalConstants.AccountServiceApiVersion.V3, traceActivityId);
                    AccountProfileV3 profile = await this.Settings.AccountServiceAccessor.GetProfileV3(accountId, GlobalConstants.ProfileTypes.Consumer, traceActivityId);
                    List<PIDLResource> returnVal = PIDLResourceFactory.GetAddressV3GroupSelectDescriptions(scenario, country, language, partner, addressGroups, profile, setting: setting);

                    PostProcessor.Process(returnVal, PIDLResourceFactory.FeatureFactory, featureContext);
                    return returnVal;
                }
                else
                {
                    throw new Common.ValidationException(ErrorCode.InvalidRequestData, "Selected partner does not support this operation");
                }
            }
            else if (string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase) && avsSuggest && !PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseV3AddressPIDL, country, setting))
            {
                // After looking through past 5 days data, no partner is calling us in the following way. 
                // We should consider to remove the code below to avoid confusion in future
                // will create a seperated PR to clean up the code path
                if (string.Equals(scenario, Constants.AddressTypes.Shipping, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(scenario, Constants.AddressTypes.Billing, StringComparison.OrdinalIgnoreCase))
                {
                    type = scenario;
                }
            }
            
            // There should be no partners that request shipping address except webpay, cart, commercialstores, consumersupport and xbox.
            // For safe purpose, force to convert address type from shipping to billing for these partners.
            if (string.Equals(type, Constants.AddressTypes.Shipping, StringComparison.OrdinalIgnoreCase) &&
                !(PIDLResourceFactory.IsJarvisAddressV3Partner(TemplateHelper.GetSettingTemplate(partner, setting, Constants.DescriptionTypes.AddressDescription, Constants.AddressTypes.Shipping), country, setting) ||
                string.Equals(partner, Constants.PartnerName.Xbox, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(partner, Constants.PartnerName.ConsumerSupport, StringComparison.OrdinalIgnoreCase)))
            {
                type = Constants.AddressTypes.Billing;
            }

            // For amcweb with "ProfileAddress" scenario, change the type to "shipping_v3"
            if (string.Equals(partner, Constants.PartnerName.AmcWeb, StringComparison.OrdinalIgnoreCase)
                && string.Equals(scenario, Constants.ScenarioNames.ProfileAddress, StringComparison.OrdinalIgnoreCase))
            {
                type = Constants.AddressTypes.ShippingV3;
            }

            // For amcweb with "ProfileAddress" scenario, change the type to "shipping_v3"
            if (string.Equals(partner, Constants.PartnerName.CommercialStores, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(type, Constants.AddressTypes.HapiV1SoldToOrganization, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(scenario, Constants.ScenarioNames.CreateBillingAccount, StringComparison.OrdinalIgnoreCase))
            {
                type = Constants.AddressTypes.HapiV1SoldToOrganizationCSP;
            }

            // All requests that hit AddressDescriptionsController is for standalone address, which is always V3
            bool overrideJarvisVersionToV3 = true;

            // Standalone address for OneDrive is for pre-requisite scenario.  Hence, using PIFD instead of Jarvis V3.
            if (PartnerHelper.IsOneDrivePartner(partner))
            {
                overrideJarvisVersionToV3 = false;
            }

            retVal = PIDLResourceFactory.Instance.GetAddressDescriptions(
                country,
                type,
                language,
                partner,
                null,
                overrideJarvisVersionToV3,
                scenario,
                this.ExposedFlightFeatures,
                operation,
                avsSuggest: avsSuggest,
                setAsDefaultBilling: setAsDefaultBilling,
                setting: setting);

            AdjustPropertiesInPIDL(type, partner, scenario, incomingOperation, retVal, this.ExposedFlightFeatures, country, language, addressId, setting);

            // Enable the tempalte partner check, to sync with the commercialstores partner.
            if ((string.Equals(partner, Constants.PartnerName.CommercialStores, StringComparison.OrdinalIgnoreCase)
                || PIDLResourceFactory.IsTemplateInList(partner, setting, Constants.DescriptionTypes.AddressDescription, Constants.AddressTypes.HapiServiceUsageAddress))
                && string.Equals(type, Constants.AddressTypes.HapiServiceUsageAddress, StringComparison.OrdinalIgnoreCase))
            {
                if (Constants.CountriesToCollectTaxIdUnderFlighting.Contains(country))
                {
                    // For Egypt country, it should only enable when the pxenableVATID flight is exposed for partners/templates
                    // Enable the tempalte partner check, to sync with the PXEnableVATID flighting, utilized for the profile.
                    bool pxenableVATID = this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableVATID, StringComparer.OrdinalIgnoreCase);
                    bool isEgyptCountry = string.Equals(country, Constants.CommercialTaxIdCountryRegionCodes.Egypt, StringComparison.OrdinalIgnoreCase);

                    if ((isEgyptCountry && pxenableVATID)
                        || (!isEgyptCountry && (pxenableVATID || PIDLResourceFactory.IsTemplateInList(partner, setting, Constants.DescriptionTypes.AddressDescription, Constants.AddressTypes.HapiServiceUsageAddress))))
                    {
                        LinkTaxPidl(retVal, country, language, partner, operation, setting, this.ExposedFlightFeatures);
                    }
                }
                else
                {
                    LinkTaxPidl(retVal, country, language, partner, operation, setting, this.ExposedFlightFeatures);
                }

                PIDLResourceFactory.RemoveEmptyPidlContainerHints(retVal);

                if (this.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.ShowAVSSuggestions, StringComparer.OrdinalIgnoreCase))
                {
                    foreach (PIDLResource pidl in retVal)
                    {
                        DisplayHint submitButtonHiddenDisplayHint = pidl.GetDisplayHintById(Constants.DisplayHintIds.SubmitButtonHidden);
                        if (submitButtonHiddenDisplayHint != null)
                        {
                            this.AddModernValidationAction(submitButtonHiddenDisplayHint, "serviceUsageAddress", type, partner, language, country);
                        }
                    }
                }

                foreach (PIDLResource pidl in retVal)
                {
                    // For HapiServiceUsageAddress, include "is_customer_consented" and "is_avs_full_validation_succeeded" in payload when posting to Hapi
                    ProxyController.AddHiddenCheckBoxElement(pidl, GlobalConstants.CommercialZipPlusFourPropertyNames.IsUserConsented, "value.serviceUsageAddress");
                    ProxyController.AddHiddenCheckBoxElement(pidl, GlobalConstants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded, "value.serviceUsageAddress");
                }
            }

            if (string.Equals(partner, Constants.PartnerName.CommercialStores, StringComparison.OrdinalIgnoreCase)
                && (string.Equals(type, Constants.AddressTypes.HapiV1SoldToOrganization, StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, Constants.AddressTypes.HapiV1ShipToOrganization, StringComparison.OrdinalIgnoreCase)))
            {
                foreach (PIDLResource pidl in retVal)
                {
                    // For HapiV1SoldToOrganization type, include "is_customer_consented" and "is_avs_full_validation_succeeded" in payload when posting to Hapi
                    ProxyController.AddHiddenCheckBoxElement(pidl, GlobalConstants.CommercialZipPlusFourPropertyNames.IsUserConsented, "address");
                    ProxyController.AddHiddenCheckBoxElement(pidl, GlobalConstants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded, "address");
                }
            }

            if (string.Equals(partner, Constants.PartnerName.CommercialStores, StringComparison.OrdinalIgnoreCase)
                && string.Equals(type, Constants.AddressTypes.Shipping, StringComparison.OrdinalIgnoreCase)
                && string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase)
                && this.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.ShowAVSSuggestions, StringComparer.OrdinalIgnoreCase))
            {
                foreach (PIDLResource pidl in retVal)
                {
                    DisplayHint validateButtonDisplayHint = pidl.GetDisplayHintById(Constants.DisplayHintIds.NextButton);
                    if (validateButtonDisplayHint != null)
                    {
                        this.AddModernValidationAction(validateButtonDisplayHint, string.Empty, type, partner, language, country);
                    }

                    // For Shipping type, include "is_customer_consented" and "is_avs_full_validation_succeeded" in payload when posting to Hapi
                    ProxyController.AddHiddenCheckBoxElement(pidl, GlobalConstants.CommercialZipPlusFourPropertyNames.IsUserConsented, "address");
                    ProxyController.AddHiddenCheckBoxElement(pidl, GlobalConstants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded, "address");
                }
            }

            if (ShowAVSSuggestionsForAMCWebProfileAddress(this.ExposedFlightFeatures, partner, scenario, country, setting))
            {
                foreach (PIDLResource pidl in retVal)
                {
                    DisplayHint saveButtonDisplayHint = pidl.GetDisplayHintById(Constants.DisplayHintIds.SaveButton);
                    if (saveButtonDisplayHint != null)
                    {
                        this.AddModernValidationAction(saveButtonDisplayHint, string.Empty, type, partner, language, country);
                    }

                    // For amc profileAddress form, include "is_customer_consented" and "is_avs_full_validation_succeeded" in payload when posting to Jarvis
                    ProxyController.AddHiddenCheckBoxElement(pidl, GlobalConstants.CommercialZipPlusFourPropertyNames.IsUserConsented, null);
                    ProxyController.AddHiddenCheckBoxElement(pidl, GlobalConstants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded, null);
                }
            }

            // Also update (consumer) profile with default_address_id in the case of OneDrive partner
            if (PartnerHelper.IsOneDrivePartner(partner))
            {
                string profileType = this.GetProfileType();
                AccountProfile profile = await this.Settings.AccountServiceAccessor.GetProfile(accountId, profileType, traceActivityId);
                PIDLResourceFactory.AddSecondarySubmitAddressContext(retVal, profile, partner, country: country, setting: setting);
            }

            PostProcessor.Process(retVal, PIDLResourceFactory.FeatureFactory, featureContext);
            return retVal;
        }

        /// <summary>
        /// Get address groups by id ("/GetAddressGroupsById" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>AddressDescriptions</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/AddressDescriptions/GetAddressGroupsById</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="country" required="true" cref="string" in="query">Two letter country code</param>
        /// <param name="operation" required="false" cref="string" in="query">operation name</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <response code="200">List&lt;PIDLResource&gt; for AddressDescriptions</response>
        /// <returns>A list of PIDLResource object</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Needs to be an instance method for Route action selection")]
        [HttpGet]
        public async Task<List<PIDLResource>> GetAddressGroupsById(
            string accountId,
            string country,
            string operation = Constants.Operations.SelectInstance,
            string language = null,
            string partner = Constants.ServiceDefaults.DefaultPartnerName)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddPartnerProperty(partner?.ToLower());
            this.Request.AddPidlOperation(operation?.ToLower());

            accountId = accountId + string.Empty;
            CMResources<AddressInfo> addressGroups = await this.Settings.AccountServiceAccessor.GetAddressesByCountry<CMResources<AddressInfo>>(accountId, country, GlobalConstants.AccountServiceApiVersion.V2, traceActivityId);
            List<PIDLResource> retVal = PIDLResourceFactory.GetAddressGroupSelectDescriptions(country, language, partner, addressGroups);

            return retVal;
        }

        /// <summary>
        /// Get address descriptions
        /// </summary>
        /// <group>AddressDescriptions</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/AddressDescriptions/GetAddressDescriptions</url>
        /// <param name="country" required="true" cref="string" in="query">Two letter country code</param>
        /// <param name="type" required="true" cref="string" in="query">address type</param>
        /// <param name="operation" required="false" cref="string" in="query">operation name</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <param name="scenario" required="false" cref="string" in="query">Scenario name</param>
        /// <response code="200">List&lt;PIDLResource&gt; for AddressDescriptions</response>
        /// <returns>A list of PIDLResource object</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Needs to be an instance method for Route action selection")]
        [HttpGet]
        public object GetAddressDescriptions(
            string country,
            string type,
            string operation = Constants.Operations.Add,
            string language = null,
            string partner = Constants.ServiceDefaults.DefaultPartnerName,
            string scenario = null)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            this.Request.AddPartnerProperty(partner?.ToLower());
            this.Request.AddPidlOperation(operation?.ToLower());

            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(operation);

            // Enable flighting based on the setting patner
            this.EnableFlightingsInPartnerSetting(setting, country);

            FeatureContext featureContext = new FeatureContext(
                country,
                partner,
                Constants.DescriptionTypes.AddressDescription,
                operation,
                scenario,
                language,
                null,
                this.ExposedFlightFeatures,
                featureConfigs: setting?.Features,
                typeName: type,
                originalPartner: partner,
                originalTypeName: type,
                isGuestAccount: GuestAccountHelper.IsGuestAccount(this.Request));
            FeatureContextProcessor.Process(featureContext, PIDLResourceFactory.FeatureContextFactory);
            type = featureContext.TypeName;

            // Pass ShowMiddleName to PidlFactory
            // Remove it from x-ms-flight to prevent passing to PIMS
            if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.ShowMiddleName))
            {
                this.ExposedFlightFeatures.Add(Constants.PartnerFlightValues.ShowMiddleName);
                this.RemovePartnerFlight(Constants.PartnerFlightValues.ShowMiddleName);
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

            // For template based flow, use the type in the request parameters after featureContext process for PIDL genearation
            if (TemplateHelper.IsTemplateBasedPIDLIncludingDefaultTemplate(ProxyController.GetSettingTemplate(partner, setting, Constants.DescriptionTypes.AddressDescription, type)))
            {
                List<PIDLResource> retVal = PIDLResourceFactory.Instance.GetAddressDescriptions(
                    country,
                    type,
                    language,
                    partner,
                    null,
                    true,
                    scenario,
                    this.ExposedFlightFeatures,
                    operation,
                    setting: setting);

                PostProcessor.Process(retVal, PIDLResourceFactory.FeatureFactory, featureContext);

                return retVal;
            }

            if (PartnerHelper.IsCommercialStoresPartner(partner) &&
                string.Equals(type, Constants.AddressTypes.Billing, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase))
            {
                var retVal = PIDLResourceFactory.Instance.GetAddressDescriptions(
                    country,
                    type,
                    language,
                    partner,
                    null,
                    true,
                    Constants.ScenarioNames.ModernAccount,
                    exposedFlightFeatures: this.ExposedFlightFeatures,
                    operation,
                    setting: setting);

                foreach (PIDLResource resource in retVal)
                {
                    if (string.Equals(scenario, Constants.ScenarioNames.ModernAccount, StringComparison.OrdinalIgnoreCase))
                    {
                        resource.SetPropertyState("country", true);
                    }
                }

                this.EnableAVS(
                    retVal,
                    this.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.ShowAVSSuggestions, StringComparer.OrdinalIgnoreCase),
                    this.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.EnableAVSAddtionalFlags, StringComparer.OrdinalIgnoreCase),
                    Constants.DisplayHintIds.ValidateButtonHidden,
                    type,
                    partner,
                    language,
                    country);

                PostProcessor.Process(retVal, PIDLResourceFactory.FeatureFactory, featureContext);
                return retVal;
            }

            if ((string.Equals(partner, Constants.PartnerName.CommercialStores, StringComparison.OrdinalIgnoreCase) || PartnerHelper.IsAzurePartner(partner))
                && (string.Equals(type, Constants.AddressTypes.Individual, StringComparison.OrdinalIgnoreCase) || string.Equals(type, Constants.AddressTypes.Organization, StringComparison.OrdinalIgnoreCase)))
            {
                type = Constants.AddressTypes.Billing;
                List<PIDLResource> retVal = PIDLResourceFactory.Instance.GetAddressDescriptions(country, type, language, partner, null, true, Constants.ScenarioNames.ModernAccount, this.ExposedFlightFeatures, operation, setting: setting);

                foreach (PIDLResource resource in retVal)
                {
                    resource.SetPropertyState("country", true);
                }

                PostProcessor.Process(retVal, PIDLResourceFactory.FeatureFactory, featureContext);

                return retVal;
            }

            if (PartnerHelper.IsAzureBasedPartner(partner) || PartnerHelper.IsBingPartner(partner))
            {
                type = (type ?? Constants.AddressTypes.Shipping) == Constants.AddressTypes.Shipping ? "shipping_v3" : type;
                List<PIDLResource> retVal = PIDLResourceFactory.Instance.GetAddressDescriptions(country, type, language, partner, null, true, exposedFlightFeatures: this.ExposedFlightFeatures, setting: setting);

                if (this.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.ShowAVSSuggestions, StringComparer.OrdinalIgnoreCase)
                    && (string.Equals(type, Constants.AddressTypes.Billing) || string.Equals(type, Constants.AddressTypes.Shipping) || string.Equals(type, Constants.AddressTypes.ShippingV3)))
                {
                    foreach (PIDLResource pidl in retVal)
                    {
                        DisplayHint validateButtonDisplayHint = pidl.GetDisplayHintById(Constants.DisplayHintIds.ValidateButtonHidden);
                        if (validateButtonDisplayHint != null)
                        {
                            this.AddModernValidationAction(validateButtonDisplayHint, string.Empty, type, partner, language, country);
                        }

                        if (string.Equals(partner, Constants.PartnerName.Azure, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(type, Constants.AddressTypes.Billing, StringComparison.OrdinalIgnoreCase))
                        {
                            // For azure billing address form, include "is_customer_consented" and "is_avs_full_validation_succeeded" in payload when posting to Jarvis
                            ProxyController.AddHiddenCheckBoxElement(pidl, GlobalConstants.CommercialZipPlusFourPropertyNames.IsUserConsented, null);
                            ProxyController.AddHiddenCheckBoxElement(pidl, GlobalConstants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded, null);
                        }
                    }
                }

                PostProcessor.Process(retVal, PIDLResourceFactory.FeatureFactory, featureContext);

                return retVal;
            }

            if (string.Equals(partner, Constants.PartnerName.CommercialSupport, StringComparison.OrdinalIgnoreCase))
            {
                List<PIDLResource> retVal = PIDLResourceFactory.Instance.GetAddressDescriptions(country, type, language, partner, null, true, Constants.ScenarioNames.ModernAccount, this.ExposedFlightFeatures, operation, setting: setting);

                // Add modern Validate for CommercialSupport partners, OrgAddress type and Add operation
                if (string.Equals(type, Constants.AddressTypes.OrgAddress, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase)
                   && this.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.ShowAVSSuggestions, StringComparer.OrdinalIgnoreCase))
                {
                    foreach (PIDLResource pidl in retVal)
                    {
                        DisplayHint validateButtonDisplayHint = pidl.GetDisplayHintById(Constants.DisplayHintIds.ValidateButtonHidden);
                        if (validateButtonDisplayHint != null)
                        {
                            this.AddModernValidationAction(validateButtonDisplayHint, string.Empty, type, partner, language, country);
                        }

                        if (this.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.EnableAVSAddtionalFlags, StringComparer.OrdinalIgnoreCase))
                        {
                            // For CommercialSupport OrgAddress address form, include "is_customer_consented" and "is_avs_full_validation_succeeded" in payload when posting to Jarvis
                            ProxyController.AddHiddenCheckBoxElement(pidl, GlobalConstants.CommercialZipPlusFourPropertyNames.IsUserConsented, null);
                            ProxyController.AddHiddenCheckBoxElement(pidl, GlobalConstants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded, null);
                        }
                    }
                }

                PostProcessor.Process(retVal, PIDLResourceFactory.FeatureFactory, featureContext);

                return retVal;
            }

            if (string.Equals(operation, Constants.Operations.Add, StringComparison.InvariantCultureIgnoreCase))
            {
                List<PIDLResource> retVal = PIDLResourceFactory.Instance.GetAddressDescriptions(country, "shipping_v3", language, partner, null, true, exposedFlightFeatures: this.ExposedFlightFeatures, setting: setting);

                if (string.Equals(partner, Constants.PartnerName.MarketPlace, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (PIDLResource resource in retVal)
                    {
                        resource.SetPropertyState("country", true);
                    }
                }

                PostProcessor.Process(retVal, PIDLResourceFactory.FeatureFactory, featureContext);

                return retVal;
            }
            else if (string.Equals(operation, Constants.Operations.Update, StringComparison.InvariantCultureIgnoreCase))
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject("[{\"identity\":{\"description_type\":\"address\",\"type\":\"shipping\",\"country\":\"USA\"},\"data_description\":{\"AddressTypeValue\":{\"propertyType\":\"clientData\",\"type\":\"hidden\",\"dataType\":\"hidden\",\"is_updatable\":true,\"default_value\":2},\"ThreeLetterISORegionName\":{\"propertyType\":\"clientData\",\"type\":\"hidden\",\"dataType\":\"hidden\",\"is_updatable\":true,\"default_value\":\"USA\"},\"RecordId\":{\"propertyType\":\"userData\",\"type\":\"string\",\"dataType\":\"string\",\"is_key\":false,\"is_optional\":true,\"is_updatable\":true},\"Street\":{\"propertyType\":\"userData\",\"type\":\"string\",\"dataType\":\"string\",\"is_key\":false,\"is_optional\":false,\"is_updatable\":true,\"validation\":{\"validationType\":\"regex\",\"regex\":\"^(?!^[\\\\u0009\\\\u000A\\\\u000D\\\\u0020\\\\u2000-\\\\u200B]*$)[\\\\u0009\\\\u000A\\\\u000D\\\\u0020-\\\\uD7FF\\\\uE000-\\\\uFFFD\\\\u10000-\\\\u10FFFF]{1,50}$\",\"error_code\":\"invalid_address\",\"error_message\":\"Incorrectly formatted address\"},\"validations\":[{\"validationType\":\"regex\",\"regex\":\"^(?!^[\\\\u0009\\\\u000A\\\\u000D\\\\u0020\\\\u2000-\\\\u200B]*$)[\\\\u0009\\\\u000A\\\\u000D\\\\u0020-\\\\uD7FF\\\\uE000-\\\\uFFFD\\\\u10000-\\\\u10FFFF]{1,50}$\",\"error_code\":\"invalid_address\",\"error_message\":\"Incorrectly formatted address\"}]},\"City\":{\"propertyType\":\"userData\",\"type\":\"string\",\"dataType\":\"string\",\"is_key\":false,\"is_optional\":false,\"is_updatable\":true,\"validation\":{\"validationType\":\"regex\",\"regex\":\"^(?!^[\\\\u0009\\\\u000A\\\\u000D\\\\u0020\\\\u2000-\\\\u200B]*$)[\\\\u0009\\\\u000A\\\\u000D\\\\u0020-\\\\uD7FF\\\\uE000-\\\\uFFFD\\\\u10000-\\\\u10FFFF]{1,50}$\",\"error_code\":\"invalid_city\",\"error_message\":\"Incorrectly formatted city\"},\"validations\":[{\"validationType\":\"regex\",\"regex\":\"^(?!^[\\\\u0009\\\\u000A\\\\u000D\\\\u0020\\\\u2000-\\\\u200B]*$)[\\\\u0009\\\\u000A\\\\u000D\\\\u0020-\\\\uD7FF\\\\uE000-\\\\uFFFD\\\\u10000-\\\\u10FFFF]{1,50}$\",\"error_code\":\"invalid_city\",\"error_message\":\"Incorrectly formatted city\"}]},\"State\":{\"propertyType\":\"userData\",\"type\":\"string\",\"dataType\":\"string\",\"is_key\":false,\"is_optional\":false,\"is_updatable\":true,\"possible_values\":{\"al\":\"Alabama\",\"ak\":\"Alaska\",\"az\":\"Arizona\",\"ar\":\"Arkansas\",\"ca\":\"California\",\"co\":\"Colorado\",\"ct\":\"Connecticut\",\"de\":\"Delaware\",\"dc\":\"District of Columbia\",\"fl\":\"Florida\",\"ga\":\"Georgia\",\"hi\":\"Hawaii\",\"id\":\"Idaho\",\"il\":\"Illinois\",\"in\":\"Indiana\",\"ia\":\"Iowa\",\"ks\":\"Kansas\",\"ky\":\"Kentucky\",\"la\":\"Louisiana\",\"me\":\"Maine\",\"md\":\"Maryland\",\"ma\":\"Massachusetts\",\"mi\":\"Michigan\",\"mn\":\"Minnesota\",\"ms\":\"Mississippi\",\"mo\":\"Missouri\",\"mt\":\"Montana\",\"ne\":\"Nebraska\",\"nv\":\"Nevada\",\"nh\":\"New Hampshire\",\"nj\":\"New Jersey\",\"nm\":\"New Mexico\",\"ny\":\"New York\",\"nc\":\"North Carolina\",\"nd\":\"North Dakota\",\"oh\":\"Ohio\",\"ok\":\"Oklahoma\",\"or\":\"Oregon\",\"pa\":\"Pennsylvania\",\"pr\":\"Puerto Rico\",\"ri\":\"Rhode Island\",\"sc\":\"South Carolina\",\"sd\":\"South Dakota\",\"tn\":\"Tennessee\",\"tx\":\"Texas\",\"ut\":\"Utah\",\"vt\":\"Vermont\",\"va\":\"Virginia\",\"wa\":\"Washington\",\"wv\":\"West Virginia\",\"wi\":\"Wisconsin\",\"wy\":\"Wyoming\"},\"validation\":{\"validationType\":\"regex\",\"regex\":\"^[A-Za-z0-9]{2,3}$\",\"error_code\":\"invalid_state\",\"error_message\":\"Incorrectly formatted state\"},\"validations\":[{\"validationType\":\"regex\",\"regex\":\"^[A-Za-z0-9]{2,3}$\",\"error_code\":\"invalid_state\",\"error_message\":\"Incorrectly formatted state\"}]},\"ZipCode\":{\"propertyType\":\"userData\",\"type\":\"string\",\"dataType\":\"string\",\"is_key\":false,\"is_optional\":false,\"is_updatable\":true,\"validation\":{\"validationType\":\"regex\",\"regex\":\"^\\\\d{5}(-\\\\d{4})?$\",\"error_code\":\"invalid_zip\",\"error_message\":\"Incorrectly formatted zip\"},\"validations\":[{\"validationType\":\"regex\",\"regex\":\"^\\\\d{5}(-\\\\d{4})?$\",\"error_code\":\"invalid_zip\",\"error_message\":\"Incorrectly formatted zip\"}]},\"ThreeLetterISORegionName\":{\"propertyType\":\"userData\",\"type\":\"string\",\"dataType\":\"string\",\"is_key\":true,\"is_optional\":false,\"is_updatable\":true,\"default_value\":\"USA\",\"possible_values\":{\"af\":\"Afghanistan\",\"ax\":\"\u00C5land Islands\",\"al\":\"Albania\",\"dz\":\"Algeria\",\"as\":\"American Samoa\",\"ad\":\"Andorra\",\"ao\":\"Angola\",\"ai\":\"Anguilla\",\"aq\":\"Antarctica\",\"ag\":\"Antigua and Barbuda\",\"ar\":\"Argentina\",\"am\":\"Armenia\",\"aw\":\"Aruba\",\"au\":\"Australia\",\"at\":\"Austria\",\"az\":\"Azerbaijan\",\"bs\":\"Bahamas\",\"bh\":\"Bahrain\",\"bd\":\"Bangladesh\",\"bb\":\"Barbados\",\"by\":\"Belarus\",\"be\":\"Belgium\",\"bz\":\"Belize\",\"bj\":\"Benin\",\"bm\":\"Bermuda\",\"bt\":\"Bhutan\",\"bo\":\"Bolivia\",\"bq\":\"Bonaire\",\"ba\":\"Bosnia and Herzegovina\",\"bw\":\"Botswana\",\"bv\":\"Bouvet Island\",\"br\":\"Brazil\",\"io\":\"British Indian Ocean Territory\",\"bn\":\"Brunei\",\"bg\":\"Bulgaria\",\"bf\":\"Burkina Faso\",\"bi\":\"Burundi\",\"kh\":\"Cambodia\",\"cm\":\"Cameroon\",\"ca\":\"Canada\",\"cv\":\"Cabo Verde\",\"ky\":\"Cayman Islands\",\"cf\":\"Central African Republic\",\"td\":\"Chad\",\"gg\":\"Channel Islands - Guernsey\",\"je\":\"Channel Islands - Jersey\",\"cl\":\"Chile\",\"cn\":\"China\",\"cx\":\"Christmas Island\",\"cc\":\"Cocos (Keeling) Islands\",\"co\":\"Colombia\",\"km\":\"Comoros\",\"cd\":\"Congo (DRC)\",\"cg\":\"Congo, Republic of\",\"ck\":\"Cook Islands\",\"cr\":\"Costa Rica\",\"ci\":\"C\u00F4te d'Ivoire (Ivory Coast)\",\"hr\":\"Croatia\",\"cw\":\"Cura\u00E7ao\",\"cy\":\"Cyprus\",\"cz\":\"Czech Republic\",\"dk\":\"Denmark\",\"dj\":\"Djibouti\",\"dm\":\"Dominica\",\"do\":\"Dominican Republic\",\"ec\":\"Ecuador\",\"eg\":\"Egypt\",\"sv\":\"El Salvador\",\"gq\":\"Equatorial Guinea\",\"er\":\"Eritrea\",\"ee\":\"Estonia\",\"et\":\"Ethiopia\",\"fk\":\"Falkland Islands\",\"fo\":\"Faroe Islands\",\"fj\":\"Fiji Islands\",\"fi\":\"Finland\",\"fr\":\"France\",\"gf\":\"French Guiana\",\"pf\":\"French Polynesia\",\"ga\":\"Gabon\",\"gm\":\"Gambia, The\",\"ge\":\"Georgia\",\"de\":\"Germany\",\"gh\":\"Ghana\",\"gi\":\"Gibraltar\",\"gr\":\"Greece\",\"gl\":\"Greenland\",\"gd\":\"Grenada\",\"gp\":\"Guadeloupe\",\"gu\":\"Guam\",\"gt\":\"Guatemala\",\"gn\":\"Guinea\",\"gw\":\"Guinea-Bissau\",\"gy\":\"Guyana\",\"ht\":\"Haiti\",\"hm\":\"Heard Island and McDonald Islands\",\"hn\":\"Honduras\",\"hk\":\"Hong Kong SAR\",\"hu\":\"Hungary\",\"is\":\"Iceland\",\"in\":\"India\",\"id\":\"Indonesia\",\"iq\":\"Iraq\",\"ie\":\"Ireland\",\"im\":\"Isle of Man\",\"il\":\"Israel\",\"it\":\"Italy\",\"jm\":\"Jamaica\",\"jp\":\"Japan\",\"jo\":\"Jordan\",\"kz\":\"Kazakhstan\",\"ke\":\"Kenya\",\"ki\":\"Kiribati\",\"kr\":\"Korea, Republic Of\",\"kw\":\"Kuwait\",\"kg\":\"Kyrgyzstan\",\"la\":\"Laos\",\"lv\":\"Latvia\",\"lb\":\"Lebanon\",\"ls\":\"Lesotho\",\"lr\":\"Liberia\",\"ly\":\"Libya\",\"li\":\"Liechtenstein\",\"lt\":\"Lithuania\",\"lu\":\"Luxembourg\",\"mo\":\"Macao SAR\",\"mk\":\"North Macedonia\",\"mg\":\"Madagascar\",\"mw\":\"Malawi\",\"my\":\"Malaysia\",\"mv\":\"Maldives\",\"ml\":\"Mali\",\"mt\":\"Malta\",\"mh\":\"Marshall Islands\",\"mq\":\"Martinique\",\"mr\":\"Mauritania\",\"mu\":\"Mauritius\",\"yt\":\"Mayotte\",\"mx\":\"Mexico\",\"fm\":\"Micronesia\",\"md\":\"Moldova\",\"mc\":\"Monaco\",\"mn\":\"Mongolia\",\"me\":\"Montenegro\",\"ms\":\"Montserrat\",\"ma\":\"Morocco\",\"mz\":\"Mozambique\",\"mm\":\"Myanmar\",\"na\":\"Namibia\",\"nr\":\"Nauru\",\"np\":\"Nepal\",\"nl\":\"Netherlands, The\",\"nc\":\"New Caledonia\",\"nz\":\"New Zealand\",\"ni\":\"Nicaragua\",\"ne\":\"Niger\",\"ng\":\"Nigeria\",\"nu\":\"Niue\",\"nf\":\"Norfolk Island\",\"mp\":\"Northern Mariana Islands\",\"no\":\"Norway\",\"om\":\"Oman\",\"pk\":\"Pakistan\",\"pw\":\"Palau\",\"ps\":\"Palestinian Authority\",\"pa\":\"Panama\",\"pg\":\"Papua New Guinea\",\"py\":\"Paraguay\",\"pe\":\"Peru\",\"ph\":\"Philippines\",\"pn\":\"Pitcairn Islands\",\"pl\":\"Poland\",\"pt\":\"Portugal\",\"pr\":\"Puerto Rico\",\"qa\":\"Qatar\",\"re\":\"R\u00E9union\",\"ro\":\"Romania\",\"ru\":\"Russia\",\"rw\":\"Rwanda\",\"bl\":\"Saint Barth\u00E9lemy\",\"sh\":\"St Helena, Ascension, Tristan da Cunha\",\"kn\":\"Saint Kitts and Nevis\",\"lc\":\"Saint Lucia\",\"mf\":\"Saint Martin (French part)\",\"pm\":\"Saint Pierre and Miquelon\",\"vc\":\"Saint Vincent and the Grenadines\",\"ws\":\"Samoa\",\"sm\":\"San Marino\",\"st\":\"Sao Tome and Principe\",\"sa\":\"Saudi Arabia\",\"sn\":\"Senegal\",\"rs\":\"Serbia\",\"sc\":\"Seychelles\",\"sl\":\"Sierra Leone\",\"sg\":\"Singapore\",\"sx\":\"Sint Maarten\",\"sk\":\"Slovakia\",\"si\":\"Slovenia\",\"sb\":\"Solomon Islands\",\"so\":\"Somalia\",\"za\":\"South Africa\",\"gs\":\"South Georgia and South Sandwich Islands\",\"es\":\"Spain\",\"lk\":\"Sri Lanka\",\"sr\":\"Suriname\",\"sj\":\"Svalbard\",\"sz\":\"Swaziland\",\"se\":\"Sweden\",\"ch\":\"Switzerland\",\"tw\":\"Taiwan\",\"tj\":\"Tajikistan\",\"tz\":\"Tanzania\",\"th\":\"Thailand\",\"tl\":\"Timor-Leste\",\"tg\":\"Togo\",\"tk\":\"Tokelau\",\"to\":\"Tonga\",\"tt\":\"Trinidad and Tobago\",\"tn\":\"Tunisia\",\"tr\":\"Turkey\",\"tm\":\"Turkmenistan\",\"tc\":\"Turks and Caicos Islands\",\"tv\":\"Tuvalu\",\"ug\":\"Uganda\",\"ua\":\"Ukraine\",\"ae\":\"United Arab Emirates\",\"gb\":\"United Kingdom\",\"USA\":\"United States\",\"um\":\"United States Minor Outlying Islands\",\"uy\":\"Uruguay\",\"uz\":\"Uzbekistan\",\"vu\":\"Vanuatu\",\"va\":\"Vatican City\",\"ve\":\"Venezuela\",\"vn\":\"Viet Nam\",\"vg\":\"Virgin Islands, British\",\"vi\":\"Virgin Islands, US\",\"wf\":\"Wallis and Futuna\",\"ye\":\"Yemen\",\"zm\":\"Zambia\",\"zw\":\"Zimbabwe\"},\"validation\":{\"validationType\":\"regex\",\"regex\":\"^USA$\",\"error_code\":\"invalid_country\",\"error_message\":\"Incorrectly formatted country\"},\"validations\":[{\"validationType\":\"regex\",\"regex\":\"^USA$\",\"error_code\":\"invalid_country\",\"error_message\":\"Incorrectly formatted country\"}]}},\"displayDescription\":[{\"displayName\":\"AddressDetailsPage\",\"members\":[{\"displayContent\":\"Update your address\",\"displayId\":\"shippingAddressPageHeading\",\"displayType\":\"heading\"},{\"displayName\":\"Street Address\",\"showDisplayName\":\"true\",\"minLength\":25,\"maxLength\":30,\"displayErrorMessages\":{\"defaultErrorMessage\":\"Incorrectly formatted address\",\"fromErrorCode\":[{\"errorCode\":\"required_field_empty\",\"errorMessage\":\"A required field is empty\"}]},\"displayId\":\"streetAddress\",\"displayType\":\"property\",\"propertyName\":\"Street\",\"tags\":{\"accessibilityName\":\"Street Address\"}},{\"displayName\":\"City\",\"showDisplayName\":\"true\",\"maxLength\":26,\"displayErrorMessages\":{\"defaultErrorMessage\":\"Incorrectly formatted city\",\"fromErrorCode\":[{\"errorCode\":\"required_field_empty\",\"errorMessage\":\"A required field is empty\"}]},\"displayId\":\"addressCity\",\"displayType\":\"property\",\"propertyName\":\"City\",\"tags\":{\"accessibilityName\":\"City\"}},{\"displayName\":\"State\",\"showDisplayName\":\"true\",\"possibleValues\":{\"al\":\"Alabama\",\"ak\":\"Alaska\",\"az\":\"Arizona\",\"ar\":\"Arkansas\",\"ca\":\"California\",\"co\":\"Colorado\",\"ct\":\"Connecticut\",\"de\":\"Delaware\",\"dc\":\"District of Columbia\",\"fl\":\"Florida\",\"ga\":\"Georgia\",\"hi\":\"Hawaii\",\"id\":\"Idaho\",\"il\":\"Illinois\",\"in\":\"Indiana\",\"ia\":\"Iowa\",\"ks\":\"Kansas\",\"ky\":\"Kentucky\",\"la\":\"Louisiana\",\"me\":\"Maine\",\"md\":\"Maryland\",\"ma\":\"Massachusetts\",\"mi\":\"Michigan\",\"mn\":\"Minnesota\",\"ms\":\"Mississippi\",\"mo\":\"Missouri\",\"mt\":\"Montana\",\"ne\":\"Nebraska\",\"nv\":\"Nevada\",\"nh\":\"New Hampshire\",\"nj\":\"New Jersey\",\"nm\":\"New Mexico\",\"ny\":\"New York\",\"nc\":\"North Carolina\",\"nd\":\"North Dakota\",\"oh\":\"Ohio\",\"ok\":\"Oklahoma\",\"or\":\"Oregon\",\"pa\":\"Pennsylvania\",\"pr\":\"Puerto Rico\",\"ri\":\"Rhode Island\",\"sc\":\"South Carolina\",\"sd\":\"South Dakota\",\"tn\":\"Tennessee\",\"tx\":\"Texas\",\"ut\":\"Utah\",\"vt\":\"Vermont\",\"va\":\"Virginia\",\"wa\":\"Washington\",\"wv\":\"West Virginia\",\"wi\":\"Wisconsin\",\"wy\":\"Wyoming\"},\"possibleOptions\":{\"al\":{\"displayText\":\"Alabama\",\"isDisabled\":false},\"ak\":{\"displayText\":\"Alaska\",\"isDisabled\":false},\"az\":{\"displayText\":\"Arizona\",\"isDisabled\":false},\"ar\":{\"displayText\":\"Arkansas\",\"isDisabled\":false},\"ca\":{\"displayText\":\"California\",\"isDisabled\":false},\"co\":{\"displayText\":\"Colorado\",\"isDisabled\":false},\"ct\":{\"displayText\":\"Connecticut\",\"isDisabled\":false},\"de\":{\"displayText\":\"Delaware\",\"isDisabled\":false},\"dc\":{\"displayText\":\"District of Columbia\",\"isDisabled\":false},\"fl\":{\"displayText\":\"Florida\",\"isDisabled\":false},\"ga\":{\"displayText\":\"Georgia\",\"isDisabled\":false},\"hi\":{\"displayText\":\"Hawaii\",\"isDisabled\":false},\"id\":{\"displayText\":\"Idaho\",\"isDisabled\":false},\"il\":{\"displayText\":\"Illinois\",\"isDisabled\":false},\"in\":{\"displayText\":\"Indiana\",\"isDisabled\":false},\"ia\":{\"displayText\":\"Iowa\",\"isDisabled\":false},\"ks\":{\"displayText\":\"Kansas\",\"isDisabled\":false},\"ky\":{\"displayText\":\"Kentucky\",\"isDisabled\":false},\"la\":{\"displayText\":\"Louisiana\",\"isDisabled\":false},\"me\":{\"displayText\":\"Maine\",\"isDisabled\":false},\"md\":{\"displayText\":\"Maryland\",\"isDisabled\":false},\"ma\":{\"displayText\":\"Massachusetts\",\"isDisabled\":false},\"mi\":{\"displayText\":\"Michigan\",\"isDisabled\":false},\"mn\":{\"displayText\":\"Minnesota\",\"isDisabled\":false},\"ms\":{\"displayText\":\"Mississippi\",\"isDisabled\":false},\"mo\":{\"displayText\":\"Missouri\",\"isDisabled\":false},\"mt\":{\"displayText\":\"Montana\",\"isDisabled\":false},\"ne\":{\"displayText\":\"Nebraska\",\"isDisabled\":false},\"nv\":{\"displayText\":\"Nevada\",\"isDisabled\":false},\"nh\":{\"displayText\":\"New Hampshire\",\"isDisabled\":false},\"nj\":{\"displayText\":\"New Jersey\",\"isDisabled\":false},\"nm\":{\"displayText\":\"New Mexico\",\"isDisabled\":false},\"ny\":{\"displayText\":\"New York\",\"isDisabled\":false},\"nc\":{\"displayText\":\"North Carolina\",\"isDisabled\":false},\"nd\":{\"displayText\":\"North Dakota\",\"isDisabled\":false},\"oh\":{\"displayText\":\"Ohio\",\"isDisabled\":false},\"ok\":{\"displayText\":\"Oklahoma\",\"isDisabled\":false},\"or\":{\"displayText\":\"Oregon\",\"isDisabled\":false},\"pa\":{\"displayText\":\"Pennsylvania\",\"isDisabled\":false},\"pr\":{\"displayText\":\"Puerto Rico\",\"isDisabled\":false},\"ri\":{\"displayText\":\"Rhode Island\",\"isDisabled\":false},\"sc\":{\"displayText\":\"South Carolina\",\"isDisabled\":false},\"sd\":{\"displayText\":\"South Dakota\",\"isDisabled\":false},\"tn\":{\"displayText\":\"Tennessee\",\"isDisabled\":false},\"tx\":{\"displayText\":\"Texas\",\"isDisabled\":false},\"ut\":{\"displayText\":\"Utah\",\"isDisabled\":false},\"vt\":{\"displayText\":\"Vermont\",\"isDisabled\":false},\"va\":{\"displayText\":\"Virginia\",\"isDisabled\":false},\"wa\":{\"displayText\":\"Washington\",\"isDisabled\":false},\"wv\":{\"displayText\":\"West Virginia\",\"isDisabled\":false},\"wi\":{\"displayText\":\"Wisconsin\",\"isDisabled\":false},\"wy\":{\"displayText\":\"Wyoming\",\"isDisabled\":false}},\"displaySelectionText\":\"--Select--\",\"displayErrorMessages\":{\"defaultErrorMessage\":\"Incorrectly formatted state\",\"fromErrorCode\":[{\"errorCode\":\"required_field_empty\",\"errorMessage\":\"A required field is empty\"}]},\"displayId\":\"addressState\",\"displayType\":\"property\",\"propertyName\":\"State\",\"tags\":{\"theme\":\"windows\",\"accessibilityName\":\"State\"}},{\"displayName\":\"ZIP code\",\"showDisplayName\":\"true\",\"displayExample\":[\"20001\"],\"displayErrorMessages\":{\"defaultErrorMessage\":\"Incorrectly formatted ZIP code\",\"fromErrorCode\":[{\"errorCode\":\"required_field_empty\",\"errorMessage\":\"A required field is empty\"}]},\"displayId\":\"addressPostalCode\",\"displayType\":\"property\",\"propertyName\":\"ZipCode\",\"tags\":{\"accessibilityName\":\"ZIP code\"}},{\"displayName\":\"Record Id\",\"showDisplayName\":\"true\",\"displayExample\":[\"20001\"],\"displayErrorMessages\":{\"defaultErrorMessage\":\"Incorrectly formatted ZIP code\",\"fromErrorCode\":[{\"errorCode\":\"required_field_empty\",\"errorMessage\":\"A required field is empty\"}]},\"displayId\":\"recordIdDisplayId\",\"displayType\":\"property\",\"isHidden\":true,\"propertyName\":\"RecordId\",\"tags\":{\"accessibilityName\":\"ZIP code\"}},{\"displayName\":\"Country/Region\",\"showDisplayName\":\"true\",\"possibleValues\":{\"af\":\"Afghanistan\",\"ax\":\"\u00C5land Islands\",\"al\":\"Albania\",\"dz\":\"Algeria\",\"as\":\"American Samoa\",\"ad\":\"Andorra\",\"ao\":\"Angola\",\"ai\":\"Anguilla\",\"aq\":\"Antarctica\",\"ag\":\"Antigua and Barbuda\",\"ar\":\"Argentina\",\"am\":\"Armenia\",\"aw\":\"Aruba\",\"au\":\"Australia\",\"at\":\"Austria\",\"az\":\"Azerbaijan\",\"bs\":\"Bahamas\",\"bh\":\"Bahrain\",\"bd\":\"Bangladesh\",\"bb\":\"Barbados\",\"by\":\"Belarus\",\"be\":\"Belgium\",\"bz\":\"Belize\",\"bj\":\"Benin\",\"bm\":\"Bermuda\",\"bt\":\"Bhutan\",\"bo\":\"Bolivia\",\"bq\":\"Bonaire\",\"ba\":\"Bosnia and Herzegovina\",\"bw\":\"Botswana\",\"bv\":\"Bouvet Island\",\"br\":\"Brazil\",\"io\":\"British Indian Ocean Territory\",\"bn\":\"Brunei\",\"bg\":\"Bulgaria\",\"bf\":\"Burkina Faso\",\"bi\":\"Burundi\",\"kh\":\"Cambodia\",\"cm\":\"Cameroon\",\"ca\":\"Canada\",\"cv\":\"Cabo Verde\",\"ky\":\"Cayman Islands\",\"cf\":\"Central African Republic\",\"td\":\"Chad\",\"gg\":\"Channel Islands - Guernsey\",\"je\":\"Channel Islands - Jersey\",\"cl\":\"Chile\",\"cn\":\"China\",\"cx\":\"Christmas Island\",\"cc\":\"Cocos (Keeling) Islands\",\"co\":\"Colombia\",\"km\":\"Comoros\",\"cd\":\"Congo (DRC)\",\"cg\":\"Congo, Republic of\",\"ck\":\"Cook Islands\",\"cr\":\"Costa Rica\",\"ci\":\"C\u00F4te d'Ivoire (Ivory Coast)\",\"hr\":\"Croatia\",\"cw\":\"Cura\u00E7ao\",\"cy\":\"Cyprus\",\"cz\":\"Czech Republic\",\"dk\":\"Denmark\",\"dj\":\"Djibouti\",\"dm\":\"Dominica\",\"do\":\"Dominican Republic\",\"ec\":\"Ecuador\",\"eg\":\"Egypt\",\"sv\":\"El Salvador\",\"gq\":\"Equatorial Guinea\",\"er\":\"Eritrea\",\"ee\":\"Estonia\",\"et\":\"Ethiopia\",\"fk\":\"Falkland Islands\",\"fo\":\"Faroe Islands\",\"fj\":\"Fiji Islands\",\"fi\":\"Finland\",\"fr\":\"France\",\"gf\":\"French Guiana\",\"pf\":\"French Polynesia\",\"ga\":\"Gabon\",\"gm\":\"Gambia, The\",\"ge\":\"Georgia\",\"de\":\"Germany\",\"gh\":\"Ghana\",\"gi\":\"Gibraltar\",\"gr\":\"Greece\",\"gl\":\"Greenland\",\"gd\":\"Grenada\",\"gp\":\"Guadeloupe\",\"gu\":\"Guam\",\"gt\":\"Guatemala\",\"gn\":\"Guinea\",\"gw\":\"Guinea-Bissau\",\"gy\":\"Guyana\",\"ht\":\"Haiti\",\"hm\":\"Heard Island and McDonald Islands\",\"hn\":\"Honduras\",\"hk\":\"Hong Kong SAR\",\"hu\":\"Hungary\",\"is\":\"Iceland\",\"in\":\"India\",\"id\":\"Indonesia\",\"iq\":\"Iraq\",\"ie\":\"Ireland\",\"im\":\"Isle of Man\",\"il\":\"Israel\",\"it\":\"Italy\",\"jm\":\"Jamaica\",\"jp\":\"Japan\",\"jo\":\"Jordan\",\"kz\":\"Kazakhstan\",\"ke\":\"Kenya\",\"ki\":\"Kiribati\",\"kr\":\"Korea, Republic Of\",\"kw\":\"Kuwait\",\"kg\":\"Kyrgyzstan\",\"la\":\"Laos\",\"lv\":\"Latvia\",\"lb\":\"Lebanon\",\"ls\":\"Lesotho\",\"lr\":\"Liberia\",\"ly\":\"Libya\",\"li\":\"Liechtenstein\",\"lt\":\"Lithuania\",\"lu\":\"Luxembourg\",\"mo\":\"Macao SAR\",\"mk\":\"North Macedonia\",\"mg\":\"Madagascar\",\"mw\":\"Malawi\",\"my\":\"Malaysia\",\"mv\":\"Maldives\",\"ml\":\"Mali\",\"mt\":\"Malta\",\"mh\":\"Marshall Islands\",\"mq\":\"Martinique\",\"mr\":\"Mauritania\",\"mu\":\"Mauritius\",\"yt\":\"Mayotte\",\"mx\":\"Mexico\",\"fm\":\"Micronesia\",\"md\":\"Moldova\",\"mc\":\"Monaco\",\"mn\":\"Mongolia\",\"me\":\"Montenegro\",\"ms\":\"Montserrat\",\"ma\":\"Morocco\",\"mz\":\"Mozambique\",\"mm\":\"Myanmar\",\"na\":\"Namibia\",\"nr\":\"Nauru\",\"np\":\"Nepal\",\"nl\":\"Netherlands, The\",\"nc\":\"New Caledonia\",\"nz\":\"New Zealand\",\"ni\":\"Nicaragua\",\"ne\":\"Niger\",\"ng\":\"Nigeria\",\"nu\":\"Niue\",\"nf\":\"Norfolk Island\",\"mp\":\"Northern Mariana Islands\",\"no\":\"Norway\",\"om\":\"Oman\",\"pk\":\"Pakistan\",\"pw\":\"Palau\",\"ps\":\"Palestinian Authority\",\"pa\":\"Panama\",\"pg\":\"Papua New Guinea\",\"py\":\"Paraguay\",\"pe\":\"Peru\",\"ph\":\"Philippines\",\"pn\":\"Pitcairn Islands\",\"pl\":\"Poland\",\"pt\":\"Portugal\",\"pr\":\"Puerto Rico\",\"qa\":\"Qatar\",\"re\":\"R\u00E9union\",\"ro\":\"Romania\",\"ru\":\"Russia\",\"rw\":\"Rwanda\",\"bl\":\"Saint Barth\u00E9lemy\",\"sh\":\"St Helena, Ascension, Tristan da Cunha\",\"kn\":\"Saint Kitts and Nevis\",\"lc\":\"Saint Lucia\",\"mf\":\"Saint Martin (French part)\",\"pm\":\"Saint Pierre and Miquelon\",\"vc\":\"Saint Vincent and the Grenadines\",\"ws\":\"Samoa\",\"sm\":\"San Marino\",\"st\":\"Sao Tome and Principe\",\"sa\":\"Saudi Arabia\",\"sn\":\"Senegal\",\"rs\":\"Serbia\",\"sc\":\"Seychelles\",\"sl\":\"Sierra Leone\",\"sg\":\"Singapore\",\"sx\":\"Sint Maarten\",\"sk\":\"Slovakia\",\"si\":\"Slovenia\",\"sb\":\"Solomon Islands\",\"so\":\"Somalia\",\"za\":\"South Africa\",\"gs\":\"South Georgia and South Sandwich Islands\",\"es\":\"Spain\",\"lk\":\"Sri Lanka\",\"sr\":\"Suriname\",\"sj\":\"Svalbard\",\"sz\":\"Swaziland\",\"se\":\"Sweden\",\"ch\":\"Switzerland\",\"tw\":\"Taiwan\",\"tj\":\"Tajikistan\",\"tz\":\"Tanzania\",\"th\":\"Thailand\",\"tl\":\"Timor-Leste\",\"tg\":\"Togo\",\"tk\":\"Tokelau\",\"to\":\"Tonga\",\"tt\":\"Trinidad and Tobago\",\"tn\":\"Tunisia\",\"tr\":\"Turkey\",\"tm\":\"Turkmenistan\",\"tc\":\"Turks and Caicos Islands\",\"tv\":\"Tuvalu\",\"ug\":\"Uganda\",\"ua\":\"Ukraine\",\"ae\":\"United Arab Emirates\",\"gb\":\"United Kingdom\",\"USA\":\"United States\",\"um\":\"United States Minor Outlying Islands\",\"uy\":\"Uruguay\",\"uz\":\"Uzbekistan\",\"vu\":\"Vanuatu\",\"va\":\"Vatican City\",\"ve\":\"Venezuela\",\"vn\":\"Viet Nam\",\"vg\":\"Virgin Islands, British\",\"vi\":\"Virgin Islands, US\",\"wf\":\"Wallis and Futuna\",\"ye\":\"Yemen\",\"zm\":\"Zambia\",\"zw\":\"Zimbabwe\"},\"possibleOptions\":{\"af\":{\"displayText\":\"Afghanistan\",\"isDisabled\":false},\"ax\":{\"displayText\":\"\u00C5land Islands\",\"isDisabled\":false},\"al\":{\"displayText\":\"Albania\",\"isDisabled\":false},\"dz\":{\"displayText\":\"Algeria\",\"isDisabled\":false},\"as\":{\"displayText\":\"American Samoa\",\"isDisabled\":false},\"ad\":{\"displayText\":\"Andorra\",\"isDisabled\":false},\"ao\":{\"displayText\":\"Angola\",\"isDisabled\":false},\"ai\":{\"displayText\":\"Anguilla\",\"isDisabled\":false},\"aq\":{\"displayText\":\"Antarctica\",\"isDisabled\":false},\"ag\":{\"displayText\":\"Antigua and Barbuda\",\"isDisabled\":false},\"ar\":{\"displayText\":\"Argentina\",\"isDisabled\":false},\"am\":{\"displayText\":\"Armenia\",\"isDisabled\":false},\"aw\":{\"displayText\":\"Aruba\",\"isDisabled\":false},\"au\":{\"displayText\":\"Australia\",\"isDisabled\":false},\"at\":{\"displayText\":\"Austria\",\"isDisabled\":false},\"az\":{\"displayText\":\"Azerbaijan\",\"isDisabled\":false},\"bs\":{\"displayText\":\"Bahamas\",\"isDisabled\":false},\"bh\":{\"displayText\":\"Bahrain\",\"isDisabled\":false},\"bd\":{\"displayText\":\"Bangladesh\",\"isDisabled\":false},\"bb\":{\"displayText\":\"Barbados\",\"isDisabled\":false},\"by\":{\"displayText\":\"Belarus\",\"isDisabled\":false},\"be\":{\"displayText\":\"Belgium\",\"isDisabled\":false},\"bz\":{\"displayText\":\"Belize\",\"isDisabled\":false},\"bj\":{\"displayText\":\"Benin\",\"isDisabled\":false},\"bm\":{\"displayText\":\"Bermuda\",\"isDisabled\":false},\"bt\":{\"displayText\":\"Bhutan\",\"isDisabled\":false},\"bo\":{\"displayText\":\"Bolivia\",\"isDisabled\":false},\"bq\":{\"displayText\":\"Bonaire\",\"isDisabled\":false},\"ba\":{\"displayText\":\"Bosnia and Herzegovina\",\"isDisabled\":false},\"bw\":{\"displayText\":\"Botswana\",\"isDisabled\":false},\"bv\":{\"displayText\":\"Bouvet Island\",\"isDisabled\":false},\"br\":{\"displayText\":\"Brazil\",\"isDisabled\":false},\"io\":{\"displayText\":\"British Indian Ocean Territory\",\"isDisabled\":false},\"bn\":{\"displayText\":\"Brunei\",\"isDisabled\":false},\"bg\":{\"displayText\":\"Bulgaria\",\"isDisabled\":false},\"bf\":{\"displayText\":\"Burkina Faso\",\"isDisabled\":false},\"bi\":{\"displayText\":\"Burundi\",\"isDisabled\":false},\"kh\":{\"displayText\":\"Cambodia\",\"isDisabled\":false},\"cm\":{\"displayText\":\"Cameroon\",\"isDisabled\":false},\"ca\":{\"displayText\":\"Canada\",\"isDisabled\":false},\"cv\":{\"displayText\":\"Cabo Verde\",\"isDisabled\":false},\"ky\":{\"displayText\":\"Cayman Islands\",\"isDisabled\":false},\"cf\":{\"displayText\":\"Central African Republic\",\"isDisabled\":false},\"td\":{\"displayText\":\"Chad\",\"isDisabled\":false},\"gg\":{\"displayText\":\"Channel Islands - Guernsey\",\"isDisabled\":false},\"je\":{\"displayText\":\"Channel Islands - Jersey\",\"isDisabled\":false},\"cl\":{\"displayText\":\"Chile\",\"isDisabled\":false},\"cn\":{\"displayText\":\"China\",\"isDisabled\":false},\"cx\":{\"displayText\":\"Christmas Island\",\"isDisabled\":false},\"cc\":{\"displayText\":\"Cocos (Keeling) Islands\",\"isDisabled\":false},\"co\":{\"displayText\":\"Colombia\",\"isDisabled\":false},\"km\":{\"displayText\":\"Comoros\",\"isDisabled\":false},\"cd\":{\"displayText\":\"Congo (DRC)\",\"isDisabled\":false},\"cg\":{\"displayText\":\"Congo, Republic of\",\"isDisabled\":false},\"ck\":{\"displayText\":\"Cook Islands\",\"isDisabled\":false},\"cr\":{\"displayText\":\"Costa Rica\",\"isDisabled\":false},\"ci\":{\"displayText\":\"C\u00F4te d'Ivoire (Ivory Coast)\",\"isDisabled\":false},\"hr\":{\"displayText\":\"Croatia\",\"isDisabled\":false},\"cw\":{\"displayText\":\"Cura\u00E7ao\",\"isDisabled\":false},\"cy\":{\"displayText\":\"Cyprus\",\"isDisabled\":false},\"cz\":{\"displayText\":\"Czech Republic\",\"isDisabled\":false},\"dk\":{\"displayText\":\"Denmark\",\"isDisabled\":false},\"dj\":{\"displayText\":\"Djibouti\",\"isDisabled\":false},\"dm\":{\"displayText\":\"Dominica\",\"isDisabled\":false},\"do\":{\"displayText\":\"Dominican Republic\",\"isDisabled\":false},\"ec\":{\"displayText\":\"Ecuador\",\"isDisabled\":false},\"eg\":{\"displayText\":\"Egypt\",\"isDisabled\":false},\"sv\":{\"displayText\":\"El Salvador\",\"isDisabled\":false},\"gq\":{\"displayText\":\"Equatorial Guinea\",\"isDisabled\":false},\"er\":{\"displayText\":\"Eritrea\",\"isDisabled\":false},\"ee\":{\"displayText\":\"Estonia\",\"isDisabled\":false},\"et\":{\"displayText\":\"Ethiopia\",\"isDisabled\":false},\"fk\":{\"displayText\":\"Falkland Islands\",\"isDisabled\":false},\"fo\":{\"displayText\":\"Faroe Islands\",\"isDisabled\":false},\"fj\":{\"displayText\":\"Fiji Islands\",\"isDisabled\":false},\"fi\":{\"displayText\":\"Finland\",\"isDisabled\":false},\"fr\":{\"displayText\":\"France\",\"isDisabled\":false},\"gf\":{\"displayText\":\"French Guiana\",\"isDisabled\":false},\"pf\":{\"displayText\":\"French Polynesia\",\"isDisabled\":false},\"ga\":{\"displayText\":\"Gabon\",\"isDisabled\":false},\"gm\":{\"displayText\":\"Gambia, The\",\"isDisabled\":false},\"ge\":{\"displayText\":\"Georgia\",\"isDisabled\":false},\"de\":{\"displayText\":\"Germany\",\"isDisabled\":false},\"gh\":{\"displayText\":\"Ghana\",\"isDisabled\":false},\"gi\":{\"displayText\":\"Gibraltar\",\"isDisabled\":false},\"gr\":{\"displayText\":\"Greece\",\"isDisabled\":false},\"gl\":{\"displayText\":\"Greenland\",\"isDisabled\":false},\"gd\":{\"displayText\":\"Grenada\",\"isDisabled\":false},\"gp\":{\"displayText\":\"Guadeloupe\",\"isDisabled\":false},\"gu\":{\"displayText\":\"Guam\",\"isDisabled\":false},\"gt\":{\"displayText\":\"Guatemala\",\"isDisabled\":false},\"gn\":{\"displayText\":\"Guinea\",\"isDisabled\":false},\"gw\":{\"displayText\":\"Guinea-Bissau\",\"isDisabled\":false},\"gy\":{\"displayText\":\"Guyana\",\"isDisabled\":false},\"ht\":{\"displayText\":\"Haiti\",\"isDisabled\":false},\"hm\":{\"displayText\":\"Heard Island and McDonald Islands\",\"isDisabled\":false},\"hn\":{\"displayText\":\"Honduras\",\"isDisabled\":false},\"hk\":{\"displayText\":\"Hong Kong SAR\",\"isDisabled\":false},\"hu\":{\"displayText\":\"Hungary\",\"isDisabled\":false},\"is\":{\"displayText\":\"Iceland\",\"isDisabled\":false},\"in\":{\"displayText\":\"India\",\"isDisabled\":false},\"id\":{\"displayText\":\"Indonesia\",\"isDisabled\":false},\"iq\":{\"displayText\":\"Iraq\",\"isDisabled\":false},\"ie\":{\"displayText\":\"Ireland\",\"isDisabled\":false},\"im\":{\"displayText\":\"Isle of Man\",\"isDisabled\":false},\"il\":{\"displayText\":\"Israel\",\"isDisabled\":false},\"it\":{\"displayText\":\"Italy\",\"isDisabled\":false},\"jm\":{\"displayText\":\"Jamaica\",\"isDisabled\":false},\"jp\":{\"displayText\":\"Japan\",\"isDisabled\":false},\"jo\":{\"displayText\":\"Jordan\",\"isDisabled\":false},\"kz\":{\"displayText\":\"Kazakhstan\",\"isDisabled\":false},\"ke\":{\"displayText\":\"Kenya\",\"isDisabled\":false},\"ki\":{\"displayText\":\"Kiribati\",\"isDisabled\":false},\"kr\":{\"displayText\":\"Korea, Republic Of\",\"isDisabled\":false},\"kw\":{\"displayText\":\"Kuwait\",\"isDisabled\":false},\"kg\":{\"displayText\":\"Kyrgyzstan\",\"isDisabled\":false},\"la\":{\"displayText\":\"Laos\",\"isDisabled\":false},\"lv\":{\"displayText\":\"Latvia\",\"isDisabled\":false},\"lb\":{\"displayText\":\"Lebanon\",\"isDisabled\":false},\"ls\":{\"displayText\":\"Lesotho\",\"isDisabled\":false},\"lr\":{\"displayText\":\"Liberia\",\"isDisabled\":false},\"ly\":{\"displayText\":\"Libya\",\"isDisabled\":false},\"li\":{\"displayText\":\"Liechtenstein\",\"isDisabled\":false},\"lt\":{\"displayText\":\"Lithuania\",\"isDisabled\":false},\"lu\":{\"displayText\":\"Luxembourg\",\"isDisabled\":false},\"mo\":{\"displayText\":\"Macao SAR\",\"isDisabled\":false},\"mk\":{\"displayText\":\"North Macedonia\",\"isDisabled\":false},\"mg\":{\"displayText\":\"Madagascar\",\"isDisabled\":false},\"mw\":{\"displayText\":\"Malawi\",\"isDisabled\":false},\"my\":{\"displayText\":\"Malaysia\",\"isDisabled\":false},\"mv\":{\"displayText\":\"Maldives\",\"isDisabled\":false},\"ml\":{\"displayText\":\"Mali\",\"isDisabled\":false},\"mt\":{\"displayText\":\"Malta\",\"isDisabled\":false},\"mh\":{\"displayText\":\"Marshall Islands\",\"isDisabled\":false},\"mq\":{\"displayText\":\"Martinique\",\"isDisabled\":false},\"mr\":{\"displayText\":\"Mauritania\",\"isDisabled\":false},\"mu\":{\"displayText\":\"Mauritius\",\"isDisabled\":false},\"yt\":{\"displayText\":\"Mayotte\",\"isDisabled\":false},\"mx\":{\"displayText\":\"Mexico\",\"isDisabled\":false},\"fm\":{\"displayText\":\"Micronesia\",\"isDisabled\":false},\"md\":{\"displayText\":\"Moldova\",\"isDisabled\":false},\"mc\":{\"displayText\":\"Monaco\",\"isDisabled\":false},\"mn\":{\"displayText\":\"Mongolia\",\"isDisabled\":false},\"me\":{\"displayText\":\"Montenegro\",\"isDisabled\":false},\"ms\":{\"displayText\":\"Montserrat\",\"isDisabled\":false},\"ma\":{\"displayText\":\"Morocco\",\"isDisabled\":false},\"mz\":{\"displayText\":\"Mozambique\",\"isDisabled\":false},\"mm\":{\"displayText\":\"Myanmar\",\"isDisabled\":false},\"na\":{\"displayText\":\"Namibia\",\"isDisabled\":false},\"nr\":{\"displayText\":\"Nauru\",\"isDisabled\":false},\"np\":{\"displayText\":\"Nepal\",\"isDisabled\":false},\"nl\":{\"displayText\":\"Netherlands, The\",\"isDisabled\":false},\"nc\":{\"displayText\":\"New Caledonia\",\"isDisabled\":false},\"nz\":{\"displayText\":\"New Zealand\",\"isDisabled\":false},\"ni\":{\"displayText\":\"Nicaragua\",\"isDisabled\":false},\"ne\":{\"displayText\":\"Niger\",\"isDisabled\":false},\"ng\":{\"displayText\":\"Nigeria\",\"isDisabled\":false},\"nu\":{\"displayText\":\"Niue\",\"isDisabled\":false},\"nf\":{\"displayText\":\"Norfolk Island\",\"isDisabled\":false},\"mp\":{\"displayText\":\"Northern Mariana Islands\",\"isDisabled\":false},\"no\":{\"displayText\":\"Norway\",\"isDisabled\":false},\"om\":{\"displayText\":\"Oman\",\"isDisabled\":false},\"pk\":{\"displayText\":\"Pakistan\",\"isDisabled\":false},\"pw\":{\"displayText\":\"Palau\",\"isDisabled\":false},\"ps\":{\"displayText\":\"Palestinian Authority\",\"isDisabled\":false},\"pa\":{\"displayText\":\"Panama\",\"isDisabled\":false},\"pg\":{\"displayText\":\"Papua New Guinea\",\"isDisabled\":false},\"py\":{\"displayText\":\"Paraguay\",\"isDisabled\":false},\"pe\":{\"displayText\":\"Peru\",\"isDisabled\":false},\"ph\":{\"displayText\":\"Philippines\",\"isDisabled\":false},\"pn\":{\"displayText\":\"Pitcairn Islands\",\"isDisabled\":false},\"pl\":{\"displayText\":\"Poland\",\"isDisabled\":false},\"pt\":{\"displayText\":\"Portugal\",\"isDisabled\":false},\"pr\":{\"displayText\":\"Puerto Rico\",\"isDisabled\":false},\"qa\":{\"displayText\":\"Qatar\",\"isDisabled\":false},\"re\":{\"displayText\":\"R\u00E9union\",\"isDisabled\":false},\"ro\":{\"displayText\":\"Romania\",\"isDisabled\":false},\"ru\":{\"displayText\":\"Russia\",\"isDisabled\":false},\"rw\":{\"displayText\":\"Rwanda\",\"isDisabled\":false},\"bl\":{\"displayText\":\"Saint Barth\u00E9lemy\",\"isDisabled\":false},\"sh\":{\"displayText\":\"St Helena, Ascension, Tristan da Cunha\",\"isDisabled\":false},\"kn\":{\"displayText\":\"Saint Kitts and Nevis\",\"isDisabled\":false},\"lc\":{\"displayText\":\"Saint Lucia\",\"isDisabled\":false},\"mf\":{\"displayText\":\"Saint Martin (French part)\",\"isDisabled\":false},\"pm\":{\"displayText\":\"Saint Pierre and Miquelon\",\"isDisabled\":false},\"vc\":{\"displayText\":\"Saint Vincent and the Grenadines\",\"isDisabled\":false},\"ws\":{\"displayText\":\"Samoa\",\"isDisabled\":false},\"sm\":{\"displayText\":\"San Marino\",\"isDisabled\":false},\"st\":{\"displayText\":\"Sao Tome and Principe\",\"isDisabled\":false},\"sa\":{\"displayText\":\"Saudi Arabia\",\"isDisabled\":false},\"sn\":{\"displayText\":\"Senegal\",\"isDisabled\":false},\"rs\":{\"displayText\":\"Serbia\",\"isDisabled\":false},\"sc\":{\"displayText\":\"Seychelles\",\"isDisabled\":false},\"sl\":{\"displayText\":\"Sierra Leone\",\"isDisabled\":false},\"sg\":{\"displayText\":\"Singapore\",\"isDisabled\":false},\"sx\":{\"displayText\":\"Sint Maarten\",\"isDisabled\":false},\"sk\":{\"displayText\":\"Slovakia\",\"isDisabled\":false},\"si\":{\"displayText\":\"Slovenia\",\"isDisabled\":false},\"sb\":{\"displayText\":\"Solomon Islands\",\"isDisabled\":false},\"so\":{\"displayText\":\"Somalia\",\"isDisabled\":false},\"za\":{\"displayText\":\"South Africa\",\"isDisabled\":false},\"gs\":{\"displayText\":\"South Georgia and South Sandwich Islands\",\"isDisabled\":false},\"es\":{\"displayText\":\"Spain\",\"isDisabled\":false},\"lk\":{\"displayText\":\"Sri Lanka\",\"isDisabled\":false},\"sr\":{\"displayText\":\"Suriname\",\"isDisabled\":false},\"sj\":{\"displayText\":\"Svalbard\",\"isDisabled\":false},\"sz\":{\"displayText\":\"Swaziland\",\"isDisabled\":false},\"se\":{\"displayText\":\"Sweden\",\"isDisabled\":false},\"ch\":{\"displayText\":\"Switzerland\",\"isDisabled\":false},\"tw\":{\"displayText\":\"Taiwan\",\"isDisabled\":false},\"tj\":{\"displayText\":\"Tajikistan\",\"isDisabled\":false},\"tz\":{\"displayText\":\"Tanzania\",\"isDisabled\":false},\"th\":{\"displayText\":\"Thailand\",\"isDisabled\":false},\"tl\":{\"displayText\":\"Timor-Leste\",\"isDisabled\":false},\"tg\":{\"displayText\":\"Togo\",\"isDisabled\":false},\"tk\":{\"displayText\":\"Tokelau\",\"isDisabled\":false},\"to\":{\"displayText\":\"Tonga\",\"isDisabled\":false},\"tt\":{\"displayText\":\"Trinidad and Tobago\",\"isDisabled\":false},\"tn\":{\"displayText\":\"Tunisia\",\"isDisabled\":false},\"tr\":{\"displayText\":\"Turkey\",\"isDisabled\":false},\"tm\":{\"displayText\":\"Turkmenistan\",\"isDisabled\":false},\"tc\":{\"displayText\":\"Turks and Caicos Islands\",\"isDisabled\":false},\"tv\":{\"displayText\":\"Tuvalu\",\"isDisabled\":false},\"ug\":{\"displayText\":\"Uganda\",\"isDisabled\":false},\"ua\":{\"displayText\":\"Ukraine\",\"isDisabled\":false},\"ae\":{\"displayText\":\"United Arab Emirates\",\"isDisabled\":false},\"gb\":{\"displayText\":\"United Kingdom\",\"isDisabled\":false},\"USA\":{\"displayText\":\"United States\",\"isDisabled\":false},\"um\":{\"displayText\":\"United States Minor Outlying Islands\",\"isDisabled\":false},\"uy\":{\"displayText\":\"Uruguay\",\"isDisabled\":false},\"uz\":{\"displayText\":\"Uzbekistan\",\"isDisabled\":false},\"vu\":{\"displayText\":\"Vanuatu\",\"isDisabled\":false},\"va\":{\"displayText\":\"Vatican City\",\"isDisabled\":false},\"ve\":{\"displayText\":\"Venezuela\",\"isDisabled\":false},\"vn\":{\"displayText\":\"Viet Nam\",\"isDisabled\":false},\"vg\":{\"displayText\":\"Virgin Islands, British\",\"isDisabled\":false},\"vi\":{\"displayText\":\"Virgin Islands, US\",\"isDisabled\":false},\"wf\":{\"displayText\":\"Wallis and Futuna\",\"isDisabled\":false},\"ye\":{\"displayText\":\"Yemen\",\"isDisabled\":false},\"zm\":{\"displayText\":\"Zambia\",\"isDisabled\":false},\"zw\":{\"displayText\":\"Zimbabwe\",\"isDisabled\":false}},\"displaySelectionText\":\"--Select--\",\"displayErrorMessages\":{\"defaultErrorMessage\":\"Incorrectly formatted country\",\"fromErrorCode\":[{\"errorCode\":\"required_field_empty\",\"errorMessage\":\"A required field is empty\"}]},\"displayId\":\"addressCountry\",\"displayType\":\"property\",\"isDisabled\":true,\"propertyName\":\"ThreeLetterISORegionName\",\"tags\":{\"theme\":\"windows\",\"accessibilityName\":\"Country/Region\"}},{\"isSubmitGroup\":true,\"layoutOrientation\":\"inline\",\"members\":[{\"displayContent\":\"Cancel\",\"displayId\":\"cancelButton\",\"displayType\":\"button\",\"isBack\":true,\"pidlAction\":{\"type\":\"gohome\",\"context\":\"\"},\"tags\":{\"accessibilityName\":\"Cancel\"}},{\"displayContent\":\"Save\",\"displayId\":\"saveButton\",\"displayType\":\"button\",\"isHighlighted\":true,\"pidlAction\":{\"type\":\"submit\",\"isDefault\":true,\"context\":{\"href\":\"https://{jarvis-endpoint}/JarvisCM/{userId}/addresses\",\"method\":\"POST\",\"headers\":{\"api-version\":\"2015-03-31\",\"x-ms-correlation-id\":\"017cc7b7-477d-4dec-ad94-913fa5b9006d\",\"x-ms-tracking-id\":\"0fad4c50-7f79-4d7e-aa5a-904db15d8803\"}}},\"tags\":{\"accessibilityName\":\"Save\"}}],\"displayId\":\"cancelNextGroup\",\"displayType\":\"group\"}],\"displayId\":\"addressA1A2CtStPcCoWithButtonPage\",\"displayType\":\"page\"}],\"strings\":{\"serverErrorCodes\":{\"InvalidAddress\":{\"ErrorMessage\":\"Check your address. There appears to be an error in it.\",\"Target\":\"postal_code,city,region\"},\"InvalidAddressFieldsCombination\":{\"ErrorMessage\":\"Check your address. There appears to be an error in it.\",\"Target\":\"postal_code,city,region\"},\"InvalidCity\":{\"ErrorMessage\":\"Check the city in your address. There appears to be an error in it.\",\"Target\":\"city\"},\"InvalidPostalCode\":{\"ErrorMessage\":\"Check the Zip or Postal code in your address. There appears to be an error in it.\",\"Target\":\"postal_code\"},\"InvalidRegion\":{\"ErrorMessage\":\"Check the state in your address. There appears to be an error in it.\",\"Target\":\"region\"}}}}]");
            }
            else if (string.Equals(operation, Constants.Operations.SelectInstance, StringComparison.InvariantCultureIgnoreCase))
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject("[{\"identity\":{\"description_type\":\"addressGroup\",\"operation\":\"selectinstance\",\"country\":\"us\"},\"data_description\":{\"addressGroupOperation\":{\"propertyType\":\"clientData\",\"type\":\"hidden\",\"dataType\":\"hidden\",\"is_updatable\":true,\"default_value\":\"selectinstance\"},\"addressGroupCountry\":{\"propertyType\":\"clientData\",\"type\":\"hidden\",\"dataType\":\"hidden\",\"is_updatable\":true,\"default_value\":\"us\"},\"RecordId\":{\"propertyType\":\"userData\",\"type\":\"string\",\"dataType\":\"string\",\"is_key\":false,\"is_optional\":false,\"is_updatable\":true,\"possible_values\":{}}},\"displayDescription\":[{\"displayName\":\"addressGroupSelectionPage\",\"members\":[{\"displayContent\":\"Your addresses\",\"displayId\":\"shippingAddressPageHeading\",\"displayType\":\"heading\"},{\"displayName\":\"AddressGroup\",\"showDisplayName\":\"false\",\"dataCollectionSource\":\"partnerData.prefillData\",\"possibleOptions\":{\"{RecordId}\":{\"displayText\":\"{FullAddress}\",\"isDisabled\":false}},\"selectType\":\"dropDown\",\"displayId\":\"addressGroup\",\"displayType\":\"property\",\"propertyName\":\"RecordId\"},{\"isSubmitGroup\":true,\"layoutOrientation\":\"inline\",\"members\":[{\"displayContent\":\"Add a new address\",\"displayId\":\"newAddressButton\",\"displayType\":\"button\",\"isHighlighted\":true,\"pidlAction\":{\"type\":\"success\",\"context\":{\"action\":\"addResource\",\"resourceActionContext\":{\"action\":\"addResource\",\"pidlDocInfo\":{\"resourceType\":\"Address\",\"parameters\":{\"language\":\"en-us\",\"country\":\"us\",\"partner\":\"webblends\"}}}}},\"tags\":{\"accessibilityName\":\"Add a new address\"}},{\"displayContent\":\"Update current address\",\"displayId\":\"updateAddressButton\",\"displayType\":\"button\",\"isHighlighted\":false,\"pidlAction\":{\"type\":\"successWithPidlPayload\",\"context\":{\"action\":\"updateResource\",\"resourceActionContext\":{\"action\":\"updateResource\",\"pidlDocInfo\":{\"resourceType\":\"Address\",\"parameters\":{\"language\":\"en-us\",\"country\":\"us\",\"partner\":\"webblends\"}}}}},\"tags\":{\"accessibilityName\":\"Update a new address\"}}],\"displayId\":\"cancelNextGroup\",\"displayType\":\"group\"}],\"displayId\":\"addressGroupSelectPage\",\"displayType\":\"page\"}]}]");
            }
            else
            {
                throw new NotSupportedException("Supported operations are: add, update and selectInstance");
            }
        }

        private static void AdjustPropertiesInPIDL(string type, string partner, string scenario, string operation, List<PIDLResource> retVal, List<string> exposedFlightFeatures, string country = null, string language = null, string addressId = null, PaymentExperienceSetting setting = null)
        {
            if (string.Equals(partner, Constants.PartnerName.WebPay, StringComparison.OrdinalIgnoreCase))
            {
                foreach (PIDLResource resource in retVal)
                {
                    resource.SetPropertyState("country", true);
                }
            }

            // This is a temp solution to make three fields for Webpay, Cart, Consumersupport, and XBox native shipping addresses to mandatory : first_name, last_name and phone_number
            // After PIDL SDK supports a feature to define optional/mandatory field in DisplayDescription, this code will be removed.
            // Task 16371783 Remove the logic on PX side for making phone number mandatroy in shipping address for Cart and Webpay
            if (string.Equals(partner, Constants.PartnerName.WebPay, StringComparison.OrdinalIgnoreCase)
                || string.Equals(partner, Constants.PartnerName.Cart, StringComparison.OrdinalIgnoreCase)
                || string.Equals(partner, Constants.PartnerName.ConsumerSupport, StringComparison.OrdinalIgnoreCase)
                || PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner)
                || (string.Equals(partner, Constants.PartnerName.AmcWeb, StringComparison.OrdinalIgnoreCase) && string.Equals(scenario, Constants.ScenarioNames.ProfileAddress, StringComparison.OrdinalIgnoreCase)))
            {
                string[] propertyNames = { "first_name", "last_name", "phone_number" };
                ProxyController.UpdateIsOptionalProperty(retVal, propertyNames, false);
            }

            if (string.Equals(partner, Constants.PartnerName.CommercialStores, StringComparison.OrdinalIgnoreCase)
                && scenario == GlobalConstants.ScenarioNames.Commercialhardware)
            {
                string[] propertyNames = { "first_name", "last_name", "email_address", "phone_number" };
                ProxyController.UpdateIsOptionalProperty(retVal, propertyNames, false);
            }

            // cleaning a payload for hapi, Bug 21254954 Commercialstores : SUA form submit fails due to extra fields
            // Enable the tempalte partner check, to sync with the commercialstores partner.
            if ((string.Equals(partner, Constants.PartnerName.CommercialStores, StringComparison.OrdinalIgnoreCase)
                || PIDLResourceFactory.IsTemplateInList(partner, setting, Constants.DescriptionTypes.ProfileDescription, GlobalConstants.ProfileTypes.Organization))
                && string.Equals(type, Constants.AddressTypes.HapiServiceUsageAddress, StringComparison.OrdinalIgnoreCase))
            {
                foreach (PIDLResource pidl in retVal)
                {
                    // properties under "data_description"
                    pidl.RemoveFirstDataDescriptionByPropertyName(Constants.DataDescriptionPropertyNames.AddressCountry);
                    pidl.RemoveFirstDataDescriptionByPropertyName(Constants.DataDescriptionPropertyNames.AddressType);

                    // properties under "data_description.value.data_description"
                    pidl.RemoveFirstDataDescriptionByPropertyName(Constants.DataDescriptionPropertyNames.DataCountry);
                    pidl.RemoveFirstDataDescriptionByPropertyName(Constants.DataDescriptionPropertyNames.DataOperation);
                    pidl.RemoveFirstDataDescriptionByPropertyName(Constants.DataDescriptionPropertyNames.DataType);

                    // properties under "data_description.value.data_description.serviceUsageAddress"
                    pidl.RemoveFirstDataDescriptionByPropertyName(Constants.DataDescriptionPropertyNames.DataCountry);
                    pidl.RemoveFirstDataDescriptionByPropertyName(Constants.DataDescriptionPropertyNames.DataOperation);
                    pidl.RemoveFirstDataDescriptionByPropertyName(Constants.DataDescriptionPropertyNames.DataType);
                }
            }

            if (string.Equals(partner, Constants.PartnerName.CommercialStores, StringComparison.OrdinalIgnoreCase)
                && (string.Equals(type, Constants.AddressTypes.HapiV1SoldToOrganization, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(type, Constants.AddressTypes.HapiV1SoldToOrganizationCSP, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(type, Constants.AddressTypes.HapiV1ShipToOrganization, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(type, Constants.AddressTypes.HapiV1SoldToIndividual, StringComparison.OrdinalIgnoreCase)))
            {
                foreach (PIDLResource pidl in retVal)
                {
                    // properties under "data_description"
                    pidl.RemoveFirstDataDescriptionByPropertyName(Constants.DataDescriptionPropertyNames.AddressCountry);
                    pidl.RemoveFirstDataDescriptionByPropertyName(Constants.DataDescriptionPropertyNames.AddressType);

                    if (PartnerHelper.IsCommercialStoresPartner(partner)
                        && string.Equals(operation, Constants.Operations.Update, StringComparison.OrdinalIgnoreCase)
                        && (string.Equals(type, Constants.AddressTypes.HapiV1SoldToOrganization, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(type, Constants.AddressTypes.HapiV1SoldToIndividual, StringComparison.OrdinalIgnoreCase)))
                    {
                        DisplayHint companyNameDisplayHint = pidl.GetDisplayHintByPropertyName("companyName");
                        if (companyNameDisplayHint != null)
                        {
                            companyNameDisplayHint.IsDisabled = true;
                        }
                    }
                }
            }

            if (string.Equals(partner, Constants.PartnerName.CommercialStores, StringComparison.OrdinalIgnoreCase)
                && string.Equals(type, Constants.AddressTypes.HapiV1SoldToOrganizationCSP, StringComparison.OrdinalIgnoreCase)
                && string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase))
            {
                foreach (PIDLResource pidl in retVal)
                {
                    PropertyDescription countryProperty = pidl.GetPropertyDescriptionByPropertyName("country");
                    if (countryProperty != null)
                    {
                        countryProperty.DefaultValue = country;
                    }

                    DisplayHint countryDisplayHint = pidl.GetDisplayHintByPropertyName("country");
                    if (countryDisplayHint != null)
                    {
                        countryDisplayHint.IsDisabled = true;
                    }
                }
            }

            if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner)
                && string.Equals(operation, Constants.Operations.Update, StringComparison.OrdinalIgnoreCase)
                && (string.Equals(scenario, Constants.AddressTypes.Shipping, StringComparison.OrdinalIgnoreCase) || string.Equals(scenario, Constants.AddressTypes.Billing, StringComparison.OrdinalIgnoreCase)))
            {
                UpdatePageHeadersForEditAddress(retVal, scenario, language);
                SetPXV3AddressId(retVal, type, addressId);
            }
        }

        private static void SetPXV3AddressId(List<PIDLResource> retVal, string type, string addressId)
        {
            string key = string.Equals(type, Constants.AddressTypes.PXV3Shipping, StringComparison.OrdinalIgnoreCase) ? Constants.DescriptionTypes.AddressShippingV3 : Constants.DescriptionTypes.AddressBillingV3;

            PIDLResource addressInfo = (retVal.First().DataDescription[key] as List<PIDLResource>).First();
            PropertyDescription id = addressInfo.DataDescription["id"] as PropertyDescription;

            id.DefaultValue = addressId;
        }

        private static void UpdatePageHeadersForEditAddress(List<PIDLResource> retVal, string scenario, string language)
        {
            bool isShippingScenario = string.Equals(scenario, Constants.AddressTypes.Shipping, StringComparison.OrdinalIgnoreCase);

            string headingText = isShippingScenario
                ? PidlModelHelper.GetLocalizedString("Update your shipping address", language)
                : PidlModelHelper.GetLocalizedString("Update your billing address", language);

            PIDLResource firstResource = retVal.First<PIDLResource>();
            PageDisplayHint firstDisplayPage = firstResource?.DisplayPages?.First<PageDisplayHint>();
            HeadingDisplayHint headingDisplayHint = firstDisplayPage?.Members?.First<DisplayHint>() as HeadingDisplayHint;

            if (headingDisplayHint != null)
            {
                headingDisplayHint.DisplayContent = headingText;
            }

            if (isShippingScenario)
            {
                PageDisplayHint secondDisplayPage = firstResource?.DisplayPages[1];
                HeadingDisplayHint page2HeadingDisplayHint = secondDisplayPage?.Members?.First<DisplayHint>() as HeadingDisplayHint;

                if (page2HeadingDisplayHint != null)
                {
                    page2HeadingDisplayHint.DisplayContent = headingText;
                }
            }
        }

        private static bool ShowAVSSuggestionsForAMCWebProfileAddress(List<string> exposedFlightFeatures, string partner, string scenario, string country, PaymentExperienceSetting setting = null)
        {
            var templateOrPartner = ProxyController.GetSettingTemplate(partner, setting, Constants.DescriptionTypes.Addresses, Constants.AddressTypes.ShippingV3);

            return (string.Equals(partner, Constants.PartnerName.AmcWeb, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(templateOrPartner, Constants.TemplateName.DefaultTemplate, StringComparison.OrdinalIgnoreCase))
                    && string.Equals(scenario, Constants.ScenarioNames.ProfileAddress, StringComparison.OrdinalIgnoreCase)
                    && exposedFlightFeatures.Contains(Constants.PartnerFlightValues.ShowAVSSuggestions, StringComparer.OrdinalIgnoreCase)
                    && Constants.CountriesNeedsToShowAVSSuggestionsForAmcWeb.Contains(country, StringComparer.OrdinalIgnoreCase);
        }

        private static void LinkTaxPidl(List<PIDLResource> retVal, string country, string language, string partner, string operation, PaymentExperienceSetting setting, List<string> exposedFlightFeatures)
        {
            string taxOperation = operation ?? Constants.Operations.UpdatePatch;
            if (string.Equals(operation, Constants.Operations.Update, StringComparison.OrdinalIgnoreCase))
            {
                taxOperation = Constants.Operations.UpdatePatch;
            }

            List<PIDLResource> taxIdPidls = PIDLResourceFactory.Instance.GetTaxIdDescriptions(
                country,
                Constants.TaxIdTypes.Commercial,
                language,
                partner,
                GlobalConstants.ProfileTypes.Legal,
                taxOperation,
                setting: setting);

            if (string.Equals(country, Constants.CommercialTaxIdCountryRegionCodes.Italy, StringComparison.OrdinalIgnoreCase))
            {
                taxIdPidls = PIDLResourceFactory.BuildItalyTaxIDForm(taxIdPidls, exposedFlightFeatures, partner, country, setting, false, true);
            }

            if (string.Equals(country, Constants.CommercialTaxIdCountryRegionCodes.Egypt, StringComparison.OrdinalIgnoreCase))
            {
                taxIdPidls = PIDLResourceFactory.BuildEgyptTaxIDForm(taxIdPidls, exposedFlightFeatures, partner, country, setting, false, true);
            }

            taxIdPidls.ForEach(r => r.MakeSecondaryResource());

            if (string.Equals(country, Constants.CountryCodes.India, StringComparison.OrdinalIgnoreCase))
            {
                foreach (PIDLResource taxIdPidl in taxIdPidls)
                {
                    // GSTID depends on state
                    // get state from parent's pidl display property
                    // this override is needed because of hapi sua format that uses 'state'
                    // when in all other cases, like legalentity profile address or credit card address, it's 'region' not 'state'
                    if (taxIdPidl.DataDescription.ContainsKey(Constants.HapiTaxIdDataDescriptionPropertyNames.State))
                    {
                        taxIdPidl.GetPropertyDescriptionByPropertyName(Constants.HapiTaxIdDataDescriptionPropertyNames.State).DisplayProperty = "state";
                    }
                }
            }

            if (retVal != null && taxIdPidls.Count > 0 && taxIdPidls[0].DisplayPages != null)
            {
                int taxIdPidlCounter = 0;
                foreach (PIDLResource taxIdPidl in taxIdPidls)
                {
                    if (taxIdPidl.DisplayPages != null)
                    {
                        PIDLResourceFactory.AddLinkedPidlToResourceList(retVal, taxIdPidl, partner, PidlContainerDisplayHint.SubmissionOrder.AfterBase, taxIdPidlCounter++);
                    }
                }
            }
        }
    }
}