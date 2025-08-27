// <copyright file="TransactionStatus.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentTransaction.Model
{
    public enum TransactionStatus
    {
        Authorized,
        Settled,
        Refunded
    }
}