// <copyright file="ApplePayPayment.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PXInternal
{
    using Newtonsoft.Json;

    /// <summary>
    /// This is a model used by PXService internally to extract apple pay payment data
    /// </summary>
    public class ApplePayPayment
    {
        public ApplePayPayment()
        {
        }

        /// <summary>
        /// Gets or sets token - The encrypted information for an authorized payment.
        /// </summary>
        [JsonProperty(PropertyName = "token")]
        public ApplePayPaymentToken Token { get; set; }

        /// <summary>
        /// Gets or sets billingContact - The billing contact selected by the user for this transaction.
        /// </summary>
        [JsonProperty(PropertyName = "billingContact")]
        public ApplePayPaymentContact BillingContact { get; set; }

        /// <summary>
        /// Gets or sets shippingContact - The shipping contact selected by the user for this transaction.
        /// </summary>
        [JsonProperty(PropertyName = "shippingContact")]
        public ApplePayPaymentContact ShippingContact { get; set; }
    }
}