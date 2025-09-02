// <copyright file="FeatureDiffTest.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.ApiSurface
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    /// <summary>
    /// FeatureDiffTest class to store the test data for a PSS feature 
    /// </summary>
    public class FeatureDiffTest
    {
        [JsonProperty(PropertyName = "featureName")]
        public string FeatureName { get; set; }
        
        [JsonProperty(PropertyName = "disableTesting")]
        public bool DisableTesting { get; set; }

        [JsonProperty(PropertyName = "testCountries")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public List<string> TestCountries { get; set; }

        [JsonProperty(PropertyName = "testPartners")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public List<string> TestPartners { get; set; }

        [JsonProperty(PropertyName = "tests")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public List<FeatureDiffTestConfig> Tests { get; set; }
    }
}
