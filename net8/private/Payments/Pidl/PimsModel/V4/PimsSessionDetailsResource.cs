// <copyright file="PimsSessionDetailsResource.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2019. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using Microsoft.Commerce.Payments.PXCommon;
    using Newtonsoft.Json;

    public class PimsSessionDetailsResource : RestResource
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "accountId")]
        public string AccountId { get; set; }

        [JsonProperty(PropertyName = "status")]
        public PimsSessionStatus Status { get; set; }

        [JsonProperty(PropertyName = "details")]
        public PimsSessionDetails Details { get; set; }
    }
}