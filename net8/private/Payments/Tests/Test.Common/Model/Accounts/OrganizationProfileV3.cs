// <copyright file="OrganizationProfileV3.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace Tests.Common.Model.Accounts 
{
    using Newtonsoft.Json;

    public class OrganizationProfileV3 : ProfileV3
    {
        [JsonProperty(PropertyName = "culture", Required = Required.Always)]
        public string Culture { get; set; }

        // Javis team has a bug to allow empty value in email property
        // BUG 14288115 Jarvis contains org profiles with null value for mandatory email field
        [JsonProperty(PropertyName = "email", Required = Required.Default)]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "company_name", Required = Required.Always)]
        public string CompanyName { get; set; }

        [JsonProperty(PropertyName = "language", Required = Required.Always)]
        public string Language { get; set; }

        [JsonProperty(PropertyName = "love_code", Required = Required.Default)]
        public string LoveCode { get; set; }

        [JsonProperty(PropertyName = "mobile_barcode", Required = Required.Default)]
        public string MobileBarcode { get; set; }
    }
}