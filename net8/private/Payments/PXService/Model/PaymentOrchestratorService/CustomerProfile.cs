// <copyright file="CustomerProfile.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class CustomerProfile
    {
        /// <summary>
        /// Gets or sets the address of the customer who is purchasing the product or service.
        /// </summary>
        public Address SoldToAddress { get; set; }

        /// <summary>
        /// Gets or sets the Billing Address.
        /// </summary>
        public Address BillingAddress { get; set; }

        public List<AddressResult> Addresses { get; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        public string Email { get; set; }
    }
}