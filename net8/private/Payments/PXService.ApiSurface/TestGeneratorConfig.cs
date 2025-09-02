// <copyright file="TestGeneratorConfig.cs" company="Microsoft">Copyright (c) Microsoft 2019. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXService.ApiSurface.Diff
{
    using System.Diagnostics.CodeAnalysis;

    public class TestGeneratorConfig
    {
        public bool AddressDescription { get; set; }

        public bool BillingGroupDescription { get; set; }

        public bool ProfileDescriptionWithEmulator { get; set; }

        public bool ProfileDescriptionWithoutEmulator { get; set; }

        public bool ChallengeDescription { get; set; }

        public bool RewardsDescriptions { get; set; }

        public bool CheckoutDescriptions { get; set; }

        public bool TaxIdDescription { get; set; }

        public bool PaymentMethodDescription { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1711: Identifiers should not have incorrect suffix", Justification = "This resource has a suffix Ex")]
        public bool PaymentInstrumentEx { get; set; }

        public bool RunDiffTestsForPSSFeatures { get; set; }
    }
}
