// <copyright file="AccountResource.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.StoredValue
{
    using Newtonsoft.Json;

    /// <summary>
    /// AccountResource contains the general information for stored value account.
    /// </summary>
    public class AccountResource
    {
        [JsonProperty("owner_id")]
        public long OwnerId { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("balance")]
        public decimal Balance { get; set; }
    }
}
