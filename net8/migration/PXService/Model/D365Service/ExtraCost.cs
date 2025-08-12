// <copyright file="ExtraCost.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.D365Service
{
    using Newtonsoft.Json;

    public class ExtraCost
    {
        [JsonProperty("extraCostType", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string ExtraCostType { get; set; }

        [JsonProperty("amount", Required = Required.Always)]
        public decimal Amount { get; set; }

        [JsonProperty("taxAmount", Required = Required.Always)]
        public decimal TaxAmount { get; set; }
    }
}