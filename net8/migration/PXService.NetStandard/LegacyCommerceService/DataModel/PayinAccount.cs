// <copyright file="PayinAccount.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.ComponentModel.DataAnnotations;
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


        [IgnoreNulls]
        [StringLength(64)]
        [RegularExpression(RegexConstants.XmlString)]
        [DataMember]
        public string FirstName { get; set; }

        [IgnoreNulls]
        [StringLength(64)]
        [RegularExpression(RegexConstants.XmlString)]
        [DataMember]
        public string FirstNamePronunciation { get; set; }

        [IgnoreNulls]
        [StringLength(64)]
        [RegularExpression(RegexConstants.XmlString)]
        [DataMember]
        public string LastName { get; set; }

        [IgnoreNulls]
        [StringLength(64)]
        [RegularExpression(RegexConstants.XmlString)]
        [DataMember]
        public string LastNamePronunciation { get; set; }

        [IgnoreNulls]
        [StringLength(64)]
        [RegularExpression(RegexConstants.XmlString)]
        [DataMember]
        public string CompanyName { get; set; }

        [IgnoreNulls]
        [StringLength(64)]
        [RegularExpression(RegexConstants.XmlString)]
        [DataMember]
        public string CompanyNamePronunciation { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Legacy code. Should be thrown away once modernAPI is available")]
        [Required]
        [ObjectCollectionValidator(typeof(Address))]
        [DataMember]
        public List<Address> AddressSet { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Legacy code. Should be thrown away once modernAPI is available")]
        [Required]
        [ObjectCollectionValidator(typeof(Phone))]
        [DataMember]
        public List<Phone> PhoneSet { get; set; }

        [OutputProperty]
        [DataMember]
        public int AnniversaryDate { get; set; }

        [OutputProperty]
        [DataMember]
        public string DefaultAddressID { get; set; }

        [OutputProperty]
        [DataMember]
        public string CorporateIdentity { get; set; }

        [OutputProperty]
        [DataMember]
        public string CorporateLegalEntity { get; set; }

        [OutputProperty]
        [DataMember]
        public string CorporateVatId { get; set; }

        [OutputProperty]
        [DataMember]
        public Address CorporateAddress { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Legacy code. Should be thrown away once modernAPI is available")]
        [DataMember]
        public List<TaxExemption> TaxExemptionSet { get; set; }
    }
}
