// <copyright file="MetricPerfCounters.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing.Metris.OTel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class MetricPerfCounters
    {
        public const string MetricNamespace = "MetricPerfCounters";

        private static readonly object countersByTypeSync = new object();
        private static Dictionary<Type, object> countersByType = null;

        /// <summary>
        /// Executes the <paramref name="action"/> with specific counters defined by <typeparamref name="TCounters"/> in a try-catch section
        /// to ensure the failure to report the counters does not impact live traffic.
        /// </summary>
        /// <typeparam name="TCounters">Type of the counters</typeparam>
        /// <param name="action">Method to report the counters.</param>
        public static void SafeReport<TCounters>(Action<TCounters> action)
        {
            try
            {
                EnsureCountersInitialized();

                if (countersByType.TryGetValue(typeof(TCounters), out object objectCounters))
                {
                    if (objectCounters is TCounters)
                    {
                        var counters = (TCounters)objectCounters;
                        action(counters);
                    }
                }
                else
                {
                    throw new NotSupportedException($"The type of metric perf counters is not supported: {typeof(TCounters).ToString()}");
                }
            }
            catch (Exception ex)
            {
                // In case an issue occurred (i.e. the metric perf counter factory method is not initialized)
                TraceCore.TraceException(new InvalidOperationException("Failed to report counters", ex));
            }
        }

        /// <summary>
        /// Gets the counters for specific <typeparamref name="TCounters"/>.
        /// Used in tests only.
        /// </summary>
        /// <typeparam name="TCounters">Type of the counters</typeparam>
        /// <returns>Counters for the specific type</returns>
        public static TCounters GetForTest<TCounters>()
        {
            EnsureCountersInitialized();
            return (TCounters)countersByType[typeof(TCounters)];
        }

        /// <summary>
        /// Sets the counters for specific <typeparamref name="TCounters"/>.
        /// Used in tests only.
        /// </summary>
        /// <typeparam name="TCounters">Type of the counters</typeparam>
        /// <param name="value">Counters for the specific type</param>
        public static void SetForTest<TCounters>(TCounters value)
        {
            EnsureCountersInitialized();
            countersByType[typeof(TCounters)] = value;
        }

        /// <summary>
        /// Returns the number of registered types of counters.
        /// Used in tests only.
        /// </summary>
        /// <returns>Number of registered types of counters.</returns>
        public static int GetNumberOfCounterTypes()
        {
            EnsureCountersInitialized();
            return countersByType.Count;
        }

        private static void EnsureCountersInitialized()
        {
            if (countersByType != null)
            {
                return; // initialized already
            }

            lock (countersByTypeSync)
            {
                if (countersByType != null)
                {
                    return; // initialized already
                }

                ServiceMetrics serviceMetrics = new ServiceMetrics(MetricNamespace);

                countersByType = new Dictionary<Type, object>
                {
                    { typeof(IIncomingRequestMetrics), serviceMetrics.IncomingRequestMetrics },
                    { typeof(IOutgoingRequestMetrics), serviceMetrics.OutgoingRequestMetrics },
                };
            }
        }
    }
}