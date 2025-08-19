namespace SelfHostedPXServiceCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.NetworkInformation;
    using Castle.Components.DictionaryAdapter;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class HostableService : IDisposable
    {
        public static List<int> PreRegisteredPorts { get; private set; }

        public string Port { get; private set; }

        public Uri BaseUri { get; private set; }

        public WebApplication SelfHostServer { get; private set; }

        public HttpClient HttpSelfHttpClient { get; private set; }

        static HostableService()
        {
            PreRegisteredPorts = new EditableList<int>();
        }

        public HostableService(Action<WebApplicationBuilder> registerConfig, string? fullBaseUrl, string? protocol)
        {
            if (string.IsNullOrEmpty(fullBaseUrl))
            {
                Port = GetAvailablePort();

                if (string.IsNullOrEmpty(protocol))
                {
                    protocol = "https";
                }

                BaseUri = new Uri(string.Format("{0}://localhost:{1}", protocol, Port));
            }
            else
            {
                BaseUri = new Uri(fullBaseUrl);
            }

            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseUrls(BaseUri.AbsoluteUri);
            builder.Services.AddControllers();
            registerConfig(builder);

            SelfHostServer = builder.Build();
            SelfHostServer.MapControllers();
            SelfHostServer.StartAsync().Wait();

            HttpSelfHttpClient = new HttpClient
            {
                BaseAddress = BaseUri,
            };
        }

        public void Dispose()
        {
            SelfHostServer.StopAsync().Wait();
            SelfHostServer.Dispose();
        }

        private static string GetAvailablePort()
        {
            var netProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpListeners = netProperties.GetActiveTcpListeners();
            var udpListeners = netProperties.GetActiveUdpListeners();

            var portsInUse = new List<int>();
            portsInUse.AddRange(tcpListeners.Select(tl => tl.Port));
            portsInUse.AddRange(udpListeners.Select(ul => ul.Port));

            int firstAvailablePort = 0;
            for (int port = 49152; port < 65535; port++)
            {
                if (!portsInUse.Contains(port) && !PreRegisteredPorts.Contains(port))
                {
                    firstAvailablePort = port;
                    break;
                }
            }

            return firstAvailablePort.ToString();
        }
    }
}