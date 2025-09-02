// <copyright file="CodeEntry.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.ShortURLDB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using Newtonsoft.Json;

    public class CodeEntry
    {
        [JsonProperty(PropertyName = "id")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "tenantId")]
        public string TenantID { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string URL { get; set; }

        [JsonProperty(PropertyName = "hits")]
        public int Hits { get; set; }

        [JsonProperty(PropertyName = "requestedTTLMinutes")]
        public int RequestedTTLMinutes { get; set; }

        [JsonProperty(PropertyName = "creationTime")]
        public DateTime CreationTime { get; set; }

        [JsonProperty(PropertyName = "expireTime")]
        public DateTime ExpireTime { get; set; }

        [JsonProperty(PropertyName = "completedTime")]
        public DateTime CompletedTime { get; set; }

        [JsonProperty(PropertyName = "markedForDeletion")]
        public bool MarkedForDeletion { get; set; }

        [JsonProperty(PropertyName = "ttl", NullValueHandling = NullValueHandling.Ignore)]
        public int? TTL { get; set; }
    }
}