// <copyright file="PasskeyAction.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Enum of the supported action types.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PasskeyAction
    {
        /// <summary>
        /// Added as a default action
        /// </summary>
        UNKNOWN,

        /// <summary>
        /// Device registration action
        /// </summary>
        REGISTER_DEVICE_BINDING,

        /// <summary>
        /// Passkey registration action
        /// </summary>
        REGISTER_PASSKEY,

        /// <summary>
        /// Passkey authentication action
        /// </summary>
        AUTHENTICATE,
    }
}