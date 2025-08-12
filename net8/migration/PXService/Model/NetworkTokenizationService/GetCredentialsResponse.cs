// <copyright file="GetCredentialsResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Represents a network token request.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class GetCredentialsResponse
    {
        /// <summary>
        /// Gets or sets the network token usage.
        /// </summary>
        public NetworkTokenUsage NetworkTokenUsage { get; set; }

        /// <summary>
        /// Gets or sets the credential type.
        /// </summary>
        public CredentialType CredentialType { get; set; }

        /// <summary>
        /// Gets or sets the encrypted token.
        /// </summary>
        public string EncryptedToken { get; set; }

        /// <summary>
        /// Gets or sets the credential value.
        /// </summary>
        public string CredentialValue { get; set; }

        /// <summary>
        /// Gets or sets the encrypted credential data.
        /// </summary>
        public string EncryptedCredentialData { get; set; }

        /// <summary>
        /// Gets or sets the expiry month of the token.
        /// </summary>
        public int? TokenExpiryMonth { get; set; }

        /// <summary>
        /// Gets or sets the expiry year of the token.
        /// </summary>
        public int? TokenExpiryYear { get; set; }

        /// <summary>
        ///  Gets or sets the Eci value of credential data.
        /// </summary>
        public string Eci { get; set; }
    }
}
