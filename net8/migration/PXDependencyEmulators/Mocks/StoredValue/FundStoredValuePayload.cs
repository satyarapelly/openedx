// <copyright file="FundStoredValuePayload.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class FundStoredValuePayload
    {
        [JsonProperty("payment_callback_url")]
        public string Success { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("puid")]
        public string IdentityValue { get; set; }

        [JsonProperty("payment_instrument_id")]
        public string PaymentInstrumentId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed for serialization purpose.")]
        [JsonProperty("risk_properties")]
        public IList<RESTProperty> RiskProperties { get; set; }
    }
}
