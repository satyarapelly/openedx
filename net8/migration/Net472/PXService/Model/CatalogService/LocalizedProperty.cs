// <copyright file="LocalizedProperty.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.CatalogService
{
    using Newtonsoft.Json;

    public class LocalizedProperty
    {
        [JsonProperty("ProductTitle")]
        public string ProductTitle { get; set; }
    }
}