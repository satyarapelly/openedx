// <copyright file="BillingEventType.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.D365Service
{
    public enum BillingEventType
    {
        Unknown = 0,
        Authorized = 1,
        Captured = 2,
        Refunded = 3,
        Chargedback = 4
    }
}