// <copyright file="TokenAsset.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService.DataModel
{
    using Newtonsoft.Json;

    public class TokenAsset
    {
        /// <summary>
        /// Gets or sets an asset ID that maps to a reference in the source catalog.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the source of the asset ID, which can be used to map the identifier to a particular
        /// catalog.
        /// </summary>
        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the value of the asset, if one is associated.
        /// </summary>
        [JsonProperty(PropertyName = "assetValue")]
        public TokenAssetValue AssetValue { get; set; }
    }
}