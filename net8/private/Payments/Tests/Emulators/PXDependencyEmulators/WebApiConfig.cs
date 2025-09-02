using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Test.Common;

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators;

/// <summary>
/// ASP.NET Core configuration for the dependency emulators.
/// </summary>
public static class WebApiConfig
{
    /// <summary>
    /// Registers services and the <see cref="TestScenarioManager"/> instances used by the emulators.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    public static void Register(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers()
            .AddNewtonsoftJson(); 

        var env = builder.Environment;

        var managers = new Dictionary<string, TestScenarioManager>
        {
            [Constants.TestScenarioManagers.PartnerSettings] = new TestScenarioManager(Path.Combine(env.ContentRootPath, "TestScenarios", "PartnerSettings"), Constants.DefaultTestScenarios.PartnerSettingsEmulator),
            [Constants.TestScenarioManagers.Account] = new TestScenarioManager(Path.Combine(env.ContentRootPath, "TestScenarios", "Account"), Constants.DefaultTestScenarios.AccountEmulator),
            [Constants.TestScenarioManagers.PIMS] = new TestScenarioManager(Path.Combine(env.ContentRootPath, "TestScenarios", "PIMS"), string.Empty),
            [Constants.TestScenarioManagers.MSRewards] = new TestScenarioManager(Path.Combine(env.ContentRootPath, "TestScenarios", "MSRewards"), Constants.DefaultTestScenarios.MSRewardsEmulator),
            [Constants.TestScenarioManagers.Catalog] = new TestScenarioManager(Path.Combine(env.ContentRootPath, "TestScenarios", "Catalog"), Constants.DefaultTestScenarios.CatalogEmulator),
            [Constants.TestScenarioManagers.IssuerService] = new TestScenarioManager(Path.Combine(env.ContentRootPath, "TestScenarios", "IssuerService"), Constants.DefaultTestScenarios.IssuerServiceEmulator),
            [Constants.TestScenarioManagers.ChallengeManagement] = new TestScenarioManager(Path.Combine(env.ContentRootPath, "TestScenarios", "ChallengeManagement"), Constants.DefaultTestScenarios.ChallengeManagementServiceEmulator),
            [Constants.TestScenarioManagers.PaymentThirdParty] = new TestScenarioManager(Path.Combine(env.ContentRootPath, "TestScenarios", "PaymentThirdParty"), string.Empty),
            [Constants.TestScenarioManagers.Purchase] = new TestScenarioManager(Path.Combine(env.ContentRootPath, "TestScenarios", "Purchase"), string.Empty),
            [Constants.TestScenarioManagers.Risk] = new TestScenarioManager(Path.Combine(env.ContentRootPath, "TestScenarios", "Risk"), Constants.DefaultTestScenarios.RiskEmulator),
            [Constants.TestScenarioManagers.SellerMarketPlace] = new TestScenarioManager(Path.Combine(env.ContentRootPath, "TestScenarios", "SellerMarketPlace"), Constants.DefaultTestScenarios.SellerMarketEmulator),
            [Constants.TestScenarioManagers.TokenPolicy] = new TestScenarioManager(Path.Combine(env.ContentRootPath, "TestScenarios", "TokenPolicy"), Constants.DefaultTestScenarios.TokenPolicyEmulator),
            [Constants.TestScenarioManagers.TransactionService] = new TestScenarioManager(Path.Combine(env.ContentRootPath, "TestScenarios", "TransactionService"), string.Empty),
            [Constants.TestScenarioManagers.PaymentOchestrator] = new TestScenarioManager(Path.Combine(env.ContentRootPath, "TestScenarios", "PaymentOchestrator"), Constants.DefaultTestScenarios.POEmulator),
            [Constants.TestScenarioManagers.FraudDetection] = new TestScenarioManager(Path.Combine(env.ContentRootPath, "TestScenarios", "FraudDetection"), Constants.DefaultTestScenarios.FraudDetectionEmulator),
        };

        // Add Swagger services
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "PXDependencyEmulators",
                Version = "v1",
                Description = "Payment X Dependency Emulators (.NET 8.0)"
            });
        });

        builder.Services.AddSingleton(managers);
    }

    /// <summary>
    /// Configures the conventional routes for the emulator controllers.
    /// </summary>
    /// <param name="routes">The application's route builder.</param>
    public static void ConfigureRoutes(IEndpointRouteBuilder routes)
    {
        routes.MapControllers();

        routes.MapAccountRoutes();
        routes.MapPartnerSettingsRoutes();
        routes.MapPIMSRoutes();
        routes.MapMSRewardsRoutes();
        routes.MapCatalogRoutes();
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
