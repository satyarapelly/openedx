// <copyright file="CheckoutStatus.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    public enum CheckoutStatus
    {
        Created,
        Completed,
        Abandoned,
        PendingClientAction,
        HandlePaymentChallenge,
        PendingInitialTransaction
    }
}