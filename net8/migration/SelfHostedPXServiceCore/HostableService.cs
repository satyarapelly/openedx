namespace SelfHostedPXServiceCore
{
    using Castle.Components.DictionaryAdapter;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXService;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.NetworkInformation;

    public class HostableService : IDisposable
    {
        public static List<int> PreRegisteredPorts { get; private set; }

        public string Port { get; private set; }

        public Uri BaseUri { get; private set; }

        public WebApplication SelfHostServer { get; private set; }

        public HttpClient HttpSelfHttpClient { get; private set; }

        static HostableService()
        {
            PreRegisteredPorts = new EditableList<int>();
        }

        public HostableService(Action<WebApplicationBuilder> registerConfig, string? fullBaseUrl, string? protocol, Action<IEndpointRouteBuilder>? configureRoutes = null)
        {
            if (string.IsNullOrEmpty(fullBaseUrl))
            {
                Port = GetAvailablePort();

                if (string.IsNullOrEmpty(protocol))
                {
                    protocol = "https";
                }

                BaseUri = new Uri(string.Format("{0}://localhost:{1}", protocol, Port));
            }
            else
            {
                BaseUri = new Uri(fullBaseUrl);
            }

            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseUrls(BaseUri.AbsoluteUri);

            registerConfig(builder);

            SelfHostServer = builder.Build();

            SelfHostServer.MapControllers();

            SelfHostServer.MapGet("/routes", (EndpointDataSource ds) =>
                Results.Text(string.Join(Environment.NewLine,
                ds.Endpoints.OfType<RouteEndpoint>().Select(e => e.RoutePattern.RawText))));

            // Define supported API versions and controllers allowed without an explicit version
            //var supportedVersions = new Dictionary<string, ApiVersion>(StringComparer.OrdinalIgnoreCase)
            //{
            //    { "v7.0", new ApiVersion("v7.0", new Version(7, 0)) }
            //};
            //string[] versionlessControllers = { GlobalConstants.ControllerNames.ProbeController };
            // SelfHostServer.UseMiddleware<PXServiceApiVersionHandler>(configureRoutes, versionlessControllers, SelfHostedPxService.PXSettings);

            SelfHostServer.Use(async (ctx, next) =>
            {
                var resolver = ctx.RequestServices.GetRequiredService<VersionedControllerSelector>();

                // Try to read what routing selected
                var endpoint = ctx.GetEndpoint();
                var cad = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

                string? controllerName = cad?.ControllerName;
                string? versionFromHeader = ctx.Request.Headers["api-version"];
                string? versionFromRoute = ctx.Request.RouteValues.TryGetValue("version", out var vObj) ? vObj?.ToString() : null;
                string? version = !string.IsNullOrWhiteSpace(versionFromHeader) ? versionFromHeader : versionFromRoute;

                // If we have a selected controller, consult the resolver
                if (!string.IsNullOrEmpty(controllerName))
                {
                    var allowedType = resolver.ResolveAllowedController(ctx); // your existing helper
                    if (allowedType is null)
                    {
                        ctx.Response.StatusCode = StatusCodes.Status404NotFound;
                        await ctx.Response.WriteAsync($"No controller mapped for version '{version ?? "(none)"}'.");
                        return;
                    }

                    // ok, let it flow
                    await next();
                    return;
                }

                // No endpoint matched (RouteData is null/empty)  try to parse and fail fast with a clearer 404
                // Expected path like: /v7.0/Account001/AddressDescriptions
                    var segments = ctx.Request.Path.Value?.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                if (segments.Length >= 3 && segments[0].StartsWith("v", StringComparison.OrdinalIgnoreCase))
                {
                    var parsedVersion = segments[0][1..];         // "7.0"
                    var parsedController = segments[2];           // "AddressDescriptions"

                    // Fake the route values just for checking
                    ctx.Request.RouteValues["controller"] = parsedController;
                    if (resolver.ResolveAllowedController(ctx) is null)
                    {
                        ctx.Response.StatusCode = StatusCodes.Status404NotFound;
                        await ctx.Response.WriteAsync($"No controller mapped for version '{parsedVersion}'.");
                        return;
                    }
                }

                // Fall through; MVC will produce its normal 404
                await next();
            });

            configureRoutes?.Invoke(SelfHostServer);
            SelfHostServer.StartAsync().Wait();

            HttpSelfHttpClient = new HttpClient
            {
                BaseAddress = BaseUri,
            };
        }

        public void Dispose()
        {
            SelfHostServer.StopAsync().Wait();
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