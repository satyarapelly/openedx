// <copyright file="PSD2AppPaymentChallengeTests.cs" company="Microsoft">Copyright (c) Microsoft 2019-2020. All rights reserved.</copyright>

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
    using PaymentChallenge = Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge;
    using PayerAuth = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService;
    using Microsoft.Commerce.Payments.PXService.Model.ThreeDSExternalService;
    using System.Linq;
    using System.Threading.Tasks;

    [TestClass]
    public class PSD2AppPaymentChallengeTests : TestBase
    {
        public TestSettings TestSettings { get; private set; }

        public PSD2AppPaymentSessionTests PaymentSessionTests { get; private set; }

        public AcsServiceClient ACSClient { get; private set; }

        [TestInitialize]
        public void TestInitialize()
        {
            this.Initialize();
            this.TestSettings = TestSettings.CreateInstance();
            this.PaymentSessionTests = new PSD2AppPaymentSessionTests();
            this.PaymentSessionTests.TestInitialize();
            this.ACSClient = new AcsServiceClient();
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void ChallengeFrictionLess()
        {
            var data = new PaymentSessionData()
            {
                Language = "en",
                Amount = 110.0m,
                Currency = "EUR",
                Partner = this.TestSettings.Partner,
                Country = "GB",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
            };

            AccountInfo testAcc = this.TestSettings.PSD2EuroAccountFrictionLess;
            this.PaymentSessionTests.CreateAndAuthenticatePaymentSession(testAcc, data, true, true, PaymentChallengeStatus.Succeeded, false);

            string sessionId = this.PaymentSessionTests.CreatePaymentSession(testAcc, data, true, true, PaymentChallengeStatus.Unknown, false);
            Authenticate(testAcc, sessionId, true, PaymentChallengeStatus.Succeeded);
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void ChallengeSucceeded()
        {
            var data = new PaymentSessionData()
            {
                Language = "en",
                Amount = 100.0m,
                Currency = "EUR",
                Partner = this.TestSettings.Partner,
                Country = "GB",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
            };

            AccountInfo testAcc = this.TestSettings.PSD2EuroAccount;
            var authResp = this.PaymentSessionTests.CreateAndAuthenticatePaymentSession(testAcc, data, true, true, PaymentChallengeStatus.Unknown, false);

            var acsPayload = new AcsStatusPayload
            {
                ThreeDSServerTransID = authResp.AuthenticateResponse.ThreeDSServerTransactionID,
                TransStatus = "Y"
            };
            ACSClient.SendChallengeStatus(authResp.AuthenticateResponse.AcsSignedContent, acsPayload).Wait();

            NotifyThreeDSChallengeCompleted(testAcc, authResp.PaymentSession.Id, authResp.AuthenticateResponse.ThreeDSServerTransactionID, true, PaymentChallengeStatus.Succeeded);
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void ChallengeSucceededMandate()
        {
            var data = new PaymentSessionData()
            {
                Language = "en",
                Amount = 0m,
                Currency = "EUR",
                Partner = this.TestSettings.Partner,
                Country = "GB",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.RecurringTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
            };

            AccountInfo testAcc = this.TestSettings.PSD2EuroAccount;
            var authResp = this.PaymentSessionTests.CreateAndAuthenticatePaymentSession(testAcc, data, true, true, PaymentChallengeStatus.Unknown, false);

            var acsPayload = new AcsStatusPayload
            {
                ThreeDSServerTransID = authResp.AuthenticateResponse.ThreeDSServerTransactionID,
                TransStatus = "Y"
            };
            ACSClient.SendChallengeStatus(authResp.AuthenticateResponse.AcsSignedContent, acsPayload).Wait();

            NotifyThreeDSChallengeCompleted(testAcc, authResp.PaymentSession.Id, authResp.AuthenticateResponse.ThreeDSServerTransactionID, true, PaymentChallengeStatus.Succeeded);
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void ChallengeSucceededRecurring()
        {
            var data = new PaymentSessionData()
            {
                Language = "en",
                Amount = 130m,
                Currency = "EUR",
                Partner = this.TestSettings.Partner,
                Country = "GB",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.RecurringTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
            };

            AccountInfo testAcc = this.TestSettings.PSD2EuroAccount;
            var authResp = this.PaymentSessionTests.CreateAndAuthenticatePaymentSession(testAcc, data, true, true, PaymentChallengeStatus.Unknown, false);

            var acsPayload = new AcsStatusPayload
            {
                ThreeDSServerTransID = authResp.AuthenticateResponse.ThreeDSServerTransactionID,
                TransStatus = "Y"
            };
            ACSClient.SendChallengeStatus(authResp.AuthenticateResponse.AcsSignedContent, acsPayload).Wait();

            NotifyThreeDSChallengeCompleted(testAcc, authResp.PaymentSession.Id, authResp.AuthenticateResponse.ThreeDSServerTransactionID, true, PaymentChallengeStatus.Succeeded);
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void ChallengeSucceededPreorder()
        {
            var data = new PaymentSessionData()
            {
                Language = "en",
                Amount = 140m,
                Currency = "EUR",
                Partner = this.TestSettings.Partner,
                Country = "GB",
                HasPreOrder = true,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
            };

            AccountInfo testAcc = this.TestSettings.PSD2EuroAccount;
            var authResp = this.PaymentSessionTests.CreateAndAuthenticatePaymentSession(testAcc, data, true, true, PaymentChallengeStatus.Unknown, false);

            var acsPayload = new AcsStatusPayload
            {
                ThreeDSServerTransID = authResp.AuthenticateResponse.ThreeDSServerTransactionID,
                TransStatus = "Y"
            };
            ACSClient.SendChallengeStatus(authResp.AuthenticateResponse.AcsSignedContent, acsPayload).Wait();

            NotifyThreeDSChallengeCompleted(testAcc, authResp.PaymentSession.Id, authResp.AuthenticateResponse.ThreeDSServerTransactionID, true, PaymentChallengeStatus.Succeeded);
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void ChallengeFailed()
        {
            var data = new PaymentSessionData()
            {
                Language = "en",
                Amount = 110.0m,
                Currency = "EUR",
                Partner = this.TestSettings.Partner,
                Country = "GB",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
            };

            AccountInfo testAcc = this.TestSettings.PSD2EuroAccount;
            var authResp = this.PaymentSessionTests.CreateAndAuthenticatePaymentSession(testAcc, data, true, true, PaymentChallengeStatus.Unknown, false);

            var acsPayload = new AcsStatusPayload
            {
                ThreeDSServerTransID = authResp.AuthenticateResponse.ThreeDSServerTransactionID,
                TransStatus = "N",
                TransStatusReason = "10"
            };
            ACSClient.SendChallengeStatus(authResp.AuthenticateResponse.AcsSignedContent, acsPayload).Wait();

            NotifyThreeDSChallengeCompleted(testAcc, authResp.PaymentSession.Id, authResp.AuthenticateResponse.ThreeDSServerTransactionID, true, PaymentChallengeStatus.Failed);
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void ChallengeCancelled()
        {
            var data = new PaymentSessionData()
            {
                Language = "en",
                Amount = 120.0m,
                Currency = "EUR",
                Partner = this.TestSettings.Partner,
                Country = "GB",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
            };

            AccountInfo testAcc = this.TestSettings.PSD2EuroAccount;
            var authResp = this.PaymentSessionTests.CreateAndAuthenticatePaymentSession(testAcc, data, true, true, PaymentChallengeStatus.Unknown, false);

            var acsPayload = new AcsStatusPayload
            {
                ThreeDSServerTransID = authResp.AuthenticateResponse.ThreeDSServerTransactionID,
                TransStatus = "N",
                TransStatusReason = "01",
                ChallengeCancel = "01"
            };
            ACSClient.SendChallengeStatus(authResp.AuthenticateResponse.AcsSignedContent, acsPayload).Wait();

            NotifyThreeDSChallengeCompleted(testAcc, authResp.PaymentSession.Id, authResp.AuthenticateResponse.ThreeDSServerTransactionID, true, PaymentChallengeStatus.Cancelled);
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]

        public void ChallengeTimedOut()
        {
            var data = new PaymentSessionData()
            {
                Language = "en",
                Amount = 160.0m,
                Currency = "EUR",
                Partner = this.TestSettings.Partner,
                Country = "GB",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
            };

            AccountInfo testAcc = this.TestSettings.PSD2EuroAccount;
            var authResp = this.PaymentSessionTests.CreateAndAuthenticatePaymentSession(testAcc, data, true, true, PaymentChallengeStatus.Unknown, false);

            var acsPayload = new AcsStatusPayload
            {
                ThreeDSServerTransID = authResp.AuthenticateResponse.ThreeDSServerTransactionID,
                TransStatus = "N",
                TransStatusReason = "14",
            };
            ACSClient.SendChallengeStatus(authResp.AuthenticateResponse.AcsSignedContent, acsPayload).Wait();

            NotifyThreeDSChallengeCompleted(testAcc, authResp.PaymentSession.Id, authResp.AuthenticateResponse.ThreeDSServerTransactionID, true, PaymentChallengeStatus.TimedOut);
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        public void AuthenticationStatus_IsVerified_ChallengeCompleted()
        {
            var paymentSessionData = new PaymentSessionData()
            {
                Language = "en",
                Amount = 101.0m,
                Currency = "EUR",
                Partner = this.TestSettings.Partner,
                Country = "GB",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
            };

            AccountInfo testAcc = this.TestSettings.GetPSD2Account(true);

            //// validate the create payment session API
            CreateAndAuthenticateResponse authResp = this.PaymentSessionTests.CreateAndAuthenticatePaymentSession(testAcc, paymentSessionData, true, true, PaymentChallengeStatus.Unknown, false);

            NotifyThreeDSChallengeCompleted(testAcc, authResp.PaymentSession.Id, authResp.AuthenticateResponse.ThreeDSServerTransactionID, true, PaymentChallengeStatus.Succeeded);

            //// validate the create and authenticate payment session API
            this.PaymentSessionTests.GetAuthenticationStatus(testAcc, authResp.PaymentSession.Id, true, true, PaymentChallengeStatus.Succeeded);
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        public void AuthenticationStatus_IsNotVerified_ChallengeFailed()
        {
            var paymentSessionData = new PaymentSessionData()
            {
                Language = "en",
                Amount = 101.0m,
                Currency = "EUR",
                Partner = this.TestSettings.Partner,
                Country = "GB",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
            };

            AccountInfo testAcc = this.TestSettings.GetPSD2Account(true);

            //// validate the create payment session API
            CreateAndAuthenticateResponse authResp = this.PaymentSessionTests.CreateAndAuthenticatePaymentSession(testAcc, paymentSessionData, true, true, PaymentChallengeStatus.Unknown, false);

            var acsPayload = new AcsStatusPayload
            {
                ThreeDSServerTransID = authResp.AuthenticateResponse.ThreeDSServerTransactionID,
                TransStatus = "N",
                TransStatusReason = "10"
            };
            ACSClient.SendChallengeStatus(authResp.AuthenticateResponse.AcsSignedContent, acsPayload).Wait();
            NotifyThreeDSChallengeCompleted(testAcc, authResp.PaymentSession.Id, authResp.AuthenticateResponse.ThreeDSServerTransactionID, true, PaymentChallengeStatus.Failed);

            //// validate the create and authenticate payment session API
            this.PaymentSessionTests.GetAuthenticationStatus(testAcc, authResp.PaymentSession.Id, true, false, PaymentChallengeStatus.Failed);
        }

        private void NotifyThreeDSChallengeCompleted(AccountInfo testAcc, string sessionId, string threeDSServerTransId, bool includeTestHeader, PaymentChallengeStatus expectedStatus)
        {
            HttpStatusCode code = HttpStatusCode.Unused;
            string body = null;

            Common.TestContext tc = includeTestHeader ?
                new Common.TestContext(
                    contact: "px.azure.cot",
                    retention: DateTime.MaxValue,
                    scenarios: "px-service-psd2-e2e-emulator")
                : null;

            // NotifyThreeDS is completed
            var notificationReq = new
            {
                ThreeDSServerTransID = threeDSServerTransId,
            };

            this.ExecuteRequest(
                string.Format(
                    "v7.0/{0}/paymentSessions/{1}/notifyThreeDSChallengeCompleted",
                    testAcc.AccountId,
                    sessionId),
                HttpMethod.Post,
                tc,
                notificationReq,
                null,
                (responseCode, responseBody) =>
                {
                    code = responseCode;
                    body = responseBody.ToString();
                });

            var resp = JsonConvert.DeserializeObject<PaymentSession>(body);

            Assert.AreEqual(HttpStatusCode.OK, code);
            Assert.IsNotNull(resp.Id);
            Assert.AreEqual(expectedStatus, resp.ChallengeStatus);
        }

        private string Authenticate(AccountInfo testAcc, string sessionId, bool includeTestHeader, PaymentChallengeStatus expectedStatus)
        {
            HttpStatusCode code = HttpStatusCode.Unused;
            string body = null;
            var authenticationRequest = PSD2AppPaymentSessionTests.GetAuthenticationRequest();

            Common.TestContext tc = includeTestHeader ?
                new Common.TestContext(
                    contact: "px.azure.cot",
                    retention: DateTime.MaxValue,
                    scenarios: "px-service-psd2-e2e-emulator")
                : null;

            // Authenticate
            this.ExecuteRequest(
                string.Format(
                    "v7.0/{0}/paymentSessions/{1}/authenticate",
                    testAcc.AccountId,
                    sessionId),
                HttpMethod.Post,
                tc,
                authenticationRequest,
                null,
                (responseCode, responseBody) =>
                {
                    code = responseCode;
                    body = responseBody.ToString();
                });

            PaymentChallenge.Model.AuthenticationResponse authenticationRes = JsonConvert.DeserializeObject<PaymentChallenge.Model.AuthenticationResponse>(body);

            Assert.AreEqual(HttpStatusCode.OK, code);
            Assert.IsNotNull(authenticationRes?.ThreeDSServerTransactionID);
            Assert.AreEqual(authenticationRes?.ChallengeStatus, expectedStatus);

            return authenticationRes.ThreeDSServerTransactionID;
        }
    }
}
