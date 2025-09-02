// <copyright file="TransactionServiceMockResponseProvider.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Transaction;
    using Microsoft.Commerce.Payments.PXService;
    using Test.Common;

    public class TransactionServiceMockResponseProvider : IMockResponseProvider
    {
        public void ResetDefaults()
        {
        }

        public async Task<HttpResponseMessage> GetMatchedMockResponse(HttpRequestMessage request)
        {
            TestContext testContext = HttpRequestHelper.GetTestHeader(request);

            if (request.RequestUri.AbsolutePath.Contains("/payments") && !request.RequestUri.AbsolutePath.Contains("/validate"))
            {
                string paymentsContent = "{\"id\":\"Z10045B36S4U5348c091-e2ae-4832-8883-5b12906180e9\",\"transactions\":[],\"type\":\"Payment\",\"version\":\"2018-05-07\",\"links\":{\"self\":{\"href\":\"/4a5635f3-33ec-4ac7-9159-084241f547fc/payments/Z10045B36S4U5348c091-e2ae-4832-8883-5b12906180e9\",\"method\":\"GET\"},\"charge\":{\"href\":\"/4a5635f3-33ec-4ac7-9159-084241f547fc/payments/Z10045B36S4U5348c091-e2ae-4832-8883-5b12906180e9/charge\",\"method\":\"POST\"},\"authorize\":{\"href\":\"/4a5635f3-33ec-4ac7-9159-084241f547fc/payments/Z10045B36S4U5348c091-e2ae-4832-8883-5b12906180e9/authorize\",\"method\":\"POST\"},\"credit\":{\"href\":\"/4a5635f3-33ec-4ac7-9159-084241f547fc/payments/Z10045B36S4U5348c091-e2ae-4832-8883-5b12906180e9/credit\",\"method\":\"POST\"},\"validate\":{\"href\":\"/4a5635f3-33ec-4ac7-9159-084241f547fc/payments/Z10045B36S4U5348c091-e2ae-4832-8883-5b12906180e9/validate\",\"method\":\"POST\"},\"predeposit\":{\"href\":\"/4a5635f3-33ec-4ac7-9159-084241f547fc/payments/Z10045B36S4U5348c091-e2ae-4832-8883-5b12906180e9/predeposit\",\"method\":\"POST\"},\"deposit\":{\"href\":\"/4a5635f3-33ec-4ac7-9159-084241f547fc/payments/Z10045B36S4U5348c091-e2ae-4832-8883-5b12906180e9/deposit\",\"method\":\"POST\"},\"withdraw\":{\"href\":\"/4a5635f3-33ec-4ac7-9159-084241f547fc/payments/Z10045B36S4U5348c091-e2ae-4832-8883-5b12906180e9/withdraw\",\"method\":\"POST\"},\"transfer\":{\"href\":\"/4a5635f3-33ec-4ac7-9159-084241f547fc/payments/Z10045B36S4U5348c091-e2ae-4832-8883-5b12906180e9/transfer\",\"method\":\"POST\"},\"preauthenticate\":{\"href\":\"/4a5635f3-33ec-4ac7-9159-084241f547fc/payments/Z10045B36S4U5348c091-e2ae-4832-8883-5b12906180e9/preauthenticate\",\"method\":\"POST\"},\"authenticate\":{\"href\":\"/4a5635f3-33ec-4ac7-9159-084241f547fc/payments/Z10045B36S4U5348c091-e2ae-4832-8883-5b12906180e9/authenticate\",\"method\":\"POST\"},\"prenotify\":{\"href\":\"/4a5635f3-33ec-4ac7-9159-084241f547fc/payments/Z10045B36S4U5348c091-e2ae-4832-8883-5b12906180e9/prenotify\",\"method\":\"POST\"}}}";

                return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        paymentsContent,
                        System.Text.Encoding.UTF8,
                        "application/json")
                });
            }
            else if (request.RequestUri.AbsolutePath.Contains("/payments") && request.RequestUri.AbsolutePath.Contains("/validate") && request.RequestUri.AbsolutePath.Contains("/TransactionValidateErrorAccountId"))
            {
                string validateCvv = "{\"payment_instrument\":\"TlCDmAEAAAAFAACA\",\"amount\":5193.73,\"currency\":\"INR\",\"country\":\"IN\",\"merchant_reference_number\":\"Z51TAY584RDP\",\"merchant_id\":\"I1098BINR1\",\"additional_validation_info\":{\"cvv_result\":\"none\",\"address_validation_result\":\"unknown\",\"zipcode_validation_result\":\"unknown\",\"name_validation_result\":\"unknown\"},\"id\":\"Z10065BQDDDEb196122b-9d55-4fdf-8235-3dae50604c03/168e33aa-51c7-4dae-baad-cfdb504b56f7\",\"status\":\"declined\",\"status_details\":{\"code\":\"processor_declined\",\"processor_response\":{\"auth_status\":\"ErrorCondition\",\"error_code\":\"ERR122\",\"error_message\":\"Please enter a valid DEBIT card number.\"},\"error_code\":\"none\"},\"type\":\"Validate\",\"version\":\"MASKED(10)\",\"links\":{\"self\":{\"href\":\"/6fc584be-f472-4aad-bb48-3a307a21f42e/validations/Z10065BQDDDEb196122b-9d55-4fdf-8235-3dae50604c03/168e33aa-51c7-4dae-baad-cfdb504b56f7\",\"method\":\"GET\"},\"payments\":{\"href\":\"/6fc584be-f472-4aad-bb48-3a307a21f42e/payments/Z10065BQDDDEb196122b-9d55-4fdf-8235-3dae50604c03\",\"method\":\"GET\"}}}";

                return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent(
                        validateCvv,
                        System.Text.Encoding.UTF8,
                        "application/json")
                });
            }
            else if (request.RequestUri.AbsolutePath.Contains("/validate") && HttpRequestHelper.HasAuthenticateThreeDSUserErrorTestScenario(testContext))
            {
                string validateContent = "{ \"payment_instrument\": \"BZhItwAAAAACAACA\", \"amount\": 14248.11, \"currency\": \"INR\", \"country\": \"IN\", \"merchant_reference_number\": \"Z63BXZW4GBAM\", \"merchant_id\": \"I1098BINR3\", \"additional_validation_info\": { \"cvv_result\": \"none\", \"address_validation_result\": \"unknown\", \"zipcode_validation_result\": \"unknown\", \"name_validation_result\": \"unknown\" }, \"id\": \"Z10119D3MYTYdbaa2a0a-e47e-4492-bdc4-ed0969dd5d50/926d7531-18d9-439c-b98f-ea7c34547527\", \"status\": \"declined\", \"status_details\": { \"code\": \"TransactionNotAllowed\", \"processor_response\": { \"response_type\": \"Microsoft.Commerce.Payments.Providers.BillDesk.Client.V2.AuthenticationResponse\", \"auth_status\": \"Failed\", \"error_code\": \"TKNFE0001\", \"error_message\": \"Card Account not found\" }, \"error_code\": \"none\" }, \"type\": \"Validate\", \"version\": \"2018-05-07\", \"links\": { \"self\": { \"href\": \"/de1e41b4-1d6d-47ba-9e70-8162f3dbbd1d/validations/Z10119D3MYTYdbaa2a0a-e47e-4492-bdc4-ed0969dd5d50/926d7531-18d9-439c-b98f-ea7c34547527\", \"method\": \"GET\" }, \"payments\": { \"href\": \"/de1e41b4-1d6d-47ba-9e70-8162f3dbbd1d/payments/Z10119D3MYTYdbaa2a0a-e47e-4492-bdc4-ed0969dd5d50\", \"method\": \"GET\" } } }";

                return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        validateContent,
                        System.Text.Encoding.UTF8,
                        "application/json")
                });
            }
            else if (request.RequestUri.AbsolutePath.Contains("/validate"))
            {
                string validateContent = "{\"payment_instrument\":\"AfC48wEAAAABAACA\",\"amount\":3505.48,\"currency\":\"INR\",\"country\":\"IN\",\"merchant_reference_number\":\"Z419J5RYOVS6\",\"merchant_id\":\"I1098BINR1\",\"additional_validation_info\":{\"cvv_result\":\"none\",\"address_validation_result\":\"unknown\",\"zipcode_validation_result\":\"unknown\",\"name_validation_result\":\"unknown\"},\"redirect_url\":\"https://paymentsredirectionservice.cp.microsoft.com/RedirectionService/CoreRedirection/Redirect/0dafd739-ae55-4bf5-b6f5-9e28b9773d6f\",\"id\":\"Z10045B36S4U5348c091-e2ae-4832-8883-5b12906180e9/2801f9d0-86e7-4bd3-9bce-28c3afc6d056\",\"status\":\"pending\",\"status_details\":{\"code\":\"none\",\"processor_response\":{\"auth_status\":\"Unknown\"},\"error_code\":\"none\"},\"type\":\"Validate\",\"version\":\"2018-05-07\",\"links\":{\"self\":{\"href\":\"/4a5635f3-33ec-4ac7-9159-084241f547fc/validations/Z10045B36S4U5348c091-e2ae-4832-8883-5b12906180e9/2801f9d0-86e7-4bd3-9bce-28c3afc6d056\",\"method\":\"GET\"},\"payments\":{\"href\":\"/4a5635f3-33ec-4ac7-9159-084241f547fc/payments/Z10045B36S4U5348c091-e2ae-4832-8883-5b12906180e9\",\"method\":\"GET\"},\"reverse\":{\"href\":\"/4a5635f3-33ec-4ac7-9159-084241f547fc/validations/Z10045B36S4U5348c091-e2ae-4832-8883-5b12906180e9/2801f9d0-86e7-4bd3-9bce-28c3afc6d056/reverse\",\"method\":\"POST\"}}}";

                return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        validateContent,
                        System.Text.Encoding.UTF8,
                        "application/json")
                });
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