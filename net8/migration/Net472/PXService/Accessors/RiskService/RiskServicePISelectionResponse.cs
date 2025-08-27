// <copyright file="RiskServicePISelectionResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.RiskService.V7
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class RiskServicePISelectionResponse
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:Collection properties should be read only", Justification = "Needs to be writeable so the JSON serializer can run")]
        [JsonProperty(PropertyName = "payment_info")]
        public IList<RiskServicePaymentInformation> PaymentInfo { get; set; }
    }
}