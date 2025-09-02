// <copyright file="StoredValueFundingCatalog.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks
{
    using Newtonsoft.Json;

    public class StoredValueFundingCatalog
    {
        [JsonProperty("type")]
        public string CatalogType { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("gift_amount")]
        public decimal Amount { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }
}
