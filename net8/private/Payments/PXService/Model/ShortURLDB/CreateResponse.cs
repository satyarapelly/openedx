// <copyright file="CreateResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.ShortURLDB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using Newtonsoft.Json;

    public class CreateResponse
    {
        [JsonProperty(PropertyName = "expirationTime")]
        public DateTime ExpirationTime { get; set; }

        [JsonProperty(PropertyName = "uri")]
        public Uri Uri { get; set; }

        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }
    }
}