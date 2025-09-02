// <copyright file="UnionPayPaymentInstrument.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    using System;
    using Newtonsoft.Json;

    public class UnionPayPaymentInstrument : PaymentInstrument
    {
        public UnionPayPaymentInstrument()
            : base(PaymentMethodRegistry.UnionPay)
        {
        }

        [JsonProperty(PropertyName = "token")]
        public string CardToken { get; set; }

        [JsonProperty(PropertyName = "expiration_date")]
        public DateTime ExpirationDate { get; set; }
    }
}
