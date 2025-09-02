// <copyright file="ServiceLoggingIncomingTraceHandler.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;

    public class ServiceLoggingIncomingTraceHandler
    {
        public ServiceLoggingIncomingTraceHandler()
        {
        }

        public ServiceLoggingIncomingTraceHandler(HttpMessageHandler innerHandler)
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
        }
    }
}
