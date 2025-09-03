// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

using Microsoft.Commerce.Payments.PXService.Model.D365Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CIT.PXService.Mocks
{
    public class D365Service : HttpMessageHandler
    {
        public static List<MockD365CheckPiResult> PaymentInstrumentCheckResponses { get; private set; }
        public static List<MockD365OrdersResponse> Orders { get; private set; }
        public static List<Cart> Carts { get; private set; }

        public D365Service()
        {
            ResetToDefaults();

            PaymentInstrumentCheckResponses = JsonConvert.DeserializeObject<List<MockD365CheckPiResult>>(
                File.ReadAllText(
                    Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Mocks",
                        "PaymentInstrumentCheckResponses.json")));

            Orders = JsonConvert.DeserializeObject<List<MockD365OrdersResponse>>(
                File.ReadAllText(
                    Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Mocks",
                        "D365OrdersResponse.json")));

            Carts = JsonConvert.DeserializeObject<List<Cart>>(
                File.ReadAllText(
                    Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Mocks",
                        "D365CartResponse.json")));
        }

        public void ResetToDefaults()
        {
            PaymentInstrumentCheckResponses = new List<MockD365CheckPiResult>();
            Orders = new List<MockD365OrdersResponse>();
            Carts = new List<Cart>();
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var trimmedSegments = request.RequestUri.Segments.Select(s => s.Trim('/')).ToArray();
            string responseContent = null;
            HttpStatusCode statusCode = HttpStatusCode.OK;

            if (request.RequestUri.ToString().Contains("paymentinstrumentcheck"))
            {
                var piId = trimmedSegments.Length > 6 ? trimmedSegments[6] : null;
                var result = PaymentInstrumentCheckResponses.FirstOrDefault(x => piId == x.PaymentInstrumentId)?.GetPaymentInstrumentCheckResponse()
                    ?? new PaymentInstrumentCheckResponse { PendingOrderIds = new List<string>() };

                responseContent = JsonConvert.SerializeObject(result);
            }
            else if (request.RequestUri.ToString().Contains("orders"))
            {
                var query = System.Web.HttpUtility.ParseQueryString(request.RequestUri.Query);
                var orderId = query.Get("orderId");

                var order = Orders.FirstOrDefault(x => x.OrderId == orderId)?.PagedResponse
                    ?? new PagedResponse<Order> { HasNextPage = false, Items = new List<Order>() };

                responseContent = JsonConvert.SerializeObject(order);
            }
            else if (request.RequestUri.ToString().Contains("GetCartByCartId"))
            {
                var query = System.Web.HttpUtility.ParseQueryString(request.RequestUri.Query);
                var cartId = query.Get("id");

                var cart = Carts.FirstOrDefault(x => cartId == x.Id) ?? new Cart();
                responseContent = JsonConvert.SerializeObject(cart);
            }

            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseContent ?? "{}", System.Text.Encoding.UTF8, "application/json")
            });
        }
    }
}