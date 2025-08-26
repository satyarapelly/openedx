// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2019. All rights reserved.</copyright>
namespace CIT.PXService.Tests
{
    using global::Tests.Common.Model;
    using global::Tests.Common.Model.Pidl;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PXService;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model;
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using static Microsoft.Commerce.Payments.PidlFactory.GlobalConstants.ServiceContextKeys;
    using static PXService.GlobalConstants;
    using PXPayerAuthServiceModel = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService;

    [TestClass]
    public class PaymentSessionsTests : TestBase
    {
        private const string DefaultVersion = "V11";
        private const string NewerVersion = "V25";
        private const string OlderVersion = "V6";

        [TestInitialize]
        public void TestInitialize()
        {
            // Set flights
            PXFlightHandler.AddToEnabledFlights("PXPSD2ProdIntegration");
        }

        /// <summary>
        /// PSD2 SafetyNet (CallSafetyNetOperation) is ON and POST PayerAuth/Authenticate returns 400
        /// </summary>
        [TestMethod]
        [DataRow(false, false)]
        [DataRow(false, true)]
        [DataRow(true, false)]
        [DataRow(true, true)]
        public async Task ChallengeDescriptions_ReturnsFingerprint(bool skipFingerprint, bool return3DSMethodURL)
        {
            // Arrange
            PXSettings.PimsService.ResponseProvider.RequiredChallenges = new List<string>() { "3ds2" };

            if (return3DSMethodURL)
            {
                PXSettings.PayerAuthService.ResponseProvider.ThreeDSMethodUrl = "https://test.com";
            }
            else
            {
                PXSettings.PayerAuthService.ResponseProvider.ThreeDSMethodUrl = string.Empty;
            }

            // Act
            if (skipFingerprint)
            {
                PXFlightHandler.AddToEnabledFlights("PXPSD2SkipFingerprint");
            }

            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/Account001/challengeDescriptions?operation=RenderPidlPage&language=en-GB&timezoneOffset=0&paymentSessionOrData={0}",
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                            new PaymentSessionData()
                            {
                                Amount = 10.0m,
                                Currency = "EUR",
                                Partner = "webblends",
                                Country = "gb",
                                PaymentInstrumentId = "Account001-Pi001-Visa",
                                Language = "en"
                            })))));

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);

            if (!skipFingerprint && return3DSMethodURL)
            {
                var pidl = ReadPidlResourceFromJson(await pxResponse.Content.ReadAsStringAsync());
                Assert.AreEqual("page", pidl[0].DisplayPages[0].DisplayHintType);
                Assert.AreEqual("iframe", pidl[0].DisplayPages[0].Members[0].DisplayHintType);

                var iFrameHint = pidl[0].DisplayPages[0].Members[0] as IFrameDisplayHint;
                Assert.AreEqual(60000, iFrameHint.MessageTimeout);
                Assert.AreEqual("Pidl", iFrameHint.MessageTimeoutClientAction.ActionType.ToString());
            }
            else
            {
                var pidl = ReadPidlResourceFromJson(await pxResponse.Content.ReadAsStringAsync());
                Assert.AreEqual(ClientActionType.ReturnContext, pidl[0].ClientAction.ActionType);
                Assert.IsNotNull(pidl[0].ClientAction.Context);
            }
        }

        /// <summary>
        /// CustomerId in the paymentSessionData object (PaymentInstrumentAccountId) does not match the CustomerId in the url
        /// </summary>
        [TestMethod]
        public async Task PaymentSessionDescriptions_AccountIdsInTokenAndPayloadDontMatch()
        {
            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/Account001/PaymentSessionDescriptions?operation=Add&language=en-GB&paymentSessionData={0}",
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                            new PaymentSessionData()
                            {
                                Amount = 10.0m,
                                Currency = "EUR",
                                Partner = "webblends",
                                Country = "gb",
                                PaymentInstrumentId = "Account001-Pi001-Visa",
                                Language = "en",
                                PaymentInstrumentAccountId = "NotAccount001"
                            })))));

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);
        }

        /// <summary>
        /// PI in the paymentSessionData object does not belong to customer id in the url
        /// </summary>
        [TestMethod]
        public async Task PaymentSessionDescriptions_AccountIdAndPiidMismatch()
        {
            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/Account001/PaymentSessionDescriptions?operation=Add&language=en-GB&paymentSessionData={0}",
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                            new PaymentSessionData()
                            {
                                Amount = 10.0m,
                                Currency = "EUR",
                                Partner = "webblends",
                                Country = "gb",
                                PaymentInstrumentId = "Account002-Pi001-Visa",
                                Language = "en"
                            })))));

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, pxResponse.StatusCode);

            var error = JsonConvert.DeserializeObject<ErrorResponse>(await pxResponse.Content.ReadAsStringAsync());
            Assert.AreEqual("PaymentInstrumentNotFound", error.ErrorCode);
            Assert.AreEqual("Caller is not authorized to access specified PaymentInstrumentId", error.Message);
        }

        /// <summary>
        /// PI in the paymentSessionData object requires a 3DS2 challenge
        /// </summary>
        [TestMethod]
        public async Task PaymentSessionDescriptions_PIRequires3DS2Challenge()
        {
            // Arrange
            PXSettings.PimsService.ResponseProvider.RequiredChallenges = new List<string>() { "3ds2" };
            PXSettings.PayerAuthService.ResponseProvider.SessionId = Guid.NewGuid().ToString();

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/Account001/PaymentSessionDescriptions?operation=Add&language=en-GB&paymentSessionData={0}",
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                            new PaymentSessionData()
                            {
                                Amount = 10.0m,
                                Currency = "EUR",
                                Partner = "webblends",
                                Country = "gb",
                                PaymentInstrumentId = "Account001-Pi001-Visa",
                                Language = "en"
                            })))));

            // Assert
            /*
            Assert.AreEqual("GET https://mockpims/v4.0/testCid/paymentInstruments/testPiid", outgoingRequests[0], true);
            Assert.AreEqual("GET https://mockpims/v4.0/testCid/paymentInstruments/testPiid/extendedView", outgoingRequests[1], true);
            Assert.AreEqual("POST https://mockPayerAuthService/CreatePaymentSessionId", outgoingRequests[2], true);
            Assert.AreEqual("POST https://mockSessionService/sessionservice/sessions/" + , outgoingRequests[3], true);
            */

            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);

            var pidl = ReadPidlResourceFromJson(await pxResponse.Content.ReadAsStringAsync());
            Assert.AreEqual(global::Tests.Common.Model.Pidl.ClientActionType.ReturnContext, pidl[0].ClientAction.ActionType);

            var paymentSession = JsonConvert.DeserializeObject<PaymentSession>(pidl[0].ClientAction.Context.ToString());
            Assert.AreEqual(true, paymentSession.IsChallengeRequired);
            Assert.AreEqual(PaymentChallengeStatus.Unknown, paymentSession.ChallengeStatus);
        }

        /// <summary>
        /// PI in the paymentSessionData object does not require a 3DS2 challenge
        /// </summary>
        [TestMethod]
        public async Task PaymentSessionDescriptions_PIDoesNotRequire3DS2Challenge()
        {
            // Arrange
            PXSettings.PayerAuthService.ResponseProvider.SessionId = Guid.NewGuid().ToString();

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/Account001/PaymentSessionDescriptions?operation=Add&language=en-GB&paymentSessionData={0}",
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                            new PaymentSessionData()
                            {
                                Amount = 10.0m,
                                Currency = "EUR",
                                Partner = "xbox",
                                Country = "gb",
                                PaymentInstrumentId = "Account001-Pi001-Visa",
                                Language = "en"
                            })))));

            // Assert
            /*
            Assert.AreEqual("GET https://mockpims/v4.0/testCid/paymentInstruments/testPiid", outgoingRequests[0], true);
            Assert.AreEqual("GET https://mockpims/v4.0/testCid/paymentInstruments/testPiid/extendedView", outgoingRequests[1], true);
            Assert.AreEqual("POST https://mockPayerAuthService/CreatePaymentSessionId", outgoingRequests[2], true);
            Assert.AreEqual("POST https://mockSessionService/sessionservice/sessions/" + sessionId, outgoingRequests[3], true);
            */

            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);

            var pidl = ReadPidlResourceFromJson(await pxResponse.Content.ReadAsStringAsync());
            Assert.AreEqual(global::Tests.Common.Model.Pidl.ClientActionType.ReturnContext, pidl[0].ClientAction.ActionType);

            var paymentSession = JsonConvert.DeserializeObject<PaymentSession>(pidl[0].ClientAction.Context.ToString());
            Assert.AreEqual(false, paymentSession.IsChallengeRequired);
            Assert.AreEqual(PaymentChallengeStatus.NotApplicable, paymentSession.ChallengeStatus);
        }

        /// <summary>
        /// Flight PXPSD2PretendPIMSReturned3DS2 is ON and PI does not require a 3DS2 challenge
        /// <paramref name="partner"/> is used to determine if the partner is OfficeSMB
        /// <paramref name="pxPSD2PretendPIMSReturned3DS2FlightStatus"/> is used to determine if the flight PXPSD2PretendPIMSReturned3DS2 is enabled
        /// <paramref name="isFeatureEnableValidatePIOnAttachChallenge"/> is used to determine if the feature PXUsePSSToEnableValidatePIOnAttachChallenge is enabled
        /// </summary>
        [DataRow("officesmb", true, true)]
        [DataRow("officesmb", false, true)]
        [DataRow("officesmb", true)]
        [DataRow("officesmb", false)]
        [DataRow("webblends", true)]
        [DataRow("webblends", false)]
        [TestMethod]
        public async Task PaymentSessionDescriptions_PXPSD2PretendPIMSReturned3DS2_PIDoesNotRequire3DS2Challenge(string partner, bool pxPSD2PretendPIMSReturned3DS2FlightStatus, bool isFeatureEnableValidatePIOnAttachChallenge = false)
        {
            // Arrange
            PXSettings.PayerAuthService.ResponseProvider.SessionId = Guid.NewGuid().ToString();

            if (pxPSD2PretendPIMSReturned3DS2FlightStatus)
            {
                PXFlightHandler.AddToEnabledFlights("PXPSD2PretendPIMSReturned3DS2");
            }

            if (string.Equals(partner, PXService.GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase))
            {
                string expectedPSSResponse = isFeatureEnableValidatePIOnAttachChallenge
                    ? "{\"handlePaymentChallenge\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"features\":{\"PXUsePSSToEnableValidatePIOnAttachChallenge\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]}}}}"
                    : "{\"handlePaymentChallenge\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\"}}";

                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
            }

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/Account001/PaymentSessionDescriptions?operation=Add&language=en-GB&paymentSessionData={0}",
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                            new PaymentSessionData()
                            {
                                Amount = 10.0m,
                                Currency = "EUR",
                                Partner = partner,
                                Country = "gb",
                                PaymentInstrumentId = "Account001-Pi001-Visa",
                                Language = "en"
                            })))));

            // Assert
            /*
            Assert.AreEqual("GET https://mockpims/v4.0/testCid/paymentInstruments/testPiid", outgoingRequests[0], true);
            Assert.AreEqual("GET https://mockpims/v4.0/testCid/paymentInstruments/testPiid/extendedView", outgoingRequests[1], true);
            Assert.AreEqual("POST https://mockPayerAuthService/CreatePaymentSessionId", outgoingRequests[2], true);
            Assert.AreEqual("POST https://mockSessionService/sessionservice/sessions/" + sessionId, outgoingRequests[3], true);
            */

            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);

            var pidl = ReadPidlResourceFromJson(await pxResponse.Content.ReadAsStringAsync());
            Assert.AreEqual(global::Tests.Common.Model.Pidl.ClientActionType.ReturnContext, pidl[0].ClientAction.ActionType);

            var paymentSession = JsonConvert.DeserializeObject<PaymentSession>(pidl[0].ClientAction.Context.ToString());

            if (string.Equals(partner, PXService.GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase) && !isFeatureEnableValidatePIOnAttachChallenge)
            {
                Assert.IsNull(paymentSession.ChallengeType);
                Assert.AreEqual(pxPSD2PretendPIMSReturned3DS2FlightStatus ? PaymentChallengeStatus.Unknown : PaymentChallengeStatus.NotApplicable, paymentSession.ChallengeStatus);
            }
            else
            {
                Assert.IsTrue(paymentSession.IsChallengeRequired, "Expected IsChallengeRequired to be true.");
                Assert.AreEqual(PaymentChallengeStatus.Unknown, paymentSession.ChallengeStatus, "Expected ChallengeStatus to be Unknown.");
                Assert.AreEqual(pxPSD2PretendPIMSReturned3DS2FlightStatus ? "PSD2Challenge" : "ValidatePIOnAttachChallenge", paymentSession.ChallengeType, "Unexpected ChallengeType.");
            }
        }

        /// <summary>
        /// Create payment session using either original or V2 PaymentSessionsHandler
        /// </summary>
        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task PaymentSessionDescriptions_TestV2HandlerWithFlighting(bool usePaymentSessionsHandlerV2)
        {
            // Arrange
            PXSettings.PayerAuthService.ArrangeResponse(
                 method: HttpMethod.Post,
                urlPattern: ".*/CreatePaymentSessionId.*",
                statusCode: HttpStatusCode.OK,
                content: "{ \"payment_session_id\" : \"1234\" }");

            string payAuthResp = "{\"enrollment_status\":\"bypassed\"}";
            PXSettings.PayerAuthService.ArrangeResponse(
                method: HttpMethod.Post,
                urlPattern: ".*/Authenticate.*",
                statusCode: HttpStatusCode.OK,
                content: payAuthResp);

            if (usePaymentSessionsHandlerV2)
            {
                PXFlightHandler.AddToEnabledFlights("PXUsePaymentSessionsHandlerV2");
            }

            // Act
            var pxResponse = await PXClient.PostAsync(
                GetPXServiceUrl("/v7.0/Account001/paymentSessions"),
                new StringContent(
                    JsonConvert.SerializeObject(
                    new PaymentSessionData()
                    {
                        Amount = 10.0m,
                        Currency = "USD",
                        Partner = "xbet",
                        Country = "us",
                        PaymentInstrumentId = "Account001-Pi002-MC",
                        Language = "en"
                    })));

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);
            string res = await pxResponse.Content.ReadAsStringAsync();
            PaymentSession session = JsonConvert.DeserializeObject<PaymentSession>(await pxResponse.Content.ReadAsStringAsync());

            Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession internalSession = await PXSettings.SessionServiceAccessor.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(
                    session.Id,
                    new Microsoft.Commerce.Tracing.EventTraceActivity());

            if (usePaymentSessionsHandlerV2)
            {
                Assert.AreEqual("V2", internalSession.HandlerVersion);
            }
            else
            {
                Assert.AreEqual("V1", internalSession.HandlerVersion);
            }

            // Clean upSessionService.ArrangeResponse
            PXSettings.PayerAuthService.Dispose();
        }

        /// <summary>
        /// Payment session with xbox rewards point through paymentSession API used by xbet.
        /// </summary>
        [TestMethod]
        public async Task PaymentSessionDescriptions_XboxRedeemRewards_PostCreateSession()
        {
            // Arrange
            PXSettings.PayerAuthService.ArrangeResponse(
                 method: HttpMethod.Post,
                urlPattern: ".*/CreatePaymentSessionId.*",
                statusCode: HttpStatusCode.OK,
                content: "{ \"payment_session_id\" : \"1234\" }");

            string payAuthResp = "{\"enrollment_status\":\"bypassed\"}";
            PXSettings.PayerAuthService.ArrangeResponse(
                method: HttpMethod.Post,
                urlPattern: ".*/Authenticate.*",
                statusCode: HttpStatusCode.OK,
                content: payAuthResp);

            bool redeemRewards = true;
            int rewardsPoints = 50;

            // Act
            var pxResponse = await PXClient.PostAsync(
                GetPXServiceUrl("/v7.0/Account001/paymentSessions"),
                new StringContent(
                    JsonConvert.SerializeObject(
                    new PaymentSessionData()
                    {
                        Amount = 10.0m,
                        Currency = "USD",
                        Partner = "xbet",
                        Country = "us",
                        PaymentInstrumentId = "Account001-Pi002-MC",
                        Language = "en",
                        RewardsPoints = rewardsPoints,
                        RedeemRewards = redeemRewards
                    })));

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);
            PaymentSession session = JsonConvert.DeserializeObject<PaymentSession>(await pxResponse.Content.ReadAsStringAsync());
            Assert.IsTrue(session.RedeemRewards);
            Assert.AreEqual(session.RewardsPoints, rewardsPoints);

            // Clean upSessionService.ArrangeResponse
            PXSettings.PayerAuthService.Dispose();
        }

        /// <summary>
        /// Payment session with xbox rewards points through paymentSession endpoint used by xboxsubs.
        /// </summary>
        [TestMethod]
        public async Task PaymentSessionDescriptions_XboxRedeemRewards_CreateAndAuthenticate()
        {
            // Arrange
            PXSettings.PayerAuthService.ArrangeResponse(
                 method: HttpMethod.Post,
                urlPattern: ".*/CreatePaymentSessionId.*",
                statusCode: HttpStatusCode.OK,
                content: "{ \"payment_session_id\" : \"1234\" }");

            string payAuthResp = "{\"enrollment_status\":\"bypassed\"}";
            PXSettings.PayerAuthService.ArrangeResponse(
                method: HttpMethod.Post,
                urlPattern: ".*/Authenticate.*",
                statusCode: HttpStatusCode.OK,
                content: payAuthResp);

            bool redeemRewards = true;
            int rewardsPoints = 50;

            var authRequest = new AuthenticationRequest()
            {
                SdkAppId = "blah",
                SdkEncData = "foo",
                SdkEphemPublicKey = new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.EphemPublicKey()
                {
                    Crv = "ba",
                    Kty = "ba",
                    X = "1.0",
                    Y = "4.0"
                },
                SdkMaxTimeout = "10",
                SdkInterface = "baz",
                SdkReferenceNumber = "Ref",
                SdkTransID = "transId",
                SdkUiType = new List<string> { "01" },
                SettingsVersion = DefaultVersion,
                SettingsVersionTryCount = 1
            };

            // Act
            var pxResponse = await PXClient.PostAsync(
                GetPXServiceUrl("/v7.0/Account001/paymentSessions/createAndAuthenticate"),
                new StringContent(
                    JsonConvert.SerializeObject(
                        new CreateAndAuthenticateRequest()
                        {
                            PaymentSessionData = new PaymentSessionData()
                            {
                                Amount = 10.0m,
                                Currency = "USD",
                                Partner = "xboxsubs",
                                Country = "us",
                                PaymentInstrumentId = "Account001-Pi002-MC",
                                Language = "en",
                                RewardsPoints = rewardsPoints,
                                RedeemRewards = redeemRewards
                            },
                            AuthenticateRequest = authRequest
                        })));

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);
            CreateAndAuthenticateResponse session = JsonConvert.DeserializeObject<CreateAndAuthenticateResponse>(await pxResponse.Content.ReadAsStringAsync());
            Assert.IsTrue(session.PaymentSession.RedeemRewards);
            Assert.AreEqual(session.PaymentSession.RewardsPoints, rewardsPoints);

            // Clean upSessionService.ArrangeResponse
            PXSettings.PayerAuthService.Dispose();
        }

        /// <summary>
        /// Payment session with xbox rewards points through CreatePaymentSession pidl component
        /// </summary>
        [TestMethod]
        public async Task PaymentSessionDescriptions_XboxRedeemRewards_PaymentSessionDescriptionsController()
        {
            // Arrange
            PXSettings.PayerAuthService.ArrangeResponse(
                 method: HttpMethod.Post,
                urlPattern: ".*/CreatePaymentSessionId.*",
                statusCode: HttpStatusCode.OK,
                content: "{ \"payment_session_id\" : \"1234\" }");

            string payAuthResp = "{\"enrollment_status\":\"bypassed\"}";
            PXSettings.PayerAuthService.ArrangeResponse(
                method: HttpMethod.Post,
                urlPattern: ".*/Authenticate.*",
                statusCode: HttpStatusCode.OK,
                content: payAuthResp);

            bool redeemRewards = true;
            int rewardsPoints = 50;

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/Account001/PaymentSessionDescriptions?operation=Add&language=en-US&paymentSessionData={0}",
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                            new PaymentSessionData()
                            {
                                Amount = 10.0m,
                                Currency = "USD",
                                Partner = "cart",
                                Country = "us",
                                PaymentInstrumentId = "Account001-Pi002-MC",
                                Language = "en",
                                RewardsPoints = rewardsPoints,
                                RedeemRewards = redeemRewards
                            })))));

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);

            var pidl = ReadPidlResourceFromJson(await pxResponse.Content.ReadAsStringAsync());
            Assert.AreEqual(global::Tests.Common.Model.Pidl.ClientActionType.ReturnContext, pidl[0].ClientAction.ActionType);

            var paymentSession = JsonConvert.DeserializeObject<PaymentSession>(pidl[0].ClientAction.Context.ToString());
            Assert.IsTrue(paymentSession.RedeemRewards);
            Assert.AreEqual(paymentSession.RewardsPoints, rewardsPoints);

            // Clean upSessionService.ArrangeResponse
            PXSettings.PayerAuthService.Dispose();
        }

        /// <summary>
        /// PSD2 SafetyNet (CallSafetyNetOperation) is ON and POST /SessionService/CreateSession returns 400
        /// </summary>
        [TestMethod]
        public async Task PaymentSessionDescriptions_PXPSD2SafetyNet_PostCreateSession_400()
        {
            // Arrange
            PXSettings.SessionService.ArrangeResponse(
                method: HttpMethod.Post,
                urlPattern: ".*/sessions/.*",
                statusCode: HttpStatusCode.BadRequest,
                content: "{ \"error_code\" : \"someErrorCode\", \"message\" : \"someMessage\", \"error_source\" : \"some error source\" }");

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/Account001/PaymentSessionDescriptions?operation=Add&language=en-GB&paymentSessionData={0}",
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                            new PaymentSessionData()
                            {
                                Amount = 10.0m,
                                Currency = "EUR",
                                Partner = "webblends",
                                Country = "gb",
                                PaymentInstrumentId = "Account001-Pi001-Visa",
                                Language = "en"
                            })))));

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);

            var pidl = ReadPidlResourceFromJson(await pxResponse.Content.ReadAsStringAsync());
            Assert.AreEqual(global::Tests.Common.Model.Pidl.ClientActionType.ReturnContext, pidl[0].ClientAction.ActionType);

            var paymentSession = JsonConvert.DeserializeObject<PaymentSession>(pidl[0].ClientAction.Context.ToString());
            Assert.AreEqual(false, paymentSession.IsChallengeRequired);
            Assert.AreEqual(PaymentChallengeStatus.NotApplicable, paymentSession.ChallengeStatus);
        }

        /// <summary>
        /// PSD2 SafetyNet (CallSafetyNetOperation) is ON and GET PIMS/paymentInstruments/{piid} returns 400
        /// </summary>
        [TestMethod]
        public async Task PaymentSessionDescriptions_PXPSD2SafetyNet_GetPI_400()
        {
            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/Account001/PaymentSessionDescriptions?operation=Add&language=en-GB&paymentSessionData={0}",
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                            new PaymentSessionData()
                            {
                                Amount = 10.0m,
                                Currency = "EUR",
                                Partner = "webblends",
                                Country = "gb",
                                PaymentInstrumentId = "NonExistentPiId",
                                Language = "en"
                            })))));

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, pxResponse.StatusCode);

            var errorResponse = JsonConvert.DeserializeObject<ServiceErrorResponse>(await pxResponse.Content.ReadAsStringAsync());
            Assert.AreEqual("PaymentInstrumentNotFound", errorResponse.ErrorCode);
        }

        /// <summary>
        /// PSD2 SafetyNet (CallSafetyNetOperation) is ON and GET PIMS/paymentInstruments/{piid}/extended returns 400
        /// </summary>
        [TestMethod]
        public async Task PaymentSessionDescriptions_PXPSD2SafetyNet_GetPIExtended_400()
        {
            // Arrange
            PXSettings.PimsService.ArrangeResponse(
                method: HttpMethod.Get,
                urlPattern: ".*/extendedView.*",
                statusCode: HttpStatusCode.BadRequest,
                content: "{ \"ErrorCode\" : \"someError\", \"Message\" : \"Some message\", \"Source\" : \"Some error source\" }");

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/Account001/PaymentSessionDescriptions?operation=Add&language=en-GB&paymentSessionData={0}",
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                            new PaymentSessionData()
                            {
                                Amount = 10.0m,
                                Currency = "EUR",
                                Partner = "webblends",
                                Country = "gb",
                                PaymentInstrumentId = "Account001-Pi001-Visa",
                                Language = "en"
                            })))));

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);

            var pidl = ReadPidlResourceFromJson(await pxResponse.Content.ReadAsStringAsync());
            Assert.AreEqual(global::Tests.Common.Model.Pidl.ClientActionType.ReturnContext, pidl[0].ClientAction.ActionType);

            var paymentSession = JsonConvert.DeserializeObject<PaymentSession>(pidl[0].ClientAction.Context.ToString());
            Assert.AreEqual(false, paymentSession.IsChallengeRequired);
            Assert.AreEqual(PaymentChallengeStatus.NotApplicable, paymentSession.ChallengeStatus);
        }

        /// <summary>
        /// PSD2 SafetyNet (CallSafetyNetOperation) is ON and POST PayerAuth/CreatePaymentSessionId returns 400
        /// </summary>
        [TestMethod]
        public async Task PaymentSessionDescriptions_PXPSD2SafetyNet_PostCreatePaymentSessionId_400()
        {
            // Arrange
            PXSettings.PayerAuthService.ArrangeResponse(
                 method: HttpMethod.Post,
                urlPattern: ".*/CreatePaymentSessionId.*",
                statusCode: HttpStatusCode.BadRequest,
                content: "{ \"error_code\" : \"someError\", \"message\" : \"Some message\", \"error_source\" : \"Some error source\" }");

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/Account001/PaymentSessionDescriptions?operation=Add&language=en-GB&paymentSessionData={0}",
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                            new PaymentSessionData()
                            {
                                Amount = 10.0m,
                                Currency = "EUR",
                                Partner = "webblends",
                                Country = "gb",
                                PaymentInstrumentId = "Account001-Pi001-Visa",
                                Language = "en"
                            })))));

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);

            var pidl = ReadPidlResourceFromJson(await pxResponse.Content.ReadAsStringAsync());
            Assert.AreEqual(global::Tests.Common.Model.Pidl.ClientActionType.ReturnContext, pidl[0].ClientAction.ActionType);

            var paymentSession = JsonConvert.DeserializeObject<PaymentSession>(pidl[0].ClientAction.Context.ToString());
            Assert.AreEqual(false, paymentSession.IsChallengeRequired);
            Assert.AreEqual(PaymentChallengeStatus.NotApplicable, paymentSession.ChallengeStatus);
        }

        /// <summary>
        /// PSD2 SafetyNet (CallSafetyNetOperation) is ON and POST PayerAuth/Authenticate returns 400
        /// </summary>
        [TestMethod]
        public async Task PaymentSessionDescriptions_PXPSD2SafetyNet_PostAuthentication_400()
        {
            // Arrange
            PXSettings.PayerAuthService.ArrangeResponse(
                method: HttpMethod.Post,
                urlPattern: ".*/Authenticate.*",
                statusCode: HttpStatusCode.BadRequest,
                content: "{ \"error_code\" : \"someError\", \"message\" : \"Some message\", \"error_source\" : \"Some error source\" }");

            // Act
            PXClient.DefaultRequestHeaders.Add("x-ms-ismoto", "true");
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/Account001/PaymentSessionDescriptions?operation=Add&language=en-GB&paymentSessionData={0}",
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                            new PaymentSessionData()
                            {
                                Amount = 10.0m,
                                Currency = "EUR",
                                Partner = "webblends",
                                Country = "gb",
                                PaymentInstrumentId = "Account001-Pi001-Visa",
                                Language = "en",
                                IsMOTO = true
                            })))));

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);

            var pidl = ReadPidlResourceFromJson(await pxResponse.Content.ReadAsStringAsync());
            Assert.AreEqual(global::Tests.Common.Model.Pidl.ClientActionType.ReturnContext, pidl[0].ClientAction.ActionType);

            var paymentSession = JsonConvert.DeserializeObject<PaymentSession>(pidl[0].ClientAction.Context.ToString());
            Assert.AreEqual(false, paymentSession.IsChallengeRequired);
            Assert.AreEqual(PaymentChallengeStatus.ByPassed, paymentSession.ChallengeStatus);
        }

        /// <summary>
        /// PSD2 SafetyNet (CallSafetyNetOperation) is ON and POST PayerAuth/Authenticate returns 400
        /// </summary>
        [TestMethod]
        public async Task ChallengeDescriptions_PXPSD2SafetyNet_PostGetThreeDSMethodURL_400()
        {
            // Arrange
            PXSettings.PimsService.ResponseProvider.RequiredChallenges = new List<string>() { "3ds2" };

            PXSettings.PayerAuthService.ArrangeResponse(
                method: HttpMethod.Post,
                urlPattern: ".*/GetThreeDSMethodURL.*",
                statusCode: HttpStatusCode.BadRequest,
                content: "{ \"error_code\" : \"someError\", \"message\" : \"Some message\", \"error_source\" : \"Some error source\" }");

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/Account001/challengeDescriptions?operation=RenderPidlPage&language=en-GB&timezoneOffset=0&paymentSessionOrData={0}",
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                            new PaymentSessionData()
                            {
                                Amount = 10.0m,
                                Currency = "EUR",
                                Partner = "webblends",
                                Country = "gb",
                                PaymentInstrumentId = "Account001-Pi001-Visa",
                                Language = "en"
                            })))));

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);

            var pidl = ReadPidlResourceFromJson(await pxResponse.Content.ReadAsStringAsync());
            Assert.AreEqual(global::Tests.Common.Model.Pidl.ClientActionType.ReturnContext, pidl[0].ClientAction.ActionType);

            var paymentSession = JsonConvert.DeserializeObject<PaymentSession>(pidl[0].ClientAction.Context.ToString());
            Assert.AreEqual(true, paymentSession.IsChallengeRequired);
            Assert.AreEqual(PaymentChallengeStatus.Succeeded, paymentSession.ChallengeStatus);
        }

        [DataRow(Partners.Webblends, true, "IN", "INR", false)]
        [DataRow(Partners.Webblends, false, "IN", "INR", false)]
        [DataRow(Partners.Webblends, true, "SG", "SGD", false)]
        [DataRow(Partners.Webblends, false, "SG", "SGD", true)]
        [DataRow(Partners.OfficeSMB, true, "SG", "SGD", false, true)]
        [DataRow(Partners.OfficeSMB, false, "SG", "SGD", true, true)]
        [DataRow(Partners.OfficeSMB, true, "SG", "SGD", false)]
        [DataRow(Partners.OfficeSMB, false, "SG", "SGD", true)]
        [TestMethod]
        public async Task GetChallengeDescription_India3DSChallenge_DisableValidatePIOnAttachChallengeForIndiaMarketAndIndiaCard(string partnerName, bool isIndiaCard, string country, string currency, bool shouldEnablePIOnAttachChallenge, bool isFeatureEnableValidatePIOnAttachChallenge = false)
        {
            // Arrange
            string accountId = "Account001";
            string language = "en-us";
            string url = string.Format(
                        "/v7.0/{0}/challengeDescriptions?language=us-en&paymentSessionOrData={1}",
                        accountId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                                new PaymentSessionData()
                                {
                                    Amount = 100.0m,
                                    Currency = currency,
                                    Partner = partnerName,
                                    Country = country,
                                    PaymentInstrumentId = "Account001-Pi001-Visa",
                                    Language = language,
                                    ChallengeScenario = ChallengeScenario.PaymentTransaction
                                })));
            Dictionary<string, string> testHeader = new Dictionary<string, string>();
            if (isIndiaCard)
            {
                var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, "Account001-Pi001-Visa");
                extendedPI.PaymentInstrumentDetails.RequiredChallenge = new List<string> { "3ds" };
                PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");
            }

            var flights = new List<string> { "India3dsEnableForBilldesk" };

            if (string.Equals(partnerName, PXService.GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase))
            {
                string expectedPSSResponse = isFeatureEnableValidatePIOnAttachChallenge
                    ? "{\"handlePaymentChallenge\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"features\":{\"PXUsePSSToEnableValidatePIOnAttachChallenge\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]}}}}"
                    : "{\"handlePaymentChallenge\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\"}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
            }

            // Act
            await GetRequest(
                url,
                testHeader,
                flights,
                (responseCode, responseBody, responseHeaders) =>
                {
                    List<PIDLResource> pidls = ReadPidlResourceFromJson(responseBody);
                    Assert.IsNotNull(pidls);

                    // Assert
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                    var session = JsonConvert.DeserializeObject<PaymentSession>(pidls[0].ClientAction.Context.ToString());

                    if (string.Equals(partnerName, PXService.GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase) && !isFeatureEnableValidatePIOnAttachChallenge)
                    {
                        Assert.IsNull(session.ChallengeType);
                    }
                    else if (shouldEnablePIOnAttachChallenge)
                    {
                        Assert.AreEqual("ValidatePIOnAttachChallenge", session.ChallengeType);
                    }
                    else
                    {
                        Assert.AreEqual("NotApplicable", session.ChallengeStatus.ToString());
                    }
                });
        }

        [TestMethod]
        public async Task ChallengeDescriptions_TestDeviceInfoHeader()
        {
            // Arrange
            PXSettings.PimsService.ResponseProvider.RequiredChallenges = new List<string>() { "3ds2" };
            string browserIp = string.Empty;
            string browserUserAgent = string.Empty;
            string expectedIpAddress = "111.111.111.111";
            string expectedUserAgent = "{Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/11.11 (KHTML, like Gecko) Chrome/111.111.111.111 Safari/11.11 Edg/111.111.111.111}";
            string encodedIpAddress = System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(expectedIpAddress));
            string encodedUserAgent = System.Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(expectedUserAgent));

            // Captures browserInfo sent to SessionService
            PXSettings.SessionService.PreProcess = async (res) =>
            {
                if (!string.IsNullOrEmpty(browserIp) && !string.IsNullOrEmpty(browserUserAgent))
                {
                    return;
                }

                string contentString = await res.Content.ReadAsStringAsync();
                dynamic result = JObject.Parse(contentString);
                string data = result.Data;
                if (!string.IsNullOrEmpty(data))
                {
                    dynamic parsedData = JObject.Parse(data);
                    dynamic browserInfo;

                    if (parsedData != null)
                    {
                        browserInfo = parsedData.BrowserInfo;
                        if (browserInfo != null &&
                            browserInfo.ContainsKey("browser_ip") &&
                            browserInfo.ContainsKey("browser_user_agent"))
                        {
                            browserIp = browserInfo["browser_ip"];
                            browserUserAgent = browserInfo["browser_user_agent"];
                        }
                    }
                }
            };

            string url = GetPXServiceUrl(
                    string.Format(
                        "/v7.0/Account001/challengeDescriptions?operation=RenderPidlPage&language=en-GB&timezoneOffset=0&paymentSessionOrData={0}",
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                            new PaymentSessionData()
                            {
                                Amount = 10.0m,
                                Currency = "EUR",
                                Partner = "webblends",
                                Country = "gb",
                                PaymentInstrumentId = "Account001-Pi001-Visa",
                                Language = "en"
                            }))));

            string deviceInfo = string.Format("ipAddress={0},userAgent={1}", encodedIpAddress, encodedUserAgent);
            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-deviceinfo", deviceInfo
                },
                {
                    "x-ms-clientcontext-encoding", "base64"
                }
            };

            await GetRequest(
                url,
                headers,
                null,
                (responseCode, responseBody, responseHeaders) =>
                {
                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                });

            Assert.AreEqual(expectedIpAddress, browserIp);
            Assert.AreEqual(expectedUserAgent, browserUserAgent);
            PXSettings.SessionService.ResetToDefaults();
        }

        /// <summary>
        /// PSD2 SafetyNet (CallSafetyNetOperation) is ON and POST PayerAuth/Authenticate returns 400
        /// </summary>
        [TestMethod]
        public async Task ChallengeDescriptions_PXPSD2AuthContractChange_AuthResponds_Y_TSR01()
        {
            // Arrange
            PXSettings.PimsService.ResponseProvider.RequiredChallenges = new List<string>() { "3ds2" };

            PXSettings.PayerAuthService.ResponseProvider.ThreeDSMethodUrl = null;
            PXSettings.PayerAuthService.ResponseProvider.EnrollmentStatus = "Bypassed";
            PXSettings.PayerAuthService.ResponseProvider.TransStatus = "Y";
            PXSettings.PayerAuthService.ResponseProvider.TransStatusReason = "TSR03";

            PXFlightHandler.AddToEnabledFlights("PXPSD2Auth-Y-_-Bypassed, PXPSD2Auth-Y-TSR02-Failed, PXPSD2Auth-Y-TSR01-Notapplicable");

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/Account001/challengeDescriptions?operation=RenderPidlPage&language=en-GB&timezoneOffset=0&paymentSessionOrData={0}",
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                            new PaymentSessionData()
                            {
                                Amount = 10.0m,
                                Currency = "EUR",
                                Partner = "webblends",
                                Country = "gb",
                                PaymentInstrumentId = "Account001-Pi001-Visa",
                                Language = "en"
                            })))));

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);

            var pidl = ReadPidlResourceFromJson(await pxResponse.Content.ReadAsStringAsync());
            Assert.AreEqual(global::Tests.Common.Model.Pidl.ClientActionType.ReturnContext, pidl[0].ClientAction.ActionType);

            var paymentSession = JsonConvert.DeserializeObject<PaymentSession>(pidl[0].ClientAction.Context.ToString());
            Assert.AreEqual(true, paymentSession.IsChallengeRequired);
            Assert.AreEqual(PaymentChallengeStatus.ByPassed, paymentSession.ChallengeStatus);
        }

        // Consumer scenarios
        [DataRow(null, "Account001", HttpStatusCode.OK)]
        [DataRow("", "Account001", HttpStatusCode.OK)]
        [DataRow("Account001", "Account001", HttpStatusCode.OK)]
        [DataRow("MismatchedCid", "Account001", HttpStatusCode.OK)] // Unauthorized request.
        [TestMethod]
        public async Task ChallengeDescriptions_PaymentSession_Validates_PiCid(string piCid, string requetorCid, HttpStatusCode expectedStatusCode)
        {
            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/{0}/challengeDescriptions?operation=RenderPidlPage&language=en-GB&timezoneOffset=0&paymentSessionOrData={1}",
                        requetorCid,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                            new PaymentSessionData()
                            {
                                Amount = 10.0m,
                                Currency = "EUR",
                                Partner = "webblends",
                                Country = "gb",
                                PaymentInstrumentId = "Account001-Pi001-Visa",
                                Language = "en",
                                PaymentInstrumentAccountId = piCid
                            })))));

            // Assert
            Assert.AreEqual(expectedStatusCode, pxResponse.StatusCode);
        }

        // With an up-to-date version in the request, PX should never return a SettingsVersionMismatch error
        [DataRow(DefaultVersion, false, "", null)]
        [DataRow(DefaultVersion, true, "", null)]
        [DataRow(DefaultVersion, false, "PXPSD2SettingVersion" + DefaultVersion, null)]
        [DataRow(DefaultVersion, true, "PXPSD2SettingVersion" + DefaultVersion, null)]

        [DataRow(NewerVersion, false, "PXPSD2SettingVersion" + NewerVersion, null)]
        [DataRow(NewerVersion, true, "PXPSD2SettingVersion" + NewerVersion, null)]

        // When an upgrade is required, PX should always return a SettingsVersionMismatch error
        [DataRow(OlderVersion, false, "", DefaultVersion)]
        [DataRow(OlderVersion, false, "PXPSD2SettingVersion" + DefaultVersion, DefaultVersion)]
        [DataRow(OlderVersion, false, "PXPSD2SettingVersion" + DefaultVersion + ", " + "PXPSD2SettingVersion" + NewerVersion, NewerVersion)]
        [DataRow(OlderVersion, true, "", DefaultVersion)]
        [DataRow(OlderVersion, true, "PXPSD2SettingVersion" + DefaultVersion, DefaultVersion)]
        [DataRow(OlderVersion, true, "PXPSD2SettingVersion" + DefaultVersion + ", " + "PXPSD2SettingVersion" + NewerVersion, NewerVersion)]

        [DataRow(DefaultVersion, false, "PXPSD2SettingVersion" + NewerVersion, NewerVersion)]
        [DataRow(DefaultVersion, true, "PXPSD2SettingVersion" + NewerVersion, NewerVersion)]

        // When a downgrade is required, PX should return a SettingsVersionMismatch error
        [DataRow(NewerVersion, false, "", DefaultVersion)]
        [DataRow(NewerVersion, false, "PXPSD2SettingVersion" + DefaultVersion, DefaultVersion)]
        [DataRow(NewerVersion, false, "PXPSD2SettingVersion" + DefaultVersion + ", " + "PXPSD2SettingVersion" + OlderVersion, DefaultVersion)]
        [DataRow(NewerVersion, true, "", DefaultVersion)]
        [DataRow(NewerVersion, true, "PXPSD2SettingVersion" + DefaultVersion, DefaultVersion)]
        [DataRow(NewerVersion, true, "PXPSD2SettingVersion" + DefaultVersion + ", " + "PXPSD2SettingVersion" + OlderVersion, DefaultVersion)]
        [TestMethod]
        public async Task PaymentSessions_SettingsVersionMismatch_ReturnedAsExpected(string settingsVersion, bool isPsd2Pi, string flights, string targetVersion)
        {
            // Arrange
            if (isPsd2Pi)
            {
                PXSettings.PimsService.ResponseProvider.RequiredChallenges = new List<string>() { "3ds2" };
            }

            if (!string.IsNullOrEmpty(flights))
            {
                PXFlightHandler.AddToEnabledFlights(flights);
            }

            // Act
            var authRequest = new AuthenticationRequest()
            {
                SdkAppId = "blah",
                SdkEncData = "foo",
                SdkEphemPublicKey = new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.EphemPublicKey()
                {
                    Crv = "ba",
                    Kty = "ba",
                    X = "1.0",
                    Y = "4.0"
                },
                SdkMaxTimeout = "10",
                SdkInterface = "baz",
                SdkReferenceNumber = "Ref",
                SdkTransID = "transId",
                SdkUiType = new List<string> { "01" },
                SettingsVersion = settingsVersion,
                SettingsVersionTryCount = 1
            };

            var pxResponse = await PXClient.PostAsync(
                GetPXServiceUrl("/v7.0/Account001/paymentSessions/createAndAuthenticate"),
                new StringContent(
                    JsonConvert.SerializeObject(
                        new CreateAndAuthenticateRequest()
                        {
                            PaymentSessionData = new PaymentSessionData()
                            {
                                Amount = 10.0m,
                                Currency = "EUR",
                                Partner = "cart",
                                Country = "gb",
                                PaymentInstrumentId = "Account001-Pi001-Visa",
                                Language = "en",
                            },
                            AuthenticateRequest = authRequest
                        })));

            HttpResponseMessage pxAuthResponse = null;
            if (isPsd2Pi)
            {
                pxAuthResponse = await PXClient.PostAsync(
                    GetPXServiceUrl("/v7.0/Account001/paymentSessions/PaymentSession001/authenticate"),
                    new StringContent(JsonConvert.SerializeObject(authRequest)));
            }

            // Assert
            if (targetVersion != null)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, pxResponse.StatusCode);
                var error = JsonConvert.DeserializeObject<ServiceErrorResponse>(await pxResponse.Content.ReadAsStringAsync());
                Assert.AreEqual("SettingsVersionMismatch", error.ErrorCode);

                if (isPsd2Pi)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, pxAuthResponse.StatusCode);
                    var authError = JsonConvert.DeserializeObject<ServiceErrorResponse>(await pxAuthResponse.Content.ReadAsStringAsync());
                    Assert.AreEqual("SettingsVersionMismatch", authError.ErrorCode);
                }
            }
            else
            {
                Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);

                if (isPsd2Pi)
                {
                    Assert.AreEqual(HttpStatusCode.OK, pxAuthResponse.StatusCode);
                }
            }
        }

        [DataRow(Partners.Azure, true, "India3DSChallenge", PaymentChallengeStatus.Unknown)]
        [DataRow(Partners.CommercialStores, true, "India3DSChallenge", PaymentChallengeStatus.Unknown)]
        [DataRow(Partners.Cart, false, null, PaymentChallengeStatus.NotApplicable)]
        [DataRow(Partners.Webblends, false, null, PaymentChallengeStatus.NotApplicable)]
        [TestMethod]
        public async Task CreatePaymentSession_IndiaThreeDS(string partnerName, bool challengeExpected, string expectedChallengeType, PaymentChallengeStatus expectedChallengeStatus)
        {
            // Arrange
            string accountId = "Account001";
            string piid = "Account001-Pi001-Visa";

            var paymentSessionData = new PaymentSessionData()
            {
                PaymentInstrumentId = piid,
                Language = "en-us",
                Amount = 800,
                Partner = partnerName,
                Currency = "INR",
                Country = "in",
                ChallengeScenario = ChallengeScenario.PaymentTransaction
            };

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/{0}/paymentSessionDescriptions?paymentSessionData={1}",
                        accountId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                            paymentSessionData)))));

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);

            var pidl = ReadPidlResourceFromJson(await pxResponse.Content.ReadAsStringAsync());
            var paymentSession = JsonConvert.DeserializeObject<PaymentSession>(pidl[0].ClientAction.Context.ToString());

            Assert.AreEqual(piid, paymentSession.PaymentInstrumentId);
            Assert.AreEqual(challengeExpected, paymentSession.IsChallengeRequired);
            Assert.AreEqual(expectedChallengeType, paymentSession.ChallengeType);
            Assert.AreEqual(expectedChallengeStatus, paymentSession.ChallengeStatus);
        }

        [DataRow(Partners.Azure)]
        [DataRow(Partners.CommercialStores)]
        [TestMethod]
        public async Task PaymentSessions_AuthenticateIndiaThreeDS(string partner)
        {
            // Arrange
            string accountId = "Account001";
            string paymentSessionId = "ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6";

            string response = "{\"Id\":\"" + paymentSessionId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"Account001\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + paymentSessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"Account001-Pi001-Visa\\\",\\\"partner\\\":\\\"" + partner + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":false,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";
            PXSettings.SessionService.ResponseProvider.SessionStore.Add(paymentSessionId, response);

            PIDLData sampleData = new PIDLData();
            sampleData.Add("cvvToken", "123");

            // Act
            var pxResponse = await PXClient.PostAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/AuthenticateIndiaThreeDS", accountId, paymentSessionId)),
                new StringContent(JsonConvert.SerializeObject(sampleData), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);

            var responseBody = JsonConvert.DeserializeObject<PIDLResource>(await pxResponse.Content.ReadAsStringAsync());
            if (partner == Partners.Azure)
            {
                Assert.AreEqual(ClientActionType.Pidl, responseBody.ClientAction.ActionType);
            }
            else
            {
                Assert.AreEqual(ClientActionType.Redirect, responseBody.ClientAction.ActionType);
            }
        }

        [TestMethod]
        [DataRow("storify")]
        [DataRow("saturn")]
        [DataRow("xbox")]
        [DataRow("webblends")]
        [DataRow("webblends_inline")]
        [DataRow("amcweb")]
        [DataRow("xbet")]
        public async Task PaymentSessions_BrowserAuthenticateIndiaThreeDS(string partner)
        {
            // Arrange
            string accountId = "Account001";
            string paymentSessionId = "ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6";
            string response = "{\"Id\":\"" + paymentSessionId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"Account001\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + paymentSessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"Account001-Pi001-Visa\\\",\\\"partner\\\":\\\"" + partner + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":false,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

            string expectedRdsURL = "https://india3dssimpleredirectgroup.azurewebsites.net?ru=https%3A%2F%2Fwww.microsoft.com%2Fen-us%2Fstore%2Frds%2FPurchaseRiskChallengeRedirectSuccess&rx=https%3A%2F%2Fwww.microsoft.com%2Fen-us%2Fstore%2Frds%2FPurchaseRiskChallengeRedirectFailure";
            PXSettings.SessionService.ResponseProvider.SessionStore.Add(paymentSessionId, response);

            PIDLData sampleData = new PIDLData();
            sampleData.Add("cvvToken", "123");

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/browserAuthenticateThreeDSOne?partner={2}", accountId, paymentSessionId, partner)));
            request.Headers.Add("x-ms-test", "{\"scenarios\":\"px-service-3ds1-test-emulator\",}");
            request.Content = new StringContent(JsonConvert.SerializeObject(sampleData), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType);

            var pxResponse = await PXClient.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);

            var responseBody = JsonConvert.DeserializeObject<PIDLResource>(await pxResponse.Content.ReadAsStringAsync());
            List<string> qrCodePartners = new List<string> { "storify", "saturn", "xbox", "amcxbox", "xbet" };
            List<string> inlinePartners = new List<string> { "webblends_inline", "amcweb" };

            if (qrCodePartners.Contains(partner))
            {
                var pidl = ReadPidlResourceFromJson(JsonConvert.SerializeObject(responseBody.ClientAction.Context)).First();
                Assert.AreEqual(ClientActionType.Pidl, responseBody.ClientAction.ActionType);
                var qrCode = pidl.GetDisplayHintById("ccThreeDSQrCodeImage");
                Assert.IsNotNull(qrCode, "Missing qr code image");

                List<string> xboxNativePartners = new List<string> { "storify", "saturn", "xbet" };
                if (xboxNativePartners.Contains(partner))
                {
                    var bankIFrame = pidl.GetDisplayHintById("ThreeDSOneBankFrame") as IFrameDisplayHint;
                    Assert.IsNotNull(bankIFrame, "Missing bank iframe");
                    Assert.AreEqual(bankIFrame.SourceUrl, expectedRdsURL);

                    var moveBack2Button = pidl.GetDisplayHintById("moveBack2Button") as ButtonDisplayHint;
                    Assert.IsNotNull(moveBack2Button);
                    Assert.IsNotNull(moveBack2Button.DisplayTags["accessibilityName"]);
                    Assert.AreEqual(moveBack2Button.DisplayTags["accessibilityName"], "Back button 1 of 2");

                    var goToBankButton = pidl.GetDisplayHintById("goToBankButton") as ButtonDisplayHint;
                    Assert.IsNotNull(goToBankButton);
                    Assert.IsNotNull(goToBankButton.DisplayTags["accessibilityName"]);
                    Assert.AreEqual(goToBankButton.DisplayTags["accessibilityName"], "Can't use your phone to scan? Go to the bank website for verification. button 2 of 2");
                }
            }
            else if (inlinePartners.Contains(partner))
            {
                Assert.AreEqual(ClientActionType.Redirect, responseBody.ClientAction.ActionType);
            }
            else
            {
                Assert.AreEqual(ClientActionType.Pidl, responseBody.ClientAction.ActionType);
            }
        }

        [TestMethod]
        [DataRow("webblends")]
        public async Task PaymentSessions_BrowserAuthenticateIndiaThreeDS_Pass_Puid(string partner)
        {
            // Arrange
            string accountId = "Account001";
            string paymentSessionId = "ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6";
            string response = "{\"Id\":\"" + paymentSessionId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"Account001\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + paymentSessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"Account001-Pi001-Visa\\\",\\\"partner\\\":\\\"" + partner + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":false,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(paymentSessionId, response);

            PIDLData sampleData = new PIDLData();
            sampleData.Add("cvvToken", "123");

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/browserAuthenticateThreeDSOne?partner={2}", accountId, paymentSessionId, partner)));
            request.Headers.Add("x-ms-msaprofile", "PUID=samplePUID,emailAddress=someone@example.com");
            request.Content = new StringContent(JsonConvert.SerializeObject(sampleData), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType);

            PXSettings.PayerAuthService.PreProcess = async (payerAuthRequest) =>
            {
                string requestContent = await payerAuthRequest.Content.ReadAsStringAsync();
                JObject jsonObj = JObject.Parse(requestContent);
                string puid = JObject.Parse(jsonObj.SelectToken("payment_session").ToString()).SelectToken("user_id").ToString();
                Assert.AreEqual(puid, "samplePUID");
            };

            var pxResponse = await PXClient.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);
            PXSettings.PayerAuthService.ResetToDefaults();
        }

        [TestMethod]
        [DataRow("webblends", true)]
        [DataRow("webblends", false)]
        [DataRow("officeOobe", true)]
        [DataRow("oxooobe", true)]
        [DataRow("payin", true)]
        [DataRow("webPay", true)]
        [DataRow("consumerSupport", true)]
        [DataRow("xboxweb", true)]
        [DataRow("setupoffice", true)]
        [DataRow("setupofficesdx", true)]
        [DataRow("northstarweb", false)]
        public async Task PaymentSessions_BrowserAuthenticateIndiaThreeDS_Iframe_multi(string partner, bool shouldEnableIframeExperience)
        {
            // Arrange
            string paymentSessionId = "ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6";
            string payAuthResp = "{\"enrollment_status\":\"bypassed\",\"enrollment_type\":\"three_ds\",\"three_ds_server_transaction_id\":\"f35eadb1-6517-433d-af58-a28a0150a19d\",\"acs_url\":\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\",\"acs_signed_content\":\"{\\\"MD\\\":\\\"A4560000307199\\\",\\\"PaReq\\\":\\\"eyJhdXRoZW50aWNhdGlvbmlkIjoiZjM1ZWFkYjEtNjUxNy00MzNkLWFmNTgtYTI4YTAxNTBhMTlkIn0=\\\",\\\"TermUrl\\\":\\\"https://www.merchanturl.com/Response.jsp\\\"}\",\"authenticate_update_url\":\"/bd888d21-f2a9-4f8b-92c2-129b8d4748b6/payments/Z10009BHVVL2e8a44f83-dc56-4689-8c5d-4a7fc003c8f1/authenticate\",\"transaction_challenge_status\":\"C\",\"is_form_post_acs_url\":true,\"is_full_page_redirect\":true}";
            string sessionResp = "{\"id\":\"" + paymentSessionId + "\",\"session_type\":\"any\",\"data\":\"{\\\"ExposedFlightFeatures\\\":[\\\"PXPSD2Auth-_-_-Succeeded\\\",\\\"PXPSD2Auth-C-_-Unknown\\\",\\\"PXPSD2Auth-N-TSR10-Failed\\\",\\\"PXPSD2Auth-R-_-Failed\\\",\\\"PXPSD2Comp-_-_-_-Succeeded\\\",\\\"PXPSD2Comp-_-_-01-Cancelled\\\",\\\"PXPSD2Comp-N-_-04-TimedOut\\\",\\\"PXPSD2Comp-N-TSR10-_-Failed\\\",\\\"PXPSD2Comp-R-_-_-Failed\\\",\\\"PXPSD2SettingVersionV19\\\"],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"bd888d21-f2a9-4f8b-92c2-129b8d4748b6\\\",\\\"BrowserInfo\\\":{\\\"browser_accept_header\\\":\\\"*/*\\\",\\\"browser_ip\\\":\\\"::1\\\",\\\"browser_java_enabled\\\":true,\\\"browser_language\\\":\\\"en-US\\\",\\\"browser_color_depth\\\":\\\"8\\\",\\\"browser_screen_height\\\":\\\"480\\\",\\\"browser_screen_width\\\":\\\"640\\\",\\\"browser_tz\\\":\\\"480\\\",\\\"browser_user_agent\\\":\\\"Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36\\\",\\\"challenge_window_size\\\":\\\"05\\\"},\\\"MethodData\\\":{\\\"three_ds_server_trans_id\\\":\\\"123\\\",\\\"three_ds_method_url\\\":\\\"https://testurl.com\\\"},\\\"AuthenticationResponse\\\":{\\\"enrollment_status\\\":\\\"bypassed\\\",\\\"enrollment_type\\\":\\\"three_ds\\\",\\\"three_ds_server_transaction_id\\\":\\\"73f512ef-48fb-4322-b87d-ce6a3622ef1b\\\",\\\"acs_url\\\":\\\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\\\",\\\"acs_transaction_id\\\":null,\\\"authenticate_update_url\\\":\\\"/bd888d21-f2a9-4f8b-92c2-129b8d4748b6/payments/Z10007BHY0DT9b144cbe-f57f-4af2-85e3-dfd2c023d4ec/authenticate\\\",\\\"acs_signed_content\\\":\\\"{\\\\\\\"MD\\\\\\\":\\\\\\\"A4560000307199\\\\\\\",\\\\\\\"PaReq\\\\\\\":\\\\\\\"eyJhdXRoZW50aWNhdGlvbmlkIjoiNzNmNTEyZWYtNDhmYi00MzIyLWI4N2QtY2U2YTM2MjJlZjFiIn0=\\\\\\\",\\\\\\\"TermUrl\\\\\\\":\\\\\\\"https://www.merchanturl.com/Response.jsp\\\\\\\"}\\\",\\\"transaction_challenge_status\\\":\\\"C\\\",\\\"transaction_challenge_status_reason\\\":\\\"01\\\",\\\"card_holder_info\\\":null,\\\"acs_rendering_type\\\":null,\\\"acs_challenge_mandated\\\":null,\\\"acs_operator_id\\\":null,\\\"acs_reference_number\\\":null,\\\"authentication_type\\\":null,\\\"ds_reference_number\\\":null,\\\"is_form_post_acs_url\\\":true,\\\"is_full_page_redirect\\\":true},\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-US\\\",\\\"SuccessUrl\\\":null,\\\"FailureUrl\\\":null,\\\"ChallengeStatus\\\":\\\"Unknown\\\",\\\"payment_session_id\\\":\\\"Z10007BHY0DT9b144cbe-f57f-4af2-85e3-dfd2c023d4ec\\\",\\\"account_id\\\":\\\"bd888d21-f2a9-4f8b-92c2-129b8d4748b6\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"9MEvEAAAAAAEAACA\\\",\\\"partner\\\":\\\"" + partner + "\\\",\\\"amount\\\":100.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"IN\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"visa\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null,\\\"cvv_token \\\":null}\",\"encrypt_data\":false,\"state\":\"INCOMPLETE\",\"test_context\":{\"scenarios\":\"px-service-billdesk-provider,px-service-psd2-e2e-emulator,px-service-3ds1-show-iframe\",\"contact\":\"kowshikp\",\"context_props\":{}}}";
            PXSettings.SessionService.ResponseProvider.SessionStore.Add(paymentSessionId, sessionResp);
            PXSettings.PayerAuthService.ArrangeResponse(
                method: HttpMethod.Post,
                urlPattern: ".*/Authenticate.*",
                statusCode: HttpStatusCode.OK,
                content: payAuthResp);

            PIDLData sampleData = new PIDLData();
            sampleData.Add("cvvToken", "123");

            string threeDSMethodData = "eyJ0aHJlZURTU2VydmVyVHJhbnNJRCI6IjNlZWQwNGI2LTRjNzAtNGRiZS05ZDg4LWZiYjZhZWNlOTUzMCIsInRocmVlRFNNZXRob2ROb3RpZmljYXRpb25VUkwiOiJodHRwczovL3BheW1lbnRpbnN0cnVtZW50cy1pbnQubXAubWljcm9zb2Z0LmNvbS9WNi4wL3BheW1lbnRTZXNzaW9ucy9aMTAwNzdDOTNJVVY4ZWY0NGZmZS1kOGEyLTRhZDAtOWUwYi1mYTdkNjAzMjQzZDkvYXV0aGVudGljYXRlIn0";
            string cspStep = shouldEnableIframeExperience ? "cspStepFingerprint" : null;
            string flight = "eyJzY2VuYXJpb3MiOiJweC1zZXJ2aWNlLXBzZDItZTJlLWVtdWxhdG9yLG1kb2xsYXJwdXJjaGFzZSIsImNvbnRhY3QiOiJ0ZXN0LG1kb2xsYXJwdXJjaGFzZSJ9";

            var formData = new Dictionary<string, string>
            {
                { "threeDSMethodData", threeDSMethodData },
                { "cspStep", cspStep },
                { "x-ms-test", flight }
            };

            var content = new FormUrlEncodedContent(formData);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, GetPXServiceUrl(string.Format("/v7.0/paymentSessions/{0}/authenticate", paymentSessionId)));

            request.Content = content;

            var pxResponse = await PXClient.SendAsync(request);
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);
            var responseBody = await pxResponse.Content.ReadAsStringAsync();
            if (shouldEnableIframeExperience)
            {
                Assert.IsTrue(responseBody.Contains("srcdoc="));
            }
            else
            {
                Assert.IsTrue(!responseBody.Contains("srcdoc="));
            }
        }

        [TestMethod]
        [DataRow("webblends", true)]
        [DataRow("webblends", false)]
        [DataRow("officeOobe", true)]
        [DataRow("oxooobe", true)]
        [DataRow("payin", true)]
        [DataRow("webPay", true)]
        [DataRow("consumerSupport", true)]
        [DataRow("xboxweb", true)]
        [DataRow("setupoffice", true)]
        [DataRow("setupofficesdx", true)]
        [DataRow("northstarweb", false)]
        public async Task PaymentSessions_BrowserAuthenticateIndiaThreeDS_challenge_Iframe_multi(string partner, bool shouldEnableIframeExperience)
        {
            // Arrange
            string paymentSessionId = "ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6";
            string payAuthResp = "{\"enrollment_status\":\"bypassed\",\"enrollment_type\":\"three_ds\",\"three_ds_server_transaction_id\":\"f35eadb1-6517-433d-af58-a28a0150a19d\",\"acs_url\":\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\",\"acs_signed_content\":\"{\\\"MD\\\":\\\"A4560000307199\\\",\\\"PaReq\\\":\\\"eyJhdXRoZW50aWNhdGlvbmlkIjoiZjM1ZWFkYjEtNjUxNy00MzNkLWFmNTgtYTI4YTAxNTBhMTlkIn0=\\\",\\\"TermUrl\\\":\\\"https://www.merchanturl.com/Response.jsp\\\"}\",\"authenticate_update_url\":\"/bd888d21-f2a9-4f8b-92c2-129b8d4748b6/payments/Z10009BHVVL2e8a44f83-dc56-4689-8c5d-4a7fc003c8f1/authenticate\",\"transaction_challenge_status\":\"C\",\"is_form_post_acs_url\":true,\"is_full_page_redirect\":true}";
            string sessionResp = "{\"id\":\"" + paymentSessionId + "\",\"session_type\":\"any\",\"data\":\"{\\\"ExposedFlightFeatures\\\":[\\\"PXPSD2Auth-_-_-Succeeded\\\",\\\"PXPSD2Auth-C-_-Unknown\\\",\\\"PXPSD2Auth-N-TSR10-Failed\\\",\\\"PXPSD2Auth-R-_-Failed\\\",\\\"PXPSD2Comp-_-_-_-Succeeded\\\",\\\"PXPSD2Comp-_-_-01-Cancelled\\\",\\\"PXPSD2Comp-N-_-04-TimedOut\\\",\\\"PXPSD2Comp-N-TSR10-_-Failed\\\",\\\"PXPSD2Comp-R-_-_-Failed\\\",\\\"PXPSD2SettingVersionV19\\\"],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"bd888d21-f2a9-4f8b-92c2-129b8d4748b6\\\",\\\"BrowserInfo\\\":{\\\"browser_accept_header\\\":\\\"*/*\\\",\\\"browser_ip\\\":\\\"::1\\\",\\\"browser_java_enabled\\\":true,\\\"browser_language\\\":\\\"en-US\\\",\\\"browser_color_depth\\\":\\\"8\\\",\\\"browser_screen_height\\\":\\\"480\\\",\\\"browser_screen_width\\\":\\\"640\\\",\\\"browser_tz\\\":\\\"480\\\",\\\"browser_user_agent\\\":\\\"Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36\\\",\\\"challenge_window_size\\\":\\\"05\\\"},\\\"MethodData\\\":{\\\"three_ds_server_trans_id\\\":\\\"123\\\",\\\"three_ds_method_url\\\":\\\"https://testurl.com\\\"},\\\"AuthenticationResponse\\\":{\\\"enrollment_status\\\":\\\"bypassed\\\",\\\"enrollment_type\\\":\\\"three_ds\\\",\\\"three_ds_server_transaction_id\\\":\\\"73f512ef-48fb-4322-b87d-ce6a3622ef1b\\\",\\\"acs_url\\\":\\\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\\\",\\\"acs_transaction_id\\\":null,\\\"authenticate_update_url\\\":\\\"/bd888d21-f2a9-4f8b-92c2-129b8d4748b6/payments/Z10007BHY0DT9b144cbe-f57f-4af2-85e3-dfd2c023d4ec/authenticate\\\",\\\"acs_signed_content\\\":\\\"{\\\\\\\"MD\\\\\\\":\\\\\\\"A4560000307199\\\\\\\",\\\\\\\"PaReq\\\\\\\":\\\\\\\"eyJhdXRoZW50aWNhdGlvbmlkIjoiNzNmNTEyZWYtNDhmYi00MzIyLWI4N2QtY2U2YTM2MjJlZjFiIn0=\\\\\\\",\\\\\\\"TermUrl\\\\\\\":\\\\\\\"https://www.merchanturl.com/Response.jsp\\\\\\\"}\\\",\\\"transaction_challenge_status\\\":\\\"C\\\",\\\"transaction_challenge_status_reason\\\":\\\"01\\\",\\\"card_holder_info\\\":null,\\\"acs_rendering_type\\\":null,\\\"acs_challenge_mandated\\\":null,\\\"acs_operator_id\\\":null,\\\"acs_reference_number\\\":null,\\\"authentication_type\\\":null,\\\"ds_reference_number\\\":null,\\\"is_form_post_acs_url\\\":true,\\\"is_full_page_redirect\\\":true},\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-US\\\",\\\"SuccessUrl\\\":null,\\\"FailureUrl\\\":null,\\\"ChallengeStatus\\\":\\\"Unknown\\\",\\\"payment_session_id\\\":\\\"Z10007BHY0DT9b144cbe-f57f-4af2-85e3-dfd2c023d4ec\\\",\\\"account_id\\\":\\\"bd888d21-f2a9-4f8b-92c2-129b8d4748b6\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"9MEvEAAAAAAEAACA\\\",\\\"partner\\\":\\\"" + partner + "\\\",\\\"amount\\\":100.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"IN\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"visa\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null,\\\"cvv_token \\\":null}\",\"encrypt_data\":false,\"state\":\"INCOMPLETE\",\"test_context\":{\"scenarios\":\"px-service-billdesk-provider,px-service-psd2-e2e-emulator,px-service-3ds1-show-iframe\",\"contact\":\"kowshikp\",\"context_props\":{}}}";
            PXSettings.SessionService.ResponseProvider.SessionStore.Add(paymentSessionId, sessionResp);
            PXSettings.PayerAuthService.ArrangeResponse(
                method: HttpMethod.Post,
                urlPattern: ".*/Authenticate.*",
                statusCode: HttpStatusCode.OK,
                content: payAuthResp);

            PIDLData sampleData = new PIDLData();
            sampleData.Add("cvvToken", "123");

            string threeDSMethodData = "eyJ0aHJlZURTU2VydmVyVHJhbnNJRCI6IjNlZWQwNGI2LTRjNzAtNGRiZS05ZDg4LWZiYjZhZWNlOTUzMCIsInRocmVlRFNNZXRob2ROb3RpZmljYXRpb25VUkwiOiJodHRwczovL3BheW1lbnRpbnN0cnVtZW50cy1pbnQubXAubWljcm9zb2Z0LmNvbS9WNi4wL3BheW1lbnRTZXNzaW9ucy9aMTAwNzdDOTNJVVY4ZWY0NGZmZS1kOGEyLTRhZDAtOWUwYi1mYTdkNjAzMjQzZDkvYXV0aGVudGljYXRlIn0";
            string cspStep = shouldEnableIframeExperience ? "cspStepChallenge" : null;
            string flight = "eyJzY2VuYXJpb3MiOiJweC1zZXJ2aWNlLXBzZDItZTJlLWVtdWxhdG9yLG1kb2xsYXJwdXJjaGFzZSIsImNvbnRhY3QiOiJ0ZXN0LG1kb2xsYXJwdXJjaGFzZSJ9";

            var formData = new Dictionary<string, string>
            {
                { "threeDSMethodData", threeDSMethodData },
                { "cspStep", cspStep },
                { "x-ms-test", flight }
            };

            var content = new FormUrlEncodedContent(formData);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, GetPXServiceUrl(string.Format("/v7.0/paymentSessions/{0}/authenticate", paymentSessionId)));

            request.Content = content;

            var pxResponse = await PXClient.SendAsync(request);
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);
            var responseBody = await pxResponse.Content.ReadAsStringAsync();
            if (shouldEnableIframeExperience)
            {
                Assert.IsTrue(responseBody.Contains("srcdoc="));
            }
            else
            {
                Assert.IsTrue(!responseBody.Contains("srcdoc="));
            }
        }

        [TestMethod]
        [DataRow("webblends")]
        [DataRow("webblends")]
        [DataRow("officeOobe")]
        [DataRow("oxooobe")]
        [DataRow("payin")]
        [DataRow("webPay")]
        [DataRow("consumerSupport")]
        [DataRow("xboxweb")]
        [DataRow("setupoffice")]
        [DataRow("setupofficesdx")]
        [DataRow("northstarweb")]
        public async Task PaymentSessions_BrowserAuthenticate_challenge_Iframe_multi_CSPProxyFrameFlightEnabled(string partner)
        {
            // Arrange
            string paymentSessionId = "ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6";
            string payAuthResp = "{\"enrollment_status\":\"bypassed\",\"enrollment_type\":\"three_ds\",\"three_ds_server_transaction_id\":\"f35eadb1-6517-433d-af58-a28a0150a19d\",\"acs_url\":\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\",\"acs_signed_content\":\"{\\\"MD\\\":\\\"A4560000307199\\\",\\\"PaReq\\\":\\\"eyJhdXRoZW50aWNhdGlvbmlkIjoiZjM1ZWFkYjEtNjUxNy00MzNkLWFmNTgtYTI4YTAxNTBhMTlkIn0=\\\",\\\"TermUrl\\\":\\\"https://www.merchanturl.com/Response.jsp\\\"}\",\"authenticate_update_url\":\"/bd888d21-f2a9-4f8b-92c2-129b8d4748b6/payments/Z10009BHVVL2e8a44f83-dc56-4689-8c5d-4a7fc003c8f1/authenticate\",\"transaction_challenge_status\":\"C\",\"is_form_post_acs_url\":true,\"is_full_page_redirect\":true}";
            string sessionResp = "{\"id\":\"" + paymentSessionId + "\",\"session_type\":\"any\",\"data\":\"{\\\"ExposedFlightFeatures\\\":[\\\"PXPSD2Auth-_-_-Succeeded\\\",\\\"PXPSD2EnableCSPProxyFrame\\\",\\\"PXPSD2Auth-C-_-Unknown\\\",\\\"PXPSD2Auth-N-TSR10-Failed\\\",\\\"PXPSD2Auth-R-_-Failed\\\",\\\"PXPSD2Comp-_-_-_-Succeeded\\\",\\\"PXPSD2Comp-_-_-01-Cancelled\\\",\\\"PXPSD2Comp-N-_-04-TimedOut\\\",\\\"PXPSD2Comp-N-TSR10-_-Failed\\\",\\\"PXPSD2Comp-R-_-_-Failed\\\",\\\"PXPSD2SettingVersionV19\\\"],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"bd888d21-f2a9-4f8b-92c2-129b8d4748b6\\\",\\\"BrowserInfo\\\":{\\\"browser_accept_header\\\":\\\"*/*\\\",\\\"browser_ip\\\":\\\"::1\\\",\\\"browser_java_enabled\\\":true,\\\"browser_language\\\":\\\"en-US\\\",\\\"browser_color_depth\\\":\\\"8\\\",\\\"browser_screen_height\\\":\\\"480\\\",\\\"browser_screen_width\\\":\\\"640\\\",\\\"browser_tz\\\":\\\"480\\\",\\\"browser_user_agent\\\":\\\"Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36\\\",\\\"challenge_window_size\\\":\\\"05\\\"},\\\"MethodData\\\":{\\\"three_ds_server_trans_id\\\":\\\"123\\\",\\\"three_ds_method_url\\\":\\\"https://testurl.com\\\"},\\\"AuthenticationResponse\\\":{\\\"enrollment_status\\\":\\\"bypassed\\\",\\\"enrollment_type\\\":\\\"three_ds\\\",\\\"three_ds_server_transaction_id\\\":\\\"73f512ef-48fb-4322-b87d-ce6a3622ef1b\\\",\\\"acs_url\\\":\\\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\\\",\\\"acs_transaction_id\\\":null,\\\"authenticate_update_url\\\":\\\"/bd888d21-f2a9-4f8b-92c2-129b8d4748b6/payments/Z10007BHY0DT9b144cbe-f57f-4af2-85e3-dfd2c023d4ec/authenticate\\\",\\\"acs_signed_content\\\":\\\"{\\\\\\\"MD\\\\\\\":\\\\\\\"A4560000307199\\\\\\\",\\\\\\\"PaReq\\\\\\\":\\\\\\\"eyJhdXRoZW50aWNhdGlvbmlkIjoiNzNmNTEyZWYtNDhmYi00MzIyLWI4N2QtY2U2YTM2MjJlZjFiIn0=\\\\\\\",\\\\\\\"TermUrl\\\\\\\":\\\\\\\"https://www.merchanturl.com/Response.jsp\\\\\\\"}\\\",\\\"transaction_challenge_status\\\":\\\"C\\\",\\\"transaction_challenge_status_reason\\\":\\\"01\\\",\\\"card_holder_info\\\":null,\\\"acs_rendering_type\\\":null,\\\"acs_challenge_mandated\\\":null,\\\"acs_operator_id\\\":null,\\\"acs_reference_number\\\":null,\\\"authentication_type\\\":null,\\\"ds_reference_number\\\":null,\\\"is_form_post_acs_url\\\":true,\\\"is_full_page_redirect\\\":true},\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-US\\\",\\\"SuccessUrl\\\":null,\\\"FailureUrl\\\":null,\\\"ChallengeStatus\\\":\\\"Unknown\\\",\\\"payment_session_id\\\":\\\"Z10007BHY0DT9b144cbe-f57f-4af2-85e3-dfd2c023d4ec\\\",\\\"account_id\\\":\\\"bd888d21-f2a9-4f8b-92c2-129b8d4748b6\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"9MEvEAAAAAAEAACA\\\",\\\"partner\\\":\\\"" + partner + "\\\",\\\"amount\\\":100.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"IN\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"visa\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null,\\\"cvv_token \\\":null}\",\"encrypt_data\":false,\"state\":\"INCOMPLETE\",\"test_context\":{\"scenarios\":\"px-service-billdesk-provider,px-service-psd2-e2e-emulator,px-service-3ds1-show-iframe\",\"contact\":\"kowshikp\",\"context_props\":{}}}";
            PXSettings.SessionService.ResponseProvider.SessionStore.Add(paymentSessionId, sessionResp);
            PXSettings.PayerAuthService.ArrangeResponse(
                method: HttpMethod.Post,
                urlPattern: ".*/Authenticate.*",
                statusCode: HttpStatusCode.OK,
                content: payAuthResp);

            PIDLData sampleData = new PIDLData();
            sampleData.Add("cvvToken", "123");

            string threeDSMethodData = "eyJ0aHJlZURTU2VydmVyVHJhbnNJRCI6IjNlZWQwNGI2LTRjNzAtNGRiZS05ZDg4LWZiYjZhZWNlOTUzMCIsInRocmVlRFNNZXRob2ROb3RpZmljYXRpb25VUkwiOiJodHRwczovL3BheW1lbnRpbnN0cnVtZW50cy1pbnQubXAubWljcm9zb2Z0LmNvbS9WNi4wL3BheW1lbnRTZXNzaW9ucy9aMTAwNzdDOTNJVVY4ZWY0NGZmZS1kOGEyLTRhZDAtOWUwYi1mYTdkNjAzMjQzZDkvYXV0aGVudGljYXRlIn0";
            string cspStep = null;
            string flight = "eyJzY2VuYXJpb3MiOiJweC1zZXJ2aWNlLXBzZDItZTJlLWVtdWxhdG9yLG1kb2xsYXJwdXJjaGFzZSIsImNvbnRhY3QiOiJ0ZXN0LG1kb2xsYXJwdXJjaGFzZSJ9";

            var formData = new Dictionary<string, string>
            {
                { "threeDSMethodData", threeDSMethodData },
                { "cspStep", cspStep },
                { "x-ms-test", flight }
            };

            var content = new FormUrlEncodedContent(formData);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, GetPXServiceUrl(string.Format("/v7.0/paymentSessions/{0}/authenticate", paymentSessionId)));

            request.Content = content;

            var pxResponse = await PXClient.SendAsync(request);
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);
            var responseBody = await pxResponse.Content.ReadAsStringAsync();
            Assert.IsTrue(responseBody.Contains("authenticate"));
        }

        [TestMethod]
        [DataRow("webblends", true, true)]
        [DataRow("webblends", false, false)]
        [DataRow("officeOobe", false, true)]
        [DataRow("oxooobe", false, true)]
        [DataRow("payin", false, true)]
        [DataRow("webPay", false, true)]
        [DataRow("consumerSupport", false, true)]
        [DataRow("xboxweb", false, true)]
        [DataRow("setupoffice", false, true)]
        [DataRow("setupofficesdx", false, true)]
        public async Task PaymentSessions_BrowserAuthenticateIndiaThreeDS_Iframe(string partner, bool sendTestHeader, bool shouldEnableIframeExperience)
        {
            // Arrange
            string accountId = "Account001";
            string paymentSessionId = "ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6";
            string sessionResp = "{\"id\":\"" + paymentSessionId + "\",\"session_type\":\"any\",\"data\":\"{\\\"ExposedFlightFeatures\\\":[\\\"PXPSD2Auth-_-_-Succeeded\\\",\\\"PXPSD2Auth-C-_-Unknown\\\",\\\"PXPSD2Auth-N-TSR10-Failed\\\",\\\"PXPSD2Auth-R-_-Failed\\\",\\\"PXPSD2Comp-_-_-_-Succeeded\\\",\\\"PXPSD2Comp-_-_-01-Cancelled\\\",\\\"PXPSD2Comp-N-_-04-TimedOut\\\",\\\"PXPSD2Comp-N-TSR10-_-Failed\\\",\\\"PXPSD2Comp-R-_-_-Failed\\\",\\\"PXPSD2SettingVersionV19\\\"],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"bd888d21-f2a9-4f8b-92c2-129b8d4748b6\\\",\\\"BrowserInfo\\\":{\\\"browser_accept_header\\\":\\\"*/*\\\",\\\"browser_ip\\\":\\\"::1\\\",\\\"browser_java_enabled\\\":true,\\\"browser_language\\\":\\\"en-US\\\",\\\"browser_color_depth\\\":\\\"8\\\",\\\"browser_screen_height\\\":\\\"480\\\",\\\"browser_screen_width\\\":\\\"640\\\",\\\"browser_tz\\\":\\\"480\\\",\\\"browser_user_agent\\\":\\\"Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36\\\",\\\"challenge_window_size\\\":\\\"05\\\"},\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":{\\\"enrollment_status\\\":\\\"bypassed\\\",\\\"enrollment_type\\\":\\\"three_ds\\\",\\\"three_ds_server_transaction_id\\\":\\\"73f512ef-48fb-4322-b87d-ce6a3622ef1b\\\",\\\"acs_url\\\":\\\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\\\",\\\"acs_transaction_id\\\":null,\\\"authenticate_update_url\\\":\\\"/bd888d21-f2a9-4f8b-92c2-129b8d4748b6/payments/Z10007BHY0DT9b144cbe-f57f-4af2-85e3-dfd2c023d4ec/authenticate\\\",\\\"acs_signed_content\\\":\\\"{\\\\\\\"MD\\\\\\\":\\\\\\\"A4560000307199\\\\\\\",\\\\\\\"PaReq\\\\\\\":\\\\\\\"eyJhdXRoZW50aWNhdGlvbmlkIjoiNzNmNTEyZWYtNDhmYi00MzIyLWI4N2QtY2U2YTM2MjJlZjFiIn0=\\\\\\\",\\\\\\\"TermUrl\\\\\\\":\\\\\\\"https://www.merchanturl.com/Response.jsp\\\\\\\"}\\\",\\\"transaction_challenge_status\\\":\\\"C\\\",\\\"transaction_challenge_status_reason\\\":\\\"01\\\",\\\"card_holder_info\\\":null,\\\"acs_rendering_type\\\":null,\\\"acs_challenge_mandated\\\":null,\\\"acs_operator_id\\\":null,\\\"acs_reference_number\\\":null,\\\"authentication_type\\\":null,\\\"ds_reference_number\\\":null,\\\"is_form_post_acs_url\\\":true,\\\"is_full_page_redirect\\\":true},\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-US\\\",\\\"SuccessUrl\\\":null,\\\"FailureUrl\\\":null,\\\"ChallengeStatus\\\":\\\"Unknown\\\",\\\"payment_session_id\\\":\\\"Z10007BHY0DT9b144cbe-f57f-4af2-85e3-dfd2c023d4ec\\\",\\\"account_id\\\":\\\"bd888d21-f2a9-4f8b-92c2-129b8d4748b6\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"9MEvEAAAAAAEAACA\\\",\\\"partner\\\":\\\"" + partner + "\\\",\\\"amount\\\":100.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"IN\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"visa\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null,\\\"cvv_token \\\":null}\",\"encrypt_data\":false,\"state\":\"INCOMPLETE\",\"test_context\":{\"scenarios\":\"px-service-billdesk-provider,px-service-psd2-e2e-emulator,px-service-3ds1-show-iframe\",\"contact\":\"kowshikp\",\"context_props\":{}}}";
            string payAuthResp = "{\"enrollment_status\":\"bypassed\",\"enrollment_type\":\"three_ds\",\"three_ds_server_transaction_id\":\"f35eadb1-6517-433d-af58-a28a0150a19d\",\"acs_url\":\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\",\"acs_signed_content\":\"{\\\"MD\\\":\\\"A4560000307199\\\",\\\"PaReq\\\":\\\"eyJhdXRoZW50aWNhdGlvbmlkIjoiZjM1ZWFkYjEtNjUxNy00MzNkLWFmNTgtYTI4YTAxNTBhMTlkIn0=\\\",\\\"TermUrl\\\":\\\"https://www.merchanturl.com/Response.jsp\\\"}\",\"authenticate_update_url\":\"/bd888d21-f2a9-4f8b-92c2-129b8d4748b6/payments/Z10009BHVVL2e8a44f83-dc56-4689-8c5d-4a7fc003c8f1/authenticate\",\"transaction_challenge_status\":\"C\",\"is_form_post_acs_url\":true,\"is_full_page_redirect\":true}";
            PXSettings.SessionService.ResponseProvider.SessionStore.Add(paymentSessionId, sessionResp);
            PXSettings.PayerAuthService.ArrangeResponse(
                method: HttpMethod.Post,
                urlPattern: ".*/Authenticate.*",
                statusCode: HttpStatusCode.OK,
                content: payAuthResp);

            PIDLData sampleData = new PIDLData();
            sampleData.Add("cvvToken", "123");

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/browserAuthenticateThreeDSOne?partner={2}", accountId, paymentSessionId, partner)));
            if (sendTestHeader)
            {
                request.Headers.Add("x-ms-test", "{\"scenarios\":\"px-service-3ds1-show-iframe\"}");
            }

            request.Content = new StringContent(JsonConvert.SerializeObject(sampleData), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType);

            var pxResponse = await PXClient.SendAsync(request);
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);
            var responseBody = JsonConvert.DeserializeObject<PIDLResource>(await pxResponse.Content.ReadAsStringAsync());
            var pidls = ReadPidlResourceFromJson(JsonConvert.SerializeObject(responseBody.ClientAction.Context));

            //// check iframe is returned.
            if (shouldEnableIframeExperience)
            {
                Assert.AreEqual("poll", pidls[0].DisplayPages[0].Action.ActionType);
                Assert.AreEqual("iframe", pidls[0].DisplayPages[0].Members[0].DisplayHintType);
                var iFrameDisplayHint = pidls[0].DisplayPages[0].Members[0] as IFrameDisplayHint;
                Assert.IsNotNull(iFrameDisplayHint.DisplayTags, "Accessibility display tags are missing");
                Assert.AreEqual(iFrameDisplayHint.DisplayTags["accessibilityName"], "The bank purchase authentication dialog");
            }
            else
            {
                Assert.AreEqual("poll", pidls[0].DisplayPages[1].Action.ActionType);
            }
        }

        [TestMethod]
        public async Task PaymentSessions_BrowserAuthenticateIndiaThreeDS_Failed()
        {
            // Arrange
            string accountId = "Account001";
            string partner = "webblends";
            string paymentSessionId = "ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6";
            string sessionResp = "{\"Id\":\"" + paymentSessionId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":[\\\"PXReturnFailedSessionState\\\"],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"Account001\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + paymentSessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"Account001-Pi001-Visa\\\",\\\"partner\\\":\\\"" + partner + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":false,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";
            string payAuthResp = "{\"enrollment_status\":\"enrolled\",\"enrollment_type\":\"three_ds\",\"authenticate_update_url\":\"/fdf3c867-e925-40b9-8650-3e72248a5fd5/payments/Z10096CJKKC13760301b-be4f-4f11-9013-73132fd8c21d/authenticate\",\"transaction_challenge_status\":\"N\",\"card_holder_info\":\"Card is not enabled for tokenization\",\"is_form_post_acs_url\":true,\"is_full_page_redirect\":true}";
            PXSettings.SessionService.ResponseProvider.SessionStore.Add(paymentSessionId, sessionResp);
            PXSettings.PayerAuthService.ArrangeResponse(
                method: HttpMethod.Post,
                urlPattern: ".*/Authenticate.*",
                statusCode: HttpStatusCode.OK,
                content: payAuthResp);

            PIDLData sampleData = new PIDLData();
            sampleData.Add("cvvToken", "123");

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/browserAuthenticateThreeDSOne?partner={2}", accountId, paymentSessionId, partner)));

            request.Content = new StringContent(JsonConvert.SerializeObject(sampleData), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType);

            var pxResponse = await PXClient.SendAsync(request);
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);
            var responseBody = JsonConvert.DeserializeObject<PIDLResource>(await pxResponse.Content.ReadAsStringAsync());
            Assert.AreEqual(responseBody.ClientAction.ActionType.ToString(), Microsoft.Commerce.Payments.PXCommon.ClientActionType.Failure.ToString());
            ServiceErrorResponse serviceError = JsonConvert.DeserializeObject<ServiceErrorResponse>(responseBody?.ClientAction?.Context.ToString());
            Assert.AreEqual(serviceError.InnerError.ErrorCode, "RejectedByProvider");
        }

        [TestMethod]
        public async Task PaymentSessions_BrowserAuthenticateRedirectionThreeDSOne()
        {
            string paymentSessionId = "ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6";
            string response = "{\"id\":\"Z10005BGAPLA4192d3ac-1071-4db4-9e9d-612e08b62946\",\"session_type\":\"any\",\"data\":\"{\\\"ExposedFlightFeatures\\\":[\\\"PXPSD2ProdIntegration\\\",\\\"PXEnablePaypalRedirectUrlText\\\",\\\"PXProfileUpdateToHapi\\\",\\\"PXPSD2Auth-_-_-Succeeded\\\",\\\"PXPSD2Auth-C-_-Unknown\\\",\\\"PXPSD2Auth-N-TSR10-Failed\\\",\\\"PXPSD2Auth-R-_-Failed\\\",\\\"PXPSD2Comp-_-_-_-Succeeded\\\",\\\"PXPSD2Comp-_-_-01-Cancelled\\\",\\\"PXPSD2Comp-N-_-04-TimedOut\\\",\\\"PXPSD2Comp-N-TSR10-_-Failed\\\",\\\"PXPSD2Comp-R-_-_-Failed\\\",\\\"PXPSD2SettingVersionV19\\\"],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"bd888d21-f2a9-4f8b-92c2-129b8d4748b6\\\",\\\"BrowserInfo\\\":{\\\"browser_accept_header\\\":\\\"*/*\\\",\\\"browser_ip\\\":\\\"::1\\\",\\\"browser_java_enabled\\\":true,\\\"browser_language\\\":\\\"en-US\\\",\\\"browser_color_depth\\\":\\\"8\\\",\\\"browser_screen_height\\\":\\\"480\\\",\\\"browser_screen_width\\\":\\\"640\\\",\\\"browser_tz\\\":\\\"480\\\",\\\"browser_user_agent\\\":\\\"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.93 Safari/537.36\\\",\\\"challenge_window_size\\\":\\\"05\\\"},\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":{\\\"enrollment_status\\\":\\\"bypassed\\\",\\\"enrollment_type\\\":\\\"three_ds\\\",\\\"three_ds_server_transaction_id\\\":\\\"b643dce6-fe1e-4c04-a6d6-3b173c4c95d2\\\",\\\"acs_url\\\":\\\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\\\",\\\"acs_transaction_id\\\":null,\\\"authenticate_update_url\\\":\\\"/bd888d21-f2a9-4f8b-92c2-129b8d4748b6/payments/Z10005BGAPLA4192d3ac-1071-4db4-9e9d-612e08b62946/authenticate\\\",\\\"acs_signed_content\\\":\\\"{\\\\\\\"MD\\\\\\\":\\\\\\\"A4560000307199\\\\\\\",\\\\\\\"PaReq\\\\\\\":\\\\\\\"eyJhdXRoZW50aWNhdGlvbmlkIjoiYjY0M2RjZTYtZmUxZS00YzA0LWE2ZDYtM2IxNzNjNGM5NWQyIn0=\\\\\\\",\\\\\\\"TermUrl\\\\\\\":\\\\\\\"https://www.merchanturl.com/Response.jsp\\\\\\\"}\\\",\\\"transaction_challenge_status\\\":\\\"C\\\",\\\"transaction_challenge_status_reason\\\":\\\"01\\\",\\\"card_holder_info\\\":null,\\\"acs_rendering_type\\\":null,\\\"acs_challenge_mandated\\\":null,\\\"acs_operator_id\\\":null,\\\"acs_reference_number\\\":null,\\\"authentication_type\\\":null,\\\"ds_reference_number\\\":null,\\\"is_form_post_acs_url\\\":true,\\\"is_full_page_redirect\\\":true},\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-US\\\",\\\"SuccessUrl\\\":null,\\\"FailureUrl\\\":null,\\\"ChallengeStatus\\\":\\\"Unknown\\\",\\\"payment_session_id\\\":\\\"Z10005BGAPLA4192d3ac-1071-4db4-9e9d-612e08b62946\\\",\\\"account_id\\\":\\\"bd888d21-f2a9-4f8b-92c2-129b8d4748b6\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"mEQdBgAAAAASAACA\\\",\\\"partner\\\":\\\"webblends\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"IN\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"visa\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null,\\\"cvv_token \\\":null}\",\"encrypt_data\":false,\"state\":\"INCOMPLETE\",\"test_context\":{\"scenarios\":\"px-service-billdesk-provider,px-service-psd2-e2e-emulator\",\"contact\":\"test\",\"context_props\":{}}}";

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(paymentSessionId, response);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, GetPXServiceUrl(string.Format("/v7.0/paymentSessions/{0}/browserAuthenticateRedirectionThreeDSOne?ru=https%3A%2F%2Faccount.microsoft.com&rx=https%3A%2F%2Fwww.microsoft.com", paymentSessionId)));

            var pxResponse = await PXClient.SendAsync(request);

            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);

            var content = pxResponse.Content.ReadAsStringAsync();

            Assert.IsTrue(content.Result.Contains("onload=\"document.forms[0].submit();\""));
            Assert.IsTrue(content.Result.Contains("<form action=\""));
            Assert.IsTrue(content.Result.Contains("method=\"post\""));
            Assert.IsTrue(content.Result.Contains("<input type=\"hidden\" name=\"MD\" value=\""));
            Assert.IsTrue(content.Result.Contains("<input type=\"hidden\" name=\"PaReq\" value=\""));
            Assert.IsTrue(content.Result.Contains("<input type=\"hidden\" name=\"TermUrl\" value=\""));
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("a")]
        [DataRow("www.microsoft.com")]
        [DataRow("http://www.microsoft.com")]
        [DataRow("https://www.contoso.com")]
        [DataRow("https%3A%2F%2Fwww.contoso.com")]
        public async Task PaymentSessions_BrowserAuthenticateRedirectionThreeDSOne_InvalidRu(string invalidRu)
        {
            string paymentSessionId = "ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, GetPXServiceUrl(string.Format("/v7.0/paymentSessions/{0}/browserAuthenticateRedirectionThreeDSOne?ru={1}&rx=https%3A%2F%2Fwww.microsoft.com", paymentSessionId, invalidRu)));

            var pxResponse = await PXClient.SendAsync(request);

            Assert.AreEqual(HttpStatusCode.BadRequest, pxResponse.StatusCode);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("a")]
        [DataRow("www.microsoft.com")]
        [DataRow("http://www.microsoft.com")]
        [DataRow("https://www.contoso.com")]
        [DataRow("https%3A%2F%2Fwww.contoso.com")]
        public async Task PaymentSessions_BrowserAuthenticateRedirectionThreeDSOne_InvalidRx(string invalidRx)
        {
            string paymentSessionId = "ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, GetPXServiceUrl(string.Format("/v7.0/paymentSessions/{0}/browserAuthenticateRedirectionThreeDSOne?ru=https%3A%2F%2Fwww.microsoft.com&rx={1}", paymentSessionId, invalidRx)));

            var pxResponse = await PXClient.SendAsync(request);

            Assert.AreEqual(HttpStatusCode.BadRequest, pxResponse.StatusCode);
        }

        [TestMethod]
        [DataRow("https%3A%2F%2Fwww.microsoft.com")]
        [DataRow("https%3A%2F%2Faccount.microsoft.com")]
        [DataRow("https%3A%2F%2Fstores.office.com")]
        [DataRow("https%3A%2F%2Fwww.xbox.com")]
        [DataRow("https%3A%2F%2Forigin-int.xbox.com")]
        public async Task PaymentSessions_BrowserAuthenticateRedirectionThreeDSOne_ValidRuRx(string validRuRx)
        {
            string paymentSessionId = "ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, GetPXServiceUrl(string.Format("/v7.0/paymentSessions/{0}/browserAuthenticateRedirectionThreeDSOne?ru={1}&rx={1}", paymentSessionId, validRuRx)));

            string response = "{\"id\":\"Z10005BGAPLA4192d3ac-1071-4db4-9e9d-612e08b62946\",\"session_type\":\"any\",\"data\":\"{\\\"ExposedFlightFeatures\\\":[\\\"PXPSD2ProdIntegration\\\",\\\"PXEnablePaypalRedirectUrlText\\\",\\\"PXProfileUpdateToHapi\\\",\\\"PXPSD2Auth-_-_-Succeeded\\\",\\\"PXPSD2Auth-C-_-Unknown\\\",\\\"PXPSD2Auth-N-TSR10-Failed\\\",\\\"PXPSD2Auth-R-_-Failed\\\",\\\"PXPSD2Comp-_-_-_-Succeeded\\\",\\\"PXPSD2Comp-_-_-01-Cancelled\\\",\\\"PXPSD2Comp-N-_-04-TimedOut\\\",\\\"PXPSD2Comp-N-TSR10-_-Failed\\\",\\\"PXPSD2Comp-R-_-_-Failed\\\",\\\"PXPSD2SettingVersionV19\\\"],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"bd888d21-f2a9-4f8b-92c2-129b8d4748b6\\\",\\\"BrowserInfo\\\":{\\\"browser_accept_header\\\":\\\"*/*\\\",\\\"browser_ip\\\":\\\"::1\\\",\\\"browser_java_enabled\\\":true,\\\"browser_language\\\":\\\"en-US\\\",\\\"browser_color_depth\\\":\\\"8\\\",\\\"browser_screen_height\\\":\\\"480\\\",\\\"browser_screen_width\\\":\\\"640\\\",\\\"browser_tz\\\":\\\"480\\\",\\\"browser_user_agent\\\":\\\"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.93 Safari/537.36\\\",\\\"challenge_window_size\\\":\\\"05\\\"},\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":{\\\"enrollment_status\\\":\\\"bypassed\\\",\\\"enrollment_type\\\":\\\"three_ds\\\",\\\"three_ds_server_transaction_id\\\":\\\"b643dce6-fe1e-4c04-a6d6-3b173c4c95d2\\\",\\\"acs_url\\\":\\\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\\\",\\\"acs_transaction_id\\\":null,\\\"authenticate_update_url\\\":\\\"/bd888d21-f2a9-4f8b-92c2-129b8d4748b6/payments/Z10005BGAPLA4192d3ac-1071-4db4-9e9d-612e08b62946/authenticate\\\",\\\"acs_signed_content\\\":\\\"{\\\\\\\"MD\\\\\\\":\\\\\\\"A4560000307199\\\\\\\",\\\\\\\"PaReq\\\\\\\":\\\\\\\"eyJhdXRoZW50aWNhdGlvbmlkIjoiYjY0M2RjZTYtZmUxZS00YzA0LWE2ZDYtM2IxNzNjNGM5NWQyIn0=\\\\\\\",\\\\\\\"TermUrl\\\\\\\":\\\\\\\"https://www.merchanturl.com/Response.jsp\\\\\\\"}\\\",\\\"transaction_challenge_status\\\":\\\"C\\\",\\\"transaction_challenge_status_reason\\\":\\\"01\\\",\\\"card_holder_info\\\":null,\\\"acs_rendering_type\\\":null,\\\"acs_challenge_mandated\\\":null,\\\"acs_operator_id\\\":null,\\\"acs_reference_number\\\":null,\\\"authentication_type\\\":null,\\\"ds_reference_number\\\":null,\\\"is_form_post_acs_url\\\":true,\\\"is_full_page_redirect\\\":true},\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-US\\\",\\\"SuccessUrl\\\":null,\\\"FailureUrl\\\":null,\\\"ChallengeStatus\\\":\\\"Unknown\\\",\\\"payment_session_id\\\":\\\"Z10005BGAPLA4192d3ac-1071-4db4-9e9d-612e08b62946\\\",\\\"account_id\\\":\\\"bd888d21-f2a9-4f8b-92c2-129b8d4748b6\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"mEQdBgAAAAASAACA\\\",\\\"partner\\\":\\\"webblends\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"IN\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"visa\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null,\\\"cvv_token \\\":null}\",\"encrypt_data\":false,\"state\":\"INCOMPLETE\",\"test_context\":{\"scenarios\":\"px-service-billdesk-provider,px-service-psd2-e2e-emulator\",\"contact\":\"test\",\"context_props\":{}}}";

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(paymentSessionId, response);

            var pxResponse = await PXClient.SendAsync(request);

            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);
        }

        [TestMethod]
        public async Task PaymentSessions_BrowserNotifyThreeDSOneChallengeCompleted()
        {
            string paymentSessionId = "ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6";
            string response = "{\"id\":\"Z10005BGAPLA4192d3ac-1071-4db4-9e9d-612e08b62946\",\"session_type\":\"any\",\"data\":\"{\\\"ExposedFlightFeatures\\\":[\\\"PXPSD2ProdIntegration\\\",\\\"PXEnablePaypalRedirectUrlText\\\",\\\"PXProfileUpdateToHapi\\\",\\\"PXPSD2Auth-_-_-Succeeded\\\",\\\"PXPSD2Auth-C-_-Unknown\\\",\\\"PXPSD2Auth-N-TSR10-Failed\\\",\\\"PXPSD2Auth-R-_-Failed\\\",\\\"PXPSD2Comp-_-_-_-Succeeded\\\",\\\"PXPSD2Comp-_-_-01-Cancelled\\\",\\\"PXPSD2Comp-N-_-04-TimedOut\\\",\\\"PXPSD2Comp-N-TSR10-_-Failed\\\",\\\"PXPSD2Comp-R-_-_-Failed\\\",\\\"PXPSD2SettingVersionV19\\\"],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"bd888d21-f2a9-4f8b-92c2-129b8d4748b6\\\",\\\"BrowserInfo\\\":{\\\"browser_accept_header\\\":\\\"*/*\\\",\\\"browser_ip\\\":\\\"::1\\\",\\\"browser_java_enabled\\\":true,\\\"browser_language\\\":\\\"en-US\\\",\\\"browser_color_depth\\\":\\\"8\\\",\\\"browser_screen_height\\\":\\\"480\\\",\\\"browser_screen_width\\\":\\\"640\\\",\\\"browser_tz\\\":\\\"480\\\",\\\"browser_user_agent\\\":\\\"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.93 Safari/537.36\\\",\\\"challenge_window_size\\\":\\\"05\\\"},\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":{\\\"enrollment_status\\\":\\\"bypassed\\\",\\\"enrollment_type\\\":\\\"three_ds\\\",\\\"three_ds_server_transaction_id\\\":\\\"b643dce6-fe1e-4c04-a6d6-3b173c4c95d2\\\",\\\"acs_url\\\":\\\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\\\",\\\"acs_transaction_id\\\":null,\\\"authenticate_update_url\\\":\\\"/bd888d21-f2a9-4f8b-92c2-129b8d4748b6/payments/Z10005BGAPLA4192d3ac-1071-4db4-9e9d-612e08b62946/authenticate\\\",\\\"acs_signed_content\\\":\\\"{\\\\\\\"MD\\\\\\\":\\\\\\\"A4560000307199\\\\\\\",\\\\\\\"PaReq\\\\\\\":\\\\\\\"eyJhdXRoZW50aWNhdGlvbmlkIjoiYjY0M2RjZTYtZmUxZS00YzA0LWE2ZDYtM2IxNzNjNGM5NWQyIn0=\\\\\\\",\\\\\\\"TermUrl\\\\\\\":\\\\\\\"https://www.merchanturl.com/Response.jsp\\\\\\\"}\\\",\\\"transaction_challenge_status\\\":\\\"C\\\",\\\"transaction_challenge_status_reason\\\":\\\"01\\\",\\\"card_holder_info\\\":null,\\\"acs_rendering_type\\\":null,\\\"acs_challenge_mandated\\\":null,\\\"acs_operator_id\\\":null,\\\"acs_reference_number\\\":null,\\\"authentication_type\\\":null,\\\"ds_reference_number\\\":null,\\\"is_form_post_acs_url\\\":true,\\\"is_full_page_redirect\\\":true},\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-US\\\",\\\"SuccessUrl\\\":\\\"https://bing.com?succeeded=true\\\",\\\"FailureUrl\\\":\\\"https://bing.com?succeeded=false\\\",\\\"ChallengeStatus\\\":\\\"Unknown\\\",\\\"payment_session_id\\\":\\\"Z10005BGAPLA4192d3ac-1071-4db4-9e9d-612e08b62946\\\",\\\"account_id\\\":\\\"bd888d21-f2a9-4f8b-92c2-129b8d4748b6\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"mEQdBgAAAAASAACA\\\",\\\"partner\\\":\\\"webblends\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"IN\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"visa\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null,\\\"cvv_token \\\":null}\",\"encrypt_data\":false,\"state\":\"INCOMPLETE\",\"test_context\":{\"scenarios\":\"px-service-billdesk-provider,px-service-psd2-e2e-emulator\",\"contact\":\"test\",\"context_props\":{}}}";

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(paymentSessionId, response);

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, GetPXServiceUrl(string.Format("/v7.0/paymentSessions/{0}/browserNotifyThreeDSOneChallengeCompleted", paymentSessionId)))
            {
                Content = new StringContent("PaRes=eyJhdXRoZW50aWNhdGlvbmlkIjoiMmQ4YWY4NzktYjdlMi00NDVmLWFjNWUtNGEwMGQ0MDJhZjRlIiwiYXV0aGVudGljYXRpb25fc3RhdHVzIjoic3VjY2VzcyJ9", Encoding.UTF8, PaymentConstants.HttpMimeTypes.FormContentType)
            };

            var pxResponse = await PXClient.SendAsync(requestMessage);

            Assert.AreEqual(pxResponse.StatusCode, HttpStatusCode.OK);
            var responseContent = await pxResponse.Content.ReadAsStringAsync();
            Assert.IsTrue(responseContent.Contains("https://bing.com/?succeeded=true"));
        }

        [TestMethod]
        public async Task PaymentSessions_BrowserNotifyThreeDSOneChallengeCompleted_Iframe()
        {
            // Arrange
            string paymentSessionId = "ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6";
            string sessionResp = "{\"id\":\"Z10005BGAPLA4192d3ac-1071-4db4-9e9d-612e08b62946\",\"session_type\":\"any\",\"data\":\"{\\\"ExposedFlightFeatures\\\":[\\\"PXPSD2ProdIntegration\\\",\\\"PXEnablePaypalRedirectUrlText\\\",\\\"PXProfileUpdateToHapi\\\",\\\"PXPSD2Auth-_-_-Succeeded\\\",\\\"PXPSD2Auth-C-_-Unknown\\\",\\\"PXPSD2Auth-N-TSR10-Failed\\\",\\\"PXPSD2Auth-R-_-Failed\\\",\\\"PXPSD2Comp-_-_-_-Succeeded\\\",\\\"PXPSD2Comp-_-_-01-Cancelled\\\",\\\"PXPSD2Comp-N-_-04-TimedOut\\\",\\\"PXPSD2Comp-N-TSR10-_-Failed\\\",\\\"PXPSD2Comp-R-_-_-Failed\\\",\\\"PXPSD2SettingVersionV19\\\"],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"bd888d21-f2a9-4f8b-92c2-129b8d4748b6\\\",\\\"BrowserInfo\\\":{\\\"browser_accept_header\\\":\\\"*/*\\\",\\\"browser_ip\\\":\\\"::1\\\",\\\"browser_java_enabled\\\":true,\\\"browser_language\\\":\\\"en-US\\\",\\\"browser_color_depth\\\":\\\"8\\\",\\\"browser_screen_height\\\":\\\"480\\\",\\\"browser_screen_width\\\":\\\"640\\\",\\\"browser_tz\\\":\\\"480\\\",\\\"browser_user_agent\\\":\\\"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.93 Safari/537.36\\\",\\\"challenge_window_size\\\":\\\"05\\\"},\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":{\\\"enrollment_status\\\":\\\"bypassed\\\",\\\"enrollment_type\\\":\\\"three_ds\\\",\\\"three_ds_server_transaction_id\\\":\\\"b643dce6-fe1e-4c04-a6d6-3b173c4c95d2\\\",\\\"acs_url\\\":\\\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\\\",\\\"acs_transaction_id\\\":null,\\\"authenticate_update_url\\\":\\\"/bd888d21-f2a9-4f8b-92c2-129b8d4748b6/payments/Z10005BGAPLA4192d3ac-1071-4db4-9e9d-612e08b62946/authenticate\\\",\\\"acs_signed_content\\\":\\\"{\\\\\\\"MD\\\\\\\":\\\\\\\"A4560000307199\\\\\\\",\\\\\\\"PaReq\\\\\\\":\\\\\\\"eyJhdXRoZW50aWNhdGlvbmlkIjoiYjY0M2RjZTYtZmUxZS00YzA0LWE2ZDYtM2IxNzNjNGM5NWQyIn0=\\\\\\\",\\\\\\\"TermUrl\\\\\\\":\\\\\\\"https://www.merchanturl.com/Response.jsp\\\\\\\"}\\\",\\\"transaction_challenge_status\\\":\\\"C\\\",\\\"transaction_challenge_status_reason\\\":\\\"01\\\",\\\"card_holder_info\\\":null,\\\"acs_rendering_type\\\":null,\\\"acs_challenge_mandated\\\":null,\\\"acs_operator_id\\\":null,\\\"acs_reference_number\\\":null,\\\"authentication_type\\\":null,\\\"ds_reference_number\\\":null,\\\"is_form_post_acs_url\\\":true,\\\"is_full_page_redirect\\\":true},\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-US\\\",\\\"ChallengeStatus\\\":\\\"Unknown\\\",\\\"payment_session_id\\\":\\\"Z10005BGAPLA4192d3ac-1071-4db4-9e9d-612e08b62946\\\",\\\"account_id\\\":\\\"bd888d21-f2a9-4f8b-92c2-129b8d4748b6\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"mEQdBgAAAAASAACA\\\",\\\"partner\\\":\\\"webblends\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"IN\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"visa\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null,\\\"cvv_token \\\":null}\",\"encrypt_data\":false,\"state\":\"INCOMPLETE\",\"test_context\":{\"scenarios\":\"px-service-billdesk-provider,px-service-psd2-e2e-emulator\",\"contact\":\"test\",\"context_props\":{}}}";
            string payAuthResp = "{\"authenticate_value\":\"{\\\"authorization_parameters\\\":{\\\"PaRes\\\":\\\"eyJhdXRoZW50aWNhdGlvbmlkIjoiYWYzOGM0ZjItNTExYi00YTVhLTg3MGMtNzg0NGVhZGFjMzlhIiwiYXV0aGVudGljYXRpb25fc3RhdHVzIjoic3VjY2VzcyJ9\\\",\\\"xid\\\":\\\"ABXcpervt=\\\",\\\"pares\\\":\\\"eyJhdXRoZW50aWNhdGlvbmlkIjoiYWYzOGM0ZjItNTExYi00YTVhLTg3MGMtNzg0NGVhZGFjMzlhIn0=\\\",\\\"md\\\":null,\\\"cavv\\\":\\\"CGtpseerccma==\\\",\\\"cavvAlgorithm\\\":\\\"2\\\"}}\",\"eci\":\"05\",\"transaction_challenge_status\":\"Y\"}";
            PXSettings.SessionService.ResponseProvider.SessionStore.Add(paymentSessionId, sessionResp);
            PXSettings.PayerAuthService.ArrangeResponse(
                method: HttpMethod.Post,
                urlPattern: ".*/CompleteChallenge.*",
                statusCode: HttpStatusCode.OK,
                content: payAuthResp);

            PXSettings.PimsService.ArrangeResponse(
                method: HttpMethod.Post,
                urlPattern: ".*/LinkTransaction.*",
                statusCode: HttpStatusCode.OK,
                content: string.Empty);

            // Act
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, GetPXServiceUrl(string.Format("/v7.0/paymentSessions/{0}/browserNotifyThreeDSOneChallengeCompleted", paymentSessionId)))
            {
                Content = new StringContent("PaRes=eyJhdXRoZW50aWNhdGlvbmlkIjoiMmQ4YWY4NzktYjdlMi00NDVmLWFjNWUtNGEwMGQ0MDJhZjRlIiwiYXV0aGVudGljYXRpb25fc3RhdHVzIjoic3VjY2VzcyJ9", Encoding.UTF8, PaymentConstants.HttpMimeTypes.FormContentType)
            };

            var pxResponse = await PXClient.SendAsync(requestMessage);

            // Assert
            Assert.AreEqual(pxResponse.StatusCode, HttpStatusCode.OK);
            Assert.IsNull(pxResponse.Content);
        }

        [DataRow(Partners.Webblends)]
        [DataRow(Partners.XBox)]
        [DataRow(Partners.AmcXbox)]
        [DataRow(Partners.Storify)]
        [DataRow(Partners.Saturn)]
        [DataRow(Partners.Xbet)]
        [DataRow(Partners.Payin)]
        [DataRow(Partners.OfficeOobe)]
        [DataRow(Partners.OXOOobe)]
        [DataRow(Partners.WebPay)]
        [DataRow(Partners.ConsumerSupport)]
        [DataRow("testHPC_consumer", true)]
        [TestMethod]
        public async Task PaymentSessions_GetPaymentSessionStatus(string partner, bool useParnterSettingsService = false)
        {
            // This test does not need to test any inline redirection partners, as they will never call GetPaymentSession
            List<string> challengeStatusList = new List<string> { "Success", "Failed", "Unknown", "InternalServerError", "TimedOut" };

            foreach (string challengeStatus in challengeStatusList)
            {
                PXSettings.SessionService.ResponseProvider.SessionStore.Clear();

                // Arrange
                string accountId = "Account001";
                string paymentSessionId = "ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6";
                string response = "{\"Id\":\"" + paymentSessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"Account001\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":{\\\"enrollment_status\\\":\\\"bypassed\\\",\\\"enrollment_type\\\":\\\"three_ds\\\",\\\"three_ds_server_transaction_id\\\":\\\"73f512ef-48fb-4322-b87d-ce6a3622ef1b\\\",\\\"acs_url\\\":\\\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\\\",\\\"acs_transaction_id\\\":null,\\\"authenticate_update_url\\\":\\\"/bd888d21-f2a9-4f8b-92c2-129b8d4748b6/payments/Z10007BHY0DT9b144cbe-f57f-4af2-85e3-dfd2c023d4ec/authenticate\\\",\\\"acs_signed_content\\\":\\\"{\\\\\\\"MD\\\\\\\":\\\\\\\"A4560000307199\\\\\\\",\\\\\\\"PaReq\\\\\\\":\\\\\\\"eyJhdXRoZW50aWNhdGlvbmlkIjoiNzNmNTEyZWYtNDhmYi00MzIyLWI4N2QtY2U2YTM2MjJlZjFiIn0=\\\\\\\",\\\\\\\"TermUrl\\\\\\\":\\\\\\\"https://www.merchanturl.com/Response.jsp\\\\\\\"}\\\",\\\"transaction_challenge_status\\\":\\\"C\\\",\\\"transaction_challenge_status_reason\\\":\\\"01\\\",\\\"card_holder_info\\\":\\\"TestCardHolderInfo\\\",\\\"acs_rendering_type\\\":null,\\\"acs_challenge_mandated\\\":null,\\\"acs_operator_id\\\":null,\\\"acs_reference_number\\\":null,\\\"authentication_type\\\":null,\\\"ds_reference_number\\\":null,\\\"is_form_post_acs_url\\\":true,\\\"is_full_page_redirect\\\":true},\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + paymentSessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"Account001-Pi001-Visa\\\",\\\"partner\\\":\\\"" + partner + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":false,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

                PXSettings.SessionService.ResponseProvider.SessionStore.Add(paymentSessionId, response);

                if (useParnterSettingsService)
                {
                    string settingJson = "{\"handlepaymentchallenge\":{\"template\":\"defaultTemplate\",\"redirectionPattern\":\"fullPage\",\"features\":{\"threeDSOne\":{\"applicableMarkets\":[\"in\"],\"displayCustomizationDetail\":[{\"india3dsEnableForBilldesk\":true,\"pxEnableIndia3DS1Challenge\":true,}]}}}}";
                    PXSettings.PartnerSettingsService.ArrangeResponse(settingJson);
                }

                HttpRequestMessage request = new HttpRequestMessage
                {
                    RequestUri = new Uri(GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/status", accountId, paymentSessionId))),
                    Method = HttpMethod.Get
                };

                if (string.Equals(challengeStatus, "Success"))
                {
                    request.Headers.Add("x-ms-test", "{\"scenarios\":\"px-service-3ds1-test-emulator,px-service-3ds1-test-emulator-challenge-success\",}");
                }
                else if (string.Equals(challengeStatus, "Failed"))
                {
                    request.Headers.Add("x-ms-test", "{\"scenarios\":\"px-service-3ds1-test-emulator,px-service-3ds1-test-emulator-challenge-failed\",}");
                }
                else if (string.Equals(challengeStatus, "InternalServerError"))
                {
                    request.Headers.Add("x-ms-test", "{\"scenarios\":\"px-service-3ds1-test-emulator,px-service-3ds1-test-emulator-challenge-internalServerError\",}");
                }
                else if (string.Equals(challengeStatus, "TimedOut"))
                {
                    request.Headers.Add("x-ms-test", "{\"scenarios\":\"px-service-3ds1-test-emulator,px-service-3ds1-test-emulator-challenge-timeOut\",}");
                }

                // Act
                var pxResponse = await PXClient.SendAsync(request);

                Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);

                bool gotResource = pxResponse.TryGetContentValue(out Microsoft.Commerce.Payments.PidlModel.V7.PIDLResource resource);

                Assert.IsNotNull(resource);
                Assert.IsTrue(gotResource);

                // Assert
                if (string.Equals(challengeStatus, "Success"))
                {
                    Assert.AreEqual(resource.ClientAction.ActionType, Microsoft.Commerce.Payments.PXCommon.ClientActionType.ReturnContext);

                    PaymentSession session = resource?.ClientAction?.Context as PaymentSession;

                    Assert.IsNotNull(session);
                    Assert.AreEqual(session.Id, paymentSessionId);
                    Assert.AreEqual(session.ChallengeStatus, PaymentChallengeStatus.Succeeded);
                }
                else if (string.Equals(challengeStatus, "Failed") || string.Equals(challengeStatus, "InternalServerError") || string.Equals(challengeStatus, "TimedOut"))
                {
                    Assert.AreEqual(resource.ClientAction.ActionType, Microsoft.Commerce.Payments.PXCommon.ClientActionType.Failure);

                    if (!IsThreeDSOneQrCodeBasedPurchasePartner(partner))
                    {
                        ServiceErrorResponse serviceError = resource?.ClientAction?.Context as ServiceErrorResponse;
                        Assert.AreEqual(serviceError.Message, "Invalid ThreeDS session status");
                        Assert.AreEqual(serviceError.ErrorCode, string.Equals(challengeStatus, "InternalServerError") ? HttpStatusCode.InternalServerError.ToString() : HttpStatusCode.BadRequest.ToString());
                        Assert.AreEqual(serviceError.InnerError.UserDisplayMessage, "TestCardHolderInfo");
                    }
                    else
                    {
                        PaymentSession session = resource?.ClientAction?.Context as PaymentSession;

                        Assert.IsNotNull(session);
                        Assert.AreEqual(session.Id, paymentSessionId);
                        Assert.AreEqual(session.ChallengeStatus.ToString(), challengeStatus);
                    }
                }
                else if (string.Equals(challengeStatus, "Unknown"))
                {
                    if (!string.Equals(partner, Partners.Webblends) && !useParnterSettingsService)
                    {
                        Assert.AreEqual(resource.ClientAction.ActionType, Microsoft.Commerce.Payments.PXCommon.ClientActionType.ReturnContext);

                        PaymentSession session = resource?.ClientAction?.Context as PaymentSession;

                        Assert.IsNotNull(session);
                        Assert.AreEqual(session.Id, paymentSessionId);
                        Assert.AreEqual(session.ChallengeStatus, PaymentChallengeStatus.Unknown);
                    }
                    else
                    {
                        Assert.AreEqual(resource.ClientAction.ActionType, Microsoft.Commerce.Payments.PXCommon.ClientActionType.Pidl);

                        var pidl = ReadPidlResourceFromJson(JsonConvert.SerializeObject(resource.ClientAction.Context)).First();

                        Assert.IsNotNull(pidl);
                        Assert.AreEqual(pidl.DisplayPages.Count(), 1);

                        ButtonDisplayHint cc3DSYesButton = pidl.GetDisplayHintById("cc3DSYesVerificationButton") as ButtonDisplayHint;
                        ButtonDisplayHint cc3DSTryAgainButton = pidl.GetDisplayHintById("cc3DSRetryButton") as ButtonDisplayHint;
                        ButtonDisplayHint cc3DSCancelButton = pidl.GetDisplayHintById("cc3DSCancelVerificationButton") as ButtonDisplayHint;

                        Assert.IsNotNull(cc3DSYesButton);
                        Assert.IsNotNull(cc3DSYesButton.Action);
                        Assert.AreEqual(cc3DSYesButton.Action.ActionType, "submit");
                        Assert.AreEqual(cc3DSYesButton.Action.IsDefault, true);

                        Assert.IsNotNull(cc3DSTryAgainButton);
                        Assert.IsNotNull(cc3DSTryAgainButton.Action);
                        Assert.IsTrue(cc3DSTryAgainButton.Action.Context.ToString().Contains($"pifd.cp.microsoft-int.com/V6.0/paymentSessions/{paymentSessionId}/browserAuthenticateRedirectionThreeDSOne"));
                        Assert.AreEqual(cc3DSTryAgainButton.Action.ActionType, "redirect");

                        Assert.IsNotNull(cc3DSCancelButton);
                        Assert.IsNotNull(cc3DSCancelButton.Action);
                        Assert.AreEqual(cc3DSCancelButton.Action.ActionType, "gohome");
                    }
                }
            }
        }

        private static bool IsThreeDSOneQrCodeBasedPurchasePartner(string partnerName)
        {
            return string.Equals(partnerName, Partners.Storify, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(partnerName, Partners.Saturn, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(partnerName, Partners.XBox, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(partnerName, Partners.AmcXbox, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(partnerName, Partners.Xbet, StringComparison.InvariantCultureIgnoreCase);
        }

        [DataRow(Partners.Azure)]
        [DataRow(Partners.CommercialStores)]
        [DataRow(Partners.Webblends)]
        [TestMethod]
        public async Task PaymentSessions_GetPaymentSessionStatus_BadSessionId(string partner)
        {
            // Arrange
            string accountId = "Account001";
            string paymentSessionId = "ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6";
            string badPaymentSessionId = "ZFFFFFFFFFFF78a93cbb-49d9-8a57-84b7-42c5f042dba6";
            string response = "{\"Id\":\"" + paymentSessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"Account001\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + paymentSessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"Account001-Pi001-Visa\\\",\\\"partner\\\":\\\"" + partner + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":false,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(paymentSessionId, response);

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/status", accountId, badPaymentSessionId)));

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, pxResponse.StatusCode);
        }

        [DataRow(Partners.Azure)]
        [DataRow(Partners.CommercialStores)]
        [TestMethod]
        public async Task PaymentSessions_Get_BadAccountId(string partner)
        {
            // Arrange
            string accountId = "Account001";
            string badAccountId = "Account002";
            string paymentSessionId = "ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6";
            string response = "{\"Id\":\"" + paymentSessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"Account001\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + paymentSessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"Account001-Pi001-Visa\\\",\\\"partner\\\":\\\"" + partner + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":false,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(paymentSessionId, response);

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/status", badAccountId, paymentSessionId)));

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, pxResponse.StatusCode);
        }

        /// <summary>
        /// PI in the paymentSessionData object does not require a 3DS2 challenge, so triggers a Validate PI On Attach for specific partners based on the scenario
        /// </summary>
        [TestMethod]
        [DataRow(10, true, "amcweb", true)]
        [DataRow(0, true, "officeoobe", true)]
        [DataRow(0, true, "oxooobe", true)]
        [DataRow(0, false, "webblends", true)]
        [DataRow(10, false, "amcweb", true)]
        [DataRow(10, false, "officeoobe", true)]
        [DataRow(10, false, "oxooobe", true)]
        [DataRow(10, false, "webblends", true)]
        [DataRow(10, true, "commercialstores", false)]
        [DataRow(0, true, "azure", false)]
        [DataRow(0, false, "xbox", false)]
        [DataRow(0, false, "officesmb", true, true)]
        [DataRow(10, false, "officesmb", true, true)]
        [DataRow(0, false, "officesmb", false)] // For OfficeSMB without the feature, there is no challenge.
        [DataRow(10, false, "officesmb", false)] // For OfficeSMB without the feature, there is no challenge.
        public async Task PaymentSessionDescriptions_ValidatePIOnAttach(int amount, bool hasPreOrder, string partner, bool validatePIOnAttachExpected, bool isFeatureEnableValidatePIOnAttachChallenge = false)
        {
            // Arrange
            PXSettings.PayerAuthService.ResponseProvider.SessionId = Guid.NewGuid().ToString();

            if (string.Equals(partner, PXService.GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase))
            {
                string expectedPSSResponse = isFeatureEnableValidatePIOnAttachChallenge
                    ? "{\"handlePaymentChallenge\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"features\":{\"PXUsePSSToEnableValidatePIOnAttachChallenge\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]}}}}"
                    : "{\"handlePaymentChallenge\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\"}}";

                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
            }

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/Account001/PaymentSessionDescriptions?operation=Add&language=en-GB&paymentSessionData={0}",
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                            new PaymentSessionData()
                            {
                                Amount = amount,
                                Currency = "EUR",
                                Partner = partner,
                                Country = "gb",
                                HasPreOrder = hasPreOrder,
                                PaymentInstrumentId = "Account001-Pi001-Visa",
                                Language = "en"
                            })))));

            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);

            var pidl = ReadPidlResourceFromJson(await pxResponse.Content.ReadAsStringAsync());
            Assert.AreEqual(ClientActionType.ReturnContext, pidl[0].ClientAction.ActionType);

            var paymentSession = JsonConvert.DeserializeObject<PaymentSession>(pidl[0].ClientAction.Context.ToString());
            Assert.AreEqual(validatePIOnAttachExpected, paymentSession.IsChallengeRequired);

            if (string.Equals(partner, PXService.GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase))
            {
                if (isFeatureEnableValidatePIOnAttachChallenge)
                {
                    Assert.AreEqual("ValidatePIOnAttachChallenge", paymentSession.ChallengeType);
                    Assert.AreEqual(PaymentChallengeStatus.Unknown, paymentSession.ChallengeStatus);
                }
                else
                {
                    Assert.IsNull(paymentSession.ChallengeType);
                    Assert.AreEqual(PaymentChallengeStatus.NotApplicable, paymentSession.ChallengeStatus);
                }
            }
            else if (validatePIOnAttachExpected)
            {
                Assert.AreEqual("ValidatePIOnAttachChallenge", paymentSession.ChallengeType);
                Assert.AreEqual(PaymentChallengeStatus.Unknown, paymentSession.ChallengeStatus);
            }
            else
            {
                Assert.IsNull(paymentSession.ChallengeType);
                Assert.AreEqual(PaymentChallengeStatus.NotApplicable, paymentSession.ChallengeStatus);
            }
        }

        [TestMethod]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, true)]
        [DataRow(PXService.GlobalConstants.Partners.Webblends, false)]
        public async Task PaymentSessionDescriptions_EnableThreeDSOne_ParnterFlight(string partner, bool sendPartnerFlight)
        {
            // Arrange
            PXSettings.PayerAuthService.ResponseProvider.SessionId = Guid.NewGuid().ToString();

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi001-Visa");
            extendedPI.PaymentInstrumentDetails.RequiredChallenge = new List<string> { "3ds" };
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            // Act
            string url = string.Format(
                        "/v7.0/Account001/PaymentSessionDescriptions?operation=Add&language=en-GB&paymentSessionData={0}",
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                            new PaymentSessionData()
                            {
                                Amount = 800,
                                Currency = "INR",
                                Partner = partner,
                                Country = "in",
                                PaymentInstrumentId = "Account001-Pi001-Visa",
                                Language = "en",
                                ChallengeScenario = ChallengeScenario.PaymentTransaction
                            })));

            Dictionary<string, string> testHeader = new Dictionary<string, string>();

            if (sendPartnerFlight)
            {
                testHeader["x-ms-flight"] = "EnableThreeDSOne,India3dsEnableForBilldesk";
            }

            // Act
            await GetRequest(
                url,
                testHeader,
                null,
                (responseCode, responseBody, responseHeaders) =>
                {
                    // Assert
                    var pidl = ReadPidlResourceFromJson(responseBody);
                    Assert.IsNotNull(pidl);

                    var paymentSession = JsonConvert.DeserializeObject<PaymentSession>(pidl[0].ClientAction.Context.ToString());

                    Assert.AreEqual(HttpStatusCode.OK, responseCode);
                    Assert.AreEqual(sendPartnerFlight, responseHeaders.GetValues("x-ms-flight").First().IndexOf("PXEnableIndia3DS1Challenge") > 0);
                    if (sendPartnerFlight)
                    {
                        Assert.AreEqual("India3DSChallenge", paymentSession.ChallengeType);
                    }
                    else
                    {
                        Assert.AreEqual(paymentSession.ChallengeStatus.ToString(), "NotApplicable");
                    }
                });
        }

        /// <summary>
        /// PI in the paymentSessionOrData object does not require a 3DS2 challenge, so triggers a Validate PI On Attach for specific partners based on the scenario
        /// Call ChallengeDescriptions with PaymentSessionData, instead of the payment session from PaymentSessionDescriptions
        /// </summary>
        [TestMethod]
        [DataRow(10, true, "amcweb", true, false)]
        [DataRow(0, true, "officeoobe", true, false)]
        [DataRow(0, true, "oxooobe", true, false)]
        [DataRow(0, false, "webblends", true, false)]
        [DataRow(10, false, "amcweb", true, false)]
        [DataRow(10, false, "officeoobe", true, false)]
        [DataRow(10, false, "officeoobe", true, false)]
        [DataRow(10, false, "oxooobe", true, false)]
        [DataRow(10, true, "commercialstores", false, false)]
        [DataRow(0, true, "azure", false, false)]
        [DataRow(0, false, "xbox", false, false)]
        [DataRow(10, true, "amcweb", true, true)]
        [DataRow(0, true, "officeoobe", true, true)]
        [DataRow(0, true, "oxooobe", true, true)]
        [DataRow(0, false, "webblends", true, true)]
        [DataRow(10, false, "officesmb", true, false, true)]
        [DataRow(10, false, "officesmb", true, false, true)]
        [DataRow(0, true, "officesmb", true, false, true)]
        [DataRow(0, true, "officesmb", true, true, true)]
        [DataRow(10, false, "officesmb", false, false)] // For OfficeSMB without the feature, there is no challenge.
        [DataRow(0, true, "officesmb", false, false)] // For OfficeSMB without the feature, there is no challenge.
        [DataRow(0, true, "officesmb", true, true)] // For OfficeSMB with the feature, there is a challenge.
        public async Task ChallengeDescriptions_ValidatePIOnAttach(int amount, bool hasPreOrder, string partner, bool validatePIOnAttachExpected, bool pimsReturnsValidatePIFailed, bool isFeatureEnableValidatePIOnAttachChallenge = false)
        {
            // Arrange
            PXSettings.PayerAuthService.ResponseProvider.SessionId = Guid.NewGuid().ToString();
            PXSettings.PimsService.PostProcess = async (pimsValidateResponse) =>
            {
                if (pimsReturnsValidatePIFailed && pimsValidateResponse?.RequestMessage != null && pimsValidateResponse.RequestMessage.RequestUri.AbsolutePath.Contains("validate"))
                {
                    if (pimsValidateResponse.Content != null)
                    {
                        string requestContent = await pimsValidateResponse.Content.ReadAsStringAsync();
                        requestContent = requestContent.Replace("Success", "Failed");
                        pimsValidateResponse.Content = new StringContent(requestContent, Encoding.UTF8, "application/json");
                    }
                }
            };

            if (string.Equals(partner, PXService.GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase))
            {
                string expectedPSSResponse = isFeatureEnableValidatePIOnAttachChallenge
                    ? "{\"handlePaymentChallenge\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"features\":{\"PXUsePSSToEnableValidatePIOnAttachChallenge\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]}}}}"
                    : "{\"handlePaymentChallenge\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\"}}";

                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
            }

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/Account001/ChallengeDescriptions?operation=RenderPidlPage&language=en-GB&paymentSessionOrData={0}",
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                            new PaymentSessionData()
                            {
                                Amount = amount,
                                Currency = "EUR",
                                Partner = partner,
                                Country = "gb",
                                HasPreOrder = hasPreOrder,
                                PaymentInstrumentId = "Account001-Pi001-Visa",
                                Language = "en"
                            })))));

            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);

            var pidl = ReadPidlResourceFromJson(await pxResponse.Content.ReadAsStringAsync());

            if (!pimsReturnsValidatePIFailed)
            {
                Assert.AreEqual(ClientActionType.ReturnContext, pidl[0].ClientAction.ActionType);

                var paymentSession = JsonConvert.DeserializeObject<PaymentSession>(pidl[0].ClientAction.Context.ToString());
                Assert.AreEqual(validatePIOnAttachExpected, paymentSession.IsChallengeRequired);

                if (string.Equals(partner, PXService.GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase))
                {
                    if (isFeatureEnableValidatePIOnAttachChallenge)
                    {
                        Assert.AreEqual("ValidatePIOnAttachChallenge", paymentSession.ChallengeType);
                        Assert.AreEqual(PaymentChallengeStatus.Succeeded, paymentSession.ChallengeStatus);
                    }
                    else
                    {
                        Assert.IsNull(paymentSession.ChallengeType);
                        Assert.AreEqual(PaymentChallengeStatus.NotApplicable, paymentSession.ChallengeStatus);
                    }
                }
                else if (validatePIOnAttachExpected)
                {
                    Assert.AreEqual("ValidatePIOnAttachChallenge", paymentSession.ChallengeType);
                    Assert.AreEqual(PaymentChallengeStatus.Succeeded, paymentSession.ChallengeStatus);
                }
                else
                {
                    Assert.IsNull(paymentSession.ChallengeType);
                    Assert.AreEqual(PaymentChallengeStatus.NotApplicable, paymentSession.ChallengeStatus);
                }
            }
            else
            {
                if (string.Equals(partner, PXService.GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase) && !isFeatureEnableValidatePIOnAttachChallenge)
                {
                    Assert.AreEqual(ClientActionType.ReturnContext, pidl[0].ClientAction.ActionType);
                    var paymentSession = JsonConvert.DeserializeObject<PaymentSession>(pidl[0].ClientAction.Context.ToString());
                    Assert.AreEqual(PaymentChallengeStatus.NotApplicable, paymentSession.ChallengeStatus);
                }
                else
                {
                    Assert.AreEqual(ClientActionType.Failure, pidl[0].ClientAction.ActionType);
                    var context = JsonConvert.DeserializeObject<ServiceErrorResponse>(pidl[0].ClientAction.Context.ToString());
                    Assert.AreEqual(PaymentChallengeStatus.Failed.ToString(), context.ErrorCode);
                    Assert.AreEqual("ValidatePIOnAttachFailed", context.InnerError.ErrorCode);
                    Assert.AreEqual(PaymentChallengeStatus.Failed.ToString(), context.InnerError.Message);
                }
            }

            PXSettings.PimsService.ResetToDefaults();
        }

        /// <summary>
        /// PI in the paymentSessionOrData object does not require a 3DS2 challenge, so triggers a Validate PI On Attach for specific partners based on the scenario
        /// Call ChallengeDescriptions with the payment session from PaymentSessionDescriptions
        /// </summary>
        [TestMethod]
        [DataRow(10, true, "amcweb", true, false)]
        [DataRow(0, true, "officeoobe", true, false)]
        [DataRow(0, true, "oxooobe", true, false)]
        [DataRow(0, false, "webblends", true, false)]
        [DataRow(10, false, "amcweb", true, false)]
        [DataRow(10, false, "officeoobe", true, false)]
        [DataRow(10, false, "oxooobe", true, false)]
        [DataRow(10, false, "webblends", true, false)]
        [DataRow(10, true, "commercialstores", false, false)]
        [DataRow(0, true, "azure", false, false)]
        [DataRow(0, false, "xbox", false, false)]
        [DataRow(10, true, "amcweb", true, true)]
        [DataRow(0, true, "officeoobe", true, true)]
        [DataRow(0, true, "oxooobe", true, true)]
        [DataRow(0, false, "webblends", true, true)]
        [DataRow(10, false, "officesmb", true, false, true)]
        [DataRow(10, false, "officesmb", true, false, true)]
        [DataRow(0, true, "officesmb", true, false, true)]
        [DataRow(0, true, "officesmb", true, true, true)]
        [DataRow(10, false, "officesmb", false, false)] // For OfficeSMB without the feature, there is no challenge.
        [DataRow(0, true, "officesmb", false, false)] // For OfficeSMB without the feature, there is no challenge.
        [DataRow(0, true, "officesmb", true, true)] // For OfficeSMB with the feature, there is a challenge.
        public async Task CreatePaymentSession_ChallengeDescriptions_ValidatePIOnAttach(int amount, bool hasPreOrder, string partner, bool validatePIOnAttachExpected, bool pimsReturnsValidatePIFailed, bool isFeatureEnableValidatePIOnAttachChallenge = false)
        {
            // Arrange
            PXSettings.PayerAuthService.ResponseProvider.SessionId = Guid.NewGuid().ToString();
            PXSettings.PimsService.PostProcess = async (pimsValidateResponse) =>
            {
                if (pimsReturnsValidatePIFailed && pimsValidateResponse?.RequestMessage != null && pimsValidateResponse.RequestMessage.RequestUri.AbsolutePath.Contains("validate"))
                {
                    if (pimsValidateResponse.Content != null)
                    {
                        string requestContent = await pimsValidateResponse.Content.ReadAsStringAsync();
                        requestContent = requestContent.Replace("Success", "Failed");
                        pimsValidateResponse.Content = new StringContent(requestContent, Encoding.UTF8, "application/json");
                    }
                }
            };

            // Arrange
            PXSettings.PayerAuthService.ResponseProvider.SessionId = Guid.NewGuid().ToString();

            if (string.Equals(partner, PXService.GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase))
            {
                string expectedPSSResponse = isFeatureEnableValidatePIOnAttachChallenge
                    ? "{\"handlePaymentChallenge\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"features\":{\"PXUsePSSToEnableValidatePIOnAttachChallenge\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]}}}}"
                    : "{\"handlePaymentChallenge\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\"}}";

                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
            }

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/Account001/PaymentSessionDescriptions?operation=Add&language=en-GB&paymentSessionData={0}",
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                            new PaymentSessionData()
                            {
                                Amount = amount,
                                Currency = "EUR",
                                Partner = partner,
                                Country = "gb",
                                HasPreOrder = hasPreOrder,
                                PaymentInstrumentId = "Account001-Pi001-Visa",
                                Language = "en"
                            })))));

            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);

            var pidl = ReadPidlResourceFromJson(await pxResponse.Content.ReadAsStringAsync());
            Assert.AreEqual(ClientActionType.ReturnContext, pidl[0].ClientAction.ActionType);
            var paymentSession = JsonConvert.DeserializeObject<PaymentSession>(pidl[0].ClientAction.Context.ToString());

            pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/Account001/ChallengeDescriptions?operation=RenderPidlPage&language=en-GB&paymentSessionOrData={0}",
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(paymentSession)))));

            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);

            pidl = ReadPidlResourceFromJson(await pxResponse.Content.ReadAsStringAsync());

            if (!pimsReturnsValidatePIFailed)
            {
                Assert.AreEqual(ClientActionType.ReturnContext, pidl[0].ClientAction.ActionType);

                paymentSession = JsonConvert.DeserializeObject<PaymentSession>(pidl[0].ClientAction.Context.ToString());
                Assert.AreEqual(validatePIOnAttachExpected, paymentSession.IsChallengeRequired);

                if (string.Equals(partner, PXService.GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase))
                {
                    if (isFeatureEnableValidatePIOnAttachChallenge)
                    {
                        Assert.AreEqual("ValidatePIOnAttachChallenge", paymentSession.ChallengeType);
                        Assert.AreEqual(PaymentChallengeStatus.Succeeded, paymentSession.ChallengeStatus);
                    }
                    else
                    {
                        Assert.IsNull(paymentSession.ChallengeType);
                        Assert.AreEqual(PaymentChallengeStatus.NotApplicable, paymentSession.ChallengeStatus);
                    }
                }
                else if (validatePIOnAttachExpected)
                {
                    Assert.AreEqual("ValidatePIOnAttachChallenge", paymentSession.ChallengeType);
                    Assert.AreEqual(PaymentChallengeStatus.Succeeded, paymentSession.ChallengeStatus);
                }
                else
                {
                    Assert.IsNull(paymentSession.ChallengeType);
                    Assert.AreEqual(PaymentChallengeStatus.NotApplicable, paymentSession.ChallengeStatus);
                }
            }
            else
            {
                if (string.Equals(partner, PXService.GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase) && !isFeatureEnableValidatePIOnAttachChallenge)
                {
                    Assert.AreEqual(ClientActionType.ReturnContext, pidl[0].ClientAction.ActionType);
                    paymentSession = JsonConvert.DeserializeObject<PaymentSession>(pidl[0].ClientAction.Context.ToString());
                    Assert.AreEqual(PaymentChallengeStatus.NotApplicable, paymentSession.ChallengeStatus);
                }
                else
                {
                    Assert.AreEqual(ClientActionType.Failure, pidl[0].ClientAction.ActionType);
                    var context = JsonConvert.DeserializeObject<ServiceErrorResponse>(pidl[0].ClientAction.Context.ToString());
                    Assert.AreEqual(PaymentChallengeStatus.Failed.ToString(), context.ErrorCode);
                    Assert.AreEqual("ValidatePIOnAttachFailed", context.InnerError.ErrorCode);
                    Assert.AreEqual(PaymentChallengeStatus.Failed.ToString(), context.InnerError.Message);
                }
            }

            PXSettings.PimsService.ResetToDefaults();
        }

        /// <summary>
        /// PI in the paymentSessionOrData object does not require a 3DS2 challenge, so triggers a Validate PI On Attach for specific partners based on the scenario
        /// Call ChallengeDescriptions with the payment session from PaymentSessionDescriptions
        /// </summary>
        [TestMethod]
        [DataRow(10, false, "officeoobe", true)]
        [DataRow(10, false, "officeoobe", false)]
        [DataRow(0, true, "officeoobe", true)]
        [DataRow(0, true, "officeoobe", false)]
        [DataRow(10, true, "officeoobe", true)]
        [DataRow(10, true, "officeoobe", false)]
        [DataRow(10, false, "officesmb", true)]
        [DataRow(10, false, "officesmb", false)]
        [DataRow(0, true, "officesmb", true)]
        [DataRow(0, true, "officesmb", false)]
        [DataRow(10, true, "officesmb", true)]
        [DataRow(10, true, "officesmb", false)]
        public async Task CreatePaymentSession_ChallengeDescriptions_ValidatePIOnAttach_ApplePay(int amount, bool hasPreOrder, string partner, bool pimsReturnsValidatePIFailed)
        {
            // Arrange
            PXSettings.PayerAuthService.ResponseProvider.SessionId = Guid.NewGuid().ToString();
            PXSettings.PimsService.PostProcess = async (pimsValidateResponse) =>
            {
                if (pimsReturnsValidatePIFailed && pimsValidateResponse?.RequestMessage != null && pimsValidateResponse.RequestMessage.RequestUri.AbsolutePath.Contains("validate"))
                {
                    if (pimsValidateResponse.Content != null)
                    {
                        string requestContent = await pimsValidateResponse.Content.ReadAsStringAsync();
                        requestContent = requestContent.Replace("Success", "Failed");
                        pimsValidateResponse.Content = new StringContent(requestContent, Encoding.UTF8, "application/json");
                    }
                }
            };

            PXFlightHandler.AddToEnabledFlights("PXUsePaymentSessionsHandlerV2");

            // Arrange
            PXSettings.PayerAuthService.ResponseProvider.SessionId = Guid.NewGuid().ToString();

            if (string.Equals(partner, PXService.GlobalConstants.Partners.OfficeSMB, StringComparison.OrdinalIgnoreCase))
            {
                string expectedPSSResponse = "{\"handlePaymentChallenge\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\"}}";

                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
            }

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/Account013/PaymentSessionDescriptions?operation=Add&language=en-GB&paymentSessionData={0}",
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                            new PaymentSessionData()
                            {
                                Amount = amount,
                                Currency = "EUR",
                                Partner = partner,
                                Country = "gb",
                                HasPreOrder = hasPreOrder,
                                PaymentInstrumentId = "cw_apay_9f8b4e00-edf0-4b22-8feb-37c152e5875b",
                                Language = "en"
                            })))));

            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);

            var pidl = ReadPidlResourceFromJson(await pxResponse.Content.ReadAsStringAsync());
            Assert.AreEqual(ClientActionType.ReturnContext, pidl[0].ClientAction.ActionType);
            var paymentSession = JsonConvert.DeserializeObject<PaymentSession>(pidl[0].ClientAction.Context.ToString());

            pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/Account013/ChallengeDescriptions?operation=RenderPidlPage&language=en-GB&paymentSessionOrData={0}",
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(paymentSession)))));

            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);

            pidl = ReadPidlResourceFromJson(await pxResponse.Content.ReadAsStringAsync());

            if ((!pimsReturnsValidatePIFailed && (amount == 0 || hasPreOrder))
                || (amount != 0 && !hasPreOrder))
            {
                Assert.AreEqual(ClientActionType.ReturnContext, pidl[0].ClientAction.ActionType);

                paymentSession = JsonConvert.DeserializeObject<PaymentSession>(pidl[0].ClientAction.Context.ToString());
                Assert.IsTrue(paymentSession.IsChallengeRequired);
                Assert.AreEqual("ValidatePIOnAttachChallenge", paymentSession.ChallengeType);
                Assert.AreEqual(PaymentChallengeStatus.Succeeded, paymentSession.ChallengeStatus);
            }
            else
            {
                Assert.AreEqual(ClientActionType.Failure, pidl[0].ClientAction.ActionType);
                var context = JsonConvert.DeserializeObject<ServiceErrorResponse>(pidl[0].ClientAction.Context.ToString());
                Assert.AreEqual(PaymentChallengeStatus.Failed.ToString(), context.ErrorCode);
                Assert.AreEqual("ValidatePIOnAttachFailed", context.InnerError.ErrorCode);
                Assert.AreEqual(PaymentChallengeStatus.Failed.ToString(), context.InnerError.Message);
            }

            PXSettings.PimsService.ResetToDefaults();
        }

        /// <summary>
        /// PI in the paymentSessionData object requires a 3DS2 challenge, but PI authorization is skipped for commercialstores with a precheck by making a call to PI /extededView
        /// </summary>
        [TestMethod]
        [DataRow("commercialstores", true)]
        [DataRow("azure", false)]
        [DataRow("webblends", false)]
        public async Task PaymentSessionDescriptions_PSD2IgnorePIAuthorization(string partner, bool ignorePSD2IgnorePIAuthorization)
        {
            // Arrange
            PXSettings.PayerAuthService.ResponseProvider.SessionId = Guid.NewGuid().ToString();
            PXSettings.PimsService.ResponseProvider.RequiredChallenges = new List<string>() { "3ds2" };
            bool pimsGetPICalled = false;
            bool onlyPimsGetPIExtendedViewCalled = false;
            bool otherPimsApiCalled = false;
            PXSettings.PimsService.PreProcess = (request) =>
            {
                var trimmedSegments = request.RequestUri.Segments.Select(s => s.Trim(new char[] { '/' })).ToArray();
                if (trimmedSegments.Length > 4 && string.Equals(trimmedSegments[4], "extendedView", StringComparison.OrdinalIgnoreCase))
                {
                    Assert.IsTrue(request.RequestUri.Query.Contains("?partner"), "when extended view API is called, a partner query param should be present");
                    if (pimsGetPICalled == false && otherPimsApiCalled == false)
                    {
                        onlyPimsGetPIExtendedViewCalled = true;
                    }
                }
                else if (trimmedSegments.Length == 5 && string.Equals(trimmedSegments[4], "Account001-Pi006-Sepa", StringComparison.OrdinalIgnoreCase))
                {
                    pimsGetPICalled = true;
                }
                else
                {
                    otherPimsApiCalled = true;
                }
            };

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/Account001/PaymentSessionDescriptions?operation=Add&language=en-GB&paymentSessionData={0}",
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                            new PaymentSessionData()
                            {
                                Amount = 10.0m,
                                Currency = "EUR",
                                Partner = partner,
                                Country = "gb",
                                PaymentInstrumentId = "Account001-Pi006-Sepa",
                                Language = "en"
                            })))));

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);

            var pidl = ReadPidlResourceFromJson(await pxResponse.Content.ReadAsStringAsync());
            Assert.AreEqual(ClientActionType.ReturnContext, pidl[0].ClientAction.ActionType);

            var paymentSession = JsonConvert.DeserializeObject<PaymentSession>(pidl[0].ClientAction.Context.ToString());
            Assert.AreEqual(false, paymentSession.IsChallengeRequired);
            Assert.AreEqual(PaymentChallengeStatus.NotApplicable, paymentSession.ChallengeStatus);

            if (ignorePSD2IgnorePIAuthorization)
            {
                // GetExtendedPI does not verify ownership.  So for commercialpartner, Get Extended PI before calling GET PI on PIMS.
                // After calling Get Extended PI, Get PI is not called if the PI is not a credit card.
                Assert.IsTrue(onlyPimsGetPIExtendedViewCalled);
                Assert.IsFalse(pimsGetPICalled);
            }
            else
            {
                Assert.IsFalse(onlyPimsGetPIExtendedViewCalled);
                Assert.IsTrue(pimsGetPICalled);
            }

            // Since the PI is a SEPA, no other PIMS APIs should get called
            Assert.IsFalse(otherPimsApiCalled);
            PXSettings.PimsService.ResetToDefaults();
        }

        [DataRow(Partners.OfficeSMB, "inline", true)]
        [DataRow(Partners.Azure, "fullPage", true)]
        [DataRow(Partners.Webblends, "fullPage", false)]
        [DataRow("testHPC_consumer", "fullPage", false)]
        [DataRow("testHPC_commercial", "fullPage", true)]
        [TestMethod]
        public async Task PaymentSessions_AuthenticateIndiaThreeDS_PSS(string partner, string redirectionPattern, bool useCommercialFlow)
        {
            // Arrange
            string accountId = "Account001";
            string paymentSessionId = "ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6";
            string action = useCommercialFlow ? "AuthenticateIndiaThreeDS" : "BrowserAuthenticateThreeDSOne";

            string response = "{\"Id\":\"" + paymentSessionId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"Account001\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + paymentSessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"Account001-Pi001-Visa\\\",\\\"partner\\\":\\\"" + partner + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":false,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";
            PXSettings.SessionService.ResponseProvider.SessionStore.Add(paymentSessionId, response);

            PIDLData sampleData = new PIDLData();
            sampleData.Add("cvvToken", "123");

            string settingJson = useCommercialFlow ? "{\"handlepaymentchallenge\":{\"template\":\"defaultTemplate\",\"redirectionPattern\":\"" + redirectionPattern + "\",\"features\":{\"threeDSOne\":{\"applicableMarkets\":[\"in\"],\"displayCustomizationDetail\":[{\"enableIndia3dsForNonZeroPaymentTransaction\":true,}]}}}}" : "{\"handlepaymentchallenge\":{\"template\":\"defaultTemplate\",\"redirectionPattern\":\"" + redirectionPattern + "\",\"features\":{\"threeDSOne\":{\"applicableMarkets\":[\"in\"],\"displayCustomizationDetail\":[{\"india3dsEnableForBilldesk\":true,\"pxEnableIndia3DS1Challenge\":true,}]}}}}";

            PXSettings.PartnerSettingsService.ArrangeResponse(settingJson);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}?partner={3}", accountId, paymentSessionId, action, partner)));
            request.Headers.Add("x-ms-flight", "PXUsePartnerSettingsService,PXDisablePSSCache");
            if (!useCommercialFlow)
            {
                request.Headers.Add("x-ms-test", "{\"scenarios\":\"px-service-3ds1-test-emulator\",}");
            }

            request.Content = new StringContent(JsonConvert.SerializeObject(sampleData), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType);

            var pxResponse = await PXClient.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);

            var responseBody = JsonConvert.DeserializeObject<PIDLResource>(await pxResponse.Content.ReadAsStringAsync());

            if (redirectionPattern.Equals("fullPage"))
            {
                Assert.AreEqual(ClientActionType.Pidl, responseBody.ClientAction.ActionType);

                var pidl = ReadPidlResourceFromJson(JsonConvert.SerializeObject(responseBody.ClientAction.Context)).First();
                Assert.AreEqual("cc3dsredirectandstatuscheckpidl", pidl.Identity["type"]);

                ButtonDisplayHint cc3DSYesButtonHintId = pidl.GetDisplayHintById("cc3DSYesButton") as ButtonDisplayHint;
                Assert.IsNotNull(cc3DSYesButtonHintId);

                if (useCommercialFlow)
                {
                    Assert.IsTrue(cc3DSYesButtonHintId.Action.Context.ToString().Contains($"partner={partner}"));
                }
                else
                {
                    Assert.IsFalse(cc3DSYesButtonHintId.Action.Context.ToString().Contains($"partner"));
                }
            }
            else if (redirectionPattern.Equals("inline"))
            {
                Assert.AreEqual(ClientActionType.Redirect, responseBody.ClientAction.ActionType);
            }
        }

        [DataRow(PaymentChallengeStatus.NotApplicable)]
        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ReturnsVerifiedTrue_ForNonCreditCardPI(PaymentChallengeStatus challengeStatus)
        {
            // Arrange
            var accountId = "SepaPicvAccount";
            var piId = "SepaPicvAccount-Pi001-AddSepaPI";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));
            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual(true, pxContent.Verified, "Verified");
            Assert.AreEqual(piId, pxContent.PiId, "PiId");
            Assert.AreEqual(sessionId, pxContent.SessionId, "SessionId");
            Assert.AreEqual(challengeStatus, pxContent.ChallengeStatus, "ChallengeStatus");
        }

        [DataRow(PaymentChallengeStatus.NotApplicable, true)]
        [DataRow(PaymentChallengeStatus.NotApplicable, false)]
        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ReturnsVerifiedTrue_IsChallengeRequiredFalse(PaymentChallengeStatus challengeStatus, bool enablePiSessionFlight)
        {
            // Arrange
            var accountId = "Account001";
            var piId = "Pi001-Visa-3DS2";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";
            var response = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"" + accountId + "\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + sessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"" + piId + "\\\",\\\"partner\\\":\\\"Azure\\\",\\\"ChallengeStatus\\\":\\\"" + challengeStatus + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":false,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";
            var piSession = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"sessionId\\\":\\\"ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012\\\",\\\"accountId\\\":\\\"Account001\\\",\\\"requiredChallenge\\\":[\\\"3ds2\\\"]}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(sessionId, response);

            if (enablePiSessionFlight)
            {
                PXSettings.SessionService.ResponseProvider.SessionStore.Add("PX-3DS2-" + piId, piSession);
                PXFlightHandler.AddToEnabledFlights("PXEnablePSD2PaymentInstrumentSession");
            }

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));
            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual(true, pxContent.Verified, "Verified");
            Assert.AreEqual(piId, pxContent.PiId, "PiId");
            Assert.AreEqual(sessionId, pxContent.SessionId, "SessionId");
            Assert.AreEqual(challengeStatus, pxContent.ChallengeStatus, "ChallengeStatus");
        }

        [DataRow(PaymentChallengeStatus.ByPassed, true)]
        [DataRow(PaymentChallengeStatus.NotApplicable, true)]
        [DataRow(PaymentChallengeStatus.Succeeded, true)]
        [DataRow(PaymentChallengeStatus.ByPassed, false)]
        [DataRow(PaymentChallengeStatus.NotApplicable, false)]
        [DataRow(PaymentChallengeStatus.Succeeded, false)]
        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ReturnsVerifiedTrue_ForGoodChallengeStatuses_IsChallengeRequiredTrue(PaymentChallengeStatus challengeStatus, bool enablePiSessionFlight)
        {
            // Arrange
            var accountId = "Account001";
            var piId = "Pi001-Visa-3DS2";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";
            var response = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"" + accountId + "\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + sessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"" + piId + "\\\",\\\"partner\\\":\\\"Azure\\\",\\\"ChallengeStatus\\\":\\\"" + challengeStatus + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";
            var piSession = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"sessionId\\\":\\\"ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012\\\",\\\"accountId\\\":\\\"Account001\\\",\\\"requiredChallenge\\\":[\\\"3ds2\\\"]}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(sessionId, response);

            if (enablePiSessionFlight)
            {
                PXSettings.SessionService.ResponseProvider.SessionStore.Add("PX-3DS2-" + piId, piSession);
                PXFlightHandler.AddToEnabledFlights("PXEnablePSD2PaymentInstrumentSession");
            }

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));
            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual(true, pxContent.Verified, "Verified");
            Assert.AreEqual(piId, pxContent.PiId, "PiId");
            Assert.AreEqual(sessionId, pxContent.SessionId, "SessionId");
            Assert.AreEqual(challengeStatus, pxContent.ChallengeStatus, "ChallengeStatus");
        }

        [DataRow(PaymentChallengeStatus.ByPassed, true)]
        [DataRow(PaymentChallengeStatus.NotApplicable, true)]
        [DataRow(PaymentChallengeStatus.Succeeded, true)]
        [DataRow(PaymentChallengeStatus.ByPassed, false)]
        [DataRow(PaymentChallengeStatus.NotApplicable, false)]
        [DataRow(PaymentChallengeStatus.Succeeded, false)]
        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ReturnsVerifiedTrue_ChallengeStatuseNotApplicable_ThroughPaymentInstrumentSession(PaymentChallengeStatus challengeStatus, bool enablePiSessionFlight)
        {
            // Arrange
            var accountId = "Account001";
            var piId = "Pi001-Visa-3DS2";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";
            var response = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"" + accountId + "\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + sessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"" + piId + "\\\",\\\"partner\\\":\\\"Azure\\\",\\\"ChallengeStatus\\\":\\\"" + challengeStatus + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";
            var piSession = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"sessionId\\\":\\\"ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012\\\",\\\"accountId\\\":\\\"Account001\\\",\\\"requiredChallenge\\\":[\\\"\\\"]}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(sessionId, response);

            if (enablePiSessionFlight)
            {
                PXSettings.SessionService.ResponseProvider.SessionStore.Add("PX-3DS2-" + piId, piSession);
                PXFlightHandler.AddToEnabledFlights("PXEnablePSD2PaymentInstrumentSession");
            }

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));
            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual(true, pxContent.Verified, "Verified");
            Assert.AreEqual(piId, pxContent.PiId, "PiId");
            Assert.AreEqual(sessionId, pxContent.SessionId, "SessionId");
            if (enablePiSessionFlight)
            {
                Assert.AreEqual(PaymentChallengeStatus.NotApplicable, pxContent.ChallengeStatus, "ChallengeStatus");
            }
            else
            {
                Assert.AreEqual(challengeStatus, pxContent.ChallengeStatus, "ChallengeStatus");
            }
        }

        [DataRow(PaymentChallengeStatus.Cancelled)]
        [DataRow(PaymentChallengeStatus.Failed)]
        [DataRow(PaymentChallengeStatus.InternalServerError)]
        [DataRow(PaymentChallengeStatus.TimedOut)]
        [DataRow(PaymentChallengeStatus.Unknown)]
        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ReturnsVerifiedFalse_ForBadChallengeStatuses_IsChallengeRequiredTrue(PaymentChallengeStatus challengeStatus)
        {
            // Arrange
            var accountId = "Account001";
            var piId = "Pi001-Visa-3DS2";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";
            var response = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"" + accountId + "\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + sessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"" + piId + "\\\",\\\"partner\\\":\\\"Azure\\\",\\\"ChallengeStatus\\\":\\\"" + challengeStatus + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(sessionId, response);

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));
            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual(false, pxContent.Verified, "Verified");
            Assert.AreEqual(piId, pxContent.PiId, "PiId");
            Assert.AreEqual(sessionId, pxContent.SessionId, "SessionId");
            Assert.AreEqual(challengeStatus, pxContent.ChallengeStatus, "ChallengeStatus");
        }

        [DataRow(PaymentChallengeStatus.Succeeded, false)]
        [DataRow(PaymentChallengeStatus.Succeeded, true)]
        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ReturnsVerifiedTrue_ForAdditionalParams_IsChallengeRequiredTrue(PaymentChallengeStatus challengeStatus, bool missingParam)
        {
            // Arrange
            var accountId = "Account001";
            var piId = "Pi001-Visa-3DS2";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";
            var response = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"" + accountId + "\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + sessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"" + piId + "\\\",\\\"partner\\\":\\\"Azure\\\",\\\"ChallengeStatus\\\":\\\"" + challengeStatus + "\\\",\\\"amount\\\":120.0,\\\"currency\\\":\\\"EUR\\\",\\\"country\\\":\\\"de\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"purchase_order_id\\\":\\\"123456\\\",\\\"is_challenge_required\\\":true}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(sessionId, response);
            PXFlightHandler.AddToEnabledFlights("ValidatePaymentSessionProperties");
            PXFlightHandler.AddToEnabledFlights("ValidatePaymentSessionPropertiesLogging");
            
            // Act
            var paymentContext = new object(); 

            if (missingParam)
            {
                paymentContext = new 
                {
                    currency = "EUR",
                    country = "de",
                    partner = "Azure",
                    hasPreOrder = false,
                    isMOTO = false,
                    challengeScenario = "PaymentTransaction",
                };
            }
            else
            {
                paymentContext = new 
                {
                    currency = "EUR",
                    country = "de",
                    partner = "Azure",
                    hasPreOrder = false,
                    isMOTO = false,
                    challengeScenario = "PaymentTransaction",
                    purchaseOrderId = "123456",
                    pretax = 100,
                    postTax = 150
                };
            }

            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus?paymentContext={3}",
                        accountId,
                        sessionId,
                        piId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(paymentContext)))));

            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            if (missingParam)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, pxResponse.StatusCode, "StatusCode");
            }
            else
            {
                Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
                Assert.AreEqual(true, pxContent.Verified, "Verified");
                Assert.AreEqual(piId, pxContent.PiId, "PiId");
                Assert.AreEqual(sessionId, pxContent.SessionId, "SessionId");
                Assert.AreEqual(challengeStatus, pxContent.ChallengeStatus, "ChallengeStatus");
            }
        }

        [DataRow(PaymentChallengeStatus.Cancelled)]
        [DataRow(PaymentChallengeStatus.Failed)]
        [DataRow(PaymentChallengeStatus.InternalServerError)]
        [DataRow(PaymentChallengeStatus.TimedOut)]
        [DataRow(PaymentChallengeStatus.Unknown)]
        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ReturnsVerifiedTrue_AndChallengeStatusNotApplicable_ForBadChallengesButWithOverrideFlight(PaymentChallengeStatus challengeStatus)
        {
            PXFlightHandler.AddToEnabledFlights("PXAuthenticateStatusOverrideVerification");

            // Arrange
            var accountId = "Account001";
            var piId = "Pi001-Visa-3DS2";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";
            var response = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"" + accountId + "\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + sessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"" + piId + "\\\",\\\"partner\\\":\\\"Azure\\\",\\\"ChallengeStatus\\\":\\\"" + challengeStatus + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":false,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(sessionId, response);

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));
            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual(true, pxContent.Verified, "Verified");
            Assert.AreEqual(piId, pxContent.PiId, "PiId");
            Assert.AreEqual(sessionId, pxContent.SessionId, "SessionId");
            Assert.AreEqual(PaymentChallengeStatus.NotApplicable, pxContent.ChallengeStatus, "ChallengeStatus");
        }

        [DataRow(PaymentChallengeStatus.Failed)]
        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ReturnsVerifiedTrue_AndChallengeStatusNotApplicable_ForBadChallenges_WithXboxOverrideFlight_CorrelationContextIncorrect(PaymentChallengeStatus challengeStatus)
        {
            PXFlightHandler.AddToEnabledFlights("PXAuthenticateStatusOverrideVerificationForXbox");

            // Arrange
            var accountId = "Account001";
            var piId = "Pi001-Visa-3DS2";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";
            var response = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"" + accountId + "\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + sessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"" + piId + "\\\",\\\"partner\\\":\\\"Azure\\\",\\\"ChallengeStatus\\\":\\\"" + challengeStatus + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(sessionId, response);

            // Act
            PXClient.DefaultRequestHeaders.Add("User-Agent", "PaymentInstrumentFD");
            PXClient.DefaultRequestHeaders.Add("api-version", "v7.0");
            PXClient.DefaultRequestHeaders.Add("Correlation-Context", "v=1,ms.b.tel.scenario=commerce.purchase.order.1,ms.b..partner=xbox,ms.c.tel.operationType=OrderV7");
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));
            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual(false, pxContent.Verified, "Verified");
            Assert.AreEqual(piId, pxContent.PiId, "PiId");
            Assert.AreEqual(sessionId, pxContent.SessionId, "SessionId");
            Assert.AreEqual(PaymentChallengeStatus.Failed, pxContent.ChallengeStatus, "ChallengeStatus");

            // Clear the header
            PXClient.DefaultRequestHeaders.Remove("Correlation-Context");
        }

        [DataRow(PaymentChallengeStatus.Failed)]
        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ReturnsVerifiedTrue_AndChallengeStatusNotApplicable_ForBadChallenges_WithXboxOverrideFlight_NoCorrelationContextInHeader(PaymentChallengeStatus challengeStatus)
        {
            PXFlightHandler.AddToEnabledFlights("PXAuthenticateStatusOverrideVerificationForXbox");

            // Arrange
            var accountId = "Account001";
            var piId = "Pi001-Visa-3DS2";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";
            var response = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"" + accountId + "\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + sessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"" + piId + "\\\",\\\"partner\\\":\\\"Azure\\\",\\\"ChallengeStatus\\\":\\\"" + challengeStatus + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(sessionId, response);

            // Act
            PXClient.DefaultRequestHeaders.Add("User-Agent", "PaymentInstrumentFD");
            PXClient.DefaultRequestHeaders.Add("api-version", "v7.0");
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));
            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual(false, pxContent.Verified, "Verified");
            Assert.AreEqual(piId, pxContent.PiId, "PiId");
            Assert.AreEqual(sessionId, pxContent.SessionId, "SessionId");
            Assert.AreEqual(PaymentChallengeStatus.Failed, pxContent.ChallengeStatus, "ChallengeStatus");
        }

        [DataRow(PaymentChallengeStatus.Failed)]
        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ReturnsVerifiedTrue_AndChallengeStatusNotApplicable_ForBadChallengesButPXAuthenticateStatusOverrideVerificationForXbox(PaymentChallengeStatus challengeStatus)
        {
            PXFlightHandler.AddToEnabledFlights("PXAuthenticateStatusOverrideVerificationForXbox");

            // Arrange
            var accountId = "Account001";
            var piId = "Pi001-Visa-3DS2";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";
            var response = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"" + accountId + "\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + sessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"" + piId + "\\\",\\\"partner\\\":\\\"Azure\\\",\\\"ChallengeStatus\\\":\\\"" + challengeStatus + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(sessionId, response);

            // Act
            PXClient.DefaultRequestHeaders.Add("Correlation-Context", "v=1,ms.b.tel.scenario=commerce.purchase.order.1,ms.b.tel.partner=xbox,ms.c.tel.operationType=OrderV7");
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));
            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual(true, pxContent.Verified, "Verified");
            Assert.AreEqual(piId, pxContent.PiId, "PiId");
            Assert.AreEqual(sessionId, pxContent.SessionId, "SessionId");
            Assert.AreEqual(PaymentChallengeStatus.NotApplicable, pxContent.ChallengeStatus, "ChallengeStatus");

            // Clear the header
            PXClient.DefaultRequestHeaders.Remove("Correlation-Context");
        }

        [DataRow(PaymentChallengeStatus.Failed)]
        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ReturnsVerifiedTrue_AndChallengeStatusNotApplicable_ForBadChallenges_NoOverrideForXboxFlight(PaymentChallengeStatus challengeStatus)
        {
            // Arrange
            var accountId = "Account001";
            var piId = "Pi001-Visa-3DS2";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";
            var response = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"" + accountId + "\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + sessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"" + piId + "\\\",\\\"partner\\\":\\\"Azure\\\",\\\"ChallengeStatus\\\":\\\"" + challengeStatus + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(sessionId, response);

            // Act
            PXClient.DefaultRequestHeaders.Add("Correlation-Context", "v=1,ms.b.tel.scenario=commerce.purchase.order.1,ms.b.tel.partner=xbox,ms.c.tel.operationType=OrderV7");
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));
            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual(false, pxContent.Verified, "Verified");
            Assert.AreEqual(piId, pxContent.PiId, "PiId");
            Assert.AreEqual(sessionId, pxContent.SessionId, "SessionId");
            Assert.AreEqual(PaymentChallengeStatus.Failed, pxContent.ChallengeStatus, "ChallengeStatus");

            // Clear the header
            PXClient.DefaultRequestHeaders.Remove("Correlation-Context");
        }

        [DataRow(PaymentChallengeStatus.Failed)]
        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ReturnsVerifiedTrue_AndChallengeStatusNotApplicable_ForBadChallenges_WithOverrideForXboxFlight_ForNonXboxPartner(PaymentChallengeStatus challengeStatus)
        {
            PXFlightHandler.AddToEnabledFlights("PXAuthenticateStatusOverrideVerificationForXbox");

            // Arrange
            var accountId = "Account001";
            var piId = "Pi001-Visa-3DS2";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";
            var response = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"" + accountId + "\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + sessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"" + piId + "\\\",\\\"partner\\\":\\\"Azure\\\",\\\"ChallengeStatus\\\":\\\"" + challengeStatus + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(sessionId, response);

            // Act
            PXClient.DefaultRequestHeaders.Add("Correlation-Context", "v=1,ms.b.tel.scenario=commerce.purchase.order.1,ms.b.tel.partner=MdollarPurchase,ms.c.tel.operationType=OrderV7");
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));
            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual(false, pxContent.Verified, "Verified");
            Assert.AreEqual(piId, pxContent.PiId, "PiId");
            Assert.AreEqual(sessionId, pxContent.SessionId, "SessionId");
            Assert.AreEqual(PaymentChallengeStatus.Failed, pxContent.ChallengeStatus, "ChallengeStatus");

            // Clear the header
            PXClient.DefaultRequestHeaders.Remove("Correlation-Context");
        }

        [DataRow(PaymentChallengeStatus.NotApplicable)]
        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ReturnsVerifiedTrue_AndChallengeStatusNotApplicable_IsChallengeRequiredFalse_WithOverrideFlight(PaymentChallengeStatus challengeStatus)
        {
            PXFlightHandler.AddToEnabledFlights("PXAuthenticateStatusOverrideVerification");

            // Arrange
            var accountId = "Account001";
            var piId = "Pi001-Visa-3DS2";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";
            var response = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"" + accountId + "\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + sessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"" + piId + "\\\",\\\"partner\\\":\\\"Azure\\\",\\\"ChallengeStatus\\\":\\\"" + challengeStatus + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":false,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(sessionId, response);

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));
            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual(true, pxContent.Verified, "Verified");
            Assert.AreEqual(piId, pxContent.PiId, "PiId");
            Assert.AreEqual(sessionId, pxContent.SessionId, "SessionId");
            Assert.AreEqual(challengeStatus, pxContent.ChallengeStatus, "ChallengeStatus");
        }

        [DataRow(PaymentChallengeStatus.ByPassed)]
        [DataRow(PaymentChallengeStatus.NotApplicable)]
        [DataRow(PaymentChallengeStatus.Succeeded)]
        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ReturnsVerifiedTrue_AndTheSessionChallengeStatus_IsChallengeRequiredTrue_ForGoodChallengesButWithOverrideFlight(PaymentChallengeStatus challengeStatus)
        {
            PXFlightHandler.AddToEnabledFlights("PXAuthenticateStatusOverrideVerification");

            // Arrange
            var accountId = "Account001";
            var piId = "Pi001-Visa-3DS2";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";
            var response = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"" + accountId + "\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + sessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"" + piId + "\\\",\\\"partner\\\":\\\"Azure\\\",\\\"ChallengeStatus\\\":\\\"" + challengeStatus + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(sessionId, response);

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));
            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual(true, pxContent.Verified, "Verified");
            Assert.AreEqual(piId, pxContent.PiId, "PiId");
            Assert.AreEqual(sessionId, pxContent.SessionId, "SessionId");
            Assert.AreEqual(challengeStatus, pxContent.ChallengeStatus, "ChallengeStatus");
        }

        [DataRow(PaymentChallengeStatus.Cancelled)]
        [DataRow(PaymentChallengeStatus.Failed)]
        [DataRow(PaymentChallengeStatus.InternalServerError)]
        [DataRow(PaymentChallengeStatus.TimedOut)]
        [DataRow(PaymentChallengeStatus.Unknown)]
        [DataRow(PaymentChallengeStatus.ByPassed)]
        [DataRow(PaymentChallengeStatus.NotApplicable)]
        [DataRow(PaymentChallengeStatus.Succeeded)]
        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ReturnsVerifiedTrue_WhenForcedFlightIsEnabled(PaymentChallengeStatus challengeStatus)
        {
            PXFlightHandler.AddToEnabledFlights("PXAuthenticateStatusForceVerifiedTrue");

            // Arrange
            var accountId = "Account001";
            var piId = "Pi001-Visa-3DS2";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";
            var response = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"" + accountId + "\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + sessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"" + piId + "\\\",\\\"partner\\\":\\\"Azure\\\",\\\"ChallengeStatus\\\":\\\"" + challengeStatus + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":false,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(sessionId, response);

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));
            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual(true, pxContent.Verified, "Verified");
            Assert.AreEqual(piId, pxContent.PiId, "PiId");
            Assert.AreEqual(sessionId, pxContent.SessionId, "SessionId");
            Assert.AreEqual(PaymentChallengeStatus.NotApplicable, pxContent.ChallengeStatus, "ChallengeStatus");
        }

        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ReturnsVerifiedTrue_WhenRequiredChallengeIsCvv()
        {
            // Arrange
            var accountId = "Account001";
            var piId = "Pi001-Visa-CVV";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));
            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual(true, pxContent.Verified, "Verified");
            Assert.AreEqual(piId, pxContent.PiId, "PiId");
            Assert.AreEqual(sessionId, pxContent.SessionId, "SessionId");
            Assert.AreEqual(PaymentChallengeStatus.NotApplicable, pxContent.ChallengeStatus, "ChallengeStatus");
        }

        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ReturnsVerifiedTrue_WhenNoChallengeIsRequired()
        {
            // Arrange
            var accountId = "Account001";
            var piId = "Account001-Pi001-Visa";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));
            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual(true, pxContent.Verified, "Verified");
            Assert.AreEqual(piId, pxContent.PiId, "PiId");
            Assert.AreEqual(sessionId, pxContent.SessionId, "SessionId");
            Assert.AreEqual(PaymentChallengeStatus.NotApplicable, pxContent.ChallengeStatus, "ChallengeStatus");
        }

        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ReturnsNotFound_WhenSessionIsNotFound()
        {
            // Arrange
            var accountId = "Account001";
            var piId = "Pi001-Visa-3DS2";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, pxResponse.StatusCode, "StatusCode");
        }

        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ReturnsVerifiedTrue_WhenOverrideFlightIsEnabled()
        {
            PXFlightHandler.AddToEnabledFlights("PXAuthenticateStatusOverrideVerification");

            // Arrange
            var accountId = "Account001";
            var piId = "Pi001-Visa-3DS2";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));
            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual(true, pxContent.Verified, "Verified");
            Assert.AreEqual(piId, pxContent.PiId, "PiId");
            Assert.AreEqual(sessionId, pxContent.SessionId, "SessionId");
            Assert.AreEqual(PaymentChallengeStatus.NotApplicable, pxContent.ChallengeStatus, "ChallengeStatus");
        }

        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ThrowsNotFound_WhenSessionAndPIDoNotExist()
        {
            // Arrange
            var accountId = "Account001";
            var piId = "Invalid-ID";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";
            var errorPayload = "{\"code\":\"NotFound\",\"data\":[],\"details\":[],\"innererror\":{\"code\":\"PaymentInstrumentNotFound\",\"data\":[],\"details\":[],\"message\":\"Payment instrument was not found\",\"target\":\"PXService\"},\"message\":\"The requested resource could not be found.\",\"source\":\"PaymentInstrumentFD\"}";
            PXSettings.PimsService.ArrangeResponse(errorPayload, HttpStatusCode.NotFound, null, ".*/extendedView.*");

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, pxResponse.StatusCode, "StatusCode");
            Assert.IsNull(pxResponse.Content, "Content");
        }

        [DataRow("Pi001-Visa-3DS2", "Account002")]
        [DataRow("cw_gpay_cc80ac8e-3e33-40f3-9fed-6efb5be47762", "Account013")]
        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ThrowsNotFound_WhenPIDoNotExist(string piId, string accountId)
        {
            // Arrange
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";

            var extendedViewResponse = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedViewResponse), HttpStatusCode.OK, null, ".*/extendedView.*");

            var errorPayload = "{\"code\":\"NotFound\",\"data\":[],\"details\":[],\"innererror\":{\"code\":\"AccountPINotFound\",\"data\":[],\"details\":[],\"message\":\"Payment instrument was not found\",\"target\":\"PXService\"},\"message\":\"The requested resource could not be found.\",\"source\":\"PaymentInstrumentFD\"}";
            PXSettings.PimsService.ArrangeResponse(errorPayload, HttpStatusCode.NotFound, null, ".*");

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", "Account001", sessionId, piId)));

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, pxResponse.StatusCode, "StatusCode");
            Assert.IsNull(pxResponse.Content, "Content");
        }

        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ThrowsException_WhenServiceFails()
        {
            // Arrange
            var accountId = "Account001";
            var piId = "Invalid-ID";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";
            var errorPayload = "{\"code\":\"InternalServerError\",\"data\":[],\"details\":[],\"message\":\"An internal error ocurred for this test.\",\"source\":\"PaymentInstrumentFD\"}";
            PXSettings.PimsService.ArrangeResponse(errorPayload, HttpStatusCode.InternalServerError, null, ".*/extendedView.*");

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));
            var pxContent = JsonConvert.DeserializeObject<ServiceErrorResponse>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.InternalServerError, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual("An internal error ocurred for this test.", pxContent.Message, "Message");
        }

        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ReturnsVerifiedTrue_WhenExtendendViewAccountIsDifferent_IsChallengeRequiredTrue()
        {
            // Arrange
            var accountId = "Account001";
            var piId = "Pi001-Visa-3DS2";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";
            var challengeStatus = PaymentChallengeStatus.Succeeded;

            var extendedViewResponse = PimsMockResponseProvider.GetPaymentInstrument("Account002", piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedViewResponse), HttpStatusCode.OK, null, ".*/extendedView.*");

            var pimsResponse = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(pimsResponse), HttpStatusCode.OK, null, ".*");

            var sessionResponse = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"" + accountId + "\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + sessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"" + piId + "\\\",\\\"partner\\\":\\\"Azure\\\",\\\"ChallengeStatus\\\":\\\"" + challengeStatus + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";
            PXSettings.SessionService.ResponseProvider.SessionStore.Add(sessionId, sessionResponse);

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));
            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual(true, pxContent.Verified, "Verified");
            Assert.AreEqual(piId, pxContent.PiId, "PiId");
            Assert.AreEqual(sessionId, pxContent.SessionId, "SessionId");
            Assert.AreEqual(challengeStatus, pxContent.ChallengeStatus, "ChallengeStatus");
        }

        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ReturnsVerifiedTrue_WhenExtendendViewAccountIsDifferent_IsChallengeRequiredFalse()
        {
            // Arrange
            var accountId = "Account001";
            var piId = "Pi001-Visa-3DS2";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";
            var challengeStatus = PaymentChallengeStatus.NotApplicable;

            var extendedViewResponse = PimsMockResponseProvider.GetPaymentInstrument("Account002", piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedViewResponse), HttpStatusCode.OK, null, ".*/extendedView.*");

            var pimsResponse = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(pimsResponse), HttpStatusCode.OK, null, ".*");

            var sessionResponse = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"" + accountId + "\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + sessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"" + piId + "\\\",\\\"partner\\\":\\\"Azure\\\",\\\"ChallengeStatus\\\":\\\"" + challengeStatus + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":false,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";
            PXSettings.SessionService.ResponseProvider.SessionStore.Add(sessionId, sessionResponse);

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));
            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual(true, pxContent.Verified, "Verified");
            Assert.AreEqual(piId, pxContent.PiId, "PiId");
            Assert.AreEqual(sessionId, pxContent.SessionId, "SessionId");
            Assert.AreEqual(challengeStatus, pxContent.ChallengeStatus, "ChallengeStatus");
        }

        [DataRow(PXPayerAuthServiceModel.ChallengeCancelIndicator.TransactionError)]
        [DataRow(PXPayerAuthServiceModel.ChallengeCancelIndicator.Unknown)]
        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ReturnsVerifiedTrue_WhenCancelIndicatorIsGood_IsChallengeRequiredTrue(PXPayerAuthServiceModel.ChallengeCancelIndicator transactionChallengeCancelIndicator)
        {
            // Arrange
            var accountId = "Account001";
            var piId = "Pi001-Visa-3DS2";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";
            var challengeStatus = PaymentChallengeStatus.Unknown;
            var transactionChallengeStatus = PXPayerAuthServiceModel.TransactionStatus.N;
            var transactionChallengeStatusReason = PXPayerAuthServiceModel.TransactionStatusReason.TSR01;
            var sessionResponse = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"transaction_challenge_status\\\":\\\"" + transactionChallengeStatus.ToString() + "\\\",\\\"transaction_challenge_status_reason\\\":\\\"" + transactionChallengeStatusReason.ToString() + "\\\",\\\"transaction_challenge_cancel_indicator\\\":\\\"" + transactionChallengeCancelIndicator.ToString() + "\\\",\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"" + accountId + "\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + sessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"" + piId + "\\\",\\\"partner\\\":\\\"Azure\\\",\\\"ChallengeStatus\\\":\\\"" + challengeStatus + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(sessionId, sessionResponse);

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));
            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual(true, pxContent.Verified, "Verified");
            Assert.AreEqual(piId, pxContent.PiId, "PiId");
            Assert.AreEqual(sessionId, pxContent.SessionId, "SessionId");
            Assert.AreEqual(challengeStatus, pxContent.ChallengeStatus, "ChallengeStatus");
        }

        [DataRow(PXPayerAuthServiceModel.ChallengeCancelIndicator.TransactionError)]
        [DataRow(PXPayerAuthServiceModel.ChallengeCancelIndicator.Unknown)]
        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ReturnsVerifiedTrue_WhenCancelIndicatorIsGood_IsChallengeRequiredFalse(PXPayerAuthServiceModel.ChallengeCancelIndicator transactionChallengeCancelIndicator)
        {
            // Arrange
            var accountId = "Account001";
            var piId = "Pi001-Visa-3DS2";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";
            var challengeStatus = PaymentChallengeStatus.NotApplicable;
            var transactionChallengeStatus = PXPayerAuthServiceModel.TransactionStatus.N;
            var transactionChallengeStatusReason = PXPayerAuthServiceModel.TransactionStatusReason.TSR01;
            var sessionResponse = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"transaction_challenge_status\\\":\\\"" + transactionChallengeStatus.ToString() + "\\\",\\\"transaction_challenge_status_reason\\\":\\\"" + transactionChallengeStatusReason.ToString() + "\\\",\\\"transaction_challenge_cancel_indicator\\\":\\\"" + transactionChallengeCancelIndicator.ToString() + "\\\",\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"" + accountId + "\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + sessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"" + piId + "\\\",\\\"partner\\\":\\\"Azure\\\",\\\"ChallengeStatus\\\":\\\"" + challengeStatus + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":false,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(sessionId, sessionResponse);

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));
            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual(true, pxContent.Verified, "Verified");
            Assert.AreEqual(piId, pxContent.PiId, "PiId");
            Assert.AreEqual(sessionId, pxContent.SessionId, "SessionId");
            Assert.AreEqual(challengeStatus, pxContent.ChallengeStatus, "ChallengeStatus");
        }

        [DataRow(PXPayerAuthServiceModel.ChallengeCancelIndicator.CancelledByCardHolder)]
        [DataRow(PXPayerAuthServiceModel.ChallengeCancelIndicator.CancelledByRequestor)]
        [DataRow(PXPayerAuthServiceModel.ChallengeCancelIndicator.TransactionAbandoned)]
        [DataRow(PXPayerAuthServiceModel.ChallengeCancelIndicator.TransactionCReqTimedOut)]
        [DataRow(PXPayerAuthServiceModel.ChallengeCancelIndicator.TransactionTimedOut)]
        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ReturnsVerifiedFalse_WhenCancelIndicatorIsBad(PXPayerAuthServiceModel.ChallengeCancelIndicator transactionChallengeCancelIndicator)
        {
            // Arrange
            var accountId = "Account001";
            var piId = "Pi001-Visa-3DS2";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";
            var challengeStatus = PaymentChallengeStatus.Unknown;
            var transactionChallengeStatus = PXPayerAuthServiceModel.TransactionStatus.N;
            var transactionChallengeStatusReason = PXPayerAuthServiceModel.TransactionStatusReason.TSR01;
            var sessionResponse = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"transaction_challenge_status\\\":\\\"" + transactionChallengeStatus.ToString() + "\\\",\\\"transaction_challenge_status_reason\\\":\\\"" + transactionChallengeStatusReason.ToString() + "\\\",\\\"transaction_challenge_cancel_indicator\\\":\\\"" + transactionChallengeCancelIndicator.ToString() + "\\\",\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"" + accountId + "\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + sessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"" + piId + "\\\",\\\"partner\\\":\\\"Azure\\\",\\\"ChallengeStatus\\\":\\\"" + challengeStatus + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(sessionId, sessionResponse);

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));
            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual(false, pxContent.Verified, "Verified");
            Assert.AreEqual(piId, pxContent.PiId, "PiId");
            Assert.AreEqual(sessionId, pxContent.SessionId, "SessionId");
            Assert.AreEqual(challengeStatus, pxContent.ChallengeStatus, "ChallengeStatus");
        }

        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ReturnsVerifiedFalse_WhentransactionChallengeStatusReasonIsTSR10()
        {
            // Arrange
            var accountId = "Account001";
            var piId = "Pi001-Visa-3DS2";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";
            var challengeStatus = PaymentChallengeStatus.Unknown;
            var transactionChallengeStatus = PXPayerAuthServiceModel.TransactionStatus.N;
            var transactionChallengeCancelIndicator = PXPayerAuthServiceModel.ChallengeCancelIndicator.Unknown;
            var transactionChallengeStatusReason = PXPayerAuthServiceModel.TransactionStatusReason.TSR10;
            var sessionResponse = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"transaction_challenge_status\\\":\\\"" + transactionChallengeStatus.ToString() + "\\\",\\\"transaction_challenge_status_reason\\\":\\\"" + transactionChallengeStatusReason.ToString() + "\\\",\\\"transaction_challenge_cancel_indicator\\\":\\\"" + transactionChallengeCancelIndicator.ToString() + "\\\",\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"" + accountId + "\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + sessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"" + piId + "\\\",\\\"partner\\\":\\\"Azure\\\",\\\"ChallengeStatus\\\":\\\"" + challengeStatus + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(sessionId, sessionResponse);

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));
            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual(false, pxContent.Verified, "Verified");
            Assert.AreEqual(piId, pxContent.PiId, "PiId");
            Assert.AreEqual(sessionId, pxContent.SessionId, "SessionId");
            Assert.AreEqual(challengeStatus, pxContent.ChallengeStatus, "ChallengeStatus");
        }

        [DataRow(PXPayerAuthServiceModel.TransactionStatus.C)]
        [DataRow(PXPayerAuthServiceModel.TransactionStatus.FR)]
        [DataRow(PXPayerAuthServiceModel.TransactionStatus.R)]
        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ReturnsVerifiedFalse_WhenTransactionStatusIsBad(PXPayerAuthServiceModel.TransactionStatus transactionChallengeStatus)
        {
            // Arrange
            var accountId = "Account001";
            var piId = "Pi001-Visa-3DS2";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";
            var challengeStatus = PaymentChallengeStatus.Unknown;
            var transactionChallengeStatusReason = PXPayerAuthServiceModel.TransactionStatusReason.TSR01;
            var transactionChallengeCancelIndicator = PXPayerAuthServiceModel.ChallengeCancelIndicator.TransactionError;
            var sessionResponse = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"transaction_challenge_status\\\":\\\"" + transactionChallengeStatus.ToString() + "\\\",\\\"transaction_challenge_status_reason\\\":\\\"" + transactionChallengeStatusReason.ToString() + "\\\",\\\"transaction_challenge_cancel_indicator\\\":\\\"" + transactionChallengeCancelIndicator.ToString() + "\\\",\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"" + accountId + "\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + sessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"" + piId + "\\\",\\\"partner\\\":\\\"Azure\\\",\\\"ChallengeStatus\\\":\\\"" + challengeStatus + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(sessionId, sessionResponse);

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));
            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual(false, pxContent.Verified, "Verified");
            Assert.AreEqual(piId, pxContent.PiId, "PiId");
            Assert.AreEqual(sessionId, pxContent.SessionId, "SessionId");
            Assert.AreEqual(challengeStatus, pxContent.ChallengeStatus, "ChallengeStatus");
        }

        [DataRow(PXPayerAuthServiceModel.TransactionStatus.A)]
        [DataRow(PXPayerAuthServiceModel.TransactionStatus.U)]
        [DataRow(PXPayerAuthServiceModel.TransactionStatus.Y)]
        [TestMethod]
        public async Task PaymentSessions_AuthenticationStatus_ReturnsVerifiedTrue_WhenTransactionStatusIsGood(PXPayerAuthServiceModel.TransactionStatus transactionChallengeStatus)
        {
            // Arrange
            var accountId = "Account001";
            var piId = "Pi001-Visa-3DS2";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";
            var challengeStatus = PaymentChallengeStatus.Unknown;
            var transactionChallengeStatusReason = PXPayerAuthServiceModel.TransactionStatusReason.TSR01;
            var transactionChallengeCancelIndicator = PXPayerAuthServiceModel.ChallengeCancelIndicator.TransactionError;
            var sessionResponse = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"transaction_challenge_status\\\":\\\"" + transactionChallengeStatus.ToString() + "\\\",\\\"transaction_challenge_status_reason\\\":\\\"" + transactionChallengeStatusReason.ToString() + "\\\",\\\"transaction_challenge_cancel_indicator\\\":\\\"" + transactionChallengeCancelIndicator.ToString() + "\\\",\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"" + accountId + "\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + sessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"" + piId + "\\\",\\\"partner\\\":\\\"Azure\\\",\\\"ChallengeStatus\\\":\\\"" + challengeStatus + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(sessionId, sessionResponse);

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));
            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual(true, pxContent.Verified, "Verified");
            Assert.AreEqual(piId, pxContent.PiId, "PiId");
            Assert.AreEqual(sessionId, pxContent.SessionId, "SessionId");
            Assert.AreEqual(challengeStatus, pxContent.ChallengeStatus, "ChallengeStatus");
        }

        [DataRow(Partners.Cart, "Account002-Pi008-PayPay", "JP", "JPY")]
        [DataRow(Partners.Cart, "Account002-Pi008-AlipayHK", "hk", "HKD")]
        [DataRow(Partners.Cart, "Account002-Pi008-GCash", "ph", "PHP")]
        [DataRow(Partners.Cart, "Account002-Pi008-TrueMoney", "th", "THB")]
        [DataRow(Partners.Cart, "Account002-Pi008-TouchnGo", "my", "MYR")]
        [TestMethod]
        public async Task CreatePaymentSession_ANTPI(string partnerName, string piid, string country, string currency)
        {
            // Arrange
            string accountId = "Account002";

            var paymentSessionData = new PaymentSessionData()
            {
                PaymentInstrumentId = piid,
                Language = "en-us",
                Amount = 800,
                Partner = partnerName,
                Currency = currency,
                Country = country,
                ChallengeScenario = ChallengeScenario.PaymentTransaction
            };

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/{0}/paymentSessionDescriptions?paymentSessionData={1}",
                        accountId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(
                            paymentSessionData)))));

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode);

            var pidl = ReadPidlResourceFromJson(await pxResponse.Content.ReadAsStringAsync());
            var paymentSession = JsonConvert.DeserializeObject<PaymentSession>(pidl[0].ClientAction.Context.ToString());

            Assert.AreEqual(piid, paymentSession.PaymentInstrumentId);
            Assert.AreEqual(false, paymentSession.IsChallengeRequired);
            Assert.AreEqual(PaymentChallengeStatus.NotApplicable, paymentSession.ChallengeStatus);
        }

        [DataRow("{\"status\":\"Declined\"}", "1", null)] // 1 = Declined
        [DataRow("", "2", "Account001-Pi003-MC")] // 2 = Active
        [DataRow("{\"status\":\"Pending\"}", "3", null)] // 3 = Pending
        [TestMethod]
        public async Task PaymentSessions_AddCCQRCode_PollingStatus(string expectedResponse, string currentStatus, string piid)
        {
            string paymentSessionId = "ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6";
            var accountId = "Account001";
            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piid);
            string sessionResponse = null;

            if (string.IsNullOrEmpty(expectedResponse))
            {
                sessionResponse = "{\"Id\":\"" + paymentSessionId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":null,\\\"AccountId\\\":\\\"Account001\\\",\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"e99fd8de-8db4-4d7f-9ad0-1857b25503d4\\\",\\\"Partner\\\":\\\"xboxsettings\\\",\\\"Country\\\":\\\"US\\\",\\\"UseCount\\\":0,\\\"Operation\\\":\\\"Add\\\",\\\"Email\\\":null,\\\"FirstName\\\":null,\\\"LastName\\\":null,\\\"PaymentMethodType\\\":null,\\\"PaymentInstrumentId\\\":\\\"" + piid + "\\\",\\\"Status\\\":" + currentStatus + ",\\\"ChallengeStatus\\\":\\\"Unknown\\\",\\\"RiskData\\\":{\\\"ipAddress\\\":null,\\\"userInfo\\\":null,\\\"deviceId\\\":null,\\\"userAgent\\\":null,\\\"greenId\\\":null,\\\"deviceType\\\":null},\\\"signature\\\":\\\"placeholder_for_paymentsession_signature_e99fd8de-8db4-4d7f-9ad0-1857b25503d4\\\",\\\"QrCodeCreatedTime\\\":\\\"2024-06-25T23:05:38.208897Z\\\",\\\"FormRenderedTime\\\":\\\"2024-06-25T23:05:38.208897Z\\\"}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";
            }
            else
            {
                sessionResponse = "{\"Id\":\"" + paymentSessionId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":null,\\\"AccountId\\\":\\\"Account001\\\",\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"e99fd8de-8db4-4d7f-9ad0-1857b25503d4\\\",\\\"Partner\\\":\\\"xboxsettings\\\",\\\"Country\\\":\\\"US\\\",\\\"UseCount\\\":0,\\\"Operation\\\":\\\"Add\\\",\\\"Email\\\":null,\\\"FirstName\\\":null,\\\"LastName\\\":null,\\\"PaymentMethodType\\\":null,\\\"PaymentInstrumentId\\\":null,\\\"Status\\\":" + currentStatus + ",\\\"ChallengeStatus\\\":\\\"Unknown\\\",\\\"RiskData\\\":{\\\"ipAddress\\\":null,\\\"userInfo\\\":null,\\\"deviceId\\\":null,\\\"userAgent\\\":null,\\\"greenId\\\":null,\\\"deviceType\\\":null},\\\"signature\\\":\\\"placeholder_for_paymentsession_signature_e99fd8de-8db4-4d7f-9ad0-1857b25503d4\\\",\\\"QrCodeCreatedTime\\\":\\\"2024-06-25T23:05:38.208897Z\\\",\\\"FormRenderedTime\\\":\\\"2024-06-25T23:05:38.208897Z\\\"}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";
            }

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(paymentSessionId, sessionResponse);

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, GetPXServiceUrl(string.Format("/v7.0/{0}/secondScreenSessions/{1}/qrCodeStatus", accountId, paymentSessionId)));

            var pxResponse = await PXClient.SendAsync(requestMessage);

            if (currentStatus == "1")
            {
                Assert.AreEqual(pxResponse.StatusCode, HttpStatusCode.InternalServerError);
            }
            else
            {
                global::Tests.Common.Model.Pims.PaymentInstrument responsePi = JsonConvert.DeserializeObject<global::Tests.Common.Model.Pims.PaymentInstrument>(await pxResponse.Content.ReadAsStringAsync());

                Assert.AreEqual(pxResponse.StatusCode, HttpStatusCode.OK);
                var responseContent = await pxResponse.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(expectedResponse))
                {
                    Assert.AreEqual(JsonConvert.SerializeObject(expectedPI), JsonConvert.SerializeObject(responsePi));
                }
                else
                {
                    Assert.AreEqual(expectedResponse, responseContent);
                }
            }
        }

        [DataRow(true, true)]
        [DataRow(true, false)]
        [DataRow(false, true)]
        [DataRow(false, false)]
        [TestMethod]
        public async Task PaymentSessions_SessionLookupOrder(bool exposeFlight, bool validSessionId)
        {
            var accountId = "Account001";
            var piId = "Pi001-Visa-3DS2";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";
            PaymentChallengeStatus challengeStatus = PaymentChallengeStatus.Succeeded;
            var response = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"" + accountId + "\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + sessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"" + piId + "\\\",\\\"partner\\\":\\\"Azure\\\",\\\"ChallengeStatus\\\":\\\"" + challengeStatus + "\\\",\\\"amount\\\":800.0,\\\"currency\\\":\\\"INR\\\",\\\"country\\\":\\\"in\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"is_challenge_required\\\":true,\\\"purchaseOrderId\\\":null}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";
            var piSession = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"sessionId\\\":\\\"ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012\\\",\\\"accountId\\\":\\\"Account001\\\",\\\"requiredChallenge\\\":[\\\"3ds2\\\"]}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(sessionId, response);

            PXSettings.SessionService.ResponseProvider.SessionStore.Add("PX-3DS2-" + piId, piSession);
            PXFlightHandler.AddToEnabledFlights("PXEnablePSD2PaymentInstrumentSession");

            if (exposeFlight)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableGetSessionWithSessionId");
            }

            if (!validSessionId)
            {
                sessionId = "123-123-123-123";
            }

            // Act
            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(string.Format("/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus", accountId, sessionId, piId)));
            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual(true, pxContent.Verified, "Verified");
            Assert.AreEqual(piId, pxContent.PiId, "PiId");
            Assert.AreEqual(sessionId, pxContent.SessionId, "SessionId");
        }

        [DataRow(true, "azure", "Azure", "EUR", 120, true)] // Relax enforcement
        [DataRow(false, "azure", "Azure", "EUR", 120, true)] // Do not relax enforcement
        [DataRow(true, "webblends", "Azure", "EUR", 120, true)] // Mismatched partner name, don't relax enforcement for webblends
        [DataRow(true, "webblends", "", "EUR", 120, true)] // Relax enforcement, no partner name
        [DataRow(true, "webblends", "Azure", "USD", 120, false)] // Mismatched partner name, don't relax enforcement for webblends, return failed validation
        [DataRow(true, "webblends", "Azure", "EUR", 0, true)] // Mismatched partner name, don't relax enforcement for webblends, return failed validation
        [TestMethod]
        public async Task VerifyAdditionalSessionDataParams_RelaxEnforcement(bool includeFlight, string relaxedEnforcementPartner, string paymentSessionPartner, string paymentContextCurrency, int amount, bool verifiedResult)
        {
            // Arrange
            var accountId = "Account001";
            var piId = "Pi001-Visa-3DS2";
            var sessionId = "ZFFFFFFFFFFF12345678-1234-1234-1234-123456789012";
            var challengeStatus = PaymentChallengeStatus.Succeeded;
            var response = "{\"Id\":\"" + sessionId + "\",\"piCid\":\"" + accountId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":[],\\\"PayerAuthApiVersion\\\":\\\"2019-04-16\\\",\\\"AccountId\\\":\\\"" + accountId + "\\\",\\\"BrowserInfo\\\":null,\\\"MethodData\\\":null,\\\"AuthenticationResponse\\\":null,\\\"billableAccountId\\\":null,\\\"classicProduct\\\":null,\\\"EmailAddress\\\":null,\\\"Language\\\":\\\"en-us\\\",\\\"payment_session_id\\\":\\\"" + sessionId + "\\\",\\\"account_id\\\":\\\"" + accountId + "\\\",\\\"caid\\\":null,\\\"payment_instrument_id\\\":\\\"" + piId + "\\\",\\\"partner\\\":\\\"" + paymentSessionPartner + "\\\",\\\"ChallengeStatus\\\":\\\"" + challengeStatus + "\\\",\\\"amount\\\":" + amount + ",\\\"currency\\\":\\\"EUR\\\",\\\"country\\\":\\\"de\\\",\\\"has_pre_order\\\":false,\\\"is_legacy\\\":false,\\\"is_moto\\\":false,\\\"three_dsecure_scenario\\\":\\\"PaymentTransaction\\\",\\\"payment_method_family\\\":\\\"credit_card\\\",\\\"payment_method_type\\\":\\\"MasterCard\\\",\\\"device_channel\\\":\\\"browser\\\",\\\"purchase_order_id\\\":\\\"123456\\\",\\\"is_challenge_required\\\":true}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument(accountId, piId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            PXSettings.SessionService.ResponseProvider.SessionStore.Add(sessionId, response);
            PXFlightHandler.AddToEnabledFlights("ValidatePaymentSessionProperties");
            PXFlightHandler.AddToEnabledFlights("ValidatePaymentSessionPropertiesLogging");

            // Act
            var paymentContext = new object();

            paymentContext = new
            {
                currency = paymentContextCurrency,
                country = "de",
                partner = "Azure",
                hasPreOrder = false,
                isMOTO = false,
                challengeScenario = "PaymentTransaction",
                purchaseOrderId = "123456",
                pretax = 100,
                postTax = 150
            };

            if (includeFlight)
            {
                PXFlightHandler.AddToEnabledFlights("PXAdditionalVerificationRelaxEnforcement_" + relaxedEnforcementPartner);
            }

            if (amount == 0)
            {
                PXFlightHandler.AddToEnabledFlights("PXSkipAdditionalValidationForZeroAmount");
            }

            var pxResponse = await PXClient.GetAsync(
                GetPXServiceUrl(
                    string.Format(
                        "/v7.0/{0}/paymentSessions/{1}/{2}/AuthenticationStatus?paymentContext={3}",
                        accountId,
                        sessionId,
                        piId,
                        HttpUtility.UrlEncode(JsonConvert.SerializeObject(paymentContext)))));

            var pxContent = JsonConvert.DeserializeObject<AuthenticationStatus>(await pxResponse.Content.ReadAsStringAsync());

            Assert.AreEqual(HttpStatusCode.OK, pxResponse.StatusCode, "StatusCode");
            Assert.AreEqual(verifiedResult, pxContent.Verified, "Verified");

            if (verifiedResult == false)
            {
                Assert.AreEqual(VerificationResult.CurrencyMismatch, pxContent.FailureReason, "FailureReason");
            }
        }
    }
}