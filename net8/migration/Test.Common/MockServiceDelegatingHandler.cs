namespace Test.Common
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class MockServiceDelegatingHandler : DelegatingHandler, IMockService
    {
        public List<string> Requests => this.mockServiceHandler.Requests;

        public List<ConditionalResponse> Responses => this.mockServiceHandler.Responses;

        public Action<HttpRequestMessage> PreProcess
        {
            get
            {
                return this.mockServiceHandler.PreProcess;
            }

            set
            {
                this.mockServiceHandler.PreProcess = value;
            }
        }

        private MockServiceHandler mockServiceHandler;
        
        public MockServiceDelegatingHandler(IMockResponseProvider mockResponseProvider, bool useArrangedResponses)
        {
            mockServiceHandler = new MockServiceHandler(mockResponseProvider, useArrangedResponses);
        }
        
        public void ArrangeResponse(
            string content,
            HttpStatusCode statusCode = HttpStatusCode.OK,
            HttpMethod method = null,
            string urlPattern = null)
        {
            this.mockServiceHandler.ArrangeResponse(content, statusCode, method, urlPattern);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await this.mockServiceHandler.SendAsync(request, cancellationToken, (message, token) => base.SendAsync(message, token));
        }
    }
}