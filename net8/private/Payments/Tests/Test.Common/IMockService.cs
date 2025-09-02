namespace Test.Common
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;

    public interface IMockService
    {
        List<string> Requests { get; }

        List<ConditionalResponse> Responses { get; }

        Action<HttpRequestMessage> PreProcess { get; set; }
        
        void ArrangeResponse(
            string content,
            HttpStatusCode statusCode = HttpStatusCode.OK,
            HttpMethod method = null,
            string urlPattern = null);
    }
}