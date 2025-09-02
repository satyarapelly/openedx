// <copyright file="GlobalConstants.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PidlFactory
{
    /// <summary>
    /// Constants container, each set of constants will be grouped into a nested class
    /// </summary>
    public static class GlobalConstants
    {
        // TODO: ServiceContextGroups and ServiceContextKeys are set to public because they are shared by both PidlFactory and PxService
        // Will figure out a better solution to share constants between PidlFactory and PxService
        public static class ServiceContextGroups
        {
            public const string Pifd = "Pifd";
            public const string PxService = "PxService";
        }

        public static class ServiceContextKeys
        {
            public const string Format = "{0}.{1}";
            public const string BaseUrl = "baseUrl";

            public static class Pifd
            {
                public static readonly string BaseUrl = string.Format(Format, ServiceContextGroups.Pifd, "baseUrl");
            }
        }

        public static class EndPointNames
        {
            public const string PaymentMethodDescriptions = "payment-method-descriptions";
            public const string AddressDescriptions = "address-descriptions";
            public const string ChallengeDescriptions = "challenge-descriptions";
            public const string DigitizationDescriptions = "digitization-descriptions";
            public const string ProfileDescriptions = "profile-descriptions";
            public const string TaxIdDescriptions = "taxid-descriptions";
            public const string TenantDescriptions = "tenant-descriptions";
            public const string StaticDescriptions = "staticDescriptions";
        }

        public static class PartnerNames
        {
            public const string AmcXbox = "amcxbox";
            public const string Webblends = "webblends";
            public const string Xbox = "xbox";
        }

        public static class SuggestedAddressesIds
        {
            public const string UserEntered = "entered";
            public const string Suggested = "suggested_";
        }

        public static class DataSourceNames
        {
            public const string PaymentInstruments = "paymentInstruments";
        }

        public static class PaymentMethodFamilyTypeIds
        {
            public const string EwalletLegacyBilldeskPayment = "ewallet.legacy_billdesk_payment";
        }

        internal static class PerformanceCounters
        {
            public const string ServiceName = "Payment Instrument Service";
            public const string GetPaymentInstrumentsTotal = "GetPaymentInstruments Total";
            public const string GetPaymentInstrumentsSuccess = "GetPaymentInstruments Success";
            public const string GetPaymentInstrumentsFailed = "GetPaymentInstruments Failed";
            public const string GetPaymentInstrumentsLatency = "GetPaymentInstruments Latency";
        }

        internal static class FolderNames
        {
            public const string WebAppData = "~/app_data/";
        }

        internal static class Defaults
        {
            public const string Locale = "en-us";
            public const string Language = "en";
            public const string CountryKey = "";
            public const string OperationKey = "add";
            public const string InfoDescriptorIdKey = "";
            public const string ProcessorKey = "";
            public const string ScenarioKey = "";
            public const string CommonKey = "";
            public const string PartnerKey = "";
            public const string FeatureNameKey = "";
            public const string DisplayName = "PLACEHOLDER";
            public const string Logo = "PLACEHOLDER";
        }

        internal static class DescriptionTypes
        {
            public const string PaymentMethodDescription = "PMD";
            public const string AddressDescription = "AD";
            public const string ChallengeDescription = "CD";
            public const string MiscellaneousDescription = "Misc";
        }

        internal static class ErrorCodes
        {
            public const string PIDLConfigFileDoesNotExist = "500000";
            public const string PIDLConfigFileInvalidNumberOfColumns = "500001";
            public const string PIDLConfigFileRequiredColumnIsMissing = "500002";
            public const string PIDLConfigFileColumnIsMalformed = "500003";
            public const string PIDLConfigUnknownPaymentMethodId = "500004";
            public const string PIDLConfigInfoDescriptionIdIsMalformed = "500005";
            public const string PIDLConfigPropertyDescriptionIdIsMalformed = "500006";
            public const string PIDLConfigPropertyInfoDescriptionIsMalformed = "500007";
            public const string PIDLConfigInfoDescriptionForIdIsMissing = "500008";

            public const string PIDLArgumentCountryIsNullOrBlank = "400000";
            public const string PIDLArgumentCountryIsInvalid = "400001";
            public const string PIDLArgumentFamilyIsNullOrBlank = "400002";
            public const string PIDLArgumentFamilyIsInvalid = "400003";
            public const string PIDLArgumentFamilyIsNotSupportedInCountry = "400004";
            public const string PIDLArgumentPaymentMethodIdIsNullOrBlank = "400005";
            public const string PIDLArgumentPaymentMethodIdIsInvalid = "400006";
            public const string PIDLArgumentPaymentMethodIdIsNotSupportedInCountry = "400007";
            public const string PIDLArgumentPaymentMethodDescriptionIdIsNullOrBlank = "400008";
            public const string PIDLArgumentPaymentMethodDescriptionIdInvalid = "400009";
            public const string PIDLArgumentChallengeDescriptionIdIsNullOrBlank = "400010";
            public const string PIDLArgumentChallengeDescriptionIdInvalid = "400011";
        }

        internal static class ExceptionDataKeys
        {
            public const string PIDLErrorCode = "Microsoft.Commerce.Payments.PidlFactory.PidlErrorCodes";
        }

        // PaymentMethodId should be consistent with the id listed in PaymentMethods.csv and kept in lower cases.
        internal static class PaymentMethodId
        {
            public const string CreditCard = "credit_card";
            public const string CreditCardVisa = "credit_card_visa";
            public const string CreditCardMC = "credit_card_mc";
            public const string CreditCardAmex = "credit_card_amex";
            public const string CreditCardDiscover = "credit_card_discover";
            public const string PayPal = "ewallet_paypal";
            public const string LightweightInstrument = "lightweight_instrument";
            public const string AliPay = "lightweight_instrument_alipay";
        }

        internal static class HttpMethods
        {
            public const string Get = "GET";
            public const string Post = "POST";
            public const string Delete = "DELETE";
            public const string Put = "PUT";
            public const string Patch = "PATCH";
        }

        internal static class SubmitActionParams
        {
            public const string Href = "href";
            public const string Method = "method";
            public const string ErrorCodeExpressions = "errorCodeExpressions";
            public const string HeaderType = "headerType";
        }

        internal static class SubmitActionHeaderTypes
        {
            public const string Jarvis = "jarvis";
        }

        internal static class HeaderValues
        {
            public const string JsonContent = "application/json";
            public const string TextContent = "text/plain";
        }

        internal static class QueryParams
        {
            public const string Scenario = "scenario";
        }
    }
}