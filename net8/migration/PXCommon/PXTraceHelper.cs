// <copyright file="InstrumentManagementTraceHelper.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Commerce.Payments.Common;
using System;

namespace Microsoft.Commerce.Payments.Management.Common
{
    public static class InstrumentManagementTraceHelper
    {
        private const string MobileCarrierBilling = "mobile_carrier_billing";

        public static void AddTracingProperties(this HttpRequest request, string accountId, string paymentInstrumentId, string family = null, string type = null, string country = null)
        {
            request.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.AccountId] = accountId;
            request.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.InstrumentId] = paymentInstrumentId;
            request.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.PaymentMethodFamily] = family;
            request.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.PaymentMethodType] = type;
            request.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.Country] = country?.ToUpperInvariant();
        }

        public static void AddCountryPropertyFromQuery(this HttpRequest request)
        {
            var query = QueryHelpers.ParseQuery(request.QueryString.Value);
            if (query.TryGetValue("country", out var country))
            {
                request.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.Country] = country.ToString().ToUpperInvariant();
            }
        }

        public static string GetOperationNameWithPendingOnInfo(this HttpRequest request)
        {
            string operationNameWithMoreInfo = request.Path; // fallback if no GetOperationName extension

            string pendingOn = request.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.PendingOn] as string;
            if (!string.IsNullOrEmpty(pendingOn))
            {
                operationNameWithMoreInfo += $"-{pendingOn}";
            }

            string family = request.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.PaymentMethodFamily] as string;
            if (!string.IsNullOrEmpty(family) && family.Equals(MobileCarrierBilling, StringComparison.OrdinalIgnoreCase))
            {
                operationNameWithMoreInfo += "-Mobi";
            }

            return operationNameWithMoreInfo;
        }

        public static void SkipLogging(this HttpRequest request)
        {
            request.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.SkipRequestLogging] = bool.TrueString;
        }

        public static void ResponseFromCache(this HttpRequest request, bool fromCache)
        {
            request.HttpContext.Items[PaymentConstants.Web.InstrumentManagementProperties.ResponseFromCache] = fromCache;
        }
    }
}
