// <copyright file="NetworkToken.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using Newtonsoft.Json;

    public class NetworkToken
    {
        [JsonProperty("networkTokenId")]
        public string Id { get; set; }

        [JsonProperty("networkTokenUsage")]
        public string Usage { get; set; }
    }
}
