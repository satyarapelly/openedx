using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Commerce.Payments.PXCommon
{
    using Common.Tracing;
    using EventTraceActivity = Common.Tracing.EventTraceActivity;

    /// <summary>
    /// DelegatingHandler that logs HTTP request/response information.
    /// Adapted for .NET 8.
    /// </summary>
    public class PXTracingHandler : DelegatingHandler
    {
        public PXTracingHandler(
            string serviceName,
            Action<string, EventTraceActivity>? logError = null,
            Action<string, string, EventTraceActivity>? logRequest = null,
            Action<string, EventTraceActivity>? logResponse = null)
            : base()
        {
            ServiceName = serviceName;
            LogError = logError ?? ((m, t) => { });
            LogRequest = logRequest ?? ((a, m, t) => { });
            LogResponse = logResponse ?? ((m, t) => { });
        }

        public PXTracingHandler(
            string serviceName,
            HttpMessageHandler httpMessageHandler,
            Action<string, EventTraceActivity>? logError = null,
            Action<string, string, EventTraceActivity>? logRequest = null,
            Action<string, EventTraceActivity>? logResponse = null)
            : base(httpMessageHandler)
        {
            ServiceName = serviceName;
            LogError = logError ?? ((m, t) => { });
            LogRequest = logRequest ?? ((a, m, t) => { });
            LogResponse = logResponse ?? ((m, t) => { });
        }

        public string ServiceName { get; set; }

        public Action<string, string, EventTraceActivity> LogRequest { get; set; }

        public Action<string, EventTraceActivity> LogResponse { get; set; }

        public Action<string, EventTraceActivity> LogError { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var startTime = DateTime.UtcNow;
            var serverTraceActivity = request.GetRequestCorrelationId();

            using (new TraceCorrelationScope(serverTraceActivity))
            {
                await PreSendOperationAsync(request, serverTraceActivity);
                var response = await base.SendAsync(request, cancellationToken);
                await PostSendOperationAsync(request, response, startTime, serverTraceActivity);

                var serverTraceActivity2 = EventTraceActivity.Current;
                Debug.Assert(
                    serverTraceActivity.ActivityId.Equals(serverTraceActivity2.ActivityId),
                    string.Format(CultureInfo.InvariantCulture, "EventTraceActivity mismatch. Expected: '{0}', Found: '{1}'", serverTraceActivity.ActivityId, serverTraceActivity2.ActivityId));

                return response;
            }
        }

        private async Task PreSendOperationAsync(HttpRequestMessage request, EventTraceActivity traceId)
        {
            try
            {
                string requestPayload = await request.GetRequestPayload();
                LogRequest(request.GetOperationName(), $"Url:{request.RequestUri}, Payload:{requestPayload}", traceId);
            }
            catch (Exception ex)
            {
                LogError($"Exception happened in presendaction. {ex.Message}", traceId);
            }
        }

        private async Task PostSendOperationAsync(HttpRequestMessage request, HttpResponseMessage response, DateTime startTime, EventTraceActivity traceId)
        {
            try
            {
                StringBuilder sb = new StringBuilder(1000);
                sb.AppendLine("Response details:");
                sb.AppendLine("Status Code: " + response.StatusCode);

                if (response.Content != null)
                {
                    if (response.Content.Headers != null)
                    {
                        foreach (var header in response.Content.Headers)
                        {
                            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", header.Key, header.GetSanitizeValueForLogging()));
                        }

                        if (response.Content.Headers.ContentLength != 0)
                        {
                            sb.AppendLine(await response.Content.ReadAsStringAsync());
                        }
                    }
                }

                string operationName = request.GetOperationNameWithPendingOnInfo();
                string version = request.GetVersion();

                if (request.Properties != null)
                {
                    string callerName = request.GetRequestCallerName();
                    sb.AppendFormat("Caller: {0}", callerName ?? Constants.ClientNames.Unknown);
                }

                LogResponse(sb.ToString(), traceId);
            }
            catch (Exception ex)
            {
                LogError($"Exception happened in postsendaction. {ex.Message}", traceId);
            }
        }
    }

    // --- Minimal stubs for missing external dependencies ---
    namespace Common.Tracing
    {
        public sealed class EventTraceActivity
        {
            public Guid ActivityId { get; } = Guid.NewGuid();
            public static EventTraceActivity Current { get; } = new();
        }

        public sealed class TraceCorrelationScope : IDisposable
        {
            public TraceCorrelationScope(EventTraceActivity activity) { }
            public void Dispose() { }
        }
    }

    public static class HttpRequestMessageExtensions
    {
        public static Task<string> GetRequestPayload(this HttpRequestMessage message) => Task.FromResult(string.Empty);
        public static EventTraceActivity GetRequestCorrelationId(this HttpRequestMessage message) => new();
        public static string GetOperationName(this HttpRequestMessage message) => string.Empty;
        public static string GetOperationNameWithPendingOnInfo(this HttpRequestMessage message) => string.Empty;
        public static string GetVersion(this HttpRequestMessage message) => string.Empty;
        public static string GetRequestCallerName(this HttpRequestMessage message) => string.Empty;
    }

    public static class HttpHeadersExtensions
    {
        public static string GetSanitizeValueForLogging(this System.Net.Http.Headers.HttpHeaders headers) => string.Empty;
        public static string GetSanitizeValueForLogging(this KeyValuePair<string, IEnumerable<string>> header) => string.Join(",", header.Value);
    }

    public static class Constants
    {
        public static class ClientNames
        {
            public const string Unknown = nameof(Unknown);
        }
    }
}

