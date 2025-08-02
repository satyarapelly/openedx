// <copyright file="Phone.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System;
    using System.Runtime.Serialization;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public enum PhoneType
    {
        [EnumMember]
        Fax = 1,
        [EnumMember]
        Home = 2,
        [EnumMember]
        Mobile = 3,
        [EnumMember]
        Primary = 4,
        [EnumMember]
        Work = 5,
        [EnumMember]
        Alternate = 6,
    }

    [DataContract(Namespace = NamespaceConstants.Namespace), Serializable]
    public class Phone : IExtensibleDataObject
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

        [IgnoreNulls]
        [DataMember]
        public PhoneType? PhoneType { get; set; }

        [IgnoreNulls]
        [StringLength(12)]
        [DataMember]
        public string PhonePrefix { get; set; }

        [IgnoreNulls]
        [StringLength(32)]
        [DataMember]
        public string PhoneNumber { get; set; }

        [IgnoreNulls]
        [StringLength(12)]
        [DataMember]
        public string PhoneExtension { get; set; }

        [IgnoreNulls]
        [StringLength(2, MinimumLength = 2)]
        [RegularExpression(RegexConstants.CountryCode)]
        [DataMember]
        public string CountryCode { get; set; }
    }
}
