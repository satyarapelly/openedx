// <copyright file="PlatformType.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Enum of the supported platform types.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PlatformType
    {
        /// <summary>
        /// Added as a default device type
        /// </summary>
        Unknown,

        /// <summary>
        /// Represents IOS app.
        /// </summary>
        Ios,

        /// <summary>
        /// Represents Android app.
        /// </summary>
        Android,

        /// <summary>
        /// Represents Windows app.
        /// </summary>
        Windows,

        /// <summary>
        /// Represents Browser-based app.
        /// </summary>
        Web,
    }
}