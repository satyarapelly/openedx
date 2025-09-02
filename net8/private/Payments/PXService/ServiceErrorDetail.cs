// <copyright file="ServiceErrorDetail.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class ServiceErrorDetail
    {
        [JsonProperty(Order = 1)]
        public string ErrorCode { get; set; }

        [JsonProperty(Order = 2)]
        public string Target { get; set; }

        [JsonProperty(Order = 3)]
        public string Message { get; set; }

        [JsonProperty(Order = 5)]
        public IEnumerable<string> Details { get; set; }
    }
}