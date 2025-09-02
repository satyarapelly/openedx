using System.Diagnostics;
using System.Threading.Tasks;
using Marketplace.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ThirdParty.Model;

namespace Marketplace.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMarketplaceDataService _marketplaceDb;

        public HomeController(
            ILogger<HomeController> logger,
            IMarketplaceDataService marketplaceDb)
        {
            _logger = logger;
            _marketplaceDb = marketplaceDb;
        }

        public async Task<IActionResult> Index()
        {
            var catalogItems = await _marketplaceDb.ListCatalogItemsAsync();
            return View(catalogItems);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
