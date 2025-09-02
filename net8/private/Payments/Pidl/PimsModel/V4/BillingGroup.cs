// <copyright file="BillingGroup.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using Newtonsoft.Json;

    public class BillingGroup
    {
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }
}