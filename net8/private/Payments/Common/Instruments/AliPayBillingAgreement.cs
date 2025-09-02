// <copyright file="AliPayBillingAgreement.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    using Newtonsoft.Json;

    public class AliPayBillingAgreement : PaymentInstrument
    {
        public AliPayBillingAgreement()
            : base(PaymentMethodRegistry.AliPayBillingAgreement)
        {
        }

        [JsonProperty(PropertyName = "billing_agreement_id")]
        public string BillingAgreementId { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public string UserId { get; set; }
    }
}
