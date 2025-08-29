// <copyright file="NetworkTokenizationServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.NetworkTokenizationService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PimsModel.V4;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Model.NetworkTokenizationService;
    using Microsoft.Commerce.Tracing;
    using MMicrosoft.Commerce.Payments.PXService.Model.NetworkTokenizationService;
    using Newtonsoft.Json;
    using static Microsoft.Commerce.Payments.PXService.GlobalConstants;

    public class NetworkTokenizationServiceAccessor : INetworkTokenizationServiceAccessor
    {
        private string serviceBaseUrl;
        private string intServiceBaseUrl;
        private string emulatorBaseUrl;
        private string apiVersion;
        private HttpClient httpClient;

        public NetworkTokenizationServiceAccessor(
            string serviceBaseUrl,
            string intServiceBaseUrl,
            string emulatorBaseUrl,
            string apiVersion,
            HttpMessageHandler messageHandler)
        {
            this.serviceBaseUrl = serviceBaseUrl;
            this.intServiceBaseUrl = intServiceBaseUrl;
            this.emulatorBaseUrl = emulatorBaseUrl;
            this.apiVersion = apiVersion;

            this.httpClient = new PXTracingHttpClient(Constants.ServiceNames.NetworkTokenizationService, messageHandler);
            this.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType));
            this.httpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.Connection, PaymentConstants.HttpHeaders.KeepAlive);
            this.httpClient.DefaultRequestHeaders.Add(PaymentConstants.HttpHeaders.KeepAlive, string.Format(PaymentConstants.HttpHeaders.KeepAliveParameter, 60));
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

        public async Task<NetworkTokenizationServiceResponse> GetNetworkTokens(string puid, string deviceId, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures)
        {
            string requestUrl = V7.Constants.UriTemplate.GetNetworkTokens;

            NetworkTokenizationServiceResponse networkTokens = await this.SendGetRequest<NetworkTokenizationServiceResponse>(
                requestUrl,
                deviceId,
                "GetNetworkTokens",
                traceActivityId,
                GenerateCustomerHeader($"{{\"puid\": \"{puid}\"}}"),
                exposedFlightFeatures);

            return networkTokens;
        }

        public async Task<ListTokenMetadataResponse> ListTokensWithExternalCardReference(string puid, string deviceId, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, string externalCardReference, string email)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.ListNetworkTokensWithExternalCardReference, externalCardReference);

            ListTokenMetadataResponse networkTokens = await this.SendGetRequest<ListTokenMetadataResponse>(
                requestUrl,
                deviceId,
                "ListNetWorkTokensWithExternalCardReference",
                traceActivityId,
                GenerateCustomerHeader(
                    $"{{\"customerType\": \"MSA\", \"puid\": \"{puid}\"}}",
                    $"{{\"email\":\"{email}\"}}",
                    "X509",
                    "1.0"),
                exposedFlightFeatures);

            return networkTokens;
        }

        public async Task<GetTokenMetadataResponse> RequestToken(string puid, string deviceId, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, string email, string country, string language, PaymentInstrument paymentInstrument)
        {
            string requestUrl = V7.Constants.UriTemplate.GetNetworkTokens;

            // Extract expiryMonth, and expiryYear from paymentInstrument
            string expiryMonth = paymentInstrument?.PaymentInstrumentDetails?.ExpiryMonth;
            string expiryYear = paymentInstrument?.PaymentInstrumentDetails?.ExpiryYear;

            // Parse expiryMonth and expiryYear to integers
            int parsedExpiryMonth = int.TryParse(expiryMonth, out int month) ? month : 0; // Default to 0 if parsing fails
            int parsedExpiryYear = int.TryParse(expiryYear, out int year) ? year : 0; // Default to 0 if parsing fails

            RequestTokenRequest requestTokenRequest = new RequestTokenRequest
            {
                SecureDataId = paymentInstrument.PaymentInstrumentDetails.SecureDataId,
                ExternalCardReference = paymentInstrument.PaymentInstrumentId,
                ExternalCardReferenceType = ExternalCardReferenceType.PaymentInstrumentId,
                CardProviderName = NetworkProviderName.Visa,
                NetworkTokenUsage = NetworkTokenUsage.EcomMerchant,
                CountryCode = country,
                LanguageCode = GlobalConstants.Defaults.Language,
                AccountHolderName = paymentInstrument.PaymentInstrumentDetails.CardHolderName,
                ExpiryMonth = parsedExpiryMonth,
                ExpiryYear = parsedExpiryYear,
            };

            GetTokenMetadataResponse networkTokens = await this.SendPostRequest<GetTokenMetadataResponse>(
                requestUrl,
                "RequestToken",
                traceActivityId,
                requestTokenRequest,
                GenerateCustomerHeader(
                    $"{{\"customerType\": \"MSA\", \"puid\": \"{puid}\"}}",
                    $"{{\"email\":\"{email}\"}}",
                    "X509",
                    "1.0"),
                exposedFlightFeatures);

            return networkTokens;
        }

        public async Task<GetTokenizationEligibilityResponse> Tokenizable(string puid, string deviceId, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, string email, string bankIdentificationNumber, string cardProviderName, string networkTokenUsage)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.Tokenizable, bankIdentificationNumber, cardProviderName, networkTokenUsage);

            ////TODO change url, change response class if needed

            GetTokenizationEligibilityResponse networkTokens = await this.SendGetRequest<GetTokenizationEligibilityResponse>(
                requestUrl,
                deviceId,
                "Tokenizable",
                traceActivityId,
                GenerateCustomerHeader(
                    $"{{\"customerType\": \"MSA\", \"puid\": \"{puid}\"}}",
                    $"{{\"email\":\"{email}\"}}",
                    "X509",
                    "1.0"),
                exposedFlightFeatures);

            return networkTokens;
        }

        public async Task<GetCredentialsResponse> FetchCredentials(string ntid, string puid, string deviceId, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, string merchantURL, string storedProfile, string email)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.FetchCredentials, ntid);

            GetCredentialsRequest fetchCredentialsRequest = new GetCredentialsRequest
            {
                NetworkTokenUsage = NetworkTokenUsage.ThirdPartyMerchant,
                CredentialType = CredentialType.Dtvv,
                MerchantURL = merchantURL,
                Encryption = new EncryptionDetails
                {
                    StoredProfile = storedProfile,
                    CertificateFormat = "JWK",
                },
            };

            GetCredentialsResponse networkTokens = await this.SendPostRequest<GetCredentialsResponse>(
                requestUrl,
                "FetchCredentials",
                traceActivityId,
                fetchCredentialsRequest,
                GenerateCustomerHeader(
                    $"{{\"customerType\": \"MSA\", \"puid\": \"{puid}\"}}",
                    $"{{\"email\":\"{email}\"}}",
                    "X509",
                    "1.0"),
                exposedFlightFeatures);

            return networkTokens;
        }

        public async Task<RequestDeviceBindingResponse> RequestDeviceBinding(string ntid, string puid, string deviceId, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, string externalCardReference, string email, object sessionContext, object browserData)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.RequestDeviceBindingFido, ntid);

            //// TODO: need to add browserData and sessionContext passed form PIDLSDK
            RequestDeviceBindingFidoRequest requestDeviceBindingRequest = new RequestDeviceBindingFidoRequest
            {
                BrowserData = browserData,
                SessionContext = sessionContext,
                AccountHolderEmail = email,
                PlatformType = PlatformType.Windows, // Assuming Windows as default, this can be parameterized if needed
            };

            RequestDeviceBindingResponse requestDeviceBindingResponse = await this.SendPostRequest<RequestDeviceBindingResponse>(
                requestUrl,
                "FetchCredentials",
                traceActivityId,
                requestDeviceBindingRequest,
                GenerateCustomerHeader(
                    $"{{\"customerType\": \"MSA\", \"puid\": \"{puid}\"}}",
                    $"{{\"email\":\"{email}\"}}",
                    "X509",
                    "1.0"),
                exposedFlightFeatures);

            return requestDeviceBindingResponse;
        }

        public async Task<RequestChallengeResponse> RequestChallenge(string ntid, string challengeId, string challengeMethodId, string puid, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, string email)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.RequestChallenge, ntid, challengeId);

            //// TODO: need to add browserData and sessionContext passed form PIDLSDK
            RequestChallengeRequest requestChallengeRequest = new RequestChallengeRequest
            {
                NetworkTokenId = ntid,
                ChallengeMethodId = challengeMethodId,
            };

            RequestChallengeResponse requestChallengeResponse = await this.SendPostRequest<RequestChallengeResponse>(
                requestUrl,
                "RequestChallenge",
                traceActivityId,
                requestChallengeRequest,
                GenerateCustomerHeader(
                    $"{{\"customerType\": \"MSA\", \"puid\": \"{puid}\"}}",
                    $"{{\"email\":\"{email}\"}}",
                    "X509",
                    "1.0"),
                exposedFlightFeatures);

            return requestChallengeResponse;
        }

        public async Task<object> ValidateChallenge(string ntid, string challengeId, string challengeMethodId, string puid, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, string otp, string email)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.ValidateChallenge, ntid, challengeId);

            //// TODO: need to add browserData and sessionContext passed form PIDLSDK
            ValidateChallengeRequest validateChallengeRequest = new ValidateChallengeRequest
            {
                NetworkTokenId = ntid,
                ChallengeMethodId = challengeMethodId,
                Otp = otp
            };

            return await this.SendPostRequest<object>(
                requestUrl,
                "ValidateChallenge",
                traceActivityId,
                validateChallengeRequest,
                GenerateCustomerHeader(
                    $"{{\"customerType\": \"MSA\", \"puid\": \"{puid}\"}}",
                    $"{{\"email\":\"{email}\"}}",
                    "X509",
                    "1.0"),
                exposedFlightFeatures);
        }

        public async Task<PasskeyOperationResponse> PasskeyAuthenticate(string ntid, int authenticationAmount, string currencyCode, string puid, string deviceId, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, object sessionContext, object browserData, string applicationUrl, string merchantName, string email, PimsModel.V4.AddressInfo address)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.PasskeyAuthenticate, ntid);

            PasskeyOperationRequest requestChallengeRequest = new PasskeyOperationRequest
            {
                AuthenticationAmount = authenticationAmount,
                CurrencyCode = currencyCode,
                BrowserData = browserData,
                SessionContext = sessionContext,
                MerchantIdentifier = new MerchantIdentifier
                {
                    ApplicationUrl = applicationUrl,
                    MerchantName = merchantName
                },
                AddressInfo = address
            };

            PasskeyOperationResponse requestChallengeResponse = await this.SendPostRequest<PasskeyOperationResponse>(
                requestUrl,
                "PasskeyAuthenticate",
                traceActivityId,
                requestChallengeRequest,
                GenerateCustomerHeader(
                    $"{{\"customerType\": \"MSA\", \"puid\": \"{puid}\"}}",
                    $"{{\"email\":\"{email}\"}}",
                    "X509",
                    "1.0"),
                exposedFlightFeatures);

            return requestChallengeResponse;
        }

        public async Task<PasskeyOperationResponse> PasskeySetup(string ntid, int authenticationAmount, string currencyCode, string puid, string deviceId, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, object sessionContext, object browserData, string applicationUrl, string merchantName, string email)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.PasskeySetup, ntid);

            PasskeyOperationRequest requestChallengeRequest = new PasskeyOperationRequest
            {
                AuthenticationAmount = authenticationAmount,
                CurrencyCode = currencyCode,
                BrowserData = browserData,
                SessionContext = sessionContext,
                MerchantIdentifier = new MerchantIdentifier
                {
                    ApplicationUrl = applicationUrl,
                    MerchantName = merchantName
                },
            };

            PasskeyOperationResponse requestChallengeResponse = await this.SendPostRequest<PasskeyOperationResponse>(
                requestUrl,
                "PasskeySetup",
                traceActivityId,
                requestChallengeRequest,
                GenerateCustomerHeader(
                    $"{{\"customerType\": \"MSA\", \"puid\": \"{puid}\"}}",
                    $"{{\"email\":\"{email}\"}}",
                    "X509",
                    "1.0"),
                exposedFlightFeatures);

            return requestChallengeResponse;
        }

        public async Task<object> SetMandates(string ntid, string puid, EventTraceActivity traceActivityId, List<string> exposedFlightFeatures, object appInstance, AssuranceData assuranceData, List<Mandate> mandates, string dfSessionId, string email)
        {
            string requestUrl = string.Format(V7.Constants.UriTemplate.SetMandates, ntid);

            ////TODO: need to get appInstance and AssuranceData from PIDLSDK
            PasskeyMandateRequest requestChallengeRequest = new PasskeyMandateRequest
            {
                AppInstance = appInstance,
                AssuranceData = assuranceData,
                Mandates = mandates,
                DfpSessionId = dfSessionId
            };

            object passkeyMandateResponse = await this.SendPostRequest<object>(
                requestUrl,
                "SetMandates",
                traceActivityId,
                requestChallengeRequest,
                GenerateCustomerHeader(
                    $"{{\"customerType\": \"MSA\", \"puid\": \"{puid}\"}}",
                    $"{{\"email\":\"{email}\"}}",
                    "X509",
                    "1.0"),
                exposedFlightFeatures);

            return passkeyMandateResponse;
        }

        private static string GenerateCustomerHeader(string target, string requester = null, string authType = null, string version = null)
        {
            JsonSerializerSettings setting = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            };

            long totalSeconds = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            var payload = new Dictionary<string, object>()
            {
                { "requester", requester ?? "{}" },
                { "target", target },
                { "caller", "PX service" },
                { "authType", authType ?? string.Empty },
                { "version", version ?? string.Empty },
                { "nbf", totalSeconds },
                { "exp", totalSeconds + (24 * 60 * 60 * 60) },
                { "iss", "urn:microsoft:px" },
            };

            byte[] payloadBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload, setting));
            var jwtHeader = new Dictionary<string, object> { { "alg", "none" }, { "typ", "JWT" } };
            byte[] headerBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jwtHeader, setting));
            string encodedHeader = Convert.ToBase64String(headerBytes);
            encodedHeader = encodedHeader.Split('=')[0];
            encodedHeader = encodedHeader.Replace('+', '-');
            encodedHeader = encodedHeader.Replace('/', '_');

            string encodedPayload = Convert.ToBase64String(payloadBytes);
            encodedPayload = encodedPayload.Split('=')[0];
            encodedPayload = encodedPayload.Replace('+', '-');
            encodedPayload = encodedPayload.Replace('/', '_');

            return $"{encodedHeader}.{encodedPayload}.";
        }

        private async Task<T> SendGetRequest<T>(string requestUrl, string deviceId, string actionName, EventTraceActivity traceActivityId, string customerHeader, List<string> exposedFlightFeatures, IList<KeyValuePair<string, string>> additionalHeaders = null)
        {
            string fullRequestUrl = string.Format("{0}/{1}", this.BaseUrl, requestUrl);

            if (exposedFlightFeatures?.Contains(Flighting.Features.PXUseNTSIntUrl, StringComparer.OrdinalIgnoreCase) ?? false)
            {
                fullRequestUrl = string.Format("{0}/{1}", this.intServiceBaseUrl, requestUrl);
            }

            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, fullRequestUrl))
            {
                request.IncrementCorrelationVector(traceActivityId);
                request.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString());
                request.Headers.Add(GlobalConstants.HeaderValues.Version, this.apiVersion);
                request.Headers.Add(GlobalConstants.HeaderValues.CustomerHeader, customerHeader);
                request.Headers.Add(GlobalConstants.HeaderValues.DeviceInfoHeader, deviceId);

                PaymentsEventSource.Log.PXServiceRequestToNetworkTokenizationService(fullRequestUrl, traceActivityId);

                request.AddOrReplaceActionName(actionName);
                using (HttpResponseMessage response = await this.httpClient.SendAsync(request))
                {
                    return await this.HandleResponse<T>(response, traceActivityId);
                }
            }
        }

        private async Task<T> SendPostRequest<T>(string url, string actionName, EventTraceActivity traceActivityId, object request, string customerHeader, List<string> exposedFlightFeatures, IList<KeyValuePair<string, string>> additionalHeaders = null)
        {
            string fullRequestUrl = string.Format("{0}/{1}", this.BaseUrl, url);

            if (exposedFlightFeatures?.Contains(Flighting.Features.PXUseNTSIntUrl, StringComparer.OrdinalIgnoreCase) ?? false)
            {
                fullRequestUrl = string.Format("{0}/{1}", this.intServiceBaseUrl, url);
            }

            using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, fullRequestUrl))
            {
                requestMessage.IncrementCorrelationVector(traceActivityId);
                requestMessage.Headers.Add(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, traceActivityId.ActivityId.ToString());
                requestMessage.Headers.Add(GlobalConstants.HeaderValues.Version, this.apiVersion);
                requestMessage.Headers.Add(GlobalConstants.HeaderValues.CustomerHeader, customerHeader);

                if (request != null)
                {
                    requestMessage.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, PaymentConstants.HttpMimeTypes.JsonContentType); // lgtm[cs/sensitive-data-transmission] lgtm[cs/web/xss] The request is being made to a web service and not to a web page.
                }

                requestMessage.AddOrReplaceActionName(actionName);
                using (HttpResponseMessage response = await this.httpClient.SendAsync(requestMessage))
                {
                    return await this.HandleResponse<T>(response, traceActivityId);
                }
            }
        }

        private async Task<T> HandleResponse<T>(HttpResponseMessage response, EventTraceActivity traceActivityId)
        {
            string responseMessage = await response.Content.ReadAsStringAsync();

            PaymentsEventSource.Log.PXServiceResponseToNetworkTokenizationService(response.StatusCode.ToString(), responseMessage, traceActivityId);

            SllWebLogger.TraceServerMessage($"SendGetRequest_NetworkTokenizationServiceAccessor", traceActivityId.ToString(), null, JsonConvert.DeserializeObject(responseMessage)?.ToString(), Diagnostics.Tracing.EventLevel.Informational);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(responseMessage);
                }
                catch
                {
                    throw TraceCore.TraceException(traceActivityId, new FailedOperationException($"Failed to deserialize success response from {Constants.ServiceNames.NetworkTokenizationService}"));
                }
            }
            else
            {
                ServiceErrorResponse error = null;
                try
                {
                    ServiceErrorResponse innerError = JsonConvert.DeserializeObject<ServiceErrorResponse>(responseMessage);
                    if (innerError == null)
                    {
                        innerError = new ServiceErrorResponse();
                    }

                    innerError.Source = string.IsNullOrWhiteSpace(innerError.Source) ? Constants.ServiceNames.NetworkTokenizationService : innerError.Source;
                    error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), ServiceName, innerError);
                }
                catch
                {
                    throw TraceCore.TraceException(traceActivityId, new FailedOperationException($"Failed to deserialize error response from {Constants.ServiceNames.NetworkTokenizationService}"));
                }

                throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response, HandlingType = ExceptionHandlingPolicy.ByPass });
            }
        }
    }
}