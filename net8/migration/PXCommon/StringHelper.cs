// <copyright file="StringHelper.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    using System.Collections.Generic;

    public static class StringHelper
    {
        /// <summary>
        /// Replaces placeholders with unicode values
        /// </summary>
        /// <param name="text">Text with unicode placeholders</param>
        /// <returns>Formatted value</returns>
        public static string MapUnicodeChars(string text)
        {
            KeyValuePair<string, string>[] iconMap =
                {
                    new KeyValuePair<string, string>("{SuperscriptOne}", Constants.CharUnicodes.SuperscriptOne),
                    new KeyValuePair<string, string>("{SuperscriptTwo}", Constants.CharUnicodes.SuperscriptTwo),
                    new KeyValuePair<string, string>("{SuperscriptThree}", Constants.CharUnicodes.SuperscriptThree),
                    new KeyValuePair<string, string>("{SuperscriptOpenParenthesis}", Constants.CharUnicodes.SuperscriptOpenParenthesis),
                    new KeyValuePair<string, string>("{SuperscriptCloseParenthesis}", Constants.CharUnicodes.SuperscriptCloseParenthesis)
                };

            foreach (KeyValuePair<string, string> kvp in iconMap)
            {
                if (text.Contains(kvp.Key))
                {
                    text = text.Replace(kvp.Key, kvp.Value);
                }
            }

            return text;
        }
    }
}
