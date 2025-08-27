// <copyright file="AttachPSD2ChallengeDataContext.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using System.ComponentModel.DataAnnotations;

    public class AttachPSD2ChallengeDataContext
    {
        [Required]
        public string PaymentInstrumentId { get; set; }

        [Required]
        public Psd2ChallengeData ChallengeData { get; set; }
    }
}