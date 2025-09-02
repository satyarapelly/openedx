// <copyright file="RiskServiceResponsePaymentInstrument.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Tests.Common.Model.RiskService
{
    using Newtonsoft.Json;

    public class RiskServiceResponsePaymentInstrument : RiskServiceRequestPaymentInstrument
    {
        [JsonProperty(PropertyName = "allowed")]
        public bool Allowed { get; set; }
    }
}
