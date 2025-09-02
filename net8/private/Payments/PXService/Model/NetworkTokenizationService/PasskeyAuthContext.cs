// <copyright file="PasskeyAuthContext.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Represents the authentication context for a passkey.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class PasskeyAuthContext
    {
        /// <summary>
        /// Gets or sets the endpoint.
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the payload.
        /// </summary>
        public string Payload { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the platform type.
        /// </summary>
        public string PlatformType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Identifier.
        /// </summary>
        public string Identifier { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the authentication preferences enabled.
        /// </summary>
        public AuthenticationPreferencesEnabled AuthenticationPreferencesEnabled { get; set; } = new AuthenticationPreferencesEnabled();
    }
}
