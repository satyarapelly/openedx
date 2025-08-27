// <copyright file="HttpRequestHelper.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Web;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Transaction;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Model;
    using Microsoft.Commerce.Payments.PXService.V7.Contexts;
    using Microsoft.Commerce.Tracing;
    using Newtonsoft.Json;
    using UAParser;

    public static class HttpRequestHelper
    {
        private static readonly HashSet<string> PSD2E2ETestScenarioPrefixes = new HashSet<string>()
        {
            "px-service-psd2-e2e-emulator"
        };

        private static readonly HashSet<string> PSD2TestScenarioPrefixes = new HashSet<string>()
        {
            "px-service-psd2-e2e-emulator",
            "px.payerauth.psd2",
            "px.pims.3ds"
        };

        private static readonly HashSet<string> ThreeDSOneTestScenarioPrefixes = new HashSet<string>()
        {
            "px-service-3ds1-e2e-emulator",
            "px-service-3ds1-test-emulator",
            "px-service-3ds1-test-emulator-challenge-success",
            "px-service-3ds1-test-emulator-challenge-failed",
            "px-service-3ds1-test-emulator-challenge-internalServerError",
            "px-service-3ds1-test-emulator-challenge-timeOut"
        };

        public static string GetRequestHeader(string headerName)
        {
            string headerValue = null;
            if (HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.Headers != null)
            {
                headerValue = HttpContext.Current.Request.Headers[headerName];
            }

            return headerValue;
        }

        public static string GetRequestHeader(string headerName, HttpRequestMessage request)
        {
            string headerValue = null;
            if (request != null && request.Headers != null && request.Headers.Contains(headerName))
            {
                headerValue = request.Headers.GetValues(headerName).First();
            }

            return headerValue;
        }

        public static string GetRequestContextItem(string key)
        {
            string itemValue = null;
            if (HttpContext.Current?.Request?.RequestContext?.HttpContext?.Items != null)
            {
                if (HttpContext.Current.Request.RequestContext.HttpContext.Items.Contains(key))
                {
                    itemValue = (string)HttpContext.Current.Request.RequestContext.HttpContext.Items[key];
                }
            }

            return itemValue;
        }

        /// <summary>
        /// Checks if the request is encoded, based on the presence of the ClientContextEncoding (x-ms-clientcontext-encoding) header.
        /// </summary>
        /// <param name="request">http request</param>
        /// <returns>True | False</returns>
        public static bool IsEncoded(HttpRequestMessage request)
        {
            bool isEncoded = false;
            string clientContextEncoding = null;
            if (request?.Headers != null)
            {
                IEnumerable<string> values = new List<string>();
                if (request.Headers.TryGetValues(GlobalConstants.HeaderValues.ClientContextEncoding, out values))
                {
                    clientContextEncoding = values?.FirstOrDefault();
                }
            }

            if (!string.IsNullOrWhiteSpace(clientContextEncoding))
            {
                isEncoded = true;
            }

            return isEncoded;
        }

        /// <summary>
        /// Gets the UserAgent from the request header x-ms-deviceinfo.
        /// </summary>
        /// <param name="request">http request</param>
        /// <returns>Value of userAgent</returns>
        public static string GetUserAgent(HttpRequestMessage request)
        {
            string devicInfoHeaderValue = GetRequestHeader(GlobalConstants.HeaderValues.DeviceInfoHeader, request);
            
            if (string.IsNullOrWhiteSpace(devicInfoHeaderValue))
            {
                return null;
            }
            
            string[] keyValuePairs = devicInfoHeaderValue?.Split(',');
            string userAgentValue = null;

            foreach (string pair in keyValuePairs)
            {
                int indexOfDelim = pair.IndexOf('=');
                if (indexOfDelim > -1)
                {
                    string key = pair.Substring(0, indexOfDelim).Trim();
                    string value = pair.Substring(indexOfDelim + 1);

                    if (key == "userAgent")
                    {
                        userAgentValue = value;
                        break;
                    }
                }
            }

            return userAgentValue;
        }

        /// <summary>
        /// Gets the UserAgent from the request header x-ms-deviceinfo.
        /// </summary>
        /// <param name="request">Current request object</param>
        /// <returns>Client info</returns>
        public static ClientInfo GetClientInfo(HttpRequestMessage request)
        {
            var userAgent = GetUserAgentDecoded(request) ?? request.Headers.UserAgent?.ToString();
            if (!string.IsNullOrWhiteSpace(userAgent))
            {
                var uaParser = Parser.GetDefault();
                return uaParser.Parse(userAgent);
            }

            return null;
        }

        /// <summary>
        /// Gets the device class based on the user agent.
        /// </summary>
        /// <param name="request">Current request object</param>
        /// <returns>Device class - web/mobile/console</returns>
        public static string GetDeviceClass(HttpRequestMessage request)
        {
            var clientInfo = GetClientInfo(request);

            switch (clientInfo?.OS.Family.ToLower())
            {
                case GlobalConstants.OperatingSystem.Android:
                case GlobalConstants.OperatingSystem.IOS:
                    return GlobalConstants.DeviceClass.Mobile;
                case GlobalConstants.OperatingSystem.Windows:
                    return GlobalConstants.DeviceClass.Web;
                default:
                    return GlobalConstants.DeviceClass.Web;
            }
        }

        /// <summary>
        /// Gets the browser name from the user agent.
        /// </summary>
        /// <param name="request">Current request object</param>
        /// <returns>Browser name - safari/chrome/etc</returns>
        public static string GetBrowser(HttpRequestMessage request)
        {
            var clientInfo = GetClientInfo(request);
            string family = clientInfo?.UA.Family.ToLower();

            return family ?? string.Empty;
        }

        /// <summary>
        /// Gets the family from the user agent.
        /// </summary>
        /// <param name="request">Current request object</param>
        /// <returns>device Family</returns>
        public static string GetOSFamily(HttpRequestMessage request)
        {
            var clientInfo = GetClientInfo(request);
            string deviceFamily = clientInfo?.OS.Family.ToLower();

            return deviceFamily ?? string.Empty;
        }

        /// <summary>
        /// Gets the browser version from the user agent.  Formatted in major.minor.patch
        /// </summary>
        /// <param name="request">Request Object</param>
        /// <returns>Browser version</returns>
        public static string GetBrowserVer(HttpRequestMessage request)
        {
            var clientInfo = GetClientInfo(request);
            var majorVersion = clientInfo?.UA.Major ?? "0";
            var minorVersion = clientInfo?.UA.Minor ?? "0";
            var patchVersion = clientInfo?.UA.Patch ?? "0";

            return $"{majorVersion}.{minorVersion}.{patchVersion}";
        }

        /// <summary>
        /// Gets the browser major version from the user agent.  Formatted in major.minor.patch
        /// </summary>
        /// <param name="request">Request Object</param>
        /// <returns>Browser major version</returns>
        public static int GetBrowserMajorVer(HttpRequestMessage request)
        {
            var clientInfo = GetClientInfo(request);
            var majorVersion = clientInfo?.UA.Major ?? "0";

            if (int.TryParse(majorVersion, out int majorVersionInt))
            {
                return majorVersionInt;
            }
            else
            {
                // Handle the case where majorVersion is not a valid integer
                return 0; // Or any default/fallback integer value
            }
        }

        /// <summary>
        /// Returns the userAgent value, decoded if it is encoded.
        /// </summary>
        /// <param name="request">http request</param>
        /// <returns>Value of userAgent</returns>
        public static string GetUserAgentDecoded(HttpRequestMessage request)
        {
            string userAgent = GetUserAgent(request);
            if (string.IsNullOrWhiteSpace(userAgent))
            {
                return null;
            }

            if (IsEncoded(request))
            {
                string retVal = string.Empty;
                try
                {
                    var decodedData = Convert.FromBase64String(userAgent);
                    retVal = System.Text.Encoding.UTF8.GetString(decodedData);
                }
                catch
                {
                }

                return retVal;
            }

            return userAgent;
        }

        /// <summary>
        /// Gets the PidlSDK version from the request headers
        /// </summary>
        /// <param name="request">http request</param>
        /// <returns>PidlSDK Version</returns>
        public static Version GetFullPidlSdkVersion(HttpRequestMessage request)
        {
            IEnumerable<string> xboxNativePidlsdkVersions;
            request.Headers.TryGetValues(GlobalConstants.HeaderValues.PidlSdkVersion, out xboxNativePidlsdkVersions);            

            if (xboxNativePidlsdkVersions != null)
            {
                List<string> pidlsdkVersionDetails = xboxNativePidlsdkVersions.FirstOrDefault().Split('_').ToList();
                Version xboxNativePidlSdkVersionConcat = new Version();

                if (pidlsdkVersionDetails.Count > 0)
                {
                    if (pidlsdkVersionDetails[0].Contains(V7.Constants.Versions.Alpha))
                    {
                        List<string> pidlsdkAlphaVersionExists = pidlsdkVersionDetails[0].Split('-').ToList();
                        List<string> pidlsdkAlphaVersionDetails = pidlsdkAlphaVersionExists.Last().Split('.').ToList();
                        var version = Version.TryParse(pidlsdkAlphaVersionExists[0].ToString() + "." + pidlsdkAlphaVersionDetails[1].ToString(), out xboxNativePidlSdkVersionConcat);
                        return xboxNativePidlSdkVersionConcat;
                    }
                    else
                    {
                        var version = Version.TryParse(pidlsdkVersionDetails[0] + ".0", out xboxNativePidlSdkVersionConcat);
                        return xboxNativePidlSdkVersionConcat;
                    }
                }
            }

            return null;
        }

        public static void AddRequestContextItem(string key, string value)
        {
            if (HttpContext.Current?.Request?.RequestContext?.HttpContext?.Items != null)
            {
                HttpContext.Current.Request.RequestContext.HttpContext.Items[key] = value;
            }
        }

        public static TestContext GetTestHeader(HttpRequestMessage incomingRequest = null)
        {
            TestContext testContext = null;
            string value = GetRequestHeader(PaymentConstants.PaymentExtendedHttpHeaders.TestHeader);
            if (value != null)
            {
                testContext = JsonConvert.DeserializeObject<TestContext>(value);
            }
            else if (incomingRequest != null)
            {
                try
                {
                    string testHeaderString = incomingRequest.GetRequestHeader(PaymentConstants.PaymentExtendedHttpHeaders.TestHeader);
                    if (testHeaderString != null)
                    {
                        testContext = JsonConvert.DeserializeObject<TestContext>(testHeaderString);
                    }
                }
                catch
                {
                    // We are eating up the exception when deserializing fails
                }
            }

            return testContext;
        }

        public static void SetTestHeader(string value)
        {
            if (HttpContext.Current?.Request?.Headers != null)
            {
                HttpContext.Current.Request.Headers.Set(PaymentConstants.PaymentExtendedHttpHeaders.TestHeader, value);
            }
        }

        public static void TransferTargetHeadersFromIncomingRequestToOutgoingRequest(List<string> targetHeaders, HttpRequestMessage outgoingRequest)
        {
            foreach (string header in targetHeaders)
            {
                string value = HttpRequestHelper.GetRequestHeader(header);

                // For the selfhosted env, HttpContext is not available, so we are also checking the SelfhostHttpContext to get the test/scenario headers
                if (value == null && WebHostingUtility.IsApplicationSelfHosted() && SelfhostHttpContext.Request != null)
                {
                    value = GetRequestHeader(header, SelfhostHttpContext.Request);
                }

                if (value != null)
                {
                    outgoingRequest.Headers.Add(header, value);
                }
            }
        }

        public static bool IsPXTestRequest(string testHeaderPrefix = "px.")
        {
            try
            {
                TestContext testContext = GetTestHeader();
                if (testContext != null)
                {
                    if (testContext != null && !string.IsNullOrEmpty(testContext.Scenarios))
                    {
                        string[] scenariosArray = testContext.Scenarios.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < scenariosArray.Length; ++i)
                        {
                            string scenario = scenariosArray[i].Trim();
                            if (scenario.StartsWith(testHeaderPrefix, StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException("HttpRequestHelper.PXTestScenarioEnabled: " + ex.ToString(), EventTraceActivity.Empty);
            }

            return false;
        }

        public static bool HasAnyPSD2TestScenarios(TestContext testContext)
        {
            return Has3dsTestScenarios(testContext, PSD2TestScenarioPrefixes);
        }

        public static bool HasE2EPSD2TestScenarios(TestContext testContext)
        {
            return Has3dsTestScenarios(testContext, PSD2E2ETestScenarioPrefixes);
        }

        public static bool HasAnyThreeDSOneTestScenarios(TestContext testContext)
        {
            return Has3dsTestScenarios(testContext, ThreeDSOneTestScenarioPrefixes);
        }

        public static bool HasThreeDSOneTestScenario(TestContext testContext)
        {
            return Has3dsTestScenario(testContext, "px-service-3ds1-test-emulator");
        }

        public static bool HasAuthenticateThreeDSUserErrorTestScenario(TestContext testContext)
        {
            return Has3dsTestScenario(testContext, "px-service-Auth3ds-test-emulator-user-error");
        }

        public static bool HasThreeDSOneTestScenarioWithSuccess(TestContext testContext)
        {
            return Has3dsTestScenario(testContext, "px-service-3ds1-test-emulator-challenge-success");
        }

        public static bool HasThreeDSOneTestScenarioWithFailure(TestContext testContext)
        {
            return Has3dsTestScenario(testContext, "px-service-3ds1-test-emulator-challenge-failed");
        }

        public static bool HasThreeDSOneTestScenarioWithInternalServerError(TestContext testContext)
        {
            return Has3dsTestScenario(testContext, "px-service-3ds1-test-emulator-challenge-internalServerError");
        }

        public static bool HasThreeDSOneTestScenarioWithTimeOut(TestContext testContext)
        {
            return Has3dsTestScenario(testContext, "px-service-3ds1-test-emulator-challenge-timeOut");
        }

        // Todo: Remove the following code once payerAuth support correct isFullPageRedirect value
        public static bool HasThreeDSOneTestScenarioIframeOverriding(TestContext testContext)
        {
            return Has3dsTestScenario(testContext, "px-service-3ds1-show-iframe");
        }

        public static RequestContext GetRequestContext(HttpRequestMessage request, EventTraceActivity traceActivityId)
        {
            RequestContext requestContext = null;
            var requestContextHeader = GetRequestHeader(GlobalConstants.HeaderValues.XMsRequestContext, request) ?? GetRequestHeader(GlobalConstants.HeaderValues.RequestContext, request);
            if (requestContextHeader != null)
            {
                try
                {
                    // Deserialize the "request-context" header value into a RequestContext object
                    requestContext = JsonConvert.DeserializeObject<RequestContext>(requestContextHeader);
                }
                catch (Exception ex)
                {
                    SllWebLogger.TracePXServiceException("HttpRequestHelper.GetRequestContext: " + ex.ToString(), traceActivityId);
                }
            }

            return requestContext;
        }

        private static bool Has3dsTestScenario(TestContext testContext, string scenarioPrefix)
        {
            try
            {
                if (testContext != null)
                {
                    if (testContext != null && !string.IsNullOrEmpty(testContext.Scenarios))
                    {
                        string[] scenariosArray = testContext.Scenarios.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        if (scenariosArray.Where((value) => string.Equals(value, scenarioPrefix)).Any())
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException("HttpRequestHelper.hasTestScenario: " + ex.ToString(), EventTraceActivity.Empty);
            }

            return false;
        }

        private static bool Has3dsTestScenarios(TestContext testContext, HashSet<string> scenarioPrefixes)
        {
            try
            {
                if (testContext != null)
                {
                    if (testContext != null && !string.IsNullOrEmpty(testContext.Scenarios))
                    {
                        string[] scenariosArray = testContext.Scenarios.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < scenariosArray.Length; ++i)
                        {
                            string scenario = scenariosArray[i].Trim();
                            if (scenarioPrefixes.Where((value) => scenario.StartsWith(value, StringComparison.OrdinalIgnoreCase)).Any())
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException("HttpRequestHelper.hasTestScenario: " + ex.ToString(), EventTraceActivity.Empty);
            }

            return false;
        }
    }
}