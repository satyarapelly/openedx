// <copyright company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>

namespace CIT.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Helpers;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GetTaxIdDescriptionsTests : UnitTestBase
    {
        readonly Dictionary<string, string[]> namedPartnerLists = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "Partners-ConsumerTaxId",
                new string[] { "amc", "amcweb", "amcxbox", "appsource", "cart", "consumersupport", "default", "northstarweb", "officeoobe", "oxooobe", "officeoobeinapp", "onedrive", "oxodime", "oxowebdirect", "smboobe", "test", "webblends", "webblends_inline", "webpay", "xbox", "xboxweb", "ggpdeds" }
            },
            {
                "Partners-CommercialTaxId",
                new string[] { "commercialstores" }
            },
            {
                "Partners-NotApplicableForTaxId",
                new string[] { "bing", "bingtravel", "commercialwebblends", "marketplace", "mseg", "office", "payin", "setupoffice", "setupofficesdx", "storeoffice", "storify", "xboxnative", "xboxsubs", "xboxsettings", "saturn", "msteams", "onepage", "twopage", "selectpmbuttonlist", "selectpmradiobuttonlist", "selectpmdropdown", "listpidropdown", "defaulttemplate", "listpiradiobutton", "listpibuttonlist", "consoletemplate", "secondscreentemplate" }
            },
            {
                "Partners-Skip",
                new string[] { "appsource", "azure", "azuresignup", "azureibiza", "commercialsupport", "wallet", "windowssettings", "windowsnative", "windowsstore", "windowssubs" }
            }
        };

        readonly List<string> taxIdSubmitButtonHintIds = new List<string>() { "submitButton", "saveButton", "saveButtonHidden", "saveNextButton" };

        [TestMethod]
        public void GetTaxIdDescriptions_EnsureCITsCoverAllPartners()
        {
            // Arrange
            var allProfilePartners = namedPartnerLists["Partners-ConsumerTaxId"]
                .Concat(namedPartnerLists["Partners-CommercialTaxId"])
                .Concat(namedPartnerLists["Partners-NotApplicableForTaxId"])
                .Concat(namedPartnerLists["Partners-Skip"]);

            // Assert
            CollectionAssert.AreEquivalent(TestConstants.AllPartners.ToList(), allProfilePartners.Distinct().ToList(), "CIT to verify standalone profile is expected to cover all partners");
        }

        // In general, use country "us" for common test cases
        // For TaxId, "us" does not have neither consumer tax id nor commercial tax id
        // Use "br" for consumer general case, "de" for commercial general case
        [DataRow("br", "consumer_tax_id", "Partners-ConsumerTaxId", "", "add", null, 1, false, "https://{pifd-endpoint}/users/{userId}/tax-ids?country=br&language=en-us&profileType=", "POST")]
        [DataRow("br", "consumer_tax_id", "Partners-ConsumerTaxId", "consumer", "add", null, 1, false, "https://{pifd-endpoint}/users/{userId}/tax-ids?country=br&language=en-us&profileType=consumer", "POST")]
        [DataRow("de", "commercial_tax_id", "Partners-CommercialTaxId", "organization", "add", null, 1, false, "https://{hapi-endpoint}/{userId}/taxids", "POST")]
        [DataRow("de", "commercial_tax_id", "Partners-CommercialTaxId", "organization", "add", null, 1, true, "https://{hapi-endpoint}/{userId}/taxids", "POST")]
        [DataRow("de", "commercial_tax_id", "Partners-CommercialTaxId", "organization", "add", "withCountryDropdown", 1, true, "https://{hapi-endpoint}/{userId}/taxids?accountId={partnerData.prefillData.accountId}&organizationId={partnerData.prefillData.organizationId}", "POST")]
        [DataRow("de", "commercial_tax_id", "Partners-CommercialTaxId", "organization", "update", "withCountryDropdown", 1, true, "https://{hapi-endpoint}/{userId}/taxids?accountId={partnerData.prefillData.accountId}&organizationId={partnerData.prefillData.organizationId}", "POST")]
        [DataRow("de", "commercial_tax_id", "Partners-CommercialTaxId", "legalentity", "add", null, 1, false, "https://{hapi-endpoint}/{userId}/taxids", "POST")]
        [DataRow("tr", "commercial_tax_id", "Partners-CommercialTaxId", "organization", "add", null, 1, false, "https://{hapi-endpoint}/{userId}/taxids", "POST")]
        [DataRow("tr", "commercial_tax_id", "Partners-CommercialTaxId", "organization", "add", null, 2, true, "https://{hapi-endpoint}/{userId}/taxids", "POST")]
        [DataRow("tr", "commercial_tax_id", "Partners-CommercialTaxId", "organization", "add", "withCountryDropdown", 2, true, "https://{hapi-endpoint}/{userId}/taxids?accountId={partnerData.prefillData.accountId}&organizationId={partnerData.prefillData.organizationId}", "POST")]
        [DataRow("tr", "commercial_tax_id", "Partners-CommercialTaxId", "organization", "update", "withCountryDropdown", 2, true, "https://{hapi-endpoint}/{userId}/taxids?accountId={partnerData.prefillData.accountId}&organizationId={partnerData.prefillData.organizationId}", "POST")]
        [DataRow("tr", "commercial_tax_id", "Partners-CommercialTaxId", "legalentity", "add", null, 1, false, "https://{hapi-endpoint}/{userId}/taxids", "POST")]
        [DataRow("by", "commercial_tax_id", "Partners-CommercialTaxId", "organization", "add", null, 1, false, "https://{hapi-endpoint}/{userId}/taxids", "POST")]
        [DataRow("by", "commercial_tax_id", "Partners-CommercialTaxId", "organization", "add", null, 2, true, "https://{hapi-endpoint}/{userId}/taxids", "POST")]
        [DataRow("by", "commercial_tax_id", "Partners-CommercialTaxId", "organization", "add", "withCountryDropdown", 2, true, "https://{hapi-endpoint}/{userId}/taxids?accountId={partnerData.prefillData.accountId}&organizationId={partnerData.prefillData.organizationId}", "POST")]
        [DataRow("by", "commercial_tax_id", "Partners-CommercialTaxId", "organization", "update", "withCountryDropdown", 2, true, "https://{hapi-endpoint}/{userId}/taxids?accountId={partnerData.prefillData.accountId}&organizationId={partnerData.prefillData.organizationId}", "POST")]
        [DataRow("by", "commercial_tax_id", "Partners-CommercialTaxId", "legalentity", "add", null, 1, false, "https://{hapi-endpoint}/{userId}/taxids", "POST")]
        [DataRow("am", "commercial_tax_id", "Partners-CommercialTaxId", "organization", "add", null, 1, false, "https://{hapi-endpoint}/{userId}/taxids", "POST")]
        [DataRow("am", "commercial_tax_id", "Partners-CommercialTaxId", "organization", "add", null, 2, true, "https://{hapi-endpoint}/{userId}/taxids", "POST")]
        [DataRow("am", "commercial_tax_id", "Partners-CommercialTaxId", "organization", "add", "withCountryDropdown", 2, true, "https://{hapi-endpoint}/{userId}/taxids?accountId={partnerData.prefillData.accountId}&organizationId={partnerData.prefillData.organizationId}", "POST")]
        [DataRow("am", "commercial_tax_id", "Partners-CommercialTaxId", "organization", "update", "withCountryDropdown", 2, true, "https://{hapi-endpoint}/{userId}/taxids?accountId={partnerData.prefillData.accountId}&organizationId={partnerData.prefillData.organizationId}", "POST")]
        [DataRow("am", "commercial_tax_id", "Partners-CommercialTaxId", "legalentity", "add", null, 1, false, "https://{hapi-endpoint}/{userId}/taxids", "POST")]
        [DataRow("no", "commercial_tax_id", "Partners-CommercialTaxId", "organization", "add", null, 1, false, "https://{hapi-endpoint}/{userId}/taxids", "POST")]
        [DataRow("no", "commercial_tax_id", "Partners-CommercialTaxId", "organization", "add", null, 2, true, "https://{hapi-endpoint}/{userId}/taxids", "POST")]
        [DataRow("no", "commercial_tax_id", "Partners-CommercialTaxId", "organization", "add", "withCountryDropdown", 2, true, "https://{hapi-endpoint}/{userId}/taxids?accountId={partnerData.prefillData.accountId}&organizationId={partnerData.prefillData.organizationId}", "POST")]
        [DataRow("no", "commercial_tax_id", "Partners-CommercialTaxId", "organization", "update", "withCountryDropdown", 2, true, "https://{hapi-endpoint}/{userId}/taxids?accountId={partnerData.prefillData.accountId}&organizationId={partnerData.prefillData.organizationId}", "POST")]
        [DataRow("no", "commercial_tax_id", "Partners-CommercialTaxId", "legalentity", "add", null, 1, false, "https://{hapi-endpoint}/{userId}/taxids", "POST")]
        [DataTestMethod]
        public void GetTaxIdDescriptions_GeneralTests(string country, string type, string partnerListName, string profileType, string operation, string scenario, int expectedPidlNumber, bool isStandalone, string expectedHref, string expectedMethod)
        {
            const string Language = "en-us";

            foreach (string partner in namedPartnerLists[partnerListName])
            {
                List<PIDLResource> taxIdPidls = PIDLResourceFactory.Instance.GetTaxIdDescriptions(country, type, Language, partner, profileType, operation, isStandalone, scenario: scenario);
                PidlAssert.IsValid(taxIdPidls, expectedPidlNumber);

                Microsoft.Commerce.Payments.PXCommon.RestLink submitActionContext = null;
                UnitTestBase.AssertSubmitHintExists(taxIdPidls, taxIdSubmitButtonHintIds, "TaxId", out submitActionContext);
                Assert.AreEqual(expectedHref, submitActionContext.Href, "TaxId submit display hint action context is expected to match the href provided");
                Assert.AreEqual(expectedMethod, submitActionContext.Method, "TaxId submit display hint action context is expected to match the method provided");
            }
        }

        [DataRow("commercialstores", "organization", true, "add", "", false, "")]
        [DataRow("commercialstores", "organization", false, "update_patch", "PXProfileUpdateToHapi", true, "")]
        [DataRow("commercialstores", "legalentity", false, "update_patch", "", true, "")]
        [DataRow("azure", "organization", true, "update", "", true, "withCountryDropdown")]
        [DataTestMethod]
        public void GetTaxIdDescriptions_TWCommercialTests(string partner, string profileType, bool isStandalone, string operation, string flightNames, bool clientPrefill, string scenario)
        {
            const string Country = "tw";
            const string Type = "commercial_tax_id";
            const string Language = "en-us";

            List<string> exposedFlights = flightNames.Split(',').ToList();
            List<PIDLResource> taxIdPidls = PIDLResourceFactory.Instance.GetTaxIdDescriptions(Country, Type, Language, partner, profileType, operation, isStandalone, exposedFlights, scenario);
            PidlAssert.IsValid(taxIdPidls, 1);

            Assert.AreEqual(taxIdPidls[0].DataSources != null, clientPrefill);
            Assert.IsTrue(taxIdPidls[0].DataDescription.ContainsKey("additionalData"), "TW commercial tax Pidl should have additionalData");

            List<PIDLResource> additionalData = taxIdPidls[0].DataDescription["additionalData"] as List<PIDLResource>;
            Assert.IsFalse(additionalData == null || additionalData.Count == 0, "TW commercial tax Pidl's additionalData can't be null or empty");
            Assert.IsFalse(additionalData[0].DataDescription.ContainsKey("dataType"), "This property should be removed from commercial tax Pidl");
            Assert.IsFalse(additionalData[0].DataDescription.ContainsKey("dataCountry"), "This property should be removed from commercial tax Pidl");
            Assert.IsFalse(additionalData[0].DataDescription.ContainsKey("dataOperation"), "This property should be removed from commercial tax Pidl");
        }

        [DataRow("in", "12ABCDE3456F7G8")] //// ^[0-9]{2}[0-9A-Z]{5}[0-9]{4}[0-9A-Z]{4}$
        [DataRow("bh", "212345678901234")] //// ^2\d{14}$
        [DataRow("cm", "A123456789000A,a123456789000a")] //// ^[A-Za-z]{1}\d{12}[A-Za-z]{1}$
        [DataRow("ge", "123456789")] //// ^\d{9}$
        [DataRow("gh", "C1234567890")] //// ^C\d{10}$
        [DataRow("is", "12345,123456")] //// ^\d{5}$|^\d{6}$
        [DataRow("ke", "P052005205K,P000000000c")] //// ^P\d{9}[A-Za-z]$
        [DataRow("md", "1234567")] //// ^\d{7}$
        [DataRow("ng", "01012345-0001")] //// ^\d{8}-0001$
        [DataRow("om", "OM1234567890")] //// ^OM\d{10}$
        [DataRow("tj", "123456789")] //// ^\d{9}$
        [DataRow("ua", "123456789012,1234567890,123456789")] ////^\d{12}$|^\d{10}$|^\d{9}$
        [DataRow("zw", "12345678")] //// ^\d{12}$
        [DataRow("fj", "11-57252-0-3,99-99999-9-9")] //// ^\d{2}\-\d{5}\-\d{1}\-\d{1}$
        [DataRow("gt", "1234567-1")] //// ^\d{7}\-[0-9]$
        [DataRow("kh", "L001-123456789,B012-123456789,K012-123456789,E012-1234567890")] //// ^L001\-\d{9}$|^B0\d{2}\-\d{9}$|^K0\d{2}\-\d{9}$|^E0\d{2}\-\d{10}$
        [DataRow("ph", "123-456-789,123-456-789-012")] //// ^\d{3}\-\d{3}\-\d{3}$|^\d{3}\-\d{3}\-\d{3}\-\d{3}$
        [DataRow("vn", "1234567890,1234567890-123")] //// ^\d{10}$|^\d{10}\-\d{3}$
        [DataRow("ae", "123456789012345")] //// ^\d{15}$
        [DataRow("sa", "123456789012345")] //// ^\d{15}$
        [DataRow("co", "123456789-1,123.123.123-1,1234567")] //// ^\d{9}\-\d{1}$|^\d{3}\.\d{3}\.\d{3}\-\d{1}$|^\d{6,11}$
        [DataRow("ci", "CI1234567A")]  ////^CI\d{7}[A-Za-z]$
        [DataRow("gh", "C1234567890")] ////^C\d{10}$
        [DataRow("sn", "1234567890123")] ////^\d{13}$
        [DataRow("zm", "1001234567")]    ////^100\d{7}$
        [DataRow("la", "123456789012")]  ////^\d{12}$
        [DataTestMethod]
        public void ValidateTaxIdDescriptionRegex_Pass_ValidTaxIdInputs(
            string country,
            string validTaxIdInputs)
        {
            string countryTaxIdRegexExpression = GetTaxIdDescriptionTaxRegexExpression(country);

            var validTaxIds = validTaxIdInputs.Split(',');
            foreach (var taxId in validTaxIds)
            {
                Assert.IsTrue(Regex.Match(taxId.Trim(), countryTaxIdRegexExpression).Success, $"country: {country}, taxId: {taxId.Trim()} {countryTaxIdRegexExpression}");
            }
        }

        [DataRow("in", "12abcde3456F7GP")] //// ^[0-9]{2}[0-9A-Z]{5}[0-9]{4}[0-9A-Z]{4}$
        [DataRow("bh", "312345678901234,21,211222222232324242424242,aBcDeFGHIjklMno")] //// ^2\d{14}$
        [DataRow("cm", "a1234567890000,a1A,a12345678900000000,1111")] //// ^[A-Za-z]{1}\d{12}[A-Za-z]{1}$
        [DataRow("ge", "1234567890,1,A")] //// ^\d{9}$
        [DataRow("gh", "C12345678901")] //// ^C\d{10}$
        [DataRow("is", "12345dd,6,AAAAA")] //// ^\d{5}$|^\d{6}$
        [DataRow("ke", "P052005205,A000000000c")] //// ^P\d{9}[A-Za-z]$
        [DataRow("md", "123456,AAAAAAA")] //// ^\d{7}$
        [DataRow("ng", "01012345")] //// ^\d{8}-0001$
        [DataRow("om", "OM123456789,MO1234567890")] //// ^OM\d{10}$
        [DataRow("tj", "12345678,1234567890")] //// ^\d{9}$
        [DataRow("ua", "1234567890123")] //// ^\d{12}$|^\d{10}$|^\d{9}$
        [DataRow("zw", "1234567,AAAAAAAAAAAAA")] //// ^\d{8}$
        [DataRow("fj", "123-45678-90,AB-CDEFG-H-I")] //// ^\d{2}\-\d{5}\-\d{1}\-\d{1}$
        [DataRow("gt", "123456-12")] //// ^\d{7}\-[0-9]$
        [DataRow("kh", "L123456789,B1234556789,A001-00000000")] //// ^L001\-\d{9}$|^B0\d{2}\-\d{9}$|^K0\d{2}\-\d{9}$|^E0\d{2}\-\d{10}$
        [DataRow("ph", "1234-56789")] //// ^\d{3}\-\d{3}\-\d{3}$|^\d{3}\-\d{3}\-\d{3}\-\d{3}$
        [DataRow("vn", "12345678901-2")] //// ^\d{10}$|^\d{10}\-\d{3}$
        [DataRow("ae", "12345678901234")] //// ^\d{15}$
        [DataRow("sa", "12345678901234")] //// ^\d{15}$
        [DataRow("co", "12345-1,123.13.123-1,12345")] //// ^\d{9}\-\d{1}$|^\d{3}\.\d{3}\.\d{3}\-\d{1}$|^\d{6,11}
        [DataRow("ci", "CI12345678,12345678")]  ////^CI\d{7}[A-Za-z]$
        [DataRow("gh", "9867453,C123456789")] ////^C\d{10}$
        [DataRow("sn", "12309874568977,ABC1234567890")] ////^\d{13}$
        [DataRow("zm", "1234567890,1007890,100A191919")]    ////^100\d{7}$
        [DataRow("la", "1234567890123,12345678901,A23456789012")]  ////^\d{12}$
        [DataTestMethod]
        public void ValidateTaxIdDescriptionRegex_Fail_InvalidTaxIdInputs(
           string country,
           string inValidTaxIdInputs)
        {
            string countryTaxIdRegexExpression = GetTaxIdDescriptionTaxRegexExpression(country);

            var invalidTaxIds = inValidTaxIdInputs.Split(',');
            foreach (var taxId in invalidTaxIds)
            {
                Assert.IsFalse(Regex.Match(taxId.Trim(), countryTaxIdRegexExpression).Success, $"country: {country}, taxId: {taxId.Trim()} {countryTaxIdRegexExpression}");
            }
        }

        private string GetTaxIdDescriptionTaxRegexExpression(string country)
        {
            const string Type = "commercial_tax_id";
            const string Language = "en-us";

            List<string> exposedFlights = new List<string>();

            List<PIDLResource> taxIdPidls =
                PIDLResourceFactory.Instance.GetTaxIdDescriptions(
                    country: country,
                    type: Type,
                    language: Language,
                    partnerName: "commercialstores",
                    profileType: "organization",
                    operation: "add",
                    isStandalone: true,
                    flightNames: exposedFlights,
                    scenario: string.Empty);

            return ((PropertyDescription)taxIdPidls[0].DataDescription["taxId"]).Validation.Regex;
        }
    }
}
