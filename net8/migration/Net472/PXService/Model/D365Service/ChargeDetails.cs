// <copyright file="ChargeDetails.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.D365Service
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class ChargeDetails
    {
        [JsonProperty("quantityItemIds", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public IList<string> QuantityItemIds { get; set; }

        [JsonProperty("itemCostDetails", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        public CostDetails ItemCostDetails { get; set; }

        [JsonProperty("shippingCostDetails", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public CostDetails ShippingCostDetails { get; set; }

        [JsonProperty("extraCostDetails", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ExtraCostDetails ExtraCostDetails { get; set; }

        [JsonProperty("totalAmount", Required = Required.Always)]
        public decimal TotalAmount { get; set; }

        [JsonProperty("totalTaxAmount", Required = Required.Always)]
        public decimal TotalTaxAmount { get; set; }
    }
}