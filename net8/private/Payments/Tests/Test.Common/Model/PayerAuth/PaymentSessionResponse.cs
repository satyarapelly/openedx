// <copyright file="PaymentSessionResponse.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.PayerAuth
{
    using Newtonsoft.Json;

    public class PaymentSessionResponse
    {
        [JsonProperty(PropertyName = "payment_session_id")]
        public string PaymentSessionId { get; set; }
    }
}
