using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Commerce.Payments.PXService.Settings;
using Newtonsoft.Json;

namespace Microsoft.Commerce.Payments.PXService
{
    /// <summary>
    /// Simplified Web API configuration for ASP.NET Core.
    /// </summary>
    public static class WebApiConfig
    {
        public static readonly Type PXSettingsType = typeof(PXServiceSettings);

        /// <summary>
        /// Registers services required for the PX service.
        /// </summary>
        /// <param name="builder">The web application builder.</param>
        /// <param name="settings">PX service settings instance.</param>
        public static void Register(WebApplicationBuilder builder, PXServiceSettings settings)
        {
            builder.Services.AddSingleton(settings);
            builder.Services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });
        }
    }
}
