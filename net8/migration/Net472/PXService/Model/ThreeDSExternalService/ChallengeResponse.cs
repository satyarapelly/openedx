// <copyright file="ChallengeResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.ThreeDSExternalService
{
    using Newtonsoft.Json;

    public class ChallengeResponse
    {
        [JsonProperty(PropertyName = "threeDSServerTransID")]
        public string ThreeDSServerTransID { get; set; }

        [JsonProperty(PropertyName = "acsCounterAtoS")]
        public string AcsCounterAtoS { get; set; }

        [JsonProperty(PropertyName = "acsTransID")]
        public string AcsTransID { get; set; }

        [JsonProperty(PropertyName = "challengeCompletionInd")]
        public string ChallengeCompletionInd { get; set; }

        [JsonProperty(PropertyName = "messageExtension")]
        public string MessageExtension { get; set; }

        [JsonProperty(PropertyName = "messageType")]
        public string MessageType { get; set; }

        [JsonProperty(PropertyName = "messageVersion")]
        public string MessageVersion { get; set; }

        [JsonProperty(PropertyName = "sdkTransID")]
        public string SdkTransID { get; set; }

        [JsonProperty(PropertyName = "transStatus")]
        public string TransStatus { get; set; }
    }
}