// <copyright file="NetworkTokenStatus.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Indicates the status of the network token.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NetworkTokenStatus
    {
        /// <summary>
        /// Indicates an unknown status.
        /// </summary>
        Unknown,

        /// <summary>
        /// Indicates an active status of the token. If the token was suspected and later was resumed - the status will be active.
        /// </summary>
        Active,

        /// <summary>
        /// Indicates a suspended status of the token.
        /// </summary>
        Suspended,

        /// <summary>
        /// Indicates a deleted status of the token.
        /// </summary>
        Deleted,

        /// <summary>
        /// Indicates an expired status of the token.
        /// </summary>
        Expired,

        /// <summary>
        /// Indicates an Inactive status of the token.
        /// </summary>
        Inactive,
    }
}