// <copyright file="RewardsSummary.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using Newtonsoft.Json;

    public class RewardsSummary
    {
        [JsonProperty(PropertyName = "pointsBalance")]
        public int PointsBalance { get; set; }

        [JsonProperty(PropertyName = "currencyBalance")]
        public double CurrencyBalance { get; set; }

        [JsonProperty(PropertyName = "currencyBalanceText")]
        public string CurrencyBalanceText { get; set; }

        [JsonProperty(PropertyName = "currencyCode")]
        public string CurrencyCode { get; set; }
    }
}
