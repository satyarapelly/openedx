// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Commerce.Payments.PXCommon
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Commerce.Tracing;

    /// <summary>
    /// Simple HTTP client that sends requests and captures responses for tracing.
    /// </summary>
    public class PXTracingHttpClient
    {
        private readonly IDictionary<string, string> defaultHeaders;
        private readonly System.Net.Http.HttpClient httpClient;

        public PXTracingHttpClient(string serviceName, IDictionary<string, string>? defaultHeaders = null, System.Net.Http.HttpClient? client = null)
        {
            this.ServiceName = serviceName;
            this.defaultHeaders = defaultHeaders ?? new Dictionary<string, string>();
            this.httpClient = client ?? new System.Net.Http.HttpClient();
        }

        public string ServiceName { get; }

        public async Task<PXHttpResponse> SendAsync(
            string method,
            string url,
            EventTraceActivity traceActivityId,
            string actionName,
            IDictionary<string, string>? headers = null,
            string? content = null)
        {
            var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod(method), url);

            foreach (var header in this.defaultHeaders)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            if (!string.IsNullOrEmpty(content))
            {
                request.Content = new System.Net.Http.StringContent(content);
            }

            using var response = await this.httpClient.SendAsync(request);
            var responseContent = response.Content != null ? await response.Content.ReadAsStringAsync() : string.Empty;
            return new PXHttpResponse((int)response.StatusCode, responseContent);
        }
    }

    public record PXHttpResponse(int StatusCode, string Content)
    {
        public bool IsSuccessStatusCode => StatusCode >= 200 && StatusCode <= 299;
    }
}
