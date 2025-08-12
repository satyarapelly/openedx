// <copyright file="EligiblePaymentMethods.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using System.Collections.Generic;

    public class EligiblePaymentMethods
    {
        public IList<PaymentMethod> PaymentMethods { get; } = new List<PaymentMethod>();

        public IList<PaymentInstrument> PaymentInstruments { get; } = new List<PaymentInstrument>();
    }
}