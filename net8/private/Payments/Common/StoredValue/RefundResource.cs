// <copyright file="RefundResource.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.StoredValue
{
    using Newtonsoft.Json;

    /// <summary>
    /// RefundResource is a copy of stored value core resource
    /// </summary>
    public class RefundResource
    {
        [JsonProperty("lot_id")]
        public int LotId { get; set; }

        [JsonProperty("type")]
        public RefundType RefundType { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("tax_amount")]
        public decimal TaxAmount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("external_reference_id")]
        public ExternalReference ExternalReferenceId { get; set; }

        [JsonProperty("original_external_reference_id")]
        public ExternalReference OriginalExternalReferenceId { get; set; }
    }
}