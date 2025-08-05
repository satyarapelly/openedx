// <copyright company="Microsoft Corporation">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Test.Common;

    public class NetworkTokenizationServiceMockResponseProvider : IMockResponseProvider
    {
        public void ResetDefaults()
        {
        }

        public async Task<HttpResponseMessage> GetMatchedMockResponse(HttpRequestMessage request)
        {
            var response = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Mocks\NetworkTokenizationServiceMockResponse.json"));

            if (request.RequestUri.ToString().Contains("/tokens?externalCardReference"))
            {
                response = "{\"tokens\":[{\"networkTokenId\":\"19c6f6df-8b31-4d29-b594-67621438e8d2\",\"networkTokenUsage\":\"firstPartyMerchant\",\"externalCardReference\":\"abb4aefb-10ec-4770-ba3f-820e52ed5bdf\",\"externalCardReferenceType\":\"paymentInstrumentId\",\"tokenInfo\":{\"tokenStatus\":\"active\",\"lastFourDigits\":\"9781\",\"expirationDate\":{\"year\":2027,\"month\":12}},\"clientDeviceInfo\":{\"bindingRequired\":false,\"deviceEnrolled\":false,\"bindingStatus\":\"unknown\"},\"cardMetadata\":{\"cardArtURL\":\"https://cardartimages.microsoft-int.com/cardartimages/mc/6713d73d-a701-4bd2-bc9b-2e98940de9c7.png\",\"mediumCardArtURL\":\"https://cardartimages.microsoft-int.com/cardartimages/mc/medium-6713d73d-a701-4bd2-bc9b-2e98940de9c7.png\",\"thumbnailCardArtURL\":\"https://cardartimages.microsoft-int.com/cardartimages/mc/thumbnail-6713d73d-a701-4bd2-bc9b-2e98940de9c7.png\",\"shortDescription\":\"MasterCard Test Bank\",\"longDescription\":\"Test Bank for MasterCard MTF\",\"isCoBranded\":true,\"coBrandedName\":\"test co-brand\",\"foregroundColor\":\"0F0F0F\",\"latestRefreshTimestamp\":\"2025-06-04T20:47:33.4997764Z\"},\"srcFlowId\":null}]}";
            }
            else if (request.RequestUri.ToString().Contains("/tokenizable?bankIdentificationNumber"))
            {
                response = "{\"tokenizable\":true}";
            }            
            else if (request.RequestUri.ToString().Contains("/passkeys/authenticate"))
            {
                response = "{\"authContext\":{\"endpoint\":\"\",\"payload\":\"\",\"action\":\"REGISTER\",\"platformType\":\"WEB\"},\"action\":\"REGISTER_DEVICE_BINDING\"}";
            }
            else if (request.RequestUri.ToString().Contains("/passkeys/setup"))
            {
                response = "{\"authContext\":{\"endpoint\":\"\",\"payload\":\"\",\"action\":\"REGISTER\",\"platformType\":\"WEB\"},\"action\":\"REGISTER_DEVICE_BINDING\"}";
            }
            else if (request.RequestUri.ToString().Contains("/devicebinding/fido"))
            {
                response = "{\"status\":\"challenge\",\"challengeId\":\"c6f4cba0-2c09-4e4c-a6cf-20b3a17ff0dd\",\"challengeMethods\":[{\"challengeMethodType\":\"appToApp\",\"challengeValue\":\"test\",\"challengeMethodId\":\"Zjg5YjY2NjhlMzU3ZjdkN2UzZDQxZmRkZmE1NDU2MDI=\"},{\"challengeMethodType\":\"customerService\",\"challengeValue\":\"1800123456\",\"challengeMethodId\":\"YTdiZGM4NGM4ZWYwMjkyZjhiMDExZWQ5MWY3NTE3MDI=\"},{\"challengeMethodType\":\"appToApp\",\"challengeValue\":\"Verify Online with Bank\",\"challengeMethodId\":\"ZWY2NTkyZDFiNjZlZTMwZGQyNjg1ZDY3NDY0YTc1MDE=\"}]}";
            }            
            else if (request.RequestUri.ToString().Contains("/devicebinding/challenges") && request.RequestUri.ToString().Contains("/request"))
            {
                response = "{\"maxChallengeAttempts\":3,\"maxValidationAttempts\":3,\"challengeTimeout\":5}";
            }
            else if (request.RequestUri.ToString().Contains("/devicebinding/challenges") && request.RequestUri.ToString().Contains("/validate"))
            {
                response = string.Empty;
            }
            else if (request.Method == HttpMethod.Post && request.RequestUri.ToString().Contains("/tokens"))
            {
                response = "{\"networkTokenId\":\"af24631b-967d-498c-9171-128c1a5261c6\",\"networkTokenUsage\":\"ecomMerchant\",\"externalCardReference\":\"visaAccount002-Pi001-Visa-AgenticPayment\",\"externalCardReferenceType\":\"paymentInstrumentId\",\"tokenInfo\":{\"tokenStatus\":\"active\",\"lastFourDigits\":\"0673\",\"expirationDate\":{\"year\":2030,\"month\":12}},\"clientDeviceInfo\":{\"bindingRequired\":true,\"deviceEnrolled\":false,\"bindingStatus\":\"unknown\"},\"cardMetadata\":null,\"srcFlowId\":null}";
            }

            return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(response, Encoding.UTF8, GlobalConstants.MediaType.JsonApplicationType)
            });
        }
    }
}