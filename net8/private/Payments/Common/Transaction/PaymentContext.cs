// <copyright file="PaymentContext.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Transaction
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Commerce.Payments.Common.Tracing;

    /// <summary>
    /// Payment Context holds on to key information required to identify and look up a payment.
    /// </summary>
    public class PaymentContext
    {
        public PaymentContext()
        {
            this.ConfigurationContexts = new List<ConfigurationContext>();
            this.TransactionContexts = new Dictionary<string, object>();
            this.CachedTransactions = new Dictionary<string, PaymentTransaction>();
            this.ScenarioContext = new List<string>();
            this.DynamicConfigurationContext = new DynamicConfigurationContext();
        }

        /// <summary>
        /// Gets or sets the tracking id associated with the payment request
        /// </summary>
        public Guid TrackingId { get; set; }

        /// <summary>
        /// Gets or sets the account id associated with the payment request
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// Gets or sets the payment id associated with the payment request
        /// </summary>
        public string PaymentId { get; set; }

        /// <summary>
        /// Gets or sets the event trace activity identifier, also known as correlation identifier, for tracing
        /// </summary>
        public EventTraceActivity TraceActivityId { get; set; }

        /// <summary>
        /// Gets or sets user context, optionally passed in by the caller, that is used to correlate all payment
        /// related transactions for the external entity. The payment system simply round trips this.
        /// </summary>
        public string UserContext { get; set; }

        /// <summary>
        /// Gets or sets the 3 letter ISO code for the Country of payment.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the ISO 4217 Code for Currency of payment amount.
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets the payment method name from transaction service wrapper.
        /// </summary>
        public string PaymentMethodName { get; set; }

        /// <summary>
        /// Gets or sets the payment method subtype from transaction service wrapper.
        /// </summary>
        public string PaymentMethodSubType { get; set; }

        /// <summary>
        /// Gets or sets the payment_method_type from PIMS
        /// </summary>
        public string PaymentMethodType { get; set; }

        /// <summary>
        /// Gets or sets the payment_method_family from PIMS
        /// </summary>
        public string PaymentMethodFamily { get; set; }

        public string BackupProviderName { get; set; }

        public bool PickProviderWithContext { get; set; }

        public int MaxTransactionVersionPerPayment { get; set; }

        public TestContext TestContext { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<ConfigurationContext> ConfigurationContexts { get; set; }

        public DynamicConfigurationContext DynamicConfigurationContext { get; set; }

        public List<string> ScenarioContext { get; private set; }

        /// <summary>
        /// Gets the transaction context populated from transaction service, and pass through to each payment provider.
        /// </summary>
        public Dictionary<string, object> TransactionContexts { get; private set; }

        public bool StubContextPopulated { get; set; }

        public int CurrentRowVersion { get; set; }

        public Dictionary<string, PaymentTransaction> CachedTransactions { get; private set; }

        public string FlightingExperimentId
        {
            get
            {
                string resultId = null;
                if (this.ConfigurationContexts != null && this.ConfigurationContexts.Count > 0)
                {
                    resultId = string.Join(",", this.ConfigurationContexts.Select(p => p.Version));
                }

                if (!string.IsNullOrEmpty(this.DynamicConfigurationContext.Id))
                {
                    resultId = (resultId ?? string.Empty) + string.Format(",{0}", this.DynamicConfigurationContext.Id);
                }

                return resultId;
            }
        }

        public string ScenarioId
        {
            get
            {
                string resultId = null;
                if (this.ScenarioContext != null && this.ScenarioContext.Count > 0)
                {
                    resultId = string.Join(",", this.ScenarioContext);
                }

                if (!string.IsNullOrEmpty(this.DynamicConfigurationContext.Id))
                {
                    resultId = (resultId ?? string.Empty) + string.Format(",{0}", this.DynamicConfigurationContext.Id);
                }

                return resultId;
            }
        }

        public void AddPaymentTransactionToCache(PaymentTransaction paymentTransaction)
        {
            string key = string.Format("{0}_{1}_{2}", this.PaymentId, paymentTransaction.Id, paymentTransaction.RowVersion);
            this.CachedTransactions[key] = paymentTransaction;
        }

        public PaymentTransaction GetPaymentTransactionFromCache(Guid transactionId, int version)
        {
            string key = string.Format("{0}_{1}_{2}", this.PaymentId, transactionId, version);
            if (this.CachedTransactions.ContainsKey(key))
            {
                return this.CachedTransactions[key];
            }

            return null;
        }
    }
}
