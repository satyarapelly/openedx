// <copyright file="WalletEligiblePaymentMethods.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using System.Collections.Generic;

    public class WalletEligiblePaymentMethods
    {
        public List<PaymentMethodResource> PaymentMethods { get; } = new List<PaymentMethodResource>();

        public List<PaymentInstrumentResource> PaymentInstruments { get; } = new List<PaymentInstrumentResource>();
    }
}