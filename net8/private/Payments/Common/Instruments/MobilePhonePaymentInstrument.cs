// <copyright file="MobilePhonePaymentInstrument.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    public class MobilePhonePaymentInstrument : PaymentInstrument
    {
        public MobilePhonePaymentInstrument()
            : base(PaymentMethodRegistry.Boku)
        {
        }

        public string MobilePhoneNumber { get; set; }

        public string MobileCarrier { get; set; }
    }
}
