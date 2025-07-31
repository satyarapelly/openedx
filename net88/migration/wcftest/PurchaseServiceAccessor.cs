// <copyright file="PurchaseServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using HttpRequest = Microsoft.AspNetCore.Http.HttpRequest;
    using HttpResponse = Microsoft.AspNetCore.Http.HttpResponse;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Internal.AntiSSRF;
    using Model.PurchaseService;
    using Newtonsoft.Json;

    public class PurchaseServiceAccessor : IPurchaseServiceAccessor
    {
        private const string UserIdFormat = "msa:{0}";
        private const string PXClientName = "PaymentExperienceService";
        private const string CSVPIType = "Token";
        private const string OrderStatePurchased = "Purchased";

        private readonly List<string> passThroughHeaders = new List<string> { PaymentConstants.PaymentExtendedHttpHeaders.TestHeader };
        private HttpClient purchaseServiceHttpClient;
        private string serviceBaseUrl;
        private string emulatorBaseUrl;
        private string defaultApiVersion;

        public PurchaseServiceAccessor(
            string serviceBaseUrl,
            string emulatorBaseUrl,
            string apiVersion,
            HttpMessageHandler messageHandler)
        {
            this.serviceBaseUrl = serviceBaseUrl;
            this.emulatorBaseUrl = emulatorBaseUrl;
            this.defaultApiVersion = apiVersion;

            this.purchaseServiceHttpClient = new PXTracingHttpClient(Constants.ServiceNames.PurchaseService, messageHandler);
            this.purchaseServiceHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType));
            this.purchaseServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.Connection, PaymentConstants.HttpHeaders.KeepAlive);
            this.purchaseServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.KeepAlive, string.Format(PaymentConstants.HttpHeaders.KeepAliveParameter, 60));
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

        public async Task<Subscription> GetSubscription(string userId, string subscriptionId, EventTraceActivity traceActivityId)
        {
            EventTraceActivity traceActivityID = new EventTraceActivity();
            Subscription subscription = await this.SendRequest<Subscription>(
                method: HttpMethod.Get,
                baseUrl: this.BaseUrl,
                actionPath: string.Format(
                    "{0}/users/{1}/recurrences/{2}",
                    GlobalConstants.PurchaseApiVersions.V8,
                    userId,
                    subscriptionId),
                actionName: "GetSubscription",
                traceActivityId: traceActivityId,
                payload: null,
                apiVersion: this.defaultApiVersion);

            return subscription;
        }

        public async Task<Subscriptions> ListSubscriptions(string userId, int maxPageSize, EventTraceActivity traceActivityId)
        {
            EventTraceActivity traceActivityID = new EventTraceActivity();
            Subscriptions subscriptions = await this.SendRequest<Subscriptions>(
                method: HttpMethod.Get,
                baseUrl: this.BaseUrl,
                actionPath: string.Format(
                    "{0}/users/{1}/recurrences/?$maxpagesize={2}",
                    GlobalConstants.PurchaseApiVersions.V8,
                    userId,
                    maxPageSize),
                actionName: "ListSubscriptions",
                traceActivityId: traceActivityId,
                payload: null,
                apiVersion: this.defaultApiVersion);

            return subscriptions;
        }

        public async Task<Orders> ListOrders(
            string userId, 
            int maxPageSize, 
            DateTime startTime, 
            DateTime? endTime, 
            List<string> validOrderStates,
            EventTraceActivity traceActivityId)
        {
            EventTraceActivity traceActivityID = new EventTraceActivity();
            var headers = new List<KeyValuePair<string, string>>();

            string dateRange = string.Format("{0}..{1}", startTime.ToString("yyyy-MM-dd"), endTime?.ToString("yyyy-MM-dd"));

            Orders orders = await this.SendRequest<Orders>(
                method: HttpMethod.Get,
                baseUrl: this.BaseUrl,
                actionPath: string.Format(
                    "{0}/users/{1}/orders?$maxpagesize={2}&orderSubmitDate={3}&orderState={4}",
                    GlobalConstants.PurchaseApiVersions.V7,
                    userId,
                    maxPageSize,
                    dateRange,
                    string.Join(",", validOrderStates)),
                actionName: "ListOrders",
                traceActivityId: traceActivityId,
                payload: null,
                apiVersion: this.defaultApiVersion,
                additionalHeaders: headers);

            return orders;
        }

        public async Task<Orders> ListOrders(
            string userId,
            string nextLink,
            EventTraceActivity traceActivityId,
            List<string> exposedFlightFeatures)
        {
            EventTraceActivity traceActivityID = new EventTraceActivity();
            var headers = new List<KeyValuePair<string, string>>();

            // Validate the URL using AntiSSRF library
            if (exposedFlightFeatures != null && exposedFlightFeatures.Contains(Flighting.Features.PXEnableSSRFPolicy, StringComparer.OrdinalIgnoreCase))
            {
                Uri baseurl = new Uri(this.BaseUrl);
                if (!URIValidate.InDomain(nextLink, baseurl.Host))
                {
                    throw new Exception("The next link URL is not in the trusted domain.");
                }
            }

            Orders orders = await this.SendRequest<Orders>(
                method: HttpMethod.Get,
                baseUrl: null,
                actionPath: nextLink,
                actionName: "ListOrdersNextPage",
                traceActivityId: traceActivityId,
                payload: null,
                apiVersion: this.defaultApiVersion,
                additionalHeaders: headers);

            return orders;
        }

        public async Task<Order> GetOrder(string puid, string orderId, EventTraceActivity traceActivityId)
        {
            EventTraceActivity traceActivityID = new EventTraceActivity();
            var headers = new List<KeyValuePair<string, string>>();
            string userId = string.Format(UserIdFormat, puid);
            Order orders = await this.SendRequest<Order>(
               method: HttpMethod.Get,
               baseUrl: this.BaseUrl,
               actionPath: string.Format(
                   "{0}/users/{1}/orders/{2}",
                   GlobalConstants.PurchaseApiVersions.V7,
                   userId,
                   orderId),
               actionName: "GetOrder",
               traceActivityId: traceActivityId,
               payload: null,
               apiVersion: this.defaultApiVersion,
               additionalHeaders: headers);
            return orders;
        }

        public async Task<PaymentInstrumentCheckResponse> CheckPaymentInstrument(string userId, string piId, EventTraceActivity traceActivityId)
        {
            EventTraceActivity traceActivityID = new EventTraceActivity();
            var headers = new List<KeyValuePair<string, string>>();
            PaymentInstrumentCheckResponse checkPiResult = await this.SendRequest<PaymentInstrumentCheckResponse>(
                method: HttpMethod.Get,
                baseUrl: this.BaseUrl,
                actionPath: string.Format(
                    "{0}/users/{1}/paymentinstruments/{2}/check",
                    GlobalConstants.PurchaseApiVersions.V7,
                    userId,
                    piId),
                actionName: "CheckPaymentInstrument",
                traceActivityId: traceActivityId,
                payload: null,
                apiVersion: this.defaultApiVersion,
                additionalHeaders: headers);

            return checkPiResult;
        }

        public async Task<Order> RedeemCSVToken(string puid, string csvToken, string market, string language, EventTraceActivity traceActivityId)
        {
            EventTraceActivity traceActivityID = new EventTraceActivity();
            var headers = new List<KeyValuePair<string, string>>();
            string userId = string.Format(UserIdFormat, puid);
            CreateOrderRequest createOrderRequest = new CreateOrderRequest()
            {
                OrderId = Guid.NewGuid().ToString(),
                OrderState = OrderStatePurchased,
                BillingInformation = new BillingInformation()
                {
                    PaymentInstrumentId = csvToken,
                    PaymentInstrumentType = CSVPIType,
                    SessionId = Guid.NewGuid().ToString(),
                },
                Items = new List<CreateOrderLineItem>(),
                ClientContext = new ClientContext()
                {
                    Client = PXClientName
                },
                Market = market,
                Language = language,
            };

            Order order = await this.SendRequest<Order>(
               method: HttpMethod.Post,
               baseUrl: this.BaseUrl,
               actionPath: string.Format(
                   "{0}/users/{1}/orders",
                   GlobalConstants.PurchaseApiVersions.V7,
                   userId),
               actionName: "CreateOrder",
               traceActivityId: traceActivityId,
               payload: createOrderRequest,
               apiVersion: this.defaultApiVersion,
               additionalHeaders: headers);

            return order;
        }

        private async Task<T> SendRequest<T>(
            HttpMethod method,
            string baseUrl,
            string actionPath,
            string actionName,
            EventTraceActivity traceActivityId,
            object payload = null,
            IList<KeyValuePair<string, string>> additionalHeaders = null,
            string apiVersion = null)
        {
            string fullRequestUrl = string.IsNullOrWhiteSpace(baseUrl) ? actionPath : string.Format("{0}/{1}", this.BaseUrl, actionPath);
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

                // CodeQL [SM03781] Safe to use. We have implemented the fix in line 144-152.
                using (HttpResponseMessage response = await this.purchaseServiceHttpClient.SendAsync(requestMessage))
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
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException(string.Format("Failed to deserialize success response from CatalogService, Response http status code {0}", response.StatusCode)));
                        }
                    }
                    else
                    {
                        ServiceErrorResponse error = null;
                        try
                        {
                            ServiceErrorResponse innerError = JsonConvert.DeserializeObject<ServiceErrorResponse>(responseMessage);
                            innerError.Source = string.IsNullOrWhiteSpace(innerError.Source) ? Constants.ServiceNames.PurchaseService : innerError.Source;
                            error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Failed to deserialize error response from CatalogService"));
                        }

                        throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response });
                    }
                }
            }
        }
    }
}