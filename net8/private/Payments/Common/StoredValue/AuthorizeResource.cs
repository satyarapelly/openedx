// <copyright file="AuthorizeResource.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.StoredValue
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// DebitResource is a copy of stored value core resource
    /// </summary>
    public class AuthorizeResource
    {
        [JsonProperty("lot_id")]
        public int LotId { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("external_reference_id")]
        public ExternalReference ExternalReferenceId { get; set; }
    }
}