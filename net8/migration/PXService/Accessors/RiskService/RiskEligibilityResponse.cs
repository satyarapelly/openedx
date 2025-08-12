// <copyright file="RiskEligibilityResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.RiskService.V7
{
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PXService.Accessors.RiskService;
    using Newtonsoft.Json;
    
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