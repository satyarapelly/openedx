// <copyright file="DeviceDataProperty.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using Newtonsoft.Json;

    public class DeviceDataProperty
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("send")]
        public bool Send { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("hashMethod")]
        public string HashMethod { get; set; }
    }
}