// <copyright file="RequestDeviceBindingFidoRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace MMicrosoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Represents a request to bind the client device to a network token.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class RequestDeviceBindingFidoRequest
    {
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

        /// <summary>
        /// Gets or sets the browser data.
        /// </summary>
        public object BrowserData { get; set; }

        /// <summary>
        /// Gets or sets the session context.
        /// </summary>
        public object SessionContext { get; set; }
    }
}
