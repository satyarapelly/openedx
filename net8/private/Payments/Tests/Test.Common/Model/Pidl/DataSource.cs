// <copyright file="DataSource.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PXCommon;
    using Newtonsoft.Json;

    public class DataSource
    {
        public DataSource(List<object> members)
        {
            this.Members = members;
        }

        [JsonProperty(PropertyName = "href")]
        public string Href { get; set; }

        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "headers")]
        public Dictionary<string, string> Headers { get; set; }

        [JsonProperty(PropertyName = "members")]
        public IList<object> Members { get; }

        [JsonProperty(PropertyName = "dataSourceConfig")]
        public DataSourceConfig DataSourceConfig { get; set; }
    }
}
