// <copyright file="Subscription.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentTransaction.Model
{
    using System;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel;
    using Newtonsoft.Json;
    using Purchase = PXService.Model.PurchaseService;

    public class Subscription
    {
        public Subscription()
        {
        }

        public Subscription(Purchase.Subscription purchaseSubscription)
        {
            this.SubscriptionId = purchaseSubscription.Id;
            this.Piid = purchaseSubscription.PaymentInstrumentId;
            this.NextRenewalDate = purchaseSubscription.ExpirationTime;
            this.ProductId = purchaseSubscription.ProductId;
            this.RecurringFrequency = purchaseSubscription.RecurrencePolicyId;
            this.StartDate = purchaseSubscription.StartTime;
            this.Title = purchaseSubscription.RecurrenceGroup;
            this.RecurrenceState = purchaseSubscription.RecurrenceState;
            this.AutoRenew = purchaseSubscription.AutoRenew;
            this.CsvTopOffPaymentInstrumentId = purchaseSubscription.CsvTopOffPaymentInstrumentId;
            this.IsBlockingPi = null;
        }

        public Subscription(SubscriptionsInfo ctpSubscription)
        {
            this.SubscriptionId = ctpSubscription.SubscriptionId;
            this.Piid = ctpSubscription.PaymentInstrumentId;
            this.NextRenewalDate = ctpSubscription.NextBillDate;
            this.ProductId = ctpSubscription.ProductGuid.ToString();
            this.RecurringFrequency = ctpSubscription.NextCycle != null && ctpSubscription.NextCycle == 1 ? "Monthly" : "Yearly";
            this.StartDate = ctpSubscription.SubscriptionCycleStartDate;
            this.Title = ctpSubscription.SubscriptionDescription;
            this.RecurrenceState = GetRecurrenceState(ctpSubscription.SubscriptionStatusInfo);
            this.IsBlockingPi = null;
            this.AutoRenew = true; // TO DO: Temporary. Figure out if CTP has concept of autoRenew or is implied by the way reccurenceState is determined
        }

        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }

        [JsonProperty("autoRenew")]
        public bool AutoRenew { get; set; }

        [JsonProperty("startDate")]
        public DateTime? StartDate { get; set; }

        [JsonProperty("recurringFrequency")]
        public string RecurringFrequency { get; set; }

        [JsonProperty("nextRenewalDate")]
        public DateTime? NextRenewalDate { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("piid")]
        public string Piid { get; set; }

        [JsonProperty("productId")]
        public string ProductId { get; set; }

        [JsonProperty("recurrenceState")]
        public string RecurrenceState { get; set; }

        [JsonProperty("csvTopOffPaymentInstrumentId")]
        public string CsvTopOffPaymentInstrumentId { get; set; }

        [JsonProperty("isBlockingPi")]
        public bool? IsBlockingPi { get; set; }

        private static string GetRecurrenceState(SubscriptionStatusInfo subscriptionStatusInfo)
        {
            return (subscriptionStatusInfo != null && string.Equals(subscriptionStatusInfo.SubscriptionStatus, "Enabled", StringComparison.OrdinalIgnoreCase)) ? "Active" : "NotEnabled";
        }
    }
}