// <copyright file="AuthorizationResponseResource.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.StoredValue
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// AuthorizationResponseResource is a copy of stored value core resource
    /// </summary>
    public class AuthorizationResponseResource
    {
        [JsonProperty("transaction_id")]
        public Guid TransactionId { get; set; }

        [JsonProperty("authorization_id")]
        public Guid AuthorizationId { get; set; }
    }
}