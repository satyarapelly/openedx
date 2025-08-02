// <copyright file="Violation.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System.Runtime.Serialization;
    using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

    [HasSelfValidation]
    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class Violation : IExtensibleDataObject
    {
        #region IExtensibleDataObject members
        private ExtensionDataObject _extensionData;
        public ExtensionDataObject ExtensionData
        {
            get { return _extensionData; }
            set { _extensionData = value; }
        }
        #endregion

        [DataMember]
        public int ViolationID { get; set; }

        [OutputProperty(Tag = "Violation.Name")]
        [DataMember]
        public string Name { get; set; }
    }
}
