// <copyright file="IPaymentOrchestratorServiceAccessor.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.PaymentOrchestratorService
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Commerce.Payments.PXService.Model.PaymentOrchestratorService;
    using Microsoft.Commerce.Payments.PXService.V7.PaymentChallenge.Model;
    using Microsoft.Commerce.Tracing;

    /// <summary>
    /// Provides functionality to access PaymentOrchestratorService
    /// </summary>
    public interface IPaymentOrchestratorServiceAccessor
    {
        /// <summary>
        /// Attaches the payment instrument to the payment request
        /// </summary>
        /// <param name="requestId"> Payment Rerquest Id</param>
        /// <param name="paymentInstrumentId">Payment Instrument Id</param>
        /// <param name="cvvToken">Cvv Token</param>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <param name="savePaymentDetails">Should save payment instrument as onfile usage type in PO</param>
        /// <returns>Attach Payment Instrument Response</returns>
        Task<AttachPaymentInstrumentResponse> AttachPaymentInstrument(string requestId, string paymentInstrumentId, string cvvToken, EventTraceActivity traceActivityId, string savePaymentDetails);

        /// <summary>
        /// Attaches the payment instrument to the payment request
        /// </summary>
        /// <param name="requestId"> Payment Rerquest Id</param>
        /// <param name="paymentInstrumentId">Payment Instrument Id</param>
        /// <param name="cvvToken">Cvv Token</param>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <param name="savePaymentDetails">Should save payment instrument as onfile usage type in PO</param>
        /// <returns>Attach Payment Instrument Response</returns>
        Task<AttachPaymentInstrumentResponse> AttachPaymentInstrumentToPaymentRequest(string requestId, string paymentInstrumentId, string cvvToken, EventTraceActivity traceActivityId, string savePaymentDetails);

        /// <summary>
        /// Delete eligible payment instrument from payment request
        /// </summary>
        /// <param name="paymentRequestId">Payment Request Id</param>
        /// <param name="piId">Payment Instrument Id</param>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <returns>Payment Request Client Actions</returns>
        Task<PaymentRequestClientActions> RemoveEligiblePaymentmethods(string paymentRequestId, string piId, EventTraceActivity traceActivityId);

        /// <summary>
        /// Attaches the address to the checkout request
        /// </summary>
        /// <param name="address">Address object</param>
        /// <param name="type">Payment Instrument Id</param>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <param name="checkoutRequestId">Checkout request Id</param>
        /// <returns>Attach Payment Instrument Response</returns>
        Task<CheckoutRequestClientActions> AttachAddress(Address address, string type, EventTraceActivity traceActivityId, string checkoutRequestId);

        /// <summary>
        /// Attaches the address to the payment request
        /// </summary>
        /// <param name="address">Address object</param>
        /// <param name="type">Payment Instrument Id</param>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <param name="paymentRequestId">Payment request Id</param>
        /// <returns>Attach Payment Instrument Response</returns>
        Task<PaymentRequestClientActions> AttachAddressToPaymentRequest(Address address, string type, EventTraceActivity traceActivityId, string paymentRequestId);

        /// <summary>
        /// Attaches the profile to the checkout request
        /// </summary>
        /// <param name="email"> Payment Rerquest Id</param>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <param name="checkoutRequestId">Checkout request Id</param>
        /// <returns>Attach Payment Instrument Response</returns>
        Task<CheckoutRequestClientActions> AttachProfile(string email, EventTraceActivity traceActivityId, string checkoutRequestId);

        /// <summary>
        /// Attaches the profile to the payment request
        /// </summary>
        /// <param name="email"> Payment Rerquest Id</param>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <param name="paymentRequestId">Payment request Id</param>
        /// <returns>Attach Payment Instrument Response</returns>
        Task<PaymentRequestClientActions> AttachProfileToPaymentRequest(string email, EventTraceActivity traceActivityId, string paymentRequestId);

        /// <summary>
        /// confirm to the checkout request
        /// </summary>
        /// <param name="piid"> Payment Rerquest Id</param>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <param name="checkoutRequestId">Checkout request Id</param>
        /// <returns>Attach Payment Instrument Response</returns>
        Task<CheckoutRequestClientActions> Confirm(string piid, EventTraceActivity traceActivityId, string checkoutRequestId);

        /// <summary>
        /// confirm to the payment request
        /// </summary>
        /// <param name="piid"> Payment Rerquest Id</param>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <param name="paymentRequestId">Payment request Id</param>
        /// <returns>Attach Payment Instrument Response</returns>
        Task<PaymentRequestClientActions> ConfirmToPaymentRequest(string piid, EventTraceActivity traceActivityId, string paymentRequestId);

        /// <summary>
        /// get client action
        /// </summary>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <param name="checkoutRequestId">Checkout request Id</param>
        /// <returns>Attach Payment Instrument Response</returns>
        Task<CheckoutRequestClientActions> GetClientAction(EventTraceActivity traceActivityId, string checkoutRequestId);

        /// <summary>
        /// get client action
        /// </summary>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <param name="paymentRequestId">payment request Id</param>
        /// <returns>Attach Payment Instrument Response</returns>
        Task<PaymentRequestClientActions> GetClientActionForPaymentRequest(EventTraceActivity traceActivityId, string paymentRequestId);

        /// <summary>
        /// Attaches the challenge data to the payment request
        /// </summary>
        /// <param name="paymentRequestId">Payment Request Id</param>
        /// <param name="piId">Payment Instrument Id</param>
        /// <param name="challengeType">Challenge type</param>/// 
        /// <param name="challengeValue">challengeValue like Cvv Token</param>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <returns>Payment Request Client Actions</returns>
        Task<PaymentRequestClientActions> PaymentRequestAttachChallengeData(string paymentRequestId, string piId, PaymentInstrumentChallengeType challengeType, string challengeValue, EventTraceActivity traceActivityId);

        /// <summary>
        /// Attaches the challenge data to the payment request
        /// </summary>
        /// <param name="requestId">Request Id, e.g. payment request id or checkout request id</param>
        /// <param name="piId">Payment Instrument Id</param>
        /// <param name="challengeType">challenge type</param> 
        /// <param name="challengeStatus">challenge status</param>
        /// <param name="paymentSessionId">payment SessionId</param>
        /// <param name="traceActivityId">Trace activity Id</param>
        /// <param name="tenantId">Tenant Id</param>
        /// <returns>Payment Request Client Actions</returns>
        Task<object> PSD2AttachChallengeData(string requestId, string piId, PaymentInstrumentChallengeType challengeType, PaymentChallengeStatus challengeStatus, string paymentSessionId, EventTraceActivity traceActivityId, string tenantId);

        /// <summary>
        /// Attaches the challenge data to the payment request
        /// </summary>
        /// <param name="requestId">Payment Request Id</param>
        /// <param name="piId">Payment Instrument Id</param>
        /// <param name="challengeType">challenge type</param> 
        /// <param name="challengeStatus">challenge status</param>
        /// <param name="paymentSessionId">payment SessionId</param>
        /// <param name="traceActivityId">Trace activity Id</param>
        /// <param name="tenantId">Tenant Id</param>
        /// <returns>Payment Request Client Actions</returns>
        Task<PaymentRequestClientActions> PSD2AttachChallengeDataToPaymentRequest(string requestId, string piId, PaymentInstrumentChallengeType challengeType, PaymentChallengeStatus challengeStatus, string paymentSessionId, EventTraceActivity traceActivityId, string tenantId);

        /// <summary>
        /// Confirms the request for payment
        /// </summary>        
        /// <param name="paymentRequestId">Payment Request Id</param>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <returns>Payment Request</returns>
        Task<PaymentRequestClientActions> PaymentRequestConfirm(string paymentRequestId, EventTraceActivity traceActivityId);

        /// <summary>
        /// Get the payment request
        /// </summary>
        /// <param name="paymentRequestId">Payment Request Id</param>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <returns>Payment request</returns>
        Task<PaymentRequest> GetPaymentRequest(string paymentRequestId, EventTraceActivity traceActivityId);

        /// <summary>
        /// Get eligible payment methods from payment orchestrator service
        /// </summary>
        /// <param name="traceActivityId">Trace Activity Id</param>
        /// <param name="requestId">Request Id</param>
        /// <returns>List of eligible payment methods</returns>
        Task<WalletEligiblePaymentMethods> GetEligiblePaymentMethods(EventTraceActivity traceActivityId, string requestId);
    }
}