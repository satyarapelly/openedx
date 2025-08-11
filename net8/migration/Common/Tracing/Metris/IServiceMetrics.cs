// <copyright file="IServiceMetrics.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Tracing.Metris
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing.Metris.OTel;

    public interface IServiceMetrics
    {
        IncomingRequestMetrics IncomingRequestMetrics { get; }

        OutgoingRequestMetrics OutgoingRequestMetrics { get; }
    }
}