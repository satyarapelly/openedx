// <copyright file="RiskEligibilityResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Tests.Common.Model.RiskService
{
    using Newtonsoft.Json;
    using System.Collections.Generic;
    
    public class RiskEligibilityResponse
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "event_type")]
        public string EventType { get; set; }

        [JsonProperty(PropertyName = "decision")]
        public string Decision { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:Collection properties should be read only", Justification = "Needs to be writeable so the JSON serializer can run")]
        [JsonProperty(PropertyName = "reasons")]
        public IList<string> Reasons { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:Collection properties should be read only", Justification = "Needs to be writeable so the JSON serializer can run")]
        [JsonProperty(PropertyName = "payment_instrument_types")]
        public IList<RiskServiceResponsePaymentInstrument> PaymentInstrumentTypes { get; set; }

        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }
    }
}
