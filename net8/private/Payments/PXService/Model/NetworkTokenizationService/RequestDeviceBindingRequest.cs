// <copyright file="RequestDeviceBindingRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Represents a request to bind the client device to a network token.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class RequestDeviceBindingRequest
    {
        /// <summary>
        /// Gets or sets the network token ID.
        /// Mandatory.
        /// </summary>
        public string NetworkTokenId { get; set; }

        /// <summary>
        /// Gets or sets the account holder email address.
        /// Optional.
        /// </summary>
        public string AccountHolderEmail { get; set; }

        /// <summary>
        /// Gets or sets the platform type.
        /// Mandatory.
        /// </summary>
        public PlatformType PlatformType { get; set; }
    }
}
