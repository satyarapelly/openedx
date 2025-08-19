// <copyright file="PaymentInstrumentDisplayDetails.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Tests.Common.Model.Pims
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    /// <summary>
    /// Objects of this class represent payment instrument details that are specific to each type
    /// </summary>
    public class PaymentInstrumentDisplayDetails
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "logo")]
        public string Logo { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        [JsonProperty(PropertyName = "logos")]
        public List<Logo> Logos { get; set; }

        [JsonProperty(PropertyName = "termsAndConditions", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string TermsAndConditions { get; set; }

        [JsonProperty("cardArt")]
        public CardArt CardArt { get; set; }
    }
}