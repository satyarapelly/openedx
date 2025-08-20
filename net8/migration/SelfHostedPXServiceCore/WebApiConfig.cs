// <copyright file="WebApiConfig.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators
{
    using System.Web.Http;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;
    using Test.Common;

    public class WebApiConfig
    {
        public static void Register(HttpConfiguration config, bool useArrangedResponses = false)
        {
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

            // Map routes for different emulators
            config.Routes.MapAccountRoutes();
            config.Routes.MapPartnerSettingsRoutes();
            config.Routes.MapPIMSRoutes();
            config.Routes.MapMSRewardsRoutes();
            config.Routes.MapCatalogRoutes();

            config.Routes.MapIssuerServiceRoutes();

            config.Routes.MapChallengeManagementRoutes();
            config.Routes.MapPaymentThirdPartyRoutes();
            config.Routes.MapPurchaseRoutes();
            config.Routes.MapRiskRoutes();
            config.Routes.MapSellerMarketPlaceRoutes();
            config.Routes.MapTokenPolicyRoutes();
            config.Routes.MapStoredValueRoutes();
            config.Routes.MapTransactionServiceRoutes();
            config.Routes.MapPaymentOchestratorRoutes();
            config.Routes.MapPayerAuthRoutes();
            config.Routes.MapFraudDetectionRoutes();
        }
    }
}
