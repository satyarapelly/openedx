// <copyright file="GenericHttpClient.cs" company="Microsoft Corporation">Copyright (c) Microsoft 2015. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PidlFactory.V7
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Newtonsoft.Json;

    internal static class GenericHttpClient
    {
        private static readonly HttpClient httpClient = new HttpClient(new HttpClientHandler
        {
            CheckCertificateRevocationList = true
        });

        public static async Task<HttpResponseMessage> SendAndReceiveAsync(
            string httpMethod,
            string fullUrl,
            EventTraceActivity traceActivityId,
            IList<KeyValuePair<string, string>> additionalHeaders,
            string requestPayload,
            Action<string> setResponsePayload)
        {
            using var request = new HttpRequestMessage(new HttpMethod(httpMethod), fullUrl);

            // Set headers
            AddHeaders(request, additionalHeaders);

            // Set content
            if (string.IsNullOrEmpty(requestPayload))
            {
                request.Content = new StringContent(string.Empty);
                request.Content.Headers.ContentLength = 0;
            }
            else
            {
                request.Content = new StringContent(requestPayload);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json"); // Adjust as needed
            }

            // Send and read response
            var response = await httpClient.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            setResponsePayload?.Invoke(responseContent);

            return response;
        }

        private static void AddHeaders(HttpRequestMessage request, IList<KeyValuePair<string, string>> headers)
        {
            if (headers == null) return;

            foreach (var header in headers)
            {
                if (string.Equals(header.Key, "Accept", StringComparison.OrdinalIgnoreCase))
                {
                    request.Headers.Accept.Clear();
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(header.Value));
                }
                else if (string.Equals(header.Key, "Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    // Handled separately when setting HttpContent
                }
                else
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }
    }
}
