// <copyright file="ProviderInstrumentationScope.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// The scope used to instrument a provider operation. 
    /// It can be used for recording provider specific counters and provider to transaction result interpretion counters.
    /// It can take handler to trace the start and completion stamps without details input/output.
    /// </summary>
    public class ProviderInstrumentationScope : IDisposable
    {
        private Action<bool, EventTraceActivity, long> traceComplete;
        private Stopwatch scopeTimer;
        private bool connectionError;
        private bool processingError;

        public ProviderInstrumentationScope(ProviderInstrumentationCounters counters, EventTraceActivity traceActivity)
            : this(counters, traceActivity, null, null)
        {
        }

        public ProviderInstrumentationScope(
            ProviderInstrumentationCounters counters,
            EventTraceActivity traceActivity,
            Action<EventTraceActivity> traceStarted,
            Action<bool, EventTraceActivity, long> traceComplete)
            : this(counters, traceActivity, traceStarted, traceComplete, null)
        {
        }

        public ProviderInstrumentationScope(
            ProviderInstrumentationCounters counters,
            EventTraceActivity traceActivity,
            Action<EventTraceActivity> traceStarted,
            Action<bool, EventTraceActivity, long> traceComplete,
            string instanceName)
        {
            this.Counters = counters;
            this.TraceActivity = traceActivity;
            this.traceComplete = traceComplete;
            this.InstanceName = instanceName;

            if (traceStarted != null)
            {
                traceStarted(this.TraceActivity);
            }

            this.scopeTimer = new Stopwatch();
            this.scopeTimer.Start();
        }

        public string InstanceName { get; }

        public ProviderInstrumentationCounters Counters
        {
            get;
            private set;
        }

        public bool ServiceError
        {
            get
            {
                return this.processingError || this.connectionError;
            }
        }

        protected EventTraceActivity TraceActivity
        {
            get;
            private set;
        }

        public void ConnectionError()
        {
            this.connectionError = true;
        }

        public void ProcessingError()
        {
            this.processingError = true;
        }

        public void Dispose()
        {
            this.scopeTimer.Stop();

            GC.SuppressFinalize(this);

            long latency = this.scopeTimer.ElapsedMilliseconds;

            if (this.traceComplete != null)
            {
                this.traceComplete(!this.connectionError && !this.processingError, this.TraceActivity, latency);
            }
        }
    }
}
