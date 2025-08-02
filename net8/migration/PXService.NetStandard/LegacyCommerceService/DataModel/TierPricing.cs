// <copyright file="TierPricing.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System.Runtime.Serialization;

    public class TierPricing
    {
        [DataMember]
        public int MinValue { get; set; }

        [DataMember]
        public string ChargeAmount { get; set; }
    }
}
