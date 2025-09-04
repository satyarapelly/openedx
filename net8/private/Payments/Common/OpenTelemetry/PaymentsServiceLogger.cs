// <copyright file="PaymentsServiceLogger.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.OpenTelemetry
{
    using System;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// Payments Event logger.
    /// </summary>
    public class PaymentsServiceLogger : IServiceLogger
    {
        private const string LogMessageFormat =
            "{ActivityId} {RelatedActivityId} {CV} {Component} {ComponentEventName} {EventName} {Message} {Parameters}";

        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentsServiceLogger"/> class.
        /// </summary>
        /// <param name="logger">The logger instance</param>
        internal PaymentsServiceLogger(ILogger logger)
        {
            this.logger = logger;
        }

        public void LogError(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, string message, string parameters)
        {
            this.logger.LogError(
                LogMessageFormat,
                traceActivityId.ActivityId.ToString(),
                string.Empty,
                cV,
                component,
                componentEventName,
                "BaseError",
                message,
                parameters?.Length > 25000 ? parameters.Substring(0, 25000) : parameters);
        }

        public void LogWarning(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, string message, string parameters)
        {
            this.logger.LogWarning(
                LogMessageFormat,
                traceActivityId.ActivityId.ToString(),
                string.Empty,
                cV,
                component,
                componentEventName,
                "BaseWarning",
                message,
                parameters?.Length > 25000 ? parameters.Substring(0, 25000) : parameters);
        }

        public void LogInformational(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, string message, string parameters)
        {
            this.logger.LogInformation(
                LogMessageFormat,
                traceActivityId.ActivityId.ToString(),
                string.Empty,
                cV,
                component,
                componentEventName,
                "BaseInformational",
                message,
                parameters?.Length > 25000 ? parameters.Substring(0, 25000) : parameters);
        }

        public void LogVerbose(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, string message, string parameters)
        {
            this.logger.LogDebug(
                LogMessageFormat,
                traceActivityId.ActivityId.ToString(),
                string.Empty,
                cV,
                component,
                componentEventName,
                "BaseVerbose",
                message,
                parameters?.Length > 25000 ? parameters.Substring(0, 25000) : parameters);
        }

        public void LogActivityTransfer(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, EventTraceActivity relatedTraceActivityId, string message, string parameters)
        {
            this.logger.LogInformation(
                LogMessageFormat,
                traceActivityId.ActivityId.ToString(),
                relatedTraceActivityId?.ActivityId.ToString() ?? string.Empty,
                cV,
                component,
                componentEventName,
                "BaseActivityTransfer",
                message,
                parameters?.Length > 25000 ? parameters.Substring(0, 25000) : parameters);
        }

        public void LogApplicationStart(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, string message, string parameters)
        {
            this.logger.LogInformation(
                LogMessageFormat,
                traceActivityId.ActivityId.ToString(),
                string.Empty,
                cV,
                component,
                componentEventName,
                "BaseApplicationStart",
                message,
                parameters?.Length > 25000 ? parameters.Substring(0, 25000) : parameters);
        }

        public void LogApplicationStop(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, string message, string parameters)
        {
            this.logger.LogInformation(
                LogMessageFormat,
                traceActivityId.ActivityId.ToString(),
                string.Empty,
                cV,
                component,
                componentEventName,
                "BaseApplicationStop",
                message,
                parameters?.Length > 25000 ? parameters.Substring(0, 25000) : parameters);
        }

        public void LogMetric(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, string instance, double counterValue, bool absolute, string parameters)
        {
            var additionalParameters = new
            {
                instance,
                counterValue,
                absolute,
                parameters
            };

            string serializedParameters;
            try
            {
                serializedParameters = JsonConvert.SerializeObject(additionalParameters);
            }
            catch (Exception ex)
            {
                serializedParameters = $"Serialization failed: {ex.Message}";
            }

            this.logger.LogDebug(
                LogMessageFormat,
                traceActivityId.ActivityId.ToString(),
                string.Empty,
                cV,
                component,
                componentEventName,
                "BaseMetric",
                string.Empty,
                serializedParameters);
        }

        public void LogEvent(EventTraceActivity traceActivityId, string cV, string eventName, object parameters)
        {
            string parametersString;

            try
            {
                parametersString = parameters != null ? JsonConvert.SerializeObject(parameters) : string.Empty;
            }
            catch (Exception ex)
            {
                parametersString = $"Serialization failed: {ex.Message}";
            }

            this.logger.LogInformation(
                LogMessageFormat,
                traceActivityId.ActivityId.ToString(),
                string.Empty,
                cV,
                string.Empty,
                string.Empty,
                eventName,
                string.Empty,
                parametersString.Length > 25000 ? parametersString.Substring(0, 25000) : parametersString);
        }
    }
}