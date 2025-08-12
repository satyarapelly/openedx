// <copyright file="DeviceDetails.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.RiskService.V7
{
    using Newtonsoft.Json;

    public class DeviceDetails
    {
        [JsonProperty(PropertyName = "ip_address")]
        public string IpAddress { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("device_type")]
        public string DeviceType { get; set; }
    }
}
