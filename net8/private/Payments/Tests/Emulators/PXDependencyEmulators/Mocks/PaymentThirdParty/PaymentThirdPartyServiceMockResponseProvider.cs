// <copyright file="PaymentThirdPartyServiceMockResponseProvider.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Web;
    using Test.Common;

    public class PaymentThirdPartyServiceMockResponseProvider : IMockResponseProvider
    {
        public void ResetDefaults()
        {
        }

        public async Task<HttpResponseMessage> GetMatchedMockResponse(HttpRequestMessage request)
        {
            string responseContent = "[]";
            HttpStatusCode statusCode = HttpStatusCode.OK;

            if (request.RequestUri.AbsolutePath.Contains("/charge"))
            {
                string payload = await request.GetRequestPayload();
                if (payload.Contains("ProcessorDeclined"))
                {
                    statusCode = HttpStatusCode.BadRequest;
                    responseContent = "{\"errorCode\": \"ProcessorDeclined\",\"message\": \"{\\\"Id\\\":\\\"Z10006BOKMH9c57764ba-ac85-402b-befa-3c99d5048896/ee716883-a889-499c-b3b8-f8dd50587350\\\",\\\"PaymentInstrument\\\":\\\"c01d6ae6-27db-42cf-b42f-ca1876d605e4\\\",\\\"Amount\\\":1.04,\\\"AmountReceived\\\":0.0,\\\"TargetAmount\\\":1.04,\\\"Currency\\\":\\\"USD\\\",\\\"Country\\\":\\\"ca\\\",\\\"MerchantReferenceNumber\\\":\\\"Z5068RXFR05F\\\",\\\"MerchantId\\\":\\\"S10103PP01\\\",\\\"TransactionId\\\":\\\"ee716883-a889-499c-b3b8-f8dd50587350\\\",\\\"type\\\":\\\"Charge\\\",\\\"Status\\\":\\\"declined\\\",\\\"StatusDetails\\\":{\\\"code\\\":\\\"processor_declined\\\",\\\"processor_response\\\":{\\\"http_status_code\\\":\\\"payment_required\\\",\\\"stripe_error\\\":{\\\"charge\\\":\\\"ch_3L1eOnQtmDZuG3en0VKogTsh\\\",\\\"code\\\":\\\"card_declined\\\",\\\"decline_code\\\":\\\"test_mode_live_card\\\",\\\"doc_url\\\":\\\"https://stripe.com/docs/error-codes/card-declined\\\",\\\"message\\\":\\\"Your card was declined. Your request was in test mode, but used a non test (live) card. For a list of valid test cards, visit: https://stripe.com/docs/testing.\\\",\\\"payment_intent\\\":{\\\"id\\\":\\\"pi_3L1eOnQtmDZuG3en0KJOHd00\\\",\\\"object\\\":\\\"payment_intent\\\",\\\"amount\\\":104,\\\"amount_capturable\\\":0,\\\"amount_received\\\":0,\\\"application\\\":\\\"ca_KFJEQpwjIjypz6SMh0kSqNrdOz7E36q2\\\",\\\"capture_method\\\":\\\"automatic\\\",\\\"charges\\\":{\\\"object\\\":\\\"list\\\",\\\"data\\\":[{\\\"id\\\":\\\"ch_3L1eOnQtmDZuG3en0VKogTsh\\\",\\\"object\\\":\\\"charge\\\",\\\"amount\\\":104,\\\"amount_captured\\\":0,\\\"amount_refunded\\\":0,\\\"application\\\":\\\"ca_KFJEQpwjIjypz6SMh0kSqNrdOz7E36q2\\\",\\\"application_fee\\\":null,\\\"balance_transaction\\\":null,\\\"billing_details\\\":{\\\"address\\\":{}},\\\"calculated_statement_descriptor\\\":\\\"MSN.COM\\\",\\\"captured\\\":false,\\\"created\\\":1653086061,\\\"currency\\\":\\\"usd\\\",\\\"customer\\\":null,\\\"destination\\\":null,\\\"dispute\\\":null,\\\"disputed\\\":false,\\\"failure_code\\\":\\\"card_declined\\\",\\\"failure_message\\\":\\\"Your card was declined. Your request was in test mode, but used a non test (live) card. For a list of valid test cards, visit: https://stripe.com/docs/testing.\\\",\\\"fraud_details\\\":{},\\\"invoice\\\":null,\\\"livemode\\\":false,\\\"metadata\\\":{\\\"mrn\\\":\\\"Z5068RXFR05F\\\"},\\\"on_behalf_of\\\":null,\\\"order\\\":null,\\\"outcome\\\":{\\\"network_status\\\":\\\"not_sent_to_network\\\",\\\"reason\\\":\\\"test_mode_live_card\\\",\\\"risk_level\\\":\\\"normal\\\",\\\"risk_score\\\":17,\\\"seller_message\\\":\\\"This charge request was in test mode, but did not use a Stripe test card number. For the list of these numbers, see stripe.com/docs/testing\\\",\\\"type\\\":\\\"invalid\\\"},\\\"paid\\\":false,\\\"payment_intent\\\":\\\"pi_3L1eOnQtmDZuG3en0KJOHd00\\\",\\\"payment_method\\\":\\\"pm_1L1eOmQtmDZuG3engROf3Mip\\\",\\\"payment_method_details\\\":{\\\"card\\\":{\\\"brand\\\":\\\"visa\\\",\\\"checks\\\":{\\\"cvc_check\\\":\\\"unchecked\\\"},\\\"country\\\":\\\"US\\\",\\\"exp_month\\\":3,\\\"exp_year\\\":2023,\\\"fingerprint\\\":\\\"txtI3xz3RKuxmWPx\\\",\\\"funding\\\":\\\"unknown\\\",\\\"last4\\\":\\\"1111\\\",\\\"network\\\":\\\"visa\\\"},\\\"type\\\":\\\"card\\\"},\\\"receipt_email\\\":\\\"sumudunu@microsoft.com\\\",\\\"refunded\\\":false,\\\"refunds\\\":{\\\"object\\\":\\\"list\\\",\\\"data\\\":[],\\\"has_more\\\":false,\\\"url\\\":\\\"/v1/charges/ch_3L1eOnQtmDZuG3en0VKogTsh/refunds\\\"},\\\"review\\\":null,\\\"source_transfer\\\":null,\\\"status\\\":\\\"failed\\\"}],\\\"has_more\\\":false,\\\"url\\\":\\\"/v1/charges?payment_intent=pi_3L1eOnQtmDZuG3en0KJOHd00\\\"},\\\"client_secret\\\":\\\"pi_3L1eOnQtmDZuG3en0KJOHd00_secret_PXRt7JGTfb5JijHFoc27YqmLo\\\",\\\"confirmation_method\\\":\\\"automatic\\\",\\\"created\\\":1653086061,\\\"currency\\\":\\\"usd\\\",\\\"customer\\\":null,\\\"invoice\\\":null,\\\"last_payment_error\\\":{\\\"charge\\\":\\\"ch_3L1eOnQtmDZuG3en0VKogTsh\\\",\\\"code\\\":\\\"card_declined\\\",\\\"decline_code\\\":\\\"test_mode_live_card\\\",\\\"doc_url\\\":\\\"https://stripe.com/docs/error-codes/card-declined\\\",\\\"message\\\":\\\"Your card was declined. Your request was in test mode, but used a non test (live) card. For a list of valid test cards, visit: https://stripe.com/docs/testing.\\\",\\\"payment_method\\\":{\\\"id\\\":\\\"pm_1L1eOmQtmDZuG3engROf3Mip\\\",\\\"object\\\":\\\"payment_method\\\",\\\"billing_details\\\":{\\\"address\\\":{}},\\\"card\\\":{\\\"brand\\\":\\\"visa\\\",\\\"checks\\\":{\\\"cvc_check\\\":\\\"unchecked\\\"},\\\"country\\\":\\\"US\\\",\\\"exp_month\\\":3,\\\"exp_year\\\":2023,\\\"fingerprint\\\":\\\"txtI3xz3RKuxmWPx\\\",\\\"funding\\\":\\\"unknown\\\",\\\"last4\\\":\\\"1111\\\",\\\"networks\\\":{\\\"available\\\":[\\\"visa\\\"]},\\\"three_d_secure_usage\\\":{\\\"supported\\\":true}},\\\"created\\\":1653086060,\\\"customer\\\":null,\\\"livemode\\\":false,\\\"metadata\\\":{},\\\"type\\\":\\\"card\\\"},\\\"type\\\":\\\"card_error\\\"},\\\"livemode\\\":false,\\\"metadata\\\":{\\\"mrn\\\":\\\"Z5068RXFR05F\\\"},\\\"on_behalf_of\\\":null,\\\"payment_method\\\":null,\\\"payment_method_options\\\":{\\\"card\\\":{\\\"request_three_d_secure\\\":\\\"automatic\\\"}},\\\"payment_method_types\\\":[\\\"card\\\"],\\\"receipt_email\\\":\\\"sumudunu@microsoft.com\\\",\\\"review\\\":null,\\\"source\\\":null,\\\"status\\\":\\\"requires_payment_method\\\"},\\\"payment_method\\\":{\\\"id\\\":\\\"pm_1L1eOmQtmDZuG3engROf3Mip\\\",\\\"object\\\":\\\"payment_method\\\",\\\"billing_details\\\":{\\\"address\\\":{}},\\\"card\\\":{\\\"brand\\\":\\\"visa\\\",\\\"checks\\\":{\\\"cvc_check\\\":\\\"unchecked\\\"},\\\"country\\\":\\\"US\\\",\\\"exp_month\\\":3,\\\"exp_year\\\":2023,\\\"fingerprint\\\":\\\"txtI3xz3RKuxmWPx\\\",\\\"funding\\\":\\\"unknown\\\",\\\"last4\\\":\\\"1111\\\",\\\"networks\\\":{\\\"available\\\":[\\\"visa\\\"]},\\\"three_d_secure_usage\\\":{\\\"supported\\\":true}},\\\"created\\\":1653086060,\\\"customer\\\":null,\\\"livemode\\\":false,\\\"metadata\\\":{},\\\"type\\\":\\\"card\\\"},\\\"type\\\":\\\"card_error\\\"}},\\\"decline_message\\\":\\\"Your card was declined. Your request was in test mode, but used a non test (live) card. For a list of valid test cards, visit: https://stripe.com/docs/testing.\\\",\\\"error_source\\\":null,\\\"error_code\\\":\\\"none\\\"},\\\"ExternalReference\\\":null,\\\"Links\\\":{\\\"self\\\":{\\\"href\\\":\\\"/b0dd7483-b951-42ab-b4dd-c86cedf5501c/charges/Z10006BOKMH9c57764ba-ac85-402b-befa-3c99d5048896/ee716883-a889-499c-b3b8-f8dd50587350\\\",\\\"method\\\":\\\"GET\\\"},\\\"reduce\\\":{\\\"href\\\":\\\"/b0dd7483-b951-42ab-b4dd-c86cedf5501c/charges/Z10006BOKMH9c57764ba-ac85-402b-befa-3c99d5048896/ee716883-a889-499c-b3b8-f8dd50587350/reduce\\\",\\\"method\\\":\\\"POST\\\"},\\\"refund\\\":{\\\"href\\\":\\\"/b0dd7483-b951-42ab-b4dd-c86cedf5501c/charges/Z10006BOKMH9c57764ba-ac85-402b-befa-3c99d5048896/ee716883-a889-499c-b3b8-f8dd50587350/refund\\\",\\\"method\\\":\\\"POST\\\"},\\\"payments\\\":{\\\"href\\\":\\\"/b0dd7483-b951-42ab-b4dd-c86cedf5501c/payments/Z10006BOKMH9c57764ba-ac85-402b-befa-3c99d5048896\\\",\\\"method\\\":\\\"GET\\\"}},\\\"authenticate_value\\\":null,\\\"eci\\\":null,\\\"redirect_url\\\":null}\",\"source\": \"TransactionService\",\"innerError\": null}";                    
                }
                else
                {
                    responseContent = "{\"id\": \"7704e9b5-df70-46ce-bc6f-ae563fe02e7c\",\"payerId\": \"af8cabea-4bd8-469b-b45b-fb7dcbae7527\",\"paymentRequestId\": \"cc28fc5c-b5b4-42ec-b070-4553ca3d8735\",\"status\": \"paid\"}";
                }
            }
            else if (request.RequestUri.AbsolutePath.Contains("/api/checkouts"))
            {
                if (request.RequestUri.AbsolutePath.EndsWith("checkouts/checkoutid-paid"))
                {
                    responseContent = "{\"id\": \"7704e9b5-df70-46ce-bc6f-ae563fe02e7c\",\"payerId\": \"af8cabea-4bd8-469b-b45b-fb7dcbae7527\",\"paymentRequestId\": \"cc28fc5c-b5b4-42ec-b070-4553ca3d8735\",\"status\": \"paid\",\"returnUrl\":\"https://paymentinstruments-int.mp.microsoft.com/V6.0/checkoutsEx/7704e9b5-df70-46ce-bc6f-ae563fe02e7c/completed?redirectUrl=https://teams.microsoft.com\",\"redirectUrl\": \"\"}";
                }
                else
                {
                    responseContent = "{\"id\": \"7704e9b5-df70-46ce-bc6f-ae563fe02e7c\",\"payerId\": \"af8cabea-4bd8-469b-b45b-fb7dcbae7527\",\"paymentRequestId\": \"cc28fc5c-b5b4-42ec-b070-4553ca3d8735\",\"status\": \"created\",\"returnUrl\":\"https://paymentinstruments-int.mp.microsoft.com/V6.0/checkoutsEx/7704e9b5-df70-46ce-bc6f-ae563fe02e7c/completed?redirectUrl=https://teams.microsoft.com\",\"redirectUrl\": \"\"}";
                }
            }
            else if (request.RequestUri.AbsolutePath.Contains("/api/payment-requests"))
            {
                responseContent = "{\"id\":\"cc28fc5c-b5b4-42ec-b070-4553ca3d8735\",\"sellerId\":\"acct_1Ke2GYQtmDZuG3en\",\"product\":{\"id\":null,\"name\":null,\"description\":\"Yogaclassforbeginners–level0-5\",\"price\":{\"amount\":\"1001\",\"currency\":{\"code\":\"USD\"}}},\"context\":\"prepaidmeeting\",\"trackingId\":null,\"platformType\":\"msteams\",\"status\":\"created\"}";
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