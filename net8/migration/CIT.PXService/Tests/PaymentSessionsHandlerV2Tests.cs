// <copyright file="PaymentSessionsHandlerV2Tests.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXService;
    using Microsoft.Commerce.Payments.PXService.Model.PayerAuthService;
    using Microsoft.Commerce.Payments.PXService.Model.ThreeDSExternalService;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model;
    using Microsoft.Commerce.Tracing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using AuthenticationRequest = Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model.AuthenticationRequest;
    using ChallengeScenario = Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model.ChallengeScenario;
    using PIMSModel = Microsoft.Commerce.Payments.PimsModel.V4;
    using Common = Microsoft.Commerce.Payments.Common.Transaction;
    using PaymentSessionData = Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model.PaymentSessionData;
    using System.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Newtonsoft.Json;
    using PaymentSession = Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model.PaymentSession;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXService.Accessors.TransactionDataService;
    using Microsoft.Commerce.Payments.Common.StoredValue;
    using System.Net;

    [TestClass]
    public class PaymentSessionsHandlerV2Tests : TestBase
    {
        private const string PifdBaseUrl = "https://pifdbaseurl";
        private const string AcountId = "accountId";
        private const string SessionId = "sessionId";
        private const string ChallengeRequiredPiId = "ChallengeRequiredPiId";
        private const string ChallengeNotRequiredPiId = "ChallengeNotRequiredPiId";
        private const string PXPSD2CompFlights = "PXPSD2Comp-_-_-_-Succeeded,PXPSD2Comp-_-_-01-Cancelled,PXPSD2Comp-N-_-04-TimedOut,PXPSD2Comp-N-TSR10-_-Failed,PXPSD2Comp-R-_-_-Failed,PXPSD2Comp-FR-_-_-Failed,PXPSD2Auth-_-_-Succeeded,PXPSD2Auth-C-_-Unknown,PXPSD2Auth-N-TSR10-Failed,PXPSD2Auth-R-_-Failed";
        private List<string> flightings = new List<string>();
        private bool defaultCreditCardChallenge = false;

        private Mock<IPayerAuthServiceAccessor> mockPayerAuthServiceAccessor;
        private Mock<IPIMSAccessor> mockPims;
        private Mock<ISessionServiceAccessor> mockSessionServiceAccessor;
        private Mock<ISessionServiceAccessor> mockQrCodeSessionServiceAccessor;
        private Mock<IAccountServiceAccessor> mockAccountServiceAccessor;
        private Mock<IPurchaseServiceAccessor> mockPurchaseServiceAccessor;
        private Mock<ITransactionServiceAccessor> mockTransactionServiceAccessor;
        private Mock<ITransactionDataServiceAccessor> mockTransactionDataServiceAccessor;

        [TestInitialize]
        public void TestInitialize()
        {
            mockPayerAuthServiceAccessor = new Mock<IPayerAuthServiceAccessor>();
            mockPims = new Mock<IPIMSAccessor>();
            mockSessionServiceAccessor = new Mock<ISessionServiceAccessor>();
            mockQrCodeSessionServiceAccessor = new Mock<ISessionServiceAccessor>();
            mockAccountServiceAccessor = new Mock<IAccountServiceAccessor>();
            mockPurchaseServiceAccessor = new Mock<IPurchaseServiceAccessor>();
            mockTransactionServiceAccessor = new Mock<ITransactionServiceAccessor>();
            mockTransactionDataServiceAccessor = new Mock<ITransactionDataServiceAccessor>();

            // sessionServiceAccessor.GetSessionResourceData
            var sessionData = new Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession
            {
                Id = SessionId
            };

            mockSessionServiceAccessor
                .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(sessionData);

            var qrCodeSessionData = new Microsoft.Commerce.Payments.PXService.Model.PXInternal.QRCodeSecondScreenSession
            {
                Id = SessionId,
                AccountId = "Some account id"
            };

            mockQrCodeSessionServiceAccessor
                .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.QRCodeSecondScreenSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(qrCodeSessionData);

            var challengeRequiredPi = new PIMSModel.PaymentInstrument()
            {
                PaymentInstrumentDetails = new PIMSModel.PaymentInstrumentDetails(),
                PaymentMethod = new PIMSModel.PaymentMethod()
                {
                    PaymentMethodFamily = "credit_card",
                    PaymentMethodType = "visa"
                }
            };

            if (defaultCreditCardChallenge)
            {
                challengeRequiredPi.PaymentInstrumentDetails.RequiredChallenge = new List<string>() { "cvv" };
            }
            else
            {
                challengeRequiredPi.PaymentInstrumentDetails.RequiredChallenge = new List<string>() { "3ds2" };
            }

            mockPims.Setup(x => x.GetPaymentInstrument(
               It.IsAny<string>(),
               ChallengeRequiredPiId,
               It.IsAny<EventTraceActivity>(),
               It.IsAny<string>(),
               It.IsAny<string>(),
               It.IsAny<string>(),
               It.IsAny<List<string>>(),
               It.IsAny<PaymentExperienceSetting>()))
               .Returns(Task.FromResult(challengeRequiredPi));

            mockPims.Setup(x => x.GetExtendedPaymentInstrument(
                ChallengeRequiredPiId,
                It.IsAny<EventTraceActivity>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>()))
                .Returns(Task.FromResult(challengeRequiredPi));

            // set up challenge not required
            var challengeNotRequiredPi = new PIMSModel.PaymentInstrument()
            {
                PaymentInstrumentDetails = new PIMSModel.PaymentInstrumentDetails(),
                PaymentMethod = new PIMSModel.PaymentMethod()
                {
                    PaymentMethodFamily = "credit_card",
                    PaymentMethodType = "visa"
                }
            };

            mockPims.Setup(x => x.GetPaymentInstrument(
               It.IsAny<string>(),
               ChallengeNotRequiredPiId,
               It.IsAny<EventTraceActivity>(),
               It.IsAny<string>(),
               It.IsAny<string>(),
               It.IsAny<string>(),
               It.IsAny<List<string>>(),
               It.IsAny<PaymentExperienceSetting>()))
               .Returns(Task.FromResult(challengeNotRequiredPi));

            mockPims.Setup(x => x.GetExtendedPaymentInstrument(
                ChallengeNotRequiredPiId,
                It.IsAny<EventTraceActivity>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>()))
                .Returns(Task.FromResult(challengeNotRequiredPi));

            mockPayerAuthServiceAccessor.Setup(x => x.CreatePaymentSessionId(
                It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentSessionData>(),
                It.IsAny<EventTraceActivity>()))
                .Returns(Task.FromResult(new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentSessionResponse()
                {
                    PaymentSessionId = Guid.NewGuid().ToString()
                }));

            mockPayerAuthServiceAccessor.Setup(x => x.Authenticate(
                It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationRequest>(),
                It.IsAny<EventTraceActivity>()))
                .Returns(Task.FromResult(new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationResponse()
                {
                    EnrollmentStatus = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentInstrumentEnrollmentStatus.Bypassed,
                    EnrollmentType = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentInstrumentEnrollmentType.ThreeDs,
                    TransactionStatus = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.TransactionStatus.Y,
                    TransactionStatusReason = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.TransactionStatusReason.TSR01
                }));
        }

        [TestMethod]
        [DataRow("PXPSD2ProdIntegration")]
        [DataRow("PXPSD2ProdIntegration,PXSkipDuplicatePostProcessForMotoAndRewards")]
        [DataRow(null)]
        public async Task CreatePaymentSession_MOTO_FlightingFeatures_Successed(string flightsString)
        {
            // Arrange
            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            var flights = flightsString?.Split(',')?.ToList();

            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                Language = "en",
                Amount = 10.0m,
                Currency = "EUR",
                Partner = "webblends",
                Country = "de",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = true,
                BillableAccountId = "blah+",
                ClassicProduct = "fooBar",
                PaymentInstrumentId = ChallengeRequiredPiId
            };

            // Act
            var result = await paymentSessionsHandler.CreatePaymentSession(
                accountId: AcountId,
                paymentSessionData: paymentSessionData,
                deviceChannel: DeviceChannel.AppBased,
                emailAddress: "test@outlook.com",
                exposedFlightFeatures: flights,
                traceActivityId: new EventTraceActivity(),
                testContext: new Microsoft.Commerce.Payments.Common.Transaction.TestContext(),
                isMotoAuthorized: "true");

            // Assert
            Assert.IsFalse(result.IsChallengeRequired);
            Assert.AreEqual(PaymentChallengeStatus.ByPassed, result.ChallengeStatus);
            mockTransactionDataServiceAccessor.Verify(x => x.UpdateCustomerChallengeAttestation(It.IsNotNull<string>(), It.IsNotNull<string>(), It.IsAny<bool>(), It.IsAny<EventTraceActivity>()), Times.Once);
        }

        [TestMethod]
        [DataRow("PXPSD2ProdIntegration", false, PaymentChallengeStatus.ByPassed)]
        [DataRow(null, false, PaymentChallengeStatus.ByPassed)]
        [DataRow("PXPSD2ProdIntegration,PXEnableChallengesForMOTO", true, PaymentChallengeStatus.Unknown)]
        public async Task CreatePaymentSession_MOTO_ChallengeRequired(string flightsString, bool expectedChallengeRequired, PaymentChallengeStatus expectedChallengeStatus)
        {
            // Arrange
            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            var flights = flightsString?.Split(',')?.ToList();

            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                Language = "en",
                Amount = 10.0m,
                Currency = "EUR",
                Partner = "webblends",
                Country = "de",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = true,
                BillableAccountId = "blah+",
                ClassicProduct = "fooBar",
                PaymentInstrumentId = ChallengeRequiredPiId
            };

            // Act
            var result = await paymentSessionsHandler.CreatePaymentSession(
                accountId: AcountId,
                paymentSessionData: paymentSessionData,
                deviceChannel: DeviceChannel.AppBased,
                emailAddress: "test@outlook.com",
                exposedFlightFeatures: flights,
                traceActivityId: new EventTraceActivity(),
                testContext: new Microsoft.Commerce.Payments.Common.Transaction.TestContext(),
                isMotoAuthorized: "true");

            // Assert
            Assert.AreEqual(result.IsChallengeRequired, expectedChallengeRequired, "ChallengeRequired does not match");
            Assert.AreEqual(result.ChallengeStatus, expectedChallengeStatus, "ChallengeStatus does not Match");
        }

        [TestMethod]
        public async Task CreatePaymentSession_QRCode_Success()
        {
            // Arrange
            var secondScreenPaymentSessionsHandlerV2 = new SecondScreenSessionHandler(
                mockQrCodeSessionServiceAccessor.Object);

            Microsoft.Commerce.Payments.PXService.Model.PXInternal.QRCodeSecondScreenSession qrCodePaymentSessionData = new Microsoft.Commerce.Payments.PXService.Model.PXInternal.QRCodeSecondScreenSession();
            qrCodePaymentSessionData.Language = "en-us";
            qrCodePaymentSessionData.Partner = "xboxsettings";
            qrCodePaymentSessionData.Country = "us";
            qrCodePaymentSessionData.AccountId = Guid.NewGuid().ToString();
            qrCodePaymentSessionData.UseCount = 0;
            qrCodePaymentSessionData.Operation = "Add";

            // Act
            var result = await secondScreenPaymentSessionsHandlerV2.CreateAddCCQRCodePaymentSession(
                context: qrCodePaymentSessionData,
                traceActivityId: new EventTraceActivity());

            // Assert
            Assert.IsNotNull(result);
            var qrCodePaymentSessionData2 = await secondScreenPaymentSessionsHandlerV2.GetQrCodeSessionData(result.Id, null);
            Assert.IsNotNull(qrCodePaymentSessionData2);
        }

        [TestMethod]
        [DataRow("webblends", true, PaymentChallengeStatus.Unknown, true, false)]
        [DataRow("xbet", true, PaymentChallengeStatus.Unknown, true, false)]
        [DataRow("officesmb", true, PaymentChallengeStatus.Unknown, false, true)]
        [DataRow("officesmb", true, PaymentChallengeStatus.Unknown, true, false)]
        [DataRow("officesmb", true, PaymentChallengeStatus.Unknown, false, false)]
        public async Task CreatePaymentSession_Success_ChallengeRequired(string partner, bool challengeRequired, PaymentChallengeStatus expectedChallengeStatus, bool isFlighted, bool isSettingEnabledForPSD2)
        {
            // Arrange
            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            var flights = "PXPSD2ProdIntegration".Split(',').ToList();
            PaymentExperienceSetting setting = null;
            if (isSettingEnabledForPSD2)
            {
                string settingJsonString = "{\"template\":\"onepage\",\"features\":{\"PSD2\":{\"applicableMarkets\":[]}}}";
                setting = JsonConvert.DeserializeObject<PaymentExperienceSetting>(settingJsonString);
            }

            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                Language = "en",
                Amount = 10.0m,
                Currency = "EUR",
                Partner = partner,
                Country = "de",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
                BillableAccountId = "blah+",
                ClassicProduct = "fooBar",
                PaymentInstrumentId = ChallengeRequiredPiId
            };

            // Act
            var result = await paymentSessionsHandler.CreatePaymentSession(
                accountId: AcountId,
                paymentSessionData: paymentSessionData,
                deviceChannel: DeviceChannel.Browser,
                emailAddress: "test@outlook.com",
                exposedFlightFeatures: isFlighted ? flights : new List<string>(),
                traceActivityId: new EventTraceActivity(),
                testContext: new Microsoft.Commerce.Payments.Common.Transaction.TestContext(),
                isMotoAuthorized: "true",
                setting: setting);

            // Assert
            Assert.AreEqual(result.IsChallengeRequired, challengeRequired);
            Assert.AreEqual(expectedChallengeStatus, result.ChallengeStatus);
        }

        [TestMethod]
        [DataRow("webblends", true, PaymentChallengeStatus.Unknown, true, false)]
        [DataRow("xbet", true, PaymentChallengeStatus.Unknown, true, false)]
        [DataRow("officesmb", true, PaymentChallengeStatus.Unknown, false, true)]
        [DataRow("officesmb", true, PaymentChallengeStatus.Unknown, true, false)]
        [DataRow("officesmb", true, PaymentChallengeStatus.Unknown, false, false)]
        public async Task CreatePaymentSession_Success_ChallengeRequired_PXEnablePSD2PaymentInstrumentSession(string partner, bool challengeRequired, PaymentChallengeStatus expectedChallengeStatus, bool isFlighted, bool isSettingEnabledForPSD2)
        {
            // Arrange
            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            var flights = "PXPSD2ProdIntegration,PXEnablePSD2PaymentInstrumentSession".Split(',').ToList();
            PaymentExperienceSetting setting = null;
            if (isSettingEnabledForPSD2)
            {
                string settingJsonString = "{\"template\":\"onepage\",\"features\":{\"PSD2\":{\"applicableMarkets\":[]}}}";
                setting = JsonConvert.DeserializeObject<PaymentExperienceSetting>(settingJsonString);
            }

            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                Language = "en",
                Amount = 10.0m,
                Currency = "EUR",
                Partner = partner,
                Country = "de",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
                BillableAccountId = "blah+",
                ClassicProduct = "fooBar",
                PaymentInstrumentId = ChallengeRequiredPiId
            };

            PaymentInstrumentSession paymentInstrumentSession = new PaymentInstrumentSession(SessionId, AcountId, new List<string>() { "3ds2" });
            PaymentInstrumentSession testPiSession = null;

            // Act
            var result = await paymentSessionsHandler.CreatePaymentSession(
                accountId: AcountId,
                paymentSessionData: paymentSessionData,
                deviceChannel: DeviceChannel.Browser,
                emailAddress: "test@outlook.com",
                exposedFlightFeatures: isFlighted ? flights : new List<string>(),
                traceActivityId: new EventTraceActivity(),
                testContext: new Microsoft.Commerce.Payments.Common.Transaction.TestContext(),
                isMotoAuthorized: "true",
                setting: setting);

            // Assert
            Assert.AreEqual(result.IsChallengeRequired, challengeRequired);
            Assert.AreEqual(expectedChallengeStatus, result.ChallengeStatus);

            if (isFlighted)
            {
                // verify the PI session
                Func<PaymentInstrumentSession, bool> evaluatePiSessionAccountId = (piSession) =>
                {
                    testPiSession = piSession;
                    return piSession.AccountId == AcountId;
                };

                mockSessionServiceAccessor.Verify(x => x.UpdateSessionResourceData(It.IsAny<string>(), It.Is<PaymentInstrumentSession>(session => evaluatePiSessionAccountId(session)), It.IsAny<EventTraceActivity>()), Times.AtLeastOnce);
                mockSessionServiceAccessor
                    .Setup(x => x.GetSessionResourceData<PaymentInstrumentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                    .ReturnsAsync(testPiSession);

                var piSessionRes = await mockSessionServiceAccessor.Object.GetSessionResourceData<PaymentInstrumentSession>("PX-3DS2-" + ChallengeRequiredPiId, new EventTraceActivity());
                Assert.IsNotNull(piSessionRes);
            }
        }

        [TestMethod]
        [DataRow("webblends", true, true, "us", "USD")]
        [DataRow("webblends", true, true, "de", "EUR")]
        [DataRow("webblends", true, true, "il", "EUR")]
        public async Task CreatePaymentSession_Success_ChallengeNotRequired(string partner, bool isFlighted, bool isSettingEnabledForPSD2, string market, string currency)
        {
            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            var flights = "PXPSD2ProdIntegration".Split(',').ToList();
            PaymentExperienceSetting setting = null;
            if (isSettingEnabledForPSD2)
            {
                string settingJsonString = "{\"template\":\"onepage\",\"features\":{\"PSD2\":{\"applicableMarkets\":[]}}}";
                setting = JsonConvert.DeserializeObject<PaymentExperienceSetting>(settingJsonString);
            }

            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                Language = "en",
                Amount = 10.0m,
                Currency = currency,
                Partner = partner,
                Country = market,
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
                BillableAccountId = "blah+",
                ClassicProduct = "fooBar",
                PaymentInstrumentId = ChallengeNotRequiredPiId
            };
            Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession testSession = null;

            mockSessionServiceAccessor
                .Setup(x => x.CreateSessionFromData(It.IsAny<string>(), It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(), It.IsAny<EventTraceActivity>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await paymentSessionsHandler.CreatePaymentSession(
                accountId: AcountId,
                paymentSessionData: paymentSessionData,
                deviceChannel: DeviceChannel.Browser,
                emailAddress: "test@outlook.com",
                exposedFlightFeatures: isFlighted ? flights : new List<string>(),
                traceActivityId: new EventTraceActivity(),
                testContext: new Microsoft.Commerce.Payments.Common.Transaction.TestContext(),
                isMotoAuthorized: "true",
                setting: setting);

            Func<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession, bool> evaluateAccountId = (paymentSession) =>
            {
                testSession = paymentSession;
                return paymentSession.PaymentInstrumentAccountId == AcountId;
            };

            mockSessionServiceAccessor.Verify(x => x.CreateSessionFromData(It.IsAny<string>(), It.Is<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(session => evaluateAccountId(session)), It.IsAny<EventTraceActivity>()), Times.AtLeastOnce);
            mockSessionServiceAccessor
                .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(testSession);

            var res = await paymentSessionsHandler.GetStoredSession(result.Id, new EventTraceActivity());

            bool verified = string.Equals(ChallengeNotRequiredPiId, res.PaymentInstrumentId, StringComparison.InvariantCultureIgnoreCase)
                   && string.Equals(AcountId, res.PaymentInstrumentAccountId, StringComparison.InvariantCultureIgnoreCase)
                   && (!res.PiRequiresAuthentication
                   || res.ChallengeStatus == PaymentChallengeStatus.ByPassed
                   || res.ChallengeStatus == PaymentChallengeStatus.NotApplicable
                   || res.ChallengeStatus == PaymentChallengeStatus.Succeeded);

            Assert.IsTrue(verified);
        }

        [TestMethod]
        [DataRow("webblends", true, true, "us", "USD")]
        [DataRow("webblends", true, true, "de", "EUR")]
        [DataRow("webblends", true, true, "il", "EUR")]
        public async Task CreatePaymentSession_Success_ChallengeNotRequired_PXEnablePSD2PaymentInstrumentSession(string partner, bool isFlighted, bool isSettingEnabledForPSD2, string market, string currency)
        {
            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            var flights = "PXPSD2ProdIntegration,PXEnablePSD2PaymentInstrumentSession".Split(',').ToList();
            PaymentExperienceSetting setting = null;
            if (isSettingEnabledForPSD2)
            {
                string settingJsonString = "{\"template\":\"onepage\",\"features\":{\"PSD2\":{\"applicableMarkets\":[]}}}";
                setting = JsonConvert.DeserializeObject<PaymentExperienceSetting>(settingJsonString);
            }

            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                Language = "en",
                Amount = 10.0m,
                Currency = currency,
                Partner = partner,
                Country = market,
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
                BillableAccountId = "blah+",
                ClassicProduct = "fooBar",
                PaymentInstrumentId = ChallengeNotRequiredPiId
            };
            Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession testSession = null;

            PaymentInstrumentSession paymentInstrumentSession = new PaymentInstrumentSession(SessionId, AcountId, null);
            PaymentInstrumentSession testPiSession = null;

            mockSessionServiceAccessor
                .Setup(x => x.CreateSessionFromData(It.IsAny<string>(), It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(), It.IsAny<EventTraceActivity>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await paymentSessionsHandler.CreatePaymentSession(
                accountId: AcountId,
                paymentSessionData: paymentSessionData,
                deviceChannel: DeviceChannel.Browser,
                emailAddress: "test@outlook.com",
                exposedFlightFeatures: isFlighted ? flights : new List<string>(),
                traceActivityId: new EventTraceActivity(),
                testContext: new Microsoft.Commerce.Payments.Common.Transaction.TestContext(),
                isMotoAuthorized: "true",
                setting: setting);

            Func<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession, bool> evaluateAccountId = (paymentSession) =>
            {
                testSession = paymentSession;
                return paymentSession.PaymentInstrumentAccountId == AcountId;
            };

            mockSessionServiceAccessor.Verify(x => x.CreateSessionFromData(It.IsAny<string>(), It.Is<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(session => evaluateAccountId(session)), It.IsAny<EventTraceActivity>()), Times.AtLeastOnce);
            mockSessionServiceAccessor
                .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(testSession);

            var res = await paymentSessionsHandler.GetStoredSession(result.Id, new EventTraceActivity());

            bool verified = string.Equals(ChallengeNotRequiredPiId, res.PaymentInstrumentId, StringComparison.InvariantCultureIgnoreCase)
                   && string.Equals(AcountId, res.PaymentInstrumentAccountId, StringComparison.InvariantCultureIgnoreCase)
                   && (!res.PiRequiresAuthentication
                   || res.ChallengeStatus == PaymentChallengeStatus.ByPassed
                   || res.ChallengeStatus == PaymentChallengeStatus.NotApplicable
                   || res.ChallengeStatus == PaymentChallengeStatus.Succeeded);

            Assert.IsTrue(verified);

            // verify the PI session
            Func<PaymentInstrumentSession, bool> evaluatePiSessionAccountId = (piSession) =>
            {
                testPiSession = piSession;
                return piSession.AccountId == AcountId;
            };

            mockSessionServiceAccessor.Verify(x => x.UpdateSessionResourceData(It.IsAny<string>(), It.Is<PaymentInstrumentSession>(session => evaluatePiSessionAccountId(session)), It.IsAny<EventTraceActivity>()), Times.AtLeastOnce);
            mockSessionServiceAccessor
                .Setup(x => x.GetSessionResourceData<PaymentInstrumentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(testPiSession);

            var piSessionRes = await mockSessionServiceAccessor.Object.GetSessionResourceData<PaymentInstrumentSession>("PX-3DS2-" + ChallengeNotRequiredPiId, new EventTraceActivity());
            Assert.IsNotNull(piSessionRes);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task CreatePaymentSession_ChallengeRequired_PIOnAttachPartners(bool isFlighted)
        {
            // Initialize PI with Cvv challenge
            defaultCreditCardChallenge = true;
            TestInitialize();

            // Arrange
            var partners = new string[] { "webblends", "amcweb", "officeoobe" };

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            var flights = new List<string>() { "PXEnableGettingStoredSessionForChallengeDescriptionsController", "PXAuthenticateChallengeTypeOnStoredSession" };

            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                Language = "en",
                Amount = 0m,
                Currency = "EUR",
                Partner = "webblends",
                Country = "de",
                HasPreOrder = true,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
                BillableAccountId = "blah+",
                ClassicProduct = "fooBar",
                PaymentInstrumentId = ChallengeRequiredPiId
            };

            foreach (var partner in partners)
            {
                paymentSessionData.Partner = partner;

                // Act
                var sessionResult = await paymentSessionsHandler.CreatePaymentSession(
                    accountId: AcountId,
                    paymentSessionData: paymentSessionData,
                    deviceChannel: DeviceChannel.Browser,
                    emailAddress: "test@outlook.com",
                    exposedFlightFeatures: isFlighted ? flights : new List<string>(),
                    traceActivityId: new EventTraceActivity(),
                    testContext: new Common.TestContext(),
                    isMotoAuthorized: "true");

                var authResult = await paymentSessionsHandler.Authenticate(AcountId, sessionResult.Id, new AuthenticationRequest(), flights, new EventTraceActivity(), null);

                // Assert
                Assert.IsTrue(sessionResult.IsChallengeRequired);
                Assert.AreEqual(PaymentChallengeStatus.Unknown, sessionResult.ChallengeStatus);
                Assert.AreEqual("ValidatePIOnAttachChallenge", sessionResult.ChallengeType);
                Assert.AreEqual(PaymentChallengeStatus.Succeeded, authResult.ChallengeStatus);
                Assert.AreEqual(PaymentInstrumentEnrollmentStatus.Bypassed, authResult.EnrollmentStatus);
                Assert.AreEqual(isFlighted, authResult.DisplayStrings == null);
            }

            // Reset the challenge type for PI
            defaultCreditCardChallenge = false;
            TestInitialize();
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public async Task CreatePaymentSession_InvalidMotoFlagNull_EmptyFlighting()
        {
            // Arrange
            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                Language = "en",
                Amount = 10.0m,
                Currency = "EUR",
                Partner = "webblends",
                Country = "de",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = true,
                BillableAccountId = "blah+",
                ClassicProduct = "fooBar",
                PaymentInstrumentId = ChallengeRequiredPiId
            };

            // Act
            var result = await paymentSessionsHandler.CreatePaymentSession(
                accountId: AcountId,
                paymentSessionData: paymentSessionData,
                deviceChannel: DeviceChannel.AppBased,
                emailAddress: "test@outlook.com",
                exposedFlightFeatures: flightings,
                traceActivityId: new EventTraceActivity(),
                testContext: new Microsoft.Commerce.Payments.Common.Transaction.TestContext(),
                isMotoAuthorized: null);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public async Task CreatePaymentSession_MOTO_InvalidMotoFlagValue_NullFlighting_Failed()
        {
            // Arrange
            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                Language = "en",
                Amount = 10.0m,
                Currency = "EUR",
                Partner = "webblends",
                Country = "de",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = true,
                BillableAccountId = "blah+",
                ClassicProduct = "fooBar",
                PaymentInstrumentId = ChallengeRequiredPiId
            };

            // Act
            var result = await paymentSessionsHandler.CreatePaymentSession(
                accountId: AcountId,
                paymentSessionData: paymentSessionData,
                deviceChannel: DeviceChannel.AppBased,
                emailAddress: "test@outlook.com",
                exposedFlightFeatures: null,
                traceActivityId: new EventTraceActivity(),
                testContext: new Microsoft.Commerce.Payments.Common.Transaction.TestContext(),
                isMotoAuthorized: "false");
        }

        [TestMethod]
        [DataRow(null, true)]
        [DataRow("", true)]
        [DataRow(AcountId, true)]
        [DataRow("MismatchedAccountId", true)]
        public async Task CreatePaymentSession_VerifyAuthorization_piCid(string piCid, bool expectedAuthorizationToSucceed)
        {
            // Arrange
            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            var flights = "PXPSD2ProdIntegration".Split(',').ToList();

            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                Language = "en-US",
                Amount = 0,
                Currency = "USD",
                Partner = "commercialstores",
                Country = "us",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.RecurringTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Two,
                IsMOTO = false,
                PaymentInstrumentId = ChallengeRequiredPiId,
                PaymentInstrumentAccountId = piCid
            };

            bool actualAuthroizationSucceeds = false;
            try
            {
                // Act
                var result = await paymentSessionsHandler.CreatePaymentSession(
                    accountId: AcountId,
                    paymentSessionData: paymentSessionData,
                    deviceChannel: DeviceChannel.Browser,
                    emailAddress: "test@outlook.com",
                    exposedFlightFeatures: flights,
                    traceActivityId: new EventTraceActivity(),
                    testContext: new Microsoft.Commerce.Payments.Common.Transaction.TestContext());

                actualAuthroizationSucceeds = true;
            }
            catch (ValidationException ex)
            {
                // Assert
                Assert.AreEqual(ex.ErrorCode, ErrorCode.InvalidRequestData, "The error code for authorization failure is not as expected");
            }

            Assert.AreEqual(expectedAuthorizationToSucceed, actualAuthroizationSucceeds, string.Format("Expected authorization to {0}", expectedAuthorizationToSucceed ? "succeed" : "fail"));
        }

        [TestMethod]
        [DataRow("storify", true, "India3DSChallenge", "PXEnableIndia3DS1Challenge,India3dsEnableForBilldesk")]
        [DataRow("webblends", true, "India3DSChallenge", "PXEnableIndia3DS1Challenge,India3dsEnableForBilldesk")]
        [DataRow("azure", true, "India3DSChallenge", "")]
        [DataRow("commercialstores", true, "India3DSChallenge", "")]
        public async Task CreatePaymentSession_India3DS_ChallengeRequired(string partner, bool expectedChallengeRequired, string expectedChallengeType, string flightsOverrides)
        {
            // Arrange
            var flights = flightsOverrides.Split(',').ToList();

            var india3ds1MockPims = new Mock<IPIMSAccessor>();

            var india3DS1Challenge = new PIMSModel.PaymentInstrument()
            {
                PaymentInstrumentDetails = new PIMSModel.PaymentInstrumentDetails(),
                PaymentMethod = new PIMSModel.PaymentMethod()
                {
                    PaymentMethodFamily = "credit_card",
                    PaymentMethodType = "visa"
                }
            };
            india3DS1Challenge.PaymentInstrumentDetails.RequiredChallenge = new List<string>() { "3ds" };

            india3ds1MockPims.Setup(x => x.GetPaymentInstrument(
               It.IsAny<string>(),
               ChallengeRequiredPiId,
               It.IsAny<EventTraceActivity>(),
               It.IsAny<string>(),
               It.IsAny<string>(),
               It.IsAny<string>(),
               It.IsAny<List<string>>(),
               It.IsAny<PaymentExperienceSetting>()))
               .Returns(Task.FromResult(india3DS1Challenge));

            india3ds1MockPims.Setup(x => x.GetExtendedPaymentInstrument(
                ChallengeRequiredPiId,
                It.IsAny<EventTraceActivity>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>()))
                .Returns(Task.FromResult(india3DS1Challenge));

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                india3ds1MockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                Language = "en",
                Amount = 10,
                Currency = "INR",
                Partner = partner,
                Country = "IN",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Five,
                BillableAccountId = "blah+",
                ClassicProduct = "fooBar",
                PaymentInstrumentId = ChallengeRequiredPiId
            };

            // Act
            var result = await paymentSessionsHandler.CreatePaymentSession(
                accountId: AcountId,
                paymentSessionData: paymentSessionData,
                deviceChannel: DeviceChannel.AppBased,
                emailAddress: "test@outlook.com",
                exposedFlightFeatures: flights,
                traceActivityId: new EventTraceActivity(),
                testContext: new Microsoft.Commerce.Payments.Common.Transaction.TestContext());

            // Assert
            Assert.IsTrue(result.IsChallengeRequired == expectedChallengeRequired);
            Assert.IsTrue(string.Equals(result.ChallengeType, expectedChallengeType, StringComparison.InvariantCultureIgnoreCase));
        }

        [TestMethod]
        [DataRow("storify", "0.0", "IN", true, true, "India3DSChallenge", "PXEnableIndia3DS1Challenge,India3dsEnableForBilldesk,PXSkipChallengeForZeroAmountIndiaAuth")]
        [DataRow("webblends", "0.0", "IN", false, false, null, "PXEnableIndia3DS1Challenge,India3dsEnableForBilldesk,PXSkipChallengeForZeroAmountIndiaAuth")]
        [DataRow("webblends", "0.0", "IN", true, true, "India3DSChallenge", "PXEnableIndia3DS1Challenge,India3dsEnableForBilldesk")]
        [DataRow("webblends", "10.0", "IN", true, true, "India3DSChallenge", "PXEnableIndia3DS1Challenge,India3dsEnableForBilldesk,PXSkipChallengeForZeroAmountIndiaAuth")]
        [DataRow("webblends", "0.0", "US", false, false, null, "PXEnableIndia3DS1Challenge,India3dsEnableForBilldesk,PXSkipChallengeForZeroAmountIndiaAuth")]
        [DataRow("azure", "0.0", "IN", true, true, "India3DSChallenge", "PXEnableIndia3DS1Challenge,India3dsEnableForBilldesk,PXSkipChallengeForZeroAmountIndiaAuth")]
        [DataRow("commercialstores", "0.0", "IN", true, true, "India3DSChallenge", "PXEnableIndia3DS1Challenge,India3dsEnableForBilldesk,PXSkipChallengeForZeroAmountIndiaAuth")]
        public async Task CreatePaymentSession_India3DSWebblends_ChallengeNotRequiredForZeroAmount(string partner, string amount, string country, bool expectedChallengeRequired, bool zsession, string expectedChallengeType, string flightsOverrides)
        {
            // Arrange
            var flights = flightsOverrides.Split(',').ToList();

            var india3ds1MockPims = new Mock<IPIMSAccessor>();

            var india3DS1Challenge = new PIMSModel.PaymentInstrument()
            {
                PaymentInstrumentDetails = new PIMSModel.PaymentInstrumentDetails(),
                PaymentMethod = new PIMSModel.PaymentMethod()
                {
                    PaymentMethodFamily = "credit_card",
                    PaymentMethodType = "visa"
                }
            };
            india3DS1Challenge.PaymentInstrumentDetails.RequiredChallenge = new List<string>() { "3ds" };

            india3ds1MockPims.Setup(x => x.GetPaymentInstrument(
               It.IsAny<string>(),
               ChallengeRequiredPiId,
               It.IsAny<EventTraceActivity>(),
               It.IsAny<string>(),
               It.IsAny<string>(),
               It.IsAny<string>(),
               It.IsAny<List<string>>(),
               It.IsAny<PaymentExperienceSetting>()))
               .Returns(Task.FromResult(india3DS1Challenge));

            india3ds1MockPims.Setup(x => x.GetExtendedPaymentInstrument(
                ChallengeRequiredPiId,
                It.IsAny<EventTraceActivity>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>()))
                .Returns(Task.FromResult(india3DS1Challenge));

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                india3ds1MockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                Language = "en",
                Amount = decimal.Parse(amount),
                Currency = "INR",
                Partner = partner,
                Country = country,
                PurchaseOrderId = null,
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.RecurringTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Five,
                BillableAccountId = "blah+",
                ClassicProduct = "fooBar",
                PaymentInstrumentId = ChallengeRequiredPiId,
            };

            // Act
            var result = await paymentSessionsHandler.CreatePaymentSession(
                accountId: AcountId,
                paymentSessionData: paymentSessionData,
                deviceChannel: DeviceChannel.AppBased,
                emailAddress: "test@outlook.com",
                exposedFlightFeatures: flights,
                traceActivityId: new EventTraceActivity(),
                testContext: new Microsoft.Commerce.Payments.Common.Transaction.TestContext());

            // Assert
            Assert.IsTrue(result.IsChallengeRequired == expectedChallengeRequired);
            Assert.IsTrue(string.Equals(result.ChallengeType, expectedChallengeType, StringComparison.InvariantCultureIgnoreCase));
        }

        [TestMethod]
        [DataRow("officesmb", true, "India3DSChallenge", "consumer", true)]
        [DataRow("officesmb", true, "India3DSChallenge", "commercial", true)]
        [DataRow("officesmb", false, null, null, false)]
        public async Task CreatePaymentSession_India3DS_ChallengeRequired_PartnerSetting(string partner, bool expectedChallengeRequired, string expectedChallengeType, string partnerType, bool enabledIndia3dsInPartnerSetting)
        {
            // Arrange
            var india3ds1MockPims = new Mock<IPIMSAccessor>();

            var india3DS1Challenge = new PIMSModel.PaymentInstrument()
            {
                PaymentInstrumentDetails = new PIMSModel.PaymentInstrumentDetails(),
                PaymentMethod = new PIMSModel.PaymentMethod()
                {
                    PaymentMethodFamily = "credit_card",
                    PaymentMethodType = "visa"
                }
            };
            india3DS1Challenge.PaymentInstrumentDetails.RequiredChallenge = new List<string>() { "3ds" };

            india3ds1MockPims.Setup(x => x.GetPaymentInstrument(
               It.IsAny<string>(),
               ChallengeRequiredPiId,
               It.IsAny<EventTraceActivity>(),
               It.IsAny<string>(),
               It.IsAny<string>(),
               It.IsAny<string>(),
               It.IsAny<List<string>>(),
               It.IsAny<PaymentExperienceSetting>()))
               .Returns(Task.FromResult(india3DS1Challenge));

            india3ds1MockPims.Setup(x => x.GetExtendedPaymentInstrument(
                ChallengeRequiredPiId,
                It.IsAny<EventTraceActivity>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>()))
                .Returns(Task.FromResult(india3DS1Challenge));

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                india3ds1MockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                Language = "en",
                Amount = 10,
                Currency = "INR",
                Partner = partner,
                Country = "IN",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Five,
                BillableAccountId = "blah+",
                ClassicProduct = "fooBar",
                PaymentInstrumentId = ChallengeRequiredPiId
            };

            PaymentExperienceSetting setting = null;

            if (enabledIndia3dsInPartnerSetting)
            {
                string settingJson = null;

                if (string.Equals(partnerType, "consumer"))
                {
                    settingJson = "{\"template\":\"defaultTemplate\",\"features\":{\"threeDSOne\":{\"applicableMarkets\":[\"in\"],\"displayCustomizationDetail\":[{\"pxEnableIndia3DS1Challenge\":true,\"india3dsEnableForBilldesk\":true}]}}}";
                }
                else if (string.Equals(partnerType, "commercial"))
                {
                    settingJson = "{\"template\":\"defaultTemplate\",\"features\":{\"threeDSOne\":{\"applicableMarkets\":[\"in\"],\"displayCustomizationDetail\":[{\"enableIndia3dsForNonZeroPaymentTransaction\":true,}]}}}";
                }

                setting = JsonConvert.DeserializeObject<PaymentExperienceSetting>(settingJson);
            }

            // Act
            var result = await paymentSessionsHandler.CreatePaymentSession(
                accountId: AcountId,
                paymentSessionData: paymentSessionData,
                deviceChannel: DeviceChannel.AppBased,
                emailAddress: "test@outlook.com",
                exposedFlightFeatures: null,
                traceActivityId: new EventTraceActivity(),
                testContext: new Microsoft.Commerce.Payments.Common.Transaction.TestContext(),
                setting: setting);

            // Assert
            Assert.IsTrue(result.IsChallengeRequired == expectedChallengeRequired);
            if (expectedChallengeRequired)
            {
                Assert.IsTrue(string.Equals(result.ChallengeType, expectedChallengeType, StringComparison.InvariantCultureIgnoreCase));
            }
            else
            {
                Assert.IsTrue(string.Equals(result.ChallengeStatus.ToString(), "NotApplicable", StringComparison.InvariantCultureIgnoreCase));
            }
        }

        [TestMethod]
        [DataRow("webblends", true, "UpiChallenge", "consumer", true, "EnableLtsUpiQRConsumer")]
        [DataRow("webblends", true, null, "consumer", null, null)]
        public async Task CreatePaymentSession_UpiQRLts_ChallengeRequired(string partner, bool expectedChallengeRequired, string expectedChallengeType, string partnerType, bool enabledIndia3dsInPartnerSetting, string flight)
        {
            List<string> exposedFlightFeatures = new List<string>();
            if (flight != null)
            {
                exposedFlightFeatures.Add("EnableLtsUpiQRConsumer");
            }

            // Arrange
            var upiQRLtsMockPims = new Mock<IPIMSAccessor>();

            var upiChallenge = new PIMSModel.PaymentInstrument()
            {
                PaymentInstrumentDetails = new PIMSModel.PaymentInstrumentDetails(),
                PaymentMethod = new PIMSModel.PaymentMethod()
                {
                    PaymentMethodFamily = "real_time_payments",
                    PaymentMethodType = "upi_qr"
                }
            };
            upiChallenge.PaymentInstrumentDetails.RequiredChallenge = new List<string>();

            upiQRLtsMockPims.Setup(x => x.GetPaymentInstrument(
               It.IsAny<string>(),
               ChallengeRequiredPiId,
               It.IsAny<EventTraceActivity>(),
               It.IsAny<string>(),
               It.IsAny<string>(),
               It.IsAny<string>(),
               It.IsAny<List<string>>(),
               It.IsAny<PaymentExperienceSetting>()))
               .Returns(Task.FromResult(upiChallenge));

            upiQRLtsMockPims.Setup(x => x.GetExtendedPaymentInstrument(
                ChallengeRequiredPiId,
                It.IsAny<EventTraceActivity>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>()))
                .Returns(Task.FromResult(upiChallenge));

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                upiQRLtsMockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                Language = "en",
                Amount = 10,
                Currency = "INR",
                Partner = partner,
                Country = "IN",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Five,
                BillableAccountId = "blah+",
                ClassicProduct = "fooBar",
                PaymentInstrumentId = ChallengeRequiredPiId
            };

            PaymentExperienceSetting setting = null;

            // Act
            var result = await paymentSessionsHandler.CreatePaymentSession(
                accountId: AcountId,
                paymentSessionData: paymentSessionData,
                deviceChannel: DeviceChannel.AppBased,
                emailAddress: "test@outlook.com",
                exposedFlightFeatures: exposedFlightFeatures,
                traceActivityId: new EventTraceActivity(),
                testContext: new Microsoft.Commerce.Payments.Common.Transaction.TestContext(),
                setting: setting);

            if (flight != null)
            {
                Assert.IsTrue(result.IsChallengeRequired == true);
                Assert.IsTrue(string.Equals(result.ChallengeType, expectedChallengeType, StringComparison.InvariantCultureIgnoreCase));
            }
            else
            {
                Assert.IsTrue(result.IsChallengeRequired == false);
                Assert.IsNull(result?.ChallengeType);
            }
        }

        [TestMethod]
        [DataRow("cart", "googlepay", true, "PXEnablePSD2ForGooglePay", false)]
        [DataRow("cart", "googlepay", false, null, false)]
        [DataRow("cart", "applepay", true, "PXEnablePSD2ForGooglePay", true)]
        [DataRow("cart", "applepay", true, null, true)]
        public async Task CreatePaymentSession_GooglePayApplePay_ChallengeRequired(string partner, string paymentMethodType, bool isChallengeRequired, string flight, bool isValidatePIOnAttachChallenge)
        {
            List<string> exposedFlightFeatures = new List<string>();
            if (flight != null)
            {
                exposedFlightFeatures.Add(flight);
            }

            // Arrange
            var mockPims = new Mock<IPIMSAccessor>();

            var mockPi = new PIMSModel.PaymentInstrument()
            {
                PaymentInstrumentDetails = new PIMSModel.PaymentInstrumentDetails(),
                PaymentMethod = new PIMSModel.PaymentMethod()
                {
                    PaymentMethodFamily = "ewallet",
                    PaymentMethodType = paymentMethodType
                }
            };

            mockPi.PaymentInstrumentDetails.RequiredChallenge = new List<string>() { "3ds2" };

            mockPims.Setup(x => x.GetPaymentInstrument(
               It.IsAny<string>(),
               ChallengeRequiredPiId,
               It.IsAny<EventTraceActivity>(),
               It.IsAny<string>(),
               It.IsAny<string>(),
               It.IsAny<string>(),
               It.IsAny<List<string>>(),
               It.IsAny<PaymentExperienceSetting>()))
               .Returns(Task.FromResult(mockPi));

            mockPims.Setup(x => x.GetExtendedPaymentInstrument(
                ChallengeRequiredPiId,
                It.IsAny<EventTraceActivity>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>()))
                .Returns(Task.FromResult(mockPi));

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                Language = "en",
                Amount = 10,
                Currency = "INR",
                Partner = partner,
                Country = "IN",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Five,
                BillableAccountId = "blah+",
                ClassicProduct = "fooBar",
                PaymentInstrumentId = ChallengeRequiredPiId
            };

            PaymentExperienceSetting setting = null;

            // Act
            var result = await paymentSessionsHandler.CreatePaymentSession(
                accountId: AcountId,
                paymentSessionData: paymentSessionData,
                deviceChannel: DeviceChannel.AppBased,
                emailAddress: "test@outlook.com",
                exposedFlightFeatures: exposedFlightFeatures,
                traceActivityId: new EventTraceActivity(),
                testContext: new Microsoft.Commerce.Payments.Common.Transaction.TestContext(),
                setting: setting);

            Assert.IsTrue(result.IsChallengeRequired == isChallengeRequired, "challenge should be required for googlePay if flight is enabled");
            Assert.AreEqual(string.Equals(result.ChallengeType, "ValidatePIOnAttachChallenge"), isValidatePIOnAttachChallenge, "challenge type is expected to be ValidatePIOnAttachChallenge");
        }

        [TestMethod]
        [DataRow("cart", "googlepay", true, false)]
        [DataRow("cart", "applepay", true, true)]
        public async Task CreatePaymentSession_GooglePayApplePay_UseTestScenario(string partner, string paymentMethodType, bool isChallengeRequired, bool isValidatePIOnAttachChallenge)
        {
            List<string> exposedFlightFeatures = new List<string>() { "PXEnablePSD2ForGooglePay" };

            // Arrange
            var mockPims = new Mock<IPIMSAccessor>();

            var mockPi = new PIMSModel.PaymentInstrument()
            {
                PaymentInstrumentDetails = new PIMSModel.PaymentInstrumentDetails(),
                PaymentMethod = new PIMSModel.PaymentMethod()
                {
                    PaymentMethodFamily = "ewallet",
                    PaymentMethodType = paymentMethodType
                }
            };

            mockPims.Setup(x => x.GetPaymentInstrument(
               It.IsAny<string>(),
               ChallengeRequiredPiId,
               It.IsAny<EventTraceActivity>(),
               It.IsAny<string>(),
               It.IsAny<string>(),
               It.IsAny<string>(),
               It.IsAny<List<string>>(),
               It.IsAny<PaymentExperienceSetting>()))
               .Returns(Task.FromResult(mockPi));

            mockPims.Setup(x => x.GetExtendedPaymentInstrument(
                ChallengeRequiredPiId,
                It.IsAny<EventTraceActivity>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>()))
                .Returns(Task.FromResult(mockPi));

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                Language = "en",
                Amount = 10,
                Currency = "INR",
                Partner = partner,
                Country = "IN",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Five,
                BillableAccountId = "blah+",
                ClassicProduct = "fooBar",
                PaymentInstrumentId = ChallengeRequiredPiId
            };

            PaymentExperienceSetting setting = null;

            Common.TestContext tc = new Common.TestContext(
                contact: "px.azure.cit",
                retention: DateTime.MaxValue,
                scenarios: "px-service-psd2-e2e-emulator");

            // Act
            var result = await paymentSessionsHandler.CreatePaymentSession(
                accountId: AcountId,
                paymentSessionData: paymentSessionData,
                deviceChannel: DeviceChannel.AppBased,
                emailAddress: "test@outlook.com",
                exposedFlightFeatures: exposedFlightFeatures,
                traceActivityId: new EventTraceActivity(),
                testContext: tc,
                setting: setting);

            Assert.IsTrue(result.IsChallengeRequired == isChallengeRequired, "challenge should be required for googlePay if flight is enabled");
            Assert.AreEqual(string.Equals(result.ChallengeType, "ValidatePIOnAttachChallenge"), isValidatePIOnAttachChallenge, "challenge type is expected to be ValidatePIOnAttachChallenge");
        }

        [TestMethod]
        [DataRow("cart", "ewallet", "googlepay", "gpay")]
        [DataRow("cart", "ewallet",  "applepay", "apay")]
        [DataRow("cart", "credit_card", "visa", null)]
        public async Task CreatePaymentSession_GooglePayApplePay_SetIsTokenCollected(string partner, string paymentMethodFamily, string paymentMethodType, string walletType)
        {
            List<string> exposedFlightFeatures = new List<string>();

            // Arrange
            var mockPims = new Mock<IPIMSAccessor>();

            var mockPi = new PIMSModel.PaymentInstrument()
            {
                PaymentInstrumentDetails = new PIMSModel.PaymentInstrumentDetails()
                {
                    WalletType = walletType
                },
                PaymentMethod = new PIMSModel.PaymentMethod()
                {
                    PaymentMethodFamily = paymentMethodFamily,
                    PaymentMethodType = paymentMethodType
                }
            };

            mockPims.Setup(x => x.GetPaymentInstrument(
               It.IsAny<string>(),
               ChallengeRequiredPiId,
               It.IsAny<EventTraceActivity>(),
               It.IsAny<string>(),
               It.IsAny<string>(),
               It.IsAny<string>(),
               It.IsAny<List<string>>(),
               It.IsAny<PaymentExperienceSetting>()))
               .Returns(Task.FromResult(mockPi));

            mockPims.Setup(x => x.GetExtendedPaymentInstrument(
                ChallengeRequiredPiId,
                It.IsAny<EventTraceActivity>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>()))
                .Returns(Task.FromResult(mockPi));

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                Language = "en",
                Amount = 10,
                Currency = "INR",
                Partner = partner,
                Country = "IN",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Five,
                BillableAccountId = "blah+",
                ClassicProduct = "fooBar",
                PaymentInstrumentId = ChallengeRequiredPiId
            };

            PaymentExperienceSetting setting = null;

            // Act
            var result = await paymentSessionsHandler.CreatePaymentSession(
                accountId: AcountId,
                paymentSessionData: paymentSessionData,
                deviceChannel: DeviceChannel.AppBased,
                emailAddress: "test@outlook.com",
                exposedFlightFeatures: exposedFlightFeatures,
                traceActivityId: new EventTraceActivity(),
                testContext: new Microsoft.Commerce.Payments.Common.Transaction.TestContext(),
                setting: setting);

            Assert.IsTrue(result.IsTokenCollected == (string.Equals(paymentMethodType, "googlepay") || string.Equals(paymentMethodType, "applepay")), "when walletType is apay or gpay, PX should set IsTokenCollected as true in the session returned to client");
        }

        [TestMethod]
        [DataRow(ChallengeScenario.PaymentTransaction, true, "UPIChallenge")]
        [DataRow(ChallengeScenario.RecurringTransaction, true, "UPIChallenge")]
        public async Task CreatePaymentSession_IndiaUPI_ChallengeRequired(ChallengeScenario challenge, bool challengeRequired, string expectedChallengeType)
        {
            var indiaUpiMockPims = new Mock<IPIMSAccessor>();

            var indiaUpiChallenge = new PIMSModel.PaymentInstrument()
            {
                PaymentInstrumentDetails = new PIMSModel.PaymentInstrumentDetails(),
                PaymentMethod = new PIMSModel.PaymentMethod()
                {
                    PaymentMethodFamily = "real_time_payments",
                    PaymentMethodType = "upi"
                }
            };

            indiaUpiMockPims.Setup(x => x.GetPaymentInstrument(
               It.IsAny<string>(),
               ChallengeRequiredPiId,
               It.IsAny<EventTraceActivity>(),
               It.IsAny<string>(),
               It.IsAny<string>(),
               It.IsAny<string>(),
               It.IsAny<List<string>>(),
               It.IsAny<PaymentExperienceSetting>()))
               .Returns(Task.FromResult(indiaUpiChallenge));

            indiaUpiMockPims.Setup(x => x.GetExtendedPaymentInstrument(
                ChallengeRequiredPiId,
                It.IsAny<EventTraceActivity>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>()))
                .Returns(Task.FromResult(indiaUpiChallenge));

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                indiaUpiMockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                Language = "en",
                Amount = 10,
                Currency = "INR",
                Partner = "azure",
                Country = "IN",
                HasPreOrder = false,
                ChallengeScenario = challenge,
                ChallengeWindowSize = ChallengeWindowSize.Five,
                BillableAccountId = "blah+",
                ClassicProduct = "fooBar",
                PaymentInstrumentId = ChallengeRequiredPiId
            };

            // Act
            var result = await paymentSessionsHandler.CreatePaymentSession(
                accountId: AcountId,
                paymentSessionData: paymentSessionData,
                deviceChannel: DeviceChannel.AppBased,
                emailAddress: "test@outlook.com",
                exposedFlightFeatures: new List<string>(),
                traceActivityId: new EventTraceActivity(),
                testContext: new Microsoft.Commerce.Payments.Common.Transaction.TestContext());

            Assert.IsTrue(result.IsChallengeRequired == challengeRequired);
            Assert.IsTrue(string.Equals(result.ChallengeType, expectedChallengeType, StringComparison.InvariantCultureIgnoreCase));
        }

        [TestMethod]
        [DataRow("cart", true, UsageType.Inline, "testcart_LinkedPaymentSessionId", false)]
        [DataRow("cart", false, UsageType.OnFile, "test_PaymentSessionId", false)]
        public async Task Test_CreatePaymentSession_SessionId(string partner, bool isGuestUser, UsageType usageType, string paymentSessionIdResult, bool challengeRequiredResult)
        {
            var guestUserMockPims = new Mock<IPIMSAccessor>();

            var guestUserExtendedPI = new PIMSModel.PaymentInstrument()
            {
                PaymentInstrumentDetails = new PIMSModel.PaymentInstrumentDetails()
                {
                    UsageType = usageType,
                    TransactionLink = new TransactionLink()
                    {
                        LinkedPaymentSessionId = "testcart_LinkedPaymentSessionId",
                    }
                },
                PaymentMethod = new PIMSModel.PaymentMethod()
                {
                    PaymentMethodFamily = "credit_card",
                    PaymentMethodType = "visa"
                }
            };

            guestUserMockPims.Setup(x => x.GetPaymentInstrument(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<EventTraceActivity>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<PaymentExperienceSetting>()))
            .Returns(Task.FromResult(guestUserExtendedPI));

            guestUserMockPims.Setup(pims => pims.GetExtendedPaymentInstrument(
                It.IsAny<string>(),
                It.IsAny<EventTraceActivity>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>()))
            .Returns(Task.FromResult(guestUserExtendedPI));

            mockPayerAuthServiceAccessor.Setup(m => m.CreatePaymentSessionId(
                It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentSessionData>(),
                It.IsAny<EventTraceActivity>()))
            .Returns(Task.FromResult(new PaymentSessionResponse() { PaymentSessionId = "test_PaymentSessionId" }));

            // Arrange
            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                guestUserMockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                Language = "en",
                Amount = 10.0m,
                Currency = "EUR",
                Partner = partner,
                Country = "de",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
                BillableAccountId = "blah+",
                ClassicProduct = "fooBar",
                PaymentInstrumentId = ChallengeRequiredPiId
            };

            // Act
            var result = await paymentSessionsHandler.CreatePaymentSession(
                accountId: AcountId,
                paymentSessionData: paymentSessionData,
                deviceChannel: DeviceChannel.Browser,
                emailAddress: "test@outlook.com",
                exposedFlightFeatures: new List<string>(),
                traceActivityId: new EventTraceActivity(),
                testContext: new Microsoft.Commerce.Payments.Common.Transaction.TestContext(),
                isMotoAuthorized: "true",
                setting: null,
                isGuestUser: isGuestUser);

            // Assert
            Assert.AreEqual(result.IsChallengeRequired, challengeRequiredResult);
            Assert.AreEqual(result.ChallengeStatus, PaymentChallengeStatus.NotApplicable);
            Assert.AreEqual(result.Id, paymentSessionIdResult);
        }

        [TestMethod]
        [DataRow(PaymentChallengeStatus.ByPassed, PaymentInstrumentEnrollmentStatus.Bypassed)]
        [DataRow(PaymentChallengeStatus.Failed, PaymentInstrumentEnrollmentStatus.Unavailable)]
        public async Task HandlePaymentChallenge_IndiaUPI_AuthenticateCallTest(PaymentChallengeStatus expectedStatus, PaymentInstrumentEnrollmentStatus enrollmentStatus)
        {
            // Arrange
            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            var challengeRequiredPi = new PIMSModel.PaymentInstrument()
            {
                PaymentInstrumentDetails = new PIMSModel.PaymentInstrumentDetails(),
                PaymentMethod = new PIMSModel.PaymentMethod()
                {
                    PaymentMethodFamily = "real_time_payments",
                    PaymentMethodType = "upi"
                }
            };

            mockPayerAuthServiceAccessor.Setup(x => x.Authenticate(
                It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationRequest>(),
                It.IsAny<EventTraceActivity>()))
                .Returns(Task.FromResult(new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationResponse()
                {
                    EnrollmentStatus = enrollmentStatus,
                    EnrollmentType = PaymentInstrumentEnrollmentType.ThreeDs,
                }));

            mockPims.Setup(x => x.GetPaymentInstrument(
               AcountId,
               ChallengeRequiredPiId,
               It.IsAny<EventTraceActivity>(),
               It.IsAny<string>(),
               It.IsAny<string>(),
               It.IsAny<string>(),
               It.IsAny<List<string>>(),
               It.IsAny<PaymentExperienceSetting>()))
               .Returns(Task.FromResult(challengeRequiredPi));

            PaymentSession paymentSessionData = new PaymentSession()
            {
                Language = "en",
                Amount = 10.0m,
                Currency = "INR",
                Partner = "webblends",
                Country = "in",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.RecurringTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                BillableAccountId = "blah+",
                ClassicProduct = "fooBar",
                PaymentInstrumentId = ChallengeRequiredPiId,
                IsChallengeRequired = true
            };

            BrowserInfo browserInfo = new BrowserInfo
            {
                ChallengeWindowSize = ChallengeWindowSize.Five
            };

            // Act
            var result = await paymentSessionsHandler.AuthenticateUpiPaymentTxn(
                accountId: AcountId,
                browserInfo,
                paymentSession: paymentSessionData,
                traceActivityId: new EventTraceActivity());

            // Assert
            Assert.IsTrue(result.PaymentSession.IsChallengeRequired);
            Assert.AreEqual(expectedStatus, result.PaymentSession.ChallengeStatus);
        }

        [TestMethod]
        [DataRow(PaymentChallengeStatus.ByPassed, PaymentInstrumentEnrollmentStatus.Bypassed, "EnableLtsUpiQRConsumer")]
        [DataRow(PaymentChallengeStatus.Failed, PaymentInstrumentEnrollmentStatus.Unavailable, "EnableLtsUpiQRConsumer")]
        public async Task HandlePaymentChallenge_IndiaUpiQR_AuthenticateCallTest(PaymentChallengeStatus expectedStatus, PaymentInstrumentEnrollmentStatus enrollmentStatus, string flight)
        {
            List<string> exposedFlightFeatures = new List<string>();
            if (flight != null)
            {
                exposedFlightFeatures.Add("EnableLtsUpiQRConsumer");
            }

            // Arrange
            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            var challengeRequiredPi = new PIMSModel.PaymentInstrument()
            {
                PaymentInstrumentDetails = new PIMSModel.PaymentInstrumentDetails(),
                PaymentMethod = new PIMSModel.PaymentMethod()
                {
                    PaymentMethodFamily = "real_time_payments",
                    PaymentMethodType = "upi_qr"
                }
            };

            mockPayerAuthServiceAccessor.Setup(x => x.Authenticate(
                It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationRequest>(),
                It.IsAny<EventTraceActivity>()))
                .Returns(Task.FromResult(new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationResponse()
                {
                    EnrollmentStatus = enrollmentStatus,
                    EnrollmentType = PaymentInstrumentEnrollmentType.ThreeDs,
                }));

            mockPims.Setup(x => x.GetPaymentInstrument(
               AcountId,
               ChallengeRequiredPiId,
               It.IsAny<EventTraceActivity>(),
               It.IsAny<string>(),
               It.IsAny<string>(),
               It.IsAny<string>(),
               It.IsAny<List<string>>(),
               It.IsAny<PaymentExperienceSetting>()))
               .Returns(Task.FromResult(challengeRequiredPi));

            PaymentSession paymentSessionData = new PaymentSession()
            {
                Language = "en",
                Amount = 10.0m,
                Currency = "INR",
                Partner = "webblends",
                Country = "in",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                BillableAccountId = "blah+",
                ClassicProduct = "fooBar",
                PaymentInstrumentId = ChallengeRequiredPiId,
                IsChallengeRequired = true
            };

            BrowserInfo browserInfo = new BrowserInfo
            {
                ChallengeWindowSize = ChallengeWindowSize.Five
            };

            // Act
            var result = await paymentSessionsHandler.AuthenticateUpiPaymentTxn(
                accountId: AcountId,
                browserInfo,
                paymentSession: paymentSessionData,
                traceActivityId: new EventTraceActivity());

            // Assert
            Assert.IsTrue(result.PaymentSession.IsChallengeRequired);
            Assert.AreEqual(expectedStatus, result.PaymentSession.ChallengeStatus);
        }

        [TestMethod]
        [DataRow(false, false, "AccountPINotFound")]
        [DataRow(false, false, "Exception Occured")]
        [DataRow(true, false, null)]
        [DataRow(true, true, null)]
        public async Task HandlePaymentChallenge_IndiaUPI_AuthenticateCallTest_ThrowsException(bool accountPIFound, bool piRetunedAsNull, string errorCode)
        {
            // Arrange
            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            mockPayerAuthServiceAccessor.Setup(x => x.Authenticate(
            It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationRequest>(),
            It.IsAny<EventTraceActivity>()))
            .Returns(Task.FromResult(new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationResponse()
            {
                EnrollmentStatus = PaymentInstrumentEnrollmentStatus.Bypassed,
                EnrollmentType = PaymentInstrumentEnrollmentType.ThreeDs,
            }));

            if (accountPIFound)
            {
                var challengeRequiredPi = new PIMSModel.PaymentInstrument()
                {
                    PaymentInstrumentDetails = new PIMSModel.PaymentInstrumentDetails(),
                    PaymentMethod = new PIMSModel.PaymentMethod()
                    {
                        PaymentMethodFamily = "credit_card",
                        PaymentMethodType = "visa"
                    }
                };

                if (piRetunedAsNull)
                {
                    mockPims.Setup(x => x.GetPaymentInstrument(
                       AcountId,
                       ChallengeRequiredPiId,
                       It.IsAny<EventTraceActivity>(),
                       It.IsAny<string>(),
                       It.IsAny<string>(),
                       It.IsAny<string>(),
                       It.IsAny<List<string>>(),
                       It.IsAny<PaymentExperienceSetting>()))
                       .Returns(Task.FromResult<PIMSModel.PaymentInstrument>(null));
                }
                else
                {
                    mockPims.Setup(x => x.GetPaymentInstrument(
                      AcountId,
                      ChallengeRequiredPiId,
                      It.IsAny<EventTraceActivity>(),
                      It.IsAny<string>(),
                      It.IsAny<string>(),
                      It.IsAny<string>(),
                      It.IsAny<List<string>>(),
                      It.IsAny<PaymentExperienceSetting>()))
                      .Returns(Task.FromResult(challengeRequiredPi));
                }
            }
            else
            {
                ServiceErrorResponseException ex = new ServiceErrorResponseException();
                ex.Error = new ServiceErrorResponse();
                ex.Error.ErrorCode = errorCode;
                mockPims.Setup(x => x.GetPaymentInstrument(
                   AcountId,
                   ChallengeRequiredPiId,
                   It.IsAny<EventTraceActivity>(),
                   It.IsAny<string>(),
                   It.IsAny<string>(),
                   It.IsAny<string>(),
                   It.IsAny<List<string>>(),
                   It.IsAny<PaymentExperienceSetting>()))
                   .Throws(ex);
            }

            PaymentSession paymentSessionData = new PaymentSession()
            {
                Language = "en",
                Amount = 10.0m,
                Currency = "INR",
                Partner = "webblends",
                Country = "in",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.RecurringTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                BillableAccountId = "blah+",
                ClassicProduct = "fooBar",
                PaymentInstrumentId = ChallengeRequiredPiId,
                IsChallengeRequired = true
            };

            BrowserInfo browserInfo = new BrowserInfo
            {
                ChallengeWindowSize = ChallengeWindowSize.Five
            };

            // Act
            try
            {
                var result = await paymentSessionsHandler.AuthenticateUpiPaymentTxn(
                    accountId: AcountId,
                    browserInfo,
                    paymentSession: paymentSessionData,
                    traceActivityId: new EventTraceActivity());
            }
            catch (Exception ex)
            {
                // Assert
                if (accountPIFound)
                {
                    ValidationException validationException = ex as ValidationException;
                    Assert.AreEqual(validationException.Message, "Provided Payment Instrument is not a valid UPI PI.");
                    Assert.AreEqual(validationException.ErrorCode, ErrorCode.InvalidPaymentInstrumentDetails);
                }
                else if (errorCode == "AccountPINotFound")
                {
                    ValidationException validationException = ex as ValidationException;
                    Assert.AreEqual(validationException.Message, "Caller is not authorized to access specified PaymentInstrumentId.");
                    Assert.AreEqual(validationException.ErrorCode, ErrorCode.PaymentInstrumentNotFound);
                }
                else
                {
                    Assert.AreEqual(ex.GetType(), typeof(ServiceErrorResponseException));
                }
            }
        }

        [TestMethod]

        // V11 is the default (in PaymentSessionHandler code) and nothing newer is flighted
        [DataRow("V11", 1, "")]
        [DataRow("V11", 1, "FlightName1")]

        // Newer version V65 flighted
        [DataRow("V65", 1, "FlightName1, PXPSD2SettingVersionV65")]

        // When multiple newer versions are flighted, the latest one is applicable
        [DataRow("V65", 1, "FlightName1, PXPSD2SettingVersionV64,  PXPSD2SettingVersionV65")]
        public async Task Authenticate_AllowOnlyDefaultOrFlightedSettingsVersion(
            string requestSettingsVersion,
            int tryCount,
            string flightsStr)
        {
            // Arrange
            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            var authRequest = new AuthenticationRequest()
            {
                SettingsVersion = requestSettingsVersion,
                SettingsVersionTryCount = (ushort)tryCount
            };
            var flights = flightsStr.Split(',').ToList();

            Common.TestContext tc = new Common.TestContext(
                      contact: "px.azure.cot",
                      retention: DateTime.MaxValue,
                      scenarios: "px-service-psd2-e2e-emulator, px.pims.3ds, px.payerauth.psd2.challenge.success");

            // Act
            var result = await paymentSessionsHandler.Authenticate(AcountId, SessionId, authRequest, flights, new EventTraceActivity(), tc);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [DataRow("V2", 1, "FlightName1, PXPSD2SettingVersionV3, FlightName2", "V3")]
        [DataRow("V2", 1, "FlightName1, PXPSD2SettingVersionV7, FlightName2", "V7")]
        [DataRow("V2", 1, "FlightName1, PXPSD2SettingVersionV8, PXPSD2SettingVersionV3, FlightName2", "V8")]
        [DataRow("V3", 1, "", "V11")]
        [DataRow("V3", 1, "FlightName1, FlightName2", "V11")]
        [DataRow("V3", 1, "FlightName1, PXPSD2SettingVersionV7, FlightName2", "V7")]
        [DataRow("V7", 1, "FlightName1, PXPSD2SettingVersionV3, FlightName2", "V3")]
        [DataRow("V7", 1, "FlightName1, PXPSD2SettingVersionV3, FlightName2, PXPSD2SettingVersionV7Dev", "V3")]
        [DataRow("V7", 1, "FlightName1, PXPSD2SettingVersionV3, FlightName2, PXPXPSD2SettingVersionV77", "V3")]
        public async Task Authenticate_ThrowsValiationException_OnSettingsVersionMismatch(
            string requestSettingsVersion,
            int tryCount,
            string flightsStr,
            string expectedTargetSettingsVersion)
        {
            // Arrange
            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            var authRequest = new AuthenticationRequest()
            {
                SettingsVersion = requestSettingsVersion,
                SettingsVersionTryCount = (ushort)tryCount
            };
            var flights = flightsStr.Split(',').ToList();

            Common.TestContext tc = new Common.TestContext(
                      contact: "px.azure.cot",
                      retention: DateTime.MaxValue,
                      scenarios: "px-service-psd2-e2e-emulator, px.pims.3ds, px.payerauth.psd2.challenge.success");

            // Act
            // Assert
            try
            {
                var result = await paymentSessionsHandler.Authenticate(AcountId, SessionId, authRequest, flights, new EventTraceActivity(), tc);
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(expectedTargetSettingsVersion, ex.Target);
                Assert.AreEqual("SettingsVersionMismatch", ex.ErrorCode.ToString());
            }
            catch (Exception ex)
            {
                Assert.Fail($"Expected exception is ValidationException, but different exception was thrown. Details {ex.Message}");
            }
        }

        [TestMethod]
        public void CreatePaymentSession_ConstructPaymentSession_EmptyPiCid()
        {
            // Arrange
            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                Language = "en-US",
                Amount = 0,
                Currency = "USD",
                Partner = "commercialstores",
                Country = "us",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.RecurringTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Two,
                IsMOTO = false,
                PaymentInstrumentId = ChallengeRequiredPiId,
                PaymentInstrumentAccountId = string.Empty
            };

            // Act
            Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentSessionData payerAuthPaymentSessionData = new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentSessionData(
                "sampleId",
                paymentSessionData,
                new PIMSModel.PaymentInstrument { PaymentMethod = new PIMSModel.PaymentMethod { PaymentMethodFamily = "credit_card", PaymentMethodType = "visa" } },
                DeviceChannel.Browser,
                true);

            // Assert
            Assert.AreEqual("sampleId", payerAuthPaymentSessionData.PaymentInstrumentAccountId);
        }

        [TestMethod]
        public void CreatePaymentSession_ConstructPaymentSession_NonEmptyPiCid()
        {
            // Arrange
            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                Language = "en-US",
                Amount = 0,
                Currency = "USD",
                Partner = "commercialstores",
                Country = "us",
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.RecurringTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Two,
                IsMOTO = false,
                PaymentInstrumentId = ChallengeRequiredPiId,
                PaymentInstrumentAccountId = "sampleId"
            };

            // Act
            Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentSessionData payerAuthPaymentSessionData = new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentSessionData(
                "AnotherId",
                paymentSessionData,
                new PIMSModel.PaymentInstrument { PaymentMethod = new PIMSModel.PaymentMethod { PaymentMethodFamily = "credit_card", PaymentMethodType = "visa" } },
                DeviceChannel.Browser,
                true);

            // Assert
            Assert.AreEqual("AnotherId", payerAuthPaymentSessionData.PaymentInstrumentAccountId);
        }

        private const string AcsSignedContentVisa = "eyJ4NWMiOlsiTUlJRTZ6Q0NBOU9nQXdJQkFnSVVkUUs3SUZUVDFWUEwwcVp2d1h5UTBySytGVHN3RFFZSktvWklodmNOQVFFTEJRQXdjVEVMTUFrR0ExVUVCaE1DVlZNeERUQUxCZ05WQkFvVEJGWkpVMEV4THpBdEJnTlZCQXNUSmxacGMyRWdTVzUwWlhKdVlYUnBiMjVoYkNCVFpYSjJhV05sSUVGemMyOWphV0YwYVc5dU1TSXdJQVlEVlFRREV4bFdhWE5oSUdWRGIyMXRaWEpqWlNCSmMzTjFhVzVuSUVOQk1CNFhEVEl4TURVd05qQTFNekEwTkZvWERUSXlNRFV3TmpBMU16QTBORm93Z1pzeER6QU5CZ05WQkFjTUJrSmxlbTl1Y3pFV01CUUdBMVVFQ0F3TlNXeGxMV1JsTFVaeVlXNWpaVEVMTUFrR0ExVUVCaE1DUmxJeEdEQVdCZ05WQkFvTUQwVnhkV1Z1YzFkdmNteGtiR2x1WlRFY01Cb0dBMVVFQ3d3VFJYRjFaVzV6VjI5eWJHUnNhVzVsSUVGRFV6RXJNQ2tHQTFVRUF3d2ljSEp2WkM1aFkzTXpaSE15TG5acGMyRXVjMmxuYmk1M2JIQXRZV056TG1OdmJUQ0NBU0l3RFFZSktvWklodmNOQVFFQkJRQURnZ0VQQURDQ0FRb0NnZ0VCQU82SlUrcmxwcjlaS05aeURIWTRDMWtKbWYzQjRSUjdFNWxQVGRCQXcxa2N1bWFpcE52OWJmUkN1aGJMcURpRERlZGFlNkxXNi9UeHZiR3pGenFUSUtndzZFRkFRRDltVkhVNDdRZEZNb1huelRjc3FIbFFkQVFFakdoZVRJZ2puREU3STBUZFczT3FVTWc3SkRzc1hwMllrcWFJSCtGSDF4N016cWMzbFcxQWN0L1AzZGZCS2dHNXlYakhXM0U5S2dJSXRrbWtaMDdHUklwbE1qbU03S2o2YVFKZWowYnRHanU2V2MyYmFmbWlGbzFldk92SlBkYlVnbm5Pek84TjN3TGQ3SVRJZmJZblJzS291MEhKOHpoZlpDYzlRS2UyMVlvZDErY2hHSldUTlBVeU9rWUZVaTJ2UTg3by9vUkFxOEM5emRUL2YzUlZQemF1S2pBTmxDMENBd0VBQWFPQ0FVNHdnZ0ZLTUF3R0ExVWRFd0VCL3dRQ01BQXdId1lEVlIwakJCZ3dGb0FVMzhNcVZTNHZRanA2UUoyU29QZEhlZGVQaDRvd1pRWUlLd1lCQlFVSEFRRUVXVEJYTUM0R0NDc0dBUVVGQnpBQ2hpSm9kSFJ3T2k4dlpXNXliMnhzTG5acGMyRmpZUzVqYjIwdlpXTnZiVzB1WTJWeU1DVUdDQ3NHQVFVRkJ6QUJoaGxvZEhSd09pOHZiMk56Y0M1MmFYTmhMbU52YlM5dlkzTndNRGtHQTFVZElBUXlNREF3TGdZRlo0RURBUUV3SlRBakJnZ3JCZ0VGQlFjQ0FSWVhhSFIwY0RvdkwzZDNkeTUyYVhOaExtTnZiUzl3YTJrd0V3WURWUjBsQkF3d0NnWUlLd1lCQlFVSEF3SXdNd1lEVlIwZkJDd3dLakFvb0NhZ0pJWWlhSFIwY0RvdkwwVnVjbTlzYkM1MmFYTmhZMkV1WTI5dEwyVkRiMjF0TG1OeWJEQWRCZ05WSFE0RUZnUVVaYjVRQnF2ekY2SW8xeS9PTXIyOG9LOG55Wll3RGdZRFZSMFBBUUgvQkFRREFnZUFNQTBHQ1NxR1NJYjNEUUVCQ3dVQUE0SUJBUUFyRExSUkE2bXY1UUN5UzRNaUVheDVuL1BuWHQ1aWsyb2ZIZmpWdllrUHd6ZUJMbjlwOUp1am0yV2JZdEFKWll6LzFISXN4Y1EyRGNyTG05NW13WnFmVXdKQnJrR2k4UFhCeFIvYk9oaXduRVpFL0tEbzlHZ0dGR2xRdWVmbnU1VXhVRU5FVWtFSE90UGFCTUlrSStsa25mNzUrdEVwNDhXU2ExaUR5WmVSUk9neFVmOWJmU21XOFd3RkVBSzZIa1ZMWWZ5YkpFZ2FBNzM1MHR1OTJsb0xVam1LZTlCeHpQdExBV3pOalJQMXQya1JiUXZIdmgzY0VURXZabUVoRlNUSGdjSkxrUm9ZbFBiTlBCRmNNWERXdFpuUUtjNGUySnhHbnNLZ2NWVk1HNmxMZjFvVElhNk9pWVBNT1p3NjJQMnJQbHZpNUdrOGhDR2NxeW4vQXpVKyIsIk1JSUZHekNDQkFPZ0F3SUJBZ0lSQU5oMFlUQkIvRHhFb0x6R1hXdzI4UkF3RFFZSktvWklodmNOQVFFTEJRQXdhekVMTUFrR0ExVUVCaE1DVlZNeERUQUxCZ05WQkFvVEJGWkpVMEV4THpBdEJnTlZCQXNUSmxacGMyRWdTVzUwWlhKdVlYUnBiMjVoYkNCVFpYSjJhV05sSUVGemMyOWphV0YwYVc5dU1Sd3dHZ1lEVlFRREV4TldhWE5oSUdWRGIyMXRaWEpqWlNCU2IyOTBNQjRYRFRFMU1EWXlOREUxTWpjd05sb1hEVEl5TURZeU1qQXdNVFl3TjFvd2NURUxNQWtHQTFVRUJoTUNWVk14RFRBTEJnTlZCQW9UQkZaSlUwRXhMekF0QmdOVkJBc1RKbFpwYzJFZ1NXNTBaWEp1WVhScGIyNWhiQ0JUWlhKMmFXTmxJRUZ6YzI5amFXRjBhVzl1TVNJd0lBWURWUVFERXhsV2FYTmhJR1ZEYjIxdFpYSmpaU0JKYzNOMWFXNW5JRU5CTUlJQklqQU5CZ2txaGtpRzl3MEJBUUVGQUFPQ0FROEFNSUlCQ2dLQ0FRRUFya21DNTBRK0drbVF5WjI5a0t4cDFkK25KNDNKd1hoR1o3YUZGMVBpTTVTbENFU1EyMnFWL2xCQTN3SFlZUDhpMTcvR1FRWU5CaUYzdTRyNmp1WElIRndqd3ZLeUZNRjZrbUJZWHZjUWE4UGQ3NUZDMW4zZmZJcmhFaitsZGJteGlkekswaFBmWXlYRVpxRHBIaGt1bm12RDdxejFCRVdLRTdOVVlWRlJFZm9wVmlmbEtpVlpjWXJIaTdDSkFlQk5ZN2R5Z3ZtSU1uSFVlSDROdERTNXFmL245RFFRZmZWeW41aEpXaTVQZUI4N25UbHR5OHpkamkydGo3bkEyK1kzUExLUkpVM3kxSWJjaHFHbG5YcXhhYUtma1RMTnNpWnE5UFR3S2FyeUgrdW0zdFhmNXU0bXVselJHT1doMlUrVWs0TG50bU1GQ2IvTHFKa1duVVZlK3dJREFRQUJvNElCc2pDQ0FhNHdId1lEVlIwakJCZ3dGb0FVRlRpRER6OHNQM0F6SHMxRy9nZU1JT0RYdzdjd0VnWURWUjBUQVFIL0JBZ3dCZ0VCL3dJQkFEQTVCZ05WSFNBRU1qQXdNQzRHQldlQkF3RUJNQ1V3SXdZSUt3WUJCUVVIQWdFV0YyaDBkSEE2THk5M2QzY3VkbWx6WVM1amIyMHZjR3RwTUlJQkN3WURWUjBmQklJQkFqQ0IvekEyb0RTZ01vWXdhSFIwY0RvdkwwVnVjbTlzYkM1MmFYTmhZMkV1WTI5dEwxWnBjMkZEUVdWRGIyMXRaWEpqWlZKdmIzUXVZM0pzTUR5Z09xQTRoalpvZEhSd09pOHZkM2QzTG1sdWRHd3VkbWx6WVdOaExtTnZiUzlqY213dlZtbHpZVU5CWlVOdmJXMWxjbU5sVW05dmRDNWpjbXd3Z1lhZ2dZT2dnWUNHZm14a1lYQTZMeTlGYm5KdmJHd3VkbWx6WVdOaExtTnZiVG96T0RrdlkyNDlWbWx6WVNCbFEyOXRiV1Z5WTJVZ1VtOXZkQ3h2UFZaSlUwRXNiM1U5Vm1sellTQkpiblJsY201aGRHbHZibUZzSUZObGNuWnBZMlVnUVhOemIyTnBZWFJwYjI0L1kyVnlkR2xtYVdOaGRHVlNaWFp2WTJGMGFXOXVUR2x6ZERBT0JnTlZIUThCQWY4RUJBTUNBUVl3SFFZRFZSME9CQllFRk4vREtsVXVMMEk2ZWtDZGtxRDNSM25YajRlS01BMEdDU3FHU0liM0RRRUJDd1VBQTRJQkFRQjlZK0Y5OXRoSEFPaHhab1FjVDlDYkNvblZDdGJtM2hXbGYybkJKbnVhUWVvZnRkT0tXdGowWU9UajdQVWFLT1dmd2NiWlNIQjYzck1tTGlWbTdacUlWbmRXeHZCQlJMMVRjZ2J3YWdEbkxnQXJRTUtIblkydUdRZlBqRU1Ba0FubldlWUpmZCtjUkpWbzZLM1I0QmJRR3pGU0hhMmkyYXI2L29YeklOeWF4QVhkb0cwNEN6MlAwUG02MTNoTUNwakZ5WWlsUy80MjVoZTFUay92SHNUbkZ3RmxrOXlZMkw4VmhCYTZqNDBmYWFGdS82ZmluNzhLb3BrOTZnSGRBSU4xdGJBMTJOTm1yN2JRMXBVczBuS0hoelFHb1JYZ3VZZDdVWU85aTJzTlZDMUM1QTNGOGRvcHdzdjJRSzIrMzNxMDVPMi80RGduRjRtNXVzNlJWOTREIiwiTUlJRG9qQ0NBb3FnQXdJQkFnSVFFNFkxVFIwL0J2TEIrV1VGMVpBY1lqQU5CZ2txaGtpRzl3MEJBUVVGQURCck1Rc3dDUVlEVlFRR0V3SlZVekVOTUFzR0ExVUVDaE1FVmtsVFFURXZNQzBHQTFVRUN4TW1WbWx6WVNCSmJuUmxjbTVoZEdsdmJtRnNJRk5sY25acFkyVWdRWE56YjJOcFlYUnBiMjR4SERBYUJnTlZCQU1URTFacGMyRWdaVU52YlcxbGNtTmxJRkp2YjNRd0hoY05NREl3TmpJMk1ESXhPRE0yV2hjTk1qSXdOakkwTURBeE5qRXlXakJyTVFzd0NRWURWUVFHRXdKVlV6RU5NQXNHQTFVRUNoTUVWa2xUUVRFdk1DMEdBMVVFQ3hNbVZtbHpZU0JKYm5SbGNtNWhkR2x2Ym1Gc0lGTmxjblpwWTJVZ1FYTnpiMk5wWVhScGIyNHhIREFhQmdOVkJBTVRFMVpwYzJFZ1pVTnZiVzFsY21ObElGSnZiM1F3Z2dFaU1BMEdDU3FHU0liM0RRRUJBUVVBQTRJQkR3QXdnZ0VLQW9JQkFRQ3ZWOTVXSG02aDJtQ3hsQ2ZMRjlzSFA0Q0ZUOGljdHREMGIwL1BtZGpoMjhKSVhEcXNPVFBISDJxTEpqMHJOZlZJc1pIQkFrNEVscEY3c0RQd3NSUk9FVysxUUs4YlJhVks3MzYyclBLZ0gxZy9Fa1pnUEkyaDRIM1BWejR6SHZ0SDhhb1Zsd2RWWnFXMUxTN1lnRm15cHcyM1J1d2hZLzgxcTZVQ3p5cjBUUDU3OVpSZGhFMm84bUNQMnc0bFBKOXpjYytVMzBycTI5OXlPSXp6bHIzeEY3elN1anRGV3NhbjlzWVhpd0dkL0Jtb0tvTVd1RHBJL2s0K29Lc0dHZWxUODRBVEIrMHR2ejhLUEZVZ09Td3NBR2wwbFVxOElMS3BlZVVZaVpHbzNCeE43N3QrTnd0ZC9qbWxpRktNQUd6c0dIeEJ2ZmFMZFhlNllKMkU1LzR0QWdNQkFBR2pRakJBTUE4R0ExVWRFd0VCL3dRRk1BTUJBZjh3RGdZRFZSMFBBUUgvQkFRREFnRUdNQjBHQTFVZERnUVdCQlFWT0lNUFB5dy9jRE1lelViK0I0d2c0TmZEdHpBTkJna3Foa2lHOXcwQkFRVUZBQU9DQVFFQVgvRkJmWHhjQ0xrcjROV1NSL3BuWEtVVHd3TWhteXRNaVViUFdVM0ovcVZBdG1QTjNYRW9sV2NSekNTczAwUnNjYTRCSUdzRG9vOFl0eWs2ZmVVV1lGTjRQTUN2RllQM2oxSXpKTDFrazVmdWkvZmJHS2h0Y2JQM0xCZlFkQ1ZwOS81clBKUytUVXRCakU3aWM5RGprQ0p6UTgzejcrcHp6a1dLc0taSi8weDluWEdJeEhZZGtGc2Q3djNNOSs3OVlLV3hlaFp4MFJiUWZCSThiR21YMjY1Zk9acHdMd1U4R1VZRW1TQTIwR0J1WVFhN0ZrS01jUGN3KytEYlpxTUFBYjNtTE5xUlg2QkdpMDFxbkQwOTNRVkcvbmEvb0FvODVBRG1KN2YvaEMzZXVpSW5saEJ4NnlMdDM5OHpuTS9qcmE2TzFJN21UMUd2RnBMZ1hQWUhEdz09Il0sImFsZyI6IlBTMjU2In0.eyJhY3NFcGhlbVB1YktleSI6eyJrdHkiOiJFQyIsIngiOiJLZEdpNjRsMEh0Z0NJeFFUdDhKQ0J6a0VGdFVKT1NPa2VKM3FTRk8ydGxNIiwieSI6IjFhMFU3eDNFOWZwWGV1WmxXUUlIR3BlRXBDcFpGMmZzTk5mMl9tSGs3TmMiLCJjcnYiOiJQLTI1NiJ9LCJzZGtFcGhlbVB1YktleSI6eyJrdHkiOiJFQyIsIngiOiJxYkZLTEVaaUNyS0Z0aUdGUWEydlBFSnJxTXhfeE44OFdKOUpQdzFrbUMwIiwieSI6IkFmUVZCeFpiUFk4TnNOZV9fcEw3VnVZbzVHcU5IUjJHQjd1ZmZPQ0xnX1UiLCJjcnYiOiJQLTI1NiJ9LCJhY3NVUkwiOiJodHRwczovL25hdGl4aXNwYXltZW50c29sdXRpb25zLTNkcy12ZG0ud2xwLWFjcy5jb20vYWNzLWNoYWxsZW5nZS1hcHAtc2VydmljZS9jaGFsbGVuZ2VBcHAvY2hhbGxlbmdlUmVxdWVzdC9hcHBCYXNlIn0.hOFQ9A55mlNYYOkdrHS_L8zUYrFkEOibV9i15ZbbF4heKbsEUi2A5e6yEf81HvXDAFQf_-olJISwgobTHD9CGfOQTiOPomy5Efvw7mZ052iAzXrOgwojOfyGcqRF8RK4c-h7JB2z5f66hBqNRKEyQNyQamQv7j47PVSUkWQepg5RoASzABlZdXJhjJzuITgk32Zfez58AjVjTA2myeWGREU6I0Ac1zvRVAASDwUW3VuRGdLVu7jhbaY_Uvto3ukYial0vb6ZPlej2Rmi0baJRIAJSJ-K8ymvqbB8b2ngwUrZVqUC27pAXO57cFBAigJr-x1pRzU7A8gcj0APiGgZVg";
        private const string AcsSignedContentEmpty = "";
        private const string AcsSignedContentBadFormat = "eyJ4NWMiOlsiTUlJRTZ6Q0NBOU9nQXdJQkFnSVVkUUs3SUZUVDFWUEwwcVp2d1h5UTBySytGVHN3RFFZSktvWklodmNOQVFFTEJRQXdjVEVMTUFrR0ExVUVCaE1DVlZ.tvWklodmNOQVFFTEJRQXdjVEVMTUFrR0ExVUVCaE1DVlZNeERUQUxCZ05WQkFvVEJGWkpVMEV4THpBdEJnTlZCQXNUSmxacGMyRWdTVzUwWlhKdVlYUn";
        private const string AcsSignedContentVisaEmulatorWithHeader = "eyJhbGciOiJQUzI1NiIsIng1YyI6WyJNSUlFQWpDQ0F1cWdBd0lCQWdJSkFMYkN5N0MzeEIybk1BMEdDU3FHU0liM0RRRUJDd1VBTUlHVk1Rc3dDUVlEVlFRR0V3SlZVekVMTUFrR0ExVUVDQXdDVjBFeEVEQU9CZ05WQkFjTUIxSmxaRzF2Ym1ReEhqQWNCZ05WQkFvTUZVMXBZM0p2YzI5bWRDQkRiM0p3YjNKaGRHbHZiakVpTUNBR0ExVUVBd3daWVdOelpXMTFiR0YwYjNJdWJXbGpjbTl6YjJaMExtTnZiVEVqTUNFR0NTcUdTSWIzRFFFSkFSWVVjSGhrWlhaelFHMXBZM0p2YzI5bWRDNWpiMjB3SGhjTk1qRXdNekF5TWpNek56VTVXaGNOTXpFd01qSTRNak16TnpVNVdqQ0JsVEVMTUFrR0ExVUVCaE1DVlZNeEN6QUpCZ05WQkFnTUFsZEJNUkF3RGdZRFZRUUhEQWRTWldSdGIyNWtNUjR3SEFZRFZRUUtEQlZOYVdOeWIzTnZablFnUTI5eWNHOXlZWFJwYjI0eElqQWdCZ05WQkFNTUdXRmpjMlZ0ZFd4aGRHOXlMbTFwWTNKdmMyOW1kQzVqYjIweEl6QWhCZ2txaGtpRzl3MEJDUUVXRkhCNFpHVjJjMEJ0YVdOeWIzTnZablF1WTI5dE1JSUJJakFOQmdrcWhraUc5dzBCQVFFRkFBT0NBUThBTUlJQkNnS0NBUUVBeE5hdUdoRVZPSGFUaGJlMkV1d0dTK3hrVzYwdmkrd1Y4K2ljRktFcFljd01Dbk5WemJNNkM2YVF1b1RZS3VrVkQ3bDR3MXRtbjg2bGdYQWpaMG9YaDR3OXdiNzZmVWFadW5sRkNndXNDQ1NtMWYyaU9lbXdVbVQyWTROb2hha21IU20xcVBpVWtraTY4L0FlSlJqVWRjVzY4c3BvcGhXVHNiNndzWDJGWjkwUHV0VkVOSk5oSUhwek82bHpnclRzME1DbkhSdnoxbkVsNEtFQ040eXBPQVJGS2VUSWdROWR2ZE9LRDRVM3czNWVQTzJjaFdDdytkZ1hBR0lkaDVpSkVFVTNrZlBxV1JQK0N4eW0xYUlTRVZ5RFhCc3JKUGNkaU1wQUtYanBPTi9hOXJTVURDejM5b3hnWURvQW11RnJDbms1VEEzaEtnb3hPd3VjT3hNN0FRSURBUUFCbzFNd1VUQWRCZ05WSFE0RUZnUVV3MEl6VDd2emRhTXJoZCtqWmJHcmpkNEpibWt3SHdZRFZSMGpCQmd3Rm9BVXcwSXpUN3Z6ZGFNcmhkK2paYkdyamQ0SmJta3dEd1lEVlIwVEFRSC9CQVV3QXdFQi96QU5CZ2txaGtpRzl3MEJBUXNGQUFPQ0FRRUFpM1l3T2M5cEtlQm1oa3VXS0ZXSnJCelFWMWtMdVU2ZTd1bnJVa2tUK0xTSXhnVERrc3orUGtLYk12eGhoVitMSVdLNWJXY2hqdlVqMk9tQUwwcFZ2ZWNxRU9Vazk0aW0zWTRSOXdGeGFDNjFrTE5xWXViRHpIMytGejNIQW9vYWRQWmpVQ3hGOWdkdTNHbnBySW1KQTlmUjR3UnZ5VGhMTTRvNzgzVDRIL2xiV3Y4TzRNSFFkdjQzdGxNWjZLTDFPRlJYNUNiNytjTG5HbXgvZ1pKNUc3Tm1MVWpEQzlEL1BBQWhKRlQzcXI0VS9YZ1BDUWlyd2NyRmdpZkdTMWdoOHVIYWhRenBIR1liWlBwVC9udDdVTnVUazZZTHRsUkNtWDlRaUw0dFNDVjRTSWVXN2JOM1lxT204b0lMTmJ6bVNxWjZGdVpHNlVvM2w2aWVZN1BVM2c9PVxuIl19.eyJhY3NFcGhlbVB1YktleSI6eyJrdHkiOiJFQyIsImNydiI6IlAtMjU2IiwieCI6IlljbG5QaldsMmt2NFRrWm5GdHZjOEg2dUJ2cWkybEtLZ1lHLTQ3cVY5YXMiLCJ5IjoiZjNXUFJoUG9PSUwyaTllUDN0SF83RFhNSnF1SU9RZXB3RmlyTGd6djk2YyJ9LCJzZGtFcGhlbVB1YktleSI6eyJrdHkiOiJFQyIsImNydiI6IlAtMjU2IiwieCI6IkFyYkd0UWhNWGhEbEpSNnQ0bnB5MEdXM2xsN1BaMWNCb000Wi13Q1dSYk0iLCJ5IjoibEhVam93c1Bick5TTWk2MXAwWEphQnBWVmpjNlBBaE1XR0RwSkI3YnBnYyJ9LCJhY3NVUkwiOiJodHRwczovL3BheW1lbnRzLWFjcy1lbXVsYXRvci5henVyZXdlYnNpdGVzLm5ldC9hY3Mvc2RrL2NoYWxsZW5nZSJ9.rrx5oT89CHJQ_zXISyp5VRx0iJGdGRLco7GMHYol78zcsEEq8XSNazU-JkfDEQGE-ZC0tfodkW0rWPa00FagXkD6zgm-0TPDA4-NbwvHoIZ2gB6mtboSiu0xcvzQFcm_wjyq71y5plsgPKnxFKxOtDNbZFD_Lt6XtfeFxL4V_YVD-L971tESxRehRO8p3UTDirJUMFYBtgGBEK7lb7SHUydjH-qYdcHI8z1BMEKZHSNfX2kxJYpW_IH9VFv_rRxoFqs-tITWd7nAuyi96fvA2-L6EcUO1KNC1a9kwzdOkrvJvU6XSM5YAhbG2JKruSs_9t1CoU5jhvEqLPuEYpRsFw";
        private const string AcsSignedContentVisaEmulatorWithoutHeader = ".eyJhY3NFcGhlbVB1YktleSI6eyJrdHkiOiJFQyIsImNydiI6IlAtMjU2IiwieCI6IlljbG5QaldsMmt2NFRrWm5GdHZjOEg2dUJ2cWkybEtLZ1lHLTQ3cVY5YXMiLCJ5IjoiZjNXUFJoUG9PSUwyaTllUDN0SF83RFhNSnF1SU9RZXB3RmlyTGd6djk2YyJ9LCJzZGtFcGhlbVB1YktleSI6eyJrdHkiOiJFQyIsImNydiI6IlAtMjU2IiwieCI6IkFyYkd0UWhNWGhEbEpSNnQ0bnB5MEdXM2xsN1BaMWNCb000Wi13Q1dSYk0iLCJ5IjoibEhVam93c1Bick5TTWk2MXAwWEphQnBWVmpjNlBBaE1XR0RwSkI3YnBnYyJ9LCJhY3NVUkwiOiJodHRwczovL3BheW1lbnRzLWFjcy1lbXVsYXRvci5henVyZXdlYnNpdGVzLm5ldC9hY3Mvc2RrL2NoYWxsZW5nZSJ9.rrx5oT89CHJQ_zXISyp5VRx0iJGdGRLco7GMHYol78zcsEEq8XSNazU-JkfDEQGE-ZC0tfodkW0rWPa00FagXkD6zgm-0TPDA4-NbwvHoIZ2gB6mtboSiu0xcvzQFcm_wjyq71y5plsgPKnxFKxOtDNbZFD_Lt6XtfeFxL4V_YVD-L971tESxRehRO8p3UTDirJUMFYBtgGBEK7lb7SHUydjH-qYdcHI8z1BMEKZHSNfX2kxJYpW_IH9VFv_rRxoFqs-tITWd7nAuyi96fvA2-L6EcUO1KNC1a9kwzdOkrvJvU6XSM5YAhbG2JKruSs_9t1CoU5jhvEqLPuEYpRsFw";
        private const string AcsSignedContentMastercard = "eyJhbGciOiJQUzI1NiIsIng1YyI6WyJNSUlGbURDQ0E0Q2dBd0lCQWdJSUR6TUxXbTRHVVdnd0RRWUpLb1pJaHZjTkFRRUxCUUF3ZURFTE1Ba0dBMVVFQmhNQ1ZWTXhFekFSQmdOVkJBb1RDazFoYzNSbGNrTmhjbVF4S0RBbUJnTlZCQXNUSDAxaGMzUmxja05oY21RZ1NXUmxiblJwZEhrZ1EyaGxZMnNnUjJWdUlETXhLakFvQmdOVkJBTVRJVkJTUkNCTllYTjBaWEpEWVhKa0lETkVVeklnU1hOemRXVnlJRk4xWWlCRFFUQWVGdzB4T1RFeE1EVXhNelEyTkRoYUZ3MHlNakV4TURReE16UTJORGhhTUlHSE1Rc3dDUVlEVlFRR0V3SkhRakViTUJrR0ExVUVDaE1TVFc5dWVtOGdRbUZ1YXlCTWFXMXBkR1ZrTVRjd05RWURWUVFMRXk1QlExTk5VeTFCUTFNdFZqSXhNQzFOVDA1YVR5MUNRVTVMTFV4SlRVbFVSUzAyTnpVM05DMVRhV2R1YVc1bk1TSXdJQVlEVlFRREV4bHRZWE4wWlhKallYSmtMbUZqY3pJdWJXOXVlbTh1WTI5dE1JSUJJakFOQmdrcWhraUc5dzBCQVFFRkFBT0NBUThBTUlJQkNnS0NBUUVBdFFLLyt5UFpmSWN4bXlNSVNxUVFvUTJ6VDRCQ2h5L0lTRGNRM0JldmVJdSsxeWZaQWJUZm1DMG9HbkU1NXpBaHdQd1NVMFdOcFpyRy9mYVB6OW9XcjdYRDB3UGViTVRvZ1NFaThlb3VxcVBLcExhYjJLcndZUkthVGdPNFAwMG1RanZYU0pkZFdpczVONlo5TzB5MW41Z2Y0TWI2bHFQS2Rvd2F5QllpOGFiajN2elkrZFVTWDhndlNudU5nSEl1dDhyT1daR1RINzZUQkVUOWFRWnJ5R2F3QWNocDNTSVgwZmFVRno4Y0w2QjlTa3FpS1pjYUlqcjVaYm9CR1lMK3JldnBpcVNTdWZVaEF1M3dOanV3SStoVkRObmFaTDdDLzFIM3hoRk9jakFzY2YxNnJhY29sMmg2RVBhL29wa1lXVUh0di9KWS9aVU16ZGxMVFIrdmFRSURBUUFCbzRJQkZEQ0NBUkF3RGdZRFZSMFBBUUgvQkFRREFnQ0FNQWtHQTFVZEV3UUNNQUF3SFFZRFZSME9CQllFRkZZaWNWMEIrOWlZNktCbGd1Z1lNZVVUa25lNk1COEdBMVVkSXdRWU1CYUFGRS8zdzU5aXlsWlVTOTlJZE10d0hHODgzUkJGTUVnR0NDc0dBUVVGQndFQkJEd3dPakE0QmdnckJnRUZCUWN3QVlZc2FIUjBjRG92TDI5amMzQXVjR3RwTG1sa1pXNTBhWFI1WTJobFkyc3ViV0Z6ZEdWeVkyRnlaQzVqYjIwd2FRWURWUjBmQkdJd1lEQmVvRnlnV29aWWFIUjBjRG92TDJOeWJDNXdhMmt1YVdSbGJuUnBkSGxqYUdWamF5NXRZWE4wWlhKallYSmtMbU52YlM4MFptWTNZek01WmpZeVkyRTFOalUwTkdKa1pqUTROelJqWWpjd01XTTJaak5qWkdReE1EUTFMbU55YkRBTkJna3Foa2lHOXcwQkFRc0ZBQU9DQWdFQU1tb1ZrbnJ6RFRvVXZlQm1RRllrMThUU1Z1UUdmQ0c2Mkh1bjNERWcwcXVxUUpBR3BLTGRyM3daV2JhQ2hTbU5yNkdKQUl2TWxBOXRVWVc5VW5pWmxlN1FMREJXQk1mWnZvNmxxbEtHMjVsRFFHQUxvOEc3cS9iZDlpejBXRzJ3cmFmWDVIQ0lGVUF1WjBncHRtejhtS0sydzhHSVNrU1RsVldJVldIa3NHUXBWQWNvTnQ5ZjZGbWpnZ3RWMXZ6OUx0cGhPMXhsNkV0eVY4K0xGTE5DWkFDVWROcXFnbGtVQ09zaDZCYWM3R2RNelIrMCtSdGhWVUcxSE1LK2t5OEM0SEdqVFJjZGVjWXFJSk1hc0ZvbGU1YWxYZGJ0a0FqZ2dXRy9FRWg3bkpCQ3c3UkYybm5mS2FuY09EZWVwOVZvUnlTc0RtS0FKTlF4VC9SUXJLNTVnQmorQ0huaTRyeHNLV1c1NmdOYVBjL1FaR1F4WWVIdXFITlNFQlhTZ0ZuWkRnVjhCN1RrK0R5eFVseExEb1p0S3JZdE9SSUVXV3ZpcUhaZjJ2L1Y1cVJoV0xmRVBMUXUzM0JwMnVuSmZPeEV6cTNnTXpZakMzQ1RJWFhaSU5tTmwwRXdHNU1EVWoxekJ3MVd2dEJ3Q0J4K016T3gyTit2VklHR1pYOHV6eEJxNE1KaHkvRkpud2hpcEU5NDYyL2NyVVRBZmIvN01yNm5kU3ZFT0czb09LNXJ1RlkzZ1dKdVo5cnFSNGFvM2J1eEFPYXRPRjIxTy9SS1lyTnJvV3FYd0JsT2Q0azRuNk5TMUNzKzRGVml2YXhRZjc5SXdMMWczZ09jaUprdnVpQThPWXU1eFg0bW5Xd0k4U0FOQklzRDQ0V0NBY2t2ZjVTUFNmdFVHaEkxeU9JPSIsIk1JSUduVENDQklXZ0F3SUJBZ0lRQ1dYQWdpVy94UXU2V1FHaTBsSHhLVEFOQmdrcWhraUc5dzBCQVFzRkFEQjhNUXN3Q1FZRFZRUUdFd0pWVXpFVE1CRUdBMVVFQ2hNS1RXRnpkR1Z5UTJGeVpERW9NQ1lHQTFVRUN4TWZUV0Z6ZEdWeVEyRnlaQ0JKWkdWdWRHbDBlU0JEYUdWamF5QkhaVzRnTXpFdU1Dd0dBMVVFQXhNbFVGSkVJRTFoYzNSbGNrTmhjbVFnU1dSbGJuUnBkSGtnUTJobFkyc2dVbTl2ZENCRFFUQWVGdzB4TnpBeE1ERXdOVEF3TURCYUZ3MHlOakEzTVRVd056QXdNREJhTUhneEN6QUpCZ05WQkFZVEFsVlRNUk13RVFZRFZRUUtFd3BOWVhOMFpYSkRZWEprTVNnd0pnWURWUVFMRXg5TllYTjBaWEpEWVhKa0lFbGtaVzUwYVhSNUlFTm9aV05ySUVkbGJpQXpNU293S0FZRFZRUURFeUZRVWtRZ1RXRnpkR1Z5UTJGeVpDQXpSRk15SUVsemMzVmxjaUJUZFdJZ1EwRXdnZ0lpTUEwR0NTcUdTSWIzRFFFQkFRVUFBNElDRHdBd2dnSUtBb0lDQVFDYkRXejU1aDhZaFV0SGpzODN5VHBvRk5iVWdJdm5QY0pUU0VVcGlpQ0xlZGljaDYxRHBRWGY4bkJJZFVGdit6czdqRUtTK2xOY1p4NzVpVXdHSjJtUm9STmlrSmo3bktXd0lQR1NyaHNKZU9HUm53a2c2a2g5UEVFTlJnVXlsNkRNVXNrUFloWjkrS05ldUhydDNIZStzZkJibEsvUlEzSlJzeU5WOTJZdE1QQkFrMk5kZEJ2bWg5L0ZTbnl1MTN3U2hMVWF0eHlzWjY5OE9JWjJicFhzRDJGTzNNajQ5UlhwWFN5TU96Um5XTUIxT0pDY3FaMW5QSmhHM3ZwSDN2YWJUZWhYMmNHTDN4a2FhNklUVzZta3JuZUtqbktvUzNNMzNiUlJlK09nQW5sdW9jWTYrcUpocU1rQ01wRkxRTEdVY25rWGlyRkNLcU1MNU9BSldYUVFnUktGWTJPckRBdWdNUHZzbTR5TlZoRUJvY0h3dzZ1YzY2SnZyRTdUQmt5NkhBOHZJSlgzc1ZVUGwySC81SHZVa1Zoa0VSd21oUzZLajJ4TlZkdzIyUVV6WGNhNXdweGh1NXF6a1lLdUU4ZkJTVE9oNVdLK0FFZnNicWtaMTA3VWc2K3JrTFpJQ3ArL2hiVG5JbTBuem9KanNOTnZXYkpIeGpQMEcyRDM2MlFpNm9PM1VLa2Q5b3N3MjlDZEhTYmtqYVlwcUJFa21DK2MyYkNxckFQcmVMQldkNDFpaXFSZUtKbU1oOHlTLzMySDl4UEJIaWRQcDBsZjcyQ1IyWi9DOXRKUE1nWVVmRVVrYlJ1VDVTUSszMCtUTklZbFpneUx4NXVMWHJ6WEt6eHNBK2FmU25qeFNnbVhVd2dlcTZGSFNMNjlRdUhpRlFyS2dHRElEWDZwcndJREFRQUJvNElCSFRDQ0FSa3dTQVlJS3dZQkJRVUhBUUVFUERBNk1EZ0dDQ3NHQVFVRkJ6QUJoaXhvZEhSd09pOHZiMk56Y0M1d2Eya3VhV1JsYm5ScGRIbGphR1ZqYXk1dFlYTjBaWEpqWVhKa0xtTnZiVEJwQmdOVkhSOEVZakJnTUY2Z1hLQmFobGhvZEhSd09pOHZZM0pzTG5CcmFTNXBaR1Z1ZEdsMGVXTm9aV05yTG0xaGMzUmxjbU5oY21RdVkyOXRMMlEwWVRVMU1XRmhPVEprWVRFeE9HSXhOR00wWm1ZeE1UZGhaVEV4WkdVeE16Rm1PVFJoWWpFdVkzSnNNQTRHQTFVZER3RUIvd1FFQXdJQmhqQVNCZ05WSFJNQkFmOEVDREFHQVFIL0FnRUFNQjBHQTFVZERnUVdCQlJQOThPZllzcFdWRXZmU0hUTGNCeHZQTjBRUlRBZkJnTlZIU01FR0RBV2dCVFVwVkdxa3RvUml4VEUveEY2NFIzaE1mbEtzVEFOQmdrcWhraUc5dzBCQVFzRkFBT0NBZ0VBbTAyQnY2aUkvWmt2c3I2ZEFHL2dpSkNPK3BldkRIODBXR2hhZ3Z2eWVJY2pJd3dmc1NJVWI5dzM1WFJSaENPa3hTblA3bHVZbW5PY29qdlNERW5lVVZiU3NNeDkvenEvUjFCbC9Gd0FJa3N6SkFjZ3hOaDFwLzRIOWpKVFZGRTBUMUpsNkMxWTBvRitYMGZoVXhRcEgxRllCWEE0V1pUKzdSREcvMzNPRDhuZEJUT3dvdWVoL040RXdGWmw4UjRrMUZyZGVhUm5tdXhTb0lJa2J2NklkZFZNM09nVE1OWHJpNGNOUDNIMmpwaTE0QmdoYld5M1YyODI0aG9EV3dmVm83U01iUDFNeG9Yb281SFdwRUVvdWt0UVhYak5EWVl0ZEZZUVdEbWxyNkZoZUFIMVFudXp0d3hkODc4L0M5RHZjdXN1VjhIUHRBRlFWVmpzWG8zZ0ljdG41TkRBK2N6WHZ3ZFljaCtTcVpibTVZdmJNVDlSSll6blpBc1MzZkQvc2RadVRnajMwbVkxMmYrSnVnaExLYXl6ZXR4NU9Pb0dvVHFRUEZuazBKOHIrNGljd1FOS054TnZIVE5oRFZrUFVzb2UvWjA3NmVkdjJyQ1g0dHlWYVhyVlo1MEswNGZWN2swSGdWSWxRellFWWVRODRsbklidU1lTVo3MGJnK0VhTk5OWnVvZGZIdXZVUWtlMi9HUUczRDc1VllNY3dxZEFBR0g2Snc1T2RuRDdNNFcvS2hFbkl6ajljam9aZ050WWJLV0ZOb3VhdDRnUG4zYkdsSW5mWnBya0cyQ0VwSWxJWnAycVZNbFQ4NWZhZ0RGam9pOEhzc2ZRbWJtNEdCT3NDT1A1dE90b1lVamh3ZWE0Mjk3Q09uMm55NndDMjdWRGxjNk9tbkQyVDg9IiwiTUlJRnh6Q0NBNitnQXdJQkFnSVFGc2p5SXVxaHc4MHdOTWpYVTQ3bGZqQU5CZ2txaGtpRzl3MEJBUXNGQURCOE1Rc3dDUVlEVlFRR0V3SlZVekVUTUJFR0ExVUVDaE1LVFdGemRHVnlRMkZ5WkRFb01DWUdBMVVFQ3hNZlRXRnpkR1Z5UTJGeVpDQkpaR1Z1ZEdsMGVTQkRhR1ZqYXlCSFpXNGdNekV1TUN3R0ExVUVBeE1sVUZKRUlFMWhjM1JsY2tOaGNtUWdTV1JsYm5ScGRIa2dRMmhsWTJzZ1VtOXZkQ0JEUVRBZUZ3MHhOakEzTVRRd056STBNREJhRncwek1EQTNNVFV3T0RFd01EQmFNSHd4Q3pBSkJnTlZCQVlUQWxWVE1STXdFUVlEVlFRS0V3cE5ZWE4wWlhKRFlYSmtNU2d3SmdZRFZRUUxFeDlOWVhOMFpYSkRZWEprSUVsa1pXNTBhWFI1SUVOb1pXTnJJRWRsYmlBek1TNHdMQVlEVlFRREV5VlFVa1FnVFdGemRHVnlRMkZ5WkNCSlpHVnVkR2wwZVNCRGFHVmpheUJTYjI5MElFTkJNSUlDSWpBTkJna3Foa2lHOXcwQkFRRUZBQU9DQWc4QU1JSUNDZ0tDQWdFQXhaRjNuQ0VpVDhYRkZhcSszQlBUMGNNRGxXRTc2SUJzZHgyN3czaEx4d1ZMb2c0MlVUYXNJZ3pteXNUS3BCYzE3SEVaeU5BcWs5R3JDSG8wT3lrNEpadVhIb1c4MGdvWmFSMnNNbm40OXl0dDdhR3NFMVBzZlZ1cDhncUFvcmZtM0lGYWIyL0NuaUpKTlhhV1Bnbjk0K1UvbnNvYXFUUTZqKzZKQm9Jd25Ga2xoYlhIZktycWxrVVpKQ1lhV2JaUmlRN25rQU5ZWU0yVGQzTjg3Rm1SYW5tRFhqNUJHNmxjOW8xY2xUQzdVdlJRbU5JTDlPZEREWjhxbHFZMkZpMGV6dEJudW8yRFVTNXRHZFZ5OFNncVBNM0UxMmZ0azRFZGxLeXJXbUJxRmNZd0d4NEFjU0o4OE8zclFtUkJNeHRrMHI1dmhncjZoRENHcTdGSEsvaFFGUDlMaFVPOTFxeFdFdE1uNzZTYTdEUENMYXMrdGZOUlZ3RzEyRkJ1RVpGaGRTL3FLTWRJWVVFNVE2dXdHVEV2VHpnMmttZ0pUM3NOYTZkYmhsWW5ZbjlpSWpUaDBkUEdnaVhhcDFCaGk4QjlhYVBGY0hFSFNxVzhuWlVJTmNyd2Y1QVVpKzdEK3EvQUc1SXRpQnRRVENhYUZtNzRndjUxeXV0endnS25IOVEreDNtdHVLL3V3bExDc2xqOURlWGdPek1XRnhGZ3V1d0xHWDM5a3REbmV0eE53M1BMYWJqSGtEbEdESWZ4ME1DUWFrTTc0c1RjdVc4SUNpSHZOQTdmeFhDbmJ0anN5N2F0L3lYWXdBZCtJRFM1MU1BL2czT1lWTjRNKzBwRzg0M1JlNlo1M29PRHAwWW11Z3gwRk5PMU54VDNITzFoZDdkWHlqQVYvdE4vR0djQ0F3RUFBYU5GTUVNd0RnWURWUjBQQVFIL0JBUURBZ0dHTUJJR0ExVWRFd0VCL3dRSU1BWUJBZjhDQVFFd0hRWURWUjBPQkJZRUZOU2xVYXFTMmhHTEZNVC9FWHJoSGVFeCtVcXhNQTBHQ1NxR1NJYjNEUUVCQ3dVQUE0SUNBUUJMcUlZb3JydFZ6NTZGNldPb0xYOUNjUmpTRmltN2dPODczYTNwNys2Mkk2am9YTXNNcjBuZDluUlBjRXdkdUVsb1pYd0ZnRXJWVVFXYVVaV05wdWUwbUd2VTdCVUFnVjlUdTBKMHlBKzlzcml6Vm9NdngrbzR6VEozVnU1cDVhVGYxYVlvSDF4WVZvNW9vRmdsL2hJL0VYRDJsby94T1VmUEtYQlk3dHdmaXFPemlRbVRHQnVxUFJxOGgzZFFSbFhZeFgvcnpHZjgwU2VjSVQ2d285S2F2RGtqT21KV0d6ekhzbjZSeW82TUVDbE1hUG4wdGU4N3VrTk43NDBBZFBoVHZOZVpkV2x3eXFXQUpwc3YyNGNhRWNralNwZ3BvSVpPamM3UEFjRVZRT1dGU3hVZXNNazRKejViVlphL0FCanpjcCtyc3ExUUxTSjVxdXFId1dGVGV3Q2h3cHc1Z3B3K0U1U3BLWTZGSUhQbFRkbCtxSFRodk44bHNLTkFRZzBxVGRFYklGWkNVUUMwQ2wzVGkzcS9jWHY4dGd1TEpOV3ZkR3pCNjAwWTMyUUhjbE1wZXlhYlQ0L1FlT2VzcXB4NkRhNzBKMkt2TFQxajZDaDJCc0tTemVWTGFocmpub1ByZGdpSVlZQk9nZUEzVDhTRTFwZ2FndDU2UjduSWtSUWJ0ZXNvUktpK05mQzdwUGIvRzFWVXNqL2NSRUFISDFpMVVLYTBhQ3NJaUFOZkVkUU41T2s2d3RGSkpocDNhcEF2blZrclpEZk9HNXdlOWJZenZHb0k3U1VubGVVUkJKK04zaWhqQVJmTDRoRGVlUkhoWXlMa00za0V5RWtySkJMNXIwR0RqaWN4TSthRmNSMmZDQkFrdjNnclQ1a3o0a0xjdnNtSFgrOURCdz09Il19.eyJhY3NFcGhlbVB1YktleSI6eyJrdHkiOiJFQyIsImNydiI6IlAtMjU2IiwieCI6IkdGU3JiM3MxNWhPdGJWcFNSLWNyMFJlY0dINXJiRXF5OHVGOG5kZkUxSDgiLCJ5IjoicXV4SHJfRWN6M3hfUTRoV3hpRFJzb0NObW9wZGJoelRsT3RCYm1lOVptayJ9LCJhY3NVUkwiOiJodHRwczovL3ZlcmlmeS5tb256by5jb20vM2RzMi9hcHBfbWVzc2FnZSIsInNka0VwaGVtUHViS2V5Ijp7Imt0eSI6IkVDIiwiY3J2IjoiUC0yNTYiLCJ4IjoiMmV3QUFjZ3Z3MkVSdkI2SHpNaUtiVWE1S1E4MUI5Ri1Sd2s2R2M2VkQ4byIsInkiOiIyeE1uTFZ6UTl0RjEtRzI4Z0k1ck1kc2F2ZHhtakNRanVZUnVTZTF0QjAwIn19.bLgIbjuWawpB_lzlItJIjnaCkY2CS2izKo3AU22j8cRHV9QqWDZDoicKj4Zax9oucFN1VOoEYxA55cM945PDza6hZrXGr2C3BmQKqpTpAmG-JDyXb_ggpFZEgF3blIyWt9hxacKhD7MF5I-ORsphLdocnshX4YyjECkD_74GjesqS46XvD0KO9zPDCRv-N72z_5qx84OcMI8mA9k6Q3k2SOg9JoKwY0X3QlMx41aXfJmrBG2ykZmde--NX9w1JN8Z-HfcOGD2wBPFLH_ZT-XJvt1X5JzlBXlyhhw1SJ-dh8cBJsdRuwq34ZvUP1doN2bA1r1F233NMKqsF93S6ZXbQ";
        private const string AcsSignedContentMastercardInvalid = "eyJ4NWMiOlsiTUlJRm1EQ0NBNENnQXdJQkFnSVJBTjVmWWdLREhKYWdYYUppcUhRZ3J6Y3dEUVlKS29aSWh2Y05BUUVMQlFBd2VERUxNQWtHQTFVRUJoTUNWVk14RXpBUkJnTlZCQW9UQ2sxaGMzUmxja05oY21ReEtEQW1CZ05WQkFzVEgwMWhjM1JsY2tOaGNtUWdTV1JsYm5ScGRIa2dRMmhsWTJzZ1IyVnVJRE14S2pBb0JnTlZCQU1USVZCU1JDQk5ZWE4wWlhKRFlYSmtJRE5FVXpJZ1NYTnpkV1Z5SUZOMVlpQkRRVEFlRncweE9UQTJNVGN4TXpRNU1qVmFGdzB5TVRBMk1UWXhNelEyTURCYU1IOHhDekFKQmdOVkJBWVRBazVQTVJBd0RnWURWUVFLRXdkTmIyUnBjblZ0TVRNd01RWURWUVFMRXlwQlExTk5VeTFCUTFNdFZqSXhNQzFOVDBSSlVsVk5YMDlWTFRReU5EazJMVTF2WkdseWRXMGdUM1V4S1RBbkJnTlZCQU1USURORVV6SXdJRWxrWlc1MGFYUjVJRU5vWldOcklGTnBaMjVwYm1jZ1QxTk1NSUlCSWpBTkJna3Foa2lHOXcwQkFRRUZBQU9DQVE4QU1JSUJDZ0tDQVFFQXRSd2dkeCtyakZyWEJCcFhIUVY2UE5DckJJNWwxbmxTWlIxWklkNCtuWEpFamg5ZnVBQkNNazhRSUd1SEtoN2pnK1kraTk0OE45R1lGcUkyRk1xUk9YSXNXMHFrXC9wZVE5OE1VWHVtTkZsaEt2ekNlUkhGUDdEVW9NbWFhQUxOaWZ0Y3A5MGVsMU5SdTJ2TjQxaGZMQktCS1dneEZQXC91czlKTUNSTWJpXC9KWFV0VFlRZGxTN2pCa1JpUnNFZ2JjRnd0U1ZrejdmTk1tQmlZRnBoZ3RPQllmeVN4M09CUGJxQXN3REo3RnM3VkpUUGRhRVptd09sY1FXZWE5emdhXC9vdW5ieE9PdXJnNDZWRVUwQktDbVRCdk9INGdiOHBPWlhmZmtsUzMwMUlQYlZSeXg4MFRNbUdsUGpcL0ZvTnhvXC91WllFWkh5Q0ZNVnByV3Nma1pQY0k1d0lEQVFBQm80SUJGRENDQVJBd0RnWURWUjBQQVFIXC9CQVFEQWdlQU1Ba0dBMVVkRXdRQ01BQXdId1lEVlIwakJCZ3dGb0FVVFwvZkRuMkxLVmxSTDMwaDB5M0FjYnp6ZEVFVXdTQVlJS3dZQkJRVUhBUUVFUERBNk1EZ0dDQ3NHQVFVRkJ6QUJoaXhvZEhSd09pOHZiMk56Y0M1d2Eya3VhV1JsYm5ScGRIbGphR1ZqYXk1dFlYTjBaWEpqWVhKa0xtTnZiVEJwQmdOVkhSOEVZakJnTUY2Z1hLQmFobGhvZEhSd09pOHZZM0pzTG5CcmFTNXBaR1Z1ZEdsMGVXTm9aV05yTG0xaGMzUmxjbU5oY21RdVkyOXRMelJtWmpkak16bG1OakpqWVRVMk5UUTBZbVJtTkRnM05HTmlOekF4WXpabU0yTmtaREV3TkRVdVkzSnNNQjBHQTFVZERnUVdCQlR0ZGdsZ3EwV2FIQ2I0YnFNNkpLcHdiVUJKMkRBTkJna3Foa2lHOXcwQkFRc0ZBQU9DQWdFQWR0eWVScTcyM2pxK0ZNSGdIcHNuUlwvcXp2bk1WRjZOVXFYUW1OUkZqdnBDR1RSWHJWV2xKdTU5YXFcL1lKNFlFVFJzcnlLV1dPT3ZiM0FmRUh2M21zVjRLM1NLdFMxMWhDRmpcL3ppSWtYYjNQNGh3NWxOYXZqeEhXU01XN3cxM3VZVlRpMG96bmw5MVVHRXljOHpJckhERWlmdzNySGhvRjZYMG96K29XWHpzSERqR2p2QlNUcEo4MjBzNU1YWnpGaDl1eGZ5cGp3U1FNXC9XMGVYSHo1cE1PZkRKaVM3NHVzczMyQ0J3c3BxXC9kcTlKdTZBRW9CU0NBa2ZOY0k3dFh3ZzhkWE9YRDV3NkFlZnZXcnVEMTJHN21YNEx5cnkrSlkwXC9LQjRCYW5NNGlqdFwvcmpmVGNFM1daT1ZYTVlmMTNpN3RhNVplVVpTaEdLOTVzWkM5K2N0VHNmeTVNc0xnRzFxUjFvcVAxQXMzdWppbXVGOHVzeTBhdlZqT2JUbWVlRXZqNG9YeFJFSzFmUHZhaFNLbHpLcEIxMkI2SEdDVERSWFNmWFplR0luUU85QnpFK0d4RGFteU5CN3FFdVNBN1crRTU4SkZcL2hrR2NaQVwvT0FFYmczUmM3Nit5RWJudjFRcXcyR2hJS3Bnc0c2TVJsaVI2VlkxcVB6WDBkM2dFOEplSnArRjR4V2RONXN5TGU0QVlxcHgzb0pLeDVuTzRsKzJFQ1NPVjdBUVNMV1dOZ1pJcVZ5YU5kazJyRFwvQVFtQTRcL2tcL1wvaEg0WEhjOU9XYkxmMHdUeCsyQjdPRTMrWmVmTWZNTkNiTHVPaGRZNG05emVVQ2NPbGwzWWJ4bVpDbW1kazZQcGJVbm41UEZvZGc4cGk4XC94cE4ralVmT1wvYkpjRU1MaXFsVlJOY1pVPSIsIk1JSUduVENDQklXZ0F3SUJBZ0lRQ1dYQWdpV1wveFF1NldRR2kwbEh4S1RBTkJna3Foa2lHOXcwQkFRc0ZBREI4TVFzd0NRWURWUVFHRXdKVlV6RVRNQkVHQTFVRUNoTUtUV0Z6ZEdWeVEyRnlaREVvTUNZR0ExVUVDeE1mVFdGemRHVnlRMkZ5WkNCSlpHVnVkR2wwZVNCRGFHVmpheUJIWlc0Z016RXVNQ3dHQTFVRUF4TWxVRkpFSUUxaGMzUmxja05oY21RZ1NXUmxiblJwZEhrZ1EyaGxZMnNnVW05dmRDQkRRVEFlRncweE56QXhNREV3TlRBd01EQmFGdzB5TmpBM01UVXdOekF3TURCYU1IZ3hDekFKQmdOVkJBWVRBbFZUTVJNd0VRWURWUVFLRXdwTllYTjBaWEpEWVhKa01TZ3dKZ1lEVlFRTEV4OU5ZWE4wWlhKRFlYSmtJRWxrWlc1MGFYUjVJRU5vWldOcklFZGxiaUF6TVNvd0tBWURWUVFERXlGUVVrUWdUV0Z6ZEdWeVEyRnlaQ0F6UkZNeUlFbHpjM1ZsY2lCVGRXSWdRMEV3Z2dJaU1BMEdDU3FHU0liM0RRRUJBUVVBQTRJQ0R3QXdnZ0lLQW9JQ0FRQ2JEV3o1NWg4WWhVdEhqczgzeVRwb0ZOYlVnSXZuUGNKVFNFVXBpaUNMZWRpY2g2MURwUVhmOG5CSWRVRnYrenM3akVLUytsTmNaeDc1aVV3R0oybVJvUk5pa0pqN25LV3dJUEdTcmhzSmVPR1Jud2tnNmtoOVBFRU5SZ1V5bDZETVVza1BZaFo5K0tOZXVIcnQzSGUrc2ZCYmxLXC9SUTNKUnN5TlY5Mll0TVBCQWsyTmRkQnZtaDlcL0ZTbnl1MTN3U2hMVWF0eHlzWjY5OE9JWjJicFhzRDJGTzNNajQ5UlhwWFN5TU96Um5XTUIxT0pDY3FaMW5QSmhHM3ZwSDN2YWJUZWhYMmNHTDN4a2FhNklUVzZta3JuZUtqbktvUzNNMzNiUlJlK09nQW5sdW9jWTYrcUpocU1rQ01wRkxRTEdVY25rWGlyRkNLcU1MNU9BSldYUVFnUktGWTJPckRBdWdNUHZzbTR5TlZoRUJvY0h3dzZ1YzY2SnZyRTdUQmt5NkhBOHZJSlgzc1ZVUGwySFwvNUh2VWtWaGtFUndtaFM2S2oyeE5WZHcyMlFVelhjYTV3cHhodTVxemtZS3VFOGZCU1RPaDVXSytBRWZzYnFrWjEwN1VnNitya0xaSUNwK1wvaGJUbkltMG56b0pqc05OdldiSkh4alAwRzJEMzYyUWk2b08zVUtrZDlvc3cyOUNkSFNia2phWXBxQkVrbUMrYzJiQ3FyQVByZUxCV2Q0MWlpcVJlS0ptTWg4eVNcLzMySDl4UEJIaWRQcDBsZjcyQ1IyWlwvQzl0SlBNZ1lVZkVVa2JSdVQ1U1ErMzArVE5JWWxaZ3lMeDV1TFhyelhLenhzQSthZlNuanhTZ21YVXdnZXE2RkhTTDY5UXVIaUZRcktnR0RJRFg2cHJ3SURBUUFCbzRJQkhUQ0NBUmt3U0FZSUt3WUJCUVVIQVFFRVBEQTZNRGdHQ0NzR0FRVUZCekFCaGl4b2RIUndPaTh2YjJOemNDNXdhMmt1YVdSbGJuUnBkSGxqYUdWamF5NXRZWE4wWlhKallYSmtMbU52YlRCcEJnTlZIUjhFWWpCZ01GNmdYS0JhaGxob2RIUndPaTh2WTNKc0xuQnJhUzVwWkdWdWRHbDBlV05vWldOckxtMWhjM1JsY21OaGNtUXVZMjl0TDJRMFlUVTFNV0ZoT1RKa1lURXhPR0l4TkdNMFptWXhNVGRoWlRFeFpHVXhNekZtT1RSaFlqRXVZM0pzTUE0R0ExVWREd0VCXC93UUVBd0lCaGpBU0JnTlZIUk1CQWY4RUNEQUdBUUhcL0FnRUFNQjBHQTFVZERnUVdCQlJQOThPZllzcFdWRXZmU0hUTGNCeHZQTjBRUlRBZkJnTlZIU01FR0RBV2dCVFVwVkdxa3RvUml4VEVcL3hGNjRSM2hNZmxLc1RBTkJna3Foa2lHOXcwQkFRc0ZBQU9DQWdFQW0wMkJ2NmlJXC9aa3ZzcjZkQUdcL2dpSkNPK3BldkRIODBXR2hhZ3Z2eWVJY2pJd3dmc1NJVWI5dzM1WFJSaENPa3hTblA3bHVZbW5PY29qdlNERW5lVVZiU3NNeDlcL3pxXC9SMUJsXC9Gd0FJa3N6SkFjZ3hOaDFwXC80SDlqSlRWRkUwVDFKbDZDMVkwb0YrWDBmaFV4UXBIMUZZQlhBNFdaVCs3UkRHXC8zM09EOG5kQlRPd291ZWhcL040RXdGWmw4UjRrMUZyZGVhUm5tdXhTb0lJa2J2NklkZFZNM09nVE1OWHJpNGNOUDNIMmpwaTE0QmdoYld5M1YyODI0aG9EV3dmVm83U01iUDFNeG9Yb281SFdwRUVvdWt0UVhYak5EWVl0ZEZZUVdEbWxyNkZoZUFIMVFudXp0d3hkODc4XC9DOUR2Y3VzdVY4SFB0QUZRVlZqc1hvM2dJY3RuNU5EQStjelh2d2RZY2grU3FaYm01WXZiTVQ5UkpZem5aQXNTM2ZEXC9zZFp1VGdqMzBtWTEyZitKdWdoTEtheXpldHg1T09vR29UcVFQRm5rMEo4cis0aWN3UU5LTnhOdkhUTmhEVmtQVXNvZVwvWjA3NmVkdjJyQ1g0dHlWYVhyVlo1MEswNGZWN2swSGdWSWxRellFWWVRODRsbklidU1lTVo3MGJnK0VhTk5OWnVvZGZIdXZVUWtlMlwvR1FHM0Q3NVZZTWN3cWRBQUdINkp3NU9kbkQ3TTRXXC9LaEVuSXpqOWNqb1pnTnRZYktXRk5vdWF0NGdQbjNiR2xJbmZacHJrRzJDRXBJbElacDJxVk1sVDg1ZmFnREZqb2k4SHNzZlFtYm00R0JPc0NPUDV0T3RvWVVqaHdlYTQyOTdDT24ybnk2d0MyN1ZEbGM2T21uRDJUOD0iLCJNSUlGeHpDQ0E2K2dBd0lCQWdJUUZzanlJdXFodzgwd05NalhVNDdsZmpBTkJna3Foa2lHOXcwQkFRc0ZBREI4TVFzd0NRWURWUVFHRXdKVlV6RVRNQkVHQTFVRUNoTUtUV0Z6ZEdWeVEyRnlaREVvTUNZR0ExVUVDeE1mVFdGemRHVnlRMkZ5WkNCSlpHVnVkR2wwZVNCRGFHVmpheUJIWlc0Z016RXVNQ3dHQTFVRUF4TWxVRkpFSUUxaGMzUmxja05oY21RZ1NXUmxiblJwZEhrZ1EyaGxZMnNnVW05dmRDQkRRVEFlRncweE5qQTNNVFF3TnpJME1EQmFGdzB6TURBM01UVXdPREV3TURCYU1Id3hDekFKQmdOVkJBWVRBbFZUTVJNd0VRWURWUVFLRXdwTllYTjBaWEpEWVhKa01TZ3dKZ1lEVlFRTEV4OU5ZWE4wWlhKRFlYSmtJRWxrWlc1MGFYUjVJRU5vWldOcklFZGxiaUF6TVM0d0xBWURWUVFERXlWUVVrUWdUV0Z6ZEdWeVEyRnlaQ0JKWkdWdWRHbDBlU0JEYUdWamF5QlNiMjkwSUVOQk1JSUNJakFOQmdrcWhraUc5dzBCQVFFRkFBT0NBZzhBTUlJQ0NnS0NBZ0VBeFpGM25DRWlUOFhGRmFxKzNCUFQwY01EbFdFNzZJQnNkeDI3dzNoTHh3VkxvZzQyVVRhc0lnem15c1RLcEJjMTdIRVp5TkFxazlHckNIbzBPeWs0Slp1WEhvVzgwZ29aYVIyc01ubjQ5eXR0N2FHc0UxUHNmVnVwOGdxQW9yZm0zSUZhYjJcL0NuaUpKTlhhV1Bnbjk0K1VcL25zb2FxVFE2ais2SkJvSXduRmtsaGJYSGZLcnFsa1VaSkNZYVdiWlJpUTdua0FOWVlNMlRkM044N0ZtUmFubURYajVCRzZsYzlvMWNsVEM3VXZSUW1OSUw5T2RERFo4cWxxWTJGaTBlenRCbnVvMkRVUzV0R2RWeThTZ3FQTTNFMTJmdGs0RWRsS3lyV21CcUZjWXdHeDRBY1NKODhPM3JRbVJCTXh0azByNXZoZ3I2aERDR3E3RkhLXC9oUUZQOUxoVU85MXF4V0V0TW43NlNhN0RQQ0xhcyt0Zk5SVndHMTJGQnVFWkZoZFNcL3FLTWRJWVVFNVE2dXdHVEV2VHpnMmttZ0pUM3NOYTZkYmhsWW5ZbjlpSWpUaDBkUEdnaVhhcDFCaGk4QjlhYVBGY0hFSFNxVzhuWlVJTmNyd2Y1QVVpKzdEK3FcL0FHNUl0aUJ0UVRDYWFGbTc0Z3Y1MXl1dHp3Z0tuSDlRK3gzbXR1S1wvdXdsTENzbGo5RGVYZ096TVdGeEZndXV3TEdYMzlrdERuZXR4TnczUExhYmpIa0RsR0RJZngwTUNRYWtNNzRzVGN1VzhJQ2lIdk5BN2Z4WENuYnRqc3k3YXRcL3lYWXdBZCtJRFM1MU1BXC9nM09ZVk40TSswcEc4NDNSZTZaNTNvT0RwMFltdWd4MEZOTzFOeFQzSE8xaGQ3ZFh5akFWXC90TlwvR0djQ0F3RUFBYU5GTUVNd0RnWURWUjBQQVFIXC9CQVFEQWdHR01CSUdBMVVkRXdFQlwvd1FJTUFZQkFmOENBUUV3SFFZRFZSME9CQllFRk5TbFVhcVMyaEdMRk1UXC9FWHJoSGVFeCtVcXhNQTBHQ1NxR1NJYjNEUUVCQ3dVQUE0SUNBUUJMcUlZb3JydFZ6NTZGNldPb0xYOUNjUmpTRmltN2dPODczYTNwNys2Mkk2am9YTXNNcjBuZDluUlBjRXdkdUVsb1pYd0ZnRXJWVVFXYVVaV05wdWUwbUd2VTdCVUFnVjlUdTBKMHlBKzlzcml6Vm9NdngrbzR6VEozVnU1cDVhVGYxYVlvSDF4WVZvNW9vRmdsXC9oSVwvRVhEMmxvXC94T1VmUEtYQlk3dHdmaXFPemlRbVRHQnVxUFJxOGgzZFFSbFhZeFhcL3J6R2Y4MFNlY0lUNndvOUthdkRrak9tSldHenpIc242UnlvNk1FQ2xNYVBuMHRlODd1a05ONzQwQWRQaFR2TmVaZFdsd3lxV0FKcHN2MjRjYUVja2pTcGdwb0laT2pjN1BBY0VWUU9XRlN4VWVzTWs0Sno1YlZaYVwvQUJqemNwK3JzcTFRTFNKNXF1cUh3V0ZUZXdDaHdwdzVncHcrRTVTcEtZNkZJSFBsVGRsK3FIVGh2Tjhsc0tOQVFnMHFUZEViSUZaQ1VRQzBDbDNUaTNxXC9jWHY4dGd1TEpOV3ZkR3pCNjAwWTMyUUhjbE1wZXlhYlQ0XC9RZU9lc3FweDZEYTcwSjJLdkxUMWo2Q2gyQnNLU3plVkxhaHJqbm9QcmRnaUlZWUJPZ2VBM1Q4U0UxcGdhZ3Q1NlI3bklrUlFidGVzb1JLaStOZkM3cFBiXC9HMVZVc2pcL2NSRUFISDFpMVVLYTBhQ3NJaUFOZkVkUU41T2s2d3RGSkpocDNhcEF2blZrclpEZk9HNXdlOWJZenZHb0k3U1VubGVVUkJKK04zaWhqQVJmTDRoRGVlUkhoWXlMa00za0V5RWtySkJMNXIwR0RqaWN4TSthRmNSMmZDQkFrdjNnclQ1a3o0a0xjdnNtSFgrOURCdz09Il0sImFsZyI6IlBTMjU2In0.eyJhY3NVUkwiOiJodHRwczpcL1wvYWNzMS4zZHMubW9kaXJ1bS5jb21cL21kcGF5YWNzXC9jcmVxYXBwIiwiYWNzRXBoZW1QdWJLZXkiOnsia3R5IjoiRUMiLCJjcnYiOiJQLTI1NiIsIngiOiJNTDJrMUt3SWFXaG4zNVl2b2V5SWZvWE01VnZGdFR4UUdabVhiaUNPUVh3IiwieSI6ImtsSUFEMDVZMUNhcURvOUJsbVgyYmk3YUttNFludWVFMVNKTjJUVFJlZ2sifSwic2RrRXBoZW1QdWJLZXkiOnsia3R5IjoiRUMiLCJjcnYiOiJQLTI1NiIsIngiOiJQdTVaRVZXZkhEVFJuajNWRlJtekJoQTFRSTZESzNoUmpnTnVNeG9OTWNVIiwieSI6Ikl4c1p6UV84VkRIYlc3bHhPWlpTZzc2Q3hfeWFrdzl2ZDl2djFRbGdDc3cifX0.CFQykBc6pqGKxDARnU6SBsHlWZT_L3_Q7tlEkIPPZxXVetZalBRecv0uEa_yag7P5PexiXCJbE0nzMTtNTmlRDCNNoS9qfgWysC4L3nChLLH5dg-fiZV8MH9JrWjZEhNwmiAiQb3OiYyJxA6TUeWVYO1epGr1I1HwM12QD7kTop9k3pbPgKtr5tMe_CctBQ1OmfWZNj4k2px8fhxCRtXWAMLCw1hQpr4KmGZ2WreVvfpLvlE7c2TBrkStVJhof0M1nZe1rZRJrJT4IqVyjie1DWYiTEIjPDXALh5RGSNBrv4IXM1-S9GySo-kfPJufUCBrh8Kgq7BwgDN6LJwwZwNQ";

        [TestMethod]
        [DataRow(1, "V17", "FlightName1,PXPSD2SettingVersionV17,PXEnablePSD2ServiceSideCertificateValidation,FlightName2", TransactionStatus.C, AcsSignedContentVisa, "visa", "", "Unknown")] /// Valid ACS and appropriate PaymentMethodType
        [DataRow(1, "V17", "FlightName1,PXPSD2SettingVersionV17,PXEnablePSD2ServiceSideCertificateValidation,FlightName2", TransactionStatus.C, AcsSignedContentMastercard, "mc", "", "Unknown")] /// Valid ACS and appropriate PaymentMethodType
        [DataRow(1, "V14", "FlightName1,PXPSD2SettingVersionV14,PXEnablePSD2ServiceSideCertificateValidation,FlightName2", TransactionStatus.C, AcsSignedContentVisa, "visa", "", "Unknown")] /// Version 14, should be bumped to 17, valid
        [DataRow(1, "V17", "FlightName1,PXPSD2SettingVersionV17,PXEnablePSD2ServiceSideCertificateValidation,FlightName2", TransactionStatus.C, AcsSignedContentEmpty, "visa", "", "Unknown")] /// Empty ACS string, invalid, TODO: Change status to Failed after code is reverted in ValidateACSSignedContent
        [DataRow(1, "V17", "FlightName1,PXPSD2SettingVersionV17,PXEnablePSD2ServiceSideCertificateValidation,FlightName2", TransactionStatus.C, AcsSignedContentBadFormat, "visa", "", "Unknown")] /// Invalid ACS string, TODO: Change status to Failed after code is reverted in ValidateACSSignedContent
        [DataRow(1, "V17", "FlightName1,PXPSD2SettingVersionV17,PXEnablePSD2ServiceSideCertificateValidation,FlightName2", TransactionStatus.C, AcsSignedContentVisaEmulatorWithHeader, "visa", "px-service-psd2-e2e-emulator", "Unknown")] /// Emulator test with valid parameters
        [DataRow(1, "V17", "FlightName1,PXPSD2SettingVersionV17,PXEnablePSD2ServiceSideCertificateValidation,FlightName2", TransactionStatus.C, AcsSignedContentVisaEmulatorWithoutHeader, "visa", "px-service-psd2-e2e-emulator", "Unknown")] /// Emulator test with invalid parameters, TODO: Change status to Failed after code is reverted in ValidateACSSignedContent
        [DataRow(1, "V17", "FlightName1,PXPSD2SettingVersionV17,PXEnablePSD2ServiceSideCertificateValidation,FlightName2", TransactionStatus.C, AcsSignedContentVisa, "mc", "", "Unknown")] /// ACS & PaymentMethodType mismatch, TODO: Change status to Failed after code is reverted in ValidateACSSignedContent
        [DataRow(1, "V17", "FlightName1,PXPSD2SettingVersionV17,PXEnablePSD2ServiceSideCertificateValidation,FlightName2", TransactionStatus.C, AcsSignedContentMastercard, "visa", "", "Unknown")] /// ACS & PaymentMethodType mismatch, TODO: Change status to Failed after code is reverted in ValidateACSSignedContent
        [DataRow(1, "V17", "FlightName1,PXPSD2SettingVersionV17,PXEnablePSD2ServiceSideCertificateValidation,FlightName2", TransactionStatus.C, AcsSignedContentVisa, "invalidPaymentMethod", "", "Failed")] /// Invalid PaymentMethodType
        [DataRow(1, "V17", "FlightName1,PXPSD2SettingVersionV17,PXEnablePSD2ServiceSideCertificateValidation,FlightName2", TransactionStatus.Y, AcsSignedContentVisa, "visa", "", "Succeeded")] /// Should not hit certificate validation code, as TranactionStatus is Y
        [DataRow(1, "V16", "FlightName1,PXPSD2SettingVersionV16,PXEnablePSD2ServiceSideCertificateValidation,FlightName2", TransactionStatus.C, AcsSignedContentVisa, "visa", "", "Unknown")] /// V16
        [DataRow(1, "V19", "FlightName1,PXPSD2SettingVersionV19,PXEnablePSD2ServiceSideCertificateValidation,FlightName2", TransactionStatus.C, AcsSignedContentVisa, "visa", "", "Unknown")] /// V19
        [DataRow(1, "V19", "FlightName1,PXPSD2SettingVersionV19,PXEnablePSD2ServiceSideCertificateValidation,FlightName2", TransactionStatus.C, null, "visa", "", "Unknown")] /// Catch null exception, TODO: Change status to Failed after code is reverted in ValidateACSSignedContent
        [DataRow(1, "V20", "FlightName1,PXPSD2SettingVersionV20,PXEnablePSD2ServiceSideCertificateValidation,FlightName2", TransactionStatus.C, AcsSignedContentVisa, "visa", "", "Unknown")] /// V19
        [DataRow(1, "V20", "FlightName1,PXPSD2SettingVersionV20,PXEnablePSD2ServiceSideCertificateValidation,FlightName2", TransactionStatus.C, null, "visa", "", "Unknown")] /// Catch null exception, TODO: Change status to Failed after code is reverted in ValidateACSSignedContent
        [DataRow(1, "V17", "FlightName1,PXPSD2SettingVersionV17,PXEnablePSD2ServiceSideCertificateValidation,FlightName2", TransactionStatus.C, AcsSignedContentMastercardInvalid, "mc", "", "Unknown")] /// Invalid Chain, status should fail, TODO: Change status to Failed after code is reverted in ValidateACSSignedContent
        [DataRow(1, "V21", "FlightName1,PXEnablePSD2ServiceSideCertificateValidation,FlightName2", TransactionStatus.C, AcsSignedContentVisa, "amex", "", "Unknown")] /// V21
        [DataRow(1, "V21", "FlightName1,PXEnablePSD2ServiceSideCertificateValidation,FlightName2", TransactionStatus.C, null, "amex", "", "Unknown")] /// Catch null exception, TODO: Change status to Failed after code is reverted in ValidateACSSignedContent
        public async Task ValidatePaymentChallenge_CheckCertificateValidation_UnknownOrFailedAsync(
            int tryCount,
            string requestSettingsVersion,
            string flightsStr,
            TransactionStatus transStatus,
            string acsContent,
            string paymentMethodType,
            string testContext,
            string expectedPaymentChallengeStatus)
        {
            /// Arrange
            mockPayerAuthServiceAccessor.Setup(x => x.Authenticate(
            It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationRequest>(),
            It.IsAny<EventTraceActivity>()))
            .Returns(Task.FromResult(new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationResponse()
            {
                EnrollmentStatus = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentInstrumentEnrollmentStatus.Bypassed,
                EnrollmentType = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentInstrumentEnrollmentType.ThreeDs,
                TransactionStatus = transStatus,
                TransactionStatusReason = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.TransactionStatusReason.TSR01,
                AcsSignedContent = acsContent
            }));

            var sessionData = new Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession
            {
                Id = SessionId,
                PaymentMethodType = paymentMethodType
            };

            mockSessionServiceAccessor
                .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(sessionData);

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            var authRequest = new AuthenticationRequest()
            {
                SettingsVersion = requestSettingsVersion,
                SettingsVersionTryCount = (ushort)tryCount
            };

            /// Act
            var flights = flightsStr.Split(',').ToList();

            Common.TestContext tc = new Common.TestContext(
                      contact: "px.azure.cot",
                      retention: DateTime.MaxValue,
                      scenarios: testContext);
            try
            {
                var x = await paymentSessionsHandler.Authenticate(AcountId, SessionId, authRequest, flights, new EventTraceActivity(), tc);
                string actualChallengeStatus = x.ChallengeStatus.ToString();

                /// Assert
                Assert.AreEqual(expectedPaymentChallengeStatus, actualChallengeStatus, string.Format("The expected payment challenge status is {0} and the output was {1}.", expectedPaymentChallengeStatus, actualChallengeStatus));
            }
            catch (Exception ex)
            {
                Assert.Fail($"Expected exception is ValidationException, but different exception was thrown. Details {ex.Message}");
            }
        }

        [DataRow(PaymentChallengeStatus.Succeeded)]
        [DataRow(PaymentChallengeStatus.Failed)]
        [DataTestMethod]
        public void GetChallengeRedirectUriFromPaymentSessionTest(PaymentChallengeStatus challengeStatus)
        {
            string specialCharacters = "@#$%^&+=`[]{};':\",/<>?";
            string encodedSpecialCharacters = HttpUtility.UrlEncode(specialCharacters);

            Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model.PaymentSession paymentSession = new Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model.PaymentSession()
            {
                ChallengeStatus = challengeStatus,
                Id = "ZZZZ1234567890",
                SuccessUrl = "https://bing.com/success",
                FailureUrl = "https://bing.com/failed",
                PaymentInstrumentId = "abcdefg" + specialCharacters
            };

            Uri uri = PaymentSessionsHandlerV2.GetChallengeRedirectUriFromPaymentSession(paymentSession);
            Assert.IsNotNull(uri);

            bool isSuccess = challengeStatus == PaymentChallengeStatus.Succeeded;
            string expectedBaseUrl = isSuccess ? paymentSession.SuccessUrl : paymentSession.FailureUrl;

            Assert.IsTrue(uri.AbsoluteUri.Contains(expectedBaseUrl));

            if (isSuccess)
            {
                Assert.IsFalse(uri.AbsoluteUri.Contains(specialCharacters));
                Assert.IsTrue(uri.AbsoluteUri.Contains(encodedSpecialCharacters));

                Assert.IsTrue(uri.AbsoluteUri.Contains($"challengeStatus={paymentSession.ChallengeStatus}"));
                Assert.IsTrue(uri.AbsoluteUri.Contains($"piid={HttpUtility.UrlEncode(paymentSession.PaymentInstrumentId)}"));
            }
        }

        [TestMethod]
        [DataRow("2.2.0", "2.2.0", "2.1.0", "2.1.0")]
        [DataRow("2.2.0", "2.2.0", "2.2.0", "2.2.0")]
        [DataRow("2.2.0", "2.2.0", null, "2.1.0")]
        [DataRow(null, "2.1.0", "2.1.0", "2.1.0")]
        [DataRow(null, "2.1.0", null, "2.1.0")]
        public async Task Authenticate_SendsMessageVersion(string authReqMessageVersion, string expectedAuthReqMessageVersion, string payerAuthResponseMessageVersion, string expectedFinalMessageVersion)
        {
            /// Arrange
            mockPayerAuthServiceAccessor.Setup(x => x.Authenticate(
            It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationRequest>(),
            It.IsAny<EventTraceActivity>()))
            .Returns(Task.FromResult(new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationResponse()
            {
                EnrollmentStatus = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentInstrumentEnrollmentStatus.Enrolled,
                EnrollmentType = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentInstrumentEnrollmentType.ThreeDs,
                TransactionStatus = TransactionStatus.C,
                TransactionStatusReason = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.TransactionStatusReason.TSR01,
                AcsSignedContent = string.Empty,
                MessageVersion = payerAuthResponseMessageVersion
            }));

            var sessionData = new Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession
            {
                Id = SessionId,
                PaymentMethodType = "visa"
            };

            mockSessionServiceAccessor
                .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(sessionData);

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            var authRequest = new Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model.AuthenticationRequest()
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
                SettingsVersion = "V11",
                SettingsVersionTryCount = 1,
                MessageVersion = authReqMessageVersion
            };

            /// Act
            Common.TestContext tc = new Common.TestContext(
                contact: "px.azure.cot",
                retention: DateTime.MaxValue,
                scenarios: string.Empty);

            PXSettings.PayerAuthService.PreProcess = (request) =>
            {
                var authRequestContent = request.Properties["Payments.Content"] as Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationRequest;
                Assert.AreEqual(authRequestContent.MessageVersion, expectedAuthReqMessageVersion, "Mismatch between expected AuthRequest Message Version");
            };

            try
            {
                var authResponse = await paymentSessionsHandler.Authenticate(AcountId, SessionId, authRequest, new List<string>(), new EventTraceActivity(), tc);
                var messageVersion = authResponse.MessageVersion;

                /// Assert
                Assert.AreEqual(messageVersion, expectedFinalMessageVersion, "ExpectedMessageVersion mismatch");
            }
            catch
            {
                Assert.Fail("Authenticate call failure");
            }

            PXSettings.PayerAuthService.ResetToDefaults();
        }

        [TestMethod]
        [DataRow("2.2.0", "2.1.0", "2.1.0")]
        [DataRow("2.2.0", "2.2.0", "2.2.0")]
        [DataRow("2.2.0", null, "2.1.0")]
        public async Task BrowserAuthenticate_SendsMessageVersion(string authReqMessageVersion, string payerAuthResponseMessageVersion, string expectedCreqMessageVersion)
        {
            /// Arrange
            var mockPayerAuthResp = new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationResponse()
            {
                EnrollmentStatus = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentInstrumentEnrollmentStatus.Enrolled,
                EnrollmentType = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentInstrumentEnrollmentType.ThreeDs,
                TransactionStatus = TransactionStatus.C,
                TransactionStatusReason = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.TransactionStatusReason.TSR01,
                AcsSignedContent = string.Empty,
                MessageVersion = payerAuthResponseMessageVersion
            };
            mockPayerAuthServiceAccessor.Setup(x => x.Authenticate(
            It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationRequest>(),
            It.IsAny<EventTraceActivity>()))
            .Returns(Task.FromResult(mockPayerAuthResp));

            var sessionData = new Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession
            {
                Id = SessionId,
                PaymentMethodFamily = "credit_card",
                PaymentMethodType = "visa",
                MethodData = new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.ThreeDSMethodData
                {
                    ThreeDSMethodURL = "fakeurl",
                    ThreeDSServerTransID = "fakeId"
                },
                ChallengeStatus = PaymentChallengeStatus.Unknown,
                AuthenticationResponse = mockPayerAuthResp,
                Amount = 800,
                Country = "GB",
                Currency = "EUR",
                Partner = "webblends",
                Language = "en-GB",
                BrowserInfo = new BrowserInfo
                {
                    ChallengeWindowSize = ChallengeWindowSize.Five
                }
            };

            mockSessionServiceAccessor
                .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(sessionData);

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            /// Act
            PXSettings.PayerAuthService.PreProcess = (request) =>
            {
                var authRequestContent = request.Properties["Payments.Content"] as Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationRequest;
                Assert.AreEqual(authRequestContent.MessageVersion, authReqMessageVersion, "Mismatch between expected AuthRequest Message Version");
            };

            try
            {
                var authResponse = await paymentSessionsHandler.Authenticate(SessionId, false, null);
                var creq = authResponse.FormInputCReq;
                var decodedUrl = Microsoft.Commerce.Payments.PXService.V7.ThreeDSUtils.DecodeUrl(creq);
                var decodedBase64 = Microsoft.Commerce.Payments.PXService.V7.ThreeDSUtils.DecodeBase64(decodedUrl);
                var decodedCreq = JsonConvert.DeserializeObject<ChallengeRequest>(decodedBase64);

                /// Assert
                Assert.AreEqual(decodedCreq.MessageVersion, expectedCreqMessageVersion, "ExpectedMessageVersion mismatch");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Expected exception is ValidationException, but different exception was thrown. Details {ex.Message}");
            }

            PXSettings.PayerAuthService.ResetToDefaults();
        }

        [TestMethod]
        public async Task Authenticate_HandleFraudTransStatus()
        {
            /// Arrange
            mockPayerAuthServiceAccessor.Setup(x => x.Authenticate(
            It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationRequest>(),
            It.IsAny<EventTraceActivity>()))
            .Returns(Task.FromResult(new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationResponse()
            {
                EnrollmentStatus = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentInstrumentEnrollmentStatus.Enrolled,
                EnrollmentType = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentInstrumentEnrollmentType.ThreeDs,
                TransactionStatus = TransactionStatus.FR,
                TransactionStatusReason = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.TransactionStatusReason.TSR01,
                AcsSignedContent = string.Empty,
                MessageVersion = "2.2.0"
            }));

            var sessionData = new Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession
            {
                Id = SessionId,
                PaymentMethodType = "visa"
            };

            mockSessionServiceAccessor
                .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(sessionData);

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            var authRequest = new Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model.AuthenticationRequest()
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
                SettingsVersion = "V11",
                SettingsVersionTryCount = 1,
                MessageVersion = "2.2.0"
            };

            /// Act
            Common.TestContext tc = new Common.TestContext(
                contact: "px.azure.cot",
                retention: DateTime.MaxValue,
                scenarios: string.Empty);

            try
            {
                var authResponse = await paymentSessionsHandler.Authenticate(AcountId, SessionId, authRequest, new List<string>(), new EventTraceActivity(), tc);

                /// Assert
                Assert.AreEqual(authResponse.ChallengeStatus, PaymentChallengeStatus.Failed, "ChallengeStatus is not failed");
            }
            catch
            {
                Assert.Fail("Authenticate call failure");
            }
        }

        [TestMethod]
        public async Task Authenticate_Exception()
        {
            var exposedFlightFeatures = PXPSD2CompFlights.Split(',').ToList();

            mockPayerAuthServiceAccessor.Setup(x => x.Authenticate(
                It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationRequest>(),
                It.IsAny<EventTraceActivity>()))
                .Throws<Exception>();

            var sessionData = new Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession
            {
                Id = SessionId,
                PaymentMethodType = "visa",
                ExposedFlightFeatures = exposedFlightFeatures,
                MethodData = new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.ThreeDSMethodData
                {
                    ThreeDSMethodURL = "fakeurl",
                    ThreeDSServerTransID = "fakeId"
                },
                BrowserInfo = new BrowserInfo
                {
                    ChallengeWindowSize = ChallengeWindowSize.Five
                },
                IsMOTO = false,
                ChallengeStatus = PaymentChallengeStatus.Unknown,
                PaymentInstrumentAccountId = "123",
                PaymentInstrumentId = "456",
                EmailAddress = "fake@fake.gov",
                Amount = 1
            };

            mockSessionServiceAccessor
                .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(sessionData);

            mockSessionServiceAccessor
                .Setup(x => x.UpdateSessionResourceData(It.IsAny<string>(), It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(), It.IsAny<EventTraceActivity>()))
                .Returns(Task.CompletedTask);

            mockTransactionDataServiceAccessor
                .Setup(x => x.UpdateCustomerChallengeAttestation(AcountId, SessionId, true, It.IsAny<EventTraceActivity>()))
                .ReturnsAsync("test")
                .Verifiable();

            mockPims.Setup(x => x.LinkSession(It.IsAny<string>(), It.IsAny<string>(), new PIMSModel.LinkSession(SessionId), It.IsAny<EventTraceActivity>(), null));

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            /// Act
            Common.TestContext tc = new Common.TestContext(
                contact: "px.azure.cot",
                retention: DateTime.MaxValue,
                scenarios: string.Empty);

            try
            {
                var authResponse = await paymentSessionsHandler.Authenticate(SessionId, true, new EventTraceActivity());
                mockSessionServiceAccessor.Verify(x => x.UpdateSessionResourceData(SessionId, It.Is<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(session => session.IsSystemError == true), It.IsAny<EventTraceActivity>()), Times.AtLeastOnce);
            }
            catch
            {
                Assert.Fail("UpdateSessionResourceData not called with session.IsSystemError == true");
            }
        }

        [TestMethod]
        public async Task CompleteThreeDSChallenge_Exception()
        {
            var exposedFlightFeatures = PXPSD2CompFlights.Split(',').ToList();

            var sessionData = new Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession
            {
                Id = SessionId,
                PaymentMethodType = "visa",
                ExposedFlightFeatures = exposedFlightFeatures,
                MethodData = new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.ThreeDSMethodData
                {
                    ThreeDSMethodURL = "fakeurl",
                    ThreeDSServerTransID = "fakeId"
                },
                BrowserInfo = new BrowserInfo
                {
                    ChallengeWindowSize = ChallengeWindowSize.Five
                },
                IsMOTO = false,
                ChallengeStatus = PaymentChallengeStatus.Unknown,
                PaymentInstrumentAccountId = "123",
                PaymentInstrumentId = "456",
                EmailAddress = "fake@fake.gov",
                Amount = 1
            };

            mockSessionServiceAccessor
                .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(sessionData);

            mockSessionServiceAccessor
                .Setup(x => x.UpdateSessionResourceData(It.IsAny<string>(), It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(), It.IsAny<EventTraceActivity>()))
                .Returns(Task.CompletedTask);

            mockPayerAuthServiceAccessor
                .Setup(x => x.CompleteChallenge(It.IsAny<CompletionRequest>(), It.IsAny<EventTraceActivity>()))
                .Throws<Exception>();

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            try
            {
                var threeDsOneChallengeResponse = await paymentSessionsHandler.CompleteThreeDSChallenge(AcountId, SessionId, exposedFlightFeatures, new EventTraceActivity());

                mockSessionServiceAccessor.Verify(x => x.UpdateSessionResourceData(SessionId, It.Is<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(session => session.IsSystemError == true), It.IsAny<EventTraceActivity>()), Times.AtLeastOnce);
            }
            catch
            {
                Assert.Fail("Attestation not called");
            }
        }

        [TestMethod]
        [DataRow("BrowserFlowContext", true)]
        [DataRow("BrowserFlowContext", false)]
        [DataRow("AuthenticationResponse", true)]
        [DataRow("AuthenticationResponse", false)]
        public async Task Authenticate_Success(string scenario, bool includeUmbrellaSafetyNetFlight)
        {
            var exposedFlightFeatures = PXPSD2CompFlights.Split(',').ToList();

            if (includeUmbrellaSafetyNetFlight)
            {
                exposedFlightFeatures.Add("PXPSD2SafetyNetAuthenticate");
            }

            var sessionData = new Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession
            {
                Id = SessionId,
                PaymentMethodType = "visa",
                ExposedFlightFeatures = exposedFlightFeatures,
                BrowserInfo = new BrowserInfo
                {
                    ChallengeWindowSize = ChallengeWindowSize.Five
                },
                MethodData = new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.ThreeDSMethodData
                {
                    ThreeDSMethodURL = "fakeurl",
                    ThreeDSServerTransID = "fakeId"
                },
                ChallengeStatus = PaymentChallengeStatus.Unknown,
                PaymentInstrumentAccountId = "123",
                PaymentInstrumentId = "456",
            };

            mockSessionServiceAccessor
                .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(sessionData);

            mockPayerAuthServiceAccessor.Setup(x => x.Authenticate(
            It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationRequest>(),
            It.IsAny<EventTraceActivity>()))
            .Returns(Task.FromResult(new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationResponse()
            {
                EnrollmentStatus = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentInstrumentEnrollmentStatus.Enrolled,
                EnrollmentType = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentInstrumentEnrollmentType.ThreeDs,
                TransactionStatus = TransactionStatus.Y,
                TransactionStatusReason = TransactionStatusReason.TSR01,
                AcsSignedContent = string.Empty,
                MessageVersion = "2.2.0"
            }));

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            if (scenario == "BrowserFlowContext")
            {
                BrowserFlowContext res = await paymentSessionsHandler.Authenticate(SessionId, true, new EventTraceActivity());
                Assert.AreEqual(res.PaymentSession.ChallengeStatus, PaymentChallengeStatus.Succeeded);
            }
            else if (scenario == "AuthenticationResponse")
            {
                Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model.AuthenticationResponse authenticationResponse = await paymentSessionsHandler.Authenticate(AcountId, SessionId, new AuthenticationRequest(), exposedFlightFeatures, new EventTraceActivity(), null);
                Assert.AreEqual(authenticationResponse.ChallengeStatus, PaymentChallengeStatus.Succeeded);
            }
        }

        [TestMethod]
        [DataRow(PaymentChallengeStatus.Failed, "BrowserFlowContext", "ServiceErrorResponseException", DeviceChannel.Browser, true, true)]
        [DataRow(PaymentChallengeStatus.Succeeded, "BrowserFlowContext", "ServiceErrorResponseException", DeviceChannel.Browser, true, false)]
        [DataRow(PaymentChallengeStatus.Failed, "BrowserFlowContext", "ServiceErrorResponseException", DeviceChannel.Browser, true, false, true, HttpStatusCode.BadRequest, "InvalidRequestData")]
        [DataRow(PaymentChallengeStatus.Failed, "BrowserFlowContext", "ServiceErrorResponseException", DeviceChannel.Browser, true, true, true, HttpStatusCode.BadRequest, "InvalidRequestData")]
        [DataRow(PaymentChallengeStatus.Succeeded, "BrowserFlowContext", "ServiceErrorResponseException", DeviceChannel.Browser, false, true)]
        [DataRow(PaymentChallengeStatus.Succeeded, "BrowserFlowContext", "ServiceErrorResponseException", DeviceChannel.Browser, false, false)]
        [DataRow(PaymentChallengeStatus.Failed, "AuthenticationResponse", "ServiceErrorResponseException", DeviceChannel.AppBased, true, true)]
        [DataRow(PaymentChallengeStatus.Succeeded, "AuthenticationResponse", "ServiceErrorResponseException", DeviceChannel.AppBased, true, false)]
        [DataRow(PaymentChallengeStatus.Failed, "AuthenticationResponse", "ServiceErrorResponseException", DeviceChannel.AppBased, true, false, true, HttpStatusCode.BadRequest, "InvalidRequestData")]
        [DataRow(PaymentChallengeStatus.Failed, "AuthenticationResponse", "ServiceErrorResponseException", DeviceChannel.AppBased, true, true, true, HttpStatusCode.BadRequest, "InvalidRequestData")]
        [DataRow(PaymentChallengeStatus.Succeeded, "AuthenticationResponse", "ServiceErrorResponseException", DeviceChannel.AppBased, false, true)]
        [DataRow(PaymentChallengeStatus.Succeeded, "AuthenticationResponse", "ServiceErrorResponseException", DeviceChannel.AppBased, false, false)]
        [DataRow(PaymentChallengeStatus.Failed, "BrowserFlowContext", "Exception", DeviceChannel.Browser, true, true)]
        [DataRow(PaymentChallengeStatus.Succeeded, "BrowserFlowContext", "Exception", DeviceChannel.Browser, false, true)]
        [DataRow(PaymentChallengeStatus.Succeeded, "BrowserFlowContext", "Exception", DeviceChannel.Browser, false, false)]
        [DataRow(PaymentChallengeStatus.Failed, "AuthenticationResponse", "Exception", DeviceChannel.AppBased, true, true)]
        [DataRow(PaymentChallengeStatus.Succeeded, "AuthenticationResponse", "Exception", DeviceChannel.AppBased, false, true)]
        [DataRow(PaymentChallengeStatus.Succeeded, "AuthenticationResponse", "Exception", DeviceChannel.AppBased, false, false)]
        public async Task Authenticate_Exception_PSD2BankError(PaymentChallengeStatus expectedResult, string scenario, string errorType, DeviceChannel deviceChannel, bool includeUmbrellaSafetyNetFlight, bool includeEnforcementFlight, bool set_ExcludeErrorFeatureFormatFlighting = false, HttpStatusCode statusCode = HttpStatusCode.OK, string errorCode = "")
        {
            var exposedFlightFeatures = PXPSD2CompFlights.Split(',').ToList();

            if (includeUmbrellaSafetyNetFlight)
            {
                exposedFlightFeatures.Add("PXPSD2SafetyNetAuthenticate");
            }

            if (includeEnforcementFlight)
            {
                exposedFlightFeatures.Add("PXPSD2BankErrorEnforcementAuthenticate" + "-" + deviceChannel);
            }

            if (set_ExcludeErrorFeatureFormatFlighting)
            {
                if (scenario == "BrowserFlowContext")
                {
                    exposedFlightFeatures.Add(string.Format("PSD2SafetyNet-BrowserAuthN-{0}-{1}", statusCode.ToString(), errorCode));
                }
                else
                {
                    exposedFlightFeatures.Add(string.Format("PSD2SafetyNet-AppAuthN-{0}-{1}", statusCode.ToString(), errorCode));
                }
            }

            var sessionData = new Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession
            {
                Id = SessionId,
                PaymentMethodType = "visa",
                ExposedFlightFeatures = exposedFlightFeatures,
                BrowserInfo = new BrowserInfo
                {
                    ChallengeWindowSize = ChallengeWindowSize.Five
                },
                MethodData = new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.ThreeDSMethodData
                {
                    ThreeDSMethodURL = "fakeurl",
                    ThreeDSServerTransID = "fakeId"
                },
                ChallengeStatus = PaymentChallengeStatus.Unknown,
                PaymentInstrumentAccountId = "123",
                PaymentInstrumentId = "456",
                DeviceChannel = deviceChannel
            };

            mockSessionServiceAccessor
                .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(sessionData);

            if (errorType == "ServiceErrorResponseException")
            {
                ServiceErrorResponseException ex = new ServiceErrorResponseException()
                {
                    Response = new System.Net.Http.HttpResponseMessage()
                    {
                        StatusCode = statusCode
                    },
                    Error = new ServiceErrorResponse()
                    {
                        ErrorCode = errorCode
                    }
                };

                mockPayerAuthServiceAccessor
                    .Setup(x => x.Authenticate(It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationRequest>(), It.IsAny<EventTraceActivity>()))
                    .Throws(ex);
            }
            else
            {
            mockPayerAuthServiceAccessor
                .Setup(x => x.Authenticate(It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationRequest>(), It.IsAny<EventTraceActivity>()))
                .Throws<Exception>();
            }

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            if (scenario == "BrowserFlowContext")
            {
                BrowserFlowContext res = await paymentSessionsHandler.Authenticate(SessionId, true, new EventTraceActivity());
                Assert.AreEqual(res.PaymentSession.ChallengeStatus, expectedResult);
            }
            else if (scenario == "AuthenticationResponse")
            {
                Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model.AuthenticationResponse authenticationResponse = await paymentSessionsHandler.Authenticate(AcountId, SessionId, new AuthenticationRequest(), exposedFlightFeatures, new EventTraceActivity(), null);
                Assert.AreEqual(authenticationResponse.ChallengeStatus, expectedResult);
            }
        }

        [DataRow("Browser", "ServiceErrorResponseException", true, true, HttpStatusCode.BadRequest, "InvalidRequestData")]
        [DataRow("AppBased", "ServiceErrorResponseException", true, true, HttpStatusCode.BadRequest, "InvalidRequestData")]
        [DataRow("Browser", "ServiceErrorResponseException", true)]
        [DataRow("AppBased", "ServiceErrorResponseException", true)]
        [DataRow("Browser", "Exception", true)]
        [DataRow("AppBased", "Exception", true)]
        [DataRow("Browser", "ServiceErrorResponseException", false, true, HttpStatusCode.BadRequest, "InvalidRequestData")]
        [DataRow("AppBased", "ServiceErrorResponseException", false, true, HttpStatusCode.BadRequest, "InvalidRequestData")]
        [DataRow("Browser", "ServiceErrorResponseException", false)]
        [DataRow("AppBased", "ServiceErrorResponseException", false)]
        [DataRow("Browser", "Exception", false)]
        [DataRow("AppBased", "Exception", false)]
        [TestMethod]
        public async Task CompleteThreeDSChallenge_Exception_PSD2BankError(string deviceChannel, string errorType, bool includeEnforcementFlight, bool set_ExcludeErrorFeatureFormatFlighting = false, HttpStatusCode statusCode = HttpStatusCode.OK, string errorCode = "")
        {
            var exposedFlightFeatures = PXPSD2CompFlights.Split(',').ToList();

            if (includeEnforcementFlight)
            {
                exposedFlightFeatures.Add("PXPSD2BankErrorEnforcementCompleteChallenge" + "-" + deviceChannel);
            }
            
            if (set_ExcludeErrorFeatureFormatFlighting)
            {
                exposedFlightFeatures.Add(string.Format("PSD2SafetyNet-Completion-{0}-{1}-{2}", statusCode.ToString(), errorCode, deviceChannel.ToString()));
            }

            var sessionData = new Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession
            {
                Id = SessionId,
                PaymentMethodType = "visa",
                ExposedFlightFeatures = exposedFlightFeatures,
                BrowserInfo = new BrowserInfo
                {
                    ChallengeWindowSize = ChallengeWindowSize.Five
                },
                ChallengeStatus = PaymentChallengeStatus.Unknown,
                PaymentInstrumentAccountId = "123",
                PaymentInstrumentId = "456",
                Amount = 1,
                DeviceChannel = deviceChannel == "Browser" ? DeviceChannel.Browser : DeviceChannel.AppBased
            };

            mockSessionServiceAccessor
                .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(sessionData);

            mockSessionServiceAccessor
                .Setup(x => x.UpdateSessionResourceData(It.IsAny<string>(), It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(), It.IsAny<EventTraceActivity>()))
                .Returns(Task.CompletedTask);

            if (errorType == "ServiceErrorResponseException")
            {
                ServiceErrorResponseException ex = new ServiceErrorResponseException()
                {
                    Response = new System.Net.Http.HttpResponseMessage()
                    {
                        StatusCode = statusCode
                    },
                    Error = new ServiceErrorResponse()
                    {
                        ErrorCode = errorCode
                    }
                };

                mockPayerAuthServiceAccessor
                .Setup(x => x.CompleteChallenge(It.IsAny<CompletionRequest>(), It.IsAny<EventTraceActivity>()))
                .Throws(ex);
            }
            else
            {
            mockPayerAuthServiceAccessor
                .Setup(x => x.CompleteChallenge(It.IsAny<CompletionRequest>(), It.IsAny<EventTraceActivity>()))
                .Throws<Exception>();
            }

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

                var threeDsOneChallengeResponse = await paymentSessionsHandler.CompleteThreeDSChallenge(AcountId, SessionId, exposedFlightFeatures, new EventTraceActivity());

            if (includeEnforcementFlight || set_ExcludeErrorFeatureFormatFlighting)
            {
                Assert.AreEqual(threeDsOneChallengeResponse.ChallengeStatus, PaymentChallengeStatus.Failed);
            }
            else
            {
                Assert.AreEqual(threeDsOneChallengeResponse.ChallengeStatus, PaymentChallengeStatus.Succeeded);
            }
        }

        [TestMethod]
        [DataRow(PXPSD2CompFlights, AcountId, TransactionStatus.Y, ChallengeCompletionIndicator.Y, null, null, PaymentChallengeStatus.Succeeded)]
        [DataRow(PXPSD2CompFlights, AcountId, TransactionStatus.U, ChallengeCompletionIndicator.Y, null, null, PaymentChallengeStatus.Succeeded)]
        [DataRow(PXPSD2CompFlights, AcountId, TransactionStatus.N, ChallengeCompletionIndicator.Y, null, TransactionStatusReason.TSR01, PaymentChallengeStatus.Succeeded)]
        [DataRow(PXPSD2CompFlights, AcountId, TransactionStatus.A, ChallengeCompletionIndicator.N, null, null, PaymentChallengeStatus.Succeeded)]
        [DataRow(PXPSD2CompFlights, AcountId, TransactionStatus.N, ChallengeCompletionIndicator.Y, null, TransactionStatusReason.TSR10, PaymentChallengeStatus.Failed)]
        [DataRow(PXPSD2CompFlights, AcountId, TransactionStatus.R, ChallengeCompletionIndicator.Y, null, null, PaymentChallengeStatus.Failed)]
        [DataRow(PXPSD2CompFlights, AcountId, TransactionStatus.FR, ChallengeCompletionIndicator.Y, null, null, PaymentChallengeStatus.Failed)]
        public async Task Authenticate_SessionStateTest(
            string flights,
            string accountId,
            TransactionStatus mockTransactionStatus,
            ChallengeCompletionIndicator mockChallengeCompletionIndicator,
            string mockChallengeCancelIndicator = null,
            TransactionStatusReason mockTransactionStatusReason = TransactionStatusReason.TSR01,
            PaymentChallengeStatus expectedChallengeStatus = PaymentChallengeStatus.Succeeded)
        {
            var exposedFlightFeatures = flights.Split(',').ToList();

            mockPayerAuthServiceAccessor.Setup(x => x.Authenticate(
            It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationRequest>(),
            It.IsAny<EventTraceActivity>()))
            .Returns(Task.FromResult(new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationResponse()
            {
                EnrollmentStatus = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentInstrumentEnrollmentStatus.Enrolled,
                EnrollmentType = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentInstrumentEnrollmentType.ThreeDs,
                TransactionStatus = mockTransactionStatus,
                TransactionStatusReason = mockTransactionStatusReason,
                AcsSignedContent = string.Empty,
                MessageVersion = "2.2.0"
            }));

            var sessionData = new Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession
            {
                Id = SessionId,
                PaymentMethodType = "visa",
                ExposedFlightFeatures = exposedFlightFeatures,
                MethodData = new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.ThreeDSMethodData
                {
                    ThreeDSMethodURL = "fakeurl",
                    ThreeDSServerTransID = "fakeId"
                },
                BrowserInfo = new BrowserInfo
                {
                    ChallengeWindowSize = ChallengeWindowSize.Five
                },
                IsMOTO = false,
                ChallengeStatus = PaymentChallengeStatus.Unknown,
                PaymentInstrumentAccountId = "123",
                PaymentInstrumentId = "456",
                EmailAddress = "fake@fake.gov",
                Amount = 1
            };

            mockSessionServiceAccessor
                .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(sessionData);

            mockSessionServiceAccessor
                .Setup(x => x.UpdateSessionResourceData(It.IsAny<string>(), It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(), It.IsAny<EventTraceActivity>()))
                .Returns(Task.CompletedTask);

            mockTransactionDataServiceAccessor
                .Setup(x => x.UpdateCustomerChallengeAttestation(AcountId, SessionId, true, It.IsAny<EventTraceActivity>()))
                .ReturnsAsync("test")
                .Verifiable();

            mockPims.Setup(x => x.LinkSession(It.IsAny<string>(), It.IsAny<string>(), new PIMSModel.LinkSession(SessionId), It.IsAny<EventTraceActivity>(), null));

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            var authRequest = new Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model.AuthenticationRequest()
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
                SettingsVersion = "V11",
                SettingsVersionTryCount = 1,
                MessageVersion = "2.2.0"
            };

            /// Act
            Common.TestContext tc = new Common.TestContext(
                contact: "px.azure.cot",
                retention: DateTime.MaxValue,
                scenarios: string.Empty);

            Func<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession, bool> evaluateSession = (session) =>
            {
                return session.ChallengeStatus == expectedChallengeStatus
                    && session.TransactionStatus == mockTransactionStatus
                    && session.TransactionStatusReason == mockTransactionStatusReason;
            };

            try
            {
                var authResponse = await paymentSessionsHandler.Authenticate(SessionId, true, new EventTraceActivity());

                mockSessionServiceAccessor.Verify(x => x.UpdateSessionResourceData(SessionId, It.Is<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(session => evaluateSession(session)), It.IsAny<EventTraceActivity>()), Times.AtLeastOnce);

                Assert.AreEqual(authResponse.PaymentSession.ChallengeStatus, expectedChallengeStatus, "ChallengeStatus is not succeeded");
            }
            catch
            {
                Assert.Fail("Attestation not called");
            }
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("unrelatedFlight")]
        [DataRow("PXGetAttestationFlightFeaturesFromStoredSession,PXSafetyNetTransactionDataServiceAttestation")]
        public async Task Authenticate_AttestationCalled(string flights)
        {
            var exposedFlightFeatures = flights.Split(',').ToList();

            mockPayerAuthServiceAccessor.Setup(x => x.Authenticate(
            It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationRequest>(),
            It.IsAny<EventTraceActivity>()))
            .Returns(Task.FromResult(new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationResponse()
            {
                EnrollmentStatus = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentInstrumentEnrollmentStatus.Enrolled,
                EnrollmentType = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentInstrumentEnrollmentType.ThreeDs,
                TransactionStatus = TransactionStatus.Y,
                TransactionStatusReason = TransactionStatusReason.TSR01,
                AcsSignedContent = string.Empty,
                MessageVersion = "2.2.0"
            }));

            var sessionData = new Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession
            {
                Id = SessionId,
                PaymentMethodType = "visa",
                ExposedFlightFeatures = exposedFlightFeatures,
                MethodData = new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.ThreeDSMethodData
                {
                    ThreeDSMethodURL = "fakeurl",
                    ThreeDSServerTransID = "fakeId"
                },
                BrowserInfo = new BrowserInfo
                {
                    ChallengeWindowSize = ChallengeWindowSize.Five
                },
                IsMOTO = false,
                ChallengeStatus = PaymentChallengeStatus.Succeeded,
                PaymentInstrumentAccountId = "123",
                PaymentInstrumentId = "456",
                EmailAddress = "fake@fake.gov",
                Amount = 1
            };

            mockSessionServiceAccessor
                .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(sessionData);

            mockTransactionDataServiceAccessor
                .Setup(x => x.UpdateCustomerChallengeAttestation(AcountId, SessionId, true, It.IsAny<EventTraceActivity>()))
                .ReturnsAsync("test")
                .Verifiable();

            mockPims.Setup(x => x.LinkSession(It.IsAny<string>(), It.IsAny<string>(), new PIMSModel.LinkSession(SessionId), It.IsAny<EventTraceActivity>(), null));

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            var authRequest = new Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model.AuthenticationRequest()
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
                SettingsVersion = "V11",
                SettingsVersionTryCount = 1,
                MessageVersion = "2.2.0"
            };

            /// Act
            Common.TestContext tc = new Common.TestContext(
                contact: "px.azure.cot",
                retention: DateTime.MaxValue,
                scenarios: string.Empty);

            try
            {
                var authResponse = await paymentSessionsHandler.Authenticate(SessionId, true, new EventTraceActivity());

                mockTransactionDataServiceAccessor.Verify(x => x.UpdateCustomerChallengeAttestation(It.IsNotNull<string>(), It.IsNotNull<string>(), It.IsAny<bool>(), It.IsAny<EventTraceActivity>()), Times.Once);

                Assert.AreEqual(authResponse.PaymentSession.ChallengeStatus, PaymentChallengeStatus.Succeeded, "ChallengeStatus is not succeeded");
            }
            catch
            {
                Assert.Fail("Attestation not called");
            }
        }

        [TestMethod]
        [DataRow(PXPSD2CompFlights, AcountId, TransactionStatus.Y, ChallengeCompletionIndicator.Y, null, null, PaymentChallengeStatus.Succeeded)]
        [DataRow(PXPSD2CompFlights, AcountId, TransactionStatus.U, ChallengeCompletionIndicator.Y, null, null, PaymentChallengeStatus.Succeeded)]
        [DataRow(PXPSD2CompFlights, AcountId, TransactionStatus.N, ChallengeCompletionIndicator.Y, null, TransactionStatusReason.TSR01, PaymentChallengeStatus.Succeeded)]
        [DataRow(PXPSD2CompFlights, AcountId, TransactionStatus.A, ChallengeCompletionIndicator.N, null, null, PaymentChallengeStatus.Succeeded)]
        [DataRow(PXPSD2CompFlights, AcountId, TransactionStatus.N, ChallengeCompletionIndicator.N, "01", null, PaymentChallengeStatus.Cancelled)]
        [DataRow(PXPSD2CompFlights, AcountId, TransactionStatus.N, ChallengeCompletionIndicator.N, "04", null, PaymentChallengeStatus.TimedOut)]
        [DataRow(PXPSD2CompFlights, AcountId, TransactionStatus.N, ChallengeCompletionIndicator.Y, null, TransactionStatusReason.TSR10, PaymentChallengeStatus.Failed)]
        [DataRow(PXPSD2CompFlights, AcountId, TransactionStatus.R, ChallengeCompletionIndicator.Y, null, null, PaymentChallengeStatus.Failed)]
        [DataRow(PXPSD2CompFlights, AcountId, TransactionStatus.FR, ChallengeCompletionIndicator.Y, null, null, PaymentChallengeStatus.Failed)]
        public async Task CompleteThreeDSChallenge_SessionStateTest(
            string flights,
            string accountId,
            TransactionStatus mockTransactionStatus,
            ChallengeCompletionIndicator mockChallengeCompletionIndicator,
            string mockChallengeCancelIndicator = null,
            TransactionStatusReason mockTransactionStatusReason = TransactionStatusReason.TSR01,
            PaymentChallengeStatus expectedChallengeStatus = PaymentChallengeStatus.Succeeded)
        {
            var exposedFlightFeatures = flights.Split(',').ToList();

            CompletionResponse mockCompletion = new CompletionResponse
            {
                TransactionStatus = mockTransactionStatus,
                ChallengeCompletionIndicator = mockChallengeCompletionIndicator,
                TransactionStatusReason = mockTransactionStatusReason,
                ChallengeCancelIndicator = mockChallengeCancelIndicator
            };

            var sessionData = new Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession
            {
                Id = SessionId,
                PaymentMethodType = "visa",
                ExposedFlightFeatures = exposedFlightFeatures,
                MethodData = new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.ThreeDSMethodData
                {
                    ThreeDSMethodURL = "fakeurl",
                    ThreeDSServerTransID = "fakeId"
                },
                BrowserInfo = new BrowserInfo
                {
                    ChallengeWindowSize = ChallengeWindowSize.Five
                },
                IsMOTO = false,
                ChallengeStatus = PaymentChallengeStatus.Unknown,
                PaymentInstrumentAccountId = "123",
                PaymentInstrumentId = "456",
                EmailAddress = "fake@fake.gov",
                Amount = 1
            };

            mockSessionServiceAccessor
                .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(sessionData);

            mockSessionServiceAccessor
                .Setup(x => x.UpdateSessionResourceData(It.IsAny<string>(), It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(), It.IsAny<EventTraceActivity>()))
                .Returns(Task.CompletedTask);

            mockPayerAuthServiceAccessor
                .Setup(x => x.CompleteChallenge(It.IsAny<CompletionRequest>(), It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(mockCompletion);

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            try
            {
                var threeDsOneChallengeResponse = await paymentSessionsHandler.CompleteThreeDSChallenge(accountId, SessionId, exposedFlightFeatures, new EventTraceActivity());

                ////mockSessionServiceAccessor.Verify(x => x.UpdateSessionResourceData(SessionId, sessionData, It.IsAny<EventTraceActivity>()), Times.Once);
                ////sessionData.ChallengeStatus = mockChallengeStatus;
                mockSessionServiceAccessor.Verify(x => x.UpdateSessionResourceData(SessionId, It.Is<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(session => session.ChallengeStatus == expectedChallengeStatus), It.IsAny<EventTraceActivity>()), Times.AtLeastOnce);

                Assert.AreEqual(threeDsOneChallengeResponse.ChallengeStatus, expectedChallengeStatus, "ChallengeStatus is not succeeded");
            }
            catch
            {
                Assert.Fail("Attestation not called");
            }
        }

        [TestMethod]
        [DataRow("", AcountId)]
        [DataRow("unrelatedFlight,anotherFlight", AcountId)]
        [DataRow("", null)]
        [DataRow("unrelatedFlight,anotherFlight", null)]
        public async Task CompleteThreeDSChallenge_AttestationCalled(string flights, string accountId)
        {
            var exposedFlightFeatures = flights.Split(',').ToList();

            CompletionResponse mockCompletion = new CompletionResponse
            {
                TransactionStatus = TransactionStatus.Y,
                ChallengeCompletionIndicator = ChallengeCompletionIndicator.Y,
            };

            var sessionData = new Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession
            {
                Id = SessionId,
                PaymentMethodType = "visa",
                ExposedFlightFeatures = exposedFlightFeatures,
                MethodData = new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.ThreeDSMethodData
                {
                    ThreeDSMethodURL = "fakeurl",
                    ThreeDSServerTransID = "fakeId"
                },
                BrowserInfo = new BrowserInfo
                {
                    ChallengeWindowSize = ChallengeWindowSize.Five
                },
                IsMOTO = false,
                ChallengeStatus = PaymentChallengeStatus.Succeeded,
                PaymentInstrumentAccountId = "123",
                PaymentInstrumentId = "456",
                EmailAddress = "fake@fake.gov",
                Amount = 1
            };

            mockSessionServiceAccessor
                .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(sessionData);

            mockTransactionDataServiceAccessor
                .Setup(x => x.UpdateCustomerChallengeAttestation(AcountId, SessionId, true, It.IsAny<EventTraceActivity>()))
                .ReturnsAsync("test")
                .Verifiable();

            mockPayerAuthServiceAccessor
                .Setup(x => x.CompleteChallenge(It.IsAny<CompletionRequest>(), It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(mockCompletion);

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            try
            {
                var threeDsOneChallengeResponse = await paymentSessionsHandler.CompleteThreeDSChallenge(accountId, SessionId, exposedFlightFeatures, new EventTraceActivity());

                mockTransactionDataServiceAccessor.Verify(x => x.UpdateCustomerChallengeAttestation(It.IsNotNull<string>(), It.IsNotNull<string>(), true, It.IsAny<EventTraceActivity>()), Times.Once);

                Assert.AreEqual(threeDsOneChallengeResponse.ChallengeStatus, PaymentChallengeStatus.Succeeded, "ChallengeStatus is not succeeded");
            }
            catch
            {
                Assert.Fail("Attestation not called");
            }
        }

        [TestMethod]
        [DataRow(PaymentInstrumentEnrollmentStatus.Bypassed)]
        public async Task Test_CreatePaymentSession_GetThreeDSMethodURL_PayerAuthServiceAccessor_Success(PaymentInstrumentEnrollmentStatus enrollmentStatus)
        {
            // Arrange
            mockPayerAuthServiceAccessor.Setup(x => x.Get3DSMethodURL(
                It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentSession>(),
                It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.ThreeDSMethodData());

            mockPayerAuthServiceAccessor.Setup(x => x.Authenticate(
            It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationRequest>(),
            It.IsAny<EventTraceActivity>()))
            .Returns(Task.FromResult(new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationResponse()
            {
                EnrollmentStatus = enrollmentStatus,
                EnrollmentType = PaymentInstrumentEnrollmentType.ThreeDs,
            }));

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                Language = "en",
                Amount = 0m,
                Currency = "EUR",
                Partner = "webblends",
                Country = "de",
                HasPreOrder = true,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
                BillableAccountId = "blah+",
                ClassicProduct = "fooBar",
                PaymentInstrumentId = ChallengeRequiredPiId
            };

            // Act
            var sessionResult = await paymentSessionsHandler.CreatePaymentSession(
                accountId: AcountId,
                paymentSessionData: paymentSessionData,
                deviceChannel: DeviceChannel.Browser,
                emailAddress: "test@outlook.com",
                exposedFlightFeatures: PXPSD2CompFlights.Split(',').ToList(),
                traceActivityId: new EventTraceActivity(),
                testContext: new Common.TestContext(),
                isMotoAuthorized: "true");

            await paymentSessionsHandler.GetThreeDSMethodURL(AcountId, new BrowserInfo(), sessionResult, new EventTraceActivity());

            Func<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession, bool> evaluateSession = (session) =>
            {
                return session.IsSystemError;
            };

            mockTransactionDataServiceAccessor.Verify(x => x.UpdateCustomerChallengeAttestation(It.IsNotNull<string>(), It.IsNotNull<string>(), It.IsAny<bool>(), It.IsAny<EventTraceActivity>()), Times.AtLeastOnce);
        }

        [TestMethod]
        [DataRow("Authenticate")]
        [DataRow("Get3DSMethodURL")]
        public async Task Test_CreatePaymentSession_GetThreeDSMethodURL_PayerAuthServiceAccessor_Exception(string functionToThrowException)
        {
            // Arrange
            if (functionToThrowException == "Authenticate")
            {
                mockPayerAuthServiceAccessor.Setup(x => x.Get3DSMethodURL(
                    It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentSession>(),
                    It.IsAny<EventTraceActivity>()))
                    .ReturnsAsync(new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.ThreeDSMethodData());

                mockPayerAuthServiceAccessor.Setup(x => x.Authenticate(
                    It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationRequest>(),
                    It.IsAny<EventTraceActivity>()))
                    .Throws<Exception>();
            }
            else
            {
                mockPayerAuthServiceAccessor.Setup(x => x.Get3DSMethodURL(
                    It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentSession>(),
                    It.IsAny<EventTraceActivity>()))
                    .Throws<Exception>();
            }

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                Language = "en",
                Amount = 0m,
                Currency = "EUR",
                Partner = "webblends",
                Country = "de",
                HasPreOrder = true,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
                BillableAccountId = "blah+",
                ClassicProduct = "fooBar",
                PaymentInstrumentId = ChallengeRequiredPiId
            };

            // Act
            var sessionResult = await paymentSessionsHandler.CreatePaymentSession(
                accountId: AcountId,
                paymentSessionData: paymentSessionData,
                deviceChannel: DeviceChannel.Browser,
                emailAddress: "test@outlook.com",
                exposedFlightFeatures: PXPSD2CompFlights.Split(',').ToList(),
                traceActivityId: new EventTraceActivity(),
                testContext: new Common.TestContext(),
                isMotoAuthorized: "true");

            await paymentSessionsHandler.GetThreeDSMethodURL(AcountId, new BrowserInfo(), sessionResult, new EventTraceActivity());

            Func<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession, bool> evaluateSession = (session) =>
            {
                return session.IsSystemError;
            };

            mockSessionServiceAccessor.Verify(x => x.UpdateSessionResourceData(It.IsAny<string>(), It.Is<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(session => evaluateSession(session)), It.IsAny<EventTraceActivity>()), Times.AtLeastOnce);
            mockTransactionDataServiceAccessor.Verify(x => x.UpdateCustomerChallengeAttestation(It.IsNotNull<string>(), It.IsNotNull<string>(), It.IsAny<bool>(), It.IsAny<EventTraceActivity>()), Times.AtLeastOnce);
        }

        [TestMethod]
        [DataRow("CompleteThreeDSChallenge", "GetSessionResourceDataException")]
        [DataRow("CompleteThreeDSChallenge", "CompleteChallengeException")]
        [DataRow("CompleteThreeDSOneChallenge", "GetSessionResourceDataException")]
        [DataRow("CompleteThreeDSOneChallenge", "CompleteChallengeException")]
        public async Task ThreeDSChallenges_TestResponsesWithAPIExceptions(string functionName, string scenario)
        {
            var exposedFlightFeatures = PXPSD2CompFlights.Split(',').ToList();

            if (scenario == "GetSessionResourceDataException")
            {
                mockSessionServiceAccessor
                    .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                    .Throws(new Exception());
            }
            else if (scenario == "CompleteChallengeException")
            {
                var sessionData = new Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession
                {
                    Id = SessionId,
                    PaymentMethodType = "visa",
                    ExposedFlightFeatures = exposedFlightFeatures,
                    MethodData = new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.ThreeDSMethodData
                    {
                        ThreeDSMethodURL = "fakeurl",
                        ThreeDSServerTransID = "fakeId"
                    },
                    BrowserInfo = new BrowserInfo
                    {
                        ChallengeWindowSize = ChallengeWindowSize.Five
                    },
                    IsMOTO = false,
                    ChallengeStatus = PaymentChallengeStatus.Unknown,
                    PaymentInstrumentAccountId = "123",
                    PaymentInstrumentId = "456",
                    EmailAddress = "fake@fake.gov",
                    Amount = 1
                };

                mockSessionServiceAccessor
                    .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                    .ReturnsAsync(sessionData);

                mockPayerAuthServiceAccessor
                    .Setup(x => x.CompleteChallenge(It.IsAny<CompletionRequest>(), It.IsAny<EventTraceActivity>()))
                    .Throws<Exception>();
            }

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            Task<PaymentSession> awaitableTask = null;

            switch (functionName)
            {
                case "CompleteThreeDSChallenge":
                    awaitableTask = paymentSessionsHandler.CompleteThreeDSChallenge(AcountId, SessionId, exposedFlightFeatures, new EventTraceActivity());
                    break;
                case "CompleteThreeDSOneChallenge":
                    awaitableTask = paymentSessionsHandler.CompleteThreeDSOneChallenge(AcountId, SessionId, exposedFlightFeatures, new EventTraceActivity());
                    break;
            }

            var res = await awaitableTask;
            switch (scenario)
            {
                case "GetSessionResourceDataException":
                    mockTransactionDataServiceAccessor.Verify(x => x.UpdateCustomerChallengeAttestation(It.IsNotNull<string>(), It.IsNotNull<string>(), It.IsAny<bool>(), It.IsAny<EventTraceActivity>()), Times.AtLeastOnce);

                    // Check for SafetyNetPaymentSession
                    Assert.AreEqual(res.Id, SessionId);
                    Assert.IsTrue(res.IsChallengeRequired);
                    Assert.AreEqual(res.ChallengeStatus, PaymentChallengeStatus.Succeeded);
                    break;

                case "CompleteChallengeException":

                    if (scenario == "GetSessionResourceDataException")
                    {
                        mockTransactionDataServiceAccessor.Verify(x => x.UpdateCustomerChallengeAttestation(It.IsNotNull<string>(), It.IsNotNull<string>(), It.IsAny<bool>(), It.IsAny<EventTraceActivity>()), Times.AtLeastOnce);
                    }

                    // Check for SafetyNetPaymentSession
                    Assert.AreEqual(res.Id, SessionId);
                    Assert.IsFalse(res.IsChallengeRequired);
                    Assert.IsTrue((functionName == "CompleteThreeDSChallenge" && res.ChallengeStatus == PaymentChallengeStatus.Succeeded) ||
                                    (functionName == "CompleteThreeDSOneChallenge" && res.ChallengeStatus == PaymentChallengeStatus.InternalServerError));
                    Assert.AreEqual(res.ChallengeScenario, ChallengeScenario.PaymentTransaction);
                    break;
            }
        }

        [TestMethod]
        [DataRow("GetSessionResourceDataException")]
        [DataRow("AuthenticateException")]
        public async Task Authenticate_TestResponsesWithAPIExceptions(string scenario)
        {
            var exposedFlightFeatures = PXPSD2CompFlights.Split(',').ToList();

            if (scenario == "GetSessionResourceDataException")
            {
                mockSessionServiceAccessor
                    .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                    .Throws(new Exception());
            }
            else if (scenario == "CompleteChallengeException")
            {
                var sessionData = new Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession
                {
                    Id = SessionId,
                    PaymentMethodType = "visa",
                    ExposedFlightFeatures = exposedFlightFeatures,
                    MethodData = new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.ThreeDSMethodData
                    {
                        ThreeDSMethodURL = "fakeurl",
                        ThreeDSServerTransID = "fakeId"
                    },
                    BrowserInfo = new BrowserInfo
                    {
                        ChallengeWindowSize = ChallengeWindowSize.Five
                    },
                    IsMOTO = false,
                    ChallengeStatus = PaymentChallengeStatus.Unknown,
                    PaymentInstrumentAccountId = "123",
                    PaymentInstrumentId = "456",
                    EmailAddress = "fake@fake.gov",
                    Amount = 1
                };

                mockSessionServiceAccessor
                    .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                    .ReturnsAsync(sessionData);

                mockPayerAuthServiceAccessor
                    .Setup(x => x.CompleteChallenge(It.IsAny<CompletionRequest>(), It.IsAny<EventTraceActivity>()))
                    .Throws<Exception>();
            }

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model.AuthenticationResponse authenticationResponse = await paymentSessionsHandler.Authenticate(AcountId, SessionId, new AuthenticationRequest(), exposedFlightFeatures, new EventTraceActivity(), null);

            mockTransactionDataServiceAccessor.Verify(x => x.UpdateCustomerChallengeAttestation(It.IsNotNull<string>(), It.IsNotNull<string>(), It.IsAny<bool>(), It.IsAny<EventTraceActivity>()), Times.Once);

            // Check for SafetyNetPaymentSession
            Assert.AreEqual(authenticationResponse.EnrollmentStatus, PaymentInstrumentEnrollmentStatus.Bypassed);
            Assert.AreEqual(authenticationResponse.ChallengeStatus, PaymentChallengeStatus.Succeeded);
        }

        [TestMethod]
        [DataRow("GetSessionResourceDataException")]
        [DataRow("AuthenticateException")]
        public async Task Authenticate_BrowserFlowContext_TestResponsesWithAPIExceptions(string scenario)
        {
            var exposedFlightFeatures = PXPSD2CompFlights.Split(',').ToList();

            if (scenario == "GetSessionResourceDataException")
            {
                mockSessionServiceAccessor
                    .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                    .Throws(new Exception());
            }
            else if (scenario == "AuthenticateException")
            {
                var sessionData = new Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession
                {
                    Id = SessionId,
                    PaymentMethodType = "visa",
                    ExposedFlightFeatures = exposedFlightFeatures,
                    MethodData = new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.ThreeDSMethodData
                    {
                        ThreeDSMethodURL = "fakeurl",
                        ThreeDSServerTransID = "fakeId"
                    },
                    BrowserInfo = new BrowserInfo
                    {
                        ChallengeWindowSize = ChallengeWindowSize.Five
                    },
                    IsMOTO = false,
                    ChallengeStatus = PaymentChallengeStatus.Unknown,
                    PaymentInstrumentAccountId = "123",
                    PaymentInstrumentId = "456",
                    EmailAddress = "fake@fake.gov",
                    Amount = 1,
                    PiRequiresAuthentication = true,
                };

                mockSessionServiceAccessor
                    .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                    .ReturnsAsync(sessionData);

                mockPayerAuthServiceAccessor
                    .Setup(x => x.Authenticate(It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationRequest>(), It.IsAny<EventTraceActivity>()))
                    .Throws<Exception>();
            }

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            var session = new PaymentSession()
            {
                Id = SessionId,
                IsChallengeRequired = true,
                ChallengeStatus = PaymentChallengeStatus.Succeeded
            };

            BrowserFlowContext res = await paymentSessionsHandler.Authenticate(SessionId, true, new EventTraceActivity());

            // Should not get called if this.storedSession is null
            Times numberOfTimes = scenario == "GetSessionResourceDataException" ? Times.Never() : Times.AtLeastOnce();
            mockSessionServiceAccessor.Verify(x => x.UpdateSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(), It.IsAny<EventTraceActivity>()), numberOfTimes);

            // Check for SafetyNetPaymentSession
            Assert.AreEqual(res.IsFingerPrintRequired, false);
            Assert.AreEqual(res.IsAcsChallengeRequired, false);
            Assert.AreEqual(res.PaymentSession.Id, SessionId);
            Assert.IsTrue(res.PaymentSession.IsChallengeRequired);
            Assert.AreEqual(res.PaymentSession.ChallengeStatus, PaymentChallengeStatus.Succeeded);
        }

        [TestMethod]
        [DataRow("GetSessionResourceDataException")]
        [DataRow("Get3DSMethodURLException")]
        [DataRow("AuthenticateException")]
        public async Task GetThreeDSMethodURL_BrowserFlowContext_TestResponsesWithAPIExceptions(string scenario)
        {
            var exposedFlightFeatures = PXPSD2CompFlights.Split(',').ToList();

            var sessionData = new Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession
            {
                Id = SessionId,
                PaymentMethodType = "visa",
                ExposedFlightFeatures = exposedFlightFeatures,
                MethodData = new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.ThreeDSMethodData
                {
                    ThreeDSMethodURL = "fakeurl",
                    ThreeDSServerTransID = "fakeId"
                },
                BrowserInfo = new BrowserInfo
                {
                    ChallengeWindowSize = ChallengeWindowSize.Five
                },
                IsMOTO = false,
                ChallengeStatus = PaymentChallengeStatus.Unknown,
                PaymentInstrumentAccountId = "123",
                PaymentInstrumentId = "456",
                EmailAddress = "fake@fake.gov",
                Amount = 1,
                PiRequiresAuthentication = true,
            };

            if (scenario == "GetSessionResourceDataException")
            {
                mockSessionServiceAccessor
                    .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                    .Throws(new Exception());
            }
            else if (scenario == "Get3DSMethodURLException")
            {
                mockSessionServiceAccessor
                    .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                    .ReturnsAsync(sessionData);

                mockPayerAuthServiceAccessor.Setup(x => x.Get3DSMethodURL(
                    It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentSession>(),
                    It.IsAny<EventTraceActivity>()))
                     .Throws(new Exception());
            }
            else if (scenario == "AuthenticateException")
            {
                mockSessionServiceAccessor
                    .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                    .ReturnsAsync(sessionData);

                mockPayerAuthServiceAccessor.Setup(x => x.Get3DSMethodURL(
                    It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.PaymentSession>(),
                    It.IsAny<EventTraceActivity>()))
                    .ReturnsAsync(new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.ThreeDSMethodData());

                mockPayerAuthServiceAccessor
                    .Setup(x => x.Authenticate(It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationRequest>(), It.IsAny<EventTraceActivity>()))
                    .Throws<Exception>();
            }

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            var session = new PaymentSession()
            {
                Id = SessionId,
                IsChallengeRequired = true,
                ChallengeStatus = PaymentChallengeStatus.Succeeded
            };

            BrowserFlowContext res = await paymentSessionsHandler.GetThreeDSMethodURL(AcountId, new BrowserInfo(), session, new EventTraceActivity());

            // Should not get called if this.storedSession is null
            mockSessionServiceAccessor.Verify(x => x.UpdateSessionResourceData<PaymentSession>(It.IsAny<string>(), It.IsAny<PaymentSession>(), It.IsAny<EventTraceActivity>()), Times.Never);

            // Check for SafetyNetPaymentSession
            Assert.AreEqual(res.IsFingerPrintRequired, false);
            Assert.AreEqual(res.IsAcsChallengeRequired, false);
            Assert.AreEqual(res.PaymentSession.Id, SessionId);
            Assert.IsTrue(res.PaymentSession.IsChallengeRequired);
            Assert.AreEqual(res.PaymentSession.ChallengeStatus, PaymentChallengeStatus.Succeeded);
        }

        [DataRow("commercialstores", false, false, null, Constants.TransactionServiceStore.OMS)]
        [DataRow("commercialstores", true, false, null, Constants.TransactionServiceStore.OMS)]
        [DataRow("commercialstores", true, true, "useOmsTransactionServiceStore", Constants.TransactionServiceStore.OMS)]
        [DataRow("azure", false, false, null, Constants.TransactionServiceStore.Azure)]
        [DataRow("azure", true, false, null, Constants.TransactionServiceStore.Azure)]
        [DataRow("azure", true, true, "useAzureTransactionServiceStore", Constants.TransactionServiceStore.Azure)]
        [DataRow("azuresignup", false, false, null, Constants.TransactionServiceStore.Azure)]
        [DataRow("azuresignup", true, false, null, Constants.TransactionServiceStore.Azure)]
        [DataRow("azuresignup", true, true, "useAzureTransactionServiceStore", Constants.TransactionServiceStore.Azure)]
        [DataRow("officesmb", false, false, null, Constants.TransactionServiceStore.Azure)]
        [DataRow("officesmb", true, false, null, Constants.TransactionServiceStore.Azure)]
        [DataRow("officesmb", true, true, "useOmsTransactionServiceStore", Constants.TransactionServiceStore.OMS)]
        [DataRow("officesmb", true, true, "useAzureTransactionServiceStore", Constants.TransactionServiceStore.Azure)]
        [TestMethod]
        public void Test_GetTransactionServiceStore_Feature(string partner, bool usePSS, bool useFeature, string featureName = null, string expectedResult = null)
        {
            PaymentExperienceSetting setting = null;

            if (usePSS)
            {
                setting = new PaymentExperienceSetting();

                FeatureConfig config = new FeatureConfig();
                config.ApplicableMarkets = new List<string>();
                config.DisplayCustomizationDetail = new List<DisplayCustomizationDetail>();

                setting.Features = new Dictionary<string, FeatureConfig>();

                if (useFeature)
                {
                    Assert.IsNotNull(featureName, "featureName is required when useFeature is TRUE");
                    Assert.IsNotNull(expectedResult, "expectedResult is required when useFeature is TRUE");

                    setting.Features.Add(featureName, config);
                }
            }

            // Act
            var result = PaymentSessionsHandlerV2.GetTransactionServiceStore(partner, setting);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(string.Equals(result, expectedResult, StringComparison.OrdinalIgnoreCase), $"expected '{expectedResult}', but saw '{result}'");
        }

        [TestMethod]
        [DataRow("xboxsettings", true, true, "jp", "JPY", true)]
        [DataRow("xboxsettings", true, false, "jp", "JPY", false)]
        [DataRow("xboxsettings", false, true, "jp", "JPY", false)]
        [DataRow("xboxsettings", true, false, "us", "EUR", false)]
        [DataRow("xboxsettings", false, false, "jp", "JPY", false)]
        public async Task CreatePaymentSession_Challenge_JCB(string partner, bool v25IsFlighted, bool showChallengeIsFlighted, string market, string currency, bool challengeRequired)
        {
            var challengeNotRequiredPi = new PIMSModel.PaymentInstrument()
            {
                PaymentInstrumentDetails = new PIMSModel.PaymentInstrumentDetails()
                {
                    RequiredChallenge = new List<string> { "3ds2" }
                },
                PaymentMethod = new PIMSModel.PaymentMethod()
                {
                    PaymentMethodFamily = "credit_card",
                    PaymentMethodType = "jcb"
                },
            };

            mockPims.Setup(x => x.GetPaymentInstrument(
               It.IsAny<string>(),
               ChallengeNotRequiredPiId,
               It.IsAny<EventTraceActivity>(),
               It.IsAny<string>(),
               It.IsAny<string>(),
               It.IsAny<string>(),
               It.IsAny<List<string>>(),
               It.IsAny<PaymentExperienceSetting>()))
               .Returns(Task.FromResult(challengeNotRequiredPi));

            mockPims.Setup(x => x.GetExtendedPaymentInstrument(
                ChallengeNotRequiredPiId,
                It.IsAny<EventTraceActivity>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>()))
                .Returns(Task.FromResult(challengeNotRequiredPi));

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            var flights = "PXPSD2ProdIntegration".Split(',').ToList();

            // TODO: Remove unused logic after flighting/testing  https://microsoft.visualstudio.com/OSGS/_boards/board/t/DPX/Deliverables%20and%20Task%20Groups/?workitem=55766231
            if (v25IsFlighted)
            {
                flights.Add("PXPSD2SettingVersionV25");
            }

            if (showChallengeIsFlighted)
            {
                flights.Add("PXDisplayJCBChallenge");
            }

            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                Language = "en",
                Amount = 10.0m,
                Currency = currency,
                Partner = partner,
                Country = market,
                HasPreOrder = false,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
                BillableAccountId = "blah+",
                ClassicProduct = "fooBar",
                PaymentInstrumentId = ChallengeNotRequiredPiId,
            };

            Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession testSession = null;

            mockSessionServiceAccessor
                .Setup(x => x.CreateSessionFromData(It.IsAny<string>(), It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(), It.IsAny<EventTraceActivity>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await paymentSessionsHandler.CreatePaymentSession(
                accountId: AcountId,
                paymentSessionData: paymentSessionData,
                deviceChannel: DeviceChannel.Browser,
                emailAddress: "test@outlook.com",
                exposedFlightFeatures: flights,
                traceActivityId: new EventTraceActivity(),
                testContext: new Microsoft.Commerce.Payments.Common.Transaction.TestContext(),
                setting: null);

            if (challengeRequired)
            {
                Assert.IsTrue(result.IsChallengeRequired);
            }
            else
            {
                Assert.IsFalse(result.IsChallengeRequired);
                mockTransactionDataServiceAccessor.Verify(x => x.UpdateCustomerChallengeAttestation(It.IsNotNull<string>(), It.IsNotNull<string>(), It.IsAny<bool>(), It.IsAny<EventTraceActivity>()), Times.Once);
            }

            Func<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession, bool> evaluateAccountId = (paymentSession) =>
            {
                testSession = paymentSession;
                return paymentSession.PaymentInstrumentAccountId == AcountId;
            };

            mockSessionServiceAccessor.Verify(x => x.CreateSessionFromData(It.IsAny<string>(), It.Is<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(session => evaluateAccountId(session)), It.IsAny<EventTraceActivity>()), Times.AtLeastOnce);
            mockSessionServiceAccessor
                .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(testSession);

            var res = await paymentSessionsHandler.GetStoredSession(result.Id, new EventTraceActivity());

            Assert.IsNotNull(res);
        }

        [TestMethod]
        [DataRow("BrowserFlowContext", PaymentChallengeStatus.NotApplicable)]
        [DataRow("BrowserFlowContext", PaymentChallengeStatus.ByPassed)]
        [DataRow("BrowserFlowContext", PaymentChallengeStatus.Succeeded)]
        [DataRow("BrowserFlowContext", PaymentChallengeStatus.Unknown)]
        [DataRow("AuthenticationResponse", PaymentChallengeStatus.NotApplicable)]
        [DataRow("AuthenticationResponse", PaymentChallengeStatus.ByPassed)]
        [DataRow("AuthenticationResponse", PaymentChallengeStatus.Succeeded)]
        [DataRow("AuthenticationResponse", PaymentChallengeStatus.Unknown)]
        public async Task PayerAuthFailed_Authenticate_CheckingAttestationFallback(string scenario, PaymentChallengeStatus challengeStatus)
        {
            var exposedFlightFeatures = PXPSD2CompFlights.Split(',').ToList();

            var sessionData = new Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession
            {
                Id = SessionId,
                PaymentMethodType = "visa",
                ExposedFlightFeatures = exposedFlightFeatures,
                MethodData = new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.ThreeDSMethodData
                {
                    ThreeDSMethodURL = "fakeurl",
                    ThreeDSServerTransID = "fakeId"
                },
                BrowserInfo = new BrowserInfo
                {
                    ChallengeWindowSize = ChallengeWindowSize.Five
                },
                IsMOTO = false,
                ChallengeStatus = challengeStatus,
                PaymentInstrumentAccountId = "123",
                PaymentInstrumentId = "456",
                EmailAddress = "fake@fake.gov",
                Amount = 1
            };

            mockSessionServiceAccessor
                .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(sessionData);

            mockPayerAuthServiceAccessor
                .Setup(x => x.Authenticate(It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationRequest>(), It.IsAny<EventTraceActivity>()))
                .Throws<Exception>();

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            if (scenario == "BrowserFlowContext")
            {
                BrowserFlowContext res = await paymentSessionsHandler.Authenticate(SessionId, true, new EventTraceActivity());
                mockTransactionDataServiceAccessor.Verify(x => x.UpdateCustomerChallengeAttestation(It.IsNotNull<string>(), It.IsNotNull<string>(), It.IsAny<bool>(), It.IsAny<EventTraceActivity>()), Times.Once);
                Assert.AreEqual(res.PaymentSession.ChallengeStatus, PaymentChallengeStatus.Succeeded);
            }
            else if (scenario == "AuthenticationResponse")
            {
                Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model.AuthenticationResponse authenticationResponse = await paymentSessionsHandler.Authenticate(AcountId, SessionId, new AuthenticationRequest(), exposedFlightFeatures, new EventTraceActivity(), null);

                mockTransactionDataServiceAccessor.Verify(x => x.UpdateCustomerChallengeAttestation(It.IsNotNull<string>(), It.IsNotNull<string>(), It.IsAny<bool>(), It.IsAny<EventTraceActivity>()), Times.Once);

                // Check for SafetyNetPaymentSession
                Assert.AreEqual(authenticationResponse.EnrollmentStatus, PaymentInstrumentEnrollmentStatus.Bypassed);
                Assert.AreEqual(authenticationResponse.ChallengeStatus, PaymentChallengeStatus.Succeeded);
            }
        }

        [TestMethod]
        [DataRow("FR")]
        [DataRow("CompleteThreeDSChallenge")]
        public async Task PayerAuth_TransStatus_FR_FailedResult(string scenario)
        {
            var exposedFlightFeatures = PXPSD2CompFlights.Split(',').ToList();

            var sessionData = new Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession
            {
                Id = SessionId,
                PaymentMethodType = "visa",
                ExposedFlightFeatures = exposedFlightFeatures,
                MethodData = new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.ThreeDSMethodData
                {
                    ThreeDSMethodURL = "fakeurl",
                    ThreeDSServerTransID = "fakeId"
                },
                BrowserInfo = new BrowserInfo
                {
                    ChallengeWindowSize = ChallengeWindowSize.Five
                },
                IsMOTO = false,
                ChallengeStatus = PaymentChallengeStatus.ByPassed,
                PaymentInstrumentAccountId = "123",
                PaymentInstrumentId = "456",
                EmailAddress = "fake@fake.gov",
                Amount = 1
            };

            mockSessionServiceAccessor
                .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(sessionData);

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            if (scenario == "FR")
            {
                mockPayerAuthServiceAccessor.Setup(x => x.Authenticate(
                It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationRequest>(),
                It.IsAny<EventTraceActivity>()))
                .Returns(Task.FromResult(new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationResponse()
                {
                    TransactionStatus = Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.TransactionStatus.FR,
                    EnrollmentStatus = PaymentInstrumentEnrollmentStatus.NotEnrolled,
                    EnrollmentType = PaymentInstrumentEnrollmentType.ThreeDs,
                    IsFormPostAcsUrl = false,
                    IsFullPageRedirect = false
                }));

                Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model.AuthenticationResponse authenticationResponse = await paymentSessionsHandler.Authenticate(AcountId, SessionId, new AuthenticationRequest(), exposedFlightFeatures, new EventTraceActivity(), null);
                Assert.AreEqual(PaymentChallengeStatus.Failed, authenticationResponse.ChallengeStatus);
            }
            else if (scenario == "CompleteThreeDSChallenge")
            {
                CompletionResponse mockCompletion = new CompletionResponse
                {
                    TransactionStatus = TransactionStatus.FR,
                };

                mockPayerAuthServiceAccessor
                .Setup(x => x.CompleteChallenge(It.IsAny<CompletionRequest>(), It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(mockCompletion);

                PaymentSession authenticationResponse = await paymentSessionsHandler.CompleteThreeDSChallenge(AcountId, SessionId, exposedFlightFeatures, new EventTraceActivity());

                Assert.AreEqual(PaymentChallengeStatus.Failed, authenticationResponse.ChallengeStatus);
            }
        }

        [TestMethod]
        [DataRow("jcb", "jcb", PaymentChallengeStatus.Failed)]
        [DataRow("visa", "jcb", PaymentChallengeStatus.Succeeded)]
        public async Task CompleteThreeDSChallenge_N19TransStatusFailure(string originalPaymentMethodType, string flightedPaymentMethodType, PaymentChallengeStatus expectedPaymentChallengeStatus)
        {
            var exposedFlightFeatures = PXPSD2CompFlights.Split(',').ToList();

            // If the flightedPaymentMethodType MATCHES the sessionId paymentMethodType, we expect the N19 status to FAIL the challenge
            // Otherwise, proceed as usual 
            exposedFlightFeatures.Add("PSD2_N_TSR19_FailCardType_" + flightedPaymentMethodType);

            CompletionResponse mockCompletion = new CompletionResponse
            {
                TransactionStatus = TransactionStatus.N,
                TransactionStatusReason = TransactionStatusReason.TSR19
            };

            var sessionData = new Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession
            {
                Id = SessionId,
                PaymentMethodType = originalPaymentMethodType,
                ExposedFlightFeatures = exposedFlightFeatures,
                ChallengeStatus = PaymentChallengeStatus.Succeeded,
            };

            mockSessionServiceAccessor
                .Setup(x => x.GetSessionResourceData<Microsoft.Commerce.Payments.PXService.Model.PXInternal.PaymentSession>(It.IsAny<string>(), It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(sessionData);

            mockTransactionDataServiceAccessor
                .Setup(x => x.UpdateCustomerChallengeAttestation(AcountId, SessionId, true, It.IsAny<EventTraceActivity>()))
                .ReturnsAsync("test")
                .Verifiable();

            mockPayerAuthServiceAccessor
                .Setup(x => x.CompleteChallenge(It.IsAny<CompletionRequest>(), It.IsAny<EventTraceActivity>()))
                .ReturnsAsync(mockCompletion);

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            var threeDsOneChallengeResponse = await paymentSessionsHandler.CompleteThreeDSChallenge(AcountId, SessionId, exposedFlightFeatures, new EventTraceActivity());

            Assert.AreEqual(threeDsOneChallengeResponse.ChallengeStatus, expectedPaymentChallengeStatus, "ChallengeStatus is not succeeded");
        }

        [TestMethod]
        [DataRow(true, TransactionStatus.N, TransactionStatusReason.TSR09, PaymentChallengeStatus.Failed)]
        [DataRow(false, TransactionStatus.N, TransactionStatusReason.TSR09, PaymentChallengeStatus.Succeeded)]
        [DataRow(true, TransactionStatus.Y, TransactionStatusReason.TSR01, PaymentChallengeStatus.Succeeded)]
        [DataRow(false, TransactionStatus.Y, TransactionStatusReason.TSR01, PaymentChallengeStatus.Succeeded)]
        public async Task CreatePaymentSession_ChallengeRequired_Authenticate_N09Failure(
            bool isFlighted,
            TransactionStatus transStatus,
            TransactionStatusReason transStatusReason,
            PaymentChallengeStatus expected_authChallengeStatus)
        {
            // Initialize PI with Cvv challenge
            defaultCreditCardChallenge = true;
            TestInitialize();

            var paymentSessionsHandler = new PaymentSessionsHandlerV2(
                mockPayerAuthServiceAccessor.Object,
                mockPims.Object,
                mockSessionServiceAccessor.Object,
                mockAccountServiceAccessor.Object,
                mockPurchaseServiceAccessor.Object,
                mockTransactionServiceAccessor.Object,
                mockTransactionDataServiceAccessor.Object,
                PifdBaseUrl);

            var flights = new List<string>();

            if (isFlighted)
            {
                flights.Add("PXPSD2Auth-N-TSR09-Failed");
            }

            PaymentSessionData paymentSessionData = new PaymentSessionData()
            {
                Language = "en",
                Amount = 0m,
                Currency = "EUR",
                Partner = "webblends",
                Country = "de",
                HasPreOrder = true,
                ChallengeScenario = ChallengeScenario.PaymentTransaction,
                ChallengeWindowSize = ChallengeWindowSize.Four,
                IsMOTO = false,
                BillableAccountId = "blah+",
                ClassicProduct = "fooBar",
                PaymentInstrumentId = ChallengeRequiredPiId
            };

                paymentSessionData.Partner = "cart";

                // Act
                var sessionResult = await paymentSessionsHandler.CreatePaymentSession(
                    accountId: AcountId,
                    paymentSessionData: paymentSessionData,
                    deviceChannel: DeviceChannel.Browser,
                    emailAddress: "test@outlook.com",
                    exposedFlightFeatures: isFlighted ? flights : new List<string>(),
                    traceActivityId: new EventTraceActivity(),
                    testContext: new Common.TestContext(),
                    isMotoAuthorized: "true");

            mockPayerAuthServiceAccessor.Setup(x => x.Authenticate(
            It.IsAny<Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationRequest>(),
            It.IsAny<EventTraceActivity>()))
            .Returns(Task.FromResult(new Microsoft.Commerce.Payments.PXService.Model.PayerAuthService.AuthenticationResponse()
            {
                TransactionStatusReason = transStatusReason,
                TransactionStatus = transStatus
            }));

            var authResult = await paymentSessionsHandler.Authenticate(AcountId, sessionResult.Id, new AuthenticationRequest(), flights, new EventTraceActivity(), null);

            // Assert
            Assert.AreEqual(expected_authChallengeStatus, authResult.ChallengeStatus);
            
            // Reset the challenge type for PI
            defaultCreditCardChallenge = false;
            TestInitialize();
        }
    }
}