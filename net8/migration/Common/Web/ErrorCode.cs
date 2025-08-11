// <copyright file="ErrorCode.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Web
{
    /// <summary>
    /// API error codes
    /// </summary>
    public enum ErrorCode
    {
        /// <summary>
        /// The error occurred when interacting with an external payment processor
        /// </summary>
        ExternalProcessor,

        /// <summary>
        /// The error occurred when there is no tracking id in request header
        /// </summary>
        NoTrackingId,

        /// <summary>
        /// There are some invalid data items in request object
        /// </summary>
        InvalidRequestData,

        /// <summary>
        /// There are some invalid data items in request object
        /// </summary>
        InvalidStateTransitionRequested,

        /// <summary>
        /// Internal service error, usually means coordinator error
        /// </summary>
        ServiceError,

        /// <summary>
        /// The transaction request is declined, maybe by Risk or Provider
        /// </summary>
        RequestDeclined,

        /// <summary>
        /// The transaction request is failed, due to payment internal issue
        /// </summary>
        RequestFailed,

        /// <summary>
        /// The transaction request is failed, maybe it is a duplicate request
        /// </summary>
        InvalidTrackingId,

        /// <summary>
        /// The transaction request is failed, due to an unrecognizable currency code
        /// </summary>
        InvalidCurrencyCode,

        /// <summary>
        /// The transaction request is failed, because the currency is not supported for this provider, market, or seller
        /// </summary>
        CurrencyNotAllowed,

        /// <summary>
        /// The error occured when the invalid parameter passed from the request.
        /// </summary>
        InvalidParameter,

        /// <summary>
        /// The transaction request is failed, due to an invalid payment instrument Id
        /// </summary>
        InvalidPaymentInstrumentId,

        /// <summary>
        /// The transaction request is failed, due to an invalid payment instrument type
        /// </summary>
        InvalidPaymentInstrumentType,

        /// <summary>
        /// The transaction request is failed, due to invalid data in payment instrument properties
        /// </summary>
        InvalidPaymentInstrumentDetails,

        /// <summary>
        /// The transaction request is failed, due to an invalid payment method family
        /// </summary>
        InvalidPaymentMethodFamily,

        /// <summary>
        /// The transaction request is failed, because the payment instrument is not active
        /// </summary>
        PaymentInstrumentNotActive,

        /// <summary>
        /// The transaction request is failed, because the operation is not supported
        /// </summary>
        OperationNotSupported,

        /// <summary>
        /// The transaction request is failed, because the request is null
        /// </summary>
        NullRequest,

        /// <summary>
        /// The transaction request is failed, due to trying to refund an amount greater than the settled amount
        /// </summary>
        RefundExceededTransactionAmount,

        /// <summary>
        /// The transaction request is failed, due to trying to capture an amount greater than the authorized limit
        /// </summary>
        CaptureAmountLimitExceeded,

        /// <summary>
        /// The transaction request is failed, due to the recurring is not supported
        /// </summary>
        RecurringTransactionNotSupported,

        /// <summary>
        /// The specified payment method does not support multiple lineitems.
        /// </summary>
        MultipleLineItemsNotSupported,

        /// <summary>
        /// Can't select a merchant basing on the currency/country/market etc.
        /// </summary>
        MerchantSelectionFailure,

        /// <summary>
        /// When querying using a given transaction-id or a tracking-id the transaction was not found
        /// </summary>
        TransactionNotFound,

        /// <summary>
        /// The requested reduce amount is either greater or lesser than permitted on a transaction
        /// permitted = {amountExpected - SumOf_PartialAmountsReceived + SumOf_PriorReduceRequests}
        /// </summary>
        InvalidReduceAmount,

        /// <summary>
        /// Not able to find payment instrument 
        /// </summary>
        PaymentInstrumentNotFound,

        /// <summary>
        /// Invalid Account Id
        /// </summary>
        InvalidAccountId,

        /// <summary>
        /// Settings Version Mismatch
        /// </summary>
        SettingsVersionMismatch,

        /// <summary>
        /// A request with insufficient previleges attempts to create a Moto (on behalf of) payment session
        /// </summary>
        UnauthorizedMotoPaymentSession,

        /// <summary>
        /// user email not found on the request
        /// </summary>
        EmailNotFound,

        /// <summary>
        /// PUID not found on the request
        /// </summary>
        PuidNotFound,

        /// <summary>
        /// IP Address not found on the request
        /// </summary>
        IPAddressNotFound,

        /// <summary>
        /// Property cannot be null
        /// </summary>
        CannotBeNull,

        /// <summary>
        /// PaymentMethodId is expected to be unique for a user
        /// </summary>
        DuplicatePaymentMethodIdNotSupported
    }
}
