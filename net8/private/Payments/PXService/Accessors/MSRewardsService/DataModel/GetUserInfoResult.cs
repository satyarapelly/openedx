// <copyright file="GetUserInfoResult.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>
// sourced these classes from https://msasg.visualstudio.com/rewards/_git/M.Rewards_Platform?path=/Models/Models/GetUserInfoResponse.cs&_a=contents&version=GBcontainers

namespace Microsoft.Commerce.Payments.PXService.Accessors.MSRewardsService.DataModel
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using Newtonsoft.Json;

    public class GetUserInfoResult
    {
        [JsonProperty("balance")]
        public long Balance { get; set; }

        [JsonProperty("catalog")]
        public IEnumerable<UserFacingCatalogItem> CatalogItems { get; set; }
    }
}