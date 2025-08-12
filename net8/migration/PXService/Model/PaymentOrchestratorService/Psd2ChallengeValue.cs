// <copyright file="Psd2ChallengeValue.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model;

    public class Psd2ChallengeValue
    {
        public PaymentChallengeStatus ChallengeStatus { get; set; }

        public string PaymentSessionId { get; set; }
    }
}