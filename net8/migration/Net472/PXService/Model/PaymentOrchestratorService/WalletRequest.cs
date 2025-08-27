// <copyright file="WalletRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{    
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    /// <summary>
    /// Attach payment instrument response model for wallet
    /// </summary>
    public class WalletRequest
    {
        [Required]
        [JsonProperty(PropertyName = "walletRequestId")]
        public string WalletRequestId { get; set; }

        [JsonProperty(PropertyName = "clientAccessToken")]
        public string ClientAccessToken { get; set; }

        [Required]
        [JsonProperty(PropertyName = "status")]
        public WalletRequestStatus Status { get; set; }

        [JsonProperty(PropertyName = "paymentInstruments")]
        [SuppressMessage("Microsoft.Performance", "CA1811", Justification = "Needed for getting a response from the payments orchestrator service")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<PaymentInstrument> PaymentInstruments { get; set; }

        [JsonProperty(PropertyName = "metadata")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}