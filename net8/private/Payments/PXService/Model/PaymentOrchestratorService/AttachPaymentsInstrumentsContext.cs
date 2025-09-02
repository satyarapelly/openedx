// <copyright file="AttachPaymentsInstrumentsContext.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{    
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    /// <summary>
    /// Payload for attaching payment instruments
    /// </summary>
    public class AttachPaymentsInstrumentsContext
    {
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This needs to be set")]
        [JsonProperty(PropertyName = "paymentInstruments")]
        public List<PaymentInstrumentContext> PaymentInstruments { get; set; }
    }
}