// <copyright file="PerformanceTracingScope.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    using System;
    using System.Diagnostics;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Tracing;

    /// <summary>
    /// Performance Instrumentation/Tracing class to log the performance of all the 
    /// calls inside this scope
    /// </summary>
    public sealed class PerformanceTracingScope : IDisposable
    {
        private const string UtcTimestampFormat = @"yyyy-MM-dd HH:mm:ss:fff";

        private static readonly DateTime EpochStartTimestamp = new DateTime(1970, 1, 1);        

        private Stopwatch stopwatch = null;
        private string eventName;
        private EventTraceActivity traceActivityId;
        private string perfEventCorrelationId;

        public PerformanceTracingScope(string eventName, EventTraceActivity traceActivityId)
        {
            ArgumentValidator.EnsureNotNullOrWhitespace(eventName, "eventName");
            ArgumentValidator.EnsureNotNull(traceActivityId, "eventTraceActivity");

            this.eventName = eventName;
            this.traceActivityId = traceActivityId;
            this.StartTracing();
        }

        public void Dispose()
        {
            this.EndTracing();
        }

        private void StartTracing()
        {
            this.perfEventCorrelationId = Guid.NewGuid().ToString();
            this.stopwatch = new Stopwatch();
            this.stopwatch.Start();

            DateTime utcNow = DateTime.UtcNow;
            double currentMillis = (utcNow - EpochStartTimestamp).TotalMilliseconds;
        }

        private void EndTracing()
        {
            this.stopwatch.Stop();

            DateTime utcNow = DateTime.UtcNow;
            double currentMillis = (utcNow - EpochStartTimestamp).TotalMilliseconds;
        }
    }
}
