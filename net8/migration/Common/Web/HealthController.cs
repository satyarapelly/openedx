// <copyright file="HealthController.cs" company="Microsoft">Copyright (c) Microsoft 2014. All rights reserved.</copyright>

using Microsoft.AspNetCore.Mvc;
namespace Microsoft.Commerce.Payments.Common.Web
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet("probe")]
        public IActionResult Probe()
        {
            return Ok();
        }
    }
}
