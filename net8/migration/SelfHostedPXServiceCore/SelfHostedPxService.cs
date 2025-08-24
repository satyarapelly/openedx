// In SelfHostedPxService.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
// …the rest of your usings

public sealed class SelfHostedPxService : IDisposable
{
    // Expose the HttpClient used by Program.cs
    public HttpClient HttpClient { get; private set; }

    // --- Option B: in-memory (TestServer) bootstrap ---
    public static SelfHostedPxService StartInMemory(bool useSelfHostedDependencies, bool useArrangedResponses)
    {
        // Build all PX DI/config exactly as you already do in your HostableService/WebApiConfig.ConfigureRoutes
        // but wire it into a HostBuilder with UseTestServer().
        var px = new SelfHostedPxService(); // private ctor
        px._host = new HostBuilder()
            .ConfigureWebHost(web =>
            {
                web.UseTestServer();
                web.ConfigureServices(services =>
                {
                    // Reuse your registration from HostableService / WebApiConfig:
                    // - controllers
                    // - PXServiceSettings + dependency emulators
                    // - VersionedControllerSelector + ApiVersion handler
                    // - any middlewares you already migrated (trace, CORS, input validation, PIDL validation)
                    SelfHostedBootstrap.ConfigureServices(services, useSelfHostedDependencies, useArrangedResponses);
                });
                web.Configure(app =>
                {
                    SelfHostedBootstrap.ConfigurePipeline(app);
                });
            })
            .Build();

        px._host.Start();
        px.HttpClient = px._host.GetTestClient();
        return px;
    }

    private IHost _host;

    private SelfHostedPxService() { /* private for the in-memory factory */ }

    public void Dispose()
    {
        HttpClient?.Dispose();
        _host?.Dispose();
    }
}
