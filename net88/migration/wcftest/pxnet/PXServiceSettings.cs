// <copyright file="PXServiceSettings.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using Accessors.OrchestrationService;
    using AddressEnrichmentService.V7;
    using Common.Environments;
    using Common.Tracing;
    using MerchantCapabilitiesService.V7;
    using Microsoft.Commerce.Payments.Authentication.AzureActiveDirectoryCaller;
    using Microsoft.Commerce.Payments.Authentication.AzureActiveDirectoryCaller.Settings;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXService.Accessors.ChallengeManagementService;
    using Microsoft.Commerce.Payments.PXService.Accessors.D365Service;
    using Microsoft.Commerce.Payments.PXService.Accessors.FraudDetectionService;
    using Microsoft.Commerce.Payments.PXService.Accessors.MSRewardsService;
    using Microsoft.Commerce.Payments.PXService.Accessors.NetworkTokenizationService;
    using Microsoft.Commerce.Payments.PXService.Accessors.PartnerSettingsService;
    using Microsoft.Commerce.Payments.PXService.Accessors.PaymentOrchestratorService;
    using Microsoft.Commerce.Payments.PXService.Accessors.SellerMarketPlaceService;
    using Microsoft.Commerce.Payments.PXService.Accessors.ShortURLService;
    using Microsoft.Commerce.Payments.PXService.Accessors.TokenizationService;
    using Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService;
    using Microsoft.Commerce.Payments.PXService.Accessors.TransactionDataService;
    using Microsoft.Commerce.Payments.PXService.Settings.FeatureConfig;
    using Microsoft.Identity.Web;
    using Newtonsoft.Json;
    using RiskService.V7;

    public abstract class PXServiceSettings
    {
        public bool AzureExpEnabled { get; protected set; } = true;

        public bool ValidateCors { get; protected set; } = true;

        public List<string> CorsAllowedOrigins { get; private set; }

        public string PifdBaseUrl { get; protected set; }

        public string PayMicrosoftBaseUrl { get; protected set; }

        public string StaticResourceServiceBaseUrl { get; protected set; }

        public IPIMSAccessor PIMSAccessor { get; protected set; }

        public IOrchestrationServiceAccessor OrchestrationServiceAccessor { get; protected set; }

        public IAccountServiceAccessor AccountServiceAccessor { get; protected set; }

        public IRDSServiceAccessor RDSServiceAccessor { get; protected set; }

        public IPaymentOrchestratorServiceAccessor PaymentOrchestratorServiceAccessor { get; protected set; }

        public X509Certificate2 AadCertificate { get; protected set; }

        public X509Certificate2 CtpCertificate { get; protected set; }

        public string AadCertificateName { get; protected set; }

        public string CtpCertificateName { get; protected set; }

        public string AadCertificateSubjectName { get; protected set; }

        public string CtpCertificateSubjectName { get; protected set; }

        public ITaxIdServiceAccessor TaxIdServiceAccessor { get; protected set; }

        public IFraudDetectionServiceAccessor FraudDetectionServiceAccessor { get; protected set; }

        public X509Certificate2 SessionServiceClientCertificate { get; protected set; }

        public ISessionServiceAccessor SessionServiceAccessor { get; protected set; }

        public PXServiceAuthorizationFilterAttribute AuthorizationFilter { get; protected set; }

        public SllEnvironmentSetting SllEnvironmentSetting { get; protected set; }

        public JsonSerializerSettings JsonSerializerSettings { get; protected set; }

        public IRiskServiceAccessor RiskServiceAccessor { get; protected set; }

        public IMSRewardsServiceAccessor MSRewardsServiceAccessor { get; protected set; }

        public bool PIMSAccessorFlightEnabled { get; protected set; }

        public string MerchantCapabilitiesUri { get; protected set; }

        public string MerchantCapabilitiesApiVersion { get; protected set; }

        public IMerchantCapabilitiesAccessor MerchantCapabilitiesAccessor { get; protected set; }

        public IPayerAuthServiceAccessor PayerAuthServiceAccessor { get; protected set; }

        public IPurchaseServiceAccessor PurchaseServiceAccessor { get; protected set; }

        public ITokenPolicyServiceAccessor TokenPolicyServiceAccessor { get; protected set; }

        public ID365ServiceAccessor D365ServiceAccessor { get; protected set; }

        public ICatalogServiceAccessor CatalogServiceAccessor { get; protected set; }

        public IStoredValueAccessor StoredValueServiceAccessor { get; protected set; }

        public string PayerAuthServiceApiVersion { get; protected set; }

        public string PurchaseServiceApiVersion { get; protected set; }

        public string D365ServiceApiVersion { get; protected set; }

        public string CatalogServiceApiVersion { get; protected set; }

        public string StoredValueServiceApiVersion { get; protected set; }

        public string IssuerServiceApiVersion { get; protected set; }

        public IAddressEnrichmentServiceAccessor AddressEnrichmentServiceAccessor { get; protected set; }

        public ITransactionServiceAccessor TransactionServiceAccessor { get; protected set; }

        public IShortURLServiceAccessor ShortURLServiceAccessor { get; protected set; }

        public IPaymentThirdPartyServiceAccessor PaymentThirdPartyServiceAccessor { get; protected set; }

        public ISellerMarketPlaceServiceAccessor SellerMarketPlaceServiceAccessor { get; protected set; }

        public IPartnerSettingsServiceAccessor PartnerSettingsServiceAccessor { get; protected set; }

        public IIssuerServiceAccessor IssuerServiceAccessor { get; protected set; }

        public IAzureExPAccessor AzureExPAccessor { get; protected set; }

        public IAnomalyDetectionAccessor AnomalyDetectionAccessor { get; protected set; }

        public IWalletServiceAccessor WalletServiceAccessor { get; protected set; }

        public ITransactionDataServiceAccessor TransactionDataServiceAccessor { get; protected set; }

        public IChallengeManagementServiceAccessor ChallengeManagementServiceAccessor { get; protected set; }

        public INetworkTokenizationServiceAccessor NetworkTokenizationServiceAccessor { get; protected set; }
        
        public ITokenizationServiceAccessor TokenizationServiceAccessor { get; protected set; }

        public string ManagedIdentityId { get; protected set; }

        public AzureActiveDirectoryTokenLoaderFactory AzureActiveDirectoryTokenLoaderFactory { get; protected set; }

        public string PXSessionTokenIssuer { get; set; }

        public int PXSessionTokenValidityPeriod { get; set; }

        public string ApplicationInsightInstrumentKey { get; set; }

        public bool ApplicationInsightMode { get; set; }

        public LocalFeatureConfigs LocalFeatureConfigs { get; set; }

        public bool PIDLDocumentValidationEnabled { get; protected set; } = false;

        public static PXServiceSettings CreateInstance(EnvironmentType environmentType, string environmentName)
        {
            switch (environmentType)
            {
                case EnvironmentType.Production:
                    if (string.Equals(environmentName, EnvironmentNames.Production.PXPMEPRODWestUS, StringComparison.OrdinalIgnoreCase))
                    {
                        return new PXServicePmeProdWestUSSetting();
                    }
                    else if (string.Equals(environmentName, EnvironmentNames.Production.PXPMEPRODEastUS, StringComparison.OrdinalIgnoreCase))
                    {
                        return new PXServicePmeProdEastUSSetting();
                    }
                    else if (string.Equals(environmentName, EnvironmentNames.Production.PXPMEPRODCentralUS, StringComparison.OrdinalIgnoreCase))
                    {
                        return new PXServicePmeProdCentralUSSetting();
                    }
                    else if (string.Equals(environmentName, EnvironmentNames.Production.PXPMEPRODWestUS2, StringComparison.OrdinalIgnoreCase))
                    {
                        return new PXServicePmeProdWestUS2Setting();
                    }
                    else if (string.Equals(environmentName, EnvironmentNames.Production.PXPMEPRODSouthCentralUS, StringComparison.OrdinalIgnoreCase))
                    {
                        return new PXServicePmeProdSouthCentralUSSetting();
                    }
                    else if (string.Equals(environmentName, EnvironmentNames.Production.PXPMEPRODWestCentralUS, StringComparison.OrdinalIgnoreCase))
                    {
                        return new PXServicePmeProdWestCentralUsSetting();
                    }
                    else if (string.Equals(environmentName, EnvironmentNames.Production.PXPMEPRODEastUS2, StringComparison.OrdinalIgnoreCase))
                    {
                        return new PXServicePmeProdEastUS2Setting();
                    }
                    else if (string.Equals(environmentName, EnvironmentNames.Production.PXPMEPRODNorthCentralUS, StringComparison.OrdinalIgnoreCase))
                    {
                        return new PXServicePmeProdNorthCentralUSSetting();
                    }
                    else
                    {
                        throw TraceCore.TraceException<NotSupportedException>(new NotSupportedException(string.Format(
                        CultureInfo.InvariantCulture,
                        "Environment {0} does not have a corresponding EnvironmentDefinition defined",
                        environmentType)));
                    }

                case EnvironmentType.PPE:
                    if (string.Equals(environmentName, EnvironmentNames.PPE.PXPMEPPEEastUS, StringComparison.OrdinalIgnoreCase))
                    {
                        return new PXServicePmePpeEastUSSettings();
                    }
                    else if (string.Equals(environmentName, EnvironmentNames.PPE.PXPMEPPEWestUS, StringComparison.OrdinalIgnoreCase))
                    {
                        return new PXServicePmePpeWestUSSettings();
                    }
                    else if (string.Equals(environmentName, EnvironmentNames.PPE.PXPMEPPEEastUS2, StringComparison.OrdinalIgnoreCase))
                    {
                        return new PXServicePmePpeEastUS2Settings();
                    }
                    else if (string.Equals(environmentName, EnvironmentNames.PPE.PXPMEPPENorthCentralUS, StringComparison.OrdinalIgnoreCase))
                    {
                        return new PXServicePmePpeNorthCentralUSSettings();
                    }
                    else if (string.Equals(environmentName, EnvironmentNames.PPE.PXPMEPPEWestCentralUS, StringComparison.OrdinalIgnoreCase))
                    {
                        return new PXServicePmePpeWestCentralUSSettings();
                    }
                    else
                    {
                        throw TraceCore.TraceException<NotSupportedException>(new NotSupportedException(string.Format(
                        CultureInfo.InvariantCulture,
                        "Environment {0} does not have a corresponding EnvironmentDefinition defined",
                        environmentType)));
                    }

                case EnvironmentType.Integration:
                    if (string.Equals(environmentName, EnvironmentNames.Integration.PXPMEIntWestUS, StringComparison.OrdinalIgnoreCase))
                    {
                        SllWebLogger.TracePXServiceException(string.Format("At {0}, Start creating PXServicePmeIntWestUSSettings", DateTime.UtcNow.ToString("s")), new EventTraceActivity());
                        return new PXServicePmeIntWestUSSettings();
                    }
                    else if (string.Equals(environmentName, EnvironmentNames.Integration.PXPMEIntWestUS2, StringComparison.OrdinalIgnoreCase))
                    {
                        SllWebLogger.TracePXServiceException(string.Format("At {0}, Start creating PXServicePmeIntWestUS2Settings", DateTime.UtcNow.ToString("s")), new EventTraceActivity());

                        // remove enviroment below
                        return new PXServicePmeIntWestUS2Settings();
                    }
                    else
                    {
                        throw TraceCore.TraceException<NotSupportedException>(new NotSupportedException(string.Format(
                           CultureInfo.InvariantCulture,
                           "Environment {0} does not have a corresponding EnvironmentDefinition defined",
                           environmentType)));
                    }

                case EnvironmentType.Aircapi:
                    if (string.Equals(environmentName, EnvironmentNames.AirCapi.PXAirCapi1, StringComparison.OrdinalIgnoreCase))
                    {
                        SllWebLogger.TracePXServiceException(string.Format("At {0}, Start creating PXServiceAircapiSettings", DateTime.UtcNow.ToString("s")), new EventTraceActivity());

                        return new PXServiceAircapiSettings();
                    }
                    else
                    {
                        throw TraceCore.TraceException<NotSupportedException>(new NotSupportedException(string.Format(
                           CultureInfo.InvariantCulture,
                           "Environment {0} does not have a corresponding EnvironmentDefinition defined",
                           environmentType)));
                    }

                case EnvironmentType.OneBox:
                    return new PXServiceOneBoxSettings();

                default:
                    throw TraceCore.TraceException<NotSupportedException>(new NotSupportedException(string.Format(
                        CultureInfo.InvariantCulture,
                        "Environment {0} does not have a corresponding EnvironmentDefinition defined",
                        environmentType)));
            }
        }

        protected static X509Certificate2 LoadCertificateFromAKV(string akvUrl, string certificateName, ICertificateLoader certificateLoader)
        {
            CertificateDescription certificateDescription = CertificateDescription.FromKeyVault(akvUrl, certificateName);
            certificateDescription.X509KeyStorageFlags = X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.UserKeySet;
            certificateLoader.LoadIfNeeded(certificateDescription);
            SllWebLogger.TracePXServiceException(string.Format("At {0}, loading certificate {1} from AKV, subject is {2}, issuer is {3}, the thumbprint is {4}", DateTime.UtcNow.ToString("s"), certificateName, certificateDescription.Certificate.SubjectName.Name, certificateDescription.Certificate.IssuerName.Name, certificateDescription.Certificate.Thumbprint), new EventTraceActivity());
            return certificateDescription.Certificate;
        }

        protected static LocalFeatureConfigs FetchStaticFeatureConfigs(string featureconfigPath, string testAccountConfigPath, string testGroup)
        {
            var rawFeatureConfigs = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(
                File.ReadAllText(
                    Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        featureconfigPath)));

            var testAccounts = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(
                File.ReadAllText(
                    Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        testAccountConfigPath)));

            return new LocalFeatureConfigs(rawFeatureConfigs, testAccounts[testGroup]);
        }

        protected static AzureActiveDirectoryTokenLoaderOption BuildAADTokenLoaderOption(string serviceName, string clientId, string resource, bool isMI)
        {
            return new AzureActiveDirectoryTokenLoaderOption
                {
                    ServiceName = serviceName,
                    ClientId = clientId,
                    Resource = resource,
                    PreFetch = true,
                    AutoRefresh = true,
                    AutoRefreshInMin = 2,
                    MinimumRemainingValidityTimeInMin = 5,
                    LoaderType = isMI ? AadTokenLoaderType.ManagedIdentityTokenLoader : AadTokenLoaderType.MSALAppTokenLoader
                };
        }

        protected static AadAuthenticationHandler GetAADRequestHandler(string serviceName, AzureActiveDirectoryTokenLoaderFactory factory)
        {
            return new AadAuthenticationHandler(factory.GetActiveDirectoryTokenLoader(serviceName), new HttpClientHandler());
        }

        protected void AddCorsAllowedOrigin(string origin)
        {
            if (this.CorsAllowedOrigins == null)
            {
                this.CorsAllowedOrigins = new List<string>();
            }

            this.CorsAllowedOrigins.Add(origin);
        }

        protected void LoadCertificate(bool loadCertFromAKV, string akvUrl, string managedIdentityClientID)
        {
            if (loadCertFromAKV)
            {
                ICertificateLoader certificateLoader = new DefaultCertificateLoader();

                // Required to explicitly provide user-assigned MI ClientID to DefaultCertificateLoader as part of package upgrade to Microsoft.Identity.Web 1.25.3
                DefaultCertificateLoader.UserAssignedManagedIdentityClientId = managedIdentityClientID;

                this.AadCertificate = LoadCertificateFromAKV(akvUrl, this.AadCertificateName, certificateLoader);
                this.CtpCertificate = LoadCertificateFromAKV(akvUrl, this.CtpCertificateName, certificateLoader);
            }
            else
            {
                this.AadCertificate = new X509Certificate2(Common.Authorization.CertificateHelper.GetCertificateByName("My", this.AadCertificateSubjectName, true));
                this.CtpCertificate = new X509Certificate2(Common.Authorization.CertificateHelper.GetCertificateByName("My", this.CtpCertificateSubjectName, true));
            }
        }
    }
}