// <copyright file="PidlModelHelper.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using Microsoft.Commerce.Payments.Pidl.Localization;
    using Microsoft.Commerce.Payments.PidlModel;

    public static class PidlModelHelper
    {
        /// <summary>
        /// Replaces placeholders like CountryId() and PartnerName in a given config string
        /// with values from the current context.
        /// </summary>
        public static string GetNonParameterizedString(string configText)
        {
            if (string.IsNullOrEmpty(configText))
                return configText;

            string retVal = configText;

            if (!string.IsNullOrEmpty(Context.Country))
            {
                retVal = retVal.Replace(Constants.ConfigSpecialStrings.CountryId, Context.Country);
            }

            if (!string.IsNullOrEmpty(Context.PartnerName))
            {
                retVal = retVal.Replace(Constants.ConfigSpecialStrings.PartnerName, Context.PartnerName);
            }

            return retVal;
        }

        /// <summary>
        /// Returns a localized version of the given config text based on current context or fallback language.
        /// </summary>
        public static string GetLocalizedString(string configText, string language = null)
        {
            if (string.IsNullOrEmpty(configText))
                return configText;

            if (Context.Culture != null)
            {
                return LocalizationRepository.Instance.GetLocalizedString(configText, Context.Culture);
            }

            if (!string.IsNullOrEmpty(language))
            {
                return LocalizationRepository.Instance.GetLocalizedString(configText, language);
            }

            return configText;
        }
    }
}