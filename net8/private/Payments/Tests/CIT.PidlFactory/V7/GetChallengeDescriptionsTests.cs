// <copyright file="GetChallengeDescriptionsTests.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace CIT.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Helpers;
    using Microsoft.Commerce.Payments.PidlFactory;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Constants = Microsoft.Commerce.Payments.PXCommon.Constants;

    [TestClass]
    public class GetChallengeDescriptionsTests
    {
        [TestMethod]
        public void PidlFactoryGetCvvChallengeForPi()
        {
            string language = "en-us";
            string sessionId = "1234-1234-1234-1234";
            string challengeType = "cvv";
            List<string> partners = new List<string> { "oxowebdirect", "webblends", "webblends_inline", "cart", "bingtravel" };

            PaymentInstrument mastercard = JsonConvert.DeserializeObject<PaymentInstrument>(TestConstants.PaymentInstruments.Mastercard);

            foreach (string partner in partners)
            {
                List<PIDLResource> challengePidl = PIDLResourceFactory.Instance.GetChallengeDescriptionsForPi(mastercard, challengeType, language, partner, sessionId);
                PidlAssert.IsValid(challengePidl);

                ImageDisplayHint cardLogo = challengePidl[0].GetDisplayHintById(TestConstants.ChallengeDisplayHintIds.CardLogo) as ImageDisplayHint;
                Assert.IsNotNull(cardLogo, "Card logo display hint was not found");

                TextDisplayHint cardName = challengePidl[0].GetDisplayHintById(TestConstants.ChallengeDisplayHintIds.CardName) as TextDisplayHint;

                Assert.IsNotNull(cardName, "Card name display hint was not found");
                Assert.AreEqual(mastercard.PaymentInstrumentDetails.CardHolderName, cardName.DisplayContent, "Card holder name display content was not as expected");
            }
        }

        [TestMethod]
        public void PidlFactoryGetThreeDSChallengeForPi()
        {
            string language = "en-us";
            string sessionId = "1234-1234-1234-1234";
            string challengeType = "threeds";
            List<string> partners = new List<string> { "oxowebdirect", "webblends", "webblends_inline", "defaulttemplate" };

            PaymentInstrument mastercard = JsonConvert.DeserializeObject<PaymentInstrument>(TestConstants.PaymentInstruments.Mastercard);

            foreach (string partner in partners)
            {
                List<PIDLResource> challengePidl = PIDLResourceFactory.Instance.GetChallengeDescriptionsForPi(mastercard, challengeType, language, partner, sessionId);
                PidlAssert.IsValid(challengePidl);

                ButtonDisplayHint nextButton = challengePidl[0].GetDisplayHintById(TestConstants.ButtonDisplayHintIds.ContinueRedirectButton) as ButtonDisplayHint;

                Assert.IsNotNull(nextButton, "Next button was not found");
                Assert.IsNotNull(nextButton.Action.Context, "Redirect url for next button was not set");
                Assert.IsNotNull(nextButton.Action.NextAction, "MoveNext action for next button was not set");
            }
        }

        [TestMethod]
        public void PidlFactoryGetSmsChallengeForPi()
        {
            string language = "en-us";
            string sessionId = "1234-1234-1234-1234";
            string challengeType = "sms";
            List<string> partners = new List<string> { "oxowebdirect", "webblends", "webblends_inline" };

            PaymentInstrument nonSimMobi = JsonConvert.DeserializeObject<PaymentInstrument>(TestConstants.PaymentInstruments.NonSimMobi);

            foreach (string partner in partners)
            {
                List<PIDLResource> challengePidl = PIDLResourceFactory.Instance.GetChallengeDescriptionsForPi(nonSimMobi, challengeType, language, partner, sessionId);
                PidlAssert.IsValid(challengePidl);

                TextDisplayHint phoneNumber = challengePidl[0].GetDisplayHintById(TestConstants.DisplayHintIds.SmsChallengeText) as TextDisplayHint;
                Assert.IsTrue(phoneNumber.DisplayContent.Contains(nonSimMobi.PaymentInstrumentDetails.Msisdn), "Phone number text was not replaced");

                ButtonDisplayHint nextButton = challengePidl[0].GetDisplayHintById(TestConstants.ButtonDisplayHintIds.OkActionButton) as ButtonDisplayHint;
                Assert.IsNotNull(nextButton, "Next button was not found");
                Assert.IsNotNull(nextButton.Action.Context, "RestAction for next button was not set");
                Assert.IsNotNull(nextButton.Action.NextAction, "MoveNext action for next button was not set");
            }
        }

        [TestMethod]
        public void PidlFactoryAddAlipayQrCodeChallenge()
        {
            const string Language = "en-us";
            const string AppSignUrl = "alipays://platformapi/startapp?appId=20000067&url=https://intlmapi.alipay.com/gateway.do?sign=ec3cfbd76158bee6784715a54ee6965e&sign_type=MD5&_input_charset=utf-8&ack_type=M&external_sign_no=IhqaedHquEqM1JxIvs3Bg1lNkhU%3d&notify_url=https%3a%2f%2fonepay.cp.microsoft-int.com%2fInstrumentManagementNotificationService%2fAliPayBillingAgreement&partner=2088801766902304&product_code=GENERAL_WITHHOLDING_P&return_url=http%3a%2f%2fpmservices.cp.microsoft-int.com%2fRedirectionService%2fCoreRedirection%2fCallback%2fPSS_0001613c3171c1134d2fbd0fd7ad3a588b06%2fsuccess&scene=INDUSTRY%7cAPPSTORE&service=alipay.dut.customer.agreement.sign&third_party_type=PARTNER";
            const string RedirectUrl = "https://pmservices.cp.microsoft-int.com/RedirectionService/CoreRedirection/Redirect/PSS_0001613c3171c1134d2fbd0fd7ad3a588b06";
            const string PendingOn = "Notification";
            const string PaymentMethodFamily = "ewallet";
            const string PaymentMethodType = "alipay_billing_agreement";
            const string QrCodeImagePrefix = "data:image/png;base64,";
            const string AlipayQrCode = "alipayQrCode";

            PaymentInstrument pi = new PaymentInstrument()
            {
                PaymentMethod = new PaymentMethod()
                {
                    PaymentMethodFamily = PaymentMethodFamily,
                    PaymentMethodType = PaymentMethodType
                }
            };

            pi.PaymentInstrumentDetails = new PaymentInstrumentDetails()
            {
                AppSignUrl = AppSignUrl,
                RedirectUrl = RedirectUrl,
                PendingOn = PendingOn
            };

            List<string> partners = new List<string>()
            {
                "amcweb",
                "amcxbox",
                "webblends",
                "oxowebdirect",
                "xbox",
                "officeoobe",
                "oxooobe",
                "smboobe"
            };

            List<string> partnersWithAlipayRedirectUrl = new List<string>()
            {
                "amcweb",
                "webblends",
                "oxowebdirect",
                "officeoobe"
            };

            foreach (var partner in partners)
            {
                List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetQrCodeChallengeDescriptionForPI(pi, Language, AlipayQrCode, partner);
                string pidl = JsonConvert.SerializeObject(pidls);
                Assert.IsFalse(string.IsNullOrWhiteSpace(pidl));

                if (partnersWithAlipayRedirectUrl.Contains(partner))
                {
                    Assert.IsTrue(pidl.Contains(RedirectUrl));
                }

                Assert.IsTrue(pidl.Contains(QrCodeImagePrefix));
            }
        }

        [DataRow(new string[] { "PXSetPayPal2ndScreenPollingIntervalFiveSeconds" }, 5000, true, 600)]
        [DataRow(new string[] { "PXSetPayPal2ndScreenPollingIntervalTenSeconds" }, 10000, true, 600)]
        [DataRow(new string[] { "PXSetPayPal2ndScreenPollingIntervalFifteenSeconds" }, 15000, true, 600)]
        [DataRow(new string[0], 3000, true, 600)]
        [TestMethod]
        public void PidlFactoryAddPaypalQrCodeChallenge(string[] exposedFlightFeatures, int pollInterval, bool checkPollingTimeOut, int maxPollingAttempts)
        {
            const string Language = "en-us";
            const string RedirectUrl = "https://pmservices.cp.microsoft-int.com/RedirectionService/CoreRedirection/Redirect/PSS_0001613c3171c1134d2fbd0fd7ad3a588b06";
            const string PaymentMethodFamily = "ewallet";
            const string PaymentMethodType = "paypal";
            const string QrCodeImagePrefix = "data:image/png;base64,";
            const string PaypalQrCode = "paypalQrCode";
            const string SessionId = "8085f894-e086-8580-3ae5-afe3633b9c17";
            const string Scenario = "paypalqrcode";
            const string Country = "us";
            const string PaypalQrCodeRedirectButton = "paypalQrCodeRedirectButton";
            const string PaypalQrCodeUseBrowserText = "paypalQrCodeUseBrowserText";

            List<string> exposedFlightFeaturesList = new List<string>();
            foreach (string exposedFlightFeature in exposedFlightFeatures)
            {
                exposedFlightFeaturesList.Add(exposedFlightFeature);
            }

            PaymentInstrument pi = new PaymentInstrument()
            {
                PaymentMethod = new PaymentMethod()
                {
                    PaymentMethodFamily = PaymentMethodFamily,
                    PaymentMethodType = PaymentMethodType
                }
            };

            pi.PaymentInstrumentDetails = new PaymentInstrumentDetails()
            {
                RedirectUrl = RedirectUrl,
            };

            List<string> partners = new List<string>()
            {
                "xbox",
                "amcxbox",
                "storify",
                "xboxsubs",
                "xboxsettings",
                "saturn",
                "oxowebdirect",
                "oxodime",
                "defaulttemplate"
            };

            foreach (var partner in partners)
            {
                bool containsUseBrowserButtonAndText = TestConstants.PartnersToEnablePayPal2ndScreenRedirectButton.Contains(partner, StringComparer.OrdinalIgnoreCase);
                List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetQrCodeChallengeDescriptionForPI(pi, Language, PaypalQrCode, partner, null, null, null, false, Country, exposedFlightFeaturesList, SessionId, Scenario);
                PollActionContext pollActionContext = (PollActionContext)pidls[0].DisplayPages[0].Action.Context;
                Assert.AreEqual(pollActionContext.Interval, pollInterval);
                Assert.AreEqual(pollActionContext.CheckPollingTimeOut, checkPollingTimeOut);
                Assert.AreEqual(pollActionContext.MaxPollingAttempts, maxPollingAttempts);
                string sessionQueryUrl = string.Format("sessions/{0}", SessionId);
                string expectedGetPiLink = $"https://{{pifd-endpoint}}/users/{{userId}}/paymentInstrumentsEx/{pi.PaymentInstrumentId}?language={Language}&partner={partner}&country={Country}&sessionQueryUrl={WebUtility.UrlEncode(sessionQueryUrl)}&scenario={Scenario}";

                Assert.IsTrue(pollActionContext.ResponseActions.ContainsKey("Active"));
                DisplayHintAction activeAction = (DisplayHintAction)pollActionContext.ResponseActions["Active"];
                Assert.AreEqual(activeAction.ActionType, DisplayHintActionType.success.ToString());
                DisplayHintAction declinedAction = (DisplayHintAction)pollActionContext.ResponseActions["Declined"];
                Assert.AreEqual(declinedAction.ActionType, DisplayHintActionType.gohome.ToString());
                Assert.AreEqual(expectedGetPiLink, pollActionContext.Href);
                string pidl = JsonConvert.SerializeObject(pidls);
                Assert.IsFalse(string.IsNullOrWhiteSpace(pidl));
                Assert.IsTrue(pidl.Contains(QrCodeImagePrefix));

                if (Constants.PartnerGroups.IsXboxNativePartner(partner))
                {
                    Assert.IsTrue(pidl.Contains(PaypalQrCodeRedirectButton) && pidl.Contains(PaypalQrCodeUseBrowserText));
                    var rootContainer = pidls[0].DisplayPages[0].Members[0] as ContainerDisplayHint;
                    var wrapperContainer = rootContainer.Members[1] as ContainerDisplayHint;
                    var bottomControlContainer = wrapperContainer.Members[1] as ContainerDisplayHint;
                    var redirectButton = bottomControlContainer.Members[1] as ButtonDisplayHint;
                    Assert.AreEqual(redirectButton.Action.ActionType, "moveLast");
                    string expectedIFrameBaseUrl = TestConstants.PollingUrls.PaypalPIQrCodeIframeRedirectSourceUrlXboxNative;
                    var iframeDisplayHint = pidls[0].DisplayPages[2].Members[0] as IFrameDisplayHint;
                    StringAssert.Contains(iframeDisplayHint.SourceUrl, expectedIFrameBaseUrl);
                }
                else if (string.Equals(partner, Constants.PartnerNames.OXOWebDirect, StringComparison.InvariantCultureIgnoreCase) || string.Equals(partner, Constants.PartnerNames.OXODIME, StringComparison.InvariantCultureIgnoreCase))
                {
                    Assert.AreEqual(containsUseBrowserButtonAndText, pidl.Contains(PaypalQrCodeRedirectButton) && pidl.Contains(PaypalQrCodeUseBrowserText));
                    Assert.IsFalse(containsUseBrowserButtonAndText);
                    var textGroupDisplayHint = pidls[0].DisplayPages[0].Members[3] as TextGroupDisplayHint;
                    var redirectionLink = textGroupDisplayHint.Members[1] as HyperlinkDisplayHint;
                    Assert.AreEqual(redirectionLink.Action.ActionType, "redirect");
                    var buttonGroupChallengePageGroupDisplayHint = pidls[0].DisplayPages[0].Members[4] as GroupDisplayHint;
                    var buttonGroupChallengePageDisplayHint = buttonGroupChallengePageGroupDisplayHint.Members[0] as ButtonDisplayHint;
                    Assert.AreEqual(buttonGroupChallengePageDisplayHint.Action.ActionType, "moveNext");

                    var buttonGroupChallengePage2DisplayHint = pidls[0].DisplayPages[1].Members[5] as GroupDisplayHint;
                    var backButton = buttonGroupChallengePage2DisplayHint.Members[0] as ButtonDisplayHint;
                    Assert.AreEqual(backButton.Action.ActionType, "gohome");
                    var paypalButton = buttonGroupChallengePage2DisplayHint.Members[1] as ButtonDisplayHint;
                    Assert.AreEqual(paypalButton.Action.ActionType, "movePrevious");
                }
                else
                {
                    Assert.AreEqual(containsUseBrowserButtonAndText, pidl.Contains(PaypalQrCodeRedirectButton) && pidl.Contains(PaypalQrCodeUseBrowserText));

                    if (containsUseBrowserButtonAndText)
                    {
                        var useBrowserButtoncontainerDisplayHint = pidls[0].DisplayPages[0].Members[5] as ContainerDisplayHint;
                        var redirectButton = useBrowserButtoncontainerDisplayHint.Members[1] as ButtonDisplayHint;
                        if (string.Equals(partner, "xbox", StringComparison.OrdinalIgnoreCase))
                        {
                            Assert.AreEqual(redirectButton.Action.ActionType, "redirect");
                        }
                        else if (string.Equals(partner, "amcxbox", StringComparison.OrdinalIgnoreCase))
                        {
                            Assert.AreEqual(redirectButton.Action.ActionType, "navigate");
                        }
                    }
                }
            }
        }

        [DataRow(new string[0], 3000, false, null)]
        [DataRow(new string[0], 3000, false, null)]
        [TestMethod]
        public void PidlFactoryAddGlobalPIQrCodeChallenge(string[] exposedFlightFeatures, int pollInterval, bool checkPollingTimeOut, int maxPollingAttempts)
        {
            const string Language = "en-us";
            const string SessionId = "3da30633-b4bc-426f-a235-85e2a010f859";
            const string QrCodeImagePrefix = "data:image/png;base64,";
            const string ChallengeDescriptionType = "globalPIQrCode";
            const string QueryPurchaseStateURL = "https://pmservices.cp.microsoft.com/RedirectionService/CoreRedirection/Query/" + SessionId;
            const string GlobalQrCodeRedirectButton = "globalPIQrCodeRedirectButton";
            const string GlobalQrCodeRedirectText = "globalPIQrCodeRedirectText";
            const string GlobalQrCodeIframe = "globalPIQrCodeIframe";
            List<string> exposedFlightFeaturesList = new List<string>();
            const string OrderId = "ceffcf3f-0f87-4d88-bd93-74c325b00746";
            foreach (string exposedFlightFeature in exposedFlightFeatures)
            {
                exposedFlightFeaturesList.Add(exposedFlightFeature);
            }

            PaymentInstrument pi = new PaymentInstrument()
            {
                PaymentMethod = new PaymentMethod()
                {
                    PaymentMethodFamily = TestConstants.PaymentMethodFamilyNames.OnlineBankTransfer,
                    PaymentMethodType = TestConstants.PaymentMethodTypeNames.Sofort
                },
                PaymentInstrumentId = "dummyPiid"
            };

            List<string> partners = new List<string>()
            {
                "xbox",
                "defaulttemplate",
                "onepage",
                "twopage",
                "storify",
                "xboxsubs",
                "xboxsettings",
                "saturn"
            };

            foreach (var partner in partners)
            {
                List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetQrCodeChallengeDescriptionForPI(pi, Language, ChallengeDescriptionType, partner, null, null, null, false, null, exposedFlightFeaturesList, SessionId, null, OrderId);

                // first pollAction
                DisplayHintAction firstPollAction = pidls[0].DisplayPages[0].Action;
                PollActionContext firstPollActionContext = (PollActionContext)firstPollAction.Context;
                Assert.AreEqual(firstPollActionContext.Interval, pollInterval);
                Assert.AreEqual(firstPollActionContext.CheckPollingTimeOut, checkPollingTimeOut);
                Assert.AreEqual(firstPollActionContext.MaxPollingAttempts, maxPollingAttempts);
                string expectedGetPiLink = $"{TestConstants.PollingUrls.GlobalPIQrCodeQueryUrl}{SessionId}";
                Assert.IsTrue(firstPollActionContext.ResponseActions.ContainsKey("pending"));
                DisplayHintAction pendingAction = (DisplayHintAction)firstPollActionContext.ResponseActions["pending"];
                Assert.AreEqual(pendingAction.ActionType, DisplayHintActionType.moveNextAndPoll.ToString());
                Assert.IsTrue(firstPollActionContext.ResponseActions.ContainsKey("failure"));
                DisplayHintAction failureAction = (DisplayHintAction)firstPollActionContext.ResponseActions["failure"];
                Assert.AreEqual(failureAction.ActionType, DisplayHintActionType.handleFailure.ToString());
                Assert.AreEqual(expectedGetPiLink, firstPollActionContext.Href);

                // second pollAction
                DisplayHintAction secondPollAction = firstPollAction.NextAction;
                PollActionContext secondPollActionContext = (PollActionContext)secondPollAction.Context;
                Assert.AreEqual(secondPollActionContext.Interval, pollInterval);
                Assert.AreEqual(secondPollActionContext.CheckPollingTimeOut, checkPollingTimeOut);
                Assert.AreEqual(secondPollActionContext.MaxPollingAttempts, maxPollingAttempts);
                expectedGetPiLink = $"{TestConstants.PollingUrls.GlobalPIQrCodeQueryUrl}{SessionId}";
                Assert.IsTrue(secondPollActionContext.ResponseActions.ContainsKey("success"));
                DisplayHintAction successAction = (DisplayHintAction)secondPollActionContext.ResponseActions["success"];
                Assert.AreEqual(successAction.ActionType, DisplayHintActionType.updatePoll.ToString());
                Assert.IsTrue(secondPollActionContext.ResponseActions.ContainsKey("failure"));
                failureAction = (DisplayHintAction)secondPollActionContext.ResponseActions["failure"];
                Assert.AreEqual(failureAction.ActionType, DisplayHintActionType.handleFailure.ToString());
                Assert.AreEqual(expectedGetPiLink, secondPollActionContext.Href);

                // third pollAction
                DisplayHintAction thirdPollAction = secondPollAction.NextAction;
                PollActionContext thirdPollActionContext = (PollActionContext)thirdPollAction.Context;
                Assert.AreEqual(thirdPollActionContext.Interval, pollInterval);
                Assert.AreEqual(thirdPollActionContext.CheckPollingTimeOut, checkPollingTimeOut);
                Assert.AreEqual(thirdPollActionContext.MaxPollingAttempts, maxPollingAttempts);
                MicrosoftMarketplaceServicesPurchaseServiceContractsV7UpdateOrderRequestV7 updateOrderRequestPayload = (MicrosoftMarketplaceServicesPurchaseServiceContractsV7UpdateOrderRequestV7)thirdPollActionContext.Payload;
                Assert.AreEqual(updateOrderRequestPayload.BillingInformation.SessionId, SessionId);
                Assert.AreEqual(updateOrderRequestPayload.BillingInformation.PaymentInstrumentId, pi.PaymentInstrumentId);
                Assert.AreEqual(updateOrderRequestPayload.OrderState, "Purchased");
                Assert.IsNotNull(updateOrderRequestPayload.ClientContext.Client);
                expectedGetPiLink = $"{TestConstants.PollingUrls.GlobalPIQrPurchaseUrl}{OrderId}";
                Assert.IsTrue(thirdPollActionContext.ResponseActions.ContainsKey("Purchased"));
                successAction = (DisplayHintAction)thirdPollActionContext.ResponseActions["Purchased"];
                Assert.AreEqual(successAction.ActionType, DisplayHintActionType.success.ToString());
                Assert.IsTrue(thirdPollActionContext.ResponseActions.ContainsKey("Canceled"));
                failureAction = (DisplayHintAction)thirdPollActionContext.ResponseActions["Canceled"];
                Assert.AreEqual(failureAction.ActionType, DisplayHintActionType.handleFailure.ToString());
                Assert.AreEqual(expectedGetPiLink, thirdPollActionContext.Href);

                string pidl = JsonConvert.SerializeObject(pidls);
                Assert.IsFalse(string.IsNullOrWhiteSpace(pidl));
                Assert.IsTrue(pidl.Contains(QueryPurchaseStateURL));
                Assert.IsTrue(pidl.Contains(QrCodeImagePrefix));

                if (Constants.PartnerGroups.IsXboxNativePartner(partner))
                {
                    Assert.IsTrue(pidl.Contains(GlobalQrCodeRedirectButton) && pidl.Contains(GlobalQrCodeRedirectText) && pidl.Contains(GlobalQrCodeIframe));
                    var buttonContainerDisplayHint = pidls[0].DisplayPages[0].Members[4] as ContainerDisplayHint;
                    var useBrowserContainerDisplayHint = buttonContainerDisplayHint.Members[1] as ContainerDisplayHint;
                    var redirectButton = useBrowserContainerDisplayHint.Members[1] as ButtonDisplayHint;
                    Assert.AreEqual(redirectButton.Action.ActionType, "updatePollAndMoveLast");
                    string expectedIFrameBaseUrl = $"{TestConstants.PollingUrls.GlobalPIQrCodeIframeRedirectSourceUrl}{SessionId}";
                    var iframeDisplayHint = pidls[0].DisplayPages[2].Members[0] as IFrameDisplayHint;
                    StringAssert.Contains(iframeDisplayHint.SourceUrl, expectedIFrameBaseUrl);
                }
            }
        }

        [TestMethod]
        [DataRow("xbox")]
        [DataRow("amcxbox")]
        public void PidlFactoryThreeDSOne_AddPaymentInstrument_QrCode(string partner)
        {
            const string Language = "en-us";
            const string RedirectUrl = "https://redirectUrl.com";
            const string PaymentMethodFamily = "credit_card";
            const string PaymentMethodType = "visa";
            const string QrCodeImagePrefix = "data:image/png;base64,";
            const string SessionId = "8085f894-e086-8580-3ae5-afe3633b9c17";
            const string Scenario = "threedsonepolling";
            const string Country = "in";
            string expectedWebviewUrl = "https://redirectUrl.com?ru=https%3A%2F%2Fwww.microsoft.com%2Fen-us%2Fstore%2Frds%2Fv2%2FGeneralAddPISuccess%3FpicvRequired%3DFalse%26type%3Dvisa%26family%3Dcredit_card%26id%3D8085f894-e086-8580-3ae5-afe3633b9c17&rx=https%3A%2F%2Fwww.microsoft.com%2Fen-us%2Fstore%2Frds%2Fv2%2FGeneralAddPIFailure";
            string sessionQueryUrl = string.Format("sessions/{0}", SessionId);

            PaymentInstrument pi = new PaymentInstrument()
            {
                PaymentMethod = new PaymentMethod()
                {
                    PaymentMethodFamily = PaymentMethodFamily,
                    PaymentMethodType = PaymentMethodType
                }
            };

            pi.PaymentInstrumentDetails = new PaymentInstrumentDetails()
            {
                RedirectUrl = RedirectUrl,
                SessionQueryUrl = sessionQueryUrl
            };

            List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetQrCodeChallengeDescriptionForPI(pi, Language, "threeDSOneQrCode", partner, null, null, null, false, Country, null, SessionId, Scenario);
            PollActionContext pollActionContext = (PollActionContext)pidls[0].DisplayPages[1].Action.Context;
            string expectedGetPiLink = $"https://{{pifd-endpoint}}/users/{{userId}}/paymentInstrumentsEx/{pi.PaymentInstrumentId}?language={Language}&partner={partner}&country={Country}&sessionQueryUrl={WebUtility.UrlEncode(sessionQueryUrl)}&scenario={Scenario}";

            Assert.IsTrue(pollActionContext.ResponseActions.ContainsKey("Active"));
            DisplayHintAction activeAction = (DisplayHintAction)pollActionContext.ResponseActions["Active"];
            Assert.AreEqual(activeAction.ActionType, DisplayHintActionType.success.ToString());
            DisplayHintAction declinedAction = (DisplayHintAction)pollActionContext.ResponseActions["Declined"];
            Assert.AreEqual(declinedAction.ActionType, DisplayHintActionType.gohome.ToString());
            Assert.AreEqual(expectedGetPiLink, pollActionContext.Href);
            string pidlResponse = JsonConvert.SerializeObject(pidls);
            Assert.IsFalse(string.IsNullOrWhiteSpace(pidlResponse));
            Assert.IsTrue(pidlResponse.Contains(QrCodeImagePrefix));

            var pidl = pidls[0];
            var qrCodeImage = pidl.GetDisplayHintById("ccThreeDSQrCodeImage");
            Assert.IsNotNull(qrCodeImage);

            var redirectButton = pidl.GetDisplayHintById("goToBankButton");
            Assert.IsNotNull(redirectButton);

            Assert.AreEqual(redirectButton.Action.ActionType, "moveNext");

            IFrameDisplayHint bankWebview = pidl.GetDisplayHintById("globalPIQrCodeIframe") as IFrameDisplayHint;
            Assert.IsNotNull(bankWebview);
            Assert.AreEqual(bankWebview.SourceUrl, expectedWebviewUrl, "IFrame does not contain expected source url");
        }

        [TestMethod]
        [DataRow("xbox", 1)]
        [DataRow("storify", 2)]
        [DataRow("saturn", 2)]
        public void PidlFactory_ThreeDSOne_PurchaseQrCodeChallenge(string partner, int pollingPageIndex)
        {
            const string Language = "en-us";
            const string RedirectBaseUrl = "https://redirectUrl.com";
            const string PaymentMethodFamily = "credit_card";
            const string PaymentMethodType = "visa";
            const string QrCodeImagePrefix = "data:image/png;base64,";
            const string SessionId = "8085f894-e086-8580-3ae5-afe3633b9c17";
            const string Country = "in";
            string expectedRedirectUrl = RedirectBaseUrl + "?ru=https%3A%2F%2Fwww.microsoft.com%2Fen-us%2Fstore%2Frds%2FPurchaseRiskChallengeRedirectSuccess&rx=https%3A%2F%2Fwww.microsoft.com%2Fen-us%2Fstore%2Frds%2FPurchaseRiskChallengeRedirectFailure";
            string sessionQueryUrl = string.Format("sessions/{0}", SessionId);

            PaymentInstrument pi = new PaymentInstrument()
            {
                PaymentMethod = new PaymentMethod()
                {
                    PaymentMethodFamily = PaymentMethodFamily,
                    PaymentMethodType = PaymentMethodType
                }
            };

            pi.PaymentInstrumentDetails = new PaymentInstrumentDetails()
            {
                RedirectUrl = RedirectBaseUrl,
                SessionQueryUrl = sessionQueryUrl
            };

            List<PIDLResource> pidls = PIDLResourceFactory.Instance.GetQrCodeChallengeDescriptionForThreeDSOnePurchase(RedirectBaseUrl, "threeDSOneQrCode", Language, Country, partner, SessionId, new object());
            PollActionContext pollActionContext = (PollActionContext)pidls[0].DisplayPages[pollingPageIndex].Action.Context;
            string expectedGetPiLink = $"https://{{pifd-endpoint}}/users/{{userId}}/paymentSessions/{SessionId}/status";

            Assert.IsTrue(pollActionContext.ResponseActions.ContainsKey("Succeeded"));
            DisplayHintAction activeAction = (DisplayHintAction)pollActionContext.ResponseActions["Succeeded"];
            Assert.AreEqual(activeAction.ActionType, DisplayHintActionType.success.ToString());
            DisplayHintAction failedAction = (DisplayHintAction)pollActionContext.ResponseActions["Failed"];
            Assert.AreEqual(failedAction.ActionType, DisplayHintActionType.success.ToString());
            DisplayHintAction timedoutAction = (DisplayHintAction)pollActionContext.ResponseActions["TimedOut"];
            Assert.AreEqual(timedoutAction.ActionType, DisplayHintActionType.success.ToString());
            DisplayHintAction cancelledAction = (DisplayHintAction)pollActionContext.ResponseActions["Cancelled"];
            Assert.AreEqual(cancelledAction.ActionType, DisplayHintActionType.success.ToString());
            DisplayHintAction internalServerErrorAction = (DisplayHintAction)pollActionContext.ResponseActions["InternalServerError"];
            Assert.AreEqual(internalServerErrorAction.ActionType, DisplayHintActionType.success.ToString());
            Assert.AreEqual(expectedGetPiLink, pollActionContext.Href);
            string pidlResponse = JsonConvert.SerializeObject(pidls);
            Assert.IsFalse(string.IsNullOrWhiteSpace(pidlResponse));
            Assert.IsTrue(pidlResponse.Contains(QrCodeImagePrefix));

            var pidl = pidls[0];
            var qrCodeImage = pidl.GetDisplayHintById("ccThreeDSQrCodeImage");
            Assert.IsNotNull(qrCodeImage, "Missing Qr code image");

            var redirectButton = pidl.GetDisplayHintById("goToBankButton");
            Assert.IsNotNull(redirectButton, "Missing go to bank button");
            Assert.AreEqual(redirectButton.Action.ActionType, "moveNext");

            if (string.Equals(partner, "xbox") || string.Equals(partner, "amcxbox"))
            {
                // Instruction page, QR code page, bank iframe
                Assert.AreEqual(pidl.DisplayPages.Count, 3);
            }
            else if (string.Equals(partner, "storify") || string.Equals(partner, "saturn"))
            {
                // Instruction page, Privacy statement iframe, QR code page, bank iframe
                Assert.AreEqual(pidl.DisplayPages.Count, 3);
                ButtonDisplayHint instructionNextButton = pidl.GetDisplayHintById("moveNext2Button") as ButtonDisplayHint;
                Assert.IsNotNull(instructionNextButton, "Missing next button on instruction page");
                Assert.IsNotNull(instructionNextButton.Action.NextAction, "Instruction page next button missing next action");
                Assert.AreEqual(instructionNextButton.Action.NextAction.ActionType, "moveNext");

                ButtonDisplayHint qrCodeBackButton = pidl.GetDisplayHintById("moveBack2Button") as ButtonDisplayHint;
                Assert.IsNotNull(qrCodeBackButton, "Missing back button on qr code page");
                Assert.IsNotNull(qrCodeBackButton.Action.NextAction, "QR code page back button missing next action");
                Assert.AreEqual(qrCodeBackButton.Action.NextAction.ActionType, "movePrevious");

                ButtonDisplayHint backButton = pidl.GetDisplayHintById("successBackButton") as ButtonDisplayHint;
                Assert.IsNotNull(backButton, "Missing back button");
                Assert.IsNotNull(backButton.Action.Context);
            }

            IFrameDisplayHint bankWebview = pidl.GetDisplayHintById("ThreeDSOneBankFrame") as IFrameDisplayHint;
            Assert.IsNotNull(bankWebview, "Missing bank webview");
            Assert.AreEqual(bankWebview.SourceUrl, expectedRedirectUrl, "Bank webview url does not match expected url");
        }

        [TestMethod]
        public void PidlFactoryThreeDSChallengeIFrame()
        {
            string acsChallengeURL = "https://acs.challenge.com/challenge";
            string creqData = "ABC12345";
            string threeDSSessionData = "ABC12345";
            string threeDSSessionId = Guid.NewGuid().ToString();
            var action = PIDLResourceFactory.GetThreeDSChallengeIFrameClientAction(acsChallengeURL, creqData, threeDSSessionData, threeDSSessionId, "none");
            var resources = action.Context as List<PIDLResource>;

            PageDisplayHint pageDisplayHint = null;
            foreach (var resource in resources)
            {
                pageDisplayHint = resource.DisplayPages[0];
            }

            Assert.IsTrue(pageDisplayHint != null);

            string threeDSChallengePageName = "PaymentChallengePage";
            Assert.AreEqual(pageDisplayHint.DisplayName, threeDSChallengePageName);
        }

        [DataRow("bing", TestConstants.PaymentInstruments.SepaPicv, true, false, TestConstants.ButtonDisplayHintIds.VerifyPicvButton)]
        [DataRow("bing", TestConstants.PaymentInstruments.SepaLegacyPicv, true, false, TestConstants.ButtonDisplayHintIds.VerifyPicvButton)]
        [DataRow("bing", TestConstants.PaymentInstruments.SepaPicvLastTry, false, true, TestConstants.ButtonDisplayHintIds.VerifyPicvButton)]
        [DataRow("defaulttemplate", TestConstants.PaymentInstruments.SepaPicv, true, false, TestConstants.ButtonDisplayHintIds.SaveNextButton)]
        [DataRow("defaulttemplate", TestConstants.PaymentInstruments.SepaLegacyPicv, true, false, TestConstants.ButtonDisplayHintIds.SaveNextButton)]
        [DataRow("defaulttemplate", TestConstants.PaymentInstruments.SepaPicvLastTry, false, true, TestConstants.ButtonDisplayHintIds.SaveNextButton)]
        [DataTestMethod]
        public void PidlFactoryGetPicvChallengeForPi(string partner, string paymentInstrument, bool hasPicvRetryText, bool hasPicvLastRetryText, string buttonHintId)
        {
            // Arrange
            string language = "en-us";
            string challengeType = "sepa_picv";

            PaymentInstrument sepaPi = JsonConvert.DeserializeObject<PaymentInstrument>(paymentInstrument);

            // Act
            List<PIDLResource> challengePidl = PIDLResourceFactory.Instance.GetPicvChallengeDescriptionForPI(sepaPi, challengeType, language, partner);
            PidlAssert.IsValid(challengePidl);

            TextDisplayHint lastRetryText = challengePidl[0].GetDisplayHintById(TestConstants.DisplayHintIds.PicvLastRetryCount) as TextDisplayHint;
            TextDisplayHint retryText = challengePidl[0].GetDisplayHintById(TestConstants.DisplayHintIds.PicvRetryCount) as TextDisplayHint;

            // Assert
            Assert.AreNotEqual(retryText == null, hasPicvRetryText, "RetryText is not shown/hidden as expected");
            Assert.AreNotEqual(lastRetryText == null, hasPicvLastRetryText, "LastRetryText is not shown/hidden as expected");

            ButtonDisplayHint verifyPicvButtonOrSaveNextButton = challengePidl[0].GetDisplayHintById(buttonHintId) as ButtonDisplayHint;

            Assert.IsTrue(verifyPicvButtonOrSaveNextButton != null && verifyPicvButtonOrSaveNextButton.Action != null && verifyPicvButtonOrSaveNextButton.Action.Context != null, "Sepe Picv challenge Pidl DisplayDescription validation failed, verifyPicvButtonOrSaveNextButton not found");

            RestLink actionContext = verifyPicvButtonOrSaveNextButton.Action.Context as RestLink;
            Assert.IsNotNull(actionContext, "Sepe Picv challenge Pidl DisplayDescription validation failed, verify button context cant be null");
            Assert.AreEqual(actionContext.Href, "https://{pifd-endpoint}/users/{userId}/paymentInstrumentsEx/" + sepaPi.PaymentInstrumentId + "/resume?language=" + language + "&partner=" + partner, "Context href is incorrect");
            Assert.AreEqual(actionContext.Method, "POST", "Context method is incorrect");
        }

        [DataRow("storify", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", true)]
        [DataRow("saturn", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", true)]
        [DataRow("xboxsubs", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", true)]
        [DataRow("xboxsettings", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", true)]
        [DataRow("oxowebdirect", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", false)]
        [DataRow("oxodime", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", false)]
        [DataRow("oxooobe", "NonSimMobiAccount", "sms", "NonSimMobiAccount-Pi001-NonSimMobi", false)]
        [TestMethod]
        public void GetChallengeDescription_SMS_EmailPopulated(string partner, string accountId, string type, string piid, bool hasSummaryText)
        {
            string language = "en-us";
            string sessionId = "1234-1234-1234-1234";
            string challengeType = "sms";
            string emailAddress = "test@microsoft.com";

            PaymentInstrument nonSimMobi = JsonConvert.DeserializeObject<PaymentInstrument>(TestConstants.PaymentInstruments.NonSimMobi);

            string flightName = string.Empty; 
            if (string.Equals(partner, Constants.PartnerNames.OXODIME, StringComparison.InvariantCultureIgnoreCase) 
                || string.Equals(partner, Constants.PartnerNames.OXOWebDirect, StringComparison.InvariantCultureIgnoreCase) 
                || string.Equals(partner, Constants.PartnerNames.OXOOobe, StringComparison.InvariantCultureIgnoreCase))
            {
                flightName = "PXEnableSMSChallengeValidation";
            }

            List<PIDLResource> challengePidl = PIDLResourceFactory.Instance.GetChallengeDescriptionsForPi(nonSimMobi, challengeType, language, partner, sessionId, emailAddress: emailAddress, exposedFlightFeatures: new List<string>() { flightName });
            PidlAssert.IsValid(challengePidl);

            TextDisplayHint summaryText = challengePidl[0].GetDisplayHintById("paymentSummaryText") as TextDisplayHint;
            if (hasSummaryText)
            {
                Assert.IsTrue(summaryText.DisplayContent.Contains(emailAddress), "Email Address not found in text");
            }
        }

        [DataRow("PXEnablePurchasePollingForUPIConfirmPayment", "upi")]
        [DataRow("PXEnablePurchasePollingForUPIConfirmPayment", "upi_qr")]
        [DataRow("PXEnablePurchasePollingForUPIConfirmPayment", "upi_commercial")]
        [DataRow("PXEnablePurchasePollingForUPIConfirmPayment", "upi_qr_commercial")]
        [DataRow(null, "upi")]
        [DataRow(null, "upi_qr")]
        [DataRow(null, "upi_commercial")]
        [DataRow(null, "upi_qr_commercial")]
        [TestMethod]
        public void GetUPIRedirectAndStatusCheckDescriptionForPITest(string flights, string type)
        {
            string language = "en-us";
            string partner = "defaultTemplate";
            string sessionId = "Z10010CF7KXN47013879-b66e-4298-b81f-20f0e938fc67";
            string orderId = "ceffcf3f-0f87-4d88-bd93-74c325b00746";
            string family = "real_time_payments";
            int pollInterval = 3000;
            bool checkPollingTimeOut = false;
            int maxPollingAttempts = 0;

            List<string> exposedFlightFeatures = new List<string>();

            if (flights != null)
            {
                exposedFlightFeatures.Add(flights);
            }
           
            string challengeType = $"{family}.{type}";

            PaymentInstrument upiPending = JsonConvert.DeserializeObject<PaymentInstrument>(TestConstants.PaymentInstruments.UPIPending);

            List<PIDLResource> redirectAndStatusCheckPidl = PIDLResourceFactory.Instance.GetUPIRedirectAndStatusCheckDescriptionForPI(upiPending, language, partner, sessionId, challengeType, null, orderId, exposedFlightFeatures);
            PidlAssert.IsValid(redirectAndStatusCheckPidl);

            Assert.AreEqual(2, redirectAndStatusCheckPidl[0].DisplayPages.Count, "2 pages are expected for redirect and status check");

            ButtonDisplayHint goToBankButton = redirectAndStatusCheckPidl[0].GetDisplayHintById(TestConstants.ChallengeDisplayHintIds.UPIGoToBankButton) as ButtonDisplayHint;
            Assert.IsNotNull(goToBankButton, "UPIGoToBankButton should not be null");
            var redirectionServiceLink = goToBankButton.Action.Context as RedirectionServiceLink;
            Assert.AreEqual(upiPending.PaymentInstrumentDetails.RedirectUrl, redirectionServiceLink.BaseUrl, "Go to bank redirect Url does not match");

            ButtonDisplayHint statusCheckButton = redirectAndStatusCheckPidl[0].GetDisplayHintById(TestConstants.ChallengeDisplayHintIds.UPIYesVerificationButton) as ButtonDisplayHint;
            Assert.IsNotNull(statusCheckButton, "UPIYesVerificationButton should not be null");

            PaymentInstrument pi = new PaymentInstrument()
            {
                PaymentMethod = new PaymentMethod()
                {
                    PaymentMethodFamily = family,
                    PaymentMethodType = type
                },
                PaymentInstrumentId = "f9fe440d-277e-4b3d-b0f2-a48366d11519"
            };

            // first pollAction
            DisplayHintAction firstPollAction = redirectAndStatusCheckPidl[0].DisplayPages[0].Action;
            PollActionContext firstPollActionContext = (PollActionContext)firstPollAction.Context;
            Assert.AreEqual(firstPollActionContext.Interval, pollInterval);
            Assert.AreEqual(firstPollActionContext.CheckPollingTimeOut, checkPollingTimeOut);
            Assert.AreEqual(firstPollActionContext.MaxPollingAttempts, maxPollingAttempts);
            string expectedGetPiLink = $"{TestConstants.PollingUrls.UPIQueryUrlForConfirmPayment}{sessionId}";
            Assert.IsTrue(firstPollActionContext.ResponseActions.ContainsKey("pending"));
            DisplayHintAction pendingAction = (DisplayHintAction)firstPollActionContext.ResponseActions["pending"];
            Assert.AreEqual(pendingAction.ActionType, DisplayHintActionType.updatePoll.ToString());
            Assert.IsTrue(firstPollActionContext.ResponseActions.ContainsKey("failure"));
            DisplayHintAction failureAction = (DisplayHintAction)firstPollActionContext.ResponseActions["failure"];
            Assert.AreEqual(failureAction.ActionType, DisplayHintActionType.handleFailure.ToString());
            Assert.AreEqual(expectedGetPiLink, firstPollActionContext.Href);

            // second pollAction
            DisplayHintAction secondPollAction = firstPollAction.NextAction;
            PollActionContext secondPollActionContext = (PollActionContext)secondPollAction.Context;
            Assert.AreEqual(secondPollActionContext.Interval, pollInterval);
            Assert.AreEqual(secondPollActionContext.CheckPollingTimeOut, checkPollingTimeOut);
            Assert.AreEqual(secondPollActionContext.MaxPollingAttempts, maxPollingAttempts);
            expectedGetPiLink = $"{TestConstants.PollingUrls.UPIQueryUrlForConfirmPayment}{sessionId}";
            Assert.IsTrue(secondPollActionContext.ResponseActions.ContainsKey("success"));
            DisplayHintAction successAction = (DisplayHintAction)secondPollActionContext.ResponseActions["success"];

            if (exposedFlightFeatures?.Contains("PXEnablePurchasePollingForUPIConfirmPayment", StringComparer.OrdinalIgnoreCase) ?? false)
            {
                Assert.AreEqual(successAction.ActionType, DisplayHintActionType.updatePoll.ToString());
            }
            else
            {
                Assert.AreEqual(successAction.ActionType, DisplayHintActionType.success.ToString());
            }

            Assert.IsTrue(secondPollActionContext.ResponseActions.ContainsKey("failure"));
            failureAction = (DisplayHintAction)secondPollActionContext.ResponseActions["failure"];
            Assert.AreEqual(failureAction.ActionType, DisplayHintActionType.handleFailure.ToString());
            Assert.AreEqual(expectedGetPiLink, secondPollActionContext.Href);

            if (exposedFlightFeatures?.Contains("PXEnablePurchasePollingForUPIConfirmPayment", StringComparer.OrdinalIgnoreCase) ?? false)
            {
                // third pollAction
                DisplayHintAction thirdPollAction = secondPollAction.NextAction;
                PollActionContext thirdPollActionContext = (PollActionContext)thirdPollAction.Context;
                Assert.AreEqual(thirdPollActionContext.Interval, pollInterval);
                Assert.AreEqual(thirdPollActionContext.CheckPollingTimeOut, checkPollingTimeOut);
                Assert.AreEqual(thirdPollActionContext.MaxPollingAttempts, maxPollingAttempts);
                MicrosoftMarketplaceServicesPurchaseServiceContractsV7UpdateOrderRequestV7 updateOrderRequestPayload = (MicrosoftMarketplaceServicesPurchaseServiceContractsV7UpdateOrderRequestV7)thirdPollActionContext.Payload;
                Assert.AreEqual(updateOrderRequestPayload.BillingInformation.SessionId, sessionId);
                Assert.AreEqual(updateOrderRequestPayload.BillingInformation.PaymentInstrumentId, pi.PaymentInstrumentId);
                Assert.AreEqual(updateOrderRequestPayload.OrderState, "Purchased");
                Assert.IsNotNull(updateOrderRequestPayload.ClientContext.Client);
                expectedGetPiLink = $"{TestConstants.PollingUrls.UPIPurchaseUrlForConfirmPayment}{orderId}";
                Assert.IsTrue(thirdPollActionContext.ResponseActions.ContainsKey("Purchased"));
                successAction = (DisplayHintAction)thirdPollActionContext.ResponseActions["Purchased"];
                Assert.AreEqual(successAction.ActionType, DisplayHintActionType.success.ToString());
                Assert.IsTrue(thirdPollActionContext.ResponseActions.ContainsKey("Canceled"));
                failureAction = (DisplayHintAction)thirdPollActionContext.ResponseActions["Canceled"];
                Assert.AreEqual(failureAction.ActionType, DisplayHintActionType.handleFailure.ToString());
                Assert.AreEqual(expectedGetPiLink, thirdPollActionContext.Href);
            }
        }
    }
}
