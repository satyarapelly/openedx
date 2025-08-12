// <copyright file="PaymentInstrument.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentThirdPartyService
{
    using Microsoft.Commerce.Payments.PidlFactory.V7;
    using Newtonsoft.Json;

    public class PaymentInstrument
    {
        [JsonProperty(PropertyName = "paymentMethodFamily")]
        public string PaymentMethodFamily { get; set; }

        [JsonProperty(PropertyName = "paymentMethodType")]
        public string PaymentMethodType { get; set; }

        [JsonProperty(PropertyName = "context")]
        public string Context { get; set; }

        [JsonProperty(PropertyName = "details")]
        public PaymentInstrumentDetails PaymentInstrumentDetails { get; set; }
    }
}