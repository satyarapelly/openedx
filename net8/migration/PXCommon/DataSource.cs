// <copyright file="DataSource.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXCommon
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class DataSource
    {
        public DataSource()
        {
            this.Members = new List<object>();
        }

        public DataSource(List<object> members)
        {
            this.Members = members;
        }

        public DataSource(string href, string method, Dictionary<string, string> headers)
        {
            this.Href = href;
            this.Method = method;
            this.Headers = headers;
        }

        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }

        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "headers")]
        public Dictionary<string, string> Headers { get; }

        [JsonProperty(PropertyName = "members")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set for serialization")]
        public IList<object> Members { get; set; }

        [JsonProperty(PropertyName = "dataSourceConfig")]
        public DataSourceConfig DataSourceConfig { get; set; }
    }
}