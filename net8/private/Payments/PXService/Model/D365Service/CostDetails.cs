// <copyright file="CostDetails.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.D365Service
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class CostDetails
    {
        [JsonProperty("totalAmount", Required = Required.Always)]
        public decimal TotalAmount { get; set; }

        [JsonProperty("totalTaxAmount", Required = Required.Always)]
        public decimal TotalTaxAmount { get; set; }

        [JsonProperty("taxDetails", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public IList<TaxDetails> TaxDetails { get; set; }
    }
}