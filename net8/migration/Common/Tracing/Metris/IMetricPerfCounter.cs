// <copyright file="IMetricPerfCounter.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing.Metris
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IMetricPerfCounter<TDimensions>
    {
        DateTime LastValueSetTime { get; }

        double LastTrackedValue { get; }

        void Increment(TDimensions dimensions);

        void SetValue(double value, TDimensions dimensions);
    }
}