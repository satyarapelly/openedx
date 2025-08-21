// <copyright file="VersionCatalog.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

// VersionCatalog.cs
using System;
using System.Collections.Generic;
using Microsoft.Commerce.Payments.PXService;
using Microsoft.Extensions.Logging;
using V7 = Microsoft.Commerce.Payments.PXService.V7;

namespace Microsoft.Commerce.Payments.Common.Web
{
    public static class VersionCatalog
    {
        public const string DefaultVersion = "v7.0";

        public static IReadOnlyDictionary<string, string> Supported => supported;
        private static readonly Dictionary<string, string> supported =
            new(StringComparer.OrdinalIgnoreCase) { { "v7.0", "v7.0" } };

        public static void Register(VersionedControllerSelector resolver, ILogger logger)
        {
            static string Key(string name) =>
                name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)
                    ? name[..^"Controller".Length]
                    : name;

            // Versionless (probe)
            resolver.AddVersionless(
                Key(GlobalConstants.ControllerNames.ProbeController),
                typeof(ProbeController));

            // --- V7 controllers (ported 1:1 from your WebApiConfig.AddV7Controllers) ---
            var v7 = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                { Key(GlobalConstants.ControllerNames.PaymentInstrumentsExController),   typeof(V7.PaymentInstrumentsExController) },
                { Key(GlobalConstants.ControllerNames.SettingsController),                typeof(V7.SettingsController) },
                { Key(GlobalConstants.ControllerNames.PaymentMethodDescriptionsController), typeof(V7.PaymentMethodDescriptionsController) },
                { Key(GlobalConstants.ControllerNames.AddressDescriptionsController),     typeof(V7.AddressDescriptionsController) },
                { Key(GlobalConstants.ControllerNames.ChallengeDescriptionsController),   typeof(V7.ChallengeDescriptionsController) },
                { Key(GlobalConstants.ControllerNames.ProfileDescriptionsController),     typeof(V7.ProfileDescriptionsController) },
                { Key(GlobalConstants.ControllerNames.BillingGroupDescriptionsController),typeof(V7.BillingGroupDescriptionsController) },
                { Key(GlobalConstants.ControllerNames.TenantDescriptionsController),      typeof(V7.TenantDescriptionsController) },
                { Key(GlobalConstants.ControllerNames.TaxIdDescriptionsController),       typeof(V7.TaxIdDescriptionsController) },
                { Key(GlobalConstants.ControllerNames.PidlTransformationController),      typeof(V7.PidlTransformationController) },
                { Key(GlobalConstants.ControllerNames.PidlValidationController),          typeof(V7.PidlValidationController) },
                { Key(GlobalConstants.ControllerNames.SessionsController),                typeof(V7.SessionsController) },
                { Key(GlobalConstants.ControllerNames.AddressesController),               typeof(V7.AddressesController) },
                { Key(GlobalConstants.ControllerNames.AddressesExController),             typeof(V7.AddressesExController) },
                { Key(GlobalConstants.ControllerNames.PaymentSessionDescriptionsController), typeof(V7.PaymentSessionDescriptionsController) },
                { Key(GlobalConstants.ControllerNames.PaymentSessionsController),         typeof(V7.PaymentChallenge.PaymentSessionsController) },
                { Key(GlobalConstants.ControllerNames.PaymentTransactionsController),     typeof(V7.PaymentTransaction.PaymentTransactionsController) },
                { Key(GlobalConstants.ControllerNames.RDSSessionController),              typeof(V7.RDSSessionController) },
                { Key(GlobalConstants.ControllerNames.CheckoutDescriptionsController),    typeof(V7.CheckoutDescriptionsController) },
                { Key(GlobalConstants.ControllerNames.CheckoutsExController),             typeof(V7.Checkouts.CheckoutsExController) },
                { Key(GlobalConstants.ControllerNames.WalletsController),                 typeof(V7.WalletsController) },
                { Key(GlobalConstants.ControllerNames.RewardsDescriptionsController),     typeof(V7.RewardsDescriptionsController) },
                { Key(GlobalConstants.ControllerNames.MSRewardsController),               typeof(V7.MSRewardsController) },
                { Key(GlobalConstants.ControllerNames.InitializationController),          typeof(V7.InitializationController) },
                { Key(GlobalConstants.ControllerNames.DescriptionsController),            typeof(V7.DescriptionsController) },
                { Key(GlobalConstants.ControllerNames.CheckoutRequestsExController),      typeof(V7.PaymentClient.CheckoutRequestsExController) },
                { Key(GlobalConstants.ControllerNames.ExpressCheckoutController),         typeof(V7.ExpressCheckoutController) },
                { Key(GlobalConstants.ControllerNames.PaymentRequestsExController),       typeof(V7.PaymentClient.PaymentRequestsExController) },
                { Key(GlobalConstants.ControllerNames.AgenticTokenDescriptionsController),typeof(V7.AgenticTokenDesctipionsController) },
                { Key(GlobalConstants.ControllerNames.TokensExController),                typeof(V7.TokensExController) },
            };

            resolver.AddVersion("v7.0", v7);

            logger.LogInformation("Registered {Count} v7 controllers and probe.", v7.Count);
        }
    }
}
