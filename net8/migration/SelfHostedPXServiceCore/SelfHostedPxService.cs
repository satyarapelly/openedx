// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Commerce.Payments.PXService;
    using Microsoft.Commerce.Payments.PXService.Accessors.IssuerService;
    using Microsoft.Commerce.Payments.PXService.Accessors.MSRewardsService;
    using Microsoft.Commerce.Payments.PXService.Accessors.PartnerSettingsService;
    using Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService;
    using Microsoft.Commerce.Payments.PXService.RiskService.V7;
    using Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators;
    using Mocks;

    public class SelfHostedPxService : IDisposable
    {
        public static HostableService PxHostableService { get; private set; }

        public static Dictionary<Type, HostableService> SelfHostedDependencies { get; private set; }

        public static PXServiceSettings PXSettings { get; private set; }

        public static PXServiceHandler PXHandler { get; private set; }

        public static PXServiceCorsHandler PXCorsHandler { get; private set; }

        public static PXServiceFlightHandler PXFlightHandler { get; private set; }

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
                    HostableService.PreRegisteredPorts.Add(uri.Port);
                }

                Action<WebApplication> configAction = app =>
                {
                    app.MapControllers();
                    app.MapPartnerSettingsRoutes();
                    app.MapPIMSRoutes();
                    app.MapMSRewardsRoutes();
                    app.MapCatalogRoutes();
                    app.MapAccountRoutes();
                    app.MapIssuerServiceRoutes();
                    app.MapChallengeManagementRoutes();
                    app.MapPaymentThirdPartyRoutes();
                    app.MapPurchaseRoutes();
                    app.MapRiskRoutes();
                    app.MapSellerMarketPlaceRoutes();
                    app.MapTokenPolicyRoutes();
                    app.MapStoredValueRoutes();
                    app.MapTransactionServiceRoutes();
                    app.MapPaymentOchestratorRoutes();
                    app.MapPayerAuthRoutes();
                    app.MapFraudDetectionRoutes();
                };
                HostableService dependencyEmulatorService = null;

                try
                {
                    dependencyEmulatorService = new HostableService(configAction, null, "http");
                }
                catch
                {
                    try
                    {
                        dependencyEmulatorService = new HostableService(configAction, null, "http");
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
            PxHostableService = new HostableService(
                app =>
                {
                    app.MapControllers();
                },
                fullBaseUrl,
                "http",
                builder =>
                {
                    builder.Services.AddSingleton(PXSettings);
                });

            var server = PxHostableService.WebApp.GetTestServer();
            PXFlightHandler = new PXServiceFlightHandler();
            PXCorsHandler = new PXServiceCorsHandler(new PXServiceSettings());
            PXHandler = new PXServiceHandler();

            PXFlightHandler.InnerHandler = PXCorsHandler;
            PXCorsHandler.InnerHandler = PXHandler;
            PXHandler.InnerHandler = server.CreateHandler();

            PxHostableService.HttpSelfHttpClient = new HttpClient(PXFlightHandler)
            {
                BaseAddress = PxHostableService.BaseUri
            };
        }

        public void ResetDependencies()
        {
            PXHandler.ResetToDefault();
            PXFlightHandler.ResetToDefault();
        }

        public void Dispose()
        {
            PxHostableService.Dispose();
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

        private string GetServiceName(string serviceFullName)
        {
            return serviceFullName
                .Split('.')
                .Last()
                .Replace("Accessor", "Service")
                .Replace("ServiceService", "Service");
        }
    }
}
