// <copyright file="CustomerIdentity.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.AccountService.DataModel
{
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class CustomerIdentity
    {
        [JsonProperty(PropertyName = "provider")]
        public string Provider { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1721", Justification = "Json Property name")]
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "data")]
        public CustomerIdentityData Data { get; set; }
    }
}