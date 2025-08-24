using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

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
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions());
            builder.WebHost.UseTestServer();

            SelfHostedBootstrap.ConfigureServices(builder, useSelfHostedDependencies, useArrangedResponses);

            var app = builder.Build();

            SelfHostedBootstrap.ConfigurePipeline(app);

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

