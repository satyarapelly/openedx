// <copyright file="UserFacingCatalogItem.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>
// sourced these classes from https://msasg.visualstudio.com/rewards/_git/M.Rewards_Platform?path=/Models/Models/GetUserInfoResponse.cs&_a=contents&version=GBcontainers

namespace Microsoft.Commerce.Payments.PXService.Accessors.MSRewardsService.DataModel
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.Json.Serialization;
    using Newtonsoft.Json;

    public class UserFacingCatalogItem
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("price")]
        public long Price { get; set; }

        [JsonProperty("attributes")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public IDictionary<string, string> Attributes { get; set; }

        [JsonProperty("config")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public IDictionary<string, string> Configuration { get; set; }

        [JsonProperty("disabled", NullValueHandling = NullValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Disabled { get; set; }
    }
}