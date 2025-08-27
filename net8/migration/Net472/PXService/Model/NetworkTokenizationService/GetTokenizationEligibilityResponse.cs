// <copyright file="GetTokenizationEligibilityResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Represents tokenization eligibility response.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class GetTokenizationEligibilityResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether tokenization eligibility is available or not.
        /// </summary>
        public bool Tokenizable { get; set; }
    }
}
