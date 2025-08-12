// <copyright file="ValidateIncomingPayload.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.WalletService
{
    using Microsoft.Commerce.Payments.Common.Transaction;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model;
    using Newtonsoft.Json;

    public class ValidateIncomingPayload
    {
        [JsonProperty(PropertyName = "piFamily")]
        public string PiFamily { get; set; }

        [JsonProperty(PropertyName = "piType")]
        public string PiType { get; set; }

        [JsonProperty(PropertyName = "tokenReference")]
        public string TokenReference { get; set; }

        [JsonProperty(PropertyName = "sessionData")]
        public PaymentSessionData SessionData { get; set; }

        [JsonProperty(PropertyName = "isCommercialTransaction")]
        public bool IsCommercialTransaction { get; set; }

        [JsonProperty(PropertyName = "authenticationData")]
        public AuthenticationData AuthenticationData { get; set; }

        [JsonProperty(PropertyName = "updateValidation")]
        public bool UpdateValidation { get; set; }
    }
}