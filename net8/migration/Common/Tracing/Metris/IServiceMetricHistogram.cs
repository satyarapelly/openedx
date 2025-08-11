// <copyright file="IServiceMetricHistogram.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing.Metris
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IServiceMetricHistogram<TDimensions>
    {
        /// <summary>
        /// Gets last tracked value of the metric, used for unit tests.
        /// </summary>
        double LastTrackedValue { get; }

        /// <summary>
        /// Records a statisitically meaningful value to compare across a multitude of values i.e. request latency.
        /// Use this method to report a wide range of values (i.e. latency).
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <param name="dimensions">Dimensions for the metric.</param>
        void Record(double value, TDimensions dimensions);
    }
}