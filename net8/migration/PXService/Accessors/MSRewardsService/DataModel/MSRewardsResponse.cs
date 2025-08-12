// <copyright file="MSRewardsResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>
// sourced these classes from https://msasg.visualstudio.com/rewards/_git/M.Rewards_Platform?path=/Models/Models/GetUserInfoResponse.cs&_a=contents&version=GBcontainers

namespace Microsoft.Commerce.Payments.PXService.Accessors.MSRewardsService.DataModel
{
    using System.Text.Json.Serialization;
    using Newtonsoft.Json;

    public class MSRewardsResponse<T>
    {
        [JsonProperty("response")]
        public T Response { get; set; }

        [JsonProperty("correlationId")]
        public string CorrelationId { get; set; }

        [JsonProperty("code")]
        public RewardsErrorCode Code { get; set; }
    }
}