// <copyright file="OpenTelemetryQosLogger.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

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
    using Microsoft.Commerce.Tracing;
    using Microsoft.Extensions.Logging;

    public class OpenTelemetryQosLogger : IQosLogger
    {
        private readonly List<IQosLogger> loggers;

        public OpenTelemetryQosLogger(params IQosLogger[] loggers)
        {
            this.loggers = new List<IQosLogger>(loggers.Where(l => l != null));
        }

        public static OpenTelemetryQosLogger Create(ILogger otelLogger)
        {
            OpenTelemetryQosLogger logger = new OpenTelemetryQosLogger(new SllEtwLogger(otelLogger));
            return logger;
        }

        public void TraceMessage(string message, QosEventLevel eventLevel, string correlationId = null, string trackingGuid = null)
        {
            foreach (IQosLogger logger in this.loggers)
            {
                logger.TraceMessage(message, eventLevel, correlationId, trackingGuid);
            }
        }

        public void TraceMISETokenValidationResult(bool succeed, string applicationId, string errorCode, string cloudInstance, string message, long latency, string cV, Exception ex)
        {
            foreach (IQosLogger logger in this.loggers)
            {
                logger.TraceMISETokenValidationResult(succeed, applicationId, errorCode, cloudInstance, message, latency, cV, ex);
            }
        }

        public void TraceOutgoingServiceRequest(string serviceName, string targetName, string targetType, string operationName, string targetOperationName, string targetOperationVersion, string requestTraceId, string serverTraceId, string targetUri, string protocol, string protocolStatusCode, string requestMethod, string responseContentType, string requestHeader, string responseHeader, object requestPayload, object responsePayload, int responseLength, int latencyMs, string message, string flightingExperimentId, bool? success, CommerceServiceRequestStatus serviceRequestStatus)
        {
            foreach (IQosLogger logger in this.loggers)
            {
                logger.TraceOutgoingServiceRequest(serviceName, targetName, targetType, operationName, targetOperationName, targetOperationVersion, requestTraceId, serverTraceId, targetUri, protocol, protocolStatusCode, requestMethod, responseContentType, requestHeader, responseHeader, requestPayload, responsePayload, responseLength, latencyMs, message, flightingExperimentId, success, serviceRequestStatus);
            }
        }

        public void TracePXServiceException(string exceptionMessage, EventTraceActivity requestTraceId)
        {
            foreach (IQosLogger logger in this.loggers)
            {
                logger.TracePXServiceException(exceptionMessage, requestTraceId);
            }
        }

        public void TracePXServiceIncomingOperation(string operationName, string accountId, string paymentInstrumentId, string paymentMethodFamily, string paymentMethodType, string country, HttpRequestMessage request, HttpResponseMessage response, string requestPayload, string responsePayload, string startTime, Stopwatch stopwatch, string requestTraceId, string authenticationInfo, string serverTraceId = null, string message = null, string errorCode = null, string errorMessage = null, bool isTest = false, string partner = null, string pidlOperation = null, string avsSuggest = null)
        {
            foreach (IQosLogger logger in this.loggers)
            {
                logger.TracePXServiceIncomingOperation(operationName, accountId, paymentInstrumentId, paymentMethodFamily, paymentMethodType, country, request, response, requestPayload, responsePayload, startTime, stopwatch, requestTraceId, authenticationInfo, serverTraceId, message, errorCode, errorMessage, isTest, partner, pidlOperation, avsSuggest);
            }
        }

        public void TracePXServiceIntegrationError(string serviceName, IntegrationErrorCode integrationErrorCode, string message, string requestTraceId, string serverTraceId = null, string correlationVector = null)
        {
            foreach (IQosLogger logger in this.loggers)
            {
                logger.TracePXServiceIntegrationError(serviceName, integrationErrorCode, message, requestTraceId, serverTraceId, correlationVector);
            }
        }

        public void TracePXServiceOutgoingOperation(string operationName, string serviceName, HttpRequestMessage request, HttpResponseMessage response, string requestPayload, string responsePayload, string startTime, Stopwatch stopwatch, string requestTraceId, string message, string certInfo, string servicePointData = null)
        {
            foreach (IQosLogger logger in this.loggers)
            {
                logger.TracePXServiceOutgoingOperation(operationName, serviceName, request, response, requestPayload, responsePayload, startTime, stopwatch, requestTraceId, message, certInfo, servicePointData);
            }
        }

        public void TracePXServiceOutgoingOperation(string operationName, string serviceName, string targetUri, string requestPayload, string responsePayload, string startTime, int latencyMs, string requestTraceId, string correlationVector, bool isSucceeded, string message, string certInfo)
        {
            foreach (IQosLogger logger in this.loggers)
            {
                logger.TracePXServiceOutgoingOperation(operationName, serviceName, targetUri, requestPayload, responsePayload, startTime, latencyMs, requestTraceId, correlationVector, isSucceeded, message, certInfo);
            }
        }

        public void TracePXServicePIAddedOnOffer(string serviceName, HttpRequestMessage request, string requestTraceId, string paymentInstrumentId, string paymentMethodFamily, string paymentMethodType, string partner, string country, string offerId, string puid)
        {
            foreach (IQosLogger logger in this.loggers)
            {
                logger.TracePXServicePIAddedOnOffer(serviceName, request, requestTraceId, paymentInstrumentId, paymentMethodFamily, paymentMethodType, partner, country, offerId, puid);
            }
        }

        public void TraceServerMessage(string serviceName, string correlationId, string trackingGuid, string message, QosEventLevel eventLevel)
        {
            foreach (IQosLogger logger in this.loggers)
            {
                logger.TraceServerMessage(serviceName, correlationId, trackingGuid, message, eventLevel);
            }
        }

        public void TraceServiceLoggingIncoming(string operationName, HttpRequestMessage request, HttpResponseMessage response, string requestPayload, string responsePayload, int latencyMs, string requestTraceId, string serverTraceId, string message)
        {
            foreach (IQosLogger logger in this.loggers)
            {
                logger.TraceServiceLoggingIncoming(operationName, request, response, requestPayload, responsePayload, latencyMs, requestTraceId, serverTraceId, message);
            }
        }

        public void TraceServiceLoggingOutgoing(string dependencyServiceName, string operationName, string operationVersion, string remoteAddress, string protocol, object wfcRequest, object wfcResponse, string requestPayload, string responsePayload, int latencyMs, string requestTraceId, string serverTraceId, string message, string flightingExperimentId, CommerceServiceRequestStatus serviceRequestStatus)
        {
            foreach (IQosLogger logger in this.loggers)
            {
                logger.TraceServiceLoggingOutgoing(dependencyServiceName, operationName, operationVersion, remoteAddress, protocol, wfcRequest, wfcResponse, requestPayload, responsePayload, latencyMs, requestTraceId, serverTraceId, message, flightingExperimentId, serviceRequestStatus);
            }
        }

        public void TraceTokenGenerationResult(bool succeed, string resource, string clientId, long latency, string cV, Exception ex, string expiresOn = null)
        {
            foreach (IQosLogger logger in this.loggers)
            {
                logger.TraceTokenGenerationResult(succeed, resource, clientId, latency, cV, ex, expiresOn);
            }
        }
    }
}
