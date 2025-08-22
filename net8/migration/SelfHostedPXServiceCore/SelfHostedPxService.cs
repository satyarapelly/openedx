// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXService;
    using Microsoft.Commerce.Payments.PXService.Accessors.IssuerService;
    using Microsoft.Commerce.Payments.PXService.Accessors.MSRewardsService;
    using Microsoft.Commerce.Payments.PXService.Accessors.PartnerSettingsService;
    using Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService;
    using Microsoft.Commerce.Payments.PXService.RiskService.V7;
    using Microsoft.Extensions.Options;
    using Mocks;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    public class SelfHostedPxService : IDisposable
    {
        public static HostableService PxHostableService { get; private set; }

        public static Dictionary<Type, HostableService> SelfHostedDependencies { get; private set; }

        public static PXServiceSettings PXSettings { get; private set; }

        public static PXServiceHandler PXHandler { get; private set; }

        public static PXServiceCorsHandler PXCorsHandler { get; private set; }

        public static PXServiceFlightHandler PXFlightHandler { get; private set; }

        public static PXServiceApiVersionHandler PXApiVersionHandler { get; private set; }


        public SelfHostedPxService(string fullBaseUrl, bool useSelfHostedDependencies, bool useArrangedResponses)
        {
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

            SelfHostedDependencies = new Dictionary<Type, HostableService>();

            if (useSelfHostedDependencies)
            {
                if (!string.IsNullOrEmpty(fullBaseUrl))
                {
                    var uri = new Uri(fullBaseUrl);

                    // Dependencies need to selfhost before px, we need to reserve px
                    // port so other dependencies don't take it.
                    HostableService.PreRegisteredPorts.Add(uri.Port);
                }

                // Create a single shared configuration action
                Action<WebApplicationBuilder> configAction = b => Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.WebApiConfig.Register(b);
                HostableService dependencyEmulatorService = null;

                try
                {
                    dependencyEmulatorService = new HostableService(configAction, null, "http", Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.WebApiConfig.ConfigureRoutes);
                }
                catch
                {
                    try
                    {
                        // Retry once again if failed for first time
                        dependencyEmulatorService = new HostableService(configAction, null, "http", Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.WebApiConfig.ConfigureRoutes);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to initialize DependencyEmulators. Error:{ex}");
                    }
                }

                foreach (var selfhostService in selfhostServices)
                {
                    var serviceName = GetServiceName(selfhostService.FullName);

                    Console.WriteLine($"{serviceName} initialized as self hosted emulator service on {dependencyEmulatorService.BaseUri}");
                    SelfHostedDependencies.Add(selfhostService, dependencyEmulatorService);
                }
            }

            PXSettings = new PXServiceSettings(SelfHostedDependencies, useArrangedResponses);
            // Define supported API versions and controllers allowed without an explicit version
            var supportedVersions = new Dictionary<string, ApiVersion>(StringComparer.OrdinalIgnoreCase)
            {
                { "v7.0", new ApiVersion("v7.0", new Version(7, 0)) }
            };
            string[] versionlessControllers = { GlobalConstants.ControllerNames.ProbeController };

            PxHostableService = new HostableService(
                builder =>
                {
                    WebApiConfig.Register(builder, PXSettings);

                    PXFlightHandler = new PXServiceFlightHandler();
                    PXHandler = new PXServiceHandler();
                    PXCorsHandler = new PXServiceCorsHandler(new PXServiceSettings());
                    PXApiVersionHandler = new PXServiceApiVersionHandler(supportedVersions, versionlessControllers, PXSettings);

                    builder.Services.AddSingleton(PXFlightHandler);
                    builder.Services.AddSingleton(PXHandler);
                    builder.Services.AddSingleton(PXCorsHandler);
                    builder.Services.AddSingleton(PXApiVersionHandler);

                    // Ensure the handlers participate in the ASP.NET Core request pipeline.
                    builder.Services.AddSingleton<IStartupFilter, PXServicePipelineFilter>();
                },
                fullBaseUrl,
                "http",
                WebApiConfig.ConfigureRoutes);
        }

        public void ResetDependencies()
        {
            PXHandler.ResetToDefault();
            PXFlightHandler.ResetToDefault();
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

        public void Dispose()
        {
            PxHostableService.SelfHostServer.StopAsync().Wait();
        }

        public static string GetPXServiceUrl(string relativePath)
        {
            Uri fullUri = new Uri(PxHostableService.BaseUri, relativePath);
            return fullUri.AbsoluteUri;
        }

        public static async Task GetRequest(string url, Dictionary<string, string> requestHeaders, Action<HttpStatusCode, string, HttpResponseHeaders> responseVerification)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, GetPXServiceUrl(url));
            requestHeaders?.ToList().ForEach(header => request.Headers.Add(header.Key, header.Value));
            var response = await PxHostableService.HttpSelfHttpClient.SendAsync(request);
            responseVerification(response.StatusCode, await response.Content.ReadAsStringAsync(), response.Headers);
        }

        public static string GetServiceName(string serviceFullName)
        {
            return serviceFullName
                .Split('.')
                .Last()
                .Replace("Accessor", "Service")
                .Replace("ServiceService", "Service");
        }
    }

    /// <summary>
    /// Configures the ASP.NET Core middleware pipeline for the self-hosted PX service.
    /// </summary>
    internal sealed class PXServicePipelineFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.UseMiddleware<PXServiceFlightHandler>();
                app.UseMiddleware<PXServiceCorsHandler>();
                app.UseMiddleware<PXServiceApiVersionHandler>();
                app.UseMiddleware<PXServiceHandler>();

                next(app);
            };
        }
    }
}