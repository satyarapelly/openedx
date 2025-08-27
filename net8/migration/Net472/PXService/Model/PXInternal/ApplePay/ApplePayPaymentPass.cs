// <copyright file="ApplePayPaymentPass.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PXInternal
{
    using Newtonsoft.Json;

    /// <summary>
    /// This is a model used by PXService internally to extract apple pay payment token
    /// </summary>
    public class ApplePayPaymentPass
    {
        public ApplePayPaymentPass()
        {
        }

        /// <summary>
        /// Gets or sets primaryAccountIdentifier - The unique identifier for the primary account number for the payment card.
        /// </summary>
        [JsonProperty(PropertyName = "primaryAccountIdentifier")]
        public string PrimaryAccountIdentifier { get; set; }

        /// <summary>
        /// Gets or sets primaryAccountNumberSuffix - A version of the primary account number suitable for display in your UI.
        /// </summary>
        [JsonProperty(PropertyName = "primaryAccountNumberSuffix")]
        public string PrimaryAccountNumberSuffix { get; set; }

        /// <summary>
        /// Gets or sets deviceAccountIdentifier - The unique identifier for the device-specific account number.
        /// </summary>
        [JsonProperty(PropertyName = "deviceAccountIdentifier")]
        public string DeviceAccountIdentifier { get; set; }

        /// <summary>
        /// Gets or sets deviceAccountNumberSuffix - A version of the device account number suitable for display in your UI.
        /// </summary>
        [JsonProperty(PropertyName = "deviceAccountNumberSuffix")]
        public string DeviceAccountNumberSuffix { get; set; }

        /// <summary>
        /// Gets or sets activationState - Payment pass activation states.
        /// </summary>
        [JsonProperty(PropertyName = "activationState")]
        public ApplePayPaymentPassActivationState ActivationState { get; set; }
    }
}