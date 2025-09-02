// <copyright file="PaymentMethodOption.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PimsModel.V4;

    public class PaymentMethodOption
    {
        public PaymentMethodOption(string displayName, bool shouldBeGrouped, HashSet<PaymentMethod> paymentMethods, string displayId)
        {
            this.DisplayName = displayName;
            this.IsGroup = shouldBeGrouped;
            this.PaymentMethods = paymentMethods;
            this.DisplayId = displayId;
        }

        /// <summary>
        /// Gets or sets the display name of payment method option.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this payment method option should be grouped or not.
        /// </summary>
        public bool IsGroup { get; set; }

        /// <summary>
        /// Gets payment methods in the payment method option
        /// </summary>
        public HashSet<PaymentMethod> PaymentMethods { get; }

        /// <summary>
        /// Gets or sets the display id of payment method option.
        /// </summary>
        public string DisplayId { get; set; }
    }
}