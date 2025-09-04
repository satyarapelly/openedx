namespace Microsoft.Commerce.Payments.Common.Web
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Tracing;
    using System.Net.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Ms.Qos;
    using Newtonsoft.Json;
    using OtelSllWebLogger = Microsoft.Commerce.Payments.Common.OpenTelemetry.SllWebLogger;

    /// <summary>
    /// Wrapper around OpenTelemetry SllWebLogger to preserve existing APIs
    /// while dropping dependency on Microsoft.Commerce.Tracing.Sll.
    /// </summary>
    public static class SllWebLogger
    {
        private const string SllWebLoggerLogMessageFormat =
            "{ActivityId} {RelatedActivityId} {CV} {Component} {ComponentEventName} {EventName} {Message} {Parameters}";

        private static ILogger logger = LoggerFactory.Create(builder => { }).CreateLogger("SllWebLogger");

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

        public static void TraceServerMessage(
            string serviceName,
            string correlationId,
            string trackingGuid,
            string message,
            EventLevel eventLevel)
        {
            OtelSllWebLogger.TraceServerMessage(logger, serviceName, correlationId, trackingGuid, message, eventLevel.ToQosEventLevel());
        }

        public static void TraceServiceLoggingIncoming(
            string operationName,
            HttpRequestMessage request,
            HttpResponseMessage response,
            string requestPayload,
            string responsePayload,
            int latencyMs,
            string requestTraceId,
            string serverTraceId,
            string message)
        {
            OtelSllWebLogger.TraceServiceLoggingIncoming(logger, operationName, request, response, requestPayload, responsePayload, latencyMs, requestTraceId, serverTraceId, message);
        }

        public static void TracePXServiceIncomingOperation(
            string operationName,
            string accountId,
            string paymentInstrumentId,
            string paymentMethodFamily,
            string paymentMethodType,
            string country,
            HttpRequestMessage request,
            HttpResponseMessage response,
            string requestPayload,
            string responsePayload,
            string startTime,
            Stopwatch stopwatch,
            string requestTraceId,
            string authenticationInfo,
            string serverTraceId = null,
            string message = null,
            string errorCode = null,
            string errorMessage = null,
            bool isTest = false,
            string partner = null,
            string pidlOperation = null,
            string avsSuggest = null)
        {
            OtelSllWebLogger.TracePXServiceIncomingOperation(
                logger,
                operationName,
                accountId,
                paymentInstrumentId,
                paymentMethodFamily,
                paymentMethodType,
                country,
                request,
                response,
                requestPayload,
                responsePayload,
                startTime,
                stopwatch,
                requestTraceId,
                authenticationInfo,
                serverTraceId,
                message,
                errorCode,
                errorMessage,
                isTest,
                partner,
                pidlOperation,
                avsSuggest);
        }

        public static void TracePXServiceOutgoingOperation(
            string operationName,
            string serviceName,
            HttpRequestMessage request,
            HttpResponseMessage response,
            string requestPayload,
            string responsePayload,
            string startTime,
            Stopwatch stopwatch,
            string requestTraceId,
            string message,
            string certInfo,
            string servicePointData = null)
        {
            OtelSllWebLogger.TracePXServiceOutgoingOperation(
                logger,
                operationName,
                serviceName,
                request,
                response,
                requestPayload,
                responsePayload,
                startTime,
                stopwatch,
                requestTraceId,
                message,
                certInfo,
                servicePointData);
        }

        public static void TracePXServiceOutgoingOperation(
            string operationName,
            string serviceName,
            string targetUri,
            string requestPayload,
            string responsePayload,
            string startTime,
            int latencyMs,
            string requestTraceId,
            string correlationVector,
            bool isSucceeded,
            string message,
            string certInfo)
        {
            OtelSllWebLogger.TracePXServiceOutgoingOperation(
                logger,
                operationName,
                serviceName,
                targetUri,
                requestPayload,
                responsePayload,
                startTime,
                latencyMs,
                requestTraceId,
                correlationVector,
                isSucceeded,
                message,
                certInfo);
        }

        public static void TracePXServicePIAddedOnOffer(
            string serviceName,
            HttpRequestMessage request,
            string requestTraceId,
            string paymentInstrumentId,
            string paymentMethodFamily,
            string paymentMethodType,
            string partner,
            string country,
            string offerId,
            string puid)
        {
            OtelSllWebLogger.TracePXServicePIAddedOnOffer(
                logger,
                serviceName,
                request,
                requestTraceId,
                paymentInstrumentId,
                paymentMethodFamily,
                paymentMethodType,
                partner,
                country,
                offerId,
                puid);
        }

        public static void TracePXServiceIntegrationError(
            string serviceName,
            IntegrationErrorCode integrationErrorCode,
            string message,
            string requestTraceId,
            string serverTraceId = null,
            string correlationVector = null)
        {
            OtelSllWebLogger.TracePXServiceIntegrationError(logger, serviceName, integrationErrorCode, message, requestTraceId, serverTraceId, correlationVector);
        }

        public static void TracePXServiceException(string exceptionMessage, EventTraceActivity requestTraceId)
        {
            OtelSllWebLogger.TracePXServiceException(logger, exceptionMessage, requestTraceId);
        }

        public static void TraceMISETokenValidationResult(
            bool succeed,
            string applicationId,
            string errorCode,
            string cloudInstance,
            string message,
            long latency,
            string cV,
            Exception ex)
        {
            OtelSllWebLogger.TraceMISETokenValidationResult(logger, succeed, applicationId, errorCode, cloudInstance, message, latency, cV, ex);
        }

        public static void TraceTokenGenerationResult(
            bool succeed,
            string resource,
            string clientId,
            long latency,
            string cV,
            Exception ex,
            string expiresOn = null)
        {
            OtelSllWebLogger.TraceTokenGenerationResult(logger, succeed, resource, clientId, latency, cV, ex, expiresOn);
        }

        public static void DatabaseActionResult(
            bool success,
            string databaseName,
            string containerName,
            string action,
            Exception ex = null)
        {
            var result = new
            {
                Success = success,
                DatabaseName = databaseName,
                ContainerName = containerName,
                Action = action,
                Exception = ex?.ToString()
            };

            var level = success ? LogLevel.Information : LogLevel.Error;
            logger.Log(level, SllWebLoggerLogMessageFormat, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "Microsoft.Commerce.Tracing.Sll.DatabaseActionResult", string.Empty, JsonConvert.SerializeObject(result));
        }
    }
}
