// <copyright file="AddressEnrichmentRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.AddressEnrichmentService.DataModel
{
    using Newtonsoft.Json;

    public class AddressEnrichmentRequest
    {
        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }
    }
}