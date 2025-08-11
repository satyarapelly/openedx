// <copyright file="PayPalPayoutPaymentInstrument.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    using Newtonsoft.Json;

    public class PayPalPayoutPaymentInstrument : PaymentInstrument
    {
        public PayPalPayoutPaymentInstrument()
            : base(PaymentMethodRegistry.PayPalPayout)
        {
        }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }
    }
}
