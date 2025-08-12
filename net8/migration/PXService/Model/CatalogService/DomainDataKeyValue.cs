// <copyright file="DomainDataKeyValue.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.CatalogService
{
    using Newtonsoft.Json;

    public class DomainDataKeyValue
    {
        [JsonProperty(PropertyName = "Value", Required = Required.Always)]
        public string Value { get; set; }
    }
}
