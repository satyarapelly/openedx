// <copyright file="ConsumerProfileV3.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace Tests.Common.Model.Accounts
{
    using Newtonsoft.Json;

    public class ConsumerProfileV3 : ProfileV3
    {
        [JsonProperty(PropertyName = "email_address", Required = Required.Default)]
        public string EmailAddress { get; set; }
    }
}