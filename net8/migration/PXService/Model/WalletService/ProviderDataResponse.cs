// <copyright file="ProviderDataResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.WalletService
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class ProviderDataResponse
    {
        [JsonProperty(PropertyName = "merchantName")]
        public string MerchantName { get; set; }

        [JsonProperty(PropertyName = "integrationType")]
        public string IntegrationType { get; set; }

        [JsonProperty(PropertyName = "directIntegrationData")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<DirectIntegrationData> DirectIntegrationData { get; set; }
    }
}