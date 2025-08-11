// <copyright file="OutgoingRequestMetrics.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing.Metris.OTel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Metrics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing.Metris.Dimensions;

    public class OutgoingRequestMetrics : IOutgoingRequestMetrics
    {
        public const string MetricNamePrefix = "OutgoingRequests_";

        /// <summary>
        /// Initializes a new instance of the <see cref="OutgoingRequestMetrics"/> class.
        /// </summary>
        /// <param name="meter">The service metrics handler.</param>
        public OutgoingRequestMetrics(Meter meter)
        {
            this.ExceptionsBeforeResponse = ServiceMetricHelper<OutgoingRequestDimensions>.CreateCounter(meter, MetricNamePrefix + "ExceptionsBeforeResponse");
            this.UnexpectedExceptionDuringResponse = ServiceMetricHelper<OutgoingRequestDimensions>.CreateCounter(meter, MetricNamePrefix + "UnexpectedExceptionDuringResponse");
            this.TimeoutExceptionDuringResponse = ServiceMetricHelper<OutgoingRequestDimensions>.CreateCounter(meter, MetricNamePrefix + "TimeoutExceptionDuringResponse");
            this.ConnectionClosedExceptionDuringResponse = ServiceMetricHelper<OutgoingRequestDimensions>.CreateCounter(meter, MetricNamePrefix + "ConnectionClosedExceptionDuringResponse");
            this.ExceptionsAfterSuccessfulResponse = ServiceMetricHelper<OutgoingRequestDimensions>.CreateCounter(meter, MetricNamePrefix + "ExceptionsAfterSuccessfulResponse");
            this.ResponseStatusCode = ServiceMetricHelper<OutgoingRequestStatusCodeDimensions>.CreateCounter(meter, MetricNamePrefix + "ResponseStatusCode");
            this.OutgoingRequests = ServiceMetricHelper<OutgoingRequestStatusCodeDimensions>.CreateCounter(meter, MetricNamePrefix + "OutgoingRequests");
            this.OutgoingApiReliability = ServiceMetricHelper<OutgoingRequestStatusCodeDimensions>.CreateHistogram(meter, MetricNamePrefix + "ApiReliability");
            this.OutgoingApiSuccessLatency = ServiceMetricHelper<OutgoingRequestStatusCodeDimensions>.CreateHistogram(meter, MetricNamePrefix + "ApiSuccessLatency");
        }

        /// <summary>
        /// Gets the metric to report incoming requests with their status result.
        /// </summary>
        public IMetricPerfCounter<OutgoingRequestStatusCodeDimensions> OutgoingRequests { get; }

        /// <summary>
        /// Gets the metric to report incoming requests reliability in percentage.
        /// </summary>
        public IServiceMetricHistogram<OutgoingRequestStatusCodeDimensions> OutgoingApiReliability { get; }

        /// <summary>
        /// Gets the histogram of values for incoming request latency.
        /// </summary>
        public IServiceMetricHistogram<OutgoingRequestStatusCodeDimensions> OutgoingApiSuccessLatency { get; }

        /// <summary>
        /// Gets the exceptions thrown before the outgoing request was sent.
        /// </summary>
        public IMetricPerfCounter<OutgoingRequestDimensions> ExceptionsBeforeResponse { get; }

        /// <summary>
        /// Gets the unexpected exceptions thrown while the outgoing request was sent.
        /// Unexpected exception is an exception other than all known exceptions.
        /// </summary>
        public IMetricPerfCounter<OutgoingRequestDimensions> UnexpectedExceptionDuringResponse { get; }

        /// <summary>
        /// Gets the Timeout exceptions thrown while the outgoing request was sent.
        /// While it is expected to have them in small numbers, we need to alert on an excessive number of timeouts.
        /// </summary>
        public IMetricPerfCounter<OutgoingRequestDimensions> TimeoutExceptionDuringResponse { get; }

        /// <summary>
        /// Gets the underlying connection was closed while the outgoing request was sent.
        /// While it is expected to have them in small numbers, we need to alert on an excessive number of closed connections.
        /// </summary>
        public IMetricPerfCounter<OutgoingRequestDimensions> ConnectionClosedExceptionDuringResponse { get; }

        /// <summary>
        /// Gets Exceptions thrown after a successful response.
        /// While it is expected to have them in small numbers (i.e. a provider can respond with 200 but with the status = Declined),
        /// we need to alert on an excessive number of exceptions.
        /// </summary>
        public IMetricPerfCounter<OutgoingRequestDimensions> ExceptionsAfterSuccessfulResponse { get; }

        /// <summary>
        /// Gets Response status code if the response could be obtained.
        /// We should alert on specific codes indicating an issue (i.e. 401 meaning we cannot authenticate).
        /// </summary>
        public IMetricPerfCounter<OutgoingRequestStatusCodeDimensions> ResponseStatusCode { get; }
    }
}
