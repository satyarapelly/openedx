// <copyright file="Identity.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Identity represents a certain type of ID. 
    /// </summary>
    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class    Identity : IExtensibleDataObject, IValidatableObject
    {
        #region IExtensibleDataObject members
        private ExtensionDataObject _extensionData;
        public ExtensionDataObject ExtensionData
        {
            get { return _extensionData; }
            set { _extensionData = value; }
        }
        #endregion

        [StringLength(32)]
        [DataMember]
        public string IdentityType { get; set; }

        [StringLength(64, MinimumLength = 1)]
        [DataMember]
        public string IdentityValue { get; set; }

        [DataMember]
        public string PassportMemberName { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(IdentityType))
            {
                yield return new ValidationResult("IdentityType is empty.", new[] { nameof(IdentityType) });
                yield break;
            }

            if (!_identityTypes.ContainsKey(IdentityType))
            {
                yield return new ValidationResult(
                    string.Format("IdentityType {0} is not valid. Accepted: {1}.",
                        IdentityType, string.Join(",", _identityTypes.Keys.OrderBy(k => k).ToArray())),
                    new[] { nameof(IdentityType) });
                yield break;
            }

            foreach (var result in _identityTypes[IdentityType].Validate(this))
            {
                yield return result;
            }
        }


        #region identity types

        private static Dictionary<string, IIdentityType> _identityTypes = new Dictionary<string, IIdentityType>(System.StringComparer.OrdinalIgnoreCase)
        {
            { "PUID", new PuidIdentityType() }
           
        };

        private interface IIdentityType
        {
            IEnumerable<ValidationResult> Validate(Identity id);
        }

        private class PuidIdentityType : IIdentityType
        {

            public IEnumerable<ValidationResult> Validate(Identity id)
            {
                if (!ulong.TryParse(id.IdentityValue, out _))
                {
                    yield return new ValidationResult("IdentityValue is not a valid ulong.", new[] { nameof(IdentityValue) });
                }
            }


        }

        #endregion

        public override bool Equals(object obj)
        {
            var id = obj as Identity;

            return id != null
                && string.Equals(id.IdentityType, this.IdentityType, System.StringComparison.OrdinalIgnoreCase)
                && string.Equals(id.IdentityValue, this.IdentityValue) /* case sensitive */;
        }

        public override int GetHashCode()
        {
            return (this.IdentityType ?? string.Empty).ToLowerInvariant().GetHashCode() ^
                (this.IdentityValue ?? string.Empty).GetHashCode();
        }

        public static bool operator ==(Identity a, Identity b)
        {
            if (object.ReferenceEquals(a, null))
                return object.ReferenceEquals(b, null);
            return a.Equals(b);
        }

        public static bool operator !=(Identity a, Identity b)
        {
            // calls to operator ==
            return !(a == b);
        }
    }
}

