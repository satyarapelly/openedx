// <copyright file="MerchantCapabilitiesErrorResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.MerchantCapabilitiesService.V7
{
    using Newtonsoft.Json;

    public class MerchantCapabilitiesErrorResponse : ServiceErrorResponse
    {
        [JsonProperty(PropertyName = "error")]
        public MerchantCapabilitiesErrorResponse Error { get; set; }

        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "message")]
        public new string Message { get; set; }

        [JsonProperty(PropertyName = "target")]
        public new string Target { get; set; }
    }
}