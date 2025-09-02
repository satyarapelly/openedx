// <copyright file="TransactionConfirmationType.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Transaction
{
    using Microsoft.Commerce.Payments.Common.Web;
    using Newtonsoft.Json;

    [JsonConverter(typeof(EnumJsonConverter))]
    public enum TransactionConfirmationType
    {
        None,

        Instant,

        Delayed,

        Offline
    }
}