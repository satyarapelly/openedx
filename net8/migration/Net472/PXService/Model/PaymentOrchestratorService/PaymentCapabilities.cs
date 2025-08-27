// <copyright file="PaymentCapabilities.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    public class PaymentCapabilities
    {
        public bool? ComputeTax { get; set; } = false;

        public bool? SendEmail { get; set; } = false;

        public bool? CollectBillingAddress { get; set; } = false;
    }
}