// <copyright file="PIDLErrorDetail.cs" company="Microsoft">Copyright (c) Microsoft 2020. All rights reserved.</copyright>

namespace Tests.Common.Model
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class PIDLErrorDetail
    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "target")]
        public string Target { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "details")]
        public IEnumerable<string> Details { get; set; }
    }
}
