// <copyright file="RestResource.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXCommon
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class RestResource
    {
        [JsonProperty(PropertyName = "type", Order = -2)]
        public string TypeName { get; set; }

        [JsonProperty(PropertyName = "version", Order = -1)]
        public string Version { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needs to be set for serialization purpose")]
        [JsonProperty(PropertyName = "links", Order = 999)]
        public Dictionary<string, RestLink> Links { get; set; }
    }
}
