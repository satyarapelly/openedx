// <copyright file="RequestTokenRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Represents a network token request.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class RequestTokenRequest
    {
        /// <summary>
        /// Gets or sets the reference to the PAN and expiration date in Castle, pass-through to PIRelay who knows how to resolve this.
        /// Mandatory.
        /// </summary>
        public string SecureDataId { get; set; }

        /// <summary>
        /// Gets or sets the reference to the PAN in PCE Tokenization service, pass-through to PIRelay who knows how to resolve this.
        /// Mandatory.
        /// </summary>
        public string PanRef { get; set; }

        /// <summary>
        /// Gets or sets the external card reference. When called from PIMS this will be the PI ID.
        /// Mandatory.
        /// </summary>
        public string ExternalCardReference { get; set; }

        /// <summary>
        /// Gets or sets the external card reference type.
        /// Mandatory.
        /// </summary>
        public ExternalCardReferenceType ExternalCardReferenceType { get; set; }

        /// <summary>
        /// Gets or sets the card provider name.
        /// Mandatory.
        /// </summary>
        public NetworkProviderName CardProviderName { get; set; }

        /// <summary>
        /// Gets or sets the network token usage.
        /// Mandatory.
        /// </summary>
        public NetworkTokenUsage NetworkTokenUsage { get; set; }

        /// <summary>
        /// Gets or sets the country code.
        /// Mandatory.
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the language code.
        /// Mandatory.
        /// </summary>
        public string LanguageCode { get; set; }

        /// <summary>
        /// Gets or sets the account holder name.
        /// Optional.
        /// </summary>
        public string AccountHolderName { get; set; }

        /// <summary>
        /// Gets or sets the account holder email address.
        /// Optional.
        /// </summary>
        public string AccountHolderEmail { get; set; }

        /// <summary>
        /// Gets or sets the billing address.
        /// Optional.
        /// </summary>
        public AddressInfo BillingAddress { get; set; }

        /// <summary>
        /// Gets or sets the expiry month of the card.
        /// </summary>
        public int? ExpiryMonth { get; set; }

        /// <summary>
        /// Gets or sets the expiry year of the card.
        /// </summary>
        public int? ExpiryYear { get; set; }

        /// <summary>
        /// Gets or sets the cvv token of the card.
        /// </summary>
        public string CvvToken { get; set; }
    }
}