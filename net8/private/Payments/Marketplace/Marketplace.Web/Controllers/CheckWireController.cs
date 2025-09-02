using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Marketplace.Web.PayModClient;
using Marketplace.Data;
using ThirdParty.Model;

namespace Marketplace.Web.Controllers
{
    [Route("api/CheckWire")]
    [ApiController]
    public class CheckWireController : ControllerBase
    {
        private readonly IMarketplaceDataService _marketplaceDb;
        

        public CheckWireController(IMarketplaceDataService marketplaceDb)
        {
            this._marketplaceDb = marketplaceDb;
        }



        public async Task<ActionResult> GenerateCheckWire()
        {
            var accountId = Guid.NewGuid().ToString();

            var payModClient = TransactionServiceClient.CreateInstance();
            dynamic response;
            var resultCode = payModClient.SendRequest(string.Format("{0}/payments", accountId), 
                "POST", null, null, out response);

            var order = await _marketplaceDb.GetOrderByIdAsync(Request.Query["orderId"][0]);
            var amt = order.Items.Sum(i => i.Price);            

            dynamic chargeRequest = new
            {
                payment_instrument = "21dd9edc-af71-4d62-80ce-37151d475326", // Check PIID
                amount = amt,
                currency = "USD",
                country = "US",
                store = "Stripe3PPDemo",
                third_party_seller = "acct_1JeSuMQvLtsf7fWu",
                seller_of_record = "1010",
                invoice_number = "G" + (new Random()).Next(99999999).ToString(),
                line_items = order.Items.Select(
                    i =>
                    new {
                        catalog_uri = @"/products/9NBLGGGZQ35V/0010/B3B6FH2758MP\",
                        billing_record_id = Guid.NewGuid().ToString(),
                        line_item_id = Guid.NewGuid().ToString(),
                        description = i.Product.Title,
                        item_name = i.Product.Title,
                        charge_amount = i.Price,
                        tax = 0m,
                        is_tax_inclusive = true,
                        quantity = 1,
                        publisher_name = i.Product.Seller.Name,
                        product_code = "Professional Service",
                        content_type = "Professional Service",
                        ip_address = "127.0.0.1"
                    }).ToArray(),
                session_id = Guid.NewGuid().ToString()
            };

            resultCode = payModClient.SendRequest(response.links.charge.href.ToString(), "POST", null, chargeRequest, out response);
            
            if (response.status == "partial_approved")
            {
                var ids = response.id.ToString().Split('/');
                var paymentId = ids[0];
                var transactionId = ids[1];
                var mrn = response.merchant_reference_number;

                var backendClient = TransactionBackendServiceClient.CreateInstance();
                resultCode = backendClient.SendRequest($"StripeInvoice/transactions/{mrn}", "GET", null, null, out response);

                var invoicePdf = response.additional_data.invoice_pdf.ToString();
                var invoiceId = response.additional_data.invoice_id.ToString();

                // Charge was created successfully on PayMod. Update order state.
                order.PaymentId = paymentId;
                order.TransactionId = transactionId;
                order.AccountId = accountId;
                order.InvoicePdf = invoicePdf;
                order.InvoiceId = invoiceId;
                order.State = OrderState.PaymentExperienceCompleted;
                
                await _marketplaceDb.UpdateOrderAsync(order);
                return Redirect(Request.Query["ru"][0]);
            }
            else
            {
                return Redirect(Request.Query["rx"][0]);
            }
        }
    }
}
