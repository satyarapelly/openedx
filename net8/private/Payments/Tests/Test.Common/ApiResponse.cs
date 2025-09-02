// <copyright file="ApiResponse.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Test.Common
{
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class ApiResponse
    {
        [JsonProperty(PropertyName = "statusCode")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public HttpStatusCode StatusCode { get; set; }

        [JsonProperty(PropertyName = "content")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public JToken Content { get; set; }
    }
}