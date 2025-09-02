// <copyright file="TaxExemption.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System;
    using System.Runtime.Serialization;

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class TaxExemption : IExtensibleDataObject
    {
        [DataMember]
        public string CertificateNumber { get; set; }
        [DataMember]
        public DateTime? DateAdded { get; set; }
        [DataMember]
        public DateTime? DateReceived { get; set; }
        [DataMember]
        public DateTime? ExpDate { get; set; }
        public ExtensionDataObject ExtensionData { get; set; }
        [DataMember]
        public TaxExemptionStatus? Status { get; set; }
        [DataMember]
        public TaxExemptionType? TaxExemptionType { get; set; }
    }

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public enum TaxExemptionStatus
    {
        [EnumMember]
        Valid = 1,
        [EnumMember]
        Invalid = 2,
        [EnumMember]
        Pending = 3,
        [EnumMember]
        PastDue = 4,
        [EnumMember]
        Expired = 5,
    }

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public enum TaxExemptionType
    {
        [EnumMember]
        VATID = 1,
        [EnumMember]
        USExempt = 2,
        [EnumMember]
        CanadianFederalExempt = 3,
        [EnumMember]
        CanadianProvinceExempt = 4,
        [EnumMember]
        BrazilCNPJID = 5,
        [EnumMember]
        BrazilCPFID = 6,
        [EnumMember]
        BrazilCCMID = 7,
    }
}
