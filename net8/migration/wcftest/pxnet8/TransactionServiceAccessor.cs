// <copyright file="TransactionServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

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
    using Microsoft.Commerce.Payments.PXService.Model.TransactionService;
    using Newtonsoft.Json;

    public class TransactionServiceAccessor : ITransactionServiceAccessor
    {
        private const string TransactionServiceName = "TransactionService";
        private readonly List<string> passThroughHeaders = new List<string> { PaymentConstants.PaymentExtendedHttpHeaders.TestHeader };
        private HttpClient transactionServiceHttpClient;

        private string serviceBaseUrl;
        private string emulatorBaseUrl;
        private string defaultApiVersion;        

        public TransactionServiceAccessor(
            string serviceBaseUrl,
            string emulatorBaseUrl,
            string apiVersion,
            HttpMessageHandler messageHandler)
        {
            this.serviceBaseUrl = serviceBaseUrl;
            this.emulatorBaseUrl = emulatorBaseUrl;
            this.defaultApiVersion = apiVersion;

            this.transactionServiceHttpClient = new PXTracingHttpClient(
                PXCommon.Constants.ServiceNames.TransactionService,
                messageHandler,
                logOutgoingRequestToApplicationInsight: ApplicationInsightsProvider.LogOutgoingOperation);
            this.transactionServiceHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType));
            this.transactionServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.Connection, PaymentConstants.HttpHeaders.KeepAlive);
            this.transactionServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.KeepAlive, string.Format(PaymentConstants.HttpHeaders.KeepAliveParameter, 60));
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

        public async Task<PaymentResource> CreatePaymentObject(string accountId, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.TransactionServiceCreatePaymentObject, accountId);

            return await this.SendPostRequest<PaymentResource>(requestUrl, null, "CreatePaymentObject", traceActivityId);
        }

        public async Task<TransactionResource> ValidateCvv(string accountId, string paymentId, ValidationParameters validationParameters, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.TransactionServiceValidateCvv, accountId, paymentId);

            return await this.SendPostRequest<TransactionResource>(requestUrl, validationParameters, "ValidateCvv", traceActivityId);
        }

        private static void AddHeaders(HttpRequestMessage request, IList<KeyValuePair<string, string>> headers)
        {
            if (headers != null && request != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }
        }

        private async Task<T> SendPostRequest<T>(string url, object request, string actionName, EventTraceActivity traceActivityId, IList<KeyValuePair<string, string>> additionalHeaders = null)
        {
            string fullRequestUrl = string.Format("{0}/{1}", this.BaseUrl, url);
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, fullRequestUrl))
            {
                requestMessage.IncrementCorrelationVector(traceActivityId);
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString());
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, Guid.NewGuid().ToString());
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.ApiVersion, this.defaultApiVersion);

                // Add action name to the request properties so that this request's OperationName is logged properly
                requestMessage.AddOrReplaceActionName(actionName);

                HttpRequestHelper.TransferTargetHeadersFromIncomingRequestToOutgoingRequest(this.passThroughHeaders, requestMessage);

                AddHeaders(requestMessage, additionalHeaders);

                if (request != null)
                {
                    requestMessage.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType);
                }

                using (HttpResponseMessage response = await this.transactionServiceHttpClient.SendAsync(requestMessage))
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
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Failed to deserialize success response from Transaction Service"));
                        }
                    }
                    else
                    {
                        ServiceErrorResponse error = null;
                        try
                        {
                            ServiceErrorResponse innerError = JsonConvert.DeserializeObject<ServiceErrorResponse>(responseMessage);
                            innerError.Source = string.IsNullOrWhiteSpace(innerError.Source) ? TransactionServiceName : innerError.Source;
                            error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Failed to deserialize error response from Transaction Service"));
                        }

                        throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response });
                    }
                }
            }
        }
    }
}
