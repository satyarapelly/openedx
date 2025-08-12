// <copyright file="Psd2ChallengeData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using System.ComponentModel.DataAnnotations;

    public class Psd2ChallengeData
    {
        [Required]
        public PaymentInstrumentChallengeType ChallengeType { get; set; }

        [Required]
        public Psd2ChallengeValue ChallengeValue { get; set; }
    }
}