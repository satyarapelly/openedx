// <copyright file="ApplePayPaymentMethodType.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PXInternal
{
    using System.Text.Json.Serialization;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// This is a model used by PXService internally to extract apple pay payment data
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ApplePayPaymentMethodType
    {
        debit,
        credit,
        prepaid,
        store
    }
}