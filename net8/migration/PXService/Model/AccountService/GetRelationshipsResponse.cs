// <copyright file="GetRelationshipsResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.AccountService
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class GetRelationshipsResponse
    {
        /// <summary>
        /// Gets or sets a value indicating the total count.
        /// </summary>
        [JsonProperty(PropertyName = "total_count")]
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the list of items object.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed for serialization purpose.")]
        [JsonProperty(PropertyName = "items")]
        public List<RelationshipItem> Items { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the object type.
        /// </summary>
        [JsonProperty(PropertyName = "object_type")]
        public string ObjectType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the resource status.
        /// </summary>
        [JsonProperty(PropertyName = "resource_status")]
        public string ResourceStatus { get; set; }
    }
}