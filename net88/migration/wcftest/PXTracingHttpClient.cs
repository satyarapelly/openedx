// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.Commerce.Payments.PXCommon
{
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Tracing;

    /// <summary>
    /// Simple HTTP client that sends requests and captures responses for tracing.
    /// </summary>
    public class PXTracingHttpClient
    {
        private readonly IDictionary<string, string> defaultHeaders;

        public PXTracingHttpClient(string serviceName, IDictionary<string, string>? defaultHeaders = null)
        {
            this.ServiceName = serviceName;
            this.defaultHeaders = defaultHeaders ?? new Dictionary<string, string>();
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
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;

            foreach (var header in this.defaultHeaders)
            {
                request.Headers[header.Key] = header.Value;
            }

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers[header.Key] = header.Value;
                }
            }

            if (!string.IsNullOrEmpty(content))
            {
                using var writer = new StreamWriter(await request.GetRequestStreamAsync());
                await writer.WriteAsync(content);
            }

            try
            {
                using var response = (HttpWebResponse)await request.GetResponseAsync();
                using var reader = new StreamReader(response.GetResponseStream());
                var body = await reader.ReadToEndAsync();
                return new PXHttpResponse(response.StatusCode, body);
            }
            catch (WebException ex) when (ex.Response is HttpWebResponse errorResponse)
            {
                using var reader = new StreamReader(errorResponse.GetResponseStream());
                var body = await reader.ReadToEndAsync();
                return new PXHttpResponse(errorResponse.StatusCode, body);
            }
        }
    }

    public record PXHttpResponse(HttpStatusCode StatusCode, string Content)
    {
        public bool IsSuccessStatusCode => (int)StatusCode >= 200 && (int)StatusCode <= 299;
    }
}
