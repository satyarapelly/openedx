// <copyright file="RecurringPaymentDetails.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PXInternal
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This is a model used by PXService internally to extract express checkout request data
    /// </summary>
    public class RecurringPaymentDetails
    {
        /// <summary>
        /// Gets or sets frequency unit
        /// </summary>
        [JsonProperty(PropertyName = "frequencyUnit")]
        public string FrequencyUnit { get; set; }

        /// <summary>
        /// Gets or sets frequency
        /// </summary>
        [JsonProperty(PropertyName = "frequency")]
        public int Frequency { get; set; }

        /// <summary>
        /// Gets or sets amount - From the cart/checkout
        /// </summary>
        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets label
        /// </summary>
        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets start time
        /// </summary>
        [JsonProperty(PropertyName = "startTime")]
        public DateTime StartTime { get; set; }
    }
}