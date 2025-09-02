// <copyright file="StoredValueLotDetails.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using System;
    using Newtonsoft.Json;

    public class StoredValueLotDetails
    {
        [JsonProperty(PropertyName = "currentBalance")]
        public decimal CurrentBalance { get; set; }

        [JsonProperty(PropertyName = "originalBalance")]
        public decimal OriginalBalance { get; set; }

        [JsonProperty(PropertyName = "pendingBalance")]
        public decimal? PendingBalance { get; set; }

        [JsonProperty(PropertyName = "expirationDate")]
        public DateTime? ExpirationDate { get; set; }

        [JsonProperty(PropertyName = "fundOrderId")]
        public long? FundOrderId { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string LotType { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "tokenInstanceId")]
        public string TokenInstanceId { get; set; }

        [JsonProperty(PropertyName = "lastUpdatedTime")]
        public DateTime LastUpdatedTime { get; set; }
    }
}