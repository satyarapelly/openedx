// <copyright file="Iso4217AmountConverter.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using Microsoft.Commerce.Payments.Common.Helper;

    /// <summary>
    /// Generates a fixed point representation of a currency amount based on ISO 4217 rules https://en.wikipedia.org/wiki/ISO_4217
    /// For example, to charge 10 USD, provide an amount value of 1000 (that is, 1000 cents).
    /// For zero-decimal currencies, still provide amounts as an integer but without multiplying by 100. For example, to charge ¥500, provide an amount value of 500.
    /// </summary>
    public static class Iso4217AmountConverter
    {
        public static decimal ConvertFromIso4217Amount(string currency, long iso4217Amount)
        {
            uint exponent = CurrencyHelper.GetCurrencyExponent(currency);

            return (decimal)iso4217Amount / (int)Math.Pow(10, exponent);
        }
    }
}