// <copyright file="TraceCorrelationHandler.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Routing;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Tracing.Metris;
    using Microsoft.Commerce.Payments.Common.Tracing.Metris.Dimensions;
    using Microsoft.Commerce.Payments.Common.Tracing.Metris.OTel;
    using Microsoft.Commerce.Tracing;
    using Microsoft.CommonSchema.Services.Logging;
    using Ms.Qos;

    /// <summary>
    /// Delegating handler which performs detailed message tracing.  Informational
    /// traces include request method, request URI, request headers, request payload
    /// (interpreted as a string), response code, response headers, and response payload
    /// (interpreted as a string).
    /// </summary>
    public class TraceCorrelationHandler : DelegatingHandler
    {
        public TraceCorrelationHandler()
        {
        }

        public TraceCorrelationHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        {
        }

        public TraceCorrelationHandler(string serviceName)
        {
            this.ServiceName = serviceName;
        }

        public TraceCorrelationHandler(string serviceName, HttpMessageHandler innerHandler) : base(innerHandler)
        {
            this.ServiceName = serviceName;
        }

        private string ServiceName { get; set; }

        /// <summary>
        /// Extracts trace correlation information from the request, sends the
        /// request up the pipeline, and then stamps correlation information on
        /// the response.
        /// </summary>
        /// <param name="request">The request message.</param>
        /// <param name="cancellationToken">A token which may be used to listen
        /// for cancellation.</param>
        /// <returns>The response message.</returns>
        protected override sealed async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CorrelationVector correlationVector = SllCorrelationVectorManager.SetCorrelationVectorAtRequestEntry(request);
            Guid correlationId = Guid.Empty;
            IEnumerable<string> correlationIdHeaderValues;
            if (request.Headers.TryGetValues(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, out correlationIdHeaderValues))
            {
                foreach (string headerValue in correlationIdHeaderValues)
                {
                    if (Guid.TryParse(headerValue, out correlationId))
                    {
                        break;
                    }
                }
            }

            EventTraceActivity serverTraceId = new EventTraceActivity { CorrelationVectorV4 = correlationVector };

            bool isDependentServiceRequest = true;
            string operationName = request.GetOperationName();
            if (operationName != null)
            {
                // If the request contains an operationName already, it means it is a request made to the dependent services.
                // Update the correlation ID on the request to the new tracking guid, so that we can uniquely 
                // identify dependent service request.
                request.Headers.Remove(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId);
                request.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, serverTraceId.ActivityId.ToString());
            }
            else
            {
                isDependentServiceRequest = false;

                // If the operation name does not exist in the request properties, then parse the request data to construct operation name.
                IHttpRouteData data = request.GetRouteData();

                StringBuilder counterNameBuilder = new StringBuilder();
                if (data != null)
                {
                    counterNameBuilder.Append(data.Values["controller"]);
                    counterNameBuilder.Append("-");
                    counterNameBuilder.Append(request.Method.ToString());

                    if (data.Values.ContainsKey("action"))
                    {
                        counterNameBuilder.Append("-");
                        counterNameBuilder.Append(data.Values["action"]);
                    }
                }
                else
                {
                    // In case there is no request data, get the action name from the request properties
                    string actionName = request.GetActionName();
                    if (actionName != null)
                    {
                        counterNameBuilder.Append(this.ServiceName);
                        counterNameBuilder.Append("-");
                        counterNameBuilder.Append(request.Method);
                        counterNameBuilder.Append("-");
                        counterNameBuilder.Append(actionName);
                    }
                    else
                    {
                        // If no action name was given, mark it as Unknown. The request sender should try to add this property to the request.
                        counterNameBuilder.Append(this.ServiceName);
                        counterNameBuilder.Append("-");
                        counterNameBuilder.Append(request.Method);
                        counterNameBuilder.Append("-Unknown");
                    }
                }

                operationName = counterNameBuilder.ToString();
                request.Properties.Add(PaymentConstants.Web.Properties.OperationName, operationName);
            }

            EventTraceActivity requestTraceId;
            if (correlationId == Guid.Empty)
            {
                // Check if we've already set the request property
                requestTraceId = request.GetServerTraceId();
                if (requestTraceId != null)
                {
                    correlationId = requestTraceId.ActivityId;
                }
                else
                {
                    correlationId = Guid.NewGuid();
                    requestTraceId = new EventTraceActivity(correlationId) { CorrelationVectorV4 = correlationVector };
                }
            }
            else
            {
                requestTraceId = new EventTraceActivity(correlationId) { CorrelationVectorV4 = correlationVector };
            }

            // Save this for other parts of the pipeline.
            if (!request.Properties.ContainsKey(PaymentConstants.Web.Properties.ServerTraceId))
            {
                // If there are multiple requests from client with same correlation id in short span of time, 
                // then the requests overlap.To avoid this we do trace transfer from requestTraceId to ServerTraceId.
                // All the payments servertraces will be correlated with serverTraceId.
                request.Properties.Add(PaymentConstants.Web.Properties.ServerTraceId, serverTraceId);
                PaymentsEventSource.Log.CommonMappingCorrelationId(serverTraceId, new EventTraceActivity(correlationId));
            }
            else
            {
                Debug.Assert(
                    ((EventTraceActivity)request.Properties[PaymentConstants.Web.Properties.ServerTraceId]).ActivityId == serverTraceId.ActivityId,
                    "Should never hit here, in which case trace IDs should be the same.");
            }

            if (!request.Properties.ContainsKey(PaymentConstants.Web.Properties.ClientTraceId) && !isDependentServiceRequest)
            {
                // Save the clientTraceId for the logging purpose
                request.Properties.Add(PaymentConstants.Web.Properties.ClientTraceId, requestTraceId);
            }

            string trackingId = null;
            IEnumerable<string> trackingIdHeaderValues;
            if (request.Headers.TryGetValues(PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, out trackingIdHeaderValues))
            {
                foreach (string headerValue in trackingIdHeaderValues)
                {
                    if (!string.IsNullOrEmpty(headerValue))
                    {
                        trackingId = headerValue;
                        break;
                    }
                }
            }

            if (trackingId == null)
            {
                trackingId = Guid.Empty.ToString();
            }

            if (!request.Properties.ContainsKey(PaymentConstants.Web.Properties.TrackingId))
            {
                request.Properties.Add(PaymentConstants.Web.Properties.TrackingId, trackingId);
            }

            // Need set the request content before processing.
            await request.GetRequestPayload();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            // Stamp the value used on the response.
            if (isDependentServiceRequest)
            {
                // if the request is made to dependent services like payments or risk etc, then update the requestID to serverTraceActivity
                response.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, serverTraceId.ActivityId.ToString());
            }
            else
            {
                // if the request is made from clients like Billing etc, then update the requestID to TraceActivity
                response.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, requestTraceId.ActivityId.ToString());
            }

            stopwatch.Stop();
            await this.TraceOperation(request, response, operationName, stopwatch.ElapsedMilliseconds, string.Empty, requestTraceId, serverTraceId);

            return response;
        }

        /// <summary>
        /// SLL Tracing
        /// </summary>
        /// <param name="request">Inbound request message to be traced.</param>
        /// <param name="response">Outbound response message to be traced.</param>
        /// <param name="operationName">The logical name of the operation being performed.</param>
        /// <param name="latency">Time taken for operation.</param>
        /// <param name="additionalMessage">Additional message to be traced.</param>
        /// <param name="requestTraceId">1. Caller created ExternalActivityId to payments service, 2. PaymentsActivityId to dependent services.</param>
        /// <param name="serverTraceId">1. PaymentsActivityId to payments service, 2. Payment created ExternalActivityId to dependent services.</param>
        /// <returns>A task representing the async work.</returns>
        protected virtual async Task TraceOperation(HttpRequestMessage request, HttpResponseMessage response, string operationName, long latency, string additionalMessage, EventTraceActivity requestTraceId, EventTraceActivity serverTraceId)
        {
            if (LoggingConfig.Mode == LoggingMode.Sll)
            {
                SllWebLogger.TraceServiceLoggingIncoming(
                operationName,
                request,
                response,
                await request.GetRequestPayload(),
                await response.GetResponsePayload(),
                (int)latency,
                requestTraceId.ActivityId.ToString(),
                serverTraceId.ActivityId.ToString(),
                string.Empty);
            }
            else if (LoggingConfig.Mode == LoggingMode.OpenTelemetry)
            {
                Logger.Qos.TraceServiceLoggingIncoming(
                operationName,
                request,
                response,
                await request.GetRequestPayload(),
                await response.GetResponsePayload(),
                (int)latency,
                requestTraceId.ActivityId.ToString(),
                serverTraceId.ActivityId.ToString(),
                string.Empty);
            }
            else
            {
                Logger.Qos.TraceServiceLoggingIncoming(
                operationName,
                request,
                response,
                await request.GetRequestPayload(),
                await response.GetResponsePayload(),
                (int)latency,
                requestTraceId.ActivityId.ToString(),
                serverTraceId.ActivityId.ToString(),
                string.Empty);

                SllWebLogger.TraceServiceLoggingIncoming(
                operationName,
                request,
                response,
                await request.GetRequestPayload(),
                await response.GetResponsePayload(),
                (int)latency,
                requestTraceId.ActivityId.ToString(),
                serverTraceId.ActivityId.ToString(),
                string.Empty);
            }

            var responseOperationStatus = response.GetResponseOperationStatus();

            var requestDimensions = new RequestDimensions
            {
                OperationName = operationName,
                ResponseStatusCode = Convert.ToInt32(response.StatusCode).ToString(),
                CallerName = request.GetRequestCallerName(),
                RequestStatus = Convert.ToInt32(responseOperationStatus).ToString(),
                Scenario = request.GetRequestScenarioId()
            };

            var errorSuccessDimensions = new RequestDimensions
            {
                OperationName = operationName,
                ResponseStatusCode = Convert.ToInt32(response.StatusCode).ToString(),
                CallerName = request.GetRequestCallerName(),
                RequestStatus = Convert.ToInt32(responseOperationStatus).ToString(),
                Scenario = request.GetRequestScenarioId(),
                PaymentMethodFamily = request.GetPaymentMethodFamily(),
                PaymentMethodType = request.GetPaymentMethodType()
            };

            var partnerDimensions = new RequestDimensions
            {
                OperationName = operationName,
                ResponseStatusCode = Convert.ToInt32(response.StatusCode).ToString(),
                CallerName = request.GetRequestCallerName(),
                RequestStatus = Convert.ToInt32(responseOperationStatus).ToString(),
                Scenario = request.GetRequestScenarioId(),
                Partner = request.GetRequestPartner()
            };

            bool isSuccess = responseOperationStatus != ServiceRequestStatus.TransportError && responseOperationStatus != ServiceRequestStatus.ServiceError;

            MetricPerfCounters.SafeReport<IIncomingRequestMetrics>((counters) =>
            {
                counters.IncomingApiRequests.Increment(requestDimensions);
                counters.IncomingApiReliability.Record(isSuccess ? 100.0 : 0.0, requestDimensions);

                if (isSuccess)
                {
                    counters.IncomingApiSuccessLatency.Record(latency, requestDimensions);
                }

                if ((int)response.StatusCode >= 500 && (int)response.StatusCode < 600)
                {
                    counters.IncomingTotalSystemErrors.Increment(errorSuccessDimensions);
                    if (!string.IsNullOrEmpty(partnerDimensions.Partner))
                    {
                        counters.IncomingTotalSystemErrorsWithPartner.Increment(partnerDimensions);
                    }
                }

                if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                {
                    counters.IncomingTotalUserErrors.Increment(errorSuccessDimensions);
                    if (!string.IsNullOrEmpty(partnerDimensions.Partner))
                    {
                        counters.IncomingTotalUserErrorsWithPartner.Increment(partnerDimensions);
                    }
                }

                if ((int)response.StatusCode >= 200 && (int)response.StatusCode < 300)
                {
                    counters.IncomingTotalSuccesses.Increment(errorSuccessDimensions);
                }

                if (!string.IsNullOrEmpty(partnerDimensions.Partner))
                {
                    counters.IncomingApiRequestsWithPartner.Increment(partnerDimensions);
                    counters.IncomingApiReliabilityWithPartner.Record(isSuccess ? 100.0 : 0.0, partnerDimensions);
                    if (isSuccess)
                    {
                        counters.IncomingApiSuccessLatencyWithPartner.Record(latency, partnerDimensions);
                        counters.IncomingTotalSuccessesWithPartner.Increment(partnerDimensions);
                    }
                }
            });
        }
    }
}