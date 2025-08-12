// <copyright file="ShippingDetails.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.D365Service
{
    using Newtonsoft.Json;

    public class ShippingDetails
    {
        [JsonProperty("shipFromId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string ShipFromId { get; set; }

        [JsonProperty("shipToAddressId", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string ShipToAddressId { get; set; }

        [JsonProperty("shippingMethodId", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string ShippingMethodId { get; set; }
    }
}