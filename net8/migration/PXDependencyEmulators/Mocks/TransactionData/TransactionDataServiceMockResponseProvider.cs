// <copyright file="TransactionDataServiceMockResponseProvider.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Test.Common;

    public class TransactionDataServiceMockResponseProvider : IMockResponseProvider
    { 
        public TransactionDataServiceMockResponseProvider()
        {
        }

        public string TransactionDataResponse { get; set; }

        public void ResetDefaults()
        {
            this.TransactionDataResponse = null;
        }

        public async Task<HttpResponseMessage> GetMatchedMockResponse(HttpRequestMessage request)
        {
            string responseContent = null;
            HttpStatusCode statusCode = HttpStatusCode.OK;
            if (request.RequestUri.ToString().Contains("provision"))
            {
                responseContent = "{\"add\":{\"template\":\"OnePage\",\"features\":{\"addressValidation\":{\"applicableMarkets\":[\"us\",\"ca\"]},\"hideElement\":{\"applicableMarkets\":[],\"displayCustomizationDetail\":[{\"hideFirstAndLastNameForCompletePrerequisites\":true}]},\"singleMarketDirective\":{\"applicableMarkets\":[\"fr\",\"gb\"]}}},\"select\":{\"template\":\"selectpmbuttonlist\",\"features\":{\"paymentMethodGrouping\":{\"applicableMarkets\":[]}}},\"validateinstance\":{\"template\":\"defaultTemplate\"},\"handlepaymentchallenge\":{\"template\":\"defaultTemplate\"}}";
                this.TransactionDataResponse = responseContent;
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