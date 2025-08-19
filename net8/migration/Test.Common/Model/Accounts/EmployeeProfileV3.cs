// <copyright file="EmployeeProfileV3.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace Tests.Common.Model.Accounts
{
    using Newtonsoft.Json;

    public class EmployeeProfileV3 : ProfileV3
    {
        // These are the custom properties
        [JsonProperty(PropertyName = "culture", Required = Required.Always)]
        public string Culture { get; set; }

        [JsonProperty(PropertyName = "name", Required = Required.Default)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "email", Required = Required.Always)]
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