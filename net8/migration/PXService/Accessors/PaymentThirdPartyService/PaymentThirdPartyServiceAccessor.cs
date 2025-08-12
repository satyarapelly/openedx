// <copyright file="PaymentThirdPartyServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
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
    using Newtonsoft.Json;
    using ThirdPartyService = Model.PaymentThirdPartyService;

    public class PaymentThirdPartyServiceAccessor : IPaymentThirdPartyServiceAccessor
    {
        private readonly List<string> passThroughHeaders = new List<string> 
        { 
            PaymentConstants.PaymentExtendedHttpHeaders.TestHeader 
        };

        private HttpClient paymentThirdPartyServiceHttpClient;
        private string serviceBaseUrl;
        private string emulatorBaseUrl;
        private string apiVersion;

        public PaymentThirdPartyServiceAccessor(
            string serviceBaseUrl,
            string emulatorBaseUrl,
            string apiVersion,
            HttpMessageHandler messageHandler)
        {
            this.serviceBaseUrl = serviceBaseUrl;
            this.emulatorBaseUrl = emulatorBaseUrl;

            this.paymentThirdPartyServiceHttpClient = new PXTracingHttpClient(Constants.ServiceNames.PaymentThirdPartyService, messageHandler);
            this.paymentThirdPartyServiceHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType));
            this.paymentThirdPartyServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.Connection, PaymentConstants.HttpHeaders.KeepAlive);
            this.paymentThirdPartyServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.KeepAlive, string.Format(PaymentConstants.HttpHeaders.KeepAliveParameter, 60));

            this.apiVersion = apiVersion;
        }

        private string BaseUrl
        {
            get
            {
                if ((HttpRequestHelper.IsPXTestRequest() 
                    || HttpRequestHelper.IsPXTestRequest("px.3pp")) 
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

        public async Task<ThirdPartyService.Checkout> GetCheckout(
            string paymentProviderId, 
            string checkoutId, 
            EventTraceActivity traceActivityId) 
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.GetCheckout, paymentProviderId, checkoutId);
            return await this.SendRequest<ThirdPartyService.Checkout>(
                HttpMethod.Get,
                "GetCheckout",
                requestUrl, 
                traceActivityId);
        }

        public async Task<ThirdPartyService.PaymentRequest> GetPaymentRequest(
            string paymentProviderId, 
            string paymentRequestId, 
            EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.GetPaymentRequest, paymentProviderId, paymentRequestId);
            return await this.SendRequest<ThirdPartyService.PaymentRequest>(
                HttpMethod.Get,
                "GetPaymentRequest",
                requestUrl,
                traceActivityId);
        }

        public async Task<ThirdPartyService.Checkout> Charge(
            string paymentProviderId, 
            ThirdPartyService.CheckoutCharge checkoutCharge, 
            EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.CheckoutCharge, paymentProviderId, checkoutCharge.CheckoutId);
            return await this.SendRequest<ThirdPartyService.Checkout>(
                HttpMethod.Post,
                "CheckoutCharge",
                requestUrl,
                traceActivityId,
                checkoutCharge);
        }

        private async Task<T> SendRequest<T>(
            HttpMethod method, 
            string actionName, 
            string requestUrl,
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
                
                // Add action name to the request properties so that this request's OperationName is logged properly
                requestMessage.AddOrReplaceActionName(actionName);
                HttpRequestHelper.TransferTargetHeadersFromIncomingRequestToOutgoingRequest(this.passThroughHeaders, requestMessage);
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.ApiVersion, this.apiVersion);

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

                using (HttpResponseMessage response = await this.paymentThirdPartyServiceHttpClient.SendAsync(requestMessage))
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
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException(string.Format("Failed to deserialize success response from {0}, Response http status code {1}", Constants.ServiceNames.PaymentThirdPartyService, response.StatusCode)));
                        }
                    }
                    else
                    {
                        ServiceErrorResponse error = null;
                        try
                        {
                            ServiceErrorResponse innerError = JsonConvert.DeserializeObject<ServiceErrorResponse>(responseMessage);
                            innerError.Source = string.IsNullOrWhiteSpace(innerError.Source) ? Constants.ServiceNames.PaymentThirdPartyService : innerError.Source;
                            error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException(string.Format("Failed to deserialize error response from {0} {1}", Constants.ServiceNames.PaymentThirdPartyService, responseMessage)));
                        }

                        throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response, HandlingType = ExceptionHandlingPolicy.ByPass });
                    }
                }
            }
        }
    }
}