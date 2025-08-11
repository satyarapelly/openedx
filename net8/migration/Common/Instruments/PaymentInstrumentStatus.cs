// <copyright file="PaymentInstrumentStatus.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Instruments
{
    using Microsoft.Commerce.Payments.Common.Web;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Payment Instrument status
    /// </summary>
    [JsonConverter(typeof(EnumJsonConverter))]
    public enum PaymentInstrumentStatus
    {
        /// <summary>
        /// indicate the payment instrument is in a unknown status
        /// </summary>
        Unknown,

        /// <summary>
        /// indicate the payment instrument is in a declined status, only available in Subscription database.
        /// </summary>
        Declined,

        /// <summary>
        /// the normal payment instrument status
        /// </summary>
        Active,

        /// <summary>
        /// status when addition of payment instrument is waiting to be completed
        /// </summary>
        Pending,

        /// <summary>
        /// payment instrument was removed
        /// </summary>
        Removed,

        /// <summary>
        /// payment instrument been completely removed after been not used for 18 months.
        /// </summary>
        Deactivated,

        /// <summary>
        /// payment instrument is banned
        /// </summary>
        Banned,
    }
}
