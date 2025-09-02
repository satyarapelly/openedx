// <copyright file="StoredValueAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXCommon;
    using Model.StoredValueService;
    using Newtonsoft.Json;

    public class StoredValueAccessor : IStoredValueAccessor
    {
        private readonly List<string> passThroughHeaders = new List<string> { PaymentConstants.PaymentExtendedHttpHeaders.TestHeader };
        private HttpClient storedValueServiceHttpClient;
        private string serviceBaseUrl;
        private string emulatorBaseUrl;
        private string apiVersion;

        public StoredValueAccessor(
            string apiVersion,
            string serviceBaseUrl,
            string emulatorBaseUrl,
            HttpMessageHandler messageHandler)
        {
            this.apiVersion = apiVersion;
            this.serviceBaseUrl = serviceBaseUrl;
            this.emulatorBaseUrl = emulatorBaseUrl;
            this.storedValueServiceHttpClient = new PXTracingHttpClient(Constants.ServiceNames.StoredValueService, messageHandler);
            this.storedValueServiceHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType));
            this.storedValueServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.Connection, PaymentConstants.HttpHeaders.KeepAlive);
            this.storedValueServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.KeepAlive, string.Format(PaymentConstants.HttpHeaders.KeepAliveParameter, 60));
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

        public async Task<IList<StoredValueFundingCatalog>> GetStoredValueFundingCatalog(string currency, EventTraceActivity traceActivityId)
        {
            string urlPath = string.Format(V7.Constants.UriTemplate.GetStoredValueFundingCatalog, currency);
            return await this.SendRequest<IList<StoredValueFundingCatalog>>(HttpMethod.Get, urlPath, "GetStoredValueFundingCatalog", traceActivityId);
        }

        public async Task<FundStoredValueTransaction> FundStoredValue(
            string amount,
            string country,
            string currency,
            string piid,
            string puid,
            string legacyAccountId,
            string greenId,
            string ipAddress,
            string userAgent,
            EventTraceActivity traceActivityId,
            string description = "")
        {
            string urlPath = string.Format(V7.Constants.UriTemplate.FundStoredValue, legacyAccountId);
            FundStoredValuePayload payload = new FundStoredValuePayload()
            {
                Amount = decimal.Parse(amount),
                Country = country,
                Currency = currency,
                PaymentInstrumentId = piid,
                IdentityValue = puid,
                Description = description,
                RiskProperties = GetRiskProperties(greenId, ipAddress, userAgent)
            };

            return await this.SendRequest<FundStoredValueTransaction>(HttpMethod.Post, urlPath, "FundStoredValue", traceActivityId, payload);
        }

        public async Task<FundStoredValueTransaction> CheckFundStoredValue(string legacyAccountId, string referenceId, EventTraceActivity traceActivityId)
        {
            string urlPath = string.Format(V7.Constants.UriTemplate.CheckFundStoredValue, legacyAccountId, referenceId);
            return await this.SendRequest<FundStoredValueTransaction>(HttpMethod.Get, urlPath, "CheckFundStoredValue", traceActivityId);
        }

        private static IList<RESTProperty> GetRiskProperties(string greenId, string ipAddress, string userAgent)
        {
            var properties = new List<RESTProperty>();

            // Property names to match with PCS, so that the existing RISK rules can work
            properties.Add(new RESTProperty() { Namespace = "RISK", Name = "THM_SESSION_ID", Value = greenId });
            properties.Add(new RESTProperty() { Namespace = "RISK", Name = "OnBehalfOfPartnerGuid", Value = "cc730318-4eb3-4ed0-ac35-18fec25df70d" });
            properties.Add(new RESTProperty() { Namespace = "RISK", Name = "PCSPartnerName", Value = "bam-msa" });
            properties.Add(new RESTProperty() { Namespace = "RISK", Name = "PCSActionParam", Value = "FUNDSTOREDVALUE" });
            properties.Add(new RESTProperty() { Namespace = "RISK", Name = "IPAddress", Value = ipAddress });
            properties.Add(new RESTProperty() { Namespace = "RISK", Name = "UserAgent", Value = userAgent });

            return properties;
        }

        private async Task<T> SendRequest<T>(
            HttpMethod method,
            string urlPath,
            string actionName,
            EventTraceActivity traceActivityId,
            object payload = null,
            IList<KeyValuePair<string, string>> additionalHeaders = null)
        {
            string fullRequestUrl = string.Format("{0}{1}", this.BaseUrl, urlPath);
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(method, fullRequestUrl))
            {
                requestMessage.IncrementCorrelationVector(traceActivityId);
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString());
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, Guid.NewGuid().ToString());
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.ApiVersion, this.apiVersion);
                HttpRequestHelper.TransferTargetHeadersFromIncomingRequestToOutgoingRequest(this.passThroughHeaders, requestMessage);

                // Add action name to the request properties so that this request's OperationName is logged properly
                requestMessage.AddOrReplaceActionName(actionName);

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

                using (HttpResponseMessage response = await this.storedValueServiceHttpClient.SendAsync(requestMessage))
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
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException(string.Format("Failed to deserialize success response from StoredValue service, Response http status code {0}", response.StatusCode)));
                        }
                    }
                    else
                    {
                        ServiceErrorResponse error = null;
                        try
                        {
                            ServiceErrorResponse innerError = JsonConvert.DeserializeObject<ServiceErrorResponse>(responseMessage);
                            innerError.Source = string.IsNullOrWhiteSpace(innerError.Source) ? Constants.ServiceNames.StoredValueService : innerError.Source;
                            error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Failed to deserialize error response from StoredValue service"));
                        }

                        throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response });
                    }
                }
            }
        }
    }
}