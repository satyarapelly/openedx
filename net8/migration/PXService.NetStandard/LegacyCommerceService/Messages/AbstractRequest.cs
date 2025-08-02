// <copyright file="AbstractRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.Messages
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel;

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public abstract class AbstractRequest : IExtensibleDataObject
    {
        #region IExtensibleDataObject members
        private ExtensionDataObject _extensionData;
        public ExtensionDataObject ExtensionData
        {
            get { return _extensionData; }
            set { _extensionData = value; }
        }
        #endregion

        [XmlIgnore]
        public virtual string RequestName
        {
            get { return this.GetType().Name; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Needed for serialization purpose.")]
        [XmlIgnore]
        public virtual ulong DelegaterId { get; private set; }

        [XmlIgnore]
        public virtual Identity Requester { get; set; }

        [XmlIgnore]
        public virtual Identity Delegater { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Needed for serialization purpose.")]
        [XmlIgnore]
        public virtual string ObjectId { get; private set; }

        [XmlIgnore]
        public abstract int ApiId { get; }

        [XmlIgnore]
        public virtual bool NeedTrackingGuidSupport { get { return false; } }

        [XmlIgnore]
        public virtual Guid EffectiveTrackingGuid { get { return Guid.Empty; } }

        [DataMember(EmitDefaultValue = false)]
        public virtual Guid OnBehalfOfPartnerGuid { get; set; }
    }
}
