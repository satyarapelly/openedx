// <copyright file="Cart.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.D365Service
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class Cart
    {
        [JsonProperty("isRequiredAmountPaid")]
        public bool? IsRequiredAmountPaid { get; set; }

        [JsonProperty("isDiscountFullyCalculated")]
        public bool? IsDiscountFullyCalculated { get; set; }

        [JsonProperty("ignoreDiscountCalculation")]
        public bool? IgnoreDiscountCalculation { get; set; }

        [JsonProperty("amountDue")]
        public decimal? AmountDue { get; set; }

        [JsonProperty("amountPaid")]
        public decimal? AmountPaid { get; set; }

        [JsonProperty("beginDateTime")]
        public string BeginDateTime { get; set; }

        [JsonProperty("businessDate")]
        public string BusinessDate { get; set; }

        [JsonProperty("cancellationChargeAmount")]
        public decimal? CancellationChargeAmount { get; set; }

        [JsonProperty("estimatedShippingAmount")]
        public decimal? EstimatedShippingAmount { get; set; }

        [JsonProperty("cartLines")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public List<CartLines> CartLines { get; set; }

        [JsonProperty("cartTypeValue")]
        public int? CartTypeValue { get; set; }

        [JsonProperty("chargeAmount")]
        public decimal? ChargeAmount { get; set; }

        [JsonProperty("customerOrderRemainingBalance")]
        public decimal? CustomerOrderRemainingBalance { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("invoiceComment")]
        public string InvoiceComment { get; set; }

        [JsonProperty("customerId")]
        public string CustomerId { get; set; }

        [JsonProperty("customerOrderModeValue")]
        public int? CustomerOrderModeValue { get; set; }

        [JsonProperty("deliveryMode")]
        public string DeliveryMode { get; set; }

        [JsonProperty("deliveryModeChargeAmount")]
        public decimal? DeliveryModeChargeAmount { get; set; }

        [JsonProperty("discountAmount")]
        public decimal? DiscountAmount { get; set; }

        [JsonProperty("discountAmountWithoutTax")]
        public decimal? DiscountAmountWithoutTax { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("suspendedCartId")]
        public string SuspendedCartId { get; set; }

        [JsonProperty("transactionTypeValue")]
        public int? TransactionTypeValue { get; set; }

        [JsonProperty("incomeExpenseTotalAmount")]
        public decimal? IncomeExpenseTotalAmount { get; set; }

        [JsonProperty("isReturnByReceipt")]
        public bool? IsReturnByReceipt { get; set; }

        [JsonProperty("returnTransactionHasLoyaltyPayment")]
        public bool? ReturnTransactionHasLoyaltyPayment { get; set; }

        [JsonProperty("isFavorite")]
        public bool? IsFavorite { get; set; }

        [JsonProperty("isRecurring")]
        public bool? IsRecurring { get; set; }

        [JsonProperty("isSuspended")]
        public bool? IsSuspended { get; set; }

        [JsonProperty("loyaltyCardId")]
        public string LoyaltyCardId { get; set; }

        [JsonProperty("modifiedDateTime")]
        public string ModifiedDateTime { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("orderNumber")]
        public string OrderNumber { get; set; }

        [JsonProperty("availableDepositAmount")]
        public decimal? AvailableDepositAmount { get; set; }

        [JsonProperty("overriddenDepositAmount")]
        public decimal? OverriddenDepositAmount { get; set; }

        [JsonProperty("overriddenDepositWithoutCarryoutAmount")]
        public decimal? OverriddenDepositWithoutCarryoutAmount { get; set; }

        [JsonProperty("prepaymentAmountPaid")]
        public decimal? PrepaymentAmountPaid { get; set; }

        [JsonProperty("prepaymentAppliedOnPickup")]
        public decimal? PrepaymentAppliedOnPickup { get; set; }

        [JsonProperty("quotationExpiryDate")]
        public string QuotationExpiryDate { get; set; }

        [JsonProperty("receiptEmail")]
        public string ReceiptEmail { get; set; }

        [JsonProperty("requestedDeliveryDate")]
        public string RequestedDeliveryDate { get; set; }

        [JsonProperty("requiredDepositAmount")]
        public decimal? RequiredDepositAmount { get; set; }

        [JsonProperty("requiredDepositWithoutCarryoutAmount")]
        public decimal? RequiredDepositWithoutCarryoutAmount { get; set; }

        [JsonProperty("salesId")]
        public string SalesId { get; set; }

        [JsonProperty("shippingAddress")]
        public ShippingAddress ShippingAddress { get; set; }

        [JsonProperty("staffId")]
        public string StaffId { get; set; }

        [JsonProperty("subtotalAmount")]
        public decimal? SubtotalAmount { get; set; }

        [JsonProperty("subtotalAmountWithoutTax")]
        public decimal? SubtotalAmountWithoutTax { get; set; }

        [JsonProperty("netPrice")]
        public decimal? NetPrice { get; set; }

        [JsonProperty("subtotalSalesAmount")]
        public decimal? SubtotalSalesAmount { get; set; }

        [JsonProperty("taxAmount")]
        public decimal? TaxAmount { get; set; }

        [JsonProperty("taxOnCancellationCharge")]
        public decimal? TaxOnCancellationCharge { get; set; }

        [JsonProperty("taxOverrideCode")]
        public string TaxOverrideCode { get; set; }

        [JsonProperty("terminalId")]
        public string TerminalId { get; set; }

        [JsonProperty("totalAmount")]
        public decimal? TotalAmount { get; set; }

        [JsonProperty("totalSalesAmount")]
        public decimal? TotalSalesAmount { get; set; }

        [JsonProperty("totalReturnAmount")]
        public decimal? TotalReturnAmount { get; set; }

        [JsonProperty("totalCarryoutSalesAmount")]
        public decimal? TotalCarryoutSalesAmount { get; set; }

        [JsonProperty("totalCustomerOrderSalesAmount")]
        public decimal? TotalCustomerOrderSalesAmount { get; set; }

        [JsonProperty("totalManualDiscountAmount")]
        public decimal? TotalManualDiscountAmount { get; set; }

        [JsonProperty("totalManualDiscountPercentage")]
        public decimal? TotalManualDiscountPercentage { get; set; }

        [JsonProperty("warehouseId")]
        public string WarehouseId { get; set; }

        [JsonProperty("isCreatedOffline")]
        public bool? IsCreatedOffline { get; set; }

        [JsonProperty("cartStatusValue")]
        public int? CartStatusValue { get; set; }

        [JsonProperty("receiptTransactionTypeValue")]
        public int? ReceiptTransactionTypeValue { get; set; }

        [JsonProperty("commissionSalesGroup")]
        public string CommissionSalesGroup { get; set; }

        [JsonProperty("version")]
        public int? Version { get; set; }

        [JsonProperty("totalItems")]
        public decimal? TotalItems { get; set; }

        [JsonProperty("hasTaxCalculationTriggered")]
        public bool? HasTaxCalculationTriggered { get; set; }

        [JsonProperty("hasChargeCalculationTriggered")]
        public bool? HasChargeCalculationTriggered { get; set; }

        [JsonProperty("shippingChargeAmount")]
        public decimal? ShippingChargeAmount { get; set; }

        [JsonProperty("otherChargeAmount")]
        public decimal? OtherChargeAmount { get; set; }
    }
}