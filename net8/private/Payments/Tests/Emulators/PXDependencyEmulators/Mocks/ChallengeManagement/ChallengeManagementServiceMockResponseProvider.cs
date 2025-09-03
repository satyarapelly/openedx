// <copyright file="ChallengeManagementServiceMockResponseProvider.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Test.Common;

    public class ChallengeManagementServiceMockResponseProvider : IMockResponseProvider
    {
        public void ResetDefaults()
        {
        }

        public async Task<HttpResponseMessage> GetMatchedMockResponse(HttpRequestMessage request)
        {
            string responseContent = "[]";
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    responseContent,
                    System.Text.Encoding.UTF8,
                    "application/json")
            };

            if (request.RequestUri.AbsolutePath.Contains("challenge/create"))
            {
                var challengePidl = File.ReadAllText(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Mocks",
                    "ChallengeManagement",
                    "ArkoseChallenge.json"));

                responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                    challengePidl,
                    System.Text.Encoding.UTF8,
                    "application/json")
                };
            }
            else if (request.RequestUri.AbsolutePath.Contains("challengesession/create"))
            {
                string getSessionResponse = "{\"session_id\":\"554403e2-96ce-4c9e-aa9a-45b4c60f3f19\",\"session_type\":\"PXAddPISession\",\"status\":\"Active\",\"parent_session_id\":null,\"child_sessions\":null,\"session_data_hash\":\"9249ca5a047185daf4d9601b210329e6c129a3e3cf6a28033aa5d4079967139d\",\"session_data\":\"{\\r\\n\\t\\\"accountId\\\":\\\"Account001\\\",\\r\\n\\t\\\"card_number\\\":\\\"4012888888889995\\\",\\r\\n\\t\\\"partner\\\":\\\"amc\\\",\\r\\n\\t\\\"operation\\\":\\\"add\\\",\\r\\n\\t\\\"family\\\":\\\"credit_card\\\",\\r\\n\\t\\\"type\\\":\\\"visa\\\",\\r\\n\\t\\\"language\\\":\\\"en-us\\\",\\r\\n\\t\\\"country\\\":\\\"usa\\\",\\r\\n\\t\\\"challengeRequired\\\":\\\"true\\\",\\r\\n\\t\\\"challengeCompleted\\\":\\\"false\\\",\\r\\n\\t\\\"challengeRetries\\\":1,\\r\\n\\t\\\"Sec-Ch-Ua\\\":\\\"Not.A/Brand;v=8,Chromium;v=114,GoogleChrome;v=114\\\",\\r\\n\\t\\\"Sec-Ch-Ua-Mobile\\\":\\\"?0,Sec-Ch-Ua-Platform:Windows\\\",\\r\\n\\t\\\"User-Agent\\\":\\\"Mozilla/5.0(WindowsNT10.0;Win64;x64)AppleWebKit/537.36(KHTML,likeGecko)Chrome/114.0.0.0Safari/537.36\\\",\\r\\n\\t\\\"client_ip\\\":\\\"1.1.0.0\\\"\\r\\n}\",\"session_length\":20,\"session_sliding_expiration\":true,\"session_expires_at\":\"2023-10-16T23:07:56.3759138Z\",\"created_by\":\"PXAddPISession\",\"updated_by\":\"PXAddPISession\",\"created_date\":\"2023-10-16T22:42:43.8934613Z\",\"updated_date\":\"2023-10-16T22:47:56.3759934Z\"}";
                responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                    getSessionResponse,
                    System.Text.Encoding.UTF8,
                    "application/json")
                };
            }
            else if (request.RequestUri.AbsolutePath.Contains("/get"))
            {
                string getSessionResponse = "{\"session_id\":\"554403e2-96ce-4c9e-aa9a-45b4c60f3f19\",\"session_type\":\"PXAddPISession\",\"status\":\"Active\",\"parent_session_id\":null,\"child_sessions\":null,\"session_data_hash\":\"9249ca5a047185daf4d9601b210329e6c129a3e3cf6a28033aa5d4079967139d\",\"session_data\":\"{\\r\\n\\t\\\"accountId\\\":\\\"Account001\\\",\\r\\n\\t\\\"card_number\\\":\\\"4012888888889995\\\",\\r\\n\\t\\\"partner\\\":\\\"amc\\\",\\r\\n\\t\\\"operation\\\":\\\"add\\\",\\r\\n\\t\\\"family\\\":\\\"credit_card\\\",\\r\\n\\t\\\"type\\\":\\\"visa\\\",\\r\\n\\t\\\"language\\\":\\\"en-us\\\",\\r\\n\\t\\\"country\\\":\\\"usa\\\",\\r\\n\\t\\\"challengeRequired\\\":\\\"true\\\",\\r\\n\\t\\\"challengeCompleted\\\":\\\"false\\\",\\r\\n\\t\\\"challengeRetries\\\":1,\\r\\n\\t\\\"Sec-Ch-Ua\\\":\\\"Not.A/Brand;v=8,Chromium;v=114,GoogleChrome;v=114\\\",\\r\\n\\t\\\"Sec-Ch-Ua-Mobile\\\":\\\"?0,Sec-Ch-Ua-Platform:Windows\\\",\\r\\n\\t\\\"User-Agent\\\":\\\"Mozilla/5.0(WindowsNT10.0;Win64;x64)AppleWebKit/537.36(KHTML,likeGecko)Chrome/114.0.0.0Safari/537.36\\\",\\r\\n\\t\\\"client_ip\\\":\\\"1.1.0.0\\\"\\r\\n}\",\"session_length\":20,\"session_sliding_expiration\":true,\"session_expires_at\":\"2023-10-16T23:07:56.3759138Z\",\"created_by\":\"PXAddPISession\",\"updated_by\":\"PXAddPISession\",\"created_date\":\"2023-10-16T22:42:43.8934613Z\",\"updated_date\":\"2023-10-16T22:47:56.3759934Z\"}";
                responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                    getSessionResponse,
                    System.Text.Encoding.UTF8,
                    "application/json")
                };
            }
            else if (request.RequestUri.AbsolutePath.Contains("/update"))
            {
                string getSessionResponse = "{\"session_id\":\"554403e2-96ce-4c9e-aa9a-45b4c60f3f19\",\"session_type\":\"PXAddPISession\",\"status\":\"Active\",\"parent_session_id\":null,\"child_sessions\":null,\"session_data_hash\":\"e57813c8a6eff71930bf4bb88948a72d29d1495b929df6256067d2275d3d1ecb\",\"session_data\":\"{\\r\\n\\t\\\"accountId\\\":\\\"Account001\\\",\\r\\n\\t\\\"card_number\\\":\\\"4012888888889995\\\",\\r\\n\\t\\\"partner\\\":\\\"amc\\\",\\r\\n\\t\\\"operation\\\":\\\"add\\\",\\r\\n\\t\\\"family\\\":\\\"credit_card\\\",\\r\\n\\t\\\"type\\\":\\\"visa\\\",\\r\\n\\t\\\"language\\\":\\\"en-us\\\",\\r\\n\\t\\\"country\\\":\\\"usa\\\",\\r\\n\\t\\\"challengeRequired\\\":\\\"false\\\",\\r\\n\\t\\\"challengeCompleted\\\":\\\"false\\\",\\r\\n\\t\\\"challengeRetries\\\":1,\\r\\n\\t\\\"Sec-Ch-Ua\\\":\\\"Not.A/Brand;v=8,Chromium;v=114,GoogleChrome;v=114\\\",\\r\\n\\t\\\"Sec-Ch-Ua-Mobile\\\":\\\"?0,Sec-Ch-Ua-Platform:Windows\\\",\\r\\n\\t\\\"User-Agent\\\":\\\"Mozilla/5.0(WindowsNT10.0;Win64;x64)AppleWebKit/537.36(KHTML,likeGecko)Chrome/114.0.0.0Safari/537.36\\\",\\r\\n\\t\\\"client_ip\\\":\\\"1.1.0.0\\\"\\r\\n}\",\"session_length\":20,\"session_sliding_expiration\":true,\"session_expires_at\":\"2023-10-16T23:10:13.7222708Z\",\"created_by\":\"PXAddPISession\",\"updated_by\":\"PXAddPISession\",\"created_date\":\"2023-10-16T22:42:43.8934613Z\",\"updated_date\":\"2023-10-16T22:50:13.7222848Z\"}";
                responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                    getSessionResponse,
                    System.Text.Encoding.UTF8,
                    "application/json")
                };
            }
            else if (request.RequestUri.AbsolutePath.Contains("/status"))
            {
                string getSessionResponse = "{\"passed\":true}";
                responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                    getSessionResponse,
                    System.Text.Encoding.UTF8,
                    "application/json")
                };
            }

            return await Task.FromResult(responseMessage);
        }
    }
}