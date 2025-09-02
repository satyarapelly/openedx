// <copyright file="Order.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PurchaseService
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentTransaction.Model;
    using Newtonsoft.Json;

    public class Order
    {
        [JsonProperty("billingInformation")]
        public BillingInformation BillingInformation { get; set; }

        [JsonProperty("isPIRequired")]
        public bool IsPIRequired { get; set; }

        [JsonProperty("purchaser")]
        public Purchaser Purchaser { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("market")]
        public string Market { get; set; }

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("orderLineItems")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<OrderLineItem> OrderLineItems { get; set; }

        [JsonProperty("orderPlacedDate")]
        public DateTime OrderPlacedDate { get; set; }

        [JsonProperty("orderRefundedDate")]
        public DateTime OrderRefundedDate { get; set; }

        [JsonProperty("orderState")]
        public string OrderState { get; set; }

        [JsonProperty("stateReasonCode")]
        public string StateReasonCode { get; set; }

        [JsonProperty("totalAmount")]
        public double TotalAmount { get; set; }

        [JsonProperty("totalAmountBeforeTax")]
        public double TotalAmountBeforeTax { get; set; }

        [JsonProperty("totalChargedToCsvTopOffPI")]
        public double TotalChargedToCsvTopOffPI { get; set; }

        [JsonProperty("totalItemAmount")]
        public double TotalItemAmount { get; set; }

        [JsonProperty("totalDeliveryPriceAmount")]
        public double TotalDeliveryPriceAmount { get; set; }

        [JsonProperty("totalDeliveryPriceTaxAmount")]
        public double TotalDeliveryPriceTaxAmount { get; set; }

        [JsonProperty("totalTaxAmount")]
        public double TotalTaxAmount { get; set; }

        [JsonProperty("totalFeeAmount")]
        public double TotalFeeAmount { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("clientContext")]
        public ClientContext ClientContext { get; set; }
    }
}