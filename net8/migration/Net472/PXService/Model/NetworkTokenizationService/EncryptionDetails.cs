// <copyright file="EncryptionDetails.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Represents a network token request.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class EncryptionDetails
    {
        /// <summary>
        /// Gets or sets the certificate key.
        /// </summary>
        public string CertificateKey { get; set; }

        /// <summary>
        /// Gets or sets the certificate format.
        /// </summary>
        public string CertificateFormat { get; set; }

        /// <summary>
        /// Gets or sets the stored profile.
        /// </summary>
        public string StoredProfile { get; set; }
    }
}