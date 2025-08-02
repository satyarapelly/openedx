// <copyright file="ShortURLServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.ShortURLService
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXCommon;
    using Model.ShortURLService;
    using Newtonsoft.Json;

    public class ShortURLServiceAccessor : IShortURLServiceAccessor
    {
        private const string ShortURLServiceName = "ShortURLService";
        private readonly List<string> passThroughHeaders = new List<string> { PaymentConstants.PaymentExtendedHttpHeaders.TestHeader };
        private HttpClient shortURLServiceHttpClient;

        private string serviceBaseUrl;

        public ShortURLServiceAccessor(
            string serviceBaseUrl,
            HttpMessageHandler messageHandler)
        {
            this.serviceBaseUrl = serviceBaseUrl;

            this.shortURLServiceHttpClient = new PXTracingHttpClient(PXCommon.Constants.ServiceNames.ShortURLService, messageHandler, ApplicationInsightsProvider.LogOutgoingOperation);
            this.shortURLServiceHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType));
            this.shortURLServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.Connection, PaymentConstants.HttpHeaders.KeepAlive);
            this.shortURLServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.KeepAlive, string.Format(PaymentConstants.HttpHeaders.KeepAliveParameter, 60));
        }

        private string BaseUrl
        {
            get
            {
                return this.serviceBaseUrl;
            }
        }

        public async Task<CreateShortURLResponse> CreateShortURL(string longUrl, int? ttlMinutes, EventTraceActivity traceActivityId)
        {
            CreateShortURLRequest req = new CreateShortURLRequest()
            {
                URL = longUrl,
                TTLMinutes = ttlMinutes
            };

            return await this.SendRequest<CreateShortURLResponse>(HttpMethod.Post, "create", req, "Create", traceActivityId);
        }

        public async Task DeleteShortURL(string codeOrUrl, EventTraceActivity traceActivityId)
        {
            var req = codeOrUrl;
            await this.SendRequest<object>(HttpMethod.Delete, string.Empty, req, "Delete", traceActivityId);
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

        private async Task<T> SendRequest<T>(
            HttpMethod method, 
            string url, 
            object request, 
            string actionName, 
            EventTraceActivity traceActivityId, 
            IList<KeyValuePair<string, string>> additionalHeaders = null)
        {
            string fullRequestUrl = string.Format("{0}/{1}", this.BaseUrl, url);

            using (HttpRequestMessage requestMessage = new HttpRequestMessage(method, fullRequestUrl))
            {
                requestMessage.IncrementCorrelationVector(traceActivityId);
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString());
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, Guid.NewGuid().ToString());

                requestMessage.AddOrReplaceActionName(actionName);

                HttpRequestHelper.TransferTargetHeadersFromIncomingRequestToOutgoingRequest(this.passThroughHeaders, requestMessage);

                AddHeaders(requestMessage, additionalHeaders);

                if (request != null)
                {
                    requestMessage.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType);
                }

                using (HttpResponseMessage response = await this.shortURLServiceHttpClient.SendAsync(requestMessage))
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
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException($"Failed to deserialize success response from {Constants.ServiceNames.ShortURLService}. Response Message: {responseMessage}"));
                        }
                    }
                    else
                    {
                        ServiceErrorResponse error = null;
                        try
                        {
                            ServiceErrorResponse innerError = JsonConvert.DeserializeObject<ServiceErrorResponse>(responseMessage);
                            innerError.Source = string.IsNullOrWhiteSpace(innerError.Source) ? ShortURLServiceName : innerError.Source;
                            error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException($"Failed to deserialize error response from {Constants.ServiceNames.ShortURLService}. Response Message: {responseMessage}"));
                        }

                        throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response, HandlingType = ExceptionHandlingPolicy.ByPass });
                    }
                }
            }
        }
    }
}