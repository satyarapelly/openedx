// <copyright file="ApplicationInsightsProvider.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Http;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Tracing;

    /// <summary>
    /// Application Insights Provider
    /// </summary>
    public static class ApplicationInsightsProvider
    {
        private static readonly object syncRoot = new object();

        private static TelemetryClient instance = null;

        internal static TelemetryClient Instance
        {
            get
            {
                lock (syncRoot)
                {
                    return instance;
                }
            }

            private set
            {
                lock (syncRoot)
                {
                    instance = value;
                }
            }
        }

        /// <summary>
        /// Setup App Insights
        /// </summary>
        /// <param name="instrumentationKey">The Application Insights instrumentation key.</param>
        /// <param name="developerMode">Indicates if application insights is operating in developer mode.</param>
        public static void SetupAppInsightsConfiguration(string instrumentationKey, bool developerMode)
        {
            if (!string.IsNullOrEmpty(instrumentationKey))
            {
                // Active telemetry client setup
                TelemetryConfiguration.Active.InstrumentationKey = instrumentationKey;
                TelemetryConfiguration.Active.TelemetryChannel.DeveloperMode = developerMode;
            }
            else
            {
                // Disabled telemetry client setup
                TelemetryConfiguration.Active.DisableTelemetry = true;
            }

            Instance = new TelemetryClient(TelemetryConfiguration.Active);
        }

        public static void LogOutgoingOperation(
            string operationName,
            string serviceName,
            HttpRequestMessage request,
            HttpResponseMessage response,
            string requestPayload,
            string responsePayload,
            string startTime,
            string requestTraceId)
        {
            var properties = new Dictionary<string, string>
            {
                { "OperationName", operationName },
                { "ServiceName", serviceName },
                { "RequestTraceId", requestTraceId },
                { "RequestHeader", SllLogger.Masker.MaskHeader(request.GetRequestHeaderString()) },
                { "ResponseHeader", response != null ? response.GetResponseHeaderString() : string.Empty },
                { "RequestDetails", SllLogger.Masker.MaskSingle(requestPayload) },
                { "ResponseDetails",  SllLogger.Masker.MaskSingle(responsePayload) },
                { "ProtocolStatusCode",  response != null ? Convert.ToInt32(response.StatusCode).ToString() : string.Empty },
                { "TargetUri",  request.RequestUri.IsAbsoluteUri ? request.RequestUri.AbsoluteUri : request.RequestUri.ToString() },
                { "DependencyName", serviceName },
                { "OperationStartTime", startTime },
                { "Cv", request.GetCorrelationVector().ToString() }
            };

            Instance?.TrackEvent("OutgoingOperation", properties);
        }

        public static void LogIncomingOperation(
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
            string requestTraceId,
            string serverTraceId = null,
            string message = null,
            string errorCode = null,
            string errorMessage = null)
        {
            var properties = new Dictionary<string, string>
            {
                { "OperationName", operationName },
                { "AccountId", accountId },
                { "PaymentInstrumentId", paymentInstrumentId },
                { "PaymentMethodFamily", paymentMethodFamily },
                { "PaymentMethodType", paymentMethodType },
                { "Country", country },
                { "RequestTraceId", requestTraceId },
                { "ServerTraceId", serverTraceId },
                { "Message", message },
                { "ErrorCode", errorCode },
                { "ErrorMessage", errorMessage },
                { "RequestHeader", SllLogger.Masker.MaskHeader(request.GetRequestHeaderString()) },
                { "ResponseHeader", response != null ? response.GetResponseHeaderString() : string.Empty },
                { "RequestDetails", SllLogger.Masker.MaskSingle(requestPayload) },
                { "ResponseDetails",  SllLogger.Masker.MaskSingle(responsePayload) },
                { "ProtocolStatusCode",  response != null ? Convert.ToInt32(response.StatusCode).ToString() : string.Empty },
                { "TargetUri",  request.RequestUri.IsAbsoluteUri ? request.RequestUri.AbsoluteUri : request.RequestUri.ToString() },
                { "OperationStartTime", startTime },
                { "Cv", request.GetCorrelationVector().ToString() }
            };

            Instance?.TrackEvent("IncomingOperation", properties);
        }
    }
}