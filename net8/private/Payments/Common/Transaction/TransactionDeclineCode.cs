// <copyright file="TransactionDeclineCode.cs" company="Microsoft">Copyright (c) Microsoft 2013. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common.Transaction
{
    using Microsoft.Commerce.Payments.Common.Web;
    using Newtonsoft.Json;

    /// <summary>
    /// NOTE: Please also update the cosmos stream https://cosmos15.osdinfra.net/cosmos/cp.prod.PIR/local/Payments/Working/ClassicModernMappings/StatusDetailsCodeDescriptionMapping.ss?property=info
    /// if there is any updates.
    /// </summary>
    [JsonConverter(typeof(EnumJsonConverter))]
    public enum TransactionDeclineCode
    {
        /// <summary>
        /// The default status when the transaction is not finished or succeeds
        /// </summary>
        None,

        /// <summary>
        /// The authorization is expired
        /// </summary>
        AuthorizationExpired,

        /// <summary>
        /// Declined by processor for no clear explanation
        /// </summary>
        ProcessorDeclined,

        /// <summary>
        /// The payment instrument has expired
        /// </summary>
        ExpiredPaymentInstrument,

        /// <summary>
        /// Processor decided that the transaction could be fraudulent and rejected it
        /// </summary>
        ProcessorRiskcheckDeclined,

        /// <summary>
        /// The requested amount exceeded the allowed threshold set by the issuer or the provider
        /// </summary>
        AmountLimitExceeded,

        /// <summary>
        /// An invalid payment instrument account number. Verify the information or use another payment instrument
        /// </summary>
        InvalidPaymentInstrument,

        /// <summary>
        /// The payment instrument lacks enough fund in the account
        /// </summary>
        InsufficientFund,

        /// <summary>
        /// The payment instrument lacks a credible funding source.
        /// </summary>
        MissingFundingSource,

        /// <summary>
        /// Customer fails to authenticate with the PIN or passcode.
        /// </summary>
        IncorrectPinOrPasscode,

        /// <summary>
        /// The CVV value didn't match the one on file.
        /// </summary>
        CvvValueMismatch,

        /// <summary>
        /// The customer's payment instrument cannot be used for this kind of purchase.
        /// </summary>
        TransactionNotAllowed,

        /// <summary>
        /// The customer needs to confirm the payment with a SMS message 
        /// </summary>
        ConfirmWithSms,

        /// <summary>
        /// The customer should verify the payment with the processor, such as, if there are sufficient credits in the payment instrument. 
        /// </summary>
        VerifyWithProcessor,

        /// <summary>
        /// The MOBI payment instrument does not have the required Identity token to complete the purchase request.
        /// </summary>
        MissingBillingToken,

        /// <summary>
        /// The session data is not available for the specified session_id.
        /// </summary>
        MissingSessionData,

        /// <summary>
        /// generic error code for input parameter validation failure, detail message should contain the specific parameter type
        /// </summary>
        InvalidTransactionData,
    }
}