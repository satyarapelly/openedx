// <copyright file="TaxDetails.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.D365Service
{
    using Newtonsoft.Json;

    public class TaxDetails
    {
        [JsonProperty("taxType", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string TaxType { get; set; }

        [JsonProperty("taxAmount", Required = Required.Always)]
        public decimal TaxAmount { get; set; }
    }
}