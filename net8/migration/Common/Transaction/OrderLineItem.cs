// <copyright file="OrderLineItem.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Transaction
{
    using Newtonsoft.Json;

    /// <summary>
    /// An order line item is an individual line item related to a payment transaction.
    /// There can be more than 1 order line items that are part of a payment transaction.
    /// </summary>
    public class OrderLineItem
    {
        /// <summary>
        /// Gets or sets a description of the order line item. Note that it is not V4 contract. Threshold expect not use it.
        /// Alipay, Boku, Worldpay, Paypal are using the field as flex descriptor.
        /// </summary>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the net amount of this order line item. Note that it is not V4 contract. Threshold expect not use it.
        /// Paypal is using the field.
        /// </summary>
        [JsonProperty(PropertyName = "net_amount")]
        public decimal NetAmount { get; set; }

        /// <summary>
        /// Gets or sets the DeviceId. Note that it is not V4 contract. Threshold expect not use it. Threshold expect not use it.
        /// Paypal payin is using it as optional field.
        /// </summary>
        [JsonProperty(PropertyName = "device_id")]
        public string DeviceId { get; set; }

        //// Transaction service V4 contract start

        [JsonProperty(PropertyName = "catalog_uri")]
        public string CatalogUri { get; set; }

        [JsonProperty(PropertyName = "billing_record_id")]
        public string BillingRecordId { get; set; }

        [JsonProperty(PropertyName = "line_item_id")]
        public string LineItemId { get; set; }

        [JsonProperty(PropertyName = "item_name")]
        public string ItemName { get; set; }

        /// <summary>
        /// Gets or sets the charge amount of the order line item.
        /// Billing comments of the field
        ///     Tax inclusive.  $10 charge amount, $1 tax, $10 total charge, this value should be $10.
        ///     Not tax inclusive.  $10 charge amount, $1 tax, $11 total charge, this value should be $10.
        /// </summary>
        [JsonProperty(PropertyName = "charge_amount")]
        public decimal ChargeAmount { get; set; }

        [JsonProperty(PropertyName = "tax")]
        public decimal Tax { get; set; }

        [JsonProperty(PropertyName = "is_tax_inclusive")]
        public bool IsTaxInclusive { get; set; }

        [JsonProperty(PropertyName = "quantity")]
        public int Quantity { get; set; }

        [JsonProperty(PropertyName = "publisher_name")]
        public string PublisherName { get; set; }

        [JsonProperty(PropertyName = "product_code")]
        public string ProductCode { get; set; }

        [JsonProperty(PropertyName = "content_type")]
        public string ContentType { get; set; }

        [JsonProperty(PropertyName = "ip_address")]
        public string IpAddress { get; set; }

        public OrderLineItem Clone()
        {
            return new OrderLineItem
            {
                Description = this.Description,
                NetAmount = this.NetAmount,
                DeviceId = this.DeviceId,
                CatalogUri = this.CatalogUri,
                BillingRecordId = this.BillingRecordId,
                LineItemId = this.LineItemId,
                ItemName = this.ItemName,
                ChargeAmount = this.ChargeAmount,
                Tax = this.Tax,
                IsTaxInclusive = this.IsTaxInclusive,
                Quantity = this.Quantity,
                PublisherName = this.PublisherName,
                ProductCode = this.ProductCode,
                ContentType = this.ContentType,
                IpAddress = this.IpAddress,
            };
        }
    }
}
