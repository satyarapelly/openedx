// <copyright file="CheckoutChargePayload.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentThirdPartyService
{
    using Newtonsoft.Json;

    public class CheckoutChargePayload
    {
        [JsonProperty(PropertyName = "paymentMethodFamily")]
        public string PaymentMethodFamily { get; set; }

        [JsonProperty(PropertyName = "paymentMethodType")]
        public string PaymentMethodType { get; set; }

        [JsonProperty(PropertyName = "context")]
        public string Context { get; set; }

        [JsonProperty(PropertyName = "details")]
        public PaymentInstrumentDetails PaymentInstrumentDetails { get; set; }

        [JsonProperty(PropertyName = "receiptEmailAddress")]
        public string ReceiptEmailAddress { get; set; }
    }
}