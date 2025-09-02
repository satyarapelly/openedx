// <copyright file="CreateOrderRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PurchaseService
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class CreateOrderRequest
    {
        [JsonProperty("billingInformation")]
        public BillingInformation BillingInformation { get; set; }

        [JsonProperty("checkEligibility")]
        public bool CheckEligibility { get; set; }

        [JsonProperty("clientContext")]
        public ClientContext ClientContext { get; set; }

        [JsonProperty("items")]
        public IEnumerable<CreateOrderLineItem> Items { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("market")]
        public string Market { get; set; }

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("orderState")]
        public string OrderState { get; set; }
    }
}