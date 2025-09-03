// <copyright file="PimsMockResponseProviderAugmented.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;
    using global::Tests.Common.Model.Pims;

    public class PimsMockResponseProviderAugmented : PimsMockResponseProvider
    {
        public PimsMockResponseProviderAugmented() : base()
        {
            var countries = new List<string>() { "us", "jp", "my", "cn", "fi", "cz", "gb", "de", "br", "nl", "hk", "ru", "in", "xk", "ly", "ss", "id", "ph", "tw", "th", "vn", "tr", "se" };

            var pmsByCountryJson = File.ReadAllText(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Mocks",
                    "PIMS",
                    "PaymentMethodsByCountryAugmented.json"));

            var paymentMethodsByCountryAugmented = JsonConvert.DeserializeObject<Dictionary<string, List<PaymentMethod>>>(
                pmsByCountryJson);

            foreach (var country in countries)
            {
                if (!PaymentMethodsByCountry.ContainsKey(country))
                {
                    PaymentMethodsByCountry.Add(country, paymentMethodsByCountryAugmented[country]);
                }
                else
                {
                    PimsMockResponseProvider.PaymentMethodsByCountry[country] = paymentMethodsByCountryAugmented[country];
                }
            }
        }
    }
}