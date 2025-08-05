namespace SelfHostedPXServiceCore
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;

    public class HostableService : IDisposable
    {
        public static List<int> PreRegisteredPorts { get; } = new();

        public string Port { get; private set; }

        public Uri BaseUri { get; private set; }

        public WebApplication WebApp { get; private set; }

        public HttpClient HttpSelfHttpClient { get; set; }

        public HostableService(Action<WebApplication> registerConfig, string fullBaseUrl, string protocol, Action<WebApplicationBuilder>? configureBuilder = null)
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();
            builder.Services.AddControllers().AddNewtonsoftJson();
            configureBuilder?.Invoke(builder);

            WebApp = builder.Build();

            registerConfig?.Invoke(WebApp);

            WebApp.Start();

            BaseUri = string.IsNullOrEmpty(fullBaseUrl)
                ? new Uri($"{(string.IsNullOrEmpty(protocol) ? "http" : protocol)}://localhost")
                : new Uri(fullBaseUrl);
            Port = BaseUri.Port.ToString();

            HttpSelfHttpClient = WebApp.GetTestClient();
            HttpSelfHttpClient.BaseAddress = BaseUri;
        }

        public void Dispose()
        {
            WebApp.Dispose();
        }
    }
}
