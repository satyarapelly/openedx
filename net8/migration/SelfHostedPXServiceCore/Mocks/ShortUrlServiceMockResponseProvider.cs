// <copyright company="Microsoft Corporation">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace SelfHostedPXServiceCore.Mocks
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.PXService.Model.ShortURLService;
    using Newtonsoft.Json;
    using Test.Common;

    public class ShortUrlServiceMockResponseProvider : IMockResponseProvider
    {
        public void ResetDefaults()
        {
        }

        public async Task<HttpResponseMessage> GetMatchedMockResponse(HttpRequestMessage request)
        {
            if (request.Method == HttpMethod.Post)
            {
                var tinyUrl = new CreateShortURLResponse("test", "https://testshorturl.ms/test", new DateTime());

                return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(tinyUrl), System.Text.Encoding.UTF8, GlobalConstants.MediaType.JsonApplicationType)
                });
            }
            else if (request.Method == HttpMethod.Delete)
            {
                return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
            }

            HttpStatusCode statusCode = HttpStatusCode.OK;
            return await Task.FromResult(new HttpResponseMessage(statusCode));
        }
    }
}