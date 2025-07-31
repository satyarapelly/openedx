// <copyright file="PXTracingHandler.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;

    public class PXTracingHandler
    {
        private readonly RequestDelegate _next;
        private readonly string _serviceName;
        private readonly Action<string, EventTraceActivity> _logError;
        private readonly Action<string, string, EventTraceActivity> _logRequest;
        private readonly Action<string, EventTraceActivity> _logResponse;

        public PXTracingHandler(
            RequestDelegate next,
            string serviceName,
            Action<string, EventTraceActivity> logError = null,
            Action<string, string, EventTraceActivity> logRequest = null,
            Action<string, EventTraceActivity> logResponse = null)
        {
            _next = next;
            _serviceName = serviceName;
            _logError = logError ?? ((m, t) => { });
            _logRequest = logRequest ?? ((a, m, t) => { });
            _logResponse = logResponse ?? ((m, t) => { });
        }

        public async Task Invoke(HttpContext context)
        {
            var traceId = context.Request.GetRequestCorrelationId();
            var startTime = DateTime.UtcNow;

            try
            {
                var requestInfo = new StringBuilder(1000);
                requestInfo.AppendLine($"Request: {context.Request.Method} {context.Request.Path}");

                foreach (var header in context.Request.Headers)
                {
                    requestInfo.AppendLine($"{header.Key}: {string.Join(",", header.Value)}");
                }

                if (context.Request.ContentLength > 0 && context.Request.Body != null)
                {
                    context.Request.EnableBuffering();
                    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                    var body = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;
                    requestInfo.AppendLine(body);
                }

                _logRequest(context.Request.Path, requestInfo.ToString(), traceId);

                await _next(context);
            }
            catch (Exception ex)
            {
                _logError($"Exception in Invoke: {ex.Message}", traceId);
                throw;
            }
            finally
            {
                await PostSendOperation(context, startTime, traceId);
            }
        }

        private async Task PostSendOperation(HttpContext context, DateTime startTime, EventTraceActivity traceId)
        {
            try
            {
                await Task.Run(() =>
                {
                    var sb = new StringBuilder(1000);
                    sb.AppendLine("Response details:");
                    sb.AppendLine("Status Code: " + context.Response.StatusCode);

                    if (context.Response.Headers != null)
                    {
                        foreach (var header in context.Response.Headers)
                        {
                            sb.AppendLine($"{header.Key}: {string.Join(",", header.Value)}");
                        }
                    }

                    _logResponse(sb.ToString(), traceId);
                });
            }
            catch (Exception ex)
            {
                await Task.Run(() =>
                {
                    _logError($"Exception in PostSendOperation: {ex.Message}", traceId);
                });
            }
        }
    }
}
