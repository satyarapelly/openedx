// <copyright file="ProviderInstrumentationCounters.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing
{
    public class ProviderInstrumentationCounters
    {
        public ProviderInstrumentationCounters(RequestsCounterTechnology technology, string category, string servicePrefix)
        {
            this.ServicePrefix = servicePrefix;
            this.TotalCounter = PerfCounter.CreateCounter(technology, category, servicePrefix + "_Total", PerfCounterType.None);

            // Provider to TransactionResult interpretion counters
            this.ChallengeResponsePendingCounter = PerfCounter.CreateCounter(technology, category, servicePrefix + "_ChallengeResponsePending", PerfCounterType.None);
            this.UnknownCounter = PerfCounter.CreateCounter(technology, category, servicePrefix + "_Unknown", PerfCounterType.None);
            this.FailedCounter = PerfCounter.CreateCounter(technology, category, servicePrefix + "_Failed", PerfCounterType.None);
            this.DeclinedCounter = PerfCounter.CreateCounter(technology, category, servicePrefix + "_Declined", PerfCounterType.None);
            this.PendingCounter = PerfCounter.CreateCounter(technology, category, servicePrefix + "_Pending", PerfCounterType.None);
            this.ApprovedCounter = PerfCounter.CreateCounter(technology, category, servicePrefix + "_Approved", PerfCounterType.None);
            this.ReversedCounter = PerfCounter.CreateCounter(technology, category, servicePrefix + "_Reversed", PerfCounterType.None);
            this.PartialApprovedCounter = PerfCounter.CreateCounter(technology, category, servicePrefix + "_PartialApproved", PerfCounterType.None);

            // Provider specific counters
            this.ConnectionErrorCounter = PerfCounter.CreateCounter(technology, category, servicePrefix + "_ConnectionError", PerfCounterType.None);
            this.ProcessingErrorCounter = PerfCounter.CreateCounter(technology, category, servicePrefix + "_ProcessingError", PerfCounterType.None);

            this.LatencyCounter = PerfCounter.CreateCounter(technology, category, servicePrefix + "_Latency", PerfCounterType.NumberPercentiles);
        }

        public string ServicePrefix
        {
            get;
            private set;
        }

        public PerfCounter TotalCounter
        {
            get;
            private set;
        }

        public PerfCounter ChallengeResponsePendingCounter
        {
            get;
            private set;
        }

        public PerfCounter UnknownCounter
        {
            get;
            private set;
        }

        public PerfCounter FailedCounter
        {
            get;
            private set;
        }

        public PerfCounter DeclinedCounter
        {
            get;
            private set;
        }

        public PerfCounter PendingCounter
        {
            get;
            private set;
        }

        public PerfCounter ApprovedCounter
        {
            get;
            private set;
        }

        public PerfCounter ReversedCounter
        {
            get;
            private set;
        }

        public PerfCounter PartialApprovedCounter
        {
            get;
            private set;
        }

        public PerfCounter ConnectionErrorCounter
        {
            get;
            private set;
        }

        public PerfCounter ProcessingErrorCounter
        {
            get;
            private set;
        }

        public PerfCounter LatencyCounter
        {
            get;
            private set;
        }
    }
}
