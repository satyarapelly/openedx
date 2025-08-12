// <copyright file="PasskeyOperationRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Represents a request to authenticate a passkey.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class PasskeyOperationRequest
    {
        /// <summary>
        /// Gets or sets the authentication amount. Max 7 digits, no decimals.
        /// </summary>
        public int AuthenticationAmount { get; set; }

        /// <summary>
        /// Gets or sets the currency code.
        /// </summary>
        public string CurrencyCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the browser data.
        /// </summary>
        public object BrowserData { get; set; }

        /// <summary>
        /// Gets or sets the session context.
        /// </summary>
        public object SessionContext { get; set; }

        /// <summary>
        /// Gets or sets the merchant identifier.
        /// </summary>
        public MerchantIdentifier MerchantIdentifier { get; set; }
    }
}
