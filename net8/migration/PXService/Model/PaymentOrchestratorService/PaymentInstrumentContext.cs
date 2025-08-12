// <copyright file="PaymentInstrumentContext.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{    
    using Newtonsoft.Json;

    /// <summary>
    /// Payment instrument context for the attached payment instrument
    /// </summary>
    public class PaymentInstrumentContext
    {
        [JsonProperty(PropertyName = "paymentInstrumentId")]
        public string PaymentInstrumentId { get; set; }

        [JsonProperty(PropertyName = "usage")]
        public PaymentInstrumentUsage Usage { get; set; }

        [JsonProperty(PropertyName = "transactionData")]
        public TransactionData TransactionData { get; set; }

        [JsonProperty(PropertyName = "actionAfterInitialTransaction")]
        public PaymentInstrumentActionType ActionAfterInitialTransaction { get; set; }
    }
}