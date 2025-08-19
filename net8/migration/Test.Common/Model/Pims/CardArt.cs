namespace Tests.Common.Model.Pims
{
    using Newtonsoft.Json;

    public class CardArt
    {
        [JsonProperty("cardArtUrl")]
        public string CardArtUrl { get; set; }

        [JsonProperty("mediumCardArtUrl")]
        public string MediumCardArtUrl { get; set; }

        [JsonProperty("thumbnailCardArtUrl")]
        public string ThumbnailCardArtUrl { get; set; }

        [JsonProperty("foregroundColor")]
        public string ForegroundColor { get; set; }

        [JsonProperty("shortDescription")]
        public string ShortDescription { get; set; }

        [JsonProperty("longDescription")]
        public string LongDescription { get; set; }

        [JsonProperty("isCoBranded")]
        public bool IsCoBranded { get; set; }

        [JsonProperty("coBrandedName")]
        public string CoBrandedName { get; set; }
    }
}
