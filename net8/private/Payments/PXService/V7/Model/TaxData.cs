// <copyright file="TaxData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class TaxData
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "version")]
        public int Version { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string TaxIdType { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "scope")]
        public ScopeData Scope { get; set; }

        [JsonProperty(PropertyName = "country_code")]
        public string CountryCode { get; set; }

        [JsonProperty(PropertyName = "version_timestamp")]
        public string VersionTimestamp { get; set; }

        public Dictionary<string, string> GetPropertyDictionary()
        {
            Dictionary<string, string> propertyDictionary = new Dictionary<string, string>();
            propertyDictionary[GlobalConstants.TaxDataFields.Value] = this.Value;
            return propertyDictionary;
        }

        public class ScopeData
        {
            [JsonProperty(PropertyName = "state")]
            public string State { get; set; }
        }
    }
}