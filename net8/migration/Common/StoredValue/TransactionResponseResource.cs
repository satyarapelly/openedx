// <copyright file="TransactionResponseResource.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.StoredValue
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// TransactionResponseResource is a copy of stored value core resource
    /// </summary>
    public class TransactionResponseResource
    {
        [JsonProperty("transaction_id")]
        public Guid TransactionId { get; set; }
    }
}