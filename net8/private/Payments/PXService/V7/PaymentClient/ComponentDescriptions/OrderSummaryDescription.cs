// <copyright file="OrderSummaryDescription.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Microsoft.Commerce.Payments.PidlModel.V7;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService;

    public class OrderSummaryDescription : ComponentDescription
    {
        private string descriptionType = V7.Constants.DescriptionTypes.Checkout;
        private string descriptionId = "orderSummary";

        public static IReadOnlyList<string> OrderSummaryDataDescriptions
        {
            get
            {
                return new List<string>()
                {
                    V7.Constants.PropertyDescriptionIds.CartTax,
                    V7.Constants.PropertyDescriptionIds.CartSubtotal,
                    V7.Constants.PropertyDescriptionIds.CartTotal
                };
            }
        }

        public static IReadOnlyList<string> OrderSummaryDisplayDescriptions
        {
            get
            {
                return new List<string>()
                {
                    V7.Constants.DisplayHintIds.CartTax,
                    V7.Constants.DisplayHintIds.CartSubtotal,
                    V7.Constants.DisplayHintIds.CartTotal
                };
            }
        }

        public override string DescriptionType
        {
            get
            {
                return this.descriptionType;
            }
        }

        public override Task<List<PIDLResource>> GetDescription()
        {
            List<PIDLResource> retVal = PIDLResourceFactory.Instance.GetPaymentClientDescriptions(this.descriptionType, this.Country, this.descriptionId, this.Operation, this.Language, this.Partner, exposedFlightFeatures: this.ExposedFlightFeatures?.ToList(), scenario: this.Scenario, setting: this.PSSSetting);

            // Default values
            decimal defaultTaxAmount = UsePaymentRequestApiEnabled() ? this.PaymentRequestClientActions?.TaxAmount ?? 0 : this.CheckoutRequestClientActions?.TaxAmount ?? 0;
            decimal defaultTotalAmount = UsePaymentRequestApiEnabled() ? this.PaymentRequestClientActions?.Amount ?? 0 : this.CheckoutRequestClientActions?.Amount ?? 0;
            decimal defaultSubTotalAmount = UsePaymentRequestApiEnabled() ? this.PaymentRequestClientActions?.SubTotalAmount ?? 0 : this.CheckoutRequestClientActions?.SubTotalAmount ?? 0;

            if (this.ActivePaymentInstruments?.Count > 0)
            {
                // Use tax amount from PI level and calculate total.
                defaultTaxAmount = this.ActivePaymentInstruments[0].PaymentInstrumentDetails?.TaxAmount ?? 0;
                defaultTotalAmount = this.ActivePaymentInstruments[0].PaymentInstrumentDetails?.TaxAmount ?? 0 + defaultSubTotalAmount;
            }

            IDictionary<string, object> cartAmounts = new Dictionary<string, object>()
            {
                { V7.Constants.PropertyDescriptionIds.CartTax, this.ConvertToCurrency(defaultTaxAmount) },
                { V7.Constants.PropertyDescriptionIds.CartTotal, this.ConvertToCurrency(defaultTotalAmount) },
                { V7.Constants.PropertyDescriptionIds.CartSubtotal, this.ConvertToCurrency(defaultSubTotalAmount) }
            };

            ComponentDescription.SetDefaultValues(retVal, cartAmounts, false);

            UpdateOrderLineItems(retVal, this.UsePaymentRequestApiEnabled() ? this.PaymentRequestClientActions?.LineItems?.First() : this.CheckoutRequestClientActions?.LineItems?.First()); 

            return Task.FromResult(retVal);
        }

        private static void UpdateOrderLineItems(List<PIDLResource> retVal, OrderLineItem lineItem)
        {
            retVal.ForEach(pidl =>
            {
                ImageDisplayHint catalogImage = pidl.GetDisplayHintById(V7.Constants.DisplayHintIds.OrderItemImage) as ImageDisplayHint;
                HeadingDisplayHint cartHeading = pidl.GetDisplayHintById(V7.Constants.DisplayHintIds.CartHeading) as HeadingDisplayHint; 

                if (lineItem != null)
                {
                    if (cartHeading != null)
                    {
                        cartHeading.DisplayContent = lineItem.ItemName;
                    }

                    if (catalogImage != null) 
                    { 
                        catalogImage.SourceUrl = lineItem.ImageUri;
                    }
                }
            });
        }

        private string ConvertToCurrency(decimal amount)
        {
            if (amount == 0)
            {
                string currencySymbol = CurrencyHelper.GetCurrencySymbol(this.Country, this.Language, this.Currency);
                return string.Format("{0}--", currencySymbol);
            }
            else
            {
                // Format the amount to a string with locale
                return CurrencyHelper.FormatCurrency(this.Country, this.Language, amount, this.Currency);
            }
        }
    }
}