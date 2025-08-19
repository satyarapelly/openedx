// <copyright file="AddressV3.cs" company="Microsoft">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace Tests.Common.Model.Accounts
{
    using Newtonsoft.Json;

    public class AddressV3
    {
        [JsonProperty(PropertyName = "customer_id")]
        public string CustomerId { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "region")]
        public string Region { get; set; }

        [JsonProperty(PropertyName = "district")]
        public string District { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "address_line1")]
        public string AddressLine1 { get; set; }

        [JsonProperty(PropertyName = "address_line2")]
        public string AddressLine2 { get; set; }

        [JsonProperty(PropertyName = "address_line3")]
        public string AddressLine3 { get; set; }

        [JsonProperty(PropertyName = "postal_code")]
        public string PostalCode { get; set; }

        [JsonProperty(PropertyName = "first_name")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "first_name_pronunciation")]
        public string FirstNamePronunciation { get; set; }

        [JsonProperty(PropertyName = "last_name")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "last_name_pronunciation")]
        public string LastNamePronunciation { get; set; }

        [JsonProperty(PropertyName = "correspondence_name")]
        public string CorrespondenceName { get; set; }

        [JsonProperty(PropertyName = "phone_number")]
        public string PhoneNumber { get; set; }

        [JsonProperty(PropertyName = "mobile")]
        public string Mobile { get; set; }

        [JsonProperty(PropertyName = "fax")]
        public string Fax { get; set; }

        [JsonProperty(PropertyName = "telex")]
        public string Telex { get; set; }

        [JsonProperty(PropertyName = "email_address")]
        public string EmailAddress { get; set; }

        [JsonProperty(PropertyName = "web_site_url")]
        public string WebSiteUrl { get; set; }

        [JsonProperty(PropertyName = "street_supplement")]
        public string StreetSupplement { get; set; }

        [JsonProperty(PropertyName = "is_within_city_limits")]
        public string IsWithinCityLimits { get; set; }

        [JsonProperty(PropertyName = "form_of_address")]
        public string FormOfAddress { get; set; }

        [JsonProperty(PropertyName = "address_notes")]
        public string AddressNotes { get; set; }

        [JsonProperty(PropertyName = "time_zone")]
        public string TimeZone { get; set; }

        [JsonProperty(PropertyName = "latitude")]
        public string Latitude { get; set; }

        [JsonProperty(PropertyName = "longitude")]
        public string Longitude { get; set; }

        [JsonProperty(PropertyName = "is_avs_validated")]
        public bool? IsAvsValidated { get; set; }

        [JsonProperty(PropertyName = "validate")]
        public bool? Validate { get; set; }

        [JsonProperty(PropertyName = "validation_stamp")]
        public object ValidationStamp { get; set; }

        [JsonProperty(PropertyName = "links")]
        public object Links { get; set; }

        [JsonProperty(PropertyName = "object_type")]
        public string ObjectType { get; set; }

        [JsonProperty(PropertyName = "contract_version")]
        public string ContractVersion { get; set; }

        [JsonProperty(PropertyName = "resource_status")]
        public string ResourceStatus { get; set; }
    }
}