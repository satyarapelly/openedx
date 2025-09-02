// <copyright file="TransactionResult.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Transaction
{
    /// <summary>
    /// Represents the result of a transaction after an attempted operation by the provider.
    /// Failed: Indicates transaction had failed. More details on failure can be found by inspecting the failure message contained in the transaction.
    /// Pending: Indicates transaction status is pending an outcome.
    /// Success: Indicates transaction successfully completed.
    /// </summary>
    public enum TransactionResult
    {
        /// <summary>
        /// Indicates the default state of a newly constructed payment transaction
        /// </summary>
        None,

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
        /// Indicates that the transaction has been reversed.
        /// </summary>
        Reversed,

        /// <summary>
        /// Indicates a partial fullfillment of a transaction - i.e. we have collected
        /// partial amount(s) towards the transaction but it's not fully settled as yet
        /// </summary>
        PartialApproved,

        OfflineApproved
    }
}
