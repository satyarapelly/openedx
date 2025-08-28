// <copyright file="PXTraceCorrelationHandler.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using static Microsoft.Commerce.Payments.Common.PaymentConstants.Web;
    using CorrelationVector = Microsoft.CommonSchema.Services.Logging.CorrelationVector;
    using Sll = Microsoft.CommonSchema.Services.Logging.Sll;

    /// <summary>
    /// Delegating handler which performs detailed message tracing.  Informational
    /// traces include request method, request URI, request headers, request payload
    /// (interpreted as a string), response code, response headers, and response payload
    /// (interpreted as a string).
    /// </summary>
    public class PXTraceCorrelationHandler : DelegatingHandler
    {
        private const string PaymentInstrumentOperationsController = "PaymentInstrumentOperationsController";
        private const string PaymentInstrumentsController = "PaymentInstrumentsController";
        private const string DefaultLogValue = "<none>";
        private const int DefaultConnectionLeaseTimeoutInMs = 120 * 1000;
        private const int DefaultMaxIdleTime = -1;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly RequestDelegate _next;
        private bool isDependentServiceRequest;

        public PXTraceCorrelationHandler(string serviceName, HttpMessageHandler innerHandler, bool isDependentServiceRequest, Action<string, EventTraceActivity> logError = null, IHttpContextAccessor httpContextAccessor = null)
            : base(innerHandler)
        {
            this.ServiceName = serviceName;
            this.isDependentServiceRequest = isDependentServiceRequest;
            this.httpContextAccessor = httpContextAccessor;
            this.LogError = logError ?? ((_, __) => { });
            this._next = _ => Task.CompletedTask;
        }

        public PXTraceCorrelationHandler(
            string serviceName,
            Action<string, string, string, string, string, string, HttpRequestMessage, HttpResponseMessage, string, string, string, string, string, string, string, string> logIncomingRequestToAppInsight,
            IHttpContextAccessor httpContextAccessor = null)
        {
            this.ServiceName = serviceName;
            this.isDependentServiceRequest = false;
            this.LogIncomingRequestToAppInsight = logIncomingRequestToAppInsight;
            this.httpContextAccessor = httpContextAccessor;
            this._next = _ => Task.CompletedTask;
        }

        public PXTraceCorrelationHandler(
            RequestDelegate next,
            string serviceName,
            Action<string, string, string, string, string, string, HttpRequestMessage, HttpResponseMessage, string, string, string, string, string, string, string, string> logIncomingRequestToAppInsight,
            IHttpContextAccessor httpContextAccessor = null)
            : this(serviceName, logIncomingRequestToAppInsight, httpContextAccessor)
        {
            this._next = next;
        }

        public PXTraceCorrelationHandler(
            string serviceName,
            HttpMessageHandler innerHandler,
            bool isDependentServiceRequest,
            Action<string, string, HttpRequestMessage, HttpResponseMessage, string, string, string, string> logOutgoingToAppInsight,
            IHttpContextAccessor httpContextAccessor = null)
           : base(innerHandler)
        {
            this.ServiceName = serviceName;
            this.isDependentServiceRequest = isDependentServiceRequest;
            this.LogToApplicationInsight = logOutgoingToAppInsight;
            this.httpContextAccessor = httpContextAccessor;
            this._next = _ => Task.CompletedTask;
        }

        public PXTraceCorrelationHandler(
            RequestDelegate next,
            string serviceName,
            bool isDependentServiceRequest = false,
            Action<string, EventTraceActivity> logError = null,
            IHttpContextAccessor httpContextAccessor = null)
            : this(serviceName, (HttpMessageHandler)null, isDependentServiceRequest, logError, httpContextAccessor)
        {
            this._next = next;
        }

        private Action<string, string, HttpRequestMessage, HttpResponseMessage, string, string, string, string> LogToApplicationInsight { get; set; }

        private Action<string, string, string, string, string, string, HttpRequestMessage, HttpResponseMessage, string, string, string, string, string, string, string, string> LogIncomingRequestToAppInsight { get; set; }

        public Action<string, EventTraceActivity> LogError { get; set; }

        private string ServiceName { get; set; }

        public static EventTraceActivity GetOrCreateCorrelationIdFromHeader(HttpRequestMessage request)
        {
            Guid correlationId = Guid.Empty;
            IEnumerable<string> correlationIdHeaderValues;
            if (request.Headers.TryGetValues(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, out correlationIdHeaderValues))
            {
                foreach (string headerValue in correlationIdHeaderValues)
                {
                    if (Guid.TryParse(headerValue, out correlationId))
                    {
                        break;
                    }
                }
            }

            EventTraceActivity requestTraceId;
            if (correlationId == Guid.Empty)
            {
                // Check if we've already set the request property
                requestTraceId = request.GetServerTraceId();
                if (requestTraceId != null)
                {
                    correlationId = requestTraceId.ActivityId;
                }
                else
                {
                    correlationId = Guid.NewGuid();
                    requestTraceId = new EventTraceActivity(correlationId);
                }
            }
            else
            {
                requestTraceId = new EventTraceActivity(correlationId);
            }

            return requestTraceId;
        }

        /// <summary>
        /// Extracts trace correlation information from the request, sends the
        /// request up the pipeline, and then stamps correlation information on
        /// the response.
        /// </summary>
        /// <param name="request">The request message.</param>
        /// <param name="cancellationToken">A token which may be used to listen
        /// for cancellation.</param>
        /// <returns>The response message.</returns>
        protected override sealed Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (this.isDependentServiceRequest)
            {
                return this.SendAsyncOutgoing(request, cancellationToken);
            }
            else
            {
                return this.SendAsyncIncoming(request, cancellationToken);
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (this.isDependentServiceRequest)
            {
                await this._next(context);
                return;
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string startTime = DateTime.UtcNow.ToString("o");

            var request = context.Request;
            var requestMessage = request.ToHttpRequestMessage();

            var routeValues = context.GetRouteData()?.Values;
            if (routeValues != null)
            {
                requestMessage.SetRouteData(new RouteValueDictionary(routeValues));
            }

            void CopyItemsToOptions()
            {
                foreach (var item in context.Items)
                {
                    if (item.Key is string key)
                    {
                        requestMessage.Options.Set(new HttpRequestOptionsKey<object>(key), item.Value);
                    }
                }
            }

            CopyItemsToOptions();

            string operationName = this.GetOperationName(requestMessage);
            if (!context.Items.ContainsKey(PaymentConstants.Web.Properties.OperationName))
            {
                context.Items[PaymentConstants.Web.Properties.OperationName] = operationName;
                requestMessage.Options.Set(new HttpRequestOptionsKey<object>(PaymentConstants.Web.Properties.OperationName), operationName);
            }

            CorrelationVector correlationVector = SllCorrelationVectorManager.SetCorrelationVectorAtRequestEntry(requestMessage);
            EventTraceActivity serverTraceId = new EventTraceActivity { CorrelationVectorV4 = correlationVector };
            EventTraceActivity requestTraceId = GetOrCreateCorrelationIdFromHeader(requestMessage);

            if (!context.Items.ContainsKey(PaymentConstants.Web.Properties.TrackingId))
            {
                string trackingId = GetOrCreateTrackingIdFromHeader(requestMessage);
                context.Items[PaymentConstants.Web.Properties.TrackingId] = trackingId;
                requestMessage.Options.Set(new HttpRequestOptionsKey<object>(PaymentConstants.Web.Properties.TrackingId), trackingId);
            }

            if (!context.Items.ContainsKey(PaymentConstants.Web.Properties.ServerTraceId))
            {
                context.Items[PaymentConstants.Web.Properties.ServerTraceId] = serverTraceId;
            }

            if (!context.Items.ContainsKey(PaymentConstants.Web.Properties.ClientTraceId))
            {
                context.Items[PaymentConstants.Web.Properties.ClientTraceId] = requestTraceId;
            }

            string reqPayload = await request.GetRequestPayload();
            requestMessage.Content = new StringContent(reqPayload ?? string.Empty, Encoding.UTF8, request.ContentType);

            try
            {
                await this._next(context);

                CopyItemsToOptions();

                context.Response.Headers.Add("x-info", "px-azure");
                context.Response.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, requestTraceId.ActivityId.ToString());
                foreach (DependenciesCertInfo dependencyNameUsingCert in Enum.GetValues(typeof(DependenciesCertInfo)))
                {
                    this.RemoveRequestContextItem(dependencyNameUsingCert.ToString());
                }

                var responsePayload = await context.Response.GetResponsePayloadAsync();
                var responseMessage = new HttpResponseMessage((HttpStatusCode)context.Response.StatusCode)
                {
                    Content = new StringContent(responsePayload ?? string.Empty, Encoding.UTF8, context.Response.ContentType)
                };

                foreach (var header in context.Response.Headers)
                {
                    responseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }

                await this.TraceOperation(requestMessage, responseMessage, request.GetOperationNameWithPendingOnInfo(), startTime, stopwatch, string.Empty, requestTraceId, serverTraceId);
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        /// <summary>
        /// SLL Tracing
        /// </summary>
        /// <param name="request">Inbound request message to be traced.</param>
        /// <param name="response">Outbound response message to be traced.</param>
        /// <param name="operationName">The logical name of the operation being performed.</param>
        /// <param name="startTime">Operation start time.</param>
        /// <param name="stopwatch">Stopwatch to measure time taken for operation.</param>
        /// <param name="additionalMessage">Additional message to be traced.</param>
        /// <param name="requestTraceId">1. Caller created ExternalActivityId to payments service, 2. PaymentsActivityId to dependent services.</param>
        /// <param name="serverTraceId">1. PaymentsActivityId to payments service, 2. Payment created ExternalActivityId to dependent services.</param>
        /// <returns>A task representing the async work.</returns>
        protected virtual async Task TraceOperation(HttpRequestMessage request, HttpResponseMessage response, string operationName, string startTime, Stopwatch stopwatch, string additionalMessage, EventTraceActivity requestTraceId, EventTraceActivity serverTraceId)
        {
            try
            {
                string accountId = null;
                string paymentInstrumentId = null;

                // To get the accountId and paymentInstrumentId, first look at the properties in the request, if not present: look at the route data.
                accountId = request.GetProperty(PaymentConstants.Web.InstrumentManagementProperties.AccountId) as string;
                paymentInstrumentId = request.GetProperty(PaymentConstants.Web.InstrumentManagementProperties.InstrumentId) as string;

                // Don't even get the route data if both are present
                if (string.IsNullOrWhiteSpace(accountId) || string.IsNullOrWhiteSpace(paymentInstrumentId))
                {
                    var data = request.GetRouteDataSafe();

                    if (data != null)
                    {
                        if (string.IsNullOrWhiteSpace(accountId) && data.TryGetValue("accountId", out var accId))
                        {
                            accountId = accId?.ToString();
                        }

                        if (string.IsNullOrWhiteSpace(paymentInstrumentId) && data.TryGetValue("instrumentId", out var piId))
                        {
                            paymentInstrumentId = piId?.ToString();
                        }
                    }
                }

                string paymentMethodFamily = request.GetProperty(InstrumentManagementProperties.PaymentMethodFamily) as string;
                string paymentMethodType = request.GetProperty(InstrumentManagementProperties.PaymentMethodType) as string;
                string country = request.GetProperty(InstrumentManagementProperties.Country) as string;
                string requestPayload = string.Empty;
                string responsePaylod = string.Empty;
                string requestTraceActivityId = requestTraceId.ActivityId.ToString();
                string serverTraceActivityId = serverTraceId.ActivityId.ToString();

                string requestOrigin = request.GetRequestHeader(PaymentConstants.HttpHeaders.Origin) ?? DefaultLogValue;
                string requestMessage = (request.GetProperty(InstrumentManagementProperties.Message) as string) ?? DefaultLogValue;
                string message = $"Origin: {requestOrigin}; Message: {requestMessage}";

                string certInfo = request.GetProperty(Properties.CertInfo) as string ?? DefaultLogValue;
                string certConfig = request.GetProperty(Properties.CertConfig) as string ?? DefaultLogValue;
                string certPrinciple = request.GetProperty(Properties.CertPrinciple) as string ?? DefaultLogValue;
                string certAuthError = request.GetProperty(Properties.CertAuthError) as string ?? DefaultLogValue;
                string certAuthInfo = request.GetProperty(Properties.CertAuthInfo) as string ?? DefaultLogValue;
                string tokenAuthWarning = request.GetProperty(Properties.TokenAuthWarning) as string ?? DefaultLogValue;
                string tokenAuthSucceed = request.GetProperty(Properties.TokenAuthResult) as string ?? DefaultLogValue;
                string tokenAuthError = request.GetProperty(Properties.TokenAuthError) as string ?? DefaultLogValue;
                string certAuthSucceed = request.GetProperty(Properties.CertAuthResult) as string ?? DefaultLogValue;
                string caller = request.GetProperty(Properties.CallerName) as string ?? DefaultLogValue;
                string authenticationInfo = $"tokenAuthSucceed:{tokenAuthSucceed}, certAuthSucceed:{certAuthSucceed}, caller: {caller}, certInfo: {certInfo}; certConfig:{certConfig}; certPrinciple:{certPrinciple}; certAuthInfo: {certAuthInfo}; certAuthError: {certAuthError}; tokenAuthWarning: {tokenAuthWarning}; tokenAuthError: {tokenAuthError}";
                string partner = request.GetProperty(Properties.Partner) as string;
                string pidlOperation = request.GetProperty(Properties.PidlOperation) as string;
                string avsSuggest = request.GetProperty(Properties.AvsSuggest) as string;
                string errorCode = null;
                string errorMessage = null;

                if (!response.IsSuccessStatusCode)
                {
                    errorCode = request.GetProperty(InstrumentManagementProperties.ErrorCode) as string;
                    errorMessage = request.GetProperty(InstrumentManagementProperties.ErrorMessage) as string;
                }

                if (request.GetProperty(InstrumentManagementProperties.SkipRequestLogging) == null)
                {
                    requestPayload = await request.GetRequestPayload();
                    if (request.GetProperty(InstrumentManagementProperties.SkipReponseDetailsLogging) == null)
                    {
                        responsePaylod = await response.GetResponsePayload();
                    }
                    else
                    {
                        responsePaylod = request.GetProperty(InstrumentManagementProperties.SkipReponseDetailsLogging) as string;
                    }
                }

                bool isTest = request.HasTestContext();

                if (LoggingConfig.Mode == LoggingMode.Sll)
                {
                    SllWebLogger.TracePXServiceIncomingOperation(
                    operationName,
                    accountId,
                    paymentInstrumentId,
                    paymentMethodFamily,
                    paymentMethodType,
                    country,
                    request,
                    response,
                    requestPayload,
                    responsePaylod,
                    startTime,
                    stopwatch,
                    requestTraceActivityId,
                    authenticationInfo,
                    serverTraceActivityId,
                    message,
                    errorCode,
                    errorMessage,
                    isTest,
                    partner: partner,
                    pidlOperation: pidlOperation,
                    avsSuggest: avsSuggest);
                }
                else if (LoggingConfig.Mode == LoggingMode.OpenTelemetry)
                {
                    Logger.Qos.TracePXServiceIncomingOperation(
                        operationName,
                        accountId,
                        paymentInstrumentId,
                        paymentMethodFamily,
                        paymentMethodType,
                        country,
                        request,
                        response,
                        requestPayload,
                        responsePaylod,
                        startTime,
                        stopwatch,
                        requestTraceActivityId,
                        authenticationInfo,
                        serverTraceActivityId,
                        message,
                        errorCode,
                        errorMessage,
                        isTest,
                        partner: partner,
                        pidlOperation: pidlOperation,
                        avsSuggest: avsSuggest);
                }
                else
                {
                    SllWebLogger.TracePXServiceIncomingOperation(
                    operationName,
                    accountId,
                    paymentInstrumentId,
                    paymentMethodFamily,
                    paymentMethodType,
                    country,
                    request,
                    response,
                    requestPayload,
                    responsePaylod,
                    startTime,
                    stopwatch,
                    requestTraceActivityId,
                    authenticationInfo,
                    serverTraceActivityId,
                    message,
                    errorCode,
                    errorMessage,
                    isTest,
                    partner: partner,
                    pidlOperation: pidlOperation,
                    avsSuggest: avsSuggest);

                    Logger.Qos.TracePXServiceIncomingOperation(
                        operationName,
                        accountId,
                        paymentInstrumentId,
                        paymentMethodFamily,
                        paymentMethodType,
                        country,
                        request,
                        response,
                        requestPayload,
                        responsePaylod,
                        startTime,
                        stopwatch,
                        requestTraceActivityId,
                        authenticationInfo,
                        serverTraceActivityId,
                        message,
                        errorCode,
                        errorMessage,
                        isTest,
                        partner: partner,
                        pidlOperation: pidlOperation,
                        avsSuggest: avsSuggest);
                }

                AuditLogger.AuditIncomingCall(
                operationName,
                request,
                response);

                this.LogIncomingRequestToAppInsight?.Invoke(
                    operationName,
                    accountId,
                    paymentInstrumentId,
                    paymentMethodFamily,
                    paymentMethodType,
                    country,
                    request,
                    response,
                    requestPayload,
                    responsePaylod,
                    startTime,
                    requestTraceActivityId,
                    serverTraceActivityId,
                    message,
                    errorCode,
                    errorMessage);
            }
            catch (Exception ex)
            {
                this.LogError?.Invoke("PXTraceCorrelationHandler.TraceClientOperation: " + ex.Message, requestTraceId);
            }
        }

        /// <summary>
        /// SLL Tracing
        /// </summary>
        /// <param name="request">Inbound request message to be traced.</param>
        /// <param name="response">Outbound response message to be traced.</param>
        /// <param name="operationName">The logical name of the operation being performed.</param>
        /// <param name="serviceName">The name of the service being called.</param>
        /// <param name="startTime">Operation start time.</param>
        /// <param name="stopwatch">Stopwatch to measure time taken for operation.</param>
        /// <param name="additionalMessage">Additional message to be traced.</param>
        /// <param name="requestTraceId">1. Caller created ExternalActivityId to payments service, 2. PaymentsActivityId to dependent services.</param>
        /// <returns>A task representing the async work.</returns>
        protected virtual async Task TraceClientOperation(HttpRequestMessage request, HttpResponseMessage response, string operationName, string serviceName, string startTime, Stopwatch stopwatch, string additionalMessage, EventTraceActivity requestTraceId)
        {
            try
            {
                // Here we set the SLL thread static instance of the correlation vector.
                // We do this because we prefer passing around the correlation vector explicitly
                // to it living in a thread static.
                // This works only for outgoing calls because the EventTraceActivity is set as a request property.
                if (requestTraceId.CorrelationVectorV4 != null)
                {
                    Sll.Context.Vector = requestTraceId.CorrelationVectorV4;
                }

                string responseContent;
                try
                {
                    // Sometimes when the request is cancelled or there is an error in the network stream,
                    // exceptions occour because the returned stream says CanSeek = false.
                    // We have this try catch to not cause an exception when trying to log the response.
                    // We also set the status code to GatewayTimeout so that we don't consider the call successful.
                    responseContent = await response.Content.ReadAsStringAsync();
                }
                catch (InvalidOperationException ex)
                {
                    response.StatusCode = HttpStatusCode.GatewayTimeout;
                    responseContent = ex.ToString();
                }

                string certInfo = request.GetProperty(PaymentConstants.Web.Properties.CertInfo) as string ?? "<none>";
                string servicePointData = request.GetProperty(Properties.ServicePointData) as string ?? "<none>";

                if (LoggingConfig.Mode == LoggingMode.Sll)
                {
                    SllWebLogger.TracePXServiceOutgoingOperation(
                    operationName,
                    serviceName,
                    request,
                    response,
                    await request.GetRequestPayload(),
                    responseContent,
                    startTime,
                    stopwatch,
                    requestTraceId.ActivityId.ToString(),
                    string.Empty,
                    certInfo,
                    servicePointData);
                }
                else if (LoggingConfig.Mode == LoggingMode.OpenTelemetry)
                {
                    Logger.Qos.TracePXServiceOutgoingOperation(
                        operationName,
                        serviceName,
                        request,
                        response,
                        await request.GetRequestPayload(),
                        responseContent,
                        startTime,
                        stopwatch,
                        requestTraceId.ActivityId.ToString(),
                        string.Empty,
                        certInfo,
                        servicePointData);
                }
                else
                {
                    SllWebLogger.TracePXServiceOutgoingOperation(
                    operationName,
                    serviceName,
                    request,
                    response,
                    await request.GetRequestPayload(),
                    responseContent,
                    startTime,
                    stopwatch,
                    requestTraceId.ActivityId.ToString(),
                    string.Empty,
                    certInfo,
                    servicePointData);

                    Logger.Qos.TracePXServiceOutgoingOperation(
                        operationName,
                        serviceName,
                        request,
                        response,
                        await request.GetRequestPayload(),
                        responseContent,
                        startTime,
                        stopwatch,
                        requestTraceId.ActivityId.ToString(),
                        string.Empty,
                        certInfo,
                        servicePointData);
                }

                this.LogToApplicationInsight?.Invoke(
                    operationName,
                    serviceName,
                    request,
                    response,
                    await request.GetRequestPayload(),
                    responseContent,
                    startTime,
                    requestTraceId.ActivityId.ToString());
            }
            catch (Exception ex)
            {
                this.LogError?.Invoke("PXTraceCorrelationHandler.TraceClientOperation: " + ex.Message, requestTraceId);
            }
        }

        private static string GetOrCreateTrackingIdFromHeader(HttpRequestMessage request)
        {
            string trackingId = null;
            IEnumerable<string> trackingIdHeaderValues;
            if (request.Headers.TryGetValues(PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, out trackingIdHeaderValues))
            {
                foreach (string headerValue in trackingIdHeaderValues)
                {
                    if (!string.IsNullOrEmpty(headerValue))
                    {
                        trackingId = headerValue;
                        break;
                    }
                }
            }

            if (trackingId == null)
            {
                trackingId = Guid.Empty.ToString();
            }

            return trackingId;
        }

        private void RemoveRequestContextItem(string key)
        {
            IDictionary<object, object> items = this.httpContextAccessor?.HttpContext?.Items;
            if (items != null && items.ContainsKey(key))
            {
                items.Remove(key);
            }
        }

        private static void SetConnectionLeaseTimeout(HttpRequestMessage request)
        {
            if (request == null)
            {
                return;
            }

            try
            {
                ServicePoint servicePoint = ServicePointManager.FindServicePoint(request.RequestUri);

                if (servicePoint != null)
                {
                    string managerData = string.Format(
                  "ConnectionLeaseTimeout: {0} | MaxIdleTime: {1} | Hash: {2} | Connections: {3} | ConnectionLimit: {4}",
                  servicePoint.ConnectionLeaseTimeout,
                  servicePoint.MaxIdleTime,
                  servicePoint.GetHashCode(),
                  servicePoint.CurrentConnections,
                  servicePoint.ConnectionLimit);

                    // Grab these properties before they are checked and potentially reset for logs to verify if they are going back to defaults
                    request.Properties[PaymentConstants.Web.Properties.ServicePointData] = managerData;
                }

                /*
                 * We need to periodically check that ConnectionLeaseTimeout is correctly set due to an issue where
                 * ServicePointManager will occasionally be garbage collected and re-created even when MaxIdleTime is set to -1,
                 * which resets the ConnectionLeaseTimeout back to it's default.
                 * This was the reason we still had issues after our initial fix.
                */
                if (servicePoint.ConnectionLeaseTimeout != DefaultConnectionLeaseTimeoutInMs)
                {
                    servicePoint.MaxIdleTime = DefaultMaxIdleTime;
                    servicePoint.ConnectionLeaseTimeout = DefaultConnectionLeaseTimeoutInMs;
                }
            }
            catch (Exception ex)
            {
                if (LoggingConfig.Mode == LoggingMode.Sll)
                {
                    SllWebLogger.TracePXServiceException(string.Format("Exception thrown while configuring ConnectionLeaseTimeout of ServicePoint {0}: {1}", request?.RequestUri?.ToString(), ex.ToString()), EventTraceActivity.Empty);
                }
                else if (LoggingConfig.Mode == LoggingMode.OpenTelemetry)
                {
                    Logger.Qos.TracePXServiceException(string.Format("Exception thrown while configuring ConnectionLeaseTimeout of ServicePoint {0}: {1}", request?.RequestUri?.ToString(), ex.ToString()), EventTraceActivity.Empty);
                }
                else
                {
                    SllWebLogger.TracePXServiceException(string.Format("Exception thrown while configuring ConnectionLeaseTimeout of ServicePoint {0}: {1}", request?.RequestUri?.ToString(), ex.ToString()), EventTraceActivity.Empty);
                    Logger.Qos.TracePXServiceException(string.Format("Exception thrown while configuring ConnectionLeaseTimeout of ServicePoint {0}: {1}", request?.RequestUri?.ToString(), ex.ToString()), EventTraceActivity.Empty);
                }
            }
        }

        private string GetOperationName(HttpRequestMessage request)
        {
            string operationName = request.GetOperationName();
            if (operationName == null)
            {
                // If the operation name does not exist in the request properties, then parse the request data to construct operation name.
                var data = request.GetRouteDataSafe();

                StringBuilder counterNameBuilder = new StringBuilder();
                if (data != null)
                {
                    string controller = data.TryGetValue("controller", out var controllerObj) ? controllerObj?.ToString() : null;

                    if (string.Equals(controller, PaymentInstrumentOperationsController, StringComparison.InvariantCultureIgnoreCase))
                    {
                        controller = PaymentInstrumentsController;
                    }

                    counterNameBuilder.Append(controller);
                    counterNameBuilder.Append("-");
                    counterNameBuilder.Append(request.Method.ToString());

                    if (data.TryGetValue("action", out var actionObj) && actionObj != null)
                    {
                        counterNameBuilder.Append("-");
                        counterNameBuilder.Append(actionObj.ToString());
                    }
                }
                else
                {
                    // In case there is no request data, get the action name from the request properties
                    string actionName = request.GetActionName();
                    if (actionName != null)
                    {
                        counterNameBuilder.Append(this.ServiceName);
                        counterNameBuilder.Append("-");
                        counterNameBuilder.Append(request.Method);
                        counterNameBuilder.Append("-");
                        counterNameBuilder.Append(actionName);
                    }
                    else
                    {
                        // If no action name was given, mark it as Unknown. The request sender should try to add this property to the request.
                        counterNameBuilder.Append(this.ServiceName);
                        counterNameBuilder.Append("-");
                        counterNameBuilder.Append(request.Method);
                        counterNameBuilder.Append("-Unknown");
                    }
                }

                operationName = counterNameBuilder.ToString();
            }

            return operationName;
        }

        private async Task<HttpResponseMessage> SendAsyncIncoming(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string startTime = System.DateTime.UtcNow.ToString("o");

            string operationName = this.GetOperationName(request);
            if (!request.Properties.ContainsKey(PaymentConstants.Web.Properties.OperationName))
            {
                request.Properties.Add(PaymentConstants.Web.Properties.OperationName, operationName);
            }

            CorrelationVector correlationVector = SllCorrelationVectorManager.SetCorrelationVectorAtRequestEntry(request);
            EventTraceActivity serverTraceId = new EventTraceActivity { CorrelationVectorV4 = correlationVector };
            EventTraceActivity requestTraceId = GetOrCreateCorrelationIdFromHeader(request);

            if (!request.Properties.ContainsKey(PaymentConstants.Web.Properties.TrackingId))
            {
                string trackingId = GetOrCreateTrackingIdFromHeader(request);
                request.Properties.Add(PaymentConstants.Web.Properties.TrackingId, trackingId);
            }

            // Save this for other parts of the pipeline.
            if (!request.Properties.ContainsKey(PaymentConstants.Web.Properties.ServerTraceId))
            {
                // If there are multiple requests from client with same correlation id in short span of time, 
                // then the requests overlap.To avoid this we do trace transfer from requestTraceId to ServerTraceId.
                // All the payments servertraces will be correlated with serverTraceId.
                request.Properties.Add(PaymentConstants.Web.Properties.ServerTraceId, serverTraceId);
            }
            else
            {
                Debug.Assert(
                    ((EventTraceActivity)request.Properties[PaymentConstants.Web.Properties.ServerTraceId]).ActivityId == serverTraceId.ActivityId,
                    "Should never hit here, in which case trace IDs should be the same.");
            }

            if (!request.Properties.ContainsKey(PaymentConstants.Web.Properties.ClientTraceId))
            {
                // Save the clientTraceId for the logging purpose
                request.Properties.Add(PaymentConstants.Web.Properties.ClientTraceId, requestTraceId);
            }

            // Need set the request content before processing.
            await request.GetRequestPayload();

            try
            {
                HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

                // if the request is made from clients like Billing etc, then update the requestID to TraceActivity
                response.Headers.Add("x-info", "px-azure");
                response.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, requestTraceId.ActivityId.ToString());
                foreach (DependenciesCertInfo dependencyNameUsingCert in Enum.GetValues(typeof(DependenciesCertInfo)))
                {
                    this.RemoveRequestContextItem(dependencyNameUsingCert.ToString());
                }

                await this.TraceOperation(request, response, request.GetOperationNameWithPendingOnInfo(), startTime, stopwatch, string.Empty, requestTraceId, serverTraceId);

                return response;
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        private async Task<HttpResponseMessage> SendAsyncOutgoing(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string startTime = System.DateTime.UtcNow.ToString("o");

            string operationName = this.GetOperationName(request);
            EventTraceActivity requestTraceId = null;

            object result;
            if (request.Properties.TryGetValue(PaymentConstants.Web.Properties.ServerTraceId, out result))
            {
                requestTraceId = (EventTraceActivity)result;
            }

            if (requestTraceId == null)
            {
                requestTraceId = GetOrCreateCorrelationIdFromHeader(request);
                request.Properties[PaymentConstants.Web.Properties.ServerTraceId] = requestTraceId;
            }

            if (!request.Properties.ContainsKey(PaymentConstants.Web.Properties.TrackingId))
            {
                string trackingId = GetOrCreateTrackingIdFromHeader(request);
                request.Properties.Add(PaymentConstants.Web.Properties.TrackingId, trackingId);
            }

            EventTraceActivity outgoingCorrelationId = new EventTraceActivity();

            // If the request contains a CorrelationId already,
            // Update the correlation ID on the request to the new tracking guid, so that we can uniquely 
            // identify dependent service request.
            request.Headers.Remove(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId);
            request.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, outgoingCorrelationId.ActivityId.ToString());

            // Need set the request content before processing.
            await request.GetRequestPayload();

            try
            {
                SetConnectionLeaseTimeout(request);

                HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

                // if the request is made to dependent services like payments or risk etc, then update the requestID to serverTraceActivity
                response.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, outgoingCorrelationId.ActivityId.ToString());
                await this.TraceClientOperation(request, response, operationName, this.ServiceName, startTime, stopwatch, string.Empty, requestTraceId);

                return response;
            }
            catch (Exception ex)
            {
                try
                {
                    HttpResponseMessage errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    {
                        Content = new StringContent(ex.ToString()) // CodeQL [SM00431] Safe to use. It writes sensitive data only to internal logging systems, not to response payloads sent to users.
                    };

                    errorResponse.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, outgoingCorrelationId.ActivityId.ToString());
                    await this.TraceClientOperation(request, errorResponse, operationName, this.ServiceName, startTime, stopwatch, string.Empty, requestTraceId);
                }
                catch
                {
                }

                throw;
            }
            finally
            {
                stopwatch.Stop();
            }
        }
    }
}