// <copyright file="BankAccountPaymentInstrument.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    using System.Collections.Generic;

    public class BankAccountPaymentInstrument : PaymentInstrument
    {
        public const string DDBankCountryCodePropertyName = "DDBankCountryCode";
        public const string AddedDateTimePropertyName = "AddedDateTime";
        public const string BankSWIFTCodePropertyName = "BankSWIFTCode";
        public const string SupportCurrencyIsoListPropertyName = "SupportCurrencyIsoList";

        public BankAccountPaymentInstrument()
            : base(PaymentMethodRegistry.BankAccountPay)
        {
        }

        public BankAccountPaymentInstrument(Dictionary<string, string> propertiers)
            : base(PaymentMethodRegistry.BankAccountPay, propertiers)
        {
            string swiftCode;
            if (this.Properties.TryGetValue(BankSWIFTCodePropertyName, out swiftCode))
            {
                this.BankSwiftCode = swiftCode;
            }
        }

        public string AccountHolderName { get; set; }

        public string CompanyName { get; set; }

        public AccountType AccountType { get; set; }

        public AccountOwnerType AccountOwnerType { get; set; }

        public string BankSwiftCode { get; set; }

        public string BankName { get; set; }

        public string BankCode { get; set; }

        public string BranchCode { get; set; }

        public AddressInfo Address { get; set; }

        public PhoneInfo Phone { get; set; }

        public AddressInfo BankAddress { get; set; }
    }
}
