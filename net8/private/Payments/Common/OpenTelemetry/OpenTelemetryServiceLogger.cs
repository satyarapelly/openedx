// <copyright file="OpenTelemetryServiceLogger.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.OpenTelemetry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Extensions.Logging;

    public class OpenTelemetryServiceLogger : IServiceLogger
    {
        private readonly List<IServiceLogger> loggers;

        public OpenTelemetryServiceLogger(params IServiceLogger[] loggers)
        {
            this.loggers = new List<IServiceLogger>(loggers.Where(l => l != null));
            if (!this.loggers.Any())
            {
                throw new ArgumentException("At least one logger must be provided.");
            }
        }

        public static OpenTelemetryServiceLogger Create(ILogger otelLogger)
        {
            OpenTelemetryServiceLogger logger = new OpenTelemetryServiceLogger(new PaymentsServiceLogger(otelLogger));
            return logger;
        }

        public void LogActivityTransfer(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, EventTraceActivity relatedTraceActivityId, string message, string parameters)
        {
            foreach (IServiceLogger logger in this.loggers)
            {
                logger.LogActivityTransfer(traceActivityId, cV, component, componentEventName, componentEventId, relatedTraceActivityId, message, parameters);
            }
        }

        public void LogApplicationStart(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, string message, string parameters)
        {
            foreach (IServiceLogger logger in this.loggers)
            {
                logger.LogApplicationStart(traceActivityId, cV, component, componentEventName, componentEventId, message, parameters);
            }
        }

        public void LogApplicationStop(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, string message, string parameters)
        {
            foreach (IServiceLogger logger in this.loggers)
            {
                logger.LogApplicationStop(traceActivityId, cV, component, componentEventName, componentEventId, message, parameters);
            }
        }

        public void LogError(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, string message, string parameters)
        {
            foreach (IServiceLogger logger in this.loggers)
            {
                logger.LogError(traceActivityId, cV, component, componentEventName, componentEventId, message, parameters);
            }
        }

        public void LogEvent(EventTraceActivity traceActivityId, string cV, string eventName, object parameters)
        {
            foreach (IServiceLogger logger in this.loggers)
            {
                logger.LogEvent(traceActivityId, cV, eventName, parameters);
            }
        }

        public void LogException(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, string message, string parameters)
        {
            foreach (IServiceLogger logger in this.loggers)
            {
                logger.LogError(traceActivityId, cV, component, componentEventName, componentEventId, message, parameters);
            }
        }

        public void LogInformational(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, string message, string parameters)
        {
            foreach (IServiceLogger logger in this.loggers)
            {
                logger.LogInformational(traceActivityId, cV, component, componentEventName, componentEventId, message, parameters);
            }
        }

        public void LogMetric(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, string instance, double counterValue, bool absolute, string parameters)
        {
            foreach (IServiceLogger logger in this.loggers)
            {
                logger.LogMetric(traceActivityId, cV, component, componentEventName, instance, counterValue, absolute, parameters);
            }
        }

        public void LogVerbose(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, string message, string parameters)
        {
            foreach (IServiceLogger logger in this.loggers)
            {
                logger.LogVerbose(traceActivityId, cV, component, componentEventName, componentEventId, message, parameters);
            }
        }

        public void LogWarning(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, string message, string parameters)
        {
            foreach (IServiceLogger logger in this.loggers)
            {
                logger.LogWarning(traceActivityId, cV, component, componentEventName, componentEventId, message, parameters);
            }
        }
    }
}
