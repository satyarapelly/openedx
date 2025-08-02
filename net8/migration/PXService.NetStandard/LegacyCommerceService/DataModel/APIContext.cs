// <copyright file="APIContext.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class APIContext : IExtensibleDataObject
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
        public Guid TrackingGuid { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Legacy code. Should be thrown away once modernAPI is available")]
        [ObjectCollectionValidator(typeof(Property))]
        [PropertyCollectionValidator]
        [DataMember]
        public List<Property> PropertyBag { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Legacy code. Should be thrown away once modernAPI is available")]
        [ObjectCollectionValidator(typeof(Property))]
        [PropertyCollectionValidator]
        [DataMember]
        public List<Property> FraudDetectionContext { get; set; }

        [DataMember]
        public DeviceInfo DeviceInfo { get; set; }

        /// <summary>
        /// It is used for API allowing partner pass Guid.Empty while we need one tracking guid to identify the call.
        /// </summary>
        public Guid GetOrCreateTrackingGuid()
        {
            if (TrackingGuid == Guid.Empty)
            {
                TrackingGuid = Guid.NewGuid();
            }

            return TrackingGuid;
        }
    }
}
