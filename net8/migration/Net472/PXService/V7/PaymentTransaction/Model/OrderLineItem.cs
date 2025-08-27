// <copyright file="OrderLineItem.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentTransaction.Model
{
    using Newtonsoft.Json;
    using D365 = PXService.Model.D365Service;
    using Purchase = PXService.Model.PurchaseService;

    public class OrderLineItem
    {
        public OrderLineItem()
        {
        }

        public OrderLineItem(Purchase.OrderLineItem purchaseItem)
        {
            this.OrderLineItemId = purchaseItem.LineItemId;
            this.Description = purchaseItem.Title;
            this.ProductId = purchaseItem.ProductId;
            this.ProductType = purchaseItem.ProductType;
            this.Currency = purchaseItem.CurrencyCode;
            this.Quantity = purchaseItem.Quantity;
            this.SkuId = purchaseItem.SkuId;
            this.Tax = purchaseItem.TaxAmount;
            this.TotalAmount = purchaseItem.TotalAmount;
        }

        public OrderLineItem(D365.OrderLineItem d365Item, string currencyCode)
        {
            // The required fields in OrderLineItem in D365 are:
            // LineItemId, Quantity, IsTaxIncluded and TaxGroup.
            this.OrderLineItemId = d365Item.LineItemId;
            this.Description = d365Item.Catalog?.Title;
            this.ProductId = d365Item.Catalog?.ProductId;
            this.ProductType = d365Item.Catalog?.ProductType.ToString();
            this.Currency = currencyCode;
            this.Quantity = (int)d365Item.Quantity;
            this.SkuId = d365Item.Catalog?.SkuId;
            this.Tax = (double)d365Item.PurchaseChargeDetails?.TotalTaxAmount;
            this.TotalAmount = (double)d365Item.PurchaseChargeDetails?.TotalAmount;
        }

        [JsonProperty("orderLineItemId")]
        public string OrderLineItemId { get; set; }

        [JsonProperty("productType")]
        public string ProductType { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("skuId")]
        public string SkuId { get; set; }

        [JsonProperty("productId")]
        public string ProductId { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("totalAmount")]
        public double TotalAmount { get; set; }

        [JsonProperty("tax")]
        public double Tax { get; set; }
    }
}