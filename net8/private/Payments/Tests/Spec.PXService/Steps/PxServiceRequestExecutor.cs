// <copyright file="PxRequestExecutor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Spec.PXService.Steps
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using SelfHostedPXServiceCore;
    using SelfHostedPXServiceCore.Mocks;
    using Tests.Common.Model;

    public class PxRequestExecutor
    {
        public static SelfHostedPxService SelfHostedPxService { get; private set; }

        public static PXServiceSettings PXSettings { get; private set; }

        public PXServiceRequestBuilder PxServiceRequestBuilder { get; set; }

        static PxRequestExecutor()
        {
            SelfHostedPxService = SelfHostedPxService.StartInMemory(null, false, true);
            PXSettings = SelfHostedPxService.PXSettings;
        }

        public PxRequestExecutor()
        {
            SelfHostedPxService.ResetDependencies();
            this.PxServiceRequestBuilder = new PXServiceRequestBuilder(SelfHostedPxService.PxHostableService.BaseUri);
        }
        
        public async Task<string> ExecuteRequest(string area, HttpMethod method = null)
        {
            foreach (var flight in this.PxServiceRequestBuilder.GetFlights())
            {
                SelfHostedPxService.PXFlightHandler.AddToEnabledFlights(flight);
            }
            
            var requestMethod = method ?? HttpMethod.Get;

            HttpRequestMessage request = new HttpRequestMessage(requestMethod, this.PxServiceRequestBuilder.GetRequestUri(area));

            foreach (var header in this.PxServiceRequestBuilder.GetHeaders())
            {
                request.Headers.Add(header.Key, header.Value);
            }

            var response = await SelfHostedPxService.PxHostableService.HttpSelfHttpClient.SendAsync(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<T> ExecuteRequest<T>(string area, HttpMethod method = null, JsonConverter[] converters = null)
        {
            var json = await ExecuteRequest(area, method);

            if (converters == null)
            {
                converters = new JsonConverter[]
                {
                    new DisplayHintDeserializer(),
                    new PidlObjectDeserializer()
                };
            }

            return JsonConvert.DeserializeObject<T>(json, converters);
        }
    }
}