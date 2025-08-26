// <copyright company="Microsoft Corporation">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using global::Tests.Common.Model;
    using global::Tests.Common.Model.Pidl;
    using global::Tests.Common.Model.Pims;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.PimsModel;
    using Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService.DataModel;
    using Microsoft.Commerce.Payments.PXService.Model.CatalogService;
    using Microsoft.Commerce.Payments.PXService.Model.FraudDetectionService;
    using Microsoft.Commerce.Payments.PXService.Model.HIPService;
    using Microsoft.Commerce.Payments.PXService.Model.IssuerService;
    using Microsoft.Commerce.Payments.PXService.V7;
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using QRCoder;
    using SelfHostedPXServiceCore.Mocks;
    using Constants = global::Tests.Common.Model.Pidl.Constants;
    using ErrorResponse = global::Tests.Common.Model.ErrorResponse;

    [TestClass]
    public class PaymentInstrumentsExTests : TestBase
    {
        [TestInitialize]
        public void Startup()
        {
            PXSettings.AddressEnrichmentService.Responses.Clear();
            PXSettings.AccountsService.Responses.Clear();
            PXSettings.PimsService.Responses.Clear();
            PXSettings.TokenPolicyService.Responses.Clear();
            PXSettings.PurchaseService.Responses.Clear();
            PXSettings.PartnerSettingsService.Responses.Clear();
            PXSettings.OrchestrationService.Responses.Clear();
            PXSettings.IssuerService.Responses.Clear();
            PXSettings.CatalogService.Responses.Clear();

            PXSettings.AddressEnrichmentService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
            PXSettings.PimsService.ResetToDefaults();
            PXSettings.TokenPolicyService.ResetToDefaults();
            PXSettings.PurchaseService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.OrchestrationService.ResetToDefaults();
            PXSettings.IssuerService.ResetToDefaults();
            PXSettings.CatalogService.ResetToDefaults();
            PXSettings.PaymentOrchestratorService.ResetToDefaults();
        }

        // Default behavior is to allow payment methods
        [DataRow("us", "webblends", true, true, false)]
        [DataRow("us", "oxowebdirect", true, true, false)]
        [DataRow("us", "cart", true, true, false)]
        [DataRow("us", "xbox", true, true, false)]
        [DataRow("ca", "webblends", true, true, false)]
        [DataRow("ca", "cart", true, true, false)]
        [DataRow("ca", "xbox", true, true, false)]

        // Visa and MC in China are only for commercial and legacy consumer partners
        [DataRow("cn", "webblends", false, true, false)]
        [DataRow("cn", "oxowebdirect", false, true, false)]
        [DataRow("cn", "cart", false, true, false)]
        [DataRow("cn", "xbox", false, true, false)]
        [DataRow("cn", "amcweb", true, true, false)]
        [DataRow("cn", "appsource", true, true, false)]
        [DataRow("cn", "azure", true, true, true)]
        [DataRow("cn", "bing", true, true, false)]
        [DataRow("cn", "commercialstores", true, true, false)]
        [DataRow("cn", "payin", true, true, false)]
        [DataRow("cn", "setupoffice", true, true, false)]
        [DataRow("cn", "setupofficesdx", true, true, false)]
        [DataRow("cn", "xboxweb", true, true, false)]

        // In  India (with flights off), Amex is disallowed for commercial partners
        [DataRow("in", "webblends", true, true, false)]
        [DataRow("in", "oxowebdirect", true, true, false)]
        [DataRow("in", "cart", true, true, false)]
        [DataRow("in", "xbox", true, true, false)]
        [DataRow("in", "amcweb", true, true, false)]
        [DataRow("in", "bing", true, false, false)]
        [DataRow("in", "azure", true, false, true)]
        [DataRow("in", "azuresignup", true, false, true)]
        [DataRow("in", "azureibiza", true, false, true)]
        [DataRow("in", "commercialstores", true, false, false)]
        [DataRow("in", "commercialwebblends", true, false, false)]

        [DataTestMethod]
        public async Task ListPaymentInstrumentsEx_CreditCard_TypesAreAsExpected(string country, string partner, bool allowVisaAndMC, bool allowAmex, bool allowLegacyInvoice)
        {
            // TODO: Remove once venmo is flighted at 100%
            PXFlightHandler.AddToEnabledFlights("PxEnableVenmo");

            var pis = await ListPIFromPXService(
                string.Format(
                    "/v7.0/Account001/paymentInstrumentsEx?country={0}&partner={1}",
                    country,
                    partner));

            var expectedPiIds = PimsMockResponseProvider.ListPaymentInstruments("Account001").Where(pi =>
            {
                return allowVisaAndMC
                    || (!string.Equals(pi.PaymentMethod.PaymentMethodType, "visa", StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(pi.PaymentMethod.PaymentMethodType, "mc", StringComparison.OrdinalIgnoreCase));
            }).Where(pi =>
            {
                return allowAmex
                    || !string.Equals(pi.PaymentMethod.PaymentMethodType, "amex", StringComparison.OrdinalIgnoreCase);
            }).Where(pi =>
            {
                return allowLegacyInvoice
                    || !string.Equals(pi.PaymentMethod.PaymentMethodType, "legacy_invoice", StringComparison.OrdinalIgnoreCase);
            }).Select(pi =>
            {
                return pi.PaymentInstrumentId;
            }).ToArray();

            var actualPiIds = pis.Select(pi =>
            {
                return pi.PaymentInstrumentId;
            }).ToArray();

            CollectionAssert.AreEquivalent(expectedPiIds, actualPiIds);
        }

        // Default behavior is to allow payment methods
        [DataRow("us", "webblends", true, true, false)]
        [DataRow("us", "oxowebdirect", true, true, false)]
        [DataRow("us", "cart", true, true, false)]
        [DataRow("us", "xbox", true, true, false)]
        [DataRow("ca", "webblends", true, true, false)]
        [DataRow("ca", "cart", true, true, false)]
        [DataRow("ca", "xbox", true, true, false)]

        // Visa and MC in China are only for commercial and legacy consumer partners
        [DataRow("cn", "webblends", false, true, false)]
        [DataRow("cn", "oxowebdirect", false, true, false)]
        [DataRow("cn", "cart", false, true, false)]
        [DataRow("cn", "xbox", false, true, false)]
        [DataRow("cn", "amcweb", true, true, false)]
        [DataRow("cn", "appsource", true, true, false)]
        [DataRow("cn", "azure", true, true, true)]
        [DataRow("cn", "bing", true, true, false)]
        [DataRow("cn", "commercialstores", true, true, false)]
        [DataRow("cn", "payin", true, true, false)]
        [DataRow("cn", "setupoffice", true, true, false)]
        [DataRow("cn", "setupofficesdx", true, true, false)]
        [DataRow("cn", "xboxweb", true, true, false)]

        // In  India (with flights off), Amex is disallowed for commercial partners
        [DataRow("in", "webblends", true, true, false)]
        [DataRow("in", "oxowebdirect", true, true, false)]
        [DataRow("in", "cart", true, true, false)]
        [DataRow("in", "xbox", true, true, false)]
        [DataRow("in", "amcweb", true, true, false)]
        [DataRow("in", "bing", true, false, false)]
        [DataRow("in", "azure", true, false, true)]
        [DataRow("in", "azuresignup", true, false, true)]
        [DataRow("in", "azureibiza", true, false, true)]
        [DataRow("in", "commercialstores", true, false, false)]
        [DataRow("in", "commercialwebblends", true, false, false)]

        [DataTestMethod]
        public async Task GetPaymentInstrumentsEx_CreditCard_TypesAreAsExpected(string country, string partner, bool allowVisaAndMC, bool allowAmex, bool allowLegacyInvoice)
        {
            // TODO: Remove once venmo is flighted at 100%
            PXFlightHandler.AddToEnabledFlights("PxEnableVenmo");

            var piIds = new string[] { "Account001-Pi001-Visa", "Account001-Pi002-MC", "Account001-Pi003-Amex", "Account001-Pi004-LegacyInvoice" };
            foreach (var piId in piIds)
            {
                var pi = await GetPIFromPXService(
                    string.Format(
                        "/v7.0/Account001/paymentInstrumentsEx/{0}?partner={1}&country={2}",
                        piId,
                        partner,
                        country));

                var piFromPims = PimsMockResponseProvider.GetPaymentInstrument("Account001", piId);
                bool isNullExpected = false;

                if (!allowVisaAndMC
                    && (string.Equals(piFromPims.PaymentMethod.PaymentMethodType, "visa", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(piFromPims.PaymentMethod.PaymentMethodType, "mc", StringComparison.OrdinalIgnoreCase)))
                {
                    isNullExpected = true;
                }

                if (!allowAmex
                    && string.Equals(piFromPims.PaymentMethod.PaymentMethodType, "amex", StringComparison.OrdinalIgnoreCase))
                {
                    isNullExpected = true;
                }

                if (!allowLegacyInvoice
                    && string.Equals(piFromPims.PaymentMethod.PaymentMethodType, "legacy_invoice", StringComparison.OrdinalIgnoreCase))
                {
                    isNullExpected = true;
                }

                if (isNullExpected != (pi == null))
                {
                    Assert.AreEqual(isNullExpected, pi == null, string.Format("Get PI with Id {0} is {1} expected to be null", piId, isNullExpected ? string.Empty : "not "));
                }
            }
        }

        [TestMethod]
        [DataRow(GlobalConstants.Partners.Webblends, GlobalConstants.PaymentInstruments.India3dsPendingCcGetPI, "sessions/5273fbca-829b-4acf-8c9e-dbb781261b0b", true)]
        [DataRow(GlobalConstants.Partners.Webblends, GlobalConstants.PaymentInstruments.India3dsActiveCcGetPI, "sessions/5273fbca-829b-4acf-8c9e-dbb781261b0b", false)]
        [DataRow(GlobalConstants.Partners.Webblends, GlobalConstants.PaymentInstruments.India3dsPendingCcGetPI, null, false)]
        [DataRow(GlobalConstants.Partners.Webblends, GlobalConstants.PaymentInstruments.India3dsActiveCcGetPI, null, false)]
        public void GetCc3DSAddClientActionToIndiaCCRequestTest_Consumer(string partner, string pi, string sessionQueryUrl, bool shouldContainPidlClientAction)
        {
            string language = "en-us";
            string country = "in";

            Microsoft.Commerce.Payments.PimsModel.V4.PaymentInstrument paymentInstrument = JsonConvert.DeserializeObject<Microsoft.Commerce.Payments.PimsModel.V4.PaymentInstrument>(pi);

            ClientActionFactory.AddClientActionToPaymentInstrument(paymentInstrument, "accountId", language, partner, null, null, null, "PidlBaseUrl", "getPI", false, country, "emailAddress", null, sessionQueryUrl);

            Assert.AreEqual(paymentInstrument.ClientAction != null, shouldContainPidlClientAction, "India 3ds Add PI with piid - " + paymentInstrument.PaymentInstrumentId + " has issue with ClientAction");
            if (shouldContainPidlClientAction)
            {
                Assert.AreEqual(paymentInstrument.ClientAction.ActionType.ToString(), "Pidl", "India 3ds Add PI with piid - " + paymentInstrument.PaymentInstrumentId + " has issue with ClientAction");
            }
        }

        /// <summary>
        /// This test is to validate the client action for the redirection pattern for the paypal flow.
        /// </summary>
        /// <param name="partner"></param>
        /// <param name="redirectionPatternType"></param>
        /// <returns></returns>
        [DataRow("officesmb", "inline")]
        [DataRow("officesmb", "QRCode")]
        [DataRow("officesmb", "fullPage")]
        [DataRow("webblends", "fullPage")]
        [DataRow("northstarweb", "inline")]
        [DataTestMethod]
        public async Task GetModernPi_PayPalForValidateClientActionRedirection(string partner, string redirectionPatternType)
        {
            // Arrange
            var headers = new Dictionary<string, string>();
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "paypal",
                paymentMethodOperation = "add",
                paymentMethodCountry = "us",
                paymentMethodResource_id = "ewallet.paypalRedirect",

                context = "purchase",
                sessionId = Guid.NewGuid().ToString(),
                details = new
                {
                    dataType = "ewallet_paypalRedirect_details",
                    dataOperation = "add",
                    dataCountry = "us"
                }
            };

            string country = "us";
            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi009-PaypalRedirect");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            var partnerSettingResponse = new
            {
                add = new
                {
                    template = "defaulttemplate",
                    redirectionPattern = redirectionPatternType
                }
            };

            headers = string.Equals(partner, "officesmb", StringComparison.OrdinalIgnoreCase) ? new Dictionary<string, string>
            {
                {
                    "x-ms-flight", "PXDisablePSSCache"
                }
            }
            :
            new Dictionary<string, string>
            {
                {
                    "x-ms-flight", "PXUsePartnerSettingsService,PXDisablePSSCache"
                }
            };

            PXSettings.PartnerSettingsService.ArrangeResponse(JsonConvert.SerializeObject(partnerSettingResponse));

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            headers?.ToList().ForEach(header => request.Headers.Add(header.Key, header.Value));
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert
            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK, "Expected 200 OK response");

            string responseContent = await result.Content.ReadAsStringAsync();
            PIDLResource pidl = ReadSinglePidlResourceFromJson(responseContent);
            var clientActionValue = redirectionPatternType == "QRCode" ? ClientActionType.Pidl.ToString() : ClientActionType.Redirect.ToString();

            Assert.IsNotNull(pidl.ClientAction, "Client action missing");
            Assert.AreEqual(clientActionValue, pidl.ClientAction.ActionType.ToString());
            Assert.IsNotNull(pidl.ClientAction.Context, "Client action context missing");

            // The pidl have some value only when the redirection is fullpage
            if (redirectionPatternType.Contains("fullPage"))
            {
                Assert.IsNotNull(pidl.ClientAction.RedirectPidl, "Client action redirection pidl missing");
                PIDLResource pidlResource = (pidl.ClientAction.RedirectPidl as List<PIDLResource>)[0];
                List<string> displayHintIds = new List<string>();

                displayHintIds = new List<string>
                {
                    "paypalRedirectHeading",
                    "paypalLogo",
                    "paypalRedirectMessage",
                    "paypalRedirectButtonGroup",
                    "paypalRedirectSubmitButton",
                };

                foreach (string displayHintId in displayHintIds)
                {
                    Assert.IsNotNull(pidlResource.GetDisplayHintById(displayHintId), $"DisplayHint with id {displayHintId} is missing.");

                    if (string.Equals(displayHintId, "paypalRedirectSubmitButton", StringComparison.OrdinalIgnoreCase))
                    {
                        var submitButton = pidlResource.GetDisplayHintById(displayHintId);
                        Assert.IsNotNull(submitButton.Action, "Action is missing for submit button");
                        Assert.AreEqual(submitButton.Action.ActionType, DisplayHintActionType.submit.ToString(), "Action type is not submit for submit button");
                        Assert.IsTrue(submitButton.Action.Context.ToString().Contains($"partner={partner}"), "Action context href should contains the original partner name");
                    }
                }
            }
            else if (redirectionPatternType.Contains("QRCode"))
            {
                PIDLResource pidlResource = (pidl.ClientAction.Context as List<PIDLResource>)[0];
                List<string> displayHintIds = new List<string>
                {
                    "paypalHeading",
                    "paypalLogo",
                    "paypalQrCodeChallengeLoginRedirectionLink",
                    "paypalQrCodeChallengeLoginRedirectionLinkText2",
                    "qrCodeChallengeImageText",
                    "qrCodeChallengeSignInDeviceText",
                    "paypalQrCodeChallengePageRefreshText",
                    "paypalQrCodeChallengePageBackButton",
                };

                foreach (string displayHintId in displayHintIds)
                {
                    Assert.IsNotNull(pidlResource.GetDisplayHintById(displayHintId), $"DisplayHint with id {displayHintId} is missing.");
                }
            }
        }

        [TestMethod]
        [DataRow("windowsstore", false)]
        [DataRow("windowsstore", true)]
        public async Task WindowsNative_PayPal(string partner, bool removeFooterElements)
        {
            PXSettings.PartnerSettingsService.ResetToDefaults();
            if (removeFooterElements)
            {
                PXSettings.PartnerSettingsService.ArrangeResponse(Constants.PSSMockResponses.PXPartnerSettingsWindowsStore);
            }
            else
            {
                string expectedPSSResponse = "{\"select\":{\"template\":\"selectpmbuttonlist\",\"features\":{\"paymentMethodGrouping\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"setGroupedSelectOptionTextBeforeLogo\":true,\"removeDefaultStyleHints\":true}]},\"selectPMButtonListStyleForWindows\":{\"applicableMarkets\":[]}}},\"add\":{\"template\":\"twopage\",\"features\":{\"addCCTwoPageForWindows\":{\"applicableMarkets\":[]},\"addPayPalForWindows\":{\"applicableMarkets\":[]},\"addVenmoForWindows\":{\"applicableMarkets\":[]},\"showRedirectURLInIframe\":{\"applicableMarkets\":[]},\"removeElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"removeEwalletYesButtons\":true,\"removeEwalletBackButtons\":false}]}},\"redirectionPattern\":\"QRCode\"}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
            }

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi009-PaypalRedirect");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            var postRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent("{\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"paypal\",\"paymentMethodOperation\":\"add\",\"paymentMethodCountry\":\"us\",\"paymentMethodResource_id\":\"ewallet.paypalRedirect\",\"sessionId\":\"9089bf48-8e1b-e85b-e203-7dbdfe9bbe5f\",\"context\":\"purchase\",\"riskData\":{\"dataType\":\"payment_method_riskData\",\"dataOperation\":\"add\",\"dataCountry\":\"us\"},\"details\":{\"dataType\":\"ewallet_paypalRedirect_details\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"authenticationMode\":\"Redirect\"}}", Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            postRequest.Headers.Add("x-ms-flight", "PXUsePartnerSettingsService");

            HttpResponseMessage response = await PXClient.SendAsync(postRequest);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var pidlResource = ReadSinglePidlResourceFromJson(await response.Content.ReadAsStringAsync());
            Assert.IsNotNull(pidlResource.ClientAction);
            Assert.AreEqual(ClientActionType.Pidl, pidlResource.ClientAction.ActionType);
            Assert.IsNotNull(pidlResource.ClientAction.Context);

            List<PIDLResource> pidlList = pidlResource.ClientAction.Context as List<PIDLResource>;
            PIDLResource pidl = pidlList[0];
            ImageDisplayHint paypalQrCodeImage = pidl.GetDisplayHintById("paypalQrCodeImage") as ImageDisplayHint;
            Assert.IsNotNull(paypalQrCodeImage);
            Assert.IsNotNull(pidl.GetDisplayHintById("globalPIQrCodeIframe"));
            Assert.IsTrue(paypalQrCodeImage.DisplayTags["accessibilityName"] == "PayPal QR code");

            if (removeFooterElements)
            {
                Assert.IsTrue(pidl.GetDisplayHintById("paypalQrCodeChallengePageBackButton") == null, "Footer elements are supposed to be removed when feature enabled");
                Assert.IsTrue(pidl.GetDisplayHintById("backButton") == null, "Footer elements are supposed to be removed when feature enabled");
                Assert.IsTrue(pidl.GetDisplayHintById("space") == null, "space text element in microsoftPrivacyTextGroup should be removed when removeSpaceInPrivacyTextGroup displayCustomization is set");
            }
            else
            {
                Assert.IsTrue(pidl.GetDisplayHintById("paypalQrCodeChallengePageBackButton") != null, "Footer elements are supposed to be present when feature disabled");
                Assert.IsTrue(pidl.GetDisplayHintById("backButton") != null, "Footer elements are supposed to be present when feature disabled");
                Assert.IsTrue(pidl.GetDisplayHintById("space") != null, "space text element in microsoftPrivacyTextGroup should be present when removeSpaceInPrivacyTextGroup displayCustomization is NOT set");
            }
        }

        [TestMethod]
        [DataRow("windowsstore", "Account001-Pi009-PaypalRedirect", "https://testshorturl.ms/test", false)]
        [DataRow("officesmb", "Account001-Pi009-PaypalRedirect", "https://testshorturl.ms/test", false)]
        [DataRow("officesmb", "Account001-Pi009-PaypalRedirect", "https://testshorturl.ms/test", true)]
        public async Task WindowsNative_PayPalShortURL(string partner, string piid, string shortUrl, bool isDisplayShortUrlAsVertical = false)
        {
            // Arrange
            PXSettings.PartnerSettingsService.ResetToDefaults();

            if (partner == "windowsstore")
            {
                PXSettings.PartnerSettingsService.ArrangeResponse(Constants.PSSMockResponses.PXPartnerSettingsWindowsStore);
            }
            else if (partner == "officesmb" && isDisplayShortUrlAsVertical == false)
            {
                PXSettings.PartnerSettingsService.ArrangeResponse(Constants.PSSMockResponses.OfficeSmbQrCodeRedirection);
            }
            else if (isDisplayShortUrlAsVertical)
            {
                PXSettings.PartnerSettingsService.ArrangeResponse(Constants.PSSMockResponses.OfficeSmbQRCodeRedirectionWithDisplayVertialLayoutFeature);
            }

            var postRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent("{\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"paypal\",\"paymentMethodOperation\":\"add\",\"paymentMethodCountry\":\"us\",\"paymentMethodResource_id\":\"ewallet.paypalRedirect\",\"sessionId\":\"9089bf48-8e1b-e85b-e203-7dbdfe9bbe5f\",\"context\":\"purchase\",\"riskData\":{\"dataType\":\"payment_method_riskData\",\"dataOperation\":\"add\",\"dataCountry\":\"us\"},\"details\":{\"dataType\":\"ewallet_paypalRedirect_details\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"authenticationMode\":\"Redirect\"}}", Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", piid);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            postRequest.Headers.Add("x-ms-flight", "PXUsePartnerSettingsService");

            // Act
            HttpResponseMessage response = await PXClient.SendAsync(postRequest);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var pidlResource = ReadSinglePidlResourceFromJson(await response.Content.ReadAsStringAsync());
            Assert.IsNotNull(pidlResource.ClientAction);
            Assert.AreEqual(ClientActionType.Pidl, pidlResource.ClientAction.ActionType);
            Assert.IsNotNull(pidlResource.ClientAction.Context);

            var pidlList = pidlResource.ClientAction.Context as List<PIDLResource>;
            Assert.AreEqual(1, pidlList.Count);
            var resource = pidlList[0];
            var shortUrlDisplayHint = resource.GetDisplayHintById("paypalPIShortUrl");
            if (partner == "windowsstore")
            {
                shortUrlDisplayHint = shortUrlDisplayHint as HyperlinkDisplayHint;
                Assert.AreEqual(shortUrlDisplayHint.Action.ActionType, DisplayHintActionType.navigate.ToString());
                Assert.AreEqual(shortUrlDisplayHint.Action.Context.ToString(), shortUrl);
            }
            else
            {
                Assert.IsTrue(shortUrlDisplayHint.DisplayHintType == "text");
            }

            GroupDisplayHint paypalQrCodeImageAndURLGroup = resource.GetDisplayHintById("paypalQrCodeImageAndURLGroup") as GroupDisplayHint;
            
            if (isDisplayShortUrlAsVertical)
            {
                Assert.IsNotNull(paypalQrCodeImageAndURLGroup);
                Assert.AreEqual(paypalQrCodeImageAndURLGroup.LayoutOrientation, "vertical");
            }

            if (partner == "officesmb")
            {
                string getStyleHintOfPaypalQRImage = paypalQrCodeImageAndURLGroup.Members[0].StyleHints[0];
                Assert.AreEqual("image-extra-medium", getStyleHintOfPaypalQRImage);
            }

            var shortUrlHeaderDisplayHint = resource.GetDisplayHintById("paypalPIShortUrlInstruction");
            Assert.IsNotNull(shortUrlDisplayHint);
            Assert.IsNotNull(shortUrlHeaderDisplayHint);
            Assert.AreEqual(shortUrl, shortUrlDisplayHint.DisplayText());
            Assert.AreEqual("Or, enter address in a browser to sign in", shortUrlHeaderDisplayHint.DisplayText());
        }

        [TestMethod]
        [DataRow("windowsstore", false)]
        [DataRow("windowsstore", true)]
        public async Task WindowsNative_VenmoQRCode(string partner, bool removeFooterElements)
        {
            PXSettings.PartnerSettingsService.ResetToDefaults();
            if (removeFooterElements)
            {
                PXSettings.PartnerSettingsService.ArrangeResponse(Constants.PSSMockResponses.PXPartnerSettingsWindowsStore);
            }
            else
            {
                string expectedPSSResponse = "{\"select\":{\"template\":\"selectpmbuttonlist\",\"features\":{\"paymentMethodGrouping\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"setGroupedSelectOptionTextBeforeLogo\":true,\"removeDefaultStyleHints\":true}]},\"selectPMButtonListStyleForWindows\":{\"applicableMarkets\":[]}}},\"add\":{\"template\":\"twopage\",\"features\":{\"addCCTwoPageForWindows\":{\"applicableMarkets\":[]},\"addPayPalForWindows\":{\"applicableMarkets\":[]},\"addVenmoForWindows\":{\"applicableMarkets\":[]},\"showRedirectURLInIframe\":{\"applicableMarkets\":[]},\"removeElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"removeEwalletYesButtons\":true,\"removeEwalletBackButtons\":false}]}},\"redirectionPattern\":\"QRCode\"}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
            }

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi015-VenmoRedirect");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            var postRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent("{\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"venmo\",\"paymentMethodOperation\":\"add\",\"paymentMethodCountry\":\"us\",\"paymentMethodResource_id\":\"ewallet.venmo\",\"sessionId\":\"77067a0c-66f3-b1e4-9a77-043e232e771e\",\"riskData\":{\"dataType\":\"payment_method_riskData\",\"dataOperation\":\"add\",\"dataCountry\":\"us\"},\"details\":{\"dataType\":\"ewallet_venmo_details\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"authenticationMode\":\"Redirect\"}}", Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            postRequest.Headers.Add("x-ms-flight", "enablePaymentMethodGrouping,vnext,PxEnableVenmo,PxEnableSelectPMAddPIVenmo,PXUsePartnerSettingsService,PXUsePartnerSettingsService");

            HttpResponseMessage response = await PXClient.SendAsync(postRequest);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var pidlResource = ReadSinglePidlResourceFromJson(await response.Content.ReadAsStringAsync());
            Assert.IsNotNull(pidlResource.ClientAction);
            Assert.AreEqual(ClientActionType.Pidl, pidlResource.ClientAction.ActionType);
            Assert.IsNotNull(pidlResource.ClientAction.Context);

            List<PIDLResource> pidlList = pidlResource.ClientAction.Context as List<PIDLResource>;
            PIDLResource pidl = pidlList[0];
            ImageDisplayHint venmoQrCodeImage = pidl.GetDisplayHintById("venmoQrCodeImage") as ImageDisplayHint;
            Assert.IsNotNull(venmoQrCodeImage);
            Assert.IsNotNull(pidl.GetDisplayHintById("globalPIQrCodeIframe"));
            Assert.IsTrue(venmoQrCodeImage.DisplayTags["accessibilityName"] == "Venmo QR code");

            if (removeFooterElements)
            {
                Assert.IsTrue(pidl.GetDisplayHintById("venmoQrCodeChallengePageBackButton") == null, "Footer elements are supposed to be removed when feature enabled");
                Assert.IsTrue(pidl.GetDisplayHintById("backMoveFirstButton") == null, "Footer elements are supposed to be removed when feature enabled");
            }
            else
            {
                Assert.IsTrue(pidl.GetDisplayHintById("venmoQrCodeChallengePageBackButton") != null, "Footer elements are supposed to be present when feature disabled");
                Assert.IsTrue(pidl.GetDisplayHintById("backMoveFirstButton") != null, "Footer elements are supposed to be present when feature disabled");
            }
        }

        [TestMethod]
        [DataRow("playxbox")]
        public async Task PaymentInstrumentsEx_VenmoQRCode(string partner)
        {
            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ArrangeResponse(Constants.PSSMockResponses.PXPartnerSettingsPlayXbox);

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi015-VenmoRedirect");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            var postRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent("{\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"venmo\",\"paymentMethodOperation\":\"add\",\"paymentMethodCountry\":\"us\",\"paymentMethodResource_id\":\"ewallet.venmo\",\"sessionId\":\"77067a0c-66f3-b1e4-9a77-043e232e771e\",\"riskData\":{\"dataType\":\"payment_method_riskData\",\"dataOperation\":\"add\",\"dataCountry\":\"us\"},\"details\":{\"dataType\":\"ewallet_venmo_details\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"authenticationMode\":\"Redirect\"}}", Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            HttpResponseMessage response = await PXClient.SendAsync(postRequest);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var pidlResource = ReadSinglePidlResourceFromJson(await response.Content.ReadAsStringAsync());
            Assert.IsNotNull(pidlResource.ClientAction);
            Assert.AreEqual(ClientActionType.Pidl, pidlResource.ClientAction.ActionType);
            Assert.IsNotNull(pidlResource.ClientAction.Context);

            List<PIDLResource> pidlList = pidlResource.ClientAction.Context as List<PIDLResource>;
            PIDLResource pidl = pidlList[0];
            Assert.AreEqual(3, pidl.DisplayPages.Count);
            ImageDisplayHint venmoQrCodeImage = pidl.GetDisplayHintById("venmoQrCodeImage") as ImageDisplayHint;
            Assert.IsNotNull(venmoQrCodeImage);
            Assert.IsNotNull(pidl.GetDisplayHintById("globalPIQrCodeIframe"));
            Assert.IsTrue(venmoQrCodeImage.DisplayTags["accessibilityName"] == "Venmo QR code");

            PageDisplayHint iframePage = pidl.DisplayPages[2];
            Assert.AreEqual("venmoQrCodeChallengePage3", iframePage.HintId);

            IFrameDisplayHint iframe = iframePage.Members[0] as IFrameDisplayHint;
            Assert.AreEqual("globalPIQrCodeIframe", iframe.HintId);

            PXSettings.PartnerSettingsService.ResetToDefaults();
        }

        [TestMethod]
        [DataRow("playxbox")]
        public async Task PaymentInstrumentsEx_PayPalQRCode(string partner)
        {
            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ArrangeResponse(Constants.PSSMockResponses.PXPartnerSettingsPlayXbox);

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi009-PaypalRedirect");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            var postRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent("{\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"paypal\",\"paymentMethodOperation\":\"add\",\"paymentMethodCountry\":\"us\",\"paymentMethodResource_id\":\"ewallet.paypalRedirect\",\"sessionId\":\"9089bf48-8e1b-e85b-e203-7dbdfe9bbe5f\",\"context\":\"purchase\",\"riskData\":{\"dataType\":\"payment_method_riskData\",\"dataOperation\":\"add\",\"dataCountry\":\"us\"},\"details\":{\"dataType\":\"ewallet_paypalRedirect_details\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"authenticationMode\":\"Redirect\"}}", Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            postRequest.Headers.Add("x-ms-flight", "PXUsePartnerSettingsService");

            HttpResponseMessage response = await PXClient.SendAsync(postRequest);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var pidlResource = ReadSinglePidlResourceFromJson(await response.Content.ReadAsStringAsync());
            Assert.IsNotNull(pidlResource.ClientAction);
            Assert.AreEqual(ClientActionType.Pidl, pidlResource.ClientAction.ActionType);
            Assert.IsNotNull(pidlResource.ClientAction.Context);

            List<PIDLResource> pidlList = pidlResource.ClientAction.Context as List<PIDLResource>;
            PIDLResource pidl = pidlList[0];
            ImageDisplayHint paypalQrCodeImage = pidl.GetDisplayHintById("paypalQrCodeImage") as ImageDisplayHint;
            Assert.IsNotNull(paypalQrCodeImage);
            Assert.IsNotNull(pidl.GetDisplayHintById("globalPIQrCodeIframe"));
            Assert.IsTrue(paypalQrCodeImage.DisplayTags["accessibilityName"] == "PayPal QR code");
        }

        [TestMethod]
        [DataRow("windowsstore", "Account001-Pi015-VenmoRedirect", "https://testshorturl.ms/test")]
        [DataRow("officesmb", "Account001-Pi015-VenmoRedirect", "https://testshorturl.ms/test")]
        public async Task WindowsNative_VenmoShortURL(string partner, string piid, string shortUrl)
        {
            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ArrangeResponse(Constants.PSSMockResponses.PXPartnerSettingsWindowsStore);

            var postRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent("{\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"venmo\",\"paymentMethodOperation\":\"add\",\"paymentMethodCountry\":\"us\",\"paymentMethodResource_id\":\"ewallet.venmo\",\"sessionId\":\"77067a0c-66f3-b1e4-9a77-043e232e771e\",\"riskData\":{\"dataType\":\"payment_method_riskData\",\"dataOperation\":\"add\",\"dataCountry\":\"us\"},\"details\":{\"dataType\":\"ewallet_venmo_details\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"authenticationMode\":\"Redirect\"}}", Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", piid);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));
            
            postRequest.Headers.Add("x-ms-flight", "enablePaymentMethodGrouping,vnext,PxEnableVenmo,PxEnableSelectPMAddPIVenmo,PXUsePartnerSettingsService");
            
            HttpResponseMessage response = await PXClient.SendAsync(postRequest);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var pidlResource = ReadSinglePidlResourceFromJson(await response.Content.ReadAsStringAsync());
            Assert.IsNotNull(pidlResource.ClientAction);
            Assert.AreEqual(ClientActionType.Pidl, pidlResource.ClientAction.ActionType);
            Assert.IsNotNull(pidlResource.ClientAction.Context);

            var pidlList = pidlResource.ClientAction.Context as List<PIDLResource>;
            Assert.AreEqual(1, pidlList.Count);
            var resource = pidlList[0];
            HyperlinkDisplayHint shortUrlDisplayHint = resource.GetDisplayHintById("venmoShortUrl") as HyperlinkDisplayHint;
            var shortUrlHeaderDisplayHint = resource.GetDisplayHintById("venmoUrlInstructionText");
            Assert.IsNotNull(shortUrlDisplayHint);
            Assert.AreEqual(shortUrlDisplayHint.Action.ActionType, DisplayHintActionType.navigate.ToString());
            Assert.AreEqual(shortUrlDisplayHint.Action.Context.ToString(), shortUrl);
            Assert.IsNotNull(shortUrlHeaderDisplayHint);
            Assert.AreEqual(shortUrl, shortUrlDisplayHint.DisplayText());
            Assert.AreEqual("Or, enter address in a browser to sign in", shortUrlHeaderDisplayHint.DisplayText());
        }

        [TestMethod]
        [DataRow("windowsstore")]
        [DataRow("officesmb")]
        public async Task VenmoQRCodePollingUrl(string partner)
        {
            // Arrange
            PXSettings.PartnerSettingsService.ArrangeResponse(Constants.PSSMockResponses.PXPartnerSettingsWindowsStore);

            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi015-VenmoRedirect");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            var postRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent("{\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"venmo\",\"paymentMethodOperation\":\"add\",\"paymentMethodCountry\":\"us\",\"paymentMethodResource_id\":\"ewallet.venmo\",\"sessionId\":\"77067a0c-66f3-b1e4-9a77-043e232e771e\",\"riskData\":{\"dataType\":\"payment_method_riskData\",\"dataOperation\":\"add\",\"dataCountry\":\"us\"},\"details\":{\"dataType\":\"ewallet_venmo_details\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"authenticationMode\":\"Redirect\"}}", Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            postRequest.Headers.Add("x-ms-flight", "enablePaymentMethodGrouping,vnext,PxEnableVenmo,PxEnableSelectPMAddPIVenmo,PXUsePartnerSettingsService,PXUsePartnerSettingsService");

            // Act
            HttpResponseMessage response = await PXClient.SendAsync(postRequest);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var pidlResource = ReadSinglePidlResourceFromJson(await response.Content.ReadAsStringAsync());
            Assert.IsNotNull(pidlResource.ClientAction);
            Assert.AreEqual(ClientActionType.Pidl, pidlResource.ClientAction.ActionType);
            Assert.IsNotNull(pidlResource.ClientAction.Context);

            List<PIDLResource> pidlList = pidlResource.ClientAction.Context as List<PIDLResource>;
            PIDLResource pidl = pidlList[0];
            PageDisplayHint firstPage = pidl.DisplayPages[0];
            Assert.IsTrue(firstPage.Action.Context.ToString().ToLower().Contains("venmoqrcode"));
            Assert.IsTrue(firstPage.Action.Context.ToString().ToLower().Contains(partner));
        }

        [TestMethod]
        [DataRow(GlobalConstants.Partners.Webblends, GlobalConstants.PaymentInstruments.India3dsPendingCcGetPI, "sessions/5273fbca-829b-4acf-8c9e-dbb781261b0b", true)]
        public void GetCc3DSAddClientActionToIndiaCCRequestTest_UsePartnerSettings(string partner, string pi, string sessionQueryUrl, bool shouldContainPidlClientAction)
        {
            string language = "en-us";
            string country = "in";

            Microsoft.Commerce.Payments.PimsModel.V4.PaymentInstrument paymentInstrument = JsonConvert.DeserializeObject<Microsoft.Commerce.Payments.PimsModel.V4.PaymentInstrument>(pi);

            Microsoft.Commerce.Payments.PartnerSettingsModel.PaymentExperienceSetting settings = new Microsoft.Commerce.Payments.PartnerSettingsModel.PaymentExperienceSetting
            {
                Template = partner,
                RedirectionPattern = "fullPage"
            };

            ClientActionFactory.AddClientActionToPaymentInstrument(paymentInstrument, "accountId", language, partner, null, null, null, "PidlBaseUrl", "getPI", false, country, "emailAddress", null, sessionQueryUrl, setting: settings);

            Assert.AreEqual(paymentInstrument.ClientAction != null, shouldContainPidlClientAction, "India 3ds Add PI with piid - " + paymentInstrument.PaymentInstrumentId + " has issue with ClientAction");
            if (shouldContainPidlClientAction)
            {
                Assert.AreEqual(paymentInstrument.ClientAction.ActionType.ToString(), "Pidl", "India 3ds Add PI with piid - " + paymentInstrument.PaymentInstrumentId + " has issue with ClientAction");
            }

            var pidlList = paymentInstrument.ClientAction.Context as List<Microsoft.Commerce.Payments.PidlModel.V7.PIDLResource>;
            Assert.IsNotNull(pidlList);
            var cc3DSYesVerificationButtonHintId = pidlList[0].GetDisplayHintById("cc3DSYesVerificationButton") as Microsoft.Commerce.Payments.PidlModel.V7.ButtonDisplayHint;
            Assert.IsNotNull(cc3DSYesVerificationButtonHintId);
            var submitlink = cc3DSYesVerificationButtonHintId.Action.Context as Microsoft.Commerce.Payments.PXCommon.RestLink;
            Assert.IsNotNull(submitlink);
            Assert.IsTrue(submitlink.Href.Contains($"partner={partner}"));
        }

        [TestMethod]
        [DataRow(GlobalConstants.Partners.Webblends, true, true)]
        [DataRow(GlobalConstants.Partners.Webblends, false, false)]
        [DataRow(GlobalConstants.Partners.OfficeOobe, false, true)]
        [DataRow(GlobalConstants.Partners.OXOOobe, false, true)]
        [DataRow(GlobalConstants.Partners.Payin, false, true)]
        [DataRow(GlobalConstants.Partners.WebPay, false, true)]
        [DataRow(GlobalConstants.Partners.ConsumerSupport, false, true)]
        [DataRow(GlobalConstants.Partners.XboxWeb, false, true)]
        [DataRow(GlobalConstants.Partners.SetupOffice, false, true)]
        [DataRow(GlobalConstants.Partners.SetupOfficesdx, false, true)]
        [DataRow(GlobalConstants.Partners.Webblends, true, true, true)]
        [DataRow(GlobalConstants.Partners.Webblends, false, false, true)]
        public async Task PaymentInstrumentsEx_3DS1_ShouldEnableIFrameOrNot(string partner, bool shouldSendIframeTestHeader, bool shouldShowIframeExperience, bool usePartnerSettingsService = false)
        {
            string piid = "Account001-3DS1-Redirect";
            var pimsRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                details = new
                {
                    address = new
                    {
                        address_line1 = "One Microsoft Way",
                        city = "Saket",
                        region = "DE",
                        country = "IN",
                        postal_code = "111111"
                    }
                }
            };

            var sessionResponse = "{\"id\":\"ab53ebd0-0b70-41e6-b1ec-12345678\",\"session_type\":\"any\",\"data\":\"{\\\"ProviderName\\\":\\\"BillDesk\\\",\\\"Data\\\":\\\"{\\\\\\\"Operation\\\\\\\":1,\\\\\\\"InitiationRedirectUrl\\\\\\\":\\\\\\\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\\\\\\\",\\\\\\\"InitiationParameters\\\\\\\":{\\\\\\\"MD\\\\\\\":\\\\\\\"FakeMD\\\\\\\",\\\\\\\"PaReq\\\\\\\":\\\\\\\"FakePaReq\\\\\\\",\\\\\\\"TermUrl\\\\\\\":\\\\\\\"https://www.merchanturl.com/Response.jsp\\\\\\\"},\\\\\\\"AccountId\\\\\\\":\\\\\\\"bd888d21-f2a9-4f8b-92c2-12345678\\\\\\\",\\\\\\\"IpAddress\\\\\\\":null,\\\\\\\"UserAgent\\\\\\\":null,\\\\\\\"SubscriptionId\\\\\\\":\\\\\\\"12345678\\\\\\\",\\\\\\\"ApiVersion\\\\\\\":\\\\\\\"v1_2\\\\\\\",\\\\\\\"PaymentInstrumentId\\\\\\\":\\\\\\\"f7940456-15a8-4c32-b562-12345678\\\\\\\",\\\\\\\"PaymentMethodFamily\\\\\\\":\\\\\\\"credit_card\\\\\\\",\\\\\\\"PaymentMethodType\\\\\\\":\\\\\\\"visa\\\\\\\",\\\\\\\"CvvToken\\\\\\\":\\\\\\\"FakeToken\\\\\\\",\\\\\\\"PaymentId\\\\\\\":\\\\\\\"Z10009BINTWG58e3bea8-9540-4ae3-9ddf-2e32d1e06d7b\\\\\\\",\\\\\\\"TransactionId\\\\\\\":\\\\\\\"255b9d4d-c3f1-4812-9fdd-65183ef32cb3\\\\\\\",\\\\\\\"MerchantReferenceNumber\\\\\\\":\\\\\\\"FakeReference\\\\\\\",\\\\\\\"TransactionType\\\\\\\":\\\\\\\"Validate\\\\\\\",\\\\\\\"MerchantId\\\\\\\":\\\\\\\"REDMOND\\\\\\\",\\\\\\\"TrackingId\\\\\\\":\\\\\\\"59cadaf3-480b-4dbc-bb84-12345678\\\\\\\",\\\\\\\"MandateId\\\\\\\":null,\\\\\\\"ThreeDSChargeStatus\\\\\\\":\\\\\\\"Initiated\\\\\\\",\\\\\\\"ThreeDSAuthParameters\\\\\\\":null,\\\\\\\"ThreeDSChargeAmount\\\\\\\":2.0,\\\\\\\"ThreeDSChargeCurrency\\\\\\\":\\\\\\\"INR\\\\\\\",\\\\\\\"RemainingThreeDSBalance\\\\\\\":2.0,\\\\\\\"AuthenticationId\\\\\\\":\\\\\\\"40c1c487-3882-4e26-a01f-12345678\\\\\\\",\\\\\\\"AuthenticationResults\\\\\\\":null,\\\\\\\"BillDeskTransactionId\\\\\\\":null,\\\\\\\"BillDeskTransactionDate\\\\\\\":\\\\\\\"0001-01-01T00:00:00\\\\\\\",\\\\\\\"IsCommercial\\\\\\\":false,\\\\\\\"IsFullPageRedirect\\\\\\\":true}\\\"}\",\"encrypt_data\":false,\"state\":\"Initial\",\"test_context\":{\"scenarios\":\"px-service-psd2-e2e-emulator,BillDeskForceCreateToken,BillDeskUseTestEncryption,BillDeskTokenization\",\"contact\":\"blah\",\"context_props\":{}}}";

            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", piid);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXSettings.SessionService.ArrangeResponse(sessionResponse);

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country=in&language=en-US&partner={partner}&scenario=fixedCountrySelection")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(pimsRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            if (usePartnerSettingsService)
            {
                request.Headers.Add("x-ms-flight", "PXUsePartnerSettingsService,PXDisablePSSCache");
                var pssResponse = new
                {
                    add = new
                    {
                        template = partner,
                        redirectionPattern = shouldShowIframeExperience ? "iFrame" : "fullPage"
                    }
                };

                PXSettings.PartnerSettingsService.ArrangeResponse(JsonConvert.SerializeObject(pssResponse));
            }

            if (shouldSendIframeTestHeader)
            {
                request.Headers.Add("x-ms-test", "{\"scenarios\": \"px-service-3ds1-show-iframe\", \"contact\": \"TestApp\"}");
            }

            HttpResponseMessage result = await PXClient.SendAsync(request);

            var pidlResource = ReadSinglePidlResourceFromJson(await result.Content.ReadAsStringAsync());
            Assert.IsNotNull(pidlResource.ClientAction, "Client action missing");
            Assert.AreEqual(ClientActionType.Pidl, pidlResource.ClientAction.ActionType);
            Assert.IsNotNull(pidlResource.ClientAction.Context, "Client action context missing");

            var pidlList = pidlResource.ClientAction.Context as List<PIDLResource>;
            Assert.AreEqual(1, pidlList.Count);
            var resource = pidlList[0];

            if (shouldShowIframeExperience)
            {
                var iFrame = resource.GetDisplayHintById("threeDSIframe") as IFrameDisplayHint;
                var mockRedirectUrl = $"https://mockRedirectUrl.com/?ru=https%3A%2F%2Fpifd.cp.microsoft-int.com%2FV6.0%2FpaymentInstrumentsEx%2FAccount001-3DS1-Redirect%2Fresume%3Fcountry%3Din%26language%3Den-US%26partner%3D{partner}%26isSuccessful%3DTrue%26sessionQueryUrl%3Dsessions%2Fabcd-12345&rx=https%3A%2F%2Fpifd.cp.microsoft-int.com%2FV6.0%2FpaymentInstrumentsEx%2FAccount001-3DS1-Redirect%2Fresume%3Fcountry%3Din%26language%3Den-US%26partner%3D{partner}%26isSuccessful%3DFalse%26sessionQueryUrl%3Dsessions%2Fabcd-12345";
                Assert.IsNotNull(iFrame, "iFrame is missing");
                Assert.IsTrue(iFrame.DisplayContent.Contains(mockRedirectUrl), "iFrame source url not as expected");
                Assert.IsNotNull(iFrame.DisplayTags, "Accessibility display tags are missing");
                Assert.AreEqual(iFrame.DisplayTags["accessibilityName"], "The bank authentication dialog");
            }
            else
            {
                var iFrame = resource.GetDisplayHintById("threeDSIframe") as IFrameDisplayHint;
                Assert.IsNull(iFrame, "iFrame should not exist");
                Assert.IsTrue(resource.DisplayPages[0].DisplayName == "RedirectContinuePage");
                Assert.IsTrue(resource.DisplayPages[1].DisplayName == "StatusCheckPage");
            }
        }

        // For commercial stores, the displayName for check PM should be overridden for all languages.
        [DataRow("offline_bank_transfer", "check", "commercialstores", "en-us", "Wire Transfer")]
        [DataRow("offline_bank_transfer", "check", "commercialstores", "pt-br", "Transferência Bancária")]
        [DataRow("offline_bank_transfer", "check", "commercialstores", "zh-cn", "电汇")]

        [DataRow("offline_bank_transfer", "check", "MACManage", "en-us", "Wire Transfer")]
        [DataRow("offline_bank_transfer", "check", "MACManage", "pt-br", "Transferência Bancária")]
        [DataRow("offline_bank_transfer", "check", "MACManage", "zh-cn", "电汇")]

        // For all other partners, it should remain what was returned from PIMS
        [DataRow("offline_bank_transfer", "check", "webblends", "en-us", "Check")]
        [DataRow("offline_bank_transfer", "check", "oxowebdirect", "en-us", "Check")]
        [DataRow("offline_bank_transfer", "check", "cart", "en-us", "Check")]
        [DataRow("offline_bank_transfer", "check", "xbox", "en-us", "Check")]
        [DataRow("offline_bank_transfer", "check", "amcweb", "en-us", "Check")]
        [DataRow("offline_bank_transfer", "check", "bing", "en-us", "Check")]
        [DataRow("offline_bank_transfer", "check", "azure", "en-us", "Check")]
        [DataRow("offline_bank_transfer", "check", "azuresignup", "en-us", "Check")]
        [DataRow("offline_bank_transfer", "check", "azureibiza", "en-us", "Check")]
        [DataRow("offline_bank_transfer", "check", "commercialwebblends", "en-us", "Check")]

        // For azure, the displayName for legacy_invoice PM should no longer be overridden for all languages.
        [DataRow("virtual", "legacy_invoice", "azure", "en-us", "Invoice")]
        [DataRow("virtual", "legacy_invoice", "officesmb", "en-us", "Invoice")]

        // legacy_invoice is not allowed except for azure
        // For all other partners, it should remain what was returned from PIMS
        // [DataRow("virtual", "legacy_invoice", "webblends", "en-us", "Invoice")]
        // [DataRow("virtual", "legacy_invoice", "cart", "en-us", "Invoice")]
        // [DataRow("virtual", "legacy_invoice", "xbox", "en-us", "Invoice")]
        // [DataRow("virtual", "legacy_invoice", "amcweb", "en-us", "Invoice")]
        // [DataRow("virtual", "legacy_invoice", "bing", "en-us", "Invoice")]
        // [DataRow("virtual", "legacy_invoice", "commercialstores", "en-us", "Invoice")]
        // [DataRow("virtual", "legacy_invoice", "commercialwebblends", "en-us", "Invoice")]
        [DataTestMethod]
        public async Task ListPaymentInstrumentsEx_DisplayNamesAreOverriddenAsExpected(string paymentMethodFamily, string paymentMethodType, string partner, string language, string expectedDisplayNameForOverriddenPM)
        {
            if (string.Equals(partner, "officesmb"))
            {
                string pssmockResponse = "{\"default\":{\"template\":\"defaulttemplate\",\"features\":{\"enableVirtualFamilyPM\":{\"applicableMarkets\":[]}}}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(pssmockResponse);
            }

            if (string.Equals(partner, "MACManage"))
            {
                string pssmockResponse = "{\"default\":{\"template\":\"listpidropdown\",\"redirectionPattern\":\"inline\",\"features\":{\"overrideCheckDisplayNameToWireTransfer\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":null}}}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(pssmockResponse);
            }

            var pis = await ListPIFromPXService(
                string.Format(
                    "/v7.0/eb8c2364-f51e-4fe2-a757-1e12fd50eaa6/paymentInstrumentsEx?language={0}&partner={1}",
                    language,
                    partner));

            var targetPi = pis.FirstOrDefault(pi => pi.PaymentMethod.EqualByFamilyAndType(paymentMethodFamily, paymentMethodType));

            Assert.AreEqual(expectedDisplayNameForOverriddenPM, targetPi?.PaymentMethod?.Display?.Name, "PaymentMethod.Display.Name is expected to match the expected override value");
            Assert.AreEqual(expectedDisplayNameForOverriddenPM, targetPi?.PaymentInstrumentDetails?.DefaultDisplayName, "PaymentMethod.Display.Name is expected to match the expected override value");
        }

        [DataRow("officesmb")]
        [DataTestMethod]
        public async Task PaymentInstrumentsEx_AddVirtualLegacy_PSS(string partner)
        {
            // Arrange
            string pssmockResponse = "{\"default\":{\"template\":\"defaulttemplate\",\"features\":{\"addressValidation\":{\"applicableMarkets\":[]},\"updatePIaddressToAccount\":{\"applicableMarkets\":[]}}}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(pssmockResponse);

            string url = $"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}&scenario=azureibiza&billableAccountId={CommerceAccountDataAccessor.BillingAccountId.AzureBusinessAccount}";
            var piPayload = new
            {
                paymentMethodFamily = "virtual",
                paymentMethodType = "legacy_invoice",
                paymentMethodOperation = "add",
                paymentMethodCountry = "us",
                paymentMethodResource_id = "virtual.legacy_invoice",
                sessionId = "f895f679-a8cf-1c44-e675-90d0d4e4425b",
                context = "purchase",
                riskData = new
                {
                    dataType = "payment_method_riskData",
                    dataOperation = "add",
                    dataCountry = "us"
                },
                details = new
                {
                    dataType = "virtual_legacy_invoice_details",
                    dataOperation = "add",
                    dataCountry = "us",
                    accountHolderName = "accountname",
                    companyPONumber = "SO22",
                    address = new
                    {
                        addressType = "billing",
                        addressOperation = "add",
                        addressCountry = "us",
                        address_line1 = "One Microsoft Way",
                        city = "Redmond",
                        region = "wa",
                        postal_code = "98052",
                        country = "us"
                    }
                }
            };

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi004-LegacyInvoice");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI), HttpStatusCode.OK);

            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl(url)),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(piPayload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            request.Headers.Add("x-ms-clientcontext-encoding", "base64");
            request.Headers.Add("x-ms-aadinfo", "tid=Y2ViMTM2ZmYtZGEzNC00NDkyLTkyNGQtNWY2MTIxMTFmNjc5,oid=ZjAyYmI0N2YtNzI4Ny00OTdlLWI2NTEtYTk0ZmRiM2RmM2Fk,altSecId=MTA1NTUxOTQxMTM0MDUwOQ==");

            // Act
            var response = await PXClient.SendAsync(request);

            // Assert
            string piJsonOrError = await response.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, piJsonOrError);
            var pi = JsonConvert.DeserializeObject<PaymentInstrument>(piJsonOrError);

            Assert.IsNotNull(pi);
            Assert.AreEqual(expectedPI.PaymentMethod.PaymentMethodFamily, pi.PaymentMethod.PaymentMethodFamily);
            Assert.AreEqual(expectedPI.PaymentMethod.PaymentMethodType, pi.PaymentMethod.PaymentMethodType);

            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PimsService.ResetToDefaults();
        }

        [DataRow("ewallet", "alipay", "amc", "en-us", "paynow", "Alipay")]
        [DataTestMethod]
        public async Task AddPI_DefaultDisplayName(string paymentMethodFamily, string paymentMethodType, string partner, string language, string scenario, string expecteddefaultDisplayName)
        {
            var addPi = await ListPIFromPXService(
                string.Format(
                    "/v7.0/eb8c2364-f51e-4fe2-a757-1e12fd50eaa6/paymentInstrumentsEx?country=cn&language={0}&partner={1}&scenario={2}",
                    language,
                    partner,
                    scenario));

            var targetPi = addPi.FirstOrDefault(pi => pi.PaymentMethod.EqualByFamilyAndType(paymentMethodFamily, paymentMethodType));

            Assert.IsNotNull(targetPi.PaymentInstrumentDetails.DefaultDisplayName);
            Assert.AreEqual(expecteddefaultDisplayName, targetPi.PaymentInstrumentDetails.DefaultDisplayName);
        }

        /// <summary>
        /// This CIT is designed to test the SEPA Picv payment instrument with various partners and redirection types.
        /// Note: Redirect client action type is only supported for full-page redirection.
        /// </summary>
        /// <param name="piId">The piId contains static data.</param>
        /// <param name="hasClientAction">Indicates whether the client action is present.</param>
        /// <param name="clientActionType">Specifies the type of client action, which can be either 'redirect' or 'pidl'.</param>
        /// <param name="redirectionType">Defines the redirection type, which can be 'full-page' or 'inline'.</param>
        [DataRow("SepaPicvAccount-Pi001-Redirect", true, "Redirect", "Account001-PI001-fullPageRedirectionDefaultTemplate")]
        [DataRow("SepaPicvAccount-Pi001-Redirect", true, "Redirect", "Account001-PI001-InlineRedirectionDefaultTemplate")]
        [DataRow("SepaPicvAccount-Pi001-Challenge", true, "Pidl", "Account001-PI001-fullPageRedirectionDefaultTemplate")]
        [DataRow("SepaPicvAccount-Pi001-Challenge", true, "Pidl", "Account001-PI001-InlineRedirectionDefaultTemplate")]
        [DataRow("SepaPicvAccount-Pi001-Valid", false, "", "Account001-PI001-fullPageRedirectionDefaultTemplate")]
        [DataRow("SepaPicvAccount-Pi001-Valid", false, "", "Account001-PI001-InlineRedirectionDefaultTemplate")]
        [DataRow("SepaPicvAccount-Pi016-Challenge", false, "", "Account001-PI001-fullPageRedirectionDefaultTemplate")]
        [DataRow("SepaPicvAccount-Pi016-Challenge", false, "", "Account001-PI001-InlineRedirectionDefaultTemplate")]
        [DataTestMethod]
        public async Task GetPaymentInstrumentsEx_SepaPicv(string piId, bool hasClientAction, string clientActionType, string redirectionType)
        {
            // Arrange
            List<string> partners = new List<string> { Constants.PartnerNames.Bing, Constants.PartnerNames.Webblends, Constants.PartnerNames.DefaultTemplate, Constants.VirtualPartnerNames.OfficeSmb };
            
            var emptyRequestBody = new
            {
                paymentMethodFamily = "direct_debit",
                paymentMethodType = "sepa"
            };

            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("SepaPicvAccount", piId);

            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            // Arrange
            foreach (string partner in partners)
            {
                string submitButtonHintId = string.Equals(partner, Constants.PartnerNames.Bing, StringComparison.OrdinalIgnoreCase) ? Constants.DisplayHintIds.VerifyPicvButton : Constants.DisplayHintIds.SaveNextButton;

                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(GetPXServiceUrl($"/v7.0/SepaPicvAccount/paymentInstrumentsEx?country=de&language=en-US&partner={partner}")),
                    Method = HttpMethod.Post,
                    Content = new StringContent(JsonConvert.SerializeObject(emptyRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
                };

                if (string.Equals(partner, Constants.VirtualPartnerNames.OfficeSmb, StringComparison.OrdinalIgnoreCase))
                {
                    var partnerSettingResponse = PartnerSettingsServiceMockResponseProvider.GetPSSMockResponseById(redirectionType);
                    PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
                }

                request.Headers.Add("x-ms-flight", "PXDisablePSSCache");
                HttpResponseMessage result = await PXClient.SendAsync(request);
                string piJson = await result.Content.ReadAsStringAsync();
                var pi = JsonConvert.DeserializeObject<PaymentInstrument>(piJson);

                // Assert
                if (string.Equals(piId, "SepaPicvAccount-Pi016-Challenge"))
                {
                    JObject jsonObj = JObject.Parse(piJson);

                    // Assert is used to check when the exception is throw.
                    Assert.AreEqual(result.StatusCode, HttpStatusCode.InternalServerError);
                    Assert.AreEqual("InvalidPicvDetailsPayload", jsonObj.SelectToken("ErrorCode").Value<string>());
                    Assert.AreEqual("Error Integrating with Service : InstrumentManagementService Message: There must be a non-empty 'remainingAttemps' property for 'inProgress' SEPA PI", jsonObj.SelectToken("Message").Value<string>());
                }
                else
                {
                    Assert.AreEqual(pi.ClientAction != null, hasClientAction, "Sepa PI " + pi.PaymentInstrumentId + " has issue with ClientAction");

                    if (hasClientAction)
                    {
                        Assert.AreEqual(pi.ClientAction.ActionType.ToString(), clientActionType, "Sepa PI " + pi.PaymentInstrumentId + " has issue with ClientAction");
                        PIDLResource pidl = ReadSinglePidlResourceFromJson(piJson);
                        Assert.IsNotNull(pidl, "PIDL resource should not be null");
                        Assert.IsNotNull(pidl.ClientAction, "Client action should not be null");
                        Assert.IsNotNull(pidl.ClientAction.Context, "Client action context should not be null");

                        if (clientActionType == "Redirect")
                        {
                            Assert.AreEqual(ClientActionType.Redirect, pidl.ClientAction.ActionType, "Client action type should be Redirect");

                            // Note that the 'bing' partner is part of the InlinePartners mentioned in the AddClientActionToSepaRequest method.
                            // Therefore, 'bing' will not form any RedirectPIDL. For PSS, this scenario is covered by the InlineRedirection for partners such as 'officesmb'.
                            // Partners such as 'webblends' and 'defaulttemplate' do not use the PSS setting mock response and are not in the list of InlinePartners, so the code will fall back to the else part.
                            if (string.Equals(partner, Constants.PartnerNames.Bing, StringComparison.OrdinalIgnoreCase) || (string.Equals(redirectionType, "Account001-PI001-InlineRedirectionDefaultTemplate") 
                                && string.Equals(partner, Constants.VirtualPartnerNames.OfficeSmb, StringComparison.OrdinalIgnoreCase)))
                            {
                                JObject actionContext = pidl.ClientAction.Context as JObject;
                                Assert.IsNotNull(actionContext, "Action context should be a valid JObject");
                                
                                string baseUrl = actionContext["baseUrl"].ToString();
                                Assert.IsNotNull(baseUrl, "Base URL should not be null");
                                
                                JObject successParams = actionContext["successParams"] as JObject;
                                Assert.IsNotNull(successParams, "Success parameters should not be null");
                                
                                string pendingOnStatus = successParams["pendingOn"].ToString();
                                Assert.IsNotNull(pendingOnStatus, "Pending status should not be null");
                                Assert.IsTrue(string.Equals(pendingOnStatus, clientActionType, StringComparison.OrdinalIgnoreCase), $"For {partner} Pending status should contain 'Redirect'");
                                Assert.IsNull(pi.ClientAction.RedirectPidl, $"For {partner} the pidl should be null");
                            }
                            else
                            {
                                // Note that apart from 'bing', partners like 'webblends' are not part of InlinePartners mentioned in the AddClientActionToSepaRequest method, so they will form the PIDL.
                                // For PSS, this scenario is covered by fullPageRedirection.
                                Assert.IsNotNull(pi.ClientAction.RedirectPidl, $"For {partner} the pidl should not be null");
                                var pidls = ReadPidlResourceFromJson(pi.ClientAction.RedirectPidl.ToString());
                                Assert.IsNotNull(pidls);

                                var sepaYesButton = pidls[0].GetDisplayHintById(Constants.DisplayHintIds.SepaYesButton) as ButtonDisplayHint;
                                var sepaYesButtonContext = sepaYesButton.Action.Context.ToString();

                                var sepaTryAgainButton = pidls[0].GetDisplayHintById(Constants.DisplayHintIds.SepaTryAgainButton) as ButtonDisplayHint;
                                var sepaTryAgainButtonContext = sepaTryAgainButton.Action.Context.ToString();

                                Assert.AreEqual(sepaYesButton.HintId.ToString(), Constants.DisplayHintIds.SepaYesButton, $"For {partner} sepaYesButton is not found.");
                                Assert.AreEqual(sepaTryAgainButton.HintId.ToString(), Constants.DisplayHintIds.SepaTryAgainButton, $"For {partner} sepaTryAgainButton is not found");
                                Assert.IsNotNull(sepaYesButtonContext, $"For {partner} sepaYesButton context is not found");
                                Assert.IsNotNull(sepaTryAgainButtonContext, $"For {partner} sepaTryAgainButton context is not found");
                            }
                        }
                        else
                        {
                            Assert.AreEqual(ClientActionType.Pidl, pidl.ClientAction.ActionType, "Client action type should be Pidl");
                            Assert.IsNotNull(pi.ClientAction.Context, $"For {partner} the pidl should not be null");

                            var pidls = ReadPidlResourceFromJson(pi.ClientAction.Context.ToString());
                            Assert.IsNotNull(pidls, "PIDLs should not be null");
                            
                            var sepasubmitButtonHintId = pidls[0].GetDisplayHintById(submitButtonHintId) as ButtonDisplayHint;
                            Assert.IsNotNull(sepasubmitButtonHintId, $"{submitButtonHintId} button hint ID should be present in pidl");
                            Assert.IsTrue(sepasubmitButtonHintId.Action.Context.ToString().Contains($"partner={partner}"), "Submit button hint context should contain the partner");
                        }
                    }
                    else
                    {
                        Assert.IsNull(pi.ClientAction, "The pidl should be null");
                    }
                }
            }
        }

        [DataRow("bing", "SepaPicvAccount-Pi001-Challenge", true, "Pidl", "Account001-PI001-InlineRedirectionDefaultTemplate")]
        [DataRow("officesmb", "SepaPicvAccount-Pi001-Challenge", true, "Pidl", "Account001-PI001-InlineRedirectionDefaultTemplate")]
        [DataRow("defaulttemplate", "SepaPicvAccount-Pi001-Challenge", true, "Pidl", "Account001-PI001-InlineRedirectionDefaultTemplate")]
        [DataRow("bing", "SepaPicvAccount-Pi001-Challenge", true, "Pidl", "Account001-PI001-fullPageRedirectionDefaultTemplate")]
        [DataRow("officesmb", "SepaPicvAccount-Pi001-Challenge", true, "Pidl", "Account001-PI001-fullPageRedirectionDefaultTemplate")]
        [DataRow("defaulttemplate", "SepaPicvAccount-Pi001-Challenge", true, "Pidl", "Account001-PI001-fullPageRedirectionDefaultTemplate")]
        [DataTestMethod]
        public async Task ResumePaymentInstrumentsEx_SepaPicv(string partner, string piId, bool hasClientAction, string clientActionType, string redirectionType)
        {
            // Arrange
            string submitButtonHintId = string.Equals(partner, Constants.PartnerNames.Bing, StringComparison.OrdinalIgnoreCase) ? Constants.DisplayHintIds.VerifyPicvButton : Constants.DisplayHintIds.SaveNextButton;
            var emptyRequestBody = new
            {
                paymentMethodFamily = "direct_debit",
                paymentMethodType = "sepa"
            };

            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("SepaPicvAccount", piId);

            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/SepaPicvAccount/paymentInstrumentsEx/q62zBAAAAAAJAACA/resume?country=de&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(emptyRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            if (partner.Contains("officesmb"))
            {
                var partnerSettingResponse = PartnerSettingsServiceMockResponseProvider.GetPSSMockResponseById(redirectionType);
                PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
            }

            request.Headers.Add("x-ms-flight", "PXDisablePSSCache");

            // Act
            HttpResponseMessage result = await PXClient.SendAsync(request);
            string piJson = await result.Content.ReadAsStringAsync();
            var pi = JsonConvert.DeserializeObject<PaymentInstrument>(piJson);
            
            // Assert
            Assert.AreEqual(pi.ClientAction != null, hasClientAction, "Sepa PI " + pi.PaymentInstrumentId + " has issue with ClientAction");
            if (hasClientAction)
            {
                Assert.AreEqual(pi.ClientAction.ActionType.ToString(), clientActionType, "Sepa PI " + pi.PaymentInstrumentId + " has issue with ClientAction");
            }

            var pidls = ReadPidlResourceFromJson(pi.ClientAction.Context.ToString());
            Assert.IsNotNull(pidls);
            Assert.IsNotNull(pidls[0].DisplayPages);
            var sepaSaveNextButton = pidls[0].GetDisplayHintById(submitButtonHintId) as ButtonDisplayHint;
            Assert.IsNotNull(sepaSaveNextButton);
            Assert.IsTrue(sepaSaveNextButton.Action.Context.ToString().Contains($"partner={partner}"));
        }

        [DataRow("officesmb", "de", "SepaPicvAccount-Pi001-Challenge")]
        [DataRow("officesmb", "de", "SepaPicvAccount-Pi016-Challenge")]
        [DataRow("bing", "de", "SepaPicvAccount-Pi001-Challenge")]
        [DataRow("bing", "de", "SepaPicvAccount-Pi001-Challenge", true)]
        [TestMethod]
        public async Task PaymentInstrumentsEx_SepaPicv_GetModernPI_Success(string partner, string country, string piid, bool isPXUsePSSFlightEnabled = false)
        {
            // Arrange
            string submitButtonHintId = (string.Equals(partner, Constants.PartnerNames.Bing, StringComparison.OrdinalIgnoreCase) && !isPXUsePSSFlightEnabled) ? Constants.DisplayHintIds.VerifyPicvButton : Constants.DisplayHintIds.SaveNextButton;
           
            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("SepaPicvAccount", $"{piid}");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));
           
            var additionaHeaders = new Dictionary<string, string>();

            if (string.Equals(partner, "officesmb", StringComparison.OrdinalIgnoreCase) || isPXUsePSSFlightEnabled)
            {
                if (isPXUsePSSFlightEnabled)
                {
                    additionaHeaders = new Dictionary<string, string>
                    {
                        { "x-ms-flight", "PXUsePartnerSettingsService,PXDisablePSSCache" }
                    };
                }

                var partnerSettingResponse = PartnerSettingsServiceMockResponseProvider.GetPSSMockResponseById("Account001-PI001-InlineRedirectionDefaultTemplate");
                PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
            }

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, $"/v7.0/SepaPicvAccount/paymentInstrumentsEx/{piid}?country={country}&language=en-US&partner={partner}");
            additionaHeaders?.ToList()?.ForEach(pair => request.Headers.TryAddWithoutValidation(pair.Key, pair.Value));

            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert
            string piJson = await result.Content.ReadAsStringAsync();

            if (string.Equals(piid, "SepaPicvAccount-Pi016-Challenge"))
            {
                JObject jsonObj = JObject.Parse(piJson);

                // Assert is used to check when the exception is throw.
                Assert.AreEqual(result.StatusCode, HttpStatusCode.InternalServerError);
                Assert.AreEqual("InvalidPicvDetailsPayload", jsonObj.SelectToken("ErrorCode").Value<string>());
                Assert.AreEqual("Error Integrating with Service : InstrumentManagementService Message: There must be a non-empty 'remainingAttemps' property for 'inProgress' SEPA PI", jsonObj.SelectToken("Message").Value<string>());
            }
            else
            {
                Assert.AreEqual(result.StatusCode, HttpStatusCode.OK, "Expected 200 OK response");
                PIDLResource pidl = ReadSinglePidlResourceFromJson(piJson);
                List<PIDLResource> pidlList = pidl.ClientAction.Context as List<PIDLResource>;
                Assert.IsNotNull(pidlList);
                Assert.IsNotNull(pidlList[0].DisplayPages);
                Assert.IsTrue(pidlList[0].DisplayPages[0].HintId.Contains("sepaPicvChallengePage"));

                var sepaSubmitButtonHintId = pidlList[0].GetDisplayHintById(submitButtonHintId) as ButtonDisplayHint;
                Assert.IsNotNull(sepaSubmitButtonHintId);
                Assert.IsTrue(sepaSubmitButtonHintId.Action.Context.ToString().Contains($"partner={partner}"));
            }
        }

        [DataRow("storify", "Account001-UnionPayCC-Active", false)]
        [DataRow("storify", "Account001-UnionPayCC-Active", true)]
        [DataTestMethod]
        public async Task XboxNative_ResumePaymentInstrumentsEx(string partner, string piId, bool useStyleHints)
        {
            // Arrange
            var emptyRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "unionpay_creditcard"
            };

            if (useStyleHints)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableXboxNativeStyleHints");
            }

            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001-UnionPayCC", piId);

            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001-UnionPayCC/paymentInstrumentsEx/{piId}/resume?language=en-US&partner={partner}&completePrerequisites={true}&country=cn")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(emptyRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            HttpResponseMessage result = await PXClient.SendAsync(request);
            string piJson = await result.Content.ReadAsStringAsync();
            var pi = JsonConvert.DeserializeObject<PaymentInstrument>(piJson);
            var pidls = ReadPidlResourceFromJson(pi.ClientAction.Context.ToString());

            // Assert
            Assert.IsNotNull(pidls);
            pidls[0].GetAllDisplayHints().ForEach(displayHint =>
            {
                if (useStyleHints)
                {
                    Assert.IsTrue(displayHint.StyleHints.Count > 0);
                }
                else
                {
                    Assert.IsTrue(displayHint.StyleHints == null);
                }
            });
        }

        [TestMethod]
        public async Task PaymentInstrumentsEx_Feature_PXAlipayQRCode_MakeSureFlightPassedToPims()
        {
            // Arrange
            bool assertCalled = false;
            string featureName = "PXAlipayQRCode";
            PXFlightHandler.AddToEnabledFlights(featureName);

            PXSettings.PimsService.PreProcess = (request) =>
            {
                // Assert
                Assert.IsTrue(request.Headers.Contains(GlobalConstants.HeaderValues.ExtendedFlightName), $"When feature PXAlipayQRCode is enabled request header must contain {GlobalConstants.HeaderValues.ExtendedFlightName} with value {featureName}");
                Assert.IsTrue(request.Headers.GetValues(GlobalConstants.HeaderValues.ExtendedFlightName).Contains(featureName), $"When feature PXAlipayQRCode is enabled request header must contain {GlobalConstants.HeaderValues.ExtendedFlightName} with value {featureName}");
                assertCalled = true;
            };

            var emptyRequestBody = new
            {
                paymentMethodFamily = "paymentMethodFamily",
                paymentMethodType = "paymentMethodType"
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync("/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner=webblends", new StringContent(JsonConvert.SerializeObject(emptyRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            Assert.IsTrue(assertCalled, "PimsTestHandler.PreProcess wasn't called");
            PXSettings.PimsService.ResetToDefaults();
        }

        [DataRow("amcweb", "Account001-Pi008-Paypal", "testaccount@outlook.com")]
        [DataRow("amcweb", "Account001-Pi007-Discover", "Discover Network ** 7000")]
        [DataRow("amcweb", "Account001-Pi003-Amex", "American Express ** 0001")]
        [DataRow("amcweb", "Account001-Pi002-MC", "MasterCard ** 0002")]
        [DataRow("amcweb", "Account001-Pi001-Visa", "Visa ** 5678")]
        [DataRow("webblends", "Account001-Pi002-MC", "John Doe ••0002")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_AddPIAsExpected(string partner, string piid, string expectedDefaultDisplayName)
        {
            // Arrange
            bool assertCalled = false;
            var emptyRequestBody = new
            {
                paymentMethodFamily = "paymentMethodFamily",
                paymentMethodType = "paymentMethodType"
            };

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", piid);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXSettings.PimsService.PreProcess = async (request) =>
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(requestContent);
                Assert.IsTrue(JObject.DeepEquals(json, JObject.FromObject(emptyRequestBody)), "Expected request data is not found");
                assertCalled = true;
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}", new StringContent(JsonConvert.SerializeObject(emptyRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            Assert.IsTrue(assertCalled, "PimsTestHandler.PreProcess wasn't called");
            string resultContent = await result.Content.ReadAsStringAsync();
            var pi = JsonConvert.DeserializeObject<global::Tests.Common.Model.Pims.PaymentInstrument>(resultContent);
            Assert.AreEqual(expectedPI.PaymentInstrumentId, pi.PaymentInstrumentId);
            Assert.IsNotNull(pi.PaymentInstrumentDetails.DefaultDisplayName);
            Assert.AreEqual(expectedDefaultDisplayName, pi.PaymentInstrumentDetails.DefaultDisplayName);
            PXSettings.PimsService.ResetToDefaults();
        }

        [DataRow("macmanage", true)]
        [DataRow("macmanage", false)]
        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_AddPI_ChangePartnerNameForPims(string partner, bool enableFeature)
        {
            // Arrange
            bool assertCalled = false;
            var emptyRequestBody = new
            {
                paymentMethodFamily = "paymentMethodFamily",
                paymentMethodType = "paymentMethodType"
            };

            Dictionary<string, string> partnerNameMappingForPims = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "macmanage", "commercialstores" }
            };

            List<string> testPIs = new List<string>() { "Account001-Pi008-Paypal", "Account001-Pi007-Discover", "Account001-Pi003-Amex", "Account001-Pi002-MC", "Account001-Pi001-Visa" };

            string pssmockResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"features\":{}}}";

            if (enableFeature)
            {
                pssmockResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"features\":{\"changePartnerNameForPims\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]}}}}";
            }

            PXSettings.PartnerSettingsService.ArrangeResponse(pssmockResponse);

            foreach (string piid in testPIs)
            {
                global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", piid);
                PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

                PXSettings.PimsService.PreProcess = (request) =>
                {
                    if (enableFeature)
                    {
                        Assert.IsTrue(request.RequestUri.ToString().Contains(partnerNameMappingForPims[partner]));
                    }
                    else
                    {
                        Assert.IsTrue(request.RequestUri.ToString().Contains(partner));
                    }

                    assertCalled = true;
                };

                // Act
                HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}", new StringContent(JsonConvert.SerializeObject(emptyRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

                // Assert (continuation)
                Assert.IsTrue(assertCalled, "PimsTestHandler.PreProcess wasn't called");
                string resultContent = await result.Content.ReadAsStringAsync();
                var pi = JsonConvert.DeserializeObject<global::Tests.Common.Model.Pims.PaymentInstrument>(resultContent);
                Assert.AreEqual(expectedPI.PaymentInstrumentId, pi.PaymentInstrumentId);

                PXSettings.PimsService.ResetToDefaults();
            }

            PXSettings.PartnerSettingsService.ResetToDefaults();
        }

        [DataRow("xbox", "Account001-Pi012-Kakaopay", "카카오페이", "kr")]
        [DataRow("storify", "Account001-Pi012-Kakaopay", "카카오페이", "kr")]
        [DataRow("saturn", "Account001-Pi012-Kakaopay", "카카오페이", "kr")]
        [DataRow("xboxsubs", "Account001-Pi012-Kakaopay", "카카오페이", "kr")]
        [DataRow("xboxsettings", "Account001-Pi012-Kakaopay", "카카오페이", "kr")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_Kakaopay_GetPIAsExpected(string partner, string piid, string expectedDefaultDisplayName, string country)
       {
            // Arrange
            bool assertCalled = false;
            var emptyRequestBody = new
            {
                paymentMethodFamily = "paymentMethodFamily",
                paymentMethodType = "paymentMethodType"
            };

            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", piid);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXSettings.PimsService.PreProcess = async (request) =>
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(requestContent);
                Assert.IsTrue(JObject.DeepEquals(json, JObject.FromObject(emptyRequestBody)), "Expected request data is not found");
                assertCalled = true;
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}", new StringContent(JsonConvert.SerializeObject(emptyRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            Assert.IsFalse(assertCalled, "PimsTestHandler.PreProcess wasn't called");
            string resultContent = await result.Content.ReadAsStringAsync();
            var pi = JsonConvert.DeserializeObject<PaymentInstrument>(resultContent);
            Assert.AreEqual(expectedPI.PaymentInstrumentId, pi.PaymentInstrumentId);
            Assert.IsNotNull(pi.PaymentInstrumentDetails.DefaultDisplayName);
            Assert.AreEqual(expectedDefaultDisplayName, pi.PaymentInstrumentDetails.DefaultDisplayName);
            PXSettings.PimsService.ResetToDefaults();
        }

        [DataRow("storify", "Account001-Pi013-KakaopayRedirect", "카카오페이", "kr")]
        [DataRow("saturn", "Account001-Pi013-KakaopayRedirect", "카카오페이", "kr")]
        [DataRow("xboxsubs", "Account001-Pi013-KakaopayRedirect", "카카오페이", "kr")]
        [DataRow("xboxsettings", "Account001-Pi013-KakaopayRedirect", "카카오페이", "kr")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_KakaopayRedirect_AddPIAsExpected(string partner, string piid, string expectedDefaultDisplayName, string country)
        {
            // Arrange
            var emptyRequestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "kakaopay"
            };

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", piid);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            // Act
            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}", new StringContent(JsonConvert.SerializeObject(emptyRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            string resultContent = await result.Content.ReadAsStringAsync();
            var pi = JsonConvert.DeserializeObject<PaymentInstrument>(resultContent);
            Assert.AreEqual(expectedPI.PaymentInstrumentId, pi.PaymentInstrumentId);
            Assert.IsNotNull(pi.PaymentInstrumentDetails.DefaultDisplayName);
            Assert.AreEqual(expectedDefaultDisplayName, pi.PaymentInstrumentDetails.DefaultDisplayName);

            // verify the PIDL elements
            Assert.AreEqual(ClientActionType.Pidl.ToString(), pi.ClientAction.ActionType.ToString());
            var pidls = ReadPidlResourceFromJson(pi.ClientAction.Context.ToString());
            Assert.AreEqual(1, pidls.Count);
            Assert.AreEqual(3, pidls[0].DisplayPages[0].Members.Count);
            var headingElement = pidls[0].DisplayPages[0].Members[0] as HeadingDisplayHint;
            Assert.IsNotNull(headingElement);
            var iframeElement = pidls[0].DisplayPages[0].Members[1] as IFrameDisplayHint;
            Assert.IsNotNull(iframeElement);
            var buttonElement = pidls[0].DisplayPages[0].Members[2] as ButtonDisplayHint;
            Assert.IsNotNull(buttonElement);
        }

        [DataRow("northstarweb", "Account001-Pi013-KakaopayRedirect", "카카오페이", "inline")]
        [DataRow("webblends", "Account001-Pi013-KakaopayRedirect", "카카오페이", "fullPage")]
        [DataRow("officesmb", "Account001-Pi013-KakaopayRedirect", "카카오페이", "fullPage")]
        [DataRow("defaulttemplate", "Account001-Pi013-KakaopayRedirect", "카카오페이", "fullPage")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_KakaopayRedirect_AddPIAsExpected_UsingPartnerSettings(string partner, string piid, string expectedDefaultDisplayName, string redirectionPattern)
        {
            // Arrange
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "kakaopay"
            };
            string country = "kr";

            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", piid);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            var pssResponse = new
            {
                add = new
                {
                    template = redirectionPattern.Equals("inline") ? "northstarweb" : "defaulttemplate",
                    redirectionPattern
                }
            };

            PXSettings.PartnerSettingsService.ArrangeResponse(JsonConvert.SerializeObject(pssResponse));
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            request.Headers.Add("x-ms-flight", "PXUsePartnerSettingsService,PXDisablePSSCache");
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert (continuation)
            string resultContent = await result.Content.ReadAsStringAsync();
            var pi = JsonConvert.DeserializeObject<PaymentInstrument>(resultContent);
            Assert.AreEqual(expectedPI.PaymentInstrumentId, pi.PaymentInstrumentId);
            Assert.IsNotNull(pi.PaymentInstrumentDetails.DefaultDisplayName);
            Assert.AreEqual(expectedDefaultDisplayName, pi.PaymentInstrumentDetails.DefaultDisplayName);

            // verify the PIDL elements
            Assert.AreEqual(ClientActionType.Redirect.ToString(), pi.ClientAction.ActionType.ToString());

            if (redirectionPattern.Equals("inline"))
            {
                Assert.IsNotNull(pi.ClientAction.Context);
                var redirectionServiceLink = JsonConvert.DeserializeObject<Microsoft.Commerce.Payments.PXCommon.RedirectionServiceLink>(pi.ClientAction.Context.ToString());
                Assert.IsNotNull(redirectionServiceLink.BaseUrl);
            }
            else
            {
                var pidls = ReadPidlResourceFromJson(pi.ClientAction.RedirectPidl.ToString());
                Assert.AreEqual(1, pidls.Count);
                Assert.AreEqual("genericredirectpidl", pidls[0].Identity["type"]);
            }
        }

        [DataRow("windowsstore", "asdfg-asdfg-asdfg-asdfg-asdfg")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_redeemCSVAsExpected_UsingPartnerSettings(string partner, string csvToken)
        {
            // Arrange
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "stored_value",
                tokenIdentifierValue = csvToken,
                actionType = "redeem"
            };
            string country = "US";
            var tokenPolicyResponse = new
            {
                asset = new
                {
                    id = "X21-85015",
                    source = "MSProduct",
                    assetValue = new
                    {
                        value = 10.3m,
                        valueMeasurement = "USD"
                    }
                },
                catalogInfo = new
                {
                    matchingAvailabilities = new List<TokenDescriptionMatchingAvailability>()
                },
                policyEvaluation = new 
                {
                    isRedeemable = true
                },
                state = "Active",
                tokenCategory = "Csv"
            };

            var purchaseResponse = new
            {
                orderState = "Purchased",
            };

            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ArrangeResponse(Constants.PSSMockResponses.PXPartnerSettingsWindowsStore);
            PXSettings.TokenPolicyService.ArrangeResponse(JsonConvert.SerializeObject(tokenPolicyResponse));
            PXSettings.PurchaseService.ArrangeResponse(JsonConvert.SerializeObject(purchaseResponse));
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            request.Headers.Add("x-ms-flight", "PXUsePartnerSettingsService");
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert (continuation)
            string resultContent = await result.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<PaymentInstrument>(resultContent);
            
            // verify the PIDL elements
            Assert.AreEqual(ClientActionType.ReturnContext.ToString(), response.ClientAction.ActionType.ToString());
        }

        [DataRow("windowsstore", "asdfg-asdfg-asdfg-asdfg-asdfg")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_redeemCSVPurchaseServiceException_UsingPartnerSettings(string partner, string csvToken)
        {
            // Arrange
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "stored_value",
                tokenIdentifierValue = csvToken,
                actionType = "redeem"
            };
            string country = "US";
            var tokenPolicyResponse = new
            {
                asset = new
                {
                    id = "X21-85015",
                    source = "MSProduct",
                    assetValue = new
                    {
                        value = 10.3m,
                        valueMeasurement = "USD"
                    }
                },
                catalogInfo = new
                {
                    matchingAvailabilities = new List<TokenDescriptionMatchingAvailability>()
                },
                policyEvaluation = new
                {
                    isRedeemable = true
                },
                state = "Active",
                tokenCategory = "Csv"
            };

            var purchaseResponse = new
            {
            };

            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ArrangeResponse(Constants.PSSMockResponses.PXPartnerSettingsWindowsStore);
            PXSettings.TokenPolicyService.ArrangeResponse(JsonConvert.SerializeObject(tokenPolicyResponse));
            PXSettings.PurchaseService.ArrangeResponse(JsonConvert.SerializeObject(purchaseResponse), HttpStatusCode.BadRequest);
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            request.Headers.Add("x-ms-flight", "PXUsePartnerSettingsService");
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert (continuation)
            string resultContent = await result.Content.ReadAsStringAsync();
            ServiceErrorResponse response = JsonConvert.DeserializeObject<ServiceErrorResponse>(resultContent);
            Assert.AreEqual("InternalError", response.ErrorCode);
        }

        [DataRow("windowsstore", "asdfg-asdfg-asdfg-asdfg-asdfg")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_redeemCSVAsFailureExpected_UsingPartnerSettings(string partner, string csvToken)
        {
            // Arrange
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "stored_value",
                tokenIdentifierValue = csvToken,
                actionType = "redeem"
            };
            string country = "US";
            var tokenPolicyResponse = new
            {
                asset = new
                {
                    id = "X21-85015",
                    source = "MSProduct",
                    assetValue = new
                    {
                        value = 10.3m,
                        valueMeasurement = "USD"
                    }
                },
                catalogInfo = new
                {
                    matchingAvailabilities = new List<TokenDescriptionMatchingAvailability>()
                },
                policyEvaluation = new
                {
                    isRedeemable = true
                },
                state = "Active",
                tokenCategory = "Csv"
            };

            var purchaseResponse = new
            {
                orderState = "None",
            };

            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ArrangeResponse(Constants.PSSMockResponses.PXPartnerSettingsWindowsStore);
            PXSettings.TokenPolicyService.ArrangeResponse(JsonConvert.SerializeObject(tokenPolicyResponse));
            PXSettings.PurchaseService.ArrangeResponse(JsonConvert.SerializeObject(purchaseResponse));
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            request.Headers.Add("x-ms-flight", "PXUsePartnerSettingsService");
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert (continuation)
            string resultContent = await result.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<PaymentInstrument>(resultContent);

            // verify the PIDL elements
            Assert.AreEqual(ClientActionType.Failure.ToString(), response.ClientAction.ActionType.ToString());
        }

        [DataRow("windowsstore", "asdfg-asdfg-asdfg-asdfg-asdfg")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_confirmCSVTokenAsExpected_UsingPartnerSettings(string partner, string csvToken)
        {
            // Arrange
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "stored_value",
                tokenIdentifierValue = csvToken,
                actionType = "validate"
            };
            string country = "US";
            var tokenPolicyResponse = new
            {
                asset = new
                {
                    id = "X21-85015",
                    source = "MSProduct",
                    assetValue = new
                    {
                        value = 10.3m,
                        valueMeasurement = "USD"
                    }
                },
                catalogInfo = new
                {
                    matchingAvailabilities = new List<TokenDescriptionMatchingAvailability>()
                },
                policyEvaluation = new
                {
                    isRedeemable = true
                },
                state = "Active",
                tokenCategory = "Csv"
            };

            var purchaseResponse = new
            {
                orderState = "Purchased",
            };

            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ArrangeResponse(Constants.PSSMockResponses.PXPartnerSettingsWindowsStore);
            PXSettings.TokenPolicyService.ArrangeResponse(JsonConvert.SerializeObject(tokenPolicyResponse));
            PXSettings.PurchaseService.ArrangeResponse(JsonConvert.SerializeObject(purchaseResponse));
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            request.Headers.Add("x-ms-flight", "PXUsePartnerSettingsService");
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert (continuation)
            string resultContent = await result.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<PaymentInstrument>(resultContent);

            // verify the PIDL elements
            Assert.AreEqual(ClientActionType.Pidl.ToString(), response.ClientAction.ActionType.ToString());
        }

        [DataRow("windowsstore", "asdfg-asdfg-asdfg-asdfg-asdfg")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_confirmCSVTokenErrorAsExpected_UsingPartnerSettings(string partner, string csvToken)
        {
            // Arrange
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "stored_value",
                tokenIdentifierValue = csvToken,
                actionType = "validate"
            };
            string country = "US";
            var tokenPolicyResponse = new
            {
                asset = new
                {
                    id = "X21-85015",
                    source = "MSProduct",
                    assetValue = new
                    {
                        value = 10.3m,
                        valueMeasurement = "USD"
                    }
                },
                catalogInfo = new
                {
                    matchingAvailabilities = new List<TokenDescriptionMatchingAvailability>()
                },
                policyEvaluation = new
                {
                    isRedeemable = true
                },
                state = "Active",
                tokenCategory = "Unknown"
            };

            var purchaseResponse = new
            {
                orderState = "Purchased",
            };

            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ArrangeResponse(Constants.PSSMockResponses.PXPartnerSettingsWindowsStore);
            PXSettings.TokenPolicyService.ArrangeResponse(JsonConvert.SerializeObject(tokenPolicyResponse));
            PXSettings.PurchaseService.ArrangeResponse(JsonConvert.SerializeObject(purchaseResponse));
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            request.Headers.Add("x-ms-flight", "PXUsePartnerSettingsService");
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert (continuation)
            string resultContent = await result.Content.ReadAsStringAsync();
            ServiceErrorResponse error = JsonConvert.DeserializeObject<ServiceErrorResponse>(resultContent);
            Assert.AreEqual(error.Message, "Invalid CSV token. Please try again.");
            Assert.AreEqual(error.ErrorCode, "NonCSVToken");
        }

        [DataRow("windowsstore", "asdfg-asdfg-asdfg-asdfg-asdfg")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_checkNonCSVCategoryCatalogContainCSVValidCSVTokenAsExpected_UsingPartnerSettings(string partner, string csvToken)
        {
            // Arrange
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "stored_value",
                tokenIdentifierValue = csvToken,
                actionType = "validate"
            };
            string country = "US";
            var tokenPolicyResponse = new
            {
                asset = new
                {
                    id = "X21-85015",
                    source = "MSProduct",
                    assetValue = new
                    {
                        value = 10.3m,
                        valueMeasurement = "USD"
                    }
                },
                catalogInfo = new
                {
                    matchingAvailabilities = new List<TokenDescriptionMatchingAvailability>()
                    {
                        new TokenDescriptionMatchingAvailability()
                    }
                },
                policyEvaluation = new
                {
                    isRedeemable = true
                },
                state = "Active",
                tokenCategory = "Other"
            };
            var purchaseResponse = new
            {
                orderState = "Purchased",
            };
            var catalogResponse = new Catalog()
            {
                Products = new List<Product>()
                {
                    new Product()
                    {
                        ProductId = "Test1_2345",
                        ProductFamily = "ewallet",
                        ProductType = "Csv"
                    }
                }
            };
            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ArrangeResponse(Constants.PSSMockResponses.PXPartnerSettingsWindowsStore);
            PXSettings.TokenPolicyService.ArrangeResponse(JsonConvert.SerializeObject(tokenPolicyResponse));
            PXSettings.PurchaseService.ArrangeResponse(JsonConvert.SerializeObject(purchaseResponse));
            PXSettings.CatalogService.ArrangeResponse(JsonConvert.SerializeObject(catalogResponse));
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            request.Headers.Add("x-ms-flight", "PXUsePartnerSettingsService");
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert (continuation)
            string resultContent = await result.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<PaymentInstrument>(resultContent);

            // verify the PIDL elements
            Assert.AreEqual(ClientActionType.Pidl.ToString(), response.ClientAction.ActionType.ToString());
            var pidls = ReadPidlResourceFromJson(JsonConvert.SerializeObject(response.ClientAction.Context));
            Assert.AreEqual(1, pidls.Count);
            Assert.AreEqual("paymentMethod", pidls[0].Identity["description_type"]);
            Assert.AreEqual("stored_value", pidls[0].Identity["type"]);
            Assert.AreEqual("confirmRedeem", pidls[0].Identity["operation"]);
            var giftCardToken = pidls[0].GetDisplayHintById("giftCardToken") as TextDisplayHint;
            Assert.IsNotNull(giftCardToken);
            Assert.AreEqual("ASDFG-ASDFG-ASDFG-ASDFG-ASDFG", giftCardToken.DisplayContent); 
        }

        [DataRow("windowsstore", "asdfg-asdfg-asdfg-asdfg-asdfg")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_checkNonCSVCategoryInvalidCSVTokenAsExpected_UsingPartnerSettings(string partner, string csvToken)
        {
            // Arrange
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "stored_value",
                tokenIdentifierValue = csvToken,
                actionType = "validate"
            };
            string country = "US";
            var tokenPolicyResponse = new
            {
                asset = new
                {
                    id = "X21-85015",
                    source = "MSProduct",
                    assetValue = new
                    {
                        value = 10.3m,
                        valueMeasurement = "USD"
                    }
                },
                catalogInfo = new
                {
                    matchingAvailabilities = new List<TokenDescriptionMatchingAvailability>()
                    {
                        new TokenDescriptionMatchingAvailability()
                    }
                },
                policyEvaluation = new
                {
                    isRedeemable = true
                },
                state = "Active",
                tokenCategory = "Other"
            };
            var purchaseResponse = new
            {
                orderState = "Purchased",
            };
            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ArrangeResponse(Constants.PSSMockResponses.PXPartnerSettingsWindowsStore);
            PXSettings.TokenPolicyService.ArrangeResponse(JsonConvert.SerializeObject(tokenPolicyResponse));
            PXSettings.PurchaseService.ArrangeResponse(JsonConvert.SerializeObject(purchaseResponse));
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            request.Headers.Add("x-ms-flight", "PXUsePartnerSettingsService");
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert (continuation)
            string resultContent = await result.Content.ReadAsStringAsync();
            ServiceErrorResponse response = JsonConvert.DeserializeObject<ServiceErrorResponse>(resultContent);
            Assert.AreEqual("NonCSVToken", response.ErrorCode);
            Assert.AreEqual("Invalid CSV token. Please try again.", response.Message);
        }

        [DataRow("windowsstore", "asdfg-asdfg-asdfg-asdfg-asdfg")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_checkNoPolicyEvalutionInvalidCSVTokenAsExpected_UsingPartnerSettings(string partner, string csvToken)
        {
            // Arrange
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "stored_value",
                tokenIdentifierValue = csvToken,
                actionType = "validate"
            };
            string country = "US";
            var tokenPolicyResponse = new
            {
                asset = new
                {
                    id = "X21-85015",
                    source = "MSProduct",
                    assetValue = new
                    {
                        value = 10.3m,
                        valueMeasurement = "USD"
                    }
                },
                catalogInfo = new
                {
                    matchingAvailabilities = new List<TokenDescriptionMatchingAvailability>()
                    {
                        new TokenDescriptionMatchingAvailability()
                    }
                },
                state = "Active",
                tokenCategory = "Csv"
            };
            var purchaseResponse = new
            {
                orderState = "Purchased",
            };
            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ArrangeResponse(Constants.PSSMockResponses.PXPartnerSettingsWindowsStore);
            PXSettings.TokenPolicyService.ArrangeResponse(JsonConvert.SerializeObject(tokenPolicyResponse));
            PXSettings.PurchaseService.ArrangeResponse(JsonConvert.SerializeObject(purchaseResponse));
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            request.Headers.Add("x-ms-flight", "PXUsePartnerSettingsService");
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert (continuation)
            string resultContent = await result.Content.ReadAsStringAsync();
            ServiceErrorResponse response = JsonConvert.DeserializeObject<ServiceErrorResponse>(resultContent);
            Assert.AreEqual("Unknown", response.ErrorCode);
            Assert.AreEqual("Invalid CSV token. Please try again.", response.Message);
        }

        [DataRow("windowsstore", "asdfg-asdfg-asdfg-asdfg-asdfg")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_checkPolicyEvaluationNotRedeemableCSVTokenAlreadyRedeemedAsExpected_UsingPartnerSettings(string partner, string csvToken)
        {
            // Arrange
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "stored_value",
                tokenIdentifierValue = csvToken,
                actionType = "validate"
            };
            string country = "US";
            var tokenPolicyResponse = new
            {
                asset = new
                {
                    id = "X21-85015",
                    source = "MSProduct",
                    assetValue = new
                    {
                        value = 10.3m,
                        valueMeasurement = "USD"
                    }
                },
                catalogInfo = new
                {
                    matchingAvailabilities = new List<TokenDescriptionMatchingAvailability>()
                    {
                        new TokenDescriptionMatchingAvailability()
                    }
                },
                policyEvaluation = new
                {
                    isRedeemable = false,
                    policyResults = new List<TokenPolicyEvaluationResult>()
                    {
                        new TokenPolicyEvaluationResult()
                        {
                            Code = "TokenNotInRedeemableState",
                        }
                    }
                },
                state = "Active",
                tokenCategory = "Csv"
            };
            var purchaseResponse = new
            {
                orderState = "Purchased",
            };
            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ArrangeResponse(Constants.PSSMockResponses.PXPartnerSettingsWindowsStore);
            PXSettings.TokenPolicyService.ArrangeResponse(JsonConvert.SerializeObject(tokenPolicyResponse));
            PXSettings.PurchaseService.ArrangeResponse(JsonConvert.SerializeObject(purchaseResponse));
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            request.Headers.Add("x-ms-flight", "PXUsePartnerSettingsService");
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert (continuation)
            string resultContent = await result.Content.ReadAsStringAsync();
            ServiceErrorResponse response = JsonConvert.DeserializeObject<ServiceErrorResponse>(resultContent);
            Assert.AreEqual("TokenAlreadyRedeemed", response.ErrorCode);
            Assert.AreEqual("Invalid CSV token. Please try again.", response.Message);
        }

        [DataRow("windowsstore", "asdfg-asdfg-asdfg-asdfg-asdfg")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_checkPolicyEvaluationNotRedeemableInvalidCSVTokenAsExpected_UsingPartnerSettings(string partner, string csvToken)
        {
            // Arrange
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "stored_value",
                tokenIdentifierValue = csvToken,
                actionType = "validate"
            };
            string country = "US";
            var tokenPolicyResponse = new
            {
                asset = new
                {
                    id = "X21-85015",
                    source = "MSProduct",
                    assetValue = new
                    {
                        value = 10.3m,
                        valueMeasurement = "USD"
                    }
                },
                catalogInfo = new
                {
                    matchingAvailabilities = new List<TokenDescriptionMatchingAvailability>()
                    {
                        new TokenDescriptionMatchingAvailability()
                    }
                },
                policyEvaluation = new
                {
                    isRedeemable = false,
                    policyResults = new List<TokenPolicyEvaluationResult>()
                    {
                        new TokenPolicyEvaluationResult()
                        {
                            Code = "TokenExpired",
                        }
                    }
                },
                state = "Active",
                tokenCategory = "Csv"
            };
            var purchaseResponse = new
            {
                orderState = "Purchased",
            };
            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ArrangeResponse(Constants.PSSMockResponses.PXPartnerSettingsWindowsStore);
            PXSettings.TokenPolicyService.ArrangeResponse(JsonConvert.SerializeObject(tokenPolicyResponse));
            PXSettings.PurchaseService.ArrangeResponse(JsonConvert.SerializeObject(purchaseResponse));
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            request.Headers.Add("x-ms-flight", "PXUsePartnerSettingsService");
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert (continuation)
            string resultContent = await result.Content.ReadAsStringAsync();
            ServiceErrorResponse response = JsonConvert.DeserializeObject<ServiceErrorResponse>(resultContent);
            Assert.AreEqual("TokenExpired", response.ErrorCode);
            Assert.AreEqual("Invalid CSV token. Please try again.", response.Message);
        }

        [DataRow("windowsstore", "asdfg-asdfg-asdfg-asdfg-asdfg")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_checkCSVTokenServiceResponseExceptionThrownAsExpected_UsingPartnerSettings(string partner, string csvToken)
        {
            // Arrange
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "stored_value",
                tokenIdentifierValue = csvToken,
                actionType = "validate"
            };
            string country = "US";
            var tokenPolicyResponse = new
            {
                code = "Unknown",
                message = "Invalid CSV token. Please try again.",
                source = "TokenPolicyService"
            };
            var purchaseResponse = new
            {
                orderState = "Purchased",
            };
            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ArrangeResponse(Constants.PSSMockResponses.PXPartnerSettingsWindowsStore);
            PXSettings.TokenPolicyService.ArrangeResponse(JsonConvert.SerializeObject(tokenPolicyResponse), HttpStatusCode.BadRequest);
            PXSettings.PurchaseService.ArrangeResponse(JsonConvert.SerializeObject(purchaseResponse));
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            request.Headers.Add("x-ms-flight", "PXUsePartnerSettingsService");
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert (continuation)
            string resultContent = await result.Content.ReadAsStringAsync();
            ServiceErrorResponse response = JsonConvert.DeserializeObject<ServiceErrorResponse>(resultContent);
            Assert.AreEqual("Unknown", response.ErrorCode);
            Assert.AreEqual("Invalid CSV token. Please try again.", response.Message);
        }

        [DataRow("windowsstore", "asdfg-asdfg-asdfg-asdfg-asdfg")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_DisableRedeemCSVFLowASExpected(string partner, string csvToken)
        {
            // Arrange
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "stored_value",
                tokenIdentifierValue = csvToken,
                actionType = "validate"
            };
            string country = "US";

            PXFlightHandler.AddToEnabledFlights("PXDisableRedeemCSVFlow");
            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ArrangeResponse(Constants.PSSMockResponses.PXPartnerSettingsWindowsStore);
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            request.Headers.Add("x-ms-flight", "PXUsePartnerSettingsService");
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert
            string resultContent = await result.Content.ReadAsStringAsync();
            ServiceErrorResponse response = JsonConvert.DeserializeObject<ServiceErrorResponse>(resultContent);
            Assert.AreEqual("CouldNotValidate", response.ErrorCode);
            Assert.AreEqual("Couldn't validate CSV token. Please try again later.", response.Message);
        }

        [DataRow("xboxsettings", "asdfg-asdfg-asdfg-asdfg-asdfg")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_xboxNative_redeemCSVAsExpected(string partner, string csvToken)
        {
            // Arrange
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "stored_value",
                tokenIdentifierValue = csvToken,
                actionType = "redeem"
            };
            string country = "US";
            var tokenPolicyResponse = new
            {
                asset = new
                {
                    id = "X21-85015",
                    source = "MSProduct",
                    assetValue = new
                    {
                        value = 10.3m,
                        valueMeasurement = "USD"
                    }
                },
                catalogInfo = new
                {
                    matchingAvailabilities = new List<TokenDescriptionMatchingAvailability>()
                },
                policyEvaluation = new
                {
                    isRedeemable = true
                },
                state = "Active",
                tokenCategory = "Csv"
            };

            var purchaseResponse = new
            {
                orderState = "Purchased",
            };

            PXSettings.TokenPolicyService.ArrangeResponse(JsonConvert.SerializeObject(tokenPolicyResponse));
            PXSettings.PurchaseService.ArrangeResponse(JsonConvert.SerializeObject(purchaseResponse));
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            request.Headers.Add("x-ms-flight", "PXEnableRedeemCSVFlow");
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert (continuation)
            string resultContent = await result.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<PaymentInstrument>(resultContent);

            // verify the PIDL elements
            Assert.AreEqual(ClientActionType.ReturnContext.ToString(), response.ClientAction.ActionType.ToString());
        }

        [DataRow("xboxsettings", "asdfg-asdfg-asdfg-asdfg-asdfg")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_xboxNative_redeemCSVPurchaseServiceException(string partner, string csvToken)
        {
            // Arrange
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "stored_value",
                tokenIdentifierValue = csvToken,
                actionType = "redeem"
            };
            string country = "US";
            var tokenPolicyResponse = new
            {
                asset = new
                {
                    id = "X21-85015",
                    source = "MSProduct",
                    assetValue = new
                    {
                        value = 10.3m,
                        valueMeasurement = "USD"
                    }
                },
                catalogInfo = new
                {
                    matchingAvailabilities = new List<TokenDescriptionMatchingAvailability>()
                },
                policyEvaluation = new
                {
                    isRedeemable = true
                },
                state = "Active",
                tokenCategory = "Csv"
            };

            var purchaseResponse = new
            {
            };

            PXSettings.TokenPolicyService.ArrangeResponse(JsonConvert.SerializeObject(tokenPolicyResponse));
            PXSettings.PurchaseService.ArrangeResponse(JsonConvert.SerializeObject(purchaseResponse), HttpStatusCode.BadRequest);
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            request.Headers.Add("x-ms-flight", "PXEnableRedeemCSVFlow");
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert (continuation)
            string resultContent = await result.Content.ReadAsStringAsync();
            ServiceErrorResponse response = JsonConvert.DeserializeObject<ServiceErrorResponse>(resultContent);
            Assert.AreEqual("InternalError", response.ErrorCode);
        }

        [DataRow("xboxsettings", "asdfg-asdfg-asdfg-asdfg-asdfg")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_xboxNative_redeemCSVAsFailureExpected(string partner, string csvToken)
        {
            // Arrange
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "stored_value",
                tokenIdentifierValue = csvToken,
                actionType = "redeem"
            };
            string country = "US";
            var tokenPolicyResponse = new
            {
                asset = new
                {
                    id = "X21-85015",
                    source = "MSProduct",
                    assetValue = new
                    {
                        value = 10.3m,
                        valueMeasurement = "USD"
                    }
                },
                catalogInfo = new
                {
                    matchingAvailabilities = new List<TokenDescriptionMatchingAvailability>()
                },
                policyEvaluation = new
                {
                    isRedeemable = true
                },
                state = "Active",
                tokenCategory = "Csv"
            };

            var purchaseResponse = new
            {
                orderState = "None",
            };

            PXSettings.TokenPolicyService.ArrangeResponse(JsonConvert.SerializeObject(tokenPolicyResponse));
            PXSettings.PurchaseService.ArrangeResponse(JsonConvert.SerializeObject(purchaseResponse));
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            request.Headers.Add("x-ms-flight", "PXEnableRedeemCSVFlow");
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert (continuation)
            string resultContent = await result.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<PaymentInstrument>(resultContent);

            // verify the PIDL elements
            Assert.AreEqual(ClientActionType.Failure.ToString(), response.ClientAction.ActionType.ToString());
        }

        [DataRow("xboxsettings", "asdfg-asdfg-asdfg-asdfg-asdfg")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_xboxNative_confirmCSVTokenAsExpected(string partner, string csvToken)
        {
            // Arrange
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "stored_value",
                tokenIdentifierValue = csvToken,
                actionType = "validate"
            };
            string country = "US";
            var tokenPolicyResponse = new
            {
                asset = new
                {
                    id = "X21-85015",
                    source = "MSProduct",
                    assetValue = new
                    {
                        value = 10.3m,
                        valueMeasurement = "USD"
                    }
                },
                catalogInfo = new
                {
                    matchingAvailabilities = new List<TokenDescriptionMatchingAvailability>()
                },
                policyEvaluation = new
                {
                    isRedeemable = true
                },
                state = "Active",
                tokenCategory = "Csv"
            };

            var purchaseResponse = new
            {
                orderState = "Purchased",
            };

            PXSettings.TokenPolicyService.ArrangeResponse(JsonConvert.SerializeObject(tokenPolicyResponse));
            PXSettings.PurchaseService.ArrangeResponse(JsonConvert.SerializeObject(purchaseResponse));
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            request.Headers.Add("x-ms-flight", "PXEnableRedeemCSVFlow");
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert (continuation)
            string resultContent = await result.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<PaymentInstrument>(resultContent);

            // verify the PIDL elements
            Assert.AreEqual(ClientActionType.Pidl.ToString(), response.ClientAction.ActionType.ToString());
        }

        [DataRow("xboxsettings", "asdfg-asdfg-asdfg-asdfg-asdfg")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_xboxNative_confirmCSVTokenErrorAsExpected(string partner, string csvToken)
        {
            // Arrange
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "stored_value",
                tokenIdentifierValue = csvToken,
                actionType = "validate"
            };
            string country = "US";
            var tokenPolicyResponse = new
            {
                asset = new
                {
                    id = "X21-85015",
                    source = "MSProduct",
                    assetValue = new
                    {
                        value = 10.3m,
                        valueMeasurement = "USD"
                    }
                },
                catalogInfo = new
                {
                    matchingAvailabilities = new List<TokenDescriptionMatchingAvailability>()
                },
                policyEvaluation = new
                {
                    isRedeemable = true
                },
                state = "Active",
                tokenCategory = "Unknown"
            };

            var purchaseResponse = new
            {
                orderState = "Purchased",
            };

            PXSettings.TokenPolicyService.ArrangeResponse(JsonConvert.SerializeObject(tokenPolicyResponse));
            PXSettings.PurchaseService.ArrangeResponse(JsonConvert.SerializeObject(purchaseResponse));
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            request.Headers.Add("x-ms-flight", "PXEnableRedeemCSVFlow");
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert (continuation)
            string resultContent = await result.Content.ReadAsStringAsync();
            ServiceErrorResponse error = JsonConvert.DeserializeObject<ServiceErrorResponse>(resultContent);
            Assert.AreEqual(error.Message, "Invalid CSV token. Please try again.");
            Assert.AreEqual(error.ErrorCode, "NonCSVToken");
        }

        [DataRow("xboxsettings", "asdfg-asdfg-asdfg-asdfg-asdfg")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_xboxNative_checkNonCSVCategoryCatalogContainCSVValidCSVTokenAsExpected(string partner, string csvToken)
        {
            // Arrange
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "stored_value",
                tokenIdentifierValue = csvToken,
                actionType = "validate"
            };
            string country = "US";
            var tokenPolicyResponse = new
            {
                asset = new
                {
                    id = "X21-85015",
                    source = "MSProduct",
                    assetValue = new
                    {
                        value = 10.3m,
                        valueMeasurement = "USD"
                    }
                },
                catalogInfo = new
                {
                    matchingAvailabilities = new List<TokenDescriptionMatchingAvailability>()
                    {
                        new TokenDescriptionMatchingAvailability()
                    }
                },
                policyEvaluation = new
                {
                    isRedeemable = true
                },
                state = "Active",
                tokenCategory = "Other"
            };
            var purchaseResponse = new
            {
                orderState = "Purchased",
            };
            var catalogResponse = new Catalog()
            {
                Products = new List<Product>()
                {
                    new Product()
                    {
                        ProductId = "Test1_2345",
                        ProductFamily = "ewallet",
                        ProductType = "Csv"
                    }
                }
            };
            PXSettings.TokenPolicyService.ArrangeResponse(JsonConvert.SerializeObject(tokenPolicyResponse));
            PXSettings.PurchaseService.ArrangeResponse(JsonConvert.SerializeObject(purchaseResponse));
            PXSettings.CatalogService.ArrangeResponse(JsonConvert.SerializeObject(catalogResponse));
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            request.Headers.Add("x-ms-flight", "PXEnableRedeemCSVFlow");
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert (continuation)
            string resultContent = await result.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<PaymentInstrument>(resultContent);

            // verify the PIDL elements
            Assert.AreEqual(ClientActionType.Pidl.ToString(), response.ClientAction.ActionType.ToString());
            var pidls = ReadPidlResourceFromJson(JsonConvert.SerializeObject(response.ClientAction.Context));
            Assert.AreEqual(1, pidls.Count);
            Assert.AreEqual("paymentMethod", pidls[0].Identity["description_type"]);
            Assert.AreEqual("stored_value", pidls[0].Identity["type"]);
            Assert.AreEqual("confirmRedeem", pidls[0].Identity["operation"]);
            var giftCardToken = pidls[0].GetDisplayHintById("giftCardToken") as TextDisplayHint;
            Assert.IsNotNull(giftCardToken);
            Assert.AreEqual("ASDFG-ASDFG-ASDFG-ASDFG-ASDFG", giftCardToken.DisplayContent);
        }

        [DataRow("xboxsettings", "asdfg-asdfg-asdfg-asdfg-asdfg")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_xboxNative_checkNonCSVCategoryInvalidCSVTokenAsExpected(string partner, string csvToken)
        {
            // Arrange
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "stored_value",
                tokenIdentifierValue = csvToken,
                actionType = "validate"
            };
            string country = "US";
            var tokenPolicyResponse = new
            {
                asset = new
                {
                    id = "X21-85015",
                    source = "MSProduct",
                    assetValue = new
                    {
                        value = 10.3m,
                        valueMeasurement = "USD"
                    }
                },
                catalogInfo = new
                {
                    matchingAvailabilities = new List<TokenDescriptionMatchingAvailability>()
                    {
                        new TokenDescriptionMatchingAvailability()
                    }
                },
                policyEvaluation = new
                {
                    isRedeemable = true
                },
                state = "Active",
                tokenCategory = "Other"
            };
            var purchaseResponse = new
            {
                orderState = "Purchased",
            };
            PXSettings.TokenPolicyService.ArrangeResponse(JsonConvert.SerializeObject(tokenPolicyResponse));
            PXSettings.PurchaseService.ArrangeResponse(JsonConvert.SerializeObject(purchaseResponse));
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            request.Headers.Add("x-ms-flight", "PXEnableRedeemCSVFlow");
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert (continuation)
            string resultContent = await result.Content.ReadAsStringAsync();
            ServiceErrorResponse response = JsonConvert.DeserializeObject<ServiceErrorResponse>(resultContent);
            Assert.AreEqual("NonCSVToken", response.ErrorCode);
            Assert.AreEqual("Invalid CSV token. Please try again.", response.Message);
        }

        [DataRow("xboxsettings", "asdfg-asdfg-asdfg-asdfg-asdfg")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_xboxNative_checkNoPolicyEvalutionInvalidCSVTokenAsExpected(string partner, string csvToken)
        {
            // Arrange
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "stored_value",
                tokenIdentifierValue = csvToken,
                actionType = "validate"
            };
            string country = "US";
            var tokenPolicyResponse = new
            {
                asset = new
                {
                    id = "X21-85015",
                    source = "MSProduct",
                    assetValue = new
                    {
                        value = 10.3m,
                        valueMeasurement = "USD"
                    }
                },
                catalogInfo = new
                {
                    matchingAvailabilities = new List<TokenDescriptionMatchingAvailability>()
                    {
                        new TokenDescriptionMatchingAvailability()
                    }
                },
                state = "Active",
                tokenCategory = "Csv"
            };
            var purchaseResponse = new
            {
                orderState = "Purchased",
            };
            PXSettings.TokenPolicyService.ArrangeResponse(JsonConvert.SerializeObject(tokenPolicyResponse));
            PXSettings.PurchaseService.ArrangeResponse(JsonConvert.SerializeObject(purchaseResponse));
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            request.Headers.Add("x-ms-flight", "PXEnableRedeemCSVFlow");
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert (continuation)
            string resultContent = await result.Content.ReadAsStringAsync();
            ServiceErrorResponse response = JsonConvert.DeserializeObject<ServiceErrorResponse>(resultContent);
            Assert.AreEqual("Unknown", response.ErrorCode);
            Assert.AreEqual("Invalid CSV token. Please try again.", response.Message);
        }

        [DataRow("xboxsettings", "asdfg-asdfg-asdfg-asdfg-asdfg")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_xboxNative_checkPolicyEvaluationNotRedeemableCSVTokenAlreadyRedeemedAsExpected(string partner, string csvToken)
        {
            // Arrange
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "stored_value",
                tokenIdentifierValue = csvToken,
                actionType = "validate"
            };
            string country = "US";
            var tokenPolicyResponse = new
            {
                asset = new
                {
                    id = "X21-85015",
                    source = "MSProduct",
                    assetValue = new
                    {
                        value = 10.3m,
                        valueMeasurement = "USD"
                    }
                },
                catalogInfo = new
                {
                    matchingAvailabilities = new List<TokenDescriptionMatchingAvailability>()
                    {
                        new TokenDescriptionMatchingAvailability()
                    }
                },
                policyEvaluation = new
                {
                    isRedeemable = false,
                    policyResults = new List<TokenPolicyEvaluationResult>()
                    {
                        new TokenPolicyEvaluationResult()
                        {
                            Code = "TokenNotInRedeemableState",
                        }
                    }
                },
                state = "Active",
                tokenCategory = "Csv"
            };
            var purchaseResponse = new
            {
                orderState = "Purchased",
            };
            PXSettings.TokenPolicyService.ArrangeResponse(JsonConvert.SerializeObject(tokenPolicyResponse));
            PXSettings.PurchaseService.ArrangeResponse(JsonConvert.SerializeObject(purchaseResponse));
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            request.Headers.Add("x-ms-flight", "PXEnableRedeemCSVFlow");
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert (continuation)
            string resultContent = await result.Content.ReadAsStringAsync();
            ServiceErrorResponse response = JsonConvert.DeserializeObject<ServiceErrorResponse>(resultContent);
            Assert.AreEqual("TokenAlreadyRedeemed", response.ErrorCode);
            Assert.AreEqual("Invalid CSV token. Please try again.", response.Message);
        }

        [DataRow("xboxsettings", "asdfg-asdfg-asdfg-asdfg-asdfg")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_xboxNative_checkPolicyEvaluationNotRedeemableInvalidCSVTokenAsExpected(string partner, string csvToken)
        {
            // Arrange
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "stored_value",
                tokenIdentifierValue = csvToken,
                actionType = "validate"
            };
            string country = "US";
            var tokenPolicyResponse = new
            {
                asset = new
                {
                    id = "X21-85015",
                    source = "MSProduct",
                    assetValue = new
                    {
                        value = 10.3m,
                        valueMeasurement = "USD"
                    }
                },
                catalogInfo = new
                {
                    matchingAvailabilities = new List<TokenDescriptionMatchingAvailability>()
                    {
                        new TokenDescriptionMatchingAvailability()
                    }
                },
                policyEvaluation = new
                {
                    isRedeemable = false,
                    policyResults = new List<TokenPolicyEvaluationResult>()
                    {
                        new TokenPolicyEvaluationResult()
                        {
                            Code = "TokenExpired",
                        }
                    }
                },
                state = "Active",
                tokenCategory = "Csv"
            };
            var purchaseResponse = new
            {
                orderState = "Purchased",
            };
            PXSettings.TokenPolicyService.ArrangeResponse(JsonConvert.SerializeObject(tokenPolicyResponse));
            PXSettings.PurchaseService.ArrangeResponse(JsonConvert.SerializeObject(purchaseResponse));
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            request.Headers.Add("x-ms-flight", "PXEnableRedeemCSVFlow");
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert (continuation)
            string resultContent = await result.Content.ReadAsStringAsync();
            ServiceErrorResponse response = JsonConvert.DeserializeObject<ServiceErrorResponse>(resultContent);
            Assert.AreEqual("TokenExpired", response.ErrorCode);
            Assert.AreEqual("Invalid CSV token. Please try again.", response.Message);
        }

        [DataRow("xboxsettings", "asdfg-asdfg-asdfg-asdfg-asdfg")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_xboxNative_checkCSVTokenServiceResponseExceptionThrownAsExpected(string partner, string csvToken)
        {
            // Arrange
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "stored_value",
                tokenIdentifierValue = csvToken,
                actionType = "validate"
            };
            string country = "US";
            var tokenPolicyResponse = new
            {
                code = "Unknown",
                message = "Invalid CSV token. Please try again.",
                source = "TokenPolicyService"
            };
            var purchaseResponse = new
            {
                orderState = "Purchased",
            };
            PXSettings.TokenPolicyService.ArrangeResponse(JsonConvert.SerializeObject(tokenPolicyResponse), HttpStatusCode.BadRequest);
            PXSettings.PurchaseService.ArrangeResponse(JsonConvert.SerializeObject(purchaseResponse));
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            request.Headers.Add("x-ms-flight", "PXEnableRedeemCSVFlow");
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert (continuation)
            string resultContent = await result.Content.ReadAsStringAsync();
            ServiceErrorResponse response = JsonConvert.DeserializeObject<ServiceErrorResponse>(resultContent);
            Assert.AreEqual("Unknown", response.ErrorCode);
            Assert.AreEqual("Invalid CSV token. Please try again.", response.Message);
        }

        [DataRow("xboxsettings", "asdfg-asdfg-asdfg-asdfg-asdfg")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_xboxNative_DisableRedeemCSVFLowASExpected(string partner, string csvToken)
        {
            // Arrange
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "stored_value",
                tokenIdentifierValue = csvToken,
                actionType = "validate"
            };
            string country = "US";

            PXFlightHandler.AddToEnabledFlights("PXDisableRedeemCSVFlow");
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            PXFlightHandler.AddToEnabledFlights("PXEnableRedeemCSVFlow");
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert
            string resultContent = await result.Content.ReadAsStringAsync();
            ServiceErrorResponse response = JsonConvert.DeserializeObject<ServiceErrorResponse>(resultContent);
            Assert.AreEqual("CouldNotValidate", response.ErrorCode);
            Assert.AreEqual("Couldn't validate CSV token. Please try again later.", response.Message);
        }

        /// <summary>
        /// This test is to verify that the paymentInstrument is returned as expected when the partner settings service is used.
        /// </summary>
        /// <param name="partner"></param>
        /// <param name="country"></param>
        /// <returns></returns>
        [DataRow("defaulttemplate", "us")]
        [DataRow("onepage", "us")]
        [DataRow("twopage", "us")]
        [DataRow("officesmb", "us")]
        [DataRow("cart", "nl")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_GlobalPI_GetPI(string partner, string country)
        {
            // Arrange
            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Paysafecard");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            // Act
            HttpResponseMessage result = await PXClient.GetAsync($"/v7.0/Account001/paymentInstrumentsEx/Account001-Paysafecard?country={country}&language=en-us&partner={partner}");

            // Assert (continuation)
            string resultContent = await result.Content.ReadAsStringAsync();
            var pi = JsonConvert.DeserializeObject<global::Tests.Common.Model.Pims.PaymentInstrument>(resultContent);
            Assert.AreEqual(pi.PaymentInstrumentId, "Account001-Paysafecard");
            Assert.AreEqual(pi.PaymentMethod.PaymentMethodType, "paysafecard");
        }

        [DataRow("xbox", "Account001-Pi008-Paypal", true)]
        [DataRow("amcweb", "Account001-Pi008-Paypal", true)]
        [DataRow("amcweb", "Account001-Pi007-Discover", true)]
        [DataRow("cart", "Account001-Pi003-Amex", false)]
        [DataRow("cart", "Account001-Pi002-MC", false)]
        [DataRow("cart", "Account001-Pi001-Visa", true)]
        [DataRow("webblends", "Account001-Pi002-MC", true)]
        [DataRow("pssBasedpartner", "Account001-Pi008-Paypal", true)]
        [DataRow("pssBasedpartner", "Account001-Pi007-Discover", true)]
        [DataRow("pssBasedpartner", "Account001-Pi003-Amex", false)]
        [DataRow("pssBasedpartner", "Account001-Pi002-MC", false)]
        [DataRow("pssBasedpartner", "Account001-Pi001-Visa", true)]
        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_AddPI_WithoutProfile(string partner, string piid, bool completePrerequisites)
        {
            // Arrange
            var emptyRequestBody = new
            {
                paymentMethodFamily = "paymentMethodFamily",
                paymentMethodType = "paymentMethodType"
            };

            // Arranging a sample PI to be returned by PIMS
            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", piid);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            // Act
            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/AccountWithNoProfile/paymentInstrumentsEx?country=us&language=en-US&partner={partner}&completeprerequisites={completePrerequisites}", new StringContent(JsonConvert.SerializeObject(emptyRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            string resultContent = await result.Content.ReadAsStringAsync();
            var pi = JsonConvert.DeserializeObject<PaymentInstrument>(resultContent);
            Assert.AreEqual(expectedPI.PaymentInstrumentId, pi.PaymentInstrumentId);

            if (completePrerequisites)
            {
                // Verify that there is a secondary PIDL to submit address / profile.
                Assert.AreEqual(ClientActionType.Pidl.ToString(), pi.ClientAction.ActionType.ToString());
                var pidls = ReadPidlResourceFromJson(pi.ClientAction.Context.ToString());
                Assert.AreEqual(1, pidls.Count);
                Assert.AreEqual("address", pidls[0].Identity["description_type"]);
                Assert.AreEqual("billing", pidls[0].Identity["type"]);
                Assert.AreEqual("secondary", pidls[0].ScenarioContext["resourceType"]);
            }
            else
            {
                Assert.IsNull(pi.ClientAction);
            }
        }

        [DataRow("webblends", "Account001-Pi002-MC", true)]
        [DataRow("webblends", "Account001-Pi002-MC", false)]
        [DataRow("amcweb", "Account001-Pi008-Paypal", false)]
        [DataRow("amcweb", "Account001-Pi007-Discover", true)]
        [DataRow("cart", "Account001-Pi003-Amex", true)]
        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_AddPIWithNoProfile_IgnoreErrorHandlingFlight(string partner, string piid, bool useFlight)
        {
            // Arrange
            var emptyRequestBody = new
            {
                paymentMethodFamily = "paymentMethodFamily",
                paymentMethodType = "paymentMethodType"
            };

            if (useFlight)
            {
                PXFlightHandler.AddToEnabledFlights("PXAddSecondaryClientActionWithIgnoreTerminatingErrorHandling");
            }

            // Arranging a sample PI to be returned by PIMS
            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", piid);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            // Act
            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/AccountWithNoProfile/paymentInstrumentsEx?country=us&language=en-US&partner={partner}&completeprerequisites=true", new StringContent(JsonConvert.SerializeObject(emptyRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            var pi = JsonConvert.DeserializeObject<PaymentInstrument>(await result.Content.ReadAsStringAsync());
            Assert.AreEqual(expectedPI.PaymentInstrumentId, pi.PaymentInstrumentId);

            // Verify that there is a secondary PIDL to submit address / profile with error handling if flight is present
            Assert.AreEqual(ClientActionType.Pidl.ToString(), pi.ClientAction.ActionType.ToString());
            var pidls = ReadPidlResourceFromJson(pi.ClientAction.Context.ToString());
            Assert.AreEqual(1, pidls.Count);
            Assert.AreEqual("address", pidls[0].Identity["description_type"]);
            Assert.AreEqual("billing", pidls[0].Identity["type"]);
            Assert.AreEqual("secondary", pidls[0].ScenarioContext["resourceType"]);
            Assert.AreEqual(useFlight, pidls[0].ScenarioContext.ContainsKey("terminatingErrorHandling") && string.Equals("ignore", pidls[0].ScenarioContext["terminatingErrorHandling"]));
        }

        [DataRow("cart")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_AddPI_GuestUser_ChallengeRequired_Error(string partner)
        {
            // Arrange
            var pxRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa"
            };

            var pimsResponse = new
            {
                ErrorCode = "ChallengeRequired",
                Message = "The payment instrument cannot be validated.",
            };
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(pimsResponse), HttpStatusCode.BadRequest);

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(pxRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };
            request.Headers.Add("x-ms-customer", CustomerHeaderTests.CustomerHeaderTestToken);

            // Act
            HttpResponseMessage result = await PXClient.SendAsync(request);

            string resultContent = await result.Content.ReadAsStringAsync();
            JObject jsonObj = JObject.Parse(resultContent);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.AreEqual("ChallengeRequired", jsonObj.SelectToken("ErrorCode").Value<string>());
        }

        [DataRow("webblends")]
        [DataRow("cart")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_AddPI_RateLimitPerAccount(string partner)
        {
            // Arrange
            var emptyRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa"
            };

            var pimsAssertCalled = false;

            PXSettings.PimsService.PreProcess = async (pimsRequest) =>
            {
                await Task.Delay(0);
                pimsAssertCalled = true;
            };

            PXFlightHandler.AddToEnabledFlights("PX9002311");

            // Act
            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}", new StringContent(JsonConvert.SerializeObject(emptyRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // TODO: Once we cleanup the flight PX9002311, we can uncomment the following code
            // HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/blockedTestAccountId/paymentInstrumentsEx?country=us&language=en-US&partner={partner}", new StringContent(JsonConvert.SerializeObject(emptyRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.AreEqual(pimsAssertCalled, false);
            PXSettings.PimsService.ResetToDefaults();
        }

        [DataRow("amcweb", "Account001-Pi008-Paypal", "testaccount@outlook.com")]
        [DataRow("amcweb", "Account001-Pi007-Discover", "Discover Network ** 7000")]
        [DataRow("amcweb", "Account001-Pi003-Amex", "American Express ** 0001")]
        [DataRow("amcweb", "Account001-Pi002-MC", "MasterCard ** 0002")]
        [DataRow("amcweb", "Account001-Pi001-Visa", "Visa ** 5678")]
        [DataRow("webblends", "Account001-Pi002-MC", "John Doe ••0002")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_UpdatePIAsExpected(string partner, string piid, string expectedDefaultDisplayName)
        {
            // Arrange
            bool assertCalled = false;
            var emptyRequestBody = new
            {
                paymentMethodFamily = "paymentMethodFamily",
                paymentMethodType = "paymentMethodType"
            };

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", piid);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXSettings.PimsService.PreProcess = async (request) =>
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(requestContent);
                Assert.IsTrue(JObject.DeepEquals(json, JObject.FromObject(emptyRequestBody)), "Expected request data is not found");
                assertCalled = true;
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx?language=en-US&partner={partner}", new StringContent(JsonConvert.SerializeObject(emptyRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            Assert.IsTrue(assertCalled, "PimsTestHandler.PreProcess wasn't called");
            string resultContent = await result.Content.ReadAsStringAsync();
            var pi = JsonConvert.DeserializeObject<global::Tests.Common.Model.Pims.PaymentInstrument>(resultContent);
            Assert.AreEqual(expectedPI.PaymentInstrumentId, pi.PaymentInstrumentId);
            Assert.IsNotNull(pi.PaymentInstrumentDetails.DefaultDisplayName);
            Assert.AreEqual(expectedDefaultDisplayName, pi.PaymentInstrumentDetails.DefaultDisplayName);
            PXSettings.PimsService.ResetToDefaults();
        }

        /// <summary>
        /// This test is to verify the pidl for credit card of update PI.
        /// </summary>
        /// <param name="partner"></param>
        /// <param name="piid"></param>
        /// <returns></returns>
        [DataRow("azure", "Account001-Pi002-MC")]
        [DataRow("twopage", "Account001-Pi002-MC")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_UpdatePI(string partner, string piid)
        {
            // Arrange
            bool assertCalled = false;
            var emptyRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "mc"
            };

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", piid);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXSettings.PimsService.PreProcess = async (request) =>
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(requestContent);
                Assert.IsTrue(JObject.DeepEquals(json, JObject.FromObject(emptyRequestBody)), "Expected request data is not found");
                assertCalled = true;
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx/{piid}/update?language=en-US&partner={partner}", new StringContent(JsonConvert.SerializeObject(emptyRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            Assert.IsTrue(assertCalled, "PimsTestHandler.PreProcess wasn't called");
            string resultContent = await result.Content.ReadAsStringAsync();
            var pi = JsonConvert.DeserializeObject<global::Tests.Common.Model.Pims.PaymentInstrument>(resultContent);
            Assert.AreEqual(expectedPI.PaymentInstrumentId, pi.PaymentInstrumentId);
            Assert.IsNotNull(pi.PaymentInstrumentDetails.DefaultDisplayName);
            PXSettings.PimsService.ResetToDefaults();
        }

        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_AddPI_AVSInvalidZipCode()
        {
            string featureName = "PXAddressZipCodeUpdateTo9Digit";
            string verifyFeatureName = "PXAddressZipCodeUpdateTo9DigitPassVerfiedToPIMS";
            PXFlightHandler.AddToEnabledFlights(featureName);
            PXFlightHandler.AddToEnabledFlights(verifyFeatureName);

            // Arrange
            bool assertCalled = false;
            var pimsRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                details = new
                {
                    address = new
                    {
                        address_line1 = "One Microsoft Way",
                        city = "Redmond",
                        region = "WA",
                        country = "US",
                        postal_code = "11111"
                    }
                }
            };

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi002-MC");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXSettings.PimsService.PreProcess = async (request) =>
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(requestContent);
                Assert.IsTrue(JObject.DeepEquals(json, JObject.FromObject(pimsRequestBody)), "Expected request data is not found");
                assertCalled = true;
            };

            PXSettings.AccountsService.ArrangeResponse("{\"original_address\":{\"country\":\"US\",\"region\":\"wa\",\"city\":\"Redmond\",\"address_line1\":\"One Microsoft Way\",\"postal_code\":\"11111\"},\"suggested_address\":{\"country\":\"US\",\"region\":\"WA\",\"city\":\"Redmond\",\"address_line1\":\"1 Microsoft Way\",\"postal_code\":\"98052-8300\"},\"status\":\"InteractionRequired\",\"validation_message\":\"Address field invalid for property: 'PostalCode'\"}");
            string avsPayload = "{\"customer_id\":null,\"id\":null,\"country\":\"US\",\"region\":\"WA\",\"district\":null,\"city\":\"Redmond\",\"address_line1\":\"One Microsoft Way\",\"address_line2\":null,\"address_line3\":null,\"postal_code\":\"11111\",\"first_name\":null,\"first_name_pronunciation\":null,\"last_name\":null,\"middle_name\":null,\"last_name_pronunciation\":null,\"correspondence_name\":null,\"phone_number\":null,\"mobile\":null,\"fax\":null,\"telex\":null,\"email_address\":null,\"web_site_url\":null,\"street_supplement\":null,\"is_within_city_limits\":null,\"form_of_address\":null,\"address_notes\":null,\"time_zone\":null,\"latitude\":null,\"longitude\":null,\"is_avs_validated\":null,\"validate\":null,\"validation_stamp\":null,\"links\":null,\"object_type\":null,\"contract_version\":null,\"resource_status\":null,\"is_customer_consented\":null,\"is_zip_plus_4_present\":null,\"etag\":null,\"is_avs_full_validation_succeeded\":null}";
            bool avsAssertCalled = false;

            PXSettings.AccountsService.PreProcess = async (avsRequest) =>
            {
                string requestContent = await avsRequest.Content.ReadAsStringAsync();
                Assert.AreEqual(avsPayload, requestContent, "Request not as expected");
                avsAssertCalled = true;
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync("/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner=webblends", new StringContent(JsonConvert.SerializeObject(pimsRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsTrue(assertCalled, "PimsTestHandler.PreProcess was called");
            Assert.IsTrue(avsAssertCalled, "AccountsService.PreProcess wasn't called");
            PXSettings.PimsService.ResetToDefaults();
        }

        [DataRow("battlenet", "2/29", "2", "2029")]
        [DataRow("battlenet", "02/29", "02", "2029")]
        [DataRow("battlenet", "02/2029", "02", "2029")]
        [DataRow("battlenet", "2/2029", "2", "2029")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_AddPI_SplitExpiryDateToMonthAndYear(string partner, string expiryDate, string expectedExpiryMonth, string expectedExpiryYear)
        {
            // Arrange
            string accountHolderName = "testaccountname";
            bool assertCalled = false;
            var pimsRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                paymentMethodOperation = "add",
                paymentMethodCountry = "us",
                details = new
                {
                    expiryDate = expiryDate,
                    accountHolderName = accountHolderName
                }
            };

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi001-Visa");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXSettings.PimsService.PreProcess = async (request) =>
            {
                // Assert the PIMS Post PI request payload request
                string requestContent = await request.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(requestContent);

                Assert.AreEqual(expectedExpiryMonth, (string)json["details"]["expiryMonth"], "The expiryMonth in the request doesn't match the expected value.");
                Assert.AreEqual(expectedExpiryYear, (string)json["details"]["expiryYear"], "The expiryYear in the request doesn't match the expected value.");
                Assert.AreEqual(accountHolderName, (string)json["details"]["accountHolderName"], "The accountHolderName in the request doesn't match the expected value.");
                assertCalled = true;
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}", new StringContent(JsonConvert.SerializeObject(pimsRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsTrue(assertCalled, "PimsTestHandler.PreProcess was called");
            PXSettings.PimsService.ResetToDefaults();
        }

        [DataRow("battlenet", "2/29", "2", "2029")]
        [DataRow("battlenet", "02/29", "02", "2029")]
        [DataRow("battlenet", "02/2029", "02", "2029")]
        [DataRow("battlenet", "2/2029", "2", "2029")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_UpdatePI_SplitExpiryDateToMonthAndYear(string partner, string expiryDate, string expectedExpiryMonth, string expectedExpiryYear)
        {
            // Arrange
            bool assertCalled = false;
            string piid = "Account001-Pi001-Visa";
            var emptyRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                details = new
                {
                    expiryDate = expiryDate
                }
            };

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", piid);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXSettings.PimsService.PreProcess = async (request) =>
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(requestContent);

                // Assert the PIMS Post PI request payload request
                Assert.AreEqual(expectedExpiryMonth, (string)json["details"]["expiryMonth"], "The expiryMonth in the request doesn't match the expected value.");
                Assert.AreEqual(expectedExpiryYear, (string)json["details"]["expiryYear"], "The expiryYear in the request doesn't match the expected value.");
                assertCalled = true;
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx/{piid}/update?language=en-US&partner={partner}", new StringContent(JsonConvert.SerializeObject(emptyRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            Assert.IsTrue(assertCalled, "PimsTestHandler.PreProcess wasn't called");
            string resultContent = await result.Content.ReadAsStringAsync();
            var pi = JsonConvert.DeserializeObject<global::Tests.Common.Model.Pims.PaymentInstrument>(resultContent);
            Assert.AreEqual(expectedPI.PaymentInstrumentId, pi.PaymentInstrumentId);
            Assert.IsNotNull(pi.PaymentInstrumentDetails.DefaultDisplayName);
            PXSettings.PimsService.ResetToDefaults();
        }

        [DataRow("battlenet", "2/29", "2", "2029")]
        [DataRow("battlenet", "02/29", "02", "2029")]
        [DataRow("battlenet", "02/2029", "02", "2029")]
        [DataRow("battlenet", "2/2029", "2", "2029")]
        [DataTestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_AddPI_SplitExpiryDateToMonthAndYear_Anonymous(string partner, string expiryDate, string expectedExpiryMonth, string expectedExpiryYear)
        {
            // Arrange
            bool assertCalled = false;
            var status = "Completed";
            string piid = "Account001-Pi001-Visa";
            string url = $"/v7.0/paymentInstrumentsEx/create?country=us&language=en-US&partner={partner}";

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", piid);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));
            PXSettings.PaymentOrchestratorService.ArrangeResponse(JsonConvert.SerializeObject(new { Id = "pr_12345", Status = status }));

            var piPayload = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                details = new
                {
                    address = new
                    {
                        addressType = "billing",
                        addressOperation = "add",
                        addressCountry = "us",
                        address_line1 = "One Microsoft Way",
                        city = "Redmond",
                        region = "wa",
                        postal_code = "98052",
                        country = "us"
                    },
                    expiryDate = expiryDate
                }
            };

            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl(url)),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(piPayload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            string requestContextHeaderValue = $"{{\"tenantId\":\"tid\",\"tenantCustomerId\":\"tcid\",\"requestId\":\"pr_12345\",\"paymentAccountId\":\"accountid\"}}";
            request.Headers.Add("request-context", requestContextHeaderValue);

            PXSettings.PimsService.PreProcess = async (pimsRequest) =>
            {
                // Assert the PIMS Post PI request payload request
                string requestContent = await pimsRequest.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(requestContent);

                Assert.AreEqual(expectedExpiryMonth, (string)json["details"]["expiryMonth"], "The expiryMonth in the request doesn't match the expected value.");
                Assert.AreEqual(expectedExpiryYear, (string)json["details"]["expiryYear"], "The expiryYear in the request doesn't match the expected value.");
                assertCalled = true;
            };

            // Act
            var response = await PXClient.SendAsync(request);

            // Assert
            string result = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(result);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(jsonObject["requestId"]);
            Assert.AreEqual(jsonObject["status"].ToString(), status);
            Assert.IsTrue(assertCalled, "PimsTestHandler.PreProcess was called");
            PXSettings.PimsService.ResetToDefaults();
        }

        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_AddPI_AVSInvalidCity()
        {
            string featureName = "PXAddressZipCodeUpdateTo9Digit";
            string verifyFeatureName = "PXAddressZipCodeUpdateTo9DigitPassVerfiedToPIMS";
            PXFlightHandler.AddToEnabledFlights(featureName);
            PXFlightHandler.AddToEnabledFlights(verifyFeatureName);

            // Arrange
            bool assertCalled = false;
            var pimsRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                details = new
                {
                    address = new
                    {
                        address_line1 = "One Microsoft Way",
                        city = "Bellevue",
                        region = "WA",
                        country = "US",
                        postal_code = "98052"
                    }
                }
            };

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi002-MC");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXSettings.PimsService.PreProcess = async (request) =>
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(requestContent);
                Assert.IsTrue(JObject.DeepEquals(json, JObject.FromObject(pimsRequestBody)), "Expected request data is not found");
                assertCalled = true;
            };

            PXSettings.AccountsService.ArrangeResponse("{\"original_address\":{\"country\":\"US\",\"region\":\"wa\",\"city\":\"Redmond\",\"address_line1\":\"One Microsoft Way\",\"postal_code\":\"11111\"},\"suggested_address\":{\"country\":\"US\",\"region\":\"WA\",\"city\":\"Redmond\",\"address_line1\":\"1 Microsoft Way\",\"postal_code\":\"98052-8300\"},\"status\":\"InteractionRequired\",\"validation_message\":\"Address field invalid for property: 'City'\"}");
            string avsPayload = "{\"customer_id\":null,\"id\":null,\"country\":\"US\",\"region\":\"WA\",\"district\":null,\"city\":\"Bellevue\",\"address_line1\":\"One Microsoft Way\",\"address_line2\":null,\"address_line3\":null,\"postal_code\":\"98052\",\"first_name\":null,\"first_name_pronunciation\":null,\"last_name\":null,\"middle_name\":null,\"last_name_pronunciation\":null,\"correspondence_name\":null,\"phone_number\":null,\"mobile\":null,\"fax\":null,\"telex\":null,\"email_address\":null,\"web_site_url\":null,\"street_supplement\":null,\"is_within_city_limits\":null,\"form_of_address\":null,\"address_notes\":null,\"time_zone\":null,\"latitude\":null,\"longitude\":null,\"is_avs_validated\":null,\"validate\":null,\"validation_stamp\":null,\"links\":null,\"object_type\":null,\"contract_version\":null,\"resource_status\":null,\"is_customer_consented\":null,\"is_zip_plus_4_present\":null,\"etag\":null,\"is_avs_full_validation_succeeded\":null}";
            bool avsAssertCalled = false;

            PXSettings.AccountsService.PreProcess = async (avsRequest) =>
            {
                string requestContent = await avsRequest.Content.ReadAsStringAsync();
                Assert.AreEqual(avsPayload, requestContent, "Request not as expected");
                avsAssertCalled = true;
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync("/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner=webblends", new StringContent(JsonConvert.SerializeObject(pimsRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsTrue(assertCalled, "PimsTestHandler.PreProcess was called");
            Assert.IsTrue(avsAssertCalled, "AccountsService.PreProcess wasn't called");
            PXSettings.PimsService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
        }

        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_AddPI_AVSInvalidStreet()
        {
            string featureName = "PXAddressZipCodeUpdateTo9Digit";
            string verifyFeatureName = "PXAddressZipCodeUpdateTo9DigitPassVerfiedToPIMS";
            PXFlightHandler.AddToEnabledFlights(featureName);
            PXFlightHandler.AddToEnabledFlights(verifyFeatureName);

            // Arrange
            bool assertCalled = false;
            var pimsRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                details = new
                {
                    address = new
                    {
                        address_line1 = "1",
                        city = "Redmond",
                        region = "WA",
                        country = "US",
                        postal_code = "98052"
                    }
                }
            };

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi002-MC");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXSettings.PimsService.PreProcess = async (request) =>
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(requestContent);
                Assert.IsTrue(JObject.DeepEquals(json, JObject.FromObject(pimsRequestBody)), "Expected request data is not found");
                assertCalled = true;
            };

            PXSettings.AccountsService.ArrangeResponse("{\"original_address\":{\"country\":\"US\",\"region\":\"wa\",\"city\":\"Redmond\",\"address_line1\":\"One Microsoft Way\",\"postal_code\":\"11111\"},\"suggested_address\":{\"country\":\"US\",\"region\":\"WA\",\"city\":\"Redmond\",\"address_line1\":\"1 Microsoft Way\",\"postal_code\":\"98052-8300\"},\"status\":\"InteractionRequired\",\"validation_message\":\"Address field invalid for property: 'AddressLine1'\"}");
            string avsPayload = "{\"customer_id\":null,\"id\":null,\"country\":\"US\",\"region\":\"WA\",\"district\":null,\"city\":\"Redmond\",\"address_line1\":\"1\",\"address_line2\":null,\"address_line3\":null,\"postal_code\":\"98052\",\"first_name\":null,\"first_name_pronunciation\":null,\"last_name\":null,\"middle_name\":null,\"last_name_pronunciation\":null,\"correspondence_name\":null,\"phone_number\":null,\"mobile\":null,\"fax\":null,\"telex\":null,\"email_address\":null,\"web_site_url\":null,\"street_supplement\":null,\"is_within_city_limits\":null,\"form_of_address\":null,\"address_notes\":null,\"time_zone\":null,\"latitude\":null,\"longitude\":null,\"is_avs_validated\":null,\"validate\":null,\"validation_stamp\":null,\"links\":null,\"object_type\":null,\"contract_version\":null,\"resource_status\":null,\"is_customer_consented\":null,\"is_zip_plus_4_present\":null,\"etag\":null,\"is_avs_full_validation_succeeded\":null}";
            bool avsAssertCalled = false;

            PXSettings.AccountsService.PreProcess = async (avsRequest) =>
            {
                string requestContent = await avsRequest.Content.ReadAsStringAsync();
                Assert.AreEqual(avsPayload, requestContent, "Request not as expected");
                avsAssertCalled = true;
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync("/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner=webblends", new StringContent(JsonConvert.SerializeObject(pimsRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsTrue(assertCalled, "PimsTestHandler.PreProcess was called");
            Assert.IsTrue(avsAssertCalled, "AccountsService.PreProcess wasn't called");
            PXSettings.PimsService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
        }

        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_AddPI_AVSInvalidAddress()
        {
            string featureName = "PXAddressZipCodeUpdateTo9Digit";
            string verifyFeatureName = "PXAddressZipCodeUpdateTo9DigitPassVerfiedToPIMS";
            PXFlightHandler.AddToEnabledFlights(featureName);
            PXFlightHandler.AddToEnabledFlights(verifyFeatureName);

            // Arrange
            bool assertCalled = false;
            var pimsRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                details = new
                {
                    address = new
                    {
                        address_line1 = "One Microsoft Way",
                        city = "Bellevue",
                        region = "WA",
                        country = "US",
                        postal_code = "11111"
                    }
                }
            };

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi002-MC");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXSettings.PimsService.PreProcess = async (request) =>
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(requestContent);
                Assert.IsTrue(JObject.DeepEquals(json, JObject.FromObject(pimsRequestBody)), "Expected request data is not found");
                assertCalled = true;
            };

            PXSettings.AccountsService.ArrangeResponse("{\"original_address\":{\"country\":\"US\",\"region\":\"wa\",\"city\":\"Redmond\",\"address_line1\":\"One Microsoft Way\",\"postal_code\":\"11111\"},\"suggested_address\":{\"country\":\"US\",\"region\":\"WA\",\"city\":\"Redmond\",\"address_line1\":\"1 Microsoft Way\",\"postal_code\":\"98052-8300\"},\"status\":\"InteractionRequired\",\"validation_message\":\"Address field invalid for property: 'Region', 'PostalCode', 'City'\"}");
            string avsPayload = "{\"customer_id\":null,\"id\":null,\"country\":\"US\",\"region\":\"WA\",\"district\":null,\"city\":\"Bellevue\",\"address_line1\":\"One Microsoft Way\",\"address_line2\":null,\"address_line3\":null,\"postal_code\":\"11111\",\"first_name\":null,\"first_name_pronunciation\":null,\"last_name\":null,\"middle_name\":null,\"last_name_pronunciation\":null,\"correspondence_name\":null,\"phone_number\":null,\"mobile\":null,\"fax\":null,\"telex\":null,\"email_address\":null,\"web_site_url\":null,\"street_supplement\":null,\"is_within_city_limits\":null,\"form_of_address\":null,\"address_notes\":null,\"time_zone\":null,\"latitude\":null,\"longitude\":null,\"is_avs_validated\":null,\"validate\":null,\"validation_stamp\":null,\"links\":null,\"object_type\":null,\"contract_version\":null,\"resource_status\":null,\"is_customer_consented\":null,\"is_zip_plus_4_present\":null,\"etag\":null,\"is_avs_full_validation_succeeded\":null}";
            bool avsAssertCalled = false;

            PXSettings.AccountsService.PreProcess = async (avsRequest) =>
            {
                string requestContent = await avsRequest.Content.ReadAsStringAsync();
                Assert.AreEqual(avsPayload, requestContent, "Request not as expected");
                avsAssertCalled = true;
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync("/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner=webblends", new StringContent(JsonConvert.SerializeObject(pimsRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsTrue(assertCalled, "PimsTestHandler.PreProcess was called");
            Assert.IsTrue(avsAssertCalled, "AccountsService.PreProcess wasn't called");
            PXSettings.PimsService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
        }

        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_AddPI_WithoutAVS()
        {
            PXSettings.AccountsService.ArrangeResponse("{\"original_address\":{\"country\":\"US\",\"region\":\"wa\",\"city\":\"Redmond\",\"address_line1\":\"One Microsoft Way\",\"postal_code\":\"98052\"},\"suggested_address\":{\"country\":\"US\",\"region\":\"WA\",\"city\":\"Redmond\",\"address_line1\":\"1 Microsoft Way\",\"postal_code\":\"98052-8300\"},\"status\":\"Verified\",\"validation_message\":\"\"}");
            string avsPayload = "{\"customer_id\":null,\"id\":null,\"country\":\"US\",\"region\":\"WA\",\"district\":null,\"city\":\"Redmond\",\"address_line1\":\"One Microsoft Way\",\"address_line2\":null,\"address_line3\":null,\"postal_code\":\"98052\",\"first_name\":null,\"first_name_pronunciation\":null,\"last_name\":null,\"middle_name\":null,\"last_name_pronunciation\":null,\"correspondence_name\":null,\"phone_number\":null,\"mobile\":null,\"fax\":null,\"telex\":null,\"email_address\":null,\"web_site_url\":null,\"street_supplement\":null,\"is_within_city_limits\":null,\"form_of_address\":null,\"address_notes\":null,\"time_zone\":null,\"latitude\":null,\"longitude\":null,\"is_avs_validated\":null,\"validate\":null,\"validation_stamp\":null,\"links\":null,\"object_type\":null,\"contract_version\":null,\"resource_status\":null,\"is_customer_consented\":null,\"is_zip_plus_4_present\":null,\"etag\":null}";
            bool avsAssertCalled = false;

            PXSettings.AccountsService.PreProcess = async (avsRequest) =>
            {
                string requestContent = await avsRequest.Content.ReadAsStringAsync();
                Assert.AreEqual(avsPayload, requestContent, "Request not as expected");
                avsAssertCalled = true;
            };

            // Arrange
            bool assertCalled = false;
            var pimsRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                details = new
                {
                    address = new
                    {
                        address_line1 = "One Microsoft Way",
                        city = "Redmond",
                        region = "WA",
                        country = "US",
                        postal_code = "98052"
                    }
                }
            };

            var expectedPimsRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                details = new
                {
                    address = new
                    {
                        address_line1 = "One Microsoft Way",
                        city = "Redmond",
                        region = "WA",
                        country = "US",
                        postal_code = "98052",
                    }
                }
            };

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi002-MC");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXSettings.PimsService.PreProcess = async (request) =>
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(requestContent);
                Assert.IsTrue(JObject.DeepEquals(json, JObject.FromObject(expectedPimsRequestBody)), "Expected request data is not found");
                assertCalled = true;
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync("/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner=webblends", new StringContent(JsonConvert.SerializeObject(pimsRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsTrue(assertCalled, "PimsTestHandler.PreProcess wasn't called");
            Assert.IsFalse(avsAssertCalled, "AccountsService.PreProcess wasn't called");
            string resultContent = await result.Content.ReadAsStringAsync();
            JObject jsonObj = JObject.Parse(resultContent);
            JToken paymentInstumentIdToken = jsonObj.SelectToken("id");
            Assert.IsNotNull(paymentInstumentIdToken);
            Assert.AreEqual(expectedPI.PaymentInstrumentId, paymentInstumentIdToken.Value<string>());
            PXSettings.PimsService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
        }

        [DataRow("commercialstores", "SepaPicvAccount-Pi001-AddSepaPI", HttpStatusCode.InternalServerError, "InternalError", "Internal server error.", "Payment method type does not support operation.")]
        [DataRow("commercialstores", "SepaPicvAccount-Pi001-AddSepaPI", HttpStatusCode.BadRequest, "InnerError", "OperationNotSupported.", "Payment method type does not support operation.")]
        [DataRow("defaulttemplate", "SepaPicvAccount-Pi001-AddSepaPI", HttpStatusCode.InternalServerError, "InternalError", "Internal server error.", "Payment method type does not support operation.")]
        [DataRow("defaulttemplate", "SepaPicvAccount-Pi001-AddSepaPI", HttpStatusCode.BadRequest, "InnerError", "OperationNotSupported.", "Payment method type does not support operation.")]
        [DataRow("officesmb", "SepaPicvAccount-Pi001-AddSepaPI", HttpStatusCode.InternalServerError, "InternalError", "Internal server error.", "Payment method type does not support operation.")]
        [DataRow("officesmb", "SepaPicvAccount-Pi001-AddSepaPI", HttpStatusCode.BadRequest, "InnerError", "OperationNotSupported.", "Payment method type does not support operation.")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_Sepa_AddPIErrorCode_OperationNotSupported(string partner, string piid, HttpStatusCode statusCode, string errorCode, string orchErrorMessage, string pxErrorMessage)
        {
            // Arrange
            ////   bool assertCalled = false;
            var emptyRequestBody = new
            {
                paymentMethodFamily = "direct_debit",
                paymentMethodType = "sepa"
            };

            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("SepaPicvAccount", piid);

            var piResult = "{\"CorrelationId\":\"853c9804-8071-4552-9b5c-4db7632c218c\",\"ErrorCode\":\"OperationNotSupported\",\"Message\":\"Trythatagain.Somethinghappenedatourend.Waitingabitcanhelp.\",\"Source\":\"PXService\",\"InnerError\":{\"ErrorCode\":\"OperationNotSupported\",\"Message\":\"Thispaymentinstrumentdoesnotsupportthisoperation.\",\"Source\":\"PIManagementService\",\"Details\":[]}}";

            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXSettings.PimsService.ArrangeResponse(piResult, statusCode);

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/SepaPicvAccount/paymentInstrumentsEx?country=de&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(emptyRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            if (partner.Contains("officesmb"))
            {
                var partnerSettingResponse = PartnerSettingsServiceMockResponseProvider.GetPSSMockResponseById("Account001-PI001-InlineRedirectionDefaultTemplate");
                PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
            }

            request.Headers.Add("x-ms-flight", "PXDisablePSSCache");

            // Act
            HttpResponseMessage result = await PXClient.SendAsync(request);

            ////HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/SepaPicvAccount/paymentInstrumentsEx?country=de&language=en-US&partner={partner}", new StringContent(JsonConvert.SerializeObject(emptyRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Act

            ////var error = JsonConvert.DeserializeObject<global::Tests.Common.Model.ErrorResponse>(await result.Content.ReadAsStringAsync());

            // Assert
            if (result.StatusCode == HttpStatusCode.InternalServerError)
            {
                string resultContent = await result.Content.ReadAsStringAsync();
                JObject jsonObj = JObject.Parse(resultContent);
                Assert.AreEqual(errorCode, jsonObj.SelectToken("ErrorCode").Value<string>());
                Assert.AreNotEqual(errorCode, jsonObj.SelectToken("ErrorCode").Value<string>());
            }
            else if (result.StatusCode == HttpStatusCode.BadRequest)
            {
                string resultContent = await result.Content.ReadAsStringAsync();
                JObject jsonObj = JObject.Parse(resultContent);
                Assert.AreEqual(errorCode, jsonObj.SelectToken("ErrorCode").Value<string>());
            }
        }

        [DataRow("pssBasedPartner", true)]
        [DataRow("pssBasedPartner", false)]
        [TestMethod]
        public async Task PaymentInstrumentsEx_AddPIErrorCode_ConditionalFields(string partner, bool enableConditionalFields)
        {
            // Arrange
            ////   bool assertCalled = false;
            var emptyRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa"
            };

            var piResult = "{\"CorrelationId\":\"853c9804-8071-4552-9b5c-4db7632c218c\",\"ErrorCode\":\"OperationNotSupported\",\"Message\":\"Trythatagain.Somethinghappenedatourend.Waitingabitcanhelp.\",\"Source\":\"PXService\",\"InnerError\":{\"ErrorCode\":\"OperationNotSupported\",\"Message\":\"Thispaymentinstrumentdoesnotsupportthisoperation.\",\"Source\":\"PIManagementService\",\"Details\":[]}}";

            PXSettings.PimsService.ArrangeResponse(piResult, HttpStatusCode.BadRequest);

            string partnerSettingResponse = "{\"add\":{\"template\":\"defaultTemplate\",\"features\":null}}";

            if (enableConditionalFields)
            {
                partnerSettingResponse = "{\"add\":{\"template\":\"defaultTemplate\",\"features\":{\"enableConditionalFieldsForBillingAddress\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]}}}}";
            }

            PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/SepaPicvAccount/paymentInstrumentsEx?country=us&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(emptyRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            HttpResponseMessage result = await PXClient.SendAsync(request);
            string resultContent = await result.Content.ReadAsStringAsync();
            JObject jsonObj = JObject.Parse(resultContent);

            if (enableConditionalFields)
            {
                Assert.IsNotNull(jsonObj.SelectToken("clientAction"));
                Assert.AreEqual(jsonObj.SelectToken("clientAction").SelectToken("type"), "UpdatePropertyValue");
                Assert.AreEqual(jsonObj.SelectToken("clientAction").SelectToken("context").SelectToken("propertyValue"), false);
                Assert.AreEqual(jsonObj.SelectToken("clientAction").SelectToken("context").SelectToken("propertyName"), "hideAddressGroup");
            }
            else
            {
                Assert.IsNull(jsonObj.SelectToken("clientAction"));
            }
        }

        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_AddPI_AVSSucceed()
        {
            string featureName = "PXAddressZipCodeUpdateTo9Digit";
            string verifyFeatureName = "PXAddressZipCodeUpdateTo9DigitPassVerfiedToPIMS";
            PXFlightHandler.AddToEnabledFlights(featureName);
            PXFlightHandler.AddToEnabledFlights(verifyFeatureName);

            PXSettings.AccountsService.ArrangeResponse("{\"original_address\":{\"country\":\"US\",\"region\":\"wa\",\"city\":\"Redmond\",\"address_line1\":\"One Microsoft Way\",\"postal_code\":\"98052\"},\"suggested_address\":{\"country\":\"US\",\"region\":\"WA\",\"city\":\"Redmond\",\"address_line1\":\"1 Microsoft Way\",\"postal_code\":\"98052-8300\"},\"status\":\"Verified\",\"validation_message\":\"\"}");
            string avsPayload = "{\"customer_id\":null,\"id\":null,\"country\":\"US\",\"region\":\"WA\",\"district\":null,\"city\":\"Redmond\",\"address_line1\":\"One Microsoft Way\",\"address_line2\":null,\"address_line3\":null,\"postal_code\":\"98052\",\"first_name\":null,\"first_name_pronunciation\":null,\"last_name\":null,\"middle_name\":null,\"last_name_pronunciation\":null,\"correspondence_name\":null,\"phone_number\":null,\"mobile\":null,\"fax\":null,\"telex\":null,\"email_address\":null,\"web_site_url\":null,\"street_supplement\":null,\"is_within_city_limits\":null,\"form_of_address\":null,\"address_notes\":null,\"time_zone\":null,\"latitude\":null,\"longitude\":null,\"is_avs_validated\":null,\"validate\":null,\"validation_stamp\":null,\"links\":null,\"object_type\":null,\"contract_version\":null,\"resource_status\":null,\"is_customer_consented\":null,\"is_zip_plus_4_present\":null,\"etag\":null,\"is_avs_full_validation_succeeded\":null}";
            bool avsAssertCalled = false;

            PXSettings.AccountsService.PreProcess = async (avsRequest) =>
            {
                string requestContent = await avsRequest.Content.ReadAsStringAsync();
                Assert.AreEqual(avsPayload, requestContent, "Request not as expected");
                avsAssertCalled = true;
            };

            // Arrange
            bool assertCalled = false;
            var pimsRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                details = new
                {
                    address = new
                    {
                        address_line1 = "One Microsoft Way",
                        city = "Redmond",
                        region = "WA",
                        country = "US",
                        postal_code = "98052"
                    }
                }
            };

            var expectedPimsRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                details = new
                {
                    address = new
                    {
                        address_line1 = "One Microsoft Way",
                        city = "Redmond",
                        region = "WA",
                        country = "US",
                        postal_code = "98052-8300",
                        verified = "true"
                    }
                }
            };

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi002-MC");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXSettings.PimsService.PreProcess = async (request) =>
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(requestContent);
                Assert.IsTrue(JObject.DeepEquals(json, JObject.FromObject(expectedPimsRequestBody)), "Expected request data is not found");
                assertCalled = true;
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync("/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner=webblends", new StringContent(JsonConvert.SerializeObject(pimsRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsTrue(assertCalled, "PimsTestHandler.PreProcess wasn't called");
            Assert.IsTrue(avsAssertCalled, "AccountsService.PreProcess wasn't called");
            string resultContent = await result.Content.ReadAsStringAsync();
            JObject jsonObj = JObject.Parse(resultContent);
            JToken paymentInstumentIdToken = jsonObj.SelectToken("id");
            Assert.IsNotNull(paymentInstumentIdToken);
            Assert.AreEqual(expectedPI.PaymentInstrumentId, paymentInstumentIdToken.Value<string>());
            PXSettings.PimsService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
        }

        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_AddPI_AVSSucceedWithoutVerified()
        {
            string featureName = "PXAddressZipCodeUpdateTo9Digit";
            PXFlightHandler.AddToEnabledFlights(featureName);

            PXSettings.AccountsService.ArrangeResponse("{\"original_address\":{\"country\":\"US\",\"region\":\"wa\",\"city\":\"Redmond\",\"address_line1\":\"One Microsoft Way\",\"postal_code\":\"98052\"},\"suggested_address\":{\"country\":\"US\",\"region\":\"WA\",\"city\":\"Redmond\",\"address_line1\":\"1 Microsoft Way\",\"postal_code\":\"98052-8300\"},\"status\":\"Verified\",\"validation_message\":\"\"}");
            string avsPayload = "{\"customer_id\":null,\"id\":null,\"country\":\"US\",\"region\":\"WA\",\"district\":null,\"city\":\"Redmond\",\"address_line1\":\"One Microsoft Way\",\"address_line2\":null,\"address_line3\":null,\"postal_code\":\"98052\",\"first_name\":null,\"first_name_pronunciation\":null,\"last_name\":null,\"middle_name\":null,\"last_name_pronunciation\":null,\"correspondence_name\":null,\"phone_number\":null,\"mobile\":null,\"fax\":null,\"telex\":null,\"email_address\":null,\"web_site_url\":null,\"street_supplement\":null,\"is_within_city_limits\":null,\"form_of_address\":null,\"address_notes\":null,\"time_zone\":null,\"latitude\":null,\"longitude\":null,\"is_avs_validated\":null,\"validate\":null,\"validation_stamp\":null,\"links\":null,\"object_type\":null,\"contract_version\":null,\"resource_status\":null,\"is_customer_consented\":null,\"is_zip_plus_4_present\":null,\"etag\":null,\"is_avs_full_validation_succeeded\":null}";
            bool avsAssertCalled = false;

            PXSettings.AccountsService.PreProcess = async (avsRequest) =>
            {
                string requestContent = await avsRequest.Content.ReadAsStringAsync();
                Assert.AreEqual(avsPayload, requestContent, "Request not as expected");
                avsAssertCalled = true;
            };

            // Arrange
            bool assertCalled = false;
            var pimsRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                details = new
                {
                    address = new
                    {
                        address_line1 = "One Microsoft Way",
                        city = "Redmond",
                        region = "WA",
                        country = "US",
                        postal_code = "98052"
                    }
                }
            };

            var expectedPimsRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                details = new
                {
                    address = new
                    {
                        address_line1 = "One Microsoft Way",
                        city = "Redmond",
                        region = "WA",
                        country = "US",
                        postal_code = "98052-8300"
                    }
                }
            };

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi002-MC");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXSettings.PimsService.PreProcess = async (request) =>
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(requestContent);
                Assert.IsTrue(JObject.DeepEquals(json, JObject.FromObject(expectedPimsRequestBody)), "Expected request data is not found");
                assertCalled = true;
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync("/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner=webblends", new StringContent(JsonConvert.SerializeObject(pimsRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsTrue(assertCalled, "PimsTestHandler.PreProcess wasn't called");
            Assert.IsTrue(avsAssertCalled, "AccountsService.PreProcess wasn't called");
            string resultContent = await result.Content.ReadAsStringAsync();
            JObject jsonObj = JObject.Parse(resultContent);
            JToken paymentInstumentIdToken = jsonObj.SelectToken("id");
            Assert.IsNotNull(paymentInstumentIdToken);
            Assert.AreEqual(expectedPI.PaymentInstrumentId, paymentInstumentIdToken.Value<string>());
            PXSettings.PimsService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
        }

        [TestMethod]
        [DataRow("storify", "Account001-Pi009-PaypalRedirect", "testaccount@outlook.com", "https://testshorturl.ms/test")]
        [DataRow("xboxsubs", "Account001-Pi009-PaypalRedirect", "testaccount@outlook.com", "https://testshorturl.ms/test")]
        [DataRow("xboxsettings", "Account001-Pi009-PaypalRedirect", "testaccount@outlook.com", "https://testshorturl.ms/test")]
        [DataRow("saturn", "Account001-Pi009-PaypalRedirect", "testaccount@outlook.com", "https://testshorturl.ms/test")]
        public async Task PaymentInstrumentsEx_PayPal_AddPIProvidesShortUrl(string partner, string piid, string expectedDefaultDisplayName, string shortUrl)
        {
            string featureName = "PXEnableShortUrlPayPal,PXEnableShortUrlPayPalText";
            PXFlightHandler.AddToEnabledFlights(featureName);

            var pimsRequestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "paypal",
                details = new
                {
                    address = new
                    {
                        address_line1 = "One Microsoft Way",
                        city = "Redmond",
                        region = "WA",
                        country = "US",
                        postal_code = "11111"
                    }
                },
                riskData = new
                {
                }
            };

            bool assertCalled = false;

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", piid);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXSettings.PimsService.PreProcess = async (request) =>
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(requestContent);
                Assert.IsTrue(JObject.DeepEquals(json, JObject.FromObject(pimsRequestBody)), "Expected request data is not found");
                assertCalled = true;
            };

            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx?country=AT&language=en-US&partner={partner}&scenario=paypalQrCode", new StringContent(JsonConvert.SerializeObject(pimsRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            Assert.IsTrue(assertCalled, "PimsTestHandler.PreProcess wasn't called");
            string resultContent = await result.Content.ReadAsStringAsync();
            var pi = JsonConvert.DeserializeObject<global::Tests.Common.Model.Pims.PaymentInstrument>(resultContent);
            Assert.AreEqual(expectedPI.PaymentInstrumentId, pi.PaymentInstrumentId);
            Assert.IsNotNull(pi.PaymentInstrumentDetails.DefaultDisplayName);
            Assert.AreEqual(expectedDefaultDisplayName, pi.PaymentInstrumentDetails.DefaultDisplayName);

            var pidlResource = ReadSinglePidlResourceFromJson(await result.Content.ReadAsStringAsync());
            Assert.IsNotNull(pidlResource.ClientAction);
            Assert.AreEqual(ClientActionType.Pidl, pidlResource.ClientAction.ActionType);
            Assert.IsNotNull(pidlResource.ClientAction.Context);

            var pidlList = pidlResource.ClientAction.Context as List<PIDLResource>;
            Assert.AreEqual(1, pidlList.Count);
            var resource = pidlList[0];
            var shortUrlDisplayHint = resource.GetDisplayHintById("paypalPIShortUrl");
            var shortUrlHeaderDisplayHint = resource.GetDisplayHintById("paypalPIShortUrlInstruction");
            Assert.IsNotNull(shortUrlDisplayHint);
            Assert.IsNotNull(shortUrlHeaderDisplayHint);
            Assert.AreEqual(shortUrl, shortUrlDisplayHint.DisplayText());
            Assert.AreEqual("-OR- Enter this link into a browser on another device:", shortUrlHeaderDisplayHint.DisplayText());

            if (partner == "xboxsettings")
            {
                var iFrameBody = resource.GetDisplayHintById("globalPIQrCodeIframe") as IFrameDisplayHint;
                var xboxNativeSourceUrl = iFrameBody.SourceUrl.ToString();
                StringAssert.Contains(xboxNativeSourceUrl, "redirectType");
            }

            PXSettings.PimsService.ResetToDefaults();
        }

        [TestMethod]
        [DataRow("xbox", "Account001-3DS1")]
        [DataRow("amcxbox", "Account001-3DS1")]
        [DataRow("xboxsettings", "Account001-3DS1")]
        public async Task PaymentInstrumentsEx_3DS1_ReturnsPIDL(string partner, string piid)
        {
            var pimsRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                details = new
                {
                    address = new
                    {
                        address_line1 = "One Microsoft Way",
                        city = "Saket",
                        region = "DE",
                        country = "IN",
                        postal_code = "111111"
                    }
                }
            };

            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", piid);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx?country=in&language=en-US&partner={partner}&scenario=fixedCountrySelection", new StringContent(JsonConvert.SerializeObject(pimsRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            var pidlResource = ReadSinglePidlResourceFromJson(await result.Content.ReadAsStringAsync());
            Assert.IsNotNull(pidlResource.ClientAction);
            Assert.AreEqual(ClientActionType.Pidl, pidlResource.ClientAction.ActionType);
            Assert.IsNotNull(pidlResource.ClientAction.Context);

            var pidlList = pidlResource.ClientAction.Context as List<PIDLResource>;
            Assert.AreEqual(1, pidlList.Count);
            var resource = pidlList[0];
            var qrCodeImage = resource.GetDisplayHintById("ccThreeDSQrCodeImage");
            Assert.IsNotNull(qrCodeImage);

            if (partner == "xboxsettings")
            {
                var iFrameBody = resource.GetDisplayHintById("globalPIQrCodeIframe") as IFrameDisplayHint;
                var xboxNativeSourceUrl = iFrameBody.SourceUrl.ToString();
                StringAssert.Contains(xboxNativeSourceUrl, "redirectType");
            }
        }

        [TestMethod]
        [DataRow("xbox", "Account001-3DS1-Redirect", false)]
        [DataRow("xbox", "Account001-3DS1-Redirect", true)]
        [DataRow("amcxbox", "Account001-3DS1-Redirect", false)]
        [DataRow("webblends", "Account001-3DS1-Redirect", false)]
        public async Task PaymentInstrumentsEx_3DS1_FullPageRedirectPIDLS(string partner, string piid, bool shouldAddOOBEHeader)
        {
            var pimsRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                details = new
                {
                    address = new
                    {
                        address_line1 = "One Microsoft Way",
                        city = "Saket",
                        region = "DE",
                        country = "IN",
                        postal_code = "111111"
                    }
                }
            };

            var sessionResponse = "{\"id\":\"ab53ebd0-0b70-41e6-b1ec-12345678\",\"session_type\":\"any\",\"data\":\"{\\\"ProviderName\\\":\\\"BillDesk\\\",\\\"Data\\\":\\\"{\\\\\\\"Operation\\\\\\\":1,\\\\\\\"InitiationRedirectUrl\\\\\\\":\\\\\\\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\\\\\\\",\\\\\\\"InitiationParameters\\\\\\\":{\\\\\\\"MD\\\\\\\":\\\\\\\"FakeMD\\\\\\\",\\\\\\\"PaReq\\\\\\\":\\\\\\\"FakePaReq\\\\\\\",\\\\\\\"TermUrl\\\\\\\":\\\\\\\"https://www.merchanturl.com/Response.jsp\\\\\\\"},\\\\\\\"AccountId\\\\\\\":\\\\\\\"bd888d21-f2a9-4f8b-92c2-12345678\\\\\\\",\\\\\\\"IpAddress\\\\\\\":null,\\\\\\\"UserAgent\\\\\\\":null,\\\\\\\"SubscriptionId\\\\\\\":\\\\\\\"12345678\\\\\\\",\\\\\\\"ApiVersion\\\\\\\":\\\\\\\"v1_2\\\\\\\",\\\\\\\"PaymentInstrumentId\\\\\\\":\\\\\\\"f7940456-15a8-4c32-b562-12345678\\\\\\\",\\\\\\\"PaymentMethodFamily\\\\\\\":\\\\\\\"credit_card\\\\\\\",\\\\\\\"PaymentMethodType\\\\\\\":\\\\\\\"visa\\\\\\\",\\\\\\\"CvvToken\\\\\\\":\\\\\\\"FakeToken\\\\\\\",\\\\\\\"PaymentId\\\\\\\":\\\\\\\"Z10009BINTWG58e3bea8-9540-4ae3-9ddf-2e32d1e06d7b\\\\\\\",\\\\\\\"TransactionId\\\\\\\":\\\\\\\"255b9d4d-c3f1-4812-9fdd-65183ef32cb3\\\\\\\",\\\\\\\"MerchantReferenceNumber\\\\\\\":\\\\\\\"FakeReference\\\\\\\",\\\\\\\"TransactionType\\\\\\\":\\\\\\\"Validate\\\\\\\",\\\\\\\"MerchantId\\\\\\\":\\\\\\\"REDMOND\\\\\\\",\\\\\\\"TrackingId\\\\\\\":\\\\\\\"59cadaf3-480b-4dbc-bb84-12345678\\\\\\\",\\\\\\\"MandateId\\\\\\\":null,\\\\\\\"ThreeDSChargeStatus\\\\\\\":\\\\\\\"Initiated\\\\\\\",\\\\\\\"ThreeDSAuthParameters\\\\\\\":null,\\\\\\\"ThreeDSChargeAmount\\\\\\\":2.0,\\\\\\\"ThreeDSChargeCurrency\\\\\\\":\\\\\\\"INR\\\\\\\",\\\\\\\"RemainingThreeDSBalance\\\\\\\":2.0,\\\\\\\"AuthenticationId\\\\\\\":\\\\\\\"40c1c487-3882-4e26-a01f-12345678\\\\\\\",\\\\\\\"AuthenticationResults\\\\\\\":null,\\\\\\\"BillDeskTransactionId\\\\\\\":null,\\\\\\\"BillDeskTransactionDate\\\\\\\":\\\\\\\"0001-01-01T00:00:00\\\\\\\",\\\\\\\"IsCommercial\\\\\\\":false,\\\\\\\"IsFullPageRedirect\\\\\\\":true}\\\"}\",\"encrypt_data\":false,\"state\":\"Initial\",\"test_context\":{\"scenarios\":\"px-service-psd2-e2e-emulator,BillDeskForceCreateToken,BillDeskUseTestEncryption,BillDeskTokenization\",\"contact\":\"blah\",\"context_props\":{}}}";

            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", piid);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXSettings.SessionService.ArrangeResponse(sessionResponse);

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country=in&language=en-US&partner={partner}&scenario=fixedCountrySelection")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(pimsRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            if (shouldAddOOBEHeader)
            {
                request.Headers.Add("x-ms-flight", "xboxOOBE");
            }

            HttpResponseMessage result = await PXClient.SendAsync(request);

            var pidlResource = ReadSinglePidlResourceFromJson(await result.Content.ReadAsStringAsync());
            Assert.IsNotNull(pidlResource.ClientAction, "Client action missing");
            Assert.AreEqual(ClientActionType.Pidl, pidlResource.ClientAction.ActionType);
            Assert.IsNotNull(pidlResource.ClientAction.Context, "Client action context missing");

            var pidlList = pidlResource.ClientAction.Context as List<PIDLResource>;
            Assert.AreEqual(1, pidlList.Count);
            var resource = pidlList[0];
            var qrCodeImage = resource.GetDisplayHintById("ccThreeDSQrCodeImage");

            var expectedURL = "https://mockRedirectUrl.com/?ru=https%3A%2F%2Fwww.microsoft.com%2Fen-US%2Fstore%2Frds%2Fv2%2FGeneralAddPISuccess%3FpicvRequired%3DFalse%26type%3Dvisa%26family%3Dcredit_card%26id%3D&rx=https%3A%2F%2Fwww.microsoft.com%2Fen-US%2Fstore%2Frds%2Fv2%2FGeneralAddPIFailure";

            // If xboxOOBE is passed in x-ms-flight, instead of redirecting we show bank url in iframe
            if (shouldAddOOBEHeader)
            {
                Assert.IsNotNull(qrCodeImage, "QR code image missing");
                var goToBankButton = resource.GetDisplayHintById("goToBankButton") as ButtonDisplayHint;
                Assert.IsNull(goToBankButton, "Go to bank button should not be found");

                var instructionTextGroup = resource.GetDisplayHintById("ccThreeDSWebviewInstructionGroup");
                Assert.IsNull(instructionTextGroup, "Instruction text should be removed");
            }
            else if (string.Equals(partner, "xbox", StringComparison.InvariantCultureIgnoreCase) || string.Equals(partner, "amcxbox", StringComparison.InvariantCultureIgnoreCase))
            {
                Assert.IsNotNull(qrCodeImage, "QR code image missing");
                var goToBankButton = resource.GetDisplayHintById("goToBankButton") as ButtonDisplayHint;
                Assert.IsNotNull(goToBankButton, "Go to bank button not found");
                Assert.AreEqual(
                    goToBankButton.Action.ActionType,
                    string.Equals(partner, "xbox", StringComparison.InvariantCultureIgnoreCase) ? "redirect" : "navigate");
                Assert.IsTrue(goToBankButton.Action.Context.ToString().Contains(expectedURL));
            }
            else
            {
                var goToBankButton = resource.GetDisplayHintById("cc3DSGoToBankButton") as ButtonDisplayHint;
                Assert.IsNotNull(goToBankButton, "Go to bank button not found");
                Assert.AreEqual(goToBankButton.Action.ActionType, "navigateAndMoveNext");
                var context = goToBankButton.Action.Context.ToString();
                Assert.IsTrue(context.Contains("https://mockRedirectUrl.com"), "Redirect context does not contain expected url");
            }
        }

        [TestMethod]
        [DataRow("amcweb", "Account001-3DS1-Redirect")]
        [DataRow("northstarweb", "Account001-3DS1-Redirect")]
        [DataRow("webblends_inline", "Account001-3DS1-Redirect")]
        public async Task PaymentInstrumentsEx_3DS1_FullPageRedirectPIDLS_InlineParners(string partner, string piid)
        {
            var pimsRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                details = new
                {
                    address = new
                    {
                        address_line1 = "One Microsoft Way",
                        city = "Saket",
                        region = "DE",
                        country = "IN",
                        postal_code = "111111"
                    }
                }
            };

            var sessionResponse = "{\"id\":\"ab53ebd0-0b70-41e6-b1ec-12345678\",\"session_type\":\"any\",\"data\":\"{\\\"ProviderName\\\":\\\"BillDesk\\\",\\\"Data\\\":\\\"{\\\\\\\"Operation\\\\\\\":1,\\\\\\\"InitiationRedirectUrl\\\\\\\":\\\\\\\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\\\\\\\",\\\\\\\"InitiationParameters\\\\\\\":{\\\\\\\"MD\\\\\\\":\\\\\\\"FakeMD\\\\\\\",\\\\\\\"PaReq\\\\\\\":\\\\\\\"FakePaReq\\\\\\\",\\\\\\\"TermUrl\\\\\\\":\\\\\\\"https://www.merchanturl.com/Response.jsp\\\\\\\"},\\\\\\\"AccountId\\\\\\\":\\\\\\\"bd888d21-f2a9-4f8b-92c2-12345678\\\\\\\",\\\\\\\"IpAddress\\\\\\\":null,\\\\\\\"UserAgent\\\\\\\":null,\\\\\\\"SubscriptionId\\\\\\\":\\\\\\\"12345678\\\\\\\",\\\\\\\"ApiVersion\\\\\\\":\\\\\\\"v1_2\\\\\\\",\\\\\\\"PaymentInstrumentId\\\\\\\":\\\\\\\"f7940456-15a8-4c32-b562-12345678\\\\\\\",\\\\\\\"PaymentMethodFamily\\\\\\\":\\\\\\\"credit_card\\\\\\\",\\\\\\\"PaymentMethodType\\\\\\\":\\\\\\\"visa\\\\\\\",\\\\\\\"CvvToken\\\\\\\":\\\\\\\"FakeToken\\\\\\\",\\\\\\\"PaymentId\\\\\\\":\\\\\\\"Z10009BINTWG58e3bea8-9540-4ae3-9ddf-2e32d1e06d7b\\\\\\\",\\\\\\\"TransactionId\\\\\\\":\\\\\\\"255b9d4d-c3f1-4812-9fdd-65183ef32cb3\\\\\\\",\\\\\\\"MerchantReferenceNumber\\\\\\\":\\\\\\\"FakeReference\\\\\\\",\\\\\\\"TransactionType\\\\\\\":\\\\\\\"Validate\\\\\\\",\\\\\\\"MerchantId\\\\\\\":\\\\\\\"REDMOND\\\\\\\",\\\\\\\"TrackingId\\\\\\\":\\\\\\\"59cadaf3-480b-4dbc-bb84-12345678\\\\\\\",\\\\\\\"MandateId\\\\\\\":null,\\\\\\\"ThreeDSChargeStatus\\\\\\\":\\\\\\\"Initiated\\\\\\\",\\\\\\\"ThreeDSAuthParameters\\\\\\\":null,\\\\\\\"ThreeDSChargeAmount\\\\\\\":2.0,\\\\\\\"ThreeDSChargeCurrency\\\\\\\":\\\\\\\"INR\\\\\\\",\\\\\\\"RemainingThreeDSBalance\\\\\\\":2.0,\\\\\\\"AuthenticationId\\\\\\\":\\\\\\\"40c1c487-3882-4e26-a01f-12345678\\\\\\\",\\\\\\\"AuthenticationResults\\\\\\\":null,\\\\\\\"BillDeskTransactionId\\\\\\\":null,\\\\\\\"BillDeskTransactionDate\\\\\\\":\\\\\\\"0001-01-01T00:00:00\\\\\\\",\\\\\\\"IsCommercial\\\\\\\":false,\\\\\\\"IsFullPageRedirect\\\\\\\":true}\\\"}\",\"encrypt_data\":false,\"state\":\"Initial\",\"test_context\":{\"scenarios\":\"px-service-psd2-e2e-emulator,BillDeskForceCreateToken,BillDeskUseTestEncryption,BillDeskTokenization\",\"contact\":\"blah\",\"context_props\":{}}}";

            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", piid);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXSettings.SessionService.ArrangeResponse(sessionResponse);

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country=in&language=en-US&partner={partner}&scenario=fixedCountrySelection")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(pimsRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            HttpResponseMessage result = await PXClient.SendAsync(request);

            var pidlResource = ReadSinglePidlResourceFromJson(await result.Content.ReadAsStringAsync());
            Assert.IsNotNull(pidlResource.ClientAction, "Client action missing");
            Assert.AreEqual(ClientActionType.Redirect, pidlResource.ClientAction.ActionType);

            Assert.IsNotNull(pidlResource.ClientAction.Context, "Client action context missing");

            var clientAction = pidlResource.ClientAction as ClientAction;
            Assert.IsTrue(clientAction.Context.ToString().Contains("https://mockRedirectUrl.com"), "base url is not expected");
            Assert.IsNull(clientAction.RedirectPidl, "redirectPidl should be null for inline partners");
        }

        [TestMethod]
        [DataRow("xbox", "Account001-3DS1-Redirect", false)]
        [DataRow("xbox", "Account001-3DS1-Redirect", true)]
        [DataRow("amcxbox", "Account001-3DS1-Redirect", false)]
        [DataRow("webblends", "Account001-3DS1-Redirect", false)]
        public async Task PaymentInstrumentsEx_3DS1_IFramePIDLS(string partner, string piid, bool shouldAddOOBEHeader)
        {
            var pimsRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                details = new
                {
                    address = new
                    {
                        address_line1 = "One Microsoft Way",
                        city = "Saket",
                        region = "DE",
                        country = "IN",
                        postal_code = "111111"
                    }
                }
            };

            var sessionResponse = "{\"id\":\"ab53ebd0-0b70-41e6-b1ec-12345678\",\"session_type\":\"any\",\"data\":\"{\\\"ProviderName\\\":\\\"BillDesk\\\",\\\"Data\\\":\\\"{\\\\\\\"Operation\\\\\\\":1,\\\\\\\"InitiationRedirectUrl\\\\\\\":\\\\\\\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\\\\\\\",\\\\\\\"InitiationParameters\\\\\\\":{\\\\\\\"MD\\\\\\\":\\\\\\\"FakeMD\\\\\\\",\\\\\\\"PaReq\\\\\\\":\\\\\\\"FakePaReq\\\\\\\",\\\\\\\"TermUrl\\\\\\\":\\\\\\\"https://www.merchanturl.com/Response.jsp\\\\\\\"},\\\\\\\"AccountId\\\\\\\":\\\\\\\"bd888d21-f2a9-4f8b-92c2-12345678\\\\\\\",\\\\\\\"IpAddress\\\\\\\":null,\\\\\\\"UserAgent\\\\\\\":null,\\\\\\\"SubscriptionId\\\\\\\":\\\\\\\"12345678\\\\\\\",\\\\\\\"ApiVersion\\\\\\\":\\\\\\\"v1_2\\\\\\\",\\\\\\\"PaymentInstrumentId\\\\\\\":\\\\\\\"f7940456-15a8-4c32-b562-12345678\\\\\\\",\\\\\\\"PaymentMethodFamily\\\\\\\":\\\\\\\"credit_card\\\\\\\",\\\\\\\"PaymentMethodType\\\\\\\":\\\\\\\"visa\\\\\\\",\\\\\\\"CvvToken\\\\\\\":\\\\\\\"FakeToken\\\\\\\",\\\\\\\"PaymentId\\\\\\\":\\\\\\\"Z10009BINTWG58e3bea8-9540-4ae3-9ddf-2e32d1e06d7b\\\\\\\",\\\\\\\"TransactionId\\\\\\\":\\\\\\\"255b9d4d-c3f1-4812-9fdd-65183ef32cb3\\\\\\\",\\\\\\\"MerchantReferenceNumber\\\\\\\":\\\\\\\"FakeReference\\\\\\\",\\\\\\\"TransactionType\\\\\\\":\\\\\\\"Validate\\\\\\\",\\\\\\\"MerchantId\\\\\\\":\\\\\\\"REDMOND\\\\\\\",\\\\\\\"TrackingId\\\\\\\":\\\\\\\"59cadaf3-480b-4dbc-bb84-12345678\\\\\\\",\\\\\\\"MandateId\\\\\\\":null,\\\\\\\"ThreeDSChargeStatus\\\\\\\":\\\\\\\"Initiated\\\\\\\",\\\\\\\"ThreeDSAuthParameters\\\\\\\":null,\\\\\\\"ThreeDSChargeAmount\\\\\\\":2.0,\\\\\\\"ThreeDSChargeCurrency\\\\\\\":\\\\\\\"INR\\\\\\\",\\\\\\\"RemainingThreeDSBalance\\\\\\\":2.0,\\\\\\\"AuthenticationId\\\\\\\":\\\\\\\"40c1c487-3882-4e26-a01f-12345678\\\\\\\",\\\\\\\"AuthenticationResults\\\\\\\":null,\\\\\\\"BillDeskTransactionId\\\\\\\":null,\\\\\\\"BillDeskTransactionDate\\\\\\\":\\\\\\\"0001-01-01T00:00:00\\\\\\\",\\\\\\\"IsCommercial\\\\\\\":false,\\\\\\\"IsFullPageRedirect\\\\\\\":false}\\\"}\",\"encrypt_data\":false,\"state\":\"Initial\",\"test_context\":{\"scenarios\":\"px-service-psd2-e2e-emulator,BillDeskForceCreateToken,BillDeskUseTestEncryption,BillDeskTokenization\",\"contact\":\"blah\",\"context_props\":{}}}";

            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", piid);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXSettings.SessionService.ArrangeResponse(sessionResponse);

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country=in&language=en-US&partner={partner}&scenario=fixedCountrySelection")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(pimsRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            if (shouldAddOOBEHeader)
            {
                request.Headers.Add("x-ms-flight", "xboxOOBE");
            }

            HttpResponseMessage result = await PXClient.SendAsync(request);

            var pidlResource = ReadSinglePidlResourceFromJson(await result.Content.ReadAsStringAsync());
            Assert.IsNotNull(pidlResource.ClientAction, "Client action missing");
            Assert.AreEqual(ClientActionType.Pidl, pidlResource.ClientAction.ActionType);
            Assert.IsNotNull(pidlResource.ClientAction.Context, "Client action context missing");

            var pidlList = pidlResource.ClientAction.Context as List<PIDLResource>;
            Assert.AreEqual(1, pidlList.Count);
            var resource = pidlList[0];

            var qrCodeImage = resource.GetDisplayHintById("ccThreeDSQrCodeImage");
            var goToBankButton = resource.GetDisplayHintById("goToBankButton");

            // If xboxOOBE is passed in x-ms-flight, instead of redirecting we show bank url in iframe
            if (shouldAddOOBEHeader)
            {
                Assert.IsNotNull(qrCodeImage, "QR code image missing");
                Assert.IsNull(goToBankButton, "Go to bank button should not be found");
                var instructionTextGroup = resource.GetDisplayHintById("ccThreeDSWebviewInstructionGroup");
                Assert.IsNull(instructionTextGroup, "Instruction text should be removed");
            }
            else if (string.Equals(partner, "xbox", StringComparison.InvariantCultureIgnoreCase) || string.Equals(partner, "amcxbox", StringComparison.InvariantCultureIgnoreCase))
            {
                Assert.IsNotNull(qrCodeImage, "QR code image missing");
                Assert.IsNotNull(goToBankButton, "Go to bank button not found");
                Assert.AreEqual("moveNext", goToBankButton.Action.ActionType);
                var iFrame = resource.GetDisplayHintById("globalPIQrCodeIframe") as IFrameDisplayHint;
                Assert.IsNotNull(iFrame, "iFrame is missing");
                var expectedURL = "https://mockRedirectUrl.com/?ru=https%3A%2F%2Fwww.microsoft.com%2Fen-US%2Fstore%2Frds%2Fv2%2FGeneralAddPISuccess%3FpicvRequired%3DFalse%26type%3Dvisa%26family%3Dcredit_card%26id%3D&rx=https%3A%2F%2Fwww.microsoft.com%2Fen-US%2Fstore%2Frds%2Fv2%2FGeneralAddPIFailure";
                Assert.AreEqual(expectedURL, iFrame.SourceUrl, "iFrame source url not as expected");
            }
            else
            {
                var iFrame = resource.GetDisplayHintById("threeDSIframe") as IFrameDisplayHint;
                var expectedContent = "<html><body onload=\"window.location.href = 'https://mockRedirectUrl.com/?ru=https%3A%2F%2Fpifd.cp.microsoft-int.com%2FV6.0%2FpaymentInstrumentsEx%2FAccount001-3DS1-Redirect%2Fresume%3Fcountry%3Din%26language%3Den-US%26partner%3Dwebblends%26isSuccessful%3DTrue%26sessionQueryUrl%3Dsessions%2Fabcd-12345&rx=https%3A%2F%2Fpifd.cp.microsoft-int.com%2FV6.0%2FpaymentInstrumentsEx%2FAccount001-3DS1-Redirect%2Fresume%3Fcountry%3Din%26language%3Den-US%26partner%3Dwebblends%26isSuccessful%3DFalse%26sessionQueryUrl%3Dsessions%2Fabcd-12345'\"></body></html>";
                Assert.IsNotNull(iFrame, "iFrame is missing");
                Assert.AreEqual(expectedContent, iFrame.DisplayContent, "iFrame source url not as expected");
                Assert.IsNotNull(iFrame.DisplayTags, "Accessibility display tags are missing");
                Assert.AreEqual(iFrame.DisplayTags["accessibilityName"], "The bank authentication dialog");
            }
        }

        [TestMethod]
        [DataRow("amcweb", "Account001-3DS1-Redirect", "inline")]
        [DataRow("northstarweb", "Account001-3DS1-Redirect", "inline")]
        [DataRow("xbox", "Account001-3DS1", "QRCode")]
        [DataRow("amcxbox", "Account001-3DS1", "QRCode")]
        [DataRow("webblends", "Account001-3DS1-Redirect", "iFrame")]
        [DataRow("webblends", "Account001-3DS1-Redirect", "fullPage")]
        [DataRow("teamsappstorefront", "Account001-3DS1-Redirect", "inline")]
        [DataRow("officesmb", "Account001-3DS1-Redirect", "inline")]
        [DataRow("officesmb", "Account001-3DS1-Redirect", "inline", true)]
        [DataRow("officesmb", "Account001-3DS1-Redirect", "fullPage", true)]
        [DataRow("officesmb", "Account001-3DS1-Redirect", "fullPage", true, true)]
        public async Task PaymentInstrumentsEx_3DS1_AllRedirectionPartners_UsingPartnerSettingsService(string partner, string piid, string redirectionPattern, bool useDefaultTemplate = false, bool enableConditionalFields = false)
        {
            var pimsRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                details = new
                {
                    address = new
                    {
                        address_line1 = "One Microsoft Way",
                        city = "Saket",
                        region = "DE",
                        country = "IN",
                        postal_code = "111111"
                    }
                }
            };

            var pssResponse = new
            {
                add = new
                {
                    template = useDefaultTemplate ? "defaultTemplate" : partner,
                    redirectionPattern
                }
            };

            string sessionResponse;
            if (string.Equals(redirectionPattern, "iFrame"))
            {
                sessionResponse = "{\"id\":\"ab53ebd0-0b70-41e6-b1ec-12345678\",\"session_type\":\"any\",\"data\":\"{\\\"ProviderName\\\":\\\"BillDesk\\\",\\\"Data\\\":\\\"{\\\\\\\"Operation\\\\\\\":1,\\\\\\\"InitiationRedirectUrl\\\\\\\":\\\\\\\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\\\\\\\",\\\\\\\"InitiationParameters\\\\\\\":{\\\\\\\"MD\\\\\\\":\\\\\\\"FakeMD\\\\\\\",\\\\\\\"PaReq\\\\\\\":\\\\\\\"FakePaReq\\\\\\\",\\\\\\\"TermUrl\\\\\\\":\\\\\\\"https://www.merchanturl.com/Response.jsp\\\\\\\"},\\\\\\\"AccountId\\\\\\\":\\\\\\\"bd888d21-f2a9-4f8b-92c2-12345678\\\\\\\",\\\\\\\"IpAddress\\\\\\\":null,\\\\\\\"UserAgent\\\\\\\":null,\\\\\\\"SubscriptionId\\\\\\\":\\\\\\\"12345678\\\\\\\",\\\\\\\"ApiVersion\\\\\\\":\\\\\\\"v1_2\\\\\\\",\\\\\\\"PaymentInstrumentId\\\\\\\":\\\\\\\"f7940456-15a8-4c32-b562-12345678\\\\\\\",\\\\\\\"PaymentMethodFamily\\\\\\\":\\\\\\\"credit_card\\\\\\\",\\\\\\\"PaymentMethodType\\\\\\\":\\\\\\\"visa\\\\\\\",\\\\\\\"CvvToken\\\\\\\":\\\\\\\"FakeToken\\\\\\\",\\\\\\\"PaymentId\\\\\\\":\\\\\\\"Z10009BINTWG58e3bea8-9540-4ae3-9ddf-2e32d1e06d7b\\\\\\\",\\\\\\\"TransactionId\\\\\\\":\\\\\\\"255b9d4d-c3f1-4812-9fdd-65183ef32cb3\\\\\\\",\\\\\\\"MerchantReferenceNumber\\\\\\\":\\\\\\\"FakeReference\\\\\\\",\\\\\\\"TransactionType\\\\\\\":\\\\\\\"Validate\\\\\\\",\\\\\\\"MerchantId\\\\\\\":\\\\\\\"REDMOND\\\\\\\",\\\\\\\"TrackingId\\\\\\\":\\\\\\\"59cadaf3-480b-4dbc-bb84-12345678\\\\\\\",\\\\\\\"MandateId\\\\\\\":null,\\\\\\\"ThreeDSChargeStatus\\\\\\\":\\\\\\\"Initiated\\\\\\\",\\\\\\\"ThreeDSAuthParameters\\\\\\\":null,\\\\\\\"ThreeDSChargeAmount\\\\\\\":2.0,\\\\\\\"ThreeDSChargeCurrency\\\\\\\":\\\\\\\"INR\\\\\\\",\\\\\\\"RemainingThreeDSBalance\\\\\\\":2.0,\\\\\\\"AuthenticationId\\\\\\\":\\\\\\\"40c1c487-3882-4e26-a01f-12345678\\\\\\\",\\\\\\\"AuthenticationResults\\\\\\\":null,\\\\\\\"BillDeskTransactionId\\\\\\\":null,\\\\\\\"BillDeskTransactionDate\\\\\\\":\\\\\\\"0001-01-01T00:00:00\\\\\\\",\\\\\\\"IsCommercial\\\\\\\":false,\\\\\\\"IsFullPageRedirect\\\\\\\":false}\\\"}\",\"encrypt_data\":false,\"state\":\"Initial\",\"test_context\":{\"scenarios\":\"px-service-psd2-e2e-emulator,BillDeskForceCreateToken,BillDeskUseTestEncryption,BillDeskTokenization\",\"contact\":\"blah\",\"context_props\":{}}}";
            }
            else
            {
                sessionResponse = "{\"id\":\"ab53ebd0-0b70-41e6-b1ec-12345678\",\"session_type\":\"any\",\"data\":\"{\\\"ProviderName\\\":\\\"BillDesk\\\",\\\"Data\\\":\\\"{\\\\\\\"Operation\\\\\\\":1,\\\\\\\"InitiationRedirectUrl\\\\\\\":\\\\\\\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\\\\\\\",\\\\\\\"InitiationParameters\\\\\\\":{\\\\\\\"MD\\\\\\\":\\\\\\\"FakeMD\\\\\\\",\\\\\\\"PaReq\\\\\\\":\\\\\\\"FakePaReq\\\\\\\",\\\\\\\"TermUrl\\\\\\\":\\\\\\\"https://www.merchanturl.com/Response.jsp\\\\\\\"},\\\\\\\"AccountId\\\\\\\":\\\\\\\"bd888d21-f2a9-4f8b-92c2-12345678\\\\\\\",\\\\\\\"IpAddress\\\\\\\":null,\\\\\\\"UserAgent\\\\\\\":null,\\\\\\\"SubscriptionId\\\\\\\":\\\\\\\"12345678\\\\\\\",\\\\\\\"ApiVersion\\\\\\\":\\\\\\\"v1_2\\\\\\\",\\\\\\\"PaymentInstrumentId\\\\\\\":\\\\\\\"f7940456-15a8-4c32-b562-12345678\\\\\\\",\\\\\\\"PaymentMethodFamily\\\\\\\":\\\\\\\"credit_card\\\\\\\",\\\\\\\"PaymentMethodType\\\\\\\":\\\\\\\"visa\\\\\\\",\\\\\\\"CvvToken\\\\\\\":\\\\\\\"FakeToken\\\\\\\",\\\\\\\"PaymentId\\\\\\\":\\\\\\\"Z10009BINTWG58e3bea8-9540-4ae3-9ddf-2e32d1e06d7b\\\\\\\",\\\\\\\"TransactionId\\\\\\\":\\\\\\\"255b9d4d-c3f1-4812-9fdd-65183ef32cb3\\\\\\\",\\\\\\\"MerchantReferenceNumber\\\\\\\":\\\\\\\"FakeReference\\\\\\\",\\\\\\\"TransactionType\\\\\\\":\\\\\\\"Validate\\\\\\\",\\\\\\\"MerchantId\\\\\\\":\\\\\\\"REDMOND\\\\\\\",\\\\\\\"TrackingId\\\\\\\":\\\\\\\"59cadaf3-480b-4dbc-bb84-12345678\\\\\\\",\\\\\\\"MandateId\\\\\\\":null,\\\\\\\"ThreeDSChargeStatus\\\\\\\":\\\\\\\"Initiated\\\\\\\",\\\\\\\"ThreeDSAuthParameters\\\\\\\":null,\\\\\\\"ThreeDSChargeAmount\\\\\\\":2.0,\\\\\\\"ThreeDSChargeCurrency\\\\\\\":\\\\\\\"INR\\\\\\\",\\\\\\\"RemainingThreeDSBalance\\\\\\\":2.0,\\\\\\\"AuthenticationId\\\\\\\":\\\\\\\"40c1c487-3882-4e26-a01f-12345678\\\\\\\",\\\\\\\"AuthenticationResults\\\\\\\":null,\\\\\\\"BillDeskTransactionId\\\\\\\":null,\\\\\\\"BillDeskTransactionDate\\\\\\\":\\\\\\\"0001-01-01T00:00:00\\\\\\\",\\\\\\\"IsCommercial\\\\\\\":false,\\\\\\\"IsFullPageRedirect\\\\\\\":true}\\\"}\",\"encrypt_data\":false,\"state\":\"Initial\",\"test_context\":{\"scenarios\":\"px-service-psd2-e2e-emulator,BillDeskForceCreateToken,BillDeskUseTestEncryption,BillDeskTokenization\",\"contact\":\"blah\",\"context_props\":{}}}";
            }

            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", piid);
            string partnerSettingsServiceResponse = JsonConvert.SerializeObject(pssResponse);
            if (useDefaultTemplate
                && enableConditionalFields
                && string.Equals(redirectionPattern, "fullPage", StringComparison.OrdinalIgnoreCase))
            {
                partnerSettingsServiceResponse = "{\"add\":{\"template\":\"defaultTemplate\",\"redirectionPattern\":\"fullPage\",\"features\":{\"enableConditionalFieldsForBillingAddress\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]}}}}";
            }

            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));
            PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingsServiceResponse);
            PXSettings.SessionService.ArrangeResponse(sessionResponse);

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country=in&language=en-US&partner={partner}&scenario=fixedCountrySelection")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(pimsRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            request.Headers.Add("x-ms-flight", "PXUsePartnerSettingsService,PXDisablePSSCache");

            HttpResponseMessage result = await PXClient.SendAsync(request);
            var pidlResource = ReadSinglePidlResourceFromJson(await result.Content.ReadAsStringAsync());
            Assert.IsNotNull(pidlResource.ClientAction, "Client action missing");
            Assert.IsNotNull(pidlResource.ClientAction.Context, "Client action context missing");

            if (string.Equals(redirectionPattern, "inline"))
            {
                Assert.AreEqual(ClientActionType.Redirect, pidlResource.ClientAction.ActionType);
                var clientAction = pidlResource.ClientAction as ClientAction;
                Assert.IsTrue(clientAction.Context.ToString().Contains("https://mockRedirectUrl.com"), "base url is not expected");
                Assert.IsNull(clientAction.RedirectPidl, "redirectPidl should be null for inline partners");
            }
            else if (string.Equals(redirectionPattern, "fullPage"))
            {
                Assert.AreEqual(ClientActionType.Pidl, pidlResource.ClientAction.ActionType);
                var pidlList = pidlResource.ClientAction.Context as List<PIDLResource>;
                Assert.AreEqual(1, pidlList.Count);
                var resource = pidlList[0];

                var goToBankButton = resource.GetDisplayHintById("cc3DSGoToBankButton") as ButtonDisplayHint;
                Assert.IsNotNull(goToBankButton, "Go to bank button not found");
                Assert.AreEqual(goToBankButton.Action.ActionType, "navigateAndMoveNext");
                var context = goToBankButton.Action.Context.ToString();
                Assert.IsTrue(context.Contains("https://mockRedirectUrl.com"), "Redirect context does not contain expected url");

                if (useDefaultTemplate)
                {
                    Assert.AreEqual(2, resource.DisplayPages.Count, "Display pages count is not as expected");

                    var cancelButton = resource.GetDisplayHintById("cc3DSCancelButton") as ButtonDisplayHint;
                    Assert.IsNotNull(cancelButton, "Cancel button not found");
                    var tryAgainButton = resource.GetDisplayHintById("cc3DSTryAgainButton") as ButtonDisplayHint;
                    Assert.IsNotNull(tryAgainButton, "tryAgain button not found");
                    Assert.AreEqual(tryAgainButton.Action.ActionType, "redirect");
                    var buttonContext = goToBankButton.Action.Context.ToString();
                    Assert.IsTrue(buttonContext.Contains("https://mockRedirectUrl.com"), "Redirect context does not contain expected url");

                    var yesButton = resource.GetDisplayHintById("cc3DSYesButton") as ButtonDisplayHint;
                    Assert.IsNotNull(yesButton, "Yes button not found");

                    var regulationText = resource.GetDisplayHintById("regulationText") as TextDisplayHint;
                    Assert.IsNotNull(regulationText, "regulationText is missing");
                    Assert.AreEqual(regulationText.DisplayContent, "Due to Reserve Bank of India regulations, we will need to verify your card.", "regulationText content is not as expected");

                    var redirectionInstructionText = resource.GetDisplayHintById("cc3DSRedirectInstructionText") as TextDisplayHint;
                    Assert.IsNotNull(redirectionInstructionText, "redirectionInstructionText is missing");
                    Assert.AreEqual(redirectionInstructionText.DisplayContent, "You will be redirected to the bank’s website and a new browser window will open.", "redirectionInstructionText content is not as expected");

                    var statusCheckHeader = resource.GetDisplayHintById("cc3DSStatusCheckHeader") as HeadingDisplayHint;
                    Assert.IsNotNull(statusCheckHeader, "statusCheckHeader is missing");
                    Assert.AreEqual(statusCheckHeader.DisplayContent, "Was your card verified?", "cc3DSStatusCheckHeader content is not as expected");

                    var statusCheckText = resource.GetDisplayHintById("cc3DSStatusCheckText") as TextDisplayHint;
                    Assert.IsNotNull(statusCheckText, "statusCheckText is missing");
                    Assert.AreEqual(statusCheckText.DisplayContent, "You can complete your purchase if you have successfully verified your card.", "cc3DSStatusCheckText content is not as expected");
                }
            }
            else if (string.Equals(redirectionPattern, "iFrame"))
            {
                Assert.AreEqual(ClientActionType.Pidl, pidlResource.ClientAction.ActionType);
                var pidlList = pidlResource.ClientAction.Context as List<PIDLResource>;
                Assert.AreEqual(1, pidlList.Count);
                var resource = pidlList[0];

                var iFrame = resource.GetDisplayHintById("threeDSIframe") as IFrameDisplayHint;
                var expectedContent = "<html><body onload=\"window.location.href = 'https://mockRedirectUrl.com/?ru=https%3A%2F%2Fpifd.cp.microsoft-int.com%2FV6.0%2FpaymentInstrumentsEx%2FAccount001-3DS1-Redirect%2Fresume%3Fcountry%3Din%26language%3Den-US%26partner%3Dwebblends%26isSuccessful%3DTrue%26sessionQueryUrl%3Dsessions%2Fabcd-12345&rx=https%3A%2F%2Fpifd.cp.microsoft-int.com%2FV6.0%2FpaymentInstrumentsEx%2FAccount001-3DS1-Redirect%2Fresume%3Fcountry%3Din%26language%3Den-US%26partner%3Dwebblends%26isSuccessful%3DFalse%26sessionQueryUrl%3Dsessions%2Fabcd-12345'\"></body></html>";
                Assert.IsNotNull(iFrame, "iFrame is missing");
                Assert.AreEqual(expectedContent, iFrame.DisplayContent, "iFrame source url not as expected");
                Assert.IsNotNull(iFrame.DisplayTags, "Accessibility display tags are missing");
                Assert.AreEqual(iFrame.DisplayTags["accessibilityName"], "The bank authentication dialog");
            }
            else if (string.Equals(redirectionPattern, "QRCode"))
            {
                Assert.AreEqual(ClientActionType.Pidl, pidlResource.ClientAction.ActionType);
                var pidlList = pidlResource.ClientAction.Context as List<PIDLResource>;
                Assert.AreEqual(1, pidlList.Count);
                var resource = pidlList[0];
                var qrCodeImage = resource.GetDisplayHintById("ccThreeDSQrCodeImage");
                Assert.IsNotNull(qrCodeImage);
            }
        }

        /// <summary>
        /// The test is to verify Add PI throws error if "The card is not enabled for 3ds/otp authentication in India."
        /// </summary>
        [TestMethod]
        [DataRow("InvalidIssuerResponseWithTRPAU0009", "PXDisplay3dsNotEnabledErrorInline")]
        [DataRow("InvalidIssuerResponseWithTRPAU0008", "PXDisplay3dsNotEnabledErrorInline")]
        [DataRow("InvalidIssuerResponseWithTRPAU0009")]
        [DataRow("InvalidIssuerResponseWithTRPAU0008")]
        public async Task TestAddPI_ThrowsInvalidIssuerErrorResponse(string expectedErrorCode, string flightName = null)
        {
            var pimsRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                paymentMethodOperation = "add",
                paymentMethodCountry = "in",

                context = "purchase",
                sessionId = Guid.NewGuid().ToString(),
                details = new
                {
                    dataType = "credit_card_visa_details",
                    dataOperation = "add",
                    dataCountry = "in",
                    accountHolderName = "Test",
                    accountToken = "dummyAccountTokenValue",
                    expiryMonth = "3",
                    expiryYear = "2023",
                    cvvToken = "dummyCvvTokenValue",
                    address = new
                    {
                        addressType = "billing",
                        addressOperation = "add",
                        addressCountry = "us",
                        address_line1 = "1 Microsoft Way",
                        city = "redmond",
                        region = "wa",
                        postal_code = "98052",
                        country = "us"
                    },
                    tokenizationConsent = "true"
                }
            };

            ServiceErrorResponse response = new ServiceErrorResponse()
            {
                ErrorCode = expectedErrorCode,
                Message = "The card is not enabled for 3ds/otp authentication in India.",
            };

            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(response), HttpStatusCode.BadRequest);

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country=in&language=en-US&partner=Azure&classicProduct=azureClassic&billableAccountId={CommerceAccountDataAccessor.BillingAccountId.AzureBusinessAccount}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(pimsRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-clientcontext-encoding", "base64"
                },
                {
                    "x-ms-aadinfo", "tid=Y2ViMTM2ZmYtZGEzNC00NDkyLTkyNGQtNWY2MTIxMTFmNjc5,oid=ZjAyYmI0N2YtNzI4Ny00OTdlLWI2NTEtYTk0ZmRiM2RmM2Fk,altSecId=MTA1NTUxOTQxMTM0MDUwOQ=="
                },
            };

            if (!string.IsNullOrEmpty(flightName))
            {
                PXFlightHandler.AddToEnabledFlights(flightName);
            }

            headers?.ToList().ForEach(header => request.Headers.Add(header.Key, header.Value));
            HttpResponseMessage result = await PXClient.SendAsync(request);

            string resultContent = await result.Content.ReadAsStringAsync();
            JObject jsonObj = JObject.Parse(resultContent);
            Assert.AreEqual(result.StatusCode, HttpStatusCode.BadRequest);
            Assert.AreEqual(jsonObj.SelectToken("ErrorCode"), expectedErrorCode);
            Assert.AreEqual(jsonObj.SelectToken("Message"), "The card is not enabled for 3ds/otp authentication in India.");
            if (!string.IsNullOrEmpty(flightName))
            {
                Assert.AreEqual(jsonObj.SelectToken("Details")[0].SelectToken("Target"), "accountToken");
                Assert.AreEqual(jsonObj.SelectToken("Details")[0].SelectToken("Message"), "The card is not enabled for 3ds/otp authentication in India.");
            }
        }

        /// <summary>
        /// The test is to verify Add PI throws generic error if UPI Add gives a 4xx request"
        /// </summary>
        [TestMethod]
        [DataRow("UPIGenericError", "PxEnableUpi")]
        [DataRow("UPIGenericError")]
        public async Task TestAddPI_ThrowsGenericMapUpiAddError(string expectedErrorCode, string flightName = null)
        {
            var pimsRequestBody = new
            {
                paymentMethodFamily = "real_time_payments",
                paymentMethodType = "upi",
                paymentMethodOperation = "add",
                paymentMethodCountry = "in",
                vpa = "test_vpa@okbank",
                context = "purchase",
                sessionId = Guid.NewGuid().ToString()
            };

            ServiceErrorResponse response = new ServiceErrorResponse()
            {
                ErrorCode = expectedErrorCode,
                Message = "UPI Add error",
            };

            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(response), HttpStatusCode.BadRequest);

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account004/paymentInstrumentsEx?country=in&language=en-US&partner=Webblends")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(pimsRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-clientcontext-encoding", "base64"
                },
                {
                    "x-ms-aadinfo", "tid=Y2ViMTM2ZmYtZGEzNC00NDkyLTkyNGQtNWY2MTIxMTFmNjc5,oid=ZjAyYmI0N2YtNzI4Ny00OTdlLWI2NTEtYTk0ZmRiM2RmM2Fk,altSecId=MTA1NTUxOTQxMTM0MDUwOQ=="
                },
            };

            if (!string.IsNullOrEmpty(flightName))
            {
                PXFlightHandler.AddToEnabledFlights(flightName);
            }

            headers?.ToList().ForEach(header => request.Headers.Add(header.Key, header.Value));
            HttpResponseMessage result = await PXClient.SendAsync(request);

            string resultContent = await result.Content.ReadAsStringAsync();
            JObject jsonObj = JObject.Parse(resultContent);
            Assert.AreEqual(result.StatusCode, HttpStatusCode.BadRequest);
            Assert.AreEqual(jsonObj.SelectToken("ErrorCode"), expectedErrorCode);
            Assert.AreEqual(jsonObj.SelectToken("Message"), "Try that again. Something happened on our end. Waiting a bit can help.");
        }

        /// <summary>
        /// The test is to verify Add PI throws invalid UPI account error if UPI Add gives a 4xx request"
        /// </summary>
        [TestMethod]
        [DataRow("AccountNotFound", "PxEnableUpi")]
        [DataRow("AccountNotFound")]
        public async Task TestAddPI_ThrowsInvalidUPIAccountMapUpiAddError(string expectedErrorCode, string flightName = null)
        {
            var pimsRequestBody = new
            {
                paymentMethodFamily = "real_time_payments",
                paymentMethodType = "upi",
                paymentMethodOperation = "add",
                paymentMethodCountry = "in",
                vpa = "test_vpa@okbank",
                context = "purchase",
                sessionId = Guid.NewGuid().ToString()
            };

            ServiceErrorResponse response = new ServiceErrorResponse()
            {
                ErrorCode = expectedErrorCode,
            };

            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(response), HttpStatusCode.BadRequest);

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account004/paymentInstrumentsEx?country=in&language=en-US&partner=Webblends")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(pimsRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-clientcontext-encoding", "base64"
                },
                {
                    "x-ms-aadinfo", "tid=Y2ViMTM2ZmYtZGEzNC00NDkyLTkyNGQtNWY2MTIxMTFmNjc5,oid=ZjAyYmI0N2YtNzI4Ny00OTdlLWI2NTEtYTk0ZmRiM2RmM2Fk,altSecId=MTA1NTUxOTQxMTM0MDUwOQ=="
                },
            };

            if (!string.IsNullOrEmpty(flightName))
            {
                PXFlightHandler.AddToEnabledFlights(flightName);
            }

            headers?.ToList().ForEach(header => request.Headers.Add(header.Key, header.Value));
            HttpResponseMessage result = await PXClient.SendAsync(request);

            string resultContent = await result.Content.ReadAsStringAsync();
            JObject jsonObj = JObject.Parse(resultContent);
            Assert.AreEqual(result.StatusCode, HttpStatusCode.BadRequest);
            Assert.AreEqual(jsonObj.SelectToken("ErrorCode"), expectedErrorCode);
        }

        [TestMethod]
        public async Task PaymentInstrumentsEx_Upi_AddPI_Success()
        {
            var pimsRequestBody = new
            {
                paymentMethodFamily = "real_time_payments",
                paymentMethodType = "upi",
                paymentMethodOperation = "add",
                paymentMethodCountry = "in",
                vpa = "test_vpa@okbank",
                context = "purchase",
                sessionId = Guid.NewGuid().ToString()
            };

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account004", "Account004-Pi001-IndiaUPI");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI), HttpStatusCode.OK);

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account004/paymentInstrumentsEx?country=in&language=en-US&partner=Webblends")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(pimsRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-clientcontext-encoding", "base64"
                },
                {
                    "x-ms-aadinfo", "tid=Y2ViMTM2ZmYtZGEzNC00NDkyLTkyNGQtNWY2MTIxMTFmNjc5,oid=ZjAyYmI0N2YtNzI4Ny00OTdlLWI2NTEtYTk0ZmRiM2RmM2Fk,altSecId=MTA1NTUxOTQxMTM0MDUwOQ=="
                },
            };

            PXFlightHandler.AddToEnabledFlights("PxEnableUpi");

            headers?.ToList().ForEach(header => request.Headers.Add(header.Key, header.Value));
            HttpResponseMessage result = await PXClient.SendAsync(request);

            string responseContent = await result.Content.ReadAsStringAsync();
            PaymentInstrument pi = JsonConvert.DeserializeObject<PaymentInstrument>(responseContent);
            PaymentMethod pm = pi.PaymentMethod;

            Assert.IsNotNull(pi, "Expected payment instrument to not be null");
            Assert.AreEqual(PaymentInstrumentStatus.Active, pi.Status, "Payment Instrument status expected to be 'Active'");
            Assert.IsNotNull(pm, "Expected payment method to not be null");
            Assert.AreEqual("real_time_payments", pm.PaymentMethodFamily, "PaymentMethodFamily expected to be real_time_payments");
            Assert.AreEqual("upi", pm.PaymentMethodType, "PaymentMethodType expected to be upi");
            Assert.IsNotNull(pi.PaymentInstrumentDetails);
            Assert.IsNotNull(pi.PaymentInstrumentDetails.Vpa);
            Assert.IsTrue(pi.PaymentInstrumentDetails.Vpa == expectedPI.PaymentInstrumentDetails.Vpa);
        }

        [TestMethod]
        public async Task PaymentInstrumentsEx_Upi_Commercial_AddPI_Success()
        {
            var pimsRequestBody = new
            {
                paymentMethodFamily = "real_time_payments",
                paymentMethodType = "upi_commercial",
                paymentMethodOperation = "add",
                paymentMethodCountry = "in",
                vpa = "9999999999@apl",
                context = "purchase",
                sessionId = Guid.NewGuid().ToString()
            };

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account008", "Account008-Pi001-IndiaUPI");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI), HttpStatusCode.OK);

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account008/paymentInstrumentsEx?country=in&language=en-US&partner=defaulttemplate")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(pimsRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-clientcontext-encoding", "base64"
                },
                {
                    "x-ms-aadinfo", "tid=Y2ViMTM2ZmYtZGEzNC00NDkyLTkyNGQtNWY2MTIxMTFmNjc5,oid=ZjAyYmI0N2YtNzI4Ny00OTdlLWI2NTEtYTk0ZmRiM2RmM2Fk,altSecId=MTA1NTUxOTQxMTM0MDUwOQ=="
                },
            };

            PXFlightHandler.AddToEnabledFlights("PxCommercialEnableUpi");

            headers?.ToList().ForEach(header => request.Headers.Add(header.Key, header.Value));
            HttpResponseMessage result = await PXClient.SendAsync(request);

            string responseContent = await result.Content.ReadAsStringAsync();
            PaymentInstrument pi = JsonConvert.DeserializeObject<PaymentInstrument>(responseContent);
            PaymentMethod pm = pi.PaymentMethod;

            Assert.IsNotNull(pi, "Expected payment instrument to not be null");
            Assert.AreEqual(PaymentInstrumentStatus.Active, pi.Status, "Payment Instrument status expected to be 'Active'");
            Assert.IsNotNull(pm, "Expected payment method to not be null");
            Assert.AreEqual("real_time_payments", pm.PaymentMethodFamily, "PaymentMethodFamily expected to be real_time_payments");
            Assert.AreEqual("upi_commercial", pm.PaymentMethodType, "PaymentMethodType expected to be upi_commercial");
            Assert.IsNotNull(pi.PaymentInstrumentDetails);
            Assert.IsNotNull(pi.PaymentInstrumentDetails.Vpa);
            Assert.IsTrue(pi.PaymentInstrumentDetails.Vpa == expectedPI.PaymentInstrumentDetails.Vpa);
        }

        [DataRow("webblends", "in", "Account004", "upi", "PxEnableUpi")]
        [DataRow("azure", "in", "Account008", "upi_commercial", "PxCommercialEnableUpi")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_Upi_GetModernPI_Success(string partner, string country, string accountid, string type, string flight)
        {
            object requestBody = new
            {
                paymentMethodFamily = "real_time_payments",
                paymentMethodType = type,
                details = new
                {
                    address = new
                    {
                        address_line1 = "One Microsoft Way",
                        city = "Redmond",
                        region = "WA",
                        country = "US",
                        postal_code = "11111"
                    },
                },
                riskData = new { }
            };

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument(accountid, $"{accountid}-Pi001-IndiaUPI");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXFlightHandler.AddToEnabledFlights(flight);
            HttpResponseMessage result = await PXClient.GetAsync($"/v7.0/{accountid}/paymentInstrumentsEx/{accountid}-Pi001-IndiaUPI?country={country}&language=en-US&partner={partner}");

            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK, "Expected 200 OK response");

            string responseContent = await result.Content.ReadAsStringAsync();
            PaymentInstrument pi = JsonConvert.DeserializeObject<PaymentInstrument>(responseContent);
            PaymentMethod pm = pi.PaymentMethod;

            Assert.IsNotNull(pi, "Expected payment instrument to not be null");
            Assert.AreEqual(PaymentInstrumentStatus.Active, pi.Status, "Payment Instrument status expected to be 'Active'");
            Assert.IsNotNull(pm, "Expected payment method to not be null");
            Assert.AreEqual("real_time_payments", pm.PaymentMethodFamily, "PaymentMethodFamily expected to be real_time_payments");
            Assert.AreEqual(type, pm.PaymentMethodType, "PaymentMethodType expected to be upi");
            Assert.IsNotNull(pi.PaymentInstrumentDetails);
            Assert.IsNotNull(pi.PaymentInstrumentDetails.Vpa);
            Assert.IsTrue(pi.PaymentInstrumentDetails.Vpa == expectedPI.PaymentInstrumentDetails.Vpa);
        }

        [DataRow("officesmb", "kr", "Account001-Pi013-KakaopayRedirect")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_KakaoPay_GetModernPI_Success(string partner, string country, string piid)
        {
            // Arrange
            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", $"{piid}");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            if (string.Equals(partner, "officesmb"))
            {
                var partnerSettingResponse = PartnerSettingsServiceMockResponseProvider.GetPSSMockResponseById("Account001-PI001-fullPageRedirectionDefaultTemplate");
                PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
            }

            // Act
            HttpResponseMessage result = await PXClient.GetAsync($"/v7.0/Account001/paymentInstrumentsEx/{piid}?country={country}&language=en-US&partner={partner}");

            // Assert
            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK, "Expected 200 OK response");
            string piJson = await result.Content.ReadAsStringAsync();
            PIDLResource pidl = ReadSinglePidlResourceFromJson(piJson);
            List<PIDLResource> pidlList = pidl.ClientAction.Context as List<PIDLResource>;
            Assert.IsNotNull(pidlList);
            Assert.IsNotNull(pidlList[0].DisplayPages);
            Assert.AreEqual(pidlList[0].DisplayPages[0].HintId, "genericPollingPage");
            Assert.IsTrue(pidlList[0].DisplayPages[0].Action.Context.ToString().Contains($"partner={partner}"));            
        }

        [DataRow("webblends", "in", "Account004", "rupay", "PXEnableRupayForIN")]
        [DataRow("azure", "in", "Account008", "rupay", "PXEnableRupayForIN")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_Rupay_GetModernPI_Success(string partner, string country, string accountid, string type, string flight)
        {
            object requestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = type,
                details = new
                {
                    address = new
                    {
                        address_line1 = "One Microsoft Way",
                        city = "Mumbai",
                        region = "MH",
                        country = "IN",
                        postal_code = "11111"
                    },
                },
                riskData = new { }
            };

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument(accountid, $"{accountid}-Pi001-Rupay");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXFlightHandler.AddToEnabledFlights(flight);
            HttpResponseMessage result = await PXClient.GetAsync($"/v7.0/{accountid}/paymentInstrumentsEx/{accountid}-Pi001-Rupay?country={country}&language=en-US&partner={partner}");

            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK, "Expected 200 OK response");

            string responseContent = await result.Content.ReadAsStringAsync();
            PaymentInstrument pi = JsonConvert.DeserializeObject<PaymentInstrument>(responseContent);
            PaymentMethod pm = pi.PaymentMethod;

            Assert.IsNotNull(pi, "Expected payment instrument to not be null");
            Assert.AreEqual(PaymentInstrumentStatus.Active, pi.Status, "Payment Instrument status expected to be 'Active'");
            Assert.IsNotNull(pm, "Expected payment method to not be null");
            Assert.AreEqual("credit_card", pm.PaymentMethodFamily, "PaymentMethodFamily expected to be credit_card");
            Assert.AreEqual(type, pm.PaymentMethodType, "PaymentMethodType expected to be rupay");
        }

        [DataRow("windowsstore", "us", "Account001", "selectinstance", true)]
        [DataRow("azure", "us", "Account001", "selectinstance", false)]
        [TestMethod]
        public async Task PaymentInstrumentsEx_ListModernPI_Success(string partner, string country, string accountid, string operation, bool usePartnerSetting)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}&operation={operation}")),
                Method = HttpMethod.Get
            };

            if (usePartnerSetting)
            {
                request.Headers.Add("x-ms-flight", "PXUsePartnerSettingsService,PXDisablePSSCache");
                string partnerSettingResponse = "{\"selectinstance\":{\"template\":\"listpidropdown\",\"features\":{\"addNewPaymentMethodOption\":{\"applicableMarkets\":[]}}}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
            }

            var expectedPIs = PimsMockResponseProvider.ListPaymentInstruments(accountid);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPIs));
            var expectedPI = expectedPIs.FirstOrDefault();
            Assert.IsNotNull(expectedPI);

            HttpResponseMessage result = await PXClient.SendAsync(request);

            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK, "Expected 200 OK response");

            string responseContent = await result.Content.ReadAsStringAsync();
            PaymentInstrument pi = JsonConvert.DeserializeObject<List<PaymentInstrument>>(responseContent).Last();
            Assert.IsNotNull(pi);
            var family = pi.PaymentMethod.PaymentMethodFamily;
            Assert.IsNotNull(family);

            if (usePartnerSetting)
            {
                Assert.AreEqual("add_new_payment_method", family, "PaymentMethodFamily expected to be add_new_payment_method");
            }
            else
            {
                Assert.AreNotEqual("add_new_payment_method", family, "PaymentMethodFamily expected to be add_new_payment_method");
            }
        }

        [DataRow("webblends", "in", "Account004", "upi", "PxEnableUpi")]
        [DataRow("azure", "in", "Account008", "upi_commercial", "PxCommercialEnableUpi")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_Upi_ListModernPI_Success(string partner, string country, string accountid, string type, string flight)
        {
            object requestBody = new
            {
                paymentMethodFamily = "real_time_payments",
                paymentMethodType = type,
                details = new
                {
                    address = new
                    {
                        address_line1 = "One Microsoft Way",
                        city = "Redmond",
                        region = "WA",
                        country = "US",
                        postal_code = "11111"
                    },
                },
                riskData = new { }
            };

            var expectedPIs = PimsMockResponseProvider.ListPaymentInstruments(accountid);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPIs));
            var expectedPI = expectedPIs.FirstOrDefault();
            Assert.IsNotNull(expectedPI);

            PXFlightHandler.AddToEnabledFlights(flight);
            HttpResponseMessage result = await PXClient.GetAsync($"/v7.0/{accountid}/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}");

            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK, "Expected 200 OK response");

            string responseContent = await result.Content.ReadAsStringAsync();
            PaymentInstrument pi = JsonConvert.DeserializeObject<List<PaymentInstrument>>(responseContent).FirstOrDefault();
            Assert.IsNotNull(pi);

            PaymentMethod pm = pi.PaymentMethod;

            Assert.IsNotNull(pi, "Expected payment instrument to not be null");
            Assert.AreEqual(PaymentInstrumentStatus.Active, pi.Status, "Payment Instrument status expected to be 'Active'");
            Assert.IsNotNull(pm, "Expected payment method to not be null");
            Assert.AreEqual("real_time_payments", pm.PaymentMethodFamily, "PaymentMethodFamily expected to be real_time_payments");
            Assert.AreEqual(type, pm.PaymentMethodType, "PaymentMethodType expected to be upi");
            Assert.IsNotNull(pi.PaymentInstrumentDetails);
            Assert.IsNotNull(pi.PaymentInstrumentDetails.Vpa);
            Assert.IsTrue(pi.PaymentInstrumentDetails.Vpa == expectedPI.PaymentInstrumentDetails.Vpa);
        }

        [TestMethod]
        public async Task PaymentInstrumentsEx_ListModernPI_HasCardArt()
        {
            var accountId = "Account012";
            
            PXFlightHandler.AddToEnabledFlights("ListModernPIsWithCardArt");
            HttpResponseMessage result = await PXClient.GetAsync($"/v7.0/{accountId}/paymentInstrumentsEx?country=us&language=en-US&partner=northstarweb");

            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK, "Expected 200 OK response");

            string responseContent = await result.Content.ReadAsStringAsync();
            PaymentInstrument pi = JsonConvert.DeserializeObject<List<PaymentInstrument>>(responseContent).Find(p => p.PaymentInstrumentId == $"{accountId}-WithNetworkTokenWithCardArt");

            Assert.IsNotNull(pi, "pi");
            Assert.IsNotNull(pi.PaymentInstrumentDetails?.NetworkTokens?.FirstOrDefault(), "PI doesn't have NetworkTokens");
            Assert.AreEqual($"{accountId}-TokenId", pi.PaymentInstrumentDetails.NetworkTokens.First().Id, "NetworkToken ID");
            Assert.IsNotNull(pi.PaymentMethod?.Display?.CardArt, "PI doesn't have Card Art");
            Assert.AreEqual("CardArt.png", pi.PaymentMethod.Display.CardArt.CardArtUrl, "Card Art Url");
            Assert.AreEqual("MediumCardArt.png", pi.PaymentMethod.Display.CardArt.MediumCardArtUrl, "Medium Card Art Url");
            Assert.AreEqual("ThumbnailCardArt.png", pi.PaymentMethod.Display.CardArt.ThumbnailCardArtUrl, "Thumbnail Card Art Url");
            Assert.AreEqual("#FFFFFF", pi.PaymentMethod.Display.CardArt.ForegroundColor, "Foreground Color");
        }

        [DataRow("WithNetworkTokenWithCardArt", false, true)]
        [DataRow("WithNetworkTokenWithNoCardArt", true, true)]
        [DataRow("WithNoNetworkTokens", true, false)]
        [TestMethod]
        public async Task PaymentInstrumentsEx_ListModernPI_HasNoCardArt(string idSuffix, bool enableFlight, bool expectNetworkTokens)
        {
            var accountId = "Account012";

            if (enableFlight)
            {
                PXFlightHandler.AddToEnabledFlights("ListModernPIsWithCardArt");
            }

            HttpResponseMessage result = await PXClient.GetAsync($"/v7.0/{accountId}/paymentInstrumentsEx?country=us&language=en-US&partner=northstarweb");

            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK, "Expected 200 OK response");

            string responseContent = await result.Content.ReadAsStringAsync();
            PaymentInstrument pi = JsonConvert.DeserializeObject<List<PaymentInstrument>>(responseContent).Find(p => p.PaymentInstrumentId == $"{accountId}-{idSuffix}");

            Assert.IsNotNull(pi, "pi");
            if (expectNetworkTokens)
            {
                Assert.IsNotNull(pi.PaymentInstrumentDetails?.NetworkTokens?.FirstOrDefault(), "PI doesn't have NetworkTokens");
            } 
            else
            {
                Assert.IsNull(pi.PaymentInstrumentDetails?.NetworkTokens, "PI has NetworkTokens");
            }

            Assert.IsNull(pi.PaymentMethod?.Display?.CardArt, "PI doesn't have Card Art");
        }

        [DataRow(HttpStatusCode.Forbidden, "")]
        [DataRow(HttpStatusCode.BadRequest, "\"errorCode\":\"invalidInput\",\"description\":\"Customer ID not provided.\"")]
        [DataRow(HttpStatusCode.InternalServerError, "\"errorCode\":\"internalError\",\"description\":\"UnhandledException.\"")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_ListModernPI_DoesNotFailWhenCardArtServiceFails(HttpStatusCode statusCode, string errorResponse)
        {
            var accountId = "Account012";

            PXSettings.NetworkTokenizationService.ArrangeResponse(errorResponse, statusCode);
            PXFlightHandler.AddToEnabledFlights("ListModernPIsWithCardArt");
            HttpResponseMessage result = await PXClient.GetAsync($"/v7.0/{accountId}/paymentInstrumentsEx?country=us&language=en-US&partner=northstarweb");

            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK, "Expected 200 OK response");

            string responseContent = await result.Content.ReadAsStringAsync();
            PaymentInstrument pi = JsonConvert.DeserializeObject<List<PaymentInstrument>>(responseContent).Find(p => p.PaymentInstrumentId == $"{accountId}-WithNetworkTokenWithCardArt");

            Assert.IsNotNull(pi, "pi");
            Assert.IsNotNull(pi.PaymentInstrumentDetails?.NetworkTokens?.FirstOrDefault(), "PI doesn't have NetworkTokens");
            Assert.AreEqual($"{accountId}-TokenId", pi.PaymentInstrumentDetails.NetworkTokens.First().Id, "NetworkToken ID");
            Assert.IsNull(pi.PaymentMethod?.Display?.CardArt, "PI has Card Art");
        }

        [TestMethod]
        [DataRow("xbox", "Account001-3DS1-Redirect")]
        [DataRow("amcxbox", "Account001-3DS1-Redirect")]
        [DataRow("webblends", "Account001-3DS1-Redirect")]
        public async Task PaymentInstrumentsEx_3DS1_ShowIFrameTestHeader(string partner, string piid)
        {
            var pimsRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                details = new
                {
                    address = new
                    {
                        address_line1 = "One Microsoft Way",
                        city = "Saket",
                        region = "DE",
                        country = "IN",
                        postal_code = "111111"
                    }
                }
            };

            var sessionResponse = "{\"id\":\"ab53ebd0-0b70-41e6-b1ec-12345678\",\"session_type\":\"any\",\"data\":\"{\\\"ProviderName\\\":\\\"BillDesk\\\",\\\"Data\\\":\\\"{\\\\\\\"Operation\\\\\\\":1,\\\\\\\"InitiationRedirectUrl\\\\\\\":\\\\\\\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/payments/ve1_2/challenge\\\\\\\",\\\\\\\"InitiationParameters\\\\\\\":{\\\\\\\"MD\\\\\\\":\\\\\\\"FakeMD\\\\\\\",\\\\\\\"PaReq\\\\\\\":\\\\\\\"FakePaReq\\\\\\\",\\\\\\\"TermUrl\\\\\\\":\\\\\\\"https://www.merchanturl.com/Response.jsp\\\\\\\"},\\\\\\\"AccountId\\\\\\\":\\\\\\\"bd888d21-f2a9-4f8b-92c2-12345678\\\\\\\",\\\\\\\"IpAddress\\\\\\\":null,\\\\\\\"UserAgent\\\\\\\":null,\\\\\\\"SubscriptionId\\\\\\\":\\\\\\\"12345678\\\\\\\",\\\\\\\"ApiVersion\\\\\\\":\\\\\\\"v1_2\\\\\\\",\\\\\\\"PaymentInstrumentId\\\\\\\":\\\\\\\"f7940456-15a8-4c32-b562-12345678\\\\\\\",\\\\\\\"PaymentMethodFamily\\\\\\\":\\\\\\\"credit_card\\\\\\\",\\\\\\\"PaymentMethodType\\\\\\\":\\\\\\\"visa\\\\\\\",\\\\\\\"CvvToken\\\\\\\":\\\\\\\"FakeToken\\\\\\\",\\\\\\\"PaymentId\\\\\\\":\\\\\\\"Z10009BINTWG58e3bea8-9540-4ae3-9ddf-2e32d1e06d7b\\\\\\\",\\\\\\\"TransactionId\\\\\\\":\\\\\\\"255b9d4d-c3f1-4812-9fdd-65183ef32cb3\\\\\\\",\\\\\\\"MerchantReferenceNumber\\\\\\\":\\\\\\\"FakeReference\\\\\\\",\\\\\\\"TransactionType\\\\\\\":\\\\\\\"Validate\\\\\\\",\\\\\\\"MerchantId\\\\\\\":\\\\\\\"REDMOND\\\\\\\",\\\\\\\"TrackingId\\\\\\\":\\\\\\\"59cadaf3-480b-4dbc-bb84-12345678\\\\\\\",\\\\\\\"MandateId\\\\\\\":null,\\\\\\\"ThreeDSChargeStatus\\\\\\\":\\\\\\\"Initiated\\\\\\\",\\\\\\\"ThreeDSAuthParameters\\\\\\\":null,\\\\\\\"ThreeDSChargeAmount\\\\\\\":2.0,\\\\\\\"ThreeDSChargeCurrency\\\\\\\":\\\\\\\"INR\\\\\\\",\\\\\\\"RemainingThreeDSBalance\\\\\\\":2.0,\\\\\\\"AuthenticationId\\\\\\\":\\\\\\\"40c1c487-3882-4e26-a01f-12345678\\\\\\\",\\\\\\\"AuthenticationResults\\\\\\\":null,\\\\\\\"BillDeskTransactionId\\\\\\\":null,\\\\\\\"BillDeskTransactionDate\\\\\\\":\\\\\\\"0001-01-01T00:00:00\\\\\\\",\\\\\\\"IsCommercial\\\\\\\":false,\\\\\\\"IsFullPageRedirect\\\\\\\":true}\\\"}\",\"encrypt_data\":false,\"state\":\"Initial\",\"test_context\":{\"scenarios\":\"px-service-psd2-e2e-emulator,BillDeskForceCreateToken,BillDeskUseTestEncryption,BillDeskTokenization\",\"contact\":\"blah\",\"context_props\":{}}}";

            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", piid);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXSettings.SessionService.ArrangeResponse(sessionResponse);

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country=in&language=en-US&partner={partner}&scenario=fixedCountrySelection")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(pimsRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            request.Headers.Add("x-ms-test", "{\"scenarios\": \"px-service-3ds1-show-iframe\", \"contact\": \"TestApp\"}");

            HttpResponseMessage result = await PXClient.SendAsync(request);

            var pidlResource = ReadSinglePidlResourceFromJson(await result.Content.ReadAsStringAsync());
            Assert.IsNotNull(pidlResource.ClientAction, "Client action missing");
            Assert.AreEqual(ClientActionType.Pidl, pidlResource.ClientAction.ActionType);
            Assert.IsNotNull(pidlResource.ClientAction.Context, "Client action context missing");

            var pidlList = pidlResource.ClientAction.Context as List<PIDLResource>;
            Assert.AreEqual(1, pidlList.Count);
            var resource = pidlList[0];

            if (string.Equals(partner, "xbox", StringComparison.InvariantCultureIgnoreCase) || string.Equals(partner, "amcxbox", StringComparison.InvariantCultureIgnoreCase))
            {
                var qrCodeImage = resource.GetDisplayHintById("ccThreeDSQrCodeImage");
                Assert.IsNotNull(qrCodeImage, "QR code image missing");

                var expectedURL = "https://mockRedirectUrl.com/?ru=https%3A%2F%2Fwww.microsoft.com%2Fen-US%2Fstore%2Frds%2Fv2%2FGeneralAddPISuccess%3FpicvRequired%3DFalse%26type%3Dvisa%26family%3Dcredit_card%26id%3D&rx=https%3A%2F%2Fwww.microsoft.com%2Fen-US%2Fstore%2Frds%2Fv2%2FGeneralAddPIFailure";

                var goToBankButton = resource.GetDisplayHintById("goToBankButton");
                Assert.IsNotNull(goToBankButton, "Go to bank button not found");
                Assert.AreEqual("moveNext", goToBankButton.Action.ActionType);
                var iFrame = resource.GetDisplayHintById("globalPIQrCodeIframe") as IFrameDisplayHint;
                Assert.AreEqual(expectedURL, iFrame.SourceUrl, "iFrame source url not as expected");
            }
            else
            {
                var iFrame = resource.GetDisplayHintById("threeDSIframe") as IFrameDisplayHint;
                var expectedContent = "<html><body onload=\"window.location.href = 'https://mockRedirectUrl.com/?ru=https%3A%2F%2Fpifd.cp.microsoft-int.com%2FV6.0%2FpaymentInstrumentsEx%2FAccount001-3DS1-Redirect%2Fresume%3Fcountry%3Din%26language%3Den-US%26partner%3Dwebblends%26isSuccessful%3DTrue%26sessionQueryUrl%3Dsessions%2Fabcd-12345&rx=https%3A%2F%2Fpifd.cp.microsoft-int.com%2FV6.0%2FpaymentInstrumentsEx%2FAccount001-3DS1-Redirect%2Fresume%3Fcountry%3Din%26language%3Den-US%26partner%3Dwebblends%26isSuccessful%3DFalse%26sessionQueryUrl%3Dsessions%2Fabcd-12345'\"></body></html>";
                Assert.IsNotNull(iFrame, "iFrame is missing");
                Assert.AreEqual(expectedContent, iFrame.DisplayContent, "iFrame source url not as expected");
                Assert.IsNotNull(iFrame.DisplayTags, "Accessibility display tags are missing");
                Assert.AreEqual(iFrame.DisplayTags["accessibilityName"], "The bank authentication dialog");
            }
        }

        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_Replace_BillableAccountIdExtracted()
        {
            // Arrange
            bool assertCalled = false;
            var emptyRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                details = new
                {
                    expiryDate = "07/35"
                }
            };

            string oldPI = "9igMnQAAAAAqAACA";
            string expectedBillableAccountId = "9igMnQAAAAAAAAAA";
            var newPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi002-MC");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(newPI));

            PXSettings.OrchestrationService.ResetToDefaults();
            PXSettings.OrchestrationService.ArrangeResponse(string.Empty, HttpStatusCode.NoContent, HttpMethod.Post, ".*/replace.*");

            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(newPI));

            PXSettings.PimsService.PreProcess = async (request) =>
            {
                if (request.RequestUri.AbsolutePath.Contains($"/v4.0/Account001/paymentInstruments"))
                {
                    var queryparams = request.GetQueryNameValuePairs();
                    Assert.IsFalse(queryparams.Contains(new KeyValuePair<string, string>("billableAccountId", expectedBillableAccountId)), "Correct billable account id was extracted from the given piid");

                    string requestContent = await request.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(requestContent);

                    // Assert the PIMS Post PI request payload request
                    Assert.AreEqual("07", (string)json["details"]["expiryMonth"], "The expiryMonth in the request doesn't match the expected value.");
                    Assert.AreEqual("2035", (string)json["details"]["expiryYear"], "The expiryYear in the request doesn't match the expected value.");

                    assertCalled = true;
                }
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx/{oldPI}/replace?country=us&language=en-US&partner=webblends", new StringContent(JsonConvert.SerializeObject(emptyRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK);
            Assert.IsTrue(assertCalled, "PimsTestHandler.PreProcess wasn't called");
            PXSettings.OrchestrationService.ResetToDefaults();
            PXSettings.PimsService.ResetToDefaults();
        }

        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_ReplaceAsExpected()
        {
            // Arrange
            bool assertCalled = false;
            var emptyRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                targetPaymentInstrumentId = "Account001-Pi002-MC"
            };

            var oldPI = "Account001-Pi001-Visa";
            PXSettings.OrchestrationService.ResetToDefaults();

            PXSettings.OrchestrationService.PreProcess = async (request) =>
            {
                if (request.RequestUri.AbsolutePath.Contains($"{oldPI}/replace"))
                {
                    string contentStr = await request.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(contentStr);
                    JToken targetPaymentInstrumentIdToken = json.SelectToken("targetPaymentInstrumentId");
                    Assert.IsNotNull(targetPaymentInstrumentIdToken);
                    Assert.AreEqual("Account001-Pi002-MC", targetPaymentInstrumentIdToken.Value<string>());
                    assertCalled = true;
                }
            };

            PXSettings.OrchestrationService.ArrangeResponse(string.Empty, HttpStatusCode.NoContent, HttpMethod.Post, ".*/replace.*");

            // Act
            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx/{oldPI}/replace?country=us&language=en-US&partner=webblends", new StringContent(JsonConvert.SerializeObject(emptyRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert
            Assert.IsTrue(result.IsSuccessStatusCode);
            Assert.IsTrue(assertCalled, "OrchestrationService.PreProcess wasn't called");

            PXSettings.OrchestrationService.ResetToDefaults();
        }

        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_Replace_PSD2ChallengeAsExpected()
        {
            // Arrange
            bool replaceActionCalled = false;
            var emptyRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa"
            };

            var oldPI = "Account001-Pi001-Visa";
            var newPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi002-MC");
            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi002-MC");
            extendedPI.PaymentInstrumentDetails.RequiredChallenge = new List<string> { "3ds2" };
            var expectedResponse = new { piid = newPI.PaymentInstrumentId, challengeRequired = true, pi = newPI };

            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(newPI));

            PXSettings.OrchestrationService.PreProcess = (request) =>
            {
                if (request.RequestUri.AbsolutePath.Contains($"{oldPI}/replace"))
                {
                    replaceActionCalled = true;
                }
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx/{oldPI}/replace?country=us&language=en-US&partner=webblends&scenario=hasSubsOrPreOrders", new StringContent(JsonConvert.SerializeObject(emptyRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            Assert.IsFalse(replaceActionCalled, "PimsTestHandler.PreProcess was called for ReplaceAction");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            var contentStr = await result.Content.ReadAsStringAsync();
            JObject actualJson = JObject.Parse(contentStr);
            Assert.AreEqual(expectedResponse.challengeRequired, actualJson.SelectToken("challengeRequired").Value<bool>());
            Assert.AreEqual(expectedResponse.piid, actualJson.SelectToken("piid").Value<string>());
            Assert.AreEqual(expectedResponse.pi.PaymentInstrumentId, actualJson.SelectToken("pi.id").Value<string>());
            PXSettings.OrchestrationService.ResetToDefaults();
        }

        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_Replace_PSD2ChallengeWithTestHeaderAsExpected()
        {
            // Arrange
            bool replaceActionCalled = false;
            var emptyRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa"
            };

            var oldPI = "Account001-Pi001-Visa";
            var newPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi002-MC");
            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi002-MC");
            extendedPI.PaymentInstrumentDetails.RequiredChallenge = new List<string>();
            var expectedResponse = new { piid = newPI.PaymentInstrumentId, challengeRequired = true, pi = newPI };

            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(newPI));

            PXSettings.OrchestrationService.PreProcess = (request) =>
            {
                if (request.RequestUri.AbsolutePath.Contains($"{oldPI}/replace"))
                {
                    replaceActionCalled = true;
                }
            };

            // Act
            HttpRequestMessage replaceRequest = new HttpRequestMessage(HttpMethod.Post, $"/v7.0/Account001/paymentInstrumentsEx/{oldPI}/replace?country=us&language=en-US&partner=webblends&scenario=hasSubsOrPreOrders");
            replaceRequest.Headers.Add("x-ms-test", "{scenarios: \"px-service-psd2-e2e-emulator\", contact: \"tester\"}");
            replaceRequest.Content = new StringContent(JsonConvert.SerializeObject(emptyRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType);
            HttpResponseMessage result = await PXClient.SendAsync(replaceRequest);

            // Assert (continuation)
            Assert.IsFalse(replaceActionCalled, "PimsTestHandler.PreProcess was called for ReplaceAction");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            var contentStr = await result.Content.ReadAsStringAsync();
            JObject actualJson = JObject.Parse(contentStr);
            Assert.AreEqual(expectedResponse.challengeRequired, actualJson.SelectToken("challengeRequired").Value<bool>());
            Assert.AreEqual(expectedResponse.piid, actualJson.SelectToken("piid").Value<string>());
            Assert.AreEqual(expectedResponse.pi.PaymentInstrumentId, actualJson.SelectToken("pi.id").Value<string>());
            PXSettings.OrchestrationService.ResetToDefaults();
        }

        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_ReplaceWithExistingPIAsExpected()
        {
            // Arrange
            bool assertCalled = false;
            var oldPI = "Account001-Pi001-Visa";
            var newPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi002-MC");
            var requestBody = new
            {
                targetPaymentInstrumentId = newPI.PaymentInstrumentId
            };

            PXSettings.OrchestrationService.ResetToDefaults();
            PXSettings.OrchestrationService.ArrangeResponse(string.Empty, HttpStatusCode.NoContent, HttpMethod.Post, ".*/replace.*");

            PXSettings.OrchestrationService.PreProcess = async (request) =>
            {
                if (request.RequestUri.AbsolutePath.Contains($"{oldPI}/replace"))
                {
                    string contentStr = await request.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(contentStr);
                    JToken targetPaymentInstrumentIdToken = json.SelectToken("targetPaymentInstrumentId");
                    Assert.IsNotNull(targetPaymentInstrumentIdToken);
                    Assert.AreEqual(newPI.PaymentInstrumentId, targetPaymentInstrumentIdToken.Value<string>());
                    assertCalled = true;
                }
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx/{oldPI}/replace?country=us&language=en-US&partner=webblends", new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            PXSettings.OrchestrationService.ResetToDefaults();
            Assert.IsTrue(assertCalled, "PimsTestHandler.PreProcess wasn't called");
        }

        [DataRow(null)]
        [DataRow("")]
        [DataRow("asdfasdf")]
        [DataTestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_ReplaceWithExistignPIAndPaymentSessionIdAsExpected(string paymentSessionId)
        {
            // Arrange
            bool assertCalled = false;
            var oldPI = "Account001-Pi001-Visa";
            var targetPaymentInstrumentId = "Account001-Pi002-MC";
            var requestBody = new
            {
                targetPaymentInstrumentId = targetPaymentInstrumentId,
                paymentSessionId = paymentSessionId
            };
            PXSettings.OrchestrationService.PreProcess = async (request) =>
            {
                if (request.RequestUri.AbsolutePath.Contains($"{oldPI}/replace"))
                {
                    string contentStr = await request.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(contentStr);
                    JToken targetPaymentInstrumentIdToken = json.SelectToken("targetPaymentInstrumentId");
                    Assert.IsNotNull(targetPaymentInstrumentIdToken);
                    Assert.AreEqual(targetPaymentInstrumentId, targetPaymentInstrumentIdToken.Value<string>());
                    JToken paymentSessionIdToken = json.SelectToken("paymentSessionId");
                    Assert.IsNotNull(paymentSessionIdToken);
                    Assert.AreEqual(paymentSessionId, paymentSessionIdToken.Value<string>());
                    assertCalled = true;
                }
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx/{oldPI}/replace?country=us&language=en-US&partner=webblends", new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            Assert.IsTrue(assertCalled, "PimsOrchestrationTestHandler.PreProcess wasn't called");
            PXSettings.OrchestrationService.ResetToDefaults();
        }

        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_ReplaceWithExistignPI_PSD2ChallengeAsExpected()
        {
            // Arrange
            bool replaceActionCalled = false;
            var oldPI = "Account001-Pi001-Visa";
            var targetPaymentInstrumentId = "Account001-Pi002-MC";
            var requestBody = new { targetPaymentInstrumentId = targetPaymentInstrumentId };
            var expectedResponse = new { piid = targetPaymentInstrumentId, challengeRequired = true };

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi002-MC");
            extendedPI.PaymentInstrumentDetails.RequiredChallenge = new List<string> { "3ds2" };
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            PXSettings.OrchestrationService.PreProcess = (request) =>
            {
                if (request.RequestUri.AbsolutePath.Contains($"{oldPI}/replace"))
                {
                    replaceActionCalled = true;
                }
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx/{oldPI}/replace?country=us&language=en-US&partner=webblends&scenario=hasSubsOrPreOrders", new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            Assert.IsFalse(replaceActionCalled, "PimsTestHandler.PreProcess was called for ReplaceAction");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(JsonConvert.SerializeObject(expectedResponse), await result.Content.ReadAsStringAsync());
            PXSettings.OrchestrationService.ResetToDefaults();
        }

        public async Task OrchestrationEx_PaymentInstrument_Replace()
        {
            PXSettings.OrchestrationService.ResetToDefaults();
            Microsoft.Commerce.Payments.PidlFactory.V7.PIDLData sampleData = new Microsoft.Commerce.Payments.PidlFactory.V7.PIDLData();
            sampleData.Add("targetPaymentInstrumentId", "textPiid");

            PXSettings.OrchestrationService.ArrangeResponse(string.Empty, HttpStatusCode.NoContent, HttpMethod.Post, ".*/replace.*");

            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx/testPiid/replace?country=us&language=en-US&partner=webblends", new StringContent(JsonConvert.SerializeObject(sampleData), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));
            Assert.IsTrue(result.IsSuccessStatusCode);

            PXSettings.OrchestrationService.ResetToDefaults();
        }

        [DataRow(HttpStatusCode.BadRequest, "PassThroughErrorCode", "Pass through Some custom error code.", "Try that again. Something happened on our end. Waiting a bit can help.")]
        [DataRow(HttpStatusCode.Forbidden, "Unauthorized", "UnAuthorized operation.", "Try that again. Something happened on our end. Waiting a bit can help.")]
        [DataRow(HttpStatusCode.InternalServerError, "InternalServer", "Internal server error.", "Try that again. Something happened on our end. Waiting a bit can help.")]
        [DataRow(HttpStatusCode.GatewayTimeout, "Timeout", "Timeout.", "Try that again. Something happened on our end. Waiting a bit can help.")]
        [DataRow(HttpStatusCode.NotFound, "NotFound", "Not found.", "Try that again. Something happened on our end. Waiting a bit can help.")]
        [DataTestMethod]
        public async Task OrchestrationEx_PaymentInstrument_Replace_Error(HttpStatusCode statusCode, string errorCode, string orchErrorMessage, string pxErrorMessage)
        {
            PXSettings.OrchestrationService.ResetToDefaults();
            Microsoft.Commerce.Payments.PidlFactory.V7.PIDLData sampleData = new Microsoft.Commerce.Payments.PidlFactory.V7.PIDLData();
            sampleData.Add("targetPaymentInstrumentId", "textPiid");

            Microsoft.Commerce.Payments.PXService.Accessors.OrchestrationService.DataModel.OrchestrationErrorResponse response = new Microsoft.Commerce.Payments.PXService.Accessors.OrchestrationService.DataModel.OrchestrationErrorResponse()
            {
                ErrorCode = errorCode,
                Message = orchErrorMessage,
                Targets = new List<string>()
            };

            PXSettings.OrchestrationService.ArrangeResponse(JsonConvert.SerializeObject(response), statusCode, HttpMethod.Post, ".*/replace.*");

            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx/testPiid/replace?country=us&language=en-US&partner=webblends", new StringContent(JsonConvert.SerializeObject(sampleData), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));
            var error = JsonConvert.DeserializeObject<global::Tests.Common.Model.ErrorResponse>(await result.Content.ReadAsStringAsync());
            Assert.AreEqual(error.ErrorCode, errorCode);
            Assert.AreEqual(error.Message, pxErrorMessage);
            Assert.AreEqual(result.StatusCode, statusCode);

            PXSettings.OrchestrationService.ResetToDefaults();
        }

        [TestMethod]
        public async Task OrchestrationEx_PaymentInstrument_Remove()
        {
            PXSettings.OrchestrationService.ResetToDefaults();

            PXSettings.OrchestrationService.ArrangeResponse(string.Empty, HttpStatusCode.NoContent, HttpMethod.Post, ".*/remove.*");

            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx/testPiid/remove?country=us&language=en-US&partner=webblends", new StringContent(string.Empty, Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));
            Assert.IsTrue(result.IsSuccessStatusCode);

            PXSettings.OrchestrationService.ResetToDefaults();
        }

        [DataRow(HttpStatusCode.BadRequest, "PassThroughErrorCode", "Pass through Some custom error code.", "Try that again. Something happened on our end. Waiting a bit can help.")]
        [DataRow(HttpStatusCode.Forbidden, "Unauthorized", "UnAuthorized operation.", "Try that again. Something happened on our end. Waiting a bit can help.")]
        [DataRow(HttpStatusCode.InternalServerError, "InternalServer", "Internal server error.", "Try that again. Something happened on our end. Waiting a bit can help.")]
        [DataRow(HttpStatusCode.GatewayTimeout, "Timeout", "Timeout.", "Try that again. Something happened on our end. Waiting a bit can help.")]
        [DataRow(HttpStatusCode.NotFound, "NotFound", "Not found.", "Try that again. Something happened on our end. Waiting a bit can help.")]
        [DataTestMethod]
        public async Task OrchestrationEx_PaymentInstrument_Remove_Error(HttpStatusCode statusCode, string errorCode, string orchErrorMessage, string pxErrorMessage)
        {
            PXSettings.OrchestrationService.ResetToDefaults();
            Microsoft.Commerce.Payments.PXService.Accessors.OrchestrationService.DataModel.OrchestrationErrorResponse response = new Microsoft.Commerce.Payments.PXService.Accessors.OrchestrationService.DataModel.OrchestrationErrorResponse()
            {
                ErrorCode = errorCode,
                Message = orchErrorMessage,
                Targets = new List<string>()
            };

            PXSettings.OrchestrationService.ArrangeResponse(JsonConvert.SerializeObject(response), statusCode, HttpMethod.Post, ".*/remove.*");

            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx/testPiid/remove?country=us&language=en-US&partner=webblends", new StringContent(JsonConvert.SerializeObject(string.Empty), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));
            var error = JsonConvert.DeserializeObject<global::Tests.Common.Model.ErrorResponse>(await result.Content.ReadAsStringAsync());
            Assert.AreEqual(error.ErrorCode, errorCode);
            Assert.AreEqual(error.Message, pxErrorMessage);
            Assert.AreEqual(result.StatusCode, statusCode);

            PXSettings.OrchestrationService.ResetToDefaults();
        }

        [DataRow(HttpStatusCode.BadRequest, "", "SubscriptionNotCanceled", "Payment Instrument cannot be removed, there are ModernSubscriptionsNotCanceled referencing it.", "Try that again. Something happened on our end. Waiting a bit can help.")]
        [DataRow(HttpStatusCode.BadRequest, "XboxNativeBaseErrorPage", "SubscriptionNotCanceled", "Payment Instrument cannot be removed, there are ModernSubscriptionsNotCanceled referencing it.", "Try that again. Something happened on our end. Waiting a bit can help.")]
        [DataRow(HttpStatusCode.BadRequest, "XboxNativeBaseErrorPage", "OutstandingBalance", "The payment instrument has outstanding balance that is not paid.", "Try that again. Something happened on our end. Waiting a bit can help.")]
        [DataRow(HttpStatusCode.BadRequest, "XboxNativeBaseErrorPage", "RemovePIAccessDeniedForTheCaller", "Remove business instrument is not supported. Details: removablePaymentInstrumentIds: . nonRemovedPaymentInstrumentIds: testPiid.", "Try that again. Something happened on our end. Waiting a bit can help.")]
        [DataRow(HttpStatusCode.BadRequest, "PXEnableXboxNativeStyleHints,PXUsePostProcessingFeatureForRemovePI", "SubscriptionNotCanceled", "Payment Instrument cannot be removed, there are ModernSubscriptionsNotCanceled referencing it.", "Try that again. Something happened on our end. Waiting a bit can help.")]
        [DataRow(HttpStatusCode.BadRequest, "XboxNativeBaseErrorPage,PXEnableXboxNativeStyleHints,PXUsePostProcessingFeatureForRemovePI", "SubscriptionNotCanceled", "Payment Instrument cannot be removed, there are ModernSubscriptionsNotCanceled referencing it.", "Try that again. Something happened on our end. Waiting a bit can help.")]
        [DataRow(HttpStatusCode.BadRequest, "XboxNativeBaseErrorPage,PXEnableXboxNativeStyleHints,PXUsePostProcessingFeatureForRemovePI", "OutstandingBalance", "The payment instrument has outstanding balance that is not paid.", "Try that again. Something happened on our end. Waiting a bit can help.")]
        [DataRow(HttpStatusCode.BadRequest, "XboxNativeBaseErrorPage,PXEnableXboxNativeStyleHints,PXUsePostProcessingFeatureForRemovePI", "RemovePIAccessDeniedForTheCaller", "Remove business instrument is not supported. Details: removablePaymentInstrumentIds: . nonRemovedPaymentInstrumentIds: testPiid.", "Try that again. Something happened on our end. Waiting a bit can help.")]
        [DataTestMethod]
        public async Task OrchestrationEx_PaymentInstrument_Remove_Error_With_Client_Action(HttpStatusCode statusCode, string flightName, string errorCode, string orchErrorMessage, string pxErrorMessage)
        {
            bool useStyleHints = flightName?.Contains("PXEnableXboxNativeStyleHints") == true && flightName?.Contains("PXUsePostProcessingFeatureForRemovePI") == true;
            PXSettings.OrchestrationService.ResetToDefaults();
            Microsoft.Commerce.Payments.PXService.Accessors.OrchestrationService.DataModel.OrchestrationErrorResponse response = new Microsoft.Commerce.Payments.PXService.Accessors.OrchestrationService.DataModel.OrchestrationErrorResponse()
            {
                ErrorCode = errorCode,
                Message = orchErrorMessage,
                Targets = new List<string>(),
            };

            if (!string.IsNullOrEmpty(flightName))
            {
                PXFlightHandler.AddToEnabledFlights(flightName);
            }

            PXSettings.OrchestrationService.ArrangeResponse(JsonConvert.SerializeObject(response), statusCode, HttpMethod.Post, ".*/remove.*");
            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx/testPiid/remove?country=us&language=en-US&partner=xboxsettings", new StringContent(JsonConvert.SerializeObject(string.Empty), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            string content = await result.Content.ReadAsStringAsync();
            PIDLResource pidl = ReadSinglePidlResourceFromJson(content);
            List<PIDLResource> pidls = pidl.ClientAction.Context as List<PIDLResource>;

            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK);
            Assert.IsNotNull(result.Content);
            Assert.IsTrue(content.Contains("clientAction"));

            foreach (PIDLResource pidlResource in pidls)
            {
                foreach (PageDisplayHint page in pidlResource.DisplayPages)
                {
                    List<DisplayHint> displayHints = pidlResource.GetAllDisplayHints(page);
                    foreach (DisplayHint displayHint in displayHints)
                    {
                        if (useStyleHints)
                        {
                            Assert.IsTrue(displayHint.StyleHints.Count > 0);
                        }
                    }
                }
            }

            PXSettings.OrchestrationService.ResetToDefaults();
        }

        [DataRow(HttpStatusCode.NotFound, "NotFound", "Not found.", "Not found.")]
        [DataRow(HttpStatusCode.InternalServerError, "InternalError", "InternalServerError", "Exception of type 'Microsoft.Commerce.Payments.PXService.ServiceErrorResponseException' was thrown.")]
        [DataTestMethod]
        public async Task GetMordernPi_StatusCode_NotFound(HttpStatusCode statusCode, string errorCode, string pimsErrorMessage, string pxErrorMessage)
        {
            ServiceErrorResponse response = new ServiceErrorResponse()
            {
                ErrorCode = errorCode,
                Message = pimsErrorMessage,
            };

            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(response), statusCode, HttpMethod.Get);

            HttpResponseMessage result = await PXClient.GetAsync($"/v7.0/Account001/paymentInstrumentsEx/testPiid");
            ServiceErrorResponse error = JsonConvert.DeserializeObject<ServiceErrorResponse>(await result.Content.ReadAsStringAsync());
            Assert.AreEqual(result.StatusCode, statusCode);
            Assert.AreEqual(error.ErrorCode, errorCode);
            Assert.AreEqual(error.Message, pxErrorMessage);
        }

        [DataRow("{\"id\":\"2a6ea821-7be7-41c8-b5a6-5508c5a5aef4\",\"accountId\":\"5f6de37b-621b-4790-8dbc-a18e0a12fa50\",\"status\":\"inProgress\",\"details\":{\"paymentInstrumentId\":\"0QVThwAAAAAEAACA\"}}", "Pending", "xbox")]
        [DataRow("{\"id\":\"2a6ea821-7be7-41c8-b5a6-5508c5a5aef4\",\"accountId\":\"5f6de37b-621b-4790-8dbc-a18e0a12fa50\",\"status\":\"created\",\"details\":{\"paymentInstrumentId\":\"0QVThwAAAAAEAACA\"}}", "Pending", "xbox")]
        [DataRow("{\"id\":\"2a6ea821-7be7-41c8-b5a6-5508c5a5aef4\",\"accountId\":\"5f6de37b-621b-4790-8dbc-a18e0a12fa50\",\"status\":\"expired\",\"details\":{\"paymentInstrumentId\":\"0QVThwAAAAAEAACA\"}}", "Declined", "xbox")]
        [DataRow("{\"id\":\"2a6ea821-7be7-41c8-b5a6-5508c5a5aef4\",\"accountId\":\"5f6de37b-621b-4790-8dbc-a18e0a12fa50\",\"status\":\"failed\",\"details\":{\"paymentInstrumentId\":\"0QVThwAAAAAEAACA\"}}", "Declined", "xbox")]
        [DataRow("{\"id\":\"2a6ea821-7be7-41c8-b5a6-5508c5a5aef4\",\"accountId\":\"5f6de37b-621b-4790-8dbc-a18e0a12fa50\",\"status\":\"inProgress\",\"details\":{\"paymentInstrumentId\":\"0QVThwAAAAAEAACA\"}}", "Pending", "amcxbox")]
        [DataRow("{\"id\":\"2a6ea821-7be7-41c8-b5a6-5508c5a5aef4\",\"accountId\":\"5f6de37b-621b-4790-8dbc-a18e0a12fa50\",\"status\":\"created\",\"details\":{\"paymentInstrumentId\":\"0QVThwAAAAAEAACA\"}}", "Pending", "amcxbox")]
        [DataRow("{\"id\":\"2a6ea821-7be7-41c8-b5a6-5508c5a5aef4\",\"accountId\":\"5f6de37b-621b-4790-8dbc-a18e0a12fa50\",\"status\":\"expired\",\"details\":{\"paymentInstrumentId\":\"0QVThwAAAAAEAACA\"}}", "Declined", "amcxbox")]
        [DataRow("{\"id\":\"2a6ea821-7be7-41c8-b5a6-5508c5a5aef4\",\"accountId\":\"5f6de37b-621b-4790-8dbc-a18e0a12fa50\",\"status\":\"failed\",\"details\":{\"paymentInstrumentId\":\"0QVThwAAAAAEAACA\"}}", "Declined", "amcxbox")]
        [DataRow("{\"id\":\"2a6ea821-7be7-41c8-b5a6-5508c5a5aef4\",\"accountId\":\"5f6de37b-621b-4790-8dbc-a18e0a12fa50\",\"status\":\"inProgress\",\"details\":{\"paymentInstrumentId\":\"0QVThwAAAAAEAACA\"}}", "Pending", "webblends")]
        [DataRow("{\"id\":\"2a6ea821-7be7-41c8-b5a6-5508c5a5aef4\",\"accountId\":\"5f6de37b-621b-4790-8dbc-a18e0a12fa50\",\"status\":\"created\",\"details\":{\"paymentInstrumentId\":\"0QVThwAAAAAEAACA\"}}", "Pending", "webblends")]
        [DataRow("{\"id\":\"2a6ea821-7be7-41c8-b5a6-5508c5a5aef4\",\"accountId\":\"5f6de37b-621b-4790-8dbc-a18e0a12fa50\",\"status\":\"expired\",\"details\":{\"paymentInstrumentId\":\"0QVThwAAAAAEAACA\"}}", "Declined", "webblends")]
        [DataRow("{\"id\":\"2a6ea821-7be7-41c8-b5a6-5508c5a5aef4\",\"accountId\":\"5f6de37b-621b-4790-8dbc-a18e0a12fa50\",\"status\":\"failed\",\"details\":{\"paymentInstrumentId\":\"0QVThwAAAAAEAACA\"}}", "Declined", "webblends")]
        [DataTestMethod]
        public async Task GetModernPi_India3DS_WhenSessionNotInSuccessStatus(string pimsSessionDetails, string expectedStatusReturned, string partner)
        {
            PXSettings.PimsService.ArrangeResponse(pimsSessionDetails);
            HttpResponseMessage result = await PXClient.GetAsync($"/v7.0/Account001/paymentInstrumentsEx/testPiid?language=en-US&partner={partner}&country=in&sessionQueryUrl=sessions%2F64a5681d-3baf-c570-5f9f-7f569201fd9d&scenario=threedsonepolling");
            var india3DSPollingResponse = await result.Content.ReadAsStringAsync();
            Assert.IsTrue(india3DSPollingResponse.Contains(expectedStatusReturned), "Polling doesn't return expected pi status based on session status");
        }

        [DataRow("{\"id\":\"2a6ea821-7be7-41c8-b5a6-5508c5a5aef4\",\"accountId\":\"5f6de37b-621b-4790-8dbc-a18e0a12fa50\",\"status\":\"inProgress\",\"details\":{\"paymentInstrumentId\":\"0QVThwAAAAAEAACA\"}}", "Pending")]
        [DataRow("{\"id\":\"2a6ea821-7be7-41c8-b5a6-5508c5a5aef4\",\"accountId\":\"5f6de37b-621b-4790-8dbc-a18e0a12fa50\",\"status\":\"created\",\"details\":{\"paymentInstrumentId\":\"0QVThwAAAAAEAACA\"}}", "Pending")]
        [DataRow("{\"id\":\"2a6ea821-7be7-41c8-b5a6-5508c5a5aef4\",\"accountId\":\"5f6de37b-621b-4790-8dbc-a18e0a12fa50\",\"status\":\"expired\",\"details\":{\"paymentInstrumentId\":\"0QVThwAAAAAEAACA\"}}", "Declined")]
        [DataRow("{\"id\":\"2a6ea821-7be7-41c8-b5a6-5508c5a5aef4\",\"accountId\":\"5f6de37b-621b-4790-8dbc-a18e0a12fa50\",\"status\":\"failed\",\"details\":{\"paymentInstrumentId\":\"0QVThwAAAAAEAACA\"}}", "Declined")]
        [DataTestMethod]
        public async Task GetMordernPi_PayPal2ndScreenPolling_WhenSessionNotInSuccessStatus(string pimsSessionDetails, string expectedStatusReturned)
        {
            PXSettings.PimsService.ArrangeResponse(pimsSessionDetails);
            HttpResponseMessage result = await PXClient.GetAsync($"/v7.0/Account001/paymentInstrumentsEx/testPiid?language=en-US&partner=xbox&country=us&sessionQueryUrl=sessions%2F64a5681d-3baf-c570-5f9f-7f569201fd9d&scenario=paypalqrcode");
            var paypal2ndScreenPollingResponse = await result.Content.ReadAsStringAsync();
            Assert.IsTrue(paypal2ndScreenPollingResponse.Contains(expectedStatusReturned), "Polling doesn't return expected pi status based on session status");
        }

        [DataRow("xbox", "paypalQrCode", "us", true)]
        [DataRow("xbox", "paypalQrCode", "fr", false)]
        [DataRow("amcxbox", "paypalQrCode", "us", true)]
        [DataRow("amcxbox", "paypalQrCode", "gb", false)]
        [DataRow("xboxsettings", "paypalQrCode", "us", true)]
        [DataRow("xboxsettings", "paypalQrCode", "de", false)]
        [DataTestMethod]
        public async Task GetMordernPi_PayPal2ndScreenPolling_WithFlighting(string partner, string scenario, string country, bool isFlightEnabled)
        {
            if (isFlightEnabled)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnablePaypalSecondScreenForXbox");
            }

            var sessionId = "f634e160-96b2-a926-1c4b-86eda833cf27";
            var pimsRequestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "paypal",
                sessionId = sessionId,
                details = new
                {
                    address = new
                    {
                        address_line1 = "One Microsoft Way",
                        city = "Redmond",
                        region = "WA",
                        country = "US",
                        postal_code = "11111"
                    },
                },
                riskData = new
                {
                }
            };

            bool assertCalled = false;

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi009-PaypalRedirect");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXSettings.PimsService.PreProcess = async (request) =>
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(requestContent);
                Assert.IsTrue(JObject.DeepEquals(json, JObject.FromObject(pimsRequestBody)), "Expected request data is not found");
                assertCalled = true;
            };
            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}&scenario={scenario}", new StringContent(JsonConvert.SerializeObject(pimsRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            Assert.IsTrue(assertCalled, "PimsTestHandler.PreProcess wasn't called");
            string resultContent = await result.Content.ReadAsStringAsync();

            Assert.IsTrue(resultContent.Contains(sessionId), "sessionId not found");

            // Assert (continuation)
            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK);
            PXSettings.PimsService.ResetToDefaults();
        }

        [DataRow("xboxsettings", "venmoQrCode", "us", ClientActionType.Pidl)]
        [DataRow("storify", "venmoQrCode", "us", ClientActionType.Pidl)]
        [DataRow("webblends", null, "us", ClientActionType.Redirect)]
        [DataRow("defaulttemplate", null, "us", ClientActionType.Redirect)]
        [DataRow("officesmb", null, "us", ClientActionType.Redirect)]
        [DataRow("onepage", null, "us", ClientActionType.Redirect)]
        [DataRow("twopage", null, "us", ClientActionType.Redirect)]
        [DataTestMethod]
        public async Task GetModernPi_VenmoPolling(string partner, string scenario, string country, ClientActionType expectedActionType)
        {
            var sessionId = "f634e160-96b2-a926-1c4b-86eda833cf27";
            var pimsRequestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "venmo",
                sessionId = sessionId,
                details = new
                {
                    address = new
                    {
                        address_line1 = "One Microsoft Way",
                        city = "Redmond",
                        region = "WA",
                        country = "US",
                        postal_code = "11111"
                    },
                },
                riskData = new
                {
                }
            };

            bool assertCalled = false;

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi015-VenmoRedirect");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXSettings.PimsService.PreProcess = async (request) =>
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(requestContent);
                Assert.IsTrue(JObject.DeepEquals(json, JObject.FromObject(pimsRequestBody)), "Expected request data is not found");
                assertCalled = true;
            };

            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}&scenario={scenario}", new StringContent(JsonConvert.SerializeObject(pimsRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            Assert.IsTrue(assertCalled, "PimsTestHandler.PreProcess wasn't called");
            string resultContent = await result.Content.ReadAsStringAsync();

            PIDLResource resource = ReadSinglePidlResourceFromJson(resultContent);
            ClientAction clientAction = resource.ClientAction;

            Assert.IsNotNull(clientAction);
            Assert.IsNotNull(clientAction.Context);
            Assert.AreEqual(expectedActionType, clientAction.ActionType);

            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK);

            List<PIDLResource> resourceList = null;

            if (expectedActionType == ClientActionType.Pidl)
            {
                resourceList = clientAction.Context as List<PIDLResource>;

                Assert.IsTrue(resultContent.Contains("venmoQrCodeChallengeText"), "No challenge text found");
                Assert.IsTrue(resultContent.Contains("venmoQrCodeImage"), "No image hint id found");
            }
            else if (expectedActionType == ClientActionType.Redirect)
            {
                resourceList = clientAction.RedirectPidl as List<PIDLResource>;
            }

            PIDLResource contextPidl = resourceList[0];

            Assert.IsNotNull(resourceList);
            Assert.IsNotNull(contextPidl);

            DisplayHintAction action = contextPidl.DisplayPages[0].Action;

            Assert.AreEqual("poll", action.ActionType);
            Assert.IsNotNull(action.Context);

            Assert.IsTrue(resultContent.Contains("https://{pifd-endpoint}/users/{userId}/paymentInstrumentsEx/Account001-Pi015-VenmoRedirect?"), "No redirect url found");
            PXSettings.PimsService.ResetToDefaults();
        }

        [DataRow("{\"id\":\"2a6ea821-7be7-41c8-b5a6-5508c5a5aef4\",\"accountId\":\"5f6de37b-621b-4790-8dbc-a18e0a12fa50\",\"status\":\"failed\",\"details\":{\"statusCode\":400,\"error\":{\"ErrorMsg\":\"Not approved\"}}}", "PimsSessionFailed")]
        [DataRow("{\"id\":\"2a6ea821-7be7-41c8-b5a6-5508c5a5aef4\",\"accountId\":\"5f6de37b-621b-4790-8dbc-a18e0a12fa50\",\"status\":\"failed\",\"details\":{\"statusCode\":400,\"error\":{\"ErrorCode\":\"TransactionNotApproved\"}}}", "TransactionNotApproved")]
        [DataTestMethod]
        public async Task GetMordernPi_SessionQueryUrl_WhenSessionNotInSuccessStatus(string pimsSessionDetails, string expectedStatusReturned)
        {
            PXSettings.PimsService.ArrangeResponse(pimsSessionDetails);
            HttpResponseMessage result = await PXClient.GetAsync($"/v7.0/Account001/paymentInstrumentsEx/testPiid?language=en-US&partner=azure&country=in&sessionQueryUrl=sessions%2F64a5681d-3baf-c570-5f9f-7f569201fd9d");
            var getPIResponse = await result.Content.ReadAsStringAsync();

            var error = JsonConvert.DeserializeObject<ErrorResponse>(await result.Content.ReadAsStringAsync());
            Assert.AreEqual(expectedStatusReturned, error.ErrorCode, "GetPI doesn't return expected error code");
        }

        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_ReplaceWithSamePiid()
        {
            // Arrange
            bool pimsReplaceCalled = false;
            var emptyRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa"
            };

            var piId = "Account001-Pi002-MC";
            var piPayload = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi002-MC");

            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(piPayload));
            PXSettings.OrchestrationService.PreProcess = (request) =>
            {
                if (request.RequestUri.AbsolutePath.Contains($"{piId}/replace"))
                {
                    pimsReplaceCalled = true;
                }
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx/{piId}/replace?country=us&language=en-US&partner=webblends", new StringContent(JsonConvert.SerializeObject(emptyRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK);
            Assert.IsFalse(pimsReplaceCalled, "Pims Replace API was called even though current and new PIIds were the same.");
            PXSettings.OrchestrationService.ResetToDefaults();
        }

        [DataRow("amcweb", false)]
        [DataRow("officesmb", true)]
        [TestMethod]
        public async Task PaymentInstrumentsEx_Redeem_StartFundStoredValue(string partner, bool usePartnerSetting)
        {
            // Arrange
            var requestBody = new
            {
                amount = "10"
            };

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-test", "{ \"scenarios\": \"px.local\" }"
                },
                {
                    "x-ms-msaprofile", "PUID=985160615739993"
                }
            };

            if (usePartnerSetting)
            {
                headers["x-ms-flight"] = "PXDisablePSSCache";

                string partnerSettingResponse = "{\"fundstoredvalue\":{\"template\":\"defaulttemplate\",\"features\":null}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
            }

            // Act
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx/F2D44338-A605-4A7E-AA50-18B0B2B1E967/redeem?language=en-us&country=us&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };
            headers?.ToList().ForEach(header => request.Headers.Add(header.Key, header.Value));

            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            var redeemAmountClientAction = ReadSinglePidlResourceFromJson(await result.Content.ReadAsStringAsync());
            Assert.IsNotNull(redeemAmountClientAction.ClientAction);
            Assert.AreEqual(ClientActionType.Pidl, redeemAmountClientAction.ClientAction.ActionType);
            Assert.IsNotNull(redeemAmountClientAction.ClientAction.Context);

            var redeemAmountPidls = redeemAmountClientAction.ClientAction.Context as List<PIDLResource>;
            Assert.AreEqual(1, redeemAmountPidls.Count);

            var redeemAmountPidl = redeemAmountPidls[0];
            Assert.AreEqual(1, redeemAmountPidl.DisplayPages.Count);
            Assert.AreEqual(5, redeemAmountPidl.DisplayPages[0].Members.Count);

            var iframeElement = redeemAmountPidl.DisplayPages[0].Members[4] as IFrameDisplayHint;
            Assert.IsNotNull(iframeElement);
            Assert.AreEqual("https://bitpay.com/invoice?id=PujzFdsApS3EymrK5BzZbo&view=iframe&lang=en-us", iframeElement.SourceUrl);

            var pollingAction = iframeElement.Action;
            var context = JsonConvert.DeserializeObject<RestLink>(JsonConvert.SerializeObject(pollingAction.Context));
            Assert.IsNotNull(context);
            Assert.IsTrue(context.Href.Contains("redeem"));
            Assert.IsTrue(context.Href.Contains("referenceId=ce9d0625-15ae-4b5e-9151-510cea66a431"));
        }

        [DataRow(false)]
        [DataRow(true)]
        [TestMethod]
        public async Task PaymentInstrumentsEx_AnonymousResumePendingOperation(bool isSuccessful)
        {
            // Act
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/paymentInstrumentsEx/q62zBAAAAAAJAACA/resume?country=in&language=en-US&partner=webblends&isSuccessful={isSuccessful}&sessionQueryUrl=sessions/5273fbca-829b-4acf-8c9e-dbb781261b0b")),
                Method = HttpMethod.Get
            };

            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            var resumeClientActionPostMessage = await result.Content.ReadAsStringAsync();
            Assert.IsNotNull(resumeClientActionPostMessage);

            if (isSuccessful)
            {
                // Clinet action in post message should be "Pidl" type if bank verification is successfully
                Assert.IsTrue(resumeClientActionPostMessage.Contains("window.parent.postMessage(\"{\\\"type\\\":\\\"Pidl\\\""));

                // Verify button submit link
                Assert.IsTrue(resumeClientActionPostMessage.Contains("\\\"href\\\":\\\"https://{pifd-endpoint}/users/{userId}/paymentInstrumentsEx/q62zBAAAAAAJAACA?language=en-US\\u0026partner=webblends\\u0026country=in\\u0026scenario=threedsonepolling\\u0026sessionQueryUrl=sessions%2F5273fbca-829b-4acf-8c9e-dbb781261b0b"));
            }
            else
            {
                // Client action in post message should be "Failure" type if bank verification is not succeesfully
                Assert.IsTrue(resumeClientActionPostMessage.Contains("<html><script>window.parent.postMessage(\"{\\\"type\\\":\\\"Failure\\\",\\\"context\\\":{\\\"CorrelationId\\\":null,\\\"ErrorCode\\\":\\\"BadRequest\\\",\\\"Message\\\":\\\"Resume add PI failed\\\",\\\"UserDisplayMessage\\\":null,\\\"Source\\\":null,\\\"Target\\\":null,\\\"clientAction\\\":null,\\\"Details\\\":null,\\\"InnerError\\\":{\\\"CorrelationId\\\":null,\\\"ErrorCode\\\":\\\"ThreeDSOneResumeAddPiFailed\\\",\\\"Message\\\":\\\"Resume add PI failed\\\",\\\"UserDisplayMessage\\\":null,\\\"Source\\\":null,\\\"Target\\\":null,\\\"clientAction\\\":null,\\\"Details\\\":null,\\\"InnerError\\\":null}},\\\"redirectPidl\\\":null,\\\"actionId\\\":\\\"5273fbca-829b-4acf-8c9e-dbb781261b0b\\\",\\\"pidlRetainUserInput\\\":null,\\\"pidlUserInputToClear\\\":null,\\\"pidlError\\\":null,\\\"nextAction\\\":null}\", \"*\");</script><body/></html>"));
            }
        }

        [DataRow("amcweb", false)]
        [DataRow("officesmb", true)]
        [TestMethod]
        public async Task PaymentInstrumentsEx_Redeem_NewMsa_StartFundStoredValue(string partner, bool usePartnerSetting)
        {
            // Arrange
            var requestBody = new
            {
                amount = "10"
            };

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-test", "{ \"scenarios\": \"px.local\" }"
                },
                {
                    "x-ms-msaprofile", "PUID=PuidWithNoLegacyBillableAccounts"
                }
            };

            if (usePartnerSetting)
            {
                headers["x-ms-flight"] = "PXDisablePSSCache";

                string partnerSettingResponse = "{\"fundstoredvalue\":{\"template\":\"defaulttemplate\",\"features\":null}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
            }

            var accountsServiceGetOrCreateLBACalled = false;
            PXSettings.AccountsService.PreProcess = (accountsServiceRequest) =>
            {
                if (accountsServiceRequest != null && accountsServiceRequest.RequestUri.AbsolutePath.Contains("get-or-create-legacy-billable-account"))
                {
                    accountsServiceGetOrCreateLBACalled = true;
                }
            };

            PXSettings.CommerceAccountDataService.PreProcessGetAccountInfo = (getAccountInfoRequest) =>
            {
                if (accountsServiceGetOrCreateLBACalled)
                {
                    getAccountInfoRequest.Requester.IdentityValue = "PuidWithLegacyBillableAccounts";
                }
            };

            // Act
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx/F2D44338-A605-4A7E-AA50-18B0B2B1E967/redeem?language=en-us&country=us&partner=amcweb")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };
            headers?.ToList().ForEach(header => request.Headers.Add(header.Key, header.Value));

            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert
            Assert.IsTrue(accountsServiceGetOrCreateLBACalled, "Accounts Service Get-Or-Create-Legacy-Billable-Account needs to be called.");

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            var redeemAmountClientAction = ReadSinglePidlResourceFromJson(await result.Content.ReadAsStringAsync());
            var redeemAmountPidls = redeemAmountClientAction.ClientAction.Context as List<PIDLResource>;
            var iframeElement = redeemAmountPidls[0].DisplayPages[0].Members[4] as IFrameDisplayHint;
            Assert.IsNotNull(iframeElement);
            Assert.AreEqual("https://bitpay.com/invoice?id=PujzFdsApS3EymrK5BzZbo&view=iframe&lang=en-us", iframeElement.SourceUrl);
            PXSettings.AccountsService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ResetToDefaults();
        }

        [DataRow("amcweb")]
        [DataRow("officesmb")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_Redeem_CheckFundStoredValue(string partner)
        {
            // Arrange
            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-test", "{ \"scenarios\": \"px.local\" }"
                },
                {
                    "x-ms-msaprofile", "PUID=985160615739993"
                }
            };

            // Act
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx/F2D44338-A605-4A7E-AA50-18B0B2B1E967/redeem?language=en-us&partner={partner}&country=us&amount=10&currency=USD&referenceId=ce9d0625-15ae-4b5e-9151-510cea66a431&greenId=greenId-15ae-4b5e-9151-510cea66a431")),
                Method = HttpMethod.Get
            };
            headers?.ToList().ForEach(header => request.Headers.Add(header.Key, header.Value));

            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual("{\"status\":\"completed\"}", await result.Content.ReadAsStringAsync());
        }

        [DataRow("xboxweb", "TestChannel", "TestReferrer", "null")]
        [DataRow("xboxweb", "TestChannel", "null", "null")]
        [DataRow("xboxweb", "TestChannel", "TestReferrer", "TestSession")]
        [DataRow("xboxweb", "null", "null", "TestSession")]
        [DataTestMethod]
        public async Task ApplyPaymentInstrument_WebPartners_Success(string partner, string channel, string referrerId, string sessionId)
        {
            // Arrange
            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-msaprofile", "PUID=985160615739993"
                }
            };

            var payload = new InitializeRequest()
            {
                Card = "XboxCreditCard",
                Market = "us",
                Channel = channel,
                ReferrerId = referrerId
            };

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/paymentInstrumentsEx?operation=apply&country=us&language=en-us&partner={partner}&sessionId={sessionId}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            headers?.ToList().ForEach(header => request.Headers.Add(header.Key, header.Value));

            // Act
            HttpResponseMessage result = await PXClient.SendAsync(request);
            var pidlResource = ReadSinglePidlResourceFromJson(await result.Content.ReadAsStringAsync());
            Assert.IsNotNull(pidlResource.ClientAction, "Client action missing");
            Assert.AreEqual(ClientActionType.Redirect, pidlResource.ClientAction.ActionType);
            Assert.IsNotNull(pidlResource.ClientAction.Context, "Client action context missing");

            var clientAction = pidlResource.ClientAction as ClientAction;
            Assert.IsTrue(clientAction.Context.ToString().Contains("https://mockRedirectUrl.com"), "base url is not expected");
            Assert.IsNull(clientAction.RedirectPidl, "redirectPidl should be null for inline partners");
        }

        [DataRow("xboxweb", "TestChannel", "TestReferrer", "TestSession")]
        [DataTestMethod]
        public async Task ApplyPaymentInstrument_WebPartners_Failure_NoPuid(string partner, string channel, string referrerId, string sessionId)
        {
            // Arrange
            var payload = new InitializeRequest()
            {
                Card = "XboxCreditCard",
                Market = "us",
                Channel = channel,
                ReferrerId = referrerId
            };

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/paymentInstrumentsEx?operation=apply&country=us&language=en-us&partner={partner}&sessionId={sessionId}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            HttpResponseMessage result = await PXClient.SendAsync(request);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [DataRow("xboxweb", null, "XboxCreditCard", "us")]
        [DataRow("xboxweb", "testChannel", null, "us")]
        [DataRow("xboxweb", "testChannel", "XboxCreditCard", null)]
        [DataTestMethod]
        public async Task ApplyPaymentInstrument_WebPartners_Failure_MissingParams(string partner, string channel, string card, string market)
        {
            // Arrange
            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-msaprofile", "PUID=985160615739993"
                }
            };

            var payload = new InitializeRequest()
            {
                Card = card,
                Market = market,
                Channel = channel
            };

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/paymentInstrumentsEx?operation=apply&country=us&language=en-us&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            headers?.ToList().ForEach(header => request.Headers.Add(header.Key, header.Value));

            // Act
            HttpResponseMessage result = await PXClient.SendAsync(request);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [DataRow("xboxweb")]
        [DataTestMethod]
        public async Task ApplyPaymentInstrument_WebPartners_Failure_BadInitialize(string partner)
        {
            // Arrange
            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-msaprofile", "PUID=985160615739993"
                }
            };

            var payload = new InitializeRequest()
            {
                Card = "XboxCreditCard",
                Market = "us",
                Channel = "testChannel",
                ReferrerId = "testReferrer"
            };

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/paymentInstrumentsEx?operation=apply&country=us&language=en-us&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            headers?.ToList().ForEach(header => request.Headers.Add(header.Key, header.Value));

            string mockResponse = $"{{\"ErrorCode\": 500, \"Message\": \"Test Error\"}}";
            PXSettings.IssuerService.ResetToDefaults();
            PXSettings.IssuerService.ArrangeResponse(mockResponse, statusCode: HttpStatusCode.InternalServerError, urlPattern: ".*/session");

            // Act
            HttpResponseMessage result = await PXClient.SendAsync(request);
            Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);

            // Reset
            PXSettings.IssuerService.ResetToDefaults();
        }

        [DataRow("xboxweb")]
        [DataTestMethod]
        public async Task ApplyPaymentInstrument_WebPartners_Failure_BadApply(string partner)
        {
            // Arrange
            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-msaprofile", "PUID=985160615739993"
                }
            };

            var payload = new InitializeRequest()
            {
                Card = "XboxCreditCard",
                Market = "us",
                Channel = "BANNER",
                ReferrerId = "MSFT-Web_Lpage"
            };

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/paymentInstrumentsEx?operation=apply&country=us&language=en-us&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            headers?.ToList().ForEach(header => request.Headers.Add(header.Key, header.Value));

            string mockResponse = $"{{\"ErrorCode\": 404, \"Message\": \"Test Error\"}}";
            PXSettings.IssuerService.ResetToDefaults();
            PXSettings.IssuerService.ArrangeResponse(mockResponse, statusCode: HttpStatusCode.NotFound, urlPattern: "applications/985160615739993");

            // Act
            HttpResponseMessage result = await PXClient.SendAsync(request);
            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            // Reset
            PXSettings.IssuerService.ResetToDefaults();
        }

        private string GetQrCodeFromUriHelper(string uri)
        {
            QRCodeGenerator generator = new QRCodeGenerator();
            QRCodeData dataQR = generator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
            Base64QRCode codeQR = new Base64QRCode(dataQR);
            string base64Id = codeQR.GetGraphic(10);
            return "data:image/png;base64," + base64Id;
        }

        [DataRow("storify", "XboxCreditCard", "Backend", "Browser", "US", "PXXboxCardApplicationEnableShortUrl")]
        [DataRow("storify", "XboxCreditCard", "Backend", "Browser", "US", "PXXboxCardApplicationEnableWebview,PXXboxCardApplicationEnableShortUrl")]
        [DataTestMethod]
        public async Task ApplyPaymentInstrument_XboxCoBrandedCardQrFlow(string partner, string cardProduct, string channel, string referrerId, string market, string flight)
        {
            // Arrange
            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-test", "{ \"scenarios\": \"px.issuerservice.default\" }"
                },
                {
                    "x-ms-msaprofile", "PUID=985160615739993"
                }
            };

            if (flight != null)
            {
                headers.Add("x-ms-flight", flight);
            }

            var applyPayload = new ApplyRequest()
            {
                SessionId = Guid.NewGuid().ToString()
            };

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/paymentInstrumentsEx?&language=en-us&partner={partner}&operation=apply&sessionId=57fa10b2-2ca2-4f4e-8817-9c19379b1c29&country=US&ocid=sample")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(applyPayload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            headers?.ToList().ForEach(header => request.Headers.Add(header.Key, header.Value));

            // Act
            HttpResponseMessage result = await PXClient.SendAsync(request);
            string resultContent = await result.Content.ReadAsStringAsync();

            PIDLResource resource = ReadSinglePidlResourceFromJson(resultContent);
            ClientAction clientAction = resource.ClientAction;
            var pidls = clientAction.Context as List<PIDLResource>;

            // verify the PIDL elements don't have style hints when PXEnableApplyPIXboxNativeStyleHints flight is disabled
            foreach (PIDLResource pidlResource in pidls)
            {
                foreach (DisplayHint displayHint in pidlResource.GetAllDisplayHints())
                {
                    PropertyDisplayHint propertyDisplayHint = displayHint as PropertyDisplayHint;

                    Assert.IsNull(displayHint.StyleHints);
                    if (propertyDisplayHint?.PossibleOptions != null)
                    {
                        foreach (var option in propertyDisplayHint.PossibleOptions)
                        {
                            Assert.IsNull(option.Value.StyleHints);
                        }
                    }
                }
            }

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsNotNull(resultContent, "Pidl is expected to be not null");
            Assert.IsTrue(resultContent.Contains("xboxCoBrandedCardIframe"));
            Assert.IsTrue(resultContent.Contains($"\"useAuth\":true"));
            Assert.IsTrue(resultContent.Contains("xboxCoBrandedCardQrCodeImage"));
            Assert.IsTrue(resultContent.Contains("/RedirectionService/CoreRedirection/Query/57fa10b2-2ca2-4f4e-8817-9c19379b1c29"));
            Assert.IsTrue(resultContent.Contains($"https://{{pifd-endpoint}}/users/{{userId}}/paymentInstrumentsEx/notApplicable?partner={partner}&country=US&sessionId=57fa10b2-2ca2-4f4e-8817-9c19379b1c29&scenario=xboxCoBrandedCard"));
            Assert.IsTrue(resultContent.Contains(GetQrCodeFromUriHelper("https://testshorturl.ms/test")));

            if (flight.Contains("PXXboxCardApplicationEnableWebview"))
            {
                Assert.IsTrue(resultContent.Contains("\"xboxCoBrandedCardQrCodeRedirectButton\",\"displayType\":\"button\",\"pidlAction\":{\"type\":\"updatePollAndMoveLast\",\"context\":\"xboxCoBrandedCardQrCodePage\",\"dest\":\"xboxCoBrandedCardQrCodePage3\""));
                Assert.IsTrue(resultContent.Contains("Hi, we're processing your request\\nThis might take a moment"));
            }
            else
            {
                Assert.IsTrue(resultContent.Contains("\"xboxCoBrandedCardQrCodeRedirectButton\",\"displayType\":\"button\",\"pidlAction\":{\"type\":\"navigate\",\"context\":\"https://www.xbox.com/en-US/xbox-mastercard/apply?channelname=&referrerid=&consoleapplysessionid=57fa10b2-2ca2-4f4e-8817-9c19379b1c29&isconsolewebview=true&ocid=sample\",\"dest\":\"applyOnConsole\""));
            }
        }

        [DataRow("storify", "XboxCreditCard", "Backend", "Browser", "US", "PXXboxCardApplicationEnableShortUrl,PXEnableApplyPIXboxNativeStyleHints")]
        [DataRow("storify", "XboxCreditCard", "Backend", "Browser", "US", "PXXboxCardApplicationEnableWebview,PXXboxCardApplicationEnableShortUrl,PXEnableApplyPIXboxNativeStyleHints")]
        [DataTestMethod]
        public async Task ApplyPaymentInstrument_XboxCoBrandedCardQrFlow_withStyleHints(string partner, string cardProduct, string channel, string referrerId, string market, string flight)
        {
            // Arrange
            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-test", "{ \"scenarios\": \"px.issuerservice.default\" }"
                },
                {
                    "x-ms-msaprofile", "PUID=985160615739993"
                }
            };

            if (flight != null)
            {
                headers.Add("x-ms-flight", flight);
            }

            var applyPayload = new ApplyRequest()
            {
                SessionId = Guid.NewGuid().ToString()
            };

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/paymentInstrumentsEx?&language=en-us&partner={partner}&operation=apply&sessionId=57fa10b2-2ca2-4f4e-8817-9c19379b1c29&country=US&ocid=sample")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(applyPayload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            headers?.ToList().ForEach(header => request.Headers.Add(header.Key, header.Value));

            // Act
            HttpResponseMessage result = await PXClient.SendAsync(request);
            string resultContent = await result.Content.ReadAsStringAsync();

            PIDLResource resource = ReadSinglePidlResourceFromJson(resultContent);
            ClientAction clientAction = resource.ClientAction;
            var pidls = clientAction.Context as List<PIDLResource>;

            // verify the PIDL elements have style hints when PXEnableApplyPIXboxNativeStyleHints flight is enabled
            Assert.IsTrue(resultContent.Contains("styleHints"));

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsNotNull(resultContent, "Pidl is expected to be not null");
            Assert.IsTrue(resultContent.Contains("xboxCoBrandedCardIframe"));
            Assert.IsTrue(resultContent.Contains($"\"useAuth\":true"));
            Assert.IsTrue(resultContent.Contains("xboxCoBrandedCardQrCodeImage"));
            Assert.IsTrue(resultContent.Contains("/RedirectionService/CoreRedirection/Query/57fa10b2-2ca2-4f4e-8817-9c19379b1c29"));
            Assert.IsTrue(resultContent.Contains($"https://{{pifd-endpoint}}/users/{{userId}}/paymentInstrumentsEx/notApplicable?partner={partner}&country=US&sessionId=57fa10b2-2ca2-4f4e-8817-9c19379b1c29&scenario=xboxCoBrandedCard"));
            Assert.IsTrue(resultContent.Contains(GetQrCodeFromUriHelper("https://testshorturl.ms/test")));

            if (flight.Contains("PXXboxCardApplicationEnableWebview"))
            {
                Assert.IsTrue(resultContent.Contains("\"xboxCoBrandedCardQrCodeRedirectButton\",\"displayType\":\"button\",\"pidlAction\":{\"type\":\"updatePollAndMoveLast\",\"context\":\"xboxCoBrandedCardQrCodePage\",\"dest\":\"xboxCoBrandedCardQrCodePage3\""));
                Assert.IsTrue(resultContent.Contains("Hi, we're processing your request\\nThis might take a moment"));
            }
            else
            {
                Assert.IsTrue(resultContent.Contains("\"xboxCoBrandedCardQrCodeRedirectButton\",\"displayType\":\"button\",\"pidlAction\":{\"type\":\"navigate\",\"context\":\"https://www.xbox.com/en-US/xbox-mastercard/apply?channelname=&referrerid=&consoleapplysessionid=57fa10b2-2ca2-4f4e-8817-9c19379b1c29&isconsolewebview=true&ocid=sample\",\"dest\":\"applyOnConsole\""));
            }
        }

        [DataRow("xboxcardapp", false)]
        [DataRow("xboxcardapp", true)]
        [DataTestMethod]
        public async Task ApplyPaymentInstrument_XboxCoBrandedCardQrFlow_ApplicationDetails_Error(string partner, bool enableStyleHints)
        {
            // Arrange
            string mockResponse = "[ {\"customerPuid\": \"sample\",\"issuerAccountId\": null,\"cardProduct\": \"XboxCreditCard\",\"channel\": \"BANNER\",\"subchannel\": null,\"market\": \"US\",\"paymentInstrumentId\": null,\"issuerCustomerId\": \"sample\",\"lastFourDigits\": null,\"expiryDate\": null,\"sessionId\": \"sample\",\"status\": \"Error\",\"errorDetails\": {  \"errorCode\": 4,  \"errorTitle\": \"keyId should not be null or white space\",  \"errorDetail\": \"Sample error details\"},\"createDate\": \"2023-09-07T20: 00: 31Z\",\"modifiedDate\": \"2023-09-07T20: 02: 01Z\" }]";
            PXSettings.IssuerService.ResetToDefaults();
            PXSettings.IssuerService.ArrangeResponse(mockResponse);

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-msaprofile", "PUID=985160615739993"
                }
            };
            if (enableStyleHints)
            {
                headers["x-ms-flight"] = "PXEnableApplyPIXboxNativeStyleHints";
            }

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx/notApplicable?partner={partner}&country=US&sessionId=57fa10b2-2ca2-4f4e-8817-9c19379b1c29&scenario=xboxCoBrandedCard")),
                Method = HttpMethod.Get,
            };

            headers?.ToList().ForEach(header => request.Headers.Add(header.Key, header.Value));

            // Act
            HttpResponseMessage result = await PXClient.SendAsync(request);
            string resultContent = await result.Content.ReadAsStringAsync();

            PIDLResource resource = ReadSinglePidlResourceFromJson(resultContent);
            ClientAction clientAction = resource.ClientAction;
            var pidls = clientAction.Context as List<PIDLResource>;

            if (enableStyleHints)
            {
                // verify the PIDL elements have style hints when PXEnableApplyPIXboxNativeStyleHints flight is enabled
                Assert.IsTrue(resultContent.Contains("styleHints"));
            }
            else
            {
                // verify the PIDL elements don't have style hints when PXEnableApplyPIXboxNativeStyleHints flight is disabled
                foreach (PIDLResource pidlResource in pidls)
                {
                    foreach (DisplayHint displayHint in pidlResource.GetAllDisplayHints())
                    {
                        PropertyDisplayHint propertyDisplayHint = displayHint as PropertyDisplayHint;

                        Assert.IsNull(displayHint.StyleHints);
                        if (propertyDisplayHint?.PossibleOptions != null)
                        {
                            foreach (var option in propertyDisplayHint.PossibleOptions)
                            {
                                Assert.IsNull(option.Value.StyleHints);
                            }
                        }
                    }
                }
            }

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsTrue(resultContent.Contains("Something went wrong"));
        }

        [DataRow("xboxcardapp", "Approved", true, "xboxCardApplySuccess", "Success!", "xboxCardGoToStoreButton", "", "PXEnableApplyPIXboxNativeStyleHints")]
        [DataRow("xboxcardapp", "Approved", true, "xboxCardApplySuccess", "Success!", "xboxCardGoToStoreSuccessButton", "", "PXXboxCardApplyDisableStoreButtonNavigation,PXEnableApplyPIXboxNativeStyleHints")]
        [DataRow("xboxcardapp", "Approved", false, "xboxCardApplicationDetailsError", "Something went wrong", "xboxCardApplyInternalErrorBackButton", "", "PXEnableApplyPIXboxNativeStyleHints")]
        [DataRow("xboxcardapp", "PendingOnIssuer", true, "xboxCardApplicationDetailsPending", "Thanks for your application!", "xboxCoBrandedCardApplyPendingBackButton", "", "PXEnableApplyPIXboxNativeStyleHints")]
        [DataRow("xboxcardapp", "PendingOnApplication", true, "xboxCardApplicationDetailsPending", "Thanks for your application!", "xboxCoBrandedCardApplyPendingBackButton", "", "PXEnableApplyPIXboxNativeStyleHints")]
        [DataRow("xboxcardapp", "Error", true, "xboxCardApplicationDetailsError", "Something went wrong", "xboxCardApplyInternalErrorBackButton", "", "PXEnableApplyPIXboxNativeStyleHints")]
        [DataRow("storify", "Approved", true, "xboxCardApplySuccess", "Success!", "xboxCardGoToStoreButton", "", "PXEnableApplyPIXboxNativeStyleHints")]
        [DataRow("storify", "Approved", true, "xboxCardApplySuccess", "Success!", "xboxCardGoToStoreSuccessButton", "", "PXXboxCardApplyDisableStoreButtonNavigation,PXEnableApplyPIXboxNativeStyleHints")]
        [DataRow("storify", "Approved", false, "xboxCardApplicationDetailsError", "Something went wrong", "xboxCardApplyInternalErrorBackButton", "", "PXEnableApplyPIXboxNativeStyleHints")]
        [DataRow("storify", "PendingOnIssuer", true, "xboxCardApplicationDetailsPending", "Thanks for your application!", "xboxCoBrandedCardApplyPendingBackButton", "", "PXEnableApplyPIXboxNativeStyleHints")]
        [DataRow("storify", "PendingOnApplication", true, "xboxCardApplicationDetailsPending", "Thanks for your application!", "xboxCoBrandedCardApplyPendingBackButton", "", "PXEnableApplyPIXboxNativeStyleHints")]
        [DataRow("storify", "Error", true, "xboxCardApplicationDetailsError", "Something went wrong", "xboxCardApplyInternalErrorBackButton", "", "PXEnableApplyPIXboxNativeStyleHints")]
        [DataTestMethod]
        public async Task ApplyPaymentInstrument_XboxCoBrandedCardApplicationDetails(string partner, string applicationDetailsStatus, bool shouldPimsSucceed = true, string expectedPageDisplayId = "", string expectedTitle = "", string expectedButton1 = "", string expectedButton2 = "", string flight = "")
        {
            // Arrange
            string mockResponse = $"[{{\"sessionId\":\"57fa10b2-2ca2-4f4e-8817-9c19379b1c29\",\"customerPuid\":null,\"jarvisAccountId\":null,\"issuerCustomerId\":null,\"cardProduct\":null,\"channel\":null,\"subChannel\":null,\"market\":null,\"issuerAccountId\":null,\"lastFourDigits\":null,\"paymentInstrumentId\":\"q62zBAAAAAAJAACA\",\"status\":\"{applicationDetailsStatus}\",\"errorDetails\":null,\"createDate\":null,\"modifiedDate\":null}}]";
            PXSettings.IssuerService.ResetToDefaults();
            PXSettings.IssuerService.ArrangeResponse(mockResponse);

            var piid = "Account001-Pi002-MC";
            if (shouldPimsSucceed)
            {
                var accountId = "Account001";
                var piFromPims = PimsMockResponseProvider.GetPaymentInstrument(accountId, piid);
                PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(piFromPims));
            }

            var applyPayload = new ApplyRequest()
            {
                SessionId = Guid.NewGuid().ToString()
            };

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/bc81f231-268a-4b9f-897a-43b7397302cc/paymentInstrumentsEx/notApplicable?partner={partner}&country=US&sessionId=57fa10b2-2ca2-4f4e-8817-9c19379b1c29&scenario=xboxCoBrandedCard")),
                Method = HttpMethod.Get,
                Content = new StringContent(JsonConvert.SerializeObject(applyPayload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType),
                Headers =
                {
                    { "x-ms-msaprofile", "PUID=123456789" },
                    { "x-ms-flight", flight }
                }
            };

            // Act
            HttpResponseMessage result = await PXClient.SendAsync(request);
            string resultContent = await result.Content.ReadAsStringAsync();

            var response = JsonConvert.DeserializeObject<PaymentInstrument>(resultContent);

            // verify the PIDL elements have style hints when PXEnableApplyPIXboxNativeStyleHints flight is enabled
            Assert.IsTrue(resultContent.Contains("styleHints"));

            // Assert
            Assert.IsNotNull(resultContent, "result is expected to be not null");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsTrue(resultContent.Contains(expectedPageDisplayId));
            Assert.IsTrue(resultContent.Contains(expectedTitle));
            Assert.IsTrue(resultContent.Contains(expectedButton1));
            Assert.IsTrue(resultContent.Contains(expectedButton2));

            // Reset Issuer Service to default so other tests don't get the mocked response
            PXSettings.IssuerService.ResetToDefaults();
        }

        [DataRow("xboxcardapp", "Approved", true, "xboxCardApplySuccess", "Success!", "xboxCardGoToStoreButton")]
        [DataRow("xboxcardapp", "Approved", true, "xboxCardApplySuccess", "Success!", "xboxCardGoToStoreSuccessButton", "", "PXXboxCardApplyDisableStoreButtonNavigation")]
        [DataRow("xboxcardapp", "Approved", false, "xboxCardApplicationDetailsError", "Something went wrong", "xboxCardApplyInternalErrorBackButton")]
        [DataRow("xboxcardapp", "PendingOnIssuer", true, "xboxCardApplicationDetailsPending", "Thanks for your application!", "xboxCoBrandedCardApplyPendingBackButton")]
        [DataRow("xboxcardapp", "PendingOnApplication", true, "xboxCardApplicationDetailsPending", "Thanks for your application!", "xboxCoBrandedCardApplyPendingBackButton")]
        [DataRow("xboxcardapp", "Error", true, "xboxCardApplicationDetailsError", "Something went wrong", "xboxCardApplyInternalErrorBackButton")]
        [DataRow("storify", "Approved", true, "xboxCardApplySuccess", "Success!", "xboxCardGoToStoreButton")]
        [DataRow("storify", "Approved", true, "xboxCardApplySuccess", "Success!", "xboxCardGoToStoreSuccessButton", "", "PXXboxCardApplyDisableStoreButtonNavigation")]
        [DataRow("storify", "Approved", false, "xboxCardApplicationDetailsError", "Something went wrong", "xboxCardApplyInternalErrorBackButton")]
        [DataRow("storify", "PendingOnIssuer", true, "xboxCardApplicationDetailsPending", "Thanks for your application!", "xboxCoBrandedCardApplyPendingBackButton")]
        [DataRow("storify", "PendingOnApplication", true, "xboxCardApplicationDetailsPending", "Thanks for your application!", "xboxCoBrandedCardApplyPendingBackButton")]
        [DataRow("storify", "Error", true, "xboxCardApplicationDetailsError", "Something went wrong", "xboxCardApplyInternalErrorBackButton")]
        [DataTestMethod]
        public async Task ApplyPaymentInstrument_XboxCoBrandedCardApplicationDetails_WithStyleHints(string partner, string applicationDetailsStatus, bool shouldPimsSucceed = true, string expectedPageDisplayId = "", string expectedTitle = "", string expectedButton1 = "", string expectedButton2 = "", string flight = "")
        {
            // Arrange
            string mockResponse = $"[{{\"sessionId\":\"57fa10b2-2ca2-4f4e-8817-9c19379b1c29\",\"customerPuid\":null,\"jarvisAccountId\":null,\"issuerCustomerId\":null,\"cardProduct\":null,\"channel\":null,\"subChannel\":null,\"market\":null,\"issuerAccountId\":null,\"lastFourDigits\":null,\"paymentInstrumentId\":\"q62zBAAAAAAJAACA\",\"status\":\"{applicationDetailsStatus}\",\"errorDetails\":null,\"createDate\":null,\"modifiedDate\":null}}]";
            PXSettings.IssuerService.ResetToDefaults();
            PXSettings.IssuerService.ArrangeResponse(mockResponse);

            var piid = "Account001-Pi002-MC";
            if (shouldPimsSucceed)
            {
                var accountId = "Account001";
                var piFromPims = PimsMockResponseProvider.GetPaymentInstrument(accountId, piid);
                PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(piFromPims));
            }

            var applyPayload = new ApplyRequest()
            {
                SessionId = Guid.NewGuid().ToString()
            };

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/bc81f231-268a-4b9f-897a-43b7397302cc/paymentInstrumentsEx/notApplicable?partner={partner}&country=US&sessionId=57fa10b2-2ca2-4f4e-8817-9c19379b1c29&scenario=xboxCoBrandedCard")),
                Method = HttpMethod.Get,
                Content = new StringContent(JsonConvert.SerializeObject(applyPayload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType),
                Headers =
                {
                    { "x-ms-msaprofile", "PUID=123456789" },
                    { "x-ms-flight", flight }
                }
            };

            // Act
            HttpResponseMessage result = await PXClient.SendAsync(request);
            string resultContent = await result.Content.ReadAsStringAsync();

            var response = JsonConvert.DeserializeObject<PaymentInstrument>(resultContent);

            // verify the PIDL elements don't have style hints when PXEnableApplyPIXboxNativeStyleHints flight is disabled
            Assert.AreEqual(ClientActionType.Pidl.ToString(), response.ClientAction.ActionType.ToString());
            var pidls = ReadPidlResourceFromJson(JsonConvert.SerializeObject(response.ClientAction.Context));

            foreach (PIDLResource pidlResource in pidls)
            {
                foreach (DisplayHint displayHint in pidlResource.GetAllDisplayHints())
                {
                    PropertyDisplayHint propertyDisplayHint = displayHint as PropertyDisplayHint;

                    Assert.IsNull(displayHint.StyleHints);
                    if (propertyDisplayHint?.PossibleOptions != null)
                    {
                        foreach (var option in propertyDisplayHint.PossibleOptions)
                        {
                            Assert.IsNull(option.Value.StyleHints);
                        }
                    }
                }
            }

            // Assert
            Assert.IsNotNull(resultContent, "result is expected to be not null");
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsTrue(resultContent.Contains(expectedPageDisplayId));
            Assert.IsTrue(resultContent.Contains(expectedTitle));
            Assert.IsTrue(resultContent.Contains(expectedButton1));
            Assert.IsTrue(resultContent.Contains(expectedButton2));

            // Reset Issuer Service to default so other tests don't get the mocked response
            PXSettings.IssuerService.ResetToDefaults();
        }

        // Ignore this test for now because it fails at CDPx pipeline, the following task is created to track the fixation
        // Task 28999776: Fix failed CIT.PXService in CDPx pipeline
        [Ignore]
        [DataRow("officeoobe", "true", "AccountNoAddress")]
        [DataRow("oxooobe", "true", "AccountNoAddress")]
        [DataRow("webblends", "true", "AccountNoAddress")]
        [DataRow("oxodime", "true", "AccountNoAddress")]
        [DataRow("oxowebdirect", "true", "AccountNoAddress")]
        [DataRow("storify", "true", "AccountNoAddress")]
        [DataRow("xboxsubs", "true", "AccountNoAddress")]
        [DataRow("xboxsettings", "true", "AccountNoAddress")]
        [DataRow("saturn", "true", "AccountNoAddress")]
        [DataRow("officeoobe", "false", "Account001")]
        [DataRow("oxooobe", "false", "Account001")]
        [DataRow("webblends", "false", "Account001")]
        [DataRow("oxodime", "false", "Account001")]
        [DataRow("oxowebdirect", "false", "Account001")]
        [DataRow("storify", "false", "Account001")]
        [DataRow("xboxsubs", "false", "Account001")]
        [DataRow("xboxsettings", "false", "Account001")]
        [DataRow("saturn", "false", "Account001")]
        [DataTestMethod]
        public async Task PaymentInstrumentsEx_Add_AddressNoCityState(string partner, string completePrerequisites, string accountId)
        {
            // Arrange
            bool addressEnrichmentAssertCalled = false;
            bool pimsAssertCalled = false;
            bool getProfilesAssertCalled = false;
            bool getAddressByCountryAssertCalled = false;
            bool postAddressAssertCalled = false;
            bool updateProfileAssertCalled = false;

            string actionNameKey = "InstrumentManagement.ActionName";
            string postPINoCityState = "{\"paymentMethodFamily\":\"credit_card\",\"paymentMethodType\":\"visa\",\"paymentMethodOperation\":\"add\",\"paymentMethodCountry\":\"us\",\"paymentMethodResource_id\":\"credit_card.visa\",\"sessionId\":\"f634e160-96b2-a926-1c4b-86eda833cf27\",\"context\":\"purchase\",\"riskData\":{\"dataType\":\"payment_method_riskData\",\"dataOperation\":\"add\",\"dataCountry\":\"us\"},\"details\":{\"dataType\":\"credit_card_visa_details\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"accountHolderName\":\"John Doe\",\"accountToken\":\"Placeholder\",\"expiryMonth\":\"2\",\"expiryYear\":\"2023\",\"cvvToken\":\"Placeholder\",\"address\":{\"addressType\":\"billing\",\"addressOperation\":\"add\",\"addressCountry\":\"us\",\"address_line1\":\"One Microsoft Way\",\"postal_code\":\"98052\",\"country\":\"us\"}}}";
            string outboundPIWithCityState = "{\"paymentMethodFamily\":\"credit_card\",\"paymentMethodType\":\"visa\",\"paymentMethodOperation\":\"add\",\"paymentMethodCountry\":\"us\",\"paymentMethodResource_id\":\"credit_card.visa\",\"sessionId\":\"f634e160-96b2-a926-1c4b-86eda833cf27\",\"context\":\"purchase\",\"riskData\":{\"dataType\":\"payment_method_riskData\",\"dataOperation\":\"add\",\"dataCountry\":\"us\"},\"details\":{\"dataType\":\"credit_card_visa_details\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"accountHolderName\":\"John Doe\",\"accountToken\":\"Placeholder\",\"expiryMonth\":\"2\",\"expiryYear\":\"2023\",\"cvvToken\":\"Placeholder\",\"address\":{\"addressType\":\"billing\",\"addressOperation\":\"add\",\"addressCountry\":\"us\",\"address_line1\":\"One Microsoft Way\",\"postal_code\":\"98052\",\"country\":\"us\",\"city\":\"redmond\",\"region\":\"wa\"}}}";
            string postAddress = "{\"id\":null,\"unit_number\":null,\"address_line1\":\"One Microsoft Way\",\"address_line2\":null,\"address_line3\":null,\"city\":\"Redmond\",\"district\":null,\"region\":\"WA\",\"postal_code\":\"98052\",\"country\":\"us\"}";

            var piFromPims = PimsMockResponseProvider.GetPaymentInstrument(accountId, string.Format("{0}-Pi001-Visa", accountId));
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(piFromPims));

            string outboundAddressEnrichmentRequest = "{\"address\":\"98052\",\"country\":\"us\"}";

            PXSettings.AddressEnrichmentService.PreProcess = async (request) =>
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                Assert.AreEqual(outboundAddressEnrichmentRequest, requestContent, "Request not as expected");
                addressEnrichmentAssertCalled = true;
                Assert.IsFalse(getProfilesAssertCalled);
            };

            PXSettings.PimsService.PreProcess = async (request) =>
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                Assert.AreEqual(outboundPIWithCityState, requestContent, "Request not as expected");
                Assert.IsTrue(addressEnrichmentAssertCalled);
                pimsAssertCalled = true;
            };

            PXSettings.AccountsService.PreProcess = async (request) =>
            {
                if (request.Method == HttpMethod.Get && request.Properties.ContainsKey(actionNameKey))
                {
                    if (string.Equals(request.Properties[actionNameKey].ToString(), "GetProfiles", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!pimsAssertCalled)
                        {
                            getProfilesAssertCalled = true;
                            Assert.IsTrue(addressEnrichmentAssertCalled);
                            Assert.IsFalse(getAddressByCountryAssertCalled);
                        }
                    }
                    else if (string.Equals(request.Properties[actionNameKey].ToString(), "GetAddressByCountry", StringComparison.OrdinalIgnoreCase))
                    {
                        Assert.IsTrue(addressEnrichmentAssertCalled);
                        Assert.IsTrue(getProfilesAssertCalled);
                        if (!pimsAssertCalled)
                        {
                            getAddressByCountryAssertCalled = true;
                            Assert.IsTrue(getProfilesAssertCalled);
                            Assert.IsFalse(postAddressAssertCalled);
                        }
                    }
                }
                else if (request.Method == HttpMethod.Post && request.Properties.ContainsKey(actionNameKey))
                {
                    if (string.Equals(request.Properties[actionNameKey].ToString(), "PostAddress", StringComparison.OrdinalIgnoreCase))
                    {
                        string requestContent = await request.Content.ReadAsStringAsync();
                        postAddressAssertCalled = true;
                        Assert.AreEqual(postAddress, requestContent, "Request not as expected");
                        Assert.IsTrue(getAddressByCountryAssertCalled);
                        Assert.IsFalse(updateProfileAssertCalled);
                    }
                    else if (string.Equals(request.Properties[actionNameKey].ToString(), "UpdateProfile", StringComparison.OrdinalIgnoreCase))
                    {
                        Assert.IsTrue(postAddressAssertCalled);
                        updateProfileAssertCalled = true;
                    }
                }
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync(string.Format("/v7.0/{0}/paymentInstrumentsEx?country=us&language=en-US&partner={1}&scenario=addressnocitystate&completeprerequisites={2}", accountId, partner, completePrerequisites), new StringContent(postPINoCityState, Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsTrue(addressEnrichmentAssertCalled, "AddressEnrichment PreProcess should be called");
            Assert.IsTrue(pimsAssertCalled, "Pims PreProcess wasn't called");
            if (string.Equals(completePrerequisites, "true", StringComparison.OrdinalIgnoreCase))
            {
                Assert.IsTrue(getProfilesAssertCalled, "GetProfiles in AccountService PreProcess wasn't called when completePrerequisites = true");
                Assert.IsTrue(getAddressByCountryAssertCalled, "GetAddressByCountry in AccountService PreProcess wasn't called when completePrerequisites = true");
                Assert.IsTrue(postAddressAssertCalled, "PostAddress in AccountService PreProcess wasn't called when completePrerequisites = true");
                Assert.IsTrue(updateProfileAssertCalled, "UpdateProfile in AccountService PreProcess wasn't called when completePrerequisites = true");
            }
            else
            {
                Assert.IsFalse(postAddressAssertCalled, "PostAddress in AccountService PreProcess was called when completePrerequisites = false");
                Assert.IsFalse(updateProfileAssertCalled, "UpdateProfile in AccountService PreProcess was called when completePrerequisites = false");
            }

            PXSettings.PimsService.ResetToDefaults();
            PXSettings.AddressEnrichmentService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
        }

        [DataRow("officeoobe", "true")]
        [DataRow("oxooobe", "true")]
        [DataRow("webblends", "true")]
        [DataRow("oxodime", "true")]
        [DataRow("oxowebdirect", "true")]
        [DataRow("storify", "true")]
        [DataRow("xboxsubs", "true")]
        [DataRow("xboxsettings", "true")]
        [DataRow("saturn", "true")]
        [DataRow("officeoobe", "false")]
        [DataRow("oxooobe", "false")]
        [DataRow("webblends", "false")]
        [DataRow("oxodime", "false")]
        [DataRow("oxowebdirect", "false")]
        [DataRow("storify", "false")]
        [DataRow("xboxsubs", "false")]
        [DataRow("xboxsettings", "false")]
        [DataRow("saturn", "false")]
        [DataTestMethod]
        public async Task PaymentInstrumentsEx_Add_AddressNoCityState_Fallback(string partner, string completePrerequisites)
        {
            // Arrange
            bool addressEnrichmentAssertCalled = false;
            bool pimsCalled = false;

            string postPINoCityState = "{\"displayedPaymentMethodTypes\":\"[\\\"visa\\\",\\\"amex\\\",\\\"mc\\\"]\",\"paymentMethodFamily\":\"credit_card\",\"paymentMethodType\":\"visa\",\"paymentMethodOperation\":\"add\",\"paymentMethodCountry\":\"us\",\"paymentMethodResource_id\":\"credit_card.visa\",\"sessionId\":\"f634e160-96b2-a926-1c4b-86eda833cf27\",\"context\":\"purchase\",\"riskData\":{\"dataType\":\"payment_method_riskData\",\"dataOperation\":\"add\",\"dataCountry\":\"us\"},\"details\":{\"dataType\":\"credit_card_visa_details\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"accountHolderName\":\"John Doe\",\"accountToken\":\"Placeholder\",\"expiryMonth\":\"2\",\"expiryYear\":\"2023\",\"cvvToken\":\"Placeholder\",\"address\":{\"addressType\":\"billing\",\"addressOperation\":\"add\",\"addressCountry\":\"us\",\"address_line1\":\"One Microsoft Way\",\"postal_code\":\"98012\",\"country\":\"us\"}}}";
            string outboundAddressEnrichmentRequest = "{\"address\":\"98012\",\"country\":\"us\"}";
            bool expectedRetainUserInputValue = true;

            PXSettings.AddressEnrichmentService.PreProcess = async (request) =>
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                Assert.AreEqual(outboundAddressEnrichmentRequest, requestContent, "Request not as expected");
                addressEnrichmentAssertCalled = true;
            };

            PXSettings.PimsService.PreProcess = (request) =>
            {
                pimsCalled = true;
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync(string.Format("/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={0}&scenario=addressnocitystate&completeprerequisites={1}", partner, completePrerequisites), new StringContent(postPINoCityState, Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsTrue(addressEnrichmentAssertCalled, "AddressEnrichment PreProcess should be called");
            Assert.IsFalse(pimsCalled, "Pims shouldn't be called");

            var pidlClientAction = ReadSinglePidlResourceFromJson(await result.Content.ReadAsStringAsync());
            Assert.IsNotNull(pidlClientAction.ClientAction);
            Assert.AreEqual(ClientActionType.Pidl, pidlClientAction.ClientAction.ActionType);
            Assert.IsNotNull(pidlClientAction.ClientAction.Context);

            var actionContext = JsonConvert.DeserializeObject<ActionContext>(pidlClientAction.ClientAction.Context.ToString());
            var pidlError = JsonConvert.DeserializeObject<PIDLError>(pidlClientAction.ClientAction.PidlError.ToString());
            Assert.IsNotNull(actionContext);
            Assert.AreEqual(actionContext.PaymentMethodFamily, "credit_card", "PaymentMethodFamily in the ActionContext is not as expected");
            Assert.AreEqual(actionContext.PaymentMethodType, "visa,amex,mc", "PaymentMethodType in the ActionContext is not as expected");
            Assert.AreEqual(actionContext.ResourceActionContext.Action, "addResource", "Action in the ResourceActionContext is not as expected");
            Assert.AreEqual(actionContext.ResourceActionContext.PidlDocInfo.ResourceType, "paymentMethod", "ResourceType in PidlDocInfo is not as expected");
            Assert.IsNotNull(pidlError);
            Assert.AreEqual(pidlError.Code, "InvalidZipCode", "Code in PidlError is not as expected");
            Assert.AreEqual(pidlClientAction.ClientAction.PidlRetainUserInput, expectedRetainUserInputValue, "Value of PidlRetainUserInput is not as expected");
            Assert.IsNotNull(pidlError.Details, "Pidl Error Details expected not to be null");
            Assert.IsTrue(pidlError.Details.Count > 1 && pidlError.Details.Count <= 3);
            if (string.Equals("true", completePrerequisites, StringComparison.OrdinalIgnoreCase))
            {
                Assert.IsTrue(string.Equals("true", actionContext.ResourceActionContext.PidlDocInfo.Parameters["completePrerequisites"], StringComparison.OrdinalIgnoreCase), "Complete prerequisites parameter was not successfully set to true");
            }
            else
            {
                Assert.IsFalse(actionContext.ResourceActionContext.PidlDocInfo.Parameters.ContainsKey("completePrerequisites"), "Complete prerequisites key was not expeceted in parameters.");
            }

            foreach (PIDLErrorDetail pidlErrorDetail in pidlError.Details)
            {
                Assert.IsNotNull(pidlErrorDetail, "Expected Error Detail not to be null");
                Assert.IsNotNull(pidlErrorDetail.Code, "Expected Error Detail Code not to be null");
                Assert.IsNotNull(pidlErrorDetail.Message, "Expected Error Detail Message not to be null");
                Assert.IsNotNull(pidlErrorDetail.Target, "Expected Error Detail Target not to be null");
            }

            PXSettings.PimsService.ResetToDefaults();
            PXSettings.AddressEnrichmentService.ResetToDefaults();
        }

        [DataTestMethod]
        public async Task PaymentInstrumentsEx_ListPI_WithCSVLot()
        {
            // Arrange
            var expectedPIs = PimsMockResponseProvider.ListPaymentInstruments("Account001").ToList();
            expectedPIs[0].PaymentInstrumentDetails.Lots = new List<global::Tests.Common.Model.Pims.StoredValueLotDetails>
            {
                new global::Tests.Common.Model.Pims.StoredValueLotDetails
                {
                    CurrentBalance = 225.65M,
                    OriginalBalance = 225.65M,
                    ExpirationDate = DateTime.Now.AddDays(30),
                    LastUpdatedTime = DateTime.Now.AddDays(-30),
                    LotType = "CSV",
                    Status = "Active",
                    TokenInstanceId = "asdf"
                }
            };

            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPIs));

            // Act
            HttpResponseMessage result = await PXClient.GetAsync($"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner=northstarweb");
            var actualPIs = JsonConvert.DeserializeObject<global::Tests.Common.Model.Pims.PaymentInstrument[]>(await result.Content.ReadAsStringAsync());

            Assert.IsNotNull(actualPIs[0].PaymentInstrumentDetails.Lots);
            Assert.AreEqual(expectedPIs[0].PaymentInstrumentDetails.Lots.Count, actualPIs[0].PaymentInstrumentDetails.Lots.Count);
            Assert.AreEqual(expectedPIs[0].PaymentInstrumentDetails.Lots[0].CurrentBalance, actualPIs[0].PaymentInstrumentDetails.Lots[0].CurrentBalance);
            Assert.AreEqual(expectedPIs[0].PaymentInstrumentDetails.Lots[0].ExpirationDate, actualPIs[0].PaymentInstrumentDetails.Lots[0].ExpirationDate);
        }

        [DataTestMethod]
        public async Task PaymentInstrumentsEx_ListPI_includeDuplicates()
        {
            // Arrange
            string includeDuplicates = "includeDuplicates=True";
            bool isIncludeDuplicatesInParams = false;

            PXSettings.PimsService.PreProcess = (request) =>
            {
                if (request.RequestUri.AbsoluteUri.Contains(includeDuplicates))
                {
                    isIncludeDuplicatesInParams = true;
                }
            };

            // Act
            HttpResponseMessage listPi = await PXClient.GetAsync(string.Format("/v7.0/Account001/paymentInstrumentsEx?language=en-US&partner=northstarweb&status=active,removed&{0}", includeDuplicates));

            // Assert
            Assert.IsTrue(isIncludeDuplicatesInParams);
            PXSettings.PimsService.ResetToDefaults();
        }

        [DataRow("xbox", "18230582934452242973", null, "Account001-Pi001-Visa", "true", true, "131.107.174.30")]
        [DataRow("xbox", null, "12345", "Account001-Pi001-Visa", "true", true, "")]
        [DataRow("xbox", "18230582934452242973", null, "Account001-Pi001-Visa", "true", false, "131.107.174.30")]
        [DataRow("xbox", "18230582934452242973", "12345", "Account001-Pi001-Visa", "true", false, "")]
        [DataRow("storify", null, "12345", "Account001-Pi008-Paypal", "true", true, "")]
        [DataRow("webblends", "18230582934452242973", null, "Account001-Pi008-Paypal", "true", false, "131.107.174.30")]
        [DataRow("amcweb", "18230582934452242973", "12345", "Account001-Pi008-Paypal", "true", false, "")]
        [DataRow("xbox", "18230582934452242973", null, "Account001-Pi008-Paypal", "true", true, "131.107.174.30")]
        [DataRow("xbox", "182305829344522429718201234567123456789", "12345", "Account001-Pi001-Visa", "true", true, "131.107.174.30")]
        [DataRow("xbox", "", "12345", "Account001-Pi001-Visa", "true", false, "")]
        [DataRow("xbox", "1823", "12345", "Account001-Pi001-Visa", "true", true, "131.107.174.30")]
        [DataRow("xbox", "18230582934452242973", "12345", "NonSimMobiAccount-Pi001-NonSimMobi", "false", false, "131.107.174.30")]
        [DataRow("xbox", "", "12345", "NonSimMobiAccount-Pi001-NonSimMobi", "false", true, "")]
        [DataRow("xbox", "1357", "12345", "NonSimMobiAccount-Pi001-NonSimMobi", "true", false, "131.107.174.30")]
        [DataRow("xbox", "182305829344522429730123456789", "12345", "NonSimMobiAccount-Pi001-NonSimMobi", "true", false, "131.107.174.30")]
        [DataRow("storify", "18230582934452242973", "", "Account001-Pi001-Visa", "true", true, "")]
        [DataRow("xboxsubs", "18230582934452242973", "", "Account001-Pi001-Visa", "true", true, "")]
        [DataRow("xboxsettings", "18230582934452242973", "", "Account001-Pi001-Visa", "true", true, "")]
        [DataRow("saturn", "18230582934452242973", "", "Account001-Pi001-Visa", "true", true, "")]
        [DataRow("storify", "18230582934452242973", "null", "Account001-Pi001-Visa", "true", false, "131.107.174.30")]
        [DataRow("storify", "18230582934452242973", "121", "Account001-Pi001-Visa", "true", true, "131.107.174.30")]
        [DataRow("storify", "18230582934452242973", "12324354", "Account001-Pi001-Visa", "true", false, "131.107.174.30")]
        [DataRow("storify", "18230582934452242973", "12345", "Account001-Pi001-Visa", "true", true, "")]
        [DataRow("xbox", "18230582934452242973", "12345", "Account001-Pi001-Visa", "true", false, "131.107.174.30")]
        [DataRow("webblends", "18230582934452242973", "12345", "Account001-Pi001-Visa", "true", true, "131.107.174.30")]
        [DataRow("amcweb", "18230582934452242973", "12345", "Account001-Pi001-Visa", "true", false, "131.107.174.30")]
        [DataRow("xbox", "18230582934452242973", "12345", "Account001-Pi001-Visa", "true", true, "")]
        [DataRow("commercialstores", "18230582934452242973", "12345", "Account001-Pi001-Visa", "true", false, "131.107.174.30")]
        [DataRow("oxodime", "18230582934452242973", "12345", "Account001-Pi001-Visa", "true", false, "131.107.174.30")]
        [DataTestMethod]
        public async Task PaymentInstrumentsEx_PassDeviceIdInRiskData(string partner, string xboxDeviceId, string deviceId, string accountId, string completePrerequisites, bool hasHeader, string ipaddress)
        {
            // Arrange
            bool pimsRequestCalled = false;
            bool containsDeviceId = false;
            bool containsRiskData = false;
            bool containsIpAddressProperty = false;
            string postReqWithRiskData = null;
            string postRequestUri = null;

            PaymentInstrument piFromPims = null;
            if (accountId == "NonSimMobiAccount-Pi001-NonSimMobi")
            {
                postReqWithRiskData = "{\"paymentMethodFamily\":\"mobile_billing_non_sim\",\"paymentMethodType\":\"vzw-us-nonsim\",\"paymentMethodOperation\":\"add\",\"paymentMethodCountry\":\"us\",\"paymentMethodResource_id\":\"mobile_billing_non_sim\",\"sessionId\":\"37e6b385-ef8a-a152-9e38-7915a1bde35c\",\"context\":\"purchase\",\"riskData\":{\"DeviceInfo\":\"" + deviceId + "\",\"dataType\":\"payment_method_riskData\",\"dataOperation\":\"add\",\"dataCountry\":\"us\"},\"details\":{\"dataType\":\"mobile_billing_non_sim_details\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"msisdn\":\"+14254176866\"}}";
                postRequestUri = $"/v7.0/NonSimMobiAccount/paymentInstrumentsEx?country=us&language=en-US&partner={partner}&completeprerequisites={completePrerequisites}";
                piFromPims = PimsMockResponseProvider.GetPaymentInstrument("NonSimMobiAccount", string.Format(accountId, "NonSimMobiAccount"));
            }
            else if (accountId == "Account001-Pi001-Visa")
            {
                postReqWithRiskData = "{\"paymentMethodFamily\":\"credit_card\",\"paymentMethodType\":\"visa\",\"paymentMethodOperation\":\"add\",\"paymentMethodCountry\":\"us\",\"paymentMethodResource_id\":\"credit_card.visa\",\"sessionId\":\"f634e160-96b2-a926-1c4b-86eda833cf27\",\"context\":\"purchase\",\"riskData\":{\"DeviceInfo\":\"" + deviceId + "\",\"dataType\":\"payment_method_riskData\",\"dataOperation\":\"add\",\"dataCountry\":\"us\"},\"details\":{\"dataType\":\"credit_card_visa_details\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"accountHolderName\":\"John Doe\",\"expiryMonth\":\"2\",\"expiryYear\":\"2023\",\"address\":{\"addressType\":\"billing\",\"addressOperation\":\"add\",\"addressCountry\":\"us\",\"address_line1\":\"One Microsoft Way\",\"postal_code\":\"98052\",\"country\":\"us\",\"city\":\"redmond\",\"region\":\"wa\"}}}";
                postRequestUri = $"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}&completePrerequisites={completePrerequisites}";
                piFromPims = PimsMockResponseProvider.GetPaymentInstrument("Account001", string.Format(accountId, "Account001"));
            }
            else if (accountId == "Account001-Pi008-Paypal")
            {
                postReqWithRiskData = "{\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"paypal\",\"paymentMethodOperation\":\"add\",\"paymentMethodCountry\":\"us\",\"paymentMethodResource_id\":\"ewallet.paypal\",\"sessionId\":\"f634e160-96b2-a926-1c4b-86eda833cf27\",\"context\":\"purchase\",\"riskData\":{\"DeviceInfo\":\"" + deviceId + "\",\"dataType\":\"payment_method_riskData\",\"dataOperation\":\"add\",\"dataCountry\":\"us\"},\"details\":{\"dataType\":\"ewallet_paypal_details\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"email\":\"\",\"encryptedPassword\":\"\",\"authenticationMode\":\"UsernameAndPassword\"}}";
                postRequestUri = $"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}&completePrerequisites={completePrerequisites}";
                piFromPims = PimsMockResponseProvider.GetPaymentInstrument("Account001", string.Format(accountId, "Account001"));
            }

            var postRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl(postRequestUri)),
                Method = HttpMethod.Post,
                Content = new StringContent(postReqWithRiskData, Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };
            if (Microsoft.Commerce.Payments.PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner) || partner == "AmcXbox" || partner == "xbox" & hasHeader)
            {
                var headers = new Dictionary<string, string>()
                {
                    {
                        "x-ms-deviceinfo", $"ipAddress={ipaddress}, xboxLiveDeviceId={xboxDeviceId}"
                    }
                };

                headers?.ToList().ForEach(header =>
                {
                    postRequest.Headers.Add(header.Key, header.Value);
                    if (header.Value.Contains("xboxLiveDeviceId") & !header.Value.EndsWith("="))
                    {
                        containsDeviceId = true;
                    }

                    if (header.Value.Contains("ipAddress"))
                    {
                        containsIpAddressProperty = true;
                    }
                });
            }

            if (postReqWithRiskData.Contains("riskData"))
            {
                containsRiskData = true;
            }

            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(piFromPims));

            PXSettings.PimsService.PreProcess = async (request) =>
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(requestContent);

                if (containsRiskData == true)
                {
                    Assert.IsTrue(requestContent.Contains("riskData"), "does not contain riskData in request content");
                    if (containsDeviceId == true)
                    {
                        Assert.IsTrue(requestContent.Contains("deviceId"), "does not contain DeviceId in riskData in request content");
                        var postReqRiskData = json.GetValue("riskData");
                        var postReqDeviceId = postReqRiskData.SelectToken("deviceId");
                        Assert.AreEqual(postReqDeviceId, xboxDeviceId, "device id does not match deviceID in risk data before PIMS");
                    }

                    if (containsIpAddressProperty == true)
                    {
                        Assert.IsTrue(requestContent.Contains("ipAddress"), "does not contain ipaddress in riskData in request content");
                        var postReqRiskData = json.GetValue("riskData");
                        string postReqIpAddress = (string)postReqRiskData.SelectToken("ipAddress");
                        Assert.AreEqual(postReqIpAddress, ipaddress, "ipAddress does not match ipAddress in risk data before PIMS");
                    }
                }

                pimsRequestCalled = true;
            };

            HttpResponseMessage response = await PXClient.SendAsync(postRequest);

            Assert.IsTrue(pimsRequestCalled, "Pims PreProcess wasn't called");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            PXSettings.PimsService.ResetToDefaults();
        }

        [DataRow("northstarweb", "Account001-Pi001-Visa", true)]
        [DataRow("northstarweb", "NonSimMobiAccount-Pi001-NonSimMobi", true)]
        [DataRow("northstarweb", "Account001-Pi008-Paypal", true)]
        [DataRow("northstarweb", "Account001-Pi001-Visa", false)]
        [DataRow("northstarweb", "NonSimMobiAccount-Pi001-NonSimMobi", false)]
        [DataRow("northstarweb", "Account001-Pi008-Paypal", false)]
        [DataTestMethod]
        public async Task PaymentInstrumentsEx_PassIpAddressInRiskData(string partner, string accountId, bool enableFlight)
        {
            // Arrange
            string greenId = "10c27091-4869-0714-2a93-8fee2cfaf6cb";
            string ipAddress = "131.107.174.30";
            bool pimsRequestCalled = false;
            string postReqWithRiskData = null;
            string postRequestUri = null;

            PaymentInstrument piFromPims = null;
            if (accountId == "NonSimMobiAccount-Pi001-NonSimMobi")
            {
                postReqWithRiskData = "{\"paymentMethodFamily\":\"mobile_billing_non_sim\",\"paymentMethodType\":\"vzw-us-nonsim\",\"paymentMethodOperation\":\"add\",\"paymentMethodCountry\":\"us\",\"paymentMethodResource_id\":\"mobile_billing_non_sim\",\"sessionId\":\"37e6b385-ef8a-a152-9e38-7915a1bde35c\",\"context\":\"purchase\",\"riskData\":{\"greenId\":\"" + greenId + "\",\"dataType\":\"payment_method_riskData\",\"dataOperation\":\"add\",\"dataCountry\":\"us\"},\"details\":{\"dataType\":\"mobile_billing_non_sim_details\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"msisdn\":\"+14254176866\"}}";
                postRequestUri = $"/v7.0/NonSimMobiAccount/paymentInstrumentsEx?country=us&language=en-US&partner={partner}";
                piFromPims = PimsMockResponseProvider.GetPaymentInstrument("NonSimMobiAccount", string.Format(accountId, "NonSimMobiAccount"));
            }
            else if (accountId == "Account001-Pi001-Visa")
            {
                postReqWithRiskData = "{\"paymentMethodFamily\":\"credit_card\",\"paymentMethodType\":\"visa\",\"paymentMethodOperation\":\"add\",\"paymentMethodCountry\":\"us\",\"paymentMethodResource_id\":\"credit_card.visa\",\"sessionId\":\"f634e160-96b2-a926-1c4b-86eda833cf27\",\"context\":\"purchase\",\"riskData\":{\"greenId\":\"" + greenId + "\",\"dataType\":\"payment_method_riskData\",\"dataOperation\":\"add\",\"dataCountry\":\"us\"},\"details\":{\"dataType\":\"credit_card_visa_details\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"accountHolderName\":\"John Doe\",\"expiryMonth\":\"2\",\"expiryYear\":\"2023\",\"address\":{\"addressType\":\"billing\",\"addressOperation\":\"add\",\"addressCountry\":\"us\",\"address_line1\":\"One Microsoft Way\",\"postal_code\":\"98052\",\"country\":\"us\",\"city\":\"redmond\",\"region\":\"wa\"}}}";
                postRequestUri = $"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}";
                piFromPims = PimsMockResponseProvider.GetPaymentInstrument("Account001", string.Format(accountId, "Account001"));
            }
            else if (accountId == "Account001-Pi008-Paypal")
            {
                postReqWithRiskData = "{\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"paypal\",\"paymentMethodOperation\":\"add\",\"paymentMethodCountry\":\"us\",\"paymentMethodResource_id\":\"ewallet.paypal\",\"sessionId\":\"f634e160-96b2-a926-1c4b-86eda833cf27\",\"context\":\"purchase\",\"riskData\":{\"greenId\":\"" + greenId + "\",\"dataType\":\"payment_method_riskData\",\"dataOperation\":\"add\",\"dataCountry\":\"us\"},\"details\":{\"dataType\":\"ewallet_paypal_details\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"email\":\"\",\"encryptedPassword\":\"\",\"authenticationMode\":\"UsernameAndPassword\"}}";
                postRequestUri = $"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}";
                piFromPims = PimsMockResponseProvider.GetPaymentInstrument("Account001", string.Format(accountId, "Account001"));
            }

            var postRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl(postRequestUri)),
                Method = HttpMethod.Post,
                Content = new StringContent(postReqWithRiskData, Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-deviceinfo", $"ipAddress={ipAddress}"
                }
            };

            if (enableFlight)
            {
                PXFlightHandler.AddToEnabledFlights("PXPassIpAddressToPIMSForAddUpdatePI");
            }

            headers?.ToList().ForEach(header =>
            {
                postRequest.Headers.Add(header.Key, header.Value);
            });

            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(piFromPims));

            PXSettings.PimsService.PreProcess = async (request) =>
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(requestContent);

                Assert.IsTrue(requestContent.Contains("riskData"), "does not contain riskData in request content");
                Assert.IsTrue(requestContent.Contains("greenId"), "does not contain greenid in riskData in request content");
                var postReqRiskData = json.GetValue("riskData");
                string postReqGreenId = (string)postReqRiskData.SelectToken("greenId");
                Assert.AreEqual(greenId, postReqGreenId, "greenid does not match greenid in risk data before PIMS");

                if (enableFlight)
                {
                    Assert.IsTrue(requestContent.Contains("ipAddress"), "does not contain DeviceId in riskData in request content");
                    postReqRiskData = json.GetValue("riskData");
                    var postReqIpAddress = (string)postReqRiskData.SelectToken("ipAddress");
                    Assert.AreEqual(ipAddress, postReqIpAddress, "ipaddress id does not match deviceID in risk data before PIMS");
                }

                pimsRequestCalled = true;
            };

            HttpResponseMessage response = await PXClient.SendAsync(postRequest);

            Assert.IsTrue(pimsRequestCalled, "Pims PreProcess wasn't called");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            PXSettings.PimsService.ResetToDefaults();
        }

        [DataRow("windowsstore", "Account001-Pi014-Venmo")]
        [DataTestMethod]
        public async Task PaymentInstrumentsEx_PassUserAgentInRiskData(string partner, string accountId)
        {
            string ipAddress = "131.107.174.30";
            string postReqWithRiskData = null;
            string postRequestUri = null;
            PaymentInstrument piFromPims = null;
            string mockResponse = Constants.PSSMockResponses.PXPartnerSettingsWindowsStore;
            string expectedUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36 Edg/122.0.0.";
            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PartnerSettingsService.ArrangeResponse(mockResponse);
            PXFlightHandler.AddToEnabledFlights("PXPassUserAgentToPIMSForAddUpdatePI, PXPassIpAddressToPIMSForAddUpdatePI, PXUsePartnerSettingsService");

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-deviceinfo", $"ipAddress={ipAddress}"
                },
                {
                    "x-ms-flight", "PXUsePartnerSettingsService"
                }
            };

            if (accountId == "Account001-Pi014-Venmo")
            {
                postReqWithRiskData = "{\"paymentMethodFamily\":\"ewallet\",\"paymentMethodType\":\"venmo\",\"paymentMethodOperation\":\"add\",\"paymentMethodCountry\":\"us\",\"paymentMethodResource_id\":\"ewallet.venmoQrCode\",\"sessionId\":\"25ed79d0-0b5a-8822-7b9b-0f8cc0f9deca\",\"riskData\":{\"dataType\":\"payment_method_riskData\",\"dataOperation\":\"add\",\"dataCountry\":\"us\"},\"details\":{\"dataType\":\"ewallet_venmo_details\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"authenticationMode\":\"Redirect\"}}";
                postRequestUri = $"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}";
                piFromPims = PimsMockResponseProvider.GetPaymentInstrument("Account001", string.Format(accountId, "Account001"));
            }

            var postRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl(postRequestUri)),
                Method = HttpMethod.Post,
                Content = new StringContent(postReqWithRiskData, Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            headers?.ToList().ForEach(header =>
            {
                postRequest.Headers.Add(header.Key, header.Value);
            });

            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(piFromPims));
            
            PXSettings.PimsService.PreProcess = async (request) =>
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(requestContent);

                Assert.IsTrue(requestContent.Contains("riskData"), "does not contain riskData in request content");
                var postReqRiskData = json.GetValue("riskData") as JObject;
                string ipAddressVal = postReqRiskData.GetValue("ipAddress").ToString();
                string userAgentVal = postReqRiskData.GetValue("userAgent").ToString();

                Assert.AreEqual(ipAddress, ipAddressVal, "ipaddress id does not match deviceID in risk data before PIMS");
                Assert.AreEqual(expectedUserAgent, userAgentVal, "userAgent does not match with user agent in risk data before PIMS");
            };

            HttpResponseMessage response = await PXClient.SendAsync(postRequest);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            PXSettings.PimsService.ResetToDefaults();
        }

        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_AddPI_SetEmptyPayInAccountAddressWithPIAddress()
        {
            // Arrange
            bool postPIToPIMSAssertCalled = false;
            string jarvisAccountId = "Account001";
            var postCreditCardPayload = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "mc",
                paymentMethodOperation = "add",
                paymentMethodCountry = "br",
                paymentMethodResource_id = "credit_card.mc",
                sessionId = "89f197ea-73be-abed-d2b8-779813f16689",
                context = "purchase",
                riskData = new
                {
                    dataType = "payment_method_riskData",
                    dataOperation = "add",
                    dataCountry = "br",
                    greenId = "35abd8c5-7d44-41b2-83d5-780beb5e16bc"
                },
                details = new
                {
                    dataType = "credit_card_mc_details",
                    dataOperation = "add",
                    dataCountry = "br",
                    accountHolderName = "Tim",
                    accountToken = "...2Yg=(44)",
                    expiryMonth = "12",
                    expiryYear = "2022",
                    cvvToken = "...2Yg=(44)",
                    address = new
                    {
                        addressType = "billing",
                        addressOperation = "add",
                        addressCountry = "br",
                        address_line1 = "one microsoft way",
                        address_line2 = string.Empty,
                        city = "Conde",
                        region = "pb",
                        postal_code = "58322-000",
                        country = "br"
                    },
                    permission = new
                    {
                        dataType = "permission_details",
                        dataOperation = "add",
                        dataCountry = "br",
                        hmac = new
                        {
                            algorithm = "hmacsha256",
                            keyToken = "MASKED(44)",
                            data = "...9rY=(44)"
                        },
                        userCredential = "MASKED(1862)"
                    }
                }
            };

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-clientcontext-encoding", "base64"
                },
                {
                    "x-ms-aadinfo", "tid=Y2ViMTM2ZmYtZGEzNC00NDkyLTkyNGQtNWY2MTIxMTFmNjc5,oid=ZjAyYmI0N2YtNzI4Ny00OTdlLWI2NTEtYTk0ZmRiM2RmM2Fk,altSecId=MTA1NTUxOTQxMTM0MDUwOQ=="
                }
            };

            var newPI = PimsMockResponseProvider.GetPaymentInstrument(jarvisAccountId, "Account001-Pi002-MC");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(newPI));

            PXSettings.PimsService.PreProcess = (req) =>
            {
                if (req.RequestUri.AbsolutePath.Contains($"/v4.0/{jarvisAccountId}/paymentInstruments"))
                {
                    postPIToPIMSAssertCalled = true;
                }
            };

            string postPIURL = $"/v7.0/Account001/paymentInstrumentsEx?"
                + "country=br&language=pt-BR&partner=Azure"
                + $"&classicProduct=azureClassic&billableAccountId={CommerceAccountDataAccessor.BillingAccountId.AzureBusinessAccount}";

            // Act
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl(postPIURL)),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(postCreditCardPayload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };
            headers?.ToList().ForEach(header => request.Headers.Add(header.Key, header.Value));

            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert
            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK);
            Assert.IsTrue(postPIToPIMSAssertCalled, "PimsTestHandler.PreProcess wasn't called");

            // Verify the legacy account address has the same address line 1 as payment instrument address
            var postedAddressSet = PXSettings.CommerceAccountDataService.UpdateAccountRequest.Account.PayinAccount.AddressSet;
            Assert.AreEqual(1, postedAddressSet.Count);
            Assert.AreEqual(postCreditCardPayload.details.address.address_line1, postedAddressSet[0].Street1);
            PXSettings.PimsService.ResetToDefaults();
        }

        [DataRow("webblends", true)]
        [DataRow("northstarweb", true)]
        [DataRow("xbox", false)]
        [DataRow("azure", false)]
        [DataRow("commercialstores", false)]
        [TestMethod]
        public async Task PaymentInstrumentsEx_JarvisHMAC(string partner, bool isJarvisAccountIdHmacEnabledPartner)
        {
            // Arrange
            dynamic emptyRequestBody;
            if (isJarvisAccountIdHmacEnabledPartner)
            {
                emptyRequestBody = new
                {
                    paymentMethodFamily = "credit_card",
                    paymentMethodType = "visa",
                    mac = "09D1F309C227EA1F20D79B80F58ED034F187EE4E4348E68BA16EA00636C83FFC"
                };
            }
            else
            {
                emptyRequestBody = new
                {
                    paymentMethodFamily = "credit_card",
                    paymentMethodType = "visa"
                };
            }

            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi001-Visa");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            bool assertCalled = false;
            PXSettings.PimsService.PreProcess = async (request) =>
            {
                // Assert
                string requestContent = await request.Content.ReadAsStringAsync();
                JObject jsonObj = JObject.Parse(requestContent);
                Assert.IsNull(jsonObj.SelectToken("pxmac"));
                assertCalled = true;
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}", new StringContent(JsonConvert.SerializeObject(emptyRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsTrue(assertCalled, "Assert in PimsTestHandler.PreProcess did not succeed.");
            PXSettings.PimsService.ResetToDefaults();
        }

        [DataRow("amcweb", true, false, false)]
        [DataRow("amcweb", false, true, false)]
        [DataRow("amcweb", true, true, true)]
        [DataRow("azure", true, false, false)]
        [DataRow("azure", false, true, false)]
        [DataRow("azure", true, true, true)]
        [DataRow("webblends", true, false, false)]
        [DataRow("webblends", false, true, false)]
        [DataRow("webblends", true, true, true)]
        [DataRow("commercialstores", true, false, false)]
        [DataRow("commercialstores", false, true, false)]
        [DataRow("commercialstores", true, true, true)]
        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_PXEnableJarvisHMAC(string partner, bool enablePXEnableJarvisHMACValidation, bool expectedSuccessfulAddPI, bool verifyJarvisAccountIdHmac)
        {
            // Arrange
            // Verify the match between PXMAC (Hashed Account ID) with the account Id "Account001".
            var pxMac = verifyJarvisAccountIdHmac ? "09D1F309C227EA1F20D79B80F58ED034F187EE4E4348E68BA16EA00636C83FFC" : string.Empty;
            var emptyRequestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                pxmac = pxMac
            };

            if (enablePXEnableJarvisHMACValidation)
            {
                PXFlightHandler.AddToEnabledFlights("PXEnableJarvisHMACValidation");
            }

            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi001-Visa");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            // Act
            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}", new StringContent(JsonConvert.SerializeObject(emptyRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert
            string resultContent = await result.Content.ReadAsStringAsync();
            string instrumentManagementPropertyMessage = result.RequestMessage.Properties["InstrumentManagement.Message"].ToString();
            Assert.IsNotNull(instrumentManagementPropertyMessage);
            if (verifyJarvisAccountIdHmac)
            {
                Assert.IsTrue(instrumentManagementPropertyMessage.Contains("JarvisAccountIdHmac matched."));
            }
            else
            {
                Assert.IsFalse(instrumentManagementPropertyMessage.Contains("JarvisAccountIdHmac matched."));
            }

            if (expectedSuccessfulAddPI)
            {
                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Expected 200 OK");
                var pi = JsonConvert.DeserializeObject<global::Tests.Common.Model.Pims.PaymentInstrument>(resultContent);
                Assert.AreEqual(expectedPI.PaymentInstrumentId, pi.PaymentInstrumentId);
                Assert.IsNotNull(pi.PaymentInstrumentDetails.DefaultDisplayName);
            }
            else
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode, "Expected 400 BadREquest");
                JObject error = JObject.Parse(resultContent);
                Assert.AreEqual("ValidationFailed", error["ErrorCode"], "Failure expected");
            }
        }

        [DataRow("xboxsettings", "NonSimMobiAccount-Pi001-NonSimMobi", "smsChallengeCode", "pin")]
        [DataRow("storify", "NonSimMobiAccount-Pi001-NonSimMobi", "smsChallengeCode", "pin")]
        [DataRow("xboxsubs", "NonSimMobiAccount-Pi001-NonSimMobi", "smsChallengeCode", "pin")]
        [DataRow("saturn", "NonSimMobiAccount-Pi001-NonSimMobi", "smsChallengeCode", "pin")]
        [DataRow("amcweb", "NonSimMobiAccount-Pi001-NonSimMobi", "smsChallengeCode", "pin")]
        [DataRow("northstarweb", "NonSimMobiAccount-Pi001-NonSimMobi", "smsChallengeCode", "pin")]
        [DataRow("commercialstores", "NonSimMobiAccount-Pi001-NonSimMobi", "smsChallengeCode", "pin")]
        [DataRow("webblends", "NonSimMobiAccount-Pi001-NonSimMobi", "smsChallengeCode", "pin")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_NonSimMobi_AddPi(string partner, string accountId, string displayHintId, string key)
        {
            // Arrange
            bool assertCalled = false;
            var emptyRequestBody = new
            {
                paymentMethodFamily = "paymentMethodFamily",
                paymentMethodType = "paymentMethodType",
                riskData = new { }
            };

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("NonSimMobiAccount", string.Format(accountId, "NonSimMobiAccount"));

            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXSettings.PimsService.PreProcess = async (request) =>
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(requestContent);
                Assert.IsTrue(JObject.DeepEquals(json, JObject.FromObject(emptyRequestBody)), "Expected request data is not found");
                assertCalled = true;
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}", new StringContent(JsonConvert.SerializeObject(emptyRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            // Assert (continuation)
            Assert.IsTrue(assertCalled, "PimsTestHandler.PreProcess wasn't called");
            string resultContent = await result.Content.ReadAsStringAsync();
            var pi = JsonConvert.DeserializeObject<global::Tests.Common.Model.Pims.PaymentInstrument>(resultContent);
            Assert.AreEqual(expectedPI.PaymentInstrumentId, pi.PaymentInstrumentId);
            Assert.IsNotNull(pi.PaymentInstrumentDetails.DefaultDisplayName);

            // Make sure the property name in the displaypages matches the data source
            var pidlResourceList = ReadSinglePidlResourceFromJson(await result.Content.ReadAsStringAsync());
            Assert.IsNotNull(pidlResourceList.ClientAction, "Client action missing");
            Assert.AreEqual(ClientActionType.Pidl, pidlResourceList.ClientAction.ActionType);
            Assert.IsNotNull(pidlResourceList.ClientAction.Context, "Client action context missing");

            var pidlResource = pidlResourceList.ClientAction.Context as List<PIDLResource>;
            Assert.IsNotNull(pidlResource[0].DataDescription.ContainsKey(key));
            Assert.AreEqual(key, pidlResource[0].GetDisplayHintById(displayHintId).PropertyName);
            PXSettings.PimsService.ResetToDefaults();
        }

        [DataRow("cart", "us")]
        [DataRow("webblends", "us")]
        [DataRow("amcweb", "us")]
        [DataRow("oxowebdirect", "us")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_Venmo_AddPI_Success_ActivePI_PostPI_ListModernPI(string partner, string country)
        {
            object requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "venmo",
                details = new
                {
                    address = new
                    {
                        address_line1 = "One Microsoft Way",
                        city = "Redmond",
                        region = "WA",
                        country = "US",
                        postal_code = "11111"
                    },
                },
                riskData = new { }
            };

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi014-Venmo");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}", new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK, "Expected 200 OK response");

            string responseContent = await result.Content.ReadAsStringAsync();
            PaymentInstrument pi = JsonConvert.DeserializeObject<PaymentInstrument>(responseContent);
            PaymentMethod pm = pi.PaymentMethod;

            Assert.IsNotNull(pi, "Expected payment instrument to not be null");
            Assert.AreEqual(PaymentInstrumentStatus.Active, pi.Status, "Payment Instrument status expected to be 'Active'");
            Assert.IsNotNull(pm, "Expected payment method to not be null");
            Assert.AreEqual("ewallet", pm.PaymentMethodFamily, "PaymentMethodFamily expected to be ewallet");
            Assert.AreEqual("venmo", pm.PaymentMethodType, "PaymentMethodType expected to be venmo");
        }

        [DataRow("playxbox", "us")]
        [TestMethod]
        public async Task PaymentInstrumentsEx_Venmo_AddPI_Success_ActivePI_PostPI_ListModernPI_PlayXbox_withDefaultTemplate(string partner, string country)
        {
            object requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "venmo",
                details = new
                {
                    address = new
                    {
                        address_line1 = "One Microsoft Way",
                        city = "Redmond",
                        region = "WA",
                        country = "US",
                        postal_code = "11111"
                    },
                },
                riskData = new { }
            };

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi014-Venmo");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));
            PXSettings.PartnerSettingsService.ArrangeResponse(Constants.PSSMockResponses.PXPartnerSettingsPlayXbox);

            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}", new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK, "Expected 200 OK response");

            string responseContent = await result.Content.ReadAsStringAsync();
            PaymentInstrument pi = JsonConvert.DeserializeObject<PaymentInstrument>(responseContent);
            PaymentMethod pm = pi.PaymentMethod;

            Assert.IsNotNull(pi, "Expected payment instrument to not be null");
            Assert.AreEqual(PaymentInstrumentStatus.Active, pi.Status, "Payment Instrument status expected to be 'Active'");
            Assert.IsNotNull(pm, "Expected payment method to not be null");
            Assert.AreEqual("ewallet", pm.PaymentMethodFamily, "PaymentMethodFamily expected to be ewallet");
            Assert.AreEqual("venmo", pm.PaymentMethodType, "PaymentMethodType expected to be venmo");
        }

        [DataRow("webblends", "us")]
        [DataRow("webblends", "us", true)]
        [DataRow("defaulttemplate", "us")]
        [DataRow("onepage", "us")]
        [DataRow("twopage", "us")]
        [DataRow("officesmb", "us")]
        [DataRow("officesmb", "us", true)]
        [TestMethod]
        public async Task PaymentInstrumentsEx_Venmo_AddPI_ExternalRedirect_PendingPI_PostPI_ListModernPI(string partner, string country, bool usePartnerSettingsService = false)
        {
            // Arrange
            object requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "venmo",
                details = new
                {
                    address = new
                    {
                        address_line1 = "One Microsoft Way",
                        city = "Redmond",
                        region = "WA",
                        country = "US",
                        postal_code = "11111"
                    },
                },
                riskData = new { }
            };

            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi015-VenmoRedirect");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));
            HttpResponseMessage result;

            if (usePartnerSettingsService)
            {
                var pssResponse = new
                {
                    add = new
                    {
                        template = "defaultTemplate", 
                        redirectionPattern = "fullPage"
                    }
                };

                PXSettings.PartnerSettingsService.ArrangeResponse(JsonConvert.SerializeObject(pssResponse));
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                    Method = HttpMethod.Post,
                    Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
                };

                request.Headers.Add("x-ms-flight", "PXUsePartnerSettingsService,PXDisablePSSCache");

                // Act
                result = await PXClient.SendAsync(request);
            }
            else
            {
                result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}", new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));
            }

            // Assert
            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK, "Expected 200 OK response");

            string responseContent = await result.Content.ReadAsStringAsync();

            PaymentInstrument pi = JsonConvert.DeserializeObject<PaymentInstrument>(responseContent);
            Assert.IsNotNull(pi, "Expected payment instrument to not be null");
            Assert.AreEqual(PaymentInstrumentStatus.Pending, pi.Status, "Payment Instrument status expected to be 'Pending'");

            PaymentMethod pm = pi.PaymentMethod;
            Assert.IsNotNull(pm, "Expected payment method to not be null");
            Assert.AreEqual("ewallet", pm.PaymentMethodFamily, "PaymentMethodFamily expected to be ewallet");
            Assert.AreEqual("venmo", pm.PaymentMethodType, "PaymentMethodType expected to be venmo");

            PIDLResource pidl = ReadSinglePidlResourceFromJson(responseContent);
            Assert.IsNotNull(pidl.ClientAction, "Client action missing");
            Assert.AreEqual(ClientActionType.Redirect, pidl.ClientAction.ActionType);
            Assert.IsNotNull(pidl.ClientAction.Context, "Client action context missing");

            JObject actionContext = pidl.ClientAction.Context as JObject;
            string baseUrl = actionContext["baseUrl"].ToString();
            Assert.IsNotNull(baseUrl, "baseUrl should not be null");
            Assert.IsTrue(baseUrl.Contains("https://mockRedirectUrl.com/"));

            List<PIDLResource> pidlList = pidl.ClientAction.RedirectPidl as List<PIDLResource>;
            PIDLResource pidlResource = pidlList[0];
            ButtonDisplayHint venmoYesButtonHintId = pidlResource.GetDisplayHintById("venmoYesButton") as ButtonDisplayHint;
            Assert.IsNotNull(venmoYesButtonHintId, "VenmoYesButton should be present in the PIDL.");

            var context = venmoYesButtonHintId.Action.Context.ToString();
            Assert.IsTrue(context.Contains($"partner={partner}"), "The action context URL should contain the original partner name.");

            List<string> displayHintIds = new List<string>
            {
                "venmoLogo",
                "venmoRedirectHeading",
                "venmoRedirectHeadingGroup",
                "venmoRedirectMessage",
                "venmoRedirectText1",
                "venmoRedirectText2",
                "venmoRedirectLink",
                "venmoRedirectTextGroup",
                "venmoYesButton",
                "venmoNoButton",
                "cancelButtonFullSize",
                "venmoRedirectButtonGroup",
            };

            foreach (string displayHintId in displayHintIds)
            {
                Assert.IsNotNull(pidlResource.GetDisplayHintById(displayHintId), $"DisplayHint with id {displayHintId} is missing.");
            }
        }

        /// <summary>
        /// Verifies PayPal retry page behavior when adding a pending Payment Instrument (PI).
        /// </summary>
        /// <param name="partner"></param>
        /// <param name="country"></param>
        /// <returns></returns>
        [DataRow("defaultTemplate", "us")]
        [DataRow("onepage", "us")]
        [DataRow("twopage", "us")]
        [DataRow("selectpmbuttonlist", "us")]
        [DataRow("officesmb", "us")]
        [DataRow("listpidropdown", "us")]
        [TestMethod]
        public async Task Paypal_AddPI_ExternalRetry_PendingPI_GetModernPI(string partner, string country)
        {
            // Arrange
            string pendingPiId = "Account001-Pi009-PaypalRedirect";
            string headers = null;

            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", pendingPiId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            var partnerSettingResponse = new
            {
                add = new
                {
                    template = "defaulttemplate",
                    redirectionPattern = "fullPage"
                }
            };

            headers = string.Equals(partner, "officesmb", StringComparison.OrdinalIgnoreCase) ? "PXDisablePSSCache" : "PXUsePartnerSettingsService,PXDisablePSSCache";

            PXSettings.PartnerSettingsService.ArrangeResponse(JsonConvert.SerializeObject(partnerSettingResponse));

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx/{pendingPiId}?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Get
            };

            request.Headers.Add("x-ms-flight", headers);

            // Act
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert
            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK, "Expected 200 OK response");

            string responseContent = await result.Content.ReadAsStringAsync();

            PaymentInstrument pi = JsonConvert.DeserializeObject<PaymentInstrument>(responseContent);
            Assert.IsNotNull(pi, "Expected payment instrument to not be null");
            Assert.AreEqual(PaymentInstrumentStatus.Pending, pi.Status, "Payment Instrument status expected to be 'Pending'");

            PaymentMethod pm = pi.PaymentMethod;
            Assert.IsNotNull(pm, "Expected payment method to not be null");
            Assert.AreEqual("ewallet", pm.PaymentMethodFamily, "PaymentMethodFamily expected to be ewallet");
            Assert.AreEqual("paypal", pm.PaymentMethodType, "PaymentMethodType expected to be paypal");

            PIDLResource pidl = ReadSinglePidlResourceFromJson(responseContent);
            Assert.IsNotNull(pidl.ClientAction, "Client action missing");
            Assert.AreEqual(ClientActionType.Pidl, pidl.ClientAction.ActionType);
            Assert.IsNotNull(pidl.ClientAction.Context, "Client action context missing");

            PIDLResource pidlResource = (pidl.ClientAction.Context as List<PIDLResource>)[0];
            List<string> displayHintIds = new List<string>
            {
                "paypalRetryHeading",
                "paypalRetryMessage",
                "paypalTryAgainButton",
                "paypalTryAnotherWayButton",
            };

            foreach (string displayHintId in displayHintIds)
            {
                Assert.IsNotNull(pidlResource.GetDisplayHintById(displayHintId), $"DisplayHint with id {displayHintId} is missing.");
            }
        }

        /// <summary>
        /// This CIT verifies the flow when the `paymentInstrumentEx` call is made in a GET request and the PIMS status is pending.
        /// In this scenario, instead of retrieving the PIDL, the user will be prompted to provide an address.
        /// </summary>
        /// <param name="partner"></param>
        /// <param name="country"></param>
        /// <param name="pendingPiId"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        [DataRow("copilotProSubscription", "us", "Account001-Pi008-Paypal", null)]
        [DataRow("copilotProSubscription", "us", "Account001-Pi008-Paypal", "PXUseJarvisV3ForCompletePrerequisites")]
        [TestMethod]
        public async Task Validate_GetModernPIForHandleProfileAddress(string partner, string country, string pendingPiId = null, string headers = null)
        {
            // Arrange
            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", pendingPiId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            var partnerSettingResponse = new
            {
                add = new
                {
                    template = "defaulttemplate",
                    redirectionPattern = "fullPage"
                }
            };
               
            PXSettings.PartnerSettingsService.ArrangeResponse(JsonConvert.SerializeObject(partnerSettingResponse));

            // Providing accountService data to ensure the account profile in the HandleProfileAddress function returns null values,
            // allowing the code to proceed to ClientActionFactory and invoke Add Profile Address ClientAction To PaymentInstrument.
            PXSettings.AccountsService.ArrangeResponse("{\"original_address\":{\"country\":\"US\",\"region\":\"wa\",\"city\":\"Redmond\",\"address_line1\":\"One Microsoft Way\",\"postal_code\":\"98052\"},\"suggested_address\":{\"country\":\"US\",\"region\":\"WA\",\"city\":\"Redmond\",\"address_line1\":\"1 Microsoft Way\",\"postal_code\":\"98052-8300\"},\"status\":\"Verified\",\"validation_message\":\"\"}");

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx/{pendingPiId}?country={country}&language=en-US&partner={partner}&completePrerequisites=true")),
                Method = HttpMethod.Get
            };

            request.Headers.Add("x-ms-flight", headers);

            // Act
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert
            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK, "Expected 200 OK response");

            string responseContent = await result.Content.ReadAsStringAsync();

            PaymentInstrument pi = JsonConvert.DeserializeObject<PaymentInstrument>(responseContent);
            Assert.IsNotNull(pi, "Expected payment instrument to not be null");
            Assert.AreEqual(PaymentInstrumentStatus.Active, pi.Status, "Payment Instrument status expected to be 'Pending'");

            PaymentMethod pm = pi.PaymentMethod;
            Assert.IsNotNull(pm, "Expected payment method to not be null");
            Assert.AreEqual("ewallet", pm.PaymentMethodFamily, "PaymentMethodFamily expected to be ewallet");
            Assert.AreEqual("paypal", pm.PaymentMethodType, "PaymentMethodType expected to be paypal");

            PIDLResource pidl = ReadSinglePidlResourceFromJson(responseContent);
            Assert.IsNotNull(pidl.ClientAction, "Client action missing");
            Assert.AreEqual(ClientActionType.Pidl, pidl.ClientAction.ActionType);
            Assert.IsNotNull(pidl.ClientAction.Context, "Client action context missing");

            PIDLResource pidlResource = (pidl.ClientAction.Context as List<PIDLResource>)[0];
            Assert.IsNotNull(pidlResource);
            Assert.IsNotNull(pidlResource.DisplayPages);

            pidlResource.DataDescription.TryGetValue("addressType", out object addressTypePropertyNameObj);
            PropertyDescription addressTypePropertyName = addressTypePropertyNameObj as PropertyDescription;
            Assert.IsNotNull(addressTypePropertyName.DefaultValue.ToString());
            Assert.AreEqual(addressTypePropertyName.DefaultValue.ToString(), Constants.AddressTypes.Billing);

            PXSettings.PimsService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
        }

        /// <summary>
        /// This method performs a PayPal polling operation to obtain a modern payment instrument (PI).
        /// It sends a request to the PimsService API with the specified partner, country, and session ID.
        /// The response is validated using assertions to ensure the expected data is present. Additional assertions are performed for the PayPal QR Code challenge.
        /// Finally, the method verifies the redirect URL for the obtained PI.
        /// </summary>
        /// <param name="partner">The partner value.</param>
        /// <param name="country">The country value.</param>
        /// <param name="expectedActionType">The expected action type.</param>
        /// <returns></returns>
        [DataRow("defaulttemplate", "us", ClientActionType.Pidl)]
        [DataRow("onepage", "us", ClientActionType.Pidl)]
        [DataRow("twopage", "us", ClientActionType.Pidl)]
        [DataRow("selectpmbuttonlist", "us", ClientActionType.Pidl)]
        [DataRow("officesmb", "us", ClientActionType.Pidl)]
        [DataRow("listpidropdown", "us", ClientActionType.Pidl)]
        [DataTestMethod]
        public async Task GetModernPi_PaypalPolling(string partner, string country, ClientActionType expectedActionType)
        {
            // Arrange
            bool assertCalled = false;
            List<PIDLResource> resourceList = null;
            var sessionId = "f634e160-96b2-a926-1c4b-86eda833cf27";
            var pimsRequestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "paypal",
                sessionId = sessionId,
                details = new
                {
                    address = new
                    {
                        address_line1 = "One Microsoft Way",
                        city = "Redmond",
                        region = "WA",
                        country = "US",
                        postal_code = "11111"
                    },
                },
                riskData = new { }
            };

            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi009-PaypalRedirect");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXSettings.PimsService.PreProcess = async (request) =>
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(requestContent);
                Assert.IsTrue(JObject.DeepEquals(json, JObject.FromObject(pimsRequestBody)), "Expected request data is not found");
                assertCalled = true;
            };

            // Act
            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}", new StringContent(JsonConvert.SerializeObject(pimsRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            string resultContent = await result.Content.ReadAsStringAsync();
            PIDLResource resource = ReadSinglePidlResourceFromJson(resultContent);
            ClientAction clientAction = resource.ClientAction;

            // Assert
            Assert.IsTrue(assertCalled, "PimsTestHandler.PreProcess wasn't called");
            Assert.IsNotNull(clientAction);
            Assert.IsNotNull(clientAction.Context);
            Assert.AreEqual(expectedActionType, clientAction.ActionType);
            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK);

            resourceList = clientAction.Context as List<PIDLResource>;

            // Assert
            Assert.IsNotNull(resourceList);
            Assert.IsTrue(resultContent.Contains("paypalHeading"), "No paypalHeading text found");
            Assert.IsTrue(resultContent.Contains("paypalLogo"), "No paypal Logo were found");
            Assert.IsTrue(resultContent.Contains("paypalQrCodeChallengeLoginRedirectionLink"), "No paypalQrCodeChallengeLoginRedirectionLink text found");
            Assert.IsTrue(resultContent.Contains("paypalQrCodeChallengeLoginRedirectionLinkText2"), "No paypalQrCodeChallengeLoginRedirectionLinkText2 text found");
            Assert.IsTrue(resultContent.Contains("qrCodeChallengeImageText"), "No qrCodeChallengeImageText text found");
            Assert.IsTrue(resultContent.Contains("qrCodeChallengeSignInDeviceText"), "No qrCodeChallengeSignInDeviceText text found");
            Assert.IsTrue(resultContent.Contains("paypalQrCodeChallengePageRefreshText"), "No paypalQrCodeChallengePageRefreshText text found");
            Assert.IsTrue(resultContent.Contains("paypalQrCodeImage"), "No image hint id found");
            Assert.IsTrue(resultContent.Contains("paypalQrCodeChallengePageBackButton"), "No back button hint id found");

            PIDLResource contextPidl = resourceList[0];
            DisplayHintAction action = contextPidl.DisplayPages[0].Action;

            // Assert
            Assert.IsNotNull(contextPidl);
            Assert.AreEqual("poll", action.ActionType);
            Assert.IsNotNull(action.Context);
            Assert.IsTrue(resultContent.Contains("https://{pifd-endpoint}/users/{userId}/paymentInstrumentsEx/Account001-Pi009-PaypalRedirect?"), "No redirect URL found");
            PXSettings.PimsService.ResetToDefaults();
        }

        /// <summary>
        /// This test is used to verify the Ideal billing agreement, client action value for the redirection pattern.
        /// Note : The webblends and northstarweb partner are not using the parnter settings service and hence the rediectionPatternType
        /// value is provided only to check the if condtions have redirect pidl or not.
        /// </summary>
        /// <param name="partner">Check the partner used</param>
        /// <param name="redirectionPattern">Redirection pattern have value based on the PSS setting.</param>
        /// <returns></returns>
        [DataRow("officesmb", "inline")]
        [DataRow("officesmb", "fullPage")]
        [DataRow("webblends", "fullPage")]
        [DataRow("northstarweb", "inline")]
        [DataTestMethod]
        public async Task GetModernPi_idealBillingForValidateClientActionRedirection(string partner, string redirectionPatternType)
        {
            // Arrange
            var headers = new Dictionary<string, string>();
            var requestBody = new
            {
                paymentMethodFamily = "direct_debit",
                paymentMethodType = "ideal_billing_agreement",
                paymentMethodOperation = "add",
                paymentMethodCountry = "nl",
                paymentMethodResource_id = "direct_debit.ideal_billing_agreement",

                context = "purchase",
                sessionId = Guid.NewGuid().ToString(),
                details = new
                {
                    dataType = "ideal_billing_agreement_details",
                    dataOperation = "add",
                    dataCountry = "nl"
                }
            };

            string country = "nl";
            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account009", "Account009-DirectDebitIdealBilling");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            if (string.Equals(partner, "officesmb", StringComparison.OrdinalIgnoreCase))
            {
                var partnerSettingResponse = new
                {
                    add = new
                    {
                        template = "defaulttemplate",
                        redirectionPattern = redirectionPatternType
                    }
                };

                headers = new Dictionary<string, string>
                {
                    {
                        "x-ms-flight", "PXDisablePSSCache"
                    }
                };

                PXSettings.PartnerSettingsService.ArrangeResponse(JsonConvert.SerializeObject(partnerSettingResponse));
            }

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            // Act
            headers?.ToList().ForEach(header => request.Headers.Add(header.Key, header.Value));
            HttpResponseMessage result = await PXClient.SendAsync(request);

            // Assert
            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK, "Expected 200 OK response");

            string responseContent = await result.Content.ReadAsStringAsync();
            PIDLResource pidl = ReadSinglePidlResourceFromJson(responseContent);

            Assert.IsNotNull(pidl.ClientAction, "Client action missing");
            Assert.AreEqual(ClientActionType.Redirect, pidl.ClientAction.ActionType);
            Assert.IsNotNull(pidl.ClientAction.Context, "Client action context missing");

            // The pidl have some value only when the redirection is fullpage
            if (redirectionPatternType.Contains("fullpage"))
            {
                Assert.IsNotNull(pidl.ClientAction.RedirectPidl, "Client action redirection pidl missing");
                PIDLResource pidlResource = (pidl.ClientAction.RedirectPidl as List<PIDLResource>)[0];
                List<string> displayHintIds = new List<string>();

                if (string.Equals(partner, "officesmb", StringComparison.OrdinalIgnoreCase))
                {
                    displayHintIds = new List<string>
                    {
                        "idealredirectHeading",
                        "idealredirectMessageLine1Line2Group",
                        "idealredirectMessageLine3TextGroup",
                        "idealCancelSuccessGroup",
                    };
                }
                else
                {
                    displayHintIds = new List<string>
                    {
                        "idealredirectHeading",
                        "idealredirectMessageLine1",
                        "idealredirectMessageLine2TextGroup",
                        "idealCancelSuccessGroup",
                    };
                }

                foreach (string displayHintId in displayHintIds)
                {
                    Assert.IsNotNull(pidlResource.GetDisplayHintById(displayHintId), $"DisplayHint with id {displayHintId} is missing.");
                }
            }
        }

        [DataRow("webblends", "us")]
        [DataRow("webblends", "us", true)]
        [DataRow("defaulttemplate", "us")]
        [DataRow("onepage", "us")]
        [DataRow("twopage", "us")]
        [DataRow("officesmb", "us")]
        [DataRow("defaulttemplate", "us", true)]
        [DataRow("copilotProSubscription", "us", true)]
        [DataRow("officesmb", "us", true)]
        [TestMethod]
        public async Task PaymentInstrumentsEx_Venmo_AddPI_ExternalRedirect_PendingPI_GetModernPI(string partner, string country, bool usePartnerSettingsService = false)
        {
            // Arrange
            string pendingPiId = "Account001-Pi015-VenmoRedirect";

            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", pendingPiId);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            HttpResponseMessage result = null;

            if (usePartnerSettingsService)
            {
                // The flight 'PxEnableVenmo' will be added by leveraging the feature 'PxEnableVenmo' below.
                string pssResponse = "{\"add\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"fullPage\",\"resources\":null,\"features\":{\"threeDSOne\":{\"applicableMarkets\":[\"in\"],\"displayCustomizationDetail\":null},\"PXEnableVenmo\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"PxEnableSelectPMAddPIVenmo\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]},\"PXPassUserAgentToPIMSForAddUpdatePI\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"usePSSForPXFeatureFlighting\":true}]}}},\"default\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"fullPage\",\"resources\":null,\"features\":null}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(pssResponse);
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account001/paymentInstrumentsEx/{pendingPiId}?country={country}&language=en-US&partner={partner}")),
                    Method = HttpMethod.Get
                };

                request.Headers.Add("x-ms-flight", "PXUsePartnerSettingsService,PXDisablePSSCache");

                // Act
                result = await PXClient.SendAsync(request);
            }
            else
            {
                PXFlightHandler.AddToEnabledFlights("PxEnableVenmo");
                result = await PXClient.GetAsync($"/v7.0/Account001/paymentInstrumentsEx/{pendingPiId}?country={country}&language=en-US&partner={partner}");
            }

            // Assert
            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK, "Expected 200 OK response");

            string responseContent = await result.Content.ReadAsStringAsync();

            PaymentInstrument pi = JsonConvert.DeserializeObject<PaymentInstrument>(responseContent);
           
            // Note: To work with Venmo, the flight 'PxEnableVenmo' must be enabled. We add this flight as a feature in each flow using the function 'this.EnableFlightingsInPartnerSetting'.
            // When the partner has PSS settings, this function adds the feature under the exposed flight name.
            // If the partner is 'defaulttemplate', the PSS setting will be empty ('setting = null'), resulting in no flight being added for the Venmo flow. This will cause the creation of PIMS to fail, and consequently, no PIDL form will be generated.
            if (string.Equals(partner, "defaulttemplate", StringComparison.OrdinalIgnoreCase) && usePartnerSettingsService)
            {
                Assert.IsNull(pi, "Expected payment instrument to be null");
            }
            else
            {
                Assert.IsNotNull(pi, "Expected payment instrument to not be null");
                Assert.AreEqual(PaymentInstrumentStatus.Pending, pi.Status, "Payment Instrument status expected to be 'Pending'");

                PaymentMethod pm = pi.PaymentMethod;
                Assert.IsNotNull(pm, "Expected payment method to not be null");
                Assert.AreEqual("ewallet", pm.PaymentMethodFamily, "PaymentMethodFamily expected to be ewallet");
                Assert.AreEqual("venmo", pm.PaymentMethodType, "PaymentMethodType expected to be venmo");

                PIDLResource pidl = ReadSinglePidlResourceFromJson(responseContent);
                Assert.IsNotNull(pidl.ClientAction, "Client action missing");
                Assert.AreEqual(ClientActionType.Pidl, pidl.ClientAction.ActionType);
                Assert.IsNotNull(pidl.ClientAction.Context, "Client action context missing");

                PIDLResource pidlResource = (pidl.ClientAction.Context as List<PIDLResource>)[0];
                List<string> displayHintIds = new List<string>
                {
                    "venmoRetryHeading",
                    "venmoRetryMessage",
                    "venmoTryAnotherWayButton",
                    "venmoTryAgainButton",
                    "venmoRetryButtonGroup",
                };

                foreach (string displayHintId in displayHintIds)
                {
                    Assert.IsNotNull(pidlResource.GetDisplayHintById(displayHintId), $"DisplayHint with id {displayHintId} is missing.");
                }
            }
        }

        /// <summary>
        /// The test is to add PI without city and state but with AddressEnrichmentService's suggested city and state.
        /// </summary>
        /// <param name="partner"></param>
        /// <param name="suggestCityState"></param>
        /// <returns></returns>
        [DataRow("azure", true)]
        [DataRow("azure", false)]
        [DataRow("webblends", true)]
        [DataRow("webblends", false)]
        [DataRow("commercialstores", true)]
        [DataRow("commercialstores", false)]
        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_AddressNoCityState_AddressEnrichmentServiceSuggestedCityState(string partner, bool suggestCityState)
        {
            // Arrange
            // AddressEnrichmentService could only suggest City and State if the request payload have address_line 1, country, and postal_code. Else, service will return fallback PidlResource with zip code error.
            dynamic requestBody = null;
            if (suggestCityState)
            {
                requestBody = new
                {
                    paymentMethodFamily = "credit_card",
                    paymentMethodType = "visa",
                    details = new
                    {
                        address = new
                        {
                            address_line1 = "One Microsoft Way",
                            country = "US",
                            postal_code = "98052"
                        }
                    }
                };
            }
            else
            {
                requestBody = new
                {
                    paymentMethodFamily = "credit_card",
                    paymentMethodType = "visa",
                    details = new
                    {
                        address = new
                        {
                        }
                    }
                };
            }

            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi001-Visa");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            // Act
            HttpResponseMessage result = await PXClient.PostAsync($"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}&scenario=addressnocitystate&completeprerequisites=true", new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType));

            string resultContent = await result.Content.ReadAsStringAsync();
            var pi = JsonConvert.DeserializeObject<global::Tests.Common.Model.Pims.PaymentInstrument>(resultContent);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Expected 200 OK");
            if (suggestCityState)
            {
                // AddressEnrichmentService has successfully returned a single city/state combination
                Assert.AreEqual(expectedPI.PaymentInstrumentId, pi.PaymentInstrumentId, "Payment Instrument Id is not as expected");
                Assert.AreEqual(expectedPI.PaymentInstrumentAccountId, pi.PaymentInstrumentAccountId, "Payment Instrument Account Id is not as expected");
                Assert.IsNotNull(pi.PaymentInstrumentDetails.DefaultDisplayName);
                Assert.IsNotNull(pi.PaymentInstrumentDetails.Address.AddressLine1);
                Assert.IsNotNull(pi.PaymentInstrumentDetails.Address.City);
                Assert.IsNotNull(pi.PaymentInstrumentDetails.Address.State);
                Assert.IsNotNull(pi.PaymentInstrumentDetails.Address.Country);
                Assert.IsNotNull(pi.PaymentInstrumentDetails.Address.Zip);
            }
            else
            {
                // AddressEnrichmentService was not able to return a single city/state combination because requestBody doesn't have the addressline_1, hence fallback PidlResource will be returned with InvalidZipCode error.
                PIDLError pidlError = JsonConvert.DeserializeObject<PIDLError>(pi.ClientAction.PidlError.ToString());
                Assert.IsNotNull(pidlError);
                Assert.IsNull(pi.PaymentInstrumentId);
                Assert.IsNull(pi.PaymentInstrumentDetails);
                Assert.IsNull(pi.PaymentInstrumentAccountId);
                Assert.AreEqual("InvalidZipCode", pidlError.Code, "Code in PidlError is not as expected");
            }
        }

        /// <summary>
        /// The test is to verify the exceptions thrown when adding payment instruments.
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="requestBody"></param>
        /// <param name="requestHeaders"></param>
        /// <param name="flightName"></param>
        /// <param name="errorContent"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        [DataRow("/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner=webblends", "", null, null, null, "response status code: InvalidPIData, error: The input PI data is invalid.")] // Add PI without PI data
        [DataRow("/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner=webblends", "{}", null, null, null, "response status code: InvalidPIData, error: paymentMethodFamily is missing")] // Add PI without payment method family
        [DataRow("/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner=webblends", "{'paymentMethodFamily':'credit_card'}", null, null, null, "response status code: InvalidPIData, error: paymentMethodType is missing")] // Add PI without payment method type

        /// Add PI to throw PIMS validation error messages
        [DataRow("/v7.0/8e342cdc-771b-4b19-84a0-bef4c44911f7/paymentInstrumentsEx?country=us&language=en-US&partner=webblends", "{'paymentMethodType':'unionpay_creditcard','paymentMethodFamily':'credit_card'}", null, null, "{'ErrorCode':'ValidationFailed','Message':'The payment instrument cannot be validated.Please contact the payment processor for help.','Details':[]}", "ValidationFailed")]  // Add PI to throw ValidationFailed error message
        [DataRow("/v7.0/8e342cdc-771b-4b19-84a0-bef4c44911f7/paymentInstrumentsEx?country=us&language=en-US&partner=webblends", "{'paymentMethodType':'unionpay_creditcard','paymentMethodFamily':'credit_card'}", null, null, "{'ErrorCode':'TooManyOperations','Message':'The payment instrument cannot be validated.Please contact the payment processor for help.','Details':[]}", "TooManyOperations")] // Add PI to throw TooManyOperations error message
        [DataRow("/v7.0/8e342cdc-771b-4b19-84a0-bef4c44911f7/paymentInstrumentsEx?country=us&language=en-US&partner=webblends", "{'paymentMethodType':'visa','paymentMethodFamily':'credit_card'}", null, null, "{'ErrorCode':'PrepaidCardNotSupported','Message':'The payment instrument cannot be validated.Please contact the payment processor for help.','Details':[]}", "PrepaidCardNotSupported")] // Add PI to throw PrepaidCardNotSupported error message
        [DataRow("/v7.0/8e342cdc-771b-4b19-84a0-bef4c44911f7/paymentInstrumentsEx?country=us&language=en-US&partner=webblends", "{'paymentMethodType':'paypal','paymentMethodFamily':'ewallet'}", null, null, "{'ErrorCode':'IncorrectCredential','Message':'The payment instrument cannot be validated.Please contact the payment processor for help.','Details':[]}", "IncorrectCredential")] // Add PI to throw IncorrectCredential error message
        [DataRow("/v7.0/8e342cdc-771b-4b19-84a0-bef4c44911f7/paymentInstrumentsEx?country=us&language=en-US&partner=webblends", "{'paymentMethodType':'sepa','paymentMethodFamily':'direct_debit'}", null, null, "{'ErrorCode':'OperationNotSupported','Message':'The payment instrument cannot be validated.Please contact the payment processor for help.','Details':[]}", "ObjectDisposedException")] // Add PI to throw OperationNotSupported error message
        [DataRow("/v7.0/8e342cdc-771b-4b19-84a0-bef4c44911f7/paymentInstrumentsEx?country=us&language=en-US&partner=webblends", "{'paymentMethodType':'sepa','paymentMethodFamily':'direct_debit'}", null, null, "{'ErrorCode':'InvalidBankCode','Message':'The payment instrument cannot be validated.Please contact the payment processor for help.','Details':[]}", "InvalidBankCode")] // Add PI to throw InvalidBankCode validation error message
        [DataRow("/v7.0/8e342cdc-771b-4b19-84a0-bef4c44911f7/paymentInstrumentsEx?country=us&language=en-US&partner=defaulttemplate", "{'paymentMethodType':'sepa','paymentMethodFamily':'direct_debit'}", null, null, "{'ErrorCode':'OperationNotSupported','Message':'The payment instrument cannot be validated.Please contact the payment processor for help.','Details':[]}", "ObjectDisposedException")] // Add PI to throw OperationNotSupported error message
        [DataRow("/v7.0/8e342cdc-771b-4b19-84a0-bef4c44911f7/paymentInstrumentsEx?country=us&language=en-US&partner=defaulttemplate", "{'paymentMethodType':'sepa','paymentMethodFamily':'direct_debit'}", null, null, "{'ErrorCode':'InvalidBankCode','Message':'The payment instrument cannot be validated.Please contact the payment processor for help.','Details':[]}", "InvalidBankCode")] // Add PI to throw InvalidBankCode validation error message
        [DataRow("/v7.0/8e342cdc-771b-4b19-84a0-bef4c44911f7/paymentInstrumentsEx?country=us&language=en-US&partner=officesmb", "{'paymentMethodType':'sepa','paymentMethodFamily':'direct_debit'}", null, null, "{'ErrorCode':'OperationNotSupported','Message':'The payment instrument cannot be validated.Please contact the payment processor for help.','Details':[]}", "ObjectDisposedException")] // Add PI to throw OperationNotSupported error message
        [DataRow("/v7.0/8e342cdc-771b-4b19-84a0-bef4c44911f7/paymentInstrumentsEx?country=us&language=en-US&partner=officesmb", "{'paymentMethodType':'sepa','paymentMethodFamily':'direct_debit'}", null, null, "{'ErrorCode':'InvalidBankCode','Message':'The payment instrument cannot be validated.Please contact the payment processor for help.','Details':[]}", "InvalidBankCode")] // Add PI to throw InvalidBankCode validation error message
        [DataRow("/v7.0/8e342cdc-771b-4b19-84a0-bef4c44911f7/paymentInstrumentsEx?country=us&language=en-US&partner=webblends", "{'paymentMethodType':'ach','paymentMethodFamily':'direct_debit'}", null, null, "{'ErrorCode':'InvalidBankCode','Message':'The payment instrument cannot be validated.Please contact the payment processor for help.','Details':[]}", "InvalidBankCode")] // Add PI to throw InvalidBankCode validation error message
        [DataRow("/v7.0/8e342cdc-771b-4b19-84a0-bef4c44911f7/paymentInstrumentsEx?country=us&language=en-US&partner=webblends", "{'paymentMethodType':'unionpay_creditcard','paymentMethodFamily':'credit_card'}", null, null, "{'ErrorCode':'InvalidPaymentInstrumentInfo','Message':'The payment instrument cannot be validated.Please contact the payment processor for help.','Details':[]}", "InvalidPaymentInstrumentInfo")] // Add PI to throw InvalidPaymentInstrumentInfo validation error message
        [DataRow("/v7.0/8e342cdc-771b-4b19-84a0-bef4c44911f7/paymentInstrumentsEx?country=us&language=en-US&partner=webblends", "{'paymentMethodType':'visa','paymentMethodFamily':'credit_card'}", null, null, "{'ErrorCode':'InvalidPaymentInstrumentInfo','Message':'The payment instrument cannot be validated.Please contact the payment processor for help.','Details':[]}", "InvalidPaymentInstrumentInfo")] // Add PI to throw InvalidPaymentInstrumentInfo validation error message
        [DataRow("/v7.0/8e342cdc-771b-4b19-84a0-bef4c44911f7/paymentInstrumentsEx?country=us&language=en-US&partner=webblends", "{'paymentMethodType':'unionpay_creditcard','paymentMethodFamily':'credit_card'}", null, null, "{'ErrorCode':'OtherErrorCode','Message':'The payment instrument cannot be validated.Please contact the payment processor for help.','Details':[]}", "OtherErrorCode")] // Add PI to throw other validation error messages
        [DataRow("/v7.0/8e342cdc-771b-4b19-84a0-bef4c44911f7/paymentInstrumentsEx?country=us&language=en-US&partner=webblends", "{'paymentMethodType':'paypal','paymentMethodFamily':'ewallet'}", null, null, "{'ErrorCode':'OtherErrorCode','Message':'The payment instrument cannot be validated.Please contact the payment processor for help.','Details':[]}", "OtherErrorCode")] // Add PI to throw other validation error messages
        [DataRow("/v7.0/8e342cdc-771b-4b19-84a0-bef4c44911f7/paymentInstrumentsEx?country=us&language=en-US&partner=webblends", "{'paymentMethodType':'ach','paymentMethodFamily':'direct_debit'}", null, null, "{'ErrorCode':'OtherErrorCode','Message':'The payment instrument cannot be validated.Please contact the payment processor for help.','Details':[]}", "OtherErrorCode")] // Add PI to throw other validation error messages
        [DataRow("/v7.0/8e342cdc-771b-4b19-84a0-bef4c44911f7/paymentInstrumentsEx?country=us&language=en-US&partner=webblends", "{'paymentMethodType':'alipay_billing_agreement','paymentMethodFamily':'ewallet'}", null, null, "{'ErrorCode':'OtherErrorCode','Message':'The payment instrument cannot be validated.Please contact the payment processor for help.','Details':[]}", "OtherErrorCode")] // Add PI to throw other validation error messages
        [DataRow("/v7.0/8e342cdc-771b-4b19-84a0-bef4c44911f7/paymentInstrumentsEx?country=us&language=en-US&partner=webblends", "{'paymentMethodType':'vzw-us-nonsim','paymentMethodFamily':'mobile_billing_non_sim'}", null, null, "{'ErrorCode':'OtherErrorCode','Message':'The payment instrument cannot be validated.Please contact the payment processor for help.','Details':[]}", "OtherErrorCode")] // Add PI to throw other validation error messages
        [DataRow("/v7.0/8e342cdc-771b-4b19-84a0-bef4c44911f7/paymentInstrumentsEx?country=us&language=en-US&partner=webblends", "{'paymentMethodType':'klarna','paymentMethodFamily':'invoice_credit'}", null, null, "{'ErrorCode':'OtherErrorCode','Message':'The payment instrument cannot be validated.Please contact the payment processor for help.','Details':[]}", "OtherErrorCode")] // Add PI to throw other validation error messages
        [DataRow("/v7.0/8e342cdc-771b-4b19-84a0-bef4c44911f7/paymentInstrumentsEx?country=us&language=en-US&partner=officesmb", "{'paymentMethodType':'klarna','paymentMethodFamily':'invoice_credit'}", null, null, "{'ErrorCode':'OtherErrorCode','Message':'The payment instrument cannot be validated.Please contact the payment processor for help.','Details':[]}", "OtherErrorCode")] // Add PI to throw other validation error messages
        [DataRow("/v7.0/8e342cdc-771b-4b19-84a0-bef4c44911f7/paymentInstrumentsEx?country=us&language=en-US&partner=azure", "{'paymentMethodType':'legacy_invoice','paymentMethodFamily':'virtual'}", null, null, "{'ErrorCode':'OtherErrorCode','Message':'The payment instrument cannot be validated.Please contact the payment processor for help.','Details':[]}", "OtherErrorCode")] // Add PI to throw other validation error messages
        [DataRow("/v7.0/8e342cdc-771b-4b19-84a0-bef4c44911f7/paymentInstrumentsEx?country=us&language=en-US&partner=officesmb", "{'paymentMethodType':'legacy_invoice','paymentMethodFamily':'virtual'}", null, null, "{'ErrorCode':'OtherErrorCode','Message':'The payment instrument cannot be validated.Please contact the payment processor for help.','Details':[]}", "OtherErrorCode")] // Add PI to throw other validation error messages
        [TestMethod]
        public async Task PaymentInstrumentsEx_AddPIWithErrorScenarios(string requestUrl, string requestBody, string requestHeaders, string flightName, string errorContent, string errorMessage)
        {
            // Arrange
            string tokenToSelect = "Message";
            HttpStatusCode responseCode = HttpStatusCode.BadRequest;
            var partnerSettingResponse = string.Empty;
            var requestUrlHeaders = new Dictionary<string, string>();

            if (requestUrl.Contains("officesmb"))
            {
                requestUrlHeaders["x-ms-flight"] = "PXDisablePSSCache";

                if (requestBody.Contains("sepa"))
                {
                    partnerSettingResponse = PartnerSettingsServiceMockResponseProvider.GetPSSMockResponseById("Account001-PI001-fullPageRedirectionDefaultTemplate").ToString();
                }
                else
                {
                    partnerSettingResponse = "{\"default\":{\"template\":\"defaulttemplate\",\"features\":{\"enableVirtualFamilyPM\":{\"applicableMarkets\":[]}}},\"add\":{\"template\":\"defaulttemplate\",\"features\":null}}";
                }

                PXSettings.PartnerSettingsService.ArrangeResponse(partnerSettingResponse);
            }

            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl(requestUrl)),
                Method = HttpMethod.Post,
                Content = new StringContent(requestBody, Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            requestUrlHeaders?.ToList().ForEach(requestUrlHeader => request.Headers.Add(requestUrlHeader.Key, requestUrlHeader.Value));

            if (!string.IsNullOrEmpty(flightName))
            {
                PXFlightHandler.AddToEnabledFlights(flightName);
            }

            if (!string.IsNullOrEmpty(requestHeaders))
            {
                JObject headers = JObject.Parse(requestHeaders);
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value.ToString());
                }
            }

            if (!string.IsNullOrEmpty(errorContent))
            {
                tokenToSelect = "ErrorCode";
                PXSettings.PimsService.ArrangeResponse(content: errorContent, statusCode: responseCode);
            }

            // Act
            HttpResponseMessage result = await PXClient.SendAsync(request);

            string resultContent = await result.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(resultContent);
            JToken message = json.SelectToken(tokenToSelect);

            // Assert
            Assert.IsNotNull(message);
            Assert.AreEqual(errorMessage, message.ToString(), "ErrorMessage is not as expected");
            Assert.AreEqual(responseCode, result.StatusCode, "StatusCode is not as expected");

            PXFlightHandler.ResetToDefault();
            PXSettings.PimsService.ResetToDefaults();
        }

        [DataRow(true)]
        [DataRow(false)]
        [DataTestMethod]
        public async Task PaymentInstrumentsEx_ListPI_Venmo(bool exposeVenmoFlight)
        {
            // Arrange
            //// Remove code related to venmo once fully flighted
            if (exposeVenmoFlight)
            {
                PXFlightHandler.AddToEnabledFlights("PxEnableVenmo");
            }

            // Act
            HttpResponseMessage listPi = await PXClient.GetAsync(string.Format("/v7.0/Account001/paymentInstrumentsEx?language=en-US&partner=northstarweb&status=active,removed"));

            // Assert
            //// Remove code related to venmo once fully flighted
            if (exposeVenmoFlight)
            {
                Assert.IsTrue(listPi.Content.ReadAsStringAsync().Result.Contains("Venmo"));
            }
            else
            {
                Assert.IsFalse(listPi.Content.ReadAsStringAsync().Result.Contains("Venmo"));
            }
        }

        [TestMethod]
        public async Task PaymentInstrumentsEx_PXChallenge_Completed()
        {
            PXFlightHandler.AddToEnabledFlights("PXChallengeSwitch");

            // Act
            HttpResponseMessage response = await PXClient.GetAsync(string.Format("/v7.0/Account001/paymentInstrumentsEx?language=en-US&partner=northstarweb&pxSessionId=775f0343-5ee3-468f-aa60-82e9a78472b4"));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Expected 200 OK");
        }

        [DataRow("us", "en-US", "commercialstores", true, "selectinstance", true, "Account001")]
        [DataRow("us", "en-US", "commercialstores", true, "selectinstance", false, "Account001")]
        [DataRow("us", "en-US", "macmanage", true, "selectinstance", true, "Account001")]
        [DataRow("us", "en-US", "macmanage", true, "selectinstance", false, "Account001")]
        [DataRow("us", "en-US", "macmanage", false, "selectinstance", false, "Account001")]
        [DataRow("us", "en-US", "cart", true, "selectinstance", true, "Account001")]
        [DataRow("us", "en-US", "cart", true, "selectinstance", true, "Account011")]
        [DataRow("us", "en-US", "cart", true, "selectinstance", false, "Account001")]
        [DataRow("us", "en-US", "northstarweb", false, null, false, "Account001")]
        [DataRow("us", "en-US", "northstarweb", false, null, true, "Account001")]
        [DataTestMethod]
        public async Task ListPaymentInstrumentsEx_IncludePidl_PidlAsExpected(string country, string language, string partner, bool includePidl, string operation, bool includeFlight, string accountID)
        {
            // Arrange
            if (includeFlight)
            {
                PXFlightHandler.AddToEnabledFlights("IncludePIDLWithPaymentInstrumentList");
            }

            string url = string.Format("/v7.0/{0}/paymentInstrumentsEx?country={1}&language={2}&partner={3}&includePidl={4}&operation={5}", accountID, country, language, partner, includePidl, operation);

            if (string.Equals(partner, GlobalConstants.Partners.MacManage, StringComparison.OrdinalIgnoreCase))
            {
                var features = (includeFlight && includePidl) ? "\"useClientSidePrefill\":{\"applicableMarkets\":[]}" : string.Empty;
                string expectedPSSResponse = "{\"selectinstance\":{\"template\":\"listpibuttonlist\",\"redirectionPattern\":\"inline\",\"resources\":null,\"features\":{" + features + "}}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);
            }

            // Act
            HttpResponseMessage result = await PXClient.GetAsync(url);

            // Assert
            if (includePidl && includeFlight)
            {
                bool gotPayload = result.TryGetContentValue(out Microsoft.Commerce.Payments.PidlModel.V7.PidlPayload payload);
                Assert.IsNotNull(payload);
                Assert.IsTrue(gotPayload);
                Assert.IsNotNull(payload.PidlInfo, "pidlInfo missing");
                Assert.IsNotNull(payload.PidlInfo.Pidls, "pidls is missing in pidlInfo");
                Assert.IsNotNull(payload.PidlInfo.Identity, "Identity is missing in pidlInfo");
                Assert.IsNotNull(payload.PaymentInstruments, "PaymentInstruments list is missing");
                Assert.IsTrue(payload.PaymentInstruments.Count > 0);
                Assert.IsNotNull(payload.PaymentInstruments[0].PaymentInstrumentId, "PaymentInstrumentId is missing");
            }
            else
            {
                bool gotpis = result.TryGetContentValue(out Microsoft.Commerce.Payments.PimsModel.V4.PaymentInstrument[] pis);
                Assert.IsNotNull(pis, "PaymentInstruments list is missing");
                Assert.IsTrue(gotpis);
            }

            // Reset flighting to default state
            PXFlightHandler.ResetToDefault();

            // Reset to PSS
            PXSettings.PartnerSettingsService.ResetToDefaults();
        }

        [DataRow(Constants.PartnerNames.Azure, true, HttpStatusCode.OK, true)]
        [DataRow(Constants.PartnerNames.Azure, false, HttpStatusCode.BadRequest, false)]
        [DataRow(Constants.PartnerNames.AzureSignup, true, HttpStatusCode.OK, true)]
        [DataRow(Constants.PartnerNames.AzureSignup, false, HttpStatusCode.BadRequest, false)]
        [DataRow(Constants.PartnerNames.AzureIbiza, true, HttpStatusCode.OK, true)]
        [DataRow(Constants.PartnerNames.AzureIbiza, false, HttpStatusCode.BadRequest, false)]
        [DataRow(Constants.PartnerNames.Payin, true, HttpStatusCode.OK, false)]
        [DataRow(Constants.PartnerNames.Payin, false, HttpStatusCode.OK, false)]
        [DataRow(Constants.PartnerNames.Cart, true, HttpStatusCode.OK, false)]
        [DataRow(Constants.PartnerNames.Cart, false, HttpStatusCode.OK, false)]
        [DataRow(Constants.PartnerNames.CommercialStores, true, HttpStatusCode.OK, false)]
        [DataRow(Constants.PartnerNames.CommercialStores, false, HttpStatusCode.OK, false)]
        [DataRow(Constants.PartnerNames.Webblends, true, HttpStatusCode.OK, false)]
        [DataRow(Constants.PartnerNames.Webblends, false, HttpStatusCode.OK, false)]
        [DataTestMethod]
        public async Task PaymentInstrumentsEx_ClassicProductAndBillableAccountWithCompletePrerequisites(string partner, bool sendOrgPuid, HttpStatusCode expectedStatus, bool legacyAccountUpdateExpected)
        {
            // Arrange
            bool assertCalled = false;
            string billableAccountId = CommerceAccountDataAccessor.BillingAccountId.AzureBusinessAccount;
            string orgPuid = "123456789qwerta";
            string url = $"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}&classicProduct=azureClassic&billableAccountId={billableAccountId}";

            var piPayload = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                details = new
                {
                    address = new
                    {
                        addressType = "billing",
                        addressOperation = "add",
                        addressCountry = "us",
                        address_line1 = "One Microsoft Way",
                        city = "Redmond",
                        region = "wa",
                        postal_code = "98052",
                        country = "us"
                    }
                }
            };

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi001-Visa");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI), HttpStatusCode.OK);

            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl(url)),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(piPayload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            PXSettings.CommerceAccountDataService.PreProcessGetAccountInfo = (getAccountInfoRequest) =>
            {
                // It should make call to get the legacy billable account
                Assert.AreEqual("OrgPUID", getAccountInfoRequest.Requester.IdentityType);
                Assert.AreEqual(orgPuid, getAccountInfoRequest.Requester.IdentityValue);

                assertCalled = true;
            };

            if (sendOrgPuid)
            {
                request.Headers.Add("x-ms-aadinfo", $"orgPuid={orgPuid}");
            }

            // Act
            var response = await PXClient.SendAsync(request);

            // Assert
            string piJsonOrError = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(expectedStatus, response.StatusCode, piJsonOrError);
            
            if (expectedStatus == HttpStatusCode.OK)
            {
                var pi = JsonConvert.DeserializeObject<PaymentInstrument>(piJsonOrError);

                Assert.IsNotNull(pi);
                Assert.AreEqual(expectedPI.PaymentMethod.PaymentMethodFamily, pi.PaymentMethod.PaymentMethodFamily);
                Assert.AreEqual(expectedPI.PaymentMethod.PaymentMethodType, pi.PaymentMethod.PaymentMethodType);
            }
            
            if (legacyAccountUpdateExpected)
            {
                Assert.IsTrue(assertCalled, "CommerceAccountDataService.PreProcessGetAccountInfo expected to be called");

                if (sendOrgPuid)
                {
                    // Verify the legacy update account request address has the same address line 1 as payment instrument address
                    var requestAccount = PXSettings.CommerceAccountDataService.UpdateAccountRequest.Account;
                    var postedAddressSet = requestAccount.PayinAccount.AddressSet;
                    Assert.AreEqual(billableAccountId, requestAccount.AccountID);
                    Assert.AreEqual(1, postedAddressSet.Count);
                    Assert.AreEqual(piPayload.details.address.address_line1, postedAddressSet[0].Street1);
                }
                else
                {
                    Assert.IsTrue(piJsonOrError.Contains("Puid or OrgPuid are required") && piJsonOrError.Contains("InvalidRequestData"), "InvalidRequestData error is expected");
                }
            }
            else
            {
                Assert.IsFalse(assertCalled, "CommerceAccountDataService.PreProcessGetAccountInfo expected to be not called");
            }

            PXSettings.PimsService.ResetToDefaults();
            PXSettings.CommerceAccountDataService.PreProcessGetAccountInfo = null;
        }

        /// <summary>
        /// The test is to verify bad request is thrown when accoundId is not valid
        /// </summary>
        [DataRow("azure")]
        [DataRow("webblends")]
        [DataRow("commercialstores")]
        [DataTestMethod]
        public async Task PaymentInstrumentsEx_PXChallengeSession_Account_NotValid(string partner)
        {
            // Arrange
            PXFlightHandler.AddToEnabledFlights("PXChallengeSwitch");

            string getSessionResponse = "{\"session_id\":\"854403e2-96ce-4c9e-aa9a-45b4c60f3f19\",\"session_type\":\"PXAddPISession\",\"status\":\"Active\",\"parent_session_id\":null,\"child_sessions\":null,\"session_data_hash\":\"9249ca5a047185daf4d9601b210329e6c129a3e3cf6a28033aa5d4079967139d\",\"session_data\":\"{\\r\\n\\t\\\"accountId\\\":\\\"654321\\\",\\r\\n\\t\\\"card_number\\\":\\\"4012888888889995\\\",\\r\\n\\t\\\"partner\\\":\\\"amc\\\",\\r\\n\\t\\\"operation\\\":\\\"add\\\",\\r\\n\\t\\\"family\\\":\\\"credit_card\\\",\\r\\n\\t\\\"type\\\":\\\"visa\\\",\\r\\n\\t\\\"language\\\":\\\"en-us\\\",\\r\\n\\t\\\"country\\\":\\\"usa\\\",\\r\\n\\t\\\"challengeRequired\\\":\\\"true\\\",\\r\\n\\t\\\"challengeCompleted\\\":\\\"false\\\",\\r\\n\\t\\\"challengeRetries\\\":1,\\r\\n\\t\\\"Sec-Ch-Ua\\\":\\\"Not.A/Brand;v=8,Chromium;v=114,GoogleChrome;v=114\\\",\\r\\n\\t\\\"Sec-Ch-Ua-Mobile\\\":\\\"?0,Sec-Ch-Ua-Platform:Windows\\\",\\r\\n\\t\\\"User-Agent\\\":\\\"Mozilla/5.0(WindowsNT10.0;Win64;x64)AppleWebKit/537.36(KHTML,likeGecko)Chrome/114.0.0.0Safari/537.36\\\",\\r\\n\\t\\\"client_ip\\\":\\\"1.1.0.0\\\"\\r\\n}\",\"session_length\":20,\"session_sliding_expiration\":true,\"session_expires_at\":\"2023-10-16T23:07:56.3759138Z\",\"created_by\":\"PXAddPISession\",\"updated_by\":\"PXAddPISession\",\"created_date\":\"2023-10-16T22:42:43.8934613Z\",\"updated_date\":\"2023-10-16T22:47:56.3759934Z\"}";

            PXSettings.ChallengeManagementService.ArrangeResponse(getSessionResponse, HttpStatusCode.OK, HttpMethod.Get, ".*/get/.*");

            // Act
            var requestHeaders = new Dictionary<string, string> { { "x-ms-pidlsdk-version", "2.3.9" } };
            HttpResponseMessage result = await SendRequestPXService($"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}&pxChallengeSessionId=854403e2-96ce-4c9e-aa9a-45b4c60f3f19", HttpMethod.Post, new StringContent(GetPXAddPIRequestBodyWithResourceContextForChallengeManagementFlowsWithShowChallenge(), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType), requestHeaders);
            var pidlResourceClientAction = ReadSinglePidlResourceFromJson(await result.Content.ReadAsStringAsync());
            var actionContext = JsonConvert.DeserializeObject<ActionContext>(pidlResourceClientAction.ClientAction.Context.ToString());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Expected 200 OK");
            Assert.IsTrue(string.Equals("true", actionContext.ResourceActionContext.PidlDocInfo.Parameters["showChallenge"], StringComparison.OrdinalIgnoreCase), "showChallenge parameter was not set to true");
            Assert.AreEqual(pidlResourceClientAction.ClientAction.PidlUserInputToClear, "captchaSolution", "Captcha solution indicates PX HIP implementation invocation");
        }

        /// <summary>
        /// The test is to verify bad request is thrown when ChallengeSession is not active
        /// </summary>
        [DataRow("azure")]
        [DataRow("webblends")]
        [DataRow("commercialstores")]
        [DataTestMethod]
        public async Task PaymentInstrumentsEx_PXChallengeSession_Session_NotActive(string partner)
        {
            // Arrange
            PXFlightHandler.AddToEnabledFlights("PXChallengeSwitch");

            string getSessionResponse = "{\"session_id\":\"854403e2-96ce-4c9e-aa9a-45b4c60f3f19\",\"session_type\":\"PXAddPISession\",\"status\":\"Abandoned\",\"parent_session_id\":null,\"child_sessions\":null,\"session_data_hash\":\"9249ca5a047185daf4d9601b210329e6c129a3e3cf6a28033aa5d4079967139d\",\"session_data\":\"{\\r\\n\\t\\\"accountId\\\":\\\"Account001\\\",\\r\\n\\t\\\"card_number\\\":\\\"4012888888889995\\\",\\r\\n\\t\\\"partner\\\":\\\"amc\\\",\\r\\n\\t\\\"operation\\\":\\\"add\\\",\\r\\n\\t\\\"family\\\":\\\"credit_card\\\",\\r\\n\\t\\\"type\\\":\\\"visa\\\",\\r\\n\\t\\\"language\\\":\\\"en-us\\\",\\r\\n\\t\\\"country\\\":\\\"usa\\\",\\r\\n\\t\\\"challengeRequired\\\":\\\"true\\\",\\r\\n\\t\\\"challengeCompleted\\\":\\\"false\\\",\\r\\n\\t\\\"challengeRetries\\\":1,\\r\\n\\t\\\"Sec-Ch-Ua\\\":\\\"Not.A/Brand;v=8,Chromium;v=114,GoogleChrome;v=114\\\",\\r\\n\\t\\\"Sec-Ch-Ua-Mobile\\\":\\\"?0,Sec-Ch-Ua-Platform:Windows\\\",\\r\\n\\t\\\"User-Agent\\\":\\\"Mozilla/5.0(WindowsNT10.0;Win64;x64)AppleWebKit/537.36(KHTML,likeGecko)Chrome/114.0.0.0Safari/537.36\\\",\\r\\n\\t\\\"client_ip\\\":\\\"1.1.0.0\\\"\\r\\n}\",\"session_length\":20,\"session_sliding_expiration\":true,\"session_expires_at\":\"2023-10-16T23:07:56.3759138Z\",\"created_by\":\"PXAddPISession\",\"updated_by\":\"PXAddPISession\",\"created_date\":\"2023-10-16T22:42:43.8934613Z\",\"updated_date\":\"2023-10-16T22:47:56.3759934Z\"}";

            PXSettings.ChallengeManagementService.ArrangeResponse(getSessionResponse, HttpStatusCode.OK, HttpMethod.Get, ".*/get/.*");

            // Act
            var requestHeaders = new Dictionary<string, string> { { "x-ms-pidlsdk-version", "2.3.9" } };
            HttpResponseMessage result = await SendRequestPXService($"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}&pxChallengeSessionId=854403e2-96ce-4c9e-aa9a-45b4c60f3f19", HttpMethod.Post, new StringContent(GetPXAddPIRequestBodyWithResourceContextForChallengeManagementFlowsWithShowChallenge(), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType), requestHeaders);

            var pidlResourceClientAction = ReadSinglePidlResourceFromJson(await result.Content.ReadAsStringAsync());
            var actionContext = JsonConvert.DeserializeObject<ActionContext>(pidlResourceClientAction.ClientAction.Context.ToString());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Expected 200 OK");
            Assert.IsTrue(string.Equals("true", actionContext.ResourceActionContext.PidlDocInfo.Parameters["showChallenge"], StringComparison.OrdinalIgnoreCase), "showChallenge parameter was not set to true");
            Assert.AreEqual(pidlResourceClientAction.ClientAction.PidlUserInputToClear, "captchaSolution", "Captcha solution indicates PX HIP implementation invocation");
        }

        /// <summary>
        /// The test is to verify challenge is reshown when challenge is not completed
        /// </summary>
        [DataRow("commercialstores")]
        [DataTestMethod]
        public async Task PaymentInstrumentsEx_PXChallengeSession_Challenge_Not_Completed(string partner)
        {
            // Arrange
            var challengeStatus = new
            {
                passed = false
            };

            PXSettings.ChallengeManagementService.ArrangeResponse(JsonConvert.SerializeObject(challengeStatus), HttpStatusCode.OK, HttpMethod.Get, ".*/status");

            PXFlightHandler.AddToEnabledFlights("PXChallengeSwitch");

            // Act
            var requestHeaders = new Dictionary<string, string> { { "x-ms-pidlsdk-version", "2.3.9" } };
            HttpResponseMessage result = await SendRequestPXService($"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}&pxChallengeSessionId=554403e2-96ce-4c9e-aa9a-45b4c60f3f19", HttpMethod.Post, new StringContent(GetPXAddPIRequestBodyWithResourceContextForChallengeManagementFlowsWithShowChallenge(), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType), requestHeaders);
            var pidlResourceClientAction = ReadSinglePidlResourceFromJson(await result.Content.ReadAsStringAsync());
            var actionContext = JsonConvert.DeserializeObject<ActionContext>(pidlResourceClientAction.ClientAction.Context.ToString());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Expected 200 OK");
            Assert.AreEqual(pidlResourceClientAction.ClientAction.ActionType, ClientActionType.Pidl);
            Assert.IsTrue(string.Equals("true", actionContext.ResourceActionContext.PidlDocInfo.Parameters["showChallenge"], StringComparison.OrdinalIgnoreCase), "showChallenge parameter was not set to true");
        }

        /// <summary>
        /// The test is to verify challenge status failures are suppressed when the error validation flight is ON
        /// </summary>
        [DataRow("commercialstores")]
        [DataTestMethod]
        public async Task PaymentInstrumentsEx_PXChallengeSession_Status_Error_Validation_Failure_Flight_ON(string partner)
        {
            // Arrange
            var challengeStatus = new
            {
                passed = false
            };

            PXSettings.ChallengeManagementService.ArrangeResponse(JsonConvert.SerializeObject(challengeStatus), HttpStatusCode.InternalServerError, HttpMethod.Get, ".*/status");

            PXFlightHandler.AddToEnabledFlights("PXChallengeSwitch");

            PXFlightHandler.AddToEnabledFlights("PXChallengeValidationFailureHandling");

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi002-MC");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            // Act
            var requestHeaders = new Dictionary<string, string> { { "x-ms-pidlsdk-version", "2.3.9" } };
            HttpResponseMessage result = await SendRequestPXService($"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}&pxChallengeSessionId=554403e2-96ce-4c9e-aa9a-45b4c60f3f19", HttpMethod.Post, new StringContent(GetPXAddPIRequestBodyWithResourceContextForChallengeManagementFlowsWithShowChallenge(), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType), requestHeaders);
            JObject json = JObject.Parse(await result.Content.ReadAsStringAsync());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Expected 200 OK");
        }

        /// <summary>
        /// The test is to verify challenge is reshown when status call returns failure and the error validation flight is OFF
        /// </summary>
        [DataRow("commercialstores")]
        [DataTestMethod]
        public async Task PaymentInstrumentsEx_PXChallengeSession_Status_Error_Validation_Failure_Flight_OFF(string partner)
        {
            // Arrange
            var challengeStatus = new
            {
                passed = false
            };

            PXSettings.ChallengeManagementService.ArrangeResponse(JsonConvert.SerializeObject(challengeStatus), HttpStatusCode.InternalServerError, HttpMethod.Get, ".*/status");

            PXFlightHandler.AddToEnabledFlights("PXChallengeSwitch");

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi002-MC");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            // Act
            var requestHeaders = new Dictionary<string, string> { { "x-ms-pidlsdk-version", "2.3.9" } };
            HttpResponseMessage result = await SendRequestPXService($"/v7.0/Account001/paymentInstrumentsEx?country=us&language=en-US&partner={partner}&pxChallengeSessionId=554403e2-96ce-4c9e-aa9a-45b4c60f3f19", HttpMethod.Post, new StringContent(GetPXAddPIRequestBodyWithResourceContextForChallengeManagementFlowsWithShowChallenge(), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType), requestHeaders);
            var pidlResourceClientAction = ReadSinglePidlResourceFromJson(await result.Content.ReadAsStringAsync());
            var actionContext = JsonConvert.DeserializeObject<ActionContext>(pidlResourceClientAction.ClientAction.Context.ToString());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Expected 200 OK");
            Assert.AreEqual(pidlResourceClientAction.ClientAction.ActionType, ClientActionType.Pidl);
            Assert.IsTrue(string.Equals("true", actionContext.ResourceActionContext.PidlDocInfo.Parameters["showChallenge"], StringComparison.OrdinalIgnoreCase), "showChallenge parameter was not set to true");
        }

        /// <summary>
        /// The test is to verify the logic when error Code ChallegeRequired received from PIMS
        /// </summary>
        [DataRow("commercialstores")]
        [DataTestMethod]
        public async Task PaymentInstrumentsEx_PXChallengeSession_With_PIMS_Challenge_Required_Signal(string partner)
        {
            // Arrange
            PXFlightHandler.AddToEnabledFlights("PXChallengeSwitch");

            var pimsPostPIResponse = new
            {
                CorrelationId = "px-challenge-session-correlation-id",
                ErrorCode = "ChallengeRequired",
                Message = "User has to solve a challenge",
                Source = "PXService",
                InnerError = new
                {
                    ErrorCode = "ChallengeRequired",
                    Message = "Challenge needs to be solved to add PI",
                    Source = "PIManagementService"
                }
            };

            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(pimsPostPIResponse), HttpStatusCode.InternalServerError);

            // Act
            var requestHeaders = new Dictionary<string, string> { { "x-ms-pidlsdk-version", "2.3.9" } };
            HttpResponseMessage result = await SendRequestPXService($"/v7.0/Account011/paymentInstrumentsEx?country=us&language=en-US&partner={partner}&pxChallengeSessionId=", HttpMethod.Post, new StringContent(GetPXAddPIRequestBodyWithResourceContextForChallengeManagementFlowsWithoutShowChallenge(), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType), requestHeaders);
            var pidlResourceClientAction = ReadSinglePidlResourceFromJson(await result.Content.ReadAsStringAsync());
            var actionContext = JsonConvert.DeserializeObject<ActionContext>(pidlResourceClientAction.ClientAction.Context.ToString());

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Expected 200 OK");
            Assert.IsTrue(string.Equals("true", actionContext.ResourceActionContext.PidlDocInfo.Parameters["showChallenge"], StringComparison.OrdinalIgnoreCase), "showChallenge parameter was not set to true");
        }

        /// <summary>
        /// The test is to verify the challenge evidence is added to pidldata when challenge is completed
        /// </summary>
        [TestMethod]
        public void PaymentInstrumentsEx_PXChallengeSession_Add_ChallengeEvidence()
        {
            // Arrange
            string pxChallengeSessionId = "sessionId";
            Microsoft.Commerce.Payments.PidlFactory.V7.PIDLData pidlData = new Microsoft.Commerce.Payments.PidlFactory.V7.PIDLData();
            pidlData.Add("details", new Dictionary<string, JToken>());

            // Act
            pidlData  = (Microsoft.Commerce.Payments.PidlFactory.V7.PIDLData)InvokePrivateStaticMethod(typeof(PaymentInstrumentsExController), "AddChallengeEvidenceToPidlData", pidlData, pxChallengeSessionId);
            Dictionary<string, JToken> detailsDictionary = pidlData["details"] as Dictionary<string, JToken>;
            Microsoft.Commerce.Payments.PimsModel.V4.ChallengeEvidenceData evidence = detailsDictionary["challengeEvidence"].ToObject<Microsoft.Commerce.Payments.PimsModel.V4.ChallengeEvidenceData>();

            // Assert
            Assert.AreEqual(evidence.ChallengeResult, "success");
            Assert.AreEqual(evidence.ChallengeId, "sessionId");
        }

        private object InvokePrivateStaticMethod(Type type, string methodName, params object[] parameters)
        {
            BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Static;
            MethodInfo methodInfo = type.GetMethod(methodName, bindingFlags);

            if (methodInfo == null)
            {
                throw new ArgumentException($"Method '{methodName}' not found in type '{type}'.");
            }

            return methodInfo.Invoke(null, parameters);
        }

        private string GetPXAddPIRequestBodyWithResourceContextForChallengeFlows()
        {
            var pidlParams = new Dictionary<string, string>
            {
                { "partner", "commercialstores" },
                { "operation", "Add" },
                { "language", "en-us" },
                { "family", "credit_card" },
                { "country", "us" }
            };

            var resourceActionContext = new
            {
                action = "addResource",
                pidlDocInfo = new
                {
                    anonymousPidl = false,
                    resourceType = "paymentMethod",
                    parameters = pidlParams
                }
            };

            var currentContext = new
            {
                id = "credit_card.",
                action = "addResource",
                paymentMethodFamily = "credit_card",
                resourceActionContext = resourceActionContext
            };

            dynamic requestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                captchaId = "1234",
                captchaReg = "EastUS2",
                details = new
                {
                    challengeEvidence = new
                    {
                        challengeType = "CHALLENGE",
                        challengeId = "1234",
                        challengeResult = "success",
                        challengeResultReason = "success"
                    },
                    captchaSolution = "3WYQMS",
                    currentContext = JsonConvert.SerializeObject(currentContext)
                }
            };

            return JsonConvert.SerializeObject(requestBody);
        }

        private string GetPXAddPIRequestBodyWithResourceContextForChallengeManagementFlowsWithShowChallenge()
        {
            var pidlParams = new Dictionary<string, string>
            {
                { "partner", "commercialstores" },
                { "operation", "Add" },
                { "language", "en-us" },
                { "family", "credit_card" },
                { "country", "us" },
                { "showChallenge", "true" }
            };

            var resourceActionContext = new
            {
                action = "addResource",
                pidlDocInfo = new
                {
                    anonymousPidl = false,
                    resourceType = "paymentMethod",
                    parameters = pidlParams
                }
            };

            var currentContext = new
            {
                id = "credit_card.",
                action = "addResource",
                paymentMethodFamily = "credit_card",
                resourceActionContext = resourceActionContext
            };

            dynamic requestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                captchaId = "1234",
                captchaReg = "EastUS2",
                details = new
                {
                    currentContext = JsonConvert.SerializeObject(currentContext)
                }
            };

            return JsonConvert.SerializeObject(requestBody);
        }

        private string GetPXAddPIRequestBodyWithResourceContextForChallengeManagementFlowsWithoutShowChallenge()
        {
            var pidlParams = new Dictionary<string, string>
            {
                { "partner", "commercialstores" },
                { "operation", "Add" },
                { "language", "en-us" },
                { "family", "credit_card" },
                { "country", "us" }
            };

            var resourceActionContext = new
            {
                action = "addResource",
                pidlDocInfo = new
                {
                    anonymousPidl = false,
                    resourceType = "paymentMethod",
                    parameters = pidlParams
                }
            };

            var currentContext = new
            {
                id = "credit_card.",
                action = "addResource",
                paymentMethodFamily = "credit_card",
                resourceActionContext = resourceActionContext
            };

            dynamic requestBody = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                captchaId = "1234",
                captchaReg = "EastUS2",
                details = new
                {
                    currentContext = JsonConvert.SerializeObject(currentContext)
                }
            };

            return JsonConvert.SerializeObject(requestBody);
        }

        [DataRow(GlobalConstants.Partners.Cart, "Account002-Pi008-PayPay", "paypay", "jp", false, false)]
        [DataRow(GlobalConstants.Partners.Cart, "Account002-Pi008-PayPay", "paypay", "jp", true, false)]
        [DataRow(GlobalConstants.Partners.Cart, "Account002-Pi008-AlipayHK", "alipayhk", "hk", false, false)]
        [DataRow(GlobalConstants.Partners.Cart, "Account002-Pi008-GCash", "gcash", "ph", false, false)]
        [DataRow(GlobalConstants.Partners.Cart, "Account002-Pi008-TrueMoney", "truemoney", "th", false, false)]
        [DataRow(GlobalConstants.Partners.Cart, "Account002-Pi008-TouchnGo", "touchngo", "my", false, false)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "Account002-Pi008-PayPay", "paypay", "jp", true, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "Account002-Pi008-AlipayHK", "alipayhk", "hk", true, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "Account002-Pi008-GCash", "gcash", "ph", true, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "Account002-Pi008-TrueMoney", "truemoney", "th", true, true)]
        [DataRow(GlobalConstants.Partners.NorthStarWeb, "Account002-Pi008-TouchnGo", "touchngo", "my", true, true)]
        [TestMethod]
        public async Task PaymentInstrumentsEx_ANTBatch_AddPIAsExpected(string partner, string piid, string type, string country, bool enabledPSS, bool isHeadingExpected)
        {
            // Arrange
            bool assertCalled = false;
            Dictionary<string, string> headers = null;
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = type,
                paymentMethodOperation = "add",
                paymentMethodCountry = country,
                paymentMethodResource_id = "ewallet." + type,
                sessionId = "21ff6edb-8801-320b-854d-841dbe13037f",
                context = "purchase",
                riskData = new
                {
                    dataType = "payment_method_riskData",
                    dataOperation = "add",
                    dataCountry = country
                },
                details = new
                {
                    dataType = "ewallet_paypay_details",
                    dataOperation = "add",
                    dataCountry = country,
                    authenticationMode = "Redirect"
                },
                channel = "web"
            };

            if (enabledPSS)
            {
                string flights = "PXDisablePSSCache,PXUsePartnerSettingsService";
                var settingPartner = partner.Equals(GlobalConstants.Partners.MacManage, System.StringComparison.OrdinalIgnoreCase) ? GlobalConstants.Partners.DefaultTemplate : partner;
                string expectedPSSResponse = "{\"add\":{\"template\":\"" + settingPartner + "\",\"redirectionPattern\":\"inline\",\"resources\":{\"paymentMethod\":{\"credit_card\":{\"template\":\"" + settingPartner + "\",\"redirectionPattern\":\"inline\"},\"ewallet.alipaycn\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\"}}},\"features\":{\"threeDSOne\":{\"applicableMarkets\":[\"in\"],\"displayCustomizationDetail\":null},\"hideElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"hidePaymentMethodHeading\":" + (!isHeadingExpected).ToString().ToLower() + ",\"moveCardNumberBeforeCardHolderName\":false,\"setSaveButtonDisplayContentAsNext\":false,\"updateCvvChallengeTextForGCO\":false,\"enableCountryAddorUpdateCC\":false,\"hideFirstAndLastNameForCompletePrerequisites\":false,\"hideAddCreditDebitCardHeading\":false,\"setGroupedSelectOptionTextBeforeLogo\":false,\"enableSecureFieldAddCC\":false,\"setSaveButtonDisplayContentAsBook\":false,\"removeCancelButton\":false,\"setBackButtonDisplayContentAsCancel\":false,\"hidePaymentSummaryText\":false,\"addressSuggestionMessage\":false}]}}}, \"selectinstance\":{\"template\":\"listpidropdown\",\"redirectionPattern\":null,\"resources\":null,\"features\":null},\"select\":{\"template\":\"selectpmbuttonlist\",\"features\":{\"paymentMethodGrouping\":{\"applicableMarkets\":[]}}},\"default\":{\"template\":\"" + settingPartner + "\",\"redirectionPattern\":null,\"resources\":null,\"features\":null}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

                headers = new Dictionary<string, string>
                {
                    { "x-ms-flight", flights }
                };
            }

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account002", piid);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXSettings.PimsService.PreProcess = async (request) =>
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(requestContent);
                Assert.IsTrue(JObject.DeepEquals(json, JObject.FromObject(requestBody)), "Expected request data is not found");
                assertCalled = true;
            };

            // Act
            var httpRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account002/paymentInstrumentsEx?country={country}&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };
            headers?.ToList().ForEach(header => httpRequest.Headers.Add(header.Key, header.Value));
            HttpResponseMessage result = await PXClient.SendAsync(httpRequest);

            // Assert (continuation)
            Assert.IsTrue(assertCalled, "PimsTestHandler.PreProcess wasn't called");
            string resultContent = await result.Content.ReadAsStringAsync();
            var pi = JsonConvert.DeserializeObject<global::Tests.Common.Model.Pims.PaymentInstrument>(resultContent);
            Assert.AreEqual(expectedPI.PaymentInstrumentId, pi.PaymentInstrumentId);
            Assert.IsNotNull(pi.PaymentInstrumentDetails.DefaultDisplayName);
            Assert.IsNotNull(pi.ClientAction);
            PXSettings.PimsService.ResetToDefaults();
        }

        [DataRow(GlobalConstants.Partners.Azure, false, false)]
        [DataRow(GlobalConstants.Partners.Azure, true, false)]
        [DataRow(GlobalConstants.Partners.CommercialStores, false, false)]
        [DataRow(GlobalConstants.Partners.CommercialStores, true, false)]
        [DataRow(GlobalConstants.Partners.MacManage, true, false)]
        [TestMethod]
        public async Task PaymentInstrumentsEx_AlipayCN_AddPIAsExpected(string partner, bool enabledPSS, bool isHeadingExpected)
        {
            // Arrange
            bool assertCalled = false;
            Dictionary<string, string> headers = null;
            var requestBody = new
            {
                paymentMethodFamily = "ewallet",
                paymentMethodType = "apipaycn",
                paymentMethodOperation = "add",
                paymentMethodCountry = "cn",
                paymentMethodResource_id = "ewallet.apipaycn",
                sessionId = "21ff6edb-8801-320b-854d-841dbe13037f",
                context = "purchase",
                riskData = new
                {
                    dataType = "payment_method_riskData",
                    dataOperation = "add",
                    dataCountry = "cn"
                },
                details = new
                {
                    dataType = "ewallet_apipaycn_details",
                    dataOperation = "add",
                    dataCountry = "cn",
                    authenticationMode = "Redirect"
                },
                channel = "web"
            };

            if (enabledPSS)
            {
                string flights = ",PXDisablePSSCache,PXUsePartnerSettingsService";
                var settingPartner = partner.Equals(GlobalConstants.Partners.MacManage, System.StringComparison.OrdinalIgnoreCase) ? GlobalConstants.Partners.DefaultTemplate : partner;
                string expectedPSSResponse = "{\"add\":{\"template\":\"" + settingPartner + "\",\"redirectionPattern\":\"inline\",\"resources\":{\"paymentMethod\":{\"credit_card\":{\"template\":\"" + settingPartner + "\",\"redirectionPattern\":\"inline\"},\"ewallet.alipaycn\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\"}}},\"features\":{\"threeDSOne\":{\"applicableMarkets\":[\"in\"],\"displayCustomizationDetail\":null},\"hideElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"hidePaymentMethodHeading\":" + (!isHeadingExpected).ToString().ToLower() + ",\"moveCardNumberBeforeCardHolderName\":false,\"setSaveButtonDisplayContentAsNext\":false,\"updateCvvChallengeTextForGCO\":false,\"enableCountryAddorUpdateCC\":false,\"hideFirstAndLastNameForCompletePrerequisites\":false,\"hideAddCreditDebitCardHeading\":false,\"setGroupedSelectOptionTextBeforeLogo\":false,\"enableSecureFieldAddCC\":false,\"setSaveButtonDisplayContentAsBook\":false,\"removeCancelButton\":false,\"setBackButtonDisplayContentAsCancel\":false,\"hidePaymentSummaryText\":false,\"addressSuggestionMessage\":false}]}}}, \"selectinstance\":{\"template\":\"listpidropdown\",\"redirectionPattern\":null,\"resources\":null,\"features\":null},\"select\":{\"template\":\"selectpmbuttonlist\",\"features\":{\"paymentMethodGrouping\":{\"applicableMarkets\":[]}}},\"default\":{\"template\":\"" + settingPartner + "\",\"redirectionPattern\":null,\"resources\":null,\"features\":null}}";
                PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

                headers = new Dictionary<string, string>
                {
                    { "x-ms-flight", flights }
                };
            }

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account002", "Account002-Pi009-AlipayCN");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            PXSettings.PimsService.PreProcess = async (request) =>
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(requestContent);
                Assert.IsTrue(JObject.DeepEquals(json, JObject.FromObject(requestBody)), "Expected request data is not found");
                assertCalled = true;
            };

            // Act
            var httpRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/Account002/paymentInstrumentsEx?country=cn&language=en-US&partner={partner}")),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };
            headers?.ToList().ForEach(header => httpRequest.Headers.Add(header.Key, header.Value));
            HttpResponseMessage result = await PXClient.SendAsync(httpRequest);

            // Assert (continuation)
            Assert.IsTrue(assertCalled, "PimsTestHandler.PreProcess wasn't called");
            string resultContent = await result.Content.ReadAsStringAsync();
            var pi = JsonConvert.DeserializeObject<global::Tests.Common.Model.Pims.PaymentInstrument>(resultContent);
            Assert.AreEqual(expectedPI.PaymentInstrumentId, pi.PaymentInstrumentId);
            Assert.IsNotNull(pi.PaymentInstrumentDetails.DefaultDisplayName);
            Assert.IsNotNull(pi.ClientAction);
            PXSettings.PimsService.ResetToDefaults();
        }

        [DataRow(true, HttpStatusCode.OK, "Account002")]
        [DataRow(false, HttpStatusCode.OK, "Account002")]
        [DataRow(true, HttpStatusCode.NotFound, null)]
        [TestMethod]
        public async Task PaymentInstrumentsEx_GetChallengeContext(bool ipFound, HttpStatusCode expectedStatusCode, string accountName)
        {
            // Arrange
            PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument(accountName, "Account002-Pi001-Visa");
            string expectedIp = null;

            // Act
            var httpRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/{accountName}/paymentInstrumentsEx/Account002-Pi001-Visa/getChallengeContext")),
                Method = HttpMethod.Get,
            };

            var headers = new Dictionary<string, string>();

            if (ipFound)
            {
                headers.Add("x-ms-deviceinfo", "ipAddress=MTc2LjQ1LjkyLjE=");
                expectedIp = "176.45.92.1";

                headers.Add("x-ms-clientcontext-encoding", "base64");
            }

            headers?.ToList().ForEach(header => httpRequest.Headers.Add(header.Key, header.Value));

            HttpResponseMessage result = await PXClient.SendAsync(httpRequest);

            // Assert 
            Assert.AreEqual(expectedStatusCode, result.StatusCode);

            if (expectedPI != null)
            {
                string resultContent = await result.Content.ReadAsStringAsync();
                ChallengeContext challengeContext = JsonConvert.DeserializeObject<global::Tests.Common.Model.Pims.ChallengeContext>(resultContent);
                Assert.IsNotNull(challengeContext);
                Assert.AreEqual("visa", challengeContext.PaymentMethodType);
                Assert.AreEqual(expectedIp, challengeContext.IpAddress);
            }
        }

        [DataRow(GlobalConstants.Partners.XboxSettings, HttpStatusCode.OK, true)]
        [DataRow(GlobalConstants.Partners.XboxSettings, HttpStatusCode.OK, false)]
        [DataRow(GlobalConstants.Partners.Storify, HttpStatusCode.OK, true)]
        [DataRow(GlobalConstants.Partners.XboxSettings, HttpStatusCode.InternalServerError, true)]
        [DataRow(GlobalConstants.Partners.Storify, HttpStatusCode.InternalServerError, true)]
        [TestMethod]
        public async Task PaymentInstrumentsEx_CreditCard_AddPI_AnonymousSecondScreen(string partner, HttpStatusCode expectedStatusCode, bool isTestAccount)
        {
            // Arrange
            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi002-MC");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            string sessionId = "ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6";
            string currentTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
            string sessionResponse = "{\"Id\":\"" + sessionId + "\",\"SessionType\":0,\"Data\":\"{\\\"ExposedFlightFeatures\\\":null,\\\"AccountId\\\":\\\"Account001\\\",\\\"Language\\\":\\\"en-us\\\",\\\"AllowTestHeader\\\":\\\"" + isTestAccount + "\\\",\\\"payment_session_id\\\":\\\"e99fd8de-8db4-4d7f-9ad0-1857b25503d4\\\",\\\"Partner\\\":\\\"xboxsettings\\\",\\\"Country\\\":\\\"US\\\",\\\"UseCount\\\":0,\\\"Operation\\\":\\\"Add\\\",\\\"Email\\\":\\\"somefakeaccount@outlook.com\\\",\\\"FirstName\\\":null,\\\"LastName\\\":null,\\\"PaymentMethodType\\\":null,\\\"PaymentInstrumentId\\\":null,\\\"Status\\\":3,\\\"ChallengeStatus\\\":\\\"Unknown\\\",\\\"RiskData\\\":{\\\"ipAddress\\\":null,\\\"userInfo\\\":null,\\\"deviceId\\\":null,\\\"userAgent\\\":null,\\\"greenId\\\":null,\\\"deviceType\\\":null},\\\"signature\\\":\\\"placeholder_for_paymentsession_signature_e99fd8de-8db4-4d7f-9ad0-1857b25503d4\\\",\\\"QrCodeCreatedTime\\\":\\\"" + currentTime + "\\\",\\\"FormRenderedTime\\\":\\\"" + currentTime + "\\\"}\",\"EncryptData\":false,\"Result\":null,\"State\":\"INCOMPLETE\",\"TestContext\":null}";
            PXSettings.SessionService.ResponseProvider.SessionStore.Add(sessionId, sessionResponse);
            string postMessageContains = "Account001-Pi002-MC";

            if (expectedStatusCode == HttpStatusCode.InternalServerError)
            {
                sessionId = "AFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6"; // Send invalid sessionid
                postMessageContains = "InternalError";
            }

            // Act
            var postRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl($"/v7.0/paymentInstrumentsEx/create?country=us&language=en-US&partner={partner}&sessionId={sessionId}&scenario=secondScreenAddPi")),
                Method = HttpMethod.Post,
                Content = new StringContent("{\"paymentMethodFamily\":\"credit_card\",\"paymentMethodType\":\"visa\",\"paymentMethodOperation\":\"add\",\"paymentMethodCountry\":\"us\",\"paymentMethodResource_id\":\"credit_card.visa\",\"sessionId\":\"ZFFFFFFFFFFF78a93cbb-8a57-49d9-84b7-42c5f042dba6\",\"context\":\"purchase\",\"riskData\":{\"dataType\":\"payment_method_riskData\",\"dataOperation\":\"add\",\"dataCountry\":\"us\"},\"details\":{\"dataType\":\"ewallet_paypalRedirect_details\",\"dataOperation\":\"add\",\"dataCountry\":\"us\",\"authenticationMode\":\"Redirect\"}}", Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            postRequest.Headers.Add("x-ms-test", "{\"scenarios\": \"testAccountHeader\", \"contact\": \"TestApp\"}");

            HttpResponseMessage response = await PXClient.SendAsync(postRequest);
            Assert.AreEqual(expectedStatusCode, response.StatusCode);
            var resumeClientActionPostMessage = await response.Content.ReadAsStringAsync();
            Assert.IsNotNull(resumeClientActionPostMessage);
            Assert.IsTrue(resumeClientActionPostMessage.Contains(postMessageContains));
        }

        [DataRow("request-context", "pr_12345")]
        [DataRow("request-context", "wr_12345")]
        [DataRow("x-ms-request-context", "pr_12345")]
        [DataRow("x-ms-request-context", "wr_12345")]
        [DataTestMethod]
        public async Task PaymentInstrumentsEx_AddCreditCard_Battlenet(string requestContextHeaderName, string requestId)
        {
            // Arrange
            var status = "Completed";
            string piid = "Account001-Pi001-Visa";
            string url = $"/v7.0/paymentInstrumentsEx/create?country=us&language=en-US&partner=battlenet";            

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", piid);
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));
            PXSettings.PaymentOrchestratorService.ArrangeResponse(JsonConvert.SerializeObject(new { Id = requestId, Status = status }));

            var piPayload = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                details = new
                {
                    address = new
                    {
                        addressType = "billing",
                        addressOperation = "add",
                        addressCountry = "us",
                        address_line1 = "One Microsoft Way",
                        city = "Redmond",
                        region = "wa",
                        postal_code = "98052",
                        country = "us"
                    }
                }
            };

            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl(url)),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(piPayload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            string requestContextHeaderValue = $"{{\"tenantId\":\"tid\",\"tenantCustomerId\":\"tcid\",\"requestId\":\"{requestId}\",\"paymentAccountId\":\"accountid\"}}";
            request.Headers.Add(requestContextHeaderName, requestContextHeaderValue);

            // Act
            var response = await PXClient.SendAsync(request);

            // Assert
            string result = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(result);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(jsonObject["requestId"]);
            Assert.AreEqual(jsonObject["requestId"].ToString(), requestId);
            Assert.AreEqual(jsonObject["status"].ToString(), status);
        }

        [DataRow(true)]
        [DataRow(false)]
        [DataTestMethod]
        public async Task PaymentInstrumentsEx_AddCreditCard_CheckoutRequest(bool usePaymentRequestApi)
        {
            // TODO to make this work for post PI checkout
            // Arrange
            string url = $"/v7.0/paymentInstrumentsEx/create?country=us&language=en-US&partner=webblends";

            string requestContext = "{\"tenantId\":\"battle.net\",\"tenantCustomerId\":\"abc\",\"requestId\":\"cr_39c93cc0-e855-42bc-8aca-183a572e14bc\",\"paymentAccountId\":\"123\",\"checkoutRequestId\":\"cr_39c93cc0-e855-42bc-8aca-183a572e14bc\"}";

            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "x-ms-request-context", requestContext },
                { "x-ms-test", "{\"scenarios\":\"px.po.attachPaymentInstruments\",\"contact\":\"pidlsdk\"}" }
            };

            if (usePaymentRequestApi)
            {
                headers.Add("x-ms-flight", "UsePaymentRequestApi");
            }

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi001-Visa");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            var piPayload = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                details = new
                {
                    address = new
                    {
                        addressType = "billing",
                        addressOperation = "add",
                        addressCountry = "us",
                        address_line1 = "One Microsoft Way",
                        city = "Redmond",
                        region = "wa",
                        postal_code = "98052",
                        country = "us"
                    }
                },
                savePaymentDetails = true
            };

            bool isCardOnFileUsed = false;
            PXSettings.PaymentOrchestratorService.PreProcess = async (pimsServiceRequest) =>
            {
                if (pimsServiceRequest.RequestUri.AbsoluteUri.Contains("attachpaymentinstruments"))
                {
                    string requestContent = await pimsServiceRequest.Content.ReadAsStringAsync();
                    isCardOnFileUsed = requestContent.Contains("\"actionAfterInitialTransaction\":\"VaultOnSuccess\"");
                }
            };

            // Act
            HttpResponseMessage result = await SendRequestPXService(url, HttpMethod.Post, new StringContent(JsonConvert.SerializeObject(piPayload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType), headers);

            // Assert
            Assert.IsNotNull(result);
            string responseContent = await result.Content.ReadAsStringAsync();
            PIDLResource pidl = ReadSinglePidlResourceFromJson(responseContent);
            JObject actionContext = pidl.ClientAction.Context as JObject;
            string explicitProperty = actionContext["explicit"].ToString();
            Assert.AreEqual(pidl.ClientAction.ActionType.ToString(), "MergeData", "client action type should be mergeData");
            Assert.AreEqual(pidl.ClientAction.NextAction.ActionType.ToString(), "None", "client action type should be mergeData");
            Assert.AreEqual(explicitProperty, "True", "explicitProperty should be true");

            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Expected statuscode is not found");
            Assert.IsTrue(isCardOnFileUsed, "UsageType should be onfile");
        }

        /// <summary>
        /// /[DataRow("Rejected")]
        /// </summary>
        /// <param name="recommendationValue"></param>
        /// <returns></returns>
        [DataRow("Approved")]
        [DataRow("Rejected")]
        [DataTestMethod]
        public async Task PaymentInstrumentsEx_AddCreditCard_CheckoutRequest_FraudDetetion(string recommendationValue)
        {
            // TODO to make this work for post PI checkout
            // Arrange
            string url = $"/v7.0/paymentInstrumentsEx/create?country=us&language=en-US&partner=webblends";

            string requestContext = "{\"tenantId\":\"battle.net\",\"tenantCustomerId\":\"abc\",\"requestId\":\"cr_39c93cc0-e855-42bc-8aca-183a572e14bc\",\"paymentAccountId\":\"123\",\"checkoutRequestId\":\"cr_39c93cc0-e855-42bc-8aca-183a572e14bc\"}";

            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "x-ms-request-context", requestContext },
                { "x-ms-test", "{\"scenarios\":\"px.po.attachPaymentInstruments\",\"contact\":\"pidlsdk\"}" },
                { "x-ms-flight", "PXIntegrateFraudDetectionService" }
            };

            global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi001-Visa");
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));

            EvaluationResult evaluationResult = new EvaluationResult()
            {
                ActivityId = "testPX",
                RiskScore = (decimal?)99.90,
                Recommendation = recommendationValue,
                Reason = "reason"
            };

            PXSettings.FraudDetectionService.ArrangeResponse(JsonConvert.SerializeObject(evaluationResult));

            var piPayload = new
            {
                paymentMethodFamily = "credit_card",
                paymentMethodType = "visa",
                details = new
                {
                    address = new
                    {
                        addressType = "billing",
                        addressOperation = "add",
                        addressCountry = "us",
                        address_line1 = "One Microsoft Way",
                        city = "Redmond",
                        region = "wa",
                        postal_code = "98052",
                        country = "us"
                    }
                },
                savePaymentDetails = true
            };

            bool isCardOnFileUsed = false;
            PXSettings.PaymentOrchestratorService.PreProcess = async (pimsServiceRequest) =>
            {
                if (pimsServiceRequest.RequestUri.AbsoluteUri.Contains("attachpaymentinstruments"))
                {
                    string requestContent = await pimsServiceRequest.Content.ReadAsStringAsync();
                    isCardOnFileUsed = requestContent.Contains("\"actionAfterInitialTransaction\":\"VaultOnSuccess\"");
                }
            };

            // Act
            HttpResponseMessage result = await SendRequestPXService(url, HttpMethod.Post, new StringContent(JsonConvert.SerializeObject(piPayload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType), headers);

            // Assert
            Assert.IsNotNull(result);
            if (string.Equals(recommendationValue, "Approved"))
            {
                string responseContent = await result.Content.ReadAsStringAsync();
                PIDLResource pidl = ReadSinglePidlResourceFromJson(responseContent);
                JObject actionContext = pidl.ClientAction.Context as JObject;
                string explicitProperty = actionContext["explicit"].ToString();
                Assert.AreEqual(pidl.ClientAction.ActionType.ToString(), "MergeData", "client action type should be mergeData");
                Assert.AreEqual(pidl.ClientAction.NextAction.ActionType.ToString(), "None", "client action type should be mergeData");
                Assert.AreEqual(explicitProperty, "True", "explicitProperty should be true");

                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Expected statuscode is not found");
                Assert.IsTrue(isCardOnFileUsed, "UsageType should be onfile");
            }
            else if (string.Equals(recommendationValue, "Rejected"))
            {
                // Updating to OK as initial integration is log and observe. Future iteration to return Error with Reject recommendation
                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }

            PXSettings.PimsService.ResetToDefaults();
            PXSettings.FraudDetectionService.ResetToDefaults();
        }

        [DataRow("paypay", "testa*****tpaypay")]
        [DataRow("alipayhk", "testa*****talipayhk")]
        [DataRow("gcash", "testa*****tgcash")]
        [DataRow("truemoney", "testa*****ttm")]
        [DataRow("touchngo", "testa*****ttng")]
        [DataTestMethod]
        public async Task ListPaymentInstrumentsEx_ANTBatch_DisplayNamesAsExpected(string paymentMethodType, string expectedDisplayName)
        {
            string partner = "northstarweb";
            string language = "en-US";
            var pis = await ListPIFromPXService(
                string.Format(
                    "/v7.0/Account002/paymentInstrumentsEx?language={0}&partner={1}",
                    language,
                    partner));

            var targetPi = pis.FirstOrDefault(pi => pi.PaymentMethod.EqualByFamilyAndType("ewallet", paymentMethodType));

            Assert.AreEqual(expectedDisplayName, targetPi?.PaymentInstrumentDetails.UserLoginId, "PaymentInstrumentDetails.UserLogOnId is expected to match the expected value");
        }

        [DataRow(Constants.PartnerNames.Azure, "SepaPicvAccount-Pi001-Valid", "EnableSepaJpmc")]
        [DataRow(Constants.PartnerNames.Bing, "SepaPicvAccount-Pi001-Valid", "EnableSepaJpmc")]
        [DataRow(Constants.PartnerNames.Cart, "SepaPicvAccount-Pi001-Valid", "EnableSepaJpmc")]
        [DataRow(Constants.PartnerNames.CommercialStores, "SepaPicvAccount-Pi001-Valid", "EnableSepaJpmc")]
        [DataRow(Constants.PartnerNames.DefaultTemplate, "SepaPicvAccount-Pi001-Valid", "EnableSepaJpmc")]
        [DataRow(Constants.VirtualPartnerNames.Macmanage, "SepaPicvAccount-Pi001-Valid", "EnableSepaJpmc")]
        [DataRow(Constants.PartnerNames.NorthStarWeb, "SepaPicvAccount-Pi001-Valid", "EnableSepaJpmc")]
        [DataRow(Constants.VirtualPartnerNames.OfficeSmb, "SepaPicvAccount-Pi001-Valid", "EnableSepaJpmc")]
        [DataRow(Constants.PartnerNames.OXODIME, "SepaPicvAccount-Pi001-Valid", "EnableSepaJpmc")]
        [DataRow(Constants.PartnerNames.OXOWebDirect, "SepaPicvAccount-Pi001-Valid", "EnableSepaJpmc")]
        [DataRow(Constants.PartnerNames.SetupOfficeSdx, "SepaPicvAccount-Pi001-Valid", "EnableSepaJpmc")]
        [DataRow(Constants.PartnerNames.Webblends, "SepaPicvAccount-Pi001-Valid", "EnableSepaJpmc")]
        [DataRow(Constants.PartnerNames.XboxSubs, "SepaPicvAccount-Pi001-Valid", "EnableSepaJpmc")]
        [DataRow(Constants.PartnerNames.Azure, "SepaPicvAccount-Pi001-Valid", null)]
        [DataRow(Constants.PartnerNames.Bing, "SepaPicvAccount-Pi001-Valid", null)]
        [DataRow(Constants.PartnerNames.Cart, "SepaPicvAccount-Pi001-Valid", null)]
        [DataRow(Constants.PartnerNames.CommercialStores, "SepaPicvAccount-Pi001-Valid", null)]
        [DataRow(Constants.PartnerNames.DefaultTemplate, "SepaPicvAccount-Pi001-Valid", null)]
        [DataRow(Constants.VirtualPartnerNames.Macmanage, "SepaPicvAccount-Pi001-Valid", null)]
        [DataRow(Constants.PartnerNames.NorthStarWeb, "SepaPicvAccount-Pi001-Valid", null)]
        [DataRow(Constants.VirtualPartnerNames.OfficeSmb, "SepaPicvAccount-Pi001-Valid", null)]
        [DataRow(Constants.PartnerNames.OXODIME, "SepaPicvAccount-Pi001-Valid", null)]
        [DataRow(Constants.PartnerNames.OXOWebDirect, "SepaPicvAccount-Pi001-Valid", null)]
        [DataRow(Constants.PartnerNames.SetupOfficeSdx, "SepaPicvAccount-Pi001-Valid", null)]
        [DataRow(Constants.PartnerNames.Webblends, "SepaPicvAccount-Pi001-Valid", null)]
        [DataRow(Constants.PartnerNames.XboxSubs, "SepaPicvAccount-Pi001-Valid", null)]
        [DataTestMethod]
        public async Task PaymentInstrumentsEx_PassUserAgentInPiObjectForSepa(string partner, string accountId, string featureFlight = null)
        {
            string postReqWithRiskData = null;
            string postRequestUri = null;
            PaymentInstrument piFromPims = null;
            string expectedUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML";

            var headers = new Dictionary<string, string>()
            {
                {
                    "x-ms-deviceinfo", $"userAgent={expectedUserAgent}"
                },
                {
                    "x-ms-flight", $"{featureFlight}"
                }
            };

            if (accountId == "SepaPicvAccount-Pi001-Valid")
            {
                postReqWithRiskData = "{\"paymentMethodFamily\":\"direct_debit\",\"paymentMethodType\":\"sepa\",\"paymentMethodOperation\":\"add\",\"paymentMethodCountry\":\"de\",\"paymentMethodResource_id\":\"direct_debit.sepa\",\"sessionId\":\"69eda14d-9e37-ecd7-e6b2-a8860add1246\",\"riskData\":{\"dataType\":\"payment_method_riskData\",\"dataOperation\":\"add\",\"dataCountry\":\"de\"},\"details\":{\"dataType\":\"direct_debit_sepa_details\",\"dataOperation\":\"add\",\"dataCountry\":\"de\"}}";
                postRequestUri = $"/v7.0/SepaPicvAccount/paymentInstrumentsEx?country=de&language=en-US&partner={partner}";
                piFromPims = PimsMockResponseProvider.GetPaymentInstrument("SepaPicvAccount", string.Format(accountId, "SepaPicvAccount"));
            }

            var postRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl(postRequestUri)),
                Method = HttpMethod.Post,
                Content = new StringContent(postReqWithRiskData, Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            headers?.ToList().ForEach(header =>
            {
                postRequest.Headers.Add(header.Key, header.Value);
            });

            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(piFromPims));

            bool assertCalled = false;

            PXSettings.PimsService.PreProcess = async (request) =>
            {
                string requestContent = await request.Content.ReadAsStringAsync();
                string extendeFlightHeaders = request.Headers.GetValues("x-ms-flight").FirstOrDefault();
                JObject json = JObject.Parse(requestContent);

                Assert.IsTrue(requestContent.Contains("riskData"), "does not contain riskData in request content");
                var postReqRiskData = json.GetValue("riskData") as JObject;

                string userAgentVal = postReqRiskData.GetValue("userAgent").ToString();
                Assert.AreEqual(expectedUserAgent, userAgentVal, "userAgent does not match with user agent in risk data before PIMS");

                assertCalled = true;
            };

            HttpResponseMessage response = await PXClient.SendAsync(postRequest);
            Assert.IsTrue(assertCalled, "PimsTestHandler.PreProcess wasn't called");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            string resultContent = await response.Content.ReadAsStringAsync();
            var pi = JsonConvert.DeserializeObject<global::Tests.Common.Model.Pims.PaymentInstrument>(resultContent);
            Assert.AreEqual(piFromPims.PaymentInstrumentId, pi.PaymentInstrumentId);
            PXSettings.PimsService.ResetToDefaults();
        }

        [DataRow(Constants.PartnerNames.OXODIME, "SepaPicvAccount-Pi001-Redirect", "PXEnableSepaRedirectUrlText")]
        [DataRow(Constants.PartnerNames.OXODIME, "SepaPicvAccount-Pi001-Redirect", null)]
        [DataTestMethod]
        public async Task PaymentInstrumentsEx_PXEnableSepaRedirectUrlText(string partner, string piid, string featureFlight = null)
        {
            // Arrange
            var headers = new Dictionary<string, string>() { { "x-ms-flight", $"{featureFlight}" } };

            var pxRequestBody = new
            {
                paymentMethodFamily = "direct_debit",
                paymentMethodType = "sepa",
                paymentMethodOperation = "add"
            };

            string postRequestUri = $"/v7.0/SepaPicvAccount/paymentInstrumentsEx?country=de&language=en-US&partner={partner}";
            PaymentInstrument piFromPims = PimsMockResponseProvider.GetPaymentInstrument("SepaPicvAccount", piid);

            var postRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri(GetPXServiceUrl(postRequestUri)),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(pxRequestBody), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType)
            };

            headers?.ToList().ForEach(header =>
            {
                postRequest.Headers.Add(header.Key, header.Value);
            });

            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(piFromPims));

            // Act
            HttpResponseMessage response = await PXClient.SendAsync(postRequest);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            string resultContent = await response.Content.ReadAsStringAsync();
            var pi = JsonConvert.DeserializeObject<global::Tests.Common.Model.Pims.PaymentInstrument>(resultContent);
            Assert.AreEqual(piFromPims.PaymentInstrumentId, pi.PaymentInstrumentId);

            Assert.AreEqual(pi.ClientAction.ActionType.ToString(), ClientActionType.Redirect.ToString());
            Assert.IsNotNull(pi.ClientAction.RedirectPidl, "Client action redirection pidl missing");

            var pidlResource = ReadPidlResourceFromJson(pi.ClientAction.RedirectPidl.ToString());

            var redirectGroup = pidlResource[0].GetDisplayHintById("sepaRedirectTextGroup");
            if (!string.IsNullOrEmpty(featureFlight) && featureFlight.Contains("PXEnableSepaRedirectUrlText"))
            {
                Assert.IsNotNull(redirectGroup, "Redirect text group is missing");

                HyperlinkDisplayHint redirectLink = pidlResource[0].GetDisplayHintById("sepaRedirectLink") as HyperlinkDisplayHint;
                Assert.IsNotNull(redirectLink, "Redirect link is missing");
                Assert.IsTrue(redirectLink.Action.Context.ToString().Contains(piid), "Action context should have redirection href.");
            }
            else
            {
                Assert.IsNull(redirectGroup, "Redirect text group is not expected.");
            }
            
            PXSettings.PimsService.ResetToDefaults();
        }
    }
}