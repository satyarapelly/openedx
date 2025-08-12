// <copyright file="SubscriptionChangeRecord.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PurchaseService
{
    using System;
    using Newtonsoft.Json;

    public class SubscriptionChangeRecord
    {
        [JsonProperty("autoRenew")]
        public bool AutoRenew { get; set; }

        [JsonProperty("billingDate")]
        public DateTime BillingDate { get; set; }

        [JsonProperty("changeReason")]
        public string ChangeReason { get; set; }

        [JsonProperty("changeReasonCode")]
        public string ChangeReasonCode { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("csvTopOffPaymentInstrumentId")]
        public string CsvTopOffPaymentInstrumentId { get; set; }

        [JsonProperty("expirationTime")]
        public DateTime ExpirationTime { get; set; }

        [JsonProperty("market")]
        public string Market { get; set; }

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("orderLineItemId")]
        public string OrderLineItemId { get; set; }

        [JsonProperty("paymentInstrumentId")]
        public string PaymentInstrumentId { get; set; }

        [JsonProperty("paymentInstrumentOwner")]
        public string PaymentInstrumentOwner { get; set; }

        [JsonProperty("paymentInstrumentSessionId")]
        public string PaymentInstrumentSessionId { get; set; }

        [JsonProperty("productId")]
        public string ProductId { get; set; }

        [JsonProperty("recurrenceGroup")]
        public string RecurrenceGroup { get; set; }

        [JsonProperty("recurrencePolicyId")]
        public string RecurrencePolicyId { get; set; }

        [JsonProperty("renewalTag")]
        public string RenewalTag { get; set; }

        [JsonProperty("skuId")]
        public string SkuId { get; set; }
    }
}