// <copyright file="CreditCardType.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    using Microsoft.Commerce.Payments.Common.Web;
    using Newtonsoft.Json;

    [JsonConverter(typeof(EnumJsonConverter))]
    public enum CreditCardType
    {
        None,
        Visa,
        Master,
        Amex,
        Discover,
        Jcb,
        Diners,
        Klcc,
        Elo,
        Hipercard,
        UnionPay,
        Maestro,
        Electron,
        CarteBancaire,
        Shinhan,
        BC,
        KbKookMin,
        Samsung,
        Hyundai,
        Lotte,
        NH,
        Hana,
        CITI,
        Jeju,
        Woori,
        Suhyup,
        Jeonbok,
        Kwangju,
        Shinhyup,
        Others,
    }
}
