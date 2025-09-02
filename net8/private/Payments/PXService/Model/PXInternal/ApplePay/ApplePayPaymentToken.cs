// <copyright file="ApplePayPaymentToken.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PXInternal
{
    using Newtonsoft.Json;

    /// <summary>
    /// This is a model used by PXService internally to extract apple pay payment token
    /// </summary>
    public class ApplePayPaymentToken
    {
        public ApplePayPaymentToken()
        {
        }

        /// <summary>
        /// Gets or sets token - The encrypted information for an authorized payment.
        /// </summary>
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets paymentMethod - Information about the card used in the transaction.
        /// </summary>
        [JsonProperty(PropertyName = "paymentMethod")]
        public ApplePayPaymentMethod PaymentMethod { get; set; }

        /// <summary>
        /// Gets or sets transactionIdentifier - A unique identifier for this payment.
        /// </summary>
        [JsonProperty(PropertyName = "transactionIdentifier")]
        public string TransactionIdentifier { get; set; }
    }
}