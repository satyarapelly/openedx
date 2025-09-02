// <copyright file="PaymentClientSettingsData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class PaymentClientSettingsData
    {
        private Dictionary<string, DeviceDataProperty> deviceDataProperties = new Dictionary<string, DeviceDataProperty>();
        private Dictionary<string, DirectoryServerData> directoryServerInfo = new Dictionary<string, DirectoryServerData>();

        [JsonProperty("settingsVersion")]
        public string SettingsVersion { get; set; }

        [JsonProperty("sdkAppID")]
        public string SDKAppID { get; set; }

        [JsonProperty("sdkReferenceNumber")]
        public string SDKReferenceNumber { get; set; }

        [JsonProperty("deviceDataVersion")]
        public string DeviceDataVersion { get; set; }

        [JsonProperty("deviceDataProperties")]
        public Dictionary<string, DeviceDataProperty> DeviceDataProperties 
        { 
            get 
            { 
                return this.deviceDataProperties; 
            } 
        }

        [JsonProperty("directoryServerInfo")]
        public Dictionary<string, DirectoryServerData> DirectoryServerInfo
        {
            get
            {
                return this.directoryServerInfo;
            }
        }

        [JsonProperty("threeDSMessageVersion")]
        public string ThreeDSMessageVersion { get; set; }
    }
}
