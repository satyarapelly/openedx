// <copyright file="TransactionLink.cs" company="Microsoft Corporation">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using Newtonsoft.Json;

    /// <summary>
    /// This is the object appended to extendedView of Credit Cards.
    /// Returns the most recent LinkedPaymentSessionId for 1PP Guest checkout.
    /// </summary>
    public class TransactionLink
    {        
        /// <summary>
        /// Gets or sets transaction data id stored in Payment Transaction Data service (Z session) for Guest Checkout.
        /// </summary>
        [JsonProperty(PropertyName = "linkedPaymentSessionId")]
        public string LinkedPaymentSessionId { get; set; }
    }
}