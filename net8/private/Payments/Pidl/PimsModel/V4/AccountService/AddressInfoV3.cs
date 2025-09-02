// <copyright file="AddressInfoV3.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class AddressInfoV3
    {
        public AddressInfoV3()
        {
        }

        [JsonProperty(PropertyName = Fields.CustomerId)]
        public string CustomerId { get; set; }

        [JsonProperty(PropertyName = Fields.Id)]
        public string Id { get; set; }

        [JsonProperty(PropertyName = Fields.Country)]
        public string Country { get; set; }

        [JsonProperty(PropertyName = Fields.Region)]
        public string Region { get; set; }

        [JsonProperty(PropertyName = Fields.District)]
        public string District { get; set; }

        [JsonProperty(PropertyName = Fields.City)]
        public string City { get; set; }

        [JsonProperty(PropertyName = Fields.AddressLine1)]
        public string AddressLine1 { get; set; }

        [JsonProperty(PropertyName = Fields.AddressLine2)]
        public string AddressLine2 { get; set; }

        [JsonProperty(PropertyName = Fields.AddressLine3)]
        public string AddressLine3 { get; set; }

        [JsonProperty(PropertyName = Fields.PostalCode)]
        public string PostalCode { get; set; }

        [JsonProperty(PropertyName = Fields.FirstName)]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = Fields.FirstNamePronunciation)]
        public string FirstNamePronunciation { get; set; }

        [JsonProperty(PropertyName = Fields.LastName)]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = Fields.MiddleName)]
        public string MiddleName { get; set; }

        [JsonProperty(PropertyName = Fields.LastNamePronunciation)]
        public string LastNamePronunciation { get; set; }

        [JsonProperty(PropertyName = Fields.CorrespondenceName)]
        public string CorrespondenceName { get; set; }

        [JsonProperty(PropertyName = Fields.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [JsonProperty(PropertyName = Fields.Mobile)]
        public string Mobile { get; set; }

        [JsonProperty(PropertyName = Fields.Fax)]
        public string Fax { get; set; }

        [JsonProperty(PropertyName = Fields.Telex)]
        public string Telex { get; set; }

        [JsonProperty(PropertyName = Fields.EmailAddress)]
        public string EmailAddress { get; set; }

        [JsonProperty(PropertyName = Fields.WebSiteUrl)]
        public string WebSiteUrl { get; set; }

        [JsonProperty(PropertyName = Fields.StreetSupplement)]
        public string StreetSupplement { get; set; }

        [JsonProperty(PropertyName = Fields.IsWithinCityLimits)]
        public string IsWithinCityLimits { get; set; }

        [JsonProperty(PropertyName = Fields.FormOfAddress)]
        public string FormOfAddress { get; set; }

        [JsonProperty(PropertyName = Fields.AddressNotes)]
        public string AddressNotes { get; set; }

        [JsonProperty(PropertyName = Fields.TimeZone)]
        public string TimeZone { get; set; }

        [JsonProperty(PropertyName = Fields.Latitude)]
        public string Latitude { get; set; }

        [JsonProperty(PropertyName = Fields.Longitude)]
        public string Longitude { get; set; }

        [JsonProperty(PropertyName = Fields.IsAvsValidated)]
        public bool? IsAvsValidated { get; set; }

        [JsonProperty(PropertyName = Fields.Validate)]
        public bool? Validate { get; set; }

        [JsonProperty(PropertyName = Fields.ValidationStamp)]
        public object ValidationStamp { get; set; }

        [JsonProperty(PropertyName = Fields.Links)]
        public object Links { get; set; }

        [JsonProperty(PropertyName = Fields.ObjectType)]
        public string ObjectType { get; set; }

        [JsonProperty(PropertyName = Fields.ContractVersion)]
        public string ContractVersion { get; set; }

        [JsonProperty(PropertyName = Fields.ResourceStatus)]
        public string ResourceStatus { get; set; }

        [JsonProperty(PropertyName = Fields.IsCustomerConsented)]
        public bool? IsCustomerConsented { get; set; }

        [JsonProperty(PropertyName = Fields.IsZipPlus4Present)]
        public bool? IsZipPlus4Present { get; set; }

        [JsonProperty(PropertyName = Fields.ETag)]
        public string Etag { get; set; }

        [JsonProperty(PropertyName = Fields.IsAVSFullValidationSucceeded)]
        public bool? IsAVSFullValidationSucceeded { get; set; }

        public virtual Dictionary<string, string> GetPropertyDictionary()
        {
            Dictionary<string, string> propertyDictionary = new Dictionary<string, string>();
            propertyDictionary[AddressInfoV3.Fields.CustomerId] = this.CustomerId;
            propertyDictionary[AddressInfoV3.Fields.Country] = this.Country;
            propertyDictionary[AddressInfoV3.Fields.Region] = this.Region;
            propertyDictionary[AddressInfoV3.Fields.District] = this.District;
            propertyDictionary[AddressInfoV3.Fields.City] = this.City;
            propertyDictionary[AddressInfoV3.Fields.AddressLine1] = this.AddressLine1;
            propertyDictionary[AddressInfoV3.Fields.AddressLine2] = this.AddressLine2;
            propertyDictionary[AddressInfoV3.Fields.AddressLine3] = this.AddressLine3;
            propertyDictionary[AddressInfoV3.Fields.PostalCode] = this.PostalCode;
            propertyDictionary[AddressInfoV3.Fields.FirstName] = this.FirstName;
            propertyDictionary[AddressInfoV3.Fields.FirstNamePronunciation] = this.FirstNamePronunciation;
            propertyDictionary[AddressInfoV3.Fields.LastName] = this.LastName;
            propertyDictionary[AddressInfoV3.Fields.LastNamePronunciation] = this.LastNamePronunciation;
            propertyDictionary[AddressInfoV3.Fields.MiddleName] = this.MiddleName;
            propertyDictionary[AddressInfoV3.Fields.CorrespondenceName] = this.CorrespondenceName;
            propertyDictionary[AddressInfoV3.Fields.PhoneNumber] = this.PhoneNumber;
            propertyDictionary[AddressInfoV3.Fields.Mobile] = this.Mobile;
            propertyDictionary[AddressInfoV3.Fields.Fax] = this.Fax;
            propertyDictionary[AddressInfoV3.Fields.Telex] = this.Telex;
            propertyDictionary[AddressInfoV3.Fields.EmailAddress] = this.EmailAddress;
            propertyDictionary[AddressInfoV3.Fields.WebSiteUrl] = this.WebSiteUrl;
            propertyDictionary[AddressInfoV3.Fields.StreetSupplement] = this.StreetSupplement;
            propertyDictionary[AddressInfoV3.Fields.IsWithinCityLimits] = this.IsWithinCityLimits;
            propertyDictionary[AddressInfoV3.Fields.FormOfAddress] = this.FormOfAddress;
            propertyDictionary[AddressInfoV3.Fields.AddressNotes] = this.AddressNotes;
            propertyDictionary[AddressInfoV3.Fields.TimeZone] = this.TimeZone;
            propertyDictionary[AddressInfoV3.Fields.Latitude] = this.Latitude;
            propertyDictionary[AddressInfoV3.Fields.Longitude] = this.Longitude;
            propertyDictionary[AddressInfoV3.Fields.IsAvsValidated] = this.IsAvsValidated.ToString();
            propertyDictionary[AddressInfoV3.Fields.Validate] = this.Validate.ToString();
            propertyDictionary[AddressInfoV3.Fields.ValidationStamp] = this.ValidationStamp == null ? null : this.ValidationStamp.ToString();
            propertyDictionary[AddressInfoV3.Fields.Links] = this.Links == null ? null : this.Links.ToString();
            propertyDictionary[AddressInfoV3.Fields.ObjectType] = this.ObjectType;
            propertyDictionary[AddressInfoV3.Fields.ContractVersion] = this.ContractVersion;
            propertyDictionary[AddressInfoV3.Fields.ResourceStatus] = this.ResourceStatus;
            propertyDictionary[AddressInfoV3.Fields.IsCustomerConsented] = this.IsCustomerConsented.ToString();
            propertyDictionary[AddressInfoV3.Fields.IsZipPlus4Present] = this.IsZipPlus4Present.ToString();
            propertyDictionary[AddressInfoV3.Fields.ETag] = this.Etag;

            return propertyDictionary;
        }

        protected static class Fields
        {
            internal const string Id = "id";
            internal const string CustomerId = "customer_id";
            internal const string Country = "country";
            internal const string Region = "region";
            internal const string District = "district";
            internal const string City = "city";
            internal const string AddressLine1 = "address_line1";
            internal const string AddressLine2 = "address_line2";
            internal const string AddressLine3 = "address_line3";
            internal const string PostalCode = "postal_code";
            internal const string FirstName = "first_name";
            internal const string FirstNamePronunciation = "first_name_pronunciation";
            internal const string LastName = "last_name";
            internal const string LastNamePronunciation = "last_name_pronunciation";
            internal const string MiddleName = "middle_name";
            internal const string CorrespondenceName = "correspondence_name";
            internal const string PhoneNumber = "phone_number";
            internal const string Mobile = "mobile";
            internal const string Fax = "fax";
            internal const string Telex = "telex";
            internal const string EmailAddress = "email_address";
            internal const string WebSiteUrl = "web_site_url";
            internal const string StreetSupplement = "street_supplement";
            internal const string IsWithinCityLimits = "is_within_city_limits";
            internal const string FormOfAddress = "form_of_address";
            internal const string AddressNotes = "address_notes";
            internal const string TimeZone = "time_zone";
            internal const string Latitude = "latitude";
            internal const string Longitude = "longitude";
            internal const string IsAvsValidated = "is_avs_validated";
            internal const string Validate = "validate";
            internal const string ValidationStamp = "validation_stamp";
            internal const string Links = "links";
            internal const string ObjectType = "object_type";
            internal const string ContractVersion = "contract_version";
            internal const string ResourceStatus = "resource_status";
            internal const string IsCustomerConsented = "is_customer_consented";
            internal const string IsZipPlus4Present = "is_zip_plus_4_present";
            internal const string ETag = "etag";
            internal const string IsAVSFullValidationSucceeded = "is_avs_full_validation_succeeded";
        }
    }
}