// <copyright file="AddressResult.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using System.Collections.Generic;

    public class AddressResult
    {
        public string AddressType { get; set; }

        public IList<Address> Address { get; }
    }
}