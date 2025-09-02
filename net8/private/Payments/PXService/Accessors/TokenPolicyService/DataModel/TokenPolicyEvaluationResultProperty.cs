// <copyright file="TokenPolicyEvaluationResultProperty.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService.DataModel
{
    using Newtonsoft.Json;

    public class TokenPolicyEvaluationResultProperty
    {
        /// <summary>
        /// Gets or sets the name of the additional attribute
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value of the additional attribute
        /// </summary>
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }
}