// <copyright file="DisplayDetails.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using System.Diagnostics.CodeAnalysis;

    public class DisplayDetails
    {
        public string Name { get; set; }

        public string Logo { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is model coming from Payment Orchestration Service.")]
        public Logo[] Logos { get; set; }

        public string TermsAndConditions { get; set; }
    }
}