// <copyright file="GetSubscriptionsRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;    
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel;

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class GetSubscriptionsRequest : AbstractRequest
    {
        public override int ApiId
        {
            get { return (int)DataAccessorType.GetSubscriptions; }
        }

        [XmlIgnore]
        public override Identity Requester
        {
            get
            {
                return CallerInfo == null ? null : CallerInfo.Requester;
            }
        }

        [XmlIgnore]
        public override Identity Delegater
        {
            get
            {
                return CallerInfo == null ? null : CallerInfo.Delegator;
            }
        }

        [DataMember]
        public APIContext APIContext { get; set; }

        [DataMember]
        public CallerInfo CallerInfo { get; set; }

        [DataMember]
        public GetCTPSubscriptionsDetailLevel DetailLevel { get; set; }

        [DataMember]
        public bool GetSubscriptionsOfAllPartners { get; set; }

        [DataMember]
        public string SubscriptionId { get; set; }

        [DataMember]
        public string InvoiceGroupId { get; set; }

        [DataMember]
        public PagingOption PagingOption { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Legacy code")]
        [DataMember]
        public List<GetSubscriptionsOrderBy> OrderBy { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Legacy code")]
        [DataMember]
        public List<string> SubscriptionStatus { get; set; }

        [DataMember]
        public bool? IsPerpetualOffer { get; set; }

        [XmlIgnore]
        public override Guid EffectiveTrackingGuid
        {
            get { return APIContext == null ? Guid.Empty : APIContext.TrackingGuid; }
        }
    }
}
