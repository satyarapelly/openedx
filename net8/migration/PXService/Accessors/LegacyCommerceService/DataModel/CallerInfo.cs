// <copyright file="CallerInfo.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System;
    using System.Runtime.Serialization;
    using System.ComponentModel.DataAnnotations;
    using System.Collections.Generic;

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class CallerInfo : IExtensibleDataObject, IValidatableObject
    {
        #region IExtensibleDataObject members
        private ExtensionDataObject _extensionData;
        public ExtensionDataObject ExtensionData
        {
            get { return _extensionData; }
            set { _extensionData = value; }
        }
        #endregion

        [ValidateComplexType]
        [DataMember]
        public Identity Delegator { get; set; }

        [ValidateComplexType]
        [DataMember]
        public Identity Requester { get; set; }

        [StringLength(16, MinimumLength = 16)]
        [DataMember]
        public string AccountId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Delegator == null && Requester == null)
            {
                yield return new ValidationResult(
                    "Delegator or Requester is required.",
                    new[] { nameof(Delegator), nameof(Requester) });
            }
            if (Delegator != null && !string.Equals(Delegator.IdentityType, "PUID", StringComparison.OrdinalIgnoreCase))
            {
                yield return new ValidationResult(
                    "Delegator only supports PUID Identity.",
                    new[] { nameof(Delegator) });
            }
        }

    }
}
