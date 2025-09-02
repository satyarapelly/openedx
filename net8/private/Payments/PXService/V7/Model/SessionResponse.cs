// <copyright file="SessionResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using Newtonsoft.Json;

    public class SessionResponse
    {
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }
    }
}