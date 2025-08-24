using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Commerce.Payments.PXService;
using Microsoft.Commerce.Payments.PXService.Accessors.IssuerService;
using Microsoft.Commerce.Payments.PXService.Accessors.MSRewardsService;
using Microsoft.Commerce.Payments.PXService.Accessors.PartnerSettingsService;
using Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService;
using Microsoft.Commerce.Payments.PXService.Model.PayerAuthService;
using Microsoft.Commerce.Payments.PXService.RiskService.V7;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocol.Handlers;
using SelfHostedPXServiceCore.Mocks;
using System;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Web.Services.Description;
using static QRCoder.PayloadGenerator;

namespace SelfHostedPXServiceCore
{
    /// <summary>
    /// Lightweight PX host that runs the service in memory using <see cref="TestServer"/>.
    /// </summary>
    public sealed class SelfHostedPxService : IDisposable
    {
        public static List<int> PreRegisteredPorts { get; private set; } = new();

        public static Uri PXBaseUri = new Uri("https://localhost:7151");

        public static HostableService PxHostableService { get; private set; }

        public static Dictionary<Type, HostableService> SelfHostedDependencies { get; private set; }

        public static PXServiceSettings PXSettings { get; private set; }

        public static PXServiceHandler? PXHandler { get; private set; }

        public static HttpClient PXClient { get; private set; }

        public static PXServiceCorsHandler PXCorsHandler { get; private set; }

        public static PXServiceFlightHandler PXFlightHandler { get; private set; }

        /// <summary>HttpClient wired to the in-memory PX service.</summary>
        public HttpClient HttpSelfHttpClient { get; private set; } = default!;

        public IHost SelfHost = default!;

        /// <summary>
        /// Spin up the PX service entirely in-memory. The returned client can be used to issue HTTP
        /// requests without opening any network sockets.
        /// </summary>
        public static SelfHostedPxService StartInMemory(string baseUrl, bool useSelfHostedDependencies, bool useArrangedResponses)
        {
            var selfHostedDependencies = new Dictionary<Type, HostableService>();

            if (useSelfHostedDependencies)
            {
                // Start up dependency emulators first so PX can connect to them.
                // in different host with available port.
                selfHostedDependencies = ConfigureDependencies(baseUrl);
            }

            if (string.IsNullOrEmpty(baseUrl))
            {
                var port = GetAvailablePort();
                var protocol = "https";
                baseUrl = string.Format("{0}://localhost:{1}", protocol, port);
                PXBaseUri = new Uri(baseUrl);
            }
            else
            {
                PXBaseUri = new Uri(baseUrl);
            }

            // Dependencies need to selfhost before px, we need to reserve px
            // port so other dependencies don't take it.
            PreRegisteredPorts.Add(PXBaseUri.Port);

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions());
            // builder.WebHost.UseTestServer();

            PXServiceSettings PXSettings = new Mocks.PXServiceSettings(
                useSelfHostedDependencies ? selfHostedDependencies : null,
                useArrangedResponses);

            WebApiConfig.Register(builder, PXSettings);

            var app = builder.Build();

            ConfigurePipeline(app);

            app.Start();

            // 4) Expose handles
            var svc = new SelfHostedPxService
            {
                SelfHost = app,
                HttpSelfHttpClient = new HttpClient { BaseAddress = PXBaseUri },
            };

            PXClient = svc.HttpSelfHttpClient;
            // Pull optional handlers if you registered them in DI
            PXHandler = svc.SelfHost.Services.GetService<PXServiceHandler>();
            PXCorsHandler = svc.SelfHost.Services.GetService<PXServiceCorsHandler>();
            PXFlightHandler = svc.SelfHost.Services.GetService<PXServiceFlightHandler>();

            return svc;
        }

        public void Dispose()
        {
            try { HttpSelfHttpClient?.Dispose(); } catch { }
            try { SelfHost?.Dispose(); } catch { }

        }

        /// <summary>
        /// Configure the middleware pipeline and map routes.
        /// </summary>
        public static void ConfigurePipeline(WebApplication app)
        {
            app.UseRouting();
            app.UseMiddleware<PXServiceApiVersionHandler>();

            WebApiConfig.AddUrlVersionedRoutes(app);
        }

        public static string GetAvailablePort()
        {
            var netProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpListeners = netProperties.GetActiveTcpListeners();
            var udpListeners = netProperties.GetActiveUdpListeners();

            var portsInUse = new List<int>();
            portsInUse.AddRange(tcpListeners.Select(tl => tl.Port));
            portsInUse.AddRange(udpListeners.Select(ul => ul.Port));

            int firstAvailablePort = 0;
            for (int port = 49152; port < 65535; port++)
            {
                if (!portsInUse.Contains(port) && !PreRegisteredPorts.Contains(port))
                {
                    firstAvailablePort = port;
                    break;
                }
            }

            return firstAvailablePort.ToString();
        }

        public static Dictionary<Type, HostableService> ConfigureDependencies(string fullBaseUrl = "", string protocol = "https")
        {
            // Decide base URL
            if (string.IsNullOrWhiteSpace(fullBaseUrl))
            {
                var p = GetAvailablePort();
                var scheme = string.IsNullOrWhiteSpace(protocol) ? "http" : protocol!;
                PXBaseUri = new Uri($"{scheme}://localhost:{p.ToString()}");
            }
            else
            {
                PXBaseUri = new Uri(fullBaseUrl);
            }

            // Dependencies need to selfhost before px, we need to reserve px
            // port so other dependencies don't take it.
            PreRegisteredPorts.Add(PXBaseUri.Port);


            // No-op: All dependencies are configured in ConfigureServices above.
            var selfhostServices = new[]
            {
                typeof(PIMSAccessor),
                typeof(AccountServiceAccessor),
                typeof(CatalogServiceAccessor),
                typeof(PayerAuthServiceAccessor),
                typeof(PurchaseServiceAccessor),
                typeof(StoredValueAccessor),
                typeof(TransactionServiceAccessor),
                typeof(MSRewardsServiceAccessor),
                typeof(TokenPolicyServiceAccessor),
                typeof(PartnerSettingsServiceAccessor),
                typeof(IssuerServiceAccessor),
                typeof(WalletServiceAccessor),
                typeof(TransactionDataService),
                typeof(ChallengeManagementService),
                typeof(RiskServiceAccessor),
                typeof(PaymentOrchestratorService),
                typeof(FraudDetectionService)
            };

            var selfHostedDependencies = new Dictionary<Type, HostableService>();

            HostableService? dependencyEmulatorService = null;

            // Create a single shared configuration action
            Action<WebApplicationBuilder> configAction = b => Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.WebApiConfig.Register(b);

            try
            {
                dependencyEmulatorService = new HostableService(configAction, Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.WebApiConfig.ConfigureRoutes, PXBaseUri);
            }
            catch
            {
                // Retry once just like the old code did
                try
                {
                    dependencyEmulatorService = new HostableService(configAction, Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.WebApiConfig.ConfigureRoutes, PXBaseUri);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to initialize DependencyEmulators. Error: {ex}");
                }
            }

            if (dependencyEmulatorService != null)
            {
                foreach (var selfhostService in selfhostServices)
                {
                    var serviceName = GetServiceName(selfhostService.FullName!);
                    Console.WriteLine($"{serviceName} initialized as self hosted emulator service on {dependencyEmulatorService.BaseUri}");
                    selfHostedDependencies[selfhostService] = dependencyEmulatorService;
                }
            }

            return selfHostedDependencies;
        }

        public void ResetDependencies()
        {
            // Reset stateful testing hooks (if registered)
            PXHandler?.ResetToDefault();
            PXFlightHandler?.ResetToDefault();

            // Reset dependency accessors
            PXSettings.PartnerSettingsService.ResetToDefaults();
            PXSettings.AccountsService.ResetToDefaults();
            PXSettings.PayerAuthService.ResetToDefaults();
            PXSettings.PimsService.ResetToDefaults();
            PXSettings.SessionService.ResetToDefaults();
            PXSettings.RiskService.ResetToDefaults();
            PXSettings.TaxIdService.ResetToDefaults();
            PXSettings.OrchestrationService.ResetToDefaults();
            PXSettings.HIPService.ResetToDefaults();
            PXSettings.WalletService.ResetToDefaults();
            PXSettings.TransactionService.ResetToDefaults();
            PXSettings.MSRewardsService.ResetToDefaults();
            PXSettings.TokenPolicyService.ResetToDefaults();
            PXSettings.TokenizationService.ResetToDefaults();
        }

        private static string GetServiceName(string serviceFullName)
        {
            return serviceFullName
                .Split('.')
                .Last()
                .Replace("Accessor", "Service")
                .Replace("ServiceService", "Service");
        }
    }
}
