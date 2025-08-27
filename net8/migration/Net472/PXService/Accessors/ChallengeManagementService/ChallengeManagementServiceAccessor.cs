// <copyright file="ChallengeManagementServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.ChallengeManagementService
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
    using Microsoft.Commerce.Payments.PXService.Model.ChallengeManagementService;
    using Microsoft.Commerce.Payments.PXService.Model.ChallengeManagementService.ChallengeManagementErrorResponse;
    using Microsoft.Commerce.Tracing;
    using Newtonsoft.Json;
    using static Microsoft.Commerce.Payments.PXService.Model.ChallengeManagementService.SessionEnumDefinition;

    public class ChallengeManagementServiceAccessor : IChallengeManagementServiceAccessor
    {
        private HttpClient challengeManagementServiceHttpClient;
        private string serviceBaseUrl;
        private string emulatorBaseUrl;

        public ChallengeManagementServiceAccessor(
            string serviceBaseUrl,
            string emulatorBaseUrl,
            HttpMessageHandler messageHandler)
        {
            this.serviceBaseUrl = serviceBaseUrl;
            this.emulatorBaseUrl = emulatorBaseUrl;
            this.challengeManagementServiceHttpClient = new PXTracingHttpClient(Constants.ServiceNames.ChallengeManagementService, messageHandler);
            this.challengeManagementServiceHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType));
            this.challengeManagementServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.Connection, PaymentConstants.HttpHeaders.KeepAlive);
            this.challengeManagementServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.KeepAlive, string.Format(PaymentConstants.HttpHeaders.KeepAliveParameter, 60));
        }

        private string BaseUrl
        {
            get
            {
                if (HttpRequestHelper.IsPXTestRequest("px.") && !string.IsNullOrWhiteSpace(this.emulatorBaseUrl))
                {
                    return this.emulatorBaseUrl;
                }
                else
                {
                    return this.serviceBaseUrl;
                }
            }
        }

        public async Task<string> CreateChallenge(string sessionId, EventTraceActivity traceActivityId, string language, int riskScore, string challengeProvider)
        {
            string requestUrl = "api/v1/challenge/create";
            ChallengeCreationModel challengeCreationModel = new ChallengeCreationModel()
            {
                SessionId = sessionId,
                ChallengeRequestorName = ChallengeEnumDefinition.ChallengeRequestor.PXService.ToString(),
                RiskScore = riskScore,
                ChallengeProviderName = challengeProvider
            };
            var additionalHeaders = new List<KeyValuePair<string, string>>();
            additionalHeaders.Add(new KeyValuePair<string, string>("Accept-Language", language));
            string response = await this.SendRequestWithRetries(
                HttpMethod.Post,
                requestUrl,
                challengeCreationModel,
                "CreateChallenge",
                traceActivityId,
                null,
                additionalHeaders);
            return response;
        }

        public async Task<SessionBusinessModel> CreateChallengeSession(string sessionData, EventTraceActivity traceActivityId)
        {
            string requestUrl = "api/v1/challengesession/create";
            SessionBusinessModel sessionRequest = new SessionBusinessModel()
            {
                SessionType = SessionEnumDefinition.SessionType.PXAddPISession.ToString(),
                SessionData = sessionData,
                SessionLength = 20,
                SessionSlidingExpiration = true
            };
            string response = await this.SendRequestWithRetries(
                HttpMethod.Post,
                requestUrl,
                sessionRequest,
                "CreateSessionData",
                traceActivityId,
                null);
            return JsonConvert.DeserializeObject<SessionBusinessModel>(response);
        }

        public async Task<SessionBusinessModel> GetChallengeSession(string sessionId, EventTraceActivity traceActivityId)
        {
            string requestUrl = $"api/v1/challengesession/get/{sessionId}";

            string response = await this.SendRequestWithRetries(
                HttpMethod.Get,
                requestUrl,
                null,
                "GetChallengeSessionData",
                traceActivityId,
                null);
            return JsonConvert.DeserializeObject<SessionBusinessModel>(response);
        }

        public async Task<ChallengeStatusResult> GetChallengeStatus(string sessionId, EventTraceActivity traceActivityId)
        {
            string requestUrl = $"api/v1/challenge/status?sessionId={sessionId}";

            string response = await this.SendRequestWithRetries(
                HttpMethod.Get,
                requestUrl,
                null,
                "GetChallengeStatus",
                traceActivityId,
                null);
            return JsonConvert.DeserializeObject<ChallengeStatusResult>(response);
        }

        public Task<object> SubmitChallenge()
        {
            throw new NotImplementedException();
        }

        public async Task<SessionBusinessModel> UpdateChallengeSession(SessionBusinessModel sessionRequest, EventTraceActivity traceActivityId)
        {
            string requestUrl = "api/v1/challengesession/update";
            string response = await this.SendRequestWithRetries(
                HttpMethod.Put,
                requestUrl,
                sessionRequest,
                "UpdateChallengeSession",
                traceActivityId,
                null);
            return JsonConvert.DeserializeObject<SessionBusinessModel>(response);
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

        private async Task<string> SendRequestWithRetries(HttpMethod method, string url, object request, string actionName, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, IList<KeyValuePair<string, string>> additionalHeaders = null)
        {
            int numAttemptsRemaining = 3;
            int delayInSec = 1;

            while (numAttemptsRemaining-- > 0)
            {
                try
                {
                    return await this.SendRequest(method, url, request, actionName, traceActivityId, exposedFlightFeatures, additionalHeaders);
                }
                catch (ServiceErrorResponseException serviceErrorResponseException)
                {
                    if (numAttemptsRemaining <= 0
                        || serviceErrorResponseException.Response == null
                        || serviceErrorResponseException.Response.StatusCode < HttpStatusCode.InternalServerError)
                    {
                        throw serviceErrorResponseException;
                    }

                    await Task.Delay(delayInSec * 500);
                }
            }

            return await this.SendRequest(method, url, request, actionName, traceActivityId, exposedFlightFeatures, additionalHeaders);
        }

        private async Task<string> SendRequest(HttpMethod method, string url, object request, string actionName, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, IList<KeyValuePair<string, string>> additionalHeaders = null)
        {
            string fullRequestUrl = string.Format("{0}/{1}", this.BaseUrl, url);

            PaymentsEventSource.Log.PXServiceTraceResponseToChallengeManagementService(fullRequestUrl, traceActivityId);

            using (HttpRequestMessage requestMessage = new HttpRequestMessage(method, fullRequestUrl))
            {
                requestMessage.IncrementCorrelationVector(traceActivityId);
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString());
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, Guid.NewGuid().ToString());
                requestMessage.AddOrReplaceActionName(actionName);
                AddHeaders(requestMessage, additionalHeaders);

                if (request != null)
                {
                    requestMessage.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType);
                }

                using (HttpResponseMessage response = await this.challengeManagementServiceHttpClient.SendAsync(requestMessage))
                {
                    string responseMessage = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            PaymentsEventSource.Log.PXServiceTraceResponseFromChallengeManagementService(response.StatusCode.ToString(), responseMessage, traceActivityId);
                            return responseMessage;
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException($"Failed to deserialize success response from {Constants.ServiceNames.ChallengeManagementService}. Response Message: {responseMessage}"));
                        }
                    }
                    else
                    {
                        ServiceErrorResponse error = null;
                        try
                        {
                            if (response.StatusCode < HttpStatusCode.InternalServerError)
                            {
                                ChallengeManagementServiceErrorResponse innerError = JsonConvert.DeserializeObject<ChallengeManagementServiceErrorResponse>(responseMessage);
                                error = new ServiceErrorResponse(innerError?.Error?.Code, innerError?.Error?.Message, Constants.ServiceNames.ChallengeManagementService);
                            }
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException($"Failed to deserialize error response from {Constants.ServiceNames.ChallengeManagementService}. Response Message: {responseMessage}"));
                        }

                        throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response, HandlingType = ExceptionHandlingPolicy.ByPass });
                    }
                }
            }
        }
    }
}
