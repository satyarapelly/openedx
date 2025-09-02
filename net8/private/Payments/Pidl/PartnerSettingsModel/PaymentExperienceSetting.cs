// <copyright file="PaymentExperienceSetting.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PartnerSettingsModel
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    public class PaymentExperienceSetting
    {
        [JsonProperty(PropertyName = "template")]
        public string Template { get; set; }

        [JsonProperty(PropertyName = "resources")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public Dictionary<string, Dictionary<string, ResourceSetting>> Resources { get; set; }

        [JsonProperty(PropertyName = "features")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public Dictionary<string, FeatureConfig> Features { get; set; }

        [JsonProperty("redirectionPattern")]
        public string RedirectionPattern { get; set; }

        [JsonProperty(PropertyName = "challengeWindowSize", NullValueHandling = NullValueHandling.Ignore)]
        public string ChallengeWindowSize { get; set; }
    }
}