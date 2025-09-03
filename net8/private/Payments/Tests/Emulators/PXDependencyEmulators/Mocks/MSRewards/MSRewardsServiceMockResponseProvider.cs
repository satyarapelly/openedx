// <copyright file="MSRewardsServiceMockResponseProvider.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Tests.Emulators.PXDependencyEmulators.Mocks
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Test.Common;

    public class MSRewardsServiceMockResponseProvider : IMockResponseProvider
    { 
        public MSRewardsServiceMockResponseProvider()
        {
        }

        public static JObject MSRewardsByOperation
        { 
            get 
            { 
                return JObject.Parse(File.ReadAllText(Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Mocks",
                        "MSRewards",
                        "MSRewardsByOperation.json")));
            } 
        }

        public string MSRewardsResponse { get; set; }

        public void ResetDefaults()
        {
            this.MSRewardsResponse = null;
        }

        public async Task<HttpResponseMessage> GetMatchedMockResponse(HttpRequestMessage request)
        {
            string responseContent = null;
            HttpStatusCode statusCode = HttpStatusCode.OK;
            if (request.RequestUri.AbsolutePath.Contains("?options ="))
            {
                responseContent = JsonConvert.SerializeObject(MSRewardsByOperation["get"]);
                this.MSRewardsResponse = responseContent;
            }
            else if (request.RequestUri.AbsolutePath.Contains("/orders"))
            {
                responseContent = JsonConvert.SerializeObject(MSRewardsByOperation["post"]);
                this.MSRewardsResponse = responseContent;
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