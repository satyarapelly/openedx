// <copyright file="ThreeDSMethodRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PayerAuthService
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using PXModel = Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model;

    /// <summary>
    /// This is a model per the PayerAuth.V3 API.  
    /// This object is sent as payload to PayerAuth.V3's POST /authenticate API
    /// This model is equivalent to the V2's AReq model.
    /// </summary>
    public class ThreeDSMethodRequest
    {
        public ThreeDSMethodRequest(PaymentSession paymentSession)
        {
            this.PaymentSession = paymentSession;
        }

        [JsonProperty(PropertyName = "payment_session")]
        public PaymentSession PaymentSession { get; set; }
    }
}