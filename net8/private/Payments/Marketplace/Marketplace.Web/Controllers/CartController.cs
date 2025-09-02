using System;
using System.Linq;
using System.Threading.Tasks;
using Marketplace.Data;
using Microsoft.AspNetCore.Mvc;
using ThirdParty.Model;

namespace Marketplace.Controllers
{
    public class CartController : Controller
    {
        private IMarketplaceDataService _marketplaceDb;
        public CartController(IMarketplaceDataService marketplaceDb)
        {
            _marketplaceDb = marketplaceDb;
        }

        public async Task<ActionResult> Index()
        {
            var cart = await _marketplaceDb.GetOrderInCartAsync();
            return View(cart);
        }

        // Cart/AddItem/<CatalogItemId>/<skuId>
        public async Task<ActionResult> AddItem(string id, string skuId)
        {
            var catalogItem = await _marketplaceDb.GetCatalogItemByIdAsync(id);
            var sku = catalogItem.Skus
                .Where(
                    s => string.Equals(s.Id, skuId, StringComparison.OrdinalIgnoreCase))
                .First();

            bool cartIsEmpty = false;
            var cart = await _marketplaceDb.GetOrderInCartAsync();
            if (cart == null)
            {
                cartIsEmpty = true;
                cart = new Order()
                {
                    Id = Guid.NewGuid().ToString(),
                    State = OrderState.InCart
                };
            }

            cart.Items.Add(
                new OrderLineItem()
                {
                    Product = catalogItem.Product,
                    Sku = sku,
                    Price = sku.Price,
                    State = FulfilmentState.Pending
                }
            );

            cart = cartIsEmpty ?
                await _marketplaceDb.AddOrderAsync(cart) :
                await _marketplaceDb.UpdateOrderAsync(cart);

            return View("Index", cart);
        }

        public ActionResult Error()
        {
            return View();
        }
   }
}
