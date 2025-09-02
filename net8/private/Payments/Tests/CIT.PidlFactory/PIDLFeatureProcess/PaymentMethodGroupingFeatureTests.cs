// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace CIT.PidlFactory.V7
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Test.Common;
    using Microsoft.Commerce.Payments.PidlFactory.V7.PIDLFeatureProcess;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;

    [TestClass]
    [TestCategory(TestCategory.SpecialCase)]
    public class PaymentMethodGroupingFeatureTests
    {
        [DataTestMethod]
        public void PaymentMethodGroupingFeature_Actions()
        {
            string partner = "cart";
            string country = "us";
            string language = "en-us";
            string operation = "select";

            var paymentMethods = new HashSet<PaymentMethod>();
            paymentMethods.Add(new PaymentMethod() { PaymentMethodFamily = "credit_card", PaymentMethodType = "visa", Properties = new PaymentMethodCapabilities(), GroupDisplayName = "Credit or Debit Card", PaymentMethodGroup = "credit_debit" });
            paymentMethods.Add(new PaymentMethod() { PaymentMethodFamily = "credit_card", PaymentMethodType = "amex", Properties = new PaymentMethodCapabilities(), GroupDisplayName = "Credit or Debit Card", PaymentMethodGroup = "credit_debit" });
            paymentMethods.Add(new PaymentMethod() { PaymentMethodFamily = "online_bank_transfer", PaymentMethodType = "ideal", Properties = new PaymentMethodCapabilities(), GroupDisplayName = "Online Bank Transfer", PaymentMethodGroup = "online_bank_transfer" });
            paymentMethods.Add(new PaymentMethod() { PaymentMethodFamily = "online_bank_transfer", PaymentMethodType = "paysafecard", Properties = new PaymentMethodCapabilities(), GroupDisplayName = "Online Bank Transfer", PaymentMethodGroup = "online_bank_transfer" });

            List<string> exposedFlightFeatures = new List<string>()
            {
                "enablePaymentMethodGrouping"
            };

            FeatureContext actionParams = new FeatureContext(country, partner, "test", "select", null, language, paymentMethods, exposedFlightFeatures);

            List<PIDLResource> pidls = PIDLResourceFactory.GetPaymentSelectDescriptions(paymentMethods, country, operation, language, partner, null, null, null, null, null, exposedFlightFeatures, null);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");

            // test for AddClassName action
            var displayHint = pidls[0].GetDisplayHintById("paymentMethod") as PropertyDisplayHint;
            Assert.IsFalse(displayHint.DisplayTags.ContainsKey("paymentMethod_pmGrouping"));
            PaymentMethodGrouping.AddClassName(pidls, actionParams);
            Assert.IsTrue(string.Equals(displayHint.DisplayTags["paymentMethod_pmGrouping"], "paymentMethod_pmGrouping"));

            // test for ChangeDisplayType action
            Assert.IsTrue(string.Equals(displayHint.SelectType, "dropDown"));
            PaymentMethodGrouping.ChangeDisplayType(pidls, actionParams);
            Assert.IsTrue(string.Equals(displayHint.SelectType, "buttonList"));

            // test for PaymentMethodGrouping action
            PaymentMethodGrouping.GroupPaymentMethod(pidls, actionParams);
            Assert.AreEqual(pidls[0].DisplayPages.Count, 2);
            Assert.AreEqual(pidls[0].DisplayPages[0].DisplayName, "paymentMethodSelectPMGroupingPage");
            PropertyDisplayHint paymentMethod = pidls[0].DisplayPages[0].Members[1] as PropertyDisplayHint;
            Assert.AreEqual(paymentMethod.SelectType, "buttonList");
            SelectOptionDescription groupSelectOptionDescription = paymentMethod.PossibleOptions.ElementAt(1).Value;
            Assert.AreEqual(groupSelectOptionDescription.PidlAction.ActionType, "moveToPageIndex");
            SelectOptionDescription creditCardselectOptionDescription = paymentMethod.PossibleOptions.ElementAt(0).Value;
            Assert.AreEqual(creditCardselectOptionDescription.DisplayText, "Credit or debit card");

            // test for RemoveUnneededDataDescription
            PaymentMethodGrouping.RemoveUnneededDataDescription(pidls, actionParams);
            PropertyDescription propertyDescription = pidls[0].DataDescription["id"] as PropertyDescription;
            Assert.IsNull(propertyDescription.IndexedOn);
            Assert.IsNull(propertyDescription.PossibleValues);
            Assert.IsNull(propertyDescription.DefaultValue);
        }

        [DataRow("storify", "us", "en-us", "select", false)]
        [DataRow("storify", "cn", "en-us", "select", false)]
        [DataRow("xboxsettings", "us", "en-us", "select", false)]
        [DataRow("xboxsettings", "cn", "en-us", "select", false)]
        [DataRow("windowssettings", "us", "en-us", "select", true)]
        [DataRow("windowssettings", "cn", "en-us", "select", true)]
        [DataTestMethod]
        public void NativePaymentMethodGroupingFeature_Actions(string partner, string country, string language, string operation, bool hasTextBeforeLogo)
        {
            var paymentMethods = new HashSet<PaymentMethod>();
            paymentMethods.Add(new PaymentMethod() { PaymentMethodFamily = "credit_card", PaymentMethodType = "visa", Properties = new PaymentMethodCapabilities(), GroupDisplayName = "Credit or debit card", PaymentMethodGroup = "credit_debit" });
            if (country == "us")
            {
                paymentMethods.Add(new PaymentMethod() { PaymentMethodFamily = "credit_card", PaymentMethodType = "amex", Properties = new PaymentMethodCapabilities(), GroupDisplayName = "Credit or debit card", PaymentMethodGroup = "credit_debit" });
                paymentMethods.Add(new PaymentMethod() { PaymentMethodFamily = "ewallet", PaymentMethodType = "paypal", Properties = new PaymentMethodCapabilities(), GroupDisplayName = "Ewallet", PaymentMethodGroup = "ewallet" });
                paymentMethods.Add(new PaymentMethod() { PaymentMethodFamily = "online_bank_transfer", PaymentMethodType = "paysafecard", Properties = new PaymentMethodCapabilities(), GroupDisplayName = "Online Bank Transfer", PaymentMethodGroup = "ewallet" });
                paymentMethods.Add(new PaymentMethod() { PaymentMethodFamily = "mobile_billing_non_sim", PaymentMethodType = "vzw-us-nonsim", Properties = new PaymentMethodCapabilities(), GroupDisplayName = "Mobile phone", PaymentMethodGroup = "mobile_phone" });
            }

            if (country == "cn")
            {
                paymentMethods.Add(new PaymentMethod() { PaymentMethodFamily = "credit_card", PaymentMethodType = "mc", Properties = new PaymentMethodCapabilities(), GroupDisplayName = "Credit or debit card", PaymentMethodGroup = "credit_debit" });
                paymentMethods.Add(new PaymentMethod() { PaymentMethodFamily = "credit_card", PaymentMethodType = "unionpay_creditcard", Properties = new PaymentMethodCapabilities(), GroupDisplayName = "UnionPay credit or debit card", PaymentMethodGroup = "credit_debit" });
                paymentMethods.Add(new PaymentMethod() { PaymentMethodFamily = "credit_card", PaymentMethodType = "unionpay_debitcard", Properties = new PaymentMethodCapabilities(), GroupDisplayName = "UnionPay credit or debit card", PaymentMethodGroup = "credit_debit" });
            }

            List<string> exposedFlightFeatures = new List<string>()
            {
                "enablePaymentMethodGrouping"
            };

            var featureConfigs = new Dictionary<string, FeatureConfig>();
            var details = new DisplayCustomizationDetail();
            details.SetGroupedSelectOptionTextBeforeLogo = hasTextBeforeLogo;
            var config = new FeatureConfig();
            config.DisplayCustomizationDetail = new List<DisplayCustomizationDetail>();
            config.DisplayCustomizationDetail.Add(details);
            featureConfigs.Add("paymentMethodGrouping", config);
            FeatureContext actionParams = new FeatureContext(country, partner, "test", "select", null, language, paymentMethods, exposedFlightFeatures, featureConfigs: featureConfigs);

            List<PIDLResource> pidls = PIDLResourceFactory.GetPaymentSelectDescriptions(paymentMethods, country, operation, language, partner, null, null, null, null, null, exposedFlightFeatures, null);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");

            // test for PaymentMethodGrouping action
            PaymentMethodGrouping.GroupPaymentMethod(pidls, actionParams);
            Assert.AreEqual(pidls[0].DisplayPages.Count, 2);
            Assert.AreEqual(pidls[0].DisplayPages[0].DisplayName, "PaymentMethodSelectionPage");
            PropertyDisplayHint paymentMethod = pidls[0].GetDisplayHintById("paymentMethod") as PropertyDisplayHint;
            Assert.AreEqual(paymentMethod.SelectType, "buttonList");

            if (country == "us")
            {
                Assert.AreEqual(paymentMethod.PossibleOptions.Count, 3);
                SelectOptionDescription nsmselectOptionDescription = paymentMethod.PossibleOptions.ElementAt(2).Value;
                Assert.AreEqual(nsmselectOptionDescription.PidlAction.ActionType, "success");
                ActionContext nsmOptionContext = nsmselectOptionDescription.PidlAction.Context as ActionContext;
                Assert.AreEqual(nsmselectOptionDescription.DisplayText, "Mobile phone");
                Assert.AreEqual(nsmOptionContext.Action, "addResource");
            }
            else if (country == "cn")
            {
                Assert.AreEqual(paymentMethod.PossibleOptions.Count, 2);
            }

            SelectOptionDescription groupSelectOptionDescription = paymentMethod.PossibleOptions.ElementAt(1).Value;
            Assert.AreEqual(groupSelectOptionDescription.PidlAction.ActionType, "moveToPageIndex");
            SelectOptionDescription creditCardSelectOptionDescription = paymentMethod.PossibleOptions.ElementAt(0).Value;
            Assert.AreEqual(creditCardSelectOptionDescription.DisplayText, "Credit or debit card");
            Assert.AreEqual(creditCardSelectOptionDescription.PidlAction.ActionType, "success");
            ActionContext creditCardOptionContext = creditCardSelectOptionDescription.PidlAction.Context as ActionContext;
            Assert.AreEqual(creditCardOptionContext.Action, "addResource");
            PropertyDescription propertyDescription = pidls[0].DataDescription["displayId"] as PropertyDescription;
            Assert.IsTrue((bool)propertyDescription.IsOptional);

            // Text before logo
            var expectedTextGroup = creditCardSelectOptionDescription.DisplayContent.Members[0] as GroupDisplayHint;
            Assert.AreEqual(string.Equals(expectedTextGroup.HintId, "paymentOptionTextGroup"), hasTextBeforeLogo, "Layout issue for paymet method option");
        }

        [DataRow("xboxsettings", "us", "en-us", "offline_bank_transfer")]
        [DataRow("storify", "us", "en-us", "offline_bank_transfer.")]
        [DataRow("xboxsettings", "us", "en-us", "ewallet")]
        [DataRow("storify", "us", "en-us", "ewallet")]
        [DataRow("xboxsettings", "us", "en-us", "ewallet.stored_value")]
        [DataRow("storify", "us", "en-us", "ewallet.paypal")]
        [DataRow("storify", "us", "en-us", "ewallet.")]
        [DataRow("storify", "us", "en-us", ".")]
        [DataRow("storify", "us", "en-us", ".type")]
        [DataRow("storify", "us", "en-us", "ewallet.incorrect.format")]
        [DataRow("xboxsettings", "us", "en-us", "upi")]
        [DataRow("storify", "us", "en-us", "upi.")]
        [DataTestMethod]
        public void PaymentMethodGroupingFeature_VerifyPXSwapSelectPMPagesFlight(string partner, string country, string language, string pmGroupId)
        {
            string operation = "select";

            var paymentMethods = new HashSet<PaymentMethod>
            {
                new PaymentMethod() { PaymentMethodFamily = "credit_card", PaymentMethodType = "visa", Properties = new PaymentMethodCapabilities(), GroupDisplayName = "Credit or Debit Card", PaymentMethodGroup = "credit_debit" },
                new PaymentMethod() { PaymentMethodFamily = "credit_card", PaymentMethodType = "amex", Properties = new PaymentMethodCapabilities(), GroupDisplayName = "Credit or Debit Card", PaymentMethodGroup = "credit_debit" },
                new PaymentMethod() { PaymentMethodFamily = "credit_card", PaymentMethodType = "mc", Properties = new PaymentMethodCapabilities(), GroupDisplayName = "Credit or Debit Card", PaymentMethodGroup = "credit_debit" },
                new PaymentMethod() { PaymentMethodFamily = "ewallet", PaymentMethodType = "paypal", Properties = new PaymentMethodCapabilities(), GroupDisplayName = "Ewallet", PaymentMethodGroup = "ewallet" },
                new PaymentMethod() { PaymentMethodFamily = "ewallet", PaymentMethodType = "venmo", Properties = new PaymentMethodCapabilities(), GroupDisplayName = "Ewallet", PaymentMethodGroup = "ewallet" },
                new PaymentMethod() { PaymentMethodFamily = "ewallet", PaymentMethodType = "stored_value", Properties = new PaymentMethodCapabilities(), GroupDisplayName = "Ewallet", PaymentMethodGroup = "ewallet" },
                new PaymentMethod() { PaymentMethodFamily = "offline_bank_transfer", PaymentMethodType = "bank_account", Properties = new PaymentMethodCapabilities(), GroupDisplayName = "Invoice", PaymentMethodGroup = "invoice" },
                new PaymentMethod() { PaymentMethodFamily = "offline_bank_transfer", PaymentMethodType = "check", Properties = new PaymentMethodCapabilities(), GroupDisplayName = "Invoice", PaymentMethodGroup = "invoice" },
                new PaymentMethod() { PaymentMethodFamily = "real_time_payments", PaymentMethodType = "upi_qr", Properties = new PaymentMethodCapabilities(), GroupDisplayName = "UPI", PaymentMethodGroup = "upi" },
                new PaymentMethod() { PaymentMethodFamily = "real_time_payments", PaymentMethodType = "upi", Properties = new PaymentMethodCapabilities(), GroupDisplayName = "UPI", PaymentMethodGroup = "upi" },
            };

            List<string> exposedFlightFeatures = new List<string>()
            {
                "enablePaymentMethodGrouping",
                "PXSwapSelectPMPages"
            };

            var featureConfigs = new Dictionary<string, FeatureConfig>();
            var config = new FeatureConfig();
            var details = new DisplayCustomizationDetail();
            details.SetGroupedSelectOptionTextBeforeLogo = false;
            config.DisplayCustomizationDetail = new List<DisplayCustomizationDetail> { details };
            featureConfigs.Add("paymentMethodGrouping", config);
            bool shouldShiftPage = pmGroupId[0] != '.' && !pmGroupId.Contains("stored_value") && pmGroupId.Count(c => c == '.') < 2;
            FeatureContext actionParams = new FeatureContext(country, partner, "test", "select", null, language, paymentMethods, exposedFlightFeatures, featureConfigs: featureConfigs, pmGroupPageId: pmGroupId);
            List<PIDLResource> pidls = PIDLResourceFactory.GetPaymentSelectDescriptions(paymentMethods, country, operation, language, partner, null, null, null, null, null, exposedFlightFeatures, null);

            // Assert
            Assert.IsNotNull(pidls, "Pidl is expected to be not null");

            // test for PaymentMethodGrouping action
            PaymentMethodGrouping.GroupPaymentMethod(pidls, actionParams);
            Assert.AreEqual(pidls[0].DisplayPages.Count, 4);
            Assert.AreEqual(pidls[0].DisplayPages[0].HintId, "paymentMethodSelectPage");
            Assert.AreEqual(pidls[0].DisplayPages[1].HintId, "paymentMethodSubGroupPage_ewallet_ewallet");
            Assert.AreEqual(pidls[0].DisplayPages[2].HintId, "paymentMethodSubGroupPage_invoice_offline_bank_transfer");
            Assert.AreEqual(pidls[0].DisplayPages[3].HintId, "paymentMethodSubGroupPage_upi_real_time_payments");

            if (PaymentMethodGrouping.CanSwapSelectPMPage(actionParams))
            {
                PaymentMethodGrouping.ShiftPMPageToFront(pidls, actionParams);
            }

            int actualHomePageIndex = shouldShiftPage ? 1 : 0;
            PageDisplayHint homePage = pidls[0].DisplayPages[actualHomePageIndex];
            GroupDisplayHint pageWrapperGroup = homePage.Members[0] as GroupDisplayHint;
            GroupDisplayHint columnGroup = pageWrapperGroup.Members[0] as GroupDisplayHint;
            GroupDisplayHint paymentOptionsGroup = columnGroup.Members.Last() as GroupDisplayHint;
            PropertyDisplayHint paymentMethodsMap = paymentOptionsGroup.Members[0] as PropertyDisplayHint;

            Microsoft.Commerce.Payments.PXCommon.MoveToPageIndexActionContext ewalletPageIndex = (Microsoft.Commerce.Payments.PXCommon.MoveToPageIndexActionContext)paymentMethodsMap.PossibleOptions["ewallet"].PidlAction.Context;
            Microsoft.Commerce.Payments.PXCommon.MoveToPageIndexActionContext invoicePageIndex = (Microsoft.Commerce.Payments.PXCommon.MoveToPageIndexActionContext)paymentMethodsMap.PossibleOptions["invoice"].PidlAction.Context;
            Microsoft.Commerce.Payments.PXCommon.MoveToPageIndexActionContext upiPageIndex = (Microsoft.Commerce.Payments.PXCommon.MoveToPageIndexActionContext)paymentMethodsMap.PossibleOptions["upi"].PidlAction.Context;
            Assert.IsTrue(paymentMethodsMap.PossibleOptions.ContainsKey("ewallet_stored_value"));

            if (pmGroupId.StartsWith("offline_bank_transfer"))
            {
                Assert.AreEqual(pidls[0].DisplayPages[0].HintId, "paymentMethodSubGroupPage_invoice_offline_bank_transfer");
                Assert.AreEqual(pidls[0].DisplayPages[1].HintId, "paymentMethodSelectPage");
                Assert.AreEqual(pidls[0].DisplayPages[2].HintId, "paymentMethodSubGroupPage_ewallet_ewallet");
                Assert.AreEqual(pidls[0].DisplayPages[3].HintId, "paymentMethodSubGroupPage_upi_real_time_payments");
                Assert.AreEqual(ewalletPageIndex.PageIndex, 2);
                Assert.AreEqual(invoicePageIndex.PageIndex, 0);
                Assert.AreEqual(upiPageIndex.PageIndex, 3);
            }
            else if (pmGroupId.StartsWith("ewallet"))
            {
                int ewalletSubPageIndex = 1 - actualHomePageIndex;
                Assert.AreEqual(pidls[0].DisplayPages[ewalletSubPageIndex].HintId, "paymentMethodSubGroupPage_ewallet_ewallet");
                Assert.AreEqual(pidls[0].DisplayPages[actualHomePageIndex].HintId, "paymentMethodSelectPage");
                Assert.AreEqual(pidls[0].DisplayPages[2].HintId, "paymentMethodSubGroupPage_invoice_offline_bank_transfer");
                Assert.AreEqual(pidls[0].DisplayPages[3].HintId, "paymentMethodSubGroupPage_upi_real_time_payments");
                Assert.AreEqual(ewalletPageIndex.PageIndex, ewalletSubPageIndex);
                Assert.AreEqual(invoicePageIndex.PageIndex, 2);
                Assert.AreEqual(upiPageIndex.PageIndex, 3);
            }
            else if (pmGroupId.StartsWith("upi"))
            {
                Assert.AreEqual(pidls[0].DisplayPages[0].HintId, "paymentMethodSubGroupPage_upi_real_time_payments");
                Assert.AreEqual(pidls[0].DisplayPages[1].HintId, "paymentMethodSelectPage");
                Assert.AreEqual(pidls[0].DisplayPages[2].HintId, "paymentMethodSubGroupPage_ewallet_ewallet");
                Assert.AreEqual(pidls[0].DisplayPages[3].HintId, "paymentMethodSubGroupPage_invoice_offline_bank_transfer");
                Assert.AreEqual(ewalletPageIndex.PageIndex, 2);
                Assert.AreEqual(invoicePageIndex.PageIndex, 3);
                Assert.AreEqual(upiPageIndex.PageIndex, 0);
            }

            if (!shouldShiftPage)
            {
                Assert.AreEqual(pidls[0].DisplayPages[0].HintId, "paymentMethodSelectPage");
                Assert.AreEqual(pidls[0].DisplayPages[1].HintId, "paymentMethodSubGroupPage_ewallet_ewallet");
                Assert.AreEqual(pidls[0].DisplayPages[2].HintId, "paymentMethodSubGroupPage_invoice_offline_bank_transfer");
                Assert.AreEqual(pidls[0].DisplayPages[3].HintId, "paymentMethodSubGroupPage_upi_real_time_payments");
            }
        }
    }
}
