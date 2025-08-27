// <copyright file="PaymentSessionRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PayerAuthService
{
    using Newtonsoft.Json;

    /// <summary>
    /// This is a model per the PayerAuth.V3 API.  
    /// This object is sent as payload to PayerAuth.V3's POST /paymentSession API.
    /// </summary>
    public class PaymentSessionRequest
    {
        public PaymentSessionRequest(PaymentSessionData paymentSessionData)
        {
            this.PaymentSessionData = paymentSessionData;
        }

        [JsonProperty(PropertyName = "payment_session_data")]
        public PaymentSessionData PaymentSessionData { get; set; }
    }
}