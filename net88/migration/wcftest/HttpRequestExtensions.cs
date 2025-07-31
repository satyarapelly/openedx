// <copyright file="HttpRequestExtensions.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Tracing;
    using Microsoft.CommonSchema.Services.Logging;
    using Microsoft.Extensions.Primitives;

    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Try to get a tracking id from request header
        /// </summary>
        /// <param name="request">Http Request</param>
        /// <param name="trackingId">tracking id</param>
        /// <returns>if there is a valid tracking id in request header, return true, else return false</returns>
        public static bool TryGetTrackingId(this HttpRequest request, out Guid trackingId)
        {
            trackingId = Guid.Empty;
            if (request.Headers.TryGetValue(PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, out StringValues headerValues))
            {
                return Guid.TryParse(headerValues.FirstOrDefault(), out trackingId);
            }

            return false;
        }

        /// <summary>
        /// Gets a tracking id from request header or creates one if none exists
        /// </summary>
        /// <param name="request">Http Request</param>
        /// <returns>if there is a valid tracking id in request header, return true, else return false</returns>
        public static string GetTrackingId(this HttpRequest request)
        {
            if (request.HttpContext.Items.TryGetValue(PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, out var trackingId))
            {
                return trackingId as string;
            }

            if (!request.Headers.TryGetValue(PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, out StringValues headerValues))
            {
                var newId = Guid.NewGuid().ToString();
                request.HttpContext.Items[PaymentConstants.PaymentExtendedHttpHeaders.TrackingId] = newId;
                return newId;
            }

            return headerValues.FirstOrDefault();
        }

        /// <summary>
        /// HasTestContext is used to do a quick check on the request, whether it contains the test header string.
        /// </summary>
        /// <param name="request">http request</param>
        /// <returns>the bool value</returns>
        public static bool HasTestContext(this HttpRequest request)
        {
            return request.Headers.TryGetValue(PaymentConstants.PaymentExtendedHttpHeaders.TestHeader, out _);
        }

        /// <summary>
        /// Gets a correlation id for this request
        /// NOTED: As GetCorrelationId is already defined in System.Net.Http, we name our version to GetRequestCorrelationId instead
        /// </summary>
        /// <param name="request">http request</param>
        /// <returns>a GUID in header or a new GUID created by this method</returns>
        public static EventTraceActivity GetRequestCorrelationId(this HttpRequest request)
        {
            if (request.HttpContext.Items.TryGetValue(PaymentConstants.Web.Properties.ServerTraceId, out var result))
            {
                return (EventTraceActivity)result;
            }

            var traceActivityId = new EventTraceActivity
            {
                CorrelationVectorV4 = request.GetCorrelationVector()
            };

            if (request.Headers.TryGetValue(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, out StringValues headerValues) &&
                Guid.TryParse(headerValues.FirstOrDefault(), out Guid correlationId))
            {
                var clientTraceActivityId = new EventTraceActivity(correlationId)
                {
                    CorrelationVectorV4 = traceActivityId.CorrelationVectorV4
                };
                PaymentsEventSource.Log.CommonMappingCorrelationId(traceActivityId, clientTraceActivityId);
                request.HttpContext.Items[PaymentConstants.Web.Properties.ClientTraceId] = clientTraceActivityId;
            }

            request.HttpContext.Items[PaymentConstants.Web.Properties.ServerTraceId] = traceActivityId;
            Sll.Context.Vector = traceActivityId.CorrelationVectorV4;

            return traceActivityId;
        }

        /// <summary>
        ///     Gets the correlation vector or a new one if none exists in header
        /// </summary>
        /// <param name="request">http request</param>
        /// <returns >Returns the correlation vector or a new one if none exists in header</returns>
        public static CorrelationVector GetCorrelationVector(this HttpRequest request)
        {
            if (request.HttpContext.Items.TryGetValue(CorrelationVector.HeaderName, out var extendedVector))
            {
                if (extendedVector is CorrelationVector incrementVector)
                {
                    incrementVector.Increment();
                    return incrementVector;
                }
            }

            string correlationVector = request.GetCorrelationVectorFromHeader();
            if (!string.IsNullOrEmpty(correlationVector))
            {
                extendedVector = CorrelationVector.Extend(correlationVector);
            }
            else if (request.HttpContext.Items.TryGetValue(PaymentConstants.Web.Properties.ServerTraceId, out var result))
            {
                extendedVector = ((EventTraceActivity)result).CorrelationVectorV4?.Increment();
            }

            var newCorrelationVector = extendedVector as CorrelationVector ?? new CorrelationVector();
            request.HttpContext.Items[CorrelationVector.HeaderName] = newCorrelationVector;

            return newCorrelationVector;
        }

        public static string GetCorrelationVectorFromHeader(this HttpRequest request)
        {
            if (request.Headers.TryGetValue(CorrelationVector.HeaderName, out StringValues correlationVectorHeaderValues) &&
                correlationVectorHeaderValues.Count > 0)
            {
                return correlationVectorHeaderValues.First();
            }

            return null;
        }

        public static string GetClientDeviceId(this HttpRequest request)
        {
            if (request.Headers.TryGetValue(PaymentConstants.PaymentExtendedHttpHeaders.ClientDeviceId, out StringValues clientDeviceIdheaderValues) &&
                clientDeviceIdheaderValues.Count > 0)
            {
                return clientDeviceIdheaderValues.First();
            }

            return null;
        }

        public static CorrelationContext GetOrCreateCorrelationContextFromRequestProperty(this HttpRequest request)
        {
            if (request.HttpContext.Items.TryGetValue(PaymentConstants.Web.Properties.CorrelationContext, out object ccobject) &&
                ccobject is CorrelationContext cc)
            {
                return cc;
            }

            return new CorrelationContext();
        }

        /// <summary>
        /// Gets the device type from http request header, and only allow Windows.Xbox to Xbox conversion for workaround.
        /// </summary>
        /// <param name="request">http request</param>
        /// <returns >Returns the device type or null if none exists in header</returns>
        public static string GetDeviceType(this HttpRequest request)
        {
            if (request.Headers.TryGetValue(PaymentConstants.PaymentExtendedHttpHeaders.DeviceType, out StringValues headerValues))
            {
                string deviceType = headerValues.FirstOrDefault();
                if (string.Equals("Windows.Xbox", deviceType, StringComparison.InvariantCultureIgnoreCase))
                {
                    return "Xbox";
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the API version in the header
        /// </summary>
        /// <param name="request">http request</param>
        /// <returns>a version object if there is an api version info in header, otherwise, null</returns>
        public static ApiVersion GetApiVersion(this HttpRequest request)
        {
            return (ApiVersion)request.HttpContext.Items[PaymentConstants.Web.Properties.Version];
        }

        /// <summary>
        /// Get the content length of request for SLL tracing.
        /// </summary>
        /// <param name="request">The http request</param>
        /// <returns>The content size of request.</returns>
        public static int GetRequestContentLength(this HttpRequest request)
        {
            if (request.ContentLength.HasValue)
            {
                return Convert.ToInt32(request.ContentLength.Value);
            }

            return 0;
        }

        /// <summary>
        /// Gets a header value for the specified header name
        /// </summary>
        /// <param name="request">http request</param>
        /// <param name="headerName">header name</param>
        /// <returns>a string value if the header name is one of header keys, otherwise null</returns>
        public static string GetRequestHeader(this HttpRequest request, string headerName)
        {
            if (request.Headers.TryGetValue(headerName, out StringValues headerValues))
            {
                return headerValues.FirstOrDefault();
            }

            return null;
        }

        public static void IncrementCorrelationVector(this HttpRequest request, EventTraceActivity traceActivityId)
        {
            if (traceActivityId.CorrelationVectorV4 == null || string.IsNullOrWhiteSpace(traceActivityId.CorrelationVectorV4.Value))
            {
                request.Headers.Add(CorrelationVector.HeaderName, new CorrelationVector().Value);
            }
            else
            {
                request.Headers.Add(CorrelationVector.HeaderName, traceActivityId.CorrelationVectorV4.Increment());
            }
        }

        public static async Task<string> GetRequestPayload(this HttpRequest request)
        {
            if (request.ContentLength == null || request.ContentLength == 0)
            {
                return "<none>";
            }

            if (!request.HttpContext.Items.ContainsKey(PaymentConstants.Web.Properties.Content))
            {
                request.EnableBuffering();
                request.Body.Position = 0;
                using var reader = new StreamReader(request.Body, leaveOpen: true);
                var requestPayload = await reader.ReadToEndAsync();
                request.Body.Position = 0;
                request.HttpContext.Items[PaymentConstants.Web.Properties.Content] = requestPayload;
            }

            var payload = request.HttpContext.Items[PaymentConstants.Web.Properties.Content] as string;
            return string.IsNullOrWhiteSpace(payload) ? "<none>" : payload;
        }

        public static string GetServiceName(this HttpRequest request)
        {
            var pathAndQuery = request.Path + request.QueryString.ToString();
            return pathAndQuery.Split(new[] { '/', '?' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        }

        public static string GetRequestHeaderString(this HttpRequest request)
        {
            var headers = request.Headers.Select(h => new KeyValuePair<string, IEnumerable<string>>(h.Key, h.Value.AsEnumerable()));
            return TraceBuilderHelper.BuildHeaderString(headers);
        }

        public static string GetRequestCallerName(this HttpRequest request)
        {
            request.HttpContext.Items.TryGetValue(PaymentConstants.Web.Properties.CallerName, out var callerObject);
            return callerObject as string;
        }

        public static string GetOperationName(this HttpRequest request)
        {
            request.HttpContext.Items.TryGetValue(PaymentConstants.Web.Properties.OperationName, out var propertyValue);
            return propertyValue as string;
        }

        public static string GetVersion(this HttpRequest request)
        {
            request.HttpContext.Items.TryGetValue(PaymentConstants.Web.Properties.Version, out var propertyValue);
            if (propertyValue is ApiVersion version)
            {
                return version.ExternalVersion;
            }

            return null;
        }

        public static void AddOrReplaceActionName(this HttpRequest request, string actionName)
        {
            request.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.ActionName] = actionName;
        }

        public static string GetActionName(this HttpRequest request)
        {
            request.HttpContext.Items.TryGetValue(PaymentConstants.Web.InstrumentManagementProperties.ActionName, out var actionName);
            return actionName as string;
        }

        public static EventTraceActivity GetServerTraceId(this HttpRequest request)
        {
            request.HttpContext.Items.TryGetValue(PaymentConstants.Web.Properties.ServerTraceId, out var serverTraceIdProperty);
            return serverTraceIdProperty as EventTraceActivity;
        }

        public static object GetProperty(this HttpRequest request, string propertyName)
        {
            if (request.HttpContext.Items == null)
            {
                return null;
            }

            request.HttpContext.Items.TryGetValue(propertyName, out var propertyValue);
            return propertyValue;
        }

        public static void AddAuthenticationDetailsProperty(this HttpRequest request, string authInfo)
        {
            request.HttpContext.Items[PaymentConstants.Web.Properties.AuthenticationDetails] = authInfo;
        }
    }
}

