// <copyright file="HttpRequestExtensions.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;
    using Microsoft.Commerce.Payments.Common.Web;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    public static class HttpRequestExtensions
    {
        private const string MobileCarrierBilling = "mobile_carrier_billing";

        private static bool TryGetOption(this HttpRequestMessage request, string key, out object value)
        {
            return request.Options.TryGetValue(GetOptionKey(key), out value);
        }

        private static HttpRequestOptionsKey<object> GetOptionKey(string key)
        {
            return new HttpRequestOptionsKey<object>(key);
        }

        private static void SetOption(this HttpRequestMessage request, string key, object value)
        {
            request.Options.Set(GetOptionKey(key), value);
        }

        public static object GetProperty(this HttpRequestMessage request, string propertyName)
        {
            object propertyValue = null;
            request.TryGetOption(propertyName, out propertyValue);
            return propertyValue;
        }

        public static bool ContainsProperty(this HttpRequestMessage request, string propertyName)
        {
            return request.TryGetOption(propertyName, out _);
        }

        public static bool TryGetProperty(this HttpRequestMessage request, string propertyName, out object value)
        {
            return request.TryGetOption(propertyName, out value);
        }

        public static void AddProperty(this HttpRequestMessage request, string propertyName, object value)
        {
            request.SetOption(propertyName, value);
        }

        public static void SetProperty(this HttpRequestMessage request, string propertyName, object value)
        {
            request.SetOption(propertyName, value);
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
        /// General method to add the tracing properties needed by the Tracing Handler to log these properties in the SLL logs.
        /// </summary>
        public static void AddTracingProperties(this HttpRequest request, string accountId, string paymentInstrumentId, string family = null, string type = null, string country = null)
        {
            request.AddAccountIdProperty(accountId);
            request.AddPaymentInstrumentIdProperty(paymentInstrumentId);
            request.AddPaymentMethodFamilyProperty(family);
            request.AddPaymentMethodTypeProperty(type);
            request.AddCountryProperty(country);
        }

        public static void AddPartnerProperty(this HttpRequest request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.HttpContext.Items[PaymentConstants.Web.Properties.Partner] = value;
            }
        }

        public static void AddScenarioProperty(this HttpRequest request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.HttpContext.Items[PaymentConstants.Web.Properties.Scenario] = value;
            }
        }

        public static void AddPidlOperation(this HttpRequest request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.HttpContext.Items[PaymentConstants.Web.Properties.PidlOperation] = value;
            }
        }

        public static EventTraceActivity GetServerTraceId(this HttpRequestMessage request)
        {
            object serverTraceIdProperty;
            request.TryGetOption(PaymentConstants.Web.Properties.ServerTraceId, out serverTraceIdProperty);
            return serverTraceIdProperty as EventTraceActivity;
        }

        public static void AddAvsSuggest(this HttpRequest request, bool value)
        {
            request.HttpContext.Items[PaymentConstants.Web.Properties.AvsSuggest] = value.ToString()?.ToLower();
        }

        public static void AddPaymentInstrumentIdProperty(this HttpRequest request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.InstrumentId] = value;
            }
        }

        public static void AddCountryProperty(this HttpRequest request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.Country] = value.ToUpperInvariant();
            }
        }

        public static void AddPaymentMethodFamilyProperty(this HttpRequest request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.PaymentMethodFamily] = value;
            }
        }

        public static void AddPaymentMethodTypeProperty(this HttpRequest request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.PaymentMethodType] = value;
            }
        }

        public static void AddAccountIdProperty(this HttpRequest request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.AccountId] = value;
            }
        }

        public static void AddErrorCodeProperty(this HttpRequest request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.ErrorCode] = value;
            }
        }

        public static void AddErrorMessageProperty(this HttpRequest request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.ErrorMessage] = value;
            }
        }

        public static string GetOperationNameWithPendingOnInfo(this HttpRequest request)
        {
            // get the original operation name created from route data
            string operationNameWithMoreInfo = request.GetOperationName();

            // get the pendingOn property
            string pendingOn = request.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.PendingOn] as string;
            if (!string.IsNullOrEmpty(pendingOn))
            {
                operationNameWithMoreInfo = string.Format("{0}-{1}", operationNameWithMoreInfo, pendingOn);
            }

            // if it's mobi, add the "Mobi" suffix
            string paymentMethodFamily = request.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.PaymentMethodFamily] as string;
            if (!string.IsNullOrEmpty(paymentMethodFamily) && paymentMethodFamily.Equals(MobileCarrierBilling, StringComparison.OrdinalIgnoreCase))
            {
                operationNameWithMoreInfo = string.Format("{0}-{1}", operationNameWithMoreInfo, "Mobi");
            }

            return operationNameWithMoreInfo;
        }
        public static EventTraceActivity GetCorrelationId(this HttpContext context)
        {
            string correlationId = context.Request.Headers["X-Correlation-ID"];

            if (!string.IsNullOrEmpty(correlationId) && Guid.TryParse(correlationId, out Guid correlationGuid))
            {
                return new EventTraceActivity(correlationGuid);
            }

            return new EventTraceActivity();
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

        private static readonly HttpRequestOptionsKey<RouteValueDictionary> RouteDataKey = new("RouteValues");

        public static RouteValueDictionary? GetRouteDataSafe(this HttpRequestMessage request)
        {
            if (request.Options.TryGetValue(RouteDataKey, out var routeData))
            {
                return routeData;
            }

            return null;
        }

        public static void SetRouteData(this HttpRequestMessage request, RouteValueDictionary routeData)
        {
            request.Options.Set(RouteDataKey, routeData);
        }
    }
}