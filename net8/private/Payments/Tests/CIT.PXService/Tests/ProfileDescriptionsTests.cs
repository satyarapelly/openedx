// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using global::Tests.Common.Model;
    using global::Tests.Common.Model.Pidl;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class ProfileDescriptionsTests : TestBase
    {
        public const string CountriesNewlyEnabledToCollectTaxId = "bb,il,kz,la,np,sg,ug,ci,gh,sn,zm";
        public const string CountriesAlreadyExistingToCollectTaxId = "am,by,mx,no,tr,id,th,bh,is,fj";

        [DataRow("x-ms-flight", "soldToHideButton", "", true)]
        [DataRow("x-ms-flight", "soldToHideButton,dummyValue", "dummyValue", true)]
        [DataRow("x-ms-flight", "dummyValue", "dummyValue", false)]
        [DataTestMethod]
        public async Task GetProfileDescriptions_HideProfileSubmitButton(string headerKey, string allHeaderValue, string leftoverHeader, bool isHidden)
        {
            // Arrange
            string url = $"/v7.0/Account001/profileDescriptions?country=us&language=en-US&type=employee&partner=commercialstores&operation=add";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXServiceWithPartnerHeader(url, headerKey, allHeaderValue, leftoverHeader);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");
            foreach (PIDLResource profilePidl in pidls)
            {
                DisplayHint saveButtonDisplayDescription = profilePidl.GetDisplayHintById("saveButton");
                Assert.IsNotNull(saveButtonDisplayDescription);
                Assert.AreEqual(saveButtonDisplayDescription.IsHidden ?? false, isHidden);

                DisplayHint cancelButtonDisplayDescription = profilePidl.GetDisplayHintById("cancelButton");
                Assert.IsNotNull(cancelButtonDisplayDescription);
                Assert.AreEqual(cancelButtonDisplayDescription.IsHidden ?? false, isHidden);
            }
        }

        /// <summary>
        /// This CIT is uses to test the feature of groupAddressFields for the differnent profile types.
        /// </summary>
        /// <param name="profileType"></param>
        /// <param name="partnerName"></param>
        /// <param name="scenario"></param>
        /// <returns></returns>
        [DataRow("employee", "officesmb", null)]
        [DataRow("organization", "officesmb", null)]
        [DataRow("organization", "officesmb", "twoColumns")]
        [DataTestMethod]
        public async Task GetProfileDescriptions_Profile_GroupAddressFields(string profileType, string partnerName, string scenario = null)
        {
            // Arrange
            string addressFieldOrderByCountryFeatureEnabled;
            string addressFieldOrderByCountryFeatureDisabled;
            List<string> operations = new List<string> { Constants.OperationTypes.Add, Constants.OperationTypes.Update };

            bool[] featureStatus = new bool[] { true, false };
            Dictionary<bool, Dictionary<string, Dictionary<bool, List<string>>>> addressGroupFieldsOrderByFeatureForAdd = new Dictionary<bool, Dictionary<string, Dictionary<bool, List<string>>>>();
            Dictionary<bool, Dictionary<string, Dictionary<bool, List<string>>>> addressGroupFieldsOrderByFeatureForUpdate = new Dictionary<bool, Dictionary<string, Dictionary<bool, List<string>>>>();

            foreach (var operation in operations)
            {
                if (string.Equals(profileType, Constants.ProfileTypes.Organization, StringComparison.OrdinalIgnoreCase)
                     && string.Equals(operation, Constants.OperationTypes.Add))
                {
                    continue;
                }

                foreach (bool isGroupAddressFeatureEnabled in featureStatus)
                {
                    Dictionary<string, Dictionary<bool, List<string>>> addressFieldsOrderByCountry = new Dictionary<string, Dictionary<bool, List<string>>>();

                    foreach (string country in Constants.Countries)
                    {
                        Dictionary<bool, List<string>> addressFieldsForIsProfileHapiFeature = new Dictionary<bool, List<string>>();

                        foreach (bool isProfileHapiFeatureEnable in featureStatus)
                        {
                            string requestUrl = $"/v7.0/Account001/profileDescriptions?country={country}&language=en-US&type={profileType}&partner={partnerName}&operation={operation}";

                            var headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };

                            string features = string.Empty;

                            if (isGroupAddressFeatureEnabled)
                            {
                                features = "\"groupAddressFields\":{\"applicableMarkets\":[]}";
                            }

                            if (isProfileHapiFeatureEnable)
                            {
                                string hapiFeature = string.Equals(profileType, Constants.ProfileTypes.Organization)
                                    ? "\"useProfileUpdateToHapi\":{\"applicableMarkets\":[]}"
                                    : "\"useEmployeeProfileUpdateToHapi\":{\"applicableMarkets\":[]}";

                                features = string.IsNullOrEmpty(features) ? hapiFeature : $"{features},{hapiFeature}";
                            }

                            string expectedPSSResponse = "{\"" + operation + "\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"fullPage\",\"resources\":{\"profile\":{\"" + profileType + "\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"fullPage\"}}},\"features\":{" + features + "}}}";
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
                                    var addressFieldsWithOrder = new SortedDictionary<int, string>();

                                    GroupDisplayHint addressGroup = pidlDisplayPage?.Members
                                                                    .FirstOrDefault(displayHint => displayHint.HintId.Equals(Constants.DisplayHintIds.AddressGroup, StringComparison.OrdinalIgnoreCase)) as GroupDisplayHint;

                                    var addressFieldHints = pidlDisplayPage?.Members
                                                            .Where(displayHint => Constants.AddressFields.Contains(displayHint.HintId, StringComparer.OrdinalIgnoreCase))
                                                            .ToList();

                                    if (isGroupAddressFeatureEnabled)
                                    {
                                        var groupAddressFieldHints = addressGroup.Members
                                            .Where(displayHint => Constants.AddressFields.Contains(displayHint.HintId, StringComparer.OrdinalIgnoreCase))
                                            .ToList();

                                        foreach (var addressFieldDisplayHint in groupAddressFieldHints)
                                        {
                                            Assert.IsNotNull(addressGroup, "addressGroup is not expected to be null");
                                            Assert.IsTrue(addressGroup.Members.Contains(addressFieldDisplayHint), $"DisplayHint \"{addressFieldDisplayHint.HintId}\" is expected to be inside addressGroup. Test Country: {country}");
                                            int index = addressGroup.Members.IndexOf(addressFieldDisplayHint);
                                            if (!addressFieldsWithOrder.ContainsKey(index))
                                            {
                                                addressFieldsWithOrder.Add(index, addressFieldDisplayHint.HintId);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (var addressFieldDisplayHint in addressFieldHints)
                                        {
                                            Assert.IsNull(addressGroup, "addressGroup is expected to be null");
                                            int index = pidlDisplayPage.Members.IndexOf(addressFieldDisplayHint);
                                            if (!addressFieldsWithOrder.ContainsKey(index))
                                            {
                                                addressFieldsWithOrder.Add(index, addressFieldDisplayHint.HintId);
                                            }
                                        }
                                    }

                                    addressFieldsWithOrderByPidl.Add(string.Join(",", addressFieldsWithOrder.Values));
                                }
                            }

                            addressFieldsForIsProfileHapiFeature.Add(isProfileHapiFeatureEnable, addressFieldsWithOrderByPidl);

                            PXSettings.PartnerSettingsService.ResetToDefaults();
                        }

                        addressFieldsOrderByCountry.Add(country, addressFieldsForIsProfileHapiFeature);
                    }

                    if (string.Equals(operation, Constants.OperationTypes.Add))
                    {
                        addressGroupFieldsOrderByFeatureForAdd.Add(isGroupAddressFeatureEnabled, addressFieldsOrderByCountry);
                    }
                    else
                    {
                        addressGroupFieldsOrderByFeatureForUpdate.Add(isGroupAddressFeatureEnabled, addressFieldsOrderByCountry);
                    }
                }

                // Assert address fields order when feature groupAddressFields is enabled and disabled
                if (string.Equals(operation, Constants.OperationTypes.Add))
                {
                    addressFieldOrderByCountryFeatureEnabled = JsonConvert.SerializeObject(addressGroupFieldsOrderByFeatureForAdd[true]);
                    addressFieldOrderByCountryFeatureDisabled = JsonConvert.SerializeObject(addressGroupFieldsOrderByFeatureForAdd[false]);
                }
                else
                {
                    addressFieldOrderByCountryFeatureEnabled = JsonConvert.SerializeObject(addressGroupFieldsOrderByFeatureForUpdate[true]);
                    addressFieldOrderByCountryFeatureDisabled = JsonConvert.SerializeObject(addressGroupFieldsOrderByFeatureForUpdate[false]);
                }

                // Compare both strings manually to find the exact difference for country's address fields order
                Assert.AreEqual(addressFieldOrderByCountryFeatureEnabled, addressFieldOrderByCountryFeatureDisabled, "Address fields order should be same when feature enabled and disabled");
            }
        }

        [DataRow("employee", "commercialstores", "PXProfileUpdateToHapi", null, false)]
        [DataRow("employee", "commercialstores", "", null, false)]
        [DataRow("organization", "commercialstores", "PXProfileUpdateToHapi", null, true)]
        [DataRow("organization", "commercialstores", "", null, true)]
        [DataRow("organization", "officesmb", "PXProfileUpdateToHapi", null, true)]
        [DataRow("organization", "officesmb", "", null, true)]
        [DataRow("organization", "smboobemodern", "PXProfileUpdateToHapi", null, true)]
        [DataRow("organization", "smboobemodern", "", null, true)]
        [DataRow("organization", "smboobe", "PXProfileUpdateToHapi", null, true)]
        [DataRow("organization", "smboobe", "PXProfileUpdateToHapi", "roobe", true)]
        [DataRow("organization", "commercialstores", "", "twoColumns", true)]
        [DataRow("consumer", "webblends", "", "", true)]
        [DataRow("consumer", "webblends", "PXProfileUpdateToHapi", "", true)]
        [DataRow("consumer", "oxowebdirect", "", "", true)]
        [DataRow("consumer", "oxowebdirect", "PXProfileUpdateToHapi", "", true)]
        [DataRow("consumer", "defaulttemplate", "", null, true)]
        [DataRow("consumer", "onepage", "", null, true)]
        [DataRow("consumer", "twopage", "", null, true)]
        [DataRow("consumer", "officesmb", "", null, true, true)]
        [DataRow("consumer", "officesmb", "", null, true, false)]
        [DataRow("consumer", "smboobemodern", "", null, true, true)]
        [DataRow("consumer", "smboobemodern", "", null, true, false)]
        [DataTestMethod]
        public async Task GetProfileDescriptions_ProfileUsePatch(string profileType, string partnerName, string flightNames, string scenario, bool clientPrefill, bool enableSubmitLinkPatchFeature = false)
        {
            // Arrange
            string url = $"/v7.0/Account001/profileDescriptions?country=us&language=en-US&type={profileType}&partner={partnerName}&operation=update";
            if (!string.IsNullOrEmpty(scenario))
            {
                url += $"&scenario={scenario}";
            }

            List<string> exposedFlights = flightNames.Split(',').ToList();

            // Act
            if (string.Equals(partnerName, GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase)
                || string.Equals(partnerName, GlobalConstants.Partners.SmboobeModern, StringComparison.OrdinalIgnoreCase))
            {
                exposedFlights.Add("PXDisablePSSCache");
                string partnerSettingResponse = exposedFlights.Contains("PXProfileUpdateToHapi") ? "{\"update\":{\"template\":\"OnePage\",\"features\":null}}"
                    : "{\"update\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"fullPage\",\"features\":{\"useProfileUpdateToHapi\":{\"applicableMarkets\":[]}}}}";

                if (enableSubmitLinkPatchFeature)
                {
                    partnerSettingResponse = "{\"update\":{\"template\":\"OnePage\",\"features\":{\"updatePidlSubmitLink\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"updateConsumerProfileSubmitLinkToJarvisPatch\":true}]}}}}";
                }

                PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
            }

            List<PIDLResource> pidls = await GetPidlFromPXServiceWithFlight(url, exposedFlights);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");
            foreach (PIDLResource profilePidl in pidls)
            {
                Assert.IsNotNull(profilePidl.DisplayPages[0].DisplayName, "Profile page is expected to be not null");
                
                if (string.Equals(profileType, "consumer", StringComparison.OrdinalIgnoreCase))
                {
                    string submitLink = profilePidl.GetDisplayHintById("saveButton").Action.Context.ToString();
                    
                    if (!string.IsNullOrEmpty(submitLink) && string.Equals(partnerName, GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase))
                    {
                        if (enableSubmitLinkPatchFeature)
                        {
                            Assert.IsTrue(submitLink.Contains("PATCH"), "Submitlink method is expected to be Patch");
                        }
                        else
                        {
                            Assert.IsTrue(submitLink.Contains("PUT"), "Submitlink method is expected to be PUT");
                        }
                    }
                }

                if (clientPrefill)
                {
                    Assert.IsNotNull(profilePidl.DataSources, "For client side prefilling to work, Profile Pidl DataSources is expected to be not null");
                    Assert.IsTrue(profilePidl.DataSources.Count > 0, "For client side prefilling to work, Profile Pidl is expected to have at least one DataSource");

                    if (string.Equals(profileType, "organization", StringComparison.OrdinalIgnoreCase) && profilePidl.LinkedPidls != null)
                    {
                        foreach (PIDLResource taxPidl in profilePidl.LinkedPidls)
                        {
                            Assert.IsNotNull(taxPidl.DataSources, "For client side prefilling to work, Tax Pidl DataSources is expected to be not null");
                            Assert.IsTrue(taxPidl.DataSources.Count > 0, "For client side prefilling to work, Tax Pidl is expected to have at least one DataSource");
                        }
                    }
                }
                else
                {
                    Assert.IsNull(profilePidl.DataSources, "For service side prefilling to work, Profile Pidl DataSources is expected to be null");
                }
            }
        }

        [DataRow("employee", "commercialstores")]
        [DataRow("organization", "commercialstores")]
        [DataRow("consumer", "webblends")]
        [DataRow("consumer", "oxowebdirect")]
        [DataTestMethod]
        public async Task TestPartnerMigration_PartnerSettingsService(string type, string partner)
        {
            var pssPidls = new List<PIDLResource>();
            var pidls = new List<PIDLResource>();
            Dictionary<string, string> testHeader = new Dictionary<string, string>()
            {
                { "x-ms-flight", "PXUsePartnerSettingsService" },
            };

            string expectedPSSResponse = $"{{\"default\":{{\"template\":\"{partner}\"}}}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            string url = $"/v7.0/Account001/profileDescriptions?country=us&language=en-US&type={type}&partner={partner}&operation=update";

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
        /// The test is to validate the linkedPidl for the enabled and updated countries in order to collect tax ID. The test covers 3 cases
        /// 1. Countries enabled before zinc, without flighting, the linked pidl should be attached.
        /// 2. Countries enabled in zinc, with flighting, the linked pidl should be attached.
        /// 3. Countries enabled in zinc, without flighting, the linked pidl should not be attached.
        /// [Note: LinkedPidl has a value when either the PXEnableVATID or enableItalyCodiceFiscale flight is enabled or when the partner is a template partner. Therefore, we have excluded the "officesmb" partner from [DataRow(CountriesNewlyEnabledToCollectTaxId, "commercialstores,azure", "", false)] and included it in [DataRow("it", "commercialstores,azure", null, true)].]
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
        [DataRow("it", "commercialstores,azure", null, true)]
        [DataRow("eg", "commercialstores,azure,officesmb", "PXEnableVATID", true)]
        [DataRow("eg", "commercialstores,azure", null, false)] 
        [TestMethod]
        public async Task GetProfileDescriptions_validateLinkedPidlToCollectTaxId(string countryList, string partnerList, string flighting, bool haveLinkedPidl)
        {
            // Arrange
            List<PIDLResource> pidls = null;
            string[] partners = partnerList.Split(',');
            string[] countries = countryList.Split(',');
            string[] operations = new string[] { Constants.OperationTypes.Update };
            string[] profileTypes = new string[] { Constants.ProfileTypes.Employee, Constants.ProfileTypes.Organization };
            foreach (string partner in partners)
            {
                foreach (string operation in operations)
                {
                    foreach (string type in profileTypes)
                    {
                        foreach (string country in countries)
                        {
                            if (!string.IsNullOrEmpty(flighting))
                            {
                                PXFlightHandler.AddToEnabledFlights(flighting);
                            }

                            string url = $"/v7.0/Account001/profileDescriptions?type={type}&language=en-us&partner={partner}&operation={operation}&country={country}";

                            // Act
                            if (string.Equals(partner, GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase))
                            {
                                var headers = new Dictionary<string, string>()
                                {
                                    {
                                        "x-ms-flight", "PXDisablePSSCache"
                                    }
                                };

                                string partnerSettingResponse = "{\"update\":{\"template\":\"OnePage\",\"features\":null}}";
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
                                && string.Equals(operation, Constants.OperationTypes.Update, StringComparison.OrdinalIgnoreCase)
                                && string.Equals(type, Constants.ProfileTypes.Organization, StringComparison.OrdinalIgnoreCase)
                                && haveLinkedPidl)
                            {
                                Assert.IsNotNull(pidls[0].LinkedPidls);
                                if (string.Equals("it", country, StringComparison.OrdinalIgnoreCase) || string.Equals("eg", country, StringComparison.OrdinalIgnoreCase))
                                {
                                    int linkedPidlCount = pidls[0].LinkedPidls.Count();
                                    if (string.IsNullOrEmpty(flighting))
                                    {
                                        Assert.AreEqual(1, linkedPidlCount);
                                    }
                                    else
                                    {
                                        Assert.AreEqual(2, linkedPidlCount);
                                    }
                                }                               
                            }
                            else
                            {
                                Assert.IsNull(pidls[0].LinkedPidls);
                            }

                            PXFlightHandler.ResetToDefault();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This test is used to verify the DefaultValue of the linkedPidl for taxID property, when the flighting PXSetItalyTaxIdValuesByFunction is enabled.
        /// PXProfileUpdateToHapi is enabled here to make sure the taxID prperty DefaultValue is not null,  this internally convert the update operation to update_partial.
        /// </summary>
        /// <param name="flights">Flights name</param>
        /// <returns></returns>
        [DataRow("enableItalyCodiceFiscale")]
        [DataRow("PXProfileUpdateToHapi,enableItalyCodiceFiscale")]
        [DataRow("PXProfileUpdateToHapi,enableItalyCodiceFiscale,PXSetItalyTaxIdValuesByFunction")]
        [TestMethod]
        public async Task GetProfileDescriptions_validateLinkedPidlDefaultValueTaxId(string flights)
        {
            // Arrage
            string url = $"/v7.0/Account001/ProfileDescriptions?type=organization&partner=commercialstores&country=it&language=en-US&operation=update";
                
            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, flightNames: flights);
               
            // Assert
            Assert.IsNotNull(pidls);
            Assert.IsNotNull(pidls[0].LinkedPidls);
               
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
            else if (!flights.Contains("PXProfileUpdateToHapi"))
            {
                Assert.IsNull(linkedTaxIdDescriptionCodaic.DefaultValue);
                Assert.IsNull(linkedtaxIdDescriptionVatId.DefaultValue);
            }
            else
            {
                Assert.AreEqual(linkedTaxIdDescriptionCodaic.DefaultValue, "({dataSources.taxResource.value[1].taxId})");
                Assert.AreEqual(linkedtaxIdDescriptionVatId.DefaultValue, "({dataSources.taxResource.value[0].taxId})");
            }

            PXFlightHandler.ResetToDefault();
        }

        /// <summary>
        /// This test is used to verify the DefaultValue of the linkedPidl for taxID property.
        /// PXProfileUpdateToHapi is enabled here to make sure the taxID prperty DefaultValue is not null,  this internally convert the update operation to update_partial.
        /// </summary>
        /// <param name="flights">Flights name</param>
        /// <returns></returns>
        [DataRow("PXEnableVATID")]
        [DataRow("PXProfileUpdateToHapi,PXEnableVATID")]
        [TestMethod]
        public async Task GetProfileDescriptions_validateLinkedPidlDefaultValueEgyptTaxId(string flights)
        {
            // Arrage
            string url = $"/v7.0/Account001/ProfileDescriptions?type=organization&partner=commercialstores&country=eg&language=en-US&operation=update";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, flightNames: flights);

            // Assert
            Assert.IsNotNull(pidls);
            Assert.IsNotNull(pidls[0].LinkedPidls);

            PropertyDescription linkedTaxIdDescriptionNationalId = pidls[0].LinkedPidls[0].GetPropertyDescriptionByPropertyName("taxId");
            PropertyDescription linkedtaxIdDescriptionVatId = pidls[0].LinkedPidls[1].GetPropertyDescriptionByPropertyName("taxId");

            string nindefaultValue = linkedTaxIdDescriptionNationalId.DefaultValue;
            string vatIdDefaultValue = pidls[0].LinkedPidls[1].GetPropertyDescriptionByPropertyName("taxId").DefaultValue;

            Assert.IsNotNull(linkedTaxIdDescriptionNationalId, "Taxid property is expected to be not null");
            Assert.IsNotNull(pidls[0].LinkedPidls, "Italy tax linked pidl is expected to be not null");
           
            Assert.IsNotNull(linkedtaxIdDescriptionVatId, "Taxid property is expected to be not null");

           if (!flights.Contains("PXProfileUpdateToHapi"))
            {
                Assert.IsNull(linkedTaxIdDescriptionNationalId.DefaultValue);
                Assert.IsNull(linkedtaxIdDescriptionVatId.DefaultValue);
            }
            else
            {
                Assert.AreEqual(linkedTaxIdDescriptionNationalId.DefaultValue, "(<|getNationalIdentificationNumber|>)");
                Assert.AreEqual(linkedtaxIdDescriptionVatId.DefaultValue, "(<|getVatId|>)");
            }

            PXFlightHandler.ResetToDefault();
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
        public async Task GetProfileDescriptions_validateLinkedPidlTaxId_UnderFlight(string partner, string country, string flights, bool isLinkedTaxIdExpected)
        {
            // Arrage
            string url = $"/v7.0/Account001/ProfileDescriptions?type=organization&partner={partner}&country={country}&language=en-US&operation=update";

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
                    Assert.IsNotNull(taxLegalText, "TaxId legal tax is expected");
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
                    Assert.IsNull(taxLegalText, "TaxId legal tax is not expected");
                }
            }

            PXFlightHandler.ResetToDefault();
        }

        [DataRow("employee", "add", null)]
        [DataRow("employee", "update", null)]
        [DataRow("organization", "update", null)]
        [DataRow("organization", "update", "twoColumns")]
        [DataTestMethod]
        public async Task GetProfileDescriptions_ChangeFirstNameLastNameToMandatory(string profileType, string operation, string scenario)
        {
            // Arrange
            List<PIDLResource> pidls = null;
            List<string> partners = new List<string>() { "commercialstores", "officesmb" };
            foreach (string partner in partners)
            {
                string url = $"/v7.0/Account001/profileDescriptions?country=us&language=en-US&type={profileType}&partner={partner}&operation={operation}";
                if (!string.IsNullOrEmpty(scenario))
                {
                    url += $"&scenario={scenario}";
                }

                // Act
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
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");

                string[] propertyNames = { "default_address.first_name", "default_address.last_name" };
                foreach (PIDLResource profilePidl in pidls)
                {
                    foreach (string propertyName in propertyNames)
                    {
                        PropertyDescription propertyDescription = profilePidl.GetPropertyDescriptionByPropertyNameWithFullPath(propertyName);
                        Assert.IsNotNull(propertyDescription, propertyName + " can't be null");
                        Assert.IsTrue(propertyDescription.IsOptional != null && propertyDescription.IsOptional == false, "Property should be mandatory");
                    }
                }
            }
        }

        [DataRow("tr", 2, false)]
        [DataRow("am", 2, false)]
        [DataRow("by", 2, false)]
        [DataRow("no", 2, false)]
        [DataRow("bd", 2, false)]
        [DataRow("my", 2, false)]
        [DataRow("cl", 2, false)]
        [DataRow("mx", 2, false)]
        [DataRow("id", 2, false)]
        [DataRow("gb", 1, true)]
        [DataRow("th", 2, false)]
        [DataRow("bh", 2, false)]
        [DataRow("cm", 2, false)]
        [DataRow("ge", 2, false)]
        [DataRow("is", 2, false)]
        [DataRow("ke", 2, false)]
        [DataRow("md", 2, false)]
        [DataRow("ng", 2, false)]
        [DataRow("om", 2, false)]
        [DataRow("tj", 2, false)]
        [DataRow("ua", 2, false)]
        [DataRow("uz", 2, false)]
        [DataRow("zw", 2, false)]
        [DataRow("fj", 2, false)]
        [DataRow("gt", 2, false)]
        [DataRow("kh", 2, false)]
        [DataRow("ph", 2, false)]
        [DataRow("vn", 2, false)]
        [DataRow("zw", 2, false)]
        [DataRow("fj", 2, false)]
        [DataRow("gt", 2, false)]
        [DataRow("kh", 2, false)]
        [DataRow("ph", 2, false)]
        [DataRow("vn", 2, false)]
        [DataRow("ae", 2, false)]
        [DataRow("bs", 2, false)]
        [DataRow("co", 2, false)]
        [DataRow("sa", 2, false)]
        [DataRow("bd", 2, false)]
        [DataTestMethod]
        public async Task GetProfileDescriptions_ModifyTaxIdToMandatory(string country, int pidlCounter, bool isTaxidOptional)
        {
            // Arrange
            List<PIDLResource> pidls = null;
            List<string> partners = new List<string>() { "commercialstores", "officesmb" };

            foreach (string partner in partners)
            {
                string url = $"/v7.0/Account001/profileDescriptions?country={country}&language=en-US&type=organization&partner={partner}&operation=update";

                // Act
                if (string.Equals(partner, GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase))
                {
                    var headers = new Dictionary<string, string>()
                    {
                        {
                            "x-ms-flight", "PXDisablePSSCache"
                        }
                    };
                    string partnerSettingResponse = "{\"update\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"fullPage\",\"resources\":{\"profile\":{\"employee\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"fullPage\"},\"organization\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"fullPage\"}}},\"features\":{\"useMultipleProfile\":{\"applicableMarkets\":[]}}}}";
                    PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
                    pidls = await GetPidlFromPXService(url, additionaHeaders: headers);
                }
                else
                {
                    pidls = await GetPidlFromPXServiceWithFlight(url, new List<string>());
                }

                // Assert
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");
                Assert.AreEqual(pidls.Count, pidlCounter, "Expected pidl counter does not match the return result");
                Assert.IsNotNull(pidls[0].LinkedPidls, "Tax Pidl is expected to be not null");

                PropertyDescription taxIdDescription = pidls[0].LinkedPidls[0].GetPropertyDescriptionByPropertyName("taxId");
                Assert.IsNotNull(taxIdDescription, "Taxid property is expected to be not null");
                Assert.AreEqual(taxIdDescription.IsOptional, isTaxidOptional, "Taxid's IsOptional property is not correct");
            }
        }

        [DataRow("x-ms-flight", "standaloneProfile", "", "gb", "profileOrganizationLegalText", true)]
        [DataRow("x-ms-flight", "standaloneProfile", "", "tw", "profileOrganizationLoveCodeProperty,profileOrganizationMobileBarcodeProperty,profileOrganizationTaxInvoiceButton", true)]
        [DataRow("x-ms-flight", "standaloneProfile", "", "tr", "profileIsTaxEnabled", true)]
        [DataRow("x-ms-flight", "standaloneProfile", "", "br", "profileOrganizationLegalTextLine1,profileOrganizationLegalTextLine2", true)]
        [DataRow("x-ms-flight", "standaloneProfile", "", "nz", "profileOrganizationLegalTextLine1,profileOrganizationLegalTextLine2,profileOrganizationLegalTextLine3", true)]
        [DataRow("x-ms-flight", "dummyValue", "dummyValue", "gb", "profileOrganizationLegalText", false)]
        [DataRow("x-ms-flight", "", "", "gb", "profileOrganizationLegalText", false)]
       
        [DataTestMethod]
        public async Task GetProfileDescriptions_StandaloneProfile(string headerKey, string allHeaderValue, string leftoverHeader, string country, string propertyName, bool isDeleted)
        {
            // Arrange
            List<string> partners = new List<string>() { "commercialstores", "officesmb" };

            foreach (string partner in partners)
            {
                string url = $"/v7.0/Account001/profileDescriptions?country={country}&language=en-US&type=organization&partner={partner}&operation=update";
                string[] taxPropertyNames = propertyName.Split(',');

                // Act
                if (string.Equals(partner, GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase))
                {
                    allHeaderValue = allHeaderValue?.Length > 0 ? allHeaderValue + ",PXDisablePSSCache" : string.Empty;
                    string partnerSettingResponse = "{\"update\":{\"template\":\"OnePage\",\"features\":null}}";
                    PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
                }

                List<PIDLResource> pidls = await GetPidlFromPXServiceWithPartnerHeader(url, headerKey, allHeaderValue, leftoverHeader);

                // Assert
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");
                foreach (PIDLResource profilePidl in pidls)
                {
                    Assert.AreEqual(profilePidl.LinkedPidls == null, isDeleted, "Linked PIDL returns wrong data");
                    foreach (string taxPropertyName in taxPropertyNames)
                    {
                        Assert.AreEqual(profilePidl.GetDisplayHintById(taxPropertyName) == null, isDeleted);
                    }
                }
            }
        }

        [DataRow("x-ms-flight", "showMiddleName", "", "organization", "update", true)]
        [DataRow("x-ms-flight", "showMiddleName", "", "employee", "add", true)]
        [DataRow("x-ms-flight", "showMiddleName", "", "employee", "update", true)]
        [DataRow("x-ms-flight", "", "", "organization", "update", false)]
        [DataRow("x-ms-flight", "", "", "employee", "add", false)]
        [DataRow("x-ms-flight", "", "", "employee", "update", false)]
        [DataTestMethod]
        public async Task GetProfileDescriptions_ShowMiddleName(string headerKey, string allHeaderValue, string leftoverHeader, string type, string operation, bool isVisible)
        {
            // Arrange
            List<string> partners = new List<string>() { "commercialstores", "officesmb" };
            foreach (string partner in partners)
            {
                string url = $"/v7.0/Account001/profileDescriptions?country=us&language=en-US&type={type}&partner={partner}&operation={operation}";

                // Act
                if (string.Equals(partner, GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase))
                {
                    allHeaderValue = allHeaderValue?.Length > 0 ? allHeaderValue + ",PXDisablePSSCache" : string.Empty;
                    string partnerSettingResponse = "{\"add\":{\"template\":\"OnePage\",\"features\":null}, \"update\":{\"template\":\"OnePage\",\"features\":null}}";
                    PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
                }

                List<PIDLResource> pidls = await GetPidlFromPXServiceWithPartnerHeader(url, headerKey, allHeaderValue, leftoverHeader);

                // Assert
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");
                foreach (PIDLResource profilePidl in pidls)
                {
                    DisplayHint addressMiddleName = profilePidl.GetDisplayHintById("addressMiddleName");
                    Assert.AreEqual(addressMiddleName != null, isVisible);
                }
            }
        }

        [DataRow("x-ms-flight", "showAVSSuggestions", "", "organization", "update", true, true)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "organization", "update", true, false)]
        [DataRow("x-ms-flight", "", "", "organization", "update", false, false)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "employee", "update", true, true)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "employee", "update", true, false)]
        [DataRow("x-ms-flight", "", "", "employee", "update", false, false)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "employee", "add", true, true)]
        [DataRow("x-ms-flight", "showAVSSuggestions", "", "employee", "add", true, false)]
        [DataRow("x-ms-flight", "", "", "employee", "add", false, false)]
        [DataTestMethod]
        public async Task GetProfileDescriptions_ShowAVSSuggestions(string headerKey, string allHeaderValue, string leftoverHeader, string type, string operation, bool showAVSSuggestions, bool usePidlPage)
        {
            // Arrange
            List<string> partners = new List<string>() { "commercialstores", "officesmb" };
            foreach (string partner in partners)
            {
                string url = $"/v7.0/Account001/profileDescriptions?country=us&language=en-US&type={type}&partner={partner}&operation={operation}";

                PXFlightHandler.AddToEnabledFlights("PXEnableAVSSuggestions");

                if (!usePidlPage)
                {
                    PXFlightHandler.AddToEnabledFlights("TradeAVSUsePidlModalInsteadofPidlPage");
                }

                // Act
                if (string.Equals(partner, GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase))
                {
                    allHeaderValue = allHeaderValue?.Length > 0 ? allHeaderValue + ",PXDisablePSSCache" : string.Empty;
                    string partnerSettingResponse = "{\"add\":{\"template\":\"OnePage\",\"features\":null}, \"update\":{\"template\":\"OnePage\",\"features\":null}}";
                    PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
                }

                List<PIDLResource> pidls = await GetPidlFromPXServiceWithPartnerHeader(url, headerKey, allHeaderValue, leftoverHeader);

                // Assert
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");
                foreach (PIDLResource profilePidl in pidls)
                {
                    DisplayHint saveButtonDisplayHint = profilePidl.GetDisplayHintById("saveButton");
                    Assert.IsNotNull(saveButtonDisplayHint);
                    Assert.IsNotNull(saveButtonDisplayHint.Action);
                    Assert.IsNotNull(saveButtonDisplayHint.Action.Context);
                    if (showAVSSuggestions)
                    {
                        Assert.IsNotNull(saveButtonDisplayHint.Action.NextAction);
                    }

                    Assert.AreEqual(saveButtonDisplayHint.Action.Context.ToString().Contains("scenario=suggestAddressesTradeAVSUsePidlModal"), !usePidlPage && showAVSSuggestions);
                    Assert.AreEqual(saveButtonDisplayHint.Action.Context.ToString().Contains("ModernValidate"), showAVSSuggestions);

                    if (string.Equals(allHeaderValue, "allHeaderValue", StringComparison.OrdinalIgnoreCase))
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

                        Assert.IsNotNull(profilePidl.DataDescription[Constants.CommercialZipPlusFourPropertyNames.IsUserConsented]);
                        Assert.IsNotNull(profilePidl.DataDescription[Constants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded]);
                        Assert.IsTrue(contain_is_customer_consented_checkbox);
                        Assert.IsTrue(contain_is_avs_full_validation_succeeded_checkbox);
                    }

                    GroupDisplayHint submitGroup = (GroupDisplayHint)profilePidl.DisplayPages[0].Members[profilePidl.DisplayPages[0].Members.Count - 1];
                    Assert.IsTrue(string.Equals("cancelSaveGroup", submitGroup.HintId, StringComparison.OrdinalIgnoreCase));
                    if (showAVSSuggestions)
                    {
                        Assert.IsTrue(string.Equals(Constants.CommercialZipPlusFourPropertyNames.IsAvsFullValidationSucceeded, profilePidl.DisplayPages[0].Members[0].PropertyName));
                        Assert.IsTrue(string.Equals(Constants.CommercialZipPlusFourPropertyNames.IsUserConsented, profilePidl.DisplayPages[0].Members[1].PropertyName));
                    }
                }
            }
        }

        // 62dc8681-6753-484a-981a-128f82a43d25 is added in the local config in int (used by int and devbox)
        // therefore it will have both PXEmployeeProfileUpdateToHapi and PXProfileUpdateToHapi.
        // As the result, the submitEndpoint should be Hapi
        [DataRow("organization", "update", true, "62dc8681-6753-484a-981a-128f82a43d25")]
        [DataRow("employee", "update", true, "62dc8681-6753-484a-981a-128f82a43d25")]
        [DataTestMethod]
        public async Task GetProfileDescriptionsOrganizationOrEmployeeProfile_CheckSubmitEndpointHapiOrNot(
            string type,
            string operation,
            bool useHapiEndpoint,
            string accountId)
        {
            // Arrange
            List<PIDLResource> pidls = null;
            List<string> partners = new List<string>() { "commercialstores", "officesmb" };
            foreach (string partner in partners)
            {
                string url = $"/v7.0/{accountId}/profileDescriptions?country=us&language=en-US&type={type}&partner={partner}&operation={operation}";

                // Act
                if (string.Equals(partner, GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase))
                {
                    var headers = new Dictionary<string, string>()
                    {
                        {
                            "x-ms-flight", "PXDisablePSSCache"
                        }
                    };
                    string partnerSettingResponse = "{\"update\":{\"template\":\"OnePage\",\"features\":null}}";
                    PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
                    pidls = await GetPidlFromPXService(url, additionaHeaders: headers);
                }
                else
                {
                    pidls = await GetPidlFromPXService(url);
                }

                // Assert
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");
                foreach (PIDLResource profilePidl in pidls)
                {
                    DisplayHint saveButtonDisplayHint = profilePidl.GetDisplayHintById("saveButton");
                    Assert.IsNotNull(saveButtonDisplayHint);
                    Assert.IsNotNull(saveButtonDisplayHint.Action);
                    Assert.IsNotNull(saveButtonDisplayHint.Action.Context);
                    Assert.AreEqual(useHapiEndpoint, saveButtonDisplayHint.Action.Context.ToString().Contains("hapi-endpoint"));
                }
            }
        }

        [DataRow("organization", "update", true)]
        [DataRow("employee", "update", false)]
        [DataRow("employee", "update", true)]
        [DataTestMethod]
        public async Task GetProfileDescriptions_CheckSubmitEndpoint(string type, string operation, bool useHapiEndpoint)
        {
            // Arrange
            List<PIDLResource> pidls = null;
            string featureName;
            List<string> partners = new List<string>() { "commercialstores", "officesmb", GlobalConstants.Partners.SmboobeModern };
            foreach (string partner in partners)
            {
                string url = $"/v7.0/Account001/profileDescriptions?country=us&language=en-US&type={type}&partner={partner}&operation={operation}";
                
                if (useHapiEndpoint)
                {
                    // TODO: Once both flights 'PXProfileUpdateToHapi' and 'PXEmployeeProfileUpdateToHapi' are merged, remove the following if condition.
                    if (string.Equals(type, "employee", StringComparison.OrdinalIgnoreCase) && !string.Equals(partner, GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase))
                    {
                        PXFlightHandler.AddToEnabledFlights("PXEmployeeProfileUpdateToHapi");
                    }
                }

                // Act
                if (string.Equals(partner, GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(partner, GlobalConstants.Partners.SmboobeModern, StringComparison.OrdinalIgnoreCase))
                {
                    var headers = new Dictionary<string, string>()
                    {
                        {
                            "x-ms-flight", "PXDisablePSSCache"
                        }
                    };

                    if (useHapiEndpoint)
                    {
                        featureName = type == "employee" ? "useEmployeeProfileUpdateToHapi" : "useProfileUpdateToHapi";
                    }
                    else
                    {
                        featureName = string.Empty;
                    }

                    string partnerSettingResponse = "{\"update\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"fullPage\",\"resources\":{\"profile\":{\"employee\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"fullPage\"},\"organization\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"fullPage\"}}},\"features\":{\"" + featureName + "\":{\"applicableMarkets\":[]}}}}";
                    PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
                    pidls = await GetPidlFromPXService(url, additionaHeaders: headers);
                }
                else
                {
                    pidls = await GetPidlFromPXService(url);
                }

                // Assert
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");
                foreach (PIDLResource profilePidl in pidls)
                {
                    DisplayHint saveButtonDisplayHint = profilePidl.GetDisplayHintById("saveButton");
                    Assert.IsNotNull(saveButtonDisplayHint);
                    Assert.IsNotNull(saveButtonDisplayHint.Action);
                    Assert.IsNotNull(saveButtonDisplayHint.Action.Context);
                    Assert.AreEqual(saveButtonDisplayHint.Action.Context.ToString().Contains("hapi-endpoint"), useHapiEndpoint);
                }
            }
        }

        [DataRow("officesmb", "organization", "update", "")]
        [DataRow("commercialstores", "organization", "update", "")]
        [DataRow("smboobe", "organization", "update", "PXProfileUpdateToHapi")]
        [DataTestMethod]
        public async Task GetProfileDescriptions_CheckDisabledProperty(string partner, string profileType, string operation, string flightNames)
        {
            // Arrange
            List<PIDLResource> pidls = null;
            string url = $"/v7.0/Account001/profileDescriptions?country=us&language=en-US&type={profileType}&partner={partner}&operation={operation}";
            string propertyName = "company_name";

            // Act
            if (string.Equals(partner, GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase))
            {
                var headers = new Dictionary<string, string>()
                {
                    {
                        "x-ms-flight", "PXDisablePSSCache"
                    }
                };
                string partnerSettingResponse = "{\"update\":{\"template\":\"OnePage\",\"features\":null}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
                pidls = await GetPidlFromPXService(url, flightNames: flightNames, additionaHeaders: headers);
            }
            else
            {
               pidls = await GetPidlFromPXService(url, flightNames: flightNames);
            }

            // Assert
            Assert.IsNotNull(pidls);
            foreach (PIDLResource pidl in pidls)
            {
                PropertyDisplayHint property = pidl.DisplayHints().First(x => string.Equals(propertyName, x.PropertyName, StringComparison.OrdinalIgnoreCase) && x is PropertyDisplayHint) as PropertyDisplayHint;
                Assert.IsTrue(property.IsDisabled.HasValue);
                Assert.IsTrue(property.IsDisabled.Value);
            }
        }

        /// <summary>
        /// Test is to validate postal code and region name for different countries
        /// </summary>
        /// <param name="country"></param>
        /// <param name="validPostalCode"></param>
        /// <param name="invalidPostalCode"></param>
        /// <param name="validRegionName"></param>
        /// <param name="invalidRegionName"></param>
        /// <returns></returns>
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
        [DataRow("KH", "12345", "1234", "Pulat", "R")]
        [DataRow("KH", "123456", "1234567", "Pulat", "R")]
        [DataTestMethod]
        public async Task GetProfileDescriptions_ValidateRegEx_StateAndPostalCode(string country, string validPostalCode, string invalidPostalCode, string validRegionName, string invalidRegionName)
        {
            // Arrange
            List<PIDLResource> pidls = null;
            string[] partners = new string[] { "commercialstores", "officesmb" };
            string[] operations = new string[] { Constants.OperationTypes.Add, Constants.OperationTypes.Update };
            string[] profileTypes = { Constants.ProfileTypes.Organization, Constants.ProfileTypes.LegalEntity, Constants.ProfileTypes.Employee };
            var headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };

            foreach (string operation in operations)
            {
                foreach (string partner in partners)
                {
                    foreach (string type in profileTypes)
                    {
                        if (!string.Equals(type, Constants.ProfileTypes.Employee, System.StringComparison.OrdinalIgnoreCase)
                            && string.Equals(operation, Constants.OperationTypes.Add, System.StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        string postalCodePropertyName = string.Equals(type, Constants.ProfileTypes.LegalEntity, System.StringComparison.OrdinalIgnoreCase) ? "postalCode" : "postal_code";
                        string url = $"/v7.0/Account001/profileDescriptions?partner={partner}&language=en-US&type={type}&operation={operation}&country={country}";

                        // Act
                        if (string.Equals(partner, GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase))
                        {
                            string partnerSettingResponse = "{\"add\":{\"template\":\"OnePage\",\"features\":null}, \"update\":{\"template\":\"OnePage\",\"features\":null}}";
                            PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
                        }
                        
                        pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

                        PropertyDescription regionDescription = pidls[0].GetPropertyDescriptionByPropertyName("region");
                        PropertyDescription postalCodeDescription = pidls[0].GetPropertyDescriptionByPropertyName(postalCodePropertyName);
                        string regionRegEx = regionDescription?.Validation?.Regex;
                        string postalCodeRegEx = postalCodeDescription?.Validation?.Regex;

                        // Assert
                        ValidatePidlPropertyRegex(pidls[0], "region", validRegionName, true, canRegexbeEmpty: true);
                        ValidatePidlPropertyRegex(pidls[0], "region", invalidRegionName, false, canRegexbeEmpty: true);

                        ValidatePidlPropertyRegex(pidls[0], postalCodePropertyName, validPostalCode, true);
                        ValidatePidlPropertyRegex(pidls[0], postalCodePropertyName, invalidPostalCode, false);

                        PXSettings.PartnerSettingsService.ResetToDefaults();
                        PXSettings.AccountsService.ResetToDefaults();
                    }
                }
            }
        }

        /// <summary>
        /// Asserts Culture & Language tranformation regex for ProfileDescriptions
        /// </summary>
        /// <returns></returns>
        [DataTestMethod]
        public async Task GetProfileDescriptions_ValidateCultureLanguageRegex()
        {
            // Arrange
            string[] countryList = "AF,AL,AQ,DZ,AS,AD,AO,AG,AX,AZ,AR,AU,AT,BS,BH,BD,AM,BB,BE,BL,BM,BT,BO,BA,BW,BV,BR,BZ,IO,SB,VG,BN,BG,BI,BY,KH,CM,CA,CV,KY,CF,LK,TD,CL,CN,TW,CX,CC,CO,KM,YT,CG,CK,CR,HR,CW,CY,CZ,BJ,DK,DM,DO,EC,SV,GQ,ET,ER,EE,FO,FK,GS,FJ,FI,FR,GF,PF,DJ,GA,GE,GM,PS,DE,GH,GI,KI,GR,GL,GD,GP,GU,GT,GN,GY,HT,HM,VA,HN,HK,HU,IS,IN,ID,IE,IL,IT,CI,JM,JP,KZ,JO,KE,KR,KW,KG,LA,LB,LS,LV,LR,LY,LI,LT,LU,MO,MF,MG,MM,MW,MY,MV,ML,MT,MQ,MR,MU,MX,MC,MN,MD,ME,MS,MA,MZ,OM,NA,NR,NP,NL,AW,NC,VU,NZ,NI,NE,NG,NU,NF,NO,MP,UM,FM,MH,PW,PK,PA,PG,PY,PE,PH,PN,PL,PT,GW,TL,PR,QA,RE,RO,RU,RW,SH,KN,AI,LC,PM,VC,SM,ST,SA,SN,SC,SL,SG,SK,VN,SI,ZA,ZW,ES,SR,SJ,SZ,SE,CH,TJ,TH,TG,TK,TO,TT,AE,TN,TR,TM,TC,TV,UG,UA,MK,EG,GB,GG,JE,IM,TZ,US,VI,BF,UY,UZ,VE,WF,WS,YE,RS,ZM,BQ,CD,IQ,XK,SO,SS,SX".Split(',');
            List<string> negativeTestValuesForCulture = new List<string> { "nl-NL", "fr-FR", "ar-AQ" };
            List<string> negativeTestValuesForLanguage = new List<string> { "ar", "RU", "ms", "sv", "el" };

            Dictionary<string, string> mandatoryCultureTestValuesByCountry = new Dictionary<string, string>
            {
                { "AF", "ps-af" },
                { "AD", "ca-es,en-US" },
                { "AX", "sv-se,en-US,en-us" },
                { "BE", "nl-BE,fr-be,nl-be" },
                { "ES", "ca-ES,eu-ES,gl-ES,ca-es,eu-es,gl-es" },
                { "TJ", "tg-cyrl-tj" },
                { "RS", "sr-latn-rs,sr-cyrl-rs" },
            };

            Dictionary<string, string> mandatoryLanguageTestValuesByCountry = new Dictionary<string, string>
            {
                { "ME", "sr-Latn-CS,sr-Cyrl,SR-LATN-CS,SR-CYRL,EN" },
                { "MA", "fr,ar,AR,FR" },
                { "NR", "EN" },
                { "NL", "NL,en" },
                { "NO", "no,NB" },
                { "TL", "pt,PT" },
                { "WL", "EN" },
                { "AO", "pt-PT,PT-PT" },
                { "ES", "es,ca,eu,gl,CA,EU,GL" },
            };

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", "PXIncludeCultureAndLanguageTransformation"
                }
            };

            foreach (string country in countryList)
            {
                string url = $"/v7.0/Account001/profileDescriptions?country={country}&language=en-US&type=employee&partner=commercialstores&operation=add";

                // Act
                var pidls = await GetPidlFromPXService(url, HttpStatusCode.OK, additionaHeaders: headers);

                // Assert
                Assert.IsNotNull(pidls, "Pidls is expected to be not null");

                ValidatePropertyTransformationRegex(pidls[0], "forSubmit", "culture", negativeTestValuesForCulture, mandatoryCultureTestValuesByCountry.ContainsKey(country) ? mandatoryCultureTestValuesByCountry[country] : null);
                ValidatePropertyTransformationRegex(pidls[0], "forSubmit", "language", negativeTestValuesForLanguage, mandatoryLanguageTestValuesByCountry.ContainsKey(country) ? mandatoryLanguageTestValuesByCountry[country] : null);
            }
        }

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
        public async Task GetProfileDescription_ValidateRegEx_FirstName(string partner, string value, bool isValid)
        {
            // Arrange
            string[] profileTypes = new string[]
            {
                Constants.ProfileTypes.Organization
            };

            foreach (string type in profileTypes)
            {
                string url = $"/v7.0/Account001/profileDescriptions?partner={partner}&language=en-US&type={type}&operation=update&country=us";

                // Act
                List<PIDLResource> pidls = await GetPidlFromPXService(url);

                // Assert
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");
                ValidatePidlPropertyRegex(pidls[0], "first_name", value, isValid);
            }
        }

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
        public async Task GetProfileDescription_ValidateRegEx_LastName(string partner, string value, bool isValid)
        {
            // Arrange
            string[] profileTypes = new string[]
            {
                Constants.ProfileTypes.Organization,
            };

            foreach (string type in profileTypes)
            {
                string url = $"/v7.0/Account001/profileDescriptions?partner={partner}&language=en-US&type={type}&operation=update&country=us";

                // Act
                List<PIDLResource> pidls = await GetPidlFromPXService(url);

                // Assert
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");
                ValidatePidlPropertyRegex(pidls[0], "last_name", value, isValid);
            }
        }

        [DataRow("officesmb", "us")]
        [DataRow("officesmb", "es")]
        [DataRow("officesmb", "pt")]
        [DataRow("officesmb", "fr")]
        [DataRow("northstarweb", "fr")]
        [DataTestMethod]
        public async Task GetProfileDescriptions_RemoveDataSource(string partner, string country)
        {
            // Arrage
            string url = $"/v7.0/Account001/profileDescriptions?type=consumer&partner={partner}&operation=update&country={country}&language=en-US";
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
                    expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"features\":{\"removeDataSource\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"removeDataSourceResources\":[\"profile\"]}]}}},\"update\":{\"template\":\"defaulttemplate\",\"features\":{\"removeDataSource\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"removeDataSourceResources\":[\"profile\"]}]}}}}";
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
        
        [DataRow("officesmb")]
        [DataRow("commercialstores")]
        [DataRow(GlobalConstants.Partners.SmboobeModern)]
        [DataTestMethod]
        public async Task GetProfileDescription_ValidateRegEx_PhoneNumber(string partner)
        {
            // Arrange
            string[] countries = new string[] { "us", "cn", "ca", "de", "gb" };
            string[] profileTypes = new string[]
            {
                Constants.ProfileTypes.Organization,
                Constants.ProfileTypes.Employee,
            };

            foreach (string country in countries)
            {
                foreach (string type in profileTypes)
                {
                    if (string.Equals(partner, "officesmb", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(partner, GlobalConstants.Partners.SmboobeModern, StringComparison.OrdinalIgnoreCase))
                    {
                        string featureName = type == "employee" ? "useEmployeeProfileJarvis" : "useProfileUpdateToHapi";
                        string expectedPSSResponse = "{\"update\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"fullPage\",\"resources\":{\"profile\":{\"employee\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"fullPage\"},\"organization\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"fullPage\"}}},\"features\":{\"" + featureName + "\":{\"applicableMarkets\":[]}}}}";                        
                        PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
                    }

                    string url = $"/v7.0/Account001/profileDescriptions?partner={partner}&language=en-US&type={type}&operation=update&country={country}";

                    // Act
                    List<PIDLResource> pidls = await GetPidlFromPXService(url);

                    // Assert
                    Assert.IsNotNull(pidls, "Pidl is expected to be not null");

                    // Not checking if the key exists before accessing is intentional. If the country key is not,
                    // the test should fail and dictoionary should be updated with country value.
                    List<string> validPhoneNumbers = Constants.TestValidPhoneNumbersByCountry[country];
                    foreach (string validPhoneNumber in validPhoneNumbers)
                    {
                        ValidatePidlPropertyRegex(pidls[0], "phone_number", validPhoneNumber, true);
                    }

                    // If the country key is not present then use the common invalid phone numbers
                    List<string> invalidPhoneNumbers = Constants.TestInvalidPhoneNumbersByCountry.ContainsKey(country) ? Constants.TestInvalidPhoneNumbersByCountry[country] : Constants.TestInvalidPhoneNumbersByCountry["common"];
                    foreach (string invalidPhoneNumber in invalidPhoneNumbers)
                    {
                        ValidatePidlPropertyRegex(pidls[0], "phone_number", invalidPhoneNumber, false);
                    }

                    PXSettings.PartnerSettingsService.ResetToDefaults();
                }
            }
        }

        [DataRow("commercialstores", "defaulttemplate", true, true, true)]
        [DataRow("commercialstores", "defaulttemplate", true, false, true)]
        [DataRow("commercialstores", "commercialstores", false, false, true)]
        [DataRow("officesmb", "defaulttemplate", true, true, true)]
        [DataRow("officesmb", "defaulttemplate", true, false, false)]
        [TestMethod]
        public async Task ShowProfileDescription_AddUpdatePartnerActionToEditProfileHyperlink(string partner, string template, bool usePSS, bool useFeature, bool shouldHaveActionContext)
        {
            // Arrange
            string url = $"/v7.0/Account001/profileDescriptions?country=us&type=legalentity&language=en-US&partner={partner}&operation=show";

            var headers = new Dictionary<string, string>();
            if (usePSS)
            {
                string defaultPSSResponse = "{\"default\":{\"template\":\"" + template + "\",\"redirectionPattern\":null,\"resources\":null,\"features\":null}}";
                string featureResponse = "{\"show\":{\"template\":\"" + template + "\",\"redirectionPattern\":\"inline\",\"resources\":{\"profile\":{\"legalentity\":{\"template\":\"" + template + "\",\"redirectionPattern\":\"inline\"}}},\"features\":{\"addUpdatePartnerActionToEditProfileHyperlink\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null}}}}";

                PXSettings.PartnerSettingsService.ArrangeResponse(useFeature ? featureResponse : defaultPSSResponse);

                headers.Add("x-ms-flight", "PXUsePartnerSettingsService,PXDisablePSSCache");
            }

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url: url, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls, "List of PIDLResources should not be null.");

            var pidl = pidls.First();
            Assert.IsNotNull(pidl, "First pidl should not be null");

            var displayHint = pidl.GetDisplayHintById("updateSoldToProfileLink");
            Assert.IsNotNull(displayHint, "displayHint var should not be null.");

            var displayHintAction = displayHint.Action;
            Assert.IsNotNull(displayHintAction, "displayHintAction var should not be null.");
            Assert.IsTrue(string.Equals(displayHintAction.ActionType, "partnerAction", StringComparison.OrdinalIgnoreCase));

            var actionContext = JsonConvert.DeserializeObject<ActionContext>(JsonConvert.SerializeObject(displayHintAction.Context));

            if (shouldHaveActionContext)
            {
                Assert.IsNotNull(actionContext, "actionContext var should NOT be null");
                Assert.IsTrue(string.Equals(actionContext.Action, PIActionType.UpdateResource.ToString(), StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                Assert.IsNull(actionContext, "actionContext var SHOULD be null");
            }
        }

        [DataTestMethod]
        public async Task AddProfile_ConsumerV3_PATCH_UsingPartnerSettingsService()
        {
            string url = $"/v7.0/Account001/ProfileDescriptions?type=consumerV3&partner=webblendsFamily&operation=Add&country=us&language=en-US";
            string expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"features\":{\"addressValidation\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"customizeSubmitButtonContext\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"operation\":\"PATCH\",\"endpoint\":\"my-family\",\"buttonDisplayHintId\":\"saveButton\",\"profileType\":\"consumerV3\"}]},\"customizeProfileForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"dataFieldsToRemoveFromPayload\":[\"profileType\",\"profileCountry\",\"profileResource_id\",\"profileOperation\"],\"profileType\":\"consumerV3\"},{\"dataFieldsToRemoveFromPayload\":[\"addressType\",\"addressCountry\",\"addressOperation\"],\"dataFieldsToRemoveFullPath\":\"default_address\",\"profileType\":\"consumerV3\"},{\"profileType\":\"consumerV3\",\"convertProfileTypeTo\":\"consumer\"}]}}},\"update\":{\"template\":\"defaulttemplate\",\"features\":{\"addressValidation\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null},\"customizeSubmitButtonContext\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"operation\":\"PATCH\",\"endpoint\":\"my-family\",\"profileType\":\"consumerV3\"}]},\"customizeProfileForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"dataFieldsToRemoveFromPayload\":[\"profileType\",\"profileCountry\",\"profileResource_id\",\"profileOperation\"]},{\"dataFieldsToRemoveFromPayload\":[\"addressType\",\"addressCountry\",\"addressOperation\"],\"dataFieldsToRemoveFullPath\":\"default_address\"},{\"convertProfileTypeTo\":\"consumer\"}]}}}}";

            Dictionary<string, string> testHeader = new Dictionary<string, string>()
            {
                { "x-ms-flight", "PXUsePartnerSettingsService,PXDisablePSSCache" },
            };

            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK, null, testHeader);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");

            // PIDL check
            var pidlPages = pidls[0].DisplayPages;
            Assert.AreEqual(pidlPages.Count, 1);

            var pidlPage = pidlPages[0];
            Assert.AreEqual(pidlPage.HintId, "profileConsumerV3DisplayDescriptionPage");
            Assert.AreEqual(pidlPage.DisplayName, "AddressDetailsPage");
            Assert.AreEqual(pidlPage.Members.Count, 12);

            var cancelSaveGroup = pidlPage.Members[11] as GroupDisplayHint;
            Assert.IsTrue(cancelSaveGroup.IsSumbitGroup);
            Assert.AreEqual(cancelSaveGroup.HintId, "cancelSaveGroup");
            Assert.AreEqual(cancelSaveGroup.Members.Count, 2);

            var saveButton = cancelSaveGroup.Members[1] as ButtonDisplayHint;
            Assert.AreEqual(saveButton.HintId, "saveButton");

            var saveButtonContext = saveButton.Action.Context;
            var contextString = saveButtonContext.ToString();
            Assert.IsNotNull(saveButtonContext);
            Assert.IsTrue(contextString.Contains("https://{pifd-endpoint}/anonymous/addresses/ModernValidate?type=internal&partner=defaulttemplate&language=en-US&scenario=suggestAddressesTradeAVSUsePidlPageV2&country=us"));

            // customizeSubmitButtonContext feature checks
            var saveButtonNextAction = saveButton.Action.NextAction;
            var nextActionString = saveButtonNextAction.Context.ToString();
            Assert.IsNotNull(saveButtonNextAction);
            Assert.IsTrue(nextActionString.Contains("\"method\": \"PATCH\""));
            Assert.IsTrue(nextActionString.Contains("https://{jarvis-endpoint}/JarvisCM/my-family/profiles/{partnerData.prefillData.childProfileId}"));

            // customizeProfileForm feature checks
            var dataDescription = pidls[0].DataDescription;
            Assert.IsTrue(dataDescription.ContainsKey("type"));
            Assert.IsTrue(dataDescription.ContainsKey("default_address"));
            Assert.IsTrue((dataDescription["type"] as PropertyDescription).DefaultValue.Equals("consumer"));
            Assert.IsFalse(dataDescription.ContainsKey("profileType"));
            Assert.IsFalse(dataDescription.ContainsKey("profileCountry"));
            Assert.IsFalse(dataDescription.ContainsKey("profileResource_id"));
            Assert.IsFalse(dataDescription.ContainsKey("profileOperation"));

            var defaultAddress = (dataDescription["default_address"] as List<PIDLResource>).First();
            var defaultAddressDataDescription = defaultAddress.DataDescription;
            var type = defaultAddress.Identity["description_type"];
            Assert.IsNotNull(defaultAddress);
            Assert.IsNotNull(defaultAddressDataDescription);
            Assert.AreEqual(type, "address");
            Assert.IsFalse(defaultAddressDataDescription.ContainsKey("addressType"));
            Assert.IsFalse(defaultAddressDataDescription.ContainsKey("addressCountry"));
            Assert.IsFalse(defaultAddressDataDescription.ContainsKey("addressOperation"));

            PXSettings.PartnerSettingsService.ResetToDefaults();
        }

        [DataRow("employee", "commercialstores", "add")]
        [DataRow("employee", "commercialstores", "add", true, true, new string[] { "companyName" }, new string[] { "profileEmployeeCompanyNameProperty" })]
        [DataRow("employee", "commercialstores", "update")]
        [DataRow("employee", "commercialstores", "update", true, true, new string[] { "lastName", "firstName" }, new string[] { "addressLastName", "addressFirstName" })]
        [DataRow("organization", "commercialstores", "update")] // organization only supports update.
        [DataRow("organization", "commercialstores", "update", true, true, new string[] { "companyName" }, new string[] { "profileOrganizationCompanyNameProperty" })] // organization only supports update. commercialstores partner logic will disable this field without feature.
        [DataRow("organization", "commercialstores", "update", false, true, new string[] { "companyName" }, new string[] { "profileOrganizationCompanyNameProperty" })] // organization only supports update
        [DataRow("organization", "commercialstores", "update", true, true, new string[] { "firstName", "lastName" }, new string[] { "addressFirstName", "addressLastName" })]
        [DataTestMethod]
        public async Task GetProfileDescriptions_Feature_CustomizeProfileForm_FieldsToBeDisabled(string profileType, string partnerName, string operation, bool useFeature = false, bool shouldDisableField = false, IEnumerable<string> fieldsToBeDisabled = null, IEnumerable<string> disabledFieldHintIds = null)
        {
            // Arrange
            string url = $"/v7.0/Account001/profileDescriptions?country=us&language=en-US&type={profileType}&partner={partnerName}&operation={operation}";

            var headers = new Dictionary<string, string>()
            {
                { "x-ms-flight", "PXUsePartnerSettingsService" },
            };

            string fieldsToBeDisabledJsonString = null;
            if (useFeature)
            {
                fieldsToBeDisabledJsonString = "[\"" + string.Join("\",\"", fieldsToBeDisabled) + "\"]"; // convert to json array string
            }

            string featureResponse = "{\"" + operation + "\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null,\"resources\":null,\"features\":{\"customizeProfileForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"fieldsToBeDisabled\":" + fieldsToBeDisabledJsonString + ",\"profileType\":\"" + profileType + "\"}]}}}}";
            string defaultPSSResponse = "{\"default\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":null,\"resources\":null,\"features\":null}}";
            
            PXSettings.PartnerSettingsService.ArrangeResponse(useFeature ? featureResponse : defaultPSSResponse);

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");

            if (fieldsToBeDisabled != null && fieldsToBeDisabled.Count() > 0)
            {
                foreach (var field in disabledFieldHintIds)
                {
                    foreach (PIDLResource pidl in pidls)
                    {
                        DisplayHint displayHint = pidl.GetDisplayHintById(field);
                        Assert.IsNotNull(displayHint, $"DisplayHint for {field} should not be null.");
                        Assert.AreEqual(shouldDisableField, displayHint.IsDisabled.HasValue, $"DisplayHint for {field}.IsDisabled should = {shouldDisableField} but the value was {displayHint.IsDisabled.ToString()}.");
                    }
                }
            }
        }
    }
}
