// <copyright file="ExternalReference.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.StoredValue
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// ExternalReference is a copy of stored value core resource
    /// </summary>
    public class ExternalReference
    {
        public ExternalReference(Guid paymentTransactionId, string merchantReferenceNumber)
        {
            this.PaymentTransactionId = paymentTransactionId;
            this.MerchantReferenceNumber = merchantReferenceNumber;
        }

        [JsonProperty("payment_transaction_id")]
        public Guid PaymentTransactionId { get; set; }

        [JsonProperty("merchantReferenceNumber")]
        public string MerchantReferenceNumber { get; set; }
    }
}