// <copyright file="AuthenticationStatus.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model
{
    using System;
    using Newtonsoft.Json;

    public class AuthenticationStatus
    {
        [JsonProperty(PropertyName = "piId")]
        public string PiId { get; set; }

        [JsonProperty(PropertyName = "sessionId")]
        public string SessionId { get; set; }

        [JsonProperty(PropertyName = "verified")]
        public bool Verified { get; set; }

        [JsonProperty(PropertyName = "challengeStatus")]
        public PaymentChallengeStatus? ChallengeStatus { get; set; }

        [JsonProperty(PropertyName = "failureReason")]
        public VerificationResult? FailureReason { get; set; }
    }
}