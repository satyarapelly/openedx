// <copyright file="PagingOption.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System.Runtime.Serialization;

    public class PagingOption
    {
        /// <summary>
        /// The offset of the return value. Work with <c>Limit</c> property.
        /// </summary>
        /// <remarks>
        /// Note this property is nullable, but is not intended, it's for backward compatibility.
        /// </remarks>
        [DataMember]
        public int Offset { get; set; }

        /// <summary>
        /// The max size to need to be returned. Work with <c>Offset</c> property.
        /// </summary>
        /// <remarks>
        /// Note this property is nullable, but is not intended, it's for backward compatibility.
        /// </remarks>
        [DataMember]
        public int Limit { get; set; }
    }
}
