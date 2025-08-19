// <copyright file="PaymentInstrumentCheckResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Tests.Common.Model.PurchaseService
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class PaymentInstrumentCheckResponse
    {
        [JsonProperty("orderIds")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<string> OrderIds { get; set; }

        [JsonProperty("paymentInstrumentInUse")]
        public bool PaymentInstrumentInUse { get; set; }

        [JsonProperty("recurrenceIds")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<string> RecurrenceIds { get; set; }
    }
}