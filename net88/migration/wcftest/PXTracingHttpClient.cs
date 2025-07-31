using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Commerce.Payments.Common.Tracing;
using Microsoft.Commerce.Payments.Common.Web;

namespace Microsoft.Commerce.Payments.PXCommon
{
    /// <summary>
    /// A lightweight tracing-enabled HttpClient wrapper for outgoing requests.
    /// </summary>
    public class PXTracingHttpClient
    {
        private readonly HttpClient _client;
        private readonly string _serviceName;
        private readonly Action<string, EventTraceActivity> _logError;
        private readonly Action<string, string, EventTraceActivity> _logRequest;
        private readonly Action<string, EventTraceActivity> _logResponse;

        public PXTracingHttpClient(
            string serviceName,
            HttpClient httpClient,
            Action<string, EventTraceActivity> logError = null,
            Action<string, string, EventTraceActivity> logRequest = null,
            Action<string, EventTraceActivity> logResponse = null)
        {
            _client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _serviceName = serviceName;
            _logError = logError ?? ((m, t) => { });
            _logRequest = logRequest ?? ((a, m, t) => { });
            _logResponse = logResponse ?? ((m, t) => { });
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            var traceActivity = EventTraceActivity.Current ?? new EventTraceActivity();

            try
            {
                string payload = request.Content != null ? await request.Content.ReadAsStringAsync() : "<no body>";
                _logRequest?.Invoke(request.RequestUri.ToString(), payload, traceActivity);

                HttpResponseMessage response = await _client.SendAsync(request);
                string responseContent = response.Content != null ? await response.Content.ReadAsStringAsync() : "<no response>";
                _logResponse?.Invoke(responseContent, traceActivity);

                return response;
            }
            catch (Exception ex)
            {
                _logError?.Invoke($"Exception in tracing HTTP call: {ex.Message}", traceActivity);
                throw;
            }
        }
    }
}
