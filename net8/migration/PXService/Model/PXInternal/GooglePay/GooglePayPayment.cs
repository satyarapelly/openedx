// <copyright file="GooglePayPayment.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PXInternal
{
    using Newtonsoft.Json;
    using static Microsoft.Commerce.Payments.Common.PaymentConstants;

    /// <summary>
    /// This is a model used by PXService internally to extract google pay payment data
    /// </summary>
    public class GooglePayPayment
    {
        public GooglePayPayment()
        {
        }

        /// <summary>
        /// Gets or sets apiVersion - Major API version. The value in the response matches the value provided in PaymentDataRequest.
        /// </summary>
        [JsonProperty(PropertyName = "apiVersion")]
        public int ApiVersion { get; set; }

        /// <summary>
        /// Gets or sets apiVersionMinor - Minor API version. The value in the response matches the value provided in PaymentDataRequest.
        /// </summary>
        [JsonProperty(PropertyName = "apiVersionMinor")]
        public int ApiVersionMinor { get; set; }

        /// <summary>
        /// Gets or sets paymentMethodData - Data about the selected payment method.
        /// </summary>
        [JsonProperty(PropertyName = "paymentMethodData")]
        public PaymentMethodData PaymentMethodData { get; set; }

        /// <summary>
        /// Gets or sets email - Email address, if emailRequired is set to true in the PaymentDataRequest. If another request has the property set to true there's no effect.
        /// </summary>
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets shippingContact - Shipping address, if shippingAddressRequired is set to true in the PaymentDataRequest.
        /// </summary>
        [JsonProperty(PropertyName = "shippingAddress")]
        public Address ShippingAddress { get; set; }
    }
}