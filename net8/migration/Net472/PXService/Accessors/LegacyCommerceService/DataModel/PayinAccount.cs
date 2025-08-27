// <copyright file="PayinAccount.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public enum CustomerType
    {
        [EnumMember]
        Personal = 1,
        [EnumMember]
        Business = 2,
        [EnumMember]
        Corporate = 3,
        //This value is PCS internal used and it should not be passed to API
        [EnumMember]
        Unknown = 4,
    }

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class PayinAccount : Account, IExtensibleDataObject
    {
        [IgnoreNulls]
        [DataMember]
        public CustomerType? CustomerType { get; set; }


        [IgnoreNulls, StringLengthValidator(64, Tag = "PayinAccount.FirstName")]
        [RegexValidator(RegexConstants.XmlString, Tag = "PayinAccount.FirstName")]
        [DataMember]
        public string FirstName { get; set; }

        [IgnoreNulls, StringLengthValidator(64, Tag = "PayinAccount.FirstNamePronunciation")]
        [RegexValidator(RegexConstants.XmlString, Tag = "PayinAccount.FirstNamePronunciation")]
        [DataMember]
        public string FirstNamePronunciation { get; set; }

        [IgnoreNulls, StringLengthValidator(64, Tag = "PayinAccount.LastName")]
        [RegexValidator(RegexConstants.XmlString, Tag = "PayinAccount.LastName")]
        [DataMember]
        public string LastName { get; set; }

        [IgnoreNulls, StringLengthValidator(64, Tag = "PayinAccount.LastNamePronunciation")]
        [RegexValidator(RegexConstants.XmlString, Tag = "PayinAccount.LastNamePronunciation")]
        [DataMember]
        public string LastNamePronunciation { get; set; }

        [IgnoreNulls, StringLengthValidator(64, Tag = "PayinAccount.CompanyName")]
        [RegexValidator(RegexConstants.XmlString, Tag = "PayinAccount.CompanyName")]
        [DataMember]
        public string CompanyName { get; set; }

        [IgnoreNulls, StringLengthValidator(64, Tag = "PayinAccount.CompanyNamePronunciation")]
        [RegexValidator(RegexConstants.XmlString, Tag = "PayinAccount.CompanyNamePronunciation")]
        [DataMember]
        public string CompanyNamePronunciation { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Legacy code. Should be thrown away once modernAPI is available")]
        [IgnoreNulls, ElementNotNull, ObjectCollectionValidator(typeof(Address), Tag = "PayinAccount.AddressSet")]
        [DataMember]
        public List<Address> AddressSet { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Legacy code. Should be thrown away once modernAPI is available")]
        [IgnoreNulls, ElementNotNull, ObjectCollectionValidator(typeof(Phone), Tag = "PayinAccount.PhoneSet")]
        [DataMember]
        public List<Phone> PhoneSet { get; set; }

        [OutputProperty(Tag = "PayinAccount.AnniversaryDate")]
        [DataMember]
        public int AnniversaryDate { get; set; }

        [OutputProperty(Tag = "PayinAccount.DefaultAddressID")]
        [DataMember]
        public string DefaultAddressID { get; set; }

        [OutputProperty(Tag = "PayinAccount.CorporateIdentity")]
        [DataMember]
        public string CorporateIdentity { get; set; }

        [OutputProperty(Tag = "PayinAccount.CorporateLegalEntity")]
        [DataMember]
        public string CorporateLegalEntity { get; set; }

        [OutputProperty(Tag = "PayinAccount.CorporateVatId")]
        [DataMember]
        public string CorporateVatId { get; set; }

        [OutputProperty(Tag = "PayinAccount.CorporateAddress")]
        [DataMember]
        public Address CorporateAddress { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Legacy code. Should be thrown away once modernAPI is available")]
        [DataMember]
        public List<TaxExemption> TaxExemptionSet { get; set; }
    }
}
