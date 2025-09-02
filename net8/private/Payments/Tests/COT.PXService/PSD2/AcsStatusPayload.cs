// <copyright file="AcsStatusPayload.cs" company="Microsoft">Copyright (c) Microsoft 2019 - 2020. All rights reserved.</copyright>

namespace COT.PXService.PSD2
{
    using Newtonsoft.Json;

    public class AcsStatusPayload
    {
        [JsonProperty("threeDSServerTransID")]
        public string ThreeDSServerTransID { get; set; }

        [JsonProperty("challengeCancel", NullValueHandling = NullValueHandling.Ignore)]
        public string ChallengeCancel { get; set; }

        [JsonProperty("transStatus", NullValueHandling = NullValueHandling.Ignore)]
        public string TransStatus { get; set; }

        [JsonProperty("transStatusReason", NullValueHandling = NullValueHandling.Ignore)]
        public string TransStatusReason { get; set; }
    }
}
