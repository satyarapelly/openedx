// <copyright file="ValidatePaymentInstrument.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using Newtonsoft.Json;

    public class ValidatePaymentInstrument
    {
        public ValidatePaymentInstrument(
            string sessionId,
            object riskData)
        {
            this.SessionId = sessionId;
            this.RiskData = riskData;
        }

        [JsonProperty(PropertyName = "sessionId")]
        public string SessionId { get; set; }

        [JsonProperty(PropertyName = "riskData")]
        public object RiskData { get; set; }
    }
}