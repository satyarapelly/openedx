// <copyright file="PaymentInstrument.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using Newtonsoft.Json;

    /// <summary>
    /// Attach payment request response payment instrument info model
    /// </summary>
    public class PaymentInstrument
    {
        [JsonProperty(PropertyName = "paymentInstrumentId")]
        public string PaymentInstrumentId { get; set; }

        [JsonProperty(PropertyName = "usage")]
        public PaymentInstrumentUsage Usage { get; set; }

        [JsonProperty(PropertyName = "paymentMethodType")]
        public PaymentMethodType PaymentMethodType { get; set; }
    }
}