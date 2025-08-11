// <copyright file="Logger.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
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

        
    }
}
