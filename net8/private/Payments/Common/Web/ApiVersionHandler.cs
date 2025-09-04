// <copyright file="ApiVersionHandler.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

using Microsoft.AspNetCore.Http;
using Microsoft.Commerce.Payments.Common.Tracing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Commerce.Payments.Common.Web
{
    public class ApiVersionHandler
    {
        private readonly RequestDelegate _next;
        private readonly IDictionary<string, ApiVersion> _supportedVersions;

        public
            ApiVersionHandler(RequestDelegate next, IDictionary<string, ApiVersion> supportedVersions)
        {
            _next = next;
            _supportedVersions = supportedVersions ?? throw new ArgumentNullException(nameof(supportedVersions));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;
            string? externalVersion = null;

            if (request.Headers.TryGetValue(PaymentConstants.PaymentExtendedHttpHeaders.ApiVersion, out var apiVersionHeaderValues))
            {
                externalVersion = apiVersionHeaderValues.FirstOrDefault();

                if (apiVersionHeaderValues.Count > 1)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Multiple api-version headers are not supported.");
                    return;
                }
            }

            // Check for version in URL segment (e.g., /v3.0/)
            var versionPattern = new Regex(@"^v\d+\.\d+/$", RegexOptions.IgnoreCase);
            foreach (var segment in request.Path.Value.Split('/', StringSplitOptions.RemoveEmptyEntries))
            {
                if (versionPattern.IsMatch(segment + "/"))
                {
                    externalVersion = segment;
                    break;
                }
            }

            if (string.IsNullOrEmpty(externalVersion))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Missing required api-version.");
                return;
            }

            if (!_supportedVersions.TryGetValue(externalVersion, out var apiVersion))
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
