// <copyright file="IssuerServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXService.Accessors.IssuerService
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
    using Microsoft.Commerce.Payments.PXService.Model.IssuerService;
    using Microsoft.Commerce.Tracing;
    using Newtonsoft.Json;

    public class IssuerServiceAccessor : IIssuerServiceAccessor
    {
        private readonly List<string> passThroughHeaders = new List<string>
        {
            PaymentConstants.PaymentExtendedHttpHeaders.TestHeader,
            GlobalConstants.HeaderValues.CustomerHeader
        };

        private string emulatorBaseUrl;
        private string serviceBaseUrl;
        private HttpClient issuerServiceClient;
        private string apiVersion;

        public IssuerServiceAccessor(string emulatorBaseUrl, string serviceBaseUrl, string apiVersion, HttpMessageHandler messageHandler)
        {
            this.emulatorBaseUrl = emulatorBaseUrl;
            this.serviceBaseUrl = serviceBaseUrl;
            this.apiVersion = apiVersion;

            this.issuerServiceClient = new PXTracingHttpClient(Constants.ServiceNames.IssuerService, messageHandler);
            this.issuerServiceClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType));
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

        public async Task<ApplyResponse> Apply(string customerPuid, ApplyRequest apply)
        {
            EventTraceActivity traceActivityID = new EventTraceActivity();

            string requestUrl = string.Format(V7.Constants.UriTemplate.Apply, customerPuid);

            return await this.SendRequest<ApplyResponse>(
               HttpMethod.Post,
               "apply",
               requestUrl,
               traceActivityID,
               apply);
        }

        public async Task<List<Application>> ApplicationDetails(string puid, string cardProduct, string sessionId)
        {
            EventTraceActivity traceActivityID = new EventTraceActivity();

            string requestUrl = string.Format(V7.Constants.UriTemplate.ApplicationDetails, puid, cardProduct, sessionId);

            return await this.SendRequest<List<Application>>(
               HttpMethod.Get,
               "applicationDetails",
               requestUrl,
               traceActivityID);
        }

        public async Task<EligibilityResponse> Eligibility(string customerPuid, string cardProduct)
        {
            EventTraceActivity traceActivityID = new EventTraceActivity();

            string requestUrl = string.Format(V7.Constants.UriTemplate.ApplyEligibility, customerPuid, cardProduct);

            return await this.SendRequest<EligibilityResponse>(
               HttpMethod.Get,
               "applyEligibility",
               requestUrl,
               traceActivityID);
        }

        public async Task<InitializeResponse> Initialize(InitializeRequest request)
        {
            EventTraceActivity traceActivityID = new EventTraceActivity();

            string requestUrl = string.Format(V7.Constants.UriTemplate.ApplyInitalize);

            return await this.SendRequest<InitializeResponse>(
               HttpMethod.Post,
               "initialize",
               requestUrl,
               traceActivityID,
               request);
        }

        private async Task<T> SendRequest<T>(
            HttpMethod method,
            string actionName,
            string requestUrl,
            EventTraceActivity traceActivityId,
            object payload = null,
            IList<KeyValuePair<string, string>> additionalHeaders = null)
        {
            string fullRequestUrl = string.Format("{0}/{1}", this.BaseUrl, requestUrl);
            PaymentsEventSource.Log.PXServiceRequestToIssuerService(fullRequestUrl, traceActivityId);

            using (HttpRequestMessage requestMessage = new HttpRequestMessage(method, fullRequestUrl))
            {
                requestMessage.IncrementCorrelationVector(traceActivityId);
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString());
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, Guid.NewGuid().ToString());
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.XMsApiVersion, this.apiVersion);

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
                    requestMessage.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType);
                }

                using (HttpResponseMessage response = await this.issuerServiceClient.SendAsync(requestMessage))
                {
                    string responseMessage = await response.Content.ReadAsStringAsync();
                    PaymentsEventSource.Log.PXServiceTraceResponseFromIssuerService(response.StatusCode.ToString(), responseMessage, traceActivityId);

                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            return JsonConvert.DeserializeObject<T>(responseMessage);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException(string.Format("Failed to deserialize success response from {0}, Response http status code {1}", Constants.ServiceNames.IssuerService, response.StatusCode)));
                        }
                    }
                    else
                    {
                        ServiceErrorResponse error = null;
                        try
                        {
                            ServiceErrorResponse innerError = JsonConvert.DeserializeObject<ServiceErrorResponse>(responseMessage);
                            innerError.Source = string.IsNullOrWhiteSpace(innerError.Source) ? Constants.ServiceNames.IssuerService : innerError.Source;
                            error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException(string.Format("Failed to deserialize error response from {0} {1}", Constants.ServiceNames.IssuerService, responseMessage)));
                        }

                        throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response, HandlingType = ExceptionHandlingPolicy.ByPass });
                    }
                }
            }
        }
    }
}