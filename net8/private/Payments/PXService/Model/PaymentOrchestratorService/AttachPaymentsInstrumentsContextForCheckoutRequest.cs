// <copyright file="AttachPaymentsInstrumentsContextForCheckoutRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using Newtonsoft.Json;

    /// <summary>
    /// Payload for attaching payment instruments
    /// </summary>
    public class AttachPaymentsInstrumentsContextForCheckoutRequest : AttachPaymentsInstrumentsContext
    {
        [JsonProperty(PropertyName = "usageType")]
        public string UsageType { get; set; }
    }
}