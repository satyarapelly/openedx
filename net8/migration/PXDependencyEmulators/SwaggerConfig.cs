// <copyright file="SwaggerConfig.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators
{
    using System.Linq;
    using System.Web;
    using System.Web.Http;
    using Swashbuckle.Application;

    public class SwaggerConfig
    {
        public static void Register()
        {
            var config = GlobalConfiguration.Configuration;

            var swaggerConfig = config.EnableSwagger(c =>
            {
                c.SingleApiVersion("v1", "PXDependencyEmulators");
                c.OperationFilter<AddHeaderParameterOperationFilter>();

                // Due to limitation of swagger, multiple endpoint can not have same route
                // As PIMS emulator endpoints conflicts we need to resolve it by picking the first one
                c.ResolveConflictingActions(apiDescriptions =>
                {
                    return apiDescriptions.First();
                });
            });

            // To Enable the swagger UI update the debug=true in below line of web.config
            // <compilation debug="true" targetFramework="4.5" />
            if (HttpContext.Current?.IsDebuggingEnabled ?? false)
            {
                swaggerConfig.EnableSwaggerUi(); // CodeQL [SM04686] Safe to use. SwaggerUI is disabled by default in all environments (dev and prod). For dev usage, a trigger (debug="true") has been implemented in the web.config file (line no. 10).
            }
        }
    }
}