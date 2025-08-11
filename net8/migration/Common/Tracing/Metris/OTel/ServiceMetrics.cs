// <copyright file="ServiceMetrics.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing.Metris.OTel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Metrics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ServiceMetrics : IServiceMetrics
    {
        private readonly Meter meter;

        public ServiceMetrics(string metricsNameSpace)
        {
            this.meter = new Meter(metricsNameSpace);

            IncomingRequestMetrics = new IncomingRequestMetrics(this.meter);

            OutgoingRequestMetrics = new OutgoingRequestMetrics(this.meter);
        }

        public IncomingRequestMetrics IncomingRequestMetrics { get; private set; }

        public OutgoingRequestMetrics OutgoingRequestMetrics { get; private set; }
    }
}