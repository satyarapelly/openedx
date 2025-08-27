// <copyright file="TokenDescriptionPolicyType.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService.DataModel
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// The token policy type
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TokenDescriptionPolicyType
    {
        /// <summary>
        /// A policy that evaluates issues when trying to find associated
        /// offers for a given token.
        /// </summary>
        Catalog,

        /// <summary>
        /// Risk Policy evaluation of geofencing rules.
        /// </summary>
        Geofencing,

        /// <summary>
        /// A policy that evaluates restrictions on state changes (e.g. redemptions)
        /// due to issues with token state itself, such as the token state or state change 
        /// date restrictions.
        /// </summary>
        TokenStateChange,

        /// <summary>
        /// Policies related to legacy Xbox 360 tokens
        /// </summary>
        LegacyXbox
    }
}