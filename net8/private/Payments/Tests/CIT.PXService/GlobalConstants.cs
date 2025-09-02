// <copyright file="GlobalConstants.cs" company="Microsoft">Copyright (c) Microsoft 2019. All rights reserved.</copyright>
namespace CIT.PXService
{
    public static class GlobalConstants
    {
        internal static class RequestPropertyKeys
        {
            public const string ExposedFlightFeatures = "PX.ExposedFlightFeatures";
        }

        internal static class HeaderValues
        {
            public const string ExtendedFlightName = "x-ms-flight";
        }

        internal static class PidlDescriptionTypes
        {
            public const string PaymentMethod = "paymentMethod";
            public const string PaymentInstrument = "PaymentInstrument";
            public const string Profile = "profile";
        }

        internal static class PaymentInstruments
        {
            internal const string India3dsPendingCcGetPI = "{\"id\":\"56ef424a-8ecd-47b0-84d7-b79510b9d404\",\"accountId\":\"abe71d91-7b8b-4f2f-aa67-7c57858e00ee\",\"paymentMethod\":{\"paymentMethodType\":\"visa\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":true,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false},\"paymentMethodFamily\":\"credit_card\",\"display\":{\"name\":\"Visa\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_visa_rect.png\",\"logos\":[{\"mimeType\":\"image/png\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_visa_rect.png\"},{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_visa.svg\"}]}},\"status\":\"Pending\",\"creationDateTime\":\"2019-05-06T06:59:45.2639355Z\",\"lastUpdatedDateTime\":\"2019-05-06T06:59:51.983\",\"details\":{\"pendingOn\":\"3ds\",\"exportable\":false,\"accountHolderName\":\"Kowshik\",\"address\":{\"address_line1\":\"D. No. 76-14-243/3\",\"city\":\"Vijayawada\",\"region\":\"andhra pradesh\",\"postal_code\":\"520012\",\"country\":\"IN\"},\"cardType\":\"credit\",\"lastFourDigits\":\"6840\",\"expiryYear\":\"2021\",\"expiryMonth\":\"2\",\"picvRequired\":false,\"balance\":0.0}}";
            internal const string India3dsActiveCcGetPI = "{\"id\":\"56ef424a-8ecd-47b0-84d7-b79510b9d404\",\"accountId\":\"abe71d91-7b8b-4f2f-aa67-7c57858e00ee\",\"paymentMethod\":{\"paymentMethodType\":\"visa\",\"properties\":{\"offlineRecurring\":true,\"userManaged\":true,\"soldToAddressRequired\":true,\"splitPaymentSupported\":true,\"supportedOperations\":[\"authorize\",\"charge\",\"refund\",\"chargeback\"],\"taxable\":false,\"providerRemittable\":false},\"paymentMethodFamily\":\"credit_card\",\"display\":{\"name\":\"Visa\",\"logo\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_visa_rect.png\",\"logos\":[{\"mimeType\":\"image/png\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_visa_rect.png\"},{\"mimeType\":\"image/svg+xml\",\"url\":\"https://pmservices.cp.microsoft-int.com/staticresourceservice/images/v4/logo_visa.svg\"}]}},\"status\":\"Active\",\"creationDateTime\":\"2019-05-06T06:59:45.2639355Z\",\"lastUpdatedDateTime\":\"2019-05-06T06:59:51.983\",\"details\":{\"pendingOn\":\"3ds\",\"exportable\":false,\"accountHolderName\":\"Kowshik\",\"address\":{\"address_line1\":\"D. No. 76-14-243/3\",\"city\":\"Vijayawada\",\"region\":\"andhra pradesh\",\"postal_code\":\"520012\",\"country\":\"IN\"},\"cardType\":\"credit\",\"lastFourDigits\":\"6840\",\"expiryYear\":\"2021\",\"expiryMonth\":\"2\",\"picvRequired\":false,\"balance\":0.0}}";
        }

        internal static class Partners
        {
            public const string AmcWeb = "amcweb";
            public const string AmcXbox = "amcxbox";
            public const string Azure = "azure";
            public const string Bing = "bing";
            public const string BingTravelAnonymous = "bingTravelAnonymous";
            public const string Cart = "cart";
            public const string CommercialStores = "commercialstores";
            public const string NorthstarWeb = "northstarweb";
            public const string OxoWebDirect = "oxowebdirect";
            public const string Saturn = "saturn";
            public const string SetupOffice = "setupoffice";
            public const string SetupOfficesdx = "setupofficesdx";
            public const string Storify = "storify";
            public const string Webblends = "webblends";
            public const string WebblendsInline = "webblends_inline";
            public const string XBox = "xbox";
            public const string OfficeOobe = "officeOobe";
            public const string OXODIME = "oxodime";
            public const string OXOOobe = "oxooobe";
            public const string Payin = "payin";
            public const string WebPay = "webPay";
            public const string ConsumerSupport = "consumerSupport";
            public const string CommercialSupport = "commercialsupport";
            public const string Xbet = "Xbet";
            public const string XboxWeb = "xboxweb";
            public const string XboxNative = "xboxnative";
            public const string Rushmore = "rushmore";
            public const string Mseg = "mseg";
            public const string NorthStarWeb = "northstarweb";
            public const string XboxSubs = "xboxsubs";
            public const string XboxSettings = "xboxsettings";
            public const string DefaultTemplate = "defaultTemplate";
            public const string TwoPage = "twopage";
            public const string WindowsSettings = "windowssettings";
            public const string OfficeSMB = "officesmb";
            public const string SmboobeModern = "smboobemodern";
            public const string MacManage = "macmanage";
            public const string XPay = "xpay";
        }

        internal static class MediaType
        {
            public const string JsonApplicationType = "application/json";
        }

        internal static class HTTPVerbs
        {
            public const string PATCH = "PATCH";
        }

        internal static class Defaults
        {
            public const string Locale = "en-us";
        }

        internal static class PaymentProviders
        {
            public const string Paypal = "paypal";
            public const string Stripe = "stripe";
        }

        internal static class DisplayHints
        {
            public const string SaveButton = "saveButton";
            public const string CancelBackButton = "cancelBackButton";
            public const string CancelButton = "cancelButton";
            public const string PrivacyStatementHyperLinkDisplayText = "privacyStatement";
        }

        internal static class TemplatePartners
        {
            public const string OnePage = "onepage";
            public const string TwoPage = "twopage";
            public const string DefaultTemplate = "defaulttemplate";
            public const string ListpiDropDown = "listpidropdown";
        }

        internal static class CountryCodes
        {
            public const string USA = "us";
            public const string China = "cn";
            public const string Japan = "jp";
            public const string HongKong = "hk";
            public const string Philippines = "ph";
            public const string Thailand = "th";
            public const string Malaysia = "my";
        }

        internal static class PaymentClientAPI
        {
            public const string Initialize = "/v7.0/PaymentClient/Initialize";
        }
    }
}
