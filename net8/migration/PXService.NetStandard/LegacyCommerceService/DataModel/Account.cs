// <copyright file="Account.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using System.ComponentModel.DataAnnotations;

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public enum AccountStatus
    {
        [EnumMember]
        Active = 1,
        [EnumMember]
        Locked = 2,
        [EnumMember]
        Closed = 5,
    }

    [KnownType(typeof(PayinAccount))]
    [KnownType(typeof(PayoutAccount))]
    [XmlInclude(typeof(PayinAccount))]
    [XmlInclude(typeof(PayoutAccount))]
    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class Account : IExtensibleDataObject, IValidatableObject
    {
        #region IExtensibleDataObject members
        protected ExtensionDataObject _extensionData;
        public ExtensionDataObject ExtensionData
        {
            get { return _extensionData; }
            set { _extensionData = value; }
        }
        #endregion

        private string _accountID;

        [DataMember]
        public string AccountID
        {
            get { return _accountID; }
            set
            {
                _accountID = value;
                _billableAccountID = 0;     // reset billable account ID to force update next time 
            }
        }

        [DataMember]
        public string AccountLevel { get; set; }

        [DataMember]
        public string AccountRole { get; set; }

        [StringLength(64)]
        [RegularExpression(RegexConstants.XmlString)]
        [DataMember]
        public string FriendlyName { get; set; }

        [StringLength(129)]
        [RegularExpression(RegexConstants.Email)]
        [DataMember]
        public string Email { get; set; }

        [StringLength(11, MinimumLength = 5)]
        [RegularExpression(RegexConstants.Locale)]
        [DataMember]
        public string Locale { get; set; }

        [StringLength(3, MinimumLength = 3)]
        [RegularExpression(RegexConstants.Currency)]
        [DataMember]
        public string Currency { get; set; }

        [StringLength(2, MinimumLength = 2)]
        [RegularExpression(RegexConstants.CountryCode)]
        [DataMember]
        public string CountryCode { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819", Justification = "Legacy code moved from PCS. Needed for serialization")]
        [DataMember]
        public Property[] CustomPropertiesField { get; set; }

        #region output properties, must be null or default value when this type is used for input

        [OutputProperty]
        [DataMember]
        public AccountStatus? Status { get; set; }

        [OutputProperty]
        [DataMember]
        public DateTime CreatedDate { get; set; }

        [OutputProperty]
        [DataMember]
        public DateTime LastUpdatedDate { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Legacy code. Should be thrown away once modernAPI is available")]
        [OutputProperty]
        // TODO: validate Violations items
        [DataMember]
        public List<Violation> Violations { get; set; }

        #endregion


        // Internal use
        private long _billableAccountID;
        public long BillableAccountID
        {
            get
            {
                if (_billableAccountID == 0)
                {
                    UpdateBillableAccountID();
                }
                return _billableAccountID;
            }
        }

        private void UpdateBillableAccountID()
        {
            if (!string.IsNullOrEmpty(AccountID))
            {
                BdkId bdk = new BdkId(AccountID);
                _billableAccountID = bdk.AccountId;
            }
        }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }
}
