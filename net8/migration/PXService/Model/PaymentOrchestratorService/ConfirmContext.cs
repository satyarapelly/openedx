// <copyright file="ConfirmContext.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Payload for attaching payment instruments
    /// </summary>
    public class ConfirmContext
    {
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This needs to be set")]
        public IList<PaymentInstrumentContext> PaymentInstruments { get; set; }
    }
}