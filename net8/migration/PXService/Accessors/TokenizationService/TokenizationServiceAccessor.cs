// <copyright file="TokenizationServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.TokenizationService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Payments.PXCommon;
    using Microsoft.Commerce.Payments.PXService.Model.TokenizationService;
    using Newtonsoft.Json;

    public class TokenizationServiceAccessor : ITokenizationServiceAccessor
    {
        private HttpClient tokenizationServiceHttpClient;

        private string serviceBaseUrl;

        private string encryptionKey;
        private DateTime publicKeyLastUpdatedTime;

        // Public Key refresh interval every 1 hours
        private int publicKeyRefreshInternvalInSec = 3600;
        private object lockObj = new object();
        private string tokenizationGetTokenURL;
        private string tokenizationGetTokenFromEncryptedValueURL;

        public TokenizationServiceAccessor(
            string serviceBaseUrl,
            string emulatorBaseUrl,            
            string tokenizationGetTokenURL,
            string tokenizationGetTokenFromEncryptedValueURL,
            HttpMessageHandler messageHandler)
        {
            this.serviceBaseUrl = serviceBaseUrl;

            this.tokenizationServiceHttpClient = new PXTracingHttpClient(Constants.ServiceNames.TokenizationService, messageHandler);

            this.publicKeyLastUpdatedTime = DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(this.publicKeyRefreshInternvalInSec));

            this.tokenizationGetTokenURL = tokenizationGetTokenURL;
            this.tokenizationGetTokenFromEncryptedValueURL = tokenizationGetTokenFromEncryptedValueURL;
        }

        private string BaseUrl
        {
            get
            {
                return this.serviceBaseUrl;
            }
        }

        public Dictionary<string, string> GetTokenizationServiceUrls()
        {
            return new Dictionary<string, string>
            {
                { "getTokenURL", this.tokenizationGetTokenURL },
                { "getTokenFromEncryptedValueURL", this.tokenizationGetTokenFromEncryptedValueURL }
            };
        }

        public async Task<string> GetEncryptionKey(EventTraceActivity traceActivityId, List<string> exposedFlightFeatures = null)
        {
            // If the caching flight is not enabled then always call the tokenization service to get latest key to avoid caching.
            if (!exposedFlightFeatures.Contains(Flighting.Features.PXEnableCachingTokenizationEncryption, StringComparer.OrdinalIgnoreCase))
            {
                return await this.SendRequestEncryptionKey(traceActivityId, exposedFlightFeatures);
            }
            else
            {
                // If the encryption key is already available and the key is not expired then return the key from cache.
                if (!string.IsNullOrEmpty(this.encryptionKey) &&
                    DateTime.UtcNow.Subtract(this.publicKeyLastUpdatedTime).TotalSeconds <= this.publicKeyRefreshInternvalInSec)
                {
                    return this.encryptionKey;
                }

                var encryptionKeyResponse = await this.SendRequestEncryptionKey(traceActivityId, exposedFlightFeatures);

                lock (this.lockObj)
                {
                    this.encryptionKey = encryptionKeyResponse;
                    this.publicKeyLastUpdatedTime = DateTime.UtcNow;
                }

                return this.encryptionKey;
            }
        }

        private async Task<string> SendRequestEncryptionKey(EventTraceActivity traceActivityId, List<string> exposedFlightFeatures)
        {
            IList<KeyValuePair<string, string>> additionalHeaders = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(Constants.HeaderKey.RequesterKey, Constants.HeaderValue.RequesterValue)
            };

            GetEncryptionKeyResponse encryptionKeyResponse = await this.SendRequest<GetEncryptionKeyResponse>(
                method: HttpMethod.Get,
                actionPath: "tokens",
                actionName: "GetEncryptionKey",
                traceActivityId: traceActivityId,
                payload: null,
                additionalHeaders: additionalHeaders,
                exposedFlightFeatures: exposedFlightFeatures);

            // Get all the valid keys
            var validKeys = encryptionKeyResponse.Keys.ToArray().Where(k => DateTimeOffset.FromUnixTimeSeconds(k.Nbf) < DateTime.UtcNow && DateTimeOffset.FromUnixTimeSeconds(k.Exp) > DateTime.UtcNow).ToList();

            // Sort the keys by expiration time and return the first key with highest expiration time
            validKeys.Sort((a, b) => DateTimeOffset.Compare(DateTimeOffset.FromUnixTimeSeconds(a.Exp), DateTimeOffset.FromUnixTimeSeconds(b.Exp)));
            return JsonConvert.SerializeObject(validKeys.First(), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        private async Task<T> SendRequest<T>(
            HttpMethod method,
            string actionPath,
            string actionName,
            EventTraceActivity traceActivityId,
            object payload,
            IList<KeyValuePair<string, string>> additionalHeaders,
            List<string> exposedFlightFeatures = null)
        {
            string fullRequestUrl = string.Format("{0}/{1}/{2}", this.BaseUrl, actionPath, actionName);

            using (HttpRequestMessage requestMessage = new HttpRequestMessage(method, fullRequestUrl))
            {
                requestMessage.IncrementCorrelationVector(traceActivityId);

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
                    var serializerSettings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    };

                    requestMessage.Content = new StringContent(JsonConvert.SerializeObject(payload, Formatting.None, serializerSettings));
                    requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(PaymentConstants.HttpMimeTypes.JsonContentType);
                }

                using (HttpResponseMessage response = await this.tokenizationServiceHttpClient.SendAsync(requestMessage))
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
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException($"Failed to deserialize success response from {Constants.ServiceNames.TokenizationService}. Response Message: {responseMessage}"));
                        }
                    }
                    else
                    {
                        ServiceErrorResponse error = null;
                        try
                        {
                            ServiceErrorResponse innerError = JsonConvert.DeserializeObject<ServiceErrorResponse>(responseMessage);
                            innerError.Source = string.IsNullOrWhiteSpace(innerError.Source) ? Constants.ServiceNames.TokenizationService : innerError.Source;
                            error = new ServiceErrorResponse(traceActivityId.ActivityId.ToString(), GlobalConstants.ServiceName, innerError.ToString());
                        }
                        catch
                        {
                            throw TraceCore.TraceException(traceActivityId, new FailedOperationException($"Failed to deserialize error response from {Constants.ServiceNames.TokenizationService}. Response Message: {responseMessage}"));
                        }

                        throw TraceCore.TraceException(traceActivityId, new ServiceErrorResponseException() { Error = error, Response = response, HandlingType = ExceptionHandlingPolicy.ByPass });
                    }
                }
            }
        }
    }
}