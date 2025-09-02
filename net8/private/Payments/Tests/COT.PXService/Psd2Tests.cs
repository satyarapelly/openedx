// <copyright file="Psd2Tests.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace COT.PXService
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model;
    using Microsoft.Commerce.Payments.PXService.V7;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Test.Common;
    using Common = Microsoft.Commerce.Payments.Common.Transaction;
    using External = Microsoft.Commerce.Payments.PXService.Model.ThreeDSExternalService;
    using PaymentChallenge = Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge;
    using OldContract = Microsoft.Commerce.Payments.PXService.V7;
    using PayerAuth = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService;

    [TestClass]
    public class Psd2Tests : TestBase
    {
        public const string UrlTemplatePaymentSessionDescriptions = "v7.0/{0}/paymentSessionDescriptions?paymentSessionData={1}";
        public const string UrlTemplateChallengeDescriptions = "v7.0/{0}/challengeDescriptions?timezoneOffset=01&paymentSessionOrData={1}";
        public const string UrlTemplateBrowserAuthenticate = "v7.0/paymentSessions/{0}/authenticate";
        public const string UrlTemplateBrowserNotifyThreeDSChallengeCompleted = "v7.0/paymentSessions/{0}/notifyThreeDSChallengeCompleted";
        public const string FingerprintIFrame = "fingerprintIFrame";
        public const string ThreeDSChallengeIFrame = "threeDSChallengeIFrame";
        public const string ReturnContext = "ReturnContext";
        public const string Succeeded = "Succeeded";
        public const string Failed = "Failed";
        public const string ByPassed = "ByPassed";
        public const string ClientActionFailure = "Failure";

        public TestSettings TestSettings { get; private set; }

        private static PaymentSessionData paymentSessionData = new PaymentSessionData()
        {
            Language = "en",
            Amount = 10.0m,
            Currency = "EUR",
            Partner = "webblends",
            Country = "de",
            HasPreOrder = false,
            ChallengeScenario = ChallengeScenario.PaymentTransaction,
            ChallengeWindowSize = External.ChallengeWindowSize.Four,
            IsMOTO = false,
            BillableAccountId = "blah+",
            ClassicProduct = "fooBar"
        };

        private static PaymentSessionData paymentSessionDataMoto = new PaymentSessionData()
        {
            Language = "en",
            Amount = 10.0m,
            Currency = "EUR",
            Partner = "webblends",
            Country = "de",
            HasPreOrder = false,
            ChallengeScenario = ChallengeScenario.PaymentTransaction,
            ChallengeWindowSize = External.ChallengeWindowSize.Four,
            IsMOTO = true,
        };

        private static PaymentChallenge.Model.AuthenticationRequest authenticationRequest = new PaymentChallenge.Model.AuthenticationRequest
        {
            SdkAppId = "4796328b-4c95-4100-a823-7be5026feeed",
            SdkMaxTimeout = "60",
            SdkReferenceNumber = "SDKREF00000001",
            SdkTransID = "d03dfef3-055e-43c7-84d2-049cb44929c3",
            SdkInterface = "03",
            SdkEphemPublicKey = new PayerAuth.EphemPublicKey
            {
                Kty = "EC",
                Crv = "P-256",
                X = "xp1tW4PXn-cNpC6Jo9jGSjjN1OC-fc6RQNSom2nVklM",
                Y = "NrRH6jr8CQEbVQxpAMUkq0U7_oAYGn1avSEi9V25E4M"
            },
            SdkUiType = new List<string>() { "01", "02", "03", "04" },
            SdkEncData = "eyJhbGciOiJSU0EtT0FFUC0yNTYiLCJlbmMiOiJBMTI4R0NNIn0.mkXezz0RGr8Ifx1jPTaQfDbiqqB4PIM9rUax4hXiolS1jgN2L2FLUpfjgX9r1u2rshQn7_Ct0bYuQ4WCQEZOUZ3yIJRqPpbedBfz8iCdn7odRL3gGywunyVCQdD4 mtFTZH0De14-x-d8mnRFxjZGuRJP4EB-wu1AdY6IEtA2lvie4YxEYNDwEvT4cTPA785fGIN1P9YaZxMWBWfKlp1YwwhkmSa7eoMWTU9GEnMQqSDibmz9g4sVeFMehDB4Au1L01lAJH3WBhJFe6wmUe2LVBQ4PyAKMGKAX_QSJY9z nHTXf7AnrA7BB69xjwPC96Xy53v1vYsrO-TD2KRt8w.oSuF_11TJ_stm0-W.HxYOeUu8u5iUrPZ-VZcu6IrxU3bJN6utqEOKO0z_Y4IXjCAvzqxwlFc9L_CkV4iADbPykK1I59RBvXbUEbVKPH0xEA6b_f5ts1J6xHg3Rhlul1tD4SAOjj5_KFJ7sGDAILdKiP7zPME5Jp6djcnXLnE547eBINxZqJ2hMWmvxauGJThxF3OR2ju5uyGpngKfQsqUIY6NDM0tqYKKZTJXtIjS5abCsd98qQv83nWZaDN02vcSTyPnog7_P34zG-sZ02xk223lNzzUcNk2fZGXbi_UeyjjH98fsDhSIKDK6T4062emVoNUCDuxpGhfr1bE9AabuQsFeRzXvOSJ76eWMoTb2ClRsoqWCJhzA1DkmkS9tv5xADHxmLT_VuEsepJ1 V45Z5WI0FyzNt2vId3ILuExcGRr-gfcHHnACLqRqfPi4PrboKnA-_zaxZvrBZr9AZURJqvVNcXaEtZqEoXQaomh9XFKnhoE9_HylEJX-1UXIaKK1ccuazD5r3N6jWCFXsmy1qRrnKXg6fzGvDYfTO0z1L9KAiS5w8za3FlCbJLyqV6kJ8GU8iK1dx75_KtPbqqeazpTRHQ3BfzGE9QoKqAmv26RmNd4HkCFeUEVTkARIK4g1hL8 AZVJlIw40taK-3CJFG69-Lqlm0HyaHyZHaWOHeLZzhfEuEc4KabbCgrYXSFmeIxhY1jWx0s7bA9njLYOGsuZ8rt2ghcPJZuWPGRaZ38k65Lf6m VuEYWnuUxoS4j-OINPQJQiFFSCUORr42Ujh_cCMzOXUNJiyxe68TlEf57sbab1SYLg8V_kZQiKHowksxkTFjpUCpJTw6eq3PTELWiVDVsE71D43THQ9X6MLLuoyX9JmtbB8OkzqTnOSmoE5iSRqpipI1mHI3kd5jovVDeRvjS7ahsULumu21odVHG5V7o25P717V8r CD17frhtRPzlvQcRYoYCCrvn_S2_Ll6EJ4FUT9U7qoxLFC4dwzwtRSkYK9Gs_jk0fBgkRWwAHhNvVujghgkqhVFOK 3EDK5yOyeeG9WxOdT1tLAEUmrj939fOaP5Q16uqaUtbg7ZrfP_7mEe_he3teQPZIaFLO3oxDodBKuobBp90DrVmz6f1OyTR5tRgi8ntB98jqxbWiB-ogXtnXOvcCD9v-8NGISd5fE8RBleBG9g6zc9abLVCST5bcw4MEDt3bogOuoUhgC7axZGCHSWoJktIcHjncL69zi9t4FF4dnIXoBvOmgNfrq9SacvOq0CXalrUbOSr saBl-iykxQvZ_65MbGhxNIfvEIIZiU33PhL4XpoV6qxBCcKGUBIF5PvBOhT2rfAsg84_TvS7hA5tRsot3xY0ozfNGxGQAcvllz_o4bRzuZG_wPxNLG_7yiM8pClob4PPc_HKGhLbM_6jympV FP8KmmVhdD6TNlIBqFLI_HTxCOATttym7jMdyofix9b9CBvoFfHzOHguqwP_5DDC64RSyztyntGhwr5v0WWeJMOUe11Wb2ecjCH iWE-_sQaABfTSxMWgERY8cG4rryQSG5K9EILknNnOYknbqLMMlpjGwnsnGRYJJ_zDCCIKkfIEE31PQ18_FVuiDfE9a7T5eS1XPY9VGUpLeyCB45jXqkhJ1j1YV97qYyGrcCy1tBMwAtKPA6HrSS1pzuce YM9VKshUJ-1rOXzUHCKD7SkF2gJbDTQI9MCFI-Kb6DwYmiPQVslOWbosHQc0mNOBoYopus6ulXVFQK6tD7e6Fk3GMV_tYJBCTsbCoUhHS3X-jtZVvg1Q9-1Q7MKZJYNg7nldq5LYztx_yCQhpmfcA4x1n9oNKKnv5FWwYGEwZFFmpV-i3DM9gooVAbjVoDkfRrMNt6r0hYdquGxXTXBJxXa315NhXeMDDFdCQalFTesVDIMbptNLGQ_5G9pVVlaWfK3dQDcZRmcsknePLDKDooqjPsoz6Yv8HoGvtQGDTSkkCyVks1oOBhOaqa7uNBXJL5hA3MiJaoCogX2mrNFT02THUyAvWpWUUKbGP_Nxxc-TfsJAaj2iW_LpkC4kwCtzY6xkxOik6aRxWMdcgY9xN5XkOU7uLLhIcQCX9hTwtBPxiYRAzgueZrsk4_MTy6GOcOaQK0uflGs9g8BirpUUkwDwFI 7OV6oVJOzcNriIsFrP5b6IrBLanE-AdofCvEgn15ztHfAUatwl_-IG9WTL1N-ib4MrGCkLDfrEgBs-1OKUUisLfkdmG3foesNr7M4Laxq95fnsAoN4b_xc24Zov8Y516DBcKAPOpkZPj3KoAMAw6c4yvpqvVStm9E4i_Mgz vdnS3i9Cc9YqhVKuvzmAhhQut4Kse1O5TRb1SEmnkDZfg4pHekt_NkW7PjH0fV6U0nTsC97R1q5fPzYeGzaNQEBY0CwOU58o9nKbrUtLwuX0WMpTsENrlvEJOaihtOEsT3X_zM_0NTlMlFUB8uPEU Mf006jxNVpa4aGAc1bUKRWqdSsnhoAxKrZsUugqAV4jhPvoMvPBWnndQ5FAaJMqg34k8CgPbq1ejtk8YLPWn1bFggiqSD6bnkmYF7viYbscwzpzN19_WXqy8fvbUhzlpGmrfpsUFL58lc0--Wm78QefAoGB7t-zTelnQu_KHH6rQ36LvCsC_Q_DPgKjMKz3RcV3qia3tpt6ueI5Ep1QCYld8CHTOR1CWrwdPT9GdL9IFWft9ImuGKex2MwyY2RYVL LL7OdNGaeteloq8C37pAmLAUqrfZZ5k43hSQfEp-J5I3pKKDdwz24iRIVbLJ3Hnoma4a82ibELpU20Gx6XFy_LfU9O2RJP_MPxjNuq4yAj90dEBAzXZ67_yF_Wb3UhtoWVX6YWdrK88_g2TUKeFzOY0h-9L7YVpyavW6W5HniRmrkGMHtYjqPDrwBuNzHkv0PPPrNQsnZBUz1U0OTiVYFtvU3zoeBDTWuMGZXNi7_WwQOAg WPMwK-5E1ZIoIVRrWIYOglpIeuNLlZoQe0TQjYKHv4hKawa4DLLgW4el0Q4aagCNVaMvJiTEaMtk4miQKPxHXtffRMC2e1TUZRiRcGBxgpc9vPmR_fWYEI5FJpUByiMqrNgpEzhfriiunHKDR561-PGR4jzjD7_Wh8NRvyllKapW-5RB_G2DhQtLetJ7NaCTjwbcKKpxGCYQevlr-tccxAlE7tvHzahSztpKhftyor2dlJU4BtGNMMU8lH0xHiypzbEY13X_R5e6RFfd1lQVPlEPNCXStZvaZyxoODYTmMKbJE9SdU4ZFi103D6rk3fG4SpdOhFnvfwaplxFc4iOaIOiCj3qomFuRVCslB8W7Av41DTe_kjJjPKzuYj4d5pMdl 1QRVjRirGbppQfRdHN5SUP7ApVFHbPUXFYWTF0kOji8uCuKjooJTkY9kDWAWDk1x3yCMut9JfPCGs0d5xhn96bYywDUm8xJa1gmD6lwyvQhDykN Xv08TxOL_6rGuT1_2u6ZgV2gqnBEd5unneumm1-Nc3n5-OpLaAavO4V9y9aHCk0yC2ayo7Gerw0NTs1PMZQsLQ0CI3cYaMaKQFFRYlZ5NVGzsxe753avu6Q89XCRx5POKS4i1JbLDVtWVPVZvJ9knJ4J0Mday_xBG5Uu-Dt43g4uxR-qP7h3Vy_yFVQX97OcBwPwup7hKvQHOwejO6cyVGKj0g91_Kr0CerthKCx4XOi4PT102m1h0hb1Mba4xVcZpnXoEvLxzk2oNygtlMF-QTu2XEJpPdAEb9fsTvmX-IBLxenc0am-IENg.mky3EOKdPsrBe6vIMNx87g",
        };

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
        [DeploymentItem(@"TestData\V17\PaymentClientSettings.json", @"TestData\V17")]
        public void TestPSD2_App_GetPaymentClientSettings()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px-service-psd2-e2e-emulator");
            this.VerifyGetPaymentClientSettings(tc);
        }

        /// <summary>
        /// Test App Flow for new contract
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        public void TestPSD2_App_PayerAuthACSEmulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px-service-psd2-e2e-emulator");
            AccountInfo testAcc = this.TestSettings.Psd2TestAccount;
            string sessionId = CreatePaymentSession(testAcc, paymentSessionData, tc);
            string tranId = Authenticate(testAcc, sessionId, tc);
            NotifyThreeDSChallengeCompleted(testAcc, sessionId, tranId, tc);

            this.CreateAndAuthenticatePaymentSession(testAcc, tc);
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        public void TestPSD2_App_MOTO()
        {
            Common.TestContext tc = null;
            AccountInfo testAcc = this.TestSettings.Psd2TestAccount;
            CreatePaymentSession(testAcc, paymentSessionDataMoto, tc, false, PaymentChallengeStatus.ByPassed, true);
        }

        /// <summary>
        /// This test verifies availability and contract of PSD2 Browser flow 
        /// Againest PayerAuth Emulator in PX
        /// </summary>
        [TestMethod]
        public void TestPSD2_Browser_PayerAuthACSEmulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px-service-psd2-e2e-emulator");
            AccountInfo testAcc = this.TestSettings.Psd2TestAccount;
            GetPaymentSessionChallengeDescriptionAndVerify(testAcc, paymentSessionData, FingerprintIFrame, tc);

            PaymentSession paymentSession = GetPaymentSessionDescriptionAndVerify(testAcc, paymentSessionData, tc);
            string threeDSMethodData = GetPaymentSessionChallengeDescriptionAndVerify(testAcc, paymentSession, FingerprintIFrame, tc);
            Authenticate_Browser(paymentSession.Id, threeDSMethodData, tc);
            NotifyThreeDSChallengeCompleted_Browser(paymentSession.Id, tc, ReturnContext);
        }

        /// <summary>
        /// This test verifies availability and contract of PSD2 Browser flow 
        /// Againest PayerAuth Emulator in PX
        /// </summary>
        [TestMethod]
        public void TestPSD2_Browser_MC()
        {
            Common.TestContext tc = null;
            AccountInfo testAcc = this.TestSettings.Psd2TestAccount;
            GetPaymentSessionChallengeDescriptionAndVerify(testAcc, paymentSessionData, FingerprintIFrame, tc);
        }

        /// <summary>
        /// This test verifies availability and contract of PSD2 Browser flow 
        /// Againest PayerAuth Emulator in PX
        /// </summary>
        [TestMethod]
        public void TestPSD2_Browser_PXEmulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.payerauth.psd2.challenge.success,px.pims.3ds");
            AccountInfo testAcc = this.TestSettings.Psd2TestAccount;

            PaymentSession paymentSession = GetPaymentSessionDescriptionAndVerify(testAcc, paymentSessionData, tc);
            string threeDSMethodData = GetPaymentSessionChallengeDescriptionAndVerify(testAcc, paymentSession, FingerprintIFrame, tc);
            Authenticate_Browser(paymentSession.Id, threeDSMethodData, tc);
            NotifyThreeDSChallengeCompleted_Browser(paymentSession.Id, tc, ReturnContext);

            GetPaymentSessionChallengeDescriptionAndVerify(testAcc, paymentSessionData, FingerprintIFrame, tc);
        }

        /// <summary>
        /// After code against payerauth new contract, will remove this cot and add CIT for this case.
        /// Skip finger print 
        /// </summary>
        [TestMethod]
        public void TestPSD2_Browser_SkipFingerPrint_PXEmulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.payerauth.psd2.challenge.skipfp,px.pims.3ds");
            AccountInfo testAcc = this.TestSettings.Psd2TestAccount;
            PaymentSession paymentSession = GetPaymentSessionDescriptionAndVerify(testAcc, paymentSessionData, tc);
            string threeDSMethodData = GetPaymentSessionChallengeDescriptionAndVerify(testAcc, paymentSession, ThreeDSChallengeIFrame, tc);
            NotifyThreeDSChallengeCompleted_Browser(paymentSession.Id, tc, ReturnContext);

            GetPaymentSessionChallengeDescriptionAndVerify(testAcc, paymentSessionData, ThreeDSChallengeIFrame, tc);
        }

        /// <summary>
        /// After code against payerauth new contract, will remove this cot and add CIT for this case.
        /// Skip finger print 
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestPSD2_Browser_SkipCreq_PXEmulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.payerauth.psd2.challenge.skipcreq,px.pims.3ds");
            AccountInfo testAcc = this.TestSettings.Psd2TestAccount;
            GetPaymentSessionChallengeDescriptionAndVerify(testAcc, paymentSessionData, ReturnContext, tc, Succeeded);
        }

        /// <summary>
        /// After code against payerauth new contract, will remove this cot and add CIT for this case.
        /// </summary>
        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        [TestCategory(TestCategory.PRODContinuousTest)]
        public void TestPSD2_CreatePaymentSession_NoChallenge()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.pims.3ds.notrequired");
            AccountInfo testAcc = this.TestSettings.Psd2TestAccount;
            CreatePaymentSession(testAcc, paymentSessionData, tc, false, PaymentChallengeStatus.NotApplicable);
        }

        /// <summary>
        /// After code against payerauth new contract, will remove this cot and add CIT for this case.
        /// </summary>
        [TestMethod]
        public void TestPSD2_Browser_Failed_PXEmulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.payerauth.psd2.challenge.failed,px.pims.3ds");
            AccountInfo testAcc = this.TestSettings.Psd2TestAccount;
            PaymentSession paymentSession = GetPaymentSessionDescriptionAndVerify(testAcc, paymentSessionData, tc);
            string threeDSMethodData = GetPaymentSessionChallengeDescriptionAndVerify(testAcc, paymentSession, FingerprintIFrame, tc);
            Authenticate_Browser(paymentSession.Id, threeDSMethodData, tc, ClientActionFailure);
        }

        /// <summary>
        /// After code against payerauth new contract, will remove this cot and add CIT for this case.
        /// Skip finger print 
        /// </summary>
        [TestMethod]
        public void TestPSD2_Browser_Cancelled_PXEmulator()
        {
            Common.TestContext tc = new Common.TestContext("PX.COT", DateTime.Now, "px.payerauth.psd2.challenge.cancelled,px.pims.3ds");
            AccountInfo testAcc = this.TestSettings.Psd2TestAccount;
            PaymentSession paymentSession = GetPaymentSessionDescriptionAndVerify(testAcc, paymentSessionData, tc);
            string threeDSMethodData = GetPaymentSessionChallengeDescriptionAndVerify(testAcc, paymentSession, FingerprintIFrame, tc);
            Authenticate_Browser(paymentSession.Id, threeDSMethodData, tc);
            NotifyThreeDSChallengeCompleted_Browser(paymentSession.Id, tc, "GoHome");
        }

        private PaymentSession GetPaymentSessionDescriptionAndVerify(AccountInfo testAcc, PaymentSessionData paymentSessionData, Common.TestContext tc)
        {
            paymentSessionData.PaymentInstrumentAccountId = testAcc.AccountId;
            paymentSessionData.PaymentInstrumentId = testAcc.CreditCardPiid;
            string encodedPaymentSessionData = WebUtility.UrlEncode(JsonConvert.SerializeObject(paymentSessionData));
            HttpStatusCode code = HttpStatusCode.Unused;
            dynamic response = null;
            string url = string.Format(
                    UrlTemplatePaymentSessionDescriptions,
                    testAcc.AccountId,
                    encodedPaymentSessionData);
            this.ExecuteRequest(
                url,
                HttpMethod.Get,
                tc,
                null,
                null,
                (responseCode, responseBody) =>
                {
                    code = responseCode;
                    response = responseBody;
                });

            // Verification:
            string value = response?.First?.clientAction?.context?.ToString();
            PaymentSession paymentSession = JsonConvert.DeserializeObject<PaymentSession>(value);
            Assert.IsNotNull(paymentSession, "resourceInfo is empty");
            Assert.IsNotNull(paymentSession.Id, "sessionId isnt set");
            Assert.AreEqual(true, paymentSession.IsChallengeRequired);
            Assert.IsNotNull(paymentSession.Signature);
            Assert.AreEqual(PaymentChallengeStatus.Unknown, paymentSession.ChallengeStatus);
            Assert.AreEqual(paymentSessionData.ChallengeWindowSize, paymentSession.ChallengeWindowSize);
            return paymentSession;
        }

        private string GetPaymentSessionChallengeDescriptionAndVerify(
            AccountInfo testAcc,
            PaymentSession paymentSession,
            string expectedDescriptionType,
            Common.TestContext tc)
        {
            HttpStatusCode code = HttpStatusCode.Unused;
            dynamic response = null;
            string encodedPaymentSession = WebUtility.UrlEncode(JsonConvert.SerializeObject(paymentSession));
            string url = string.Format(
                    UrlTemplateChallengeDescriptions,
                    testAcc.AccountId,
                    encodedPaymentSession);

            this.ExecuteRequest(
                url,
                HttpMethod.Get,
                tc,
                null,
                null,
                (responseCode, responseBody) =>
                {
                    code = responseCode;
                    response = responseBody;
                });

            string type = response.First.identity.description_type;
            Assert.AreEqual(type, expectedDescriptionType);
            Assert.AreEqual(HttpStatusCode.OK, code);

            // Verification:
            if (expectedDescriptionType == FingerprintIFrame)
            {
                // fetch the authenticate URL from encoded ThreeDSMethodData
                string iframe = response.First.displayDescription.First.members.First.displayContent.ToString();
                Assert.IsNotNull(iframe, response.ToString());
                string encodedThreeDSMethodData = ParseOutThreeDSMethodDataValue(iframe);
                string name = ParseOutThreeDSMethodDataName(iframe);
                return string.Format("{0}={1}", name, encodedThreeDSMethodData);
            }
            else if (expectedDescriptionType == ThreeDSChallengeIFrame)
            {
                Assert.IsTrue(response.ToString().Contains("action=\\\"https"));
            }
            else
            {
                Assert.Fail("Unexpected expectedDescriptionType");
            }

            return string.Empty;
        }

        private static string ParseOutThreeDSMethodDataName(string iframe)
        {
            int start = iframe.IndexOf("name=\"") + 6;
            int end = iframe.IndexOf("\" value=");
            return iframe.Substring(start, end - start);
        }

        private static string ParseOutThreeDSMethodDataValue(string iframe)
        {
            int start = iframe.IndexOf("value=\"") + 7;
            int end = iframe.IndexOf("\" />");
            return iframe.Substring(start, end - start);
        }

        private void GetPaymentSessionChallengeDescriptionAndVerify(
            AccountInfo testAcc,
            PaymentSessionData paymentSession,
            string expectedDescriptionType,
            Common.TestContext tc,
            string expectedResult = null)
        {
            paymentSessionData.PaymentInstrumentAccountId = testAcc.AccountId;
            paymentSessionData.PaymentInstrumentId = testAcc.CreditCardPiid;

            HttpStatusCode code = HttpStatusCode.Unused;
            dynamic response = null;
            string encodedPaymentSession = WebUtility.UrlEncode(JsonConvert.SerializeObject(paymentSession));
            string url = string.Format(
                    UrlTemplateChallengeDescriptions,
                    testAcc.AccountId,
                    encodedPaymentSession);

            this.ExecuteRequest(
                url,
                HttpMethod.Get,
                tc,
                null,
                null,
                (responseCode, responseBody) =>
                {
                    code = responseCode;
                    response = responseBody;
                });

            // Verification:
            if (expectedDescriptionType == FingerprintIFrame)
            {
                string type = response.First.identity.description_type;
                Assert.AreEqual(type, expectedDescriptionType);
                Assert.AreEqual(HttpStatusCode.OK, code);
                string iframe = response.First.displayDescription.First.members.First.displayContent.ToString();
                Assert.IsNotNull(iframe, response.ToString());
            }
            else if (expectedDescriptionType == ThreeDSChallengeIFrame)
            {
                string type = response.First.identity.description_type;
                Assert.AreEqual(type, expectedDescriptionType);
                Assert.AreEqual(HttpStatusCode.OK, code);
                Assert.IsTrue(response.ToString().Contains("action=\\\"https"));
            }
            else if (expectedDescriptionType == ReturnContext)
            {
                string type = response.First.clientAction.type;
                Assert.AreEqual(type, expectedDescriptionType);
                Assert.AreEqual(HttpStatusCode.OK, code);
                string challengeStatus = response.First.clientAction.context.challengeStatus;
                Assert.AreEqual(challengeStatus, expectedResult);
            }
            else 
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, code);
            }
        }

        private void Authenticate_Browser(
            string sessionId, 
            string threeDSMethodCompletedFormData, 
            Common.TestContext tc,
            string expectedPostMessgeClientAction = "threeDSChallengeIFrame")
        {
            string value = threeDSMethodCompletedFormData.Replace("threeDSMethodData=", string.Empty);
            External.ThreeDSMethodData data = JsonConvert.DeserializeObject<External.ThreeDSMethodData>(ThreeDSUtils.DecodeBase64(value));
            string expectedUrl = string.Format(
                    UrlTemplateBrowserAuthenticate,
                    sessionId);
            string pifdEndPoint = string.Format("https://{0}/V6.0", this.TestSettings.PifdHostName);
            string authenticateUrl = data.ThreeDSMethodNotificationURL.Replace(pifdEndPoint, "v7.0");
            Assert.AreEqual(expectedUrl, authenticateUrl, "Authenticate url is wrong");

            HttpStatusCode code = HttpStatusCode.Unused;
            string body = null;
            this.ExecuteRequest(
                authenticateUrl,
                HttpMethod.Post,
                tc,
                threeDSMethodCompletedFormData,
                null,
                (responseCode, responseBody) =>
                {
                    code = responseCode;
                    body = responseBody.ToString();
                },
                Test.Common.Constants.HeaderValues.FormContent);

            Assert.IsTrue(body.Contains("window.parent.postMessage"));
            if (expectedPostMessgeClientAction == ThreeDSChallengeIFrame)
            {
                Assert.IsTrue(body.Contains(ThreeDSChallengeIFrame));
                Assert.IsTrue(body.Contains("action=\\\\\\\"https"));
                Assert.IsTrue(body.Contains("width\\\":\\\"600px"));
                Assert.IsTrue(body.Contains("height\\\":\\\"400px"));
            }
            else if (expectedPostMessgeClientAction == ClientActionFailure)
            {
                Assert.IsTrue(body.Contains(ClientActionFailure));
            }  
        }

        private void NotifyThreeDSChallengeCompleted_Browser(string sessionId, Common.TestContext tc, string clientActionName)
        {
            // NotifyThreeDSChallengeCompleted
            string threeDSChallengeCompletedFormData = "cres=eyJ0aHJlZURTTWV0aG9kTm90aWZpY2F0aW9uVVJMIjoiaHR0cDovL2xvY2FsaG9zdC9weC92Ny4wL3Nlc3Npb25zLzU5OGEwYjIyLTAwNWQtNDFhMC1hYWE4LWQ4NjQ0ZWFkY2JmNy9jaGFsbGVuZ2VDb21wbGV0ZWQiLCJ0aHJlZURTU2VydmVyVHJhbnNJRCI6ImNjYzAxOGVkLTdlYzEtNGJlYi05YzQyLTE4OWE5NjUwMDkyNCJ9";
            HttpStatusCode code = HttpStatusCode.Unused;
            string body = null;

            this.ExecuteRequest(
                string.Format(
                    UrlTemplateBrowserNotifyThreeDSChallengeCompleted,
                    sessionId),
                HttpMethod.Post,
                tc,
                threeDSChallengeCompletedFormData,
                null,
                (responseCode, responseBody) =>
                {
                    code = responseCode;
                    body = responseBody.ToString();
                },
                Test.Common.Constants.HeaderValues.FormContent);

            Assert.IsTrue(body.Contains(sessionId));
            Assert.IsTrue(body.Contains(clientActionName));
        }

        private string CreatePaymentSession(
            AccountInfo testAcc, 
            PaymentSessionData data, 
            Common.TestContext tc, 
            bool expectedIsChallengeRequired = true, 
            PaymentChallengeStatus expectedStatus = PaymentChallengeStatus.Unknown,
            bool isMoto = false)
        {
            data.PaymentInstrumentId = testAcc.CreditCardPiid;

            HttpStatusCode code = HttpStatusCode.Unused;
            string body = null;
            var headers = new Dictionary<string, string>();
            if (isMoto)
            {
                headers.Add("x-ms-ismoto", "true");
            }

            this.ExecuteRequest(
                string.Format(
                    "v7.0/{0}/paymentSessions",
                    testAcc.AccountId),
                HttpMethod.Post,
                tc,
                data,
                headers,
                (responseCode, responseBody) =>
                {
                    code = responseCode;
                    body = responseBody.ToString();
                });

            Assert.AreEqual(HttpStatusCode.OK, code);
            var resp = JsonConvert.DeserializeObject<PaymentSession>(body);
            Assert.IsNotNull(resp?.Id);
            Assert.AreEqual(expectedIsChallengeRequired, resp.IsChallengeRequired);
            Assert.AreEqual(expectedStatus, resp.ChallengeStatus);
            return resp.Id;
        }

        private string Authenticate(AccountInfo testAcc, string sessionId, Common.TestContext tc)
        {
            HttpStatusCode code = HttpStatusCode.Unused;
            string body = null;

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
            return authenticationRes.ThreeDSServerTransactionID;
        }

        private string CreateAndAuthenticatePaymentSession(AccountInfo testAcc, Common.TestContext tc)
        {
            paymentSessionData.PaymentInstrumentAccountId = testAcc.AccountId;
            paymentSessionData.PaymentInstrumentId = testAcc.CreditCardPiid;
            CreateAndAuthenticateRequest request = new CreateAndAuthenticateRequest()
            {
                PaymentSessionData = paymentSessionData,
                AuthenticateRequest = authenticationRequest
            };

            HttpStatusCode code = HttpStatusCode.Unused;
            string body = null;
            this.ExecuteRequest(
                string.Format(
                    "v7.0/{0}/paymentSessions/createAndAuthenticate",
                    testAcc.AccountId),
                HttpMethod.Post,
                tc,
                request,
                null,
                (responseCode, responseBody) =>
                {
                    code = responseCode;
                    body = responseBody.ToString();
                });

            Assert.AreEqual(HttpStatusCode.OK, code);
            var resp = JsonConvert.DeserializeObject<CreateAndAuthenticateResponse>(body);
            Assert.IsNotNull(resp?.PaymentSession?.Id);
            return resp.PaymentSession.Id;
        }

        private void NotifyThreeDSChallengeCompleted(AccountInfo testAcc, string sessionId, string threeDSServerTransId, Common.TestContext tc)
        {
            HttpStatusCode code = HttpStatusCode.Unused;
            string body = null;

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
        }

        private void VerifyGetPaymentClientSettings(Common.TestContext tc)
        {
            const string Version = "V17";
            HttpStatusCode code = HttpStatusCode.Unused;
            string body = null;
            this.ExecuteRequest(
                string.Format("v7.0/settings/Microsoft.Payments.Client/{0}", Version),
                HttpMethod.Get,
                tc,
                null,
                null,
                (responseCode, responseBody) =>
                {
                    code = responseCode;
                    body = responseBody.ToString();
                });

            Assert.AreEqual(HttpStatusCode.OK, code);
            var actualResp = JsonConvert.DeserializeObject<JObject>(body);

            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format(@"TestData\{0}\PaymentClientSettings.json", Version));
            string expectedPaymentClientSettings = File.ReadAllText(filePath);
            var expectedResp = JsonConvert.DeserializeObject<JObject>(expectedPaymentClientSettings);

            Assert.IsTrue(JToken.DeepEquals(expectedResp, actualResp), "Expected paymentclientsettings is not returned");
        }
    }
}