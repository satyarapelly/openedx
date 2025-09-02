// <copyright file="PidlModelHelper.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using Microsoft.Commerce.Payments.PidlModel;

    public class PidlModelHelper
    {
        // Example: configText could be "Enter a valid CountryId() address" and CurrentCountry=US
        // return value should be "Enter a valid US address"
        public static string GetNonParameterizedString(string configText)
        {
            string retVal = configText;
            if (!string.IsNullOrEmpty(Context.Country))
            {
                retVal = configText.Replace(Constants.ConfigSpecialStrings.CountryId, Context.Country);
            }

            return retVal;
        }

        // Example: configText is always in en-US strings (not resource keys).  When CurrentCulture is fr-FR
        // the return value should be the fr-FR equivalent of that en-US string
        public static string GetLocalizedString(string configText)
        {
            return configText;
        }
    }
}