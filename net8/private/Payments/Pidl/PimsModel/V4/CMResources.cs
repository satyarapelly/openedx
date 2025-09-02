// <copyright file="CMResources.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class CMResources<T> where T : class
    {
        [JsonProperty(PropertyName = "item_count")]
        public int ItemCount { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Json deserializer will need to write to this collection.  Hence it cannot be readonly.")]
        [JsonProperty(PropertyName = "items")]
        public List<T> Items { get; set; }
    }
}