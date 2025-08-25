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
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;

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

        /// <summary>
        /// Spin up the PX service in-memory. The returned instance can be used to
        /// issue HTTP requests without opening any network sockets.
        /// </summary>
        public static SelfHostedPxService StartInMemory(string? baseUrl, bool useSelfHostedDependencies, bool useArrangedResponses)
        {

         var selfHostedDependencies = new Dictionary<Type, HostableService>();

		 if (useSelfHostedDependencies)
		 {
		     // Start up dependency emulators first so PX can connect to them.
		     // in different host with available port.
		     selfHostedDependencies = ConfigureDependencies(baseUrl);
		 }
 
            var baseUri = string.IsNullOrEmpty(baseUrl)
                ? new Uri("http://localhost")
                : new Uri(baseUrl);
 // Dependencies need to selfhost before px, we need to reserve px
 // port so other dependencies don't take it.
 PreRegisteredPorts.Add(PXBaseUri.Port);

 var builder = WebApplication.CreateBuilder(new WebApplicationOptions());
 // builder.WebHost.UseTestServer();

            var dependencies = useSelfHostedDependencies
                ? ConfigureDependencies(baseUri)
                : null;

 PXServiceSettings PXSettings = new Mocks.PXServiceSettings(
     useSelfHostedDependencies ? selfHostedDependencies : null,
     useArrangedResponses);

            PxHostableService = new HostableService(
                builder =>
                {
                    // Register PX controllers/services
                    SelfHostedBootstrap.ConfigureServices(builder, dependencies, useArrangedResponses);

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
                    SelfHostedBootstrap.ConfigurePipeline(app);
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
