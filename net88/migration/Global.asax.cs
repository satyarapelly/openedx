// <copyright file="Global.asax.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators
{
    using System.Diagnostics.CodeAnalysis;
    using System.Web.Http;

    public class PXDependencyEmulatorsWebApplication : System.Web.HttpApplication
    {
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "This needs to be instance methods for web app to work.")]
        protected void Application_Start()
        {
            WebApiConfig.Register(GlobalConfiguration.Configuration);

            var envType = Common.Environments.Environment.Current.EnvironmentType;

            if (envType == Common.Environments.EnvironmentType.OneBox)
            {
                SwaggerConfig.Register();
            }
        }
    }
}