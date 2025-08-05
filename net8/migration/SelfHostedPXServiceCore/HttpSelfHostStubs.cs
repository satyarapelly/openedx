namespace System.Web.Http.SelfHost
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Minimal stub of HttpSelfHostConfiguration for .NET Core compatibility.
    /// Supports registering delegating handlers and storing the base address.
    /// </summary>
    public class HttpSelfHostConfiguration
    {
        public HttpSelfHostConfiguration(string baseAddress)
        {
            BaseAddress = baseAddress;
        }

        /// <summary>
        /// Gets the base address for the self hosted service.
        /// </summary>
        public string BaseAddress { get; }

        /// <summary>
        /// Collection of message handlers to apply to the HttpClient pipeline.
        /// </summary>
        public IList<DelegatingHandler> MessageHandlers { get; } = new List<DelegatingHandler>();
    }

    /// <summary>
    /// Minimal stub of HttpSelfHostServer acting as an <see cref="HttpMessageHandler"/>.
    /// The server does not perform any request processing; it simply returns 404 responses.
    /// </summary>
    public class HttpSelfHostServer : HttpMessageHandler
    {
        public HttpSelfHostServer(HttpSelfHostConfiguration configuration)
        {
            Configuration = configuration;
        }

        public HttpSelfHostConfiguration Configuration { get; }

        public Task OpenAsync() => Task.CompletedTask;

        public Task CloseAsync() => Task.CompletedTask;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);
            return Task.FromResult(response);
        }
    }
}
