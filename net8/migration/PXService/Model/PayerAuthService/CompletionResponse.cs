// <copyright file="CompletionResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PayerAuthService
{
    using Newtonsoft.Json;

    /// <summary>
    /// This is a model per the PayerAuth.V3 API.
    /// This object is sent as payload to PayerAuth.V3's POST /completeChallenge API
    /// </summary>
    public class CompletionResponse 
    {
        [JsonProperty(PropertyName = "transaction_challenge_status")]
        public TransactionStatus TransactionStatus { get; set; }

        [JsonProperty(PropertyName = "transaction_challenge_status_reason")]
        public TransactionStatusReason TransactionStatusReason { get; set; }

        [JsonProperty(PropertyName = "challenge_completion_indicator")]
        public ChallengeCompletionIndicator ChallengeCompletionIndicator { get; set; }

        [JsonProperty(PropertyName = "transaction_challenge_cancel_indicator")]
        public string ChallengeCancelIndicator { get; set; }
    }
}