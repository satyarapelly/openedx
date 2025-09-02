// <copyright file="CustomerIdentity.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Tests.Common.Model.Accounts
{
    using Newtonsoft.Json;
    using System.Diagnostics.CodeAnalysis;

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
