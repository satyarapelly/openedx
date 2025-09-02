// <copyright file="AppleProviderData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.WalletService
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class AppleProviderData
    {
        [JsonProperty(PropertyName = "merchantIdentifier")]
        public string MerchantIdentifier { get; set; }

        [JsonProperty(PropertyName = "merchantCapabilities")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<string> MerchantCapabilities { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }
    }
}