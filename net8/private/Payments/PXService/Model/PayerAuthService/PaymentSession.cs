// <copyright file="PaymentSession.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PayerAuthService
{
    using Newtonsoft.Json;
    using ThreeDSExternalService;
    using PXModel = V7.PaymentChallenge.Model;
    
    /// <summary>
    /// This is a model per the PayerAuth.V3 API.  
    /// This extends the PaymentSessionData model by adding an Id property to it.
    /// The Id is returned by PayerAuth.V3's POST /paymentSession API
    /// </summary>
    public class PaymentSession : PaymentSessionData
    {
        public PaymentSession()
        {
        }

        public PaymentSession(PaymentSession rhs) : base(rhs: rhs)
        {
            this.Id = rhs.Id;
        }

        public PaymentSession(
            PaymentSessionData data, 
            string id) : 
            base(
                rhs: data)
        {
            this.Id = id;
        }

        public PaymentSession(
            string accountId,
            string id, 
            PXModel.PaymentSessionData data, 
            PimsModel.V4.PaymentInstrument paymentInstrument, 
            DeviceChannel deviceChannel, 
            bool piRequiresAuthentication) : 
            base(
                accountId: accountId,
                data: data,
                paymentInstrument: paymentInstrument,
                deviceChannel: deviceChannel,
                piRequiresAuthentication: piRequiresAuthentication)
        {
            this.Id = id;
        }

        [JsonProperty(PropertyName = "payment_session_id")]
        public string Id { get; set; }
    }
}