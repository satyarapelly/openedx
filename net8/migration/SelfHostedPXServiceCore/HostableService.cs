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
        public WebApplication App { get; private set; } = default!;

        /// <summary>
        /// Build + start a host. Use this overload when you only need to configure the app pipeline.
        /// </summary>
        /// <param name="configureApp">Configure middleware/endpoints. <c>MapControllers()</c> is already called.</param>
        /// <param name="fullBaseUrl">e.g. "http://localhost:49152". If null/empty a free port is chosen.</param>
        public HostableService(Action<WebApplication> configureApp, Uri fullBaseUrl)
            : this(_ => { }, configureApp, fullBaseUrl)
        {
        }

        /// <summary>
        /// Build + start a host with service configuration and app configuration callbacks.
        /// </summary>
        /// <param name="configureServices">Add DI/services. Controllers + Newtonsoft JSON are already registered.</param>
        /// <param name="configureApp">Configure middleware/endpoints. A routing pipeline is already wired so that
        /// middlewares added here run <em>after</em> <c>UseRouting()</c> but before endpoints are mapped.</param>
        /// <param name="fullBaseUrl">e.g. "http://localhost:49152". If null/empty a free port is chosen.</param>
        public HostableService(
            Action<WebApplicationBuilder> configureServices,
            Action<WebApplication> configureApp,
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

            // Ensure the routing matcher runs before custom middleware so HttpContext.GetEndpoint()
            // is populated when those middlewares execute.
            App.UseRouting();

            // Emit the resolved endpoint for each request so callers can verify routing
            // is functioning as expected.
            App.Use(async (ctx, next) =>
            {
                var ep = ctx.GetEndpoint();
                Console.WriteLine($"[HostableService] Endpoint: {ep?.DisplayName ?? "(null)"}");
                await next();
            });

            // Callers can add middlewares, filters, etc. They will execute after routing but before
            // the selected endpoint is invoked.
            configureApp?.Invoke(App);

            // Finalize the pipeline: register any conventional routes and map controllers
            // via UseEndpoints so that routing metadata is available to consumers like
            // HttpContext.GetEndpoint().
            App.UseEndpoints(endpoints =>
            {
                // Simple probe endpoint for quick sanity checks
                endpoints.MapGet("/probe", async ctx => await ctx.Response.WriteAsync("OK"));

                // Allow callers to register conventional routes prior to mapping controllers
                configureEndpoints?.Invoke(endpoints);

                // Map attribute/route-based controllers and finalize the endpoint pipeline
                endpoints.MapControllers();
            });

            // Start server
            App.Start();

            HttpSelfHttpClient = App.GetTestClient();
            HttpSelfHttpClient.BaseAddress = BaseUri;
        }

        public void Dispose()
        {
            try { App?.StopAsync().GetAwaiter().GetResult(); } catch { }
            try { HttpSelfHttpClient?.Dispose(); } catch { }
        }
    }
}