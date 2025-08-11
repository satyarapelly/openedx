// <copyright file="Logger.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>



namespace Microsoft.Commerce.Payments.Common.Tracing
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Tracing;
    using Newtonsoft.Json;
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Runtime.CompilerServices;

    public class ConsoleQosLogger : IQosLogger
    {
        public void TraceMessage(string message, QosEventLevel eventLevel, string correlationId = null, string trackingGuid = null)
        {
            LogMessage(new { message, eventLevel, correlationId, trackingGuid });
        }

        public void TraceMISETokenValidationResult(bool succeed, string applicationId, string errorCode, string cloudInstance, string message, long latency, string cV, Exception ex)
        {
            LogMessage(new { succeed, applicationId, errorCode, cloudInstance, message, latency, cV });
        }

        public void TraceOutgoingServiceRequest(string serviceName, string targetName, string targetType, string operationName, string targetOperationName, string targetOperationVersion, string requestTraceId, string serverTraceId, string targetUri, string protocol, string protocolStatusCode, string requestMethod, string responseContentType, string requestHeader, string responseHeader, object requestPayload, object responsePayload, int responseLength, int latencyMs, string message, string flightingExperimentId, bool? success, CommerceServiceRequestStatus serviceRequestStatus)
        {
            LogMessage(new { serviceName, targetName, targetType, operationName, targetOperationName, targetOperationVersion, requestTraceId, serverTraceId, targetUri, protocol, protocolStatusCode, requestMethod, responseContentType, requestHeader, responseHeader, responseLength, latencyMs, message, flightingExperimentId, success, serviceRequestStatus });
        }

        public void TracePXServiceException(string exceptionMessage, EventTraceActivity requestTraceId)
        {
            LogMessage(new { exceptionMessage });
        }

        public void TracePXServiceIncomingOperation(string operationName, string accountId, string paymentInstrumentId, string paymentMethodFamily, string paymentMethodType, string country, HttpRequestMessage request, HttpResponseMessage response, string requestPayload, string responsePayload, string startTime, Stopwatch stopwatch, string requestTraceId, string authenticationInfo, string serverTraceId = null, string message = null, string errorCode = null, string errorMessage = null, bool isTest = false, string partner = null, string pidlOperation = null, string avsSuggest = null)
        {
            LogMessage(new { operationName, accountId, paymentInstrumentId, paymentMethodFamily, paymentMethodType, country, requestPayload, responsePayload, startTime, requestTraceId, authenticationInfo, serverTraceId, message, errorCode, errorMessage, isTest, partner, pidlOperation, avsSuggest });
        }

        public void TracePXServiceIntegrationError(string serviceName, IntegrationErrorCode integrationErrorCode, string message, string requestTraceId, string serverTraceId = null, string correlationVector = null)
        {
            LogMessage(new { serviceName, message, requestTraceId, serverTraceId, correlationVector });
        }

        public void TracePXServiceOutgoingOperation(string operationName, string serviceName, HttpRequestMessage request, HttpResponseMessage response, string requestPayload, string responsePayload, string startTime, Stopwatch stopwatch, string requestTraceId, string message, string certInfo, string servicePointData = null)
        {
            LogMessage(new { operationName, serviceName, requestPayload, responsePayload, startTime, requestTraceId, message, certInfo, servicePointData });
        }

        public void TracePXServiceOutgoingOperation(string operationName, string serviceName, string targetUri, string requestPayload, string responsePayload, string startTime, int latencyMs, string requestTraceId, string correlationVector, bool isSucceeded, string message, string certInfo)
        {
            LogMessage(new { operationName, serviceName, targetUri, requestPayload, responsePayload, startTime, latencyMs, requestTraceId, correlationVector, isSucceeded, message, certInfo });
        }

        public void TracePXServicePIAddedOnOffer(string serviceName, HttpRequestMessage request, string requestTraceId, string paymentInstrumentId, string paymentMethodFamily, string paymentMethodType, string partner, string country, string offerId, string puid)
        {
            LogMessage(new { serviceName, requestTraceId, paymentInstrumentId, paymentMethodFamily, paymentMethodType, partner, country, offerId, puid });
        }

        public void TraceServerMessage(string serviceName, string correlationId, string trackingGuid, string message, QosEventLevel eventLevel)
        {
            LogMessage(new { serviceName, correlationId, trackingGuid, message, eventLevel });
        }

        public void TraceServiceLoggingIncoming(string operationName, HttpRequestMessage request, HttpResponseMessage response, string requestPayload, string responsePayload, int latencyMs, string requestTraceId, string serverTraceId, string message)
        {
            LogMessage(new { operationName, requestPayload, responsePayload, latencyMs, requestTraceId, serverTraceId, message });
        }

        public void TraceServiceLoggingOutgoing(string dependencyServiceName, string operationName, string operationVersion, string remoteAddress, string protocol, object wfcRequest, object wfcResponse, string requestPayload, string responsePayload, int latencyMs, string requestTraceId, string serverTraceId, string message, string flightingExperimentId, CommerceServiceRequestStatus serviceRequestStatus)
        {
            LogMessage(new { dependencyServiceName, operationName, operationVersion, remoteAddress, protocol, requestPayload, responsePayload, latencyMs, requestTraceId, serverTraceId, message, flightingExperimentId, serviceRequestStatus });
        }

        public void TraceTokenGenerationResult(bool succeed, string resource, string clientId, long latency, string cV, Exception ex, string expiresOn = null)
        {
            LogMessage(new { succeed, resource, clientId, latency, cV, expiresOn });
        }

        private static void LogMessage(object parameters, [CallerMemberName] string callingFunction = null)
        {
            Console.WriteLine("{0}: {1}", callingFunction, JsonSerialize(parameters));
        }
        public static string JsonSerialize(object parameters)
        {
            return JsonConvert.SerializeObject(parameters);
        }

    }
}

