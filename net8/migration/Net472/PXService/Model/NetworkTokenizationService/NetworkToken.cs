// <copyright file="NetworkToken.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Newtonsoft.Json;

    public class NetworkToken
    {
        [JsonProperty("networkTokenId")]
        public string NetworkTokenId { get; set; }

        [JsonProperty("networkTokenUsage")]
        public string NetworkTokenUsage { get; set; }

        [JsonProperty("externalCardReference")]
        public string ExternalCardReference { get; set; }

        [JsonProperty("externalCardReferenceType")]
        public string ExternalCardReferenceType { get; set; }

        [JsonProperty("tokenInfo")]
        public TokenInfo TokenInfo { get; set; }

        [JsonProperty("clientDeviceInfo")]
        public ClientDeviceInfo ClientDeviceInfo { get; set; }

        [JsonProperty("cardMetadata")]
        public CardArt CardMetadata { get; set; }

        [JsonProperty("srcFlowId")]
        public string SrcFlowId { get; set; }
    }
}