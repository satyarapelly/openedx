// <copyright file="HttpRequestMessageExtensions.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Tracing;
    using Microsoft.CommonSchema.Services.Logging;

    public static class HttpRequestMessageExtensions
    {
        private const string MobileCarrierBilling = "mobile_carrier_billing";

        private static HttpRequestOptionsKey<object> GetOptionKey(string key)
        {
            return new HttpRequestOptionsKey<object>(key);
        }

        private static bool TryGetOption(this HttpRequestMessage request, string key, out object value)
        {
            return request.Options.TryGetValue(GetOptionKey(key), out value);
        }

        private static void SetOption(this HttpRequestMessage request, string key, object value)
        {
            request.Options.Set(GetOptionKey(key), value);
        }
        /// <summary>
        /// Try to get a tracking id from request header
        /// </summary>
        /// <param name="request">Http Request</param>
        /// <param name="trackingId">tracking id</param>
        /// <returns>if there is a valid tracking id in request header, return true, else return false</returns>
        public static bool TryGetTrackingId(this HttpRequestMessage request, out Guid trackingId)
        {
            IEnumerable<string> headerValues;
            trackingId = Guid.Empty;
            return request.Headers.TryGetValues(PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, out headerValues) &&
                   Guid.TryParse(headerValues.FirstOrDefault(), out trackingId);
        }

        /// <summary>
        /// Gets a tracking id from request header or creates one if none exists
        /// </summary>
        /// <param name="request">Http Request</param>
        /// <returns>if there is a valid tracking id in request header, return true, else return false</returns>
        public static string GetTrackingId(this HttpRequestMessage request)
        {
            if (request.TryGetOption(PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, out var trackingId))
            {
                return trackingId as string;
            }

            IEnumerable<string> headerValues;
            if (!request.Headers.TryGetValues(PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, out headerValues))
            {
                string newId = Guid.NewGuid().ToString();
                request.SetOption(PaymentConstants.PaymentExtendedHttpHeaders.TrackingId, newId);
                return newId;
            }

            return headerValues.FirstOrDefault();
        }

        /// <summary>
        /// HasTestContext is used to do a quick check on the request, whether it contains the test header string.
        /// </summary>
        /// <param name="request">http request message</param>
        /// <returns>the bool value</returns>
        public static bool HasTestContext(this HttpRequestMessage request)
        {
            IEnumerable<string> testHeaders = null;

            return request.Headers.TryGetValues(PaymentConstants.PaymentExtendedHttpHeaders.TestHeader, out testHeaders);
        }

        /// <summary>
        /// Gets a correlation id for this request
        /// NOTED: As GetCorrelationId is already defined in System.Net.Http, we name our version to GetRequestCorrelationId instead
        /// </summary>
        /// <param name="request">http request message</param>
        /// <returns>a GUID in header or a new GUID created by this method</returns>
        public static EventTraceActivity GetRequestCorrelationId(this HttpRequestMessage request)
        {
            EventTraceActivity traceActivityId = null;
            if (request.TryGetOption(PaymentConstants.Web.Properties.ServerTraceId, out var result))
            {
                traceActivityId = (EventTraceActivity)result;
            }
            else
            {
                // server correlationId
                Guid correlationId = Guid.Empty;
                traceActivityId = new EventTraceActivity();
                traceActivityId.CorrelationVectorV4 = request.GetCorrelationVector();

                IEnumerable<string> headerValues;
                if (request.Headers.TryGetValues(PaymentConstants.PaymentExtendedHttpHeaders.CorrelationId, out headerValues) &&
                    Guid.TryParse(headerValues.FirstOrDefault(), out correlationId))
                {
                    // client correlationId
                    EventTraceActivity clientTraceActivityId = new EventTraceActivity(correlationId) { CorrelationVectorV4 = traceActivityId.CorrelationVectorV4 };
                    PaymentsEventSource.Log.CommonMappingCorrelationId(traceActivityId, clientTraceActivityId);
                    request.SetOption(PaymentConstants.Web.Properties.ClientTraceId, clientTraceActivityId);
                }

                request.SetOption(PaymentConstants.Web.Properties.ServerTraceId, traceActivityId);
                Sll.Context.Vector = traceActivityId.CorrelationVectorV4;
            }

            return traceActivityId;
        }

        /// <summary>
        ///     Gets the correlation vector or a new one if none exists in header
        /// </summary>
        /// <param name="request">http request message</param>
        /// <returns >Returns the correlation vector or a new one if none exists in header</returns>
        public static CorrelationVector GetCorrelationVector(this HttpRequestMessage request)
        {
            object extendedVector = null;

            if (request.TryGetOption(CorrelationVector.HeaderName, out extendedVector))
            {
                CorrelationVector incrementVector = extendedVector as CorrelationVector;
                if (incrementVector != null)
                {
                    incrementVector.Increment();
                    return incrementVector;
                }
            }

            // Check if header contains CV
            string correlationVector = request.GetCorrelationVectorFromHeader();
            if (!string.IsNullOrEmpty(correlationVector))
            {
                extendedVector = CorrelationVector.Extend(correlationVector);
            }
            else
            {
                // try to get it from serverTraceId
                object result;
                if (request.TryGetOption(PaymentConstants.Web.Properties.ServerTraceId, out result))
                {
                    extendedVector = ((EventTraceActivity)result).CorrelationVectorV4?.Increment();
                }
            }

            CorrelationVector newCorrelationVector = extendedVector as CorrelationVector ?? new CorrelationVector();

            request.SetOption(CorrelationVector.HeaderName, newCorrelationVector);

            return newCorrelationVector;
        }

        public static string GetCorrelationVectorFromHeader(this HttpRequestMessage request)
        {
            IEnumerable<string> correlationVectorHeaderValues;
            if (request.Headers.TryGetValues(CorrelationVector.HeaderName, out correlationVectorHeaderValues)
                && correlationVectorHeaderValues.Count() > 0)
            {
                return correlationVectorHeaderValues.First();
            }
            else
            {
                return null;
            }
        }

        public static string GetClientDeviceId(this HttpRequestMessage request)
        {
            IEnumerable<string> clientDeviceIdheaderValues;
            if (request.Headers.TryGetValues(PaymentConstants.PaymentExtendedHttpHeaders.ClientDeviceId, out clientDeviceIdheaderValues)
                && clientDeviceIdheaderValues.Count() > 0)
            {
                return clientDeviceIdheaderValues.First();
            }
            else
            {
                return null;
            }
        }

        public static CorrelationContext GetOrCreateCorrelationContextFromRequestProperty(this HttpRequestMessage request)
        {
            if (request.TryGetOption(PaymentConstants.Web.Properties.CorrelationContext, out object ccobject))
            {
                var cc = ccobject as CorrelationContext;
                if (cc != null)
                {
                    return cc;
                }
            }

            return new CorrelationContext();
        }

        /// <summary>
        /// Gets the device type from http request header, and only allow Windows.Xbox to Xbox conversion for workaround.
        /// </summary>
        /// <param name="request">http request message</param>
        /// <returns >Returns the device type or null if none exists in header</returns>
        public static string GetDeviceType(this HttpRequestMessage request)
        {
            // TODO, remove after fully transition to API payload from header.
            IEnumerable<string> headerValues;

            if (request.Headers.TryGetValues(PaymentConstants.PaymentExtendedHttpHeaders.DeviceType, out headerValues))
            {
                string deviceType = headerValues.FirstOrDefault();
                if (string.Equals("Windows.Xbox", deviceType, StringComparison.InvariantCultureIgnoreCase))
                {
                    return "Xbox";
                }

                return null;
            }

            return null;
        }

        /// <summary>
        /// Gets the API version in the header
        /// </summary>
        /// <param name="request">http request message</param>
        /// <returns>a version object if there is an api version info in header, otherwise, null</returns>
        public static ApiVersion GetApiVersion(this HttpRequestMessage request)
        {
            request.TryGetOption(PaymentConstants.Web.Properties.Version, out var version);
            return (ApiVersion)version;
        }

        /// <summary>
        /// Get the content length of request for SLL tracing.
        /// </summary>
        /// <param name="request">The http requst message</param>
        /// <returns>The content size of request.</returns>
        public static int GetRequestContentLength(this HttpRequestMessage request)
        {
            if (request.Content != null &&
                request.Content.Headers != null &&
                request.Content.Headers.ContentLength.HasValue)
            {
                return Convert.ToInt32(request.Content.Headers.ContentLength.Value);
            }

            return 0;
        }

        /// <summary>
        /// Gets a header value for the specified header name
        /// </summary>
        /// <param name="request">http request message</param>
        /// <param name="headerName">header name</param>
        /// <returns>a string value if the header name is one of header keys, otherwise null</returns>
        public static string GetRequestHeader(this HttpRequestMessage request, string headerName)
        {
            IEnumerable<string> headerValues;
            if (request.Headers.TryGetValues(headerName, out headerValues))
            {
                return headerValues.FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// Clones a request message.
        /// In retry scenario, we need a new cloned request instead of reusing the original request message which has already sent
        /// </summary>
        /// <param name="request">the original request message</param>
        /// <returns>a cloned request message</returns>
        public static HttpRequestMessage Clone(this HttpRequestMessage request)
        {
            HttpRequestMessage clonedRequest = new HttpRequestMessage(request.Method, request.RequestUri);

            clonedRequest.Content = request.Content;
            clonedRequest.Version = request.Version;

            foreach (KeyValuePair<string, object> prop in request.Options)
            {
                clonedRequest.Options.Set(new HttpRequestOptionsKey<object>(prop.Key), prop.Value);
            }

            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            {
                clonedRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clonedRequest;
        }

        public static void IncrementCorrelationVector(this HttpRequestMessage request, EventTraceActivity traceActivityId)
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

        public static async Task<string> GetRequestPayload(this HttpRequestMessage request)
        {
            string requestPayload;
            if (request.Content == null
                || request.Content.Headers == null
                || request.Content.Headers.ContentLength == 0)
            {
                requestPayload = "<none>";
            }
            else
            {
                if (!request.TryGetOption(PaymentConstants.Web.Properties.Content, out var content))
                {
                    requestPayload = await request.Content.ReadAsStringAsync();
                    request.SetOption(PaymentConstants.Web.Properties.Content, requestPayload);
                }
                else
                {
                    requestPayload = (string)content;
                }
            }

            return string.IsNullOrWhiteSpace(requestPayload) ? "<none>" : requestPayload;
        }

        public static string GetServiceName(this HttpRequestMessage request)
        {
            var pathAndQuery = request.RequestUri.IsAbsoluteUri ? request.RequestUri.PathAndQuery : request.RequestUri.ToString();
            return pathAndQuery.Split(new[] { '/', '?' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        }

        public static string GetRequestHeaderString(this HttpRequestMessage request)
        {
            return TraceBuilderHelper.BuildHeaderString(request.Headers);
        }

        public static string GetRequestCallerName(this HttpRequestMessage request)
        {
            object callerObject;
            request.TryGetOption(PaymentConstants.Web.Properties.CallerName, out callerObject);
            return (string)callerObject;
        }

        public static string GetOperationName(this HttpRequestMessage request)
        {
            object propertyValue;
            request.TryGetOption(PaymentConstants.Web.Properties.OperationName, out propertyValue);
            return propertyValue as string;
        }

        public static string GetVersion(this HttpRequestMessage request)
        {
            object propertyValue;
            request.TryGetOption(PaymentConstants.Web.Properties.Version, out propertyValue);
            if (propertyValue != null)
            {
                ApiVersion version = propertyValue as ApiVersion;
                if (version != null)
                {
                    return version.ExternalVersion;
                }
            }

            return null;
        }

        public static void AddOrReplaceActionName(this HttpRequestMessage request, string actionName)
        {
            request.SetOption(PaymentConstants.Web.InstrumentManagementProperties.ActionName, actionName);
        }

        public static string GetActionName(this HttpRequestMessage request)
        {
            object actionName;
            request.TryGetOption(PaymentConstants.Web.InstrumentManagementProperties.ActionName, out actionName);
            return actionName as string;
        }

        public static EventTraceActivity GetServerTraceId(this HttpRequestMessage request)
        {
            object serverTraceIdProperty;
            request.TryGetOption(PaymentConstants.Web.Properties.ServerTraceId, out serverTraceIdProperty);
            return serverTraceIdProperty as EventTraceActivity;
        }

        public static object GetProperty(this HttpRequestMessage request, string propertyName)
        {
            object propertyValue = null;
            request.TryGetOption(propertyName, out propertyValue);
            return propertyValue;
        }

        public static void AddAuthenticationDetailsProperty(this HttpRequestMessage request, string authInfo)
        {
            request.SetOption(PaymentConstants.Web.Properties.AuthenticationDetails, authInfo);
        }

        public static void AddTracingProperties(this HttpRequestMessage request, string accountId, string paymentInstrumentId, string family = null, string type = null, string country = null)
        {
            request.AddAccountIdProperty(accountId);
            request.AddPaymentInstrumentIdProperty(paymentInstrumentId);
            request.AddPaymentMethodFamilyProperty(family);
            request.AddPaymentMethodTypeProperty(type);
            request.AddCountryProperty(country);
        }

        public static void AddPartnerProperty(this HttpRequestMessage request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.SetOption(PaymentConstants.Web.Properties.Partner, value);
            }
        }

        public static void AddScenarioProperty(this HttpRequestMessage request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.SetOption(PaymentConstants.Web.Properties.Scenario, value);
            }
        }

        public static void AddPidlOperation(this HttpRequestMessage request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.SetOption(PaymentConstants.Web.Properties.PidlOperation, value);
            }
        }

        public static void AddAvsSuggest(this HttpRequestMessage request, bool value)
        {
            request.SetOption(PaymentConstants.Web.Properties.AvsSuggest, value.ToString().ToLowerInvariant());
        }

        public static void AddPaymentInstrumentIdProperty(this HttpRequestMessage request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.SetOption(PaymentConstants.Web.InstrumentManagementProperties.InstrumentId, value);
            }
        }

        public static void AddCountryProperty(this HttpRequestMessage request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.SetOption(PaymentConstants.Web.InstrumentManagementProperties.Country, value.ToUpperInvariant());
            }
        }

        public static void AddPaymentMethodFamilyProperty(this HttpRequestMessage request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.SetOption(PaymentConstants.Web.InstrumentManagementProperties.PaymentMethodFamily, value);
            }
        }

        public static void AddPaymentMethodTypeProperty(this HttpRequestMessage request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.SetOption(PaymentConstants.Web.InstrumentManagementProperties.PaymentMethodType, value);
            }
        }

        public static void AddAccountIdProperty(this HttpRequestMessage request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.SetOption(PaymentConstants.Web.InstrumentManagementProperties.AccountId, value);
            }
        }

        public static void AddErrorCodeProperty(this HttpRequestMessage request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.SetOption(PaymentConstants.Web.InstrumentManagementProperties.ErrorCode, value);
            }
        }

        public static void AddErrorMessageProperty(this HttpRequestMessage request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.SetOption(PaymentConstants.Web.InstrumentManagementProperties.ErrorMessage, value);
            }
        }

        public static string GetOperationNameWithPendingOnInfo(this HttpRequestMessage request)
        {
            string operationNameWithMoreInfo = request.GetOperationName();

            string pendingOn = request.GetProperty(PaymentConstants.Web.InstrumentManagementProperties.PendingOn) as string;
            if (!string.IsNullOrEmpty(pendingOn))
            {
                operationNameWithMoreInfo = string.Format("{0}-{1}", operationNameWithMoreInfo, pendingOn);
            }

            string paymentMethodFamily = request.GetProperty(PaymentConstants.Web.InstrumentManagementProperties.PaymentMethodFamily) as string;
            if (!string.IsNullOrEmpty(paymentMethodFamily) && paymentMethodFamily.Equals(MobileCarrierBilling, StringComparison.OrdinalIgnoreCase))
            {
                operationNameWithMoreInfo = string.Format("{0}-{1}", operationNameWithMoreInfo, "Mobi");
            }

            return operationNameWithMoreInfo;
        }
    }
}
