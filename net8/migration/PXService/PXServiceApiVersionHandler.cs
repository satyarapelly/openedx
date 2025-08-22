using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Commerce.Payments.Common;
using Microsoft.Commerce.Payments.Common.Web;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Commerce.Payments.PXService
{
    /// <summary>
    /// Middleware that validates the api-version supplied for PX service calls.
    /// </summary>
    public class PXServiceApiVersionHandler
    {
        private readonly RequestDelegate _next;
        private readonly string[] _versionlessControllers;

        public PXServiceApiVersionHandler(RequestDelegate next, string[] versionlessControllers)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _versionlessControllers = versionlessControllers ?? Array.Empty<string>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var controllerName = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerName
                ?? context.Request.RouteValues["controller"]?.ToString();

            if (!string.IsNullOrEmpty(controllerName) &&
                _versionlessControllers.Contains(controllerName, StringComparer.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            var selector = context.RequestServices.GetRequiredService<VersionedControllerSelector>();
            var supportedVersions = selector.SupportedVersions;

            string? externalVersion = null;
            if (context.Request.Headers.TryGetValue(PaymentConstants.PaymentExtendedHttpHeaders.ApiVersion, out var versions))
            {
                externalVersion = versions.FirstOrDefault();
                if (versions.Count > 1)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Multiple api-version headers are not supported.");
                    return;
                }
            }

            var versionPattern = new Regex(@"^v\d+\.\d+/$", RegexOptions.IgnoreCase);
            if (string.IsNullOrEmpty(externalVersion))
            {
                foreach (var seg in context.Request.Path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>())
                {
                    if (versionPattern.IsMatch(seg + "/"))
                    {
                        externalVersion = seg;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(externalVersion))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Missing required api-version.");
                return;
            }

            if (!supportedVersions.TryGetValue(externalVersion, out var apiVersion))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync($"Unsupported api-version: {externalVersion}");
                return;
            }

            context.Items[PaymentConstants.Web.Properties.Version] = apiVersion;
            await _next(context);
        }
    }
}
