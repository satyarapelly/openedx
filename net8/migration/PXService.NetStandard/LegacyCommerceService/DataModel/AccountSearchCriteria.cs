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

        // TODO: validate Identity
        [DataMember]
        public Identity Identity { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            if (AccountId == null && Identity == null)
            {
                results.Add(new ValidationResult("No search criteria specified.", new[] { nameof(AccountId), nameof(Identity) }));
            }

            string criteriaSpecified = null;
            if (AccountId != null)
            {
                criteriaSpecified = ValidateCriteria(nameof(AccountId), criteriaSpecified, results);
            }
            if (Identity != null)
            {
                criteriaSpecified = ValidateCriteria(nameof(Identity), criteriaSpecified, results);
            }

            return results;
        }

        private string ValidateCriteria(string currentCriteria, string specifiedCriteria, ICollection<ValidationResult> results)
        {
            if (specifiedCriteria == null)
            {
                return currentCriteria;
            }

            results.Add(new ValidationResult(
                $"More than one search criteria specified: {currentCriteria} and {specifiedCriteria}.",
                new[] { currentCriteria }));
            return specifiedCriteria;
        }
    }
}

