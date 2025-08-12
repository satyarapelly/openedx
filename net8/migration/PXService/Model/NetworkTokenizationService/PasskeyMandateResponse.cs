// <copyright file="PasskeyMandateResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Represents a response to a passkey mandate request.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class PasskeyMandateResponse
    {
        /// <summary>
        /// Gets or sets the mandate identifier.
        /// </summary>
        public string MandateId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the client reference ID.
        /// </summary>
        public string ClientReferenceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the transaction ID.
        /// </summary>
        public string TransactionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public string Status { get; set; } = string.Empty;
    }
}
