// <copyright file="CurrencyHelper.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Helper
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Common.Web;

    public static class CurrencyHelper
    {
        private static Dictionary<string, Currency> currencyNameMap = null;

        private static object syncLock = new object();

        private static Dictionary<string, Currency> CurrencyNameMap
        {
            get
            {
                if (currencyNameMap == null)
                {
                    Initiate();
                }

                return currencyNameMap;
            }
        }

        /// <summary>
        /// This function is a centralized place for currency exponent mapping.
        /// This is based on ISO currency standard. If the provider has its own non-standard exponent setting, an extra check should happen in provider layer.
        /// </summary>
        /// <param name="currency">three character currency code</param>
        /// <returns>exponent or decimal place of the currency</returns>
        public static uint GetCurrencyExponent(string currency)
        {
            if (CurrencyNameMap.ContainsKey(currency))
            {
                return CurrencyNameMap[currency].CurrencyExponent;
            }
            else
            {
                throw new ValidationException(ErrorCode.InvalidCurrencyCode, string.Format("{0} is not a supported currency.", currency));
            }
        }

        private static void Initiate()
        {
            if (currencyNameMap == null)
            {
                lock (syncLock)
                {
                    if (currencyNameMap == null)
                    {
                        using (StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Microsoft.Commerce.Payments.Common.Resources.Currency.csv")))
                        {
                            Dictionary<string, Currency> tempCurrencyNameMap = new Dictionary<string, Currency>(StringComparer.InvariantCultureIgnoreCase);

                            reader.ReadLine();

                            while (reader.Peek() != -1)
                            {
                                string[] contents = reader.ReadLine().Split(',');
                                Currency currency = new Currency { CurrencyCode = contents[0].ToUpper(), CurrencyExponent = uint.Parse(contents[2]) };
                                tempCurrencyNameMap.Add(currency.CurrencyCode, currency);
                            }

                            currencyNameMap = tempCurrencyNameMap;
                        }
                    }
                }
            }
        }

        internal class Currency
        {
            public string CurrencyCode { get; set; }

            public uint CurrencyExponent { get; set; }
        }
    }
}
