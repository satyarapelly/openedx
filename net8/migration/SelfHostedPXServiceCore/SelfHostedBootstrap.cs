using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Commerce.Payments.PXService;
using Microsoft.Commerce.Payments.PXService.Settings;

namespace SelfHostedPXServiceCore
{
    /// <summary>
    /// Central bootstrap for configuring the in-memory PX service so that
    /// endpoint routing is wired the same way as the full self-host.
    /// </summary>
    internal static class SelfHostedBootstrap
    {
        /// <summary>
        /// Register PX controllers and supporting services.
        /// </summary>
        public static void ConfigureServices(
            WebApplicationBuilder builder,
            bool useSelfHostedDependencies,
            bool useArrangedResponses)
        {
            PXServiceSettings settings = new Mocks.PXServiceSettings(
                useSelfHostedDependencies ? new Dictionary<Type, HostableService>() : null,
                useArrangedResponses);

            WebApiConfig.Register(builder, settings);
        }

        /// <summary>
        /// Configure the middleware pipeline and map routes.
        /// </summary>
        public static void ConfigurePipeline(WebApplication app)
        {
            app.UseRouting();
            app.UseMiddleware<PXServiceApiVersionHandler>();

            WebApiConfig.AddUrlVersionedRoutes(app);
        }
    }
}

