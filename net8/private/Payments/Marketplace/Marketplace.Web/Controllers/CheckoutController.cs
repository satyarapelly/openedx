using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Marketplace.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ThirdParty.Model;

namespace Marketplace.Web.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly IMarketplaceDataService _marketplaceDb;
        private readonly IConfiguration _config;
        private readonly bool _isDevelopmentEnv;

        public CheckoutController(
            IMarketplaceDataService marketplaceDb, 
            IConfiguration config)
        {
            _marketplaceDb = marketplaceDb;
            _config = config;

            _isDevelopmentEnv = string.Equals(
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                "Development",
                StringComparison.OrdinalIgnoreCase);
        }

        // GET /Checkout/<orderId>
        public async Task<ActionResult> Index(string orderId)
        {
            var customer = await _marketplaceDb.GetCustomerByIdAsync("erin.buyer@contoso.com");
            var order = await _marketplaceDb.GetOrderByIdAsync(orderId);

            if (order.State != OrderState.InCart
                && order.State != OrderState.PaymentFailed)
            {
                ViewBag.ErrorTitle = "Cannot checkout this order in its current state.";
                ViewBag.UserErrorMessage = "";

                if (_isDevelopmentEnv)
                {
                    ViewBag.DeveloperErrorMessage = $"Checkout called on order {orderId} which is in a {order.State} state.";
                }
                return View("Error");
            }

            ViewBag.Order = order;
            ViewBag.Customer = customer;
            return View("Index");
        }

        public async Task<ActionResult> PayWithPIOnFile(string orderId, string piId)
        {
            var customer = await _marketplaceDb.GetCustomerByIdAsync("erin.buyer@contoso.com");
            var order = await _marketplaceDb.GetOrderByIdAsync(orderId);

            if (customer.ProcessorId == null)
            {
                var stripeCustomer = await CreateStripesCopyOfCustomerAsync(customer);

                customer.ProcessorId = stripeCustomer.Id;
                customer.ProcessorObject = stripeCustomer;
                customer = await _marketplaceDb.UpdateCustomerAsync(customer);
            }

            var paymentInstrument = customer.PaymentInstruments.Where(
                p => string.Equals(p.Id, piId, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            var piObject = (JObject)paymentInstrument.Details;
            var pi = piObject.ToObject<Card>();

            // Create a Just-In-Time (single-use) Token
            var tokenService = new Stripe.TokenService();
            var tokenOptions = new Stripe.TokenCreateOptions()
            {
                Card = new Stripe.TokenCardOptions()
                {
                    Number = pi.Number,     // e.g. 4242424242424242
                    ExpMonth = pi.ExpMonth, // e.g. 12
                    ExpYear = pi.ExpYear,   // e.g. 23
                    Cvc = pi.CVV            // e.g. 456
                }
            };
            var cardToken = tokenService.Create(tokenOptions);

            // Crate a PaymentMethod from the Single-Use Token
            var pmService = new Stripe.PaymentMethodService();
            var pmOptions = new Stripe.PaymentMethodCreateOptions()
            {
                Type = "card",
                Card = new Stripe.PaymentMethodCardOptions()
                {
                    Token = cardToken.Id
                }
            };
            var pm = pmService.Create(pmOptions);

            // Use the PaymentMethod in a PaymentIntent and Confirm
            // which completes payment barring 3DS/PSD2 etc.
            var pinService = new Stripe.PaymentIntentService();
            var pinOptions = new Stripe.PaymentIntentCreateOptions
            {
                Description = order.Id,
                Customer = customer.ProcessorId,
                PaymentMethod = pm?.Id,
                Amount = (long)order.Total * 100,
                Currency = "usd",
                ApplicationFeeAmount = (long)order.Total * 10,
                TransferData = new Stripe.PaymentIntentTransferDataOptions
                {
                    Destination = order.Items[0].Product.Seller.AccountId
                },
            };
            var pin = await pinService.CreateAsync(pinOptions);

            // Moving this order into InCheckout.  One of the effects of this is that the CheckoutDate
            // gets updated.
            order.PaymentIntentId = pin.Id;
            order.State = OrderState.InCheckOut;
            order = await this._marketplaceDb.UpdateOrderAsync(order);

            // Confirm the PaymentIntent (causes a Charge/Capture on a card).  Chagne the order state
            // based on the PaymentIntent state.
            pin = await pinService.ConfirmAsync(pin.Id);

            if (string.Equals(pin.Status, "succeeded", StringComparison.OrdinalIgnoreCase))
            {
                order.State = OrderState.PaymentSucceeded;
                order = await this.OverwriteUpdateOrderAsync("Controller", order);
                return RedirectToAction("Index", "Orders", new { highlightId = order.Id });
            }
            else
            {
                order.State = OrderState.PaymentFailed;
                order = await this.OverwriteUpdateOrderAsync("Controller", order);
                return RedirectToAction("Index", "Orders", new { highlightId = order.Id });
            }
        }

        // POST: Checkout/Callback
        [HttpPost]
        public async Task<IActionResult> Callback()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeEvent = JsonConvert.DeserializeObject<Stripe.Event>(json);
            if (string.Equals(stripeEvent.Type, "checkout.session.completed", StringComparison.OrdinalIgnoreCase))
            {
                System.Diagnostics.Debug.WriteLine($"Marketplace.Web -> Callback: {stripeEvent.Type}");
                var stripeSession = (Stripe.Checkout.Session)stripeEvent.Data.Object;
                var newState = string.Equals(
                    stripeSession.PaymentStatus,
                    "paid",
                    StringComparison.OrdinalIgnoreCase)
                    ? OrderState.PaymentSucceeded
                    : OrderState.PaymentFailed;

                // We had saved the orderId as the ClientReferenceId when creating the CheckoutSession
                var orderId = stripeSession.ClientReferenceId;

                var order = await _marketplaceDb.GetOrderByIdAsync(orderId);
                order.State = OrderState.PaymentSucceeded;
                order.PaymentIntentId = stripeSession.PaymentIntentId;
                await OverwriteUpdateOrderAsync("Callback", order);
            }
            else if (string.Equals(stripeEvent.Type, "payment_intent.succeeded", StringComparison.OrdinalIgnoreCase))
            {
                System.Diagnostics.Debug.WriteLine($"Marketplace.Web -> Callback: {stripeEvent.Type}");
                var stripePaymentIntent = (Stripe.PaymentIntent)stripeEvent.Data.Object;
                var newState = string.Equals(
                    stripePaymentIntent.Status,
                    "succeeded",
                    StringComparison.OrdinalIgnoreCase)
                    ? OrderState.PaymentSucceeded
                    : OrderState.PaymentFailed;

                // We had saved the orderId in the Description field when creating the PaymentIntent
                var orderId = stripePaymentIntent.Description;

                var order = await _marketplaceDb.GetOrderByIdAsync(orderId);
                order.State = OrderState.PaymentSucceeded;
                order.PaymentIntentId = stripePaymentIntent.Id;
                await OverwriteUpdateOrderAsync("Callback", order);
            }
            
            return Ok();
        }

        private async Task<Stripe.Customer> CreateStripesCopyOfCustomerAsync(Customer customer)
        {
            var customerService = new Stripe.CustomerService();
            var customerCreateOption = new Stripe.CustomerCreateOptions()
            {
                Name = $"{customer.FirstName} {customer.LastName}",
                Email = customer.Email,
                Address = new Stripe.AddressOptions()
                {
                    Line1 = customer.Address.AddressLine1,
                    City = customer.Address.City,
                    State = customer.Address.State,
                    PostalCode = customer.Address.ZipCode,
                    Country = customer.Address.Region
                },
            };

            return await customerService.CreateAsync(customerCreateOption);
        }

        // If we get an ETag mismatch (collision), get the order object again and update State
        // and PaymentIntentId.  If others had also changed these fields, they will be
        // overwritten.  If others had made changes to other properties, they will be preserved.
        private async Task<Order> OverwriteUpdateOrderAsync(string caller, Order order)
        {
            int attempts = 0;
            do
            {
                System.Diagnostics.Debug.WriteLine($"Marketplace.Web -> {caller} -> UpdateOrderStateAsync -> Attempts: {attempts}");
                try
                {
                    attempts++;
                    order = await _marketplaceDb.UpdateOrderAsync(order);
                    return order;
                }
                catch (CosmosException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Marketplace.Web -> {caller} -> UpdateOrderStateAsync -> Exception StatusCode: {ex.StatusCode}");
                    if (ex.StatusCode == HttpStatusCode.PreconditionFailed)
                    {
                        if (attempts < 3)
                        {
                            var orderInDb = await _marketplaceDb.GetOrderByIdAsync(order.Id);
                            order.Etag = orderInDb.Etag;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            } while (true);

            throw new Exception("Could not update Order data in the database even after 3 attempts.");
        }
    }
}
