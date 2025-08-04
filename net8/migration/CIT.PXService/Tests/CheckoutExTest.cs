// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2022. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    [TestClass]
    public class CheckoutExTest : TestBase
    {
        [TestMethod]
        [DataRow(GlobalConstants.PaymentProviders.Stripe, GlobalConstants.Defaults.Locale, "dummytoken")]
        [DataRow(GlobalConstants.PaymentProviders.Stripe, GlobalConstants.Defaults.Locale, "ProcessorDeclined")]
        [DataRow(GlobalConstants.PaymentProviders.Stripe, "fr-FR", "dummytoken")]
        [DataRow(GlobalConstants.PaymentProviders.Stripe, null, "dummytoken")]
        [DataRow(GlobalConstants.PaymentProviders.Paypal, GlobalConstants.Defaults.Locale, "dummytoken")]
        [DataRow(GlobalConstants.PaymentProviders.Paypal, GlobalConstants.Defaults.Locale, "ProcessorDeclined")]
        [DataRow(GlobalConstants.PaymentProviders.Paypal, "fr-FR", "dummytoken")]
        [DataRow(GlobalConstants.PaymentProviders.Paypal, null, "dummytoken")]
        public async Task CheckoutEx_ChargeAsExpected(string paymentProviderId, string language, string accountToken)
        {
            // Arrange
            string checkoutId = "123";
            List<string> partners = new List<string> { "msteams", "defaulttemplate", "officesmb" };
            HttpResponseMessage result = new HttpResponseMessage();
            bool isLanguagePassed = false;
            var payload = new
            {
                context = "purchase",
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                emailAddress = "test@microsoft.com",
                paymentMethodCountry = "sj",
                details = new
                {
                    accountHolderName = "test test",
                    accountToken = accountToken,
                    cvvToken = "dummytoken",
                    expiryMonth = "11",
                    expiryYear = "2033",
                    address = new
                    {
                        // postal code is intentionally not included since it is optional.
                        country = "sj",
                    }
                }
            };

            foreach (var partner in partners)
            {
                if (string.Equals(partner, "officesmb", StringComparison.OrdinalIgnoreCase))
                {
                    string expectedPSSResponse = "{\"renderPidlPage\":{\"template\":\"defaulttemplate\",\"features\":null}}";
                    PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
                }

                string pxServiceRelativePath = $"v7.0/checkoutsEx/{checkoutId}/charge?partner={partner}&paymentProviderId={paymentProviderId}&redirectUrl=pay.microsoft.com";
                
                if (!string.IsNullOrEmpty(language))
                {
                    pxServiceRelativePath += "&language=" + language;
                }

                PXSettings.PaymentThirdPartyService.PreProcess = async (ptpsRequest) =>
                {
                    if (ptpsRequest.RequestUri.AbsolutePath.Contains($"/charge"))
                    {
                        string requestContent = await ptpsRequest.Content.ReadAsStringAsync();
                        JObject jsonObjPtps = JObject.Parse(requestContent);
                        var locale = string.IsNullOrEmpty(language) ? GlobalConstants.Defaults.Locale : language;

                        isLanguagePassed = jsonObjPtps.SelectToken("failureUrl").Value<string>().Contains(locale);
                    }
                };

                // Act
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(GetPXServiceUrl(pxServiceRelativePath)),
                    Method = HttpMethod.Post,
                    Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
                };

                result = await PXClient.SendAsync(request);
            }

            // Assert
            if (result.StatusCode == HttpStatusCode.OK)
            {
                string resultContent = await result.Content.ReadAsStringAsync();
                JObject jsonObj = JObject.Parse(resultContent);

                if (accountToken == "ProcessorDeclined")
                {
                    Assert.AreEqual("tppcheckouterrorpidl", jsonObj.SelectToken("clientAction.context[0].identity.type").Value<string>());
                    Assert.AreEqual("Your payment wasn't completed. Please use a different payment method.", jsonObj.SelectToken("clientAction.context[0].displayDescription[0].members[0].members[1].displayContent").Value<string>());
                }
                else
                {
                    Assert.AreEqual("Redirect", jsonObj.SelectToken("clientAction.type").Value<string>());
                    Assert.IsTrue(isLanguagePassed, "Language param added to failure URL");
                }
            }
            else
            {
                Assert.Fail("No status other than OK / BadRequest is expected");
            }

            PXSettings.PaymentThirdPartyService.ResetToDefaults();
        }
    }
}
