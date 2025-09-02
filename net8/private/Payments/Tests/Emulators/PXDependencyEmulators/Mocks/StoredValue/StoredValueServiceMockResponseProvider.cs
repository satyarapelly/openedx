// <copyright file="StoredValueServiceMockResponseProvider.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Test.Common;

    public class StoredValueServiceMockResponseProvider : IMockResponseProvider
    {
        public void ResetDefaults()
        {
        }

        public async Task<HttpResponseMessage> GetMatchedMockResponse(HttpRequestMessage request)
        {
            if (request.RequestUri.AbsolutePath.Contains("gift-catalog"))
            {
                string giftCatalog = "[{\"type\":\"GiftCatalogResource\",\"currency\":\"USD\",\"gift_amount\":5.0,\"description\":\"Test SKU for 5 USD\",\"sku\":\"AMG-01000\",\"version\":\"V1\"},{\"type\":\"GiftCatalogResource\",\"currency\":\"USD\",\"gift_amount\":10.0,\"description\":\"Test SKU for 10 USD\",\"sku\":\"AMG-01000\",\"version\":\"V1\"},{\"type\":\"GiftCatalogResource\",\"currency\":\"USD\",\"gift_amount\":15.0,\"description\":\"Test SKU for 15 USD\",\"sku\":\"AMG-01000\",\"version\":\"V1\"},{\"type\":\"GiftCatalogResource\",\"currency\":\"USD\",\"gift_amount\":20.0,\"description\":\"Test SKU for 20 USD\",\"sku\":\"AMG-01000\",\"version\":\"V1\"},{\"type\":\"GiftCatalogResource\",\"currency\":\"USD\",\"gift_amount\":25.0,\"description\":\"Test SKU for 25 USD\",\"sku\":\"AMG-01000\",\"version\":\"V1\"},{\"type\":\"GiftCatalogResource\",\"currency\":\"USD\",\"gift_amount\":50.0,\"description\":\"Test SKU for 50 USD\",\"sku\":\"AMG-01000\",\"version\":\"V1\"},{\"type\":\"GiftCatalogResource\",\"currency\":\"USD\",\"gift_amount\":75.0,\"description\":\"Test SKU for 75 USD\",\"sku\":\"AMG-01000\",\"version\":\"V1\"},{\"type\":\"GiftCatalogResource\",\"currency\":\"USD\",\"gift_amount\":100.0,\"description\":\"Test SKU for 100 USD\",\"sku\":\"AMG-01000\",\"version\":\"V1\"}]";

                return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        giftCatalog,
                        System.Text.Encoding.UTF8,
                        "application/json")
                });
            }
            else if (request.RequestUri.AbsolutePath.Contains("funds"))
            {
                if (request.Method == HttpMethod.Post)
                {
                    string redeemStarted = "{\"type\":\"FundResource\",\"redirect_content\":\"https://bitpay.com/invoice?id=PujzFdsApS3EymrK5BzZbo\",\"id\":\"ce9d0625-15ae-4b5e-9151-510cea66a431\",\"status\":\"processing\",\"country\":\"US\",\"currency\":\"USD\",\"amount\":20.0,\"description\":\"pcs fund store value\",\"puid\":\"1688852040390626\",\"payment_callback_url\":null,\"payment_transaction_id\":null,\"payment_instrument_id\":\"F2D44338-A605-4A7E-AA50-18B0B2B1E967\",\"version\":\"V1\",\"links\":{\"self\":{\"href\":\"VFYAAAAAAAAAAAAA/funds/ce9d0625-15ae-4b5e-9151-510cea66a431\",\"method\":\"GET\"}}}";

                    return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(
                            redeemStarted,
                            System.Text.Encoding.UTF8,
                            "application/json")
                    });
                }
                else
                {
                    string redeemCompleted = "{\"type\":\"FundResource\",\"redirect_content\":\"https://bitpay.com/invoice?id=PujzFdsApS3EymrK5BzZbo\",\"id\":\"ce9d0625-15ae-4b5e-9151-510cea66a431\",\"status\":\"completed\",\"country\":\"US\",\"currency\":\"USD\",\"amount\":20.0,\"description\":\"pcs fund store value\",\"puid\":\"1688852040390626\",\"payment_callback_url\":null,\"payment_transaction_id\":null,\"payment_instrument_id\":\"F2D44338-A605-4A7E-AA50-18B0B2B1E967\",\"version\":\"V1\",\"links\":{\"self\":{\"href\":\"VFYAAAAAAAAAAAAA/funds/ce9d0625-15ae-4b5e-9151-510cea66a431\",\"method\":\"GET\"}}}";

                    return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(
                            redeemCompleted,
                            System.Text.Encoding.UTF8,
                            "application/json")
                    });
                }
            }
            
            string responseContent = "[]";
            HttpStatusCode statusCode = HttpStatusCode.OK;

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