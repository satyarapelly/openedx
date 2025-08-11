// <copyright file="OtelInitialization.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing.Metris.OTel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using global::OpenTelemetry;
    using global::OpenTelemetry.Exporter.Geneva;
    using global::OpenTelemetry.Metrics;

    public class OtelInitialization
    {
        public static MeterProvider SetupGenevaMetrics(string account = "default")
        {
            return Sdk.CreateMeterProviderBuilder()
                .AddMeter(MetricPerfCounters.MetricNamespace)
                .AddGenevaMetricExporter(options =>
                {
                    options.ConnectionString = $"Account={account};Namespace={MetricPerfCounters.MetricNamespace}";
                })
                .Build();
        }
    }
}
