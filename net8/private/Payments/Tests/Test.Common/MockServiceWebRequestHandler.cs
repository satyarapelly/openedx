using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Test.Common;

public class MockServiceWebRequestHandler : HttpClientHandler
{
    private readonly IMockResponseProvider responseProvider;
    private readonly bool useArrangedResponses;
    private readonly List<ArrangedResponse> responses = new List<ArrangedResponse>();

    public Action<HttpRequestMessage>? PreProcess { get; set; }
    public Func<HttpResponseMessage, Task>? PostProcess { get; set; }

    public IList<ArrangedResponse> Responses => responses;

    public MockServiceWebRequestHandler(IMockResponseProvider responseProvider, bool useArrangedResponses)
    {
        this.responseProvider = responseProvider;
        this.useArrangedResponses = useArrangedResponses;
    }

    public virtual void ResetToDefaults()
    {
        responses.Clear();
        responseProvider.ResetDefaults();
    }

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
        PreProcess?.Invoke(request);

        HttpResponseMessage response;

        if (useArrangedResponses)
        {
            var manual = responses.Find(r => r.IsMatch(request));
            if (manual != null)
            {
                response = manual.Response;
                goto Post;
            }

            var arrangedResponse = await responseProvider.GetMatchedMockResponse(request);
            if (arrangedResponse != null)
            {
                response = arrangedResponse;
                goto Post;
            }
        }

        response = await base.SendAsync(request, cancellationToken);

Post:
        if (PostProcess != null)
        {
            await PostProcess.Invoke(response);
        }

        return response;
    }

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
                return false;

            if (!string.IsNullOrEmpty(UrlPattern) && !Regex.IsMatch(request.RequestUri.ToString(), UrlPattern))
                return false;

            return true;
        }
    }
}
