// <copyright file="PaymentRequestClientActions.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;

    public class PaymentRequestClientActions
    {
        /// <summary>
        /// Gets or sets the Payment request id.
        /// </summary>
        [Required]
        public string PaymentRequestId { get; set; }

        /// <summary>
        /// Gets or sets the The status of the payment request.
        /// </summary>
        [Required]
        public PaymentRequestStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the Country code of the merchant. The format is ISO 3166 (e.g. US, UK, DE).
        /// </summary>
        [Required]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the Currency used for payments. The format is ISO 4217 (e.g. USD, CNY, GBP).
        /// </summary>
        [Required]
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets the User's preferred language.
        /// Language code following the ISO 639-1 standard (e.g., en, fr, de)
        /// Or, with country code suffix following the ISO 3166 standard (e.g., en-US, fr-FR, de-DE)
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the The list of the payment instruments used for the payment request.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Needed for serialization purpose.")]
        public List<PaymentInstrument> PaymentInstruments { get; set; } = new List<PaymentInstrument>();

        /// <summary>
        /// Gets or sets the The amount of the payment request.
        /// </summary>
        [Required]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the tax amount.
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Gets or sets subtotal amount.
        /// </summary>
        public decimal SubTotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the Moto. Indicates whether this is a Moto (Mail Order/Telephone Order) transaction.
        /// The default value is false.
        /// </summary>
        public bool? Moto { get; set; }

        /// <summary>
        /// Gets or sets the PreOrder. Indicates whether this is a pre-order transaction.
        /// The default value is false.
        /// </summary>
        public bool? PreOrder { get; set; }

        /// <summary>
        /// Gets or sets the the eligible payment methods to use for this payment request.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Needed for serialization purpose.")]
        public IList<EligiblePaymentMethods> EligiblePaymentMethods { get; set; } = new List<EligiblePaymentMethods>();

        /// <summary>
        /// Gets or sets the the list of pending client actions.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Needed for serialization purpose.")]
        public IList<PaymentRequestClientAction> ClientActions { get; set; } = new List<PaymentRequestClientAction>();

        /// <summary>
        /// Gets or sets the response from the list of payment instruments stored in user's account
        /// </summary>
        public PaymentMethodResult PaymentMethodResults { get; set; } // response from list PI

        /// <summary>
        /// Gets or sets the capabilities.
        /// Payment flow capabilities.
        /// </summary>
        public PaymentCapabilities Capabilities { get; set; }

        /// <summary>
        /// Gets or sets the LineItems.
        /// The line items information.
        /// The line items are for informational purposes only and might be sent to the payment processor for display purposes.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Needed for serialization purpose.")]
        public List<OrderLineItem> LineItems { get; set; }

        /// <summary>
        /// Gets or sets the MerchantAccountProfile.
        /// The merchant account profile.
        /// </summary>
        public MerchantAccountProfile MerchantAccountProfile { get; set; }

        /// <summary>
        /// Gets or sets the Profile.
        /// </summary>
        public CustomerProfile Profile { get; set; }

        /// <summary>
        /// Gets or sets the ClientAction - This is PIDL client action to handle PIDL/User next action.
        /// </summary>
        [JsonProperty(PropertyName = "clientAction")]
        public PXCommon.ClientAction ClientAction { get; set; }
    }
}