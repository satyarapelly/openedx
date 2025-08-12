// <copyright file="TransactionData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using Microsoft.Commerce.Payments.Common.Transaction;
    using Newtonsoft.Json;

    /// <summary>
    /// Transaction data for the attached payment instrument
    /// </summary>
    public class TransactionData
    {
        [JsonProperty(PropertyName = "dataSchema")]
        public PaymentInstrumentChallengeType DataSchema { get; set; }

        [JsonProperty(PropertyName = "dataValue")]
        public object DataValue { get; set; }
    }
}