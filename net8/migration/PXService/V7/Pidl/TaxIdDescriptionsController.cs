// <copyright file="TaxIdDescriptionsController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;

    public class TaxIdDescriptionsController : ProxyController
    {
        /// <summary>
        /// Get TaxId Descriptions
        /// </summary>
        /// <group>ProfileDescriptions</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/TaxIdDescriptions</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="country" required="true" cref="string" in="path">two letter country id</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <param name="type" required="false" cref="string" in="query">address type</param>
        /// <response code="200">A list of PIDLResource</response>
        /// <returns>A list of PIDLResource object</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Needs to be an instance method for Route action selection")]
        [HttpGet]
        public List<PIDLResource> Get(
            [FromRoute] string accountId,
            [FromRoute] string country,
            [FromQuery] string? language = null,
            [FromQuery] string partner = Constants.ServiceDefaults.DefaultPartnerName,
            [FromQuery] string? type = null)
        {
            // Use Partner Settings if enabled for the partner
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(Constants.Operations.Add);
            this.EnableFlightingsInPartnerSetting(setting, country);

            return PIDLResourceFactory.Instance.GetTaxIdDescriptions(country, type, language, partner, setting: setting);
        }

        /// <summary>
        /// Get TaxId Descriptions ("/GetStandaloneTaxPidl" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>ProfileDescriptions</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/TaxIdDescriptions/GetStandaloneTaxPidl</url>
        /// <param name="accountId" required="true" cref="string" in="path">Account ID</param>
        /// <param name="country" required="true" cref="string" in="path">two letter country id</param>
        /// <param name="operation" required="false" cref="string" in="query">operation name</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <param name="type" required="false" cref="string" in="query">address type</param>
        /// <param name="scenario" required="false" cref="string" in="query">scenario name</param>
        /// <response code="200">A list of PIDLResource</response>
        /// <returns>A list of PIDLResource object</returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public List<PIDLResource> GetStandaloneTaxPidl(
            [FromRoute] string accountId,
            [FromRoute] string country,
            [FromQuery] string operation,
            [FromQuery] string? language = null,
            [FromQuery] string partner = Constants.ServiceDefaults.DefaultPartnerName,
            [FromQuery] string? type = null,
            [FromQuery] string? scenario = null)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            accountId = accountId + string.Empty;

            // Use Partner Settings if enabled for the partner
            PaymentExperienceSetting setting = this.GetPaymentExperienceSetting(operation);
            this.EnableFlightingsInPartnerSetting(setting, country);

            if (string.Equals(partner, Constants.PartnerName.CommercialStores, StringComparison.OrdinalIgnoreCase)
                || PartnerHelper.IsAzurePartner(partner)
                || (TemplateHelper.IsTemplateBasedPIDLIncludingDefaultTemplate(TemplateHelper.GetSettingTemplate(partner, setting, null, null))
                    && string.Equals(type, Constants.TaxIdTypes.Commercial, StringComparison.OrdinalIgnoreCase)))
            {
                if (string.IsNullOrEmpty(scenario) && string.Equals(operation, Constants.Operations.Update, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Operation update without scenario is not supported for TaxidDescriptions");
                }

                string internalScenario = string.Equals(scenario, Constants.ScenarioNames.DepartmentalPurchase, StringComparison.OrdinalIgnoreCase) ? Constants.ScenarioNames.WithCountryDropdown : scenario;

                List<PIDLResource> taxIdPidls = PIDLResourceFactory.Instance.GetTaxIdDescriptions(country, Constants.TaxIdTypes.Commercial, language, partner, Constants.ProfileType.OrganizationProfile, operation, true, scenario: internalScenario, setting: setting);

                // For scenario departmentalPurchase, current logic is:
                // IN, PAN is mandatory, GST is optional
                // Other countries/regions, Tax id is mandatory
                // There is an on-going DCR discussion to remove the following logic
                if (string.Equals(scenario, Constants.ScenarioNames.DepartmentalPurchase, StringComparison.OrdinalIgnoreCase) && !string.Equals(country, Constants.CommercialTaxIdCountryRegionCodes.India, StringComparison.OrdinalIgnoreCase))
                {
                    string[] propertyNames = { "taxId" };
                    ProxyController.UpdateIsOptionalProperty(taxIdPidls, propertyNames, false);
                }

                if (string.Equals(country, Constants.CommercialTaxIdCountryRegionCodes.India, StringComparison.OrdinalIgnoreCase))
                {
                    // If partner passes "x-ms-flight: dpHideCountry", hide state field
                    if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.DpHideCountry))
                    {
                        ProxyController.HideDisplayDescriptionById(taxIdPidls, Constants.DisplayHintIds.AddressState);
                    }

                    // For in, there are two tax pidls, one for GST, one for PAN
                    // In the following code, mark PAN pidl as primary pidl and make GST pidl a linked pidl.
                    PIDLResource currentIndiaStateGstPidl = null;
                    PIDLResource currentIndiaPanPidl = null;
                    foreach (PIDLResource taxIdPidl in taxIdPidls)
                    {
                        string taxIdPidlType = null;
                        taxIdPidl.Identity.TryGetValue(Constants.PidlIdentityFields.Type, out taxIdPidlType);
                        if (string.Equals(taxIdPidlType, Constants.CommercialTaxIdTypes.IndiaGst, StringComparison.InvariantCultureIgnoreCase))
                        {
                            currentIndiaStateGstPidl = taxIdPidl;
                        }
                        else if (string.Equals(taxIdPidlType, Constants.CommercialTaxIdTypes.IndiaPan, StringComparison.InvariantCultureIgnoreCase))
                        {
                            currentIndiaPanPidl = taxIdPidl;
                        }
                    }

                    if (currentIndiaPanPidl != null && currentIndiaStateGstPidl != null)
                    {
                        // For scenario "withCountryDropdown" and operation "update"
                        // Disable state field for GST tax pidl
                        if (string.Equals(internalScenario, Constants.ScenarioNames.WithCountryDropdown, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(operation, Constants.Operations.Update, StringComparison.OrdinalIgnoreCase))
                        {
                            DisplayHint stateDisplayDescription = currentIndiaStateGstPidl.GetDisplayHintById(Constants.DisplayHintIds.AddressState);
                            if (stateDisplayDescription != null)
                            {
                                stateDisplayDescription.IsDisabled = true;
                            }
                        }

                        // if flight or feature is enabled for add operation then only one call is made to save the PanID if the GST field is empty.
                        if ((this.ExposedFlightFeatures.Contains(Flighting.Features.PXEnabledNoSubmitIfGSTIDEmpty)
                                || PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PartnerSettingsHelper.Features.NoSubmitIfGSTIDEmpty, Constants.CommercialTaxIdCountryRegionCodes.India, setting))
                            && string.Equals(operation, Constants.Operations.Add, StringComparison.OrdinalIgnoreCase))
                        {
                            // Update 'clientData' and 'IsOptional' properties of 'data_description' members except 'taxId'
                            foreach (var dataDescription in currentIndiaStateGstPidl.DataDescription)
                            {
                                if (dataDescription.Key != null && !string.Equals(dataDescription.Key, Constants.TaxIdPropertyDescriptionName.TaxId, StringComparison.OrdinalIgnoreCase))
                                {
                                    currentIndiaStateGstPidl.UpdateIsOptionalProperty(dataDescription.Key, true);
                                    currentIndiaStateGstPidl.UpdatePropertyType(dataDescription.Key, "clientData");
                                }
                            }
                        }

                        currentIndiaStateGstPidl.MakeSecondaryResource();
                        taxIdPidls = new List<PIDLResource> { currentIndiaPanPidl };
                        PIDLResourceFactory.AddLinkedPidlToResourceList(taxIdPidls, currentIndiaStateGstPidl, partner);
                    }
                }

                if (string.Equals(country, Constants.CommercialTaxIdCountryRegionCodes.Italy, StringComparison.OrdinalIgnoreCase))
                {
                    if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.DpHideCountry))
                    {
                        ProxyController.HideDisplayDescriptionById(taxIdPidls, Constants.DisplayHintIds.HapiTaxCountryProperty);
                    }

                    bool isDepartmentalOrCountryDropdownScenario = string.Equals(scenario, Constants.ScenarioNames.WithCountryDropdown, StringComparison.OrdinalIgnoreCase) || string.Equals(scenario, Constants.ScenarioNames.DepartmentalPurchase, StringComparison.OrdinalIgnoreCase);
                    taxIdPidls = PIDLResourceFactory.BuildItalyTaxIDForm(taxIdPidls, this.ExposedFlightFeatures, partner, country, setting, true, isDepartmentalOrCountryDropdownScenario);
                }

                if (string.Equals(country, Constants.CommercialTaxIdCountryRegionCodes.Egypt, StringComparison.OrdinalIgnoreCase))
                {
                    if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.DpHideCountry))
                    {
                        ProxyController.HideDisplayDescriptionById(taxIdPidls, Constants.DisplayHintIds.HapiTaxCountryProperty);
                    }

                    bool isDepartmentalOrCountryDropdownScenario = string.Equals(scenario, Constants.ScenarioNames.WithCountryDropdown, StringComparison.OrdinalIgnoreCase) || string.Equals(scenario, Constants.ScenarioNames.DepartmentalPurchase, StringComparison.OrdinalIgnoreCase);
                    taxIdPidls = PIDLResourceFactory.BuildEgyptTaxIDForm(taxIdPidls, this.ExposedFlightFeatures, partner, country, setting, true, isDepartmentalOrCountryDropdownScenario);
                }

                if (string.Equals(internalScenario, Constants.ScenarioNames.WithCountryDropdown, StringComparison.OrdinalIgnoreCase))
                {
                    if (this.IsPartnerFlightExposed(Constants.PartnerFlightValues.DpHideCountry))
                    {
                        // If partner passes "x-ms-flight: dpHideCountry" and "scenario=withCountryDropdown", the real scenario is "departmentalPurchase" 
                        // Hide country field
                        // Remove flight, do not pass it to PIMS 
                        ProxyController.HideDisplayDescriptionById(taxIdPidls, Constants.DisplayHintIds.HapiTaxCountryProperty);
                        this.RemovePartnerFlight(Constants.PartnerFlightValues.DpHideCountry);
                    }
                    else
                    {
                        // If partner passes "scenario=withCountryDropdown" only
                        // Hide all submit/cancel button
                        ProxyController.HideDisplayDescriptionById(taxIdPidls, Constants.DisplayHintIds.SaveButton);
                        ProxyController.HideDisplayDescriptionById(taxIdPidls, Constants.DisplayHintIds.SaveButtonSuccess);
                        ProxyController.HideDisplayDescriptionById(taxIdPidls, Constants.DisplayHintIds.CancelButton);

                        // If partner is commercialstores and it passes "operation=update"
                        // Disable country field
                        if ((string.Equals(partner, Constants.PartnerName.CommercialStores, StringComparison.OrdinalIgnoreCase) || TemplateHelper.IsTemplateBasedPIDLIncludingDefaultTemplate(TemplateHelper.GetSettingTemplate(partner, setting, Constants.DescriptionTypes.TaxIdDescription, type))) && string.Equals(operation, Constants.Operations.Update, StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (PIDLResource taxIdPidl in taxIdPidls)
                            {
                                DisplayHint countryDisplayDescription = taxIdPidl.GetDisplayHintById(Constants.DisplayHintIds.HapiTaxCountryProperty);
                                if (countryDisplayDescription != null)
                                {
                                    countryDisplayDescription.IsDisabled = true;
                                }
                            }
                        }
                    }
                }

                if ((string.Equals(partner, Constants.PartnerName.CommercialStores, StringComparison.OrdinalIgnoreCase) || TemplateHelper.IsTemplateBasedPIDLIncludingDefaultTemplate(TemplateHelper.GetSettingTemplate(partner, setting, Constants.DescriptionTypes.TaxIdDescription, type))) && string.Equals(operation, Constants.Operations.Update, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (PIDLResource taxIdPidl in taxIdPidls)
                    {
                        taxIdPidl.RemoveDataSource();
                    }
                }

                // Update taxid to mandatory if there is a checkbox in tax pidl and checkbox is selected
                PIDLResourceFactory.AdjustTaxPropertiesInPIDL(taxIdPidls, country, Constants.ProfileType.OrganizationProfile);

                FeatureContext featureContext = new FeatureContext(
                    country,
                    GetSettingTemplate(partner, setting, Constants.DescriptionTypes.PaymentMethodDescription),
                    Constants.DescriptionTypes.TaxIdDescription,
                    operation,
                    scenario,
                    language,
                    null,
                    this.ExposedFlightFeatures,
                    setting?.Features);

                PostProcessor.Process(taxIdPidls, PIDLResourceFactory.FeatureFactory, featureContext);

                return taxIdPidls;
            }
            else
            {
                // Fall back to the previous GET function
                var taxIdPidls = PIDLResourceFactory.Instance.GetTaxIdDescriptions(country, type, language, partner, setting: setting);

                FeatureContext featureContext = new FeatureContext(
                    country,
                    GetSettingTemplate(partner, setting, Constants.DescriptionTypes.TaxIdDescription),
                    Constants.DescriptionTypes.TaxIdDescription,
                    operation,
                    scenario,
                    language,
                    null,
                    this.ExposedFlightFeatures,
                    setting?.Features);

                PostProcessor.Process(taxIdPidls, PIDLResourceFactory.FeatureFactory, featureContext);

                return taxIdPidls;
            }
        }

        /// <summary>
        /// Get TaxId Descriptions ("/GetStandaloneTaxPidl" is not in the real path, just as a workaround to show multiple APIs with the same path and http verb but with different params in open API doc)
        /// </summary>
        /// <group>ProfileDescriptions</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/TaxIdDescriptions/GetStandaloneTaxPidl</url>
        /// <param name="country" required="true" cref="string" in="path">two letter country id</param>
        /// <param name="operation" required="false" cref="string" in="query">operation name</param>
        /// <param name="language" required="false" cref="string" in="query">Language code</param>
        /// <param name="partner" required="false" cref="string" in="query">Partner name</param>
        /// <param name="type" required="false" cref="string" in="query">address type</param>
        /// <param name="scenario" required="false" cref="string" in="query">scenario name</param>
        /// <response code="200">A list of PIDLResource</response>
        /// <returns>A list of PIDLResource object</returns>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public List<PIDLResource> GetStandaloneTaxPidl(string country, string operation, string language = null, string partner = Constants.ServiceDefaults.DefaultPartnerName, string type = null, string scenario = null)
        {
            return this.GetStandaloneTaxPidl(
                string.Empty,
                country,
                operation,
                language,
                partner,
                type,
                scenario);
        }
    }
}