// <copyright file="PaymentSessionResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PayerAuthService
{
    using Newtonsoft.Json;

    /// <summary>
    /// This is a model per the PayerAuth.V3 API.  
    /// This object is sent as payload to PayerAuth.V3's POST /paymentSession API.
    /// </summary>
    public class PaymentSessionResponse
    {
        [JsonProperty(PropertyName = "payment_session_id")]
        public string PaymentSessionId { get; set; }
    }
}