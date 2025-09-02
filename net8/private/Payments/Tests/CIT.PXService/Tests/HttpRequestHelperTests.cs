// <copyright file="HttpRequestHelper.cs" company="Microsoft">Copyright (c) Microsoft 2019. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXService;
    using Microsoft.Commerce.Payments.PXService.Model.PXInternal;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class HttpRequestHelperTests : TestBase
    {
        [TestMethod]
        public void HasTestScenariosExist()
        {
            const string ScenarioE2E = "px-service-psd2-e2e-emulator";
            const string HeadlessHdrPrefix = "px.";

            HashSet<string> scenarios = new HashSet<string> { ScenarioE2E, HeadlessHdrPrefix };
            var testCtx = new Microsoft.Commerce.Payments.Common.Transaction.TestContext();
            testCtx.Scenarios = ScenarioE2E + "," + "px.service";

            bool exist = HttpRequestHelper.HasE2EPSD2TestScenarios(testCtx);
            Assert.AreEqual(exist, true);

            scenarios.Clear();
            testCtx.Scenarios = "random-hdr";
            exist = HttpRequestHelper.HasAnyPSD2TestScenarios(testCtx);
            Assert.AreEqual(exist, false);
        }

        [DataRow("px-service-3ds1-test-emulator", true)]
        [DataRow("some-random-header", false)]
        [DataTestMethod]
        public void HasIndia3DS1TestScenarioExists(string testHeader, bool expectedReturned)
        {
            var testCtx = new Microsoft.Commerce.Payments.Common.Transaction.TestContext
            {
                Scenarios = testHeader + ",px.service"
            };

            bool hasHeader = HttpRequestHelper.HasThreeDSOneTestScenario(testCtx);
            Assert.AreEqual(hasHeader, expectedReturned);
        }

        [DataRow("px-service-3ds1-test-emulator-challenge-success", true)]
        [DataRow("some-random-header", false)]
        [DataTestMethod]
        public void HasIndia3DS1TestScenarioWithSuccessExists(string testHeader, bool expectedReturned)
        {
            var testCtx = new Microsoft.Commerce.Payments.Common.Transaction.TestContext
            {
                Scenarios = testHeader + ",px.service"
            };

            bool hasHeader = HttpRequestHelper.HasThreeDSOneTestScenarioWithSuccess(testCtx);
            Assert.AreEqual(hasHeader, expectedReturned);
        }

        [DataRow("px-service-3ds1-test-emulator-challenge-failed", true)]
        [DataRow("some-random-header", false)]
        [DataTestMethod]
        public void HasIndia3DS1TestScenarioWithFailureExists(string testHeader, bool expectedReturned)
        {
            var testCtx = new Microsoft.Commerce.Payments.Common.Transaction.TestContext
            {
                Scenarios = testHeader + ",px.service"
            };

            bool hasHeader = HttpRequestHelper.HasThreeDSOneTestScenarioWithFailure(testCtx);
            Assert.AreEqual(hasHeader, expectedReturned);
        }

        [DataRow("{\"paymentSessionData\": {\"language\": \"en-US\",\"partner\": \"amcxbox1\",\"country\": \"us\"}}", "amcxbox1", "en-US", "us")]
        [DataRow("{\"paymentSessionData\": {\"language\": \"en-US\"}}", null, "en-US", null)]
        [DataRow("{\"paymentSessionData\": {\"partner\": \"amcxbox1\",\"country\": \"us\"}}", "amcxbox1", null, "us")]
        [DataRow("{\"paymentSessionData\": {\"partner\": \"\",\"country\": \"us\"}}", "", null, "us")] // partner is empty
        [DataRow(null, null, null, null)] // invalid json
        [DataRow("\"paymentSessionData\": {\"partner\": null,\"country\": \"us\"}}", null, null, null)] // invalid json
        [DataRow("{\"paymentSessionData\": {\"partner\": null,\"country\": \"us\"}}", null, null, "us")] // partner is null
        [DataRow("{\"paymentSessionData\": {\"partner\": 20.2,\"country\": \"us\"}}", "20.2", null, "us")] // partner is a number
        [DataRow("{\"paymentSessionData\": {\"partner\": {\"foo\": \"bar\"},\"country\": \"us\"}}", null, null, "us")] // partner is an object
        [DataRow("{\"paymentSessionData\": {\"partner\": [\"foo\", \"bar\"],\"country\": \"us\"}}", null, null, "us")] // partner is an array
        [TestMethod]
        public async Task TestTryGetPayloadPropertyValue(string jsonPayload, string expectedPartner, string expectedLanguage, string expectedCountry)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            if (jsonPayload != null)
            {
                request.Content = new StringContent(jsonPayload);
            }

            string value = await request.TryGetPayloadPropertyValue("paymentSessionData.language");
            Assert.AreEqual(expectedLanguage, value);
            value = await request.TryGetPayloadPropertyValue("paymentSessionData.country");
            Assert.AreEqual(expectedCountry, value);
            value = await request.TryGetPayloadPropertyValue("paymentSessionData.partner");
            Assert.AreEqual(expectedPartner, value);
        }

        [TestMethod]
        public void IsEncoded_HeaderPresent_ReturnsTrue()
        {
            // Arrange
            var request = new HttpRequestMessage();
            request.Headers.Add("x-ms-clientcontext-encoding", "base64");

            // Act
            bool result = HttpRequestHelper.IsEncoded(request);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsEncoded_HeaderNotPresent_ReturnsFalse()
        {
            // Arrange
            var request = new HttpRequestMessage();

            // Act
            bool result = HttpRequestHelper.IsEncoded(request);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GetUserAgent_WithValidHeader_ReturnsUserAgent()
        {
            // Arrange
            var request = new HttpRequestMessage();
            request.Headers.Add("x-ms-deviceinfo", "userAgent=TestUserAgent,otherKey=otherValue");

            // Act
            string result = HttpRequestHelper.GetUserAgent(request);

            // Assert
            Assert.AreEqual("TestUserAgent", result);
        }

        [TestMethod]
        public void GetUserAgent_WithInvalidHeader_ReturnsNull()
        {
            // Arrange
            var request = new HttpRequestMessage();
            request.Headers.Add("x-ms-deviceinfo", "invalidHeaderNoUserAgent");

            // Act
            string result = HttpRequestHelper.GetUserAgent(request);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetUserAgentDecoded_WithEncodedUserAgent_ReturnsDecodedUserAgent()
        {
            // Arrange
            var userAgentHeaderValue = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:122.0) Gecko/20100101 Firefox/122.0";

            byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(userAgentHeaderValue);
            string encodedText = System.Convert.ToBase64String(plainTextBytes);

            // actual user Agent: "ipAddress=MTc2LjQ1LjkyLjE=,userAgent=TW96aWxsYS81LjAgKFdpbmRvd3MgTlQgMTAuMDsgV2luNjQ7IHg2NDsgcnY6MTIyLjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvMTIyLjA=,deviceId=MASKED"
            string userAgent = "ipAddress=MTc2LjQ1LjkyLjE=,userAgent=" + encodedText + ",deviceId=MASKED";

            var request = new HttpRequestMessage();
            request.Headers.Add("x-ms-clientcontext-encoding", "base64");
            request.Headers.Add("x-ms-deviceinfo", userAgent);

            // Act
            string result = HttpRequestHelper.GetUserAgentDecoded(request);

            // Assert
            Assert.AreEqual(userAgentHeaderValue, result);
        }

        [TestMethod]
        public void GetUserAgentDecoded_WithoutEncodedUserAgent_ReturnsOriginalUserAgent()
        {
            // Arrange
            var request = new HttpRequestMessage();
            request.Headers.Add("x-ms-deviceinfo", "ipAddress=testip,userAgent=Plain Text UserAgent,DeviceId=Masked");

            // Act
            string result = HttpRequestHelper.GetUserAgentDecoded(request);

            // Assert
            Assert.AreEqual("Plain Text UserAgent", result);
        }

        [DataRow("android", "edge mobile", "Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Mobile Safari/537.36 EdgA/125.0.0.0")]
        [DataRow("android", "chrome mobile", "Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Mobile Safari/537.36")]
        [DataRow("android", "firefox mobile", "Mozilla/5.0 (Android 13; Mobile; rv:124.0) Gecko/124.0 Firefox/124.0")]
        [DataRow("ios", "edge mobile", "Mozilla/5.0 (iPhone; CPU iPhone OS 17_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) EdgiOS/125.0.2535.72 Version/17.0 Mobile/15E148 Safari/604.1")]
        [DataRow("ios", "chrome mobile ios", "Mozilla /5.0 (iPhone; CPU iPhone OS 17_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) CriOS/126.0.6478.54 Mobile/15E148 Safari/604.1")]
        [DataRow("ios", "mobile safari", "Mozilla /5.0 (iPhone; CPU iPhone OS 17_1_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1.1 Mobile/15E148 Safari/604.1")]
        [DataRow("ios", "firefox ios", "Mozilla/5.0 (iPhone; CPU iPhone OS 17_5_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) FxiOS/128.0 Mobile/15E148 Safari/605.1.15")]
        [DataRow("mac os x", "edge", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML%2C like Gecko) Chrome/125.0.0.0 Safari/537.36 Edg/125.0.0.0")]
        [DataRow("mac os x", "safari", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.4.1 Safari/605.1.15")]
        [DataRow("mac os x", "chrome", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.3")]
        [DataRow("mac os x", "firefox", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:128.0) Gecko/20100101 Firefox/128.0")]

        // ipad start from here.
        // ipad firefox only can provide userAgent looks like mac os x safari 16.0.
        [DataRow("mac os x", "safari", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.0 Safari/605.1.15")]
        [DataRow("ios", "mobile safari", "Mozilla/5.0 (iPad; CPU OS 17_5_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.5 Mobile/15E148 Safari/604.1")]
        [DataRow("ios", "chrome mobile ios", "Mozilla/5.0 (iPad; CPU OS 17_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) CriOS/126.0.6478.153 Mobile/15E148 Safari/604.1")]
        [DataRow("windows", "edge", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; WebView/3.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36 Edge/18.22631")]
        [DataRow("windows", "edge", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36 Edg/124.0.0.0")]
        [DataRow("windows", "edge", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36 Edge/18.22631")]
        [DataRow("windows", "chrome", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36")]
        [DataRow("windows", "firefox", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:127.0) Gecko/20100101 Firefox/127.0")]
        [DataTestMethod]
        public void GetUserAgentBrowser_ReturnsExpectedValues(string expectedOS, string expectedBrowser, string userAgent)
        {
            // Arrange
            byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(userAgent);
            string encodedText = System.Convert.ToBase64String(plainTextBytes);

            // actual user Agent: "ipAddress=MTc2LjQ1LjkyLjE=,userAgent=TW96aWxsYS81LjAgKFdpbmRvd3MgTlQgMTAuMDsgV2luNjQ7IHg2NDsgcnY6MTIyLjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvMTIyLjA=,deviceId=MASKED"
            string deviceInfo = "ipAddress=MTc2LjQ1LjkyLjE=,userAgent=" + encodedText + ",deviceId=MASKED";

            var request = new HttpRequestMessage();
            request.Headers.Add("x-ms-clientcontext-encoding", "base64");
            request.Headers.Add("x-ms-deviceinfo", deviceInfo);

            // Act
            var browser = HttpRequestHelper.GetBrowser(request);
            var deviceFamily = HttpRequestHelper.GetOSFamily(request);

            // Assert
            Assert.AreEqual(expectedBrowser, browser);
            Assert.AreEqual(expectedOS, deviceFamily);
        }

        [DataRow("Mozilla/5.0 (iPhone; CPU iPhone OS 17_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) EdgiOS/125.0.2535.72 Version/17.0 Mobile/15E148 Safari/604.1", "125.0.2535")]
        [DataRow("Mozilla/5.0 (Windows NT 10.0; Win64; x64; MSAppHost/3.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36 Edge/18.19045", "18.19045.0")]
        [DataRow("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36", "125.0.0")]
        [DataRow("Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Mobile Safari/537.36", "125.0.0")]
        [DataRow("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.4.1 Safari/605.1.15", "17.4.1")]
        [DataRow("Mozilla/5.0 (iPhone; CPU iPhone OS 17_1_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1.1 Mobile/15E148 Safari/604.1", "17.1.1")]
        [DataTestMethod]
        public void GetUserAgentVersion(string userAgent, string expected)
        {
            // Arrange
            byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(userAgent);
            string encodedText = System.Convert.ToBase64String(plainTextBytes);

            // actual user Agent: "ipAddress=MTc2LjQ1LjkyLjE=,userAgent=TW96aWxsYS81LjAgKFdpbmRvd3MgTlQgMTAuMDsgV2luNjQ7IHg2NDsgcnY6MTIyLjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvMTIyLjA=,deviceId=MASKED"
            string deviceInfo = "ipAddress=MTc2LjQ1LjkyLjE=,userAgent=" + encodedText + ",deviceId=MASKED";

            var request = new HttpRequestMessage();
            request.Headers.Add("x-ms-clientcontext-encoding", "base64");
            request.Headers.Add("x-ms-deviceinfo", deviceInfo);

            // Act
            var version = HttpRequestHelper.GetBrowserVer(request);

            // Assert
            Assert.AreEqual(expected, version);
        }

        [DataRow("Mozilla/5.0 (iPhone; CPU iPhone OS 17_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) EdgiOS/125.0.2535.72 Version/17.0 Mobile/15E148 Safari/604.1", 125)]
        [DataRow("Mozilla/5.0 (Windows NT 10.0; Win64; x64; MSAppHost/3.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36 Edge/18.19045", 18)]
        [DataRow("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36", 125)]
        [DataRow("Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Mobile Safari/537.36", 125)]
        [DataRow("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.4.1 Safari/605.1.15", 17)]
        [DataRow("Mozilla/5.0 (iPhone; CPU iPhone OS 17_1_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1.1 Mobile/15E148 Safari/604.1", 17)]
        [DataRow("Mozilla/5.0 (iPhone; CPU iPhone OS 17_1_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/ Mobile/15E148 Safari/604.1", 0)]
        [DataTestMethod]
        public void GetUserAgentMajorVersion(string userAgent, int expected)
        {
            // Arrange
            byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(userAgent);
            string encodedText = System.Convert.ToBase64String(plainTextBytes);

            // actual user Agent: "ipAddress=MTc2LjQ1LjkyLjE=,userAgent=TW96aWxsYS81LjAgKFdpbmRvd3MgTlQgMTAuMDsgV2luNjQ7IHg2NDsgcnY6MTIyLjApIEdlY2tvLzIwMTAwMTAxIEZpcmVmb3gvMTIyLjA=,deviceId=MASKED"
            string deviceInfo = "ipAddress=MTc2LjQ1LjkyLjE=,userAgent=" + encodedText + ",deviceId=MASKED";

            var request = new HttpRequestMessage();
            request.Headers.Add("x-ms-clientcontext-encoding", "base64");
            request.Headers.Add("x-ms-deviceinfo", deviceInfo);

            // Act
            var version = HttpRequestHelper.GetBrowserMajorVer(request);

            // Assert
            Assert.AreEqual(expected, version);
        }
    }
}