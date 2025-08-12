// <copyright file="PaymentRequestClientAction.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    public class PaymentRequestClientAction
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "This is a model coming from the Payment Orchestrator service.")]
        public ClientActionType Type { get; set; }

        public PaymentInstrumentChallengeType ChallengeType { get; set; }

        public PaymentInstrument PaymentInstrument { get; set; }
    }
}