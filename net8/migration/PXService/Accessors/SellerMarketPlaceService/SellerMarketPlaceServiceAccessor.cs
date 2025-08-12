// <copyright file="SellerMarketPlaceServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft 2022 All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.SellerMarketPlaceService
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Model.SellerMarketPlaceService;
    using Newtonsoft.Json;

    public class SellerMarketPlaceServiceAccessor : ISellerMarketPlaceServiceAccessor
    {
        private readonly List<string> passThroughHeaders = new List<string>
        {
            PaymentConstants.PaymentExtendedHttpHeaders.TestHeader
        };

        private HttpClient sellerMarketServiceHttpClient;
        private string serviceBaseUrl;
        private string emulatorBaseUrl;

        public SellerMarketPlaceServiceAccessor(
            string serviceBaseUrl,
            string emulatorBaseUrl,
            HttpMessageHandler messageHandler)
        {
            this.serviceBaseUrl = serviceBaseUrl;
            this.emulatorBaseUrl = emulatorBaseUrl;

            this.sellerMarketServiceHttpClient = new PXTracingHttpClient(Constants.ServiceNames.SellerMarketPlaceService, messageHandler);
            this.sellerMarketServiceHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType));
            this.sellerMarketServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.Connection, PaymentConstants.HttpHeaders.KeepAlive);
            this.sellerMarketServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.KeepAlive, string.Format(PaymentConstants.HttpHeaders.KeepAliveParameter, 60));
        }

        private string BaseUrl
        {
            get
            {
                if ((HttpRequestHelper.IsPXTestRequest()
                    || HttpRequestHelper.IsPXTestRequest("px.sellermarket"))
                    && !string.IsNullOrWhiteSpace(this.emulatorBaseUrl))
                {
                    return this.emulatorBaseUrl;
                }
                else
                {
                    return this.serviceBaseUrl;
                }
            }
        }

        public async Task<Seller> GetSeller(string partner, string paymentProviderId, string sellerId, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.GetSeller, paymentProviderId, sellerId);
            return await this.SendRequest<Seller>(
                HttpMethod.Get,
                "getSeller",
                requestUrl,
                partner,
                traceActivityId);
        }

        private async Task<T> SendRequest<T>(
            HttpMethod method,
            string actionName,
            string requestUrl,
            string partner,
            EventTraceActivity traceActivityId,
            object payload = null,
            IList<KeyValuePair<string, string>> additionalHeaders = null)
        {
            string fullRequestUrl = string.Format("{0}/{1}", this.BaseUrl, requestUrl);
            
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(method, fullRequestUrl))
            {
                requestMessage.IncrementCorrelationVector(traceActivityId);
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString());
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, Guid.NewGuid().ToString());
                requestMessage.Headers.Add("PlatformType", partner);

                // Add action name to the request properties so that this request's OperationName is logged properly
                requestMessage.AddOrReplaceActionName(actionName);
                HttpRequestHelper.TransferTargetHeadersFromIncomingRequestToOutgoingRequest(this.passThroughHeaders, requestMessage);

                if (additionalHeaders != null)
                {
                    foreach (var headerKvp in additionalHeaders)
                    {
                        requestMessage.Headers.Add(headerKvp.Key, headerKvp.Value);
                    }
                }

                if (payload != null)
                {
                    requestMessage.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType);
                }

                using (HttpResponseMessage response = await this.sellerMarketServiceHttpClient.SendAsync(requestMessage))
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
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException(string.Format("Failed to deserialize success response from PayerAuth, Response http status code {0}", response.StatusCode)));
                        }
                    }
                    else
                    {
                        ServiceErrorResponse error = null;
                        try
                        {
                            Model.PayerAuthService.ErrorResponse payAuthError = JsonConvert.DeserializeObject<Model.PayerAuthService.ErrorResponse>(responseMessage);
                            ServiceErrorResponse innerError = new ServiceErrorResponse(payAuthError.ErrorCode, payAuthError.Message);
                            innerError.Source = string.IsNullOrWhiteSpace(innerError.Source) ? Constants.ServiceNames.PaymentThirdPartyService : innerError.Source;
                            error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException(string.Format("Failed to deserialize error response from PayerAuth {0}", responseMessage)));
                        }

                        throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response, HandlingType = ExceptionHandlingPolicy.ByPass });
                    }
                }
            }
        }
    }
}