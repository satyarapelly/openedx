// <copyright file="PhoneInfo.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using Newtonsoft.Json;

    public class PhoneInfo
    {
        [JsonProperty(PropertyName = "area_code")]
        public string AreaCode { get; set; }

        [JsonProperty(PropertyName = "local_number")]
        public string LocalNumber { get; set; }

        [JsonProperty(PropertyName = "extension")]
        public string Extension { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }
    }
}