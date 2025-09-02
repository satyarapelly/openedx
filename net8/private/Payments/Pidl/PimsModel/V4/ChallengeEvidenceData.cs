// <copyright file="ChallengeEvidenceData.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using Newtonsoft.Json;

    public class ChallengeEvidenceData
    {
        [JsonProperty(PropertyName = "challengeType")]
        public string ChallengeType { get; set; }

        [JsonProperty(PropertyName = "challengeId")]
        public string ChallengeId { get; set; }

        [JsonProperty(PropertyName = "challengeResult")]
        public string ChallengeResult { get; set; }

        [JsonProperty(PropertyName = "challengeResultReason")]
        public string ChallengeResultReason { get; set; }
    }
}