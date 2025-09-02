// <copyright file="PaymentConstants.cs" company="Microsoft">Copyright (c) Microsoft 2015. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.Common
{
    using System.Diagnostics.CodeAnalysis;

    public static class PaymentConstants
    {
        /// <summary>
        /// the payment method  data constants
        /// </summary>
        public static class PaymentMethodData
        {
            /// <summary>
            /// the payment instrument owner type
            /// </summary>
            public const string PaymentInstrumentOwnerType = "PaymentInstrumentOwnerType";

            /// <summary>
            /// the payment instrument owner id
            /// </summary>
            public const string PaymentInstrumentOwnerId = "PaymentInstrumentOwnerId";
        }

        public static class ProviderRouteData
        {
            public const string ProviderName = "ProviderName";
        }

        /// <summary>
        /// Payment extended http header names for both transaction API and management API
        /// </summary>
        public static class PaymentExtendedHttpHeaders
        {
            /// <summary>
            /// Http header name for client-device id
            /// </summary>
            public const string ClientDeviceId = "x-ms-client-device-id"; 

            /// <summary>
            /// Http header name for tracking guid
            /// </summary>
            public const string TrackingId = "x-ms-tracking-id";

            /// <summary>
            /// Http header name for correlation guid
            /// </summary>
            public const string CorrelationId = "x-ms-correlation-id";

            /// <summary>
            /// Http header name for correlation context
            /// </summary>
            public const string CorrelationContext = "Correlation-Context";

            /// <summary>
            /// API version that is requested
            /// </summary>
            public const string ApiVersion = "api-version";

            /// <summary>
            /// API version that is requested
            /// </summary>
            public const string XMsApiVersion = "x-ms-api-version";

            /// <summary>
            /// Service name that sends the request
            /// </summary>
            public const string CallerName = "x-ms-caller-name";

            /// <summary>
            /// Accept header
            /// </summary>
            public const string Accept = "Accept";

            /// <summary>
            /// Serialized TestContext object (JSON)
            /// </summary>
            public const string TestHeader = "x-ms-test";

            public const string IdempotencyHeaderName = "x-ms-idempotency";

            /// <summary>
            /// In response header to identify the test transaction
            /// </summary>
            public const string TestMarkHeaderName = "x-ms-test-mark";

            /// <summary>
            /// The HTTP request header that indicates what sub-store scenario/client is producing the request
            /// This is pass through from M$ to Billing Core to Payments so that Payments can use it for merchant business data select.
            /// Header value examples: Windows.Xbox, Windows.Desktop, Windows.Mobile, iOS.Phone, iOS.Tablet, Android.Phone, Android.Tablet. 
            /// </summary>
            public const string DeviceType = "x-ms-device";

            /// <summary>
            /// Authorization header.
            /// </summary>
            public const string Authorization = "Authorization";

            /// <summary>
            /// Wallet Service Api version header key.
            /// </summary>
            public const string WalletServiceAPIVersion = "x-api-version";

            /// <summary>
            /// Downstream service flighiting header key.
            /// </summary>
            public const string FlightHeader = "x-ms-flight";
        }

        /// <summary>
        /// Strings of HTTP methods
        /// </summary>
        public static class HttpMethods
        {
            /// <summary>
            /// Http GET
            /// </summary>
            public const string GET = "GET";

            /// <summary>
            /// Http POST
            /// </summary>
            public const string POST = "POST";
        }

        public static class HttpMimeTypes
        {
            public const string FormContentType = "application/x-www-form-urlencoded";
            public const string JsonContentType = "application/json";
            public const string XmlContentType = "application/xml";
            public const string PdfContentType = "application/pdf";
            public const string RawTextType = "text/plain";
            public const string TextUrlType = "text/url-list";
        }

        public static class NamedPorperties 
        {
            public const string TraceActivityId = "TraceActivityId";
            public const string SettlementMode = "SettlementMode";
            public const string AutoReverseRequired = "AutoReverseRequired";
            public const string FingerPrintTimedout = "fingerPrintTimedout";
        }

        public static class ErrorTypes
        {
            public const string FailedOperation = "FailedOperation";
            public const string UnknownFailure = "UnknownFailure";
        }

        public static class HttpHeaders
        {
            /// <summary>
            /// Keep-Alive header
            /// </summary>
            public const string KeepAlive = "Keep-Alive";

            /// <summary>
            /// Connection header
            /// </summary>
            public const string Connection = "Connection";

            /// <summary>
            /// Keep-Alive header parameters
            /// </summary>
            public const string KeepAliveParameter = "timeout={0}";

            /// <summary>
            /// Authorization header
            /// </summary>
            public const string Authorization = "Authorization";

            /// <summary>
            /// From header
            /// </summary>
            public const string From = "From";

            /// <summary>
            /// Origin / Domain name header
            /// </summary>
            public const string Origin = "Origin";

            /// <summary>
            /// Origin / Domain name header
            /// </summary>
            public const string TraceParent = "traceparent";
        }

        /// <summary>
        /// Constants related to hosted services.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Justification = "Web is a good descriptive term in this scenario (constants related to the Web namespace).")]
        public static class Web
        {
            /// <summary>
            /// The property which holds the NameValueCollection of query parameters
            /// from the request URL.
            /// </summary>
            public static readonly string DefaultOutOfServiceFile = @"d:\data\oos.txt";

            public enum DependenciesCertInfo
            {
                HIPCertInfo,
                PIMSCertInfo,
                AccountServiceCertInfo,
                RiskServiceCertInfo,
                StoredValueCertInfo,
                TaxServiceCertInfo,
                LegacyCommerceServiceCertInfo,
            }

            public static class VersionErrorCodes
            {
                public static readonly string MultipleApiVersions = "MultipleApiVersions";

                public static readonly string InvalidApiVersion = "InvalidApiVersion";

                public static readonly string NoApiVersion = "NoApiVersion";
            }

            /// <summary>
            /// Maintains constants related to property keys for the Properties found
            /// on Http*Message.  These are used for passing data between layers in
            /// HTTP pipeline.
            /// </summary>
            public static class Properties
            {
                public const string CorrelationContext = "CorrelationContext";

                /// <summary>
                /// The property which holds the NameValueCollection of query parameters
                /// from the request URL.
                /// </summary>
                public static readonly string QueryParameters = "Payments.QueryParameters";

                /// <summary>
                /// The property which holds the Version of the API to use for handling
                /// this request.
                /// </summary>
                public static readonly string Version = "Payments.Version";

                /// <summary>
                /// The property which holds the server trace identifier for the request.
                /// </summary>
                public static readonly string ServerTraceId = "Payments.ServerTraceId";

                /// <summary>
                /// The property which holds the client trace identifier for the request.
                /// </summary>
                public static readonly string ClientTraceId = "Payments.ClientTraceId";

                /// <summary>
                /// The property which holds the string tracking ID for the request.
                /// </summary>
                public static readonly string TrackingId = "Payments.TrackingId";

                /// <summary>
                /// The property which holds the request content as a string.
                /// </summary>
                public static readonly string Content = "Payments.Content";

                /// <summary>
                /// The property which holds the name of the operation that is being executed.
                /// </summary>
                public static readonly string OperationName = "Payments.OperationName";

                /// <summary>
                /// The property which holds the client caller name. 
                /// </summary>
                public static readonly string CallerName = "Payments.CallerName";

                /// <summary>
                /// The property which holds the client cert thumbprint. 
                /// </summary>
                public static readonly string CallerThumbprint = "Payments.CallerThumbprint";

                /// <summary>
                /// The property which holds the counter name. 
                /// </summary>
                public static readonly string CounterName = "Payments.CounterName";

                public static readonly string FlightingExperimentId = "Payments.FlightingExperimentId";

                public static readonly string ScenarioId = "Payments.ScenarioId";

                /// <summary>
                /// The property which holds the certificate information including subject name, issuer name and thumbprint. 
                /// </summary>
                public static readonly string CertInfo = "Payments.CertInfo";

                /// <summary>
                /// The property which holds the authrizedCert configuation including role, requestUri, allowed paths, allowed accounts. 
                /// </summary>
                public static readonly string CertConfig = "Payments.CertConfig";

                /// <summary>
                /// The property which holds whether certificate has principle. 
                /// </summary>
                public static readonly string CertPrinciple = "Payments.CertPrinciple";

                /// <summary>
                /// The property which holds the authentication error for certificate. 
                /// </summary>
                public static readonly string CertAuthError = "Payments.CertAuthError";

                /// <summary>
                /// The property which holds the authentication information for certificate. 
                /// </summary>
                public static readonly string CertAuthInfo = "Payments.CertAuthInfo";

                /// <summary>
                /// The property which holds the authentication information for certificate. 
                /// </summary>
                public static readonly string TokenAuthWarning = "Payments.TokenAuthWarning";

                /// <summary>
                /// The property which holds the authentication information for certificate. 
                /// </summary>
                public static readonly string TokenAuthError = "Payments.TokenAuthError";

                /// <summary>
                /// The property is true when token auth succeed 
                /// </summary>
                public static readonly string TokenAuthResult = "Payments.TokenAuthSucceed";

                /// <summary>
                /// The property is true when cert auth succeed 
                /// </summary>
                public static readonly string CertAuthResult = "Payments.CertAuthSucceed";

                /// <summary>
                /// The property which holds the partner who calls PIDLSDK. 
                /// </summary>
                public static readonly string Partner = "Payments.Partner";

                /// <summary>
                /// The property which holds the scenario who calls PIDLSDK. 
                /// </summary>
                public static readonly string Scenario = "Payments.Scenario";

                /// <summary>
                /// The property which holds the Pidl Operation parameter when calls PX. 
                /// </summary>
                public static readonly string PidlOperation = "Payments.PidlOperation";

                /// <summary>
                /// The property which holds the avsSuggest flag. 
                /// </summary>
                public static readonly string AvsSuggest = "Payments.AvsSuggest";

                /// <summary>
                /// The property which holds the value of various ServicePointManager data.
                /// </summary>
                public static readonly string ServicePointData = "Payments.ServicePointData";
            }

            public static class InstrumentManagementProperties
            {
                /// <summary>
                /// PIMS
                /// The property which holds the action name of the request. This is used by
                /// TraceCorrelationHandler to create an operation name and this, in turn, is used
                /// by InstrumentManagementTracingHandler to create a counter name.
                /// </summary>
                public const string ActionName = "InstrumentManagement.ActionName";

                public const string Country = "InstrumentManagement.Country";

                public const string PaymentMethodFamily = "InstrumentManagement.PaymentMethodFamily";

                public const string PaymentMethodType = "InstrumentManagement.PaymentMethodType";

                public const string InstrumentId = "InstrumentManagement.Id";

                public const string AccountId = "InstrumentManagement.AccountId";

                public const string ErrorCode = "InstrumentManagement.ErrorCode";

                public const string ErrorMessage = "InstrumentManagement.ErrorMessage";

                public const string PendingOn = "InstrumentManagement.PendingOn";

                public const string SkipRequestLogging = "InstrumentManagement.SkipRequestLogging";

                public const string SkipReponseDetailsLogging = "InstrumentManagement.SkipReponseDetailsLogging";

                public const string OverriddenController = "InstrumentManagement.OverriddenController";

                public const string OverriddenSessionId = "InstrumentManagement.OverriddenSessionId";

                public const string ResponseFromCache = "InstrumentManagement.ResponseFromCache";

                public const string Message = "InstrumentManagement.Message";
            }
        }

        public static class PaymentJournalAdditionalDataNames
        {
            /// <summary>
            /// The Journal additional data key for original payment instrument id
            /// </summary>
            public const string OriginalPaymentInstrumentIdPropertyName = "original_payment_instrument_id";

            /// <summary>
            /// The Journal additional data key for override payment instrument id
            /// </summary>
            public const string OverridePaymentInstrumentIdPropertyName = "override_payment_instrument_id";

            /// <summary>
            /// The Journal additional data key for registered payment method id
            /// </summary>
            public const string RegisteredPaymentMethodIdPropertyName = "registered_payment_method_id";

            /// <summary>
            /// The Journal additional data key for payment method id
            /// </summary>
            public const string PaymentMethodIdPropertyName = "payment_method_id";

            /// <summary>
            /// The Journal additional data key for payment message id
            /// </summary>
            public const string PaymentMessageIdPropertyName = "payment_message_id";

            /// <summary>
            /// The Journal additional data key for payment method type
            /// </summary>
            public const string PaymentMethodTypePropertyName = "payment_method_type";

            /// <summary>
            /// The Journal additional data key for payment method subtype
            /// </summary>
            public const string PaymentMethodSubTypePropertyName = "payment_method_sub_type";

            /// <summary>
            /// Credit card bin number
            /// </summary>
            public const string BinNumber = "bin_number";

            /// <summary>
            /// The Journal additional data key for is 3ds transaction
            /// </summary>
            public const string Is3dsPropertyName = "is_3ds";

            /// <summary>
            /// The Journal additional data key for eci
            /// </summary>
            public const string EciOf3dsPropertyName = "3ds_eci";

            /// <summary>
            /// The Journal additional data key for cavv or ucaf
            /// </summary>
            public const string CavvUcafIn3dsPropertyName = "3ds_cavv_ucaf";

            /// <summary>
            /// The Journal additional data key for modification source
            /// </summary>
            public const string ModificationSourcePropertyName = "modification_source";

            /// <summary>
            /// The Journal additional data key for merchant description
            /// </summary>
            public const string MerchantDescriptionPropertyName = "merchant_description";

            public const string AdditionalPropertiesPropertyName = "additional_properties";

            /// <summary>
            /// The Payment Account token used by pay-by-phone (MobiNonSim).
            /// </summary>
            public const string PaymentAccountPropertyName = "payment_account";

            /// <summary>
            /// The MSISDN associated with this PI (used by MobiNonSim).
            /// </summary>
            public const string MsisdnPropertyName = "msisdn";

            /// <summary>
            /// The Journal additional data key for Non-3DS liability shift status
            /// </summary>
            public const string NonThreeDSecureShiftStatusPropertyName = "NonThreeDSecureShiftStatus";

            public const string TransactionInternalScenario = "transaction_internal_scenario";
        }
    }
}
