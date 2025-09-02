// <copyright file="TokenInfo.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Tests.Common.Model.Pims
{
    using Newtonsoft.Json;

    public class TokenInfo
    {
        [JsonProperty(PropertyName = "tokenExpiryMonth")]
        public string TokenExpiryMonth { get; set; }

        [JsonProperty(PropertyName = "tokenExpiryYear")]
        public string TokenExpiryYear { get; set; }

        [JsonProperty(PropertyName = "tokenStatus")]
        public string TokenStatus { get; set; }
    }
}
