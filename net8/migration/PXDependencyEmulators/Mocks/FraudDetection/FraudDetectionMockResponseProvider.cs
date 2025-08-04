// <copyright file="FraudDetectionMockResponseProvider.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Test.Common;

    public class FraudDetectionMockResponseProvider : IMockResponseProvider
    {
        public string Response { get; set; }

        public void ResetDefaults()
        {
            this.Response = null;
        }

        public async Task<HttpResponseMessage> GetMatchedMockResponse(HttpRequestMessage request)
        {
            string responseContent = null;
            HttpStatusCode statusCode = HttpStatusCode.OK;

            if (request.RequestUri.ToString().Contains("botcheck"))
            {
                responseContent = "{\"activityId\":\"id12345\",\"paymentInstrumentIds\":[],\"riskScore\":5,\"recommendation\":\"Approved\",\"reason\":\"Low risk based on transaction history\"}";
                this.Response = responseContent;
            }

            if (string.IsNullOrEmpty(responseContent))
            {
                return null;
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