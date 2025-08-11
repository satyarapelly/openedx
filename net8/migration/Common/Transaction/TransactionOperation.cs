// <copyright file="TransactionOperation.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Transaction
{
    /// <summary>
    /// Types of operations that can be performed in payments. Mostly maps 1-1 to transaction type, except reverse, which is an operation that can be
    /// performend on any transaction
    /// </summary>
    public enum TransactionOperation
    {
        /// <summary>
        /// Indicates an unknown transaction type.
        /// </summary>
        None,

        /// <summary>
        /// Indicates that the transaction type is an authorization transaction for later settlement.
        /// </summary>
        Authorize,

        /// <summary>
        /// Indicates that the transaction type is a charge, authorization and settlement are combined.
        /// </summary>
        Charge,

        /// <summary>
        /// Indicates that the transaction type is a chargeback, when customer disputes a settled transaction.
        /// </summary>
        Chargeback,

        /// <summary>
        /// Indicates that the transaction is reversal of a previous chargeback, when we successfully dispute customer's chargeback.
        /// </summary>
        ReverseChargeback,

        /// <summary>
        /// Indicates that the transaction is a credit to customer
        /// </summary>
        Credit,

        /// <summary>
        /// Indicates that the transaction is a refund against a previously settled transaction
        /// </summary>
        Refund,

        /// <summary>
        /// Indicates a reversal operation on a payment transaction 
        /// </summary>
        Reverse,

        /// <summary>
        /// Indicates that the transaction is a settle request against a previous authorization
        /// </summary>
        Settle,

        /// <summary>
        /// Indicates a transaction amount is being reduced - used on transactions that have
        /// partial settlements
        /// </summary>
        Reduce,

        Validate
    }
}
