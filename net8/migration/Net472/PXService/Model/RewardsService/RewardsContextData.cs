// <copyright file="RewardsContextData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.RewardsService
{
    using Newtonsoft.Json;

    public class RewardsContextData
    {
        [JsonProperty(PropertyName = "orderAmount")]
        public decimal? OrderAmount { get; set; }

        [JsonProperty(PropertyName = "currency", Required = Required.Always)]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "catalogSkuAmount")]
        public decimal? CatalogSkuAmount { get; set; }

        [JsonProperty(PropertyName = "catalogSku")]
        public string CatalogSku { get; set; }

        [JsonProperty(PropertyName = "rewardsPoints")]
        public long? RewardsPoints { get; set; }

        [JsonProperty(PropertyName = "isVariableAmountSku")]
        public bool? IsVariableAmountSku { get; set; }
    }
}
