// <copyright file="ExpressCheckoutOptions.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PXInternal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    /// <summary>
    /// This is a model used by PXService internally to extract express checkout request data
    /// </summary>
    public class ExpressCheckoutOptions
    {
        /// <summary>
        /// Gets or sets Corner radius of express checkout buttons
        /// </summary>
        [JsonProperty(PropertyName = "cornerRadius")]
        public string CornerRadius { get; set; }

        /// <summary>
        /// Gets or sets buttonColor of express checkout buttons
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]

        [JsonProperty(PropertyName = "buttonColor")]
        public Dictionary<string, string> ButtonColor { get; set; }

        /// <summary>
        /// Gets or sets buttonType of express checkout buttons
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        [JsonProperty(PropertyName = "buttonType")]
        public Dictionary<string, string> ButtonType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether requireShippingAddress option for express checkout
        /// </summary>
        [JsonProperty(PropertyName = "requireShippingAddress")]
        public bool RequireShippingAddress { get; set; }
    }
}