// <copyright file="PointsBalanceDetails.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using Newtonsoft.Json;

    public class PointsBalanceDetails
    {
        [JsonProperty(PropertyName = "rewardsEnabled")]
        public bool RewardsEnabled { get; set; }

        [JsonProperty(PropertyName = "rewardsProgramDetail")]
        public RewardsProgramDetails RewardsProgramDetail { get; set; }

        [JsonProperty(PropertyName = "rewardsSummary")]
        public RewardsSummary RewardsSummary { get; set; }
    }
}
