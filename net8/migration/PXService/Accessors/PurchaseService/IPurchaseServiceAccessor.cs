// <copyright file="IPurchaseServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Model.PurchaseService;

    public interface IPurchaseServiceAccessor
    {
        Task<Orders> ListOrders(
            string userId,
            int maxPageSize,
            DateTime startTime,
            DateTime? endTime,
            List<string> validOrderStates,
            EventTraceActivity traceActivityId);

        Task<Orders> ListOrders(
            string userId,
            string nextLink,
            EventTraceActivity traceActivityId,
            List<string> exposedFlightFeatures);

        Task<Subscriptions> ListSubscriptions(string userId, int maxPageSize, EventTraceActivity traceActivityId);

        Task<Subscription> GetSubscription(string userId, string subscriptionId, EventTraceActivity traceActivityId);

        Task<Order> GetOrder(string puid, string orderId, EventTraceActivity traceActivityId);

        Task<PaymentInstrumentCheckResponse> CheckPaymentInstrument(string userId, string piId, EventTraceActivity traceActivityId);

        Task<Order> RedeemCSVToken(string puid, string csvToken, string market, string language, EventTraceActivity traceActivityId);
    }
}
