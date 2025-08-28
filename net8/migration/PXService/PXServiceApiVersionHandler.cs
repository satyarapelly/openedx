// <copyright file="PXServiceApiVersionHandler.cs" company="Microsoft Corporation">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using Azure.Core;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Model;
    using Microsoft.Commerce.Payments.PXService.Settings;
    using Microsoft.Commerce.Payments.PXService.Settings.FeatureConfig;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Middleware which validates that an appropriate api-version is
    /// passed with the request.
    /// </summary>
    public class PXServiceApiVersionHandler
    {
        private const string OperationVersionHeader = "x-ms-operation-version";
        private const string ExposableFlightsHeader = "x-ms-flight";
        private const string ContentTypeOptionsHeader = "X-Content-Type-Options";
        private const string RetryOnServerErrorHeader = "x-ms-px-retry-servererr";
        private const string PXFlightAssignmentContextHeader = "x-ms-px-flight-assignmentcontext";
        private const string NoSniff = "nosniff";

        private readonly HashSet<string> versionlessControllers;
        private readonly IReadOnlyDictionary<string, ApiVersion> supportedVersions;
        private readonly PXServiceSettings settings;
        private readonly RequestDelegate next;

        /// <summary>
        /// Initializes a new instance of the <see cref="PXServiceApiVersionHandler"/> class.
        /// </summary>
        /// <param name="supportedVersions">Mapping from api-version string to internal version numbers.</param>
        /// <param name="versionlessControllers">Names of controllers that should be available with no version.</param>
        /// <param name="settings">PXServiceSettings instance for the current service.</param>
        public PXServiceApiVersionHandler(IReadOnlyDictionary<string, ApiVersion> supportedVersions, IEnumerable<string> versionlessControllers, PXServiceSettings settings)
            : this(null, supportedVersions, versionlessControllers, settings)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PXServiceApiVersionHandler"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="supportedVersions">Mapping from api-version string to internal version numbers.</param>
        /// <param name="versionlessControllers">Names of controllers that should be available with no version.</param>
        /// <param name="settings">PXServiceSettings instance for the current service.</param>
        public PXServiceApiVersionHandler(RequestDelegate next, IReadOnlyDictionary<string, ApiVersion> supportedVersions, IEnumerable<string> versionlessControllers, PXServiceSettings settings)
        {
            this.next = next;
            this.supportedVersions = supportedVersions;
            this.versionlessControllers = versionlessControllers != null ? new HashSet<string>(versionlessControllers, StringComparer.OrdinalIgnoreCase) : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            this.settings = settings;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var endpoint = httpContext.GetEndpoint();
            var cad = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
            var controllerName = cad?.ControllerName; // short name (no "Controller")

            // Fallback: from route values (works for conventional routes)
            controllerName ??= httpContext.Request.RouteValues.TryGetValue("controller", out var c)
                ? Convert.ToString(c)
                : null;

            if (controllerName != null && this.versionlessControllers.Contains(controllerName))
            {
                await this.next(httpContext);
                return;
            }

            var responseMessage = await this.SendAsync(httpContext, httpContext.RequestAborted);

            if (!httpContext.Response.HasStarted)
            {
                httpContext.Response.StatusCode = (int)responseMessage.StatusCode;
                foreach (var header in responseMessage.Headers)
                {
                    httpContext.Response.Headers[header.Key] = header.Value.ToArray();
                }

                if (responseMessage.Content != null)
                {
                    foreach (var header in responseMessage.Content.Headers)
                    {
                        httpContext.Response.Headers[header.Key] = header.Value.ToArray();
                    }

                    await responseMessage.Content.CopyToAsync(httpContext.Response.Body);
                }
            }
        }

        /// <summary>
        /// Extracts the api-version from the request and validates it. If validation
        /// fails, a response will be returned immediately.
        /// </summary>
        /// <param name="request">The inbound request.</param>
        /// <param name="cancellationToken">A token which may be used to listen
        /// for cancellation.</param>
        /// <returns>The outbound response.</returns>
        private async Task<HttpResponseMessage> SendAsync(HttpContext httpContext, CancellationToken cancellationToken)
        {
            HttpRequestMessage request = httpContext.Request.ToHttpRequestMessage();
            httpContext.Request.EnableBuffering();
            httpContext.Request.Body.Position = 0;
            request.Content = new StreamContent(httpContext.Request.Body);
            request.Options.Set(new HttpRequestOptionsKey<HttpContext>("HttpContext"), httpContext);

            void SetProperty(string key, object? value)
            {
                request.SetProperty(key, value);
                httpContext.Request.SetProperty(key, value);
            }

            // Extract version and accountId from the RequestUri.
            // As can be seen from sample PX RequestUris below, version ("v7.0" in the example below) is part of the url.  Also,
            // if the commerce account id exists ("f2ac3e1d-e724-4820-baa0-0098584c6dcc" in the example below), it appears in the 
            // segment immediately after the version
            // https://paymentexperience.cp.microsoft.com/px/v7.0/transformation 
            // https://paymentexperience.cp.microsoft.com/px/v7.0/settings/Microsoft.MicrosoftWallet/16336.2.2.0
            // https://paymentexperience.cp.microsoft.com/px/v7.0/f2ac3e1d-e724-4820-baa0-0098584c6dcc/paymentInstrumentsEx?country=us&language=en-US&partner=xbox
            string accountId = string.Empty;
            string accountIdPattern = @"^[a-f0-9-]{30,40}/$";
            Regex accountIdRegex = new Regex(accountIdPattern, RegexOptions.IgnoreCase);

            string externalVersion = string.Empty;
            string versionPattern = @"^v\d+\.\d+/$";
            Regex versionRegex = new Regex(versionPattern, RegexOptions.IgnoreCase);
            bool versionFound = false;
            foreach (string seg in request.RequestUri.Segments)
            {
                try
                {
                    if (versionFound)
                    {
                        MatchCollection accountIdMatches = accountIdRegex.Matches(seg);
                        if (accountIdMatches.Count == 1)
                        {
                            accountId = accountIdMatches[0].Value.TrimEnd(new char[] { '/' });
                        }

                        break;
                    }

                    MatchCollection matches = versionRegex.Matches(seg);
                    if (matches.Count == 1)
                    {
                        string versionInUrl = matches[0].Value.Substring(0, matches[0].Value.Length - 1);
                        decimal.Parse(versionInUrl.Substring(1));
                        externalVersion = versionInUrl;
                        versionFound = true;
                    }
                }
                catch
                {
                }
            }

            string ipAddress = GetUserIpAddress(request);
            string country = null;
            request.TryGetQueryParameterValue("country", out country, "paymentSession(Or)?Data");
            string partner = null;
            request.TryGetQueryParameterValue("partner", out partner, "paymentSession(Or)?Data");
            string language = null;
            request.TryGetQueryParameterValue("language", out language, "paymentSession(Or)?Data");
            string family = null;
            request.TryGetQueryParameterValue("family", out family, "paymentSession(Or)?Data");
            string operation = null;
            request.TryGetQueryParameterValue("operation", out operation, "paymentSession(Or)?Data");
            string component = null;
            request.TryGetQueryParameterValue("component", out component, "paymentSession(Or)?Data");

            string pidlsdkVersion = null;

            string operatingSystem = null;
            string operatingSystemVer = null;
            string browser = null;
            string browserVer = null;
            string pidlSdkVer = null;
            string referrerDomain = null;

            ////Extract the partner, country and language from the payload if the request is a POST
            ////And the URI is createAndAuthenticate or PostPaymentSession

            try
            {
                if (request.Method == HttpMethod.Post &&
                    request.RequestUri.AbsolutePath.Contains("/paymentSessions/createAndAuthenticate"))
                {
                    partner = partner ?? await request.TryGetPayloadPropertyValue("paymentSessionData.partner");
                    country = country ?? await request.TryGetPayloadPropertyValue("paymentSessionData.country");
                    language = language ?? await request.TryGetPayloadPropertyValue("paymentSessionData.language");
                    family = family ?? await request.TryGetPayloadPropertyValue("paymentSessionData.family");
                    operation = operation ?? await request.TryGetPayloadPropertyValue("paymentSessionData.operation");
                }
                else if (request.Method == HttpMethod.Post &&
                    request.RequestUri.AbsolutePath.Contains("/paymentSessions") &&
                    !request.RequestUri.AbsolutePath.Contains("notifyThreeDSChallengeCompleted"))
                {
                    partner = partner ?? await request.TryGetPayloadPropertyValue("partner");
                    country = country ?? await request.TryGetPayloadPropertyValue("country");
                    language = language ?? await request.TryGetPayloadPropertyValue("language");
                    family = family ?? await request.TryGetPayloadPropertyValue("family");
                    operation = operation ?? await request.TryGetPayloadPropertyValue("operation");
                }

                IEnumerable<string> pidlsdkVersions;
                if (request.Headers.TryGetValues(GlobalConstants.HeaderValues.PidlSdkVersion, out pidlsdkVersions))
                {
                    pidlsdkVersion = pidlsdkVersions.FirstOrDefault();
                }

                // Get the pidl sdk version from the request headers. This one is the version (major.minor.build) which can be used to check
                // greater than or equal to a specific version etc. instead of just being string comparison like pidlSdkVersion. Didn't change the existing
                // one to make sure we don't break any existing flights setup using pidlSdkVersion.
                var version = HttpRequestHelper.GetFullPidlSdkVersion(request);
                pidlSdkVer = version?.ToString(3);

                UAParser.ClientInfo clientInfo = HttpRequestHelper.GetClientInfo(request);
                if (clientInfo != null)
                {
                    browser = HttpRequestHelper.GetBrowser(request); // Browser family like Edge, Chrome, Firefox, etc.
                    browserVer = HttpRequestHelper.GetBrowserVer(request);
                    Version browserVersion;
                    if (!Version.TryParse(browserVer, out browserVersion))
                    {
                        browserVer = null;
                    }

                    operatingSystem = HttpRequestHelper.GetOSFamily(request); // OS family like Windows, Mac, Linux, etc.

                    operatingSystemVer = clientInfo.OS.Major + "." + (clientInfo.OS.Minor ?? "0") + "." + (clientInfo.OS.Patch ?? "0");
                    Version operatingSystemVersion;
                    if (!Version.TryParse(operatingSystemVer, out operatingSystemVersion))
                    {
                        operatingSystemVer = null;
                    }
                }

                // For ABK challenge flow in integration environment, set the test header to use emulator for 3DS2 challenge flow.
                try
                {
                    if (string.Equals(partner, V7.Constants.PartnerName.BattleNet, StringComparison.OrdinalIgnoreCase) && string.Equals(operation, V7.Constants.Operations.Add, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(component, V7.Constants.Component.Confirm, StringComparison.OrdinalIgnoreCase)
                        && Common.Environments.Environment.Current.EnvironmentType == Common.Environments.EnvironmentType.Integration)
                    {
                        HttpRequestHelper.SetTestHeader("{\"scenarios\":\"px-service-psd2-e2e-emulator\",\"contact\":\"pidlsdk\"}");
                    }
                }
                catch
                {
                }

                IEnumerable<string> riskInfo;

                if (request.Headers.TryGetValues(GlobalConstants.HeaderValues.RiskInfoHeader, out riskInfo))
                {
                    referrerDomain = null;
                    var riskInfoList = riskInfo?.FirstOrDefault()?.Split(',');
                    if (riskInfoList != null && riskInfoList.Length > 0)
                    {
                        foreach (string riskData in riskInfoList)
                        {
                            if (!string.IsNullOrEmpty(riskData) && riskData.StartsWith("referrer=", StringComparison.OrdinalIgnoreCase))
                            {
                                IEnumerable<string> values = null;
                                string clientContextEncoding;
                                request?.Headers.TryGetValues(GlobalConstants.HeaderValues.ClientContextEncoding, out values);
                                clientContextEncoding = values?.FirstOrDefault();

                                string referrerUrl = string.Empty;

                                if (!string.IsNullOrEmpty(clientContextEncoding)
                                    && string.Equals(clientContextEncoding, "base64", StringComparison.OrdinalIgnoreCase))
                                {
                                    var encodedReferrer = riskData.Replace("referrer=", string.Empty);
                                    var base64EncodedBytes = Convert.FromBase64String(encodedReferrer);
                                    referrerUrl = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
                                }
                                else
                                {
                                    referrerUrl = riskData.Replace("referrer=", string.Empty);
                                }

                                Uri referrerUri;
                                if (Uri.TryCreate(referrerUrl, UriKind.RelativeOrAbsolute, out referrerUri))
                                {
                                    referrerDomain = referrerUri?.Host?.Trim()?.ToLower();
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            var traceActivityId = request.GetRequestCorrelationId();

            // Get the partner name from request context tenant id
            var requestContext = HttpRequestHelper.GetRequestContext(request, traceActivityId);
            if (requestContext?.TenantId != null && string.IsNullOrEmpty(partner))
            {
                // Get the partner name from tenant id, this is used to get the partner settings for the tenant id
                if (!PartnerSettingsHelper.TenantIdPartnerNameMapper.TryGetValue(requestContext.TenantId, out partner))
                {
                    partner = requestContext.TenantId;
                    accountId = requestContext.PaymentAccountId;
                }
            }

            // flightContext and request.Properties[GlobalConstants.RequestPropertyKeys.FlightContext] 
            // are only used in CIT to verify the country, partner and language are successfully set.
            Dictionary<string, string> flightContext = new Dictionary<string, string>();

            // only following 3 are used by local configuration
            flightContext.Add(Flighting.ContextKeys.AccountId, accountId);
            flightContext.Add(Flighting.ContextKeys.Country, country);
            flightContext.Add(Flighting.ContextKeys.Partner, partner);
            flightContext.Add(Flighting.ContextKeys.BaseMsCV, request.GetBaseCorrelationVector());

            // the following context keys can be used by remote flighting (Azure ExP)
            flightContext.Add(Flighting.ContextKeys.IpAddress, ipAddress);
            flightContext.Add(Flighting.ContextKeys.Language, language);
            flightContext.Add(Flighting.ContextKeys.Family, family);
            flightContext.Add(Flighting.ContextKeys.Operation, operation);
            flightContext.Add(Flighting.ContextKeys.PidlSdkVersion, pidlsdkVersion);
            flightContext.Add(Flighting.ContextKeys.EnvType, Common.Environments.Environment.Current.EnvironmentType.ToString());
            flightContext.Add(Flighting.ContextKeys.EnvName, Common.Environments.Environment.Current.EnvironmentName);
            flightContext.Add(Flighting.ContextKeys.PidlSdkVer, pidlSdkVer);
            flightContext.Add(Flighting.ContextKeys.OperatingSystem, operatingSystem);
            flightContext.Add(Flighting.ContextKeys.OperatingSystemVer, operatingSystemVer);
            flightContext.Add(Flighting.ContextKeys.Browser, browser);
            flightContext.Add(Flighting.ContextKeys.BrowserVer, browserVer);
            flightContext.Add(Flighting.ContextKeys.ReferrerDomain, referrerDomain);

            SetProperty(GlobalConstants.RequestPropertyKeys.FlightContext, flightContext);

            // Get feature config from AzureExp
            // TODO: if AzureExp is supporeted and needed in Sovereign clouds, then we shall enable AzureExp
            Flighting.FeatureConfig featureConfig = settings.AzureExpEnabled ? await settings.AzureExPAccessor.GetExposableFeatures(flightContext, traceActivityId) : null;
            List<string> exposableFeatures = featureConfig?.EnabledFeatures ?? new List<string>();

            exposableFeatures = LocalFeatureConfigs.MergeMatchedEligibleLocalAndRemoteFeatures(
                accountId: accountId,
                partner: partner,
                country: country,
                localFeatureConfigs: settings.LocalFeatureConfigs,
                remoteFeatures: exposableFeatures,
                traceActivityId);

            string originCountryFlight = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.OriginCountryPrefix);
            if (!string.IsNullOrEmpty(originCountryFlight))
            {
                exposableFeatures.Add(originCountryFlight);
                RemovePartnerFlight(request, originCountryFlight);
            }

            string originCountry = GetOriginCountry(originCountryFlight);
            if (!string.IsNullOrEmpty(originCountry) && Constants.MarketGroups.CommercialSMDEnabledMarkets.Contains(originCountry, StringComparer.OrdinalIgnoreCase))
            {
                exposableFeatures.Add(Flighting.Features.PXEnableSmdCommercial);
            }

            string partnerSettingsVersionPartnerFlight = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PartnerSettingsVersionPrefix);
            string partnerSettingsVersion = null;
            if (!string.IsNullOrEmpty(partnerSettingsVersionPartnerFlight))
            {
                partnerSettingsVersion = GetPartnerSettingsVersion(partnerSettingsVersionPartnerFlight);
                exposableFeatures.Add(partnerSettingsVersionPartnerFlight);
            }

            string xboxOOBEFlight = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.XboxOOBE);
            if (!string.IsNullOrEmpty(xboxOOBEFlight))
            {
                exposableFeatures.Add(xboxOOBEFlight);
                RemovePartnerFlight(request, originCountryFlight);
            }

            string pxServicePSSPPEEnvironmentFlight = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXServicePSSPPEEnvironment);
            if (!string.IsNullOrEmpty(pxServicePSSPPEEnvironmentFlight))
            {
                exposableFeatures.Add(pxServicePSSPPEEnvironmentFlight);
                RemovePartnerFlight(request, pxServicePSSPPEEnvironmentFlight);
            }

            string pxEnableXboxAccessibilityHint = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXEnableXboxAccessibilityHint);
            if (!string.IsNullOrEmpty(pxEnableXboxAccessibilityHint))
            {
                exposableFeatures.Add(pxEnableXboxAccessibilityHint);
                RemovePartnerFlight(request, pxEnableXboxAccessibilityHint);
            }

            string pxEnableXboxNewAddressSequenceFrNl = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXEnableXboxNewAddressSequenceFrNl);
            if (!string.IsNullOrEmpty(pxEnableXboxNewAddressSequenceFrNl))
            {
                exposableFeatures.Add(pxEnableXboxNewAddressSequenceFrNl);
                RemovePartnerFlight(request, pxEnableXboxNewAddressSequenceFrNl);
            }

            string applyAccentBorderWithGutterOnFocus = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.ApplyAccentBorderWithGutterOnFocus);
            if (!string.IsNullOrEmpty(applyAccentBorderWithGutterOnFocus))
            {
                exposableFeatures.Add(applyAccentBorderWithGutterOnFocus);
                RemovePartnerFlight(request, applyAccentBorderWithGutterOnFocus);
            }

            string hipCaptchaFlight = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXEnableHIPCaptcha);
            if (!string.IsNullOrEmpty(hipCaptchaFlight))
            {
                exposableFeatures.Add(Flighting.Features.PXEnableHIPCaptcha);
            }

            string hipCaptchaGroupFlight = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXEnableHIPCaptchaGroup);
            if (!string.IsNullOrEmpty(hipCaptchaGroupFlight))
            {
                exposableFeatures.Add(Flighting.Features.PXEnableHIPCaptchaGroup);
            }

            string pxXboxCardApplicationEnableShortUrl = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXXboxCardApplicationEnableShortUrl);
            if (!string.IsNullOrEmpty(pxXboxCardApplicationEnableShortUrl))
            {
                exposableFeatures.Add(Flighting.Features.PXXboxCardApplicationEnableShortUrl);
                RemovePartnerFlight(request, pxXboxCardApplicationEnableShortUrl);
            }

            string pxXboxCardApplicationEnableShortUrlText = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXXboxCardApplicationEnableShortUrlText);
            if (!string.IsNullOrEmpty(pxXboxCardApplicationEnableShortUrlText))
            {
                exposableFeatures.Add(Flighting.Features.PXXboxCardApplicationEnableShortUrlText);
                RemovePartnerFlight(request, pxXboxCardApplicationEnableShortUrlText);
            }

            string pxEnableShortUrlPayPalText = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXEnableShortUrlPayPalText);
            if (!string.IsNullOrEmpty(pxEnableShortUrlPayPalText))
            {
                exposableFeatures.Add(Flighting.Features.PXEnableShortUrlPayPalText);
                RemovePartnerFlight(request, pxEnableShortUrlPayPalText);
            }

            string pxEnableRedeemGift = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXEnableRedeemCSVFlow);
            if (!string.IsNullOrEmpty(pxEnableRedeemGift))
            {
                exposableFeatures.Add(Flighting.Features.PXEnableRedeemCSVFlow);
                RemovePartnerFlight(request, pxEnableRedeemGift);
            }

            string pxUsePostProcessingFeatureForRemovePI = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXUsePostProcessingFeatureForRemovePI);
            if (!string.IsNullOrEmpty(pxUsePostProcessingFeatureForRemovePI))
            {
                exposableFeatures.Add(Flighting.Features.PXUsePostProcessingFeatureForRemovePI);
                RemovePartnerFlight(request, pxUsePostProcessingFeatureForRemovePI);
            }

            string pxDisableRedeemGift = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXDisableRedeemCSVFlow);
            if (!string.IsNullOrEmpty(pxDisableRedeemGift))
            {
                exposableFeatures.Add(Flighting.Features.PXDisableRedeemCSVFlow);
                RemovePartnerFlight(request, pxDisableRedeemGift);
            }

            string pxEnableXboxNativeStyleHints = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXEnableXboxNativeStyleHints);
            if (!string.IsNullOrEmpty(pxEnableXboxNativeStyleHints))
            {
                exposableFeatures.Add(Flighting.Features.PXEnableXboxNativeStyleHints);
                RemovePartnerFlight(request, pxEnableXboxNativeStyleHints);
            }

            string pxUseFontIcons = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXUseFontIcons);
            if (!string.IsNullOrEmpty(pxUseFontIcons))
            {
                exposableFeatures.Add(Flighting.Features.PXUseFontIcons);
                RemovePartnerFlight(request, pxUseFontIcons);
            }

            string pxEnableShortUrlPayPal = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXEnableShortUrlPayPal);
            if (!string.IsNullOrEmpty(pxEnableShortUrlPayPal))
            {
                exposableFeatures.Add(Flighting.Features.PXEnableShortUrlPayPal);
                RemovePartnerFlight(request, pxEnableShortUrlPayPal);
            }

            string pxEnableShortUrlVenmoText = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXEnableShortUrlVenmoText);
            if (!string.IsNullOrEmpty(pxEnableShortUrlVenmoText))
            {
                exposableFeatures.Add(Flighting.Features.PXEnableShortUrlVenmoText);
                RemovePartnerFlight(request, pxEnableShortUrlVenmoText);
            }

            string pxEnableShortUrlVenmo = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXEnableShortUrlVenmo);
            if (!string.IsNullOrEmpty(pxEnableShortUrlVenmo))
            {
                exposableFeatures.Add(Flighting.Features.PXEnableShortUrlVenmo);
                RemovePartnerFlight(request, pxEnableShortUrlVenmo);
            }

            string pxEnableUpdateCCLogo = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXEnableUpdateCCLogo);
            if (!string.IsNullOrEmpty(pxEnableUpdateCCLogo))
            {
                exposableFeatures.Add(Flighting.Features.PXEnableUpdateCCLogo);
                RemovePartnerFlight(request, pxEnableUpdateCCLogo);
            }

            string pxEnableXboxCardUpsellPaymentOptions = GetPartnerFlight(request, Flighting.Features.PXEnableXboxCardUpsellPaymentOptions);
            if (!string.IsNullOrEmpty(pxEnableXboxCardUpsellPaymentOptions))
            {
                exposableFeatures.Add(Flighting.Features.PXEnableXboxCardUpsellPaymentOptions);
                RemovePartnerFlight(request, Flighting.Features.PXEnableXboxCardUpsellPaymentOptions);
            }

            string pxXboxCardApplyEnableFeedbackButton = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXXboxCardApplyEnableFeedbackButton);
            if (!string.IsNullOrEmpty(pxXboxCardApplyEnableFeedbackButton))
            {
                exposableFeatures.Add(V7.Constants.PartnerFlightValues.PXXboxCardApplyEnableFeedbackButton);
                RemovePartnerFlight(request, pxXboxCardApplyEnableFeedbackButton);
            }

            string pxXboxCardApplyDisableStoreButtonNavigation = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXXboxCardApplyDisableStoreButtonNavigation);
            if (!string.IsNullOrEmpty(pxXboxCardApplyDisableStoreButtonNavigation))
            {
                exposableFeatures.Add(V7.Constants.PartnerFlightValues.PXXboxCardApplyDisableStoreButtonNavigation);
                RemovePartnerFlight(request, pxXboxCardApplyDisableStoreButtonNavigation);
            }

            string pxEnableXboxCardUpsell = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXEnableXboxCardUpsell);
            if (!string.IsNullOrEmpty(pxEnableXboxCardUpsell))
            {
                exposableFeatures.Add(Flighting.Features.PXEnableXboxCardUpsell);
                RemovePartnerFlight(request, pxEnableXboxCardUpsell);
            }

            // Pass EnableThreeDS
            // Remove it from x-ms-flight to prevent passing to downstream service
            string enableThreeDSOne = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.EnableThreeDSOne);
            if (!string.IsNullOrEmpty(enableThreeDSOne))
            {
                exposableFeatures.Add(Flighting.Features.PXEnableIndia3DS1Challenge);
                RemovePartnerFlight(request, enableThreeDSOne);
            }

            // Remove it from x-ms-flight to prevent passing to downstream service
            HandleSecureFieldFlights(exposableFeatures, request);

            HandleFlight(exposableFeatures, request, V7.Constants.PartnerFlightValues.PXEnableUpdateDiscoverCreditCardRegex);
            HandleFlight(exposableFeatures, request, V7.Constants.PartnerFlightValues.PXEnableUpdateVisaCreditCardRegex);

            string useJarvisV3ForCompletePrerequisites = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXUseJarvisV3ForCompletePrerequisites);
            if (!string.IsNullOrEmpty(useJarvisV3ForCompletePrerequisites))
            {
                exposableFeatures.Add(Flighting.Features.PXUseJarvisV3ForCompletePrerequisites);
                RemovePartnerFlight(request, useJarvisV3ForCompletePrerequisites);
            }

            // Pass Enable IgnoreTerminatingErrorHandling while adding secondary client action
            string enableIgnoreTerminatingErrorHandling = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXAddSecondaryClientActionWithIgnoreTerminatingErrorHandling);
            if (!string.IsNullOrEmpty(enableIgnoreTerminatingErrorHandling))
            {
                exposableFeatures.Add(Flighting.Features.PXAddSecondaryClientActionWithIgnoreTerminatingErrorHandling);
                RemovePartnerFlight(request, enableIgnoreTerminatingErrorHandling);
            }

            string indiaExpiryGroupDelete = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.IndiaExpiryGroupDelete);
            if (!string.IsNullOrEmpty(indiaExpiryGroupDelete))
            {
                exposableFeatures.Add(Flighting.Features.IndiaExpiryGroupDelete);
                RemovePartnerFlight(request, indiaExpiryGroupDelete);
            }

            string includePIDLWithPaymentInstrumentList = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.IncludePIDLWithPaymentInstrumentList);
            if (!string.IsNullOrEmpty(includePIDLWithPaymentInstrumentList))
            {
                exposableFeatures.Add(Flighting.Features.IncludePIDLWithPaymentInstrumentList);
                RemovePartnerFlight(request, includePIDLWithPaymentInstrumentList);
            }

            string preventAddNewPaymentMethodDefaultSelection = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXPreventAddNewPaymentMethodDefaultSelection);
            if (!string.IsNullOrEmpty(preventAddNewPaymentMethodDefaultSelection))
            {
                exposableFeatures.Add(Flighting.Features.PXPreventAddNewPaymentMethodDefaultSelection);
                RemovePartnerFlight(request, preventAddNewPaymentMethodDefaultSelection);
            }

            string india3dsEnableForBilldesk = GetPartnerFlight(request, Flighting.Features.India3dsEnableForBilldesk);
            if (!string.IsNullOrEmpty(india3dsEnableForBilldesk))
            {
                exposableFeatures.Add(Flighting.Features.India3dsEnableForBilldesk);
                RemovePartnerFlight(request, india3dsEnableForBilldesk);
            }

            string pxDisplay3dsNotEnabledErrorInline = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXDisplay3dsNotEnabledErrorInline);
            if (!string.IsNullOrEmpty(pxDisplay3dsNotEnabledErrorInline))
            {
                exposableFeatures.Add(V7.Constants.PartnerFlightValues.PXDisplay3dsNotEnabledErrorInline);
                RemovePartnerFlight(request, pxDisplay3dsNotEnabledErrorInline);
            }

            string pxEnableUpi = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PxEnableUpi);
            if (!string.IsNullOrEmpty(pxEnableUpi))
            {
                exposableFeatures.Add(V7.Constants.PartnerFlightValues.PxEnableUpi);
                RemovePartnerFlight(request, pxEnableUpi);
            }

            // flight cleanup task - 57811922
            string enableNewLogoForSepa = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.EnableNewLogoSepa);
            if (!string.IsNullOrEmpty(enableNewLogoForSepa))
            {
                // if flight is on, we add this flight to exposableFeatures to use this to use the logo for sepa
                exposableFeatures.Add(V7.Constants.PartnerFlightValues.EnableNewLogoSepa);
                RemovePartnerFlight(request, enableNewLogoForSepa);
            }

            string pxCommercialEnableUpi = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXCommercialEnableUpi);

            if (!string.IsNullOrEmpty(pxCommercialEnableUpi))
            {
                exposableFeatures.Add(V7.Constants.PartnerFlightValues.PXCommercialEnableUpi);
                RemovePartnerFlight(request, pxCommercialEnableUpi);
            }

            if (exposableFeatures.Contains(V7.Constants.PartnerFlightValues.PxEnableUpi, StringComparer.OrdinalIgnoreCase) || exposableFeatures.Contains(V7.Constants.PartnerFlightValues.PXCommercialEnableUpi, StringComparer.OrdinalIgnoreCase))
            {
                exposableFeatures.Add(V7.Constants.PartnerFlightValues.PXUsePartnerSettingsService); // this flight is required to handle the percentage qualified subsequent request.
            }

            string enableGlobalUpiQr = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.EnableGlobalUpiQr);
            if (!string.IsNullOrEmpty(enableGlobalUpiQr))
            {
                exposableFeatures.Add(V7.Constants.PartnerFlightValues.EnableGlobalUpiQr);
                RemovePartnerFlight(request, enableGlobalUpiQr);
            }

            string enableCommercialGlobalUpiQr = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.EnableCommercialGlobalUpiQr);
            if (!string.IsNullOrEmpty(enableCommercialGlobalUpiQr))
            {
                exposableFeatures.Add(V7.Constants.PartnerFlightValues.EnableCommercialGlobalUpiQr);
                RemovePartnerFlight(request, enableCommercialGlobalUpiQr);
            }

            string indiaUpiEnable = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.IndiaUPIEnable);
            if (!string.IsNullOrEmpty(indiaUpiEnable))
            {
                exposableFeatures.Add(V7.Constants.PartnerFlightValues.IndiaUPIEnable);
                RemovePartnerFlight(request, indiaUpiEnable);
            }

            string enableSelectUpiQr = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.EnableSelectUpiQr);
            if (!string.IsNullOrEmpty(enableSelectUpiQr))
            {
                exposableFeatures.Add(V7.Constants.PartnerFlightValues.EnableSelectUpiQr);
                RemovePartnerFlight(request, enableSelectUpiQr);
            }

            string enableCommercialSelectUpiQr = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.EnableCommercialSelectUpiQr);
            if (!string.IsNullOrEmpty(enableCommercialSelectUpiQr))
            {
                exposableFeatures.Add(V7.Constants.PartnerFlightValues.EnableCommercialSelectUpiQr);
                RemovePartnerFlight(request, enableCommercialSelectUpiQr);
            }

            string enableIndiaTokenExpiryDetails = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.EnableIndiaTokenExpiryDetails);
            if (!string.IsNullOrEmpty(enableIndiaTokenExpiryDetails))
            {
                exposableFeatures.Add(V7.Constants.PartnerFlightValues.EnableIndiaTokenExpiryDetails);
                RemovePartnerFlight(request, enableIndiaTokenExpiryDetails);
            }

            string pXEnableRupayForIN = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXEnableRupayForIN);
            if (!string.IsNullOrEmpty(pXEnableRupayForIN))
            {
                exposableFeatures.Add(V7.Constants.PartnerFlightValues.PXEnableRupayForIN);
                RemovePartnerFlight(request, pXEnableRupayForIN);
            }

            string pXIndiaRupayEnable = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.IndiaRupayEnable);
            if (!string.IsNullOrEmpty(pXIndiaRupayEnable))
            {
                exposableFeatures.Add(V7.Constants.PartnerFlightValues.IndiaRupayEnable);
                RemovePartnerFlight(request, pXIndiaRupayEnable);
            }

            string enableLtsUpiQRConsumer = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.EnableLtsUpiQRConsumer);
            if (!string.IsNullOrEmpty(enableLtsUpiQRConsumer))
            {
                exposableFeatures.Add(V7.Constants.PartnerFlightValues.EnableLtsUpiQRConsumer);
                RemovePartnerFlight(request, enableLtsUpiQRConsumer);
            }

            string pxEnableReturnFailedSessionState = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXReturnFailedSessionState);
            if (!string.IsNullOrEmpty(pxEnableReturnFailedSessionState))
            {
                exposableFeatures.Add(V7.Constants.PartnerFlightValues.PXReturnFailedSessionState);
                RemovePartnerFlight(request, pxEnableReturnFailedSessionState);
            }

            string pxEnablePSD2PaymentInstrumentSession = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.PXEnablePSD2PaymentInstrumentSession);
            if (!string.IsNullOrEmpty(pxEnablePSD2PaymentInstrumentSession))
            {
                exposableFeatures.Add(V7.Constants.PartnerFlightValues.PXEnablePSD2PaymentInstrumentSession);
                RemovePartnerFlight(request, pxEnablePSD2PaymentInstrumentSession);
            }

            string enableItalyCodiceFiscale = GetPartnerFlight(request, V7.Constants.PartnerFlightValues.EnableItalyCodiceFiscale);
            if (!string.IsNullOrEmpty(enableItalyCodiceFiscale))
            {
                exposableFeatures.Add(V7.Constants.PartnerFlightValues.EnableItalyCodiceFiscale);
                RemovePartnerFlight(request, enableItalyCodiceFiscale);
            }

            string includeCultureAndLanguageTransformation = GetPartnerFlight(request, Flighting.Features.PXIncludeCultureAndLanguageTransformation);
            if (!string.IsNullOrEmpty(includeCultureAndLanguageTransformation))
            {
                exposableFeatures.Add(Flighting.Features.PXIncludeCultureAndLanguageTransformation);
                RemovePartnerFlight(request, includeCultureAndLanguageTransformation);
            }

            string pXEnabledNoSubmitIfGSTIDEmpty = GetPartnerFlight(request, Flighting.Features.PXEnabledNoSubmitIfGSTIDEmpty);
            if (!string.IsNullOrEmpty(pXEnabledNoSubmitIfGSTIDEmpty))
            {
                exposableFeatures.Add(Flighting.Features.PXEnabledNoSubmitIfGSTIDEmpty);
                RemovePartnerFlight(request, pXEnabledNoSubmitIfGSTIDEmpty);
            }

            string overrideCultureAndLanguageTransformation = GetPartnerFlight(request, Flighting.Features.PXOverrideCultureAndLanguageTransformation);
            if (!string.IsNullOrEmpty(overrideCultureAndLanguageTransformation))
            {
                exposableFeatures.Add(Flighting.Features.PXOverrideCultureAndLanguageTransformation);
                RemovePartnerFlight(request, overrideCultureAndLanguageTransformation);
            }

            string pxPaasAddCCDfpIframe = GetPartnerFlight(request, Flighting.Features.PXPaasAddCCDfpIframe);
            if (!string.IsNullOrEmpty(pxPaasAddCCDfpIframe))
            {
                exposableFeatures.Add(Flighting.Features.PXPaasAddCCDfpIframe);
                RemovePartnerFlight(request, pxPaasAddCCDfpIframe);
            }

            string pxAddCCDfpIframe = GetPartnerFlight(request, Flighting.Features.PXAddCCDfpIframe);
            if (!string.IsNullOrEmpty(pxAddCCDfpIframe))
            {
                exposableFeatures.Add(Flighting.Features.PXAddCCDfpIframe);
                RemovePartnerFlight(request, pxAddCCDfpIframe);
            }

            string pxConfirmDfpIframe = GetPartnerFlight(request, Flighting.Features.PXConfirmDfpIframe);
            if (!string.IsNullOrEmpty(pxConfirmDfpIframe))
            {
                exposableFeatures.Add(Flighting.Features.PXConfirmDfpIframe);
                RemovePartnerFlight(request, pxConfirmDfpIframe);
            }

            string setDefaultAddressCountryForProfileUpdatePartial = GetPartnerFlight(request, Flighting.Features.PXSetDefaultAddressCountryForProfileUpdatePartial);
            if (!string.IsNullOrEmpty(setDefaultAddressCountryForProfileUpdatePartial))
            {
                exposableFeatures.Add(Flighting.Features.PXSetDefaultAddressCountryForProfileUpdatePartial);
                RemovePartnerFlight(request, setDefaultAddressCountryForProfileUpdatePartial);
            }

            string pxPSD2EnableCSPUrlProxyFrame = GetPartnerFlight(request, Flighting.Features.PXPSD2EnableCSPUrlProxyFrame);
            if (!string.IsNullOrEmpty(pxPSD2EnableCSPUrlProxyFrame))
            {
                exposableFeatures.Add(Flighting.Features.PXPSD2EnableCSPUrlProxyFrame);
                RemovePartnerFlight(request, pxPSD2EnableCSPUrlProxyFrame);
            }

            string pxPSD2EnableCSPUrlProxyFrameWithSanitizedInput = GetPartnerFlight(request, Flighting.Features.PXPSD2EnableCSPUrlProxyFrameWithSanitizedInput);
            if (!string.IsNullOrEmpty(pxPSD2EnableCSPUrlProxyFrameWithSanitizedInput))
            {
                exposableFeatures.Add(Flighting.Features.PXPSD2EnableCSPUrlProxyFrameWithSanitizedInput);
                RemovePartnerFlight(request, pxPSD2EnableCSPUrlProxyFrameWithSanitizedInput);
            }

            // Add PXEnableIndia3DS1Challenge flight to India 3ds test accounts.
            // Task 38194974 was created to remove this change after testing.
            if (IsThreeDSOneTestAccount(accountId))
            {
                exposableFeatures.Add(Flighting.Features.PXEnableIndia3DS1Challenge);
                exposableFeatures.Add(Flighting.Features.India3dsEnableForBilldesk);
            }

            // Partner Setttings Service
            // Remove it from x-ms-flight to prevent passing to downstream service
            string partnerSettingsEnabled = GetPartnerFlight(request, Flighting.Features.PXUsePartnerSettingsService);
            if (!string.IsNullOrEmpty(partnerSettingsEnabled))
            {
                exposableFeatures.Add(Flighting.Features.PXUsePartnerSettingsService);
                RemovePartnerFlight(request, Flighting.Features.PXUsePartnerSettingsService);
            }

            string partnerSettingsCacheDisabled = GetPartnerFlight(request, Flighting.Features.PXDisablePSSCache);
            if (!string.IsNullOrEmpty(partnerSettingsCacheDisabled))
            {
                exposableFeatures.Add(Flighting.Features.PXDisablePSSCache);
                RemovePartnerFlight(request, Flighting.Features.PXDisablePSSCache);
            }

            string pxDisableMSRewardsVariableAmount = GetPartnerFlight(request, Flighting.Features.PXDisableMSRewardsVariableAmount);
            if (!string.IsNullOrEmpty(pxDisableMSRewardsVariableAmount))
            {
                exposableFeatures.Add(Flighting.Features.PXDisableMSRewardsVariableAmount);
                RemovePartnerFlight(request, Flighting.Features.PXDisableMSRewardsVariableAmount);
            }

            string useCDNForStaticResourceService = GetPartnerFlight(request, Flighting.Features.PXUseCDNForStaticResourceService);
            if (!string.IsNullOrEmpty(useCDNForStaticResourceService))
            {
                exposableFeatures.Add(Flighting.Features.PXUseCDNForStaticResourceService);
                RemovePartnerFlight(request, Flighting.Features.PXUseCDNForStaticResourceService);
            }

            string pxEnablePIMSGetPaymentMethodsCache = GetPartnerFlight(request, Flighting.Features.PXEnablePIMSGetPaymentMethodsCache);
            if (!string.IsNullOrEmpty(pxEnablePIMSGetPaymentMethodsCache))
            {
                exposableFeatures.Add(Flighting.Features.PXEnablePIMSGetPaymentMethodsCache);
                RemovePartnerFlight(request, pxEnablePIMSGetPaymentMethodsCache);
            }

            string pxUsePifdBaseUrlInsteadOfForwardedHostHeader = GetPartnerFlight(request, Flighting.Features.PXUsePifdBaseUrlInsteadOfForwardedHostHeader);
            if (!string.IsNullOrEmpty(pxUsePifdBaseUrlInsteadOfForwardedHostHeader))
            {
                exposableFeatures.Add(Flighting.Features.PXUsePifdBaseUrlInsteadOfForwardedHostHeader);
                RemovePartnerFlight(request, pxUsePifdBaseUrlInsteadOfForwardedHostHeader);
            }

            string pxEnableSMSChallengeValidation = GetPartnerFlight(request, Flighting.Features.PXEnableSMSChallengeValidation);
            if (!string.IsNullOrEmpty(pxEnableSMSChallengeValidation))
            {
                exposableFeatures.Add(Flighting.Features.PXEnableSMSChallengeValidation);
                RemovePartnerFlight(request, pxEnableSMSChallengeValidation);
            }

            string updateNewPaymentMethodLinkActionContext = GetPartnerFlight(request, Flighting.Features.UpdateNewPaymentMethodLinkActionContext);
            if (!string.IsNullOrEmpty(updateNewPaymentMethodLinkActionContext))
            {
                exposableFeatures.Add(Flighting.Features.UpdateNewPaymentMethodLinkActionContext);
                RemovePartnerFlight(request, updateNewPaymentMethodLinkActionContext);
            }

            string pxEnableApplyPIXboxNativeStyleHints = GetPartnerFlight(request, Flighting.Features.PXEnableApplyPIXboxNativeStyleHints);
            if (!string.IsNullOrEmpty(pxEnableApplyPIXboxNativeStyleHints))
            {
                exposableFeatures.Add(Flighting.Features.PXEnableApplyPIXboxNativeStyleHints);
                RemovePartnerFlight(request, pxEnableApplyPIXboxNativeStyleHints);
            }

            string pxEnableMSRewardsChallenge = GetPartnerFlight(request, Flighting.Features.PXEnableMSRewardsChallenge);
            if (!string.IsNullOrEmpty(pxEnableMSRewardsChallenge))
            {
                exposableFeatures.Add(Flighting.Features.PXEnableMSRewardsChallenge);
                RemovePartnerFlight(request, pxEnableMSRewardsChallenge);
            }

            string skipJarvisAddressSyncToLegacy = GetPartnerFlight(request, Flighting.Features.PXSkipJarvisAddressSyncToLegacy);
            if (!string.IsNullOrEmpty(skipJarvisAddressSyncToLegacy))
            {
                exposableFeatures.Add(Flighting.Features.PXSkipJarvisAddressSyncToLegacy);
                RemovePartnerFlight(request, Flighting.Features.PXSkipJarvisAddressSyncToLegacy);
            }

            string pxEnableAddCcQrCode = GetPartnerFlight(request, Flighting.Features.PxEnableAddCcQrCode);
            if (!string.IsNullOrEmpty(pxEnableAddCcQrCode))
            {
                exposableFeatures.Add(Flighting.Features.PxEnableAddCcQrCode);
                RemovePartnerFlight(request, Flighting.Features.PxEnableAddCcQrCode);
            }

            string pxUsePaymentSessionsHandlerV2 = GetPartnerFlight(request, Flighting.Features.PXUsePaymentSessionsHandlerV2);
            if (!string.IsNullOrEmpty(pxUsePaymentSessionsHandlerV2))
            {
                exposableFeatures.Add(Flighting.Features.PXUsePaymentSessionsHandlerV2);
                RemovePartnerFlight(request, Flighting.Features.PXUsePaymentSessionsHandlerV2);
            }

            string pxUseGetVersionBasedPaymentSessionsHandler = GetPartnerFlight(request, Flighting.Features.PXUseGetVersionBasedPaymentSessionsHandler);
            if (!string.IsNullOrEmpty(pxUseGetVersionBasedPaymentSessionsHandler))
            {
                exposableFeatures.Add(Flighting.Features.PXUseGetVersionBasedPaymentSessionsHandler);
                RemovePartnerFlight(request, Flighting.Features.PXUseGetVersionBasedPaymentSessionsHandler);
            }

            string pxEnableDefaultPaymentMethod = GetPartnerFlight(request, Flighting.Features.PXEnableDefaultPaymentMethod);
            if (!string.IsNullOrEmpty(pxEnableDefaultPaymentMethod))
            {
                exposableFeatures.Add(Flighting.Features.PXEnableDefaultPaymentMethod);
                RemovePartnerFlight(request, Flighting.Features.PXEnableDefaultPaymentMethod);
            }

            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXDisablePMGrouping);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXLuhnValidationEnabledPartners);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXDisableInvalidPaymentInstrumentType);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableCUPInternational);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableSkipGetPMIfCreditCardIsTheOnlyOption);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnablePIMSPPEEnvironment);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnablePayPay);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableAlipayHK);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableGCash);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableTrueMoney);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableTouchNGo);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableAlipayCN);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableEmpOrgListPI);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableReplaceContextInstanceWithPaymentInstrumentId);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableVNextToPIMS);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableIsSelectPMSkippedValue);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXMakeAccountsAddressEnrichmentCall);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXUseJarvisAccountsForAddressEnrichment);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableAlipayCNLimitText);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableCachingTokenizationEncryption);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableTokenizationEncryptionAddUpdateCC);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableTokenizationEncryptionOtherOperation);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableTokenizationEncryptionFetchConfigAddUpdateCC);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableTokenizationEncryptionFetchConfigWithScript);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableTokenizationEncryptionFetchConfigOtherOperation);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableTokenizationEncFetchConfigAddCCPiAuthKey);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXDisableTokenizationEncPiAuthKeyFetchConfigtEncPayload);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXSetIsSubmitGroupFalseForTradeAVSV1);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableSetCancelButtonDisplayContentAsBack);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableAddAllFieldsRequiredText);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableVATID);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableSecondaryValidationMode);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnablePSD2ForGuestCheckoutFlow);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXUsePSSToEnableValidatePIOnAttachChallenge);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableThrowInvalidUrlParameterException);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableExpiryCVVGrouping);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableSavePaymentDetails);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnablePSD2ForGooglePay);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXUseMockWalletConfig);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableGooglePayApplePayOnlyInUS);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.GPayApayInstancePI);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.IndiaCvvChallengeExpiryGroupDelete);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXSkipChallengeForZeroAmountIndiaAuth);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableModernIdealPayment);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXPSD2SkipFingerprint);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXPSD2TimeoutOnPostViaSrc);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXIntegrateFraudDetectionService);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableChallengeCvvValidation);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXWalletConfigAddDeviceSupportStatus);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXWalletConfigDisableGooglePay);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXWalletConfigDisableApplePay);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXPSD2EnableCSPProxyFrame);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXPSD2EnableCSPPostThreeDSMethodDataSrc);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXPSD2EnableCSPPostThreeDSSessionDataSrc);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXSkipDuplicatePostProcessForMotoAndRewards);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableBrowserBasedDeviceChannel);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableAllComponentDescriptions);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableUsePOCapabilities);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.UsePaymentRequestApi);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableCachedPrefetcherData);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.UpdateAddressline1MaxLength);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXRemoveJarvisHeadersFromSubmitUrl);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableChallengesForMOTO);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXSkipPifdAddressPostForNonAddressesType);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXUseInlineExpressCheckoutHtml);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXExpressCheckoutUseIntStaticResources);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXExpressCheckoutUseProdStaticResources);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableSepaRedirectUrlText);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXPSD2SkipFingerprintByUrl);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableGetSessionWithSessionId);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXChangeExpiryMonthYearToExpiryDateTextBox);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnablePartnerSettingsDeepCopy);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXOverrideHasAnyPIToTrue);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableXboxNativeRewards);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXShowRewardsErrorPageOnChallenge);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXSkipAdditionalValidationForZeroAmount);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXEnableAddAsteriskToAllMandatoryFields);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXUseNTSIntUrl);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXDisableGetWalletConfigCache);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXPaasAddCCDfpIframeForCommerceRisk);
            ExtractAndRemovePartnerFlight(request, exposableFeatures, Flighting.Features.PXCombineExpiryMonthYearToDateTextBox);

            string pXEnableSearchTransactionParallelRequest = GetPartnerFlight(request, Flighting.Features.PXEnableSearchTransactionParallelRequest);
            if (!string.IsNullOrEmpty(pXEnableSearchTransactionParallelRequest))
            {
                exposableFeatures.Add(Flighting.Features.PXEnableSearchTransactionParallelRequest);
                RemovePartnerFlight(request, Flighting.Features.PXEnableSearchTransactionParallelRequest);
            }

            string pxEnableGettingStoredSessionForChallengeDescriptionsController = GetPartnerFlight(request, Flighting.Features.PXEnableGettingStoredSessionForChallengeDescriptionsController);
            if (!string.IsNullOrEmpty(pxEnableGettingStoredSessionForChallengeDescriptionsController))
            {
                exposableFeatures.Add(Flighting.Features.PXEnableGettingStoredSessionForChallengeDescriptionsController);
                RemovePartnerFlight(request, Flighting.Features.PXEnableGettingStoredSessionForChallengeDescriptionsController);
            }

            string pXEnableHandleTransactionNotAllowed = GetPartnerFlight(request, Flighting.Features.PXEnableHandleTransactionNotAllowed);
            if (!string.IsNullOrEmpty(pXEnableHandleTransactionNotAllowed))
            {
                exposableFeatures.Add(Flighting.Features.PXEnableHandleTransactionNotAllowed);
                RemovePartnerFlight(request, Flighting.Features.PXEnableHandleTransactionNotAllowed);
            }

            string pxEnableRiskEligibilityCheck = GetPartnerFlight(request, Flighting.Features.PxEnableRiskEligibilityCheck);
            if (!string.IsNullOrEmpty(pxEnableRiskEligibilityCheck))
            {
                exposableFeatures.Add(Flighting.Features.PxEnableRiskEligibilityCheck);
                RemovePartnerFlight(request, Flighting.Features.PxEnableRiskEligibilityCheck);
            }

            if (WebHostingUtility.IsApplicationSelfHosted())
            {
                // Used to allow the local and PR DiffTest to request the PSS Partner mock from emulator and required for only Selfhosted env
                string pxUsePSSPartnerMockForDiffTest = GetPartnerFlight(request, Flighting.Features.PXUsePSSPartnerMockForDiffTest);
                if (!string.IsNullOrEmpty(pxUsePSSPartnerMockForDiffTest))
                {
                    exposableFeatures.Add(Flighting.Features.PXUsePSSPartnerMockForDiffTest);
                    RemovePartnerFlight(request, pxUsePSSPartnerMockForDiffTest);
                }

                // Add the reuqest to selfhostHttpContext for getting the request headers in Selfhost env while sending the outgoing requests like pss settings
                SelfhostHttpContext.Request = request;
            }

            // Remove flight if the pidlsdkVersion is less than the lowest compatible version
            Version fullPidlSdkVersion = HttpRequestHelper.GetFullPidlSdkVersion(request);
            try
            {
                Version lowestCompatiblePidlVersion = new Version(V7.Constants.PidlSdkVersionNumber.PidlSdkMajor2, V7.Constants.PidlSdkVersionNumber.PidlSdkMinor4, V7.Constants.PidlSdkVersionNumber.PidlSdkBuild0, V7.Constants.PidlSdkVersionNumber.PidlSdkAlpha0);
                FlightByPidlVersion(fullPidlSdkVersion, lowestCompatiblePidlVersion, V7.Constants.PartnerFlightValues.PXEnableXboxAccessibilityHint, exposableFeatures);
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException(ex.ToString(), request.GetRequestCorrelationId());
            }

            try
            {
                Version lowestCompatiblePidlVersion = new Version(V7.Constants.PidlSdkVersionNumber.PidlSdkMajor2, V7.Constants.PidlSdkVersionNumber.PidlSdkMinor5, V7.Constants.PidlSdkVersionNumber.PidlSdkBuild0, V7.Constants.PidlSdkVersionNumber.PidlSdkAlpha0);
                FlightByPidlVersion(fullPidlSdkVersion, lowestCompatiblePidlVersion, V7.Constants.PartnerFlightValues.PXEnableXboxNewAddressSequenceFrNl, exposableFeatures);
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException(ex.ToString(), request.GetRequestCorrelationId());
            }

            try
            {
                Version lowestCompatiblePidlVersion = new Version(V7.Constants.PidlSdkVersionNumber.PidlSdkMajor2, V7.Constants.PidlSdkVersionNumber.PidlSdkMinor7, V7.Constants.PidlSdkVersionNumber.PidlSdkBuild0, V7.Constants.PidlSdkVersionNumber.PidlSdkAlpha0);
                FlightByPidlVersion(fullPidlSdkVersion, lowestCompatiblePidlVersion, V7.Constants.PartnerFlightValues.PXEnableXboxNativeStyleHints, exposableFeatures);
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException(ex.ToString(), request.GetRequestCorrelationId());
            }

            // Use Partner Setttings Service if flight is enabled
            PartnerSettings partnerSettings = await PartnerSettingsHelper.GetPaymentExperienceSetting(settings, partner, partnerSettingsVersion, traceActivityId, exposableFeatures);

            SetProperty(GlobalConstants.RequestPropertyKeys.PartnerSettings, partnerSettings);
            SetProperty(GlobalConstants.RequestPropertyKeys.ExposedFlightFeatures, exposableFeatures);
            SetProperty(GlobalConstants.RequestPropertyKeys.FlightAssignmentContext, featureConfig?.AssignmentContext);
            SetProperty(GlobalConstants.RequestPropertyKeys.FlightFeatureConfig, featureConfig);

            // When an account has the PXRateLimitPerAccountOnChallengeApis, return BadRequest response.
            // This is different from PXReturn502ForMaliciousRequest as that flight is to prevent sdk retires
            // and PXRateLimitPerAccountOnChallengeApis prevents accounts from hitting the APIs
            // Side note: In order to add unit test coverage to flights in this handler class, we need to
            // abstract the FlightClient and mock it.
            if (exposableFeatures != null
                && exposableFeatures.Contains(Flighting.Features.PXRateLimitPerAccountOnChallengeApis, StringComparer.OrdinalIgnoreCase)
                && (request.RequestUri.AbsolutePath.IndexOf("ChallengeDescriptions", StringComparison.OrdinalIgnoreCase) >= 0
                    || request.RequestUri.AbsolutePath.IndexOf("PaymentSessions", StringComparison.OrdinalIgnoreCase) >= 0
                    || request.RequestUri.AbsolutePath.IndexOf("PaymentSessionDescriptions", StringComparison.OrdinalIgnoreCase) >= 0
                    || request.RequestUri.AbsolutePath.IndexOf("RDSSession", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return request.CreateResponse(HttpStatusCode.BadRequest);
            }

            // when the flight PXReturn502ForMaliciousRequest is on, return 502(BadGateway) response with a flag to disable retry on server
            if (exposableFeatures != null
                && exposableFeatures.Contains(Flighting.Features.PXReturn502ForMaliciousRequest, StringComparer.OrdinalIgnoreCase))
            {
                HttpResponseMessage responseMessage = request.CreateResponse(HttpStatusCode.BadGateway);
                responseMessage.Headers.Add(RetryOnServerErrorHeader, "false");
                return responseMessage;
            }

            IEnumerable<KeyValuePair<string, string>>? queryStrings = null;
            if (request.ContainsProperty(PaymentConstants.Web.Properties.QueryParameters))
            {
                queryStrings = request.GetProperty<IEnumerable<KeyValuePair<string, string>>>(PaymentConstants.Web.Properties.QueryParameters);
            }

            if (queryStrings == null)
            {
                queryStrings = request.GetQueryNameValuePairs();
                if (queryStrings != null)
                {
                    request.AddProperty(PaymentConstants.Web.Properties.QueryParameters, queryStrings);
                }
            }

            if (string.IsNullOrEmpty(externalVersion))
            {
                return request.CreateNoApiVersionResponse();
            }

            ApiVersion apiVersion;
            if (!this.supportedVersions.TryGetValue(externalVersion, out apiVersion))
            {
                return request.CreateInvalidApiVersionResponse(externalVersion);
            }

            if (request.ContainsProperty(PaymentConstants.Web.Properties.Version))
            {
                SetProperty(PaymentConstants.Web.Properties.Version, apiVersion);
            }
            else
            {
                request.AddProperty(PaymentConstants.Web.Properties.Version, apiVersion);
            }

            if (this.next != null && httpContext != null)
            {
                if (httpContext.Request.Body.CanSeek)
                {
                    httpContext.Request.Body.Position = 0;
                }

                httpContext.Response.OnStarting(() =>
                {
                    httpContext.Response.Headers[OperationVersionHeader] = apiVersion.ExternalVersion;

                    if (exposableFeatures != null && exposableFeatures.Count != 0)
                    {
                        httpContext.Response.Headers[ExposableFlightsHeader] = string.Join(",", exposableFeatures);
                    }

                    if (!string.IsNullOrWhiteSpace(featureConfig?.AssignmentContext))
                    {
                        httpContext.Response.Headers[PXFlightAssignmentContextHeader] = featureConfig.AssignmentContext;
                    }

                    if (exposableFeatures != null && exposableFeatures.Contains(Flighting.Features.PXSendContentTypeOptionsHeader, StringComparer.OrdinalIgnoreCase))
                    {
                        httpContext.Response.Headers[ContentTypeOptionsHeader] = NoSniff;
                    }

                    if (exposableFeatures != null && exposableFeatures.Contains(Flighting.Features.PXSendNoRetryOnServerErrorHeader, StringComparer.OrdinalIgnoreCase))
                    {
                        httpContext.Response.Headers[RetryOnServerErrorHeader] = "false";
                    }

                    return Task.CompletedTask;
                });

                await this.next(httpContext);

                return new HttpResponseMessage((HttpStatusCode)httpContext.Response.StatusCode);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        /// <summary>
        /// Remove flight to prevent passing through dwonstream service
        /// </summary>
        /// <param name="request">Http request message</param>
        /// <param name="exposableFeatures">Flight features list</param>
        /// <param name="flightName">Flight to be removed</param>
        private static void ExtractAndRemovePartnerFlight(HttpRequestMessage request, List<string> exposableFeatures, string flightName)
        {
            string partnerFlight = GetPartnerFlight(request, flightName);
            if (!string.IsNullOrEmpty(partnerFlight))
            {
                exposableFeatures.Add(flightName);
                RemovePartnerFlight(request, partnerFlight);
            }
        }

        private static void HandleSecureFieldFlights(List<string> exposableFeatures, HttpRequestMessage request)
        {
            HandleFlight(exposableFeatures, request, V7.Constants.PartnerFlightValues.PXEnableSecureFieldAddCreditCard);
            HandleFlight(exposableFeatures, request, V7.Constants.PartnerFlightValues.PXEnableSecureFieldUpdateCreditCard);
            HandleFlight(exposableFeatures, request, V7.Constants.PartnerFlightValues.PXEnableSecureFieldReplaceCreditCard);
            HandleFlight(exposableFeatures, request, V7.Constants.PartnerFlightValues.PXEnableSecureFieldSearchTransaction);
            HandleFlight(exposableFeatures, request, V7.Constants.PartnerFlightValues.PXEnableSecureFieldCvvChallenge);
            HandleFlight(exposableFeatures, request, V7.Constants.PartnerFlightValues.PXEnableSecureFieldIndia3DSChallenge);
        }

        private static void HandleFlight(List<string> exposableFeatures, HttpRequestMessage request, string flightPrefix)
        {
            string flight = GetPartnerFlight(request, flightPrefix);
            if (!string.IsNullOrEmpty(flight))
            {
                exposableFeatures.Add(flight);
                RemovePartnerFlight(request, flight);
            }
        }

        private static string GetPartnerFlight(HttpRequestMessage request, string flightPrefix)
        {
            string flightValueString = request.GetRequestHeader(GlobalConstants.HeaderValues.ExtendedFlightName);
            if (flightValueString != null)
            {
                List<string> flightValues = new List<string>(flightValueString.Split(',').Select(value => value.Trim()));
                return flightValues.Where(flightValue => flightValue.StartsWith(flightPrefix, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            }

            return null;
        }

        private static void RemovePartnerFlight(HttpRequestMessage request, string flightName)
        {
            string flightValueString = request.GetRequestHeader(GlobalConstants.HeaderValues.ExtendedFlightName);
            if (flightValueString != null)
            {
                List<string> flightValue = new List<string>(flightValueString.Split(','));
                int targetFlightIndex = flightValue.FindIndex(x => x.Equals(flightName, StringComparison.OrdinalIgnoreCase));
                if (targetFlightIndex != -1)
                {
                    flightValue.RemoveAt(targetFlightIndex);
                    string newflightValueString = string.Join(",", flightValue.ToArray());
                    request.Headers.Remove(GlobalConstants.HeaderValues.ExtendedFlightName);
                    if (!string.IsNullOrEmpty(newflightValueString))
                    {
                        request.Headers.Add(GlobalConstants.HeaderValues.ExtendedFlightName, newflightValueString);
                    }
                }
            }
        }

        private static void FlightByPidlVersion(Version fullPidlSdkVersion, Version lowestCompatiblePidlVersion, string flightName, List<string> exposableFeatures)
        {
            if (fullPidlSdkVersion != null && lowestCompatiblePidlVersion != null && exposableFeatures != null && fullPidlSdkVersion < lowestCompatiblePidlVersion && exposableFeatures.Contains(flightName))
            {
                exposableFeatures.Remove(flightName);
            }
        }

        private static string GetOriginCountry(string originCountryFlight)
        {
            string originCountry = null;
            if (!string.IsNullOrEmpty(originCountryFlight) && originCountryFlight.Length > V7.Constants.PartnerFlightValues.OriginCountryPrefix.Length)
            {
                originCountry = originCountryFlight.Substring(V7.Constants.PartnerFlightValues.OriginCountryPrefix.Length).ToLowerInvariant();
            }

            return originCountry;
        }

        private static string GetPartnerSettingsVersion(string partnerSettingsVersionPartnerFlight)
        {
            string partnerSettingsVersion = null;
            if (!string.IsNullOrEmpty(partnerSettingsVersionPartnerFlight) && partnerSettingsVersionPartnerFlight.Length > V7.Constants.PartnerFlightValues.PartnerSettingsVersionPrefix.Length)
            {
                partnerSettingsVersion = partnerSettingsVersionPartnerFlight.Substring(V7.Constants.PartnerFlightValues.PartnerSettingsVersionPrefix.Length).ToLowerInvariant();
            }

            return partnerSettingsVersion;
        }

        private static bool IsThreeDSOneTestAccount(string accountId)
        {
            return GlobalConstants.ThreeDSTestAccountIds.Contains(accountId, StringComparer.OrdinalIgnoreCase);
        }

        private string GetUserIpAddress(HttpRequestMessage request)
        {
            string retVal = null;
            try
            {
                Dictionary<string, object> context = null;
                ContextHelper.GetContext(request, settings, ref context);
                retVal = ContextHelper.TryGetContextValue(context, GlobalConstants.ClientContextKeys.DeviceInfo.IPAddress);
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException(ex.ToString(), request.GetRequestCorrelationId());
            }

            return retVal;
        }
    }
}
