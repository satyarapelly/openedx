// <copyright company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>

namespace CIT.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Helpers;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlFactory;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Test.Common;
    using Constants = Microsoft.Commerce.Payments.PidlFactory.V7.Constants;

    [TestClass]
    public class PidlResourceFactoryTests
    {
        [TestMethod]
        [TestCategory(TestCategory.SpecialCase)]
        public void TestCoverage_AllPartnersAreTested()
        {
            // Act
            var allPartnersExpected = Directory.GetDirectories(AppDomain.CurrentDomain.BaseDirectory + @"\V7\Config\DisplayDescriptions").Select(d => Path.GetFileName(d));
            allPartnersExpected = allPartnersExpected.Concat(new string[] { TestConstants.PartnerNames.AzureSignup, TestConstants.PartnerNames.AzureIbiza, TestConstants.PartnerNames.Storify, TestConstants.PartnerNames.XboxSubs, TestConstants.PartnerNames.XboxSettings, TestConstants.PartnerNames.Saturn, TestConstants.PartnerNames.WindowsSettings, TestConstants.PartnerNames.WindowsSubs, TestConstants.PartnerNames.WindowsStore });

            // Assert
            CollectionAssert.AreEquivalent(allPartnersExpected.ToList(), TestConstants.AllPartners.ToList(), "Partners list being tested is not the same as the partners' config folders");
        }

        [TestMethod]
        public void PidlFactoryIsCountryValid()
        {
            // Ensure that valid countries return true
            Assert.AreEqual<bool>(true, PIDLResourceFactory.IsCountryValid("us")); // USA
            Assert.AreEqual<bool>(true, PIDLResourceFactory.IsCountryValid("br")); // Britain
            Assert.AreEqual<bool>(true, PIDLResourceFactory.IsCountryValid("ru")); // Russia
            Assert.AreEqual<bool>(true, PIDLResourceFactory.IsCountryValid("cn")); // China
            Assert.AreEqual<bool>(true, PIDLResourceFactory.IsCountryValid("im")); // Isle of Man
            Assert.AreEqual<bool>(true, PIDLResourceFactory.IsCountryValid("gu")); // Guam
            Assert.AreEqual<bool>(true, PIDLResourceFactory.IsCountryValid("xk")); // Kosovo

            // Ensure that bogus countries return false
            Assert.AreEqual<bool>(false, PIDLResourceFactory.IsCountryValid("zz"));
            Assert.AreEqual<bool>(false, PIDLResourceFactory.IsCountryValid("1234"));
            Assert.AreEqual<bool>(false, PIDLResourceFactory.IsCountryValid("?"));
            Assert.AreEqual<bool>(false, PIDLResourceFactory.IsCountryValid("?&"));

            // Check error cases
            Assert.AreEqual<bool>(false, PIDLResourceFactory.IsCountryValid(null));
            Assert.AreEqual<bool>(false, PIDLResourceFactory.IsCountryValid(string.Empty));
            Assert.AreEqual<bool>(false, PIDLResourceFactory.IsCountryValid("\\n\\t"));
            Assert.AreEqual<bool>(false, PIDLResourceFactory.IsCountryValid("  "));
            Assert.AreEqual<bool>(false, PIDLResourceFactory.IsCountryValid("__"));
        }

        [TestMethod]
        public void PidlFactoryAddCupCreditCardSmsChallenge()
        {
            const string Language = "en-us";
            const string PhoneNumber = "123456789";
            PaymentInstrument pi = new PaymentInstrument();
            pi.PaymentInstrumentDetails = new PaymentInstrumentDetails() { Phone = PhoneNumber, Msisdn = "1234" };

            List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetSmsChallengeDescriptionForPI(pi, Language);
            string pidl = JsonConvert.SerializeObject(pidls);
            Assert.IsFalse(string.IsNullOrWhiteSpace(pidl));
            Assert.IsTrue(pidl.Contains(PhoneNumber));
            //// TODO: Validate more of the pidl details
        }

        /// <summary>
        /// This test is used to verify the for Jarvis profile V3 partners, the profile description should be V3.
        /// </summary>
        [TestMethod]
        public void PidlFactoryGetJarvisProfileApiVersion()
        {
            List<string> profileV3Partners = new List<string> { "commercialstores", "azure", "webpay", "Webpay", "defaulttemplate" };
            List<string> profileV2Partners = new List<string> { "cart", "Cart", "oxowebdirect", "webblends", "webblends_inline", "xbox" };

            foreach (string partner in profileV3Partners)
            {
                Assert.IsTrue(PIDLResourceFactory.IsJarvisProfileV3Partner(partner), partner + " should be Jarvis profile V3 partner");
            }

            foreach (string partner in profileV2Partners)
            {
                Assert.IsFalse(PIDLResourceFactory.IsJarvisProfileV3Partner(partner), partner + " should be Jarvis profile V2 partner");
            }

            string settingStr = "{\"template\":\"defaulttemplate\",\"features\":null}";
            string featureSettingStr = "{\"template\":\"defaulttemplate\",\"features\":{\"useJarvisV3ForProfile\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null}}}";

            PaymentExperienceSetting setting = JsonConvert.DeserializeObject<PaymentExperienceSetting>(settingStr);
            PaymentExperienceSetting featureSetting = JsonConvert.DeserializeObject<PaymentExperienceSetting>(featureSettingStr);

            foreach (string partner in profileV3Partners)
            {
                Assert.IsTrue(PIDLResourceFactory.IsJarvisProfileV3Partner(partner, "us", featureSetting), partner + " should be Jarvis profile V3 partner");
            }

            foreach (string partner in profileV2Partners)
            {
                Assert.IsFalse(PIDLResourceFactory.IsJarvisProfileV3Partner(partner, "us", setting), partner + " should be Jarvis profile V2 partner");

                // If partner is a v2 partner, but has the useJarvisV3ForProfile feature enabled they will return true from IsJarvisProfileV3Partner()
                Assert.IsTrue(PIDLResourceFactory.IsJarvisProfileV3Partner(partner, "us", featureSetting), partner + " should be Jarvis profile V3 partner when using useJarvisV3ForProfile feature.");
            }
        }

        /// <summary>
        /// This test is used to verify the for Jarvis address V3 partners, the address description should be V3.
        /// </summary>
        [TestMethod]
        public void PidlFactoryGetJarvisAddressApiVersion()
        {
            List<string> addressV3Partners = new List<string> { "commercialstores", "webpay", "cart", "Cart", "defaulttemplate" };
            List<string> addressV2Partners = new List<string> { "oxowebdirect", "webblends", "webblends_inline", "xbox", "Xbox" };

            foreach (string partner in addressV3Partners)
            {
                Assert.IsTrue(PIDLResourceFactory.IsJarvisAddressV3Partner(partner), partner + " should be Jarvis address V3 partner");
            }

            foreach (string partner in addressV2Partners)
            {
                Assert.IsFalse(PIDLResourceFactory.IsJarvisAddressV3Partner(partner), partner + " should be Jarvis address V2 partner");
            }

            string settingStr = "{\"template\":\"defaulttemplate\",\"features\":null}";
            string featureSettingStr = "{\"template\":\"defaulttemplate\",\"features\":{\"useJarvisV3ForAddress\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null}}}";

            PaymentExperienceSetting setting = JsonConvert.DeserializeObject<PaymentExperienceSetting>(settingStr);
            PaymentExperienceSetting featureSetting = JsonConvert.DeserializeObject<PaymentExperienceSetting>(featureSettingStr);

            foreach (string partner in addressV3Partners)
            {
                Assert.IsTrue(PIDLResourceFactory.IsJarvisAddressV3Partner(partner, "us", featureSetting), partner + " should be Jarvis address V3 partner");
            }

            foreach (string partner in addressV2Partners)
            {
                Assert.IsFalse(PIDLResourceFactory.IsJarvisAddressV3Partner(partner, "us", setting), partner + " should be Jarvis address V2 partner");

                // If partner is a v2 partner, but has the useJarvisV3ForAddress feature enabled they will return true from IsJarvisAddressV3Partner()
                Assert.IsTrue(PIDLResourceFactory.IsJarvisAddressV3Partner(partner, "us", featureSetting), partner + " should be Jarvis address V3 partner when using useJarvisV3ForAddress feature.");
            }
        }

        [TestMethod]
        public void PidlFactoryGetBillingGroupDescriptions()
        {
            const string Operation = "add";
            const string Country = "us";
            const string Language = "en-US";
            List<string> partners = new List<string> { "commercialstores" };
            string[] bgTypes = new string[] { "lightweight", "lightweightv7" };

            foreach (string partner in partners)
            {
                foreach (var type in bgTypes)
                {
                    List<PIDLResource> billingGroupDescriptionsPidl = PIDLResourceFactory.Instance.GetBillingGroupDescriptions(type, Operation, Country, Language, partner);
                    PidlAssert.IsValid(billingGroupDescriptionsPidl);

                    HyperlinkDisplayHint addNewPaymentInstrumentHyperLink = billingGroupDescriptionsPidl[0].GetDisplayHintById(TestConstants.DisplayHintIds.BillingGroupLightWeightAddNewPaymentInstrument) as HyperlinkDisplayHint;
                    Assert.AreEqual(addNewPaymentInstrumentHyperLink.Action.ActionType, DisplayHintActionType.partnerAction.ToString(), "Adding a new payment instrument should be a partner action");

                    var actionContext = addNewPaymentInstrumentHyperLink.Action.Context as ActionContext;
                    Assert.AreEqual(actionContext.Action, PaymentInstrumentActions.ToString(PIActionType.SelectResourceType), "The action needs to be select resource type");
                    Assert.AreEqual(actionContext.ResourceActionContext.PidlDocInfo.ResourceType, TestConstants.DescriptionTypes.PaymentMethodDescription, "The resource type should be payment method description");
                }
            }
        }

        [TestMethod]
        public void VerifySubmitLinkForAddressDescriptionsPIDL_BillingGroup()
        {
            // for commercial stores legal entity update
            List<PIDLResource> addressDescriptionsPidl = PIDLResourceFactory.Instance.GetAddressDescriptions("us", "billingGroup", "en-us", "commercialstores");
            PidlAssert.IsValid(addressDescriptionsPidl);

            ButtonDisplayHint saveButtonDisplayHint = addressDescriptionsPidl[0].GetDisplayHintById(TestConstants.ButtonDisplayHintIds.SaveButton) as ButtonDisplayHint;
            Assert.AreEqual(saveButtonDisplayHint.Action.ActionType, DisplayHintActionType.submit.ToString(), "Save button should exist to submit the data");

            var actionContext = saveButtonDisplayHint.Action.Context as Microsoft.Commerce.Payments.PXCommon.RestLink;
            Assert.IsNull(actionContext.Href, "Expected no Href link");
            Assert.IsNull(actionContext.Method, "Expected no method type");
            Assert.IsTrue(actionContext.ErrorCodeExpressions.SequenceEqual(new[] { "({contextData.code}.{contextData.parameters.property_name})", "({contextData.code})" }), "errorCodeExpressions are not as expected");
            Assert.IsNull(actionContext.Headers, "Expected no headers");
        }

        [TestMethod]
        public void VerifySubmitLinkForAddressDescriptionsPIDL_billing()
        {
            // for commercial stores legal entity update
            List<PIDLResource> addressDescriptionsPidl = PIDLResourceFactory.Instance.GetAddressDescriptions("us", "billing", "en-us", "commercialstores");
            PidlAssert.IsValid(addressDescriptionsPidl);

            ButtonDisplayHint saveButtonDisplayHint = addressDescriptionsPidl[0].GetDisplayHintById(TestConstants.ButtonDisplayHintIds.SaveButton) as ButtonDisplayHint;
            Assert.AreEqual(saveButtonDisplayHint.Action.ActionType, DisplayHintActionType.submit.ToString(), "Save button should exist to submit the data");

            var actionContext = saveButtonDisplayHint.Action.Context as Microsoft.Commerce.Payments.PXCommon.RestLink;
            Assert.AreEqual(actionContext.Href, "https://{jarvis-endpoint}/JarvisCM/{userId}/addresses", "Submit Href link is not as expected");
            Assert.AreEqual(actionContext.Method, "post", true, "Submit method type is not as expected");

            Dictionary<string, string> expectedHeaders = new Dictionary<string, string>();
            expectedHeaders["api-version"] = "2015-03-31";
            expectedHeaders["x-ms-correlation-id"] = actionContext.Headers["x-ms-correlation-id"];
            expectedHeaders["x-ms-tracking-id"] = actionContext.Headers["x-ms-tracking-id"];
            Assert.IsNull(actionContext.ErrorCodeExpressions, "Expected no ErrorCodeExpressions");
            Assert.IsTrue(expectedHeaders.Count == actionContext.Headers.Count && !expectedHeaders.Except(actionContext.Headers).Any(), "Expected headers are not found");
        }

        [TestMethod]
        public void VerifySubmitLinkAndContextForLegalEntityUpdate()
        {
            // for commercial stores legal entity update
            List<PIDLResource> profileDescriptionsPidl = PIDLResourceFactory.Instance.GetProfileDescriptions("us", "legalentity", "update", "en-us", "commercialstores", null, null, null, false, null);
            PidlAssert.IsValid(profileDescriptionsPidl, clientSidePrefill: true);
            Assert.AreEqual("update", profileDescriptionsPidl[0].Identity["operation"], "expected operation is not found in PIDL identity");

            ButtonDisplayHint saveButtonDisplayHint = profileDescriptionsPidl[0].GetDisplayHintById(TestConstants.ButtonDisplayHintIds.SaveButton) as ButtonDisplayHint;
            Assert.AreEqual(saveButtonDisplayHint.Action.ActionType, DisplayHintActionType.submit.ToString(), "Save button should exist to submit the data");

            var actionContext = saveButtonDisplayHint.Action.Context as Microsoft.Commerce.Payments.PXCommon.RestLink;
            Assert.AreEqual(actionContext.Href, "https://{hapi-endpoint}/{userId}/customerService/{id}/updateSoldToAddress", "Submit Href link is not as expected");
            Assert.AreEqual(actionContext.Method, "put", true, "Submit method type is not as expected");
            Assert.IsTrue(actionContext.ErrorCodeExpressions.SequenceEqual(new[] { "({contextData.code}.{contextData.parameters.property_name})", "({contextData.code})" }), "errorCodeExpressions are not as expected");
            Assert.IsNull(actionContext.Headers, "Expected no headers");
        }

        [TestMethod]
        public void VerifySubmitLinkAndContextForOrgProfileUpdate()
        {
            // for v3 org profile update
            string resourceId = "sampleProfileId";
            Dictionary<string, string> profileV3Headers = new Dictionary<string, string>() { { "sampleHeaderOne", "sampleHeaderOneValue" }, { "sampleHeaderTwo", "sampleHeaderTwoValue" } };

            List<PIDLResource> profileDescriptionsPidl = PIDLResourceFactory.Instance.GetProfileDescriptions("us", "organization", "update", "en-us", "commercialstores", null, resourceId, profileV3Headers, true, null, null);
            PidlAssert.IsValid(profileDescriptionsPidl);
            Assert.AreEqual("update", profileDescriptionsPidl[0].Identity["operation"], "expected operation is not found in PIDL identity");

            ButtonDisplayHint saveButtonDisplayHint = profileDescriptionsPidl[0].GetDisplayHintById(TestConstants.ButtonDisplayHintIds.SaveButton) as ButtonDisplayHint;
            Assert.AreEqual(saveButtonDisplayHint.Action.ActionType, DisplayHintActionType.submit.ToString(), "Save button should exist to submit the data");

            var actionContext = saveButtonDisplayHint.Action.Context as Microsoft.Commerce.Payments.PXCommon.RestLink;
            Assert.AreEqual(actionContext.Href, string.Format("https://{{jarvis-endpoint}}/JarvisCM/{{userId}}/profiles/{0}", resourceId), "Submit Href link is not as expected");
            Assert.AreEqual(actionContext.Method, "put", true, "Submit method type is not as expected");

            Dictionary<string, string> expectedHeaders = profileV3Headers;
            expectedHeaders["api-version"] = "2015-03-31";
            expectedHeaders["x-ms-correlation-id"] = actionContext.Headers["x-ms-correlation-id"];
            expectedHeaders["x-ms-tracking-id"] = actionContext.Headers["x-ms-tracking-id"];
            Assert.IsNull(actionContext.ErrorCodeExpressions, "Expected no ErrorCodeExpressions");
            Assert.IsTrue(expectedHeaders.Count == actionContext.Headers.Count && !expectedHeaders.Except(actionContext.Headers).Any(), "Expected headers are not found");
        }

        [TestMethod]
        public void VerifySubmitLinkTaxIdDescriptions_CommercialType()
        {
            string[] commercialProfileTypes = { "organization", "legalentity", "employee" };
            foreach (var profileType in commercialProfileTypes)
            {
                List<PIDLResource> profileDescriptionsPidl = PIDLResourceFactory.Instance.GetTaxIdDescriptions("gb", "commercial_tax_id", "en-us", "commercialstores", profileType, "update", false);
                PidlAssert.IsValid(profileDescriptionsPidl);

                ButtonDisplayHint saveButtonDisplayHint = profileDescriptionsPidl[0].GetDisplayHintById(TestConstants.ButtonDisplayHintIds.SaveButtonHidden) as ButtonDisplayHint;
                Assert.AreEqual(saveButtonDisplayHint.Action.ActionType, DisplayHintActionType.submit.ToString(), "Save button should exist to submit the data");

                var actionContext = saveButtonDisplayHint.Action.Context as Microsoft.Commerce.Payments.PXCommon.RestLink;
                Assert.AreEqual(actionContext.Href, "https://{hapi-endpoint}/{userId}/taxids", "Submit Href link is not as expected");
                Assert.AreEqual(actionContext.Method, "post", true, "Submit method type is not as expected");
            }
        }

        [TestMethod]
        public void VerifySubmitLinkTaxIdDescriptions_ConsumerType()
        {
            string[] consumerProfileTypes = { "consumer", "isv" };
            foreach (var profileType in consumerProfileTypes)
            {
                List<PIDLResource> profileDescriptionsPidl = PIDLResourceFactory.Instance.GetTaxIdDescriptions("br", "consumer_tax_id", "en-us", "webblends", profileType, "update", false);
                PidlAssert.IsValid(profileDescriptionsPidl);

                ButtonDisplayHint saveButtonDisplayHint = profileDescriptionsPidl[0].GetDisplayHintById(TestConstants.ButtonDisplayHintIds.SaveButtonHidden) as ButtonDisplayHint;
                Assert.AreEqual(saveButtonDisplayHint.Action.ActionType, DisplayHintActionType.submit.ToString(), "Save button should exist to submit the data");

                var actionContext = saveButtonDisplayHint.Action.Context as Microsoft.Commerce.Payments.PXCommon.RestLink;
                Assert.AreEqual(actionContext.Href, string.Format("https://{{pifd-endpoint}}/users/{{userId}}/tax-ids?country=br&language=en-us&profileType={0}", profileType), "Submit Href link is not as expected");
                Assert.AreEqual(actionContext.Method, "post", true, "Submit method type is not as expected");
            }
        }

        [TestMethod]
        public void VerifySubmitLinkTaxIdDescriptions_Standalone()
        {
            // Arrange
            string[] partners = { TestConstants.PartnerNames.CommercialStores, TestConstants.PartnerNames.Azure, TestConstants.PartnerNames.AzureIbiza, TestConstants.PartnerNames.AzureSignup };
            var hapiResponses = JArray.Parse("[{\"error\": {\"code\": \"tax_id_not_updatable\", \"message\": \"TaxId is not updatable: compliance policy prevents updates for TaxIds in PT.\", \"parameters\": {\"is_retriable\": \"false\"}}},{\"error\": {\"code\": \"ValidationError\", \"detail\": {\"code\": \"tax_id_not_updatable\", \"message\": \"NIF cannot be edited.\", \"parameters\": {\"is_retriable\": \"false\"}}}},{\"error\": {\"details\": [{\"errorCode\": \"tax_id_not_updatable\", \"message\": \"TaxId is not updatable: compliance policy prevents updates for TaxIds in PT.\"}], \"errorCode\": \"ValidationError\"}}]");
            string[] errorCodeExpressions = new[] { "({contextData.error.details[0].errorCode})", "({contextData.error.detail.code})", "({contextData.error.code})" };

            foreach (string partner in partners)
            {
                // Act
                List<PIDLResource> profileDescriptionsPidl = PIDLResourceFactory.Instance.GetTaxIdDescriptions("gb", "commercial_tax_id", "en-us", partner, "organization", "add", true, null, "withCountryDropdown");

                // Assert
                PidlAssert.IsValid(profileDescriptionsPidl);

                ButtonDisplayHint saveButtonDisplayHint = profileDescriptionsPidl[0].GetDisplayHintById(TestConstants.ButtonDisplayHintIds.SaveButton) as ButtonDisplayHint;
                Assert.AreEqual(saveButtonDisplayHint.Action.ActionType, DisplayHintActionType.submit.ToString(), "Save button should exist to submit the data");

                var actionContext = saveButtonDisplayHint.Action.Context as Microsoft.Commerce.Payments.PXCommon.RestLink;
                Assert.AreEqual(actionContext.Href, "https://{hapi-endpoint}/{userId}/taxids?accountId={partnerData.prefillData.accountId}&organizationId={partnerData.prefillData.organizationId}", "Submit Href link is not as expected");
                Assert.AreEqual(actionContext.Method, "post", true, "Submit method type is not as expected");

                if (TestConstants.IsAzurePartner(partner))
                {
                    Assert.IsTrue(actionContext.ErrorCodeExpressions.SequenceEqual(errorCodeExpressions), "errorCodeExpressions are not as expected");

                    foreach (JToken response in hapiResponses)
                    {
                        string errorCodeName = string.Empty;

                        foreach (string errorCodeExpression in actionContext.ErrorCodeExpressions)
                        {
                            string errorCodePath = errorCodeExpression.Replace("({contextData.", string.Empty).Replace("})", string.Empty);
                            errorCodeName = response.SelectToken("$.." + errorCodePath)?.ToString();

                            if (!string.IsNullOrEmpty(errorCodeName))
                            {
                                break;
                            }
                        }

                        Assert.AreEqual("tax_id_not_updatable", errorCodeName);
                    }
                }
                else
                {
                    Assert.IsTrue(actionContext.ErrorCodeExpressions.SequenceEqual(new[] { "({contextData.error.code})" }), "errorCodeExpressions are not as expected");
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(PIDLArgumentException))]
        public void PidlResourceFactory_GetDictionaryFromConfigString_EmptyInput()
        {
            // Act
            PIDLResourceFactory.GetDictionaryFromConfigString(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(PIDLConfigException))]
        public void PidlResourceFactory_GetDictionaryFromConfigString_UnknownCollectionName()
        {
            // Act
            PIDLResourceFactory.GetDictionaryFromConfigString("{}UnknownCollection");
        }

        [TestMethod]
        [ExpectedException(typeof(PIDLConfigException))]
        public void PidlResourceFactory_GetDictionaryFromConfigString_ExtraDelimiter()
        {
            // Act
            PIDLResourceFactory.GetDictionaryFromConfigString("Name=Value=ExtraValue");
        }

        [TestMethod]
        public void AddLinkedPidlToResourceList_LinksSuccessfully()
        {
            // Arrange
            var partnerToTest = "test"; // because the focus of this test is the PidlFactory code that is common to all partners.
            var profileTypeToTest = "organization"; // because this Pidl has an empty PidlContainer necessary for this test.
            var countryToTest = "tw"; // a country that has commercial tax.
            var pidlIndexToTest = 8;  // arbitrary number for testing.  
            var submitOrderToTest = PidlContainerDisplayHint.SubmissionOrder.AfterBase;

            var profilePidls = PIDLResourceFactory.Instance.GetProfileDescriptions(country: countryToTest, type: profileTypeToTest, operation: "update", language: "en-us", partnerName: partnerToTest, profileId: "123");
            var taxPidls = PIDLResourceFactory.Instance.GetTaxIdDescriptions(country: countryToTest, type: "commercial_tax_id", language: "en-us", profileType: profileTypeToTest, partnerName: partnerToTest);

            // Get the first empty pidl container as we expect it to be modified by the code under test
            var pidlLink = profilePidls[0].DisplayHints().OfType<PidlContainerDisplayHint>().First(c => c.LinkedPidlIdentity == null);

            // Act
            PIDLResourceFactory.AddLinkedPidlToResourceList(
                pidlResource: profilePidls,
                linkedPidl: taxPidls[0],
                partner: partnerToTest,
                submitOrder: submitOrderToTest,
                pidlIndex: pidlIndexToTest);

            // Assert
            PidlAssert.HasLinkedPidl(profilePidls, taxPidls[0], submitOrderToTest, pidlIndexToTest);
        }

        [TestMethod]
        [TestCategory(TestCategory.SpecialCase)]
        public void AddLinkedPidlToResourceList_MovesSubmitToLastPageForSplitPagePartners()
        {
            // Arrange
            var profileTypeToTest = "consumerprerequisites"; // because this pidl has hint ids (profilePrerequisitesPage4, profilePrerequisitesPage3 etc) that get special cased in code under test. 
            var countryToTest = "br"; // because code under test has special treatment for brazil_cpf_id
            var pidlIndexToTest = 8;  // this is just an arbitrary number for testing.  We only care that this is appended to the HintId.
            var submitOrderToTest = PidlContainerDisplayHint.SubmissionOrder.AfterBase;

            foreach (var partnerToTest in TestConstants.PartnersWithPageSplits)
            {
                var profilePidls = PIDLResourceFactory.Instance.GetProfileDescriptions(country: countryToTest, type: profileTypeToTest, operation: "add", language: "en-us", partnerName: partnerToTest);
                var taxPidls = PIDLResourceFactory.Instance.GetTaxIdDescriptions(country: countryToTest, type: "consumer_tax_id", language: "en-us", profileType: "consumer", partnerName: partnerToTest);

                // Get the first empty pidl container as we expect it to be modified by the code under test
                var pidlLink = profilePidls[0].DisplayHints().OfType<PidlContainerDisplayHint>().FirstOrDefault(c => c.LinkedPidlIdentity == null);
                if (pidlLink == null)
                {
                    Assert.Inconclusive("This test is meaningful only if the original pidl has an empty pidl container to start with.");
                }

                // Act
                PIDLResourceFactory.AddLinkedPidlToResourceList(
                    pidlResource: profilePidls,
                    linkedPidl: taxPidls[0],
                    partner: partnerToTest,
                    submitOrder: submitOrderToTest,
                    pidlIndex: pidlIndexToTest);

                // Assert
                PidlAssert.HasLinkedPidl(profilePidls, taxPidls[0], submitOrderToTest, pidlIndexToTest);

                foreach (var page in profilePidls[0].DisplayPages)
                {
                    var expectedButtons = new List<string>();
                    expectedButtons.Add(page == profilePidls[0].DisplayPages.First() ? "gohome" : "movePrevious");
                    expectedButtons.AddRange(page == profilePidls[0].DisplayPages.Last() ? new List<string>() { "submit", "navigate" } : new List<string>() { "moveNext" });

                    CollectionAssert.AreEqual(expectedButtons, page.DisplayHints().OfType<ButtonDisplayHint>().Select(button => button.Action.ActionType).ToList());
                }
            }
        }

        [TestMethod]
        [TestCategory(TestCategory.SpecialCase)]
        public void PidlResourceFactory_UpdateProfilePidlSubmitUrl()
        {
            const string Country = "us";
            const string Type = "organization";
            const string Operation = "update_partial";
            const string Language = "en-us";
            const string Partner = "commercialstores";
            const string Scenario = "withProfileAddress";
            const string CustomerId = "a1eae9d9-1df5-419e-a64c-ec79ded25dee";

            List<PIDLResource> profilePidls = PIDLResourceFactory.Instance.GetProfileDescriptions(Country, Type, Operation, Language, Partner, null, "mockid", scenario: Scenario);
            PIDLResourceFactory.UpdateProfilePidlSubmitUrl(profilePidls[0], CustomerId);

            ButtonDisplayHint submitButton = profilePidls[0].GetDisplayHintById(TestConstants.ButtonDisplayHintIds.SaveButtonHidden) as ButtonDisplayHint;
            Assert.IsTrue(submitButton != null && submitButton.Action != null && submitButton.Action.Context != null, "Profile Pidl DisplayDescription validation failed, saveButton not found");
            RestLink actionContext = submitButton.Action.Context as RestLink;
            Assert.IsNotNull(actionContext, "Profile Pidl DisplayDescription validation failed, submit button context cant be null");

            Assert.IsTrue(actionContext.Href.Contains(CustomerId + "/profile"));
            Assert.IsFalse(actionContext.Href.Contains("{userId}/profile"));
        }

        [TestMethod]
        public void PidlResourceFactory_RedirectClientActionWithoutRuRxParamsSetting()
        {
            const string RedirectUrl = "https://www.microsoft.com";
            PIDLResource pidlWithFlag = PIDLResourceFactory.GetRedirectPidl(RedirectUrl, true);
            PIDLResource pidlWoFlag = PIDLResourceFactory.GetRedirectPidl(RedirectUrl);
            RedirectionServiceLink actionContext = pidlWithFlag.ClientAction.Context as RedirectionServiceLink;
            RedirectionServiceLink actionContextWoFlag = pidlWoFlag.ClientAction.Context as RedirectionServiceLink;
            Assert.IsFalse(actionContextWoFlag.NoCallbackParams);
            Assert.IsTrue(actionContext.NoCallbackParams);
        }

        /// <summary>
        /// The test is to verify whether the postal code is optional or not for different description types.
        /// </summary>
        [TestMethod]
        public void PidlResourceFactory_ValidatePostalCodeForDescriptionTypes()
        {
            // Arrange
            string postalCode = "postal_code";
            string hapiPostalCode = "postalCode";
            string[] countriesToValidatePostalCode = new string[] { "kw", "et", "hn", "ni", "us", "tr" };

            // The partners listed below are the most significantly affected by the mandatory postal code requirement, primarily in the countries KW, HN, and HN.
            string[] partnersToValidatePostalCode = new string[]
            {
                TestConstants.PartnerNames.Webblends,
                TestConstants.PartnerNames.OfficeOobe,
                TestConstants.PartnerNames.SetupOffice,
                TestConstants.PartnerNames.AmcWeb,
                TestConstants.PartnerNames.Xbox,
                TestConstants.PartnerNames.NorthStarWeb,
                TestConstants.PartnerNames.Cart,
                TestConstants.PartnerNames.CommercialStores,
                TestConstants.PartnerNames.Azure
            };

            foreach (string partner in partnersToValidatePostalCode)
            {
                foreach (string country in countriesToValidatePostalCode)
                {
                    foreach (string type in TestConstants.AddressTypesToValidate)
                    {
                        string postalCodeType = TestConstants.AddressTypesWithHapi.Contains(type) ? hapiPostalCode : postalCode;

                        // Act
                        List<PIDLResource> addressDescriptionsPidl = PIDLResourceFactory.Instance.GetAddressDescriptions(country, type, TestConstants.DisplayLanguages.EnUs, partner);

                        // Assert
                        ValidatePostalCode(partner, addressDescriptionsPidl, postalCodeType, country);
                    }

                    // Act
                    List<PIDLResource> profileDescriptionsPidl = PIDLResourceFactory.Instance.GetProfileDescriptions(country, TestConstants.DescriptionTypes.LegalEntityDescription, TestConstants.PidlOperationTypes.Update, TestConstants.DisplayLanguages.EnUs, partner);

                    // Assert
                    ValidatePostalCode(partner, profileDescriptionsPidl, hapiPostalCode, country);
                }
            }
        }

        [TestMethod]
        public void PidlResourceFactory_PSD2CSPProxyFrameTests()
        {
            string htmlBlob = PIDLResourceFactory.ComposeHtmlCSPThreeDSFingerprintIFrameContent("https://test.com", "2323123123");
            Assert.AreEqual(string.IsNullOrEmpty(htmlBlob), false);

            htmlBlob = PIDLResourceFactory.ComposeHtmlCSPThreeDSChallengeIFrameDescription("https://test.com", "creq", "sessionData");
            Assert.AreEqual(string.IsNullOrEmpty(htmlBlob), false);
        }

        [TestMethod]
        [DataRow(false, false)]
        [DataRow(true, true)]
        public void GetProfileDescriptions_ValidateIdentity(bool overrideJarvisVersionToV3, bool isGuestAccount)
        {
            var partnerToTest = "test"; // because the focus of this test is the PidlFactory code that is common to all partners.
            var profileTypeToTest = "consumer";
            var countryToTest = "us";

            var profilePidls = PIDLResourceFactory.Instance.GetProfileDescriptions(country: countryToTest, type: profileTypeToTest, operation: "update", language: "en-us", partnerName: partnerToTest, profileId: "123", overrideJarvisVersionToV3: overrideJarvisVersionToV3, isGuestAccount: isGuestAccount);

            Assert.IsNotNull(profilePidls);

            var identity = new Dictionary<string, string>()
            {
                { Constants.DescriptionIdentityFields.DescriptionType, "profile" },
                { Constants.DescriptionIdentityFields.Type, "consumer" },
                { Constants.DescriptionIdentityFields.Country, "us" },
                { Constants.DescriptionIdentityFields.Operation, "update" }
            };

            var guestUserIdentity = new Dictionary<string, string>()
            {
                { Constants.DescriptionIdentityFields.DescriptionType, "profile" }
            };

            CollectionAssert.AreEqual(
                profilePidls.First().Identity,
                isGuestAccount ? guestUserIdentity : identity);
        }

        /// <summary>
        /// Validation and assertion of the postal code happen here. 
        /// </summary>
        /// <param name="partner"></param>
        /// <param name="descriptionsPidl"></param>
        /// <param name="postalCodeType"></param>
        /// <param name="country"></param>
        private void ValidatePostalCode(string partner, List<PIDLResource> descriptionsPidl, string postalCodeType, string country)
        {
            Assert.IsNotNull(descriptionsPidl);

            var postalCode = descriptionsPidl[0].GetPropertyDescriptionByPropertyName(postalCodeType);
            Assert.IsNotNull(postalCode);

            if (string.Equals(country, "et"))
            {
                Assert.AreEqual(postalCode.IsOptional, true);
            }
            else
            {
                Assert.AreEqual(postalCode.IsOptional, false);
            }
        }

        [TestMethod]
        public void ParseIFrameTimeout_ShouldReturnCorrectTimeout()
        {
            // Arrange
            var exposedFlightFeatures = new List<string>
            {
                "PXPSD2ThreeDSFingerprintTimeout_ABCD",
                "PXPSD2ThreeDSFingerprintTimeout_5000"
            };

            var timeoutRegex = new Regex("PXPSD2ThreeDSFingerprintTimeout_([0-9]+)", RegexOptions.IgnoreCase);

            // Act
            int timeout = PIDLResourceFactory.ParseIFrameTimeout(timeoutRegex, exposedFlightFeatures);

            // Assert
            Assert.AreEqual(5000, timeout, "The timeout value should match the first valid PXPSD2ThreeDSFingerprintTimeout_ value.");
        }

        [TestMethod]
        public void ParseIFrameTimeout_ShouldReturnDefaultTimeout_WhenNoValidFeature()
        {
            // Arrange
            var exposedFlightFeatures = new List<string>
            {
                "SomeOtherFeature",
                "InvalidTimeout_ABC"
            };

            var timeoutRegex = new Regex("PXPSD2ThreeDSFingerprintTimeout_([0-9]+)", RegexOptions.IgnoreCase);

            // Act
            int timeout = PIDLResourceFactory.ParseIFrameTimeout(timeoutRegex, exposedFlightFeatures);

            // Assert
            Assert.AreEqual(60000, timeout, "The default timeout value should be returned when no valid PXPSD2ThreeDSFingerprintTimeout_ value is found.");
        }

        [TestMethod]
        public void ParseIFrameTimeout_ShouldReturnDefaultTimeout_WhenFeatureListIsEmpty()
        {
            // Arrange
            var exposedFlightFeatures = new List<string>();

            var timeoutRegex = new Regex("PXPSD2ThreeDSFingerprintTimeout_([0-9]+)", RegexOptions.IgnoreCase);

            // Act
            int timeout = PIDLResourceFactory.ParseIFrameTimeout(timeoutRegex, exposedFlightFeatures);

            // Assert
            Assert.AreEqual(60000, timeout, "The default timeout value should be returned when the feature list is empty.");
        }

        [TestMethod]
        public void ParseIFrameTimeout_ShouldReturnDefaultTimeout_WhenFeatureListIsNull()
        {
            List<string> exposedFlightFeatures = null;

            var timeoutRegex = new Regex("PXPSD2ThreeDSFingerprintTimeout_([0-9]+)", RegexOptions.IgnoreCase);

            // Act
            int timeout = PIDLResourceFactory.ParseIFrameTimeout(timeoutRegex, exposedFlightFeatures);

            // Assert
            Assert.AreEqual(60000, timeout, "The default timeout value should be returned when the feature list is empty.");
        }

        [TestMethod]
        [DataRow("PXPSD2ThreeDSFingerprintTimeout_1000", 1000)]
        [DataRow("", 60000)]
        public void GetThreeDSFingerprintIFrameDescription_ShouldSetMessageTimeoutBasedOnExposedFlightFeature(string flights, int expectedTimeout)
        {
            // Arrange
            var exposedFlightFeatures = new List<string>
            {
                flights
            };
            string threeDSMethodURL = "https://test.com/3ds-method";
            string threeDSMethodData = "testData";
            string threeDSSessionId = "session123";
            string pxAuthURL = "https://test.com/auth";
            string cspStep = "step1";
            string testHeader = "testHeader";

            // Act
            var result = PIDLResourceFactory.GetThreeDSFingerprintIFrameDescription(
                threeDSMethodURL,
                threeDSMethodData,
                threeDSSessionId,
                pxAuthURL,
                cspStep,
                testHeader,
                null,
                exposedFlightFeatures);

            // Assert
            Assert.IsNotNull(result, "The result should not be null.");
            IFrameDisplayHint fingerprintingIFrame = result[0].GetDisplayHintOrPageById(TestConstants.StaticDisplayHintIds.ThreeDSFingerprintIFrameId) as IFrameDisplayHint;
            Assert.IsNotNull(fingerprintingIFrame, "The fingerprinting IFrame should not be null.");
            Assert.AreEqual(expectedTimeout, fingerprintingIFrame.MessageTimeout, "The MessageTimeout should be set to expectedTimeout based on the exposed flight feature.");
        }
        
        [TestMethod]
        public void GetThreeDSFingerprintUrlIFrameDescription_ReturnsIFrame()
        {
            // Arrange
            string threeDSMethodURL = "https://test.com/3ds-method";
            string threeDSMethodData = "testData";
            string threeDSSessionId = "session123";
            string pxAuthURL = "https://test.com/auth";
            string cspStep = "step1";
            string testHeader = "testHeader";

            // Act
            var result = PIDLResourceFactory.GetThreeDSFingerprintUrlIFrameDescription(
                threeDSMethodURL,
                threeDSMethodData,
                threeDSSessionId,
                pxAuthURL,
                cspStep,
                testHeader,
                null);

            // Assert
            Assert.IsNotNull(result, "The result should not be null.");
            IFrameDisplayHint fingerprintingIFrame = result[0].GetDisplayHintOrPageById(TestConstants.StaticDisplayHintIds.ThreeDSFingerprintIFrameId) as IFrameDisplayHint;
            Assert.IsNotNull(fingerprintingIFrame, "The fingerprinting IFrame should not be null.");
        }

        [TestMethod]
        [DataRow("PXPSD2ThreeDSTimeoutFingerprintTimeout_1000", 1000)]
        [DataRow("", 60000)]
        public void GetTimeoutThreeDSFingerprintIFrameDescription_ShouldSetMessageTimeoutBasedOnExposedFlightFeature(string flights, int expectedTimeout)
        {
            // Arrange
            var exposedFlightFeatures = new List<string>
            {
                flights
            };
            string threeDSMethodData = "testData";
            string threeDSSessionId = "session123";
            string pxAuthURL = "https://test.com/auth";
            var mockClientAction = new ClientAction(ClientActionType.ReturnContext);

            // Act
            var result = PIDLResourceFactory.GetTimeoutThreeDSFingerprintIFrameDescription(
                pxAuthURL,
                threeDSMethodData,
                threeDSSessionId,
                mockClientAction,
                exposedFlightFeatures);

            // Assert
            Assert.IsNotNull(result, "The result should not be null.");
            IFrameDisplayHint fingerprintingIFrame = result[0].GetDisplayHintOrPageById(TestConstants.StaticDisplayHintIds.ThreeDSTimeoutFingerprintIFrame) as IFrameDisplayHint;
            Assert.IsNotNull(fingerprintingIFrame, "The fingerprinting timeout IFrame should not be null.");
            Assert.AreEqual(expectedTimeout, fingerprintingIFrame.MessageTimeout, "The MessageTimeout should be set to expectedTimeout based on the exposed flight feature.");
        }

        [TestMethod]
        [DataRow("PXPSD2TimeoutOnPostViaSrc", "https://pmservices.cp.microsoft.com/staticresourceservice/scripts/v6/Prod/psd2_csp_onload_submit.js", "body onload=")]
        [DataRow(null, "body onload=", "https://pmservices.cp.microsoft.com/staticresourceservice/scripts/v6/Prod/psd2_csp_onload_submit.js")]
        public void GetTimeoutThreeDSFingerprintIFrameDescription_DisplayContents(string flights, string expectedContents, string contentsNotAllowed)
        {
            // Arrange
            var exposedFlightFeatures = new List<string>
            {
                flights
            };
            string threeDSMethodData = "testData";
            string threeDSSessionId = "session123";
            string pxAuthURL = "https://test.com/auth";
            var mockClientAction = new ClientAction(ClientActionType.ReturnContext);

            // Act
            var result = PIDLResourceFactory.GetTimeoutThreeDSFingerprintIFrameDescription(
                pxAuthURL,
                threeDSMethodData,
                threeDSSessionId,
                mockClientAction,
                exposedFlightFeatures);

            // Assert
            Assert.IsNotNull(result, "The result should not be null.");
            IFrameDisplayHint fingerprintingIFrame = result[0].GetDisplayHintOrPageById(TestConstants.StaticDisplayHintIds.ThreeDSTimeoutFingerprintIFrame) as IFrameDisplayHint;
            Assert.IsNotNull(fingerprintingIFrame, "The fingerprinting timeout IFrame should not be null.");
            Assert.IsTrue(fingerprintingIFrame.DisplayContent.Contains(expectedContents), "Iframe should contain expected contents.");
            Assert.IsTrue(!fingerprintingIFrame.DisplayContent.Contains(contentsNotAllowed), "Iframe should not contain disallowed contents.");
        }
    }
}