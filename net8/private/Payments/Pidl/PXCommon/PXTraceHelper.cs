// <copyright file="InstrumentManagementTraceHelper.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Management.Common
{
    using System;
	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.WebUtilities;
	using Microsoft.Commerce.Payments.Common;

    public static class InstrumentManagementTraceHelper
    {
        private const string MobileCarrierBilling = "mobile_carrier_billing";

        /// <summary>
        /// General method to add the tracing properties needed by the Trancing Handler to log this properties in the SLL logs.
        /// </summary>
        /// <param name="request">HttpRequestMessage to which the properties will be added.</param>
        /// <param name="accountId">The account id. Can be null.</param>
        /// <param name="paymentInstrumentId">The payment instrument id. Can be null.</param>
        /// <param name="family">The payment method family. Can be null.</param>
        /// <param name="type">The payment method type. Can be null.</param>
        /// <param name="country">Country of the Pi. Can be null.</param>
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
