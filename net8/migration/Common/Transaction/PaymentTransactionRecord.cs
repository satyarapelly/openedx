// <copyright file="PaymentTransactionRecord.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Transaction
{
    using System;
    using Microsoft.Commerce.Payments.Common.Transaction;

    public class PaymentTransactionRecord
    {
        public string PaymentId { get; set; }

        public string AccountId { get; set; }

        public DateTime CreatedDatetime { get; set; }
        
        // Gets or sets the tracking GUID associated for this transaction. This field is not intended
        // to be consumed by services / external entities other than the transaction coordinator.
        // The purpose of this property is to help identify idempotent tranactions and we do
        // so by setting this property during read operations of records from the payment store.
        public Guid TrackingId { get; set; }

        public PaymentTransaction PaymentTransaction { get; set; }

        public int Version { get; set; }
    }
}
