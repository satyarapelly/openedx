// <copyright file="Address.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System;
    using System.Runtime.Serialization;
    using System.ComponentModel.DataAnnotations;
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

        [StringLength(16, MinimumLength = 16)]
        [DataMember]
        public string AddressID { get; set; }

        [StringLength(64)]
        [RegularExpression(RegexConstants.XmlString)]
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

        [StringLength(15)]
        [RegularExpression(RegexConstants.XmlString)]
        [DataMember]
        public string UnitNumber { get; set; }

        [StringLength(128, MinimumLength = 1)]
        [RegularExpression(RegexConstants.XmlString)]
        [DataMember]
        public string Street1 { get; set; }

        [StringLength(128)]
        [RegularExpression(RegexConstants.XmlString)]
        [DataMember]
        public string Street2 { get; set; }

        [StringLength(128)]
        [RegularExpression(RegexConstants.XmlString)]
        [DataMember]
        public string Street3 { get; set; }

        [StringLength(64, MinimumLength = 1)]
        [RegularExpression(RegexConstants.XmlString)]
        [DataMember]
        public string City { get; set; }

        [StringLength(64)]
        [RegularExpression(RegexConstants.XmlString)]
        [DataMember]
        public string District { get; set; }

        [StringLength(64)]
        [RegularExpression(RegexConstants.XmlString)]
        [DataMember]
        public string State { get; set; }

        [StringLength(2, MinimumLength = 2)]
        [RegularExpression(RegexConstants.CountryCode)]
        [DataMember]
        public string CountryCode { get; set; }

        [StringLength(16)]
        [DataMember]
        public string PostalCode { get; set; }

        // TODO: validate MapAddressResult
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
