// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2022. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using global::Tests.Common.Model;
    using global::Tests.Common.Model.Pidl;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService;
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.Metrics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using static Microsoft.Commerce.Payments.PidlFactory.GlobalConstants.ServiceContextKeys;

    [TestClass]
    public class CheckoutRequestExTests : TestBase
    {
        [DataRow("mergeData", false)]
        [DataRow("", false)]
        [DataRow("mergeData", true)]
        [DataRow("", true)]
        [TestMethod]
        public async Task CheckoutRequestExAttachAddressTests(string scenario, bool useUsePaymentRequestApi)
        {
            // Arrange
            string requestId = useUsePaymentRequestApi ? "pr_39c93cc0-e855-42bc-8aca-183a572e14bc" : "cr_39c93cc0-e855-42bc-8aca-183a572e14bc";
            string requestUrl = string.Format($"/v7.0/PaymentClient/CheckoutRequestsEx/{requestId}/attachAddress");
            if (!string.IsNullOrEmpty(scenario))
            {
                requestUrl += "?scenario=" + scenario;
            }

            string requestContext = $"{{\"tenantId\":\"battle.net\",\"tenantCustomerId\":\"abc\",\"requestId\":\"{requestId}\",\"paymentAccountId\":\"123\",\"checkoutRequestId\":\"{requestId}\"}}";

            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "x-ms-request-context", requestContext },
                { "x-ms-test", "{\"scenarios\":\"px.po.attachAddress,px.account.v2.address.legacyvalidate.valid\",\"contact\":\"pidlsdk\"}" }
            };

            if (useUsePaymentRequestApi)
            {
                headers.Add("x-ms-flight", "UsePaymentRequestApi");
            }

            var payload = new
            {
                address_line1 = "123 Main St",
                address_line2 = "Apt 1",
                city = "Redmond",
                state = "WA",
                postalCode = "98052",
                country = "US"
            };

            bool useCheckoutRequestAttachAddress = false;
            bool usePaymentRequestAttachAddress = false;

            PXSettings.PaymentOrchestratorService.PreProcess = (poServiceRequest) =>
            {
                if (poServiceRequest.RequestUri.AbsolutePath.Contains("/attachaddress"))
                {
                    useCheckoutRequestAttachAddress = poServiceRequest.RequestUri.AbsolutePath.Contains("/checkoutRequests");
                    usePaymentRequestAttachAddress = poServiceRequest.RequestUri.AbsolutePath.Contains("/paymentRequests");
                }
            };

            HttpResponseMessage result = await SendRequestPXService(requestUrl, HttpMethod.Post, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType), headers);

            string responseContent = await result.Content.ReadAsStringAsync();
            PIDLResource pidl = ReadSinglePidlResourceFromJson(responseContent);

            // Assert
            if (scenario.Equals("mergeData", StringComparison.OrdinalIgnoreCase))
            {
                JObject actionContext = pidl.ClientAction.Context as JObject;
                string explicitProperty = actionContext["explicit"].ToString();
                Assert.AreEqual(pidl.ClientAction.ActionType.ToString(), "MergeData", "client action type should be mergeData");
                Assert.AreEqual(explicitProperty, "True", "explicitProperty should be true");
            }
            else
            {
                Assert.IsNull(pidl.ClientAction, "without mergeData scenario, mergeData clientAction shouldn't be added");
            }

            if (useUsePaymentRequestApi)
            {
                Assert.IsTrue(usePaymentRequestAttachAddress, "PaymentRequest attach address should be used for payment request attach address call.");
            }
            else
            {
                Assert.IsTrue(useCheckoutRequestAttachAddress, "CheckoutRequest attach address should be used for checkout request attach address call.");
            }

            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PaymentOrchestratorService.ResetToDefaults();
            PXFlightHandler.ResetToDefault();
        }

        [DataRow(false)]
        [DataRow(true)]
        [TestMethod]
        public async Task CheckoutRequestExAttachProfileTests(bool useUsePaymentRequestApi)
        {
            // Arrange
            string requestId = useUsePaymentRequestApi ? "pr_39c93cc0-e855-42bc-8aca-183a572e14bc" : "cr_39c93cc0-e855-42bc-8aca-183a572e14bc";
            string requestUrl = string.Format($"/v7.0/PaymentClient/CheckoutRequestsEx/{requestId}/attachProfile");
            string requestContext = "{\"tenantId\":\"battle.net\",\"tenantCustomerId\":\"abc\",\"requestId\":\"cr_39c93cc0-e855-42bc-8aca-183a572e14bc\",\"paymentAccountId\":\"123\",\"checkoutRequestId\":\"cr_39c93cc0-e855-42bc-8aca-183a572e14bc\"}";

            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "x-ms-request-context", requestContext },
                { "x-ms-test", "{\"scenarios\":\"px.po.attachProfile\",\"contact\":\"pidlsdk\"}" }
            };

            if (useUsePaymentRequestApi)
            {
                headers.Add("x-ms-flight", "UsePaymentRequestApi");
            }

            var payload = new
            {
                email_address = "test@email.com"
            };

            bool useCheckoutRequestAttachProfile = false;
            bool usePaymentRequestAttachProfile = false;

            PXSettings.PaymentOrchestratorService.PreProcess = (poServiceRequest) =>
            {
                if (poServiceRequest.RequestUri.AbsolutePath.Contains("/attachprofile"))
                {
                    useCheckoutRequestAttachProfile = poServiceRequest.RequestUri.AbsolutePath.Contains("/checkoutRequests");
                    usePaymentRequestAttachProfile = poServiceRequest.RequestUri.AbsolutePath.Contains("/paymentRequests");
                }
            };

            HttpResponseMessage result = await SendRequestPXService(requestUrl, HttpMethod.Post, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType), headers);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Expected statuscode is not found");

            if (useUsePaymentRequestApi)
            {
                Assert.IsTrue(usePaymentRequestAttachProfile, "PaymentRequest attach profile should be used for payment request attach profile call.");
            }
            else
            {
                Assert.IsTrue(useCheckoutRequestAttachProfile, "CheckoutRequest attach profile should be used for checkout request attach profile call.");
            }

            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PaymentOrchestratorService.ResetToDefaults();
            PXFlightHandler.ResetToDefault();
        }

        [DataRow("visa", false)]
        [DataRow("googlepay", false)]
        [DataRow("applepay", false)]
        [DataRow("visa", true)]
        [DataRow("googlepay", true)]
        [DataRow("applepay", true)]
        [TestMethod]
        public async Task CheckoutRequestExConfirmTests(string paymentMethodType, bool useUsePaymentRequestApi)
        {
            // Arrange
            if (paymentMethodType.Equals("applepay", StringComparison.OrdinalIgnoreCase)
                    || paymentMethodType.Equals("googlepay", StringComparison.OrdinalIgnoreCase))
            {
                string id = paymentMethodType.Equals("applepay", StringComparison.OrdinalIgnoreCase) ? "cw_apay_9f8b4e00-edf0-4b22-8feb-37c152e5875b" : "cw_gpay_cc80ac8e-3e33-40f3-9fed-6efb5be47762";
                global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account013", id);
                PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));
            }

            string requestId = useUsePaymentRequestApi ? "pr_39c93cc0-e855-42bc-8aca-183a572e14bc" : "cr_39c93cc0-e855-42bc-8aca-183a572e14bc";
            string requestUrl = string.Format($"/v7.0/PaymentClient/CheckoutRequestsEx/{requestId}/confirm");
            string requestContext = $"{{\"tenantId\":\"battle.net\",\"tenantCustomerId\":\"abc\",\"requestId\":\"{requestId}\",\"paymentAccountId\":\"123\",\"checkoutRequestId\":\"{requestId}\"}}";

            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "x-ms-request-context", requestContext },
                { "x-ms-test", "{\"scenarios\":\"px.po.confirm\",\"contact\":\"pidlsdk\"}" }
            };

            if (useUsePaymentRequestApi)
            {
                headers.Add("x-ms-flight", "UsePaymentRequestApi");
            }

            bool isStandaloneAttachPaymentTypeUsed = false;
            PXSettings.PimsService.PreProcess = async (pimsServiceRequest) =>
            {
                string requestContent = await pimsServiceRequest.Content.ReadAsStringAsync();
                isStandaloneAttachPaymentTypeUsed = requestContent.Contains("\"AttachmentType\":\"Standalone\"");
            };

            bool isPaymentInstrumentContextUsed = false;
            bool postAttachProfileCalled = false;
            bool useCheckoutRequestAttachProfile = false;
            bool usePaymentRequestAttachProfile = false;
            bool useCheckoutRequestConfirm = false;
            bool usePaymentRequestConfirm = false;

            PXSettings.PaymentOrchestratorService.PreProcess = async (poServiceRequest) =>
            {
                if (poServiceRequest.RequestUri.AbsolutePath.Contains("/confirm"))
                {
                    string requestContent = await poServiceRequest.Content.ReadAsStringAsync();
                    isPaymentInstrumentContextUsed = requestContent.Contains("paymentInstrumentId") && requestContent.Contains("PaymentInstruments");
                    useCheckoutRequestConfirm = poServiceRequest.RequestUri.AbsolutePath.Contains("/checkoutRequests");
                    usePaymentRequestConfirm = poServiceRequest.RequestUri.AbsolutePath.Contains("/paymentRequests");
                }

                if (paymentMethodType.Equals("applepay", StringComparison.OrdinalIgnoreCase)
                    || paymentMethodType.Equals("googlepay", StringComparison.OrdinalIgnoreCase))
                {
                    if (poServiceRequest.RequestUri.AbsolutePath.Contains("attachprofile"))
                    {
                        postAttachProfileCalled = poServiceRequest.Method == HttpMethod.Post;
                        useCheckoutRequestAttachProfile = poServiceRequest.RequestUri.AbsolutePath.Contains("/checkoutRequests");
                        usePaymentRequestAttachProfile = poServiceRequest.RequestUri.AbsolutePath.Contains("/paymentRequests");
                    }
                }
            };

            string googlePayData = "{\"apiVersionMinor\":0,\"apiVersion\":2,\"paymentMethodData\":{\"description\":\"Visa •••• 1234\",\"token\":\"<PCE Token which get from /getToken/apay>\",\"type\":\"CARD\",\"info\":{\"cardNetwork\":\"VISA\",\"cardDetails\":\"1234\",\"billingAddress\":{\"phoneNumber\":\"+1 111-111-1111\",\"address3\":\"\",\"sortingCode\":\"\",\"address2\":\"\",\"countryCode\":\"US\",\"address1\":\"1 Microsoft Way\",\"postalCode\":\"98052\",\"name\":\"First Last\",\"locality\":\"Redmond\",\"administrativeArea\":\"WA\"}}},\"shippingOptionData\":{\"id\":\"shipping-001\"},\"shippingAddress\":{\"address3\":\"\",\"sortingCode\":\"\",\"address2\":\"\",\"countryCode\":\"US\",\"address1\":\"1 Microsoft Way\",\"postalCode\":\"98052-7073\",\"name\":\"First Last\",\"locality\":\"REDMOND\",\"administrativeArea\":\"WA\"},\"email\":\"mstest@gmail.com\"}";
            string applePayData = "{\"billingContact\":{\"addressLines\":[\"1 way microsoft\"],\"administrativeArea\":\"WA\",\"country\":\"United States\",\"countryCode\":\"US\",\"familyName\":\"LastName\",\"givenName\":\"FirstName\",\"locality\":\"redmond\",\"postalCode\":\"98052\",\"subAdministrativeArea\":\"\",\"subLocality\":\"\"},\"shippingContact\":{\"addressLines\":[\"1 microsoft way\"],\"administrativeArea\":\"WA\",\"country\":\"United States\",\"countryCode\":\"US\",\"emailAddress\":\"mstest@gmail.com\",\"familyName\":\"LastName\",\"givenName\":\"FirstName\",\"locality\":\"redmond\",\"phoneNumber\":\"1234567890\",\"postalCode\":\"98052-8300\",\"subAdministrativeArea\":\"\",\"subLocality\":\"\"},\"token\":{\"token\":\"<PCE Token which get from /getToken/apay>\",\"paymentMethod\":{\"displayName\":\"Visa 1234\",\"network\":\"Visa\",\"type\":\"credit\"},\"transactionIdentifier\":\"8db6cbfe1b35798839f8c14075393f4533b62fa0783c70022d5c32dea4eab141\"}}";

            var payload = new
            {
                selected_PIID = paymentMethodType.Equals("visa", StringComparison.OrdinalIgnoreCase) ? "piid" : null,
                paymentMethodType = paymentMethodType,
                confirmCountry = "US",
                expressCheckoutPaymentData = paymentMethodType.Equals("googlepay", StringComparison.OrdinalIgnoreCase) ? googlePayData : applePayData,
            };

            HttpResponseMessage result = await SendRequestPXService(requestUrl, HttpMethod.Post, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType), headers);

            // Assert
            bool isGooglePayOrApplePay = paymentMethodType.Equals("googlepay", StringComparison.OrdinalIgnoreCase) || paymentMethodType.Equals("applepay", StringComparison.OrdinalIgnoreCase);
            if (isGooglePayOrApplePay)
            {
                Assert.IsTrue(postAttachProfileCalled, "Post attach profile was not called");

                if (useUsePaymentRequestApi)
                {
                    Assert.IsTrue(usePaymentRequestAttachProfile, "PaymentRequest attach profile should be used for payment request attach profile call.");
                }
                else
                {
                    Assert.IsTrue(useCheckoutRequestAttachProfile, "CheckoutRequest attach profile should be used for checkout request attach profile call.");
                }
            }

            Assert.AreEqual(isStandaloneAttachPaymentTypeUsed, isGooglePayOrApplePay, "Standalone Attach Payment Type is not used for post PI of googlepay or apple pay in confirm");
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode, "Expected statuscode is not found");
            Assert.IsTrue(isPaymentInstrumentContextUsed, "Payment Instrument context is not used in confirm");

            if (useUsePaymentRequestApi)
            {
                Assert.IsTrue(usePaymentRequestConfirm, "PaymentRequest confirm should be used for payment request confirm call.");
            }
            else
            {
                Assert.IsTrue(useCheckoutRequestConfirm, "CheckoutRequest confirm should be used for checkout request confirm call.");
            }

            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PaymentOrchestratorService.ResetToDefaults();
            PXFlightHandler.ResetToDefault();
        }

        [DataRow("visa", false, "02")]
        [DataRow("googlepay", false, "03")]
        [DataRow("visa", true, "03")]
        [DataRow("googlepay", true, "03")]
        [DataRow("visa", false)]
        [DataRow("googlepay", false)]
        [DataRow("visa", true)]
        [DataRow("googlepay", true)]
        [DataRow("visa", false, "02", true)]
        [DataRow("googlepay", false, "03", true)]
        [DataRow("visa", true, "03", true)]
        [DataRow("googlepay", true, "03", true)]
        [DataRow("visa", false, "", true)]
        [DataRow("googlepay", false, "", true)]
        [DataRow("visa", true, "", true)]
        [DataRow("googlepay", true, "", true)]
        [TestMethod]
        public async Task CheckoutRequestExConfirmPSD2Tests(string paymentMethodType, bool useUsePaymentRequestApi, string challengeWindowSize = "", bool challeneWindowSizeValueFromURL = false)
        {
            // Arrange
            if (paymentMethodType.Equals("googlepay", StringComparison.OrdinalIgnoreCase))
            {
                string id = "cw_gpay_cc80ac8e-3e33-40f3-9fed-6efb5be47762";
                global::Tests.Common.Model.Pims.PaymentInstrument expectedPI = PimsMockResponseProvider.GetPaymentInstrument("Account013", id);
                PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(expectedPI));
            }

            string expectedPSSResponse = "{\"confirm\":{\"template\":\"defaulttemplate\",\"redirectionPattern\":\"inline\",\"challengeWindowSize\":\" " + challengeWindowSize.ToLower().ToString()  + "\"}}";
            PXSettings.PartnerSettingsService.ArrangeResponse(expectedPSSResponse);

            var extendedPI = PimsMockResponseProvider.GetPaymentInstrument("Account001", "Account001-Pi001-Visa");
            extendedPI.PaymentInstrumentDetails.RequiredChallenge = new List<string> { "3ds2" };
            PXSettings.PimsService.ArrangeResponse(JsonConvert.SerializeObject(extendedPI), HttpStatusCode.OK, null, ".*/extendedView.*");

            PXSettings.PayerAuthService.ArrangeResponse(
               method: HttpMethod.Post,
               urlPattern: ".*/GetThreeDSMethodURL.*",
               statusCode: HttpStatusCode.OK,
               content: "{\"three_ds_server_trans_id\":\"7b41a540-cbf8-4ada-85f6-24d4705f983b\",\"three_ds_method_url\":\"https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net/acs/fingerprint\",\"tracking_id\":\"00000000-0000-0000-0000-000000000000\"}");

            var paymentRequestClientActions = new PaymentRequestClientActions
            {
                PaymentRequestId = "cr_123",
                Status = PaymentRequestStatus.PendingClientAction,
                Country = "US",
                Currency = "USD",
                Amount = 100,
                PaymentInstruments = new List<PaymentInstrument>(),
                ClientActions = new List<PaymentRequestClientAction>
                {
                    new PaymentRequestClientAction
                    {
                        Type = Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService.ClientActionType.HandleChallenge,
                        ChallengeType = PaymentInstrumentChallengeType.ThreeDs2,
                        PaymentInstrument = new PaymentInstrument
                        {
                            PaymentInstrumentId = "piid",
                            Usage = PaymentInstrumentUsage.PrimaryPaymentInstrument,
                            PaymentMethodType = Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService.PaymentMethodType.Visa
                        }
                    }
                }
            };

            PXSettings.PaymentOrchestratorService.ArrangeResponse(JsonConvert.SerializeObject(paymentRequestClientActions));

            string requestId = useUsePaymentRequestApi ? "pr_39c93cc0-e855-42bc-8aca-183a572e14bc" : "cr_39c93cc0-e855-42bc-8aca-183a572e14bc";
            string requestUrl = string.Format($"/v7.0/PaymentClient/CheckoutRequestsEx/{requestId}/confirm");
            string requestContext = $"{{\"tenantId\":\"battle.net\",\"tenantCustomerId\":\"abc\",\"requestId\":\"{requestId}\",\"paymentAccountId\":\"123\",\"checkoutRequestId\":\"{requestId}\"}}";

            string flightFlags = "PXEnablePSD2ForGooglePay";
            if (useUsePaymentRequestApi)
            {
                flightFlags += ",UsePaymentRequestApi";
            }

            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "x-ms-request-context", requestContext },
                { "x-ms-test", "{\"scenarios\":\"px-service-psd2-e2e-emulator\",\"contact\":\"pidlsdk\"}" },
                { "x-ms-flight", flightFlags }
            };

            string googlePayData = "{\"apiVersionMinor\":0,\"apiVersion\":2,\"paymentMethodData\":{\"description\":\"Visa •••• 1234\",\"token\":\"<PCE Token which get from /getToken/apay>\",\"type\":\"CARD\",\"info\":{\"cardNetwork\":\"VISA\",\"cardDetails\":\"1234\",\"billingAddress\":{\"phoneNumber\":\"+1 111-111-1111\",\"address3\":\"\",\"sortingCode\":\"\",\"address2\":\"\",\"countryCode\":\"US\",\"address1\":\"1 Microsoft Way\",\"postalCode\":\"98052\",\"name\":\"First Last\",\"locality\":\"Redmond\",\"administrativeArea\":\"WA\"}}},\"shippingOptionData\":{\"id\":\"shipping-001\"},\"shippingAddress\":{\"address3\":\"\",\"sortingCode\":\"\",\"address2\":\"\",\"countryCode\":\"US\",\"address1\":\"1 Microsoft Way\",\"postalCode\":\"98052-7073\",\"name\":\"First Last\",\"locality\":\"REDMOND\",\"administrativeArea\":\"WA\"},\"email\":\"mstest@gmail.com\"}";

            var payload = new
            {
                selected_PIID = paymentMethodType.Equals("visa", StringComparison.OrdinalIgnoreCase) ? "piid" : null,
                paymentMethodType = paymentMethodType,
                confirmCountry = "US",
                expressCheckoutPaymentData = googlePayData,
            };

            HttpResponseMessage result = await SendRequestPXService(requestUrl, HttpMethod.Post, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType), headers);
            string responseContent = await result.Content.ReadAsStringAsync();
            PIDLResource pidl = ReadSinglePidlResourceFromJson(responseContent);

            // Assert
            Assert.IsNotNull(pidl.ClientAction, "Client action missing");
            Assert.AreEqual(global::Tests.Common.Model.Pidl.ClientActionType.Pidl, pidl.ClientAction.ActionType);
            Assert.IsNotNull(pidl.ClientAction.Context, "Client action context missing");

            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PaymentOrchestratorService.ResetToDefaults();
            PXFlightHandler.ResetToDefault();
        }

        [TestMethod]
        public async Task CheckoutRequestExConfirmCVVChallengeTests()
        {
            // Arrange
            var paymentRequestClientActions = new PaymentRequestClientActions
            {
                PaymentRequestId = "cr_123",
                Status = PaymentRequestStatus.PendingClientAction,
                Country = "US",
                Currency = "USD",
                Amount = 100,
                Language = "en-us",
                PaymentInstruments = new List<PaymentInstrument>(),
                ClientActions = new List<PaymentRequestClientAction>
                {
                    new PaymentRequestClientAction
                    {
                        Type = Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService.ClientActionType.HandleChallenge,
                        ChallengeType = PaymentInstrumentChallengeType.Cvv,
                        PaymentInstrument = new PaymentInstrument
                        {
                            PaymentInstrumentId = "piid",
                            Usage = PaymentInstrumentUsage.PrimaryPaymentInstrument,
                            PaymentMethodType = Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService.PaymentMethodType.Visa
                        }
                    }
                }
            };

            PXSettings.PaymentOrchestratorService.ArrangeResponse(JsonConvert.SerializeObject(paymentRequestClientActions));

            string requestId = "pr_39c93cc0-e855-42bc-8aca-183a572e14bc";
            string requestUrl = string.Format($"/v7.0/PaymentClient/CheckoutRequestsEx/{requestId}/confirm");
            string requestContext = $"{{\"tenantId\":\"battle.net\",\"tenantCustomerId\":\"abc\",\"requestId\":\"{requestId}\",\"paymentAccountId\":\"123\",\"checkoutRequestId\":\"{requestId}\"}}";

            string flightFlags = "UsePaymentRequestApi";

            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "x-ms-request-context", requestContext },
                { "x-ms-flight", flightFlags }
            };

            string googlePayData = "{\"apiVersionMinor\":0,\"apiVersion\":2,\"paymentMethodData\":{\"description\":\"Visa •••• 1234\",\"token\":\"<PCE Token which get from /getToken/apay>\",\"type\":\"CARD\",\"info\":{\"cardNetwork\":\"VISA\",\"cardDetails\":\"1234\",\"billingAddress\":{\"phoneNumber\":\"+1 111-111-1111\",\"address3\":\"\",\"sortingCode\":\"\",\"address2\":\"\",\"countryCode\":\"US\",\"address1\":\"1 Microsoft Way\",\"postalCode\":\"98052\",\"name\":\"First Last\",\"locality\":\"Redmond\",\"administrativeArea\":\"WA\"}}},\"shippingOptionData\":{\"id\":\"shipping-001\"},\"shippingAddress\":{\"address3\":\"\",\"sortingCode\":\"\",\"address2\":\"\",\"countryCode\":\"US\",\"address1\":\"1 Microsoft Way\",\"postalCode\":\"98052-7073\",\"name\":\"First Last\",\"locality\":\"REDMOND\",\"administrativeArea\":\"WA\"},\"email\":\"mstest@gmail.com\"}";

            var payload = new
            {
                selected_PIID = "piid",
                paymentMethodType = "visa",
                confirmCountry = "US",
                expressCheckoutPaymentData = googlePayData,
            };

            HttpResponseMessage result = await SendRequestPXService(requestUrl, HttpMethod.Post, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType), headers);
            string responseContent = await result.Content.ReadAsStringAsync();
            PIDLResource pidl = ReadSinglePidlResourceFromJson(responseContent);

            // Assert
            Assert.IsNotNull(pidl.ClientAction, "Client action missing");
            Assert.AreEqual(global::Tests.Common.Model.Pidl.ClientActionType.Pidl, pidl.ClientAction.ActionType);
            Assert.IsNotNull(pidl.ClientAction.Context, "Client action context missing");

            List<PIDLResource> pidls = pidl.ClientAction.Context as List<PIDLResource>;
            Assert.IsTrue(pidls[0].DisplayPages.Count > 0, "Display pages should not be empty");
            PageDisplayHint displayPage = pidls[0].DisplayPages[0];
            Assert.AreEqual("challenge_rootPage", displayPage.HintId, "The displayPage hint ID should be 'challenge_rootPage'");

            // Find the submit button and verify its URL format
            ButtonDisplayHint submitButton = pidls[0].GetDisplayHintById("nextButton") as ButtonDisplayHint;
            Assert.IsNotNull(submitButton, "Submit button is required in the CVV challenge");

            var submitLink = JsonConvert.DeserializeObject<RestLink>(JsonConvert.SerializeObject(submitButton.Action.Context));
            Assert.IsNotNull(submitLink, "Submit button should have a RestLink action context");
            string expectedUrlPattern = $"https://{{pifd-endpoint}}/PaymentClient/paymentRequestsEx/{requestId}/attachChallengeData";
            Assert.IsTrue(submitLink.Href.EndsWith($"PaymentClient/paymentRequestsEx/{requestId}/attachChallengeData"), $"Submit button URL should match format: {expectedUrlPattern}, but was {submitLink.Href}");
            Assert.AreEqual("POST", submitLink.Method, "Submit button should use POST method");

            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.PaymentOrchestratorService.ResetToDefaults();
            PXFlightHandler.ResetToDefault();
        }
    }
}
