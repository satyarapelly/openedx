// <copyright file="D365ServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.D365Service
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
    using Microsoft.Commerce.Tracing;
    using Model.D365Service;
    using Newtonsoft.Json;

    public class D365ServiceAccessor : ID365ServiceAccessor
    {
        private const string UserIdFormat = "msa:{0}";

        private readonly List<string> passThroughHeaders = new List<string> { PaymentConstants.PaymentExtendedHttpHeaders.TestHeader };
        private HttpClient d365ServiceHttpClient;
        private string serviceBaseUrl;
        private string emulatorBaseUrl;
        private string apiVersion;

        public D365ServiceAccessor(
            string serviceBaseUrl,
            string emulatorBaseUrl,
            string apiVersion,
            HttpMessageHandler messageHandler)
        {
            this.serviceBaseUrl = serviceBaseUrl;
            this.emulatorBaseUrl = emulatorBaseUrl;
            this.apiVersion = apiVersion;

            this.d365ServiceHttpClient = new PXTracingHttpClient(Constants.ServiceNames.D365Service, messageHandler);
            this.d365ServiceHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType));
            this.d365ServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.Connection, PaymentConstants.HttpHeaders.KeepAlive);
            this.d365ServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.KeepAlive, string.Format(PaymentConstants.HttpHeaders.KeepAliveParameter, 60));
        }

        private string BaseUrl
        {
            get
            {
                if (HttpRequestHelper.IsPXTestRequest() && !string.IsNullOrWhiteSpace(this.emulatorBaseUrl))
                {
                    return this.emulatorBaseUrl;
                }
                else
                {
                    return this.serviceBaseUrl;
                }
            }
        }

        public async Task<PagedResponse<Order>> GetOrder(string puid, string orderId, EventTraceActivity traceActivityId)
        {
            string userId = string.Format(UserIdFormat, puid);
            PagedResponse<Order> orders = await this.SendRequest<PagedResponse<Order>>(
               method: HttpMethod.Get,
               baseUrl: this.BaseUrl,
               actionPath: string.Format(
                   "{0}/users/{1}/orders?orderId={2}",
                   this.apiVersion,
                   userId,
                   orderId),
               actionName: "GetOrderByD365",
               traceActivityId: traceActivityId);
            return orders;
        }

        public async Task<Cart> GetCartByCartId(string cartId, EventTraceActivity traceActivityId)
        {
            Cart cart = await this.SendRequest<Cart>(
               method: HttpMethod.Get,
               baseUrl: this.BaseUrl,
               actionPath: string.Format(
                   "{0}/CartExport/GetCartByCartId?id={1}",
                   this.apiVersion,
                   cartId),
               actionName: "GetCartByCartIdByD365",
               traceActivityId: traceActivityId);
            return cart;
        }

        public async Task<PaymentInstrumentCheckResponse> CheckPaymentInstrument(string userId, string piId, EventTraceActivity traceActivityId)
        {
            PaymentInstrumentCheckResponse checkPiResult = await this.SendRequest<PaymentInstrumentCheckResponse>(
                method: HttpMethod.Get,
                baseUrl: this.BaseUrl,
                actionPath: string.Format(
                    "{0}/users/{1}/orders/paymentinstrumentcheck/{2}",
                    this.apiVersion,
                    userId,
                    piId),
                actionName: "CheckPaymentInstrumentByD365",
                traceActivityId: traceActivityId);

            return checkPiResult;
        }

        private async Task<T> SendRequest<T>(
            HttpMethod method,
            string baseUrl,
            string actionPath,
            string actionName,
            EventTraceActivity traceActivityId,
            object payload = null,
            IList<KeyValuePair<string, string>> additionalHeaders = null)
        {
            string fullRequestUrl = string.IsNullOrWhiteSpace(baseUrl) ? actionPath : string.Format("{0}/{1}", this.BaseUrl, actionPath);
            PaymentsEventSource.Log.PXServiceRequestToD365Service(fullRequestUrl, traceActivityId);
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(method, fullRequestUrl))
            {
                requestMessage.IncrementCorrelationVector(traceActivityId);
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString());
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, Guid.NewGuid().ToString());
                requestMessage.Headers.Add("Request-Id", Guid.NewGuid().ToString());

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

                using (HttpResponseMessage response = await this.d365ServiceHttpClient.SendAsync(requestMessage))
                {
                    string responseMessage = await response.Content.ReadAsStringAsync();
                    PaymentsEventSource.Log.PXServiceTraceResponseFromD365Service(response.StatusCode.ToString(), responseMessage, traceActivityId);

                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            return JsonConvert.DeserializeObject<T>(responseMessage);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException(string.Format("Failed to deserialize success response from D365Service, Response http status code {0}", response.StatusCode)));
                        }
                    }
                    else
                    {
                        ServiceErrorResponse error = null;
                        try
                        {
                            ApiError d365ErrorResponse = JsonConvert.DeserializeObject<ApiError>(responseMessage);
                            ServiceErrorResponse innerError = new ServiceErrorResponse()
                            {
                                ErrorCode = d365ErrorResponse.Code.ToString(),
                                Message = d365ErrorResponse.Message,
                                Source = Constants.ServiceNames.D365Service
                            }; 
                            error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Failed to deserialize error response from D365Service"));
                        }

                        throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response });
                    }
                }
            }
        }
    }
}