// <copyright file="PaymentMethod.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    public class PaymentMethod
    {
        public PaymentMethodType PaymentMethodType { get; set; }

        public DisplayDetails DisplayDetails { get; set; }
    }
}