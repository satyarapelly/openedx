// <copyright file="PaymentInstrumentSession.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// A session object for payment instrument
    /// This is used by Authentication Status api to validate a PSD2 session
    /// </summary>
    public class PaymentInstrumentSession
    {
        public PaymentInstrumentSession(
            string sessionId,
            string accountId,
            List<string> requiredChallenge)
        {
            this.SessionId = sessionId;
            this.AccountId = accountId;
            this.RequiredChallenge = requiredChallenge;
        }

        [JsonProperty(PropertyName = "sessionId", Required = Required.Always)]
        public string SessionId { get; set; }

        [JsonProperty(PropertyName = "accountId", Required = Required.Always)]
        public string AccountId { get; set; }

        [JsonProperty(PropertyName = "requiredChallenge")]
        public List<string> RequiredChallenge { get; }
    }
}
