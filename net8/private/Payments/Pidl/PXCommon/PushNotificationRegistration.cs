// <copyright file="PushNotificationRegistration.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXCommon
{
    using System;
    using Newtonsoft.Json;

    public class PushNotificationRegistration
    {
        [JsonProperty("target_device_id")]
        public ulong TargetDeviceId { get; set; }

        [JsonProperty("target_app_id")]
        public string TargetAppId { get; set; }

        [JsonProperty("accountId")]
        public string AccountId { get; set; }

        [JsonProperty("notification_uri")]
        public string NotificationUri { get; set; }
    }
}