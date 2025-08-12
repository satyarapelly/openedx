// <copyright file="EvaluationResult.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.FraudDetectionService
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class EvaluationResult
    {
        [JsonPropertyName("activityId")]
        public string ActivityId { get; set; }

        [JsonPropertyName("paymentInstrumentIds")]
        public List<string> PaymentInstrumentIds { get; }

        [JsonPropertyName("riskScore")]
        public decimal? RiskScore { get; set; }

        [JsonPropertyName("recommendation")]
        public string Recommendation { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }
    }
}