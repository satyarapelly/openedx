// <copyright file="StoredValueChargebackNotificationResource.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.StoredValue
{
    using Newtonsoft.Json;

    /// <summary>
    /// This is used as parameter for sending CSV Chargeback notification to paymod
    /// </summary>
    public class StoredValueChargebackNotificationResource
    {
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("merchantReferenceNumber")]
        public string MerchantReferenceNumber { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }
    }
}
