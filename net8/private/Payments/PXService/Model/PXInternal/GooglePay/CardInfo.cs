// <copyright file="CardInfo.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PXInternal
{
    using Newtonsoft.Json;

    /// <summary>
    /// This is a model used by PXService internally to extract google pay payment data
    /// </summary>
    public class CardInfo
    {
        public CardInfo()
        {
        }

        /// <summary>
        /// Gets or sets cardDetails - The details about the card. This value is commonly the last four digits of the selected payment account number.
        /// </summary>
        [JsonProperty(PropertyName = "cardDetails")]
        public string CardDetails { get; set; }

        /// <summary>
        /// Gets or sets assuranceDetails - This object provides information about the validation performed on the returned payment data if assuranceDetailsRequired is set to true in the CardParameters.
        /// </summary>
        [JsonProperty(PropertyName = "assuranceDetails")]
        public AssuranceDetailsSpecifications AssuranceDetails { get; set; }

        /// <summary>
        /// Gets or sets cardNetwork - The payment card network of the selected payment. Returned values match the format of allowedCardNetworks in CardParameters.
        /// This card network value should not be displayed to the buyer. It's used when the details of a buyer's card are needed.
        /// For example, if customer support needs this value to identify the card a buyer used for their transaction. For a user-visible description, use the description property of PaymentMethodData instead.
        /// </summary>
        [JsonProperty(PropertyName = "cardNetwork")]
        public string CardNetwork { get; set; }

        /// <summary>
        /// Gets or sets billingAddress - The billing address associated with the provided payment method, if billingAddressRequired is set to true in CardParameters.
        /// </summary>
        [JsonProperty(PropertyName = "billingAddress")]
        public Address BillingAddress { get; set; }
    }
}