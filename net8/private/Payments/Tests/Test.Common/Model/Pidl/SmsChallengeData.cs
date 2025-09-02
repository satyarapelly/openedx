// <copyright file="SmsChallengeData.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using Newtonsoft.Json;

    public class SmsChallengeData
    {
        public SmsChallengeData()
        {
        }

        public SmsChallengeData(string paymentInstrumentId, string sessionId)
        {
            this.PaymentInstrumentId = paymentInstrumentId;
            this.SessionId = sessionId;
        }

        [JsonProperty(PropertyName = "payment_instrument_id")]
        public string PaymentInstrumentId { get; set; }

        [JsonProperty(PropertyName = "session_id")]
        public string SessionId { get; set; }
    }
}
