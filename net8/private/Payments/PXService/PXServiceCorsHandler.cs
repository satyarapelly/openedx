using Microsoft.AspNetCore.Http;
using Microsoft.Commerce.Payments.Common.Web;
using Microsoft.Commerce.Payments.PXCommon;
using Microsoft.Commerce.Payments.PXService.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.Commerce.Payments.PXService
{
    /// <summary>
    /// Middleware that validates CORS requests for PX service.
    /// </summary>
    public class PXServiceCorsHandler
    {
        private readonly RequestDelegate _next;
        private readonly IList<string> _allowedOrigins;

        public PXServiceCorsHandler(RequestDelegate next, PXServiceSettings settings)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _allowedOrigins = settings?.CorsAllowedOrigins ?? new List<string>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var origin = context.Request.GetRequestHeader("Origin");
            var isCorsRequest = !string.IsNullOrEmpty(origin);

            if (!isCorsRequest)
            {
                await _next(context);
                return;
            }

            var allowedOrigin = _allowedOrigins.FirstOrDefault(o => string.Equals(o, origin, StringComparison.OrdinalIgnoreCase));
            if (allowedOrigin == null)
            {
                SllWebLogger.TracePXServiceException($"CORS request from domain not in allowed list: {origin}", context.Request.GetRequestCorrelationId());
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            if (HttpMethods.Options.Equals(context.Request.Method, StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
            }
            else
            {
                await _next(context);
            }

            context.Response.Headers.Add("Access-Control-Allow-Origin", origin);
        }
    }
}
