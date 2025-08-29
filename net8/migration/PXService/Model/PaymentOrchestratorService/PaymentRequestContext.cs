// <copyright file="PaymentRequestContext.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    public class PaymentRequestContext
    {
        public string Country { get; set; }

        public string Currency { get; set; }

        public string Language { get; set; }

        public decimal Amount { get; set; }

        public MerchantAccountProfile MerchantAccountProfile { get; set; }

        public PaymentCapabilities Capabilities { get; set; }
    }
}