using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CIT.PXService.Tests
{
    /// <summary>
    /// Quick smoke test that ensures each controller has at least one routable endpoint.
    /// Helps catch missing route registrations after migration.
    /// </summary>
    [TestClass]
    public class RoutingSmokeTests : TestBase
    {
        [TestMethod]
        public async Task AllControllerEndpointsAreReachable()
        {
            var dataSources = SelfHostedPxService.PxHostableService.App.Services.GetServices<EndpointDataSource>();
            var endpoints = dataSources
                .SelectMany(ds => ds.Endpoints)
                .OfType<RouteEndpoint>()
                .Where(ep => ep.Metadata.GetMetadata<ControllerActionDescriptor>() != null)
                .GroupBy(ep => ep.Metadata.GetMetadata<ControllerActionDescriptor>()!.ControllerName)
                .Select(g => g.First());

            foreach (var ep in endpoints)
            {
                var pattern = "/" + ep.RoutePattern.RawText;
                pattern = pattern.Replace("{version}", "v7.0", StringComparison.OrdinalIgnoreCase)
                                 .Replace("{accountId}", "acc")
                                 .Replace("{piid}", "pi")
                                 .Replace("{ntid}", "nt")
                                 .Replace("{challengeid}", "ch")
                                 .Replace("{paymentRequestId}", "pr")
                                 .Replace("{checkoutRequestId}", "cr")
                                 .Replace("{sessionId}", "sid")
                                 .Replace("{appName}", "app")
                                 .Replace("{appVersion}", "1");
                pattern = Regex.Replace(pattern, "{[^/]+}", "1");

                var method = ep.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods?.FirstOrDefault() ?? HttpMethods.Get;
                var request = new HttpRequestMessage(new HttpMethod(method), GetPXServiceUrl(pattern));
                if (HttpMethods.IsPost(method) || HttpMethods.IsPatch(method))
                {
                    request.Content = new StringContent("{}", Encoding.UTF8, "application/json");
                }

                var response = await PXClient.SendAsync(request);
                Assert.AreNotEqual(HttpStatusCode.NotFound, response.StatusCode, $"{method} {pattern} returned 404");
            }
        }
    }
}
