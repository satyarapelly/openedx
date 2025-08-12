// <copyright file="ForceTriggerSmsChallengeDetails.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>
// Sourced these classes from : https://msasg.visualstudio.com/rewards/_git/M.Rewards_Platform?path=/Models/Models/RequestModels.cs&version=GBcontainers&_a=contents

namespace Microsoft.Commerce.Payments.PXService.Accessors.MSRewardsService.DataModel
{
    using System.Text.Json.Serialization;
    using Newtonsoft.Json;

    public class ForceTriggerSmsChallengeDetails
    {
        [JsonProperty("msa_invalid")]
        public bool IsInvalidMsaNumber { get; set; }

        [JsonProperty("incl_sms_chg_sku")]
        public bool IncludeSkuForForceSmsChallenge { get; set; }
    }
}