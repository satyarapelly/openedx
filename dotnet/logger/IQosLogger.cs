// <copyright file="IQosLogger.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Commerce.Payments.Common.Web;

    public interface IQosLogger
    {
        void TraceMessage(string message, QosEventLevel eventLevel, string correlationId = null, string trackingGuid = null);

        [SuppressMessage("Microsoft.Usage", "CA1801", Justification = "wfcRequest not yet used")]
        [SuppressMessage("Microsoft.Usage", "CA1801", Justification = "wfcResponse not yet used")]
        void TraceServiceLoggingOutgoing(
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
           CommerceServiceRequestStatus serviceRequestStatus);

        void TraceOutgoingServiceRequest(
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
            CommerceServiceRequestStatus serviceRequestStatus);

        void TraceServerMessage(
            string serviceName,
            string correlationId,
            string trackingGuid,
            string message,
            QosEventLevel eventLevel);

        void TraceServiceLoggingIncoming(
            string operationName,
            HttpRequest request,
            HttpResponse response,
            string requestPayload,
            string responsePayload,
            int latencyMs,
            string requestTraceId,
            string serverTraceId,
            string message);

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Easy to understand and cannot simplify more.")]
        void TracePXServiceIncomingOperation(
          string operationName,
          string accountId,
          string paymentInstrumentId,
          string paymentMethodFamily,
          string paymentMethodType,
          string country,
          HttpRequest request,
          HttpResponse response,
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
          string avsSuggest = null);

        void TracePXServiceOutgoingOperation(
           string operationName,
           string serviceName,
           HttpRequest request,
           HttpResponse response,
           string requestPayload,
           string responsePayload,
           string startTime,
           Stopwatch stopwatch,
           string requestTraceId,
           string message,
           string certInfo,
           string servicePointData = null);

        void TracePXServiceOutgoingOperation(
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
           string certInfo);

        void TracePXServicePIAddedOnOffer(
           string serviceName,
           HttpRequest request,
           string requestTraceId,
           string paymentInstrumentId,
           string paymentMethodFamily,
           string paymentMethodType,
           string partner,
           string country,
           string offerId,
           string puid);

        void TracePXServiceIntegrationError(
            string serviceName,
            IntegrationErrorCode integrationErrorCode,
            string message,
            string requestTraceId,
            string serverTraceId = null,
            string correlationVector = null);

        void TracePXServiceException(
            EventTraceActivity requestTraceId,
            string exceptionMessage);

        void TraceMISETokenValidationResult(
            bool succeed,
            string applicationId,
            string errorCode,
            string cloudInstance,
            string message,
            long latency,
            string cV,
            Exception ex);

        void TraceTokenGenerationResult(
           bool succeed,
           string resource,
           string clientId,
           long latency,
           string cV,
           Exception ex,
           string expiresOn = null);
    }
}
