// <copyright file="MapAddressResult.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System;
    using System.Runtime.Serialization;

    [DataContract(Namespace = NamespaceConstants.Namespace), Serializable]
    public class MapAddressResult : IExtensibleDataObject
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

        [DataMember]
        public bool? AddressMapAttempted { get; set; }

        [DataMember]
        public bool? AddressMapSucceeded { get; set; }

        [DataMember]
        public byte? AddressMapFailureReason { get; set; }

        [DataMember]
        public double? AddressMapConfidenceScore { get; set; }

        [DataMember]
        public bool? ManualAddressSpecified { get; set; }
    }
}
