// Copyright (c) Microsoft.
// .NET 8 version – no System.Web.* dependencies.
#nullable enable

namespace SelfHostedPXServiceCore
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Newtonsoft.Json;
    using System;
    using System.Net.Http;

    /// <summary>
    /// Lightweight self-host wrapper for ASP.NET Core used by tests/emulators.
    /// </summary>
    public sealed class HostableService : IDisposable
    {
        public Uri BaseUri { get; private set; } = default!;
        public HttpClient HttpSelfHttpClient { get; private set; } = default!;
        public HttpClient Client => HttpSelfHttpClient;
        public WebApplication App { get; private set; } = default!;

        /// <summary>
        /// Build + start a host. Use this overload when you only need to configure the app pipeline.
        /// </summary>
        /// <param name="configureApp">Configure middleware/endpoints. <c>MapControllers()</c> is already called.</param>
        /// <param name="fullBaseUrl">e.g. "http://localhost:49152". If null/empty a free port is chosen.</param>
        public HostableService(Action<WebApplication> configureApp, Uri fullBaseUrl)
            : this(_ => { }, null, configureApp, fullBaseUrl)
        {
        }

        /// <summary>
        /// Build + start a host with service configuration and app configuration callbacks.
        /// </summary>
        /// <param name="configureServices">Add DI/services. Controllers + Newtonsoft JSON are already registered.</param>
        /// <param name="configureBeforeRouting">Configure middleware that must run <em>before</em> routing executes
        /// (e.g. URL rewriters).</param>
        /// <param name="configureApp">Configure middleware that runs <em>after</em> routing but before endpoints are
        /// invoked.</param>
        /// <param name="fullBaseUrl">e.g. "http://localhost:49152". If null/empty a free port is chosen.</param>
        public HostableService(
            Action<WebApplicationBuilder> configureServices,
            Action<WebApplication>? configureBeforeRouting,
            Action<WebApplication>? configureApp,
            Uri baseUri,
            Action<IEndpointRouteBuilder>? configureEndpoints = null)
        {

            BaseUri = baseUri;
            // Build host
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = Array.Empty<string>()
            });

            // Run entirely in-memory
            builder.WebHost.UseTestServer();

            // Baseline service setup (controllers + Newtonsoft, ignore nulls like old WebApiConfig did)
            builder.Services
                .AddControllers()
                .AddNewtonsoftJson(o => o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore);

            // Callers can add more services
            configureServices?.Invoke(builder);

            App = builder.Build();

            // Allow callers to register middleware that needs to run before routing (e.g. to mutate
            // the request path used for endpoint matching).
            configureBeforeRouting?.Invoke(App);

            // Wire up routing so the endpoint matcher runs before custom post-routing middleware.
            App.UseRouting();

            // Log which endpoint was selected for each request to aid in debugging tests.
            App.Use(async (ctx, next) =>
            {
                var ep = ctx.GetEndpoint();
                Console.WriteLine($"[HostableService] Endpoint: {ep?.DisplayName ?? "(null)"}");
                if (ep == null)
                {
                    throw new InvalidOperationException($"No endpoint matched for {ctx.Request.Method} {ctx.Request.Path}");
                }
                await next();
            });

            // Callers can add middlewares, filters, etc. They will execute after routing but before
            // the selected endpoint is invoked.
            configureApp?.Invoke(App);

            // Map routes + controllers using top-level registration so HttpContext.GetEndpoint()
            // returns the matched endpoint during the middleware above.
            configureEndpoints?.Invoke(App);
            App.MapControllers();

            // Start server
            App.Start();

            HttpSelfHttpClient = App.GetTestClient();
            HttpSelfHttpClient.BaseAddress = BaseUri;
        }

        public void Dispose()
        {
            try { App?.StopAsync().GetAwaiter().GetResult(); } catch { }
            try { App?.DisposeAsync().AsTask().GetAwaiter().GetResult(); } catch { }
            try { HttpSelfHttpClient?.Dispose(); } catch { }
        }
    }
}