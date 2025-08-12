// <copyright file="ID365ServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.D365Service
{
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Model.D365Service;

    public interface ID365ServiceAccessor
    {
        Task<PagedResponse<Order>> GetOrder(string puid, string orderId, EventTraceActivity traceActivityId);

        Task<PaymentInstrumentCheckResponse> CheckPaymentInstrument(string userId, string piId, EventTraceActivity traceActivityId);

        Task<Cart> GetCartByCartId(string cartId, EventTraceActivity traceActivityId);
    }
}