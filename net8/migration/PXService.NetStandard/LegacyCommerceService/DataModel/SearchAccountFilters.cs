// <copyright file="SearchAccountFilters.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System.Runtime.Serialization;

    public class SearchAccountFilters : IExtensibleDataObject
    {
        public ExtensionDataObject ExtensionData { get; set; }

        [DataMember]
        public bool BillableAccountAdminOnly { get; set; }

        [DataMember]
        public CustomerType? CustomerType { get; set; }

        [DataMember]
        public string CountryCode { get; set; }
    }
}

