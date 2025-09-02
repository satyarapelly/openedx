// <copyright file="PXServiceIntSettings.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net.Http;
    using Accessors.OrchestrationService;
    using global::Azure.Identity;
    using Microsoft.Commerce.Payments.Authentication.AzureActiveDirectoryCaller;
    using Microsoft.Commerce.Payments.Authentication.AzureActiveDirectoryCaller.Settings;
    using Microsoft.Commerce.Payments.PXService.Accessors.FraudDetectionService;
    using Microsoft.Commerce.Payments.PXService.Accessors.NetworkTokenizationService;
    using Microsoft.Commerce.Payments.PXService.Accessors.PaymentOrchestratorService;
    using Microsoft.Commerce.Payments.PXService.Accessors.ShortURLDB;
    using Microsoft.Commerce.Payments.PXService.Accessors.ShortURLService;
    using Microsoft.Commerce.Payments.PXService.Accessors.TokenizationService;
    using Microsoft.Commerce.Payments.PXService.Accessors.TransactionDataService;
    using Microsoft.Commerce.Payments.PXService.AddressEnrichmentService.V7;
    using Microsoft.Commerce.Payments.PXService.MerchantCapabilitiesService.V7;
    using Microsoft.Commerce.Payments.PXService.Model.Authentication;
    using Microsoft.Commerce.Payments.PXService.RiskService.V7;
    using Microsoft.IdentityModel.S2S.Configuration;
    using Newtonsoft.Json;

    public class PXServiceIntSettings : PXServiceSettings
    {
        [SuppressMessage("Microsoft.Maintainability", "CA1506: Avoid excessive class coupling", Justification = "Central class initialization method.")]
        public PXServiceIntSettings()
        {
            this.AadCertificateName = "aad-pxclientauth-paymentexperience-azclient-int-ms";
            this.CtpCertificateName = "ctpcommerce-pxclientauth-paymentexperience-azclient-int-ms";

            this.AadCertificateSubjectName = "aad-pxclientauth.paymentexperience.azclient-int.ms";
            this.CtpCertificateSubjectName = "ctpcommerce-pxclientauth.paymentexperience.azclient-int.ms";

            this.ManagedIdentityId = "bd11e518-82c4-438f-898b-585a8cc3da0d";

            this.LoadCertificate(false, "https://kv-px-int-wus-1.vault.azure.net/", this.ManagedIdentityId); // PXService-INT MI ClientID

            this.LocalFeatureConfigs = PXServiceSettings.FetchStaticFeatureConfigs(
                "Settings\\FeatureConfig\\featureconfigs.json",
                "Settings\\FeatureConfig\\testusergroups.json",
                "int_test_user_group");

            this.PIDLDocumentValidationEnabled = true;

            var azureActiveDirectoryTokenClientOptions = new List<AzureActiveDirectoryTokenClientOption>
            {
                new AzureActiveDirectoryTokenClientOption
                {
                    ClientId = this.ManagedIdentityId, // PXService-INT MI
                    ClientType = AadTokenClientType.ManagedIdentityTokenClient,
                },
                new AzureActiveDirectoryTokenClientOption
                {
                    ClientId = "3e88f276-2b04-48e2-a702-0a75a5284af4", // PX-Service-INT-PME
                    Authority = "https://login.windows.net/975f013f-7f24-47e8-a7d3-abc4752bf346",
                    Certificate = this.AadCertificate,
                    ClientType = AadTokenClientType.MSALAppTokenClient,
                }
            };

            var azureActiveDirectoryTokenLoaderOptions = new List<AzureActiveDirectoryTokenLoaderOption>
            {
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.AzureExPService, this.ManagedIdentityId, "https://exp.azure.net/", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.RiskService, this.ManagedIdentityId, "https://ks.cp.microsoft-int.com", true),
                //// MS rewards removed their app ID in PME tenant and so we had to comment this temporarily
                //// BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.MSRewardsService, this.ManagedIdentityId, "dd171b14-a597-438b-b238-df1c295d5099", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.AccountService, this.ManagedIdentityId, "https://jarvisapi.v2.account.microsoft.com", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.TaxIdService, this.ManagedIdentityId, "api://taxidmanagement.cp.microsoft-int.com", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.StoredValueService, this.ManagedIdentityId, "https://storedvalue.cp.microsoft-int.com", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.InstrumentManagementService, this.ManagedIdentityId, "api://paymentsinstrumentservice-int/63f80278-605c-47bf-b918-aa160922c5ee", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.PartnerSettingsService, this.ManagedIdentityId, "edfcfbd9-8539-404f-bf21-c0eeac690e29", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.SessionService, this.ManagedIdentityId, "520a3785-c79a-4871-b272-1555e58b2dce", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.TransactionService, this.ManagedIdentityId, "520a3785-c79a-4871-b272-1555e58b2dce", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.PayerAuthService, this.ManagedIdentityId, "520a3785-c79a-4871-b272-1555e58b2dce", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.PaymentThirdPartyService, this.ManagedIdentityId, "520a3785-c79a-4871-b272-1555e58b2dce", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.PurchaseService, this.ManagedIdentityId, "https://purchase-int.mp.microsoft.com", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.CatalogService, this.ManagedIdentityId, "https://onestore.microsoft.com", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.TokenPolicyService, this.ManagedIdentityId, "https://onestore.microsoft.com", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.OrchestrationService, this.ManagedIdentityId, "758aa3d6-f5bb-44a9-98de-4361c459b1c4", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.SellerMarketPlaceService, this.ManagedIdentityId, "dcc1e88f-b1b5-46d6-a0ab-292eb83610c0", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.ShortURLService, this.ManagedIdentityId, "65e6464b-008a-4d5f-a5de-2cc2e5bf440e", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.IssuerService, this.ManagedIdentityId, "https://mspmecloud.onmicrosoft.com/issuerservice-int", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.WalletService, this.ManagedIdentityId, "api://044d0f5c-5294-4205-809a-3bc68ffaec98", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.TransactionDataService, this.ManagedIdentityId, "520a3785-c79a-4871-b272-1555e58b2dce", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.ChallengeManagementService, this.ManagedIdentityId, "d3f17ace-65c0-413f-ad99-2a3915810834", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.NetworkTokenizationService, this.ManagedIdentityId, "api://nts-int/716099fb-f878-40a0-9d46-3caa8502776a", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.AddressEnrichmentService, this.ManagedIdentityId, "02dc602e-5077-48eb-8385-55751a2fa661", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.PaymentOrchestratorService, this.ManagedIdentityId, "4f1510c7-3540-4d26-a2b7-87dc55a996b6", true),
                BuildAADTokenLoaderOption(PXCommon.Constants.ServiceNames.FraudDetectionService, this.ManagedIdentityId, "api://f1bca360-a368-45d3-92f1-24287e23a870", true),
            };

            var authenticationLogger = new TokenGenerationLogger();
            var azureActiveDirectoryTokenClientFactory = new AzureActiveDirectoryTokenClientFactory(azureActiveDirectoryTokenClientOptions, authenticationLogger);
            this.AzureActiveDirectoryTokenLoaderFactory = new AzureActiveDirectoryTokenLoaderFactory(azureActiveDirectoryTokenLoaderOptions, azureActiveDirectoryTokenClientFactory, authenticationLogger);

            var azureExpMessageHandler = GetAADRequestHandler(PXCommon.Constants.ServiceNames.AzureExPService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.AzureExPAccessor = new AzureExPAccessor(
                expBlobUrl: "https://default.exp-tas.com/exptas9/6efb2a85-0548-45d7-9c66-3201f7dfcd7b-paymentexpint/api/v1/experimentationblob",
                tokenLoader: this.AzureActiveDirectoryTokenLoaderFactory.GetActiveDirectoryTokenLoader(PXCommon.Constants.ServiceNames.AzureExPService),
                messageHandler: azureExpMessageHandler);

            var orchestrationMessageHandler = GetAADRequestHandler(PXCommon.Constants.ServiceNames.OrchestrationService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.OrchestrationServiceAccessor = new OrchestrationServiceAccessor(
                serviceBaseUrl: "https://orchestration-int.paymentsinstrument.cp.microsoft-int.com",
                emulatorBaseUrl: "tbd",
                apiVersion: "v1.0",
                messageHandler: orchestrationMessageHandler);

            this.AnomalyDetectionAccessor = new AnomalyDetectionAccessor(
                adResultsContainerPath: "https://pxadresultsint.blob.core.windows.net/adresults",
                tokenCredential: new ManagedIdentityCredential(clientId: this.ManagedIdentityId));

            this.RDSServiceAccessor = new RDSServiceAccessor(
                baseUrl: "https://pmservices.cp.microsoft-int.com/RedirectionService/CoreRedirection",
                messageHandler: new HttpClientHandler());

            this.RiskServiceRequestHandler = PXServiceSettings.GetAADRequestHandler(PXCommon.Constants.ServiceNames.RiskService, this.AzureActiveDirectoryTokenLoaderFactory);

            var taxIdWebRequestHandler = GetAADRequestHandler(PXCommon.Constants.ServiceNames.TaxIdService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.TaxIdServiceAccessor = new TaxIdServiceAccessor(
                serviceBaseUrl: "https://taxidmanagement.cp.microsoft-int.com",
                messageHandler: taxIdWebRequestHandler);

            this.CommerceAccountDataServiceAccessor = new CommerceAccountDataAccessor(
                baseUrl: "https://sps.msn-int.com/Commerce/Account/AccountWebService.svc",
                authCert: this.CtpCertificate);

            this.CtpCommerceDataServiceAccessor = new CTPCommerceDataAccessor(
                baseUrl: "https://sps.msn-int.com/CTPCommerce/CommerceAPI.svc",
                authCert: this.CtpCertificate);

            this.MerchantCapabilitiesUri = "https://merchant.pay.microsoft-ppe.com";
            this.MerchantCapabilitiesApiVersion = "v1";
            this.MerchantCapabilitiesAccessor = new MerchantCapabilitiesAccessor(this);

            var sessionServiceRequestHandler = GetAADRequestHandler(PXCommon.Constants.ServiceNames.SessionService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.SessionServiceAccessor = new SessionServiceAccessor(
                baseUrl: "https://sessionservice.cp.microsoft-int.com",
                apiVersion: "2015-09-23",
                requestHandler: sessionServiceRequestHandler);

            this.PXSessionTokenIssuer = "https://paymentexperience-test.cp.microsoft-int.com/px";
            this.PXSessionTokenValidityPeriod = 20;

            var addressEnrichmentWebRequestHandlerV2 = GetAADRequestHandler(PXCommon.Constants.ServiceNames.AddressEnrichmentService, this.AzureActiveDirectoryTokenLoaderFactory);
            var addressEnrichmentServiceUrlV2 = "https://addressvalidationservice.microsoft-int.com";
            this.AddressEnrichmentServiceAccessor = new AddressEnrichmentServiceAccessor(
                emulatorBaseUrl: string.Empty,
                serviceBaseUrl: addressEnrichmentServiceUrlV2,
                messageHandler: addressEnrichmentWebRequestHandlerV2);

            this.IssuerServiceApiVersion = GlobalConstants.IssuerServiceApiVersions.V1;

            var shortUrlRequestHandler = GetAADRequestHandler(PXCommon.Constants.ServiceNames.ShortURLService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.ShortURLServiceAccessor = new ShortURLServiceAccessor(
                serviceBaseUrl: "https://pay-int.com",
                messageHandler: shortUrlRequestHandler);

            this.ShortURLDBAccessor = new ShortURLDBAccessor("https://px-shorturl-db-int.documents.azure.com:443/", "pay-ms-int.com");

            var transactionDataWebRequestHandler = GetAADRequestHandler(PXCommon.Constants.ServiceNames.TransactionDataService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.TransactionDataServiceAccessor = new TransactionDataServiceAccessor(
                serviceBaseUrl: "https://transactiondataservice.cp.microsoft-int.com",
                apiVersion: "2018-05-07",
                messageHandler: transactionDataWebRequestHandler);

            var networkTokenizationServiceRequestHandler = GetAADRequestHandler(PXCommon.Constants.ServiceNames.NetworkTokenizationService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.NetworkTokenizationServiceAccessor = new NetworkTokenizationServiceAccessor(
                serviceBaseUrl: "https://nts.cp.microsoft-int.com",
                intServiceBaseUrl: "https://nts.cp.microsoft-int.com",
                emulatorBaseUrl: string.Empty,
                apiVersion: "1.0",
                messageHandler: networkTokenizationServiceRequestHandler);
                
            this.TokenizationServiceAccessor = new TokenizationServiceAccessor(
                serviceBaseUrl: "https://tokenization.cp.microsoft-int.com",
                emulatorBaseUrl: "TBD",
                tokenizationGetTokenURL: "https://tokenization.cp.microsoft-int.com/tokens",
                tokenizationGetTokenFromEncryptedValueURL: "https://tokenizationfd.cp.microsoft-int.com/tokens",
                 messageHandler: new HttpClientHandler());

            ResourceLifecycleStateManager.Initialize(
                this,
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceLifecycleStateManager.ResourceLifecycleConstants.ErrorConfigFilePath),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceLifecycleStateManager.ResourceLifecycleConstants.ClientActionConfigFilePath));

            this.JsonSerializerSettings = new JsonSerializerSettings();
            this.JsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            this.JsonSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            this.PifdBaseUrl = "https://pifd.cp.microsoft-int.com/V6.0";

            this.PayMicrosoftBaseUrl = "https://payint.microsoft.com";

            this.StaticResourceServiceBaseUrl = "https://staticresources.payments.microsoft-int.com/staticresourceservice";

            this.InitializeAuthenticationSettings();
        }

        protected HttpMessageHandler RiskServiceRequestHandler { get; private set; }

        private void InitializeAuthenticationSettings()
        {
            IList<UserInformation> partnerInformationList = new List<UserInformation>()
            {
                new UserInformation()
                {
                    Role = GlobalConstants.ClientRoles.Test,
                    AllowedAccounts = "18c69db8-dd2e-48a0-8887-1ccabd0bbcb2,3cfa0e51-97ae-49a8-9a71-398ca2ba0683,62dc8681-6753-484a-981a-128f82a43d25,7e5242d0-33ea-4bd1-a691-5193af93c4c7,ec8c235c-65e2-4a3d-bd7d-a20ed8ec1688",
                    AllowedAuthenticatedPathTemplate = "/v7.0/{0}",
                    AllowedUnAuthenticatedPaths = "/v7.0/settings/Microsoft.Payments.Client,/v7.0/addresses/legacyValidate,/v7.0/addresses/modernValidate,/v7.0/paymentSessions,/v7.0/sessions,/v7.0/checkoutsEx,/v7.0/checkoutDescriptions,/v7.0/shortURL",
                    PartnerName = Partner.Name.PXCOT.ToString(),
                    ApplicationId = "a2b81e22-f9e9-436b-8bf8-f3d41fc2516e" // Application name: PX-MI-COT-INT 
                },
                new UserInformation()
                {
                    Role = GlobalConstants.ClientRoles.Admin,
                    PartnerName = Partner.Name.PIFDService.ToString(),
                    CertificateVerificationRule = new Management.CertificateVerificationCore.VerifyBySubjectIssuerThumbprint(
                        "CN=clientauth-pifd.pims-int.azclient.ms",
                        new List<Management.CertificateVerificationCore.IssuerGroup>()
                        {
                            Management.CertificateVerificationCore.IssuerGroup.AME
                        }),
                    ApplicationId = "20ef9c38-bec0-4c7a-922b-b33cc638592b" // Application name: mi-pifd-int-gbl-aad-wu2, tenant id (PME) : 975f013f-7f24-47e8-a7d3-abc4752bf346
                },
                new UserInformation()
                {
                    Role = GlobalConstants.ClientRoles.Admin,
                    PartnerName = Partner.Name.PaymentOrchestrator.ToString(),
                    ApplicationId = "4f1510c7-3540-4d26-a2b7-87dc55a996b6" // Application name: PaymentsAPI-MSDP-INT, tenant id (PME) : 975f013f-7f24-47e8-a7d3-abc4752bf346
                }
            };

            var uberUserDirectory = new UberUserDirectory(partnerInformationList);

            var aadAuthOptions = new AadAuthenticationOptions
            {
                ClientId = "ede6832e-2581-4c10-8141-9b4cbe81e06c",  // Payment Experience Service INT FPA Client ID
                TenantId = "975f013f-7f24-47e8-a7d3-abc4752bf346",  // PX PME Tenant ID
                Audience = "https://paymentexperience-test.cp.microsoft-int.com/",
                Instance = "https://login.microsoftonline.com/"
            };

            this.AuthorizationFilter = new PXServiceAuthorizationFilterAttribute
            {
                AllowUnauthenticatedHttpsCalls = false,
                AllowUnauthenticatedHttpCalls = true,
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