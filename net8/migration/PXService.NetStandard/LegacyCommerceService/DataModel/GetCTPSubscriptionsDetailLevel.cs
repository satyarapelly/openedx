// <copyright file="GetCTPSubscriptionsDetailLevel.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System.Runtime.Serialization;
    
    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public enum GetCTPSubscriptionsDetailLevel : int
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        RatingRules = 1,

        [EnumMember]
        AllRatingRules = 2
    }
}
