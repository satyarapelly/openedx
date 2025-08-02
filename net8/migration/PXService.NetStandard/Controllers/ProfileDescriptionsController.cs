// <copyright file="ProfileDescriptionsController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;

    public class ProfileDescriptionsController : ProxyController
    {
        /// <summary>
        /// Get Profile Descriptions
        /// </summary>
        /// <group>ProfileDescriptions</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/ProfileDescriptions</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="country" required="true" cref="string" in="query">Two letter country code</param>
        /// <param name="type" required="true" cref="string" in="query">address type</param>
        /// <param name="operation" required="false" cref="string" in="query">operation name</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <param name="scenario" required="false" cref="object" in="body">scenario name</param>
        /// <response code="200">A list of PIDLResource</response>
        /// <returns>A list of PIDLResource object</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Needs to be an instance method for Route action selection")]
        [HttpGet]
        public async Task<List<PIDLResource>> GetByCountry(string accountId, string country, string type, string operation = Constants.Operations.Add, string language = null, string partner = Constants.ServiceDefaults.DefaultPartnerName, string scenario = null)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            accountId = accountId + string.Empty;
            HashSet<string> partnerFlights = new HashSet<string>();

            string profileId = null;
            bool overrideJarvisVersionToV3 = false;
            Dictionary<string, string> profileV3Headers = null;

            // Use Partner Settings if enabled for the partner
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(operation);
            this.EnableFlightingsInPartnerSetting(setting, country);

            // In today's prod, update emp/org profile sends a patch call to Jarvis
            // Our new target is to send patch call to HAPI service, migration work is in progress, with flight name PXProfileUpdateToHapi
            // Remove the following if statement once migration is completed
            // Task 23089363: [PxService] Remove service side (from Jarvis) prefilling code for org/emp profile
            if (string.Equals(operation, Constants.Operations.Update, StringComparison.OrdinalIgnoreCase))
            {
                overrideJarvisVersionToV3 = true;
                profileV3Headers = new Dictionary<string, string>();

                if (IsServicePrefillEtagRequired(type, this.ExposedFlightFeatures, scenario, partner, setting, country))
                {
                    AccountProfileV3 profile = await this.Settings.AccountServiceAccessor.GetProfileV3(accountId, this.GetProfileType(), traceActivityId);

                    if (profile == null)
                    {
                        throw new InvalidOperationException("Parameter operation is not valid with the content of profile object");
                    }

                    profileId = profile.Id;
                    profileV3Headers.Add(AccountService.V7.Constants.AccountV3ExtendedHttpHeaders.Etag, profile.Etag);
                    profileV3Headers.Add(AccountService.V7.Constants.AccountV3ExtendedHttpHeaders.IfMatch, profile.Etag);
                }
                else if (IsClientPrefillEtagRequired(type, this.ExposedFlightFeatures, scenario, partner, setting, country))
                {
                    profileV3Headers.Add(AccountService.V7.Constants.AccountV3ExtendedHttpHeaders.Etag, Constants.PidlTemplatePath.Etag);
                    profileV3Headers.Add(AccountService.V7.Constants.AccountV3ExtendedHttpHeaders.IfMatch, Constants.PidlTemplatePath.Etag);
                }
            }

            // All requests that hit ProfileDescriptionsController is for standalone profile, which is always V3
            // For flight PXProfileUpdateToHapi, or scenario TwoColumns, update organization profile with partial payload to Hapi endpoint
            // Pass internalProfileOperation to PIDLFactory to get the profile PIDL, tax PIDL still uses original operation
            // Enable the template partner check for the PXProfileUpdateToHapi flighting to use the inline feature UseProfileUpdateToHapi, utilized for the profile.
            string internalProfileOperation = operation;
            if (string.Equals(operation, Constants.Operations.Update, StringComparison.OrdinalIgnoreCase)
                && string.Equals(type, GlobalConstants.ProfileTypes.Organization, StringComparison.OrdinalIgnoreCase)
                && (this.ExposedFlightFeatures.Contains(Flighting.Features.PXProfileUpdateToHapi, StringComparer.OrdinalIgnoreCase)
                || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseProfileUpdateToHapi, country, setting)
                || string.Equals(scenario, Constants.ScenarioNames.TwoColumns, StringComparison.OrdinalIgnoreCase)))
            {
                internalProfileOperation += "_partial";
            }

            // TODO: Once both flights 'PXProfileUpdateToHapi' and 'PXEmployeeProfileUpdateToHapi' are merged, remove the following if condition and merge it to the above condition.
            // Enable the template partner check for the PXEmployeeProfileUpdateToHapi flighting to use the inline feature UseEmployeeProfileUpdateToHapi, utilized for the profile.
            if (string.Equals(operation, Constants.Operations.Update, StringComparison.OrdinalIgnoreCase)
                && string.Equals(type, GlobalConstants.ProfileTypes.Employee, StringComparison.OrdinalIgnoreCase)
                && (this.ExposedFlightFeatures.Contains(Flighting.Features.PXEmployeeProfileUpdateToHapi, StringComparer.OrdinalIgnoreCase) 
                || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseEmployeeProfileUpdateToHapi, country, setting)))
            {
                internalProfileOperation += "_partial";
            }

            // Add known partner flight StandaloneProfile into the partnerFlights
            // If the partner passes StandaloneProfile, 
            // instead of returning multiple profile pidl, we will only return only 1 profile
            if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.StandaloneProfile)
                && string.Equals(type, GlobalConstants.ProfileTypes.Organization, StringComparison.OrdinalIgnoreCase))
            {
                partnerFlights.Add(Constants.PartnerFlightValues.StandaloneProfile);
            }

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
                }
            }

            List<PIDLResource> retVal = PIDLResourceFactory.Instance.GetProfileDescriptions(
                country,
                type,
                internalProfileOperation,
                language,
                partner,
                null,
                profileId,
                profileV3Headers,
                overrideJarvisVersionToV3,
                this.ExposedFlightFeatures,
                scenario,
                partnerFlights,
                setting);

            // Due to PIDL SDK's extra check between api operation and identity operation, restore PIDL's identity operation to basic value
            // Enable the template partner check for the PXProfileUpdateToHapi flighting to use the inline feature UseProfileUpdateToHapi, utilized for the profile.
            if (string.Equals(operation, Constants.Operations.Update, StringComparison.OrdinalIgnoreCase)
                && string.Equals(type, GlobalConstants.ProfileTypes.Organization, StringComparison.OrdinalIgnoreCase)
                && (this.ExposedFlightFeatures.Contains(Flighting.Features.PXProfileUpdateToHapi, StringComparer.OrdinalIgnoreCase) 
                || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseProfileUpdateToHapi, country, setting) 
                || string.Equals(scenario, Constants.ScenarioNames.TwoColumns, StringComparison.OrdinalIgnoreCase)))
            {
                foreach (PIDLResource pidlResource in retVal)
                {
                    pidlResource.Identity["operation"] = operation;
                }
            }

            // TODO: Once both flights 'PXProfileUpdateToHapi' and 'PXEmployeeProfileUpdateToHapi' are merged, remove the following if condition and merge it to the above condition.
            // Enable the template partner check for the PXEmployeeProfileUpdateToHapi flighting to use the inline feature UseEmployeeProfileUpdateToHapi, utilized for the profile.
            if (string.Equals(operation, Constants.Operations.Update, StringComparison.OrdinalIgnoreCase)
                && string.Equals(type, GlobalConstants.ProfileTypes.Employee, StringComparison.OrdinalIgnoreCase)
                && (this.ExposedFlightFeatures.Contains(Flighting.Features.PXEmployeeProfileUpdateToHapi, StringComparer.OrdinalIgnoreCase) 
                || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseEmployeeProfileUpdateToHapi, country, setting)))
            {
                foreach (PIDLResource pidlResource in retVal)
                {
                    pidlResource.Identity["operation"] = operation;
                }
            }

            // If partner passes "x-ms-flight: soldToHideButton", hide save and cancel button
            // Remove this flight, do not pass it to PIMS 
            if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.SoldToHideButton))
            {
                ProxyController.HideDisplayDescriptionById(retVal, Constants.DisplayHintIds.SaveButton);
                ProxyController.HideDisplayDescriptionById(retVal, Constants.DisplayHintIds.CancelButton);

                this.RemovePartnerFlight(Constants.PartnerFlightValues.SoldToHideButton);
            }

            // For emp and org profile, mark address's first_name and last_name to mandatory
            if (string.Equals(type, GlobalConstants.ProfileTypes.Employee, StringComparison.OrdinalIgnoreCase) || string.Equals(type, GlobalConstants.ProfileTypes.Organization, StringComparison.OrdinalIgnoreCase))
            {
                foreach (PIDLResource pidl in retVal)
                {
                    ProxyController.UpdateIsOptionalPropertyWithFullPath(pidl, Constants.PropertiesToMakeMandatory.ToArray(), false);
                }
            }

            // Service side prefill only applies to add (prefill from MSA and context data) and update (prefill from Jarvis's response)
            // Service side prefill does not apply to show and update_patch operations, these two use client side prefilling
            // Service side prefill is also disabled if flight PXProfileUpdateToHapi is enabled or scenario is twoColumns (new feature)
            // Enable the template partner check with the inline feature for the PXProfileUpdateToHapi flight to use the inline feature UseProfileUpdateToHapi.
            // Also, enable the PXEmployeeProfileUpdateToHapi flighting to use the inline feature UseEmployeeProfileUpdateToHapi, utilized for the profile.
            if (!string.Equals(partner, Constants.PartnerName.Wallet, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(type, GlobalConstants.ProfileTypes.Legal, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(type, GlobalConstants.ProfileTypes.Consumer, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(operation, Constants.Operations.Show, StringComparison.OrdinalIgnoreCase)
                && !((this.ExposedFlightFeatures.Contains(Flighting.Features.PXProfileUpdateToHapi, StringComparer.OrdinalIgnoreCase) 
                || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseProfileUpdateToHapi, country, setting)) 
                && string.Equals(type, GlobalConstants.ProfileTypes.Organization, StringComparison.OrdinalIgnoreCase))
                && !((this.ExposedFlightFeatures.Contains(Flighting.Features.PXEmployeeProfileUpdateToHapi, StringComparer.OrdinalIgnoreCase) 
                || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseEmployeeProfileUpdateToHapi, country, setting))
                && string.Equals(type, GlobalConstants.ProfileTypes.Employee, StringComparison.OrdinalIgnoreCase))
                && !string.Equals(scenario, Constants.ScenarioNames.TwoColumns, StringComparison.OrdinalIgnoreCase))
            {
                await this.PrefillUserData(retVal, accountId, country, partner, traceActivityId);
            }

            // Retrieve tax info
            // As confirmed with Tax team, employee profile does not have tax info, exclude it from the following logic.
            // TODO: Confirmed with PB, admin and signup, commercialstores
            // only should use standaloneprofile, standalone TaxID and hapiusage address with linked TaxID
            // profile with linked taxId isn't needed anymore, due to linked TaxId won't show error.  
            // However still see very small amount profile call not sending standaloneprofile (20 per 5 days),
            // Once PB confirm it is due to risk condition
            // we will remove the following logic to add linked tax id in profile to avoid future confusion
            if (!string.Equals(type, GlobalConstants.ProfileTypes.Employee, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(type, GlobalConstants.ProfileTypes.Consumer, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(type, GlobalConstants.ProfileTypes.ConsumerV3, StringComparison.OrdinalIgnoreCase)
                && !this.IsPartnerFlightExposed(Constants.PartnerFlightValues.StandaloneProfile))
            {
                if (Constants.CountriesToCollectTaxIdUnderFlighting.Contains(country))
                {
                    // For Egypt country, it should only enable when the pxenableVATID flight is exposed for partners/templates
                    // Enable the tempalte partner check, to sync with the PXEnableVATID flighting, utilized for the profile.
                    bool pxenableVATID = this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnableVATID, StringComparer.OrdinalIgnoreCase);
                    bool isEgyptCountry = string.Equals(country, Constants.CommercialTaxIdCountryRegionCodes.Egypt, StringComparison.OrdinalIgnoreCase);

                    if ((isEgyptCountry && pxenableVATID)
                        || (!isEgyptCountry && (pxenableVATID || PIDLResourceFactory.IsTemplateInList(partner, setting, Constants.DescriptionTypes.ProfileDescription, GlobalConstants.ProfileTypes.Organization))))
                    {
                        await this.LinkTaxIdPIDL(retVal, accountId, type, partner, country, operation, language, scenario, traceActivityId, setting);
                    }
                }
                else
                {
                    await this.LinkTaxIdPIDL(retVal, accountId, type, partner, country, operation, language, scenario, traceActivityId, setting);
                }

                PIDLResourceFactory.RemoveEmptyPidlContainerHints(retVal);
            }

            // For standalone profile, remove all tax related display hints
            if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.StandaloneProfile))
            {
                string[] taxDisplayDescriptions = { "profileIsTaxEnabled", "profileOrganizationLegalText", "profileOrganizationLoveCodeProperty", "profileOrganizationMobileBarcodeProperty", "profileOrganizationTaxInvoiceButton", "profileOrganizationLegalTextLine1", "profileOrganizationLegalTextLine2", "profileOrganizationLegalTextLine3" };
                ProxyController.RemoveDisplayDescription(retVal, taxDisplayDescriptions);

                PIDLResourceFactory.RemoveEmptyPidlContainerHints(retVal);
                this.RemovePartnerFlight(Constants.PartnerFlightValues.StandaloneProfile);
            }

            // Disable company name edit for org profile update in commercialstores
            // Enable the tempalte partner check, to sync with the commercialstores partner.
            if ((string.Equals(partner, Constants.PartnerName.CommercialStores, StringComparison.OrdinalIgnoreCase)
                || PIDLResourceFactory.IsTemplateInList(partner, setting, Constants.DescriptionTypes.ProfileDescription, GlobalConstants.ProfileTypes.Organization)
                || string.Equals(partner, Constants.PartnerName.SmbOobe, StringComparison.OrdinalIgnoreCase))
                && string.Equals(type, GlobalConstants.ProfileTypes.Organization, StringComparison.OrdinalIgnoreCase)
                && string.Equals(operation, Constants.Operations.Update, StringComparison.OrdinalIgnoreCase))
            {
                foreach (PIDLResource pidl in retVal)
                {
                    // Enable the Template based check, to sync with the commercialstore partner.
                    if (PartnerHelper.IsCommercialStoresPartner(partner) || PartnerHelper.IsSmbOobePartner(partner) || PIDLResourceFactory.IsTemplateInList(partner, setting, Constants.DescriptionTypes.ProfileDescription, GlobalConstants.ProfileTypes.Organization))
                    {
                        DisplayHint companyNameDisplayHint = pidl.GetDisplayHintByPropertyName("company_name");
                        if (companyNameDisplayHint != null)
                        {
                            companyNameDisplayHint.IsDisabled = true;
                        }
                    }
                }
            }

            // Add modern validation for address is partner passes "x-ms-flight: showAVSSuggestions" in header for commercialstores
            // Update operation for both employee profile and organization profile
            // Enabled for add operation for employee profile
            if ((string.Equals(type, GlobalConstants.ProfileTypes.Organization, StringComparison.OrdinalIgnoreCase) || string.Equals(type, GlobalConstants.ProfileTypes.Employee, StringComparison.OrdinalIgnoreCase))
                && (string.Equals(operation, Constants.Operations.Update, StringComparison.OrdinalIgnoreCase)
                    || (string.Equals(type, GlobalConstants.ProfileTypes.Employee, StringComparison.OrdinalIgnoreCase) && string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase)))
                && this.ExposedFlightFeatures.Contains(Constants.PartnerFlightValues.ShowAVSSuggestions, StringComparer.OrdinalIgnoreCase))
            {
                foreach (PIDLResource pidl in retVal)
                {
                    DisplayHint saveButtonDisplayHint = pidl.GetDisplayHintById(Constants.DisplayHintIds.SaveButton);
                    if (saveButtonDisplayHint != null)
                    {
                        this.AddModernValidationAction(saveButtonDisplayHint, "default_address", type, partner, language, country);
                    }

                    // Include "is_customer_consented" and "is_avs_full_validation_succeeded" in payload when posting to Jarvis
                    ProxyController.AddHiddenCheckBoxElement(pidl, GlobalConstants.CommercialZipPlusFourPropertyNames.IsUserConsented, "default_address");
                    ProxyController.AddHiddenCheckBoxElement(pidl, GlobalConstants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded, "default_address");
                }
            }

            FeatureContext featureContext = new FeatureContext(
                country,
                GetSettingTemplate(partner, setting, Constants.DescriptionTypes.ProfileDescription),
                Constants.DescriptionTypes.ProfileDescription,
                operation,
                scenario,
                language,
                null,
                this.ExposedFlightFeatures,
                setting?.Features,
                originalPartner: partner,
                isGuestAccount: GuestAccountHelper.IsGuestAccount(this.Request),
                typeName: type,
                originalTypeName: type);

            PostProcessor.Process(retVal, PIDLResourceFactory.FeatureFactory, featureContext);

            return retVal;
        }

        private static bool IsServicePrefillEtagRequired(string profileType, List<string> exposedFlightFeatures, string scenario, string partner, PaymentExperienceSetting setting = null, string country = null)
        {
            bool isLegalProfile = string.Equals(profileType, GlobalConstants.ProfileTypes.Legal, StringComparison.OrdinalIgnoreCase);
            bool isConsumerProfile = string.Equals(profileType, GlobalConstants.ProfileTypes.Consumer, StringComparison.OrdinalIgnoreCase);
            bool isTwoColumnsScenario = string.Equals(scenario, Constants.ScenarioNames.TwoColumns, StringComparison.OrdinalIgnoreCase);
            bool isProfileUpdateToHapiFlightEnabled = exposedFlightFeatures.Contains(Flighting.Features.PXProfileUpdateToHapi, StringComparer.OrdinalIgnoreCase);
            bool isProfileHapiEnabled = PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseProfileUpdateToHapi, country, setting);
            bool isOrganizationProfile = string.Equals(profileType, GlobalConstants.ProfileTypes.Organization, StringComparison.OrdinalIgnoreCase);
            bool isEmployeeProfile = string.Equals(profileType, GlobalConstants.ProfileTypes.Employee, StringComparison.OrdinalIgnoreCase);
            bool isEmployeeProfileUpdateToHapiFlightEnabled = exposedFlightFeatures.Contains(Flighting.Features.PXEmployeeProfileUpdateToHapi, StringComparer.OrdinalIgnoreCase);
            bool isEmployeeProfileHapiEnabled = PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseEmployeeProfileUpdateToHapi, country, setting);

            return !isLegalProfile
                && !isConsumerProfile
                && !isTwoColumnsScenario
                && !((isProfileUpdateToHapiFlightEnabled || isProfileHapiEnabled) && isOrganizationProfile)
                && !((isEmployeeProfileUpdateToHapiFlightEnabled || isEmployeeProfileHapiEnabled) && isEmployeeProfile);
        }

        private static bool IsClientPrefillEtagRequired(string profileType, List<string> exposedFlightFeatures, string scenario, string partner, PaymentExperienceSetting setting = null, string country = null)
        {
            bool isConsumerProfile = string.Equals(profileType, GlobalConstants.ProfileTypes.Consumer, StringComparison.OrdinalIgnoreCase);
            bool isOrganizationProfile = string.Equals(profileType, GlobalConstants.ProfileTypes.Organization, StringComparison.OrdinalIgnoreCase);
            bool isProfileUpdateToHapiFlightEnabled = exposedFlightFeatures.Contains(Flighting.Features.PXProfileUpdateToHapi, StringComparer.OrdinalIgnoreCase);
            bool isProfileHapiEnabled = PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseProfileUpdateToHapi, country, setting);
            bool isTwoColumnsScenario = string.Equals(scenario, Constants.ScenarioNames.TwoColumns, StringComparison.OrdinalIgnoreCase);
            bool isEmployeeProfile = string.Equals(profileType, GlobalConstants.ProfileTypes.Employee, StringComparison.OrdinalIgnoreCase);
            bool isEmployeeProfileUpdateToHapiFlightEnabled = exposedFlightFeatures.Contains(Flighting.Features.PXEmployeeProfileUpdateToHapi, StringComparer.OrdinalIgnoreCase);
            bool isEmployeeProfileHapiEnabled = PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseEmployeeProfileUpdateToHapi, country, setting);
          
            return isConsumerProfile
                || (isOrganizationProfile && (isProfileUpdateToHapiFlightEnabled || isProfileHapiEnabled || isTwoColumnsScenario))
                || (isEmployeeProfile && (isEmployeeProfileUpdateToHapiFlightEnabled || isEmployeeProfileHapiEnabled));
        }

        private async Task LinkTaxIdPIDL(List<PIDLResource> pidlResource, string accountId, string profileType, string partner, string country, string operation, string language, string scenario, EventTraceActivity traceActivityId, PaymentExperienceSetting setting)
        {
            // For LegalEntity to use client side prefilling tax Pidl, convert the operation from "update" to "update_patch"
            // If flight PXProfileUpdateToHapi is enabled, also use client prefilling for org profile 
            // Enable the template partner check for the PXProfileUpdateToHapi flighting to use the inline feature UseProfileUpdateToHapi, utilized for the profile.
            if ((string.Equals(profileType, GlobalConstants.ProfileTypes.Legal, StringComparison.OrdinalIgnoreCase)
                || (string.Equals(profileType, GlobalConstants.ProfileTypes.Organization, StringComparison.OrdinalIgnoreCase)
                && (this.ExposedFlightFeatures.Contains(Flighting.Features.PXProfileUpdateToHapi, StringComparer.OrdinalIgnoreCase) 
                || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.UseProfileUpdateToHapi, country, setting)
                || string.Equals(scenario, Constants.ScenarioNames.TwoColumns, StringComparison.OrdinalIgnoreCase))))
                && string.Equals(operation, Constants.Operations.Update, StringComparison.OrdinalIgnoreCase))
            {
                operation = Constants.Operations.UpdatePatch;
            }

            string taxIdType = string.Equals(profileType, GlobalConstants.ProfileTypes.Consumer, StringComparison.OrdinalIgnoreCase) ? Constants.TaxIdTypes.Consumer : Constants.TaxIdTypes.Commercial;
            List<PIDLResource> taxIdPidls = PIDLResourceFactory.Instance.GetTaxIdDescriptions(country, taxIdType, language, partner, profileType, operation, false, this.ExposedFlightFeatures, setting: setting);

            if (string.Equals(country, Constants.CommercialTaxIdCountryRegionCodes.Italy, StringComparison.OrdinalIgnoreCase))
            {
                taxIdPidls = PIDLResourceFactory.BuildItalyTaxIDForm(taxIdPidls, this.ExposedFlightFeatures, partner, country, setting, false, true);
            }

            if (string.Equals(country, Constants.CommercialTaxIdCountryRegionCodes.Egypt, StringComparison.OrdinalIgnoreCase))
            {
                taxIdPidls = PIDLResourceFactory.BuildEgyptTaxIDForm(taxIdPidls, this.ExposedFlightFeatures, partner, country, setting, false, true);
            }

            taxIdPidls.ForEach(r => r.MakeSecondaryResource());
            if (pidlResource != null && taxIdPidls.Count > 0 && taxIdPidls[0].DisplayPages != null)
            {
                // There might be more than one taxIdPidls to link to PidlContianer (ex, India has two)
                // Add a counter here, append it to the displayid of PidlContianer to make it unique.
                int taxIdPidlCounter = 0;
                foreach (PIDLResource taxIdPidl in taxIdPidls)
                {
                    if (taxIdPidl.DisplayPages != null)
                    {
                        PIDLResourceFactory.AddLinkedPidlToResourceList(pidlResource, taxIdPidl, partner, PidlContainerDisplayHint.SubmissionOrder.AfterBase, taxIdPidlCounter++);

                        // Tax service side prefilling only applies to update operation.
                        // Operation show and update_patch use client side prefilling.
                        // Operation add does not apply to any prefilling.
                        if (string.Equals(operation, Constants.Operations.Update, StringComparison.OrdinalIgnoreCase))
                        {
                            // Pass type to PrefillUserData, it is required for India scenario to know which value (Pan or GST) to fill.
                            // For other cases, it remains the same since type is set to null by default.
                            string taxIdPidlType = null;
                            taxIdPidl.Identity.TryGetValue(Constants.PidlIdentityFields.Type, out taxIdPidlType);

                            // Prefill taxIdPidl one by one.
                            // For India case, it has two Pidls with same property name, which is the key for prefill. 
                            await this.PrefillUserData(new List<PIDLResource> { taxIdPidl }, accountId, country, partner, traceActivityId, taxIdPidlType);
                        }
                    }
                }
            }

            // Update taxid to mandatory if there is a checkbox in tax pidl and checkbox is selected
            PIDLResourceFactory.AdjustTaxPropertiesInPIDL(taxIdPidls, country, Constants.ProfileType.OrganizationProfile);
        }
    }
}
