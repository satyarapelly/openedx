// <copyright file="ResourceSetting.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PartnerSettingsModel
{
    using Newtonsoft.Json;

    public class ResourceSetting
    {
        [JsonProperty("template")]
        public string Template { get; set; }

        [JsonProperty("redirectionPattern")]
        public string RedirectionPattern { get; set; }
    }
}
