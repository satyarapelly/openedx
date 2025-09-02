// <copyright file="HttpRequestExtensions.cs" company="Microsoft">Copyright (c) Microsoft 2025. All rights reserved.</copyright>

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Commerce.Payments.Common.Tracing;
using Microsoft.Commerce.Payments.Common.Transaction;
using Microsoft.CommonSchema.Services.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Commerce.Payments.Common.Web
{
    public static class HttpRequestExtensions
    {
        public const string IPHeader = "CLIENT-IP";
        public const string UserAgentHeader = "User-Agent";

        public static bool TryGetTrackingId(this HttpRequest request, out Guid trackingId)
        {
            trackingId = Guid.Empty;
            return request.Headers.TryGetValue(PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, out var headerValues)
                && Guid.TryParse(headerValues.FirstOrDefault(), out trackingId);
        }

        public static HttpRequestMessage ToHttpRequestMessage(this HttpRequest request)
        {
            var requestMessage = new HttpRequestMessage(new HttpMethod(request.Method), new Uri(request.GetDisplayUrl()));
            foreach (var header in request.Headers)
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

            return requestMessage;
        }

        public static IEnumerable<KeyValuePair<string, string>> GetQueryNameValuePairs(this HttpRequest request)
        {
            return request?.Query?.Select(q => new KeyValuePair<string, string>(q.Key, q.Value.ToString()))
                   ?? Enumerable.Empty<KeyValuePair<string, string>>();
        }

        public static bool TryGetQueryParameterValue(this HttpRequest request, string key, out string value, string defaultValue = null)
        {
            if (request?.Query.TryGetValue(key, out var result) == true)
            {
                value = result.ToString();
                return true;
            }

            value = defaultValue;
            return false;
        }

        public static string GetTrackingId(this HttpRequest request)
        {
            if (request.HttpContext.Items.TryGetValue(PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, out var trackingId))
            {
                return trackingId as string;
            }

            if (!request.Headers.TryGetValue(PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, out var headerValues))
            {
                var newTrackingId = Guid.NewGuid().ToString();
                request.HttpContext.Items[PaymentConstants.PaymentExtendedHttpHeaders.TrackingId] = newTrackingId;
                return newTrackingId;
            }

            return headerValues.FirstOrDefault();
        }

        public static bool TryGetTestContext(this HttpRequest request, out TestContext testContext)
        {
            testContext = null;
            if (request.Headers.TryGetValue(PaymentConstants.PaymentExtendedHttpHeaders.TestHeader, out var testHeaders))
            {
                foreach (var testHeader in testHeaders)
                {
                    testContext = JsonConvert.DeserializeObject<TestContext>(testHeader);
                    if (testContext != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool HasTestContext(this HttpRequest request)
        {
            return request.Headers.ContainsKey(PaymentConstants.PaymentExtendedHttpHeaders.TestHeader);
        }

        public static EventTraceActivity GetRequestCorrelationId(this HttpRequest request)
        {
            if (request.HttpContext.Items.TryGetValue(PaymentConstants.Web.Properties.ServerTraceId, out var result) && result is EventTraceActivity traceActivityId)
            {
                return traceActivityId;
            }

            var trace = new EventTraceActivity
            {
                CorrelationVectorV4 = request.GetCorrelationVector()
            };

            // Try to extract client correlation ID from headers
            if (request.Headers.TryGetValue(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, out StringValues headerValues) &&
                Guid.TryParse(headerValues.FirstOrDefault(), out var correlationId))
            {
                var clientTrace = new EventTraceActivity(correlationId)
                {
                    CorrelationVectorV4 = trace.CorrelationVectorV4
                };
                request.HttpContext.Items[PaymentConstants.Web.Properties.ClientTraceId] = clientTrace;
            }

            request.HttpContext.Items[PaymentConstants.Web.Properties.ServerTraceId] = trace;
            Sll.Context.Vector = trace.CorrelationVectorV4;

            return trace;
        }

        public static CorrelationVector GetCorrelationVector(this HttpRequest request)
        {
            if (!request.HttpContext.Items.TryGetValue(CorrelationVector.HeaderName, out var extendedVector))
            {
                var correlationVector = request.GetCorrelationVectorFromHeader();
                extendedVector = string.IsNullOrEmpty(correlationVector) ? new CorrelationVector() : CorrelationVector.Extend(correlationVector);
                request.HttpContext.Items[CorrelationVector.HeaderName] = extendedVector;
            }

            if (extendedVector is CorrelationVector cv)
            {
                cv.Increment();
                return cv;
            }

            return new CorrelationVector();
        }

        public static string GetCorrelationVectorFromHeader(this HttpRequest request)
        {
            return request.Headers.TryGetValue(CorrelationVector.HeaderName, out var values) ? values.FirstOrDefault() : null;
        }

        public static string GetDeviceType(this HttpRequest request)
        {
            if (request.Headers.TryGetValue(PaymentConstants.PaymentExtendedHttpHeaders.DeviceType, out var headerValues))
            {
                var deviceType = headerValues.FirstOrDefault();
                return deviceType?.Equals("Windows.Xbox", StringComparison.InvariantCultureIgnoreCase) == true ? "Xbox" : null;
            }

            return null;
        }

        public static int GetRequestContentLength(this HttpRequest request)
        {
            return request.ContentLength.HasValue ? (int)request.ContentLength.Value : 0;
        }

        public static string GetApiVersion(this HttpRequest request)
        {
            if (request.HttpContext.Items.TryGetValue(PaymentConstants.Web.Properties.Version, out var versionObj) && versionObj is ApiVersion version)
            {
                return version.ExternalVersion;
            }

            return null;
        }

        public static string GetRequestHeader(this HttpRequest request, string headerName)
        {
            return request.Headers.TryGetValue(headerName, out var values) ? values.FirstOrDefault() : null;
        }

        public static string GetClientIP(this HttpRequest request)
        {
            var ipWithPort = request.GetRequestHeader(IPHeader);
            if (!string.IsNullOrWhiteSpace(ipWithPort))
            {
                return ipWithPort.Split(':')[0];
            }
            return "127.0.0.1";
        }

        public static string GetUserAgent(this HttpRequest request)
        {
            return request.GetRequestHeader(UserAgentHeader);
        }

        public static string GetRequestHeaderString(this HttpRequest request)
        {
            if (request != null && request.Headers != null && request.Headers.Any())
            {
                return TraceBuilderHelper.BuildHeaderString(request.Headers);
            }

            return "<no headers>";
        }

        public static async Task<string> GetRequestPayloadAsync(this HttpRequest request)
        {
            if (request.Body == null || !request.ContentLength.HasValue || request.ContentLength == 0)
            {
                return "<none>";
            }

            // Allow multiple reads
            request.EnableBuffering();

            request.Body.Position = 0;
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            string body = await reader.ReadToEndAsync();

            // Reset position so downstream middleware can read again
            request.Body.Position = 0;

            return string.IsNullOrWhiteSpace(body) ? "<none>" : body;
        }

        public static async Task<string> ReadRequestBodyAsync(this HttpRequest request)
        {
            request.EnableBuffering();

            if (request.Body == null || !request.Body.CanRead)
                return "<none>";

            request.Body.Position = 0;

            using var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            var body = await reader.ReadToEndAsync();

            request.Body.Position = 0; // Reset for downstream middleware
            return string.IsNullOrWhiteSpace(body) ? "<none>" : body;
        }

        public static string GetServiceName(this HttpRequest request) =>
            request.Path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "unknown";
        public static string GetOperationName(this HttpRequest request)
        {
            if (request.HttpContext.Items.TryGetValue(PaymentConstants.Web.Properties.OperationName, out var value))
            {
                return value as string;
            }
            return null;
        }

        public static string GetVersion(this HttpRequest request)
        {
            if (request.HttpContext.Items.TryGetValue(PaymentConstants.Web.Properties.Version, out var value) && value is ApiVersion version)
            {
                return version.ExternalVersion;
            }
            return null;
        }

        public static Task<HttpResponseMessage> CreateJsonResponseAsync(this HttpRequestMessage request, HttpStatusCode statusCode, object content)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = JsonContent.Create(content)
            };

            return Task.FromResult(response);
        }

        public static HttpResponseMessage CreateResponse<T>(this HttpRequest request, T value)
        {
            return request.CreateResponse(HttpStatusCode.OK, value);
        }

        public static HttpResponseMessage CreateResponse(this HttpRequest request, HttpStatusCode statusCode, object value, string mediaType = "application/json")
        {
            var response = new HttpResponseMessage(statusCode);
            if (value != null)
            {
                var json = JsonConvert.SerializeObject(value);
                response.Content = new StringContent(json, Encoding.UTF8, mediaType);
            }
            return response;
        }

        public static HttpResponseMessage CreateErrorResponse(this HttpRequest request, HttpStatusCode statusCode, object error)
        {
            return request.CreateResponse(statusCode, error);
        }

        public static async Task<string> GetRequestPayload(this HttpRequest request)
        {
            if (request.ContentLength == null || request.ContentLength == 0)
            {
                return string.Empty;
            }

            request.EnableBuffering();
            request.Body.Position = 0;
            using var reader = new StreamReader(request.Body, leaveOpen: true);
            var payload = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return payload;
        }


    }
}
