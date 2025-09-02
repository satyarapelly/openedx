// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace Tests.Common.Model.PX
{
    using Newtonsoft.Json;

    public class PaymentSession : PaymentSessionData
    {
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "isChallengeRequired", Required = Required.Always)]
        public bool IsChallengeRequired { get; set; }

        [JsonProperty(PropertyName = "signature", Required = Required.Always)]
        public string Signature { get; set; }

        [JsonProperty(PropertyName = "challengeStatus", Required = Required.Always)]
        public PaymentChallengeStatus ChallengeStatus { get; set; }

        [JsonProperty(PropertyName = "sessionToken")]
        public string SessionToken { get; set; }

        [JsonProperty(PropertyName = "challengeType")]
        public string ChallengeType { get; set; }
    }
}
