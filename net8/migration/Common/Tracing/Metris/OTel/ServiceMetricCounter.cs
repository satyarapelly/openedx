// <copyright file="ServiceMetricCounter.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing.Metris.OTel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Metrics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ServiceMetricCounter<TDimensions> : IMetricPerfCounter<TDimensions>
        where TDimensions : class, new()
    {
        private Counter<double> counter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceMetricCounter{TDimensions}"/> class.
        /// </summary>
        /// <param name="counter">Counter created for this metric.</param>
        public ServiceMetricCounter(Counter<double> counter)
        {
            if (counter == null)
            {
                throw new ArgumentNullException(nameof(counter));
            }

            this.counter = counter;

            // Initialize the metric so it appears in Jarvis portal
            this.TrackValue(0.0, new TDimensions());
        }

        /// <summary>
        /// Gets last tracked value of the metric, used for unit tests.
        /// </summary>
        public double LastTrackedValue { get; private set; }

        /// <summary>
        /// Gets last time the value was set.
        /// </summary>
        public DateTime LastValueSetTime { get; private set; }

        public void Increment(TDimensions dimensions)
        {
            this.TrackValue(1.0, dimensions);
        }

        public void SetValue(double value, TDimensions dimensions)
        {
            this.TrackValue(value, dimensions);
        }

        private void TrackValue(double value, TDimensions dimensions)
        {
            try
            {
                var tagList = ServiceMetricHelper<TDimensions>.GetDimensions(dimensions);

                this.counter.Add(value, tagList);

                this.LastTrackedValue = value;

                this.LastValueSetTime = DateTime.UtcNow;
            }
            catch (Exception)
            {
                // To do after OTelLOg Impemented

                // Logger.Qos.TraceMessage($"MetricException: {ex}", QosEventLevel.Error);
            }
        }
    }
}