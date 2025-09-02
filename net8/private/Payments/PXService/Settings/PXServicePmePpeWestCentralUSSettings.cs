// <copyright file="PXServicePmePpeWestCentralUSSettings.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Settings
{
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using Microsoft.Commerce.Payments.PXService.Accessors.ChallengeManagementService;
    using Microsoft.Commerce.Payments.PXService.Accessors.D365Service;
    using Microsoft.Commerce.Payments.PXService.Accessors.FraudDetectionService;
    using Microsoft.Commerce.Payments.PXService.Accessors.IssuerService;
    using Microsoft.Commerce.Payments.PXService.Accessors.MSRewardsService;
    using Microsoft.Commerce.Payments.PXService.Accessors.PartnerSettingsService;
    using Microsoft.Commerce.Payments.PXService.Accessors.SellerMarketPlaceService;
    using Microsoft.Commerce.Payments.PXService.Accessors.TokenizationService;
    using Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService;
    using Microsoft.Commerce.Payments.PXService.RiskService.V7;

    public class PXServicePmePpeWestCentralUSSettings : PXServiceProdSettings
    {
        [SuppressMessage("Microsoft.Maintainability", "CA1506: Avoid excessive class coupling", Justification = "Central class initialization method.")]
        public PXServicePmePpeWestCentralUSSettings() : base()
        {
            this.PifdBaseUrl = "https://paymentinstruments-int.mp.microsoft.com/V6.0";

            this.PayMicrosoftBaseUrl = "https://payppe.microsoft.com";

            var azureExpMessageHandler = PXServiceSettings.GetAADRequestHandler(PXCommon.Constants.ServiceNames.AzureExPService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.AzureExPAccessor = new AzureExPAccessor(
                expBlobUrl: "https://default.exp-tas.com/exptas49/03947902-89f9-4d38-972c-251138ba5b61-paymentexpprd/api/v1/experimentationblob",
                tokenLoader: this.AzureActiveDirectoryTokenLoaderFactory.GetActiveDirectoryTokenLoader(PXCommon.Constants.ServiceNames.AzureExPService),
                messageHandler: azureExpMessageHandler);

            this.PIMSAccessor = new PIMSAccessor(
                serviceBaseUrl: this.PimsBaseUrl,
                emulatorBaseUrl: "https://px-pxdependencyemulators-ppe-westcentralus.azurewebsites.net",
                servicePPEBaseUrl: "https://app-pims-pims-prod-eus2-1.azurewebsites.net/InstrumentManagementService",
                apiVersion: this.PimsApiVersion,
                messageHandler: this.PimsRequestHandler);

            this.AccountServiceAccessor = new AccountServiceAccessor(
                serviceBaseUrl: this.AccountServiceBaseUrl,
                emulatorBaseUrl: "https://px-pxdependencyemulators-ppe-westcentralus.azurewebsites.net",
                messageHandler: this.AccountServiceRequestHandler);

            this.PayerAuthServiceAccessor = new PayerAuthServiceAccessor(
                serviceBaseUrl: this.PayerAuthServiceBaseUrl,
                emulatorBaseUrl: "https://px-pxdependencyemulators-ppe-westcentralus.azurewebsites.net",
                apiVersion: this.PayerAuthServiceApiVersion,
                messageHandler: this.PayerAuthServiceRequestHandler);

            this.PurchaseServiceAccessor = new PurchaseServiceAccessor(
                serviceBaseUrl: this.PurchaseServiceBaseUrl,
                emulatorBaseUrl: "https://px-pxdependencyemulators-ppe-westcentralus.azurewebsites.net",
                apiVersion: this.PurchaseServiceApiVersion,
                messageHandler: this.PurchaseServiceRequestHandler);

            // D365 Service uses different base url for Ppe and Prod environment.
            this.D365ServiceAccessor = new D365ServiceAccessor(
                serviceBaseUrl: "https://orders.ppe.store-web.dynamics.com",
                "tbd",
                apiVersion: this.D365ServiceApiVersion,
                this.D365ServiceRequestHandler);

            this.CatalogServiceAccessor = new CatalogServiceAccessor(
                serviceBaseUrl: "https://px-pxdependencyemulators-ppe-westcentralus.azurewebsites.net",
                emulatorBaseUrl: "https://px-pxdependencyemulators-ppe-westcentralus.azurewebsites.net",
                apiVersion: this.CatalogServiceApiVersion,
                messageHandler: this.CatalogServiceRequestHandler);

            this.TokenPolicyServiceAccessor = new TokenPolicyServiceAccessor(
                serviceBaseUrl: this.TokenPolicyServiceBaseUrl,
                emulatorBaseUrl: "https://px-pxdependencyemulators-ppe-westcentralus.azurewebsites.net",
                messageHandler: this.TokenPolicyServiceRequestHandler);

            this.StoredValueServiceAccessor = new StoredValueAccessor(
                serviceBaseUrl: this.StoredValueServiceBaseUrl,
                emulatorBaseUrl: "https://px-pxdependencyemulators-ppe-westcentralus.azurewebsites.net",
                apiVersion: this.StoredValueServiceApiVersion,
                messageHandler: this.StoredValueServiceRequestHandler);

            this.TransactionServiceAccessor = new TransactionServiceAccessor(
                serviceBaseUrl: "https://paymentstransactionservice.cp.microsoft.com/transactionService",
                emulatorBaseUrl: "https://px-pxdependencyemulators-ppe-westcentralus.azurewebsites.net",
                apiVersion: "2018-05-07",
                messageHandler: this.TransactionServiceRequestHandler);

            this.SellerMarketPlaceServiceAccessor = new SellerMarketPlaceServiceAccessor(
                serviceBaseUrl: "https://seller-marketplace-prod.microsoft.com",
                emulatorBaseUrl: "https://px-pxdependencyemulators-ppe-westcentralus.azurewebsites.net",
                messageHandler: this.SellerMarketPlaceServiceWebRequestHandler);

            this.PaymentThirdPartyServiceAccessor = new PaymentThirdPartyServiceAccessor(
                serviceBaseUrl: "https://paymentthirdpartyservice.cp.microsoft.com",
                emulatorBaseUrl: "https://px-pxdependencyemulators-ppe-westcentralus.azurewebsites.net",
                apiVersion: "2022-02-09",
                messageHandler: this.ThirdPartyMarketPlaceServiceWebRequestHandler);

            this.MSRewardsServiceAccessor = new MSRewardsServiceAccessor(
                serviceBaseUrl: "https://prod.rewardsplatform.microsoft.com",
                emulatorBaseUrl: "https://px-pxdependencyemulators-ppe-westcentralus.azurewebsites.net",
                messageHandler: this.MSRewardsRequestHandler);

            this.PartnerSettingsServiceAccessor = new PartnerSettingsServiceAccessor(
                serviceBaseUrl: "https://partnersettingsservice.cp.microsoft.com",
                servicePPEBaseUrl: "https://partnersettings-ppe-westus.azurewebsites.net",
                emulatorBaseUrl: "https://px-pxdependencyemulators-ppe-westcentralus.azurewebsites.net",
                messageHandler: this.PartnerSettingsServiceWebRequestHandler);

            this.IssuerServiceAccessor = new IssuerServiceAccessor(
                serviceBaseUrl: "https://issuerservice.cp.microsoft.com",
                emulatorBaseUrl: "https://px-pxdependencyemulators-ppe-westcentralus.azurewebsites.net",
                apiVersion: this.IssuerServiceApiVersion,
                messageHandler: this.IssuerServiceWebRequestHandler);

            this.WalletServiceAccessor = new WalletServiceAccessor(
                serviceBaseUrl: "https://paymentswalletservice.cp.microsoft.com",
                apiVersion: "2023-1-1",
                messageHandler: this.WalletServiceWebRequestHandler);

            this.ChallengeManagementServiceAccessor = new ChallengeManagementServiceAccessor(
                serviceBaseUrl: "https://challengemanager-ppe-fjaxb6erbcf2fbej.z01.azurefd.net",
                emulatorBaseUrl: "https://px-pxdependencyemulators-ppe-westcentralus.azurewebsites.net",
                messageHandler: this.ChallengeManagementServiceRequestHandler);

            this.PXSessionTokenIssuer = "https://paymentexperience.cp.microsoft-int.com/px/";
            this.PXSessionTokenValidityPeriod = 5;

            this.PIDLDocumentValidationEnabled = true;

            this.RiskServiceAccessor = new RiskServiceAccessor(
                serviceBaseUrl: "https://ks.cp.microsoft.com",
                emulatorBaseUrl: "https://px-pxdependencyemulators-ppe-westcentralus.azurewebsites.net",
                apiVersion: "2015-02-28",
                messageHandler: this.RiskServiceRequestHandler);

            this.TokenizationServiceAccessor = new TokenizationServiceAccessor(
                serviceBaseUrl: "https://tokenization.cp.microsoft.com",
                emulatorBaseUrl: "TBD",
                tokenizationGetTokenURL: "https://tokenization.cp.microsoft.com/tokens",
                tokenizationGetTokenFromEncryptedValueURL: "https://tokenizationfd.cp.microsoft.com/tokens",
                messageHandler: new HttpClientHandler());

            var fraudDetectionMessageHandler = GetAADRequestHandler(PXCommon.Constants.ServiceNames.FraudDetectionService, this.AzureActiveDirectoryTokenLoaderFactory);
            this.FraudDetectionServiceAccessor = new FraudDetectionServiceAccessor(
                serviceBaseUrl: "https://paymentsfrauddetectionservice.cp.microsoft.com",
                emulatorBaseUrl: "https://px-pxdependencyemulators-ppe-westcentralus.azurewebsites.net",
                messageHandler: fraudDetectionMessageHandler);
        }
    }
}