// <copyright file="MockCheckPiResult.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;
    using global::Tests.Common.Model.PurchaseService;

    public class MockCheckPiResult
    {
        [JsonProperty("paymentInstrumentId")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public string PaymentInstrumentId { get; set; }

        [JsonProperty("orderIds")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<string> OrderIds { get; set; }

        [JsonProperty("paymentInstrumentInUse")]
        public bool PaymentInstrumentInUse { get; set; }

        [JsonProperty("recurrenceIds")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<string> RecurrenceIds { get; set; }

        public PaymentInstrumentCheckResponse GetPaymentInstrumentCheckResponse()
        {
            return new PaymentInstrumentCheckResponse
            {
                OrderIds = this.OrderIds,
                PaymentInstrumentInUse = this.PaymentInstrumentInUse,
                RecurrenceIds = this.RecurrenceIds
            };
        }
    }
}
