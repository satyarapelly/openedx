// <copyright file="ServiceMetricHistogram.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing.Metris.OTel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Metrics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ServiceMetricHistogram<TDimensions> : IServiceMetricHistogram<TDimensions>
        where TDimensions : class, new()
    {
        private Histogram<double> histogram;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceMetricHistogram{TDimensions}"/> class.
        /// </summary>
        /// <param name="histogram">Histogram created for this metric.</param>
        public ServiceMetricHistogram(Histogram<double> histogram)
        {
            if (histogram == null)
            {
                throw new ArgumentNullException(nameof(histogram));
            }

            this.histogram = histogram;

            // Initialize the metric so it appears in Jarvis portal
            this.Record(0.0, new TDimensions());
        }

        /// <summary>
        /// Gets last tracked value of the metric, used for unit tests.
        /// </summary>
        public double LastTrackedValue { get; private set; }

        /// <inheritdoc/>
        public void Record(double value, TDimensions dimensions)
        {
            try
            {
                var tagList = ServiceMetricHelper<TDimensions>.GetDimensions(dimensions);
                this.histogram.Record(value, tagList);

                this.LastTrackedValue = value;
            }
            catch (Exception)
            {
                // To do after otel log merged

                // Logger.Qos.TraceMessage($"Track service metric counter {this.histogram.Name} failed with error: {ex}", QosEventLevel.Error);
            }
        }
    }
}