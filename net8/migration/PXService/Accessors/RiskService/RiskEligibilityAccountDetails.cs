// <copyright file="RiskEligibilityAccountDetails.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.RiskService
{
    using Newtonsoft.Json;

    public class RiskEligibilityAccountDetails
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "id_namespace")]
        public string IdNameSpace { get; set; }

        [JsonProperty(PropertyName = "tenant_id")]
        public string TenantId { get; set; }

        [JsonProperty(PropertyName = "objectid")]
        public string ObjectId { get; set; }

        [JsonProperty(PropertyName = "account_id")]
        public string AccountId { get; set; }

        [JsonProperty(PropertyName = "external_user_id")]
        public string ExternalUserId { get; set; }
    }
}