// <copyright file="GetAccountIdFromPaymentInstrumentInfoRequest.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.Messages
{
    using System.Runtime.Serialization;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel;
    using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class GetAccountIdFromPaymentInstrumentInfoRequest : AbstractRequest
    {
        [NotNullValidator(Tag = "GetAccountIdFromPaymentInstrumentInfoRequest")]
        [ObjectValidator(Tag = "GetAccountIdFromPaymentInstrumentInfoRequest")]
        [DataMember]
        public APIContext APIContext { get; set; }

        public override int ApiId
        {
            get { return (int)DataAccessorType.GetAccountIdFromPaymentInstrumentInfo; }
        }

        [IgnoreNulls]
        [DataMember]
        public CallerInfo CallerInfo { get; set; }

        [DataMember]
        public string PaymentInstrumentType { get; set; }

        [DataMember]
        public string CreditCardType { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819", Justification = "Legacy code moved from PCS. Needed for serialization")]
        [DataMember]
        public byte[] EncryptedAccountNumber { get; set; }

    }
}
