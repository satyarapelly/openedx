// <copyright file="PidlCacheKey.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlModel.V7
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