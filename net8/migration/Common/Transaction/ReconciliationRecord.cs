// <copyright file="ReconciliationRecord.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Transaction
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Reconciliation record is a statement of fact about a transaction from another source.
    /// If the reconciliation is about a transaction that has a payment amount associated with it, it includes
    /// amount.
    /// </summary>
    public class ReconciliationRecord
    {
        /// <summary>
        /// Gets or sets the timestamp when the reconciliation record was added.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the amount contained in the reconciliation statement.
        /// </summary>
        public TransactionAmount Amount { get; set; }

        /// <summary>
        /// Gets or sets the fees charged by the provider.
        /// </summary>
        public TransactionAmount FeeAmount { get; set; }

        /// <summary>
        /// Gets or sets the EPA filename that was the basis of this reconciliation statement
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the transaction that this belongs to, is reconciled or not.
        /// </summary>
        public bool Reconciled { get; set; }

        public ReconciliationRecord Clone()
        {
            return new ReconciliationRecord
            {
                Timestamp = this.Timestamp,
                Amount = this.Amount.Clone(),
                FeeAmount = this.FeeAmount.Clone(),
                Filename = this.Filename,
                Reconciled = this.Reconciled,
            };
        }
    }
}
