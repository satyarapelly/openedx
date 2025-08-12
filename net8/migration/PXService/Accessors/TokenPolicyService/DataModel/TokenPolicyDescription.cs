// <copyright file="TokenPolicyDescription.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService.DataModel
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class TokenPolicyDescription
    {
        /// <summary>
        /// Gets or sets the Asset associated with the token
        /// </summary>
        [JsonProperty(PropertyName = "asset")]
        public TokenAsset Asset { get; set; }

        /// <summary>
        /// Gets or sets catalog information corresponding to the token
        /// </summary>
        [JsonProperty(PropertyName = "catalogInfo")]
        public TokenDescriptionCatalogInfo CatalogInfo { get; set; }

        /// <summary>
        /// Gets or sets the legacy offer Id (if the token is a legacy token)
        /// </summary>
        [JsonProperty(PropertyName = "legacyOfferId")]
        public string LegacyOfferId { get; set; }

        /// <summary>
        /// Gets or sets the results of any policies evaluated for the token
        /// </summary>
        [JsonProperty(PropertyName = "policyEvaluation")]
        public TokenPolicyEvaluation PolicyEvaluation { get; set; }

        /// <summary>
        ///     Gets or sets the state of the token. The valid states are:
        /// <list type="table">
        ///     <listheader>
        ///         <state>Token State</state>
        ///         <description>Description</description>
        ///     </listheader>
        ///     <item>
        ///         <state>Active</state>
        ///         <description>The token is in an active state, and may be redeemed.</description>
        ///     </item>
        ///     <item>
        ///         <state>Deactivated</state>
        ///         <description>The token has been deactivated, and may not be redeemed from this state.</description>
        ///     </item>
        ///     <item>
        ///         <state>Inactive</state>
        ///         <description>The token has been minted, but has not yet been activated.</description>
        ///     </item>
        ///     <item>
        ///         <state>Redeemed</state>
        ///         <description>The token has already been redeemed.</description>
        ///     </item>
        ///     <item>
        ///         <state>Scrapped</state>
        ///         <description>The token has been scrapped, and may no longer be redeemed.</description>
        ///     </item>
        /// </list>
        /// </summary>
        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the reason a token is in its current state (optional)
        /// The set of possible StateReason values may change without notice.
        /// </summary>
        [JsonProperty(PropertyName = "stateReason")]
        public string StateReason { get; set; }

        /// <summary>
        /// Gets or sets category indicating a class of assets that the token represents.
        /// </summary>
        /// <remarks>Always use associated CatalogInfo to determine related product information.
        /// This property is an approximation of asset "class" to help determine the token type in certain
        /// cases where CatalogInfo is not available for the token.</remarks>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "tokenCategory")]
        public TokenCategory TokenCategory { get; set; }
    }
}