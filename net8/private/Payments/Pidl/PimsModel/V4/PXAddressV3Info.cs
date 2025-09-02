// <copyright file="PXAddressV3Info.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PimsModel.V4
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class PXAddressV3Info : AddressInfoV3
    {
        public PXAddressV3Info()
        {
        }

        public PXAddressV3Info(AddressInfoV3 address)
        {
            this.AddressLine1 = address.AddressLine1;
            this.AddressLine2 = address.AddressLine2;
            this.AddressLine3 = address.AddressLine3;
            this.ContractVersion = address.ContractVersion;
            this.CustomerId = address.CustomerId;
            this.CorrespondenceName = address.CorrespondenceName;
            this.City = address.City;
            this.Country = address.Country;
            this.FirstName = address.FirstName;
            this.Id = address.Id;
            this.IsAvsValidated = address.IsAvsValidated;
            this.IsWithinCityLimits = address.IsWithinCityLimits;
            this.LastName = address.LastName;
            this.LastNamePronunciation = address.LastNamePronunciation;
            this.Latitude = address.Latitude;
            this.Links = address.Links;
            this.Longitude = address.Longitude;
            this.MiddleName = address.MiddleName;
            this.Mobile = address.Mobile;
            this.PhoneNumber = address.PhoneNumber;
            this.PostalCode = address.PostalCode;
            this.Region = address.Region;
            this.ResourceStatus = address.ResourceStatus;
            this.StreetSupplement = address.StreetSupplement;
            this.Telex = address.Telex;
            this.TimeZone = address.TimeZone;
            this.Validate = address.Validate;
            this.ValidationStamp = address.ValidationStamp;
            this.WebSiteUrl = address.WebSiteUrl;
            this.IsCustomerConsented = address.IsCustomerConsented;
            this.IsZipPlus4Present = address.IsZipPlus4Present;
            this.Etag = address.Etag;
            this.IsAVSFullValidationSucceeded = address.IsAVSFullValidationSucceeded;
        }

        public PXAddressV3Info(Dictionary<string, object> pidlData)
        {
            var addressData = pidlData;

            if (addressData.ContainsKey(PXFields.AddressShippingV3))
            {
                addressData = GetNestedAddressData(pidlData, addressData, PXFields.AddressShippingV3);
            }

            if (addressData.ContainsKey(PXFields.AddressBillingV3))
            {
                addressData = GetNestedAddressData(pidlData, addressData, PXFields.AddressBillingV3);
            }

            this.ObjectType = addressData.ContainsKey(Fields.ObjectType) ? addressData[Fields.ObjectType] as string : null;

            this.Id = addressData.ContainsKey(Fields.Id) ? addressData[Fields.Id] as string : null;
            this.AddressLine1 = addressData.ContainsKey(Fields.AddressLine1) ? addressData[Fields.AddressLine1] as string : null;
            this.AddressLine2 = addressData.ContainsKey(Fields.AddressLine2) ? addressData[Fields.AddressLine2] as string : null;
            this.AddressLine3 = addressData.ContainsKey(Fields.AddressLine3) ? addressData[Fields.AddressLine3] as string : null;
            this.City = addressData.ContainsKey(Fields.City) ? addressData[Fields.City] as string : null;
            this.PostalCode = addressData.ContainsKey(Fields.PostalCode) ? addressData[Fields.PostalCode] as string : null;
            this.Country = addressData.ContainsKey(Fields.Country) ? addressData[Fields.Country] as string : null;
            this.Region = addressData.ContainsKey(Fields.Region) ? addressData[Fields.Region] as string : null;

            this.FirstName = addressData.ContainsKey(Fields.FirstName) ? addressData[Fields.FirstName] as string : null;
            this.LastName = addressData.ContainsKey(Fields.LastName) ? addressData[Fields.LastName] as string : null;
            this.PhoneNumber = addressData.ContainsKey(Fields.PhoneNumber) ? addressData[Fields.PhoneNumber] as string : null;

            string isCustomerConsented = addressData.ContainsKey(Fields.IsCustomerConsented) ? Convert.ToString(addressData[Fields.IsCustomerConsented]) : string.Empty;
            this.IsCustomerConsented = string.IsNullOrEmpty(isCustomerConsented) ? false : Convert.ToBoolean(isCustomerConsented);

            string isAVSFullValidationSucceeded = addressData.ContainsKey(Fields.IsAVSFullValidationSucceeded) ? Convert.ToString(addressData[Fields.IsAVSFullValidationSucceeded]) : string.Empty;
            this.IsAVSFullValidationSucceeded = string.IsNullOrEmpty(isAVSFullValidationSucceeded) ? false : Convert.ToBoolean(isAVSFullValidationSucceeded);

            string defaultShipping = addressData.ContainsKey(PXFields.DefaultShippingAddress) ? Convert.ToString(addressData[PXFields.DefaultShippingAddress]) : string.Empty;
            this.DefaultShippingAddress = string.IsNullOrEmpty(defaultShipping) ? false : Convert.ToBoolean(defaultShipping);

            string defaultBilling = addressData.ContainsKey(PXFields.DefaultBillingAddress) ? Convert.ToString(addressData[PXFields.DefaultBillingAddress]) : string.Empty;
            this.DefaultBillingAddress = string.IsNullOrEmpty(defaultBilling) ? false : Convert.ToBoolean(defaultBilling);
        }

        [JsonProperty(PropertyName = PXFields.DefaultShippingAddress)]
        public bool DefaultShippingAddress { get; set; }

        [JsonProperty(PropertyName = PXFields.DefaultBillingAddress)]
        public bool DefaultBillingAddress { get; set; }

        [JsonProperty(PropertyName = PXFields.IsUserEntered)]
        public bool IsUserEntered { get; set; }

        public Dictionary<string, string> GetPropertyDictionaryWithId(string id)
        {
            var propertyDictionary = this.GetPropertyDictionary();

            propertyDictionary[Fields.Id] = id;

            return propertyDictionary;
        }

        public override Dictionary<string, string> GetPropertyDictionary()
        {
            var propertyDictionary = base.GetPropertyDictionary();

            propertyDictionary[PXFields.DefaultShippingAddress] = this.DefaultShippingAddress ? "true" : "false";
            propertyDictionary[PXFields.DefaultBillingAddress] = this.DefaultBillingAddress ? "true" : "false";
            propertyDictionary[Fields.Id] = this.Id;

            return propertyDictionary;
        }

        public Dictionary<string, object> GetPropertyDictionaryForAVSAddress()
        {
            Dictionary<string, object> propertyDictionary = new Dictionary<string, object>();
            propertyDictionary[AddressInfoV3.Fields.Region] = this.Region;
            propertyDictionary[AddressInfoV3.Fields.City] = this.City;
            propertyDictionary[AddressInfoV3.Fields.AddressLine1] = this.AddressLine1;
            propertyDictionary[AddressInfoV3.Fields.AddressLine2] = this.AddressLine2;
            propertyDictionary[AddressInfoV3.Fields.AddressLine3] = this.AddressLine3;
            propertyDictionary[AddressInfoV3.Fields.PostalCode] = this.PostalCode;
            propertyDictionary[AddressInfoV3.Fields.IsCustomerConsented] = this.IsCustomerConsented;
            propertyDictionary[AddressInfoV3.Fields.IsAVSFullValidationSucceeded] = this.IsAVSFullValidationSucceeded;

            return propertyDictionary;
        }

        private static Dictionary<string, object> GetNestedAddressData(Dictionary<string, object> pidlData, Dictionary<string, object> addressData, string key)
        {
            var addressObj = addressData[key] as JObject;
            addressData = addressObj.ToObject<Dictionary<string, object>>();

            // Get top level data from original addressData parameter and insert into new addressData object
            if (pidlData.ContainsKey(PXFields.DefaultShippingAddress))
            {
                var defaultShippingValue = pidlData[PXFields.DefaultShippingAddress];
                addressData[PXFields.DefaultShippingAddress] = defaultShippingValue;
            }

            if (pidlData.ContainsKey(PXFields.DefaultBillingAddress))
            {
                var defaultBillingValue = pidlData[PXFields.DefaultBillingAddress];
                addressData[PXFields.DefaultBillingAddress] = defaultBillingValue;
            }

            return addressData;
        }

        private static class PXFields
        {
            internal const string DefaultShippingAddress = "set_as_default_shipping_address";
            internal const string DefaultBillingAddress = "set_as_default_billing_address";
            internal const string AddressShippingV3 = "addressShippingV3";
            internal const string AddressBillingV3 = "addressBillingV3";
            internal const string IsUserEntered = "is_user_entered";
        }
    }
}
