namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Commerce.Payments.PXCommon;
    using Test.Common;

    /// <summary>
    /// Provides configuration helpers for the dependency emulator service.
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// Registers services used by the dependency emulators.
        /// </summary>
        /// <param name="builder">The web application builder.</param>
        /// <param name="useArrangedResponses">Indicates if arranged responses should be used.</param>
        public static void Register(WebApplicationBuilder builder, bool useArrangedResponses = false)
        {
            builder.Services.AddControllers()
                .AddNewtonsoftJson();

            // Register scenario managers for the emulator responses.
            if (WebHostingUtility.IsApplicationSelfHosted())
            {
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.PartnerSettings,
                    new TestScenarioManager("PXDependencyEmulators\\TestScenarios\\PartnerSettings", string.Empty));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.Account,
                    new TestScenarioManager("PXDependencyEmulators\\TestScenarios\\Account", Constants.DefaultTestScenarios.AccountEmulator));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.PIMS,
                    new TestScenarioManager("PXDependencyEmulators\\TestScenarios\\PIMS", string.Empty));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.MSRewards,
                    new TestScenarioManager("PXDependencyEmulators\\TestScenarios\\MSRewards", string.Empty));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.Catalog,
                    new TestScenarioManager("PXDependencyEmulators\\TestScenarios\\Catalog", string.Empty));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.IssuerService,
                    new TestScenarioManager("PXDependencyEmulators\\TestScenarios\\IssuerService", Constants.DefaultTestScenarios.IssuerServiceEmulator));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.ChallengeManagement,
                    new TestScenarioManager("PXDependencyEmulators\\TestScenarios\\ChallengeManagement", Constants.DefaultTestScenarios.ChallengeManagementServiceEmulator));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.PaymentThirdParty,
                    new TestScenarioManager("PXDependencyEmulators\\TestScenarios\\PaymentThirdParty", string.Empty));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.Purchase,
                    new TestScenarioManager("PXDependencyEmulators\\TestScenarios\\Purchase", string.Empty));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.Risk,
                    new TestScenarioManager("PXDependencyEmulators\\TestScenarios\\Risk", string.Empty));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.SellerMarketPlace,
                    new TestScenarioManager("PXDependencyEmulators\\TestScenarios\\SellerMarketPlace", string.Empty));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.TokenPolicy,
                    new TestScenarioManager("PXDependencyEmulators\\TestScenarios\\TokenPolicy", string.Empty));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.TransactionService,
                    new TestScenarioManager("PXDependencyEmulators\\TestScenarios\\TransactionService", string.Empty));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.PaymentOchestrator,
                    new TestScenarioManager("PXDependencyEmulators\\TestScenarios\\PaymentOchestrator", string.Empty));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.FraudDetection,
                    new TestScenarioManager("PXDependencyEmulators\\TestScenarios\\FraudDetection", Constants.DefaultTestScenarios.FraudDetectionEmulator));
            }
            else
            {
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.Account,
                    new TestScenarioManager("~/TestScenarios/Account", Constants.DefaultTestScenarios.AccountEmulator));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.PartnerSettings,
                    new TestScenarioManager("~/TestScenarios/PartnerSettings", string.Empty));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.PIMS,
                    new TestScenarioManager("~/TestScenarios/PIMS", string.Empty));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.MSRewards,
                    new TestScenarioManager("~/TestScenarios/MSRewards", string.Empty));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.Catalog,
                    new TestScenarioManager("~/TestScenarios/Catalog", string.Empty));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.IssuerService,
                    new TestScenarioManager("~/TestScenarios/IssuerService", Constants.DefaultTestScenarios.IssuerServiceEmulator));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.ChallengeManagement,
                    new TestScenarioManager("~/TestScenarios/ChallengeManagement", Constants.DefaultTestScenarios.ChallengeManagementServiceEmulator));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.PaymentThirdParty,
                    new TestScenarioManager("~/TestScenarios/PaymentThirdParty", string.Empty));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.Purchase,
                    new TestScenarioManager("~/TestScenarios/Purchase", string.Empty));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.Risk,
                    new TestScenarioManager("~/TestScenarios/Risk", string.Empty));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.SellerMarketPlace,
                    new TestScenarioManager("~/TestScenarios/SellerMarketPlace", string.Empty));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.TokenPolicy,
                    new TestScenarioManager("~/TestScenarios/TokenPolicy", string.Empty));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.TransactionService,
                    new TestScenarioManager("~/TestScenarios/TransactionService", string.Empty));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.PaymentOchestrator,
                    new TestScenarioManager("~/TestScenarios/PaymentOchestrator", string.Empty));
                builder.Services.RegisterScenarioManager(
                    Constants.TestScenarioManagers.FraudDetection,
                    new TestScenarioManager("~/TestScenarios/FraudDetection", Constants.DefaultTestScenarios.FraudDetectionEmulator));
            }
        }

        /// <summary>
        /// Maps emulator routes on the specified route builder.
        /// </summary>
        /// <param name="routes">The endpoint route builder.</param>
        public static void MapRoutes(IEndpointRouteBuilder routes)
        {
            routes.MapControllers();
            routes.MapPartnerSettingsRoutes();
            routes.MapPIMSRoutes();
            routes.MapMSRewardsRoutes();
            routes.MapCatalogRoutes();
            routes.MapAccountRoutes();
            routes.MapIssuerServiceRoutes();
            routes.MapChallengeManagementRoutes();
            routes.MapPaymentThirdPartyRoutes();
            routes.MapPurchaseRoutes();
            routes.MapRiskRoutes();
            routes.MapSellerMarketPlaceRoutes();
            routes.MapTokenPolicyRoutes();
            routes.MapStoredValueRoutes();
            routes.MapTransactionServiceRoutes();
            routes.MapPaymentOchestratorRoutes();
            routes.MapPayerAuthRoutes();
            routes.MapFraudDetectionRoutes();
        }
    }
}

