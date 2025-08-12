// <copyright file="GetTokenMetadataResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Represents a GetTokenMetadata API call response.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class GetTokenMetadataResponse
    {
        /// <summary>
        /// Gets or sets the network token ID.
        /// </summary>
        public string NetworkTokenId { get; set; }

        /// <summary>
        /// Gets or sets the network token usage.
        /// </summary>
        public NetworkTokenUsage NetworkTokenUsage { get; set; }

        /// <summary>
        /// Gets or sets an external reference to a card from the client.
        /// </summary>
        public string ExternalCardReference { get; set; }

        /// <summary>
        /// Gets or sets the type of the external reference to a card from the client.
        /// </summary>
        public ExternalCardReferenceType ExternalCardReferenceType { get; set; }

        /// <summary>
        /// Gets or sets the token metadata e.g. status, last four digits, exp date.
        /// </summary>
        public TokenInfo TokenInfo { get; set; }

        /// <summary>
        /// Gets or sets the status of the client device e.g. binding required, device enrolled, binding complete, etc.
        /// </summary>
        public ClientDeviceInfo ClientDeviceInfo { get; set; }

        /// <summary>
        /// Gets or sets the card metadata of the token including art URL.
        /// </summary>
        public CardMetadata CardMetadata { get; set; }

        /// <summary>
        /// Gets or sets the [X-Src-Cx-Flow-Id] header data.
        /// </summary>
        public string SrcFlowId { get; set; }
    }
}