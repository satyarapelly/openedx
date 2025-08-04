// <copyright file="PaymentInstrumentsExTest.cs" company="Microsoft">Copyright (c) Microsoft 2019-2020. All rights reserved.</copyright>


namespace COT.PXService
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Test.Common;
    using Common = Microsoft.Commerce.Payments.Common.Transaction;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model;
    using Microsoft.Commerce.Payments.PXService.Model.ThreeDSExternalService;
    using PayerAuth = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService;
    using System.Threading.Tasks;

    [TestClass]
    public class PaymentInstrumentsExTest : TestBase
    {
        public TestSettings TestSettings { get; private set; }

        [TestInitialize]
        public void TestInitialize()
        {
            this.Initialize();
            this.TestSettings = TestSettings.CreateInstance();
        }

        [TestMethod]
        [TestCategory(TestCategory.INTContinuousTest)]
        public void WindowsStore_PayPalQrCode()
        {
            AccountInfo testAcc = TestSettings.GetPSD2Account(false);
            Common.TestContext tc = new Common.TestContext(
                      contact: "px.azure.cot",
                      retention: DateTime.MaxValue,
                      scenarios: "px.pims.paypal.add.success");

            HttpStatusCode code = HttpStatusCode.Unused;
            string body = null;
            var headers = new Dictionary<string, string>();

            object data = JsonConvert.DeserializeObject<object>("{\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"paypal\",\"paymentMethodOperation\":\"add\",\"paymentMethodCountry\":\"us\",\"paymentMethodResource_id\":\"ewallet.paypalRedirect\",\"sessionId\":\"a6ef53f6-d257-157f-b8b0-7ccb6d9bf5b3\",\"context\":\"purchase\",\"riskData\":{\"dataType\":\"payment_method_riskData\",\"dataOperation\":\"add\",\"dataCountry\":\"us\"},\"details\":{\"dataType\":\"ewallet_paypalRedirect_details\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"authenticationMode\":\"Redirect\"}}");

            this.ExecuteRequest(
                 string.Format("v7.0/{0}/paymentInstrumentsEx?country=us&language=en-US&partner=windowsstore", this.TestSettings.AccountId),
                HttpMethod.Post,
                tc,
                data,
                headers,
                (responseCode, responseBody) =>
                {
                    code = responseCode;
                    body = responseBody?.ToString();
                });

            Assert.AreEqual(HttpStatusCode.OK, code, "");
            Assert.IsTrue(body.Contains("pay-int.ms"), "Body must contain short url with domain 'pay-int.ms'");
        }
    }
}
