// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace CIT.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using Helpers;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Test.Common;

    [TestClass]
    [TestCategory(TestCategory.SpecialCase)]
    public class GetPaymentInsturmentSelectDescriptionsTests
    {
        [TestMethod]
        public void GetPaymentInsturmentSelectDescriptions_ClientSidePrefillingIsAsExpected()
        {
            const string Country = "us";
            const string Language = "en-US";
            List<string> partners = new List<string> { "azure", "commercialstores", "ggpdeds", "marketplace", "onedrive", "payin", "setupoffice", "listpidropdown" };
            PaymentExperienceSetting setting = null;

            foreach (string partner in partners)
            {
                if (string.Equals(partner, TestConstants.TemplateNames.Listpidropdown, StringComparison.OrdinalIgnoreCase))
                {
                    string settingJsonString = "{\"selectinstance\":{\"template\":\"listpidropdown\"}}";
                    setting = JsonConvert.DeserializeObject<PaymentExperienceSetting>(settingJsonString);
                }

                List<PIDLResource> listPIPidl = PIDLResourceFactory.GetPaymentInsturmentSelectDescriptions(Country, Language, partner, null, setting: setting);
                PidlAssert.IsValid(listPIPidl, clientSidePrefill: true);
            }
        }

        [DataRow("azure",            PIActionType.SelectResourceType, true,  null)]
        [DataRow("appsource",        PIActionType.SelectResourceType, true,  null)]
        [DataRow("ggpdeds",          PIActionType.AddResource,        true,  null)]
        [DataRow("payin",            PIActionType.SelectResourceType, true,  null)]
        [DataRow("setupoffice",      PIActionType.SelectResourceType, true,  null)]
        [DataRow("storeoffice",      PIActionType.SelectResourceType, true,  null)]
        [DataRow("commercialstores", PIActionType.SelectResourceType, false, "departmentalPurchase")]
        [DataRow("listpidropdown", PIActionType.SelectResourceType, true, null)]
        [DataTestMethod]
        public void GetPaymentInsturmentSelectDescriptions_ListPILinkAndAddPILinkIsAsExpected(string partner, PIActionType newPaymentMethodLinkAction, bool hasClassicProductAndBillableAccountId, string scenario)
        {
            const string Country = "us";
            const string Language = "en-US";
            string classicProduct = hasClassicProductAndBillableAccountId ? "azureClassic" : null;
            string billableAccountId = hasClassicProductAndBillableAccountId ? "dummyBillableAccountId" : null;
            PaymentExperienceSetting setting = null;

            if (string.Equals(partner, TestConstants.TemplateNames.Listpidropdown, StringComparison.OrdinalIgnoreCase))
            {
                string settingJsonString = "{\"template\":\"listpidropdown\",\"features\":null}";
                setting = JsonConvert.DeserializeObject<PaymentExperienceSetting>(settingJsonString);
            }

            List<PIDLResource> listPIPidls = PIDLResourceFactory.GetPaymentInsturmentSelectDescriptions(Country, Language, partner, scenario, classicProduct, billableAccountId, setting: setting);

            Assert.AreEqual(1, listPIPidls.Count, "Only one list PI PIDL is expected");

            PIDLResource listPIPidl = listPIPidls[0];

            // Verify List PI link in dataSources
            Assert.IsTrue(listPIPidl.DataSources["paymentInstruments"].Href.Contains($"&partner={partner}"), "Partner query param should be present");

            if (hasClassicProductAndBillableAccountId)
            {
                Assert.IsTrue(listPIPidl.DataSources["paymentInstruments"].Href.Contains($"&classicProduct={classicProduct}"), "classicProduct query param should be present");
                Assert.IsTrue(listPIPidl.DataSources["paymentInstruments"].Href.Contains($"&billableAccountId={billableAccountId}"), "billableAccountId query param should be present");
            }

            // Verify Add PI link
            if (!string.Equals(partner, TestConstants.TemplateNames.Listpidropdown, StringComparison.OrdinalIgnoreCase))
            {
                var newPaymentMethodLink = listPIPidl.GetDisplayHintById(TestConstants.DisplayHintIds.NewPaymentMethodLink) as ButtonDisplayHint;
                Assert.AreEqual(newPaymentMethodLink.Action.ActionType, DisplayHintActionType.partnerAction.ToString(), "newPaymentMethodLink should have partnerAction as action type");

                var actionContext = newPaymentMethodLink.Action.Context as ActionContext;
                Assert.AreEqual(PaymentInstrumentActions.ToString(newPaymentMethodLinkAction), actionContext.Action);
                Assert.AreEqual(TestConstants.DescriptionTypes.PaymentInstrumentDescription, actionContext.ResourceActionContext.PidlDocInfo.ResourceType);
            }
        }

        [DataRow("payin", "in", "en-us", true)]
        [DataRow("setupoffice", "in", "en-us", true)]
        [DataRow("payin", "us", "en-us", true)]
        [DataRow("setupoffice", "us", "en-us", true)]
        [DataRow("payin", "in", "en-us", false)]
        [DataRow("setupoffice", "in", "en-us", false)]
        [DataTestMethod]
        public void GetPaymentInsturmentSelectDescriptions_IndiaTokenizationPurgeMessage(string partner, string country, string language, bool flightPassed)
        {
            List<string> exposedFlightFeatures = new List<string>();
            if (flightPassed && string.Equals(country, "in"))
            {
                exposedFlightFeatures.Add("IndiaTokenizationMessage");
            }

            List<PIDLResource> pidls = PIDLResourceFactory.GetPaymentInsturmentSelectDescriptions(country, language, partner, null, null, null, exposedFlightFeatures);
            Assert.IsNotNull(pidls);
            var root = pidls[0]?.DisplayPages[0];
            var displayType = string.Empty;

            if (string.Equals(country, "in") && flightPassed)
            {
                var page = root?.Members[0] as TextGroupDisplayHint;
                var group = page?.Members[0] as TextDisplayHint;
                displayType = group.HintId;
                Assert.AreEqual("paymentInstrumentSelectMessage", displayType, "India Tokenization Purge Message not displayed");
            }
            else
            {
                var page = root?.Members[0] as PropertyDisplayHint;
                displayType = page.HintId;
                Assert.AreEqual("paymentInstrumentShowPi", displayType, "India Tokenization Purge Message was displayed-not expected");
            }
        }
    }
}
