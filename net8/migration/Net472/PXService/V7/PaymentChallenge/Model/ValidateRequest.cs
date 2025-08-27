// <copyright file="ValidateRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.WalletService
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Commerce.Payments.Common.Transaction;
    using Microsoft.Commerce.Payments.PXService.V7;
    using Newtonsoft.Json;

    public class ValidateRequest
    {
        [JsonProperty(PropertyName = "sessionId")]
        public string SessionId { get; set; }

        [JsonProperty(PropertyName = "accountId")]
        public string AccountId { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "piFamily")]
        public string PiFamily { get; set; }

        [JsonProperty(PropertyName = "piType")]
        public string PiType { get; set; }

        [JsonProperty(PropertyName = "riskData")]
        public RiskData RiskData { get; set; }

        [JsonProperty(PropertyName = "partner")]
        public string Partner { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "isCommercialTransaction")]
        public bool IsCommercialTransaction { get; set; }

        [JsonProperty(PropertyName = "authenticationData")]
        public AuthenticationData AuthenticationData { get; set; }

        [JsonProperty(PropertyName = "updateValidation")]
        public bool UpdateValidation { get; set; }
    }
}