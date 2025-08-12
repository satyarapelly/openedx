// <copyright file="PaymentMethodData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PXInternal
{
    using Newtonsoft.Json;

    /// <summary>
    /// This is a model used by PXService internally to extract google pay payment data
    /// </summary>
    public class PaymentMethodData
    {
        public PaymentMethodData()
        {
        }

        /// <summary>
        /// Gets or sets type - PaymentMethod type selected in the Google Pay payment sheet.
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string PaymentMethodType { get; set; }

        /// <summary>
        /// Gets or sets description - User-facing message to describe the payment method that funds this transaction.
        /// </summary>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets tokenizationData - Payment tokenization data for the selected payment method.
        /// </summary>
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets info - The value of this property depends on the payment method type returned. For CARD, see CardInfo.
        /// </summary>
        [JsonProperty(PropertyName = "info")]
        public CardInfo Info { get; set; }
    }
}