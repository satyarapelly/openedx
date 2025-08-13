// PXTracingHandler.cs - Updated for .NET 8.0

namespace Microsoft.Commerce.Payments.PXCommon
{
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Tracing;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class PXTracingHandler : DelegatingHandler
    {
        public PXTracingHandler(
            string serviceName,
            Action<string, EventTraceActivity>? logError = null,
            Action<string, string, EventTraceActivity>? logRequest = null,
            Action<string, EventTraceActivity>? logResponse = null)
            : base()
        {
            this.ServiceName = serviceName;
            this.LogError = logError ?? ((m, t) => { });
            this.LogRequest = logRequest ?? ((a, m, t) => { });
            this.LogResponse = logResponse ?? ((m, t) => { });
        }

        public PXTracingHandler(
            string serviceName,
            HttpMessageHandler httpMessageHandler,
            Action<string, EventTraceActivity>? logError = null,
            Action<string, string, EventTraceActivity>? logRequest = null,
            Action<string, EventTraceActivity>? logResponse = null)
            : base(httpMessageHandler)
        {
            this.ServiceName = serviceName;
            this.LogError = logError ?? ((m, t) => { });
            this.LogRequest = logRequest ?? ((a, m, t) => { });
            this.LogResponse = logResponse ?? ((m, t) => { });
        }

        public string ServiceName { get; set; }
        public Action<string, string, EventTraceActivity> LogRequest { get; set; }
        public Action<string, EventTraceActivity> LogResponse { get; set; }
        public Action<string, EventTraceActivity> LogError { get; set; }

        protected override sealed async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            DateTime startTime = DateTime.UtcNow;
            EventTraceActivity serverTraceActivity = request.GetRequestCorrelationId();

            using (new TraceCorrelationScope(serverTraceActivity))
            {
                await PreSendOperationAsync(request, serverTraceActivity);
                HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
                await PostSendOperationAsync(request, response, startTime, serverTraceActivity);

                Debug.Assert(
                    serverTraceActivity.ActivityId.Equals(EventTraceActivity.Current.ActivityId),
                    string.Format(CultureInfo.InvariantCulture,
                        "EventTraceActivity mismatch. Expected: '{0}', Found: '{1}'",
                        serverTraceActivity.ActivityId,
                        EventTraceActivity.Current.ActivityId));

                return response;
            }
        }

        private async Task PreSendOperationAsync(HttpRequestMessage request, EventTraceActivity traceId)
        {
            try
            {
                string requestPayload = await request.GetRequestPayload();
                string message = $"Url:{request.RequestUri}, Payload:{requestPayload}";
                this.LogRequest(request.GetOperationName(), message, traceId);
            }
            catch (Exception ex)
            {
                this.LogError($"Exception in PreSendOperation: {ex.Message}", traceId);
            }
        }

        private async Task PostSendOperationAsync(HttpRequestMessage request, HttpResponseMessage response, DateTime startTime, EventTraceActivity traceId)
        {
            try
            {
                StringBuilder sb = new();
                sb.AppendLine("Response details:");
                sb.AppendLine($"Status Code: {response.StatusCode}");

                if (response.Content?.Headers != null)
                {
                    foreach (var header in response.Content.Headers)
                    {
                        sb.AppendLine($"{header.Key}: {header.GetSanitizeValueForLogging()}");
                    }

                    if (response.Content.Headers.ContentLength != 0)
                    {
                        sb.AppendLine(await response.Content.ReadAsStringAsync());
                    }
                }

                string operationName = request.GetOperationNameWithPendingOnInfo();
                string version = request.GetVersion();
                string? callerName = request.GetRequestCallerName();

                sb.AppendLine($"Caller: {callerName ?? Constants.ClientNames.Unknown}");

                this.LogResponse(sb.ToString(), traceId);
            }
            catch (Exception ex)
            {
                this.LogError($"Exception in PostSendOperation: {ex.Message}", traceId);
            }
        }
    }
}
