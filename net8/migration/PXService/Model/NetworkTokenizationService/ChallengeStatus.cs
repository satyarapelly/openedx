// <copyright file="ChallengeStatus.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Supported challenge method types.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ChallengeStatus
    {
        /// <summary>
        /// Indicates an unknown challenge status.
        /// </summary>
        Unknown,

        /// <summary>
        /// The challenge is in Challenge status.
        /// </summary>
        Challenge,

        /// <summary>
        /// The challenge is in Approved status.
        /// </summary>
        Approved,

        /// <summary>
        /// The challenge is in Declined status.
        /// </summary>
        Declined,
    }
}
