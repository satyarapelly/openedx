// <copyright file="RiskEligibilityEventDetails.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.RiskService
{
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PXService.RiskService.V7;
    using Newtonsoft.Json;

    public class RiskEligibilityEventDetails
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:Collection properties should be read only", Justification = "Needs to be writeable so the JSON serializer can run")]
        [JsonProperty(PropertyName = "account_details")]
        public RiskEligibilityAccountDetails AccountDetails { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:Collection properties should be read only", Justification = "Needs to be writeable so the JSON serializer can run")]
        [JsonProperty(PropertyName = "device_details")]
        public DeviceDetails DeviceDetails { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:Collection properties should be read only", Justification = "Needs to be writeable so the JSON serializer can run")]
        [JsonProperty(PropertyName = "payment_instrument_types")]
        public IList<RiskServiceRequestPaymentInstrument> PaymentInstrumentType { get; set; }

        [JsonProperty(PropertyName = "client")]
        public string Client { get; set; }
    }
}