// <copyright file="SllLogger.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Tracing;
    using System.Net.Http;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Tracing.Sll;
    using Microsoft.CommonSchema.Services;
    using Microsoft.CommonSchema.Services.Logging;
    using Ms.Qos;
    using Newtonsoft.Json;

    /// <summary>
    /// The class of SllLogger.
    /// </summary>
    public static class SllLogger
    {
        static SllLogger()
        {
            EnvironmentLogOption = LogOption.None;
            Masker = new JsonDataMasker();
        }

        public static LogOption EnvironmentLogOption { get; private set; }

        public static JsonDataMasker Masker { get; private set; }

        public static void SetRealtimeLogging()
        {
            EnvironmentLogOption = LogOption.Realtime;
        }

        /// <summary>
        /// Trace a general server message for debugging purpose. 
        /// </summary>
        /// <param name="message">The trace message.</param>
        /// <param name="eventLevel">The event trace level. </param>
        /// <param name="correlationId">The request correlation id, if it exists. </param>
        /// <param name="trackingGuid">The request tracking guid, if it exists. </param>
        public static void TraceMessage(string message, EventLevel eventLevel, string correlationId = null, string trackingGuid = null)
        {
            ServerMessage serverMessage = new ServerMessage()
            {
                CorrelationId = correlationId,
                TrackingGuid = trackingGuid,
                Message = Masker.MaskSingle(message)
            };

            serverMessage.Log(eventLevel, EnvironmentLogOption);
        }

        public static void TraceMetrics(
            EventLevel eventLevel,
            string serviceName, 
            string metricsName, 
            string status, 
            string timestamp, 
            double quantity,
            string description = null,
            string message = null,
            string provider = null,
            string merchantId = null,
            string currency = null,
            string filename = null)
        {
            MetricsDetails metrics = new MetricsDetails()
            {
                ServiceName = serviceName,
                MetricsName = metricsName,
                Status = status,
                Timestamp = timestamp,
                Quantity = quantity,
                Description = description,
                Message = message,
                Provider = provider,
                MerchantId = merchantId,
                Currency = currency,
                IngestedFileName = filename
            };

            metrics.Log(eventLevel, EnvironmentLogOption);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", Justification = "wfcRequest not yet used")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", Justification = "wfcResponse not yet used")]
        public static void TraceServiceLoggingOutgoing(
            string dependencyServiceName,
            string operationName,
            string operationVersion,
            string remoteAddress,
            string protocol,
            object wfcRequest,
            object wfcResponse,
            string requestPayload,
            string responsePayload,
            int latencyMs,
            string requestTraceId,
            string serverTraceId,
            string message,
            string flightingExperimentId,
            CommerceServiceRequestStatus serviceRequestStatus)
        {
            TraceOutgoingServiceRequest(
                dependencyServiceName,
                dependencyServiceName,
                "Service",
                operationName,
                operationName,
                operationVersion,
                requestTraceId,
                serverTraceId,
                remoteAddress,
                protocol,
                serviceRequestStatus != CommerceServiceRequestStatus.ServiceError ? "200" : "500",
                "POST",
                "application/xml",
                null,
                null,
                requestPayload,
                responsePayload,
                string.IsNullOrEmpty(responsePayload) ? 0 : responsePayload.Length,
                latencyMs,
                message,
                flightingExperimentId,
                serviceRequestStatus != CommerceServiceRequestStatus.ServiceError,
                serviceRequestStatus);
        }

        public static void TraceOutgoingServiceRequest(
            string serviceName,
            string targetName,
            string targetType,
            string operationName,
            string targetOperationName,
            string targetOperationVersion,
            string requestTraceId,
            string serverTraceId,
            string targetUri,
            string protocol,
            string protocolStatusCode,
            string requestMethod,
            string responseContentType,
            string requestHeader,
            string responseHeader,
            object requestPayload,
            object responsePayload,
            int responseLength,
            int latencyMs,
            string message,
            string flightingExperimentId,
            bool? success,
            CommerceServiceRequestStatus serviceRequestStatus)
        {
            Ms.Qos.ServiceRequestStatus operationStatus;
            if (!Enum.TryParse(serviceRequestStatus.ToString(), out operationStatus))
            {
                operationStatus = Ms.Qos.ServiceRequestStatus.Undefined;
            }

            bool succeeded = success ?? operationStatus == Ms.Qos.ServiceRequestStatus.Success;

            ClientOperationDetails operationDetails = new ClientOperationDetails()
            {
                ServiceName = serviceName,
                RequestTraceId = requestTraceId,
                ServerTraceId = serverTraceId,
                RequestHeader = requestHeader,
                ResponseHeader = responseHeader,
                RequestDetails = Masker.MaskSingle((string)requestPayload),
                ResponseDetails = Masker.MaskSingle((string)responsePayload),
                Message = message,
                baseData =
                {
                    operationName = operationName,
                    latencyMs = latencyMs,
                    serviceErrorCode = 0,
                    protocol = protocol ?? string.Empty,
                    protocolStatusCode = protocolStatusCode ?? string.Empty,
                    requestMethod = requestMethod,
                    responseContentType = responseContentType,
                    requestStatus = operationStatus,
                    succeeded = succeeded,
                    targetUri = targetUri,
                    dependencyName = targetName,
                    dependencyOperationName = targetOperationName,
                    dependencyOperationVersion = targetOperationVersion,
                    dependencyType = targetType,
                    responseSizeBytes = responseLength,
                }
            };

            operationDetails.Log(
                succeeded ? EventLevel.Informational : EventLevel.Error,
                EnvironmentLogOption,
                (envelope) =>
                {
                    if (!string.IsNullOrEmpty(flightingExperimentId))
                    {
                        envelope.SetApp(new Telemetry.Extensions.app { expId = flightingExperimentId.ToString() });
                    }
                });
        }
    }
}
