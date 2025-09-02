// <copyright file="CreateOrderLineItem.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PurchaseService
{
    using Newtonsoft.Json;

    public class CreateOrderLineItem
    {
        [JsonProperty("availabilityId")]
        public string AvailabilityId { get; set; }

        [JsonProperty("productId")]
        public string ProductId { get; set; }

        [JsonProperty("skuId")]
        public string SkuId { get; set; }
    }
}