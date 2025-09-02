// <copyright file="PayByPhonePaymentInstrument.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    /// <summary>
    /// Payment Instrument for users that wish to pay using Carrier billing, but not necessarily through their WP device.
    /// Also known as Mobi-Non-Sim.
    /// </summary>
    public class PayByPhonePaymentInstrument : PaymentInstrument
    {
        public PayByPhonePaymentInstrument()
            : base(PaymentMethodRegistry.PayByPhone)
        {
        }

        /// <summary>
        /// Gets or sets the Payment Account token used for the transactions
        /// </summary>
        public string PaymentAccount { get; set; }

        /// <summary>
        /// Gets or sets the mobile operator ID. For example: att-us
        /// </summary>
        public string MobileOperatorId { get; set; }

        /// <summary>
        /// Gets or sets the MSISDN (Phone number) associated with this PI
        /// </summary>
        public string Msisdn { get; set; }
    }
}
