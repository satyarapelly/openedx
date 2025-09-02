// <copyright file="GetSubscriptionsOrderBy.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System.Runtime.Serialization;

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public enum GetSubscriptionsOrderBy : int
    {
        [EnumMember]
        PurchaseDateDesc = 0,

        [EnumMember]
        PurchaseDateAsc = 1,

        [EnumMember]
        FriendlyNameDesc = 2,

        [EnumMember]
        FriendlyNameAsc = 3
    }
}
