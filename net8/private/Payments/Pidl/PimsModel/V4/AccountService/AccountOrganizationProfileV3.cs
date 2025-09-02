// <copyright file="AccountOrganizationProfileV3.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class AccountOrganizationProfileV3 : AccountProfileV3
    {
        // These are the custom properties
        [JsonProperty(PropertyName = Fields.Culture, Required = Required.Always)]
        public string Culture { get; set; }

        // Javis team has a bug to allow empty value in email property
        // BUG 14288115 Jarvis contains org profiles with null value for mandatory email field
        [JsonProperty(PropertyName = Fields.Email, Required = Required.Default)]
        public string Email { get; set; }

        [JsonProperty(PropertyName = Fields.CompanyName, Required = Required.Always)]
        public string CompanyName { get; set; }

        [JsonProperty(PropertyName = Fields.Language, Required = Required.Always)]
        public string Language { get; set; }

        [JsonProperty(PropertyName = Fields.LoveCode, Required = Required.Default)]
        public string LoveCode { get; set; }

        [JsonProperty(PropertyName = Fields.MobileBarcode, Required = Required.Default)]
        public string MobileBarcode { get; set; }

        public override string GetEmailAddressPropertyValue()
        {
            return this.Email;
        }

        public override Dictionary<string, string> GetPropertyDictionary()
        {
            Dictionary<string, string> propertyDictionary = base.GetPropertyDictionary();
            propertyDictionary[AccountOrganizationProfileV3.Fields.Culture] = this.Culture;
            propertyDictionary[AccountOrganizationProfileV3.Fields.CompanyName] = this.CompanyName;
            propertyDictionary[AccountOrganizationProfileV3.Fields.Language] = this.Language;
            propertyDictionary[AccountOrganizationProfileV3.Fields.LoveCode] = this.LoveCode;
            propertyDictionary[AccountOrganizationProfileV3.Fields.MobileBarcode] = this.MobileBarcode;
            propertyDictionary[AccountOrganizationProfileV3.Fields.Email] = this.Email;
            return propertyDictionary;
        }

        private static class Fields
        {
            internal const string Culture = "culture";
            internal const string CompanyName = "company_name";
            internal const string Language = "language";
            internal const string LoveCode = "love_code";
            internal const string MobileBarcode = "mobile_barcode";
            internal const string Email = "email";
        }
    }
}