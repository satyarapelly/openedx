// <copyright file="ServiceInstrumentationCounters.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing
{
    public class ServiceInstrumentationCounters
    {
        public ServiceInstrumentationCounters(RequestsCounterTechnology technology, string category, string servicePrefix)
        {
            this.ServicePrefix = servicePrefix;
            this.TotalCounter = PerfCounter.CreateCounter(technology, category, servicePrefix + "_Total", PerfCounterType.None);
            this.UserErrorCounter = PerfCounter.CreateCounter(technology, category, servicePrefix + "_UserError", PerfCounterType.None);
            this.SystemErrorCounter = PerfCounter.CreateCounter(technology, category, servicePrefix + "_SystemError", PerfCounterType.None);
            this.SuccessCounter = PerfCounter.CreateCounter(technology, category, servicePrefix + "_Success", PerfCounterType.None);
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

        public PerfCounter UserErrorCounter
        {
            get;
            private set;
        }

        public PerfCounter SystemErrorCounter
        {
            get;
            private set;
        }

        public PerfCounter SuccessCounter
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
