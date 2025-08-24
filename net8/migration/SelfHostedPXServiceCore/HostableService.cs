// Copyright (c) Microsoft.
// .NET 8 version – no System.Web.* dependencies.
#nullable enable

namespace SelfHostedPXServiceCore
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using System;
    using System.Net.Http;

    /// <summary>
    /// Lightweight self-host wrapper for ASP.NET Core used by tests/emulators.
    /// Runs entirely in-process via <see cref="TestServer"/> so no TCP ports are
    /// required which keeps unit and integration tests fast and reliable.
    /// </summary>
    public sealed class HostableService : IDisposable
    {
        /// <summary>The base address used when generating absolute URLs.</summary>
        public Uri BaseUri { get; private set; } = default!;

        /// <summary>Client wired directly to the in-memory server.</summary>
        public HttpClient HttpSelfHttpClient { get; private set; } = default!;

        /// <summary>The ASP.NET Core application instance.</summary>
        public WebApplication App { get; private set; } = default!;

        /// <summary>
        /// Build and start a host with service and app configuration callbacks.
        /// </summary>
        /// <param name="configureServices">Adds services to the DI container.</param>
        /// <param name="configureApp">Configures the middleware pipeline.</param>
        /// <param name="baseUri">Base address used for generated URLs. No actual network binding occurs.</param>
        /// <param name="configureEndpoints">Optional conventional routing configuration.</param>
        public HostableService(
            Action<WebApplicationBuilder> configureServices,
            Action<WebApplication> configureApp,
            Uri baseUri,
            Action<IEndpointRouteBuilder>? configureEndpoints = null)
        {
            BaseUri = baseUri ?? new Uri("http://localhost");

            // Build host
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = Array.Empty<string>()
            });

            // Use the in-memory test server so no sockets are opened
            builder.WebHost.UseTestServer();

            // Baseline service setup (controllers + Newtonsoft, ignore nulls like old WebApiConfig did)
            builder.Services
                .AddControllers()
                .AddNewtonsoftJson(o => o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore);

            // Allow callers to register additional services
            configureServices?.Invoke(builder);

            App = builder.Build();

            // Run routing before custom middleware so endpoint metadata is available.
            App.UseRouting();

            // Allow callers to add middleware/endpoints
            configureApp?.Invoke(App);

            // Allow conventional routes prior to mapping controllers
            configureEndpoints?.Invoke(App);

            // Map attribute routed controllers and finalize pipeline
            App.MapControllers();

            // Start the in-memory server and create a client that talks to it
            App.Start();
            HttpSelfHttpClient = App.GetTestClient();
            HttpSelfHttpClient.BaseAddress = BaseUri;
        }

        public void Dispose()
        {
            try { HttpSelfHttpClient?.Dispose(); } catch { }
            try { App?.Dispose(); } catch { }
        }
    }
}
