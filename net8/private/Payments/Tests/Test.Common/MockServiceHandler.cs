namespace Test.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.PXCommon;

    public class MockServiceHandler : IMockService
    {
        public List<string> Requests { get; }

        public List<ConditionalResponse> Responses { get; }

        public Action<HttpRequestMessage> PreProcess { get; set; }

        public Func<HttpResponseMessage, Task> PostProcess { get; set; }

        private readonly IMockResponseProvider mockResponseProvider;

        private readonly bool useArrangedResponses;

        public MockServiceHandler(IMockResponseProvider mockResponseProvider, bool useArrangedResponses)
        {
            this.mockResponseProvider = mockResponseProvider;
            this.useArrangedResponses = useArrangedResponses;
            Requests = new List<string>();
            Responses = new List<ConditionalResponse>();
        }
        
        public void ArrangeResponse(
            string content,
            HttpStatusCode statusCode = HttpStatusCode.OK,
            HttpMethod method = null,
            string urlPattern = null)
        {
            Responses.Add(new ConditionalResponse()
            {
                Method = method,
                UrlPattern = urlPattern,
                Content = content,
                StatusCode = statusCode
            });
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken, Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseResolver)
        {
            // When running selfhosted DiffTest or PXService CITs,
            // let the handler find a mock, but not when running PXService COTs
            if (!WebHostingUtility.IsApplicationSelfHosted())
            {
                return await responseResolver(request, cancellationToken);
            }

            var response = await this.GetArrangedResponse(request);
            if (response != null)
            {
                return response;
            }

            response = await responseResolver(request, cancellationToken);
            
            return response;
        }

        public void ResetToDefaults()
        {
            Requests.Clear();
            Responses.Clear();
            PreProcess = null;
            PostProcess = null;
            this.mockResponseProvider.ResetDefaults();
        }
        
        private async Task<HttpResponseMessage> GetArrangedResponse(HttpRequestMessage request)
        {
            if (PreProcess != null)
            {
                PreProcess(request);
            }

            var mock = await this.GetResponse(request);
            if (mock != null)
            {
                return mock;
            }

            mock = await this.mockResponseProvider.GetMatchedMockResponse(request);

            if (PostProcess != null)
            {
                await PostProcess(mock);
            }

            if (mock != null)
            {
                return mock;
            }

            return null;
        }

        private async Task<HttpResponseMessage> GetResponse(HttpRequestMessage request)
        {
            if (!this.useArrangedResponses)
            {
                return null;
            }

            Requests?.Add(string.Format("{0} {1}", request.Method, request.RequestUri));

            var foundMatch = Responses.FirstOrDefault(resp => resp.IsMatch(request));

            if (foundMatch != null)
            {
                return await Task.FromResult(new HttpResponseMessage(foundMatch.StatusCode)
                {
                    Content = new StringContent(
                            content: foundMatch.Content,
                            encoding: System.Text.Encoding.UTF8,
                            mediaType: "application/json")
                });
            }
            else
            {
                return await Task.FromResult<HttpResponseMessage>(null);
            }
        }
    }
}