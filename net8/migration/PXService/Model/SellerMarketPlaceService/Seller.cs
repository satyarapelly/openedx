// <copyright file="Seller.cs" company="Microsoft">Copyright (c) Microsoft 2022 All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.SellerMarketPlaceService
{
    using Newtonsoft.Json;

    public class Seller
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "sellerName")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "sellerCountry")]
        public string Country { get; set; }
    }
}
