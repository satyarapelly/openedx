// <copyright file="ExternalPaymentTokenTransformerFactory.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.V7
{
    using System;
    using System.Linq;
    using Microsoft.Commerce.Payments.Common;
    using Microsoft.Commerce.Payments.Common.Tracing;

    public class ExternalPaymentTokenTransformerFactory
    {
        /// <summary>
        ///  Create instance of token transformer to support Add PI/POST PI to PIMS
        /// </summary>
        /// <param name="paymentMethodType">Payment methods type e.g googlePay, applePaay</param>
        /// <param name="paymentToken">Payment token from provider</param>
        /// <param name="traceActivityId">Trace activity object to log telemetry</param>
        /// <returns>Object of the token transformer</returns>
        public static IExternalPaymentTokenTransformer Instance(string paymentMethodType, string paymentToken, EventTraceActivity traceActivityId)
        {
            try
            {
                if (QuickPaymentDescription.SupportedPaymentMethods.Contains(paymentMethodType, StringComparer.OrdinalIgnoreCase))
                {
                    switch (paymentMethodType)
                    {
                        case Constants.PaymentMethodType.ApplePay:
                            return new AppleTokenTransformer(paymentToken, traceActivityId);
                        case Constants.PaymentMethodType.GooglePay:
                            return new GoogleTokenTransformer(paymentToken, traceActivityId);
                    }
                }
            }
            catch
            {
                throw TraceCore.TraceException(traceActivityId, new FailedOperationException($"Failed to create instance of ExternalPaymentTokenTransformer for request Id: {paymentMethodType}"));
            }

            throw TraceCore.TraceException(traceActivityId, new NotSupportedException($"Invalid PaymentMethodType: {paymentMethodType}"));
        }

        /// <summary>
        ///  Create instance of token transformer to support generate PIDL
        /// </summary>
        /// <param name="paymentMethodType">Payment methods type e.g googlePay, applePaay</param>
        /// <param name="traceActivityId">Trace activity object to log telemetry</param>
        /// <returns>Object of the token transformer</returns>
        public static IExternalPaymentTokenTransformer Instance(string paymentMethodType, EventTraceActivity traceActivityId)
        {
            try
            {
                if (QuickPaymentDescription.SupportedPaymentMethods.Contains(paymentMethodType, StringComparer.OrdinalIgnoreCase))
                {
                    switch (paymentMethodType)
                    {
                        case Constants.PaymentMethodType.ApplePay:
                            return new AppleTokenTransformer();
                        case Constants.PaymentMethodType.GooglePay:
                            return new GoogleTokenTransformer();
                    }
                }
            }
            catch
            {
                throw TraceCore.TraceException(traceActivityId, new FailedOperationException($"Failed to create instance of ExternalPaymentTokenTransformer for request Id: {paymentMethodType}"));
            }

            throw TraceCore.TraceException(traceActivityId, new NotSupportedException($"Invalid PaymentMethodType: {paymentMethodType}"));
        }
    }
}