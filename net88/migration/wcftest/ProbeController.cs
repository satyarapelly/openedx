// <copyright file="ProbeController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.Http;

    /// <summary>
    /// Probe Controller for health status
    /// </summary>
    public class ProbeController : ApiController
    {
        public const string BuildVersionKey = "BuildVersion";

        /// <summary>
        /// Gets a probe
        /// </summary>
        /// <returns>Probe status</returns>
        [HttpGet]
        [SuppressMessage("Microsoft.Performance", "CA1822", Justification = "Needs to be an instance method until we update with full functionality.")]
        public ServiceStatus Get()
        {
            string buildVersion = ConfigurationManager.AppSettings[BuildVersionKey];
            return new ServiceStatus { Status = "Alive", BuildVersion = buildVersion };
        }
    }
}
