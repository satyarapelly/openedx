// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using Microsoft.Commerce.Payments.PXService;
    using Microsoft.Commerce.Payments.PXService.Accessors.ChallengeManagementService;
    using Microsoft.Commerce.Payments.PXService.Accessors.D365Service;
    using Microsoft.Commerce.Payments.PXService.Accessors.IssuerService;
    using Microsoft.Commerce.Payments.PXService.Accessors.MSRewardsService;
    using Microsoft.Commerce.Payments.PXService.Accessors.NetworkTokenizationService;
    using Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService;
    using Microsoft.Commerce.Payments.PXService.Accessors.OrchestrationService;
    using Microsoft.Commerce.Payments.PXService.Accessors.PartnerSettingsService;
    using Microsoft.Commerce.Payments.PXService.Accessors.SellerMarketPlaceService;
    using Microsoft.Commerce.Payments.PXService.Accessors.ShortURLService;
    using Microsoft.Commerce.Payments.PXService.Accessors.TransactionDataService;
    using Microsoft.Commerce.Payments.PXService.AddressEnrichmentService.V7;
    using Microsoft.Commerce.Payments.PXService.RiskService.V7;
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks;
    using Microsoft.Commerce.Payments.PXService.Accessors.TokenizationService;
    using Microsoft.Commerce.Payments.PXService.Accessors.PaymentOrchestratorService;
    using Microsoft.Commerce.Payments.PXService.Accessors.FraudDetectionService;

    public class PXServiceSettings : Microsoft.Commerce.Payments.PXService.Settings.PXServiceSettings
    {
        public Mocks.AccountService AccountsService { get; private set; }

        public Mocks.PimsService PimsService { get; private set; }

        public Mocks.OrchestrationService OrchestrationService { get; private set; }

        public Mocks.PayerAuthService PayerAuthService { get; private set; }

        public Mocks.PurchaseService PurchaseService { get; private set; }

        public Mocks.D365Service D365Service { get; private set; }

        public Mocks.CatalogService CatalogService { get; private set; }

        public Mocks.SessionService SessionService { get; private set; }

        public Mocks.StoredValueService StoredValueService { get; private set; }

        public Mocks.RiskService RiskService { get; private set; }

        public Mocks.TaxIdService TaxIdService { get; private set; }

        public Mocks.AddressEnrichmentService AddressEnrichmentService { get; private set; }

        public Mocks.TransactionService TransactionService { get; private set; }

        public Mocks.SellerMarketPlaceService SellerMarketPlaceService { get; private set; }

        public Mocks.PaymentThirdPartyService PaymentThirdPartyService { get; private set; }

        public Mocks.CommerceAccountDataAccessor CommerceAccountDataService { get; private set; }

        public Mocks.CTPCommerceDataAccessor CtpCommerceDataService { get; private set; }

        public Mocks.ShortURLService ShortURLService { get; private set; }

        public Mocks.AzureExPService AzureExPService { get; private set; }

        public Mocks.HIPService HIPService { get; private set; }

        public Mocks.PartnerSettingsService PartnerSettingsService { get; private set; }

        public Mocks.WalletService WalletService { get; private set; }

        public Mocks.TransactionDataService TransactionDataService { get; private set; }

        public Mocks.IssuerService IssuerService { get; private set; }

        public Mocks.ChallengeManagementService ChallengeManagementService { get; private set; }

        public Mocks.NetworkTokenizationService NetworkTokenizationService { get; private set; }

        public Mocks.MSRewardsService MSRewardsService { get; private set; }

        public Mocks.TokenizationService TokenizationService { get; private set; }

        public Mocks.TokenPolicyService TokenPolicyService { get; private set; }

        public Mocks.PaymentOrchestratorService PaymentOrchestratorService { get; private set; }

        public Mocks.FraudDetectionService FraudDetectionService { get; private set; }

        private readonly Dictionary<Type, HostableService> selfHostedDependencies;

        public PXServiceSettings(Dictionary<Type, HostableService>? selfHostedDependencies = null, bool useArrangedResponses = true)
        {
            this.selfHostedDependencies = selfHostedDependencies ?? new Dictionary<Type, HostableService>();
            AccountsService = new Mocks.AccountService(new AccountServiceMockResponseProvider(), useArrangedResponses);
            PimsService = new Mocks.PimsService(new PimsMockResponseProvider(), useArrangedResponses);
            OrchestrationService = new Mocks.OrchestrationService(new OrchestrationServiceMockResponseProvider(), useArrangedResponses);
            PayerAuthService = new Mocks.PayerAuthService(new PayerAuthServiceMockResponseProvider(), useArrangedResponses);
            SessionService = new Mocks.SessionService(new SessionServiceMockResponseProvider(), useArrangedResponses);
            StoredValueService = new Mocks.StoredValueService(new StoredValueServiceMockResponseProvider(), useArrangedResponses);
            RiskService = new Mocks.RiskService(new RiskServiceMockResponseProvider(), useArrangedResponses);
            TaxIdService = new Mocks.TaxIdService(new TaxIdServiceMockResponseProvider(), useArrangedResponses);
            PurchaseService = new Mocks.PurchaseService(new PurchaseServiceMockResponseProvider(), useArrangedResponses);
            D365Service = new Mocks.D365Service(new D365ServiceMockResponseProvider(), useArrangedResponses);
            CatalogService = new Mocks.CatalogService(new CatalogServiceMockResponseProvider(), useArrangedResponses);
            AddressEnrichmentService = new Mocks.AddressEnrichmentService(new AddressEnrichmentServiceMockResponseProvider(), useArrangedResponses);
            TransactionService = new Mocks.TransactionService(new TransactionServiceMockResponseProvider(), useArrangedResponses);
            SellerMarketPlaceService = new Mocks.SellerMarketPlaceService(new SellerMarketPlaceServiceMockResponseProvider(), useArrangedResponses);
            PaymentThirdPartyService = new Mocks.PaymentThirdPartyService(new PaymentThirdPartyServiceMockResponseProvider(), useArrangedResponses);
            ShortURLService = new Mocks.ShortURLService(new ShortUrlServiceMockResponseProvider(), useArrangedResponses);
            AzureExPService = new Mocks.AzureExPService(new AzureExPServiceMockResponseProvider(), useArrangedResponses);
            PartnerSettingsService = new Mocks.PartnerSettingsService(new PartnerSettingsServiceMockResponseProvider(), useArrangedResponses);
            HIPService = new Mocks.HIPService(new HIPServiceMockResponseProvider(), useArrangedResponses);
            CommerceAccountDataService = new Mocks.CommerceAccountDataAccessor();
            CtpCommerceDataService = new Mocks.CTPCommerceDataAccessor();
            IssuerService = new Mocks.IssuerService(new IssuerServiceMockResponseProvider(), useArrangedResponses);
            WalletService = new Mocks.WalletService(new WalletServiceMockResponseProvider(), useArrangedResponses);
            TransactionDataService = new Mocks.TransactionDataService(new TransactionDataServiceMockResponseProvider(), useArrangedResponses);
            ChallengeManagementService = new Mocks.ChallengeManagementService(new ChallengeManagementServiceMockResponseProvider(), useArrangedResponses);
            NetworkTokenizationService = new Mocks.NetworkTokenizationService(new NetworkTokenizationServiceMockResponseProvider(), useArrangedResponses);
            MSRewardsService = new Mocks.MSRewardsService(new MSRewardsServiceMockResponseProvider(), useArrangedResponses);
            TokenPolicyService = new Mocks.TokenPolicyService(new TokenPolicyServiceMockResponseProvider(), useArrangedResponses);
            TokenizationService = new Mocks.TokenizationService(new TokenizationServiceMockResponseProvider(), useArrangedResponses);
            PaymentOrchestratorService = new Mocks.PaymentOrchestratorService(new PaymentOrchestratorServiceMockResponseProvider(), useArrangedResponses);
            FraudDetectionService = new Mocks.FraudDetectionService(new FraudDetectionMockResponseProvider(), useArrangedResponses);

            this.ValidateCors = false;
            this.AddCorsAllowedOrigin("https://pidlsdktestportal.azurewebsites.net");
            this.AddCorsAllowedOrigin("http://localhost:3000");

            this.PIDLDocumentValidationEnabled = true;
            
            this.LocalFeatureConfigs = PXServiceSettings.FetchStaticFeatureConfigs(
                "Settings\\FeatureConfig\\featureconfigs.json",
                "Settings\\FeatureConfig\\testusergroups.json",
                "int_test_user_group");

            this.PifdBaseUrl = "https://pifd.cp.microsoft-int.com/V6.0";

            this.PayMicrosoftBaseUrl = "https://payint.microsoft.com";

            this.StaticResourceServiceBaseUrl = "https://staticresources.payments.microsoft-int.com/staticresourceservice";

            InitializeAccessors();

            this.CommerceAccountDataServiceAccessor = CommerceAccountDataService;

            this.CtpCommerceDataServiceAccessor = CtpCommerceDataService;

            DirectoryInfo directoryInfo = new DirectoryInfo(System.Environment.CurrentDirectory);
            this.SllEnvironmentSetting = new Microsoft.Commerce.Payments.Common.Tracing.SllEnvironmentSetting
            {
                SllLogPath = directoryInfo.Parent == null ? directoryInfo.FullName : directoryInfo.Parent.FullName,
                SllLogNamePrefix = "PXServiceSll",
                SllMaxFileSizeBytes = 52428800L,
                SllMaxFileCount = 64,
            };
        }

        private void InitializeAccessors()
        {
            if (this.selfHostedDependencies.ContainsKey(typeof(PIMSAccessor)))
            {
                var accessor = this.selfHostedDependencies[typeof(PIMSAccessor)];
                this.PIMSAccessor = new PIMSAccessor(
                    serviceBaseUrl: accessor.BaseUri.ToString(),
                    emulatorBaseUrl: accessor.BaseUri.ToString(),
                    servicePPEBaseUrl: accessor.BaseUri.ToString(),
                    apiVersion: "TestApiVersion",
                    messageHandler: new WebRequestHandler());
            }
            else
            {
                this.PIMSAccessor = new PIMSAccessor(
                    serviceBaseUrl: "https://mockPims",
                    emulatorBaseUrl: "https://testEmulatorBaseUrl",
                    servicePPEBaseUrl: null,
                    apiVersion: "TestApiVersion",
                    messageHandler: PimsService);
            }

            if (this.selfHostedDependencies.ContainsKey(typeof(OrchestrationServiceAccessor)))
            {
                var accessor = this.selfHostedDependencies[typeof(OrchestrationServiceAccessor)];
                this.OrchestrationServiceAccessor = new OrchestrationServiceAccessor(
                    serviceBaseUrl: accessor.BaseUri.ToString(),
                    emulatorBaseUrl: accessor.BaseUri.ToString(),
                    apiVersion: "TestApiVersion",
                    messageHandler: new WebRequestHandler());
            }
            else
            {
                this.OrchestrationServiceAccessor = new OrchestrationServiceAccessor(
                    serviceBaseUrl: "https://mockOrchestrationService",
                    emulatorBaseUrl: "https://testEmulatorBaseUrl",
                    apiVersion: "TestApiVersion",
                    messageHandler: OrchestrationService);
            }

            if (this.selfHostedDependencies.ContainsKey(typeof(AccountServiceAccessor)))
            {
                var accessor = this.selfHostedDependencies[typeof(AccountServiceAccessor)];
                this.AccountServiceAccessor = new AccountServiceAccessor(
                    serviceBaseUrl: accessor.BaseUri.ToString(),
                    emulatorBaseUrl: accessor.BaseUri.ToString(),
                    messageHandler: new WebRequestHandler());
            }
            else
            {
                this.AccountServiceAccessor = new AccountServiceAccessor(
                    serviceBaseUrl: "https://mockAccountService",
                    emulatorBaseUrl: "https://testEmulatorBaseUrl",
                    messageHandler: AccountsService);
            }

            if (this.selfHostedDependencies.ContainsKey(typeof(PayerAuthServiceAccessor)))
            {
                var accessor = this.selfHostedDependencies[typeof(PayerAuthServiceAccessor)];
                this.PayerAuthServiceAccessor = new PayerAuthServiceAccessor(
                    serviceBaseUrl: accessor.BaseUri.ToString(),
                    emulatorBaseUrl: accessor.BaseUri.ToString(),
                    apiVersion: "testApiVersion",
                    messageHandler: new WebRequestHandler());
            }
            else
            {
                this.PayerAuthServiceAccessor = new PayerAuthServiceAccessor(
                    serviceBaseUrl: "https://mockPayerAuthService",
                    emulatorBaseUrl: "https://testEmulatorBaseUrl",
                    apiVersion: "testApiVersion",
                    messageHandler: PayerAuthService);
            }

            if (this.selfHostedDependencies.ContainsKey(typeof(PurchaseServiceAccessor)))
            {
                var accessor = this.selfHostedDependencies[typeof(PurchaseServiceAccessor)];
                this.PurchaseServiceAccessor = new PurchaseServiceAccessor(
                    serviceBaseUrl: accessor.BaseUri.ToString(),
                    emulatorBaseUrl: null,
                    apiVersion: this.PurchaseServiceApiVersion,
                    messageHandler: new WebRequestHandler());
            }
            else
            {
                this.PurchaseServiceAccessor = new PurchaseServiceAccessor(
                    serviceBaseUrl: "https://mockPurchaseService",
                    emulatorBaseUrl: null,
                    apiVersion: this.PurchaseServiceApiVersion,
                    messageHandler: PurchaseService);
            }

            if (this.selfHostedDependencies.ContainsKey(typeof(CatalogServiceAccessor)))
            {
                var accessor = this.selfHostedDependencies[typeof(CatalogServiceAccessor)];
                this.CatalogServiceAccessor = new CatalogServiceAccessor(
                    serviceBaseUrl: accessor.BaseUri.ToString(),
                    emulatorBaseUrl: null,
                    apiVersion: this.CatalogServiceApiVersion,
                    messageHandler: new WebRequestHandler());
            }
            else
            {
                this.CatalogServiceAccessor = new CatalogServiceAccessor(
                    serviceBaseUrl: "https://mockPurchaseService",
                    emulatorBaseUrl: null,
                    apiVersion: this.CatalogServiceApiVersion,
                    messageHandler: CatalogService);
            }

            if (this.selfHostedDependencies.ContainsKey(typeof(SessionServiceAccessor)))
            {
                var accessor = this.selfHostedDependencies[typeof(SessionServiceAccessor)];
                this.SessionServiceAccessor = new SessionServiceAccessor(
                    baseUrl: accessor.BaseUri.ToString(),
                    apiVersion: "2015-09-23",
                    requestHandler: new WebRequestHandler());
            }
            else
            {
                this.SessionServiceAccessor = new SessionServiceAccessor(
                    baseUrl: "https://mockSessionService",
                    apiVersion: "2015-09-23",
                    requestHandler: SessionService);
            }

            if (this.selfHostedDependencies.ContainsKey(typeof(StoredValueAccessor)))
            {
                var accessor = this.selfHostedDependencies[typeof(StoredValueAccessor)];
                this.StoredValueServiceAccessor = new StoredValueAccessor(
                    apiVersion: "2014-10-10",
                    serviceBaseUrl: accessor.BaseUri.ToString(),
                    emulatorBaseUrl: accessor.BaseUri.ToString(),
                    messageHandler: new WebRequestHandler());
            }
            else
            {
                this.StoredValueServiceAccessor = new StoredValueAccessor(
                    apiVersion: "2014-10-10",
                    serviceBaseUrl: "http://localhost/StoredValueEmulator", // lgtm[cs/non-https-url] Suppressing Semmle warning // DevSkim: ignore DS137138 as this to access the locally hosted endpoint
                    emulatorBaseUrl: "http://localhost/StoredValueEmulator", // lgtm[cs/non-https-url] Suppressing Semmle warning // DevSkim: ignore DS137138 as this to access the locally hosted endpoint
                    messageHandler: StoredValueService);
            }

            if (this.selfHostedDependencies.ContainsKey(typeof(RiskServiceAccessor)))
            {
                var accessor = this.selfHostedDependencies[typeof(RiskServiceAccessor)];
                this.RiskServiceAccessor = new RiskServiceAccessor(
                    serviceBaseUrl: accessor.BaseUri.ToString(),
                    emulatorBaseUrl: accessor.BaseUri.ToString(),
                    apiVersion: "2015-09-23",
                    messageHandler: new WebRequestHandler());
            }
            else
            {
                this.RiskServiceAccessor = new RiskServiceAccessor(
                    serviceBaseUrl: "https://mockRiskService",
                    emulatorBaseUrl: "http://localhost/RiskEmulator",
                    apiVersion: "2015-09-23",
                    messageHandler: RiskService);
            }

            if (this.selfHostedDependencies.ContainsKey(typeof(TaxIdServiceAccessor)))
            {
                var accessor = this.selfHostedDependencies[typeof(TaxIdServiceAccessor)];
                this.TaxIdServiceAccessor = new TaxIdServiceAccessor(
                    serviceBaseUrl: accessor.BaseUri.ToString(),
                    messageHandler: new WebRequestHandler());
            }
            else
            {
                this.TaxIdServiceAccessor = new TaxIdServiceAccessor(
                    serviceBaseUrl: "https://mockTaxIdService",
                    messageHandler: TaxIdService);
            }

            if (this.selfHostedDependencies.ContainsKey(typeof(AddressEnrichmentServiceAccessor)))
            {
                var accessor = this.selfHostedDependencies[typeof(AddressEnrichmentServiceAccessor)];
                this.AddressEnrichmentServiceAccessor = new AddressEnrichmentServiceAccessor(
                    serviceBaseUrl: accessor.BaseUri.ToString(),
                    emulatorBaseUrl: string.Empty,
                    messageHandler: new WebRequestHandler());
            }
            else
            {
                this.AddressEnrichmentServiceAccessor = new AddressEnrichmentServiceAccessor(
                    serviceBaseUrl: "https://mockAddressEnrichmentService",
                    emulatorBaseUrl: string.Empty,
                    messageHandler: AddressEnrichmentService);
            }

            if (this.selfHostedDependencies.ContainsKey(typeof(TransactionServiceAccessor)))
            {
                var accessor = this.selfHostedDependencies[typeof(TransactionServiceAccessor)];
                this.TransactionServiceAccessor = new TransactionServiceAccessor(
                    serviceBaseUrl: accessor.BaseUri.ToString(),
                    emulatorBaseUrl: accessor.BaseUri.ToString(),
                    apiVersion: "2018-05-07",
                    messageHandler: new WebRequestHandler());
            }
            else
            {
                this.TransactionServiceAccessor = new TransactionServiceAccessor(
                    serviceBaseUrl: "https://mockTransactionService",
                    emulatorBaseUrl:
                    "http://localhost/TransactionServiceEmulator", // DevSkim: ignore DS137138 as this to access the locally hosted endpoint
                    apiVersion: "2018-05-07",
                    messageHandler: TransactionService);
            }

            if (this.selfHostedDependencies.ContainsKey(typeof(SellerMarketPlaceServiceAccessor)))
            {
                var accessor = this.selfHostedDependencies[typeof(SellerMarketPlaceServiceAccessor)];
                this.SellerMarketPlaceServiceAccessor = new SellerMarketPlaceServiceAccessor(
                    serviceBaseUrl: accessor.BaseUri.ToString(),
                    emulatorBaseUrl: accessor.BaseUri.ToString(),
                    messageHandler: new WebRequestHandler());
            }
            else
            {
                this.SellerMarketPlaceServiceAccessor = new SellerMarketPlaceServiceAccessor(
                    serviceBaseUrl: "https://mockPaymentThirdPartyServiceAccessor",
                    emulatorBaseUrl:
                    "http://localhost/SellerMarketPlaceEmulator", // DevSkim: ignore DS137138 as this to access the locally hosted endpoint
                    messageHandler: SellerMarketPlaceService);
            }

            if (this.selfHostedDependencies.ContainsKey(typeof(PaymentThirdPartyServiceAccessor)))
            {
                var accessor = this.selfHostedDependencies[typeof(PaymentThirdPartyServiceAccessor)];
                this.PaymentThirdPartyServiceAccessor = new PaymentThirdPartyServiceAccessor(
                    serviceBaseUrl: accessor.BaseUri.ToString(),
                    emulatorBaseUrl: accessor.BaseUri.ToString(),
                    apiVersion: "2022-02-09",
                    messageHandler: new WebRequestHandler());
            }
            else
            {
                this.PaymentThirdPartyServiceAccessor = new PaymentThirdPartyServiceAccessor(
                    serviceBaseUrl: "https://mockPaymentThirdPartyServiceAccessor",
                    emulatorBaseUrl:
                    "http://localhost/PaymentThirdPartyEmulator", // DevSkim: ignore DS137138 as this to access the locally hosted endpoint
                    apiVersion: "2022-02-09",
                    messageHandler: PaymentThirdPartyService);
            }

            if (this.selfHostedDependencies.ContainsKey(typeof(AzureExPAccessor)))
            {
                var accessor = this.selfHostedDependencies[typeof(AzureExPAccessor)];
                this.AzureExPAccessor = new AzureExPAccessor(
                    expBlobUrl: accessor.BaseUri.ToString(),
                    tokenLoader: new Mocks.AuthTokenGetter(),
                    messageHandler: new WebRequestHandler(),
                    enableTestHook: true);
            }
            else
            {
                this.AzureExPAccessor = new AzureExPAccessor(
                    expBlobUrl: "https://mockAzureExPService",
                    tokenLoader: new Mocks.AuthTokenGetter(),
                    messageHandler: AzureExPService,
                    enableTestHook: true);
            }

            if (this.selfHostedDependencies.ContainsKey(typeof(PartnerSettingsServiceAccessor)))
            {
                var accessor = this.selfHostedDependencies[typeof(PartnerSettingsServiceAccessor)];
                this.PartnerSettingsServiceAccessor = new PartnerSettingsServiceAccessor(
                    serviceBaseUrl: accessor.BaseUri.ToString(),
                    servicePPEBaseUrl: accessor.BaseUri.ToString(),
                    emulatorBaseUrl: accessor.BaseUri.ToString(),
                    messageHandler: new WebRequestHandler());
            }
            else
            {
                this.PartnerSettingsServiceAccessor = new PartnerSettingsServiceAccessor(
                    serviceBaseUrl: "https://mockPartnerSettingsService",
                    servicePPEBaseUrl: null,
                    emulatorBaseUrl:
                    "http://localhost/PartnerSettingsEmulator", // DevSkim: ignore DS137138 as this to access the locally hosted endpoint
                    messageHandler: PartnerSettingsService,
                    disablePSSCache: true);
            }

            if (this.selfHostedDependencies.ContainsKey(typeof(IssuerServiceAccessor)))
            {
                var accessor = this.selfHostedDependencies[typeof(IssuerServiceAccessor)];
                this.IssuerServiceAccessor = new IssuerServiceAccessor(
                    serviceBaseUrl: accessor.BaseUri.ToString(),
                    emulatorBaseUrl: accessor.BaseUri.ToString(),
                    apiVersion: this.IssuerServiceApiVersion,
                    messageHandler: new WebRequestHandler());
            }
            else
            {
                this.IssuerServiceAccessor = new IssuerServiceAccessor(
                    serviceBaseUrl: "https://mockIssuerService",
                    emulatorBaseUrl: "http://localhoust/IssuerServiceEmulator",
                    apiVersion: this.IssuerServiceApiVersion,
                    messageHandler: IssuerService);
            }

            if (this.selfHostedDependencies.ContainsKey(typeof(ChallengeManagementServiceAccessor)))
            {
                var accessor = this.selfHostedDependencies[typeof(ChallengeManagementServiceAccessor)];
                this.ChallengeManagementServiceAccessor = new ChallengeManagementServiceAccessor(
                    serviceBaseUrl: accessor.BaseUri.ToString(),
                    emulatorBaseUrl: accessor.BaseUri.ToString(),
                    messageHandler: new WebRequestHandler());
            }
            else
            {
                this.ChallengeManagementServiceAccessor = new ChallengeManagementServiceAccessor(
                    serviceBaseUrl: "https://mockChallengeManagementService",
                    emulatorBaseUrl: "http://localhost/ChallengeManagementServiceEmulator",
                    messageHandler: ChallengeManagementService);
            }

            if (this.selfHostedDependencies.ContainsKey(typeof(WalletServiceAccessor)))
            {
                var accessor = this.selfHostedDependencies[typeof(WalletServiceAccessor)];
                this.WalletServiceAccessor = new WalletServiceAccessor(
                    serviceBaseUrl: accessor.BaseUri.ToString(),
                    apiVersion: "2023-1-1",
                    messageHandler: new WebRequestHandler());
            }
            else
            {
                this.WalletServiceAccessor = new WalletServiceAccessor(
                    serviceBaseUrl: "https://mockWalletService",
                    apiVersion: "2023-1-1",
                    messageHandler: WalletService);
            }

            if (this.selfHostedDependencies.ContainsKey(typeof(TransactionDataServiceAccessor)))
            {
                var accessor = this.selfHostedDependencies[typeof(TransactionDataServiceAccessor)];
                this.TransactionDataServiceAccessor = new TransactionDataServiceAccessor(
                    serviceBaseUrl: accessor.BaseUri.ToString(),
                    apiVersion: "2023-1-1",
                    messageHandler: new WebRequestHandler());
            }
            else
            {
                this.TransactionDataServiceAccessor = new TransactionDataServiceAccessor(
                    serviceBaseUrl: "https://mockTransactionDataService",
                    apiVersion: "2023-1-1",
                    messageHandler: TransactionDataService);
            }

            if (this.selfHostedDependencies.ContainsKey(typeof(MSRewardsServiceAccessor)))
            {
                var accessor = this.selfHostedDependencies[typeof(MSRewardsServiceAccessor)];
                this.MSRewardsServiceAccessor = new MSRewardsServiceAccessor(
                    serviceBaseUrl: accessor.BaseUri.ToString(),
                    emulatorBaseUrl: accessor.BaseUri.ToString(),
                    messageHandler: new WebRequestHandler());
            }
            else
            {
                this.MSRewardsServiceAccessor = new MSRewardsServiceAccessor(
                    serviceBaseUrl: "https://mockMSRewardsService",
                    emulatorBaseUrl: "https://testEmulatorBaseUrl",
                    messageHandler: MSRewardsService);
            }

            if (this.selfHostedDependencies.ContainsKey(typeof(TokenPolicyServiceAccessor)))
            {
                var accessor = this.selfHostedDependencies[typeof(TokenPolicyServiceAccessor)];
                this.TokenPolicyServiceAccessor = new TokenPolicyServiceAccessor(
                    serviceBaseUrl: accessor.BaseUri.ToString(),
                    emulatorBaseUrl: accessor.BaseUri.ToString(),
                    messageHandler: new WebRequestHandler());
            }
            else
            {
                this.TokenPolicyServiceAccessor = new TokenPolicyServiceAccessor(
                    serviceBaseUrl: "https://mockTokenPolicyService",
                    emulatorBaseUrl: "https://testEmulatorBaseUrl",
                    messageHandler: TokenPolicyService);
            }

            if (this.selfHostedDependencies.ContainsKey(typeof(TokenizationServiceAccessor)))
            {
                var accessor = this.selfHostedDependencies[typeof(TokenizationServiceAccessor)];
                this.TokenizationServiceAccessor = new TokenizationServiceAccessor(
                    serviceBaseUrl: accessor.BaseUri.ToString(),
                    emulatorBaseUrl: accessor.BaseUri.ToString(),
                    tokenizationGetTokenURL: accessor.BaseUri.ToString(),
                    tokenizationGetTokenFromEncryptedValueURL: accessor.BaseUri.ToString(),    
                    messageHandler: new WebRequestHandler());
            }
            else
            {
                this.TokenizationServiceAccessor = new TokenizationServiceAccessor(
                    serviceBaseUrl: "https://mockTokenizationService",
                    emulatorBaseUrl: "https://testEmulatorBaseUrl",
                    tokenizationGetTokenURL: "https://mockTokenizationService/tokens",
                    tokenizationGetTokenFromEncryptedValueURL: "https://mockTokenizationService/tokens",
                    messageHandler: TokenizationService);
            }

            if (this.selfHostedDependencies.ContainsKey(typeof(PaymentOrchestratorServiceAccessor)))
            {
                var accessor = this.selfHostedDependencies[typeof(PaymentOrchestratorServiceAccessor)];
                this.PaymentOrchestratorServiceAccessor = new PaymentOrchestratorServiceAccessor(
                    serviceBaseUrl: accessor.BaseUri.ToString(),
                    emulatorBaseUrl: accessor.BaseUri.ToString(),
                    apiVersion: "1.1",
                    messageHandler: new WebRequestHandler());
            }
            else
            {
                this.PaymentOrchestratorServiceAccessor = new PaymentOrchestratorServiceAccessor(
                    serviceBaseUrl: "https://mockPaymentOrchestratorService",
                    emulatorBaseUrl: "tbd",
                    apiVersion: "1.1",
                    messageHandler: PaymentOrchestratorService);
            }

            if (this.selfHostedDependencies.ContainsKey(typeof(FraudDetectionServiceAccessor)))
            {
                var accessor = this.selfHostedDependencies[typeof(FraudDetectionServiceAccessor)];
                this.FraudDetectionServiceAccessor = new FraudDetectionServiceAccessor(
                    serviceBaseUrl: accessor.BaseUri.ToString(),
                    emulatorBaseUrl: accessor.BaseUri.ToString(),
                    messageHandler: new WebRequestHandler());
            }
            else
            {
                this.FraudDetectionServiceAccessor = new FraudDetectionServiceAccessor(
                    serviceBaseUrl: "https://mockFraudDetectionService",
                    emulatorBaseUrl: "tbd",
                    messageHandler: FraudDetectionService);
            }

            this.AnomalyDetectionAccessor = new AnomalyDetectionAccessor(
                enableTestHook: true);

            this.D365ServiceAccessor = new D365ServiceAccessor(
                serviceBaseUrl: "https://mockD365Service",
                emulatorBaseUrl: null,
                apiVersion: this.D365ServiceApiVersion,
                messageHandler: D365Service);

            this.ShortURLServiceAccessor = new ShortURLServiceAccessor(
                serviceBaseUrl: "https://mockShortURLService",
                messageHandler: ShortURLService);
                
            this.NetworkTokenizationServiceAccessor = new NetworkTokenizationServiceAccessor(
                serviceBaseUrl: "https://mockNetworkTokenizationService",
                emulatorBaseUrl: string.Empty,
                apiVersion: "1.0",
                messageHandler: NetworkTokenizationService);
        }
    }
}
