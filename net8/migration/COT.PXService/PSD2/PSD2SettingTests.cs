// <copyright file="PSD2SettingTests.cs" company="Microsoft">Copyright (c) Microsoft 2019-2020. All rights reserved.</copyright>

namespace COT.PXService.PSD2
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Test.Common;

    [TestClass]
    public class PSD2SettingTests : TestBase
    {
        public TestSettings TestSettings { get; private set; }

        [TestInitialize]
        public void TestInitialize()
        {
            this.Initialize();
            this.TestSettings = TestSettings.CreateInstance();
        }

        /// <summary>
        /// Test Get PaymentClient Settings
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        [DeploymentItem(@"TestData\V16\PaymentClientSettings.json", @"TestData\V16")]
        [DeploymentItem(@"TestData\V17\PaymentClientSettings.json", @"TestData\V17")]
        public void GetPaymentClientSettings()
        {
            this.VerifyGetPaymentClientSettings("V16");
            this.VerifyGetPaymentClientSettings("V17");
        }

        private void VerifyGetPaymentClientSettings(string settingsVersion)
        {
            HttpStatusCode code = HttpStatusCode.Unused;
            string body = null;
            this.ExecuteRequest(
                string.Format("v7.0/settings/Microsoft.Payments.Client/{0}", settingsVersion),
                HttpMethod.Get,
                null,
                null,
                null,
                (responseCode, responseBody) =>
                {
                    code = responseCode;
                    body = responseBody.ToString();
                });

            Assert.AreEqual(HttpStatusCode.OK, code);
            var actualResp = JsonConvert.DeserializeObject<JObject>(body);

            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format(@"TestData\{0}\PaymentClientSettings.json", settingsVersion));
            string expectedPaymentClientSettings = File.ReadAllText(filePath);
            var expectedResp = JsonConvert.DeserializeObject<JObject>(expectedPaymentClientSettings);
            Assert.IsTrue(JToken.DeepEquals(expectedResp, actualResp), "Expected paymentclientsettings is not returned");
        }
    }
}
