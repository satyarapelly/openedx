// <copyright file="TokenDescriptionMatchingAvailability.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService.DataModel
{
    using Newtonsoft.Json;

    public class TokenDescriptionMatchingAvailability
    {
        /// <summary>
        /// Gets or sets the catalog action filtered for the availability.
        /// </summary>
        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the ID of the modern catalog availability.
        /// </summary>
        [JsonProperty(PropertyName = "availabilityId")]
        public string AvailabilityId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the modern catalog product.
        /// </summary>
        [JsonProperty(PropertyName = "productId")]
        public string ProductId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the modern catalog sku.
        /// </summary>
        [JsonProperty(PropertyName = "skuId")]
        public string SkuId { get; set; }

        /// <summary>
        /// Gets or sets the associated token validation policy, if any.
        /// </summary>
        [JsonProperty(PropertyName = "tokenValidationPolicy")]
        public string TokenValidationPolicy { get; set; }
    }
}