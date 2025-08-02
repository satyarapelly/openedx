// <copyright file="CreateAccountResponse.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.Messages
{
    using System.Runtime.Serialization;
    using Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel;

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class CreateAccountResponse : AbstractResponse
    {
        [DataMember]
        public PayinPayoutAccount Account { get; set; }
    }
}
