// <copyright file="SllLogger.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.OpenTelemetry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// The class of SllLogger.
    /// </summary>
    public static class SllLogger
    {
        private const string SllLoggerMessageFormat =
          "{ActivityId} {RelatedActivityId} {Component} {EventName} {Message} {Parameters}";

        public static JsonDataMasker Masker { get; } = new JsonDataMasker();

        /// <summary>
        /// Trace a general server message for debugging purpose.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="message">The trace message.</param>
        /// <param name="eventLevel">The event trace level. </param>
        /// <param name="correlationId">The request correlation id, if it exists. </param>
        /// <param name="trackingGuid">The request tracking guid, if it exists. </param>
        public static void TraceMessage(ILogger logger, string message, QosEventLevel eventLevel, string correlationId = null, string trackingGuid = null)
        {
            switch (eventLevel)
            {
                case QosEventLevel.Error:
                    logger.LogError(SllLoggerMessageFormat, trackingGuid, correlationId, string.Empty, "BaseError", Masker.MaskSingle(message), string.Empty);
                    break;
                case QosEventLevel.Information:
                    logger.LogInformation(SllLoggerMessageFormat, trackingGuid, correlationId, string.Empty, "BaseInformational", Masker.MaskSingle(message), string.Empty);
                    break;
                case QosEventLevel.Warning:
                    logger.LogWarning(SllLoggerMessageFormat, trackingGuid, correlationId, string.Empty, "BaseWarning", Masker.MaskSingle(message), string.Empty);
                    break;
                default:
                    logger.LogTrace(SllLoggerMessageFormat, trackingGuid, correlationId, string.Empty, "TraceMessage", Masker.MaskSingle(message), string.Empty);
                    break;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", Justification = "wfcRequest not yet used")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", Justification = "wfcResponse not yet used")]
        public static void TraceServiceLoggingOutgoing(
            ILogger logger,
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
                logger,
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
            ILogger logger,
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

            var operationDetails = new
            {
                ServiceName = serviceName,
                RequestTraceId = requestTraceId,
                ServerTraceId = serverTraceId,
                RequestHeader = requestHeader,
                ResponseHeader = responseHeader,
                RequestDetails = Masker.MaskSingle((string)requestPayload),
                ResponseDetails = Masker.MaskSingle((string)responsePayload),
                Message = message,
                baseData = new
                {
                    operationName,
                    latencyMs,
                    serviceErrorCode = 0,
                    protocol = protocol ?? string.Empty,
                    protocolStatusCode = protocolStatusCode ?? string.Empty,
                    requestMethod,
                    responseContentType,
                    requestStatus = operationStatus,
                    succeeded,
                    targetUri,
                    dependencyName = targetName,
                    dependencyOperationName = targetOperationName,
                    dependencyOperationVersion = targetOperationVersion,
                    dependencyType = targetType,
                    responseSizeBytes = responseLength,
                },
                flightingExperimentId
            };

            string serializedOperationDetails;
            try
            {
                serializedOperationDetails = JsonConvert.SerializeObject(operationDetails);
            }
            catch (JsonSerializationException ex)
            {
                logger.LogError("Serialization failed for operation details: {0}", ex.Message);
                serializedOperationDetails = "SerializationError";
            }

            if (succeeded)
            {
                logger.LogInformation(SllLoggerMessageFormat, requestTraceId, serverTraceId, serviceName, "Microsoft.Commerce.Tracing.Sll.ClientOperationDetails", message, serializedOperationDetails);
            }
            else
            {
                logger.LogError(SllLoggerMessageFormat, requestTraceId, serverTraceId, serviceName, "Microsoft.Commerce.Tracing.Sll.ClientOperationDetails", message, serializedOperationDetails);
            }
        }
    }
}
