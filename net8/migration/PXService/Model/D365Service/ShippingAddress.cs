// <copyright file="ShippingAddress.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.D365Service
{
    using Newtonsoft.Json;

    public class ShippingAddress
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("fullAddress")]
        public string FullAddress { get; set; }

        [JsonProperty("street")]
        public string Street { get; set; }

        [JsonProperty("streetNumber")]
        public string StreetNumber { get; set; }

        [JsonProperty("county")]
        public string County { get; set; }

        [JsonProperty("countyName")]
        public string CountyName { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("districtName")]
        public string DistrictName { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("stateName")]
        public string StateName { get; set; }

        [JsonProperty("zipCode")]
        public string ZipCode { get; set; }
    }
}