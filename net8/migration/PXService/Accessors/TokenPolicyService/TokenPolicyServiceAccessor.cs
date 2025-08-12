// <copyright file="TokenPolicyServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>
// Documentation for this serivce can be found at https://microsoft.sharepoint.com/:w:/r/teams/CatalogPurchaseUseTeam/Shared%20Documents/_archive/M$%20Feature%20Crews/M$%20Purchase/API%20Documentation/ToPS/ToPS%20TokenDescription%20v1.0%20API%20Guide%20-%2020201016.docx?d=wb484ef351f464592aea64197616df7b2&csf=1&web=1&e=hsj6Z8
// It is the same team that owns the M$ Purchase service.

namespace Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Accessors.TokenPolicyService.DataModel;
    using Newtonsoft.Json;

    public class TokenPolicyServiceAccessor : ITokenPolicyServiceAccessor
    {
        private const string MSTrustedClientIpAddress = "MS-TrustedClientIpAddress";
        
        private const string UserIdFormat = "msa:{0}";

        private readonly List<string> passThroughHeaders = new List<string> { PaymentConstants.PaymentExtendedHttpHeaders.TestHeader };

        private HttpClient tokenPolicyServiceHttpClient;

        private string serviceBaseUrl;
        private string emulatorBaseUrl;

        public TokenPolicyServiceAccessor(
            string serviceBaseUrl,
            string emulatorBaseUrl,
            HttpMessageHandler messageHandler)
        {
            this.serviceBaseUrl = serviceBaseUrl;
            this.emulatorBaseUrl = emulatorBaseUrl;

            this.tokenPolicyServiceHttpClient = new PXTracingHttpClient(Constants.ServiceNames.TokenPolicyService, messageHandler);
            this.tokenPolicyServiceHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType));
            this.tokenPolicyServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.Connection, PaymentConstants.HttpHeaders.KeepAlive);
            this.tokenPolicyServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.KeepAlive, string.Format(PaymentConstants.HttpHeaders.KeepAliveParameter, 60));
        }

        private string BaseUrl
        {
            get
            {
                if (HttpRequestHelper.IsPXTestRequest("px.tops") && !string.IsNullOrWhiteSpace(this.emulatorBaseUrl))
                {
                    return this.emulatorBaseUrl;
                }
                else
                {
                    return this.serviceBaseUrl;
                }
            }
        }

        public async Task<TokenPolicyDescription> GetTokenDescriptionAsync(string puid, string tokenValue, string market, string language, string clientIP, EventTraceActivity traceActivityId)
        {
            string userId = string.Format(UserIdFormat, puid);
            string requestUrl = string.Format(V7.Constants.UriTemplate.GetTokenDescription, GlobalConstants.TokenPolicyServiceApiVersions.V1, userId);

            TokenPolicyDescriptionRequest tokenPolicyDescriptionRequest = new TokenPolicyDescriptionRequest
            {
                Language = language,
                Market = market,
                TokenIdentifierValue = tokenValue,
                TokenIdentifierType = TokenDescriptionRequestIdentifierType.TokenCode
            };

            var additionalHeaders = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(MSTrustedClientIpAddress, clientIP)
            };

            TokenPolicyDescription tokenPolicyDescription = await this.SendRequest<TokenPolicyDescription>(HttpMethod.Post, requestUrl, tokenPolicyDescriptionRequest, "GetTokenDescription", traceActivityId, additionalHeaders);

            return tokenPolicyDescription;
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

        private async Task<T> SendRequest<T>(HttpMethod method, string url, object request, string actionName, EventTraceActivity traceActivityId, IList<KeyValuePair<string, string>> additionalHeaders = null)
        {
            string baseUrl = this.BaseUrl;
            string fullRequestUrl = string.Format("{0}/{1}", baseUrl, url);

            using (HttpRequestMessage requestMessage = new HttpRequestMessage(method, fullRequestUrl))
            {
                requestMessage.IncrementCorrelationVector(traceActivityId);
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString());
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, Guid.NewGuid().ToString());
                requestMessage.Headers.Add("Request-Id", Guid.NewGuid().ToString());

                AddHeaders(requestMessage, additionalHeaders);

                if (string.Equals(baseUrl, this.emulatorBaseUrl, StringComparison.OrdinalIgnoreCase))
                {
                    HttpRequestHelper.TransferTargetHeadersFromIncomingRequestToOutgoingRequest(this.passThroughHeaders, requestMessage);
                }

                // Add action name to the request properties so that this request's OperationName is logged properly
                requestMessage.AddOrReplaceActionName(actionName);

                if (request != null)
                {
                    requestMessage.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType);
                }

                using (HttpResponseMessage response = await this.tokenPolicyServiceHttpClient.SendAsync(requestMessage))
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
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException($"Failed to deserialize success response from {Constants.ServiceNames.TokenPolicyService}. Response Message: {responseMessage}"));
                        }
                    }
                    else
                    {
                        ServiceErrorResponse error = null;
                        try
                        {
                            TokenPolicyServiceErrorResponse topsErrorResponse = JsonConvert.DeserializeObject<TokenPolicyServiceErrorResponse>(responseMessage);
                            ServiceErrorResponse innerError = new ServiceErrorResponse()
                            {
                                ErrorCode = topsErrorResponse.Code.ToString(),
                                Message = topsErrorResponse.Message,
                                Source = Constants.ServiceNames.TokenPolicyService
                            };
                            error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException($"Failed to deserialize error response from {Constants.ServiceNames.TokenPolicyService}. Response Message: {responseMessage}"));
                        }

                        throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response, HandlingType = ExceptionHandlingPolicy.ByPass });
                    }
                }
            }
        }
    }
}