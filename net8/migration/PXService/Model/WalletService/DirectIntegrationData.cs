// <copyright file="DirectIntegrationData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.WalletService
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class DirectIntegrationData
    {
        [JsonProperty(PropertyName = "PiFamily")]
        public string PiFamily { get; set; }

        [JsonProperty(PropertyName = "PiType")]
        public string PiType { get; set; }

        [JsonProperty(PropertyName = "CountrySupportedNetworks")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public Dictionary<string, List<string>> CountrySupportedNetworks { get; set; }

        [JsonProperty(PropertyName = "ProviderData")]
        public object ProviderData { get; set; }
    }
}