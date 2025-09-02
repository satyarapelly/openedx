// <copyright file="TransactionStatus.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Transaction
{
    using Microsoft.Commerce.Payments.Common.Web;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the transaction status of a payment transaction.
    /// Failed: Indicates transaction had failed. More details on failure can be found by inspecting the failure message contained in the transaction.
    /// Pending: Indicates transaction status is pending an outcome.
    /// Success: Indicates transaction successfully completed.
    /// </summary>
    [JsonConverter(typeof(EnumJsonConverter))]
    public enum TransactionStatus
    {
        /// <summary>
        /// Indicates the default state of a newly constructed payment transaction
        /// </summary>
        None,

        /// <summary>
        /// Indicates the transaction is in progress
        /// </summary>
        InProgress,

        /// <summary>
        /// Indicates the transaction is in progress, and is pending a challenge response
        /// </summary>
        ChallengeResponsePending,

        /// <summary>
        /// Indicates the transaction completed but is in an unknown state, meaning its result is in doubt.
        /// </summary>
        Unknown,

        /// <summary>
        /// Indicates that the transaction failed, therefore can be safely reattempted if so desired
        /// </summary>
        Failed,

        /// <summary>
        /// Indicates that the transaction has successfully completed, and the outcome is denied.
        /// </summary>
        Declined,

        /// <summary>
        /// Indicates that the transaction successfully completed and is pending its outcome.
        /// </summary>
        Pending,

        /// <summary>
        /// Indicates that the transaction successfully completed and the outcome is a success.
        /// </summary>
        Approved,

        /// <summary>
        /// Indicates that the transation reversal is in progress.
        /// </summary>
        ReverseInProgress,

        /// <summary>
        /// Indicates that the transaction reversal request outcome is unknown
        /// </summary>
        ReverseUnknown,

        /// <summary>
        /// Indicates that the transaction reversal failed - couldnt send request to provider
        /// </summary>
        ReverseFailed,

        /// <summary>
        /// Indicates that the transaction reversal got declined by the provider
        /// </summary>
        ReverseDeclined,

        /// <summary>
        /// Indicate that the transaciton reversal request is pending
        /// </summary>
        ReversePending,

        /// <summary>
        /// Indicates that the transaction that was previously successfully completed had since been reversed
        /// </summary>
        Reversed,

        /// <summary>
        /// Indicates a partial fullfillment of a transaction - i.e. we have collected
        /// partial amount(s) towards the transaction but it's not fully settled as yet
        /// </summary>
        PartialApproved,
        
        /// <summary>
        /// Indicates that the transaction that was previously failed or declined but later proved to be succeeded based on EPA or other offline sources
        /// </summary>
        OfflineApproved
    }
}
