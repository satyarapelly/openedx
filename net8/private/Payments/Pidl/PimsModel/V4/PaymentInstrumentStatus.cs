// <copyright file="PaymentInstrumentStatus.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    /// <summary>
    /// Payment Instrument status
    /// </summary>
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

        /// <summary>
        /// payment instrument is cancelled
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Cancelled", Justification = "Based on UK english.")]
        Cancelled,
    }
}