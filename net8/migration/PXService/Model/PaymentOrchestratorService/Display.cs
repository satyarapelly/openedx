// <copyright file="Display.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using System.Diagnostics.CodeAnalysis;

    public class Display
    {
        public string Name { get; set; }

        public string Logo { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Matching with PO service model")]
        public Logo[] Logos { get; set; } = new Logo[0];
    }
}