// SelfHostedBootstrap.cs (new)
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

internal static class SelfHostedBootstrap
{
    public static void ConfigureServices(IServiceCollection services, bool useSelfHostedDependencies, bool useArrangedResponses)
    {
        // Mirror your HostableService services:
        // - AddControllers().AddNewtonsoftJson(opts => opts.SerializerSettings.NullValueHandling = NullValueHandling.Ignore);
        // - add VersionedControllerSelector + registrations
        // - register PXServiceSettings and dependency emulator accessors
        // - any singletons used by middlewares
    }

    public static void ConfigurePipeline(IApplicationBuilder app)
    {
        // Mirror your HostableService pipeline order exactly:
        // - UseRouting();
        // - UsePXTraceCorrelationMiddleware (if enabled)
        // - UsePXServiceApiVersionHandler (middleware version)
        // - UseCors (if enabled)
        // - UseEndpoints + your versioned routes
        // - /routes helper endpoint (optional)
    }
}
