// <copyright file="IncomingRequestMetrics.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing.Metris.OTel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Metrics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing.Metris.Dimensions;

    public class IncomingRequestMetrics : IIncomingRequestMetrics
    {
        public const string MetricNamePrefix = "IncomingRequests_";

        /// <summary>
        /// Initializes a new instance of the <see cref="IncomingRequestMetrics"/> class.
        /// </summary>
        /// <param name="meter">The service metrics handler.</param>
        public IncomingRequestMetrics(Meter meter)
        {
            this.IncomingApiRequests = ServiceMetricHelper<RequestDimensions>.CreateCounter(meter, MetricNamePrefix + "ApiRequests");
            this.IncomingApiReliability = ServiceMetricHelper<RequestDimensions>.CreateHistogram(meter, MetricNamePrefix + "ApiReliability");
            this.IncomingApiSuccessLatency = ServiceMetricHelper<RequestDimensions>.CreateHistogram(meter, MetricNamePrefix + "ApiSuccessLatency");
            this.IncomingTotalSystemErrors = ServiceMetricHelper<RequestDimensions>.CreateCounter(meter, MetricNamePrefix + "TotalSystemErrors");
            this.IncomingTotalUserErrors = ServiceMetricHelper<RequestDimensions>.CreateCounter(meter, MetricNamePrefix + "TotalUserErrors");
            this.IncomingTotalSuccesses = ServiceMetricHelper<RequestDimensions>.CreateCounter(meter, MetricNamePrefix + "TotalSuccesses");
            this.IncomingApiReliabilityWithPartner = ServiceMetricHelper<RequestDimensions>.CreateHistogram(meter, MetricNamePrefix + "ApiReliabilityWithPartner");
            this.IncomingApiSuccessLatencyWithPartner = ServiceMetricHelper<RequestDimensions>.CreateHistogram(meter, MetricNamePrefix + "ApiSuccessLatencyWithPartner");
            this.IncomingApiRequestsWithPartner = ServiceMetricHelper<RequestDimensions>.CreateCounter(meter, MetricNamePrefix + "ApiRequestsWithPartner");
            this.IncomingTotalSuccessesWithPartner = ServiceMetricHelper<RequestDimensions>.CreateCounter(meter, MetricNamePrefix + "TotalSuccessesWithPartner");
            this.IncomingTotalUserErrorsWithPartner = ServiceMetricHelper<RequestDimensions>.CreateCounter(meter, MetricNamePrefix + "TotalUserErrorsWithPartner");
            this.IncomingTotalSystemErrorsWithPartner = ServiceMetricHelper<RequestDimensions>.CreateCounter(meter, MetricNamePrefix + "TotalSystemErrorsWithPartner");
        }

        /// <summary>
        /// Gets the metric to report incoming requests with their status result.
        /// </summary>
        public IMetricPerfCounter<RequestDimensions> IncomingApiRequests { get; }

        /// <summary>
        /// Gets the metric to report incoming requests reliability in percentage.
        /// </summary>
        public IServiceMetricHistogram<RequestDimensions> IncomingApiReliability { get; }

        /// <summary>
        /// Gets the histogram of values for incoming request latency.
        /// </summary>
        public IServiceMetricHistogram<RequestDimensions> IncomingApiSuccessLatency { get; }

        /// <summary>
        /// Gets the metric to report incoming total system errors.
        /// </summary>
        public IMetricPerfCounter<RequestDimensions> IncomingTotalSystemErrors { get; }

        /// <summary>
        /// Gets the metric to report incoming total user errors.
        /// </summary>
        public IMetricPerfCounter<RequestDimensions> IncomingTotalUserErrors { get; }

        /// <summary>
        /// Gets the metric to report incoming total successes.
        /// </summary>
        public IMetricPerfCounter<RequestDimensions> IncomingTotalSuccesses { get; }

        /// <summary>
        /// Gets the metric to report incoming requests reliability in percentage with partner.
        /// </summary>
        public IServiceMetricHistogram<RequestDimensions> IncomingApiReliabilityWithPartner { get; }

        /// <summary>
        /// Gets the histogram of values for incoming request latency with partner.
        /// </summary>
        public IServiceMetricHistogram<RequestDimensions> IncomingApiSuccessLatencyWithPartner { get; }

        /// <summary>
        /// Gets the metric to report incoming requests with partner.
        /// </summary>
        public IMetricPerfCounter<RequestDimensions> IncomingApiRequestsWithPartner { get; }

        /// <summary>
        /// Gets the metric to report incoming total successes with partner.
        /// </summary>
        public IMetricPerfCounter<RequestDimensions> IncomingTotalSuccessesWithPartner { get; }

        /// <summary>
        /// Gets the metric to report incoming total user errors with partner.
        /// </summary>
        public IMetricPerfCounter<RequestDimensions> IncomingTotalUserErrorsWithPartner { get; }

        /// <summary>
        /// Gets the metric to report incoming total system errors with partner.
        /// </summary>
        public IMetricPerfCounter<RequestDimensions> IncomingTotalSystemErrorsWithPartner { get; }
    }
}