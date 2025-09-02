// <copyright file="DelayedPaymentInstrument.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    using System.Diagnostics;

    /// <summary>
    /// the delayed payment instrument can be used to depict the PI like AliPay,
    /// which are redirection-based payment instrument and didn't store any payment method specific 
    /// information 
    /// </summary>
    public class DelayedPaymentInstrument : PaymentInstrument
    {
        public DelayedPaymentInstrument(PaymentMethod paymentMethod)
            : base(paymentMethod)
        {
        }
    }
}
