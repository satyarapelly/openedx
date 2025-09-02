// <copyright file="GetStaticPidlDescriptionsTests.cs" company="Microsoft">Copyright (c) Microsoft  All rights reserved.</copyright>

namespace CIT.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Helpers;
    using Microsoft.Commerce.Payments.Common.Helper;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    [TestClass]
    public class GetStaticPidlDescriptionsTests
    {
        [DataRow("webblends")]
        [DataRow("defaultTemplate")]
        [DataTestMethod]
        public void GetCc3DSRedirectAndStatusCheckDescriptionForPaymentSessionTest(string partnerName)
        {
            string sessionId = "ZZZZZ12345678";
            string language = "en-US";
            string redirectionUrl = $"paymentSessions/{sessionId}/browserAuthenticateRedirectionThreeDSOne";

            PIDLResource resource = PIDLResourceFactory.Instance.GetCc3DSRedirectAndStatusCheckDescriptionForPaymentSession(sessionId, language, partnerName, redirectionUrl);
            Assert.IsNotNull(resource);
            Assert.AreEqual(resource.ClientAction.ActionType, ClientActionType.Pidl);

            List<PIDLResource> pidls = resource.ClientAction.Context as List<PIDLResource>;
            PidlAssert.IsValid(pidls);

            PIDLResource pidl = pidls[0];

            ButtonDisplayHint goToBankButton = pidl.GetDisplayHintById(TestConstants.StaticDisplayHintIds.Cc3DSGoToBankButton) as ButtonDisplayHint;
            Assert.IsNotNull(goToBankButton);

            RedirectionServiceLink redirectionLink = goToBankButton.Action.Context as RedirectionServiceLink;
            Assert.IsNotNull(redirectionLink);
            Assert.IsTrue(redirectionLink.BaseUrl.Contains($"paymentSessions/{sessionId}/browserAuthenticateRedirectionThreeDSOne"));

            ButtonDisplayHint tryAgainButtonButton = pidl.GetDisplayHintById(TestConstants.StaticDisplayHintIds.Cc3DSTryAgainButton) as ButtonDisplayHint;
            Assert.IsNotNull(tryAgainButtonButton, "tryAgainButtonButton should not be null");
            Assert.AreEqual(tryAgainButtonButton.Action.ActionType.ToString(), "redirect");
            RedirectionServiceLink tryAgainButtonButtonrestLink = tryAgainButtonButton.Action.Context as RedirectionServiceLink;
            Assert.IsTrue(tryAgainButtonButtonrestLink.BaseUrl.Contains($"paymentSessions/{sessionId}/browserAuthenticateRedirectionThreeDSOne"));

            ButtonDisplayHint yesImDoneWithBankButton = pidl.GetDisplayHintById(TestConstants.StaticDisplayHintIds.Cc3DSYesButton) as ButtonDisplayHint;
            Assert.IsNotNull(yesImDoneWithBankButton);

            RestLink restLink = yesImDoneWithBankButton.Action.Context as RestLink;
            Assert.IsNotNull(restLink);
            Assert.IsTrue(restLink.Href.Contains("/users/{userId}/paymentSessions/" + sessionId + "/status"));
            Assert.AreEqual(restLink.Method, "GET");

            // DisplayPages[1] is Yes Im done with bank verification page
            PageDisplayHint doneWithBankPage = pidl.DisplayPages[1];
            DisplayHintAction pollAction = doneWithBankPage.Action;
            Assert.IsNotNull(pollAction);

            PollActionContext pollContext = pollAction.Context as PollActionContext;
            Assert.IsNotNull(pollContext);
            Assert.IsTrue(pollContext.Href.Contains("users/{userId}/paymentSessions/" + sessionId + "/status"));
            Assert.AreEqual(pollContext.ResponseResultExpression, "clientAction.context.challengeStatus");

            var actions = pollContext.ResponseActions;

            Assert.IsTrue(actions.ContainsKey("Succeeded"));
            var displayHintAction = actions["Succeeded"] as DisplayHintAction;
            Assert.AreEqual(displayHintAction.ActionType, DisplayHintActionType.success.ToString());

            Assert.IsTrue(actions.ContainsKey("Failed"));
            displayHintAction = actions["Failed"] as DisplayHintAction;
            Assert.AreEqual(displayHintAction.ActionType, DisplayHintActionType.handleFailure.ToString());

            Assert.IsTrue(actions.ContainsKey("TimedOut"));
            displayHintAction = actions["TimedOut"] as DisplayHintAction;
            Assert.AreEqual(displayHintAction.ActionType, DisplayHintActionType.handleFailure.ToString());

            Assert.IsTrue(actions.ContainsKey("Cancelled"));
            displayHintAction = actions["Cancelled"] as DisplayHintAction;
            Assert.AreEqual(displayHintAction.ActionType, DisplayHintActionType.gohome.ToString());

            Assert.IsTrue(actions.ContainsKey("InternalServerError"));
            displayHintAction = actions["InternalServerError"] as DisplayHintAction;
            Assert.AreEqual(displayHintAction.ActionType, DisplayHintActionType.handleFailure.ToString());
        }

        [TestMethod]
        public void GetCc3DSRedirectAndStatusCheckDescriptionForPITest()
        {
            string language = "en-us";
            string partner = "azure";
            string scenario = "azureIbiza";
            string classicProduct = "azureClassic";
            string country = "in";

            PaymentInstrument india3dsPendingCc = JsonConvert.DeserializeObject<PaymentInstrument>(TestConstants.PaymentInstruments.India3dsPendingCc);

            List<PIDLResource> redirectAndStatusCheckPidl = PIDLResourceFactory.Instance.GetCc3DSRedirectAndStatusCheckDescriptionForPI(india3dsPendingCc, language, partner, scenario, classicProduct, false, country);
            PidlAssert.IsValid(redirectAndStatusCheckPidl);

            Assert.AreEqual(2, redirectAndStatusCheckPidl[0].DisplayPages.Count, "2 pages are expected for redirect and status check");

            ButtonDisplayHint goToBankButton = redirectAndStatusCheckPidl[0].GetDisplayHintById(TestConstants.StaticDisplayHintIds.Cc3DSGoToBankButton) as ButtonDisplayHint;
            Assert.IsNotNull(goToBankButton, "Cc3DSGoToBankButton should not be null");
            var redirectionServiceLink = goToBankButton.Action.Context as RedirectionServiceLink;
            Assert.AreEqual(india3dsPendingCc.PaymentInstrumentDetails.RedirectUrl, redirectionServiceLink.BaseUrl, "Go to bank redirect Url does not match");
            Assert.AreEqual("3ds", redirectionServiceLink.RuParameters["pendingOn"], "pendingOn param needs to be 3ds");

            ButtonDisplayHint statusCheckButton = redirectAndStatusCheckPidl[0].GetDisplayHintById(TestConstants.StaticDisplayHintIds.Cc3DSYesButton) as ButtonDisplayHint;
            Assert.IsNotNull(statusCheckButton, "Cc3DSYesButton should not be null");
            var getPiLink = statusCheckButton.Action.Context as RestLink;
            string expectedGetPiLink = $"https://{{pifd-endpoint}}/users/{{userId}}/paymentInstrumentsEx/{india3dsPendingCc.PaymentInstrumentId}?language=en-us&partner={partner}&country={country}&scenario={scenario}&sessionQueryUrl={WebUtility.UrlEncode(india3dsPendingCc.PaymentInstrumentDetails.SessionQueryUrl)}&classicProduct={classicProduct}";
            Assert.AreEqual(expectedGetPiLink, getPiLink.Href, "Get PI link for status check is not as expected");
        }

        [TestMethod]
        [DataRow(TestConstants.PartnerNames.Webblends)]
        [DataRow("defaultTemplate")]
        public void GetCc3DSRedirectAndStatusCheckDescriptionForPITest_Consumer(string partner)
        {
            string language = "en-us";
            string country = "in";

            PaymentInstrument india3dsPendingCc = JsonConvert.DeserializeObject<PaymentInstrument>(TestConstants.PaymentInstruments.India3dsPendingCc);

            List<PIDLResource> redirectAndStatusCheckPidl = PIDLResourceFactory.Instance.GetCc3DSRedirectAndStatusCheckDescriptionForPI(india3dsPendingCc, language, partner, null, null, false, country, true);
            PidlAssert.IsValid(redirectAndStatusCheckPidl);

            Assert.AreEqual(2, redirectAndStatusCheckPidl[0].DisplayPages.Count, "2 pages are expected for redirect and status check");

            ButtonDisplayHint goToBankButton = redirectAndStatusCheckPidl[0].GetDisplayHintById(TestConstants.StaticDisplayHintIds.Cc3DSGoToBankButton) as ButtonDisplayHint;
            Assert.IsNotNull(goToBankButton, "Cc3DSGoToBankButton should not be null");
            var redirectionServiceLink = goToBankButton.Action.Context as RedirectionServiceLink;
            Assert.AreEqual(india3dsPendingCc.PaymentInstrumentDetails.RedirectUrl, redirectionServiceLink.BaseUrl, "Go to bank redirect Url does not match");
            Assert.AreEqual("3ds", redirectionServiceLink.RuParameters["pendingOn"], "pendingOn param needs to be 3ds");

            ButtonDisplayHint statusCheckButton = redirectAndStatusCheckPidl[0].GetDisplayHintById(TestConstants.StaticDisplayHintIds.Cc3DSYesButton) as ButtonDisplayHint;
            Assert.IsNotNull(statusCheckButton, "Cc3DSYesButton should not be null");
            var getPiLink = statusCheckButton.Action.Context as RestLink;
            string expectedGetPiLink = $"https://{{pifd-endpoint}}/users/{{userId}}/paymentInstrumentsEx/{india3dsPendingCc.PaymentInstrumentId}?language=en-us&partner={partner}&country={country}&sessionQueryUrl={WebUtility.UrlEncode(india3dsPendingCc.PaymentInstrumentDetails.SessionQueryUrl)}";
            Assert.AreEqual(expectedGetPiLink, getPiLink.Href, "Get PI link for status check is not as expected");

            ButtonDisplayHint cc3DSTryAgainButton = redirectAndStatusCheckPidl[0].GetDisplayHintById(TestConstants.StaticDisplayHintIds.Cc3DSTryAgainButton) as ButtonDisplayHint;
            Assert.IsNotNull(cc3DSTryAgainButton, "cc3DSTryAgainButton should not be null");
            Assert.IsTrue(string.Equals(cc3DSTryAgainButton.Action.ActionType, "redirect"));
            JObject actionContext = JObject.Parse(JsonConvert.SerializeObject(cc3DSTryAgainButton.Action.Context));
            Assert.IsTrue(string.Equals(actionContext.SelectToken("baseUrl").ToString(), india3dsPendingCc.PaymentInstrumentDetails.RedirectUrl));

            DisplayHintAction pollAction = redirectAndStatusCheckPidl[0].DisplayPages[0].Action;
            Assert.IsNotNull(pollAction);
            Assert.AreEqual(pollAction.ActionType, "poll", "action type should be poll");
            var pollActionContext = pollAction.Context as PollActionContext;
            Assert.IsTrue(string.Equals(pollActionContext.Href, $"https://{{pifd-endpoint}}/users/{{userId}}/paymentInstrumentsEx/56ef424a-8ecd-47b0-84d7-b79510b9d404?language=en-us&partner={partner}&country=in&scenario=threedsonepolling&sessionQueryUrl=sessions%2F0b027a5d-09b9-4879-8fb7-64031b926c97"));
            DisplayHintAction action;
            pollActionContext.ResponseActions.TryGetValue("Active", out action);
            Assert.AreEqual(action.ActionType.ToString(), "success");
            pollActionContext.ResponseActions.TryGetValue("Declined", out action);
            Assert.AreEqual(action.ActionType.ToString(), "handleFailure");
        }
        
        [TestMethod]
        [DataRow(TestConstants.PartnerNames.Webblends)]
        public void GetCc3DSIframeRedirectAndStatusCheckDescriptionForPITest_Consumer(string partner)
        {
            string language = "en-us";
            string country = "in";
            string baseUrl = "https://pifd.cp.microsoft-int.com/V6.0";

            PaymentInstrument india3dsPendingCc = JsonConvert.DeserializeObject<PaymentInstrument>(TestConstants.PaymentInstruments.India3dsPendingCc);

            List<PIDLResource> redirectAndStatusCheckPidl = PIDLResourceFactory.GetCc3DSIframeRedirectAndStatusCheckDescriptionForPI(india3dsPendingCc, language, partner, null, null, false, country, baseUrl);

            Assert.AreEqual(1, redirectAndStatusCheckPidl[0].DisplayPages.Count, "1 page is expected for redirect and status check");

            IFrameDisplayHint threeDSIframe = redirectAndStatusCheckPidl[0].GetDisplayHintById(TestConstants.StaticDisplayHintIds.ThreeDSIframe) as IFrameDisplayHint;
            Assert.IsNotNull(threeDSIframe, "threeDSIframe should not be null");
            Assert.IsNotNull(threeDSIframe.DisplayTags, "Accessibility display tags are missing");
            Assert.AreEqual(threeDSIframe.DisplayTags["accessibilityName"], "The bank authentication dialog");
            Assert.IsTrue(threeDSIframe.DisplayContent.Contains("https://pmservices.cp.microsoft-int.com/RedirectionService/CoreRedirection/Redirect/0b027a5d-09b9-4879-8fb7-64031b926c97?ru=https%3A%2F%2Fpifd.cp.microsoft-int.com%2FV6.0%2FpaymentInstrumentsEx%2F56ef424a-8ecd-47b0-84d7-b79510b9d404%2Fresume%3Fcountry%3Din%26language%3Den-us%26partner%3Dwebblends%26isSuccessful%3DTrue%26sessionQueryUrl%3Dsessions%2F0b027a5d-09b9-4879-8fb7-64031b926c97&rx=https%3A%2F%2Fpifd.cp.microsoft-int.com%2FV6.0%2FpaymentInstrumentsEx%2F56ef424a-8ecd-47b0-84d7-b79510b9d404%2Fresume%3Fcountry%3Din%26language%3Den-us%26partner%3Dwebblends%26isSuccessful%3DFalse%26sessionQueryUrl%3Dsessions%2F0b027a5d-09b9-4879-8fb7-64031b926c97"));
            Assert.IsTrue(string.Equals(threeDSIframe.ExpectedClientActionId, "0b027a5d-09b9-4879-8fb7-64031b926c97"));
        }

        [TestMethod]
        public void GetCc3DSStatusCheckDescriptionForPITest()
        {
            string language = "en-us";
            string partner = "azure";
            string scenario = "azureIbiza";
            string classicProduct = "azureClassic";
            string country = "in";

            PaymentInstrument india3dsPendingCc = JsonConvert.DeserializeObject<PaymentInstrument>(TestConstants.PaymentInstruments.India3dsPendingCc);

            List<PIDLResource> statusCheckPidl = PIDLResourceFactory.Instance.GetCc3DSStatusCheckDescriptionForPI(india3dsPendingCc, language, partner, scenario, classicProduct, false, country, india3dsPendingCc.PaymentInstrumentDetails.SessionQueryUrl);
            PidlAssert.IsValid(statusCheckPidl);

            Assert.AreEqual(1, statusCheckPidl[0].DisplayPages.Count, "Only 1 page is expected for status check");

            ButtonDisplayHint statusCheckButton = statusCheckPidl[0].GetDisplayHintById(TestConstants.StaticDisplayHintIds.Cc3DSYesButton) as ButtonDisplayHint;
            Assert.IsNotNull(statusCheckButton, "Cc3DSYesButton should not be null");
            var getPiLink = statusCheckButton.Action.Context as RestLink;
            string expectedGetPiLink = $"https://{{pifd-endpoint}}/users/{{userId}}/paymentInstrumentsEx/{india3dsPendingCc.PaymentInstrumentId}?language=en-us&partner={partner}&country={country}&scenario={scenario}&sessionQueryUrl={WebUtility.UrlEncode(india3dsPendingCc.PaymentInstrumentDetails.SessionQueryUrl)}&classicProduct={classicProduct}";
            Assert.AreEqual(expectedGetPiLink, getPiLink.Href, "Get PI link for status check is not as expected");
        }

        [TestMethod]
        public void GetCc3DSRedirectPidlForPaymentSessionTest_Consumer_Inline()
        {
            // In PROD the url will be pointing to  /browserAuthenticateRedirectionThreeDSOne
            string redirectionUrl = "https://www.bing.com/success";

            PIDLResource pidl = PIDLResourceFactory.GetRedirectPidl(redirectionUrl);

            Assert.AreEqual(pidl.ClientAction.ActionType, ClientActionType.Redirect);

            RedirectionServiceLink pidlRedirectLink = pidl.ClientAction.Context as RedirectionServiceLink;
            Assert.AreEqual(pidlRedirectLink.BaseUrl, redirectionUrl);

            // We don't add these yet, but testing for 0 so we remember to update the test to reflect when we potentially add them.
            Assert.AreEqual(pidlRedirectLink.RuParameters.Count, 0);
            Assert.AreEqual(pidlRedirectLink.RxParameters.Count, 0);
        }

        [TestMethod]
        [DataRow(TestConstants.PartnerNames.Webblends)]
        [DataRow("defaultTemplate")]
        public void GetCc3DSStatusCheckDescriptionForPITest_Consumer(string partner)
        {
            string language = "en-us";
            string country = "in";

            PaymentInstrument india3dsPendingCc = JsonConvert.DeserializeObject<PaymentInstrument>(TestConstants.PaymentInstruments.India3dsPendingCc);

            List<PIDLResource> statusCheckPidl = PIDLResourceFactory.Instance.GetCc3DSStatusCheckDescriptionForPI(india3dsPendingCc, language, partner, null, null, false, country, india3dsPendingCc.PaymentInstrumentDetails.SessionQueryUrl);
            PidlAssert.IsValid(statusCheckPidl);

            Assert.AreEqual(1, statusCheckPidl[0].DisplayPages.Count, "Only 1 page is expected for status check");

            ButtonDisplayHint statusCheckButton = statusCheckPidl[0].GetDisplayHintById(TestConstants.StaticDisplayHintIds.Cc3DSYesVerificationButton) as ButtonDisplayHint;
            Assert.IsNotNull(statusCheckButton, "Cc3DSYesButton should not be null");
            var getPiLink = statusCheckButton.Action.Context as RestLink;
            string expectedGetPiLink = $"https://{{pifd-endpoint}}/users/{{userId}}/paymentInstrumentsEx/{india3dsPendingCc.PaymentInstrumentId}?language=en-us&partner={partner}&country={country}&sessionQueryUrl={WebUtility.UrlEncode(india3dsPendingCc.PaymentInstrumentDetails.SessionQueryUrl)}";
            Assert.AreEqual(expectedGetPiLink, getPiLink.Href, "Get PI link for status check is not as expected");
            ButtonDisplayHint cc3DSTryAgainButton = statusCheckPidl[0].GetDisplayHintById(TestConstants.StaticDisplayHintIds.Cc3DSRetryButton) as ButtonDisplayHint;
            Assert.IsNotNull(cc3DSTryAgainButton, "cc3DSTryAgainButton should not be null");
            Assert.IsTrue(string.Equals(cc3DSTryAgainButton.Action.ActionType, "redirect"));
            JObject actionContext = JObject.Parse(JsonConvert.SerializeObject(cc3DSTryAgainButton.Action.Context));
            Assert.IsTrue(string.Equals(actionContext.SelectToken("baseUrl").ToString(), "https://{redirection-endpoint}/RedirectionService/CoreRedirection/Redirect/0b027a5d-09b9-4879-8fb7-64031b926c97"));
        }

        [TestMethod]
        [DataRow(TestConstants.PartnerNames.Webblends)]
        public void GetCc3DSIframeStatusCheckDescriptionForPITest_Consumer(string partner)
        {
            string language = "en-us";
            string country = "in";

            PaymentInstrument india3dsPendingCc = JsonConvert.DeserializeObject<PaymentInstrument>(TestConstants.PaymentInstruments.India3dsPendingCc);

            List<PIDLResource> statusCheckPidl = PIDLResourceFactory.GetCc3DSIframeStatusCheckDescriptionForPI(india3dsPendingCc.PaymentInstrumentId, language, partner, null, null, false, country, india3dsPendingCc.PaymentInstrumentDetails.SessionQueryUrl);

            Assert.AreEqual(1, statusCheckPidl[0].DisplayPages.Count, "Only 1 page is expected for status check");

            DisplayHintAction pollAction = statusCheckPidl[0].DisplayPages[0].Action;
            Assert.IsNotNull(pollAction);
            Assert.AreEqual(pollAction.ActionType, "poll", "action type should be poll");
            var pollActionContext = pollAction.Context as PollActionContext;
            Assert.IsTrue(string.Equals(pollActionContext.Href, "https://{pifd-endpoint}/users/{userId}/paymentInstrumentsEx/56ef424a-8ecd-47b0-84d7-b79510b9d404?language=en-us&partner=webblends&country=in&scenario=threedsonepolling&sessionQueryUrl=sessions%2F0b027a5d-09b9-4879-8fb7-64031b926c97"));
            DisplayHintAction action;
            pollActionContext.ResponseActions.TryGetValue("Active", out action);
            Assert.AreEqual(action.ActionType.ToString(), "success");
            pollActionContext.ResponseActions.TryGetValue("Declined", out action);
            Assert.AreEqual(action.ActionType.ToString(), "handleFailure");
        }

        [TestMethod]
        [DataRow("azure")]
        [DataRow("defaultTemplate")]
        public void GetCc3DSRedirectAndStatusCheckDescriptionForPaymentAuthTest(string partner)
        {
            string language = "en-us";
            string scenario = "azureIbiza";
            string redirectUrl = "https://rdsurl";
            string country = "in";
            string rdsSessionId = "randomRDSSessionId";
            string paymentSessionId = "randomSessionId";
            string resourceType = "cc3DSRedirectAndStatusCheckPidl";

            List<PIDLResource> redirectAndStatusCheckPidl = PIDLResourceFactory.Instance.Get3DSRedirectAndStatusCheckDescriptionForPaymentAuth(redirectUrl, rdsSessionId, paymentSessionId, partner, language, country, resourceType, scenario);
            PidlAssert.IsValid(redirectAndStatusCheckPidl);

            Assert.AreEqual(2, redirectAndStatusCheckPidl[0].DisplayPages.Count, "2 pages are expected for redirect and status check");

            ButtonDisplayHint goToBankButton = redirectAndStatusCheckPidl[0].GetDisplayHintById(TestConstants.StaticDisplayHintIds.Cc3DSGoToBankButton) as ButtonDisplayHint;
            Assert.IsNotNull(goToBankButton, "Cc3DSGoToBankButton should not be null");
            var redirectionServiceLink = goToBankButton.Action.Context as RedirectionServiceLink;
            Assert.AreEqual(redirectUrl, redirectionServiceLink.BaseUrl, "Go to bank redirect Url does not match");

            ButtonDisplayHint statusCheckButton = redirectAndStatusCheckPidl[0].GetDisplayHintById(TestConstants.StaticDisplayHintIds.Cc3DSYesButton) as ButtonDisplayHint;
            Assert.IsNotNull(statusCheckButton, "Cc3DSYesButton should not be null");
            var getStatusLink = statusCheckButton.Action.Context as RestLink;
            string expectedGetStatusLink = $"https://{{pifd-endpoint}}/anonymous/rdssession/query?sessionid={rdsSessionId}&country={country}&language={language}&partner={partner}&scenario={scenario}";
            Assert.AreEqual(expectedGetStatusLink, getStatusLink.Href, "Get PI link for status check is not as expected");
        }

        /// <summary>
        /// This test is used to verify the Redirect action for Legacy Bill Desk Payment.
        /// </summary>
        /// <param name="partner"></param>
        [TestMethod]
        [DataRow("azure")]
        [DataRow("defaultTemplate")]
        public void GetLegacyBillDeskPaymentRedirectAndStatusCheckDescriptionForPaymentAuthTest(string partner)
        {
            // Arrange
            string language = "en-us";
            string scenario = "pollAction";
            string redirectUrl = "https://rdsurl";
            string country = "in";
            string rdsSessionId = "randomRDSSessionId";
            string paymentSessionId = "randomSessionId";
            string resourceType = "legacyBillDesk3DSStatusCheckPidl";
            string paymentMethodFamilyTypeId = "ewallet.legacy_billdesk_payment";

            // Act
            List<PIDLResource> redirectAndStatusCheckPidl = PIDLResourceFactory.Instance.Get3DSRedirectAndStatusCheckDescriptionForPaymentAuth(redirectUrl, rdsSessionId, paymentSessionId, partner, language, country, resourceType, scenario, paymentMethodFamilyTypeId);
            
            // Assert
            PidlAssert.IsValid(redirectAndStatusCheckPidl);

            Assert.AreEqual(1, redirectAndStatusCheckPidl[0].DisplayPages.Count, "1 pages are expected for redirect and status check");

            ButtonDisplayHint legacyBillDesk3DSYesButton = redirectAndStatusCheckPidl[0].GetDisplayHintById(TestConstants.ButtonDisplayHintIds.LegacyBillDesk3DSYesButton) as ButtonDisplayHint;
            Assert.IsNotNull(legacyBillDesk3DSYesButton, "LegacyBillDesk3DSGoToBankButton should not be null");
            var getStatusLink = legacyBillDesk3DSYesButton.Action.Context as RestLink;
            string expectedGetStatusLink = $"https://{{pifd-endpoint}}/anonymous/rdssession/query?sessionid={rdsSessionId}&country={country}&language={language}&partner={partner}&scenario={scenario}";
            Assert.AreEqual(expectedGetStatusLink, getStatusLink.Href, "Get PI link for status check is not as expected");
        }

        [TestMethod]
        public void GetCc3DSStatusCheckDescriptionForPaymentAuthTest()
        {
            string language = "en-us";
            string partner = "azure";
            string scenario = "azureIbiza";
            string rdsSessionId = "randomRDSSessionId";
            string paymentSessionId = "randomSessionId";
            string country = "in";
            string resourceType = "cc3DSStatusCheckPidl";

            List<PIDLResource> statusCheckPidl = PIDLResourceFactory.Instance.Get3DSStatusCheckDescriptionForPaymentAuth(rdsSessionId, paymentSessionId, partner, language, country, resourceType, scenario);
            PidlAssert.IsValid(statusCheckPidl);

            Assert.AreEqual(1, statusCheckPidl[0].DisplayPages.Count, "Only 1 page is expected for status check");

            ButtonDisplayHint statusCheckButton = statusCheckPidl[0].GetDisplayHintById(TestConstants.StaticDisplayHintIds.Cc3DSYesButton) as ButtonDisplayHint;
            Assert.IsNotNull(statusCheckButton, "Cc3DSYesButton should not be null");
            var getStatusLink = statusCheckButton.Action.Context as RestLink;
            string expectedGetStatusLink = $"https://{{pifd-endpoint}}/anonymous/rdssession/query?sessionid={rdsSessionId}&country={country}&language={language}&partner={partner}&scenario={scenario}";
            Assert.AreEqual(expectedGetStatusLink, getStatusLink.Href, "Get PI link for status check is not as expected");
        }

        [TestMethod]
        [DataRow(null, TestConstants.PartnerNames.OXOWebDirect)]
        [DataRow(new string[] { TestConstants.FlightNames.PXEnablePaypalRedirectUrlText }, TestConstants.PartnerNames.OXOWebDirect)]
        [DataRow(null, TestConstants.PartnerNames.Webblends)]
        [DataRow(new string[] { TestConstants.FlightNames.PXEnablePaypalRedirectUrlText }, TestConstants.PartnerNames.Webblends)]
        [DataRow(null, TestConstants.PartnerNames.NorthStarWeb)]
        [DataRow(new string[] { TestConstants.FlightNames.PXEnablePaypalRedirectUrlText }, TestConstants.PartnerNames.NorthStarWeb)]
        public void GetRedirectPidlForPI_PXEnablePaypalRedirectUrlText_Flight(string[] additionalFlights, string partner)
        {
            List<string> flightNames = new List<string>() { "PXTestBasicFlight", "PXTestSampleFlight" };
            string language = "en-us";
            string country = "us";
            int expectedMemberCount = 3;

            if (additionalFlights != null)
            {
                foreach (string flight in additionalFlights)
                {
                    flightNames.Add(flight);
                }
            }

            PaymentInstrument paypalRedirectPI = JsonConvert.DeserializeObject<PaymentInstrument>(TestConstants.PaymentInstruments.PayPalRedirect);
            List<PIDLResource> paypalRedirectPidl = PIDLResourceFactory.Instance.GetRedirectPidlForPI(paypalRedirectPI, TestConstants.PidlResourceDescriptionType.PaypalRedirectStaticPidl, language, partner, false, country, flightNames);

            PidlAssert.IsValid(paypalRedirectPidl);

            Assert.AreEqual(1, paypalRedirectPidl[0].DisplayPages.Count, "Only 1 page is expected for status check");

            if (additionalFlights != null)
            {
                if (flightNames.Contains(TestConstants.FlightNames.PXEnablePaypalRedirectUrlText, StringComparer.OrdinalIgnoreCase))
                {
                    // case where we have no original URL flight, but have the second flight and it is not webblends partner will be ignored
                    expectedMemberCount = 4;

                    HyperlinkDisplayHint paypalRedirectHyperlink = paypalRedirectPidl[0].GetDisplayHintById(TestConstants.StaticDisplayHintIds.PaypalRedirectHyperlink) as HyperlinkDisplayHint;
                    Assert.IsNotNull(paypalRedirectHyperlink, $"{TestConstants.StaticDisplayHintIds.PaypalRedirectHyperlink} should not be null");
                    var piRedirectionServiceLink = paypalRedirectHyperlink.Action.Context as RedirectionServiceLink;
                    Assert.AreEqual(paypalRedirectPI.PaymentInstrumentDetails.RedirectUrl, piRedirectionServiceLink.BaseUrl, "Pidl redirect link does not match PI link");
                    Assert.IsTrue(piRedirectionServiceLink.RuParameters.ContainsKey("id"));
                    Assert.IsTrue(piRedirectionServiceLink.RuParameters.ContainsKey("family"));
                    Assert.IsTrue(piRedirectionServiceLink.RuParameters.ContainsKey("type"));
                    Assert.IsTrue(piRedirectionServiceLink.RuParameters.ContainsKey("pendingOn"));
                    Assert.IsTrue(piRedirectionServiceLink.RuParameters.ContainsKey("picvRequired"));
                }
                else
                {
                    HyperlinkDisplayHint paypalRedirectHyperlink = paypalRedirectPidl[0].GetDisplayHintById(TestConstants.StaticDisplayHintIds.PaypalRedirectHyperlink) as HyperlinkDisplayHint;
                    Assert.IsNull(paypalRedirectHyperlink, $"{TestConstants.StaticDisplayHintIds.PaypalRedirectHyperlink} should be null");
                }

                ButtonDisplayHint tryAgainRedirectionButtonDisplayHint = paypalRedirectPidl[0].GetDisplayHintById(TestConstants.StaticDisplayHintIds.PaypalNoButton) as ButtonDisplayHint;
                Assert.IsNotNull(tryAgainRedirectionButtonDisplayHint, $"{TestConstants.StaticDisplayHintIds.PaypalNoButton} should not be null");

                if (TestConstants.PartnersToEnablePaypalRedirectOnTryAgain.Contains(partner, StringComparer.OrdinalIgnoreCase))
                {
                    Assert.AreEqual("redirect", tryAgainRedirectionButtonDisplayHint.Action.ActionType, "ActionType is not redirect");

                    var piRedirectionServiceLink = tryAgainRedirectionButtonDisplayHint.Action.Context as RedirectionServiceLink;
                    Assert.IsTrue(piRedirectionServiceLink.RuParameters.ContainsKey("id"));
                    Assert.IsTrue(piRedirectionServiceLink.RuParameters.ContainsKey("family"));
                    Assert.IsTrue(piRedirectionServiceLink.RuParameters.ContainsKey("type"));
                    Assert.IsTrue(piRedirectionServiceLink.RuParameters.ContainsKey("pendingOn"));
                    Assert.IsTrue(piRedirectionServiceLink.RuParameters.ContainsKey("picvRequired"));
                }
                else
                {
                    Assert.AreEqual("restartFlow", tryAgainRedirectionButtonDisplayHint.Action.ActionType, "ActionType is not restartFlow");
                }

                Assert.AreEqual(expectedMemberCount, paypalRedirectPidl[0].DisplayPages[0].Members.Count, "Unexpected count of members in DisplayPages with flighting turned on");
            }
            else
            {
                Assert.AreEqual(expectedMemberCount, paypalRedirectPidl[0].DisplayPages[0].Members.Count, "Unexpected count of members in DisplayPages with flighting turned off");
            }
        }

        [TestMethod]
        [DataRow(TestConstants.PartnerNames.Webblends, TestConstants.PaymentMethodTypeNames.Venmo, null, 4)]
        [DataRow(TestConstants.TemplateNames.DefaultTemplate, TestConstants.PaymentMethodTypeNames.Venmo, null, 4)]
        [DataRow(TestConstants.TemplateNames.OnePage, TestConstants.PaymentMethodTypeNames.Venmo, null, 4)]
        [DataRow(TestConstants.TemplateNames.TwoPage, TestConstants.PaymentMethodTypeNames.Venmo, null, 4)]

        public void GetRedirectPidlForPI(string partner, string piType, string[] flightNames, int expectedMemberCount)
        {
            string language = "en-us";
            string country = "us";
          
            PaymentInstrument redirectPi = JsonConvert.DeserializeObject<PaymentInstrument>(TestConstants.PaymentInstruments.VenmoRedirect);
            List<PIDLResource> redirectPidl = PIDLResourceFactory.Instance.GetRedirectPidlForPI(redirectPi, TestConstants.PidlResourceDescriptionType.VenmoRedirectStaticPidl, language, partner, false, country, flightNames?.ToList<string>());

            PidlAssert.IsValid(redirectPidl);

            HyperlinkDisplayHint redirectHyperlink = redirectPidl[0].GetDisplayHintById(TestConstants.StaticDisplayHintIds.VenmoRedirectHyperlink) as HyperlinkDisplayHint;
            Assert.IsNotNull(redirectHyperlink, $"{TestConstants.StaticDisplayHintIds.VenmoRedirectHyperlink} should not be null");

            RedirectionServiceLink piRedirectionServiceLink = redirectHyperlink.Action.Context as RedirectionServiceLink;
            Assert.AreEqual(redirectPi.PaymentInstrumentDetails.RedirectUrl, piRedirectionServiceLink.BaseUrl, "Pidl redirect link does not match PI link");

            ButtonDisplayHint tryAgainRedirectionButtonDisplayHint = redirectPidl[0].GetDisplayHintById(TestConstants.StaticDisplayHintIds.VenmoNoButton) as ButtonDisplayHint;
            Assert.IsNotNull(tryAgainRedirectionButtonDisplayHint, $"{TestConstants.StaticDisplayHintIds.VenmoNoButton} should not be null");

            Assert.AreEqual("restartFlow", tryAgainRedirectionButtonDisplayHint.Action.ActionType, "ActionType is not redirect");
            Assert.AreEqual(expectedMemberCount, redirectPidl[0].DisplayPages[0].Members.Count, "Unexpected count of members in DisplayPages.");

            Assert.AreEqual("poll", redirectPidl[0].DisplayPages[0].Action.ActionType, "Expected ActionType is poll");

            if (string.Equals(partner, "webblends", StringComparison.OrdinalIgnoreCase))
            {
                Assert.IsTrue(((PollActionContext)redirectPidl[0].DisplayPages[0].Action.Context).Href.Contains("sessionQueryUrl"), "sessionQueryUrl query param is expected in the poll URL for Venmo");
                Assert.IsTrue(((PollActionContext)redirectPidl[0].DisplayPages[0].Action.Context).Href.Contains("scenario"), "scenario query param is expected in the poll URL for Venmo");
            }

            Assert.IsTrue(piRedirectionServiceLink.RuParameters.ContainsKey("id"));
            Assert.IsTrue(piRedirectionServiceLink.RuParameters.ContainsKey("family"));
            Assert.IsTrue(piRedirectionServiceLink.RuParameters.ContainsKey("type"));
            Assert.IsTrue(piRedirectionServiceLink.RuParameters.ContainsKey("pendingOn"));
            Assert.IsTrue(piRedirectionServiceLink.RuParameters.ContainsKey("picvRequired"));
        }
    }
}
