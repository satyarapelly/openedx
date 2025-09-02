// <copyright file="FeatureDiffTestConfig.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.ApiSurface
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class FeatureDiffTestConfig
    {
        [JsonProperty(PropertyName = "userType")]
        public string UserType { get; set; }

        [JsonProperty(PropertyName = "operations")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public List<string> Operations { get; set; }

        [JsonProperty(PropertyName = "types")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public List<string> Types { get; set; }

        [JsonProperty(PropertyName = "testSceanrioHeaders")]
        public string TestSceanrioHeaders { get; set; }
        
        [JsonProperty(PropertyName = "flights")]
        public string Flights { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
        
        [JsonProperty(PropertyName = "additionalHeaders")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public Dictionary<string, string> AdditionalHeaders { get; set; }

        [JsonProperty(PropertyName = "skipCombinations")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public List<FeatureTestSkipCombination> SkipCombinations { get; set; }
    }
}
