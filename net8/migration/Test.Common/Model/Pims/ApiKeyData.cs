// <copyright file="ApiKeyData.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Tests.Common.Model.Pims
{
    using Newtonsoft.Json;

    public class ApiKeyData
    {
        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }

        [JsonProperty("sharedSecret")]
        public string SharedSecret { get; set; }

        [JsonProperty("apiKeyUsage")]
        public string ApiKeyUsage { get; set; }
    }
}