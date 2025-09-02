// <copyright file="RiskServiceRequestPaymentInstrument.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Tests.Common.Model.RiskService
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
