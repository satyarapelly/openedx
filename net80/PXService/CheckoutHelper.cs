// <copyright file="CheckoutHelper.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System.Collections.Generic;

    public class CheckoutHelper
    {
        /// <summary>
        /// List of supported locale for checkout terms in static resources service
        /// </summary>
        private static List<string> checkoutTermsAvailablelocale = new List<string>
        {
            "bg-bg", "cs-cz", "da-dk", "de-de", "en-us", "es-es", "fi-fi", "fr-ca", "fr-fr", "hr-hr", "hu-hu", "it-it", "ja-jp", "nb-no", "nl-nl", "pl-pl", "pt-pt", "ro-ro", "sv-se"
        };

        private static string checkoutTermsRelativeUrl = "{0}/resources/checkout/{1}/terms.htm";

        /// <summary>
        /// Get checkout terms URL based on the current locale
        /// </summary>
        /// <param name="baseUrl">Static resources service base url</param>
        /// <param name="locale">Current Locale</param>
        /// <returns>Checkout terms URL</returns>
        public static string GetCheckoutTermsURL(string baseUrl, string locale)
        {
            return string.Format(checkoutTermsRelativeUrl, baseUrl, checkoutTermsAvailablelocale.Contains(locale.ToLower()) ? locale.ToLower() : GlobalConstants.Defaults.Locale.ToLower());
        }
    }
}