// <copyright file="ValidateAddressResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using Newtonsoft.Json;

    public class ValidateAddressResponse
    {
        [JsonProperty(PropertyName = "original_address")]
        public object OriginalAddress { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
    }
}