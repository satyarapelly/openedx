// <copyright file="DomainData.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.CatalogService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    public class DomainData
    {
        [JsonProperty(PropertyName = "DomainDataType")]
        public string DomainDataType { get; set; }

        [JsonProperty(PropertyName = "Language")]
        public string Language { get; set; }

        [JsonProperty(PropertyName = "Market")]
        public string Market { get; set; }

        [JsonProperty(PropertyName = "ValidUntil")]
        public string ValidUntil { get; set; }

        [JsonProperty(PropertyName = "Values")]
        public IList<DomainDataTypeValue> Values { get; } = new List<DomainDataTypeValue>();

        public List<string> GetMarkets()
        {
            var markets = new List<string>();
            DomainDataTypeValue marketValue = this.Values.Where(val => string.Equals(val.Key, "Market", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            foreach (var market in marketValue?.Values)
            {
                markets.Add(market.Value.ToLower());
            }

            return markets;
        }

        public bool IsExpired()
        {
            DateTime validUntilDateTime;
            if (DateTime.TryParse(this.ValidUntil, out validUntilDateTime))
            {
                return validUntilDateTime < DateTime.UtcNow;
            }

            return true;
        }
    }
}
