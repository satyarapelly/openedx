// <copyright file="Property.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System;
    using System.Runtime.Serialization;
    using System.ComponentModel.DataAnnotations;
    using System.Collections.Generic;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716", Justification = "Legacy code moved from PCS. Needed for serialization")]
    [DataContract(Namespace = NamespaceConstants.Namespace), Serializable]
    public class Property : IExtensibleDataObject, IValidatableObject
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

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(Namespace) && string.IsNullOrEmpty(Name))
            {
                yield return new ValidationResult(
                    "Namespace and Name in a Property can not be both empty.",
                    new[] { nameof(Namespace), nameof(Name) });
            }
        }
    }
}

