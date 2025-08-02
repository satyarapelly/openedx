// <copyright file="AbstractResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel;

    [Serializable]
    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public abstract class AbstractResponse : IExtensibleDataObject
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

        public AckCodeType Ack { get; set; }

        public ErrorType Error { get; set; }

        [DataMember]
        public bool IsSameTrackingGuidRetry { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed for serialization purpose.")]
        [DataMember]
        public List<Property> PropertyBag { get; set; }

        public Guid ResponseGuid { get; set; }

        public virtual string ResponseName
        {
            get { return this.GetType().Name; }
            set { }
        }

        public DataAccessorTracerResult TracerResult { get; set; }
    }
}
