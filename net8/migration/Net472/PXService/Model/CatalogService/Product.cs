// <copyright file="Product.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.CatalogService
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class Product
    {
        [JsonProperty("LocalizedProperties")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<LocalizedProperty> LocalizedProperties { get; set; }

        [JsonProperty("ProductBSchema")]
        public string ProductBSchema { get; set; }

        [JsonProperty("ProductId")]
        public string ProductId { get; set; }

        [JsonProperty("ProductType")]
        public string ProductType { get; set; }

        [JsonProperty("ProductFamily")]
        public string ProductFamily { get; set; }

        [JsonProperty("ProductKind")]
        public string ProductKind { get; set; }
    }
}