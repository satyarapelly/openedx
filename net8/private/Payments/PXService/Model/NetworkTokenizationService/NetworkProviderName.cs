// <copyright file="NetworkProviderName.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Indicates the name of the network token provider.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NetworkProviderName
    {
        /// <summary>
        /// Indicates an unknown provider.
        /// </summary>
        Unknown,

        /// <summary>
        /// Indicates Visa as a network token provider.
        /// </summary>
        Visa,

        /// <summary>
        /// Indicates Mastercard as a network token provider.
        /// </summary>
        Mastercard,
    }
}