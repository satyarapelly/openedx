// <copyright file="PaymentMethodType.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum PaymentMethodType
    {
        // Credit card types
        Visa,
        Mc,
        Amex,
        Discover,
        Jcb,
        Unionpay,
        Diners,

        // Ewallet
        ApplePay,
        GooglePay
    }
}