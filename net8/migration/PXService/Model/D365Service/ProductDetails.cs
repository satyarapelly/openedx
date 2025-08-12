// <copyright file="ProductDetails.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.D365Service
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class ProductDetails
    {
        [JsonProperty("subscriptionPartnerId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string SubscriptionPartnerId { get; set; }

        [JsonProperty("isRental", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool IsRental { get; set; }

        [JsonProperty("isPreorder", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public bool IsPreorder { get; set; }

        [JsonProperty("preOrderReleaseDate", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? PreOrderReleaseDate { get; set; }

        [JsonProperty("publisherId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string PublisherId { get; set; }

        [JsonProperty("publisherName", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string PublisherName { get; set; }

        [JsonProperty("brandId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string BrandId { get; set; }

        [JsonProperty("developerName", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string DeveloperName { get; set; }

        [JsonProperty("paymentInstrumentInclusionFilterTags", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public IList<string> PaymentInstrumentInclusionFilterTags { get; set; }

        [JsonProperty("redemptionUrlTemplate", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string RedemptionUrlTemplate { get; set; }

        [JsonProperty("musicDetails", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public MusicDetails MusicDetails { get; set; }

        [JsonProperty("videoDetails", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public VideoDetails VideoDetails { get; set; }

        [JsonProperty("installationTerms", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string InstallationTerms { get; set; }

        [JsonProperty("expiringDownloadOptions", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public IList<TimeExpiringDownloadOption> ExpiringDownloadOptions { get; set; }

        [JsonProperty("paymentInstrumentExclusionFilterTags", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public IList<string> PaymentInstrumentExclusionFilterTags { get; set; }

        [JsonProperty("estimatedDeliveryOverlayMessage", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string EstimatedDeliveryOverlayMessage { get; set; }

        [JsonProperty("pdpUrl", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public Uri PdpUrl { get; set; }

        [JsonProperty("bundleImageUrl", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public Uri BundleImageUrl { get; set; }

        [JsonProperty("productId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string ProductId { get; set; }

        [JsonProperty("skuId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string SkuId { get; set; }

        [JsonProperty("sapSkuId", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string SapSkuId { get; set; }

        [JsonProperty("displayRank", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public int DisplayRank { get; set; }

        [JsonProperty("market", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)] public string Market { get; set; }

        [JsonProperty("language", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Language { get; set; }

        [JsonProperty("bundleImageBackgroundUrl", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public Uri BundleImageBackgroundUrl { get; set; }

        [JsonProperty("title", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Title { get; set; }

        [JsonProperty("productFamily", Required = Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string ProductFamily { get; set; }

        [JsonProperty("productType", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public ProductType ProductType { get; set; }

        [JsonProperty("listPrice", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public decimal ListPrice { get; set; }

        [JsonProperty("msrp", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public decimal MSRP { get; set; }

        [JsonProperty("imageUrl", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public Uri ImageUrl { get; set; }

        [JsonProperty("imageBackgroundUrl", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public Uri ImageBackgroundUrl { get; set; }

        [JsonProperty("bundleTitle", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string BundleTitle { get; set; }

        [JsonProperty("bundleProductSkuRankMap", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public IDictionary<string, uint> BundleProductSkuRankMap { get; set; }
    }
}