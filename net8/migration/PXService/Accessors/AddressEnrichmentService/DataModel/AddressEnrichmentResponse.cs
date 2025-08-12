// <copyright file="AddressEnrichmentResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.AddressEnrichmentService.DataModel
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class AddressEnrichmentResponse
    {
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        [JsonProperty("suggested_addresses")]
        public List<SuggestedAddress> SuggestedAddresses { get; set; }
    }
}