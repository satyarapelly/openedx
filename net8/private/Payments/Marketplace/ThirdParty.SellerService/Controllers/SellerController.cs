using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ThirdParty.Model;

namespace ThirdParty.SellerService.Controllers
{
    [ApiController]
    public class SellerController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<SellerController> _logger;

        public SellerController(ILogger<SellerController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        // Using attribute routing per
        // https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-6.0
        [Route("sellers/{id}")]
        public ActionResult<Seller> Get(string id)
        {
            string someYogaId = "1EBE42F4-5029-42C5-BE77-30BBB3223234";
            if (string.Equals(id, someYogaId, StringComparison.OrdinalIgnoreCase))
            {
                return new Seller
                {
                    Id = someYogaId,
                    Name = "Some Yoga LLC",
                };
            }
            else
            {
                return NotFound();
            }
        }
    }
}
