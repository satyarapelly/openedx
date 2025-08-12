// <copyright file="OrchestrationServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.OrchestrationService
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Common;
    using Common.Tracing;
    using Common.Web;
    using DataModel;
    using Newtonsoft.Json;
    using PXCommon;

    public class OrchestrationServiceAccessor : IOrchestrationServiceAccessor
    {
        private readonly List<string> passThroughHeaders = new List<string>
        {
            GlobalConstants.HeaderValues.AuthInfoHeader,
            GlobalConstants.HeaderValues.ExtendedFlightName,
            PaymentConstants.PaymentExtendedHttpHeaders.TestHeader,
            PaymentConstants.PaymentExtendedHttpHeaders.CorrelationContext,
            GlobalConstants.HeaderValues.CustomerHeader
        };

        private string serviceBaseUrl;
        private string emulatorBaseUrl;

        private string apiVersion;
        private HttpClient orchestrationHttpClient;

        public OrchestrationServiceAccessor(
            string serviceBaseUrl,
            string emulatorBaseUrl,
            string apiVersion,
            HttpMessageHandler messageHandler)
        {
            this.serviceBaseUrl = serviceBaseUrl;
            this.emulatorBaseUrl = emulatorBaseUrl;
            this.apiVersion = apiVersion;

            this.orchestrationHttpClient = new PXTracingHttpClient(PXCommon.Constants.ServiceNames.OrchestrationService, messageHandler);
            this.orchestrationHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType));
            this.orchestrationHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.Connection, PaymentConstants.HttpHeaders.KeepAlive);
            this.orchestrationHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.KeepAlive, string.Format(PaymentConstants.HttpHeaders.KeepAliveParameter, 60));
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

        public async Task RemovePaymentInstrument(string paymentInstrumentId, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.OrchestrationServiceRemovePaymentInstrument, this.apiVersion, paymentInstrumentId);

            await this.SendRequest<object>(
                HttpMethod.Post,
                requestUrl,
                null,
                "Remove",
                traceActivityId);
        }

        public async Task ReplacePaymentInstrument(string sourcePaymentInstrumentId, string targetPaymentInstrumentId, string paymentSessionId, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.OrchestrationServiceReplacePaymentInstrument, this.apiVersion, sourcePaymentInstrumentId);

            ReplaceRequest replaceRequest = new ReplaceRequest()
            {
                TargetPaymentInstrumentId = targetPaymentInstrumentId,
                PaymentSessionId = paymentSessionId
            };

            await this.SendRequest<object>(
                HttpMethod.Post,
                requestUrl,
                replaceRequest,
                "Replace",
                traceActivityId);
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

        private async Task<T> SendRequest<T>(HttpMethod method, string url, object request, string actionName, EventTraceActivity traceActivityId, IList<KeyValuePair<string, string>> additionalHeaders = null)
        {
            string fullRequestUrl = string.Format("{0}/{1}", this.BaseUrl, url);

            using (HttpRequestMessage requestMessage = new HttpRequestMessage(method, fullRequestUrl))
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
                    requestMessage.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType);
                }

                using (HttpResponseMessage response = await this.orchestrationHttpClient.SendAsync(requestMessage))
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
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException($"Failed to deserialize success response from {Constants.ServiceNames.OrchestrationService}. Response Message: {responseMessage}"));
                        }
                    }
                    else
                    {
                        ServiceErrorResponse error = null;
                        try
                        {
                            OrchestrationErrorResponse orchestrationError = JsonConvert.DeserializeObject<OrchestrationErrorResponse>(responseMessage);
                            ServiceErrorResponse innerError = new ServiceErrorResponse()
                            {
                                ErrorCode = orchestrationError.ErrorCode,
                                Message = orchestrationError.Message,
                                Target = orchestrationError.Targets == null ? string.Empty : string.Join(",", orchestrationError.Targets),
                                Source = Constants.ServiceNames.OrchestrationService
                            };
                            error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException($"Failed to deserialize error response from {Constants.ServiceNames.OrchestrationService}. Response Message: {responseMessage}"));
                        }

                        throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response, HandlingType = ExceptionHandlingPolicy.ByPass });
                    }
                }
            }
        }
    }
}