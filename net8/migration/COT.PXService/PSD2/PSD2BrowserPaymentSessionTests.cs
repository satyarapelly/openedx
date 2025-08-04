// <copyright file="PSD2BrowserPaymentSessionTests.cs" company="Microsoft">Copyright (c) Microsoft 2019-2020. All rights reserved.</copyright>

namespace COT.PXService.PSD2
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
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using System.Text.RegularExpressions;
    using System.Text;
    using System.Threading.Tasks;

    [TestClass]
    public class PSD2BrowserPaymentSessionTests : TestBase
    {
        public const string UrlTemplateCreatePaymentSession = "v7.0/{0}/paymentSessionDescriptions?paymentSessionData={1}";
        public const string UrlTemplateHandlePaymentChallenge = "v7.0/{0}/challengeDescriptions?timezoneOffset=01&paymentSessionOrData={1}";

        public TestSettings TestSettings { get; private set; }

        [TestInitialize]
        public void TestInitialize()
        {
            this.Initialize();
            this.TestSettings = TestSettings.CreateInstance();
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void CreatePaymentSessionMOTO()
        {
            var paymentSessionDataMoto = new PaymentSessionData()
            {
                Language = "en",
                Amount = 205.0m,
                Currency = "EUR",
                Partner = this.TestSettings.Partner,
                Country = "GB",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = true,
            };

            AccountInfo testAcc = this.TestSettings.GetPSD2Account(true);

            //// validate the create payment session API
            CreatePaymentSession(testAcc, paymentSessionDataMoto, true, false, PaymentChallengeStatus.ByPassed, true);

            //// validate the create and authenticate payment session API
            HandlePaymentChallenge(testAcc, paymentSessionDataMoto, true, false, PaymentChallengeStatus.ByPassed, false, true).Wait();
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void CreatePaymentSessionNonPSD2Market()
        {
            var paymentSessionDataMoto = new PaymentSessionData()
            {
                Language = "en",
                Amount = 215.0m,
                Currency = "USD",
                Partner = this.TestSettings.Partner,
                Country = "US",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
            };

            AccountInfo testAcc = this.TestSettings.GetPSD2Account(false);

            //// validate the create payment session API
            CreatePaymentSession(testAcc, paymentSessionDataMoto, false, false, PaymentChallengeStatus.NotApplicable, false);

            //// validate the create and authenticate payment session API
            HandlePaymentChallenge(testAcc, paymentSessionDataMoto, false, false, PaymentChallengeStatus.NotApplicable, false, false).Wait();
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void CreatePaymentSessionChallengeRequired()
        {
            var paymentSessionDataMoto = new PaymentSessionData()
            {
                Language = "en",
                Amount = 201.0m,
                Currency = "GBP",
                Partner = this.TestSettings.Partner,
                Country = "GB",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
            };

            AccountInfo testAcc = this.TestSettings.GetPSD2Account(true);

            //// validate the create payment session API
            CreatePaymentSession(testAcc, paymentSessionDataMoto, true, true, PaymentChallengeStatus.Unknown, false);
        }

        public PaymentSession CreatePaymentSession(
             AccountInfo testAcc,
             PaymentSessionData data,
             bool includeTestHeader,
             bool expectedIsChallengeRequired = true,
             PaymentChallengeStatus expectedStatus = PaymentChallengeStatus.Unknown,
             bool isMoto = false)
        {
            Common.TestContext tc = includeTestHeader ?
                new Common.TestContext(
                    contact: "px.azure.cot",
                    retention: DateTime.MaxValue,
                    scenarios: "px-service-psd2-e2e-emulator")
                : null;

            data.PaymentInstrumentId = testAcc.CreditCardPiid;

            HttpStatusCode code = HttpStatusCode.Unused;
            dynamic body = null;
            var headers = new Dictionary<string, string>();
            if (isMoto)
            {
                headers.Add("x-ms-ismoto", "true");
            }

            this.ExecuteRequest(
                string.Format(
                    UrlTemplateCreatePaymentSession,
                    testAcc.AccountId,
                    JsonConvert.SerializeObject(data)),
                HttpMethod.Get,
                tc,
                null,
                headers,
                (responseCode, responseBody) =>
                {
                    code = responseCode;
                    body = responseBody;
                });

            Assert.AreEqual(HttpStatusCode.OK, code);

            var pidlRes = RetrievePaymentSessionFromPIDLResource(body);
            var resp = JsonConvert.DeserializeObject<PaymentSession>(pidlRes.ClientAction.Context.ToString());

            Assert.IsNotNull(resp?.Id);
            Assert.AreEqual(expectedIsChallengeRequired, resp?.IsChallengeRequired);
            Assert.AreEqual(expectedStatus, resp?.ChallengeStatus);

            return resp;
        }

        public async Task<PaymentSession> HandlePaymentChallenge(
             AccountInfo testAcc,
             PaymentSessionData data,
             bool includeTestHeader,
             bool expectedIsChallengeRequired = true,
             PaymentChallengeStatus expectedStatus = PaymentChallengeStatus.Unknown,
             bool frictionLessFlow = false,
             bool isMoto = false)
        {
            data.PaymentInstrumentAccountId = testAcc.AccountId;
            data.PaymentInstrumentId = testAcc.CreditCardPiid;

            Common.TestContext tc = includeTestHeader ?
                new Common.TestContext(
                    contact: "px.azure.cot",
                    retention: DateTime.MaxValue,
                    scenarios: "px-service-psd2-e2e-emulator")
                : null;

            var headers = new Dictionary<string, string>();
            if (isMoto)
            {
                headers.Add("x-ms-ismoto", "true");
            }

            dynamic body = null;
            HttpStatusCode code = HttpStatusCode.Unused;
            this.ExecuteRequest(
                string.Format(
                    UrlTemplateHandlePaymentChallenge,
                    testAcc.AccountId,
                    JsonConvert.SerializeObject(data)),
                HttpMethod.Get,
                tc,
                null,
                headers,
                (responseCode, responseBody) =>
                {
                    code = responseCode;
                    body = responseBody;
                });

            Assert.AreEqual(HttpStatusCode.OK, code);

            if (isMoto)
            {
                var pidlRes = RetrievePaymentSessionFromPIDLResource(body);
                var resp = JsonConvert.DeserializeObject<PaymentSession>(pidlRes.ClientAction.Context.ToString());

                Assert.IsNotNull(resp?.Id);
                Assert.AreEqual(expectedIsChallengeRequired, resp?.IsChallengeRequired);
                Assert.AreEqual(expectedStatus, resp?.ChallengeStatus);
                return resp;
            }
            else if (!expectedIsChallengeRequired)
            {
                var pidlRes = RetrievePaymentSessionFromPIDLResource(body);
                var resp = JsonConvert.DeserializeObject<PaymentSession>(pidlRes.ClientAction.Context.ToString());

                Assert.IsNotNull(resp?.Id);
                Assert.AreEqual(expectedIsChallengeRequired, resp?.IsChallengeRequired);
                Assert.AreEqual(expectedStatus, resp?.ChallengeStatus);
                return resp;
            }
            else
            {
                // check if the finger print is requested
                var respArray = body as JArray;
                if (respArray != null && respArray[0]["identity"]["description_type"].Value<string>() == "fingerprintIFrame")
                {
                    return await ProcessFingerprintStep(expectedIsChallengeRequired, expectedStatus, frictionLessFlow, tc, headers, respArray);
                }
                else if (frictionLessFlow)
                {
                    return ParsePXFinalResponse(body, expectedIsChallengeRequired, expectedStatus);
                }
                else
                {
                    return ProcessChallengeStep(expectedIsChallengeRequired, expectedStatus, tc, headers, body);
                }
            }
        }

        private async Task<PaymentSession> ProcessFingerprintStep(
            bool expectedIsChallengeRequired, 
            PaymentChallengeStatus expectedStatus, 
            bool frictionLessFlow, 
            Common.TestContext tc, 
            Dictionary<string, string> headers, 
            JArray respArray)
        {
            var jsonResp = respArray[0];

            string displayContent = string.Empty;
            var tokens = jsonResp.SelectTokens("$..messageTimeoutClientAction..displayContent");
            foreach (JToken token in tokens)
            {
                displayContent = token.Value<string>();
                break;
            }

            //// go to the next step when the fingerprint is requested. It is optional step for the ACS Emulator
            //// just let the PXService know that the fingerprint is completed
            Regex pattern = new Regex("form action=\"(.*?)\" method=\"post\"><input type=\"hidden\" name=\"threeDSMethodData\" value=\"(.*)\" /><input type=\"hidden\" name=\"fingerPrintTimedout\"");
            Match match = pattern.Match(displayContent);

            Assert.AreEqual(true, match.Success);
            Assert.AreEqual(3, match.Groups.Count);
            string fingerPrintCompletedUrl = match.Groups[1].Value;
            fingerPrintCompletedUrl = fingerPrintCompletedUrl.Replace("https://pifd.cp.microsoft-int.com/V6.0/", "v7.0/");
            fingerPrintCompletedUrl = fingerPrintCompletedUrl.Replace("https://paymentinstruments-int.mp.microsoft.com/V6.0/", "v7.0/");
            fingerPrintCompletedUrl = fingerPrintCompletedUrl.Replace("https://paymentinstruments.mp.microsoft.com/V6.0/", "v7.0/");

            string methodData = match.Groups[2].Value;
            string testHeader = ParseTestHeader(displayContent, true);
            string payload = string.Format("threeDSMethodData={0}&x-ms-test={1}", methodData, testHeader);

            //// notify the px that the fingerprint is completed
            string body = null;
            var code = HttpStatusCode.Unused;
            this.ExecuteRequest(
                fingerPrintCompletedUrl,
                HttpMethod.Post,
                tc,
                payload,
                headers,
                (responseCode, responseBody) =>
                {
                    code = responseCode;
                    body = responseBody;
                },
                contentType: Constants.HeaderValues.FormContent);

            Assert.AreEqual(HttpStatusCode.OK, code);

            if (frictionLessFlow)
            {
                return ParsePXFinalResponse(body, expectedIsChallengeRequired, expectedStatus);
            }
            else
            {
                return await ProcessChallengeStep(expectedIsChallengeRequired, expectedStatus, tc, headers, body);
            }
        }

        private static string ParseTestHeader(string displayContent, bool assert)
        {
            // read the test header
            Regex pattern = new Regex("<input type=\"hidden\" name=\"x-ms-test\" value=\"(.*?)\" />");
            var match = pattern.Match(displayContent);
            if (assert)
            {
                Assert.AreEqual(assert, match.Success);
                Assert.AreEqual(2, match.Groups.Count);
                return match.Groups[1].Value;
            }

            return string.Empty;
        }

        private async Task<PaymentSession> ProcessChallengeStep(bool expectedIsChallengeRequired, PaymentChallengeStatus expectedStatus, Common.TestContext tc, Dictionary<string, string> headers, string responseContent)
        {
            // challenge flow logic
            Regex pattern = new Regex(@"<html><script>window.parent.postMessage\((.*),\s.*\);</script><body/></html>");
            Match match = pattern.Match(responseContent);
            Assert.IsTrue(match.Success);
            Assert.AreEqual(2, match.Groups.Count);

            var retData = match.Groups[1].Value;
            var retObj = JsonConvert.DeserializeObject<JValue>(retData);
            var pidl = JsonConvert.DeserializeObject<JObject>(retObj.Value<string>());

            var pidlType = pidl["type"].Value<string>();
            Assert.AreEqual("Pidl", pidlType);
            var descriptionType = pidl["context"][0]["identity"]["description_type"].Value<string>();
            Assert.AreEqual("threeDSChallengeIFrame", descriptionType);
            var displayContent = pidl["context"][0]["displayDescription"][0]["members"][0]["displayContent"].Value<string>();
            Assert.IsNotNull(displayContent);

            // read the threeDSSessionData
            pattern = new Regex("<input type=\"hidden\" name=\"threeDSSessionData\" value=\"(.*?)\" />");
            match = pattern.Match(displayContent);
            Assert.IsTrue(match.Success);
            Assert.AreEqual(2, match.Groups.Count);
            var sessionData = AcsServiceClient.DecodeBase64Url(match.Groups[1].Value);
            var sessionDataObj = JsonConvert.DeserializeObject<JObject>(sessionData);
            Assert.IsFalse(string.IsNullOrWhiteSpace(sessionDataObj["threeDSServerTransID"].Value<string>()));

            var threeDSServerTransID = sessionDataObj["threeDSServerTransID"].Value<string>();
            AcsStatusPayload acsPayload = CreateACSPayload(threeDSServerTransID, expectedStatus);

            string testHeader = ParseTestHeader(displayContent, false);

            // read the ACS url
            pattern = new Regex("form action=\"(.*?)\" method=\"post\"><input type=\"hidden\" name=\"creq\"");
            match = pattern.Match(displayContent);
            Assert.IsTrue(match.Success);
            Assert.AreEqual(2, match.Groups.Count);
            string acsUrl = match.Groups[1].Value;

            // notify the ACS that the challenge is completed
            AcsServiceClient acsClient = new AcsServiceClient();
            await acsClient.SendChallengeStatusData(acsUrl, acsPayload);

            var sessionId = pidl["actionId"].Value<string>();
            var challengeCompletedUrl = string.Format("v7.0/paymentSessions/{0}/NotifyThreeDSChallengeCompleted", sessionId);

            // cresData is NOT used by the server so sending the sessionData as cres
            var cresData = sessionData;
            var payload = string.Format("threeDSSessionData={0}&x-ms-test={1}&cres={2}", sessionData, testHeader, cresData);

            //// notify the px that the challenge is completed
            responseContent = null;
            var code = HttpStatusCode.Unused;
            this.ExecuteRequest(
                challengeCompletedUrl,
                HttpMethod.Post,
                tc,
                payload,
                headers,
                (responseCode, responseBody) =>
                {
                    code = responseCode;
                    responseContent = responseBody;
                },
                contentType: Constants.HeaderValues.FormContent);

            Assert.AreEqual(HttpStatusCode.OK, code);

            return ParsePXFinalResponse(responseContent, expectedIsChallengeRequired, expectedStatus);
        }

        private static AcsStatusPayload CreateACSPayload(string threeDSServerTransID, PaymentChallengeStatus expectedStatus)
        {
            var payload = new AcsStatusPayload
            {
                ThreeDSServerTransID = threeDSServerTransID,
                TransStatus = "Y"
            };

            switch (expectedStatus)
            {
                case PaymentChallengeStatus.Failed:
                    payload.TransStatus = "N";
                    payload.TransStatusReason = "10";
                    break;

                case PaymentChallengeStatus.Cancelled:
                    payload.TransStatus = "N";
                    payload.TransStatusReason = "01";
                    payload.ChallengeCancel = "01";
                    break;

                case PaymentChallengeStatus.TimedOut:
                    payload.TransStatus = "N";
                    payload.TransStatusReason = "14";
                    break;

                default:
                    payload.TransStatus = "Y";
                    break;
            }

            return payload;
        }

        private static PaymentSession ParsePXFinalResponse(string content, bool expectedIsChallengeRequired, PaymentChallengeStatus expectedStatus)
        {
            // first step to check if the frictionless 
            Regex pattern = new Regex(@"<html><script>window.parent.postMessage\((.*),\s.*\);</script><body/></html>");
            Match match = pattern.Match(content);
            Assert.IsTrue(match.Success);
            Assert.AreEqual(2, match.Groups.Count);

            var retData = match.Groups[1].Value;
            var retObj = JsonConvert.DeserializeObject<JValue>(retData);
            var clientAction = JsonConvert.DeserializeObject<JObject>(retObj.Value<string>());
            if (expectedStatus == PaymentChallengeStatus.Failed || expectedStatus == PaymentChallengeStatus.TimedOut)
            {
                Assert.AreEqual(expectedStatus.ToString(), clientAction["context"]["ErrorCode"].Value<string>());
                return null;
            }
            else
            {
                var session = JsonConvert.DeserializeObject<PaymentSession>(clientAction.SelectToken("context").ToString());
                Assert.IsNotNull(session?.Id);
                Assert.AreEqual(expectedIsChallengeRequired, session?.IsChallengeRequired);
                Assert.AreEqual(expectedStatus, session?.ChallengeStatus);

                return session;
            }
        }

        private static PIDLResource RetrievePaymentSessionFromPIDLResource(dynamic body)
        {
            var jsonResponse = body as JArray;
            var pidlResource = JsonConvert.DeserializeObject<PIDLResource>(JsonConvert.SerializeObject(jsonResponse[0]));
            return pidlResource;
        }
    }
}
