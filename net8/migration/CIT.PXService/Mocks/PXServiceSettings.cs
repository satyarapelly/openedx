// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace CIT.PXService.Mocks
{
    using System.IO;
    using Microsoft.Commerce.Payments.PXService;
    using Microsoft.Commerce.Payments.PXService.AddressEnrichmentService.V7;
    using Microsoft.Commerce.Payments.PXService.RiskService.V7;
    using Microsoft.Commerce.Payments.PXService.Accessors.OrchestrationService;
    using Microsoft.Commerce.Payments.PXService.Accessors.D365Service;
    using Microsoft.Commerce.Payments.PXService.Accessors.ShortURLService;

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

        public Mocks.CommerceAccountDataAccessor CommerceAccountDataService { get; private set; }

        public Mocks.CTPCommerceDataAccessor CtpCommerceDataService { get; private set; }

        public Mocks.ShortURLService ShortURLService { get; private set; }

        public PXServiceSettings()
        {
            AccountsService = new Mocks.AccountService();
            PimsService = new Mocks.PimsService();
            OrchestrationService = new Mocks.OrchestrationService();
            PayerAuthService = new Mocks.PayerAuthService();
            SessionService = new Mocks.SessionService();
            StoredValueService = new Mocks.StoredValueService();
            RiskService = new Mocks.RiskService();
            TaxIdService = new Mocks.TaxIdService();
            PurchaseService = new Mocks.PurchaseService();
            D365Service = new Mocks.D365Service();
            CatalogService = new Mocks.CatalogService();            
            AddressEnrichmentService = new Mocks.AddressEnrichmentService();
            TransactionService = new Mocks.TransactionService();
            ShortURLService = new Mocks.ShortURLService();

            CommerceAccountDataService = new Mocks.CommerceAccountDataAccessor();
            CtpCommerceDataService = new Mocks.CTPCommerceDataAccessor();

            this.ValidateCors = false;
            this.AddCorsAllowedOrigin("https://pidlsdktestportal.azurewebsites.net");
            this.AddCorsAllowedOrigin("http://localhost:3000");

            this.LocalFeatureConfigs = PXServiceSettings.FetchStaticFeatureConfigs(
                "Settings\\FeatureConfig\\featureconfigs.json",
                "Settings\\FeatureConfig\\testusergroups.json",
                "int_test_user_group");

            this.PifdBaseUrl = "https://pifd.cp.microsoft-int.com/V6.0";

            this.PIMSAccessor = new PIMSAccessor(
                serviceBaseUrl: "https://mockPims",
                emulatorBaseUrl: "https://testEmulatorBaseUrl",
                apiVersion: "TestApiVersion",
                messageHandler: PimsService);

            this.OrchestrationServiceAccessor = new OrchestrationServiceAccessor(
                serviceBaseUrl: "https://mockOrchestrationService",
                emulatorBaseUrl: "https://testEmulatorBaseUrl",
                apiVersion: "TestApiVersion",
                authTokenGetter: new Mocks.AuthTokenGetter(),
                messageHandler: OrchestrationService);

            this.AccountServiceAccessor = new AccountServiceAccessor(
                serviceBaseUrl: "https://mockAccountService",
                emulatorBaseUrl: "https://testEmulatorBaseUrl",
                messageHandler: AccountsService);

            this.PayerAuthServiceAccessor = new PayerAuthServiceAccessor(
                serviceBaseUrl: "https://mockPayerAuthService",
                emulatorBaseUrl: "https://testEmulatorBaseUrl",
                apiVersion: "testApiVersion",
                messageHandler: PayerAuthService,
                authTokenGetter: new Mocks.AuthTokenGetter());

            this.PurchaseServiceAccessor = new PurchaseServiceAccessor(
                serviceBaseUrl: "https://mockPurchaseService",
                emulatorBaseUrl: null,
                apiVersion: this.PurchaseServiceApiVersion,
                authTokenGetter: new Mocks.AuthTokenGetter(),
                messageHandler: PurchaseService);

            this.D365ServiceAccessor = new D365ServiceAccessor(
                serviceBaseUrl: "https://mockD365Service",
                emulatorBaseUrl: null,
                apiVersion: this.D365ServiceApiVersion,
                authTokenGetter: new Mocks.AuthTokenGetter(),
                messageHandler: D365Service);

            this.CatalogServiceAccessor = new CatalogServiceAccessor(
                serviceBaseUrl: "https://mockPurchaseService",
                emulatorBaseUrl: null,
                apiVersion: this.CatalogServiceApiVersion,
                authTokenGetter: new Mocks.AuthTokenGetter(),
                messageHandler: CatalogService);

            this.SessionServiceAccessor = new SessionServiceAccessor(
                baseUrl: "https://mockSessionService",
                apiVersion: "2015-09-23",
                requestHandler: SessionService,
                authTokenGetter: new Mocks.AuthTokenGetter());

            this.StoredValueServiceAccessor = new StoredValueAccessor(
                apiVersion: "2014-10-10",
                serviceBaseUrl: "http://localhost/StoredValueEmulator", // lgtm[cs/non-https-url] Suppressing Semmle warning // DevSkim: ignore DS137138 as this to access the locally hosted endpoint
                emulatorBaseUrl: "http://localhost/StoredValueEmulator", // lgtm[cs/non-https-url] Suppressing Semmle warning // DevSkim: ignore DS137138 as this to access the locally hosted endpoint
                messageHandler: StoredValueService);

            this.RiskServiceAccessor = new RiskServiceAccessor(
                serviceBaseUrl: "https://mockRiskService",
                apiVersion: "2015-09-23",
                messageHandler: RiskService);

            this.TaxIdServiceAccessor = new TaxIdServiceAccessor(
                serviceBaseUrl: "https://mockTaxIdService",
                messageHandler: TaxIdService);

            this.AddressEnrichmentServiceAccessor = new AddressEnrichmentServiceAccessor(
                serviceBaseUrl: "https://mockAddressEnrichmentService",
                keyVaultAccessor: new Mocks.KeyVaultAccessor(),
                addressEnrichmentApiKeySecretName: "AddressEncrichmentApiKey",
                messageHandler: AddressEnrichmentService);

            ////this.AddressEnrichmentServiceAccessor = new AddressEnrichmentServiceAccessor(
            ////    serviceBaseUrl: "https://enrichment.cdsk.microsoft-int.com",
            ////    keyVaultAccessor: new Microsoft.Commerce.Payments.PXService.KeyVaultAccessor(
            ////        vaultName: "pxservice-int",
            ////        clientId: "53fe0d08-a3e3-4bd8-af64-08006b1869d6",
            ////        authCert: new X509Certificate2(CertificateHelper.GetCertificateByName("My", "aad-pxclientauth-int.cp.microsoft.com", true))),
            ////    addressEnrichmentApiKeySecretName: "AddressEncrichmentApiKey",
            ////    messageHandler: AddressEnrichmentService);

            this.TransactionServiceAccessor = new TransactionServiceAccessor(
                serviceBaseUrl: "https://mockTransactionService",
                emulatorBaseUrl: "http://localhost/TransactionServiceEmulator", // DevSkim: ignore DS137138 as this to access the locally hosted endpoint
                apiVersion: "2018-05-07",
                authTokenGetter: new Mocks.AuthTokenGetter(),
                messageHandler: TransactionService);

            this.ShortURLServiceAccessor = new ShortURLServiceAccessor(
                serviceBaseUrl: "https://mockShortURLService",
                emulatorBaseUrl: "https://testEmulatorBaseUrl",
                messageHandler: ShortURLService);

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
    }
}
