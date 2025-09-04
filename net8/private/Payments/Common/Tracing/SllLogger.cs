namespace Microsoft.Commerce.Payments.Common.Tracing
{
    using System.Diagnostics.Tracing;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Ms.Qos;
    using OtelSllLogger = Microsoft.Commerce.Payments.Common.OpenTelemetry.SllLogger;

    /// <summary>
    /// Lightweight wrapper around OpenTelemetry SllLogger to preserve existing APIs
    /// while removing the dependency on Microsoft.Commerce.Tracing.Sll.
    /// </summary>
    public static class SllLogger
    {
        private const string SllLoggerMessageFormat =
            "{ActivityId} {RelatedActivityId} {Component} {EventName} {Message} {Parameters}";

        private static ILogger logger = LoggerFactory.Create(builder => { }).CreateLogger("SllLogger");

        public static LogOption EnvironmentLogOption { get; private set; } = LogOption.None;

        public static JsonDataMasker Masker { get; } = new JsonDataMasker();

        /// <summary>
        /// Allows consumers to provide their own logger instance.
        /// </summary>
        public static void Initialize(ILogger newLogger)
        {
            if (newLogger != null)
            {
                logger = newLogger;
            }
        }

        public static void SetRealtimeLogging() => EnvironmentLogOption = LogOption.Realtime;

        /// <summary>
        /// Trace a general server message for debugging purpose.
        /// </summary>
        public static void TraceMessage(string message, EventLevel eventLevel, string correlationId = null, string trackingGuid = null)
        {
            OtelSllLogger.TraceMessage(logger, message, eventLevel.ToQosEventLevel(), correlationId, trackingGuid);
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
            OtelSllLogger.TraceServiceLoggingOutgoing(
                logger,
                dependencyServiceName,
                operationName,
                operationVersion,
                remoteAddress,
                protocol,
                wfcRequest,
                wfcResponse,
                requestPayload,
                responsePayload,
                latencyMs,
                requestTraceId,
                serverTraceId,
                message,
                flightingExperimentId,
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
            OtelSllLogger.TraceOutgoingServiceRequest(
                logger,
                serviceName,
                targetName,
                targetType,
                operationName,
                targetOperationName,
                targetOperationVersion,
                requestTraceId,
                serverTraceId,
                targetUri,
                protocol,
                protocolStatusCode,
                requestMethod,
                responseContentType,
                requestHeader,
                responseHeader,
                requestPayload,
                responsePayload,
                responseLength,
                latencyMs,
                message,
                flightingExperimentId,
                success,
                serviceRequestStatus);
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
            var level = eventLevel.ToQosEventLevel();
            var payload = new
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

            switch (level)
            {
                case QosEventLevel.Error:
                    logger.LogError(SllLoggerMessageFormat, string.Empty, string.Empty, serviceName, metricsName, message ?? string.Empty, JsonConvert.SerializeObject(payload));
                    break;
                case QosEventLevel.Warning:
                    logger.LogWarning(SllLoggerMessageFormat, string.Empty, string.Empty, serviceName, metricsName, message ?? string.Empty, JsonConvert.SerializeObject(payload));
                    break;
                case QosEventLevel.Information:
                    logger.LogInformation(SllLoggerMessageFormat, string.Empty, string.Empty, serviceName, metricsName, message ?? string.Empty, JsonConvert.SerializeObject(payload));
                    break;
                default:
                    logger.LogTrace(SllLoggerMessageFormat, string.Empty, string.Empty, serviceName, metricsName, message ?? string.Empty, JsonConvert.SerializeObject(payload));
                    break;
            }
        }
    }
}
