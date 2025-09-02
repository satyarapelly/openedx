// <copyright file="RiskData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using Newtonsoft.Json;

    public class RiskData
    {
        [JsonProperty(PropertyName = "ipAddress")]
        public string IPAddress { get; set; }

        [JsonProperty(PropertyName = "userInfo")]
        public UserInfo UserInfo { get; set; }

        [JsonProperty(PropertyName = "deviceId")]
        public string DeviceId { get; set; }

        [JsonProperty(PropertyName = "userAgent")]
        public string UserAgent { get; set; }

        [JsonProperty(PropertyName = "greenId")]
        public string GreenId { get; set; }

        [JsonProperty(PropertyName = "deviceType")]
        public string DeviceType { get; set; }
    }
}