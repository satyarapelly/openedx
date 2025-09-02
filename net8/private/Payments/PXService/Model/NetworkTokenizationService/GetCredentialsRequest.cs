// <copyright file="GetCredentialsRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Represents a network token request.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class GetCredentialsRequest
    {
        /// <summary>
        /// Gets or sets the network token usage.
        /// Mandatory.
        /// </summary>
        public NetworkTokenUsage NetworkTokenUsage { get; set; }

        /// <summary>
        /// Gets or sets the credential type.
        /// Mandatory.
        /// </summary>
        public CredentialType CredentialType { get; set; }

        /// <summary>
        /// Gets or sets the encryption details.
        /// </summary>
        public EncryptionDetails Encryption { get; set; }

        /// <summary>
        /// Gets or sets the MerchantURL.
        /// </summary>
        public string MerchantURL { get; set; }
    }
}