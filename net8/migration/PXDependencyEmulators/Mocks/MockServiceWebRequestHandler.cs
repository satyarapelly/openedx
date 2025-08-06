// <copyright file="MockServiceWebRequestHandler.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Generic HTTP handler used by the self-hosted PX service to mock dependent services.
    /// It delegates to an <see cref="IMockResponseProvider"/> for arranged responses and
    /// falls back to the default <see cref="HttpClientHandler"/> when no match is found.
    /// </summary>
    public class MockServiceWebRequestHandler : HttpClientHandler
    {
        private readonly IMockResponseProvider responseProvider;
        private readonly bool useArrangedResponses;

        private readonly List<ArrangedResponse> responses = new List<ArrangedResponse>();

        public MockServiceWebRequestHandler(IMockResponseProvider responseProvider, bool useArrangedResponses)
        {
            this.responseProvider = responseProvider;
            this.useArrangedResponses = useArrangedResponses;
        }

        /// <summary>
        /// Gets the list of arranged responses for tests. Tests may clear this collection
        /// to reset any previously configured responses.
        /// </summary>
        public IList<ArrangedResponse> Responses => responses;

        /// <summary>
        /// Clears any arranged responses and resets the underlying response provider.
        /// </summary>
        public virtual void ResetToDefaults()
        {
            responses.Clear();
            responseProvider.ResetDefaults();
        }

        /// <summary>
        /// Adds a custom response that will be returned when a request matches the
        /// optional HTTP method and URL pattern.
        /// </summary>
        /// <param name="content">Body of the mock response.</param>
        /// <param name="statusCode">HTTP status code to return.</param>
        /// <param name="method">Optional HTTP method to match.</param>
        /// <param name="urlPattern">Optional regular expression to match the request URL.</param>
        public void ArrangeResponse(string content, HttpStatusCode statusCode = HttpStatusCode.OK, HttpMethod? method = null, string? urlPattern = null)
        {
            var httpResponse = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content ?? string.Empty, Encoding.UTF8, "application/json"),
            };

            responses.Add(new ArrangedResponse(urlPattern, method, httpResponse));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (useArrangedResponses)
            {
                var manual = responses.Find(r => r.IsMatch(request));
                if (manual != null)
                {
                    return manual.Response;
                }

                var arrangedResponse = await responseProvider.GetMatchedMockResponse(request);
                if (arrangedResponse != null)
                {
                    return arrangedResponse;
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// Container for an arranged response.
        /// </summary>
        public class ArrangedResponse
        {
            public ArrangedResponse(string? urlPattern, HttpMethod? method, HttpResponseMessage response)
            {
                UrlPattern = urlPattern;
                Method = method;
                Response = response;
            }

            public string? UrlPattern { get; }

            public HttpMethod? Method { get; }

            public HttpResponseMessage Response { get; }

            public bool IsMatch(HttpRequestMessage request)
            {
                if (Method != null && request.Method != Method)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(UrlPattern) && !Regex.IsMatch(request.RequestUri.ToString(), UrlPattern))
                {
                    return false;
                }

                return true;
            }
        }
    }
}
