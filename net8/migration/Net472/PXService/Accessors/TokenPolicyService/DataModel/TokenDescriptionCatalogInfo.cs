// <copyright file="TokenDescriptionCatalogInfo.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService.DataModel
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class TokenDescriptionCatalogInfo
    {
        /// <summary>
        /// Gets or sets any matching availabilities from the modern catalog that correspond to
        /// a token description request for a given token and other filters.
        /// </summary>
        [JsonProperty(PropertyName = "matchingAvailabilities")]
        public IEnumerable<TokenDescriptionMatchingAvailability> MatchingAvailabilities { get; set; }

        /// <summary>
        /// Gets or sets details from the Xbox 360 legacy catalog related to the token.
        /// </summary>
        [JsonProperty(PropertyName = "xbox360TokenDescription")]
        public Xbox360TokenDescription Xbox360TokenDescription { get; set; }
    }
}