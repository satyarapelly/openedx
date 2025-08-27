// <copyright file="PaymentMethodResult.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using System.Collections.Generic;

    public class PaymentMethodResult
    {
        public IList<PimsModel.V4.PaymentMethod> PaymentMethods { get; } = new List<PimsModel.V4.PaymentMethod>();

        public IList<PimsModel.V4.PaymentInstrument> PaymentInstruments { get; } = new List<PimsModel.V4.PaymentInstrument>();

        public PimsModel.V4.PaymentInstrument DefaultPaymentInstrument { get; set; }
    }
}