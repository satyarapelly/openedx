// <copyright file="ClientDeviceInfo.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using Newtonsoft.Json;

    public class ClientDeviceInfo
    {
        [JsonProperty("bindingRequired")]
        public bool BindingRequired { get; set; }

        [JsonProperty("deviceEnrolled")]
        public bool DeviceEnrolled { get; set; }

        [JsonProperty("bindingStatus")]
        public string BindingStatus { get; set; }
    }
}