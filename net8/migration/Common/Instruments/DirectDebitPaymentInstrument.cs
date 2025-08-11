// <copyright file="DirectDebitPaymentInstrument.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    using System;

    public enum DirectDebitType
    {
        ACH,
        SEPA
    }

    public class DirectDebitPaymentInstrument : PaymentInstrument
    {
        public DirectDebitPaymentInstrument()
            : base(PaymentMethodRegistry.DirectDebit)
        {
        }

        public string BankName { get; set; }

        public string BankCode { get; set; }

        public string AccountHolderName { get; set; }

        public AccountType AccountType { get; set; }

        public DirectDebitType Subtype
        {
            get
            {
                return this.Properties.ContainsKey("IsSEPAPayin") && this.Properties["IsSEPAPayin"].ToLower() == "true"
                    ? DirectDebitType.SEPA
                    : DirectDebitType.ACH;
            }
        }

        public DateTime? MandateReceivedDate { get; set; }
        
        public string MandateReferenceNumber
        {
            get
            {
                string mandateReferenceNumber;
                return this.Properties.TryGetValue("MandateReferenceNumber", out mandateReferenceNumber) ? mandateReferenceNumber : null;
            }
        }

        //// SEPA PI have following keys in this.Properties:
        ////   MandateReferenceNumber
        ////   IsSEPAPayin
        //// Most of SEPA PI have:
        ////   EventId
        ////   AddedDateTime
        ////   SupportCurrencyIsoList
        ////   NextRetentionCheckDate
    }
}