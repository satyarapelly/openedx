// <copyright file="MerchantIdentifier.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// MerchantIdentifier Entity.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class MerchantIdentifier
    {
        /// <summary>
        /// Gets or sets The merchant’s URL, Application Package ID, or Application Bundle ID.
        /// </summary>
        public string ApplicationUrl { get; set; }

        /// <summary>
        /// Gets or sets the merchant name.
        /// </summary>
        public string MerchantName { get; set; }
    }
}
