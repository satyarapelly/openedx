// <copyright file="Payment.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Transaction
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// <para>
    /// A payment represents a set of transaction ledger entries that pertain to a single payment.
    /// The payment is uniquely identified by a payment id. The payment id is provided by the caller or creator
    /// of the payment.
    /// </para>
    /// <para>
    /// The payment may contain many sub-transactions that pertain to the payment. 
    /// Example: A payment can have a single authorization and many settle transactions.
    /// Example: A payment may also include chargebacks, chargeback reverses.
    /// Example: A payment may also include refunds issued against a settled transaction.
    /// </para>
    /// </summary>
    public class Payment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Payment"/> class.
        /// </summary>
        public Payment()
        {
            this.PaymentTransactions = new List<PaymentTransaction>();
            this.ConfigurationContexts = new List<ConfigurationContext>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Payment"/> class.
        /// </summary>
        /// <param name="paymentId">A unique identifier for this payment</param>
        /// <param name="accountId">Account id on whose behalf this payment record is created</param>
        public Payment(string paymentId, string accountId)
            : this()
        {
            this.Id = paymentId;
            this.AccountId = accountId;
        }

        /// <summary>
        /// Gets or sets the payment identifier associated with this payment transaction.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the account id of this payment transaction's owner.
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// Gets individual transactions of different types that corresponds to this payment transaction.
        /// Examples: Authorize, Charge, Settle, Chargeback, ChargebackReverse, Refund etcetera
        /// </summary>
        public IList<PaymentTransaction> PaymentTransactions { get; private set; }

        /// <summary>
        /// Gets or sets the 3 letter ISO code for the Country of payment.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the ISO 4217 Code for Currency of payment amount.
        /// </summary>
        public string Currency { get; set; }

        public int MaxTransactionVersionPerPayment { get; set; }

        public TestContext TestContext { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<ConfigurationContext> ConfigurationContexts { get; set; }
    }
}
