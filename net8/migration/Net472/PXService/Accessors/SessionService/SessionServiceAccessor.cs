// <copyright file="SessionServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService
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
    using Microsoft.Commerce.Payments.PXService.Model.SessionService;
    using Microsoft.Commerce.Tracing;
    using Newtonsoft.Json;

    public class SessionServiceAccessor : ISessionServiceAccessor
    {
        private readonly List<string> passThroughHeaders = new List<string> { PaymentConstants.PaymentExtendedHttpHeaders.TestHeader };
        private HttpClient sessionServiceHttpClient;
        private string apiVersion;
        private string baseUrl;

        public SessionServiceAccessor(
            string baseUrl,
            string apiVersion,
            HttpMessageHandler requestHandler)
        {
            this.apiVersion = apiVersion;
            this.baseUrl = baseUrl;

            this.sessionServiceHttpClient = new PXTracingHttpClient(SessionService.V7.Constants.ServiceNames.SessionService, requestHandler, ApplicationInsightsProvider.LogOutgoingOperation);
            this.sessionServiceHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType));
            this.sessionServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.Connection, PaymentConstants.HttpHeaders.KeepAlive);
            this.sessionServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.KeepAlive, string.Format(PaymentConstants.HttpHeaders.KeepAliveParameter, 60));
        }

        public async Task<string> GenerateId(string sessionType, EventTraceActivity traceActivityId)
        {
            string requestUrl = sessionType == null
                    ? SessionService.V7.Constants.UriTemplate.GenerateSessionId
                    : string.Format("{0}/{1}", SessionService.V7.Constants.UriTemplate.GenerateSessionId, sessionType);

            string sessionId = await this.SendRequest<string>(
                   "GenerateSessionId",
                    requestUrl,
                    HttpMethod.Get,
                    null,
                    traceActivityId);

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new IntegrationException(Constants.ServiceNames.SessionService, "Id is empty", "EmptyResponse");
            }

            return sessionId;
        }

        public async Task<T> GetSessionResourceData<T>(string sessionId, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(SessionService.V7.Constants.UriTemplate.GetSessionWithType, sessionId, SessionType.Any.ToString());
            SessionResource resource = await this.SendRequest<SessionResource>(
                "GetSessionResourceData",
                requestUrl,
                HttpMethod.Get,
                null,
                traceActivityId); 

            if (resource == null || resource.Data == null)
            {
                throw new FailedOperationException("Session.Data is missing");
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(resource.Data);
            }
            catch (Exception ex)
            {
                // Task 20216839: [PXService] Reword the error messages 
                throw new FailedOperationException(ex.Message, ex);
            }
        }

        public async Task CreateSessionFromData<T>(string sessionId, T data, EventTraceActivity traceActivityId)
        {
            SessionResource sessionObject = BuildSessionResource(sessionId, data);
            await this.SendRequest<HttpResponseMessage>(
                "CreateSession",
                string.Format(SessionService.V7.Constants.UriTemplate.Session, sessionId),
                HttpMethod.Post,
                sessionObject,
                traceActivityId);
        }

        public async Task UpdateSessionResourceData<T>(string sessionId, T data, EventTraceActivity traceActivityId)
        {
            SessionResource resouce = new SessionResource();
            resouce.Data = JsonConvert.SerializeObject(data);
            string requestUrl = string.Format(SessionService.V7.Constants.UriTemplate.Session, sessionId);
            await this.SendRequest<HttpResponseMessage>(
                "UpdateSession",
                requestUrl,
                HttpMethod.Put,
                resouce,
                traceActivityId);
        }

        private static SessionResource BuildSessionResource<T>(string sessionId, T data)
        {
            return new SessionResource
            {
                Id = sessionId,
                Data = JsonConvert.SerializeObject(data),
                EncryptData = true,
                SessionType = SessionType.Any
            };
        }
        
        private async Task<T> SendRequest<T>(string actionName, string requestUrl, HttpMethod httpMethod, object requestContent, EventTraceActivity traceActivityId)
        {
            string fullRequestUrl = string.Format("{0}/{1}", this.baseUrl, requestUrl);
            using (HttpRequestMessage request = new HttpRequestMessage(httpMethod, fullRequestUrl))
            {
                request.IncrementCorrelationVector(traceActivityId);
                request.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString());
                request.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, Guid.NewGuid().ToString());
                request.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.ApiVersion, this.apiVersion);
                HttpRequestHelper.TransferTargetHeadersFromIncomingRequestToOutgoingRequest(this.passThroughHeaders, request);

                request.AddOrReplaceActionName(actionName);
                
                if (requestContent != null)
                {
                    request.Content = new StringContent(JsonConvert.SerializeObject(requestContent), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType);
                }

                PaymentsEventSource.Log.PXServiceRequestToSessionService(fullRequestUrl, traceActivityId);
                using (HttpResponseMessage response = await this.sessionServiceHttpClient.SendAsync(request))
                {
                    string responseMessage = await response.Content.ReadAsStringAsync();
                    PaymentsEventSource.Log.PXServiceTraceResponseFromSessionService(response.StatusCode.ToString(), responseMessage, traceActivityId);

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
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw TraceCore.TraceException(traceActivityId, new KeyNotFoundException(string.Format("Receive not found request response from Session service: {0}.", responseMessage ?? string.Empty)));
                    }
                    else
                    {
                        ServiceErrorResponse error = null;
                        try
                        {
                            Model.PayerAuthService.ErrorResponse errorResponse = JsonConvert.DeserializeObject<Model.PayerAuthService.ErrorResponse>(responseMessage);
                            ServiceErrorResponse innerError = new ServiceErrorResponse(errorResponse.ErrorCode, errorResponse.Message);
                            innerError.Source = string.IsNullOrWhiteSpace(innerError.Source) ? "SessionService" : innerError.Source;
                            error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException("Failed to deserialize error response from PayerAuth"));
                        }
                        
                        throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response, HandlingType = ExceptionHandlingPolicy.ByPass });
                    }
                }
            }
        }
    }
}