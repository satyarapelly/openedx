// <copyright file="CallerInfo.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System;
    using System.Runtime.Serialization;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.Practices.EnterpriseLibrary.Validation;
    using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class CallerInfo : IExtensibleDataObject
    {
        #region IExtensibleDataObject members
        private ExtensionDataObject _extensionData;
        public ExtensionDataObject ExtensionData
        {
            get { return _extensionData; }
            set { _extensionData = value; }
        }
        #endregion

        [ObjectValidator]
        [DataMember]
        public Identity Delegator { get; set; }

        [ObjectValidator]
        [DataMember]
        public Identity Requester { get; set; }

        [IgnoreNulls]
        [StringLength(16, MinimumLength = 16)]
        [DataMember]
        public string AccountId { get; set; }

        [SelfValidation]
        public void Validate(ValidationResults results)
        {

            if (Delegator == null && Requester == null)
            {
                results.AddResult(new ValidationResult(
                    "Delegator or Requester is required.",
                    this,
                    "Delegator and Requester",
                    "CallerInfo",
                    null));
            }
            if (Delegator != null && !string.Equals(Delegator.IdentityType, "PUID", StringComparison.OrdinalIgnoreCase))
            {
                results.AddResult(new ValidationResult(
                    "Delegator only supports PUID Identity.",
                    this,
                    "Delegator",
                    "CallerInfo",
                    null));
            }
        }

    }
}
