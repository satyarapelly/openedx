// <copyright file="Subscription.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Tests.Common.Model.PurchaseService
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class Subscription
    {
        [JsonProperty("autoRenew")]
        public bool AutoRenew { get; set; }

        [JsonProperty("csvTopOffPaymentInstrumentId")]
        public string CsvTopOffPaymentInstrumentId { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("documentVersion")]
        public int DocumentVersion { get; set; }

        [JsonProperty("expirationTime")]
        public DateTime ExpirationTime { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("purchaserAccountId")]
        public string PurchaserAccountId { get; set; }

        [JsonProperty("entitlementId")]
        public string EntitlementId { get; set; }

        [JsonProperty("market")]
        public string Market { get; set; }

        [JsonProperty("billingDate")]
        public DateTime BillingDate { get; set; }

        [JsonProperty("paymentInstrumentId")]
        public string PaymentInstrumentId { get; set; }

        [JsonProperty("paymentInstrumentOwner")]
        public string PaymentInstrumentOwner { get; set; }

        [JsonProperty("paymentInstrumentSessionId")]
        public string PaymentInstrumentSessionId { get; set; }

        [JsonProperty("productId")]
        public string ProductId { get; set; }

        [JsonProperty("purchaser")]
        public string Purchaser { get; set; }

        [JsonProperty("recurrenceGroup")]
        public string RecurrenceGroup { get; set; }

        [JsonProperty("recurrencePolicyId")]
        public string RecurrencePolicyId { get; set; }

        [JsonProperty("recurrenceState")]
        public string RecurrenceState { get; set; }

        [JsonProperty("renewalTag")]
        public string RenewalTag { get; set; }

        [JsonProperty("skuId")]
        public string SkuId { get; set; }

        [JsonProperty("startTime")]
        public DateTime StartTime { get; set; }

        [JsonProperty("changeRecords")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<SubscriptionChangeRecord> SubscriptionChangeRecords { get; set; }
    }
}