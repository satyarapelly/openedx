// <copyright file="CheckoutCharge.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentThirdPartyService
{
    using Newtonsoft.Json;

    // Since we don't want to expose PI information in the checkout object to our caller,
    // in additional to checkout, checkoutcharge is needed to hold the information.
    public class CheckoutCharge
    {
        [JsonProperty(PropertyName = "checkoutId")]
        public string CheckoutId { get; set; }

        [JsonProperty(PropertyName = "paymentInstrument")]
        public PaymentInstrument PaymentInstrument { get; set; }

        // It is the return url provider/bank calls, if verification (e.g. psd2)/charge(e.g. paypal) is successful.
        // If it fails, the failureUrl will be called.
        [JsonProperty(PropertyName = "returnUrl")]
        public string ReturnUrl { get; set; }

        // Email address to which the receipt for the charge would be sent
        [JsonProperty(PropertyName = "receiptEmailAddress")]
        public string ReceiptEmailAddress { get; set; }

        [JsonProperty(PropertyName = "failureUrl")]
        public string FailureUrl { get; set; }
    }
}