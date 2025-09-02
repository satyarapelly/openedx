// <copyright file="ChallengeData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using System.ComponentModel.DataAnnotations;

    public class ChallengeData
    {
        [Required]
        public PaymentInstrumentChallengeType ChallengeType { get; set; }

        [Required]
        public string ChallengeValue { get; set; }
    }
}