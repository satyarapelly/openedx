// <copyright file="CardArt.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
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
