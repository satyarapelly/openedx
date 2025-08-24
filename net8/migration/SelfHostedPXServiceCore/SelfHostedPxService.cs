using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Commerce.Payments.PXService;
using Microsoft.Commerce.Payments.PXService.Settings;
using PxMockSettings = SelfHostedPXServiceCore.Mocks.PXServiceSettings;

namespace SelfHostedPXServiceCore
{
    /// <summary>
    /// Lightweight PX host that runs the service in memory using <see cref="TestServer"/>.
    /// </summary>
    public sealed class SelfHostedPxService : IDisposable
    {
        /// <summary>HttpClient wired to the in-memory PX service.</summary>
        public HttpClient HttpClient { get; private set; } = default!;

        private IHost _host = default!;

        /// <summary>
        /// Spin up the PX service entirely in-memory. The returned client can be used to issue HTTP
        /// requests without opening any network sockets.
        /// </summary>
        public static SelfHostedPxService StartInMemory(bool useSelfHostedDependencies, bool useArrangedResponses)
        {
            PXServiceSettings settings = new PxMockSettings(
                useSelfHostedDependencies ? new Dictionary<Type, HostableService>() : null,
                useArrangedResponses);

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions());
            builder.WebHost.UseTestServer();

            // Register controllers, version selector and middleware dependencies
            WebApiConfig.Register(builder, settings);

            var app = builder.Build();

            // Ensure endpoint routing runs before custom middleware so HttpContext.GetEndpoint()
            // is populated for downstream components.
            app.UseRouting();
            app.UseMiddleware<PXServiceApiVersionHandler>();

            // Conventional + attribute routes
            WebApiConfig.AddUrlVersionedRoutes(app);

            app.Start();

            return new SelfHostedPxService
            {
                _host = app,
                HttpClient = app.GetTestClient()
            };
        }

        public void Dispose()
        {
            try { HttpClient?.Dispose(); } catch { }
            try { _host?.Dispose(); } catch { }
        }
    }
}

