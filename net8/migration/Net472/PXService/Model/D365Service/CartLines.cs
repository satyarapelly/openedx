// <copyright file="CartLines.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.D365Service
{
    using Newtonsoft.Json;

    public class CartLines
    {
        [JsonProperty("shippingAddress")]
        public ShippingAddress ShippingAddress { get; set; }
    }
}