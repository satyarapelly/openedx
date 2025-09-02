// <copyright file="AccountEmployeeProfileV3.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class AccountEmployeeProfileV3 : AccountProfileV3
    {
        // These are the custom properties
        [JsonProperty(PropertyName = "culture", Required = Required.Always)]
        public string Culture { get; set; }

        [JsonProperty(PropertyName = "name", Required = Required.Default)]
        public string Name { get; set; }

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

        public override string GetEmailAddressPropertyValue()
        {
            return this.Email;
        }

        public override Dictionary<string, string> GetPropertyDictionary()
        {
            Dictionary<string, string> propertyDictionary = base.GetPropertyDictionary();
            propertyDictionary[AccountEmployeeProfileV3.Fields.Culture] = this.Culture;
            propertyDictionary[AccountEmployeeProfileV3.Fields.CompanyName] = this.CompanyName;
            propertyDictionary[AccountEmployeeProfileV3.Fields.Language] = this.Language;
            propertyDictionary[AccountEmployeeProfileV3.Fields.Name] = this.Name;
            propertyDictionary[AccountEmployeeProfileV3.Fields.LoveCode] = this.LoveCode;
            propertyDictionary[AccountEmployeeProfileV3.Fields.MobileBarcode] = this.MobileBarcode;
            propertyDictionary[AccountEmployeeProfileV3.Fields.Email] = this.Email;
            return propertyDictionary;
        }

        private static class Fields
        {
            internal const string Culture = "culture";
            internal const string Name = "name";
            internal const string Email = "email";
            internal const string CompanyName = "company_name";
            internal const string Language = "language";
            internal const string LoveCode = "love_code";
            internal const string MobileBarcode = "mobile_barcode";
        }
    }
}