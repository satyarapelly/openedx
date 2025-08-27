// <copyright file="PasskeyOperationResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Represents a response to a passkey authentication request.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class PasskeyOperationResponse
    {
        /// <summary>
        /// Gets or sets the authentication context.
        /// </summary>
        public PasskeyAuthContext AuthContext { get; set; }

        /// <summary>
        /// Gets or sets the passkey action that needs to be acted upon.
        /// </summary>
        public PasskeyAction Action { get; set; } = PasskeyAction.UNKNOWN;
    }
}
