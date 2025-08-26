// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class PXServiceApiVersionHandlerTests : TestBase
    {
        [TestInitialize]
        public void TestInitialize()
        {
            PXHandler.CallInnerHandler = false;
            PXHandler.PostProcess = (HttpRequestMessage request, HttpResponseMessage response) =>
            {
                response = new HttpResponseMessage(HttpStatusCode.OK);
                return response;
            };
        }

        [TestMethod]
        public async Task ApiVersionHandlerReturnsSucceessOnSupportedVersion()
        {
            // Act
            var response = await PXClient.GetAsync(GetPXServiceUrl("/v7.0/probe"));

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Response status code was not as expected");
        }

        [TestMethod]
        public async Task ApiVersionHandlerReturnsErrorOnUnsupportedVersion()
        {
            // Act
            var response = await PXClient.GetAsync(GetPXServiceUrl("/v8.0/probe"));
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(await response.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "Response status code was not as expected");
            Assert.IsNotNull(errorResponse, "Response could not be deserialized to an expected ErrorResponse object");
            Assert.AreEqual("InvalidApiVersion", errorResponse.ErrorCode, "ErrorResponse.ErrorCode was incorrect");
        }

        // Bug 16146733: When no api version is found, PXService returns InvalidApiVersion instead of NoApiVersion
        [TestMethod]
        [Ignore]
        [WorkItem(16146733)]
        public async Task ApiVersionHandlerReturnsErrorOnMissingVersion()
        {
            // Act
            var response = await PXClient.GetAsync(GetPXServiceUrl("/probe"));
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(await response.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode, "Response status code was not as expected");
            Assert.IsNotNull(errorResponse, "Response could not be deserialized to an expected ErrorResponse object");
            Assert.AreEqual("NoApiVersion", errorResponse.ErrorCode, "ErrorResponse.ErrorCode was incorrect");
        }

        [TestMethod]
        public async Task ApiVersionHandlerSetsApiVersionInRequestProperties()
        {
            // Arrange
            bool versionKeyFound = false;
            object versionObject = null;
            ApiVersion version = null;
            PXHandler.PreProcess = (request) =>
            {
                versionKeyFound = request.GetProperties().ContainsKey(PaymentConstants.Web.Properties.Version);
                versionObject = request.GetProperties()[PaymentConstants.Web.Properties.Version];
                version = versionObject as ApiVersion;
            };

            // Act
            await PXClient.GetAsync(GetPXServiceUrl("/v7.0/probe"));

            // Assert
            Assert.IsTrue(versionKeyFound, "Version key was not found in request properties");
            Assert.IsNotNull(versionObject, "Version is null in request properties");
            Assert.IsNotNull(version, "Version in request propertis is not of type {0}", typeof(ApiVersion).FullName);
            Assert.AreEqual("v7.0", version.ExternalVersion, "Version.ExternalVersion in request properties is incorrect");
            Assert.AreEqual(7, version.InternalVersion.Major, "Version.InternalVersion.Major in request properties is incorrect");
            Assert.AreEqual(0, version.InternalVersion.Minor, "Version.InternalVersion.Minor in request properties is incorrect");
        }

        [TestMethod]
        public async Task ApiVersionHandlerSetsFlightContextInRequestProperties()
        {
            // Arrange
            bool flightContextKeyFound = false;
            object flightContextObject = null;
            Dictionary<string, string> flightContext = null;
            PXHandler.PreProcess = (request) =>
            {
                flightContextKeyFound = request.GetProperties().ContainsKey("PX.FlightContext");
                flightContextObject = request.GetProperties()["PX.FlightContext"];
                flightContext = flightContextObject as Dictionary<string, string>;
            };

            // Act
            await PXClient.GetAsync(GetPXServiceUrl("/v7.0/f2ac3e1d-e724-4820-baa0-0098584c6dcc/addressDescriptions?country=us&partner=webblends"));

            // Assert
            Assert.IsTrue(flightContextKeyFound, "Flight context key was not found in request properties");
            Assert.IsNotNull(flightContextObject, "Flight context is null in request properties");
            Assert.IsNotNull(flightContext, "Flight context in request properties is not of type {0}", typeof(Dictionary<string, string>).FullName);
            Assert.AreEqual("webblends", flightContext["partner"], "Partner flight context is incorrect");
            Assert.AreEqual("us", flightContext["country"], "Country flight context is incorrect");
            Assert.AreEqual("f2ac3e1d-e724-4820-baa0-0098584c6dcc", flightContext["accountId"], "AccountId flight context is incorrect");
        }

        [TestMethod]
        public async Task ApiVersionHandlerSetsQueryParametersInRequestProperties()
        {
            // Arrange
            bool queryParamsKeyFound = false;
            object queryParamsObject = null;
            IEnumerable<KeyValuePair<string, string>> queryParams = null;
            PXHandler.PreProcess = (request) =>
            {
                queryParamsKeyFound = request.GetProperties().ContainsKey("Payments.QueryParameters");
                queryParamsObject = request.GetProperties()["Payments.QueryParameters"];
                queryParams = queryParamsObject as IEnumerable<KeyValuePair<string, string>>;
            };

            // Act
            await PXClient.GetAsync(GetPXServiceUrl("/v7.0/f2ac3e1d-e724-4820-baa0-0098584c6dcc/addressDescriptions?country=us&partner=webblends"));

            // Assert
            Assert.IsTrue(queryParamsKeyFound, "Query parameters key was not found in request properties");
            Assert.IsNotNull(queryParamsObject, "Query parameters is null in request properties");
            Assert.IsNotNull(queryParams, "Query parameters in request properties is not of type IEnumerable<KeyValuePair<string, object>");
            Assert.AreEqual(2, queryParams.Count());
            Assert.IsNotNull(
                queryParams.FirstOrDefault(kv => string.Equals(kv.Key, "partner") && string.Equals(kv.Value, "webblends")).Value,
                "Query parameter partner=webblends was not found");
            Assert.IsNotNull(
                queryParams.FirstOrDefault(kv => string.Equals(kv.Key, "country") && string.Equals(kv.Value, "us")).Value,
                "Query parameter country=us was not found");
        }

        [TestMethod]
        public async Task ApiVersionHandlerSetsOperationVersionInResponseHeader()
        {
            // Act
            var response = await PXClient.GetAsync(GetPXServiceUrl("/v7.0/f2ac3e1d-e724-4820-baa0-0098584c6dcc/addressDescriptions?country=us&partner=webblends"));

            // Assert
            Assert.IsTrue(response.Headers.Contains("x-ms-operation-version"), "Operation version header was found in the response header");
            CollectionAssert.AreEquivalent(
                response.Headers.GetValues("x-ms-operation-version").ToList<string>(),
                new List<string>() { "v7.0" },
                "Opeartion version is incorrect in the response header");
        }

        [DataRow("/v7.0/f2ac3e1d-e724-4820-baa0-0098584c6dcc/addressDescriptions?country=us&partner=webblends")]
        [DataRow("/v7.0/f2ac3e1d-e724-4820-baa0-0098584c6dcc/addressDescriptions?country=us&partner=oxowebdirect")]
        [DataRow("/v7.0/paymentMethodDescriptions?family=credit_card&operation=Search&partner=commercialsupport&country=us")]
        [DataRow("/v7.0/f2ac3e1d-e724-4820-baa0-0098584c6dcc/paymentMethodDescriptions?family=credit_card")]

        [DataTestMethod]
        [Ignore]

        // Task 26049283: Issue related to enable flights, see task description for more details
        public async Task ApiVersionHandlerSetsContentTypeOptionsInResponseHeader(string pxSubPath)
        {
            // Act
            var response = await PXClient.GetAsync(GetPXServiceUrl(pxSubPath));

            // Assert
            Assert.IsTrue(response.Headers.Contains("X-Content-Type-Options"), "Content Type Options header was not found in the response header");
            CollectionAssert.AreEquivalent(
                new List<string>() { "nosniff" },
                response.Headers.GetValues("X-Content-Type-Options").ToList<string>(),
                "Content Type Options header value is not as expected in the response header");
        }

        [DataRow("/v7.0/f2ac3e1d-e724-4820-baa0-0098584c6dcc/addressDescriptions?country=us&partner=webblends", false)]
        [DataRow("/v7.0/f2ac3e1d-e724-4820-baa0-0098584c6dcc/addressDescriptions?country=us&partner=oxowebdirect", false)]
        [DataRow("/v7.0/paymentMethodDescriptions?family=credit_card&operation=Search&partner=commercialsupport&country=us", true)]
        [DataRow("/v7.0/f2ac3e1d-e724-4820-baa0-0098584c6dcc/paymentMethodDescriptions?family=credit_card", false)]
        [DataTestMethod]
        [Ignore]
        //// Task 26049283: Issue related to enable flights, see task description for more details
        public async Task ApiVersionHandlerSetsContentTypeOptionsInResponseHeader(string pxSubPath, bool retryOnServerError)
        {
            // Arrange
            if (!retryOnServerError)
            {
                PXFlightHandler.AddToEnabledFlights("PXSendNoRetryOnServerErrorHeader");
            }

            // Act
            var response = await PXClient.GetAsync(GetPXServiceUrl(pxSubPath));

            // Assert
            IEnumerable<string> headerValue;
            bool hasHeaderValue = response.Headers.TryGetValues("x-ms-px-retry-servererr", out headerValue);
            if (!retryOnServerError)
            {
                Assert.IsTrue(headerValue.Contains("false"));
            }
            else
            {
                Assert.IsFalse(hasHeaderValue);
            }
        }

        [DataRow("/v7.0/f2ac3e1d-e724-4820-baa0-0098584c6dcc/addressDescriptions?country=us&partner=webblends", false)]
        [DataRow("/v7.0/f2ac3e1d-e724-4820-baa0-0098584c6dcc/addressDescriptions?country=us&partner=oxowebdirect", false)]
        [DataRow("/v7.0/paymentMethodDescriptions?family=credit_card&operation=Search&partner=commercialsupport&country=us", true)]
        [DataRow("/v7.0/f2ac3e1d-e724-4820-baa0-0098584c6dcc/paymentMethodDescriptions?family=credit_card", false)]
        [DataTestMethod]
        [Ignore]
        //// Task 26049283: Issue related to enable flights, see task description for more details
        //// ApiVersionHandlerSetsContentTypeOptionsInResponseHeader test in CIT.PXService is currently ignored, 
        //// because enable flight is evaluated in one of the handlers, we don't have an easy way to fix this issue at this moment. 
        public async Task ApiVersionHandlerReturn502WithPXReturn502ForMaliciousRequestFlighting(string pxSubPath, bool enablePXReturn502ForMaliciousRequestFlighting)
        {
            // Arrange
            if (enablePXReturn502ForMaliciousRequestFlighting)
            {
                PXFlightHandler.AddToEnabledFlights("PXReturn502ForMaliciousRequest");
            }

            // Act
            var response = await PXClient.GetAsync(GetPXServiceUrl(pxSubPath));

            // Assert
            IEnumerable<string> headerValue;
            bool hasHeaderValue = response.Headers.TryGetValues("x-ms-px-retry-servererr", out headerValue);
            if (enablePXReturn502ForMaliciousRequestFlighting)
            {
                Assert.IsTrue(headerValue.Contains("false"));
                Assert.AreEqual(HttpStatusCode.BadGateway, response.StatusCode);
            }
            else
            {
                Assert.IsFalse(hasHeaderValue);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }
        }
    }
}