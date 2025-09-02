// <copyright file="RefundType.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.StoredValue
{
    /// <summary>
    /// RefundType is a copy of stored value core resource
    /// </summary>
    public enum RefundType
    {
        RefundCredit = 1,
     
        RefundDebit = 2,

        ChargebackDebit = 3,
    }
}