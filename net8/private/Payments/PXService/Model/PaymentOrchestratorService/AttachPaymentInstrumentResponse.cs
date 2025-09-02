// <copyright file="AttachPaymentInstrumentResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using Newtonsoft.Json;

    /// <summary>
    /// Response model for attaching a payment instrument.
    /// </summary>
    public class AttachPaymentInstrumentResponse
    {
        [JsonProperty(PropertyName = "requestId")]
        public string RequestId { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
    }
}