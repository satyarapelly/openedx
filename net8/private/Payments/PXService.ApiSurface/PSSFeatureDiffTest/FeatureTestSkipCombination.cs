// <copyright file="FeatureTestSkipCombination.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.ApiSurface
{
    using Newtonsoft.Json;

    public class FeatureTestSkipCombination
    {
        [JsonProperty(PropertyName = "partner")]
        public string Partner { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string ResourceType { get; set; }
        
        [JsonProperty(PropertyName = "operation")]
        public string Operation { get; set; }
        
        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }
    }
}
