// <copyright file="CardMetadata.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Represents a network token request.
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class CardMetadata
    {
        /// <summary>
        /// Gets or sets the card art url.
        /// </summary>
        public string CardArtURL { get; set; }

        /// <summary>
        /// Gets or sets the medium-sized card art url.
        /// </summary>
        public string MediumCardArtURL { get; set; }

        /// <summary>
        /// Gets or sets the thumbnail-sized card art url.
        /// </summary>
        public string ThumbnailCardArtURL { get; set; }

        /// <summary>
        /// Gets or sets the short description.
        /// </summary>
        public string ShortDescription { get; set; }

        /// <summary>
        /// Gets or sets the long description.
        /// </summary>
        public string LongDescription { get; set; }

        /// <summary>
        ///  Gets or sets a value indicating whether value for CoBrandedName is present.
        /// </summary>
        public bool IsCoBranded { get; set; }

        /// <summary>
        /// Gets or sets the co-branding name if present.
        /// </summary>
        public string CoBrandedName { get; set; }

        /// <summary>
        /// Gets or sets the color for overlaying text.
        /// </summary>
        public string ForegroundColor { get; set; }

        /// <summary>
        /// Gets or sets the LatestRefreshTimestamp.
        /// </summary>
        public DateTime LatestRefreshTimestamp { get; set; }
    }
}