// <copyright file="CredentialType.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Supported credential types.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CredentialType
    {
        /// <summary>
        /// None token verification value.
        /// </summary>
        None,

        /// <summary>
        /// dynamic token verification value.
        /// </summary>
        Dtvv,

        /// <summary>
        /// token authentication verification value.
        /// </summary>
        Tavv,
    }
}