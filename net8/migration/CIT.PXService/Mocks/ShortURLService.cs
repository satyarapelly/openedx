// <copyright company="Microsoft Corporation">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace CIT.PXService.Mocks
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using static CIT.PXService.GlobalConstants;

    public class ShortURLService : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Post)
            {
                var tinyUrl = new Uri("https://www.bing.com/");
                return new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent(
                        JsonConvert.SerializeObject(tinyUrl),
                        System.Text.Encoding.UTF8,
                        MediaType.JsonApplicationType)
                };
            }
            else if (request.Method == HttpMethod.Delete)
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
