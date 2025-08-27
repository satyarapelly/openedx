// <copyright file="TimeExpiringDownloadOption.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.D365Service
{
    using Newtonsoft.Json;

    public class TimeExpiringDownloadOption
    {
        [JsonProperty("friendlyFileName", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string FriendlyFileName { get; set; }

        [JsonProperty("title", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("displayRank", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int DisplayRank { get; set; }
    }
}