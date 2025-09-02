// <copyright file="RewardsProgramDetails.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using Newtonsoft.Json;

    public class RewardsProgramDetails
    {
        [JsonProperty(PropertyName = "conversionRate")]
        public double ConversionRate { get; set; }

        [JsonProperty(PropertyName = "minRedeemPoints")]
        public int MinRedeemPoints { get; set; }

        [JsonProperty(PropertyName = "maxRedeemPoints")]
        public int MaxRedeemPoints { get; set; }
    }
}
