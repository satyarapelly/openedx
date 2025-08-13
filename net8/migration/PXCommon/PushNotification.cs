// <copyright file="PushNotification.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXCommon
{
    using System;
    using Newtonsoft.Json;

    public class PushNotification
    {
        [JsonProperty("target_device_id")]
        public ulong TargetDeviceId { get; set; }

        [JsonProperty("target_app_id")]
        public string TargetAppId { get; set; }

        [JsonProperty("type")]
        public string NotificationType { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("status_description")]
        public string StatusDescription { get; set; }

        [JsonProperty("device_connection_status")]
        public string DeviceConnectionStatus { get; set; }

        [JsonProperty("accountId")]
        public string AccountId { get; set; }
    }
}