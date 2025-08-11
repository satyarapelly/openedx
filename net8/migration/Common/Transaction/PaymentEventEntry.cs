// <copyright file="PaymentEventEntry.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Transaction
{
    using System;

    public class PaymentEventEntry
    {
        public long Offset { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public PaymentEvent PaymentEvent { get; set; }
    }
}