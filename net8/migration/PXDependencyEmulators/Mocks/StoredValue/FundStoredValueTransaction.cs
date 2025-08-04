// <copyright file="FundStoredValueTransaction.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks
{
    using Newtonsoft.Json;

    public class FundStoredValueTransaction
    {
        [JsonProperty("redirect_content")]
        public string RedirectionUrl { get; set; }

        [JsonProperty("type")]
        public string TransactionType { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

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

        [JsonProperty("payment_transaction_id")]
        public string PaymentTransactionId { get; set; }

        [JsonProperty("Version")]
        public string Version { get; set; }

        [JsonProperty("payment_callback_url")]
        public string PaymentCallbackUrl { get; set; }

        [JsonProperty("payment_instrument_id")]
        public string PaymentInstrumentId { get; set; }
    }
}
