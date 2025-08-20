// <copyright file="SllWebLogger.cs" company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Tracing;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using Microsoft.Commerce.Tracing.Sll;
    using Microsoft.CommonSchema.Services;
    using Microsoft.CommonSchema.Services.Logging;
    using Microsoft.Diagnostics.Tracing;
    using Ms.Qos;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Tracing;

    public static class SllWebLogger
    {
        private const int MaxResponseLength = 26500;
        private const int MaxAuthenticationInfoLength = 26500;

        public static void TraceServerMessage(
            string serviceName,
            string correlationId,
            string trackingGuid,
            string message,
            EventLevel eventLevel)
        {
            ServerMessage serverMessage = new ServerMessage()
            {
                ServiceName = serviceName,
                CorrelationId = correlationId,
                TrackingGuid = trackingGuid,
                Message = message
            };

            serverMessage.Log(eventLevel, SllLogger.EnvironmentLogOption);
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
            // Not all the APIs have version.
            string apiExternalVersion = string.Empty;
            if (request.Options.TryGetValue(new HttpRequestOptionsKey<object>(PaymentConstants.Web.Properties.Version), out _))
            {
                apiExternalVersion = request.GetApiVersion().ExternalVersion;
            }

            request.Options.TryGetValue(new HttpRequestOptionsKey<object>(PaymentConstants.Web.Properties.CallerName), out var callerObject);

            request.Options.TryGetValue(new HttpRequestOptionsKey<object>(PaymentConstants.Web.Properties.CallerThumbprint), out var callerThumbprintObject);

            Ms.Qos.ServiceRequestStatus operationStatus = Ms.Qos.ServiceRequestStatus.Undefined;

            if (response != null)
            {
                if (response.IsSuccessStatusCode)
                {
                    operationStatus = Ms.Qos.ServiceRequestStatus.Success;
                }
                else
                {
                    if (response.StatusCode.IsClientError())
                    {
                        operationStatus = Ms.Qos.ServiceRequestStatus.CallerError;
                    }
                    else if (response.StatusCode == HttpStatusCode.BadGateway && response.DoesReponseIndicateIdempotentTransaction())
                    {
                        // Caller should not invoke an idempotent request for 502 responses (transaction: failed)
                        operationStatus = Ms.Qos.ServiceRequestStatus.CallerError;
                    }
                    else if (response.StatusCode == HttpStatusCode.GatewayTimeout)
                    {
                        operationStatus = Ms.Qos.ServiceRequestStatus.TransportError;
                    }
                    else
                    {
                        operationStatus = Ms.Qos.ServiceRequestStatus.ServiceError;
                    }
                }
            }

            bool succeeded = response != null && response.IsSuccessStatusCode;

            OperationDetails operationDetails = new OperationDetails()
            {
                ServiceName = request.GetServiceName(),
                RequestTraceId = requestTraceId,
                ServerTraceId = serverTraceId,
                RequestHeader = request.GetRequestHeaderString(),
                ResponseHeader = response != null ? response.GetResponseHeaderString() : string.Empty,
                RequestDetails = SllLogger.Masker.MaskSingle(requestPayload),
                ResponseDetails = SllLogger.Masker.MaskSingle(responsePayload),
                Message = message,
                Thumbprint = (string)callerThumbprintObject ?? string.Empty,
                baseData =
                {
                    operationName = operationName,
                    latencyMs = latencyMs,
                    callerName = (string)callerObject ?? string.Empty,
                    serviceErrorCode = 0, // We don't use int error code.
                    operationVersion = apiExternalVersion,
                    callerIpAddress = request.Headers.Host,
                    protocol = request.RequestUri.IsAbsoluteUri ? request.RequestUri.Scheme : string.Empty,
                    protocolStatusCode = response != null ? Convert.ToInt32(response.StatusCode).ToString() : string.Empty,
                    requestMethod = request.Method.Method,
                    responseContentType = response != null ? response.GetResponseContentType() : string.Empty,
                    requestSizeBytes = request.GetRequestContentLength(),
                    requestStatus = operationStatus,
                    succeeded = succeeded,
                    targetUri = request.RequestUri.IsAbsoluteUri ? request.RequestUri.AbsoluteUri : request.RequestUri.ToString(),
                }
            };

            operationDetails.Log(
                succeeded ? EventLevel.Informational : EventLevel.Error,
                SllLogger.EnvironmentLogOption,
                (envelope) =>
                {
                    if (request.Options.TryGetValue(new HttpRequestOptionsKey<object>(PaymentConstants.Web.Properties.FlightingExperimentId), out var flightingExperimentId))
                    {
                        envelope.SetApp(new Telemetry.Extensions.app { expId = flightingExperimentId.ToString() });
                    }

                    if (request.Options.TryGetValue(new HttpRequestOptionsKey<object>(PaymentConstants.Web.Properties.ScenarioId), out var scenarioId) && envelope.tags != null)
                    {
                        envelope.tags["scenarioId"] = scenarioId.ToString();
                    }

                    envelope.cV = request.GetCorrelationVector().ToString();
                });
        }

        /// <summary>
        /// Trace PXService incoming operation details. 
        /// </summary>
        /// <param name="operationName">The name of current operation.</param>
        /// <param name="accountId">Account Id which current operation is applied to.</param>
        /// <param name="paymentInstrumentId">The payment instrument Id which current operation is applied to.</param>
        /// <param name="paymentMethodFamily">The payment method family which current operation applied to.</param>
        /// <param name="paymentMethodType">The payment method type which current operation applied to.</param>
        /// <param name="country">The country which current operation applied to.</param>
        /// <param name="request">Http request message of current operation.</param>
        /// <param name="response">Http response message of current operation.</param>
        /// <param name="requestPayload">The payload of request message.</param>
        /// <param name="responsePayload">The payload of response message.</param>
        /// <param name="startTime">Operation start time.</param>
        /// <param name="stopwatch">The stopwatch used to get duration in milliseconds between start and stop of operation.</param>
        /// <param name="requestTraceId">The request correlation id, if it exists. </param>
        /// <param name="authenticationInfo">The authentication information </param>
        /// <param name="serverTraceId">The server trace id, if it exists. </param>
        /// <param name="message">The additional message put in the PartC.</param>
        /// <param name="errorCode">The PIMS error code put in the PartC.</param>
        /// <param name="errorMessage">The error Message put in the PartC.</param>
        /// <param name="isTest">The flag on isTest.</param>
        /// <param name="partner">The partner who calls pidlsdk</param>
        /// <param name="pidlOperation">The operation parameter when call PX eg. "add" operation for AddressDescriptions-GET or "validateInstance/select/selectInstance/update" we have in description controller</param>
        /// <param name="avsSuggest">The avsSuggest parameter when call PX in zip4 flow</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Easy to understand and cannot simplify more.")]
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
            // Not all the APIs have version.
            string apiExternalVersion = string.Empty;
            if (request.Options.TryGetValue(new HttpRequestOptionsKey<object>(PaymentConstants.Web.Properties.Version), out _))
            {
                apiExternalVersion = request.GetApiVersion().ExternalVersion;
            }

            request.Options.TryGetValue(new HttpRequestOptionsKey<object>(PaymentConstants.Web.Properties.CallerName), out var callerObject);

            ServiceRequestStatus operationStatus = ServiceRequestStatus.Undefined;

            if (response != null)
            {
                if (response.IsSuccessStatusCode)
                {
                    operationStatus = ServiceRequestStatus.Success;
                }
                else
                {
                    if (response.StatusCode.IsClientError())
                    {
                        operationStatus = ServiceRequestStatus.CallerError;
                    }
                    else
                    {
                        operationStatus = ServiceRequestStatus.ServiceError;
                    }

                    // if the response is not successful and there is no errorCode:
                    // try to get it from the response payload or add the protocol status code as error code
                    if (string.IsNullOrEmpty(errorCode))
                    {
                        JObject token;
                        try
                        {
                            token = JObject.Parse(responsePayload);
                            errorCode = (string)token.SelectToken("ErrorCode");
                        }
                        catch (JsonReaderException)
                        {
                        }

                        errorCode = string.IsNullOrEmpty(errorCode) ? "StatusCode." + response.StatusCode.ToString() : errorCode;
                    }
                }
            }

            bool succeeded = response != null && response.IsSuccessStatusCode;
            responsePayload = Truncate(SllLogger.Masker.MaskSingle(responsePayload), MaxResponseLength);
            authenticationInfo = Truncate(authenticationInfo, MaxAuthenticationInfoLength);

            PXServiceIncomingOperation incomingOperation = new PXServiceIncomingOperation()
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
                ResponseHeader = response != null ? response.GetResponseHeaderString() : string.Empty,
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
                    callerName = (string)callerObject ?? string.Empty,
                    serviceErrorCode = 0, // We don't use int error code.
                    operationVersion = apiExternalVersion,
                    callerIpAddress = request.Headers.Host,
                    protocol = request.RequestUri.IsAbsoluteUri ? request.RequestUri.Scheme : string.Empty,
                    protocolStatusCode = response != null ? Convert.ToInt32(response.StatusCode).ToString() : string.Empty,
                    requestMethod = request.Method.Method,
                    responseContentType = response != null ? response.GetResponseContentType() : string.Empty,
                    requestSizeBytes = request.GetRequestContentLength(),
                    requestStatus = operationStatus,
                    succeeded = succeeded,
                    targetUri = request.RequestUri.IsAbsoluteUri ? request.RequestUri.AbsoluteUri : request.RequestUri.ToString(),
                }
            };

            stopwatch.Stop();
            incomingOperation.baseData.latencyMs = (int)stopwatch.ElapsedMilliseconds;

            incomingOperation.Log(
                succeeded ? EventLevel.Informational : EventLevel.Error,
                SllLogger.EnvironmentLogOption,
                envelope => envelope.cV = request.GetCorrelationVector().ToString());
        }

        /// <summary>
        /// Trace PXService outgoing call details. 
        /// </summary>
        /// <param name="operationName">The name of current operation.</param>
        /// <param name="serviceName">The name of the service being called.</param>
        /// <param name="request">Http request message of current operation.</param>
        /// <param name="response">Http response message of current operation.</param>
        /// <param name="requestPayload">The payload of request message.</param>
        /// <param name="responsePayload">The payload of response message.</param>
        /// <param name="startTime">Operation start time.</param>
        /// <param name="stopwatch">The stopwatch used to get duration in milliseconds between start and stop of operation.</param>
        /// <param name="requestTraceId">The event trace activity id of legacy etw tracing.</param>        
        /// <param name="message">The additional message put in the PartC.</param>
        /// <param name="certInfo">The certificate information of the request.</param>
        /// <param name="servicePointData">The relevant properties of the ServicePoint for the request.</param>
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
            // Not all the APIs have version.
            object apiVersion = null;
            request.Options.TryGetValue(new HttpRequestOptionsKey<object>(PaymentConstants.Web.Properties.Version), out apiVersion);

            Ms.Qos.ServiceRequestStatus operationStatus = Ms.Qos.ServiceRequestStatus.Undefined;

            if (response != null)
            {
                if (response.IsSuccessStatusCode)
                {
                    operationStatus = Ms.Qos.ServiceRequestStatus.Success;
                }
                else
                {
                    if (response.StatusCode.IsClientError())
                    {
                        operationStatus = Ms.Qos.ServiceRequestStatus.CallerError;
                    }
                    else if (response.StatusCode == HttpStatusCode.BadGateway && response.DoesReponseIndicateIdempotentTransaction())
                    {
                        // Caller should not invoke an idempotent request for 502 responses (transaction: failed)
                        operationStatus = Ms.Qos.ServiceRequestStatus.CallerError;
                    }
                    else if (response.StatusCode == HttpStatusCode.GatewayTimeout)
                    {
                        operationStatus = Ms.Qos.ServiceRequestStatus.TransportError;
                    }
                    else
                    {
                        operationStatus = Ms.Qos.ServiceRequestStatus.ServiceError;
                    }
                }
            }

            bool succeeded = response != null && response.IsSuccessStatusCode;
            responsePayload = Truncate(SllLogger.Masker.MaskSingle(responsePayload), MaxResponseLength);

            PXServiceOutgoingOperation operationDetails = new PXServiceOutgoingOperation()
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
                baseData =
                {
                    operationName = operationName,
                    serviceErrorCode = 0, // Task 1562648:[PIMS TODOs] ClientOperation SllLogger enhancement. Some services might return INT codes . How to extract it a generic way ?                 
                    protocol = request.RequestUri.IsAbsoluteUri ? request.RequestUri.Scheme : string.Empty,
                    protocolStatusCode = response != null ? Convert.ToInt32(response.StatusCode).ToString() : string.Empty,
                    requestMethod = request.Method.Method,
                    responseContentType = response != null ? response.GetResponseContentType() : string.Empty,
                    requestStatus = operationStatus,
                    succeeded = succeeded,
                    targetUri = request.RequestUri.IsAbsoluteUri ? request.RequestUri.AbsoluteUri : request.RequestUri.ToString(),
                    dependencyName = serviceName,
                    dependencyOperationName = serviceName, // Task 1562648:[PIMS TODOs] ClientOperation SllLogger enhancement. Xpert seems to have issues with displaying a coutner aggregrated with miltiple operations, so using single operation name per service
                    dependencyOperationVersion = apiVersion == null ? null : apiVersion.ToString(),
                    dependencyType = null, // Task 1562648:[PIMS TODOs] ClientOperation SllLogger enhancement. Figure out how to make the best use of this field.
                    responseSizeBytes = response.GetRequestContentLength()
                }
            };

            stopwatch.Stop();
            operationDetails.baseData.latencyMs = (int)stopwatch.ElapsedMilliseconds;

            string correlationVector = request.GetCorrelationVectorFromHeader();
            if (string.IsNullOrEmpty(correlationVector))
            {
                correlationVector = request.GetCorrelationVector().ToString();
            }

            operationDetails.Log(
                succeeded ? EventLevel.Informational : EventLevel.Error,
                SllLogger.EnvironmentLogOption,
                envelope => envelope.cV = correlationVector);
        }

        /// <summary>
        /// Trace PXService outgoing call details for SOAP APIs
        /// </summary>
        /// <param name="operationName">The name of current operation.</param>
        /// <param name="serviceName">The name of the service being called.</param>
        /// <param name="targetUri">RequestUri of the outbound request</param>
        /// <param name="requestPayload">The payload of request message.</param>
        /// <param name="responsePayload">The payload of response message.</param>
        /// <param name="startTime">Operation start time.</param>
        /// <param name="latencyMs">Duration in milliseconds between start and stop of operation.</param>
        /// <param name="requestTraceId">The event trace activity id of legacy etw tracing.</param>                
        /// <param name="correlationVector">correlationVector of current operation.</param>        
        /// <param name="isSucceeded">Whether the current operation succeeded</param>
        /// <param name="message">The additional message put in the PartC.</param>
        /// <param name="certInfo">The certificate information of the request.</param>
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
            ServiceRequestStatus operationStatus = isSucceeded ? ServiceRequestStatus.Success : ServiceRequestStatus.ServiceError;
            PXServiceOutgoingOperation operationDetails = new PXServiceOutgoingOperation()
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
                baseData =
                {
                    operationName = operationName,
                    serviceErrorCode = 0,
                    protocol = Uri.UriSchemeHttps,
                    protocolStatusCode = isSucceeded ? HttpStatusCode.OK.ToString() : HttpStatusCode.InternalServerError.ToString(),
                    requestMethod = string.Empty,
                    responseContentType = string.Empty,
                    requestStatus = operationStatus,
                    succeeded = isSucceeded,
                    targetUri = targetUri,
                    dependencyName = serviceName,
                    dependencyOperationName = serviceName,
                    dependencyOperationVersion = null,
                    dependencyType = null,
                    responseSizeBytes = !string.IsNullOrEmpty(responsePayload) ? responsePayload.Length : 0
                }
            };

            operationDetails.baseData.latencyMs = latencyMs;

            operationDetails.Log(
                isSucceeded ? EventLevel.Informational : EventLevel.Error,
                SllLogger.EnvironmentLogOption,
                envelope => envelope.cV = correlationVector);
        }

        /// <summary>
        /// Trace PXService outgoing call details. 
        /// </summary>
        /// <param name="serviceName">Name of the service loggin this event.</param>
        /// <param name="request">Http request message of current operation.</param>
        /// <param name="requestTraceId">The event trace activity id of legacy etw tracing.</param>        
        /// <param name="paymentInstrumentId">Id of the newly created PI</param>
        /// <param name="paymentMethodFamily">Family of the newly created PI</param>
        /// <param name="paymentMethodType">Type of the newly created PI</param>
        /// <param name="partner">Partner whose portal/ui where the user added this PI</param>
        /// <param name="country">Country where the user added this PI</param>
        /// <param name="offerId">OfferId associated with this add PI operation</param>
        /// <param name="puid">Puid of the user</param>
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
            PXServicePIAddedOnOffer paymentInstrumentAddedOnOffer = new PXServicePIAddedOnOffer()
            {
                serviceName = serviceName,
                requestTraceId = requestTraceId,
                paymentInstrumentId = paymentInstrumentId,
                paymentMethodFamily = paymentMethodFamily,
                paymentMethodType = paymentMethodType,
                partner = partner,
                country = country,
                offerId = offerId,
                puid = puid
            };

            string correlationVector = request.GetCorrelationVectorFromHeader();
            if (string.IsNullOrEmpty(correlationVector))
            {
                correlationVector = request.GetCorrelationVector().ToString();
            }

            paymentInstrumentAddedOnOffer.Log(
                EventLevel.Informational,
                LogOption.Realtime,
                envelope => envelope.cV = correlationVector);
        }

        /// <summary>
        /// Trace PXService integration errors. 
        /// </summary>
        /// <param name="serviceName">The name of the service being called.</param>
        /// <param name="integrationErrorCode">The error code for an integration error.</param>
        /// <param name="message">The error message for an integration error</param>
        /// <param name="requestTraceId">The request correlation id</param>
        /// <param name="serverTraceId">The server trace id, if it exists. </param>
        /// <param name="correlationVector">The correlation vector, if it exists. </param>
        public static void TracePXServiceIntegrationError(
            string serviceName,
            IntegrationErrorCode integrationErrorCode,
            string message,
            string requestTraceId,
            string serverTraceId = null,
            string correlationVector = null)
        {
            PXServiceIntegrationError integrationError = new PXServiceIntegrationError()
            {
                ServiceName = serviceName,
                ServerTraceId = serverTraceId ?? string.Empty,
                RequestTraceId = requestTraceId,
                IntegrationErrorCode = integrationErrorCode.ToString(),
                Message = message,
            };

            integrationError.Log(
                EventLevel.Warning,
                SllLogger.EnvironmentLogOption,
                envelope => envelope.cV = correlationVector ?? string.Empty);
        }

        /// <summary>
        /// Trace PXService exceptions. 
        /// </summary>
        /// <param name="exceptionMessage">Exceptin Message</param>
        /// <param name="requestTraceId">The correlation id of request</param>
        public static void TracePXServiceException(
            string exceptionMessage,
            EventTraceActivity requestTraceId)
        {
            const int LogMaxLength = 3375;

            PXServiceTraceException traceException = new PXServiceTraceException()
            {
                RequestTraceId = requestTraceId.ActivityId.ToString(),
                Exception = Truncate(exceptionMessage, LogMaxLength),
            };

            traceException.Log(
                EventLevel.Error,
                SllLogger.EnvironmentLogOption);
        }

        /// <summary>
        /// Trace Token Authentication Result
        /// </summary>
        /// <param name="succeed">authentication succeed or not</param>
        /// <param name="applicationId">aad applicationId resolved in the authentication</param>
        /// <param name="errorCode">if succeed = false, the error code should be returned</param>
        /// <param name="cloudInstance">cloud instance </param>
        /// <param name="message">The debug message </param>
        /// <param name="latency">authentication latency</param>
        /// <param name="cV">Incoming request CV</param>
        /// <param name="ex">if succeed = false, more detail from exception</param>
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
            MISETokenValidationResult authenticationResult = new MISETokenValidationResult()
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

            authenticationResult.Log(
                EventLevel.Informational,
                SllLogger.EnvironmentLogOption);
        }

        /// <summary>
        /// Trace Token Authentication Result
        /// </summary>
        /// <param name="succeed">authentication succeed or not</param>
        /// <param name="resource">dependancy endpoint</param>
        /// <param name="clientId">mi client id of calling application</param>
        /// <param name="latency">authentication latency</param>
        /// <param name="cV">Incoming request CV</param>
        /// <param name="ex">if succeed = false, more detail from exception</param>
        /// <param name="expiresOn">expire time on a generated token</param>
        public static void TraceTokenGenerationResult(
            bool succeed,
            string resource,
            string clientId,
            long latency,
            string cV,
            Exception ex,
            string expiresOn = null)
        {
            TokenGenerationResult authenticationResult = new TokenGenerationResult()
            {
                Success = succeed,
                Resource = resource,
                ClientID = clientId,
                Latency = latency,
                CV = cV,
                ExpiresOn = expiresOn,
                Exception = ex?.ToString()
            };

            authenticationResult.Log(
                EventLevel.Informational,
                SllLogger.EnvironmentLogOption);
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