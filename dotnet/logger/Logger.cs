// <copyright file="Logger.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.AspNetCore.Http;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Tracing;
    using Newtonsoft.Json;

    public static class Logger
    {
        public static IServiceLogger Log { get; private set; } = new ConsoleServiceLogger();

        public static IQosLogger Qos { get; private set; } = new ConsoleQosLogger();

        public static void Initialize(IServiceLogger serviceLogger, IQosLogger qosLogger)
        {
            Log = serviceLogger ?? new ConsoleServiceLogger();
            Qos = qosLogger ?? new ConsoleQosLogger();
        }

        public static string JsonSerialize(object parameters)
        {
            return JsonConvert.SerializeObject(parameters);
        }

        public static string FormatMessage(string format, params object[] parameters)
        {
            if (format == null)
            {
                return null;
            }

            try
            {
                return string.Format(format, parameters);
            }
            catch (FormatException)
            {
                return "Too few parameters for: " + format;
            }
        }

        private class ConsoleServiceLogger : IServiceLogger
        {
            public void LogActivityTransfer(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, EventTraceActivity relatedTraceActivityId, string message, string parameters)
            {
            }

            public void LogApplicationStart(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, string message, string parameters)
            {
            }

            public void LogApplicationStop(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, string message, string parameters)
            {
            }

            public void LogError(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, string message, string parameters)
            {
            }

            public void LogEvent(EventTraceActivity traceActivityId, string cV, string eventName, object parameters)
            {
            }

            public void LogInformational(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, string message, string parameters)
            {
            }

            public void LogMetric(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, string instance, double counterValue, bool absolute, string parameters)
            {
            }

            public void LogVerbose(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, string message, string parameters)
            {
            }

            public void LogWarning(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, string message, string parameters)
            {
            }
        }

        private class ConsoleQosLogger : IQosLogger
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

            public void TracePXServiceException(EventTraceActivity requestTraceId, string exceptionMessage)
            {
                LogMessage(new { exceptionMessage });
            }

            public void TracePXServiceIncomingOperation(string operationName, string accountId, string paymentInstrumentId, string paymentMethodFamily, string paymentMethodType, string country, HttpRequest request, HttpResponse response, string requestPayload, string responsePayload, string startTime, Stopwatch stopwatch, string requestTraceId, string authenticationInfo, string serverTraceId = null, string message = null, string errorCode = null, string errorMessage = null, bool isTest = false, string partner = null, string pidlOperation = null, string avsSuggest = null)
            {
                LogMessage(new { operationName, accountId, paymentInstrumentId, paymentMethodFamily, paymentMethodType, country, requestPayload, responsePayload, startTime, requestTraceId, authenticationInfo, serverTraceId, message, errorCode, errorMessage, isTest, partner, pidlOperation, avsSuggest });
            }

            public void TracePXServiceIntegrationError(string serviceName, IntegrationErrorCode integrationErrorCode, string message, string requestTraceId, string serverTraceId = null, string correlationVector = null)
            {
                LogMessage(new { serviceName, message, requestTraceId, serverTraceId, correlationVector });
            }

            public void TracePXServiceOutgoingOperation(string operationName, string serviceName, HttpRequest request, HttpResponse response, string requestPayload, string responsePayload, string startTime, Stopwatch stopwatch, string requestTraceId, string message, string certInfo, string servicePointData = null)
            {
                LogMessage(new { operationName, serviceName, requestPayload, responsePayload, startTime, requestTraceId, message, certInfo, servicePointData });
            }

            public void TracePXServiceOutgoingOperation(string operationName, string serviceName, string targetUri, string requestPayload, string responsePayload, string startTime, int latencyMs, string requestTraceId, string correlationVector, bool isSucceeded, string message, string certInfo)
            {
                LogMessage(new { operationName, serviceName, targetUri, requestPayload, responsePayload, startTime, latencyMs, requestTraceId, correlationVector, isSucceeded, message, certInfo });
            }

            public void TracePXServicePIAddedOnOffer(string serviceName, HttpRequest request, string requestTraceId, string paymentInstrumentId, string paymentMethodFamily, string paymentMethodType, string partner, string country, string offerId, string puid)
            {
                LogMessage(new { serviceName, requestTraceId, paymentInstrumentId, paymentMethodFamily, paymentMethodType, partner, country, offerId, puid });
            }

            public void TraceServerMessage(string serviceName, string correlationId, string trackingGuid, string message, QosEventLevel eventLevel)
            {
                LogMessage(new { serviceName, correlationId, trackingGuid, message, eventLevel });
            }

            public void TraceServiceLoggingIncoming(string operationName, HttpRequest request, HttpResponse response, string requestPayload, string responsePayload, int latencyMs, string requestTraceId, string serverTraceId, string message)
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
                try
                {
                    Console.WriteLine("{0}: {1}", callingFunction, JsonSerialize(parameters));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in {0}: {1}", callingFunction, ex.Message);
                }
            }
        }
    }
}
