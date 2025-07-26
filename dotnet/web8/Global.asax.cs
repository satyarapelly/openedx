// <copyright file="Global.asax.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>
namespace Microsoft.Commerce.Payments.PXService
{
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Web.Http;
    using Microsoft.Commerce.Payments.Common.Environments;
    using Microsoft.Commerce.Payments.PXService.Settings;

    /// <summary>
    /// Payments instruments Web Application
    /// </summary>
    /// <remarks>Note: For instructions on enabling IIS6 or IIS7 classic mode,/// visit https://go.microsoft.com/?LinkId=9394801</remarks>
    public class PXWebApplication : System.Web.HttpApplication
    {
        private PXServiceSettings settings = null;

        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "This needs to be instance methods for web app to work.")]
        protected void Application_Start()
        {
            // Remove protocols we know to be insecure (Ssl3, Tls (Tls10), Tls11)
            ServicePointManager.SecurityProtocol &= ~(SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11); // DevSkim: ignore DS440000,DS440020,DS144436 as old protocols are being explicitly removed // lgtm[cs/hard-coded-deprecated-security-protocol] lgtm[cs/hard-coded-security-protocol] -Suppressing because of a false positive from Semmle
            if (ServicePointManager.SecurityProtocol == 0)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; // DevSkim: ignore DS440000,DS440020 as old protocols are being explicitly removed // lgtm[cs/hard-coded-security-protocol] -Suppressing because of a false positive from Semmle
            }

            this.settings = PXServiceSettings.CreateInstance(Environment.Current.EnvironmentType, Environment.Current.EnvironmentName);
            WebApiConfig.Register(GlobalConfiguration.Configuration, this.settings);
        }

        protected void Application_End()
        {
            // Stop AzureExP blob polling task
            this.settings?.AzureExPAccessor?.StopPolling();
        }
    }
}