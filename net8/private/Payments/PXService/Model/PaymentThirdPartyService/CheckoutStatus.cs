// <copyright file="CheckoutStatus.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentThirdPartyService
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum CheckoutStatus
    {
        Created,
        Paid,
        Failed,
        Invalid,
    }
}