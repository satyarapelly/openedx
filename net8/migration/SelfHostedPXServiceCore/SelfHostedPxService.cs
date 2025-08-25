using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;

namespace SelfHostedPXServiceCore
{
    /// <summary>
    /// Lightweight PX host that runs the service entirely in-memory using
    /// <see cref="Microsoft.AspNetCore.TestHost.TestServer"/>. This mirrors the
    /// old HttpSelfHostServer approach and avoids the need for real TCP ports
    /// making unit and integration tests fast and reliable.
    /// </summary>
    public sealed class SelfHostedPxService : IDisposable
    {
        /// <summary>Wrapper around the ASP.NET Core host.</summary>
        public static HostableService PxHostableService { get; private set; } = default!;

        /// <summary>Dependency emulator hosts keyed by accessor type.</summary>
        public static Dictionary<Type, HostableService> SelfHostedDependencies { get; private set; } = new();

        /// <summary>PX service settings registered with the host.</summary>
        public static PXServiceSettings PXSettings { get; private set; } = default!;

        /// <summary>Testing hook that allows request/response interception.</summary>
        public static PXServiceHandler PXHandler { get; private set; } = default!;

        /// <summary>Handler that stamps enabled flights onto requests.</summary>
        public static PXServiceFlightHandler PXFlightHandler { get; private set; } = default!;

        /// <summary>Exposed for completeness – tests do not currently mutate it.</summary>
        public static PXServiceCorsHandler? PXCorsHandler { get; private set; }

        /// <summary>Client wired directly to the in-memory PX service.</summary>
        public static HttpClient PXClient { get; private set; } = default!;

        /// <summary>Convenience accessor for the HttpClient.</summary>
        public HttpClient HttpSelfHttpClient => PXClient;

        /// <summary>Expose the underlying host if required.</summary>
        public IHost SelfHost => PxHostableService.App;

        /// <summary>Ports that have been reserved to avoid collisions.</summary>
        public static List<int> PreRegisteredPorts { get; } = new();

        /// <summary>Base address used by the in-memory host.</summary>
        public static Uri PXBaseUri { get; private set; } = new Uri("http://localhost");

        /// <summary>
        /// Spin up the PX service in-memory. The returned instance can be used to
        /// issue HTTP requests without opening any network sockets.
        /// </summary>
        public static SelfHostedPxService StartInMemory(string? baseUrl, bool useSelfHostedDependencies, bool useArrangedResponses)
        {
            var baseUri = string.IsNullOrEmpty(baseUrl)
                ? new Uri($"http://localhost:{GetAvailablePort()}")
                : new Uri(baseUrl);

            PXBaseUri = baseUri;

            // Dependencies need to selfhost before PX, reserve the port so others don't take it.
            PreRegisteredPorts.Add(PXBaseUri.Port);

            var selfHostedDependencies = new Dictionary<Type, HostableService>();
            if (useSelfHostedDependencies)
            {
                // Start up dependency emulators first so PX can connect to them.
                selfHostedDependencies = ConfigureDependencies(baseUri);
            }

            var dependencies = useSelfHostedDependencies ? selfHostedDependencies : null;

            PxHostableService = new HostableService(
                builder =>
                {
                    // Register PX controllers/services
                    ConfigureServices(builder, dependencies, useArrangedResponses);

                    // Middlewares exposed as testing hooks
                    builder.Services.AddSingleton<PXServiceHandler>();
                    builder.Services.AddSingleton<PXServiceFlightHandler>();
                },
                app =>
                {
                    // Order matters: run our test hooks before the standard PX pipeline
                    app.UseMiddleware<PXServiceHandler>();
                    app.UseMiddleware<PXServiceFlightHandler>();
                    app.UseMiddleware<PXServiceCorsHandler>();

                    // Configure routing and versioned endpoints
                    ConfigurePipeline(app);
                },
                baseUri);

            PXClient = PxHostableService.HttpSelfHttpClient;
            PXSettings = PxHostableService.App.Services.GetRequiredService<PXServiceSettings>();
            PXHandler = PxHostableService.App.Services.GetRequiredService<PXServiceHandler>();
            PXFlightHandler = PxHostableService.App.Services.GetRequiredService<PXServiceFlightHandler>();
            PXCorsHandler = PxHostableService.App.Services.GetService<PXServiceCorsHandler>();
            SelfHostedDependencies = dependencies ?? new Dictionary<Type, HostableService>();

            return new SelfHostedPxService();
        }

        private static void ConfigureServices(WebApplicationBuilder builder, Dictionary<Type, HostableService>? dependencies, bool useArrangedResponses)
        {
            var settings = new Mocks.PXServiceSettings(dependencies, useArrangedResponses);
            WebApiConfig.Register(builder, settings);
        }

        private static void ConfigurePipeline(WebApplication app)
        {
            app.UseRouting();

            app.Use(async (ctx, next) =>
            {
                var ep = ctx.GetEndpoint();
                Console.WriteLine($"[SelfHostedPxService] Endpoint: {ep?.DisplayName ?? "(null)"}");
                await next();
            });

            app.UseMiddleware<PXServiceApiVersionHandler>();
            WebApiConfig.AddUrlVersionedRoutes(app);
            app.MapControllers();
        }

        private static string GetAvailablePort()
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

        public void Dispose()
        {
            try { PxHostableService?.Dispose(); } catch { }
        }

        /// <summary>
        /// Resets stateful testing hooks and dependency accessors to their default
        /// configuration. This mirrors the behaviour of the legacy self-host.
        /// </summary>
        public void ResetDependencies()
        {
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

        private static Dictionary<Type, HostableService> ConfigureDependencies(Uri baseUri)
        {
            var selfhostServices = new[]
            {
                typeof(PIMSAccessor),
                typeof(OrchestrationServiceAccessor),
                typeof(AccountServiceAccessor),
                typeof(PayerAuthServiceAccessor),
                typeof(PurchaseServiceAccessor),
                typeof(CatalogServiceAccessor),
                typeof(SessionServiceAccessor),
                typeof(StoredValueAccessor),
                typeof(RiskServiceAccessor),
                typeof(TaxIdServiceAccessor),
                typeof(AddressEnrichmentServiceAccessor),
                typeof(TransactionServiceAccessor),
                typeof(SellerMarketPlaceServiceAccessor),
                typeof(PaymentThirdPartyServiceAccessor),
                typeof(AzureExPAccessor),
                typeof(PartnerSettingsServiceAccessor),
                typeof(IssuerServiceAccessor),
                typeof(ChallengeManagementServiceAccessor),
                typeof(WalletServiceAccessor),
                typeof(TransactionDataServiceAccessor),
                typeof(MSRewardsServiceAccessor),
                typeof(TokenPolicyServiceAccessor),
                typeof(TokenizationServiceAccessor),
                typeof(PaymentOrchestratorServiceAccessor),
                typeof(FraudDetectionServiceAccessor)
            };

            var selfHostedDependencies = new Dictionary<Type, HostableService>();

            HostableService? dependencyEmulatorService = null;

            Action<WebApplicationBuilder> configAction = b =>
                Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.WebApiConfig.Register(b);

            try
            {
                dependencyEmulatorService = new HostableService(
                    configAction,
                    _ => { },
                    baseUri,
                    Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.WebApiConfig.ConfigureRoutes);
            }
            catch
            {
                try
                {
                    dependencyEmulatorService = new HostableService(
                        configAction,
                        _ => { },
                        baseUri,
                        Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.WebApiConfig.ConfigureRoutes);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to initialize DependencyEmulators. Error: {ex}");
                }
            }

            if (dependencyEmulatorService != null)
            {
                foreach (var svc in selfhostServices)
                {
                    var serviceName = GetServiceName(svc.FullName!);
                    Console.WriteLine($"{serviceName} initialized as self hosted emulator service on {dependencyEmulatorService.BaseUri}");
                    selfHostedDependencies[svc] = dependencyEmulatorService;
                }
            }

            return selfHostedDependencies;
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
