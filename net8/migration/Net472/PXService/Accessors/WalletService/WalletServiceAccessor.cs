// <copyright file="WalletServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Model.WalletService;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model;
    using Microsoft.Commerce.Tracing;
    using Microsoft.CTP.CommerceAPI.Proxy.v201112;
    using Newtonsoft.Json;
    using static Microsoft.Commerce.Payments.PXService.V7.Constants;

    public class WalletServiceAccessor : IWalletServiceAccessor
    {
        public const string WalletServiceName = "WalletService";
        private readonly List<string> passThroughHeaders = new List<string> { PaymentConstants.PaymentExtendedHttpHeaders.TestHeader };

        private HttpClient walletHttpClient;
        private string serviceBaseUrl;
        private string apiVersion;
        private string emulatorBaseUrl;

        private ProviderDataResponse providerDataResponseCache;
        private DateTime providerDataResponseCacheLastUpdatedTime;

        // ProviderDataResponse refresh interval every 1 hours
        private int providerDataResponseCacheRefreshIntervalInSec = 3600;
        private object lockObj = new object();

        public WalletServiceAccessor(
            string serviceBaseUrl,
            string apiVersion,
            HttpMessageHandler messageHandler)
        {
            this.walletHttpClient = new PXTracingHttpClient(PXCommon.Constants.ServiceNames.WalletService, messageHandler, ApplicationInsightsProvider.LogOutgoingOperation);
            this.serviceBaseUrl = serviceBaseUrl;
            this.emulatorBaseUrl = "http://localhost/WalletEmulator";
            this.apiVersion = apiVersion;
            this.providerDataResponseCacheLastUpdatedTime = DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(this.providerDataResponseCacheRefreshIntervalInSec));
        }

        private string BaseUrl
        {
            get
            {
                if (HttpRequestHelper.IsPXTestRequest("px.wallet") && !string.IsNullOrWhiteSpace(this.emulatorBaseUrl))
                {
                    return this.emulatorBaseUrl;
                }
                else
                {
                    return this.serviceBaseUrl;
                }
            }
        }

        public async Task<ProviderDataResponse> GetProviderData(EventTraceActivity traceActivityId, List<string> exposedFlightFeatures)
        {
            string requestUrl = V7.Constants.UriTemplate.GetWalletConfig;

            if (exposedFlightFeatures.Contains(Flighting.Features.PXDisableGetWalletConfigCache))
            {
                return await this.SendGetRequest<ProviderDataResponse>(requestUrl, "GetWalletConfig", traceActivityId);
            }

            if (this.providerDataResponseCache != null &&
                    DateTime.UtcNow.Subtract(this.providerDataResponseCacheLastUpdatedTime).TotalSeconds <= this.providerDataResponseCacheRefreshIntervalInSec)
            {
                return this.providerDataResponseCache;
            }

            ProviderDataResponse response = await this.SendGetRequest<ProviderDataResponse>(requestUrl, "GetWalletConfig", traceActivityId);

            lock (this.lockObj)
            {
                this.providerDataResponseCache = response;
                this.providerDataResponseCacheLastUpdatedTime = DateTime.UtcNow;
            }

            return this.providerDataResponseCache;
        }

        public async Task<string> SetupProviderSession(SetupProviderSessionIncomingPayload providerSessionPayload, EventTraceActivity traceActivityId)
        {
            string requestUrl = V7.Constants.UriTemplate.SetupProviderSession;
            SetupProviderSessionOutgoingPayload requestPayload = new SetupProviderSessionOutgoingPayload
            {
                PiFamily = providerSessionPayload.PiFamily,
                PiType = providerSessionPayload.PiType,
                WalletSessionData = JsonConvert.SerializeObject(providerSessionPayload.WalletSessionData)
            };
            var response = await this.SendPostRequest<SessionDataResponse>(requestUrl, "SetupProviderSession", requestPayload, traceActivityId);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<ProvisionWalletTokenResponse> Provision(
            string sessionId,
            string accountId,
            ProvisionWalletTokenIncomingPayload providerSessionPayload,
            EventTraceActivity traceActivityId)
        {
            string requestUrl = V7.Constants.UriTemplate.ProvisionWalletToken;
            ProvisionWalletTokenRequest requestPayload = new ProvisionWalletTokenRequest
            {
                SessionId = sessionId,
                AccountId = accountId,
                Country = providerSessionPayload.SessionData.Country,
                PiFamily = providerSessionPayload.PiFamily,
                PiType = providerSessionPayload.PiType,
                IntegrationType = WalletServiceConstants.IntegrationType,
                TokenReference = providerSessionPayload.TokenReference,
                TransactionType = providerSessionPayload.SessionData.ChallengeScenario.ToString(),
                AuthorizationGroups = AdaptToWalletAuthorizationGroup(providerSessionPayload.SessionData.AuthorizationGroups),
            };

            var response = await this.SendPostRequest<ProvisionWalletTokenResponse>(requestUrl, "ProvisionWalletToken", requestPayload, traceActivityId);
            return response;
        }

        public async Task<ValidateDataResponse> Validate(
            string sessionId,
            string accountId,
            ValidateIncomingPayload validatePayload,
            EventTraceActivity traceActivityId)
        {
            string requestURL = V7.Constants.UriTemplate.WalletValidate;
            ValidateRequest requestPayload = new ValidateRequest
            {
                SessionId = sessionId,
                AccountId = accountId,
                Country = validatePayload.SessionData.Country,
                PiFamily = validatePayload.PiFamily,
                PiType = validatePayload.PiType,
                Partner = validatePayload.SessionData.Partner,
                Currency = validatePayload.SessionData.Currency,
                IsCommercialTransaction = false,
                UpdateValidation = true,
            };

            var response = await this.SendPostRequest<ValidateDataResponse>(requestURL, "ValidateData", requestPayload, traceActivityId);
            return response;
        }

        private static List<WalletAuthorizationGroup> AdaptToWalletAuthorizationGroup(List<AuthorizationGroup> authorizationGroups)
        {
            if (authorizationGroups == null)
            {
                return null;
            }

            var walletAuthorizationGroups = authorizationGroups.Select(x => new WalletAuthorizationGroup { Id = x.Id, TotalAmount = x.TotalAmount }).ToList();
            return walletAuthorizationGroups;
        }

        private async Task<T> SendGetRequest<T>(string requestUrl, string actionName, EventTraceActivity traceActivityId, IEnumerable<string> testHeaders = null)
        {
            string fullRequestUrl = string.Format("{0}/{1}", this.BaseUrl, requestUrl);
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, fullRequestUrl))
            {
                request.IncrementCorrelationVector(traceActivityId);
                request.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString());
                request.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.WalletServiceAPIVersion, this.apiVersion);
                HttpRequestHelper.TransferTargetHeadersFromIncomingRequestToOutgoingRequest(this.passThroughHeaders, request);

                if (testHeaders != null)
                {
                    request.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.TestHeader, testHeaders);
                }
                
                PaymentsEventSource.Log.PXServiceRequestToWalletService(fullRequestUrl, traceActivityId);
                request.AddOrReplaceActionName(actionName);
                using (HttpResponseMessage response = await this.walletHttpClient.SendAsync(request))
                {
                    string responseMessage = await response.Content.ReadAsStringAsync();
                    PaymentsEventSource.Log.PXServiceTraceResponseFromWalletService(response.StatusCode.ToString(), responseMessage, traceActivityId);
                    SllWebLogger.TraceServerMessage("SendGetRequest_WalletServiceAccessor", traceActivityId.ToString(), null, JsonConvert.DeserializeObject<T>(responseMessage).ToString(), Diagnostics.Tracing.EventLevel.Informational);

                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            return JsonConvert.DeserializeObject<T>(responseMessage);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Failed to deserialize success response from Wallet"));
                        }
                    }
                    else
                    {
                        ServiceErrorResponse error = null;
                        try
                        {
                            ServiceErrorResponse innerError = JsonConvert.DeserializeObject<ServiceErrorResponse>(responseMessage);
                            innerError.Source = string.IsNullOrWhiteSpace(innerError.Source) ? WalletServiceName : innerError.Source;
                            error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Failed to deserialize error response from Wallet"));
                        }

                        throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response });
                    }
                }
            }
        }

        private async Task<T> SendPostRequest<T>(string url, string actionName, object request, EventTraceActivity traceActivityId, IList<KeyValuePair<string, string>> additionalHeaders = null)
        {
            string fullRequestUrl = string.Format("{0}/{1}", this.BaseUrl, url);
            PaymentsEventSource.Log.WalletClientRequestToService(fullRequestUrl, traceActivityId);
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, fullRequestUrl))
            {
                requestMessage.IncrementCorrelationVector(traceActivityId);
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString());
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.WalletServiceAPIVersion, this.apiVersion);
                HttpRequestHelper.TransferTargetHeadersFromIncomingRequestToOutgoingRequest(this.passThroughHeaders, requestMessage);

                if (request != null)
                {
                    requestMessage.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType); // lgtm[cs/sensitive-data-transmission] lgtm[cs/web/xss] The request is being made to a web service and not to a web page.
                }

                requestMessage.AddOrReplaceActionName(actionName);
                using (HttpResponseMessage response = await this.walletHttpClient.SendAsync(requestMessage))
                {
                    string responseMessage = await response.Content.ReadAsStringAsync();
                    PaymentsEventSource.Log.PXServiceTraceResponseFromWalletService(response.StatusCode.ToString(), responseMessage, traceActivityId);
                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            return JsonConvert.DeserializeObject<T>(responseMessage);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Failed to deserialize success response from Wallet service"));
                        }
                    }
                    else
                    {
                        ServiceErrorResponse error = null;
                        try
                        {
                            ServiceErrorResponse innerError = JsonConvert.DeserializeObject<ServiceErrorResponse>(responseMessage);
                            innerError.Source = string.IsNullOrWhiteSpace(innerError.Source) ? WalletServiceName : innerError.Source;
                            error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Failed to deserialize error response from Wallet Service"));
                        }

                        throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response });
                    }
                }
            }
        }
    }
}