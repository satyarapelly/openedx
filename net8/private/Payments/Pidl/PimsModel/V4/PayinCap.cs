// <copyright file="PayinCap.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using Newtonsoft.Json;

    public class PayinCap
    {
        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "maxPrice")]
        public decimal MaxPrice { get; set; }

        [JsonProperty(PropertyName = "minPrice")]
        public decimal MinPrice { get; set; }
    }
}