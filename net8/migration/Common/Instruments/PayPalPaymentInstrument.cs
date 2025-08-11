// <copyright file="PayPalPaymentInstrument.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    using Newtonsoft.Json;

    public class PayPalPaymentInstrument : PaymentInstrument
    {
        public PayPalPaymentInstrument()
            : base(PaymentMethodRegistry.PayPal)
        {
        }

        [JsonProperty(PropertyName = "billing_agreement_id")]
        public string BillingAgreementId { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }
    }
}
