// <copyright file="CheckoutRequestClientActions.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{    
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class CheckoutRequestClientActions
    {
        public string CheckoutRequestId { get; set; }

        public string PaymentRequestId { get; set; }

        public decimal Amount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal SubTotalAmount { get; set; }

        public string Currency { get; set; }

        public string Country { get; set; }

        public string Language { get; set; }

        public bool? PreOrder { get; set; }

        public IDictionary<string, string> Metadata { get; } = new Dictionary<string, string>();

        public IList<OrderLineItem> LineItems { get; } = new List<OrderLineItem>();

        public PaymentMethodResult PaymentMethodResults { get; set; }

        public CheckoutStatus CheckoutStatus { get; set; }

        public string PartnerName { get; set; }

        public CustomerProfile Profile { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public List<PaymentInstrument> PaymentInstruments { get; set; }

        /// <summary>
        /// Gets or sets the ClientActions - This is PO client action to handle challenge types
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Public setter used in serialization.")]
        public IList<ClientAction> ClientActions { get; set; }

        /// <summary>
        /// Gets or sets the ClientAction - This is PIDL client action to handle PIDL/User next action.
        /// </summary>
        [JsonProperty(PropertyName = "clientAction")]
        public PXCommon.ClientAction ClientAction { get; set; }

        /// <summary>
        /// Gets or sets Payment flow capabilities.
        /// </summary>
        public PaymentCapabilities Capabilities { get; set; }
    }
}