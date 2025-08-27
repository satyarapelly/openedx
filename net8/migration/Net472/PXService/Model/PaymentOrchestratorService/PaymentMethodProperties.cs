// <copyright file="PaymentMethodProperties.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService
{
    using System.Diagnostics.CodeAnalysis;

    public class PaymentMethodProperties
    {
        public bool OfflineRecurring { get; set; }

        public bool UserManaged { get; set; }

        public bool SoldToAddressRequired { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Matching with PO service model")]
        public PayinCap[] ChargeThresholds { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Matching with PO service model")]
        public string[] SupportedOperations { get; set; }

        public bool Taxable { get; set; }

        public bool ProviderRemittable { get; set; }

        public string MoBillingIdentityUrl { get; set; }

        public bool RiskyPaymentMethod { get; set; }

        public int AuthWindow { get; set; }

        public int FundsAvailabilityWindow { get; set; }

        public bool MultipleLineItemsSupported { get; set; }

        public bool SplitPaymentSupported { get; set; }

        public int PurchaseWaitTime { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Matching with PO service model")]
        public string[] RedirectRequired { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Matching with PO service model")]
        public int[] ReAuthWindows { get; set; }
    }
}