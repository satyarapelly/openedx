// <copyright file="RiskOrderContext.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>
// Sourced these classes from : https://msasg.visualstudio.com/rewards/_git/M.Rewards_Platform?path=/Models/Models/RequestModels.cs&version=GBcontainers&_a=contents

namespace Microsoft.Commerce.Payments.PXService.Accessors.MSRewardsService.DataModel
{
    using System.Text.Json.Serialization;
    using Newtonsoft.Json;

    public class RiskOrderContext
    {
        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        [JsonProperty("challenge_preference")]
        public RiskVerificationType ChallengePreference { get; set; }

        [JsonProperty("challenge_message_template")]
        public string ChallengeMessageTemplate { get; set; }

        [JsonProperty("green_id")]
        public string GreenId { get; set; }

        [JsonProperty("solve_code")]
        public string SolveCode { get; set; }

        [JsonProperty("challenge_token")]
        public string ChallengeToken { get; set; }

        [JsonProperty("ip_address")]
        public string IpAddress { get; set; }

        [JsonProperty("ui_language")]
        public string UiLanguage { get; set; }

        [JsonProperty("user_agent")]
        public string UserAgent { get; set; }

        [JsonProperty("device_type")]
        public string DeviceType { get; set; }

        [JsonProperty("device_id")]
        public string DeviceId { get; set; }
    }
}