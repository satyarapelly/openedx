// <copyright file="PaymentRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentThirdPartyService
{
    using Newtonsoft.Json;

    public class PaymentRequest
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "sellerId")]
        public string SellerId { get; set; }

        [JsonProperty(PropertyName = "product")]
        public Product Product { get; set; }

        [JsonProperty(PropertyName = "context")]
        public string Context { get; set; }

        [JsonProperty(PropertyName = "trackingId")]
        public string TrackingId { get; set; }

        [JsonProperty(PropertyName = "platformType")]
        public string PlatformType { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
    }
}