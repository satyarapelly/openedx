// <copyright file="SllWebLogger.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.OpenTelemetry
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Tracing.Sll;
    using Microsoft.CommonSchema.Services.Logging;
    using Microsoft.Extensions.Logging;
    using Ms.Qos;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Tracing;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;

    public static class SllWebLogger
    {
        private const string SllWebLoggerLogMessageFormat =
           "{ActivityId} {RelatedActivityId} {CV} {Component} {ComponentEventName} {EventName} {Message} {Parameters}";

        private const int MaxResponseLength = 26500;
        private const int MaxAuthenticationInfoLength = 26500;

        public static void TraceServerMessage(
            ILogger logger,
            string serviceName,
            string correlationId,
            string trackingGuid,
            string message,
            QosEventLevel eventLevel)
        {
            switch (eventLevel)
            {
                case QosEventLevel.Error:
                    logger.LogError(SllWebLoggerLogMessageFormat, correlationId, trackingGuid, string.Empty, serviceName, string.Empty, "BaseError", message, string.Empty);
                    break;
                case QosEventLevel.Information:
                    logger.LogInformation(SllWebLoggerLogMessageFormat, correlationId, trackingGuid, string.Empty, serviceName, string.Empty, "BaseInformational", message, string.Empty);
                    break;
                case QosEventLevel.Warning:
                    logger.LogWarning(SllWebLoggerLogMessageFormat, correlationId, trackingGuid, string.Empty, serviceName, string.Empty, "BaseWarning", message, string.Empty);
                    break;
                default:
                    logger.LogTrace(SllWebLoggerLogMessageFormat, correlationId, trackingGuid, string.Empty, serviceName, string.Empty, "TraceMessage", message, string.Empty);
                    break;
            }
        }

        public static void TraceServiceLoggingIncoming(
            ILogger logger,
            string operationName,
            HttpRequest request,
            HttpResponse response,
            string requestPayload,
            string responsePayload,
            int latencyMs,
            string requestTraceId,
            string serverTraceId,
            string message)
        {
            var httpContext = request.HttpContext;

            string apiExternalVersion = httpContext.Items.TryGetValue(PaymentConstants.Web.Properties.Version, out var versionObj)
                && versionObj is ApiVersion version ? version.ExternalVersion : string.Empty;

            httpContext.Items.TryGetValue(PaymentConstants.Web.Properties.CallerName, out var callerObject);
            httpContext.Items.TryGetValue(PaymentConstants.Web.Properties.CallerThumbprint, out var callerThumbprint);
            httpContext.Items.TryGetValue(PaymentConstants.Web.Properties.FlightingExperimentId, out var flightingExperimentId);
            httpContext.Items.TryGetValue(PaymentConstants.Web.Properties.ScenarioId, out var scenarioId);

            var operationStatus = Ms.Qos.ServiceRequestStatus.Undefined;
            if (response != null)
            {
                var statusCode = response.StatusCode;
                if ((int)statusCode >= 200 && (int)statusCode <= 299 || statusCode == StatusCodes.Status303SeeOther)
                {
                    operationStatus = Ms.Qos.ServiceRequestStatus.Success;
                }
                else if ((int)statusCode >= 400 && (int)statusCode <= 499)
                {
                    operationStatus = Ms.Qos.ServiceRequestStatus.CallerError;
                }
                else if (statusCode == StatusCodes.Status502BadGateway)
                {
                    operationStatus = Ms.Qos.ServiceRequestStatus.CallerError;
                }
                else if (statusCode == StatusCodes.Status504GatewayTimeout)
                {
                    operationStatus = Ms.Qos.ServiceRequestStatus.TransportError;
                }
                else
                {
                    operationStatus = Ms.Qos.ServiceRequestStatus.ServiceError;
                }
            }

            bool succeeded = response?.StatusCode >= 200 && response.StatusCode <= 299;

            var operationDetails = new
            {
                ServiceName = request.Path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "Unknown",
                RequestTraceId = requestTraceId,
                ServerTraceId = serverTraceId,
                RequestHeader = request.Headers.ToString(),
                ResponseHeader = response?.Headers?.ToString() ?? string.Empty,
                RequestDetails = SllLogger.Masker.MaskSingle(requestPayload),
                ResponseDetails = SllLogger.Masker.MaskSingle(responsePayload),
                Message = message,
                Thumbprint = (string)callerThumbprint ?? string.Empty,
                baseData = new
                {
                    operationName,
                    latencyMs,
                    callerName = (string)callerObject ?? string.Empty,
                    serviceErrorCode = 0,
                    operationVersion = apiExternalVersion,
                    callerIpAddress = request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    protocol = request.Scheme,
                    protocolStatusCode = ((int?)response?.StatusCode)?.ToString() ?? string.Empty,
                    requestMethod = request.Method,
                    responseContentType = response?.ContentType ?? string.Empty,
                    requestSizeBytes = request.ContentLength ?? 0,
                    requestStatus = operationStatus.ToString(),
                    succeeded,
                    targetUri = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}"
                },
                flightingExperimentId,
                scenarioId
            };

            string cv = httpContext.Items.TryGetValue("CorrelationVector", out var cvObj) ? cvObj?.ToString() : string.Empty;

            if (succeeded)
            {
                logger.LogInformation(SllWebLoggerLogMessageFormat, requestTraceId, serverTraceId, cv, operationDetails.ServiceName, string.Empty, "Microsoft.Commerce.Tracing.Sll.OperationDetails", message, JsonConvert.SerializeObject(operationDetails));
            }
            else
            {
                logger.LogError(SllWebLoggerLogMessageFormat, requestTraceId, serverTraceId, cv, operationDetails.ServiceName, string.Empty, "Microsoft.Commerce.Tracing.Sll.OperationDetails", message, JsonConvert.SerializeObject(operationDetails));
            }
        }

        public static void TracePXServiceOutgoingOperation(
        ILogger logger,
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
        string servicePointData = null)
        {
            Ms.Qos.ServiceRequestStatus operationStatus = Ms.Qos.ServiceRequestStatus.Undefined;

            if (response != null)
            {
                int statusCode = response.StatusCode;
                if (statusCode >= 200 && statusCode <= 299)
                {
                    operationStatus = Ms.Qos.ServiceRequestStatus.Success;
                }
                else
                {
                    if (statusCode >= 400 && statusCode <= 499)
                    {
                        operationStatus = Ms.Qos.ServiceRequestStatus.CallerError;
                    }
                    else if (statusCode == StatusCodes.Status502BadGateway && response.DoesReponseIndicateIdempotentTransaction())
                    {
                        operationStatus = Ms.Qos.ServiceRequestStatus.CallerError;
                    }
                    else if (statusCode == StatusCodes.Status504GatewayTimeout)
                    {
                        operationStatus = Ms.Qos.ServiceRequestStatus.TransportError;
                    }
                    else
                    {
                        operationStatus = Ms.Qos.ServiceRequestStatus.ServiceError;
                    }
                }
            }

            bool succeeded = response != null && response.StatusCode >= 200 && response.StatusCode <= 299;
            responsePayload = Truncate(SllLogger.Masker.MaskSingle(responsePayload), MaxResponseLength);

            var operationDetails = new
            {
                ServiceName = serviceName,
                RequestTraceId = requestTraceId,
                RequestHeader = SllLogger.Masker.MaskHeader(request.GetRequestHeaderString()),
                ResponseHeader = response != null ? response.GetResponseHeaderString() : string.Empty,
                RequestDetails = SllLogger.Masker.MaskSingle(requestPayload),
                ResponseDetails = responsePayload,
                Message = message,
                StartTime = startTime,
                CertInfo = certInfo,
                ServicePointData = servicePointData,
                baseData = new
                {
                    operationName = operationName,
                    serviceErrorCode = 0,
                    protocol = request.Scheme,
                    protocolStatusCode = response != null ? response.StatusCode.ToString() : string.Empty,
                    requestMethod = request.Method,
                    responseContentType = response != null ? response.GetResponseContentType() : string.Empty,
                    requestStatus = operationStatus,
                    succeeded = succeeded,
                    targetUri = request.GetDisplayUrl(),
                    dependencyName = serviceName,
                    dependencyOperationName = serviceName,
                    dependencyOperationVersion = request.GetApiVersion(),
                    dependencyType = string.Empty,
                    responseSizeBytes = response?.GetResponseContentLength(),
                    latencyMs = (int)stopwatch.ElapsedMilliseconds
                }
            };

            stopwatch.Stop();

            string correlationVector = request.Headers.TryGetValue(CorrelationVector.HeaderName, out var cvValues)
                ? cvValues.FirstOrDefault()
                : request.HttpContext.Items.TryGetValue(CorrelationVector.HeaderName, out var cvObj) && cvObj is CorrelationVector cv
                    ? cv.ToString()
                    : string.Empty;

            if (succeeded)
            {
                logger.LogInformation(SllWebLoggerLogMessageFormat, requestTraceId, string.Empty, correlationVector, request.GetServiceName(), string.Empty, "Microsoft.Commerce.Tracing.Sll.PXServiceOutgoingOperation", message, JsonConvert.SerializeObject(operationDetails));
            }
            else
            {
                logger.LogError(SllWebLoggerLogMessageFormat, requestTraceId, string.Empty, correlationVector, request.GetServiceName(), string.Empty, "Microsoft.Commerce.Tracing.Sll.PXServiceOutgoingOperation", message, JsonConvert.SerializeObject(operationDetails));
            }
        }

        public static void TracePXServiceOutgoingOperation(
           ILogger logger,
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
            ServiceRequestStatus operationStatus = isSucceeded ? ServiceRequestStatus.Success : ServiceRequestStatus.ServiceError;
            var operationDetails = new
            {
                ServiceName = serviceName,
                RequestTraceId = requestTraceId,
                RequestHeader = string.Empty, // No headers in SOAP calls
                ResponseHeader = string.Empty,  // No headers in SOAP calls
                RequestDetails = XmlDataMasker.Mask(requestPayload),
                ResponseDetails = XmlDataMasker.Mask(responsePayload),
                Message = message,
                StartTime = startTime,
                CertInfo = certInfo,
                baseData = new
                {
                    operationName,
                    latencyMs,
                    serviceErrorCode = 0,
                    protocol = Uri.UriSchemeHttps,
                    protocolStatusCode = isSucceeded ? HttpStatusCode.OK.ToString() : HttpStatusCode.InternalServerError.ToString(),
                    requestMethod = string.Empty,
                    responseContentType = string.Empty,
                    requestStatus = operationStatus,
                    succeeded = isSucceeded,
                    targetUri,
                    dependencyName = serviceName,
                    dependencyOperationName = serviceName,
                    dependencyOperationVersion = string.Empty,
                    dependencyType = string.Empty,
                    responseSizeBytes = !string.IsNullOrEmpty(responsePayload) ? responsePayload.Length : 0
                }
            };
            if (isSucceeded)
            {
                logger.LogInformation(SllWebLoggerLogMessageFormat, requestTraceId, string.Empty, correlationVector, string.Empty, string.Empty, "Microsoft.Commerce.Tracing.Sll.PXServiceOutgoingOperation", message, JsonConvert.SerializeObject(operationDetails));
            }
            else
            {
                logger.LogError(SllWebLoggerLogMessageFormat, requestTraceId, string.Empty, correlationVector, string.Empty, string.Empty, "Microsoft.Commerce.Tracing.Sll.PXServiceOutgoingOperation", message, JsonConvert.SerializeObject(operationDetails));
            }
        }

        public static void TracePXServicePIAddedOnOffer(
    ILogger logger,
    string serviceName,
    HttpRequest request,
    string requestTraceId,
    string paymentInstrumentId,
    string paymentMethodFamily,
    string paymentMethodType,
    string partner,
    string country,
    string offerId,
    string puid)
        {
            var paymentInstrumentAddedOnOffer = new
            {
                serviceName,
                requestTraceId,
                paymentInstrumentId,
                paymentMethodFamily,
                paymentMethodType,
                partner,
                country,
                offerId,
                puid
            };

            string correlationVector = request.Headers.TryGetValue(CorrelationVector.HeaderName, out var cvValues)
                ? cvValues.FirstOrDefault()
                : string.Empty;

            if (string.IsNullOrEmpty(correlationVector))
            {
                // fallback to extended CV from context if available
                if (request.HttpContext.Items.TryGetValue(CorrelationVector.HeaderName, out var cvObj) && cvObj is CorrelationVector cv)
                {
                    correlationVector = cv.ToString();
                }
            }

            logger.LogInformation(
                SllWebLoggerLogMessageFormat,
                requestTraceId,
                string.Empty,
                correlationVector,
                request.Path.HasValue ? request.Path.Value.Split('/').Skip(1).FirstOrDefault() : "UnknownService",
                string.Empty,
                "Microsoft.Commerce.Tracing.Sll.PXServicePIAddedOnOffer",
                string.Empty,
                JsonConvert.SerializeObject(paymentInstrumentAddedOnOffer));
        }

        public static void TracePXServiceIntegrationError(
            ILogger logger,
            string serviceName,
            IntegrationErrorCode integrationErrorCode,
            string message,
            string requestTraceId,
            string serverTraceId = null,
            string correlationVector = null)
        {
            var integrationError = new
            {
                ServiceName = serviceName,
                ServerTraceId = serverTraceId ?? string.Empty,
                RequestTraceId = requestTraceId,
                IntegrationErrorCode = integrationErrorCode.ToString(),
                Message = message,
            };

            logger.LogWarning(SllWebLoggerLogMessageFormat, requestTraceId, serverTraceId, correlationVector, string.Empty, string.Empty, "Microsoft.Commerce.Tracing.Sll.PXServiceIntegrationError", message, JsonConvert.SerializeObject(integrationError));
        }

        public static void TracePXServiceException(
           ILogger logger,
           string exceptionMessage,
           Commerce.Tracing.EventTraceActivity requestTraceId)
        {
            const int LogMaxLength = 3375;

            var traceException = new
            {
                RequestTraceId = requestTraceId.ActivityId.ToString(),
                Exception = Truncate(exceptionMessage, LogMaxLength),
            };
            logger.LogError(SllWebLoggerLogMessageFormat, requestTraceId, string.Empty, string.Empty, string.Empty, string.Empty, "Microsoft.Commerce.Tracing.Sll.PXServiceTraceException", string.Empty, JsonConvert.SerializeObject(traceException));
        }

        public static void TraceMISETokenValidationResult(
            ILogger logger,
            bool succeed,
            string applicationId,
            string errorCode,
            string cloudInstance,
            string message,
            long latency,
            string cV,
            Exception ex)
        {
            var authenticationResult = new
            {
                Success = succeed,
                ApplicationId = applicationId,
                AuthenticationErrorCode = errorCode,
                Message = message,
                Latency = latency,
                CV = cV,
                Exception = ex?.ToString(),
                CloudInstance = cloudInstance,
            };
            logger.LogInformation(SllWebLoggerLogMessageFormat, string.Empty, string.Empty, cV, string.Empty, string.Empty, "Microsoft.Commerce.Tracing.Sll.MISETokenValidationResult", message, JsonConvert.SerializeObject(authenticationResult));
        }

        public static void TracePXServiceIncomingOperation(
    ILogger logger,
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
    string avsSuggest = null)
        {
            string apiExternalVersion = request.GetApiVersion();

            request.HttpContext.Items.TryGetValue(PaymentConstants.Web.Properties.CallerName, out var callerObject);

            ServiceRequestStatus operationStatus = ServiceRequestStatus.Undefined;

            if (response != null)
            {
                if (response.StatusCode >= 200 && response.StatusCode < 300)
                {
                    operationStatus = ServiceRequestStatus.Success;
                }
                else
                {
                    operationStatus = response.StatusCode >= 400 && response.StatusCode < 500
                        ? ServiceRequestStatus.CallerError
                        : ServiceRequestStatus.ServiceError;

                    if (string.IsNullOrEmpty(errorCode))
                    {
                        try
                        {
                            var token = JObject.Parse(responsePayload);
                            errorCode = (string)token.SelectToken("ErrorCode");
                        }
                        catch (JsonReaderException) { }

                        errorCode ??= "StatusCode." + response.StatusCode.ToString();
                    }
                }
            }

            bool succeeded = response?.StatusCode is >= 200 and < 300;
            responsePayload = Truncate(SllLogger.Masker.MaskSingle(responsePayload), MaxResponseLength);
            authenticationInfo = Truncate(authenticationInfo, MaxAuthenticationInfoLength);

            var incomingOperation = new PXServiceIncomingOperation
            {
                ServiceName = request.GetServiceName(),
                RequestTraceId = requestTraceId,
                ServerTraceId = serverTraceId ?? string.Empty,
                AccountId = accountId ?? string.Empty,
                InstrumentId = paymentInstrumentId ?? string.Empty,
                PaymentMethodFamily = string.IsNullOrEmpty(paymentMethodFamily) ? "Unknown" : paymentMethodFamily,
                PaymentMethodType = string.IsNullOrEmpty(paymentMethodType) ? "Unknown" : paymentMethodType,
                Country = string.IsNullOrEmpty(country) ? "Unknown" : country,
                RequestHeader = SllLogger.Masker.MaskHeader(request.GetRequestHeaderString()),
                ResponseHeader = response.GetResponseHeaderString(),
                RequestDetails = SllLogger.Masker.MaskSingle(requestPayload),
                ResponseDetails = responsePayload,
                Message = message ?? string.Empty,
                StartTime = startTime ?? string.Empty,
                ErrorCode = errorCode ?? string.Empty,
                ErrorMessage = errorMessage ?? string.Empty,
                IsTest = isTest,
                AuthenticationInfo = authenticationInfo,
                Partner = partner ?? string.Empty,
                PidlOperation = pidlOperation ?? string.Empty,
                AvsSuggest = avsSuggest ?? string.Empty,
                baseData =
        {
            operationName = operationName,
            callerName = callerObject as string ?? string.Empty,
            serviceErrorCode = 0,
            operationVersion = apiExternalVersion,
            callerIpAddress = request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
            protocol = request.Scheme,
            protocolStatusCode = ((int)response.StatusCode).ToString(),
            requestMethod = request.Method,
            responseContentType = response.ContentType ?? string.Empty,
            requestSizeBytes = request.GetRequestContentLength(),
            requestStatus = operationStatus,
            succeeded = succeeded,
            targetUri = request.GetDisplayUrl()
        }
            };

            stopwatch.Stop();
            incomingOperation.baseData.latencyMs = (int)stopwatch.ElapsedMilliseconds;

            incomingOperation.Log(
                succeeded ? EventLevel.Informational : EventLevel.Error,
                SllLogger.EnvironmentLogOption,
                envelope => envelope.cV = request.GetCorrelationVector()?.ToString());
        }


        public static void TraceTokenGenerationResult(
           ILogger logger,
           bool succeed,
           string resource,
           string clientId,
           long latency,
           string cV,
           Exception ex,
           string expiresOn = null)
        {
            var authenticationResult = new
            {
                Success = succeed,
                Resource = resource,
                ClientID = clientId,
                Latency = latency,
                CV = cV,
                ExpiresOn = expiresOn,
                Exception = ex?.ToString()
            };
            logger.LogInformation(SllWebLoggerLogMessageFormat, string.Empty, string.Empty, cV, string.Empty, string.Empty, "Microsoft.Commerce.Tracing.Sll.TokenGenerationResult", string.Empty, JsonConvert.SerializeObject(authenticationResult));
        }

        /// <summary>
        /// Truncates a given string to allowed max size
        /// </summary>
        /// <param name="message">The string to be truncated</param>
        /// <param name="maxLength">The max length allowed</param>
        /// <returns>Message truncated to ETW supported length</returns>
        private static string Truncate(string message, int maxLength)
        {
            // Task 1564455:[PIMS TODOs] PaymentsManagementEventSource should not truncate message but split them into multiple messages
            if (message != null && message.Length > maxLength)
            {
                return message.Substring(0, maxLength);
            }

            return message;
        }
    }
}
