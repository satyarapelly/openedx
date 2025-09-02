// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web;
    using global::Tests.Common.Model.PX;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class FlightingTests : TestBase
    {
        /// <summary>
        /// Flight context is extracted from query string
        /// </summary>
        [TestMethod]
        public async Task FlightingContext_IsExtractedFromQueryString()
        {
            // Arrange
            Dictionary<string, string> actualFlightContext = null;
            PXHandler.PreProcess = (request) =>
            {
                actualFlightContext = request.GetProperties()["PX.FlightContext"] as Dictionary<string, string>;
            };

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                        "/v7.0/testCid/paymentMethodDescriptions?family=credit_card&type=visa&partner=cart&country=us&language=en-us"));

            // Assert
            Assert.IsTrue(actualFlightContext.ContainsKey("partner"));
            Assert.AreEqual(actualFlightContext["partner"], "cart");

            Assert.IsTrue(actualFlightContext.ContainsKey("country"));
            Assert.AreEqual(actualFlightContext["country"], "us");

            Assert.IsTrue(actualFlightContext.ContainsKey("language"));
            Assert.AreEqual(actualFlightContext["language"], "en-us");
        }

        /// <summary>
        /// Flight context should be extracted from a json object named "paymentSessionData" in the query
        /// </summary>
        [TestMethod]
        public async Task FlightingContext_IsExtractedFromQueryObject_PaymentSessionData()
        {
            // Arrange
            Dictionary<string, string> actualFlightContext = null;
            PXHandler.PreProcess = (request) =>
            {
                actualFlightContext = request.GetProperties()["PX.FlightContext"] as Dictionary<string, string>;
            };

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/testCid/PaymentSessionDescriptions?operation=Add&paymentSessionData={0}",
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                            new PaymentSessionData()
                            {
                                Amount = 10.0m,
                                Currency = "EUR",
                                Partner = "webblends",
                                Country = "gb",
                                PaymentInstrumentId = "testPiid",
                                Language = "en"
                            })))));

            // Assert
            Assert.IsTrue(actualFlightContext.ContainsKey("partner"));
            Assert.AreEqual(actualFlightContext["partner"], "webblends");

            Assert.IsTrue(actualFlightContext.ContainsKey("country"));
            Assert.AreEqual(actualFlightContext["country"], "gb");

            Assert.IsTrue(actualFlightContext.ContainsKey("language"));
            Assert.AreEqual(actualFlightContext["language"], "en");
        }

        /// <summary>
        /// Flight context should be extracted from a json object named "paymentSessionOrData" in the query
        /// </summary>
        [TestMethod]
        public async Task FlightingContext_IsExtractedFromQueryObject_PaymentSessionOrData()
        {
            // Arrange
            Dictionary<string, string> actualFlightContext = null;
            PXHandler.PreProcess = (request) =>
            {
                actualFlightContext = request.GetProperties()["PX.FlightContext"] as Dictionary<string, string>;
            };

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/testCid/challengeDescriptions?paymentSessionOrData={0}",
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                            new PaymentSessionData()
                            {
                                Amount = 10.0m,
                                Currency = "EUR",
                                Partner = "webblends",
                                Country = "gb",
                                PaymentInstrumentId = "testPiid",
                                Language = "en"
                            })))));

            // Assert
            Assert.IsTrue(actualFlightContext.ContainsKey("partner"));
            Assert.AreEqual(actualFlightContext["partner"], "webblends");

            Assert.IsTrue(actualFlightContext.ContainsKey("country"));
            Assert.AreEqual(actualFlightContext["country"], "gb");

            Assert.IsTrue(actualFlightContext.ContainsKey("language"));
            Assert.AreEqual(actualFlightContext["language"], "en");
        }

        /// <summary>
        /// Flight context in the query string overrides same named context if is available in the query object
        /// </summary>
        [TestMethod]
        public async Task FlightingContext_FromQueryStringOverridesQueryObject()
        {
            // Arrange
            Dictionary<string, string> actualFlightContext = null;
            PXHandler.PreProcess = (request) =>
            {
                actualFlightContext = request.GetProperties()["PX.FlightContext"] as Dictionary<string, string>;
            };

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/testCid/challengeDescriptions?language=de-DE&paymentSessionOrData={0}",
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                            new PaymentSessionData()
                            {
                                Amount = 10.0m,
                                Currency = "EUR",
                                Partner = "webblends",
                                Country = "gb",
                                PaymentInstrumentId = "testPiid",
                                Language = "en"
                            })))));

            // Assert
            Assert.IsTrue(actualFlightContext.ContainsKey("language"));
            Assert.AreEqual(actualFlightContext["language"], "de-DE");
        }

        /// <summary>
        /// When extracting flight context from an object, any exceptions from invalid json strings are caught and
        /// remaining context that can be extracted from query string are still extracted
        /// </summary>
        [TestMethod]
        public async Task FlightingContext_InvalidJsonObjectInQueryIsHandledGracefully()
        {
            // Arrange
            Dictionary<string, string> actualFlightContext = null;
            PXHandler.PreProcess = (request) =>
            {
                actualFlightContext = request.GetProperties()["PX.FlightContext"] as Dictionary<string, string>;
            };

            // Act
            // query string below has an invalid json string (it has a missing double quote after the property called partner)
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/testCid/challengeDescriptions?language=de-DE&paymentSessionOrData={0}",
                        HttpUtility.UrlEncode("{ \"partner : \"cart\" }")))); 

            // Assert
            // Despite the json being ivalid, other context that could be extracted should be extracted
            Assert.IsTrue(actualFlightContext.ContainsKey("language"));
            Assert.AreEqual(actualFlightContext["language"], "de-DE");
        }
    }
}
