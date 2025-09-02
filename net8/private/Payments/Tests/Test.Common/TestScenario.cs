// <copyright file="TestScenario.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Test.Common
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class TestScenario
    {
        [JsonProperty(PropertyName = "testScenarioName")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "responsesPerApiCall")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public Dictionary<string, ApiResponse> ResponsesPerApiCall { get; set; }
    }
}