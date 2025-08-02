// <copyright file="GetSubscriptionsResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.Messages
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel;

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class GetSubscriptionsResponse : AbstractResponse
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Legacy code")]
        [DataMember]
        public List<SubscriptionsInfo> SubscriptionInfoList { get; set; }

        [DataMember]
        public int TotalCount { get; set; }
    }
}
