// <copyright file="PidlCacheKey.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Tests.Common.Model.Pidl
{
    using Newtonsoft.Json;

    public class PidlCacheKey
    {
        public PidlCacheKey()
        {
        }

        [JsonProperty(PropertyName = "operationType")]
        public string OperationType { get; set; }

        [JsonProperty(PropertyName = "resourceType")]
        public string ResourceType { get; set; }

        [JsonProperty(PropertyName = "pidlDocInfo")]
        public PidlDocInfo PidlDocInfo { get; set; }
    }
}