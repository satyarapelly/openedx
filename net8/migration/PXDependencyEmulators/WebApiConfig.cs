// Copyright (c) Microsoft. All rights reserved.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Web.Http;
using Microsoft.Commerce.Payments.PXCommon;
using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;
using Test.Common;

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators;

/// <summary>
/// ASP.NET Core Web API configuration for the dependency emulators.
/// </summary>
public static class WebApiConfig
{
    /// <summary>
    /// Register services required for the dependency emulators.
    /// </summary>
    /// <param name="builder">Application builder.</param>
    /// <param name="useArrangedResponses">Flag to enable arranged responses.</param>
    public static void Register(WebApplicationBuilder builder, bool useArrangedResponses = false)
    {
        var config = new HttpConfiguration();

        config.MessageHandlers.Add(new MockServiceDelegatingHandlerV2(new PartnerSettingsServiceMockResponseProvider(), useArrangedResponses));
        config.MessageHandlers.Add(new MockServiceDelegatingHandlerV2(new AccountServiceMockResponseProvider(), useArrangedResponses));
        config.MessageHandlers.Add(new MockServiceDelegatingHandlerV2(new PimsMockResponseProviderAugmented(), useArrangedResponses));
        config.MessageHandlers.Add(new MockServiceDelegatingHandlerV2(new MSRewardsServiceMockResponseProvider(), useArrangedResponses));
        config.MessageHandlers.Add(new MockServiceDelegatingHandlerV2(new CatalogServiceMockResponseProvider(), useArrangedResponses));
        config.MessageHandlers.Add(new MockServiceDelegatingHandlerV2(new IssuerServiceMockResponseProvider(), useArrangedResponses));
        config.MessageHandlers.Add(new MockServiceDelegatingHandlerV2(new ChallengeManagementServiceMockResponseProvider(), useArrangedResponses));
        config.MessageHandlers.Add(new MockServiceDelegatingHandlerV2(new PurchaseServiceMockResponseProvider(), useArrangedResponses));
        config.MessageHandlers.Add(new MockServiceDelegatingHandlerV2(new RiskServiceMockResponseProvider(), useArrangedResponses));
        config.MessageHandlers.Add(new MockServiceDelegatingHandlerV2(new TokenPolicyServiceMockResponseProvider(), useArrangedResponses));
        config.MessageHandlers.Add(new MockServiceDelegatingHandlerV2(new StoredValueServiceMockResponseProvider(), useArrangedResponses));
        config.MessageHandlers.Add(new MockServiceDelegatingHandlerV2(new WalletServiceMockResponseProvider(), useArrangedResponses));
        config.MessageHandlers.Add(new MockServiceDelegatingHandlerV2(new TransactionDataServiceMockResponseProvider(), useArrangedResponses));
        config.MessageHandlers.Add(new MockServiceDelegatingHandlerV2(new TransactionServiceMockResponseProvider(), useArrangedResponses));
        config.MessageHandlers.Add(new MockServiceDelegatingHandlerV2(new PaymentOrchestratorServiceMockResponseProvider(), useArrangedResponses));
        config.MessageHandlers.Add(new MockServiceDelegatingHandlerV2(new PayerAuthServiceMockResponseProvider(), useArrangedResponses));
        config.MessageHandlers.Add(new MockServiceDelegatingHandlerV2(new FraudDetectionMockResponseProvider(), useArrangedResponses));

        // In Selfhost env, httpContext is not available due to which HttpContext.Current?.Server?.MapPath("~/TestScenarios")
        // will return null. In such cases, we need to navigate to the TestScenarios folder from the current assembly path.
        if (WebHostingUtility.IsApplicationSelfHosted())
        {
            config.RegisterScenarioManager(Constants.TestScenarioManagers.PartnerSettings, "PXDependencyEmulators\\TestScenarios\\PartnerSettings", string.Empty);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.Account, "PXDependencyEmulators\\TestScenarios\\Account", Constants.DefaultTestScenarios.AccountEmulator);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.PIMS, "PXDependencyEmulators\\TestScenarios\\PIMS", string.Empty);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.MSRewards, "PXDependencyEmulators\\TestScenarios\\MSRewards", string.Empty);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.Catalog, "PXDependencyEmulators\\TestScenarios\\Catalog", string.Empty);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.IssuerService, "PXDependencyEmulators\\TestScenarios\\IssuerService", Constants.DefaultTestScenarios.IssuerServiceEmulator);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.ChallengeManagement, "PXDependencyEmulators\\TestScenarios\\ChallengeManagement", Constants.DefaultTestScenarios.ChallengeManagementServiceEmulator);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.PaymentThirdParty, "PXDependencyEmulators\\TestScenarios\\PaymentThirdParty", string.Empty);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.Purchase, "PXDependencyEmulators\\TestScenarios\\Purchase", string.Empty);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.Risk, "PXDependencyEmulators\\TestScenarios\\Risk", string.Empty);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.SellerMarketPlace, "PXDependencyEmulators\\TestScenarios\\SellerMarketPlace", string.Empty);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.TokenPolicy, "PXDependencyEmulators\\TestScenarios\\TokenPolicy", string.Empty);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.TransactionService, "PXDependencyEmulators\\TestScenarios\\TransactionService", string.Empty);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.PaymentOchestrator, "PXDependencyEmulators\\TestScenarios\\PaymentOchestrator", string.Empty);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.FraudDetection, "PXDependencyEmulators\\TestScenarios\\FraudDetection", Constants.DefaultTestScenarios.FraudDetectionEmulator);
        }
        else
        {
            config.RegisterScenarioManager(Constants.TestScenarioManagers.Account, "~/TestScenarios/Account", Constants.DefaultTestScenarios.AccountEmulator);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.PartnerSettings, "~/TestScenarios/PartnerSettings", string.Empty);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.PIMS, "~/TestScenarios/PIMS", string.Empty);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.MSRewards, "~/TestScenarios/MSRewards", string.Empty);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.Catalog, "~/TestScenarios/Catalog", string.Empty);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.IssuerService, "~/TestScenarios/IssuerService", Constants.DefaultTestScenarios.IssuerServiceEmulator);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.ChallengeManagement, "~/TestScenarios/ChallengeManagement", Constants.DefaultTestScenarios.ChallengeManagementServiceEmulator);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.PaymentThirdParty, "~/TestScenarios/PaymentThirdParty", string.Empty);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.Purchase, "~/TestScenarios/Purchase", string.Empty);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.Risk, "~/TestScenarios/Risk", string.Empty);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.SellerMarketPlace, "~/TestScenarios/SellerMarketPlace", string.Empty);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.TokenPolicy, "~/TestScenarios/TokenPolicy", string.Empty);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.TransactionService, "~/TestScenarios/TransactionService", string.Empty);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.PaymentOchestrator, "~/TestScenarios/PaymentOchestrator", string.Empty);
            config.RegisterScenarioManager(Constants.TestScenarioManagers.FraudDetection, "~/TestScenarios/FraudDetection", Constants.DefaultTestScenarios.FraudDetectionEmulator);
        }

        builder.Services.AddSingleton(config);

        // Controllers from the legacy WebApi project are brought in using the
        // compatibility shim so that existing ApiController types continue to work.
        builder.Services.AddControllers()
            .AddNewtonsoftJson()
            .AddWebApiConventions();
    }

    /// <summary>
    /// Configure HTTP routes for the emulators.
    /// </summary>
    /// <param name="routes">Route builder.</param>
    public static void ConfigureRoutes(IEndpointRouteBuilder routes)
    {
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

