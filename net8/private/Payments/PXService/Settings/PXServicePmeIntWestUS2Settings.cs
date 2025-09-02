// <copyright file="PXServicePmeIntWestUS2Settings.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Settings
{
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using Microsoft.Commerce.Payments.PXService.Accessors.ChallengeManagementService;
    using Microsoft.Commerce.Payments.PXService.Accessors.FraudDetectionService;
    using Microsoft.Commerce.Payments.PXService.Accessors.IssuerService;
    using Microsoft.Commerce.Payments.PXService.Accessors.MSRewardsService;
    using Microsoft.Commerce.Payments.PXService.Accessors.PartnerSettingsService;
    using Microsoft.Commerce.Payments.PXService.Accessors.PaymentOrchestratorService;
    using Microsoft.Commerce.Payments.PXService.Accessors.SellerMarketPlaceService;
    using Microsoft.Commerce.Payments.PXService.Accessors.TokenizationService;
    using Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService;
    using Microsoft.Commerce.Payments.PXService.RiskService.V7;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Don't need the excessive class coupling check here")]
    public class PXServicePmeIntWestUS2Settings : PXServiceIntSettings
    {
        [SuppressMessage("Microsoft.Performance", "CA1801", Justification = "EnvironmentName is needed to adhere to the convention.")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506: Avoid excessive class coupling", Justification = "Central class initialization method.")]
        public PXServicePmeIntWestUS2Settings() : base()
        {
            var pimsWebRequestHandler = GetAADRequestHandler(PXCommon.Constants.ServiceNames.InstrumentManagementService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.PIMSAccessor = new PIMSAccessor(
                serviceBaseUrl: "https://paymentsinstrumentservice.cp.microsoft-int.com/InstrumentManagementService",
                emulatorBaseUrl: "https://px-pxdependencyemulators-int-westus2.azurewebsites.net/",
                servicePPEBaseUrl: null,
                apiVersion: "2014-09-30",
                messageHandler: pimsWebRequestHandler);
            this.PIMSAccessorFlightEnabled = true;
            
            var accountServiceRequestHandler = GetAADRequestHandler(PXCommon.Constants.ServiceNames.AccountService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.AccountServiceAccessor = new AccountServiceAccessor(
                serviceBaseUrl: "https://accounts.cp.microsoft-int.com",
                emulatorBaseUrl: "https://px-pxdependencyemulators-int-westus2.azurewebsites.net",
                messageHandler: accountServiceRequestHandler);

            var payAuthWebRequestHandler = GetAADRequestHandler(PXCommon.Constants.ServiceNames.PayerAuthService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.PayerAuthServiceAccessor = new PayerAuthServiceAccessor(
                serviceBaseUrl: "https://payerauthservice.cp.microsoft-int.com/PayerAuthService",
                emulatorBaseUrl: "https://px-pxdependencyemulators-int-westus2.azurewebsites.net",
                apiVersion: GlobalConstants.PayerAuthApiVersions.V3,
                messageHandler: payAuthWebRequestHandler);

            var purchaseWebRequestHandler = GetAADRequestHandler(PXCommon.Constants.ServiceNames.PurchaseService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.PurchaseServiceAccessor = new PurchaseServiceAccessor(
                serviceBaseUrl: "https://px-pxdependencyemulators-int-westus2.azurewebsites.net",
                emulatorBaseUrl: "https://px-pxdependencyemulators-int-westus2.azurewebsites.net",
                apiVersion: GlobalConstants.PurchaseApiVersions.V7,
                messageHandler: purchaseWebRequestHandler);

            var catalogWebRequestHandler = GetAADRequestHandler(PXCommon.Constants.ServiceNames.CatalogService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.CatalogServiceAccessor = new CatalogServiceAccessor(
                serviceBaseUrl: "https://px-pxdependencyemulators-int-westus2.azurewebsites.net",
                emulatorBaseUrl: "https://px-pxdependencyemulators-int-westus2.azurewebsites.net",
                apiVersion: GlobalConstants.CatalogApiVersions.V8,
                messageHandler: catalogWebRequestHandler);

            var tokenPolicyRequestHandler = GetAADRequestHandler(PXCommon.Constants.ServiceNames.TokenPolicyService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.TokenPolicyServiceAccessor = new TokenPolicyServiceAccessor(
                serviceBaseUrl: "https://tops-int.mp.microsoft.com",
                emulatorBaseUrl: "https://px-pxdependencyemulators-int-westus2.azurewebsites.net",
                messageHandler: tokenPolicyRequestHandler);

            var storedValueWebRequestHandler = GetAADRequestHandler(PXCommon.Constants.ServiceNames.StoredValueService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.StoredValueServiceAccessor = new StoredValueAccessor(
                apiVersion: "2014-10-10",
                serviceBaseUrl: "https://storedvalue.cp.microsoft-int.com",
                emulatorBaseUrl: "https://px-pxdependencyemulators-int-westus2.azurewebsites.net",
                messageHandler: storedValueWebRequestHandler);

            var paymentOrchestratorMessageHandler = GetAADRequestHandler(PXCommon.Constants.ServiceNames.PaymentOrchestratorService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.PaymentOrchestratorServiceAccessor = new PaymentOrchestratorServiceAccessor(
                serviceBaseUrl: "https://paymentorchestratorservice.cp.microsoft-int.com",
                emulatorBaseUrl: "https://px-pxdependencyemulators-int-westus2.azurewebsites.net",
                apiVersion: "1.1",
                messageHandler: paymentOrchestratorMessageHandler);

            var transactionServiceWebRequestHandler = GetAADRequestHandler(PXCommon.Constants.ServiceNames.TransactionService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.TransactionServiceAccessor = new TransactionServiceAccessor(
                serviceBaseUrl: "https://paymentstransactionservice.cp.microsoft-int.com/transactionService",
                emulatorBaseUrl: "https://px-pxdependencyemulators-int-westus2.azurewebsites.net",
                apiVersion: "2018-05-07",
                messageHandler: transactionServiceWebRequestHandler);

            var sellerMarketPlaceServiceWebRequestHandler = GetAADRequestHandler(PXCommon.Constants.ServiceNames.SellerMarketPlaceService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.SellerMarketPlaceServiceAccessor = new SellerMarketPlaceServiceAccessor(
                serviceBaseUrl: "https://seller-marketplace-ppe.microsoft.com",
                emulatorBaseUrl: "https://px-pxdependencyemulators-int-westus2.azurewebsites.net",
                messageHandler: sellerMarketPlaceServiceWebRequestHandler);

            var thirdPartyMarketPlaceServiceWebRequestHandler = GetAADRequestHandler(PXCommon.Constants.ServiceNames.PaymentThirdPartyService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.PaymentThirdPartyServiceAccessor = new PaymentThirdPartyServiceAccessor(
                serviceBaseUrl: "https://paymentthirdpartyservice-int-westus2.azurewebsites.net",
                emulatorBaseUrl: "https://px-pxdependencyemulators-int-westus2.azurewebsites.net",
                apiVersion: "2022-02-09",
                messageHandler: thirdPartyMarketPlaceServiceWebRequestHandler);

            var msRewardsRequestHandler = new HttpClientHandler();
            this.MSRewardsServiceAccessor = new MSRewardsServiceAccessor(
                serviceBaseUrl: "https://int.rewardsplatform.microsoft.com",
                emulatorBaseUrl: "https://px-pxdependencyemulators-int-westus2.azurewebsites.net",
                messageHandler: msRewardsRequestHandler);

            var partnerSettingsServiceWebRequestHandler = GetAADRequestHandler(PXCommon.Constants.ServiceNames.PartnerSettingsService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.PartnerSettingsServiceAccessor = new PartnerSettingsServiceAccessor(
                serviceBaseUrl: "https://partnersettingsservice.cp.microsoft-int.com",
                servicePPEBaseUrl: null,
                emulatorBaseUrl: "https://px-pxdependencyemulators-int-westus2.azurewebsites.net",
                messageHandler: partnerSettingsServiceWebRequestHandler);

            var issuerServiceRequestHandler = GetAADRequestHandler(PXCommon.Constants.ServiceNames.IssuerService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.IssuerServiceAccessor = new IssuerServiceAccessor(
                serviceBaseUrl: "https://issuerservice.cp.microsoft-int.com",
                emulatorBaseUrl: "https://px-pxdependencyemulators-int-westus2.azurewebsites.net",
                apiVersion: this.IssuerServiceApiVersion,
                messageHandler: issuerServiceRequestHandler);

            var walletWebRequestHandler = GetAADRequestHandler(PXCommon.Constants.ServiceNames.WalletService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.WalletServiceAccessor = new WalletServiceAccessor(
                serviceBaseUrl: "https://paymentswalletservice.cp.microsoft-int.com",
                apiVersion: "2023-1-1",
                messageHandler: walletWebRequestHandler);

            var challengeManagementServiceRequestHandler = GetAADRequestHandler(PXCommon.Constants.ServiceNames.ChallengeManagementService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.ChallengeManagementServiceAccessor = new ChallengeManagementServiceAccessor(
                serviceBaseUrl: "https://challengemanager-ppe-fjaxb6erbcf2fbej.z01.azurefd.net",
                emulatorBaseUrl: "https://px-pxdependencyemulators-int-westus2.azurewebsites.net",
                messageHandler: challengeManagementServiceRequestHandler);

            this.RiskServiceAccessor = new RiskServiceAccessor(
                serviceBaseUrl: "https://ks.cp.microsoft-int.com",
                emulatorBaseUrl: "https://px-pxdependencyemulators-int-westus2.azurewebsites.net",
                apiVersion: "2015-02-28",
                messageHandler: this.RiskServiceRequestHandler);

            this.TokenizationServiceAccessor = new TokenizationServiceAccessor(
                serviceBaseUrl: "https://tokenization.cp.microsoft-int.com",
                emulatorBaseUrl: "TBD",
                tokenizationGetTokenURL: "https://tokenization.cp.microsoft-int.com/tokens",
                tokenizationGetTokenFromEncryptedValueURL: "https://tokenizationfd.cp.microsoft-int.com/tokens",
                messageHandler: new HttpClientHandler());

            var fraudDetectionMessageHandler = GetAADRequestHandler(PXCommon.Constants.ServiceNames.FraudDetectionService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.FraudDetectionServiceAccessor = new FraudDetectionServiceAccessor(
                serviceBaseUrl: "https://paymentsfrauddetectionservice.cp.microsoft-int.com",
                emulatorBaseUrl: "https://px-pxdependencyemulators-int-westus2.azurewebsites.net",
                messageHandler: fraudDetectionMessageHandler);
        }
    }
}