// <copyright file="SllEtwLogger.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.OpenTelemetry
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Extensions.Logging;

    public class SllEtwLogger : IQosLogger
    {
        private readonly ILogger logger;

        internal SllEtwLogger(ILogger logger)
        {
            this.logger = logger;
        }

        public void TraceMessage(string message, QosEventLevel eventLevel, string correlationId = null, string trackingGuid = null)
        {
            SllLogger.TraceMessage(this.logger, message, eventLevel, correlationId, trackingGuid);
        }

        public void TraceMISETokenValidationResult(bool succeed, string applicationId, string errorCode, string cloudInstance, string message, long latency, string cV, Exception ex)
        {
            SllWebLogger.TraceMISETokenValidationResult(this.logger, succeed, applicationId, errorCode, cloudInstance, message, latency, cV, ex);
        }

        public void TraceOutgoingServiceRequest(string serviceName, string targetName, string targetType, string operationName, string targetOperationName, string targetOperationVersion, string requestTraceId, string serverTraceId, string targetUri, string protocol, string protocolStatusCode, string requestMethod, string responseContentType, string requestHeader, string responseHeader, object requestPayload, object responsePayload, int responseLength, int latencyMs, string message, string flightingExperimentId, bool? success, CommerceServiceRequestStatus serviceRequestStatus)
        {
            SllLogger.TraceOutgoingServiceRequest(this.logger, serviceName, targetName, targetType, operationName, targetOperationName, targetOperationVersion, requestTraceId, serverTraceId, targetUri, protocol, protocolStatusCode, requestMethod, responseContentType, requestHeader, responseHeader, requestPayload, responsePayload, responseLength, latencyMs, message, flightingExperimentId, success, serviceRequestStatus);
        }

        public void TracePXServiceException(string exceptionMessage, EventTraceActivity requestTraceId)
        {
            SllWebLogger.TracePXServiceException(this.logger, exceptionMessage, requestTraceId);
        }

        public void TracePXServiceIncomingOperation(string operationName, string accountId, string paymentInstrumentId, string paymentMethodFamily, string paymentMethodType, string country, HttpRequestMessage request, HttpResponseMessage response, string requestPayload, string responsePayload, string startTime, Stopwatch stopwatch, string requestTraceId, string authenticationInfo, string serverTraceId = null, string message = null, string errorCode = null, string errorMessage = null, bool isTest = false, string partner = null, string pidlOperation = null, string avsSuggest = null)
        {
            SllWebLogger.TracePXServiceIncomingOperation(this.logger, operationName, accountId, paymentInstrumentId, paymentMethodFamily, paymentMethodType, country, request, response, requestPayload, responsePayload, startTime, stopwatch, requestTraceId, authenticationInfo, serverTraceId, message, errorCode, errorMessage, isTest, partner, pidlOperation, avsSuggest);
        }

        public void TracePXServiceIntegrationError(string serviceName, IntegrationErrorCode integrationErrorCode, string message, string requestTraceId, string serverTraceId = null, string correlationVector = null)
        {
            SllWebLogger.TracePXServiceIntegrationError(this.logger, serviceName, integrationErrorCode, message, requestTraceId, serverTraceId, correlationVector);
        }

        public void TracePXServiceOutgoingOperation(string operationName, string serviceName, HttpRequestMessage request, HttpResponseMessage response, string requestPayload, string responsePayload, string startTime, Stopwatch stopwatch, string requestTraceId, string message, string certInfo, string servicePointData = null)
        {
            SllWebLogger.TracePXServiceOutgoingOperation(this.logger, operationName, serviceName, request, response, requestPayload, responsePayload, startTime, stopwatch, requestTraceId, message, certInfo, servicePointData);
        }

        public void TracePXServiceOutgoingOperation(string operationName, string serviceName, string targetUri, string requestPayload, string responsePayload, string startTime, int latencyMs, string requestTraceId, string correlationVector, bool isSucceeded, string message, string certInfo)
        {
            SllWebLogger.TracePXServiceOutgoingOperation(this.logger, operationName, serviceName, targetUri, requestPayload, responsePayload, startTime, latencyMs, requestTraceId, correlationVector, isSucceeded, message, certInfo);
        }

        public void TracePXServicePIAddedOnOffer(string serviceName, HttpRequestMessage request, string requestTraceId, string paymentInstrumentId, string paymentMethodFamily, string paymentMethodType, string partner, string country, string offerId, string puid)
        {
            SllWebLogger.TracePXServicePIAddedOnOffer(this.logger, serviceName, request, requestTraceId, paymentInstrumentId, paymentMethodFamily, paymentMethodType, partner, country, offerId, puid);
        }

        public void TraceServerMessage(string serviceName, string correlationId, string trackingGuid, string message, QosEventLevel eventLevel)
        {
            SllWebLogger.TraceServerMessage(this.logger, serviceName, correlationId, trackingGuid, message, eventLevel);
        }

        public void TraceServiceLoggingIncoming(string operationName, HttpRequestMessage request, HttpResponseMessage response, string requestPayload, string responsePayload, int latencyMs, string requestTraceId, string serverTraceId, string message)
        {
            SllWebLogger.TraceServiceLoggingIncoming(this.logger, operationName, request, response, requestPayload, responsePayload, latencyMs, requestTraceId, serverTraceId, message);
        }

        public void TraceServiceLoggingOutgoing(string dependencyServiceName, string operationName, string operationVersion, string remoteAddress, string protocol, object wfcRequest, object wfcResponse, string requestPayload, string responsePayload, int latencyMs, string requestTraceId, string serverTraceId, string message, string flightingExperimentId, CommerceServiceRequestStatus serviceRequestStatus)
        {
            SllLogger.TraceServiceLoggingOutgoing(this.logger, dependencyServiceName, operationName, operationVersion, remoteAddress, protocol, wfcRequest, wfcResponse, requestPayload, responsePayload, latencyMs, requestTraceId, serverTraceId, message, flightingExperimentId, serviceRequestStatus);
        }

        public void TraceTokenGenerationResult(bool succeed, string resource, string clientId, long latency, string cV, Exception ex, string expiresOn = null)
        {
            SllWebLogger.TraceTokenGenerationResult(this.logger, succeed, resource, clientId, latency, cV, ex, expiresOn);
        }
    }
}
