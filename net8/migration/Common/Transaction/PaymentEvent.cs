// <copyright file="PaymentEvent.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Transaction
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class PaymentEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentEvent"/> class.
        /// </summary>
        public PaymentEvent()
        {
            this.PaymentAdditionalData = new Dictionary<string, object>();
            this.JournalAdditionalData = new Dictionary<string, object>();
            this.EventId = Guid.NewGuid();
            this.Timestamp = DateTime.UtcNow;
            this.ConfigurationContexts = new List<ConfigurationContext>();
        }

        /// <summary>
        /// Gets or sets the account id of this payment transaction's owner.
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// Gets or sets the payment identifier associated with this payment transaction.
        /// </summary>
        public string PaymentId { get; set; }

        /// <summary>
        /// Gets or sets the transaction id associated with the payment
        /// </summary>
        public Guid TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the payment transaction reference number, which is a formatted representation that includes
        /// payment id and transaction id
        /// </summary>
        public string PaymentTransactionReferenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the transaction type of the payment transaction
        /// </summary>
        public TransactionType TransactionType { get; set; }

        /// <summary>
        /// Gets or sets the status of the payment transaction
        /// </summary>
        public TransactionStatus TransactionStatus { get; set; }

        /// <summary>
        /// Gets or sets the eventId. Required by the journal.
        /// </summary>        
        public Guid EventId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp. Required by the journal.
        /// </summary>        
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the partner system that made the payment transaction on behalf of the store
        /// </summary>
        public string Store { get; set; }

        /// <summary>
        /// Gets or sets the device type.
        /// </summary>
        public string DeviceType { get; set; }
        
        /// <summary>
        /// Gets or sets the 3 letter ISO code for the Country of payment.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the country code that where the merchant is sold to.
        /// </summary>
        public string SoldToCountry { get; set; }

        /// <summary>
        /// Gets or sets the ISO 4217 Code for Currency of payment amount.
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets the payment transaction amount
        /// </summary>
        public decimal Amount { get; set; }

        public decimal AmountReceived { get; set; }

        /// <summary>
        /// Gets or sets the third party seller associated with the transaction
        /// </summary>
        public string ThirdPartySeller { get; set; }

        /// <summary>
        /// Gets or sets the SellerOfRecord associated with the transaction
        /// </summary>
        public string SellerOfRecord { get; set; }

        /// <summary>
        /// Gets or sets the StatusDetailsCode associated with the transaction
        /// </summary>
        public string StatusDetailsCode { get; set; }

        /// <summary>
        /// Gets or sets the StatusDetailsProcessorResponse associated with the transaction
        /// </summary>
        public object StatusDetailsProcessorResponse { get; set; }

        public string StatusDetailsDeclineMessage { get; set; }

        /// <summary>
        /// Gets or sets the MerchantReferenceNumber associated with the transaction
        /// </summary>
        public string MerchantReferenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the MerchantId associated with the transaction
        /// </summary>
        public string MerchantId { get; set; }
        
        /// <summary>
        /// Gets or sets the GatewayMerchantId associated with the transaction
        /// </summary>
        public string GatewayMerchantId { get; set; }

        /// <summary>
        /// Gets or sets the IssuerClassification associated with the transaction
        /// </summary>
        public string IssuerClassification { get; set; }

        /// <summary>
        /// Gets or sets the ProcessorName associated with the transaction
        /// </summary>
        public string ProcessorName { get; set; }
       
        /// <summary>
        /// Gets or sets the MerchantDescriptor associated with the transaction
        /// </summary>
        public string MerchantDescriptor { get; set; }

        /// <summary>
        /// Gets or sets the MerchantSupportInfo associated with the transaction
        /// </summary>
        public string MerchantSupportInfo { get; set; }

        /// <summary>
        /// Gets or sets the PaymentInstrumentId associated with the transaction
        /// </summary>
        public string PaymentInstrumentId { get; set; }

        public Guid ReferenceTransactionId { get; set; }

        public string SessionId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a recurring transaction
        /// </summary>
        public bool RecurringTransaction { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a retrying transaction
        /// </summary>
        public bool RetryTransaction { get; set; }

        public string RiskToken { get; set; }

        public TestContext TestContext { get; set; }

        public ExternalReference ExternalReference { get; set; }        

        public TransactionConfirmationType TransactionConfirmation { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227", Justification = "Needed to be set.")]
        public List<ConfigurationContext> ConfigurationContexts { get; set; }

        /// <summary>
        /// Gets a set of key value pairs of additional data required to complete the transaction. These are specific
        /// to payment methods and data to pass is inferred by the caller querying the payment instrument for additional
        /// data required.
        /// Example: SourceIPAddress
        /// </summary>
        public Dictionary<string, object> PaymentAdditionalData { get; private set; }

        /// <summary>
        /// Gets a set of key value pairs of additional data exposed to Journal service.
        /// Example: payment_method_name
        /// </summary>
        public Dictionary<string, object> JournalAdditionalData { get; private set; }

        public static PaymentEvent CreatePaymentEvent(PaymentContext paymentContext, PaymentTransaction paymentTransaction)
        {
            return new PaymentEvent
                {
                    PaymentId = paymentContext.PaymentId,
                    AccountId = paymentContext.AccountId,
                    TransactionId = paymentTransaction.Id,
                    TransactionType = paymentTransaction.TransactionType,
                    TransactionStatus = paymentTransaction.Status,
                    Country = paymentContext.Country ?? paymentTransaction.Country,
                    SoldToCountry = paymentTransaction.SoldToCountry,
                    Currency = paymentContext.Currency ?? paymentTransaction.Amount.Currency,
                    Amount = paymentTransaction.InitialTransactionAmount == null ? paymentTransaction.Amount.Amount : paymentTransaction.InitialTransactionAmount.Amount,
                    AmountReceived = ((paymentTransaction.Status == TransactionStatus.PartialApproved || paymentTransaction.Status == TransactionStatus.Approved || paymentTransaction.Status == TransactionStatus.OfflineApproved) && (paymentTransaction.TransactionType == TransactionType.Charge || paymentTransaction.TransactionType == TransactionType.Credit)) ? paymentTransaction.Amount.Amount : 0m,
                    PaymentTransactionReferenceNumber = paymentTransaction.GetPaymentTransactionReferenceNumber(paymentContext),
                    PaymentAdditionalData = paymentTransaction.PaymentAdditionalData,
                    JournalAdditionalData = paymentTransaction.JournalAdditionalData,
                    Store = paymentTransaction.Partner,
                    DeviceType = paymentTransaction.DeviceType,
                    ThirdPartySeller = paymentTransaction.ThirdPartySeller,
                    SellerOfRecord = paymentTransaction.SellerOfRecord,
                    StatusDetailsCode = paymentTransaction.StatusDetails == null ? TransactionDeclineCode.None.ToString() : paymentTransaction.StatusDetails.Code.ToString(),
                    StatusDetailsProcessorResponse = paymentTransaction.StatusDetails == null ? null : paymentTransaction.StatusDetails.ProcessorResponse,
                    StatusDetailsDeclineMessage = paymentTransaction.StatusDetails == null ? null : paymentTransaction.StatusDetails.DeclineMessage,
                    ProcessorName = paymentTransaction.ProviderName,
                    PaymentInstrumentId = paymentTransaction.PaymentInstrumentId,
                    ReferenceTransactionId = paymentTransaction.ReferenceTransactionId,
                    SessionId = paymentTransaction.SessionId,
                    MerchantReferenceNumber = paymentTransaction.MerchantReferenceNumber,
                    RecurringTransaction = paymentTransaction.RecurringTransaction,
                    RetryTransaction = paymentTransaction.RetryTransaction,
                    MerchantDescriptor = paymentTransaction.MerchantDescriptor,
                    MerchantSupportInfo = paymentTransaction.MerchantSupportInfo,
                    MerchantId = paymentTransaction.MerchantId,
                    IssuerClassification = paymentTransaction.IssuerClassification,
                    TestContext = paymentContext.TestContext,
                    ExternalReference = paymentTransaction.ExternalReference,
                    TransactionConfirmation = paymentTransaction.TransactionConfirmation,
                    GatewayMerchantId = paymentTransaction.GatewayMerchantId,
                    ConfigurationContexts = paymentContext.ConfigurationContexts,
                    RiskToken = paymentTransaction.RiskToken
                };
        }
    }
}
