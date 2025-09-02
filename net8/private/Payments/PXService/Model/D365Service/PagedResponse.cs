// <copyright file="PagedResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.D365Service
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class PagedResponse<TContract> : IPartialErrorResponse
    {
        [JsonProperty("items", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public IList<TContract> Items { get; set; }

        [JsonProperty("hasNextPage", Required = Required.Always)]
        public bool HasNextPage { get; set; }
    }
}