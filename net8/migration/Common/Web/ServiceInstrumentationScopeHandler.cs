// <copyright file="ServiceInstrumentationScopeHandler.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Commerce.Payments.Common.Tracing;

namespace Microsoft.Commerce.Payments.Common.Web
{
    public class ServiceInstrumentationScopeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ServiceInstrumentationCounters _counters;

        public ServiceInstrumentationScopeMiddleware(RequestDelegate next, ServiceInstrumentationCounters counters)
        {
            _next = next;
            _counters = counters;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;

            string operationName = request.GetOperationName();
            string externalVersion = request.GetApiVersion();
            string instanceName = string.IsNullOrEmpty(externalVersion) ? operationName : $"{operationName}_{externalVersion}";

            var traceId = request.GetRequestCorrelationId();

            using (var scope = new ServiceInstrumentationScope(_counters, traceId, null, null, instanceName))
            {
                // Buffer the response to capture status code
                var originalBody = response.Body;
                using var memoryStream = new MemoryStream();
                response.Body = memoryStream;

                await _next(context); // invoke next middleware

                response.Body = originalBody;
                memoryStream.Seek(0, SeekOrigin.Begin);
                await memoryStream.CopyToAsync(originalBody);

                var statusCode = context.Response.StatusCode;

                if (scope != null)
                {
                    if (statusCode >= 200 && statusCode < 300 ||
                        context.Response.DoesReponseIndicateIdempotentTransaction() ||
                        statusCode == StatusCodes.Status504GatewayTimeout)
                    {
                        scope.Success();
                    }

                    if (statusCode >= 400 && statusCode < 500)
                    {
                        scope.UserError();
                    }
                }
            }
        }
    }
}
