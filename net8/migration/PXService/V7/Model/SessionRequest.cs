// <copyright file="SessionRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using Newtonsoft.Json;

    public class SessionRequest
    {
        [JsonProperty(PropertyName = "shopperId")]
        public string ShopperId { get; set; }
    }
}