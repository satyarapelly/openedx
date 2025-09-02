// <copyright file="InstrumentHelper.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.Common.Tracing;

    public static class InstrumentHelper
    {
        private static Dictionary<string, CreditCardType> cardTypeMap = new Dictionary<string, CreditCardType>()
        {
            { "VISA",           CreditCardType.Visa },
            { "MC",             CreditCardType.Master },
            { "DISCOVER",       CreditCardType.Discover },
            { "JCB",            CreditCardType.Jcb },
            { "AMEX",           CreditCardType.Amex },
            { "DINERS",         CreditCardType.Diners },
            { "KLCC",           CreditCardType.Klcc },
            { "ELO",            CreditCardType.Elo },
            { "HIPERCARD",      CreditCardType.Hipercard },
            { "UNIONPAY",       CreditCardType.UnionPay },
            { "UNION_PAY",      CreditCardType.UnionPay },
            { "MAESTRO",        CreditCardType.Maestro },
            { "ELECTRON",       CreditCardType.Electron },
            { "CARTEBANCAIRE",  CreditCardType.CarteBancaire },
            { "CARTE_BANCAIRE", CreditCardType.CarteBancaire },
            { "SHINHAN",        CreditCardType.Shinhan },
            { "BC",             CreditCardType.BC },
            { "KB_KOOK_MIN",    CreditCardType.KbKookMin },
            { "SAMSUNG",        CreditCardType.Samsung },
            { "HYUNDAI",        CreditCardType.Hyundai },
            { "LOTTE",          CreditCardType.Lotte },
            { "NH",             CreditCardType.NH },
            { "HANA",           CreditCardType.Hana },
            { "CITI",           CreditCardType.CITI },
            { "JEJU",           CreditCardType.Jeju },
            { "WOORI",          CreditCardType.Woori },
            { "SUHYUP",         CreditCardType.Suhyup },
            { "JEONBOK",        CreditCardType.Jeonbok },
            { "KWANGJU",        CreditCardType.Kwangju },
            { "SHINHYUP",       CreditCardType.Shinhyup }
        };

        /// <summary>
        /// Gets display text for each card type - Used for CaaS
        /// </summary>
        public static IDictionary<string, string> CardTypeDisplayMap
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    { CreditCardType.Visa.ToString().ToLower(),     "Visa" },
                    { CreditCardType.Master.ToString().ToLower(),   "Mastercard" },
                    { CreditCardType.Discover.ToString().ToLower(), "Discover" },
                    { CreditCardType.Jcb.ToString().ToLower(),      "Jcb" },
                    { CreditCardType.Amex.ToString().ToLower(),     "Amex" }
                };
            }
        }

        public static PaymentInstrumentOwnerType GetOwnerType(short ownerTypeId, EventTraceActivity traceActivityId)
        {
            switch (ownerTypeId)
            {
                case 1:
                    return PaymentInstrumentOwnerType.Passport;
                case 2:
                    return PaymentInstrumentOwnerType.ScsAccount;
                case 3:
                    return PaymentInstrumentOwnerType.AdCenter;
                case 4:
                    return PaymentInstrumentOwnerType.AdCenterCustomerId;
                case 5:
                    return PaymentInstrumentOwnerType.OrgPuid;
                case 9:
                    return PaymentInstrumentOwnerType.AnonymousId;
                default:
                    throw TraceCore.TraceException<InvalidOperationException>(traceActivityId, new InvalidOperationException(string.Format("'{0}' is not a known owner type id", ownerTypeId)));
            }
        }

        public static PaymentInstrumentStatus GetStatus(int statusId, EventTraceActivity traceActivityId)
        {   
            switch (statusId)
            {
                case 1:
                    return PaymentInstrumentStatus.Active;
                case 2:
                    return PaymentInstrumentStatus.Declined;
                case 5:
                    return PaymentInstrumentStatus.Deactivated;
                case 6:
                case 7: // Nuked PI will be shown as REMOVED state
                    return PaymentInstrumentStatus.Removed;
                case 8:
                    return PaymentInstrumentStatus.Pending;
                default:
                    throw TraceCore.TraceException<InvalidOperationException>(traceActivityId, new InvalidOperationException(string.Format("'{0}' is an unknown PaymentInstrument status", statusId)));
            }
        }

        public static CreditCardType GetCardType(string cardType, EventTraceActivity traceActivityId)
        {
            if (string.IsNullOrEmpty(cardType))
            {
                throw TraceCore.TraceException(traceActivityId, new NotSupportedException("credit card type should be specified."));
            }

            CreditCardType creditCardType;
            if (cardTypeMap.TryGetValue(cardType.ToUpper(), out creditCardType))
            {
                return creditCardType;
            }
            else
            {
                throw TraceCore.TraceException(traceActivityId, new NotSupportedException(string.Format("'{0}' credit card type is not supported.", cardType)));
            }
        }

        public static CreditCardType GetCardType(int cardType, EventTraceActivity traceActivityId)
        {
            switch (cardType)
            {
                case 1:
                    return CreditCardType.Visa;
                case 2:
                    return CreditCardType.Master;
                case 4:
                    return CreditCardType.Discover;
                case 5:
                    return CreditCardType.Jcb;
                case 3:
                    return CreditCardType.Amex;
                case 6:
                    return CreditCardType.Diners;
                case 7:
                    return CreditCardType.Klcc;
                case 8:
                    return CreditCardType.Elo;
                case 9:
                    return CreditCardType.Hipercard;
                case 11:
                    return CreditCardType.UnionPay;
                case 12:
                    return CreditCardType.Maestro;
                case 13:
                    return CreditCardType.Electron;
                case 14:
                    return CreditCardType.CarteBancaire;
                default:
                    throw TraceCore.TraceException(traceActivityId, new NotSupportedException(string.Format("'{0}' credit card type is not supported.", cardType)));
            }
        }

        public static byte GetCardTypeId(CreditCardType cardType)
        {
            switch (cardType)
            {
                case CreditCardType.Visa:
                    return 1;
                case CreditCardType.Master:
                    return 2;
                case CreditCardType.Amex:
                    return 3;
                case CreditCardType.Discover:
                    return 4;
                case CreditCardType.Jcb:
                    return 5;
                case CreditCardType.Diners:
                    return 6;
                case CreditCardType.Klcc:
                    return 7;
                case CreditCardType.Elo:
                    return 8;
                case CreditCardType.Hipercard:
                    return 9;
                case CreditCardType.UnionPay:
                    return 11;
                case CreditCardType.Maestro:
                    return 12;
                case CreditCardType.Electron:
                    return 13;
                case CreditCardType.CarteBancaire:
                    return 14;
                default:
                    throw new NotSupportedException(string.Format("'{0}' credit card type is not supported.", cardType));
            }
        }

        public static DirectDebitType GetDirectDebitType(string directDebitType, EventTraceActivity traceActivityId)
        {
            if (string.IsNullOrEmpty(directDebitType))
            {
                throw TraceCore.TraceException(traceActivityId, new NotSupportedException("direct debit type should be specified."));
            }

            switch (directDebitType.ToUpper())
            {
                case "SEPA":
                    return DirectDebitType.SEPA;
                case "ACH":
                    return DirectDebitType.ACH;
                default:
                    throw TraceCore.TraceException(traceActivityId, new NotSupportedException(string.Format("'{0}' direct debit type is not supported.", directDebitType)));
            }
        }
    }
}
