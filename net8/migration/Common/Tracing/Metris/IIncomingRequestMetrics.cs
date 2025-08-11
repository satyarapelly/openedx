// <copyright file="IIncomingRequestMetrics.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing.Metris
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing.Metris.Dimensions;

    public interface IIncomingRequestMetrics
    {
        /// <summary>
        /// Gets the metric to report incoming requests with their status result.
        /// </summary>
        IMetricPerfCounter<RequestDimensions> IncomingApiRequests { get; }

        /// <summary>
        /// Gets the metric to report incoming requests reliability in percentage.
        /// </summary>
        IServiceMetricHistogram<RequestDimensions> IncomingApiReliability { get; }

        /// <summary>
        /// Gets the histogram of values for incoming request latency.
        /// </summary>
        IServiceMetricHistogram<RequestDimensions> IncomingApiSuccessLatency { get; }

        /// <summary>
        /// Gets the metric to report total incoming system errors (HTTP 5xx).
        /// </summary>
        IMetricPerfCounter<RequestDimensions> IncomingTotalSystemErrors { get; }

        /// <summary>
        /// Gets the metric to report total incoming user errors (HTTP 4xx).
        /// </summary>
        IMetricPerfCounter<RequestDimensions> IncomingTotalUserErrors { get; }

        /// <summary>
        /// Gets the metric to report total incoming successful requests.
        /// </summary>
        IMetricPerfCounter<RequestDimensions> IncomingTotalSuccesses { get; }

        /// <summary>
        /// Gets the metric to report incoming requests reliability in percentage with partner.
        /// </summary>
        IServiceMetricHistogram<RequestDimensions> IncomingApiReliabilityWithPartner { get; }

        /// <summary>
        /// Gets the histogram of values for incoming request latency with partner.
        /// </summary>
        IServiceMetricHistogram<RequestDimensions> IncomingApiSuccessLatencyWithPartner { get; }

        /// <summary>
        /// Gets the metric to report incoming requests with partner.
        /// </summary>
        IMetricPerfCounter<RequestDimensions> IncomingApiRequestsWithPartner { get; }

        /// <summary>
        /// Gets the metric to report total incoming successful requests with partner.
        /// </summary>
        IMetricPerfCounter<RequestDimensions> IncomingTotalSuccessesWithPartner { get; }

        /// <summary>
        /// Gets the metric to report total incoming user errors with partner.
        /// </summary>
        IMetricPerfCounter<RequestDimensions> IncomingTotalUserErrorsWithPartner { get; }

        /// <summary>
        ///  Gets the metric to report total incoming system errors with partner
        /// </summary>
        IMetricPerfCounter<RequestDimensions> IncomingTotalSystemErrorsWithPartner { get; }
    }
}