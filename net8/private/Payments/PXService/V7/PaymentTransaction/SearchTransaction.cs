// <copyright file="SearchTransaction.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7.PaymentTransaction.Model
{
    using System.Collections.Generic;
    using Microsoft.Commerce.Payments.PimsModel.V4;

    public class SearchTransaction
    {
        public string PaymentInstrumentAccountId { get; set; }

        public string Email { get; set; }

        public string Puid { get; set; }

        public IEnumerable<PaymentInstrument> PaymentInstruments { get; set; }
    }
}