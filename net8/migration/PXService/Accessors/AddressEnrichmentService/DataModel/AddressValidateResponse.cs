// <copyright file="AddressValidateResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.AddressEnrichmentService.DataModel
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class AddressValidateResponse
    {
        [JsonProperty(PropertyName = "original_address")]
        public Address OriginalAddress { get; set; }

        [JsonProperty(PropertyName = "suggested_address")]

        public Address SuggestedAddress { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Without this, cannot get suggested addresses from enrichment service")]
        [JsonProperty(PropertyName = "suggested_addresses")]
        public List<Address> SuggestedAddresses { get; set; }

        [JsonProperty(PropertyName = "status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EnrichmentValidationStatus Status { get; set; }

        [JsonProperty(PropertyName = "validation_message")]
        public string ValidationMessage { get; set; }
    }
}