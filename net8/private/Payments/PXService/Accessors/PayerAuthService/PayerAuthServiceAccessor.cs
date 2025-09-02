// <copyright file="PayerAuthServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

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
    using Microsoft.Commerce.Payments.PXService.Model.PayerAuthService;
    using Newtonsoft.Json;
    using ThreeDSServer = Model.ThreeDSExternalService;

    public class PayerAuthServiceAccessor : IPayerAuthServiceAccessor
    {
        private const string PayerAuthServiceName = "PayerAuthService";
        private readonly List<string> passThroughHeaders = new List<string> { PaymentConstants.PaymentExtendedHttpHeaders.TestHeader };
        private HttpClient payerAuthServiceHttpClient;
        private string serviceBaseUrl;
        private string emulatorBaseUrl;
        private string defaultApiVersion;

        public PayerAuthServiceAccessor(
            string serviceBaseUrl,
            string emulatorBaseUrl,
            string apiVersion,
            HttpMessageHandler messageHandler)
        {
            this.serviceBaseUrl = serviceBaseUrl;
            this.emulatorBaseUrl = emulatorBaseUrl;
            this.defaultApiVersion = apiVersion;

            this.payerAuthServiceHttpClient = new PXTracingHttpClient(
                Constants.ServiceNames.PayerAuthService,
                messageHandler,
                logOutgoingRequestToApplicationInsight: ApplicationInsightsProvider.LogOutgoingOperation);
            this.payerAuthServiceHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType));
            this.payerAuthServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.Connection, PaymentConstants.HttpHeaders.KeepAlive);
            this.payerAuthServiceHttpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.KeepAlive, string.Format(PaymentConstants.HttpHeaders.KeepAliveParameter, 60));
        }

        private string BaseUrl
        {
            get
            {
                if ((HttpRequestHelper.IsPXTestRequest() || HttpRequestHelper.IsPXTestRequest("pxpayerauthemulator.")) && !string.IsNullOrWhiteSpace(this.emulatorBaseUrl))
                {
                    return this.emulatorBaseUrl;
                }
                else
                {
                    return this.serviceBaseUrl;
                }
            }
        }

        public async Task<PaymentSessionResponse> CreatePaymentSessionId(PaymentSessionData paymentSessionData, EventTraceActivity traceActivityId)
        {
            return await this.SendRequest<PaymentSessionResponse>(
                method: HttpMethod.Post,
                actionName: "CreatePaymentSessionId",
                traceActivityId: traceActivityId,
                payload: new PaymentSessionRequest(paymentSessionData),
                apiVersion: GlobalConstants.PayerAuthApiVersions.V3);
        }

        public async Task<ThreeDSMethodData> Get3DSMethodURL(
            PaymentSession paymentSession, 
            EventTraceActivity traceActivityId)
        {
            EventTraceActivity traceActivityID = new EventTraceActivity();
            ThreeDSMethodData methodData = await this.SendRequest<ThreeDSMethodData>(
                method: HttpMethod.Post,
                actionName: "GetThreeDSMethodURL",
                traceActivityId: traceActivityId,
                payload: new ThreeDSMethodRequest(paymentSession),
                apiVersion: GlobalConstants.PayerAuthApiVersions.V3);

            if (methodData == null || methodData.ThreeDSServerTransID == null)
            {
                throw new IntegrationException(Constants.ServiceNames.PayerAuthService, "Missing ThreeDSServerTransID from payerAuthResponse", "MissingParameter");
            }

            return methodData;
        }

        public async Task<AuthenticationResponse> Authenticate(
            AuthenticationRequest authRequest, 
            EventTraceActivity traceActivityId)
        {
            var ares = await this.SendRequest<AuthenticationResponse>(
                method: HttpMethod.Post,
                actionName: "Authenticate",
                traceActivityId: traceActivityId,
                payload: authRequest,
                apiVersion: GlobalConstants.PayerAuthApiVersions.V3);

            if (ares == null 
                || (ares.EnrollmentStatus != PaymentInstrumentEnrollmentStatus.Bypassed && ares.AcsTransactionId == null))
            {
                throw new IntegrationException(Constants.ServiceNames.PayerAuthService, "Missing AcsTransactionId from payerAuthResponse", "MissingParameter");
            }

            if (ares.EnrollmentStatus == PaymentInstrumentEnrollmentStatus.Enrolled 
                && authRequest.PaymentSession.DeviceChannel == ThreeDSServer.DeviceChannel.Browser 
                && ares.AcsUrl == null)
            {
                throw new IntegrationException(Constants.ServiceNames.PayerAuthService, "Missing AcsUrl in Browser flow from payerAuthResponse", "MissingParameter");
            }

            if (ares.EnrollmentStatus == PaymentInstrumentEnrollmentStatus.Enrolled 
                && authRequest.PaymentSession.DeviceChannel == ThreeDSServer.DeviceChannel.AppBased 
                && (ares.AcsSignedContent == null || ares.ThreeDSServerTransactionId == null))
            {
                throw new IntegrationException(Constants.ServiceNames.PayerAuthService, "Missing AcsSignedContent in App flow from payerAuthResponse", "MissingParameter");
            }

            return ares;
        }

        public async Task<AuthenticationResponse> AuthenticateThreeDSOne(
            AuthenticationRequest authRequest,
            EventTraceActivity traceActivityId)
        {
            var ares = await this.SendRequest<AuthenticationResponse>(
                method: HttpMethod.Post,
                actionName: "Authenticate",
                traceActivityId: traceActivityId,
                payload: authRequest,
                apiVersion: GlobalConstants.PayerAuthApiVersions.V3);

            if (ares.EnrollmentStatus == PaymentInstrumentEnrollmentStatus.Enrolled
                && authRequest.PaymentSession.DeviceChannel == ThreeDSServer.DeviceChannel.Browser
                && ares.AcsUrl == null)
            {
                throw new IntegrationException(Constants.ServiceNames.PayerAuthService, "Missing AcsUrl in Browser flow from payerAuthResponse", "MissingParameter");
            }

            if (ares.EnrollmentStatus == PaymentInstrumentEnrollmentStatus.Enrolled
                && authRequest.PaymentSession.DeviceChannel == ThreeDSServer.DeviceChannel.AppBased
                && (ares.AcsSignedContent == null || ares.ThreeDSServerTransactionId == null))
            {
                throw new IntegrationException(Constants.ServiceNames.PayerAuthService, "Missing AcsSignedContent in App flow from payerAuthResponse", "MissingParameter");
            }

            return ares;
        }

        public async Task<CompletionResponse> CompleteChallenge(
            CompletionRequest completionRequest, 
            EventTraceActivity traceActivityId)
        {
            var completionRes = await this.SendRequest<CompletionResponse>(
                method: HttpMethod.Post,
                actionName: "CompleteChallenge",
                traceActivityId: traceActivityId,
                payload: completionRequest,
                apiVersion: GlobalConstants.PayerAuthApiVersions.V3);

            if (completionRes == null)
            {
                throw new IntegrationException(Constants.ServiceNames.PayerAuthService, "CompletionResponse for PayerAuth is null", "MissingParameter");
            }

            return completionRes;
        }

        private async Task<T> SendRequest<T>(
            HttpMethod method, 
            string actionName, 
            EventTraceActivity traceActivityId, 
            object payload = null, 
            IList<KeyValuePair<string, string>> additionalHeaders = null, 
            string apiVersion = null)
        {
            string fullRequestUrl = string.Format("{0}/{1}", this.BaseUrl, actionName);
            using (HttpRequestMessage requestMessage = new HttpRequestMessage(method, fullRequestUrl))
            {
                requestMessage.IncrementCorrelationVector(traceActivityId);
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString());
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, Guid.NewGuid().ToString());
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.ApiVersion, apiVersion ?? this.defaultApiVersion);
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

                using (HttpResponseMessage response = await this.payerAuthServiceHttpClient.SendAsync(requestMessage))
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
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException(string.Format("Failed to deserialize success response from PayerAuth, Response http status code {0}", response.StatusCode)));
                        }
                    }
                    else
                    {
                        ServiceErrorResponse error = null;
                        try
                        {
                            Model.PayerAuthService.ErrorResponse payAuthError = JsonConvert.DeserializeObject<Model.PayerAuthService.ErrorResponse>(responseMessage);
                            ServiceErrorResponse innerError = new ServiceErrorResponse(payAuthError.ErrorCode, payAuthError.Message);
                            innerError.Source = string.IsNullOrWhiteSpace(innerError.Source) ? PayerAuthServiceName : innerError.Source;
                            error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError);
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException(string.Format("Failed to deserialize error response from PayerAuth {0}", responseMessage)));
                        }

                        throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response, HandlingType = ExceptionHandlingPolicy.ByPass });
                    }
                }
            }
        }
    }
}