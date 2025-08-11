// <copyright file="ServiceMetricHelper.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing.Metris.OTel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Metrics;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing.Metris.Dimensions;

    public static class ServiceMetricHelper<TDimensions>
        where TDimensions : class, new()
    {
        private const string DefaultDimensionValue = "-";
        private static readonly List<KeyValuePair<string, string>> staticDimensions = new List<KeyValuePair<string, string>>
            {
                DefaultDimensions.RegionName,
                DefaultDimensions.SiteName,
                DefaultDimensions.SlotName,
                DefaultDimensions.WebJobName,
                DefaultDimensions.MachineName,
                DefaultDimensions.BuildVersion
            };

        public static IMetricPerfCounter<TDimensions> CreateCounter(Meter meter, string metricName)
        {
            ThrowIf.Argument.IsNull(meter, nameof(meter));
            ThrowIf.Argument.IsNullOrWhiteSpace(metricName, nameof(metricName));

            var counter = meter.CreateCounter<double>(metricName);
            return new ServiceMetricCounter<TDimensions>(counter);
        }

        public static IServiceMetricHistogram<TDimensions> CreateHistogram(Meter meter, string metricName)
        {
            ThrowIf.Argument.IsNull(meter, nameof(meter));
            ThrowIf.Argument.IsNullOrWhiteSpace(metricName, nameof(metricName));

            var histogram = meter.CreateHistogram<double>(metricName);
            return new ServiceMetricHistogram<TDimensions>(histogram);
        }

        /// <summary>
        /// Returns a list of dimension from the instance of the metric dimensions.
        /// </summary>
        /// <param name="instance">An instance of metric dimensions</param>
        /// <returns>List of dimensions</returns>
        public static TagList GetDimensions(TDimensions instance)
        {
            ThrowIf.Argument.IsNull(instance, nameof(instance));

            List<string> dimensionValues = GetDimensionValues(instance);
            List<string> dimensionNames = GetDimensionNames();

            TagList tagList = new TagList();
            for (int i = 0; i < dimensionValues.Count; i++)
            {
                tagList.Add(dimensionNames[i], EnsureNotEmpty(dimensionValues[i] ?? string.Empty));
            }

            return tagList;
        }

        /// <summary>
        /// Returns a list of dimension values from the instance of the metric dimensions.
        /// </summary>
        /// <param name="instance">Instance of the metric dimensions.</param>
        /// <returns>A list of dimension values.</returns>
        private static List<string> GetDimensionValues(TDimensions instance)
        {
            ThrowIf.Argument.IsNull(instance, nameof(instance));

            // The order of the values of the dimensions should correspond to the dimension names returned in GetDimensionNames()
            var result = typeof(TDimensions).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => Convert.ToString(p.GetValue(instance))).ToList();

            // Insert static dimension values
            result.InsertRange(0, staticDimensions.Select(i => i.Value));
            return result;
        }

        /// <summary>
        /// Return dimension name.
        /// </summary>
        /// <returns>A dimension name string. </returns>
        private static List<string> GetDimensionNames()
        {
            // The order of the names of the dimensions should correspond to the dimension values returned in GetDimensionValues()
            var result = typeof(TDimensions).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.Name).ToList();

            // Insert static dimension names
            result.InsertRange(0, staticDimensions.Select(i => i.Key));
            return result;
        }

        private static string EnsureNotEmpty(string value)
        {
            // Geneva does not support the dimension value to be null. Setting it to default value.
            if (string.IsNullOrWhiteSpace(value))
            {
                return DefaultDimensionValue;
            }
            else
            {
                return value;
            }
        }
    }
}