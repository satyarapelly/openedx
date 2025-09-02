// <copyright file="ServiceInstrumentationScope.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// The scope used to instrument a service operation. 
    /// It can be used for recording perf counters: total, usererror, systemerror, success, latency.
    /// It can take handler to trace the start and completion stamps without details input/output.
    /// </summary>
    public class ServiceInstrumentationScope : IDisposable
    {
        private Action<bool, EventTraceActivity, long> traceComplete;
        private Stopwatch scopeTimer;
        private bool completedSuccessfully;

        public ServiceInstrumentationScope(ServiceInstrumentationCounters counters, EventTraceActivity traceActivity)
            : this(counters, traceActivity, null, null)
        {
        }

        public ServiceInstrumentationScope(
            ServiceInstrumentationCounters counters,
            EventTraceActivity traceActivity,
            Action<EventTraceActivity> traceStarted,
            Action<bool, EventTraceActivity, long> traceComplete)
            : this(counters, traceActivity, traceStarted, traceComplete, null)
        {
        }

        public ServiceInstrumentationScope(
            ServiceInstrumentationCounters counters,
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

        public ServiceInstrumentationCounters Counters
        {
            get;
            private set;
        }

        protected EventTraceActivity TraceActivity
        {
            get;
            private set;
        }

        public void Success()
        {
            this.completedSuccessfully = true;
        }

        public void UserError()
        {
            this.completedSuccessfully = true;
        }

        public void Dispose()
        {
            this.scopeTimer.Stop();

            GC.SuppressFinalize(this);

            long latency = this.scopeTimer.ElapsedMilliseconds;

            if (this.traceComplete != null)
            {
                this.traceComplete(this.completedSuccessfully, this.TraceActivity, latency);
            }
        }
    }
}
