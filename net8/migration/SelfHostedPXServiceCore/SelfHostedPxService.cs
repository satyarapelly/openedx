using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Commerce.Payments.PXService;
using Microsoft.Commerce.Payments.PXService.Settings;
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
            var baseUri = string.IsNullOrEmpty(baseUrl)
                ? new Uri("http://localhost")
                : new Uri(baseUrl);

            PxHostableService = new HostableService(
                builder =>
                {
                    // Register PX controllers/services
                    SelfHostedBootstrap.ConfigureServices(builder, useSelfHostedDependencies, useArrangedResponses);

                    // Middlewares exposed as testing hooks
                    builder.Services.AddSingleton<PXServiceHandler>();
                    builder.Services.AddSingleton<PXServiceFlightHandler>();
                },
                app =>
                {
                    // Order matters: run our test hooks before versioning/routes
                    app.UseMiddleware<PXServiceHandler>();
                    app.UseMiddleware<PXServiceFlightHandler>();
                    app.UseMiddleware<PXServiceCorsHandler>();
                    app.UseMiddleware<PXServiceApiVersionHandler>();
                    WebApiConfig.AddUrlVersionedRoutes(app);
                },
                baseUri);

            PXClient = PxHostableService.HttpSelfHttpClient;
            PXSettings = PxHostableService.App.Services.GetRequiredService<PXServiceSettings>();
            PXHandler = PxHostableService.App.Services.GetRequiredService<PXServiceHandler>();
            PXFlightHandler = PxHostableService.App.Services.GetRequiredService<PXServiceFlightHandler>();
            PXCorsHandler = PxHostableService.App.Services.GetService<PXServiceCorsHandler>();

            return new SelfHostedPxService();
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
    }
}

