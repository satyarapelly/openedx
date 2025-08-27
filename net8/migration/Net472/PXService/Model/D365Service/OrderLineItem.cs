// <copyright file="OrderLineItem.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.D365Service
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class OrderLineItem
    {
        [JsonProperty("psa", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string PSA { get; set; }

        [JsonProperty("bundleSlotType", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public BundleSlotType BundleSlotType { get; set; }

        [JsonProperty("bundleInstanceId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string BundleInstanceId { get; set; }

        [JsonProperty("bundleId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string BundleId { get; set; }

        [JsonProperty("recurrenceId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string RecurrenceId { get; set; }

        [JsonProperty("gifteeRecipientId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string GifteeRecipientId { get; set; }

        [JsonProperty("beneficiaryId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string BeneficiaryId { get; set; }

        [JsonProperty("buildToOrderDetails", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public BuildToOrderDetails BuildToOrderDetails { get; set; }

        [JsonProperty("fulfillmentDate", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? FulfillmentDate { get; set; }

        [JsonProperty("enfLink", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string EnfLink { get; set; }

        [JsonProperty("documentId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string DocumentId { get; set; }

        [JsonProperty("taxGroup", Required = Required.Always)]
        public string TaxGroup { get; set; }

        [JsonProperty("isTaxIncluded", Required = Required.Always)]
        public bool IsTaxIncluded { get; set; }

        [JsonProperty("isEligibleForUpdatePayment", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool IsEligibleForUpdatePayment { get; set; }

        [JsonProperty("returnableQuantity", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public uint ReturnableQuantity { get; set; }

        [JsonProperty("refundableQuantity", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public uint RefundableQuantity { get; set; }

        [JsonProperty("cancellableQuantity", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public uint CancellableQuantity { get; set; }

        [JsonProperty("quantity", Required = Required.Always)]
        public uint Quantity { get; set; }

        [JsonProperty("partialRefunds", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public IList<RefundDetails> PartialRefunds { get; set; }

        [JsonProperty("fullRefunds", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public IList<RefundDetails> FullRefunds { get; set; }

        [JsonProperty("actualChargeDetails", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ChargeDetails ActualChargeDetails { get; set; }

        [JsonProperty("purchaseChargeDetails", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ChargeDetails PurchaseChargeDetails { get; set; }

        [JsonProperty("catalog", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ProductDetails Catalog { get; set; }

        [JsonProperty("bundledCatalogs", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public IList<ProductDetails> BundledCatalogs { get; set; }

        [JsonProperty("paymentDetails", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public PaymentDetails PaymentDetails { get; set; }

        [JsonProperty("quantityItems", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public IList<QuantityItem> QuantityItems { get; set; }

        [JsonProperty("redeemedOrderId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string RedeemedOrderId { get; set; }

        [JsonProperty("lineItemState", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string LineItemState { get; set; }

        [JsonProperty("lineItemId", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string LineItemId { get; set; }

        [JsonProperty("shippingDetails", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ShippingDetails ShippingDetails { get; set; }

        [JsonProperty("returnByDate", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? ReturnByDate { get; set; }
    }
}