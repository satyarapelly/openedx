// <copyright file="IOutgoingRequestMetrics.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing.Metris
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing.Metris.Dimensions;

    public interface IOutgoingRequestMetrics
    {
        IMetricPerfCounter<OutgoingRequestDimensions> ExceptionsBeforeResponse { get; }

        IMetricPerfCounter<OutgoingRequestDimensions> UnexpectedExceptionDuringResponse { get; }

        IMetricPerfCounter<OutgoingRequestDimensions> TimeoutExceptionDuringResponse { get; }

        IMetricPerfCounter<OutgoingRequestDimensions> ConnectionClosedExceptionDuringResponse { get; }

        IMetricPerfCounter<OutgoingRequestDimensions> ExceptionsAfterSuccessfulResponse { get; }

        IMetricPerfCounter<OutgoingRequestStatusCodeDimensions> ResponseStatusCode { get; }

        IMetricPerfCounter<OutgoingRequestStatusCodeDimensions> OutgoingRequests { get; }

        IServiceMetricHistogram<OutgoingRequestStatusCodeDimensions> OutgoingApiReliability { get; }

        IServiceMetricHistogram<OutgoingRequestStatusCodeDimensions> OutgoingApiSuccessLatency { get; }
    }
}
