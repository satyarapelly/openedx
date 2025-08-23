// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService;
    using Microsoft.Commerce.Payments.PXService.Accessors.IssuerService;
    using Microsoft.Commerce.Payments.PXService.Accessors.MSRewardsService;
    using Microsoft.Commerce.Payments.PXService.Accessors.PartnerSettingsService;
    using Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService;
    using Microsoft.Commerce.Payments.PXService.RiskService.V7;
    using Microsoft.Commerce.Payments.PXService.V7;
    using Microsoft.Extensions.DependencyInjection;
    using Mocks;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    public sealed class SelfHostedPxService : IDisposable
    {
        public static HostableService PxHostableService { get; private set; } = default!;
        public static Dictionary<Type, HostableService> SelfHostedDependencies { get; private set; } = default!;
        public static PXServiceSettings PXSettings { get; private set; } = default!;

        // If you migrated these handlers to “state” singletons used by middlewares,
        // keep references here so tests can ResetToDefault() like before.
        public static PXServiceHandler? PXHandler { get; private set; }
        public static PXServiceCorsHandler? PXCorsHandler { get; private set; }
        public static PXServiceFlightHandler? PXFlightHandler { get; private set; }

        public SelfHostedPxService(string? fullBaseUrl, bool useSelfHostedDependencies, bool useArrangedResponses)
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

            // Ensure PX port is “reserved” if you pass a fixed URL
            if (useSelfHostedDependencies && !string.IsNullOrWhiteSpace(fullBaseUrl))
            {
                var uri = new Uri(fullBaseUrl);
                HostableService.PreRegisteredPorts.Add(uri.Port);
            }

            // Spin up dependency emulator host (one host shared by all deps)
            if (useSelfHostedDependencies)
            {
                HostableService? dependencyEmulatorService = null;

                // Create a single shared configuration action
                Action<WebApplicationBuilder> configAction = b => Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.WebApiConfig.Register(b);

                try
                {
                    dependencyEmulatorService = new HostableService(configAction, Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.WebApiConfig.ConfigureRoutes, null, "http");
                }
                catch
                {
                    // Retry once just like the old code did
                    try
                    {
                        dependencyEmulatorService = new HostableService(configAction, Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.WebApiConfig.ConfigureRoutes, null, "http");
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
                        SelfHostedDependencies[selfhostService] = dependencyEmulatorService;
                    }
                }
            }

            // Create PX settings (consumes dependency hosts)
            PXSettings = new PXServiceSettings(SelfHostedDependencies, useArrangedResponses);

            // Define supported API versions and controllers allowed without an explicit version
            var supportedVersions = new Dictionary<string, ApiVersion>(StringComparer.OrdinalIgnoreCase)
            {
                { "v7.0", new ApiVersion("v7.0", new Version(7, 0)) }
            };
            string[] versionlessControllers = { GlobalConstants.ControllerNames.ProbeController };

            // Spin up the PX host
            PxHostableService = new HostableService(
                configureServices: builder =>
                {
                    WebApiConfig.Register(builder, PXSettings);
                    builder.Services.AddSingleton<PXServiceHandler>();     // your migrated state (used by PXServiceHandler middleware)
                    builder.Services.AddSingleton<PXServiceFlightHandler>(); // state used by flighting middleware
                },
                configureApp: app =>
                {
                    // Diagnostic middleware used during development to verify that routing has
                    // selected a controller. It prints the resolved controller name before and
                    // after the remainder of the pipeline executes. If HttpContext.GetEndpoint()
                    // is ever null here, the HostableService pipeline is misconfigured.
                    app.Use(async (context, next) =>
                    {
                        var beforeEndpoint = context.GetEndpoint();
                        var beforeCad = beforeEndpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
                        var beforeName = beforeCad?.ControllerName ?? "<null>";

                        await next();

                        var afterEndpoint = context.GetEndpoint();
                        var afterCad = afterEndpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
                        var afterName = afterCad?.ControllerName ?? "<null>";

                        Console.WriteLine(
                            $"[Endpoint] Path: {context.Request.Path}, Controller before: {beforeName}, after: {afterName}");
                    });

                    // Pull singletons for test access
                    if (!WebHostingUtility.IsApplicationSelfHosted())
                    {
                        app.UseMiddleware<PXTraceCorrelationHandler>();
                    }

                    app.UseMiddleware<PXServiceApiVersionHandler>();

                    if (PXSettings.PIDLDocumentValidationEnabled)
                    {
                        app.UseMiddleware<PXServicePIDLValidationHandler>();
                    }

                    app.UseMiddleware<PXServiceHandler>();

                    app.UseMiddleware<PXServiceFlightHandler>();
                },
                fullBaseUrl: fullBaseUrl,
                protocol: "https",
                configureEndpoints: endpoints =>
                {
                    // Conventional maps that mimic your old WebApiConfig
                    WebApiConfig.AddUrlVersionedRoutes(endpoints);
                });
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

        public void Dispose()
        {
            PxHostableService?.Dispose();
            // Note: dependency emulator HostableService lives in SelfHostedDependencies values;
            // they share a single instance—disposal is handled when test process ends, or you can dispose here if you track it.
        }

        public static string GetPXServiceUrl(string relativePath)
        {
            var fullUri = new Uri(PxHostableService.BaseUri, relativePath);
            return fullUri.AbsoluteUri;
        }

        public static async Task GetRequest(
            string url,
            Dictionary<string, string>? requestHeaders,
            Action<HttpStatusCode, string, HttpResponseHeaders> responseVerification)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, GetPXServiceUrl(url));
            requestHeaders?.ToList().ForEach(h => request.Headers.Add(h.Key, h.Value));
            var response = await PxHostableService.HttpSelfHttpClient.SendAsync(request);
            responseVerification(response.StatusCode, await response.Content.ReadAsStringAsync(), response.Headers);
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