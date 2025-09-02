// <copyright file="PidlTransformationParameter.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlModel.V7
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class PidlTransformationParameter
    {
        [JsonProperty(PropertyName = "transformationTarget")]
        public string TransformationTarget { get; set; }

        [JsonProperty(PropertyName = "propertyName")]
        public string PropertyName { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set for serialization")]
        [JsonProperty(PropertyName = "pidlIdentity")]
        public Dictionary<string, string> PidlIdentity { get; set; }
    }
}