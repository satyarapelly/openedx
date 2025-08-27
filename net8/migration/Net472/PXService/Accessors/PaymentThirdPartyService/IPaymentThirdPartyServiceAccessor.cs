// <copyright file="IPaymentThirdPartyServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System.Threading.Tasks;
    using Tracing;
    using ThirdPartyService = Model.PaymentThirdPartyService;

    public interface IPaymentThirdPartyServiceAccessor
    {
        Task<ThirdPartyService.Checkout> GetCheckout(string paymentProviderId, string checkoutId, EventTraceActivity traceActivityId);

        Task<ThirdPartyService.PaymentRequest> GetPaymentRequest(string paymentProviderId, string paymentRequestId, EventTraceActivity traceActivityId);

        Task<ThirdPartyService.Checkout> Charge(string paymentProviderId, ThirdPartyService.CheckoutCharge checkoutCharge, EventTraceActivity traceActivityId);
    }
}