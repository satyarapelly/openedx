// <copyright file="ClientContext.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentTransaction.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using Newtonsoft.Json;

    public class ClientContext
    {
        [JsonProperty("client")]
        public string Client { get; set; }

        [JsonProperty("deviceFamily")]
        public string DeviceFamily { get; set; }

        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        [JsonProperty("deviceType")]
        public string DeviceType { get; set; }
    }
}