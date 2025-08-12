// <copyright file="PXServiceOneBoxSettings.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net.Http;
    using global::Azure.Identity;
    using MerchantCapabilitiesService.V7;
    using Microsoft.Commerce.Payments.PXService.Accessors.ChallengeManagementService;
    using Microsoft.Commerce.Payments.PXService.Accessors.FraudDetectionService;
    using Microsoft.Commerce.Payments.PXService.Accessors.IssuerService;
    using Microsoft.Commerce.Payments.PXService.Accessors.MSRewardsService;
    using Microsoft.Commerce.Payments.PXService.Accessors.NetworkTokenizationService;
    using Microsoft.Commerce.Payments.PXService.Accessors.OrchestrationService;
    using Microsoft.Commerce.Payments.PXService.Accessors.PartnerSettingsService;
    using Microsoft.Commerce.Payments.PXService.Accessors.PaymentOrchestratorService;
    using Microsoft.Commerce.Payments.PXService.Accessors.SellerMarketPlaceService;
    using Microsoft.Commerce.Payments.PXService.Accessors.ShortURLService;
    using Microsoft.Commerce.Payments.PXService.Accessors.TokenizationService;
    using Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService;
    using Microsoft.Commerce.Payments.PXService.Accessors.TransactionDataService;
    using Microsoft.Commerce.Payments.PXService.AddressEnrichmentService.V7;
    using Microsoft.Commerce.Payments.PXService.Model.Authentication;
    using Microsoft.Commerce.Payments.PXService.RiskService.V7;
    using Microsoft.IdentityModel.S2S.Configuration;
    using Newtonsoft.Json;

    public class PXServiceOneBoxSettings : PXServiceSettings
    {
        [SuppressMessage("Microsoft.Maintainability", "CA1506: Avoid excessive class coupling", Justification = "Central class initialization method.")]
        public PXServiceOneBoxSettings()
        {
            this.AddCorsAllowedOrigin("https://pidlsdktestportal.azurewebsites.net");
            this.AddCorsAllowedOrigin("http://localhost:3000");
            this.AddCorsAllowedOrigin("https://payments-acs-emulator-fzeug0g7atchf8d7.b02.azurefd.net");
            this.AddCorsAllowedOrigin("https://pay.microsoft.com");
            this.AddCorsAllowedOrigin("https://payint.microsoft.com");
            this.AddCorsAllowedOrigin("https://payppe.microsoft.com");
            this.AddCorsAllowedOrigin("https://mspayment-ppe-centralus.azurewebsites.net");
            this.AddCorsAllowedOrigin("https://account.microsoft-ppe.com");
            this.AddCorsAllowedOrigin("https://alphastore.microsoft-int.com");

            this.LocalFeatureConfigs = PXServiceSettings.FetchStaticFeatureConfigs(
                "Settings\\FeatureConfig\\featureconfigs.json",
                "Settings\\FeatureConfig\\testusergroups.json",
                "int_test_user_group");

            this.PIDLDocumentValidationEnabled = true;

            this.AzureExpEnabled = false;

            var orchestrationServiceWebRequestHandler = new HttpClientHandler();
            this.OrchestrationServiceAccessor = new OrchestrationServiceAccessor(
                serviceBaseUrl: "https://orchestration-int.paymentsinstrument.cp.microsoft-int.com",
                emulatorBaseUrl: "tbd",
                apiVersion: "v1.0",
                messageHandler: orchestrationServiceWebRequestHandler);

            this.AnomalyDetectionAccessor = new AnomalyDetectionAccessor(
                adResultsContainerPath: "https://pxadresultsint.blob.core.windows.net/adresults",
                tokenCredential: new ManagedIdentityCredential(clientId: this.ManagedIdentityId));

            var pimsWebRequestHandler = new HttpClientHandler();
            this.PIMSAccessor = new PIMSAccessor(
                serviceBaseUrl: "https://paymentsinstrumentservice.cp.microsoft-int.com/InstrumentManagementService",
                emulatorBaseUrl: "http://localhost/PXDependencyEmulators", // lgtm[cs/non-https-url] Suppressing Semmle warning // DevSkim: ignore DS137138 as this to access the locally hosted endpoint
                servicePPEBaseUrl: null,
                apiVersion: "2014-09-30",
                messageHandler: pimsWebRequestHandler);
            this.PIMSAccessorFlightEnabled = true;

            this.RDSServiceAccessor = new RDSServiceAccessor(
                baseUrl: "https://pmservices.cp.microsoft-int.com/RedirectionService/CoreRedirection",
                messageHandler: new HttpClientHandler());

            var accountServiceRequestHandler = new HttpClientHandler();
            this.AccountServiceAccessor = new AccountServiceAccessor(
                serviceBaseUrl: "https://accounts.cp.microsoft-int.com",
                emulatorBaseUrl: "http://localhost/PXDependencyEmulators", // lgtm[cs/non-https-url] Suppressing Semmle warning // DevSkim: ignore DS137138 as this to access the locally hosted endpoint
                messageHandler: accountServiceRequestHandler);

            var riskWebRequestHandler = new HttpClientHandler();
            this.RiskServiceAccessor = new RiskServiceAccessor(
                serviceBaseUrl: "https://ks.cp.microsoft-int.com",
                emulatorBaseUrl: "http://localhost/PXDependencyEmulators", // lgtm[cs/non-https-url] Suppressing Semmle warning // DevSkim: ignore DS137138 as this to access the locally hosted endpoint
                apiVersion: "2015-02-28",
                messageHandler: riskWebRequestHandler);

            var msRewardsRequestHandler = new HttpClientHandler();
            this.MSRewardsServiceAccessor = new MSRewardsServiceAccessor(
                serviceBaseUrl: "https://int.rewardsplatform.microsoft.com",
                emulatorBaseUrl: "http://localhost/PXDependencyEmulators", // lgtm[cs/non-https-url] Suppressing Semmle warning // DevSkim: ignore DS137138 as this to access the locally hosted endpoint
                messageHandler: msRewardsRequestHandler);

            var taxIdWebRequestHandler = new HttpClientHandler();
            this.TaxIdServiceAccessor = new TaxIdServiceAccessor(
                serviceBaseUrl: "https://taxidmanagement.cp.microsoft-int.com",
                messageHandler: taxIdWebRequestHandler);

            //this.CommerceAccountDataServiceAccessor = new CommerceAccountDataAccessor(
            //    baseUrl: "https://sps.msn-int.com/Commerce/Account/AccountWebService.svc",
            //    authCert: null);

            //this.CtpCommerceDataServiceAccessor = new CTPCommerceDataAccessor(
            //    baseUrl: "https://sps.msn-int.com/CTPCommerce/CommerceAPI.svc",
            //    authCert: null);

            this.MerchantCapabilitiesUri = "https://merchant.pay.microsoft-ppe.com";
            this.MerchantCapabilitiesApiVersion = "v1";
            this.MerchantCapabilitiesAccessor = new MerchantCapabilitiesAccessor(this);

            var sessionServiceRequestHandler = new HttpClientHandler();
            this.SessionServiceAccessor = new SessionServiceAccessor(
                baseUrl: "https://sessionservice.cp.microsoft-int.com",
                apiVersion: "2015-09-23",
                requestHandler: sessionServiceRequestHandler);

            this.JsonSerializerSettings = new JsonSerializerSettings();
            this.JsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            this.JsonSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            var payAuthWebRequestHandler = new HttpClientHandler();
            this.PayerAuthServiceAccessor = new PayerAuthServiceAccessor(
                serviceBaseUrl: "https://payerauthservice.cp.microsoft-int.com/PayerAuthService",
                emulatorBaseUrl: "http://localhost/PXDependencyEmulators", // lgtm[cs/non-https-url] Suppressing Semmle warning // DevSkim: ignore DS137138 as this to access the locally hosted endpoint
                apiVersion: GlobalConstants.PayerAuthApiVersions.V3,
                messageHandler: payAuthWebRequestHandler);

            var purchaseWebRequestHandler = new HttpClientHandler();
            this.PurchaseServiceAccessor = new PurchaseServiceAccessor(
                //// serviceBaseUrl: "https://purchase-int.mp.microsoft.com" (INT), serviceBaseUrl: "https://purchase.md.mp.microsoft.com" (PROD),
                serviceBaseUrl: "http://localhost/PXDependencyEmulators", // lgtm[cs/non-https-url] Suppressing Semmle warning // DevSkim: ignore DS137138 as this to access the locally hosted endpoint
                emulatorBaseUrl: "http://localhost/PXDependencyEmulators", // lgtm[cs/non-https-url] Suppressing Semmle warning // DevSkim: ignore DS137138 as this to access the locally hosted endpoint
                apiVersion: GlobalConstants.PurchaseApiVersions.V7,
                messageHandler: purchaseWebRequestHandler);

            var catalogWebRequestHandler = new HttpClientHandler();
            this.CatalogServiceAccessor = new CatalogServiceAccessor(
                ////serviceBaseUrl: "https://frontdoor-displaycatalog.bigcatalog.microsoft.com", emulatorBaseUrl: "http://WD.CPPaymentExperienceService-Test-CO4.CO4.ap.gbl/CatalogService",
                serviceBaseUrl: "http://localhost/PXDependencyEmulators",
                emulatorBaseUrl: "http://localhost/PXDependencyEmulators", // lgtm[cs/non-https-url] Suppressing Semmle warning // DevSkim: ignore DS137138 as this to access the locally hosted endpoint
                apiVersion: GlobalConstants.CatalogApiVersions.V8,
                messageHandler: catalogWebRequestHandler);

            var tokenPolicyRequestHandler = new HttpClientHandler();
            this.TokenPolicyServiceAccessor = new TokenPolicyServiceAccessor(
                serviceBaseUrl: "https://tops-int.mp.microsoft.com",
                emulatorBaseUrl: "http://localhost/PXDependencyEmulators", // lgtm[cs/non-https-url] Suppressing Semmle warning // DevSkim: ignore DS137138 as this to access the locally hosted endpoint
                messageHandler: tokenPolicyRequestHandler);

            var storedValueWebRequestHandler = new HttpClientHandler();
            this.StoredValueServiceAccessor = new StoredValueAccessor(
                apiVersion: "2014-10-10",
                serviceBaseUrl: "https://storedvalue.cp.microsoft-int.com",
                emulatorBaseUrl: "http://localhost/PXDependencyEmulators", // lgtm[cs/non-https-url] Suppressing Semmle warning // DevSkim: ignore DS137138 as this to access the locally hosted endpoint
                messageHandler: storedValueWebRequestHandler);

            this.PXSessionTokenIssuer = "https://paymentexperience-test.cp.microsoft-int.com/px";
            this.PXSessionTokenValidityPeriod = 20;

            var addressEnrichmentWebRequestHandler = new HttpClientHandler();
            this.AddressEnrichmentServiceAccessor = new AddressEnrichmentServiceAccessor(
                serviceBaseUrl: " https://addressvalidationservice.microsoft-int.com",
                emulatorBaseUrl: "http://localhost/PXDependencyEmulators", // lgtm[cs/non-https-url] Suppressing Semmle warning // DevSkim: ignore DS137138 as this to access the locally hosted endpoint
                messageHandler: addressEnrichmentWebRequestHandler);

            var transactionServiceWebRequestHandler = new HttpClientHandler();
            this.TransactionServiceAccessor = new TransactionServiceAccessor(
                serviceBaseUrl: "https://paymentstransactionservice.cp.microsoft-int.com/transactionService",
                emulatorBaseUrl: "http://localhost/PXDependencyEmulators", // lgtm[cs/non-https-url] Suppressing Semmle warning // DevSkim: ignore DS137138 as this to access the locally hosted endpoint
                apiVersion: "2018-05-07",
                messageHandler: transactionServiceWebRequestHandler);

            var sellerMarketPlaceServiceWebRequestHandler = new HttpClientHandler();
            this.SellerMarketPlaceServiceAccessor = new SellerMarketPlaceServiceAccessor(
                serviceBaseUrl: "https://seller-marketplace-ppe.microsoft.com",
                emulatorBaseUrl: "http://localhost/PXDependencyEmulators",
                messageHandler: sellerMarketPlaceServiceWebRequestHandler);

            var thirdPartyMarketPlaceServiceWebRequestHandler = new HttpClientHandler();
            this.PaymentThirdPartyServiceAccessor = new PaymentThirdPartyServiceAccessor(
                serviceBaseUrl: "https://paymentthirdpartyservice-int-westus2.azurewebsites.net",
                emulatorBaseUrl: "http://localhost/PXDependencyEmulators",
                apiVersion: "2022-02-09",
                messageHandler: thirdPartyMarketPlaceServiceWebRequestHandler);
            
            var issuerServiceRequestHandler = new HttpClientHandler();
            this.IssuerServiceAccessor = new IssuerServiceAccessor(
                serviceBaseUrl: "https://issuerservice.cp.microsoft-int.com",
                emulatorBaseUrl: "http://localhost/PXDependencyEmulators",
                apiVersion: this.IssuerServiceApiVersion,
                messageHandler: issuerServiceRequestHandler);

            var shortUrlRequestHandler = new HttpClientHandler();
            this.ShortURLServiceAccessor = new ShortURLServiceAccessor(
                serviceBaseUrl: "https://rds-int.ms",
                messageHandler: shortUrlRequestHandler);

            var partnerSettingsRequestHandler = new HttpClientHandler();
            this.PartnerSettingsServiceAccessor = new PartnerSettingsServiceAccessor(
                serviceBaseUrl: "https://partnersettingsservice.cp.microsoft-int.com",
                servicePPEBaseUrl: null,
                emulatorBaseUrl: "http://localhost/PXDependencyEmulators",
                messageHandler: partnerSettingsRequestHandler);

            var walletWebRequestHandler = new HttpClientHandler();
            this.WalletServiceAccessor = new WalletServiceAccessor(
                serviceBaseUrl: "https://paymentswalletservice.cp.microsoft-int.com",
                apiVersion: "2023-1-1",
                messageHandler: walletWebRequestHandler);

            var transactionDataWebRequestHandler = new HttpClientHandler();
            this.TransactionDataServiceAccessor = new TransactionDataServiceAccessor(
                serviceBaseUrl: "https://transactiondataservice.cp.microsoft-int.com",
                apiVersion: "2018-05-07",
                messageHandler: transactionDataWebRequestHandler);

            var challengeManagementServiceRequestHandler = new HttpClientHandler();
            this.ChallengeManagementServiceAccessor = new ChallengeManagementServiceAccessor(
                serviceBaseUrl: "https://challengemanager-ppe-fjaxb6erbcf2fbej.z01.azurefd.net",
                emulatorBaseUrl: "http://localhost/PXDependencyEmulators",
                messageHandler: challengeManagementServiceRequestHandler);

            var networkTokenizationServiceRequestHandler = new HttpClientHandler();
            this.NetworkTokenizationServiceAccessor = new NetworkTokenizationServiceAccessor(
                serviceBaseUrl: "https://nts.cp.microsoft-int.com",
                emulatorBaseUrl: string.Empty,
                apiVersion: "1.0",
                messageHandler: networkTokenizationServiceRequestHandler);
                
            var tokenizationServiceRequestHandler = new HttpClientHandler();
            this.TokenizationServiceAccessor = new TokenizationServiceAccessor(
                serviceBaseUrl: "https://tokenization.cp.microsoft-int.com",
                emulatorBaseUrl: "TBD",
                tokenizationGetTokenURL: "https://tokenization.cp.microsoft-int.com/tokens",
                tokenizationGetTokenFromEncryptedValueURL: "https://tokenizationfd.cp.microsoft-int.com/tokens",
                messageHandler: tokenizationServiceRequestHandler);

            var paymentOrchestratorServiceWebRequestHandler = new HttpClientHandler();
            this.PaymentOrchestratorServiceAccessor = new PaymentOrchestratorServiceAccessor(
                serviceBaseUrl: "https://paymentorchestratorservice.cp.microsoft-int.com",
                emulatorBaseUrl: "http://localhost/PXDependencyEmulators",
                apiVersion: "1.1",
                messageHandler: paymentOrchestratorServiceWebRequestHandler);

            var fraudDetectionMessageHandler = new HttpClientHandler();
            this.FraudDetectionServiceAccessor = new FraudDetectionServiceAccessor(
                serviceBaseUrl: "https://paymentsfrauddetectionservice.cp.microsoft-int.com",
                emulatorBaseUrl: "http://localhost/PXDependencyEmulators",
                messageHandler: fraudDetectionMessageHandler);

            ResourceLifecycleStateManager.Initialize(
                this,
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceLifecycleStateManager.ResourceLifecycleConstants.ErrorConfigFilePath),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceLifecycleStateManager.ResourceLifecycleConstants.ClientActionConfigFilePath));

            this.PifdBaseUrl = "https://pifd.cp.microsoft-int.com/V6.0";

            this.PayMicrosoftBaseUrl = "https://payint.microsoft.com";

            this.StaticResourceServiceBaseUrl = "https://staticresources.payments.microsoft-int.com/staticresourceservice";

            ////  Move path outside of the app folder to void recycle.
            DirectoryInfo directoryInfo = new DirectoryInfo(System.Environment.CurrentDirectory);
            this.SllEnvironmentSetting = new Common.Tracing.SllEnvironmentSetting
            {
                SllLogPath = directoryInfo.Parent == null ? directoryInfo.FullName : directoryInfo.Parent.FullName,
                SllLogNamePrefix = "PXServiceSll",
                SllMaxFileSizeBytes = 52428800L,
                SllMaxFileCount = 64,
            };

            this.InitializeAuthenticationSettings();
        }

        private void InitializeAuthenticationSettings()
        {
            IList<UserInformation> partnerInformationList = new List<UserInformation>()
            {
                new UserInformation()
                {
                    Role = GlobalConstants.ClientRoles.Test,
                    AllowedAccounts = "18c69db8-dd2e-48a0-8887-1ccabd0bbcb2,3cfa0e51-97ae-49a8-9a71-398ca2ba0683,62dc8681-6753-484a-981a-128f82a43d25,7e5242d0-33ea-4bd1-a691-5193af93c4c7,ec8c235c-65e2-4a3d-bd7d-a20ed8ec1688",
                    AllowedAuthenticatedPathTemplate = "/pxservice/v7.0/{0}",
                    AllowedUnAuthenticatedPaths = "/v7.0/settings/Microsoft.Payments.Client,/v7.0/addresses/legacyValidate,/v7.0/addresses/modernValidate,/v7.0/paymentSessions",
                    PartnerName = Partner.Name.PXCOT.ToString(),
                    ApplicationId = "7033f9b1-b4e6-4d49-9bde-738d53c14ae9" // Application name: PX-COT-INT-PME 
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
                UberUserDirectory = uberUserDirectory,
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