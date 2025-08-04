// <copyright file="TaxIdServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.V7;
    using Newtonsoft.Json;

    public class TaxIdServiceAccessor : ITaxIdServiceAccessor
    {
        private HttpClient taxIdServiceHttpClient;
        private string serviceBaseUrl = null;

        public TaxIdServiceAccessor(
            string serviceBaseUrl,
            HttpMessageHandler messageHandler)
        {
            this.serviceBaseUrl = serviceBaseUrl;
            this.taxIdServiceHttpClient = new PXTracingHttpClient(
                TaxIdService.V7.Constants.ServiceNames.TaxIdService,
                messageHandler,
                logOutgoingRequestToApplicationInsight: ApplicationInsightsProvider.LogOutgoingOperation);
            this.taxIdServiceHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType));
            this.taxIdServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.Connection, PaymentConstants.HttpHeaders.KeepAlive);
            this.taxIdServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.KeepAlive, string.Format(PaymentConstants.HttpHeaders.KeepAliveParameter, 60));
        }

        public async Task<object[]> GetTaxIds(string accountId, EventTraceActivity traceActivityId)
        {
            object[] taxIds = null;
            string requestUrl = string.Format(V7.Constants.UriTemplate.GetTaxIds, accountId);

            taxIds = await this.SendGetRequest<object[]>(
                requestUrl,
                "2015-03-01",
                "GetTaxIds",
                traceActivityId);
            return taxIds;
        }

        public async Task<TaxData[]> GetTaxIdsByProfileTypeAndCountryWithState(string accountId, string profileType, string country, EventTraceActivity traceActivityId)
        {
            TaxData[] taxIds = null;
            string requestUrl = string.Format(TaxIdService.V7.Constants.UriTemplate.GetTaxIdsWithTypeCountryAndStatus, accountId, profileType, country);

            try
            {
                taxIds = await this.SendGetRequest<TaxData[]>(
                    requestUrl,
                    "2015-08-30",
                    "GetTaxIds",
                    traceActivityId);
                return taxIds;
            }
            catch (FailedOperationException ex)
            {
                if (!ex.Message.Contains(HttpStatusCode.NotFound.ToString()))
                {
                    throw;
                }
            }

            return null;
        }

        private async Task<T> SendGetRequest<T>(string requestUrl, string apiVersion, string actionName, EventTraceActivity traceActivityId, IEnumerable<string> testHeaders = null)
        {
            string fullRequestUrl = string.Format("{0}{1}", this.serviceBaseUrl, requestUrl);
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, fullRequestUrl))
            {
                request.IncrementCorrelationVector(traceActivityId);
                request.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString());
                request.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.ApiVersion, apiVersion);
                request.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CallerName, GlobalConstants.ServiceName);
                if (testHeaders != null)
                {
                    request.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.TestHeader, testHeaders);
                }

                // Add action name to the request properties so that this request's OperationName is logged properly
                request.AddOrReplaceActionName(actionName);

                using (HttpResponseMessage response = await this.taxIdServiceHttpClient.SendAsync(request))
                {
                    string responseMessage = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            return JsonConvert.DeserializeObject<T>(responseMessage);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException(string.Format("Failed to deserialize {0} response message.", TaxIdService.V7.Constants.ServiceNames.TaxIdService)));
                        }
                    }
                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        throw TraceCore.TraceException(traceActivityId, new InvalidOperationException(string.Format("Received a bad request response from {0}: {1}.", TaxIdService.V7.Constants.ServiceNames.TaxIdService, responseMessage ?? string.Empty)));
                    }
                    else
                    {
                        throw TraceCore.TraceException(traceActivityId, new FailedOperationException(string.Format("Received an error response from {0}, response status code: {1}, error: {2}", TaxIdService.V7.Constants.ServiceNames.TaxIdService, response.StatusCode, responseMessage != null ? responseMessage : string.Empty)));
                    }
                }
            }
        }
    }
}
