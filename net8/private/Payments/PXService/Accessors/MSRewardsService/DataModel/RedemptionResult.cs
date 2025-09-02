// <copyright file="RedemptionResult.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>
// Sourced these classes from : https://msasg.visualstudio.com/rewards/_git/M.Rewards_Platform?path=/Models/Models/RequestModels.cs&version=GBcontainers&_a=contents

namespace Microsoft.Commerce.Payments.PXService.Accessors.MSRewardsService.DataModel
{
    using System.Text.Json.Serialization;
    using Newtonsoft.Json;

    public class RedemptionResult
    {
        [JsonProperty("result_message")]
        public string ResultMessage { get; set; }

        [JsonProperty("order")]
        public RedemptionOrder Order { get; set; }

        [JsonProperty("msa_phone_number")]
        public string MsaPhoneNumber { get; set; }

        [JsonProperty("code")]
        public RewardsErrorCode Code { get; set; }
    }
}