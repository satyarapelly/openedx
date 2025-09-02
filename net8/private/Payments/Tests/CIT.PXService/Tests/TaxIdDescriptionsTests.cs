// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>
namespace CIT.PXService.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using global::Tests.Common.Model.Pidl;    
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class TaxIdDescriptionsTests : TestBase
    {
        /// <summary>
        /// Test is to validate Tax ID Regex
        /// </summary>
        /// <param name="country"></param>
        /// <param name="validTaxIdInputs"></param>
        /// <param name="invalidTaxIdInputs"></param>
        /// <returns></returns>
        [DataRow("il", "123456789", "9867453")]
        [DataRow("np", "123456789", "98765432")]
        [DataRow("gt", "1234567-1", "123456-12")]
        [DataRow("tr", "4340050485", "12345678")]
        [DataRow("am", "02673166/1", "265876984")]
        [DataRow("mx", "P&G851223B24", "1233445")]
        [DataRow("la", "123456789123", "A123456B")]
        [DataRow("ug", "1000029336", "A123456789")]
        [DataRow("ci", "CI1234567A", "CI12345678")]
        [DataRow("gh", "C1234567890", "9867453")]
        [DataRow("sn", "1234567890123", "12309874568977")]
        [DataRow("zm", "1001234567", "123456789A")]
        [DataRow("zm", "1001234567", "12345678901")]
        [DataRow("by", "190190190", "98456123300")]
        [DataRow("md", "1234567", "123456,AAAAAAA")]
        [DataRow("ng", "01012345-0001", "01012345")]
        [DataRow("sg", "200304231M", "123456789012")]
        [DataRow("ge", "123456789", "1234567890,1,A")]
        [DataRow("gh", "C1234567890", "C12345678901")]
        [DataRow("in", "ABCTY1234D", "ABCTY1234D344")]
        [DataRow("bb", "1234567891234", "24576545791")]
        [DataRow("kz", "910740000153", "12309874568977")]
        [DataRow("no", "NO957485030 MVA", "MVA123456778")]
        [DataRow("is", "12345,123456", "12345dd,6,AAAAA")]
        [DataRow("tj", "123456789", "12345678,1234567890")]
        [DataRow("zw", "12345678", "1234567,AAAAAAAAAAAAA")]
        [DataRow("ae", "123456789012345", "12345678901234")]
        [DataRow("sa", "123456789012345", "12345678901234")]
        [DataRow("om", "OM1234567890", "OM123456789,MO1234567890")]
        [DataRow("ph", "123-456-789,123-456-789-012", "1234-56789")]
        [DataRow("vn", "1234567890,1234567890-123", "12345678901-2")]
        [DataRow("ke", "P052005205K,P000000000c", "P052005205,A000000000c")]
        [DataRow("ua", "123456789012,1234567890,123456789", "1234567890123")]
        [DataRow("fj", "11-57252-0-3,99-99999-9-9", "123-45678-90,AB-CDEFG-H-I")]
        [DataRow("co", "123456789-1,123.123.123-1,1234567", "12345-1,123.13.123-1,12345")]
        [DataRow("kh", "B105-902401197,B123-123456789,L001-123456789,B012-123456789,K012-123456789,E012-0123456789", "B201-123456789,B105-1234567890,A123-123456789,11L001-123456789,1B2105-123456789,E012-123456789")]
        [DataTestMethod]
        public async Task GetTaxIdDescriptions_ValidateTaxIdRegEx_UsingPartnerSettings(string country, string validTaxIdInputs, string invalidTaxIdInputs)
        {
            // Arrange
            var validTaxIds = validTaxIdInputs.Split(',');
            var invalidTaxIds = invalidTaxIdInputs.Split(',');
            var operations = new List<string> { "add", "update" };
            var scenarios = new List<string> { "departmentalPurchase", "withCountryDropdown" };
            Dictionary<string, string> headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };
            string expectedPSSResponse = "{\"add\":{\"template\":\"onepage\",\"features\":{\"addressValidation\":{\"applicableMarkets\":[]}}},\"update\":{\"template\":\"onepage\",\"features\":{\"addressValidation\":{\"applicableMarkets\":[]}}}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            foreach (string operation in operations)
            {
                foreach (string scenario in scenarios)
                {
                    string url = $"/v7.0/Account001/taxidDescriptions?scenario={scenario}&country={country}&operation={operation}&language=en-US&type=commercial_tax_id&partner=onepage";

                    // Act
                    List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

                    // Assert
                    Assert.IsNotNull(pidls, "Pidl is expected to be not null");

                    foreach (var validTaxId in validTaxIds)
                    {
                        ValidatePidlPropertyRegex(pidls[0], "taxId", validTaxId, true);
                    }

                    foreach (var invalidTaxId in invalidTaxIds)
                    {
                        ValidatePidlPropertyRegex(pidls[0], "taxId", invalidTaxId, false);
                    }
                }
            }
        }

        /// <summary>
        /// Test is to validate Tax ID form
        /// </summary>
        /// <param name="country"></param>
        /// <param name="pidlCounter"></param>
        /// <param name="isOptional"></param>
        /// <param name="expectedTaxIdDisplayText"></param>
        /// <returns></returns>
        [DataRow("np", 2, false, "VAT")]
        [DataRow("tr", 2, false, "VKN")]
        [DataRow("am", 2, false, "AAH")]
        [DataRow("mx", 2, false, "RFC")]
        [DataRow("by", 2, false, "NDS")]
        [DataRow("kz", 2, false, "RUT")]
        [DataRow("co", 2, false, "NIT")]
        [DataRow("gt", 2, false, "NIT")]
        [DataRow("la", 2, false, "TIN")]
        [DataRow("ug", 2, false, "TIN")]
        [DataRow("gh", 2, false, "TIN")]
        [DataRow("bb", 2, false, "TIN")]
        [DataRow("ph", 2, false, "TIN")]
        [DataRow("vn", 2, false, "TIN")]
        [DataRow("fj", 2, false, "TIN")]
        [DataRow("bs", 2, false, "TIN")]
        [DataRow("il", 2, false, "Ma'am")]
        [DataRow("no", 2, false, "Orgnr")]
        [DataRow("md", 2, false, "VAT ID")]
        [DataRow("ng", 2, false, "VAT ID")]
        [DataRow("ge", 2, false, "VAT ID")]
        [DataRow("is", 2, false, "VAT ID")]
        [DataRow("tj", 2, false, "VAT ID")]
        [DataRow("zw", 2, false, "VAT ID")]
        [DataRow("ae", 2, false, "VAT ID")]
        [DataRow("sa", 2, false, "VAT ID")]
        [DataRow("om", 2, false, "VAT ID")]
        [DataRow("ke", 2, false, "VAT ID")]
        [DataRow("ua", 2, false, "VAT ID")]
        [DataRow("sg", 2, false, "GST/VAT ID")]
        [DataTestMethod]
        public async Task GetTaxIdDescriptions_ValidateTaxForm_UsingPartnerSettings(string country, int pidlCounter, bool isOptional, string expectedTaxIdDisplayText)
        {
            // Arrange
            var operations = new List<string> { "add", "update" };
            var scenarios = new List<string> { "departmentalPurchase", "withCountryDropdown" };
            string expectedPSSResponse = "{\"add\":{\"template\":\"onepage\",\"features\":null},\"update\":{\"template\":\"onepage\",\"features\":null}}";

            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            foreach (string operation in operations)
            {
                foreach (string scenario in scenarios)
                {
                    await ValidateTaxIdForm(country, "onepage", operation, scenario, pidlCounter, isOptional, expectedTaxIdDisplayText, "PXDisablePSSCache");
                }
            }
        }

        /// <summary>
        /// Test is to validate Data Source property
        /// </summary>
        /// <param name="country"></param>
        /// <returns></returns>       
        [DataRow("tr")]
        [DataRow("gb")]
        [DataRow("br")]
        [DataRow("mx")]
        [DataRow("by")]
        [DataRow("kz")]
        [DataRow("co")]
        [DataRow("gt")]
        [DataRow("la")]
        [DataRow("ug")]
        [DataRow("bb")]
        [DataRow("ph")]
        [DataRow("vn")]
        [DataRow("fj")]
        [DataRow("bs")]
        [DataRow("il")]
        [DataRow("no")]
        [DataRow("md")]
        [DataRow("ng")]
        [DataRow("ge")]
        [DataRow("gh")]
        [DataRow("is")]
        [DataRow("tj")]
        [DataRow("zw")]
        [DataRow("ae")]
        [DataRow("sa")]
        [DataRow("om")]
        [DataRow("ke")]
        [DataRow("ua")]
        [DataRow("sg")]
        [DataRow("ci")]
        [DataRow("gh")]
        [DataRow("sn")]
        [DataRow("zm")]
        [DataTestMethod]
        public async Task GetTaxIdDescriptions_ValidateDataSourceProperty_UsingPartnerSettings(string country)
        {
            // Arrange
            var operations = new List<string> { "add", "update" };
            var scenarios = new List<string> { "departmentalPurchase", "withCountryDropdown" };
            Dictionary<string, string> headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };
            string expectedPSSResponse = "{\"add\":{\"template\":\"onepage\",\"features\":null},\"update\":{\"template\":\"onepage\",\"features\":null}}";

            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            foreach (string operation in operations)
            {
                foreach (string scenario in scenarios)
                {
                    string url = $"/v7.0/Account001/taxidDescriptions?language=en-US&type=commercial_tax_id&partner=onepage&country={country}&operation={operation}&scenario={scenario}";

                    // Act
                    List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

                    // Assert
                    Assert.IsNull(pidls[0].DataSources, "Datasource should be null.");
                }
            }
        }

        [DataRow("defaulttemplate", "add")]
        [DataRow("defaulttemplate", "update")]
        [DataTestMethod]
        public async Task GetTaxIdDescriptions_ValidateFormDesign(string partner, string operation)
        {
            // Arrange
            var countries = new string[] { "br", "pt", "tw", "in", "kr", "li", "no", "za", "ch", "at", "be", "bg", "hr", "ci", "cy", "cz", "dk", "ee", "eg", "fi", "fr", "de", "gr", "hu", "ie", "it", "lv", "lt", "lu", "mt", "nl", "pl", "ro", "sk", "si", "es", "se", "sn", "gb", "im", "mc", "au", "nz", "ae", "sa", "tr", "bs", "rs", "co", "am", "by", "bd", "my", "cl", "mx", "id", "th", "bh", "cm", "ge", "gh", "is", "ke", "md", "ng", "om", "tj", "ua", "uz", "zw", "fj", "gt", "kh", "ph", "vn", "bb", "il", "kz", "la", "np", "sg", "ug", "zm" };
            var scenarios = new List<string> { string.Empty, "departmentalPurchase", "withCountryDropdown" };

            foreach (string country in countries)
            {
                foreach (string scenario in scenarios)
                {
                    string url = $"/v7.0/Account001/taxidDescriptions?language=en-US&type=commercial_tax_id&partner={partner}&country={country}&operation={operation}";

                    if (!string.IsNullOrEmpty(scenario))
                    {
                        url += $"&scenario={scenario}";
                    }
                    else if (string.Equals(operation, "update"))
                    {
                        continue;
                    }

                    // Act
                    List<PIDLResource> pidls = await GetPidlFromPXService(url);

                    // Assert

                    // All forms should have *Required group and privacy statement
                    TestContext.WriteLine($"Url: {url}");
                    var startRequired = pidls[0].GetDisplayHintById("starRequiredTextGroup") as TextGroupDisplayHint;
                    Assert.IsNotNull(startRequired, $"starRequiredTextGroup is expected in taxId form");

                    var privacyNotice = pidls[0].GetDisplayHintById("microsoftPrivacyTextGroup") as TextGroupDisplayHint;
                    Assert.IsNotNull(privacyNotice, $"microsoftPrivacyTextGroup is expected in taxId form");
                }
            }
        }

        /// <summary>
        /// Test is to validate country is disabled or not
        /// </summary>
        /// <param name="country"></param>
        /// <returns></returns>
        [DataRow("np")]
        [DataRow("tr")]
        [DataRow("am")]
        [DataRow("mx")]
        [DataRow("by")]
        [DataRow("kz")]
        [DataRow("co")]
        [DataRow("gt")]
        [DataRow("la")]
        [DataRow("ug")]
        [DataRow("bb")]
        [DataRow("ph")]
        [DataRow("vn")]
        [DataRow("fj")]
        [DataRow("il")]
        [DataRow("no")]
        [DataRow("in")]
        [DataRow("md")]
        [DataRow("ng")]
        [DataRow("ge")]
        [DataRow("gh")]
        [DataRow("is")]
        [DataRow("tj")]
        [DataRow("zw")]
        [DataRow("ae")]
        [DataRow("sa")]
        [DataRow("om")]
        [DataRow("ke")]
        [DataRow("ua")]
        [DataRow("sg")]
        [DataRow("it")]
        [DataRow("eg")]
        [DataRow("ci")]
        [DataRow("gh")]
        [DataRow("sn")]
        [DataRow("zm")]
        [DataTestMethod]
        public async Task GetTaxIdDescriptions_ValidateCountryIsDisabledOrNotWithCountryDropdown_UsingPartnerSettings(string country)
        {
            // Arrange
            var operations = new List<string> { "add", "update" };
            string[] buttonIds = { "saveButton", "cancelButton", "saveButtonSuccess" };
            Dictionary<string, string> headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };
            string expectedPSSResponse = "{\"add\":{\"template\":\"onepage\",\"features\":null},\"update\":{\"template\":\"onepage\",\"features\":null}}";

            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            foreach (string operation in operations)
            {
                string url = $"/v7.0/Account001/taxidDescriptions?operation={operation}&country={country}&language=en-US&type=commercial_tax_id&partner=onepage&scenario=withCountryDropdown";

                // Act
                List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

                // Assert
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");

                foreach (PIDLResource pidl in pidls)
                {
                    DisplayHint countryDisplayDescription = pidl.GetDisplayHintById("hapiTaxCountryProperty");
                    Assert.IsNotNull(countryDisplayDescription, "country property is expected to be not null");
                    Assert.IsTrue(countryDisplayDescription.IsDisabled ?? true, "Country property is invalid");

                    foreach (string buttonId in buttonIds)
                    {
                        DisplayHint buttonDisplayDescription = pidl.GetDisplayHintById(buttonId);

                        if (buttonDisplayDescription != null)
                        {
                            Assert.IsTrue(buttonDisplayDescription.IsHidden ?? false, buttonId + " should be hidden with scenario 'withCountryDropdown'");
                        }
                    }

                    if (string.Equals(country, "in", StringComparison.OrdinalIgnoreCase))
                    {                    
                        List<PIDLResource> linkedPidls = pidl.LinkedPidls;
                        Assert.IsNotNull(linkedPidls, "LinkedPidls is expected to be not null");

                        DisplayHint stateDisplayDescription = linkedPidls[0].GetDisplayHintById("addressState");
                        Assert.IsNotNull(stateDisplayDescription, "state property is expected to be not null");

                        if (string.Equals(operation, Constants.OperationTypes.Add, StringComparison.OrdinalIgnoreCase))
                        {
                            Assert.IsNull(stateDisplayDescription.IsDisabled);
                        }                            
                        else if (string.Equals(operation, Constants.OperationTypes.Update, StringComparison.OrdinalIgnoreCase))
                        {
                            Assert.IsTrue(stateDisplayDescription.IsDisabled ?? false, "state property is expected to be disabled");
                        }                            
                    }
                }
            }
        }

        /// <summary>
        /// Test is to validate linked Tax PIDLs
        /// </summary>
        [DataRow("in", false)]
        [DataRow("in", true, null, true, "clientData")]
        [DataRow("in", false, "PXEnabledNoSubmitIfGSTIDEmpty", true, "clientData")]
        [DataRow("in", true, "PXEnabledNoSubmitIfGSTIDEmpty", true, "clientData")]
        [DataRow("it", false)]
        [DataRow("it", true)]
        [DataRow("it", false, "enableItalyCodiceFiscale")]
        [DataRow("eg", false, null, true)]        
        [DataRow("eg", false, "PXEnableEGTaxIdsRequired", false)]
        [DataTestMethod]
        public async Task GetTaxIdDescriptions_ValidateLinkedTaxPidls_UsingPartnerSettings(string country, bool enableNoSubmitIfGSTIDEmptyFeature, string flight = null, bool isOptional = false, string propertyType = "userData")
        {
            // Arrange
            var operations = new List<string> { "add", "update" };
            var scenarios = new List<string> { "departmentalPurchase", "withCountryDropdown" };
            Dictionary<string, string> headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };
            string expectedPSSResponse = "{\"add\":{\"template\":\"onepage\",\"features\":null},\"update\":{\"template\":\"onepage\",\"features\":null}}";
            expectedPSSResponse = enableNoSubmitIfGSTIDEmptyFeature ? "{\"add\":{\"features\": {\"noSubmitIfGSTIDEmpty\": {\"applicableMarkets\": [\"in\"]}}, \"template\": \"onepage\"},\"update\":{\"template\":\"onepage\",\"features\":null}}" : expectedPSSResponse;

            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            foreach (string operation in operations)
            {
                foreach (string scenario in scenarios)
                {
                    if (flight != null)
                    {
                        PXFlightHandler.AddToEnabledFlights(flight);
                    }
                    else
                    {
                        PXFlightHandler.ResetToDefault();
                    }

                    string url = $"/v7.0/Account001/taxidDescriptions?scenario={scenario}&operation={operation}&country={country}&language=en-US&type=commercial_tax_id&partner=onepage";

                    // Act
                    List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

                    // Assert
                    Assert.IsNotNull(pidls, "Pidl is expected to be not null");

                    List<PIDLResource> linkedPidls = pidls[0].LinkedPidls;

                    if (string.Equals(country, "in", StringComparison.OrdinalIgnoreCase))
                    {
                        // The flight PXEnabledNoSubmitIfGSTIDEmpty is enabled only for add operation
                        if (string.Equals(operation, Constants.OperationTypes.Update, StringComparison.OrdinalIgnoreCase))
                        {
                            isOptional = false;
                            propertyType = "userData";
                        }

                        Assert.IsNotNull(linkedPidls, "India tax pidl should have a linked GST tax pidl");
                        Assert.AreEqual(1, linkedPidls.Count, "Inida tax pidl should have only one linked GST tax pidl");

                        PropertyDescription taxIdDescription = linkedPidls[0].GetPropertyDescriptionByPropertyName("taxId");
                        PropertyDescription typeDescription = linkedPidls[0].GetPropertyDescriptionByPropertyName("type");
                        PropertyDescription stateDescription = linkedPidls[0].GetPropertyDescriptionByPropertyName("state");
                        PropertyDescription countryDescription = linkedPidls[0].GetPropertyDescriptionByPropertyName("country");

                        Assert.AreEqual(taxIdDescription.IsOptional, true, "Taxid's IsOptional property is not correct");
                        Assert.AreEqual(stateDescription.IsOptional, true, "State's IsOptional property is not correct");
                        Assert.AreEqual(typeDescription.IsOptional, isOptional, "Type's IsOptional property is not correct");
                        Assert.AreEqual(countryDescription.IsOptional, isOptional, "Country's IsOptional property is not correct");

                        Assert.AreEqual(taxIdDescription.PropertyType, "userData", "Taxid's PropertyType property is not correct");
                        Assert.AreEqual(typeDescription.PropertyType, "clientData", "Type's PropertyType property is not correct");
                        Assert.AreEqual(stateDescription.PropertyType, propertyType, "State's PropertyType property is not correct");
                        Assert.AreEqual(countryDescription.PropertyType, propertyType, "Country's PropertyType property is not correct");
                    }
                    else if (string.Equals(country, "it", StringComparison.OrdinalIgnoreCase))
                    {
                        // PSS templates are by default enabled for ItalyCodiceFiscale TaxId on GetTaxIdDescriptions
                        Assert.AreEqual("national_identification_number", pidls[0].Identity["type"]);
                        ValidatePidlPropertyRegex(pidls[0], "taxId", "AAAAAA11A11A111A", true);

                        Assert.IsNotNull(linkedPidls, "Italy tax pidl should have a linked Vat ID tax pidl");
                        Assert.AreEqual(1, linkedPidls.Count, "Italy tax pidl should have only one linked Vat ID tax pidl");

                        PropertyDescription taxIdDescription = linkedPidls[0].GetPropertyDescriptionByPropertyName("taxId");
                        Assert.AreEqual(taxIdDescription.IsOptional, true, "Taxid's IsOptional property is not correct");
                    }
                    else if (string.Equals(country, "eg", StringComparison.OrdinalIgnoreCase))
                    {                        
                        Assert.AreEqual("egypt_national_identification_number", pidls[0].Identity["type"]);
                        ValidatePidlPropertyRegex(pidls[0], "taxId", "10000010166d9770abe966h4vk7H9oj0cd7c8a8", true);

                        Assert.IsNotNull(linkedPidls, "Egypt tax pidl should have a linked Vat ID tax pidl");
                        Assert.AreEqual(1, linkedPidls.Count, "Egypt tax pidl should have only one linked Vat ID tax pidl");

                        PropertyDescription taxIdDescription = linkedPidls[0].GetPropertyDescriptionByPropertyName("taxId");                       
                        PropertyDescription typeDescription = linkedPidls[0].GetPropertyDescriptionByPropertyName("type");
                        PropertyDescription countryDescription = linkedPidls[0].GetPropertyDescriptionByPropertyName("country");

                        Assert.AreEqual(taxIdDescription.PropertyType, "userData", "Taxid's PropertyType property is not correct");
                        Assert.AreEqual(typeDescription.PropertyType, "clientData", "Type's PropertyType property is not correct");
                        Assert.AreEqual(countryDescription.PropertyType, propertyType, "Country's PropertyType property is not correct");                        
                        if (string.Equals(flight, Constants.PartnerFlightValues.PXEnableEGTaxIdsRequired, StringComparison.OrdinalIgnoreCase))
                        {
                            Assert.AreEqual(taxIdDescription.IsOptional, false, "Taxid's IsOptional property is not correct");
                        }
                        else
                        {
                            Assert.AreEqual(taxIdDescription.IsOptional, isOptional, "Taxid's IsOptional property is not correct");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Test is to validate customize taxId form
        /// </summary>
        [DataRow("onepage", "it", true)]
        [DataTestMethod]
            public async Task GetTaxIdDescriptions_ValidateCustomizeTaxIdForm_Add_UsingPSS(string partner, string country, bool disableCountryDropDown)
        {
            // Arrange
            var operations = new List<string> { "add" };
            var scenarios = new List<string> { "departmentalPurchase", "withCountryDropdown" };
            Dictionary<string, string> headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };
            string pssResponse = "{\"add\":{\"template\":\"onepage\",\"features\":null},\"update\":{\"template\":\"onepage\",\"features\":null}}";

            string pssResponseWithCountryDisabled = "{\"add\":{\"features\": {\"customizeTaxIdForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"DisableCountryDropdown\":true}]}}, \"template\": \"onepage\"},\"update\":{\"template\":\"onepage\",\"features\":null}}";

            PXSettings.PartnerSettingsService.ArrangeResponse(disableCountryDropDown ? pssResponseWithCountryDisabled : pssResponse);

            foreach (string operation in operations)
            {
                foreach (string scenario in scenarios)
                {
                    PXFlightHandler.ResetToDefault();

                    string url = $"/v7.0/Account001/taxidDescriptions?scenario={scenario}&operation={operation}&country={country}&language=en-US&type=commercial_tax_id&partner={partner}";

                    // Act
                    List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

                    // Assert
                    Assert.IsNotNull(pidls, "Pidl is expected to be not null");

                    DisplayHint countryDisplayHint = pidls[0].GetDisplayHintByPropertyName("country");
                    Assert.AreEqual(disableCountryDropDown, countryDisplayHint.IsDisabled ?? false, "Country should be disabled");

                    foreach (var linkedPidl in pidls[0].LinkedPidls ?? Enumerable.Empty<PIDLResource>())
                    {
                        countryDisplayHint = linkedPidl.GetDisplayHintByPropertyName("country");
                        Assert.AreEqual(disableCountryDropDown, countryDisplayHint.IsDisabled ?? false, "Country should be disabled");
                    }
                }
            }
        }

        /// <summary>
        /// Test is to validate linked Tax PIDLs secondary resource context
        /// </summary>
        [DataRow("azurebmx", "it", true)]
        [DataRow("azurebmx", "it", false)]
        [DataRow("azurebmx", "eg", true)]
        [DataRow("azurebmx", "eg", false)]
        [DataTestMethod]
        public async Task GetTaxIdDescriptions_ValidateLinkedPidl_SecondaryResourceContext(string partner, string country, bool skipSecondaryResourceContext)
        {
            // Arrange
            var operations = new List<string> { "add" };
            var scenarios = new List<string> { "departmentalPurchase", "withCountryDropdown" };
            Dictionary<string, string> headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };
            string pssResponse = "{\"add\":{\"template\":\"onepage\",\"features\":null},\"update\":{\"template\":\"onepage\",\"features\":null}}";

            string pssResponseWithSkipResourceContext = "{\"add\":{\"features\": {\"PXTaxIdFormSkipSecondaryResourceContext\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]}}, \"template\": \"onepage\"},\"update\":{\"template\":\"onepage\",\"features\":null}}";

            PXSettings.PartnerSettingsService.ArrangeResponse(skipSecondaryResourceContext ? pssResponseWithSkipResourceContext : pssResponse);

            foreach (string operation in operations)
            {
                foreach (string scenario in scenarios)
                {
                    PXFlightHandler.ResetToDefault();

                    string url = $"/v7.0/Account001/taxidDescriptions?scenario={scenario}&operation={operation}&country={country}&language=en-US&type=commercial_tax_id&partner={partner}";

                    // Act
                    List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

                    // Assert
                    Assert.IsNotNull(pidls, "Pidl is expected to be not null");

                    foreach (var linkedPidl in pidls[0].LinkedPidls ?? Enumerable.Empty<PIDLResource>())
                    {
                        bool isSecondaryResource = linkedPidl.ScenarioContext?.ContainsKey("resourceType") ?? false;
                        Assert.AreEqual(!skipSecondaryResourceContext, isSecondaryResource, "Secondary resource context should be skipped");
                    }
                }
            }
        }

        [DataRow("in")]
        [DataRow("gb")]
        [DataRow("it")]
        [DataTestMethod]
        public async Task GetTaxIdDescriptions_ValidateHideCountryFieldAndSubmitButton_UsingPartnerSettings(string country)
        {
            // Arrange
            bool buttonIsHidden = true;
            bool countryIsHidden = false;
            var flights = new List<string> { null, "dpHideCountry" };
            var scenarios = new List<string> { "departmentalPurchase", "withCountryDropdown" };
            string expectedPSSResponse = "{\"add\":{\"template\":\"onepage\",\"features\":null}}";
            Dictionary<string, string> headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };

            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            foreach (string flight in flights)
            {
                foreach (string scenario in scenarios)
                {
                    if (flight != null)
                    {
                        buttonIsHidden = false;
                        countryIsHidden = true;
                        headers["x-ms-flight"] += $",{flight}";
                    }

                    string url = $"/v7.0/Account001/taxidDescriptions?language=en-US&type=commercial_tax_id&partner=commercialstores&operation=add&country={country}&scenario={scenario}";

                    // Act
                    List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

                    // Assert
                    Assert.IsNotNull(pidls, "Pidl is expected to be not null");

                    foreach (PIDLResource profilePidl in pidls)
                    {
                        DisplayHint countryDisplayDescription = profilePidl.GetDisplayHintById("hapiTaxCountryProperty");
                        DisplayHint cancelButtonDisplayDescription = profilePidl.GetDisplayHintById("cancelButton");
                        DisplayHint saveButtonDisplayDescription = profilePidl.GetDisplayHintById("saveButton");

                        Assert.IsNotNull(countryDisplayDescription);
                        Assert.IsNotNull(saveButtonDisplayDescription);
                        Assert.IsNotNull(cancelButtonDisplayDescription);
                        Assert.AreEqual(countryDisplayDescription.IsHidden ?? false, countryIsHidden);
                        Assert.AreEqual(saveButtonDisplayDescription.IsHidden ?? false, buttonIsHidden);
                        Assert.AreEqual(cancelButtonDisplayDescription.IsHidden ?? false, buttonIsHidden);
                    }
                }
            }
        }

        /// <summary>
        /// Test is to validate Taiwan's commercial tax pidls
        /// </summary>
        /// <returns></returns>
        [DataTestMethod]
        public async Task GetTaxIdDescriptions_ValidateTWCommercialTaxPidls_UsingPartnerSettings()
        {
            // Arrange
            var operations = new List<string> { "add", "update" };
            var scenarios = new List<string> { "departmentalPurchase", "withCountryDropdown" };
            Dictionary<string, string> headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };
            string expectedPSSResponse = "{\"add\":{\"template\":\"onepage\",\"features\":null},\"update\":{\"template\":\"onepage\",\"features\":null}}";

            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            foreach (string operation in operations)
            {
                foreach (string scenario in scenarios)
                {
                    string url = $"/v7.0/Account001/taxidDescriptions?country=tw&language=en-US&type=commercial_tax_id&partner=onepage&operation={operation}&scenario={scenario}";

                    // Act
                    List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

                    // Assert
                    Assert.IsNotNull(pidls, "Pidl is expected to be not null");
                    Assert.IsTrue(pidls[0].DataDescription.ContainsKey("additionalData"), "TW commercial tax Pidl should have additionalData");

                    List<PIDLResource> additionalData = pidls[0].DataDescription["additionalData"] as List<PIDLResource>;
                    Assert.IsFalse(additionalData == null || additionalData.Count == 0, "TW commercial tax Pidl's additionalData can't be null or empty");
                    Assert.IsFalse(additionalData[0].DataDescription.ContainsKey("dataType"), "This property should be removed from commercial tax Pidl");
                    Assert.IsFalse(additionalData[0].DataDescription.ContainsKey("dataCountry"), "This property should be removed from commercial tax Pidl");
                    Assert.IsFalse(additionalData[0].DataDescription.ContainsKey("dataOperation"), "This property should be removed from commercial tax Pidl");
                    Assert.IsNull(pidls[0].DataSources, "Datasource should be null.");
                }
            }
        }

        /// <summary>
        /// Test is to validate DpHideCountry feature using PSS
        /// </summary>
        /// <param name="country"></param>
        /// <returns></returns>
        [DataRow("np")]
        [DataRow("tr")]
        [DataRow("am")]
        [DataRow("mx")]
        [DataRow("by")]
        [DataRow("kz")]
        [DataRow("co")]
        [DataRow("gt")]
        [DataRow("la")]
        [DataRow("ug")]
        [DataRow("bb")]
        [DataRow("ph")]
        [DataRow("vn")]
        [DataRow("fj")]
        [DataRow("il")]
        [DataRow("no")]
        [DataRow("md")]
        [DataRow("ng")]
        [DataRow("ge")]
        [DataRow("gh")]
        [DataRow("is")]
        [DataRow("tj")]
        [DataRow("zw")]
        [DataRow("ae")]
        [DataRow("sa")]
        [DataRow("ke")]
        [DataRow("ua")]
        [DataRow("sg")]
        [DataRow("in")]
        [DataRow("it")]
        [DataRow("eg")]
        [DataRow("ci")]
        [DataRow("gh")]
        [DataRow("sn")]
        [DataRow("zm")]
        [DataTestMethod]
        public async Task GetTaxIdDescriptions_ValidateDpHideCountry_UsingPartnerSettings(string country)
        {
            // Arrange
            List<string> operations = new List<string> { "add", "update" };
            List<bool> dpHideCountryFeatureStatuses = new List<bool> { true, false };
            List<string> scenarios = new List<string> { "departmentalPurchase", "withCountryDropdown" };
            List<string> buttonDisplayHintIds = new List<string> { "saveButton", "cancelButton", "saveButtonSuccess" };
            Dictionary<string, string> headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };

            foreach (string operation in operations)
            {
                foreach (string scenario in scenarios)
                {
                    foreach (bool dpHideCountryFeatureStatus in dpHideCountryFeatureStatuses)
                    {
                        string featureProperty = dpHideCountryFeatureStatus ? "\"dpHideCountry\":{\"applicableMarkets\":[]}" : null;
                        string expectedPSSResponse = "{\"add\":{\"template\":\"onepage\",\"features\":{" + featureProperty + "}},\"update\":{\"template\":\"onepage\",\"features\":{" + featureProperty + "}}}";
                        string url = $"/v7.0/Account001/taxidDescriptions?operation={operation}&country={country}&language=en-US&type=commercial_tax_id&partner=onepage&scenario={scenario}";

                        PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

                        // Act
                        List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

                        // Assert
                        Assert.IsNotNull(pidls, "Pidl is expected to be not null");

                        foreach (PIDLResource pidl in pidls)
                        {
                            DisplayHint stateDisplayDescription = null;
                            DisplayHint countryDisplayDescription = pidl.GetDisplayHintById("hapiTaxCountryProperty");
                            Assert.IsNotNull(countryDisplayDescription, "Country property is expected to be not null");

                            if (string.Equals(country, "in", StringComparison.OrdinalIgnoreCase))
                            {
                                List<PIDLResource> linkedPidls = pidl.LinkedPidls;
                                Assert.IsNotNull(linkedPidls, "India tax pidl should have a linked GST tax pidl");
                                Assert.AreEqual(linkedPidls.Count, 1, "Inida tax pidl should have only one linked GST tax pidl");

                                stateDisplayDescription = linkedPidls[0].GetDisplayHintById("addressState");
                                Assert.IsNotNull(stateDisplayDescription, "India's state property is expected to be not null");
                            }

                            if (dpHideCountryFeatureStatus)
                            {
                                if (string.Equals(country, "in", StringComparison.OrdinalIgnoreCase))
                                {
                                    Assert.IsTrue(stateDisplayDescription.IsHidden, "India's state property should be hidden");
                                }
                                else if (string.Equals(country, "it", StringComparison.OrdinalIgnoreCase))
                                {
                                    Assert.IsTrue(countryDisplayDescription.IsHidden, "Italy's country property should be hidden");
                                }

                                if (string.Equals(scenario, "withCountryDropdown", StringComparison.OrdinalIgnoreCase) || string.Equals(scenario, "departmentalPurchase", StringComparison.OrdinalIgnoreCase))
                                {
                                    Assert.IsTrue(countryDisplayDescription.IsHidden, "Country property should be hidden");

                                    foreach (string buttonDisplayHintId in buttonDisplayHintIds)
                                    {
                                        DisplayHint buttonDisplayDescription = pidl.GetDisplayHintById(buttonDisplayHintId);
                                        if (buttonDisplayDescription != null)
                                        {
                                            Assert.IsFalse(buttonDisplayDescription.IsHidden, buttonDisplayHintId + " should not be hidden with scenario 'withCountryDropdown'");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (string.Equals(country, "in", StringComparison.OrdinalIgnoreCase))
                                {
                                    Assert.IsNull(stateDisplayDescription.IsHidden, "India's state  property should be null");
                                }
                                else if (string.Equals(country, "it", StringComparison.OrdinalIgnoreCase))
                                {
                                    // Expected due to codicefiscaletax id is enabled for all the template based partners
                                    Assert.IsTrue(countryDisplayDescription.IsHidden, "Italy's country property should be hidden");
                                }

                                if (string.Equals(scenario, "withCountryDropdown", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (string.Equals(operation, "update", StringComparison.OrdinalIgnoreCase))
                                    {
                                        Assert.IsTrue(countryDisplayDescription.IsDisabled, "Country property should be disabled with scenario 'withCountryDropdown'");
                                    }
                                    else
                                    {
                                        Assert.IsNull(countryDisplayDescription.IsDisabled, "Country property should be null");
                                    }

                                    foreach (string buttonDisplayHintId in buttonDisplayHintIds)
                                    {
                                        DisplayHint buttonDisplayDescription = pidl.GetDisplayHintById(buttonDisplayHintId);
                                        if (buttonDisplayDescription != null)
                                        {
                                            Assert.IsTrue(buttonDisplayDescription.IsHidden, buttonDisplayHintId + " should be hidden with scenario 'withCountryDropdown'");
                                        }
                                    }
                                }
                            }
                        }

                        PXSettings.PartnerSettingsService.Responses.Clear();
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
        [DataRow("gh", 2, false)]
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
        [DataRow("co", 2, false)]
        [DataRow("bs", 2, false)]
        [DataRow("sa", 2, false)]
        [DataRow("ae", 2, false)]
        [DataRow("ci", 2, false)]
        [DataRow("sn", 2, false)]
        [DataRow("zm", 2, false)]
        [DataRow("la", 2, false)]
        [DataRow("ae", 2, false)]
        [DataTestMethod]
        public async Task GetTaxIdDescriptions_ModifyTaxIdToMandatory(string country, int pidlCounter, bool isOptional)
        {
            // Arrange
            string url = $"/v7.0/Account001/taxidDescriptions?country={country}&language=en-US&type=commercial_tax_id&partner=commercialstores&operation=add";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");
            Assert.AreEqual(pidls.Count, pidlCounter, "Expected pidl counter does not match the return result");

            PropertyDescription taxIdDescription = pidls[0].GetPropertyDescriptionByPropertyName("taxId");
            Assert.IsNotNull(taxIdDescription, "Taxid property is expected to be not null");
            Assert.AreEqual(taxIdDescription.IsOptional, isOptional, "Taxid's IsOptional property is not correct");
        }

        /// <summary>
        /// This test is used to verify the change in defaultValue for TaxId property when PXSetItalyTaxIdValuesByFunction flight is enabled.
        /// </summary>
        /// <param name="scenario">Scenario name</param>
        /// <param name="flights">Flights name</param>
        /// <returns></returns>
        [DataRow("withCountryDropdown", "enableItalyCodiceFiscale")]
        [DataRow("departmentalPurchase", "enableItalyCodiceFiscale,PXSetItalyTaxIdValuesByFunction")]
        [DataRow("withCountryDropdown", "enableItalyCodiceFiscale,PXSetItalyTaxIdValuesByFunction")]
        [DataTestMethod]
        public async Task GetTaxIdDescriptions_DefaultValueForTaxId(string scenario, string flights)
        {
            // Arrange
            List<string> partners = new List<string> { "azure", "commercialstores" };

            foreach (string partner in partners)
            {
                string url = $"/v7.0/Account001/taxidDescriptions?country=it&language=en-US&type=commercial_tax_id&partner={partner}&operation=update&scenario={scenario}";

                // Act
                List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK, flightNames: flights);

                // Assert
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");
                
                PropertyDescription taxIdDescription = pidls[0].GetPropertyDescriptionByPropertyName("taxId");
                Assert.IsNotNull(taxIdDescription, "Taxid property is expected to be not null");
                Assert.IsNotNull(pidls[0].LinkedPidls, "Italy tax linked pidl is expected to be not null");
                
                PropertyDescription linkedtaxIdDescription = pidls[0].LinkedPidls[0].GetPropertyDescriptionByPropertyName("taxId");
                Assert.IsNotNull(linkedtaxIdDescription, "Taxid property is expected to be not null");

                if (flights.Contains("PXSetItalyTaxIdValuesByFunction") && scenario != null)
                {
                    Assert.AreEqual(taxIdDescription.DefaultValue, "(<|getNationalIdentificationNumber|>)");
                    Assert.AreEqual(linkedtaxIdDescription.DefaultValue, "(<|getVatId|>)");
                }
                else
                {
                    Assert.AreEqual(taxIdDescription.DefaultValue, "({dataSources.taxResource.value[1].taxId})");
                    Assert.AreEqual(linkedtaxIdDescription.DefaultValue, "({dataSources.taxResource.value[0].taxId})");
                }
            }
        }

        /// <summary>
        /// This test is used to verify the change in defaultValue for TaxId property.
        /// </summary>
        /// <param name="scenario">Scenario name</param>
        /// <param name="flights">Flights name</param>
        /// <returns></returns>
        [DataRow("withCountryDropdown", "")]
        [DataRow("departmentalPurchase", "")]
        [DataTestMethod]
        public async Task GetTaxIdDescriptions_DefaultValueForEgyptTaxId(string scenario, string flights)
        {
            // Arrange
            List<string> partners = new List<string> { "azure", "commercialstores" };

            foreach (string partner in partners)
            {
                string url = $"/v7.0/Account001/taxidDescriptions?country=eg&language=en-US&type=commercial_tax_id&partner={partner}&operation=update&scenario={scenario}";

                Dictionary<string, string> headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };

                // Act
                List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK, flightNames: flights, headers);

                // Assert
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");

                PropertyDescription taxIdDescription = pidls[0].GetPropertyDescriptionByPropertyName("taxId");
                Assert.IsNotNull(taxIdDescription, "Taxid property is expected to be not null");
                Assert.IsNotNull(pidls[0].LinkedPidls, "Egypt tax linked pidl is expected to be not null");

                PropertyDescription linkedtaxIdDescription = pidls[0].LinkedPidls[0].GetPropertyDescriptionByPropertyName("taxId");
                Assert.IsNotNull(linkedtaxIdDescription, "Taxid property is expected to be not null");
               
                Assert.AreEqual(taxIdDescription.DefaultValue, "(<|getNationalIdentificationNumber|>)");
                Assert.AreEqual(linkedtaxIdDescription.DefaultValue, "(<|getVatId|>)");                
            }
        }

        /// <summary>
        /// The test is to verify the tax ID RegEx.
        /// </summary>
        /// <param name="country"></param>
        /// <param name="validTaxIdInputs"></param>
        /// <param name="invalidTaxIdInputs"></param>
        /// <returns></returns>
        [DataRow("bb", "1234567891234", "24576545791")]
        [DataRow("il", "123456789", "9867453")]
        [DataRow("kz", "910740000153", "12309874568977")]
        [DataRow("la", "123456789123", "A123456B")]
        [DataRow("np", "123456789", "98765432")]
        [DataRow("sg", "200304231M", "123456789012")]
        [DataRow("ug", "1000029336", "A123456789")]
        [DataRow("am", "02673166/1", "265876984")]
        [DataRow("by", "190190190", "98456123300")]
        [DataRow("mx", "P&G851223B24", "1233445")]
        [DataRow("no", "NO957485030 MVA", "MVA123456778")]
        [DataRow("tr", "4340050485", "12345678")]
        [DataRow("in", "ABCTY1234D", "ABCTY1234D344")]
        [DataRow("bh", "212345678901234", "312345678901234,21,211222222232324242424242,aBcDeFGHIjklMno")]
        [DataRow("cm", "A123456789000A,a123456789000a", "a1234567890000,a1A,a12345678900000000,1111")]
        [DataRow("ge", "123456789", "1234567890,1,A")]
        [DataRow("gh", "C1234567890", "C12345678901")]
        [DataRow("is", "12345,123456", "12345dd,6,AAAAA")]
        [DataRow("ke", "P052005205K,P000000000c", "P052005205,A000000000c")]
        [DataRow("md", "1234567", "123456,AAAAAAA")]
        [DataRow("ng", "01012345-0001", "01012345")]
        [DataRow("om", "OM1234567890", "OM123456789,MO1234567890")]
        [DataRow("tj", "123456789", "12345678,1234567890")]
        [DataRow("ua", "123456789012,1234567890,123456789", "1234567890123")]
        [DataRow("zw", "12345678", "1234567,AAAAAAAAAAAAA")]
        [DataRow("fj", "11-57252-0-3,99-99999-9-9", "123-45678-90,AB-CDEFG-H-I")]
        [DataRow("gt", "1234567-1", "123456-12")]
        [DataRow("kh", "L001-123456789,B012-123456789,K012-123456789,E012-1234567890", "L123456789,B1234556789,A001-00000000")]
        [DataRow("ph", "123-456-789,123-456-789-012", "1234-56789")]
        [DataRow("vn", "1234567890,1234567890-123", "12345678901-2")]
        [DataRow("ae", "123456789012345", "12345678901234")]
        [DataRow("sa", "123456789012345", "12345678901234")]
        [DataRow("co", "123456789-1,123.123.123-1,1234567", "12345-1,123.13.123-1,12345")]
        [DataRow("it", "AAAAAA11A11A111A", "AAAAAA11A11A1112A", "enableItalyCodiceFiscale")]
        [DataRow("it", "12345678901", "1234567890", "enableItalyCodiceFiscale")]
        [DataRow("it", "IT12345678901", "IT1234567890")]
        [DataRow("eg", "10000010166d9770abe966h4vk7H9oj0cd7c8a8", "EG1234567890")]
        [DataRow("ci", "CI1234567A", "CI12345678")]
        [DataRow("gh", "C1234567890", "9867453")]
        [DataRow("sn", "1234567890123", "12309874568977")]
        [DataRow("zm", "1001234567", "123456789A")]
        [DataTestMethod]
        public async Task GetTaxIdDescriptions_ValidateTaxIdRegEx(string country, string validTaxIdInputs, string invalidTaxIdInputs, string flightName = null)
        {
            var validTaxIds = validTaxIdInputs.Split(',');
            var invalidTaxIds = invalidTaxIdInputs.Split(',');
            var operations = new List<string> { "update" };
            var scenarios = new List<string> { "withCountryDropdown" };
            var partners = new List<string> { Constants.PartnerNames.CommercialStores, Constants.PartnerNames.Azure };

            foreach (var partner in partners)
            {
                if (string.Equals(partner, Constants.PartnerNames.CommercialStores))
                {
                    operations.Add("add");
                    scenarios.Add("departmentalPurchase");
                }

                foreach (var operation in operations)
                {
                    foreach (var scenario in scenarios)
                    {
                        string url = $"/v7.0/Account001/taxidDescriptions?country={country}&language=en-US&type=commercial_tax_id&partner={partner}&operation={operation}&scenario={scenario}";

                        List<PIDLResource> pidls = await GetPidlFromPXService(url, flightNames: flightName);
                        Assert.IsNotNull(pidls, "Pidl is expected to be not null");

                        foreach (var validTaxId in validTaxIds)
                        {
                            ValidatePidlPropertyRegex(pidls[0], "taxId", validTaxId, true);
                        }

                        foreach (var invalidTaxId in invalidTaxIds)
                        {
                            ValidatePidlPropertyRegex(pidls[0], "taxId", invalidTaxId, false);
                        }
                    }
                }
            }
        }

        [DataRow("bb", 2, false, "TIN")]
        [DataRow("il", 2, false, "Ma'am")]
        [DataRow("kz", 2, false, "RUT")]
        [DataRow("la", 2, false, "TIN")]
        [DataRow("np", 2, false, "VAT")]
        [DataRow("sg", 2, false, "GST/VAT ID")]
        [DataRow("ug", 2, false, "TIN")]
        [DataRow("tr", 2, false, "VKN")]
        [DataRow("am", 2, false, "AAH")]
        [DataRow("by", 2, false, "NDS")]
        [DataRow("no", 2, false, "Orgnr")]
        [DataRow("bd", 2, false, "BIN")]
        [DataRow("my", 2, false, "Tax ID")]
        [DataRow("cl", 2, false, "RUT")]
        [DataRow("mx", 2, false, "RFC")]
        [DataRow("id", 2, false, "NPWP")]
        [DataRow("th", 2, false, "TIN")]
        [DataRow("bh", 2, false, "VAT ID")]
        [DataRow("cm", 2, false, "VAT ID")]
        [DataRow("ge", 2, false, "VAT ID")]
        [DataRow("is", 2, false, "VAT ID")]
        [DataRow("ke", 2, false, "VAT ID")]
        [DataRow("md", 2, false, "VAT ID")]
        [DataRow("ng", 2, false, "VAT ID")]
        [DataRow("om", 2, false, "VAT ID")]
        [DataRow("tj", 2, false, "VAT ID")]
        [DataRow("ua", 2, false, "VAT ID")]
        [DataRow("uz", 2, false, "VAT ID")]
        [DataRow("zw", 2, false, "VAT ID")]
        [DataRow("fj", 2, false, "TIN")]
        [DataRow("gt", 2, false, "NIT")]
        [DataRow("kh", 2, false, "VAT-TIN")]
        [DataRow("ph", 2, false, "TIN")]
        [DataRow("vn", 2, false, "TIN")]
        [DataRow("co", 2, false, "NIT")]
        [DataRow("bs", 2, false, "TIN")]
        [DataRow("sa", 2, false, "VAT ID")]
        [DataRow("ae", 2, false, "VAT ID")]
        [DataRow("ci", 2, false, "TVA")]
        [DataRow("gh", 2, false, "TIN")]
        [DataRow("sn", 2, false, "NINEA")]
        [DataRow("zm", 2, false, "TPIN")]
        [DataTestMethod]
        public async Task GetTaxIdDescriptions_ModifyTaxIdToMandatory_CommercialStores_Update(string country, int pidlCounter, bool isTaxIdOptional, string expectedTaxVatIdDisplayText)
        {
            // Arrange
            var operations = new List<string>
            {
                "update"
            };

            var scenarios = new List<string>
            {
                "departmentalPurchase",
                "withCountryDropdown",
            };

            foreach (var operation in operations)
            {
                foreach (var scenario in scenarios)
                {
                    await ValidateTaxIdForm(country, "commercialstores", operation, scenario, pidlCounter, isTaxIdOptional, expectedTaxVatIdDisplayText, "dpHideCountry");
                }
            }
        }

        [DataRow("bb", 2, false, "TIN")]
        [DataRow("il", 2, false, "Ma'am")]
        [DataRow("kz", 2, false, "RUT")]
        [DataRow("la", 2, false, "TIN")]
        [DataRow("np", 2, false, "VAT")]
        [DataRow("sg", 2, false, "GST/VAT ID")]
        [DataRow("ug", 2, false, "TIN")]
        [DataRow("tr", 2, false, "VKN")]
        [DataRow("no", 2, false, "Orgnr")]
        [DataRow("am", 2, false, "AAH")]
        [DataRow("by", 2, false, "NDS")]
        [DataRow("bd", 2, false, "BIN")]
        [DataRow("my", 2, false, "Tax ID")]
        [DataRow("cl", 2, false, "RUT")]
        [DataRow("mx", 2, false, "RFC")]
        [DataRow("id", 2, false, "NPWP")]
        [DataRow("th", 2, false, "TIN")]
        [DataRow("bh", 2, false, "VAT ID")]
        [DataRow("cm", 2, false, "VAT ID")]
        [DataRow("ge", 2, false, "VAT ID")]
        [DataRow("is", 2, false, "VAT ID")]
        [DataRow("ke", 2, false, "VAT ID")]
        [DataRow("md", 2, false, "VAT ID")]
        [DataRow("ng", 2, false, "VAT ID")]
        [DataRow("om", 2, false, "VAT ID")]
        [DataRow("tj", 2, false, "VAT ID")]
        [DataRow("ua", 2, false, "VAT ID")]
        [DataRow("uz", 2, false, "VAT ID")]
        [DataRow("zw", 2, false, "VAT ID")]
        [DataRow("fj", 2, false, "TIN")]
        [DataRow("gt", 2, false, "NIT")]
        [DataRow("kh", 2, false, "VAT-TIN")]
        [DataRow("ph", 2, false, "TIN")]
        [DataRow("vn", 2, false, "TIN")]
        [DataRow("co", 2, false, "NIT")]
        [DataRow("bs", 2, false, "TIN")]
        [DataRow("sa", 2, false, "VAT ID")]
        [DataRow("ae", 2, false, "VAT ID")]
        [DataRow("ci", 2, false, "TVA")]
        [DataRow("gh", 2, false, "TIN")]
        [DataRow("sn", 2, false, "NINEA")]
        [DataRow("zm", 2, false, "TPIN")]
        [DataTestMethod]
        public async Task GetTaxIdDescriptions_ModifyTaxIdToMandatory_Azure(string country, int pidlCounter, bool isTaxIdOptional, string expectedTaxVatIdDisplayText)
        {
            // Arrange
            var operations = new List<string>
            {
                "update",
                "add"
            };

            var scenarios = new List<string>
            {
                "departmentalPurchase",
                "withCountryDropdown",
            };

            foreach (var operation in operations)
            {
                foreach (var scenario in scenarios)
                {
                    await ValidateTaxIdForm(country, "azure", operation, scenario, pidlCounter, isTaxIdOptional, expectedTaxVatIdDisplayText);
                }
            }
        }

        [DataRow("tr", "commercialstores", "add", null)]
        [DataRow("tr", "commercialstores", "update", true)]
        [DataRow("tr", "azure", "add", true)]
        [DataRow("tr", "azure", "update", true)]
        [DataRow("gb", "commercialstores", "add", null)]
        [DataRow("gb", "commercialstores", "update", true)]
        [DataRow("gb", "azure", "add", true)]
        [DataRow("gb", "azure", "update", true)]
        [DataRow("in", "commercialstores", "add", null)]
        [DataRow("in", "commercialstores", "update", true)]
        [DataRow("it", "commercialstores", "add", null, "enableItalyCodiceFiscale")]
        [DataRow("it", "commercialstores", "update", true, "enableItalyCodiceFiscale")]
        [DataRow("it", "commercialstores", "add", null)]
        [DataRow("it", "commercialstores", "update", true)]
        [DataRow("eg", "commercialstores", "add", null)]
        [DataRow("eg", "commercialstores", "update", true)]
        [DataRow("eg", "azure", "add", true)]
        [DataRow("eg", "azure", "update", true)]
        [DataTestMethod]
        public async Task GetTaxIdDescriptions_ModifyCountryToDisabled(string country, string partner, string operation, bool? isDisabled, string flightName = null)
        {
            // Arrange
            string url = $"/v7.0/Account001/taxidDescriptions?country={country}&language=en-US&type=commercial_tax_id&partner={partner}&operation={operation}&scenario=withCountryDropdown";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, flightNames: flightName);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");
            foreach (PIDLResource pidl in pidls)
            {
                DisplayHint countryDisplayDescription = pidl.GetDisplayHintById("hapiTaxCountryProperty");
                Assert.IsNotNull(countryDisplayDescription, "country property is expected to be not null");
                Assert.AreEqual(countryDisplayDescription.IsDisabled, isDisabled, "country's isDisabled property should be set correctly");

                string[] buttonIds = { "saveButton", "cancelButton", "saveButtonSuccess" };

                foreach (string buttonId in buttonIds)
                {
                    DisplayHint buttonDisplayDescription = pidl.GetDisplayHintById(buttonId);
                    if (buttonDisplayDescription != null)
                    {
                        Assert.IsTrue(buttonDisplayDescription.IsHidden ?? false, buttonId + " should be hidden with scenario 'withCountryDropdown'");
                    }
                }

                if (string.Equals(country, "in", StringComparison.OrdinalIgnoreCase))
                {
                    List<PIDLResource> linkedPidls = pidl.LinkedPidls;
                    Assert.IsNotNull(linkedPidls, "India tax pidl should have a linked GST tax pidl");
                    Assert.AreEqual(linkedPidls.Count, 1, "Inida tax pidl should have only one linked GST tax pidl");

                    DisplayHint stateDisplayDescription = linkedPidls[0].GetDisplayHintById("addressState");
                    Assert.IsNotNull(stateDisplayDescription, "state property is expected to be not null");
                    Assert.AreEqual(stateDisplayDescription.IsDisabled, isDisabled, "state's isDisabled property should be set correctly");
                }

                if (string.Equals(country, "it", StringComparison.OrdinalIgnoreCase))
                {
                    List<PIDLResource> linkedPidls = pidl.LinkedPidls;
                    if (string.Equals(flightName, Constants.PartnerFlightValues.EnableItalyCodiceFiscale, StringComparison.OrdinalIgnoreCase))
                    {
                        Assert.IsNotNull(linkedPidls, "Italy tax pidl should have a linked Vat ID tax pidl");
                        Assert.AreEqual(linkedPidls.Count, 1, "Italy tax pidl should have only one linked Vat ID tax pidl");
                    }
                    else
                    {
                        Assert.IsNull(linkedPidls, "Italy tax pidl should not have a linked tax pidl");
                    }
                }

                if (string.Equals(country, "eg", StringComparison.OrdinalIgnoreCase))
                {
                    List<PIDLResource> linkedPidls = pidl.LinkedPidls;
                    Assert.IsNotNull(linkedPidls, "Egypt tax pidl should have a linked Vat ID tax pidl");
                    Assert.AreEqual(linkedPidls.Count, 1, "Egypt tax pidl should have only one linked Vat ID tax pidl");
                }
            }
        }

        [DataRow("tr", "commercialstores", "add", "", false)]
        [DataRow("tr", "commercialstores", "add", "withCountryDropdown", false)]
        [DataRow("tr", "commercialstores", "add", "departmentalPurchase", false)]
        [DataRow("tr", "azure", "add", "withCountryDropdown", false)]
        [DataRow("tr", "commercialstores", "update", "withCountryDropdown", false)]
        [DataRow("tr", "commercialstores", "update", "departmentalPurchase", false)]
        [DataRow("tr", "azure", "update", "withCountryDropdown", true)]
        [DataRow("gb", "commercialstores", "add", "", false)]
        [DataRow("gb", "commercialstores", "add", "withCountryDropdown", false)]
        [DataRow("gb", "commercialstores", "add", "departmentalPurchase", false)]
        [DataRow("gb", "azure", "add", "withCountryDropdown", false)]
        [DataRow("gb", "commercialstores", "update", "withCountryDropdown", false)]
        [DataRow("gb", "commercialstores", "update", "departmentalPurchase", false)]
        [DataRow("gb", "azure", "update", "withCountryDropdown", true)]
        [DataRow("br", "commercialstores", "add", "", false)]
        [DataRow("br", "commercialstores", "add", "withCountryDropdown", false)]
        [DataRow("br", "commercialstores", "add", "departmentalPurchase", false)]
        [DataRow("br", "azure", "add", "withCountryDropdown", false)]
        [DataRow("br", "commercialstores", "update", "withCountryDropdown", false)]
        [DataRow("br", "commercialstores", "update", "departmentalPurchase", false)]
        [DataRow("br", "azure", "update", "withCountryDropdown", true)]
        [DataTestMethod]
        public async Task GetTaxIdDescriptions_DataSourceRemoved(string country, string partner, string operation, string scenario, bool hasDataSource)
        {
            // Arrange
            string url = $"/v7.0/Account001/taxidDescriptions?country={country}&language=en-US&type=commercial_tax_id&partner={partner}&operation={operation}&scenario={scenario}";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");
            Assert.AreEqual(pidls[0].DataSources != null, hasDataSource, "Datasource is set to wrong value");
        }

        [DataRow("departmentalPurchase", "x-ms-flight", "dpHideCountry", "", true, false)]
        [DataRow("departmentalPurchase", "x-ms-flight", "dpHideCountry,dummyValue", "dummyValue", true, false)]
        [DataRow("withCountryDropdown", "x-ms-flight", "dummyValue", "dummyValue", false, true)]
        [DataTestMethod]
        public async Task GetTaxIdDescriptions_HideCountryFieldAndSubmitButton(string scenario, string headerKey, string allHeaderValue, string leftoverHeader, bool countryIsHidden, bool buttonIsHidden)
        {
            // Arrange
            string url = $"/v7.0/Account001/taxidDescriptions?country=gb&language=en-US&type=commercial_tax_id&partner=commercialstores&operation=add&scenario={scenario}";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXServiceWithPartnerHeader(url, headerKey, allHeaderValue, leftoverHeader);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");
            foreach (PIDLResource profilePidl in pidls)
            {
                DisplayHint countryDisplayDescription = profilePidl.GetDisplayHintById("hapiTaxCountryProperty");
                Assert.IsNotNull(countryDisplayDescription);
                Assert.AreEqual(countryDisplayDescription.IsHidden ?? false, countryIsHidden);

                DisplayHint saveButtonDisplayDescription = profilePidl.GetDisplayHintById("saveButton");
                Assert.IsNotNull(saveButtonDisplayDescription);
                Assert.AreEqual(saveButtonDisplayDescription.IsHidden ?? false, buttonIsHidden);

                DisplayHint cancelButtonDisplayDescription = profilePidl.GetDisplayHintById("cancelButton");
                Assert.IsNotNull(cancelButtonDisplayDescription);
                Assert.AreEqual(cancelButtonDisplayDescription.IsHidden ?? false, buttonIsHidden);
            }
        }

        [DataRow("commercialstores", "add", "", false)]
        [DataRow("commercialstores", "add", "withCountryDropdown", false)]
        [DataRow("commercialstores", "add", "departmentalPurchase", false)]
        [DataRow("azure", "add", "withCountryDropdown", false)]
        [DataRow("azure", "update", "withCountryDropdown", true)]
        [DataRow("commercialstores", "update", "withCountryDropdown", false)]
        [DataRow("commercialstores", "update", "departmentalPurchase", false)]
        [DataTestMethod]
        public async Task GetTaxIdDescriptions_TWCommercialTests(string partner, string operation, string scenario, bool clientPrefill)
        {
            // Arrange
            string url = $"/v7.0/Account001/taxidDescriptions?country=tw&language=en-US&type=commercial_tax_id&partner={partner}&operation={operation}&scenario={scenario}";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");
            Assert.AreEqual(pidls[0].DataSources != null, clientPrefill);
            Assert.IsTrue(pidls[0].DataDescription.ContainsKey("additionalData"), "TW commercial tax Pidl should have additionalData");

            List<PIDLResource> additionalData = pidls[0].DataDescription["additionalData"] as List<PIDLResource>;
            Assert.IsFalse(additionalData == null || additionalData.Count == 0, "TW commercial tax Pidl's additionalData can't be null or empty");
            Assert.IsFalse(additionalData[0].DataDescription.ContainsKey("dataType"), "This property should be removed from commercial tax Pidl");
            Assert.IsFalse(additionalData[0].DataDescription.ContainsKey("dataCountry"), "This property should be removed from commercial tax Pidl");
            Assert.IsFalse(additionalData[0].DataDescription.ContainsKey("dataOperation"), "This property should be removed from commercial tax Pidl");
        }

        [DataRow("commercialstores", "add", "")]
        [DataRow("commercialstores", "add", "withCountryDropdown")]
        [DataRow("commercialstores", "add", "departmentalPurchase")]
        [DataRow("azure", "add", "withCountryDropdown")]
        [DataRow("azure", "update", "withCountryDropdown")]
        [DataRow("commercialstores", "update", "withCountryDropdown")]
        [DataRow("commercialstores", "update", "departmentalPurchase")]
        [DataTestMethod]
        public async Task TestPartnerMigration_PartnerSettingsService(string partner, string operation, string scenario)
        {
            var pssPidls = new List<PIDLResource>();
            var pidls = new List<PIDLResource>();
            Dictionary<string, string> testHeader = new Dictionary<string, string>()
            {
                { "x-ms-flight", "PXUsePartnerSettingsService" },
            };

            string expectedPSSResponse = $"{{\"default\":{{\"template\":\"{partner}\"}}}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            string url = $"/v7.0/Account001/taxidDescriptions?country=tw&language=en-US&type=commercial_tax_id&partner={partner}&operation={operation}&scenario={scenario}";

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

        // Expected taxid to mandatory if there is a checkbox in tax pidl and checkbox is selected
        [DataRow("", "gb", true, null)]
        [DataRow("departmentalPurchase", "gb", false, null)]
        [DataRow("withCountryDropdown", "gb", true, null)]
        [DataRow("", "in", false, true)]
        [DataRow("departmentalPurchase", "in", false, true)]
        [DataRow("withCountryDropdown", "in", false, true)]
        [DataRow("", "it", false, true, "enableItalyCodiceFiscale")]
        [DataRow("departmentalPurchase", "it", false, true, "enableItalyCodiceFiscale")]
        [DataRow("withCountryDropdown", "it", false, true, "enableItalyCodiceFiscale")]
        [DataRow("", "it", true, true)]
        [DataRow("departmentalPurchase", "it", false, true)]
        [DataRow("withCountryDropdown", "it", true, true)]
        [DataRow("", "eg", true, true)]
        [DataRow("departmentalPurchase", "eg", true, true)]
        [DataRow("withCountryDropdown", "eg", true, true)]
        [DataRow("departmentalPurchase", "eg", false, false, "PXEnableEGTaxIdsRequired")]
        [DataRow("withCountryDropdown", "eg", false, false, "PXEnableEGTaxIdsRequired")]
        [DataTestMethod]
        public async Task GetTaxIdDescriptions_DepartmentalPurchaseTests(string scenario, string country, bool isFirstTaxidOptional, bool isSecondTaxidOptional, string flightName = null)
        {
            // Arrange
            string url = $"/v7.0/Account001/taxidDescriptions?country={country}&language=en-US&type=commercial_tax_id&partner=commercialstores&operation=add&scenario={scenario}";

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, flightNames: flightName);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");
            PropertyDescription taxIdDescription = pidls[0].GetPropertyDescriptionByPropertyName("taxId");
            Assert.IsNotNull(taxIdDescription, "Taxid property is expected to be not null");

            if (string.Equals(country, "eg", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(flightName, Constants.PartnerFlightValues.PXEnableEGTaxIdsRequired, StringComparison.OrdinalIgnoreCase))
                {
                    Assert.IsNotNull(pidls[0].LinkedPidls, "Egypt tax linked pidl is expected to be not null");
                    PropertyDescription vatIdDescription = pidls[0].LinkedPidls[0].GetPropertyDescriptionByPropertyName("taxId");
                    Assert.IsNotNull(vatIdDescription, "Taxid property is expected to be not null");
                    Assert.AreEqual(taxIdDescription.IsOptional, isFirstTaxidOptional, "Taxid's IsOptional property is not correct");
                    Assert.AreEqual(vatIdDescription.IsOptional, isSecondTaxidOptional, "Taxid's IsOptional property is not correct");
                }
                else
                {
                    Assert.IsNotNull(pidls[0].LinkedPidls, "Egypt tax pidl should not have a linked tax pidl");
                }
            }
            else
            {
                Assert.AreEqual(taxIdDescription.IsOptional, isFirstTaxidOptional, "Taxid's IsOptional property is not correct");
            }           

            if (string.Equals(country, "in", StringComparison.OrdinalIgnoreCase))
            {
                Assert.IsNotNull(pidls[0].LinkedPidls, "India tax linked pidl is expected to be not null");
                PropertyDescription gstIdDescription = pidls[0].LinkedPidls[0].GetPropertyDescriptionByPropertyName("taxId");
                Assert.IsNotNull(gstIdDescription, "Taxid property is expected to be not null");
                Assert.AreEqual(gstIdDescription.IsOptional, isSecondTaxidOptional, "Taxid's IsOptional property is not correct");
            }

            if (string.Equals(country, "it", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(flightName, Constants.PartnerFlightValues.EnableItalyCodiceFiscale, StringComparison.OrdinalIgnoreCase))
                {
                    Assert.IsNotNull(pidls[0].LinkedPidls, "Italy tax linked pidl is expected to be not null");
                    PropertyDescription vatIdDescription = pidls[0].LinkedPidls[0].GetPropertyDescriptionByPropertyName("taxId");
                    Assert.IsNotNull(vatIdDescription, "Taxid property is expected to be not null");
                    Assert.AreEqual(vatIdDescription.IsOptional, isSecondTaxidOptional, "Taxid's IsOptional property is not correct");
                }
                else
                {
                    Assert.IsNull(pidls[0].LinkedPidls, "Italy tax pidl should not have a linked tax pidl");
                }
            }          
        }

        [DataRow("in", "add", "commercialstores")]
        [DataTestMethod]
        public async Task GetTaxDescriptions_ModifyIndiaGSTFieldsToOptional(string country, string operation, string partner)
        {
            string featureName = "PXEnabledNoSubmitIfGSTIDEmpty";
            PXFlightHandler.AddToEnabledFlights(featureName);
            Assert.AreEqual(country, "in", "Country should be India");
            Assert.AreEqual(operation, "add", "Operation should be Add");
            Assert.AreEqual(partner, "commercialstores", "Partner should be Commercial Stores");
            string url = $"/v7.0/Account001/taxidDescriptions?country={country}&language=en-US&type=commercial_tax_id&partner={partner}&operation={operation}";
            List<PIDLResource> pidls = await GetPidlFromPXService(url);
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");
            Assert.AreEqual(pidls[0].LinkedPidls.Count, 1, "Expected linked pidl's count does not match the return result");
            foreach (var dataDescription in pidls[0].LinkedPidls[0].DataDescription)
            {
                if (dataDescription.Key != null && !string.Equals(dataDescription.Key, Constants.DescriptionTypes.TaxIdDescription, StringComparison.OrdinalIgnoreCase))
                {
                    PropertyDescription propertyDescription = pidls[0].LinkedPidls[0].GetPropertyDescriptionByPropertyName(dataDescription.Key);
                    Assert.IsNotNull(propertyDescription, $"'{dataDescription.Key}' is expected to be not null");
                    Assert.AreEqual(propertyDescription.IsOptional, true, $"'IsOptional' property of '{dataDescription.Key}' is expected to be 'true'");             
                    Assert.AreEqual(propertyDescription.PropertyType, "clientData", $"'propertyType' property of '{dataDescription.Key}' is expected to be 'clientData'");
                }
            }
        }

        private async Task ValidateTaxIdForm(
            string country,
            string partner,
            string operation,
            string scenario, 
            int pidlCount,
            bool isTaxIdOptional,
            string expectedTaxVatIdDisplayText,
            string flights = null)
        {
            string url = $"/v7.0/Account001/taxidDescriptions?country={country}&language=en-US&type=commercial_tax_id&partner={partner}&operation={operation}&scenario={scenario}";

            // Act
            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-flight", flights
                }
            };
            List<PIDLResource> pidls = await GetPidlFromPXService(url, HttpStatusCode.OK, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");
            Assert.AreEqual(pidlCount, pidls.Count, "Expected pidl counter does not match the return result");

            PropertyDescription taxIdDescription = pidls[0].GetPropertyDescriptionByPropertyName("taxId");
            Assert.IsNotNull(taxIdDescription, "Taxid property is expected to be not null");
            Assert.AreEqual(isTaxIdOptional, taxIdDescription.IsOptional, $"Taxid's IsOptional property is not correct request {url}");

            // checkbox with text "Please confirm you are an authorized purchaser for a VAT registered entity" is required to be shown for tax id collection liability
            if (pidlCount > 1)
            {
                DisplayHint checkboxEnabled = pidls[0].GetDisplayHintById("profileIsTaxEnabled");
                DisplayHint checkboxDisabled = pidls[1].GetDisplayHintById("profileIsTaxDisabled");
                Assert.IsNotNull(checkboxEnabled, "profileIsTaxEnabled is expected to be not null");
                Assert.IsNotNull(checkboxDisabled, "profileIsTaxDisabled is expected to be not null");
            }

            PropertyDisplayHint hapiTaxVatIdProperty = (PropertyDisplayHint)pidls[0].GetDisplayHintById("hapiTaxVatIdProperty");
            Assert.IsNotNull(hapiTaxVatIdProperty);
            Assert.AreEqual(hapiTaxVatIdProperty.DisplayName, expectedTaxVatIdDisplayText, $"hapiTaxVatIdProperty display test is not expected in country {country}, partner {partner}");

            PropertyDisplayHint hapiTaxVatIdPropertyDisabled = (PropertyDisplayHint)pidls[1].GetDisplayHintById("hapiTaxVatIdPropertyDisabled");
            Assert.IsNotNull(hapiTaxVatIdPropertyDisabled);
            Assert.AreEqual(hapiTaxVatIdPropertyDisabled.DisplayName, expectedTaxVatIdDisplayText, $"hapiTaxVatIdPropertyDisabled display test is not expected in country {country}, partner {partner}");
        }

        [DataRow("onepage", "gb")]
        [DataRow("onepage", "it")]
        [DataRow("onepage", "in")]
        [DataRow("onepage", "fr")]
        [DataRow("onepage", "eg")]
        [DataRow("northstarweb", "fr")]
        [DataRow("northstarweb", "it")]
        [DataRow("northstarweb", "in")]
        [DataRow("northstarweb", "eg")]
        [DataTestMethod]
        public async Task GetTaxDescriptions_RemoveDataSource(string partner, string country)
        {
            // Arrage
            string url = $"/v7.0/Account001/taxIdDescriptions?type=commercial_tax_id&scenario=withCountryDropdown&partner={partner}&operation=update&country={country}&language=en-US";
            bool[] featureStatus = new bool[] { true, false };
            Dictionary<string, string> testHeader = new Dictionary<string, string>()
            {
                { "x-ms-flight", "PXDisablePSSCache,PXUsePartnerSettingsService" },
            };

            foreach (var isFeatureEnabled in featureStatus)
            {
                string expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\"},\"update\":{\"template\":\"defaulttemplate\"}}";
                if (isFeatureEnabled)
                {
                    expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"features\":{\"removeDataSource\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"removeDataSourceResources\":[\"taxId\"]}]}}},\"update\":{\"template\":\"defaulttemplate\",\"features\":{\"removeDataSource\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"removeDataSourceResources\":[\"taxId\"]}]}}}}";
                }

                // Act
                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

                List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: testHeader);

                // Assert
                // DataSources for standalone taxId main pidls is removed by default for template based partners with upadte operation
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");
                Assert.IsTrue(pidls[0].DataSources == null, "DataSources is expected to be null");

                if (isFeatureEnabled)
                {
                    Assert.IsTrue(pidls[0].LinkedPidls?[0].DataSources == null, "DataSources for linkedPidl is expected to be null");
                }
                else
                {
                    Assert.IsTrue(pidls[0].LinkedPidls == null ? true : pidls[0].LinkedPidls[0].DataSources != null, "DataSources for linkedPidl is not expected to be null");
                }

                PXSettings.PartnerSettingsService.ResetToDefaults();
            }
        }

        [DataRow("onepage", "it")]
        [DataRow("onepage", "in")]
        [DataRow("onepage", "eg")]
        [DataTestMethod]
        public async Task GetTaxDescriptions_SetSubmitURLToEmptyForTaxId(string partner, string country)
        {
            // Arrage
            string url = $"/v7.0/Account001/taxIdDescriptions?type=commercial_tax_id&scenario=withCountryDropdown&partner={partner}&operation=add&country={country}&language=en-US";
            bool[] featureStatus = new bool[] { true, false };
            Dictionary<string, string> testHeader = new Dictionary<string, string>()
            {
                { "x-ms-flight", "PXDisablePSSCache" },
            };

            foreach (var isFeatureEnabled in featureStatus)
            {
                string expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\"}}";
                if (isFeatureEnabled)
                {
                    expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"features\":{\"updatePidlSubmitLink\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"setSubmitURLToEmptyForTaxId\":true}]}}}}";
                }

                // Act
                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

                List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: testHeader);

                // Assert
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");
                var submitButton = pidls[0].GetDisplayHintById("saveButton");
               
                var context = JsonConvert.DeserializeObject<Microsoft.Commerce.Payments.PXCommon.RestLink>(JsonConvert.SerializeObject(submitButton.Action.Context));
                Assert.IsNotNull(context);

                Assert.IsTrue(isFeatureEnabled ? string.IsNullOrEmpty(context.Href) : !string.IsNullOrEmpty(context.Href));

                PXSettings.PartnerSettingsService.ResetToDefaults();
            }
        }

        [DataRow("onepage", "tw")]
        [DataRow("onepage", "br")]
        [DataRow("defaulttemplate", "tw")]
        [DataRow("defaulttemplate", "br")]
        [DataRow("consumersupport", "tw")]
        [DataRow("consumersupport", "br")]
        [DataTestMethod]
        public async Task GetTaxDescriptions_Template_ConsumerTaxId(string partner, string country)
        {
            // Arrage
            string url = $"/v7.0/Account001/taxIdDescriptions?type=consumer_tax_id&&partner={partner}&operation=add&country={country}&language=en-US";
            Dictionary<string, string> testHeader = new Dictionary<string, string>()
            {
                { "x-ms-flight", "PXDisablePSSCache" },
            };

            if (string.Equals(partner, "onepage", StringComparison.OrdinalIgnoreCase))
            {
                string expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\"}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
            }

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: testHeader);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");
            Assert.AreEqual("taxId", pidls[0].Identity["description_type"]);

            var taxId = pidls[0].GetDisplayHintByPropertyName("value");

            Assert.IsNotNull(taxId, "Conumer TaxId expected to be not null");

            PXSettings.PartnerSettingsService.ResetToDefaults();
        }

        [DataRow("onepage", "gb")]
        [DataRow("onepage", "it")]
        [DataRow("onepage", "in")]
        [DataRow("onepage", "fr")]
        [DataRow("northstarweb", "fr")]
        [DataRow("northstarweb", "it")]
        [DataRow("northstarweb", "in")]
        [DataRow("azurebmx", "fr")]
        [DataRow("macmanaged", "fr")]
        [DataRow("azurebmx", "ge")]
        [DataRow("macmanaged", "ge")]
        [DataRow("onepage", "eg")]
        [DataRow("northstarweb", "eg")]
        [DataTestMethod]
        public async Task GetTaxDescriptions_Anonymous(string partner, string country)
        {
            // Arrage
            string url = $"/v7.0/taxIdDescriptions?type=commercial_tax_id&scenario=withCountryDropdown&partner={partner}&operation=update&country={country}&language=en-US";
            bool[] featureStatus = new bool[] { true, false };
            Dictionary<string, string> testHeader = new Dictionary<string, string>()
            {
                { "x-ms-flight", "PXDisablePSSCache,PXUsePartnerSettingsService" },
            };

            foreach (var isFeatureEnabled in featureStatus)
            {
                string expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\"},\"update\":{\"template\":\"defaulttemplate\"}}";
                if (isFeatureEnabled)
                {
                    expectedPSSResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"features\":{\"removeDataSource\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"removeDataSourceResources\":[\"taxId\"]}]}}},\"update\":{\"template\":\"defaulttemplate\",\"features\":{\"removeDataSource\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"removeDataSourceResources\":[\"taxId\"]}]}}}}";
                }

                // Act
                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

                List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: testHeader);

                // Assert
                // DataSources for standalone taxId main pidls is removed by default for template based partners with upadte operation
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");
                Assert.IsTrue(pidls[0].DataSources == null, "DataSources is expected to be null");

                if (isFeatureEnabled)
                {
                    Assert.IsTrue(pidls[0].LinkedPidls?[0].DataSources == null, "DataSources for linkedPidl is expected to be null");
                }
                else
                {
                    Assert.IsTrue(pidls[0].LinkedPidls == null ? true : pidls[0].LinkedPidls[0].DataSources != null, "DataSources for linkedPidl is not expected to be null");
                }

                PXSettings.PartnerSettingsService.ResetToDefaults();
            }
        }

        /// <summary>
        /// Test is to validate the enablePlaceholder PSS feature
        /// </summary>
        /// <param name="country"></param>
        /// <returns></returns>
        [DataRow("in")]
        [DataRow("it")]
        [DataRow("fr")]
        [DataRow("de")]
        [DataRow("nl")]
        [DataRow("eg")]
        [DataTestMethod]
        public async Task GetTaxDescriptions_EnablePlaceholderPssFeature(string country)
        {
            // Arrange
            List<bool> featureStatuses = new List<bool>() { false, true };
            List<string> operations = new List<string>() { "add", "update" };
            Dictionary<string, string> headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };

            foreach (string operation in operations)
            {
                foreach (bool featureStatus in featureStatuses)
                {
                    string enablePlaceholderPssSettings = featureStatus ? "\"enablePlaceholder\":{\"applicableMarkets\":[]}," : string.Empty;
                    string url = $"/v7.0/Account001/taxidDescriptions?operation={operation}&country={country}&language=en-US&type=commercial_tax_id&partner=onepage&scenario=withCountryDropdown\";";
                    string partnerSettingResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"features\":{" + enablePlaceholderPssSettings + "\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\"},{\"addressType\":\"hapiV1BillToIndividual\",\"dataSource\":\"hapi\"}]}}},\"update\":{\"template\":\"defaulttemplate\",\"features\":{" + enablePlaceholderPssSettings + "\"customizeAddressForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"addressType\":\"hapiV1SoldToIndividual\",\"dataSource\":\"hapi\"},{\"addressType\":\"hapiV1BillToIndividual\",\"dataSource\":\"hapi\"}]}}}}";
                    PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);

                    // Act
                    List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

                    // Assert
                    Assert.IsNotNull(pidls, "Pidl is expected to be not null");

                    foreach (PIDLResource pidl in pidls)
                    {
                        ValidatePidlProperties(pidl, featureStatus);

                        if (pidl.LinkedPidls != null)
                        {
                            foreach (var linkedPidl in pidl.LinkedPidls)
                            {
                                ValidatePidlProperties(linkedPidl, featureStatus);
                            }
                        }
                    }

                    PXSettings.PartnerSettingsService.Responses.Clear();
                }
            }
        }

        /// <summary>
        /// Test is to validate the CustomizeTaxIdForm feature
        /// </summary>
        /// <param name="country"></param>
        /// <returns></returns>
        [DataRow("commercialstores", "add", "withCountryDropdown", null, false, false, false)]
        [DataRow("commercialstores", "add", "withCountryDropdown", null, true, false, false)]
        [DataRow("commercialstores", "add", "withCountryDropdown", null, true, true, true)]
        [DataRow("commercialstores", "add", "withCountryDropdown", "vat_id", false, false, false)]
        [DataRow("commercialstores", "add", "withCountryDropdown", "vat_id", true, false, false)]
        [DataRow("commercialstores", "add", "withCountryDropdown", "vat_id", true, true, true)]
        [DataRow("commercialstores", "add", "withCountryDropdown", "commercial_tax_id", false, false, false)]
        [DataRow("commercialstores", "add", "withCountryDropdown", "commercial_tax_id", true, false, false)]
        [DataRow("commercialstores", "add", "withCountryDropdown", "commercial_tax_id", true, true, true)]
        [DataRow("commercialstores", "update", "withCountryDropdown", null, false, false, true)]
        [DataRow("commercialstores", "update", "withCountryDropdown", null, true, false, true)]
        [DataRow("commercialstores", "update", "withCountryDropdown", null, true, true, true)]
        [DataRow("commercialstores", "update", "withCountryDropdown", "vat_id", false, false, true)]
        [DataRow("commercialstores", "update", "withCountryDropdown", "vat_id", true, false, true)]
        [DataRow("commercialstores", "update", "withCountryDropdown", "vat_id", true, true, true)]
        [DataRow("commercialstores", "update", "withCountryDropdown", "commercial_tax_id", false, false, true)]
        [DataRow("commercialstores", "update", "withCountryDropdown", "commercial_tax_id", true, false, true)]
        [DataRow("commercialstores", "update", "withCountryDropdown", "commercial_tax_id", true, true, true)]
        [DataRow("azure", "add", "withCountryDropdown", null, false, false, true)]
        [DataRow("azure", "add", "withCountryDropdown", null, true, false, false)]
        [DataRow("azure", "add", "withCountryDropdown", null, true, true, true)]
        [DataRow("azure", "add", "withCountryDropdown", "vat_id", false, false, true)]
        [DataRow("azure", "add", "withCountryDropdown", "vat_id", true, false, false)]
        [DataRow("azure", "add", "withCountryDropdown", "vat_id", true, true, true)]
        [DataRow("azure", "add", "withCountryDropdown", "commercial_tax_id", false, false, true)]
        [DataRow("azure", "add", "withCountryDropdown", "commercial_tax_id", true, false, false)]
        [DataRow("azure", "add", "withCountryDropdown", "commercial_tax_id", true, true, true)]
        [DataRow("azure", "update", "withCountryDropdown", null, false, false, true)]
        [DataRow("azure", "update", "withCountryDropdown", null, true, false, true)]
        [DataRow("azure", "update", "withCountryDropdown", null, true, true, true)]
        [DataRow("azure", "update", "withCountryDropdown", "vat_id", false, false, true)]
        [DataRow("azure", "update", "withCountryDropdown", "vat_id", true, false, true)]
        [DataRow("azure", "update", "withCountryDropdown", "vat_id", true, true, true)]
        [DataRow("azure", "update", "withCountryDropdown", "commercial_tax_id", false, false, true)]
        [DataRow("azure", "update", "withCountryDropdown", "commercial_tax_id", true, false, true)]
        [DataRow("azure", "update", "withCountryDropdown", "commercial_tax_id", true, true, true)]
        [DataTestMethod]
        public async Task GetTaxDescriptions_PSS_CustomizeTaxIdForm_Update(string partner, string operation, string scenario, string type, bool usePSS, bool useFeature, bool shouldDisableCountry)
        {
            // Arrange
            List<string> countries = new List<string>() { "de", "it", "pl", "gb", "au", "es", "nl" };
            Dictionary<string, string> headers = new Dictionary<string, string>();

            foreach (string country in countries)
            {
                if (usePSS)
                {
                    if (!headers.ContainsKey("x-ms-flight"))
                    {
                        headers.Add("x-ms-flight", "PXUsePartnerSettingsService");
                    }

                    string featureSetting = useFeature ? ",\"features\":{\"customizeTaxIdForm\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"disableCountryDropdown\":true}]}}" : null;
                    string partnerSettingResponse = "{\"update\":{\"template\":\"defaulttemplate\"" + featureSetting + "},\"add\":{\"template\":\"defaulttemplate\"" + featureSetting + "}}";

                    PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
                }

                // partners never call this without a scenario.
                string url = $"/v7.0/Account001/taxidDescriptions?operation={operation}&country={country}&language=en-US&type={type}&partner={partner}&scenario={scenario}";

                List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

                Assert.AreEqual(pidls.Count, 1, "Pidl is expected to be not null");

                DisplayHint countryDisplayDescription = pidls[0].GetDisplayHintById("hapiTaxCountryProperty");
                Assert.IsNotNull(countryDisplayDescription, "hapiTaxCountryProperty Display Description should not be missing.");

                if (shouldDisableCountry)
                {
                    Assert.IsTrue(countryDisplayDescription.IsDisabled, "country field IsDisabled should be TRUE, but had value: \"" + (countryDisplayDescription == null ? "null" : countryDisplayDescription.IsDisabled.ToString()) + "\" for type " + type + ".");
                }
                else
                {
                    Assert.IsNull(countryDisplayDescription.IsDisabled, "country field should be NULL, but had value: \"" + countryDisplayDescription.IsDisabled.ToString() + "\" for type " + type + ".");
                }
            }
        }

        [DataRow("cstconsumer", "br")]
        [DataRow("cstconsumer", "tw")]
        [DataTestMethod]
        public async Task GetTaxIdDescriptions_MakeFieldRequired(string partner, string country)
        {
            // Arrange
            var operations = new List<string> { "add" };
            Dictionary<string, string> headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };
            string pssResponse = "{\"add\": {\"template\": \"defaulttemplate\",\"features\": {\"customizeTaxIdForm\": {\"applicableMarkets\": [],\"displayCustomizationDetail\": [{\"fieldsToMakeRequired\": [\"value\"],\"dataSource\": \"consumerTaxId\"}]}}}}";

            PXSettings.PartnerSettingsService.ArrangeResponse(pssResponse);

            foreach (string operation in operations)
            {
                PXFlightHandler.ResetToDefault();

                string url = $"/v7.0/Account001/taxidDescriptions?&operation={operation}&country={country}&language=en-US&type=consumer_tax_id&partner={partner}";

                // Act
                List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

                // Assert
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");
                var valueField = pidls[0].DataDescription["value"];
                var propertyDescription = valueField as PropertyDescription;
                var valueDisplay = pidls[0].DisplayPages[0].Members[0].DisplayText();

                Assert.IsFalse(propertyDescription?.IsOptional);
                if (country.Equals("br", StringComparison.OrdinalIgnoreCase))
                {
                    Assert.AreEqual(valueDisplay, "CPF");
                }
                else
                {
                    Assert.AreEqual(valueDisplay, "Tax ID");
                }
            }
        }

        [DataRow("cstconsumer", "br")]
        [DataRow("cstconsumer", "tw")]
        [DataTestMethod]
        public async Task GetTaxIdDescriptions_NotMakeFieldRequired(string partner, string country)
        {
            // Arrange
            var operations = new List<string> { "add" };
            Dictionary<string, string> headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };
            string pssResponse = "{\"add\": {\"template\": \"defaulttemplate\",\"features\": {\"customizeTaxIdForm\": {\"applicableMarkets\": [],\"displayCustomizationDetail\": [{\"fieldsToMakeRequired\": [],\"dataSource\": \"consumerTaxId\"}]}}}}";

            PXSettings.PartnerSettingsService.ArrangeResponse(pssResponse);

            foreach (string operation in operations)
            {
                PXFlightHandler.ResetToDefault();

                string url = $"/v7.0/Account001/taxidDescriptions?&operation={operation}&country={country}&language=en-US&type=consumer_tax_id&partner={partner}";

                // Act
                List<PIDLResource> pidls = await GetPidlFromPXService(url, additionaHeaders: headers);

                // Assert
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");
                var valueField = pidls[0].DataDescription["value"];
                var propertyDescription = valueField as PropertyDescription;
                var valueDisplay = pidls[0].DisplayPages[0].Members[0].DisplayText();

                Assert.IsTrue(propertyDescription?.IsOptional);
                if (country.Equals("br", StringComparison.OrdinalIgnoreCase))
                {
                    Assert.AreEqual(valueDisplay, "CPF (optional)");
                }
                else
                {
                    Assert.AreEqual(valueDisplay, "Tax ID");
                }
            }
        }
    }
}