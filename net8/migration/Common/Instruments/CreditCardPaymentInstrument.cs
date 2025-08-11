// <copyright file="CreditCardPaymentInstrument.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    using System;

    public class CreditCardPaymentInstrument : PaymentInstrument
    {
        public CreditCardPaymentInstrument()
            : base(PaymentMethodRegistry.CreditCard)
        {
        }

        public DateTime ExpirationDate { get; set; }

        public AddressInfo BillingAddress { get; set; }

        public CreditCardType CardType { get; set; }

        public string BankIdentificationNumber { get; set; }

        public string AccountHolderName { get; set; }

        public override string SubType
        {
            get
            {
                // This is temporary solution adapt PIMS and bizops configuration.
                if (this.CardType == CreditCardType.UnionPay || this.CardType == CreditCardType.Master)
                {
                    return this.CardType.ToString();
                }
                else
                {
                    return this.PaymentMethodType;
                }
            }
        }
    }
}
