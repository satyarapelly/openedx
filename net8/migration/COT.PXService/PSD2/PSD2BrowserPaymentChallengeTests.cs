// <copyright file="PSD2BrowserPaymentChallengeTests.cs" company="Microsoft">Copyright (c) Microsoft 2019-2020. All rights reserved.</copyright>

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
    public class PSD2BrowserPaymentChallengeTests : TestBase
    {
        public TestSettings TestSettings { get; private set; }

        public PSD2BrowserPaymentSessionTests PaymentSessionTests { get; private set; }

        public AcsServiceClient ACSClient { get; private set; }

        [TestInitialize]
        public void TestInitialize()
        {
            this.Initialize();
            this.TestSettings = TestSettings.CreateInstance();
            this.PaymentSessionTests = new PSD2BrowserPaymentSessionTests();
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
                Amount = 210.0m,
                Currency = "EUR",
                Partner = this.TestSettings.Partner,
                Country = "GB",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
            };

            AccountInfo testAcc = this.TestSettings.PSD2EuroAccountFrictionLess;
            this.PaymentSessionTests.HandlePaymentChallenge(testAcc, data, true, true, PaymentChallengeStatus.Succeeded, true, false).Wait();

            PaymentSession session = this.PaymentSessionTests.CreatePaymentSession(testAcc, data, true, true, PaymentChallengeStatus.Unknown, false);
            this.PaymentSessionTests.HandlePaymentChallenge(testAcc, session, true, true, PaymentChallengeStatus.Succeeded, true, false).Wait();
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
                Amount = 200.0m,
                Currency = "EUR",
                Partner = this.TestSettings.Partner,
                Country = "GB",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
            };

            AccountInfo testAcc = this.TestSettings.GetPSD2Account(true);
            this.PaymentSessionTests.HandlePaymentChallenge(testAcc, data, true, true, PaymentChallengeStatus.Succeeded, false, false).Wait();

            PaymentSession session = this.PaymentSessionTests.CreatePaymentSession(testAcc, data, true, true, PaymentChallengeStatus.Unknown, false);
            this.PaymentSessionTests.HandlePaymentChallenge(testAcc, session, true, true, PaymentChallengeStatus.Succeeded, false, false).Wait();
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
                Amount = 210.0m,
                Currency = "EUR",
                Partner = this.TestSettings.Partner,
                Country = "GB",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
            };

            AccountInfo testAcc = this.TestSettings.GetPSD2Account(true);
            this.PaymentSessionTests.HandlePaymentChallenge(testAcc, data, true, true, PaymentChallengeStatus.Failed, false, false).Wait();

            PaymentSession session = this.PaymentSessionTests.CreatePaymentSession(testAcc, data, true, true, PaymentChallengeStatus.Unknown, false);
            this.PaymentSessionTests.HandlePaymentChallenge(testAcc, session, true, true, PaymentChallengeStatus.Failed, false, false).Wait();
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
                Amount = 220.0m,
                Currency = "EUR",
                Partner = this.TestSettings.Partner,
                Country = "GB",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
            };

            AccountInfo testAcc = this.TestSettings.GetPSD2Account(true);
            this.PaymentSessionTests.HandlePaymentChallenge(testAcc, data, true, true, PaymentChallengeStatus.Cancelled, false, false).Wait();

            PaymentSession session = this.PaymentSessionTests.CreatePaymentSession(testAcc, data, true, true, PaymentChallengeStatus.Unknown, false);
            this.PaymentSessionTests.HandlePaymentChallenge(testAcc, session, true, true, PaymentChallengeStatus.Cancelled, false, false).Wait();
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
                Amount = 260.0m,
                Currency = "EUR",
                Partner = this.TestSettings.Partner,
                Country = "GB",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
            };

            AccountInfo testAcc = this.TestSettings.GetPSD2Account(true);
            this.PaymentSessionTests.HandlePaymentChallenge(testAcc, data, true, true, PaymentChallengeStatus.TimedOut, false, false).Wait();

            PaymentSession session = this.PaymentSessionTests.CreatePaymentSession(testAcc, data, true, true, PaymentChallengeStatus.Unknown, false);
            this.PaymentSessionTests.HandlePaymentChallenge(testAcc, session, true, true, PaymentChallengeStatus.TimedOut, false, false).Wait();
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
            this.PaymentSessionTests.HandlePaymentChallenge(testAcc, data, true, true, PaymentChallengeStatus.Succeeded, false, false).Wait();
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
                Amount = 230m,
                Currency = "EUR",
                Partner = this.TestSettings.Partner,
                Country = "GB",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.RecurringTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
            };

            AccountInfo testAcc = this.TestSettings.PSD2EuroAccount;
            this.PaymentSessionTests.HandlePaymentChallenge(testAcc, data, true, true, PaymentChallengeStatus.Succeeded, false, false).Wait();
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
                Amount = 240m,
                Currency = "EUR",
                Partner = this.TestSettings.Partner,
                Country = "GB",
                HasPreOrder = true,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
            };

            AccountInfo testAcc = this.TestSettings.PSD2EuroAccount;
            this.PaymentSessionTests.HandlePaymentChallenge(testAcc, data, true, true, PaymentChallengeStatus.Succeeded, false, false).Wait();
        }
    }
}
