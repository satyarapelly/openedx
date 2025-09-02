// <copyright file="PaymentRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{    
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    /// <summary>
    /// Attach payment instrument response model for payment request
    /// </summary>
    public class PaymentRequest
    {
        [Required]
        [JsonProperty(PropertyName = "paymentRequestId")]
        public string PaymentRequestId { get; set; }

        [JsonProperty(PropertyName = "clientAccessToken")]
        public string ClientAccessToken { get; set; }

        [Required]
        [JsonProperty(PropertyName = "status")]
        public PaymentRequestStatus Status { get; set; }

        [JsonProperty(PropertyName = "paymentInstruments")]
        [SuppressMessage("Microsoft.Performance", "CA1811", Justification = "Needed for getting a response from the payments orchestrator service")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<PaymentInstrument> PaymentInstruments { get; set; }

        [JsonProperty(PropertyName = "metadata")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}