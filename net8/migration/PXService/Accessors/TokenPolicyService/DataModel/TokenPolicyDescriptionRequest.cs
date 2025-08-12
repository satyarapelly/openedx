// <copyright file="TokenPolicyDescriptionRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>
// Sourced from https://microsoft.visualstudio.com/Universal%20Store/_git/SC.MDollar.Service.Purchase.ToPS?path=/src/TokenPolicyServiceContracts/V1/TokenPolicyDescriptionRequestV1.cs&_a=contents&version=GBmaster

namespace Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService.DataModel
{
    using Newtonsoft.Json;

    public class TokenPolicyDescriptionRequest
    {
        /// <summary>
        /// Gets or sets the language. Only used in "Qualified" API requests.
        /// </summary>
        [JsonProperty(PropertyName = "language")]
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the market. Only used in "Qualified" API requests.
        /// </summary>
        [JsonProperty(PropertyName = "market")]
        public string Market { get; set; }

        /// <summary>
        /// Gets or sets the token identifier type, indicating the type of the token
        /// </summary>
        [JsonProperty(PropertyName = "tokenIdentifierType")]
        public TokenDescriptionRequestIdentifierType TokenIdentifierType { get; set; }

        /// <summary>
        /// Gets or sets the token identifier value.
        /// </summary>
        [JsonProperty(PropertyName = "tokenIdentifierValue")]
        public string TokenIdentifierValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to return Redeemed Party details.
        /// </summary>
        [JsonProperty(PropertyName = "returnRedeemedParty")]
        public bool ReturnRedeemedParty { get; set; }
    }
}