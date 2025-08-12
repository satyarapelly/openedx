// <copyright file="MSRewardsRedeemRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.RewardsService
{
    using Newtonsoft.Json;

    public class MSRewardsRedeemRequest
    {
        [JsonProperty("catalogItem")]
        public string CatalogItem { get; set; }

        [JsonProperty("catalogItemAmount")]
        public decimal? CatalogItemAmount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("challengePreference")]
        public string ChallengePreference { get; set; }

        [JsonProperty("greenId")]
        public string GreenId { get; set; }

        [JsonProperty("solveCode")]
        public string SolveCode { get; set; }

        [JsonProperty("challengeToken")]
        public string ChallengeToken { get; set; }

        [JsonProperty("deviceType")]
        public string DeviceType { get; set; }

        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }
    }
}
