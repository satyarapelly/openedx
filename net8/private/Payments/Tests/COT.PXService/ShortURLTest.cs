// <copyright file="ShortURLTest.cs" company="Microsoft">Copyright (c) Microsoft 2019-2020. All rights reserved.</copyright>


namespace COT.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using Microsoft.Commerce.Payments.PXService.Model.ShortURLDB;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Test.Common;

    [TestClass]
    public class ShortURLTest : TestBase
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
        public void ShortURLController_CreateAndGet()
        {
            AccountInfo testAcc = TestSettings.GetPSD2Account(false);

            HttpStatusCode code = HttpStatusCode.Unused;
            string body = null;
            var headers = new Dictionary<string, string>();

            // Health check
            this.ExecuteRequest(
                 string.Format("v7.0/shortURL"),
                HttpMethod.Get,
                null,
                null,
                headers,
                (responseCode, responseBody) =>
                {
                    code = responseCode;
                    body = responseBody?.ToString();
                });

            string healthCheckMessage = "Welcome to ShortURL in PX!";
            Assert.AreEqual(HttpStatusCode.OK, code, $"Responses should be OK, instead got {code}");
            Assert.IsTrue(body != null && body.Contains(healthCheckMessage), $"Uri should not be null, should say '{healthCheckMessage}'");

            CreateRequest requestData = new CreateRequest
            {
                URL = "https://www.bing.com",
            };

            this.ExecuteRequest(
                 string.Format("v7.0/shortURL"),
                HttpMethod.Post,
                null,
                requestData,
                headers,
                (responseCode, responseBody) =>
                {
                    code = responseCode;
                    body = responseBody?.ToString();
                });

            Assert.AreEqual(HttpStatusCode.Created, code, $"Responses should be Created, instead got {code}");
            CreateResponse deserializedBody = JsonConvert.DeserializeObject<CreateResponse>(body);
            Assert.IsTrue(deserializedBody.Uri != null, "Uri should not be null");
            Assert.IsTrue(deserializedBody.Code != null, "Code should not be null");

            this.ExecuteRequest(
                 string.Format("v7.0/shortURL/{0}", deserializedBody.Code),
                HttpMethod.Get,
                null,
                null,
                headers,
                (responseCode, responseBody) =>
                {
                    code = responseCode;
                    body = responseBody?.ToString();
                });

            Assert.AreEqual(HttpStatusCode.Redirect, code, $"Calling {deserializedBody.Uri} should result in redirection");
        }
    }
}
