// <copyright file="ServiceClient.cs" company="Microsoft">Copyright (c) Microsoft 2017. All rights reserved.</copyright>

namespace Test.Common
{
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Transaction;
    using Microsoft.Commerce.Payments.Common.Web;
    using Microsoft.Commerce.Tracing;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Serialization;

    public class ServiceClient
    {
        public const string HeaderTrackingId = "x-ms-tracking-id";
        public const string HeaderCorrelationId = "x-ms-correlation-id";
        public const string HeaderTest = "x-ms-test";
        public const string ApiVersion = "api-version";
        public const string AcceptHeader = "Accept";
        public const string AcceptLanguage = "Accept-Language";
        public const string HeaderFlight = "x-ms-flight";

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new SnakeCasingJsonContractResolver()
        };

        private readonly ServiceClientSettings settings;
        private readonly HttpClientHandler handler;
        private readonly HttpClient httpClient;

        public ServiceClient(ServiceClientSettings settings)
        {
            this.settings = settings;
            this.handler = new HttpClientHandler();

            if (settings.ClientCertificate != null)
            {
                this.handler.ClientCertificates.Add(settings.ClientCertificate);
            }

            this.httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(settings.ServiceEndpoint)
            };
        }

        public HttpStatusCode SendRequest<T>(
            string url,
            string method,
            TestContext context,
            string requestContent,
            Guid? trackingId,
            Dictionary<string, string> requestHeaders,
            EventTraceActivity traceActivityId,
            out T responseOutput,
            out WebHeaderCollection responseHeaders,
            string contentType = Constants.HeaderValues.JsonContent,
            Constants.AuthenticationType authenticationType = Constants.AuthenticationType.AAD,
            Constants.AADClientType aadClientType = Constants.AADClientType.PME) where T : class
        {
            responseOutput = null;
            responseHeaders = new WebHeaderCollection();
            var request = new HttpRequestMessage(new HttpMethod(method), url);

            // Auth headers
            if (authenticationType == Constants.AuthenticationType.AAD)
            {
                var token = settings.AadTokenProviders[aadClientType].AcquireToken().Result;
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            else if (authenticationType == Constants.AuthenticationType.TestAADFallsBackToCert)
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "fakeToken");
            }

            // Tracking headers
            var corrId = trackingId ?? Guid.NewGuid();
            request.Headers.Add(HeaderTrackingId, corrId.ToString());
            request.Headers.Add(HeaderCorrelationId, traceActivityId.ActivityId.ToString());
            request.Headers.Add(Microsoft.CommonSchema.Services.Logging.CorrelationVector.HeaderName, new Microsoft.CommonSchema.Services.Logging.CorrelationVector().Value);

            if (requestHeaders != null)
            {
                foreach (var kv in requestHeaders)
                {
                    request.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                }
            }

            if (context != null)
            {
                request.Headers.Add(HeaderTest, JsonConvert.SerializeObject(context));
            }

            request.Headers.TryAddWithoutValidation(AcceptHeader, GetAcceptHeader());

            // Set body
            if (!string.IsNullOrEmpty(requestContent))
            {
                request.Content = new StringContent(requestContent, Encoding.UTF8, contentType);
            }

            HttpResponseMessage response = httpClient.Send(request);
            var responseBody = response.Content.ReadAsStringAsync().Result;

            foreach (var header in response.Headers)
            {
                responseHeaders.Add(header.Key, string.Join(",", header.Value));
            }

            if (response.IsSuccessStatusCode ||
                response.StatusCode == HttpStatusCode.BadGateway ||
                response.StatusCode == HttpStatusCode.GatewayTimeout ||
                response.StatusCode == HttpStatusCode.Conflict ||
                response.StatusCode == HttpStatusCode.NotFound)
            {
                responseOutput = Deserialize<T>(responseBody, response.Content.Headers.ContentType?.MediaType);
            }
            else if (response.StatusCode == HttpStatusCode.InternalServerError ||
                     response.StatusCode == HttpStatusCode.BadRequest)
            {
                Trace.TraceWarning($"Error response with httpStatusCode '{response.StatusCode}': {responseBody}");
                responseOutput = responseBody as T;
            }

            return response.StatusCode;
        }

        public HttpWebRequest CreateRequest(
            string pathAndQuery,
            Constants.AADClientType aadClientType = Constants.AADClientType.PME,
            Constants.AuthenticationType authenticationType = Constants.AuthenticationType.AAD)
        {
            throw new NotSupportedException("HttpWebRequest is deprecated in .NET 8. Use SendRequest instead.");
        }

        protected virtual string GetContentType() => "application/json";

        protected virtual string GetAcceptHeader() => "application/json";

        protected virtual string SerializeObject(object requestInput, string contentType)
        {
            if (contentType.ToLower().Contains("application/json"))
            {
                return JsonConvert.SerializeObject(requestInput, SerializerSettings);
            }
            else if (contentType.ToLower().Contains("application/xml") || contentType.ToLower().Contains("text/xml"))
            {
                var serializer = new XmlSerializer(requestInput.GetType());
                using var sw = new StringWriter();
                serializer.Serialize(sw, requestInput);
                return sw.ToString();
            }
            else
            {
                throw new NotSupportedException($"{contentType} is not supported");
            }
        }

        protected virtual T Deserialize<T>(string value, string contentType)
        {
            if (string.IsNullOrEmpty(value))
            {
                return default;
            }

            if (contentType.ToLower().Contains("application/json"))
            {
                return JsonConvert.DeserializeObject<T>(value, SerializerSettings);
            }
            else if (contentType.ToLower().Contains("application/xml") || contentType.ToLower().Contains("text/xml"))
            {
                var serializer = new XmlSerializer(typeof(T));
                using var sr = new StringReader(value);
                return (T)serializer.Deserialize(sr);
            }
            else
            {
                throw new NotSupportedException($"{contentType} is not supported");
            }
        }
    }
}
