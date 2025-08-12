// <copyright file="Order.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentTransaction.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Newtonsoft.Json;
    using D365 = PXService.Model.D365Service;
    using Purchase = PXService.Model.PurchaseService;

    public class Order
    {
        public Order()
        {
        }

        public Order(Purchase.Order purchaseOrder)
        {
            if (purchaseOrder != null)
            {
                this.Amount = purchaseOrder.TotalAmount;
                this.Tax = purchaseOrder.TotalTaxAmount;
                this.UserId = purchaseOrder.Purchaser.IdentityValue;
                this.OrderedDate = purchaseOrder.OrderPlacedDate;
                this.RefundedDate = purchaseOrder.OrderRefundedDate;
                this.OrderId = purchaseOrder.OrderId;
                this.OrderState = purchaseOrder.OrderState;
                this.Piid = purchaseOrder.IsPIRequired ? purchaseOrder.BillingInformation?.PaymentInstrumentId : null;
                this.CsvTopOffPaymentInstrumentId = purchaseOrder.IsPIRequired ? purchaseOrder.BillingInformation?.CsvTopOffPaymentInstrumentId : null;

                // map from purchase order line items 
                if (purchaseOrder.OrderLineItems != null && purchaseOrder.OrderLineItems.Count > 0)
                {
                    this.Description = purchaseOrder.OrderLineItems[0].Title;
                    this.Currency = purchaseOrder.OrderLineItems[0].CurrencyCode;
                    this.SubscriptionId = purchaseOrder.OrderLineItems[0].RecurrenceId;
                    this.OrderLineItems = new List<OrderLineItem>();
                    purchaseOrder.OrderLineItems.ForEach(purchaseItem => this.OrderLineItems.Add(new OrderLineItem(purchaseItem)));
                }

                if (purchaseOrder.ClientContext != null)
                {
                    if (purchaseOrder.ClientContext.DeviceType.Equals("unknown", StringComparison.OrdinalIgnoreCase))
                    {
                        this.Device = purchaseOrder.ClientContext.DeviceFamily;
                    }
                    else
                    {
                        this.Device = purchaseOrder.ClientContext.DeviceType;
                    }
                }

                this.Country = purchaseOrder.Market;

                this.CheckPiResult = null;
            }
        }

        public Order(D365.Order d365Order)
        {
            if (d365Order != null)
            {
                // The required fields in Order in D365 are:
                // OrderId, DisplayOrderId, OrderLineItems, CreatedTime
                // Market, Language, TotalAmount, TotalTaxAmount, TotalChargedAmount
                // TotalChargedTaxAmount, TotalRefundAmount, TotalRefundTaxAmount
                this.Amount = (double)d365Order.TotalAmount; 
                this.Tax = (double)d365Order.TotalTaxAmount;
                this.UserId = d365Order?.Puid == null ? null : d365Order.Puid.ToString();
                
                this.OrderedDate = d365Order.CreatedTime.UtcDateTime;
                this.OrderId = d365Order.OrderId;
                this.OrderState = d365Order?.OrderState;
                this.SetPIIDFromD365Order(d365Order);
                this.CsvTopOffPaymentInstrumentId = null;

                // map from d365 order line items 
                if (d365Order.OrderLineItems != null && d365Order.OrderLineItems.Count > 0)
                {
                    this.Description = d365Order.OrderLineItems[0].Catalog?.Title;
                    //// Temporarily set USD as default value since listing d365 pending orders in North Star Page
                    //// will firstly launch in US market. Consider to do currency code mapping based on market(required field)
                    //// if this feature is launched to other markets in the future.
                    this.Currency = d365Order?.CurrencyCode ?? "USD";
                    //// There is no subscription in D365 orders since there are physical goods.
                    this.SubscriptionId = null;
                    this.OrderLineItems = new List<OrderLineItem>();
                    foreach (var d365Item in d365Order.OrderLineItems)
                    {
                        this.OrderLineItems.Add(new OrderLineItem(d365Item, this.Currency));
                    }
                }

                this.CheckPiResult = true;
            }
        }

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("orderLineItems")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<OrderLineItem> OrderLineItems { get; set; }

        [JsonProperty("clientContext")]
        public ClientContext ClientContext { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("orderedDate")]
        public DateTime OrderedDate { get; set; }

        [JsonProperty("csvTopOffPaymentInstrumentId")]
        public string CsvTopOffPaymentInstrumentId { get; set; }

        [JsonProperty("piid")]
        public string Piid { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("transactionStatus")]
        public TransactionStatus TransactionStatus { get; set; }

        [JsonProperty("orderState")]
        public string OrderState { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("taxAmount")]
        public double Tax { get; set; }

        [JsonProperty("refundedDate")]
        public DateTime RefundedDate { get; set; }

        [JsonProperty("SubscriptionId")]
        public string SubscriptionId { get; set; }

        [JsonProperty("checkPiResult")]
        public bool? CheckPiResult { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("device")]
        public string Device { get; set; }

        private void SetPIIDFromD365Order(D365.Order d365Order) 
        {
            this.Piid = null;

            if (d365Order?.PaymentDetails?.Payments != null && d365Order.PaymentDetails.Payments.Count > 0)
            {
                // It's possible that the order has multiple Piids during replacement scenario.
                // The target and source Piid appear at the same time. As dynamics 365 suggested we should select primary PI only.
                this.Piid = d365Order.PaymentDetails.Payments.FirstOrDefault(
                    item => item?.IsPrimary != null && item.IsPrimary == true 
                    && !string.IsNullOrEmpty(item.PaymentInstrumentId)).PaymentInstrumentId;
            }
        }
    }
}