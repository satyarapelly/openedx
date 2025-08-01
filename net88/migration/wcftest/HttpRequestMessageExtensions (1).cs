// <copyright file="HttpRequestMessageExtensions.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXCommon
{
    using System;
    using System.Net.Http;
    using System.Web;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Web;

    public static class HttpRequestMessageExtensions
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
                request.Properties[PaymentConstants.Web.Properties.Partner] = value;
            }
        }

        public static void AddScenarioProperty(this HttpRequestMessage request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.Properties[PaymentConstants.Web.Properties.Scenario] = value;
            }
        }

        public static void AddPidlOperation(this HttpRequestMessage request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.Properties[PaymentConstants.Web.Properties.PidlOperation] = value;
            }
        }

        public static void AddAvsSuggest(this HttpRequestMessage request, bool value)
        {
            request.Properties[PaymentConstants.Web.Properties.AvsSuggest] = value.ToString()?.ToLower();
        }

        public static void AddPaymentInstrumentIdProperty(this HttpRequestMessage request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.Properties[PaymentConstants.Web.InstrumentManagementProperties.InstrumentId] = value;
            }
        }

        public static void AddCountryProperty(this HttpRequestMessage request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.Properties[PaymentConstants.Web.InstrumentManagementProperties.Country] = value.ToUpperInvariant();
            }
        }

        public static void AddPaymentMethodFamilyProperty(this HttpRequestMessage request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.Properties[PaymentConstants.Web.InstrumentManagementProperties.PaymentMethodFamily] = value;
            }
        }

        public static void AddPaymentMethodTypeProperty(this HttpRequestMessage request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.Properties[PaymentConstants.Web.InstrumentManagementProperties.PaymentMethodType] = value;
            }
        }

        public static void AddAccountIdProperty(this HttpRequestMessage request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.Properties[PaymentConstants.Web.InstrumentManagementProperties.AccountId] = value;
            }
        }

        public static void AddErrorCodeProperty(this HttpRequestMessage request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.Properties[PaymentConstants.Web.InstrumentManagementProperties.ErrorCode] = value;
            }
        }

        public static void AddErrorMessageProperty(this HttpRequestMessage request, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                request.Properties[PaymentConstants.Web.InstrumentManagementProperties.ErrorMessage] = value;
            }
        }

        public static string GetOperationNameWithPendingOnInfo(this HttpRequestMessage request)
        {
            // get the original operation name created from route data
            string operationNameWithMoreInfo = request.GetOperationName();

            // get the pendingOn property
            string pendingOn = request.GetProperty(PaymentConstants.Web.InstrumentManagementProperties.PendingOn) as string;
            if (!string.IsNullOrEmpty(pendingOn))
            {
                operationNameWithMoreInfo = string.Format("{0}-{1}", operationNameWithMoreInfo, pendingOn);
            }

            // if it's mobi, add the "Mobi" suffix
            string paymentMethodFamily = request.GetProperty(PaymentConstants.Web.InstrumentManagementProperties.PaymentMethodFamily) as string;
            if (!string.IsNullOrEmpty(paymentMethodFamily) && paymentMethodFamily.Equals(MobileCarrierBilling, StringComparison.OrdinalIgnoreCase))
            {
                operationNameWithMoreInfo = string.Format("{0}-{1}", operationNameWithMoreInfo, "Mobi");
            }

            return operationNameWithMoreInfo;
        }
    }
}
