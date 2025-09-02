// <copyright file="TransactionType.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Transaction
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Transaction type enumerates the different type transaction entries that can be posted to a Payment Transaction.
    /// In the EPA v3 pipeline, we explicitly followed same rule which applied to the PayMod transactions.
    /// For non-transactional types, we followed similar concept – if the nature of the activity is money inflow, the amount is +ve 
    /// whose sign represents whether it is inflow. Otherwise, it is –ve which means sign represents whether it is outflow.
    /// For example, the charge is an inflow activity and ‘+’ means it is inflow and ‘-‘ means it is outflow. For refund it is the opposite.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TransactionType
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
        /// Flow direction: +ve
        /// </summary>
        Charge,

        /// <summary>
        /// Indicates that the transaction type is a chargeback, when customer disputes a settled transaction.
        /// Flow direction: -ve
        /// </summary>
        Chargeback,

        /// <summary>
        /// Indicates that the transaction is reversal of a previous chargeback, when we successfully dispute customer's chargeback.
        /// Flow direction: +ve
        /// </summary>
        ReverseChargeback,

        /// <summary>
        /// Indicates that the transaction is a credit to customer
        /// Flow direction: -ve
        /// </summary>
        Credit,

        /// <summary>
        /// Indicates that the transaction is a refund against a previously settled transaction
        /// Flow direction: -ve
        /// </summary>
        Refund,

        /// <summary>
        /// Indicates that the transaction is a settle request against a previous authorization
        /// Flow direction: +ve
        /// </summary>
        Settle,

        /// <summary>
        /// Indicates a new transaction that results in reducing the overall transaction amount
        /// to be collected - used on transactions that have partial settlements.
        /// </summary>
        Reduce,

        /// <summary>
        /// Indicates that the transaction type is an token auth transaction forbidden settlement.
        /// </summary>
        Validate,
    }
}
