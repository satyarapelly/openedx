// <copyright file="Address.cs" company="Microsoft">Copyright (c) Microsoft 2022 All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentThirdPartyService
{
    using Newtonsoft.Json;

    public class Address
    {
        [JsonProperty(PropertyName = "postal_code")]
        public string PostalCode { get; set; }

        [JsonProperty(PropertyName = "country", Required = Required.Always)]
        public string CountryCode { get; set; }
    }
}
