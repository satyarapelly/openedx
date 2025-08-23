// Copyright (c) Microsoft.
// .NET 8 version – no System.Web.* dependencies.
#nullable enable

namespace SelfHostedPXServiceCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.NetworkInformation;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Newtonsoft.Json;

    /// <summary>
    /// Lightweight self-host wrapper for ASP.NET Core used by tests/emulators.
    /// </summary>
    public sealed class HostableService : IDisposable
    {
        /// <summary>Ports you want to “reserve” before starting other hosts.</summary>
        public static List<int> PreRegisteredPorts { get; } = new();

        public string Port { get; private set; } = string.Empty;
        public Uri BaseUri { get; private set; } = default!;
        public HttpClient HttpSelfHttpClient { get; private set; } = default!;
        public WebApplication App { get; private set; } = default!;

        /// <summary>
        /// Build + start a host. Use this overload when you only need to configure the app pipeline.
        /// </summary>
        /// <param name="configureApp">Configure middleware/endpoints. <c>MapControllers()</c> is already called.</param>
        /// <param name="fullBaseUrl">e.g. "http://localhost:49152". If null/empty a free port is chosen.</param>
        /// <param name="protocol">"http" (default) or "https" (requires dev cert bound).</param>
        public HostableService(Action<WebApplication> configureApp, string? fullBaseUrl, string? protocol)
            : this(_ => { }, configureApp, fullBaseUrl, protocol)
        {
        }

        /// <summary>
        /// Build + start a host with service configuration and app configuration callbacks.
        /// </summary>
        /// <param name="configureServices">Add DI/services. Controllers + Newtonsoft JSON are already registered.</param>
        /// <param name="configureApp">Configure middleware/endpoints. A routing pipeline is already wired so that
        /// middlewares added here run <em>after</em> <c>UseRouting()</c> but before endpoints are mapped.</param>
        /// <param name="fullBaseUrl">e.g. "http://localhost:49152". If null/empty a free port is chosen.</param>
        /// <param name="protocol">"http" (default) or "https" (requires dev cert bound).</param>
        public HostableService(
            Action<WebApplicationBuilder> configureServices,
            Action<WebApplication> configureApp,
            string? fullBaseUrl,
            string? protocol)
        {
            // Decide base URL
            if (string.IsNullOrWhiteSpace(fullBaseUrl))
            {
                var p = GetAvailablePort();
                Port = p.ToString();
                var scheme = string.IsNullOrWhiteSpace(protocol) ? "http" : protocol!;
                BaseUri = new Uri($"{scheme}://localhost:{Port}");
            }
            else
            {
                BaseUri = new Uri(fullBaseUrl);
                Port = BaseUri.Port.ToString();
            }

            // Build host
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = Array.Empty<string>()
            });

            // Baseline service setup (controllers + Newtonsoft, ignore nulls like old WebApiConfig did)
            builder.Services
                .AddControllers()
                .AddNewtonsoftJson(o => o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore);

            // Callers can add more services
            configureServices?.Invoke(builder);

            App = builder.Build();

            // Bind to requested URL/port
            App.Urls.Clear();
            App.Urls.Add(BaseUri.ToString());

            // Conditionally redirect HTTP to HTTPS only when not self-hosted
            if (!WebHostingUtility.IsApplicationSelfHosted())
            {
                App.UseHttpsRedirection();
            }

            // Ensure the routing matcher runs before custom middleware so HttpContext.GetEndpoint()
            // is populated when those middlewares execute. Endpoints must be registered before the
            // matcher is built, so controller routes are added inside UseEndpoints which runs after
            // our custom middleware.
            App.UseRouting();

            // Callers can add middlewares, filters, etc. They will execute after routing but before
            // the selected endpoint is invoked.
            configureApp?.Invoke(App);

            // Map attribute/route-based controllers and finalize the endpoint pipeline
            App.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Start server
            App.Start();

            HttpSelfHttpClient = new HttpClient
            {
                BaseAddress = BaseUri
            };
        }

        public void Dispose()
        {
            try { App?.StopAsync().GetAwaiter().GetResult(); } catch { }
            try { HttpSelfHttpClient?.Dispose(); } catch { }
        }

        private static string GetAvailablePort()
        {
            var netProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpListeners = netProperties.GetActiveTcpListeners();
            var udpListeners = netProperties.GetActiveUdpListeners();

            var portsInUse = new List<int>();
            portsInUse.AddRange(tcpListeners.Select(tl => tl.Port));
            portsInUse.AddRange(udpListeners.Select(ul => ul.Port));

            int firstAvailablePort = 0;
            for (int port = 49152; port < 65535; port++)
            {
                if (!portsInUse.Contains(port) && !PreRegisteredPorts.Contains(port))
                {
                    firstAvailablePort = port;
                    break;
                }
            }

            return firstAvailablePort.ToString();
        }
    }
}
