// <copyright file="PerfCounter.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing
{
    using System;
    using System.Collections.Generic;

    public abstract class PerfCounter
    {
        private static Dictionary<RequestsCounterTechnology, Func<string, string, PerfCounterType, PerfCounter>> perfCounterFactories = new Dictionary<RequestsCounterTechnology, Func<string, string, PerfCounterType, PerfCounter>>();

        public static void RegisterPerfCounterFactory(RequestsCounterTechnology technology, Func<string, string, PerfCounterType, PerfCounter> perfCounterFactory)
        {
            perfCounterFactories.Add(technology, perfCounterFactory);
        }

        public static PerfCounter CreateCounter(RequestsCounterTechnology technology, string category, string name, PerfCounterType perfCounterType)
        {
            Func<string, string, PerfCounterType, PerfCounter> perfCounterFactory;
            if (perfCounterFactories.TryGetValue(technology, out perfCounterFactory))
            {
                return perfCounterFactory(category, name, perfCounterType);
            }

            return new NoautopilotTraceCounter(category, name, perfCounterType);
        }
        
        public class NoautopilotTraceCounter : PerfCounter
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", Justification = "perfCounterType not yet used")]
            public NoautopilotTraceCounter(string category, string name, PerfCounterType perfCounterType)
            {
                this.Category = category;
                this.Name = name;
            }

            public string Category { get; }

            public string Name { get; }
        }
    }
}
