// <copyright file="MusicDetails.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.D365Service
{
    using Newtonsoft.Json;

    public class MusicDetails
    {
        [JsonProperty("artist", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Artist { get; set; }

        [JsonProperty("album", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Album { get; set; }
    }
}