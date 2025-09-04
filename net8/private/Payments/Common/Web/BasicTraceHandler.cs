// <copyright file="BasicTraceHandler.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Commerce.Payments.Common.Tracing;
using Microsoft.AspNetCore.Routing;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;


namespace Microsoft.Commerce.Payments.Common.Web
{
    public class BasicTraceHandler
    {
        private readonly RequestDelegate next;
        private readonly Action<HttpContext, string, string, EventTraceActivity, EventTraceActivity> traceRequest;
        private readonly Action<HttpContext, string, string, long, EventTraceActivity, EventTraceActivity> traceResponse;
        private readonly Func<HttpContext, string, long, string, EventTraceActivity, EventTraceActivity, Task> apiDetailTrace;

        public BasicTraceHandler(
            RequestDelegate next,
            Action<HttpContext, string, string, EventTraceActivity, EventTraceActivity> traceRequest,
            Action<HttpContext, string, string, long, EventTraceActivity, EventTraceActivity> traceResponse,
            Func<HttpContext, string, long, string, EventTraceActivity, EventTraceActivity, Task> apiDetailTrace)
        {
            this.next = next;
            this.traceRequest = traceRequest;
            this.traceResponse = traceResponse;
            this.apiDetailTrace = apiDetailTrace;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var serverTraceId = new EventTraceActivity(Guid.TryParse(context.TraceIdentifier, out var id) ? id : Guid.NewGuid());
            var requestTraceId = new EventTraceActivity(Guid.NewGuid())
            {
                CorrelationVectorV4 = serverTraceId.CorrelationVectorV4
            };

            string operationName = GetOperationName(context);
            context.Items["OperationName"] = operationName;

            string trackingId = context.Request.Headers.TryGetValue("X-TrackingId", out var tId) ? tId.ToString() : string.Empty;

            var stopwatch = Stopwatch.StartNew();

            traceRequest?.Invoke(context, string.Empty, operationName, requestTraceId, serverTraceId);

            await next(context);

            stopwatch.Stop();

            traceResponse?.Invoke(context, string.Empty, operationName, stopwatch.ElapsedMilliseconds, requestTraceId, serverTraceId);

            if (apiDetailTrace != null)
            {
                await apiDetailTrace(context, operationName, stopwatch.ElapsedMilliseconds, string.Empty, requestTraceId, serverTraceId);
            }
        }

        private string GetOperationName(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                var descriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                var method = context.Request.Method;

                if (descriptor != null)
                {
                    var controller = descriptor.ControllerName;
                    var action = descriptor.ActionName;

                    return $"{controller}-{method}-{action}";
                }
            }

            return $"{context.Request.Method}-{context.Request.Path}";
        }
    }
}
