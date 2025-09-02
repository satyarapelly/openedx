// <copyright file="PidlValidationParameter.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    // Task 17438397: Remove propertyName and pidlIdentity and make urlValidationType a required property after clients move to the new contract
    public class PidlValidationParameter
    {
        [JsonProperty(PropertyName = "propertyName", Required = Required.AllowNull)]
        public string PropertyName { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "urlValidationType", Required = Required.AllowNull)]
        public string UrlValidationType { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set for serialization")]
        [JsonProperty(PropertyName = "pidlIdentity", Required = Required.AllowNull)]
        public Dictionary<string, string> PidlIdentity { get; set; }
    }
}