// <copyright file="ApplePayPaymentMethod.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PXInternal
{
    using Newtonsoft.Json;

    /// <summary>
    /// This is a model used by PXService internally to extract apple pay payment token
    /// </summary>
    public class ApplePayPaymentMethod
    {
        public ApplePayPaymentMethod()
        {
        }

        /// <summary>
        /// Gets or sets displayName - suitable for display, that describes the card.
        /// </summary>
        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets network - suitable for display, that is the name of the payment network backing the card.
        /// </summary>
        [JsonProperty(PropertyName = "network")]
        public string Network { get; set; }

        /// <summary>
        /// Gets or sets type - The payment method type can be one of the following string values:"debit", "credit", "prepaid" and "store".
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public ApplePayPaymentMethodType PaymentMethodType { get; set; }

        /// <summary>
        /// Gets or sets paymentPass - The payment pass object currently selected to complete the payment.
        /// </summary>
        [JsonProperty(PropertyName = "paymentPass")]
        public ApplePayPaymentPass PaymentPass { get; set; }

        /// <summary>
        /// Gets or sets billingContact - The billing contact associated with the card.
        /// </summary>
        [JsonProperty(PropertyName = "billingContact")]
        public ApplePayPaymentContact BillingContact { get; set; }
    }
}