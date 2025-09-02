using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marketplace.Data;
using Marketplace.Web.PayModClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ThirdParty.Model;

namespace Marketplace.Web.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ILogger<OrdersController> _logger;
        private readonly IMarketplaceDataService _marketplaceDb;
        private readonly IConfiguration _config;

        public OrdersController(
            ILogger<OrdersController> logger,
            IMarketplaceDataService marketplaceDb,
            IConfiguration config)
        {
            _logger = logger;
            _marketplaceDb = marketplaceDb;
            _config = config;
        }

        // GET /Orders
        public async Task<ActionResult> Index(string highlightId)
        {
            var orders = await _marketplaceDb.ListOrdersAsync();

            var ordersToShow = orders
                .Where(o =>
                    o.State == OrderState.PaymentExperienceCompleted
                    || o.State == OrderState.PaymentSucceeded
                    || o.State == OrderState.PaymentFailed
                    || o.State == OrderState.Fulfilled)
                .OrderByDescending(o => o.CheckoutDate)
                .ToList();

            ViewBag.HighlightId = highlightId;
            ViewBag.StripePublishableKey = _config.GetSection("StripePublishKey").Value;
            ViewBag.Ru = "https://localhost:44344/Orders";
            ViewBag.Rx = "https://localhost:44344/Orders";

            return View(ordersToShow);
        }

        // GET /Orders/Delete/{id}
        public async Task<ActionResult> Delete(string id)
        {
            await _marketplaceDb.DeleteOrderAsync(id);

            var orders = await _marketplaceDb.ListOrdersAsync();
            var ordersToShow = orders
                .Where(o =>
                    o.State == OrderState.PaymentExperienceCompleted
                    || o.State == OrderState.PaymentSucceeded
                    || o.State == OrderState.PaymentFailed
                    || o.State == OrderState.Fulfilled)
                .OrderByDescending(o => o.CheckoutDate)
                .ToList();

            return Redirect("https://localhost:44344/Orders");
        }

        // GET /Orders/Refresh/{id}
        public async Task<ActionResult> Refresh(string id)
        {
            var order = await _marketplaceDb.GetOrderByIdAsync(id);

            var payModClient = TransactionServiceClient.CreateInstance();
            dynamic response;

            //             var resultCode = payModClient.SendRequest(string.Format("{0}/charges/{1}/{2}", order.AccountId, order.PaymentId, order.TransactionId),
            // "GET", new TestContext("TokenCot", DateTime.Now, TestContext.EmulatorScenario), null, out response);

            var resultCode = payModClient.SendRequest(string.Format("{0}/payments/{1}", order.AccountId, order.PaymentId),
                "GET", new TestContext("TokenCot", DateTime.Now, TestContext.EmulatorScenario), null, out response);

            string s = response.transactions[0].status.ToString();

            if (string.Equals(s, "approved", StringComparison.OrdinalIgnoreCase))
            {
                var invoiceService = new Stripe.InvoiceService();
                var invoice = await invoiceService.GetAsync(order.InvoiceId);
                

                order.State = OrderState.Fulfilled;

                await _marketplaceDb.UpdateOrderAsync(order);
            }

            return Redirect("https://localhost:44344/Orders");
        }

        // GET /Orders/PayByCheck/{id}
        public async Task<ActionResult> PayByCheck(string id)
        {
            var order = await _marketplaceDb.GetOrderByIdAsync(id);

            var invoiceService = new Stripe.InvoiceService();
            var invoice = await invoiceService.GetAsync(order.InvoiceId);

            var customerService = new Stripe.CustomerService();
            var customerGetOptions = new Stripe.CustomerGetOptions();
            customerGetOptions.AddExpand("sources");
            var customer = customerService.Get(invoice.CustomerId, customerGetOptions);
            var checkSource = customer.Sources.Data.Where(i => (i as Stripe.Source).Type == "paper_check").FirstOrDefault();
            var checkSourceId = (checkSource as Stripe.Source).Id;

            // Update check source balance using this hack
            var options = new Stripe.SourceUpdateOptions
            {
                Owner = new Stripe.SourceOwnerOptions
                {
                    Email = $"amount_{invoice.AmountDue}@example.com",
                },
            };

            var sourceService = new Stripe.SourceService();
            await sourceService.UpdateAsync(checkSourceId, options);

            // Wait for Stripe to activate the check balance
            await Task.Delay(TimeSpan.FromSeconds(5));

            // Pay the invoice with check source
            await invoiceService.PayAsync(
                order.InvoiceId,
                new Stripe.InvoicePayOptions
                {
                    Source = checkSourceId
                });

            return Redirect("https://localhost:44344/Orders");
        }

        // POST /Orders/PayNow/{id}
        [HttpPost]
        public async Task<ActionResult> PayNow(string id, string card, int month, int year, string cvv)
        {
            var order = await _marketplaceDb.GetOrderByIdAsync(id);

            var invoiceService = new Stripe.InvoiceService();
            var invoice = await invoiceService.GetAsync(order.InvoiceId);

            invoice = invoiceService.Update(
                invoice.Id,
                new Stripe.InvoiceUpdateOptions
                {
                    PaymentSettings = new Stripe.InvoicePaymentSettingsOptions
                    { PaymentMethodTypes = new List<string>() { "card" } },
                });

            var pmService = new Stripe.PaymentMethodService();
            var pmCreateOptions = new Stripe.PaymentMethodCreateOptions
            {
                Type = "card",
                Card = new Stripe.PaymentMethodCardOptions
                {
                    Number = card,
                    ExpMonth = month,
                    ExpYear = year,
                    Cvc = cvv
                }
            };

            var pm = pmService.Create(pmCreateOptions);

            var paymentIntentService = new Stripe.PaymentIntentService();
            var piUpdateOptions = new Stripe.PaymentIntentUpdateOptions
            {
                PaymentMethod = pm.Id
            };

            paymentIntentService.Update(invoice.PaymentIntentId, piUpdateOptions);

            var result = paymentIntentService.Confirm(invoice.PaymentIntentId);

            return Redirect("https://localhost:44344/Orders");
        }
    }
}
