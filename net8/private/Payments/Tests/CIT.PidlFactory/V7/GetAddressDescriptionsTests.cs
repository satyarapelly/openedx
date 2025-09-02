// <copyright file="GetAddressDescriptionsTests.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace CIT.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Helpers;
    using Microsoft.Commerce.Payments.PidlFactory;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using PXCommon = Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Test.Common;
    using Microsoft.Commerce.Payments.PXCommon;
    using System.Text.RegularExpressions;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Newtonsoft.Json;

    [TestClass]
    public class GetAddressDescriptionsTests : UnitTestBase
    {
        readonly Dictionary<string, string[]> partnerScenarioLists = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            //// Billing Address
            {
                "Partners-V2Address",
                new string[] { "amc", "amcweb", "amcxbox", "appsource", "bingtravel", "commercialwebblends", "consumersupport", "default", "ggpdeds", "marketplace", "mseg", "northstarweb", "office", "officeoobe", "oxooobe", "officeoobeinapp", "onedrive", "oxodime", "oxowebdirect", "setupoffice", "setupofficesdx", "storeoffice", "test", "wallet", "webblends", "webblends_inline",  "xbox", "xboxweb", "windowssettings", "windowssubs", "windowsstore" }
            },
            {
                "Partners-V3Address",
                new string[] { "cart", "azure", "azuresignup", "azureibiza", "commercialstores", "smboobe", "webpay" }
            },
            {
                "Partners-NotApplicable",
                new string[] { "bing", "commercialsupport", "payin" }
            },
            //// Shipping Address
            {
                "Partners-V3AddressShippingV3WithSubmitLink",
                new string[] { "cart",  "commercialstores", "webpay" }
            },
            {
                "Partners-ConsumerSupportAddressShippingV3",
                new string[] { "consumersupport" }
            },
            {
                "Partners-XboxNativeAddressShippingV3WithSubmitLink",
                new string[] { "storify", "xboxsubs", "xboxsettings", "saturn" }
            },
            {
                "Partners-XboxNativeAddressBillingV3WithSubmitLink",
                new string[] { "storify", "xboxsubs", "xboxsettings", "saturn" }
            },
            {
                "Partners-V2AddressShippingV3WithSubmitLink",
                new string[] { "marketplace", "test" }
            },
            {
                "Partners-NonAddressShippingV3WithSubmitLink",
                new string[] { "amc", "amcweb", "amcxbox", "appsource", "azure", "azuresignup", "azureibiza", "bing", "bingtravel", "commercialsupport", "commercialwebblends", "default", "ggpdeds", "mseg", "northstarweb", "office", "officeoobe", "oxooobe", "officeoobeinapp", "onedrive", "oxodime", "oxowebdirect", "payin", "setupoffice", "setupofficesdx", "smboobe", "storeoffice", "wallet", "webblends", "webblends_inline",  "xbox", "xboxweb", "windowssettings", "windowssubs", "windowsstore" }
            },
            //// HapiServiceUsage Address
            {
                "Partners-HapiAddressWithSubmitLink",
                new string[] { "commercialstores" }
            },
            {
                "Partners-Skip",
                new string[] { "xboxnative", "windowsnative", "msteams", "selectpmbuttonlist", "selectpmradiobuttonlist", "selectpmdropdown", "onepage", "twopage", "listpidropdown", "defaulttemplate", "listpiradiobutton", "listpibuttonlist", "consoletemplate", "secondscreentemplate" }
            },
        };

        readonly List<string> addressSubmitButtonHintIds = new List<string>() { "submitButton", "saveButton", "saveButtonHidden", "saveNextButton", "submitButtonHidden", "doneSubmitButton" };

        [TestMethod]
        [TestCategory(TestCategory.TestCoverage)]
        public void GetAddressDescriptions_EnsureCITsCoverAllV2andV3Partners()
        {
            List<string[]> subsets = new List<string[]>() { partnerScenarioLists["Partners-V2Address"], partnerScenarioLists["Partners-V3Address"], partnerScenarioLists["Partners-NotApplicable"], partnerScenarioLists["Partners-XboxNativeAddressShippingV3WithSubmitLink"], partnerScenarioLists["Partners-Skip"] };
            UnitTestBase.TestPartnerSetCoverage(subsets, "V2 billing, V3 billing and skip billing Addresses");
        }

        [TestMethod]
        [TestCategory(TestCategory.TestCoverage)]
        public void GetAddressDescriptions_EnsureCITsCoverAllAddressShippingV3AndNonPartners()
        {
            UnitTestBase.TestPartnerSetCoverage(
                new List<string[]>()
                {
                    partnerScenarioLists["Partners-V3AddressShippingV3WithSubmitLink"],
                    partnerScenarioLists["Partners-V2AddressShippingV3WithSubmitLink"],
                    partnerScenarioLists["Partners-ConsumerSupportAddressShippingV3"],
                    partnerScenarioLists["Partners-NonAddressShippingV3WithSubmitLink"],
                    partnerScenarioLists["Partners-XboxNativeAddressShippingV3WithSubmitLink"],
                    partnerScenarioLists["Partners-Skip"],
                },
                "V3 Shipping Addresses with submit links for V3 Address Partners, V2 Address Partners, and all others");
        }

        [DataRow("Partners-V2Address", "billing", "profileAddressAutoSubmit", false, "https://{pifd-endpoint}/users/{userId}/addresses", "POST")]
        [DataRow("Partners-V3Address", "billing", "profileAddressAutoSubmit", false, "https://{jarvis-endpoint}/JarvisCM/{userId}/addresses", "POST")]
        [DataRow("Partners-V2Address", "billing", "profileAddressAutoSubmit", true, "https://{jarvis-endpoint}/JarvisCM/{userId}/addresses", "POST")]
        [DataRow("Partners-V3Address", "billing", "profileAddressAutoSubmit", true, "https://{jarvis-endpoint}/JarvisCM/{userId}/addresses", "POST")]
        [DataRow("Partners-ConsumerSupportAddressShippingV3", "shipping", null, false, "https://{jarvis-endpoint}/JarvisCM/{userId}/addresses", "POST")]
        [DataRow("Partners-ConsumerSupportAddressShippingV3", "shipping", null, true, "https://{jarvis-endpoint}/JarvisCM/{userId}/addresses", "POST")]
        [DataRow("Partners-V2AddressShippingV3WithSubmitLink", "shipping_v3", null, false, "https://{pifd-endpoint}/users/{userId}/addresses", "POST")]
        [DataRow("Partners-V3AddressShippingV3WithSubmitLink", "shipping_v3", null, false, "https://{jarvis-endpoint}/JarvisCM/{userId}/addresses", "POST")]
        [DataRow("Partners-HapiAddressWithSubmitLink", "hapiServiceUsageAddress", null, true, "https://{hapi-endpoint}/my-org/orders/{partnerData.prefillData.orderId}/orderservice.updateServiceUsageAddress", "POST")]
        [DataRow("Partners-V2AddressShippingV3WithSubmitLink", "shipping_v3", null, true, "https://{jarvis-endpoint}/JarvisCM/{userId}/addresses", "POST")]
        [DataRow("Partners-V3AddressShippingV3WithSubmitLink", "shipping_v3", null, true, "https://{jarvis-endpoint}/JarvisCM/{userId}/addresses", "POST")]
        [DataRow("Partners-XboxNativeAddressShippingV3WithSubmitLink", "px_v3_shipping", null, true, "https://{pifd-endpoint}/users/{userId}/addressesEx", "POST")]
        [DataRow("Partners-XboxNativeAddressBillingV3WithSubmitLink", "px_v3_billing", null, true, "https://{pifd-endpoint}/users/{userId}/addressesEx", "POST")]
        [DataTestMethod]
        public void GetAddressDescriptions_AddressSubmitLinkIsAsExpected(string partnerAddressGroup, string identity, string displayDescriptionId, bool overrideJarvisVersionToV3, string expectedHref, string expectedMethod)
        {
            var hrefBeforeXboxNativeStringChange = expectedHref;
            foreach (string partner in partnerScenarioLists[partnerAddressGroup])
            {
                if (partnerAddressGroup == "Partners-XboxNativeAddressShippingV3WithSubmitLink"
                  || partnerAddressGroup == "Partners-XboxNativeAddressBillingV3WithSubmitLink")
                {
                    expectedHref = hrefBeforeXboxNativeStringChange + $"?partner={partner}&language=en-US&avsSuggest=true";
                }

                this.TestContext.WriteLine("Start testing: Partner \"{0}\"", partner);
                List<PIDLResource> addressPidls = PIDLResourceFactory.Instance.GetAddressDescriptions("us", identity, "en-US", partner, displayDescriptionId, overrideJarvisVersionToV3);
                PXCommon.RestLink submitActionContext = null;
                UnitTestBase.AssertSubmitHintExists(addressPidls, addressSubmitButtonHintIds, "Address", out submitActionContext);
                Assert.AreEqual(expectedHref, submitActionContext.Href, "Address submit display hint action context is expected to match the href provided");
                Assert.AreEqual(expectedMethod, submitActionContext.Method, "Address submit display hint action context is expected to match the method provided");
                this.TestContext.WriteLine("...done");
            }
        }

        [DataRow("oxowebdirect", "billing", null, true, false, "https://{jarvis-endpoint}/JarvisCM/{userId}/addresses", "POST", 3)]
        [DataRow("oxowebdirect", "billing", null, true, true, "https://{pifd-endpoint}/users/{userId}/addressesEx?partner=oxowebdirect&language=en-US&avsSuggest=true", "POST", null)]
        [DataRow("webblends", "billing", null, true, false, "https://{jarvis-endpoint}/JarvisCM/{userId}/addresses", "POST", 3)]
        [DataRow("webblends", "billing", null, true, true, "https://{pifd-endpoint}/users/{userId}/addressesEx?partner=webblends&language=en-US&avsSuggest=true", "POST", null)]
        [DataRow("cart", "shipping", null, true, false, "https://{jarvis-endpoint}/JarvisCM/{userId}/addresses", "POST", 3)]
        [DataRow("cart", "shipping", null, true, true, "https://{pifd-endpoint}/users/{userId}/addressesEx?partner=cart&language=en-US&avsSuggest=true", "POST", null)]
        [DataRow("xbox", "billing", null, true, false, "https://{jarvis-endpoint}/JarvisCM/{userId}/addresses", "POST", 3)]
        [DataRow("xbox", "shipping", null, true, true, "https://{pifd-endpoint}/users/{userId}/addressesEx?partner=xbox&language=en-US&avsSuggest=true", "POST", null)]
        [DataTestMethod]
        public void GetAddressDescriptions_AddressSubmitLinkIsAsExpectedForAvsSuggestEnabledPartners(
            string partner,
            string identity,
            string displayDescriptionId,
            bool overrideJarvisVersionToV3,
            bool avsSuggest,
            string expectedHref,
            string expectedMethod,
            int? expectedHeaderCount)
        {
            this.TestContext.WriteLine("Start testing: Partner \"{0}\"", partner);
            List<PIDLResource> addressPidls = PIDLResourceFactory.Instance.GetAddressDescriptions("us", identity, "en-US", partner, displayDescriptionId, overrideJarvisVersionToV3, avsSuggest: avsSuggest);
            PXCommon.RestLink submitActionContext = null;
            UnitTestBase.AssertSubmitHintExists(addressPidls, addressSubmitButtonHintIds, "Address", out submitActionContext);
            Assert.AreEqual(expectedHref, submitActionContext.Href, "Address submit display hint action context is expected to match the href provided");
            Assert.AreEqual(expectedMethod, submitActionContext.Method, "Address submit display hint action context is expected to match the method provided");
            Assert.AreEqual(expectedHeaderCount, submitActionContext.Headers?.Count());
            this.TestContext.WriteLine("...done");
        }

        [TestMethod]
        public void PidlFactoryGetAddressDescriptions_us_billing_enus()
        {
            const string AddressCountry = "us";
            const string AddressType = "billing";
            const string AddressLanguage = "en-us";
            const string ExpectedDescriptionType = "address";
            List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetAddressDescriptions(AddressCountry, AddressType, AddressLanguage);

            // Check that we have something resembling an address description pidl
            PidlAssert.IsValid(pidls);
            foreach (PIDLResource pidl in pidls)
            {
                // Check a few of the address fields
                Assert.IsTrue(pidl.DataDescription.ContainsKey("address_line1"));
                Assert.IsTrue(pidl.DataDescription.ContainsKey("city"));
                Assert.IsTrue(pidl.DataDescription.ContainsKey("region"));

                // Validate the PIDL identiy
                ValidatePidlIdentity(pidl, AddressCountry, AddressType, ExpectedDescriptionType);
            }
        }

        [TestMethod]
        public void PidlFactoryGetAddressDescriptions_us_hapi_serviceusageaddress()
        {
            const string Partner = "commercialstores";
            const string AddressCountry = "us";
            const string AddressType = "hapiServiceUsageAddress";
            const string AddressScenario = "serviceusageaddress";
            const string AddressLanguage = "en-us";
            const string ExpectedDescriptionType = "address";
            string[] operations = new string[] { "add", "update" };

            foreach (string operation in operations)
            {
                List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetAddressDescriptions(AddressCountry, AddressType, AddressLanguage, Partner, scenario: AddressScenario, operation: operation);

                PidlAssert.IsValid(pidls, clientSidePrefill: operation == "update");
                foreach (PIDLResource pidl in pidls)
                {
                    Assert.IsNotNull(pidl.GetPropertyDescriptionByPropertyName("line1"));
                    Assert.IsNotNull(pidl.GetPropertyDescriptionByPropertyName("city"));
                    Assert.IsNotNull(pidl.GetPropertyDescriptionByPropertyName("state"));

                    ValidatePidlIdentity(pidl, AddressCountry, AddressType, ExpectedDescriptionType);
                }
            }
        }

        [DataRow("tr", true)]
        [DataRow("am", true)]
        [DataRow("no", true)]
        [DataRow("by", true)]
        [DataRow("cl", true)]
        [DataRow("mx", true)]
        [DataRow("my", true)]
        [DataRow("bd", true)]
        [DataRow("id", true)]
        [DataRow("th", true)]
        [DataRow("bh", true)]
        [DataRow("cm", true)]
        [DataRow("ge", true)]
        [DataRow("gh", true)]
        [DataRow("is", true)]
        [DataRow("ke", true)]
        [DataRow("md", true)]
        [DataRow("ng", true)]
        [DataRow("om", true)]
        [DataRow("tj", true)]
        [DataRow("ua", true)]
        [DataRow("uz", true)]
        [DataRow("zw", true)]
        [DataRow("fj", true)]
        [DataRow("gt", true)]
        [DataRow("kh", true)]
        [DataRow("ph", true)]
        [DataRow("vn", true)]
        [DataRow("ci", true)]
        [DataRow("sn", true)]
        [DataRow("zm", true)]
        [DataRow("la", true)]
        [DataTestMethod]
        public void PidlFactoryGetAddressDescriptions_tr_hapi_serviceusageaddress_HasDisabledTaxPidl(
            string addressCountry,
            bool expectedContainTaxForm)
        {
            // Arrange
            const string Partner = "commercialstores";
            const string AddressType = "hapiServiceUsageAddress";
            const string AddressScenario = "serviceusageaddress";
            const string AddressLanguage = "en-us";
            const string ExpectedType = "hapiServiceUsageAddressDisabledTax";
            string[] operations = new string[] { "add", "update" };

            foreach (string operation in operations)
            {
                // Act
                List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetAddressDescriptions(addressCountry, AddressType, AddressLanguage, Partner, scenario: AddressScenario, operation: operation);

                // Assert
                if (expectedContainTaxForm)
                {
                    PidlAssert.IsValid(pidls);
                    Assert.AreEqual(2, pidls.Count);
                    PIDLResource disabledTaxPidl = pidls.FirstOrDefault((pidl) => pidl.Identity.ContainsKey(TestConstants.DescriptionIdentityFields.Type)
                                                                                && string.Equals(pidl.Identity[TestConstants.DescriptionIdentityFields.Type], ExpectedType, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(disabledTaxPidl);
                }
                else
                {
                    // Ensure the taxID form isn't added to profile description page for specific countries
                    PidlAssert.IsValid(pidls);
                    Assert.AreEqual(1, pidls.Count);
                }
            }
        }

        [DataRow("commercialstores", true, true)]
        [DataRow("commercialstores", false, true)]
        [DataRow("commercialstores", false, true)]
        [DataRow("officesmb", true, true)]
        [DataRow("officesmb", false, false)]
        [DataRow("officesmb", false, false)]
        [DataTestMethod]
        public void PidlFactoryGetAddressDescriptions_tr_hapi_serviceusageaddress_HasDisabledTaxPidl_Feature(string partner, bool useFeature, bool expectedContainTaxForm)
        {
            // Arrange
            var countries = new string[] { "tr", "am", "no", "by", "cl", "mx", "my", "bd", "id", "th", "bh", "cm", "ge", "gh", "is", "ke", "md", "ng", "om", "tj", "ua", "uz", "zw", "fj", "gt", "kh", "ph", "vn" };
            const string ExpectedType = "hapiServiceUsageAddressDisabledTax";
            string[] operations = new string[] { "add", "update" };

            string settingStr = "{\"template\":\"defaulttemplate\",\"features\":null}";
            string featureSettingStr = "{\"template\":\"defaulttemplate\",\"features\":{\"addHapiSUADisabledTaxResourceId\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null}}}";

            PaymentExperienceSetting setting = JsonConvert.DeserializeObject<PaymentExperienceSetting>(useFeature ? featureSettingStr : settingStr);

            foreach (string operation in operations)
            {
                foreach (string country in countries)
                {
                    // Act
                    List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetAddressDescriptions(country, "hapiServiceUsageAddress", "en-us", partner, scenario: "serviceusageaddress", operation: operation, setting: setting);

                    // Assert
                    if (expectedContainTaxForm)
                    {
                        PidlAssert.IsValid(pidls, 2);
                        PIDLResource disabledTaxPidl = pidls.FirstOrDefault((pidl) => pidl.Identity.ContainsKey(TestConstants.DescriptionIdentityFields.Type)
                            && string.Equals(pidl.Identity[TestConstants.DescriptionIdentityFields.Type], ExpectedType, StringComparison.OrdinalIgnoreCase));

                        Assert.IsNotNull(disabledTaxPidl);
                    }
                    else
                    {
                        // Ensure the taxID form isn't added to profile description page for specific countries
                        PidlAssert.IsValid(pidls, 1);
                    }
                }
            }
        }

        [DataRow("billing", "azure", null, "submit", "https://{pifd-endpoint}/anonymous/addresses/legacyValidate")]
        [DataRow("shipping_v3", "azure", null, "submit", "https://{pifd-endpoint}/anonymous/addresses/legacyValidate")]
        [DataRow("shipping_v3", "commercialstores", "commercialhardware", "submit", "https://{pifd-endpoint}/anonymous/addresses/modernValidate")]
        [DataRow("shipping_v3", "defaulttemplate", "commercialhardware", "submit", "https://{pifd-endpoint}/anonymous/addresses/modernValidate")]
        [DataRow("billing", "bing", null, "submit", "https://{pifd-endpoint}/anonymous/addresses/legacyValidate")]
        [DataRow("hapiV1SoldToOrganization", "commercialstores", null, "validate", "https://{pifd-endpoint}/anonymous/addresses/legacyValidate?type=hapiV1SoldToOrganization")]
        [DataRow("hapiV1ShipToOrganization", "commercialstores", null, "validate", "https://{pifd-endpoint}/anonymous/addresses/legacyValidate?type=hapiV1ShipToOrganization")]
        [DataRow("hapiV1BillToOrganization", "commercialstores", null, "validate", "https://{pifd-endpoint}/anonymous/addresses/legacyValidate?type=hapiV1BillToOrganization")]
        [DataRow("hapiV1SoldToOrganization", "azure", null, "validate", "https://{pifd-endpoint}/anonymous/addresses/legacyValidate?type=hapiV1SoldToOrganization")]
        [DataRow("hapiV1ShipToOrganization", "azure", null, "validate", "https://{pifd-endpoint}/anonymous/addresses/legacyValidate?type=hapiV1ShipToOrganization")]
        [DataRow("hapiV1BillToOrganization", "azure", null, "validate", "https://{pifd-endpoint}/anonymous/addresses/legacyValidate?type=hapiV1BillToOrganization")]
        [TestMethod]
        public void PidlFactoryGetAddressDescriptions_us_VerifyValidationLink(string type, string partner, string scenario, string actionType, string expectedLink)
        {
            // Arrange
            const string AddressCountry = "us";
            const string AddressLanguage = "en-us";
            const string ExpectedDescriptionType = "address";
            string[] operations = new string[] { "add", "update" };
            Func<DisplayHint, string, bool> buttonHasLink = (hint, link) =>
            {
                var button = hint as ButtonDisplayHint;
                return button.Action != null
                    && button.Action.ActionType == actionType
                    && button.Action.Context != null
                    && button.Action.Context is RestLink
                    && string.Equals((button.Action.Context as RestLink).Href, expectedLink, StringComparison.OrdinalIgnoreCase);
            };

            foreach (string operation in operations)
            {
                // Act
                List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetAddressDescriptions(AddressCountry, type, AddressLanguage, partner, scenario: scenario, operation: operation);

                PidlAssert.IsValid(pidls);
                foreach (PIDLResource pidl in pidls)
                {
                    // Assert
                    var validationButton = pidl.GetDisplayHints().FirstOrDefault(hint => hint is ButtonDisplayHint && buttonHasLink(hint, expectedLink));
                    Assert.IsNotNull(validationButton, $"Button with action link {expectedLink} is missing");

                    ValidatePidlIdentity(pidl, AddressCountry, type, ExpectedDescriptionType);
                }
            }
        }

        [DataRow("shipping_v3", "commercialstores", "commercialhardware", "submit", "us", "https://{pifd-endpoint}/anonymous/addresses/modernValidate")]
        [DataRow("shipping_v3", "commercialstores", "commercialhardware", "submit", "nl", "https://{pifd-endpoint}/anonymous/addresses/legacyValidate")]
        [DataRow("shipping_v3", "commercialstores", "commercialhardware", "submit", "gb", "https://{pifd-endpoint}/anonymous/addresses/legacyValidate")]
        [DataRow("shipping_v3", "defaultTemplate", "commercialhardware", "submit", "us", "https://{pifd-endpoint}/anonymous/addresses/modernValidate")]
        [DataRow("shipping_v3", "defaultTemplate", "commercialhardware", "submit", "gb", "https://{pifd-endpoint}/anonymous/addresses/modernValidate")]
        [TestMethod]
        public void PidlFactoryGetAddressDescriptions_VerifyValidationLink(string type, string partner, string scenario, string actionType, string country, string expectedLink)
        {
            // Arrange
            const string AddressLanguage = "en-us";
            const string ExpectedDescriptionType = "address";
            string[] operations = new string[] { "add", "update" };
            Func<DisplayHint, string, bool> buttonHasLink = (hint, link) =>
            {
                var button = hint as ButtonDisplayHint;
                return button.Action != null
                    && button.Action.ActionType == actionType
                    && button.Action.Context != null
                    && button.Action.Context is RestLink
                    && string.Equals((button.Action.Context as RestLink).Href, expectedLink, StringComparison.OrdinalIgnoreCase);
            };

            foreach (string operation in operations)
            {
                // Act
                List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetAddressDescriptions(country, type, AddressLanguage, partner, scenario: scenario, operation: operation);

                PidlAssert.IsValid(pidls);
                foreach (PIDLResource pidl in pidls)
                {
                    // Assert
                    var validationButton = pidl.GetDisplayHints().FirstOrDefault(hint => hint is ButtonDisplayHint && buttonHasLink(hint, expectedLink));
                    Assert.IsNotNull(validationButton, $"Button with action link {expectedLink} is missing");

                    ValidatePidlIdentity(pidl, country, type, ExpectedDescriptionType);
                }
            }
        }

        [TestMethod]
        [DataRow("us", null, new string[] { "addressLine2" })]
        [DataRow("us", "displayOptionalFields", new string[] { "addressLine2" })]
        [DataRow("in", null, new string[] { "addressLine2", "addressLine3" })]
        [DataRow("in", "displayOptionalFields", new string[] { "addressLine2", "addressLine3" })]
        public void PidlFactoryGetAddressDescriptions_DisplayOptionalFields(string country, string scenario, string[] optionalFieldsInTest)
        {
            string type = "billing";

            string[] operations = new string[]
            {
                "add",
                "update"
            };

            string[] partners = new string[]
            {
                TestConstants.PartnerNames.WebblendsInline,
                TestConstants.PartnerNames.Xbox,
                TestConstants.PartnerNames.OXODIME,
                TestConstants.PartnerNames.OXOWebDirect
            };

            foreach (string partner in partners)
            {
                foreach (string operation in operations)
                {
                    foreach (string optionalField in optionalFieldsInTest)
                    {
                        List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetAddressDescriptions(country, type, "en-US", partner, scenario: scenario);
                        Assert.IsNotNull(pidls, "Returned PIDLs are expected to be not null");
                        foreach (PIDLResource pidl in pidls)
                        {
                            Assert.IsNotNull(pidl, "PIDL is expected to be not null");
                            PropertyDisplayHint optionalFieldObj = pidl.GetDisplayHintById(optionalField) as PropertyDisplayHint;
                            Assert.IsNotNull(optionalFieldObj, $"{optionalField} missing for {country} market");
                            bool isHiddenProperty = optionalFieldObj.IsHidden.HasValue && optionalFieldObj.IsHidden.Value;

                            if (string.Equals(partner, TestConstants.PartnerNames.Xbox, StringComparison.OrdinalIgnoreCase)
                               || (string.Equals(partner, TestConstants.PartnerNames.WebblendsInline, StringComparison.OrdinalIgnoreCase)
                                   && !string.Equals(scenario, "displayOptionalFields", StringComparison.OrdinalIgnoreCase)))
                            {
                                Assert.IsTrue(isHiddenProperty, "IsHidden property was not set to true for expected partner under null scenario");
                            }
                            else
                            {
                                Assert.IsFalse(isHiddenProperty, "IsHidden property was not set to false for expected partner under displayOptionalFields scenario");
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void PidlFactoryGetAddressDescriptions_xk_billing()
        {
            const string AddressCountry = "xk";
            const string AddressType = "billing";
            const string AddressLanguage = "en-us";
            const string ExpectedDescriptionType = "address";
            List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetAddressDescriptions(AddressCountry, AddressType, AddressLanguage);

            // Check that we have an address description pidl
            PidlAssert.IsValid(pidls);
            foreach (PIDLResource pidl in pidls)
            {
                // Check the address fields for "XK" A1CtPcStCo
                Assert.IsTrue(pidl.DataDescription.ContainsKey("address_line1"));
                Assert.IsTrue(pidl.DataDescription.ContainsKey("city"));
                Assert.IsTrue(pidl.DataDescription.ContainsKey("postal_code"));
                Assert.IsTrue(pidl.DataDescription.ContainsKey("region"));
                Assert.IsTrue(pidl.DataDescription.ContainsKey("country"));
                Assert.IsTrue(!pidl.DataDescription.ContainsKey("address_line2"));
                Assert.IsTrue(!pidl.DataDescription.ContainsKey("address_line3"));

                // Validate the PIDL identiy
                ValidatePidlIdentity(pidl, AddressCountry, AddressType, ExpectedDescriptionType);
            }
        }

        private static void ValidatePidlIdentity(PIDLResource pidl, string addressCountry, string addressType, string expectedDescriptionType)
        {
            Assert.IsTrue(pidl.Identity.ContainsKey(TestConstants.DescriptionIdentityFields.Country));
            Assert.IsTrue(string.Equals(pidl.Identity[TestConstants.DescriptionIdentityFields.Country], addressCountry, StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(pidl.Identity.ContainsKey(TestConstants.DescriptionIdentityFields.Type));
            Assert.IsTrue(string.Equals(pidl.Identity[TestConstants.DescriptionIdentityFields.Type], addressType, StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(pidl.Identity.ContainsKey(TestConstants.DescriptionIdentityFields.DescriptionType));
            Assert.IsTrue(string.Equals(pidl.Identity[TestConstants.DescriptionIdentityFields.DescriptionType], expectedDescriptionType, StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        [ExpectedException(typeof(PIDLArgumentException))]
        public void PidlFactoryGetAddressDescriptions_nullCountry()
        {
            PIDLResourceFactory.Instance.GetAddressDescriptions(null, "billing", "en-us");
        }

        [TestMethod]
        [ExpectedException(typeof(PIDLArgumentException))]
        public void PidlFactoryGetAddressDescriptions_badCountry()
        {
            PIDLResourceFactory.Instance.GetAddressDescriptions("9o5", "billing", "en-us");
        }

        [TestMethod]
        [ExpectedException(typeof(PIDLArgumentException))]
        public void PidlFactoryGetAddressDescriptions_nullType()
        {
            PIDLResourceFactory.Instance.GetAddressDescriptions("en", null, "en-us");
        }

        [TestMethod]
        [ExpectedException(typeof(PIDLArgumentException))]
        public void PidlFactoryGetAddressDescriptions_badType()
        {
            PIDLResourceFactory.Instance.GetAddressDescriptions("en", "badType", "en-us");
        }

        [TestMethod]
        [ExpectedException(typeof(PIDLArgumentException))]
        public void PidlFactoryGetAddressDescriptions_nullLanguage()
        {
            PIDLResourceFactory.Instance.GetAddressDescriptions("en", "billing", null);
        }

        [TestMethod]
        [ExpectedException(typeof(PIDLArgumentException))]
        public void PidlFactoryGetAddressDescriptions_badLanguage()
        {
            PIDLResourceFactory.Instance.GetAddressDescriptions("en", "billing", "zz-11");
        }

        [DataRow("billing", "add")]
        [DataRow("billing", "update")]
        [DataRow("shipping_v3", "add")]
        [DataRow("shipping_v3", "update")]
        [TestMethod]
        public void PidlFactoryExpectedStatesForUSAddress(string type, string operation)
        {
            const string AddressCountry = "us";
            const string AddressLanguage = "en-us";

            List<string> currentTestPartners = new List<string>()
            {
                TestConstants.PartnerNames.Azure,
                TestConstants.PartnerNames.Cart,
                TestConstants.PartnerNames.ConsumerSupport,
            };

            Dictionary<string, string> expectedAllStateValues = PIDLResourceFactory.GetCopiedDictionaryFromDomainDictionaries(TestConstants.DomainDictionaryNames.USStates);

            foreach (string partner in currentTestPartners)
            {
                List<PIDLResource> pidls = null;

                pidls = PIDLResourceFactory.Instance.GetAddressDescriptions(AddressCountry, type, AddressLanguage, partner, operation: operation);
                PidlAssert.IsValid(pidls);

                foreach (PIDLResource pidl in pidls)
                {
                    PropertyDescription propertyDescription = pidl.GetPropertyDescriptionByPropertyName(TestConstants.DataDescriptionIds.Region);
                    PropertyDisplayHint displayHint = pidl.GetDisplayHintById(TestConstants.DisplayHintIds.AddressState) as PropertyDisplayHint;
                    Assert.IsNotNull(displayHint);
                    Assert.IsNotNull(propertyDescription);
                    Assert.AreEqual(expectedAllStateValues.Count, propertyDescription.PossibleValues.Count);
                    Assert.AreEqual(expectedAllStateValues.Count, displayHint.PossibleValues.Count, "DisplayHint state count was not as expected");
                    foreach (string key in expectedAllStateValues.Keys)
                    {
                        Assert.IsTrue(displayHint.PossibleValues.ContainsKey(key), string.Format("displayHint.PossibleValues does not contain key [{0}]", key));
                        Assert.AreEqual(expectedAllStateValues[key], displayHint.PossibleValues[key], string.Format("[{0}] key was not equal in the pidl and DomainDescriptions", key));
                        Assert.IsTrue(propertyDescription.PossibleValues.ContainsKey(key), string.Format("PropertyDescription.PossibleValues does not contain [{0}] key", key));
                        Assert.AreEqual(expectedAllStateValues[key], propertyDescription.PossibleValues[key], string.Format("[{0}] key was not equal in the pidl and DomainDescriptions", key));
                    }
                }
            }
        }

        [DataRow("tw", "10000")] //// ^[0-9]{3,3}((-|\s)?[0-9]{2,3})?
        [DataRow("tw", "100000")] //// ^[0-9]{3,3}((-|\s)?[0-9]{2,3})?
        [DataRow("tw", "100")] //// ^[0-9]{3,3}((-|\s)?[0-9]{2,3})?
        [DataRow("tw", "100-00")] //// ^[0-9]{3,3}((-|\s)?[0-9]{2,3})?
        [DataRow("tw", "100-000")] //// ^[0-9]{3,3}((-|\s)?[0-9]{2,3})?
        [DataTestMethod]
        public void ValidateAddressDescriptionRegex_Pass_ValidPostalCodeInputs(string country, string validPostalCodeInputs)
        {
            string postalCodeRegexExpression = GetAddressDescriptionsPostalCodeRegexExpression(country);
            Assert.IsNotNull(postalCodeRegexExpression);
            Assert.IsTrue(Regex.Match(validPostalCodeInputs.Trim(), postalCodeRegexExpression).Success, $"country: {country}, postal_code: {validPostalCodeInputs.Trim()} {postalCodeRegexExpression}");
        }

        private string GetAddressDescriptionsPostalCodeRegexExpression(string country)
        {
            const string Type = "billing";
            const string Language = "en-us";
            List<PIDLResource> addressPidls = PIDLResourceFactory.Instance.GetAddressDescriptions(
                country: country,
                type: Type,
                language: Language,
                partnerName: "commercialstores",
                operation: "add");
            return ((PropertyDescription)addressPidls[0].DataDescription["postal_code"]).Validation.Regex;
        }
    }
}
