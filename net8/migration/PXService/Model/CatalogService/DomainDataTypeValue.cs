// <copyright file="DomainDataTypeValue.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.CatalogService
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class DomainDataTypeValue
    {
        [JsonProperty(PropertyName = "Key")]
        public string Key { get; set; }

        [JsonProperty(PropertyName = "Values")]
        public IList<DomainDataKeyValue> Values { get; } = new List<DomainDataKeyValue>();
    }
}
