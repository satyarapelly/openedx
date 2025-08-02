// <copyright file="ICTPCommerceDataAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.Messages;

    public interface ICTPCommereceDataAccessor
    {
        GetSubscriptionsResponse GetSubscriptions(GetSubscriptionsRequest request, EventTraceActivity traceActivityId);
    }
}