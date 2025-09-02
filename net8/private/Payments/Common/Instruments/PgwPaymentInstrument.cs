// <copyright file="PgwPaymentInstrument.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    using Newtonsoft.Json;

    public class PgwPaymentInstrument : PaymentInstrument
    {
        public PgwPaymentInstrument(PaymentMethod paymentMethod)
            : base(paymentMethod)
        {
        }
    }
}
