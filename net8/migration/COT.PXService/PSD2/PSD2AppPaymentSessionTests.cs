// <copyright file="PSD2AppPaymentSessionTests.cs" company="Microsoft">Copyright (c) Microsoft 2019-2020. All rights reserved.</copyright>

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
    using System.Threading.Tasks;

    [TestClass]
    public class PSD2AppPaymentSessionTests : TestBase
    {
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
        public void TestConnectionBetweenPXandPayerAuth()
        {
            AccountInfo testAcc = TestSettings.GetPSD2Account(true);
            Common.TestContext tc = new Common.TestContext(
                      contact: "px.azure.cot",
                      retention: DateTime.MaxValue,
                      scenarios: "px-service-psd2-e2e-emulator");

            HttpStatusCode code = HttpStatusCode.Unused;
            string body = null;
            var headers = new Dictionary<string, string>();
            object data = new
            {
                shopperId = testAcc.AccountId
            };

            this.ExecuteRequest(
                 "v7.0/sessions",
                HttpMethod.Post,
                tc,
                data,
                headers,
                (responseCode, responseBody) =>
                {
                    code = responseCode;
                    body = responseBody?.ToString();
                });

            Assert.AreEqual(HttpStatusCode.OK, code);
            var resp = JsonConvert.DeserializeObject<object>(body);
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
                Amount = 105.0m,
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
            CreateAndAuthenticatePaymentSession(testAcc, paymentSessionDataMoto, true, false, PaymentChallengeStatus.ByPassed, true);
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
                Amount = 115.0m,
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
            CreateAndAuthenticatePaymentSession(testAcc, paymentSessionDataMoto, false, false, PaymentChallengeStatus.NotApplicable, false);
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
            CreatePaymentSession(testAcc, paymentSessionDataMoto, true, true, PaymentChallengeStatus.Unknown, false);

            //// validate the create and authenticate payment session API
            CreateAndAuthenticatePaymentSession(testAcc, paymentSessionDataMoto, true, true, PaymentChallengeStatus.Unknown, false);
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        public void AuthenticationStatus_IsVerified()
        {
            var paymentSessionData = new PaymentSessionData()
            {
                Language = "en",
                Amount = 115.0m,
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
            var sessionId = CreatePaymentSession(testAcc, paymentSessionData, false, false, PaymentChallengeStatus.NotApplicable, false);

            //// validate the authentication status
            GetAuthenticationStatus(testAcc, sessionId, false, true, PaymentChallengeStatus.NotApplicable);
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        public void AuthenticationStatus_IsNotVerified()
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
            var sessionId = CreatePaymentSession(testAcc, paymentSessionData, true, true, PaymentChallengeStatus.Unknown, false);

            //// validate the create and authenticate payment session API
            GetAuthenticationStatus(testAcc, sessionId, true, false, PaymentChallengeStatus.Unknown);
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        public void AuthenticationStatus_IsVerified_IsMoto()
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
                IsMOTO = true,
            };

            AccountInfo testAcc = this.TestSettings.GetPSD2Account(true);

            //// validate the create payment session API
            var sessionId = CreatePaymentSession(testAcc, paymentSessionData, true, true, PaymentChallengeStatus.ByPassed, true);

            //// validate the create and authenticate payment session API
            GetAuthenticationStatus(testAcc, sessionId, true, true, PaymentChallengeStatus.ByPassed);
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        public void AuthenticationStatus_IsVerified_Frictionless()
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

            AccountInfo testAcc = this.TestSettings.PSD2EuroAccountFrictionLess;

            //// validate the create payment session API
            CreateAndAuthenticateResponse res = CreateAndAuthenticatePaymentSession(testAcc, paymentSessionData, true, true, PaymentChallengeStatus.Succeeded, false);

            //// validate the create and authenticate payment session API
            GetAuthenticationStatus(testAcc, res.PaymentSession.Id, true, true, PaymentChallengeStatus.Succeeded);
        }

        [TestMethod]
        [TestCategory(TestCategory.OneBoxAcceptanceTest)]
        [TestCategory(TestCategory.INTContinuousTest)]
        [TestCategory(TestCategory.PPEContinuousTest)]
        public void AuthenticationStatus_IsNotVerified_Frictionless()
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
            CreateAndAuthenticateResponse res = CreateAndAuthenticatePaymentSession(testAcc, paymentSessionData, true, true, PaymentChallengeStatus.Unknown, false);

            //// validate the create and authenticate payment session API
            GetAuthenticationStatus(testAcc, res.PaymentSession.Id, true, false, PaymentChallengeStatus.Unknown);
        }

        public string CreatePaymentSession(
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

        public CreateAndAuthenticateResponse CreateAndAuthenticatePaymentSession(
             AccountInfo testAcc,
             PaymentSessionData data,
             bool includeTestHeader,
             bool expectedIsChallengeRequired = true,
             PaymentChallengeStatus expectedStatus = PaymentChallengeStatus.Unknown,
             bool isMoto = false)
        {
            data.PaymentInstrumentAccountId = testAcc.AccountId;
            data.PaymentInstrumentId = testAcc.CreditCardPiid;
            CreateAndAuthenticateRequest request = new CreateAndAuthenticateRequest()
            {
                PaymentSessionData = data,
                AuthenticateRequest = GetAuthenticationRequest()
            };

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

            HttpStatusCode code = HttpStatusCode.Unused;
            string body = null;
            this.ExecuteRequest(
                string.Format(
                    "v7.0/{0}/paymentSessions/createAndAuthenticate",
                    testAcc.AccountId),
                HttpMethod.Post,
                tc,
                request,
                headers,
                (responseCode, responseBody) =>
                {
                    code = responseCode;
                    body = responseBody.ToString();
                });

            Assert.AreEqual(HttpStatusCode.OK, code);
            var resp = JsonConvert.DeserializeObject<CreateAndAuthenticateResponse>(body);
            Assert.IsNotNull(resp?.PaymentSession?.Id);
            Assert.AreEqual(expectedIsChallengeRequired, resp?.PaymentSession.IsChallengeRequired);
            Assert.AreEqual(expectedStatus, resp?.PaymentSession.ChallengeStatus);

            return resp;
        }

        public static AuthenticationRequest GetAuthenticationRequest()
        {
            var authenticationRequest = new AuthenticationRequest
            {
                SdkAppId = "e6284868-f62b-4186-9989-e500362c4f7b",
                SdkMaxTimeout = "60",
                SdkReferenceNumber = "3DS_LOA_SDK_MICO_020100_00169",
                SdkTransID = Guid.NewGuid().ToString(),
                SdkInterface = "03",
                SdkEphemPublicKey = new PayerAuth.EphemPublicKey
                {
                    Kty = "EC",
                    Crv = "P-256",
                    X = "xp1tW4PXn-cNpC6Jo9jGSjjN1OC-fc6RQNSom2nVklM",
                    Y = "NrRH6jr8CQEbVQxpAMUkq0U7_oAYGn1avSEi9V25E4M"
                },
                SdkUiType = new List<string>() { "01", "02", "03", "04", "05" },
                SdkEncData = "eyJhbGciOiJSU0EtT0FFUC0yNTYiLCJlbmMiOiJBMTI4R0NNIn0.mkXezz0RGr8Ifx1jPTaQfDbiqqB4PIM9rUax4hXiolS1jgN2L2FLUpfjgX9r1u2rshQn7_Ct0bYuQ4WCQEZOUZ3yIJRqPpbedBfz8iCdn7odRL3gGywunyVCQdD4 mtFTZH0De14-x-d8mnRFxjZGuRJP4EB-wu1AdY6IEtA2lvie4YxEYNDwEvT4cTPA785fGIN1P9YaZxMWBWfKlp1YwwhkmSa7eoMWTU9GEnMQqSDibmz9g4sVeFMehDB4Au1L01lAJH3WBhJFe6wmUe2LVBQ4PyAKMGKAX_QSJY9z nHTXf7AnrA7BB69xjwPC96Xy53v1vYsrO-TD2KRt8w.oSuF_11TJ_stm0-W.HxYOeUu8u5iUrPZ-VZcu6IrxU3bJN6utqEOKO0z_Y4IXjCAvzqxwlFc9L_CkV4iADbPykK1I59RBvXbUEbVKPH0xEA6b_f5ts1J6xHg3Rhlul1tD4SAOjj5_KFJ7sGDAILdKiP7zPME5Jp6djcnXLnE547eBINxZqJ2hMWmvxauGJThxF3OR2ju5uyGpngKfQsqUIY6NDM0tqYKKZTJXtIjS5abCsd98qQv83nWZaDN02vcSTyPnog7_P34zG-sZ02xk223lNzzUcNk2fZGXbi_UeyjjH98fsDhSIKDK6T4062emVoNUCDuxpGhfr1bE9AabuQsFeRzXvOSJ76eWMoTb2ClRsoqWCJhzA1DkmkS9tv5xADHxmLT_VuEsepJ1 V45Z5WI0FyzNt2vId3ILuExcGRr-gfcHHnACLqRqfPi4PrboKnA-_zaxZvrBZr9AZURJqvVNcXaEtZqEoXQaomh9XFKnhoE9_HylEJX-1UXIaKK1ccuazD5r3N6jWCFXsmy1qRrnKXg6fzGvDYfTO0z1L9KAiS5w8za3FlCbJLyqV6kJ8GU8iK1dx75_KtPbqqeazpTRHQ3BfzGE9QoKqAmv26RmNd4HkCFeUEVTkARIK4g1hL8 AZVJlIw40taK-3CJFG69-Lqlm0HyaHyZHaWOHeLZzhfEuEc4KabbCgrYXSFmeIxhY1jWx0s7bA9njLYOGsuZ8rt2ghcPJZuWPGRaZ38k65Lf6m VuEYWnuUxoS4j-OINPQJQiFFSCUORr42Ujh_cCMzOXUNJiyxe68TlEf57sbab1SYLg8V_kZQiKHowksxkTFjpUCpJTw6eq3PTELWiVDVsE71D43THQ9X6MLLuoyX9JmtbB8OkzqTnOSmoE5iSRqpipI1mHI3kd5jovVDeRvjS7ahsULumu21odVHG5V7o25P717V8r CD17frhtRPzlvQcRYoYCCrvn_S2_Ll6EJ4FUT9U7qoxLFC4dwzwtRSkYK9Gs_jk0fBgkRWwAHhNvVujghgkqhVFOK 3EDK5yOyeeG9WxOdT1tLAEUmrj939fOaP5Q16uqaUtbg7ZrfP_7mEe_he3teQPZIaFLO3oxDodBKuobBp90DrVmz6f1OyTR5tRgi8ntB98jqxbWiB-ogXtnXOvcCD9v-8NGISd5fE8RBleBG9g6zc9abLVCST5bcw4MEDt3bogOuoUhgC7axZGCHSWoJktIcHjncL69zi9t4FF4dnIXoBvOmgNfrq9SacvOq0CXalrUbOSr saBl-iykxQvZ_65MbGhxNIfvEIIZiU33PhL4XpoV6qxBCcKGUBIF5PvBOhT2rfAsg84_TvS7hA5tRsot3xY0ozfNGxGQAcvllz_o4bRzuZG_wPxNLG_7yiM8pClob4PPc_HKGhLbM_6jympV FP8KmmVhdD6TNlIBqFLI_HTxCOATttym7jMdyofix9b9CBvoFfHzOHguqwP_5DDC64RSyztyntGhwr5v0WWeJMOUe11Wb2ecjCH iWE-_sQaABfTSxMWgERY8cG4rryQSG5K9EILknNnOYknbqLMMlpjGwnsnGRYJJ_zDCCIKkfIEE31PQ18_FVuiDfE9a7T5eS1XPY9VGUpLeyCB45jXqkhJ1j1YV97qYyGrcCy1tBMwAtKPA6HrSS1pzuce YM9VKshUJ-1rOXzUHCKD7SkF2gJbDTQI9MCFI-Kb6DwYmiPQVslOWbosHQc0mNOBoYopus6ulXVFQK6tD7e6Fk3GMV_tYJBCTsbCoUhHS3X-jtZVvg1Q9-1Q7MKZJYNg7nldq5LYztx_yCQhpmfcA4x1n9oNKKnv5FWwYGEwZFFmpV-i3DM9gooVAbjVoDkfRrMNt6r0hYdquGxXTXBJxXa315NhXeMDDFdCQalFTesVDIMbptNLGQ_5G9pVVlaWfK3dQDcZRmcsknePLDKDooqjPsoz6Yv8HoGvtQGDTSkkCyVks1oOBhOaqa7uNBXJL5hA3MiJaoCogX2mrNFT02THUyAvWpWUUKbGP_Nxxc-TfsJAaj2iW_LpkC4kwCtzY6xkxOik6aRxWMdcgY9xN5XkOU7uLLhIcQCX9hTwtBPxiYRAzgueZrsk4_MTy6GOcOaQK0uflGs9g8BirpUUkwDwFI 7OV6oVJOzcNriIsFrP5b6IrBLanE-AdofCvEgn15ztHfAUatwl_-IG9WTL1N-ib4MrGCkLDfrEgBs-1OKUUisLfkdmG3foesNr7M4Laxq95fnsAoN4b_xc24Zov8Y516DBcKAPOpkZPj3KoAMAw6c4yvpqvVStm9E4i_Mgz vdnS3i9Cc9YqhVKuvzmAhhQut4Kse1O5TRb1SEmnkDZfg4pHekt_NkW7PjH0fV6U0nTsC97R1q5fPzYeGzaNQEBY0CwOU58o9nKbrUtLwuX0WMpTsENrlvEJOaihtOEsT3X_zM_0NTlMlFUB8uPEU Mf006jxNVpa4aGAc1bUKRWqdSsnhoAxKrZsUugqAV4jhPvoMvPBWnndQ5FAaJMqg34k8CgPbq1ejtk8YLPWn1bFggiqSD6bnkmYF7viYbscwzpzN19_WXqy8fvbUhzlpGmrfpsUFL58lc0--Wm78QefAoGB7t-zTelnQu_KHH6rQ36LvCsC_Q_DPgKjMKz3RcV3qia3tpt6ueI5Ep1QCYld8CHTOR1CWrwdPT9GdL9IFWft9ImuGKex2MwyY2RYVL LL7OdNGaeteloq8C37pAmLAUqrfZZ5k43hSQfEp-J5I3pKKDdwz24iRIVbLJ3Hnoma4a82ibELpU20Gx6XFy_LfU9O2RJP_MPxjNuq4yAj90dEBAzXZ67_yF_Wb3UhtoWVX6YWdrK88_g2TUKeFzOY0h-9L7YVpyavW6W5HniRmrkGMHtYjqPDrwBuNzHkv0PPPrNQsnZBUz1U0OTiVYFtvU3zoeBDTWuMGZXNi7_WwQOAg WPMwK-5E1ZIoIVRrWIYOglpIeuNLlZoQe0TQjYKHv4hKawa4DLLgW4el0Q4aagCNVaMvJiTEaMtk4miQKPxHXtffRMC2e1TUZRiRcGBxgpc9vPmR_fWYEI5FJpUByiMqrNgpEzhfriiunHKDR561-PGR4jzjD7_Wh8NRvyllKapW-5RB_G2DhQtLetJ7NaCTjwbcKKpxGCYQevlr-tccxAlE7tvHzahSztpKhftyor2dlJU4BtGNMMU8lH0xHiypzbEY13X_R5e6RFfd1lQVPlEPNCXStZvaZyxoODYTmMKbJE9SdU4ZFi103D6rk3fG4SpdOhFnvfwaplxFc4iOaIOiCj3qomFuRVCslB8W7Av41DTe_kjJjPKzuYj4d5pMdl 1QRVjRirGbppQfRdHN5SUP7ApVFHbPUXFYWTF0kOji8uCuKjooJTkY9kDWAWDk1x3yCMut9JfPCGs0d5xhn96bYywDUm8xJa1gmD6lwyvQhDykN Xv08TxOL_6rGuT1_2u6ZgV2gqnBEd5unneumm1-Nc3n5-OpLaAavO4V9y9aHCk0yC2ayo7Gerw0NTs1PMZQsLQ0CI3cYaMaKQFFRYlZ5NVGzsxe753avu6Q89XCRx5POKS4i1JbLDVtWVPVZvJ9knJ4J0Mday_xBG5Uu-Dt43g4uxR-qP7h3Vy_yFVQX97OcBwPwup7hKvQHOwejO6cyVGKj0g91_Kr0CerthKCx4XOi4PT102m1h0hb1Mba4xVcZpnXoEvLxzk2oNygtlMF-QTu2XEJpPdAEb9fsTvmX-IBLxenc0am-IENg.mky3EOKdPsrBe6vIMNx87g",
                MessageVersion = "2.1.0"
            };
            return authenticationRequest;
        }

        public AuthenticationStatus GetAuthenticationStatus(
            AccountInfo testAcc,
            string sessionId,
            bool includeTestHeader,
            bool expectedVerified,
            PaymentChallengeStatus expectedChallengeStatus)
        {
            Common.TestContext tc = includeTestHeader ?
                new Common.TestContext(
                    contact: "px.azure.cot",
                    retention: DateTime.MaxValue,
                    scenarios: "px-service-psd2-e2e-emulator")
                : null;

            var headers = new Dictionary<string, string>();

            HttpStatusCode code = HttpStatusCode.Unused;
            string body = null;
            this.ExecuteRequest(
                string.Format("v7.0/{0}/paymentSessions/{1}/{2}/authenticationStatus", testAcc.AccountId, sessionId, testAcc.CreditCardPiid),
                HttpMethod.Get,
                tc,
                null,
                headers,
                (responseCode, responseBody) =>
                {
                    code = responseCode;
                    body = responseBody?.ToString();
                });

            Assert.AreEqual(HttpStatusCode.OK, code, "code");
            Assert.IsNotNull(body, "body");
            var resp = JsonConvert.DeserializeObject<AuthenticationStatus>(body);
            Assert.IsNotNull(resp, "resp");
            Assert.AreEqual(sessionId, resp.SessionId, "SessionId");
            Assert.AreEqual(testAcc.CreditCardPiid, resp.PiId, "PiId");
            Assert.AreEqual(expectedVerified, resp.Verified, "Verified");
            Assert.AreEqual(expectedChallengeStatus, resp.ChallengeStatus, "ChallengeStatus");

            return resp;
        }
    }
}
