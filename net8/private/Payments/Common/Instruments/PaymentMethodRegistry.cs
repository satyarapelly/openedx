// <copyright file="PaymentMethodRegistry.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    public class PaymentMethodRegistry
    {
        static PaymentMethodRegistry()
        {
            AliPay = new PaymentMethod(Names.AliPay);
            AliPayBillingAgreement = new PaymentMethod(Names.AliPayBillingAgreement);
            BabelFish = new PaymentMethod(Names.BabelFish);
            Check = new PaymentMethod(Names.Check);
            EnterpriseAgreementCheck = new PaymentMethod(Names.EnterpriseAgreementCheck);
            BankAccountPay = new PaymentMethod(Names.BankAccountPay);
            BitPay = new PaymentMethod(Names.BitPay);
            Boku = new PaymentMethod(Names.Boku);
            Boleto = new PaymentMethod(Names.Boleto);
            CreditCard = new PaymentMethod(Names.CreditCard);
            DirectDebit = new PaymentMethod(Names.DirectDebit);
            Giropay = new PaymentMethod(Names.Giropay);
            IDeal = new PaymentMethod(Names.IDeal);
            Inicis = new PaymentMethod(Names.Inicis);
            Klarna = new PaymentMethod(Names.Klarna);
            MobileCarrierBilling = new PaymentMethod(Names.MobileCarrierBilling);
            Nordea = new PaymentMethod(Names.Nordea);
            PayByPhone = new PaymentMethod(Names.PayByPhone);
            PayPal = new PaymentMethod(Names.PayPal);
            PayPalPayout = new PaymentMethod(Names.PayPalPayout);
            Sofort = new PaymentMethod(Names.Sofort);
            Paytrail = new PaymentMethod(Names.Paytrail);
            Eps = new PaymentMethod(Names.Eps);
            Trustly = new PaymentMethod(Names.Trustly);
            Poli = new PaymentMethod(Names.Poli);
            Dotpay = new PaymentMethod(Names.Dotpay);
            StoredValue = new PaymentMethod(Names.StoredValue);
            UnionPay = new PaymentMethod(Names.UnionPay);
            WebMoney = new PaymentMethod(Names.WebMoney);
            Yandex = new PaymentMethod(Names.Yandex);
            IDealBillingAgreement = new PaymentMethod(Names.IDealBillingAgreement);
            TvPay = new PaymentMethod(Names.TvPay);
            Token = new PaymentMethod(Names.Token);
            MonetaryCommitment = new PaymentMethod(Names.MonetaryCommitment);
        }

        public static PaymentMethod AliPay { get; private set; }

        public static PaymentMethod AliPayBillingAgreement { get; private set; }

        public static PaymentMethod BabelFish { get; private set; }

        public static PaymentMethod BankAccountPay { get; private set; }

        public static PaymentMethod BitPay { get; private set; }

        public static PaymentMethod Boku { get; private set; }

        public static PaymentMethod Boleto { get; private set; }

        public static PaymentMethod Check { get; private set; }

        public static PaymentMethod EnterpriseAgreementCheck { get; private set; }

        public static PaymentMethod CreditCard { get; private set; }

        public static PaymentMethod DirectDebit { get; private set; }

        public static PaymentMethod Giropay { get; private set; }

        public static PaymentMethod IDeal { get; private set; }

        public static PaymentMethod Klarna { get; private set; }

        public static PaymentMethod Inicis { get; private set; }

        public static PaymentMethod MobileCarrierBilling { get; private set; }

        public static PaymentMethod Nordea { get; private set; }

        public static PaymentMethod PayByPhone { get; private set; }

        public static PaymentMethod PayPal { get; private set; }

        public static PaymentMethod PayPalPayout { get; private set; }

        public static PaymentMethod Sofort { get; private set; }

        public static PaymentMethod Paytrail { get; private set; }

        public static PaymentMethod Eps { get; private set; }

        public static PaymentMethod Trustly { get; private set; }

        public static PaymentMethod Poli { get; private set; }

        public static PaymentMethod Dotpay { get; private set; }

        public static PaymentMethod StoredValue { get; private set; }

        public static PaymentMethod UnionPay { get; private set; }

        public static PaymentMethod WebMoney { get; private set; }

        public static PaymentMethod Yandex { get; private set; }

        public static PaymentMethod IDealBillingAgreement { get; private set; }

        public static PaymentMethod TvPay { get; private set; }

        public static PaymentMethod Token { get; private set; }

        public static PaymentMethod MonetaryCommitment { get; private set; }

        public static class Names
        {
            public const string AliPay = "AliPay";
            public const string AliPayBillingAgreement = "AliPayBillingAgreement";
            public const string BabelFish = "BabelFish";
            public const string BankAccountPay = "BankAccountPay";
            public const string BitPay = "BitPay";
            public const string Boku = "Boku";
            public const string Boleto = "Boleto";
            public const string Check = "Check";
            public const string EnterpriseAgreementCheck = "EACheck";
            public const string CreditCard = "CreditCard";
            public const string DirectDebit = "DirectDebit";
            public const string Giropay = "GiroPay";
            public const string IDeal = "IDeal";
            public const string Inicis = "Inicis";
            public const string Klarna = "Klarna";
            public const string MobileCarrierBilling = "MobileCarrierBilling";
            public const string Nordea = "Nordea";
            public const string PayByPhone = "PayByPhone";
            public const string PayPal = "PayPal";
            public const string PayPalPayout = "PayPalPayout";
            public const string Sofort = "Sofort";
            public const string Paytrail = "Paytrail";
            public const string Eps = "eps";
            public const string Trustly = "Trustly";
            public const string Poli = "POLI";
            public const string Dotpay = "DotPay";
            public const string StoredValue = "StoredValue";
            public const string UnionPay = "UnionPay";
            public const string WebMoney = "WebMoney";
            public const string Yandex = "Yandex";
            public const string IDealBillingAgreement = "IDealBillingAgreement";
            public const string TvPay = "TvPay";
            public const string Token = "Token";
            public const string MonetaryCommitment = "MonetaryCommitment";
        }
    }
}
