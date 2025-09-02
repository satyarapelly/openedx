// <copyright file="BotdetectionRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.FraudDetectionService
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class BotdetectionRequest
    {
        [JsonPropertyName("greenId")]
        public string GreenId { get; set; }

        [JsonPropertyName("ipAddress")]
        public string IpAddress { get; set; }

        [JsonPropertyName("isChallengeResolved")]
        public bool IsChallengeResolved { get; set; }
    }
}