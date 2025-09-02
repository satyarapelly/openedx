// <copyright file="AttachChallengeDataContext.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using System.ComponentModel.DataAnnotations;

    public class AttachChallengeDataContext
    {
        [Required]
        public string PaymentInstrumentId { get; set; }

        [Required]
        public ChallengeData ChallengeData { get; set; }
    }
}