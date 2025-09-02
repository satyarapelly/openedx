// <copyright file="NetworkTokenUsage.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Indicates the intended usage of the network token.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NetworkTokenUsage
    {
        /// <summary>
        /// Indicates an unknown usage.
        /// </summary>
        Unknown,

        /// <summary>
        /// Indicates that the usage of the token is to transact on the 1st party merchant on Microsoft property.
        /// </summary>
        FirstPartyMerchant,

        /// <summary>
        /// Indicates that the usage of the token is to transact on a 3rd party merchant.
        /// </summary>
        ThirdPartyMerchant,

        /// <summary>
        /// Indicates that the usage of the token is to transact on a ecommerce merchant.
        /// </summary>
        EcomMerchant,
    }
}