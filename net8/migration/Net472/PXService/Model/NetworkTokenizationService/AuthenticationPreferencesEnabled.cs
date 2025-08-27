// <copyright file="AuthenticationPreferencesEnabled.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Represents the authentication context for a passkey.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class AuthenticationPreferencesEnabled
    {
        /// <summary>
        /// Gets or sets the endpoint.
        /// </summary>
        public string ResponseMode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the payload.
        /// </summary>
        public string ResponseType { get; set; } = string.Empty;
    }
}