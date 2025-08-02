// <copyright file="AccountSearchCriteria.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System.Runtime.Serialization;
    using System.ComponentModel.DataAnnotations;
    using System.Collections.Generic;

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class AccountSearchCriteria : IExtensibleDataObject, IValidatableObject
    {
        #region IExtensibleDataObject members
        private ExtensionDataObject _extensionData;
        public ExtensionDataObject ExtensionData
        {
            get { return _extensionData; }
            set { _extensionData = value; }
        }
        #endregion

        [StringLength(16, MinimumLength = 16)]
        [DataMember]
        public string AccountId { get; set; }

        [DataMember]
        public Identity Identity { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (AccountId == null && Identity == null)
            {
                yield return new ValidationResult(
                    "No search criteria specified.",
                    new[] { nameof(AccountId), nameof(Identity) });
            }

            string criteriaSpecified = null;
            if (AccountId != null)
            {
                if (criteriaSpecified != null)
                {
                    yield return new ValidationResult(
                        "More than one search criteria specified: AccountID and " + criteriaSpecified,
                        new[] { nameof(AccountId) });
                }
                criteriaSpecified = "AccountID";
            }
            if (Identity != null)
            {
                if (criteriaSpecified != null)
                {
                    yield return new ValidationResult(
                        "More than one search criteria specified: Identity and " + criteriaSpecified,
                        new[] { nameof(Identity) });
                }
                criteriaSpecified = "Identity";
            }
        }
    }
}

