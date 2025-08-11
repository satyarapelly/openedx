// <copyright file="AuthorizeReverseResource.cs" company="Microsoft">Copyright (c) Microsoft 2016. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.StoredValue
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Settle resource is a copy of the stored value core resource
    /// </summary>
    public class AuthorizeReverseResource
    {
        [JsonProperty("lot_id")]
        public int LotId { get; set; }

        [JsonProperty("authorization_id")]
        public Guid AuthorizationId { get; set; }

        [JsonProperty("external_reference_id")]
        public ExternalReference ExternalReferenceId { get; set; }
    }
}
