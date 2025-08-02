// <copyright file="Address.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System;
    using System.Runtime.Serialization;
    using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
    using System.Collections.Generic;

    [DataContract(Namespace = NamespaceConstants.Namespace), Serializable]
    public class Address : IExtensibleDataObject
    {
        #region IExtensibleDataObject members
        [NonSerialized]
        private ExtensionDataObject _extensionData;
        public ExtensionDataObject ExtensionData
        {
            get { return _extensionData; }
            set { _extensionData = value; }
        }
        #endregion

        [IgnoreNulls, StringLengthValidator(16, 16, Tag = "Address.AddressID")]
        [DataMember]
        public string AddressID { get; set; }

        [IgnoreNulls, StringLengthValidator(64, Tag = "Address.FriendlyName")]
        [RegexValidator(RegexConstants.XmlString, Tag = "Address.FriendlyName")]
        [DataMember]
        public string FriendlyName { get; set; }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string FirstNamePronunciation { get; set; }

        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public string LastNamePronunciation { get; set; }

        [DataMember]
        public string CompanyName { get; set; }
        [DataMember]
        public string CompanyNamePronunciation { get; set; }

        [IgnoreNulls, StringLengthValidator(0, 15, Tag = "Address.UnitNumber")]
        [RegexValidator(RegexConstants.XmlString, Tag = "Address.UnitNumber")]
        [DataMember]
        public string UnitNumber { get; set; }

        [StringLengthValidator(1, 128, Tag = "Address.Street1")]
        [RegexValidator(RegexConstants.XmlString, Tag = "Address.Street1")]
        [DataMember]
        public string Street1 { get; set; }

        [IgnoreNulls, StringLengthValidator(128, Tag = "Address.Street2")]
        [RegexValidator(RegexConstants.XmlString, Tag = "Address.Street2")]
        [DataMember]
        public string Street2 { get; set; }

        [IgnoreNulls, StringLengthValidator(128, Tag = "Address.Street3")]
        [RegexValidator(RegexConstants.XmlString, Tag = "Address.Street3")]
        [DataMember]
        public string Street3 { get; set; }

        [StringLengthValidator(1, 64, Tag = "Address.City")]
        [RegexValidator(RegexConstants.XmlString, Tag = "Address.City")]
        [DataMember]
        public string City { get; set; }

        [IgnoreNulls, StringLengthValidator(64, Tag = "Address.District")]
        [RegexValidator(RegexConstants.XmlString, Tag = "Address.District")]
        [DataMember]
        public string District { get; set; }

        [IgnoreNulls, StringLengthValidator(64, Tag = "Address.State")]
        [RegexValidator(RegexConstants.XmlString, Tag = "Address.State")]
        [DataMember]
        public string State { get; set; }

        [IgnoreNulls, StringLengthValidator(2, 2, Tag = "Address.CountryCode")]
        [RegexValidator(RegexConstants.CountryCode, Tag = "Address.CountryCode")]
        [DataMember]
        public string CountryCode { get; set; }

        [IgnoreNulls, StringLengthValidator(16, Tag = "Address.PostalCode")]
        [DataMember]
        public string PostalCode { get; set; }

        [IgnoreNulls, ObjectValidator(Tag = "Address.MapAddressResult")]
        [DataMember]
        public MapAddressResult MapAddressResult { get; set; }

        public Dictionary<string, string> GetPropertyDictionary()
        {
            Dictionary<string, string> propertyDictionary = new Dictionary<string, string>();
            propertyDictionary[Address.Fields.Country] = this.CountryCode;
            propertyDictionary[Address.Fields.Region] = this.State;
            propertyDictionary[Address.Fields.District] = this.District;
            propertyDictionary[Address.Fields.City] = this.City;
            propertyDictionary[Address.Fields.AddressLine1] = this.Street1;
            propertyDictionary[Address.Fields.AddressLine2] = this.Street2;
            propertyDictionary[Address.Fields.AddressLine3] = this.Street3;
            propertyDictionary[Address.Fields.PostalCode] = this.PostalCode;
            return propertyDictionary;
        }

        private static class Fields
        {
            internal const string Country = "country";
            internal const string Region = "region";
            internal const string District = "district";
            internal const string City = "city";
            internal const string AddressLine1 = "address_line1";
            internal const string AddressLine2 = "address_line2";
            internal const string AddressLine3 = "address_line3";
            internal const string PostalCode = "postal_code";
        }
    }
}
