// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace CIT.PXService.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.NetworkInformation;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using global::Tests.Common.Model;
    using global::Tests.Common.Model.Pidl;
    using global::Tests.Common.Model.Pims;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.PXService;
    using Microsoft.Commerce.Payments.PXService.Model.PXInternal;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using SelfHostedPXServiceCore;
    using SelfHostedPXServiceCore.Mocks;

    [TestClass]
    public class TestBase
    {
        public TestContext TestContext { get; set; }
        
        public static SelfHostedPxService SelfHostedPxService { get; private set; }
        
        public static PXServiceSettings PXSettings { get; private set; }

        public static HttpClient PXClient { get; private set; }

        public static Uri PXBaseUri { get; private set; }
        
        public static PXServiceCorsHandler PXCorsHandler { get; private set; }

        public static PXServiceHandler PXHandler { get; private set; }

        public static PXServiceFlightHandler PXFlightHandler { get; private set; }
        
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            // Spin up the PX service in-memory using TestServer so tests can issue HTTP
            // requests without opening real network sockets.
            SelfHostedPxService = SelfHostedPxService.StartInMemory(null, false, true);
            PXHandler = SelfHostedPxService.PXHandler;
            PXSettings = SelfHostedPxService.PXSettings;
            PXBaseUri = SelfHostedPxService.PxHostableService.BaseUri;
            PXClient = SelfHostedPxService.PxHostableService.HttpSelfHttpClient;
            PXBaseUri = SelfHostedPxService.PxHostableService.BaseUri;
            PXCorsHandler = SelfHostedPxService.PXCorsHandler;
            PXFlightHandler = SelfHostedPxService.PXFlightHandler;

            // Verify routing is configured – if this fails the console output from
            // HostableService will show which endpoint could not be resolved.
            var probeResponse = PXClient.GetAsync(GetPXServiceUrl("/v7.0/probe")).GetAwaiter().GetResult();
            Assert.AreEqual(HttpStatusCode.OK, probeResponse.StatusCode, "PX probe endpoint is unreachable");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            SelfHostedPxService.ResetDependencies();
        }

        [AssemblyCleanup]
        public static void Cleanup()
        {
            SelfHostedPxService.Dispose();
        }

        public static string GetPXServiceUrl(string relativePath)
        {
            Uri fullUri = new Uri(PXBaseUri, relativePath);
            return fullUri.AbsoluteUri;
        }

        public static async Task GetRequest(string url, Dictionary<string, string> requestHeaders, List<string> flightFeatures, Action<HttpStatusCode, string, HttpResponseHeaders> responseVerification)
        {
            if (flightFeatures != null)
            {
                PXHandler.PreProcess = (pxRequest) =>
                {
                    pxRequest.Properties[PXService.GlobalConstants.RequestPropertyKeys.ExposedFlightFeatures] = flightFeatures == null ? new List<string>() : flightFeatures;
                };
            }

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, GetPXServiceUrl(url));
            requestHeaders?.ToList().ForEach(header => request.Headers.Add(header.Key, header.Value));

            var response = await PXClient.SendAsync(request);
            responseVerification(response.StatusCode, await response.Content.ReadAsStringAsync(), response.Headers);
        }

        public static async Task<List<PIDLResource>> GetPidlFromPXService(string url, HttpStatusCode statusCode = HttpStatusCode.OK, string flightNames = null, Dictionary<string, string> additionaHeaders = null)
        {
            if (!string.IsNullOrEmpty(flightNames))
            {
                PXHandler.PreProcess = (request) =>
                {
                    request.Properties[PXService.GlobalConstants.RequestPropertyKeys.ExposedFlightFeatures] = string.IsNullOrEmpty(flightNames) ? new List<string>() : flightNames.Split(',').ToList<string>();
                };
            }

            HttpRequestMessage pxRequest = new HttpRequestMessage(HttpMethod.Get, GetPXServiceUrl(url));
            additionaHeaders?.ToList()?.ForEach(pair => pxRequest.Headers.TryAddWithoutValidation(pair.Key, pair.Value));

            var response = await PXClient.SendAsync(pxRequest);
            string responseContent = await response.Content.ReadAsStringAsync();

            Assert.AreEqual(statusCode, response.StatusCode, $"Failed for URL:{url} with PXResponse:{responseContent}");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return ReadPidlResourceFromJson(responseContent);
            }
            else
            {
                return new List<PIDLResource>();
            }
        }

        public static async Task<List<PIDLResource>> GetPidlFromPXServiceWithPartnerHeader(string url, string headerKey, string headerValue, string leftoverHeader)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, GetPXServiceUrl(url));
            request.Headers.Add(headerKey, headerValue);

            var response = await PXClient.SendAsync(request);

            IEnumerable<string> headerValues;
            Assert.IsTrue(request.Headers.TryGetValues(headerKey, out headerValues));
            headerValue = headerValues.FirstOrDefault();
            Assert.AreEqual(leftoverHeader, headerValue);

            string responseJson = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, responseJson);
            return ReadPidlResourceFromJson(responseJson);
        }

        public static async Task<List<PIDLResource>> GetPidlFromPXServiceWithFlight(string url, List<string> flightNames)
        {
            List<string> enabledFeatures = null;
            PXHandler.PreProcess = (request) =>
            {
                enabledFeatures = request.Properties[PXService.GlobalConstants.RequestPropertyKeys.ExposedFlightFeatures] as List<string> ?? new List<string>();
                foreach (string flightName in flightNames)
                {
                    enabledFeatures.Add(flightName);
                }
            };

            var response = await PXClient.GetAsync(GetPXServiceUrl(url));
            string responseJson = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, responseJson);
            return ReadPidlResourceFromJson(responseJson);
        }

        public static async Task<List<PIDLResource>> GetPidlFromPXServiceWithFlightOverrides(string url, string flightNames)
        {
            PXHandler.PreProcess = (request) =>
            {
                request.Properties[PXService.GlobalConstants.RequestPropertyKeys.ExposedFlightFeatures] = string.IsNullOrEmpty(flightNames) ? new List<string>() : flightNames.Split(',').ToList<string>();
            };

            var response = await PXClient.GetAsync(GetPXServiceUrl(url));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            string pidlJson = await response.Content.ReadAsStringAsync();
            return ReadPidlResourceFromJson(pidlJson);
        }

        public static async Task<object> SendRequestPXServiceWithFlightOverrides(string url, HttpMethod method, object payload, Dictionary<string, string> additionaHeaders, string flightNames)
        {
            PXHandler.PreProcess = (r) =>
            {
                r.Properties[PXService.GlobalConstants.RequestPropertyKeys.ExposedFlightFeatures] = string.IsNullOrEmpty(flightNames) ? new List<string>() : flightNames.Split(',').ToList<string>();
            };

            var request = new HttpRequestMessage(method, url);
            additionaHeaders?.ToList()?.ForEach(pair => request.Headers.TryAddWithoutValidation(pair.Key, pair.Value));
            request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType);
            HttpResponseMessage response = await PXClient.SendAsync(request);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            string pidlJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject(pidlJson);
        }

        public static async Task<object> SendRequestPXServiceWithFlightOverrides(string url, HttpMethod method, object payload, string flightNames)
        {
            PXHandler.PreProcess = (r) =>
            {
                r.Properties[PXService.GlobalConstants.RequestPropertyKeys.ExposedFlightFeatures] = string.IsNullOrEmpty(flightNames) ? new List<string>() : flightNames.Split(',').ToList<string>();
            };

            var request = new HttpRequestMessage(method, url);
            request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType);
            return await PXClient.SendAsync(request);
        }

        public static async Task<object> SendRequestPXService(string url, HttpMethod method, object payload)
        {
            var request = new HttpRequestMessage(method, url);
            request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType);
            return await PXClient.SendAsync(request);
        }

        public static async Task<HttpResponseMessage> SendRequestPXService(string url, HttpMethod method, StringContent content, Dictionary<string, string> additionalHeaders)
        {
            var request = new HttpRequestMessage(method, url);
            additionalHeaders?.ToList()?.ForEach(pair => request.Headers.TryAddWithoutValidation(pair.Key, pair.Value));
            request.Content = content;
            return await PXClient.SendAsync(request);
        }

        public static async Task<List<PaymentInstrument>> ListPIFromPXService(string url)
        {
            var response = await PXClient.GetAsync(GetPXServiceUrl(url));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            string piJson = await response.Content.ReadAsStringAsync();
            var pis = JsonConvert.DeserializeObject<List<PaymentInstrument>>(piJson);

            return pis;
        }

        public static async Task<PaymentInstrument> GetPIFromPXService(string url)
        {
            var response = await PXClient.GetAsync(GetPXServiceUrl(url));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            string piJson = await response.Content.ReadAsStringAsync();
            var pi = JsonConvert.DeserializeObject<PaymentInstrument>(piJson);

            return pi;
        }

        public static async Task<PaymentInstrument> ResumePIFromPXService(string url)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, GetPXServiceUrl(url));
            var response = await PXClient.SendAsync(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            string piJson = await response.Content.ReadAsStringAsync();
            var pi = JsonConvert.DeserializeObject<PaymentInstrument>(piJson);

            return pi;
        }

        public static List<PIDLResource> ReadPidlResourceFromJson(string pidlJson)
        {
            var pidls = JsonConvert.DeserializeObject<List<PIDLResource>>(
                pidlJson,
                new JsonConverter[]
                {
                    new DisplayHintDeserializer(),
                    new DataSourceDeserializer(),
                    new PidlObjectDeserializer()
                });

            return pidls;
        }

        public static PIDLResource ReadSinglePidlResourceFromJson(string pidlJson)
        {
            var pidls = JsonConvert.DeserializeObject<PIDLResource>(
                pidlJson,
                new JsonConverter[]
                {
                    new DisplayHintDeserializer(),
                    new PidlObjectDeserializer()
                });

            return pidls;
        }

        public static void VerifyPaymentMethodPidl(string responseBody, string expectedOperation, string expectedCountry)
        {
            var pidls = ReadPidlResourceFromJson(responseBody);
            pidls.ForEach(
                pidl =>
                {
                    Assert.AreEqual(PXService.GlobalConstants.PidlDescriptionTypes.PaymentMethod, pidl.Identity["description_type"]);
                    Assert.AreEqual(expectedOperation, pidl.Identity["operation"]);
                    Assert.AreEqual(expectedCountry, pidl.Identity["country"], true);
                });
        }

        public static void VerifyProfilePidl(string responseBody, string expectedType, string expectedOperation, string expectedCountry)
        {
            var pidls = ReadPidlResourceFromJson(responseBody);
            pidls.ForEach(
                pidl =>
                {
                    Assert.AreEqual(PXService.GlobalConstants.PidlDescriptionTypes.Profile, pidl.Identity["description_type"]);
                    Assert.AreEqual(expectedOperation, pidl.Identity["operation"], true);
                    Assert.AreEqual(expectedCountry, pidl.Identity["country"], true);
                    Assert.AreEqual(expectedType, pidl.Identity["type"], true);
                });
        }

        public static void VerifySelectPaymentInstrumentPidl(string responseBody, string expectedOperation, string expectedCountry, string expectedResourceId)
        {
            var pidls = ReadPidlResourceFromJson(responseBody);
            pidls.ForEach(
                pidl =>
                {
                    Assert.AreEqual(PXService.GlobalConstants.PidlDescriptionTypes.PaymentInstrument, pidl.Identity["description_type"], true);
                    Assert.AreEqual(expectedOperation, pidl.Identity["operation"], true);
                    Assert.AreEqual(expectedCountry, pidl.Identity["country"], true);
                    Assert.AreEqual(expectedResourceId, pidl.Identity["resource_id"], true);
                });
        }

        public static void VerifyDisplaySinglePiPidl(string responseBody, string expectedResponse, int textHintIndex)
        {
            var pidls = ReadPidlResourceFromJson(responseBody);
            var root = pidls[0]?.DisplayPages[0];
            var page = root?.Members[textHintIndex] as GroupDisplayHint;
            var cardText = page?.Members[1] as TextDisplayHint;
            var text = cardText?.DisplayContent;

            Assert.AreEqual(expectedResponse, text);
        }

        protected static void ValidatePidlPropertyRegex(PIDLResource pidl, string propertyName, string value, bool isValid, string expectedRegexErrorMessage = "", bool canRegexbeEmpty = false, string errorToShowOnAssertFail = "")
        {
            Assert.IsNotNull(pidl, "PIDL is not expected to be null");

            PropertyDescription propertyDescription = pidl.GetPropertyDescriptionByPropertyName(propertyName);
            string propertyRegex = propertyDescription?.Validation?.Regex;
            
            if (!canRegexbeEmpty || (canRegexbeEmpty && !string.IsNullOrEmpty(propertyRegex)))
            {
                Assert.IsNotNull(propertyRegex, $"Regex for {propertyName} is not expected to be null");

                if (isValid)
                {
                    Assert.IsTrue(Regex.Match(value, propertyRegex).Success, string.IsNullOrEmpty(errorToShowOnAssertFail) ? $"{propertyName}: '{value}' should match with the regex '{propertyRegex}'" : errorToShowOnAssertFail);
                }
                else
                {
                    Assert.IsFalse(Regex.Match(value, propertyRegex).Success, string.IsNullOrEmpty(errorToShowOnAssertFail) ? $"Invalid {propertyName}: '{value}' shouldn't match with the regex '{propertyRegex}'" : errorToShowOnAssertFail);
                }

                if (!string.IsNullOrEmpty(expectedRegexErrorMessage))
                {
                    Assert.AreEqual(expectedRegexErrorMessage, propertyDescription.Validation.ErrorMessage, "Regex validation error message is not as expected");
                }
            }
        }

        /// <summary>
        /// Validates the Property Transformation Regex similar to pidl sdk regex tranformation process.
        /// </summary>
        /// <param name="pidl">Single Pidl</param>
        /// <param name="target">Target for Tranformation. e.g. forSubmit,forDisplay</param>
        /// <param name="propertyName">Property for validating regex</param>
        /// <param name="commonTestValues">Used for negative testing. List of common values from which single value is used for testing that is not present in tranform input regex.</param>
        /// <param name="mandatoryTestValues">List of mandatory input values which all are tested for Pidl</param>
        protected static void ValidatePropertyTransformationRegex(PIDLResource pidl, string target, string propertyName, List<string> commonTestValues, string mandatoryTestValues)
        {
            List<string> testValuesCollection = string.IsNullOrEmpty(mandatoryTestValues) ? new List<string>() : mandatoryTestValues.Split(',').ToList();

            PropertyDescription propertyDescription = pidl.GetPropertyDescriptionByPropertyName(propertyName);
            string inputRegex = null;
            string transformRegex = null;

            if (propertyDescription?.Transformation != null && propertyDescription.Transformation.ContainsKey(target))
            {
                inputRegex = propertyDescription.Transformation[target].InputRegex;
                transformRegex = propertyDescription.Transformation[target].TransformRegex;
            }

            Assert.IsNotNull(inputRegex, $"Tranformation Regex for {propertyName} is not expected to be null");
            Assert.IsNotNull(commonTestValues, "commonTestValues is expected to be not null");

            foreach (string testValue in commonTestValues)
            {
                if (!inputRegex.Contains(testValue) && !testValuesCollection.Contains(testValue)) 
                {
                    testValuesCollection.Add(testValue);
                    break;
                }
            }

            if (testValuesCollection.Count == 0)
            {
                Assert.Fail("Provided list of input values isn't enough for testing");
            }

            Assert.IsFalse(Regex.Match(transformRegex, inputRegex).Success);

            foreach (string testInput in testValuesCollection)
            {
                // pidl sdk transforms in the similar way as below
                string transformedValue = Regex.Replace(testInput, inputRegex, transformRegex);
                Assert.IsTrue(string.Equals(transformRegex, transformedValue) || inputRegex.Contains(transformedValue));
            }
        }

        protected string ToBase64(string value)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(plainTextBytes);
        }

        private static string GetAvailablePort()
        {
            var netProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpListeners = netProperties.GetActiveTcpListeners();
            var udpListeners = netProperties.GetActiveUdpListeners();

            var portsInUse = new List<int>();
            portsInUse.AddRange(tcpListeners.Select(tl => tl.Port));
            portsInUse.AddRange(udpListeners.Select(ul => ul.Port));

            int firstAvailablePort = 0;
            for (int port = 49152; port < 65535; port++)
            {
                if (!portsInUse.Contains(port))
                {
                    firstAvailablePort = port;
                    break;
                }
            }

            return firstAvailablePort.ToString();
        }

        protected static void ValidatePidlProperties(PIDLResource pidl, bool featureStatus)
        {
            IEnumerable<DisplayHint> displayHints = pidl.DisplayHints();

            foreach (DisplayHint displayHint in displayHints)
            {
                PropertyDisplayHint displayProperty = displayHint as PropertyDisplayHint;

                if (displayProperty != null)
                {
                    Assert.AreEqual("true", displayProperty.ShowDisplayName, "ShowDisplayName should be true");

                    if (featureStatus)
                    {
                        if (displayProperty.PossibleValues == null && displayProperty.SelectType == null)
                        {
                            Assert.IsTrue(displayProperty.DisplayExample.Contains(displayProperty.DisplayName));
                        }
                    }
                    else
                    {
                        Assert.IsNull(displayProperty.DisplayExample);
                    }
                }
            }
        }

        /// <summary>
        /// Validates the PlaceHolder for DisplayHints for the given PIDL.
        /// </summary>
        /// <param name="pidl">PIDL Resource</param>
        /// <param name="propertyName">Property Name</param>
        /// <param name="expectedPlaceHolderValue">Expected placeHolder Value</param>
        public void ValidatePlaceHolderForDisplayHints(PIDLResource pidl, string propertyName, string expectedPlaceHolderValue)
        {
            Assert.IsNotNull(pidl.DisplayPages, "DisplayPages is expected to be not null");

            PropertyDisplayHint displayHintIdForPropertyName = pidl.GetDisplayHintByPropertyName(propertyName) as PropertyDisplayHint;
            Assert.IsNotNull(displayHintIdForPropertyName, $"DisplayHint for property '{propertyName}' is expected to be not null");

            if (!string.IsNullOrEmpty(expectedPlaceHolderValue))
            {
                if (displayHintIdForPropertyName.DisplayExample != null && displayHintIdForPropertyName.DisplayExample.Contains(expectedPlaceHolderValue))
                {
                    Assert.IsTrue(displayHintIdForPropertyName.DisplayExample.Contains(expectedPlaceHolderValue), $"DisplayExample should contain the expected placeholder value '{expectedPlaceHolderValue}'");
                }
                else if (!string.IsNullOrEmpty(displayHintIdForPropertyName.DisplayDescription) && displayHintIdForPropertyName.DisplayDescription.Contains(expectedPlaceHolderValue))
                {
                    Assert.IsTrue(displayHintIdForPropertyName.DisplayDescription.Contains(expectedPlaceHolderValue), $"DisplayDescription should contain the expected placeholder value '{expectedPlaceHolderValue}'");
                }
            }
        }

        public static void ValidateOrderSummaryDescriptions(List<PIDLResource> pidls, string expectedValidation, string expectedIdentity = "checkout", string expectedTax = "$--", string component = null)
        {
            if (component?.Equals("ordersummary") ?? true)
            {
                ImageDisplayHint image = pidls[0].GetDisplayHintById("orderItemImage") as ImageDisplayHint;
                Assert.AreEqual(expectedValidation, image.SourceUrl, "Order summary image url not expected");
                Assert.AreEqual("$150.00", pidls[0].GetPropertyDescriptionByPropertyName("cart_subtotal").DefaultValue, "cart_subtotal value is not as expected");
                Assert.AreEqual(expectedTax, pidls[0].GetPropertyDescriptionByPropertyName("cart_tax")?.DefaultValue, "cart_tax value is not as expected");
                Assert.AreEqual(expectedIdentity.ToLower(), pidls[0].Identity["description_type"].ToLower(), "PIDL idenity not expected");
            }
        }

        public static void ValidateQuickPaymentDescription(List<PIDLResource> pidls, string expectedValidation = "success", bool isPSD2 = true, string paymentMethodType = "*", string language = "en-us", string component = null, string flightName = null)
        {
            if (component?.Equals("quickPayment") ?? true)
            {
                Assert.IsNotNull(pidls[0].DataSources, "PIDL data source should not be null for quickpayment");

                var dataSource = pidls[0].DataSources["walletConfig"];
                Assert.IsNotNull(dataSource, "PIDL data source should not be null for quick payment");

                var walletConfig = JsonConvert.DeserializeObject<WalletConfig>(JsonConvert.SerializeObject(dataSource.Members.FirstOrDefault()));
                Assert.IsNotNull(walletConfig, "WalletConfig should not be null for quick payment");
                Assert.IsNotNull(walletConfig.PIDLConfig, "PIDLConfig should not be null for quick payment");
                if (language?.Equals("it-IT", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    Assert.AreEqual("importo dovuto più imposte applicabili", walletConfig.PaymentInstrumentHandlers[0].PayLabel, "Googlepay handler Paylabel should be importo dovuto più le tasse applicabili");
                    Assert.AreEqual("importo dovuto più imposte applicabili", walletConfig.PaymentInstrumentHandlers[1].PayLabel, "Applepay handler Paylabel should be importo dovuto più le tasse applicabili");
                }
                else
                {
                    Assert.AreEqual("amount due plus applicable taxes", walletConfig.PaymentInstrumentHandlers[0].PayLabel, "Googlepay handler Paylabel should be amount due plus applicable taxes");
                    Assert.AreEqual("amount due plus applicable taxes", walletConfig.PaymentInstrumentHandlers[1].PayLabel, "Applepay handler Paylabel should be amount due plus applicable taxes");
                }

                if (isPSD2)
                {
                    Assert.IsTrue(walletConfig.PaymentInstrumentHandlers?.FirstOrDefault()?.AllowedAuthMethodsPerCountry?.ContainsKey("gb"));
                    Assert.IsTrue(walletConfig.PaymentInstrumentHandlers?.LastOrDefault()?.AllowedAuthMethodsPerCountry?.ContainsKey("gb"));
                }

                var googlePayButton = pidls[0].GetDisplayHintById("googlepayExpressCheckoutFrame") as ExpressCheckoutButtonDisplayHint;
                var applePayButton = pidls[0].GetDisplayHintById("applepayExpressCheckoutFrame") as ExpressCheckoutButtonDisplayHint;

                if ((paymentMethodType?.Equals("*", StringComparison.OrdinalIgnoreCase) ?? false)
                        || (paymentMethodType?.Equals("applepay", StringComparison.OrdinalIgnoreCase) ?? false))
                {
                    Assert.IsNotNull(applePayButton, "Apple pay button should not be null");
                    Assert.AreEqual(expectedValidation, Convert.ToString(applePayButton.Payload["actionType"]), "Action type for quick paymen should be success");
                    Assert.AreEqual(language, Convert.ToString(applePayButton.Payload["language"]), "language is not as expected");

                    Assert.IsNull(paymentMethodType.Equals("applepay", StringComparison.OrdinalIgnoreCase) ? googlePayButton : null, "Google pay button should be null");

                    if (flightName?.Contains("PXUseInlineExpressCheckoutHtml") ?? false)
                    {
                        Assert.IsTrue(applePayButton.SourceUrl.Contains("inline/applepay.html"));
                    }

                    if (flightName?.Contains("PXExpressCheckoutUseIntStaticResources") ?? false)
                    {
                        Assert.IsTrue(applePayButton.SourceUrl.Contains("pmservices.cp.microsoft-int.com"));
                    }
                    else if (flightName?.Contains("PXExpressCheckoutUseProdStaticResources") ?? false)
                    {
                        Assert.IsTrue(applePayButton.SourceUrl.Contains("pmservices.cp.microsoft.com"));
                    }
                }

                if ((paymentMethodType?.Equals("*", StringComparison.OrdinalIgnoreCase) ?? false)
                    || (paymentMethodType?.Equals("googlepay", StringComparison.OrdinalIgnoreCase) ?? false))
                {
                    Assert.IsNotNull(googlePayButton, "Google pay button should not be null");
                    Assert.AreEqual(expectedValidation, Convert.ToString(googlePayButton.Payload["actionType"]), "Action type for quick paymen should be success");
                    Assert.AreEqual(language.Split('-').FirstOrDefault(), Convert.ToString(googlePayButton.Payload["language"]), "language is not as expected");

                    Assert.IsNull(paymentMethodType.Equals("googlepay", StringComparison.OrdinalIgnoreCase) ? applePayButton : null, "Apple pay button should be null");

                    if (flightName?.Contains("PXUseInlineExpressCheckoutHtml") ?? false)
                    {
                        Assert.IsTrue(googlePayButton.SourceUrl.Contains("inline/googlepay.html"));
                    }

                    if (flightName?.Contains("PXExpressCheckoutUseIntStaticResources") ?? false)
                    {
                        Assert.IsTrue(googlePayButton.SourceUrl.Contains("pmservices.cp.microsoft-int.com"));
                    }
                    else if (flightName?.Contains("PXExpressCheckoutUseProdStaticResources") ?? false)
                    {
                        Assert.IsTrue(googlePayButton.SourceUrl.Contains("pmservices.cp.microsoft.com"));
                    }
                }
            }
        }

        public static void ValidatePaymentDescriptions_Select(List<PIDLResource> pidls, int paramsCount, string expectedValidation, string component = null)
        {
            if (component?.Equals("payment") ?? true)
            {
                var selectOption = pidls[0].GetDisplayHintById("paymentMethod") as PropertyDisplayHint;
                Assert.IsTrue(selectOption?.PossibleOptions.ContainsKey(expectedValidation));
                Assert.IsNotNull(pidls[0].PIDLInstanceContexts, "PIDLInstanceContexts should not be null");

                var resourceActionContext = pidls[0].PIDLInstanceContexts[expectedValidation];
                Assert.IsNotNull(resourceActionContext, "ResourceActionContext should not be null");
                Assert.IsNotNull(resourceActionContext.PidlDocInfo, "PidlDocInfo should not be null");
                Assert.AreEqual(resourceActionContext.PidlDocInfo.Parameters.Count, paramsCount);

                var pidlInstance = selectOption.PossibleOptions[expectedValidation].DisplayContent.Members.FirstOrDefault(dh => dh.HintId == (expectedValidation == "list_pi" ? "pidlInstanceListPI" : "pidlInstance" + expectedValidation)) as PidlInstanceDisplayHint;
                Assert.AreEqual(pidlInstance.TriggerSubmitOrder, "beforeBase", "Select option pidl instance should have trigger submit order as before base");

                Assert.AreEqual(pidlInstance.PidlInstance, expectedValidation, "PIDL instance property id and key in pidl resource context should match.");

                var idProperty = pidls[0].DataDescription["id"] as PropertyDescription;
                Assert.IsTrue(idProperty.DefaultValue.Equals(expectedValidation));

                if (expectedValidation.Equals("list_pi", StringComparison.OrdinalIgnoreCase))
                {
                    Assert.AreEqual("<|not|<|stringEqualsIgnoreCase|{id};Saved|>|>", pidlInstance.ConditionalFields["isHidden"]);
                    Assert.AreEqual(resourceActionContext.Action, "selectResource");
                    Assert.IsTrue(idProperty.IsConditionalFieldValue, "Id property should set the IsConditionalFieldValue is true");
                    Assert.AreEqual("<|ternary|<|stringEqualsIgnoreCase|{id};list_pi|>;true;false|>", idProperty?.SideEffects["show_summary"], "Id property should have the side effects to display address summary");
                }
                else
                {
                    Assert.IsNotNull(resourceActionContext.PidlDocInfo.Parameters["family"]);
                    Assert.AreEqual("<|not|<|stringEqualsIgnoreCase|{id};Credit card or debit card|>|>", pidlInstance.ConditionalFields["isHidden"]);
                    Assert.AreEqual(resourceActionContext.Action, "addResource");
                    Assert.IsNull(idProperty?.SideEffects);
                }

                Assert.IsTrue(pidls[0].DataDescription.ContainsKey("show_summary"), "Show summary property is required to show address summary");
                Assert.IsTrue(pidls[0].DisplayPages[0].Members.Any(dh => dh.PropertyName == "show_summary"), "Show summary displayhint is required to show address summary");
            }
        }

        public static void ValidateAddressDescriptions(List<PIDLResource> pidls, string component = null)
        {
            if (component?.Equals("address") ?? true)
            {
                pidls.ForEach(pidl =>
                {
                    var showSummary = pidl.GetPropertyDescriptionByPropertyName("show_summary");
                    Assert.IsTrue(showSummary.IsKey, "Is key is should be true for show_summary property");

                    var billingTitle = pidl.GetDisplayHintById("billingAddressTitle");
                    Assert.IsNotNull(billingTitle, "Address title should not be null");

                    var submitGroup = pidl.GetDisplayHintById("cancelSaveGroup") as GroupDisplayHint;
                    Assert.IsNotNull(submitGroup, "Address PIDL submit group should not be null");
                    Assert.IsTrue(submitGroup.IsSumbitGroup, "Address PIDL should have the submit group");

                    if (pidl.Identity["resource_id"].Equals("billing.form", StringComparison.OrdinalIgnoreCase))
                    {
                        Assert.AreEqual("^false$", showSummary?.Validation?.Regex, "Show summary validation is not expected for address form");
                        Assert.IsTrue(billingTitle.IsHidden, "Billing title should be hidden state for address form");
                    }
                    else
                    {
                        Assert.AreEqual("^true$", showSummary?.Validation?.Regex, "Show summary validation is not expected address summary");
                        Assert.IsNull(billingTitle.IsHidden, "Billing title should not be in hidden state for address summary");
                    }

                    OrderSummaryDescription.OrderSummaryDataDescriptions.ToList().ForEach(dataDescriptions =>
                    {
                        if (pidl.DataDescription.ContainsKey(dataDescriptions) && ComponentDescription.ShouldEnableEmitEventOnPropertyUpdate(dataDescriptions))
                        {
                            var cartProperty = pidl.DataDescription[dataDescriptions] as PropertyDescription;
                            Assert.IsTrue(cartProperty.EmitEventOnPropertyUpdate);
                        }
                    });

                    AddressDescription.AddressDataDescriptions.ToList().ForEach(dataDescriptions =>
                    {
                        if (pidl.DataDescription.ContainsKey(dataDescriptions))
                        {
                            var addresssProperty = pidl.DataDescription[dataDescriptions] as PropertyDescription;
                            Assert.IsNotNull(addresssProperty, "Address property description should not be null");

                            var addressDisplayHint = pidl.GetDisplayHintByPropertyName(dataDescriptions) as PropertyDisplayHint;
                            Assert.IsNotNull(addressDisplayHint, "Address display hint should not be null");

                            if (pidl.Identity["resource_id"].Equals("billing.form", StringComparison.OrdinalIgnoreCase))
                            {
                                Assert.AreEqual(addresssProperty.BroadcastTo, dataDescriptions, "Broadcast to property should be matching with property name for address form");

                                Assert.IsNotNull(addressDisplayHint.Onfocusout, "Address form display hint should have the onfocusout event");
                                Assert.AreEqual(addressDisplayHint.Onfocusout.EventType, "validateOnChange", "Address form display onfocusout event type should be validate on change");

                                Assert.IsNotNull(addressDisplayHint.Onfocusout.Context, "Address form display hint onfocusout event context should not be null");
                                var eventContext = JsonConvert.DeserializeObject<EventContext>(JsonConvert.SerializeObject(addressDisplayHint.Onfocusout.Context));
                                Assert.IsTrue(eventContext.Silent, "Address form display onfocusout event context should be marked as silent when enablePaymentRequestAddressValidation is not enabled");
                            }
                            else
                            {
                                Assert.IsTrue(addresssProperty.UsePreExistingValue, "User pre existing value should be true for address summary property");

                                Assert.IsTrue(addressDisplayHint.IsHidden, "Address summary display hint should be hidden mode");
                            }
                        }
                    });
                });
            }
        }

        public static void ValidatePaymentClientDescriptions(List<PIDLResource> pidls, string expectedIdentity, string component = null)
        {
            if ((component?.Equals("confirm") ?? true)
                || (component?.Equals("profile") ?? false))
            {
                Assert.IsNotNull(pidls, "Pidl is expected to be not null");
                Assert.AreEqual(1, pidls.Count, "PIDL count not expected");
                Assert.AreEqual(expectedIdentity.ToLower(), pidls[0].Identity["description_type"].ToLower(), "PIDL idenity not expected");
            }
        }
    }
}
