// <copyright file="OrderLineItem.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Tests.Common.Model.PurchaseService
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class OrderLineItem
    {
        [JsonProperty("currencyCode")]
        public string CurrencyCode { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("lineItemId")]
        public string LineItemId { get; set; }

        [JsonProperty("listPrice")]
        public double ListPrice { get; set; }

        [JsonProperty("payments")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<OrderPaymentInstrument> Payments { get; set; }

        [JsonProperty("billingState")]
        public string BillingState { get; set; }

        [JsonProperty("productId")]
        public string ProductId { get; set; }

        [JsonProperty("productType")]
        public string ProductType { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("skuId")]
        public string SkuId { get; set; }

        [JsonProperty("shipToAddressId")]
        public string ShipToAddressId { get; set; }

        [JsonProperty("taxAmount")]
        public double TaxAmount { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("totalAmount")]
        public double TotalAmount { get; set; }

        [JsonProperty("recurrenceId")]
        public string RecurrenceId { get; set; }

        [JsonProperty("retailPrice")]
        public double RetailPrice { get; set; }
    }
}