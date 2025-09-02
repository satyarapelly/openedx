// <copyright file="IdealBillingAgreementPaymentInstrument.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    using Newtonsoft.Json;

    public class IDealBillingAgreementPaymentInstrument : PaymentInstrument
    {
        public IDealBillingAgreementPaymentInstrument()
            : base(PaymentMethodRegistry.IDealBillingAgreement)
        {
        }

        [JsonProperty(PropertyName = "payer_email")]
        public string PayerEmail { get; set; }

        [JsonProperty(PropertyName = "issuer_id")]
        public string IssuerId { get; set; }

        [JsonProperty(PropertyName = "locale")]
        public string Locale { get; set; }
    }
}
