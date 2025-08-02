// <copyright file="Property.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System;
    using System.Runtime.Serialization;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.Practices.EnterpriseLibrary.Validation;
    using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716", Justification = "Legacy code moved from PCS. Needed for serialization")]
    [HasSelfValidation]
    [DataContract(Namespace = NamespaceConstants.Namespace), Serializable]
    public class Property : IExtensibleDataObject
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

        [Required]
        [DataMember]
        public string Namespace { get; set; }

        [Required]
        [DataMember]
        public string Name { get; set; }

        [Required]
        [DataMember]
        public string Value { get; set; }

        [SelfValidation]
        public void Validate(ValidationResults results)
        {
            if (string.IsNullOrEmpty(Namespace) && string.IsNullOrEmpty(Name))
            {
                results.AddResult(new ValidationResult(
                    "Namespace and Name in a Property can not be both empty.",
                    this,
                    "Namespace and Name",
                    "Property",
                    null));
            }
        }
    }
}

