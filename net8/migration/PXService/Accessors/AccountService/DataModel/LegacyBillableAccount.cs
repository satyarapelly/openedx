// <copyright file="LegacyBillableAccount.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.AccountService.DataModel
{
    using System;
    using Newtonsoft.Json;

    public class LegacyBillableAccount
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "bdk_id")]
        public string BdkId { get; set; }

        [JsonProperty(PropertyName = "cid")]
        public string Cid { get; set; }

        [JsonProperty(PropertyName = "country_code")]
        public string CountryCode { get; set; }

        [JsonProperty(PropertyName = "account_level")]
        public string AccountLevel { get; set; }

        [JsonProperty(PropertyName = "customer_type")]
        public string CustomerType { get; set; }

        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; }

        [JsonProperty(PropertyName = "lastUpdatedDateTime")]
        public DateTime? LastUpdatedDateTime { get; set; }

        [JsonProperty(PropertyName = "profile_type")]
        public string ProfileType { get; set; }

        [JsonProperty(PropertyName = "identity")]
        public LegacyIdentity Identity { get; set; }

        [JsonProperty(PropertyName = "object_type")]
        public string ObjectType { get; set; }

        [JsonProperty(PropertyName = "resource_status")]
        public string ResourceStatus { get; set; }
    }
}