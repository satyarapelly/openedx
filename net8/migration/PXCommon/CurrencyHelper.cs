// <copyright file="CurrencyHelper.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    using System;
    using System.Globalization;

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
            var currencyCulture = GetCurrencyCulture(currency, country, language);

            if (currencyCulture == null)
            {
                return string.Format("{0} {1}", value, currency);
            }

            var clonedCulture = (CultureInfo)currencyCulture.Clone();
            clonedCulture.NumberFormat.CurrencySymbol = GetCurrencySymbol(country, language, currency);

            return string.Format(clonedCulture, "{0:C}", value);
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
            var currencyCulture = GetCurrencyCulture(currency, country, language);

            if (currencyCulture != null)
            {
                return currencyCulture.NumberFormat.CurrencySymbol;
            }

            var symbol = TryGetSymbolFromCurrencyCode(currency);
            return string.IsNullOrEmpty(symbol) ? currency : symbol;
        }

        private static CultureInfo? GetCurrencyCulture(string currency, string country, string language)
        {
            CultureInfo? candidate = null;

            foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                try
                {
                    var region = new RegionInfo(culture.Name);

                    if (region.ISOCurrencySymbol.Equals(currency, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrEmpty(country) && region.TwoLetterISORegionName.Equals(country, StringComparison.OrdinalIgnoreCase))
                        {
                            return culture;
                        }

                        candidate ??= culture;
                    }
                }
                catch (CultureNotFoundException)
                {
                }
            }

            if (candidate != null)
            {
                return candidate;
            }

            try
            {
                return new CultureInfo(language);
            }
            catch (CultureNotFoundException)
            {
                return null;
            }
        }

        private static string? TryGetSymbolFromCurrencyCode(string currency)
        {
            foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                try
                {
                    var region = new RegionInfo(culture.Name);
                    if (region.ISOCurrencySymbol.Equals(currency, StringComparison.OrdinalIgnoreCase))
                    {
                        return region.CurrencySymbol;
                    }
                }
                catch (CultureNotFoundException)
                {
                }
            }

            return null;
        }
    }
}