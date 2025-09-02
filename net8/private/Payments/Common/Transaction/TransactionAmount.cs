// <copyright file="TransactionAmount.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Transaction
{
    /// <summary>
    /// Represents the transaction amount for a payment transaction.
    /// An amount is qualified by Currency and Country.
    /// </summary>
    public class TransactionAmount
    {
        /// <summary>
        /// Gets or sets the amount related to the transaction.
        /// </summary>
        /// <remarks>
        /// We are assuming that there will be a way to extract the exponent from here as needed.
        /// </remarks>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the ISO 4217 Code for Currency of payment amount.
        /// </summary>
        public string Currency { get; set; }

        public TransactionAmount Clone()
        {
            return new TransactionAmount
            {
                Amount = this.Amount,
                Currency = this.Currency,
            };
        }
    }
}
