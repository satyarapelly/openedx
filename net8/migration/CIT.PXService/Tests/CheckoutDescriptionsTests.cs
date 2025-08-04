// <copyright file="CheckoutDescriptionsTests.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using global::Tests.Common.Model.Pidl;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json.Linq;

    [TestClass]
    public class CheckoutDescriptionsTests : TestBase
    {
        [DataRow(4, "/v7.0/checkoutDescriptions?operation=RenderPidlPage&checkoutId=123&partner=msteams&paymentProviderId=stripe&redirectUrl=pay.microsoft.com&country=us")]
        [DataRow(2, "/v7.0/checkoutDescriptions?operation=RenderPidlPage&checkoutId=123&partner=msteams&paymentProviderId=paypal&redirectUrl=pay.microsoft.com&country=us")]
        [DataRow(4, "/v7.0/checkoutDescriptions?operation=RenderPidlPage&checkoutId=123&partner=defaulttemplate&paymentProviderId=stripe&redirectUrl=pay.microsoft.com&country=us")]
        [DataRow(2, "/v7.0/checkoutDescriptions?operation=RenderPidlPage&checkoutId=123&partner=defaulttemplate&paymentProviderId=paypal&redirectUrl=pay.microsoft.com&country=us")]
        [DataRow(4, "/v7.0/checkoutDescriptions?operation=RenderPidlPage&checkoutId=123&partner=officesmb&paymentProviderId=stripe&redirectUrl=pay.microsoft.com&country=us")]
        [DataRow(2, "/v7.0/checkoutDescriptions?operation=RenderPidlPage&checkoutId=123&partner=officesmb&paymentProviderId=paypal&redirectUrl=pay.microsoft.com&country=us")]
        [DataTestMethod]
        public async Task GetCheckoutDescriptions(int expectedPidlsCount, string requestUrl)
        {
            // Arrange
            var headers = new Dictionary<string, string>();

            if (requestUrl.Contains("officesmb"))
            {
                string expectedPSSResponse = "{\"renderPidlPage\":{\"template\":\"defaulttemplate\",\"features\":null}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
                headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };
            }

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(requestUrl, additionaHeaders: headers);
            
            // Assert
            Assert.IsNotNull(pidls, "PIDLs are expected not to be null");
            Assert.AreEqual(expectedPidlsCount, pidls.Count);           
        }

        [DataRow(GlobalConstants.PaymentProviders.Stripe, "/v7.0/checkoutDescriptions?operation=RenderPidlPage&checkoutId=123&partner=msteams&paymentProviderId=stripe&country=us&redirectUrl=pay.microsoft.com", true, "Back button should be invisible for ")]
        [DataRow(GlobalConstants.PaymentProviders.Paypal, "/v7.0/checkoutDescriptions?operation=RenderPidlPage&checkoutId=123&partner=msteams&paymentProviderId=paypal&country=us&redirectUrl=pay.microsoft.com&family=ewallet&type=paypal&scenario=pidlClientAction", false, "Back button should be visible on credit card form for ")]
        [DataRow(GlobalConstants.PaymentProviders.Paypal, "/v7.0/checkoutDescriptions?operation=RenderPidlPage&checkoutId=123&partner=msteams&paymentProviderId=paypal&country=us&redirectUrl=pay.microsoft.com&family=credit_card&type=visa&scenario=pidlClientAction", false, "Back button should be visible on paypal redirection form for ")]
        [DataRow(GlobalConstants.PaymentProviders.Paypal, "/v7.0/checkoutDescriptions?operation=RenderPidlPage&checkoutId=123&partner=msteams&paymentProviderId=paypal&country=us&redirectUrl=pay.microsoft.com", null, "Back button object should be null on PM selection page for ")]
        [DataRow(GlobalConstants.PaymentProviders.Stripe, "/v7.0/checkoutDescriptions?operation=RenderPidlPage&checkoutId=123&partner=defaulttemplate&paymentProviderId=stripe&country=us&redirectUrl=pay.microsoft.com", true, "Back button should be invisible for ")]
        [DataRow(GlobalConstants.PaymentProviders.Paypal, "/v7.0/checkoutDescriptions?operation=RenderPidlPage&checkoutId=123&partner=defaulttemplate&paymentProviderId=paypal&country=us&redirectUrl=pay.microsoft.com&family=ewallet&type=paypal&scenario=pidlClientAction", false, "Back button should be visible on credit card form for ")]
        [DataRow(GlobalConstants.PaymentProviders.Paypal, "/v7.0/checkoutDescriptions?operation=RenderPidlPage&checkoutId=123&partner=defaulttemplate&paymentProviderId=paypal&country=us&redirectUrl=pay.microsoft.com&family=credit_card&type=visa&scenario=pidlClientAction", false, "Back button should be visible on paypal redirection form for ")]
        [DataRow(GlobalConstants.PaymentProviders.Paypal, "/v7.0/checkoutDescriptions?operation=RenderPidlPage&checkoutId=123&partner=defaulttemplate&paymentProviderId=paypal&country=us&redirectUrl=pay.microsoft.com", null, "Back button object should be null on PM selection page for ")]
        [DataRow(GlobalConstants.PaymentProviders.Stripe, "/v7.0/checkoutDescriptions?operation=RenderPidlPage&checkoutId=123&partner=officesmb&paymentProviderId=stripe&country=us&redirectUrl=pay.microsoft.com", true, "Back button should be invisible for ")]
        [DataRow(GlobalConstants.PaymentProviders.Paypal, "/v7.0/checkoutDescriptions?operation=RenderPidlPage&checkoutId=123&partner=officesmb&paymentProviderId=paypal&country=us&redirectUrl=pay.microsoft.com&family=ewallet&type=paypal&scenario=pidlClientAction", false, "Back button should be visible on credit card form for ")]
        [DataRow(GlobalConstants.PaymentProviders.Paypal, "/v7.0/checkoutDescriptions?operation=RenderPidlPage&checkoutId=123&partner=officesmb&paymentProviderId=paypal&country=us&redirectUrl=pay.microsoft.com&family=credit_card&type=visa&scenario=pidlClientAction", false, "Back button should be visible on paypal redirection form for ")]
        [DataRow(GlobalConstants.PaymentProviders.Paypal, "/v7.0/checkoutDescriptions?operation=RenderPidlPage&checkoutId=123&partner=officesmb&paymentProviderId=paypal&country=us&redirectUrl=pay.microsoft.com", null, "Back button object should be null on PM selection page for ")]
        [DataTestMethod]
        public async Task CheckBackButtonExistence(string providerID, string requestUrl, bool? expectedBackButtonHidden, string message)
        {
            // Arrange
            var headers = new Dictionary<string, string>();

            if (requestUrl.Contains("officesmb"))
            {
                string expectedPSSResponse = "{\"renderPidlPage\":{\"template\":\"defaulttemplate\",\"features\":null}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
                headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };
            }

            // Act
            List<PIDLResource> pidls = await GetPidlFromPXService(requestUrl, additionaHeaders: headers);

            // Assert
            Assert.IsNotNull(pidls, "PIDLs are expected not to be null");

            foreach (var pidl in pidls)
            {
                var backButton = pidl.GetDisplayHintById("backButton") as ButtonDisplayHint;
                
                if (backButton != null)
                {
                    Assert.AreEqual(expectedBackButtonHidden, backButton.IsHidden, message + providerID);
                }
                else
                {
                    Assert.AreEqual(expectedBackButtonHidden, backButton, message + providerID);
                }
            }         
        }

        [DataRow(GlobalConstants.Defaults.Locale, GlobalConstants.PaymentProviders.Stripe, GlobalConstants.Defaults.Locale)]
        [DataRow(GlobalConstants.Defaults.Locale, GlobalConstants.PaymentProviders.Paypal, GlobalConstants.Defaults.Locale)]
        [DataRow("fr-fr", GlobalConstants.PaymentProviders.Stripe, "fr-fr")]
        [DataRow("fr-fr", GlobalConstants.PaymentProviders.Paypal, "fr-fr")]
        [DataRow("it-it", GlobalConstants.PaymentProviders.Stripe, "it-it")]
        [DataRow("it-it", GlobalConstants.PaymentProviders.Paypal, "it-it")]
        [DataRow("fr-dz", GlobalConstants.PaymentProviders.Stripe, GlobalConstants.Defaults.Locale)]
        [DataRow("fr-dz", GlobalConstants.PaymentProviders.Paypal, GlobalConstants.Defaults.Locale)]
        [DataRow(null, GlobalConstants.PaymentProviders.Stripe, GlobalConstants.Defaults.Locale)]
        [DataRow(null, GlobalConstants.PaymentProviders.Paypal, GlobalConstants.Defaults.Locale)]
        [DataTestMethod]
        public async Task GetCheckoutDescriptionsWithTerms(string locale, string providerID, string exceptedLocale)
        {
            // Arrange
            var headers = new Dictionary<string, string>();
            List<string> partners = new List<string> { "msteams", "defaulttemplate", "officesmb" };
            List<PIDLResource> pidls = null;
            var expectedTermsUrl = string.Format("https://staticresources.payments.microsoft-int.com/staticresourceservice/resources/checkout/{0}/terms.htm", exceptedLocale);

            // Act
            foreach (string partner in partners)
            {
                if (string.Equals(partner, "officesmb", StringComparison.OrdinalIgnoreCase))
                {
                    string expectedPSSResponse = "{\"renderPidlPage\":{\"template\":\"defaulttemplate\",\"features\":null}}";
                    PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
                    headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };
                }

                string requestUrl = $"/v7.0/checkoutDescriptions?operation=RenderPidlPage&checkoutId=123&partner={partner}&paymentProviderId={providerID}&redirectUrl=pay.microsoft.com&country=us";

                if (!string.IsNullOrEmpty(locale))
                {
                    requestUrl += "&language=" + locale;
                }

                pidls = await GetPidlFromPXService(requestUrl, additionaHeaders: headers);
            }

            // Assert
            Assert.IsNotNull(pidls, "PIDLs are expected not to be null");

            foreach (var pidl in pidls)
            {
                var termsCheckout = pidl.GetDisplayHintById("termsCheckout") as HyperlinkDisplayHint;
                Assert.AreEqual(termsCheckout.SourceUrl, expectedTermsUrl, "Current locale should be part of the terms url");
            }
        }

        [DataTestMethod]
        public async Task GetCheckoutDescriptions_PaypalProvider_PidlContext()
        {
            // Arrange
            List<string> partners = new List<string> { "msteams", "defaulttemplate", "officesmb" };
            HttpResponseMessage result = null;
            string url = string.Empty;

            // Act
            foreach (string partner in partners)
            {
                if (string.Equals(partner, "officesmb", StringComparison.OrdinalIgnoreCase))
                {
                    string expectedPSSResponse = "{\"renderPidlPage\":{\"template\":\"defaulttemplate\",\"features\":null}}";
                    PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
                }

                url = $"/v7.0/checkoutDescriptions?operation=RenderPidlPage&checkoutId=123&partner={partner}&paymentProviderId=paypal&redirectUrl=pay.microsoft.com&country=us&scenario=pidlContext";
                var req = new HttpRequestMessage(HttpMethod.Get, url);
                req.Headers.Add("x-ms-flight", "PXDisablePSSCache");
                result = await PXClient.SendAsync(req);
            }

            var pidlResource = ReadSinglePidlResourceFromJson(await result.Content.ReadAsStringAsync());
            JObject contextJson = JObject.Parse(Convert.ToString(pidlResource.ClientAction.Context));

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(ClientActionType.Pidl, pidlResource.ClientAction.ActionType);
            Assert.IsNotNull(pidlResource.ClientAction.Context);
            Assert.AreEqual("collectResourceInfo", contextJson.SelectToken("$.resourceActionContext.action"));
            Assert.AreEqual("pidlClientAction", contextJson.SelectToken("$.resourceActionContext.pidlDocInfo.parameters.scenario"));
        }

        [DataTestMethod]
        public async Task GetCheckoutDescriptions_Paid_Redirect()
        {
            // Arrange
            var headers = new Dictionary<string, string>();
            List<string> partners = new List<string> { "msteams", "defaulttemplate", "offciesmb" };
            List<PIDLResource> pidls = null;
            string url = string.Empty;
            string redirectUrl = "pay.microsoft.com";

            // Act
            foreach (string partner in partners)
            {
                if (string.Equals(partner, "officesmb", StringComparison.OrdinalIgnoreCase))
                {
                    string expectedPSSResponse = "{\"renderPidlPage\":{\"template\":\"defaulttemplate\",\"features\":null}}";
                    PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
                    headers = new Dictionary<string, string>() { { "x-ms-flight", "PXDisablePSSCache" } };
                }

                url = $"/v7.0/checkoutDescriptions?operation=RenderPidlPage&checkoutId=checkoutid-paid&partner={partner}&paymentProviderId=paypal&redirectUrl={redirectUrl}&country=us";
                pidls = await GetPidlFromPXService(url, HttpStatusCode.OK, additionaHeaders: headers);
            }

            JObject contextJson = JObject.Parse(Convert.ToString(pidls[0].ClientAction.Context));

            // Assert
            Assert.IsNotNull(pidls, "PIDLs are expected not to be null");
            Assert.AreEqual(1, pidls.Count);
            Assert.AreEqual(ClientActionType.Redirect, pidls[0].ClientAction.ActionType);
            Assert.AreEqual(redirectUrl, contextJson.SelectToken("$.baseUrl"));
        }
    }
}
