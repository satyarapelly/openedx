// <copyright file="Identity.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.AccountService
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class representing the identity object schema
    /// </summary>
    public class Identity
    {
        /// <summary>
        /// Gets or sets a value indicating the provider name
        /// </summary>
        [JsonProperty(PropertyName = "provider")]
        public string Provider { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the type
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string IdentityType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the data type
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public IdentityData IdentityData { get; set; }
    }
}