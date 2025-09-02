// <copyright file="RDSServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
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
    using Microsoft.Commerce.Payments.PXService.Accessors.RDSSessionService.DataModel;
    using Newtonsoft.Json;

    public class RDSServiceAccessor : IRDSServiceAccessor
    {
        private const string RDSServiceName = "RDSService";
        private readonly List<string> passThroughHeaders = new List<string> { GlobalConstants.HeaderValues.ExtendedFlightName, PaymentConstants.PaymentExtendedHttpHeaders.TestHeader };

        private string baseUrl;
        private HttpClient rdsServiceHttpClient;

        public RDSServiceAccessor(
            string baseUrl,
            HttpMessageHandler messageHandler)
        {
            this.baseUrl = baseUrl;

            this.rdsServiceHttpClient = new PXTracingHttpClient(Constants.ServiceNames.RDSService, messageHandler);
            this.rdsServiceHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType));
            this.rdsServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.Connection, PaymentConstants.HttpHeaders.KeepAlive);
            this.rdsServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.KeepAlive, string.Format(PaymentConstants.HttpHeaders.KeepAliveParameter, 60));
        }

        public async Task<string> GetRDSSessionState(string rdsSessionId, EventTraceActivity traceActivityId)
        {
            var sessionState = await this.SendRequest<QueryResponse>(
                method: HttpMethod.Get,
                url: string.Format(V7.Constants.UriTemplate.RDSServiceQuery, rdsSessionId),
                actionName: "Query",
                traceActivityId: traceActivityId);

            return sessionState?.SessionState;
        }

        private async Task<T> SendRequest<T>(HttpMethod method, string url, string actionName, EventTraceActivity traceActivityId, object payload = null, IList<KeyValuePair<string, string>> additionalHeaders = null)
        {
            string fullRequestUrl = string.Format("{0}{1}", this.baseUrl, url);
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(method, fullRequestUrl))
            {
                requestMessage.IncrementCorrelationVector(traceActivityId);
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString());
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, Guid.NewGuid().ToString());
                requestMessage.Headers.Add("Request-Id", Guid.NewGuid().ToString());

                // Add action name to the request properties so that this request's OperationName is logged properly
                requestMessage.AddOrReplaceActionName(actionName);
                HttpRequestHelper.TransferTargetHeadersFromIncomingRequestToOutgoingRequest(this.passThroughHeaders, requestMessage);

                if (additionalHeaders != null)
                {
                    foreach (var headerKvp in additionalHeaders)
                    {
                        requestMessage.Headers.Add(headerKvp.Key, headerKvp.Value);
                    }
                }

                if (payload != null)
                {
                    requestMessage.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType); // lgtm[cs/web/xss] The request is being made to a web service and not to a web page.
                }

                using (HttpResponseMessage response = await this.rdsServiceHttpClient.SendAsync(requestMessage))
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
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException(string.Format("Failed to deserialize success response from RDSService, Response http status code {0}", response.StatusCode)));
                        }
                    }
                    else
                    {
                        ServiceErrorResponse error = null;
                        try
                        {
                            ServiceErrorResponse innerError = JsonConvert.DeserializeObject<ServiceErrorResponse>(responseMessage);
                            innerError.Source = string.IsNullOrWhiteSpace(innerError.Source) ? RDSServiceName : innerError.Source;
                            error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Failed to deserialize error response from RDSService"));
                        }

                        throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response });
                    }
                }
            }
        }
    }
}
