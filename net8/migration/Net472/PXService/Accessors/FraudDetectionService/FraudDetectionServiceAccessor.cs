// <copyright file="FraudDetectionServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.FraudDetectionService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Common;
    using Common.Tracing;
    using Common.Web;
    using Microsoft.Commerce.Payments.PXService.Model.FraudDetectionService;
    using Newtonsoft.Json;
    using PXCommon;
    using Tracing;

    /// <summary>
    /// Provides functionality to access FraudDetectionService
    /// </summary>
    public class FraudDetectionServiceAccessor : IFraudDetectionServiceAccessor
    {
        private readonly List<string> passThroughHeaders = new List<string>
        {
            PaymentConstants.PaymentExtendedHttpHeaders.TestHeader,
            GlobalConstants.HeaderValues.XMsRequestContext
        };

        private string serviceBaseUrl;
        private string emulatorBaseUrl;

        private HttpClient fraudDetectionServiceHttpClient;

        public FraudDetectionServiceAccessor(
            string serviceBaseUrl,
            string emulatorBaseUrl,
            HttpMessageHandler messageHandler)
        {
            this.serviceBaseUrl = serviceBaseUrl;
            this.emulatorBaseUrl = emulatorBaseUrl;

            this.fraudDetectionServiceHttpClient = new PXTracingHttpClient(PXCommon.Constants.ServiceNames.FraudDetectionService, messageHandler);
            this.fraudDetectionServiceHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType));
            this.fraudDetectionServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.Connection, PaymentConstants.HttpHeaders.KeepAlive);
            this.fraudDetectionServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.KeepAlive, string.Format(PaymentConstants.HttpHeaders.KeepAliveParameter, 60));
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

        /// <summary>
        /// Bot detection 
        /// </summary>
        /// <param name="requestId">Request Id, e.g. payment request or checkout request id</param> 
        /// <param name="traceActivityId">Trace Activity Id</param> 
        /// <returns>Evaluation Result</returns>       
        public async Task<EvaluationResult> BotDetection(string requestId, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.FraudDetectionBotDetectionUrl);

            BotdetectionRequest payload = new BotdetectionRequest()
            {
                GreenId = requestId,
                IpAddress = "ipaddress" // TODO: remove ipaddress field after fraud detection service make it as opitonal field
            };

            return await this.SendRequest<EvaluationResult>(
                HttpMethod.Post,
                requestUrl,
                payload,
                "BotDetection",
                traceActivityId);
        }

        /// <summary>
        /// Bot detection 
        /// </summary>
        /// <param name="requestId">Request Id, e.g. payment request or checkout request id</param> 
        /// <param name="traceActivityId">Trace Activity Id</param> 
        /// <param name="isChallengeResolved">Challenge Resolution Result</param> 
        /// <returns>Evaluation Result</returns>       
        public async Task<EvaluationResult> BotDetectionConfirmation(string requestId, EventTraceActivity traceActivityId, bool isChallengeResolved)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.FraudDetectionBotDetectionUrl);

            BotdetectionRequest payload = new BotdetectionRequest()
            {
                GreenId = requestId,
                IpAddress = "ipaddress", // TODO: remove ipaddress field after fraud detection service make it as opitonal field
                IsChallengeResolved = isChallengeResolved
            };

            return await this.SendRequest<EvaluationResult>(
                HttpMethod.Post,
                requestUrl,
                payload,
                "BotDetection",
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

                HttpRequestHelper.TransferTargetHeadersFromIncomingRequestToOutgoingRequest(this.passThroughHeaders, requestMessage);

                // Add action name to the request properties so that this request's OperationName is logged properly
                requestMessage.AddOrReplaceActionName(actionName);

                AddHeaders(requestMessage, additionalHeaders);

                if (request != null)
                {
                    requestMessage.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType);
                }

                try
                {
                    using (HttpResponseMessage response = await this.fraudDetectionServiceHttpClient.SendAsync(requestMessage))
                    {
                        string responseMessage = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            try
                            {
                                return JsonConvert.DeserializeObject<T>(responseMessage);
                            }
                            catch (JsonException jsonEx)
                            {
                                throw TraceCore.TraceException(traceActivityId, new FailedOperationException($"Failed to deserialize success response from {Constants.ServiceNames.FraudDetectionService}. Response Message: {responseMessage}", jsonEx));
                            }
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(responseMessage))
                            {
                                AccessorHandler.HandleEmptyErrorResponses(response, actionName, traceActivityId, PXCommon.Constants.ServiceNames.FraudDetectionService);
                            }

                            ServiceErrorResponse error = null;
                            try
                            {
                                ServiceErrorResponse innerError = JsonConvert.DeserializeObject<ServiceErrorResponse>(responseMessage);
                                innerError.Source = string.IsNullOrWhiteSpace(innerError.Source) ? Constants.ServiceNames.FraudDetectionService : innerError.Source;
                                error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError);
                            }
                            catch (JsonException jsonEx)
                            {
                                throw TraceCore.TraceException(traceActivityId, new FailedOperationException($"Failed to deserialize error response from {Constants.ServiceNames.FraudDetectionService}. Response Message: {responseMessage}", jsonEx));
                            }
                            catch (Exception ex)
                            {
                                throw TraceCore.TraceException(traceActivityId, new FailedOperationException($"Failed to deserialize error response from {Constants.ServiceNames.FraudDetectionService}. Response Message: {responseMessage}", ex));
                            }

                            throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response, HandlingType = ExceptionHandlingPolicy.ByPass });
                        }
                    }
                }
                catch (Exception ex)
                {
                    SllWebLogger.TracePXServiceException($"Error occured while calling fraud detection service: {ex.ToString()}.", traceActivityId);
                    throw;
                }
            }
        }
    }
}