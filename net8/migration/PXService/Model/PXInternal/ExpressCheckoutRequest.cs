// <copyright file="ExpressCheckoutRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PXInternal
{
    using Newtonsoft.Json;

    /// <summary>
    /// This is a model used by PXService internally to extract express checkout request data
    /// </summary>
    public class ExpressCheckoutRequest
    {
        /// <summary>
        /// Gets or sets amount - From the cart/checkout
        /// </summary>
        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets currency code
        /// </summary>
        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets country code - ISO 3166-1 alpha-2 country code.
        /// </summary>
        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets language
        /// </summary>
        [JsonProperty(PropertyName = "language")]
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets top domain URL
        /// </summary>
        [JsonProperty(PropertyName = "topDomainUrl")]
        public string TopDomainUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the tax is included in the amount.
        /// </summary>
        [JsonProperty(PropertyName = "isTaxIncluded")]
        public bool IsTaxIncluded { get; set; }

        /// <summary>
        /// Gets or sets RecurringPaymentDetails will be needed for ApplePay transactions with subscriptions.
        /// </summary>
        [JsonProperty(PropertyName = "recurringPaymentDetails", NullValueHandling = NullValueHandling.Ignore)]
        public RecurringPaymentDetails RecurringPaymentDetails { get; set; }

        /// <summary>
        /// Gets or sets Options that control the look and feel of the express checkout/quick payment buttons.
        /// </summary>
        [JsonProperty(PropertyName = "options", NullValueHandling = NullValueHandling.Ignore)]
        public ExpressCheckoutOptions Options { get; set; }
    }
}