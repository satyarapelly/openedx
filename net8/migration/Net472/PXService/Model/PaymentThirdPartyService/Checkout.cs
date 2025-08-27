// <copyright file="Checkout.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentThirdPartyService
{
    using Newtonsoft.Json;

    public class Checkout
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "status")]
        public CheckoutStatus Status { get; set; }

        [JsonProperty(PropertyName = "payerId")]
        public string PayerId { get; set; }

        [JsonProperty(PropertyName = "paymentRequestId")]
        public string PaymentRequestId { get; set; }

        // the url that the user will be redirected-to (in iFrame) for the challenge
        [JsonProperty(PropertyName = "redirectUrl")]
        public string RedirectUrl { get; set; }

        // the url that the provider/bank calls after the challenge is complete
        [JsonProperty(PropertyName = "returnUrl")]
        public string ReturnUrl { get; set; }
    }
}