// <copyright file="PaymentMethodResource.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using System.Diagnostics.CodeAnalysis;

    public class PaymentMethodResource
    {
        public string PaymentMethodType { get; set; }

        public string PaymentMethodFamily { get; set; }

        public Display Display { get; set; }

        public PaymentMethodProperties Properties { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Matching with PO service model")]
        public string[] ExclusionTags { get; set; }

        public string PaymentMethodGroup { get; set; }

        public string GroupDisplayName { get; set; }
    }
}