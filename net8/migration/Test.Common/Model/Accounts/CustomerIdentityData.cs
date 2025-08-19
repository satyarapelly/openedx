// <copyright file="CustomerIdentityData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Tests.Common.Model.Accounts
{
    using Newtonsoft.Json;

    public class CustomerIdentityData
    {
        [JsonProperty(PropertyName = "puid")]
        public string PUID { get; set; }
    }
}