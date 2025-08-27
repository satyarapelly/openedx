// <copyright file="PaymentResource.cs" company="Microsoft">Copyright (c) Microsoft  All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.TransactionService
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Commerce.Payments.Common.Web;

    public class PaymentResource
    {
        public string Id { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1811", Justification = "This is a field used by JSON to generate HAL response")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<HalResource> Transactions { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1811", Justification = "This is a field used by JSON to generate HAL response")]
        [SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public Dictionary<string, RestLink> Links { get; set; }
    }
}
