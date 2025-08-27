// <copyright file="ChallengeRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.ThreeDSExternalService
{
    using Newtonsoft.Json;

    public class ChallengeRequest
    {
        [JsonProperty(PropertyName = "threeDSServerTransID")]
        public string ThreeDSServerTransID { get; set; }

        [JsonProperty(PropertyName = "acsTransID")]
        public string AcsTransID { get; set; }

        [JsonProperty(PropertyName = "challengeCancel")]
        public string ChallengeCancel { get; set; }

        [JsonProperty(PropertyName = "challengeDataEntry")]
        public string ChallengeDataEntry { get; set; }

        [JsonProperty(PropertyName = "challengeHTMLDataEntry")]
        public string ChallengeHTMLDataEntry { get; set; }

        [JsonProperty(PropertyName = "challengeWindowSize")]
        public ChallengeWindowSize ChallengeWindowSize { get; set; }

        [JsonProperty(PropertyName = "messageExtension")]
        public string MessageExtension { get; set; }

        [JsonProperty(PropertyName = "messageType")]
        public string MessageType { get; set; }

        [JsonProperty(PropertyName = "messageVersion")]
        public string MessageVersion { get; set; }

        [JsonProperty(PropertyName = "oobContinue")]
        public string OObContinue { get; set; }

        [JsonProperty(PropertyName = "resendChallenge")]
        public string ResendChallenge { get; set; }

        [JsonProperty(PropertyName = "sdkTransID")]
        public string SdkTransID { get; set; }

        [JsonProperty(PropertyName = "sdkCounterStoA")]
        public string SdkCounterStoA { get; set; }
    }
}