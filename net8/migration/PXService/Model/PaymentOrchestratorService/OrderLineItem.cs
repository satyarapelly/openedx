// <copyright file="OrderLineItem.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;

    public class OrderLineItem
    {
        /// <summary>
        /// Gets or sets the description of the order line item.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the name of the order line item.
        /// </summary>
        [Required]
        public string ItemName { get; set; }

        /// <summary>
        /// Gets or sets the charge amount of the order line item.
        /// Billing comments of the field
        ///     Tax inclusive.  $10 charge amount, $1 tax, $10 total charge, this value should be $10.
        ///     Not tax inclusive.  $10 charge amount, $1 tax, $11 total charge, this value should be $10.
        /// </summary>
        [Required]
        public decimal ChargeAmount { get; set; }

        /// <summary>
        /// Gets or sets the tax amount of the order line item.
        /// </summary>
        [Required]
        public decimal Tax { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the charge amount is tax inclusive.
        /// </summary>
        [Required]
        public bool IsTaxInclusive { get; set; }

        /// <summary>
        /// Gets or sets the number of units of the order line item.
        /// </summary>
        [Required]
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the company or entity that published the product or service.
        /// </summary>
        [Required]
        public string PublisherName { get; set; }

        /// <summary>
        /// Gets or sets the product code of the order line item.
        /// </summary>
        [Required]
        public string ProductCode { get; set; }

        /// <summary>
        /// Gets or sets the category or format of the product or service.
        /// </summary>
        [Required]
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the address where the product will be delivered.
        /// </summary>
        public Address ShipToAddress { get; set; }

        /// <summary>
        /// Gets the information from the tenant provided for back reference purpose.
        /// NOTE: The information should NOT contain any personally identifiable information (PII) or sensitive information, as it will be logged and will not be scrubbed.
        /// </summary>
        public IDictionary<string, string> Metadata { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the image Uri.
        /// </summary>
        public string ImageUri { get; set; }
    }
}
