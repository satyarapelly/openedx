// <copyright file="CatalogServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

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
    using Model.CatalogService;
    using Newtonsoft.Json;

    public class CatalogServiceAccessor : ICatalogServiceAccessor
    {
        private readonly List<string> passThroughHeaders = new List<string> { PaymentConstants.PaymentExtendedHttpHeaders.TestHeader };
        private HttpClient catalogServiceHttpClient;
        private string serviceBaseUrl;
        private string emulatorBaseUrl;
        private string defaultApiVersion;

        private DomainData euDirective;

        public CatalogServiceAccessor(
            string serviceBaseUrl,
            string emulatorBaseUrl,
            string apiVersion,
            HttpMessageHandler messageHandler)
        {
            this.serviceBaseUrl = serviceBaseUrl;
            this.emulatorBaseUrl = emulatorBaseUrl;
            this.defaultApiVersion = apiVersion;

            this.catalogServiceHttpClient = new PXTracingHttpClient(Constants.ServiceNames.CatalogService, messageHandler);
            this.catalogServiceHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType));
            this.catalogServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.Connection, PaymentConstants.HttpHeaders.KeepAlive);
            this.catalogServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.KeepAlive, string.Format(PaymentConstants.HttpHeaders.KeepAliveParameter, 60));
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

        public async Task<Catalog> GetProducts(List<string> productIds, string market, string language, string fieldsTemplate, string actionFilter, EventTraceActivity traceActivityId)
        {
            string actionFilterParam = string.IsNullOrEmpty(actionFilter) ? string.Empty : $"&actionFilter={actionFilter}";
            string fieldsTemplateParam = string.IsNullOrEmpty(fieldsTemplate) ? string.Empty : $"&fieldsTemplate={fieldsTemplate}";
            Catalog catalog = await this.SendRequest<Catalog>(
                method: HttpMethod.Get,
                actionPath: string.Format(
                    "{0}/products?bigIds={1}{2}&market={3}&languages={4}{5}&catalogIds=1",
                    GlobalConstants.CatalogApiVersions.V8,
                    string.Join(",", productIds),
                    fieldsTemplateParam,
                    market,
                    language,
                    actionFilterParam),
                actionName: "GetProducts",
                traceActivityId: traceActivityId,
                payload: null,
                apiVersion: this.defaultApiVersion);

            return catalog;
        }

        public async Task<List<string>> GetSingleMarkets(EventTraceActivity traceActivityId)
        {
            // 1. If the local copy of EU SMD markets is not null and not expired, return the local copy.
            // 2. If the local copy of EU SMD markets is null or expired, get a fresh copy from Catalog Service, keep a local copy and return it.
            // 3. If fetching the EU SMD markets from Catalog Service runs into an exception, then it is fine to return the local copy (does not matter expired or not).
            // 4. If fetching the EU SMD markets from Catalog Service runs into an exception, and if the local copy is null, then return null, so that countries from MarketsEUSMD Domain Dictionary is used.
            // Going with this logic, since Single Market Directive in EU is a compliance requirement and we can avoid taking a hard dependency on Catalog Service.
            if (this.euDirective == null || this.euDirective.IsExpired())
            {
                try
                {
                    this.euDirective = await this.SendRequest<DomainData>(
                        method: HttpMethod.Get,
                        actionPath: string.Format("{0}/domaindata/eudirective?market=neutral&languages=neutral", GlobalConstants.CatalogApiVersions.V8),
                        actionName: "GetSingleMarkets",
                        traceActivityId: traceActivityId,
                        payload: null,
                        apiVersion: this.defaultApiVersion);
                }
                catch (Exception ex)
                {
                    // We want getting the EU Directive to be a best-effor operation
                    SllWebLogger.TracePXServiceException(ex.ToString(), traceActivityId);
                }
            }

            return this.euDirective == null ? null : this.euDirective.GetMarkets();
        }

        private async Task<T> SendRequest<T>(
                        HttpMethod method,
                        string actionPath,
                        string actionName,
                        EventTraceActivity traceActivityId,
                        object payload = null,
                        IList<KeyValuePair<string, string>> additionalHeaders = null,
                        string apiVersion = null)
        {
            string fullRequestUrl = string.Format("{0}/{1}", this.BaseUrl, actionPath);
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

                using (HttpResponseMessage response = await this.catalogServiceHttpClient.SendAsync(requestMessage))
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
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException(string.Format("Failed to deserialize success response from Catalog, Response http status code {0}", response.StatusCode)));
                        }
                    }
                    else
                    {
                        ServiceErrorResponse error = null;
                        try
                        {
                            ServiceErrorResponse innerError = JsonConvert.DeserializeObject<ServiceErrorResponse>(responseMessage);
                            innerError.Source = string.IsNullOrWhiteSpace(innerError.Source) ? Constants.ServiceNames.CatalogService : innerError.Source;
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