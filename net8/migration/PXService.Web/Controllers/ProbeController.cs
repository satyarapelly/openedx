// <copyright file="ProbeController.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Commerce.Payments.PXService.Controllers
{
    /// <summary>
    /// Probe Controller for health status
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProbeController : ControllerBase
    {
        private const string BuildVersionKey = "BuildVersion";
        private readonly IConfiguration configuration;

        public ProbeController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Gets a probe
        /// </summary>
        /// <returns>Probe status</returns>
        [HttpGet]
        public ActionResult<ServiceStatus> Get()
        {
            string buildVersion = this.configuration[BuildVersionKey] ?? "unknown";
            return Ok(new ServiceStatus { Status = "Alive", BuildVersion = buildVersion });
        }
    }
}