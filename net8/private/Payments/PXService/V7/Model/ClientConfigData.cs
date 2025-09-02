// <copyright file="ClientConfigData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using Newtonsoft.Json;

    public class ClientConfigData
    {
        [JsonProperty(PropertyName = "market")]
        public string Market { get; set; }

        [JsonProperty(PropertyName = "locale")]
        public string Locale { get; set; }

        [JsonProperty(PropertyName = "browserType")]
        public string BrowserType { get; set; }

        [JsonProperty(PropertyName = "browserVersion")]
        public string BrowserVersion { get; set; }

        [JsonProperty(PropertyName = "osInfo")]
        public string OsInfo { get; set; }

        [JsonProperty(PropertyName = "platform")]
        public string Platform { get; set; }
    }
}