// <copyright file="AccountLegalProfileV3.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using Newtonsoft.Json;

    public class AccountLegalProfileV3 : AccountProfileV3
    {
        /// <summary>
        /// Gets or sets a value indicating the culture.
        /// </summary>
        [JsonProperty(PropertyName = Fields.Culture)]
        public string Culture { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the company name.
        /// </summary>
        [JsonProperty(PropertyName = Fields.CompanyName)]
        public string CompanyName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the email.
        /// </summary>
        [JsonProperty(PropertyName = Fields.EmailAddress)]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the language.
        /// </summary>
        [JsonProperty(PropertyName = Fields.Language)]
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the default address.
        /// </summary>
        [JsonProperty(PropertyName = Fields.DefaultAddress)]
        public AddressInfoV3 DefaultAddress { get; set; }

        public override string GetEmailAddressPropertyValue()
        {
            return this.Email;
        }

        public override Dictionary<string, string> GetPropertyDictionary()
        {
            Dictionary<string, string> propertyDictionary = base.GetPropertyDictionary();
            propertyDictionary[AccountLegalProfileV3.Fields.Culture] = this.Culture;
            propertyDictionary[AccountLegalProfileV3.Fields.CompanyName] = this.CompanyName;
            propertyDictionary[AccountLegalProfileV3.Fields.Language] = this.Language;
            propertyDictionary[AccountLegalProfileV3.Fields.EmailAddress] = this.Email;
            propertyDictionary[AccountLegalProfileV3.Fields.AddressLine1] = this.DefaultAddress.AddressLine1;
            propertyDictionary[AccountLegalProfileV3.Fields.AddressLine2] = this.DefaultAddress.AddressLine2;
            propertyDictionary[AccountLegalProfileV3.Fields.AddressLine3] = this.DefaultAddress.AddressLine3;
            propertyDictionary[AccountLegalProfileV3.Fields.City] = this.DefaultAddress.City;
            propertyDictionary[AccountLegalProfileV3.Fields.Region] = this.DefaultAddress.Region;
            propertyDictionary[AccountLegalProfileV3.Fields.PostalCode] = this.DefaultAddress.PostalCode;
            propertyDictionary[AccountLegalProfileV3.Fields.Country] = this.DefaultAddress.Country;
            propertyDictionary[AccountLegalProfileV3.Fields.PhoneNumber] = this.DefaultAddress.PhoneNumber;
            return propertyDictionary;
        }

        private static class Fields
        {
            internal const string Culture = "culture";
            internal const string CompanyName = "company_name";
            internal const string DefaultAddress = "default_address";
            internal const string Language = "language";
            internal const string EmailAddress = "email_address";
            internal const string AddressLine1 = "address_line1";
            internal const string AddressLine2 = "address_line2";
            internal const string AddressLine3 = "address_line3";
            internal const string City = "city";
            internal const string Region = "region";
            internal const string PostalCode = "postal_code";
            internal const string Country = "country";
            internal const string PhoneNumber = "phone_number";
        }
    }
}