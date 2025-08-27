// <copyright file="PXServiceProdSettings.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net.Http;
    using Accessors.OrchestrationService;
    using global::Azure.Identity;
    using MerchantCapabilitiesService.V7;
    using Microsoft.Commerce.Payments.Authentication.AzureActiveDirectoryCaller;
    using Microsoft.Commerce.Payments.Authentication.AzureActiveDirectoryCaller.Settings;
    using Microsoft.Commerce.Payments.PXService.Accessors.FraudDetectionService;
    using Microsoft.Commerce.Payments.PXService.Accessors.NetworkTokenizationService;
    using Microsoft.Commerce.Payments.PXService.Accessors.PaymentOrchestratorService;
    using Microsoft.Commerce.Payments.PXService.Accessors.ShortURLService;
    using Microsoft.Commerce.Payments.PXService.Accessors.TokenizationService;
    using Microsoft.Commerce.Payments.PXService.Accessors.TransactionDataService;
    using Microsoft.Commerce.Payments.PXService.AddressEnrichmentService.V7;
    using Microsoft.Commerce.Payments.PXService.Model.Authentication;
    using Microsoft.Commerce.Payments.PXService.RiskService.V7;
    using Microsoft.IdentityModel.S2S.Configuration;
    using Newtonsoft.Json;

    public abstract class PXServiceProdSettings : PXServiceSettings
    {
        [SuppressMessage("Microsoft.Maintainability", "CA1506: Avoid excessive class coupling", Justification = "Central class initialization method.")]
        public PXServiceProdSettings()
        {
            this.AadCertificateName = "aad-pxclientauth-paymentexperience-azclient-ms";
            this.CtpCertificateName = "ctpcommerce-pxclientauth-paymentexperience-azclient-ms";

            this.AadCertificateSubjectName = "aad-pxclientauth.paymentexperience.azclient.ms";
            this.CtpCertificateSubjectName = "ctpcommerce-pxclientauth.paymentexperience.azclient.ms";

            this.ManagedIdentityId = "de6f8403-2bc1-4e12-87ce-b0a9b4397ce1";

            this.LoadCertificate(false, "https://px-kv-prod.vault.azure.net/", this.ManagedIdentityId); // PaymentExperience-PROD MI ClientID

            this.LocalFeatureConfigs = PXServiceSettings.FetchStaticFeatureConfigs(
                "Settings\\FeatureConfig\\featureconfigs.json",
                "Settings\\FeatureConfig\\testusergroups.json",
                "prod_test_user_group");

            var azureActiveDirectoryTokenClientOptions = new List<AzureActiveDirectoryTokenClientOption>
            {
                new AzureActiveDirectoryTokenClientOption
                {
                    ClientId = this.ManagedIdentityId,
                    ClientType = AadTokenClientType.ManagedIdentityTokenClient,
                },
                new AzureActiveDirectoryTokenClientOption
                {
                    ClientId = "8be7ced7-e5fe-40a8-81c1-de6363e41d41", // PX-Service-PROD-PME
                    Authority = "https://login.windows.net/975f013f-7f24-47e8-a7d3-abc4752bf346",
                    Certificate = this.AadCertificate,
                    ClientType = AadTokenClientType.MSALAppTokenClient,
                }
            };

            var azureActiveDirectoryTokenLoaderOptions = new List<AzureActiveDirectoryTokenLoaderOption>
            {
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.AzureExPService, this.ManagedIdentityId, "https://exp.azure.net/", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.RiskService, this.ManagedIdentityId, "https://ks.cp.microsoft.com", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.MSRewardsService, this.ManagedIdentityId, "7c6d467d-d205-4849-bc3f-09b60470b5bb", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.TaxIdService, this.ManagedIdentityId, "https://taxidmanagement.cp.microsoft.com", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.StoredValueService, this.ManagedIdentityId, "https://storedvalue.cp.microsoft.com", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.InstrumentManagementService, this.ManagedIdentityId, "api://paymentsinstrumentservice-prod/37cf75f6-f334-4416-8aec-07f940a25736", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.AccountService, this.ManagedIdentityId, "https://jarvisapi.v2.account.microsoft.com", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.PartnerSettingsService, this.ManagedIdentityId, "0d35679a-9429-48cc-afa5-65ebc5eb386c", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.SessionService, this.ManagedIdentityId, "0055be90-94c1-4e92-971f-1c8f3b3aee43", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.TransactionService, this.ManagedIdentityId, "0055be90-94c1-4e92-971f-1c8f3b3aee43", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.PayerAuthService, this.ManagedIdentityId, "0055be90-94c1-4e92-971f-1c8f3b3aee43", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.PaymentThirdPartyService, this.ManagedIdentityId, "0055be90-94c1-4e92-971f-1c8f3b3aee43", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.PurchaseService, this.ManagedIdentityId, "https://onestore.microsoft.com", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.CatalogService, this.ManagedIdentityId, "https://onestore.microsoft.com", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.TokenPolicyService, this.ManagedIdentityId, "https://onestore.microsoft.com", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.OrchestrationService, this.ManagedIdentityId, "f074233c-97f4-4296-b80a-0f5753ee83e8", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.SellerMarketPlaceService, this.ManagedIdentityId, "23cfbf1b-13a3-44c5-931c-dc3d4092bd8c", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.ShortURLService, this.ManagedIdentityId, "8cf6afd3-8a58-4947-9c8b-373071d1e4b1", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.IssuerService, this.ManagedIdentityId, "https://mspmecloud.onmicrosoft.com/issuerservice-prod", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.D365Service, "8be7ced7-e5fe-40a8-81c1-de6363e41d41", "api://20c33330-43b0-4b20-ab9e-fca47ebf3069", false),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.WalletService, this.ManagedIdentityId, "api://8eb2dd4c-8159-453b-9202-f05a060dbfb7", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.TransactionDataService, this.ManagedIdentityId, "0055be90-94c1-4e92-971f-1c8f3b3aee43", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.ChallengeManagementService, this.ManagedIdentityId, "8d15aaed-718d-48b0-98e0-d20eb96b3aa9", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.NetworkTokenizationService, this.ManagedIdentityId, "api://nts-prod/c7921391-15ba-44fa-900f-5f0dc3f8eb2b", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.AddressEnrichmentService, this.ManagedIdentityId, "02dc602e-5077-48eb-8385-55751a2fa661", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.PaymentOrchestratorService, this.ManagedIdentityId, "4ee094d4-c5b9-482d-ba03-a862e5592543", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.FraudDetectionService, this.ManagedIdentityId, "api://6060294b-3a13-4313-877e-8751b44c9254", true),
            };

            var authenticationLogger = new TokenGenerationLogger();
            var azureActiveDirectoryTokenClientFactory = new AzureActiveDirectoryTokenClientFactory(azureActiveDirectoryTokenClientOptions, authenticationLogger);
            this.AzureActiveDirectoryTokenLoaderFactory = new AzureActiveDirectoryTokenLoaderFactory(azureActiveDirectoryTokenLoaderOptions, azureActiveDirectoryTokenClientFactory, authenticationLogger);

            var azureExpMessageHandler = PXServiceSettings.GetAADRequestHandler(PXCommon.Constants.ServiceNames.AzureExPService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.AzureExPAccessor = new AzureExPAccessor(
                expBlobUrl: "https://default.exp-tas.com/exptas49/03947902-89f9-4d38-972c-251138ba5b61-paymentexpprd/api/v1/experimentationblob",
                tokenLoader: this.AzureActiveDirectoryTokenLoaderFactory.GetActiveDirectoryTokenLoader(PXCommon.Constants.ServiceNames.AzureExPService),
                messageHandler: azureExpMessageHandler);

            this.AnomalyDetectionAccessor = new AnomalyDetectionAccessor(
                adResultsContainerPath: "https://pxadresultsint.blob.core.windows.net/adresults",
                tokenCredential: new ManagedIdentityCredential(clientId: this.ManagedIdentityId));

            this.PimsBaseUrl = "https://paymentsinstrumentservice.cp.microsoft.com/InstrumentManagementService";
            this.PimsApiVersion = "2014-09-30";
            this.PimsRequestHandler = PXServiceSettings.GetAADRequestHandler(PXCommon.Constants.ServiceNames.InstrumentManagementService, this.AzureActiveDirectoryTokenLoaderFactory);

            this.RDSServiceAccessor = new RDSServiceAccessor(
                baseUrl: "https://pmservices.cp.microsoft.com/RedirectionService/CoreRedirection",
                messageHandler: new WebRequestHandler());

            this.OrchestrationServiceRequestHandler = PXServiceSettings.GetAADRequestHandler(PXCommon.Constants.ServiceNames.OrchestrationService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.OrchestrationServiceBaseUrl = "https://orchestration.paymentsinstrument.commerce.microsoft.com";
            this.OrchestrationServiceAccessor = new OrchestrationServiceAccessor(
                serviceBaseUrl: this.OrchestrationServiceBaseUrl,
                emulatorBaseUrl: "tbd",
                apiVersion: "v1.0",
                messageHandler: this.OrchestrationServiceRequestHandler);

            this.AccountServiceBaseUrl = "https://accounts.cp.microsoft.com";
            this.AccountServiceRequestHandler = PXServiceSettings.GetAADRequestHandler(PXCommon.Constants.ServiceNames.AccountService, this.AzureActiveDirectoryTokenLoaderFactory);

            // this.PayerAuthServiceBaseUrl = "https://10.42.162.179/PayerAuthService"; - For pointing to one of the VIP of Payment Auth
            this.PayerAuthServiceBaseUrl = "https://payerauthservice.cp.microsoft.com/PayerAuthService";
            this.PayerAuthServiceRequestHandler = PXServiceSettings.GetAADRequestHandler(PXCommon.Constants.ServiceNames.PayerAuthService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.PayerAuthServiceApiVersion = GlobalConstants.PayerAuthApiVersions.V3;

            // D365 Service
            this.D365ServiceRequestHandler = PXServiceSettings.GetAADRequestHandler(PXCommon.Constants.ServiceNames.D365Service, this.AzureActiveDirectoryTokenLoaderFactory);
            this.D365ServiceApiVersion = GlobalConstants.D365ServiceApiVersions.V1;

            this.PurchaseServiceBaseUrl = "https://purchase.md.mp.microsoft.com";
            this.PurchaseServiceRequestHandler = PXServiceSettings.GetAADRequestHandler(PXCommon.Constants.ServiceNames.PurchaseService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.PurchaseServiceApiVersion = GlobalConstants.PurchaseApiVersions.V7;

            this.CatalogServiceBaseUrl = "https://frontdoor-displaycatalog.bigcatalog.microsoft.com";
            this.CatalogServiceRequestHandler = PXServiceSettings.GetAADRequestHandler(PXCommon.Constants.ServiceNames.CatalogService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.CatalogServiceApiVersion = GlobalConstants.CatalogApiVersions.V8;

            this.TokenPolicyServiceBaseUrl = "https://tops.mp.microsoft.com";
            this.TokenPolicyServiceRequestHandler = PXServiceSettings.GetAADRequestHandler(PXCommon.Constants.ServiceNames.TokenPolicyService, this.AzureActiveDirectoryTokenLoaderFactory);

            this.RiskServiceRequestHandler = PXServiceSettings.GetAADRequestHandler(PXCommon.Constants.ServiceNames.RiskService, this.AzureActiveDirectoryTokenLoaderFactory);

            this.MSRewardsRequestHandler = PXServiceSettings.GetAADRequestHandler(PXCommon.Constants.ServiceNames.MSRewardsService, this.AzureActiveDirectoryTokenLoaderFactory);

            var taxIdWebRequestHandler = PXServiceSettings.GetAADRequestHandler(PXCommon.Constants.ServiceNames.TaxIdService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.TaxIdServiceAccessor = new TaxIdServiceAccessor(
                serviceBaseUrl: "https://taxidmanagement.cp.microsoft.com",
                messageHandler: taxIdWebRequestHandler);

            this.CommerceAccountDataServiceAccessor = new CommerceAccountDataAccessor(
                baseUrl: "https://sps.msn.com/Commerce/Account/AccountWebService.svc",
                authCert: this.CtpCertificate);

            this.CtpCommerceDataServiceAccessor = new CTPCommerceDataAccessor(
                baseUrl: "https://sps.msn.com/CTPCommerce/CommerceAPI.svc",
                authCert: this.CtpCertificate);

            this.MerchantCapabilitiesUri = "https://merchant.pay.microsoft.com";
            this.MerchantCapabilitiesApiVersion = "v1";
            this.MerchantCapabilitiesAccessor = new MerchantCapabilitiesAccessor(this);

            var sessionServiceRequestHandler = PXServiceSettings.GetAADRequestHandler(PXCommon.Constants.ServiceNames.SessionService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.SessionServiceAccessor = new SessionServiceAccessor(
                baseUrl: "https://sessionservice.cp.microsoft.com",
                apiVersion: "2015-09-23",
                requestHandler: sessionServiceRequestHandler);

            this.StoredValueServiceBaseUrl = "https://storedvalue.cp.microsoft.com";
            this.StoredValueServiceApiVersion = "2014-10-10";
            this.StoredValueServiceRequestHandler = PXServiceSettings.GetAADRequestHandler(PXCommon.Constants.ServiceNames.StoredValueService, this.AzureActiveDirectoryTokenLoaderFactory);

            this.PXSessionTokenIssuer = "https://paymentexperience.cp.microsoft.com/px/";
            this.PXSessionTokenValidityPeriod = 5;

            var addressEnrichmentWebRequestHandlerV2 = PXServiceSettings.GetAADRequestHandler(PXCommon.Constants.ServiceNames.AddressEnrichmentService, this.AzureActiveDirectoryTokenLoaderFactory);
            var addressEnrichmentServiceUrlV2 = "https://addressvalidationservice.microsoft.com";
            this.AddressEnrichmentServiceAccessor = new AddressEnrichmentServiceAccessor(
                serviceBaseUrl: addressEnrichmentServiceUrlV2,
                emulatorBaseUrl: string.Empty,
                messageHandler: addressEnrichmentWebRequestHandlerV2);

            this.WalletServiceWebRequestHandler = PXServiceSettings.GetAADRequestHandler(PXCommon.Constants.ServiceNames.WalletService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.WalletServiceAccessor = new WalletServiceAccessor(
                serviceBaseUrl: "https://paymentswalletservice.cp.microsoft-int.com",
                apiVersion: "2023-1-1",
                messageHandler: this.WalletServiceWebRequestHandler);

            this.TransactionServiceRequestHandler = PXServiceSettings.GetAADRequestHandler(PXCommon.Constants.ServiceNames.TransactionService, this.AzureActiveDirectoryTokenLoaderFactory);

            this.SellerMarketPlaceServiceWebRequestHandler = PXServiceSettings.GetAADRequestHandler(PXCommon.Constants.ServiceNames.SellerMarketPlaceService, this.AzureActiveDirectoryTokenLoaderFactory);

            this.ThirdPartyMarketPlaceServiceWebRequestHandler = PXServiceSettings.GetAADRequestHandler(PXCommon.Constants.ServiceNames.PaymentThirdPartyService, this.AzureActiveDirectoryTokenLoaderFactory);

            this.PartnerSettingsServiceWebRequestHandler = PXServiceSettings.GetAADRequestHandler(PXCommon.Constants.ServiceNames.PartnerSettingsService, this.AzureActiveDirectoryTokenLoaderFactory);

            this.IssuerServiceWebRequestHandler = PXServiceSettings.GetAADRequestHandler(PXCommon.Constants.ServiceNames.IssuerService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.IssuerServiceApiVersion = GlobalConstants.IssuerServiceApiVersions.V1;

            this.ShortUrlServiceWebRequestHandler = PXServiceSettings.GetAADRequestHandler(PXCommon.Constants.ServiceNames.ShortURLService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.ShortURLServiceAccessor = new ShortURLServiceAccessor(
                serviceBaseUrl: "https://www.pay.ms",
                messageHandler: this.ShortUrlServiceWebRequestHandler);

            var transactionDataWebRequestHandler = PXServiceSettings.GetAADRequestHandler(PXCommon.Constants.ServiceNames.TransactionDataService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.TransactionDataServiceAccessor = new TransactionDataServiceAccessor(
                serviceBaseUrl: "https://transactiondataservice.cp.microsoft.com",
                apiVersion: "2018-05-07",
                messageHandler: transactionDataWebRequestHandler);

            this.TokenizationServiceAccessor = new TokenizationServiceAccessor(
                serviceBaseUrl: "https://tokenization.cp.microsoft.com",
                emulatorBaseUrl: "TBD",
                tokenizationGetTokenURL: "https://tokenization.cp.microsoft.com/tokens",
                tokenizationGetTokenFromEncryptedValueURL: "https://tokenizationfd.cp.microsoft.com/tokens",
                messageHandler: new WebRequestHandler());

            this.ChallengeManagementServiceBaseUrl = "https://ChallengeManager-FrontDoor-Prod-dxgpeub8fxgsf3b4.z01.azurefd.net";
            this.ChallengeManagementServiceRequestHandler = PXServiceSettings.GetAADRequestHandler(PXCommon.Constants.ServiceNames.ChallengeManagementService, this.AzureActiveDirectoryTokenLoaderFactory);

            this.NetworkTokenizationServiceRequestHandler = PXServiceSettings.GetAADRequestHandler(PXCommon.Constants.ServiceNames.NetworkTokenizationService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.NetworkTokenizationServiceAccessor = new NetworkTokenizationServiceAccessor(
                serviceBaseUrl: "https://nts.cp.microsoft.com",
                intServiceBaseUrl: "https://nts.cp.microsoft-int.com",
                emulatorBaseUrl: string.Empty,
                apiVersion: "1.0",
                messageHandler: this.NetworkTokenizationServiceRequestHandler);

            var paymentOrchestratorMessageHandler = GetAADRequestHandler(PXCommon.Constants.ServiceNames.PaymentOrchestratorService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.PaymentOrchestratorServiceAccessor = new PaymentOrchestratorServiceAccessor(
                serviceBaseUrl: "https://paymentorchestratorservice.cp.microsoft.com",
                emulatorBaseUrl: "tbd",
                apiVersion: "1.1",
                messageHandler: paymentOrchestratorMessageHandler);

            ResourceLifecycleStateManager.Initialize(
                this,
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceLifecycleStateManager.ResourceLifecycleConstants.ErrorConfigFilePath),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceLifecycleStateManager.ResourceLifecycleConstants.ClientActionConfigFilePath));

            this.PifdBaseUrl = "https://paymentinstruments.mp.microsoft.com/V6.0";

            this.PayMicrosoftBaseUrl = "https://pay.microsoft.com";

            this.StaticResourceServiceBaseUrl = "https://staticresources.payments.microsoft.com/staticresourceservice";

            this.JsonSerializerSettings = new JsonSerializerSettings();
            this.JsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            this.JsonSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            this.InitializeAuthenticationSettings();
        }

        protected string PimsBaseUrl { get; private set; }

        protected string PimsApiVersion { get; private set; }

        protected string OrchestrationServiceBaseUrl { get; private set; }

        protected string AccountServiceBaseUrl { get; private set; }

        protected string PayerAuthServiceBaseUrl { get; private set; }

        protected string PurchaseServiceBaseUrl { get; private set; }

        protected string TokenPolicyServiceBaseUrl { get; private set; }

        protected string CatalogServiceBaseUrl { get; private set; }

        protected string StoredValueServiceBaseUrl { get; private set; }

        protected string ChallengeManagementServiceBaseUrl { get; private set; }

        protected HttpMessageHandler PimsRequestHandler { get; private set; }

        protected HttpMessageHandler OrchestrationServiceRequestHandler { get; private set; }

        protected HttpMessageHandler AccountServiceRequestHandler { get; private set; }

        protected HttpMessageHandler PayerAuthServiceRequestHandler { get; private set; }

        protected HttpMessageHandler PurchaseServiceRequestHandler { get; private set; }

        protected HttpMessageHandler TokenPolicyServiceRequestHandler { get; private set; }

        protected HttpMessageHandler D365ServiceRequestHandler { get; private set; }

        protected HttpMessageHandler CatalogServiceRequestHandler { get; private set; }

        protected HttpMessageHandler StoredValueServiceRequestHandler { get; private set; }

        protected HttpMessageHandler TransactionServiceRequestHandler { get; private set; }

        protected HttpMessageHandler SellerMarketPlaceServiceWebRequestHandler { get; private set; }

        protected HttpMessageHandler ThirdPartyMarketPlaceServiceWebRequestHandler { get; private set; }

        protected HttpMessageHandler MSRewardsRequestHandler { get; private set; }

        protected HttpMessageHandler PartnerSettingsServiceWebRequestHandler { get; private set; }

        protected HttpMessageHandler IssuerServiceWebRequestHandler { get; private set; }

        protected HttpMessageHandler ShortUrlServiceWebRequestHandler { get; private set; }

        protected HttpMessageHandler WalletServiceWebRequestHandler { get; private set; }

        protected HttpMessageHandler ChallengeManagementServiceRequestHandler { get; private set; }

        protected HttpMessageHandler NetworkTokenizationServiceRequestHandler { get; private set; }

        protected HttpMessageHandler RiskServiceRequestHandler { get; private set; }

        // consider refactor the settings
        private void InitializeAuthenticationSettings()
        {
            IList<UserInformation> partnerInformationList = new List<UserInformation>()
            {
                new UserInformation()
                {
                    Role = GlobalConstants.ClientRoles.Test,
                    AllowedAccounts = "5f805b21-d164-41a8-ab3c-7063ecbeb5c9,fed295b8-8aac-4280-b3b7-4cea8da4286b,4015c80e-f369-43a4-8ffe-6e9e7fdca4d6,8505da08-7ebd-4cce-b6d6-bd87e4dadd07,8e22c40d-9011-411c-a09c-c64921959f15,8e22c40d-9011-411c-a09c-c64921959f15,ec8c235c-65e2-4a3d-bd7d-a20ed8ec1688",
                    AllowedAuthenticatedPathTemplate = "/v7.0/{0}",
                    AllowedUnAuthenticatedPaths = "/v7.0/settings/Microsoft.Payments.Client,/v7.0/addresses/legacyValidate,/v7.0/addresses/modernValidate,/v7.0/paymentSessions,/v7.0/sessions,/v7.0/checkoutsEx,/v7.0/checkoutDescriptions",
                    PartnerName = Partner.Name.PXCOT.ToString(),
                    ApplicationId = "a2b81e22-f9e9-436b-8bf8-f3d41fc2516e" // Application name: PX-COT-INT-PME (Managed Identity), tenant id (PME) : 975f013f-7f24-47e8-a7d3-abc4752bf346
                },
                new UserInformation()
                {
                    Role = GlobalConstants.ClientRoles.Admin,
                    PartnerName = Partner.Name.PIFDService.ToString(),
                    CertificateVerificationRule = new Management.CertificateVerificationCore.VerifyBySubjectIssuerThumbprint(
                        "CN=clientauth-pifd.pims.azclient.ms",
                        new List<Management.CertificateVerificationCore.IssuerGroup>()
                        {
                            Management.CertificateVerificationCore.IssuerGroup.AME
                        }),
                    ApplicationId = "a9a83e54-8530-4d39-9f94-ef2ca4fc1832" // Application name: mi-pifd-prod-gbl-aad-wu2, tenant id (PME) : 975f013f-7f24-47e8-a7d3-abc4752bf346
                },
                new UserInformation()
                {
                    Role = GlobalConstants.ClientRoles.Admin,
                    PartnerName = Partner.Name.PIFDServicePPE.ToString(),
                    ApplicationId = "765f46b4-d50c-4a5c-a59d-1767c2ce1039" // Application name: mi-pifd-ppe-gbl-aad-wu2, tenant id (PME) : 975f013f-7f24-47e8-a7d3-abc4752bf346
                },
                new UserInformation()
                {
                    Role = GlobalConstants.ClientRoles.Admin,
                    PartnerName = Partner.Name.PaymentOrchestrator.ToString(),
                    ApplicationId = "4ee094d4-c5b9-482d-ba03-a862e5592543" // Application name: PaymentsAPI-MSDP-PROD, tenant id (PME) : 975f013f-7f24-47e8-a7d3-abc4752bf346
                }
            };

            var uberUserDirectory = new UberUserDirectory(partnerInformationList);

            var aadAuthOptions = new AadAuthenticationOptions
            {
                ClientId = "997f2cfc-edc7-47c2-8103-9837cf31e9f1",  // Payment Experience Service FPA Client ID
                TenantId = "975f013f-7f24-47e8-a7d3-abc4752bf346",  // PX PME Tenant ID
                Audience = "https://paymentexperience.cp.microsoft.com/",
                Instance = "https://login.microsoftonline.com/"
            };

            this.AuthorizationFilter = new PXServiceAuthorizationFilterAttribute
            {
                AllowUnauthenticatedHttpsCalls = false,
                AllowUnauthenticatedHttpCalls = false,
                UberUserDirectory = uberUserDirectory
            };

            this.AuthorizationFilter.TokenMiseValidator = new PXCommon.TokenMiseValidator(aadAuthOptions, new AuthenticationLogger());

            this.AuthorizationFilter.CertificateAuthenticator = new Management.CertificateVerificationCore.UserDirectory(
                users: uberUserDirectory.CertificateVerificationRules,
                online: true,
                issuerFetcher: null,
                verifyRootCA: false,
                verifyOfflineRevocation: false,
                verifyExpirationTime: true,
                logger: new CertificateLogger());
        }
    }
}