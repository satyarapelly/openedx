// <copyright company="Microsoft Corporation">Copyright (c) Microsoft 2018. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.PXService.Model.D365Service;
    using Newtonsoft.Json;
    using Test.Common;

    public class D365ServiceMockResponseProvider : IMockResponseProvider
    {
        public static List<MockD365CheckPiResult> PaymentInstrumentCheckResponses { get; private set; }

        public static List<MockD365OrdersResponse> Orders { get; private set; }

        public static List<Cart> Carts { get; private set; }

        public D365ServiceMockResponseProvider()
        {
            var paymentInstrumentCheckResponsesJson = File.ReadAllText(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    @"Mocks\PaymentInstrumentCheckResponses.json"));

            PaymentInstrumentCheckResponses = JsonConvert.DeserializeObject<List<MockD365CheckPiResult>>(
                paymentInstrumentCheckResponsesJson);

            var ordersJson = File.ReadAllText(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    @"Mocks\D365OrdersResponse.json"));

            Orders = JsonConvert.DeserializeObject<List<MockD365OrdersResponse>>(
                ordersJson);

            var cartJson = File.ReadAllText(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    @"Mocks\D365CartResponse.json"));

            Carts = JsonConvert.DeserializeObject<List<Cart>>(
                cartJson);
        }

        public void ResetDefaults()
        {
        }

        public async Task<HttpResponseMessage> GetMatchedMockResponse(HttpRequestMessage request)
        {
            var trimmedSegments = request.RequestUri.Segments.Select(s => s.Trim(new char[] { '/' })).ToArray();

            string responseContent = null;
            HttpStatusCode statusCode = HttpStatusCode.OK;
            if (request.RequestUri.ToString().Contains("paymentinstrumentcheck"))
            {
                var filteredCheckPiResult = PaymentInstrumentCheckResponses.FirstOrDefault(x => trimmedSegments[6] == x.PaymentInstrumentId)?.GetPaymentInstrumentCheckResponse();
                if (filteredCheckPiResult == null)
                {
                    filteredCheckPiResult = new PaymentInstrumentCheckResponse()
                    {
                        PendingOrderIds = new List<string>()
                    };
                }

                responseContent = JsonConvert.SerializeObject(filteredCheckPiResult);
            }
            else
            {
                if (request.RequestUri.ToString().Contains("orders"))
                {
                    var orderId = System.Web.HttpUtility.ParseQueryString(request.RequestUri.Query).Get("orderId");

                    // GerOrder
                    var filteredOrder = Orders.FirstOrDefault(x => orderId == x.OrderId)?.PagedResponse;
                    if (filteredOrder == null)
                    {
                        filteredOrder = new PagedResponse<Order>()
                        {
                            HasNextPage = false,
                            Items = new List<Order>(),
                        };
                    }

                    responseContent = JsonConvert.SerializeObject(filteredOrder);
                }
                else if (request.RequestUri.ToString().Contains("GetCartByCartId"))
                {
                    var cartId = System.Web.HttpUtility.ParseQueryString(request.RequestUri.Query).Get("id");

                    // Get Cart
                    var filteredCart = Carts.FirstOrDefault(x => cartId == x.Id);
                    if (filteredCart == null)
                    {
                        filteredCart = new Cart();
                    }

                    responseContent = JsonConvert.SerializeObject(filteredCart);
                }
            }

            return await Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(
                    responseContent,
                    System.Text.Encoding.UTF8,
                    "application/json")
            });
        }
    }
}