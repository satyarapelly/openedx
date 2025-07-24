// <copyright file="CurrencyHelper.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    using Microsoft.Osgs.Catalog.Utilities.Core.Formatters;

    public static class CurrencyHelper
    {
        /// <summary>
        /// Apply currency formation to the given values
        /// </summary>
        /// <param name="country">Country code Ex. us, fr</param>
        /// <param name="language">Language Ex. en-US, fr-FR </param>
        /// <param name="value">Acutal value to apply formatting</param>
        /// <param name="currency">Currency code Ex. USD, EUR</param>
        /// <returns>Formatted value</returns>
        public static string FormatCurrency(string country, string language, decimal value, string currency)
        {
            var currencyCulture = CurrencySymbolOverride.GetCultureInfoFromCurrencyCodeMarketAndLocale(currency, country, language);
            return currencyCulture != null
                ? CurrencySymbolOverride.FormatPriceByCultureInfoWithRequiredEncodings((double)value, currencyCulture)
                : string.Format("{0} {1}", value, currency);
        }

        /// <summary>
        /// Get currency symbol for the given market/country/culture
        /// </summary>
        /// <param name="country">Country code Ex. us, fr</param>
        /// <param name="language">Language Ex. en-US, fr-FR </param>
        /// <param name="currency">Currency code Ex. USD, EUR</param>
        /// <returns>Currency symbol</returns>
        public static string GetCurrencySymbol(string country, string language, string currency)
        {
            var currencyCulture = CurrencySymbolOverride.GetCultureInfoFromCurrencyCodeMarketAndLocale(currency, country, language);
            return currencyCulture != null
                ? currencyCulture.NumberFormat?.CurrencySymbol
                : currency;
        }
    }
}
