// <copyright file="PartnerSettingsServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.PartnerSettingsService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PartnerSettingsModel;
    using Microsoft.Commerce.Payments.PXCommon;
    using Newtonsoft.Json;

    public class PartnerSettingsServiceAccessor : IPartnerSettingsServiceAccessor
    {
        private const string DisableCache = "DisableCache";
        private readonly List<string> passThroughHeaders = new List<string> { PaymentConstants.PaymentExtendedHttpHeaders.TestHeader };
        private HttpClient partnerSettingsServiceHttpClient;

        private string serviceBaseUrl;
        private string servicePPEBaseUrl;
        private string emulatorBaseUrl;        

        private Dictionary<string, Tuple<Dictionary<string, PaymentExperienceSetting>, DateTime>> pssCache;
        private int partnerSettingsRefreshInternvalInSec = 3600;
        private object partnerSettingsLockObj = new object();
        private bool disablePSSCache;

        public PartnerSettingsServiceAccessor(
            string serviceBaseUrl,
            string emulatorBaseUrl,
            string servicePPEBaseUrl,
            HttpMessageHandler messageHandler,
            bool disablePSSCache = false)
        {
            this.serviceBaseUrl = serviceBaseUrl;
            this.servicePPEBaseUrl = servicePPEBaseUrl;
            this.emulatorBaseUrl = emulatorBaseUrl;

            this.pssCache = new Dictionary<string, Tuple<Dictionary<string, PaymentExperienceSetting>, DateTime>>();
            this.disablePSSCache = disablePSSCache;

            this.partnerSettingsServiceHttpClient = new PXTracingHttpClient(Constants.ServiceNames.PartnerSettingsService, messageHandler);
            this.partnerSettingsServiceHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType));
            this.partnerSettingsServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.Connection, PaymentConstants.HttpHeaders.KeepAlive);
            this.partnerSettingsServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.KeepAlive, string.Format(PaymentConstants.HttpHeaders.KeepAliveParameter, 60));
        }

        private string BaseUrl
        {
            get
            {
                if (HttpRequestHelper.IsPXTestRequest("px.partnersettings") && !string.IsNullOrWhiteSpace(this.emulatorBaseUrl))
                {
                    return this.emulatorBaseUrl;
                }
                else
                {
                    return this.serviceBaseUrl;
                }
            }
        }

        public async Task<Dictionary<string, PaymentExperienceSetting>> GetPaymentExperienceSettings(string partnerName, string settingsVersion, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.GetPaymentExperienceSettings, partnerName);
            string cachingKey = partnerName;

            if (settingsVersion != null)
            {
                requestUrl = string.Format(requestUrl + "&version={0}", settingsVersion);
                cachingKey = string.Format("{0}+{1}", partnerName, settingsVersion);
            }

            if (HttpRequestHelper.IsPXTestRequest("px.partnersettings") || this.disablePSSCache || IsPartnerSettingsServiceFlightExposed(Flighting.Features.PXDisablePSSCache, exposedFlightFeatures))
            {
                var flightHeaders = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(PaymentConstants.PaymentExtendedHttpHeaders.FlightHeader, DisableCache)
                };
                return await this.SendRequest<Dictionary<string, PaymentExperienceSetting>>(HttpMethod.Get, requestUrl, null, "getPartnerSettings", traceActivityId, exposedFlightFeatures, flightHeaders);
            }

            Tuple<Dictionary<string, PaymentExperienceSetting>, DateTime> cachedResponse;
            
            if (!this.pssCache.TryGetValue(cachingKey, out cachedResponse) 
                    || DateTime.UtcNow.Subtract(cachedResponse.Item2).TotalSeconds > this.partnerSettingsRefreshInternvalInSec)
            {
                try
                {
                    var response = await this.SendRequest<Dictionary<string, PaymentExperienceSetting>>(HttpMethod.Get, requestUrl, null, "getPartnerSettings", traceActivityId, exposedFlightFeatures);

                    lock (this.partnerSettingsLockObj)
                    {
                        this.pssCache[cachingKey] = new Tuple<Dictionary<string, PaymentExperienceSetting>, DateTime>(response, DateTime.UtcNow);
                    }

                    return response;
                }
                catch (Exception serviceResponseException)
                {
                    if (cachedResponse != null)
                    {
                        SllWebLogger.TracePXServiceException($"Exception Calling Partner Settings Service, Using Cached Response:" + serviceResponseException.Message, traceActivityId);
                        lock (this.partnerSettingsLockObj)
                        {
                            this.pssCache[cachingKey] = new Tuple<Dictionary<string, PaymentExperienceSetting>, DateTime>(cachedResponse.Item1, DateTime.UtcNow);
                        }

                        return cachedResponse.Item1;
                    }

                    throw serviceResponseException;
                }             
            }
            else
            {
                return cachedResponse.Item1;
            }
        }

        private static void AddHeaders(HttpRequestMessage request, IList<KeyValuePair<string, string>> headers)
        {
            if (headers != null && request != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }
        }

        private static bool IsPartnerSettingsServiceFlightExposed(string flightName, List<string> exposedFlightFeatures)
        {
            return exposedFlightFeatures != null && exposedFlightFeatures.Contains(flightName, StringComparer.OrdinalIgnoreCase);
        }

        private async Task<T> SendRequest<T>(HttpMethod method, string url, object request, string actionName, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, IList<KeyValuePair<string, string>> additionalHeaders = null)
        {
            string fullRequestUrl = string.Empty;

            if (IsPartnerSettingsServiceFlightExposed(Flighting.Features.PXServicePSSPPEEnvironment, exposedFlightFeatures))
            {
                fullRequestUrl = string.Format("{0}/{1}", this.servicePPEBaseUrl, url);
            }
            else
            {
                fullRequestUrl = string.Format("{0}/{1}", this.BaseUrl, url);
            }

            using (HttpRequestMessage requestMessage = new HttpRequestMessage(method, fullRequestUrl))
            {
                requestMessage.IncrementCorrelationVector(traceActivityId);
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString());
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, Guid.NewGuid().ToString());

                requestMessage.AddOrReplaceActionName(actionName);

                HttpRequestHelper.TransferTargetHeadersFromIncomingRequestToOutgoingRequest(this.passThroughHeaders, requestMessage);

                AddHeaders(requestMessage, additionalHeaders);

                // Adds PXUsePSSPartnerMockForDiffTest flight to request for PSS emualtor to return the respnse from PartnerSettingsByPartner
                if (IsPartnerSettingsServiceFlightExposed(Flighting.Features.PXUsePSSPartnerMockForDiffTest, exposedFlightFeatures))
                {
                    requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.FlightHeader, Flighting.Features.PXUsePSSPartnerMockForDiffTest);
                }

                if (request != null)
                {
                    requestMessage.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType);
                }

                using (HttpResponseMessage response = await this.partnerSettingsServiceHttpClient.SendAsync(requestMessage))
                {
                    string responseMessage = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            return JsonConvert.DeserializeObject<T>(
                                responseMessage,
                                new JsonSerializerSettings
                                {
                                    MissingMemberHandling = MissingMemberHandling.Ignore
                                });
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException($"Failed to deserialize success response from {Constants.ServiceNames.PartnerSettingsService}. Response Message: {responseMessage}"));
                        }
                    }
                    else
                    {
                        ServiceErrorResponse error = null;
                        try
                        {
                            ServiceErrorResponse innerError = JsonConvert.DeserializeObject<ServiceErrorResponse>(responseMessage);
                            innerError.Source = string.IsNullOrWhiteSpace(innerError.Source) ? Constants.ServiceNames.PartnerSettingsService : innerError.Source;
                            error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError.ToString());
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException($"Failed to deserialize error response from {Constants.ServiceNames.PartnerSettingsService}. Response Message: {responseMessage}"));
                        }

                        throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response, HandlingType = ExceptionHandlingPolicy.ByPass });
                    }
                }
            }
        }
    }
}