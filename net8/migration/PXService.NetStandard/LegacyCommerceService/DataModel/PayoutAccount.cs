// <copyright file="PayoutAccount.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System.Runtime.Serialization;

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class PayoutAccount : Account, IExtensibleDataObject
    {


    }
}
