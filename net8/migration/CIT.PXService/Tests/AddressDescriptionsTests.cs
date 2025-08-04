  // <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2019. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using global::Tests.Common.Model;
    using global::Tests.Common.Model.Pidl;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using static Tests.AddressTestsUtil;

    [TestClass]
    public class AddressDescriptionsTests : TestBase
    {
        public const string CountriesNewlyEnabledToCollectTaxId = "bb,il,kz,la,np,sg,ug,ci,gh,sn,zm,la";
        public const string CountriesAlreadyExistingToCollectTaxId = "am,by,mx,no,tr,id,th,bh,is,fj";

        /// <summary>
        /// Test is to validate the disableFirstNameLastNameGrouping, enableZipCodeStateGrouping and showMiddleName features using PSS
        /// </summary>
        /// <param name="country"></param>
        /// <param name="regionAndPostalCodeGroupDisplayHintId"></param>
        /// <returns></returns>
        [DataRow("us", "hapiV1ModernAccountV20190531Address_regionAndPostalCodeGroup")]
        [DataRow("in", "hapiV1ModernAccountV20190531Address_regionAndPostalCodeGroup")]
        [DataRow("gb", "hapiV1ModernAccountV20190531Address_regionAndPostalCodeGroup")]
        [DataRow("xk", "hapiV1ModernAccountV20190531Address_postalCodeAndRegionGroup")]
        [DataRow("ng", "hapiV1ModernAccountV20190531Address_postalCodeAndRegionGroup")]
        [DataTestMethod]
        public async Task GetPaymentMethodDescriptions_ValidateDisableFirstNameLastNameGroupingAndEnableZipCodeStateGrouping_UsePartnerSettings(string country, string regionAndPostalCodeGroupDisplayHintId)
        {
            // Arrange
            List<bool?> groupingFeatureStatuses = new List<bool?>() { null, false, true };
            List<bool?> showMiddleNameFeatureStatuses = new List<bool?>() { null, false, true };
            Dictionary<string, string> headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };
            List<string> addressTypes = new List<string>() { Constants.AddressTypes.HapiV1, Constants.AddressTypes.HapiV1SoldToIndividual };

            foreach (string addressType in addressTypes) 
            {
                foreach (bool? groupingFeatureStatus in groupingFeatureStatuses)
                {
                    foreach (bool? showMiddleNameFeatureStatus in showMiddleNameFeatureStatuses)
                    {
                        string url = $"/v7.0/Account001/AddressDescriptions?country={country}&type={addressType}&language=en-US&partner=officesmb&operation=add";
                        string firstNameAndLastNameGroupDisplayHintId = string.Equals(addressType, Constants.AddressTypes.HapiV1, StringComparison.OrdinalIgnoreCase) ? Constants.DisplayHintIds.HapiV1ModernAccountV20190531AddressFirstAndLastNameGroup : Constants.DisplayHintIds.HapiV1ModernAccountV20190531IndividualAddressFirstAndLastNameGroup;

                        if (groupingFeatureStatus != null)
                        {
                            string partnerSettingResponse = "{\"add\":{\"template\":\"onepage\",\"features\":{\"disableFirstNameLastNameGrouping\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":" + groupingFeatureStatus.ToString().ToLower() + "}]},\"enableZipCodeStateGrouping\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":" + groupingFeatureStatus.ToString().ToLower() + "}]},\"enableZipCodeStateGrouping\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":" + groupingFeatureStatus.ToString().ToLower() + "}]},\"enableZipCodeStateGrouping\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":" + groupingFeatureStatus.ToString().ToLower() + "}]},\"showMiddleName\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":" + (showMiddleNameFeatureStatus ?? false).ToString().ToLower() + "}]}}}}";
                            PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
                        }

                        // Act
                        List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

                        // Assert
                        Assert.IsNotNull(pidls, "Pidl is expected to be not null");

                        foreach (var pidl in pidls)
                        {
                            foreach (var displayPage in pidl.DisplayPages)
                            {
                                GroupDisplayHint firstNameLastNameGroup = displayPage.Members.FirstOrDefault(item => item.HintId == firstNameAndLastNameGroupDisplayHintId) as GroupDisplayHint;
                                Assert.IsTrue(groupingFeatureStatus ?? false ? firstNameLastNameGroup == null : firstNameLastNameGroup != null);

                                DisplayHint zipCodeStateGrouping = displayPage.Members.FirstOrDefault(item => item.HintId == regionAndPostalCodeGroupDisplayHintId);
                                Assert.IsTrue(groupingFeatureStatus ?? false ? zipCodeStateGrouping != null : zipCodeStateGrouping == null);

                                if (groupingFeatureStatus ?? false) 
                                {
                                    Assert.IsTrue(zipCodeStateGrouping.DisplayTags.ContainsKey("zipcode-state-group"));
                                }

                                if (string.Equals(addressType, Constants.AddressTypes.HapiV1, StringComparison.OrdinalIgnoreCase))
                                {
                                    List<DisplayHint> membersToIterate = groupingFeatureStatus ?? false ? displayPage.Members : firstNameLastNameGroup.Members;
                                    DisplayHint middleName = membersToIterate.FirstOrDefault(item => item.HintId == Constants.DisplayHintIds.HapiV1ModernAccountV20190531AddressMiddleName);
                                    Assert.IsTrue(((showMiddleNameFeatureStatus ?? false) && groupingFeatureStatus != null) ? middleName != null : middleName == null);
                                }
                            }
                        }

                        PXSettings.PartnerSettingsService.Responses.Clear();
                    }
                }
            }
        }

        /// <summary>
        /// This test is used to verify the hapiServiceUsageAddress pidl with linkedPidl for taxId.
        /// Pidl for it counrty, has a value when either the enableItalyCodiceFiscale flight is enabled or when the partner is a template partner. Therefore, we have excluded the "officesmb" partner.
        /// </summary>
        /// <param name="country"></param>
        /// <param name="hasLinkedTaxPidl"></param>
        /// <param name="taxTypesStr"></param>
        /// <param name="operation"></param>
        /// <param name="flightName"></param>
        /// <returns></returns>
        [DataRow("us", false, null)]
        [DataRow("br", true, "brazil_cnpj_id")]
        [DataRow("gb", true, "vat_id")]
        [DataRow("tw", true, "vat_idAdditionalData")]
        [DataRow("in", true, "india_state_gst_in_id,india_pan_id")]
        [DataRow("gb", true, "vat_id", "update")]
        [DataRow("it", true, "national_identification_number,vat_id", null, "enableItalyCodiceFiscale")]
        [DataRow("it", true, "vat_id")]
        [DataRow("eg", true, "egypt_national_identification_number,vat_id", null, "PXEnableVATID")]
        [DataRow("eg", true, "vat_id", null, "PXEnableVATID")]
        [TestMethod]
        public async Task GetAddressDescription_hapi_serviceusageaddress_WithLinkedPidl(string country, bool hasLinkedTaxPidl, string taxTypesStr, string operation = null, string flightName = null)
        {
            // Arrange
            string operationParameter = (operation != null) ? $"&operation={operation}" : string.Empty;
            string[] partners = new string[] { "commercialstores", "officesmb" }; // List of partners to iterate
            List<PIDLResource> pidls = null;

            foreach (string partner in partners)
            {
                string url = $"/v7.0/Account001/AddressDescriptions?type=hapiServiceUsageAddress&language=en-us&partner={partner}&country={country}{operationParameter}";

                // Act
                if (string.Equals(partner, GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase))
                {
                    operationParameter = operation == null
                    ? (taxTypesStr == "vat_idAdditionalData" || taxTypesStr == "vat_id") ? "&operation=update" : "&operation=add"
                    : operationParameter;
                    url = $"/v7.0/Account001/AddressDescriptions?type=hapiServiceUsageAddress&language=en-us&partner={partner}&country={country}{operationParameter}";
                   
                    var headers = new Dictionary<string, string>()
                    {
                        {
                            "x-ms-flight", "PXDisablePSSCache"
                        }
                    };

                    string partnerSettingResponse = "{\"add\":{\"template\":\"OnePage\",\"features\":null}, \"update\":{\"template\":\"OnePage\",\"features\":null}}";
                    PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
                    pidls = await GetPidlFromPXService(url, flightNames: flightName, additionaHeaders: headers);
                }
                else
                {
                    pidls = await GetPidlFromPXService(url, flightNames: flightName);
                }

                // Assert
                Assert.IsNotNull(pidls);
                foreach (PIDLResource pidl in pidls)
                {
                    if (hasLinkedTaxPidl)
                    {
                        Assert.IsNotNull(pidl.LinkedPidls);
                        IEnumerable<PIDLResource> linkedPidls = pidl.LinkedPidls.Where(x => x.Identity[Constants.DescriptionIdentityFields.DescriptionType] == "hapitaxId");
                        Assert.IsNotNull(linkedPidls);
                        if (string.Equals("it", country, StringComparison.OrdinalIgnoreCase))
                        {
                            int linkedPidlCount = linkedPidls.Count();
                            Assert.AreEqual(
                                string.IsNullOrEmpty(flightName)
                                    ? (string.Equals(partner, GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase) && taxTypesStr == "vat_id" ? 2 : 1)
                                    : 2, 
                                linkedPidlCount);
                        }

                        IEnumerable<string> taxTypes = taxTypesStr.Split(',').Select(x => x.Trim());
                        foreach (string taxType in taxTypes)
                        {
                            PIDLResource linkedTaxPidl = linkedPidls.FirstOrDefault(x => string.Equals(x.Identity[Constants.DescriptionIdentityFields.Type], taxType, StringComparison.OrdinalIgnoreCase));
                            Assert.IsNotNull(linkedTaxPidl);
                        }
                    }
                    else
                    {
                        Assert.IsNull(pidl.LinkedPidls);
                    }

                    if (string.Equals(operation, Constants.OperationTypes.Update, StringComparison.OrdinalIgnoreCase))
                    {
                        List<PIDLResource> dataDescription = pidl.DataDescription["value"] as List<PIDLResource>;
                        Assert.AreEqual(dataDescription[0].Identity["operation"], operation);
                    }
                }
            }
        }

        /// <summary>
        /// The test is to validate the linkedPidl for the enabled and updated countries in order to collect tax ID. The test covers 3 cases
        /// 1. Countries enabled before zinc, without flighting, the linked pidl should be attached.
        /// 2. Countries enabled in zinc, with flighting, the linked pidl should be attached.
        /// 3. Countries enabled in zinc, without flighting, the linked pidl should not be attached.
        /// [Note: LinkedPidl has a value when the PXEnableVATID flight is enabled or when the partner is a template partner, which is why we have excluded the officesmb from [DataRow(CountriesNewlyEnabledToCollectTaxId, "commercialstores,azure", "", false)]].
        /// </summary>
        /// <param name="countryList"></param>
        /// <param name="partnerList"></param>
        /// <param name="flighting"></param>
        /// <param name="haveLinkedPidl"></param>
        /// <returns></returns>
        [DataRow(CountriesAlreadyExistingToCollectTaxId, "commercialstores,azure,officesmb", "", true)]
        [DataRow(CountriesNewlyEnabledToCollectTaxId, "commercialstores,azure,officesmb", "PXEnableVATID", true)]
        [DataRow(CountriesNewlyEnabledToCollectTaxId, "commercialstores,azure", "", false)]
        [DataRow("it", "commercialstores,azure,officesmb", "enableItalyCodiceFiscale", true)]
        [DataRow("it", "commercialstores,azure,officesmb", null, true)]
        [DataRow("eg", "commercialstores,azure,officesmb", "PXEnableVATID", true)]
        [DataRow("eg", "commercialstores,azure", null, false)]
        [TestMethod]
        public async Task GetAddressDescription_validateLinkedPidlToCollectTaxI(string countryList, string partnerList, string flighting, bool haveLinkedPidl)
        {
            // Arrange
            List<PIDLResource> pidls = null;
            string[] partners = partnerList.Split(',');
            string[] countries = countryList.Split(',');
            string[] operations = new string[] { Constants.OperationTypes.Add, Constants.OperationTypes.Update };
            string[] addressTypes = new string[] 
            {
                Constants.AddressTypes.Billing,
                Constants.AddressTypes.OrgAddress,
                Constants.AddressTypes.BillingGroup,
                Constants.AddressTypes.HapiV1ShipToIndividual,
                Constants.AddressTypes.HapiV1SoldToIndividual,
                Constants.AddressTypes.HapiV1BillToIndividual,
                Constants.AddressTypes.HapiServiceUsageAddress,
                Constants.AddressTypes.HapiV1ShipToOrganization,
                Constants.AddressTypes.HapiV1SoldToOrganization,
                Constants.AddressTypes.HapiV1BillToOrganization 
            };
            foreach (string partner in partners)
            {
                foreach (string operation in operations)
                {
                    foreach (string type in addressTypes)
                    {
                        foreach (string country in countries)
                        {
                            if (!string.IsNullOrEmpty(flighting))
                            {
                                PXFlightHandler.AddToEnabledFlights(flighting);
                            }

                            // Act
                            string url = $"/v7.0/Account001/AddressDescriptions?type={type}&language=en-us&partner={partner}&operation={operation}&country={country}";

                            if (string.Equals(partner, GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase))
                            {
                                var headers = new Dictionary<string, string>()
                                {
                                    {
                                        "x-ms-flight", "PXDisablePSSCache"
                                    }
                                };

                                string partnerSettingResponse = "{\"add\":{\"template\":\"OnePage\",\"features\":null}, \"update\":{\"template\":\"OnePage\",\"features\":null}}";
                                PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
                                pidls = await GetPidlFromPXService(url, additionaHeaders: headers);
                            }
                            else
                            {
                                pidls = await GetPidlFromPXService(url); 
                            }

                            // Assert
                            Assert.IsNotNull(pidls);

                            if ((string.Equals(partner, Constants.PartnerNames.CommercialStores, StringComparison.OrdinalIgnoreCase)
                                || string.Equals(partner, GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase))
                                && string.Equals(type, Constants.AddressTypes.HapiServiceUsageAddress, StringComparison.OrdinalIgnoreCase)
                                && haveLinkedPidl)
                            {
                                Assert.IsNotNull(pidls[0].LinkedPidls);
                            }
                            else 
                            {
                                Assert.IsNull(pidls[0].LinkedPidls);
                            }

                            // Reset flighting to default state
                            PXFlightHandler.ResetToDefault();
                        }
                    }
                }
            }
        }

        [DataRow("webpay", null, "first_name")]
        [DataRow("webpay", null, "last_name")]
        [DataRow("webpay", null, "phone_number")]
        [DataRow("cart", null, "first_name")]
        [DataRow("cart", null, "last_name")]
        [DataRow("cart", null, "phone_number")]
        [DataRow("consumersupport", null, "first_name")]
        [DataRow("consumersupport", null, "last_name")]
        [DataRow("consumersupport", null, "phone_number")]
        [DataRow("commercialstores", "commercialhardware", "first_name")]
        [DataRow("commercialstores", "commercialhardware", "last_name")]
        [DataRow("commercialstores", "commercialhardware", "email_address")]
        [DataRow("commercialstores", "commercialhardware", "phone_number")]

        [TestMethod]
        public async Task GetAddressDescription_shipping_CheckMandatoryProperty(string partner, string scenario, string propertyName)
        {
            // Arrange
            string url = $"/v7.0/Account001/AddressDescriptions?type=shipping&language=en-us&partner={partner}&country=us&scenario={scenario}";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url);

            // Assert
            Assert.IsNotNull(pidls);
            foreach (PIDLResource pidl in pidls)
            {
                PropertyDescription property = pidl.DataDescription[propertyName] as PropertyDescription;
                Assert.IsTrue(property.IsOptional.HasValue);
                Assert.IsFalse(property.IsOptional.Value);
            }
        }

        /// <summary>
        /// This test is used to validate the sequence of first name and last name for the cart partner.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task GetAddressDescription_shipping_ValidateFirstNameLastNameSequenceForCartPartner()
        {
            // Arrange
            var partners = new List<string> { "cart", "officesmb" };

            foreach (string partner in partners)
            {
                string url = $"/v7.0/Account001/AddressDescriptions?type=shipping&language=en-us&partner={partner}&country=jp";

                if (partner == "officesmb")
                {
                    string expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\", \"resources\":{\"address\":{\"shipping\":{\"template\":\"defaulttemplate\"}}}}}";
                    PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

                    // opeeration is requried for PSS call.
                    url = url + "&operation=add";
                }

                var firstNameDisplayHintValue = partner == "cart" ? "addressFirstName" : "addressFirstNameOptional";
                var lastNameDisplayHintValue = partner == "cart" ? "addressLastName" : "addressLastNameOptional";

                // Act
                List<PIDLResource> pidls = await GetPidlFromPXService(url);

                // Assert
                Assert.IsNotNull(pidls, "pidls should not be null");

                PIDLResource firstPidl = pidls.FirstOrDefault();
                Assert.IsNotNull(firstPidl, "PIDLResource should not be null");

                // Extract DisplayHints
                DisplayHint firstNameDisplayHint = firstPidl.GetDisplayHintById(firstNameDisplayHintValue);
                DisplayHint lastNameDisplayHint = firstPidl.GetDisplayHintById(lastNameDisplayHintValue);

                // Assert DisplayHints
                Assert.IsNotNull(firstNameDisplayHint, "firstName should not be null");
                Assert.IsNotNull(lastNameDisplayHint, "lastName should not be null");

                // Extract Members from the first DisplayPage
                List<DisplayHint> pidlMembers = firstPidl.DisplayPages.FirstOrDefault()?.Members;

                // Assert Members and their sequence
                Assert.IsNotNull(pidlMembers, "pidlMembers should not be null");
                Assert.AreEqual(lastNameDisplayHint, pidlMembers.FirstOrDefault(), "Last name should be the first member");
                Assert.AreEqual(firstNameDisplayHint, pidlMembers.Skip(1).FirstOrDefault(), "First name should be the second member");
            }
        }

        [DataRow("addressShippingV3", "first_name")]
        [DataRow("addressShippingV3", "last_name")]
        [DataRow("addressShippingV3", "address_line1")]
        [DataRow("addressShippingV3", "city")]
        [DataRow("addressShippingV3", "region")]
        [DataRow(null, "set_as_default_shipping_address")]
        [TestMethod]
        public async Task GetAddressDescription_shipping_CheckMandatory_ChildProperty(string childObjectName, string propertyName)
        {
            string[] xboxNativePartners = { "storify", "xboxsubs", "xboxsettings", "saturn" };
            for (int i = 0; i < xboxNativePartners.Length; i++)
            {
                // Arrange
                string url = $"/v7.0/Account001/AddressDescriptions?type=jarvis_v3&scenario=shipping&language=en-us&partner={xboxNativePartners[i]}&country=us";

                // Act
                List<PIDLResource> pidls = await GetPidlFromPXService(url);

                // Assert
                Assert.IsNotNull(pidls);
                foreach (PIDLResource pidl in pidls)
                {
                    if (childObjectName == null)
                    {
                        PropertyDescription property = pidl.DataDescription[propertyName] as PropertyDescription;
                        Assert.IsTrue(property.IsOptional.HasValue);
                    }
                    else
                    {
                        List<PIDLResource> children = pidl.DataDescription[childObjectName] as List<PIDLResource>;
                        PropertyDescription property = children[0].DataDescription[propertyName] as PropertyDescription;
                        Assert.IsTrue(property.IsOptional.HasValue);
                        Assert.IsFalse(property.IsOptional.Value);
                    }
                }
            }
        }

        [DataRow("addressBillingV3", "address_line1")]
        [DataRow("addressBillingV3", "city")]
        [DataRow("addressBillingV3", "region")]
        [DataRow(null, "set_as_default_billing_address")]
        [TestMethod]
        public async Task GetAddressDescription_billing_CheckMandatory_ChildProperty(string childObjectName, string propertyName)
        {
            string[] xboxNativePartners = { "storify", "xboxsubs", "xboxsettings", "saturn" };
            for (int i = 0; i < xboxNativePartners.Length; i++)
            {
                // Arrange
                string url = $"/v7.0/Account001/AddressDescriptions?type=jarvis_v3&scenario=billing&language=en-us&partner={xboxNativePartners[i]}&country=us";

                // Act
                List<PIDLResource> pidls = await GetPidlFromPXService(url);

                // Assert
                Assert.IsNotNull(pidls);
                foreach (PIDLResource pidl in pidls)
                {
                    if (childObjectName == null)
                    {
                        PropertyDescription property = pidl.DataDescription[propertyName] as PropertyDescription;
                        Assert.IsTrue(property.IsOptional.HasValue);
                    }
                    else
                    {
                        List<PIDLResource> children = pidl.DataDescription[childObjectName] as List<PIDLResource>;
                        PropertyDescription property = children[0].DataDescription[propertyName] as PropertyDescription;
                        Assert.IsTrue(property.IsOptional.HasValue);
                        Assert.IsFalse(property.IsOptional.Value);
                    }
                }
            }
        }

        [DataRow("webpay", "country")]
        [TestMethod]
        public async Task GetAddressDescription_shipping_CheckDisabledProperty(string partner, string propertyName)
        {
            // Arrange
            string url = $"/v7.0/Account001/AddressDescriptions?type=shipping&language=en-us&partner={partner}&country=us";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url);

            // Assert
            Assert.IsNotNull(pidls);
            foreach (PIDLResource pidl in pidls)
            {
                PropertyDisplayHint property = pidl.DisplayHints().First(x => string.Equals(propertyName, x.PropertyName, StringComparison.OrdinalIgnoreCase) && x is PropertyDisplayHint) as PropertyDisplayHint;
                Assert.IsTrue(property.IsDisabled.HasValue);
                Assert.IsFalse(property.IsDisabled.Value);
            }
        }

        [DataRow("onedrive")]
        [TestMethod]
        public async Task GetAddressDescription_billing_CheckSecondarySubmitAddressContext(string partner)
        {
            // Arrange
            string url = $"/v7.0/Account001/AddressDescriptions?type=billing&language=en-us&partner={partner}&country=us";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url);

            // Assert
            Assert.IsNotNull(pidls);
            foreach (PIDLResource pidl in pidls)
            {
                DisplayHint saveButtonDisplayDescription = pidl.GetDisplayHintById("saveButton");
                Assert.IsNotNull(saveButtonDisplayDescription.Action.Context2);
            }
        }

        [DataRow("azure", "individual", false, null)]
        [DataRow("azure", "organization", false, null)]
        [DataRow("azure", "billing", true, null)]
        [DataRow("azure", "shipping", true, null)]
        [DataRow("azure", "billingGroup", true, null)]
        [DataRow("commercialstores", "individual", false, "modernAccount")]
        [DataRow("commercialstores", "organization", false, "modernAccount")]
        [DataRow("commercialstores", "billing", false, "modernAccount")]
        [DataRow("commercialstores", "shipping", true, "modernAccount")]
        [DataRow("commercialstores", "billingGroup", true, "modernAccount")]
        [TestMethod]
        public async Task GetAddressDescription_organization_individual_CheckDisabledProperty(string partner, string type, bool isDisabled, string scenario)
        {
            // Arrange
            string propertyName = "country";
            string url = $"/v7.0/AddressDescriptions?type={type}&language=en-us&partner={partner}&country=us&scenario={scenario}";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url);

            // Assert
            Assert.IsNotNull(pidls);
            foreach (PIDLResource pidl in pidls)
            {
                PropertyDisplayHint property = pidl.DisplayHints().First(x => string.Equals(propertyName, x.PropertyName, StringComparison.OrdinalIgnoreCase) && x is PropertyDisplayHint) as PropertyDisplayHint;
                Assert.IsTrue(property.IsDisabled.HasValue);
                Assert.AreEqual(isDisabled, property.IsDisabled.Value);
            }
        }

        [DataRow("commercialstores", "hapiV1SoldToOrganization", "update")]
        [DataRow("commercialstores", "hapiV1SoldToIndividual", "update")]
        [TestMethod]
        public async Task GetAddressDescription_ModernAccount_CheckDisabledProperty(string partner, string type, string operation)
        {
            // Arrange
            string propertyName = "companyName";
            string url = $"/v7.0/Account001/AddressDescriptions?type={type}&language=en-us&partner={partner}&country=us&operation={operation}";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url);

            // Assert
            Assert.IsNotNull(pidls);
            foreach (PIDLResource pidl in pidls)
            {
                PropertyDisplayHint property = pidl.DisplayHints().First(x => string.Equals(propertyName, x.PropertyName, StringComparison.OrdinalIgnoreCase) && x is PropertyDisplayHint) as PropertyDisplayHint;
                Assert.IsTrue(property.IsDisabled.HasValue);
                Assert.IsTrue(property.IsDisabled.Value);
            }
        }

        [DataRow("commercialstores", "hapiV1SoldToOrganization", "update", false)]
        [DataRow("commercialstores", "hapiV1ShipToOrganization", "add", false)]
        [DataRow("commercialstores", "hapiV1ShipToOrganization", "update", false)]
        [DataRow("commercialstores", "hapiV1BillToOrganization", "add", true)]
        [DataRow("commercialstores", "hapiV1BillToOrganization", "update", false)]
        [DataRow("azure", "hapiV1SoldToOrganization", "update", true)]
        [DataRow("azure", "hapiV1ShipToOrganization", "update", true)]
        [DataRow("azure", "hapiV1BillToOrganization", "update", true)]
        [DataRow("commercialstores", "hapiV1SoldToIndividual", "update", false)]
        [DataRow("commercialstores", "hapiV1BillToIndividual", "add", true)]
        [DataRow("commercialstores", "hapiV1BillToIndividual", "update", false)]
        [DataRow("azure", "hapiV1SoldToIndividual", "update", true)]
        [DataRow("azure", "hapiV1ShipToIndividual", "update", true)]
        [DataRow("azure", "hapiV1BillToIndividual", "update", true)]
        [TestMethod]
        public async Task GetAddressDescription_ModernAccount_CheckSubmitContext(string partner, string type, string operation, bool hasEmptySubmitAction)
        {
            // Arrange
            string url = $"/v7.0/Account001/AddressDescriptions?type={type}&language=en-us&partner={partner}&country=us&operation={operation}";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url);

            // Assert
            Assert.IsNotNull(pidls);
            foreach (PIDLResource pidl in pidls)
            {
                DisplayHint validateThenSubmitButtonHidden = pidl.GetDisplayHintById("validateThenSubmitButtonHidden") as ButtonDisplayHint;
                Assert.IsNotNull(validateThenSubmitButtonHidden);
                Assert.IsNotNull(validateThenSubmitButtonHidden.Action);
                Assert.IsNotNull(validateThenSubmitButtonHidden.Action.Context);
                Assert.IsNotNull(validateThenSubmitButtonHidden.Action.NextAction);
                Assert.IsNotNull(validateThenSubmitButtonHidden.Action.NextAction.Context);

                var contextObj = validateThenSubmitButtonHidden.Action.NextAction.Context;
                var context = JsonConvert.DeserializeObject<RestLink>(JsonConvert.SerializeObject(contextObj));
                Assert.IsNotNull(context);
                if (hasEmptySubmitAction)
                {
                    Assert.AreEqual(context.Href, "https://mockurl", ignoreCase: true);
                }
                else
                {
                    Assert.AreNotEqual(context.Href, "https://mockurl", ignoreCase: true);
                }
            }
        }

        [DataRow("x-ms-flight", "showMiddleName", "", "hapiV1SoldToIndividual", true)]
        [DataRow("x-ms-flight", "showMiddleName", "", "hapiV1BillToIndividual", false)]
        [DataRow("x-ms-flight", "", "", "hapiV1SoldToIndividual", false)]
        [DataRow("x-ms-flight", "", "", "hapiV1BillToIndividual", false)]
        [DataTestMethod]
        public async Task GetAddressDescription_ModernAccount_ShowMiddleName(string headerKey, string allHeaderValue, string leftoverHeader, string type, bool isVisible)
        {
            // Arrange
            string url = $"/v7.0/addressDescriptions?country=us&language=en-US&type={type}&partner=azure&operation=add";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXServiceWithPartnerHeader(url, headerKey, allHeaderValue, leftoverHeader);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");
            foreach (PIDLResource profilePidl in pidls)
            {
                DisplayHint addressMiddleName = profilePidl.GetDisplayHintById("hapiV1ModernAccountV20190531IndividualAddress_middleName");
                Assert.AreEqual(addressMiddleName != null, isVisible);
            }
        }

        [DataRow("amcweb", null)]
        [DataRow("setupoffice", null)]
        [DataRow("setupofficesdx", null)]
        [DataRow("xboxweb", null)]
        [DataRow("windowssetting", null)]
        [TestMethod]
        public async Task GetValidateAddressDescription_MissingAddressId(string partner, string flightOverrides)
        {
            string accountId = "Account001";

            // Arrange
            string url = $"/v7.0/{accountId}/AddressDescriptions?country=us&language=en-US&partner={partner}&operation=validateInstance&type=jarvis_v3";

            // Act
            await GetRequest(
                url,
                null,
                null,
                (responseCode, responseBody, responseHeaders) =>
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, responseCode);
                    dynamic error = JsonConvert.DeserializeObject(responseBody);
                    Assert.AreEqual("InvalidRequestData", error.ErrorCode.ToString());
                    Assert.AreEqual("Missing query parameters addressId, language or/and country", error.Message.ToString());
                });
        }

        [DataRow("officeoobe", "billing", "add", true, "rs5", true, true)]
        [DataRow("oxooobe", "billing", "add", true, "rs5", true, true)]
        [DataRow("cart", "billing", "add", true, null, true, true)]
        [DataRow("cart", "shipping", "add", false, null, true, true)]
        [DataRow("cart", "billing", "add", false, null, true, true)]
        [DataRow("xbox", "shipping", "add", false, null, true, true)]
        [DataRow("xbox", "billing", "add", false, null, true, true)]
        [DataRow("xbox", "billing", "add", true, null, true, true)]
        [DataRow("webblends", "billing", "add", false, null, true, true)]
        [DataRow("webblends", "billing", "add", true, null, true, true)]
        [DataRow("oxowebdirect", "billing", "add", false, null, true, true)]
        [DataRow("oxowebdirect", "billing", "add", true, null, true, true)]
        [DataRow("azure", "billing", "add", false, null, true, false)]
        [DataRow("azure", "shipping", "add", false, null, true, false)]
        [DataRow("saturn", "px_v3_billing", "update", false, "billing", true, false)]
        [DataRow("commercialstores", "hapiV1SoldToOrganizationCSP", "add", false, null, true, false)]
        [DataRow("officesmb", "shipping", "add", false, null, true, true)]
        [TestMethod]
        public async Task GetAddAddressDescription(
            string partner,
            string type,
            string operation,
            bool avsSuggest,
            string scenario,
            bool isValid,
            bool hasSubmitUrl)
        {
            // Arrange
            string accountId = "Account001";

            string url = $"/v7.0/{accountId}/AddressDescriptions?scenario={scenario}&partner={partner}&operation={operation}&language=en-us&country=us&avsSuggest={avsSuggest}&type={type}";

            if (partner == "officesmb")
            {
                string expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\", \"resources\":{\"address\":{\"shipping\":{\"template\":\"defaulttemplate\"}}}}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
            }

            if (isValid)
            {
                // Act
                List<PIDLResource> pidls = await GetPidlFromPXService(url);

                // Assert
                Assert.IsNotNull(pidls);
                if (hasSubmitUrl)
                {
                    foreach (PIDLResource pidl in pidls)
                    {
                        DisplayHint saveButtonDisplayDescription = pidl.GetDisplayHintById("saveButton");
                        var contextObj = saveButtonDisplayDescription.Action.Context;
                        var context = JsonConvert.DeserializeObject<RestLink>(JsonConvert.SerializeObject(contextObj));
                        Assert.IsNotNull(context);
                        if (avsSuggest)
                        {
                            string expectedUrl = $"https://{{pifd-endpoint}}/users/{{userId}}/addressesEx?partner={partner}&language=en-US&avsSuggest=true";
                            if (!string.IsNullOrEmpty(scenario))
                            {
                                expectedUrl += $"&scenario={scenario}";
                            }

                            Assert.AreEqual(expectedUrl, context.Href, ignoreCase: true);
                        }
                        else
                        {
                            Assert.AreEqual("https://{jarvis-endpoint}/JarvisCM/{userId}/addresses", context.Href, ignoreCase: true);
                        }
                    }
                }

                Assert.AreEqual(pidls[0].Identity["type"], type, true);
            }
            else
            {
                await GetRequest(
                     url,
                     null,
                     null,
                     (responseCode, responseBody, responseHeaders) =>
                     {
                         Assert.AreEqual(HttpStatusCode.InternalServerError, responseCode);
                     });
            }
        }

        [DataRow("amcweb,cart,consumersupport,mseg,officeoobe,oxooobe,onedrive,oxowebdirect,setupoffice,smboobe,storeoffice,webblends,webblends_inline", "us", "en-us", "addressA1A2CtStPcCoWithButtonPage", "cancelSaveGroup", "cancelButton", "saveButton", 10)]
        [DataRow("amcweb,cart,consumersupport,mseg,officeoobe,oxooobe,onedrive,oxowebdirect,setupoffice,smboobe,storeoffice,webblends,webblends_inline", "ca", "en-ca", "addressA1A2CtPvPcCoWithButtonPage", "cancelSaveGroup", "cancelButton", "saveButton", 10)]
        [DataRow("amcweb,cart,consumersupport,mseg,officeoobe,oxooobe,onedrive,oxowebdirect,setupoffice,smboobe,storeoffice,webblends,webblends_inline", "in", "en-in", "addressA1A2A3CtStPcCoWithButtonPage", "cancelSaveGroup", "cancelButton", "saveButton", 11)]
        [DataRow("amcweb,cart,consumersupport,mseg,officeoobe,oxooobe,onedrive,oxowebdirect,setupoffice,smboobe,storeoffice,webblends,webblends_inline", "gb", "en-gb", "addressA1A2CtCnPcCoWithButtonPage", "cancelSaveGroup", "cancelButton", "saveButton", 10)]
        [DataRow("amcweb,cart,consumersupport,mseg,officeoobe,oxooobe,onedrive,oxowebdirect,setupoffice,smboobe,storeoffice,webblends,webblends_inline", "qa", "en-qa", "addressA1A2CtCoWithButtonPage", "cancelSaveGroup", "cancelButton", "saveButton", 8)]
        [DataRow("amcweb,cart,consumersupport,mseg,officeoobe,oxooobe,onedrive,oxowebdirect,setupoffice,smboobe,storeoffice,webblends,webblends_inline", "ie", "en-ie", "addressA1A2CtDtPcCoWithButtonPage", "cancelSaveGroup", "cancelButton", "saveButton", 9)]
        [DataRow("amcweb,cart,consumersupport,mseg,officeoobe,oxooobe,onedrive,oxowebdirect,setupoffice,smboobe,storeoffice,webblends,webblends_inline", "af", "en-af", "addressA1A2CtPcCoWithButtonPage", "cancelSaveGroup", "cancelButton", "saveButton", 9)]
        [DataRow("amcweb,cart,consumersupport,mseg,officeoobe,oxooobe,onedrive,oxowebdirect,setupoffice,smboobe,storeoffice,webblends,webblends_inline", "ng", "en-ng", "addressA1A2CtPcStCoWithButtonPage", "cancelSaveGroup", "cancelButton", "saveButton", 10)]
        [DataRow("amcweb,cart,consumersupport,mseg,officeoobe,oxooobe,onedrive,oxowebdirect,setupoffice,smboobe,storeoffice,webblends,webblends_inline", "tr", "en-tr", "addressA1A2CtPvPcCoWithButtonPage", "cancelSaveGroup", "cancelButton", "saveButton", 10)]
        [DataRow("amcweb,cart,consumersupport,mseg,officeoobe,oxooobe,onedrive,oxowebdirect,setupoffice,smboobe,storeoffice,webblends,webblends_inline", "iq", "en-iq", "addressA1A2DtCtStPcCoWithButtonPage", "cancelSaveGroup", "cancelButton", "saveButton", 10)]
        [DataRow("amcweb,cart,consumersupport,mseg,officeoobe,oxooobe,onedrive,oxowebdirect,setupoffice,smboobe,storeoffice,webblends,webblends_inline", "al", "en-al", "addressA1A2PcCtCoWithButtonPage", "cancelSaveGroup", "cancelButton", "saveButton", 9)]
        [DataRow("amcweb,cart,consumersupport,mseg,officeoobe,oxooobe,onedrive,oxowebdirect,setupoffice,smboobe,storeoffice,webblends,webblends_inline", "be", "en-be", "addressA1CtPcCoWithButtonPage", "cancelSaveGroup", "cancelButton", "saveButton", 8)]
        [DataRow("amcweb,cart,consumersupport,mseg,officeoobe,oxooobe,onedrive,oxowebdirect,setupoffice,smboobe,storeoffice,webblends,webblends_inline", "mo", "en-mo", "addressCoCtA1A2WithButtonPage", "cancelSaveGroup", "cancelButton", "saveButton", 8)]
        [DataRow("amcweb,cart,consumersupport,mseg,officeoobe,oxooobe,onedrive,oxowebdirect,setupoffice,smboobe,storeoffice,webblends,webblends_inline", "kr", "en-kr", "addressCoPcStCtA1A2WithButtonPage", "cancelSaveGroup", "cancelButton", "saveButton", 10)]
        [DataRow("amcweb,cart,consumersupport,mseg,officeoobe,oxooobe,onedrive,oxowebdirect,setupoffice,smboobe,storeoffice,webblends,webblends_inline", "cn", "en-cn", "addressCoPvCtDtA1A2PcWithButtonPage", "cancelSaveGroup", "cancelButton", "saveButton", 10)]
        [DataRow("amcweb,cart,consumersupport,mseg,officeoobe,oxooobe,onedrive,oxowebdirect,setupoffice,smboobe,storeoffice,webblends,webblends_inline", "jp", "en-jp", "addressPcStCtA1A2CoWithButtonPage", "cancelSaveGroup", "cancelButton", "saveButton", 10)]
        [DataRow("setupofficesdx,bingtravel", "us", "en-us", "addressA1A2CtStPcCoWithButtonPage", "backSaveAddressGroup", "backButton", "saveAddressButton", 10)]
        [DataRow("setupofficesdx,bingtravel", "ca", "en-ca", "addressA1A2CtPvPcCoWithButtonPage", "backSaveAddressGroup", "backButton", "saveAddressButton", 10)]
        [DataRow("setupofficesdx,bingtravel", "in", "en-in", "addressA1A2A3CtStPcCoWithButtonPage", "backSaveAddressGroup", "backButton", "saveAddressButton", 11)]
        [DataRow("setupofficesdx,bingtravel", "gb", "en-gb", "addressA1A2CtCnPcCoWithButtonPage", "backSaveAddressGroup", "backButton", "saveAddressButton", 10)]
        [DataRow("setupofficesdx,bingtravel", "qa", "en-qa", "addressA1A2CtCoWithButtonPage", "backSaveAddressGroup", "backButton", "saveAddressButton", 8)]
        [DataRow("setupofficesdx,bingtravel", "ie", "en-ie", "addressA1A2CtDtPcCoWithButtonPage", "backSaveAddressGroup", "backButton", "saveAddressButton", 9)]
        [DataRow("setupofficesdx,bingtravel", "af", "en-af", "addressA1A2CtPcCoWithButtonPage", "backSaveAddressGroup", "backButton", "saveAddressButton", 9)]
        [DataRow("setupofficesdx,bingtravel", "ng", "en-ng", "addressA1A2CtPcStCoWithButtonPage", "backSaveAddressGroup", "backButton", "saveAddressButton", 10)]
        [DataRow("setupofficesdx,bingtravel", "tr", "en-tr", "addressA1A2CtPvPcCoWithButtonPage", "backSaveAddressGroup", "backButton", "saveAddressButton", 10)]
        [DataRow("setupofficesdx,bingtravel", "iq", "en-iq", "addressA1A2DtCtStPcCoWithButtonPage", "backSaveAddressGroup", "backButton", "saveAddressButton", 10)]
        [DataRow("setupofficesdx,bingtravel", "al", "en-al", "addressA1A2PcCtCoWithButtonPage", "backSaveAddressGroup", "backButton", "saveAddressButton", 9)]
        [DataRow("setupofficesdx,bingtravel", "be", "en-be", "addressA1CtPcCoWithButtonPage", "backSaveAddressGroup", "backButton", "saveAddressButton", 8)]
        [DataRow("setupofficesdx,bingtravel", "mo", "en-mo", "addressCoCtA1A2WithButtonPage", "backSaveAddressGroup", "backButton", "saveAddressButton", 8)]
        [DataRow("setupofficesdx,bingtravel", "kr", "en-kr", "addressCoPcStCtA1A2WithButtonPage", "backSaveAddressGroup", "backButton", "saveAddressButton", 10)]
        [DataRow("setupofficesdx,bingtravel", "cn", "en-cn", "addressCoPvCtDtA1A2PcWithButtonPage", "backSaveAddressGroup", "backButton", "saveAddressButton", 10)]
        [DataRow("setupofficesdx,bingtravel", "jp", "en-jp", "addressPcStCtA1A2CoWithButtonPage", "backSaveAddressGroup", "backButton", "saveAddressButton", 10)]
        [TestMethod]
        public async Task GetAddressDescription_AddressDetailsPageWithoutSummaryPage(string partners, string country, string language, string pageDisplayHint, string buttonGroup, string cancelButtonId, string saveButtonId, int detailsPageMemberCount)
        {
            string accountId = "Account001";

            foreach (string partner in partners.Split(','))
            {
                // Arrange
                string url = $"/v7.0/{accountId}/AddressDescriptions?partner={partner}&operation=add&language={language}&country={country}&type=billing";

                // Act
                List<PIDLResource> pidls = await GetPidlFromPXService(url);

                // Assert
                Assert.AreEqual(1, pidls.Count, $"For partner: {partner}");

                var pidl = pidls[0];
                Assert.IsNotNull(pidl, $"For partner: {partner}");
                Assert.AreEqual(1, pidl.DisplayPages.Count, $"For partner: {partner}");

                // Single page - address details plus Cancel/Save buttons
                var addressDetailsPage = pidl.DisplayPages[0];
                ValidatePageDisplayHint(addressDetailsPage, "AddressDetailsPage", pageDisplayHint, detailsPageMemberCount, partner);
                var cancelSaveGroup = addressDetailsPage.Members[detailsPageMemberCount - 1] as GroupDisplayHint;
                ValidateButtonGroup(cancelSaveGroup, buttonGroup, new List<string> { cancelButtonId, saveButtonId }, partner);

                PXSettings.AccountsService.ResetToDefaults();
                PXSettings.AddressEnrichmentService.ResetToDefaults();
            }
        }

        [DataRow("default,payin", "us", "en-us", "addressA1A2CtStPcCoWithButtonPage", 10)]
        [DataRow("default,payin", "ca", "en-ca", "addressA1A2CtPvPcCoWithButtonPage", 10)]
        [DataRow("default,payin", "in", "en-in", "addressA1A2A3CtStPcCoWithButtonPage", 11)]
        [DataRow("default,payin", "gb", "en-gb", "addressA1A2CtCnPcCoWithButtonPage", 10)]
        [DataRow("default,payin", "qa", "en-qa", "addressA1A2CtCoWithButtonPage", 8)]
        [DataRow("default,payin", "ie", "en-ie", "addressA1A2CtDtPcCoWithButtonPage", 9)]
        [DataRow("default,payin", "af", "en-af", "addressA1A2CtPcCoWithButtonPage", 9)]
        [DataRow("default,payin", "ng", "en-ng", "addressA1A2CtPcStCoWithButtonPage", 10)]
        [DataRow("default,payin", "tr", "en-tr", "addressA1A2CtPvPcCoWithButtonPage", 10)]
        [DataRow("default,payin", "iq", "en-iq", "addressA1A2DtCtStPcCoWithButtonPage", 10)]
        [DataRow("default,payin", "al", "en-al", "addressA1A2PcCtCoWithButtonPage", 9)]
        [DataRow("default,payin", "be", "en-be", "addressA1CtPcCoWithButtonPage", 8)]
        [DataRow("default,payin", "mo", "en-mo", "addressCoCtA1A2WithButtonPage", 8)]
        [DataRow("default,payin", "kr", "en-kr", "addressCoPcStCtA1A2WithButtonPage", 10)]
        [DataRow("default,payin", "cn", "en-cn", "addressCoPvCtDtA1A2PcWithButtonPage", 10)]
        [DataRow("default,payin", "jp", "en-jp", "addressPcStCtA1A2CoWithButtonPage", 10)]
        [TestMethod]
        public async Task GetAddressDescription_AddressDetailsPageWithSummaryPage(string partners, string country, string language, string pageDisplayHint, int detailsPageMemberCount)
        {
            string accountId = "Account001";

            foreach (string partner in partners.Split(','))
            {
                // Arrange
                string url = $"/v7.0/{accountId}/AddressDescriptions?partner={partner}&operation=add&language={language}&country={country}&type=billing";

                // Act
                List<PIDLResource> pidls = await GetPidlFromPXService(url);

                // Assert
                Assert.AreEqual(1, pidls.Count, $"For partner: {partner}");

                var pidl = pidls[0];
                Assert.IsNotNull(pidl, $"For partner: {partner}");
                Assert.AreEqual(2, pidl.DisplayPages.Count, $"For partner: {partner}");

                // First page - address details  Cancel/Next buttons
                var addressDetailsPage = pidl.DisplayPages[0];
                ValidatePageDisplayHint(addressDetailsPage, "AddressDetailsPage", pageDisplayHint, detailsPageMemberCount, partner);
                var cancelNextGroup = addressDetailsPage.Members[detailsPageMemberCount - 1] as GroupDisplayHint;
                ValidateButtonGroup(cancelNextGroup, "cancelNextGroup", new List<string> { "cancelButton", "nextButton" }, partner);

                // Second page - summary plus Cancel/Save buttons
                var addressSummaryPage = pidl.DisplayPages[1];
                ValidatePageDisplayHint(addressSummaryPage, "AddressSummaryPage", "addressSummaryPage", 6, partner);
                var cancelSaveGroup = addressSummaryPage.Members[5] as GroupDisplayHint;
                ValidateButtonGroup(cancelSaveGroup, "cancelSaveGroup", new List<string> { "cancelButton", "saveButton" }, partner);

                PXSettings.AccountsService.ResetToDefaults();
                PXSettings.AddressEnrichmentService.ResetToDefaults();
            }
        }

        /// <summary>
        /// This test is used to verify the default value of the taxId property for Italy, when the flight PXSetItalyTaxIdValuesByFunction is enabled.
        /// </summary>
        /// <param name="flights">Flights Name</param>
        /// <returns></returns>
        [DataRow("enableItalyCodiceFiscale")]
        [DataRow("enableItalyCodiceFiscale,PXSetItalyTaxIdValuesByFunction")]
        [TestMethod]
        public async Task GetAddressDescription_hapi_serviceusageaddress_WithLinkedPidlDefaultValueForTaxId(string flights)
        {
            // Arrange
            string url = $"/v7.0/Account001/AddressDescriptions?type=hapiServiceUsageAddress&language=en-us&partner=commercialstores&country=it";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, flightNames: flights);

            // Assert
            Assert.IsNotNull(pidls);
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");
            
            PropertyDescription linkedTaxIdDescriptionCodaic = pidls[0].LinkedPidls[0].GetPropertyDescriptionByPropertyName("taxId");
            Assert.IsNotNull(linkedTaxIdDescriptionCodaic, "Taxid property is expected to be not null");
            Assert.IsNotNull(pidls[0].LinkedPidls, "Italy tax linked pidl is expected to be not null");
            
            PropertyDescription linkedtaxIdDescriptionVatId = pidls[0].LinkedPidls[1].GetPropertyDescriptionByPropertyName("taxId");
            Assert.IsNotNull(linkedtaxIdDescriptionVatId, "Taxid property is expected to be not null");

            if (flights.Contains("PXSetItalyTaxIdValuesByFunction"))
            {
                Assert.AreEqual(linkedTaxIdDescriptionCodaic.DefaultValue, "(<|getNationalIdentificationNumber|>)");
                Assert.AreEqual(linkedtaxIdDescriptionVatId.DefaultValue, "(<|getVatId|>)");
            }
            else
            {
                Assert.AreEqual(linkedTaxIdDescriptionCodaic.DefaultValue, "({dataSources.taxResource.value[1].taxId})");
                Assert.AreEqual(linkedtaxIdDescriptionVatId.DefaultValue, "({dataSources.taxResource.value[0].taxId})");
            }
        }

        /// <summary>
        /// This test is used to verify the default value of the taxId property for Egypt
        /// </summary>
        /// <param name="flights">Flights Name</param>
        /// <returns></returns>
        [DataRow("PXEnableVATID")]      
        [TestMethod]
        public async Task GetAddressDescription_hapi_serviceusageaddress_WithLinkedPidlDefaultValueForEgyptTaxId(string flights)
        {
            // Arrange
            string url = $"/v7.0/Account001/AddressDescriptions?type=hapiServiceUsageAddress&language=en-us&partner=commercialstores&country=eg";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, flightNames: flights);

            // Assert
            Assert.IsNotNull(pidls);
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");

            PropertyDescription linkedTaxIdDescriptionNationalid = pidls[0].LinkedPidls[0].GetPropertyDescriptionByPropertyName("taxId");
            Assert.IsNotNull(linkedTaxIdDescriptionNationalid, "Taxid property is expected to be not null");
            Assert.IsNotNull(pidls[0].LinkedPidls, "Egypt tax linked pidl is expected to be not null");

            PropertyDescription linkedtaxIdDescriptionVatId = pidls[0].LinkedPidls[1].GetPropertyDescriptionByPropertyName("taxId");
            Assert.IsNotNull(linkedtaxIdDescriptionVatId, "Taxid property is expected to be not null");
           
            Assert.AreEqual(linkedTaxIdDescriptionNationalid.DefaultValue, "(<|getNationalIdentificationNumber|>)");
            Assert.AreEqual(linkedtaxIdDescriptionVatId.DefaultValue, "(<|getVatId|>)");            
        }

        // Egypt - Collected with flight for template & non-template
        [DataRow("commercialstores", "eg", "PXEnableVATID", true)]
        [DataRow("commercialstores", "eg", null, false)]
        [DataRow("defaulttemplate", "eg", "PXEnableVATID", true)]
        [DataRow("defaulttemplate", "eg", null, false)]

        // CI - Collected with flight for partners
        [DataRow("commercialstores", "ci", "PXEnableVATID", true)]
        [DataRow("commercialstores", "ci", null, false)]
        [DataRow("defaulttemplate", "ci", "PXEnableVATID", true)]
        [DataRow("defaulttemplate", "ci", null, true)]

        // GH - Collected with flight for partners
        [DataRow("commercialstores", "gh", "PXEnableVATID", true)]
        [DataRow("commercialstores", "gh", null, false)]
        [DataRow("defaulttemplate", "gh", "PXEnableVATID", true)]
        [DataRow("defaulttemplate", "gh", null, true)]

        // SN - Collected with flight for partners
        [DataRow("commercialstores", "sn", "PXEnableVATID", true)]
        [DataRow("commercialstores", "sn", null, false)]
        [DataRow("defaulttemplate", "sn", "PXEnableVATID", true)]
        [DataRow("defaulttemplate", "sn", null, true)]

        // ZM - Collected with flight for partners
        [DataRow("commercialstores", "zm", "PXEnableVATID", true)]
        [DataRow("commercialstores", "zm", null, false)]
        [DataRow("defaulttemplate", "zm", "PXEnableVATID", true)]
        [DataRow("defaulttemplate", "zm", null, true)]

        // LA - Collected with flight for partners
        [DataRow("commercialstores", "la", "PXEnableVATID", true)]
        [DataRow("commercialstores", "la", null, false)]
        [DataRow("defaulttemplate", "la", "PXEnableVATID", true)]
        [DataRow("defaulttemplate", "la", null, true)]

        // UG - Collected with flight for partners
        [DataRow("commercialstores", "ug", "PXEnableVATID", true)]
        [DataRow("commercialstores", "ug", null, false)]
        [DataRow("defaulttemplate", "ug", "PXEnableVATID", true)]
        [DataRow("defaulttemplate", "ug", null, true)]

        // BB - Collected with flight for partners
        [DataRow("commercialstores", "bb", "PXEnableVATID", true)]
        [DataRow("commercialstores", "bb", null, false)]
        [DataRow("defaulttemplate", "bb", "PXEnableVATID", true)]
        [DataRow("defaulttemplate", "bb", null, true)]

        // NP - Collected with flight for partners
        [DataRow("commercialstores", "np", "PXEnableVATID", true)]
        [DataRow("commercialstores", "np", null, false)]
        [DataRow("defaulttemplate", "np", "PXEnableVATID", true)]
        [DataRow("defaulttemplate", "np", null, true)]

        // PT - open for all 
        [DataRow("commercialstores", "pt", "PXEnableVATID", true)]
        [DataRow("commercialstores", "pt", null, true)]
        [DataRow("defaulttemplate", "pt", "PXEnableVATID", true)]
        [DataRow("defaulttemplate", "pt", null, true)]
        [TestMethod]
        public async Task GetAddressDescription_validateLinkedPidlTaxId_UnderFlight(string partner, string country, string flights, bool isLinkedTaxIdExpected)
        {
            // Arrage
            string url = $"/v7.0/Account001/AddressDescriptions?type=hapiServiceUsageAddress&language=en-us&partner={partner}&country={country}";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, flightNames: flights);

            // Assert
            Assert.IsNotNull(pidls);
            var taxLegalText = pidls[0].GetDisplayHintById("profileOrganizationLegalText");

            if (isLinkedTaxIdExpected)
            {
                Assert.IsNotNull(pidls[0].LinkedPidls, "Linked TaxIds expected to be not null");
                Assert.IsTrue(pidls[0].LinkedPidls.Count > 0, "One or more Linked TaxId Pidls are expected.");
                
                if (string.Equals(country, "eg"))
                {
                    Assert.IsNotNull(taxLegalText, "TaxId legal text is expected");
                }

                foreach (var taxPidl in pidls[0].LinkedPidls)
                {
                    PropertyDescription linkedTaxIdDescription = taxPidl.GetPropertyDescriptionByPropertyName("taxId");
                    Assert.IsNotNull(linkedTaxIdDescription, "Taxid property is expected to be not null");
                }
            }
            else
            {
                Assert.IsNull(pidls[0].LinkedPidls, "Linked TaxIds expected to be null");

                if (string.Equals(country, "eg"))
                {
                    Assert.IsNull(taxLegalText, "TaxId legal text is not expected");
                }
            }

            PXFlightHandler.ResetToDefault();
        }

        /// <summary>
        /// This test is used to validate the address type shipping with the scenario profileAddress.
        /// In this scenario, we are validating the page, page count, and postal code regex.
        /// </summary>
        /// <param name="partners"> Partners name</param>
        /// <param name="country"> Country name</param>
        /// <param name="language"> languauge</param>
        /// <param name="pageDisplayHint">HintId for the display Page</param>
        /// <param name="detailsPageMemberCount">Number of memebers count under display page</param>
        /// <param name="postCodeValidationRegex">Postal code reeegex value</param>
        /// <returns></returns>
        [DataRow("amcweb,officesmb", "us", "en-us", "profileAddressA1A2CtStPcCoPage", 12, "^\\d{5}(-\\d{4})?$")]
        [DataRow("amcweb,officesmb", "ca", "en-ca", "profileAddressA1A2CtPvPcCoPage", 10, "^[a-zA-Z][0-9][a-zA-Z][ ]?[0-9][a-zA-Z][0-9]$")]
        [DataRow("amcweb,officesmb", "in", "en-in", "profileAddressA1A2A3CtStPcCoPage", 11, "^[0-9]{6,6}$")]
        [DataRow("amcweb,officesmb", "gb", "en-gb", "profileAddressA1A2CtCnPcCoPage", 10, "^([gG][iI][rR][ ]?0[aA]{2,2}|[a-zA-Z]([0-9][0-9a-zA-Z]?|[a-zA-Z][0-9][0-9a-zA-Z]?)[ ]?[0-9][a-zA-Z]{2,2})$")]
        [DataRow("amcweb,officesmb", "qa", "en-qa", "profileAddressA1A2CtCoPage", 8, null)]
        [DataRow("amcweb,officesmb", "ie", "en-ie", "profileAddressA1A2CtDtPcCoPage", 9, "(^[0-9a-zA-Z]{0,2}$|^[dD][uU][bB][lL][iI][nN][ ]{0,1}[0-9a-zA-Z]{1,2}$|^[a-zA-Z]{2,2}[0-9]{1,2}[ ]{0,1}[a-zA-Z]{3,3}$)|((?:^[AC-FHKNPRTV-Y][0-9]{2}|D6W)[ -]?[0-9AC-FHKNPRTV-Y]{4}$)")]
        [DataRow("amcweb,officesmb", "af", "en-af", "profileAddressA1A2CtPcCoPage", 9, "^[0-9]{4,4}$")]
        [DataRow("amcweb,officesmb", "ng", "en-ng", "profileAddressA1A2CtPcStCoPage", 10, "^[0-9]{6,6}$")]
        [DataRow("amcweb,officesmb", "tr", "en-tr", "profileAddressA1A2CtPvPcCoPage", 10, "^[0-9]{5,5}$")]
        [DataRow("amcweb,officesmb", "iq", "en-iq", "profileAddressA1A2DtCtStPcCoPage", 10, "^[0-9]{5,5}$")]
        [DataRow("amcweb,officesmb", "al", "en-al", "profileAddressA1A2PcCtCoPage", 9, "^[0-9]{4,4}$")]
        [DataRow("amcweb,officesmb", "be", "en-be", "profileAddressA1CtPcCoPage", 8, "^[0-9]{4,4}$")]
        [DataRow("amcweb,officesmb", "mo", "en-mo", "profileAddressCoCtA1A2Page", 8, null)]
        [DataRow("amcweb,officesmb", "kr", "en-kr", "profileAddressCoPcStCtA1A2Page", 10, "^[0-9]{5}$")]
        [DataRow("amcweb,officesmb", "cn", "en-cn", "profileAddressCoPvCtDtA1A2PcPage", 10, "^[0-9]{6,6}$")]
        [DataRow("amcweb,officesmb", "jp", "en-jp", "profileAddressPcStCtA1A2CoPage", 10, "^[0-9]{3,3}[-]?[0-9]{4,4}$")]
        [DataRow("amcweb,officesmb", "ru", "en-us", "profileAddressA1A2CtStPcCoPage", 10, "^[0-9]{6,6}$")]
        [TestMethod]
        public async Task GetAddressDescription_ProfileAddress(string partners, string country, string language, string pageDisplayHint, int detailsPageMemberCount, string postCodeValidationRegex)
        {
            // Arrange
            string accountId = "Account001";
           
            foreach (string partner in partners.Split(','))
            {
                string url = $"/v7.0/{accountId}/AddressDescriptions?partner={partner}&operation=add&language={language}&country={country}&type=shipping&scenario=profileAddress";
                PXFlightHandler.AddToEnabledFlights("PXEnableAVSSuggestions");

                if (partner == "officesmb")
                {
                    string expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"resources\":{\"address\":{\"shipping_v3\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null}}},\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"shipping\",\"dataSource\":\"jarvisprofileaddress\"}]}}}}";
                    PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
                }

                // Act
                List<PIDLResource> pidls = await GetPidlFromPXServiceWithPartnerHeader(url, "x-ms-flight", "showAVSSuggestions,PXDisablePSSCache", string.Empty);

                // Assert
                Assert.AreEqual(1, pidls.Count, $"For partner: {partner}");

                var pidl = pidls[0];
                Assert.IsNotNull(pidl, $"For partner: {partner}");

                var addressDetailsPage = pidl.DisplayPages[0];
                ValidateProfileAddressPageDisplayHint(pidl, "AddressDetailsPage", pageDisplayHint, detailsPageMemberCount, partner, country, postCodeValidationRegex);
                var cancelNextGroup = addressDetailsPage.Members[detailsPageMemberCount - 1] as GroupDisplayHint;
                ValidateButtonGroupProfileAddress(cancelNextGroup, "cancelSaveGroup", new List<string> { "cancelButton", "saveButton" }, partner, "addressesEx");

                if (string.Equals(country, "us"))
                {
                    Assert.IsTrue(string.Equals(Constants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded, addressDetailsPage.Members[0].PropertyName));
                    Assert.IsTrue(string.Equals(Constants.CommercialZipPlusFourPropertyNames.IsUserConsented, addressDetailsPage.Members[1].PropertyName));
                }

                PXSettings.AccountsService.ResetToDefaults();
                PXSettings.AddressEnrichmentService.ResetToDefaults();
                PXSettings.PartnerSettingsService.ResetToDefaults();
            }
        }

        private void ValidateButtonGroupProfileAddress(GroupDisplayHint group, string name, List<string> buttonHindIds, string partner, string apiName)
        {
            Assert.IsNotNull(group, $"For partner: {partner}");
            Assert.AreEqual(name, group.HintId, $"For partner: {partner}");
            Assert.AreEqual(buttonHindIds.Count, group.Members.Count, $"For partner: {partner}");
            for (int i = 0; i < buttonHindIds.Count; ++i)
            {
                Assert.AreEqual(buttonHindIds[i], group.Members[i].HintId, $"For partner: {partner}");
                Assert.AreEqual(buttonHindIds[i], group.Members[i].HintId, $"For partner: {partner}");
                if (string.Equals(group.Members[i].Action.ActionType, "submit", StringComparison.OrdinalIgnoreCase))
                {
                    Assert.IsTrue(group.Members[i].Action.Context.ToString().Contains(apiName));
                    Assert.IsFalse(group.Members[i].Action.Context.ToString().Contains("api-version"));
                    Assert.IsFalse(group.Members[i].Action.Context.ToString().Contains("x-ms-correlation-id"));
                    Assert.IsFalse(group.Members[i].Action.Context.ToString().Contains("x-ms-tracking-id"));
                }
            }
        }

        private void AssertDisplayHints(PIDLResource pidl, PIDLResource pidlWithFeatureFlight, List<string> displayHintIds, bool featureEnabled)
        {
            foreach (var displayHintId in displayHintIds)
            {
                var displayHint = pidl.GetDisplayHintById(displayHintId) as PropertyDisplayHint;
                var displayHintForFeatureFlight = pidlWithFeatureFlight.GetDisplayHintById(displayHintId) as PropertyDisplayHint;

                Assert.IsNotNull(displayHint, $"DisplayHint with id {displayHintId} is missing.");
                Assert.IsNotNull(displayHintForFeatureFlight, $"DisplayHint with id {displayHintId} is missing.");

                if (featureEnabled)
                {
                    Assert.AreEqual(displayHint.DisplayName, displayHintForFeatureFlight.DisplayName, $"DisplayHint with HintId {displayHint.DisplayName} not matched with {displayHintForFeatureFlight.DisplayName}.");
                }
                else
                {
                    Assert.AreNotEqual(displayHint.DisplayName, displayHintForFeatureFlight.DisplayName, $"DisplayHint with HintId {displayHint.DisplayName} matched with {displayHintForFeatureFlight.DisplayName}.");
                }
            }
        }

        private void AssertPropertyDescriptions(PIDLResource pidl, List<string> propertyDescriptionIds, bool featureEnabled)
        {
            foreach (var propertyDescriptionId in propertyDescriptionIds)
            {
                var property = pidl.GetPropertyDescriptionByPropertyName(propertyDescriptionId) as PropertyDescription;
                Assert.IsNotNull(property, $"Property with id {propertyDescriptionId} is missing");

                if (featureEnabled)
                {
                    Assert.IsFalse(property.IsOptional.Value);
                }
                else
                {
                    Assert.IsTrue(property.IsOptional.Value);
                }
            }
        }

        private void ValidateProfileAddressPageDisplayHint(PIDLResource pidl, string displayName, string hintId, int memberCount, string partner, string country, string postCodeValidationRegex)
        {
            HashSet<string> addressLine2MCountries = new HashSet<string>() { };
            HashSet<string> addressLine2OCountries = new HashSet<string>() { "us", "in", "gb", "af", "ng", "tr", "al", "kr", "cn", "jp" };
            HashSet<string> addressStateMCountries = new HashSet<string>() { "us", "iq", "in", "ng", "kr", "jp" };
            HashSet<string> addressStateOCountries = new HashSet<string>() { "ru" };
            HashSet<string> addressProvinceMCountries = new HashSet<string>() { "cn" };
            HashSet<string> addressProvinceOCountries = new HashSet<string>() { "tr" };
            HashSet<string> addressPostalCodeMCountries = new HashSet<string>() { "us", "iq", "in", "gb", "af", "ng", "tr", "al", "be", "kr", "cn", "jp" };
            HashSet<string> addressPostalCodeOCountries = new HashSet<string>() { "ie" };

            var addressDetailsPage = pidl.DisplayPages[0];
            Assert.IsNotNull(addressDetailsPage, $"For partner: {partner}");
            Assert.AreEqual(displayName, addressDetailsPage.DisplayName, $"For partner: {partner}");
            Assert.AreEqual(hintId, addressDetailsPage.HintId, $"For partner: {partner}");
            Assert.AreEqual(memberCount, addressDetailsPage.Members.Count, $"For partner: {partner}");

            if (string.Equals(country, "us", StringComparison.OrdinalIgnoreCase))
            {
                Assert.IsNotNull(pidl.DataDescription[Constants.CommercialZipPlusFourPropertyNames.IsUserConsented]);
                Assert.IsNotNull(pidl.DataDescription[Constants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded]);
            }

            StringBuilder sb = new StringBuilder();
            PropertyDescription property = null;
            bool contain_is_customer_consented_checkbox = false;
            bool contain_is_avs_full_validation_succeeded_checkbox = false;
            foreach (DisplayHint displayHint in pidl.DisplayPages[0].Members)
            {
                if (string.Equals(displayHint.PropertyName, Constants.CommercialZipPlusFourPropertyNames.IsUserConsented, StringComparison.OrdinalIgnoreCase))
                {
                    contain_is_customer_consented_checkbox = true;
                }

                if (string.Equals(displayHint.PropertyName, Constants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded, StringComparison.OrdinalIgnoreCase))
                {
                    contain_is_avs_full_validation_succeeded_checkbox = true;
                }

                switch (displayHint.HintId)
                {
                    case "profileAddressLine1":
                        sb.Append("A1");
                        property = pidl.DataDescription["address_line1"] as PropertyDescription;
                        Assert.IsNotNull(property);
                        break;
                    case "profileAddressLine2":
                        sb.Append("A2");
                        property = pidl.DataDescription["address_line2"] as PropertyDescription;
                        Assert.IsNotNull(property);
                        //// check if property's "IsOptional" is set correctly
                        if (addressLine2MCountries.Contains(country))
                        {
                            Assert.IsFalse(property.IsOptional.Value);
                        }

                        if (addressLine2OCountries.Contains(country))
                        {
                            Assert.IsTrue(property.IsOptional.Value);
                        }

                        break;
                    case "profileAddressLine3":
                        sb.Append("A3");
                        property = pidl.DataDescription["address_line3"] as PropertyDescription;
                        Assert.IsNotNull(property);
                        break;
                    case "profileAddressCity":
                        sb.Append("Ct");
                        property = pidl.DataDescription["city"] as PropertyDescription;
                        Assert.IsNotNull(property);
                        break;
                    case "profileAddressState":
                        sb.Append("St");
                        property = pidl.DataDescription["region"] as PropertyDescription;
                        Assert.IsNotNull(property);
                        //// check if property's "IsOptional" is set correctly
                        if (addressStateMCountries.Contains(country))
                        {
                            Assert.IsFalse(property.IsOptional.Value);
                        }

                        if (addressStateOCountries.Contains(country))
                        {
                            Assert.IsTrue(property.IsOptional.Value);
                        }

                        break;
                    case "profileAddressCounty":
                        sb.Append("Cn");
                        property = pidl.DataDescription["region"] as PropertyDescription;
                        Assert.IsNotNull(property);
                        break;
                    case "profileAddressProvince":
                        sb.Append("Pv");
                        property = pidl.DataDescription["region"] as PropertyDescription;
                        Assert.IsNotNull(property);
                        //// check if property's "IsOptional" is set correctly
                        if (addressProvinceMCountries.Contains(country))
                        {
                            Assert.IsFalse(property.IsOptional.Value);
                        }

                        if (addressProvinceOCountries.Contains(country))
                        {
                            Assert.IsTrue(property.IsOptional.Value);
                        }

                        break;
                    case "profileAddressPostalCode":
                        sb.Append("Pc");
                        property = pidl.DataDescription["postal_code"] as PropertyDescription;
                        Assert.IsNotNull(property);
                        //// check if property's "IsOptional" is set correctly
                        if (addressPostalCodeMCountries.Contains(country))
                        {
                            Assert.IsFalse(property.IsOptional.Value);
                        }

                        if (addressPostalCodeOCountries.Contains(country))
                        {
                            Assert.IsTrue(property.IsOptional.Value);
                        }

                        if (postCodeValidationRegex != null)
                        {
                            string.Equals(property.Validation.Regex, postCodeValidationRegex);
                        }

                        break;
                    default:
                        break;
                }
            }

            //// check if the sequence of elements is expected
            string elementSequence = hintId.Replace("Dt", string.Empty);
            Assert.IsTrue(elementSequence.Contains(sb.ToString()), hintId);
            if (string.Equals(country, "us", StringComparison.OrdinalIgnoreCase))
            {
                Assert.IsTrue(contain_is_customer_consented_checkbox);
                Assert.IsTrue(contain_is_avs_full_validation_succeeded_checkbox);
            }
        }

        [DataRow("amcweb", "first_name")]
        [DataRow("amcweb", "last_name")]
        [DataRow("amcweb", "phone_number")]
        [TestMethod]
        public async Task GetAddressDescription_ProfileAddress_CheckMandatory(string partner, string propertyName)
        {
            // Arrange
            string url = $"/v7.0/Account001/AddressDescriptions?type=shipping&scenario=profileAddress&language=en-us&partner={partner}&country=us";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url);

            // Assert
            Assert.IsNotNull(pidls);
            foreach (PIDLResource pidl in pidls)
            {
                PropertyDescription property = pidl.DataDescription[propertyName] as PropertyDescription;
                Assert.IsTrue(property.IsOptional.HasValue);
                Assert.IsFalse(property.IsOptional.Value);
            }
        }

        private void ValidatePageDisplayHint(PageDisplayHint page, string displayName, string hintId, int memberCount, string partner)
        {
            Assert.IsNotNull(page, $"For partner: {partner}");
            Assert.AreEqual(displayName, page.DisplayName, $"For partner: {partner}");
            Assert.AreEqual(hintId, page.HintId, $"For partner: {partner}");
            Assert.AreEqual(memberCount, page.Members.Count, $"For partner: {partner}");
        }

        private void ValidateButtonGroup(GroupDisplayHint group, string name, List<string> buttonHindIds, string partner)
        {
            Assert.IsNotNull(group, $"For partner: {partner}");
            Assert.AreEqual(name, group.HintId, $"For partner: {partner}");
            Assert.AreEqual(buttonHindIds.Count, group.Members.Count, $"For partner: {partner}");
            for (int i = 0; i < buttonHindIds.Count; ++i)
            {
                Assert.AreEqual(buttonHindIds[i], group.Members[i].HintId, $"For partner: {partner}");
            }
        }

        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.ErrorObjectNotFound, "", "")]
        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.ErrorObjectNotFound, "", "paynow")]
        [DataRow("setupoffice", AddressTestsUtil.TestSuggestedAddressType.ErrorObjectNotFound, "", "")]
        [DataRow("setupofficesdx", AddressTestsUtil.TestSuggestedAddressType.ErrorObjectNotFound, "", "")]
        [DataRow("xboxweb", AddressTestsUtil.TestSuggestedAddressType.ErrorObjectNotFound, "", "")]
        [TestMethod]
        public async Task GetValidateAddressDescription_ErrorCase_DirectlyReturnAddressWithIdOnly(
            string partner,
            string addressId,
            string flightOverrides,
            string scenario)
        {
            string accountId = "Account001";
            var expectedPidls = new List<PIDLResource>()
            {
                new PIDLResource()
                {
                    ClientAction = new ClientAction(ClientActionType.ReturnContext, new { id = addressId })
                }
            };
            await TestVerifyDirectReturn(accountId, partner, addressId, flightOverrides, expectedPidls, scenario);
        }

        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.NonUS, null, "paynow")]
        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.NonUS, null, "")]
        [DataRow("setupoffice", AddressTestsUtil.TestSuggestedAddressType.NonUS, null, "")]
        [DataRow("setupofficesdx", AddressTestsUtil.TestSuggestedAddressType.NonUS, null, "")]
        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.UserConsentTrue, null, "")]
        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.UserConsentTrue, null, "paynow")]
        [DataRow("setupoffice", AddressTestsUtil.TestSuggestedAddressType.UserConsentTrue, null, "")]
        [DataRow("setupofficesdx", AddressTestsUtil.TestSuggestedAddressType.UserConsentTrue, null, "")]
        [DataRow("xboxweb", AddressTestsUtil.TestSuggestedAddressType.UserConsentTrue, null, "")]
        [TestMethod]
        public async Task GetValidateAddressDescription_ReturnOriginalAddress(string partner, string addressId, string flightOverrides, string scenario)
        {
            string accountId = "Account001";
            var expectedPidls = new List<PIDLResource>()
            {
                new PIDLResource()
                {
                    ClientAction = new ClientAction(ClientActionType.ReturnContext, AddressTestsUtil.Addresses[addressId])
                }
            };
            await TestVerifyDirectReturn(accountId, partner, addressId, flightOverrides, expectedPidls, scenario);
        }

        private async Task TestVerifyDirectReturn(string accountId, string partner, string addressId, string flightOverrides, List<PIDLResource> expectedPidls, string scenario)
        {
            string url = AppendParameterScenario($"/v7.0/{accountId}/AddressDescriptions?country=us&language=en-US&partner={partner}&operation=validateInstance&type=px_v3&addressId={addressId}", scenario);

            AddressTestsUtil.SetupSuggestAddressPayload(PXSettings.AccountsService, PXSettings.AddressEnrichmentService, accountId, addressId);

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXServiceWithFlightOverrides(url, flightOverrides);

            // Assert
            Assert.AreEqual(1, pidls.Count);
            Assert.AreEqual(JsonConvert.SerializeObject(expectedPidls), JsonConvert.SerializeObject(pidls));
            PXSettings.AccountsService.ResetToDefaults();
            PXSettings.AddressEnrichmentService.ResetToDefaults();
        }

        [DataRow("pt", "orgAddress", "add", "commercialsupport", "region")]
        [DataRow("pt", "orgAddress", "add", "commercialsupport", "address_line3")]
        [DataRow("de", "orgAddress", "add", "commercialsupport", "region")]
        [DataRow("de", "orgAddress", "add", "commercialsupport", "address_line3")]
        [DataRow("us", "orgAddress", "add", "commercialsupport", "region")]
        [DataRow("us", "orgAddress", "add", "commercialsupport", "address_line3")]
        [DataTestMethod]
        public async Task GetAddressDescriptions_ModernValidate_CommercialSupport_orgAddress(string country, string type, string operation, string partner, string pidlFieldName)
        {
            // Arrange
            string url = $"/v7.0/Account001/AddressDescriptions?country={country}&language=en-US&type={type}&partner={partner}&operation={operation}";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url);

            // Assert
            Assert.AreEqual(1, pidls.Count);
            PIDLResource resource = pidls[0];
            if (resource.DataDescription.Count != 0)
            {
                if (country == "pt")
                {
                    Assert.IsFalse(resource.DataDescription.ContainsKey(pidlFieldName));
                }
                else
                {
                    Assert.IsTrue(resource.DataDescription.ContainsKey(pidlFieldName));
                }
            }
            else
            {
                Assert.IsNull(resource.DataDescription);
            }
        }

        [DataRow("x-ms-flight", "showAVSSuggestions,enableAVSAddtionalFlags", "", "billing", "add", true, "commercialstores", "validateButtonHidden")]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "billing", "add", true, "commercialstores", "validateButtonHidden")]
        [DataRow("x-ms-flight", "", "", "billing", "add", false, "commercialstores", "validateButtonHidden")]
        [DataRow("x-ms-flight", "showAVSSuggestions,enableAVSAddtionalFlags", "", "shipping", "add", false, "commercialstores", "validateButtonHidden")]
        [DataRow("x-ms-flight", "showAVSSuggestions,enableAVSAddtionalFlags", "", "hapiServiceUsageAddress", "add", false, "commercialstores", "validateButtonHidden")]
        [DataRow("x-ms-flight", "showAVSSuggestions,enableAVSAddtionalFlags", "", "hapiv1SoldToOrganization", "add", false, "commercialstores", "validateButtonHidden")]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "orgAddress", "add", true, "commercialsupport", "validateButtonHidden")]
        [DataRow("x-ms-flight", "showAVSSuggestions,enableAVSAddtionalFlags", "", "orgAddress", "add", true, "commercialsupport", "validateButtonHidden")]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "orgAddress", "add", true, "commercialsupport", "validateButtonHidden", "suggestAddressesTradeAVS")]
        [DataRow("x-ms-flight", "showAVSSuggestions,enableAVSAddtionalFlags", "", "orgAddress", "add", true, "commercialsupport", "validateButtonHidden", "suggestAddressesTradeAVS")]     
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "orgAddress", "add", true, "commercialsupport", "validateButtonHidden", null, true)]
        [DataRow("x-ms-flight", "showAVSSuggestions,enableAVSAddtionalFlags", "", "orgAddress", "add", true, "commercialsupport", "validateButtonHidden", null, true)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "orgAddress", "add", true, "commercialsupport", "validateButtonHidden", "suggestAddressesTradeAVS", true)]
        [DataRow("x-ms-flight", "showAVSSuggestions,enableAVSAddtionalFlags", "", "orgAddress", "add", true, "commercialsupport", "validateButtonHidden", "suggestAddressesTradeAVS", true)]
        [DataTestMethod]
        public async Task GetAddressDescriptions_ModernValidate_Anonymous(string headerKey, string allHeaderValue, string leftoverHeader, string type, string operation, bool showAVSSuggestions, string partner, string buttonDisplayId, string scenario = null, bool enableValidationFlightStatus = false)
        {
            // Arrange
            PXFlightHandler.AddToEnabledFlights("PXEnableAVSSuggestions");
           
            if (enableValidationFlightStatus)
            {
                PXFlightHandler.AddToEnabledFlights(Constants.PartnerFlightValues.PXEnableSecondaryValidationMode);
            }

            string url = $"/v7.0/AddressDescriptions?country=us&language=en-US&type={type}&partner={partner}&operation={operation}&scenario={scenario}";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXServiceWithPartnerHeader(url, headerKey, allHeaderValue, leftoverHeader);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");
            foreach (PIDLResource profilePidl in pidls)
            {
                DisplayHint validateButtonDisplayHint = profilePidl.GetDisplayHintById(buttonDisplayId);
                if ((string.Equals(partner, "commercialstores", StringComparison.OrdinalIgnoreCase) && string.Equals(type, "billing", StringComparison.OrdinalIgnoreCase))
                    || (string.Equals(partner, Constants.PartnerNames.CommercialSupport, StringComparison.OrdinalIgnoreCase) && string.Equals(type, Constants.AddressTypes.OrgAddress, StringComparison.OrdinalIgnoreCase)))
                {
                    Assert.IsNotNull(validateButtonDisplayHint);
                    Assert.IsNotNull(validateButtonDisplayHint.Action);
                    Assert.IsNotNull(validateButtonDisplayHint.Action.Context);
                    Assert.AreEqual(validateButtonDisplayHint.Action.Context.ToString().Contains("ModernValidate"), showAVSSuggestions);
                    if (showAVSSuggestions)
                    {
                        Assert.IsNotNull(validateButtonDisplayHint.Action.NextAction);
                    }

                    if (string.Equals(allHeaderValue, "showAVSSuggestions,enableAVSAddtionalFlags", StringComparison.OrdinalIgnoreCase))
                    {
                        bool contain_is_customer_consented_checkbox = false;
                        bool contain_is_avs_full_validation_succeeded_checkbox = false;
                        foreach (DisplayHint displayHint in profilePidl.DisplayPages[0].Members)
                        {
                            if (string.Equals(displayHint.PropertyName, Constants.CommercialZipPlusFourPropertyNames.IsUserConsented, StringComparison.OrdinalIgnoreCase))
                            {
                                contain_is_customer_consented_checkbox = true;
                            }
                            else if (string.Equals(displayHint.PropertyName, Constants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded, StringComparison.OrdinalIgnoreCase))
                            {
                                contain_is_avs_full_validation_succeeded_checkbox = true;
                            }
                        }

                        Assert.IsTrue(contain_is_customer_consented_checkbox);
                        Assert.IsTrue(contain_is_avs_full_validation_succeeded_checkbox);
                        Assert.IsNotNull(profilePidl.DataDescription[Constants.CommercialZipPlusFourPropertyNames.IsUserConsented]);
                        Assert.IsNotNull(profilePidl.DataDescription[Constants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded]);
                    }

                    if (showAVSSuggestions && validateButtonDisplayHint.Action.Context.ToString().Contains("ModernValidate"))
                    {
                        var address = new JObject
                        {
                            ["original_address"] = new JObject
                            {
                                ["address_line1"] = "1 Microsoft Way",
                                ["city"] = "Redmond",
                                ["country"] = "US",
                                ["postal_code"] = "98052",
                                ["region"] = "WA"
                            },
                            ["suggested_address"] = new JObject
                            {
                                ["address_line1"] = "1 MICROSOFT WAY",
                                ["city"] = "REDMOND",
                                ["country"] = "US",
                                ["postal_code"] = "98052-8300",
                                ["region"] = "WA"
                            },
                            ["status"] = "Verified"
                        };

                        if (string.Equals(partner, Constants.PartnerNames.CommercialSupport,  StringComparison.OrdinalIgnoreCase)
                            && string.Equals(type, Constants.AddressTypes.OrgAddress, StringComparison.OrdinalIgnoreCase))
                        {
                            address["original_address"]["validation_mode"] = "LegacyBusiness";
                        }

                        PXSettings.AccountsService.ArrangeResponse(address.ToString());
                            
                        var validAddress = new JObject
                        {
                            ["address_line1"] = "1 Microsoft Way",
                            ["country"] = "US",
                            ["city"] = "REDMOND",
                            ["region"] = "WA",
                            ["postal_code"] = "98052"
                        };

                        if (string.Equals(partner, Constants.PartnerNames.CommercialSupport, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(type, Constants.AddressTypes.OrgAddress, StringComparison.OrdinalIgnoreCase))
                        {
                            validAddress["validation_mode"] = "LegacyBusiness";
                        }

                        HttpResponseMessage result = await PXClient.PostAsync("/v7.0/addresses/modernValidate/", new StringContent(JsonConvert.SerializeObject(validAddress), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));
                        Assert.IsNotNull(result);
                        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Modern address validation failed with valid address");

                        string contentStr = await result.Content.ReadAsStringAsync();
                        JObject responseJson = JObject.Parse(contentStr);
                        Assert.IsNotNull(responseJson);

                        JToken originalAddress = responseJson["original_address"];
                        Assert.IsNotNull(originalAddress);

                        // Check if the PXEnableSecondaryValidationMode is enabled.
                        if (enableValidationFlightStatus)
                        {
                            JToken secondaryValidationMode = originalAddress["secondary_validation_mode"];
                            Assert.IsNotNull(secondaryValidationMode);
                            Assert.AreEqual("LegacyBusiness", secondaryValidationMode.ToString());
                        }
                        else
                        {
                            JToken secondaryValidationMode = originalAddress["secondary_validation_mode"];
                            Assert.IsNull(secondaryValidationMode);
                        }
                    }
                }
                else
                {
                    Assert.IsNull(validateButtonDisplayHint);
                    Assert.IsFalse(profilePidl.DataDescription.ContainsKey(Constants.CommercialZipPlusFourPropertyNames.IsUserConsented));
                    Assert.IsFalse(profilePidl.DataDescription.ContainsKey(Constants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded));
                }
            }
        }

        /// <summary>
        /// Test to validate the updateAddressShippingToShippingV3 pss feature with addressValidatin feature.
        /// </summary>
        [DataRow("officesmb", "shipping", true, "saveButton")]
        [DataRow("officesmb", "shipping", false, "saveButton")]
        [DataRow("officesmb", "shipping_v3", true, "saveButton")]
        [DataRow("officesmb", "billing", true, "saveButton")]
        [DataTestMethod]
        public async Task GetAddressDescriptions_Anonymous_Feature_UpdateAddressShippingToShippingV3(string partner, string type, bool enableUpdateShippingToShippingV3, string buttonDisplayHintId)
        {
            string url = $"/v7.0/AddressDescriptions?country=us&language=en-US&type={type}&partner={partner}&operation=add";

            // Arrange
            string features = enableUpdateShippingToShippingV3 ? "\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"shipping\",\"DataSource\":\"jarvisShippingV3\"}]},\"addressValidation\":{\"applicableMarkets\":[]}" : "\"addressValidation\":{\"applicableMarkets\":[]}";
            string expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"features\":{" + features + "}}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");
            foreach (PIDLResource pidl in pidls)
            {
                DisplayHint validateButtonDisplayHint = pidl.GetDisplayHintById(buttonDisplayHintId);
                Assert.IsNotNull(validateButtonDisplayHint);

                if (enableUpdateShippingToShippingV3 && string.Equals(type, "shipping"))
                {
                    Assert.AreEqual("shipping_v3", pidl.Identity["type"]);
                }
                else
                {
                    Assert.AreEqual(type, pidl.Identity["type"]);
                }

                // expected for all address types when address validation feature is enabled expect shipping which is not enabled
                if (enableUpdateShippingToShippingV3 && !string.Equals(type, "shipping"))
                {
                    Assert.IsNotNull(validateButtonDisplayHint);
                    Assert.IsNotNull(validateButtonDisplayHint.Action);
                    Assert.IsNotNull(validateButtonDisplayHint.Action.Context);
                    Assert.IsTrue(validateButtonDisplayHint.Action.Context.ToString().Contains("ModernValidate"));

                    Assert.IsTrue(pidl.DataDescription.ContainsKey(Constants.CommercialZipPlusFourPropertyNames.IsUserConsented));
                    Assert.IsTrue(pidl.DataDescription.ContainsKey(Constants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded));
                }
            }
        }

        /// <summary>
        /// Test to validate the UpdateIndividualAndOrganizationToBillingForAnonymous pss feature for anonymous flow
        /// </summary>
        [DataRow("officesmb", "organization", "billing", "add")]
        [DataRow("officesmb", "individual", "billing", "add")]
        [DataRow("officesmb", "organization", "billing", "update")]
        [DataRow("officesmb", "individual", "billing", "update")]
        [DataRow("officesmb", "hapiv1SoldToIndividual", "hapiv1soldtoindividual", "add")]
        [DataTestMethod]
        public async Task GetAddressDescriptions_Feature_UpdateIndividualAndOrganizationToBillingForAnonymous(string partner, string type, string expectedPidlType, string operation)
        {
            // Arrange
            string url = $"/v7.0/AddressDescriptions?country=us&language=en-US&type={type}&partner={partner}&operation={operation}";

            // organization & individual are not working for template, so we expect to get 200 repsonse when feature is enabled
            string expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"individual\",\"DataSource\":\"jarvisBilling\",\"fieldsToBeEnabled\":[\"country\"]},{\"addressType\":\"organization\",\"DataSource\":\"jarvisBilling\",\"fieldsToBeEnabled\":[\"country\"]},{\"addressType\":\"hapiv1SoldToIndividual\",\"fieldsToBeEnabled\":[\"country\"]}]}}},\"update\":{\"template\":\"defaulttemplate\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"individual\",\"DataSource\":\"jarvisBilling\",\"fieldsToBeEnabled\":[\"country\"]},{\"addressType\":\"organization\",\"DataSource\":\"jarvisBilling\",\"fieldsToBeEnabled\":[\"country\"]}]}}}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");

            foreach (PIDLResource pidl in pidls)
            {
                var country = pidl.GetDisplayHintByPropertyName("country");
                Assert.IsNotNull(country);
                Assert.AreEqual(expectedPidlType, pidl.Identity["type"]);
                Assert.IsFalse(country.IsDisabled);
            }

            PXSettings.PartnerSettingsService.ResetToDefaults();
        }

        [DataRow("hapiv1SoldToIndividual", "add", "commercialsignup", "validateThenSubmitButton", "address", false)]
        [DataRow("jarvis_V3", "add", "windowsstore", "saveButton", "addressBillingV3", true)]
        [DataRow("jarvis_V3", "add", "windowsstore", "saveButton", "addressBillingV3", false)]
        [DataTestMethod]
        public async Task GetAddressDescriptions_ModernValidate_Anonymous_PartnerSettings(string type, string operation, string partner, string buttonDisplayId, string dataDescription, bool setAsDefaultBilling)
        {
            string url = $"/v7.0/AddressDescriptions?country=us&language=en-US&type={type}&partner={partner}&operation={operation}";
            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", "PXDisablePSSCache,PXUsePartnerSettingsService"
                }
            };

            string partnerSettingResponse = null; 
            if (partner == "commercialsignup")
            {
                partnerSettingResponse = "{\"add\":{\"template\":\"onepage\",\"features\":{\"addressValidation\":{\"applicableMarkets\":[]}}}}";
            }
            else if (partner == "windowsstore")
            {
                url = $"/v7.0/Account001/AddressDescriptions?country=us&language=en-US&type={type}&partner={partner}&operation={operation}&setAsDefaultBilling={setAsDefaultBilling}&avsSuggest=true";
                partnerSettingResponse = "{\"add\":{\"template\":\"twopage\",\"features\":{\"addressValidation\":{\"applicableMarkets\":[]},\"useV3AddressPIDL\":{\"applicableMarkets\":[]},\"useAddressesExSubmit\":{\"applicableMarkets\":[]}}}}";
            }

            PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);

            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");

            foreach (PIDLResource addressPidl in pidls)
            {
                DisplayHint submitButton = addressPidl.GetDisplayHintById(buttonDisplayId);
                Assert.IsNotNull(submitButton);
                Assert.IsNotNull(submitButton.Action);
                Assert.IsNotNull(submitButton.Action.Context);
                Assert.IsTrue(submitButton.Action.Context.ToString().Contains("ModernValidate"));
                Assert.IsNotNull(submitButton.Action.NextAction);

                bool contain_is_customer_consented_checkbox = false;
                bool contain_is_avs_full_validation_succeeded_checkbox = false;
                foreach (DisplayHint displayHint in addressPidl.DisplayPages[0].Members)
                {
                    if (string.Equals(displayHint.PropertyName, Constants.CommercialZipPlusFourPropertyNames.IsUserConsented, StringComparison.OrdinalIgnoreCase))
                    {
                        contain_is_customer_consented_checkbox = true;
                    }
                    else if (string.Equals(displayHint.PropertyName, Constants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded, StringComparison.OrdinalIgnoreCase))
                    {
                        contain_is_avs_full_validation_succeeded_checkbox = true;
                    }
                }

                Assert.IsTrue(contain_is_customer_consented_checkbox);
                Assert.IsTrue(contain_is_avs_full_validation_succeeded_checkbox);
                List<PIDLResource> addressPidls = addressPidl.DataDescription[dataDescription] as List<PIDLResource>;
                
                if (partner == "windowsstore")
                {
                    Assert.IsNotNull(addressPidl.DataDescription[Constants.CommercialZipPlusFourPropertyNames.IsUserConsented]);
                    Assert.IsNotNull(addressPidl.DataDescription[Constants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded]);
                    Assert.IsTrue(submitButton.Action.NextAction.Context.ToString().Contains("addressesEx"));
                    if (setAsDefaultBilling)
                    {
                        Assert.IsTrue(addressPidl.DataDescription.ContainsKey("set_as_default_billing_address"));
                    }
                }
                else
                {
                    Assert.IsNotNull(addressPidls[0].DataDescription[Constants.CommercialZipPlusFourPropertyNames.IsUserConsented]);
                    Assert.IsNotNull(addressPidls[0].DataDescription[Constants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded]);
                }
            }
        }

        /// <summary>
        /// This test is used to verify the address validation for the following scenarios.
        /// </summary>
        /// <param name="headerKey"></param>
        /// <param name="allHeaderValue"></param>
        /// <param name="leftoverHeader"></param>
        /// <param name="type"></param>
        /// <param name="operation"></param>
        /// <param name="showAVSSuggestions"></param>
        /// <param name="partner"></param>
        /// <param name="buttonDisplayId"></param>
        /// <param name="usePidlPage"></param>
        /// <returns></returns>
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "hapiServiceUsageAddress", "add", true, "officesmb", "submitButtonHidden", true)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "hapiServiceUsageAddress", "add", true, "officesmb", "submitButtonHidden", false)]
        [DataRow("x-ms-flight", "", "", "hapiServiceUsageAddress", "add", false, "officesmb", "submitButtonHidden", false)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "hapiv1SoldToOrganization", "add", true, "commercialstores", "validateThenSubmitButtonHidden", true)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "hapiv1SoldToOrganization", "add", true, "commercialstores", "validateThenSubmitButtonHidden", false)]
        [DataRow("x-ms-flight", "", "", "hapiv1SoldToOrganization", "add", false, "commercialstores", "validateThenSubmitButtonHidden", false)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "hapiServiceUsageAddress", "add", true, "commercialstores", "submitButtonHidden", true)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "hapiServiceUsageAddress", "add", true, "commercialstores", "submitButtonHidden", false)]
        [DataRow("x-ms-flight", "", "", "hapiServiceUsageAddress", "add", false, "commercialstores", "submitButtonHidden", false)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "hapiv1SoldToIndividual", "add", true, "azure", "validateThenSubmitButtonHidden", true)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "hapiv1SoldToIndividual", "add", true, "azure", "validateThenSubmitButtonHidden", false)]
        [DataRow("x-ms-flight", "", "", "hapiv1SoldToIndividual", "add", false, "azure", "validateThenSubmitButtonHidden", false)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "hapiv1BillToIndividual", "add", true, "azure", "validateThenSubmitButtonHidden", true)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "hapiv1BillToIndividual", "add", true, "azure", "validateThenSubmitButtonHidden", false)]
        [DataRow("x-ms-flight", "", "", "hapiv1BillToIndividual", "add", false, "azure", "validateThenSubmitButtonHidden", false)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "hapiv1SoldToOrganization", "add", true, "azure", "validateThenSubmitButtonHidden", true)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "hapiv1SoldToOrganization", "add", true, "azure", "validateThenSubmitButtonHidden", false)]
        [DataRow("x-ms-flight", "", "", "hapiv1SoldToOrganization", "add", false, "azure", "validateThenSubmitButtonHidden", false)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "hapiv1BilltoOrganization", "add", true, "azure", "validateThenSubmitButtonHidden", true)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "hapiv1BilltoOrganization", "add", true, "azure", "validateThenSubmitButtonHidden", false)]
        [DataRow("x-ms-flight", "", "", "hapiv1BilltoOrganization", "add", false, "azure", "validateThenSubmitButtonHidden", false)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "billing", "add", true, "azure", "validateButtonHidden", true)]
        [DataRow("x-ms-flight", "showAVSSuggestionsModal", "", "billing", "add", true, "azure", "validateButtonHidden", true)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "billing", "add", true, "azure", "validateButtonHidden", false)]
        [DataRow("x-ms-flight", "", "", "billing", "add", false, "azure", "validateButtonHidden", false)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "shipping", "add", true, "amcweb", "saveButton", true)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "shipping", "add", true, "amcweb", "saveButton", false)]
        [DataRow("x-ms-flight", "", "", "shipping", "add", false, "amcweb", "saveButton", false)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "HapiV1ShipToOrganization", "add", true, "commercialstores", "validateThenSubmitButtonHidden", true)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "HapiV1ShipToOrganization", "add", true, "commercialstores", "validateThenSubmitButtonHidden", false)]
        [DataRow("x-ms-flight", "", "", "HapiV1ShipToOrganization", "add", false, "commercialstores", "validateThenSubmitButtonHidden", false)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "shipping", "add", true, "commercialstores", "nextButton", true)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "shipping", "add", true, "commercialstores", "nextButton", false)]
        [DataRow("x-ms-flight", "", "", "shipping", "add", false, "commercialstores", "nextButton", false)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "orgAddress", "add", true, "commercialsupport", "validateButtonHidden", true)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "orgAddress", "add", true, "commercialsupport", "validateButtonHidden", false)]
        [DataRow("x-ms-flight", "showAVSSuggestions,enableAVSAddtionalFlags", "", "orgAddress", "add", true, "commercialsupport", "validateButtonHidden", true)]
        [DataRow("x-ms-flight", "showAVSSuggestions,enableAVSAddtionalFlags", "", "orgAddress", "add", true, "commercialsupport", "validateButtonHidden", false)]
        [DataRow("x-ms-flight", "", "", "orgAddress", "add", false, "commercialsupport", "validateButtonHidden", false)]
        [DataTestMethod]
        public async Task GetAddressDescriptions_ModernValidate(string headerKey, string allHeaderValue, string leftoverHeader, string type, string operation, bool showAVSSuggestions, string partner, string buttonDisplayId, bool usePidlPage)
        {
            // Arrange
            string url = $"/v7.0/Account001/AddressDescriptions?country=us&language=en-US&type={type}&partner={partner}&operation={operation}";

            if (string.Equals(partner, "azure") || string.Equals(partner, "commercialsupport"))
            {
                url = $"/v7.0/AddressDescriptions?country=us&language=en-US&type={type}&partner={partner}&operation={operation}";
            }

            if (string.Equals(partner, "amcweb"))
            {
                url = $"/v7.0/Account001/AddressDescriptions?country=us&language=en-US&type={type}&partner={partner}&operation={operation}&scenario=profileAddress";
            }

            PXFlightHandler.AddToEnabledFlights("PXEnableAVSSuggestions");

            if (!usePidlPage)
            {
                PXFlightHandler.AddToEnabledFlights("TradeAVSUsePidlModalInsteadofPidlPage");
            }

            // Act
            if (string.Equals(partner, GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase))
            {
                allHeaderValue = allHeaderValue?.Length > 0 ? allHeaderValue + ",PXDisablePSSCache" : string.Empty;
                string partnerSettingResponse = "{\"add\":{\"template\":\"OnePage\",\"features\":null}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
            }

            List<PIDLResource> pidls = await GetPidlFromPXServiceWithPartnerHeader(url, headerKey, allHeaderValue, leftoverHeader);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");
            foreach (PIDLResource profilePidl in pidls)
            {
                DisplayHint saveButtonDisplayHint = profilePidl.GetDisplayHintById(buttonDisplayId);
                Assert.IsNotNull(saveButtonDisplayHint);
                Assert.IsNotNull(saveButtonDisplayHint.Action);
                if (showAVSSuggestions)
                {
                    Assert.IsNotNull(saveButtonDisplayHint.Action.NextAction);
                }

                Assert.IsNotNull(saveButtonDisplayHint.Action.Context);
                if (string.Equals("showAVSSuggestionsModal", allHeaderValue) ||
                    (!usePidlPage && showAVSSuggestions))
                {
                    Assert.IsTrue(saveButtonDisplayHint.Action.Context.ToString().Contains("scenario=suggestAddressesTradeAVSUsePidlModal"));
                }

                Assert.AreEqual(saveButtonDisplayHint.Action.Context.ToString().Contains("ModernValidate"), showAVSSuggestions);
                AddressTestsUtil.VerifyModernValidationErrorStrings(JsonConvert.SerializeObject(profilePidl.PidlResourceStrings));

                if ((string.Equals(allHeaderValue, "showAVSSuggestions", StringComparison.OrdinalIgnoreCase)
                   && ((string.Equals(partner, "azure", StringComparison.OrdinalIgnoreCase) && string.Equals(type, "billing", StringComparison.OrdinalIgnoreCase))
                   || (string.Equals(partner, "commercialstores", StringComparison.OrdinalIgnoreCase) && (string.Equals(type, "hapiServiceUsageAddress", StringComparison.OrdinalIgnoreCase) || string.Equals(type, "hapiv1SoldToOrganization", StringComparison.OrdinalIgnoreCase) || string.Equals(type, "HapiV1ShipToOrganization", StringComparison.OrdinalIgnoreCase)))))
                   || (string.Equals(partner, GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase) && string.Equals(type, "hapiServiceUsageAddress", StringComparison.OrdinalIgnoreCase))
                   || (string.Equals(allHeaderValue, "showAVSSuggestions,enableAVSAddtionalFlags", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(partner, "commercialsupport", StringComparison.OrdinalIgnoreCase) && string.Equals(type, "orgAddress", StringComparison.OrdinalIgnoreCase)))
                {
                    bool contain_is_customer_consented_checkbox = false;
                    bool contain_is_avs_full_validation_succeeded_checkbox = false;
                    foreach (DisplayHint displayHint in profilePidl.DisplayPages[0].Members)
                    {
                        if (string.Equals(displayHint.PropertyName, Constants.CommercialZipPlusFourPropertyNames.IsUserConsented, StringComparison.OrdinalIgnoreCase))
                        {
                            contain_is_customer_consented_checkbox = true;
                        }

                        if (string.Equals(displayHint.PropertyName, Constants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded, StringComparison.OrdinalIgnoreCase))
                        {
                            contain_is_avs_full_validation_succeeded_checkbox = true;
                        }
                    }

                    if (string.Equals(allHeaderValue, "showAVSSuggestions", StringComparison.OrdinalIgnoreCase)
                        && (string.Equals(partner, "commercialstores", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(partner, GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (string.Equals(type, "hapiv1SoldToOrganization", StringComparison.OrdinalIgnoreCase))
                        {
                            List<PIDLResource> addressPidls = profilePidl.DataDescription["address"] as List<PIDLResource>;
                            Assert.IsNotNull(addressPidls[0].DataDescription[Constants.CommercialZipPlusFourPropertyNames.IsUserConsented]);
                            Assert.IsNotNull(addressPidls[0].DataDescription[Constants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded]);
                        }
                        else if (string.Equals(type, "hapiServiceUsageAddress", StringComparison.OrdinalIgnoreCase))
                        {
                            List<PIDLResource> valuePidls = profilePidl.DataDescription["value"] as List<PIDLResource>;
                            List<PIDLResource> serviceUsageAddressPidls = valuePidls[0].DataDescription["serviceUsageAddress"] as List<PIDLResource>;
                            Assert.IsNotNull(serviceUsageAddressPidls[0].DataDescription[Constants.CommercialZipPlusFourPropertyNames.IsUserConsented]);
                            Assert.IsNotNull(serviceUsageAddressPidls[0].DataDescription[Constants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded]);
                        }
                        else if (string.Equals(type, "HapiV1ShipToOrganization", StringComparison.OrdinalIgnoreCase))
                        {
                            List<PIDLResource> addressPidls = profilePidl.DataDescription["address"] as List<PIDLResource>;
                            Assert.IsNotNull(addressPidls[0].DataDescription[Constants.CommercialZipPlusFourPropertyNames.IsUserConsented]);
                            Assert.IsNotNull(addressPidls[0].DataDescription[Constants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded]);
                        }
                    }
                    else if (string.Equals(allHeaderValue, "showAVSSuggestions", StringComparison.OrdinalIgnoreCase)
                        && string.Equals(partner, "Azure", StringComparison.OrdinalIgnoreCase)
                        && string.Equals(type, "billing", StringComparison.OrdinalIgnoreCase))
                    {
                        Assert.IsNotNull(profilePidl.DataDescription[Constants.CommercialZipPlusFourPropertyNames.IsUserConsented]);
                        Assert.IsNotNull(profilePidl.DataDescription[Constants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded]);
                    }

                    Assert.IsTrue(contain_is_customer_consented_checkbox);
                    Assert.IsTrue(contain_is_avs_full_validation_succeeded_checkbox);
                }
            }
        }

        [DataRow("hapiv1SoldToIndividual", "add", "officesmb", "us", true, "hapiV1ModernAccountV20190531Address_companyName,hapiV1ModernAccountV20190531Address_phoneNumber,hapiV1ModernAccountV20190531Address_email")]
        [DataRow("hapiv1SoldToIndividual", "add", "officesmb", "us", false, "hapiV1ModernAccountV20190531Address_companyName,hapiV1ModernAccountV20190531Address_phoneNumber,hapiV1ModernAccountV20190531Address_email")]
        [DataRow("hapiv1SoldToIndividual", "update", "officesmb", "us", true, "hapiV1ModernAccountV20190531Address_companyName,hapiV1ModernAccountV20190531Address_phoneNumber,hapiV1ModernAccountV20190531Address_email")]
        [DataRow("hapiv1SoldToIndividual", "update", "officesmb", "us", false, "hapiV1ModernAccountV20190531Address_companyName,hapiV1ModernAccountV20190531Address_phoneNumber,hapiV1ModernAccountV20190531Address_email")]
        [DataRow("hapiv1SoldToIndividual", "add", "teamsappstorefront", "us", true, "hapiV1ModernAccountV20190531Address_companyName,hapiV1ModernAccountV20190531Address_phoneNumber,hapiV1ModernAccountV20190531Address_email,hapiV1AddressHeading")]
        [DataRow("hapiv1SoldToIndividual", "update", "teamsappstorefront", "us", true, "hapiV1ModernAccountV20190531Address_companyName,hapiV1ModernAccountV20190531Address_phoneNumber,hapiV1ModernAccountV20190531Address_email,hapiV1AddressHeading")]
        [DataRow("hapiv1SoldToIndividual", "add", "commercialsignup", "us", true, "hapiV1ModernAccountV20190531Address_companyName,hapiV1ModernAccountV20190531Address_phoneNumber,hapiV1ModernAccountV20190531Address_email,hapiV1AddressHeading,hapiV1ModernAccountV20190531Address_firstName,hapiV1ModernAccountV20190531Address_lastName,starRequiredTextGroup,microsoftPrivacyTextGroup")]
        [DataRow("hapiv1SoldToIndividual", "update", "commercialsignup", "us", true, "hapiV1ModernAccountV20190531Address_companyName,hapiV1ModernAccountV20190531Address_phoneNumber,hapiV1ModernAccountV20190531Address_email,hapiV1AddressHeading,hapiV1ModernAccountV20190531Address_firstName,hapiV1ModernAccountV20190531Address_lastName,starRequiredTextGroup,microsoftPrivacyTextGroup")]
        [DataTestMethod]
        public async Task GetAddressDescriptions_customizeAddressForm_FeatureContextProcess_UsingPartnerSettingsService(string type, string operation, string partner, string country, bool addressCustomizationEnabledToHideFields, string displayHintIdsToBehidden)
        {
            string url = $"/v7.0/Account001/AddressDescriptions?country={country}&language=en-US&type={type}&partner={partner}&operation={operation}";

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", "PXDisablePSSCache"
                }
            };

            // This response doesn't contain hideAddCreditDebitCardHeading property in json.
            string partnerSettingResponse;
            if (partner.Equals("teamsappstorefront"))
            {
                partnerSettingResponse = "{\"add\":{\"template\":\"onepage\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\",\"fieldsToBeHidden\":[\"companyName\",\"email\",\"phoneNumber\",\"pageTitle\"],\"fieldsToMakeRequired\":[\"firstName\",\"lastName\"],\"disableCountryDropdown\":true,\"ungroupAddressFirstNameLastName\":true,\"useAddressDataSourceForUpdate\":false}]},\"addressValidation\":{\"applicableMarkets\":[]}}},\"update\":{\"template\":\"onepage\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\",\"fieldsToBeHidden\":[\"companyName\",\"email\",\"phoneNumber\",\"pageTitle\"],\"fieldsToMakeRequired\":[\"firstName\",\"lastName\"],\"disableCountryDropdown\":true,\"ungroupAddressFirstNameLastName\":true,\"useAddressDataSourceForUpdate\":false}]},\"addressValidation\":{\"applicableMarkets\":[]}}}}";
            }
            else if (partner.Equals("commercialsignup"))
            {
                partnerSettingResponse = "{\"add\":{\"template\":\"onepage\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\",\"fieldsToBeHidden\":[\"companyName\",\"email\",\"phoneNumber\",\"pageTitle\",\"firstName\",\"lastName\",\"starRequiredTextGroup\",\"microsoftPrivacyTextGroup\"],\"useAddressDataSourceForUpdate\":false}]},\"addressValidation\":{\"applicableMarkets\":[]}}},\"update\":{\"template\":\"onepage\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\",\"fieldsToBeHidden\":[\"companyName\",\"email\",\"phoneNumber\",\"pageTitle\",\"firstName\",\"lastName\",\"starRequiredTextGroup\",\"microsoftPrivacyTextGroup\"],\"useAddressDataSourceForUpdate\":false}]},\"addressValidation\":{\"applicableMarkets\":[]}}}}";
            }
            else if (addressCustomizationEnabledToHideFields)
            {
                partnerSettingResponse = "{\"add\":{\"template\":\"onepage\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\",\"fieldsToBeHidden\":[\"companyName\",\"email\",\"phoneNumber\"],\"useAddressDataSourceForUpdate\":true}]},\"addressValidation\":{\"applicableMarkets\":[]}}},\"update\":{\"template\":\"onepage\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\",\"fieldsToBeHidden\":[\"companyName\",\"email\",\"phoneNumber\"],\"useAddressDataSourceForUpdate\":true}]},\"addressValidation\":{\"applicableMarkets\":[]}}}}";
            }
            else
            {
                partnerSettingResponse = "{\"add\":{\"template\":\"onepage\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\"}]},\"addressValidation\":{\"applicableMarkets\":[]}}},\"update\":{\"template\":\"onepage\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\", \"useAddressDataSourceForUpdate\":true}]},\"addressValidation\":{\"applicableMarkets\":[]}}}}";
            }

            PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");

            // addressResource should be enabled in pidl.
            foreach (PIDLResource addressPidl in pidls)
            {
                if (string.Equals(operation, "update", StringComparison.OrdinalIgnoreCase) && partner.Equals("officesmb"))
                {
                    string.Equals(addressPidl.DataSources["addressResource"].Href, "https://{hapi-endpoint}/complexOrganization/soldTo?accountId={partnerData.prefillData.accountId}&organizationId={partnerData.prefillData.orgId})", StringComparison.OrdinalIgnoreCase);
                }
            }

            // Display hints
            List<string> displayHintIds = new List<string>
            {
                "hapiV1AddressHeading",
                "hapiV1ModernAccountV20190531Address_firstAndLastNameGroup",
                "hapiV1ModernAccountV20190531Address_firstName",
                "hapiV1ModernAccountV20190531Address_lastName",
                "hapiV1ModernAccountV20190531Address_email",
                "hapiV1ModernAccountV20190531Address_phoneNumber",
                "hapiV1ModernAccountV20190531Address_companyName",
            };

            if (partner.Equals("teamsappstorefront")
                || partner.Equals("commercialsignup"))
            {
                displayHintIds = new List<string>
                {
                    "hapiV1ModernAccountV20190531Address_firstName",
                    "hapiV1ModernAccountV20190531Address_lastName",
                    "hapiV1ModernAccountV20190531Address_addressLine1",
                    "hapiV1ModernAccountV20190531Address_addressLine2",
                    "hapiV1ModernAccountV20190531Address_city",
                    "hapiV1ModernAccountV20190531Address_country",
                    "hapiV1ModernAccountV20190531Address_postalCodeGroup",
                    "hapiV1ModernAccountV20190531Address_regionGroup"
                };
            }

            foreach (string hintId in displayHintIds)
            {
                Assert.IsNotNull(pidls[0].GetDisplayHintById(hintId), $"DisplayHint with id {hintId} is missing.");
            }

            if (addressCustomizationEnabledToHideFields)
            {
                foreach (string hintId in displayHintIdsToBehidden.Split(',').ToList())
                {
                    Assert.IsTrue(pidls[0].GetDisplayHintById(hintId).IsHidden, $"DisplayHint with id {hintId} is not hiddend.");
                }
            }
        }

        /// <summary>
        /// This CIT is used to verify the `fieldsToMakeRequired` feature in other languages,
        /// ensuring that their display names do not contain the optional text once the feature is enabled.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="operation"></param>
        /// <param name="partner"></param>
        /// <param name="country"></param>
        /// <param name="enableFeatureToFieldsToMakeRequired"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        [DataRow("shipping", "add", "officesmb", "us", true, "hi")]
        [DataRow("shipping", "update", "officesmb", "us", true, "hi")]
        [DataRow("shipping", "add", "officesmb", "us", false, "hi")]
        [DataRow("shipping", "update", "officesmb", "us", false, "hi")]
        [DataRow("shipping", "add", "officesmb", "us", true)]
        [DataRow("shipping", "update", "officesmb", "us", true)]
        [DataRow("shipping", "add", "officesmb", "us", false)]
        [DataRow("shipping", "update", "officesmb", "us", false)]
        [DataRow("shipping_v3", "add", "officesmb", "us", true, "hi")]
        [DataRow("shipping_v3", "update", "officesmb", "us", true, "hi")]
        [DataRow("shipping_v3", "add", "officesmb", "us", false, "hi")]
        [DataRow("shipping_v3", "update", "officesmb", "us", false, "hi")]
        [DataRow("shipping_v3", "add", "officesmb", "us", true)]
        [DataRow("shipping_v3", "update", "officesmb", "us", true)]
        [DataRow("shipping_v3", "add", "officesmb", "us", false)]
        [DataRow("shipping_v3", "update", "officesmb", "us", false)]
        [DataRow("orgaddress", "add", "officesmb", "us", true, "hi")]
        [DataRow("orgaddress", "update", "officesmb", "us", true, "hi")]
        [DataRow("orgaddress", "add", "officesmb", "us", false, "hi")]
        [DataRow("orgaddress", "update", "officesmb", "us", false, "hi")]
        [DataRow("orgaddress", "add", "officesmb", "us", true)]
        [DataRow("orgaddress", "update", "officesmb", "us", true)]
        [DataRow("orgaddress", "add", "officesmb", "us", false)]
        [DataRow("orgaddress", "update", "officesmb", "us", false)]
        [DataRow("hapiv1SoldToIndividual", "add", "officesmb", "us", true, "hi")]
        [DataRow("hapiv1SoldToIndividual", "update", "officesmb", "us", true, "hi")]
        [DataRow("hapiv1SoldToIndividual", "add", "officesmb", "us", false, "hi")]
        [DataRow("hapiv1SoldToIndividual", "update", "officesmb", "us", false, "hi")]
        [DataRow("hapiv1SoldToIndividual", "add", "officesmb", "us", true)]
        [DataRow("hapiv1SoldToIndividual", "update", "officesmb", "us", true)]
        [DataRow("hapiv1SoldToIndividual", "add", "officesmb", "us", false)]
        [DataRow("hapiv1SoldToIndividual", "update", "officesmb", "us", false)]
        [DataRow("hapiV1BillToIndividual", "add", "officesmb", "us", true, "hi")]
        [DataRow("hapiV1BillToIndividual", "update", "officesmb", "us", true, "hi")]
        [DataRow("hapiV1BillToIndividual", "add", "officesmb", "us", false, "hi")]
        [DataRow("hapiV1BillToIndividual", "update", "officesmb", "us", false, "hi")]
        [DataRow("hapiV1BillToIndividual", "add", "officesmb", "us", true)]
        [DataRow("hapiV1BillToIndividual", "update", "officesmb", "us", true)]
        [DataRow("hapiV1BillToIndividual", "add", "officesmb", "us", false)]
        [DataRow("hapiV1BillToIndividual", "update", "officesmb", "us", false)]
        [DataTestMethod]
        public async Task GetAddressDescriptions_customizeAddressForm_FeatureOptionalRemove(string type, string operation, string partner, string country, bool enableFeatureToFieldsToMakeRequired, string language = "en-US")
        {
            // Arrange
            string url = $"/v7.0/Account001/AddressDescriptions?country={country}&language={language}&type={type}&partner={partner}&operation={operation}";

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", "PXDisablePSSCache"
                }
            };

            string partnerSettingResponse;
            if (enableFeatureToFieldsToMakeRequired)
            {
                partnerSettingResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"resources\":{\"address\":{\"shipping\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null},\"shipping_v3\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null}}},\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"shipping\",\"dataSource\":\"jarvisShipping\",\"fieldsToMakeRequired\":[\"firstName\",\"lastName\",\"email\",\"phoneNumber\"]},{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\",\"fieldsToMakeRequired\":[\"companyName\",\"email\",\"phoneNumber\"]},{\"addressType\":\"hapiV1BillToIndividual\",\"dataSource\":\"hapi\",\"fieldsToMakeRequired\":[\"firstName\",\"lastName\",\"companyName\",\"email\",\"phoneNumber\"]},{\"addressType\":\"shipping_v3\",\"dataSource\":\"jarvisShippingV3\",\"fieldsToMakeRequired\":[\"firstName\",\"lastName\",\"email\",\"phoneNumber\"]},{\"addressType\":\"orgaddress\",\"dataSource\":\"jarvisOrgAddress\",\"fieldsToMakeRequired\":[\"orgEmail\",\"orgPhoneNumber\"]}]},\"addressValidation\":{\"applicableMarkets\":[]}}},\"update\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"resources\":{\"address\":{\"shipping\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null},\"shipping_v3\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null}}},\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"shipping\",\"dataSource\":\"jarvisShipping\",\"fieldsToMakeRequired\":[\"firstName\",\"lastName\",\"email\",\"phoneNumber\"]},{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\",\"fieldsToMakeRequired\":[\"companyName\",\"email\",\"phoneNumber\"]},{\"addressType\":\"hapiV1BillToIndividual\",\"dataSource\":\"hapi\",\"fieldsToMakeRequired\":[\"firstName\",\"lastName\",\"companyName\",\"email\",\"phoneNumber\"]},{\"addressType\":\"shipping_v3\",\"dataSource\":\"jarvisShippingV3\",\"fieldsToMakeRequired\":[\"firstName\",\"lastName\",\"email\",\"phoneNumber\"]},{\"addressType\":\"orgaddress\",\"dataSource\":\"jarvisOrgAddress\",\"fieldsToMakeRequired\":[\"orgEmail\",\"orgPhoneNumber\"]}]},\"addressValidation\":{\"applicableMarkets\":[]}}}}";
            }
            else
            {
                partnerSettingResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"resources\":{\"address\":{\"shipping\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null},\"shipping_v3\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null}}},\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"shipping\",\"dataSource\":\"jarvisShipping\"},{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\"},{\"addressType\":\"hapiV1BillToIndividual\",\"dataSource\":\"hapi\"},{\"addressType\":\"shipping_v3\",\"dataSource\":\"jarvisShippingV3\"},{\"addressType\":\"orgaddress\",\"dataSource\":\"jarvisOrgAddress\"}]},\"addressValidation\":{\"applicableMarkets\":[]}}},\"update\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"resources\":{\"address\":{\"shipping\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null},\"shipping_v3\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null}}},\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"shipping\",\"dataSource\":\"jarvisShipping\"},{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\"},{\"addressType\":\"hapiV1BillToIndividual\",\"dataSource\":\"hapi\"},{\"addressType\":\"shipping_v3\",\"dataSource\":\"jarvisShippingV3\"},{\"addressType\":\"orgaddress\",\"dataSource\":\"jarvisOrgAddress\"}]},\"addressValidation\":{\"applicableMarkets\":[]}}}}";
            }

            PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

            // Get PIDL with feature flight
            List<string> flights = new List<string>() { "removeOptionalInLabel" };
            List<PIDLResource> pidlWithFeatureFlights = await GetPidlFromPXServiceWithFlight(url, flights);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");

            // Below display hints are expected to be present in the PIDL in display content with "optional" and and without "optional" when the flighting feature `removeOptionalInLabel` is enabled.
            var displayHintIdsDictionary = Constants.GetDisplayHintIdsByType(type);
            var propertyDescriptionIdsDictionary = Constants.GetPropertyDescriptionIdsByType(type);

            var displayHintIds = displayHintIdsDictionary.ContainsKey(type) ? displayHintIdsDictionary[type] : new List<string>();
            var propertyDescriptionIds = propertyDescriptionIdsDictionary.ContainsKey(type) ? propertyDescriptionIdsDictionary[type] : new List<string>();

            foreach (var pidl in pidls)
            {
                foreach (var pidlWithFeatureFlight in pidlWithFeatureFlights)
                {
                    AssertDisplayHints(pidl, pidlWithFeatureFlight, displayHintIds, enableFeatureToFieldsToMakeRequired);
                }

                AssertPropertyDescriptions(pidl, propertyDescriptionIds, enableFeatureToFieldsToMakeRequired);
            }

            // Reset flighting to default state
            PXFlightHandler.ResetToDefault();

            // Reset to PSS
            PXSettings.PartnerSettingsService.ResetToDefaults();
        }

        [DataRow("hapiv1SoldToIndividual", "add", "smboobemodern", "us", "hapiV1ModernAccountV20190531Address_companyName,hapiV1ModernAccountV20190531Address_phoneNumber,hapiV1ModernAccountV20190531Address_email")]
        [DataRow("hapiv1SoldToIndividual", "update", "smboobemodern", "us", "hapiV1ModernAccountV20190531Address_companyName,hapiV1ModernAccountV20190531Address_phoneNumber,hapiV1ModernAccountV20190531Address_email")]
        [DataRow("hapiV1BillToIndividual", "add", "smboobemodern", "us", "hapiV1ModernAccountV20190531Address_companyName,hapiV1ModernAccountV20190531Address_phoneNumber,hapiV1ModernAccountV20190531Address_email,hapiV1ModernAccountV20190531Address_firstName,hapiV1ModernAccountV20190531Address_lastName")]
        [DataRow("hapiV1BillToIndividual", "update", "smboobemodern", "us", "hapiV1ModernAccountV20190531Address_companyName,hapiV1ModernAccountV20190531Address_phoneNumber,hapiV1ModernAccountV20190531Address_email,hapiV1ModernAccountV20190531Address_firstName,hapiV1ModernAccountV20190531Address_lastName")]
        [DataRow("shipping", "add", "officesmb", "us", "addressFirstNameOptional,addressLastNameOptional,emailAddressOptional,addressPhoneNumberOptional")]
        [DataRow("shipping", "update", "officesmb", "us", "addressFirstNameOptional,addressLastNameOptional,emailAddressOptional,addressPhoneNumberOptional")]
        [DataRow("shipping_v3", "add", "officesmb", "us", "addressFirstNameOptional,addressLastNameOptional,emailAddressOptional,addressPhoneNumberOptional")]
        [DataRow("shipping_v3", "update", "officesmb", "us", "addressFirstNameOptional,addressLastNameOptional,emailAddressOptional,addressPhoneNumberOptional")]
        [DataRow("orgaddress", "add", "officesmb", "us", "orgAddressModern_email,orgAddressModern_phoneNumber")]
        [DataRow("orgaddress", "update", "officesmb", "us", "orgAddressModern_email,orgAddressModern_phoneNumber")]
        [DataTestMethod]
        public async Task GetAddressDescriptions_customizeAddressForm_FeatureContextProcess_UsingPartnerSettingsService_SupportMultipleType(string type, string operation, string partner, string country, string displayHintIdsToBehidden)
        {
            // Arrange
            string url = $"/v7.0/Account001/AddressDescriptions?country={country}&language=en-US&type={type}&partner={partner}&operation={operation}";
            string partnerSettingResponse;

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", "PXDisablePSSCache"
                }
            };
           
            if (type == "shipping" || type == "shipping_v3" || type == "orgaddress")
            {
                partnerSettingResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"resources\":{\"address\":{\"shipping\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null}}},\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"shipping\",\"dataSource\":\"jarvisShipping\",\"fieldsToBeHidden\":[\"firstName\",\"lastName\",\"email\",\"phoneNumber\"]},{\"addressType\":\"shipping_v3\",\"dataSource\":\"jarvisShippingV3\",\"fieldsToBeHidden\":[\"firstName\",\"lastName\",\"email\",\"phoneNumber\"]},{\"addressType\":\"orgaddress\",\"dataSource\":\"jarvisOrgAddress\",\"fieldsToBeHidden\":[\"orgEmail\",\"orgPhoneNumber\"]}]}}},\"update\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"resources\":{\"address\":{\"shipping\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null}}},\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"shipping\",\"dataSource\":\"jarvisShipping\",\"fieldsToBeHidden\":[\"firstName\",\"lastName\",\"email\",\"phoneNumber\"]},{\"addressType\":\"shipping_v3\",\"dataSource\":\"jarvisShippingV3\",\"fieldsToBeHidden\":[\"firstName\",\"lastName\",\"email\",\"phoneNumber\"]},{\"addressType\":\"orgaddress\",\"dataSource\":\"jarvisOrgAddress\",\"fieldsToBeHidden\":[\"orgEmail\",\"orgPhoneNumber\"]}]}}}}";
            }
            else
            {
                // This response doesn't contain hideAddCreditDebitCardHeading property in json.
                partnerSettingResponse = "{\"add\":{\"template\":\"onepage\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\",\"fieldsToBeHidden\":[\"companyName\",\"email\",\"phoneNumber\"]},{\"addressType\":\"hapiV1BillToIndividual\",\"dataSource\":\"hapi\",\"fieldsToBeHidden\":[\"firstName\",\"lastName\",\"companyName\",\"email\",\"phoneNumber\"]}]},\"addressValidation\":{\"applicableMarkets\":[]}}},\"update\":{\"template\":\"onepage\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\",\"fieldsToBeHidden\":[\"companyName\",\"email\",\"phoneNumber\"]},{\"addressType\":\"hapiV1BillToIndividual\",\"dataSource\":\"hapi\",\"fieldsToBeHidden\":[\"firstName\",\"lastName\",\"companyName\",\"email\",\"phoneNumber\"]}]},\"addressValidation\":{\"applicableMarkets\":[]}}}}";
            }

            PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");

            foreach (string hintId in displayHintIdsToBehidden.Split(',').ToList())
            {
                Assert.IsTrue(pidls[0].GetDisplayHintById(hintId).IsHidden, $"DisplayHint with id {hintId} is not hiddend.");
            }
        }
        
        /// <summary>
        /// This test verifies the sequential positioning of the first name and last name fields within the address form.
        /// It also incorporates the feature to disable first name and last name grouping,
        /// and to change the address type feature from hapiv1SoldToIndividual to hapiV1.
        /// </summary>
        /// <param name="type"> Address Type</param>
        /// <param name="operation"> Operations</param>
        /// <param name="enableDisableFirstNameLastNameGroupingPSSFeature"> Check the feature disableFirstNameLastNameGrouping  is enabled or not.</param>
        /// <param name="isAddressTypeHapiV1"> Check the address type is hapiV1 or not.</param>
        /// <returns></returns>
        [DataRow("hapiv1SoldToIndividual", "add", true)]
        [DataRow("hapiv1SoldToIndividual", "add", false)]
        [DataRow("hapiv1SoldToIndividual", "update", true)]
        [DataRow("hapiv1SoldToIndividual", "update", false)]
        [DataRow("hapiV1", "add", false, true)]
        [DataRow("hapiV1", "update", false, true)]
        [DataTestMethod]
        public async Task GetAddressDescriptions_customizeAddressForm_ValidateFirstNameAndLastNameSequencePosition(string type, string operation, bool enableDisableFirstNameLastNameGroupingPSSFeature, bool isAddressTypeHapiV1 = false)
        {
            // Arrange
            List<string> countries = new List<string> { "cn", "jp", "kr", "tw" };
            string partnerSettingResponse = string.Empty;
            string hapiFirstName = Constants.DisplayHintIds.HapiFirstName;
            string hapiLastName = Constants.DisplayHintIds.HapiLastName;
            int firstNameMemberSequencePosition, lastNameMemberSequencePosition = 0;

            foreach (string country in countries)
            {
                string url = $"/v7.0/Account001/AddressDescriptions?country={country}&language=en-US&type={type}&partner=officesmb&operation={operation}";

                var headers = new Dictionary<string, string>()
                {
                    {
                        "x-ms-flight", "PXDisablePSSCache"
                    }
                };

                if (enableDisableFirstNameLastNameGroupingPSSFeature)
                {
                    partnerSettingResponse = "{\"add\":{\"template\":\"onepage\",\"features\":{\"customizeElementLocation\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"moveLastNameBeforeFirstName\":true}]},\"disableFirstNameLastNameGrouping\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\",\"fieldsToBeHidden\":[\"companyName\",\"email\",\"phoneNumber\"]}]},\"showMiddleName\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"addressValidation\":{\"applicableMarkets\":[]}}},\"update\":{\"template\":\"onepage\",\"features\":{\"customizeElementLocation\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"moveLastNameBeforeFirstName\":true}]},\"disableFirstNameLastNameGrouping\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\",\"fieldsToBeHidden\":[\"companyName\",\"email\",\"phoneNumber\"]}]},\"showMiddleName\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"addressValidation\":{\"applicableMarkets\":[]}}}}";
                }
                else if (isAddressTypeHapiV1)
                {
                    partnerSettingResponse = "{\"add\":{\"template\":\"onepage\",\"features\":{\"customizeElementLocation\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"moveLastNameBeforeFirstName\":true}]},\"showMiddleName\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"addressValidation\":{\"applicableMarkets\":[]}}},\"update\":{\"template\":\"onepage\",\"features\":{\"customizeElementLocation\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"moveLastNameBeforeFirstName\":true}]},\"showMiddleName\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"addressValidation\":{\"applicableMarkets\":[]}}}}";
                }
                else
                {
                    partnerSettingResponse = "{\"add\":{\"template\":\"onepage\",\"features\":{\"customizeElementLocation\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"moveLastNameBeforeFirstName\":true}]},\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\",\"fieldsToBeHidden\":[\"companyName\",\"email\",\"phoneNumber\"]}]},\"showMiddleName\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"addressValidation\":{\"applicableMarkets\":[]}}},\"update\":{\"template\":\"onepage\",\"features\":{\"customizeElementLocation\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"moveLastNameBeforeFirstName\":true}]},\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\",\"fieldsToBeHidden\":[\"companyName\",\"email\",\"phoneNumber\"]}]},\"showMiddleName\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"addressValidation\":{\"applicableMarkets\":[]}}}}";
                }

                PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);

                // Act
                List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

                // Assert
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");

                firstNameMemberSequencePosition = pidls[0].DisplayPages[0].Members.FindIndex(displayHint => displayHint.HintId == hapiFirstName);
                lastNameMemberSequencePosition = pidls[0].DisplayPages[0].Members.FindIndex(displayHint => displayHint.HintId == hapiLastName);

                if (firstNameMemberSequencePosition == -1 && lastNameMemberSequencePosition == -1)
                {
                    GroupDisplayHint firstNameAndLastNameMemberGroup = pidls[0].DisplayPages[0].Members.Find(displayHint => displayHint.HintId == Constants.DisplayHintIds.HapiV1ModernAccountV20190531AddressFirstAndLastNameGroup) as GroupDisplayHint;
                    firstNameMemberSequencePosition = firstNameAndLastNameMemberGroup.Members.FindIndex(displayHint => displayHint.HintId == hapiFirstName);
                    lastNameMemberSequencePosition = firstNameAndLastNameMemberGroup.Members.FindIndex(displayHint => displayHint.HintId == hapiLastName);
                }

                Assert.IsTrue(lastNameMemberSequencePosition < firstNameMemberSequencePosition);
            }
        }

        /// <summary>
        /// For org forms, organization name should be placed before email.
        /// </summary>
        /// <param name="type"> Address Type</param>
        /// <param name="operation"> Operations</param>
        /// <returns></returns>
        [DataRow("hapiv1SoldToOrganization", "add")]
        [DataTestMethod]
        public async Task GetAddressDescriptions_customizeAddressForm_ValidateEmailAndOrganizationNameSequencePosition(string type, string operation)
        {
            // Arrange
            string partnerSettingResponse = string.Empty;
            string hapiCompanyName = Constants.DisplayHintIds.HapiCompanyName;
            string hapiEmail = Constants.DisplayHintIds.HapiEmail;
            int companyNameNameMemberSequencePosition, emailMemberSequencePosition = 0;

            string url = $"/v7.0/AddressDescriptions?country=us&language=en-US&type={type}&partner=azurebmx&operation={operation}";

            var headers = new Dictionary<string, string>()
                {
                    {
                        "x-ms-flight", "PXDisablePSSCache"
                    }
                };

            partnerSettingResponse = "{\"add\":{\"template\":\"onepage\",\"features\":{\"customizeElementLocation\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"moveOrganizationNameBeforeEmailAddress\":true}]},\"disableFirstNameLastNameGrouping\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"" + type + "\",\"dataSource\":\"hapi\",\"fieldsToBeHidden\":[\"companyName\",\"email\",\"phoneNumber\"]}]},\"showMiddleName\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"addressValidation\":{\"applicableMarkets\":[]}}},\"update\":{\"template\":\"onepage\",\"features\":{\"customizeElementLocation\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"moveLastNameBeforeFirstName\":true}]},\"disableFirstNameLastNameGrouping\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"" + type + "\",\"dataSource\":\"hapi\",\"fieldsToBeHidden\":[\"companyName\",\"email\",\"phoneNumber\"]}]},\"showMiddleName\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"addressValidation\":{\"applicableMarkets\":[]}}}}";

            PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");

            companyNameNameMemberSequencePosition = pidls[0].DisplayPages[0].Members.FindIndex(displayHint => displayHint.HintId == hapiCompanyName);
            emailMemberSequencePosition = pidls[0].DisplayPages[0].Members.FindIndex(displayHint => displayHint.HintId == hapiEmail);

            Assert.IsTrue(companyNameNameMemberSequencePosition < emailMemberSequencePosition);
        }

        [DataRow("billing", "add", "officeoobe", "us", "PXSkipJarvisAddressSyncToLegacy", "https://{jarvis-endpoint}/JarvisCM/{userId}/addresses?syncToLegacy=0")]
        [DataRow("billing", "add", "officeoobe", "us", "", "https://{jarvis-endpoint}/JarvisCM/{userId}/addresses")]
        [DataTestMethod]
        public async Task GetAddressDescriptions_Test_SkipJarvisAddressSyncToLegacy(string type, string operation, string partner, string country, string flights, string jarvisUrl)
        {
            string url = $"/v7.0/Account001/AddressDescriptions?country={country}&language=en-US&type={type}&partner={partner}&operation={operation}";

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", flights
                }
            };

            // Get PIDL
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");

            foreach (var pidl in pidls)
            {
                dynamic submitLink = pidl.GetDisplayHintById("saveButton").Action.Context;
                Assert.IsNotNull(submitLink);
                Assert.AreEqual(jarvisUrl, submitLink.href.ToString());
            }
        }

        /// <summary>
        /// CIT to test the PSS feature GroupAddressFields when enabled or disabled
        /// </summary>
        /// <param name="type"></param>
        /// <param name="operation"></param>
        /// <param name="partner"></param>
        /// <returns></returns>
        [DataRow("billing", "add", "officesmb")]
        [DataRow("billing", "update", "officesmb")]
        [DataRow("shipping", "add", "officesmb")]
        [DataRow("shipping", "update", "officesmb")]
        [DataRow("shipping_v3", "add", "officesmb")]
        [DataRow("shipping_v3", "update", "officesmb")]
        [DataRow("orgaddress", "add", "officesmb")]
        [DataRow("orgaddress", "update", "officesmb")]
        [DataRow("hapiserviceusageaddress", "add", "officesmb")]
        [DataRow("hapiserviceusageaddress", "update", "officesmb")]
        [DataRow("hapiv1SoldToIndividual", "add", "officesmb")]
        [DataRow("hapiv1SoldToIndividual", "update", "officesmb")]
        [DataRow("hapiv1SoldToOrganization", "add", "officesmb")]
        [DataRow("hapiv1SoldToOrganization", "update", "officesmb")]
        [DataRow("hapiV1BillToIndividual", "add", "officesmb")]
        [DataRow("hapiV1BillToIndividual", "update", "officesmb")]
        [DataRow("hapiV1BillToOrganization", "add", "officesmb")]
        [DataRow("hapiV1BillToOrganization", "update", "officesmb")]
        [DataTestMethod]
        public async Task GetAddressDescriptions_Test_PSS_GroupAddressFields(string type, string operation, string partner)
        {
            // Arrange
            bool[] featureStatus = new bool[] { true, false };

            // groupAddressFields featureStatus, country, enableZipCodeStateGrouping featureStatus, addressFields
            Dictionary<bool, Dictionary<string, Dictionary<bool, List<string>>>> addressGroupFieldsOrderByFeature = new Dictionary<bool, Dictionary<string, Dictionary<bool, List<string>>>>();
            foreach (bool useGroupAddressFieldsFeature in featureStatus)
            {
                Dictionary<string, Dictionary<bool, List<string>>> addressFieldsOrderByCountry = new Dictionary<string, Dictionary<bool, List<string>>>();
               
                foreach (string country in Constants.Countries)
                {
                    Dictionary<bool, List<string>> addressFieldsOrderByZipCodeStateGrouping = new Dictionary<bool, List<string>>();
                    foreach (bool enableZipCodeStateGrouping in featureStatus)
                    {
                        string requestUrl = $"/v7.0/Account001/AddressDescriptions?country={country}&language=en-US&type={type}&partner={partner}&operation={operation}";
                        var headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };

                        string features = useGroupAddressFieldsFeature ? "\"groupAddressFields\":{\"applicableMarkets\":[]}" : string.Empty;

                        if (enableZipCodeStateGrouping)
                        {
                            string zipCodeStateGroupingFeature = "\"enableZipCodeStateGrouping\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]}";
                            features = string.IsNullOrEmpty(features) ? zipCodeStateGroupingFeature : $"{features},{zipCodeStateGroupingFeature}";
                        }

                        string expectedPSSResponse = "{\"" + operation + "\":{\"template\":\"defaulttemplate\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"shipping\",\"dataSource\":\"jarvisShipping\"},{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\"},{\"addressType\":\"hapiV1BillToOrganization\",\"dataSource\":\"hapi\"},{\"addressType\":\"hapiv1SoldToOrganization\",\"dataSource\":\"hapi\"},{\"addressType\":\"hapiV1BillToIndividual\",\"dataSource\":\"hapi\"},{\"addressType\":\"shipping_v3\",\"dataSource\":\"jarvisShippingV3\"}]},\"addressValidation\":{\"applicableMarkets\":[]}," + features + "}}}";
                        
                        PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

                        // Act
                        List<PIDLResource> pidls = await GetPidlFromPXService(requestUrl, additionaHeaders: headers);

                        // Assert
                        Assert.IsNotNull(pidls, "Pidls expected to not be null");

                        List<string> addressFieldsWithOrderByPidl = new List<string>();

                        foreach (var pidl in pidls)
                        {
                            Assert.IsNotNull(pidl.DisplayPages, "DisplayPages is expected to be not null");

                            foreach (PageDisplayHint pidlDisplayPage in pidl.DisplayPages)
                            {
                                SortedDictionary<int, string> addressFieldsWithOrder = new SortedDictionary<int, string>();
                                GroupDisplayHint addressGroup = pidl.GetDisplayHintById("addressGroup") as GroupDisplayHint;

                                foreach (var addressField in Constants.AddressFields)
                                {
                                    DisplayHint addressFieldDisplayHint = pidl.GetDisplayHintById(addressField);
                                    if (addressFieldDisplayHint != null)
                                    {
                                        if (useGroupAddressFieldsFeature)
                                        {
                                            Assert.IsNotNull(addressGroup, "addressGroup is not expected to be null");
                                            Assert.IsTrue(addressGroup.Members.Contains(addressFieldDisplayHint), $"DisplayHint \"{addressField}\" is expected to be inside addressGroup. Test Country: {country}");
                                            addressFieldsWithOrder.Add(addressGroup.Members.IndexOf(addressFieldDisplayHint), addressField);
                                        }
                                        else
                                        {
                                            Assert.IsNull(addressGroup, "addressGroup is expected to be null");
                                            addressFieldsWithOrder.Add(pidl.DisplayPages[0].Members.IndexOf(addressFieldDisplayHint), addressField);
                                        }
                                    }
                                }

                                addressFieldsWithOrderByPidl.Add(string.Join(",", addressFieldsWithOrder.Values));
                            }
                        }

                        addressFieldsOrderByZipCodeStateGrouping.Add(enableZipCodeStateGrouping, addressFieldsWithOrderByPidl);

                        PXSettings.PartnerSettingsService.ResetToDefaults();
                    }

                    addressFieldsOrderByCountry.Add(country, addressFieldsOrderByZipCodeStateGrouping);
                }

                addressGroupFieldsOrderByFeature.Add(useGroupAddressFieldsFeature, addressFieldsOrderByCountry);
            }

            // Assert address fields order when feature groupAddressFields is enabled and disabled
            string addressFieldOrderByCountryFeatureEnabled = JsonConvert.SerializeObject(addressGroupFieldsOrderByFeature[true]);
            string addressFieldOrderByCountryFeatureDisabled = JsonConvert.SerializeObject(addressGroupFieldsOrderByFeature[false]);

            // Compare both strings manually to find the exact difference for country's address fields order
            Assert.AreEqual(addressFieldOrderByCountryFeatureEnabled, addressFieldOrderByCountryFeatureDisabled, "Address fields order should be same when feature enabled and disabled");
        }

        /// <summary>
        /// This CIT validates the changes made under the feature when the addAllFieldsRequiredText feature or the PXEnableAddAllFieldsRequiredText feature flight is enabled.
        /// It ensures that the fields contain the new members with "All fields are mandatory/required." based on different conditions.
        /// </summary>
        /// <param name="partner"></param>
        /// <param name="isFeatureFlightPXEnableAddAllFieldsRequiredTextEnable">Indicates if the PXEnableAddAllFieldsRequiredText feature flight is enabled.</param>
        /// <param name="isFeatureAddAllFieldsRequiredTextStatus">Checks if the addAllFieldsRequiredText feature is enabled for the pssBased partner.</param>
        /// <param name="isPSSPartnerEnabledForPartner">Checks if the partner is PSS parner or not.</param>
        /// <returns></returns>
        [DataRow("officesmb", false, true, true)]
        [DataRow("officesmb", false, false, true)]
        [DataRow("commercialstores", true, false, false)]
        [DataRow("commercialstores", false, false, false)]
        [DataRow("webblends", true, false, false)]
        [DataRow("webblends", false, false, false)]
        [DataTestMethod]
        public async Task GetAddressDescriptions_Test_PSS_MandatoryFeildsTextFeature(string partner, bool isFeatureFlightPXEnableAddAllFieldsRequiredTextEnable, bool isFeatureAddAllFieldsRequiredTextStatus, bool isPSSPartnerEnabledForPartner)
        {
            // Arrange
            List<string> operations = new List<string> { Constants.OperationTypes.Add, Constants.OperationTypes.Update };
            string partnerSettingResponse;

          List<string> addressTypes = new List<string> { "billing", "hapiv1SoldToIndividual", "hapiv1SoldToOrganization", "hapiV1BillToIndividual", "hapiV1BillToOrganization", "shipping", "shipping_v3", "orgaddress" };
          //// List<string> addressTypes = new List<string> { "hapiv1SoldToOrganization" };

            foreach (var type in addressTypes)
            {
                var headers = new Dictionary<string, string>()
                {
                    {
                        "x-ms-flight", "PXDisablePSSCache"
                    }
                };
                foreach (string operation in operations)
                {
                    string url = $"/v7.0/Account001/AddressDescriptions?country=us&language=en-US&type={type}&partner={partner}&operation={operation}";

                    if (isPSSPartnerEnabledForPartner)
                    {
                        string pssPartnerName = string.Equals(partner, Constants.VirtualPartnerNames.OfficeSmb) ? "defaulttemplate" : "twopage";
                        partnerSettingResponse = "{\"add\":{\"template\":\"" + pssPartnerName + "\",\"features\":{\"customizeDisplayContent\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addAllFieldsRequiredText\":" + isFeatureAddAllFieldsRequiredTextStatus.ToString().ToLower() + "}]}}},\"update\":{\"template\":\"" + pssPartnerName + "\",\"features\":{\"customizeDisplayContent\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addAllFieldsRequiredText\":" + isFeatureAddAllFieldsRequiredTextStatus.ToString().ToLower() + "}]}}}}";
                        PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
                    }

                    if (isFeatureFlightPXEnableAddAllFieldsRequiredTextEnable)
                    {
                        PXFlightHandler.AddToEnabledFlights(Constants.PartnerFlightValues.PXEnableAddAllFieldsRequiredText);
                    }

                    // Act
                    List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

                    // Assert
                    Assert.IsNotNull(pidls, "Pidl is expected to be not null");

                    foreach (PIDLResource pidl in pidls)
                    {
                        if (pidl.DisplayPages != null)
                        {
                            foreach (PageDisplayHint displayPage in pidl.DisplayPages)
                            {
                                PropertyDisplayHint propertyNmaeValue = displayPage?.Members.Find(displayHint => displayHint.DisplayHintType.Equals(Constants.DisplayHintTypes.Property, StringComparison.OrdinalIgnoreCase)) as PropertyDisplayHint;

                                TextGroupDisplayHint starTextGroupDisplayHint = displayPage?.Members.Find(displayHint => displayHint.HintId.Equals(Constants.DisplayHintIds.StarRequiredTextGroup, StringComparison.OrdinalIgnoreCase)) as TextGroupDisplayHint;
                                TextDisplayHint mandatoryFieldsMessageHint = displayPage?.Members.Find(displayHint => displayHint.HintId.Equals(Constants.DisplayHintIds.MandatoryFieldsMessage, StringComparison.OrdinalIgnoreCase)) as TextDisplayHint;

                                if (isFeatureAddAllFieldsRequiredTextStatus || isFeatureFlightPXEnableAddAllFieldsRequiredTextEnable)
                                {
                                    // For the Paypal, sepa, and klarna family types, the starRequiredTextGroup and mandatory text should be null in PIDL.
                                    if (propertyNmaeValue == null)
                                    {
                                        Assert.IsNull(starTextGroupDisplayHint, $"For {url} When the feature is enabled, starRequiredTextGroup should be null in PIDL.");
                                        Assert.IsNull(mandatoryFieldsMessageHint, $"For {url} mandatory_fields_message should be null in PIDL.");
                                    }
                                    else
                                    {
                                        Assert.IsNull(starTextGroupDisplayHint, $"For {url} When the feature is enabled, starRequiredTextGroup should be null in PIDL.");
                                        Assert.IsNotNull(mandatoryFieldsMessageHint, $"For {url} mandatory_fields_message should not be in PIDL when the feature or flight feature is enabled.");
                                        Assert.AreEqual(mandatoryFieldsMessageHint.DisplayContent, "All fields are mandatory/required.", $"For {url} The mandatory message should contain 'All fields are mandatory/required.'");
                                    }
                                }
                                else
                                {
                                    if (isPSSPartnerEnabledForPartner)
                                    {
                                        // shipping address don't have startTextGroupDisplayHint.
                                        if (string.Equals(type, Constants.AddressTypes.Shipping, StringComparison.OrdinalIgnoreCase)
                                            || string.Equals(type, Constants.AddressTypes.ShippingV3, StringComparison.OrdinalIgnoreCase))
                                        {
                                            Assert.IsNull(starTextGroupDisplayHint, $"For {url} When feature is not enabled the starRequiredTextGroup should not be null in PIDL.");
                                        }
                                        else
                                        {
                                            Assert.IsNotNull(starTextGroupDisplayHint, $"For {url} When feature is not enabled the starRequiredTextGroup should not be null in PIDL.");
                                        }
                                    }
                                    else
                                    {
                                        Assert.IsNull(mandatoryFieldsMessageHint, $"For {url} mandatory_fields_message should not be present in PIDL when feature is enabled");
                                    }
                                }
                            }
                        }
                    }

                    PXSettings.PartnerSettingsService.ResetToDefaults();
                    PXSettings.PimsService.ResetToDefaults();
                    PXFlightHandler.ResetToDefault();
                }
            }
        }

        /// <summary>
        /// This CIT validates the changes made under the feature flight to validate the headers.
        /// </summary>
        /// <param name="type">The type of address description.</param>
        /// <param name="operation">The operation being performed (e.g., add, update).</param>
        /// <param name="partner">The partner for which the address description is being validated.</param>
        /// <param name="enableAddtionalHeaderFooterTitle">Flag to enable additional header and footer title.</param>
        /// <param name="enableAddtionalAddressHeader">Flag to enable additional address header.</param>
        /// <param name="isaddHapiSUADisabledTaxResourceIdStatus">Flag to check if Hapi SUA Disabled Tax Resource ID status is enabled.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [DataRow("hapiv1SoldToIndividual", "add", GlobalConstants.Partners.OfficeSMB, false, false)]
        [DataRow("hapiv1SoldToIndividual", "add", GlobalConstants.Partners.OfficeSMB, true, false)]
        [DataRow("hapiv1SoldToOrganization", "add", GlobalConstants.Partners.OfficeSMB, false, false)]
        [DataRow("hapiv1SoldToOrganization", "add", GlobalConstants.Partners.OfficeSMB, true, false)]
        [DataRow("hapiV1BillToIndividual", "add", GlobalConstants.Partners.OfficeSMB, false, false)]
        [DataRow("hapiV1BillToIndividual", "add", GlobalConstants.Partners.OfficeSMB, true, false)]
        [DataRow("hapiV1BillToOrganization", "add", GlobalConstants.Partners.OfficeSMB, false, false)]
        [DataRow("hapiV1BillToOrganization", "add", GlobalConstants.Partners.OfficeSMB, true, false)]
        [DataRow("hapiserviceusageaddress", "add", GlobalConstants.Partners.MacManage, false, false)]
        [DataRow("hapiserviceusageaddress", "add", GlobalConstants.Partners.MacManage, false, true)]
        [DataRow("hapiserviceusageaddress", "add", GlobalConstants.Partners.MacManage, false, false, true)]
        [DataRow("hapiserviceusageaddress", "add", GlobalConstants.Partners.MacManage, false, true, true)]
        [DataTestMethod]
        public async Task GetPaymentMethodDescriptions_AddtionalHeaderAndFooter_UsePartnerSettings(string type, string operation, string partner, bool enableAddtionalHeaderFooterTitle, bool enableAddtionalAddressHeader, bool isaddHapiSUADisabledTaxResourceIdStatus = false)
        {
            // Arrange
            string partnerSettingResponse;
            string pssResponse;

            var hapiSUADisabledTaxResourceIdCountries = new string[] { "tr", "am", "no", "by", "cl", "mx", "my", "bd", "id", "th", "bh", "cm", "ge", "gh", "is", "ke", "md", "ng", "om", "tj", "ua", "uz", "zw", "fj", "gt", "kh", "ph", "vn" };
            var countries = isaddHapiSUADisabledTaxResourceIdStatus ? hapiSUADisabledTaxResourceIdCountries : Constants.Countries;

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", "PXDisablePSSCache"
                }
            };

            // Note: The prefillData functionality is only applicable for the add operation, as it is not available for the update operation.
            string addressTypeChangeFeature = (string.Equals(type, "hapiV1BillToIndividual", StringComparison.OrdinalIgnoreCase) || string.Equals(type, "hapiv1SoldToOrganization", StringComparison.OrdinalIgnoreCase) || string.Equals(type, "hapiV1BillToOrganization", StringComparison.OrdinalIgnoreCase)) ? "\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"" + type + "\",\"dataSource\":\"hapi\"}]}," : string.Empty;
            partnerSettingResponse = "{\"" + operation + "\":{\"template\":\"onepage\",\"features\":{" + addressTypeChangeFeature + "\"enableAddtionalAddressTitle\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":" + enableAddtionalHeaderFooterTitle.ToString().ToLower() + "}]},\"enableAddtionalAddressHeader\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":" + enableAddtionalHeaderFooterTitle.ToString().ToLower() + "}]},\"enableAddtionalAddressFooter\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":" + enableAddtionalHeaderFooterTitle.ToString().ToLower() + "}]}}}}";

            string isaddHapiSUADisabledTaxResourceIdFeature = isaddHapiSUADisabledTaxResourceIdStatus ? "\"addHapiSUADisabledTaxResourceId\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null}," : string.Empty;
            pssResponse = "{\"" + operation + "\":{\"template\":\"defaulttemplate\",\"features\":{" + isaddHapiSUADisabledTaxResourceIdFeature + "\"enableAddtionalAddressHeader\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":" + enableAddtionalAddressHeader.ToString().ToLower() + "}]}}}}";

            PXSettings.PartnerSettingsService.ArrangeResponse(string.Equals(partner, GlobalConstants.Partners.MacManage, StringComparison.OrdinalIgnoreCase) ? pssResponse : partnerSettingResponse);

            foreach (string country in countries)
            {
                string url = $"/v7.0/Account001/AddressDescriptions?country={country}&language=en-US&type={type}&partner={partner}&operation={operation}";

                // Act
                List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

                // Assert
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");

                foreach (var pidl in pidls)
                {
                    Assert.IsNotNull(pidl.DisplayPages, "DisplayPages is expected to be not null");

                    foreach (var displayPage in pidl.DisplayPages)
                    {
                        var pageMembers = displayPage.Members;
                        var addtionalHeader = pageMembers.FirstOrDefault(item => item.HintId == "additionalAddressHeaderPlaceHolder");
                        var addtionalFooter = pageMembers.FirstOrDefault(item => item.HintId == "additionalAddressFooterPlaceHolder");
                        var addtionalTitle = pageMembers.FirstOrDefault(item => item.HintId == "additionalAddressTitlePlaceHolder");
                        Assert.IsTrue((enableAddtionalHeaderFooterTitle || enableAddtionalAddressHeader) ? addtionalHeader != null : addtionalHeader == null);
                        Assert.IsTrue(enableAddtionalHeaderFooterTitle ? addtionalFooter != null : addtionalFooter == null);
                        Assert.IsTrue(enableAddtionalHeaderFooterTitle ? addtionalTitle != null : addtionalTitle == null);
                    }
                }
            }

            PXSettings.PartnerSettingsService.ResetToDefaults();
        }

        [DataRow("hapiv1SoldToIndividual", "add", "officesmb", "validateThenSubmitButton", "us", true)]
        [DataRow("hapiv1SoldToIndividual", "add", "officesmb", "validateThenSubmitButton", "jp", false)]
        [DataRow("billing", "add", "officesmb", "saveButton", "us", true)]
        [DataRow("billing", "add", "officesmb", "saveButton", "jp", false)]
        [DataRow("shipping", "add", "officesmb", "saveButton", "us", true)]
        [DataRow("shipping", "add", "officesmb", "saveButton", "jp", false)]
        [DataRow("shipping_v3", "add", "officesmb", "saveButton", "us", true)]
        [DataRow("shipping_v3", "add", "officesmb", "saveButton", "jp", false)]
        [DataRow("hapiserviceusageaddress", "add", "officesmb", "submitButtonHidden", "us", true)]
        [DataRow("hapiserviceusageaddress", "add", "officesmb", "submitButtonHidden", "jp", false)]
        [DataTestMethod]
        public async Task GetAddressDescriptions_ModernValidate_PartnerSetting(string type, string operation, string partner, string buttonDisplayId, string country, bool isAddressValidationEnabled)
        {
            // Arrange
            string expectedPSSResponse = null;
            Dictionary<string, string> headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };
            string url = $"/v7.0/Account001/AddressDescriptions?country={country}&language=en-US&type={type}&partner={partner}&operation={operation}";

            if (isAddressValidationEnabled)
            {
                expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"resources\":{\"address\":{\"shipping\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null},\"shipping_v3\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null}}},\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"shipping\",\"dataSource\":\"jarvisShippingV3\"},{\"addressType\":\"shipping_v3\",\"dataSource\":\"jarvisShippingV3\"}]},\"addressValidation\":{\"applicableMarkets\":[]}}}}";
            }
            else
            {
                expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"features\":null}}";
            }

            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");
            foreach (PIDLResource profilePidl in pidls)
            {
                DisplayHint saveButtonDisplayHint = profilePidl.GetDisplayHintById(buttonDisplayId);
                Assert.IsNotNull(saveButtonDisplayHint);
                Assert.IsNotNull(saveButtonDisplayHint.Action);
                Assert.IsNotNull(saveButtonDisplayHint.Action.Context);

                if (saveButtonDisplayHint.Action.ActionType != Constants.ActionType.Submit)
                {
                    Assert.IsNotNull(saveButtonDisplayHint.Action.NextAction);
                }

                if (isAddressValidationEnabled)
                {
                    Assert.IsTrue(saveButtonDisplayHint.Action.Context.ToString().Contains("ModernValidate"));

                    AddressTestsUtil.VerifyModernValidationErrorStrings(JsonConvert.SerializeObject(profilePidl.PidlResourceStrings));

                    bool contain_is_customer_consented_checkbox = false;
                    bool contain_is_avs_full_validation_succeeded_checkbox = false;
                    foreach (DisplayHint displayHint in profilePidl.DisplayPages[0].Members)
                    {
                        if (string.Equals(displayHint.PropertyName, Constants.CommercialZipPlusFourPropertyNames.IsUserConsented, StringComparison.OrdinalIgnoreCase))
                        {
                            contain_is_customer_consented_checkbox = true;
                        }

                        if (string.Equals(displayHint.PropertyName, Constants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded, StringComparison.OrdinalIgnoreCase))
                        {
                            contain_is_avs_full_validation_succeeded_checkbox = true;
                        }
                    }

                    Assert.IsTrue(contain_is_customer_consented_checkbox);
                    Assert.IsTrue(contain_is_avs_full_validation_succeeded_checkbox);

                    // The address type hapiv1SoldToIndividual has the property 'address' in the data description, while the rest of the address types do not have the 'address' property.
                    List<PIDLResource> addressPidls = type == "hapiv1SoldToIndividual"
                        ? profilePidl.DataDescription["address"] as List<PIDLResource>
                        : new List<PIDLResource> { profilePidl };
                    Assert.IsNotNull(addressPidls[0].DataDescription[Constants.CommercialZipPlusFourPropertyNames.IsUserConsented]);
                    Assert.IsNotNull(addressPidls[0].DataDescription[Constants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded]);
                }
                else
                {
                    var urlContextValue = (type == "billing" || type == "shipping" || type == "shipping_v3") ? "JarvisCM" : type == "hapiserviceusageaddress" ? "hapi-endpoint" : "legacyValidate";
                    Assert.IsTrue(saveButtonDisplayHint.Action.Context.ToString().Contains(urlContextValue));
                }
            }

            PXSettings.PartnerSettingsService.ResetToDefaults();
        }

        /// <summary>
        /// This test is used to verify the submit href for the address types when the JarvisCM or addressEx feature is enabled.
        /// </summary>
        /// <param name="type">Address type</param>
        /// <param name="operation">Operation type</param>
        /// <param name="partner">Partner</param>
        /// <param name="buttonDisplayId">It store the button type is used for submit action.</param>
        /// <param name="country">Country type</param>
        /// <param name="jarvisEnableFeature">It is indicate the JarvisCM submitActionType feature is enabled or not.</param>
        /// <param name="addressExEnableFeature">It indicates the AddressEx submitActionType feature is enbaled or not.</param>
        /// <returns></returns>
        [DataRow("hapiv1SoldToIndividual", "add", "officesmb", "validateThenSubmitButton", "us", true)]
        [DataRow("hapiv1SoldToIndividual", "add", "officesmb", "validateThenSubmitButton", "us", false)]
        [DataRow("orgAddress", "add", "officesmb", "validateButtonHidden", "us", true)]
        [DataRow("orgAddress", "add", "officesmb", "validateButtonHidden", "us", false)]
        [DataRow("hapiserviceusageaddress", "add", "officesmb", "validateThenSubmitButton", "us", true)]
        [DataRow("hapiserviceusageaddress", "add", "officesmb", "submitButtonHidden", "us", false)]
        [DataRow("billing", "add", "officesmb", "saveButton", "us", true)]
        [DataRow("billing", "add", "officesmb", "saveButton", "us", false)]
        [DataRow("shipping", "add", "officesmb", "saveButton", "us", true)]
        [DataRow("shipping", "add", "officesmb", "saveButton", "us", false)]
        [DataRow("hapiv1SoldToIndividual", "add", "officesmb", "validateThenSubmitButton", "us", false, true)]
        [DataRow("orgAddress", "add", "officesmb", "validateButtonHidden", "us", false, true)]
        [DataRow("hapiserviceusageaddress", "add", "officesmb", "validateThenSubmitButton", "us", false, true)]
        [DataRow("billing", "add", "officesmb", "saveButton", "us", false, true)]
        [DataRow("shipping", "add", "officesmb", "saveButton", "us", false, true)]
        [DataRow("hapiv1SoldToIndividual", "update", "officesmb", "validateThenSubmitButton", "us", false, true)]
        [DataRow("orgAddress", "update", "officesmb", "validateButtonHidden", "us", false, true)]
        [DataRow("hapiserviceusageaddress", "update", "officesmb", "validateThenSubmitButton", "us", false, true)]
        [DataRow("billing", "update", "officesmb", "saveButton", "us", false, true)]
        [DataRow("shipping", "update", "officesmb", "saveButton", "us", false, true)]
        [DataRow("hapiv1SoldToIndividual", "add", "macmange", "validateThenSubmitButton", "us", false, true)]
        [DataRow("orgAddress", "add", "macmange", "validateButtonHidden", "us", false, true)]
        [DataRow("hapiserviceusageaddress", "add", "macmange", "validateThenSubmitButton", "us", false, true)]
        [DataRow("billing", "add", "macmange", "saveButton", "us", false, true)]
        [DataRow("shipping", "add", "macmange", "saveButton", "us", false, true)]
        [DataRow("hapiv1SoldToIndividual", "update", "macmange", "validateThenSubmitButton", "us", false, true)]
        [DataRow("orgAddress", "update", "macmange", "validateButtonHidden", "us", false, true)]
        [DataRow("hapiserviceusageaddress", "update", "macmange", "validateThenSubmitButton", "us", false, true)]
        [DataRow("billing", "update", "macmange", "saveButton", "us", false, true)]
        [DataRow("shipping", "update", "macmange", "saveButton", "us", false, true)]
        [DataTestMethod]
        public async Task GetAddressDescriptions_SubmitActionForJarvis_PartnerSetting(string type, string operation, string partner, string buttonDisplayId, string country, bool jarvisEnableFeature, bool addressExEnableFeature = false)
        {
            // Arrange
            Dictionary<string, string> headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };
            string url = $"/v7.0/Account001/AddressDescriptions?country={country}&language=en-US&type={type}&partner={partner}&operation={operation}";
            var sceanrio = type == "orgAddress" ? "modernAccount" : string.Empty;

            string expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\"}}";
            
            if (jarvisEnableFeature)
            {
                expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[\"us\"],\"displayCustomizationDetail\":[{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\",\"submitActionType\":\"jarvisCM\"},{\"addressType\":\"orgAddress\",\"dataSource\":\"jarvisOrgAddress\",\"submitActionType\":\"jarvisCM\"},{\"addressType\":\"hapiserviceusageaddress\",\"dataSource\":\"hapi\",\"submitActionType\":\"jarvisCM\"}]}}}}";
            }
            else if (addressExEnableFeature)
            {
                expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"shipping\",\"dataSource\":\"jarvisShippingV3\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":true},{\"addressType\":\"billing\",\"dataSource\":\"jarvisBilling\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":true},{\"addressType\":\"hapiv1SoldToIndividual\",\"dataSource\":\"hapi\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":true},{\"addressType\":\"orgAddress\",\"dataSource\":\"jarvisOrgAddress\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":true},{\"addressType\":\"hapiserviceusageaddress\",\"dataSource\":\"hapi\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":true}]}}},\"update\":{\"template\":\"defaulttemplate\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"shipping\",\"dataSource\":\"jarvisShippingV3\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":true},{\"addressType\":\"billing\",\"dataSource\":\"jarvisBilling\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":true},{\"addressType\":\"hapiv1SoldToIndividual\",\"dataSource\":\"hapi\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":true},{\"addressType\":\"orgAddress\",\"dataSource\":\"jarvisOrgAddress\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":true},{\"addressType\":\"hapiserviceusageaddress\",\"dataSource\":\"hapi\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":true}]}}},\"validateinstance\":{\"template\":\"defaulttemplate\"}}";
                url = url + $"&scenario={sceanrio}&avsSuggest=true";
            }

            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");
            foreach (PIDLResource profilePidl in pidls)
            {
                DisplayHint saveButtonDisplayHint = profilePidl.GetDisplayHintById(buttonDisplayId);
                Assert.IsNotNull(saveButtonDisplayHint);

                // If it has next action then next action is expected to be jarvis cm when feature enabled
                var contextObj = saveButtonDisplayHint.Action.NextAction != null ? saveButtonDisplayHint.Action.NextAction.Context : saveButtonDisplayHint.Action.Context;
                var context = JsonConvert.DeserializeObject<RestLink>(JsonConvert.SerializeObject(contextObj));

                if (jarvisEnableFeature || ((string.Equals(type, "billing") || string.Equals(type, "shipping")) && addressExEnableFeature == false))
                {
                    Assert.IsNotNull(context);
                    Assert.AreEqual("https://{jarvis-endpoint}/JarvisCM/{userId}/addresses", context.Href, ignoreCase: true, "Submit action url is not as expected");
                    Assert.IsTrue(contextObj.ToString().Contains("api-version") && contextObj.ToString().Contains("x-ms-correlation-id") && contextObj.ToString().Contains("x-ms-tracking-id"), "Three headers expected for jarvis cm submit action");
                    Assert.AreEqual("POST", context.Method);
                }
                else if (addressExEnableFeature)
                {
                    Assert.IsNotNull(context);

                    if (!string.IsNullOrEmpty(sceanrio))
                    {
                        Assert.AreEqual($"https://{{pifd-endpoint}}/users/{{userId}}/addressesEx?partner={partner}&language=en-US&avsSuggest=True&scenario={sceanrio}", context.Href, ignoreCase: true, "Submit action url is not as expected");
                    }
                    else
                    {
                        Assert.AreEqual($"https://{{pifd-endpoint}}/users/{{userId}}/addressesEx?partner={partner}&language=en-US&avsSuggest=true", context.Href, ignoreCase: true, "Submit action url is not as expected");
                    }

                    Assert.IsTrue(contextObj.ToString().Contains("api-version") && contextObj.ToString().Contains("x-ms-correlation-id") && contextObj.ToString().Contains("x-ms-tracking-id"), "Three headers expected for jarvis cm submit action");
                    Assert.AreEqual("POST", context.Method);
                }
                else
                {
                    Assert.IsFalse(contextObj?.ToString()?.Contains("JarvisCM"));
                }
            }

            PXSettings.PartnerSettingsService.ResetToDefaults();
        }

        [DataRow("hapiv1SoldToIndividual", "add", "commercialstores", "us")]
        [DataRow("hapiv1SoldToIndividual", "add", "azure", "jp")]
        [DataTestMethod]
        public async Task TestPartnerMigration_PartnerSettingsService(string type, string operation, string partner, string country)
        {
            var pssPidls = new List<PIDLResource>();
            var pidls = new List<PIDLResource>();
            Dictionary<string, string> testHeader = null;

            testHeader = new Dictionary<string, string>()
            {
                { "x-ms-flight", "PXUsePartnerSettingsService" },
            };

            string expectedPSSResponse = $"{{\"default\":{{\"template\":\"{partner}\"}}}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            string url = $"/v7.0/Account001/AddressDescriptions?country={country}&language=en-US&type={type}&partner={partner}&operation={operation}";

            pssPidls = await GetPidlFromPXService(
                url,
                HttpStatusCode.OK,
                null,
                testHeader);

            pidls = await GetPidlFromPXService(
                url,
                HttpStatusCode.OK);

            // Assert
            Assert.IsNotNull(pssPidls, "Pidl using PSS is expected to be not null");
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");
            Assert.IsTrue(pssPidls.Count == pidls.Count, "Pidl count is not expected");
            for (int i = 0; i < pssPidls.Count; i++)
            {
                Assert.IsTrue(pidls[i].Identity.Count == pssPidls[i].Identity.Count && !pidls[i].Identity.Except(pssPidls[i].Identity).Any(), "Pidl should not be different");
            }
        }

        /// <summary>
        /// This CIT validates that the display content of the cancel button changes from "Cancel" to "Back" when the feature `setCancelButtonDisplayContentAsBack` is enabled.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="operation"></param>
        /// <param name="expectedTextOnButton"></param>
        /// <param name="featureStatus"></param>
        /// <returns></returns>
        [DataRow("officesmb", "billing", "add", "Back", true, false)]
        [DataRow("officesmb", "billing", "add", "Cancel", false, false)]
        [DataRow("officesmb", "billing", "update", "Back", true, false)]
        [DataRow("officesmb", "billing", "update", "Cancel", false, false)]
        [DataRow("officesmb", "shipping", "add", "Back", true, false)]
        [DataRow("officesmb", "shipping", "add", "Cancel", false, false)]
        [DataRow("officesmb", "shipping", "update", "Back", true, false)]
        [DataRow("officesmb", "shipping", "update", "Cancel", false, false)]
        [DataRow("officesmb", "billingGroup", "add", "Back", true, false)]
        [DataRow("officesmb", "billingGroup", "add", "Cancel", false, false)]
        [DataRow("officesmb", "billingGroup", "update", "Back", true, false)]
        [DataRow("officesmb", "billingGroup", "update", "Cancel", false, false)]
        [DataRow("cart", "billing", "add", "Back", false, true)]
        [DataRow("cart", "billing", "add", "Cancel", false, false)]
        [DataRow("cart", "billing", "update", "Back", false, true)]
        [DataRow("cart", "billing", "update", "Cancel", false, false)]
        [DataRow("cart", "shipping", "add", "Back", false, true)]
        [DataRow("cart", "shipping", "add", "Cancel", false, false)]
        [DataRow("cart", "shipping", "update", "Back", false, true)]
        [DataRow("cart", "shipping", "update", "Cancel", false, false)]
        [DataRow("azure", "billingGroup", "add", "Back", false, true)]
        [DataRow("azure", "billingGroup", "add", "Cancel", false, false)]
        [DataRow("azure", "billingGroup", "update", "Back", false, true)]
        [DataRow("azure", "billingGroup", "update", "Cancel", false, false)]
        [DataTestMethod]
        public async Task GetAddressDescription_SetCancelButtonDisplayContentAsBack_UsingPSS(string partner, string type, string operation, string expectedTextOnButton, bool featureStatus, bool flightingFeature)
        {
            // Arrange
            string flightName = null, partnerSettingResponse = null;
            Dictionary<string, string> testHeader = new Dictionary<string, string>()
            {
                { "x-ms-flight", "PXDisablePSSCache" },
            };

            if (featureStatus == true)
            {
                partnerSettingResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"shipping\",\"setCancelButtonDisplayContentAsBack\":true},{\"addressType\":\"billing\",\"setCancelButtonDisplayContentAsBack\":true},{\"addressType\":\"billinggroup\",\"setCancelButtonDisplayContentAsBack\":true},{\"addressType\":\"shipping_v3\",\"setCancelButtonDisplayContentAsBack\":true}]}}},\"update\":{\"template\":\"defaulttemplate\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"shipping\",\"setCancelButtonDisplayContentAsBack\":true},{\"addressType\":\"billing\",\"setCancelButtonDisplayContentAsBack\":true},{\"addressType\":\"billinggroup\",\"setCancelButtonDisplayContentAsBack\":true},{\"addressType\":\"shipping_v3\",\"setCancelButtonDisplayContentAsBack\":true}]}}}}";
            }
            else if (featureStatus == false && string.Equals(partner, Constants.VirtualPartnerNames.OfficeSmb, StringComparison.OrdinalIgnoreCase))
            {
                partnerSettingResponse = "{\"add\":{\"template\":\"defaulttemplate\"},\"update\":{\"template\":\"defaulttemplate\"}}";
            }

            PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);

            if (flightingFeature == true)
            {
                flightName = "PXEnableSetCancelButtonDisplayContentAsBack";
            }

            string url = $"/v7.0/Account001/AddressDescriptions?country=us&language=en-US&type={type}&partner={partner}&operation={operation}";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(
                                        url,
                                        HttpStatusCode.OK,
                                        flightNames: flightName,
                                       additionaHeaders: testHeader);

            // Assert
            Assert.IsNotNull(pidls, "Pidl using PSS is expected to be not null");
            
            foreach (PIDLResource pidl in pidls)
            {
                DisplayHint cancelButtonDisplayHint = pidl.GetDisplayHintById(GlobalConstants.DisplayHints.CancelButton) as ButtonDisplayHint;
                Assert.IsNotNull(cancelButtonDisplayHint, "Display hint, " + GlobalConstants.DisplayHints.CancelButton + " should not be null");
                Assert.AreEqual(expectedTextOnButton, cancelButtonDisplayHint.DisplayText());
                Assert.AreEqual(expectedTextOnButton, cancelButtonDisplayHint.DisplayTags["accessibilityName"]);
            }

            PXFlightHandler.ResetToDefault();
            PXSettings.PartnerSettingsService.ResetToDefaults();
        }

        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, null, "paynow")]
        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, null, "")]
        [DataRow("setupoffice", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, null, "")]
        [DataRow("setupofficesdx", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, null, "")]
        [DataRow("xbox", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, null, "")]
        [DataRow("webblends", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, null, "")]
        [DataRow("oxowebdirect", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, null, "")]
        [DataRow("cart", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, null, "")]
        [DataRow("webblends_inline", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, null, "")]
        [DataRow("officeoobe", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, null, "")]
        [DataRow("oxooobe", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, null, "")]
        [DataRow("smboobe", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, null, "")]
        [DataRow("storeoffice", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, null, "")]
        [DataRow("consumersupport", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, null, "")]
        [DataRow("xboxweb", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, null, "")]
        [DataRow("windowssettings", AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions, null, "")]
        [TestMethod]
        public async Task GetValidateAddressDescription_MultipleSuggestions(string partner, string addressId, string flightingOverrides, string scenario)
        {
            string accountId = "Account001";

            // Arrange
            string url = $"/v7.0/{accountId}/AddressDescriptions?country=us&language=en-US&partner={partner}&operation=validateInstance&type=px_v3&addressId={addressId}";

            SetupSuggestAddressPayload(PXSettings.AccountsService, PXSettings.AddressEnrichmentService, accountId, addressId);

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXServiceWithFlightOverrides(url, flightingOverrides);

            PXSettings.AccountsService.ResetToDefaults();
            PXSettings.AddressEnrichmentService.ResetToDefaults();

            // Assert
            Assert.AreEqual(4, pidls.Count);

            foreach (PIDLResource pidl in pidls)
            {
                if (partner == "windowssettings")
                {
                    Assert.AreEqual("submit", pidl.GetDisplayHintById("addressUseButton").Action.ActionType.ToString());
                }
                else
                {
                    Assert.AreEqual("restAction", pidl.GetDisplayHintById("addressUseButton").Action.ActionType.ToString());
                }
            }
        }

        /// <summary>
        /// This test is used to verify the address validation feature for the submit link.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="buttonDisplayId"></param>
        /// <returns></returns>
        [DataRow("billing", "saveButton", "add")]
        [DataRow("billing", "saveButton", "update")]
        [DataRow("shipping", "saveButton", "add")]
        [DataRow("shipping", "saveButton", "update")]
        [DataRow("orgAddress", "validateButtonHidden", "add")]
        [DataRow("orgAddress", "validateButtonHidden", "update")]
        [DataRow("px_v3_billing", "saveButton", "add")]
        [TestMethod]
        public async Task GetValidateAddressDescription_SubmitLinkValidation(string type, string buttonDisplayId, string operation)
        {
            // Arrange
            Dictionary<string, string> headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };

            string partnerSettingResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"billing\",\"dataSource\":\"jarvisBilling\",\"submitActionType\":\"legacyValidate\",\"SubmitHeaderType\":null},{\"addressType\":\"shipping\",\"dataSource\":\"jarvisShippingV3\",\"submitActionType\":\"legacyValidate\",\"SubmitHeaderType\":null},{\"addressType\":\"px_v3_billing\",\"dataSource\":\"jarvisBilling\",\"submitActionType\":\"legacyValidate\",\"SubmitHeaderType\":null},{\"addressType\":\"orgAddress\",\"dataSource\":\"jarvisOrgAddress\",\"submitActionType\":\"legacyValidate\",\"SubmitHeaderType\":null}]}}},\"update\":{\"template\":\"defaulttemplate\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"billing\",\"dataSource\":\"jarvisBilling\",\"submitActionType\":\"legacyValidate\",\"SubmitHeaderType\":null},{\"addressType\":\"shipping\",\"dataSource\":\"jarvisShippingV3\",\"submitActionType\":\"legacyValidate\",\"SubmitHeaderType\":null},{\"addressType\":\"orgAddress\",\"dataSource\":\"jarvisOrgAddress\",\"submitActionType\":\"legacyValidate\",\"SubmitHeaderType\":null}]}}}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);

            string accountId = "Account001";

            string url = $"/v7.0/{accountId}/AddressDescriptions?country=us&language=en-US&partner=officesmb&operation={operation}&type={type}";
            url = type == "orgAddress" ? url + "&scenario=modernAccount" : url;

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls[0].DisplayPages.Count, "Pidls Display Pages should not be null");
            var buttonDisplayHint = pidls[0].GetDisplayHintById(buttonDisplayId);
            Assert.IsNotNull(buttonDisplayHint);
            Assert.IsNotNull(buttonDisplayHint.Action);
            Assert.IsNotNull(buttonDisplayHint.Action.Context);
            Assert.IsTrue(buttonDisplayHint.Action.Context.ToString().Contains("legacyValidate"));

            PXSettings.PartnerSettingsService.ResetToDefaults();
        }

        /// <summary>
        /// This test is used to verify the scenario when display page is null
        /// </summary>
        /// <param name="type"></param>
        /// <param name="buttonDisplayId"></param>
        /// <returns></returns>
        [DataRow("shipping", "saveButton", "add")]
        [DataRow("shipping", "saveButton", "update")]
        [TestMethod]
        public async Task GetValidateAddressDescription_DisplayPageValidation(string type, string buttonDisplayId, string operation)
        {
            // Arrange
            Dictionary<string, string> headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache, PXUsePartnerSettingsService" } };

            string partnerSettingResponse = "{\"default\":{\"template\":\"oxooobe\",\"resources\":null,\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"shipping\",\"dataSource\":\"jarvisShippingV3\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":\"true\"},{\"addressType\":\"billing\",\"dataSource\":\"jarvisBilling\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":\"true\"}]}}}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);

            string accountId = "Account001";

            string url = $"/v7.0/{accountId}/AddressDescriptions?country=us&language=en-US&partner=oxooobe&operation={operation}&type={type}&avsSuggest=true";

            // Act
            await GetRequest(
                url,
                headers,
                null,
                (responseCode, responseBody, responseHeaders) =>
                {
                    // Assert
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                    List<PIDLResource> pidls = ReadPidlResourceFromJson(responseBody);
                    foreach (PIDLResource pidl in pidls)
                    {
                        Assert.IsNull(pidl.DisplayPages);
                    }
                });
        }

        [TestMethod]
        public async Task GetValidateAddressDescription_MultipleSuggestions_GetsExpectedStylehints_FromVerifyAddressStylingFeature()
        {
            string accountId = "Account001";
            string partner = "windowsstore";
            string addressId = AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions;

            // Arrange
            string url = $"/v7.0/{accountId}/AddressDescriptions?country=us&language=en-US&partner={partner}&operation=ValidateInstance&type=billing&addressId={addressId}";
            
            string expectedPSSResponse = "{\"validateInstance\":{\"template\":\"default\",\"features\":{\"verifyAddressStyling\":{\"applicableMarkets\":[]}}}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            SetupSuggestAddressPayload(PXSettings.AccountsService, PXSettings.AddressEnrichmentService, accountId, addressId);

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", "PXUsePartnerSettingsService,PXDisablePSSCache"
                }
            };

            var radioStyles = "radio-container-align-items-flex-start, radio-layout-column, radio-location-before-label, radio-label-container-marginHorizontal-none";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

            PXSettings.AccountsService.ResetToDefaults();
            PXSettings.AddressEnrichmentService.ResetToDefaults();

            // Assert
            Assert.AreEqual(4, pidls.Count, "pidls count");

            foreach (PIDLResource pidl in pidls)
            {
                Assert.AreEqual(2, pidl.DisplayPages.Count, "pages count");
                
                var addressEnteredRadio = pidl.GetDisplayHintById("addressEntered") as PropertyDisplayHint;
                Assert.IsNotNull(addressEnteredRadio, "addressEnteredRadio");
                Assert.AreEqual(radioStyles, string.Join(", ", addressEnteredRadio.StyleHints ?? new List<string>()), "addressEntered radio style hints");

                var addressSuggestedRadio = pidl.GetDisplayHintById("addressSuggested") as PropertyDisplayHint;
                Assert.IsNotNull(addressSuggestedRadio, "addressSuggestedRadio");
                Assert.AreEqual(radioStyles, string.Join(", ", addressSuggestedRadio.StyleHints ?? new List<string>()), "addressSuggested radio style hints");
                
                var addressEnteredRadioOptions = addressEnteredRadio.PossibleOptions;
                Assert.IsNotNull(addressEnteredRadioOptions, "addressEnteredRadioOptions");
                foreach (var addressEnteredRadioOption in addressEnteredRadioOptions)
                {
                    Assert.AreEqual("margin-start-small", string.Join(", ", addressEnteredRadioOption.Value?.DisplayContent?.StyleHints ?? new List<string>()), "addressEntered radio options style hints");
                }

                var addressSuggestedRadioOptions = addressSuggestedRadio.PossibleOptions;
                Assert.IsNotNull(addressSuggestedRadioOptions, "addressSuggestedRadioOptions");
                foreach (var addressSuggestedRadioOption in addressSuggestedRadioOptions)
                {
                    Assert.AreEqual("margin-start-small", string.Join(", ", addressSuggestedRadioOption.Value?.DisplayContent?.StyleHints ?? new List<string>()), "addressSuggested radio options style hints");
                }
            }
        }

        [DataRow("windowsstore")]
        [DataRow("officesmb")]
        [TestMethod]
        public async Task GetValidateAddressDescription_MultipleSuggestions_GetsExpectedLayout_FromAddressValidationFeature(string partner)
        {
            string accountId = "Account001";
            string expectedPSSResponse;
            string addressId = AddressTestsUtil.TestSuggestedAddressType.MultipleSuggestions;

            // Arrange
            string url = $"/v7.0/{accountId}/AddressDescriptions?country=us&language=en-US&partner={partner}&operation=ValidateInstance&type=billing&addressId={addressId}";

            if (partner == "officesmb")
            {
                // Feature `AddressSuggestion is enabled for officesmb partner.
                expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"shipping\",\"dataSource\":\"jarvisShippingV3\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":true},{\"addressType\":\"billing\",\"dataSource\":\"jarvisBilling\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":true},{\"addressType\":\"hapiv1SoldToIndividual\",\"dataSource\":\"hapi\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":false},{\"addressType\":\"orgAddress\",\"dataSource\":\"jarvisOrgAddress\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":false},{\"addressType\":\"hapiserviceusageaddress\",\"dataSource\":\"hapi\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":false}]}}},\"update\":{\"template\":\"defaulttemplate\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"shipping\",\"dataSource\":\"jarvisShippingV3\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":true},{\"addressType\":\"billing\",\"dataSource\":\"jarvisBilling\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":true},{\"addressType\":\"hapiv1SoldToIndividual\",\"dataSource\":\"hapi\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":false},{\"addressType\":\"orgAddress\",\"dataSource\":\"jarvisOrgAddress\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":false},{\"addressType\":\"hapiserviceusageaddress\",\"dataSource\":\"hapi\",\"submitActionType\":\"addressEx\",\"addressSuggestion\":false}]}}},\"validateinstance\":{\"template\":\"defaulttemplate\"}}";
            }
            else
            {
                expectedPSSResponse = "{\"validateInstance\":{\"template\":\"default\",\"features\":{\"addressValidation\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"verifyAddressPidlModification\": true}]}}}}";
            }
            
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            SetupSuggestAddressPayload(PXSettings.AccountsService, PXSettings.AddressEnrichmentService, accountId, addressId);

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", "PXUsePartnerSettingsService,PXDisablePSSCache"
                }
            };

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

            PXSettings.AccountsService.ResetToDefaults();
            PXSettings.AddressEnrichmentService.ResetToDefaults();

            // Assert
            Assert.AreEqual(4, pidls.Count, "pidls count");

            foreach (PIDLResource pidl in pidls)
            {
                Assert.AreEqual(2, pidl.DisplayPages.Count, "pages count");

                if (partner == "windowsstore")
                {
                    var privacyNotice = pidl.GetDisplayHintById("microsoftPrivacyTextGroup") as GroupDisplayHint;
                    Assert.IsNotNull(privacyNotice, "privacyNotice");
                    Assert.AreEqual("inline", privacyNotice.LayoutOrientation);
                }

                var addressOptionGroup = pidl.GetDisplayHintById("addressOptionsGroup") as GroupDisplayHint;
                Assert.IsNotNull(addressOptionGroup, "addressOptionGroup");
                Assert.IsNull(addressOptionGroup.LayoutOrientation, "addressOptionGroup.LayoutOrientation");
            }
        }

        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, 1)]
        [DataRow("setupoffice", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, 1)]
        [DataRow("setupofficesdx", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, 1)]
        [DataRow("xbox", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 1, null, 1)]
        [DataRow("webblends", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, 1)]
        [DataRow("oxowebdirect", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, 1)]
        [DataRow("cart", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, 1)]
        [DataRow("payin", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, 1)]
        [DataRow("webblends_inline", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, 1)]
        [DataRow("officeoobe", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, 1)]
        [DataRow("oxooobe", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, 1)]
        [DataRow("mseg", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, 1)]
        [DataRow("onedrive", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, 1)]
        [DataRow("smboobe", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, 1)]
        [DataRow("storeoffice", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, 1)]
        [DataRow("consumersupport", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, 1)]
        [DataRow("xboxweb", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, 1)] 
        [TestMethod]
        public async Task GetValidateAddressDescription_SingleSuggestions(string partner, string addressId, int expectedPageCount, string flightingOverrides, int buttonsOnPage)
        {
            string accountId = "Account001";

            // Arrange
            string url = $"/v7.0/{accountId}/AddressDescriptions?country=us&language=en-US&partner={partner}&operation=validateInstance&type=px_v3&addressId={addressId}";

            SetupSuggestAddressPayload(PXSettings.AccountsService, PXSettings.AddressEnrichmentService, accountId, addressId);

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXServiceWithFlightOverrides(url, flightingOverrides);
            if (partner == "xbox")
            {
                VerifySuggestedAddressPidlUsingButtonList(pidls);
            }
            else
            {
                VerifySuggestedAddressPidl(pidls, partner, expectedPageCount, null, buttonsOnPage);
            }

            PXSettings.AccountsService.ResetToDefaults();
            PXSettings.AddressEnrichmentService.ResetToDefaults();
        }

        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, "paynow", 1)]
        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, "", 1)]
        [DataRow("setupoffice", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, "", 1)]
        [DataRow("setupofficesdx", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, "", 1)]
        [DataRow("cart", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, "", 1)]
        [DataRow("xboxweb", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, "", 1)] 
        [TestMethod]
        public async Task GetValidateAddressDescription_SingleSuggestions_AddressesExHelper(
            string partner,
            string addressId,
            int expectedPageCount,
            string flightingOverrides,
            string scenario,
            int buttonsOnPage)
        {
            string accountId = "Account001";

            // Arrange
            string url = $"/v7.0/{accountId}/AddressDescriptions?country=us&language=en-US&partner={partner}&operation=validateInstance&type=px_v3&addressId={addressId}";
            url = AppendParameterScenario(url, scenario);

            SetupSuggestAddressPayload(PXSettings.AccountsService, PXSettings.AddressEnrichmentService, accountId, addressId);

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXServiceWithFlightOverrides(url, flightingOverrides);
            if (partner == "xbox")
            {
                VerifySuggestedAddressPidlUsingButtonList(pidls);
            }
            else
            {
                VerifySuggestedAddressPidl(pidls, partner, expectedPageCount, scenario, buttonsOnPage);
            }

            PXSettings.AccountsService.ResetToDefaults();
            PXSettings.AddressEnrichmentService.ResetToDefaults();
        }

        [DataRow("webblends", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, null)]
        [DataRow("oxowebdirect", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, null)]
        [DataRow("cart", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, null)]
        [DataRow("payin", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, null)]
        [DataRow("webblends_inline", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, null)]
        [DataRow("officeoobe", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, null)]
        [DataRow("oxooobe", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, null)]
        [DataRow("mseg", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, null)]
        [DataRow("onedrive", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, null)]
        [DataRow("smboobe", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, null)]
        [DataRow("storeoffice", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, null)]
        [DataRow("consumersupport", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, null)]
        [DataRow("xboxweb", AddressTestsUtil.TestSuggestedAddressType.VerifiedShippable, null)] 
        public async Task GetValidateAddressDescription_Verified_SetAsDefaultBilling(string partner, string addressId, string flightingOverrides)
        {
            string accountId = "Account001";

            // Arrange
            string url = $"/v7.0/{accountId}/AddressDescriptions?country=us&language=en-US&partner={partner}&operation=validateInstance&type=jarvis_v3&addressId={addressId}&setAsDefaultBilling=true";

            SetupSuggestAddressPayload(PXSettings.AccountsService, PXSettings.AddressEnrichmentService, accountId, addressId);

            // Act`
            List<PIDLResource> pidls = await GetPidlFromPXServiceWithFlightOverrides(url, flightingOverrides);

            Assert.IsNotNull(pidls);
            Assert.AreEqual(1, pidls.Count);
            var pidl = pidls.SingleOrDefault().ClientAction.Context;
            JObject json = JObject.Parse(Convert.ToString(pidl));
            Assert.AreEqual(true, json["set_as_default_billing_address"]);
            Assert.AreEqual(false, json["set_as_default_shipping_address"]);

            PXSettings.AccountsService.ResetToDefaults();
            PXSettings.AddressEnrichmentService.ResetToDefaults();
        }

        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, "paynow")]
        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, "")]
        [DataRow("setupoffice", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, "")]
        [DataRow("setupofficesdx", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, "")]
        [DataRow("xbox", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 1, null, "")]
        [DataRow("webblends", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, "")]
        [DataRow("oxowebdirect", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, "")]
        [DataRow("cart", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, "")]
        [DataRow("webblends_inline", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, "")]
        [DataRow("officeoobe", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, "")]
        [DataRow("oxooobe", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, "")]
        [DataRow("smboobe", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, "")]
        [DataRow("storeoffice", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, "")]
        [DataRow("consumersupport", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, "")]
        [DataRow("xboxweb", AddressTestsUtil.TestSuggestedAddressType.SingleSuggestion, 2, null, "")] 
        [TestMethod]
        public async Task GetValidateAddressDescription_SingleSuggestions_PatchAddress(string partner, string addressId, int expectedPageCount, string flightingOverrides, string scenario)
        {
            string accountId = "Account001";

            // Arrange
            string url = $"/v7.0/{accountId}/AddressDescriptions?country=us&language=en-US&partner={partner}&operation=validateInstance&type=jarvis_v3&addressId={addressId}";
            url = AppendParameterScenario(url, scenario);

            AddressTestsUtil.SetupSuggestAddressPayload(PXSettings.AccountsService, PXSettings.AddressEnrichmentService, accountId, addressId);

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXServiceWithFlightOverrides(url, flightingOverrides);
            if (partner == "xbox")
            {
                VerifySuggestedAddressPidlUsingButtonList(pidls);
            }
            else
            {
                VerifySuggestedAddressPidl(pidls, partner, expectedPageCount, scenario);
                DisplayHintAction action = VerifyUserEnteredAddressPidl(pidls, partner, expectedPageCount, flightingOverrides, Addresses[addressId].id, scenario: scenario);
                dynamic link = action.Context;
                var additionalHeaders = new Dictionary<string, string>()
                {
                    { Constants.CustomHeaders.IfMatch, link.headers[Constants.CustomHeaders.IfMatch].ToString() }
                };
                dynamic response = await SendRequestPXServiceWithFlightOverrides(link.href.ToString().Replace(Constants.SubmitUrls.PifdBaseUrl, $"/v7.0/{accountId}"), new HttpMethod(link.method.ToString()), link.payload, additionalHeaders, flightingOverrides);
                Assert.AreEqual("ReturnContext", response.clientAction.type.ToString());
                Assert.AreEqual(addressId, response.clientAction.context.id.ToString());
            }

            PXSettings.AccountsService.ResetToDefaults();
            PXSettings.AddressEnrichmentService.ResetToDefaults();
        }

        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.None, 2, null, "paynow")]
        [DataRow("amcweb", AddressTestsUtil.TestSuggestedAddressType.None, 2, null, "")]
        [DataRow("setupoffice", AddressTestsUtil.TestSuggestedAddressType.None, 2, null, "")]
        [DataRow("setupofficesdx", AddressTestsUtil.TestSuggestedAddressType.None, 2, null, "")]
        [DataRow("xbox", AddressTestsUtil.TestSuggestedAddressType.None, 2, null, "")]
        [DataRow("webblends", AddressTestsUtil.TestSuggestedAddressType.None, 2, null, "")]
        [DataRow("oxowebdirect", AddressTestsUtil.TestSuggestedAddressType.None, 2, null, "")]
        [DataRow("cart", AddressTestsUtil.TestSuggestedAddressType.None, 2, null, "")]
        [DataRow("webblends_inline", AddressTestsUtil.TestSuggestedAddressType.None, 2, null, "")]
        [DataRow("officeoobe", AddressTestsUtil.TestSuggestedAddressType.None, 2, null, "")]
        [DataRow("oxooobe", AddressTestsUtil.TestSuggestedAddressType.None, 2, null, "")]
        [DataRow("smboobe", AddressTestsUtil.TestSuggestedAddressType.None, 2, null, "")]
        [DataRow("storeoffice", AddressTestsUtil.TestSuggestedAddressType.None, 2, null, "")]
        [DataRow("consumersupport", AddressTestsUtil.TestSuggestedAddressType.None, 2, null, "")]
        [DataRow("xboxweb", AddressTestsUtil.TestSuggestedAddressType.None, 2, null, "")] 
        [TestMethod]
        public async Task GetValidateAddressDescription_None_PatchAddress(string partner, string addressId, int expectedPageCount, string flightingOverrides, string scenario)
        {
            string accountId = "Account001";

            // Arrange
            string url = $"/v7.0/{accountId}/AddressDescriptions?country=us&language=en-US&partner={partner}&operation=validateInstance&type=px_v3&addressId={addressId}";
            url = AppendParameterScenario(url, scenario);

            AddressTestsUtil.SetupSuggestAddressPayload(PXSettings.AccountsService, PXSettings.AddressEnrichmentService, accountId, addressId);

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXServiceWithFlightOverrides(url, flightingOverrides);

            // Assert
            Assert.AreEqual(1, pidls.Count);
            PIDLResource resource = pidls[0];
            if (expectedPageCount != 0)
            {
                Assert.AreEqual(expectedPageCount, resource.DisplayPages.Count);
                VerifyUserEnteredAddressOnlyPage(resource.DisplayPages[0], addressId, partner, flightingOverrides, scenario);
                VerifyAddressChangePage(resource.DisplayPages[1], partner, scenario);
            }
            else
            {
                Assert.IsNull(resource.DisplayPages);
            }

            PXSettings.AccountsService.ResetToDefaults();
            PXSettings.AddressEnrichmentService.ResetToDefaults();
        }

        [DataRow("storify", AddressTestsUtil.TestListAddressType.MultipleAddresses)]
        [DataRow("xboxsubs", AddressTestsUtil.TestListAddressType.MultipleAddresses)]
        [DataRow("xboxsettings", AddressTestsUtil.TestListAddressType.MultipleAddresses)]
        [DataRow("saturn", AddressTestsUtil.TestListAddressType.MultipleAddresses)]
        [DataRow("amcweb", AddressTestsUtil.TestListAddressType.MultipleAddresses)]
        [DataRow("amc", AddressTestsUtil.TestListAddressType.MultipleAddresses)]
        [DataRow("webblends", AddressTestsUtil.TestListAddressType.MultipleAddresses)]
        [DataRow("oxowebdirect", AddressTestsUtil.TestListAddressType.MultipleAddresses)]
        [DataRow("setupoffice", AddressTestsUtil.TestListAddressType.MultipleAddresses)]
        [DataRow("setupofficesdx", AddressTestsUtil.TestListAddressType.MultipleAddresses)]
        [DataRow("xbox", AddressTestsUtil.TestListAddressType.MultipleAddresses)]
        [DataRow("cart", AddressTestsUtil.TestListAddressType.MultipleAddresses)]
        [DataRow("payin", AddressTestsUtil.TestListAddressType.MultipleAddresses)]
        [DataRow("webblends_inline", AddressTestsUtil.TestListAddressType.MultipleAddresses)]
        [DataRow("officeoobe", AddressTestsUtil.TestListAddressType.MultipleAddresses)]
        [DataRow("oxooobe", AddressTestsUtil.TestListAddressType.MultipleAddresses)]
        [DataRow("mseg", AddressTestsUtil.TestListAddressType.MultipleAddresses)]
        [DataRow("onedrive", AddressTestsUtil.TestListAddressType.MultipleAddresses)]
        [DataRow("smboobe", AddressTestsUtil.TestListAddressType.MultipleAddresses)]
        [DataRow("storeoffice", AddressTestsUtil.TestListAddressType.MultipleAddresses)]
        [DataRow("consumersupport", AddressTestsUtil.TestListAddressType.MultipleAddresses)]
        [TestMethod]
        public async Task GetAddressDescription_ListAddress(string partner, string testType)
        {
            string accountId = "Account001";

            string url = $"/v7.0/{accountId}/addressDescriptions?partner={partner}&operation=SelectInstance&language=en_US&country=US&scenario=shipping&type=jarvis_v3";

            AddressTestsUtil.SetupListAddressPayload(PXSettings.AccountsService, accountId, testType);

            // Act
            if (Microsoft.Commerce.Payments.PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner))
            {
                List<PIDLResource> pidls = await GetPidlFromPXService(url);
                PXSettings.AccountsService.ResetToDefaults();

                // Assert
                Assert.AreEqual(1, pidls.Count);
                var addressList = pidls[0].DisplayPages[0].Members[1] as PropertyDisplayHint;
                Assert.AreEqual(3, addressList.PossibleOptions.Count);
                Assert.IsTrue(string.Equals(addressList.PossibleValues.Last().Value.ToString(), "newAddressLink", StringComparison.OrdinalIgnoreCase), "Last ListAddress Option key must be: newAddressLink");
            }
            else
            {
                HttpResponseMessage response = await PXClient.GetAsync(GetPXServiceUrl(url));
                Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest, "ListAddress only supported by xbox native partners");
            }
        }

        [DataRow(Constants.PartnerNames.PlayXbox, TestListAddressType.MultipleAddresses, "")]
        [DataRow(Constants.PartnerNames.PlayXbox, TestListAddressType.MultipleAddresses, "PXRemoveJarvisHeadersFromSubmitUrl")]
        [DataRow(Constants.PartnerNames.PlayXbox, TestListAddressType.MultipleAddresses, "PXSkipPifdAddressPostForNonAddressesType")] // This flight shouldn't affect the addresses submit url as it only restricts profiles type to not use the PIFD/addressEx url.
        [DataRow(Constants.PartnerNames.PlayXbox, TestListAddressType.MultipleAddresses, "PXRemoveJarvisHeadersFromSubmitUrl,PXSkipPifdAddressPostForNonAddressesType")]
        [TestMethod]
        public async Task ManageAddress_PlayXbox(string partner, string testType, string flights)
        {
            string accountId = "Account001";
            bool removeJarvisHeaders = flights.Contains("PXRemoveJarvisHeadersFromSubmitUrl");

            AddressTestsUtil.SetupListAddressPayload(PXSettings.AccountsService, accountId, testType);

            if (removeJarvisHeaders)
            {
                PXSettings.PartnerSettingsService.ArrangeResponse(Constants.PSSMockResponses.PXPartnerSettingsPlayXbox);
            }
            else
            {
                PXSettings.PartnerSettingsService.ArrangeResponse("{\"default\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null,\"resources\":null,\"features\":null},\"add\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"QRCode\",\"features\":{\"useV3AddressPIDL\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"useAddressesExSubmit\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"addressValidation\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"verifyAddressPidlModification\":true}]},\"skipJarvisV3ForProfile\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"PXEnableVenmo\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"PxEnableSelectPMAddPIVenmo\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"showRedirectURLInIframe\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null}}},\"update\":{\"template\":\"defaulttemplate\",\"features\":{\"useV3AddressPIDL\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"useAddressesExSubmit\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"addressValidation\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"verifyAddressPidlModification\":true}]}}},\"select\":{\"template\":\"selectpmbuttonlist\",\"features\":{\"paymentMethodGrouping\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"PXSwapSelectPMPages\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"PXEnableVenmo\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"PxEnableSelectPMAddPIVenmo\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]}}},\"selectinstance\":{\"template\":\"listpibuttonlist\",\"resources\":{\"address\":{\"billing\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null},\"px_v3_billing\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null}}},\"features\":{\"addRedeemGiftCardButton\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"unhideElements\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"elementsToBeUnhidden\":[\"hiddenCancelBackButton\"]}]},\"customizeDisplayTag\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"displayTagsToBeAdded\":{\"newPaymentMethodLink\":{\"addIcon\":\"addIcon\"},\"redeemGiftCardLink\":{\"giftCardIcon\":\"giftCardIcon\",\"accessibilityName\":\"Redeem a gift card\"},\"optionUpdate_\":{\"target\":\"_self\"}}}]},\"addStyleHintsToDisplayHints\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"styleHintsToBeAdded\":{\"newPaymentMethodLink\":[\"left\"],\"redeemGiftCardLink\":[\"left\"],\"hiddenCancelBackButton\":[\"large\"]}}]},\"selectInstanceForAddress\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"useV3AddressPIDL\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"PXEnableVenmo\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"PxEnableSelectPMAddPIVenmo\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]}}}}");
            }

            // List Address
            string url = $"/v7.0/{accountId}/addressDescriptions?partner={partner}&operation=SelectInstance&language=en-US&country=US&scenario=billing&type=jarvis_v3";
            List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK, flights);
            Assert.AreEqual(1, pidls.Count);
            var addressList = pidls[0].DisplayPages[0].Members[1] as PropertyDisplayHint;
            Assert.AreEqual(3, addressList.PossibleOptions.Count);
            Assert.IsTrue(string.Equals(addressList.PossibleValues.Last().Value.ToString(), "newAddressLink", StringComparison.OrdinalIgnoreCase), "Last ListAddress Option key must be: newAddressLink");

            // Add Address
            url = $"/v7.0/{accountId}/addressDescriptions?partner={partner}&operation=Add&type=jarvis_v3&scenario=billing&language=en-US&country=US";
            pidls = await GetPidlFromPXService(url);
            Assert.AreEqual(1, pidls.Count);
            ButtonDisplayHint saveButton = pidls[0].GetDisplayHintById("saveButton") as ButtonDisplayHint;
            Assert.IsNotNull(saveButton);
            var context = saveButton.Action.Context as JObject;
            Assert.AreEqual("https://{pifd-endpoint}/anonymous/addresses/ModernValidate?type=internal&partner=playxbox&language=en-US&scenario=suggestAddressesTradeAVSUsePidlPageV2&country=US", context["href"]);
            Assert.AreEqual("submit", saveButton.Action.NextAction.ActionType);
            var nextActionContext = saveButton.Action.NextAction.Context as JObject;
            var headers = nextActionContext["headers"] as JObject;
            Assert.AreEqual("https://{pifd-endpoint}/users/{userId}/addressesEx?partner=playxbox&language=en-US&avsSuggest=false&scenario=billing", nextActionContext["href"]);
            var apiVersion = headers["api-version"];
            if (removeJarvisHeaders)
            {
                Assert.IsNull(apiVersion, "api-version is needed only when calling Jarvis");
            }
            else
            {
                Assert.IsNotNull(apiVersion, "api-version should be present when Jarvis is used");
            }

            // Update Address
            url = $"/v7.0/{accountId}/addressDescriptions?partner={partner}&operation=Update&type=jarvis_v3&scenario=billing&language=en-US&country=US&addressId=61ffea36-b7a6-417b-9b26-fd390e1c811e";
            pidls = await GetPidlFromPXService(url);
            Assert.AreEqual(1, pidls.Count);
            saveButton = pidls[0].GetDisplayHintById("saveButton") as ButtonDisplayHint;
            Assert.IsNotNull(saveButton);
            context = saveButton.Action.Context as JObject;
            Assert.AreEqual("https://{pifd-endpoint}/anonymous/addresses/ModernValidate?type=internal&partner=playxbox&language=en-US&scenario=suggestAddressesTradeAVSUsePidlPageV2&country=US", context["href"]);
            Assert.AreEqual("submit", saveButton.Action.NextAction.ActionType);
            nextActionContext = saveButton.Action.NextAction.Context as JObject;
            headers = nextActionContext["headers"] as JObject;
            Assert.AreEqual("https://{pifd-endpoint}/users/{userId}/addressesEx?partner=playxbox&language=en-US&avsSuggest=false&scenario=billing", nextActionContext["href"]);
            apiVersion = headers["api-version"];
            if (removeJarvisHeaders)
            {
                Assert.IsNull(apiVersion, "api-version is needed only when calling Jarvis");
            }
            else
            {
                Assert.IsNotNull(apiVersion, "api-version should be present when Jarvis is used");
            }

            PXSettings.AccountsService.ResetToDefaults();
            PXSettings.AddressEnrichmentService.ResetToDefaults();
            PXSettings.PartnerSettingsService.Responses.Clear();
        }

        [DataRow(Constants.PartnerNames.Storify, Constants.AddressTypes.Shipping, TestListAddressType.MultipleAddresses)]
        [DataRow(Constants.PartnerNames.Storify, Constants.AddressTypes.Billing, TestListAddressType.MultipleAddresses)]
        [DataRow(Constants.PartnerNames.XboxSettings, Constants.AddressTypes.Shipping, TestListAddressType.MultipleAddresses)]
        [DataRow(Constants.PartnerNames.XboxSettings, Constants.AddressTypes.Billing, TestListAddressType.MultipleAddresses)]
        [TestMethod]
        public async Task GetAddressDescription_XboxNativeListAddressOptionsLabels(string partner, string scenario, string testType)
        {
            string accountId = "Account001";
            string url = $"/v7.0/{accountId}/addressDescriptions?partner={partner}&operation=SelectInstance&language=en_US&country=US&scenario={scenario}&type=jarvis_v3";
            AddressTestsUtil.SetupListAddressPayload(PXSettings.AccountsService, accountId, testType);

            List<PIDLResource> pidls = await GetPidlFromPXService(url);
            PropertyDisplayHint addressList = pidls[0].DisplayPages[0].Members[1] as PropertyDisplayHint;
            int totalOptions = addressList.PossibleOptions.Count, position = 1;
            string positionFormat = " Option {0} of {1}";
            foreach (KeyValuePair<string, SelectOptionDescription> option in addressList.PossibleOptions)
            {
                if (option.Key == "newAddressLink")
                {
                    Assert.IsTrue(option.Value.AccessibilityName == string.Format("Add new {0} address", scenario.ToLower()));
                }
                else
                {
                    Assert.IsTrue(option.Value.AccessibilityName.StartsWith(string.Format(positionFormat, position++, totalOptions)));
                }

                List<DisplayHint> textDisplayHints = pidls[0].GetAllDisplayHints(option.Value.DisplayContent).Where(hint => hint is TextDisplayHint).ToList();
                textDisplayHints.ForEach(hint =>
                {
                    Assert.IsTrue(hint.DisplayTags["noPidlddc"] == "pidlddc-disable-live");
                });
            }
        }

        [DataRow("storify", "billing", false)]
        [DataRow("storify", "billing", true)]
        [DataRow("xboxsettings", "billing", false)]
        [DataRow("xboxsettings", "billing", true)]
        [DataRow("storify", "shipping", false)]
        [DataRow("storify", "shipping", true)]
        [DataRow("xboxsettings", "shipping", false)]
        [DataRow("xboxsettings", "shipping", true)]
        [TestMethod]
        public async Task GetAddressDescription_FlightHeader_UpdateAccessibilityName(string partner, string addressType, bool isFlightEnabled)
        {
            string flight = string.Empty;

            if (isFlightEnabled)
            {
                PXFlightHandler.AddToEnabledFlights("XboxUpdateAccessibilityNameWithPosition");
                flight = "XboxUpdateAccessibilityNameWithPosition";
            }

            string url = $"/v7.0/Account001/AddressDescriptions?country=us&language=en-US&type={addressType}&partner={partner}&operation=add";
            
            if (string.Equals(addressType, "shipping", StringComparison.OrdinalIgnoreCase))
            {
                url = $"/v7.0/Account001/AddressDescriptions?country=us&language=en-US&type=jarvis_v3&scenario=shipping&partner={partner}&operation=Add";
            }
            
            var pidls = await GetPidlFromPXServiceWithPartnerHeader(url, "x-ms-flight", flight, flight);

            Assert.IsNotNull(pidls, "Pidl is expected to be not null");

            foreach (var pidl in pidls)
            {
                ButtonDisplayHint saveButton = pidl.GetDisplayHintById("saveButton") as ButtonDisplayHint;
                ButtonDisplayHint backButton = pidl.GetDisplayHintById("addressPreviousBillingButton") as ButtonDisplayHint;
                ButtonDisplayHint viewTermsButton = pidl.GetDisplayHintById("viewTermsButton") as ButtonDisplayHint;
                ButtonDisplayHint nextButton = pidl.GetDisplayHintById("nextButton") as ButtonDisplayHint;
                ButtonDisplayHint shippingAddressBackButton = pidl.GetDisplayHintById("addressPage1BackButton") as ButtonDisplayHint;

                string viewTermsButtonValue = "Microsoft respects your privacy. See our privacy statement.";
                int totalButtonCount = string.Equals(addressType, "billing", StringComparison.OrdinalIgnoreCase) ? 3 : 2;

                if (isFlightEnabled)
                {
                    Assert.AreEqual(saveButton.DisplayTags["accessibilityName"], $"Save 1 of {totalButtonCount}");
                    
                    if (string.Equals(addressType, "billing", StringComparison.OrdinalIgnoreCase))
                    {
                        Assert.AreEqual(viewTermsButton.DisplayTags["accessibilityName"], $"{viewTermsButtonValue} 3 of 3");
                        Assert.AreEqual(backButton.DisplayTags["accessibilityName"], $"Back 2 of {totalButtonCount}");
                    }
                    else
                    {
                        Assert.AreEqual(nextButton.DisplayTags["accessibilityName"], $"Next 1 of {totalButtonCount}");
                        Assert.AreEqual(shippingAddressBackButton.DisplayTags["accessibilityName"], $"Back 2 of {totalButtonCount}");
                    }
                }
                else
                {
                    Assert.AreEqual(saveButton.DisplayTags["accessibilityName"], "Save");
                    
                    if (string.Equals(addressType, "billing", StringComparison.OrdinalIgnoreCase))
                    {
                        Assert.AreEqual(viewTermsButton.DisplayTags["accessibilityName"], viewTermsButtonValue);
                        Assert.AreEqual(backButton.DisplayTags["accessibilityName"], "Back");
                    }
                    else
                    {
                        Assert.AreEqual(nextButton.DisplayTags["accessibilityName"], "Next");
                        Assert.AreEqual(shippingAddressBackButton.DisplayTags["accessibilityName"], "Back");
                    }
                }
            }
        }

        [DataRow("windowsstore", "billing")]
        [TestMethod]
        public async Task GetAddressDescription_Windows_SelectInstance(string partner, string scenario)
        {
            string expectedPSSResponse = "{\"selectinstance\":{\"template\":\"listpidropdown\", \"resources\":{\"address\":{\"billing\":{\"template\":\"defaulttemplate\"}}}, \"features\":{\"addNewPaymentMethodOption\":{\"applicableMarkets\":[]},\"useListModernResource\":{\"applicableMarkets\":[]},\"includeCreditCardLogos\":{\"applicableMarkets\":[]},\"listPIForWindows\":{\"applicableMarkets\":[]},\"selectInstanceForAddress\":{\"applicableMarkets\":[]},\"listAddressForWindows\":{\"applicableMarkets\":[]}}}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
            var requestHeaders = new Dictionary<string, string>()
            {
                { "x-ms-flight", "PXUsePartnerSettingsService" }
            };
            
            string url = $"/v7.0/Account001/AddressDescriptions?type=jarvis_v3&scenario={scenario}&partner={partner}&operation=SelectInstance&language=en-US&country=US";
            await GetRequest(
                url,
                requestHeaders,
                null,
                (responseCode, responseBody, responseHeaders) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                    List<PIDLResource> pidls = ReadPidlResourceFromJson(responseBody);
                    foreach (PIDLResource pidl in pidls)
                    {
                        if (partner == "windowsstore")
                        {
                            Assert.IsTrue(pidl.DisplayPages.Count == 1);
                        }
                    }
                });
        }

        [DataRow("windowsstore", "billing")]
        [TestMethod]
        public async Task GetAddressDescriptions_Windows_Add(string partner, string scenario)
        {
            PXSettings.PartnerSettingsService.ArrangeResponse(Constants.PSSMockResponses.PXPartnerSettingsWindowsStore);
            var requestHeaders = new Dictionary<string, string>()
            {
                { "x-ms-flight", "PXUsePartnerSettingsService" }
            };

            string url = $"/v7.0/Account001/AddressDescriptions?type=jarvis_v3&scenario={scenario}&partner={partner}&operation=Add&language=en-US&country=US";
            await GetRequest(
                url,
                requestHeaders,
                null,
                (responseCode, responseBody, responseHeaders) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                    List<PIDLResource> pidls = ReadPidlResourceFromJson(responseBody);
                    foreach (PIDLResource pidl in pidls)
                    {
                        Assert.IsTrue(pidl.DisplayPages.Count == 1);
                        Assert.IsTrue(pidl.DisplayPages[0].Members[1].HintId == "starRequiredTextGroup");
                        HeadingDisplayHint heading = pidl.GetDisplayHintById("billingAddressPageHeading") as HeadingDisplayHint;
                        Assert.IsTrue(heading.DisplayContent == "Add billing address");
                    }
                });
        }

        /// <summary>
        /// Test is to validate postal code and region name for different countries
        /// </summary>
        [DataRow("NI", "14003", "1403", "Managua", "")]
        [DataRow("CN", "100006", "10000", "BJ", "Lorem ipsum")]
        [DataRow("US", "98052-8300", "9805-8300", "CA", "Lorem ipsum")]
        [DataRow("PA", "1000", "100", "Veraguas", "Lorem ipsum dolor")]
        [DataRow("RU", "987654", "98765", "Tatarstan Resp", "")]
        [DataRow("AM", "0050", "005", "Yerevan", "")]
        [DataRow("UA", "01001", "0100", "Lvivsk", "")]
        [DataRow("IN", "600018", "60001", "Telangana", "T")]
        [DataRow("AR", "X6111AFF", "X6111AF", "Cordoba", "C")]
        [DataRow("SA", "152587182", "1525871", "Ar Riyadh", "R")]
        [DataRow("KH", "12345", "1234", "Pursat", "P")]
        [DataRow("KH", "123456", "1234567", "Pursat", "P")]
        [DataRow("AU", "1234", "1234567", "Queensland", "P")]
        [DataRow("PS", "1234", "1234567", "Queensland", "P")]
        [DataRow("CL", "323-2383", "1sns3567", "Aconcagua", "A")]
        [DataTestMethod]
        public async Task GetAddressDescription_ValidateRegEx_StateAndPostalCode(string country, string validPostalCode, string invalidPostalCode, string validRegionName, string invalidRegionName)
        {
            // Arrange
            string[] operations = new string[] { Constants.OperationTypes.Add, Constants.OperationTypes.Update };
            List<string> partners = Constants.AllPartners.Concat(new[] { Constants.VirtualPartnerNames.OfficeSmb }).ToList();
            Dictionary<string, string> headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };

            string[] addressTypes = new string[]
            {
                Constants.AddressTypes.Billing,
                Constants.AddressTypes.OrgAddress,
                Constants.AddressTypes.BillingGroup,
                Constants.AddressTypes.HapiV1ShipToIndividual,
                Constants.AddressTypes.HapiV1SoldToIndividual,
                Constants.AddressTypes.HapiV1BillToIndividual,
                Constants.AddressTypes.HapiV1ShipToOrganization,
                Constants.AddressTypes.HapiV1SoldToOrganization,
                Constants.AddressTypes.HapiV1BillToOrganization
            };   

            foreach (string operation in operations)
            {
                foreach (string type in addressTypes)
                {
                    foreach (string partner in partners)
                    {
                        string postalCodePropertyName = "postal_code";

                        if (string.Equals(partner, Constants.PartnerNames.PlayXbox))
                        {
                            PXSettings.PartnerSettingsService.ArrangeResponse(Constants.PSSMockResponses.PXPartnerSettingsPlayXbox);
                        }

                        if (string.Equals(partner, Constants.VirtualPartnerNames.OfficeSmb, System.StringComparison.OrdinalIgnoreCase))
                        {
                            string partnerSettingResponse = "{\"add\":{\"template\":\"defaulttemplate\"},\"update\":{\"template\":\"defaulttemplate\"}}";
                            PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
                        }                        

                        if (!string.Equals(type, Constants.AddressTypes.Billing, System.StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(type, Constants.AddressTypes.OrgAddress, System.StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(type, Constants.AddressTypes.BillingGroup, System.StringComparison.OrdinalIgnoreCase))
                        {
                            if (!string.Equals(partner, Constants.PartnerNames.CommercialStores, System.StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(partner, Constants.PartnerNames.Azure, System.StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            postalCodePropertyName = "postalCode";
                        }

                        string url = $"/v7.0/Account001/addressDescriptions?partner={partner}&language=en-US&type={type}&operation={operation}&country={country}";

                        // Act
                        List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

                        PropertyDescription postalCodeDescription = pidls[0].GetPropertyDescriptionByPropertyName(postalCodePropertyName);
                        
                        ValidatePidlPropertyRegex(pidls[0], "region", validRegionName, true, canRegexbeEmpty: true);
                        ValidatePidlPropertyRegex(pidls[0], "region", invalidRegionName, false, canRegexbeEmpty: true);
                        
                        ValidatePidlPropertyRegex(pidls[0], postalCodePropertyName, validPostalCode, true, canRegexbeEmpty: true);
                        ValidatePidlPropertyRegex(pidls[0], postalCodePropertyName, invalidPostalCode, false, canRegexbeEmpty: true);

                        // Assert
                        Assert.IsNotNull(pidls, "Pidl is expected to be not null");

                        PropertyDisplayHint postalCodeDisplayHint = pidls[0].DisplayPages != null ? pidls[0].GetDisplayHintById("hapiV1ModernAccountV20190531Address_postalCode") as PropertyDisplayHint : null;
                        if (postalCodeDisplayHint != null && (bool)postalCodeDescription.IsOptional)
                        {
                            Assert.IsTrue(postalCodeDisplayHint.DisplayName.Contains("Zip"), "PostalCode display Name is not as expected");
                            Assert.IsTrue(postalCodeDisplayHint.DisplayTags["accessibilityName"].Contains("Zip"), "PostalCode accessibilityName tag is not as expected");
                        }

                        PXSettings.PartnerSettingsService.Responses.Clear();
                    }
                }
            }
        }

        /// <summary>
        /// This test validates the placeholder value of postal code and phone number and also validate the regex.
        /// </summary>
        /// <param name="partner">Partner name</param>
        /// <param name="addressType">Address type</param>
        /// <param name="country">Country type</param>
        /// <param name="validPostalCode">Valid postal code</param>
        /// <param name="inValidPostalCode">Invalid postal code</param>
        /// <param name="validPhoneNumber">Valid phone number</param>
        /// <param name="inValidPhoneNumber">Invalid phone number</param>
        /// <returns></returns>
        [DataRow(Constants.PartnerNames.Cart, Constants.AddressTypes.Billing, "ca", "A9A 9A9", "A9A-9A9", "+1 (999) 999-9999", "+1.123-456-78901")]
        [DataRow(Constants.PartnerNames.Cart, Constants.AddressTypes.Shipping, "ca", "A9A 9A9", "A9A-9A9", "+1 (999) 999-9999", "+2 123-456-7890")]
        [DataRow(Constants.PartnerNames.Cart, Constants.AddressTypes.Billing, "nz", "9999", "12A4", "+64 (9) 999999", "+123-456-78A9012")]
        [DataRow(Constants.PartnerNames.Cart, Constants.AddressTypes.Shipping, "nz", "9999", "12A4", "+64 (9) 999999", "+123-456-78A9012")]
        [DataRow(Constants.PartnerNames.Cart, Constants.AddressTypes.Billing, "jp", "999-9999", "12-34567", "+8109 9999 9999", "++61 123456")]
        [DataRow(Constants.PartnerNames.Cart, Constants.AddressTypes.Shipping, "jp", "999-9999", "12-34567", "+8109 9999 9999", "++61 123456")]
        [DataRow(Constants.PartnerNames.Cart, Constants.AddressTypes.Billing, "sg", "999999", "ABC-4567", "+65 8999 9999", "+ 123 456 789")]
        [DataRow(Constants.PartnerNames.Cart, Constants.AddressTypes.Shipping, "sg", "999999", "ABC-4567", "+65 8999 9999", "+ 123 456 789")]
        [DataTestMethod]
        public async Task GetAddressDescription_ValidatePlaceHolderAndRegex_PostalCodeAndPhoneNumber(string partner, string addressType, string country, string validPostalCode, string inValidPostalCode, string validPhoneNumber, string inValidPhoneNummber)
        {
            // Arrange
            List<string> operations = new List<string> { Constants.OperationTypes.Add, Constants.OperationTypes.Update };
            List<bool> isAnonymous = new List<bool> { true, false };

            foreach (string operation in operations)
            {
                foreach (bool isAnonymousStatus in isAnonymous)
                {
                    if (isAnonymousStatus && string.Equals(operation, Constants.OperationTypes.Update, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string postalCodePropertyName = "postal_code";
                    string phoneNumberPropertyName = "phone_number";

                    string baseUrl = isAnonymousStatus ? "/v7.0/" : "/v7.0/Account001/";
                    string url = baseUrl + $"addressDescriptions?partner={partner}&language=en-US&type={addressType}&operation={operation}&country={country}";

                    // Act
                    List<PIDLResource> pidls = await GetPidlFromPXService(url);

                    // Assert
                    Assert.IsNotNull(pidls, "Pidl is expected to be not null");

                    foreach (PIDLResource pidl in pidls)
                    {
                        ValidatePidlPropertyRegex(pidl, phoneNumberPropertyName, validPhoneNumber, true, canRegexbeEmpty: true);
                        ValidatePidlPropertyRegex(pidl, phoneNumberPropertyName, inValidPhoneNummber, false, canRegexbeEmpty: true);

                        ValidatePidlPropertyRegex(pidl, postalCodePropertyName, validPostalCode, true, canRegexbeEmpty: true);
                        ValidatePidlPropertyRegex(pidl, postalCodePropertyName, inValidPostalCode, false, canRegexbeEmpty: true);

                        ValidatePlaceHolderForDisplayHints(pidl, postalCodePropertyName, validPostalCode);

                        // Check if the user is not anonymous and the address type is not Billing
                        if (isAnonymousStatus || !string.Equals(addressType, Constants.AddressTypes.Billing, StringComparison.OrdinalIgnoreCase))
                        {
                            // Validate placeholder for display hints if the address type is not Billing or the user is anonymous
                            ValidatePlaceHolderForDisplayHints(pidl, phoneNumberPropertyName, validPhoneNumber);
                        }
                    }
                }
            }
        }

        [DataRow("windowssettings")]
        [DataTestMethod]
        public async Task ValidateAddressPIDL_DefaultTemplate(string partner)
        {
            Dictionary<string, string> testHeader = new Dictionary<string, string>()
            {
                { "x-ms-flight", "PXUsePartnerSettingsService" },
            };
   
            // Arrange
            string url = $"/v7.0/Account001/AddressDescriptions?country=us&language=en-US&partner={partner}&operation=validateInstance&type=px_v3&addressId={AddressTestsUtil.TestListAddressType.MultipleAddresses}";

            SetupSuggestAddressPayload(PXSettings.AccountsService, PXSettings.AddressEnrichmentService, "Account001", AddressTestsUtil.TestListAddressType.MultipleAddresses);

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK, null, testHeader);
            PXSettings.AccountsService.ResetToDefaults();

            Assert.IsNotNull(pidls);

            foreach (PIDLResource pi in pidls)
            {
                Assert.AreEqual(2, pi.DisplayPages.Count);
                PropertyDisplayHint addressSuggested = pi.GetDisplayHintById("addressSuggested") as PropertyDisplayHint;
                Assert.AreEqual("radio", addressSuggested.SelectType);
                Assert.IsNotNull(addressSuggested.PossibleOptions);
                Assert.IsNotNull(addressSuggested.PossibleOptions["MulitpleSuggestions"]);
                Assert.AreEqual(string.Empty, addressSuggested.PossibleOptions["MulitpleSuggestions"].DisplayText);
                Assert.AreEqual("The address I entered", addressSuggested.PossibleOptions["MulitpleSuggestions"].DisplayContent.Members[0].DisplayText());
                GroupDisplayHint currentAddress = pi.GetDisplayHintById("addressOptionsGroup") as GroupDisplayHint;
                Assert.IsNotNull(currentAddress.Members);
            }            
        }

        /// <summary>
        /// This test validates company name regex on Hapi address forms
        /// </summary>
        /// <param name="partner"></param>
        /// <param name="value">Value to test against regex</param>
        /// <param name="isValid">Is supplied value is valid or not</param>
        /// <returns></returns>
        //// Azure - Valid
        [DataRow(Constants.PartnerNames.Azure, "字", true)]
        [DataRow(Constants.PartnerNames.Azure, "my company 123", true)]
        [DataRow(Constants.PartnerNames.Azure, "NA P", true)]
        [DataRow(Constants.PartnerNames.Azure, "COMPANY1", true)]
        [DataRow(Constants.PartnerNames.Azure, "Company & name", true)]
        [DataRow(Constants.PartnerNames.Azure, "字%", true)]
        [DataRow(Constants.PartnerNames.Azure, "&&&*KJhukuke", true)]
        [DataRow(Constants.PartnerNames.Azure, "Test_Testsonu", true)]

        // Azure - Invalid
        [DataRow(Constants.PartnerNames.Azure, "k", false)]
        [DataRow(Constants.PartnerNames.Azure, "$#468", false)]
        [DataRow(Constants.PartnerNames.Azure, "123456", false)]
        [DataRow(Constants.PartnerNames.Azure, "1234 561111", false)]
        [DataRow(Constants.PartnerNames.Azure, "company name ", false)]
        [DataRow(Constants.PartnerNames.Azure, " company name", false)]
        [DataRow(Constants.PartnerNames.Azure, " company name ", false)]
        [DataRow(Constants.PartnerNames.Azure, "Na", false)]
        [DataRow(Constants.PartnerNames.Azure, "nO", false)]
        [DataRow(Constants.PartnerNames.Azure, "n/A", false)]
        [DataRow(Constants.PartnerNames.Azure, "N/A123", false)]
        [DataRow(Constants.PartnerNames.Azure, "NoNe", false)]
        [DataRow(Constants.PartnerNames.Azure, "more than 220length mycompanynameeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee", false)]

        // CommercialStores - Valid
        [DataRow(Constants.PartnerNames.CommercialStores, "字", true)]
        [DataRow(Constants.PartnerNames.CommercialStores, "my company 123", true)]
        [DataRow(Constants.PartnerNames.CommercialStores, "NA P", true)]
        [DataRow(Constants.PartnerNames.CommercialStores, "COMPANY1", true)]
        [DataRow(Constants.PartnerNames.CommercialStores, "Company & name", true)]
        [DataRow(Constants.PartnerNames.CommercialStores, "字%", true)]
        [DataRow(Constants.PartnerNames.CommercialStores, "&&&*KJhukuke", true)]
        [DataRow(Constants.PartnerNames.CommercialStores, "Test_Testsonu", true)]

        // CommercialStores - Invalid
        [DataRow(Constants.PartnerNames.CommercialStores, "k", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "123456", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "1234 561111", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "$#468", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "company name ", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, " company name", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, " company name ", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "Na", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "nO", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "n/A", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "N/A123", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "NoNe", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "more than 220length mycompanynameeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee", false)]
        [DataTestMethod]
        public async Task GetAddressDescription_Hapi_ValidateRegEx_CompanyName(string partner, string value, bool isValid)
        {
            // Arrange
            string[] addressTypes = new string[]
            {
                Constants.AddressTypes.HapiV1BillToIndividual,
                Constants.AddressTypes.HapiV1BillToOrganization,
                Constants.AddressTypes.HapiV1SoldToIndividual,
                Constants.AddressTypes.HapiV1SoldToOrganization,
                Constants.AddressTypes.HapiV1ShipToIndividual,
                Constants.AddressTypes.HapiV1ShipToOrganization,
            };

            foreach (string type in addressTypes)
            {
                string url = $"/v7.0/Account001/addressDescriptions?partner={partner}&language=en-US&type={type}&operation=add&country=us";

                // Act
                List<PIDLResource> pidls = await GetPidlFromPXService(url);

                // Assert
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");
                ValidatePidlPropertyRegex(pidls[0], "companyName", value, isValid);
            }
        }

        /// <summary>
        /// This test validates first name regex on Hapi address forms
        /// </summary>
        /// <param name="partner"></param>
        /// <param name="value">Value to test against regex</param>
        /// <param name="isValid">Is supplied value is valid or not</param>
        /// <returns></returns>
        //// Azure - Valid
        [DataRow(Constants.PartnerNames.Azure, "sonu", true)]
        [DataRow(Constants.PartnerNames.Azure, "adminsonu", true)]
        [DataRow(Constants.PartnerNames.Azure, "Na", true)]
        [DataRow(Constants.PartnerNames.Azure, "N/A123", true)]
        [DataRow(Constants.PartnerNames.Azure, "moonshrine lee", true)]
        [DataRow(Constants.PartnerNames.Azure, "administrator123", true)]
        [DataRow(Constants.PartnerNames.Azure, "Dev", true)]

        // Azure - Invalid
        [DataRow(Constants.PartnerNames.Azure, "a", false)]
        [DataRow(Constants.PartnerNames.Azure, "TeST", false)]
        [DataRow(Constants.PartnerNames.Azure, "123456", false)]
        [DataRow(Constants.PartnerNames.Azure, "1234 5611", false)]
        [DataRow(Constants.PartnerNames.Azure, "test", false)]
        [DataRow(Constants.PartnerNames.Azure, "DeveLOper", false)]
        [DataRow(Constants.PartnerNames.Azure, "ADMin", false)]
        [DataRow(Constants.PartnerNames.Azure, "admIN", false)]
        [DataRow(Constants.PartnerNames.Azure, "adminISTrator", false)]
        [DataRow(Constants.PartnerNames.Azure, "n/A", false)]
        [DataRow(Constants.PartnerNames.Azure, "N/a", false)]
        [DataRow(Constants.PartnerNames.Azure, " LEE", false)]
        [DataRow(Constants.PartnerNames.Azure, "moonshrine ", false)]
        [DataRow(Constants.PartnerNames.Azure, " Bob Lee ", false)]
        [DataRow(Constants.PartnerNames.Azure, "mymorethan 40length nameeeeeeeeeeeeeeeeee", false)]

        // CommercialStores - Valid
        [DataRow(Constants.PartnerNames.CommercialStores, "sonu", true)]
        [DataRow(Constants.PartnerNames.CommercialStores, "adminsonu", true)]
        [DataRow(Constants.PartnerNames.CommercialStores, "Na", true)]
        [DataRow(Constants.PartnerNames.CommercialStores, "N/A123", true)]
        [DataRow(Constants.PartnerNames.CommercialStores, "moonshrine lee", true)]
        [DataRow(Constants.PartnerNames.CommercialStores, "administrator123", true)]
        [DataRow(Constants.PartnerNames.CommercialStores, "Dev", true)]

        // CommercialStores - Invalid
        [DataRow(Constants.PartnerNames.CommercialStores, "a", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "TeST", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "123456", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "1234 5611", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "test", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "DeveLOper", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "ADMin", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "admIN", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "adminISTrator", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "n/A", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "N/a", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, " LEE", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "moonshrine ", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, " Bob Lee ", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "mymorethan 40length nameeeeeeeeeeeeeeeeee", false)]
        [DataTestMethod]
        public async Task GetAddressDescription_Hapi_ValidateRegEx_FirstName(string partner, string value, bool isValid)
        {
            // Arrange
            string[] addressTypes = new string[]
            {
                Constants.AddressTypes.HapiV1BillToIndividual,
                Constants.AddressTypes.HapiV1SoldToIndividual,
                Constants.AddressTypes.HapiV1ShipToIndividual,
            };

            foreach (string type in addressTypes)
            {
                string url = $"/v7.0/Account001/addressDescriptions?partner={partner}&language=en-US&type={type}&operation=add&country=us";

                // Act
                List<PIDLResource> pidls = await GetPidlFromPXService(url);

                // Assert
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");
                ValidatePidlPropertyRegex(pidls[0], "firstName", value, isValid);
            }
        }

        /// <summary>
        /// This test validates last name regex on Hapi address forms
        /// </summary>
        /// <param name="partner"></param>
        /// <param name="value">Value to test against regex</param>
        /// <param name="isValid">Is supplied value is valid or not</param>
        /// <returns></returns>
        //// Azure - Valid
        [DataRow(Constants.PartnerNames.Azure, "sonu", true)]
        [DataRow(Constants.PartnerNames.Azure, "adminsonu", true)]
        [DataRow(Constants.PartnerNames.Azure, "Na", true)]
        [DataRow(Constants.PartnerNames.Azure, "N/A123", true)]
        [DataRow(Constants.PartnerNames.Azure, "moonshrine lee", true)]
        [DataRow(Constants.PartnerNames.Azure, "administrator123", true)]
        [DataRow(Constants.PartnerNames.Azure, "Dev", true)]

        // Azure - Invalid
        [DataRow(Constants.PartnerNames.Azure, "a", false)]
        [DataRow(Constants.PartnerNames.Azure, "TeST", false)]
        [DataRow(Constants.PartnerNames.Azure, "123456", false)]
        [DataRow(Constants.PartnerNames.Azure, "123 45611", false)]
        [DataRow(Constants.PartnerNames.Azure, "test", false)]
        [DataRow(Constants.PartnerNames.Azure, "DeveLOper", false)]
        [DataRow(Constants.PartnerNames.Azure, "ADMin", false)]
        [DataRow(Constants.PartnerNames.Azure, "admIN", false)]
        [DataRow(Constants.PartnerNames.Azure, "adminISTrator", false)]
        [DataRow(Constants.PartnerNames.Azure, "n/A", false)]
        [DataRow(Constants.PartnerNames.Azure, "N/a", false)]
        [DataRow(Constants.PartnerNames.Azure, " LEE", false)]
        [DataRow(Constants.PartnerNames.Azure, "moonshrine ", false)]
        [DataRow(Constants.PartnerNames.Azure, " Bob Lee ", false)]
        [DataRow(Constants.PartnerNames.Azure, "mymorethan 40length nameeeeeeeeeeeeeeeeee", false)]

        // CommercialStores - Valid
        [DataRow(Constants.PartnerNames.CommercialStores, "sonu", true)]
        [DataRow(Constants.PartnerNames.CommercialStores, "adminsonu", true)]
        [DataRow(Constants.PartnerNames.CommercialStores, "Na", true)]
        [DataRow(Constants.PartnerNames.CommercialStores, "N/A123", true)]
        [DataRow(Constants.PartnerNames.CommercialStores, "moonshrine lee", true)]
        [DataRow(Constants.PartnerNames.CommercialStores, "administrator123", true)]
        [DataRow(Constants.PartnerNames.CommercialStores, "Dev", true)]

        // CommercialStores - Invalid
        [DataRow(Constants.PartnerNames.CommercialStores, "a", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "TeST", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "123456", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "1234 5611", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "test", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "DeveLOper", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "ADMin", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "admIN", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "adminISTrator", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "n/A", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "N/a", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, " LEE", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "moonshrine ", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, " Bob Lee ", false)]
        [DataRow(Constants.PartnerNames.CommercialStores, "mymorethan 40length nameeeeeeeeeeeeeeeeee", false)]
        [DataTestMethod]
        public async Task GetAddressDescription_Hapi_ValidateRegEx_LastName(string partner, string value, bool isValid)
        {
            // Arrange
            string[] addressTypes = new string[]
            {
                Constants.AddressTypes.HapiV1BillToIndividual,
                Constants.AddressTypes.HapiV1SoldToIndividual,
                Constants.AddressTypes.HapiV1ShipToIndividual,
            };

            foreach (string type in addressTypes)
            {
                string url = $"/v7.0/Account001/addressDescriptions?partner={partner}&language=en-US&type={type}&operation=add&country=us";

                // Act
                List<PIDLResource> pidls = await GetPidlFromPXService(url);

                // Assert
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");
                ValidatePidlPropertyRegex(pidls[0], "lastName", value, isValid);
            }
        }

        /// <summary>
        /// Test is validate the OrgAddress Trade AVS PIDLs of template partner using PSS
        /// </summary>
        /// <param name="partner"></param>
        /// <param name="type"></param>
        /// <param name="isAnonymous"></param>
        /// <param name="enableAddressValidation"></param>
        /// <returns></returns>
        [DataRow("officesmb", "orgAddress", false, false)]
        [DataRow("officesmb", "orgAddress", true, false)]
        [DataRow("officesmb", "orgAddress", false, true)]
        [DataRow("officesmb", "orgAddress", true, true)]
        [TestMethod]
        public async Task GetAddressDescription_ValidateOrgAddressTradeAVS_UsingPartnerSettingsService(string partner, string type, bool isAnonymous, bool enableAddressValidation)
        {
            // Arrange
            string accountId = isAnonymous ? "/" : "/Account001/";
            Dictionary<string, string> headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };
            string url = $"/v7.0{accountId}AddressDescriptions?partner={partner}&type={type}&operation=add&language=en-us&country=us";
            string addressValidation = enableAddressValidation ? ",\"features\":{\"addressValidation\":{\"applicableMarkets\":[]}}" : string.Empty;
            string partnerSettingResponse = "{\"add\":{\"template\":\"defaulttemplate\"" + addressValidation + "}}";

            PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls);            

            foreach (PIDLResource addressPidl in pidls)
            {
                Assert.AreEqual(1, addressPidl.DisplayPages.Count, "Only one DisplayPages PIDL is expected");

                DisplayHint validateButtonDisplayHint = addressPidl.GetDisplayHintById(Constants.DisplayHintIds.ValidateButtonHidden);
                Assert.IsNotNull(validateButtonDisplayHint);
                Assert.IsNotNull(validateButtonDisplayHint.Action);
                Assert.IsNotNull(validateButtonDisplayHint.Action.Context);

                if (enableAddressValidation)
                {
                    Assert.IsTrue(validateButtonDisplayHint.Action.Context.ToString().Contains("ModernValidate"));
                    Assert.IsNotNull(validateButtonDisplayHint.Action.NextAction);

                    bool containIsCustomerConsentedCheckbox = false, containIsAvsFullValidationSucceededCheckbox = false;

                    foreach (DisplayHint displayHint in addressPidl.DisplayPages[0].Members)
                    {
                        if (string.Equals(displayHint.PropertyName, Constants.CommercialZipPlusFourPropertyNames.IsUserConsented, StringComparison.OrdinalIgnoreCase))
                        {
                            containIsCustomerConsentedCheckbox = true;
                        }
                        else if (string.Equals(displayHint.PropertyName, Constants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded, StringComparison.OrdinalIgnoreCase))
                        {
                            containIsAvsFullValidationSucceededCheckbox = true;
                        }
                    }

                    Assert.IsTrue(containIsCustomerConsentedCheckbox);
                    Assert.IsTrue(containIsAvsFullValidationSucceededCheckbox);
                    Assert.IsNotNull(addressPidl.DataDescription[Constants.CommercialZipPlusFourPropertyNames.IsUserConsented]);
                    Assert.IsNotNull(addressPidl.DataDescription[Constants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded]);
                }
                else
                {
                    Assert.IsFalse(validateButtonDisplayHint.Action.Context.ToString().Contains("ModernValidate"));
                    Assert.IsNull(validateButtonDisplayHint.Action.NextAction);
                }
            }

            PXSettings.PartnerSettingsService.Responses.Clear();
        }

        /// <summary>
        /// Test is validate the fieldsToBeDisabled PSS feature
        /// </summary>
        /// <param name="addressType"></param>
        /// <returns></returns>
        [DataRow("hapiV1SoldToIndividual")]
        [DataRow("hapiV1BillToIndividual")]
        [TestMethod]
        public async Task GetAddressDescription_ValidateFieldsToBeDisabledFeature_UsingPartnerSettingsService(string addressType)
        {
            // Arrange
            List<bool> disableFeatureStatuses = new List<bool>() { false, true };
            List<bool> removeOptionalTextFeatureStatuses = new List<bool>() { false, true };
            List<string> fieldsToDisable = new List<string>() { "companyName", "phoneNumber", "country" };
            List<string> fieldsToRemoveOptional = new List<string>() { "companyName", "phoneNumber" };
            Dictionary<string, string> headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };
            string url = $"/v7.0/Account001/AddressDescriptions?partner=officesmb&type={addressType}&operation=add&language=en-us&country=us";

            foreach (bool disableFeatureStatus in disableFeatureStatuses)
            {
                foreach (bool removeOptionalTextFeatureStatus in removeOptionalTextFeatureStatuses)
                {
                    string fieldsToBeDisabled = disableFeatureStatus ? ",\"fieldsToBeDisabled\":[" + string.Join(", ", fieldsToDisable.Select(field => $"\"{field}\"")) + "]" : string.Empty;
                    string partnerSettingResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"" + addressType + "\",\"dataSource\":\"hapi\"" + fieldsToBeDisabled + ",\"removeOptionalTextFromFields\":" + removeOptionalTextFeatureStatus.ToString().ToLower() + "}]},\"addressValidation\":{\"applicableMarkets\":[]}}}}";
                    PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);

                    // Act
                    List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

                    // Assert
                    Assert.IsNotNull(pidls);

                    foreach (PIDLResource pidl in pidls)
                    {
                        foreach (string fieldDisabled in fieldsToDisable)
                        {
                            PropertyDisplayHint property = pidl.DisplayHints().First(x => string.Equals(fieldDisabled, x.PropertyName, StringComparison.OrdinalIgnoreCase) && x is PropertyDisplayHint) as PropertyDisplayHint;
                            Assert.IsNotNull(fieldDisabled);
                            Assert.IsTrue(disableFeatureStatus ? property.IsDisabled : property.IsDisabled != true);
                        }

                        foreach (string fieldRemovedOptional in fieldsToRemoveOptional)
                        {
                            PropertyDisplayHint property = pidl.DisplayHints().First(x => string.Equals(fieldRemovedOptional, x.PropertyName, StringComparison.OrdinalIgnoreCase) && x is PropertyDisplayHint) as PropertyDisplayHint;
                            Assert.IsNotNull(fieldRemovedOptional);
                            Assert.IsTrue(removeOptionalTextFeatureStatus ? !property.DisplayText().ToString().Contains("optional") : property.DisplayText().ToString().Contains("optional"));
                        }
                    }

                    PXSettings.PartnerSettingsService.Responses.Clear();
                }
            }
        }

        /// <summary>
        /// Test is to validate the fieldsToBeDisabled PSS feature
        /// </summary>
        /// <param name="addressType"></param>
        /// <param name="country"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        [DataRow("billing")]
        [DataRow("shipping", "in")]
        [DataRow("billing", "br", "pt-br")]
        [DataRow("hapiV1BillToIndividual")]
        [DataRow("hapiV1BillToIndividual", "br", "pt-br")]
        [TestMethod]
        public async Task GetAddressDescription_RemoveOptionalInlable_UsingPartnerSettingsService(string addressType, string country = "us", string language = "en-us")
        {
            // Arrange
            List<bool> featureStatuses = new List<bool>() { false, true };
            Dictionary<string, string> headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };
            string url = $"/v7.0/Account001/AddressDescriptions?partner=officesmb&type={addressType}&operation=add&language={language}&country={country}";

            List<string> optionalDisplayHintIds = new List<string>
            {
                "hapiV1ModernAccountV20190531Address_companyName",
                "hapiV1ModernAccountV20190531Address_phoneNumber",
                "hapiV1ModernAccountV20190531Address_email",
                "hapiV1ModernAccountV20190531Address_firstName",
                "hapiV1ModernAccountV20190531Address_lastName",
                "hapiV1ModernAccountV20190531Address_addressLine2",
                "addressLine2",
                "addressLine3"
            };

            foreach (bool featureStatus in featureStatuses)
            {
                string removeOptionalInLabelPssSettings = featureStatus ? "\"removeOptionalInLabel\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]}," : string.Empty;
                string partnerSettingResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"features\":{" + removeOptionalInLabelPssSettings + "\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\"},{\"addressType\":\"hapiV1BillToIndividual\",\"dataSource\":\"hapi\"}]}}}}";

                PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);

                // Act
                List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

                // Assert
                Assert.IsNotNull(pidls);

                foreach (PIDLResource pidl in pidls)
                {
                    foreach (string optionalDisplayHintId in optionalDisplayHintIds)
                    {
                        PropertyDisplayHint property = pidl.GetDisplayHintById(optionalDisplayHintId) as PropertyDisplayHint;

                        if (property != null)
                        {
                            switch (country)
                            {
                                case "us":
                                    Assert.IsTrue(featureStatus ? !property.DisplayText().ToString().ToLower().Contains("(optional)") : property.DisplayText().ToString().ToLower().Contains("(optional)"), $"{property.DisplayName} is not set with correctly");
                                    break;
                                case "br":
                                    Assert.IsTrue(featureStatus ? !property.DisplayText().ToString().ToLower().Contains("(opcional)") : property.DisplayText().ToString().ToLower().Contains("(opcional)"), $"{property.DisplayName} is not set with correctly");
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }

                PXSettings.PartnerSettingsService.Responses.Clear();
            }
        }

        [DataRow("officesmb", "us")]
        [DataRow("officesmb", "de")]
        [DataRow("officesmb", "gb")]
        [DataRow("officesmb", "ca")]
        [DataRow("northstarweb", "us")]
        [DataTestMethod]
        public async Task GetAddressDescription_RemoveDataSource(string partner, string country)
        {
            // Arrage
            string url = $"/v7.0/Account001/AddressDescriptions?type=hapiServiceUsageAddress&partner={partner}&operation=Update&country={country}&language=en-US";
            bool[] featureStatus = new bool[] { true, false };
            Dictionary<string, string> testHeader = new Dictionary<string, string>()
            {
                { "x-ms-flight", "PXDisablePSSCache,PXUsePartnerSettingsService" },
            };

            foreach (bool isFeatureEnabled in featureStatus)
            {
                string expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\"},\"update\":{\"template\":\"defaulttemplate\"}}";

                if (isFeatureEnabled)
                {
                    expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"features\":{\"removeDataSource\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"removeDataSourceResources\":[\"address\"]}]}}},\"update\":{\"template\":\"defaulttemplate\",\"features\":{\"removeDataSource\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"removeDataSourceResources\":[\"address\"]}]}}}}";
                }

                // Act
                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
                List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: testHeader);

                // Assert
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");
                if (isFeatureEnabled)
                {
                    Assert.IsTrue(pidls[0].DataSources == null, "DataSources is expected to be null");
                }
                else
                {
                    Assert.IsTrue(pidls[0].DataSources != null, "DataSources is not expected to be null");
                }

                PXSettings.PartnerSettingsService.ResetToDefaults();
            }
        }

        [DataRow("defaulttemplate")]
        [DataRow("commercialstores")]
        [DataTestMethod]
        public async Task GetAddressDescription_ValidateRegEx_PhoneNumber(string partner)
        {
            // Arrange
            string[] countries = new string[] { "us", "cn", "ca", "de", "gb" };
            string[] addressTypes = new string[]
            {
                Constants.AddressTypes.HapiV1BillToIndividual,
                Constants.AddressTypes.HapiV1BillToOrganization,
                Constants.AddressTypes.HapiV1ShipToOrganization,
                Constants.AddressTypes.HapiV1SoldToIndividual,
                Constants.AddressTypes.HapiV1SoldToOrganization,
                Constants.AddressTypes.OrgAddress,
                Constants.AddressTypes.Shipping,
            };

            foreach (string country in countries)
            {
                foreach (string type in addressTypes)
                {
                    if ((string.Equals(type, Constants.AddressTypes.HapiV1ShipToOrganization) 
                            || string.Equals(type, Constants.AddressTypes.HapiV1SoldToOrganization)
                            || string.Equals(type, Constants.AddressTypes.HapiV1BillToIndividual)
                            || string.Equals(type, Constants.AddressTypes.HapiV1BillToOrganization)) 
                        && !string.Equals(partner, "commercialstores"))
                    {
                        continue;
                    }

                    if (string.Equals(type, Constants.AddressTypes.OrgAddress) && string.Equals(partner, "commercialstores"))
                    {
                        continue;
                    }

                    string propertyName = type.Contains("hapi") || string.Equals(type, Constants.AddressTypes.OrgAddress) ? "phoneNumber" : "phone_number";

                    string url = $"/v7.0/Account001/AddressDescriptions?partner={partner}&language=en-US&type={type}&operation=add&country={country}";

                    // Act
                    List<PIDLResource> pidls = await GetPidlFromPXService(url);

                    // Assert
                    Assert.IsNotNull(pidls, "Pidl is expected to be not null");
                    
                    // Not checking if the key exists before accessing is intentional. If the country key is not,
                    // the test should fail and dictoionary should be updated with country value.
                    List<string> validPhoneNumbers = Constants.TestValidPhoneNumbersByCountry[country];
                    foreach (string validPhoneNumber in validPhoneNumbers)
                    {
                        ValidatePidlPropertyRegex(pidls[0], propertyName, validPhoneNumber, true);
                    }

                    // If the country key is not present then use the common invalid phone numbers
                    List<string> invalidPhoneNumbers = Constants.TestInvalidPhoneNumbersByCountry.ContainsKey(country) ? Constants.TestInvalidPhoneNumbersByCountry[country] : Constants.TestInvalidPhoneNumbersByCountry["common"];
                    foreach (string invalidPhoneNumber in invalidPhoneNumbers)
                    {
                        ValidatePidlPropertyRegex(pidls[0], propertyName, invalidPhoneNumber, false);
                    }
                }
            }
        }

        [DataRow(GlobalConstants.Partners.CommercialStores, "billing", "modernAccount", GlobalConstants.CountryCodes.USA, false, true, true)]
        [DataRow(GlobalConstants.Partners.CommercialStores, "billing", "modernAccount", GlobalConstants.CountryCodes.USA, true, true, true)]
        [DataRow(GlobalConstants.Partners.CommercialStores, "billing", "modernAccount", GlobalConstants.CountryCodes.China, false, false, true)]
        [DataRow(GlobalConstants.Partners.Azure, "billing", "modernAccount", GlobalConstants.CountryCodes.USA, false, false, true)]
        [DataRow(GlobalConstants.Partners.Azure, "billing", "modernAccount", GlobalConstants.CountryCodes.USA, true, false, true)]
        [DataRow(GlobalConstants.Partners.Azure, "billing", "modernAccount", GlobalConstants.CountryCodes.China, true, false, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "billing", null, GlobalConstants.CountryCodes.USA, true, true, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "billing", null, GlobalConstants.CountryCodes.USA, true, false, false)]
        [TestMethod]
        public async Task GetAddressDescription_Billing_ModernAccount(string partner, string type, string scenario, string country, bool usePSS, bool groupAddressStatePostalCode, bool removeHeading)
        {
            // Arrange
            Dictionary<string, string> headers = null;
            string url = $"/v7.0/AddressDescriptions?type={type}&language=en-us&partner={partner}&country={country}&operation=add&language=en-us";

            if (scenario != null)
            {
                url += $"&scenario={scenario}";
            }

            if (usePSS)
            {
                var settingPartner = partner.Equals(GlobalConstants.Partners.MacManage, System.StringComparison.OrdinalIgnoreCase) ? GlobalConstants.Partners.DefaultTemplate : partner;
                string partnerSettingResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"features\":{\"enableZipCodeStateGrouping\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":" + groupAddressStatePostalCode.ToString().ToLower() + "}]},\"removeElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"removeAddressFormHeading\":" + removeHeading.ToString().ToLower() + "}]}}},\"default\":{\"template\":\"" + settingPartner + "\",\"redirectionPattern\":null,\"resources\":null,\"features\":null}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);

                headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache,PXUsePartnerSettingsService" } };
            }

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls);

            foreach (PIDLResource pidl in pidls)
            {
                var groupDisplayHint = pidl.GetDisplayHintById("addressStatePostalCodeGroup") as GroupDisplayHint;
                var actualGroupDisplayHintExpected = groupDisplayHint == null ? false : true;
                Assert.AreEqual(groupAddressStatePostalCode, actualGroupDisplayHintExpected, "groupaddress state and postal code expected for commercialstores partner");
                
                if (actualGroupDisplayHintExpected && usePSS)
                {
                    Assert.IsTrue(groupDisplayHint.DisplayTags.ContainsKey("zipcode-state-group"));
                }

                var headingDisplayHint = pidl.GetDisplayHintById("billingAddressPageHeading") as HeadingDisplayHint;
                var actualHeadingDisplayHintExpected = headingDisplayHint == null ? true : false;
                Assert.AreEqual(removeHeading, actualHeadingDisplayHintExpected, "Heading not expected for commercialstores partner");
            }

            PXSettings.PartnerSettingsService.Responses.Clear();
        }

        [DataRow("macmanage", "us", "add", true)]
        [DataRow("macmanage", "us", "add", false)]
        [DataRow("macmanage", "us", "update", true)]
        [DataRow("macmanage", "us", "update", false)]
        [DataRow("macmanage", "gb", "add", true)]
        [DataRow("macmanage", "gb", "add", false)]
        [DataRow("macmanage", "gb", "update", true)]
        [DataRow("macmanage", "gb", "update", false)]
        [DataTestMethod]
        public async Task GetAddressDescription_PSS_EnableDoingBusinessAsField(string partner, string country, string operation, bool enableWithFeature)
        {
            // Arrage
            string url = $"/v7.0/Account001/AddressDescriptions?type=hapiV1SoldToOrganization&partner={partner}&operation={operation}&country={country}&language=en-US";

            string expectedPSSResponse = "{\"" + operation + "\":{\"template\":\"defaulttemplate\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"hapiV1SoldToOrganization\",\"dataSource\":\"hapi\",\"fieldsToBeHidden\":[\"pageTitle\",\"firstName\",\"middleName\",\"lastName\",\"microsoftPrivacyTextGroup\"],\"fieldsToBeEnabled\":null,\"fieldsToMakeRequired\":[\"companyName\"],\"fieldsToBeRemoved\":null,\"removeOptionalTextFromFields\":true}]},\"customizeElementLocation\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"hapiV1SoldToOrganization\",\"moveOrganizationNameBeforeEmailAddress\":true}]},\"disableFirstNameLastNameGrouping\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]}}}}";
            if (enableWithFeature)
            {
                expectedPSSResponse = "{\"" + operation + "\":{\"template\":\"defaulttemplate\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"hapiV1SoldToOrganization\",\"dataSource\":\"hapi\",\"fieldsToBeHidden\":[\"pageTitle\",\"firstName\",\"middleName\",\"lastName\",\"microsoftPrivacyTextGroup\"],\"fieldsToBeEnabled\":null,\"fieldsToMakeRequired\":[\"companyName\"],\"fieldsToBeRemoved\":null,\"removeOptionalTextFromFields\":true}]},\"customizeElementLocation\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"hapiV1SoldToOrganization\",\"moveOrganizationNameBeforeEmailAddress\":true}]},\"enableDoingBusinessAsField\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"disableFirstNameLastNameGrouping\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]}}}}";
            }

            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");

            var doingBusinessFieldDisplayHint = pidls[0].GetDisplayHintById("hapiV1ModernAccountV20190531Address_tradeName");
            if (enableWithFeature)
            {
                Assert.IsNotNull(doingBusinessFieldDisplayHint, "Doing business as field is expected.");
            }
            else
            {
                Assert.IsNull(doingBusinessFieldDisplayHint, "Doing business as field is expected to be null.");
            }

            PXSettings.PartnerSettingsService.ResetToDefaults();
        }

        [DataRow("hapiv1SoldToIndividual", "add", "officesmb", "us", "hapiV1ModernAccountV20190531Address_companyName,hapiV1ModernAccountV20190531Address_phoneNumber,hapiV1ModernAccountV20190531Address_email,hapiV1AddressHeading,starRequiredTextGroup")]
        [DataRow("hapiv1SoldToIndividual", "update", "officesmb", "us", "hapiV1ModernAccountV20190531Address_companyName,hapiV1ModernAccountV20190531Address_phoneNumber,hapiV1ModernAccountV20190531Address_email,hapiV1AddressHeading,starRequiredTextGroup")]
        [DataRow("shipping", "add", "officesmb", "us", "addressFirstNameOptional,addressLastNameOptional,emailAddressOptional,addressPhoneNumberOptional")]
        [DataRow("shipping", "update", "officesmb", "us", "addressFirstNameOptional,addressLastNameOptional,emailAddressOptional,addressPhoneNumberOptional")]
        [DataRow("shipping_v3", "add", "officesmb", "us", "addressFirstNameOptional,addressLastNameOptional,emailAddressOptional,addressPhoneNumberOptional")]
        [DataRow("shipping_v3", "update", "officesmb", "us", "addressFirstNameOptional,addressLastNameOptional,emailAddressOptional,addressPhoneNumberOptional")]
        [DataRow("orgaddress", "add", "officesmb", "us", "orgAddressModern_email,orgAddressModern_phoneNumber")]
        [DataRow("orgaddress", "update", "officesmb", "us", "orgAddressModern_email,orgAddressModern_phoneNumber")]
        [DataRow("billing", "add", "officesmb", "us", "addressCountry")]
        [DataRow("billing", "update", "officesmb", "us", "addressCountry")]
        [DataTestMethod]
        public async Task GetAddressDescriptions_PSS_CustomizeAddressForm_FieldsToBeRemoved(string type, string operation, string partner, string country, string displayHintIdsToCheck)
        {
            // Arrange
            string url = $"/v7.0/Account001/AddressDescriptions?country={country}&language=en-US&type={type}&partner={partner}&operation={operation}";
            string partnerSettingResponse;

            // Do not change order of values, it required to first execute the false to collect the property names
            bool[] featureStatus = new bool[] { false, true };
            Dictionary<string, string> displayHintIdMappingWithPropertyName = new Dictionary<string, string>();

            var headers = new Dictionary<string, string>()
            {
                { "x-ms-flight", "PXDisablePSSCache" }
            };

            foreach (bool featureEnabled in featureStatus)
            {
                if (featureEnabled)
                {
                    partnerSettingResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"shipping\",\"dataSource\":\"jarvisShipping\",\"fieldsToBeRemoved\":[\"firstName\",\"lastName\",\"email\",\"phoneNumber\"]},{\"addressType\":\"shipping_v3\",\"dataSource\":\"jarvisShippingV3\",\"fieldsToBeRemoved\":[\"firstName\",\"lastName\",\"email\",\"phoneNumber\"]},{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\",\"fieldsToBeRemoved\":[\"companyName\",\"email\",\"phoneNumber\",\"pageTitle\",\"starRequiredTextGroup\"]},{\"addressType\":\"billing\",\"dataSource\":\"jarvisBilling\",\"fieldsToBeRemoved\":[\"country\"]},{\"addressType\":\"orgaddress\",\"dataSource\":\"jarvisOrgAddress\",\"fieldsToBeRemoved\":[\"orgEmail\",\"orgPhoneNumber\"]}]}}},\"update\":{\"template\":\"defaulttemplate\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"shipping\",\"dataSource\":\"jarvisShipping\",\"fieldsToBeRemoved\":[\"firstName\",\"lastName\",\"email\",\"phoneNumber\"]},{\"addressType\":\"shipping_v3\",\"dataSource\":\"jarvisShippingV3\",\"fieldsToBeRemoved\":[\"firstName\",\"lastName\",\"email\",\"phoneNumber\"]},{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\",\"fieldsToBeRemoved\":[\"companyName\",\"email\",\"phoneNumber\",\"pageTitle\",\"starRequiredTextGroup\"]},{\"addressType\":\"billing\",\"dataSource\":\"jarvisBilling\",\"fieldsToBeRemoved\":[\"country\"]},{\"addressType\":\"orgaddress\",\"dataSource\":\"jarvisOrgAddress\",\"fieldsToBeRemoved\":[\"orgEmail\",\"orgPhoneNumber\"]}]}}}}";
                }
                else
                {
                    partnerSettingResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"shipping\",\"dataSource\":\"jarvisShipping\"},{\"addressType\":\"shipping_v3\",\"dataSource\":\"jarvisShippingV3\"},{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\"},{\"addressType\":\"billing\",\"dataSource\":\"jarvisBilling\"},{\"addressType\":\"orgaddress\",\"dataSource\":\"jarvisOrgAddress\"}]}}},\"update\":{\"template\":\"defaulttemplate\",\"features\":{\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"shipping\",\"dataSource\":\"jarvisShipping\"},{\"addressType\":\"shipping_v3\",\"dataSource\":\"jarvisShippingV3\"},{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\"},{\"addressType\":\"billing\",\"dataSource\":\"jarvisBilling\"},{\"addressType\":\"orgaddress\",\"dataSource\":\"jarvisOrgAddress\"}]}}}}";
                }

                PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);

                // Act
                List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

                // Assert
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");
                Assert.IsNotNull(pidls[0].DataDescription, "DataDescription is expected to be not null");

                foreach (string hintId in displayHintIdsToCheck.Split(','))
                {
                    var displayHint = pidls[0].GetDisplayHintById(hintId);
                    JToken dataDescription = JToken.FromObject(pidls[0].DataDescription);
                    bool isPropertyDisplayHint = displayHint as PropertyDisplayHint != null;

                    if (featureEnabled)
                    {
                        Assert.IsNull(displayHint, $"{hintId} displayhint is expected to be removed");
                        
                        if (isPropertyDisplayHint)
                        {
                            JToken hintPropertyToken = dataDescription.SelectToken("$.." + displayHintIdMappingWithPropertyName[hintId]);
                            Assert.IsNull(hintPropertyToken, $"{displayHintIdMappingWithPropertyName[hintId]} is expected to be removed from DataDescription");
                        }
                    }
                    else
                    {
                        Assert.IsNotNull(displayHint, $"{hintId} displayhint is expected in pidl");

                        if (isPropertyDisplayHint)
                        {
                            JToken hintPropertyToken = dataDescription.SelectToken("$.." + displayHint.PropertyName);
                            Assert.IsNotNull(hintPropertyToken, $"{displayHint.PropertyName} is expected to in DataDescription");

                            displayHintIdMappingWithPropertyName.Add(hintId, displayHint.PropertyName);
                        }
                    }
                }

                PXSettings.PartnerSettingsService.ResetToDefaults();
                PXSettings.PimsService.ResetToDefaults();
            }
        }

        /// <summary>
        /// Test is to validate the enablePlaceholder PSS feature
        /// </summary>
        /// <returns></returns>
        [DataTestMethod]
        public async Task GetAddressDescription_EnablePlaceholderPssFeature()
        {
            // Arrange
            List<bool> featureStatuses = new List<bool>() { false, true };
            List<string> operations = new List<string>() { "add", "update" };
            Dictionary<string, string> headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };
            List<string> addressTypes = new List<string>() { "billing", "shipping", "hapiV1SoldToIndividual", "hapiV1BillToIndividual" };

            foreach (string operation in operations)
            {
                foreach (string type in addressTypes)
                {
                    foreach (bool featureStatus in featureStatuses)
                    {
                        string enablePlaceholderPssSettings = featureStatus ? "\"enablePlaceholder\":{\"applicableMarkets\":[]}," : string.Empty;
                        string url = $"/v7.0/Account001/AddressDescriptions?partner=officesmb&operation={operation}&type={type}&country=us&language=en-US";
                        string partnerSettingResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"features\":{" + enablePlaceholderPssSettings + "\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\"},{\"addressType\":\"hapiV1BillToIndividual\",\"dataSource\":\"hapi\"}]}}},\"update\":{\"template\":\"defaulttemplate\",\"features\":{" + enablePlaceholderPssSettings + "\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\"},{\"addressType\":\"hapiV1BillToIndividual\",\"dataSource\":\"hapi\"}]}}}}";
                        PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);

                        // Act
                        List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

                        // Assert
                        Assert.IsNotNull(pidls, "Pidl is expected to be not null");

                        foreach (PIDLResource pidl in pidls)
                        {
                            IEnumerable<DisplayHint> displayHints = pidl.DisplayHints();

                            foreach (DisplayHint displayHint in displayHints)
                            {
                                PropertyDisplayHint displayProperty = displayHint as PropertyDisplayHint;

                                if (displayProperty != null)
                                {
                                    Assert.AreEqual("true", displayProperty.ShowDisplayName, "ShowDisplayName should be true");

                                    if (featureStatus)
                                    {
                                        if (displayProperty.PossibleValues == null && displayProperty.SelectType == null)
                                        {
                                            Assert.IsTrue(displayProperty.DisplayExample.Contains(displayProperty.DisplayName));
                                        }                                            
                                    }
                                    else
                                    {
                                        Assert.IsNull(displayProperty.DisplayExample);
                                    }
                                }
                            }
                        }

                        PXSettings.PartnerSettingsService.Responses.Clear();
                    }
                }
            }
        }
    }
}