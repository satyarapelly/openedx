// <copyright company="Microsoft">Copyright (c) Microsoft 2019. All rights reserved.</copyright>

namespace CIT.PidlFactory.V7
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;
    using Newtonsoft.Json;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;

    [TestClass]
    public class GetBillingGroupsDescriptionsTests
    {
        readonly Dictionary<string, string[]> namedPartnerLists = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "Partners-BG",
                new string[] { "commercialstores", "defaulttemplate" }
            },
            {
                "Partners-NotApplicableForBG",
                new string[] { "amc", "amcweb", "amcxbox", "appsource", "azure", "azuresignup", "azureibiza", "bing", "bingtravel", "cart", "commercialsupport", "commercialwebblends", "consumersupport", "default", "ggpdeds", "marketplace", "mseg", "northstarweb", "office", "officeoobe", "oxooobe", "onedrive", "officeoobeinapp", "oxodime", "oxowebdirect", "payin", "setupoffice", "setupofficesdx", "smboobe", "storeoffice", "storify", "xboxsubs", "xboxsettings", "saturn", "test", "wallet", "webblends", "webblends_inline", "webpay",  "xbox", "xboxweb", "windowssubs", "windowssettings", "windowsstore", "msteams", "onepage", "twopage", "selectpmbuttonlist", "selectpmradiobuttonlist", "selectpmdropdown", "listpidropdown", "listpiradiobutton", "listpibuttonlist" }
            },
            {
                "Partners-Skip",
                new string[] { "xboxnative", "windowsnative", "consoletemplate", "secondscreentemplate" }
            }
        };

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void GetBillingGroupsDescriptionsTests_EnsureCITsCoverAllPartners()
        {
            // Arrange
            var allProfilePartners = namedPartnerLists["Partners-BG"]
                .Concat(namedPartnerLists["Partners-NotApplicableForBG"])
                .Concat(namedPartnerLists["Partners-Skip"]);

            // Assert
            CollectionAssert.AreEquivalent(TestConstants.AllPartners.ToList(), allProfilePartners.Distinct().ToList(), "CIT to verify billing group is expected to cover all partners");
        }

        [DataRow("Partners-BG", true)]
        [DataRow("Partners-NotApplicableForBG", false)]
        [DataTestMethod]
        public void GetBillingGroupsDescriptionsTests_SelectInstanceBillingGroup(string partnerListName, bool hasDisplayDescription)
        {
            // Arrange
            const string Country = "us";
            const string Language = "en-us";
            const string Operation = TestConstants.PidlOperationTypes.SelectInstance;
            string[] bgTypes = new string[] { null, string.Empty, "lightweight", "lightweightv7" };

            foreach (var partner in namedPartnerLists[partnerListName])
            {
                foreach (var type in bgTypes)
                {
                    PaymentExperienceSetting setting = null;
                    this.TestContext.WriteLine("Start testing get billing group client prefill: Country \"{0}\", Partner \"{1}\", Operation \"{2}\", Type \"{3}\"", Country, partner, Operation, type);
                    
                    if (string.Equals(partner, TestConstants.TemplateNames.DefaultTemplate, StringComparison.OrdinalIgnoreCase))
                    {
                        string settingJsonString = "{\"template\":\"defaulttemplate\",\"features\":{\"addPartnerActionToBillingGroupAddAndUpdate\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null}}}";
                        setting = JsonConvert.DeserializeObject<PaymentExperienceSetting>(settingJsonString);
                    }

                    // Act
                    List<PIDLResource> billingGroupPidls = PIDLResourceFactory.GetBillingGroupListDescriptions(type, Operation, Country, Language, partner, setting: setting);

                    // Assert
                    if (hasDisplayDescription)
                    {                        
                        // get billing group list pidl uses client side prefill, dataSource is expected
                        PidlAssert.IsValid(billingGroupPidls, 1, clientSidePrefill: true);

                        // Validate action in pidl
                        ValidatePidlActionForBillingGroup(billingGroupPidls[0], TestConstants.DisplayHintIds.BillingGroupListAddBGHyperlinkId, "addResource");
                    }
                    else
                    {
                        // For not supported partners, display description should be null
                        PidlAssert.IsValid(billingGroupPidls, 1, displayDescription: false, clientSidePrefill: true);
                    }
                }
            }
        }

        [DataRow("Partners-BG", true)]
        [DataRow("Partners-NotApplicableForBG", false)]
        [DataTestMethod]
        public void GetBillingGroupsDescriptionsTests_AddBillingGroup(string partnerListName, bool hasDisplayDescription)
        {
            // Arrange
            const string Country = "us";
            const string Language = "en-us";
            const string Operation = TestConstants.PidlOperationTypes.Add;
            string[] bgTypes = new string[] { "lightweight", "lightweightv7" };

            foreach (var partner in namedPartnerLists[partnerListName])
            {
                foreach (var type in bgTypes)
                {
                    PaymentExperienceSetting setting = null;
                    this.TestContext.WriteLine("Start testing get billing group client prefill: Country \"{0}\", Partner \"{1}\", Operation \"{2}\", Type \"{3}\"", Country, partner, Operation, type);
                    
                    if (string.Equals(partner, TestConstants.TemplateNames.DefaultTemplate, StringComparison.OrdinalIgnoreCase))
                    {
                        string settingJsonString = "{\"template\":\"defaulttemplate\",\"features\":{\"addSelectResourcePartnerActionToBillingGroupAddPi\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null}}}";
                        setting = JsonConvert.DeserializeObject<PaymentExperienceSetting>(settingJsonString);
                    }

                    // Act
                    List<PIDLResource> billingGroupPidls = PIDLResourceFactory.Instance.GetBillingGroupDescriptions(type, Operation, Country, Language, partner, setting: setting);

                    // Assert
                    if (hasDisplayDescription)
                    {
                        // add billing group pidl uses client side prefill, dataSource is expected
                        PidlAssert.IsValid(billingGroupPidls, 1, clientSidePrefill: true);

                        // Validate action in pidl
                        ValidatePidlActionForBillingGroup(billingGroupPidls[0], TestConstants.DisplayHintIds.BillingGroupLightWeightAddNewPaymentInstrument, "selectResourceType");

                        // Validate submit button in pidl
                        ValidateSubmitButtonForBillingGroup(billingGroupPidls[0], TestConstants.ButtonDisplayHintIds.SaveButton, TestConstants.HTTPVerbs.Post, this.GetExpectedBillingGroupAddEndpoint(type));
                    }
                    else
                    {
                        // For not supported partners, display description should be null
                        PidlAssert.IsValid(billingGroupPidls, 1, displayDescription: false, clientSidePrefill: true);
                    }
                }
            }
        }

        [DataRow("Partners-BG", true)]
        [DataRow("Partners-NotApplicableForBG", false)]
        [DataTestMethod]
        public void GetBillingGroupsDescriptionsTests_UpdatePONumberBillingGroup(string partnerListName, bool hasDisplayDescription)
        {
            const string Language = "en-us";
            const string Country = "us";
            const string Scenario = "billingGroupPONumber";
            const string Operation = TestConstants.PidlOperationTypes.Update;
            string[] bgTypes = new string[] { "lightweight", "lightweightv7" };

            foreach (var partner in namedPartnerLists[partnerListName])
            {
                foreach (var type in bgTypes)
                {
                    this.TestContext.WriteLine("Start testing get billing group client prefill: Country \"{0}\", Partner \"{1}\", Operation \"{2}\", Type \"{3}\"", Country, partner, Operation, type);

                    List<PIDLResource> billingGroupPidls = PIDLResourceFactory.Instance.GetBillingGroupDescriptions(type, Operation, Country, Language, partner, Scenario);

                    if (hasDisplayDescription)
                    {
                        // update billing group pidl uses no prefill
                        PidlAssert.IsValid(billingGroupPidls);

                        // Validate submit button in pidl
                        ValidateSubmitButtonForBillingGroup(billingGroupPidls[0], TestConstants.ButtonDisplayHintIds.SaveButton, TestConstants.HTTPVerbs.Patch, this.GetExpectedBillingGroupUpdateEndpoint(type));
                    }
                    else
                    {
                        // For not supported partners, display description should be null
                        PidlAssert.IsValid(billingGroupPidls, 1, displayDescription: false);
                    }
                }
            }
        }

        [DataTestMethod]
        public void GetBillingGroupsDescriptionsTests_HasEditBillingDetailsLink()
        {
            // Arrange
            const string Country = "us";
            const string Language = "en-us";
            string[] bgTypes = new string[] { "lightweight", "lightweightv7" };
            const string Operation = TestConstants.PidlOperationTypes.SelectInstance;

            foreach (string partner in namedPartnerLists["Partners-BG"])
            {
                foreach (var type in bgTypes)
                {
                    PaymentExperienceSetting setting = null;

                    if (string.Equals(partner, TestConstants.TemplateNames.DefaultTemplate, StringComparison.OrdinalIgnoreCase))
                    {
                        string settingJsonString = "{\"template\":\"defaulttemplate\",\"features\":{\"addPartnerActionToBillingGroupAddAndUpdate\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null}}}";
                        setting = JsonConvert.DeserializeObject<PaymentExperienceSetting>(settingJsonString);
                    }

                    // Act
                    List<PIDLResource> pidls = PIDLResourceFactory.GetBillingGroupListDescriptions(type, Operation, Country, Language, partner, setting: setting);

                    // Assert
                    PidlAssert.IsValid(pidls, 1);

                    ValidatePidlActionForBillingGroup(pidls[0], TestConstants.DisplayHintIds.BillingGroupListEditBillingDetailsBGHyperlinkId, "updateResource");
                }
            }
        }

        private void ValidatePidlActionForBillingGroup(PIDLResource pidl, string hyperlinkId, string action)
        {
            string actionType = "partnerAction";
            HyperlinkDisplayHint hyperLink = pidl.GetDisplayHintById(hyperlinkId) as HyperlinkDisplayHint;
            Assert.IsTrue(hyperLink != null && hyperLink.Action != null && hyperLink.Action.Context != null, "BillingGroup Pidl DisplayDescription validation failed, " + hyperlinkId + " not found");

            DisplayHintAction displayHintAction = hyperLink.Action as DisplayHintAction;
            Assert.IsNotNull(displayHintAction, "BillingGroup Pidl DisplayDescription validation failed, " + hyperlinkId + " action cant be null");
            Assert.IsTrue(string.Equals(displayHintAction.ActionType, actionType, StringComparison.OrdinalIgnoreCase), "Action type is incorrect");

            ActionContext actionContext = displayHintAction.Context as ActionContext;
            Assert.IsNotNull(actionContext, "BillingGroup Pidl DisplayDescription validation failed, " + hyperlinkId + " action context cant be null");
            Assert.IsTrue(string.Equals(actionContext.Action, action, StringComparison.OrdinalIgnoreCase), "ActionContext action is incorrect");
        }

        private void ValidateSubmitButtonForBillingGroup(PIDLResource pidl, string buttonDisplayId, string operation, string expectedEndpointUrl)
        {
            // Get submit button from pidl
            ButtonDisplayHint submitButton = pidl.GetDisplayHintById(buttonDisplayId) as ButtonDisplayHint;

            Assert.IsTrue(submitButton != null && submitButton.Action != null && submitButton.Action.Context != null, "BillingGroup Pidl DisplayDescription validation failed, saveButton not found");

            RestLink actionContext = submitButton.Action.Context as RestLink;
            Assert.IsNotNull(actionContext, "BillingGroup Pidl DisplayDescription validation failed, submit button context cant be null");
            Assert.IsTrue(string.Equals(actionContext.Method, operation, StringComparison.OrdinalIgnoreCase), "Context method is incorrect");
            Assert.IsTrue(string.Equals(actionContext.Href, expectedEndpointUrl, StringComparison.OrdinalIgnoreCase), "Endpoint url is incorrect");
        }

        private string GetExpectedBillingGroupUpdateEndpoint(string type)
        {
            switch (type)
            {
                case "lightweightv7":
                    return TestConstants.ServiceEndpoints.BillingGroupV7UpdatePONumberEndpoint;
                default:
                    return TestConstants.ServiceEndpoints.BillingGroupUpdatePONumberEndpoint;
            }
        }

        private string GetExpectedBillingGroupAddEndpoint(string type)
        {
            switch (type)
            {
                case "lightweightv7":
                    return TestConstants.ServiceEndpoints.BillingGroupV7AddEndpoint;
                default:
                    return TestConstants.ServiceEndpoints.BillingGroupAddEndpoint;
            }
        }
    }
}
