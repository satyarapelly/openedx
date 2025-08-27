// <copyright file="VideoDetails.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.D365Service
{
    using Newtonsoft.Json;

    public class VideoDetails
    {
        [JsonProperty("episodeNumber", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int? EpisodeNumber { get; set; }

        [JsonProperty("seasonNumber", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int? SeasonNumber { get; set; }

        [JsonProperty("seriesTitle", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string SeriesTitle { get; set; }

        [JsonProperty("resolutionFormat", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ResolutionFormat ResolutionFormat { get; set; }
    }
}