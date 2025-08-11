// <copyright file="ServiceLoggingIncomingTraceHandler.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing.Metris;
    using Microsoft.Commerce.Payments.Common.Tracing.Metris.Dimensions;
    using Microsoft.Commerce.Payments.Common.Tracing.Metris.OTel;
    using Microsoft.Commerce.Tracing;
    using Ms.Qos;
    using Tracing;

    public class ServiceLoggingIncomingTraceHandler : DetailedTraceHandler
    {
        public ServiceLoggingIncomingTraceHandler()
            : base(TraceApiDetails)
        {
        }

        public ServiceLoggingIncomingTraceHandler(HttpMessageHandler innerHandler)
            : base(TraceApiDetails, innerHandler)
        {
        }

        protected static async Task TraceApiDetails(HttpRequestMessage request, HttpResponseMessage response, string operationName, long latency, string additionalMessage, EventTraceActivity requestTraceId, EventTraceActivity serverTraceId)
        {
            string requestPayload = await request.GetRequestPayload();

            // Always truncate the success response for GET in SLL logs.
            string responsePayload = await response.GetResponsePayload();

            if (LoggingConfig.Mode == LoggingMode.Sll)
            {
                SllWebLogger.TraceServiceLoggingIncoming(
                operationName,
                request,
                response,
                requestPayload,
                responsePayload,
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
                requestPayload,
                responsePayload,
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
                requestPayload,
                responsePayload,
                (int)latency,
                requestTraceId.ActivityId.ToString(),
                serverTraceId.ActivityId.ToString(),
                string.Empty);

                SllWebLogger.TraceServiceLoggingIncoming(
                operationName,
                request,
                response,
                requestPayload,
                responsePayload,
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
