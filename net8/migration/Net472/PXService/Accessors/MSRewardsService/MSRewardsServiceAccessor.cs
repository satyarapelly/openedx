// <copyright file="MSRewardsServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.MSRewardsService
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Accessors.MSRewardsService.DataModel;
    using Microsoft.Commerce.Tracing;
    using Newtonsoft.Json;

    public class MSRewardsServiceAccessor : IMSRewardsServiceAccessor
    {
        private const string RewardsChannel = "PaymentsPayWithPointsStore";
        private const string RewardsHeaderCountry = "X-Rewards-Country";
        private const string RewardsHeaderPartnerId = "X-Rewards-PartnerId";
        private const string RewardsHeaderHasPI = "X-Rewards-HasPI";
        private readonly List<string> passThroughHeaders = new List<string> { PaymentConstants.PaymentExtendedHttpHeaders.TestHeader };

        private HttpClient msRewardsServiceHttpClient;

        private string serviceBaseUrl;
        private string emulatorBaseUrl;

        public MSRewardsServiceAccessor(
            string serviceBaseUrl,
            string emulatorBaseUrl,
            HttpMessageHandler messageHandler)
        {
            this.serviceBaseUrl = serviceBaseUrl;
            this.emulatorBaseUrl = emulatorBaseUrl;

            this.msRewardsServiceHttpClient = new PXTracingHttpClient(Constants.ServiceNames.MSRewardsService, messageHandler);
            this.msRewardsServiceHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType));
            this.msRewardsServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.Connection, PaymentConstants.HttpHeaders.KeepAlive);
            this.msRewardsServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.KeepAlive, string.Format(PaymentConstants.HttpHeaders.KeepAliveParameter, 60));
        }

        private string BaseUrl
        {
            get
            {
                if (HttpRequestHelper.IsPXTestRequest("px.msrewards") && !string.IsNullOrWhiteSpace(this.emulatorBaseUrl))
                {
                    return this.emulatorBaseUrl;
                }
                else
                {
                    return this.serviceBaseUrl;
                }
            }
        }

        public async Task<GetUserInfoResult> GetUserInfo(string userId, string country, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.GetMSRewardsUserInfo, userId, RewardsChannel);

            MSRewardsResponse<GetUserInfoResult> userInfoResponse = await this.SendRequest<MSRewardsResponse<GetUserInfoResult>>(HttpMethod.Get, requestUrl, country, null, "GetUserInfo", traceActivityId);

            return userInfoResponse?.Response;
        }

        public async Task<RedemptionResult> RedeemRewards(string userId, string country, string partnerName, bool hasAnyPI, RedemptionRequest redemptionRequest, EventTraceActivity traceActivityId)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.RedeemMSRewards, userId);

            if (redemptionRequest != null)
            {
                redemptionRequest.Channel = RewardsChannel;
                redemptionRequest.PhoneNumberOnChallengeFirst = true;

                if (redemptionRequest.RiskContext != null)
                {
                    if (redemptionRequest.RiskContext.ChallengePreference == RiskVerificationType.Unknown)
                    {
                        // MSRewards service is no longer accepting Unknown as a value for ChallengePreference. We need to default to SMS in those cases.
                        redemptionRequest.RiskContext.ChallengePreference = RiskVerificationType.SMS;
                    }
                    else
                    {
                        redemptionRequest.IsPhoneNumberOnVerificationCodeRequest = true;
                    }
                }
            }

            var additionalHeaders = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(RewardsHeaderPartnerId, partnerName)
            };

            if (hasAnyPI)
            {
                additionalHeaders.Add(new KeyValuePair<string, string>(RewardsHeaderHasPI, hasAnyPI.ToString()));
            }

            MSRewardsResponse<RedemptionResult> redemptionResult = await this.SendRequest<MSRewardsResponse<RedemptionResult>>(HttpMethod.Post, requestUrl, country, redemptionRequest, "RedeemRewards", traceActivityId, additionalHeaders);

            if (redemptionResult?.Response != null)
            {
                redemptionResult.Response.Code = redemptionResult.Code;
            }

            return redemptionResult?.Response;
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

        private async Task<T> SendRequest<T>(HttpMethod method, string url, string country, object request, string actionName, EventTraceActivity traceActivityId, IList<KeyValuePair<string, string>> additionalHeaders = null)
        {
            string fullRequestUrl = string.Format("{0}/{1}", this.BaseUrl, url);

            PaymentsEventSource.Log.PXServiceRequestToMSRewardsService(fullRequestUrl, traceActivityId);

            using (HttpRequestMessage requestMessage = new HttpRequestMessage(method, fullRequestUrl))
            {
                requestMessage.IncrementCorrelationVector(traceActivityId);
                requestMessage.Headers.Add(RewardsHeaderCountry, country);

                requestMessage.AddOrReplaceActionName(actionName);

                AddHeaders(requestMessage, additionalHeaders);
                if (HttpRequestHelper.IsPXTestRequest("px.msrewards") && !string.IsNullOrWhiteSpace(this.emulatorBaseUrl))
                {
                    HttpRequestHelper.TransferTargetHeadersFromIncomingRequestToOutgoingRequest(this.passThroughHeaders, requestMessage);
                }

                if (request != null)
                {
                    requestMessage.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType);
                }

                using (HttpResponseMessage response = await this.msRewardsServiceHttpClient.SendAsync(requestMessage))
                {
                    string responseMessage = await response.Content.ReadAsStringAsync();
                    PaymentsEventSource.Log.PXServiceTraceResponseFromMSRewardsService(response.StatusCode.ToString(), responseMessage, traceActivityId);

                    if (response.IsSuccessStatusCode)
                    {
                        try
                        {
                            return JsonConvert.DeserializeObject<T>(responseMessage);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException($"Failed to deserialize success response from {Constants.ServiceNames.MSRewardsService}. Response Message: {responseMessage}"));
                        }
                    }
                    else
                    {
                        ServiceErrorResponse error = null;
                        try
                        {
                            ServiceErrorResponse innerError = JsonConvert.DeserializeObject<ServiceErrorResponse>(responseMessage);
                            innerError.Source = string.IsNullOrWhiteSpace(innerError.Source) ? Constants.ServiceNames.MSRewardsService : innerError.Source;
                            error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError.ToString());
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException($"Failed to deserialize error response from {Constants.ServiceNames.MSRewardsService}. Response Message: {responseMessage}"));
                        }

                        throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response, HandlingType = ExceptionHandlingPolicy.ByPass });
                    }
                }
            }
        }
    }
}