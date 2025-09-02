// <copyright file="SettingsController.cs" company="Microsoft Corporation">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using Microsoft.AspNetCore.Mvc;
    using Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.Pidl.Localization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class SettingsController : ProxyController
    {
        private static readonly Dictionary<string, string> walletSettings = new Dictionary<string, string>()
        {
            { "ReplenishmentThresholdCount", "2" },
            { "ReplenishmentThresholdPercentage", "25" },
            { "ReplenishmentThresholdExpiryDays", "7" }
        };

        private static ConcurrentDictionary<string, string> paymentClientSettings = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Get Settings
        /// </summary>
        /// <group>Settings</group>
        /// <verb>GET</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/Settings</url>
        /// <param name="appName" required="true" cref="string" in="query">app name</param>
        /// <param name="appVersion" required="true" cref="string" in="query">app version</param>
        /// <param name="language" required="false" cref="string" in="query">language code</param>
        /// <response code="200">A setting object</response>
        /// <returns>A setting object</returns>
        [HttpGet]
        public ActionResult GetSettings([FromRoute] string appName, [FromRoute] string appVersion, [FromQuery] string language = null)
        {
            if (string.Equals(appName, Constants.AppDetails.WalletPackageName, StringComparison.OrdinalIgnoreCase))
            {
                return this.StatusCode((int)HttpStatusCode.OK, walletSettings);
            }
            else if (string.Equals(appName, Constants.AppDetails.PaymentClientAppName, StringComparison.OrdinalIgnoreCase))
            {
                return this.GetPaymentClientSettings(appVersion);
            }
            else if (string.Equals(appName, Constants.AppDetails.PaymentOptionsAppName, StringComparison.OrdinalIgnoreCase) && language != null)
            {
                Dictionary<string, string> localizedErrorObject = new Dictionary<string, string>
                {
                    { Constants.PaymentOptionsAppGenericErrorStringNames.Header, LocalizationRepository.Instance.GetLocalizedString(Constants.PaymentOptionsAppErrorStrings.Header, language) },
                    { Constants.PaymentOptionsAppGenericErrorStringNames.Body, LocalizationRepository.Instance.GetLocalizedString(Constants.PaymentOptionsAppErrorStrings.Body, language) },
                    { Constants.PaymentOptionsAppGenericErrorStringNames.SupportLink, LocalizationRepository.Instance.GetLocalizedString(Constants.PaymentOptionsAppErrorStrings.SupportLink, language) },
                    { Constants.PaymentOptionsAppGenericErrorStringNames.SupportLinkText, LocalizationRepository.Instance.GetLocalizedString(Constants.PaymentOptionsAppErrorStrings.SupportLinkText, language) },
                    { Constants.PaymentOptionsAppGenericErrorStringNames.ButtonText, LocalizationRepository.Instance.GetLocalizedString(Constants.PaymentOptionsAppErrorStrings.ButtonText, language) }
                };
                return this.StatusCode((int)HttpStatusCode.OK, localizedErrorObject);
            }
            else
            {
                return this.StatusCode(
                    (int)HttpStatusCode.NotFound,
                    new ServiceErrorResponse(
                        "SettingsDataNotFound",
                        string.Format("The settings data not found for App : {0}, Version : {1}", appName, appVersion)));
            }
        }

        /// <summary>
        /// Post Settings
        /// </summary>
        /// <group>Settings</group>
        /// <verb>POST</verb>
        /// <url>https://paymentexperience.cp.microsoft.com/v7.0/{accountId}/Settings</url>
        /// <param name="accountId" required="true" cref="string" in="path">account id</param>
        /// <param name="clientConfigData" required="true" cref="string" in="body">client config data</param>
        /// <response code="200">A setting object</response>
        /// <returns>A setting object</returns>
        [HttpPost]
        public ServerSettingResponse GetSettingsInPost([FromRoute] string accountId, [FromBody] ClientConfigData clientConfigData)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();

            // TODO: this change is to enable the endpoint only, I am adding a testing rule in the following
            // Will update rules based on the real business requirement.
            string browserType = clientConfigData.BrowserType;
            bool usePidlUI = false;
            if (string.Equals(browserType, "chrome", StringComparison.OrdinalIgnoreCase))
            {
                usePidlUI = true;
            }

            var response = new ServerSettingResponse()
            {
                UsePidlUI = usePidlUI
            };

            return response;
        }

        private ActionResult GetPaymentClientSettings(string version)
        {
            EventTraceActivity traceActivityId = this.Request.GetRequestCorrelationId();
            try
            {
                string paymentClientConfig = null;

                if (!paymentClientSettings.TryGetValue(version, out paymentClientConfig))
                {
                    paymentClientConfig = System.IO.File.ReadAllText(
                        Path.Combine(
                            AppDomain.CurrentDomain.BaseDirectory,
                            string.Format(@"App_Data\PSD2Config\{0}\PaymentClientSettings.json", version)));

                    paymentClientSettings.TryAdd(version, paymentClientConfig);
                }

                JObject settingsConfig = JsonConvert.DeserializeObject<JObject>(paymentClientConfig);
                return this.StatusCode((int)HttpStatusCode.OK, settingsConfig);
            }
            catch (Exception ex)
            {
                SllWebLogger.TracePXServiceException(ex.ToString(), traceActivityId);
                return this.StatusCode(
                    (int)HttpStatusCode.NotFound,
                    new ServiceErrorResponse(
                        "SettingsDataNotFound",
                        string.Format("The version not found for Version : {0}", version)));
            }
        }
    }
}