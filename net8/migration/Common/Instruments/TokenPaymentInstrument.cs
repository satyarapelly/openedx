// <copyright file="TokenPaymentInstrument.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    public class TokenPaymentInstrument : PaymentInstrument
    {
        public TokenPaymentInstrument()
            : base(PaymentMethodRegistry.Token)
        {
        }
    }
}
