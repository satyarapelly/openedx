// <copyright file="PIMSAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Tracing;
    using System.IdentityModel.Tokens.Jwt;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentTransaction.Model;
    using Newtonsoft.Json;
    using V7;
    using static Microsoft.Commerce.Payments.PXService.GlobalConstants;

    public class PIMSAccessor : IPIMSAccessor
    {
        public const string PimsServiceName = "PIManagementService";
        private const int PimsPaymentMethodsRefreshInternvalInSec = 60;
        private const int PimsPaymentMethodsCacheMaxLimit = 1000;

        private readonly List<string> passThroughHeaders = new List<string>
        {
            GlobalConstants.HeaderValues.AuthInfoHeader,
            GlobalConstants.HeaderValues.ExtendedFlightName,
            PaymentConstants.PaymentExtendedHttpHeaders.TestHeader,
            PaymentConstants.PaymentExtendedHttpHeaders.CorrelationContext,
            GlobalConstants.HeaderValues.CustomerHeader,
            GlobalConstants.HeaderValues.DeviceInfoHeader,
            GlobalConstants.HeaderValues.RiskInfoHeader,
            GlobalConstants.HeaderValues.XMsRequestContext
        };

        private readonly Dictionary<string, string> partnerNamesMappingForPimsRequests = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { PXCommon.Constants.PartnerNames.Macmanage, PXCommon.Constants.PartnerNames.CommercialStores }
        };

        private string serviceBaseUrl;
        private string emulatorBaseUrl;
        private string servicePPEBaseUrl;

        private string apiVersion;
        private HttpClient pimsHttpClient;

        private Dictionary<string, Tuple<List<PaymentMethod>, DateTime>> pimsPaymentMethodsCache;
        private object pimsPaymentMethodsLockObj = new object();

        public PIMSAccessor(
            string serviceBaseUrl,
            string emulatorBaseUrl,
            string servicePPEBaseUrl,
            string apiVersion,
            HttpMessageHandler messageHandler)
        {
            this.serviceBaseUrl = serviceBaseUrl;
            this.emulatorBaseUrl = emulatorBaseUrl;
            this.servicePPEBaseUrl = servicePPEBaseUrl;
            this.apiVersion = apiVersion;

            this.pimsHttpClient = new PXTracingHttpClient(
               PXCommon.Constants.ServiceNames.InstrumentManagementService,
               messageHandler,
               logOutgoingRequestToApplicationInsight: ApplicationInsightsProvider.LogOutgoingOperation);
            this.pimsHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType));
            this.pimsHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.Connection, PaymentConstants.HttpHeaders.KeepAlive);
            this.pimsHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.KeepAlive, string.Format(PaymentConstants.HttpHeaders.KeepAliveParameter, 60));

            this.pimsPaymentMethodsCache = new Dictionary<string, Tuple<List<PaymentMethod>, DateTime>>();
        }

        private string BaseUrl
        {
            get
            {
                if (HttpRequestHelper.IsPXTestRequest() && !string.IsNullOrWhiteSpace(this.emulatorBaseUrl))
                {
                    return this.emulatorBaseUrl;
                }
                else
                {
                    return this.serviceBaseUrl;
                }
            }
        }

        public async Task<List<PaymentMethod>> GetPaymentMethods(string country, string family, string type, string language, EventTraceActivity traceActivityId, IList<KeyValuePair<string, string>> additionalHeaders = null, string partner = null, List<string> exposedFlightFeatures = null, string operation = null, PaymentExperienceSetting setting = null)
        {
            // All Pi's should be accessible for the XboxSettings partner
            if (string.Equals(partner, PXCommon.Constants.PartnerNames.XboxSettings) &&
                (string.Equals(operation, V7.Constants.Operations.Update, StringComparison.OrdinalIgnoreCase) || string.Equals(operation, V7.Constants.Operations.Delete, StringComparison.OrdinalIgnoreCase)))
            {
                country = string.Empty;
            }

            List<PaymentMethod> paymentMethods = new List<PaymentMethod>();

            string requestUrl = string.Format(V7.Constants.UriTemplate.GetPaymentMethods, country, family, type, language);

            if (additionalHeaders == null)
            {
                additionalHeaders = new List<KeyValuePair<string, string>>();
            }

            if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.VNextToPIMS, StringComparer.OrdinalIgnoreCase) && string.Equals(country, "br", StringComparison.InvariantCultureIgnoreCase))
            {
                additionalHeaders.Add(new KeyValuePair<string, string>(GlobalConstants.HeaderValues.ExtendedFlightName, "v-next"));
            }
            else if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXEnableVNextToPIMS, StringComparer.OrdinalIgnoreCase))
            {
                additionalHeaders.Add(new KeyValuePair<string, string>(GlobalConstants.HeaderValues.ExtendedFlightName, Flighting.Features.Vnext));
            }

            paymentMethods = await this.SendGetRequest<List<PaymentMethod>>(
                requestUrl,
                "GetPaymentMethods",
                traceActivityId,
                additionalHeaders,
                exposedFlightFeatures) ?? paymentMethods;

            paymentMethods = paymentMethods?.Where(
                pm => IsPaymentMethodAllowed(pm, country, partner, exposedFlightFeatures, operation, setting)).ToList();

            paymentMethods.ForEach(pm => PIHelper.OverridePMDisplayName(pm, partner, language, country, setting));

            return paymentMethods;
        }

        public async Task<PaymentInstrument> GetPaymentInstrument(string accountId, string piid, EventTraceActivity traceActivityId, string partner = null, string country = null, string language = null, List<string> exposedFlightFeatures = null, PaymentExperienceSetting setting = null)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.GetPI, accountId, piid);
            var pi = await this.SendGetRequest<PaymentInstrument>(
                requestUrl,
                "GetPaymentInstrument",
                traceActivityId,
                exposedFlightFeatures: exposedFlightFeatures);

            V7.PIHelper.SetPendingIfPayPalMIB(pi, partner);

            pi = IsPaymentMethodAllowed(pi?.PaymentMethod, country, partner, exposedFlightFeatures, setting: setting) ? pi : null;

            PIHelper.OverridePMDisplayName(pi?.PaymentMethod, partner, language, country, setting: setting);
            PIHelper.AddDefaultDisplayName(pi, partner, traceActivityId);

            return pi;
        }

        public async Task<PaymentInstrument> GetExtendedPaymentInstrument(string piid, EventTraceActivity traceActivityId, string partner = null, string country = null, List<string> exposedFlightFeatures = null)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.AccountlessGetExtendedPI, piid);
            List<KeyValuePair<string, string>> queryParams = new List<KeyValuePair<string, string>>();
            if (!string.IsNullOrWhiteSpace(partner))
            {
                queryParams.Add(new KeyValuePair<string, string>(V7.Constants.QueryParameterName.Partner, partner));
            }

            requestUrl = AppendQueryParams(requestUrl, queryParams);
            return await this.SendGetRequest<PaymentInstrument>(
                requestUrl,
                "GetExtendedPaymentInstrument",
                traceActivityId,
                exposedFlightFeatures: exposedFlightFeatures);
        }

        public async Task<PimsSessionDetailsResource> GetSessionDetails(string accountId, string sessionQueryUrl, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.GetSessionDetails, accountId, sessionQueryUrl);
            var sessionDetails = await this.SendGetRequest<PimsSessionDetailsResource>(
                requestUrl,
                "GetSessionDetails",
                traceActivityId);

            return sessionDetails;
        }

        public async Task<List<SearchTransactionAccountinfoByPI>> SearchByAccountNumber(object piid, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.PIMSPostSearchByAccountNumber);

            var searchTransactionAccountResponse = await this.SendPostRequest<SearchTransactionAccountInfoResponse>(
               requestUrl,
               piid,
               "SearchByAccountNumber",
               traceActivityId);

            return searchTransactionAccountResponse.Result.ToList();
        }

        public async Task<PaymentInstrument[]> ListPaymentInstrument(string accountId, ulong deviceId, string[] status, EventTraceActivity traceActivityId, IEnumerable<KeyValuePair<string, string>> queryParams = null, string partner = null, string country = null, string language = null, List<string> exposedFlightFeatures = null, string operation = null, PaymentExperienceSetting setting = null)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.ListPI, accountId, deviceId);
            requestUrl = AppendArrayQueryParam(requestUrl, "status", status);
            requestUrl = AppendQueryParams(requestUrl, queryParams);

            if (!string.IsNullOrEmpty(partner) && (queryParams == null || !queryParams.Any(x => string.Equals(x.Key, "partner", StringComparison.OrdinalIgnoreCase))))
            {
                requestUrl = AppendArrayQueryParam(requestUrl, "partner", new string[] { partner });
            }

            var additionalHeaders = new List<KeyValuePair<string, string>>();

            if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.GPayApayInstancePI, StringComparer.OrdinalIgnoreCase))
            {
                additionalHeaders.Add(new KeyValuePair<string, string>(GlobalConstants.HeaderValues.ExtendedFlightName, V7.Constants.FlightValues.ReturnCardWalletInstanceIdForPidlList));
            }

            IList<PaymentInstrument> paymentInstruments = await this.SendGetRequest<PaymentInstrument[]>(
                requestUrl,
                "ListPaymentInstrument",
                traceActivityId,
                exposedFlightFeatures: exposedFlightFeatures,
                additionalHeaders: additionalHeaders);

            List<PaymentInstrument> paymentInstrumentList = new List<PaymentInstrument>(paymentInstruments);

            paymentInstrumentList = paymentInstrumentList?.Where(
                pi => IsPaymentMethodAllowed(pi?.PaymentMethod, country, partner, exposedFlightFeatures, operation, setting)).ToList();

            paymentInstrumentList.ForEach(pi =>
            {
                V7.PIHelper.SetPendingIfPayPalMIB(pi, partner);
                PIHelper.OverridePMDisplayName(pi?.PaymentMethod, partner, language, country, setting: setting);
                PIHelper.AddDefaultDisplayName(pi, partner, traceActivityId, country: country, exposedFlightFeatures);
            });

            return paymentInstrumentList.ToArray();
        }

        public async Task<object> GetCardProfile(string accountId, string piid, ulong deviceId, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.CardProfile, accountId, piid, deviceId);
            return await this.SendGetRequest<object>(
                requestUrl,
                "GetCardProfile",
                traceActivityId);
        }

        public async Task<object> GetSeCardPersos(string accountId, string piid, ulong deviceId, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.SeCardPersos, accountId, piid, deviceId);
            return await this.SendGetRequest<object>(
                requestUrl,
                "GetSeCardPersos",
                traceActivityId);
        }

        public async Task<PaymentInstrument> PostPaymentInstrument(string accountId, object postPiData, EventTraceActivity traceActivityId, IEnumerable<KeyValuePair<string, string>> queryParams = null, IList<KeyValuePair<string, string>> additionalHeaders = null, string partner = null, List<string> exposedFlightFeatures = null)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.PostPI, accountId);
            requestUrl = AppendQueryParams(requestUrl, queryParams);

            var pi = await this.SendPostRequest<PaymentInstrument>(
                requestUrl,
                postPiData,
                "PostPaymentInstrument",
                traceActivityId,
                additionalHeaders,
                exposedFlightFeatures: exposedFlightFeatures);

            PIHelper.AddDefaultDisplayName(pi, partner, traceActivityId);

            V7.PIHelper.SetPendingIfPayPalMIB(pi, partner);
            return pi;
        }

        public async Task<PaymentInstrument> PostPaymentInstrument(object postPiData, EventTraceActivity traceActivityId, IEnumerable<KeyValuePair<string, string>> queryParams = null, IList<KeyValuePair<string, string>> additionalHeaders = null, string partner = null, List<string> exposedFlightFeatures = null)
        {
            string requestUrl = V7.Constants.UriTemplate.PostPIForPaymentAccountId;
            requestUrl = AppendQueryParams(requestUrl, queryParams);

            var pi = await this.SendPostRequest<PaymentInstrument>(
                requestUrl,
                postPiData,
                "PostPaymentInstrument",
                traceActivityId,
                additionalHeaders,
                exposedFlightFeatures: exposedFlightFeatures);

            PIHelper.AddDefaultDisplayName(pi, partner, traceActivityId);

            return pi;
        }

        public async Task<PaymentInstrument> UpdatePaymentInstrument(
            string accountId,
            string piid,
            object updatePiData,
            EventTraceActivity traceActivityId,
            string partner = null,
            List<string> exposedFlightFeatures = null,
            IEnumerable<KeyValuePair<string, string>> queryParams = null)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.UpdatePI, accountId, piid);
            requestUrl = AppendQueryParams(requestUrl, queryParams);
            var pi = await this.SendPostRequest<PaymentInstrument>(
                requestUrl,
                updatePiData,
                "UpdatePaymentInstrument",
                traceActivityId,
                exposedFlightFeatures: exposedFlightFeatures);
            PIHelper.AddDefaultDisplayName(pi, partner, traceActivityId);
            return pi;
        }

        public async Task RemovePaymentInstrument(string accountId, string piid, object removeReason, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.RemovePI, accountId, piid);

            await this.SendPostRequest<object>(
                requestUrl,
                removeReason,
                "RemovePaymentInstrument",
                traceActivityId);
        }

        public async Task<PaymentInstrument> ResumePendingOperation(string accountId, string piid, object pendingOpRequestData, EventTraceActivity traceActivityId, IEnumerable<KeyValuePair<string, string>> queryParams = null)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.PiPendingOperationsResume, accountId, piid);
            requestUrl = AppendQueryParams(requestUrl, queryParams);

            return await this.SendPostRequest<PaymentInstrument>(
                requestUrl,
                pendingOpRequestData,
                "ResumePendingOperation",
                traceActivityId);
        }

        public async Task<PaymentInstrument> ValidatePicv(string accountId, string piid, object requestData, EventTraceActivity traceActivityId, IEnumerable<KeyValuePair<string, string>> queryParams = null)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.ValidatePicv, accountId, piid);
            requestUrl = AppendQueryParams(requestUrl, queryParams);

            return await this.SendPostRequest<PaymentInstrument>(
                requestUrl,
                requestData,
                "ValidatePicv",
                traceActivityId);
        }

        public async Task<object> ReplenishTransactionCredentials(string accountId, string piid, ulong deviceId, object requestData, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.ReplenishTransactionCredentials, accountId, piid, deviceId);

            return await this.SendPostRequest<object>(
                requestUrl,
                requestData,
                "ReplenishTransactionCredentials",
                traceActivityId);
        }

        public async Task<object> AcquireLUKs(string accountId, string piid, ulong deviceId, object requestData, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.AcquireLUKs, accountId, piid, deviceId);

            return await this.SendPostRequest<object>(
                requestUrl,
                requestData,
                "AcquireLUKs",
                traceActivityId);
        }

        public async Task<object> ConfirmLUKs(string accountId, string piid, ulong deviceId, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.ConfirmLUKs, accountId, piid, deviceId);

            return await this.SendPostRequest<object>(
                requestUrl,
                null,
                "ConfirmLUKs",
                traceActivityId);
        }

        public async Task<object> ValidateCvv(string accountId, string piid, object requestData, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.ValidateCvv, accountId, piid);

            return await this.SendPostRequest<object>(
                requestUrl,
                requestData,
                "ValidateCvv",
                traceActivityId);
        }

        public async Task LinkSession(string accountId, string piid, LinkSession linkSession, EventTraceActivity traceActivityId, IEnumerable<KeyValuePair<string, string>> queryParams = null)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.LinkSession, accountId, piid);
            requestUrl = AppendQueryParams(requestUrl, queryParams);

            await this.SendPostRequest<object>(
                requestUrl,
                linkSession,
                "LinkSession",
                traceActivityId);
        }

        public async Task<ValidatePaymentInstrumentResponse> ValidatePaymentInstrument(string accountId, string piid, ValidatePaymentInstrument validatePaymentInstrument, EventTraceActivity traceActivityId, IEnumerable<KeyValuePair<string, string>> queryParams = null)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.Validate, accountId, piid);
            requestUrl = AppendQueryParams(requestUrl, queryParams);

            return await this.SendPostRequest<ValidatePaymentInstrumentResponse>(
                requestUrl,
                validatePaymentInstrument,
                "Validate",
                traceActivityId);
        }

        public async Task<List<PaymentMethod>> GetThirdPartyPaymentMethods(string provider, string sellerCountry, string buyerCountry, EventTraceActivity traceActivityId, string partner = null, string language = null, IList<KeyValuePair<string, string>> additionalHeaders = null, List<string> exposedFlightFeatures = null)
        {
            List<PaymentMethod> paymentMethods = new List<PaymentMethod>();

            string requestUrl = string.Format(V7.Constants.UriTemplate.GetThirdPartyPaymentMethods, provider, sellerCountry, buyerCountry);

            paymentMethods = await this.SendGetRequest<List<PaymentMethod>>(
                requestUrl,
                "GetThirdPartyPaymentMethods",
                traceActivityId,
                additionalHeaders,
                exposedFlightFeatures: exposedFlightFeatures) ?? paymentMethods;

            return paymentMethods;
        }

        public async Task<PaymentInstrument[]> ListUserAndTenantPaymentInstrument(ulong deviceId, string[] status, EventTraceActivity traceActivityId, IEnumerable<KeyValuePair<string, string>> queryParams = null, string partner = null, string country = null, string language = null, List<string> exposedFlightFeatures = null, string operation = null, PaymentExperienceSetting setting = null)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.ListEmpOrgPI, deviceId);
            requestUrl = AppendArrayQueryParam(requestUrl, "status", status);
            requestUrl = AppendQueryParams(requestUrl, queryParams);

            if (!string.IsNullOrEmpty(partner) && (queryParams == null || !queryParams.Any(x => string.Equals(x.Key, "partner", StringComparison.OrdinalIgnoreCase))))
            {
                requestUrl = AppendArrayQueryParam(requestUrl, "partner", new string[] { partner });
            }

            IList<PaymentInstrument> paymentInstruments = await this.SendGetRequest<PaymentInstrument[]>(
                requestUrl,
                "ListUserAndTenantPaymentInstrument",
                traceActivityId,
                exposedFlightFeatures: exposedFlightFeatures);
            List<PaymentInstrument> paymentInstrumentList = new List<PaymentInstrument>(paymentInstruments);

            paymentInstrumentList = paymentInstrumentList?.Where(
                pi => IsPaymentMethodAllowed(pi?.PaymentMethod, country, partner, exposedFlightFeatures, operation, setting)).ToList();

            paymentInstrumentList.ForEach(pi =>
            {
                V7.PIHelper.SetPendingIfPayPalMIB(pi, partner);
                PIHelper.OverridePMDisplayName(pi?.PaymentMethod, partner, language, setting: setting);
                PIHelper.AddDefaultDisplayName(pi, partner, traceActivityId, country: country, exposedFlightFeatures);
            });

            return paymentInstrumentList.ToArray();
        }

        private static string AppendQueryParams(string requestUrl, IEnumerable<KeyValuePair<string, string>> queryParams)
        {
            HashSet<string> paramsToUrlEncode = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            paramsToUrlEncode.Add(GlobalConstants.QueryParamNames.BillableAccountId);

            if (queryParams != null && queryParams.Any())
            {
                string queryString = string.Join(
                    "&",
                    queryParams.Select((kvp) =>
                    {
                        string paramValue = paramsToUrlEncode.Contains(kvp.Key) ? WebUtility.UrlEncode(kvp.Value) : kvp.Value;
                        return string.Format("{0}={1}", kvp.Key, paramValue);
                    }));

                char separator = requestUrl.Contains("?") ? '&' : '?';
                requestUrl = string.Format("{0}{1}{2}", requestUrl, separator, queryString);
            }

            return requestUrl;
        }

        private static void AddHeaders(HttpRequestMessage request, IList<KeyValuePair<string, string>> headers)
        {
            if (headers != null && request != null)
            {
                foreach (var header in headers)
                {
                    if (string.Equals(header.Key, GlobalConstants.HeaderValues.ExtendedFlightName, StringComparison.OrdinalIgnoreCase))
                    {
                        // Adding duplicate HTTP headers is concatinating values with a comma and a space.  Downstream PIMS service is not parsing 
                        // the additonal space to identify the flights.
                        string existingFlightValue = request.GetRequestHeader(GlobalConstants.HeaderValues.ExtendedFlightName);
                        string additionalFlightValue = header.Value;
                        string newFlightValue = GetNewFlightValue(existingFlightValue, additionalFlightValue);

                        if (!string.IsNullOrWhiteSpace(newFlightValue))
                        {
                            request.Headers.Remove(header.Key);
                            request.Headers.Add(header.Key, newFlightValue);
                        }
                    }
                    else
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }
            }
        }

        private static string GetNewFlightValue(string existingFlightValue, string additionalFlightValue)
        {
            string newFlightValue = null;

            if (string.IsNullOrWhiteSpace(existingFlightValue))
            {
                if (!string.IsNullOrWhiteSpace(additionalFlightValue))
                {
                    newFlightValue = additionalFlightValue;
                }
            }
            else if (string.IsNullOrWhiteSpace(additionalFlightValue))
            {
                newFlightValue = existingFlightValue;
            }
            else
            {
                newFlightValue = string.Join(",", existingFlightValue, additionalFlightValue);
            }

            return newFlightValue;
        }

        private static string AppendArrayQueryParam(string url, string paramName, string[] paramValues)
        {
            bool firstParam = false;
            if (!url.Contains("?"))
            {
                firstParam = true;
            }

            // PIMS supports "name=value0&name=value1.." format
            // We cannot use UriBuilder class and its Query NameValueCollection to build this url because
            // the Query NameValueCollection handles multiple name-values with the same name by appending values
            // in the "name=value0,value1,.." form which is not supported by PIMS.
            // Also,"name[0]=value0&name[1]=value1.." is not suppported by PIMS.
            StringBuilder urlBuilder = new StringBuilder(url);
            for (int i = 0; i < paramValues.Length; i++)
            {
                if (firstParam)
                {
                    firstParam = false;
                    urlBuilder.Append(string.Format("?{0}={1}", paramName, paramValues[i]));
                }
                else
                {
                    urlBuilder.Append(string.Format("&{0}={1}", paramName, paramValues[i]));
                }
            }

            return urlBuilder.ToString();
        }

        private static bool IsPaymentMethodAllowed(PaymentMethod pm, string country, string partner, List<string> flights = null, string operation = null, PaymentExperienceSetting setting = null)
        {
            var commercialPartners = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                V7.Constants.PartnerName.AppSource,
                V7.Constants.PartnerName.Azure,
                V7.Constants.PartnerName.AzureSignup,
                V7.Constants.PartnerName.AzureIbiza,
                V7.Constants.PartnerName.Bing,
                V7.Constants.PartnerName.CommercialStores,
                V7.Constants.PartnerName.CommercialWebblends
            };

            // All commercial and legacy consumer partners
            var chinaVisaMCAllowedPartners = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                V7.Constants.PartnerName.AmcWeb,
                V7.Constants.PartnerName.AppSource,
                V7.Constants.PartnerName.Azure,
                V7.Constants.PartnerName.AzureSignup,
                V7.Constants.PartnerName.AzureIbiza,
                V7.Constants.PartnerName.Bing,
                V7.Constants.PartnerName.CommercialStores,
                V7.Constants.PartnerName.ConsumerSupport,
                V7.Constants.PartnerName.GGPDEDS,
                V7.Constants.PartnerName.OneDrive,
                V7.Constants.PartnerName.Payin,
                V7.Constants.PartnerName.SetupOffice,
                V7.Constants.PartnerName.SetupOfficeSdx,
                V7.Constants.PartnerName.XboxWeb,
                V7.Constants.PartnerName.NorthStarWeb,
                V7.Constants.PartnerName.Storify,
                V7.Constants.PartnerName.XboxSettings,
                V7.Constants.PartnerName.XboxSubs,
                V7.Constants.PartnerName.Saturn,
                V7.Constants.TemplateName.DefaultTemplate,
            };

            var partnersToDisableSepaNLAddSelect = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                V7.Constants.PartnerName.SetupOffice,
                V7.Constants.PartnerName.SetupOfficeSdx
            };

            if (pm == null)
            {
                return false;
            }

            if (string.Equals(partner, V7.Constants.PartnerName.Wallet, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // TODO: once amcweb is ready for market expansion, remove the logic here.
            if ((flights?.Contains(Flighting.Features.PXEnableGooglePayApplePayOnlyInUS, StringComparer.OrdinalIgnoreCase) ?? false)
                && !string.Equals(country, GlobalConstants.CountryCodes.US, StringComparison.OrdinalIgnoreCase)
                && (pm.IsGooglePay() || pm.IsApplePay()))
            {
                return false;
            }

            if (pm.IsVirtualLegacyInvoice()
                && !(PartnerHelper.IsAzurePartner(partner)
                    || PidlFactory.V7.PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PidlFactory.V7.PartnerSettingsHelper.Features.EnableVirtualFamilyPM, country, setting)))
            {
                return false;
            }

            if (pm.IsCreditCardAmex())
            {
                if (string.Equals(country, GlobalConstants.CountryCodes.IN, StringComparison.OrdinalIgnoreCase) &&
                    commercialPartners.Contains(partner) &&
                    !(flights?.Contains(Flighting.Features.PXEnableAmexForIN) ?? false))
                {
                    return false;
                }
            }

            if (string.Equals(country, GlobalConstants.CountryCodes.CN, StringComparison.OrdinalIgnoreCase)
                && (pm.IsCreditCardVisa() || pm.IsCreditCardMasterCard()))
            {
                // VISA and MC need to be allowed in China only for Commercial and Legacy Consumer partners only
                if (chinaVisaMCAllowedPartners.Contains(partner)
                    || (setting != null && PidlFactory.V7.PartnerSettingsHelper.IsFeatureEnabledUsingPartnerSettings(PidlFactory.V7.PartnerSettingsHelper.Features.ChinaAllowVisaMasterCard, country, setting)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            // Remove Venmo from allowedPaymentMethods if the flight is not turned on
            // TODO: When cleaning up PxEnableVenmo flight, remove both if statements (isVenmo and flighting if statement) 
            if (pm.IsVenmo())
            {
                if (!flights?.Contains(V7.Constants.PartnerFlightValues.PxEnableVenmo, StringComparer.OrdinalIgnoreCase) ?? false)
                {
                    return false;
                }

                if (string.Equals(operation, V7.Constants.Operations.Select, StringComparison.OrdinalIgnoreCase) || string.Equals(operation, V7.Constants.Operations.Add, StringComparison.OrdinalIgnoreCase))
                {
                    // Both flights need to be on for Select PM or Add PI flow to show
                    if (!flights?.Contains(V7.Constants.PartnerFlightValues.PxEnableSelectPMAddPIVenmo, StringComparer.OrdinalIgnoreCase) ?? false)
                    {
                        return false;
                    }
                }
            }

            // Remove upi from allowedPaymentMethods if the flight is not turned on
            // TODO: When cleaning up PxEnableUpi and IndiaUPIEnable flight, remove both if statements of flights and keep operation check.
            if (pm.IsUpi())
            {
                // overall UPI controlling flight for all operations based on Partner, it should be 0 or 100%.
                if (!flights?.Contains(V7.Constants.PartnerFlightValues.PxEnableUpi, StringComparer.OrdinalIgnoreCase) ?? false)
                {
                    return false;
                }

                if (string.Equals(operation, V7.Constants.Operations.Select, StringComparison.OrdinalIgnoreCase))
                {
                    // Both flights need to be on for Select PM flow to show.
                    if (!flights?.Contains(V7.Constants.PartnerFlightValues.IndiaUPIEnable, StringComparer.OrdinalIgnoreCase) ?? false)
                    {
                        return false;
                    }
                }
            }

            if (pm.IsUpiCommercial())
            {
                // overall UPI controlling flight for all operations based on Partner, it should be 0 or 100%.
                if (!flights?.Contains(V7.Constants.PartnerFlightValues.PXCommercialEnableUpi, StringComparer.OrdinalIgnoreCase) ?? false)
                {
                    return false;
                }

                if (string.Equals(operation, V7.Constants.Operations.Select, StringComparison.OrdinalIgnoreCase))
                {
                    // Both flights need to be on for Select PM flow to show.
                    if (!flights?.Contains(V7.Constants.PartnerFlightValues.IndiaUPIEnable, StringComparer.OrdinalIgnoreCase) ?? false)
                    {
                        return false;
                    }
                }
            }

            // TODO: When cleaning up EnableGlobalUpiQr flight, both if statements of flights and keep operation check.
            if (pm.IsUpiQr())
            {
                if (string.Equals(operation, V7.Constants.Operations.Select, StringComparison.OrdinalIgnoreCase))
                {
                    // Flight helps to enable selct PM without code change.
                    if (flights?.Contains(V7.Constants.PartnerFlightValues.EnableSelectUpiQr, StringComparer.OrdinalIgnoreCase) ?? false)
                    {
                        return true;
                    }

                    return false;
                }

                // Allow only select instance or select single instance for UPIQR.
                if (string.Equals(operation, V7.Constants.Operations.SelectInstance, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(operation, V7.Constants.Operations.SelectSingleInstance, StringComparison.OrdinalIgnoreCase))
                {
                    // TODO: When cleaning up remove only below if statment for EnableGlobalUpiQr flight.
                    if (!flights?.Contains(V7.Constants.PartnerFlightValues.EnableGlobalUpiQr, StringComparer.OrdinalIgnoreCase) ?? false)
                    {
                        return false;
                    }
                }
            }

            // TODO: When cleaning up EnableCommercialGlobalUpiQr flight, both if statements of flights and keep operation check.
            if (pm.IsUpiQrCommercial())
            {
                if (string.Equals(operation, V7.Constants.Operations.Select, StringComparison.OrdinalIgnoreCase))
                {
                    // Flight helps to enable selct PM without code change.
                    if (flights?.Contains(V7.Constants.PartnerFlightValues.EnableCommercialSelectUpiQr, StringComparer.OrdinalIgnoreCase) ?? false)
                    {
                        return true;
                    }

                    return false;
                }

                // Allow only select instance or select single instance for UPIQR.
                if (string.Equals(operation, V7.Constants.Operations.SelectInstance, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(operation, V7.Constants.Operations.SelectSingleInstance, StringComparison.OrdinalIgnoreCase))
                {
                    // TODO: When cleaning up remove only below if statment for EnableCommercialGlobalUpiQr flight.
                    if (!flights?.Contains(V7.Constants.PartnerFlightValues.EnableCommercialGlobalUpiQr, StringComparer.OrdinalIgnoreCase) ?? false)
                    {
                        return false;
                    }
                }
            }

            if (pm.IsCreditCardRupay())
            {
                // overall Rupay controlling flight for all operations based on Partner, it should be 0 or 100%.
                if (string.Equals(country, GlobalConstants.CountryCodes.IN, StringComparison.OrdinalIgnoreCase) &&
                    !(flights?.Contains(V7.Constants.PartnerFlightValues.PXEnableRupayForIN, StringComparer.OrdinalIgnoreCase) ?? false))
                {
                    return false;
                }

                if (string.Equals(operation, V7.Constants.Operations.Select, StringComparison.OrdinalIgnoreCase))
                {
                    // Both flights need to be on for Select PM flow to show.
                    if (!flights?.Contains(V7.Constants.PartnerFlightValues.IndiaRupayEnable, StringComparer.OrdinalIgnoreCase) ?? false)
                    {
                        return false;
                    }
                }
            }

            if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner)
                && string.Equals(operation, V7.Constants.Operations.Select, StringComparison.OrdinalIgnoreCase))
            {
                if (flights?.Contains(V7.Constants.PartnerFlightValues.EnablePaymentMethodGrouping, StringComparer.OrdinalIgnoreCase) ?? false)
                {
                    return !pm.IsInvoiceCreditKlarna();
                }
                else
                {
                    return !(pm.IsOnlineBankTransferPaySafe() || pm.IsInvoiceCreditKlarna());
                }
            }

            if (PXCommon.Constants.PartnerGroups.IsXboxNativePartner(partner)
                && string.Equals(operation, V7.Constants.Operations.SelectInstance, StringComparison.OrdinalIgnoreCase)
                && (flights?.Contains(V7.Constants.PartnerFlightValues.EnablePaymentMethodGrouping, StringComparer.OrdinalIgnoreCase) ?? false))
            {
                return !pm.IsOnlineBankTransferPaySafe();
            }

            if (pm.IsDirectDebitSepa())
            {
                if (flights?.Contains(V7.Constants.PartnerFlightValues.EnableNewLogoSepa, StringComparer.OrdinalIgnoreCase) ?? false)
                {
                    UpdateSepaLogo(pm);
                }

                if (string.Equals(country, GlobalConstants.CountryCodes.NL, StringComparison.OrdinalIgnoreCase)
                    && (string.Equals(operation, V7.Constants.Operations.Add, StringComparison.OrdinalIgnoreCase) || string.Equals(operation, V7.Constants.Operations.Select, StringComparison.OrdinalIgnoreCase))
                    && partnersToDisableSepaNLAddSelect.Contains(partner, StringComparer.OrdinalIgnoreCase))
                {
                    return false;
                }
                else if ((string.Equals(country, GlobalConstants.CountryCodes.DE, StringComparison.OrdinalIgnoreCase) || string.Equals(country, GlobalConstants.CountryCodes.AT, StringComparison.OrdinalIgnoreCase))
                    && (flights?.Contains(Flighting.Features.PXDisableSepaATDE, StringComparer.OrdinalIgnoreCase) ?? false))
                {
                    return false;
                }
                else if (!(string.Equals(country, GlobalConstants.CountryCodes.DE, StringComparison.OrdinalIgnoreCase) || string.Equals(country, GlobalConstants.CountryCodes.AT, StringComparison.OrdinalIgnoreCase))
                    && (flights?.Contains(Flighting.Features.PXDisableSepaNonATDE, StringComparer.OrdinalIgnoreCase) ?? false))
                {
                    return false;
                }
            }

            if (pm.IsAch()
                && (flights?.Contains(Flighting.Features.PXDisableAch, StringComparer.OrdinalIgnoreCase) ?? false))
            {
                return false;
            }

            if (pm.IsPaymentMethodType(V7.Constants.PaymentMethodFamily.credit_card.ToString(), V7.Constants.PaymentMethodType.UnionpayInternational))
            {
                return IsPXEnableFlight(Flighting.Features.PXEnableCUPInternational, flights);
            }

            if (string.Equals(operation, V7.Constants.Operations.Select, StringComparison.OrdinalIgnoreCase))
            {
                if (pm.IsPaymentMethodType(V7.Constants.PaymentMethodFamily.ewallet.ToString(), V7.Constants.PaymentMethodType.AlipayCN))
                {
                    return IsPXEnableFlight(Flighting.Features.PXEnableAlipayCN, flights);
                }

                if (pm.IsPaymentMethodType(V7.Constants.PaymentMethodFamily.ewallet.ToString(), V7.Constants.PaymentMethodType.PayPay))
                {
                    return IsPXEnableFlight(Flighting.Features.PXEnablePayPay, flights);
                }

                if (pm.IsPaymentMethodType(V7.Constants.PaymentMethodFamily.ewallet.ToString(), V7.Constants.PaymentMethodType.AlipayHK))
                {
                    return IsPXEnableFlight(Flighting.Features.PXEnableAlipayHK, flights);
                }

                if (pm.IsPaymentMethodType(V7.Constants.PaymentMethodFamily.ewallet.ToString(), V7.Constants.PaymentMethodType.GCash))
                {
                    return IsPXEnableFlight(Flighting.Features.PXEnableGCash, flights);
                }

                if (pm.IsPaymentMethodType(V7.Constants.PaymentMethodFamily.ewallet.ToString(), V7.Constants.PaymentMethodType.TrueMoney))
                {
                    return IsPXEnableFlight(Flighting.Features.PXEnableTrueMoney, flights);
                }

                if (pm.IsPaymentMethodType(V7.Constants.PaymentMethodFamily.ewallet.ToString(), V7.Constants.PaymentMethodType.TouchNGo))
                {
                    return IsPXEnableFlight(Flighting.Features.PXEnableTouchNGo, flights);
                }
            }

            // None of the blocking filters caught this PM above.  So, it is allowed
            return true;
        }

        private static string GetCachingKey(string requestUrl, HttpRequestMessage request)
        {
            string flightHeader = HttpRequestHelper.GetRequestHeader(GlobalConstants.HeaderValues.ExtendedFlightName, request) ?? string.Empty;
            string testHeader = HttpRequestHelper.GetRequestHeader(PaymentConstants.PaymentExtendedHttpHeaders.TestHeader, request) ?? string.Empty;
            string customerHeader = HttpRequestHelper.GetRequestHeader(GlobalConstants.HeaderValues.CustomerHeader, request) ?? string.Empty;
            return string.Format("RequestUrl:{0},FlightHeader:{1},TestHeader:{2},Customer:{3}", requestUrl, flightHeader, testHeader, customerHeader).ToLower();
        }

        private static void UpdateSepaLogo(PaymentMethod pm)
        {
            // update the single logo URL
            var logo = pm.Display?.Logo;
            if (!string.IsNullOrEmpty(logo))
            {
                var uri = new Uri(logo);
                var newLogo = logo.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)
                    ? "logo_sepa_v2.svg"
                    : "logo_sepa_v2.png";

                pm.Display.Logo = logo.Replace(Path.GetFileName(uri.LocalPath), newLogo);
            }

            // update the list of logos
            if (pm.Display?.Logos != null)
            {
                foreach (var logoItem in pm.Display.Logos)
                {
                    if (logoItem != null)
                    {
                        if (!string.IsNullOrEmpty(logoItem.Url))
                        {
                            var uri = new Uri(logoItem.Url);
                            string newFileName = string.Empty;

                            if (string.Equals(logoItem.MimeType, "image/svg+xml", StringComparison.OrdinalIgnoreCase))
                            {
                                newFileName = "logo_sepa_v2.svg";
                            }
                            else
                            {
                                newFileName = "logo_sepa_v2.png";
                            }

                            logoItem.Url = logoItem.Url.Replace(Path.GetFileName(uri.LocalPath), newFileName);
                        }
                    }
                }
            }
        }

        private static bool IsPXEnablePIMSGetPaymentMethodsCache(string actionName, List<string> exposedFlightFeatures = null)
        {
            return exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXEnablePIMSGetPaymentMethodsCache, StringComparer.OrdinalIgnoreCase)
                     && string.Equals(actionName, APINames.GetPaymentMethods, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsPXEnableFlight(string flightName, List<string> exposedFlightFeatures = null)
        {
            return exposedFlightFeatures != null && exposedFlightFeatures.Contains(flightName, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Replaces the value of partner param in url if present with the mapping name.
        /// </summary>
        /// <param name="url">Input URL</param>
        /// <returns>Returns url with updated partner if found else original url</returns>
        private string UpdatePartnerInURLIfPresent(string url)
        {
            Regex regex = new Regex(@"partner=([^&]*)", RegexOptions.IgnoreCase);
            Match match = regex.Match(url);

            if (match.Success)
            {
                string currentPartnerName = match.Groups[1].Value;

                if (!string.IsNullOrEmpty(currentPartnerName) && this.partnerNamesMappingForPimsRequests.ContainsKey(currentPartnerName))
                {
                    string updatedUrl = regex.Replace(url, "partner=" + this.partnerNamesMappingForPimsRequests[currentPartnerName]);

                    return updatedUrl;
                }
            }

            return url;
        }

        private async Task<T> SendGetRequest<T>(string requestUrl, string actionName, EventTraceActivity traceActivityId, IList<KeyValuePair<string, string>> additionalHeaders = null, List<string> exposedFlightFeatures = null)
        {
            string fullRequestUrl = string.Format("{0}/{1}", this.BaseUrl, requestUrl);
            if (IsPXEnableFlight(Flighting.Features.PXEnablePIMSPPEEnvironment, exposedFlightFeatures))
            {
                fullRequestUrl = string.Format("{0}/{1}", this.servicePPEBaseUrl, requestUrl);
            }

            if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(PidlFactory.V7.PartnerSettingsHelper.Features.ChangePartnerNameForPims, StringComparer.OrdinalIgnoreCase))
            {
                fullRequestUrl = this.UpdatePartnerInURLIfPresent(fullRequestUrl);
            }

            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, fullRequestUrl))
            {
                request.IncrementCorrelationVector(traceActivityId);
                request.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString());
                request.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.ApiVersion, this.apiVersion);

                HttpRequestHelper.TransferTargetHeadersFromIncomingRequestToOutgoingRequest(this.passThroughHeaders, request);

                // Add action name to the request properties so that this request's OperationName is logged properly
                request.AddOrReplaceActionName(actionName);
                AddHeaders(request, additionalHeaders);

                // Get payment methods from PX caching.
                if (IsPXEnablePIMSGetPaymentMethodsCache(actionName, exposedFlightFeatures))
                {
                    try
                    {
                        string cacheKey = GetCachingKey(requestUrl, request);
                        if (this.pimsPaymentMethodsCache.TryGetValue(cacheKey, out Tuple<List<PaymentMethod>, DateTime> cachedResponse)
                            && (DateTime.UtcNow.Subtract(cachedResponse.Item2).TotalSeconds < PimsPaymentMethodsRefreshInternvalInSec))
                        {
                            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(cachedResponse.Item1));
                        }
                    }
                    catch (Exception ex)
                    {
                        SllWebLogger.TracePXServiceException(string.Format("API:{0},ReadException:{1}", actionName, ex), traceActivityId);
                    }
                }

                using (HttpResponseMessage response = await this.pimsHttpClient.SendAsync(request))
                {
                    string responseMessage = await response.Content.ReadAsStringAsync();

                    SllWebLogger.TraceServerMessage("SendGetRequest_PIMSAccessor", traceActivityId.ToString(), null, response.IsSuccessStatusCode ? JsonConvert.DeserializeObject<T>(responseMessage).ToString() : JsonConvert.DeserializeObject(responseMessage).ToString(), EventLevel.Informational);

                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            // Adding payment methods result to PX caching.
                            if (IsPXEnablePIMSGetPaymentMethodsCache(actionName, exposedFlightFeatures))
                            {
                                var requestResult = JsonConvert.DeserializeObject<T>(responseMessage);

                                try
                                {
                                    string cacheKey = GetCachingKey(requestUrl, request);
                                    lock (this.pimsPaymentMethodsLockObj)
                                    {
                                        if (this.pimsPaymentMethodsCache.Count >= PimsPaymentMethodsCacheMaxLimit)
                                        {
                                            List<string> expiredKeys = this.pimsPaymentMethodsCache.Where(x => DateTime.UtcNow.Subtract(x.Value.Item2).TotalSeconds >= PimsPaymentMethodsRefreshInternvalInSec).Select(x => x.Key).ToList();
                                            foreach (var expiredKey in expiredKeys)
                                            {
                                                this.pimsPaymentMethodsCache.Remove(expiredKey);
                                            }
                                        }

                                        if (this.pimsPaymentMethodsCache.Count < PimsPaymentMethodsCacheMaxLimit)
                                        {
                                            this.pimsPaymentMethodsCache[cacheKey] = new Tuple<List<PaymentMethod>, DateTime>(requestResult as List<PaymentMethod>, DateTime.UtcNow);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    SllWebLogger.TracePXServiceException(string.Format("API:{0},AddRemvoeException:{1}", actionName, ex), traceActivityId);
                                }

                                return requestResult;
                            }
                            else
                            {
                                return JsonConvert.DeserializeObject<T>(responseMessage);
                            }
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Failed to deserialize success response from PIMS"));
                        }
                    }
                    else
                    {
                        ServiceErrorResponse error = null;
                        try
                        {
                            ServiceErrorResponse innerError = JsonConvert.DeserializeObject<ServiceErrorResponse>(responseMessage);
                            innerError.Source = string.IsNullOrWhiteSpace(innerError.Source) ? PimsServiceName : innerError.Source;
                            error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Failed to deserialize error response from PIMS"));
                        }

                        // If the country is not supported, we should bypass the exception handling policy to override the internal server error of PX and return the PIMS error.
                        if (string.Equals(error.InnerError?.Message, PaymentErrorMessages.CountryNotSupported, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(error.InnerError?.Target, QueryParamNames.Country, StringComparison.OrdinalIgnoreCase))
                        {
                            throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response, HandlingType = ExceptionHandlingPolicy.ByPass });
                        }
                        else
                        {
                            throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response });
                        }
                    }
                }
            }
        }

        private async Task<T> SendPostRequest<T>(string url, object request, string actionName, EventTraceActivity traceActivityId, IList<KeyValuePair<string, string>> additionalHeaders = null, List<string> exposedFlightFeatures = null)
        {
            string fullRequestUrl = string.Format("{0}/{1}", this.BaseUrl, url);

            if (IsPXEnableFlight(Flighting.Features.PXEnablePIMSPPEEnvironment, exposedFlightFeatures))
            {
                fullRequestUrl = string.Format("{0}/{1}", this.servicePPEBaseUrl, url);
            }

            if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(PidlFactory.V7.PartnerSettingsHelper.Features.ChangePartnerNameForPims, StringComparer.OrdinalIgnoreCase))
            {
                fullRequestUrl = this.UpdatePartnerInURLIfPresent(fullRequestUrl);
            }

            using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, fullRequestUrl))
            {
                requestMessage.IncrementCorrelationVector(traceActivityId);
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString());
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, Guid.NewGuid().ToString());
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.ApiVersion, this.apiVersion);

                // Add action name to the request properties so that this request's OperationName is logged properly
                requestMessage.AddOrReplaceActionName(actionName);

                HttpRequestHelper.TransferTargetHeadersFromIncomingRequestToOutgoingRequest(this.passThroughHeaders, requestMessage);

                AddHeaders(requestMessage, additionalHeaders);

                if (request != null)
                {
                    requestMessage.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType); // lgtm[cs/sensitive-data-transmission] lgtm[cs/web/xss] The request is being made to a web service and not to a web page.
                }

                using (HttpResponseMessage response = await this.pimsHttpClient.SendAsync(requestMessage))
                {
                    string responseMessage = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            return JsonConvert.DeserializeObject<T>(responseMessage);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Failed to deserialize success response from PIMS"));
                        }
                    }
                    else
                    {
                        ServiceErrorResponse error = null;
                        try
                        {
                            ServiceErrorResponse innerError = JsonConvert.DeserializeObject<ServiceErrorResponse>(responseMessage);
                            innerError.Source = string.IsNullOrWhiteSpace(innerError.Source) ? PimsServiceName : innerError.Source;
                            error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Failed to deserialize error response from PIMS"));
                        }

                        throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response });
                    }
                }
            }
        }
    }
}
