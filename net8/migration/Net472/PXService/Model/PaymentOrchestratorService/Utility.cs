// <copyright file="Utility.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using System;

    public static class Utility
    {
        public static string ParseEnum<TEnum>(this TEnum enumValue) where TEnum : Enum
        {
            return Enum.GetName(typeof(TEnum), enumValue);
        }
    }
}