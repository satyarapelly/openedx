// <copyright file="Order.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.D365Service
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class Order
    {
        [JsonProperty("totalRefundAmount", Required = Required.Always)]
        public decimal TotalRefundAmount { get; set; }

        [JsonProperty("totalChargedTaxAmount", Required = Required.Always)]
        public decimal TotalChargedTaxAmount { get; set; }

        [JsonProperty("totalChargedAmount", Required = Required.Always)]
        public decimal TotalChargedAmount { get; set; }

        [JsonProperty("totalTaxAmount", Required = Required.Always)]
        public decimal TotalTaxAmount { get; set; }

        [JsonProperty("totalAmount", Required = Required.Always)]
        public decimal TotalAmount { get; set; }

        [JsonProperty("omniChannelFulfillmentOrderId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string OmniChannelFulfillmentOrderId { get; set; }

        [JsonProperty("isUpdatePaymentInstrumentAllowed", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool IsUpdatePaymentInstrumentAllowed { get; set; }

        [JsonProperty("billingInformation", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public BillingInformation BillingInformation { get; set; }

        [JsonProperty("language", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Language { get; set; }

        [JsonProperty("market", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Market { get; set; }

        [JsonProperty("currencyCode", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string CurrencyCode { get; set; }

        [JsonProperty("totalRefundTaxAmount", Required = Required.Always)]
        public decimal TotalRefundTaxAmount { get; set; }

        [JsonProperty("isInManualReview", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool IsInManualReview { get; set; }

        [JsonProperty("orderPlacedDate", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? OrderPlacedDate { get; set; }

        [JsonProperty("paymentDetails", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public PaymentDetails PaymentDetails { get; set; }

        [JsonProperty("orderState", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string OrderState { get; set; }

        [JsonProperty("billingEvents", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public IList<BillingEvent> BillingEvents { get; set; }

        [JsonProperty("orderLineItems", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public IList<OrderLineItem> OrderLineItems { get; set; }

        [JsonProperty("salesOrderId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string SalesOrderId { get; set; }

        [JsonProperty("displayOrderId", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string DisplayOrderId { get; set; }

        [JsonProperty("orderId", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string OrderId { get; set; }

        [JsonProperty("receiptEmail", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string ReceiptEmail { get; set; }

        [JsonProperty("cid", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Cid { get; set; }

        [JsonProperty("puid", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public long Puid { get; set; }

        [JsonProperty("createdTime", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public DateTimeOffset CreatedTime { get; set; }

        [JsonProperty("testScenarios", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string TestScenarios { get; set; }
    }
}