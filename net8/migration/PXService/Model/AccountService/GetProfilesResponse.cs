// <copyright file="GetProfilesResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.AccountService
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Newtonsoft.Json;

    public class GetProfilesResponse
    {
        /// <summary>
        /// Gets or sets a value indicating the total count
        /// </summary>
        [JsonProperty(PropertyName = "total_count")]
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the list of items object.
        /// </summary>
        [JsonProperty(PropertyName = "items")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed for serialization purpose.")]
        public List<AccountLegalProfileV3> Items { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the object type
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