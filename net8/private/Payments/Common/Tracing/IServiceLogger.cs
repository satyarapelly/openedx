// <copyright file="IserviceLogger.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IServiceLogger
    {
        void LogEvent(EventTraceActivity traceActivityId, string cV, string eventName, object parameters);

        void LogError(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, string message, string parameters);

        void LogWarning(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, string message, string parameters);

        void LogInformational(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, string message, string parameters);

        void LogVerbose(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, string message, string parameters);

        void LogActivityTransfer(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, EventTraceActivity relatedTraceActivityId, string message, string parameters);

        void LogApplicationStart(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, string message, string parameters);

        void LogApplicationStop(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, int componentEventId, string message, string parameters);

        void LogMetric(EventTraceActivity traceActivityId, string cV, string component, string componentEventName, string instance, double counterValue, bool absolute, string parameters);
    }
}
