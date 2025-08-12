// <copyright file="RiskServiceRequestPaymentInstrument.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.RiskService
{
    using Newtonsoft.Json;

    public class RiskServiceRequestPaymentInstrument
    {
        [JsonProperty(PropertyName = "payment_instrument_family")]
        public string PaymentInstrumentFamily { get; set; }

        [JsonProperty(PropertyName = "payment_instrument_type")]
        public string PaymentInstrumentType { get; set; }
    }
}