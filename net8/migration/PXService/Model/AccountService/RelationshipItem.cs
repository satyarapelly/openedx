// <copyright file="RelationshipItem.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.AccountService
{
    using Newtonsoft.Json;

    public class RelationshipItem
    {
        /// <summary>
        /// Gets or sets a value indicating the related account id.
        /// </summary>
        [JsonProperty(PropertyName = "related_account_id")]
        public string RelatedAccountId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the related account id.
        /// </summary>
        [JsonProperty(PropertyName = "account_id")]
        public string AccountId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the relationship type.
        /// </summary>
        [JsonProperty(PropertyName = "relationship_type")]
        public string RelationshipType { get; set; }

        /// <summary>
        ///  Gets or sets a value indicating the id of a given relationship
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        ///  Gets or sets the etag
        /// </summary>
        [JsonProperty(PropertyName = "etag")]
        public string Etag { get; set; }
    }
}