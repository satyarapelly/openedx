// <copyright file="AckCodeType.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System.Runtime.Serialization;

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public enum AckCodeType
    {
        /// <remarks/>
        [EnumMember]
        Success,

        /// <remarks/>
        [EnumMember]
        RetryableFailure,

        /// <remarks/>
        [EnumMember]
        NonRetryableFailure,

        /// <remarks/>
        [EnumMember]
        InternalError,
    }
}
