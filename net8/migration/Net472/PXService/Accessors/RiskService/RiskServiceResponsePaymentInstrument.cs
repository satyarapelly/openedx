// <copyright file="RiskServiceResponsePaymentInstrument.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.RiskService
{
    using Newtonsoft.Json;

    public class RiskServiceResponsePaymentInstrument : RiskServiceRequestPaymentInstrument
    {
        [JsonProperty(PropertyName = "allowed")]
        public bool Allowed { get; set; }
    }
}