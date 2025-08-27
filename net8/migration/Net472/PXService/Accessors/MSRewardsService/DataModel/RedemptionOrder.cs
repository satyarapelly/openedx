// <copyright file="RedemptionOrder.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>
// Sourced these classes from : https://msasg.visualstudio.com/rewards/_git/M.Rewards_Platform?path=/Models/Models/RequestModels.cs&version=GBcontainers&_a=contents

namespace Microsoft.Commerce.Payments.PXService.Accessors.MSRewardsService.DataModel
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class RedemptionOrder
    {
        [JsonProperty("id")]
        public string OrderId { get; set; }

        [JsonProperty("sku")]
        public string OrderSKU { get; set; }

        [JsonProperty("item_snapshot")]
        public UserFacingCatalogItem ItemSnapshot { get; set; }

        [JsonProperty(PropertyName = "a")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public IDictionary<string, string> Attributes { get; set; }

        [JsonProperty(PropertyName = "p")]
        public long Price { get; set; }
    }
}