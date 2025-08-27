// <copyright file="AccountSearchCriteria.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System.Runtime.Serialization;
    using Microsoft.Practices.EnterpriseLibrary.Validation;
    using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

    [HasSelfValidation]
    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class AccountSearchCriteria : IExtensibleDataObject
    {
        #region IExtensibleDataObject members
        private ExtensionDataObject _extensionData;
        public ExtensionDataObject ExtensionData
        {
            get { return _extensionData; }
            set { _extensionData = value; }
        }
        #endregion

        [IgnoreNulls, StringLengthValidator(16, 16,
            MessageTemplate = "AccountId:{0} must be 16 characters",
            Tag = "AccountSearchCriteria.AccountID")]
        [DataMember]
        public string AccountId { get; set; }

        [ObjectValidator(Tag = "AccountSearchCriteria.Identity")]
        [DataMember]
        public Identity Identity { get; set; }

        [SelfValidation]
        public void Validate(ValidationResults results)
        {
            if (AccountId == null && Identity == null)
            {
                results.AddResult(new ValidationResult(
                    "No search criteria specified.",
                    this, "AccountID and Identity", "AccountSearchCriteria", null));
            }

            string criteriaSpecified = null;
            if (AccountId != null)
            {
                criteriaSpecified = ValidateCriteria("AccountID", criteriaSpecified, results);
            }
            if (Identity != null)
            {
                criteriaSpecified = ValidateCriteria("Identity", criteriaSpecified, results);
            }
        }

        private string ValidateCriteria(string currentCriteria, string specifiedCriteria, ValidationResults results)
        {
            if (specifiedCriteria == null)
            {
                return currentCriteria;
            }
            else
            {
                results.AddResult(new ValidationResult(
                    "More than one search criteria specified: " + currentCriteria + " and " + specifiedCriteria,
                    this, currentCriteria, "AccountSearchCriteria", null));
                return specifiedCriteria;
            }
        }
    }
}

