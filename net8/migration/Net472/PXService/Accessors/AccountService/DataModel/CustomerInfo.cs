// <copyright file="CustomerInfo.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.AccountService.DataModel
{
    using Newtonsoft.Json;

    public class CustomerInfo
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "etag")]
        public string Etag { get; set; }

        [JsonProperty(PropertyName = "customer_type")]
        public string CustomerType { get; set; }

        [JsonProperty(PropertyName = "customer_subtype")]
        public string CustomerSubType { get; set; }

        [JsonProperty(PropertyName = "identity")]
        public CustomerIdentity Identity { get; set; }

        [JsonProperty(PropertyName = "is_test")]
        public bool IsTest { get; set; }

        [JsonProperty(PropertyName = "links")]
        public object Links { get; set; }

        [JsonProperty(PropertyName = "object_type")]
        public string ObjectType { get; set; }

        [JsonProperty(PropertyName = "resource_status")]
        public string ResourceStatus { get; set; }
    }
}